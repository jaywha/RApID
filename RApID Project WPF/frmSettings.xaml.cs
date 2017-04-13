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
using Microsoft.Win32;
using WinForm = System.Windows.Forms;

namespace RApID_Project_WPF
{
    /// <summary>
    /// Interaction logic for frmSettings.xaml
    /// </summary>
    public partial class frmSettings : Window
    {
        SerialPort sp;
        csPrintQCDQELabel printLabel;
        StaticVars sVars = StaticVars.StaticVarsInstance();

        public frmSettings()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            buildDBSettings();
            buildSerialPortSettings();
            buildLogSettings();
            buildPrinterSettings();
            this.Activate();
        }

        #region Database Settings
        private void buildDBSettings()
        {
            txtHBDBNew.Visibility = txtRepairDBNew.Visibility = txtAOIDBNew.Visibility = btnHBDBDeleteConnString.Visibility = btnRepairDBDeleteConnString.Visibility = btnAOIDBDeleteConnString.Visibility = Visibility.Hidden;
            cbHBDB.Visibility = cbRepairDB.Visibility = cbAOIDB.Visibility = Visibility.Visible;

            updateCBConnection(Properties.Settings.Default.HBConnStrings, Properties.Settings.Default.HBConn, cbHBDB, btnHBDBDeleteConnString);
            updateCBConnection(Properties.Settings.Default.RepairConnStrings, Properties.Settings.Default.RepairConn, cbRepairDB, btnRepairDBDeleteConnString);
            updateCBConnection(Properties.Settings.Default.YesDBConnStrings, Properties.Settings.Default.YesDBConn, cbAOIDB, btnAOIDBDeleteConnString);

            if (Properties.Settings.Default.AccessDatabase)
                cbUseAccess.IsChecked = true;
            else cbUseAccess.IsChecked = false;

#if !DEBUG
            cbUseAccess.IsChecked = false;
            cbUseAccess.Visibility = System.Windows.Visibility.Hidden;
#endif
        }
        private void dbBTNHelper(Control cVisible, Control cHide, Button bChangeText, string sButtonText)
        {
            cVisible.Visibility = Visibility.Visible;
            cHide.Visibility = Visibility.Hidden;
            bChangeText.Content = sButtonText;
        }

        /// <summary>
        /// This will run after a connection string has been deleted.
        /// </summary>
        private void updateCBConnection(string connStringList, string connString, ComboBox cb, Button btn)
        {
            cb.Items.Clear();

            foreach (string s in csAppSettings.GetConnectionStrings(connStringList))
                cb.Items.Add(s);
            cb.Text = connString;
            if (!String.IsNullOrEmpty(cb.Text))
                btn.Visibility = Visibility.Visible;
        }

