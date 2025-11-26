using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DailyPoetryA.Library.Models;
using DailyPoetryA.Library.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DailyPoetryA.Library.ViewModels;

/// <summary>
/// 专辑墙视图模型
/// </summary>
public class AlbumViewModel : ViewModelBase
{
    private readonly IMusicStorage _musicStorage;
    private readonly IContentNavigationService _contentNavigationService;
    private bool _isLoading = false;
    private bool _suppressFilterUpdates = false;

    private string _filterText = string.Empty;
    private string _selectedSortOption;
    private string _selectedQuickFilter;
    private bool _isGridView = true;
    private bool _isListView = false;
    private string _filterInfo = "正在加载专辑...";
    private bool _isEmpty = false;
    private int _totalAlbums;
    private int _totalSongs;
    private int _favoriteSongs;

    private readonly string[] _sortOptions = [
        "添加时间 ↓",
        "添加时间 ↑",
        "名称 A→Z",
        "名称 Z→A",
        "歌曲数量 ↓",
        "歌曲数量 ↑"
    ];

    private readonly string[] _quickFilters = [
        "全部",
        "最近新增",
        "收藏精选",
        "歌曲量≥10"
    ];
    
    /// <summary>
    /// 专辑集合
    /// </summary>
    public ObservableCollection<Album> Albums { get; } = new ObservableCollection<Album>();

    /// <summary>
    /// 经过筛选的专辑集合
    /// </summary>
    public ObservableCollection<Album> FilteredAlbums { get; } = new ObservableCollection<Album>();
    
