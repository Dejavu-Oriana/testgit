using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DailyMusicA.Library.ViewModels;

namespace DailyMusicA.Views;

public partial class AlbumView : UserControl
{
    public AlbumView()
    {
        InitializeComponent();
        DataContext = ServiceLocator.Current.AlbumViewModel;
        Loaded += AlbumView_Loaded;
    }

    private void AlbumView_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is AlbumViewModel viewModel &&
            viewModel.LoadAlbumsCommand?.CanExecute(null) == true)
        {
            viewModel.LoadAlbumsCommand.Execute(null);
        }
    }
}