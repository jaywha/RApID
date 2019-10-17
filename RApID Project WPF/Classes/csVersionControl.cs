/*
 * csVersionControl.cs: This is used to constantly check to make sure the user is using the latest version of the program.
 * Created By: Eric Stabile
 */

using System;
using System.Windows.Threading;
using System.IO;
using System.Windows.Forms;

//NOTE: This may not be needed since the updates have finished for the most part.

namespace RApID_Project_WPF
{
    public class csVersionControl
    {
        private string _fileCheckLocation = @"\\joi\eu\Public\EE Process Test\Software\RApID Project WPF\Main\version.txt";
        public double CurrentVersion
        {
            get; private set;
        } = 1.4;
        public DispatcherTimer tVersionChecker;

        private void tVersionChecker_Tick(object sender, EventArgs e)
        {
            try
            {
                using (StreamReader reader = new StreamReader(_fileCheckLocation))
                {
                    string sVer = reader.ReadLine();
                    double dVer = Convert.ToDouble(sVer);
                    if (dVer < CurrentVersion)
                        MessageBox.Show("OLD VERSION", $"You are running an old version of RApID ({CurrentVersion}). Please completely restart RApID to update to the newest version ({dVer})."
                            , MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch { }
        }
    }
}
