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
using EricStabileLibrary;
using System.IO.Ports;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Windows.Threading;

//TODO: Add logging to the QC/DQE page.

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


        public frmQCDQE()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            buildDGViews();
            handleInitSerialPort();
            txtQCDQETech.Text = System.Environment.UserName;
            txtRepairBarcode.Focus();
            dtpDateReceived.SelectedDate = DateTime.Now;
#if DEBUG
            txtRepairBarcode.Text = "123456";
#endif
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
            resetForm(false);
            if(!string.IsNullOrEmpty(txtRepairBarcode.Text))
            {
                fillDataLog();
            }
        }

        private void fillDataLog()
        {
            string sRefDesignator = string.Empty;
            string sPartReplaced = string.Empty;
            string query = "SELECT * FROM TechnicianSubmission WHERE SaveID = '" + txtRepairBarcode.Text + "' ORDER BY ID DESC;";

            if((bool)cbxScanSwitch.IsChecked)
            {
                query = "SELECT * FROM TechnicianSubmission WHERE SerialNumber = '" + txtRepairBarcode.Text + "' ORDER BY ID DESC;";
            }

            var conn = new SqlConnection(holder.RepairConnectionString);
            var cmd = new SqlCommand(query, conn);
            try
            {
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
                        ScannedUnitInformation.Technician = csCrossClassInteraction.dbValSubmit(reader["Technician"].ToString());
                        ScannedUnitInformation.PartName = csCrossClassInteraction.dbValSubmit(reader["PartName"].ToString());
                        ScannedUnitInformation.PartNumber = csCrossClassInteraction.dbValSubmit(reader["PartNumber"].ToString());
                        ScannedUnitInformation.PartSeries = csCrossClassInteraction.dbValSubmit(reader["Series"].ToString());
                        ScannedUnitInformation.CommoditySubClass = csCrossClassInteraction.dbValSubmit(reader["CommoditySubClass"].ToString());
                        ScannedUnitInformation.SoftwareVersion = csCrossClassInteraction.dbValSubmit(reader["SoftwareVersion"].ToString());
                        ScannedUnitInformation.Quantity = csCrossClassInteraction.dbValSubmit(reader["Quantity"].ToString());
                        
                        if (!reader["DateReceived"].Equals(DBNull.Value))
                            ScannedUnitInformation.DateReceived = Convert.ToDateTime(reader["DateReceived"].ToString());
                        
                        ScannedUnitInformation.SerialNumber = csCrossClassInteraction.dbValSubmit(reader["SerialNumber"].ToString());
                        ScannedUnitInformation.TypeOfReturn = csCrossClassInteraction.dbValSubmit(reader["TypeOfReturn"].ToString());
                        ScannedUnitInformation.TypeOfFailure = csCrossClassInteraction.dbValSubmit(reader["TypeOfFailure"].ToString());
                        ScannedUnitInformation.HoursOnUnit = csCrossClassInteraction.dbValSubmit(reader["HoursOnUnit"].ToString());
                        ScannedUnitInformation.AdditionalComments = csCrossClassInteraction.dbValSubmit(reader["AdditionalComments"].ToString());
                        ScannedUnitInformation.TechAction1 = csCrossClassInteraction.dbValSubmit(reader["TechAct1"].ToString());
                        ScannedUnitInformation.TechAction2 = csCrossClassInteraction.dbValSubmit(reader["TechAct2"].ToString());
                        ScannedUnitInformation.TechAction3 = csCrossClassInteraction.dbValSubmit(reader["TechAct3"].ToString());
                        ScannedUnitInformation.CustomerInformation.CustomerNumber = csCrossClassInteraction.dbValSubmit(reader["CustomerNumber"].ToString());
                        ScannedUnitInformation.QCandDQEComments.FullTechList = csCrossClassInteraction.dbValSubmit(reader["Quality"].ToString());
                        ScannedUnitInformation.QCandDQEComments.FullTechComments = csCrossClassInteraction.dbValSubmit(reader["QCDQEComments"].ToString());
                        ScannedUnitInformation.LoadSuccessful = true;
                        break;
                    }
                }
                conn.Close();

                getUnitIssueInfo();

                if(ScannedUnitInformation.CustomerInformation != null && !string.IsNullOrEmpty(ScannedUnitInformation.CustomerInformation.CustomerNumber))
                    ScannedUnitInformation.CustomerInformation = csCrossClassInteraction.CustomerInformationQuery(ScannedUnitInformation.CustomerInformation.CustomerNumber);

                populateFormWithData();

                if(!string.IsNullOrEmpty(txtSN.Text))
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
            txtTechName.Text = csCrossClassInteraction.dbValSubmit(ScannedUnitInformation.Technician);
            dtpDateReceived.Text = csCrossClassInteraction.dbValSubmit(ScannedUnitInformation.DateReceived.ToString("MM/dd/yyyy"));
            txtSN.Text = csCrossClassInteraction.dbValSubmit(ScannedUnitInformation.SerialNumber);
            txtPartName.Text = csCrossClassInteraction.dbValSubmit(ScannedUnitInformation.PartName);
            txtPartNumber.Text = csCrossClassInteraction.dbValSubmit(ScannedUnitInformation.PartNumber);
            txtPartSeries.Text = csCrossClassInteraction.dbValSubmit(ScannedUnitInformation.PartSeries);
            txtCommSubClass.Text = csCrossClassInteraction.dbValSubmit(ScannedUnitInformation.CommoditySubClass);
            txtSWVersion.Text = csCrossClassInteraction.dbValSubmit(ScannedUnitInformation.SoftwareVersion);
            txtQTY.Text = csCrossClassInteraction.dbValSubmit(ScannedUnitInformation.Quantity);
            txtTOR.Text = csCrossClassInteraction.dbValSubmit(ScannedUnitInformation.TypeOfReturn);
            txtTOF.Text = csCrossClassInteraction.dbValSubmit(ScannedUnitInformation.TypeOfFailure);
            txtHOU.Text = csCrossClassInteraction.dbValSubmit(ScannedUnitInformation.HoursOnUnit);

            //TODO: 99% of the data only has one unit issue. Will expand to multiple in a future update.
            RepairMultipleIssues rmi = null;
            if(ScannedUnitInformation.UnitIssues.Count > 0)
                rmi = ScannedUnitInformation.UnitIssues[0];

            if(rmi != null)
            {
                txtReportedIssue.Text = rmi.ReportedIssue;
                txtTestResult.Text = rmi.TestResult;
                txtTestResultAbort.Text = rmi.TestResultAbort;
                txtCause.Text = rmi.Cause;
                txtReplacement.Text = rmi.Replacement;
                if(!string.IsNullOrEmpty(rmi.SinglePartReplaced.PartReplaced) || !string.IsNullOrEmpty(rmi.SinglePartReplaced.RefDesignator))
                {
                    dgMultipleParts.Items.Add(rmi.SinglePartReplaced);
                }
            }

            txtTechAct1.Text = csCrossClassInteraction.dbValSubmit(ScannedUnitInformation.TechAction1);
            txtTechAct2.Text = csCrossClassInteraction.dbValSubmit(ScannedUnitInformation.TechAction2);
            txtTechAct3.Text = csCrossClassInteraction.dbValSubmit(ScannedUnitInformation.TechAction3);

            if (ScannedUnitInformation.CustomerInformation != null)
            {
                txtCustomerNumber.Text = csCrossClassInteraction.dbValSubmit(ScannedUnitInformation.CustomerInformation.CustomerNumber);
                txtCustomerName.Text = csCrossClassInteraction.dbValSubmit(ScannedUnitInformation.CustomerInformation.CustomerName);
            }

            if(ScannedUnitInformation.QCandDQEComments != null && !string.IsNullOrEmpty(ScannedUnitInformation.QCandDQEComments.FullTechComments))
                rtbQCDQEComments.AppendText(Environment.NewLine + ScannedUnitInformation.QCandDQEComments.FullTechComments);
        }

        private void QueryTechReport()
        {
            //NOTE: Old DB
            string query = "SELECT Date_Time, Technician FROM tblManufacturingTechReport WHERE SerialNumber = '" + txtSN.Text.TrimEnd() + "';";
            csCrossClassInteraction.dgTechReport(query, true, dgPrevRepairInfo, txtSN.Text.TrimEnd());

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
                conn.Open();
                cmd.Parameters.AddWithValue("@quality", _submitData.QualityTech);
                cmd.Parameters.AddWithValue("@submissionStatus", _submitData.SubmissionStatus);
                cmd.Parameters.AddWithValue("@QCDQEComments", _submitData.QCDQEComments);
                cmd.Parameters.AddWithValue("@QCDQEDateSubmit", _submitData.QCDQEDateSubmit);
                cmd.ExecuteNonQuery();
                conn.Close();

                MessageBox.Show("The QC/DQE submission was successful.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                resetForm(true);
            }
            catch(Exception ex)
            {
                if (conn != null)
                    conn.Close();

                MessageBox.Show("There was an issue submitting the data.\nError Message: " + ex.Message, "Submission Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            submitButtonClicks("COMPLETE");
        }

        private void btnSendToQC_Click(object sender, RoutedEventArgs e)
        {
            submitButtonClicks("SENDTOQC");
        }

        private void btnSendToDQE_Click(object sender, RoutedEventArgs e)
        {
            submitButtonClicks("SENDTODQE");
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
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
        {

        }

        private void rtbGotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void rtbLostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void txtGotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void txtLostFocus(object sender, RoutedEventArgs e)
        {

        }
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
