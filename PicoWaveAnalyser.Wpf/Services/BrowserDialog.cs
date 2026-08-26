using PicoWaveAnalyser.Application.Services.Dialogs;
using Forms = System.Windows.Forms;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace PicoWaveAnalyser.Wpf.Services;

internal sealed class BrowserDialog : IBrowserDialog
{
    public Task<string> BrowseAsync(string description)
    {
        using FolderBrowserDialog dialog = new Forms.FolderBrowserDialog 
        { 
            Description = description,
            SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        string selectedPath = dialog.ShowDialog() == DialogResult.OK ? dialog.SelectedPath : string.Empty;

        return Task.FromResult(selectedPath);
    }

    public Task<string> OpenSaveDialog(string filter, string fileName, string defaultExt) 
    {
        SaveFileDialog dialog = new SaveFileDialog
        {
            Filter = filter,
            FileName = fileName,
            DefaultExt = defaultExt,
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        string selectedPath = dialog.ShowDialog() == true ? dialog.FileName : string.Empty;

        return Task.FromResult(selectedPath);
    }
}
