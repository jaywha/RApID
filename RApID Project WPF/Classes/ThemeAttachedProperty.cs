﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace RApID_Project_WPF.Classes
{
    /// <summary> Theme Property to manage in the <see cref="SimpleThemeManager"/> instance. </summary>
    public static class ThemeAttachedProperty
    {
        #region ThemeType
        public static Themes GetThemeType(DependencyObject obj) => (Themes)obj.GetValue(ThemeTypeProperty);
        public static void SetThemeType(DependencyObject obj, Themes value) => obj.SetValue(ThemeTypeProperty, value);
        public static readonly DependencyProperty ThemeTypeProperty =
            DependencyProperty.RegisterAttached("ThemeType", typeof(Themes), typeof(ThemeAttachedProperty),
                new PropertyMetadata(Themes.Default));
        #endregion

        public static DependencyObject GetParent(DependencyObject item, Func<DependencyObject, bool> condition)
        {
            if (item == null)
                return null;
            return condition(item) ? item : GetParent(VisualTreeHelper.GetParent(item), condition);
        }
    }
}
