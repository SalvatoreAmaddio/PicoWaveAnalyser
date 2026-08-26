using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PicoWaveAnalyser.Application.Services.Analyses;
using PicoWaveAnalyser.Application.Services.Dialogs;
using PicoWaveAnalyser.Application.Services.IO;
using PicoWaveAnalyser.Domain;
using PicoWaveAnalyser.Wpf.Services;
using System.Collections.ObjectModel;
using System.IO;

namespace PicoWaveAnalyser.Wpf.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly BrowserDialog browserDialog = new();
    private readonly IDialogMessageService dialogMessageService = new DialogMessageService();
    private readonly WaveformReader _reader = new();
    private readonly FrequencyAnalyser _analyser = new(new FourierTransformer());
    private readonly ResultWriter _writer = new();

    [ObservableProperty] private string folderPath = string.Empty;
    [ObservableProperty] private string status = "Choose the folder containing the waveform CSV files.";
    [ObservableProperty] private double progress;

    public ObservableCollection<FrequencyResult> Results { get; } = [];

    public MainWindowViewModel()
    {
    }

    [RelayCommand]
    private async Task BrowseAsync()
    {
        FolderPath = await browserDialog.BrowseAsync("Select waveform folder");
    }

    [RelayCommand]
    private async Task AnalyseAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(FolderPath))
            {
                await dialogMessageService.DisplayErrorAsync("Please choose a folder containing csv wave files", "Forgot something?");
                return;
            }

            Results.Clear();

            Progress = 0;

            string[] files = Directory.GetFiles(FolderPath, "*.csv", SearchOption.TopDirectoryOnly);

            if (files.Length == 0)
            {
                Status = "No CSV files found.";
                return;
            }

            List<FrequencyResult> calculated = new List<FrequencyResult>(files.Length);

            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                Status = $"Analysing {Path.GetFileName(file)} ({i + 1}/{files.Length})...";
                Waveform waveform = await _reader.ReadAsync(file);

                // FFT work is CPU-bound; move it off the UI thread so the window stays responsive.
                double frequency = await Task.Run(() => _analyser.FindDominantFrequency(waveform));
                calculated.Add(new FrequencyResult(Path.GetFileName(file), frequency));
                Progress = (i + 1) * 100.0 / files.Length;
            }

            foreach (FrequencyResult? result in calculated.OrderBy(x => x.FrequencyHz))
            {
                Results.Add(result);
            }

            Status = $"Completed. {Results.Count} waveform files analysed.";
        }
        catch (Exception ex)
        {
            Status = "Analysis failed.";
            await dialogMessageService.DisplayErrorAsync(ex.Message, "Analysis error");
        }
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        if (Results.Count == 0)
        {
            await dialogMessageService.DisplayErrorAsync("You did not run any analyses", "Export error");
            return;
        }

        string fileName = await browserDialog.OpenSaveDialog("CSV files (*.csv)|*.csv", "waveform-frequencies.csv", ".csv");

        if (string.IsNullOrEmpty(fileName))
            return;

        try
        {
            await _writer.WriteAsync(fileName, Results);
            Status = $"Results exported to {fileName}";
            await dialogMessageService.DisplaySuccessAsync(Status, "Done!");
        }
        catch (Exception ex)
        {
            await dialogMessageService.DisplayErrorAsync(ex.Message, "Export error");
        }
    }
}