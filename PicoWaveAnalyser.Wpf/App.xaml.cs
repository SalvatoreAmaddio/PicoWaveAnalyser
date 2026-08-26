using Microsoft.Extensions.DependencyInjection;
using PicoWaveAnalyser.Application.Services.Analyses;
using PicoWaveAnalyser.Application.Services.Dialogs;
using PicoWaveAnalyser.Application.Services.IO;
using PicoWaveAnalyser.Wpf.Services;
using PicoWaveAnalyser.Wpf.ViewModels;
using System.Windows;

namespace PicoWaveAnalyser.Wpf;

public partial class App : System.Windows.Application
{
    private readonly ServiceProvider _serviceProvider;

    public App()
    {
        ServiceCollection services = new();

        ConfigureServices(services);

        _serviceProvider = services.BuildServiceProvider();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<ITransformer, FourierTransformer>();
        services.AddTransient<FrequencyAnalyser>();

        services.AddTransient<WaveformReader>();
        services.AddTransient<ResultWriter>();

        services.AddSingleton<IBrowserDialog, BrowserDialog>();
        services.AddSingleton<IDialogMessageService, DialogMessageService>();

        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        MainWindow mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider.Dispose();

        base.OnExit(e);
    }
}