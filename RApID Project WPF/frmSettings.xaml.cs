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
using RApID_Project_WPF.Classes;

namespace RApID_Project_WPF
{
    /// <summary>
    /// Interaction logic for frmSettings.xaml
    /// </summary>
    public partial class frmSettings : Window
    {
        private SerialPort BarcodeScanner { get; set; }
        csPrintQCDQELabel printLabel;
        StaticVars sVars = StaticVars.StaticVarsInstance();
        csObjectHolder.csObjectHolder holder = csObjectHolder.csObjectHolder.ObjectHolderInstance();

        /// <summary> Default ctor </summary>
        public frmSettings()
        {
            InitializeComponent();
            if (!RDM.ReadFromReg<bool>(RDM.DefaultKey, RDM.IsInited)) RDM.InitReg();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            buildDBSettings();
            buildSerialPortSettings();
            buildLogSettings();
            buildPrinterSettings();
            Activate();
        }

        #region Database Settings
        private void buildDBSettings()
        {
            txtHBDBNew.Visibility = txtRepairDBNew.Visibility = txtAOIDBNew.Visibility = btnHBDBDeleteConnString.Visibility = btnRepairDBDeleteConnString.Visibility = btnAOIDBDeleteConnString.Visibility = Visibility.Hidden;
            cbHBDB.Visibility = cbRepairDB.Visibility = cbAOIDB.Visibility = Visibility.Visible;

            updateCBConnection(holder.HummingBirdConnectionString, holder.HummingBirdConnectionString, cbHBDB, btnHBDBDeleteConnString);
            updateCBConnection(holder.RepairConnectionString, holder.RepairConnectionString, cbRepairDB, btnRepairDBDeleteConnString);
            updateCBConnection(holder.YesDBConnectionString, holder.YesDBConnectionString, cbAOIDB, btnAOIDBDeleteConnString);

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
            if (!string.IsNullOrEmpty(cb.Text))
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
                    if (!string.IsNullOrEmpty(cbHBDB.Text))
                        btnHBDBDeleteConnString.Visibility = Visibility.Visible;
                }
            }
            else if (((Control)sender).Name.ToString().Equals("btnHBDBDeleteConnString"))
            {
                if (cbHBDB.Visibility == Visibility.Visible && cbHBDB.SelectedItem != null)
                {
                    holder.HummingBirdConnectionString = csAppSettings.RemoveConnectionString(holder.HummingBirdConnectionString, cbHBDB.Text);
                    Properties.Settings.Default.Save();
                    updateCBConnection(holder.HummingBirdConnectionString, holder.HummingBirdConnectionString, cbHBDB, btnHBDBDeleteConnString);
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
                    if (!string.IsNullOrEmpty(cbRepairDB.Text))
                        btnRepairDBDeleteConnString.Visibility = Visibility.Visible;
                }
            }
            else if (((Control)sender).Name.ToString().Equals("btnRepairDBDeleteConnString"))
            {
                if (cbRepairDB.Visibility == Visibility.Visible && cbRepairDB.SelectedItem != null)
                {
                    holder.RepairConnectionString = csAppSettings.RemoveConnectionString(holder.RepairConnectionString, cbRepairDB.Text);
                    Properties.Settings.Default.Save();
                    updateCBConnection(holder.RepairConnectionString, holder.RepairConnectionString, cbRepairDB, btnRepairDBDeleteConnString);
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
                    if (!string.IsNullOrEmpty(cbAOIDB.Text))
                        btnAOIDBDeleteConnString.Visibility = Visibility.Visible;
                }
            }
            else if (((Control)sender).Name.ToString().Equals("btnRapidDBDeleteConnString"))
            {
                if (cbAOIDB.Visibility == Visibility.Visible && cbAOIDB.SelectedItem != null)
                {
                    holder.YesDBConnectionString = csAppSettings.RemoveConnectionString(holder.YesDBConnectionString, cbAOIDB.Text);
                    Properties.Settings.Default.Save();
                    updateCBConnection(holder.YesDBConnectionString, holder.YesDBConnectionString, cbAOIDB, btnAOIDBDeleteConnString);
                }
            }
        }

        private void btnSaveDatabaseSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtHBDBNew.Visibility == Visibility.Visible && !string.IsNullOrEmpty(txtHBDBNew.Text))
                {
                    holder.HummingBirdConnectionString = csAppSettings.AddNewConnectionString(holder.HummingBirdConnectionString, txtHBDBNew.Text);
                    holder.HummingBirdConnectionString = txtHBDBNew.Text;
                }
                else if (cbHBDB.Visibility == Visibility.Visible && !string.IsNullOrEmpty(cbHBDB.Text))
                {
                    if (holder.HummingBirdConnectionString != cbHBDB.Text)
                        holder.HummingBirdConnectionString = cbHBDB.Text;
                }

                if (txtRepairDBNew.Visibility == Visibility.Visible && !string.IsNullOrEmpty(txtRepairDBNew.Text))
                {
                    holder.RepairConnectionString = csAppSettings.AddNewConnectionString(holder.RepairConnectionString, txtRepairDBNew.Text);
                    holder.RepairConnectionString = txtRepairDBNew.Text;
                }
                else if (cbRepairDB.Visibility == Visibility.Visible && !string.IsNullOrEmpty(cbRepairDB.Text))
                {
                    if (holder.RepairConnectionString != cbRepairDB.Text)
                        holder.RepairConnectionString = cbRepairDB.Text;
                }

                if (txtAOIDBNew.Visibility == Visibility.Visible && !string.IsNullOrEmpty(txtAOIDBNew.Text))
                {
                    holder.YesDBConnectionString = csAppSettings.AddNewConnectionString(holder.YesDBConnectionString, txtAOIDBNew.Text);
                    holder.YesDBConnectionString = txtAOIDBNew.Text;
                }
                else if (cbAOIDB.Visibility == Visibility.Visible && !string.IsNullOrEmpty(cbAOIDB.Text))
                {
                    if (holder.YesDBConnectionString != cbAOIDB.Text)
                        holder.YesDBConnectionString = cbAOIDB.Text;
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
            if (!string.IsNullOrEmpty(cbHBDB.Text))
                btnHBDBDeleteConnString.Visibility = Visibility.Visible;
            else btnHBDBDeleteConnString.Visibility = Visibility.Hidden;
        }

        private void cbRepairDB_DropDownClosed(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(cbRepairDB.Text))
                btnRepairDBDeleteConnString.Visibility = Visibility.Visible;
            else btnRepairDBDeleteConnString.Visibility = Visibility.Hidden;
        }

        private void cbRapidDB_DropDownClosed(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(cbAOIDB.Text))
                btnAOIDBDeleteConnString.Visibility = Visibility.Visible;
            else btnAOIDBDeleteConnString.Visibility = Visibility.Hidden;
        }
        #endregion

        #region Serial Port Settings
        private void buildSerialPortSettings()
        {
            void InitRegAndOptions(ref ComboBox cmbx , string[] options, string regKey) {
                string targetValue = RDM.ReadFromReg<string>(RDM.DefaultKey, regKey);

                foreach (string s in options) cmbx.Items.Add(s);
                cmbx.Text = targetValue;
            }

            #region Ports
            InitRegAndOptions(ref cbPort, csSerialPort.GetPortNames(), RDM.COMPort);
            #endregion

            #region Baud Rate
            InitRegAndOptions(ref cbBaudRate, csSerialPort.GetBaudRates(), RDM.BaudRate);
            #endregion

            #region Parity
            for (int i = 0; i < csSerialPort.GetParityList().Count; i++)
                cbParity.Items.Add(csSerialPort.GetParityList()[i]);
            cbParity.SelectedItem = Enum.Parse(typeof(Parity), RDM.ReadFromReg<string>(RDM.DefaultKey, RDM.Parity));
            #endregion

            #region Data Bits
            foreach (int i in csSerialPort.GetDataBits())
                cbDataBits.Items.Add(i.ToString());
            cbDataBits.SelectedItem = RDM.ReadFromReg<string>(RDM.DefaultKey, RDM.DataBits);
            #endregion

            #region Stop Bits
            for (int i = 0; i < csSerialPort.GetStopBits().Count; i++)
                cbStopBits.Items.Add(csSerialPort.GetStopBits()[i]);
            cbStopBits.SelectedItem = Enum.Parse(typeof(StopBits), RDM.ReadFromReg<string>(RDM.DefaultKey, RDM.StopBits));
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
                if(BarcodeScanner != null)
                {
                    if (BarcodeScanner.IsOpen)
                        BarcodeScanner.Close();
                    BarcodeScanner = null;
                }

                RDM.CurrentCOMIdentifier = "Default";

                BarcodeScanner = new SerialPort(cbPort.SelectedItem.ToString(), Convert.ToInt32(cbBaudRate.SelectedItem.ToString()), (Parity)cbParity.SelectedItem, Convert.ToInt16(cbDataBits.SelectedItem), (StopBits)cbStopBits.SelectedItem);
                BarcodeScanner.DataReceived += new SerialDataReceivedEventHandler(spDataReceived);

                if (BarcodeScanner != null)
                    lblPortStatus.Content = "Port Status: Created";
                else lblPortStatus.Content = "Port Status: Not Created";
            }
            catch { }
        }

        private void btnOpenPort_Click(object sender, RoutedEventArgs e)
        {
            if (BarcodeScanner == null)
            {
                lblPortStatus.Content = "Port Status: Not Created";
                return;
            }

            if (BarcodeScanner.IsOpen)
            {
                BarcodeScanner.Close();
                if (!BarcodeScanner.IsOpen)
                {
                    lblPortStatus.Content = "Port Status: Closed";
                    btnOpenPort.Content = "Open Port";
                }
            }
            else if (!BarcodeScanner.IsOpen)
            {
                BarcodeScanner.Open();
                if (BarcodeScanner.IsOpen)
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
                RDM.WriteToReg<string>(RDM.DefaultKey, cbPort.Text, RDM.COMPort);
                RDM.WriteToReg<string>(RDM.DefaultKey, cbBaudRate.Text, RDM.BaudRate);
                RDM.WriteToReg<string>(RDM.DefaultKey, cbParity.SelectedItem.ToString(), RDM.Parity);
                RDM.WriteToReg<string>(RDM.DefaultKey, cbDataBits.Text, RDM.DataBits);
                RDM.WriteToReg<string>(RDM.DefaultKey, cbStopBits.SelectedItem.ToString(), RDM.StopBits);
                MessageBox.Show("Settings saved successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Settings did not save. \n\nError Message: " + ex.Message);
            }
        }

        private void spDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = string.Empty;
                while (true)
                {
                    if (!BarcodeScanner.IsOpen)
                    {
                        if (BarcodeScanner != null)
                        {
                            BarcodeScanner.DiscardInBuffer();
                            BarcodeScanner.DiscardOutBuffer();
                            lblPortStatus.Content = "Port Status: Closed";
                        }
                        else if (BarcodeScanner == null)
                        {
                            lblPortStatus.Content = "Port Status: Not Created";
                        }
                    }

                    if (BarcodeScanner != null)
                        data += BarcodeScanner.ReadExisting();

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
                                data = string.Empty;
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
                        data = string.Empty;
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

        private void btnUpdateLogLocation_Click(object sender, RoutedEventArgs e)
        {
            var fbd = new WinForm.FolderBrowserDialog();
            if (fbd.ShowDialog() == WinForm.DialogResult.OK)
            {
                if (!string.IsNullOrEmpty(fbd.SelectedPath))
                {
                    txtLogWriteLoc.Text = fbd.SelectedPath.ToString() + @"\";
                }
            }
        }

        private void btnSaveLogSettings_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.LogWriteLocation = txtLogWriteLoc.Text;
            Properties.Settings.Default.Save();
            MessageBox.Show("Log Settings Updated!");
        }

        private void btnViewLogNewWindow_Click(object sender, RoutedEventArgs e)
        {
            var fvfl = new frmViewFullLog();
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

            if (!string.IsNullOrEmpty(Properties.Settings.Default.PrinterToUse))
                cbPrinterList.Text = Properties.Settings.Default.PrinterToUse;

            txtXOffset.Text = Properties.Settings.Default.PrinterXOffset.ToString();
            txtYOffset.Text = Properties.Settings.Default.PrinterYOffset.ToString();
        }

        private void btnSavePrinterSettings_Click(object sender, RoutedEventArgs e)
        {

            if (!string.IsNullOrEmpty(cbPrinterList.Text))
                Properties.Settings.Default.PrinterToUse = cbPrinterList.Text;
            if (!string.IsNullOrEmpty(txtXOffset.Text))
                Properties.Settings.Default.PrinterXOffset = Convert.ToInt32(txtXOffset.Text);
            if (!string.IsNullOrEmpty(txtYOffset.Text))
                Properties.Settings.Default.PrinterYOffset = Convert.ToInt32(txtYOffset.Text);

            if(!Properties.Settings.Default.PrinterInitSetup)
            {
                if(!string.IsNullOrEmpty(Properties.Settings.Default.PrinterToUse))
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
            var fpqcedqel = new frmPrintQCDQELabel();
            fpqcedqel.ShowDialog();
        }
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (BarcodeScanner != null && BarcodeScanner.IsOpen)
            {
                BarcodeScanner.Close();
                BarcodeScanner = null;
            }

            MainWindow.GlobalInstance.MakeFocus();
        }
    }
}
