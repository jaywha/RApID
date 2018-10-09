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
        public static Thread thread_Hide;
        public static Thread thread_Show;
        public static Thread thread_Close;

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
            try
            {
                if (SplashScreen == null) return;

                if (!SplashScreen.Dispatcher.CheckAccess())
                {
                    thread_Hide = new Thread(new System.Threading.ThreadStart(delegate ()
                        {
                            SplashScreen.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
                                {
                                    Thread.Sleep(1000);
                                    try
                                    {
                                        SplashScreen.Hide();
                                    }
                                    catch { }
                                }));
                        }));
                    thread_Hide.Name = "RApID_Hide";
                    thread_Hide.SetApartmentState(ApartmentState.STA);
                    thread_Hide.Start();
                }
                else SplashScreen.Hide();
            }
            catch (Exception ex)
            {
                csExceptionLogger.csExceptionLogger.Write("SplashScreenHelper_Hide", ex);
            }
        }

        /// <summary>
        /// Show/Change the text displayed on the Splash Screen.
        /// </summary>
        public static void ShowText(string text)
        {
            try
            {
                if (SplashScreen == null) return;

                if (!SplashScreen.Dispatcher.CheckAccess())
                {
                    thread_Show = new Thread(
                        new System.Threading.ThreadStart(
                            delegate ()
                            {
                                SplashScreen.Dispatcher.Invoke(
                                    DispatcherPriority.Normal,

                                    new Action(delegate ()
                                        {
                                            ((SplashScreenVM)SplashScreen.DataContext).SplashText = text;
                                        }
                                ));
                                SplashScreen.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
                            }
                    ));
                    thread_Show.Name = "RApID_Show";
                    thread_Show.SetApartmentState(ApartmentState.STA);
                    thread_Show.Start();
                }
                else ((SplashScreenVM)SplashScreen.DataContext).SplashText = text;
            }
            catch (Exception ex)
            {
                csExceptionLogger.csExceptionLogger.Write("SplashScreenHelper_ShowText", ex);
            }
        }

        /// <summary>
        /// Close Splash Screen
        /// </summary>
        public static void Close()
        {
            try
            {
                if (SplashScreen == null) return;

                if (!SplashScreen.Dispatcher.CheckAccess())
                {
                    thread_Close = new Thread(new System.Threading.ThreadStart(delegate ()
                    {
                        SplashScreen.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
                        {
                            Thread.Sleep(1000);
                            try
                            {
                                SplashScreen.Close();
                            }
                            catch { }
                        }));
                    }));
                    thread_Close.Name = "RApID_Close";
                    thread_Close.SetApartmentState(ApartmentState.STA);
                    thread_Close.Start();
                }
                else SplashScreen.Close();
            }
            catch (Exception ex)
            {
                csExceptionLogger.csExceptionLogger.Write("SplashScreenHelper_Close", ex);
            }
        }
    }
}
