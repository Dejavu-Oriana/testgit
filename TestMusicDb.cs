using System;
using System.IO;
using SQLite;
using DailyPoetryA.Library.Helpers;

class TestMusicDb
{
    static void Main()
    {
        // 获取数据库路径
        string dbPath = PathHelper.GetLocalFilePath("musicdb.sqlite3");
        Console.WriteLine($"数据库路径: {dbPath}");
        
        // 检查文件是否存在
        if (!File.Exists(dbPath))
        {
            Console.WriteLine("错误: 数据库文件不存在!");
            Console.WriteLine("尝试从processdb目录复制数据库文件...");
            
            string sourceDbPath = "c:\\Users\\pc\\Desktop\\DailyPoetryA\\processdb\\poetrydb.sqlite3";
            if (File.Exists(sourceDbPath))
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
                    File.Copy(sourceDbPath, dbPath, true);
                    Console.WriteLine("成功复制数据库文件");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"复制数据库文件失败: {ex.Message}");
                    return;
                }
            }
            else
            {
                Console.WriteLine("错误: 源数据库文件也不存在");
                return;
            }
        }
        
        // 连接数据库并查询专辑数量
        try
        {
            using var connection = new SQLiteConnection(dbPath);
            connection.Open();
            
            // 检查表是否存在
            var command = connection.CreateCommand("SELECT name FROM sqlite_master WHERE type='table' AND name='Albums'");
            var result = command.ExecuteScalar();
            
            if (result == null)
            {
                Console.WriteLine("错误: Albums表不存在");
                return;
            }
            
            // 查询专辑数量
            command.CommandText = "SELECT COUNT(*) FROM Albums";
            int albumCount = Convert.ToInt32(command.ExecuteScalar());
            Console.WriteLine($"数据库中专辑数量: {albumCount}");
            
            // 如果有专辑，显示详细信息
            if (albumCount > 0)
            {
                Console.WriteLine("\n专辑列表:");
                command.CommandText = "SELECT Id, Name, Artist FROM Albums";
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine($"ID: {reader["Id"]}, Name: {reader["Name"]}, Artist: {reader["Artist"]}");
                }
            }
            
            // 查询Songs表
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Songs'";
            result = command.ExecuteScalar();
            if (result != null)
            {
                command.CommandText = "SELECT COUNT(*) FROM Songs";
                int songCount = Convert.ToInt32(command.ExecuteScalar());
                Console.WriteLine($"数据库中歌曲数量: {songCount}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"查询数据库失败: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}