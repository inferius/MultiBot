using System;
using System.Globalization;
using System.Windows.Data;

namespace FoE.Farmer.Library.Windows.Helpers
{
    [ValueConversion(typeof(bool?), typeof(bool))]
    public class BooleanToTagConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)(value ?? false) ? parameter : Binding.DoNothing;
        }
    }
}
