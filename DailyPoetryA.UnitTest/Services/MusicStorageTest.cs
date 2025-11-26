// csharp
using DailyPoetryA.Library.Models;
using DailyPoetryA.Library.Services;
using DailyPoetryA.UnitTest.Helpers;
using Moq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        _testDbPath = MusicStorageHelper.GetTestDbPath();
        // 初始化 Mock
        _preferenceStorageMock = new Mock<IPreferenceStorage>();

        // 使用 MusicStorageHelper 确保测试前删除数据库文件
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
        // 设置 Mock 返回已初始化状态
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

            // 添加专辑
            await musicStorage.AddAlbumAsync(album);

            // 获取专辑
            var retrievedAlbum = await musicStorage.GetAlbumAsync(album.Id);

            // 验证结果
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
            // 创建测试专辑
            var album = new Album
            {
                Name = "测试专辑",
                Artist = "测试歌手",
                CoverUrl = "http://example.com/cover.jpg",
                Price = 9.99m
            };

            // 添加专辑
            await musicStorage.AddAlbumAsync(album);

            // 准备歌曲数据
            var songs = new List<Song>
            {
                new Song { Title = "歌曲1", Artist = "测试歌手", AlbumId = album.Id },
                new Song { Title = "歌曲2", Artist = "测试歌手", AlbumId = album.Id },
                new Song { Title = "歌曲3", Artist = "测试歌手", AlbumId = album.Id }
            };

            // 添加歌曲
            await musicStorage.AddSongsAsync(songs);

            // 通过专辑ID获取歌曲
            var retrievedSongs = await musicStorage.GetSongsByAlbumIdAsync(album.Id);

            // 验证结果
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
            // 添加测试专辑
            var album = new Album
            {
                Id = "test-album-update",
                Name = "原始专辑名",
                Artist = "原始歌手",
                Price = 50.00m
            };
            await musicStorage.AddAlbumAsync(album);

            // 更新专辑信息
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
            // 添加多个测试专辑
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
            // 添加带歌曲的专辑
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
            var firstPage = await musicStorage.GetAlbumsAsync(skip: 0, take: 20);
            Assert.Equal(10, firstPage.Count);

            // 获取前5个
            var firstFive = await musicStorage.GetAlbumsAsync(skip: 0, take: 5);
            Assert.Equal(5, firstFive.Count);

            // 获取后5个
            var lastFive = await musicStorage.GetAlbumsAsync(skip: 5, take: 5);
            Assert.Equal(5, lastFive.Count);

            // 验证分页内容不重叠
            Assert.DoesNotContain(firstFive, a => lastFive.Any(b => b.Id == a.Id));
        }
        finally
        {
            await musicStorage.CloseAsync();
        }
    }
}