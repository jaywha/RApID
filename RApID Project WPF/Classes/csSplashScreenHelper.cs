﻿/*
 * csSplashScreenHelper.cs: Helps display the SplashScreen.
 * Created By: Eric Stabile
 */

using System;
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
        /// <summary> Stop Token generator for controlling when splash class threads should close. </summary>
        public static CancellationTokenSource StopTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2.0));
        /// <summary> Stop Token tied to the splash class threads. </summary>
        [Obsolete("Not in use. Kept for legacy and debug purposes.")]
        public static CancellationToken StopToken = StopTokenSource.Token;
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
                    thread_Hide = new Thread(new ThreadStart(async delegate ()
                        {
                            await SplashScreen.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(delegate ()
                                {
                                    Thread.Sleep(TIMEOUT);
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
                    thread_Show = new Thread(new ThreadStart(async delegate ()
                    {
                        await SplashScreen.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(delegate ()
                        {
                            ((SplashScreenVM)SplashScreen.DataContext).SplashText = text;
                        }));
                    }));
                    thread_Show.Name = "RApID_Show";
                    thread_Show.SetApartmentState(ApartmentState.STA);
                    thread_Show.Start();
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
                    thread_Close = new Thread(new ThreadStart(async delegate ()
                    {
                        await SplashScreen.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(delegate ()
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
                    }));
                    thread_Close.Name = "RApID_Close";
                    thread_Close.SetApartmentState(ApartmentState.STA);
                    thread_Close.Start();
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