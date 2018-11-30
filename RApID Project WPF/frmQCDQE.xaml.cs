using System;
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
using EricStabileLibrary;
using System.IO.Ports;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Windows.Threading;

namespace RApID_Project_WPF
{
    /// <summary>
    /// Interaction logic for frmQCDQE.xaml
    /// </summary>
    public partial class frmQCDQE : Window
    {
        SerialPort sp;
        DispatcherTimer tSPChecker;
        bool bTimerRebootAttempt = false;
        QCDQEPageLoad ScannedUnitInformation = new QCDQEPageLoad();
        csObjectHolder.csObjectHolder holder = csObjectHolder.csObjectHolder.ObjectHolderInstance();

        InitSplash initS = new InitSplash();
        StaticVars sVars = StaticVars.StaticVarsInstance();

        public frmQCDQE()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            initS.InitSplash1("Initializing form...");
            buildDGViews();
            csSplashScreenHelper.ShowText("Handling Serial Port...");
            handleInitSerialPort();
            csSplashScreenHelper.ShowText("Waking up...");
            txtQCDQETech.Text = System.Environment.UserName;
            txtRepairBarcode.Focus();
            dtpDateReceived.SelectedDate = DateTime.Now;
            csSplashScreenHelper.ShowText("Preparing logs...");
            setupLogging();

            GC.Collect();
            csSplashScreenHelper.ShowText("Done!");
            csSplashScreenHelper.Hide();
            this.Activate();

            #if DEBUG
                txtRepairBarcode.Text = "123456";
            #endif
            
        }

        private void setupLogging()
        {
            try
            {
                sVars.LogHandler.CheckDirectory(Environment.UserName);
            }catch { }
        }

        private void buildDGViews()
        {
            csCrossClassInteraction.dgBuildView(dgMultipleParts, "MULTIPLEPARTS");
            csCrossClassInteraction.dgBuildView(dgPrevRepairInfo, "PREVREPAIRINFO");
        }

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

        private void resetForm(bool bCompleteReset)
        {
            sVars.resetStaticVars();

            if (bCompleteReset)
            {
                txtRepairBarcode.Text = string.Empty;
                cbxScanSwitch.IsChecked = false;
                lblRepairBarcode.Content = "Scan Repair Label:";
            }

            foreach(UIElement uie in gMainGrid.Children)
            {
                if (uie.GetType().Name.Equals("TextBox"))
                {
                    var txtBox = (TextBox)uie;
                    txtBox.Text = string.Empty;
                }
            }

            foreach (UIElement uie in gUI1.Children)
            {
                if (uie.GetType().Name.Equals("TextBox"))
                {
                    var txtBox = (TextBox)uie;
                    txtBox.Text = string.Empty;
                }
            }

            foreach (UIElement uie in gUI2.Children)
            {
                if (uie.GetType().Name.Equals("TextBox"))
                {
                    var txtBox = (TextBox)uie;
                    txtBox.Text = string.Empty;
                }
            }

            foreach (UIElement uie in gUI3.Children)
            {
                if (uie.GetType().Name.Equals("TextBox"))
                {
                    var txtBox = (TextBox)uie;
                    txtBox.Text = string.Empty;
                }
            }

            dgMultipleParts.Items.Clear();
            dgMultipleParts2.Items.Clear();
            dgMultipleParts3.Items.Clear();

            tiUI2.IsEnabled = tiUI3.IsEnabled = false;
            tiUI1.Focus();

            foreach(UIElement uie in gTechAction.Children)
            {
                if(uie.GetType().Name.Equals("TextBox"))
                {
                    var txtBox = (TextBox)uie;
                    txtBox.Text = string.Empty;
                }
            }

            foreach(UIElement uie in gCustInfo.Children)
            {
                if(uie.GetType().Name.Equals("TextBox"))
                {
                    var txtBox = (TextBox)uie;
                    txtBox.Text = string.Empty;
                }
            }

            dtpDateReceived.SelectedDate = DateTime.Now;
            cbxScrap.IsChecked = false;
            dgPrevRepairInfo.Items.Clear();
            rtbAdditionalComments.Document.Blocks.Clear();
            rtbQCDQEComments.Document.Blocks.Clear();

            ScannedUnitInformation = new QCDQEPageLoad();
            txtRepairBarcode.Focus();
            txtQCDQETech.Text = System.Environment.UserName;
        }

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

