# 音乐管理应用 - DailyPoetryA

这是一个使用Avalonia UI框架开发的音乐管理应用，采用MVVM架构模式，支持专辑管理、歌曲收藏和数据统计功能。

## 技术栈

- **UI框架**: Avalonia UI
- **MVVM框架**: CommunityToolkit.Mvvm
- **数据库**: SQLite
- **依赖注入**: Microsoft.Extensions.DependencyInjection
- **开发语言**: C#

## 项目结构

```
├── DailyPoetryA.Library/         # 核心库
│   ├── Models/                   # 数据模型
│   ├── Services/                 # 服务层
│   └── ViewModels/               # 视图模型
├── DailyPoetryA/                 # 应用程序
│   ├── Services/                 # 应用服务
│   ├── ViewModels/               # 应用视图模型
│   └── Views/                    # 视图
└── DailyPoetryA.UnitTest/        # 单元测试
```

## View与ViewModel详解

### 1. MainWindow与MainWindowViewModel

**功能**: 应用程序主窗口，负责承载应用内容。

**MainWindow.xaml.cs**:
```csharp
public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();
    }
}
```

**MainWindowViewModel.cs**:
```csharp
namespace DailyPoetryA.Library.ViewModels;

public class MainWindowViewModel : ViewModelBase {
    private ViewModelBase? _content;

    public ViewModelBase? Content {
        get => _content;
        set => SetProperty(ref _content, value);
    }
```

### 2. MainView与MainViewModel

**功能**: 主视图，负责导航菜单和内容区域管理，实现页面导航和返回功能，维护应用标题和侧边栏状态。

**MainView.xaml.cs**:
```csharp
public partial class MainView : UserControl {
    public MainView() {
        InitializeComponent();
        DataContext = ServiceLocator.Current.MainViewModel;
    }
    
    private void OnMenuItemTapped(object? sender, RoutedEventArgs e) {
        ((MainViewModel)DataContext).OnMenuTapped();
    }
}
```

**MainViewModel.cs (详细功能)**:
- 管理导航菜单和内容区域
- 处理页面导航和返回功能
- 实现数据自动刷新机制
- 维护应用标题和侧边栏状态

```csharp
namespace DailyPoetryA.Library.ViewModels;

public class MainViewModel : ViewModelBase {
    private readonly IMenuNavigationService _menuNavigationService;
    private string _title = "MusicApp";
    private bool _isPaneOpen = true;
    private ViewModelBase? _content;
    private MenuItem? _selectedMenuItem;

    public MainViewModel(IMenuNavigationService menuNavigationService) {
        _menuNavigationService = menuNavigationService;
        GoBackCommand = new RelayCommand(GoBack);
        OnMenuTappedCommand = new RelayCommand(OnMenuTapped);
        SelectedMenuItem = MenuItem.HomeView;
    }

    public string Title {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public bool IsPaneOpen {
        get => _isPaneOpen;
        set => SetProperty(ref _isPaneOpen, value);
    }

    public ViewModelBase? Content {
        get => _content;
        private set => SetProperty(ref _content, value);
    }

    public ObservableCollection<ViewModelBase> ContentStack { get; } = [];

    public MenuItem? SelectedMenuItem {
        get => _selectedMenuItem;
        set => SetProperty(ref _selectedMenuItem, value);
    }

    // 关键方法：设置菜单和内容
    public void SetMenuAndContent(string view, ViewModelBase content) {
        ContentStack.Clear();
        PushContent(content);
        SelectedMenuItem = MenuItem.MenuItems.First(p => p.View == view);
        Title = SelectedMenuItem.Name;
        
        // 自动刷新对应数据
        if (content is HomeViewModel homeViewModel) {
            _ = homeViewModel.LoadStatisticsAsync();
        } else if (content is FavoriteViewModel favoriteViewModel) {
            _ = favoriteViewModel.LoadFavoriteSongsAsync();
        } else if (content is StatsViewModel statsViewModel) {
            _ = statsViewModel.LoadStatsAsync();
        } else if (content is AlbumViewModel albumViewModel) {
            _ = albumViewModel.LoadAlbumsAsync();
        }
    }

    public void PushContent(ViewModelBase content) => ContentStack.Add(Content = content);

    // 返回上一页
    public void GoBack() {
        if (ContentStack.Count <= 1) return;
        ContentStack.RemoveAt(ContentStack.Count - 1);
        Content = ContentStack[^1];
        
        // 返回时自动刷新数据
        if (Content is AlbumViewModel albumViewModel) {
            albumViewModel.RefreshAlbumsCommand.Execute(null);
        } else if (Content is HomeViewModel homeViewModel) {
            _ = homeViewModel.LoadStatisticsAsync();
        } else if (Content is FavoriteViewModel favoriteViewModel) {
            _ = favoriteViewModel.LoadFavoriteSongsAsync();
        } else if (Content is StatsViewModel statsViewModel) {
            _ = statsViewModel.LoadStatsAsync();
        }
    }

    public void OnMenuTapped() {
        if (SelectedMenuItem is null) return;
        _menuNavigationService.NavigateTo(SelectedMenuItem.View);
    }

    public ICommand GoBackCommand { get; }
    public ICommand OnMenuTappedCommand { get; }
}
```

### HomeView与HomeViewModel

**功能**: 主页视图，显示应用统计信息和快捷入口。

**HomeView.xaml.cs**:
```csharp
public partial class HomeView : UserControl {
    private readonly IMenuNavigationService _menuNavigationService;
    
    public HomeView() {
        InitializeComponent();
        DataContext = ServiceLocator.Current.HomeViewModel;
        _menuNavigationService = ServiceLocator.Current.GetService<IMenuNavigationService>();
    }
    
    private void NavigateToAlbumView(object? sender, RoutedEventArgs e) {
        _menuNavigationService.NavigateTo(MenuNavigationConstant.AlbumView);
    }
}
```

