//using MonkeyCache.FileStore;
using System;
using System.Windows;

namespace RApID_Project_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                // Barrel.ApplicationId = AppDomain.CurrentDomain.FriendlyName;
                // GetNetLib.VersionTest.InstallIfLowerThan45();
            } catch (Exception e) {
                MessageBox.Show("The cache library is causing an issue.", "DEBUGGING MESSAGE", MessageBoxButton.OK, MessageBoxImage.Stop);
                csExceptionLogger.csExceptionLogger.Write("MonkeyCacheIssues", e);
            } finally
            {
                var application = new App();
                application.InitializeComponent();
                application.Run();
            }
        }
    }
}
