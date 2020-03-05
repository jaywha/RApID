using System;
using System.IO;
using System.Windows;
using RDM = RApID_Project_WPF.Classes.RDM;

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

            try
            {
                App application = new App();
                application.InitializeComponent();
                application.Run();
            } catch (Exception e) {
                Mailman.SendEmail(subject: "", body: "", exception: e);
                throw;
            }
        }
    }
}
