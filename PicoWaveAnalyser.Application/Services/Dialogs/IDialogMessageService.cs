namespace PicoWaveAnalyser.Application.Services.Dialogs;

public interface IDialogMessageService
{
    Task DisplayErrorAsync(string text, string title = "Error");
    Task DisplaySuccessAsync(string text, string title);
}