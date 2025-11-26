using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DailyPoetryA.Library.Services;


namespace DailyPoetryA.Library.ViewModels;

public class MainViewModel : ViewModelBase {
    private readonly IMenuNavigationService _menuNavigationService;

    public MainViewModel(IMenuNavigationService menuNavigationService) {
        _menuNavigationService = menuNavigationService;

        GoBackCommand = new RelayCommand(GoBack);
        OnMenuTappedCommand = new RelayCommand(OnMenuTapped);

        // 程序启动时将主页标记为选择状态
        SelectedMenuItem = MenuItem.HomeView;
    }

    private string _title = "MusicApp";

    public string Title {
        get => _title;
        set => SetProperty(ref _title, value);
    }



    private bool _isPaneOpen = true;

    public bool IsPaneOpen {
        get => _isPaneOpen;
        set => SetProperty(ref _isPaneOpen, value);
    }

    private ViewModelBase? _content;

    public ViewModelBase? Content {
        get => _content;
        private set => SetProperty(ref _content, value);
    }

    public void PushContent(ViewModelBase content) =>
        ContentStack.Add(Content = content);

    public void SetMenuAndContent(string view, ViewModelBase content) {
        ContentStack.Clear();
        PushContent(content);
        SelectedMenuItem = MenuItem.MenuItems.First(p => p.View == view);
        Title = SelectedMenuItem.Name;
    }

    private MenuItem? _selectedMenuItem;

    public MenuItem? SelectedMenuItem {
        get => _selectedMenuItem;
        set => SetProperty(ref _selectedMenuItem, value);
    }

    public ICommand OnMenuTappedCommand { get; }

    public void OnMenuTapped() {
        if (SelectedMenuItem is null) {
            return;
        }

        _menuNavigationService.NavigateTo(SelectedMenuItem.View);
    }

    public ObservableCollection<ViewModelBase> ContentStack { get; } = [];

    public ICommand GoBackCommand { get; }

    public void GoBack() {
        if (ContentStack.Count <= 1) {
            return;
        }

        ContentStack.RemoveAt(ContentStack.Count - 1);
        Content = ContentStack[^1];
    }
}

public class MenuItem {
        public required string View { get; init; }

        public required string Name { get; init; }

        private MenuItem() {
        }

        public static MenuItem HomeView =>
            new() { Name = "主页", View = MenuNavigationConstant.HomeView };

        public static MenuItem AlbumView =>
            new() { Name = "专辑墙", View = MenuNavigationConstant.AlbumView };

        public static MenuItem FavoriteView =>
            new() { Name = "收藏", View = MenuNavigationConstant.FavoriteView };

        public static MenuItem StatsView =>
            new() { Name = "数据概览", View = MenuNavigationConstant.StatsView };

        public static IEnumerable<MenuItem> MenuItems { get; } = [
            HomeView, AlbumView, FavoriteView, StatsView
        ];
}