using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RApID_Project_WPF.UserControls
{
    /// <summary>
    /// Interaction logic for ucIssueTabControl.xaml
    /// </summary>
    public partial class ucIssueTabControl : UserControl
    {
        #region Fields
        public int CurrentNewTabIndex = 1;

        private List<TabItem> _tabItems;
        private TabItem _backupNewTab;
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty DesignWidthProperty = DependencyProperty.Register("DesignWidth", typeof(double), typeof(ucIssueTabControl), new PropertyMetadata(275.0));
        public static readonly DependencyProperty DesignHeightProperty = DependencyProperty.Register("DesignHeight", typeof(double), typeof(ucIssueTabControl), new PropertyMetadata(450.0));
        public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.Register("ReadOnly", typeof(bool), typeof(ucIssueTabControl), new PropertyMetadata(false));
        #endregion

        #region Properties
        [Description("User Control Width"), Category("Design")]
        public double DesignWidth
        {
            get { return (double)GetValue(DesignWidthProperty); }
            set { SetValue(DesignWidthProperty, value); }
        }
        [Description("User Control Height"), Category("Design")]
        public double DesignHeight
        {
            get { return (double)GetValue(DesignHeightProperty); }
            set { SetValue(DesignHeightProperty, value); }
        }
        [Description("Deterimines if users can edit data."), Category("Common")]
        public bool ReadOnly
        {
            get { return (bool)GetValue(ReadOnlyProperty); }
            set { SetValue(ReadOnlyProperty, value); }
        }

        public int Count
        {
            get { return tcTabs.Items.Count; }
        }
        #endregion

        public ucUnitIssue this[int index]
        {
            get {
                if (_tabItems[index].Equals(_backupNewTab)) return null;
                return _tabItems[index].Content as ucUnitIssue;
            }
            private set { _tabItems[index].Content = value; }
        }

        public ucIssueTabControl()
        {
            try
            {
                InitializeComponent();

                _tabItems = new List<TabItem> { tiNewTab };

                AddTabItem();

                tcTabs.DataContext = _tabItems;
                tcTabs.SelectedIndex = 0;
            } catch(Exception e)
            {
                #if DEBUG
                    Console.WriteLine($"UIC_InitError -> [{e.StackTrace}]");
                #else
                    csExceptionLogger.csExceptionLogger.Write("UnitIssueContainer_InitError", e);
                #endif
            }

            _backupNewTab = tiNewTab;
        }

        private TabItem AddTabItem()
        {
            int count = _tabItems.Count;

            var newTab = new TabItem() {
                Header = $"Unit Issue #{count}",
                Name = $"tiUnitIssue{count}",
                Content = new ucUnitIssue(count) { Name = $"otherIssue{count}" },
                HeaderTemplate = tcTabs.FindResource(ReadOnly ? "ROTabHeader" : "TabHeader") as DataTemplate
            };

            _tabItems.Insert(count - 1, newTab);
            return newTab;
        }

        private void tcTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tcTabs.SelectedItem is TabItem tab && tab.Header != null && tab.Equals(tiNewTab))
            {                
                tcTabs.SelectedItem = UpdateTabCollection(tcTabs, AddTabItem);                
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var tabName = (sender as Button).CommandParameter.ToString();

            if ((from i in tcTabs.Items.Cast<TabItem>()
                 where i.Name.Equals(tabName)
                 select i).FirstOrDefault() is TabItem tab)
            {
                if (_tabItems.Count < 3)
                {
                    MessageBox.Show("Can't remove the last tab.");
                }
                else if (MessageBox.Show($"Are you sure you want to remove the {tab.Header.ToString()} tab?", "Remove Tab", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    UpdateTabCollection<TabItem, bool>(tcTabs, _tabItems.Remove, tab);

                    if (!(tcTabs.SelectedItem is TabItem selectedTab) || selectedTab.Equals(tab))
                    {
                        selectedTab = _tabItems[0];
                    }

                    tcTabs.SelectedItem = selectedTab;
                }
            }
        }

        /// <summary>
        /// Performs the <see cref="Func{T, TResult}"/> on the given <see cref="TabControl"/>.
        /// </summary>
        /// <param name="tc">The main tab control of the user control</param>
        /// <param name="operation">The desired operation to run on the collection.</param>
        /// <param name="args">Any needed arguments for the function.</param>
        private TResult UpdateTabCollection<T, TResult>(TabControl tc, Func<T, TResult> operation, params object[] args)
        {
            tc.DataContext = null;            
            var result = (TResult) operation.DynamicInvoke(args);
            if (tiNewTab == null) tiNewTab = _backupNewTab;
            _tabItems.Add(tiNewTab);
            tc.DataContext = _tabItems;

            return result;
        }

        /// <summary>
        /// Performs the <see cref="Func{TResult}"/> on the given <see cref="TabControl"/>.
        /// </summary>
        /// <param name="tc">The main tab control of the user control</param>
        /// <param name="operation">The desired operation to run on the collection.</param>
        private TResult UpdateTabCollection<TResult>(TabControl tc, Func<TResult> operation)
        {
            tc.DataContext = null;
            var result = operation.Invoke();
            if (tiNewTab == null) tiNewTab = _backupNewTab;
            _tabItems.Add(tiNewTab);
            tc.DataContext = _tabItems;

            return result;
        }
    }
}
