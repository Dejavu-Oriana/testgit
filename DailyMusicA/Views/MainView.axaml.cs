using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using DailyMusicA.Library.ViewModels;

namespace DailyMusicA.Views;

public partial class MainView : UserControl {
    public MainView() {
        InitializeComponent();
        // 设置数据上下文
        DataContext = ServiceLocator.Current.MainViewModel;
    }

    private void OnMenuItemTapped(object? sender, TappedEventArgs e) {
        if (DataContext is MainViewModel viewModel) {
            viewModel.OnMenuTapped();
        }
    }
}