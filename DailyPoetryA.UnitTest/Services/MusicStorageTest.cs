// csharp
using DailyPoetryA.Library.Models;
using DailyPoetryA.Library.Services;
using DailyPoetryA.UnitTest.Helpers;
using Moq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DailyPoetryA.UnitTest.Services;

public class MusicStorageTest : IDisposable
{
    private Mock<IPreferenceStorage> _preferenceStorageMock;
    private readonly string _testDbPath;

    public MusicStorageTest()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), "music_test_db.sqlite3");
        // 初始化 Mock
        _preferenceStorageMock = new Mock<IPreferenceStorage>();

        // 确保测试前删除数据库文件
        MusicStorageHelper.RemoveDBFile(_testDbPath);
    }

    public void Dispose()
    {
        // 测试结束后删除数据库文件
        MusicStorageHelper.RemoveDBFile(_testDbPath);
    }

    private MusicStorage CreateMusicStorage()
    {
        return new MusicStorage(_preferenceStorageMock.Object, _testDbPath);
    }

    [Fact]
    public async Task TestInitializeAsync_WhenDBFileNotExists_DBFileShouldBeCreated()
    {
        var musicStorage = CreateMusicStorage();

        Assert.False(System.IO.File.Exists(_testDbPath));
        await musicStorage.InitializeAsync();
        Assert.True(System.IO.File.Exists(_testDbPath));

        // 验证版本设置
        _preferenceStorageMock.Verify(
            p => p.Set(MusicStorageConstant.VersionKey, MusicStorageConstant.Version),
            Times.Once);

        await musicStorage.CloseAsync();
    }

    [Fact]
    public void TestIsInitialized_WhenInitialized_ShouldReturnTrue()
    {
        _preferenceStorageMock
            .Setup(p => p.Get(MusicStorageConstant.VersionKey, default(int)))
            .Returns(MusicStorageConstant.Version);

        var musicStorage = CreateMusicStorage();
        Assert.True(musicStorage.IsInitialized);

        _preferenceStorageMock.Verify(
            p => p.Get(MusicStorageConstant.VersionKey, default(int)),
            Times.Once
        );
    }

    [Fact]
    public async Task TestAddAlbumAndGetAlbum_ShouldReturnCorrectAlbum()
    {
        var musicStorage = CreateMusicStorage();
        await musicStorage.InitializeAsync();

        try
        {
            // 创建测试专辑
            var album = new Album
            {
                Name = "测试专辑",
                Artist = "测试歌手",
                CoverUrl = "http://example.com/cover.jpg",
                Price = 9.99m
            };

            await musicStorage.AddAlbumAsync(album);
            var retrievedAlbum = await musicStorage.GetAlbumAsync(album.Id);
            
            Assert.NotNull(retrievedAlbum);
            Assert.Equal(album.Name, retrievedAlbum.Name);
            Assert.Equal(album.Artist, retrievedAlbum.Artist);
            Assert.Equal(album.CoverUrl, retrievedAlbum.CoverUrl);
            Assert.Equal(album.Price, retrievedAlbum.Price);
        }
        finally
        {
            await musicStorage.CloseAsync();
        }
    }

    [Fact]
    public async Task TestAddSongsAndGetSongsByAlbum_ShouldReturnCorrectSongs()
    {
        var musicStorage = CreateMusicStorage();
        await musicStorage.InitializeAsync();
        try
        {
            var album = new Album
            {
                Name = "测试专辑",
                Artist = "测试歌手",
                CoverUrl = "http://example.com/cover.jpg",
                Price = 9.99m
            };
            await musicStorage.AddAlbumAsync(album);
            var songs = new List<Song>
            {
                new Song { Title = "歌曲1", Artist = "测试歌手", AlbumId = album.Id },
                new Song { Title = "歌曲2", Artist = "测试歌手", AlbumId = album.Id },
                new Song { Title = "歌曲3", Artist = "测试歌手", AlbumId = album.Id }
            };
            await musicStorage.AddSongsAsync(songs);
            var retrievedSongs = await musicStorage.GetSongsByAlbumIdAsync(album.Id);
            Assert.NotNull(retrievedSongs);
            Assert.Equal(3, retrievedSongs.Count);
            Assert.Contains(retrievedSongs, s => s.Title == "歌曲1");
            Assert.Contains(retrievedSongs, s => s.Title == "歌曲2");
            Assert.Contains(retrievedSongs, s => s.Title == "歌曲3");
        }
        finally
        {
            await musicStorage.CloseAsync();
        }
    }

    [Fact]
    public async Task TestUpdateAlbumAsync_ShouldUpdateAlbumInfo()
    {
        var musicStorage = CreateMusicStorage();
        await musicStorage.InitializeAsync();

        try
        {
            var album = new Album
            {
                Id = "test-album-update",
                Name = "原始专辑名",
                Artist = "原始歌手",
                Price = 50.00m
            };
            await musicStorage.AddAlbumAsync(album);

            album.Name = "更新后的专辑名";
            album.Price = 88.88m;
            await musicStorage.UpdateAlbumAsync(album);

            // 验证更新是否成功
            var updatedAlbum = await musicStorage.GetAlbumAsync("test-album-update");
            Assert.NotNull(updatedAlbum);
            Assert.Equal("更新后的专辑名", updatedAlbum.Name);
            Assert.Equal(88.88m, updatedAlbum.Price);
        }
        finally
        {
            await musicStorage.CloseAsync();
        }
    }


    [Fact]
    public async Task TestSearchAlbumsAsync_ShouldReturnMatchingAlbums()
    {
        var musicStorage = CreateMusicStorage();
        await musicStorage.InitializeAsync();
        try
        {
            var album1 = new Album { Id = "search-album-1", Name = "周杰伦的专辑", Artist = "周杰伦", Price = 88.00m };
            var album2 = new Album { Id = "search-album-2", Name = "林俊杰精选集", Artist = "林俊杰", Price = 99.00m };
            var album3 = new Album { Id = "search-album-3", Name = "周杰伦演唱会", Artist = "周杰伦", Price = 128.00m };

            await musicStorage.AddAlbumAsync(album1);
            await musicStorage.AddAlbumAsync(album2);
            await musicStorage.AddAlbumAsync(album3);

            // 按专辑名搜索
            var resultsByName = await musicStorage.SearchAlbumsAsync("精选集");
            Assert.Single(resultsByName);
            Assert.Equal("林俊杰精选集", resultsByName.First().Name);

            // 按歌手名搜索
            var resultsByArtist = await musicStorage.SearchAlbumsAsync("周杰伦");
            Assert.Equal(2, resultsByArtist.Count);
            Assert.All(resultsByArtist, a => Assert.Equal("周杰伦", a.Artist));

            // 搜索不存在的关键词
            var noResults = await musicStorage.SearchAlbumsAsync("不存在的歌手");
            Assert.Empty(noResults);

            // 空关键词搜索（应该返回所有专辑）
            var allResults = await musicStorage.SearchAlbumsAsync("");
            Assert.Equal(3, allResults.Count);
        }
        finally
        {
            await musicStorage.CloseAsync();
        }
    }

    [Fact]
    public async Task TestAddSongAsync_ShouldAddSingleSong()
    {
        var musicStorage = CreateMusicStorage();
        await musicStorage.InitializeAsync();

        try
        {
            // 先添加一个专辑
            var album = new Album { Id = "single-song-album", Name = "单歌曲专辑", Artist = "测试歌手" };
            await musicStorage.AddAlbumAsync(album);

            // 添加单首歌曲
            var song = new Song { Id = "single-song", Title = "单首歌曲", Artist = "测试歌手", AlbumId = "single-song-album" };
            await musicStorage.AddSongAsync(song);

            // 验证歌曲是否添加成功
            var albumWithSongs = await musicStorage.GetAlbumAsync("single-song-album");
            Assert.NotNull(albumWithSongs);
            Assert.Single(albumWithSongs.Songs);
            Assert.Equal("单首歌曲", albumWithSongs.Songs.First().Title);
        }
        finally
        {
            await musicStorage.CloseAsync();
        }
    }

    [Fact]
    public async Task TestUpdateSongAsync_ShouldUpdateSongInfo()
    {
        var musicStorage = CreateMusicStorage();
        await musicStorage.InitializeAsync();

        try
        {
            // 添加带歌曲的专辑
            var album = new Album
            {
                Id = "update-song-album",
                Name = "专辑",
                Artist = "歌手",
                Songs = new ObservableCollection<Song>
                {
                    new Song { Id = "song-to-update", Title = "原始歌曲名", Artist = "原始歌手" }
                }
            };
            await musicStorage.AddAlbumAsync(album);

            // 更新歌曲信息
            var song = new Song { Id = "song-to-update", Title = "更新后的歌曲名", Artist = "更新后的歌手", AlbumId = "update-song-album" };
            await musicStorage.UpdateSongAsync(song);

            // 验证更新是否成功
            var updatedSongs = await musicStorage.GetSongsByAlbumIdAsync("update-song-album");
            var updatedSong = updatedSongs.FirstOrDefault(s => s.Id == "song-to-update");

            Assert.NotNull(updatedSong);
            Assert.Equal("更新后的歌曲名", updatedSong.Title);
            Assert.Equal("更新后的歌手", updatedSong.Artist);
        }
        finally
        {
            await musicStorage.CloseAsync();
        }
    }

    [Fact]
    public async Task TestDeleteSongAsync_ShouldDeleteSingleSong()
    {
        var musicStorage = CreateMusicStorage();
        await musicStorage.InitializeAsync();
        try
        {
            var album = new Album
            {
                Id = "delete-song-album",
                Name = "专辑",
                Artist = "歌手",
                Songs = new ObservableCollection<Song>
                {
                    new Song { Id = "song-to-delete", Title = "要删除的歌曲", Artist = "歌手" },
                    new Song { Id = "song-to-keep", Title = "要保留的歌曲", Artist = "歌手" }
                }
            };
            await musicStorage.AddAlbumAsync(album);

            // 删除指定歌曲
            await musicStorage.DeleteSongAsync("song-to-delete");

            // 验证歌曲是否被删除
            var remainingSongs = await musicStorage.GetSongsByAlbumIdAsync("delete-song-album");
            Assert.Single(remainingSongs);
            Assert.Equal("song-to-keep", remainingSongs.First().Id);
        }
        finally
        {
            await musicStorage.CloseAsync();
        }
    }

    [Fact]
    public async Task TestGetAlbumsAsync_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        var musicStorage = CreateMusicStorage();
        await musicStorage.InitializeAsync();

        try
        {
            // 空数据库测试
            var albums = await musicStorage.GetAlbumsAsync();
            Assert.Empty(albums);

            // 使用分页参数的空数据库测试
            var paginatedAlbums = await musicStorage.GetAlbumsAsync(null, 0, 10);
            Assert.Empty(paginatedAlbums);
        }
        finally
        {
            await musicStorage.CloseAsync();
        }
    }

    [Fact]
    public async Task TestGetAlbumsAsync_WithNormalData_ShouldReturnAllAlbums()
    {
        var musicStorage = CreateMusicStorage();
        await musicStorage.InitializeAsync();

        try
        {
            // 添加几张专辑
            var album1 = new Album { Id = "album1", Name = "专辑1", Artist = "歌手A", Price = 10 };
            var album2 = new Album { Id = "album2", Name = "专辑2", Artist = "歌手B", Price = 20 };
            var album3 = new Album { Id = "album3", Name = "专辑3", Artist = "歌手C", Price = 30 };

            await musicStorage.AddAlbumAsync(album1);
            await musicStorage.AddAlbumAsync(album2);
            await musicStorage.AddAlbumAsync(album3);

            // 获取所有专辑
            var albums = await musicStorage.GetAlbumsAsync();
            Assert.Equal(3, albums.Count);
            
            // 验证返回的专辑包含所有添加的专辑
            Assert.Contains(albums, a => a.Id == "album1");
            Assert.Contains(albums, a => a.Id == "album2");
            Assert.Contains(albums, a => a.Id == "album3");
        }
        finally
        {
            await musicStorage.CloseAsync();
        }
    }

    [Fact]
    public async Task TestGetAlbumsAsync_WithLargeData_ShouldReturnAllAlbums()
    {
        var musicStorage = CreateMusicStorage();
        await musicStorage.InitializeAsync();

        try
        {
            // 添加25张专辑用于测试
            for (int i = 1; i <= 25; i++)
            {
                var album = new Album
                {
                    Id = $"large-album-{i}",
                    Name = $"大测试专辑{i}",
                    Artist = $"测试歌手{i % 5 + 1}",
                    Price = 10 * i
                };
                await musicStorage.AddAlbumAsync(album);
            }

            // 获取所有专辑（显式设置take为一个较大值以确保返回所有专辑）
            var allAlbums = await musicStorage.GetAlbumsAsync(take: 100);
            Assert.Equal(25, allAlbums.Count);

            // 验证返回的专辑ID符合预期
            for (int i = 1; i <= 25; i++)
            {
                Assert.Contains(allAlbums, a => a.Id == $"large-album-{i}");
            }
        }
        finally
        {
            await musicStorage.CloseAsync();
        }
    }

    [Fact]
    public async Task TestGetAlbumsAsync_WithPagination_ShouldReturnCorrectPage()
    {
        var musicStorage = CreateMusicStorage();
        await musicStorage.InitializeAsync();

        try
        {
            // 添加多个专辑用于分页测试
            for (int i = 1; i <= 10; i++)
            {
                var album = new Album
                {
                    Id = $"pagination-album-{i}",
                    Name = $"分页专辑{i}",
                    Artist = "测试歌手",
                    Price = 10 * i
                };
                await musicStorage.AddAlbumAsync(album);
            }

            // 获取第一页（默认每页20个，应该返回所有10个）
            var firstPage = await musicStorage.GetAlbumsAsync(null, skip: 0, take: 20);
            Assert.Equal(10, firstPage.Count);

            var firstFive = await musicStorage.GetAlbumsAsync(null, skip: 0, take: 5);
            Assert.Equal(5, firstFive.Count);

            var lastFive = await musicStorage.GetAlbumsAsync(null, skip: 5, take: 5);
            Assert.Equal(5, lastFive.Count);

            // 验证分页内容不重叠
            Assert.DoesNotContain(firstFive, a => lastFive.Any(b => b.Id == a.Id));
        }
        finally
        {
            await musicStorage.CloseAsync();
        }
    }

    [Fact]
    public async Task TestGetAlbumsAsync_WithSkipBeyondCount_ShouldReturnEmptyList()
    {
        var musicStorage = CreateMusicStorage();
        await musicStorage.InitializeAsync();

        try
        {
            // 添加3张专辑
            for (int i = 1; i <= 3; i++)
            {
                var album = new Album
                {
                    Id = $"skip-album-{i}",
                    Name = $"跳过专辑{i}",
                    Artist = "测试歌手"
                };
                await musicStorage.AddAlbumAsync(album);
            }

            // 跳过超过总数
            var albums = await musicStorage.GetAlbumsAsync(null, skip: 5, take: 10);
            Assert.Empty(albums);

            // 跳过等于总数
            var exactlyAtEnd = await musicStorage.GetAlbumsAsync(null, skip: 3, take: 10);
            Assert.Empty(exactlyAtEnd);
        }
        finally
        {
            await musicStorage.CloseAsync();
        }
    }

    [Fact]
    public async Task TestGetAlbumsAsync_WithTakeZero_ShouldReturnEmptyList()
    {
        var musicStorage = CreateMusicStorage();
        await musicStorage.InitializeAsync();

        try
        {
            // 添加几张专辑
            for (int i = 1; i <= 5; i++)
            {
                var album = new Album
                {
                    Id = $"take-zero-album-{i}",
                    Name = $"零获取专辑{i}",
                    Artist = "测试歌手"
                };
                await musicStorage.AddAlbumAsync(album);
            }

            // take为0应该返回空列表
            var albums = await musicStorage.GetAlbumsAsync(null, skip: 0, take: 0);
            Assert.Empty(albums);
        }
        finally
        {
            await musicStorage.CloseAsync();
        }
    }

    [Fact]
    public async Task TestGetAlbumsAsync_WithNegativeParameters_ShouldHandleGracefully()
    {
        var musicStorage = CreateMusicStorage();
        await musicStorage.InitializeAsync();

        try
        {
            // 添加几张专辑
            for (int i = 1; i <= 5; i++)
            {
                var album = new Album
                {
                    Id = $"negative-param-album-{i}",
                    Name = $"负数参数专辑{i}",
                    Artist = "测试歌手"
                };
                await musicStorage.AddAlbumAsync(album);
            }

            // 负数skip参数（应该视为0）
            var negativeSkip = await musicStorage.GetAlbumsAsync(null, skip: -3, take: 10);
            Assert.NotNull(negativeSkip);
            
            // 负数take参数（应该返回空列表或正常处理）
            var negativeTake = await musicStorage.GetAlbumsAsync(null, skip: 0, take: -5);
            Assert.NotNull(negativeTake);
            
            // 验证数据库仍然可以正常查询
            var normalQuery = await musicStorage.GetAlbumsAsync();
            Assert.Equal(5, normalQuery.Count);
        }
        finally
        {
            await musicStorage.CloseAsync();
        }
    }

    [Fact]
    public async Task TestDeleteAlbumAsync_ShouldDeleteAlbumAndSongs()
    {
        var musicStorage = CreateMusicStorage();
        await musicStorage.InitializeAsync();
        try
        {
            // 创建带歌曲的测试专辑
            var albumId = "delete-test-album";
            var album = new Album
            {
                Id = albumId,
                Name = "要删除的专辑",
                Artist = "测试歌手",
                Songs = new ObservableCollection<Song>
                {
                    new Song { Id = "song1", Title = "歌曲1", Artist = "测试歌手", AlbumId = albumId },
                    new Song { Id = "song2", Title = "歌曲2", Artist = "测试歌手", AlbumId = albumId }
                }
            };
            await musicStorage.AddAlbumAsync(album);
            var addedAlbum = await musicStorage.GetAlbumAsync(albumId);
            Assert.NotNull(addedAlbum);
            Assert.Equal(2, addedAlbum.Songs.Count);

            await musicStorage.DeleteAlbumAsync(albumId);

            var deletedAlbum = await musicStorage.GetAlbumAsync(albumId);
            Assert.Null(deletedAlbum);

            var remainingSongs = await musicStorage.GetSongsByAlbumIdAsync(albumId);
            Assert.Empty(remainingSongs);
        }
        finally
        {
            await musicStorage.CloseAsync();
        }
    }

    [Fact]
    public async Task TestDeleteAlbumAsync_WithNonExistentAlbum_ShouldNotThrowException()
    {
        var musicStorage = CreateMusicStorage();
        await musicStorage.InitializeAsync();

        try
        {
            // 尝试删除不存在的专辑，不应该抛出异常
            await musicStorage.DeleteAlbumAsync("non-existent-album-id");

            // 验证操作成功完成
            var albums = await musicStorage.GetAlbumsAsync();
            Assert.Empty(albums);
        }
        finally
        {
            await musicStorage.CloseAsync();
        }
    }

    [Fact]
    public async Task TestDeleteSongsByAlbumIdAsync_ShouldDeleteAllSongs()
    {
        var musicStorage = CreateMusicStorage();
        await musicStorage.InitializeAsync();
        try
        {
            var albumId = "delete-songs-album";
            var album = new Album
            {
                Id = albumId,
                Name = "测试专辑",
                Artist = "测试歌手",
                Songs = new ObservableCollection<Song>
                {
                    new Song { Id = "song1", Title = "歌曲1", Artist = "测试歌手", AlbumId = albumId },
                    new Song { Id = "song2", Title = "歌曲2", Artist = "测试歌手", AlbumId = albumId },
                    new Song { Id = "song3", Title = "歌曲3", Artist = "测试歌手", AlbumId = albumId }
                }
            };
            await musicStorage.AddAlbumAsync(album);

            var songsBefore = await musicStorage.GetSongsByAlbumIdAsync(albumId);
            Assert.Equal(3, songsBefore.Count);

            await musicStorage.DeleteSongsByAlbumIdAsync(albumId);

            var songsAfter = await musicStorage.GetSongsByAlbumIdAsync(albumId);
            Assert.Empty(songsAfter);

            var albumAfter = await musicStorage.GetAlbumAsync(albumId);
            Assert.NotNull(albumAfter);
            Assert.Empty(albumAfter.Songs);
        }
        finally
        {
            await musicStorage.CloseAsync();
        }
    }

    [Fact]
    public async Task TestDeleteSongsByAlbumIdAsync_WithNonExistentAlbumId_ShouldNotThrowException()
    {
        var musicStorage = CreateMusicStorage();
        await musicStorage.InitializeAsync();

        try
        {
            // 尝试删除不存在专辑的歌曲，不应该抛出异常
            await musicStorage.DeleteSongsByAlbumIdAsync("non-existent-album-id");

            // 添加一个测试专辑来验证数据库仍然正常工作
            var testAlbum = new Album { Id = "test-album", Name = "测试专辑", Artist = "测试歌手" };
            await musicStorage.AddAlbumAsync(testAlbum);
            
            var addedAlbum = await musicStorage.GetAlbumAsync("test-album");
            Assert.NotNull(addedAlbum);
        }
        finally
        {
            await musicStorage.CloseAsync();
        }
    }

    [Fact]
    public async Task TestDeleteSongsByAlbumIdAsync_WithEmptyAlbum_ShouldWorkCorrectly()
    {
        var musicStorage = CreateMusicStorage();
        await musicStorage.InitializeAsync();

        try
        {
            // 创建一个没有歌曲的专辑
            var albumId = "empty-album";
            var album = new Album
            {
                Id = albumId,
                Name = "空专辑",
                Artist = "测试歌手"
            };
            await musicStorage.AddAlbumAsync(album);

            // 验证专辑没有歌曲
            var songsBefore = await musicStorage.GetSongsByAlbumIdAsync(albumId);
            Assert.Empty(songsBefore);

            // 尝试删除空专辑的歌曲
            await musicStorage.DeleteSongsByAlbumIdAsync(albumId);

            // 验证专辑仍然存在且没有歌曲
            var songsAfter = await musicStorage.GetSongsByAlbumIdAsync(albumId);
            Assert.Empty(songsAfter);
            
            var albumAfter = await musicStorage.GetAlbumAsync(albumId);
            Assert.NotNull(albumAfter);
        }
        finally
        {
            await musicStorage.CloseAsync();
        }
    }
}