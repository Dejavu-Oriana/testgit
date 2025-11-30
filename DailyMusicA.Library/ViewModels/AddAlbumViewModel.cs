using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using DailyMusicA.Library.Helpers;
using DailyMusicA.Library.Models;
using DailyMusicA.Library.Services;

namespace DailyMusicA.Library.ViewModels;

/// <summary>
/// 添加专辑视图模型
/// 用于创建新专辑
/// </summary>
public class AddAlbumViewModel : ViewModelBase
{
    private readonly IContentNavigationService _navigationService;
    private readonly MainViewModel _mainViewModel;
    private readonly IMusicStorage _musicStorage;
    private readonly IFileDialogService _fileDialogService;
    
    // 私有字段
    private string _albumName = string.Empty;
    private string _artist = string.Empty;
    private string _coverUrl = string.Empty;
    private string _originalSelectedImagePath = string.Empty; // 初始化为空字符串，避免null
    private decimal _price = 0;
    private int _nextAlbumId = 1;
    private Bitmap _coverImage = null;
    private bool _isLoading = false;
    private string _errorMessage = string.Empty;
    
    // 歌曲列表
    private readonly ObservableCollection<Song> _songs = new();
    
    // 命令
    private readonly RelayCommand _backCommand;
    private readonly RelayCommand _addAlbumCommand;
    private readonly RelayCommand _addSongCommand;
    private readonly RelayCommand _resetCommand;
    private readonly RelayCommand _previewCommand;
    private readonly RelayCommand _pickCoverCommand;
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public AddAlbumViewModel(
        IContentNavigationService navigationService, 
        MainViewModel mainViewModel, 
        IMusicStorage musicStorage,
        IFileDialogService fileDialogService)
    {
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
        _pickCoverCommand = new RelayCommand(PickCover);
        
        // 初始化表单
        InitializeForm();
    }
    
    /// <summary>
    /// 初始化表单
    /// </summary>
    private async void InitializeForm()
    {
        IsLoading = true;
        try
        {
            if (!_musicStorage.IsInitialized)
            {
                await _musicStorage.InitializeAsync();
            }
            
            // 计算下一个专辑ID
            NextAlbumId = await CalculateNextAlbumIdAsync();
            
            // 添加一首空白歌曲
            AddSong();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"初始化失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    #region 属性
    
    /// <summary>
    /// 专辑名称
    /// </summary>
    public string AlbumName
    {
        get => _albumName;
        set
        {
            if (SetProperty(ref _albumName, value))
            {
                _addAlbumCommand?.NotifyCanExecuteChanged();
                ClearError();
            }
        }
    }
    
    /// <summary>
    /// 歌手
    /// </summary>
    public string Artist
    {
        get => _artist;
        set
        {
            if (SetProperty(ref _artist, value))
            {
                _addAlbumCommand?.NotifyCanExecuteChanged();
                ClearError();
            }
        }
    }
    
    /// <summary>
    /// 封面URL
    /// </summary>
    public string CoverUrl
    {
        get => _coverUrl;
        set => SetProperty(ref _coverUrl, value);
    }
    
    /// <summary>
    /// 封面图片
    /// </summary>
    public Bitmap CoverImage
    {
        get => _coverImage;
        set => SetProperty(ref _coverImage, value);
    }
    
    /// <summary>
    /// 价格文本（用于绑定）
    /// </summary>
    public string PriceText
    {
        get => _price.ToString("0.00");
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _price = 0;
            }
            else if (decimal.TryParse(value, out decimal parsedPrice) && parsedPrice >= 0)
            {
                _price = parsedPrice;
            }
            
            OnPropertyChanged(nameof(PriceText));
            _addAlbumCommand?.NotifyCanExecuteChanged();
            ClearError();
        }
    }
    
    /// <summary>
    /// 价格（只读）
    /// </summary>
    public decimal Price => _price;
    
    /// <summary>
    /// 下一个专辑ID
    /// </summary>
    public int NextAlbumId
    {
        get => _nextAlbumId;
        private set => SetProperty(ref _nextAlbumId, value);
    }
    
    /// <summary>
    /// 歌曲列表
    /// </summary>
    public ObservableCollection<Song> Songs => _songs;
    
