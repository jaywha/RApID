using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RApID_Project_WPF.UserControls
{
    /// <summary>
    /// Interaction logic for ucUnitIssues.xaml
    /// </summary>
    public partial class ucUnitIssue : UserControl, INotifyPropertyChanged
    {
        #region NotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty ReportedIssueProperty = DependencyProperty.Register("ReportedIssue", typeof(string), typeof(ucUnitIssue));
        public static readonly DependencyProperty TestResultProperty = DependencyProperty.Register("TestResult", typeof(string), typeof(ucUnitIssue));
        public static readonly DependencyProperty AbortResultProperty = DependencyProperty.Register("AbortResult", typeof(string), typeof(ucUnitIssue));
        public static readonly DependencyProperty CauseProperty = DependencyProperty.Register("Cause", typeof(string), typeof(ucUnitIssue));
        public static readonly DependencyProperty ReplacementProperty = DependencyProperty.Register("Replacement", typeof(string), typeof(ucUnitIssue));
        public static readonly DependencyProperty IssueProperty = DependencyProperty.Register("Issue", typeof(string), typeof(ucUnitIssue));
        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register("Item", typeof(string), typeof(ucUnitIssue));
        public static readonly DependencyProperty ProblemProperty = DependencyProperty.Register("Problem", typeof(string), typeof(ucUnitIssue));
        public static readonly DependencyProperty PartsReplacedProperty = DependencyProperty.Register("PartsReplaced", typeof(List<MultiplePartsReplaced>), typeof(ucUnitIssue));

        public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.Register("ReadOnly", typeof(bool), typeof(ucUnitIssue), new PropertyMetadata(true));
        public static readonly DependencyProperty LabelColorProperty = DependencyProperty.Register("LabelColor", typeof(Brush), typeof(ucUnitIssue), new PropertyMetadata(Brushes.Black));
        #endregion

        #region Properties
        [Description("Issue reported to tech"), Category("Unit Issue 1")]
        public string ReportedIssue
        {
            get { return (string)GetValue(ReportedIssueProperty); }
            set
            {
                SetValue(ReportedIssueProperty, value);
                OnPropertyChanged();
            }
        }
        [Description("Normally running test result"), Category("Unit Issue 1")]
        public string TestResult
        {
            get => (string)GetValue(TestResultProperty);
            set
            {
                SetValue(TestResultProperty, value);
                OnPropertyChanged();
            }
        }
        [Description("Interrupted test result"), Category("Unit Issue 1")]
        public string AbortResult
        {
            get => (string)GetValue(AbortResultProperty);
            set
            {
                SetValue(AbortResultProperty, value);
                OnPropertyChanged();
            }
        }
        [Description("Probable cause of issue"), Category("Unit Issue 1")]
        public string Cause
        {
            get { return (string)GetValue(CauseProperty); }
            set
            {
                SetValue(CauseProperty, value);
                OnPropertyChanged();
            }
        }
        [Description("Major non-component replacements"), Category("Unit Issue 1")]
        public string Replacement
        {
            get { return (string)GetValue(ReplacementProperty); }
            set
            {
                SetValue(ReplacementProperty, value);
                OnPropertyChanged();
            }
        }
        [Description("Diagnosed issue"), Category("Unit Issue 1")]
        public string Issue
        {
            get => (string)GetValue(IssueProperty);
            set
            {
                SetValue(IssueProperty, value);
                OnPropertyChanged();
            }
        }
        [Description("Source of issue"), Category("Unit Issue 1")]
        public string Item
        {
            get => (string)GetValue(ItemProperty);
            set
            {
                SetValue(ItemProperty, value);
                OnPropertyChanged();
            }
        }
        [Description("Root cause of issue"), Category("Unit Issue 1")]
        public string Problem
        {
            get => (string)GetValue(ProblemProperty);
            set
            {
                SetValue(ProblemProperty, value);
                OnPropertyChanged();
            }
        }
        [Description("List of parts replaced"), Category("Unit Issue 1")]
        public List<MultiplePartsReplaced> PartsReplaced
        {
            get => (List<MultiplePartsReplaced>)GetValue(PartsReplacedProperty);
            set
            {
                SetValue(PartsReplacedProperty, value);
                OnPropertyChanged();
            }
        }

        [Description("Changes textboxes to comboboxes for editing"), Category("Common")]
        public bool ReadOnly
        {
            get { return (bool)GetValue(ReadOnlyProperty); }
            set
            {
                SetValue(ReadOnlyProperty, value);
                OnPropertyChanged();
                if(!ReadOnly) MutateToComboBoxes();
            }
        }

        [Description("Changes the text color of all labels."), Category("Brush")]
        public Brush LabelColor
        {
            get => (Brush)GetValue(LabelColorProperty);
            set
            {
                SetValue(LabelColorProperty, value);
                OnPropertyChanged();
            }
        }
        #endregion

        public ucUnitIssue()
        {
            InitializeComponent();
            DataContext = this;
        }

        public ucUnitIssue(int issueNum)
        {
            InitializeComponent();

            stkMain.Name += issueNum;

            foreach (Control c in stkMain.Children)
            {
                if (string.IsNullOrEmpty(c.Name)) continue;
                else c.Name += issueNum;
            }
        }

        public bool MutateToComboBoxes()
        {
            var controls = stkMain.Children;

            try
            {
                for (int index = 0; index < controls.Count; index++)
                {
                    if (controls[index] is TextBox txtbx)
                    {
                        controls[index] = new ComboBox()
                        {
                            Name = txtbx.Name,
                            Width = 250,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top
                        };

                        (controls[index] as ComboBox).SetBinding(ComboBox.TextProperty,
                            txtbx.GetBindingExpression(TextBox.TextProperty).ParentBinding);
                    }
                }
            } catch(Exception e)
            {
                csExceptionLogger.csExceptionLogger.Write("ucUnitIssue_MutateToComboBoxes", e);
                return false;
            }

            return true;
        }
    }
}
