using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DailyPoetryA.Library.Helpers;
using DailyPoetryA.Library.Models;
using DailyPoetryA.Library.Services;
using Avalonia.Media.Imaging;
using DailyPoetryA;

namespace DailyPoetryA.Library.ViewModels;

/// <summary>
/// 专辑详情视图模型
/// 用于显示和编辑专辑信息
/// </summary>
public class AlbumDetailViewModel : ViewModelBase
{
    private readonly IContentNavigationService _navigationService;
    private readonly MainViewModel _mainViewModel;
    private readonly IMusicStorage _musicStorage;
    private readonly IFileDialogService _fileDialogService;
    
    private Album? _selectedAlbum;
    private Album? _originalAlbum; // 保存原始数据，用于重置
    private bool _isLoading = false;
    private bool _isSaving = false;
    private string _errorMessage = string.Empty;
    
    // 命令
    private readonly RelayCommand _backCommand;
    private readonly RelayCommand _saveCommand;
    private readonly RelayCommand _saveAndBackCommand;
    private readonly RelayCommand _resetCommand;
    private readonly RelayCommand _deleteCommand;
    private readonly RelayCommand _addSongCommand;
    private readonly RelayCommand _pickCoverCommand;
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public AlbumDetailViewModel(
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
        _saveCommand = new RelayCommand(async () => await SaveChangesAsync(), CanSave);
        _saveAndBackCommand = new RelayCommand(async () => await SaveAndBackAsync(), CanSave);
        _resetCommand = new RelayCommand(Reset);
        _deleteCommand = new RelayCommand(async () => await DeleteAlbumAsync(), CanDelete);
        _addSongCommand = new RelayCommand(AddSong);
        _pickCoverCommand = new RelayCommand(PickCover);
    }
    
    #region 属性
    
