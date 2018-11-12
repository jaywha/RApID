using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace RApID_Project_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        csObjectHolder.csObjectHolder holder = csObjectHolder.csObjectHolder.ObjectHolderInstance();
        StaticVars sVars = StaticVars.StaticVarsInstance();

        public MainWindow()
        {
            holder.vGetServerName("");
            csExceptionLogger.csExceptionLogger.DefaultLogLocation
                = $@"\\joi\EU\Public\_Error Logs\{AppDomain.CurrentDomain.FriendlyName.Replace(".exe", "")}\{DateTime.Now:yyyy-MM-dd}";
            if (!Directory.Exists(csExceptionLogger.csExceptionLogger.DefaultLogLocation))
                Directory.CreateDirectory(csExceptionLogger.csExceptionLogger.DefaultLogLocation);
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            sVars.initStaticVars();

            var s = ((decimal)sVars.VersionControl.CurrentVersion).ToString();
            Title = "RApID: v." + s;
        }

        private void btnClicks(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (((Control)sender).Name.ToString())
                {
                    case "btnRework":
                        Hide();
                        var fpr = new frmProduction { Owner = this };
                        fpr.ShowDialog();
                        Show();
                        Activate();
                        break;
                    case "btnRepair":
                        Hide();
                        var rpr = new Repair(false) { Owner = this };
                        rpr.ShowDialog();
                        Show();
                        Activate();
                        break;
                    case "btnReportViewer":
                        System.Diagnostics.Process.Start(Properties.Settings.Default.DefaultReportManagerLink);
                        break;
                    case "btnQCDQE":
                        Hide();
                        var fQC = new frmQCDQE { Owner = this };
                        fQC.ShowDialog();
                        Show();
                        Activate();
                        break;
                    case "btnSettings":
                        Hide();
                        var fSettings = new frmSettings { Owner = this };
                        fSettings.ShowDialog();
                        Show();
                        Activate();
                        break;
                    case "btnTicketLookup":
                        Hide();
                        frmGlobalSearch.Instance.Owner = this;
                        frmGlobalSearch.Instance.Show();
                        Show();
                        Activate();
                        break;
                }
            }
            catch (Exception ex)
            {
                csExceptionLogger.csExceptionLogger.Write("MainWindow_btnClicks", ex);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try {
                csSplashScreenHelper.StopTokenSource.Cancel();
            } catch (Exception ex) {
                csExceptionLogger.csExceptionLogger.Write("WindowClosing_ThreadStillRunning", ex);
            }

            notifyRapid = null; // Ensure GC collects notify icon
        }

        private void btnShow_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Normal;
            Show();
            BringIntoView();
            Activate();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e) => Close();
    }
}
