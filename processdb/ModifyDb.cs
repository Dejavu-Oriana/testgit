using System;
using System.Data.SQLite;
using System.IO;

class ModifyDb
{
    static void Main()
    {
        string dbPath = "C:\\Users\\pc\\Desktop\\DailyPoetryA\\processdb\\poetrydb.sqlite3";
        
        Console.WriteLine($"正在处理数据库文件: {dbPath}");
        
        // 确保processdb目录存在
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
        
        // 删除旧的数据库文件（如果存在）
        if (File.Exists(dbPath))
        {
            Console.WriteLine("删除现有数据库文件...");
            File.Delete(dbPath);
        }
        
        // 创建新的SQLite数据库连接
        using var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
        connection.Open();
        
        // 创建命令对象
        using var command = connection.CreateCommand();
        
        // 创建Albums表
        Console.WriteLine("创建Albums表...");
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Albums (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                Artist TEXT NOT NULL,
                CoverUrl TEXT,
                Price DECIMAL(10,2),
                AddedDate TEXT
            );
        ";
        command.ExecuteNonQuery();
        
        // 创建Songs表
        Console.WriteLine("创建Songs表...");
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Songs (
                Id TEXT PRIMARY KEY,
                Title TEXT NOT NULL,
                Artist TEXT NOT NULL,
                AlbumId TEXT NOT NULL,
                FOREIGN KEY (AlbumId) REFERENCES Albums(Id)
            );
        ";
        command.ExecuteNonQuery();
        
        // 插入示例专辑数据
        Console.WriteLine("插入示例数据...");
        
        // 插入第一个专辑
        string album1Id = Guid.NewGuid().ToString();
        command.CommandText = $@"
            INSERT INTO Albums (Id, Name, Artist, CoverUrl, Price, AddedDate)
            VALUES ('{album1Id}', '古典诗词配乐集', '古典音乐团队', 'cover1.jpg', 99.90, '{DateTime.Now.AddDays(-10).ToString("yyyy-MM-dd HH:mm:ss")}');
        ";
        command.ExecuteNonQuery();
        
        // 插入第一个专辑的歌曲
        command.CommandText = $@"
            INSERT INTO Songs (Id, Title, Artist, AlbumId)
            VALUES 
                ('{Guid.NewGuid()}', '静夜思', '李白', '{album1Id}'),
                ('{Guid.NewGuid()}', '望庐山瀑布', '李白', '{album1Id}'),
                ('{Guid.NewGuid()}', '春望', '杜甫', '{album1Id}');
        ";
        command.ExecuteNonQuery();
        
        // 插入第二个专辑
        string album2Id = Guid.NewGuid().ToString();
        command.CommandText = $@"
            INSERT INTO Albums (Id, Name, Artist, CoverUrl, Price, AddedDate)
            VALUES ('{album2Id}', '现代诗歌精选', '现代音乐家', 'cover2.jpg', 79.90, '{DateTime.Now.AddDays(-5).ToString("yyyy-MM-dd HH:mm:ss")}');
        ";
        command.ExecuteNonQuery();
        
        // 插入第二个专辑的歌曲
        command.CommandText = $@"
            INSERT INTO Songs (Id, Title, Artist, AlbumId)
            VALUES 
                ('{Guid.NewGuid()}', '再别康桥', '徐志摩', '{album2Id}'),
                ('{Guid.NewGuid()}', '雨巷', '戴望舒', '{album2Id}');
        ";
        command.ExecuteNonQuery();
        
        // 验证数据
        Console.WriteLine("验证数据...");
        command.CommandText = "SELECT COUNT(*) FROM Albums";
        int albumCount = Convert.ToInt32(command.ExecuteScalar());
        
        command.CommandText = "SELECT COUNT(*) FROM Songs";
        int songCount = Convert.ToInt32(command.ExecuteScalar());
        
        Console.WriteLine($"成功创建 {albumCount} 个专辑");
        Console.WriteLine($"成功创建 {songCount} 首歌曲");
        Console.WriteLine("数据库修改完成！");
        Console.WriteLine($"音乐数据库已创建在: {dbPath}");
    }
}