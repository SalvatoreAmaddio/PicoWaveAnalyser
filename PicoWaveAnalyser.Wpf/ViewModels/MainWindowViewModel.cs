using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PicoWaveAnalyser.Application.Services.Analyses;
using PicoWaveAnalyser.Application.Services.Dialogs;
using PicoWaveAnalyser.Application.Services.IO;
using PicoWaveAnalyser.Domain;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;

namespace PicoWaveAnalyser.Wpf.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    #region Services
    private readonly IBrowserDialog BrowserDialog;
    private readonly IDialogMessageService DialogMessageService;
    private readonly WaveformReader Reader;
    private readonly FrequencyAnalyser Analyser;
    private readonly ResultWriter Writer;
    #endregion

    private CancellationTokenSource? _analysisCts;

    #region Properties
    [ObservableProperty] private bool isAnalysing = false;
    [ObservableProperty] private bool isFolderSet = false;
    [ObservableProperty] private string folderPath = string.Empty;
    [ObservableProperty] private string status = "Choose the folder containing the waveform CSV files.";
    #endregion

    public ObservableCollection<FrequencyResult> Results { get; } = [];

    public MainWindowViewModel(IBrowserDialog browserDialog, IDialogMessageService dialogMessageService, WaveformReader reader, FrequencyAnalyser analyser, ResultWriter writer)
    {
        BrowserDialog = browserDialog;
        DialogMessageService = dialogMessageService;
        Reader = reader;
        Analyser = analyser;
        Writer = writer;
    }

    private bool CanCancelAnalysis() => IsAnalysing;
    private bool CanExportAnalysis() => Results.Count > 0;
    private bool CanAnalysis() => !string.IsNullOrWhiteSpace(FolderPath);

    partial void OnIsAnalysingChanged(bool value)
    {
        CancelAnalysisCommand.NotifyCanExecuteChanged();
        ExportCommand.NotifyCanExecuteChanged();

        if (value)
        {
            Status = "Analysing...";
        }
    }

    partial void OnFolderPathChanged(string value)
    {
        IsFolderSet = !string.IsNullOrEmpty(value);
        AnalyseCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private async Task BrowseAsync()
    {
        FolderPath = await BrowserDialog.BrowseAsync("Select waveform folder");
    }

    [RelayCommand(CanExecute = nameof(CanAnalysis))]
    private async Task AnalyseAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(FolderPath))
            {
                await DialogMessageService.DisplayErrorAsync("Please choose a folder containing csv wave files", "Forgot something?");
                return;
            }

            Results.Clear();

            IsAnalysing = true;

            string[] files = Directory.GetFiles(FolderPath, "*.csv", SearchOption.TopDirectoryOnly);

            if (files.Length == 0)
            {
                throw new Exception("No CSV files found.");
            }

            ConcurrentBag<FrequencyResult> calculated = new();

            _analysisCts = new CancellationTokenSource();

            ParallelOptions options = new()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = _analysisCts.Token
            };

            await Parallel.ForEachAsync(files, options, async (file, cancellationToken) =>
            {
                Waveform waveform = await Reader.ReadAsync(file, cancellationToken);

                double frequency = Analyser.FindDominantFrequency(waveform);

                calculated.Add(new FrequencyResult(Path.GetFileName(file), frequency));
            });

            foreach (FrequencyResult result in calculated.OrderBy(wave => wave.FrequencyHz))
            {
                Results.Add(result);
            }

            Status = $"Completed. {Results.Count} waveform files analysed.";
        }
        catch (OperationCanceledException)
        {
            Status = "Analysis cancelled.";
        }
        catch (Exception ex)
        {
            Status = "Analysis failed.";
            await DialogMessageService.DisplayErrorAsync(ex.Message, "Analysis error");
        }
        finally
        {
            _analysisCts?.Dispose();
            _analysisCts = null;
            IsAnalysing = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancelAnalysis))]
    private void CancelAnalysis()
    {
        _analysisCts?.Cancel();
    }

    [RelayCommand(CanExecute = nameof(CanExportAnalysis))]
    private async Task ExportAsync()
    {
        if (Results.Count == 0)
        {
            await DialogMessageService.DisplayErrorAsync("You did not run any analyses", "Export error");
            return;
        }

        string fileName = await BrowserDialog.OpenSaveDialog("CSV files (*.csv)|*.csv", "waveform-frequencies.csv", ".csv");

        if (string.IsNullOrEmpty(fileName))
            return;

        try
        {
            await Writer.WriteAsync(fileName, Results);
            Status = $"Results exported to {fileName}";
            await DialogMessageService.DisplaySuccessAsync(Status, "Done!");
        }
        catch (Exception ex)
        {
            await DialogMessageService.DisplayErrorAsync(ex.Message, "Export error");
        }
    }
}