**HomeViewModel.cs (关键功能)**:
- 加载和显示应用统计信息
- 实现刷新功能

```csharp
public async Task LoadStatisticsAsync() {
    IsLoading = true;
    try {
        if (!_musicStorage.IsInitialized) {
            await _musicStorage.InitializeAsync();
        }

        var albums = await _musicStorage.GetAlbumsAsync(null, 0, 10000);
        AlbumCount = albums?.Count ?? 0;
        // 计算歌曲总数和收藏歌曲数
    }
    finally {
        IsLoading = false;
    }
}
```

### 3. AlbumView与AlbumViewModel

**功能**: 专辑墙视图，显示所有专辑并提供筛选、排序和视图切换功能，支持专辑导航。

**AlbumView.xaml.cs**:
```csharp
public partial class AlbumView : UserControl {
    public AlbumView() {
        InitializeComponent();
        DataContext = ServiceLocator.Current.AlbumViewModel;
        Loaded += AlbumView_Loaded;
    }
    
    private void AlbumView_Loaded(object? sender, RoutedEventArgs e) {
        ((AlbumViewModel)DataContext).LoadAlbumsCommand.Execute(null);
    }
}
```

**AlbumViewModel.cs (详细功能)**:
- 加载和显示专辑列表
- 提供专辑筛选、排序功能
- 支持网格视图和列表视图切换
- 管理专辑墙显示状态和统计信息

```csharp
namespace DailyPoetryA.Library.ViewModels;

public class AlbumViewModel : ViewModelBase {
    private readonly IMusicStorage _musicStorage;
    private readonly IContentNavigationService _contentNavigationService;
    private bool _isLoading = false;
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

    // 排序和筛选选项
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

    // 专辑集合
    public ObservableCollection<Album> Albums { get; } = new ObservableCollection<Album>();
    public ObservableCollection<Album> FilteredAlbums { get; } = new ObservableCollection<Album>();

    // 关键方法：加载专辑数据
    private async Task LoadAlbumsAsync() {
        IsLoading = true;
        try {
            if (!_musicStorage.IsInitialized) {
                await _musicStorage.InitializeAsync();
            }

            var albums = await _musicStorage.GetAlbumsAsync(null, 0, 1000) ?? new List<Album>();
            
            // 处理专辑数据和封面加载
            foreach (var album in albums) {
                // 确保专辑信息完整
                if (string.IsNullOrEmpty(album.Name)) album.Name = "未知专辑";
                if (string.IsNullOrEmpty(album.Artist)) album.Artist = "未知艺术家";
                
                // 加载封面图片
                try {
                    if (!string.IsNullOrEmpty(album.CoverUrl)) {
                        // 确保封面URL使用正确的file://协议格式
                        if (!album.CoverUrl.StartsWith("file://")) {
                            try {
                                album.CoverUrl = new Uri(album.CoverUrl).AbsoluteUri;
                            } catch { /* 转换失败时保持原有值 */ }
                        }
                        
                        // 加载封面图片到CoverImage属性
                        if (Uri.TryCreate(album.CoverUrl, UriKind.Absolute, out var coverUri) && coverUri.Scheme == "file") {
                            var localPath = coverUri.LocalPath;
                            if (System.IO.File.Exists(localPath)) {
                                album.CoverImage = new Bitmap(localPath);
                            }
                        }
                    }
                } catch (Exception ex) {
                    System.Console.WriteLine($"加载专辑封面失败: {ex.Message}");
                }
                
                // 加载专辑歌曲
                try {
                    album.Songs = new ObservableCollection<Song>(await _musicStorage.GetSongsByAlbumIdAsync(album.Id) ?? Array.Empty<Song>());
                } catch (Exception ex) {
                    album.Songs = new ObservableCollection<Song>();
                    System.Console.WriteLine($"加载专辑歌曲失败: {ex.Message}");
                }
            }

            // 更新UI集合
            Albums.Clear();
            foreach (var album in albums) {
                Albums.Add(album);
            }

            UpdateStatistics();
            ApplyFilters();
        } catch (Exception ex) {
            ErrorMessage = $"加载专辑失败: {ex.Message}";
        } finally {
            IsLoading = false;
        }
    }

    // 其他属性和方法...
    public bool IsLoading { get; set; }
    public string FilterText { get; set; }
    public string SelectedSortOption { get; set; }
    public string SelectedQuickFilter { get; set; }
    public bool IsGridView { get; set; }
    public bool IsListView { get; set; }
    public string FilterInfo { get; set; }
    public bool IsEmpty { get; set; }
    public int TotalAlbums { get; set; }
    public int TotalSongs { get; set; }
    public int FavoriteSongs { get; set; }
    public string ErrorMessage { get; set; }

    // 命令
    public ICommand LoadAlbumsCommand { get; }
    public ICommand RefreshAlbumsCommand { get; }
    public ICommand AddAlbumCommand { get; }
    public ICommand ViewAlbumCommand { get; }
    public ICommand ClearFilterCommand { get; }
    
    // 构造函数
    public AlbumViewModel(IMusicStorage musicStorage, IContentNavigationService contentNavigationService) {
        _musicStorage = musicStorage;
        _contentNavigationService = contentNavigationService;
        
        LoadAlbumsCommand = new RelayCommand(async () => await LoadAlbumsAsync());
        RefreshAlbumsCommand = new RelayCommand(async () => await LoadAlbumsAsync());
        AddAlbumCommand = new RelayCommand(NavigateToAddAlbum);
        ViewAlbumCommand = new RelayCommand<Album?>(ViewAlbum);
        ClearFilterCommand = new RelayCommand(ClearFilters);
        
        _selectedSortOption = _sortOptions[0];
        _selectedQuickFilter = _quickFilters[0];
    }
}
```