        private void dbBTNClick(object sender, RoutedEventArgs e)
        {
            if (((Control)sender).Name.ToString().Equals("btnHBDBNew"))
            {
                if (btnHBDBNew.Content.Equals("Add New Connection String"))
                {
                    dbBTNHelper(txtHBDBNew, cbHBDB, btnHBDBNew, "Cancel");
                    if (btnHBDBDeleteConnString.Visibility == Visibility.Visible)
                        btnHBDBDeleteConnString.Visibility = Visibility.Hidden;
                }
                else if (btnHBDBNew.Content.Equals("Cancel"))
                {
                    dbBTNHelper(cbHBDB, txtHBDBNew, btnHBDBNew, "Add New Connection String");
                    if (!String.IsNullOrEmpty(cbHBDB.Text))
                        btnHBDBDeleteConnString.Visibility = Visibility.Visible;
                }
            }
            else if (((Control)sender).Name.ToString().Equals("btnHBDBDeleteConnString"))
            {
                if (cbHBDB.Visibility == Visibility.Visible && cbHBDB.SelectedItem != null)
                {
                    Properties.Settings.Default.HBConnStrings = csAppSettings.RemoveConnectionString(Properties.Settings.Default.HBConnStrings, cbHBDB.Text);
                    Properties.Settings.Default.Save();
                    updateCBConnection(Properties.Settings.Default.HBConnStrings, Properties.Settings.Default.HBConn, cbHBDB, btnHBDBDeleteConnString);
                }
            }
            else if (((Control)sender).Name.ToString().Equals("btnRepairDBNew"))
            {
                if (btnRepairDBNew.Content.Equals("Add New Connection String"))
                {
                    dbBTNHelper(txtRepairDBNew, cbRepairDB, btnRepairDBNew, "Cancel");
                    if (btnRepairDBDeleteConnString.Visibility == Visibility.Visible)
                        btnRepairDBDeleteConnString.Visibility = Visibility.Hidden;
                }
                else if (btnRepairDBNew.Content.Equals("Cancel"))
                {
                    dbBTNHelper(cbRepairDB, txtRepairDBNew, btnRepairDBNew, "Add New Connection String");
                    if (!String.IsNullOrEmpty(cbRepairDB.Text))
                        btnRepairDBDeleteConnString.Visibility = Visibility.Visible;
                }
            }
            else if (((Control)sender).Name.ToString().Equals("btnRepairDBDeleteConnString"))
            {
                if (cbRepairDB.Visibility == Visibility.Visible && cbRepairDB.SelectedItem != null)
                {
                    Properties.Settings.Default.RepairConnStrings = csAppSettings.RemoveConnectionString(Properties.Settings.Default.RepairConnStrings, cbRepairDB.Text);
                    Properties.Settings.Default.Save();
                    updateCBConnection(Properties.Settings.Default.RepairConnStrings, Properties.Settings.Default.RepairConn, cbRepairDB, btnRepairDBDeleteConnString);
                }
            }
            else if (((Control)sender).Name.ToString().Equals("btnAOIDBNew"))
            {
                if (btnAOIDBNew.Content.Equals("Add New Connection String"))
                {
                    dbBTNHelper(txtAOIDBNew, cbAOIDB, btnAOIDBNew, "Cancel");
                    if (btnAOIDBDeleteConnString.Visibility == Visibility.Visible)
                        btnAOIDBDeleteConnString.Visibility = Visibility.Hidden;
                }
                else if (btnAOIDBNew.Content.Equals("Cancel"))
                {
                    dbBTNHelper(cbAOIDB, txtAOIDBNew, btnAOIDBNew, "Add New Connection String");
                    if (!String.IsNullOrEmpty(cbAOIDB.Text))
                        btnAOIDBDeleteConnString.Visibility = Visibility.Visible;
                }
            }
            else if (((Control)sender).Name.ToString().Equals("btnRapidDBDeleteConnString"))
            {
                if (cbAOIDB.Visibility == Visibility.Visible && cbAOIDB.SelectedItem != null)
                {
                    Properties.Settings.Default.YesDBConnStrings = csAppSettings.RemoveConnectionString(Properties.Settings.Default.YesDBConnStrings, cbAOIDB.Text);
                    Properties.Settings.Default.Save();
                    updateCBConnection(Properties.Settings.Default.YesDBConnStrings, Properties.Settings.Default.YesDBConn, cbAOIDB, btnAOIDBDeleteConnString);
                }
            }
        }

