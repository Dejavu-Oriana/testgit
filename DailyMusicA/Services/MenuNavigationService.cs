using System;
using DailyMusicA.Library.Services;
using DailyMusicA.Library.ViewModels;

namespace DailyMusicA.Services;

public class MenuNavigationService : IMenuNavigationService {
    public void NavigateTo(string view, object? parameter = null) {
        ViewModelBase viewModel = view switch {
            MenuNavigationConstant.HomeView => ServiceLocator.Current
                .HomeViewModel,
            MenuNavigationConstant.AlbumView => ServiceLocator.Current
                .AlbumViewModel,
            MenuNavigationConstant.FavoriteView => ServiceLocator.Current
                .FavoriteViewModel,
            MenuNavigationConstant.StatsView => ServiceLocator.Current
                .StatsViewModel,
            _ => throw new Exception("Unknown view")
        };

        ServiceLocator.Current.MainViewModel.SetMenuAndContent(view, viewModel);
    }
}