### 4. AlbumDetailView与AlbumDetailViewModel

**功能**: 专辑详情视图，显示专辑信息和歌曲列表，提供专辑信息编辑、歌曲管理和封面上传功能。

**AlbumDetailView.xaml.cs**:
```csharp
public partial class AlbumDetailView : UserControl {
    public AlbumDetailView() {
        InitializeComponent();
        DataContext = ServiceLocator.Current.AlbumDetailViewModel;
    }
    
    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        ((AlbumDetailViewModel)DataContext).RefreshAlbumViewModel();
    }
    
    private void OnSaveButtonClick(object? sender, RoutedEventArgs e) {
        ((AlbumDetailViewModel)DataContext).SaveAlbumCommand.Execute(null);
    }
    
    private void OnDeleteButtonClick(object? sender, RoutedEventArgs e) {
        ((AlbumDetailViewModel)DataContext).DeleteAlbumCommand.Execute(null);
    }
    
    public void SetAlbum(Album album) {
        if (DataContext is AlbumDetailViewModel viewModel) {
            viewModel.UpdateAlbum(album);
        }
    }
}
```

**AlbumDetailViewModel.cs (详细功能)**:
- 显示专辑详情和歌曲列表
- 提供专辑信息编辑功能
- 处理歌曲播放和收藏操作
- 支持专辑封面上传和更新

```csharp
namespace DailyPoetryA.Library.ViewModels;

public class AlbumDetailViewModel : ViewModelBase {
    private readonly IMusicStorage _musicStorage;
    private readonly IContentNavigationService _contentNavigationService;
    private readonly IFileDialogService _fileDialogService;
    private readonly MainViewModel _mainViewModel;
    private bool _isLoading = false;
    private Album? _selectedAlbum;
    private Bitmap? _albumCoverImage;
    private string _errorMessage = string.Empty;
    private bool _isEditing = false;
    private bool _coverImageChanged = false;
    private int _selectedSongIndex = -1;

    // 关键属性
    public Album? SelectedAlbum {
        get => _selectedAlbum;
        set {
            if (SetProperty(ref _selectedAlbum, value)) {
                if (value != null) {
                    LoadAlbumCover();
                }
            }
        }
    }

    public Bitmap? AlbumCoverImage {
        get => _albumCoverImage;
        set => SetProperty(ref _albumCoverImage, value);
    }

    // 加载专辑封面
    public void LoadAlbumCover() {
        if (SelectedAlbum == null) {
            AlbumCoverImage = null;
            return;
        }

        try {
            if (!string.IsNullOrWhiteSpace(SelectedAlbum.CoverUrl)) {
                // 使用缓存或直接从文件加载
                if (System.IO.File.Exists(SelectedAlbum.CoverUrl)) {
                    AlbumCoverImage = new Bitmap(SelectedAlbum.CoverUrl);
                } else {
                    // 尝试使用URI加载
                    if (Uri.TryCreate(SelectedAlbum.CoverUrl, UriKind.Absolute, out var uri)) {
                        if (uri.Scheme == "file") {
                            var localPath = uri.LocalPath;
                            if (System.IO.File.Exists(localPath)) {
                                AlbumCoverImage = new Bitmap(localPath);
                            }
                        }
                    }
                }
            } else {
                AlbumCoverImage = null;
            }
        } catch (Exception ex) {
            System.Console.WriteLine($"加载专辑封面失败: {ex.Message}");
            AlbumCoverImage = null;
        }
    }

    // 更新专辑数据
    public void UpdateAlbum(Album album) {
        if (album == null) return;
        
        IsLoading = true;
        try {
            // 创建专辑的深拷贝，避免直接修改原始数据
            _selectedAlbum = new Album {
                Id = album.Id,
                Name = album.Name,
                Artist = album.Artist,
                CoverUrl = album.CoverUrl,
                Price = album.Price,
                AddedDate = album.AddedDate,
                Songs = new ObservableCollection<Song>(album.Songs?.Select(s => new Song {
                    Id = s.Id,
                    AlbumId = s.AlbumId,
                    Title = s.Title,
                    Artist = s.Artist,
                    Record = s.Record,
                    IsFavorite = s.IsFavorite
                }) ?? Array.Empty<Song>())
            };
            LoadAlbumCover();
        } finally {
            IsLoading = false;
        }
    }

    // 更新专辑信息
    public async Task<bool> UpdateAlbumAsync() {
        try {
            if (SelectedAlbum == null) return false;

            if (_coverImageChanged && AlbumCoverImage != null) {
                // 保存新的封面图片
                var coverPath = await SaveAlbumCoverAsync(SelectedAlbum.Id, AlbumCoverImage);
                if (!string.IsNullOrEmpty(coverPath)) {
                    SelectedAlbum.CoverUrl = coverPath;
                }
            }

            await _musicStorage.UpdateAlbumAsync(SelectedAlbum);
            return true;
        } catch (Exception ex) {
            ErrorMessage = ex.Message;
            return false;
        }
    }

    // 命令
    public ICommand SaveAlbumCommand { get; }
    public ICommand DeleteAlbumCommand { get; }
    public ICommand UploadCoverCommand { get; }
    public ICommand AddSongCommand { get; }
    public ICommand DeleteSongCommand { get; }
    public ICommand ToggleFavoriteCommand { get; }
    public ICommand PlaySongCommand { get; }

    // 构造函数
    public AlbumDetailViewModel(IMusicStorage musicStorage, IContentNavigationService contentNavigationService, 
                              IFileDialogService fileDialogService, MainViewModel mainViewModel) {
        _musicStorage = musicStorage;
        _contentNavigationService = contentNavigationService;
        _fileDialogService = fileDialogService;
        _mainViewModel = mainViewModel;
        
        // 初始化命令
        SaveAlbumCommand = new RelayCommand(async () => await OnSaveAlbumAsync());
        DeleteAlbumCommand = new RelayCommand(async () => await OnDeleteAlbumAsync());
        UploadCoverCommand = new RelayCommand(async () => await OnUploadCoverAsync());
        AddSongCommand = new RelayCommand(async () => await OnAddSongAsync());
        DeleteSongCommand = new RelayCommand<Song>(async (song) => await OnDeleteSongAsync(song));
        ToggleFavoriteCommand = new RelayCommand<Song>(async (song) => await OnToggleFavoriteAsync(song));
        PlaySongCommand = new RelayCommand<Song>(async (song) => await OnPlaySongAsync(song));
    }

    // 其他方法...
    private async Task OnSaveAlbumAsync() {
        if (await UpdateAlbumAsync()) {
            IsEditing = false;
            RefreshAlbumViewModel();
        }
    }
    
    private async Task OnDeleteAlbumAsync() {
        // 实现删除专辑逻辑
    }
    
    private async Task OnUploadCoverAsync() {
        // 实现上传封面逻辑
    }
    
    private async Task OnAddSongAsync() {
        // 实现添加歌曲逻辑
    }
    
    private async Task OnDeleteSongAsync(Song? song) {
        // 实现删除歌曲逻辑
    }
    
    private async Task OnToggleFavoriteAsync(Song? song) {
        // 实现收藏歌曲逻辑
    }
    
    private async Task OnPlaySongAsync(Song? song) {
        // 实现播放歌曲逻辑
    }
}
```


