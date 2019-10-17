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
using System.IO;
using System.Globalization;

namespace RApID_Project_WPF
{
    class LogInfo
    {
        private string _logDispName;
        public string LogDisplayName
        {
            get { return _logDispName; }
        }

        private string _logFileName;
        public string LogFileName
        {
            get { return _logFileName; }
        }

        private string _logFilePath;
        public string LogFilePath
        {
            get { return _logFilePath; }
        }

        private string _techName;

        public LogInfo(string sTechName, string sFileName)
        {
            _techName = sTechName;
            _logFileName = sFileName;
            SetDisplayName();
            SetFilePath();
        }

        private void SetDisplayName()
        {
            string[] splitters = { "_", "." };
            string[] sSplit = _logFileName.Split(splitters, StringSplitOptions.RemoveEmptyEntries);

            //-Convert the timestamp of the file into a format we can understand.
            string sFormat = "MMddyyhhmmsstt";
            DateTime res = DateTime.ParseExact(sSplit[1], sFormat, CultureInfo.InvariantCulture);

            _logDispName = $"{sSplit[0]}: {res.ToString("MM/dd/yyyy hh:mm:ss tt")}";
        }

        private void SetFilePath()
        {
            _logFilePath = $@"{Properties.Settings.Default.LogWriteLocation}\{_techName}\{_logFileName}";
        }
    }

    /// <summary>
    /// Interaction logic for frmViewFullLog.xaml
    /// </summary>
    public partial class frmViewFullLog : Window
    {
        csLog lLogToView;
        List<LogInfo> lLogList = new List<LogInfo>();

        public frmViewFullLog()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < System.Enum.GetNames(typeof(csLogging.LogState)).Length; i++)
                cbFilters.Items.Add((csLogging.LogState)i);

            filterStatus(false);
            loadForm();
        }

        private void loadForm()
        {
            lLogList.Clear();
            lbTechList.Items.Clear();
            lbLogsToView.Items.Clear();
            lbFilterList.Items.Clear();
            rtbLog.Document.Blocks.Clear();
            cbFilters.SelectedIndex = -1;

            try
            {
                string[] Dir = Directory.GetDirectories(Properties.Settings.Default.LogWriteLocation);
                string[] splitters = { @"\" };
                for (int i = 0; i < Dir.Length; i++)
                {
                    string[] sDirSplit = Dir[i].Split(splitters, StringSplitOptions.RemoveEmptyEntries);
                    lbTechList.Items.Add(sDirSplit[sDirSplit.Length - 1]);
                }
            }
            catch { }
        }

        /// <summary>
        /// Allows to easily enable/disable filters controls.
        /// </summary>
        /// <param name="bIsEnabled">Should the control be enabled?</param>
        private void filterStatus(bool bIsEnabled)
        {
            cbFilters.SelectedIndex = -1;
            cbFilters.IsEnabled = btnAddFilter.IsEnabled = btnClearFilters.IsEnabled = bIsEnabled;
            lbFilterList.Items.Clear();
        }

        private async void lbTechList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            await Task.Factory.StartNew(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (lbTechList.SelectedItem != null)
                    {
                        lLogList.Clear();
                        lbLogsToView.Items.Clear();
                        rtbLog.Document.Blocks.Clear();

                        string[] Files = Directory.GetFiles(Properties.Settings.Default.LogWriteLocation + @"\" + lbTechList.SelectedItem.ToString());
                        string[] splitters = { @"\" };
                        string[] ignoreYears = new string[] { "2016", "2017" };
                        for (int i = 0; i < Files.Length; i++)
                        {
                            string[] sFileSplit = Files[i].Split(splitters, StringSplitOptions.RemoveEmptyEntries);
                            LogInfo li = new LogInfo(lbTechList.SelectedItem.ToString(), sFileSplit[sFileSplit.Length - 1]);
                            if (li.LogDisplayName.Contains("2016") || li.LogDisplayName.Contains("2017")) continue;
                            lbLogsToView.Items.Add(li.LogDisplayName);
                            lLogList.Add(li);
                        }

                        filterStatus(false);
                    }
                });
            });
        }

        private void lbLogsToView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            rtbLog.Document.Blocks.Clear();

            if (lbLogsToView.SelectedItem != null)
            {
                try
                {
                    LogInfo li = lLogList.Find(x => x.LogDisplayName == lbLogsToView.SelectedItem.ToString());
                    lLogToView = csSerialization.deserializeFile(li.LogFilePath);
                    csDisplayLog.DisplayLog(rtbLog, csLogging.LogState.NONE, lLogToView, true);
                    filterStatus(true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to load the selected log.\nError Message: " + ex.Message, "Log Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnAddFilter_Click(object sender, RoutedEventArgs e)
        {
            if (cbFilters.SelectedItem != null && !lbFilterList.Items.Contains(cbFilters.SelectedItem))
            {
                lbFilterList.Items.Add(cbFilters.SelectedItem);

                rtbLog.Document.Blocks.Clear();

                for (int i = 0; i < lbFilterList.Items.Count; i++)
                {
                    csDisplayLog.DisplayLog(rtbLog, (csLogging.LogState)lbFilterList.Items[i], lLogToView, false);
                }
            }
        }

        private void btnClearFilters_Click(object sender, RoutedEventArgs e)
        {
            lbFilterList.Items.Clear();
            csDisplayLog.DisplayLog(rtbLog, csLogging.LogState.NONE, lLogToView, true);
            cbFilters.SelectedIndex = -1;
        }
    }
}
