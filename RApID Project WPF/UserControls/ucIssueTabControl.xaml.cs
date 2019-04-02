using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace RApID_Project_WPF.UserControls
{
    /// <summary>
    /// Interaction logic for ucIssueTabControl.xaml
    /// </summary>
    public partial class ucIssueTabControl : UserControl, INotifyPropertyChanged
    {
        #region NotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        #region Fields
        /// <summary> The current index to place a new tab -- NewTab Button Index </summary>
        public int CurrentNewTabIndex = 1;

        private List<TabItem> _tabItems;
        private TabItem _backupNewTab;
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty DesignWidthProperty = DependencyProperty.Register("DesignWidth", typeof(double), typeof(ucIssueTabControl), new PropertyMetadata(275.0));
        public static readonly DependencyProperty DesignHeightProperty = DependencyProperty.Register("DesignHeight", typeof(double), typeof(ucIssueTabControl), new PropertyMetadata(450.0));
        public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.Register("ReadOnly", typeof(bool), typeof(ucIssueTabControl), new PropertyMetadata(false));
        public static readonly DependencyProperty IsRepairProperty = DependencyProperty.Register("IsRepair", typeof(bool), typeof(ucIssueTabControl), new PropertyMetadata(false));
        #endregion

        #region Properties
        [Description("User Control Width"), Category("Layout")]
        public double DesignWidth
        {
            get { return (double)GetValue(DesignWidthProperty); }
            set
            {
                SetValue(DesignWidthProperty, value);
                OnPropertyChanged();
            }
        }
        [Description("User Control Height"), Category("Layout")]
        public double DesignHeight
        {
            get { return (double)GetValue(DesignHeightProperty); }
            set
            {
                SetValue(DesignHeightProperty, value);
                OnPropertyChanged();
            }
        }
        [Description("Deterimines if users can edit data."), Category("Common")]
        public bool ReadOnly
        {
            get { return (bool)GetValue(ReadOnlyProperty); }
            set
            {
                SetValue(ReadOnlyProperty, value);
                OnPropertyChanged();
            }
        }
        [Description("Determine what data to present."), Category("Common")]
        public bool IsRepair
        {
            get { return (bool)GetValue(IsRepairProperty); }
            set
            {
                SetValue(IsRepairProperty, value);
                OnPropertyChanged();
            }
        }


        public int Count
        {
            get { return tcTabs.Items.Count; }
        }
        #endregion

        /// <summary> Gets the ucUnitIssue at the provided index.</summary>
        /// <remarks>Exposes the underlying collection to be indexed by its implementing class.</remarks>
        /// <param name="index">The provided index.</param>
        /// <returns>The ucUnitIssue in <see cref="_tabItems"/></returns>
        public ucUnitIssue this[int index]
        {
            get
            {
                if (_tabItems.Count == 0 || _tabItems[index].Equals(_backupNewTab)) return null;
                return _tabItems[index].Content as ucUnitIssue;
            }
            private set { _tabItems[index].Content = value; } // Do not remove - causes ghost errors
        }

        /// <summary>
        /// Default ctor - initializes the control
        /// </summary>
        public ucIssueTabControl()
        {
            InitializeComponent();

            try
            {
                _backupNewTab = new TabItem()
                {
                   Name="tiNewTab",
                   Header="+"
                };
                _backupNewTab.SetBinding(VisibilityProperty, new Binding() {
                    Source = this,
                    Path = new PropertyPath(ReadOnlyProperty),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    Converter = new BoolToVisibilityConverter()
                });

                _tabItems = new List<TabItem> { _backupNewTab };

                AddTabItem();

                tcTabs.DataContext = _tabItems;
                tcTabs.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                csExceptionLogger.csExceptionLogger.Write("UnitIssueContainer_InitError", ex);
            }
        }

        private void UccIssueTabControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Loaded events -- ???
        }

        internal void AddTabItem(ucUnitIssue unitIssue, string customHeader = null)
        {
            lock (_tabItems)
            {
                int count = _tabItems.Count;

                var newTab = new TabItem()
                {
                    Header = customHeader ?? $"Unit Issue #{count}",
                    Name = $"tiUnitIssue{count}",
                    Content = unitIssue,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top
                };
                (newTab.Content as ucUnitIssue).HorizontalAlignment = HorizontalAlignment.Center;

                #region Tab Bindings
                (newTab.Content as ucUnitIssue).SetBinding(HeightProperty, new Binding()
                {
                    Source = this,
                    Path = new PropertyPath(DesignHeightProperty),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
                (newTab.Content as ucUnitIssue).SetBinding(WidthProperty, new Binding()
                {
                    Source = this,
                    Path = new PropertyPath(DesignWidthProperty),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
                (newTab.Content as ucUnitIssue).SetBinding(ucUnitIssue.ReadOnlyProperty, new Binding()
                {
                    Source = this,
                    Path = new PropertyPath(ReadOnlyProperty),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
                (newTab.Content as ucUnitIssue).SetBinding(ucUnitIssue.IsRepairFormProperty, new Binding()
                {
                    Source = this,
                    Path = new PropertyPath(IsRepairProperty),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
                #endregion

                newTab.HeaderTemplate = (DataTemplate)tcTabs.FindResource("TabHeader");

                (newTab.Content as ucUnitIssue).btnResetIssueData.Content += $"#{count - 1}";
                (newTab.Content as ucUnitIssue).btnResetIssueData.Click += btnDelete_Click;
                _tabItems.Insert(count - 1, newTab);
            }
        }

        internal (TabItem Tab, int ActualTabIndex) AddTabItem()
        {
            lock (_tabItems)
            {
                int count = _tabItems.Count;

                var newTab = new TabItem()
                {
                    Header = $"Unit Issue #{count}",
                    Name = $"tiUnitIssue{count}",
                    Content = new ucUnitIssue(count)
                    {
                        Name = $"otherIssue{count}",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Top
                    },
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top
                };

                #region Tab Bindings
                (newTab.Content as ucUnitIssue).SetBinding(HeightProperty, new Binding()
                {
                    Source = this,
                    Path = new PropertyPath(DesignHeightProperty),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
                (newTab.Content as ucUnitIssue).SetBinding(WidthProperty, new Binding()
                {
                    Source = this,
                    Path = new PropertyPath(DesignWidthProperty),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
                (newTab.Content as ucUnitIssue).SetBinding(ucUnitIssue.ReadOnlyProperty, new Binding()
                {
                    Source = this,
                    Path = new PropertyPath(ReadOnlyProperty),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
                (newTab.Content as ucUnitIssue).SetBinding(ucUnitIssue.IsRepairFormProperty, new Binding()
                {
                    Source = this,
                    Path = new PropertyPath(IsRepairProperty),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
                #endregion

                newTab.HeaderTemplate = (DataTemplate)tcTabs.FindResource("TabHeader");

                (newTab.Content as ucUnitIssue).btnResetIssueData.Content += $"#{count - 1}";
                (newTab.Content as ucUnitIssue).btnResetIssueData.Click += btnDelete_Click;
                _tabItems.Insert(count - 1, newTab);

                return (newTab, count - 1);
            }
        }

        private void tcTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tcTabs.SelectedItem is TabItem tab && tab.Header != null && tab.Header.Equals("+"))
            {
                tcTabs.SelectedItem = UpdateTabCollection(tcTabs, AddTabItem).Tab;
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ReadOnly)
            {
                MessageBox.Show("Can't remove tabs in read-only control.");
                return;
            }

            var tabName = (sender as Button).CommandParameter.ToString();

            if ((from i in _tabItems
                 where i.Name.Equals(tabName)
                 select i).FirstOrDefault() is TabItem tab)
            {
                lock (_tabItems)
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

                        int header_index = 1;
                        foreach (var t in _tabItems)
                        {
                            t.Header
                                = t.Header.ToString().Substring(0, t.Header.ToString().IndexOf("#") + 1)
                                + (t.Header.ToString().StartsWith("Unit Issue") ? header_index++ + "" : "");
                        }

                        _tabItems.Last().Header = "+";
                        tcTabs.SelectedItem = selectedTab;
                    }
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

            var result = (TResult)operation.DynamicInvoke(args);
            if (!_tabItems.Contains(_backupNewTab)) _tabItems.Add(_backupNewTab);

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
            if (!_tabItems.Contains(_backupNewTab)) _tabItems.Add(_backupNewTab);

            tc.DataContext = _tabItems;

            return result;
        }
    }
}