### 5. AddAlbumView与AddAlbumViewModel

**功能**: 添加新专辑视图，用于创建新专辑、管理歌曲和上传封面。

**AddAlbumView.xaml.cs**:
```csharp
public partial class AddAlbumView : UserControl {
    public AddAlbumView() {
        InitializeComponent();
        DataContext = ServiceLocator.Current.AddAlbumViewModel;
    }
    
    private void OnSaveButtonClick(object? sender, RoutedEventArgs e) {
        ((AddAlbumViewModel)DataContext)._addAlbumCommand.Execute(null);
    }
    
    private void OnCancelButtonClick(object? sender, RoutedEventArgs e) {
        ((AddAlbumViewModel)DataContext)._backCommand.Execute(null);
    }
}
```

**AddAlbumViewModel.cs (详细功能)**:
- 创建新专辑
- 添加和管理专辑歌曲
- 选择和上传专辑封面
- 表单验证和重置

```csharp
namespace DailyPoetryA.Library.ViewModels;

public class AddAlbumViewModel : ViewModelBase {
    private readonly IMusicStorage _musicStorage;
    private readonly IContentNavigationService _navigationService;
    private readonly IFileDialogService _fileDialogService;
    private readonly MainViewModel _mainViewModel;
    private Album _newAlbum;
    private Bitmap? _coverImage;
    private bool _coverImageChanged = false;
    private bool _isLoading = false;
    private string _errorMessage = string.Empty;

    // 命令
    private readonly ICommand _backCommand;
    private readonly ICommand _addAlbumCommand;
    private readonly ICommand _addSongCommand;
    private readonly ICommand _resetCommand;
    private readonly ICommand _previewCommand;
    private readonly ICommand _pickCoverCommand;

    // 构造函数
    public AddAlbumViewModel(
        IContentNavigationService navigationService, 
        MainViewModel mainViewModel, 
        IMusicStorage musicStorage,
        IFileDialogService fileDialogService) {
        _navigationService = navigationService;
        _mainViewModel = mainViewModel;
        _musicStorage = musicStorage;
        _fileDialogService = fileDialogService;
        
        // 初始化命令
        _backCommand = new RelayCommand(Back);
        _addAlbumCommand = new RelayCommand(async () => await AddAlbumAsync(), CanAddAlbum);
        _addSongCommand = new RelayCommand(AddSong);
        _resetCommand = new RelayCommand(Reset);
        _previewCommand = new RelayCommand(Preview);
        _pickCoverCommand = new RelayCommand(async () => await PickCoverAsync());
        
        // 初始化表单
        InitializeForm();
    }

    // 关键方法：创建新专辑
    private async Task AddAlbumAsync() {
        if (!CanAddAlbum) return;
        
        IsLoading = true;
        try {
            // 保存封面图片
            if (_coverImageChanged && CoverImage != null) {
                var coverPath = await SaveCoverImageAsync(NewAlbum.Id, CoverImage);
                NewAlbum.CoverUrl = coverPath;
            }

            // 确保歌曲信息完整
            foreach (var song in NewAlbum.Songs) {
                if (string.IsNullOrWhiteSpace(song.Title)) {
                    ErrorMessage = "歌曲标题不能为空";
                    return;
                }
                if (string.IsNullOrWhiteSpace(song.Artist)) {
                    song.Artist = NewAlbum.Artist;
                }
                if (song.Id == Guid.Empty) {
                    song.Id = Guid.NewGuid();
                }
                song.AlbumId = NewAlbum.Id;
            }

            // 保存专辑到存储
            await _musicStorage.AddAlbumAsync(NewAlbum);
            
            // 返回专辑列表
            Back();
        } catch (Exception ex) {
            ErrorMessage = ex.Message;
        } finally {
            IsLoading = false;
        }
    }

    // 表单验证
    private bool CanAddAlbum => 
        !string.IsNullOrWhiteSpace(NewAlbum.Name) &&
        !string.IsNullOrWhiteSpace(NewAlbum.Artist) &&
        !IsLoading;

    // 初始化表单
    private void InitializeForm() {
        NewAlbum = new Album {
            Id = Guid.NewGuid(),
            Name = string.Empty,
            Artist = string.Empty,
            CoverUrl = string.Empty,
            Description = string.Empty,
            ReleaseDate = DateTime.Now,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Songs = new ObservableCollection<Song>()
        };
    }

    // 其他方法...
    private void Back() {
        _navigationService.NavigateTo(typeof(AlbumViewModel));
    }
    
    private void AddSong() {
        NewAlbum.Songs.Add(new Song {
            Id = Guid.NewGuid(),
            Title = string.Empty,
            Artist = NewAlbum.Artist,
            AlbumId = NewAlbum.Id,
            Duration = 0,
            FilePath = string.Empty
        });
    }
    
    private void Reset() {
        InitializeForm();
        CoverImage = null;
        _coverImageChanged = false;
    }
    
    private void Preview() {
        // 实现预览功能
    }
    
    private async Task PickCoverAsync() {
        // 实现选择封面功能
    }
}
```

