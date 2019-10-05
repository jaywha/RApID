using EricStabileLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RApID_Project_WPF.Classes
{
    /// <summary>
    /// Model/Singleton for housing commonly used registry constants and keys.
    /// </summary>
    public class RDM : INotifyPropertyChanged
    {
        #region Property Changed Implementation
        /// <summary> Event to trigger when a property is changed and propogate data. </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Static Property Changed Implementation
        /// <summary> Event to trigger when a property is changed and propogate data. </summary>
        public static event PropertyChangedEventHandler StaticPropertyChanged;
        private static void OnStaticPropertyChanged([CallerMemberName] string propName = "")
            => StaticPropertyChanged?.Invoke(Instance, new PropertyChangedEventArgs(propName));
        #endregion

        private static readonly Lazy<RDM> lazy
            = new Lazy<RDM>(() => new RDM());

        /// <summary> Singleton Accessor to the global Instance of this class </summary>
        public static RDM Instance { get { return lazy.Value; } }

        private static string _currentCOMIdentifier = "Default";
        /// <summary> Internal ID of the target COM device </summary>
        public static string CurrentCOMIdentifier {
            get => _currentCOMIdentifier;
            set {
                _currentCOMIdentifier = value;
                OnStaticPropertyChanged();
            }
        }

        private RDM() { }

        /// <summary>
        /// The currently running applicaiton's friendly name from <see cref="AppDomain.CurrentDomain"/>.
        /// </summary>
        public static readonly string CurrentRunningApplication = AppDomain.CurrentDomain.FriendlyName.Replace(".exe", "");

        /// <summary> Have we inited the registry? </summary>
        public static string Inited {
            get => $"{CurrentCOMIdentifier}_IsInited";
        }
        /// <summary> Current COM Port's Baude Rate </summary>
        public static string BaudRate { 
            get => $"{CurrentCOMIdentifier}_BaudRate";
        }
        /// <summary> Current COM Port's COM Port </summary>
        public static string COMPort { 
            get => $"{CurrentCOMIdentifier}_COMPort";
        }
        /// <summary> Current COM Port's Data Bits </summary>
        public static string DataBits { 
            get => $"{CurrentCOMIdentifier}_DataBits";
        }
        /// <summary> Current COM Port's Stop Bits</summary>
        public static string StopBits {
            get => $"{CurrentCOMIdentifier}_StopBits";
        }
        /// <summary> Current COM Port's Parity </summary>
        public static string Parity { 
            get => $"{CurrentCOMIdentifier}_Parity";
        }
        /// <summary> Common Entrypoint RegistryKey </summary>
        public static readonly RegistryKey DefaultKey;
        /// <summary> Entrypoint for top level LocalMachine RegistryKey </summary>
        public static readonly RegistryKey LocalMachine;
        /// <summary> Entrypoint for the LocalMachine Uninstall RegistryKey </summary>
        public static readonly RegistryKey WindowsLocalMachine;
        /// <summary> Entrypoint for the LocalMachine Uninstall RegistryKey (64-bit version) </summary>
        public static readonly RegistryKey WindowsLocalMachinex64;

        /// <summary> Set Variable on Static Level to avoid uninstantiated variables </summary>
        static RDM()
        {
            DefaultKey = Registry.CurrentUser.CreateSubKey($@"SOFTWARE\{CurrentRunningApplication}");
            LocalMachine = Registry.LocalMachine;
            WindowsLocalMachine = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            WindowsLocalMachinex64 = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
        }

        /// <summary>
        /// Initializes the registry for the current application.
        /// </summary>
        /// <returns>Boolean success as result</returns>
        public static bool InitReg()
        {
            void Set(string @default, object value)
                => DefaultKey.SetValue(@default, value, RegistryValueKind.String);
            try
            {
                Set(Inited, "true");
                Set(BaudRate, "9600");
                Set(COMPort, "COM3");
                Set(DataBits, "8");
                Set(Parity, "None");
                Set(StopBits, "One");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR:RDM_Initializing] ==> {e}");
                return false;
            }

            Console.WriteLine($"[INFO:RDM_Initializing] ==> {CurrentCOMIdentifier}");

            return true;
        }

        /// <summary>
        /// Will pull the value of the specified RegistryKey as a generic object.
        /// </summary>
        /// <exception cref="System.Security.SecurityException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <param name="key">The target <see cref="RegistryKey"></see> value.</param>
        /// <param name="valueName">The name of the value to read from the target key.</param>
        /// <returns> The Value held in the target RegistryKey </returns>
        public static object ReadFromReg(RegistryKey key, string valueName)
            => key.GetValue(valueName) ?? default;

        /// <summary>
        /// Will try to cast the value of the specified RegistryKey to type <typeparamref name="T"/>.
        /// </summary>
        /// <exception cref="System.Security.SecurityException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <typeparam name="T">The target type to cast toward.</typeparam>
        /// <param name="key">The target <see cref="RegistryKey"></see> value.</param>/// 
        /// <param name="valueName">The name of the value to read from the target key.</param>
        /// <returns> The Value held in the target RegistryKey </returns>
        public static T ReadFromReg<T>(RegistryKey key, string valueName)
            => ((IConvertible)key.GetValue(valueName)).TryChangeTypeTo(out T result) ? result : default;

        /// <summary>
        /// Will push the specified value to the RegistryKey.
        /// </summary>
        /// <example>
        /// RDM.WrtieToReg(RDM.LocalMachine, val, name);
        /// => RDM.LocalMachine.SetValue(name, val.ToString());
        /// </example>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="System.Security.SecurityException"/>
        /// <exception cref="IOException"/>
        /// <param name="key">The target <see cref="RegistryKey"></see> value.</param>
        /// <param name="val">The Value to write to the name part of key.</param>
        /// <param name="name">The name of the attribute to write to in the registry.</param>
        public static void WriteToReg(RegistryKey key, IConvertible val, string name)
            => key.SetValue(name, val.ToString());

        /// <summary>
        /// Will try to push the specified value as the designated type to the RegistryKey.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="System.Security.SecurityException"/>
        /// <exception cref="IOException"/>
        /// <typeparam name="T">The target type from which to cast to object.</typeparam>
        /// <param name="key">The target <see cref="RegistryKey"></see> value.</param>
        /// <param name="val">The Value to write to the name part of key.</param>
        /// <param name="name">The Value to write to the key.</param>
        public static void WriteToReg<T>(RegistryKey key, IConvertible val, string name)
            => key.SetValue(name, (val.TryChangeTypeTo(out T result) ? result : default).ToString());
    }
}
