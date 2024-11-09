namespace Audionomy.Helpers
{
    using System.Collections;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class IsListEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable collection)
            {
                var enumerator = collection.GetEnumerator();
                return enumerator.MoveNext() ? Visibility.Collapsed : Visibility.Visible;

            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
