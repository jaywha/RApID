using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

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
        public static readonly DependencyProperty DropDownEventProperty = DependencyProperty.Register("DropDownEvent", typeof(EventHandler), typeof(ucUnitIssue));
        public static readonly DependencyProperty IsRepairFormProperty = DependencyProperty.Register("IsRepairForm", typeof(bool), typeof(ucUnitIssue));

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

        #region Fields
        private readonly csObjectHolder.csObjectHolder holder = csObjectHolder.csObjectHolder.ObjectHolderInstance();
        private object @lockDropDownEvent = new object();        
        #endregion

        #region Properties
        [Description("Is this control in a Repiar type form?"),Category("Common")]
        public bool IsRepairForm
        {
            get => (bool)GetValue(IsRepairFormProperty);
            set
            {
                SetValue(IsRepairFormProperty, value);
                OnPropertyChanged();
            }
        }

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
                if (!ReadOnly) MutateToComboBoxes();
                else MutateToTextBoxes();
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

        [Description("Will trigger whenever any Combobox Dropdown is closed.")]
        public event EventHandler DropDownEvent
        {
            add
            {
                lock (@lockDropDownEvent)
                {
                    SetValue(DropDownEventProperty, value);
                    OnPropertyChanged();
                }
            }
            remove
            {
                lock (@lockDropDownEvent)
                {
                    SetValue(DropDownEventProperty, null);
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        public ucUnitIssue()
        {
            InitializeComponent();
            holder.vGetServerName("");
            DataContext = this;
        }

        public ucUnitIssue(int issueNum)
        {
            InitializeComponent();
            holder.vGetServerName("");

            stkMain.Name += issueNum;

            foreach (Control c in stkMain.Children)
            {
                if (string.IsNullOrEmpty(c.Name)) continue;
                else c.Name += issueNum;
            }
        }

        #region Switch Readonly Mode (TextBoxes <-> ComboBoxes)

        public bool MutateToComboBoxes()
        {
            return Dispatcher.Invoke(() => { 
                var controls = new UIElement[stkMain.Children.Count];
                stkMain.Children.Cast<UIElement>().ToList().CopyTo(controls, 0);

                try
                {
                    foreach (var control in controls)
                    {
                        if (control is TextBox txtbx)
                        {
                            var cmbx = new ComboBox()
                            {
                                Name = txtbx.Name,
                                Width = txtbx.Width,
                                HorizontalAlignment = txtbx.HorizontalAlignment,
                                VerticalAlignment = txtbx.VerticalAlignment,
                                Visibility =  txtbx.Visibility
                            }; cmbx.DropDownClosed += ComboBox_DropDownClosed;

                            cmbx.SetBinding(ComboBox.TextProperty, txtbx.GetBindingExpression(TextBox.TextProperty).ParentBinding);

                            ComboBoxFiller(ref cmbx);

                            var currChildIndex = stkMain.Children.IndexOf(txtbx);
                            stkMain.Children.Remove(txtbx);
                            stkMain.Children.Insert(currChildIndex, cmbx);
                        }
                    }
                }
                catch (Exception e)
                {
                    csExceptionLogger.csExceptionLogger.Write("ucUnitIssue_MutateToComboBoxes", e);
                    return false;
                }

                return true;
            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        public bool MutateToTextBoxes()
        {
            return Dispatcher.Invoke(() => {
                var controls = new UIElement[stkMain.Children.Count];
                stkMain.Children.Cast<UIElement>().ToList().CopyTo(controls, 0);

                try
                {
                    foreach (var control in controls)
                    {
                        if (control is ComboBox cmbx)
                        {
                            var txtbx = new TextBox()
                            {
                                Name = cmbx.Name,
                                Width = cmbx.Width,
                                HorizontalAlignment = cmbx.HorizontalAlignment,
                                VerticalAlignment = cmbx.VerticalAlignment,
                                Visibility = cmbx.Visibility
                            };

                            txtbx.SetBinding(TextBox.TextProperty, cmbx.GetBindingExpression(ComboBox.TextProperty).ParentBinding);

                            var currChildIndex = stkMain.Children.IndexOf(cmbx);
                            stkMain.Children.Remove(cmbx);
                            stkMain.Children.Insert(currChildIndex, txtbx);
                        }
                    }
                }
                catch (Exception e)
                {
                    csExceptionLogger.csExceptionLogger.Write("ucUnitIssue_MutateToTextBoxes", e);
                    return false;
                }

                return true;
            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        public void ComboBoxFiller(ref ComboBox cmbx)
        {
            switch(cmbx.Name.Replace("txt",""))
            {
                case "ReportedIssue":
                    if(!IsRepairForm) cmbx.PullItemsFromQuery("SELECT [ReportedIssue] FROM RApID_DropDowns", holder.RepairConnectionString);
                    else              cmbx.PullItemsFromQuery("SELECT [PC1] FROM JDECodes", holder.RepairConnectionString);
                    break;
                case "TestResult":
                    cmbx.PullItemsFromQuery("SELECT [TestResult] FROM RApID_DropDowns", holder.RepairConnectionString);
                    break;
                case "TestResultAbort":
                    cmbx.PullItemsFromQuery("SELECT [TestResult_Abort] FROM RApID_DropDowns", holder.RepairConnectionString);
                    break;
                case "Cause":
                    cmbx.PullItemsFromQuery("SELECT [Cause] FROM RApID_DropDowns", holder.RepairConnectionString);
                    break;
                case "Replacement":
                    cmbx.PullItemsFromQuery("SELECT [Replacement] FROM RApID_DropDowns", holder.RepairConnectionString);
                    break;
                case "Issue":
                    cmbx.PullItemsFromQuery("SELECT DISTINCT [Issue] FROM tblManufacturingTechIssues", holder.RepairConnectionString);
                    break;
                case "Item":
                    cmbx.PullItemsFromQuery("SELECT DISTINCT [Item] FROM tblManufacturingTechIssues", holder.RepairConnectionString);
                    break;
                case "Problem":
                    cmbx.PullItemsFromQuery("SELECT DISTINCT [Problem] FROM tblManufacturingTechIssues", holder.RepairConnectionString);
                    break;
                default:
                    cmbx.Items.Add("<NULLBOXNAME>");
                    break;
            }
        }
        #endregion

        #region UIElement Events
        private void ComboBox_DropDownClosed(object sender, EventArgs e) => ((EventHandler)GetValue(DropDownEventProperty))?.Invoke(sender, e);

        private void btnResetIssueData_Click(object sender, RoutedEventArgs e)
        {
            ReportedIssue = string.Empty;
            TestResult = string.Empty;
            AbortResult = string.Empty;
            Issue = string.Empty;
            Item = string.Empty;
            Problem = string.Empty;
            Cause = string.Empty;
            Replacement = string.Empty;
            dgMultipleParts.Items.Clear();
        }
        #endregion
    }
}
