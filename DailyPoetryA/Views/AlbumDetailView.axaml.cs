using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DailyPoetryA.Library.ViewModels;
using DailyPoetryA.Library.Models;

namespace DailyPoetryA.Views;

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
