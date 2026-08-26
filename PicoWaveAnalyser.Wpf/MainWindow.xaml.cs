using PicoWaveAnalyser.Wpf.ViewModels;
using System.Windows;

namespace PicoWaveAnalyser.Wpf
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}