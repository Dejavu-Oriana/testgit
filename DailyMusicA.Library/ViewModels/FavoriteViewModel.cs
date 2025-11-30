using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Linq;
using Avalonia.Media.Imaging;
using System;
using System.IO;
using DailyMusicA.Library.Models;
using DailyMusicA.Library.Services;

namespace DailyMusicA.Library.ViewModels;

/// <summary>
/// 收藏视图模型
/// </summary>
public class FavoriteViewModel : ViewModelBase
{
    private readonly IMusicStorage _musicStorage;
    private bool _isLoading = false;
    
    /// <summary>
    /// 收藏的歌曲列表
    /// </summary>
    public ObservableCollection<Song> FavoriteSongs { get; } = new ObservableCollection<Song>();
    
    /// <summary>
    /// 是否正在加载
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }
    
    /// <summary>
    /// 刷新命令
    /// </summary>
    public ICommand RefreshCommand { get; }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="musicStorage">音乐存储服务</param>
    public FavoriteViewModel(IMusicStorage musicStorage)
    {
        _musicStorage = musicStorage;
        
        RefreshCommand = new RelayCommand(async () => await LoadFavoriteSongsAsync());
        
        // 初始化时加载收藏歌曲
        _ = LoadFavoriteSongsAsync();
    }
    
    /// <summary>
    /// 加载封面图片到CoverImage属性
    /// </summary>
    /// <param name="album">要加载封面的专辑</param>
    private void LoadCoverImage(Album album)
    {
        if (album == null || string.IsNullOrEmpty(album.CoverUrl))
        {
            return;
        }
        try
        {
            // 确保封面URL使用正确的file://协议格式
            string coverUrl = album.CoverUrl;
            if (!coverUrl.StartsWith("file://"))
            {
                try
                {
                    // 转换为绝对URI格式
                    coverUrl = new Uri(coverUrl).AbsoluteUri;
                }
                catch
                {
                    // 如果转换失败，保持原有值
                }
            }
            if (Uri.TryCreate(coverUrl, UriKind.Absolute, out var coverUri))
            {
                // 如果是file://协议，直接使用本地路径
                if (coverUri.Scheme == "file")
                {
                    var localPath = coverUri.LocalPath;
                    if (System.IO.File.Exists(localPath))
                    {
                        album.CoverImage = new Bitmap(localPath);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"FavoriteViewModel: 加载专辑封面失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 加载收藏的歌曲
    /// </summary>
    public virtual async Task LoadFavoriteSongsAsync()
    {
        // 如果已经在加载中，直接返回，避免并发调用导致重复添加
        if (IsLoading)
        {
            return;
        }
        
        IsLoading = true;
        try
        {
            // 获取所有专辑
            var albums = await _musicStorage.GetAlbumsAsync();
            
            // 为每个专辑加载封面图片
            foreach (var album in albums)
            {
                LoadCoverImage(album);
            }
            
            // 清空当前列表
            FavoriteSongs.Clear();
            
            // 筛选出所有收藏的歌曲
            foreach (var album in albums)
            {
                var songs = await _musicStorage.GetSongsByAlbumIdAsync(album.Id);
                foreach (var song in songs.Where(s => s.IsFavorite))
                {
                    // 设置歌曲的专辑信息
                    song.Album = album;
                    FavoriteSongs.Add(song);
                }
            }
        }
        catch (System.Exception ex)
        {
            // 处理错误，可以添加错误通知
            System.Console.WriteLine($"加载收藏歌曲失败: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}