using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using GetNetLib;

namespace RApIDStarter
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            string error = "ERROR: ";
            string StartupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            StartupFolder += @"\RApID\";
            string mainFileFolder = @"\\joi\eu\Public\EE Process Test\Software\RApID Project WPF\Main\RApID Project WPF.exe";
            string version471 = @"\\joi\eu\Public\EE Process Test\Software\RApID Project WPF\PreRelease\RApID Project WPF.exe";
            string barcodeDLL = @"\\joi\eu\Public\EE Process Test\Software\RApID Project WPF\Main\BarcodeLib.dll";

            string exeName = Path.GetFileName(mainFileFolder);
            string barcodeName = Path.GetFileName(barcodeDLL);
            string exeNameOnly = Path.GetFileNameWithoutExtension(mainFileFolder);

            try
            {
                if(VersionTest.MostRecent)
                {
                    if (!Directory.Exists(StartupFolder))
                    {
                        Directory.CreateDirectory(StartupFolder);
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
                    System.Diagnostics.Process.Start(StartupFolder + exeName);
                } else if (File.Exists(mainFileFolder))
                {
                    if (!Directory.Exists(StartupFolder))
                    {
                        Directory.CreateDirectory(StartupFolder);
                    }

                    StartupFolder += exeNameOnly + @"\";
                    if (!Directory.Exists(StartupFolder))
                    {
                        Directory.CreateDirectory(StartupFolder);
                    }

                    //Copy the DLL file.
                    File.Copy(barcodeDLL, StartupFolder + barcodeName, true);

                    //Copy the main file.
                    File.Copy(mainFileFolder, StartupFolder + exeName, true);

                    //Start the program.
                    System.Diagnostics.Process.Start(StartupFolder + exeName);
                }
                else
                {
                    error += "File does not exist.\n\n" + mainFileFolder;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(error + "\n" + ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (error != "ERROR: ")
                {
                    MessageBox.Show(error, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
