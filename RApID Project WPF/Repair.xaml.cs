﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.IO.Ports;
using System.Windows.Threading;
using System.Collections;
using EricStabileLibrary;
using System.Text.RegularExpressions;

namespace RApID_Project_WPF
{
    /// <summary>
    /// Interaction logic for Repair.xaml
    /// </summary>
    public partial class Repair : Window
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
        
        private string sRPNum = String.Empty;
        string sUserDepartmentNumber = "";
        string sDQE_DeptNum = "320900";
        double dLineNumber;

        InitSplash initS = new InitSplash();
        #endregion
        
        public Repair(bool bRework)
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            initS.InitSplash1("Initializing Form...");
            buildDGViews();
            csSplashScreenHelper.ShowText("Loading DataLog...");
            initDataLogForm();
            csSplashScreenHelper.ShowText("Initial SQL Query...");
            initSQLQuery();
            csSplashScreenHelper.ShowText("Building Serial Port...");
            handleInitSerialPort();
            csSplashScreenHelper.ShowText("Iniializing Logging...");
            setupLogging();
            
            GC.Collect();
            csSplashScreenHelper.ShowText("Done!");
            csSplashScreenHelper.Hide();
            this.Activate();
#if DEBUG
            //txtBarcode.Text = "131119030211";
#endif
        }


        #region Initialization
        private void buildDGViews()
        {
            csCrossClassInteraction.dgBuildView(dgMultipleParts, "MULTIPLEPARTS");
            csCrossClassInteraction.dgBuildView(dgMultipleParts_2, "MULTIPLEPARTS");
            csCrossClassInteraction.dgBuildView(dgMultipleParts_3, "MULTIPLEPARTS");
            csCrossClassInteraction.dgBuildView(dgvAOI, "AOI");
            csCrossClassInteraction.dgBuildView(dgvDefectCodes, "DEFECTCODES");
            csCrossClassInteraction.dgBuildView(dgPrevRepairInfo, "PREVREPAIRINFO");
        }

        /// <summary>
        /// Takes care of items that need to be 'edited' when the form is first initialized
        /// </summary>
        private void initDataLogForm()
        {
            txtTechName.Text = System.Environment.UserName;
#if DEBUG
            //txtTechName.Text = "rkaiser";
            txtOrderNumber.Text = "7601898";
            txtBarcode.Text = "161130180149";
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
            List<string> lTechAction1 = new List<string>();
            List<string> lTechAction2 = new List<string>();
            List<string> lTechAction3 = new List<string>();
            #endregion

            string query1 = "SELECT * FROM RApID_DropDowns";
            string query2 = "SELECT PC1 FROM JDECodes";

            SqlConnection conn = new SqlConnection(holder.RepairConnectionString);
            SqlCommand cmd = new SqlCommand(query1, conn);
            try
            {
                conn.Open();
                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        if (!String.IsNullOrEmpty(reader["TypeOfReturn"].ToString()))
                            lTOR.Add(reader["TypeOfReturn"].ToString());

                        if (!String.IsNullOrEmpty(reader["TypeOfFailure"].ToString()))
                            lTOF.Add(reader["TypeOfFailure"].ToString());

                        if (!String.IsNullOrEmpty(reader["Cause"].ToString()))
                            lCause.Add(reader["Cause"].ToString());

                        if (!String.IsNullOrEmpty(reader["Replacement"].ToString()))
                            lReplacement.Add(reader["Replacement"].ToString());

                        if(!String.IsNullOrEmpty(reader["TestResult"].ToString()))
                            lTestResult.Add(reader["TestResult"].ToString());

                        if (!String.IsNullOrEmpty(reader["TestResult_Abort"].ToString()))
                            lTestResultAbort.Add(reader["TestResult_Abort"].ToString());

                        if(!String.IsNullOrEmpty(reader["TechAction"].ToString()))
                        {
                            if (Convert.ToBoolean(reader["ShowTechActionInAll"]))
                            {
                                lTechAction1.Add(reader["TechAction"].ToString());
                                lTechAction2.Add(reader["TechAction"].ToString());
                                lTechAction3.Add(reader["TechAction"].ToString());
                            }
                            else lTechAction1.Add(reader["TechAction"].ToString());
                        }
                    }
                }
                conn.Close();

                cmd = new SqlCommand(query2, conn);

                conn.Open();

                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        if (!String.IsNullOrEmpty(reader[0].ToString()))
                            lReportedIssue.Add(reader[0].ToString());
                    }
                }

                conn.Close();

                #region Combobox Fill
                csCrossClassInteraction.cbFill(cbTOR, csCrossClassInteraction.lSortList(lTOR));
                csCrossClassInteraction.cbFill(cbTOF, csCrossClassInteraction.lSortList(lTOF));

                csCrossClassInteraction.cbFill(cbReportedIssue, csCrossClassInteraction.lSortList(lReportedIssue));
                csCrossClassInteraction.cbFill(cbTestResult, csCrossClassInteraction.lSortList(lTestResult));
                csCrossClassInteraction.cbFill(cbTestResultAbort, csCrossClassInteraction.lSortList(lTestResultAbort));
                csCrossClassInteraction.cbFill(cbCause, csCrossClassInteraction.lSortList(lCause));
                csCrossClassInteraction.cbFill(cbReplacement, csCrossClassInteraction.lSortList(lReplacement));
                csCrossClassInteraction.cbFill(cbTechAction1, csCrossClassInteraction.lSortList(lTechAction1));
                csCrossClassInteraction.cbFill(cbTechAction2, csCrossClassInteraction.lSortList(lTechAction2));
                csCrossClassInteraction.cbFill(cbTechAction3, csCrossClassInteraction.lSortList(lTechAction3));

                csCrossClassInteraction.cbFill(cbReportedIssue_2, csCrossClassInteraction.lSortList(lReportedIssue));
                csCrossClassInteraction.cbFill(cbTestResult_2, csCrossClassInteraction.lSortList(lTestResult));
                csCrossClassInteraction.cbFill(cbTestResultAbort_2, csCrossClassInteraction.lSortList(lTestResultAbort));
                csCrossClassInteraction.cbFill(cbCause_2, csCrossClassInteraction.lSortList(lCause));
                csCrossClassInteraction.cbFill(cbReplacement_2, csCrossClassInteraction.lSortList(lReplacement));

                csCrossClassInteraction.cbFill(cbReportedIssue_3, csCrossClassInteraction.lSortList(lReportedIssue));
                csCrossClassInteraction.cbFill(cbTestResult_3, csCrossClassInteraction.lSortList(lTestResult));
                csCrossClassInteraction.cbFill(cbTestResultAbort_3, csCrossClassInteraction.lSortList(lTestResultAbort));
                csCrossClassInteraction.cbFill(cbCause_3, csCrossClassInteraction.lSortList(lCause));
                csCrossClassInteraction.cbFill(cbReplacement_3, csCrossClassInteraction.lSortList(lReplacement));
                #endregion

            }
            catch(Exception ex)
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
                            if (!String.IsNullOrEmpty(reader["PC1"].ToString()))
                            {
                                lPC1.Add(new PC1 { CodeName = reader["PC1"].ToString(), ReturnCode = reader["PC1Code"].ToString() });
                            }
                        }
                        if (reader["PC2"] != DBNull.Value)
                        {
                            if (!String.IsNullOrEmpty(reader["PC2"].ToString()))
                            {
                                lPC2.Add(new PC2 { CodeName = reader["PC2"].ToString(), ReturnCode = reader["PC2Code"].ToString() });
                            }
                        }
                        if (reader["PC3"] != DBNull.Value)
                        {
                            if (!String.IsNullOrEmpty(reader["PC3"].ToString()))
                            {
                                lPC3.Add(new PC3 { CodeName = reader["PC3"].ToString(), ReturnCode = reader["PC3Code"].ToString() });
                            }
                        }
                        if (reader["EndUse"] != DBNull.Value)
                        {
                            if (!String.IsNullOrEmpty(reader["EndUse"].ToString()))
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
            if (SerialPort.GetPortNames().Any(x => x == Properties.Settings.Default.SPPortName))
            {
                try
                {
                    sp = new SerialPort(Properties.Settings.Default.SPPortName, Properties.Settings.Default.SPBaudRate, Properties.Settings.Default.SPParity, Properties.Settings.Default.SPDataBit, Properties.Settings.Default.SPStopBit);
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
                txtOrderNumber.Text = String.Empty;
                lblRPNumber.Content = String.Empty;
                sRPNum = String.Empty;
                txtBarcode.Text = String.Empty;
                dLineNumber = 0;
                resetCustomerInformation();
                txtOrderNumber.Focus();
            }

            txtPartName.Text = String.Empty;
            txtPartNumber.Text = String.Empty;
            txtSeries.Text = String.Empty;
            txtCommSubClass.Text = String.Empty;

            txtQTY.Text = String.Empty;
            lblQTY.Visibility = txtQTY.Visibility = Visibility.Hidden;
            cbxNPF.IsChecked = false;
            cbxNPF.IsEnabled = false;
            cbxNPF.Visibility = System.Windows.Visibility.Hidden;

            txtSWVersion.Text = String.Empty;
            cbxScrap.IsChecked = false;
            dtpDateReceived.SelectedDate = DateTime.Now;
            cbTOR.Text = String.Empty;
            cbTOF.Text = String.Empty;
            txtHOU.Text = String.Empty;

            rtbAdditionalComments.Document.Blocks.Clear();

            dgPrevRepairInfo.Items.Clear();

            cbReportedIssue.SelectedIndex = -1;
            resetUnitIssues();
            resetEOLTab();
            resetAOITab();
            cbTechAction1.SelectedIndex = -1;
            cbTechAction2.SelectedIndex = -1;
            cbTechAction3.SelectedIndex = -1;
        }

        /// <summary>
        /// Reset all of the unit issues
        /// </summary>
        private void resetUnitIssues()
        {
            foreach (UIElement uie in gridUI1.Children)
            {
                if (uie.GetType().Name.Equals("ComboBox"))
                {
                    ComboBox cb = (ComboBox)uie;
                    cb.SelectedIndex = -1;
                }
                if (uie.GetType().Name.Equals("TextBox"))
                {
                    TextBox tb = (TextBox)uie;
                    tb.Text = String.Empty;
                }
                if (uie.GetType().Name.Equals("DataGrid"))
                {
                    DataGrid dg = (DataGrid)uie;
                    dg.Items.Clear();
                }
            }

            foreach (UIElement uie in gridUI2.Children)
            {
                if (uie.GetType().Name.Equals("ComboBox"))
                {
                    ComboBox cb = (ComboBox)uie;
                    cb.SelectedIndex = -1;
                }
                if (uie.GetType().Name.Equals("TextBox"))
                {
                    TextBox tb = (TextBox)uie;
                    tb.Text = String.Empty;
                }
                if (uie.GetType().Name.Equals("DataGrid"))
                {
                    DataGrid dg = (DataGrid)uie;
                    dg.Items.Clear();
                }
            }

            foreach (UIElement uie in gridUI3.Children)
            {
                if (uie.GetType().Name.Equals("ComboBox"))
                {
                    ComboBox cb = (ComboBox)uie;
                    cb.SelectedIndex = -1;
                }
                if (uie.GetType().Name.Equals("TextBox"))
                {
                    TextBox tb = (TextBox)uie;
                    tb.Text = String.Empty;
                }
                if (uie.GetType().Name.Equals("DataGrid"))
                {
                    DataGrid dg = (DataGrid)uie;
                    dg.Items.Clear();
                }
            }

            tiUI2.IsEnabled = tiUI3.IsEnabled = false;
            tiUI1.Focus();
        }

        private void resetUnitIssues(int iUReset)
        {
            switch (iUReset)
            {
                case 1:
                    foreach (UIElement uie in gridUI1.Children)
                    {
                        if (uie.GetType().Name.Equals("ComboBox"))
                        {
                            ComboBox cb = (ComboBox)uie;
                            cb.SelectedIndex = -1;
                        }
                        if (uie.GetType().Name.Equals("TextBox"))
                        {
                            TextBox tb = (TextBox)uie;
                            tb.Text = String.Empty;
                        }
                        if (uie.GetType().Name.Equals("DataGrid"))
                        {
                            DataGrid dg = (DataGrid)uie;
                            dg.Items.Clear();
                        }
                    }
                    break;
                case 2:
                    foreach (UIElement uie in gridUI2.Children)
                    {
                        if (uie.GetType().Name.Equals("ComboBox"))
                        {
                            ComboBox cb = (ComboBox)uie;
                            cb.SelectedIndex = -1;
                        }
                        if (uie.GetType().Name.Equals("TextBox"))
                        {
                            TextBox tb = (TextBox)uie;
                            tb.Text = String.Empty;
                        }
                        if (uie.GetType().Name.Equals("DataGrid"))
                        {
                            DataGrid dg = (DataGrid)uie;
                            dg.Items.Clear();
                        }
                    }
                    break;
                case 3:
                    foreach (UIElement uie in gridUI3.Children)
                    {
                        if (uie.GetType().Name.Equals("ComboBox"))
                        {
                            ComboBox cb = (ComboBox)uie;
                            cb.SelectedIndex = -1;
                        }
                        if (uie.GetType().Name.Equals("TextBox"))
                        {
                            TextBox tb = (TextBox)uie;
                            tb.Text = String.Empty;
                        }
                        if (uie.GetType().Name.Equals("DataGrid"))
                        {
                            DataGrid dg = (DataGrid)uie;
                            dg.Items.Clear();
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void resetCustomerInformation()
        {
            foreach(UIElement uie in gCustInfo.Children)
            {
                if(uie.GetType().Name.Equals("TextBox"))
                {
                    TextBox tb = (TextBox)uie;
                    tb.Text = String.Empty;
                }
            }
        }

        /// <summary>
        /// Reset all of the data in the EOL Tab
        /// </summary>
        private void resetEOLTab()
        {
            cbEOLTestID.Items.Clear();
            cbPRETestID.Items.Clear();
            cbPOSTTestID.Items.Clear();
            cbBEAMSTestType.Items.Clear();
            cbBEAMSTestID.Items.Clear();
            cbBEAMSBeamNum.Items.Clear();

            lsvEOL.Items.Clear();
            lsvPreBurnIn.Items.Clear();
            lsvPostBurnIn.Items.Clear();
            lsvBeamTestId.Items.Clear();
        }

        /// <summary>
        /// Reset all of the data in the AOI Tab
        /// </summary>
        private void resetAOITab()
        {
            dgvAOI.Items.Clear();
            dgvDefectCodes.Items.Clear();
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
                    if (!String.IsNullOrEmpty(cb.Text))
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
                    if (!String.IsNullOrEmpty(cb.Text))
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
                    if (!String.IsNullOrEmpty(cb.Text))
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

        private void fillDataLog()
        {
            QueryProduction();

            if (!String.IsNullOrEmpty(txtPartNumber.Text))
            {
                sVar.LogHandler.CreateLogAction("Attempting to get the Part Series now.", csLogging.LogState.NOTE);

                if (String.IsNullOrEmpty(txtSeries.Text)) { txtSeries.Text = csCrossClassInteraction.SeriesQuery(txtPartNumber.Text); }
                if (!String.IsNullOrEmpty(txtSeries.Text)) { sVar.LogHandler.CreateLogAction("The Part Series was found. (" + txtSeries.Text.TrimEnd() + ")", csLogging.LogState.NOTE); }

                fillCommoditySubClass();
            }
            QueryTechReport();
        }

        private void QueryProduction()
        {
            string query = "";

            if (lblRPNumber.Content.ToString().Replace("RP Number: ", "").StartsWith("SV")) // this is a transducer so lets do something different
                query = "SELECT PartNumber FROM tblXducerTestResults WHERE SerialNumber = '" + txtBarcode.Text + "';";
            else query = "SELECT Assy FROM Production3 WHERE SerialNum = '" + txtBarcode.Text + "';";
            

            string sProdQueryResults = csCrossClassInteraction.ProductionQuery(query);
            if(sProdQueryResults.ToLower().Contains("rev"))
            {
                csCrossClassInteraction.LoadPartNumberForm(false, new List<TextBox> { txtPartNumber, txtPartName, txtSeries });
            }
            else
            {
                sVar.LogHandler.CreateLogAction("Part Number '" + sProdQueryResults + "' was found.", csLogging.LogState.NOTE);
                txtPartNumber.Text = sProdQueryResults;
                QueryItemMaster();
            }
        }

        private void QueryItemMaster()
        {
            string query = "SELECT PartName FROM ItemMaster WHERE PartNumber = '" + txtPartNumber.Text + "';";
            string sItemMasterQueryResults = csCrossClassInteraction.ItemMasterQuery(query);

            if(!String.IsNullOrEmpty(sItemMasterQueryResults))
            {
                txtPartName.Text = sItemMasterQueryResults;
                sVar.LogHandler.CreateLogAction("txtPartName's value has been set to " + txtPartName.Text + ".", csLogging.LogState.NOTE);
            }
        }

        private void QueryTechReport()
        {
            //NOTE: Old Database
            //string query = "SELECT Date_Time, Technician FROM tblManufacturingTechReport WHERE SerialNumber = '" + txtBarcode.Text + "';";
            //sVar.LogHandler.CreateLogAction("Querying the Tech Report for previous tech information PRIOR to the release of RApID.", csLogging.LogState.NOTE);
            //sVar.LogHandler.CreateLogAction("SQL QUERY: " + query, csLogging.LogState.SQLQUERY);
            //csCrossClassInteraction.dgTechReport(query, true, dgPrevRepairInfo, txtBarcode.Text);

            //NOTE: New Database
            string query = "SELECT DateSubmitted, Technician, ID FROM TechnicianSubmission WHERE SerialNumber = '" + txtBarcode.Text + "';";
            sVar.LogHandler.CreateLogAction("Querying the Tech Report for previous tech information from the TechnicianSubmission table.", csLogging.LogState.NOTE);
            sVar.LogHandler.CreateLogAction("SQL QUERY: " + query, csLogging.LogState.SQLQUERY);
            csCrossClassInteraction.dgTechReport(query, false, dgPrevRepairInfo, txtBarcode.Text);

        }

        /// <summary>
        /// Fills EOL Test section of the form with the data associated to the scanned/typed barcode
        /// </summary>
        private void fillEOLData()
        {
            resetEOLTab();

            string query = "";

            if (lblRPNumber.Content.ToString().Replace("RP Number: ", "").StartsWith("SV")) // this is a transducer so lets do something different
                query = "SELECT DISTINCT TestID FROM tblXducerTestResultsBenchTest WHERE SerialNumber = '" + txtBarcode.Text + "';";
            else query = "SELECT TestID FROM tblEOL WHERE PCBSerial = '" + txtBarcode.Text + "';";
            csCrossClassInteraction.cbFillFromQuery(cbEOLTestID, query);

            query = "SELECT TestID FROM tblPRE WHERE PCBSerial = '" + txtBarcode.Text + "';";
            csCrossClassInteraction.cbFillFromQuery(cbPRETestID, query);

            if (lblRPNumber.Content.ToString().Replace("RP Number: ", "").StartsWith("SV")) // this is a transducer so lets do something different
                query = "SELECT DISTINCT TestID FROM tblXducerTestResults WHERE SerialNumber = '" + txtBarcode.Text + "';";
            else query = "SELECT TestID FROM tblPOST WHERE PCBSerial = '" + txtBarcode.Text + "';";
            csCrossClassInteraction.cbFillFromQuery(cbPOSTTestID, query);

            if (!lblRPNumber.Content.ToString().Replace("RP Number: ", "").StartsWith("SV")) // this is a transducer so lets do something different
            {
                if (cbEOLTestID.Items.Count > 0)
                    cbBEAMSTestType.Items.Add("EOL");

                if (cbPRETestID.Items.Count > 0)
                    cbBEAMSTestType.Items.Add("PRE");

                if (cbPOSTTestID.Items.Count > 0)
                    cbBEAMSTestType.Items.Add("POST");
            }
        }

        /// <summary>
        /// Fills the AOI Section with data associated to the scanned/typed barcode
        /// </summary>
        private void fillAOIData()
        {
            resetAOITab();
            csCrossClassInteraction.AOIQuery(dgvAOI, dgvDefectCodes, txtBarcode.Text);
        }

        /// <summary>
        /// Fills txtCommSubClass based on the given Part Number
        /// </summary>
        private void fillCommoditySubClass()
        {
            string query = "SELECT CommodityClass FROM ItemMaster WHERE PartNumber = '" + txtPartNumber.Text.TrimEnd() + "'";
            sVar.LogHandler.CreateLogAction("Attempting to fill the Commodity Sub-Class.", csLogging.LogState.NOTE);
            csCrossClassInteraction.txtFillFromQuery(query, txtCommSubClass);
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
                    if (!String.IsNullOrEmpty(cb.Text))
                        tiUI3.IsEnabled = true;
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(cb.Text))
                    tiUI2.IsEnabled = true;
            }
        }

        private string getPartReplacedPartDescription(string _sPartReplaced)
        {
            if (string.IsNullOrEmpty(_sPartReplaced))
                return string.Empty;

            string query = "SELECT PartName FROM ItemMaster WHERE PartNumber = '" + _sPartReplaced + "';";
            string sPRPD = csCrossClassInteraction.ItemMasterQuery(query);
            if (sPRPD == "N/A")
                sPRPD = string.Empty;
            return sPRPD;
        }

        private bool checkForRefDesPartRep()
        {
            bool bCanSubmit = true;
            string sWarning = "Submission Criteria not met.\n";

            #region Unit Issue 1
            if (!string.IsNullOrEmpty(txtPartReplaced.Text))
            {
                string _sPRPD = getPartReplacedPartDescription(txtPartReplaced.Text);

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
                string _sPRPD = getPartReplacedPartDescription(txtPartReplaced_2.Text);

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
                string _sPRPD = getPartReplacedPartDescription(txtPartReplaced_3.Text);

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
            if(cbTOR.Text.Equals("Credit Return"))
            {
                alReturn.Add(true);
                alReturn.Add("CR");
            }
            else
            {
                bool bSubmit = true;

                if (string.IsNullOrEmpty(txtOrderNumber.Text) && !sUserDepartmentNumber.Equals(sDQE_DeptNum))
                {
                    bSubmit = false;
                    sErrMsg += "-Order Number\n";
                }

                if (string.IsNullOrEmpty(cbTechAction1.Text) && !sUserDepartmentNumber.Equals(sDQE_DeptNum))
                {
                    bSubmit = false;
                    sErrMsg += "-At least 1 Technician Action\n";
                }

                if (string.IsNullOrEmpty(txtCustomerNumber.Text) && !sUserDepartmentNumber.Equals(sDQE_DeptNum))
                {
                    bSubmit = false;
                    sErrMsg += "-Customer Information\n";
                }

                if (string.IsNullOrEmpty(cbTOF.Text))
                {
                    bSubmit = false;
                    sErrMsg += "-Type of Failure\n";
                }

                if(string.IsNullOrEmpty(txtPartName.Text))
                {
                    bSubmit = false;
                    sErrMsg += "-Part Name\n";
                }

                if(string.IsNullOrEmpty(txtPartNumber.Text))
                {
                    bSubmit = false;
                    sErrMsg += "-Part Number\n";
                }

                if(string.IsNullOrEmpty(txtCommSubClass.Text))
                {
                    bSubmit = false;
                    sErrMsg += "-Commodity Sub-Class\n";
                }

                if(string.IsNullOrEmpty(cbTOR.Text))
                {
                    bSubmit = false;
                    sErrMsg += "-Type of Return\n";
                }


                bool bUnitIssueFound = false;
                if (string.IsNullOrEmpty(cbReportedIssue.Text))
                {
                    bSubmit = false;
                    sErrMsg += "-Reported Issue\n";
                }
                else bUnitIssueFound = true;

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
                    sErrMsg += "-At least one unit issue";
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
                                  "TypeOfFailure, HoursOnUnit, ReportedIssue, TestResult, TestResultAbort, Cause, Replacement, PartsReplaced, RefDesignator, AdditionalComments, CustomerNumber, SerialNumber, DateSubmitted, "+
                                  "SubmissionStatus, SaveID, RP, TechAct1, TechAct2, TechAct3, OrderNumber, LineNumber, ProblemCode1, ProblemCode2, RepairCode, TechComments, Series) " +
                                  "VALUES (@Technician, @DateReceived, @PartName, @PartNumber, @CommoditySubClass, @Quantity, @SoftwareVersion, @Scrap, @TypeOfReturn, " + 
                                  "@TypeOfFailure, @HoursOnUnit, @ReportedIssue, @TestResult, @TestResultAbort, @Cause, @Replacement, @PartsReplaced, @RefDesignator, @AdditionalComments, @CustomerNumber, " +
                                  "@SerialNumber, @DateSubmitted, @SubmissionStatus, @SaveID, @RP, @TechAct1, @TechAct2, @TechAct3, @OrderNumber, @LineNumber, @pc1, @pc2, @rc, @tc, @series)";

            SqlConnection conn = new SqlConnection(holder.RepairConnectionString);
            SqlCommand cmd = new SqlCommand(insertQuery, conn);
            try
            {
                conn.Open();

                cmd.Parameters.AddWithValue("@Technician", txtTechName.Text.ToString().TrimEnd());
                cmd.Parameters.AddWithValue("@DateReceived", dtpDateReceived.SelectedDate.Value.ToString("MM/dd/yyyy"));
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

                cmd.Parameters.AddWithValue("@ReportedIssue", csCrossClassInteraction.dbValSubmit(cbReportedIssue.Text));

                #region Unit Issues
                RepairMultipleIssues lUI = getUnitIssueString(0);
                cmd.Parameters.AddWithValue("@TestResult", csCrossClassInteraction.dbValSubmit(lUI.TestResult));
                cmd.Parameters.AddWithValue("@TestResultAbort", csCrossClassInteraction.dbValSubmit(lUI.TestResultAbort));
                cmd.Parameters.AddWithValue("@Cause", csCrossClassInteraction.dbValSubmit(lUI.Cause));
                cmd.Parameters.AddWithValue("@Replacement", csCrossClassInteraction.dbValSubmit(lUI.Replacement));
                cmd.Parameters.AddWithValue("@PartsReplaced", csCrossClassInteraction.dbValSubmit(lUI.MultiPartsReplaced[0].PartReplaced));
                cmd.Parameters.AddWithValue("@RefDesignator", csCrossClassInteraction.dbValSubmit(lUI.MultiPartsReplaced[0].RefDesignator));
                #endregion

                cmd.Parameters.AddWithValue("@AdditionalComments", new TextRange(rtbAdditionalComments.Document.ContentStart, rtbAdditionalComments.Document.ContentEnd).Text.ToString());
                
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
                if(readerID > 0)
                {
                    submitMultipleUnitData(readerID);
                }

                return true;
            }
            catch(Exception ex)
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

            if(_codeType.Equals("PC1"))
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
            List<RepairMultipleIssues> lRMI = getUnitIssues();

            SqlConnection conn = new SqlConnection(holder.RepairConnectionString);
            SqlCommand cmd = new SqlCommand(query, conn);
            try
            {
                for(int i = 0; i < lRMI.Count; i++)
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
                        cmd.Parameters.AddWithValue("partsReplaced", DBNull.Value);
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
                            cmd.Parameters.AddWithValue("partsReplaced", csCrossClassInteraction.unitStripNF(lRMI[i].MultiPartsReplaced[j].PartReplaced));
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
            catch(Exception ex)
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
        private RepairMultipleIssues getUnitIssueString(int iUIData)
        {
            RepairMultipleIssues pmuiReturn = new RepairMultipleIssues();

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
            
            pmuiReturn.TestResult = pmuiReturn.TestResult.TrimEnd(new Char[] { ',', ' ' });
            pmuiReturn.TestResultAbort = pmuiReturn.TestResultAbort.TrimEnd(new Char[] { ',', ' ' });
            pmuiReturn.Cause = pmuiReturn.Cause.TrimEnd(new Char[] { ',', ' ' });
            pmuiReturn.Replacement = pmuiReturn.Replacement.TrimEnd(new Char[] { ',', ' ' });
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
        private List<RepairMultipleIssues> getUnitIssues()
        {
            List<RepairMultipleIssues> lMPUI = new List<RepairMultipleIssues>();

            lMPUI.Add(getUnitIssueString(1));

            if (checkForUITabData(2))
                lMPUI.Add(getUnitIssueString(2));

            if (checkForUITabData(3))
                lMPUI.Add(getUnitIssueString(3));

            return lMPUI;
        }

        private void beginSerialNumberSearch()
        {
            resetForm(false);
            vSleep(500);

            sVar.LogHandler.LogCreation = DateTime.Now;

            if(!String.IsNullOrEmpty(txtBarcode.Text))
            {
                sVar.LogHandler.CreateLogAction("**** This is a Repair Log ****", csLogging.LogState.NOTE);
                sVar.LogHandler.CreateLogAction("The Serial Number related to this log is: " + txtBarcode.Text.TrimEnd(), csLogging.LogState.NOTE);
                fillDataLog();
                fillEOLData();
                fillAOIData();
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
                catch(Exception ex)
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
                        OrderNumberInformation oni = new OrderNumberInformation { 
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

                return bOrderNumberFound;
            }

            if(lOrderNumInfo.Count == 0)
            {
                string sErrMsg = String.Format("No information associated with the Order Number '{0}' could be found. Please verify you have entered the correct Order Number and try again.", txtOrderNumber.Text.ToString());
                MessageBox.Show(sErrMsg, "Order Number Issue", MessageBoxButton.OK, MessageBoxImage.Error);
                sVar.LogHandler.CreateLogAction(sErrMsg, csLogging.LogState.WARNING);
                lblRPNumber.Content = String.Empty;
                sRPNum = String.Empty;
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
                if (String.IsNullOrEmpty(lOrderNumInfo[0].RPNumber.ToString()))
                {
                    bTrueOrderData = false;
                    sDataMissing += "- RP Number\n";
                }
                if (String.IsNullOrEmpty(lOrderNumInfo[0].CustomerNumber.ToString()))
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
                    string sOrdNumDet = String.Format("Order Number Details:\nRP Number: {0}\nCustomer Number: {1}\nLine Number: {2}", lOrderNumInfo[0].RPNumber.ToString(), lOrderNumInfo[0].CustomerNumber.ToString(), lOrderNumInfo[0].LineNumber.ToString());
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
                frmMultipleRP fmrp = new frmMultipleRP(txtOrderNumber.Text.ToString().TrimEnd());
                fmrp.ShowDialog();
                if (!String.IsNullOrEmpty(sVar.SelectedRPNumber.RPNumber))
                {
                    string sDataMissing = "The following items were not loaded from the table CustomerRepairOrderFromJDE:\n";
                    bool bTrueOrderData = true;
                    if (String.IsNullOrEmpty(sVar.SelectedRPNumber.LineNumber.ToString()))
                    {
                        bTrueOrderData = false;
                        sDataMissing += "- Line Number\n";
                    }
                    if (String.IsNullOrEmpty(sVar.SelectedRPNumber.RPNumber.ToString()))
                    {
                        bTrueOrderData = false;
                        sDataMissing += "- RP Number\n";
                    }
                    if (String.IsNullOrEmpty(sVar.SelectedRPNumber.CustInfo.CustomerNumber.ToString()))
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
                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
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
            catch(Exception ex)
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
            //TODO: Log this?

            bool bExistingCustomer = false;
            bool bNoSearchError = true;
            
            #region Check To See If Customer Exists

            string query = "SELECT CustomerNumber FROM RepairCustomerInformation WHERE CustomerNumber = '" + cInfo.CustomerNumber + "'";
            SqlConnection conn = new SqlConnection(holder.RepairConnectionString);
            SqlCommand cmd = new SqlCommand(query, conn);
            try
            {
                string _sCustTest = String.Empty;
                conn.Open();
                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        _sCustTest = reader[0].ToString();
                    }
                }
                conn.Close();

                if (!string.IsNullOrEmpty(_sCustTest))
                    bExistingCustomer = true;
            }
            catch(Exception ex)
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
                    }
                    catch(Exception ex)
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
                    }
                    catch(Exception ex)
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
            if(e.Key.Equals(Key.Enter))
            {
                if (searchOrderNumber())
                    txtBarcode.Focus();
                else { txtOrderNumber.Focus(); txtOrderNumber.SelectAll(); }
            }
        }

        private void txtMultiRefKeyUp(object sender, KeyEventArgs e)
        {
            csCrossClassInteraction.checkForValidRefDes((TextBox)sender);
        }

        private void txtCustomerNumber_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key.Equals(Key.Enter) && !String.IsNullOrEmpty(txtCustomerNumber.Text))
            {
                loadCustomerInformation(txtCustomerNumber.Text.ToString());
            }
        }

        private void cbEOLTestID_DropDownClosed(object sender, EventArgs e)
        {
            if(!String.IsNullOrEmpty(cbEOLTestID.Text))
            {
                initS.InitSplash1("Loading EOL Data...");
                if (lblRPNumber.Content.ToString().Replace("RP Number: ", "").StartsWith("SV")) // this is a transducer so lets do something different
                    csCrossClassInteraction.lsvFillFromQuery(holder.HummingBirdConnectionString, "SELECT * FROM tblXducerTestResultsBenchTest WHERE TestID = '" + cbEOLTestID.Text + "';", lsvEOL);
                else csCrossClassInteraction.lsvFillFromQuery(holder.HummingBirdConnectionString, "SELECT * FROM tblEOL WHERE TestID = '" + cbEOLTestID.Text + "';", lsvEOL);
                csSplashScreenHelper.ShowText("Done...");
                csSplashScreenHelper.Hide();
            }
        }

        private void cbPRETestID_DropDownClosed(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(cbPRETestID.Text))
            {
                initS.InitSplash1("Loading PRE Data...");
                csCrossClassInteraction.lsvFillFromQuery(holder.HummingBirdConnectionString, "SELECT * FROM tblPRE WHERE TestID = '" + cbPRETestID.Text + "';", lsvPreBurnIn);
                csSplashScreenHelper.ShowText("Done...");
                csSplashScreenHelper.Hide();
            }
        }

        private void cbPOSTTestID_DropDownClosed(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(cbPOSTTestID.Text))
            {
                initS.InitSplash1("Loading POST Data...");
                if (lblRPNumber.Content.ToString().Replace("RP Number: ", "").StartsWith("SV")) // this is a transducer so lets do something different
                    csCrossClassInteraction.lsvFillFromQuery(holder.HummingBirdConnectionString, "SELECT * FROM tblXducerTestResults WHERE TestID = '" + cbPOSTTestID.Text + "';", lsvPostBurnIn);
                else csCrossClassInteraction.lsvFillFromQuery(holder.HummingBirdConnectionString, "SELECT * FROM tblPOST WHERE TestID = '" + cbPOSTTestID.Text + "';", lsvPostBurnIn);
                csSplashScreenHelper.ShowText("Done...");
                csSplashScreenHelper.Hide();
            }
        }

        private void cbBEAMSTestType_DropDownClosed(object sender, EventArgs e)
        {
            switch(cbBEAMSTestType.Text)
            {
                case "EOL":
                    csCrossClassInteraction.FillBeamTestIDFromType(cbEOLTestID, cbBEAMSTestID, lsvBeamTestId, cbBEAMSBeamNum);
                    break;
                case "PRE":
                    csCrossClassInteraction.FillBeamTestIDFromType(cbPRETestID, cbBEAMSTestID, lsvBeamTestId, cbBEAMSBeamNum);
                    break;
                case "POST":
                    csCrossClassInteraction.FillBeamTestIDFromType(cbPOSTTestID, cbBEAMSTestID, lsvBeamTestId, cbBEAMSBeamNum);
                    break;
                default:
                    break;
            }
        }

        private void cbBEAMSTestID_DropDownClosed(object sender, EventArgs e)
        {
            if(!String.IsNullOrEmpty(cbBEAMSTestID.Text))
            {
                initS.InitSplash1("Generating Beams...");
                csCrossClassInteraction.BeamsQuery(txtBarcode.Text, cbBEAMSBeamNum, lsvBeamTestId);
                csSplashScreenHelper.ShowText("Done...");
                csSplashScreenHelper.Hide();
            }
        }

        private void cbBEAMSBeamNum_DropDownClosed(object sender, EventArgs e)
        {
            if(!String.IsNullOrEmpty(cbBEAMSTestType.Text) && !String.IsNullOrEmpty(cbBEAMSTestID.Text) && !String.IsNullOrEmpty(cbBEAMSBeamNum.Text))
            {
                initS.InitSplash1("Loading Beam Data...");
                csCrossClassInteraction.BeamsQuery("SELECT * FROM Beams WHERE TestID = '" + cbBEAMSTestID.Text + "' AND PCBSerial = '" + txtBarcode.Text + "' AND BeamNumber = '" + csCrossClassInteraction.GetSpecificBeamNumber(cbBEAMSBeamNum.Text) + "';", lsvBeamTestId);
                csSplashScreenHelper.ShowText("Done...");
                csSplashScreenHelper.Hide();
            }
        }

        private bool getCRStatus(string s)
        {
            if (s.Equals("CR"))
                return true;
            else return false;
        }

        #region Button Clicks
        private void btnComplete_Click(object sender, RoutedEventArgs e)
        {
            sVar.LogHandler.CreateLogAction((Button)sender, csLogging.LogState.CLICK);

            if (checkForRefDesPartRep())
            {
                ArrayList submitStatus = canSubmit();

                if ((bool)submitStatus[0])
                {
                    sVar.LogHandler.CreateLogAction("The submission criteria has been met. Attempting to submit...", csLogging.LogState.NOTE);
                    if (submitData(SubmissionStatus.COMPLETE, -1, getCRStatus(submitStatus[1].ToString())))
                    {
                        sVar.LogHandler.writeLogToFile();
                        MessageBox.Show("Submission Successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        resetForm(true);
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
                long iID = Convert.ToInt64(String.Format("{0}{1}", rdm.Next(1, 9), DateTime.Now.ToString("mmddyyhhmmss")));
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
                long iID = Convert.ToInt64(String.Format("{0}{1}", rdm.Next(1, 10), DateTime.Now.ToString("mmddyyhhmmss")));
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

                        string _sPRPD = getPartReplacedPartDescription(txtPartReplaced_2.Text);

                        if (string.IsNullOrEmpty(_sPRPD) && !string.IsNullOrEmpty(txtPartReplaced_2.Text))
                        {
                            string sWarning = string.Format("The Part Replaced entered ( {0} ) does not exist. Please verify the Part Number and try again.", txtPartReplaced_2.Text);
                            sVar.LogHandler.CreateLogAction(sWarning, csLogging.LogState.WARNING);
                            MessageBox.Show(sWarning, "Part Replaced Description Issue", MessageBoxButton.OK, MessageBoxImage.Warning);
                            txtPartReplaced_2.Focus();
                            txtPartReplaced_2.SelectAll();
                        }
                        else
                        {
                            MultiplePartsReplaced mpr = new MultiplePartsReplaced { RefDesignator = txtRefDes_2.Text.TrimEnd(), PartReplaced = txtPartReplaced_2.Text.TrimEnd(), PartsReplacedPartDescription = _sPRPD };
                            if (string.IsNullOrEmpty(mpr.PartReplaced) && !string.IsNullOrEmpty(mpr.RefDesignator))
                                sVar.LogHandler.CreateLogAction(string.Format("Adding Ref Designator '{0}' to dgMultipleParts. Parts Replaced is empty.", mpr.RefDesignator), csLogging.LogState.NOTE);
                            else if (!string.IsNullOrEmpty(mpr.PartReplaced) && string.IsNullOrEmpty(mpr.RefDesignator))
                                sVar.LogHandler.CreateLogAction(string.Format("Adding Part Replaced '{0}' to dgMultipleParts. Ref Designator is empty.", mpr.PartReplaced), csLogging.LogState.NOTE);
                            else sVar.LogHandler.CreateLogAction(string.Format("Adding Part Replaced '{0}' and Ref Designator '{1}' to dgMultipleParts.", mpr.PartReplaced, mpr.RefDesignator), csLogging.LogState.NOTE);

                            dgMultipleParts_2.Items.Add(mpr);
                            txtPartReplaced_2.Text = txtRefDes_2.Text = String.Empty;
                        }
                    }
                }
                else if (((Control)sender).Name.EndsWith("3"))
                {
                    if (string.IsNullOrEmpty(txtRefDes_3.Text) && string.IsNullOrEmpty(txtPartReplaced_3.Text)) { return; }
                    else
                    {
                        sVar.LogHandler.CreateLogAction((Button)sender, csLogging.LogState.CLICK);

                        string _sPRPD = getPartReplacedPartDescription(txtPartReplaced_3.Text);

                        if (string.IsNullOrEmpty(_sPRPD) && !string.IsNullOrEmpty(txtPartReplaced_3.Text))
                        {
                            string sWarning = String.Format("The Part Replaced entered ( {0} ) does not exist. Please verify the Part Number and try again.", txtPartReplaced_3.Text);
                            sVar.LogHandler.CreateLogAction(sWarning, csLogging.LogState.WARNING);
                            MessageBox.Show(sWarning, "Part Replaced Description Issue", MessageBoxButton.OK, MessageBoxImage.Warning);
                            txtPartReplaced_3.Focus();
                            txtPartReplaced_3.SelectAll();
                        }
                        else
                        {
                            MultiplePartsReplaced mpr = new MultiplePartsReplaced { RefDesignator = txtRefDes_3.Text.TrimEnd(), PartReplaced = txtPartReplaced_3.Text.TrimEnd(), PartsReplacedPartDescription = _sPRPD };
                            if (String.IsNullOrEmpty(mpr.PartReplaced) && !String.IsNullOrEmpty(mpr.RefDesignator))
                                sVar.LogHandler.CreateLogAction(String.Format("Adding Ref Designator '{0}' to dgMultipleParts. Parts Replaced is empty.", mpr.RefDesignator), csLogging.LogState.NOTE);
                            else if (!String.IsNullOrEmpty(mpr.PartReplaced) && String.IsNullOrEmpty(mpr.RefDesignator))
                                sVar.LogHandler.CreateLogAction(String.Format("Adding Part Replaced '{0}' to dgMultipleParts. Ref Designator is empty.", mpr.PartReplaced), csLogging.LogState.NOTE);
                            else sVar.LogHandler.CreateLogAction(String.Format("Adding Part Replaced '{0}' and Ref Designator '{1}' to dgMultipleParts.", mpr.PartReplaced, mpr.RefDesignator), csLogging.LogState.NOTE);

                            dgMultipleParts_3.Items.Add(mpr);
                            txtPartReplaced_3.Text = txtRefDes_3.Text = String.Empty;
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

                    string _sPRPD = getPartReplacedPartDescription(txtPartReplaced.Text);

                    if (string.IsNullOrEmpty(_sPRPD) && !String.IsNullOrEmpty(txtPartReplaced.Text))
                    {
                        string sWarning = String.Format("The Part Replaced entered ( {0} ) does not exist. Please verify the Part Number and try again.", txtPartReplaced.Text);
                        sVar.LogHandler.CreateLogAction(sWarning, csLogging.LogState.WARNING);
                        MessageBox.Show(sWarning, "Part Replaced Description Issue", MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtPartReplaced.Focus();
                        txtPartReplaced.SelectAll();
                    }
                    else
                    {
                        MultiplePartsReplaced mpr = new MultiplePartsReplaced { RefDesignator = txtRefDes.Text.TrimEnd(), PartReplaced = txtPartReplaced.Text.TrimEnd(), PartsReplacedPartDescription = _sPRPD.TrimEnd() };
                        if (String.IsNullOrEmpty(mpr.PartReplaced) && !String.IsNullOrEmpty(mpr.RefDesignator))
                            sVar.LogHandler.CreateLogAction(String.Format("Adding Ref Designator '{0}' to dgMultipleParts. Parts Replaced is empty.", mpr.RefDesignator), csLogging.LogState.NOTE);
                        else if (!String.IsNullOrEmpty(mpr.PartReplaced) && String.IsNullOrEmpty(mpr.RefDesignator))
                            sVar.LogHandler.CreateLogAction(String.Format("Adding Part Replaced '{0}' to dgMultipleParts. Ref Designator is empty.", mpr.PartReplaced), csLogging.LogState.NOTE);
                        else sVar.LogHandler.CreateLogAction(String.Format("Adding Part Replaced '{0}' and Ref Designator '{1}' to dgMultipleParts.", mpr.PartReplaced, mpr.RefDesignator), csLogging.LogState.NOTE);

                        dgMultipleParts.Items.Add(mpr);
                        txtPartReplaced.Text = txtRefDes.Text = String.Empty;
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
            sVar.LogHandler.CreateLogAction(String.Format("The operator selected Part Number '{0}', Part Name '{1}', and Part Series '{2}'.", sVar.SelectedPartNumberPartName.PartNumber, sVar.SelectedPartNumberPartName.PartName, sVar.SelectedPartNumberPartName.PartSeries), csLogging.LogState.NOTE);

            if (!String.IsNullOrEmpty(txtPartNumber.Text))
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
            if (dgPrevRepairInfo.SelectedItem != null)
            {
#if DEBUG
                //TODO: new form that is not working yet
                repairPRI pri = new repairPRI((PreviousRepairInformation)dgPrevRepairInfo.SelectedItem);
                pri.Owner = this;
                pri.ShowDialog();
#else
                // Working form with only one issue that can be displayed
                PrevRepairInfo pri = new PrevRepairInfo((PreviousRepairInformation)dgPrevRepairInfo.SelectedItem);
                pri.ShowDialog();
#endif
            }
        }

        private void txtQTY_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(txtQTY.Text))
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
        private void PreviewTextInput(object sender, TextCompositionEventArgs e)
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
                tbPortName.Content = tbBaudRate.Content = tbParity.Content = tbDataBits.Content = tbStopBits.Content = String.Empty;
                btnRebootSP.Visibility = Visibility.Hidden;
            }
            else if (sp == null)
            {
                tbPortStatus.Content = "Port Status: Does Not Exist";
                tbPortName.Content = tbBaudRate.Content = tbParity.Content = tbDataBits.Content = tbStopBits.Content = String.Empty;
                btnRebootSP.Visibility = Visibility.Hidden;
            }
        }

        private void spDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                sVar.LogHandler.CreateLogAction("Serial Port Data Received - Begin", csLogging.LogState.NOTE);
                Dispatcher.Invoke((Action)delegate
                {
                    txtBarcode.Text = String.Empty;
                });
                string data = String.Empty;
                while(true)
                {
                    if(!sp.IsOpen)
                    {
                        if(sp != null)
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
                        if(data.Length > 0)
                        {
                            if (txtBarcode.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
                                txtBarcode.Text += data;
                            else
                            {
                                Dispatcher.Invoke((Action)delegate
                                {
                                    txtBarcode.Text += data;
                                });
                                data = String.Empty;
                            }
                        }
                    }
                }

                if(data.Length > 0)
                {
                    if (txtBarcode.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
                    {
                        txtBarcode.Text += data;
                        beginSerialNumberSearch();
                    }
                    else
                    {
                        Dispatcher.Invoke((Action)delegate
                        {
                            txtBarcode.Text += data;
                            txtBarcode.Text = txtBarcode.Text.TrimEnd();
                            sVar.LogHandler.CreateLogAction("Serial Port Data Received - End", csLogging.LogState.NOTE);
                            beginSerialNumberSearch();
                        });
                        data = String.Empty;
                    }
                }
            }
            catch(Exception ex)
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
        #endregion

        private void dgBeginEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            e.Cancel = true;
        }

        #region Log Actions
        private void txtGotFocus(object sender, RoutedEventArgs e)
        {
            sVar.LogHandler.CreateLogAction((TextBox)sender, csLogging.LogState.ENTER);
        }

        private void txtLostFocus(object sender, RoutedEventArgs e)
        {
            sVar.LogHandler.CreateLogAction((TextBox)sender, csLogging.LogState.LEAVE);
        }

        private void rtbGotFocus(object sender, RoutedEventArgs e)
        {
            sVar.LogHandler.CreateLogAction((RichTextBox)sender, csLogging.LogState.ENTER);
        }

        private void rtbLostFocus(object sender, RoutedEventArgs e)
        {
            sVar.LogHandler.CreateLogAction((RichTextBox)sender, csLogging.LogState.LEAVE);
        }

        private void cbDDClosed(object sender, EventArgs e)
        {
            sVar.LogHandler.CreateLogAction((ComboBox)sender, csLogging.LogState.DROPDOWNCLOSED);

            if(!((Control)sender).Name.ToString().Equals("cbTOF") && !((Control)sender).Name.ToString().Equals("cbTOR"))
                handleUnitIssues((ComboBox)sender);

            if(((Control)sender).Name.Equals("cbReportedIssue"))
            {
                cbReportedIssue_2.Text = cbReportedIssue_3.Text = cbReportedIssue.Text;
            }

            if(((Control)sender).Name.ToString().Equals("cbTOR"))
            {
                if (cbTOR.Text.Equals("Credit Return"))
                {
                    lblQTY.Visibility = txtQTY.Visibility = Visibility.Visible;
                    cbxNPF.Visibility = System.Windows.Visibility.Visible;
                    cbxNPF.IsEnabled = false;
                    sVar.LogHandler.CreateLogAction("Credit Return was selected. txtQTY and cbxNPF are now visible. cbxNPF is set to disabled.", csLogging.LogState.NOTE);
                }
                else
                {
                    lblQTY.Visibility = txtQTY.Visibility = Visibility.Hidden;
                    cbxNPF.Visibility = System.Windows.Visibility.Hidden;
                    sVar.LogHandler.CreateLogAction("Credit Return was selected. txtQTY and cbxNPF are now hidden.", csLogging.LogState.NOTE);
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
        }

        private void vSleep(int iSleepTime)
        {
            DateTime dt = DateTime.Now.AddMilliseconds(iSleepTime);
            while (DateTime.Now < dt)
            { }
        }
    }
}