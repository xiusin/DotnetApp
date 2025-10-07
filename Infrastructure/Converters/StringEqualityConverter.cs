using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ConfigButtonDisplay.Infrastructure.Converters;

/// <summary>
/// 字符串相等性转换器，用于比较字符串值
/// </summary>
public class StringEqualityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        var stringValue = value.ToString();
        var parameterValue = parameter.ToString();

        return string.Equals(stringValue, parameterValue, StringComparison.OrdinalIgnoreCase);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
