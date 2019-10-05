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
        /// <summary>
        /// Custom Program Entry point
        /// </summary>
        /// <param name="args">Command Line Arguments</param>
        [STAThread]
        public static void Main(string[] args)
        {
            if(string.IsNullOrWhiteSpace(csExceptionLogger.csExceptionLogger.DefaultLogLocation))
            {
                csExceptionLogger.csExceptionLogger.DefaultLogLocation 
                    = $@"\\joi\EU\Public\EE Process Test\Logs\RApID\_Exceptions\{DateTime.Today:yyyy}\{DateTime.Today:MMMM}\";
                Directory.CreateDirectory(csExceptionLogger.csExceptionLogger.DefaultLogLocation);
            }
            
            // Set this form to be the form that should appear when a BOM is not found.
            SNMapperLib.csSerialNumberMapper.Instance.RApIDForm = new frmBoardAliases();

            try
            {
                RApID_Project_WPF.Classes.RDM.InitReg();
                //GetNetLib.VersionTest.InstallIfLowerThan45();
            } catch (Exception e) {
                MessageBox.Show("The cache library or registry class is causing an issue.", "DEBUGGING MESSAGE", MessageBoxButton.OK, MessageBoxImage.Stop);
                csExceptionLogger.csExceptionLogger.Write("MonkeyCacheORRegsitryIssues", e);
            } finally
            {
                var application = new App();
                application.InitializeComponent();
                application.Run();
            }
        }
    }
}
