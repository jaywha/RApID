using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RApID_Project_WPF.UserControls.Converters
{
    /// <summary>
    /// Will accept any double to multiply or divide the size. Defaults to 2.0
    /// </summary>
    [ValueConversion(typeof(double),typeof(double))]
    public class HeightToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (double)value / (double)(parameter ?? 2.0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (double) value * (double) (parameter ?? 2.0);
        }
    }
}
