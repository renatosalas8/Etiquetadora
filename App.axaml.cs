using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Etiquetadora.Services;
using Etiquetadora.ViewModels;
using Etiquetadora.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Etiquetadora;

public partial class App : Application
{
    public new static App? Current => Application.Current as App;
    
    public IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainView();
            desktop.MainWindow = mainWindow;

            
            var services = new ServiceCollection();
            services.AddSingleton<IFilesService>(sp => new FilesService(mainWindow));
            services.AddSingleton<IPresetService>(sp => new PresetService(mainWindow));
            services.AddTransient<MainViewModel>();
            Services = services.BuildServiceProvider();
            mainWindow.DataContext = Services.GetRequiredService<MainViewModel>();
        }

        base.OnFrameworkInitializationCompleted();
    }
}