/*
 * csVersionControl.cs: This is used to constantly check to make sure the user is using the latest version of the program.
 * Created By: Eric Stabile
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.IO;

//TODO: Fully Implement This.
//NOTE: This may not be needed since the updates have finished for the most part.

namespace RApID_Project_WPF
{
    public class csVersionControl
    {
        private double _currentVersion = 1.233;
        private string _fileCheckLocation = @"\\joi\eu\Public\EE Process Test\Software\RApID Project WPF\Main\version.txt";
        public double CurrentVersion
        {
            get { return _currentVersion; }
        }
        public DispatcherTimer tVersionChecker;

        private void tVersionChecker_Tick(object sender, EventArgs e)
        {
            try
            {
                using (var reader = new StreamReader(_fileCheckLocation))
                {
                    string sVer = reader.ReadLine();
                    double dVer = Convert.ToDouble(sVer);
                    if (dVer < _currentVersion)
                        System.Windows.Forms.MessageBox.Show("You are running an old version of RApID. Please completely restart RApID to update to the newest version.", "OLD VERSION", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
                }
            }
            catch { }
        }
    }
}
