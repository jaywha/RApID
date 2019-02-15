﻿/*
 * csSplashScreenHelper.cs: Helps display the SplashScreen.
 * Created By: Eric Stabile
 */

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using EricStabileLibrary;

namespace RApID_Project_WPF
{
    internal class csSplashScreenHelper
    {
        public static frmSplashScreen SplashScreen { get; set; }
        public static readonly int TIMEOUT = 500; // in miliseconds

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
                    SplashScreen.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(delegate ()
                    {
                        Thread.Sleep(TIMEOUT);
                        try
                        {
                            SplashScreen.Hide();
                        }
                        catch { }
                    }));
                }
                else SplashScreen.Hide();
            }
            catch (ThreadAbortException tce)
            {
                Console.WriteLine("Thread - caught ThreadAbortException - resetting.");
                Console.WriteLine("Exception message: {0}", tce.Message);
                Thread.ResetAbort();
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
                    SplashScreen.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(delegate ()
                    {
                        ((SplashScreenVM)SplashScreen.DataContext).SplashText = text;
                    }));
                }
                else ((SplashScreenVM)SplashScreen.DataContext).SplashText = text;
            }
            catch (ThreadAbortException tce)
            {
                Console.WriteLine("Thread - caught ThreadAbortException - resetting.");
                Console.WriteLine("Exception message: {0}", tce.Message);
                Thread.ResetAbort();
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
                    SplashScreen.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(delegate ()
                    {
                        Thread.Sleep(TIMEOUT);
                        try
                        {
                            SplashScreen.Close();
                            SplashScreen = null;
                        }
                        catch (InvalidOperationException ioe)
                        {
                            csExceptionLogger.csExceptionLogger.Write("SplashScreenHelper_CloseAsyncCatch", ioe);
                        }
                    }));
                }
                else SplashScreen.Close();
            }
            catch (ThreadAbortException tce)
            {
                Console.WriteLine("Thread - caught ThreadAbortException - resetting.");
                Console.WriteLine("Exception message: {0}", tce.Message);
                Thread.ResetAbort();
            }
            catch (Exception ex)
            {
                csExceptionLogger.csExceptionLogger.Write("SplashScreenHelper_Close", ex);
            }
        }
    }
}
