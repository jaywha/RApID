using EricStabileLibrary;
using RApID_Project_WPF.Classes;
using RApID_Project_WPF.Forms;
using RetestVerifierAppWPF.Classes;
using SNMapperLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using SNM = SNMapperLib.csSerialNumberMapper;

namespace RApID_Project_WPF
{
    //TODO: Random crashes in Repair => TryCatch & Breakpoint Step Debugging


    /// <summary>
    /// Interaction logic for Repair.xaml
    /// </summary>
    public partial class frmRepair : Window, INotifyPropertyChanged
    {
        #region Variables
        private enum SubmissionStatus { COMPLETE, SENDTOQC, SENDTODQE };

        csSQL.csSQLClass sqlClass = new csSQL.csSQLClass();
        csObjectHolder.csObjectHolder holder = csObjectHolder.csObjectHolder.ObjectHolderInstance();

        StaticVars sVar = StaticVars.StaticVarsInstance();
        SerialPort sp;
        DispatcherTimer tSPChecker;
        List<PC1> lPC1;
        List<PC2> lPC2;
        List<PC3> lPC3;
        List<EndUse> lEndUse;

        bool bTimerRebootAttempt = false; //NOTE: tSPChecker will attempt to reboot itself once if it gets disconnected. This flag will be used to track that.
        bool bStop = false;

        private string sRPNum = string.Empty;
        string sUserDepartmentNumber = "";
        string sDQE_DeptNum = "320900";
        double dLineNumber;
        private bool _BomFileActive = false;
        public bool BOMFileActive
        {
            get => _BomFileActive;
            set
            {
                _BomFileActive = value;
                OnPropertyChanged();
            }
        }

        InitSplash initS = new InitSplash();
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public frmRepair(bool bRework)
        {
            InitializeComponent();

            CheckForManual();
        }

        private void CheckForManual()
        {
            txtPartReplaced.PrepForManualInput();
            txtPartReplaced_2.PrepForManualInput();
            txtPartReplaced_3.PrepForManualInput();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Hide();
            initS.InitSplash1("Initializing Form...");
            buildDGViews();
            csSplashScreenHelper.ShowText("Loading DataLog...");
            initDataLogForm();
            csSplashScreenHelper.ShowText("Initial SQL Query...");
            initSQLQuery();
            csSplashScreenHelper.ShowText("Building Serial Port...");
            handleInitSerialPort();
            csSplashScreenHelper.ShowText("Initializing Logging...");
            setupLogging();

            GC.Collect();
            csSplashScreenHelper.ShowText("Done!");
            csSplashScreenHelper.Hide();
            this.Activate();
#if DEBUG
            //txtBarcode.Text = "131119030211";
#endif
            WindowState = WindowState.Maximized;
            Show();
        }

        #region Initialization
        private void buildDGViews()
        {
            dgMultipleParts.dgBuildView(DataGridTypes.MULTIPLEPARTS);
            dgMultipleParts_2.dgBuildView(DataGridTypes.MULTIPLEPARTS);
            dgMultipleParts_3.dgBuildView(DataGridTypes.MULTIPLEPARTS);
            ucAOITab.dgAOI.dgBuildView(DataGridTypes.AOI);
            ucAOITab.dgDefectCodes.dgBuildView(DataGridTypes.DEFECTCODES);
            dgPrevRepairInfo.dgBuildView(DataGridTypes.PREVREPAIRINFO);

            txtPartReplaced.ItemsSource = OrigPartSource;
            txtPartReplaced_2.ItemsSource = OrigPartSource;
            txtPartReplaced_3.ItemsSource = OrigPartSource;
            txtRefDes.ItemsSource = OrigRefSource;
            txtRefDes_2.ItemsSource = OrigRefSource;
            txtRefDes_3.ItemsSource = OrigRefSource;
        }

        /// <summary>
        /// Takes care of items that need to be 'edited' when the form is first initialized
        /// </summary>
        private void initDataLogForm()
        {
            txtTechName.Text = System.Environment.UserName;
#if DEBUG
            txtOrderNumber.Text = "8149953";
            txtBarcode.Text = "180405030127";
#endif
            dtpDateReceived.SelectedDate = DateTime.Now;

            // --->
            // need to get the department name so that I can tell who is in DQE/Alpharetta to allow
            // for the DQE form to submit if a unit comes directly to them without an order number
            sUserDepartmentNumber = sqlClass.SQLGet_String(@"Select [Department Number] From UserLogin Where Username = '" + txtTechName.Text + "'",
                holder.HummingBirdConnectionString); //([Department Number] = N'320900' = Alpharetta)

            if (sUserDepartmentNumber.Equals(sDQE_DeptNum))
            {
                txtOrderNumber.IsReadOnly = true;
                txtOrderNumber.Background = Brushes.LightGray;
                txtBarcode.Focus();
            }
            else txtOrderNumber.Focus();
            // <---
        }

        private void initSQLQuery()
        {
            #region Temp List for Sorting
            List<string> lTOR = new List<string>();
            List<string> lTOF = new List<string>();
            List<string> lReportedIssue = new List<string>();
            List<string> lTestResult = new List<string>();
            List<string> lTestResultAbort = new List<string>();
            List<string> lCause = new List<string>();
            List<string> lReplacement = new List<string>();
            Dictionary<string, bool> lBoolShowAll = new Dictionary<string, bool>();
            List<string> lTechAction1 = new List<string>();
            List<string> lTechAction2 = new List<string>();
            List<string> lTechAction3 = new List<string>();
            #endregion

            string query1 = "SELECT * FROM RApID_DropDowns";
            string query2 = "SELECT PC1, PC2 FROM JDECodes";

            SqlConnection conn = new SqlConnection(holder.RepairConnectionString);
            SqlCommand cmd = new SqlCommand(query1, conn);
            try
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (!string.IsNullOrEmpty(reader["TypeOfReturn"].ToString())
                            && !reader["TypeOfReturn"].ToString().Trim().ToLower().Equals("production"))
                            lTOR.Add(reader["TypeOfReturn"].ToString());

                        if (!string.IsNullOrEmpty(reader["TypeOfFailure"].ToString()))
                            lTOF.Add(reader["TypeOfFailure"].ToString());

                        if (!string.IsNullOrEmpty(reader["Cause"].ToString()))
                            lCause.Add(reader["Cause"].ToString());

                        if (!string.IsNullOrEmpty(reader["Replacement"].ToString()))
                            lReplacement.Add(reader["Replacement"].ToString());

                        if (!string.IsNullOrEmpty(reader["TestResult"].ToString()))
                            lTestResult.Add(reader["TestResult"].ToString());

                        if (!string.IsNullOrEmpty(reader["TestResult_Abort"].ToString()))
                            lTestResultAbort.Add(reader["TestResult_Abort"].ToString());

                        if (!string.IsNullOrEmpty(reader["TechAction"].ToString()))
                        {
                            if (Convert.ToBoolean(reader["ShowTechActionInAll"]))
                            {
                                lBoolShowAll.Add(reader["TechAction"].ToString(),
                                    (bool)reader["ShowTechActionInAll"]);
                            }
                        }
                    }
                }
                conn.Close();

