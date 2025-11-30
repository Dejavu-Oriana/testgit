using DailyMusicA.Library.Models;
using DailyMusicA.Library.Services;
using DailyMusicA.Library.ViewModels;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DailyMusicA.UnitTest.Helpers;
using Xunit;

namespace DailyMusicA.UnitTest.ViewModels
{
    public class AlbumViewModelTest : IDisposable
    {
        private readonly Mock<IMusicStorage> _musicStorageMock;
        private readonly Mock<IContentNavigationService> _navigationServiceMock;
        private readonly AlbumViewModel _viewModel;
        private readonly List<Album> _testAlbums;

        public AlbumViewModelTest()
        {
            try
            {
                MusicStorageHelper.RemoveDBFile();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"初始化测试时清理数据文件失败: {ex.Message}");
            }
            
            _musicStorageMock = new Mock<IMusicStorage>();
            _navigationServiceMock = new Mock<IContentNavigationService>();

            // 设置MusicStorage.IsInitialized为true
            _musicStorageMock.Setup(x => x.IsInitialized).Returns(true);

            _testAlbums = new List<Album>
            {
                new Album
                {
                    Id = "1",
                    Name = "专辑1",
                    Artist = "歌手A",
                    AddedDate = System.DateTime.Now.AddDays(-2),
                    Songs = new ObservableCollection<Song>
                    {
                        new Song { Id = "101", Title = "歌曲1", IsFavorite = true },
                        new Song { Id = "102", Title = "歌曲2", IsFavorite = false }
                    }
                },
                new Album
                {
                    Id = "2",
                    Name = "专辑2",
                    Artist = "歌手B",
                    AddedDate = System.DateTime.Now.AddDays(-5),
                    Songs = new ObservableCollection<Song>
                    {
                        new Song { Id = "201", Title = "歌曲3", IsFavorite = false }
                    }
                },
                new Album
                {
                    Id = "3",
                    Name = "热门专辑",
                    Artist = "歌手A",
                    AddedDate = System.DateTime.Now.AddDays(-1),
                    Songs = new ObservableCollection<Song>
                    {
                        // 创建11首歌曲以测试"歌曲量≥10"的筛选和统计（共14首）
                        new Song { Id = "301", Title = "歌曲4", IsFavorite = true },
                        new Song { Id = "302", Title = "歌曲5", IsFavorite = false },
                        new Song { Id = "303", Title = "歌曲6", IsFavorite = false },
                        new Song { Id = "304", Title = "歌曲7", IsFavorite = false },
                        new Song { Id = "305", Title = "歌曲8", IsFavorite = false },
                        new Song { Id = "306", Title = "歌曲9", IsFavorite = false },
                        new Song { Id = "307", Title = "歌曲10", IsFavorite = false },
                        new Song { Id = "308", Title = "歌曲11", IsFavorite = false },
                        new Song { Id = "309", Title = "歌曲12", IsFavorite = false },
                        new Song { Id = "310", Title = "歌曲13", IsFavorite = false },
                        new Song { Id = "311", Title = "歌曲14", IsFavorite = false }
                    }
                }
            };

            // 设置MusicStorage方法返回测试数据
            _musicStorageMock.Setup(x => x.GetAlbumsAsync(null, 0, 1000)).ReturnsAsync(_testAlbums);
            foreach (var album in _testAlbums)
            {
                _musicStorageMock.Setup(x => x.GetSongsByAlbumIdAsync(album.Id)).ReturnsAsync(album.Songs);
            }

            // 初始化ViewModel
            _viewModel = new AlbumViewModel(_musicStorageMock.Object, _navigationServiceMock.Object);
        }

        [Fact]
        public void Constructor_ShouldInitializePropertiesCorrectly()
        {
            // 验证属性初始化
            Assert.NotNull(_viewModel.Albums);
            Assert.NotNull(_viewModel.FilteredAlbums);
            Assert.False(_viewModel.IsLoading);
            Assert.Equal(string.Empty, _viewModel.FilterText);
            Assert.Equal("添加时间 ↓", _viewModel.SelectedSortOption);
            Assert.Equal("全部", _viewModel.SelectedQuickFilter);
            Assert.True(_viewModel.IsGridView);
            Assert.False(_viewModel.IsListView);

            // 验证命令初始化
            Assert.NotNull(_viewModel.LoadAlbumsCommand);
            Assert.NotNull(_viewModel.RefreshAlbumsCommand);
            Assert.NotNull(_viewModel.AddAlbumCommand);
            Assert.NotNull(_viewModel.ViewAlbumCommand);
            Assert.NotNull(_viewModel.ClearFilterCommand);
        }

