﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace RApID_Project_WPF.UserControls
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(parameter != null)
            {
                if (parameter is string s)
                {
                    if (s.Equals("Repair"))
                    {
                        return (bool)value ? Visibility.Visible : Visibility.Collapsed;
                    }
                    return (bool)value ? Visibility.Collapsed : Visibility.Visible;
                }
            } return (bool) value ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (Visibility) value != Visibility.Collapsed;
        }
    }
}
