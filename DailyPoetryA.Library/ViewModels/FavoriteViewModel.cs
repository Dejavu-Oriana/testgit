using DailyPoetryA.Library.Services;
using DailyPoetryA.Library.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Linq;

namespace DailyPoetryA.Library.ViewModels;

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
    /// 加载收藏的歌曲
    /// </summary>
    public async Task LoadFavoriteSongsAsync()
    {
        IsLoading = true;
        try
        {
            // 获取所有专辑
            var albums = await _musicStorage.GetAlbumsAsync();
            
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