        [Fact]
        public async Task RefreshAlbumsCommand_ShouldLoadAlbums_WhenExecuted()
        {
            // 执行加载专辑操作
            _viewModel.RefreshAlbumsCommand.Execute(null);
            // 等待异步操作完成
            await Task.Delay(100);

            _musicStorageMock.Verify(x => x.GetAlbumsAsync(null, 0, 1000), Times.Once);
            foreach (var album in _testAlbums)
            {
                _musicStorageMock.Verify(x => x.GetSongsByAlbumIdAsync(album.Id), Times.Once);
            }

            Assert.Equal(_testAlbums.Count, _viewModel.Albums.Count);
            Assert.Equal(_testAlbums.Count, _viewModel.FilteredAlbums.Count);
        }

        [Fact]
        public async Task RefreshAlbumsCommand_ShouldHandleException_WhenErrorOccurs()
        {
            // 设置MusicStorage抛出异常
            _musicStorageMock.Setup(x => 
                x.GetAlbumsAsync(null, 0, 1000)).ThrowsAsync(new System.Exception("加载失败"));

            _viewModel.RefreshAlbumsCommand.Execute(null);
            await Task.Delay(100);

            // 验证错误处理
            Assert.Contains("加载失败", _viewModel.ErrorMessage);
            Assert.False(_viewModel.IsDatabaseConnected);
        }

        [Fact]
        public async Task NavigateToAddAlbum_ShouldCallNavigationService()
        {
            // 执行添加专辑命令
            _viewModel.AddAlbumCommand.Execute(null);

            // 验证导航服务被调用
            _navigationServiceMock.Verify(x =>
                x.NavigateTo(ContentNavigationConstant.AddAlbum, null), Times.Once);
        }

        [Fact]
        public async Task ViewAlbum_ShouldCallNavigationService_WhenAlbumIsNotNull()
        {
            // 先加载专辑
            _viewModel.RefreshAlbumsCommand.Execute(null);
            await Task.Delay(100);
            var albumToView = _viewModel.Albums.First();

            // 执行查看专辑命令
            _viewModel.ViewAlbumCommand.Execute(albumToView);

            // 验证导航服务被调用
            _navigationServiceMock.Verify(x => 
                x.NavigateTo(ContentNavigationConstant.AlbumDetail, albumToView), Times.Once);
        }

