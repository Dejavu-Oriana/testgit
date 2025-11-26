using DailyPoetryA.Library.ViewModels;
using DailyPoetryA.UnitTest.Helpers;

namespace DailyPoetryA.UnitTest.ViewModels;

public class ResultViewModelTest : IDisposable {
    public ResultViewModelTest() => PoetryStorageHelper.RemoveDBFile();

    public void Dispose() => PoetryStorageHelper.RemoveDBFile();

    [Fact]
    public async Task TestPoetryCollection() {
        var poetryStorage =
            await PoetryStorageHelper.GetInitializedPoetryStorageAsync();
        var resultViewModel = new ResultViewModel(poetryStorage);

        var statusList = new List<string>();
        resultViewModel.PropertyChanged += (_, args) => {
            if (args.PropertyName == nameof(ResultViewModel.Status)) {
                statusList.Add(resultViewModel.Status);
            }
        };

        Assert.Empty(resultViewModel.PoetryCollection);
        await resultViewModel.PoetryCollection.LoadMoreAsync();
        Assert.Equal(2, statusList.Count);
        Assert.Equal(ResultViewModel.Loading, statusList[0]);
        Assert.Equal("", statusList[1]);
        Assert.Equal(20, resultViewModel.PoetryCollection.Count);
        Assert.True(resultViewModel.PoetryCollection.CanLoadMore);

        await resultViewModel.PoetryCollection.LoadMoreAsync();
        Assert.Equal(5, statusList.Count);
        Assert.Equal(30, resultViewModel.PoetryCollection.Count);
        Assert.False(resultViewModel.PoetryCollection.CanLoadMore);

        await poetryStorage.CloseAsync();
    }
}