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
                }
            }
            catch (Exception ex)
            {
                csExceptionLogger.csExceptionLogger.Write("MainWindow_btnClicks", ex);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                // --->
                // for some reason, the program will not close while in visual studio. Trying to figure out whats happening...
                // ANSWER: Threads aren't being closed correctly

                //EricStabileLibrary.InitSplash.thread_Splash = null;


                //MessageBox.Show(csSplashScreenHelper.thread_Hide.Name + " = " + csSplashScreenHelper.thread_Hide.ThreadState.ToString() + "\n" +
                //    csSplashScreenHelper.thread_Show.Name + " = " + csSplashScreenHelper.thread_Show.ThreadState.ToString() + "\n" +
                //    csSplashScreenHelper.thread_Close.Name + " = " + csSplashScreenHelper.thread_Close.ThreadState.ToString() + "\n" +
                //    EricStabileLibrary.InitSplash.thread_Splash.ThreadState.ToString() +
                //    System.Diagnostics.Process.GetCurrentProcess().Threads.Count);

                //EricStabileLibrary.InitSplash.thread_Splash = null;

                // <---  
            }
            catch (Exception ex)
            {
                csExceptionLogger.csExceptionLogger.Write("MainWindow_Window__Closing", ex);
            }
        }
    }
}