### 6. FavoriteView与FavoriteViewModel

**功能**: 收藏视图，显示所有收藏的歌曲，提供歌曲管理和封面显示功能。

**FavoriteView.xaml.cs**:
```csharp
public partial class FavoriteView : UserControl {
    public FavoriteView() {
        InitializeComponent();
        DataContext = ServiceLocator.Current.FavoriteViewModel;
    }
    
    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        ((FavoriteViewModel)DataContext).LoadFavoriteSongsCommand.Execute(null);
    }
}
```

**FavoriteViewModel.cs (详细功能)**:
- 加载和显示收藏的歌曲
- 实现歌曲去重机制
- 管理收藏歌曲的专辑封面

```csharp
namespace DailyPoetryA.Library.ViewModels;

public class FavoriteViewModel : ViewModelBase {
    private readonly IMusicStorage _musicStorage;
    private bool _isLoading = false;
    private string _errorMessage = string.Empty;
    private int _favoriteSongsCount = 0;

    // 收藏歌曲集合
    public ObservableCollection<Song> FavoriteSongs { get; } = new ObservableCollection<Song>();

    // 命令
    public ICommand LoadFavoriteSongsCommand { get; }
    public ICommand RefreshSongsCommand { get; }
    public ICommand ToggleFavoriteCommand { get; }
    public ICommand PlaySongCommand { get; }

    // 构造函数
    public FavoriteViewModel(IMusicStorage musicStorage) {
        _musicStorage = musicStorage;
        
        // 初始化命令
        LoadFavoriteSongsCommand = new RelayCommand(async () => await LoadFavoriteSongsAsync());
        RefreshSongsCommand = new RelayCommand(async () => await LoadFavoriteSongsAsync());
        ToggleFavoriteCommand = new RelayCommand<Song>(async (song) => await ToggleFavoriteAsync(song));
        PlaySongCommand = new RelayCommand<Song>(async (song) => await PlaySongAsync(song));
    }

    // 关键方法：加载收藏歌曲
    public async Task LoadFavoriteSongsAsync() {
        // 如果已经在加载中，直接返回，避免并发调用导致重复添加
        if (IsLoading) {
            return;
        }
        
        IsLoading = true;
        try {
            // 获取所有专辑
            var albums = await _musicStorage.GetAlbumsAsync();
            
            // 为每个专辑加载封面图片
            foreach (var album in albums) {
                LoadCoverImage(album);
            }
            
            // 清空当前列表
            FavoriteSongs.Clear();
            
            // 筛选出所有收藏的歌曲
            foreach (var album in albums) {
                var songs = await _musicStorage.GetSongsByAlbumIdAsync(album.Id);
                foreach (var song in songs.Where(s => s.IsFavorite)) {
                    // 设置歌曲的专辑信息
                    song.Album = album;
                    FavoriteSongs.Add(song);
                }
            }
            
            // 更新统计信息
            FavoriteSongsCount = FavoriteSongs.Count;
        } catch (Exception ex) {
            ErrorMessage = ex.Message;
        } finally {
            IsLoading = false;
        }
    }

    // 其他方法...
    private void LoadCoverImage(Album album) {
        if (!string.IsNullOrWhiteSpace(album.CoverUrl)) {
            try {
                if (File.Exists(album.CoverUrl)) {
                    album.CoverImage = new Bitmap(album.CoverUrl);
                } else if (Uri.TryCreate(album.CoverUrl, UriKind.Absolute, out var uri)) {
                    if (uri.Scheme == "file") {
                        var localPath = uri.LocalPath;
                        if (File.Exists(localPath)) {
                            album.CoverImage = new Bitmap(localPath);
                        }
                    }
                }
            } catch (Exception ex) {
                System.Console.WriteLine($"加载专辑封面失败: {ex.Message}");
            }
        }
    }
    
    private async Task ToggleFavoriteAsync(Song? song) {
        if (song == null) return;
        
        try {
            song.IsFavorite = !song.IsFavorite;
            await _musicStorage.UpdateSongAsync(song);
            
            // 重新加载收藏列表
            await LoadFavoriteSongsAsync();
        } catch (Exception ex) {
            ErrorMessage = ex.Message;
        }
    }
    
    private async Task PlaySongAsync(Song? song) {
        if (song == null) return;
        
        try {
            // 实现播放歌曲逻辑
        } catch (Exception ex) {
            ErrorMessage = ex.Message;
        }
    }
}
```

### 7. InitializationView与InitializationViewModel

**功能**: 应用初始化视图，负责应用启动时的初始化工作和欢迎界面。

**InitializationView.xaml.cs**:
```csharp
public partial class InitializationView : UserControl {
    public InitializationView() {
        InitializeComponent();
        DataContext = ServiceLocator.Current.InitializationViewModel;
    }
}
```

**InitializationViewModel.cs (详细功能)**:
- 处理应用初始化逻辑
- 显示启动欢迎界面
- 初始化音乐存储和其他服务

