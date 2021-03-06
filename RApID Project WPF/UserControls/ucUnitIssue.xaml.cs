﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Reflection;

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
        public static readonly DependencyProperty AddPartReplacedProperty = DependencyProperty.Register("AddPartReplaced", typeof(RoutedEventHandler), typeof(ucUnitIssue));

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
        private readonly List<string> SpecialCases = new List<string>() { "Cause", "Replacement", "Item", "Problem" };

        private Binding IsRepairVisibilityBinding = new Binding() {
            ElementName = "uccUnitIssue",
            Path = new PropertyPath("IsRepairForm"),
            Converter = new BoolToVisibilityConverter(),
            ConverterParameter = "Repair",
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        };
        private Binding IsProductionVisibilityBinding = new Binding()
        {
            ElementName = "uccUnitIssue",
            Path = new PropertyPath("IsRepairForm"),
            Converter = new BoolToVisibilityConverter(),
            ConverterParameter = "Produciton",
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged

        };

        private object @lockDropDownEvent = new object();
        private object @lockAddPartReplacedEvent = new object();
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

        [Description("Will trigger when the add part button is pressed.")]
        public event RoutedEventHandler AddPartReplaced
        {
            add
            {
                lock (@lockAddPartReplacedEvent)
                {
                    SetValue(AddPartReplacedProperty, value);
                    OnPropertyChanged();
                }
            }
            remove
            {
                lock (@lockAddPartReplacedEvent)
                {
                    SetValue(AddPartReplacedProperty, null);
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
            dgMultipleParts.dgBuildView(DataGridTypes.MULTIPLEPARTS);
        }

        public ucUnitIssue(int issueNum)
        {
            InitializeComponent();
            holder.vGetServerName("");            

            stkMain.Name += issueNum;

            foreach (FrameworkElement u in stkMain.Children)
            {
                if (string.IsNullOrEmpty(u.Name)) continue;
                else u.Name += issueNum;
            }
        }

        #region Switch Readonly Mode (TextBoxes <-> ComboBoxes)

        private Binding GetConvParam(FrameworkElement bx)
        {
            switch (bx.Name)
            {
                case "txtCause":
                case "txtReplacement":
                    return IsRepairVisibilityBinding;
                case "txtItem":
                case "txtProblem":
                    return IsProductionVisibilityBinding;
                default:
                    return null;
            }
        }

        public bool MutateToComboBoxes()
        {
            return Dispatcher.Invoke(() => {
                UIElement[] controls = new UIElement[stkMain.Children.Count];
                stkMain.Children.Cast<UIElement>().ToList().CopyTo(controls, 0);

                try
                {
                    foreach (UIElement control in controls)
                    {
                        if (control is TextBox txtbx)
                        {
                            ComboBox cmbx = new ComboBox()
                            {
                                Name = txtbx.Name,
                                Width = txtbx.Width,
                                HorizontalAlignment = txtbx.HorizontalAlignment,
                                VerticalAlignment = txtbx.VerticalAlignment
                            }; cmbx.DropDownClosed += ComboBox_DropDownClosed;

                            cmbx.SetBinding(ComboBox.TextProperty, txtbx.GetBindingExpression(TextBox.TextProperty).ParentBinding);
                            if(GetConvParam(txtbx) is Binding b) cmbx.SetBinding(VisibilityProperty, b);

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
                UIElement[] controls = new UIElement[stkMain.Children.Count];
                stkMain.Children.Cast<UIElement>().ToList().CopyTo(controls, 0);

                try
                {
                    foreach (UIElement control in controls)
                    {
                        if (control is ComboBox cmbx && cmbx.Name.StartsWith("txt"))
                        {
                            TextBox txtbx = new TextBox()
                            {
                                Name = cmbx.Name,
                                Width = cmbx.Width,
                                HorizontalAlignment = cmbx.HorizontalAlignment,
                                VerticalAlignment = cmbx.VerticalAlignment
                            };

                            txtbx.SetBinding(TextBox.TextProperty, cmbx.GetBindingExpression(ComboBox.TextProperty).ParentBinding);
                            if (GetConvParam(cmbx) is Binding b) txtbx.SetBinding(VisibilityProperty, b);

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
        private void btnAddPartsReplaced_Click(object sender, RoutedEventArgs e) { ((RoutedEventHandler)GetValue(AddPartReplacedProperty))?.Invoke(sender, e); }

        

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
            cmbxPartNumber.SelectedIndex = -1;
            cmbxRefDesignator.SelectedIndex = -1;
            dgMultipleParts.Items.Clear();
        }
        #endregion

        #region Model Conversion
        /// <summary>
        /// Conversion from the data model to the UserControl.
        /// </summary>
        /// <param name="uim">Source data model. </param>
        public static implicit operator ucUnitIssue(UnitIssueModel uim)
        {
            ucUnitIssue result = new ucUnitIssue() {
                ReportedIssue = uim.ReportedIssue,
                TestResult = uim.TestResult,
                AbortResult = uim.TestResultAbort,
                Cause = uim.Cause,
                Replacement = uim.Replacement,
                Issue = uim.Issue,
                Item = uim.Item,
                Problem = uim.Problem
            };

            if(uim.MultiPartsReplaced != null && uim.MultiPartsReplaced.Count > 0)
            {
                foreach(MultiplePartsReplaced part in uim.MultiPartsReplaced)
                {
                    result.PartsReplaced.Add(part);
                }
            } else if(uim.SinglePartReplaced != null)
            {
                result.PartsReplaced.Add(uim.SinglePartReplaced);
            }

            return result;
        }

        /// <summary>
        /// Conversion from the UserControl to the data model.
        /// </summary>
        /// <param name="issue">Source UserControl. </param>
        public static implicit operator UnitIssueModel(ucUnitIssue issue)
        {
            UnitIssueModel result = new UnitIssueModel()
            {
                ReportedIssue = issue.ReportedIssue,
                TestResult = issue.TestResult,
                TestResultAbort = issue.AbortResult,
                Cause = issue.Cause,
                Replacement = issue.Replacement,
                Issue = issue.Issue,
                Item = issue.Item,
                Problem = issue.Problem
            };

            if (issue.PartsReplaced.Count == 1)
            {
                foreach (MultiplePartsReplaced part in issue.PartsReplaced)
                {
                    result.MultiPartsReplaced.Add(part);
                }
            }
            else if (issue.PartsReplaced.Count > 1)
            {
                result.SinglePartReplaced = issue.PartsReplaced[0];
            }

            return result;
        }
        #endregion

        private void cmbxRefDesignator_SelectionChanged(object sender, SelectionChangedEventArgs e) => cmbxPartNumber.SelectedIndex = cmbxRefDesignator.SelectedIndex;
    }
}
