using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.Register("ReadOnly", typeof(bool), typeof(ucIssueTabControl), new PropertyMetadata(false));
        public static readonly DependencyProperty IsRepairProperty = DependencyProperty.Register("IsRepair", typeof(bool), typeof(ucIssueTabControl), new PropertyMetadata(false));
        #endregion

        #region Properties
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
                return Issues[index];
            }
            private set {
                Issues[index] = value;
            }
        }

        private ObservableCollection<ucUnitIssue> _issues = new ObservableCollection<ucUnitIssue>();
        public ObservableCollection<ucUnitIssue> Issues
        {
            get => _issues;
            set {
                _issues = value;
                OnPropertyChanged();
            }
        }
        #endregion

        private static bool once = true;

        public ucIssueTabControl()
        {
            InitializeComponent();

            try
            { if (once)
                {
                    Issues.CollectionChanged += Issues_CollectionChanged;
                    Issues.Add(new ucUnitIssue());
                    DataContext = this;
                    once = false;
                }
            } catch(Exception e)
            {
                #if DEBUG
                    Console.WriteLine($"UIC_InitError -> [{e.StackTrace}]");
                #else
                    csExceptionLogger.csExceptionLogger.Write("UnitIssueContainer_InitError", e);
                #endif
            }
        }

        private void Issues_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach(ucUnitIssue item in e.NewItems)
            {
                item.Width = tcTabs.Width;
            }
        }

        public ucUnitIssue Last() => Issues.Last();
    }
}
