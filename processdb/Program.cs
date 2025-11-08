using SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModifyMusicDb
{
    // 定义与MusicStorage中相同的模型类
    [Table("Albums")]
    public class Album
    {
        [PrimaryKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Column("Name")]
        public string Name { get; set; } = string.Empty;
        
        [Column("Artist")]
        public string Artist { get; set; } = string.Empty;
        
        [Column("CoverUrl")]
        public string CoverUrl { get; set; } = string.Empty;
        
        [Column("Price")]
        public decimal Price { get; set; }
        
        [Column("AddedDate")]
        public DateTime AddedDate { get; set; } = DateTime.Now;
    }

    [Table("Songs")]
    public class Song
    {
        [PrimaryKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Column("Title")]
        public string Title { get; set; } = string.Empty;
        
        [Column("Artist")]
        public string Artist { get; set; } = string.Empty;
        
        [Column("AlbumId")]
        public string AlbumId { get; set; } = string.Empty;
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            string dbPath = "C:\\Users\\pc\\Desktop\\DailyPoetryA\\processdb\\poetrydb.sqlite3";
            
            Console.WriteLine($"正在处理数据库文件: {dbPath}");
            
            // 创建数据库连接
            using var connection = new SQLiteAsyncConnection(dbPath);
            
            // 删除旧表（如果存在）
            Console.WriteLine("删除可能存在的旧表...");
            await connection.DropTableAsync<Album>();
            await connection.DropTableAsync<Song>();
            
            // 创建新表
            Console.WriteLine("创建新表结构...");
            await connection.CreateTableAsync<Album>();
            await connection.CreateTableAsync<Song>();
            
            // 插入示例数据
            Console.WriteLine("插入示例专辑和歌曲数据...");
            
            // 创建第一个专辑
            var album1 = new Album
            {
                Name = "古典诗词配乐集",
                Artist = "古典音乐团队",
                CoverUrl = "cover1.jpg",
                Price = 99.9m,
                AddedDate = DateTime.Now.AddDays(-10)
            };
            await connection.InsertAsync(album1);
            
            // 为第一个专辑添加歌曲
            var songs1 = new List<Song>
            {
                new Song { Title = "静夜思", Artist = "李白", AlbumId = album1.Id },
                new Song { Title = "望庐山瀑布", Artist = "李白", AlbumId = album1.Id },
                new Song { Title = "春望", Artist = "杜甫", AlbumId = album1.Id }
            };
            await connection.InsertAllAsync(songs1);
            
            // 创建第二个专辑
            var album2 = new Album
            {
                Name = "现代诗歌精选",
                Artist = "现代音乐家",
                CoverUrl = "cover2.jpg",
                Price = 79.9m,
                AddedDate = DateTime.Now.AddDays(-5)
            };
            await connection.InsertAsync(album2);
            
            // 为第二个专辑添加歌曲
            var songs2 = new List<Song>
            {
                new Song { Title = "再别康桥", Artist = "徐志摩", AlbumId = album2.Id },
                new Song { Title = "雨巷", Artist = "戴望舒", AlbumId = album2.Id }
            };
            await connection.InsertAllAsync(songs2);
            
            // 验证数据插入
            Console.WriteLine("验证数据插入结果...");
            var albums = await connection.Table<Album>().ToListAsync();
            Console.WriteLine($"成功创建 {albums.Count} 个专辑");
            
            var songs = await connection.Table<Song>().ToListAsync();
            Console.WriteLine($"成功创建 {songs.Count} 首歌曲");
            
            Console.WriteLine("数据库修改完成！");
            Console.WriteLine("现在可以在C:\\Users\\pc\\Desktop\\DailyPoetryA\\processdb\\poetrydb.sqlite3中使用音乐数据库");
        }
    }
}