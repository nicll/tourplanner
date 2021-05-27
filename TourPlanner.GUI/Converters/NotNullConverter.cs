using System;
using System.Globalization;
using System.Windows.Data;

namespace TourPlanner.GUI.Converters
{
    public class NotNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is not null;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new InvalidOperationException("IsNullConverter can only be used OneWay.");
    }
}
