using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RApID_Project_WPF.UserControls.Converters
{
    [ValueConversion(typeof(int),typeof(bool))]
    class NumericToBoolConverter : IValueConverter
    {
        private Dictionary<string,int> PastConversions = new Dictionary<string, int>();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (PastConversions.ContainsKey((string)parameter)) PastConversions.Remove((string)parameter);
            PastConversions.Add((string) parameter, (int)value);
            return (int) value > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var ret = PastConversions[(string)parameter];
            PastConversions.Remove((string)parameter);
            return ret;
        }
    }
}
