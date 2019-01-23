using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dragablz;

namespace RApID_Project_WPF.Classes
{
    /// <summary>
    /// Helps in tracking the current index of a given <see cref="DragablzItem"/> in a <see cref="TabablzControl"/>'s <see cref="DataTemplate"/>.
    /// </summary>
    /// <remarks> Comes unedited from https://stackoverflow.com/a/15557180/7476183 </remarks>
    public static class IndexAttachedProperty
    {
        #region TabItemIndex
        public static int GetTabItemIndex(DependencyObject obj) => (int)obj.GetValue(TabItemIndexProperty);

        public static void SetTabItemIndex(DependencyObject obj, int value) => obj.SetValue(TabItemIndexProperty, value);

        public static readonly DependencyProperty TabItemIndexProperty =
            DependencyProperty.RegisterAttached("TabItemIndex", typeof(int), typeof(IndexAttachedProperty),
                                                new PropertyMetadata(1));
        #endregion

        #region TrackTabItemIndex
        public static bool GetTrackTabItemIndex(DependencyObject obj) => (bool)obj.GetValue(TrackTabItemIndexProperty);

        public static void SetTrackTabItemIndex(DependencyObject obj, bool value) => obj.SetValue(TrackTabItemIndexProperty, value);

        public static readonly DependencyProperty TrackTabItemIndexProperty =
            DependencyProperty.RegisterAttached("TrackTabItemIndex", typeof(bool), typeof(IndexAttachedProperty),
                                                new PropertyMetadata(false, TrackTabItemIndexOnPropertyChanged));

        private static void TrackTabItemIndexOnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tabzItem = GetParent(d, p => p is DragablzItem) as DragablzItem;
            if (!(GetParent(d, p => p is TabablzControl) is TabablzControl tabzControl) || tabzItem == null)
                return;
            if (!(bool)e.NewValue)
                return;
            int index = tabzControl.Items.IndexOf(tabzItem.DataContext ?? tabzItem);
            SetTabItemIndex(d, index);
        }
        #endregion

        public static DependencyObject GetParent(DependencyObject item, Func<DependencyObject, bool> condition)
        {
            if (item == null)
                return null;
            return condition(item) ? item : GetParent(VisualTreeHelper.GetParent(item), condition);
        }
    }
}
