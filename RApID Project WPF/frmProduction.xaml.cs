using EricStabileLibrary;
using RApID_Project_WPF.Classes;
using SNMapperLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace RApID_Project_WPF
{
    /// <summary>
    /// Interaction logic for frmProduction.xaml
    /// </summary>
    public partial class frmProduction : Window, INotifyPropertyChanged
    {
        #region Variables
        List<IssueItemProblemCombinations> lIIPC = new List<IssueItemProblemCombinations>();

        SerialPort sp;
        DispatcherTimer tSPChecker;
        /// <summary>
        /// Used to track tSPChecker reboot
        /// </summary>
        bool bTimerRebootAttempt = false;

        StaticVars sVar = StaticVars.StaticVarsInstance();

        InitSplash initS = new InitSplash();
        csObjectHolder.csObjectHolder holder = csObjectHolder.csObjectHolder.ObjectHolderInstance();
        public static readonly DependencyProperty BOMFileActiveProperty = DependencyProperty.Register("BOMFileActive", typeof(bool), typeof(frmProduction), new PropertyMetadata(false));
        public bool BOMFileActive
        {
            get => (bool)GetValue(BOMFileActiveProperty);
            set
            {
                SetValue(BOMFileActiveProperty, value);
                OnPropertyChanged();
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public frmProduction()
        {
            InitializeComponent();

            CheckForManual();
        }

        private void CheckForManual() {
            #if DEBUG
                txtMultiPartNum.PrepForManualInput();
                txtMultiPartNum_2.PrepForManualInput();
                txtMultiPartNum_3.PrepForManualInput();
            #endif
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Hide();
            initS.InitSplash1("Initializing Form..."); //--Initialize the Splash Screen.
            buildDG();
            csSplashScreenHelper.ShowText("Loading DataLog...");
            initDataLog();
            csSplashScreenHelper.ShowText("Initial SQL Query...");
            initSQLQuery();
            csSplashScreenHelper.ShowText("Building Serial Port...");
            initSerialPort();
            csSplashScreenHelper.ShowText("Initializing Logging...");
            initLogging();
            txtSerialNumber.Focus();
            GC.Collect();
            csSplashScreenHelper.ShowText("Done!");
            csSplashScreenHelper.Hide();
            this.Activate();
#if DEBUG
            txtSerialNumber.Text = "160815030117";
#endif
            WindowState = WindowState.Maximized;
            Show();
            //tcUnitIssues.SelectionChanged += tcUnitIssues_SelectionChanged;
        }

        #region Initialize Form
        private void buildDG()
        {
            dgMultipleParts.dgBuildView(DataGridTypes.MULTIPLEPARTS);
            dgMultipleParts_2.dgBuildView(DataGridTypes.MULTIPLEPARTS);
            dgMultipleParts_3.dgBuildView(DataGridTypes.MULTIPLEPARTS);
            ucAOITab.dgAOI.dgBuildView(DataGridTypes.AOI);
            ucAOITab.dgDefectCodes.dgBuildView(DataGridTypes.DEFECTCODES);
            dgPrevRepairInfo.dgBuildView(DataGridTypes.PREVREPAIRINFO);
            dgBOMList.dgBuildView(DataGridTypes.MULTIPLEPARTS);
        }

        private void initDataLog()
        {
            txtTechName.Text = Environment.UserName;
            dtpDateReceived.SelectedDate = DateTime.Now;
        }

        private void initSQLQuery()
        {
            #region Temp List for Sorting
            var lReportedIssue = new List<string>();
            var lTestResult = new List<string>();
            var lTestResultAbort = new List<string>();
            var lFromArea = new List<string>();
            var lTechAction1 = new List<string>();
            var lTechAction2 = new List<string>();
            var lTechAction3 = new List<string>();
            #endregion

            string query = "SELECT * FROM RApID_DropDowns;";
            var conn = new SqlConnection(holder.RepairConnectionString);
            var cmd = new SqlCommand(query, conn);
            try
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (!string.IsNullOrEmpty(reader["ReportedIssue"].ToString()))
                            lReportedIssue.Add(reader["ReportedIssue"].ToString());

                        if (!string.IsNullOrEmpty(reader["TestResult"].ToString()))
                            lTestResult.Add(reader["TestResult"].ToString());

                        if (!string.IsNullOrEmpty(reader["TestResult_Abort"].ToString()))
                            lTestResultAbort.Add(reader["TestResult_Abort"].ToString());

                        if (!string.IsNullOrEmpty(reader["FromArea"].ToString()))
                            lFromArea.Add(reader["FromArea"].ToString());

                        if (!string.IsNullOrEmpty(reader["TechAction"].ToString()))
                        {
                            if (Convert.ToBoolean(reader["ShowTechActionInAll"]))
                            {
                                lTechAction1.Add(reader["TechAction"].ToString());
                                lTechAction2.Add(reader["TechAction"].ToString());
                                lTechAction3.Add(reader["TechAction"].ToString());
                            }
                        }
                    }
                }
                conn.Close();

                cbReportedIssue.cbFill(lReportedIssue);
                cbTestResult.cbFill(lTestResult);
                cbTestResultAbort.cbFill(lTestResultAbort);
                cbFromArea.cbFill(lFromArea);
                cbTechAction1.cbFill(lTechAction1);

                cbReportedIssue_2.cbFill(lReportedIssue);
                cbTestResult_2.cbFill(lTestResult);
                cbTestResultAbort_2.cbFill(lTestResultAbort);
                cbTechAction_2.cbFill(lTechAction2);

                cbReportedIssue_3.cbFill(lReportedIssue);
                cbTestResult_3.cbFill(lTestResult);
                cbTestResultAbort_3.cbFill(lTestResultAbort);
                cbTechAction_3.cbFill(lTechAction3);

                buildIIP();
            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();
                MessageBox.Show("Issue initializing the form.\nError Message: " + ex.Message, "initSQLQuery()", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            lReportedIssue = lTestResult = lTestResultAbort = lFromArea = null;
            GC.Collect();
        }

        /// <summary>
        /// Fills the Issue, Item, Problem List
        /// </summary>
        private void buildIIP()
        {
            string query = "SELECT * FROM tblManufacturingTechIssues ORDER BY ID;";
            var conn = new SqlConnection(holder.RepairConnectionString);
            var cmd = new SqlCommand(query, conn);
            try
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var iipc = new IssueItemProblemCombinations
                        {
                            Issue = reader["Issue"].ToString(),
                            Item = reader["Item"].ToString(),
                            Problem = reader["Problem"].ToString()
                        };
                        lIIPC.Add(iipc);
                    }
                }
                conn.Close();

                for (int i = 0; i < lIIPC.Count; i++)
                {
                    if (!cbIssue.Items.Contains(lIIPC[i].Issue))
                        cbIssue.Items.Add(lIIPC[i].Issue);

                    if (!cbIssue_2.Items.Contains(lIIPC[i].Issue))
                        cbIssue_2.Items.Add(lIIPC[i].Issue);

                    if (!cbIssue_3.Items.Contains(lIIPC[i].Issue))
                        cbIssue_3.Items.Add(lIIPC[i].Issue);
                }

            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();
                MessageBox.Show("Issue building IIP.\nError Message: " + ex.Message, "buildIIP()", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void initSerialPort()
        {
            if (SerialPort.GetPortNames().Any(x => x == RDM.ReadFromReg<string>(RDM.DefaultKey, RDM.COMPort)))
            {
                try
                {
                    sp = new SerialPort()
                    {
                        PortName = RDM.ReadFromReg<string>(RDM.DefaultKey, RDM.COMPort),
                        BaudRate = RDM.ReadFromReg<int>(RDM.DefaultKey, RDM.BaudRate),
                        Parity = (Parity)Enum.Parse(typeof(Parity), RDM.ReadFromReg<string>(RDM.DefaultKey, RDM.Parity)),
                        DataBits = RDM.ReadFromReg<int>(RDM.DefaultKey, RDM.DataBits),
                        StopBits = (StopBits)Enum.Parse(typeof(StopBits), RDM.ReadFromReg<string>(RDM.DefaultKey, RDM.StopBits))
                    };
                    sp.DataReceived += new SerialDataReceivedEventHandler(spDataReceived);
                    if (sp != null)
                        sp.Open();
                }
                catch { }
            }
            serialPortStatusUpdate();
        }

        private void initLogging()
        {
            try
            {
                sVar.LogHandler.CheckDirectory(Environment.UserName);
            }
            catch { }
        }
        #endregion

        #region Reset Items
        /// <summary>
        /// Resets the form.
        /// </summary>
        /// <param name="bFullReset">Should I do a full reset?</param>
        private void resetForm(bool bFullReset)
        {
            sVar.resetStaticVars();
            BOMFileActive = false;

            if (bFullReset)
                txtSerialNumber.Text = string.Empty;

            txtPartName.Text = txtPartNumber.Text = txtPartSeries.Text = txtCommSubClass.Text = txtSWVersion.Text = string.Empty;
            dtpDateReceived.SelectedDate = DateTime.Now;
            cbFromArea.SelectedIndex = -1;
            cbxScrap.IsChecked = false;
            dgPrevRepairInfo.Items.Clear();
            rtbAdditionalComments.Document.Blocks.Clear();
            dgBOMList.Items.Clear();
            dgBOMList.Items.Refresh();

            ucEOLTab.lblEOL.Content = "End of Line";
            ucEOLTab.lblPOST.Content = "Post Burn-In";            

            resetUnitIssues();
            ucEOLTab.Reset();
            ucAOITab.Reset();

            txtMultiRefDes.Items.Clear();
            txtMultiRefDes_2.Items.Clear();
            txtMultiRefDes_3.Items.Clear();
            txtMultiPartNum.Items.Clear();
            txtMultiPartNum_2.Items.Clear();
            txtMultiPartNum_3.Items.Clear();

            txtSerialNumber.Focus();
        }

        /// <summary>
        /// Resets all of the unit issues.
        /// </summary>
        private void resetUnitIssues()
        {
            cbReportedIssue.SelectedIndex = cbTestResult.SelectedIndex = cbTestResultAbort.SelectedIndex = cbIssue.SelectedIndex = cbItem.SelectedIndex = cbProblem.SelectedIndex = -1;
            txtMultiRefDes.Text = txtMultiPartNum.Text = string.Empty;
            dgMultipleParts.Items.Clear();
            cbTechAction1.SelectedIndex = -1;

            cbReportedIssue_2.SelectedIndex = cbTestResult_2.SelectedIndex = cbTestResultAbort_2.SelectedIndex = cbIssue_2.SelectedIndex = cbItem_2.SelectedIndex = cbProblem_2.SelectedIndex = -1;
            txtMultiRefDes_2.Text = txtMultiPartNum_2.Text = string.Empty;
            dgMultipleParts_2.Items.Clear();
            cbTechAction_2.SelectedIndex = -1;

            cbReportedIssue_3.SelectedIndex = cbTestResult_3.SelectedIndex = cbTestResultAbort_3.SelectedIndex = cbIssue_3.SelectedIndex = cbItem_3.SelectedIndex = cbProblem_3.SelectedIndex = -1;
            txtMultiRefDes_3.Text = txtMultiPartNum_3.Text = string.Empty;
            dgMultipleParts_3.Items.Clear();
            cbTechAction_3.SelectedIndex = -1;

            cbItem.IsEnabled = cbProblem.IsEnabled = false;
            cbItem_2.IsEnabled = cbProblem_2.IsEnabled = false;
            cbItem_3.IsEnabled = cbProblem_3.IsEnabled = false;

            lblRefDes.Visibility = lblPartNum.Visibility = txtMultiRefDes.Visibility = txtMultiPartNum.Visibility = btnAddRefPart.Visibility = dgMultipleParts.Visibility = System.Windows.Visibility.Hidden;
            lblRefDes_2.Visibility = lblPartNum_2.Visibility = txtMultiRefDes_2.Visibility = txtMultiPartNum_2.Visibility = btnAddRefPart_2.Visibility = dgMultipleParts_2.Visibility = System.Windows.Visibility.Hidden;
            lblRefDes_3.Visibility = lblPartNum_3.Visibility = txtMultiRefDes_3.Visibility = txtMultiPartNum_3.Visibility = btnAddRefPart_3.Visibility = dgMultipleParts_3.Visibility = System.Windows.Visibility.Hidden;

            tiUI2.IsEnabled = tiUI3.IsEnabled = false;
            tiUI1.IsSelected = true;
        }

        /// <summary>
        /// Resets a specific unit issue tab.
        /// </summary>
        /// <param name="iUIReset">Which tab do I reset?</param>
        private void resetUnitIssues(int iUIReset)
        {
            switch (iUIReset)
            {
                case 1:
                    cbReportedIssue.SelectedIndex = cbTestResult.SelectedIndex = cbTestResultAbort.SelectedIndex = cbIssue.SelectedIndex = cbItem.SelectedIndex = cbProblem.SelectedIndex = -1;
                    txtMultiRefDes.Text = txtMultiPartNum.Text = string.Empty;
                    dgMultipleParts.Items.Clear();
                    /*txtMultiRefDes.Items.Clear();
                    txtMultiPartNum.Items.Clear();*/
                    cbItem.IsEnabled = cbProblem.IsEnabled = false;
                    lblRefDes.Visibility = lblPartNum.Visibility = txtMultiRefDes.Visibility = txtMultiPartNum.Visibility = btnAddRefPart.Visibility = dgMultipleParts.Visibility = Visibility.Hidden;
                    brdRefDes.BorderThickness = new Thickness(0.0);
                    break;
                case 2:
                    cbReportedIssue_2.SelectedIndex = cbTestResult_2.SelectedIndex = cbTestResultAbort_2.SelectedIndex = cbIssue_2.SelectedIndex = cbItem_2.SelectedIndex = cbProblem_2.SelectedIndex = -1;
                    txtMultiRefDes_2.Text = txtMultiPartNum_2.Text = string.Empty;
                    dgMultipleParts_2.Items.Clear();
                    /*txtMultiRefDes_2.Items.Clear();
                    txtMultiPartNum_2.Items.Clear();*/
                    cbItem_2.IsEnabled = cbProblem_2.IsEnabled = false;
                    lblRefDes_2.Visibility = lblPartNum_2.Visibility = txtMultiRefDes_2.Visibility = txtMultiPartNum_2.Visibility = btnAddRefPart_2.Visibility = dgMultipleParts_2.Visibility = Visibility.Hidden;
                    brdRefDes_2.BorderThickness = new Thickness(0.0);
                    break;
                case 3:
                    cbReportedIssue_3.SelectedIndex = cbTestResult_3.SelectedIndex = cbTestResultAbort_3.SelectedIndex = cbIssue_3.SelectedIndex = cbItem_3.SelectedIndex = cbProblem_3.SelectedIndex = -1;
                    txtMultiRefDes_3.Text = txtMultiPartNum_3.Text = string.Empty;
                    dgMultipleParts_3.Items.Clear();
                    /*txtMultiRefDes_3.Items.Clear();
                    txtMultiPartNum_3.Items.Clear();*/
                    cbItem_3.IsEnabled = cbProblem_3.IsEnabled = false;
                    lblRefDes_3.Visibility = lblPartNum_3.Visibility = txtMultiRefDes_3.Visibility = txtMultiPartNum_3.Visibility = btnAddRefPart_3.Visibility = dgMultipleParts_3.Visibility = Visibility.Hidden;
                    brdRefDes_3.BorderThickness = new Thickness(0.0);
                    break;
            }
        }

        /// <summary>
        /// Check to see if a Unit Issue tab meets the criteria to be disabled.
        /// </summary>
        private void checkToDisableUITabs()
        {
            bool bUI3DataFound = false;
            bool bUI2DataFound = false;
            bool bUI1DataFound = false;

            foreach (UIElement uie in gridUI3.Children)
            {
                if (uie.GetType().Name.Equals("ComboBox"))
                {
                    var cb = (ComboBox)uie;
                    if (!string.IsNullOrEmpty(cb.Text))
                    {
                        bUI3DataFound = true;
                    }
                }
            }

            foreach (UIElement uie in gridUI2.Children)
            {
                if (uie.GetType().Name.Equals("ComboBox"))
                {
                    var cb = (ComboBox)uie;
                    if (!string.IsNullOrEmpty(cb.Text))
                    {
                        bUI2DataFound = true;
                    }
                }
            }

            foreach (UIElement uie in gridUI1.Children)
            {
                if (uie.GetType().Name.Equals("ComboBox"))
                {
                    var cb = (ComboBox)uie;
                    if (!string.IsNullOrEmpty(cb.Text))
                    {
                        bUI1DataFound = true;
                    }
                }
            }

            if (!bUI3DataFound && !bUI2DataFound && tiUI3.IsEnabled)
            {
                resetUnitIssues(3);
                tiUI3.IsEnabled = false;
            }

            if (!bUI2DataFound && !bUI1DataFound && tiUI2.IsEnabled)
            {
                resetUnitIssues(2);
                tiUI2.IsEnabled = false;
            }

            if (!tiUI3.IsEnabled && !tiUI2.IsEnabled)
                tiUI1.IsSelected = true;
        }

        /// <summary>
        /// Check to see if a unit issue tab has any entered data.
        /// </summary>
        /// <param name="iUITab">Which tab do I check?</param>
        /// <returns>True/False based on if data is found or not.</returns>
        private bool checkForUITabData(int iUITab)
        {
            Grid gToCheck = null;

            if (iUITab == 1) gToCheck = gridUI1;
            else if (iUITab == 2) gToCheck = gridUI2;
            else if (iUITab == 3) gToCheck = gridUI3;

            if (gToCheck != null)
            {
                foreach (UIElement uie in gToCheck.Children)
                {
                    if (uie.GetType().Name.Equals("ComboBox"))
                    {
                        var cb = (ComboBox)uie;
                        if (!string.IsNullOrEmpty(cb.Text) && !cb.Name.StartsWith("cbReportedIssue"))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        #endregion

        /// <summary>
        /// Update the toolbar with the Serial Port Information.
        /// </summary>
        private void serialPortStatusUpdate()
        {
            if (sp != null && sp.IsOpen)
            {
                tbPortStatus.Content = "Port Status: Connected";
                tbPortName.Content = "Port Name: " + sp.PortName;
                tbBaudRate.Content = "Baud Rate: " + sp.BaudRate.ToString();
                tbParity.Content = "Parity: " + sp.Parity.ToString();
                tbDataBits.Content = "Data Bits: " + sp.DataBits.ToString();
                tbStopBits.Content = "Stop Bits: " + sp.StopBits.ToString();

                btnRebootSP.Visibility = Visibility.Visible;

                if (tSPChecker != null)
                {
                    if (tSPChecker.IsEnabled)
                        tSPChecker.IsEnabled = false;
                    tSPChecker = null;
                }

                tSPChecker = new DispatcherTimer();
                tSPChecker.Tick += new EventHandler(tSPChecker_Tick);
                tSPChecker.Interval = new TimeSpan(0, 0, 30);
                if (!tSPChecker.Dispatcher.HasShutdownStarted) tSPChecker.Start();
            }
            else if (sp != null && !sp.IsOpen)
            {
                tbPortStatus.Content = "Port Status: Created/Not Connected";
                tbPortName.Content = tbBaudRate.Content = tbParity.Content = tbDataBits.Content = tbStopBits.Content = string.Empty;
                btnRebootSP.Visibility = Visibility.Hidden;
            }
            else if (sp == null)
            {
                tbPortStatus.Content = "Port Status: Does Not Exist";
                tbPortName.Content = tbBaudRate.Content = tbParity.Content = tbDataBits.Content = tbStopBits.Content = string.Empty;
                btnRebootSP.Visibility = Visibility.Hidden;
            }
        }

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
            if (!cbProblemEdit.Name.Contains("_"))
                resetIIPItems(false, cbItem, cbProblem, txtMultiRefDes, lblRefDes, txtMultiPartNum, lblPartNum, btnAddRefPart, dgMultipleParts, brdRefDes);
            else if (cbProblemEdit.Name.EndsWith("2"))
                resetIIPItems(false, cbItem_2, cbProblem_2, txtMultiRefDes_2, lblRefDes_2, txtMultiPartNum_2, lblPartNum_2, btnAddRefPart_2, dgMultipleParts_2, brdRefDes_2);
            else if (cbProblemEdit.Name.EndsWith("3"))
                resetIIPItems(false, cbItem_3, cbProblem_3, txtMultiRefDes_3, lblRefDes_3, txtMultiPartNum_3, lblPartNum_3, btnAddRefPart_3, dgMultipleParts_3, brdRefDes_3);

            for (int i = 0; i < lNarrowedDownList.Count; i++)
            {
                if (!cbProblemEdit.Items.Contains(lNarrowedDownList[i].Problem))
                    cbProblemEdit.Items.Add(lNarrowedDownList[i].Problem);
            }

            cbProblemEdit.IsEnabled = true;
        }

        private void resetIIPItems(bool bResetAll, ComboBox cbItemReset, ComboBox cbProblemReset, Control txtRefReset, Label lblRefReset, Control txtPartReset, Label lblPartReset, Button btnAddReset, DataGrid dgReset, Border borderError)
        {
            if (bResetAll)
            {
                cbItemReset.Items.Clear();
                cbItemReset.SelectedIndex = -1;
                cbItemReset.IsEnabled = false;
                txtRefReset.SetContent(string.Empty);
                lblRefReset.Visibility = Visibility.Hidden;
                txtRefReset.Visibility = Visibility.Hidden;
                borderError.Visibility = Visibility.Hidden;
            }

            cbProblemReset.Items.Clear();
            cbProblemReset.SelectedIndex = -1;
            cbProblemReset.IsEnabled = false;
            txtPartReset.SetContent(string.Empty);
            dgReset.Items.Clear();
            lblPartReset.Visibility = Visibility.Hidden;
            txtPartReset.Visibility = Visibility.Hidden;
            btnAddReset.Visibility = Visibility.Hidden;
            dgReset.Visibility = Visibility.Hidden;
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

        private async void MapRefDesToPartNum()
        {
            try
            {
                using (var mapper = csSerialNumberMapper.Instance)
                {
                    await Task.Factory.StartNew(new Action(() => // in new task
                    {
                        Dispatcher.BeginInvoke(new Action(async () => // perform dispatched UI actions
                        {
                            if (!mapper.GetData(txtSerialNumber.Text))
                            {
                                MessageBox.Show("Couldn't find the barcode's entry in the database.\nPlease enter information manually.",
                                        "Soft Error - Database Lookup", MessageBoxButton.OK, MessageBoxImage.Warning);
                                new frmBoardAliases().ShowDialog();
                            }
                            else
                            {
                                var (filename, found) = await mapper.FindFileAsync(".xls");

                                if(!found)
                                {
                                    MessageBox.Show("We got some database info, but we still need help finindg the BOM.",
                                        "Soft Error - BOM Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
                                    frmBoardAliases techForm = new frmBoardAliases(mapper.PartNumber);
                                    techForm.ShowDialog();

                                    if (!techForm.WasEntryFound) {
                                        MessageBox.Show("No BOM Loaded!", "Warning: BOM Missing!", 
                                            MessageBoxButton.OK, MessageBoxImage.Warning);
                                        return;
                                    } else {
                                        filename = techForm.BOMFileName;
                                    }
                                }

                                csCrossClassInteraction.DoExcelOperations(filename, progMapper, dgBOMList,
                                new Tuple<Control, Control>(txtMultiRefDes, txtMultiPartNum),
                                new Tuple<Control, Control>(txtMultiRefDes_2, txtMultiPartNum_2),
                                new Tuple<Control, Control>(txtMultiRefDes_3, txtMultiPartNum_3));

                                csCrossClassInteraction.MapperSuccessMessage(filename, mapper.PartNumber);

                                BOMFileActive = true;
                                CheckForManual();
                            }
                        }), DispatcherPriority.ApplicationIdle);
                    }));
                }
            }
            catch (InvalidOperationException ioe)
            {
                csExceptionLogger.csExceptionLogger.Write("BadBarcode-MapRefDesToPartNum", ioe);
                return;
            }
        }

        /// <summary>
        /// Begins searching the database for the entered serial number.
        /// </summary>
        private void beginSerialNumberSearch()
        {
            resetForm(false);

            sVar.LogHandler.LogCreation = DateTime.Now;

            if (!string.IsNullOrEmpty(txtSerialNumber.Text))
            {
                sVar.LogHandler.CreateLogAction("**** This is a Production Log ****", csLogging.LogState.NOTE);
                sVar.LogHandler.CreateLogAction("The Serial Number related to this log is: " + txtSerialNumber.Text.TrimEnd(), csLogging.LogState.NOTE);
                fillDataLog();
                ucEOLTab.Fill();
                ucAOITab.Fill();

                MapRefDesToPartNum();
            }
        }

        /// <summary>
        /// Fills the DataLog tab with the info related to the serial number.
        /// </summary>
        private void fillDataLog()
        {
            if (!QueryEOL())
            {
                sVar.LogHandler.CreateLogAction("QueryEOL() was unsuccessful. Attempting QueryProduction() then QueryItemMaster().", csLogging.LogState.NOTE);
                QueryProduction();
            }

            if (!string.IsNullOrEmpty(txtPartNumber.Text))
            {
                sVar.LogHandler.CreateLogAction("Attempting to get the Part Series now.", csLogging.LogState.NOTE);

                if (string.IsNullOrEmpty(txtPartSeries.Text)) { txtPartSeries.Text = csCrossClassInteraction.SeriesQuery(txtPartNumber.Text); }
                if (!string.IsNullOrEmpty(txtPartSeries.Text)) { sVar.LogHandler.CreateLogAction("The Part Series was found. (" + txtPartSeries.Text.ToString() + ")", csLogging.LogState.NOTE); }

                fillCommSubClass();
                fillSoftwareVersion();
            }
            QueryTechReport();
        }

        /// <summary>
        /// Queries the EOL table to see if any EOL information exists.
        /// </summary>
        /// <returns>True/False based on if EOL information exists.</returns>
        private bool QueryEOL()
        {
            try
            {
                string query = "SELECT PartNumber, ModelDesc FROM tblEOL WHERE PCBSerial = '" + txtSerialNumber.Text.ToString().TrimEnd() + "';";
                bool bQueryEOLPassed = csCrossClassInteraction.SNEOLQuery(query, txtPartNumber, txtPartName);

                if (bQueryEOLPassed) { sVar.LogHandler.CreateLogAction("The EOL Query was successful!", csLogging.LogState.NOTE); }

                return bQueryEOLPassed;
            }
            catch (Exception e)
            {
                csExceptionLogger.csExceptionLogger.Write("QueryEOL_SerialNumFail", e);
                return false;
            }
        }

        /// <summary>
        /// Queries the Production table for information related to the serial number.
        /// </summary>
        private void QueryProduction()
        {
            try
            {
                string query = "SELECT Model FROM Production WHERE SerialNum = '" + txtSerialNumber.Text + "';";
                string sProdQueryResults = csCrossClassInteraction.ProductionQuery(query);

                if (sProdQueryResults.ToLower().Contains("rev"))
                {
                    csCrossClassInteraction.LoadPartNumberForm(true, new List<TextBox> { txtPartNumber, txtPartName, txtPartSeries });
                }
                else
                {
                    sVar.LogHandler.CreateLogAction("Part Number '" + sProdQueryResults + "' was found.", csLogging.LogState.NOTE);
                    txtPartNumber.Text = sProdQueryResults;
                    QueryItemMaster();
                }
            }
            catch (Exception e)
            {
                csExceptionLogger.csExceptionLogger.Write("QueryProduction_SerialNumFail", e);
            }
        }

        /// <summary>
        /// Queries the Item Master table for information related to the Part Number.
        /// </summary>
        private void QueryItemMaster()
        {
            try
            {
                string query = "SELECT PartName FROM ItemMaster WHERE PartNumber = '" + txtPartNumber.Text.ToString().TrimEnd() + "';";
                string sItemMasterQueryResults = csCrossClassInteraction.ItemMasterQuery(query);

                if (!string.IsNullOrEmpty(sItemMasterQueryResults))
                {
                    txtPartName.Text = sItemMasterQueryResults;
                    sVar.LogHandler.CreateLogAction("txtPartName's value has been set to " + txtPartName.Text + ".", csLogging.LogState.NOTE);
                }
            }
            catch (Exception e)
            {
                csExceptionLogger.csExceptionLogger.Write("QueryItemMaster_SerialNumFail", e);
            }
        }

        /// <summary>
        /// Queries the ItemMaster table to grab the CommodityClass information related to the Part Number.
        /// </summary>
        private void fillCommSubClass()
        {
            string query = "SELECT CommodityClass FROM ItemMaster WHERE PartNumber = '" + txtPartNumber.Text.TrimEnd() + "';";
            sVar.LogHandler.CreateLogAction("Attempting to fill the Commodity Sub-Class.", csLogging.LogState.NOTE);
            sVar.LogHandler.CreateLogAction("SQL QUERY: " + query, csLogging.LogState.SQLQUERY);
            csCrossClassInteraction.txtFillFromQuery(query, txtCommSubClass);
        }

        /// <summary>
        /// Fills txtSWVersion based on the given Serial Number
        /// </summary>
        private void fillSoftwareVersion()
        {
            string query = $"SELECT TOP(5) SoftwareVersion FROM tblPost WHERE PCBSerial = '{txtSerialNumber.Text.TrimEnd()}' ORDER BY [DateAndTime] DESC";
            sVar.LogHandler.CreateLogAction("Attempting to fill the Software Version.", csLogging.LogState.NOTE);
            csCrossClassInteraction.txtFillFromQuery(query, txtSWVersion);
            txtSWVersion.Text = txtSWVersion.Text.Split(',')[0];
        }

        /// <summary>
        /// First, this queries the tblManufacturingTechReport table for any submission data related to the serial number prior to the release of RApID.
        /// Finally, this queries the TechnicianSubmission table for any submission data related to the serial number after the release of RApID.
        /// </summary>
        private void QueryTechReport()
        {
            ////NOTE: Old DB
            //string query = "SELECT Date_Time, Technician FROM tblManufacturingTechReport WHERE SerialNumber = '" + txtSerialNumber.Text + "';";
            //sVar.LogHandler.CreateLogAction("Querying the Tech Report for previous tech information PRIOR to the release of RApID.", csLogging.LogState.NOTE);
            //sVar.LogHandler.CreateLogAction("SQL QUERY: " + query, csLogging.LogState.SQLQUERY);
            //csCrossClassInteraction.dgTechReport(query, true, dgPrevRepairInfo, txtSerialNumber.Text);

            //NOTE: New DB
            string query = "SELECT DateSubmitted, Technician, ID FROM TechnicianSubmission WHERE SerialNumber = '" + txtSerialNumber.Text + "';";
            sVar.LogHandler.CreateLogAction("Querying the Tech Report for previous tech information AFTER the release of RApID.", csLogging.LogState.NOTE);
            sVar.LogHandler.CreateLogAction("SQL QUERY: " + query, csLogging.LogState.SQLQUERY);
            csCrossClassInteraction.dgTechReport(query, false, dgPrevRepairInfo, txtSerialNumber.Text);
        }

        /// <summary>
        /// On submit, this will verify that there is not a part replaced/ref des in the textbox.
        /// If there is, it will add it to the datagrid after it verifies the part replaced has a part description
        /// </summary>
        /// <returns></returns>
        private bool checkForRefDesPartRep()
        {
            bool bCanSubmit = true;
            string sWarning = "Submission Criteria not met.\n";

            #region Unit Issue 1
            if (!string.IsNullOrEmpty(txtMultiPartNum.Text))
            {
                string _sPRPD = csCrossClassInteraction.GetPartReplacedPartDescription(txtMultiPartNum.Text);

                if (string.IsNullOrEmpty(_sPRPD))
                {
                    sWarning += string.Format("The Part Replaced entered into Unit Issue #1 ( {0} ) does not exist. Please verify the Part Number and try again.\n", txtMultiPartNum.Text);
                    bCanSubmit = false;
                }
                else
                {
                    var mpr = new MultiplePartsReplaced { RefDesignator = txtMultiRefDes.Text, PartReplaced = txtMultiPartNum.Text, PartsReplacedPartDescription = _sPRPD };
                    dgMultipleParts.Items.Add(mpr);
                    txtMultiPartNum.Text = txtMultiRefDes.Text = _sPRPD = string.Empty;
                }
            }
            else if (!string.IsNullOrEmpty(txtMultiRefDes.Text))
            {
                var mpr = new MultiplePartsReplaced { RefDesignator = txtMultiRefDes.Text, PartReplaced = string.Empty, PartsReplacedPartDescription = string.Empty };
                dgMultipleParts.Items.Add(mpr);
                txtMultiRefDes.Text = string.Empty;
            }
            #endregion

            #region Unit Issue 2
            if (!string.IsNullOrEmpty(txtMultiPartNum_2.Text))
            {
                string _sPRPD = csCrossClassInteraction.GetPartReplacedPartDescription(txtMultiPartNum_2.Text);

                if (string.IsNullOrEmpty(_sPRPD))
                {
                    sWarning += string.Format("The Part Replaced entered into Unit Issue #2 ( {0} ) does not exist. Please verify the Part Number and try again.\n", txtMultiPartNum_2.Text);
                    bCanSubmit = false;
                }
                else
                {
                    var mpr = new MultiplePartsReplaced { RefDesignator = txtMultiRefDes_2.Text, PartReplaced = txtMultiPartNum_2.Text, PartsReplacedPartDescription = _sPRPD };
                    dgMultipleParts_2.Items.Add(mpr);
                    txtMultiPartNum_2.Text = txtMultiRefDes_2.Text = _sPRPD = string.Empty;
                }
            }
            else if (!string.IsNullOrEmpty(txtMultiRefDes_2.Text))
            {
                var mpr = new MultiplePartsReplaced { RefDesignator = txtMultiRefDes_2.Text, PartReplaced = string.Empty, PartsReplacedPartDescription = string.Empty };
                dgMultipleParts_2.Items.Add(mpr);
                txtMultiRefDes_2.Text = string.Empty;
            }
            #endregion

            #region Unit Issue 3
            if (!string.IsNullOrEmpty(txtMultiPartNum_3.Text))
            {
                string _sPRPD = csCrossClassInteraction.GetPartReplacedPartDescription(txtMultiPartNum_3.Text);

                if (string.IsNullOrEmpty(_sPRPD))
                {
                    sWarning += string.Format("The Part Replaced entered into Unit Issue #3 ( {0} ) does not exist. Please verify the Part Number and try again.\n", txtMultiPartNum_3.Text);
                    bCanSubmit = false;
                }
                else
                {
                    var mpr = new MultiplePartsReplaced { RefDesignator = txtMultiRefDes_3.Text, PartReplaced = txtMultiPartNum_3.Text, PartsReplacedPartDescription = _sPRPD };
                    dgMultipleParts_3.Items.Add(mpr);
                }
            }
            else if (!string.IsNullOrEmpty(txtMultiRefDes_3.Text))
            {
                var mpr = new MultiplePartsReplaced { RefDesignator = txtMultiRefDes_3.Text, PartReplaced = string.Empty, PartsReplacedPartDescription = string.Empty };
                dgMultipleParts_3.Items.Add(mpr);
                txtMultiRefDes_3.Text = string.Empty;
            }
            #endregion

            if (!bCanSubmit)
            {
                sVar.LogHandler.CreateLogAction(sWarning, csLogging.LogState.WARNING);
                MessageBox.Show(sWarning, "Submission Error", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            return bCanSubmit;
        }

        /// <summary>
        /// Checks to see if the submission criteria has been met.
        /// </summary>
        /// <returns>Returns an ArrayList(2) with: 1.(bool)Has Submission Criteria Been Met, 2.(string)What submission criteria has been missed.</returns>
        private ArrayList canSubmit()
        {
            var alReturn = new ArrayList(2);
            string sErrMsg = "The following items are missing:\n";
            bool bCanSubmit = true;

            if (string.IsNullOrEmpty(txtSerialNumber.Text))
            {
                bCanSubmit = false;
                sErrMsg += "-Serial Number\n";
            }
            if (string.IsNullOrEmpty(txtPartNumber.Text))
            {
                bCanSubmit = false;
                sErrMsg += "-Part Number\n";
            }
            if (string.IsNullOrEmpty(txtPartName.Text))
            {
                bCanSubmit = false;
                sErrMsg += "-Part Name\n";
            }
            if (string.IsNullOrEmpty(txtPartSeries.Text))
            {
                bCanSubmit = false;
                sErrMsg += "-Part Series\n";
            }
            if (string.IsNullOrEmpty(txtCommSubClass.Text))
            {
                bCanSubmit = false;
                sErrMsg += "-Commodity Sub-Class\n";
            }
            if (string.IsNullOrEmpty(cbFromArea.Text))
            {
                bCanSubmit = false;
                sErrMsg += "-From Area\n";
            }

            bool bUnitIssueFound = false;
            if (!string.IsNullOrEmpty(cbReportedIssue.Text))
                bUnitIssueFound = true;
            if (!string.IsNullOrEmpty(cbTestResult.Text))
                bUnitIssueFound = true;
            if (!string.IsNullOrEmpty(cbTestResultAbort.Text))
                bUnitIssueFound = true;
            if (!string.IsNullOrEmpty(cbIssue.Text))
                bUnitIssueFound = true;

            if (!bUnitIssueFound)
            {
                sErrMsg += "-At least one unit issue";
                bCanSubmit = false;
            }

            alReturn.Add(bCanSubmit);
            alReturn.Add(sErrMsg);

            return alReturn;
        }

        /// <summary>
        /// Attempts to submit the data to the TechnicianSubmission table.
        /// </summary>
        /// <returns>True/Falsed based on if the submission was successful.</returns>
        private bool submitData()
        {
            string dtSubmission = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt");
            string sLogString = "Attemping to submit the data with the following parameters to the TechnicianSubmission Table:\n";
            string query = "INSERT INTO TechnicianSubmission " +
                           "(SerialNumber, Technician, DateReceived, PartName, PartNumber, Series, CommoditySubClass, SoftwareVersion, TypeOfReturn, FromArea, Scrap, " +
                           "ReportedIssue, TestResult, TestResultAbort, Issue, Item, Problem, RefDesignator, PartsReplaced, AdditionalComments, " +
                           "DateSubmitted, SubmissionStatus, SaveID, Quantity, TechAct1, TechAct2, TechAct3, LogID) " +
                           "VALUES (@serialNum, @technician, @dateReceived, @partName, @partNumber, @partSeries, @commoditySubClass, @softwareVersion, @typeOfReturn, @fromArea, @scrap, " +
                           "@reportedIssue, @testResult, @testResultAbort, @issue, @item, @problem, @refDesignator, @partsReplaced, @additionalComments, " +
                           "@dateSubmitted, @submissionStatus, @saveID, @quantity, @ta1, @ta2, @ta3, @logID);";

            sVar.LogHandler.CreateLogAction("Attempting to submit the tech data into the TechnicianSubmission Table.", csLogging.LogState.NOTE);
            sVar.LogHandler.CreateLogAction("SQL QUERY: " + query, csLogging.LogState.SQLQUERY);

            var conn = new SqlConnection(holder.RepairConnectionString);
            var cmd = new SqlCommand(query, conn);
            try
            {
                conn.Open();

                cmd.Parameters.AddWithValue("@logID", csSerialization.CurrLogIDToUse);
                cmd.Parameters.AddWithValue("@serialNum", txtSerialNumber.Text.ToString().TrimEnd());
                cmd.Parameters.AddWithValue("@technician", txtTechName.Text.ToString());
                cmd.Parameters.AddWithValue("@dateReceived", dtpDateReceived.SelectedDate.Value.ToString("MM/dd/yyyy"));
                cmd.Parameters.AddWithValue("@partName", txtPartName.Text.TrimEnd());
                cmd.Parameters.AddWithValue("@partNumber", txtPartNumber.Text.TrimEnd());
                cmd.Parameters.AddWithValue("@partSeries", txtPartSeries.Text.TrimEnd());
                cmd.Parameters.AddWithValue("@commoditySubClass", txtCommSubClass.Text.TrimEnd());
                cmd.Parameters.AddWithValue("@softwareVersion", csCrossClassInteraction.EmptyIfNull(txtSWVersion.Text));
                cmd.Parameters.AddWithValue("@typeOfReturn", "Production"); //--Will always be 'Production'
                cmd.Parameters.AddWithValue("@fromArea", csCrossClassInteraction.EmptyIfNull(cbFromArea.Text));
                cmd.Parameters.AddWithValue("@scrap", cbxScrap.IsChecked);

                #region Unit Issues
                UnitIssueModel lUI = getUnitIssueString(0);
                cmd.Parameters.AddWithValue("@reportedIssue", lUI.ReportedIssue);
                cmd.Parameters.AddWithValue("@testResult", csCrossClassInteraction.EmptyIfNull(lUI.TestResult));
                cmd.Parameters.AddWithValue("@testResultAbort", csCrossClassInteraction.EmptyIfNull(lUI.TestResultAbort));
                cmd.Parameters.AddWithValue("@issue", csCrossClassInteraction.EmptyIfNull(lUI.Issue));
                cmd.Parameters.AddWithValue("@item", csCrossClassInteraction.EmptyIfNull(lUI.Item));
                cmd.Parameters.AddWithValue("@problem", csCrossClassInteraction.EmptyIfNull(lUI.Problem));
                cmd.Parameters.AddWithValue("@refDesignator", csCrossClassInteraction.EmptyIfNull(lUI.MultiPartsReplaced[0].RefDesignator));
                cmd.Parameters.AddWithValue("@partsReplaced", csCrossClassInteraction.EmptyIfNull(lUI.MultiPartsReplaced[0].PartReplaced));
                #endregion

                cmd.Parameters.AddWithValue("@additionalComments", new TextRange(rtbAdditionalComments.Document.ContentStart, rtbAdditionalComments.Document.ContentEnd).Text.ToString());

                cmd.Parameters.AddWithValue("@dateSubmitted", dtSubmission);
                cmd.Parameters.AddWithValue("@submissionStatus", "COMPLETE"); //--Will always be 'COMPLETE' for Production
                cmd.Parameters.AddWithValue("@saveID", "-1"); //--Will always be '-1' for Production.
                cmd.Parameters.AddWithValue("@quantity", 1); //--Will always be 1 for Production.

                if (!string.IsNullOrEmpty(cbTechAction1.Text))
                    cmd.Parameters.AddWithValue("@ta1", cbTechAction1.Text);
                else cmd.Parameters.AddWithValue("@ta1", DBNull.Value);

                if (!string.IsNullOrEmpty(cbTechAction_2.Text))
                    cmd.Parameters.AddWithValue("@ta2", cbTechAction1.Text);
                else cmd.Parameters.AddWithValue("@ta2", DBNull.Value);

                if (!string.IsNullOrEmpty(cbTechAction_3.Text))
                    cmd.Parameters.AddWithValue("@ta3", cbTechAction1.Text);
                else cmd.Parameters.AddWithValue("@ta3", DBNull.Value);

                cmd.ExecuteNonQuery();
                conn.Close();

                for (int i = 0; i < cmd.Parameters.Count; i++)
                {
                    if (string.IsNullOrEmpty(cmd.Parameters[i].Value.ToString()))
                        sLogString += cmd.Parameters[i].ToString() + ": N/A\n";
                    else sLogString += cmd.Parameters[i].ToString() + ": " + cmd.Parameters[i].Value.ToString() + "\n";
                }
                sVar.LogHandler.CreateLogAction(sLogString, csLogging.LogState.SUBMISSIONDETAILS);
                sVar.LogHandler.CreateLogAction("Submission Successful!", csLogging.LogState.NOTE);

                int readerID = csCrossClassInteraction.GetDBIDValue("SELECT ID FROM TechnicianSubmission WHERE Technician = '" + txtTechName.Text + "' AND DateSubmitted = '" + dtSubmission + "' AND SerialNumber = '" + txtSerialNumber.Text + "';");
                if (readerID > 0) { submitMultipleUnitData(readerID); }

                return true;
            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();
                string sErr = "There was an error while attempting to submit to the database.\nError Message: " + ex.Message;
                MessageBox.Show(sErr, "submitData()", MessageBoxButton.OK, MessageBoxImage.Error);
                sVar.LogHandler.CreateLogAction(sErr, csLogging.LogState.ERROR);
                return false;
            }
        }

        /// <summary>
        /// Attempts to submit the unit issue data to the TechnicianUnitIssues table.
        /// </summary>
        /// <param name="_id">ID of the submission in the TechnicianSubmissions table</param>
        private void submitMultipleUnitData(int _id)
        {
            string query = "INSERT INTO TechnicianUnitIssues (ID, PartNumber, PartName, CommoditySubClass, ReportedIssue, TestResult, TestResultAbort, Issue, Item, Problem, PartsReplaced, RefDesignator, PartsReplacedPartDescription) " +
                           "VALUES (@id, @partNumber, @partName, @commoditySubClass, @reportedIssue, @testResult, @testResultAbort, @issue, @item, @problem, @partsReplaced, @refDesignator, @prpd);";
            sVar.LogHandler.CreateLogAction("Multiple Unit Issues were found. Attempting to submit to the TechnicianUnitIssues Table.", csLogging.LogState.NOTE);
            sVar.LogHandler.CreateLogAction("SQL QUERY: " + query, csLogging.LogState.SQLQUERY);

            string sLogString = "";
            List<UnitIssueModel> lPI = getUnitIssues();
            var conn = new SqlConnection(holder.RepairConnectionString);
            var cmd = new SqlCommand(query, conn);
            try
            {
                for (int i = 0; i < lPI.Count; i++)
                {
                    if (lPI[i].MultiPartsReplaced.Count == 0)
                    {
                        sLogString = "Attempting to submit the following data to the TechnicianUnitIssues Table:\n";

                        cmd = new SqlCommand(query, conn);
                        conn.Open();

                        cmd.Parameters.AddWithValue("@id", _id);
                        cmd.Parameters.AddWithValue("@partNumber", txtPartNumber.Text);
                        cmd.Parameters.AddWithValue("@partName", txtPartName.Text);
                        cmd.Parameters.AddWithValue("@commoditySubClass", txtCommSubClass.Text);
                        cmd.Parameters.AddWithValue("@reportedIssue", csCrossClassInteraction.unitStripNF(lPI[i].ReportedIssue));
                        cmd.Parameters.AddWithValue("@testResult", csCrossClassInteraction.unitStripNF(lPI[i].TestResult));
                        cmd.Parameters.AddWithValue("@testResultAbort", csCrossClassInteraction.unitStripNF(lPI[i].TestResultAbort));
                        cmd.Parameters.AddWithValue("@issue", csCrossClassInteraction.unitStripNF(lPI[i].Issue));
                        cmd.Parameters.AddWithValue("@item", csCrossClassInteraction.unitStripNF(lPI[i].Item));
                        cmd.Parameters.AddWithValue("@problem", csCrossClassInteraction.unitStripNF(lPI[i].Problem));
                        cmd.Parameters.AddWithValue("@partsReplaced", DBNull.Value);
                        cmd.Parameters.AddWithValue("@refDesignator", DBNull.Value);
                        cmd.Parameters.AddWithValue("@prpd", DBNull.Value);
                        cmd.ExecuteNonQuery();

                        conn.Close();

                        for (int k = 0; k < cmd.Parameters.Count; k++)
                        {
                            if (string.IsNullOrEmpty(cmd.Parameters[k].Value.ToString()))
                                sLogString += cmd.Parameters[k].ToString() + ": N/A\n";
                            else sLogString += cmd.Parameters[k].ToString() + ": " + cmd.Parameters[k].Value.ToString() + "\n";
                        }

                        sVar.LogHandler.CreateLogAction(sLogString, csLogging.LogState.SUBMISSIONDETAILS);
                        sVar.LogHandler.CreateLogAction("Submission Successful!", csLogging.LogState.NOTE);
                    }
                    else
                    {
                        for (int j = 0; j < lPI[i].MultiPartsReplaced.Count; j++)
                        {
                            sLogString = "Attempting to submit the following data to the TechnicianUnitIssues Table:\n";

                            cmd = new SqlCommand(query, conn);
                            conn.Open();

                            cmd.Parameters.AddWithValue("@id", _id);
                            cmd.Parameters.AddWithValue("@partNumber", txtPartNumber.Text);
                            cmd.Parameters.AddWithValue("@partName", txtPartName.Text);
                            cmd.Parameters.AddWithValue("@commoditySubClass", txtCommSubClass.Text);
                            cmd.Parameters.AddWithValue("@reportedIssue", csCrossClassInteraction.unitStripNF(lPI[i].ReportedIssue));
                            cmd.Parameters.AddWithValue("@testResult", csCrossClassInteraction.unitStripNF(lPI[i].TestResult));
                            cmd.Parameters.AddWithValue("@testResultAbort", csCrossClassInteraction.unitStripNF(lPI[i].TestResultAbort));
                            cmd.Parameters.AddWithValue("@issue", csCrossClassInteraction.unitStripNF(lPI[i].Issue));
                            cmd.Parameters.AddWithValue("@item", csCrossClassInteraction.unitStripNF(lPI[i].Item));
                            cmd.Parameters.AddWithValue("@problem", csCrossClassInteraction.unitStripNF(lPI[i].Problem));
                            cmd.Parameters.AddWithValue("@partsReplaced", csCrossClassInteraction.unitStripNF(lPI[i].MultiPartsReplaced[j].PartReplaced));
                            cmd.Parameters.AddWithValue("@refDesignator", csCrossClassInteraction.unitStripNF(lPI[i].MultiPartsReplaced[j].RefDesignator));
                            cmd.Parameters.AddWithValue("@prpd", csCrossClassInteraction.unitStripNF(lPI[i].MultiPartsReplaced[j].PartsReplacedPartDescription));
                            cmd.ExecuteNonQuery();

                            conn.Close();

                            for (int k = 0; k < cmd.Parameters.Count; k++)
                            {
                                if (string.IsNullOrEmpty(cmd.Parameters[k].Value.ToString()))
                                    sLogString += cmd.Parameters[k].ToString() + ": N/A\n";
                                else sLogString += cmd.Parameters[k].ToString() + ": " + cmd.Parameters[k].Value.ToString() + "\n";
                            }

                            sVar.LogHandler.CreateLogAction(sLogString, csLogging.LogState.SUBMISSIONDETAILS);
                            sVar.LogHandler.CreateLogAction("Submission Successful!", csLogging.LogState.NOTE);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();
                string sErr = "Issue submitting the multiple unit issue data.\nError Message: " + ex.Message;
                MessageBox.Show(sErr, "submitMultipleUnitData()", MessageBoxButton.OK, MessageBoxImage.Error);
                sVar.LogHandler.CreateLogAction(sErr, csLogging.LogState.ERROR);
            }
        }

        /// <summary>
        /// To be used when submitting basic information to the TechnicianSubmission table
        /// </summary>
        private UnitIssueModel getUnitIssueString(int iUIData)
        {
            var pmuiReturn = new UnitIssueModel();

            if (tiUI1.IsEnabled && (iUIData == 0 || iUIData == 1)) //-Will always be enabled but doing 'if' to be uniform
            {
                pmuiReturn.ReportedIssue = csCrossClassInteraction.unitIssuesValSubmit(cbReportedIssue);
                pmuiReturn.TestResult += csCrossClassInteraction.unitIssuesValSubmit(cbTestResult);
                pmuiReturn.TestResultAbort += csCrossClassInteraction.unitIssuesValSubmit(cbTestResultAbort);
                pmuiReturn.Issue += csCrossClassInteraction.unitIssuesValSubmit(cbIssue);
                pmuiReturn.Item += csCrossClassInteraction.unitIssuesValSubmit(cbItem);
                pmuiReturn.Problem += csCrossClassInteraction.unitIssuesValSubmit(cbProblem);
            }

            if (tiUI2.IsEnabled && checkForUITabData(2) && (iUIData == 0 || iUIData == 2))
            {
                pmuiReturn.ReportedIssue = csCrossClassInteraction.unitIssuesValSubmit(cbReportedIssue_2);
                pmuiReturn.TestResult += csCrossClassInteraction.unitIssuesValSubmit(cbTestResult_2);
                pmuiReturn.TestResultAbort += csCrossClassInteraction.unitIssuesValSubmit(cbTestResultAbort_2);
                pmuiReturn.Issue += csCrossClassInteraction.unitIssuesValSubmit(cbIssue_2);
                pmuiReturn.Item += csCrossClassInteraction.unitIssuesValSubmit(cbItem_2);
                pmuiReturn.Problem += csCrossClassInteraction.unitIssuesValSubmit(cbProblem_2);
            }

            if (tiUI3.IsEnabled && checkForUITabData(3) && (iUIData == 0 || iUIData == 3))
            {
                pmuiReturn.ReportedIssue = csCrossClassInteraction.unitIssuesValSubmit(cbReportedIssue_3);
                pmuiReturn.TestResult += csCrossClassInteraction.unitIssuesValSubmit(cbTestResult_3);
                pmuiReturn.TestResultAbort += csCrossClassInteraction.unitIssuesValSubmit(cbTestResultAbort_3);
                pmuiReturn.Issue += csCrossClassInteraction.unitIssuesValSubmit(cbIssue_3);
                pmuiReturn.Item += csCrossClassInteraction.unitIssuesValSubmit(cbItem_3);
                pmuiReturn.Problem += csCrossClassInteraction.unitIssuesValSubmit(cbProblem_3);
            }

            pmuiReturn.ReportedIssue = pmuiReturn.ReportedIssue.TrimEnd(new char[] { ',', ' ' });
            pmuiReturn.TestResult = pmuiReturn.TestResult.TrimEnd(new char[] { ',', ' ' });
            pmuiReturn.TestResultAbort = pmuiReturn.TestResultAbort.TrimEnd(new char[] { ',', ' ' });
            pmuiReturn.Issue = pmuiReturn.Issue.TrimEnd(new char[] { ',', ' ' });
            pmuiReturn.Item = pmuiReturn.Item.TrimEnd(new char[] { ',', ' ' });
            pmuiReturn.Problem = pmuiReturn.Problem.TrimEnd(new char[] { ',', ' ' });
            if (iUIData == 0)
                pmuiReturn.MultiPartsReplaced = new List<MultiplePartsReplaced>() { getMPRString() };
            else pmuiReturn.MultiPartsReplaced = getMPRString(iUIData);
            return pmuiReturn;
        }

        /// <summary>
        /// Generates a MultiplePartsReplaced class that is a full string of all items entered for this submission.
        /// </summary>
        /// <param name="iUIData">0 = Grab all Unit Issue Data. 1,2,3 = Specific unit issue data.</param>
        /// <returns>MultiplePartsReplaced object</returns>
        private MultiplePartsReplaced getMPRString()
        {
            var mpr = new MultiplePartsReplaced();

            if (tiUI1.IsEnabled)
            {
                mpr = csCrossClassInteraction.getMPRString(dgMultipleParts, mpr);
            }

            if (tiUI2.IsEnabled && checkForUITabData(2))
            {
                mpr = csCrossClassInteraction.getMPRString(dgMultipleParts_2, mpr);
            }

            if (tiUI3.IsEnabled && checkForUITabData(3))
            {
                mpr = csCrossClassInteraction.getMPRString(dgMultipleParts_3, mpr);
            }

            return mpr;
        }

        /// <summary>
        /// Generates a List of MultiplePartsReplaced.
        /// </summary>
        /// <param name="iUIData">1,2,3 = Specific unit issue data.</param>
        /// <returns>A list of MultiplePartsReplaced objects</returns>
        private List<MultiplePartsReplaced> getMPRString(int iUIData)
        {
            var mpr = new List<MultiplePartsReplaced>(); //NOTE: Houses the entire list of MultiplePartsReplaced.

            if (tiUI1.IsEnabled && iUIData == 1)
            {
                var res = mpr.Concat(csCrossClassInteraction.getMPRList(dgMultipleParts));
                mpr = res.ToList();
            }

            if (tiUI2.IsEnabled && checkForUITabData(2) && iUIData == 2)
            {
                var res = mpr.Concat(csCrossClassInteraction.getMPRList(dgMultipleParts_2));
                mpr = res.ToList();
            }

            if (tiUI3.IsEnabled && checkForUITabData(3) && iUIData == 3)
            {
                var res = mpr.Concat(csCrossClassInteraction.getMPRList(dgMultipleParts_3));
                mpr = res.ToList();
            }

            return mpr;
        }

        /// <summary>
        /// To be used when submitting individual items to the TechnicianUnitIssues table
        /// </summary>
        private List<UnitIssueModel> getUnitIssues()
        {
            var lMPUI = new List<UnitIssueModel>();

            lMPUI.Add(getUnitIssueString(1));

            if (checkForUITabData(2))
                lMPUI.Add(getUnitIssueString(2));

            if (checkForUITabData(3))
                lMPUI.Add(getUnitIssueString(3));

            return lMPUI;
        }

        /// <summary>
        /// Controls enabling the Unit Issue Tabs.
        /// </summary>
        /// <param name="cb"></param>
        private void handleUnitIssues(ComboBox cb)
        {
            string[] splitters = { "_" };

            if (cb.Name.Contains("_")) //NOTE: Will be Unit Issue 2 or 3
            {
                string[] sSplit = cb.Name.Split(splitters, StringSplitOptions.RemoveEmptyEntries);
                if (sSplit[1].Equals("2"))
                {
                    if (!string.IsNullOrEmpty(cb.Text))
                        tiUI3.IsEnabled = true;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(cb.Text))
                    tiUI2.IsEnabled = true;
            }
        }

        private void keepReportedIssueUpdated(ComboBox cb)
        {
            cbReportedIssue.Text = cb.Text;
            if (tiUI2.IsEnabled)
                cbReportedIssue_2.Text = cb.Text;
            if (tiUI3.IsEnabled)
                cbReportedIssue_3.Text = cb.Text;
        }

        #region Events

        #region Button Click
        private void btnLookupPartName_Click(object sender, RoutedEventArgs e)
        {
            sVar.LogHandler.CreateLogAction((Button)sender, csLogging.LogState.CLICK);
            csCrossClassInteraction.LoadPartNumberForm(true, new List<TextBox> { txtPartNumber, txtPartName, txtPartSeries });
            sVar.LogHandler.CreateLogAction(string.Format("The operator selected Part Number '{0}', Part Name '{1}', Part Series '{2}'.", sVar.SelectedPartNumberPartName.PartNumber, sVar.SelectedPartNumberPartName.PartName, sVar.SelectedPartNumberPartName.PartSeries), csLogging.LogState.NOTE);

            if (!string.IsNullOrEmpty(txtPartNumber.Text))
                fillCommSubClass();
        }

        private void btnRebootSP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sp != null)
                {
                    sp.Close();
                    System.Threading.Thread.Sleep(500);
                    sp.Open();
                }
                else if (sp == null)
                {
                    MessageBox.Show("The serial port does not exist.", "Serial Port Error", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch { }

            serialPortStatusUpdate();
        }

        private void btnStartOver_Click(object sender, RoutedEventArgs e)
        {
            resetForm(true);
        }

        private void btnComplete_Click(object sender, RoutedEventArgs e)
        {
            sVar.LogHandler.CreateLogAction((Button)sender, csLogging.LogState.CLICK);

            if (checkForRefDesPartRep())
            {
                ArrayList alResults = canSubmit();

                if ((bool)alResults[0])
                {
                    sVar.LogHandler.CreateLogAction("The submission criteria has been met.", csLogging.LogState.NOTE);
                    submitData();
                    sVar.LogHandler.writeLogToFile();
                    resetForm(true);
                }
                else
                {
                    MessageBox.Show(alResults[1].ToString(), "Submission Criteria Not Met", MessageBoxButton.OK, MessageBoxImage.Warning);
                    sVar.LogHandler.CreateLogAction("The submission criteria was not met.\n" + alResults[1].ToString(), csLogging.LogState.WARNING);
                }
            }
        }

        private void btnAddRefPart_Click(object sender, RoutedEventArgs e)
        {
            if (((Control)sender).Name.Contains("_"))
            {
                if (((Control)sender).Name.EndsWith("2"))
                {
                    if (string.IsNullOrEmpty(txtMultiRefDes_2.Text) || string.IsNullOrEmpty(txtMultiPartNum_2.Text)) { return; }
                    else
                    {
                        sVar.LogHandler.CreateLogAction((Button)sender, csLogging.LogState.CLICK);

                        if (BOMFileActive && (!txtMultiRefDes_2.Items.Contains(txtMultiRefDes_2.Text)
                        || dgMultipleParts_2.Items
                        .OfType<MultiplePartsReplaced>()
                        .Where(mpr => mpr.RefDesignator.Equals(txtMultiRefDes_2.Text)).Count() > 0))
                        {
                            brdRefDes_2.BorderBrush = Brushes.Red;
                            brdRefDes_2.BorderThickness = new Thickness(1.0);
                            MessageBox.Show("Invalid Ref Designator", $"{txtMultiRefDes_2.Text} is not a valid designator!", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        else
                        {
                            brdRefDes_2.BorderBrush = null;
                            brdRefDes_2.BorderThickness = new Thickness(0.0);
                        }

                        if (BOMFileActive && !txtMultiRefDes_2.Items.Contains(txtMultiRefDes_2.Text))
                        {
                            string sWarning = string.Format($"The Reference Designator entered ( {txtMultiRefDes_2.Text} ) does not exist.\n" +
                                "Please verify the Part Number and try again.");
                            sVar.LogHandler.CreateLogAction(sWarning, csLogging.LogState.WARNING);
                            MessageBox.Show(sWarning, "Part Replaced Reference Designator Issue", MessageBoxButton.OK, MessageBoxImage.Warning);
                            txtMultiRefDes_2.Focus();
                            txtMultiRefDes_2.SelectAll();
                            return;
                        }

                        string _sPRPD = csCrossClassInteraction.GetPartReplacedPartDescription(txtMultiPartNum_2.Text);

                        if (BOMFileActive && string.IsNullOrEmpty(_sPRPD) && !string.IsNullOrEmpty(txtMultiPartNum_2.Text))
                        {
                            string sWarning = string.Format("The Part Replaced entered ( {0} ) does not exist. Please verify the Part Number and try again.", txtMultiPartNum_2.Text);
                            sVar.LogHandler.CreateLogAction(sWarning, csLogging.LogState.WARNING);
                            MessageBox.Show(sWarning, "Part Replaced Description Issue", MessageBoxButton.OK, MessageBoxImage.Warning);
                            txtMultiRefDes_2.Focus();
                            txtMultiRefDes_2.SelectAll();
                        }
                        else
                        {
                            var mpr = new MultiplePartsReplaced { RefDesignator = txtMultiRefDes_2.Text.TrimEnd(), PartReplaced = txtMultiPartNum_2.Text.TrimEnd(), PartsReplacedPartDescription = _sPRPD };
                            if (string.IsNullOrEmpty(mpr.PartReplaced) && !string.IsNullOrEmpty(mpr.RefDesignator))
                                sVar.LogHandler.CreateLogAction(string.Format("Adding Ref Designator '{0}' to dgMultipleParts. Parts Replaced is empty.", mpr.RefDesignator), csLogging.LogState.NOTE);
                            else if (!string.IsNullOrEmpty(mpr.PartReplaced) && string.IsNullOrEmpty(mpr.RefDesignator))
                                sVar.LogHandler.CreateLogAction(string.Format("Adding Part Replaced '{0}' to dgMultipleParts. Ref Designator is empty.", mpr.PartReplaced), csLogging.LogState.NOTE);
                            else sVar.LogHandler.CreateLogAction(string.Format("Adding Part Replaced '{0}' and Ref Designator '{1}' to dgMultipleParts.", mpr.PartReplaced, mpr.RefDesignator), csLogging.LogState.NOTE);

                            dgMultipleParts_2.Items.Add(mpr);
                            txtMultiPartNum_2.Text = txtMultiRefDes_2.Text = string.Empty;
                        }
                    }
                }
                else if (((Control)sender).Name.EndsWith("3"))
                {
                    if (string.IsNullOrEmpty(txtMultiRefDes_3.Text) || string.IsNullOrEmpty(txtMultiPartNum_3.Text)) { return; }
                    else
                    {
                        sVar.LogHandler.CreateLogAction((Button)sender, csLogging.LogState.CLICK);

                        if (BOMFileActive && (!txtMultiRefDes_3.Items.Contains(txtMultiRefDes_3.Text)
                        || dgMultipleParts_3.Items
                        .OfType<MultiplePartsReplaced>()
                        .Where(mpr => mpr.RefDesignator.Equals(txtMultiRefDes_3.Text)).Count() > 0))
                        {
                            brdRefDes_3.BorderBrush = Brushes.Red;
                            brdRefDes_3.BorderThickness = new Thickness(1.0);
                            MessageBox.Show("Invalid Ref Designator", $"{txtMultiRefDes_3.Text} is not a valid designator!", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        else
                        {
                            brdRefDes_3.BorderBrush = null;
                            brdRefDes_3.BorderThickness = new Thickness(0.0);
                        }

                        if (BOMFileActive && !txtMultiRefDes_3.Items.Contains(txtMultiRefDes_3.Text))
                        {
                            string sWarning = string.Format($"The Reference Designator entered ( {txtMultiRefDes_3.Text} ) does not exist.\n" +
                                "Please verify the Part Number and try again.");
                            sVar.LogHandler.CreateLogAction(sWarning, csLogging.LogState.WARNING);
                            MessageBox.Show(sWarning, "Part Replaced Reference Designator Issue", MessageBoxButton.OK, MessageBoxImage.Warning);
                            txtMultiRefDes_3.Focus();
                            txtMultiRefDes_3.SelectAll();
                            return;
                        }

                        string _sPRPD = csCrossClassInteraction.GetPartReplacedPartDescription(txtMultiPartNum_3.Text);

                        if (BOMFileActive && string.IsNullOrEmpty(_sPRPD) && !string.IsNullOrEmpty(txtMultiPartNum_3.Text))
                        {
                            string sWarning = string.Format("The Part Replaced entered ( {0} ) does not exist. Please verify the Part Number and try again.", txtMultiPartNum_3.Text);
                            sVar.LogHandler.CreateLogAction(sWarning, csLogging.LogState.WARNING);
                            MessageBox.Show(sWarning, "Part Replaced Description Issue", MessageBoxButton.OK, MessageBoxImage.Warning);
                            txtMultiRefDes_3.Focus();
                            txtMultiRefDes_3.SelectAll();
                        }
                        else
                        {
                            var mpr = new MultiplePartsReplaced { RefDesignator = txtMultiRefDes_3.Text.TrimEnd(), PartReplaced = txtMultiPartNum_3.Text.TrimEnd(), PartsReplacedPartDescription = _sPRPD };
                            if (string.IsNullOrEmpty(mpr.PartReplaced) && !string.IsNullOrEmpty(mpr.RefDesignator))
                                sVar.LogHandler.CreateLogAction(string.Format("Adding Ref Designator '{0}' to dgMultipleParts. Parts Replaced is empty.", mpr.RefDesignator), csLogging.LogState.NOTE);
                            else if (!string.IsNullOrEmpty(mpr.PartReplaced) && string.IsNullOrEmpty(mpr.RefDesignator))
                                sVar.LogHandler.CreateLogAction(string.Format("Adding Part Replaced '{0}' to dgMultipleParts. Ref Designator is empty.", mpr.PartReplaced), csLogging.LogState.NOTE);
                            else sVar.LogHandler.CreateLogAction(string.Format("Adding Part Replaced '{0}' and Ref Designator '{1}' to dgMultipleParts.", mpr.PartReplaced, mpr.RefDesignator), csLogging.LogState.NOTE);

                            dgMultipleParts_3.Items.Add(mpr);
                            txtMultiPartNum_3.Text = txtMultiRefDes_3.Text = string.Empty;
                        }
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(txtMultiRefDes.Text) || string.IsNullOrEmpty(txtMultiPartNum.Text)) { return; }
                else
                {
                    sVar.LogHandler.CreateLogAction((Button)sender, csLogging.LogState.CLICK);

                    if (BOMFileActive && (!txtMultiRefDes.Items.Contains(txtMultiRefDes.Text)
                        || dgMultipleParts.Items
                        .OfType<MultiplePartsReplaced>()
                        .Where(mpr => mpr.RefDesignator.Equals(txtMultiRefDes.Text)).Count() > 0))
                    {
                        brdRefDes.BorderBrush = Brushes.Red;
                        brdRefDes.BorderThickness = new Thickness(1.0);
                        MessageBox.Show("Invalid Ref Designator", $"{txtMultiRefDes.Text} is not a valid designator!", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    else
                    {
                        brdRefDes.BorderBrush = null;
                        brdRefDes.BorderThickness = new Thickness(0.0);
                    }

                    if (BOMFileActive & !txtMultiRefDes.Items.Contains(txtMultiRefDes.Text))
                    {
                        string sWarning = string.Format($"The Reference Designator entered ( {txtMultiRefDes.Text} ) does not exist.\n" +
                            "Please verify the Part Number and try again.");
                        sVar.LogHandler.CreateLogAction(sWarning, csLogging.LogState.WARNING);
                        MessageBox.Show(sWarning, "Part Replaced Reference Designator Issue", MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtMultiRefDes.Focus();
                        txtMultiRefDes.SelectAll();
                        return;
                    }

                    string _sPRPD = csCrossClassInteraction.GetPartReplacedPartDescription(txtMultiPartNum.Text);

                    if (BOMFileActive & string.IsNullOrEmpty(_sPRPD) && !string.IsNullOrEmpty(txtMultiPartNum.Text))
                    {
                        string sWarning = string.Format("The Part Replaced entered ( {0} ) does not exist. Please verify the Part Number and try again.", txtMultiPartNum.Text);
                        sVar.LogHandler.CreateLogAction(sWarning, csLogging.LogState.WARNING);
                        MessageBox.Show(sWarning, "Part Replaced Description Issue", MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtMultiRefDes.Focus();
                        txtMultiRefDes.SelectAll();
                    }
                    else
                    {
                        var mpr = new MultiplePartsReplaced { RefDesignator = txtMultiRefDes.Text.TrimEnd(), PartReplaced = txtMultiPartNum.Text.TrimEnd(), PartsReplacedPartDescription = _sPRPD.TrimEnd() };
                        if (string.IsNullOrEmpty(mpr.PartReplaced) && !string.IsNullOrEmpty(mpr.RefDesignator))
                            sVar.LogHandler.CreateLogAction(string.Format("Adding Ref Designator '{0}' to dgMultipleParts. Parts Replaced is empty.", mpr.RefDesignator), csLogging.LogState.NOTE);
                        else if (!string.IsNullOrEmpty(mpr.PartReplaced) && string.IsNullOrEmpty(mpr.RefDesignator))
                            sVar.LogHandler.CreateLogAction(string.Format("Adding Part Replaced '{0}' to dgMultipleParts. Ref Designator is empty.", mpr.PartReplaced), csLogging.LogState.NOTE);
                        else sVar.LogHandler.CreateLogAction(string.Format("Adding Part Replaced '{0}' and Ref Designator '{1}' to dgMultipleParts.", mpr.PartReplaced, mpr.RefDesignator), csLogging.LogState.NOTE);

                        dgMultipleParts.Items.Add(mpr);
                        txtMultiPartNum.Text = txtMultiRefDes.Text = string.Empty;
                    }
                }
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            sVar.LogHandler.CreateLogAction((Button)sender, csLogging.LogState.CLICK);

            if (((Control)sender).Name.ToString().Equals("btnReset"))
            {
                resetUnitIssues(1);
                checkToDisableUITabs();
            }
            else if (((Control)sender).Name.ToString().Equals("btnReset_2"))
            {
                resetUnitIssues(2);
                checkToDisableUITabs();
            }
            else if (((Control)sender).Name.ToString().Equals("btnReset_3"))
            {
                resetUnitIssues(3);
                checkToDisableUITabs();
            }
        }
        #endregion

        private void dgBeginEdit(object sender, DataGridBeginningEditEventArgs e)
            => e.Cancel = true;

        private void dgPrevRepairInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgPrevRepairInfo.SelectedItem != null)
                {
                    var pri = new frmProductionPRI((PreviousRepairInformation)dgPrevRepairInfo.SelectedItem)
                    {
                        Owner = this
                    };
                    pri.Show();
                    Activate();
                }
            } catch(ArgumentOutOfRangeException aoore)
            {
                csExceptionLogger.csExceptionLogger.Write("ProductionPRI_RandomCrahsing", aoore);
            }
        }

        private void previewInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void txtSerialNumber_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
                beginSerialNumberSearch();
        }

        private void spDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                sVar.LogHandler.CreateLogAction("Serial Port Data Received - Begin", csLogging.LogState.NOTE);
                Dispatcher.Invoke(delegate
                {
                    txtSerialNumber.Text = string.Empty;
                });
                string data = string.Empty;
                while (true)
                {
                    if (!sp.IsOpen)
                    {
                        if (sp != null)
                        {
                            sp.DiscardInBuffer();
                            sp.DiscardOutBuffer();
                        }
                        serialPortStatusUpdate();
                        break;
                    }

                    if (sp != null)
                        data += sp.ReadExisting();

                    if (data.EndsWith("\r") || data.EndsWith("\n"))
                        break;
                    else
                    {
                        if (data.Length > 0)
                        {
                            if (txtSerialNumber.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
                                txtSerialNumber.Text += data;
                            else
                            {
                                Dispatcher.Invoke(delegate
                                {
                                    txtSerialNumber.Text += data;
                                });
                                data = string.Empty;
                            }
                        }
                    }
                }

                if (data.Length > 0)
                {
                    if (txtSerialNumber.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
                    {
                        txtSerialNumber.Text += data;
                        beginSerialNumberSearch();
                    }
                    else
                    {
                        Dispatcher.Invoke(delegate
                        {
                            txtSerialNumber.Text += data;
                            txtSerialNumber.Text = txtSerialNumber.Text.TrimEnd();
                            sVar.LogHandler.CreateLogAction("Serial Port Data Received - End", csLogging.LogState.NOTE);
                            beginSerialNumberSearch();
                        });
                        data = string.Empty;
                    }
                }
            }
            catch (InvalidOperationException ioe)
            {
                MessageBox.Show($"{ioe.Message}\nspDataReceived()", "Serial Number - Null Binding", MessageBoxButton.OK, MessageBoxImage.Warning);
                sVar.LogHandler.CreateLogAction("Error parsing data from the COM Port.\nError Message: " + ioe.Message, csLogging.LogState.WARNING);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error receiving data from the COM Port.\n\nError Message: " + ex.Message, "spDataReceived()", MessageBoxButton.OK, MessageBoxImage.Error);
                sVar.LogHandler.CreateLogAction("Error receiving data from the COM Port.\nError Message: " + ex.Message, csLogging.LogState.ERROR);
            }
        }

        /// <summary>
        ///     The tSPChecker will tick ever 30? seconds to make sure the scanner is still connected.
        //      If the SP has been disconnected, it will attempt to reconnect itself.
        //      If that fails, it will alert the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tSPChecker_Tick(object sender, EventArgs e)
        {
            try
            {
                if (sp != null)
                {
                    if (bTimerRebootAttempt && !sp.IsOpen)
                    {
                        sp.Open();
                        System.Threading.Thread.Sleep(250);
                        if (!sp.IsOpen)
                        {
                            MessageBox.Show("The serial port has attempted to reboot itself twice unsuccessfully. Please check the serial port settings.", "Serial Port Issue", MessageBoxButton.OK, MessageBoxImage.Error);
                            sp = null;
                            bTimerRebootAttempt = false;
                            tSPChecker.Stop();
                        }
                        else bTimerRebootAttempt = false;
                    }
                    else if (bTimerRebootAttempt && sp.IsOpen)
                    {
                        bTimerRebootAttempt = false;
                    }
                    else if (!sp.IsOpen)
                    {
                        sp.Open();
                        bTimerRebootAttempt = true;
                    }
                }
                else
                {
                    //NOTE: The serial port doesn't exist so the timer has no need to run.
                    tSPChecker.Stop();
                    bTimerRebootAttempt = false;
                }
            }
            catch { }
        }

        private void txtMultiRefKeyUp(object sender, KeyEventArgs e)
        {
            /*TextBox tbox;

            if(sender is ComboBox cbox)
            {
                tbox = (cbox.Template.FindName("PART_EditableTextBox", cbox) as TextBox);
            } else if (sender is TextBox ttbox) {
                tbox = ttbox;
            } else {
                Console.WriteLine($"Null path not supported for txtMultiRefKeyUp");
                return;
            }

            csCrossClassInteraction.checkForValidRefDes(tbox);*/
        }

        private void dataGrid_KeyUp(object sender, KeyEventArgs e)
        {
            var grid = ((DataGrid)sender);

            if (e.Key == Key.Delete && grid.SelectedItem != null)
            {
                var row = (DataGridRow)dgBOMList.ItemContainerGenerator.ContainerFromItem(grid.SelectedItem);
                if (row != null) { row.Foreground = Brushes.Black; row.Background = Brushes.White; }
                grid.Items.Remove(grid.SelectedItem);
            }
        }

        #region Log Actions
        private void rtbGotFocus(object sender, RoutedEventArgs e)
        {
            sVar.LogHandler.CreateLogAction((RichTextBox)sender, csLogging.LogState.ENTER);
        }

        private void rtbLostFocus(object sender, RoutedEventArgs e)
        {
            sVar.LogHandler.CreateLogAction((RichTextBox)sender, csLogging.LogState.LEAVE);
        }

        private void txtGotFocus(object sender, RoutedEventArgs e)
        {
            sVar.LogHandler.CreateLogAction((TextBox)sender, csLogging.LogState.ENTER);
        }

        private void txtLostFocus(object sender, RoutedEventArgs e)
        {
            sVar.LogHandler.CreateLogAction((TextBox)sender, csLogging.LogState.LEAVE);
        }

        private void cbDDClosed(object sender, EventArgs e)
        {
            sVar.LogHandler.CreateLogAction((ComboBox)sender, csLogging.LogState.DROPDOWNCLOSED);

            var cmbx = (ComboBox)sender;
            var chosenOption = ((Control)sender).Name.ToString();

            if (!chosenOption.Equals("cbFromArea"))
                handleUnitIssues((ComboBox)sender);

            if (chosenOption.Equals("cbReportedIssue"))
            {
                cbReportedIssue_2.Text = cbReportedIssue_3.Text = cbReportedIssue.Text;
            }

            //NOTE: Need to handle the Issue, Item and Problem items separately
            if (chosenOption.StartsWith("cbIssue"))
            {
                if (!chosenOption.Contains("_"))
                {
                    resetIIPItems(true, cbItem, cbProblem, txtMultiRefDes, lblRefDes, txtMultiPartNum, lblPartNum, btnAddRefPart, dgMultipleParts, brdRefDes);
                    fillItemCB(cbIssue, cbItem, cbProblem);
                }
                else if (chosenOption.EndsWith("2"))
                {
                    resetIIPItems(true, cbItem_2, cbProblem_2, txtMultiRefDes_2, lblRefDes_2, txtMultiPartNum_2, lblPartNum_2, btnAddRefPart_2, dgMultipleParts_2, brdRefDes_2);
                    fillItemCB(cbIssue_2, cbItem_2, cbProblem_2);
                }
                else if (chosenOption.EndsWith("3"))
                {
                    resetIIPItems(true, cbItem_3, cbProblem_3, txtMultiRefDes_3, lblRefDes_3, txtMultiPartNum_3, lblPartNum_3, btnAddRefPart_3, dgMultipleParts_3, brdRefDes_3);
                    fillItemCB(cbIssue_3, cbItem_3, cbProblem_3);
                }
            }
            else if (chosenOption.StartsWith("cbItem"))
            {
                if (!chosenOption.Contains("_"))
                {
                    if (!string.IsNullOrEmpty(cbItem.Text))
                    {
                        resetIIPItems(false, cbItem, cbProblem, txtMultiRefDes, lblRefDes, txtMultiPartNum, lblPartNum, btnAddRefPart, dgMultipleParts, brdRefDes);
                        fillProblemCB(cbIssue, cbItem, cbProblem);
                        dispIIPElements(lblRefDes, txtMultiRefDes, lblPartNum, txtMultiPartNum, cbItem, cbProblem, dgMultipleParts, btnAddRefPart, brdRefDes);
                    }
                }
                else if (chosenOption.EndsWith("2"))
                {
                    if (!string.IsNullOrEmpty(cbItem_2.Text))
                    {
                        resetIIPItems(false, cbItem_2, cbProblem_2, txtMultiRefDes_2, lblRefDes_2, txtMultiPartNum_2, lblPartNum_2, btnAddRefPart_2, dgMultipleParts_2, brdRefDes_2);
                        fillProblemCB(cbIssue_2, cbItem_2, cbProblem_2);
                        dispIIPElements(lblRefDes_2, txtMultiRefDes_2, lblPartNum_2, txtMultiPartNum_2, cbItem_2, cbProblem_2, dgMultipleParts_2, btnAddRefPart_2, brdRefDes_2);
                    }
                }
                else if (chosenOption.EndsWith("3"))
                {
                    if (!string.IsNullOrEmpty(cbItem_3.Text))
                    {
                        resetIIPItems(false, cbItem_3, cbProblem_3, txtMultiRefDes_3, lblRefDes_3, txtMultiPartNum_3, lblPartNum_3, btnAddRefPart_3, dgMultipleParts_3, brdRefDes_3);
                        fillProblemCB(cbIssue_3, cbItem_3, cbProblem_3);
                        dispIIPElements(lblRefDes_3, txtMultiRefDes_3, lblPartNum_3, txtMultiPartNum_3, cbItem_3, cbProblem_3, dgMultipleParts_3, btnAddRefPart_3, brdRefDes_3);
                    }
                }
            }
            else if (chosenOption.StartsWith("cbProblem"))
            {
                if (!chosenOption.Contains("_"))
                {
                    dispIIPElements(lblRefDes, txtMultiRefDes, lblPartNum, txtMultiPartNum, cbItem, cbProblem, dgMultipleParts, btnAddRefPart, brdRefDes);
                }
                else if (chosenOption.EndsWith("2"))
                {
                    dispIIPElements(lblRefDes_2, txtMultiRefDes_2, lblPartNum_2, txtMultiPartNum_2, cbItem_2, cbProblem_2, dgMultipleParts_2, btnAddRefPart_2, brdRefDes_2);
                }
                else if (chosenOption.EndsWith("3"))
                {
                    dispIIPElements(lblRefDes_3, txtMultiRefDes_3, lblPartNum_3, txtMultiPartNum_3, cbItem_3, cbProblem_3, dgMultipleParts_3, btnAddRefPart_3, brdRefDes_3);
                }
            }
            else if (chosenOption.StartsWith("cbTechAction"))
            {
                var currString = rtbAdditionalComments.GetContent().ToString();

                if (!chosenOption.Contains("_"))
                {
                    //Tech Action 1
                    //rtbAdditionalComments.AppendText("Tech Action #1: " + cmbx.Text + Environment.NewLine);
                }
                else if (chosenOption.EndsWith("2"))
                {
                    //Tech Action 2
                    //rtbAdditionalComments.AppendText("Tech Action #2: " + cmbx.Text + Environment.NewLine);
                }
                else if (chosenOption.EndsWith("3"))
                {
                    //Tech Action 3
                    //rtbAdditionalComments.AppendText("Tech Action #3: " + cmbx.Text + Environment.NewLine);
                }
            }
        }
        #endregion


        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sp != null && sp.IsOpen)
                sp.Close();
            sp = null;

            if (tSPChecker != null && tSPChecker.IsEnabled)
                tSPChecker.Stop();
            tSPChecker = null;

            sVar.resetStaticVars();
            csSplashScreenHelper.Close();
            MainWindow.GlobalInstance.MakeFocus();
        }

        private void refDesIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is Control c && c.Name.Contains("_"))
            {
                if (c.Name.EndsWith("2"))
                {
                    txtMultiPartNum_2.SelectedIndex = txtMultiRefDes_2.SelectedIndex;
                }
                else if (c.Name.EndsWith("3"))
                {
                    txtMultiPartNum_3.SelectedIndex = txtMultiRefDes_3.SelectedIndex;
                }
            }
            else
            {
                txtMultiPartNum.SelectedIndex = txtMultiRefDes.SelectedIndex;
            }
        }

        private void dgBOMList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var targetGrid = (DataGrid)(tcUnitIssues.SelectedContent as Grid).FindName($"dgMultipleParts{(tcUnitIssues.SelectedIndex > 0 ? $"_{tcUnitIssues.SelectedIndex + 1}" : "")}");
            var item = (MultiplePartsReplaced)dgBOMList.SelectedItem;

            if (!targetGrid.Items.Contains(item))
            {
                var row = (DataGridRow)dgBOMList.ItemContainerGenerator.ContainerFromIndex(dgBOMList.SelectedIndex);
                if (row != null) { row.Foreground = Brushes.White; row.Background = Brushes.DarkGreen; }
                targetGrid.Items.Add(item);
            }
        }

        private Grid PrevGrid = null;
        private void tcUnitIssues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var targetGrid = (DataGrid)(tcUnitIssues.SelectedContent as Grid).FindName($"dgMultipleParts{(tcUnitIssues.SelectedIndex > 0 ? $"_{tcUnitIssues.SelectedIndex + 1}" : "")}");
                var pgridIndex = int.Parse(PrevGrid.Name.Last().ToString());
                var prevGrid = (DataGrid)PrevGrid.FindName($"dgMultipleParts{(pgridIndex > 0 ? $"_{pgridIndex + 1}" : "")}");
                if (prevGrid != null)
                {
                    foreach (MultiplePartsReplaced mpr in prevGrid.Items)
                    {
                        if (dgBOMList.Items.Contains(mpr))
                        {
                            var row = (DataGridRow)dgBOMList.ItemContainerGenerator.ContainerFromItem(mpr);
                            if (row != null) { row.Foreground = Brushes.Black; row.Background = Brushes.White; }
                        }
                    }
                }

                foreach (MultiplePartsReplaced mpr in targetGrid.Items)
                {
                    if (dgBOMList.Items.Contains(mpr))
                    {
                        var row = (DataGridRow)dgBOMList.ItemContainerGenerator.ContainerFromItem(mpr);
                        if (row != null) { row.Foreground = Brushes.White; row.Background = Brushes.DarkGreen; }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            } finally
            {
                PrevGrid = tcUnitIssues.SelectedContent as Grid ?? null;
            }
        }

        private void ExpBOMInfo_Expanded(object sender, RoutedEventArgs e)
        {
            expBOMInfo.Margin = new Thickness(15, 200, 650, 300);
        }

        private void ExpBOMInfo_Collapsed(object sender, RoutedEventArgs e)
        {
            expBOMInfo.Margin = new Thickness(15, 200, 1050, 300);
        }
    }
}
