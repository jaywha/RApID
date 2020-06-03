using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RApID_Project_WPF.UserControls
{
    /// <summary>
    /// Will accept bool to negated bool conversions.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class BoolToNotBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(value as bool? ?? true);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(value as bool? ?? false);
        }
    }
}
