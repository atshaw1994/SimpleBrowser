using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using SimpleBrowser.Services;
using SimpleBrowser.Services.Abstractions;
using SimpleBrowser.ViewModels;
using SimpleBrowser.Views;
using System;

namespace SimpleBrowser;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    private MainViewModel? _viewModel;

    public override void OnFrameworkInitializationCompleted()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        Services = serviceCollection.BuildServiceProvider();

        _viewModel = Services.GetRequiredService<MainViewModel>();
        DataContext = _viewModel;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = Services.GetRequiredService<MainWindow>();

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IHistoryRepository, JsonHistoryItemRepository>();
        services.AddTransient<IHistoryService, HistoryService>();
        services.AddSingleton<IBookmarksRepository, JsonBookmarksRepository>();
        services.AddTransient<IBookmarksService, BookmarkService>();
        services.AddSingleton<MainServicesContext>();
        services.AddTransient<MainViewModel>();
        services.AddTransient(provider => new MainWindow(
            provider.GetRequiredService<MainViewModel>()
        ));
    }
}