```csharp
namespace DailyPoetryA.Library.ViewModels;

public class InitializationViewModel : ViewModelBase {
    // 这是一个简单的初始化视图模型，主要用于应用启动时的欢迎界面
    // 它可以扩展为包含更多初始化逻辑，如数据库初始化、设置加载等
    
    public InitializationViewModel() {
        // 初始化逻辑可以在这里添加
        InitializeApp();
    }
    
    private void InitializeApp() {
        // 实现应用初始化逻辑
        // 例如：初始化数据库连接、加载用户设置等
    }
}

### 8. StatsView与StatsViewModel

**功能**: 数据概览视图，显示应用统计信息。

**StatsView.xaml.cs**:
```csharp
public partial class StatsView : UserControl {
    public StatsView() {
        InitializeComponent();
        DataContext = ServiceLocator.Current.StatsViewModel;
    }
}
```

**StatsViewModel.cs (关键功能)**:
- 加载和显示应用统计信息
- 提供数据概览功能

```csharp
public async Task LoadStatsAsync() {
    IsLoading = true;
    try {
        if (!_musicStorage.IsInitialized) {
            await _musicStorage.InitializeAsync();
        }

        var albums = await _musicStorage.GetAlbumsAsync(null, 0, 1000) ?? new List<Album>();

        AlbumCount = albums.Count;
        var allSongs = new List<Song>();
        foreach (var album in albums) {
            var songs = await _musicStorage.GetSongsByAlbumIdAsync(album.Id) ?? new List<Song>();
            album.Songs = new ObservableCollection<Song>(songs);
            allSongs.AddRange(songs.Select(s => {
                s.Album = album;
                return s;
            }));
        }

        SongCount = allSongs.Count;
        FavoriteSongCount = allSongs.Count(s => s.IsFavorite);
        LatestAlbum = albums.OrderByDescending(a => a.AddedDate).FirstOrDefault();
        MostSongsAlbum = albums.OrderByDescending(a => a.Songs?.Count ?? 0).FirstOrDefault();
        // 计算热门艺术家
    }
    finally {
        IsLoading = false;
    }
}
```

## 数据模型

### Album

**功能**: 专辑数据模型

```csharp
[Table("Albums")]
public class Album {
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Column("Name")]
    public string Name { get; set; } = string.Empty;
    
    [Column("Artist")]
    public string Artist { get; set; } = string.Empty;
    
    [Column("CoverUrl")]
    public string CoverUrl { get; set; } = string.Empty;
    
    [Column("Price")]
    public decimal Price { get; set; }
    
    [Column("AddedDate")]
    public DateTime AddedDate { get; set; } = DateTime.Now;
    
    [Ignore]
    public ObservableCollection<Song> Songs { get; set; } = new ObservableCollection<Song>();
    
    [Ignore]
    public Bitmap? CoverImage { get; set; }
}
```

### Song

**功能**: 歌曲数据模型

```csharp
[Table("Songs")]
public class Song {
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Column("Title")]
    public string Title { get; set; } = string.Empty;
    
    [Column("Artist")]
    public string Artist { get; set; } = string.Empty;
    
    [Column("AlbumId")]
    public string AlbumId { get; set; } = string.Empty;
    
    [Column("IsFavorite")]
    public bool IsFavorite { get; set; } = false;
    
    [Column("Record")]
    public string Record { get; set; } = string.Empty;
    
    [Ignore]
    public Album? Album { get; set; }
}
```

## 服务层

### 1. IMusicStorage与MusicStorage

**功能**: 音乐数据存储服务，负责与SQLite数据库交互，提供专辑和歌曲的完整CRUD操作。

**IMusicStorage接口**: 定义了音乐存储服务的核心功能
```csharp
public interface IMusicStorage {
    bool IsInitialized { get; }
    Task InitializeAsync();
    
    // 专辑相关操作
    Task<Album?> GetAlbumAsync(string id);
    Task<IList<Album>> GetAlbumsAsync(Expression<Func<Album, bool>>? where = null, int skip = 0, int take = 20);
    Task AddAlbumAsync(Album album);
    Task UpdateAlbumAsync(Album album);
    Task DeleteAlbumAsync(string id);
    
    // 歌曲相关操作
    Task<IList<Song>> GetSongsByAlbumIdAsync(string albumId);
    Task AddSongsAsync(IEnumerable<Song> songs);
    Task AddSongAsync(Song song);
    Task UpdateSongAsync(Song song);
    Task DeleteSongAsync(string songId);
    Task DeleteSongsByAlbumIdAsync(string albumId);
    
    // 搜索相关操作
    Task<IList<Album>> SearchAlbumsAsync(string keyword);
    
    Task CloseAsync();
}
```

**MusicStorage实现**: 提供了SQLite数据库操作的具体实现
```csharp
public class MusicStorage : IMusicStorage
{
    private IPreferenceStorage _preferenceStorage;
    private SQLiteAsyncConnection? _connection;
    private string _dbPath;
    
    public const string DbName = "musicdb.sqlite3";
    public static readonly string DefaultMusicDbPath = PathHelper.GetLocalFilePath(DbName);
    
    public MusicStorage(IPreferenceStorage preferenceStorage)
    {
        _preferenceStorage = preferenceStorage;
        _dbPath = DefaultMusicDbPath;
    }
    
    // 数据库初始化
    public async Task InitializeAsync()
    {
        // 确保数据库文件存在
        if (!System.IO.File.Exists(_dbPath))
        {
            await using var dbFileStream = new System.IO.FileStream(_dbPath, System.IO.FileMode.Create);
            dbFileStream.Close();
        }
        
        // 创建或更新表结构
        await connection.CreateTableAsync<Album>();
        await connection.CreateTableAsync<Song>();
        
        // 设置版本信息
        _preferenceStorage.Set(MusicStorageConstant.VersionKey, MusicStorageConstant.Version);
    }
    
