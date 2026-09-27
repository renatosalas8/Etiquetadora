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
            // 1. Create MainView first so desktop.MainWindow isn't null
            var mainWindow = new MainView();
            desktop.MainWindow = mainWindow;

            // 2. Configure DI Container
            var services = new ServiceCollection();
            
            // Register FilesService with reference to the active window
            services.AddSingleton<IFilesService>(sp => new FilesService(mainWindow));
            
            // Register ViewModel
            services.AddTransient<MainViewModel>();

            // Build ServiceProvider (Ensure Microsoft.Extensions.DependencyInjection NuGet package is installed)
            Services = services.BuildServiceProvider();

            // 3. Resolve DataContext from DI
            mainWindow.DataContext = Services.GetRequiredService<MainViewModel>();
        }

        base.OnFrameworkInitializationCompleted();
    }
}