        private void beginSerialNumberSearch()
        {
            sVars.LogHandler.LogCreation = DateTime.Now;

            resetForm(false);
            if(!string.IsNullOrEmpty(txtRepairBarcode.Text))
            {
                sVars.LogHandler.CreateLogAction("**** This is a DQE/QC Log ****", csLogging.LogState.NOTE);
                sVars.LogHandler.CreateLogAction($"The Serial Number related to this log is: {txtSN.Text.TrimEnd()}", csLogging.LogState.NOTE);
                fillDataLog();
            }
        }

        /// <summary>
        /// Fills txtSWVersion based on the given Serial Number
        /// </summary>
        private void fillSoftwareVersion()
        {
            string query = $"SELECT TOP(5) SoftwareVersion FROM tblPost WHERE PCBSerial = '{txtRepairBarcode.Text.TrimEnd()}' ORDER BY [DateAndTime] DESC";
            sVars.LogHandler.CreateLogAction("Attempting to fill the Software Version.", csLogging.LogState.NOTE);
            csCrossClassInteraction.txtFillFromQuery(query, txtSWVersion);
            txtSWVersion.Text = txtSWVersion.Text.Split(',')[0];
        }

        private void fillDataLog()
        {
            string sRefDesignator = string.Empty;
            string sPartReplaced = string.Empty;
            string query = "SELECT * FROM TechnicianSubmission WHERE SaveID = '" + txtRepairBarcode.Text + "' ORDER BY ID DESC;";

            if(!(bool)cbxScanSwitch.IsChecked && txtRepairBarcode.Text.Length == 12 && 
                MessageBox.Show("This looks like a serial number.\nDid you mean to click the checkbox for scanning Serial Numbers?",
                    "Serial Number Format Recognized", MessageBoxButton.YesNo, MessageBoxImage.Warning)==MessageBoxResult.Yes)
            {
                cbxScanSwitch.IsChecked = true;
                cbxScanSwitch_Click(null, null);
            }

            if((bool)cbxScanSwitch.IsChecked)
            {
                query = "SELECT * FROM TechnicianSubmission WHERE SerialNumber = '" + txtRepairBarcode.Text + "' ORDER BY ID DESC;";
            }

            var conn = new SqlConnection(holder.RepairConnectionString);
            var cmd = new SqlCommand(query, conn);
            try
            {
                sVars.LogHandler.CreateLogAction("Attempting to get the Scanned Unit Information...", csLogging.LogState.NOTE);

                ScannedUnitInformation = new QCDQEPageLoad();
                ScannedUnitInformation.LoadSuccessful = false;
                conn.Open();
                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        ScannedUnitInformation.CustomerInformation = new CustomerInformation();
                        ScannedUnitInformation.QCandDQEComments = new QCDQETechComments();

                        ScannedUnitInformation.ID = Convert.ToInt32(reader["ID"].ToString());
                        ScannedUnitInformation.Technician = csCrossClassInteraction.EmptyIfNull(reader["Technician"].ToString());
                        ScannedUnitInformation.PartName = csCrossClassInteraction.EmptyIfNull(reader["PartName"].ToString());
                        ScannedUnitInformation.PartNumber = csCrossClassInteraction.EmptyIfNull(reader["PartNumber"].ToString());
                        ScannedUnitInformation.PartSeries = csCrossClassInteraction.EmptyIfNull(reader["Series"].ToString());
                        ScannedUnitInformation.CommoditySubClass = csCrossClassInteraction.EmptyIfNull(reader["CommoditySubClass"].ToString());
                        ScannedUnitInformation.SoftwareVersion = csCrossClassInteraction.EmptyIfNull(reader["SoftwareVersion"].ToString());
                        ScannedUnitInformation.Quantity = csCrossClassInteraction.EmptyIfNull(reader["Quantity"].ToString());
                        
                        if (!reader["DateReceived"].Equals(DBNull.Value))
                            ScannedUnitInformation.DateReceived = Convert.ToDateTime(reader["DateReceived"].ToString());
                        
                        ScannedUnitInformation.SerialNumber = csCrossClassInteraction.EmptyIfNull(reader["SerialNumber"].ToString());
                        ScannedUnitInformation.TypeOfReturn = csCrossClassInteraction.EmptyIfNull(reader["TypeOfReturn"].ToString());
                        ScannedUnitInformation.TypeOfFailure = csCrossClassInteraction.EmptyIfNull(reader["TypeOfFailure"].ToString());
                        ScannedUnitInformation.HoursOnUnit = csCrossClassInteraction.EmptyIfNull(reader["HoursOnUnit"].ToString());
                        ScannedUnitInformation.AdditionalComments = csCrossClassInteraction.EmptyIfNull(reader["AdditionalComments"].ToString());
                        ScannedUnitInformation.TechAction1 = csCrossClassInteraction.EmptyIfNull(reader["TechAct1"].ToString());
                        ScannedUnitInformation.TechAction2 = csCrossClassInteraction.EmptyIfNull(reader["TechAct2"].ToString());
                        ScannedUnitInformation.TechAction3 = csCrossClassInteraction.EmptyIfNull(reader["TechAct3"].ToString());
                        ScannedUnitInformation.CustomerInformation.CustomerNumber = csCrossClassInteraction.EmptyIfNull(reader["CustomerNumber"].ToString());
                        ScannedUnitInformation.QCandDQEComments.FullTechList = csCrossClassInteraction.EmptyIfNull(reader["Quality"].ToString());
                        ScannedUnitInformation.QCandDQEComments.FullTechComments = csCrossClassInteraction.EmptyIfNull(reader["QCDQEComments"].ToString());
                        ScannedUnitInformation.LoadSuccessful = true;
                        break;
                    }
                }
                conn.Close();

