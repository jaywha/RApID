﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

using RApID_Project_WPF.UserControls.Converters;

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
        public static readonly DependencyProperty StaticVarsProperty = DependencyProperty.Register("StaticVars", typeof(StaticVars), typeof(ucUnitIssue));
        #endregion

        #region Fields
        private readonly csObjectHolder.csObjectHolder holder = csObjectHolder.csObjectHolder.ObjectHolderInstance();
        private readonly List<string> SpecialCases = new List<string>() { "Cause", "Replacement", "Item", "Problem" };
        private List<IssueItemProblemCombinations> lIIPC = new List<IssueItemProblemCombinations>();
        private int _id = 0;

        private Binding IsRepairVisibilityBinding = new Binding()
        {
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
        [Description("Is this control in a Repiar type form?"), Category("Common")]
        public bool IsRepairForm
        {
            get => (bool)GetValue(IsRepairFormProperty);
            set
            {
                SetValue(IsRepairFormProperty, value);
                OnPropertyChanged();
            }
        }

        #region UnitIssueModel Props
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
        #endregion

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

        [Description("The parent instance of StaticVars for logging, if available."),Category("Automation")]
        public StaticVars StaticVars
        {
            get => (StaticVars)GetValue(StaticVarsProperty);
            set {
                SetValue(StaticVarsProperty, value);
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

        public int ID {
            get => _id;
            set {
                _id = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Constructors
        public ucUnitIssue()
        {
            InitializeComponent();
            holder.vGetServerName("");
            if (string.IsNullOrEmpty(MonkeyCache.FileStore.Barrel.ApplicationId)) MonkeyCache.FileStore.Barrel.ApplicationId = AppDomain.CurrentDomain.FriendlyName;
            DataContext = this;
            dgMultipleParts.dgBuildView(DataGridTypes.MULTIPLEPARTS);
        }

        public ucUnitIssue(int issueNum)
        {
            InitializeComponent();
            holder.vGetServerName("");
            if (string.IsNullOrEmpty(MonkeyCache.FileStore.Barrel.ApplicationId)) MonkeyCache.FileStore.Barrel.ApplicationId = AppDomain.CurrentDomain.FriendlyName;

            stkMain.Name += issueNum;

            foreach (FrameworkElement u in stkMain.Children)
            {
                if (string.IsNullOrEmpty(u.Name)) continue;
                else u.Name += issueNum;
            }
        }
        #endregion

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
            return Dispatcher.Invoke(() =>
            {
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
                                VerticalAlignment = txtbx.VerticalAlignment
                            }; cmbx.DropDownClosed += ComboBox_DropDownClosed;

                            cmbx.SetBinding(ComboBox.TextProperty, txtbx.GetBindingExpression(TextBox.TextProperty).ParentBinding);
                            if (GetConvParam(txtbx) is Binding b) cmbx.SetBinding(VisibilityProperty, b);

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
            return Dispatcher.Invoke(() =>
            {
                var controls = new UIElement[stkMain.Children.Count];
                stkMain.Children.Cast<UIElement>().ToList().CopyTo(controls, 0);

                try
                {
                    foreach (var control in controls)
                    {
                        if (control is ComboBox cmbx && cmbx.Name.StartsWith("txt"))
                        {
                            var txtbx = new TextBox()
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
            switch (cmbx.Name.Replace("txt", ""))
            {
                case "ReportedIssue":
                    if (!IsRepairForm) cmbx.PullItemsFromQuery("SELECT [ReportedIssue] FROM RApID_DropDowns", holder.RepairConnectionString);
                    else cmbx.PullItemsFromQuery("SELECT [PC1] FROM JDECodes", holder.RepairConnectionString);
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
        private void cmbxRefDesignator_SelectionChanged(object sender, SelectionChangedEventArgs e) => cmbxPartNumber.SelectedIndex = cmbxRefDesignator.SelectedIndex;

        #region Issue Item and Problem Section

        private void fillItemCB(ComboBox cbIssueEdit, ComboBox cbItemEdit, ComboBox cbProblemEdit)
        {
            var lTempFindings = new List<IssueItemProblemCombinations>();

            if (cbIssueEdit.Text.ToLower().Equals("no trouble found")) { return; }

            for (int i = 0; i < lIIPC.Count; i++)
            {
                if (cbIssueEdit.Text.ToString().Equals(lIIPC[i].Issue))
                    lTempFindings.Add(lIIPC[i]);
            }

            for (int i = 0; i < lTempFindings.Count; i++)
            {
                if (!cbItemEdit.Items.Contains(lTempFindings[i].Item) && !string.IsNullOrEmpty(lTempFindings[i].Item))
                    cbItemEdit.Items.Add(lTempFindings[i].Item);
            }

            if (cbItemEdit.Items.Count < 1)
                fillProblemCB(lTempFindings, cbProblemEdit);
            else cbItemEdit.IsEnabled = true;
        }

        private void fillProblemCB(ComboBox cbIssueEdit, ComboBox cbItemEdit, ComboBox cbProblemEdit)
        {
            var lTempFindings = new List<IssueItemProblemCombinations>();

            for (int i = 0; i < lIIPC.Count; i++)
            {
                if (cbIssueEdit.Text.ToString().Equals(lIIPC[i].Issue) && cbItemEdit.Text.ToString().Equals(lIIPC[i].Item))
                    lTempFindings.Add(lIIPC[i]);
            }

            for (int i = 0; i < lTempFindings.Count; i++)
            {
                if (!cbProblemEdit.Items.Contains(lTempFindings[i].Problem))
                    cbProblemEdit.Items.Add(lTempFindings[i].Problem);
            }

            cbProblemEdit.IsEnabled = true;
        }

        private void fillProblemCB(List<IssueItemProblemCombinations> lNarrowedDownList, ComboBox cbProblemEdit)
        {
            //if (!cbProblemEdit.Name.Contains("_"))
            resetIIPItems(false, txtItem, txtProblem, cmbxRefDesignator, lblRefDes, cmbxPartNumber, lblPartReplaced, btnAddPartsReplaced, dgMultipleParts, null);
            /*else if (cbProblemEdit.Name.EndsWith("2"))
                resetIIPItems(false, unitIssue.txtItem, unitIssue.txtProblem, unitIssue.cmbxRefDesignator, null, unitIssue.cmbxPartNumber, null, unitIssue.btnAddPartsReplaced, unitIssue.dgMultipleParts, null);
            else if (cbProblemEdit.Name.EndsWith("3"))
                resetIIPItems(false, unitIssue.txtItem, unitIssue.txtProblem, unitIssue.cmbxRefDesignator, null, unitIssue.cmbxPartNumber, null, unitIssue.btnAddPartsReplaced, unitIssue.dgMultipleParts, null);*/

            for (int i = 0; i < lNarrowedDownList.Count; i++)
            {
                if (!cbProblemEdit.Items.Contains(lNarrowedDownList[i].Problem))
                    cbProblemEdit.Items.Add(lNarrowedDownList[i].Problem);
            }

            cbProblemEdit.IsEnabled = true;
        }

        private void resetIIPItems(bool bResetAll, TextBox cbItemReset, TextBox cbProblemReset, Control txtRefReset, Label lblRefReset, Control txtPartReset, Label lblPartReset, Button btnAddReset, DataGrid dgReset, Border borderError)
        {
            if (bResetAll)
            {
                cbItemReset.IsEnabled = false;
                txtRefReset.SetContent(string.Empty);
                lblRefReset.Visibility = Visibility.Hidden;
                txtRefReset.Visibility = Visibility.Hidden;
                borderError.Visibility = Visibility.Hidden;
            }

            cbProblemReset.IsEnabled = false;
            txtPartReset.SetContent(string.Empty);
            dgReset.Items.Clear();
            lblPartReset.Visibility = Visibility.Hidden;
            txtPartReset.Visibility = Visibility.Hidden;
            btnAddReset.Visibility = Visibility.Collapsed;
            dgReset.Visibility = Visibility.Collapsed;
        }

        private void dispIIPElements(Label lblRefToDisp, Control txtRefToDisp, Label lblPartToDisp, Control txtPartToDisp, ComboBox cbItemToCompare, ComboBox cbProblemToCompare, DataGrid dgToEdit, Button btnAddToDG, Border borderError)
        {
            bool bDispAll = false;

            if (cbItemToCompare.Text.Equals("Ref Designator Code"))
            {
                borderError.Visibility = Visibility.Visible;
                lblRefToDisp.Visibility = Visibility.Visible;
                txtRefToDisp.Visibility = Visibility.Visible;
                bDispAll = true;
            }
            else
            {
                borderError.Visibility = Visibility.Hidden;
                lblRefToDisp.Visibility = Visibility.Hidden;
                txtRefToDisp.Visibility = Visibility.Hidden;
            }

            if (cbProblemToCompare.Text.Equals("Part Number"))
            {
                lblPartToDisp.Visibility = Visibility.Visible;
                txtPartToDisp.Visibility = Visibility.Visible;
                bDispAll = true;
            }
            else
            {
                lblPartToDisp.Visibility = Visibility.Hidden;
                txtPartToDisp.Visibility = Visibility.Hidden;
            }

            if (bDispAll)
            {
                btnAddToDG.Visibility = Visibility.Visible;
                dgToEdit.Visibility = Visibility.Visible;
            }
        }

        #endregion
        #endregion

        #region Conversion Operators
        /// <summary>
        /// Allows creating a <see cref="ucUnitIssue"/> from a given <see cref="UnitIssueModel"/>.
        /// </summary>
        /// <param name="model">The given <see cref="UnitIssueModel"/> instance.</param>
        public static implicit operator ucUnitIssue(UnitIssueModel model)
        {
            var r = new ucUnitIssue()
            {
                ID = model.ID,
                ReportedIssue = model.ReportedIssue ?? "",
                TestResult = model.TestResult ?? "",
                AbortResult = model.TestResultAbort ?? "",
                Issue = model.Issue ?? "",
                Problem = model.Problem ?? "",
                Cause = model.Cause ?? "",
                Replacement = model.Replacement ?? ""
            };

            if (model.MultiPartsReplaced != null) model.MultiPartsReplaced.ForEach(mpr => r.dgMultipleParts.Items.Add(mpr));
            else if (model.SinglePartReplaced != null) r.dgMultipleParts.Items.Add(model.SinglePartReplaced);

            return r;
        }

        /// <summary>
        /// Allows creating a <see cref="UnitIssueModel"/> from a given <see cref="ucUnitIssue"/>.
        /// </summary>
        /// <param name="unitIssue">The given <see cref="ucUnitIssue"/> UserControl instance.</param>
        public static implicit operator UnitIssueModel(ucUnitIssue unitIssue)
        {
            var r = new UnitIssueModel()
            {
                ID = unitIssue.ID,
                ReportedIssue = unitIssue.ReportedIssue ?? "",
                TestResult = unitIssue.TestResult ?? "",
                TestResultAbort = unitIssue.AbortResult ?? "",
                Issue = unitIssue.Issue ?? "",
                Problem = unitIssue.Problem ?? "",
                Cause = unitIssue.Cause ?? "",
                Replacement = unitIssue.Replacement ?? ""
            };

            var partsList = unitIssue.dgMultipleParts.Items.Cast<MultiplePartsReplaced>().ToList();
            if (unitIssue.dgMultipleParts != null) r.MultiPartsReplaced = partsList;
            else if (unitIssue.dgMultipleParts != null) r.SinglePartReplaced = partsList[0];

            return r;
        }

        /// <summary>
        /// Allows casting from <see cref="ucUnitIssue"/> to <see cref="TabItem"/> for use in <see cref="ucIssueTabControl"/>.
        /// </summary>
        /// <param name="unitIssue">The given <see cref="ucUnitIssue"/> UserControl instance.</param>
        public static implicit operator TabItem(ucUnitIssue unitIssue)
        {
            var ti = new TabItem()
            {
                Header = "Unit Issue #ISSUENUMBER",
                Content = unitIssue,
                Width = unitIssue.Width,
                Height = unitIssue.Height
            };

            return ti;
        }
        #endregion

    }
}
