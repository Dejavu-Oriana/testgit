using DailyMusicA.Library.Services;
using System;
using System.IO;

namespace DailyMusicA.UnitTest.Helpers
{
    public static class MusicStorageHelper
    {
        public static string GetTestDbPath()
        {
            // 使用与实际应用相同的路径处理方式
            return MusicStorage.DefaultMusicDbPath;
        }

        public static void RemoveDBFile(string? dbPath = null)
        {
            if (string.IsNullOrEmpty(dbPath))
            {
                dbPath = GetTestDbPath();
            }

            try
            {
                if (File.Exists(dbPath))
                {
                    File.Delete(dbPath);
                }
            }
            catch (IOException)
            {
                // 如果文件被占用，尝试释放资源后再删除
                System.Threading.Thread.Sleep(100);
                try
                {
                    if (File.Exists(dbPath))
                    {
                        File.Delete(dbPath);
                    }
                }
                catch (IOException ex)
                {
                    // 如果再次失败，记录异常但不抛出，避免测试中断
                    System.Diagnostics.Debug.WriteLine($"无法删除测试数据库文件: {ex.Message}");
                }
            }
        }

        public static MusicStorage CreateMusicStorage(string? dbPath = null)
        {
            if (string.IsNullOrEmpty(dbPath))
            {
                dbPath = GetTestDbPath();
            }

            var preferenceStorageMock = new Moq.Mock<IPreferenceStorage>();
            // 设置版本模拟行为
            preferenceStorageMock.Setup(p => p.Get(MusicStorageConstant.VersionKey, Moq.It.IsAny<int>()))
                .Returns(MusicStorageConstant.Version);

            var musicStorage = new MusicStorage(preferenceStorageMock.Object, dbPath);
            return musicStorage;
        }

        public static async System.Threading.Tasks.Task<MusicStorage> CreateAndInitializeMusicStorageAsync(string? dbPath = null)
        {
            var musicStorage = CreateMusicStorage(dbPath);
            await musicStorage.InitializeAsync();
            return musicStorage;
        }
    }
}