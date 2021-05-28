using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Windows.Data;

namespace TourPlanner.GUI.Converters
{
    internal class PercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string stringValue)
                throw new ArgumentException("Not a valid string value.", nameof(value));

            try
            {
                // throws FormatException for invalid inputs
                var number = double.Parse(stringValue
                    .Replace(culture.NumberFormat.PercentSymbol, String.Empty)
                    .Replace(culture.NumberFormat.PercentDecimalSeparator, culture.NumberFormat.NumberDecimalSeparator));

                return number / 100d;
            }
            catch (FormatException)
            {
                return new ValidationResult($"'{value}' is not a valid percentage.");
            }
        }
    }
}
