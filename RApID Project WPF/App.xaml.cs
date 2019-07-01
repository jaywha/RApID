using System;
using System.IO;
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
            if(string.IsNullOrWhiteSpace(csExceptionLogger.csExceptionLogger.DefaultLogLocation))
            {
                csExceptionLogger.csExceptionLogger.DefaultLogLocation 
                    = $@"\\joi\EU\Public\EE Process Test\Logs\RApID\_Exceptions\{DateTime.Today:yyyy}\{DateTime.Today:MMMM}\";
                Directory.CreateDirectory(csExceptionLogger.csExceptionLogger.DefaultLogLocation);
            }

            try
            {
                //GetNetLib.VersionTest.InstallIfLowerThan45();
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
