using DailyPoetryA.Library.Services;
using Moq;
using System.IO;

namespace DailyPoetryA.UnitTest.Helpers;

public static class PoetryStorageHelper {
    public static void RemoveDBFile() {
        try {
            // 尝试删除实际的数据库文件
            string dbPath = PoetryStorage.PoetryDbPath;
            if (File.Exists(dbPath)) {
                File.Delete(dbPath);
            }
        } catch (IOException) {
            // 如果文件被占用，尝试释放资源后再删除
            System.Threading.Thread.Sleep(100);
            try {
                string dbPath = PoetryStorage.PoetryDbPath;
                if (File.Exists(dbPath)) {
                    File.Delete(dbPath);
                }
            } catch (IOException ex) {
                // 如果再次失败，记录异常但不抛出，避免测试中断
                System.Diagnostics.Debug.WriteLine($"无法删除数据库文件: {ex.Message}");
            }
        }
    }

    public static async Task<PoetryStorage> GetInitializedPoetryStorageAsync() {
        // 创建模拟的偏好存储
        var preferenceStorageMock = new Mock<IPreferenceStorage>();
        
        // 设置模拟对象在调用Get时返回正确的版本
        preferenceStorageMock
            .Setup(p => p.Get(PoetryStorageConstant.VersionKey, default(int)))
            .Returns(PoetryStorageConstant.Version);
            
        var poetryStorage = new PoetryStorage(preferenceStorageMock.Object);
        
        try {
            // 尝试初始化
            await poetryStorage.InitializeAsync();
        } catch (Exception ex) {
            // 如果初始化失败，记录异常但不抛出
            System.Diagnostics.Debug.WriteLine($"初始化PoetryStorage失败: {ex.Message}");
        }
        
        return poetryStorage;
    }
}