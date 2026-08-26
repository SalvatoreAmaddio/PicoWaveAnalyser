using PicoWaveAnalyser.Application.Services.Dialogs;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace PicoWaveAnalyser.Wpf.Services;

internal sealed class DialogMessageService : IDialogMessageService
{
    public Task DisplayErrorAsync(string text, string title = "error")
    {
        MessageBox.Show(text, title, MessageBoxButton.OK, MessageBoxImage.Error);
        return Task.CompletedTask;
    }

    public Task DisplaySuccessAsync(string text, string title)
    {
        MessageBox.Show(text, title, MessageBoxButton.OK, MessageBoxImage.Information);
        return Task.CompletedTask;
    }
}