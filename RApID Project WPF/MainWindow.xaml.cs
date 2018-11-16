using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using System.DirectoryServices.AccountManagement;
using System.Threading.Tasks;

namespace RApID_Project_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        csObjectHolder.csObjectHolder holder = csObjectHolder.csObjectHolder.ObjectHolderInstance();
        StaticVars sVars = StaticVars.StaticVarsInstance();

        public static TaskbarIcon Notify;

        public MainWindow()
        {
            holder.vGetServerName("");
            csExceptionLogger.csExceptionLogger.DefaultLogLocation
                = $@"\\joi\EU\Public\_Error Logs\{AppDomain.CurrentDomain.FriendlyName.Replace(".exe", "")}\{DateTime.Now:yyyy-MM-dd}";
            if (!Directory.Exists(csExceptionLogger.csExceptionLogger.DefaultLogLocation))
                Directory.CreateDirectory(csExceptionLogger.csExceptionLogger.DefaultLogLocation);
            InitializeComponent();
            Notify = notifyRapid;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            sVars.initStaticVars();

            var s = ((decimal)sVars.VersionControl.CurrentVersion).ToString();
            Title = "RApID: v." + s;

            await WelcomeUser();

            //await HookService();
        }

        protected internal async Task WelcomeUser()
        {
            string msgTitle = "";
            string msgWelcome = $"Important updates related to RApID will show up here.";

            if(DateTime.Now.Hour >= 4 && DateTime.Now.Hour < 12)
            {
                msgTitle = "Good Morning";
            } else if (DateTime.Now.ToLocalTime().Hour >= 12 && DateTime.Now.ToLocalTime().Hour < 17)
            {
                msgTitle = "Good Afternoon";
            } else // DateTime.Now is between 17 and 4
            {
                msgTitle = "Good Evening";
            }

            await Task.Factory.StartNew(new Action(() => {
                var uName = UserPrincipal.Current.DisplayName.Trim().Split(',');

                Notify.ShowBalloonTip(msgTitle + $", {uName[1].Trim()} {uName[0].Trim()}!", msgWelcome, BalloonIcon.Info);
            }));            
        }

        protected internal async Task HookService()
        {
            await Task.Factory.StartNew(new Action(() => {
                ServiceManager.InstallAndStart("RApID Service", "RApID Reporting Service", "RApID Service.exe");
            }));
        }

        private void btnClicks(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (((Control)sender).Name.ToString())
                {
                    case "btnRework":
                        Hide();
                        var fpr = new frmProduction { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner };
                        fpr.ShowDialog();
                        MakeFocus();
                        break;
                    case "btnRepair":
                        Hide();
                        var rpr = new Repair(false) { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner };
                        rpr.ShowDialog();
                        MakeFocus();
                        break;
                    case "btnReportViewer":
                        Process.Start(Properties.Settings.Default.DefaultReportManagerLink);
                        break;
                    case "btnQCDQE":
                        Hide();
                        var fQC = new frmQCDQE { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner };
                        fQC.ShowDialog();
                        MakeFocus();
                        break;
                    case "btnSettings":
                        Hide();
                        var fSettings = new frmSettings { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner };
                        fSettings.ShowDialog();
                        MakeFocus();
                        break;
                    case "btnTicketLookup":
                        Hide();
                        frmGlobalSearch.Instance.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        frmGlobalSearch.Instance.Show();
                        MakeFocus();
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
            notifyRapid = null; // Ensure GC collects notify icon
            try
            {
                csSplashScreenHelper.thread_Show?.Abort();
                csSplashScreenHelper.thread_Hide?.Abort();
                csSplashScreenHelper.thread_Close?.Abort();
            }
            catch (Exception ex)
            when (ex is System.Security.SecurityException || ex is System.Threading.ThreadStateException
                || ex is System.Threading.ThreadAbortException)
            {
                csExceptionLogger.csExceptionLogger.Write("MainWindow_ThreadedClosingIssues", ex);
            }
        }



        private void wndMain_Closed(object sender, EventArgs e)
        {
            try
            {
                /*Wide net to catch rouge threads...*/
                EricStabileLibrary.InitSplash.thread_Splash?.Abort();
                Notify = null;
                notifyRapid = null;

                //ServiceManager.StopService("RApID Service");
            }
            catch (System.Threading.Tasks.TaskCanceledException tce)
            {
                Console.WriteLine($"Task ID: {tce.Task.Id} | CanBeCanceled = {tce.CancellationToken.CanBeCanceled}\n\tSource -> {tce.Source}");
            }
        }

        private void btnShow_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Normal;
            MakeFocus();
        }

        private void MakeFocus()
        {
            Show();
            BringIntoView();
            Activate();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e) => Close();

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Use service to start a new instance.
        }
    }
}
