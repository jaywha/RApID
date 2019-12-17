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
using System.Windows.Media.Animation;

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
        public string FullName = "";

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

            // attempted to fix visual studio exception on stopping debugging
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(
                delegate (object sender, UnhandledExceptionEventArgs args) {
                    Exception e = ((Exception)args.ExceptionObject);
                    var out_msg = $"[UnhandledExceptionHandler]: {e.Message}\n" +
                        $"(Stack Trace)\n{new string('-', 20)}\n\n{e.StackTrace}\n\n{new string('-', 20)}\n" +
                        $"Will runtime terminate now? -> \'{(args.IsTerminating ? "Yes" : "No")}\'";

                    Console.WriteLine(out_msg);
                    #if !DEBUG
                        Mailman.SendEmail(subject: "", body: "", exception: e);
                    #endif

                    if (e is TaskCanceledException tce)
                    {
                        var err_msg = $"[UnhandledException] Task: {tce.Task.Id} | CanBeCanceled = {tce.CancellationToken.CanBeCanceled}\n\tSource -> {tce.Source}";
                        #if !DEBUG
                            csExceptionLogger.csExceptionLogger.Write("Unhandled_Exception", e);
                        #endif
                        
                        Console.WriteLine(err_msg);
                    }

                });
            AppDomain.CurrentDomain.FirstChanceException += new EventHandler<System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs>(
                delegate (object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs args)
                {
                    Exception e = args.Exception;
                    var out_msg = $"[FirstChanceHandler]: {e.Message}\n" +
                        $"(Stack Trace)\n{new string('-', 20)}\n\n{e.StackTrace}\n";

                    Console.WriteLine(out_msg);
                    if (e is TaskCanceledException tce)
                    {
                        var err_msg = $"[FirstChanceException] Task ID: {tce.Task.Id} | CanBeCanceled = {tce.CancellationToken.CanBeCanceled}\n\tSource -> {tce.Source}";
                        #if !DEBUG
                            csExceptionLogger.csExceptionLogger.Write("FirstChance_Exception", e);
                        #endif
                        Console.WriteLine(err_msg);
                    }
                });
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            sVars.initStaticVars();

            var s = ((decimal)sVars.VersionControl.CurrentVersion).ToString();
            Title = "RApID: v." + s;

            await WelcomeUser();
        }

        protected internal async Task WelcomeUser()
        {
            string msgTitle = "";
            string msgWelcome = $"Important updates related to RApID will show up here.";
            if (DateTime.Now.Hour >= 4 && DateTime.Now.Hour < 12)
            {
                msgTitle = "Good Morning";
            }
            else if (DateTime.Now.ToLocalTime().Hour >= 12 && DateTime.Now.ToLocalTime().Hour < 17)
            {
                msgTitle = "Good Afternoon";
            }
            else // DateTime.Now is between 17 and 4
            {
                msgTitle = "Good Evening";
            }

            await Task.Factory.StartNew(new Action(() => {
                Notify.ShowBalloonTip(msgTitle, msgWelcome, BalloonIcon.Info);
                /*var uName = UserPrincipal.Current.DisplayName.Trim().Split(',');
                FullName = uName[1].Trim() + " " + uName[0].Trim();
                Notify.ShowBalloonTip(msgTitle + $", {FullName}!", msgWelcome, BalloonIcon.Info);*/
            }));
        }

        private void btnClicks(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (((Control)sender).Name.ToString())
                {
                    case nameof(btnRework):
                        frmProduction fpr = new frmProduction { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner };                        
                        fpr.Show();
                        Hide();
                        break;
                    case nameof(btnRepair):
                        frmRepair rpr = new frmRepair(false) { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner };
                        rpr.Show();
                        Hide();
                        break;
                    case nameof(btnReportViewer):
                        Process.Start(Properties.Settings.Default.DefaultReportManagerLink);
                        break;
                    case nameof(btnQCDQE):
                        frmQCDQE fQC = new frmQCDQE { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner };
                        fQC.Show();
                        Hide();
                        break;
                    case nameof(btnSettings):
                        frmSettings fSettings = new frmSettings { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner  };
                        fSettings.Show();
                        Hide();
                        break;
                    case nameof(btnTicketLookup):
                        frmGlobalSearch.Instance.Owner = GlobalInstance;
                        frmGlobalSearch.Instance.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        frmGlobalSearch.Instance.CenterWindow();
                        frmGlobalSearch.Instance.Show();
                        Hide();
                        break;
                    case nameof(btnTechFilesForm):
                        frmBoardFileManager alias = new frmBoardFileManager(directDialog: true) { StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen };
                        alias.Show();
                        break;
                    case nameof(btnFirebaseWindow):
                        wndFireabase fireabase = new wndFireabase();
                        fireabase.Show();
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
                if (this != null)
                {
                    Dispatcher.InvokeShutdown();
                }

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
            }
            catch (TaskCanceledException tce)
            {
                Console.WriteLine($"[wndMain_Closed] Task ID: {tce.Task.Id} | CanBeCanceled = {tce.CancellationToken.CanBeCanceled}\n\tSource -> {tce.Source}");
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

        private void BtnShake_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation anime 
                = new DoubleAnimation(wndMain.Left, wndMain.Left / 0.9708737864, new Duration(new TimeSpan(0, 0, 0, 0, 50)))
                {
                    AutoReverse = true,
                    RepeatBehavior = new RepeatBehavior(3.0),
                    FillBehavior = FillBehavior.Stop
                };
            BeginAnimation(LeftProperty, anime);
        }
    }
}
