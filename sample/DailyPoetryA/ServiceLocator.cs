using System;
using Avalonia;
using Avalonia.Controls;
using DailyPoetryA.Library.Services;
using DailyPoetryA.Library.ViewModels;
using DailyPoetryA.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DailyPoetryA;

public class ServiceLocator {
    private readonly IServiceProvider _serviceProvider;

    private static ServiceLocator? _current;

    public static ServiceLocator Current {
        get {
            if (_current is not null) {
                return _current;
            }

            if (Application.Current!.TryGetResource(nameof(ServiceLocator),
                    out var resource) &&
                resource is ServiceLocator serviceLocator) {
                return _current = serviceLocator;
            }

            throw new Exception("this should not happen");
        }
    }

    public ResultViewModel ResultViewModel =>
        _serviceProvider.GetRequiredService<ResultViewModel>();

    public MainWindowViewModel MainWindowViewModel =>
        _serviceProvider.GetRequiredService<MainWindowViewModel>();

    public InitializationViewModel InitializationViewModel =>
        _serviceProvider.GetRequiredService<InitializationViewModel>();

    public MainViewModel MainViewModel =>
        _serviceProvider.GetRequiredService<MainViewModel>();

    public TodayViewModel TodayViewModel =>
        _serviceProvider.GetRequiredService<TodayViewModel>();

    public TodayDetailViewModel TodayDetailViewModel =>
        _serviceProvider.GetRequiredService<TodayDetailViewModel>();

    public QueryViewModel QueryViewModel =>
        _serviceProvider.GetRequiredService<QueryViewModel>();

    // TODO delete this
    public IRootNavigationService RootNavigationService =>
        _serviceProvider.GetRequiredService<IRootNavigationService>();

    public ServiceLocator() {
        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddSingleton<IPreferenceStorage, FilePreferenceStorage>();
        serviceCollection.AddSingleton<IPoetryStorage, PoetryStorage>();

        serviceCollection
            .AddSingleton<IRootNavigationService, RootNavigationService>();
        serviceCollection
            .AddSingleton<IContentNavigationService,
                ContentNavigationService>();
        serviceCollection
            .AddSingleton<IMenuNavigationService, MenuNavigationService>();

        serviceCollection.AddSingleton<ResultViewModel>();
        serviceCollection.AddSingleton<MainWindowViewModel>();
        serviceCollection.AddSingleton<InitializationViewModel>();
        serviceCollection.AddSingleton<MainViewModel>();
        serviceCollection.AddSingleton<TodayViewModel>();
        serviceCollection.AddSingleton<TodayDetailViewModel>();
        serviceCollection.AddSingleton<QueryViewModel>();

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }
}