    // 关键方法示例：获取专辑及其歌曲
    public async Task<Album?> GetAlbumAsync(string id)
    {
        var album = await connection.Table<Album>().FirstOrDefaultAsync(a => a.Id == id);
        if (album != null)
        {
            // 加载歌曲
            var songs = await GetSongsByAlbumIdAsync(id);
            album.Songs = new ObservableCollection<Song>(songs);
        }
        return album;
    }
    
    // 其他实现方法...
}
```

### 2. ContentNavigationService

**功能**: 内容导航服务，负责管理应用程序内部视图之间的导航，处理页面切换和参数传递。

```csharp
public class ContentNavigationService : IContentNavigationService {
    public void NavigateTo(string view, object? parameter = null) {
        ViewModelBase content = view switch {
            ContentNavigationConstant.TodayDetail => ServiceLocator.Current
                .HomeViewModel,
            "MusicDetail" => ServiceLocator.Current.HomeViewModel,
            ContentNavigationConstant.AddAlbum => ServiceLocator.Current
                .AddAlbumViewModel,
            ContentNavigationConstant.AlbumDetail => ServiceLocator.Current
                .AlbumDetailViewModel,
            _ => throw new Exception("Unknown view")
        };

        // 导航到专辑详情页面时，将专辑参数传递给视图模型
        if (view == ContentNavigationConstant.AlbumDetail && parameter is Album album) {
            if (content is AlbumDetailViewModel albumDetailViewModel) {
                albumDetailViewModel.UpdateAlbum(album);
            }
        }

        // 检查当前内容是否已经是目标视图模型，如果是则不再重复添加
        var mainViewModel = ServiceLocator.Current.MainViewModel;
        if (mainViewModel.Content != content) {
            mainViewModel.PushContent(content);
        }
    }
}
```

### 3. FileDialogService

**功能**: 文件对话框服务，封装了Avalonia的文件对话框操作，提供统一的文件选择和保存接口。

```csharp
public class FileDialogService : IFileDialogService
{
    public async Task<string[]?> OpenFileDialogAsync(string title, string filters, bool allowMultiple = false)
    {
        if (GetMainWindow() is not Window window) return null;
        
        var dialog = new OpenFileDialog
        {
            Title = title,
            Filters = ParseFilters(filters),
            AllowMultiple = allowMultiple
        };
        
        return await dialog.ShowAsync(window);
    }

    public async Task<string?> SaveFileDialogAsync(string title, string fileName, string filters)
    {
        if (GetMainWindow() is not Window window) return null;
        
        var dialog = new SaveFileDialog
        {
            Title = title,
            InitialFileName = fileName,
            Filters = ParseFilters(filters)
        };
        
        return await dialog.ShowAsync(window);
    }
    
