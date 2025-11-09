using DailyPoetryA.Library.Helpers;
using DailyPoetryA.Library.Models;
using SQLite;
using System.Collections.Generic;
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
        // 检查是否存在musicdb.sqlite3文件（类似PoetryStorage的处理方式）
        var dbFileExists = false;
        
        // 尝试从程序集资源中获取数据库文件
        await using var dbAssertStream = typeof(Album).Assembly.GetManifestResourceStream(DbName);
        if (dbAssertStream != null)
        {
            // 如果资源中存在数据库文件，则复制到目标位置
            await using var dbFileStream = new System.IO.FileStream(_dbPath, System.IO.FileMode.Create);
            await dbAssertStream.CopyToAsync(dbFileStream);
            dbFileExists = true;
        }
        // 同时检查当前目录是否存在musicdb.sqlite3文件
        else if (System.IO.File.Exists(DbName))
        {
            // 如果当前目录存在数据库文件，则复制到目标位置
            await using var sourceStream = new System.IO.FileStream(DbName, System.IO.FileMode.Open);
            await using var destStream = new System.IO.FileStream(_dbPath, System.IO.FileMode.Create);
            await sourceStream.CopyToAsync(destStream);
            dbFileExists = true;
        }
        
        // 如果没有找到预存的数据库文件，则创建新的数据库结构
        if (!dbFileExists)
        {
            // 创建数据库文件
            await using var dbFileStream = new System.IO.FileStream(_dbPath, System.IO.FileMode.Create);
            dbFileStream.Close();
            
            // 创建表
            await connection.CreateTableAsync<Album>();
            await connection.CreateTableAsync<Song>();
        }
        
        // 设置版本信息
        _preferenceStorage.Set(MusicStorageConstant.VersionKey, MusicStorageConstant.Version);
    }
    
    // 专辑相关操作
    public async Task<Album?> GetAlbumAsync(string id)
    {
        var album = await connection.Table<Album>().FirstOrDefaultAsync(a => a.Id == id);
        if (album != null)
        {
            // 加载歌曲
            var songs = await GetSongsByAlbumIdAsync(id);
            album.Songs = songs.ToList();
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
            album.Songs = songs.ToList();
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
            album.Songs = songs.ToList();
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