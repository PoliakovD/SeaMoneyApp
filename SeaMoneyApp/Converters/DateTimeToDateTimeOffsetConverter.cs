using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SeaMoneyApp.Converters;

public class DateTimeToDateTimeOffsetConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            if (value is DateTime dateTime)
            {
                return new DateTimeOffset(dateTime);
            }
        }
        catch (ArgumentOutOfRangeException e)
        {
            return new DateTimeOffset(DateTime.Now);
        }
        
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTimeOffset dateTime)
        {
            return new DateTime(dateTime.Ticks);
        }
        return null;
    }
}