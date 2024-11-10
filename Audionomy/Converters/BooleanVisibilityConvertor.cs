namespace Audionomy.Converters
{
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows;

    public class BooleanVisibilityConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {

                return boolValue ? Visibility.Visible : Visibility.Collapsed;

            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
