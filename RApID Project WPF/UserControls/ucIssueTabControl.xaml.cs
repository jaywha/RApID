using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;

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
            set {
                SetValue(ReadOnlyProperty, value);                
                OnPropertyChanged();
            }
        }
        [Description("Determine what data to present."),Category("Common")]
        public bool IsRepair
        {
            get { return (bool)GetValue(IsRepairProperty); }
            set {
                SetValue(IsRepairProperty, value);
                OnPropertyChanged();
            }
        }

        public ucUnitIssue this[int index]
        {
            get
            {
                return Issues[index].Content as ucUnitIssue;
            }
            private set { Issues[index].Content = value; }
        }

        protected internal List<ucUnitIssue> _issues = new List<ucUnitIssue>();
        public List<ucUnitIssue> Issues
        {
            get => _issues;
            set {
                _issues = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public ucIssueTabControl()
        {
            try
            {
                InitializeComponent();
                Issues.Add(new ucUnitIssue());
            } catch(Exception e)
            {
                #if DEBUG
                    Console.WriteLine($"UIC_InitError -> [{e.StackTrace}]");
                #else
                    csExceptionLogger.csExceptionLogger.Write("UnitIssueContainer_InitError", e);
                #endif
            }
        }

        public ucUnitIssue Last() => Issues.Last();

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ReadOnly) {
                MessageBox.Show("Can't remove tabs in read-only control.");
                return;
            }

            var tabName = (sender as Button).CommandParameter.ToString();

            if ((from i in tcTabs.Items.Cast<TabItem>()
                 where i.Name.Equals(tabName)
                 select i).FirstOrDefault() is TabItem tab)
            {
                if (Issues.Count < 3)
                {
                    MessageBox.Show("Can't remove the last tab.");
                }
                else if (MessageBox.Show($"Are you sure you want to remove the {tab.Header.ToString()} tab?", "Remove Tab", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    UpdateTabCollection<ucUnitIssue, bool>(tcTabs, Issues.Remove, tab);

                    if (!(tcTabs.SelectedItem is ucUnitIssue selectedIssue) || selectedIssue.Equals(tab))
                    {
                        selectedIssue = Issues[0];
                    }

                    tcTabs.SelectedItem = selectedIssue;
                }
            }
        }

        /// <summary>
        /// Performs the <see cref="Func{T, TResult}"/> on the given <see cref="TabControl"/>.
        /// </summary>
        /// <param name="tc">The main tab control of the user control</param>
        /// <param name="operation">The desired operation to run on the collection.</param>
        /// <param name="args">Any needed arguments for the function.</param>
        private TResult UpdateTabCollection<T, TResult>(TabControl tc, Func<T, TResult> operation, params TabItem[] args)
        {
            tc.DataContext = null;            
            var result = (TResult) operation.DynamicInvoke(args);
            tc.DataContext = Issues;

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
            tc.DataContext = Issues;

            return result;
        }
    }
}