    /// <summary>
    /// 是否正在加载
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }
    
    /// <summary>
    /// 错误信息
    /// </summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }
    
    #endregion
    
    #region 命令
    
    /// <summary>
    /// 返回命令
    /// </summary>
    public ICommand BackCommand => _backCommand;
    
    /// <summary>
    /// 添加专辑命令
    /// </summary>
    public ICommand AddAlbumCommand => _addAlbumCommand;
    
    /// <summary>
    /// 添加歌曲命令
    /// </summary>
    public ICommand AddSongCommand => _addSongCommand;
    
    /// <summary>
    /// 重置命令
    /// </summary>
    public ICommand ResetCommand => _resetCommand;
    
    /// <summary>
    /// 预览命令
    /// </summary>
    public ICommand PreviewCommand => _previewCommand;
    
    /// <summary>
    /// 选择封面命令
    /// </summary>
    public ICommand PickCoverCommand => _pickCoverCommand;
    
    /// <summary>
    /// 上移歌曲命令
    /// </summary>
    public ICommand MoveSongUpCommand => new RelayCommand<Song>(MoveSongUp);
    
    /// <summary>
    /// 下移歌曲命令
    /// </summary>
    public ICommand MoveSongDownCommand => new RelayCommand<Song>(MoveSongDown);
    
    /// <summary>
    /// 删除歌曲命令
    /// </summary>
    public ICommand RemoveSongCommand => new RelayCommand<string>(RemoveSong);
    
    #endregion
    
    #region 方法
    
    /// <summary>
    /// 返回上一页
    /// </summary>
    private void Back()
    {
        _mainViewModel.GoBack();
    }
    
    /// <summary>
    /// 重置表单
    /// </summary>
    private async void Reset()
    {
        AlbumName = string.Empty;
        Artist = string.Empty;
        CoverUrl = string.Empty;
        _originalSelectedImagePath = string.Empty; // 清空原始图片路径
        CoverImage = null; // 清空封面图片
        PriceText = "0.00";
        _songs.Clear();
        AddSong();
        
        NextAlbumId = await CalculateNextAlbumIdAsync();
        ClearError();
    }
    
    /// <summary>
    /// 预览专辑
    /// </summary>
    private void Preview()
    {
        // 预览功能可以在这里实现
        ErrorMessage = "预览功能暂未实现";
    }
    
    /// <summary>
    /// 选择封面
    /// </summary>
    private async void PickCover()
    {
        try
        {
            // 使用文件对话框服务打开文件选择对话框
            var files = await _fileDialogService.OpenFileDialogAsync(
                "选择专辑封面",
                "图片文件|*.png;*.jpg;*.jpeg;*.bmp;*.gif|所有文件|*.*",
                false);

            if (files != null && files.Length > 0)
            {
                var selectedFile = files[0];
                
                // 存储原始选中的图片路径，仅在保存专辑时才会复制到目标文件夹
                _originalSelectedImagePath = selectedFile;
                
                // 直接从原始文件路径加载CoverImage用于预览
                try
                {
                    CoverImage = new Bitmap(selectedFile);
                    ErrorMessage = string.Empty;
                    System.Diagnostics.Debug.WriteLine("CoverImage loaded successfully from original path");
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"加载封面图片失败: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine("Failed to load CoverImage: " + ex.Message);
                    CoverImage = null;
                    _originalSelectedImagePath = null;
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"选择封面失败: {ex.Message}";
        }
    }
    
    /// <summary>
    /// 添加歌曲
    /// </summary>
    private void AddSong()
    {
        _songs.Add(new Song
        {
            Title = string.Empty,
            Artist = string.Empty,
            Record = string.Empty,
            IsFavorite = false
        });
    }
    
    /// <summary>
    /// 上移歌曲
    /// </summary>
    private void MoveSongUp(Song? song)
    {
        if (song == null) return;
        
        int index = _songs.IndexOf(song);
        if (index > 0)
        {
            _songs.Move(index, index - 1);
        }
    }
    
    /// <summary>
    /// 下移歌曲
    /// </summary>
    private void MoveSongDown(Song? song)
    {
        if (song == null) return;
        
        int index = _songs.IndexOf(song);
        if (index >= 0 && index < _songs.Count - 1)
        {
            _songs.Move(index, index + 1);
        }
    }
    
    /// <summary>
    /// 删除歌曲
    /// </summary>
    private void RemoveSong(string? songId)
    {
        if (string.IsNullOrEmpty(songId)) return;
        
        var song = _songs.FirstOrDefault(s => s.Id == songId);
        if (song != null)
        {
            _songs.Remove(song);
        }
    }
    
    /// <summary>
    /// 判断是否可以添加专辑
    /// </summary>
    private bool CanAddAlbum()
    {
        return !string.IsNullOrWhiteSpace(AlbumName) && 
               !string.IsNullOrWhiteSpace(Artist) && 
               Price > 0;
    }
    
    /// <summary>
    /// 添加专辑
    /// </summary>
    private async Task AddAlbumAsync()
    {
        if (!CanAddAlbum())
        {
            ErrorMessage = "请填写完整信息：专辑名称、歌手、价格和至少一首歌曲";
            return;
        }
        
        IsLoading = true;
        ErrorMessage = string.Empty;
        
        try
        {
            if (!_musicStorage.IsInitialized)
            {
                await _musicStorage.InitializeAsync();
            }
            
            string albumId = NextAlbumId.ToString();
            
            string coverUrl = CoverUrl.Trim();
            
            if (!string.IsNullOrEmpty(_originalSelectedImagePath))
            {
                var fileName = Path.GetFileName(_originalSelectedImagePath);
                
                // 确保文件有扩展名
                if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
                {
                    fileName += ".jpg";
                }

                var uniqueFileName = $"album_{DateTime.Now.Ticks}_{fileName}";
                var savePath = PathHelper.GetAlbumCoverFilePath(uniqueFileName);

                // 确保album_cover文件夹存在
                var albumCoverDir = Path.GetDirectoryName(savePath);
                if (!string.IsNullOrEmpty(albumCoverDir) && !Directory.Exists(albumCoverDir))
                {
                    Directory.CreateDirectory(albumCoverDir);
                }
                // 复制文件到目标文件夹
                File.Copy(_originalSelectedImagePath, savePath, true);
                
                // 验证文件是否成功保存
                if (File.Exists(savePath))
                {
                    // 更新封面URL，使用明确的file://协议格式
                    var fileUri = new Uri(savePath, UriKind.Absolute);
                    coverUrl = fileUri.ToString();
                    System.Diagnostics.Debug.WriteLine("CoverUrl set to: " + coverUrl);
                }
            }
            
            // 创建新专辑
            var album = new Album
            {
                Id = albumId,
                Name = AlbumName.Trim(),
                Artist = Artist.Trim(),
                CoverUrl = coverUrl,
                Price = Price,
                AddedDate = DateTime.Now,
                Songs = new ObservableCollection<Song>()
            };
            
            // 添加有效的歌曲
            char songSuffix = 'a';
            var hasValidSong = false;
            foreach (var song in _songs.Where(s => 
                !string.IsNullOrWhiteSpace(s.Title) && 
                !string.IsNullOrWhiteSpace(s.Artist)))
            {
                var newSong = new Song
                {
                    Id = $"{albumId}{songSuffix}",
                    AlbumId = albumId,
                    Title = song.Title.Trim(),
                    Artist = song.Artist.Trim(),
                    Record = song.Record?.Trim() ?? string.Empty,
                    IsFavorite = song.IsFavorite
                };
                album.Songs.Add(newSong);
                songSuffix++;
                hasValidSong = true;
            }

            if (!hasValidSong)
            {
                ErrorMessage = "专辑已创建，可在详情页面补充歌曲。";
            }
            
            // 保存到数据库
            await _musicStorage.AddAlbumAsync(album);
            
            // 刷新相关视图模型的数据
            foreach (var viewModel in _mainViewModel.ContentStack)
            {
                if (viewModel is AlbumViewModel albumViewModel)
                {
                    albumViewModel.RefreshAlbumsCommand.Execute(null);
                }
                else if (viewModel is HomeViewModel homeViewModel)
                {
                    // 触发HomeViewModel的刷新命令
                    homeViewModel.RefreshCommand.Execute(null);
                }
                else if (viewModel is StatsViewModel statsViewModel)
                {
                    // 触发StatsViewModel的刷新命令
                    statsViewModel.RefreshCommand.Execute(null);
                }
            }
            
            // 重置表单
            Reset();
            
            // 返回上一页
            Back();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"添加专辑失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// 计算下一个专辑ID
    /// </summary>
    private async Task<int> CalculateNextAlbumIdAsync()
    {
        try
        {
            if (!_musicStorage.IsInitialized)
            {
                await _musicStorage.InitializeAsync();
            }
            
            var albums = await _musicStorage.GetAlbumsAsync(null, 0, 10000);
            
            if (albums != null && albums.Count > 0)
            {
                var maxId = albums
                    .Select(a =>
                    {
                        if (int.TryParse(a.Id, out int id))
                            return id;
                        return 0;
                    })
                    .Max();
                
                return maxId + 1;
            }
            
            return 1;
        }
        catch (Exception)
        {
            return 1;
        }
    }
    
    /// <summary>
    /// 清除错误信息
    /// </summary>
    private void ClearError()
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ErrorMessage = string.Empty;
        }
    }
    
    #endregion
}