using System;
using DailyPoetryA.Library.Services;
using DailyPoetryA.Library.ViewModels;

namespace DailyPoetryA.Services;

public class MenuNavigationService : IMenuNavigationService {
    public void NavigateTo(string view, object? parameter = null) {
        ViewModelBase viewModel = view switch {
            MenuNavigationConstant.HomeView => ServiceLocator.Current
                .HomeViewModel,
            MenuNavigationConstant.AlbumView => ServiceLocator.Current
                .AlbumViewModel,
            MenuNavigationConstant.FavoriteView => ServiceLocator.Current
                .FavoriteViewModel,
            _ => throw new Exception("Unknown view")
        };

        ServiceLocator.Current.MainViewModel.SetMenuAndContent(view, viewModel);
    }
}