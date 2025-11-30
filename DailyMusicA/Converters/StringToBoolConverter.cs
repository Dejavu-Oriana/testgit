using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DailyMusicA.Converters;

public class StringToBoolConverter : IValueConverter
{
    public static readonly StringToBoolConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            bool result = !string.IsNullOrWhiteSpace(str);
            
            // 如果参数是 "Invert"，则反转结果
            if (parameter is string param && param == "Invert")
            {
                result = !result;
            }
            
            return result;
        }
        
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

