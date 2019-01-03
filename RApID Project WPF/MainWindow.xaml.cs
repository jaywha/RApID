using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using System.DirectoryServices.AccountManagement;
using System.Threading.Tasks;
using System.Security.Permissions;
using System.Windows.Controls.Primitives;
using System.Collections.Generic;

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
        public static MainWindow GlobalInstance;

        public List<string> Suggestions = new List<string>() { "Test", "Another Test" };

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        public MainWindow()
        {
            holder.vGetServerName("");
            csExceptionLogger.csExceptionLogger.DefaultLogLocation
                = $@"\\joi\EU\Public\_Error Logs\{AppDomain.CurrentDomain.FriendlyName.Replace(".exe", "")}\{DateTime.Now:yyyy-MM-dd}";
            if (!Directory.Exists(csExceptionLogger.csExceptionLogger.DefaultLogLocation))
                Directory.CreateDirectory(csExceptionLogger.csExceptionLogger.DefaultLogLocation);

            InitializeComponent();

#if DEBUG
            lblDebug.Visibility = Visibility.Visible;
            btnTest.Visibility = Visibility.Visible;
#endif

            Notify = notifyRapid;
            GlobalInstance = this;

            // fixes visual studio exception on stopping debugging
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(
                delegate (object sender, UnhandledExceptionEventArgs args) {
                    var e = ((Exception)args.ExceptionObject);
                    Console.WriteLine(
                        $"[UEHandler]: {e.Message}\n" +
                        $"(Stack Trace)\n{new string('-', 20)}\n\n{e.StackTrace}\n\n{new string('-', 20)}\n" +
                        $"Will runtime terminate now? -> \'{(args.IsTerminating ? "Yes" : "No")}\'"
                    ); if (e is TaskCanceledException) return;
#if !DEBUG
                        csExceptionLogger.csExceptionLogger.Write("Unhandled_Exception", e);
#endif
                });
            AppDomain.CurrentDomain.FirstChanceException += new EventHandler<System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs>(
                delegate (object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs args)
                {
                    var e = args.Exception;
                    Console.WriteLine(
                        $"[UEHandler]: {e.Message}\n" +
                        $"(Stack Trace)\n{new string('-', 20)}\n\n{e.StackTrace}\n"
                    ); if (e is TaskCanceledException) return;
#if !DEBUG
                        csExceptionLogger.csExceptionLogger.Write("Unhandled_Exception", e);
#endif
                });
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

            if (DateTime.Now.Hour >= 4 && DateTime.Now.Hour < 12)
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
                        var fpr = new frmProduction { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner };                        
                        fpr.Show();
                        Hide();
                        break;
                    case "btnRepair":
                        var rpr = new frmRepair(false) { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner };
                        rpr.Show();
                        Hide();
                        break;
                    case "btnReportViewer":
                        Process.Start(Properties.Settings.Default.DefaultReportManagerLink);
                        break;
                    case "btnQCDQE":
                        var fQC = new frmQCDQE { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner };
                        fQC.Show();
                        Hide();
                        break;
                    case "btnSettings":
                        var fSettings = new frmSettings { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner  };
                        fSettings.Show();
                        Hide();
                        break;
                    case "btnTicketLookup":
                        frmGlobalSearch.Instance.Owner = GlobalInstance;
                        frmGlobalSearch.Instance.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        frmGlobalSearch.Instance.CenterWindow();
                        frmGlobalSearch.Instance.Show();
                        Hide();
                        break;
                }
            }
            catch (Exception ex)
            {
                csExceptionLogger.csExceptionLogger.Write("MainWindow_btnClicks", ex);
                Show();
            }
        }

        protected internal void MakeFocus()
        {
            WindowState = WindowState.Normal;
            Show();
            BringIntoView();
            Activate();
            Focus();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                //csSplashScreenHelper.StopTokenSource.Cancel();
                Notify = null;
                notifyRapid.Dispose(); // Ensure GC collects notify icon
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
                Notify = null;

                //ServiceManager.StopService("RApID Service");
            }
            catch (TaskCanceledException tce)
            {
                Console.WriteLine($"Task ID: {tce.Task.Id} | CanBeCanceled = {tce.CancellationToken.CanBeCanceled}\n\tSource -> {tce.Source}");
            }
            finally
            {
                Environment.Exit(0);
            }
        }

        private void btnShow_Click(object sender, RoutedEventArgs e) {
            MakeFocus();
            btnShake.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }
        private void btnExit_Click(object sender, RoutedEventArgs e) => Close();
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            //-TODO: Use service to start a new instance.
        }

        private void Button_Click(object sender, RoutedEventArgs e) => new TestWindow { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner }.Show();
    }
}
