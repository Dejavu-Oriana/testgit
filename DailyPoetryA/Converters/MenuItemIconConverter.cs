using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DailyPoetryA.Converters;

public class MenuItemIconConverter : IValueConverter
{
    public static readonly MenuItemIconConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string name)
        {
            return name switch
            {
                "主页" => "🏠",
                "专辑墙" => "💿",
                "收藏" => "⭐",
                "数据概览" => "📊",
                _ => "📄"
            };
        }
        return "📄";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

