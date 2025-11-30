using System;
using DailyPoetryA.Library.Services;
using DailyPoetryA.Library.ViewModels;

namespace DailyPoetryA.Services;

public class MenuNavigationService : IMenuNavigationService {
    public void NavigateTo(string view, object? parameter = null) {
        ViewModelBase viewModel = view switch {
            MenuNavigationConstant.TodayView => ServiceLocator.Current
                .TodayViewModel,
            MenuNavigationConstant.QueryView => ServiceLocator.Current
                .QueryViewModel,
            _ => throw new Exception("Unknown view")
        };

        ServiceLocator.Current.MainViewModel.SetMenuAndContent(view, viewModel);
    }
}