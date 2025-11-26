using DailyPoetryA.Library.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System.Linq;

namespace DailyPoetryA.Library.ViewModels;

/// <summary>
/// 主页视图模型
/// 负责处理主页的业务逻辑和数据展示
/// </summary>
public class HomeViewModel : ViewModelBase
{
    private readonly IMusicStorage _musicStorage;
    private readonly IPreferenceStorage _preferenceStorage;
    
    private int _albumCount = 0;
    private int _songCount = 0;
    private int _favoriteCount = 0;
    private bool _isLoading = false;

    /// <summary>
    /// 专辑数量
    /// </summary>
    public int AlbumCount
    {
        get => _albumCount;
        private set => SetProperty(ref _albumCount, value);
    }

    /// <summary>
    /// 歌曲数量
    /// </summary>
    public int SongCount
    {
        get => _songCount;
        private set => SetProperty(ref _songCount, value);
    }

    /// <summary>
    /// 收藏数量
    /// </summary>
    public int FavoriteCount
    {
        get => _favoriteCount;
        private set => SetProperty(ref _favoriteCount, value);
    }

    /// <summary>
    /// 是否正在加载
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    /// <summary>
    /// 刷新命令
    /// </summary>
    public ICommand RefreshCommand { get; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="musicStorage">音乐存储服务</param>
    /// <param name="preferenceStorage">偏好设置存储服务</param>
    public HomeViewModel(IMusicStorage musicStorage, IPreferenceStorage preferenceStorage)
    {
        _musicStorage = musicStorage;
        _preferenceStorage = preferenceStorage;
        
        RefreshCommand = new RelayCommand(async () => await LoadStatisticsAsync());
        
        // 初始化时加载统计信息
        _ = LoadStatisticsAsync();
    }

    /// <summary>
    /// 加载统计信息
    /// </summary>
    public async Task LoadStatisticsAsync()
    {
        IsLoading = true;
        try
        {
            if (!_musicStorage.IsInitialized)
            {
                await _musicStorage.InitializeAsync();
            }

            var albums = await _musicStorage.GetAlbumsAsync(null, 0, 10000);
            AlbumCount = albums?.Count ?? 0;

            int totalSongs = 0;
            int favoriteSongs = 0;

            if (albums != null)
            {
                foreach (var album in albums)
                {
                    var songs = await _musicStorage.GetSongsByAlbumIdAsync(album.Id);
                    totalSongs += songs?.Count ?? 0;
                    favoriteSongs += songs?.Count(s => s.IsFavorite) ?? 0;
                }
            }

            SongCount = totalSongs;
            FavoriteCount = favoriteSongs;
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"加载统计信息失败: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}