    /// <summary>
    /// 当前选中的专辑
    /// </summary>
    public Album? SelectedAlbum
    {
        get => _selectedAlbum;
        set => SetProperty(ref _selectedAlbum, value);
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
    /// 是否正在保存
    /// </summary>
    public bool IsSaving
    {
        get => _isSaving;
        private set => SetProperty(ref _isSaving, value);
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
    /// 保存命令
    /// </summary>
    public ICommand SaveCommand => _saveCommand;
    
    /// <summary>
    /// 保存并返回命令
    /// </summary>
    public ICommand SaveAndBackCommand => _saveAndBackCommand;
    
    /// <summary>
    /// 重置命令
    /// </summary>
    public ICommand ResetCommand => _resetCommand;
    
    /// <summary>
    /// 删除命令
    /// </summary>
    public ICommand DeleteCommand => _deleteCommand;
    
    /// <summary>
    /// 添加歌曲命令
    /// </summary>
    public ICommand AddSongCommand => _addSongCommand;
    
    /// <summary>
    /// 选择封面命令
    /// </summary>
    public ICommand PickCoverCommand => _pickCoverCommand;
    
    /// <summary>
    /// 上移歌曲命令
    /// </summary>
    public ICommand MoveSongUpCommand => new RelayCommand<Song?>(MoveSongUp);
    
    /// <summary>
    /// 下移歌曲命令
    /// </summary>
    public ICommand MoveSongDownCommand => new RelayCommand<Song?>(MoveSongDown);
    
    /// <summary>
    /// 删除歌曲命令
    /// </summary>
    public ICommand RemoveSongCommand => new RelayCommand<string?>(RemoveSong);
    
    #endregion
    
    #region 方法
    
    /// <summary>
    /// 更新专辑数据
    /// </summary>
    /// <param name="album">要显示的专辑</param>
    public void UpdateAlbum(Album album)
    {
        if (album == null) return;
        
        IsLoading = true;
        try
        {
            // 创建专辑的深拷贝，避免直接修改原始数据
            _selectedAlbum = new Album
            {
                Id = album.Id,
                Name = album.Name,
                Artist = album.Artist,
                CoverUrl = album.CoverUrl,
                Price = album.Price,
                AddedDate = album.AddedDate,
                Songs = new ObservableCollection<Song>(album.Songs?.Select(s => new Song
                {
                    Id = s.Id,
                    AlbumId = s.AlbumId,
                    Title = s.Title,
                    Artist = s.Artist,
                    Record = s.Record,
                    IsFavorite = s.IsFavorite
                }) ?? Array.Empty<Song>())
            };
            
            // 加载封面图片到CoverImage属性
            LoadCoverImage(_selectedAlbum);
            
            // 保存原始数据的备份，用于重置
            _originalAlbum = new Album
            {
                Id = album.Id,
                Name = album.Name,
                Artist = album.Artist,
                CoverUrl = album.CoverUrl,
                CoverImage = album.CoverImage, // 复制封面图片
                Price = album.Price,
                AddedDate = album.AddedDate,
                Songs = new ObservableCollection<Song>(album.Songs?.Select(s => new Song
                {
                    Id = s.Id,
                    AlbumId = s.AlbumId,
                    Title = s.Title,
                    Artist = s.Artist,
                    Record = s.Record,
                    IsFavorite = s.IsFavorite
                }) ?? Array.Empty<Song>())
            };
            
            OnPropertyChanged(nameof(SelectedAlbum));
            ClearError();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"加载专辑失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
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
            
            // 加载封面图片到CoverImage属性
            if (Uri.TryCreate(coverUrl, UriKind.Absolute, out var coverUri))
            {
                // 如果是file://协议，直接使用本地路径
                if (coverUri.Scheme == "file")
                {
                    var localPath = coverUri.LocalPath;
                    if (System.IO.File.Exists(localPath))
                    {
                        album.CoverImage = new Bitmap(localPath);
                        System.Console.WriteLine($"AlbumDetailViewModel: 成功加载专辑封面图片: {localPath}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"AlbumDetailViewModel: 加载专辑封面失败: {ex.Message}");
        }
    }
    
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
    private void Reset()
    {
        if (_originalAlbum == null || _selectedAlbum == null) return;
        
        // 恢复原始数据
        _selectedAlbum.Name = _originalAlbum.Name;
        _selectedAlbum.Artist = _originalAlbum.Artist;
        _selectedAlbum.CoverUrl = _originalAlbum.CoverUrl;
        _selectedAlbum.Price = _originalAlbum.Price;
        
        // 恢复歌曲列表
        _selectedAlbum.Songs.Clear();
        foreach (var song in _originalAlbum.Songs)
        {
            _selectedAlbum.Songs.Add(new Song
            {
                Id = song.Id,
                AlbumId = song.AlbumId,
                Title = song.Title,
                Artist = song.Artist,
                Record = song.Record,
                IsFavorite = song.IsFavorite
            });
        }
        
        OnPropertyChanged(nameof(SelectedAlbum));
        ClearError();
    }
    
    /// <summary>
    /// 添加歌曲
    /// </summary>
    private async void AddSong()
    {
        if (_selectedAlbum == null) return;
        
        // 为新歌曲生成ID
        char songSuffix = 'a';
        while (_selectedAlbum.Songs.Any(s => s.Id == _selectedAlbum.Id + songSuffix))
        {
            songSuffix++;
            if (songSuffix > 'z')
            {
                // 如果超过 z，使用数字
                int numSuffix = 1;
                while (_selectedAlbum.Songs.Any(s => s.Id == _selectedAlbum.Id + numSuffix.ToString()))
                {
                    numSuffix++;
                }
                var newSong = new Song
                {
                    Id = _selectedAlbum.Id + numSuffix.ToString(),
                    AlbumId = _selectedAlbum.Id,
                    Title = string.Empty,
                    Artist = _selectedAlbum.Artist,
                    Record = string.Empty,
                    IsFavorite = false
                };
                // 从界面上添加歌曲
                _selectedAlbum.Songs.Add(newSong);
                OnPropertyChanged(nameof(SelectedAlbum));
                
                // 从数据库中直接添加歌曲
                if (_musicStorage != null)
                {
                    await _musicStorage.AddSongAsync(newSong);
                    System.Console.WriteLine($"直接添加歌曲: ID={newSong.Id}");
                }
                
                // 更新原始歌曲列表
                if (_originalAlbum != null && _originalAlbum.Songs != null)
                {
                    _originalAlbum.Songs.Add(new Song
                    {
                        Id = newSong.Id,
                        AlbumId = newSong.AlbumId,
                        Title = newSong.Title,
                        Artist = newSong.Artist,
                        Record = newSong.Record,
                        IsFavorite = newSong.IsFavorite
                    });
                }
                
                // 刷新相关视图模型的数据
                foreach (var viewModel in _mainViewModel.ContentStack)
                {
                    if (viewModel is HomeViewModel homeViewModel)
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
                
                return;
            }
        }
        
        var song = new Song
        {
            Id = _selectedAlbum.Id + songSuffix,
            AlbumId = _selectedAlbum.Id,
            Title = string.Empty,
            Artist = _selectedAlbum.Artist,
            Record = string.Empty,
            IsFavorite = false
        };
        
        // 从界面上添加歌曲
        _selectedAlbum.Songs.Add(song);
        OnPropertyChanged(nameof(SelectedAlbum));
        
        // 从数据库中直接添加歌曲
        if (_musicStorage != null)
        {
            await _musicStorage.AddSongAsync(song);
            System.Console.WriteLine($"直接添加歌曲: ID={song.Id}");
        }
        
        // 更新原始歌曲列表
        if (_originalAlbum != null && _originalAlbum.Songs != null)
        {
            _originalAlbum.Songs.Add(new Song
            {
                Id = song.Id,
                AlbumId = song.AlbumId,
                Title = song.Title,
                Artist = song.Artist,
                Record = song.Record,
                IsFavorite = song.IsFavorite
            });
        }
        
        // 刷新相关视图模型的数据
        foreach (var viewModel in _mainViewModel.ContentStack)
        {
            if (viewModel is HomeViewModel homeViewModel)
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
    }
    
    /// <summary>
    /// 上移歌曲
    /// </summary>
    private void MoveSongUp(Song? song)
    {
        if (song == null || _selectedAlbum == null) return;
        
        int index = _selectedAlbum.Songs.IndexOf(song);
        if (index > 0)
        {
            _selectedAlbum.Songs.Move(index, index - 1);
            OnPropertyChanged(nameof(SelectedAlbum));
        }
    }
    
    /// <summary>
    /// 下移歌曲
    /// </summary>
    private void MoveSongDown(Song? song)
    {
        if (song == null || _selectedAlbum == null) return;
        
        int index = _selectedAlbum.Songs.IndexOf(song);
        if (index >= 0 && index < _selectedAlbum.Songs.Count - 1)
        {
            _selectedAlbum.Songs.Move(index, index + 1);
            OnPropertyChanged(nameof(SelectedAlbum));
        }
    }
    
    /// <summary>
    /// 移除歌曲
    /// </summary>
    private async void RemoveSong(string? songId)
    {
        if (string.IsNullOrEmpty(songId) || _selectedAlbum == null) return;
        
        var song = _selectedAlbum.Songs.FirstOrDefault(s => s.Id == songId);
        if (song != null)
        {
            // 从界面上移除歌曲
            _selectedAlbum.Songs.Remove(song);
            OnPropertyChanged(nameof(SelectedAlbum));
            
            // 从数据库中直接删除歌曲
            if (_musicStorage != null)
            {
                await _musicStorage.DeleteSongAsync(songId);
            }
            
            // 更新原始歌曲列表，确保下次保存时不会重复删除
            if (_originalAlbum != null && _originalAlbum.Songs != null)
            {
                var originalSong = _originalAlbum.Songs.FirstOrDefault(s => s.Id == songId);
                if (originalSong != null)
                {
                    _originalAlbum.Songs.Remove(originalSong);
                }
            }
            
            // 刷新相关视图模型的数据
            foreach (var viewModel in _mainViewModel.ContentStack)
            {
                if (viewModel is HomeViewModel homeViewModel)
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
        }
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

            if (files != null && files.Length > 0 && SelectedAlbum != null)
            {
                var selectedFile = files[0];
                var fileName = Path.GetFileName(selectedFile);
                
                // 确保文件有扩展名
                if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
                {
                    fileName += ".jpg";
                }

                // 生成唯一文件名
                var uniqueFileName = $"album_{DateTime.Now.Ticks}_{fileName}";
                var savePath = PathHelper.GetAlbumCoverFilePath(uniqueFileName);

                // 确保album_cover文件夹存在
                var albumCoverDir = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(albumCoverDir))
                {
                    Directory.CreateDirectory(albumCoverDir);
                }

                // 复制文件到目标文件夹
                File.Copy(selectedFile, savePath, true);

                // 更新封面URL，使用file://协议格式
                SelectedAlbum.CoverUrl = new Uri(savePath).AbsoluteUri;
                ErrorMessage = string.Empty;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"选择封面失败: {ex.Message}";
        }
    }
    
    /// <summary>
    /// 判断是否可以保存
    /// </summary>
    private bool CanSave()
    {
        return _selectedAlbum != null && 
               !string.IsNullOrWhiteSpace(_selectedAlbum.Name) && 
               !string.IsNullOrWhiteSpace(_selectedAlbum.Artist) && 
               _selectedAlbum.Price >= 0 &&
               !IsSaving;
    }
    
    /// <summary>
    /// 判断是否可以删除
    /// </summary>
    private bool CanDelete()
    {
        return _selectedAlbum != null && !IsSaving;
    }
    
    /// <summary>
        /// 保存修改
        /// </summary>
        private async Task SaveChangesAsync()
        {
            if (!CanSave())
            {
                ErrorMessage = "请填写完整信息：专辑名称、歌手和价格";
                return;
            }
            
            IsSaving = true;
            ErrorMessage = string.Empty;
            
            try
            {
                if (!_musicStorage.IsInitialized)
                {
                    await _musicStorage.InitializeAsync();
                }
                
                if (_selectedAlbum == null) return;
                
                // 保存专辑数据
                await _musicStorage.UpdateAlbumAsync(_selectedAlbum);
                
                // 获取当前歌曲列表中的有效ID
                var currentSongIds = _selectedAlbum.Songs?.Select(s => s.Id).Where(id => !string.IsNullOrWhiteSpace(id)).ToHashSet() ?? new HashSet<string>();
                var originalSongIds = _originalAlbum?.Songs?.Select(s => s.Id).Where(id => !string.IsNullOrWhiteSpace(id)).ToHashSet() ?? new HashSet<string>();
                var deletedSongIds = originalSongIds.Except(currentSongIds);
                
                // 调试信息
                System.Console.WriteLine($"原始歌曲ID: {string.Join(", ", originalSongIds)}");
                System.Console.WriteLine($"当前歌曲ID: {string.Join(", ", currentSongIds)}");
                System.Console.WriteLine($"要删除的歌曲ID: {string.Join(", ", deletedSongIds)}");
                
                // 删除不再存在的歌曲
                foreach (var songId in deletedSongIds)
                {
                    System.Console.WriteLine($"删除歌曲ID: {songId}");
                    await _musicStorage.DeleteSongAsync(songId);
                }
                
                // 保存歌曲数据
                if (_selectedAlbum.Songs != null)
                {
                    foreach (var song in _selectedAlbum.Songs)
                    {
                        // 设置歌曲的AlbumId
                        song.AlbumId = _selectedAlbum.Id;
                        
                        // 检查歌曲是否是原始列表中存在的歌曲
                        bool isExistingSong = _originalAlbum?.Songs?.Any(s => s.Id == song.Id) ?? false;
                        
                        if (isExistingSong)
                        {
                            // 现有歌曲，更新
                            await _musicStorage.UpdateSongAsync(song);
                        }
                        else
                        {
                            // 新歌曲，需要添加
                            await _musicStorage.AddSongAsync(song);
                        }
                    }
                }
                
                // 更新原始数据备份，包括歌曲列表
                if (_originalAlbum != null)
                {
                    _originalAlbum.Name = _selectedAlbum.Name;
                    _originalAlbum.Artist = _selectedAlbum.Artist;
                    _originalAlbum.CoverUrl = _selectedAlbum.CoverUrl;
                    _originalAlbum.Price = _selectedAlbum.Price;
                    
                    // 深拷贝歌曲列表，确保备份数据的独立性
                    _originalAlbum.Songs = new ObservableCollection<Song>(_selectedAlbum.Songs?.Select(s => new Song
                    {
                        Id = s.Id,
                        AlbumId = s.AlbumId,
                        Title = s.Title,
                        Artist = s.Artist,
                        Record = s.Record,
                        IsFavorite = s.IsFavorite
                    }) ?? Array.Empty<Song>());
                }
                
                // 重新从数据库加载当前专辑数据，确保显示的信息是最新的
                if (_selectedAlbum != null)
                {
                    var updatedAlbum = await _musicStorage.GetAlbumAsync(_selectedAlbum.Id);
                    if (updatedAlbum != null)
                    {
                        // 加载歌曲数据
                        updatedAlbum.Songs = new ObservableCollection<Song>(await _musicStorage.GetSongsByAlbumIdAsync(updatedAlbum.Id) ?? Array.Empty<Song>());
                        
                        // 使用现有的LoadCoverImage方法加载封面图片
                        LoadCoverImage(updatedAlbum);
                        
                        // 使用新的专辑对象替换当前显示的对象
                        _selectedAlbum = updatedAlbum;
                        OnPropertyChanged(nameof(SelectedAlbum));
                    }
                }
                
                // 检查ContentStack中是否有AlbumViewModel，如果有则刷新
                foreach (var viewModel in _mainViewModel.ContentStack)
                {
                    if (viewModel is AlbumViewModel albumViewModel)
                    {
                        albumViewModel.RefreshAlbumsCommand.Execute(null);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"保存失败: {ex.Message}";
            }
            finally
            {
                IsSaving = false;
            }
    }
    
    /// <summary>
    /// 保存并返回
    /// </summary>
    private async Task SaveAndBackAsync()
    {
        await SaveChangesAsync();
        if (string.IsNullOrEmpty(ErrorMessage))
        {
            Back();
        }
    }
    
    /// <summary>
    /// 删除专辑
    /// </summary>
    private async Task DeleteAlbumAsync()
    {
        if (_selectedAlbum == null) return;
        
        IsSaving = true;
        ErrorMessage = string.Empty;
        
        try
        {
            if (!_musicStorage.IsInitialized)
            {
                await _musicStorage.InitializeAsync();
            }
            
            // 删除专辑（这会同时删除所有歌曲）
            await _musicStorage.DeleteAlbumAsync(_selectedAlbum.Id);
            
            // 检查ContentStack中是否有AlbumViewModel，如果有则刷新
            foreach (var viewModel in _mainViewModel.ContentStack)
            {
                if (viewModel is AlbumViewModel albumViewModel)
                {
                    albumViewModel.RefreshAlbumsCommand.Execute(null);
                    break;
                }
            }
            
            // 返回上一页
            Back();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"删除失败: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
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
