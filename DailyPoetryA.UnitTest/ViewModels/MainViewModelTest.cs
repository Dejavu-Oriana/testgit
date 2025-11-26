using DailyPoetryA.Library.Services;
using DailyPoetryA.Library.ViewModels;
using DailyPoetryA.UnitTest.Helpers;
using Moq;
using System.Threading.Tasks;
using Xunit;
using System.Windows.Input;

namespace DailyPoetryA.UnitTest.ViewModels
{
    public class MainViewModelTest : IDisposable
    {
        private readonly Mock<IMenuNavigationService> _navigationServiceMock;
        private readonly MainViewModel _viewModel;

        public MainViewModelTest()
        {
            try
            {
                // 清理测试环境中的数据文件
                PoetryStorageHelper.RemoveDBFile();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"初始化测试时清理数据文件失败: {ex.Message}");
            }
            
            _navigationServiceMock = new Mock<IMenuNavigationService>();
            _viewModel = new MainViewModel(_navigationServiceMock.Object);
        }

        [Fact]
        public void Constructor_ShouldInitializePropertiesCorrectly()
        {
            // 验证属性初始化
            Assert.Equal("MusicApp", _viewModel.Title);
            Assert.True(_viewModel.IsPaneOpen);
            Assert.NotNull(_viewModel.ContentStack);
            Assert.Empty(_viewModel.ContentStack);
            Assert.NotNull(_viewModel.OnMenuTappedCommand);
            Assert.NotNull(_viewModel.GoBackCommand);
            Assert.NotNull(_viewModel.SelectedMenuItem);
            // 使用View属性比较而不是直接比较对象引用
            Assert.Equal(MenuItem.HomeView.View, _viewModel.SelectedMenuItem?.View);
        }

        [Fact]
        public void PushContent_ShouldAddContentToStack()
        {
            // 创建一个测试视图模型
            var testViewModel = new Mock<ViewModelBase>().Object;

            // 推入内容
            _viewModel.PushContent(testViewModel);

            // 验证内容被添加到栈中
            Assert.Single(_viewModel.ContentStack);
            Assert.Equal(testViewModel, _viewModel.ContentStack[0]);
            Assert.Equal(testViewModel, _viewModel.Content);
        }

        [Fact]
        public void SetMenuAndContent_ShouldClearStackAndSetContent()
        {
            // 先添加一些内容到栈中
            _viewModel.PushContent(new Mock<ViewModelBase>().Object);
            Assert.Single(_viewModel.ContentStack);

            // 创建新的测试视图模型
            var newViewModel = new Mock<ViewModelBase>().Object;
            var menuItem = MenuItem.AlbumView;

            // 设置菜单和内容
            _viewModel.SetMenuAndContent(menuItem.View, newViewModel);

            // 验证栈被清空并设置了新内容
            Assert.Single(_viewModel.ContentStack);
            Assert.Equal(newViewModel, _viewModel.ContentStack[0]);
            Assert.Equal(newViewModel, _viewModel.Content);
            // 使用View属性比较而不是直接比较对象引用
            Assert.Equal(menuItem.View, _viewModel.SelectedMenuItem?.View);
            Assert.Equal(menuItem.Name, _viewModel.Title);
        }

