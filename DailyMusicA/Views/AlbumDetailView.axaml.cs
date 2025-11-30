using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DailyMusicA.Library.ViewModels;
using DailyMusicA.Library.Models;

namespace DailyMusicA.Views;

public partial class AlbumDetailView : UserControl
{
    public AlbumDetailView()
    {
        InitializeComponent();
        DataContext = ServiceLocator.Current.AlbumDetailViewModel;
    }
    
    /// <summary>
    /// 设置专辑数据
    /// </summary>
    /// <param name="album">专辑对象</param>
    public void SetAlbum(Album album)
    {
        if (DataContext is AlbumDetailViewModel viewModel)
        {
            viewModel.UpdateAlbum(album);
        }
    }
}
