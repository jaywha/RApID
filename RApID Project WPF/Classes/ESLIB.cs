/*
 * ESLIB.cs: A collection of classes used throughout RApID
 * Created By: Eric Stabile
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.IO;
using System.Xml.Serialization;
using System.Data.Sql;
using System.Data.SqlClient;
using RApID_Project_WPF;
using System.Threading;

namespace EricStabileLibrary
{
    public static class csSerialPort
    {
        /// <summary>
        /// </summary>
        /// <returns>Returns a string array with all of the available ports on the computer.</returns>
        public static string[] GetPortNames()
        {
            return SerialPort.GetPortNames();
        }

        /// <summary>
        /// </summary>
        /// <returns>Returns a string array with all of the common baud rates for RS232.</returns>
        public static string[] GetBaudRates()
        {
            return new string[] { "300", "1200", "2400", "4800", "9600", "14400", "19200", "28800", "38400", "57600", "115200", "230400" };
        }

        /// <summary>
        /// </summary>
        /// <returns>Returns an int array with a list of the common data bits for RS232.</returns>
        public static int[] GetDataBits()
        {
            return new int[] { 7, 8 };
        }

        /// <summary>
        /// </summary>
        /// <returns>Returns a list of Parity</returns>
        public static List<Parity> GetParityList()
        {
            var lParity = new List<Parity>();
            foreach(Parity p in Enum.GetValues(typeof(Parity)))
            {
                lParity.Add(p);
            }
            return lParity;
        }

        /// <summary>
        /// </summary>
        /// <returns>Returns a list of all available Stop Bits.</returns>
        public static List<StopBits> GetStopBits()
        {
            var lStopBits = new List<StopBits>();
            foreach(StopBits sb in Enum.GetValues(typeof(StopBits)))
            {
                lStopBits.Add(sb);
            }
            return lStopBits;
        }
    }

    /// <summary>
    /// Extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Quick extension method for forcing combo boxes to accept manual input.
        /// </summary>
        /// <param name="cmbx"></param>
        public static void PrepForManualInput(this System.Windows.Controls.ComboBox cmbx)
        {
            cmbx.IsReadOnly = false;
            cmbx.IsEnabled = true;
            cmbx.IsEditable = true;
        }

        /// <summary>
        /// Error symbol to ensure we can see when an error happens.
        /// </summary>
        public const string ERROR_GLYPH = "╓╥╫╥┘";

        /// <summary>
        /// Convenience extension method for doing <see cref="IConvertible"/> conversions, piped to a <see cref="Nullable&lt;T&gt;"/>.
        /// </summary>
        /// <typeparam name="T">Conversion Target Type</typeparam>
        /// <param name="o">Source object to convert - normally a string.</param>
        /// <returns>The value as an instance of type <typeparamref name="T"/>, or type <typeparamref name="T"/>'s default value.</returns>
        public static T TryChangeTypeTo<T>(this object o)
        {
            if (o.ToString().Equals(ERROR_GLYPH)) return default(T);

            try
            {
                Type t = typeof(T);
                Type u = Nullable.GetUnderlyingType(t);

                if (u != null)
                {
                    return (o == null) ? default(T) : (T)Convert.ChangeType(o, u);
                }
                else
                {
                    return (T)Convert.ChangeType(o, t);
                }
            }
            catch (Exception ex)
              when (ex is ArgumentNullException
                  || ex is FormatException
                  || ex is OverflowException
                  || ex is InvalidCastException)
            {
                Console.WriteLine($"[ChangeTypeTo_TypicalException] ==> {ex}");
                Console.WriteLine($"[Exception:{ex.GetType()}] -> {ex.Message}");
                return default(T);
            }
        }

        /// <summary>
        /// Convenience extension method for doing <see cref="IConvertible"/> conversions, piped to a <see cref="Nullable&lt;T&gt;"/>.
        /// </summary>
        /// <typeparam name="T">Conversion Target Type</typeparam>
        /// <param name="o">Source object to convert - normally a string.</param>
        /// <param name="result">The value as an instance of type <typeparamref name="T"/>, or type <typeparamref name="T"/>'s default value.</param>
        /// <returns>True if operation successful.</returns>
        public static bool TryChangeTypeTo<T>(this object o, out T result)
        {
            if (o == null || o.ToString().Equals(ERROR_GLYPH))
            {
                result = default(T);
                return false;
            }

            try
            {
                Type t = typeof(T);
                Type u = Nullable.GetUnderlyingType(t);

                if (u != null)
                {
                    if (o.ToString().Contains("Bad Value"))
                    {
                        result = default(T);
                        return false;
                    }
                    result = (o == null) ? default(T) : (T)Convert.ChangeType(o, u);
                }
                else
                {
                    if (o.ToString().Contains("Bad Value"))
                    {
                        result = default(T);
                        return false;
                    }
                    result = (T)Convert.ChangeType(o, t);
                }
            }
            catch (Exception ex)
              when (ex is ArgumentNullException
                  || ex is FormatException
                  || ex is OverflowException
                  || ex is InvalidCastException)
            {
                Console.WriteLine($"[ChangeTypeTo_TypicalException] ==> {ex}");
                Console.WriteLine($"[Exception:{ex.GetType()}] -> {ex.Message}");
                result = default(T);
                return false;
            }
            return true;
        }
    }

    public static class csAppSettings
    {
        /// <summary>
        /// </summary>
        /// <returns>Returns a list of all connection strings for a specific connection type.</returns>
        public static string[] GetConnectionStrings(string sConnList)
        {
            string[] splitters = { "{", "}" };
            return sConnList.Split(splitters, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// </summary>
        /// <param name="sConnList">The current connection string list.</param>
        /// <param name="sConnToAdd">The connection string to add.</param>
        /// <returns>Returns a string that represents all possible connection strings separated with brackets after the specified connection string has been added.</returns>
        public static string AddNewConnectionString(string sConnList, string sConnToAdd)
        {
            string sNewConnList = "{";

            string[] splitters = { "{", "}" };
            string[] sSplit = sConnList.Split(splitters, StringSplitOptions.RemoveEmptyEntries);

            foreach(string s in sSplit)
            {
                sNewConnList += s + "}{";
            }

            sNewConnList += sConnToAdd + "}";

            return sNewConnList;
        }

        /// <summary>
        /// </summary>
        /// <param name="sConnList">The current connection string list.</param>
        /// <param name="sConnToRemove">The connection string to remove.</param>
        /// <returns>Returns a string that represents all possible connection strings separated with brackets after the specified connection string has been removed.</returns>
        public static string RemoveConnectionString(string sConnList, string sConnToRemove)
        {
            string sNewConnList = "{";

            string[] splitters = { "{", "}" };
            string[] sSplit = sConnList.Split(splitters, StringSplitOptions.RemoveEmptyEntries);

            foreach(string s in sSplit)
            {
                if (s != sConnToRemove)
                    sNewConnList += s + "}{";
            }

            if (sNewConnList.EndsWith("{"))
                sNewConnList = sNewConnList.TrimEnd(new char[] { '{' });

            return sNewConnList;
        }
    }

    /// <summary>
    /// csSerialization will be used to help serialize/deserialize the log files.
    /// </summary>
    public static class csSerialization
    {
        public static int CurrLogIDToUse = 0;

        /// <summary>
        /// Used to serialize a file.
        /// </summary>
        public static bool serializeFile(string tech, DateTime dtLC, List<csLogAction> lLA, string sFileLoc, string sFileName)
        {
            var csL = new csLog();
            csL.buildCSLog(tech, dtLC, lLA);

            StreamWriter sw = null;
            try
            {
                var _ser = new XmlSerializer(typeof(csLog));
                sw = new StreamWriter(sFileLoc + sFileName, false);
                _ser.Serialize(sw, csL);
                sw.Close();
                sw = null;


                #region Submit Log to DB

                using (var conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
                {
                    conn.Open();

                    /* [1.] Get ActionID index to increment by 1. */
                    var actionID = 0;
                    using (var cmd = new SqlCommand("SELECT ActionID FROM TechLogActions ORDER BY ActionID DESC", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                actionID = reader.GetInt32(0) + 1;
                            }
                            else throw new IOException("ActionID could not be set.\nPlease confirm validity in the Database.");
                        }
                    }

                    /* [2.] Insert initial log details into TechLogs table. */
                    using (var cmd = new SqlCommand("INSERT INTO TechLogs (ActionID, Tech, LogCreationTime, LogSubmitTime) " +
                        "VALUES (@aid, @tech, @createtime, @submittime)", conn))
                    {

                        cmd.Parameters.AddWithValue("@aid", actionID);
                        cmd.Parameters.AddWithValue("@tech", csL.Tech);
                        cmd.Parameters.AddWithValue("@createtime", csL.LogCreationTime);
                        cmd.Parameters.AddWithValue("@submittime", csL.LogSubmitTime);

                        if (cmd.ExecuteNonQuery() == 0) return false;
                    }

                    /* [3.] Get newest LogID after instering log entry (Auto Increment ID)*/
                    using(var cmd  = new SqlCommand("SELECT ID FROM TechLogs ORDER BY ID DESC", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                            if (reader.Read())
                            {
                                CurrLogIDToUse = reader.GetInt32(0) + 1;
                            }
                            else throw new IOException("LogID could not be set.\nPlease confirm validity in the Database.");
                    }

                    /* [4.] Insert all relevant actions into TechLogActions table with ActionID as the pairing column. */
                    foreach (var action in csL.lActions)
                    {
                        using (var cmd = new SqlCommand("INSERT INTO TechLogActions (ActionID, ControlType, ControlName, " +
                            "ControlContent, LogState, EventTiming, LogNote, LogError)" +
                            "VALUES (@aid, @cType, @cName, @cContent, @state, @time, @note, @error)", conn))
                        {
                            cmd.Parameters.AddWithValue("@aid", actionID);
                            cmd.Parameters.AddWithValue("@cType", action.ControlType ?? "");
                            cmd.Parameters.AddWithValue("@cName", action.ControlName ?? "");
                            cmd.Parameters.AddWithValue("@cContent", action.ControlContent ?? "");
                            cmd.Parameters.AddWithValue("@state", action.EventType.ToString() ?? "");
                            cmd.Parameters.AddWithValue("@time", action.EventTiming.ToString("yyyy-MM-dd HH:mm:ss.fff")
                                ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                            cmd.Parameters.AddWithValue("@note", action.LogNote ?? "");
                            cmd.Parameters.AddWithValue("@error", action.LogError);

                            if (cmd.ExecuteNonQuery() == 0) return false;
                        }
                    }

                }
                
                #endregion

                return true;
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Windows.MessageBox.Show("Error writing log to file.\n\nError Message: " + ex.Message);
#endif
                if (sw != null)
                    sw.Close();
                sw = null;
                return false;
            }
        }

        /// <summary>
        /// Used to deserialize a file.
        /// </summary>
        public static csLog deserializeFile(string _sFileLoc)
        {
            csLog logReadIn = null;

            var serRead = new XmlSerializer(typeof(csLog));
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(_sFileLoc);
                logReadIn = (csLog)serRead.Deserialize(sr);
                sr.Close();
                sr = null;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error while reading in log file.\n\nError Message: " + ex.Message);
                if (sr != null)
                    sr.Close();
                sr = null;
            }

            return logReadIn;
        }
    }

    /// <summary>
    /// csLogging handles every single log action within RApID
    /// </summary>
    public class csLogging
    {
        public enum LogState { ENTER, LEAVE, CLICK, NOTE, ERROR, WARNING, SUBMISSIONDETAILS, DROPDOWNCLOSED, SQLQUERY, NONE };

        private string Technician;
        private string LogLocation;
        public List<csLogAction> lLogActions = new List<csLogAction>();
        private int iTempCounter = 0;

        private DateTime _logCreation;
        public DateTime LogCreation
        {
            set { _logCreation = value; }
        }

        public csLogging(string sTech, DateTime dtLC, string sLL)
        {
            Technician = sTech;
            _logCreation = dtLC;
            LogLocation = sLL;
        }

        /// <summary>
        /// This will check to see if the technician has a directory in the Log location. If not it will generate a new directory.
        /// </summary>
        public void CheckDirectory(string _tech)
        {
            if(!Directory.Exists(LogLocation + _tech) && iTempCounter < 3) 
            { 
                Directory.CreateDirectory(LogLocation + _tech);
                CheckDirectory(_tech);
                iTempCounter++;
            }

            LogLocation = LogLocation + _tech + @"\";

        }

        #region Adds a specific log action to the log
        public void CreateLogAction(string sLAction, LogState ls)
        {
            lLogActions.Add(new csLogAction {EventType = ls, LogNote = sLAction, EventTiming = DateTime.Now });
        }

        public void CreateLogAction(System.Windows.Controls.TextBox _tb, LogState ls)
        {
            lLogActions.Add(new csLogAction { ControlType = "Textbox", ControlName = _tb.Name, ControlContent = _tb.Text.ToString(), EventType = ls, EventTiming = DateTime.Now });
        }
        
        public void CreateLogAction(System.Windows.Controls.RichTextBox _rtb, LogState ls)
        {
            lLogActions.Add(new csLogAction { ControlType = "RichTextBox", ControlName = _rtb.Name, ControlContent = new System.Windows.Documents.TextRange(_rtb.Document.ContentStart, _rtb.Document.ContentEnd).Text.TrimEnd().ToString(), EventType = ls, EventTiming = DateTime.Now });
        }

        public void CreateLogAction(System.Windows.Controls.Button _btn, LogState ls)
        {
            lLogActions.Add(new csLogAction { ControlType = "Button", ControlName = _btn.Name, ControlContent = _btn.Content.ToString(), EventType = ls, EventTiming = DateTime.Now });
        }

        public void CreateLogAction(System.Windows.Controls.ComboBox _cb, LogState ls)
        {
            lLogActions.Add(new csLogAction { ControlType = "ComboBox", ControlName = _cb.Name, ControlContent = _cb.Text.ToString(), EventType = ls, EventTiming = DateTime.Now });
        }

        public void CreateLogAction(bool bLogErr, LogState ls)
        {
            lLogActions.Add(new csLogAction { LogError = bLogErr, EventType = ls, EventTiming = DateTime.Now });
        }
        #endregion

        /// <summary>
        /// Resets the log.
        /// </summary>
        public void resetLog()
        {
            lLogActions.Clear();
        }

        /// <summary>
        /// Writes the log to a file.
        /// </summary>
        public void writeLogToFile()
        {
            csSerialization.serializeFile(Technician, _logCreation, lLogActions, LogLocation, Technician + "_" + DateTime.Now.ToString("MMddyyhhmmsstt") + ".xml");
        }
    }

    [Serializable()]
    public class csLog
    {
        public string Tech { get; set; }
        public DateTime LogCreationTime { get; set; }
        public List<csLogAction> lActions { get; set; }
        public DateTime LogSubmitTime { get; set; }
        public void buildCSLog(string sTech, DateTime dtLC, List<csLogAction> lLA)
        {
            Tech = sTech;
            LogCreationTime = dtLC;
            lActions = lLA;
            LogSubmitTime = DateTime.Now;
        }
    }

    public class csLogAction
    {
        public string ControlType { get; set; }
        public string ControlName { get; set; }
        public string ControlContent { get; set; }
        public csLogging.LogState EventType { get; set; }
        public DateTime EventTiming { get; set; }
        public string LogNote { get; set; }
        public bool LogError { get; set; }
    }

    /// <summary>
    /// Displays the selected log
    /// </summary>
    public class csDisplayLog
    {
        public static void DisplayLog(System.Windows.Controls.RichTextBox _rtb, csLogging.LogState _ls, csLog _log, bool bClearRTB)
        {
            if(bClearRTB)
                _rtb.Document.Blocks.Clear();

            string sLogData = $"{_log.Tech} began this entry at {_log.LogCreationTime.ToString("MM/dd/yyyy hh:mm:ss tt")}.\n";

            sLogData += "*** Filtered Actions Associated With This Log File*** \n";

            for (int i = 0; i < _log.lActions.Count; i++)
            {
                if (_ls.Equals(_log.lActions[i].EventType) || _ls.Equals(csLogging.LogState.NONE))
                {
                    sLogData += $"Event Type: {_log.lActions[i].EventType.ToString()}\n";

                    if (_log.lActions[i].LogNote != null)
                        sLogData += $"Log Action: {_log.lActions[i].LogNote}\n";
                    if (_log.lActions[i].ControlType != null)
                        sLogData += $"Control Type: {_log.lActions[i].ControlType}\n";
                    if (_log.lActions[i].ControlName != null)
                        sLogData += $"Control Name: {_log.lActions[i].ControlName}\n";
                    if (_log.lActions[i].ControlContent != null)
                        sLogData += $"Control Content: {_log.lActions[i].ControlContent}\n";

                    sLogData += "Event Timing: " + _log.lActions[i].EventTiming.ToString("MM/dd/yyyy hh:mm:ss tt") + "\n";

                    sLogData += "----------------------------\n";
                }
            }

            sLogData += "This log was completed on " + _log.LogSubmitTime.ToString("MM/dd/yyyy hh:mm:ss tt") + "\n";
            sLogData += "*** End of Log ***";

            _rtb.AppendText(sLogData);
        }
    }

    /// <summary>
    /// ViewModel for the SplashScreen
    /// </summary>
    public class SplashScreenVM : RApID_Project_WPF.VMB.ViewModelBase
    {
        private string _splashText = "Loading...";
        public string SplashText
        {
            get { return _splashText; }
            set
            {
                if (_splashText != value)
                {
                    _splashText = value;
                    NotifyPropertyChanged(() => SplashText);
                }
            }
        }
    }

    public class MultipleRPVM: RApID_Project_WPF.VMB.ViewModelBase
    {
        private string _rpNumber;
        public string VMRPNumber
        {
            get { return _rpNumber; }
            set
            {
                if(_rpNumber != value)
                {
                    _rpNumber = value;
                    NotifyPropertyChanged(() => VMRPNumber);
                }
            }
        }
        private string _customerNumber;
        public string VMCustomerNumber
        {
            get { return _customerNumber; }
            set
            {
                if(_customerNumber != value)
                {
                    _customerNumber = value;
                    NotifyPropertyChanged(() => _customerNumber);
                }
            }
        }
        private string _customerName;
        public string VMCustomerName
        {
            get { return _customerName; }
            set
            {
                if(_customerName != value)
                {
                    _customerName = value;
                    NotifyPropertyChanged(() => _customerName);
                }
            }
        }
    }

    /// <summary>
    /// Initializes the splash screen.
    /// </summary>
    public class InitSplash
    {
        public static Thread thread_Splash;

        public void InitSplash1(string sLoadText)
        {
            try
            {                
                thread_Splash = new Thread(new ThreadStart(
                    delegate ()
                    {
                        csSplashScreenHelper.SplashScreen = new frmSplashScreen();
                        csSplashScreenHelper.Show();
                        if (!string.IsNullOrEmpty(sLoadText))
                        {
                            csSplashScreenHelper.ShowText(sLoadText);
                        }
                        System.Windows.Threading.Dispatcher.Run();
                    }));
                thread_Splash.Name = "RApID_InitSplash";
                thread_Splash.SetApartmentState(ApartmentState.STA);
                thread_Splash.IsBackground = true;
                thread_Splash.Start();

                Thread.Sleep(1000);
            }
            catch(ThreadAbortException tce)
            {
                Console.WriteLine("Thread - caught ThreadAbortException - resetting.");
                Console.WriteLine("Exception message: {0}", tce.Message);
                Thread.ResetAbort();
            }
            catch (Exception ex)
            {
                csExceptionLogger.csExceptionLogger.Write("InitSplash_InitSplash1", ex);
            }
        }
    }
}
