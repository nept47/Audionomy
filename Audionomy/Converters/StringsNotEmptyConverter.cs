namespace Audionomy.Converters
{
    using System.Globalization;
    using System.Windows.Data;

    public class StringsNotEmptyConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return false;

            // Check if both values are non-null strings and have content
            string? firstString = values[0] as string;
            string? secondString = values[1] as string;

            return !string.IsNullOrEmpty(firstString) && !string.IsNullOrEmpty(secondString);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack is not supported.");
        }
    }
}
