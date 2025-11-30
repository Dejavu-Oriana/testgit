using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DailyPoetryA.Library.Services;

namespace DailyPoetryA.Library.ViewModels;

public class TodayViewModel : ViewModelBase {
    private IContentNavigationService _contentNavigationService;

    public TodayViewModel(IContentNavigationService contentNavigationService) {
        _contentNavigationService = contentNavigationService;

        ShowDetailCommand = new RelayCommand(ShowDetail);
    }

    public ICommand ShowDetailCommand { get; }

    public void ShowDetail() {
        _contentNavigationService.NavigateTo(ContentNavigationConstant
            .TodayDetail);
    }
}