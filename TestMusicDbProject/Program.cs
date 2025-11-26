using System;
using System.IO;
using System.Collections.Generic;
using SQLite;
using DailyPoetryA.Library.Helpers;

namespace TestMusicDbProject
{
    public class AlbumDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public string CoverUrl { get; set; }
        public decimal Price { get; set; }
        public string AddedDate { get; set; }
    }
    
    public class SongDTO
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string AlbumId { get; set; }
        public int IsFavorite { get; set; }
        public string Record { get; set; }
    }
    
    class Program
    {
        static void Main()
        {
            // 获取数据库路径
            string dbPath = PathHelper.GetLocalFilePath("musicdb.sqlite3");
            Console.WriteLine($"数据库路径: {dbPath}");
            
            // 确保数据库目录存在
            string directoryPath = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Console.WriteLine("创建数据库目录成功");
            }
            
            // 如果数据库文件不存在，将在后续操作中自动创建
            if (!File.Exists(dbPath))
            {
                Console.WriteLine("数据库文件不存在，将创建新的数据库文件...");
            }
            
            // 连接数据库并查询专辑数量
            try
            {
                using var connection = new SQLiteConnection(dbPath);
                
                // 检查表是否存在
                var result = connection.ExecuteScalar<string>("SELECT name FROM sqlite_master WHERE type='table' AND name='Albums'");
                
                if (result == null)
                {
                    Console.WriteLine("错误: Albums表不存在");
                    // 创建表，使用完整的表结构定义
                    Console.WriteLine("尝试创建Albums表...");
                    connection.Execute(@"
                        CREATE TABLE IF NOT EXISTS Albums (
                            Id TEXT PRIMARY KEY,
                            Name TEXT NOT NULL,
                            Artist TEXT NOT NULL,
                            CoverUrl TEXT,
                            Price DECIMAL(10,2),
                            AddedDate TEXT
                        )
                    ");
                    Console.WriteLine("Albums表创建成功");
                    
                    Console.WriteLine("尝试创建Songs表...");
                    connection.Execute(@"
                        CREATE TABLE IF NOT EXISTS Songs (
                            Id TEXT PRIMARY KEY,
                            Title TEXT NOT NULL,
                            Artist TEXT NOT NULL,
                            AlbumId TEXT,
                            IsFavorite INTEGER DEFAULT 0,
                            Record TEXT,
                            FOREIGN KEY (AlbumId) REFERENCES Albums (Id)
                        )
                    ");
                    Console.WriteLine("Songs表创建成功");
                }
                
                // 查询专辑数量
                int albumCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Albums");
                Console.WriteLine($"数据库中专辑数量: {albumCount}");
                
                // 如果有专辑，显示详细信息
                if (albumCount > 0)
                {
                    Console.WriteLine("\n专辑列表:");
                    // 使用DTO类查询
                    var albums = connection.Query<AlbumDTO>("SELECT * FROM Albums");
                    foreach (var album in albums)
                    {
                        Console.WriteLine($"ID: {album.Id}, Name: {album.Name}, Artist: {album.Artist}, Price: {album.Price}");
                        
                        // 查询该专辑的歌曲
                        var songs = connection.Query<SongDTO>("SELECT * FROM Songs WHERE AlbumId = ?", album.Id);
                        foreach (var song in songs)
                        {
                            Console.WriteLine($"  - 歌曲: {song.Title}, 艺术家: {song.Artist}, 收藏: {song.IsFavorite == 1}");
                        }
                    }
                }
                else
                {
                    // 插入测试数据
                    Console.WriteLine("\n数据库中没有专辑，尝试插入测试数据...");
                    try
                    {
                        // 创建测试专辑和歌曲数据
                    var album1Id = Guid.NewGuid().ToString();
                    connection.Execute(
                        "INSERT INTO Albums (Id, Name, Artist, CoverUrl, Price, AddedDate) VALUES (?, ?, ?, ?, ?, ?)",
                        album1Id, "唐诗精选", "古代诗人", "cover1.jpg", 99.99, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    
                    var album2Id = Guid.NewGuid().ToString();
                    connection.Execute(
                        "INSERT INTO Albums (Id, Name, Artist, CoverUrl, Price, AddedDate) VALUES (?, ?, ?, ?, ?, ?)",
                        album2Id, "宋词鉴赏", "宋代词人", "cover2.jpg", 88.88, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    
                    // 添加一些歌曲
                    connection.Execute(
                        "INSERT INTO Songs (Id, Title, Artist, AlbumId, IsFavorite, Record) VALUES (?, ?, ?, ?, ?, ?)",
                        Guid.NewGuid().ToString(), "静夜思", "李白", album1Id, 0, "唐代经典诗歌");
                    
                    connection.Execute(
                        "INSERT INTO Songs (Id, Title, Artist, AlbumId, IsFavorite, Record) VALUES (?, ?, ?, ?, ?, ?)",
                        Guid.NewGuid().ToString(), "望庐山瀑布", "李白", album1Id, 1, "山水诗代表作");
                    
                    connection.Execute(
                        "INSERT INTO Songs (Id, Title, Artist, AlbumId, IsFavorite, Record) VALUES (?, ?, ?, ?, ?, ?)",
                        Guid.NewGuid().ToString(), "水调歌头", "苏轼", album2Id, 1, "中秋词经典");
                        
                        Console.WriteLine("成功插入测试专辑和歌曲数据");
                        
                        // 重新查询专辑数量
                        albumCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Albums");
                        Console.WriteLine($"插入后专辑数量: {albumCount}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"插入测试数据失败: {ex.Message}");
                    }
                }
                
                // 查询Songs表
                var songsTableExists = connection.ExecuteScalar<string>("SELECT name FROM sqlite_master WHERE type='table' AND name='Songs'");
                if (songsTableExists != null)
                {
                    int songCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Songs");
                    Console.WriteLine($"数据库中歌曲数量: {songCount}");
                }
                
                Console.WriteLine("\n数据库测试完成！");
                Console.WriteLine("请重新运行DailyPoetryA应用查看专辑墙");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"查询数据库失败: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}