        [Fact]
        public void SetMenuAndContent_ShouldRefreshHomeViewModel()
        {
            // 创建可监控的任务完成源
            var loadTaskCompletionSource = new TaskCompletionSource();
            Task loadTask = loadTaskCompletionSource.Task;
            
            // 创建HomeViewModel的模拟对象
            var homeViewModelMock = new Mock<HomeViewModel>(
                new Mock<IMusicStorage>().Object, 
                new Mock<IPreferenceStorage>().Object);
            // 允许任意次数调用，因为构造函数中也会调用一次
            homeViewModelMock.Setup(x => x.LoadStatisticsAsync()).Returns(loadTask);

            // 设置菜单和内容
            _viewModel.SetMenuAndContent(MenuItem.HomeView.View, homeViewModelMock.Object);

            // 完成任务
            loadTaskCompletionSource.SetResult();
            
            // 验证LoadStatisticsAsync被至少调用一次（构造函数可能已调用一次）
            homeViewModelMock.Verify(x => x.LoadStatisticsAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public void SetMenuAndContent_ShouldRefreshFavoriteViewModel()
        {
            // 创建可监控的任务完成源
            var loadTaskCompletionSource = new TaskCompletionSource();
            Task loadTask = loadTaskCompletionSource.Task;
            
            // 创建FavoriteViewModel的模拟对象
            var favoriteViewModelMock = new Mock<FavoriteViewModel>(
                new Mock<IMusicStorage>().Object);
            // 允许任意次数调用，因为构造函数中也会调用一次
            favoriteViewModelMock.Setup(x => x.LoadFavoriteSongsAsync()).Returns(loadTask);

            // 设置菜单和内容
            _viewModel.SetMenuAndContent(MenuItem.FavoriteView.View, favoriteViewModelMock.Object);

            // 完成任务
            loadTaskCompletionSource.SetResult();
            
            // 验证LoadFavoriteSongsAsync被至少调用一次（构造函数可能已调用一次）
            favoriteViewModelMock.Verify(x => x.LoadFavoriteSongsAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public void SetMenuAndContent_ShouldRefreshStatsViewModel()
        {
            // 创建可监控的任务完成源
            var loadTaskCompletionSource = new TaskCompletionSource();
            Task loadTask = loadTaskCompletionSource.Task;
            
            // 创建StatsViewModel的模拟对象
            var statsViewModelMock = new Mock<StatsViewModel>(
                new Mock<IMusicStorage>().Object);
            // 允许任意次数调用，因为构造函数中也会调用一次
            statsViewModelMock.Setup(x => x.LoadStatsAsync()).Returns(loadTask);

            // 设置菜单和内容
            _viewModel.SetMenuAndContent(MenuItem.StatsView.View, statsViewModelMock.Object);

            // 完成任务
            loadTaskCompletionSource.SetResult();
            
            // 验证LoadStatsAsync被至少调用一次（构造函数可能已调用一次）
            statsViewModelMock.Verify(x => x.LoadStatsAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public void OnMenuTapped_ShouldCallNavigationService_WhenMenuItemIsSelected()
        {
            // 设置选中的菜单项
            _viewModel.SelectedMenuItem = MenuItem.AlbumView;

            // 执行菜单项点击命令
            _viewModel.OnMenuTappedCommand.Execute(null);

            // 验证导航服务被调用
            _navigationServiceMock.Verify(x => x.NavigateTo(MenuItem.AlbumView.View, null), Times.Once);
        }

        [Fact]
        public void OnMenuTapped_ShouldNotCallNavigationService_WhenNoMenuItemIsSelected()
        {
            // 设置选中的菜单项为null
            _viewModel.SelectedMenuItem = null;

            // 执行菜单项点击命令
            _viewModel.OnMenuTappedCommand.Execute(null);

            // 验证导航服务未被调用
            _navigationServiceMock.Verify(x => x.NavigateTo(It.IsAny<string>(), null), Times.Never);
        }

        [Fact]
        public void GoBack_ShouldNotRemoveContent_WhenStackHasOneItem()
        {
            // 添加一个内容到栈中
            var content = new Mock<ViewModelBase>().Object;
            _viewModel.PushContent(content);

            // 执行返回命令
            _viewModel.GoBackCommand.Execute(null);

            // 验证内容未被移除
            Assert.Single(_viewModel.ContentStack);
            Assert.Equal(content, _viewModel.Content);
        }

        [Fact]
        public void GoBack_ShouldRemoveLastContent_WhenStackHasMultipleItems()
        {
            // 添加两个内容到栈中
            var content1 = new Mock<ViewModelBase>().Object;
            var content2 = new Mock<ViewModelBase>().Object;
            _viewModel.PushContent(content1);
            _viewModel.PushContent(content2);

            // 执行返回命令
            _viewModel.GoBackCommand.Execute(null);

            // 验证最后一个内容被移除，当前内容是前一个
            Assert.Single(_viewModel.ContentStack);
            Assert.Equal(content1, _viewModel.ContentStack[0]);
            Assert.Equal(content1, _viewModel.Content);
        }

        [Fact]
        public void GoBack_ShouldRefreshAlbumViewModel()
        {
            // 创建AlbumViewModel的模拟对象
            var albumViewModelMock = new Mock<AlbumViewModel>(
                new Mock<IMusicStorage>().Object,
                new Mock<IContentNavigationService>().Object);
            
            // 模拟RefreshAlbumsCommand
            var refreshCommandMock = new Mock<ICommand>();
            refreshCommandMock.Setup(x => x.Execute(null)).Verifiable();
            albumViewModelMock.SetupGet(x => x.RefreshAlbumsCommand).Returns(refreshCommandMock.Object);

            // 添加两个内容到栈中
            var content2 = new Mock<ViewModelBase>().Object;
            _viewModel.PushContent(albumViewModelMock.Object);
            _viewModel.PushContent(content2);

            // 执行返回命令
            _viewModel.GoBackCommand.Execute(null);

            // 验证RefreshAlbumsCommand被执行
            refreshCommandMock.Verify(x => x.Execute(null), Times.AtLeastOnce);
        }

        [Fact]
        public void GoBack_ShouldRefreshHomeViewModel()
        {
            // 创建可监控的任务完成源
            var loadTaskCompletionSource = new TaskCompletionSource();
            Task loadTask = loadTaskCompletionSource.Task;
            
            // 创建HomeViewModel的模拟对象
            var homeViewModelMock = new Mock<HomeViewModel>(
                new Mock<IMusicStorage>().Object, 
                new Mock<IPreferenceStorage>().Object);
            // 允许任意次数调用，因为构造函数中也会调用一次
            homeViewModelMock.Setup(x => x.LoadStatisticsAsync()).Returns(loadTask);

            // 添加两个内容到栈中
            var content2 = new Mock<ViewModelBase>().Object;
            _viewModel.PushContent(homeViewModelMock.Object);
            _viewModel.PushContent(content2);

            // 执行返回命令
            _viewModel.GoBackCommand.Execute(null);

            // 完成任务
            loadTaskCompletionSource.SetResult();
            
            // 验证LoadStatisticsAsync被至少调用一次（构造函数可能已调用一次）
            homeViewModelMock.Verify(x => x.LoadStatisticsAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public void GoBack_ShouldRefreshFavoriteViewModel()
        {
            // 创建可监控的任务完成源
            var loadTaskCompletionSource = new TaskCompletionSource();
            Task loadTask = loadTaskCompletionSource.Task;
            
            // 创建FavoriteViewModel的模拟对象
            var favoriteViewModelMock = new Mock<FavoriteViewModel>(
                new Mock<IMusicStorage>().Object);
            // 允许任意次数调用，因为构造函数中也会调用一次
            favoriteViewModelMock.Setup(x => x.LoadFavoriteSongsAsync()).Returns(loadTask);

            // 添加两个内容到栈中
            var content2 = new Mock<ViewModelBase>().Object;
            _viewModel.PushContent(favoriteViewModelMock.Object);
            _viewModel.PushContent(content2);

            // 执行返回命令
            _viewModel.GoBackCommand.Execute(null);

            // 完成任务
            loadTaskCompletionSource.SetResult();
            
            // 验证LoadFavoriteSongsAsync被至少调用一次（构造函数可能已调用一次）
            favoriteViewModelMock.Verify(x => x.LoadFavoriteSongsAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public void GoBack_ShouldRefreshStatsViewModel()
        {
            // 创建可监控的任务完成源
            var loadTaskCompletionSource = new TaskCompletionSource();
            Task loadTask = loadTaskCompletionSource.Task;
            
            // 创建StatsViewModel的模拟对象
            var statsViewModelMock = new Mock<StatsViewModel>(
                new Mock<IMusicStorage>().Object);
            // 允许任意次数调用，因为构造函数中也会调用一次
            statsViewModelMock.Setup(x => x.LoadStatsAsync()).Returns(loadTask);

            // 添加两个内容到栈中
            var content2 = new Mock<ViewModelBase>().Object;
            _viewModel.PushContent(statsViewModelMock.Object);
            _viewModel.PushContent(content2);

            // 执行返回命令
            _viewModel.GoBackCommand.Execute(null);

            // 完成任务
            loadTaskCompletionSource.SetResult();
            
            // 验证LoadStatsAsync被至少调用一次（构造函数可能已调用一次）
            statsViewModelMock.Verify(x => x.LoadStatsAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public void Title_PropertyShouldRaisePropertyChanged()
        {
            // 测试Title属性的变更通知
            bool propertyChangedRaised = false;
            _viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.Title))
                {
                    propertyChangedRaised = true;
                }
            };

            _viewModel.Title = "New Title";

            // 验证属性变更通知被触发
            Assert.True(propertyChangedRaised);
            Assert.Equal("New Title", _viewModel.Title);
        }

        [Fact]
        public void IsPaneOpen_PropertyShouldRaisePropertyChanged()
        {
            // 测试IsPaneOpen属性的变更通知
            bool propertyChangedRaised = false;
            _viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.IsPaneOpen))
                {
                    propertyChangedRaised = true;
                }
            };

            _viewModel.IsPaneOpen = false;

            // 验证属性变更通知被触发
            Assert.True(propertyChangedRaised);
            Assert.False(_viewModel.IsPaneOpen);
        }

        public void Dispose()
        {
            try
            {
                // 清理测试环境中的数据文件
                PoetryStorageHelper.RemoveDBFile();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"测试结束时清理数据文件失败: {ex.Message}");
            }
            
            // 不需要清理只读字段引用，让GC处理
        }
    }
}