        private void btnSaveDatabaseSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtHBDBNew.Visibility == Visibility.Visible && !String.IsNullOrEmpty(txtHBDBNew.Text))
                {
                    Properties.Settings.Default.HBConnStrings = csAppSettings.AddNewConnectionString(Properties.Settings.Default.HBConnStrings, txtHBDBNew.Text);
                    Properties.Settings.Default.HBConn = txtHBDBNew.Text;
                }
                else if (cbHBDB.Visibility == Visibility.Visible && !String.IsNullOrEmpty(cbHBDB.Text))
                {
                    if (Properties.Settings.Default.HBConn != cbHBDB.Text)
                        Properties.Settings.Default.HBConn = cbHBDB.Text;
                }

                if (txtRepairDBNew.Visibility == Visibility.Visible && !String.IsNullOrEmpty(txtRepairDBNew.Text))
                {
                    Properties.Settings.Default.RepairConnStrings = csAppSettings.AddNewConnectionString(Properties.Settings.Default.RepairConnStrings, txtRepairDBNew.Text);
                    Properties.Settings.Default.RepairConn = txtRepairDBNew.Text;
                }
                else if (cbRepairDB.Visibility == Visibility.Visible && !String.IsNullOrEmpty(cbRepairDB.Text))
                {
                    if (Properties.Settings.Default.RepairConn != cbRepairDB.Text)
                        Properties.Settings.Default.RepairConn = cbRepairDB.Text;
                }

                if (txtAOIDBNew.Visibility == Visibility.Visible && !String.IsNullOrEmpty(txtAOIDBNew.Text))
                {
                    Properties.Settings.Default.YesDBConnStrings = csAppSettings.AddNewConnectionString(Properties.Settings.Default.YesDBConnStrings, txtAOIDBNew.Text);
                    Properties.Settings.Default.YesDBConn = txtAOIDBNew.Text;
                }
                else if (cbAOIDB.Visibility == Visibility.Visible && !String.IsNullOrEmpty(cbAOIDB.Text))
                {
                    if (Properties.Settings.Default.YesDBConn != cbAOIDB.Text)
                        Properties.Settings.Default.YesDBConn = cbAOIDB.Text;
                }

                Properties.Settings.Default.AccessDatabase = (bool)cbUseAccess.IsChecked;

                Properties.Settings.Default.Save();

                MessageBox.Show("Database settings saved correctly.");

                sVars.initStaticVars();

                buildDBSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an issue saving the new database settings. \n\nError Message: " + ex.Message, "btnSaveDatabaseSettings_Click()");
            }
        }

        private void cbHBDB_DropDownClosed(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(cbHBDB.Text))
                btnHBDBDeleteConnString.Visibility = Visibility.Visible;
            else btnHBDBDeleteConnString.Visibility = Visibility.Hidden;
        }

        private void cbRepairDB_DropDownClosed(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(cbRepairDB.Text))
                btnRepairDBDeleteConnString.Visibility = Visibility.Visible;
            else btnRepairDBDeleteConnString.Visibility = Visibility.Hidden;
        }

        private void cbRapidDB_DropDownClosed(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(cbAOIDB.Text))
                btnAOIDBDeleteConnString.Visibility = Visibility.Visible;
            else btnAOIDBDeleteConnString.Visibility = Visibility.Hidden;
        }
        #endregion

        #region Serial Port Settings
        private void buildSerialPortSettings()
        {
            #region Ports
            foreach (string s in csSerialPort.GetPortNames())
                cbPort.Items.Add(s);
            cbPort.Text = Properties.Settings.Default.SPPortName;
            #endregion

            #region Baud Rate
            foreach (string s in csSerialPort.GetBaudRates())
                cbBaudRate.Items.Add(s);
            cbBaudRate.Text = Properties.Settings.Default.SPBaudRate.ToString();
            #endregion

            #region Parity
            for (int i = 0; i < csSerialPort.GetParityList().Count; i++)
                cbParity.Items.Add(csSerialPort.GetParityList()[i]);
            cbParity.Text = Properties.Settings.Default.SPParity.ToString();
            #endregion

            #region Data Bits
            foreach (int i in csSerialPort.GetDataBits())
                cbDataBits.Items.Add(i.ToString());
            cbDataBits.Text = Properties.Settings.Default.SPDataBit.ToString();
            #endregion

            #region Stop Bits
            for (int i = 0; i < csSerialPort.GetStopBits().Count; i++)
                cbStopBits.Items.Add(csSerialPort.GetStopBits()[i]);
            cbStopBits.Text = Properties.Settings.Default.SPStopBit.ToString();
            #endregion

            serialPortButtonControl();
        }

        private void serialPortButtonControl()
        {
            if (cbPort.SelectedItem == null || cbBaudRate.SelectedItem == null || cbParity.SelectedItem == null || cbDataBits.SelectedItem == null || cbStopBits.SelectedItem == null)
            {
                btnCreatePort.IsEnabled = btnOpenPort.IsEnabled = btnSaveSettings.IsEnabled = false;
            }
            else
            {
                btnCreatePort.IsEnabled = btnOpenPort.IsEnabled = btnSaveSettings.IsEnabled = true;
            }
        }

        private void btnCreatePort_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(sp != null)
                {
                    if (sp.IsOpen)
                        sp.Close();
                    sp = null;
                }

                sp = new SerialPort(cbPort.SelectedItem.ToString(), Convert.ToInt32(cbBaudRate.SelectedItem.ToString()), (Parity)cbParity.SelectedItem, Convert.ToInt16(cbDataBits.SelectedItem), (StopBits)cbStopBits.SelectedItem);
                sp.DataReceived += new SerialDataReceivedEventHandler(spDataReceived);

                if (sp != null)
                    lblPortStatus.Content = "Port Status: Created";
                else lblPortStatus.Content = "Port Status: Not Created";
            }
            catch { }
        }

        private void btnOpenPort_Click(object sender, RoutedEventArgs e)
        {
            if (sp == null)
            {
                lblPortStatus.Content = "Port Status: Not Created";
                return;
            }

            if (sp.IsOpen)
            {
                sp.Close();
                if (!sp.IsOpen)
                {
                    lblPortStatus.Content = "Port Status: Closed";
                    btnOpenPort.Content = "Open Port";
                }
            }
            else if (!sp.IsOpen)
            {
                sp.Open();
                if (sp.IsOpen)
                {
                    lblPortStatus.Content = "Port Status: Opened";
                    btnOpenPort.Content = "Close Port";
                }
            }
        }

        private void btnSavePortSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.SPPortName = cbPort.Text.ToString();
                Properties.Settings.Default.SPBaudRate = Convert.ToInt32(cbBaudRate.Text);
                Properties.Settings.Default.SPParity = (Parity)cbParity.SelectedItem;
                Properties.Settings.Default.SPDataBit = Convert.ToInt16(cbDataBits.Text);
                Properties.Settings.Default.SPStopBit = (StopBits)cbStopBits.SelectedItem;
                Properties.Settings.Default.Save();
                MessageBox.Show("Settings saved successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Settings did not save. \n\nError Message: " + ex.Message);
            }
        }

        public void spDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = String.Empty;
                while (true)
                {
                    if (!sp.IsOpen)
                    {
                        if (sp != null)
                        {
                            sp.DiscardInBuffer();
                            sp.DiscardOutBuffer();
                            lblPortStatus.Content = "Port Status: Closed";
                        }
                        else if (sp == null)
                        {
                            lblPortStatus.Content = "Port Status: Not Created";
                        }
                    }

                    if (sp != null)
                        data += sp.ReadExisting();

                    if (data.EndsWith("\r") || data.EndsWith("\n"))
                        break;
                    else
                    {
                        if (data.Length > 0)
                        {
                            if (rtbPortData.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
                            {
                                rtbPortData.AppendText(data);
                            }
                            else
                            {
                                Dispatcher.Invoke((Action)delegate
                                {
                                    rtbPortData.AppendText(data);
                                });
                                data = String.Empty;
                            }
                        }
                    }
                }

                if (data.Length > 0)
                {
                    if (rtbPortData.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
                    {
                        rtbPortData.AppendText(data);
                    }
                    else
                    {
                        Dispatcher.Invoke((Action)delegate
                        {
                            rtbPortData.AppendText(data);
                        });
                        data = String.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error receiving data from the COM Port.\n\nError Message: " + ex.Message, "spDataReceived()", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void spDropDownClosed(object sender, EventArgs e)
        {
            serialPortButtonControl();
        }
        #endregion

        #region Log Settings
        private void buildLogSettings()
        {
            txtLogWriteLoc.Text = Properties.Settings.Default.LogWriteLocation;
        }

        //private void attemptToReadLog(string sLogLoc)
        //{
        //    rtbLogInfo.Document.Blocks.Clear();
        //    LogToReview = csSerialization.deserializeFile(sLogLoc);

        //    if (LogToReview == null)
        //    {
        //        rtbLogInfo.AppendText("There was an error reading in the selected log.");
        //        btnViewLogNewWindow.Visibility = Visibility.Hidden;
        //    }
        //    else if (LogToReview != null)
        //    {
        //        btnViewLogNewWindow.Visibility = Visibility.Visible;

        //        string sLogData = String.Format("{0} began this entry at {1}.\n", LogToReview.Tech, LogToReview.LogCreationTime.ToString("MM/dd/yyyy hh:mm:ss tt"));

        //        if (LogToReview.IsCR)
        //            sLogData += "This was regarding a Credit Return.\n";
        //        else sLogData += "This was not a Credit Return.\n";

        //        sLogData += "Actions Associated With This Log File:\n";

        //        //Note: Count the number of times each action appears.
        //        for (int i = 0; i < System.Enum.GetNames(typeof(csLogging.LogState)).Length; i++)
        //        {
        //            int iCount = 0;
        //            csLogging.LogState _stateChecker = (csLogging.LogState)i;
        //            for (int j = 0; j < LogToReview.lActions.Count; j++)
        //            {
        //                if (LogToReview.lActions[j].EventType.Equals(_stateChecker))
        //                    iCount++;
        //            }
        //            sLogData += String.Format("Type of Event: {0} | Number of Occurrences: {1}\n", _stateChecker.ToString(), iCount.ToString());
        //        }

        //        rtbLogInfo.AppendText(sLogData);
        //    }
        //}

        private void btnUpdateLogLocation_Click(object sender, RoutedEventArgs e)
        {
            WinForm.FolderBrowserDialog fbd = new WinForm.FolderBrowserDialog();
            if (fbd.ShowDialog() == WinForm.DialogResult.OK)
            {
                if (!String.IsNullOrEmpty(fbd.SelectedPath))
                {
                    txtLogWriteLoc.Text = fbd.SelectedPath.ToString() + @"\";

                    //if (!txtLogWriteLoc.Text.EndsWith(@"\"))
                      //  txtLogWriteLoc.Text = fbd.SelectedPath.ToString() + @"\";

                }
            }
        }

        private void btnSaveLogSettings_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.LogWriteLocation = txtLogWriteLoc.Text;
            Properties.Settings.Default.Save();
            MessageBox.Show("Log Settings Updated!");
        }

        //private void btnReadLog_Click(object sender, RoutedEventArgs e)
        //{
        //    string sLogFileLoc = String.Empty;
        //    OpenFileDialog ofd = new OpenFileDialog();
        //    ofd.Filter = "XML Files (*.xml)|*xml";
        //    if (ofd.ShowDialog() == true)
        //    {
        //        sLogFileLoc = ofd.FileName;
        //        Console.WriteLine(sLogFileLoc);
        //        if (!String.IsNullOrEmpty(sLogFileLoc))
        //        {
        //            attemptToReadLog(sLogFileLoc);
        //        }
        //    }
        //}

        private void btnViewLogNewWindow_Click(object sender, RoutedEventArgs e)
        {
            frmViewFullLog fvfl = new frmViewFullLog();
            fvfl.ShowDialog();
        }
        #endregion

        #region Printer Settings
        private void buildPrinterSettings()
        {
            for(int i = 0; i < System.Drawing.Printing.PrinterSettings.InstalledPrinters.Count; i++)
            {
                cbPrinterList.Items.Add(System.Drawing.Printing.PrinterSettings.InstalledPrinters[i].ToString());
            }

            //cbPrinterList.Items.Add(@"\\eufs04\EU_B3_Repair label printer");

            if (!String.IsNullOrEmpty(Properties.Settings.Default.PrinterToUse))
                cbPrinterList.Text = Properties.Settings.Default.PrinterToUse;

            txtXOffset.Text = Properties.Settings.Default.PrinterXOffset.ToString();
            txtYOffset.Text = Properties.Settings.Default.PrinterYOffset.ToString();
        }

        private void btnSavePrinterSettings_Click(object sender, RoutedEventArgs e)
        {

            if (!String.IsNullOrEmpty(cbPrinterList.Text))
                Properties.Settings.Default.PrinterToUse = cbPrinterList.Text;
            if (!String.IsNullOrEmpty(txtXOffset.Text))
                Properties.Settings.Default.PrinterXOffset = Convert.ToInt32(txtXOffset.Text);
            if (!String.IsNullOrEmpty(txtYOffset.Text))
                Properties.Settings.Default.PrinterYOffset = Convert.ToInt32(txtYOffset.Text);

            if(!Properties.Settings.Default.PrinterInitSetup)
            {
                if(!String.IsNullOrEmpty(Properties.Settings.Default.PrinterToUse))
                {
                    Properties.Settings.Default.PrinterInitSetup = true;
                }
            }

            Properties.Settings.Default.Save();

            MessageBox.Show("Printer Settings Saved Successfully", "Save Successful", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnTestPrintPreview_Click(object sender, RoutedEventArgs e)
        {
            printLabel = new csPrintQCDQELabel("Sent To QC/DQE", System.Environment.UserName, DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"), "23091616030900");
            printLabel.PrintPreview();
        }

        private void btnTestPrint_Click(object sender, RoutedEventArgs e)
        {
            printLabel = new csPrintQCDQELabel("Sent To QC/DQE", System.Environment.UserName, DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"), "23091616030900");
            printLabel.PrintLabel();
        }

        private void btnPrintQCDQELabel_Click(object sender, RoutedEventArgs e)
        {
            frmPrintQCDQELabel fpqcedqel = new frmPrintQCDQELabel();
            fpqcedqel.ShowDialog();
        }
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sp != null && sp.IsOpen)
            {
                sp.Close();
                sp = null;
            }
        }
    }
}