        [Fact]
        public async Task ViewAlbum_ShouldNotCallNavigationService_WhenAlbumIsNull()
        {
            // 执行查看专辑命令，传递null
            _viewModel.ViewAlbumCommand.Execute(null);

            // 验证导航服务未被调用
            _navigationServiceMock.Verify(x => 
                x.NavigateTo(ContentNavigationConstant.AlbumDetail, It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task FilterText_ShouldFilterAlbums()
        {
            // 先加载专辑
            _viewModel.RefreshAlbumsCommand.Execute(null);
            await Task.Delay(100);

            // 设置筛选文本
            _viewModel.FilterText = "专辑1";

            // 验证筛选结果
            Assert.Single(_viewModel.FilteredAlbums);
            Assert.Equal("专辑1", _viewModel.FilteredAlbums[0].Name);
        }

        [Fact]
        public async Task FilterText_ShouldFilterByArtist()
        {
            // 先加载专辑
            _viewModel.RefreshAlbumsCommand.Execute(null);
            await Task.Delay(100);

            // 设置筛选文本
            _viewModel.FilterText = "歌手A";

            // 验证筛选结果
            Assert.Equal(2, _viewModel.FilteredAlbums.Count);
            Assert.All(_viewModel.FilteredAlbums, album => Assert.Equal("歌手A", album.Artist));
        }

        [Fact]
        public async Task SelectedQuickFilter_Recent_ShouldFilterRecentAlbums()
        {
            // 先加载专辑
            _viewModel.RefreshAlbumsCommand.Execute(null);
            await Task.Delay(100);

            // 设置快速筛选为"最近新增"
            _viewModel.SelectedQuickFilter = "最近新增";

            // 验证筛选结果，应该只有最近7天内添加的专辑
            Assert.All(_viewModel.FilteredAlbums, album => 
                Assert.True((System.DateTime.Now - album.AddedDate).TotalDays <= 7));
        }

        [Fact]
        public async Task SelectedQuickFilter_Favorites_ShouldFilterAlbumsWithFavorites()
        {
            // 先加载专辑
            _viewModel.RefreshAlbumsCommand.Execute(null);
            await Task.Delay(100);

            // 设置快速筛选为"收藏精选"
            _viewModel.SelectedQuickFilter = "收藏精选";

            // 验证筛选结果，应该只有包含收藏歌曲的专辑
            Assert.All(_viewModel.FilteredAlbums, album => 
                Assert.True(album.Songs.Any(song => song.IsFavorite)));
        }

        [Fact]
        public async Task SelectedQuickFilter_ManySongs_ShouldFilterAlbumsWithManySongs()
        {
            // 先加载专辑
            _viewModel.RefreshAlbumsCommand.Execute(null);
            await Task.Delay(100);

            // 设置快速筛选为"歌曲量≥10"
            _viewModel.SelectedQuickFilter = "歌曲量≥10";

            // 验证筛选结果，应该只有歌曲数量≥10的专辑
            Assert.All(_viewModel.FilteredAlbums, album => 
                Assert.True(album.Songs.Count >= 10));
        }

        [Fact]
        public async Task SelectedSortOption_NameAscending_ShouldSortAlbumsByNameAscending()
        {
            // 先加载专辑
            _viewModel.RefreshAlbumsCommand.Execute(null);
            await Task.Delay(100);

            // 设置排序选项为"名称 A→Z"
            _viewModel.SelectedSortOption = "名称 A→Z";

            // 验证排序结果
            var sortedNames = _viewModel.FilteredAlbums.Select(a => a.Name).ToList();
            var expectedNames = _viewModel.Albums.OrderBy(a => a.Name).Select
                (a => a.Name).ToList();
            Assert.Equal(expectedNames, sortedNames);
        }

        [Fact]
        public async Task SelectedSortOption_SongCountDescending_ShouldSortAlbumsBySongCountDescending()
        {
            // 先加载专辑
            _viewModel.RefreshAlbumsCommand.Execute(null);
            await Task.Delay(100);

            // 设置排序选项为"歌曲数量 ↓"
            _viewModel.SelectedSortOption = "歌曲数量 ↓";

            // 验证排序结果
            var sortedCounts = _viewModel.FilteredAlbums.Select(a => a.Songs.Count).ToList();
            var expectedCounts = _viewModel.Albums.OrderByDescending
                (a => a.Songs.Count).Select(a => a.Songs.Count).ToList();
            Assert.Equal(expectedCounts, sortedCounts);
        }

        [Fact]
        public async Task ClearFiltersCommand_ShouldResetAllFilters()
        {
            // 先加载专辑并设置筛选条件
            _viewModel.RefreshAlbumsCommand.Execute(null);
            await Task.Delay(100);
            _viewModel.FilterText = "测试";
            _viewModel.SelectedSortOption = "名称 A→Z";
            _viewModel.SelectedQuickFilter = "收藏精选";

            // 执行清空筛选命令
            _viewModel.ClearFilterCommand.Execute(null);

            // 验证筛选条件被重置
            Assert.Equal(string.Empty, _viewModel.FilterText);
            Assert.Equal("添加时间 ↓", _viewModel.SelectedSortOption);
            Assert.Equal("全部", _viewModel.SelectedQuickFilter);
            Assert.Equal(_viewModel.Albums.Count, _viewModel.FilteredAlbums.Count);
        }

        [Fact]
        public async Task UpdateStatistics_ShouldUpdateCountersCorrectly()
        {
            // 先加载专辑
            _viewModel.RefreshAlbumsCommand.Execute(null);
            await Task.Delay(100);

            // 验证统计数据
            Assert.Equal(3, _viewModel.TotalAlbums);
            Assert.Equal(14, _viewModel.TotalSongs); // 专辑1有2首，专辑2有1首，专辑3有10首，加上专辑3额外的1首
            Assert.Equal(2, _viewModel.FavoriteSongs); // 专辑1有1首，专辑3有1首
        }

        [Fact]
        public void IsGridView_SetToTrue_ShouldUpdateIsListViewToFalse()
        {
            // 先设置ListView为true
            _viewModel.IsListView = true;

            // 设置GridView为true
            _viewModel.IsGridView = true;

            // 验证IsListView被更新为false
            Assert.False(_viewModel.IsListView);
            Assert.True(_viewModel.IsGridView);
        }

        [Fact]
        public void IsListView_SetToTrue_ShouldUpdateIsGridViewToFalse()
        {
            // 确保GridView为true
            _viewModel.IsGridView = true;

            // 设置ListView为true
            _viewModel.IsListView = true;

            // 验证IsGridView被更新为false
            Assert.False(_viewModel.IsGridView);
            Assert.True(_viewModel.IsListView);
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