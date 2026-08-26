using PicoWaveAnalyser.Application.Services.Dialogs;
using Forms = System.Windows.Forms;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace PicoWaveAnalyser.Wpf.Services;

internal sealed class BrowserDialog : IBrowserDialog
{
    public Task<string> BrowseAsync(string description)
    {
        using FolderBrowserDialog dialog = new Forms.FolderBrowserDialog { Description = description };

        if (dialog.ShowDialog() == Forms.DialogResult.OK)
        {
            return Task.FromResult(dialog.SelectedPath);
        }

        return Task.FromResult(string.Empty);
    }

    public Task<string> OpenSaveDialog(string filter, string fileName, string defaultExt) 
    {
        SaveFileDialog dialog = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv",
            FileName = "waveform-frequencies.csv",
            DefaultExt = ".csv"
        };

        if (dialog.ShowDialog() != true) return Task.FromResult(string.Empty);

        return Task.FromResult(dialog.FileName);
    }
}
