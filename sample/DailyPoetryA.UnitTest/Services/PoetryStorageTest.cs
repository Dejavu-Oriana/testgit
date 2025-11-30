using System.Linq.Expressions;
using DailyPoetryA.Library.Models;
using DailyPoetryA.Library.Services;
using DailyPoetryA.UnitTest.Helpers;
using Moq;

namespace DailyPoetryA.UnitTest.Services;

public class PoetryStorageTest : IDisposable {
    public PoetryStorageTest() => 
        PoetryStorageHelper.RemoveDBFile();

    public void Dispose() => 
        PoetryStorageHelper.RemoveDBFile();
    

    [Fact]
    public async Task
        TestInitializeAsync_WhenDBFileNotExists_DBFileShouldBeCreated() {
        var preferenceStorageMock = new Mock<IPreferenceStorage>();
        var mockPreferenceStorage = preferenceStorageMock.Object;

        var poetryStorage = new PoetryStorage(mockPreferenceStorage);
        
        Assert.False(File.Exists(PoetryStorage.PoetryDbPath));
        await poetryStorage.InitializeAsync();
        Assert.True(File.Exists(PoetryStorage.PoetryDbPath));

        preferenceStorageMock.Verify(
            p => p.Set(PoetryStorageConstant.VersionKey, 
                PoetryStorageConstant.Version), Times.Once);
    }

    [Fact]
    public void TestIsInitialized_WhenInitialized_ShouldReturnTrue() {
        var preferenceStorageMock = new Mock<IPreferenceStorage>();
        preferenceStorageMock
            .Setup(p => p.Get(PoetryStorageConstant.VersionKey, default(int)))
            .Returns(PoetryStorageConstant.Version);
        var mockPreferenceStorage = preferenceStorageMock.Object;

        var poetryStorage = new PoetryStorage(mockPreferenceStorage);
        Assert.True(poetryStorage.IsInitialized);

        preferenceStorageMock.Verify(
            p => p.Get(PoetryStorageConstant.VersionKey, default(int)),
            Times.Once
        );
    }

    [Fact]
    public async Task TestGetPoetryAsync_GivenCorrectId_ShouldReturnPoetry() {
        var poetryStorage = 
            await PoetryStorageHelper.GetInitializedPoetryStorageAsync();
        var poetry = await poetryStorage.GetPoetryAsync(10001);
        Assert.Equal("临江仙 · 夜归临皋", poetry.Name);
        await poetryStorage.CloseAsync();
    }

    [Fact]
    public async Task TestGetPoetriesAsync_GetIntMaxValue_ShouldReturnNumberPoetry() {
        var poetryStorage = await PoetryStorageHelper.GetInitializedPoetryStorageAsync();
        var poetries = await poetryStorage.GetPoetriesAsync(
            Expression.Lambda<Func<Poetry, bool>>(Expression.Constant(true),
            Expression.Parameter(typeof(Poetry), "p"))
            , 0, int.MaxValue
        );
        Assert.Equal(PoetryStorage.NumberPoetry, poetries.Count);
        await poetryStorage.CloseAsync();
    }
}