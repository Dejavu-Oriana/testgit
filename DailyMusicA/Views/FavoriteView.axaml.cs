using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DailyMusicA.Library.ViewModels;

namespace DailyMusicA.Views;

public partial class FavoriteView : UserControl {
    public FavoriteView() {
        InitializeComponent();
        // 设置DataContext为FavoriteViewModel
        DataContext = ServiceLocator.Current.FavoriteViewModel;
    }
}