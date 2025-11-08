namespace DailyPoetryA.Library.Services;

public interface IContentNavigationService {
    void NavigateTo(string view, object? parameter = null);
}

public static class ContentNavigationConstant {
    public const string TodayDetail = nameof(TodayDetail);
}