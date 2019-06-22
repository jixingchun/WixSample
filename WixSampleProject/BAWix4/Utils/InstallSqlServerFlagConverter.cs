using System;
using System.Globalization;
using System.Windows.Data;

namespace CustomBA.Utils
{
    public class InstallSqlServerFlagConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool))

                throw new InvalidOperationException("The target must be a boolean");

            string str = value as string;
            if("1".Equals(str) || "true".Equals(str, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string))

                throw new InvalidOperationException("The target must be a string");

            if((bool)value)
            {
                return "1";
            }
            else
            {
                return "0";
            }

        }
    }
}
