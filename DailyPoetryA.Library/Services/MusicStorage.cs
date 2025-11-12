using DailyPoetryA.Library.Helpers;
using DailyPoetryA.Library.Models;
using SQLite;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DailyPoetryA.Library.Services;

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
    
    // 用于测试的构造函数，允许指定数据库路径
    public MusicStorage(IPreferenceStorage preferenceStorage, string dbPath)
    {
        _preferenceStorage = preferenceStorage;
        _dbPath = dbPath;
    }
    
    private SQLiteAsyncConnection connection =>
        _connection ??= new SQLiteAsyncConnection(_dbPath);
    
    public bool IsInitialized =>
        _preferenceStorage.Get(MusicStorageConstant.VersionKey, default(int)) == MusicStorageConstant.Version;
    
    public async Task InitializeAsync()
    {
        // 确保数据库文件存在
        if (!System.IO.File.Exists(_dbPath))
        {
            // 创建数据库文件
            await using var dbFileStream = new System.IO.FileStream(_dbPath, System.IO.FileMode.Create);
            dbFileStream.Close();
        }
        
        // 始终创建或更新表结构，这样可以确保表存在且结构正确
        // 使用CreateTableAsync会检查表是否存在，如果不存在则创建，如果存在则更新结构
        await connection.CreateTableAsync<Album>();
        await connection.CreateTableAsync<Song>();
        
        // 设置版本信息
        _preferenceStorage.Set(MusicStorageConstant.VersionKey, MusicStorageConstant.Version);
        
        System.Console.WriteLine("MusicStorage: 数据库初始化完成，表结构已创建或更新");
    }
    
    // 专辑相关操作
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
    
    public async Task<IList<Album>> GetAlbumsAsync(Expression<Func<Album, bool>>? where = null, int skip = 0, int take = 20)
    {
        var query = connection.Table<Album>();
        
        if (where != null)
        {
            query = query.Where(where);
        }
        
        var albums = await query.Skip(skip).Take(take).ToListAsync();
        
        // 加载每个专辑的歌曲
        foreach (var album in albums)
        {
            var songs = await GetSongsByAlbumIdAsync(album.Id);
            album.Songs = new ObservableCollection<Song>(songs);
        }
        
        return albums;
    }
    
    public async Task AddAlbumAsync(Album album)
    {
        await connection.InsertAsync(album);
        
        // 如果专辑包含歌曲，添加歌曲
        if (album.Songs != null && album.Songs.Count > 0)
        {
            foreach (var song in album.Songs)
            {
                song.AlbumId = album.Id;
                await connection.InsertAsync(song);
            }
        }
    }
    
    public async Task UpdateAlbumAsync(Album album)
    {
        await connection.UpdateAsync(album);
    }
    
    public async Task DeleteAlbumAsync(string id)
    {
        // 先删除专辑的所有歌曲
        await DeleteSongsByAlbumIdAsync(id);
        // 再删除专辑
        await connection.DeleteAsync<Album>(id);
    }
    
    // 歌曲相关操作
    public async Task<IList<Song>> GetSongsByAlbumIdAsync(string albumId)
    {
        return await connection.Table<Song>()
            .Where(s => s.AlbumId == albumId)
            .ToListAsync();
    }
    
    public async Task AddSongsAsync(IEnumerable<Song> songs)
    {
        await connection.InsertAllAsync(songs);
    }
    
    public async Task AddSongAsync(Song song)
    {
        await connection.InsertAsync(song);
    }
    
    public async Task UpdateSongAsync(Song song)
    {
        await connection.UpdateAsync(song);
    }
    
    public async Task DeleteSongAsync(string songId)
    {
        await connection.DeleteAsync<Song>(songId);
    }
    
    public async Task DeleteSongsByAlbumIdAsync(string albumId)
    {
        var songs = await GetSongsByAlbumIdAsync(albumId);
        foreach (var song in songs)
        {
            await connection.DeleteAsync<Song>(song.Id);
        }
    }
    
    // 搜索相关操作
    public async Task<IList<Album>> SearchAlbumsAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return await GetAlbumsAsync();
        }
        
        var lowerKeyword = keyword.ToLower();
        var albums = await connection.Table<Album>()
            .Where(a => a.Name.ToLower().Contains(lowerKeyword) || a.Artist.ToLower().Contains(lowerKeyword))
            .OrderByDescending(a => a.AddedDate)
            .ToListAsync();
        
        // 加载歌曲
        foreach (var album in albums)
        {
            var songs = await GetSongsByAlbumIdAsync(album.Id);
            album.Songs = new ObservableCollection<Song>(songs);
        }
        
        return albums;
    }
    
    public async Task CloseAsync()
    {
        await connection.CloseAsync();
    }
}

public static class MusicStorageConstant
{
    public const string VersionKey = nameof(MusicStorageConstant) + "." + nameof(Version);
    public const int Version = 1;
}