                cmd = new SqlCommand(query2, conn);

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (!string.IsNullOrEmpty(reader[0].ToString()))
                            lReportedIssue.Add(reader[0].ToString());
                        if (!string.IsNullOrEmpty(reader["PC2"].ToString()))
                        {
                            if (lBoolShowAll.Keys.Contains(reader["PC2"].ToString())
                                && lBoolShowAll[reader["PC2"].ToString()])
                            {
                                lTechAction1.Add(reader["PC2"].ToString());
                                lTechAction2.Add(reader["PC2"].ToString());
                                lTechAction3.Add(reader["PC2"].ToString());
                            }
                            else lTechAction1.Add(reader["PC2"].ToString());
                        }
                    }
                }

                conn.Close();

                #region Combobox Fill
                cbTOR.cbFill(lTOR);
                cbTOF.cbFill(lTOF);

                cbReportedIssue.cbFill(lReportedIssue);
                cbTestResult.cbFill(lTestResult);
                cbTestResultAbort.cbFill(lTestResultAbort);
                cbCause.cbFill(lCause);
                cbReplacement.cbFill(lReplacement);
                cbTechAction1.cbFill(lTechAction1);

                cbReportedIssue_2.cbFill(lReportedIssue);
                cbTestResult_2.cbFill(lTestResult);
                cbTestResultAbort_2.cbFill(lTestResultAbort);
                cbCause_2.cbFill(lCause);
                cbReplacement_2.cbFill(lReplacement);
                cbTechAction2.cbFill(lTechAction2);

                cbReportedIssue_3.cbFill(lReportedIssue);
                cbTestResult_3.cbFill(lTestResult);
                cbTestResultAbort_3.cbFill(lTestResultAbort);
                cbCause_3.cbFill(lCause);
                cbReplacement_3.cbFill(lReplacement);
                cbTechAction3.cbFill(lTechAction3);
                #endregion

            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();

                MessageBox.Show("Issue initializing the form.\nError Message: " + ex.Message, "initSQLQuery()", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            initJDECodes();
        }

        private void initJDECodes()
        {
            string query = "SELECT * FROM JDECodes";
            lPC1 = new List<PC1>();
            lPC2 = new List<PC2>();
            lPC3 = new List<PC3>();
            lEndUse = new List<EndUse>();

            SqlConnection conn = new SqlConnection(holder.RepairConnectionString);
            SqlCommand cmd = new SqlCommand(query, conn);
            try
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["PC1"] != DBNull.Value)
                        {
                            if (!string.IsNullOrEmpty(reader["PC1"].ToString()))
                            {
                                lPC1.Add(new PC1 { CodeName = reader["PC1"].ToString(), ReturnCode = reader["PC1Code"].ToString() });
                            }
                        }
                        if (reader["PC2"] != DBNull.Value)
                        {
                            if (!string.IsNullOrEmpty(reader["PC2"].ToString()))
                            {
                                lPC2.Add(new PC2 { CodeName = reader["PC2"].ToString(), ReturnCode = reader["PC2Code"].ToString() });
                            }
                        }
                        if (reader["PC3"] != DBNull.Value)
                        {
                            if (!string.IsNullOrEmpty(reader["PC3"].ToString()))
                            {
                                lPC3.Add(new PC3 { CodeName = reader["PC3"].ToString(), ReturnCode = reader["PC3Code"].ToString() });
                            }
                        }
                        if (reader["EndUse"] != DBNull.Value)
                        {
                            if (!string.IsNullOrEmpty(reader["EndUse"].ToString()))
                            {
                                lEndUse.Add(new EndUse { CodeName = reader["EndUse"].ToString(), ReturnCode = reader["EndUseCode"].ToString() });
                            }
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();

                MessageBox.Show("Error initializing Return Codes.\nError Message: " + ex.Message, "initJDECodes() Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Attemps to open the serial port from the data stored in settings.
        /// </summary>
        private void handleInitSerialPort()
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

        private void setupLogging()
        {
            try
            {
                sVar.LogHandler.CheckDirectory(System.Environment.UserName);
            }
            catch { }
        }
        #endregion

        #region Reset Form Items
        private void resetForm(bool bCompleteReset)
        {
            sVar.resetStaticVars();

            if (bCompleteReset)
            {
                txtOrderNumber.Text = string.Empty;
                lblRPNumber.Content = string.Empty;
                sRPNum = string.Empty;
                txtBarcode.Text = string.Empty;
                dLineNumber = 0;
                resetCustomerInformation();
                txtOrderNumber.Focus();
            }

            txtPartName.Text = string.Empty;
            txtPartNumber.Text = string.Empty;
            txtSeries.Text = string.Empty;
            txtCommSubClass.Text = string.Empty;

            brdRefDes.BorderThickness = new Thickness(0.0);
            brdRefDes_2.BorderThickness = new Thickness(0.0);
            brdRefDes_3.BorderThickness = new Thickness(0.0);

            txtQTY.Text = string.Empty;
            lblQTY.Visibility = Visibility.Hidden;
            txtQTY.Visibility = Visibility.Hidden;
            cbxNPF.IsChecked = false;
            cbxNPF.IsEnabled = false;
            cbxNPF.Visibility = Visibility.Hidden;

            txtSWVersion.Text = string.Empty;
            cbxScrap.IsChecked = false;
            dtpDateReceived.SelectedDate = DateTime.Now;
            cbTOR.Text = string.Empty;
            cbTOF.Text = string.Empty;
            txtHOU.Text = string.Empty;

            rtbAdditionalComments.Document.Blocks.Clear();

            dgPrevRepairInfo.Items.Clear();

            BOMFileActive = false;

            ucEOLTab.lblEOL.Content = "End of Line";
            ucEOLTab.lblPOST.Content = "Post Burn In";

            if (OrigPartSource != null) OrigPartSource.Clear();
            if (OrigRefSource != null) OrigRefSource.Clear();

            cbReportedIssue.SelectedIndex = -1;
            resetUnitIssues();
            ucEOLTab.Reset();
            ucAOITab.Reset();
            cbTechAction1.SelectedIndex = -1;
            cbTechAction2.SelectedIndex = -1;
            cbTechAction3.SelectedIndex = -1;
        }

        /// <summary>
        /// Reset all of the unit issues
        /// </summary>
        private void resetUnitIssues()
        {
            resetUnitIssues(1);
            resetUnitIssues(2);
            resetUnitIssues(3);

            tiUI2.IsEnabled = tiUI3.IsEnabled = false;
            tiUI1.Focus();
        }

        /// <summary>
        /// Will reset a specifc unit issue.
        /// </summary>
        /// <param name="iUReset">The unit issue number from <see cref="tabcUnitIssues"/>, in order. </param>
        private void resetUnitIssues(int iUReset)
        {
            Grid grid = (iUReset == 1 ? gridUI1 : (
                        iUReset == 2 ? gridUI2 : (
                        iUReset == 3 ? gridUI3 : null)));

            if (grid == null) throw new ArgumentOutOfRangeException($"frmRepair.resetUnitIssues(int) -> No grid found with id {iUReset}!");

            foreach (UIElement uie in grid.Children)
            {
                if (uie.GetType().Name.Equals("ComboBox"))
                {
                    ComboBox cb = (ComboBox)uie;
                    cb.SelectedIndex = -1;
                    cb.Text = "";
                    // Only gets RefDes & PN --- if (uie is ComboBox cbx && cbx.Name.Contains("txt")) cbx.ItemsSource = new List<string>() { "" };
                }
                if (uie.GetType().Name.Equals("TextBox"))
                {
                    TextBox tb = (TextBox)uie;
                    tb.Text = string.Empty;
                }
                if (uie.GetType().Name.Equals("DataGrid"))
                {
                    DataGrid dg = (DataGrid)uie;
                    dg.Items.Clear();
                }
            }
        }

        private void resetCustomerInformation()
        {
            foreach (UIElement uie in gCustInfo.Children)
            {
                if (uie.GetType().Name.Equals("TextBox"))
                {
                    TextBox tb = (TextBox)uie;
                    tb.Text = string.Empty;
                }
            }
        }
        #endregion

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
                    ComboBox cb = (ComboBox)uie;
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
                    ComboBox cb = (ComboBox)uie;
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
                    ComboBox cb = (ComboBox)uie;
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
                        ComboBox cb = (ComboBox)uie;

                        if (cb.Name.ToString().StartsWith("cbReportedIssue"))
                            continue;
                        else if (!string.IsNullOrEmpty(cb.Text))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void beginSerialNumberSearch()
        {
            resetForm(false);

            sVar.LogHandler.LogCreation = DateTime.Now;
            if (!string.IsNullOrEmpty(txtBarcode.Text))
            {
                sVar.LogHandler.CreateLogAction("**** This is a Repair Log ****", csLogging.LogState.NOTE);
                sVar.LogHandler.CreateLogAction("The Serial Number related to this log is: " + txtBarcode.Text.TrimEnd(), csLogging.LogState.NOTE);
                fillDataLog();
                ucEOLTab.Fill();
                ucAOITab.Fill();

                MapRefDesToPartNum();
            }
        }

        private void fillDataLog()
        {
            QueryProduction(); if (bStop) return;

            if (!string.IsNullOrEmpty(txtPartNumber.Text))
            {
                sVar.LogHandler.CreateLogAction("Attempting to get the Part Series now.", csLogging.LogState.NOTE);

                if (string.IsNullOrEmpty(txtSeries.Text)) { txtSeries.Text = csCrossClassInteraction.SeriesQuery(txtPartNumber.Text); }
                if (!string.IsNullOrEmpty(txtSeries.Text)) { sVar.LogHandler.CreateLogAction("The Part Series was found. (" + txtSeries.Text.TrimEnd() + ")", csLogging.LogState.NOTE); }

                fillCommoditySubClass();
                fillSoftwareVersion();
            }
            QueryTechReport();
        }

        private void QueryProduction()
        {
            try
            {
                string query = "SELECT Xducer, Model FROM Production WHERE SerialNum = '" + txtBarcode.Text + "';";
                using (SqlConnection conn = new SqlConnection(holder.HummingBirdConnectionString))
                {
                    conn.Open();
                    using (SqlDataReader reader = new SqlCommand(query, conn).ExecuteReader())
                    {
                        if (reader.Read() && reader.HasRows)
                        {
                            bool isXducer = reader.IsDBNull(0) ? false : reader.GetBoolean(0);
                            string sProdQueryResults = reader.IsDBNull(1) ? "" : reader.GetString(1); 
                            conn.Close();

                            CheckForXDucer(ref sProdQueryResults, isXducer); if (bStop) return;

                            sVar.LogHandler.CreateLogAction("Part Number '" + sProdQueryResults + "' was found.", csLogging.LogState.NOTE);
                            txtPartNumber.Text = sProdQueryResults;
                        }
                        QueryItemMaster();
                    }
                }
            }
            catch (Exception e)
            {
                csExceptionLogger.csExceptionLogger.Write("RPR_QueryProduction", e);
            }
        }

        /// <summary>
        /// Ensures a part number was found for Xducers
        /// </summary>
        /// <param name="sProdQueryResults">empty string but variable reference</param>
        /// <param name="isXducer"></param>
        public void CheckForXDucer(ref string sProdQueryResults, bool isXducer)
        {
            if (isXducer)
            {
                #region Xducer Path
                string query2 = "SELECT PartNumber FROM tblXducerTestResults WHERE SerialNumber = '" + txtBarcode.Text + "';";
                string sProdQuery2Results = csCrossClassInteraction.ProductionQuery(query2);

                if (!string.IsNullOrWhiteSpace(sProdQuery2Results))
                {
                    sProdQueryResults = sProdQuery2Results;
                }
                else
                {
                    MessageBoxResult ans = MessageBox.Show("We couldn't find the serial number in the final test database.\n" +
                        "That means this Xducer wasn't final tested." +
                        "Would you like to continue?",
                        "Untested Serial Number", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (ans == MessageBoxResult.No)
                    {
                        resetForm(true); bStop = true; return;
                    }

                    string query3 = "SELECT PartNumber FROM tblXducerTestResultsBenchTest WHERE SerialNumber = '" + txtBarcode.Text + "';";
                    string sProdQuery3Results = csCrossClassInteraction.ProductionQuery(query3);

                    if (!string.IsNullOrWhiteSpace(sProdQuery3Results))
                    {
                        sProdQueryResults = sProdQuery3Results;
                    }
                }
                #endregion
            }
            else
            {
                #region Unit Path

                string query2 = "SELECT Assy FROM Production3 WHERE SerialNum = '" + txtBarcode.Text + "';";
                string sProdQuery2Results = csCrossClassInteraction.ProductionQuery(query2);

                if (!string.IsNullOrWhiteSpace(sProdQuery2Results))
                {
                    sProdQueryResults = sProdQuery2Results;
                }
                #endregion
            }
        }

        private void QueryItemMaster()
        {
            string query = "SELECT PartName FROM ItemMaster WHERE PartNumber = '" + txtPartNumber.Text + "';";
            string sItemMasterQueryResults = csCrossClassInteraction.ItemMasterQuery(query);

            if (!string.IsNullOrEmpty(sItemMasterQueryResults))
            {
                txtPartName.Text = sItemMasterQueryResults;
                sVar.LogHandler.CreateLogAction("txtPartName's value has been set to " + txtPartName.Text + ".", csLogging.LogState.NOTE);
            }
        }

        private void QueryTechReport()
        {
            string query = "SELECT DateSubmitted, Technician, ID FROM TechnicianSubmission WHERE SerialNumber = '" + txtBarcode.Text + "';";
            sVar.LogHandler.CreateLogAction("Querying the Tech Report for previous tech information from the TechnicianSubmission table.", csLogging.LogState.NOTE);
            sVar.LogHandler.CreateLogAction("SQL QUERY: " + query, csLogging.LogState.SQLQUERY);
            csCrossClassInteraction.dgTechReport(query, false, dgPrevRepairInfo, txtBarcode.Text);

        }

        /// <summary>
        /// Fills txtCommSubClass based on the given Part Number
        /// </summary>
        private void fillCommoditySubClass()
        {
            string query = $"SELECT CommodityClass FROM ItemMaster WHERE PartNumber = '{txtPartNumber.Text.TrimEnd()}'";
            sVar.LogHandler.CreateLogAction("Attempting to fill the Commodity Sub-Class.", csLogging.LogState.NOTE);
            csCrossClassInteraction.txtFillFromQuery(query, txtCommSubClass);

            if (txtSeries.Text.Contains("XDR"))
            {
                ucEOLTab.lblEOL.Content = "Bench Test";
                ucEOLTab.lblPOST.Content = "Final Test";

                ucEOLTab.Fill();
            }
        }

        /// <summary>
        /// Fills txtSWVersion based on the given Serial Number
        /// </summary>
        private void fillSoftwareVersion()
        {
            string query = $"SELECT TOP(5) SoftwareVersion FROM tblPost WHERE PCBSerial = '{txtBarcode.Text.TrimEnd()}' ORDER BY [DateAndTime] DESC";
            sVar.LogHandler.CreateLogAction("Attempting to fill the Software Version.", csLogging.LogState.NOTE);
            csCrossClassInteraction.txtFillFromQuery(query, txtSWVersion);
            txtSWVersion.Text = txtSWVersion.Text.Split(',')[0];
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

        private bool checkForRefDesPartRep()
        {
            bool bCanSubmit = true;
            string sWarning = "Submission Criteria not met.\n";

            #region Unit Issue 1
            if (!string.IsNullOrEmpty(txtPartReplaced.Text))
            {
                string _sPRPD = csCrossClassInteraction.GetPartReplacedPartDescription(txtPartReplaced.Text);

                if (string.IsNullOrEmpty(_sPRPD))
                {
                    sWarning += string.Format("The Part Replaced entered into Unit Issue #1 ( {0} ) does not exist. Please verify the Part Number and try again.\n", txtPartReplaced.Text);
                    bCanSubmit = false;
                }
                else
                {
                    MultiplePartsReplaced mpr = new MultiplePartsReplaced { RefDesignator = txtRefDes.Text, PartReplaced = txtPartReplaced.Text, PartsReplacedPartDescription = _sPRPD };
                    dgMultipleParts.Items.Add(mpr);
                    txtPartReplaced.Text = txtRefDes.Text = _sPRPD = string.Empty;
                }
            }
            else if (!string.IsNullOrEmpty(txtRefDes.Text))
            {
                MultiplePartsReplaced mpr = new MultiplePartsReplaced { RefDesignator = txtRefDes.Text, PartReplaced = string.Empty, PartsReplacedPartDescription = string.Empty };
                dgMultipleParts.Items.Add(mpr);
                txtRefDes.Text = string.Empty;
            }
            #endregion

            #region Unit Issue 2
            if (!string.IsNullOrEmpty(txtPartReplaced_2.Text))
            {
                string _sPRPD = csCrossClassInteraction.GetPartReplacedPartDescription(txtPartReplaced_2.Text);

                if (string.IsNullOrEmpty(_sPRPD))
                {
                    sWarning += string.Format("The Part Replaced entered into Unit Issue #2 ( {0} ) does not exist. Please verify the Part Number and try again.\n", txtPartReplaced_2.Text);
                    bCanSubmit = false;
                }
                else
                {
                    MultiplePartsReplaced mpr = new MultiplePartsReplaced { RefDesignator = txtRefDes_2.Text, PartReplaced = txtPartReplaced_2.Text, PartsReplacedPartDescription = _sPRPD };
                    dgMultipleParts_2.Items.Add(mpr);
                    txtPartReplaced_2.Text = txtRefDes_2.Text = _sPRPD = string.Empty;
                }
            }
            else if (!string.IsNullOrEmpty(txtRefDes_2.Text))
            {
                MultiplePartsReplaced mpr = new MultiplePartsReplaced { RefDesignator = txtRefDes_2.Text, PartReplaced = string.Empty, PartsReplacedPartDescription = string.Empty };
                dgMultipleParts_2.Items.Add(mpr);
                txtRefDes_2.Text = string.Empty;
            }
            #endregion

            #region Unit Issue 3
            if (!string.IsNullOrEmpty(txtPartReplaced_3.Text))
            {
                string _sPRPD = csCrossClassInteraction.GetPartReplacedPartDescription(txtPartReplaced_3.Text);

                if (string.IsNullOrEmpty(_sPRPD))
                {
                    sWarning += string.Format("The Part Replaced entered into Unit Issue #3 ( {0} ) does not exist. Please verify the Part Number and try again.\n", txtPartReplaced_3.Text);
                    bCanSubmit = false;
                }
                else
                {
                    MultiplePartsReplaced mpr = new MultiplePartsReplaced { RefDesignator = txtRefDes_3.Text, PartReplaced = txtPartReplaced_3.Text, PartsReplacedPartDescription = _sPRPD };
                    dgMultipleParts_3.Items.Add(mpr);
                }
            }
            else if (!string.IsNullOrEmpty(txtRefDes_3.Text))
            {
                MultiplePartsReplaced mpr = new MultiplePartsReplaced { RefDesignator = txtRefDes_3.Text, PartReplaced = string.Empty, PartsReplacedPartDescription = string.Empty };
                dgMultipleParts_3.Items.Add(mpr);
                txtRefDes_3.Text = string.Empty;
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
        /// This will ensure that the criteria for submission has been met before attempting to submit.
        /// </summary>
        /// <returns>An array with true/false based on if the submission criteria has been found and a string of all missing items if the criteria has not been met.</returns>
        private ArrayList canSubmit()
        {
            ArrayList alReturn = new ArrayList(2);
            string sErrMsg = "The following items are missing:\n";

            //NOTE: Removed any restrictions to submitting a credit return. Add a few if it becomes a problem.
            if (cbTOR.Text.Equals("Credit Return"))
            {
                alReturn.Add(true);
                alReturn.Add("CR");
            }
            else
            {
                bool bSubmit = true;

                if (!ckbxSNUnavailable.IsChecked.Value && string.IsNullOrEmpty(txtBarcode.Text) && !sUserDepartmentNumber.Equals(sDQE_DeptNum))
                {
                    bSubmit = false;
                    sErrMsg += "- Serial Number is empty\n";
                }

                if (string.IsNullOrEmpty(txtOrderNumber.Text) && !sUserDepartmentNumber.Equals(sDQE_DeptNum))
                {
                    bSubmit = false;
                    sErrMsg += "- Order Number\n";
                }

                if (string.IsNullOrEmpty(cbTechAction1.Text) && !sUserDepartmentNumber.Equals(sDQE_DeptNum))
                {
                    bSubmit = false;
                    sErrMsg += "- At least 1 Technician Action\n";
                }

                if (string.IsNullOrEmpty(txtCustomerNumber.Text) && !sUserDepartmentNumber.Equals(sDQE_DeptNum))
                {
                    bSubmit = false;
                    sErrMsg += "- Customer Information\n";
                }

                if (string.IsNullOrEmpty(cbTOF.Text))
                {
                    bSubmit = false;
                    sErrMsg += "- Type of Failure\n";
                }

                if (string.IsNullOrEmpty(txtPartName.Text))
                {
                    bSubmit = false;
                    sErrMsg += "- Part Name\n";
                }

                if (string.IsNullOrEmpty(txtPartNumber.Text))
                {
                    bSubmit = false;
                    sErrMsg += "- Part Number\n";
                }

                if (string.IsNullOrEmpty(txtCommSubClass.Text))
                {
                    bSubmit = false;
                    sErrMsg += "- Commodity Sub-Class\n";
                }

                if (string.IsNullOrEmpty(cbTOR.Text))
                {
                    bSubmit = false;
                    sErrMsg += "- Type of Return\n";
                }


                bool bUnitIssueFound = false;
                if (!string.IsNullOrEmpty(cbReportedIssue.Text))
                    bUnitIssueFound = true;
                if (!string.IsNullOrEmpty(cbTestResult.Text))
                    bUnitIssueFound = true;
                if (!string.IsNullOrEmpty(cbTestResultAbort.Text))
                    bUnitIssueFound = true;
                if (!string.IsNullOrEmpty(cbCause.Text))
                    bUnitIssueFound = true;
                if (!string.IsNullOrEmpty(cbReplacement.Text))
                    bUnitIssueFound = true;

                if (!bUnitIssueFound)
                {
                    sErrMsg += "- At least one full unit issue";
                    bSubmit = false;
                }

                alReturn.Add(bSubmit);
                alReturn.Add(sErrMsg);
            }

            return alReturn;
        }

        /// <summary>
        /// Submits the form to the database then resets the form.
        /// </summary>
        private bool submitData(SubmissionStatus subStatus, long saveID, bool bIsCR)
        {
            string dtSubmission = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt");

            string sLogString = "Attemping to submit the data with the following parameters to the TechnicianSubmission Table:\n";

            string insertQuery = @"INSERT INTO TechnicianSubmission (Technician, DateReceived, PartName, PartNumber, CommoditySubClass, Quantity, SoftwareVersion, Scrap, TypeOfReturn, " +
                                  "TypeOfFailure, HoursOnUnit, ReportedIssue, TestResult, TestResultAbort, Cause, Replacement, PartsReplaced, RefDesignator, AdditionalComments, CustomerNumber, SerialNumber, DateSubmitted, " +
                                  "SubmissionStatus, SaveID, RP, TechAct1, TechAct2, TechAct3, OrderNumber, LineNumber, ProblemCode1, ProblemCode2, RepairCode, TechComments, Series, SerialNumberUnavailable) " +
                                  "VALUES (@Technician, @DateReceived, @PartName, @PartNumber, @CommoditySubClass, @Quantity, @SoftwareVersion, @Scrap, @TypeOfReturn, " +
                                  "@TypeOfFailure, @HoursOnUnit, @ReportedIssue, @TestResult, @TestResultAbort, @Cause, @Replacement, @PartsReplaced, @RefDesignator, @AdditionalComments, @CustomerNumber, " +
                                  "@SerialNumber, @DateSubmitted, @SubmissionStatus, @SaveID, @RP, @TechAct1, @TechAct2, @TechAct3, @OrderNumber, @LineNumber, @pc1, @pc2, @rc, @tc, @series, @snUnavailable)";

            SqlConnection conn = new SqlConnection(holder.RepairConnectionString);
            SqlCommand cmd = new SqlCommand(insertQuery, conn);
            try
            {
                conn.Open();

                cmd.Parameters.AddWithValue("@Technician", txtTechName.Text.ToString().TrimEnd());
                cmd.Parameters.AddWithValue("@DateReceived",
                    dtpDateReceived.SelectedDate?.ToString("MM/dd/yyyy") ?? DateTime.Now.AddDays(-1.0).ToString());
                cmd.Parameters.AddWithValue("@PartName", txtPartName.Text.ToString().TrimEnd());
                cmd.Parameters.AddWithValue("@PartNumber", txtPartNumber.Text.ToString().TrimEnd());
                cmd.Parameters.AddWithValue("@CommoditySubClass", txtCommSubClass.Text.ToString().TrimEnd());

                if (!string.IsNullOrEmpty(txtQTY.Text))
                    cmd.Parameters.AddWithValue("@Quantity", Convert.ToInt32(txtQTY.Text));
                else cmd.Parameters.AddWithValue("@Quantity", 1);

                cmd.Parameters.AddWithValue("@SoftwareVersion", txtSWVersion.Text.ToString());
                cmd.Parameters.AddWithValue("@Scrap", cbxScrap.IsChecked);
                cmd.Parameters.AddWithValue("@TypeOfReturn", cbTOR.Text.ToString());
                cmd.Parameters.AddWithValue("@TypeOfFailure", cbTOF.Text.ToString());

                if (string.IsNullOrEmpty(txtHOU.Text))
                    cmd.Parameters.AddWithValue("@HoursOnUnit", DBNull.Value);
                else cmd.Parameters.AddWithValue("@HoursOnUnit", Convert.ToInt32(txtHOU.Text));

                cmd.Parameters.AddWithValue("@ReportedIssue", cbReportedIssue.Text.EmptyIfNull());

                #region Unit Issues
                UnitIssueModel lUI = getUnitIssueString(0);
                cmd.Parameters.AddWithValue("@TestResult", csCrossClassInteraction.EmptyIfNull(lUI.TestResult));
                cmd.Parameters.AddWithValue("@TestResultAbort", csCrossClassInteraction.EmptyIfNull(lUI.TestResultAbort));
                cmd.Parameters.AddWithValue("@Cause", csCrossClassInteraction.EmptyIfNull(lUI.Cause));
                cmd.Parameters.AddWithValue("@Replacement", csCrossClassInteraction.EmptyIfNull(lUI.Replacement));
                cmd.Parameters.AddWithValue("@PartsReplaced", csCrossClassInteraction.EmptyIfNull(lUI.MultiPartsReplaced[0].PartReplaced));
                cmd.Parameters.AddWithValue("@RefDesignator", csCrossClassInteraction.EmptyIfNull(lUI.MultiPartsReplaced[0].RefDesignator));
                #endregion

                cmd.Parameters.AddWithValue("@AdditionalComments", new TextRange(rtbAdditionalComments.Document?.ContentStart, rtbAdditionalComments.Document?.ContentEnd)
                    ?.Text.ToString() ?? "");

                if (string.IsNullOrEmpty(txtCustomerNumber.Text))
                    cmd.Parameters.AddWithValue("@CustomerNumber", DBNull.Value);
                else cmd.Parameters.AddWithValue("@CustomerNumber", Convert.ToInt32(txtCustomerNumber.Text));

                cmd.Parameters.AddWithValue("@SerialNumber", txtBarcode.Text.ToString());
                cmd.Parameters.AddWithValue("@DateSubmitted", dtSubmission);
                cmd.Parameters.AddWithValue("@SubmissionStatus", subStatus.ToString());
                cmd.Parameters.AddWithValue("@SaveID", saveID.ToString());
                cmd.Parameters.AddWithValue("@RP", sRPNum);
                cmd.Parameters.AddWithValue("@TechAct1", cbTechAction1.Text);
                cmd.Parameters.AddWithValue("@TechAct2", cbTechAction2.Text);
                cmd.Parameters.AddWithValue("@TechAct3", cbTechAction3.Text);

                if (string.IsNullOrEmpty(txtOrderNumber.Text) || sUserDepartmentNumber.Equals(sDQE_DeptNum))
                    cmd.Parameters.AddWithValue("@OrderNumber", string.Empty);
                else cmd.Parameters.AddWithValue("@OrderNumber", txtOrderNumber.Text);

                if (dLineNumber >= 1)
                    cmd.Parameters.AddWithValue("@LineNumber", dLineNumber);
                else cmd.Parameters.AddWithValue("@LineNumber", 1.0);

                if (sUserDepartmentNumber.Equals(sDQE_DeptNum))
                    cmd.Parameters.AddWithValue("@pc1", "");
                else cmd.Parameters.AddWithValue("@pc1", getRepairCodes("PC1", cbReportedIssue.Text, bIsCR));

                if (string.IsNullOrEmpty(cbTechAction1.Text) || sUserDepartmentNumber.Equals(sDQE_DeptNum))
                    cmd.Parameters.AddWithValue("@pc2", "");
                else cmd.Parameters.AddWithValue("@pc2", getRepairCodes("PC2", cbTechAction1.Text, bIsCR));

                if (string.IsNullOrEmpty(cbTechAction2.Text) || sUserDepartmentNumber.Equals(sDQE_DeptNum))
                    cmd.Parameters.AddWithValue("@rc", "");
                else cmd.Parameters.AddWithValue("@rc", getRepairCodes("PC3", cbTechAction2.Text, bIsCR));

                if (string.IsNullOrEmpty(cbTechAction3.Text) || sUserDepartmentNumber.Equals(sDQE_DeptNum))
                    cmd.Parameters.AddWithValue("@tc", "");
                else cmd.Parameters.AddWithValue("@tc", getRepairCodes("EndUse", cbTechAction3.Text, bIsCR));

                if (string.IsNullOrEmpty(txtSeries.Text))
                    cmd.Parameters.AddWithValue("@series", DBNull.Value);
                else cmd.Parameters.AddWithValue("@series", txtSeries.Text.ToString());

                cmd.Parameters.AddWithValue("@logid", DateTime.Now.Ticks);
                cmd.Parameters.AddWithValue("@snUnavailable", ckbxSNUnavailable.IsChecked.Value);

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

                int readerID = csCrossClassInteraction.GetDBIDValue("SELECT ID FROM TechnicianSubmission WHERE Technician = '" + txtTechName.Text + "' AND DateSubmitted = '" + dtSubmission + "' AND SerialNumber = '" + txtBarcode.Text + "';");
                if (readerID > 0)
                {
                    submitMultipleUnitData(readerID);
                }

                return true;
            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();

                MessageBox.Show("There was an error attempting to submit the data.\n\nError Message: " + ex.Message, "submitData()", MessageBoxButton.OK, MessageBoxImage.Error);
                sVar.LogHandler.CreateLogAction("Error attempting to submit the data.\nError Message: " + ex.Message + "\nStack Trace: " + ex.StackTrace, csLogging.LogState.ERROR);
                return false;
            }
        }

        private string getRepairCodes(string _codeType, string _codeName, bool bCR)
        {
            string sReturnCode = "";

            //--This is to bypass checking for code names if the tech does a credit return.
            if (bCR && string.IsNullOrEmpty(_codeName))
                return string.Empty;

            if (_codeType.Equals("PC1"))
            {
                PC1 rc = lPC1.Find(x => x.CodeName.ToLower() == _codeName.ToLower());
                sReturnCode = rc.ReturnCode;
            }
            else if (_codeType.Equals("PC2"))
            {
                PC2 rc = lPC2.Find(x => x.CodeName.ToLower() == _codeName.ToLower());
                sReturnCode = rc.ReturnCode;
            }
            else if (_codeType.Equals("PC3"))
            {
                PC3 rc = lPC3.Find(x => x.CodeName.ToLower() == _codeName.ToLower());
                sReturnCode = rc.ReturnCode;
            }
            else if (_codeType.Equals("EndUse"))
            {
                EndUse rc = lEndUse.Find(x => x.CodeName.ToLower() == _codeName.ToLower());
                sReturnCode = rc.ReturnCode;
            }
            return sReturnCode;
        }

        private void submitMultipleUnitData(int id)
        {
            string query = @"INSERT INTO TechnicianUnitIssues (ID, PartNumber, PartName, CommoditySubClass, ReportedIssue, TestResult, TestResultAbort, Cause, Replacement, PartsReplaced, RefDesignator, PartsReplacedPartDescription) " +
                                    "VALUES (@id, @partNumber, @partName, @commoditySubClass, @reportedIssue, @testResult, @testResultAbort, @cause, @replacement, @partsReplaced, @refDesignator, @prpd)";
            sVar.LogHandler.CreateLogAction("Submitting the Unit Issues to the TechnicianUnitIssues Table.", csLogging.LogState.NOTE);
            string sLogString = "";
            List<UnitIssueModel> lRMI = getUnitIssues();

            SqlConnection conn = new SqlConnection(holder.RepairConnectionString);
            SqlCommand cmd = new SqlCommand(query, conn);
            try
            {
                for (int i = 0; i < lRMI.Count; i++)
                {
                    if (lRMI[i].MultiPartsReplaced.Count == 0)
                    {
                        sLogString = "Attempting to submit the following data to the TechnicianUnitIssues Table:\n";

                        cmd = new SqlCommand(query, conn);
                        conn.Open();

                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@partNumber", txtPartNumber.Text);
                        cmd.Parameters.AddWithValue("@partName", txtPartName.Text);
                        cmd.Parameters.AddWithValue("@commoditySubClass", txtCommSubClass.Text);
                        cmd.Parameters.AddWithValue("@reportedIssue", cbReportedIssue.Text);
                        cmd.Parameters.AddWithValue("@testResult", csCrossClassInteraction.unitStripNF(lRMI[i].TestResult));
                        cmd.Parameters.AddWithValue("@testResultAbort", csCrossClassInteraction.unitStripNF(lRMI[i].TestResultAbort));
                        cmd.Parameters.AddWithValue("@cause", csCrossClassInteraction.unitStripNF(lRMI[i].Cause));
                        cmd.Parameters.AddWithValue("@replacement", csCrossClassInteraction.unitStripNF(lRMI[i].Replacement));
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
                        for (int j = 0; j < lRMI[i].MultiPartsReplaced.Count; j++)
                        {
                            sLogString = "Attempting to submit the following data to the TechnicianUnitIssues Table:\n";

                            cmd = new SqlCommand(query, conn);
                            conn.Open();

                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.Parameters.AddWithValue("@partNumber", txtPartNumber.Text);
                            cmd.Parameters.AddWithValue("@partName", txtPartName.Text);
                            cmd.Parameters.AddWithValue("@commoditySubClass", txtCommSubClass.Text);
                            cmd.Parameters.AddWithValue("@reportedIssue", cbReportedIssue.Text);
                            cmd.Parameters.AddWithValue("@testResult", csCrossClassInteraction.unitStripNF(lRMI[i].TestResult));
                            cmd.Parameters.AddWithValue("@testResultAbort", csCrossClassInteraction.unitStripNF(lRMI[i].TestResultAbort));
                            cmd.Parameters.AddWithValue("@cause", csCrossClassInteraction.unitStripNF(lRMI[i].Cause));
                            cmd.Parameters.AddWithValue("@replacement", csCrossClassInteraction.unitStripNF(lRMI[i].Replacement));
                            cmd.Parameters.AddWithValue("@partsReplaced", csCrossClassInteraction.unitStripNF(lRMI[i].MultiPartsReplaced[j].PartReplaced));
                            cmd.Parameters.AddWithValue("@refDesignator", csCrossClassInteraction.unitStripNF(lRMI[i].MultiPartsReplaced[j].RefDesignator));
                            cmd.Parameters.AddWithValue("@prpd", csCrossClassInteraction.unitStripNF(lRMI[i].MultiPartsReplaced[j].PartsReplacedPartDescription));
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
                sVar.LogHandler.CreateLogAction(sErr + "\nStack Trace: " + ex.StackTrace, csLogging.LogState.ERROR);
            }

        }

        /// <summary>
        /// To be used when submitting basic information to the TechnicianSubmission table
        /// </summary>
        private UnitIssueModel getUnitIssueString(int iUIData)
        {
            UnitIssueModel pmuiReturn = new UnitIssueModel();

            if (tiUI1.IsEnabled && (iUIData == 0 || iUIData == 1)) //-Will always be enabled but doing 'if' to be uniform
            {
                pmuiReturn.TestResult += csCrossClassInteraction.unitIssuesValSubmit(cbTestResult);
                pmuiReturn.TestResultAbort += csCrossClassInteraction.unitIssuesValSubmit(cbTestResultAbort);
                pmuiReturn.Cause += csCrossClassInteraction.unitIssuesValSubmit(cbCause);
                pmuiReturn.Replacement += csCrossClassInteraction.unitIssuesValSubmit(cbReplacement);
            }

            if (tiUI2.IsEnabled && checkForUITabData(2) && (iUIData == 0 || iUIData == 2))
            {
                pmuiReturn.TestResult += csCrossClassInteraction.unitIssuesValSubmit(cbTestResult_2);
                pmuiReturn.TestResultAbort += csCrossClassInteraction.unitIssuesValSubmit(cbTestResultAbort_2);
                pmuiReturn.Cause += csCrossClassInteraction.unitIssuesValSubmit(cbCause_2);
                pmuiReturn.Replacement += csCrossClassInteraction.unitIssuesValSubmit(cbReplacement_2);
            }

            if (tiUI3.IsEnabled && checkForUITabData(3) && (iUIData == 0 || iUIData == 3))
            {
                pmuiReturn.TestResult += csCrossClassInteraction.unitIssuesValSubmit(cbTestResult_3);
                pmuiReturn.TestResultAbort += csCrossClassInteraction.unitIssuesValSubmit(cbTestResultAbort_3);
                pmuiReturn.Cause += csCrossClassInteraction.unitIssuesValSubmit(cbCause_3);
                pmuiReturn.Replacement += csCrossClassInteraction.unitIssuesValSubmit(cbReplacement_3);
            }

            pmuiReturn.TestResult = pmuiReturn.TestResult.TrimEnd(new char[] { ',', ' ' });
            pmuiReturn.TestResultAbort = pmuiReturn.TestResultAbort.TrimEnd(new char[] { ',', ' ' });
            pmuiReturn.Cause = pmuiReturn.Cause.TrimEnd(new char[] { ',', ' ' });
            pmuiReturn.Replacement = pmuiReturn.Replacement.TrimEnd(new char[] { ',', ' ' });
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
            MultiplePartsReplaced mpr = new MultiplePartsReplaced();

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
            List<MultiplePartsReplaced> mpr = new List<MultiplePartsReplaced>(); //NOTE: Houses the entire list of MultiplePartsReplaced.

            if (tiUI1.IsEnabled && iUIData == 1)
            {
                IEnumerable<MultiplePartsReplaced> res = mpr.Concat(csCrossClassInteraction.getMPRList(dgMultipleParts));
                mpr = res.ToList();
            }

            if (tiUI2.IsEnabled && checkForUITabData(2) && iUIData == 2)
            {
                IEnumerable<MultiplePartsReplaced> res = mpr.Concat(csCrossClassInteraction.getMPRList(dgMultipleParts_2));
                mpr = res.ToList();
            }

            if (tiUI3.IsEnabled && checkForUITabData(3) && iUIData == 3)
            {
                IEnumerable<MultiplePartsReplaced> res = mpr.Concat(csCrossClassInteraction.getMPRList(dgMultipleParts_3));
                mpr = res.ToList();
            }

            return mpr;
        }

        /// <summary>
        /// To be used when submitting individual items to the TechnicianUnitIssues table
        /// </summary>
        private List<UnitIssueModel> getUnitIssues()
        {
            List<UnitIssueModel> lMPUI = new List<UnitIssueModel>
            {
                getUnitIssueString(1)
            };

            if (checkForUITabData(2))
                lMPUI.Add(getUnitIssueString(2));

            if (checkForUITabData(3))
                lMPUI.Add(getUnitIssueString(3));

            return lMPUI;
        }

        private void refDesIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is Control c && c.Name.Contains("_"))
            {
                if (c.Name.EndsWith("2"))
                {
                    txtPartReplaced_2.SelectedIndex = txtRefDes_2.SelectedIndex;
                }
                else if (c.Name.EndsWith("3"))
                {
                    txtPartReplaced_3.SelectedIndex = txtRefDes_3.SelectedIndex;
                }
            }
            else
            {
                txtPartReplaced.SelectedIndex = txtRefDes.SelectedIndex;
            }
        }

        private static readonly CancellationTokenSource MapperTokenSource = new CancellationTokenSource();

        /// <summary> Makes calls to <see cref="csSerialNumberMapper"/> methods. </summary>
        private async void MapRefDesToPartNum()
        {
            try
            {
                using (SNM mapper = SNM.Instance)
                {
                    await Task.Factory.StartNew(
                        new Action(() => {
                            tabcUnitIssues.Dispatcher.BeginInvoke(new Action(async () => // perform actions on dispatched thread
                            {
                                if (!mapper.GetData(txtBarcode.Text))
                                {
                                    MessageBox.Show("Couldn't find the barcode's entry in the database.\nPlease enter information manually.",
                                        "Soft Error - BOM Lookup"
                                        , MessageBoxButton.OK
                                        , MessageBoxImage.Warning);
                                }
                                else
                                {
                                    var filename = await mapper.TechFormProcessAsync(txtBarcode, txtPartNumber).ConfigureAwait(true);

                                    if (!File.Exists(filename)) return;

                                    (OrigRefSource, OrigPartSource) = csCrossClassInteraction.DoExcelOperations(filename, progMapper);

                                    if (!mapper.NoFilesFound)
                                        csCrossClassInteraction.MapperSuccessMessage(filename, mapper.PartNumber);

                                    txtRefDes.ItemsSource = OrigRefSource;
                                    txtRefDes_2.ItemsSource = OrigRefSource;
                                    txtRefDes_3.ItemsSource = OrigRefSource;

                                    txtPartReplaced.ItemsSource = OrigPartSource;
                                    txtPartReplaced_2.ItemsSource = OrigPartSource;
                                    txtPartReplaced_3.ItemsSource = OrigPartSource;

                                    BOMFileActive = true;
                                    CheckForManual();
                                }
                        }), DispatcherPriority.Background);
                    }),
                    MapperTokenSource.Token, 
                    TaskCreationOptions.LongRunning, 
                    TaskScheduler.Current)
                    .ConfigureAwait(true);
                }
            }
            catch (InvalidOperationException ioe)
            {
                csExceptionLogger.csExceptionLogger.DefaultLogLocation = "";
                csExceptionLogger.csExceptionLogger.Write("BadBarcode-MapRefDesToPartNum", ioe);
                return;
            }
            catch (TaskCanceledException tce)
            {
                csExceptionLogger.csExceptionLogger.DefaultLogLocation = "";
                csExceptionLogger.csExceptionLogger.Write("BadBarcode-MapRefDesToPartNum_TaskFaulted", tce);
                return;
            }
        }

        private void generateLabel(string sentWhere, long lID)
        {
            if (Properties.Settings.Default.PrinterInitSetup)
            {
                try
                {
                    csPrintQCDQELabel printLabel = new csPrintQCDQELabel(sentWhere, txtTechName.Text, DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"), lID.ToString());
                    printLabel.PrintLabel();
                }
                catch (Exception ex)
                {
                    string alert = "There was an issue printing the label.\nThe ID associated with this unit being sent to " + sentWhere + " is " + lID.ToString() + ".\nError Message: " + ex.Message;
                    MessageBox.Show(alert, "Printer Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    sVar.LogHandler.CreateLogAction(alert, csLogging.LogState.ERROR);
                }
            }
            else
            {
                string alert = "The printer has not been setup yet so the label could not be printed.\nThe ID associated with this unit being sent to " + sentWhere + " is " + lID.ToString() + ".";
                MessageBox.Show(alert, "Printer Not Setup", MessageBoxButton.OK, MessageBoxImage.Information);
                sVar.LogHandler.CreateLogAction(alert, csLogging.LogState.ERROR);
            }
        }

        private bool searchOrderNumber()
        {
            bool bOrderNumberFound = true;
            sVar.LogHandler.LogCreation = DateTime.Now;
            List<OrderNumberInformation> lOrderNumInfo = new List<OrderNumberInformation>();
            string query = "SELECT ItemNumber, CustomerNumber, LineNumber FROM CustomerRepairOrderFromJDE WHERE OrderNumber = '" + txtOrderNumber.Text.ToString().TrimEnd() + "'";
            sVar.LogHandler.CreateLogAction("Attempting to search CustomerRepairOrderFromJDE for the Order Number.\nSQL CMD: " + query, csLogging.LogState.NOTE);

            SqlConnection conn = new SqlConnection(holder.RepairConnectionString);
            SqlCommand cmd = new SqlCommand(query, conn);
            try
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        OrderNumberInformation oni = new OrderNumberInformation
                        {
                            OrderNumber = txtOrderNumber.Text.ToString().TrimEnd(),
                            RPNumber = reader[0].ToString().TrimEnd(),
                            CustomerNumber = reader[1].ToString().TrimEnd(),
                            LineNumber = Convert.ToDouble(reader[2])
                        };
                        lOrderNumInfo.Add(oni);
                    }
                }
                conn.Close();
                sVar.LogHandler.CreateLogAction("Query Successful. A total of " + lOrderNumInfo.Count.ToString() + " order number(s) were found.", csLogging.LogState.NOTE);
            }
            catch (Exception ex)
            {
                bOrderNumberFound = false;
                if (conn != null)
                    conn.Close();

                MessageBox.Show("There was an issue searching for the order number.\nError Message: " + ex.Message, "searchOrderNumber()", MessageBoxButton.OK, MessageBoxImage.Error);
                sVar.LogHandler.CreateLogAction("Error searching for the order number.\nError Message: " + ex.Message + "\nStack Trace: " + ex.StackTrace, csLogging.LogState.ERROR);

                lblRPNumber.Content = string.Empty;

                return bOrderNumberFound;
            }

            if (lOrderNumInfo.Count == 0)
            {
                string sErrMsg = string.Format("No information associated with the Order Number '{0}' could be found. Please verify you have entered the correct Order Number and try again.", txtOrderNumber.Text.ToString());
                MessageBox.Show(sErrMsg, "Order Number Issue", MessageBoxButton.OK, MessageBoxImage.Error);
                sVar.LogHandler.CreateLogAction(sErrMsg, csLogging.LogState.WARNING);
                lblRPNumber.Content = string.Empty;
                sRPNum = string.Empty;
                bOrderNumberFound = false;
            }
            else if (lOrderNumInfo.Count == 1)
            {
                string sDataMissing = "The following items were not loaded from the table CustomerRepairOrderFromJDE:\n\n";
                bool bTrueOrderData = true;
                if (lOrderNumInfo[0].LineNumber < 1.0)
                {
                    bTrueOrderData = false;
                    sDataMissing += "- Line Number\n";
                }
                if (string.IsNullOrEmpty(lOrderNumInfo[0].RPNumber.ToString()))
                {
                    bTrueOrderData = false;
                    sDataMissing += "- RP Number\n";
                }
                if (string.IsNullOrEmpty(lOrderNumInfo[0].CustomerNumber.ToString()))
                {
                    bTrueOrderData = false;
                    sDataMissing += "- Customer Number\n";
                }
                sDataMissing += "\nPlease wait 15 minutes and try again. If you have seen this same message related to this order number more than once, please let your supervisor know.";

                if (bTrueOrderData)
                {
                    lblRPNumber.Content = "RP Number: " + lOrderNumInfo[0].RPNumber;
                    sRPNum = lOrderNumInfo[0].RPNumber;
                    loadCustomerInformation(lOrderNumInfo[0].CustomerNumber);
                    dLineNumber = lOrderNumInfo[0].LineNumber;
                    string sOrdNumDet = string.Format("Order Number Details:\nRP Number: {0}\nCustomer Number: {1}\nLine Number: {2}", lOrderNumInfo[0].RPNumber.ToString(), lOrderNumInfo[0].CustomerNumber.ToString(), lOrderNumInfo[0].LineNumber.ToString());
                    sVar.LogHandler.CreateLogAction(sOrdNumDet, csLogging.LogState.NOTE);
                }
                else
                {
                    MessageBox.Show(sDataMissing, "Missing Order Information", MessageBoxButton.OK, MessageBoxImage.Error);
                    sVar.LogHandler.CreateLogAction(sDataMissing, csLogging.LogState.WARNING);
                    bOrderNumberFound = false;
                }
            }
            else
            {
                sVar.LogHandler.CreateLogAction("Multiple RP Numbers Found. Opening frmMultipleRP to let user pick the correct RP Number.", csLogging.LogState.NOTE);
                frmMultipleItems fmrp = new frmMultipleItems(txtOrderNumber.Text.ToString().TrimEnd());
                fmrp.ShowDialog();
                if (!string.IsNullOrEmpty(sVar.SelectedRPNumber.RPNumber))
                {
                    string sDataMissing = "The following items were not loaded from the table CustomerRepairOrderFromJDE:\n";
                    bool bTrueOrderData = true;
                    if (string.IsNullOrEmpty(sVar.SelectedRPNumber.LineNumber.ToString()))
                    {
                        bTrueOrderData = false;
                        sDataMissing += "- Line Number\n";
                    }
                    if (string.IsNullOrEmpty(sVar.SelectedRPNumber.RPNumber.ToString()))
                    {
                        bTrueOrderData = false;
                        sDataMissing += "- RP Number\n";
                    }
                    if (string.IsNullOrEmpty(sVar.SelectedRPNumber.CustInfo.CustomerNumber.ToString()))
                    {
                        bTrueOrderData = false;
                        sDataMissing += "- Customer Number\n";
                    }
                    sDataMissing += "Please wait 15 minutes and try again. If you have seen this same message related to this order number more than once, please let your supervisor know.";

                    if (bTrueOrderData)
                    {
                        lblRPNumber.Content = "RP Number: " + sVar.SelectedRPNumber.RPNumber.ToString();
                        sRPNum = sVar.SelectedRPNumber.RPNumber;
                        dLineNumber = sVar.SelectedRPNumber.LineNumber;
                        fillCustomerData(sVar.SelectedRPNumber.CustInfo);
                        sVar.LogHandler.CreateLogAction("Order Number Details:\nRP Number: " + sRPNum.ToString() + "\nCustomer Number: " + sVar.SelectedRPNumber.CustInfo.CustomerNumber + "\nLine Number: " + sVar.SelectedRPNumber.LineNumber, csLogging.LogState.NOTE);
                    }
                    else
                    {
                        MessageBox.Show(sDataMissing, "Missing Order Information", MessageBoxButton.OK, MessageBoxImage.Error);
                        sVar.LogHandler.CreateLogAction(sDataMissing, csLogging.LogState.WARNING);
                        bOrderNumberFound = false;
                    }

                }
                else
                {
                    string sNoSelection = "A RP Number was not selected. Please re-scan the order number and select the appropriate RP Number associated with the order number.";
                    MessageBox.Show(sNoSelection, "RP Missing", MessageBoxButton.OK, MessageBoxImage.Error);
                    sVar.LogHandler.CreateLogAction(sNoSelection, csLogging.LogState.WARNING);
                    bOrderNumberFound = false;
                }
            }
            return bOrderNumberFound;
        }

        private void loadCustomerInformation(string sCustNum)
        {
            CustomerInformation cInfo = new CustomerInformation();
            string query = "SELECT * FROM CustomerRepairOrderFromJDE WHERE CustomerNumber = '" + sCustNum + "'";
            sVar.LogHandler.CreateLogAction("Attempting to load the Customer Information from CustomerRepairOrderFromJDE.\nSQL CMD: " + query, csLogging.LogState.NOTE);
            SqlConnection conn = new SqlConnection(holder.RepairConnectionString);
            SqlCommand cmd = new SqlCommand(query, conn);
            try
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cInfo.CustomerNumber = sCustNum;
                        cInfo.CustomerName = reader["CustomerName"].ToString();
                        cInfo.CustomerAddy1 = reader["CustomerAddressLine1"].ToString();
                        cInfo.CustomerAddy2 = reader["CustomerAddressLine2"].ToString();
                        cInfo.CustomerAddy3 = reader["CustomerAddressLine3"].ToString();
                        cInfo.CustomerAddy4 = reader["CustomerAddressLine4"].ToString();
                        cInfo.CustomerCity = reader["CustomerCity"].ToString();
                        cInfo.CustomerState = reader["CustomerState"].ToString();
                        cInfo.CustomerPostalCode = reader["CustomerPostalCode"].ToString();
                        cInfo.CustomerCountryCode = reader["CustomerCountryCode"].ToString();
                    }
                }
                conn.Close();
                sVar.LogHandler.CreateLogAction("Query Successful.", csLogging.LogState.NOTE);
                sVar.LogHandler.CreateLogAction("Filling Customer Information Section now.", csLogging.LogState.NOTE);
                fillCustomerData(cInfo);

            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();
                MessageBox.Show("There was an issue loading the Customer Information.\nError Message: " + ex.Message, "loadCustomerInformation()", MessageBoxButton.OK, MessageBoxImage.Error);
                sVar.LogHandler.CreateLogAction("Error loading Customer Information.\nError Message" + ex.Message + "\nStack Trace: " + ex.StackTrace, csLogging.LogState.ERROR);
            }
        }

        private void fillCustomerData(CustomerInformation cInfo)
        {
            resetCustomerInformation();

            txtCustomerNumber.Text = cInfo.CustomerNumber;
            txtCustomerName.Text = cInfo.CustomerName;
            txtCustAddy1.Text = cInfo.CustomerAddy1;
            txtCustAddy2.Text = cInfo.CustomerAddy2;
            txtCustAddy3.Text = cInfo.CustomerAddy3;
            txtCustAddy4.Text = cInfo.CustomerAddy4;
            txtCustCity.Text = cInfo.CustomerCity;
            txtCustState.Text = cInfo.CustomerState;
            txtCustPostal.Text = cInfo.CustomerPostalCode;
            txtCustCountryCode.Text = cInfo.CustomerCountryCode;

            addCustomerToDB(cInfo);
        }

        private void addCustomerToDB(CustomerInformation cInfo)
        {
            bool bExistingCustomer = false;
            bool bNoSearchError = true;

            #region Check To See If Customer Exists

            string query = "SELECT CustomerNumber FROM RepairCustomerInformation WHERE CustomerNumber = '" + cInfo.CustomerNumber + "'";
            SqlConnection conn = new SqlConnection(holder.RepairConnectionString);
            SqlCommand cmd = new SqlCommand(query, conn);
            try
            {
                string _sCustTest = string.Empty;
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        _sCustTest = reader[0].ToString();
                    }
                }
                conn.Close();

                if (!string.IsNullOrEmpty(_sCustTest))
                    bExistingCustomer = true;
            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();

                sVar.LogHandler.CreateLogAction("Error checking for customer information.\nError Message: " + ex.Message, csLogging.LogState.ERROR);
                bNoSearchError = false;
            }

            #endregion

            if (bNoSearchError)
            {
                if (!bExistingCustomer)
                {
                    sVar.LogHandler.CreateLogAction("Adding new customer to the database...", csLogging.LogState.NOTE);
                    query = "INSERT INTO RepairCustomerInformation (CustomerNumber, CustomerName, CustomerAddressLine1, CustomerAddressLine2, CustomerAddressLine3, CustomerAddressLine4, CustomerCity, CustomerState, CustomerPostalCode, CustomerCountryCode) " +
                                             "VALUES (@cNum, @cName, @cA1, @cA2, @cA3, @cA4, @cCity, @cState, @cPostCode, @cCountryCode)";
                    cmd = new SqlCommand(query, conn);
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@cNum", cInfo.CustomerNumber);
                        cmd.Parameters.AddWithValue("@cName", cInfo.CustomerName);
                        cmd.Parameters.AddWithValue("@cA1", cInfo.CustomerAddy1);
                        cmd.Parameters.AddWithValue("@cA2", cInfo.CustomerAddy2);
                        cmd.Parameters.AddWithValue("@cA3", cInfo.CustomerAddy3);
                        cmd.Parameters.AddWithValue("@cA4", cInfo.CustomerAddy4);
                        cmd.Parameters.AddWithValue("@cCity", cInfo.CustomerCity);
                        cmd.Parameters.AddWithValue("@cState", cInfo.CustomerState);
                        cmd.Parameters.AddWithValue("@cPostCode", cInfo.CustomerPostalCode);
                        cmd.Parameters.AddWithValue("@cCountryCode", cInfo.CustomerCountryCode);
                        cmd.ExecuteNonQuery();
                        conn.Close();

                        var sLogString = "Inserting new record into the RepairCustomerInformation table.\n";
                        for (int pCount = 0; pCount < cmd.Parameters.Count; pCount++)
                        {
                            var paramName = cmd.Parameters[pCount].ToString();
                            if (string.IsNullOrEmpty(cmd.Parameters[pCount].Value.ToString()))
                                sLogString += paramName + ": N/A\n";
                            else sLogString += $"{paramName}: {cmd.Parameters[pCount].Value.ToString()}\n";
                        }
                        sVar.LogHandler.CreateLogAction(sLogString, csLogging.LogState.SUBMISSIONDETAILS);
                    }
                    catch (Exception ex)
                    {
                        if (conn != null)
                            conn.Close();

                        sVar.LogHandler.CreateLogAction("Error adding new customer to the database.\nError Message: " + ex.Message, csLogging.LogState.ERROR);
                    }
                }
                else
                {
                    query = "UPDATE RepairCustomerInformation SET CustomerName = @cName, CustomerAddressLine1 = @cA1, CustomerAddressLine2 = @cA2, CustomerAddressLine3 = @cA3, " +
                            "CustomerAddressLine4 = @cA4, CustomerCity = @cCity, CustomerState = @cState, CustomerPostalCode = @cPostCode, CustomerCountryCode = @cCountryCode " +
                            "WHERE CustomerNumber = '" + cInfo.CustomerNumber + "'";
                    cmd = new SqlCommand(query, conn);
                    try
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@cName", cInfo.CustomerName);
                        cmd.Parameters.AddWithValue("@cA1", cInfo.CustomerAddy1);
                        cmd.Parameters.AddWithValue("@cA2", cInfo.CustomerAddy2);
                        cmd.Parameters.AddWithValue("@cA3", cInfo.CustomerAddy3);
                        cmd.Parameters.AddWithValue("@cA4", cInfo.CustomerAddy4);
                        cmd.Parameters.AddWithValue("@cCity", cInfo.CustomerCity);
                        cmd.Parameters.AddWithValue("@cState", cInfo.CustomerState);
                        cmd.Parameters.AddWithValue("@cPostCode", cInfo.CustomerPostalCode);
                        cmd.Parameters.AddWithValue("@cCountryCode", cInfo.CustomerCountryCode);
                        cmd.ExecuteNonQuery();
                        conn.Close();

                        var sLogString = "Updating existing record in the RepairCustomerInformation table.\n";
                        for (int pCount = 0; pCount < cmd.Parameters.Count; pCount++)
                        {
                            var paramName = cmd.Parameters[pCount].ToString();
                            if (string.IsNullOrEmpty(cmd.Parameters[pCount].Value.ToString()))
                                sLogString += paramName + ": N/A\n";
                            else sLogString += $"{paramName}: {cmd.Parameters[pCount].Value.ToString()}\n";
                        }
                        sVar.LogHandler.CreateLogAction(sLogString, csLogging.LogState.SUBMISSIONDETAILS);
                    }
                    catch (Exception ex)
                    {
                        if (conn != null)
                            conn.Close();

                        sVar.LogHandler.CreateLogAction("Error updating existing customer to the database.\nError Message: " + ex.Message, csLogging.LogState.ERROR);
                    }
                }
            }
        }

        /// <summary>
        /// Searches the database for the associated barcode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
                beginSerialNumberSearch();
        }

        private void txtOrderNumber_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                if (searchOrderNumber())
                    txtBarcode.Focus();
                else { txtOrderNumber.Focus(); txtOrderNumber.SelectAll(); }
            }
        }

        /// <summary> The original list of reference designators pulled from the BOM. </summary>
        private List<string> OrigRefSource = new List<string>();
        /// <summary> The original list of part numbers pulled from the BOM. </summary>
        private List<string> OrigPartSource = new List<string>();
        private void txtMultiRefKeyUp(object sender, KeyEventArgs e)
        {
            /*if (sender is ComboBox cbox)
            {
                if (cbox.Name.Contains("Ref") && OrigRefSource != null)
                {
                    cbox.ItemsSource = OrigRefSource.Where((p) => p.Contains(cbox.Text));
                }
                else if (cbox.Name.Contains("Part") && OrigPartSource != null)
                {
                    cbox.ItemsSource = OrigPartSource.Where((p) => p.Contains(cbox.Text));
                }
            }*/
        }

        private void txtCustomerNumber_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter) && !string.IsNullOrEmpty(txtCustomerNumber.Text))
            {
                loadCustomerInformation(txtCustomerNumber.Text.ToString());
            }
        }

        private bool getCRStatus(string s)
        {
            if (s.Equals("CR"))
                return true;
            else return false;
        }

        private CancellationTokenSource CTS = new CancellationTokenSource();

        #region Button Clicks
        private async void btnComplete_Click(object sender, RoutedEventArgs e)
        {
            sVar.LogHandler.CreateLogAction((Button)sender, csLogging.LogState.CLICK);

            if (checkForRefDesPartRep())
            {
                ArrayList submitStatus = canSubmit();


                if ((bool)submitStatus[0])
                {
                    if (ckbxBatchSubmission.IsChecked ?? false)
                    {
                        var batch = new BatchWindow();
                        batch.Boards.Add(txtBarcode.Text);
                        if (sp != null)
                        {
                            sp.DataReceived -= spDataReceived;
                            batch.MainBarcodeScanner = sp;
                        }
                        if (batch.ShowDialog() == true)
                        {
                            sVar.LogHandler.CreateLogAction($"The submission criteria has been met. Attempting to submit batch of size {batch.Boards.Count} boards...", csLogging.LogState.NOTE);
                            progMapper.Value = 0;
                            progMapper.Maximum = batch.Boards.Count;
                            progMapper.Visibility = Visibility.Visible;

                            foreach (var board in batch.Boards)
                            {
                                txtBarcode.Text = board;
                                submitData(SubmissionStatus.COMPLETE, -1, getCRStatus(submitStatus[1].ToString()));
                                progMapper.Value += 1;
                                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Render, new Action(() => { }));
                            }

                            sVar.LogHandler.writeLogToFile();
                            resetForm(true);
                        }
                        
                        if(sp != null) sp.DataReceived += spDataReceived;
                        return;
                    }
                    else
                    {
                        sVar.LogHandler.CreateLogAction("The submission criteria has been met. Attempting to submit...", csLogging.LogState.NOTE);
                        if (submitData(SubmissionStatus.COMPLETE, -1, getCRStatus(submitStatus[1].ToString())))
                        {
                            sVar.LogHandler.writeLogToFile();
                            MessageBox.Show("Submission Successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            resetForm(true);
                        }
                    }
                }
                else
                {
                    MessageBox.Show(submitStatus[1].ToString(), "Submission Criteria Not Met", MessageBoxButton.OK, MessageBoxImage.Error);
                    sVar.LogHandler.CreateLogAction("The submission criteria has not been met.\n" + submitStatus[1].ToString(), csLogging.LogState.WARNING);
                }
            }
        }

        private void btnSendToQC_Click(object sender, RoutedEventArgs e)
        {
            sVar.LogHandler.CreateLogAction((Button)sender, csLogging.LogState.CLICK);

            if (checkForRefDesPartRep())
            {
                ArrayList submitStatus = canSubmit();
                Random rdm = new Random();
                long iID = Convert.ToInt64(string.Format("{0}{1}", rdm.Next(1, 9), DateTime.Now.ToString("mmddyyhhmmss")));
                sVar.LogHandler.CreateLogAction("The SaveID is: " + iID.ToString(), csLogging.LogState.NOTE);

                if ((bool)submitStatus[0])
                {
                    sVar.LogHandler.CreateLogAction("The save criteria has been met. Attempting to submit...", csLogging.LogState.NOTE);
                    if (submitData(SubmissionStatus.SENDTOQC, iID, getCRStatus(submitStatus[1].ToString())))
                    {
                        sVar.LogHandler.CreateLogAction("Attempting to print the label.", csLogging.LogState.NOTE);
                        generateLabel("QC", iID);
                        sVar.LogHandler.writeLogToFile();
                        MessageBox.Show("Submission Successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        resetForm(true);
                    }
                }
                else
                {
                    MessageBox.Show(submitStatus[1].ToString(), "Save Criteria Not Met", MessageBoxButton.OK, MessageBoxImage.Error);
                    sVar.LogHandler.CreateLogAction("The submission criteria has not been met.\n" + submitStatus[1].ToString(), csLogging.LogState.WARNING);
                }
            }
        }

        private void btnSendToDQE_Click(object sender, RoutedEventArgs e)
        {
            sVar.LogHandler.CreateLogAction((Button)sender, csLogging.LogState.CLICK);

            if (checkForRefDesPartRep())
            {
                ArrayList submitStatus = canSubmit();
                Random rdm = new Random();
                long iID = Convert.ToInt64(string.Format("{0}{1}", rdm.Next(1, 10), DateTime.Now.ToString("mmddyyhhmmss")));
                sVar.LogHandler.CreateLogAction("The SaveID is: " + iID.ToString(), csLogging.LogState.NOTE);

                if ((bool)submitStatus[0])
                {
                    sVar.LogHandler.CreateLogAction("The save criteria has been met. Attempting to submit...", csLogging.LogState.NOTE);
                    if (submitData(SubmissionStatus.SENDTODQE, iID, getCRStatus(submitStatus[1].ToString())))
                    {
                        sVar.LogHandler.CreateLogAction("Attempting to print the label.", csLogging.LogState.NOTE);
                        generateLabel("DQE", iID);
                        sVar.LogHandler.writeLogToFile();
                        MessageBox.Show("Submission Successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        resetForm(true);
                    }
                }
                else
                {
                    MessageBox.Show(submitStatus[1].ToString(), "Save Criteria Not Met", MessageBoxButton.OK, MessageBoxImage.Error);
                    sVar.LogHandler.CreateLogAction("The submission criteria has not been met.\n" + submitStatus[1].ToString(), csLogging.LogState.WARNING);
                }
            }
        }

        private void btnStartOver_Click(object sender, RoutedEventArgs e)
        {
            sVar.LogHandler.CreateLogAction((Button)sender, csLogging.LogState.CLICK);
            sVar.LogHandler.writeLogToFile();
            resetForm(true);
        }

        private void btnAddMultiParts_Click(object sender, RoutedEventArgs e)
        {
            sVar.LogHandler.CreateLogAction((Button)sender, csLogging.LogState.CLICK);

            if (((Control)sender).Name.Contains("_"))
            {
                if (((Control)sender).Name.EndsWith("2"))
                {
                    if (string.IsNullOrEmpty(txtRefDes_2.Text) && string.IsNullOrEmpty(txtPartReplaced_2.Text)) { return; }
                    else
                    {
                        sVar.LogHandler.CreateLogAction((Button)sender, csLogging.LogState.CLICK);

                        /*if (BOMFileActive && (!txtRefDes_2.Items.Contains(txtRefDes_2.Text)
                            || dgMultipleParts_2.Items
                        .OfType<MultiplePartsReplaced>()
                        .Where(mpr => mpr.RefDesignator.Equals(txtRefDes_2.Text)).Any()))
                        {
                            brdRefDes_2.BorderBrush = Brushes.Red;
                            brdRefDes_2.BorderThickness = new Thickness(1.0);
                            MessageBox.Show("Invalid Ref Designator", $"{txtRefDes_2.Text} is not a valid designator!", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        else
                        {
                            brdRefDes_2.BorderBrush = null;
                            brdRefDes_2.BorderThickness = new Thickness(0.0);
                        }*/

                        string _sPRPD = csCrossClassInteraction.GetPartReplacedPartDescription(txtPartReplaced_2.Text);

                        if (string.IsNullOrEmpty(_sPRPD) && !string.IsNullOrEmpty(txtPartReplaced_2.Text))
                        {
                            string sWarning = $"The Part Replaced entered ( {txtPartReplaced_2.Text} ) does not exist. Please verify the Part Number and try again.";
                            sVar.LogHandler.CreateLogAction(sWarning, csLogging.LogState.WARNING);
                            //MessageBox.Show(sWarning, "Part Replaced Description Issue", MessageBoxButton.OK, MessageBoxImage.Warning);
                            txtPartReplaced_2.Focus();
                            txtPartReplaced_2.SelectAll();
                        }
                        else
                        {
                            MultiplePartsReplaced mpr = new MultiplePartsReplaced { RefDesignator = txtRefDes_2.Text.TrimEnd(), PartReplaced = txtPartReplaced_2.Text.TrimEnd(), PartsReplacedPartDescription = _sPRPD };
                            if (string.IsNullOrEmpty(mpr.PartReplaced) && !string.IsNullOrEmpty(mpr.RefDesignator))
                                sVar.LogHandler.CreateLogAction($"Adding Ref Designator '{mpr.RefDesignator}' to dgMultipleParts. Parts Replaced is empty.", csLogging.LogState.NOTE);
                            else if (!string.IsNullOrEmpty(mpr.PartReplaced) && string.IsNullOrEmpty(mpr.RefDesignator))
                                sVar.LogHandler.CreateLogAction($"Adding Part Replaced '{mpr.PartReplaced}' to dgMultipleParts. Ref Designator is empty.", csLogging.LogState.NOTE);
                            else sVar.LogHandler.CreateLogAction($"Adding Part Replaced '{mpr.PartReplaced}' and Ref Designator '{mpr.RefDesignator}' to dgMultipleParts.", csLogging.LogState.NOTE);

                            dgMultipleParts_2.Items.Add(mpr);
                            txtPartReplaced_2.Text = txtRefDes_2.Text = string.Empty;
                        }
                    }
                }
                else if (((Control)sender).Name.EndsWith("3"))
                {
                    if (string.IsNullOrEmpty(txtRefDes_3.Text) && string.IsNullOrEmpty(txtPartReplaced_3.Text)) { return; }
                    else
                    {
                        sVar.LogHandler.CreateLogAction((Button)sender, csLogging.LogState.CLICK);

                        /*if (BOMFileActive && (!txtRefDes_3.Items.Contains(txtRefDes_3.Text)
                            || dgMultipleParts_3.Items
                        .OfType<MultiplePartsReplaced>()
                        .Where(mpr => mpr.RefDesignator.Equals(txtRefDes_3.Text)).Any()))
                        {
                            brdRefDes_3.BorderBrush = Brushes.Red;
                            brdRefDes_3.BorderThickness = new Thickness(1.0);
                            MessageBox.Show("Invalid Ref Designator", $"{txtRefDes_3.Text} is not a valid designator!", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        else
                        {
                            brdRefDes_3.BorderBrush = null;
                            brdRefDes_3.BorderThickness = new Thickness(0.0);
                        }*/

                        string _sPRPD = csCrossClassInteraction.GetPartReplacedPartDescription(txtPartReplaced_3.Text);

                        if (string.IsNullOrEmpty(_sPRPD) && !string.IsNullOrEmpty(txtPartReplaced_3.Text))
                        {
                            string sWarning = $"The Part Replaced entered ( {txtPartReplaced_3.Text} ) does not exist. Please verify the Part Number and try again.";
                            sVar.LogHandler.CreateLogAction(sWarning, csLogging.LogState.WARNING);
                            MessageBox.Show(sWarning, "Part Replaced Description Issue", MessageBoxButton.OK, MessageBoxImage.Warning);
                            txtPartReplaced_3.Focus();
                            txtPartReplaced_3.SelectAll();
                        }
                        else
                        {
                            MultiplePartsReplaced mpr = new MultiplePartsReplaced { RefDesignator = txtRefDes_3.Text.TrimEnd(), PartReplaced = txtPartReplaced_3.Text.TrimEnd(), PartsReplacedPartDescription = _sPRPD };
                            if (string.IsNullOrEmpty(mpr.PartReplaced) && !string.IsNullOrEmpty(mpr.RefDesignator))
                                sVar.LogHandler.CreateLogAction($"Adding Ref Designator '{mpr.RefDesignator}' to dgMultipleParts. Parts Replaced is empty.", csLogging.LogState.NOTE);
                            else if (!string.IsNullOrEmpty(mpr.PartReplaced) && string.IsNullOrEmpty(mpr.RefDesignator))
                                sVar.LogHandler.CreateLogAction($"Adding Part Replaced '{mpr.PartReplaced}' to dgMultipleParts. Ref Designator is empty.", csLogging.LogState.NOTE);
                            else sVar.LogHandler.CreateLogAction($"Adding Part Replaced '{mpr.PartReplaced}' and Ref Designator '{mpr.RefDesignator}' to dgMultipleParts.", csLogging.LogState.NOTE);

                            dgMultipleParts_3.Items.Add(mpr);
                            txtPartReplaced_3.Text = txtRefDes_3.Text = string.Empty;
                        }
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(txtRefDes.Text) && string.IsNullOrEmpty(txtPartReplaced.Text)) { return; }
                else
                {
                    sVar.LogHandler.CreateLogAction((Button)sender, csLogging.LogState.CLICK);

                    /*if (BOMFileActive && (!txtRefDes.Items.Contains(txtRefDes.Text)
                        || dgMultipleParts.Items
                        .OfType<MultiplePartsReplaced>()
                        .Where(mpr => mpr.RefDesignator.Equals(txtRefDes.Text)).Any()))
                    {
                        brdRefDes.BorderBrush = Brushes.Red;
                        brdRefDes.BorderThickness = new Thickness(1.0);
                        MessageBox.Show("Invalid Ref Designator", $"{txtRefDes.Text} is not a valid designator!", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    else
                    {
                        brdRefDes.BorderBrush = null;
                        brdRefDes.BorderThickness = new Thickness(0.0);
                    }*/

                    string _sPRPD = csCrossClassInteraction.GetPartReplacedPartDescription(txtPartReplaced.Text);

                    if (string.IsNullOrEmpty(_sPRPD) && !string.IsNullOrEmpty(txtPartReplaced.Text))
                    {
                        string sWarning = $"The Part Replaced entered ( {txtPartReplaced.Text} ) does not exist. Please verify the Part Number and try again.";
                        sVar.LogHandler.CreateLogAction(sWarning, csLogging.LogState.WARNING);
                        MessageBox.Show(sWarning, "Part Replaced Description Issue", MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtPartReplaced.Focus();
                        txtPartReplaced.SelectAll();
                    }
                    else
                    {
                        MultiplePartsReplaced mpr = new MultiplePartsReplaced { RefDesignator = txtRefDes.Text.TrimEnd(), PartReplaced = txtPartReplaced.Text.TrimEnd(), PartsReplacedPartDescription = _sPRPD.TrimEnd() };
                        if (string.IsNullOrEmpty(mpr.PartReplaced) && !string.IsNullOrEmpty(mpr.RefDesignator))
                            sVar.LogHandler.CreateLogAction($"Adding Ref Designator '{mpr.RefDesignator}' to dgMultipleParts. Parts Replaced is empty.", csLogging.LogState.NOTE);
                        else if (!string.IsNullOrEmpty(mpr.PartReplaced) && string.IsNullOrEmpty(mpr.RefDesignator))
                            sVar.LogHandler.CreateLogAction($"Adding Part Replaced '{mpr.PartReplaced}' to dgMultipleParts. Ref Designator is empty.", csLogging.LogState.NOTE);
                        else sVar.LogHandler.CreateLogAction($"Adding Part Replaced '{mpr.PartReplaced}' and Ref Designator '{mpr.RefDesignator}' to dgMultipleParts.", csLogging.LogState.NOTE);

                        dgMultipleParts.Items.Add(mpr);
                        txtPartReplaced.Text = txtRefDes.Text = string.Empty;
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

        private void btnRebootSP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sp != null)
                {
                    sp.Close();
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

        private void btnLookupPartName_Click(object sender, RoutedEventArgs e)
        {
            sVar.LogHandler.CreateLogAction((Button)sender, csLogging.LogState.CLICK);
            csCrossClassInteraction.LoadPartNumberForm(false, new List<TextBox> { txtPartNumber, txtPartName, txtSeries });
            sVar.LogHandler.CreateLogAction($"The operator selected Part Number '{sVar.SelectedPartNumberPartName.PartNumber}', " +
                $"Part Name '{sVar.SelectedPartNumberPartName.PartName}', and " +
                $"Part Series '{sVar.SelectedPartNumberPartName.PartSeries}'.", csLogging.LogState.NOTE);

            if (!string.IsNullOrEmpty(txtPartNumber.Text))
                fillCommoditySubClass();
        }
        #endregion

        private void cbxScrap_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)cbxScrap.IsChecked)
                sVar.LogHandler.CreateLogAction("cbxScrap was selected.", csLogging.LogState.CLICK);
            else sVar.LogHandler.CreateLogAction("cbxScrap was deselected.", csLogging.LogState.CLICK);
        }

        private void cbxNPF_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)cbxNPF.IsChecked)
                sVar.LogHandler.CreateLogAction("cbxNPF was selected.", csLogging.LogState.CLICK);
            else sVar.LogHandler.CreateLogAction("cbxNPF was deselected.", csLogging.LogState.CLICK);
        }

        private void dgPrevRepairInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgPrevRepairInfo.SelectedItem != null)
                {
                    frmRepairPRI pri = new frmRepairPRI((PreviousRepairInformation)dgPrevRepairInfo.SelectedItem)
                    {
                        Owner = this
                    };
                    pri.Closing += delegate
                    {
                        dgPrevRepairInfo.Dispatcher.Invoke(() =>
                        dgPrevRepairInfo.IsEnabled = true);
                    };
                    pri.Loaded += delegate
                    {
                        dgPrevRepairInfo.IsEnabled = false;
                    };
                    pri.Show();
                    Activate();
                }
            }
            catch (ArgumentOutOfRangeException aoore)
            {
                csExceptionLogger.csExceptionLogger.Write("RandomCrashing_RepairPRI", aoore);
                dgPrevRepairInfo.IsEnabled = true;
            }
        }

        private void txtQTY_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtQTY.Text))
            {
                sVar.LogHandler.CreateLogAction("txtQTY is now empty so cbxNPF will now be disabled.", csLogging.LogState.NOTE);
                cbxNPF.IsEnabled = false;
            }
            else
            {
                sVar.LogHandler.CreateLogAction("txtQTY is no longer empty so cbxNPF will now be enabled.", csLogging.LogState.NOTE);
                cbxNPF.IsEnabled = true;
            }
        }

        /// <summary>
        /// Only allow numbers in this control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private new void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        #region Serial Port Actions
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
                tSPChecker.Start();
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

        private void spDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                sVar.LogHandler.CreateLogAction("Serial Port Data Received - Begin", csLogging.LogState.NOTE);
                Dispatcher.Invoke(delegate
                {
                    txtBarcode.Text = string.Empty;
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
                            if (txtBarcode.Dispatcher.Thread == Thread.CurrentThread)
                                txtBarcode.Text += data;
                            else
                            {
                                Dispatcher.Invoke(delegate
                                {
                                    txtBarcode.Text += data;
                                });
                                data = string.Empty;
                            }
                        }
                    }
                }

                if (data.Length > 0)
                {
                    if (txtBarcode.Dispatcher.Thread == Thread.CurrentThread)
                    {
                        txtBarcode.Text += data;
                        beginSerialNumberSearch();
                    }
                    else
                    {
                        Dispatcher.Invoke(delegate
                        {
                            txtBarcode.Text += data;
                            txtBarcode.Text = txtBarcode.Text.TrimEnd();
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
                        Thread.Sleep(250);
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
        #endregion

        private void dgBeginEdit(object sender, DataGridBeginningEditEventArgs e)
            => e.Cancel = true;

        #region Log Actions
        private void txtGotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox c)
            {
                sVar.LogHandler.CreateLogAction(c, csLogging.LogState.ENTER);
                c.IsDropDownOpen = true;
            }
            else
                sVar.LogHandler.CreateLogAction((TextBox)sender, csLogging.LogState.ENTER);
        }

        private void txtLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox c)
            {
                sVar.LogHandler.CreateLogAction(c, csLogging.LogState.LEAVE);
                c.IsDropDownOpen = false;
            }
            else
                sVar.LogHandler.CreateLogAction((TextBox)sender, csLogging.LogState.LEAVE);
        }

        private void rtbGotFocus(object sender, RoutedEventArgs e)
            => sVar.LogHandler.CreateLogAction((RichTextBox)sender, csLogging.LogState.ENTER);

        private void rtbLostFocus(object sender, RoutedEventArgs e)
            => sVar.LogHandler.CreateLogAction((RichTextBox)sender, csLogging.LogState.LEAVE);

        private void cbDDClosed(object sender, EventArgs e)
        {
            sVar.LogHandler.CreateLogAction((ComboBox)sender, csLogging.LogState.DROPDOWNCLOSED);

            if (!((Control)sender).Name.ToString().Equals("cbTOF") && !((Control)sender).Name.ToString().Equals("cbTOR"))
                handleUnitIssues((ComboBox)sender);

            /*if (((Control)sender).Name.Equals("cbReportedIssue"))
            {
                cbReportedIssue_2.Text = cbReportedIssue_3.Text = cbReportedIssue.Text;
            }*/

            if (((Control)sender).Name.ToString().Equals("cbTOR"))
            {
                if (cbTOR.Text.Equals("Credit Return") || cbTOR.Text.Equals("International"))
                {
                    lblQTY.Visibility = txtQTY.Visibility = Visibility.Visible;
                    cbxNPF.Visibility = Visibility.Visible;
                    cbxNPF.IsEnabled = false;
                    sVar.LogHandler.CreateLogAction("Credit Return or International was selected. txtQTY and cbxNPF are now visible. cbxNPF is set to disabled.", csLogging.LogState.NOTE);
                }
                else
                {
                    lblQTY.Visibility = txtQTY.Visibility = Visibility.Hidden;
                    cbxNPF.Visibility = Visibility.Hidden;
                    sVar.LogHandler.CreateLogAction("Credit Return or International was unselected. txtQTY and cbxNPF are now hidden.", csLogging.LogState.NOTE);
                }
            }
        }
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
            MapperTokenSource.Cancel();
            MainWindow.GlobalInstance.MakeFocus();
        }

        private void vSleep(int iSleepTime)
        {
            DateTime dt = DateTime.Now.AddMilliseconds(iSleepTime);
            while (DateTime.Now < dt)
            { }
        }

        private void MnuiDeleteRow_Click(object sender, RoutedEventArgs e)
        {
            switch (tabcUnitIssues.SelectedIndex)
            {
                case 0:
                    dgMultipleParts.Items.RemoveAt(dgMultipleParts.SelectedIndex);
                    break;
                case 1:
                    dgMultipleParts_2.Items.RemoveAt(dgMultipleParts_2.SelectedIndex);
                    break;
                case 2:
                    dgMultipleParts_3.Items.RemoveAt(dgMultipleParts_3.SelectedIndex);
                    break;
            }
        }

        private void BtnTech_Click(object sender, RoutedEventArgs e)
        {
            var pn = txtPartNumber?.Text ?? "";
            using (frmBoardFileManager alias = new frmBoardFileManager(partNumber: pn, directDialog: true) { StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen })
            {
                alias.Show();
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((tbMain.SelectedItem as TabItem).Header.Equals("EOL Test"))
            {
                //TODO: ucEOLTab.Dispatcher.Invoke(() => ucEOLTab.TriggerDropDowns());
            }
        }
    }
}