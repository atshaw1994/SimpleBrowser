using System;
using System.Globalization;
using System.Windows.Data;

namespace SimpleBrowser
{
    /// <summary>
    /// WPF/Silverlight ValueConverter : return true if Value is greater than Parameter
    /// </summary>
    public class IsGreaterThanConvertor : IValueConverter
    {
        public static readonly IValueConverter Instance = new IsGreaterThanConvertor();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double intValue = (double)value;
            double compareToValue = (double)parameter;

            return intValue > compareToValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}