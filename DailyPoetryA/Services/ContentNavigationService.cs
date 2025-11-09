using System;
using DailyPoetryA.Library.Services;
using DailyPoetryA.Library.ViewModels;

namespace DailyPoetryA.Services;

public class ContentNavigationService : IContentNavigationService {
    public void NavigateTo(string view, object? parameter = null) {
        ViewModelBase content = view switch {
            ContentNavigationConstant.TodayDetail => ServiceLocator.Current
                .HomeViewModel,
            "MusicDetail" => ServiceLocator.Current.HomeViewModel,
            ContentNavigationConstant.AddAlbum => ServiceLocator.Current
                .AddAlbumViewModel,
            _ => throw new Exception("Unknown view")
        };

        ServiceLocator.Current.MainViewModel.PushContent(content);
    }
}