    // 解析过滤器字符串为Avalonia的FileDialogFilter对象
    private List<FileDialogFilter> ParseFilters(string filters)
    {
        var filterList = new List<FileDialogFilter>();
        string[] filterParts = filters.Split('|');
        
        for (int i = 0; i < filterParts.Length; i += 2)
        {
            // 实现过滤器解析逻辑...
        }
        
        return filterList;
    }
}
```

### 4. MenuNavigationService

**功能**: 菜单导航服务，负责管理应用程序主菜单的导航功能，连接菜单项与对应视图。

```csharp
public class MenuNavigationService : IMenuNavigationService {
    public void NavigateTo(string view, object? parameter = null) {
        ViewModelBase viewModel = view switch {
            MenuNavigationConstant.HomeView => ServiceLocator.Current
                .HomeViewModel,
            MenuNavigationConstant.AlbumView => ServiceLocator.Current
                .AlbumViewModel,
            MenuNavigationConstant.FavoriteView => ServiceLocator.Current
                .FavoriteViewModel,
            MenuNavigationConstant.StatsView => ServiceLocator.Current
                .StatsViewModel,
            _ => throw new Exception("Unknown view")
        };

        ServiceLocator.Current.MainViewModel.SetMenuAndContent(view, viewModel);
    }
}
```

### 5. RootNavigationService

**功能**: 根导航服务，负责应用程序最顶层的导航，管理主窗口内容的切换。

```csharp
public class RootNavigationService : IRootNavigationService {
    public void NavigateTo(string view) {
        ServiceLocator.Current.MainWindowViewModel.Content = view switch {
            RootNavigationConstant.InitializationView => ServiceLocator.Current
                .InitializationViewModel,
            RootNavigationConstant.MainView => ServiceLocator.Current
                .MainViewModel,
            _ => throw new Exception("Unknown view")
        };
    }
}
```

### 服务架构特点

1. **依赖注入设计**：所有服务都实现了对应的接口，便于测试和替换实现
2. **服务定位模式**：通过ServiceLocator获取所需的ViewModel和其他服务
3. **导航层次清晰**：
   - RootNavigationService：管理根级视图切换
   - MenuNavigationService：管理主菜单导航
   - ContentNavigationService：管理内容区域的视图切换
4. **功能单一职责**：每个服务专注于特定的功能领域，职责划分明确

## 关键技术特点

1. **MVVM架构**: 采用CommunityToolkit.Mvvm实现MVVM模式，支持数据绑定和命令绑定。

2. **依赖注入**: 使用Microsoft.Extensions.DependencyInjection管理服务，提高代码的可测试性和可维护性。

3. **数据自动刷新**: 在MainViewModel中实现了数据自动刷新机制，当导航到不同界面时自动加载最新数据。

4. **异步加载**: 所有数据加载操作都采用异步方式，提高应用的响应性。

5. **数据去重**: 在FavoriteViewModel中实现了歌曲去重机制，避免并发调用导致的歌曲重复显示问题。

6. **封面图片加载**: 自动加载专辑封面图片，并处理不同格式的图片URL。

7. **专辑歌曲管理**: 支持添加、删除、排序专辑歌曲，并实时更新数据库。

## 异步操作(Async/Await)详解

### 异步操作的基本概念

Async/Await是C#中用于编写异步代码的关键字，允许开发者编写类似同步代码的异步程序，使异步操作更加直观和易于理解。在本项目中，异步操作主要用于数据加载、数据库操作和文件操作等耗时任务。

### 异步操作在项目中的应用场景

1. **数据加载操作**
   ```csharp
   public async Task LoadStatisticsAsync() {
       IsLoading = true;
       try {
           if (!_musicStorage.IsInitialized) {
               await _musicStorage.InitializeAsync();
           }

           var albums = await _musicStorage.GetAlbumsAsync(null, 0, 10000);
           // 处理数据...
       } finally {
           IsLoading = false;
       }
   }
   ```

2. **数据库操作**
   ```csharp
   public async Task SaveAlbumAsync(Album album) {
       await _dbConnection.RunInTransactionAsync(async (connection) => {
           await connection.UpdateAsync(album);
           // 更新相关数据...
       });
   }
   ```

3. **导航中的异步调用**
   ```csharp
   public void SetMenuAndContent(string view, ViewModelBase content) {
       // ...
       // 自动刷新对应数据
       if (content is HomeViewModel homeViewModel) {
           _ = homeViewModel.LoadStatisticsAsync();
       } else if (content is FavoriteViewModel favoriteViewModel) {
           _ = favoriteViewModel.LoadFavoriteSongsAsync();
       }
       // ...
   }
   ```

### Await/Async的工作原理

1. **方法标记**：使用`async`关键字标记方法，表示该方法包含异步操作。
2. **返回类型**：异步方法通常返回`Task`或`Task<T>`类型。
3. **等待操作**：使用`await`关键字等待异步操作完成，同时不会阻塞当前线程。
4. **状态机**：编译器会将async方法转换为状态机，跟踪异步操作的执行状态。

### 异步操作的好处

1. **提高UI响应性**
   - 异步操作允许UI线程在等待耗时操作完成时继续响应用户输入
   - 避免应用界面出现卡顿或假死现象
   - 提升用户体验

2. **优化资源利用率**
   - 当一个任务在等待IO操作时，线程可以被释放去执行其他任务
   - 减少线程阻塞，降低系统资源消耗
   - 提高应用程序的吞吐量

3. **简化错误处理**
   - 可以使用try/catch块来捕获异步操作中的异常，与同步代码类似
   - 异常会被自动传播到await调用处
   - 便于统一的错误处理策略

4. **支持取消操作**
   - 可以配合`CancellationToken`实现异步操作的取消
   - 允许用户在操作执行过程中取消长时间运行的任务

5. **组合多个异步操作**
   - 使用`Task.WhenAll`和`Task.WhenAny`可以并行执行多个异步操作
   - 提高复杂操作的执行效率

### 异步操作的最佳实践

1. **避免async void**
   - 除了事件处理程序外，应避免使用`async void`，因为它难以捕获异常
   - 优先使用`async Task`作为返回类型

2. **正确处理ConfigureAwait**
   - 在非UI代码中，可以使用`ConfigureAwait(false)`避免返回到原始上下文
   - 减少线程切换开销，提高性能

3. **使用异步命令**
   - 在MVVM模式中，使用`AsyncRelayCommand`处理异步命令
   - 支持命令执行过程中的加载状态和错误处理

4. **避免死锁**
   - 不要在异步代码中使用`.Result`或`.Wait()`，这可能导致死锁
   - 始终使用`await`来等待异步操作完成

5. **合理使用异步初始化**
   - 对于需要异步初始化的服务，提供明确的初始化方法
   - 确保在使用服务前完成初始化

### 项目中的异步模式示例

1. **异步命令模式**
   ```csharp
   // 在ViewModel中定义异步命令
   public AsyncRelayCommand SaveCommand { get; }

   // 命令执行方法
   private async Task ExecuteSaveAsync() {
       try {
           IsSaving = true;
           await _albumService.SaveAlbumAsync(Album);
           // 处理保存成功
       } catch (Exception ex) {
           // 处理异常
       } finally {
           IsSaving = false;
       }
   }
   ```

2. **任务忽略模式**
   ```csharp
   // 当不需要等待异步操作完成时
   _ = LoadDataAsync(); // 使用下划线忽略返回的Task
   ```

3. **异步初始化模式**
   ```csharp
   public async Task InitializeAsync() {
       if (_isInitialized) return;
       
       // 执行初始化操作
       await _dbConnection.OpenAsync();
       _isInitialized = true;
   }
   ```

通过正确使用async/await异步模式，本项目实现了高效、响应迅速的用户界面，同时保持了代码的可读性和可维护性。异步操作是现代C#应用程序开发中不可或缺的一部分，尤其对于需要处理IO操作、网络请求或数据库访问的应用程序。

## 运行项目

1. 克隆项目代码
2. 打开DailyPoetryA.sln解决方案
3. 还原NuGet包
4. 运行DailyPoetryA项目

## 项目截图

### 主页
显示应用统计信息和快捷入口。

### 专辑墙
显示所有专辑，支持筛选、排序和视图切换。

### 专辑详情
编辑专辑信息和管理专辑歌曲。

### 添加专辑
创建新专辑，添加歌曲和选择封面。

### 收藏视图
显示所有收藏的歌曲。

### 数据概览
显示应用统计信息，包括专辑数量、歌曲数量、收藏歌曲数量等。

## 总结

这个音乐管理应用采用了现代的MVVM架构模式，使用Avalonia UI框架实现了跨平台的用户界面，支持专辑管理、歌曲收藏和数据统计功能。应用具有良好的代码结构和可维护性，适合作为学习Avalonia UI和MVVM架构的参考项目。