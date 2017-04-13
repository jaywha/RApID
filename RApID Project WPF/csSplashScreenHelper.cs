/*
 * csSplashScreenHelper.cs: Helps display the SplashScreen.
 * Created By: Eric Stabile
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Threading;
using EricStabileLibrary;

namespace RApID_Project_WPF
{
    internal class csSplashScreenHelper
    {
        public static frmSplashScreen SplashScreen { get; set; }

        /// <summary>
        /// Display Splash Screen
        /// </summary>
        public static void Show()
        {
            if (SplashScreen != null)
                SplashScreen.Show();
        }

        /// <summary>
        /// Hide Splash Screen
        /// </summary>
        public static void Hide()
        {
            if (SplashScreen == null) return;

            if (!SplashScreen.Dispatcher.CheckAccess())
            {
                Thread thread = new Thread(new System.Threading.ThreadStart(delegate()
                    {
                        SplashScreen.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate()
                            {
                                Thread.Sleep(1000);
                                try
                                {
                                    SplashScreen.Hide();
                                }
                                catch { }
                            }));
                    }));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
            else SplashScreen.Hide();
        }

        /// <summary>
        /// Show/Change the text displayed on the Splash Screen.
        /// </summary>
        public static void ShowText(string text)
        {
            if (SplashScreen == null) return;

            if (!SplashScreen.Dispatcher.CheckAccess())
            {
                Thread thread = new Thread(
                    new System.Threading.ThreadStart(
                        delegate()
                        {
                            SplashScreen.Dispatcher.Invoke(
                                DispatcherPriority.Normal,

                                new Action(delegate()
                                    {
                                        ((SplashScreenVM)SplashScreen.DataContext).SplashText = text;
                                    }
                            ));
                            SplashScreen.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
                        }
                ));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
            else ((SplashScreenVM)SplashScreen.DataContext).SplashText = text;
        }
    }
}
