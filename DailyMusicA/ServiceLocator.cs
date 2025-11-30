using System;
using Avalonia;
using Avalonia.Controls;
using DailyMusicA.Services;
using DailyMusicA.Library.Services;
using DailyMusicA.Library.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DailyMusicA;

public class ServiceLocator {
    private readonly IServiceProvider _serviceProvider;

    private static ServiceLocator? _current;

    public static ServiceLocator Current {
        get {
            if (_current is null) {
                _current = new ServiceLocator();
            }
            return _current;
        }
    }


    public MainWindowViewModel MainWindowViewModel =>
        _serviceProvider.GetRequiredService<MainWindowViewModel>();

    public InitializationViewModel InitializationViewModel =>
        _serviceProvider.GetRequiredService<InitializationViewModel>();

    public MainViewModel MainViewModel =>
        _serviceProvider.GetRequiredService<MainViewModel>();
        
    public T GetService<T>() =>
        _serviceProvider.GetRequiredService<T>();

    public HomeViewModel HomeViewModel =>
        _serviceProvider.GetRequiredService<HomeViewModel>();
    
    public AlbumViewModel AlbumViewModel =>
        _serviceProvider.GetRequiredService<AlbumViewModel>();
    
    public FavoriteViewModel FavoriteViewModel =>
        _serviceProvider.GetRequiredService<FavoriteViewModel>();
        
    public AddAlbumViewModel AddAlbumViewModel =>
        _serviceProvider.GetRequiredService<AddAlbumViewModel>();
    
    public AlbumDetailViewModel AlbumDetailViewModel =>
        _serviceProvider.GetRequiredService<AlbumDetailViewModel>();

    public StatsViewModel StatsViewModel =>
        _serviceProvider.GetRequiredService<StatsViewModel>();



    // TODO delete this
    public IRootNavigationService RootNavigationService =>
        _serviceProvider.GetRequiredService<IRootNavigationService>();

    public ServiceLocator() {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IPreferenceStorage,DailyMusicA.Library.Services.PreferenceStorage>();
        serviceCollection.AddSingleton<IMusicStorage, MusicStorage>();
        serviceCollection.AddSingleton<DailyMusicA.Library.Services.IFileDialogService, Services.FileDialogService>();

        serviceCollection
            .AddSingleton<IRootNavigationService, RootNavigationService>();
        serviceCollection
            .AddSingleton<IContentNavigationService,
                ContentNavigationService>();
        serviceCollection
            .AddSingleton<IMenuNavigationService, MenuNavigationService>();

        serviceCollection.AddSingleton<MainWindowViewModel>();
        serviceCollection.AddSingleton<InitializationViewModel>();
        serviceCollection.AddSingleton<MainViewModel>();
        serviceCollection.AddSingleton<HomeViewModel>();
        serviceCollection.AddSingleton<AlbumViewModel>();
        serviceCollection.AddSingleton<FavoriteViewModel>();
        serviceCollection.AddSingleton<AddAlbumViewModel>();
        serviceCollection.AddSingleton<AlbumDetailViewModel>();
        serviceCollection.AddSingleton<StatsViewModel>();

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }
}