using PicoWaveAnalyser.Wpf.ViewModels;
using System.IO;
using System.Windows;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;

namespace PicoWaveAnalyser.Wpf;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void Folder_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (paths.Length == 1 && Directory.Exists(paths[0]))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
                return;
            }
        }

        e.Effects = DragDropEffects.None;
        e.Handled = true;
    }

    private void Folder_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            return;

        string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);

        if (paths.Length != 1 || !Directory.Exists(paths[0]))
            return;

        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.FolderPath = paths[0];
        }
    }
}