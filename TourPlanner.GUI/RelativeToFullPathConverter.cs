using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace TourPlanner.GUI
{
    public class RelativeToFullPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => Path.GetFullPath(value as string ?? throw new ArgumentException("Value is not a string.", nameof(value)));

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new InvalidOperationException("Cannot convert absolute path to relative path.");
    }
}
