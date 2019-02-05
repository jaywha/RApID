using RApID_Project_WPF.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace RApID_Project_WPF.UserControls.Converters
{
    [ValueConversion(typeof(Themes),typeof(Brush))]
    public class ThemeToBackground : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[1] == DependencyProperty.UnsetValue)
            {
                Console.WriteLine($"Unset Value again... maybe ? -> {values[1]} | {(Themes)values[1]}!");
                return ((Grid)values[0]).Background;
            }

            switch ((Themes) values[1])
            {
                case Themes.Dark:
                    return SimpleThemeManager.DarkThemeBackground;
                case Themes.Light:
                    return SimpleThemeManager.LightThemeBackground;
                case Themes.Default:
                default:
                    return SimpleThemeManager.DefaultThemeBackground;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
