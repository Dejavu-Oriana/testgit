using DailyMusicA.Library.Models;
using DailyMusicA.Library.Services;
using DailyMusicA.Library.ViewModels;
using Moq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;
using System.Linq.Expressions;
using DailyMusicA.UnitTest.Helpers;
using Xunit;

namespace DailyMusicA.UnitTest.ViewModels
{
    public class FavoriteViewModelTest : IDisposable
    {
        private readonly Mock<IMusicStorage> _musicStorageMock;
        private readonly FavoriteViewModel _viewModel;
        private readonly List<Album> _testAlbums;
        private readonly List<Song> _testFavoriteSongs;

        public FavoriteViewModelTest()
        {
            try
            {
                // 清理测试环境中的数据文件
                MusicStorageHelper.RemoveDBFile();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"初始化测试时清理数据文件失败: {ex.Message}");
            }
            
            _musicStorageMock = new Mock<IMusicStorage>();

            // 创建测试专辑和歌曲数据
            _testAlbums = new List<Album>
            {
                new Album
                {
                    Id = "1",
                    Name = "专辑1",
                    Artist = "歌手A",
                    CoverUrl = "http://example.com/cover1.jpg"
                },
                new Album
                {
                    Id = "2",
                    Name = "专辑2",
                    Artist = "歌手B",
                    CoverUrl = "http://example.com/cover2.jpg"
                }
            };

            // 创建测试歌曲，部分设置为收藏
            var songs1 = new List<Song>
            {
                new Song { Id = "101", AlbumId = "1", Title = "歌曲1", Artist = "歌手A", IsFavorite = true },
                new Song { Id = "102", AlbumId = "1", Title = "歌曲2", Artist = "歌手A", IsFavorite = false }
            };

            var songs2 = new List<Song>
            {
                new Song { Id = "201", AlbumId = "2", Title = "歌曲3", Artist = "歌手B", IsFavorite = true },
                new Song { Id = "202", AlbumId = "2", Title = "歌曲4", Artist = "歌手B", IsFavorite = false }
            };

            _testFavoriteSongs = new List<Song> { songs1[0], songs2[0] };

            // 设置MusicStorage方法返回测试数据
            _musicStorageMock.Setup(x => x.GetAlbumsAsync(null, 0, 20)).ReturnsAsync(_testAlbums);
            _musicStorageMock.Setup(x => x.GetSongsByAlbumIdAsync("1")).ReturnsAsync(songs1);
            _musicStorageMock.Setup(x => x.GetSongsByAlbumIdAsync("2")).ReturnsAsync(songs2);

            // 初始化ViewModel（注意：构造函数会异步调用LoadFavoriteSongsAsync）
            _viewModel = new FavoriteViewModel(_musicStorageMock.Object);
        }

        [Fact]
        public void Constructor_ShouldInitializePropertiesCorrectly()
        {
            Assert.NotNull(_viewModel.FavoriteSongs);
            Assert.False(_viewModel.IsLoading);
            Assert.NotNull(_viewModel.RefreshCommand);
        }

        [Fact]
        public async Task LoadFavoriteSongsAsync_ShouldLoadOnlyFavoriteSongs()
        {
            // 执行加载操作
            await _viewModel.LoadFavoriteSongsAsync();

            // 验证MusicStorage方法被调用
            _musicStorageMock.Verify(x => x.GetAlbumsAsync(null, 0, 20), Times.AtLeast(1));
            _musicStorageMock.Verify(x => x.GetSongsByAlbumIdAsync("1"), Times.AtLeastOnce);
            _musicStorageMock.Verify(x => x.GetSongsByAlbumIdAsync("2"), Times.AtLeastOnce);

            // 验证只有收藏的歌曲被加载
            Assert.Equal(2, _viewModel.FavoriteSongs.Count);
            Assert.Contains(_viewModel.FavoriteSongs, song => song.Title == "歌曲1");
            Assert.Contains(_viewModel.FavoriteSongs, song => song.Title == "歌曲3");
            Assert.All(_viewModel.FavoriteSongs, song => Assert.True(song.IsFavorite));
            
            // 验证每首歌曲都关联了正确的专辑信息
            Assert.All(_viewModel.FavoriteSongs, song => Assert.NotNull(song.Album));
        }

        [Fact]
        public async Task LoadFavoriteSongsAsync_ShouldHandleEmptyDatabase()
        {
            // 重置mock以避免前一个测试的影响
            _musicStorageMock.Reset();
            
            // 清除ViewModel的FavoriteSongs集合
            _viewModel.FavoriteSongs.Clear();
            
            // 设置MusicStorage方法返回空数据
            _musicStorageMock.Setup(x => x.GetAlbumsAsync(null, 0, 1000)).ReturnsAsync(new List<Album>());

            // 执行加载收藏歌曲操作
            await _viewModel.LoadFavoriteSongsAsync();

            // 验证没有歌曲被加载
            Assert.Empty(_viewModel.FavoriteSongs);
        }