                sVars.LogHandler.CreateLogAction($"The Unit Information was found for {ScannedUnitInformation.SerialNumber}. " +
                    $"It was a {ScannedUnitInformation.PartName}. The series was {ScannedUnitInformation.PartSeries}", csLogging.LogState.NOTE);

                getUnitIssueInfo();

                if(ScannedUnitInformation.CustomerInformation != null && !string.IsNullOrEmpty(ScannedUnitInformation.CustomerInformation.CustomerNumber))
                    ScannedUnitInformation.CustomerInformation = csCrossClassInteraction.CustomerInformationQuery(ScannedUnitInformation.CustomerInformation.CustomerNumber);

                populateFormWithData();

                fillSoftwareVersion();

                if (!string.IsNullOrEmpty(txtSN.Text))
                    QueryTechReport();

                rtbQCDQEComments.Focus();
            }
            catch(Exception ex)
            {
                if (conn != null)
                    conn.Close();
                MessageBox.Show("There was an issue loading the unit information.\nError Message: " + ex.Message, "DataLog Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void getUnitIssueInfo()
        {
            var lRMI = new List<RepairMultipleIssues>();
            string query = "SELECT * FROM TechnicianUnitIssues WHERE ID = '" + ScannedUnitInformation.ID + "' ORDER BY ID DESC";
            var conn = new SqlConnection(holder.RepairConnectionString);
            var cmd = new SqlCommand(query, conn);

            try
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        lRMI.Add(new RepairMultipleIssues()
                        {
                            ID = Convert.ToInt32(reader["ID"].ToString()),
                            ReportedIssue = reader["ReportedIssue"].ToString(),
                            TestResult = reader["TestResult"].ToString(),
                            TestResultAbort = reader["TestResultAbort"].ToString(),
                            Cause = reader["Cause"].ToString(),
                            Replacement = reader["Replacement"].ToString(),
                            SinglePartReplaced = new MultiplePartsReplaced()
                            {
                                PartReplaced = reader["PartsReplaced"].ToString(),
                                RefDesignator = reader["RefDesignator"].ToString(),
                                PartsReplacedPartDescription = reader["PartsReplacedPartDescription"].ToString()
                            }
                        });
                    }
                }
                conn.Close();

                ScannedUnitInformation.UnitIssues = lRMI;

            }
            catch(Exception ex)
            {
                if (conn != null)
                    conn.Close();

                MessageBox.Show("Error loading the unit issue for this particular serial number.\nError Message: " + ex.Message, "Issue Loading Unit Issue Information", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void populateFormWithData()
        {
            txtTechName.Text = csCrossClassInteraction.EmptyIfNull(ScannedUnitInformation.Technician);
            dtpDateReceived.Text = csCrossClassInteraction.EmptyIfNull(ScannedUnitInformation.DateReceived.ToString("MM/dd/yyyy"));
            txtSN.Text = csCrossClassInteraction.EmptyIfNull(ScannedUnitInformation.SerialNumber);
            txtPartName.Text = csCrossClassInteraction.EmptyIfNull(ScannedUnitInformation.PartName);
            txtPartNumber.Text = csCrossClassInteraction.EmptyIfNull(ScannedUnitInformation.PartNumber);
            txtPartSeries.Text = csCrossClassInteraction.EmptyIfNull(ScannedUnitInformation.PartSeries);
            txtCommSubClass.Text = csCrossClassInteraction.EmptyIfNull(ScannedUnitInformation.CommoditySubClass);
            txtSWVersion.Text = csCrossClassInteraction.EmptyIfNull(ScannedUnitInformation.SoftwareVersion);
            txtQTY.Text = csCrossClassInteraction.EmptyIfNull(ScannedUnitInformation.Quantity);
            txtTOR.Text = csCrossClassInteraction.EmptyIfNull(ScannedUnitInformation.TypeOfReturn);
            txtTOF.Text = csCrossClassInteraction.EmptyIfNull(ScannedUnitInformation.TypeOfFailure);
            txtHOU.Text = csCrossClassInteraction.EmptyIfNull(ScannedUnitInformation.HoursOnUnit);

            if (ScannedUnitInformation.UnitIssues.Count > 0)
            {
                foreach (RepairMultipleIssues rmi in ScannedUnitInformation.UnitIssues)
                {
                    if (rmi != null)
                    {
                        txtReportedIssue.Text = rmi.ReportedIssue;
                        txtTestResult.Text = rmi.TestResult;
                        txtTestResultAbort.Text = rmi.TestResultAbort;
                        txtCause.Text = rmi.Cause;
                        txtReplacement.Text = rmi.Replacement;
                        if (!string.IsNullOrEmpty(rmi.SinglePartReplaced.PartReplaced) || !string.IsNullOrEmpty(rmi.SinglePartReplaced.RefDesignator))
                        {
                            dgMultipleParts.Items.Add(rmi.SinglePartReplaced);
                        }
                    }
                }
            }

            txtTechAct1.Text = csCrossClassInteraction.EmptyIfNull(ScannedUnitInformation.TechAction1);
            txtTechAct2.Text = csCrossClassInteraction.EmptyIfNull(ScannedUnitInformation.TechAction2);
            txtTechAct3.Text = csCrossClassInteraction.EmptyIfNull(ScannedUnitInformation.TechAction3);

            if (ScannedUnitInformation.CustomerInformation != null)
            {
                txtCustomerNumber.Text = csCrossClassInteraction.EmptyIfNull(ScannedUnitInformation.CustomerInformation.CustomerNumber);
                txtCustomerName.Text = csCrossClassInteraction.EmptyIfNull(ScannedUnitInformation.CustomerInformation.CustomerName);
            }

            if(ScannedUnitInformation.QCandDQEComments != null && !string.IsNullOrEmpty(ScannedUnitInformation.QCandDQEComments.FullTechComments))
                rtbQCDQEComments.AppendText(Environment.NewLine + ScannedUnitInformation.QCandDQEComments.FullTechComments);
        }

        private void QueryTechReport()
        {
            sVars.LogHandler.CreateLogAction("Querying the Tech Report for previous tech information from the tblManufacturingTechReport table.", csLogging.LogState.NOTE);

            //NOTE: Old DB
            string query = "SELECT Date_Time, Technician FROM tblManufacturingTechReport WHERE SerialNumber = '" + txtSN.Text.TrimEnd() + "';";
            csCrossClassInteraction.dgTechReport(query, true, dgPrevRepairInfo, txtSN.Text.TrimEnd());
            sVars.LogHandler.CreateLogAction("SQL QUERY: " + query, csLogging.LogState.SQLQUERY);

            sVars.LogHandler.CreateLogAction("Querying the Tech Report for previous tech information from the TechnicianSubmission table.", csLogging.LogState.NOTE);
            sVars.LogHandler.CreateLogAction("SQL QUERY: " + query, csLogging.LogState.SQLQUERY);

            //NOTE: New DB
            query = "SELECT DateSubmitted, Technician, ID FROM TechnicianSubmission WHERE SerialNumber = '" + txtSN.Text.TrimEnd() + "';";
            csCrossClassInteraction.dgTechReport(query, false, dgPrevRepairInfo, txtSN.Text.TrimEnd());
        }

        private string prepTechForSubmission()
        {
            string[] splitters = { "," };
            string _sTechSubmit = string.Empty;

            if (ScannedUnitInformation.QCandDQEComments.FullTechList.Contains(","))
            {
                string[] sSplit = ScannedUnitInformation.QCandDQEComments.FullTechList.Split(splitters, StringSplitOptions.RemoveEmptyEntries);
                bool bAddTechToList = true;

                foreach (string s in sSplit)
                {
                    if (s.Equals(txtQCDQETech.Text))
                    {
                        bAddTechToList = false;
                        break;
                    }
                }

                if (bAddTechToList)
                    _sTechSubmit = txtQCDQETech.Text + "," + ScannedUnitInformation.QCandDQEComments.FullTechList;
                else _sTechSubmit = ScannedUnitInformation.QCandDQEComments.FullTechList;
            }
            else if(!string.IsNullOrEmpty(ScannedUnitInformation.QCandDQEComments.FullTechList))
            {
                if (!txtQCDQETech.Text.Equals(ScannedUnitInformation.QCandDQEComments.FullTechList))
                    _sTechSubmit = txtQCDQETech.Text + "," + ScannedUnitInformation.QCandDQEComments.FullTechList;
            }
            else
            {
                _sTechSubmit = txtQCDQETech.Text;
            }


            return _sTechSubmit;
        }

        private string prepareCommentsForSubmission()
        {
            string sReturnComments = "";

            sReturnComments = txtQCDQETech.Text + ":" + DateTime.Now.ToString("MM/dd/yyyy") + ": " + new TextRange(rtbQCDQEComments.Document.ContentStart, rtbQCDQEComments.Document.ContentEnd).Text.ToString().Trim();

            return sReturnComments;
        }

        private QCDQESubmitData prepDataForSubmission(string _submitStatus)
        {
            var _submitInfo = new QCDQESubmitData();

            if((bool)cbxScanSwitch.IsChecked)
            {
                _submitInfo.SubmitQuery = "UPDATE TechnicianSubmission SET Quality = @quality, SubmissionStatus = @submissionStatus, QCDQEComments = @QCDQEComments, QCDQEDateSubmitted = @QCDQEDateSubmit " +
                                          "WHERE ID = '" + ScannedUnitInformation.ID + "'";
            }
            else
            {
                _submitInfo.SubmitQuery = "UPDATE TechnicianSubmission SET Quality = @quality, SubmissionStatus = @submissionStatus, QCDQEComments = @QCDQEComments, QCDQEDateSubmitted = @QCDQEDateSubmit " +
                                          "WHERE SaveID = '" + txtRepairBarcode.Text.TrimEnd() + "'";
            }

            _submitInfo.QualityTech = prepTechForSubmission();
            _submitInfo.QCDQEComments = prepareCommentsForSubmission();
            _submitInfo.SubmissionStatus = _submitStatus;
            _submitInfo.QCDQEDateSubmit = DateTime.Now;

            return _submitInfo;
        }

        private void submitData(QCDQESubmitData _submitData)
        {
            var conn = new SqlConnection(holder.RepairConnectionString);
            var cmd = new SqlCommand(_submitData.SubmitQuery, conn);
            try
            {
                var sLogString = "Attemping to submit the data with the following parameters to the TechnicianSubmission Table:\n";

                conn.Open();
                cmd.Parameters.AddWithValue("@quality", _submitData.QualityTech);
                cmd.Parameters.AddWithValue("@submissionStatus", _submitData.SubmissionStatus);
                cmd.Parameters.AddWithValue("@QCDQEComments", _submitData.QCDQEComments);
                cmd.Parameters.AddWithValue("@QCDQEDateSubmit", _submitData.QCDQEDateSubmit);
                cmd.ExecuteNonQuery();
                conn.Close();

                for (int i = 0; i < cmd.Parameters.Count; i++)
                {
                    var paramName = cmd.Parameters[i].ToString();
                    if (string.IsNullOrEmpty(cmd.Parameters[i].Value.ToString()))
                        sLogString += paramName + ": N/A\n";
                    else sLogString += $"{paramName}: {cmd.Parameters[i].Value.ToString()}\n";
                    
                }
                sVars.LogHandler.CreateLogAction(sLogString, csLogging.LogState.SUBMISSIONDETAILS);
                sVars.LogHandler.CreateLogAction("Submission Successful!", csLogging.LogState.NOTE);

                MessageBox.Show("The QC/DQE submission was successful.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                resetForm(true);
            }
            catch(Exception ex)
            {
                if (conn != null)
                    conn.Close();

                MessageBox.Show("There was an issue submitting the data.\nError Message: " + ex.Message, "Submission Error", MessageBoxButton.OK, MessageBoxImage.Error);
                sVars.LogHandler.CreateLogAction($"Error attempting to submit the data.\nError Message: {ex.Message}\nStack Trace: {ex.StackTrace}", csLogging.LogState.ERROR);
            }
        }

        private void dgBeginEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            e.Cancel = true;
        }

        private void submitButtonClicks(string submissionStatus)
        {
            bool bCanSubmit = false;
            if (!string.IsNullOrEmpty(txtRepairBarcode.Text) && ScannedUnitInformation.LoadSuccessful)
            {
                if (ScannedUnitInformation.QCandDQEComments != null && !string.IsNullOrEmpty(new TextRange(rtbQCDQEComments.Document.ContentStart, rtbQCDQEComments.Document.ContentEnd).Text.ToString().Trim()))
                {
                    string _qcComments = new TextRange(rtbQCDQEComments.Document.ContentStart, rtbQCDQEComments.Document.ContentEnd).Text.ToString().Trim();
                    if (_qcComments.Length != ScannedUnitInformation.QCandDQEComments.FullTechComments.Length)
                        bCanSubmit = true;
                    else
                    {
                        MessageBox.Show("Unable to submit. No new QC/DQE comments have been entered.", "Submission Criteria Not Met", MessageBoxButton.OK, MessageBoxImage.Information);
                        bCanSubmit = false;
                    }
                }
                else if (string.IsNullOrEmpty(new TextRange(rtbQCDQEComments.Document.ContentStart, rtbQCDQEComments.Document.ContentEnd).Text.ToString().Trim()))
                {
                    MessageBox.Show("Unable to submit. No QC/DQE comments have been entered.", "Submission Criteria Not Met", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                if (!ScannedUnitInformation.LoadSuccessful)
                {
                    MessageBox.Show("Unable to submit due to no unit being scanned in.", "Submission Criteria Not Met", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            if (bCanSubmit)
            {
                QCDQESubmitData sd = prepDataForSubmission(submissionStatus);
                submitData(sd);
            }
        }

        #region Button Click

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
                    MessageBox.Show("Unable to reboot the Com Port. The Com Port does not exist.", "COM Port Issue", MessageBoxButton.OK, MessageBoxImage.Error); 
                }
            }
            catch { }

            serialPortStatusUpdate();
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            sVars.LogHandler.CreateLogAction((Button)sender, csLogging.LogState.CLICK);
            submitButtonClicks("COMPLETE");
        }

        private void btnSendToQC_Click(object sender, RoutedEventArgs e)
        {
            sVars.LogHandler.CreateLogAction((Button)sender, csLogging.LogState.CLICK);

            submitButtonClicks("SENDTOQC");
        }

        private void btnSendToDQE_Click(object sender, RoutedEventArgs e)
        {
            sVars.LogHandler.CreateLogAction((Button)sender, csLogging.LogState.CLICK);
            submitButtonClicks("SENDTODQE");
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            sVars.LogHandler.CreateLogAction((Button)sender, csLogging.LogState.CLICK);
            resetForm(true);
        }

        private void btnViewFullCustInfo_Click(object sender, RoutedEventArgs e)
        {
            if (ScannedUnitInformation.CustomerInformation != null)
            {
                if (!string.IsNullOrEmpty(ScannedUnitInformation.CustomerInformation.CustomerNumber))
                {
                    var fullInfo = new frmFullCustomerInformation(ScannedUnitInformation.CustomerInformation);
                    fullInfo.Closed += delegate { btnViewFullCustInfo.IsEnabled = true; };
                    fullInfo.Show();
                    btnViewFullCustInfo.IsEnabled = false;
                }
            }
        }

        #endregion

        #region Log Events
        private void cbDDClosed(object sender, EventArgs e) 
            => sVars.LogHandler.CreateLogAction((ComboBox)sender, csLogging.LogState.DROPDOWNCLOSED);

        private void rtbGotFocus(object sender, RoutedEventArgs e) 
            => sVars.LogHandler.CreateLogAction((RichTextBox)sender, csLogging.LogState.ENTER);

        private void rtbLostFocus(object sender, RoutedEventArgs e) 
            => sVars.LogHandler.CreateLogAction((RichTextBox)sender, csLogging.LogState.LEAVE);

        private void txtGotFocus(object sender, RoutedEventArgs e) 
            => sVars.LogHandler.CreateLogAction((TextBox)sender, csLogging.LogState.ENTER);

        private void txtLostFocus(object sender, RoutedEventArgs e) 
            => sVars.LogHandler.CreateLogAction((TextBox)sender, csLogging.LogState.LEAVE);
        #endregion

        private void txtRepairBarcode_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(txtRepairBarcode.Text.EndsWith("\r") || txtRepairBarcode.Text.EndsWith("\n"))
            {
                txtRepairBarcode.Text = txtRepairBarcode.Text.TrimEnd();
                beginSerialNumberSearch();
            }
        }

        private void dgPrevRepairInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgPrevRepairInfo.SelectedItem != null)
            {
                var pri = new PrevRepairInfo((PreviousRepairInformation)dgPrevRepairInfo.SelectedItem);
                pri.ShowDialog();
            }
        }

        private void txtRepairBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
                beginSerialNumberSearch();
        }

        private void cbxScanSwitch_Click(object sender, RoutedEventArgs e)
        {
            if((bool)cbxScanSwitch.IsChecked)
            {
                lblRepairBarcode.Content = "Scan Serial Number:";
            }
            else
            {
                lblRepairBarcode.Content = "Scan Repair Label:";
            }
        }

        private void spDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Dispatcher.Invoke((Action)delegate
                {
                    txtRepairBarcode.Text = string.Empty;
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
                            if (txtRepairBarcode.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
                                txtRepairBarcode.Text += data;
                            else
                            {
                                Dispatcher.Invoke((Action)delegate
                                {
                                    txtRepairBarcode.Text += data;
                                });
                                data = string.Empty;
                            }
                        }
                    }
                }

                if (data.Length > 0)
                {
                    if (txtRepairBarcode.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
                    {
                        txtRepairBarcode.Text += data;
                        beginSerialNumberSearch();
                    }
                    else
                    {
                        Dispatcher.Invoke((Action)delegate
                        {
                            txtRepairBarcode.Text += data;
                            txtRepairBarcode.Text = txtRepairBarcode.Text.TrimEnd();
                            beginSerialNumberSearch();
                        });
                        data = string.Empty;
                    }
                }

                using (var mapper = csSerialNumberMapper.Instance)
                {
                    Task.Factory.StartNew(new Action(() =>
                    {
                        Dispatcher.Invoke(delegate // perform actions on dispatched thread
                        {
                            if (!mapper.GetData(txtSN.Text))
                                throw new InvalidOperationException("Couldn't find data for this barcode!");
                            else
                            {
                                var result = mapper.FindFile(".xls");
                                //TODO: Make User Control for BOM Matcher (2 cmbx: Ref Des. -> Part Number)
                            }
                        });
                    }));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error receiving data from the COM Port.\n\nError Message: " + ex.Message, "spDataReceived()", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sp != null && sp.IsOpen)
                sp.Close();
            sp = null;

            if (tSPChecker != null && tSPChecker.IsEnabled)
                tSPChecker.Stop();
            tSPChecker = null;
        }
    }
}
