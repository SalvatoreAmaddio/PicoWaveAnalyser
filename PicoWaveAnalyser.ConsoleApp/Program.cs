using PicoWaveAnalyser.Application.Services.Analyses;
using PicoWaveAnalyser.Application.Services.IO;
using PicoWaveAnalyser.Domain;
using System.Collections.Concurrent;

namespace PicoWaveAnalyser.ConsoleApp;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Pico Waveform Analyser");
        Console.WriteLine();

        Console.Write("Enter the waveform folder or drag it here: ");

        string? folderPath = Console.ReadLine()?.Trim().Trim('"');

        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
        {
            Console.WriteLine("The selected folder does not exist.");
            return;
        }

        string[] files = Directory.GetFiles(folderPath, "*.csv", SearchOption.TopDirectoryOnly);

        if (files.Length == 0)
        {
            Console.WriteLine("No CSV files found.");
            return;
        }

        WaveformReader reader = new();
        FrequencyAnalyser analyser = new(new FourierTransformer());

        ConcurrentBag<FrequencyResult> results = [];

        using CancellationTokenSource analysisCts = new();
        using CancellationTokenSource listenerCts = new();

        ParallelOptions options = new()
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            CancellationToken = analysisCts.Token
        };

        Task cancelListener = Task.Run(async () =>
        {
            while (!listenerCts.IsCancellationRequested)
            {
                if (Console.KeyAvailable && Console.ReadKey(intercept: true).Key == ConsoleKey.Escape)
                {
                    await analysisCts.CancelAsync();
                    return;
                }

                await Task.Delay(100, listenerCts.Token);
            }
        });

        try
        {
            object progressLock = new();

            int completed = 0;

            await Parallel.ForEachAsync(files, options, async (file, cancellationToken) =>
            {
                Waveform waveform = await reader.ReadAsync(file, cancellationToken);

                double frequency = analyser.FindDominantFrequency(waveform);

                results.Add(new FrequencyResult(Path.GetFileName(file), frequency, file));

                lock (progressLock)
                {
                    completed++;

                    double percentage = completed * 100.0 / files.Length;

                    Console.Write($"\rAnalysing waveforms... " +
                                  $"{completed}/{files.Length} " +
                                  $"({percentage:F0}%) - Press ESC to cancel");
                }
            });

            Console.WriteLine();
            Console.WriteLine("Exporting...");

            List<FrequencyResult> sortedResults = results.OrderBy(x => x.FrequencyHz).ToList();

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            string outputPath = Path.Combine(desktopPath, "waveform-frequencies.csv");

            ResultWriter writer = new();

            await writer.WriteAsync(outputPath, sortedResults);

            Console.WriteLine();
            Console.WriteLine("Results exported to:");
            Console.WriteLine(outputPath);
        }
        catch (OperationCanceledException) when (analysisCts.IsCancellationRequested)
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Analysis cancelled.");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine($"Analysis failed: {ex.Message}");
        }
        finally
        {
            await listenerCts.CancelAsync();

            try
            {
                await cancelListener;
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}