    /// <summary>
    /// 是否正在加载
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }
    
    /// <summary>
    /// 数据库连接状态
    /// </summary>
    private bool _isDatabaseConnected = false;
    
    /// <summary>
    /// 数据库连接状态
    /// </summary>
    public bool IsDatabaseConnected
    {
        get => _isDatabaseConnected;
        private set => SetProperty(ref _isDatabaseConnected, value);
    }
    
    /// <summary>
    /// 错误信息
    /// </summary>
    private string _errorMessage = string.Empty;
    
    /// <summary>
    /// 错误信息
    /// </summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }
    
    /// <summary>
    /// 添加专辑命令
    /// </summary>
    public ICommand AddAlbumCommand { get; }
    
    /// <summary>
    /// 查看专辑详情命令
    /// </summary>
    public ICommand ViewAlbumCommand { get; }
    
    /// <summary>
    /// 加载专辑命令
    /// </summary>
    public ICommand LoadAlbumsCommand { get; }
    
    /// <summary>
    /// 刷新专辑命令
    /// </summary>
    public ICommand RefreshAlbumsCommand { get; }

    /// <summary>
    /// 清空筛选命令
    /// </summary>
    public ICommand ClearFilterCommand { get; }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="musicStorage">音乐存储服务</param>
    /// <param name="contentNavigationService">内容导航服务</param>
    public AlbumViewModel(IMusicStorage musicStorage, IContentNavigationService contentNavigationService)
    {
        _musicStorage = musicStorage ?? throw new ArgumentNullException(nameof(musicStorage), "音乐存储服务不能为空");
        _contentNavigationService = contentNavigationService ?? throw new ArgumentNullException(nameof(contentNavigationService), "内容导航服务不能为空");
        
        // 初始化命令
        LoadAlbumsCommand = new RelayCommand(async () => await LoadAlbumsAsync());
        RefreshAlbumsCommand = new RelayCommand(async () => await LoadAlbumsAsync());
        AddAlbumCommand = new RelayCommand(NavigateToAddAlbum);
        ViewAlbumCommand = new RelayCommand<Album?>(ViewAlbum);
        ClearFilterCommand = new RelayCommand(ClearFilters);

        _selectedSortOption = _sortOptions[0];
        _selectedQuickFilter = _quickFilters[0];
        _isListView = false;
        
        // 构造函数中不再自动加载专辑数据，由视图加载时触发LoadAlbumsCommand
    }
    
    /// <summary>
    /// 导航到添加专辑页面
    /// </summary>
    private void NavigateToAddAlbum()
    {
        _contentNavigationService.NavigateTo(ContentNavigationConstant.AddAlbum);
    }
    
    /// <summary>
    /// 搜索关键字
    /// </summary>
    public string FilterText
    {
        get => _filterText;
        set
        {
            if (SetProperty(ref _filterText, value) && !_suppressFilterUpdates)
            {
                ApplyFilters();
            }
        }
    }

    /// <summary>
    /// 可选排序选项
    /// </summary>
    public IReadOnlyList<string> SortOptions => _sortOptions;

    /// <summary>
    /// 当前选中的排序方式
    /// </summary>
    public string SelectedSortOption
    {
        get => _selectedSortOption;
        set
        {
            if (SetProperty(ref _selectedSortOption, value) && !_suppressFilterUpdates)
            {
                ApplyFilters();
            }
        }
    }

    /// <summary>
    /// 快速过滤选项
    /// </summary>
    public IReadOnlyList<string> QuickFilters => _quickFilters;

    /// <summary>
    /// 当前选中的快速过滤方式
    /// </summary>
    public string SelectedQuickFilter
    {
        get => _selectedQuickFilter;
        set
        {
            if (SetProperty(ref _selectedQuickFilter, value) && !_suppressFilterUpdates)
            {
                ApplyFilters();
            }
        }
    }

    /// <summary>
    /// 是否为网格视图
    /// </summary>
    public bool IsGridView
    {
        get => _isGridView;
        set
        {
            if (SetProperty(ref _isGridView, value))
            {
                if (value)
                {
                    _isListView = false;
                    OnPropertyChanged(nameof(IsListView));
                }
                else if (!_isListView)
                {
                    _isListView = true;
                    OnPropertyChanged(nameof(IsListView));
                }
            }
        }
    }

    /// <summary>
    /// 是否为列表视图
    /// </summary>
    public bool IsListView
    {
        get => _isListView;
        set
        {
            if (SetProperty(ref _isListView, value))
            {
                if (value)
                {
                    _isGridView = false;
                    OnPropertyChanged(nameof(IsGridView));
                }
                else if (!_isGridView)
                {
                    _isGridView = true;
                    OnPropertyChanged(nameof(IsGridView));
                }
            }
        }
    }

    /// <summary>
    /// 筛选提示文本
    /// </summary>
    public string FilterInfo
    {
        get => _filterInfo;
        private set => SetProperty(ref _filterInfo, value);
    }

    /// <summary>
    /// 是否为空状态
    /// </summary>
    public bool IsEmpty
    {
        get => _isEmpty;
        private set => SetProperty(ref _isEmpty, value);
    }

    public int TotalAlbums
    {
        get => _totalAlbums;
        private set => SetProperty(ref _totalAlbums, value);
    }

    public int TotalSongs
    {
        get => _totalSongs;
        private set => SetProperty(ref _totalSongs, value);
    }

    public int FavoriteSongs
    {
        get => _favoriteSongs;
        private set => SetProperty(ref _favoriteSongs, value);
    }

    /// <summary>
    /// 查看专辑详情
    /// </summary>
    /// <param name="album">要查看的专辑</param>
    private void ViewAlbum(Album? album)
    {
        if (album != null)
        {
            _contentNavigationService.NavigateTo(ContentNavigationConstant.AlbumDetail, album);
        }
    }
    
    /// <summary>
    /// 加载专辑数据
    /// </summary>
    private async Task LoadAlbumsAsync()
    {
        IsLoading = true;
        IsDatabaseConnected = false;
        ErrorMessage = string.Empty;
        
        try
        {            // 初始化数据库连接
            System.Console.WriteLine("AlbumViewModel: 开始加载专辑数据");
            if (!_musicStorage.IsInitialized)
            {
                System.Console.WriteLine("AlbumViewModel: 数据库未初始化，正在初始化...");
                await _musicStorage.InitializeAsync();
            }
            
            IsDatabaseConnected = _musicStorage.IsInitialized;
            System.Console.WriteLine($"AlbumViewModel: 数据库初始化状态: {_musicStorage.IsInitialized}");
            
            // 获取所有专辑
            System.Console.WriteLine("AlbumViewModel: 开始获取专辑列表...");
            var albums = await _musicStorage.GetAlbumsAsync(null, 0, 1000) ?? new List<Album>();
            System.Console.WriteLine($"AlbumViewModel: 获取到 {albums.Count} 个专辑");
            
            // 准备专辑数据
            List<Album> albumsToAdd = new List<Album>();
            
            foreach (var album in albums)
            {
                // 确保专辑信息完整
                if (string.IsNullOrEmpty(album.Name))
                {
                    album.Name = "未知专辑";
                }
                if (string.IsNullOrEmpty(album.Artist))
                {
                    album.Artist = "未知艺术家";
                }
                
                System.Console.WriteLine($"AlbumViewModel: 处理专辑 - ID: {album.Id}, 名称: {album.Name}, 艺术家: {album.Artist}");
                
                // 加载专辑关联的歌曲（简化版本，不使用超时）
                try
                {                    album.Songs = new ObservableCollection<Song>(await _musicStorage.GetSongsByAlbumIdAsync(album.Id) ?? Array.Empty<Song>());
                    System.Console.WriteLine($"AlbumViewModel: 专辑 {album.Name} 加载了 {album.Songs.Count} 首歌曲");
                }                catch (Exception ex)
                {                    album.Songs = new ObservableCollection<Song>();
                    System.Console.WriteLine($"AlbumViewModel: 加载专辑 {album.Name} 的歌曲失败: {ex.Message}");
                }
                
                albumsToAdd.Add(album);
            }
            
            // 更新UI集合
            System.Console.WriteLine($"AlbumViewModel: 准备添加 {albumsToAdd.Count} 个专辑到UI集合");
            Albums.Clear();
            foreach (var album in albumsToAdd)
            {
                Albums.Add(album);
            }
            System.Console.WriteLine($"AlbumViewModel: 专辑加载完成，UI集合中有 {Albums.Count} 个专辑");

            UpdateStatistics();
            ApplyFilters();
        }
        catch (Exception ex)
        {            IsDatabaseConnected = false;
            ErrorMessage = $"加载专辑失败: {ex.Message}";
            System.Console.WriteLine($"AlbumViewModel: 加载专辑数据出错: {ex.Message}");
            System.Console.WriteLine($"AlbumViewModel: 错误堆栈: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Console.WriteLine($"AlbumViewModel: 内部错误: {ex.InnerException.Message}");
            }
        }
        finally
        {            IsLoading = false;
            System.Console.WriteLine("AlbumViewModel: 加载专辑完成，IsLoading设为false");
        }
    }

    private void UpdateStatistics()
    {
        TotalAlbums = Albums.Count;
        TotalSongs = Albums.Sum(a => a.Songs?.Count ?? 0);
        FavoriteSongs = Albums.Sum(a => a.Songs?.Count(s => s.IsFavorite) ?? 0);
    }

    private void ApplyFilters()
    {
        if (IsLoading)
        {
            return;
        }

        IEnumerable<Album> query = Albums;

        if (!string.IsNullOrWhiteSpace(FilterText))
        {
            var text = FilterText.Trim();
            query = query.Where(a =>
                (!string.IsNullOrEmpty(a.Name) && a.Name.Contains(text, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(a.Artist) && a.Artist.Contains(text, StringComparison.OrdinalIgnoreCase)));
        }

        query = _selectedQuickFilter switch
        {
            "最近新增" => query.Where(a => (DateTime.Now - a.AddedDate).TotalDays <= 7),
            "收藏精选" => query.Where(a => a.Songs?.Any(s => s.IsFavorite) ?? false),
            "歌曲量≥10" => query.Where(a => (a.Songs?.Count ?? 0) >= 10),
            _ => query
        };

        query = _selectedSortOption switch
        {
            "添加时间 ↑" => query.OrderBy(a => a.AddedDate),
            "名称 A→Z" => query.OrderBy(a => a.Name),
            "名称 Z→A" => query.OrderByDescending(a => a.Name),
            "歌曲数量 ↓" => query.OrderByDescending(a => a.Songs?.Count ?? 0),
            "歌曲数量 ↑" => query.OrderBy(a => a.Songs?.Count ?? 0),
            _ => query.OrderByDescending(a => a.AddedDate)
        };

        var filtered = query.ToList();

        FilteredAlbums.Clear();
        foreach (var album in filtered)
        {
            FilteredAlbums.Add(album);
        }

        FilterInfo = $"显示 {filtered.Count} / {Albums.Count} 张专辑";
        IsEmpty = filtered.Count == 0;
    }

    private void ClearFilters()
    {
        _suppressFilterUpdates = true;
        FilterText = string.Empty;
        SelectedQuickFilter = _quickFilters[0];
        SelectedSortOption = _sortOptions[0];
        _suppressFilterUpdates = false;
        ApplyFilters();
    }
}