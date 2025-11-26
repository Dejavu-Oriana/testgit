namespace DailyPoetryA.Library.Services;

public interface IMenuNavigationService {
    void NavigateTo(string view, object? parameter = null);
}

public static class MenuNavigationConstant {
    /// <summary>
    /// 主页视图
    /// </summary>
    public const string HomeView = "HomeView";

    /// <summary>
    /// 专辑墙视图
    /// </summary>
    public const string AlbumView = "AlbumView";
    
    /// <summary>
    /// 收藏视图
    /// </summary>
    public const string FavoriteView = "FavoriteView";

    /// <summary>
    /// 数据概览视图
    /// </summary>
    public const string StatsView = "StatsView";
}