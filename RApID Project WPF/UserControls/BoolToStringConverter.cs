using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RApID_Project_WPF.UserControls
{
    [ValueConversion(typeof(bool), typeof(string[]))]
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((value as bool? ?? true) ? (parameter as string[])[0] : (parameter as string[])[1]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (parameter as string[])[0].Equals(value as string) ? true : false;
        }
    }
}
