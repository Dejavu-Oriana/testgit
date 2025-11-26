using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DailyPoetryA.Library.ViewModels;
using DailyPoetryA.Library.Services;
using static DailyPoetryA.Library.Services.MenuNavigationConstant;

namespace DailyPoetryA.Views;

public partial class HomeView : UserControl {
    private readonly IMenuNavigationService _menuNavigationService;
    
    public HomeView() {
        InitializeComponent();
        DataContext = ServiceLocator.Current.HomeViewModel;
        _menuNavigationService = ServiceLocator.Current.GetService<IMenuNavigationService>();
    }
    
    private void NavigateToAlbumView(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _menuNavigationService.NavigateTo(MenuNavigationConstant.AlbumView);
    }
    
    private void NavigateToFavoriteView(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _menuNavigationService.NavigateTo(MenuNavigationConstant.FavoriteView);
    }
}