using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Forms;
using GetNetLib;

namespace RApIDStarter
{
    class Program
    {
        private static System.Diagnostics.Process RApID_Process;

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        [STAThread]
        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            string error = "ERROR: ";
            string StartupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            StartupFolder += @"\RApID\";
            string mainFileFolder = @"\\joi\eu\Public\EE Process Test\Software\RApID Project WPF\Main\RApID Project WPF.exe";
            string version471 = @"\\joi\eu\Public\EE Process Test\Software\RApID Project WPF\PreRelease\RApID Project WPF.exe";
            string barcodeDLL = @"\\joi\eu\Public\EE Process Test\Software\RApID Project WPF\Main\BarcodeLib.dll";
            string frameworkUpdater = @"\\joi\eu\Public\EE Process Test\Software\FrameworkUpdater\UpdateFramework.bat";

            string exeName = Path.GetFileName(mainFileFolder);
            string barcodeName = Path.GetFileName(barcodeDLL);
            string exeNameOnly = Path.GetFileNameWithoutExtension(mainFileFolder);

            try
            {
                if(VersionTest.Get45PlusFromRegistry() && VersionTest.MostRecent)
                {
                    try
                    {
                        if (!Directory.Exists(StartupFolder))
                        {
                            Directory.CreateDirectory(StartupFolder);
                        }
                    }
                    catch (Exception ex)
                    {
                        error += "File does not exist.\n\n" + mainFileFolder + "\n\nMessage: " + ex.Message + "\n\nStack Trace: " + ex.StackTrace + "\n";
                    }

                    StartupFolder += exeNameOnly + @"\PreRelease\";
                    if (!Directory.Exists(StartupFolder))
                    {
                        Directory.CreateDirectory(StartupFolder);
                    }

                    //Copy the DLL file.
                    File.Copy(barcodeDLL, StartupFolder + barcodeName, true);

                    //Copy the main file.
                    File.Copy(version471, StartupFolder + exeName, true);

                    //Start the program.
                    RApID_Process = System.Diagnostics.Process.Start(StartupFolder + exeName);
                } else {
                    var choice = MessageBox.Show("To use the latest version of RApID, you will need to run the Framework Updater program.",
                        "Update .NET Framework", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

                    if(choice==DialogResult.OK)
                    {
                        System.Diagnostics.Process.Start(frameworkUpdater);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowWindow(handle, SW_SHOW);
                MessageBox.Show(error + "\n" + ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (error != "ERROR: ")
                {
                    MessageBox.Show(error, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if(RApID_Process != null)
            {
                RApID_Process.WaitForExit();
            }
        }
    }
}
