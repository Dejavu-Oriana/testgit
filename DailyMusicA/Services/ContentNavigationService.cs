using System;
using DailyMusicA.Library.Services;
using DailyMusicA.Library.ViewModels;
using DailyMusicA.Library.Models;

namespace DailyMusicA.Services;

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

        // 导航到专辑详情页面时，将专辑参数传递给视图模型
        if (view == ContentNavigationConstant.AlbumDetail && parameter is Album album) {
            if (content is AlbumDetailViewModel albumDetailViewModel) {
                albumDetailViewModel.UpdateAlbum(album);
            }
        }

        // 检查当前内容是否已经是目标视图模型，如果是则不再重复添加，避免导航栈中出现重复项
        var mainViewModel = ServiceLocator.Current.MainViewModel;
        if (mainViewModel.Content != content) {
            mainViewModel.PushContent(content);
        }
    }
}