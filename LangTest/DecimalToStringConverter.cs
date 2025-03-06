using System.Globalization;
using System.Windows.Data;

namespace LangTest
{
    [ValueConversion(typeof(decimal), typeof(string))]
    internal class DecimalToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return string.Format(culture, "{0:N2}", value);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
