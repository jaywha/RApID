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
        private double _currentVersion = 1.232;
        private string _fileCheckLocation = @"P:\EE Process Test\Software\RApID\version.txt";
        public double CurrentVersion
        {
            get { return _currentVersion; }
        }
        public DispatcherTimer tVersionChecker;

        public void initVersionControl()
        {
            //tVersionChecker = new DispatcherTimer();
            //tVersionChecker.Tick += new EventHandler(tVersionChecker_Tick);
            //tVersionChecker.Interval = new TimeSpan(0, 0, 30);
            //tVersionChecker.Start();
        }

        private void tVersionChecker_Tick(object sender, EventArgs e)
        {
            try
            {
                using (StreamReader reader = new StreamReader(_fileCheckLocation))
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
