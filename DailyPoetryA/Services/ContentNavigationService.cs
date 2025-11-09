using System;
using DailyPoetryA.Library.Services;

namespace DailyPoetryA.Services;

public class ContentNavigationService : IContentNavigationService {
    public void NavigateTo(string view, object? parameter = null) {
        var content = view switch {
            ContentNavigationConstant.TodayDetail => ServiceLocator.Current
                .HomeViewModel,
            "MusicDetail" => ServiceLocator.Current.HomeViewModel,
            _ => throw new Exception("Unknown view")
        };

        ServiceLocator.Current.MainViewModel.PushContent(content);
    }
}