using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DailyMusicA.Library.Models;

namespace DailyMusicA.Library.Services;

public interface IMusicStorage
{
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