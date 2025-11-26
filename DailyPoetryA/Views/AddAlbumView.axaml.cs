using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DailyPoetryA.Library.ViewModels;

namespace DailyPoetryA.Views;

public partial class AddAlbumView : UserControl
{
    public AddAlbumView()
    {
        InitializeComponent();
        DataContext = ServiceLocator.Current.AddAlbumViewModel;
    }
}