using SQLite;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using Avalonia.Media.Imaging;

namespace DailyMusicA.Library.Models;

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
    
    [Ignore]
    public ObservableCollection<Song> Songs { get; set; } = new ObservableCollection<Song>();
    
    [Ignore]
    public Bitmap? CoverImage { get; set; }
    
}