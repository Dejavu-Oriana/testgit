using SQLite;

namespace DailyPoetryA.Library.Models;

[Table("Songs")]
public class Song
{
    [PrimaryKey]
    public string Id { get; set; } = System.Guid.NewGuid().ToString();
    
    [Column("Title")]
    public string Title { get; set; } = string.Empty;
    
    [Column("Artist")]
    public string Artist { get; set; } = string.Empty;
    
    [Column("AlbumId")]
    public string AlbumId { get; set; } = string.Empty;
    
    [Column("IsFavorite")]
    public bool IsFavorite { get; set; } = false;
    
    [Column("Record")]
    public string Record { get; set; } = string.Empty;
    
    [Ignore]
    public Album? Album { get; set; }
}