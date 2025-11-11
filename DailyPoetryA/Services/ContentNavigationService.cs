using System;
using DailyPoetryA.Library.Services;
using DailyPoetryA.Library.ViewModels;
using DailyPoetryA.Library.Models;

namespace DailyPoetryA.Services;

public class ContentNavigationService : IContentNavigationService {
    public void NavigateTo(string view, object? parameter = null) {
        ViewModelBase content = view switch {
            ContentNavigationConstant.TodayDetail => ServiceLocator.Current
                .HomeViewModel,
            "MusicDetail" => ServiceLocator.Current.HomeViewModel,
            ContentNavigationConstant.AddAlbum => ServiceLocator.Current
                .AddAlbumViewModel,
            ContentNavigationConstant.AlbumDetail => ServiceLocator.Current
                .AlbumDetailViewModel,
            _ => throw new Exception("Unknown view")
        };

        // 导航到专辑详情页面时不设置专辑数据，保持空白页面

        ServiceLocator.Current.MainViewModel.PushContent(content);
    }
}