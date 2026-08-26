using PicoWaveAnalyser.Wpf.ViewModels;
using System.Windows;

namespace PicoWaveAnalyser.Wpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }
}