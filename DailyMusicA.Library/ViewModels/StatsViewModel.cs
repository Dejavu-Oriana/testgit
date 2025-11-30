using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DailyMusicA.Library.Models;
using DailyMusicA.Library.Services;

namespace DailyMusicA.Library.ViewModels;

/// <summary>
/// 数据概览视图模型
/// </summary>
public class StatsViewModel : ViewModelBase
{
    private readonly IMusicStorage _musicStorage;
    private bool _isLoading;
    private int _albumCount;
    private int _songCount;
    private int _favoriteSongCount;
    private Album? _latestAlbum;
    private Album? _mostSongsAlbum;
    private string _topArtistName = "暂无数据";
    private int _topArtistSongCount;

    public StatsViewModel(IMusicStorage musicStorage)
    {
        _musicStorage = musicStorage;
        RefreshCommand = new RelayCommand(async () => await LoadStatsAsync());
        FavoriteSongsPreview = new ObservableCollection<Song>();
        _ = LoadStatsAsync();
    }

    public ObservableCollection<Album> RecentAlbums { get; } = new();
    public ObservableCollection<Song> FavoriteSongsPreview { get; }

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public int AlbumCount
    {
        get => _albumCount;
        private set => SetProperty(ref _albumCount, value);
    }

    public int SongCount
    {
        get => _songCount;
        private set => SetProperty(ref _songCount, value);
    }

    public int FavoriteSongCount
    {
        get => _favoriteSongCount;
        private set => SetProperty(ref _favoriteSongCount, value);
    }

    public Album? LatestAlbum
    {
        get => _latestAlbum;
        private set => SetProperty(ref _latestAlbum, value);
    }

    public Album? MostSongsAlbum
    {
        get => _mostSongsAlbum;
        private set => SetProperty(ref _mostSongsAlbum, value);
    }

    public string TopArtistName
    {
        get => _topArtistName;
        private set => SetProperty(ref _topArtistName, value);
    }

    public int TopArtistSongCount
    {
        get => _topArtistSongCount;
        private set => SetProperty(ref _topArtistSongCount, value);
    }

    public ICommand RefreshCommand { get; }

    public virtual async Task LoadStatsAsync()
    {
        IsLoading = true;
        try
        {
            if (!_musicStorage.IsInitialized)
            {
                await _musicStorage.InitializeAsync();
            }

            var albums = await _musicStorage.GetAlbumsAsync(null, 0, 1000) ?? new List<Album>();

            AlbumCount = albums.Count;
            var allSongs = new List<Song>();
            foreach (var album in albums)
            {
                var songs = await _musicStorage.GetSongsByAlbumIdAsync(album.Id) ?? new List<Song>();
                album.Songs = new ObservableCollection<Song>(songs);
                allSongs.AddRange(songs.Select(s =>
                {
                    s.Album = album;
                    return s;
                }));
            }

            SongCount = allSongs.Count;
            FavoriteSongCount = allSongs.Count(s => s.IsFavorite);
            LatestAlbum = albums.OrderByDescending(a => a.AddedDate).FirstOrDefault();
            MostSongsAlbum = albums.OrderByDescending(a => a.Songs?.Count ?? 0).FirstOrDefault();

            if (allSongs.Count > 0)
            {
                var topArtist = allSongs
                    .Where(s => !string.IsNullOrWhiteSpace(s.Artist))
                    .GroupBy(s => s.Artist)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();
                if (topArtist != null)
                {
                    TopArtistName = topArtist.Key;
                    TopArtistSongCount = topArtist.Count();
                }
                else
                {
                    TopArtistName = "暂无数据";
                    TopArtistSongCount = 0;
                }
            }
            else
            {
                TopArtistName = "暂无数据";
                TopArtistSongCount = 0;
            }

            RecentAlbums.Clear();
            foreach (var album in albums
                .OrderByDescending(a => a.AddedDate)
                .Take(5))
            {
                RecentAlbums.Add(album);
            }

            FavoriteSongsPreview.Clear();
            foreach (var favSong in allSongs
                         .Where(s => s.IsFavorite)
                         .OrderByDescending(s => s.Album?.AddedDate)
                         .Take(5))
            {
                FavoriteSongsPreview.Add(favSong);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}

