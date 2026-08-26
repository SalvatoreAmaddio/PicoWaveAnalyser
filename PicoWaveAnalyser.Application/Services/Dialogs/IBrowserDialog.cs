namespace PicoWaveAnalyser.Application.Services.Dialogs;

public interface IBrowserDialog
{
    Task<string> FolderBrowseAsync(string description);
    Task<string> OpenSaveDialog(string filter, string fileName, string defaultExt);
}