using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DailyMusicA.Library.ViewModels;

namespace DailyMusicA.Views;

public partial class AddAlbumView : UserControl
{
    public AddAlbumView()
    {
        InitializeComponent();
        DataContext = ServiceLocator.Current.AddAlbumViewModel;
    }
}