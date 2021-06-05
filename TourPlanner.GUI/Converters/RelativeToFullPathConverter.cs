using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace TourPlanner.GUI.Converters
{
    internal class RelativeToFullPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => String.IsNullOrEmpty(value as string) ? String.Empty : Path.GetFullPath((string)value);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new InvalidOperationException("Cannot convert absolute path to relative path.");
    }
}
