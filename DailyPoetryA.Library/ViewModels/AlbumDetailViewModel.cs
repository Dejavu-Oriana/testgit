using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DailyPoetryA.Library.Models;
using DailyPoetryA.Library.Services;

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
        IMusicStorage musicStorage)
    {
        _navigationService = navigationService;
        _mainViewModel = mainViewModel;
        _musicStorage = musicStorage;
        
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
            
            // 保存原始数据的备份，用于重置
            _originalAlbum = new Album
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
    private void AddSong()
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
                _selectedAlbum.Songs.Add(newSong);
                OnPropertyChanged(nameof(SelectedAlbum));
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
        
        _selectedAlbum.Songs.Add(song);
        OnPropertyChanged(nameof(SelectedAlbum));
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
    /// 删除歌曲
    /// </summary>
    private void RemoveSong(string? songId)
    {
        if (string.IsNullOrEmpty(songId) || _selectedAlbum == null) return;
        
        var song = _selectedAlbum.Songs.FirstOrDefault(s => s.Id == songId);
        if (song != null)
        {
            _selectedAlbum.Songs.Remove(song);
            OnPropertyChanged(nameof(SelectedAlbum));
        }
    }
    
    /// <summary>
    /// 选择封面
    /// </summary>
    private async void PickCover()
    {
        try
        {
            // 这里应该打开文件选择对话框或URL输入对话框
            // 暂时使用简单的输入方式
            // 实际实现可以使用 Avalonia.Controls.OpenFileDialog 或自定义对话框
            ErrorMessage = "封面选择功能需要实现文件对话框";
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
            
            // 保存歌曲数据
            foreach (var song in _selectedAlbum.Songs)
            {
                if (string.IsNullOrWhiteSpace(song.Id))
                {
                    // 新歌曲，需要添加
                    await _musicStorage.AddSongAsync(song);
                }
                else
                {
                    // 现有歌曲，更新
                    await _musicStorage.UpdateSongAsync(song);
                }
            }
            
            // 更新原始数据备份
            if (_originalAlbum != null)
            {
                _originalAlbum.Name = _selectedAlbum.Name;
                _originalAlbum.Artist = _selectedAlbum.Artist;
                _originalAlbum.CoverUrl = _selectedAlbum.CoverUrl;
                _originalAlbum.Price = _selectedAlbum.Price;
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
