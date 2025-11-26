using DailyPoetryA.Library.Services;
using Moq;

namespace DailyPoetryA.UnitTest.Helpers;

public static class PoetryStorageHelper {
    public static void RemoveDBFile() =>
        File.Delete(PoetryStorage.PoetryDbPath);

    public static async Task<PoetryStorage> GetInitializedPoetryStorageAsync() {
        var preferenceStorageMock = new Mock<IPreferenceStorage>();
        var mockPreferenceStorage = preferenceStorageMock.Object;
        var poetryStorage = new PoetryStorage(mockPreferenceStorage);
        await poetryStorage.InitializeAsync();
        return poetryStorage;
    }
}