        [Fact]
        public async Task LoadFavoriteSongsAsync_ShouldHandleExceptions()
        {
            // 重置mock以避免前一个测试的影响
            _musicStorageMock.Reset();
            
            // 清除ViewModel的FavoriteSongs集合
            _viewModel.FavoriteSongs.Clear();
            
            // 设置mock抛出异常
            _musicStorageMock.Setup(x => x.GetAlbumsAsync(null, 0, 20)).ThrowsAsync(new Exception("Test exception"));

            await _viewModel.LoadFavoriteSongsAsync();

            Assert.Empty(_viewModel.FavoriteSongs);
            Assert.False(_viewModel.IsLoading);
        }

        [Fact]
        public async Task LoadFavoriteSongsAsync_ShouldNotLoadTwice_WhenAlreadyLoading()
        {
            // 清除现有数据
            _viewModel.FavoriteSongs.Clear();
            _musicStorageMock.Reset();
            
            // 设置mock以延迟返回数据
            var tcs = new TaskCompletionSource<IList<Album>>();
            _musicStorageMock.Setup(x => x.GetAlbumsAsync(It.IsAny<Expression<Func<Album, bool>>>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(() => tcs.Task);
            
            // 设置GetSongsByAlbumIdAsync返回空列表
            _musicStorageMock.Setup(x => x.GetSongsByAlbumIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<Song>());

            // 启动第一次加载
            var firstLoadTask = _viewModel.LoadFavoriteSongsAsync();

            // 等待一小段时间确保加载已经开始
            await Task.Delay(10);
            Assert.True(_viewModel.IsLoading);

            // 尝试启动第二次加载
            var secondLoadTask = _viewModel.LoadFavoriteSongsAsync();

            // 允许第一次加载完成
            tcs.SetResult(new List<Album>());
            await firstLoadTask;
            await secondLoadTask;

            // 验证只调用了一次GetAlbumsAsync
            _musicStorageMock.Verify(x => x.GetAlbumsAsync(It.IsAny<Expression<Func<Album, bool>>>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task RefreshCommand_ShouldExecuteLoadFavoriteSongsAsync()
        {
            // 确保_loadTask为null，以便可以执行加载
            await _viewModel.LoadFavoriteSongsAsync();

            // 重新设置mock以验证调用
            _musicStorageMock.ResetCalls();
            _musicStorageMock.Setup(x => x.GetAlbumsAsync(null, 0, 1000)).ReturnsAsync(_testAlbums);
            _musicStorageMock.Setup(x => x.GetSongsByAlbumIdAsync("1")).ReturnsAsync(new List<Song>());
            _musicStorageMock.Setup(x => x.GetSongsByAlbumIdAsync("2")).ReturnsAsync(new List<Song>());

            // 执行刷新命令
            _viewModel.RefreshCommand.Execute(null);

            // 等待异步操作完成
            await Task.Delay(100);

            // 验证GetAlbumsAsync被调用
            _musicStorageMock.Verify(x => x.GetAlbumsAsync(null, 0, 20), Times.Once);
        }

        [Fact]
        public async Task LoadFavoriteSongsAsync_ShouldClearExistingSongsBeforeAddingNew()
        {
            // 先加载一些收藏歌曲
            await _viewModel.LoadFavoriteSongsAsync();
            var initialCount = _viewModel.FavoriteSongs.Count;
            Assert.True(initialCount > 0);

            // 修改返回的数据
            _musicStorageMock.ResetCalls();
            _musicStorageMock.Setup(x => x.GetAlbumsAsync(null, 0, 1000)).ReturnsAsync(_testAlbums);
            _musicStorageMock.Setup(x => x.GetSongsByAlbumIdAsync("1")).ReturnsAsync(new List<Song>());
            _musicStorageMock.Setup(x => x.GetSongsByAlbumIdAsync("2")).ReturnsAsync(new List<Song>());

            // 再次加载
            await _viewModel.LoadFavoriteSongsAsync();

            // 验证列表被清空
            Assert.Empty(_viewModel.FavoriteSongs);
        }

        public void Dispose()
        {
            try
            {
                // 清理测试环境中的数据文件
                MusicStorageHelper.RemoveDBFile();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"测试结束时清理数据文件失败: {ex.Message}");
            }
            
            // 不需要清理只读字段引用，让GC处理
        }
    }
}