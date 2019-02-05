﻿/*
 * csCrossClassInteraction.cs: Used when the same class/function/method is needed across multiple forms.
 * Created By: Eric Stabile
 */

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using ExcelDataReader;
using DesginatorPair = System.Tuple<System.Windows.Controls.Control, System.Windows.Controls.Control>;
using System.Threading.Tasks;
using System.Windows;
using RApID_Project_WPF.UserControls;
using System.Drawing;
using System.Windows.Threading;
using System.Runtime.Caching;
using MonkeyCache.FileStore;
using MonkeyCache;

namespace RApID_Project_WPF
{
    public enum DataGridTypes
    {
        /// <summary> UnitIssues<para/>(Reference Designator, Part Number, Part Description)</summary>
        MULTIPLEPARTS,
        /// <summary> Tech Subs [AOI Tab Main]<para/>(Sys ID,Inspector,Assembly,SerialNumber,Ref ID,Defect Code,Part Total,Rec Type,Reworked,PartNumber,BoardFail)</summary>
        AOI,
        /// <summary> Tech Subs [AOI Tab Legend]<para/>(Code,Description)</summary>
        DEFECTCODES,
        /// <summary> Tech Subs [DataLog Tab]<para/>(Tech Name, Date Submitted, SerialNumber)</summary>
        PREVREPAIRINFO,
        /// <summary> UNUSED <para/> (Part Name,Part Number)</summary>
        PARTNUMBERNAME,
        /// <summary> Multiple RP Numbers <para/> (Repair Number, Customer Number, Customer Name)</summary>
        CUSTOMERINFO
    }

    public static class csCrossClassInteraction
    {
        /// <summary> Shortcut to <see cref="Banana"/> instance modelled in <see cref="Barrel.Current"/> </summary>
        public static IBarrel Cache = Barrel.Current;
        private static StaticVars sVar = StaticVars.StaticVarsInstance();
        private static csObjectHolder.csObjectHolder holder = csObjectHolder.csObjectHolder.ObjectHolderInstance();

        public static List<string> ReferenceDesignators { get; private set; } = new List<string>();
        public static List<string> PartNumbers { get; private set; } = new List<string>();

        /// <summary>
        /// Takes an empy datagrid and fills it with the appropriate columns based on the criteria.
        /// This is used in Production and Repair.
        /// </summary>
        /// <param name="dgToBuid"></param>
        /// <param name="sType">"MULTIPLEPARTS", "AOI", "DEFECTCODES", "PREVREPAIRINFO", "PARTNUMBERNAME", "CUSTOMERINFO"</param>
        public static void dgBuildView(this DataGrid dgToBuid, DataGridTypes sType)
        {
            switch (sType)
            {
                case DataGridTypes.MULTIPLEPARTS:
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Ref Designator", "RefDesignator"));
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Part Replaced", "PartReplaced"));
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Part Description", "PartsReplacedPartDescription"));
                    break;
                case DataGridTypes.AOI:
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("System ID", "SystemID"));
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Inspector", "Inspector"));
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Assy", "Assy"));
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("IDate", "IDate"));
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("SN", "SN"));
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Ref ID", "RefID"));
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Defect Code", "DefectCode"));
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Part Total", "PartTotal"));
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Rec Type", "RecType"));
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Reworked", "Reworked"));
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("PN", "PN"));
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("BrdFail", "BrdFail"));
                    break;
                case DataGridTypes.DEFECTCODES:
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Code", "Code"));
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Description", "Description"));
                    break;
                case DataGridTypes.PREVREPAIRINFO:
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Tech Name", "TechName"));
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Date Submitted", "DateSubmitted"));
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Serial Number", "SerialNumber"));
                    break;
                case DataGridTypes.PARTNUMBERNAME:
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Part Name", "PartName"));
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Part Number", "PartNumber"));
                    break;
                case DataGridTypes.CUSTOMERINFO:
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("RP Number", "RPNumber"));
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Customer Number", "CustomerNumber"));
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Customer Name", "CustomerName"));
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Fills a combobox with data from a list that is sorted internally.
        /// </summary>
        /// <param name="cb">ComboBox to be filled.</param>
        /// <param name="lData">List to fill CB with</param>
        public static void cbFill(this ComboBox cb, List<string> lData)
        {
            lData.Sort();

            for (int i = 0; i < lData.Count; i++)
            {
                if (!cb.Items.Contains(lData[i]))
                    cb.Items.Add(lData[i]);
            }
        }

        /// <summary>
        /// Gets the typically visible content of the given <see cref="Control"/>.
        /// </summary>
        /// <param name="c">Specified Control</param>
        /// <returns>Value of Control</returns>
        public static object GetContent(this Control c)
        {
            if (c is ComboBox cbox)
                return cbox.SelectedValue;
            else if (c is TextBox tbox /*or a SuggestBox since custom conversion*/)
                return tbox.Text;
            else
                return null;
        }

        /// <summary>
        /// Sets the typically visible content of the given <see cref="Control"/>.
        /// </summary>
        /// <param name="c">Specified Control</param>
        /// <param name="val">Value to set</param>
        public static void SetContent(this Control c, object val)
        {
            if (c is ComboBox cbox)
                cbox.SelectedValue = val;
            else if (c is TextBox tbox /*or a SuggestBox since custom conversion*/)
                tbox.Text = val as string;
            else if (c is Label lbl)
                lbl.Content = val as string;
        }

        /// <summary>
        /// Force window to center of current screen.
        /// </summary>
        /// <param name="w">The current calling window</param>
        public static void CenterWindow(this Window w)
        {
            //get the current monitor
            var currentMonitor = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(w).Handle);

            //find out if our app is being scaled by the monitor
            var source = PresentationSource.FromVisual(w);
            double dpiScaling = (source != null && source.CompositionTarget != null ? source.CompositionTarget.TransformFromDevice.M11 : 1);

            //get the available area of the monitor
            Rectangle workArea = currentMonitor.WorkingArea;
            var workAreaWidth = (int)Math.Floor(workArea.Width * dpiScaling);
            var workAreaHeight = (int)Math.Floor(workArea.Height * dpiScaling);

            //move to the centre
            w.Left = (((workAreaWidth - (w.Width * dpiScaling)) / 2) + (workArea.Left * dpiScaling));
            w.Top = (((workAreaHeight - (w.Height * dpiScaling)) / 2) + (workArea.Top * dpiScaling));
        }

        /// <summary>
        /// Does the excel operations for grabbing Reference and Part numbers.
        /// </summary>
        /// <param name="filePath">Path to the Excel file - normally the BoM file.</param>
        /// <param name="progData">Any related progress bar to semi-report operation progress.</param>
        /// <param name="designators">A list of reference and part number designators to give autocompletion.</param>
        public static void DoExcelOperations(string filePath, ProgressBar progData = null, params DesginatorPair[] designators) => DoExcelOperations(filePath, progData, null, designators);

        /// <summary>
        /// Does the excel operations for grabbing Reference and Part numbers.
        /// </summary>
        /// <param name="filePath">Path to the Excel file - normally the BoM file.</param>
        /// <param name="progData">Any related progress bar to semi-report operation progress.</param>
        /// <param name="bomlist">The datagrid to fill with the results, if any.</param>
        /// <param name="designators">A list of reference and part number designators to give autocompletion.</param>
        public static async void DoExcelOperations(string filePath, ProgressBar progData = null, DataGrid bomlist = null, params DesginatorPair[] designators)
        {
            Cache.EmptyExpired();

            try
            {
                await Task.Factory.StartNew(new Action(() =>
                {
                    if (progData != null) progData.Dispatcher.Invoke(() => progData.Visibility = Visibility.Visible);
                    if (string.IsNullOrEmpty(filePath)) {
                        MainWindow.Notify.ShowBalloonTip("Couldn't find BOM File",
                            "The BOM file path was empty! There're no autocomplete or quick links available.\nPlease alert Dex or Jay.", 
                            Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Warning);
                        return;
                    }

                    var (exists, partNumbers, referenceDesignators) = CheckCache(filePath);
                    if (exists) {
                        MainWindow.Notify.ShowBalloonTip($"Using Cached Data for {filePath.Substring(filePath.LastIndexOf('\\') + 1, 8)}",
                            "The data was pulled from the cache.\nIf data is outdated, please notify Jay W.",
                            Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
                        PartNumbers = partNumbers.Split(',').ToList();
                        ReferenceDesignators = referenceDesignators.Split(',').ToList();
                    } else {
                        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                        {
                            using (var reader = ExcelReaderFactory.CreateReader(stream))
                            {
                                while (reader.NextResult() && reader.Name != null && !reader.Name.Equals("JUKI"))
                                { /*spin until JUKI sheet*/ }

                                while (reader.Read() && !string.IsNullOrEmpty(reader.GetValue(0)?.ToString())
                                                     && !string.IsNullOrEmpty(reader.GetValue(4)?.ToString()))
                                {
                                    var rd = reader.GetValue(0).ToString();
                                    var pn = reader.GetValue(4).ToString();
                                    ReferenceDesignators.Add(rd);
                                    PartNumbers.Add(pn);

                                    /*bomlist?.Dispatcher.Invoke(() => {
                                        if (PartNumbers.Contains(pn)) {
                                        }

                                        bomlist.Items.Add(new MultiplePartsReplaced(rd, pn,
                                            pn.Contains("NP") ? "NO PART" : frmProduction.csCrossClassInteraction.getPartReplacedPartDescription(pn)));
                                    });*/
                                }
                            }
                        }
                    }

                    foreach (var designator in designators)
                    {
                        var reference = designator.Item1;
                        var partnumber = designator.Item2;

                        if (reference is ComboBox refbox)
                        {
                            refbox.Dispatcher.Invoke(() => refbox.ItemsSource = ReferenceDesignators);
                        }

                        if (partnumber is ComboBox invbox)
                        {
                            invbox.Dispatcher.Invoke(() => invbox.ItemsSource = PartNumbers);
                        }
                    }

                    UpdateCache("PN", filePath);
                    UpdateCache("RD", filePath);

                    GC.Collect();
                    Thread.Sleep(500);

                    if (progData != null) progData.Dispatcher.Invoke(() => progData.Visibility = Visibility.Hidden);
                }));
            }
            catch (IOException ioe)
            {
                csExceptionLogger.csExceptionLogger.Write("RetestVerifier-MainWindow_ExcelOps-LockedFile", ioe);
            }
        }

        /// <summary>
        /// Checks the <see cref="ApplicationCache"/> for any data related to the <see cref="MultiplePartsReplaced"/> model for the given component number.
        /// </summary>
        /// <param name="filePath">The Excel File path </param>
        private static (bool exists, string partNumbers, string referenceDesignators) CheckCache(string filePath)
        {
            var componentNumber = filePath.Substring(filePath.LastIndexOf('\\') + 1, 8);
            if (!Cache.Exists(componentNumber))
                return (false, string.Empty, string.Empty);
            else
                return (true, Cache.Get<string>(componentNumber + "PN"), Cache.Get<string>(componentNumber + "RD"));
        }

        /// <summary>
        /// Updates the <see cref="ApplicationCache"/> with any data related to the <see cref="MultiplePartsReplaced"/> model for the given component number.
        /// </summary>
        /// <param name="cacheTarget">Either Part Number or Reference Designators</param>
        /// <param name="filePath">The Excel File path </param>
        private static void UpdateCache(string cacheTarget, string filePath)
        {
            var componentNumber = filePath.Substring(filePath.LastIndexOf('\\') + 1, 8);
            if (!Cache.Exists(componentNumber))
                Cache.Add(componentNumber, filePath, TimeSpan.Parse("12:01:00"));
            
            string data = "";
            switch(cacheTarget)
            {
                case "PN":
                    data = PartNumbers.Aggregate((curr,next) => $"{curr},{next}");
                    break;
                case "RD":
                    data = ReferenceDesignators.Aggregate((curr, next) => $"{curr},{next}");
                    break;
                default:
                    Console.WriteLine($"[WARN]: RApID_Project_WPF::csCrossClassInteraction.UpdateCache() with unknown cacheTarget = {cacheTarget}.");
                    return;
            }

            Cache.Add(componentNumber + cacheTarget, data, TimeSpan.FromHours(12.0));            
        }

        /// <summary>
        /// Will use the given part number to find the part's description in the ItemMaster table.
        /// </summary>
        /// <param name="_sPartReplaced">Part Number replaced</param>
        /// <returns>Description or an empty string</returns>
        public static string GetPartReplacedPartDescription(string _sPartReplaced)
        {
            if (string.IsNullOrEmpty(_sPartReplaced))
                return string.Empty;

            string query = "SELECT PartName FROM ItemMaster WHERE PartNumber = '" + _sPartReplaced + "';";
            string sPRPD = ItemMasterQuery(query);
            if (sPRPD == "N/A")
                return string.Empty;
            else return sPRPD;
        }

        /// <summary>
        /// Adds data from a sql query to a combobox's item list.
        /// </summary>
        /// <param name="cbToFill">ComboBox to fill</param>
        /// <param name="_query">SQL Query</param>
        /// <param name="connString">Optional Connection String</param>
        public static void PullItemsFromQuery(this ComboBox cbToFill, string _query, string connString = "")
        {
            sVar.LogHandler.CreateLogAction(string.Format("Attempting to fill {0} from a SQL Query.", cbToFill.Name.ToString()), EricStabileLibrary.csLogging.LogState.NOTE);
            sVar.LogHandler.CreateLogAction("SQL QUERY: " + _query, EricStabileLibrary.csLogging.LogState.SQLQUERY);

            var conn = new SqlConnection(string.IsNullOrEmpty(connString) ? holder.HummingBirdConnectionString : connString);
            var cmd = new SqlCommand(_query, conn);
            try
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader[0] != DBNull.Value && !string.IsNullOrEmpty(reader[0].ToString()))
                        {
                            cbToFill.Items.Add(reader[0].ToString());
                            sVar.LogHandler.CreateLogAction(string.Format("{0} was found and added to {1}.", reader[0].ToString(), cbToFill.Name.ToString()), EricStabileLibrary.csLogging.LogState.NOTE);
                        }
                    }
                }
                conn.Close();
                sVar.LogHandler.CreateLogAction("Query was successful!", EricStabileLibrary.csLogging.LogState.NOTE);
            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();
                string sErr = "There was an issue loading the requested data.\nError Message: " + ex.Message;
                System.Windows.Forms.MessageBox.Show(sErr, "cbFillFromQuery()", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                sVar.LogHandler.CreateLogAction(sErr, EricStabileLibrary.csLogging.LogState.ERROR);
            }
        }

        /// <summary>
        /// Fills a Textbox with information from a sql query.
        /// </summary>
        /// <param name="_query">SQL Query</param>
        /// <param name="txtToFill">TextBox to fill.</param>
        public static void txtFillFromQuery(string _query, TextBox txtToFill)
        {
            sVar.LogHandler.CreateLogAction(string.Format("Attempting to fill {0} from a SQL Query.", txtToFill.Name.ToString()), EricStabileLibrary.csLogging.LogState.NOTE);
            sVar.LogHandler.CreateLogAction("SQL QUERY: " + _query, EricStabileLibrary.csLogging.LogState.SQLQUERY);

            var conn = new SqlConnection(holder.HummingBirdConnectionString);
            var cmd = new SqlCommand(_query, conn);
            try
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader[0] != DBNull.Value)
                        {
                            txtToFill.Text = reader[0].ToString().TrimEnd();
                            sVar.LogHandler.CreateLogAction(string.Format("{0} was found and added to {1}.", reader[0].ToString().TrimEnd(), txtToFill.Name.ToString()), EricStabileLibrary.csLogging.LogState.NOTE);
                            break;
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();

                string sErr = "Error filling textbox.\nError Message: " + ex.Message;
                System.Windows.MessageBox.Show(sErr, "txtFillFromQuery()", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                sVar.LogHandler.CreateLogAction(sErr, EricStabileLibrary.csLogging.LogState.ERROR);
            }
        }

        /// <summary>
        /// Fills a list view with data from a query.
        /// </summary>
        /// <param name="_conn">Type of SQL Connection</param>
        /// <param name="_query">SQL Query</param>
        /// <param name="lsvToFill">List To Fill</param>
        public static void lsvFillFromQuery(string _conn, string _query, ListView lsvToFill)
        {
            var conn = new SqlConnection
            {
                ConnectionString = _conn
            };
            var cmd = new SqlCommand(_query, conn);
            try
            {
                string[] testVal = new string[2];
                lsvToFill.Items.Clear();
                var gv = new GridView();
                lsvToFill.View = gv;
                gv.Columns.Add(new GridViewColumn { Header = "Test", DisplayMemberBinding = new Binding("Test") });
                gv.Columns.Add(new GridViewColumn { Header = "Value", DisplayMemberBinding = new Binding("Value") });
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            if (reader[i] != DBNull.Value)
                            {
                                try
                                {
                                    testVal[0] = reader.GetName(i).ToString();
                                    if (testVal[0] == "DateAndTime")
                                    {
                                        testVal[1] = reader["DateAndTime"].ToString();
                                    }
                                    else
                                    {
                                        testVal[1] = reader[i].ToString();
                                    }
                                    lsvToFill.Items.Add(new EOLLISTVIEW { Test = testVal[0], Value = testVal[1] });
                                }
                                catch { }
                            }
                        }
                    }
                }
                conn.Close();
            }
            catch
            {
                if (conn != null)
                    conn.Close();
            }
        }

        /// <summary>
        /// Fills the dgTechReport with SQL Data
        /// </summary>
        /// <param name="_query">SQL Query</param>
        /// <param name="oldDb">Are we searching the old database?</param>
        /// <param name="dgToFill">DataGrid to fill</param>
        /// <param name="_serialNum">Serial Number we are searching</param>
        public static void dgTechReport(string _query, bool oldDb, DataGrid dgToFill, string _serialNum)
        {
            var conn = new SqlConnection(holder.RepairConnectionString);
            var cmd = new SqlCommand(_query, conn);
            try
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dgToFill.Items.Add(new PreviousRepairInformation
                        {
                            DateSubmitted = Convert.ToDateTime(reader[0]),
                            TechName = reader[1].ToString(),
                            ID = (oldDb ? -1 : Convert.ToInt32(reader[2])),
                            SerialNumber = _serialNum,
                            OldDB = oldDb
                        });
                    }
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();

                string sErr = "";

                if (oldDb)
                    sErr = "Error Loading Previous Tech Report from the old Tech Database.\nError Message: " + ex.Message;
                else sErr = "Error Loading Previous Tech Report from the new Tech Database.\nError Message: " + ex.Message;

                System.Windows.Forms.MessageBox.Show(sErr, "dgTechReport()", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                sVar.LogHandler.CreateLogAction(sErr, EricStabileLibrary.csLogging.LogState.ERROR);
            }
        }

        /// <summary>
        /// Queries the EOL Table for Part Number and Part Name information
        /// </summary>
        /// <param name="query">SQL Query</param>
        /// <param name="_txtPNum">Textbox related to the part number to fill</param>
        /// <param name="_txtPName">Textbox related to the part name to fill</param>
        /// <returns>Yes/No depending if the query was successful or not</returns>
        public static bool SNEOLQuery(string query, TextBox _txtPNum, TextBox _txtPName)
        {
            sVar.LogHandler.CreateLogAction("Attempting to query the EOL Table.", EricStabileLibrary.csLogging.LogState.NOTE);
            sVar.LogHandler.CreateLogAction("SQL QUERY: " + query, EricStabileLibrary.csLogging.LogState.SQLQUERY);

            bool bQueryPassed = false;

            var conn = new SqlConnection(holder.HummingBirdConnectionString);
            var cmd = new SqlCommand(query, conn);
            try
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader[0] != DBNull.Value) { _txtPNum.Text = reader[0].ToString().TrimEnd(); }
                        if (reader[1] != DBNull.Value) { _txtPName.Text = reader[1].ToString().TrimEnd(); }

                        break;
                    }
                }
                conn.Close();

                if (string.IsNullOrEmpty(_txtPName.Text) && string.IsNullOrEmpty(_txtPNum.Text))
                    bQueryPassed = false;
                else
                {
                    sVar.LogHandler.CreateLogAction(string.Format("Found Part Number: {0}, Part Name: {1}", _txtPNum.Text, _txtPName.Text), EricStabileLibrary.csLogging.LogState.NOTE);
                    bQueryPassed = true;
                }
            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();

                string sErr = "Error Querying the EOL table.\n\nError Message: " + ex.Message;
                System.Windows.Forms.MessageBox.Show(sErr, "SNEOLQuery()", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                sVar.LogHandler.CreateLogAction(sErr, EricStabileLibrary.csLogging.LogState.ERROR);

                bQueryPassed = false;
            }
            return bQueryPassed;
        }

        public static string SeriesQuery(string sPN)
        {
            string sReturnSeries = "";
            string query = "SELECT CommoditySubClassDesc FROM ItemMaster WHERE PartNumber = '" + sPN + "';";

            sVar.LogHandler.CreateLogAction("Attempting to query the ItemMaster Table.", EricStabileLibrary.csLogging.LogState.NOTE);
            sVar.LogHandler.CreateLogAction("SQL QUERY: " + query, EricStabileLibrary.csLogging.LogState.SQLQUERY);

            var conn = new SqlConnection(holder.HummingBirdConnectionString);
            var cmd = new SqlCommand(query, conn);
            try
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sReturnSeries = reader[0].ToString().TrimEnd();
                    }
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();
                string sErr = "Error Querying for the series.\n\nError Message: " + ex.Message;
                System.Windows.Forms.MessageBox.Show(sErr, "SeriesQuery()", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }

            if (!string.IsNullOrEmpty(sReturnSeries))
                sReturnSeries = StripSeries(sReturnSeries);

            return sReturnSeries;
        }

        /// <summary>
        /// Strip the series of unwanted chars
        /// </summary>
        private static string StripSeries(string s)
        {
            string sReturnStrip = "";

            string[] splitters = { "HB", "(" };
            string[] sSplit = s.Split(splitters, StringSplitOptions.RemoveEmptyEntries);
            sReturnStrip = sSplit[0].ToString().Trim();

            return sReturnStrip;
        }

        /// <summary>
        /// Queries the production table for a Part Number
        /// </summary>
        public static string ProductionQuery(string _query)
        {
            sVar.LogHandler.CreateLogAction("Attempting to query the Production table for the Part Number.", EricStabileLibrary.csLogging.LogState.NOTE);
            sVar.LogHandler.CreateLogAction("SQL QUERY: " + _query, EricStabileLibrary.csLogging.LogState.SQLQUERY);
            string sReturn = "";

            var conn = new SqlConnection(holder.HummingBirdConnectionString);
            var cmd = new SqlCommand(_query, conn);
            try
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader[0] != DBNull.Value)
                        {
                            sReturn = reader[0].ToString();
                            break;
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();

                System.Windows.Forms.MessageBox.Show("Error with Production Query.\nError Message: " + ex.Message);
            }

            return sReturn;
        }

        /// <summary>
        /// Queries the Item Master table for a Part Name.
        /// </summary>
        public static string ItemMasterQuery(string _query)
        {
            sVar.LogHandler.CreateLogAction("Attempting to query the ItemMaster table for the Part Name.", EricStabileLibrary.csLogging.LogState.NOTE);
            sVar.LogHandler.CreateLogAction("SQL QUERY: " + _query, EricStabileLibrary.csLogging.LogState.SQLQUERY);
            string sReturn = "";

            var conn = new SqlConnection(holder.HummingBirdConnectionString);
            var cmd = new SqlCommand(_query, conn);
            try
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        sReturn = "N/A";
                        sVar.LogHandler.CreateLogAction("No information was found related to this query.", EricStabileLibrary.csLogging.LogState.NOTE);
                    }
                    while (reader.Read())
                    {
                        if (reader[0] != DBNull.Value)
                        {
                            sReturn = reader[0].ToString().TrimEnd();
                            sVar.LogHandler.CreateLogAction(string.Format("{0} was found in the ItemMaster database.", sReturn), EricStabileLibrary.csLogging.LogState.NOTE);
                            break;
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();
                string sErr = "Error querying the ItemMaster Database.\nError Message: " + ex.Message;
                System.Windows.Forms.MessageBox.Show(sErr, "ItemMasterQuery()", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                sVar.LogHandler.CreateLogAction(sErr, EricStabileLibrary.csLogging.LogState.ERROR);
            }
            return sReturn;
        }

        /// <summary>
        /// Queries the AOI table for relevant AOI data related to the serial number.
        /// </summary>
        public static void AOIQuery(DataGrid _dgAOI, DataGrid _dgDefect, string _sSN)
        {
            var lDefectCode = new List<string>();

            string query = "SELECT * FROM SPC_Data WHERE SN = '" + _sSN + "';";

            sVar.LogHandler.CreateLogAction("Attemping to query the AOI Table.", EricStabileLibrary.csLogging.LogState.NOTE);
            sVar.LogHandler.CreateLogAction("SQL QUERY: " + query, EricStabileLibrary.csLogging.LogState.SQLQUERY);

            var conn = new SqlConnection(holder.YesDBConnectionString);
            var cmd = new SqlCommand(query, conn);
            try
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        _dgAOI.Items.Add(new DGVAOI
                        {
                            SystemID = reader["SystemID"].ToString(),
                            Inspector = reader["Inspector"].ToString(),
                            Assy = reader["Assy"].ToString(),
                            IDate = (string.IsNullOrEmpty(reader["IDate"].ToString()) ? Convert.ToDateTime("1/1/1111") : Convert.ToDateTime(reader["IDate"])),
                            SN = reader["SN"].ToString(),
                            RefID = reader["RefID"].ToString(),
                            DefectCode = reader["DefectCode"].ToString(),
                            PartTotal = reader["PartTotal"].ToString(),
                            RecType = reader["RecType"].ToString(),
                            Reworked = reader["Reworked"].ToString(),
                            PN = reader["PN"].ToString(),
                            BrdFail = reader["BrdFail"].ToString()
                        });

                        string sAOI = "The following AOI Data was found: \n" +
                                       "SystemID = " + reader["SystemID"].ToString() + "\n" +
                                       "Inspector = " + reader["Inspector"].ToString() + "\n" +
                                       "Assy = " + reader["Assy"].ToString() + "\n" +
                                       "IDate = " + (string.IsNullOrEmpty(reader["IDate"].ToString()) ? Convert.ToDateTime("1/1/1111") : Convert.ToDateTime(reader["IDate"])) + "\n" +
                                       "SN = " + reader["SN"].ToString() + "\n" +
                                       "RefID = " + reader["RefID"].ToString() + "\n" +
                                       "DefectCode = " + reader["DefectCode"].ToString() + "\n" +
                                       "PartTotal = " + reader["PartTotal"].ToString() + "\n" +
                                       "RecType = " + reader["RecType"].ToString() + "\n" +
                                       "Reworked = " + reader["Reworked"].ToString() + "\n" +
                                       "PN = " + reader["PN"].ToString() + "\n" +
                                       "BrdFail = " + reader["BrdFail"].ToString();

                        sVar.LogHandler.CreateLogAction(sAOI, EricStabileLibrary.csLogging.LogState.NOTE);

                        if (!lDefectCode.Contains(reader["DefectCode"].ToString()))
                        {
                            lDefectCode.Add(reader["DefectCode"].ToString());
                            sVar.LogHandler.CreateLogAction(string.Format("{0} was added to the DefectCode List.", reader["DefectCode"].ToString()), EricStabileLibrary.csLogging.LogState.NOTE);
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();
                string sErr = "Error querying AOI data.\nError Message: " + ex.Message;
                System.Windows.Forms.MessageBox.Show(sErr, "AOIQuery()", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                sVar.LogHandler.CreateLogAction(sErr, EricStabileLibrary.csLogging.LogState.ERROR);
            }

            if (lDefectCode.Count > 0)
            {
                dgFillDefectCode(_dgDefect, lDefectCode);
            }
        }

        /// <summary>
        /// Checks for a technician submission ID.
        /// </summary>
        /// <param name="_query"></param>
        /// <returns></returns>
        public static int GetDBIDValue(string _query)
        {
            int rID = -1;

            var conn = new SqlConnection(holder.RepairConnectionString);
            var cmd = new SqlCommand(_query, conn);
            try
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rID = Convert.ToInt32(reader[0].ToString());
                        break;
                    }
                }
                conn.Close();
            }
            catch
            {
                if (conn != null)
                    conn.Close();

                rID = -1;
            }

            return rID;
        }

        /// <summary>
        /// Fills a DataGrid with relevant defect codes.
        /// </summary>
        private static void dgFillDefectCode(DataGrid _dgDef, List<string> lDefCode)
        {
            string query = "SELECT Code, Description FROM DefectCode WHERE ";

            for (int i = 0; i < lDefCode.Count; i++)
            {
                if (i == lDefCode.Count - 1)
                    query += "CODE = '" + lDefCode[i].ToString() + "';";
                else query += "CODE = '" + lDefCode[i].ToString() + "' OR ";
            }

            sVar.LogHandler.CreateLogAction("Attempting to query the DefectCode table.", EricStabileLibrary.csLogging.LogState.NOTE);
            sVar.LogHandler.CreateLogAction("SQL QUERY: " + query, EricStabileLibrary.csLogging.LogState.SQLQUERY);

            var conn = new SqlConnection(holder.YesDBConnectionString);
            var cmd = new SqlCommand(query, conn);
            try
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        _dgDef.Items.Add(new DGVDEFECTCODES { Code = reader[0].ToString(), Description = reader[1].ToString() });
                        string sDefect = "The follow Defect Data was found:\n" +
                                         "Code: " + reader[0].ToString() + "\n" +
                                         "Description: " + reader[1].ToString();
                        sVar.LogHandler.CreateLogAction(sDefect, EricStabileLibrary.csLogging.LogState.NOTE);
                    }
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();
                string sErr = "Error querying the DefectCode table.\nError Message: " + ex.Message;
                System.Windows.Forms.MessageBox.Show(sErr, "dgFillDefectCode()", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                sVar.LogHandler.CreateLogAction(sErr, EricStabileLibrary.csLogging.LogState.ERROR);
            }
        }

        /// <summary>
        /// Queries the Beams table for information relevant to the serial number.
        /// </summary>
        public static void BeamsQuery(string _sn, ComboBox cbBeams, ListView lsvToClear, bool isXDR = false) 
        {
            cbBeams.Items.Clear();
            lsvToClear.Items.Clear();

            bool bFoundSN = false;

            var _query = "";
            if (isXDR) _query = $"SELECT SerialNumber FROM tblXducerTestResultsBenchTest WHERE SerialNumber = '{_sn}';";
            else _query = "SELECT PCBSerial FROM Beams WHERE PCBSerial = '" + _sn + "';";

            sVar.LogHandler.CreateLogAction("Attempting to query the Beams table to see if the Serial Number exists.", EricStabileLibrary.csLogging.LogState.NOTE);
            sVar.LogHandler.CreateLogAction("SQL QUERY: " + _query, EricStabileLibrary.csLogging.LogState.SQLQUERY);

            var conn = new SqlConnection(holder.HummingBirdConnectionString);
            var cmd = new SqlCommand(_query, conn);
            try
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader[0].ToString().Equals(_sn))
                        {
                            bFoundSN = true;
                            break;
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();
                string sErr = "Error loading beam data.\nError Message: " + ex.Message;
                System.Windows.Forms.MessageBox.Show(sErr, "BeamsQuery()", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                sVar.LogHandler.CreateLogAction(sErr, EricStabileLibrary.csLogging.LogState.ERROR);
            }

            if (bFoundSN)
            {
                var hBeams = new HashSet<string>();
                var lBeams = new List<string>();
                if (isXDR) _query = $"SELECT BeamNumber FROM tblXducerTestResults WHERE SerialNumber = '{_sn}' ORDER BY BeamNumber ASC;";
                else _query = "SELECT BeamNumber FROM Beams WHERE PCBSerial = '" + _sn + "' ORDER BY BeamNumber ASC;";
                cmd = new SqlCommand(_query, conn);
                try
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader[0] != DBNull.Value)
                            {
                                if (!lBeams.Contains(reader[0].ToString()))
                                    lBeams.Add(reader[0].ToString());
                            }
                        }
                    }
                    conn.Close();

                    for (int i = 0; i < lBeams.Count; i++)
                    {
                        cbBeams.Items.Add("Beam " + lBeams[i]);
                    }
                }
                catch (Exception)
                {
                    if (conn != null)
                        conn.Close();
                    sVar.LogHandler.CreateLogAction("The Beam # could not be found.", EricStabileLibrary.csLogging.LogState.ERROR);
                }

            }
            else sVar.LogHandler.CreateLogAction("The Serial Number does not exist in the beams table.", EricStabileLibrary.csLogging.LogState.NOTE);

        }

        public static void BeamsQuery(string _query, ListView lsvToFill, string serialName = "PCBSerial")
        {
            var conn = new SqlConnection(holder.HummingBirdConnectionString);
            var cmd = new SqlCommand(_query, conn);
            try
            {
                string[] testVal = new string[2];
                lsvToFill.Items.Clear();
                var gv = new GridView();
                lsvToFill.View = gv;
                gv.Columns.Add(new GridViewColumn { Header = "Test", DisplayMemberBinding = new Binding("Test") });
                gv.Columns.Add(new GridViewColumn { Header = "Value", DisplayMemberBinding = new Binding("Value") });
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            if (reader[i] != DBNull.Value)
                            {
                                try
                                {
                                    testVal[0] = reader.GetName(i).ToString();
                                    if (testVal[0].Equals("TestID") || testVal[0].Equals(serialName) || testVal[0].Equals("BeamNumber") || testVal[0].Equals("TestType") || testVal[0].Equals("BeamKey")) { }//ignore these values
                                    else
                                    {
                                        testVal[1] = reader[i].ToString();
                                        lsvToFill.Items.Add(new EOLLISTVIEW { Test = testVal[0], Value = testVal[1] });
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                }
                conn.Close();
            }
            catch
            {
                if (conn != null)
                    conn.Close();
            }
        }

        /// <summary>
        /// Returns the beam number of a specific beam.
        /// </summary>
        public static string GetSpecificBeamNumber(string s)
        {
            string[] splitters = { "Beam " };
            string[] sSplit = s.Split(splitters, StringSplitOptions.RemoveEmptyEntries);
            return sSplit[0];
        }

        public static void FillBeamTestIDFromType(ComboBox cbToRead, ComboBox cbToFill, ListView lsvToClear, ComboBox cbToClear)
        {
            lsvToClear.Items.Clear();
            cbToFill.Items.Clear();
            cbToClear.Items.Clear();

            for (int i = 0; i < cbToRead.Items.Count; i++)
                cbToFill.Items.Add(cbToRead.Items[i].ToString());
        }

        public static void LoadPartNumberForm(bool bProduction, List<TextBox> lTB)
        {
            var fpn = new frmPartNumber(bProduction);
            try
            {
                MaterialDesignThemes.Wpf.DialogHost.Show(fpn);
            } catch(Exception e)
            {
                csExceptionLogger.csExceptionLogger.Write("NoDialogHostFound", e);
                fpn.ShowDialog();
            }
            if (sVar.SelectedPartNumberPartName.PartNumberSelected)
            {
                lTB[0].Text = sVar.SelectedPartNumberPartName.PartNumber;
                lTB[1].Text = sVar.SelectedPartNumberPartName.PartName;
                lTB[2].Text = sVar.SelectedPartNumberPartName.PartSeries;
            }
        }

        /// <summary>
        /// Checks to see if the string passed in is null or equal to <see cref="DBNull"/>.
        /// </summary>
        /// <param name="valToTest">Value that needs to be checked.</param>
        /// <returns>Returns String.Empty if the value is null or DBNull; otherwise, return the value passed in.</returns>
        public static string EmptyIfNull(this string valToTest) 
            => string.IsNullOrEmpty(valToTest) || ((object)valToTest) == DBNull.Value ? string.Empty : valToTest;

        public static string unitIssuesValSubmit(ComboBox cbToCheck)
        {
            return string.IsNullOrEmpty(cbToCheck.Text) ? "NF, " : cbToCheck.Text + ", ";
        }

        public static string unitIssuesValSubmit(TextBox txtToCheck)
        {
            return string.IsNullOrEmpty(txtToCheck.Text) ? "NF, " : txtToCheck.Text + ", ";
        }

        public static string unitIssuesValSubmit(string strToCheck)
        {
            return string.IsNullOrEmpty(strToCheck) ? "NF, " : strToCheck + ", ";
        }

        public static string unitStripNF(string strToCheck)
        {
            return strToCheck.Equals("NF") ? string.Empty : strToCheck;
        }
        /* ---- END ---- */

        /// <summary>
        /// Helps determine if the ref designator is valid.
        /// </summary>
        public static void checkForValidRefDes(this TextBox txtToCheck)
        {
            TextBox t = txtToCheck;
            string sRefCheck = t.Text.ToString().ToUpper();
            int iLetterCount = 0;
            for (int i = 0; i < sRefCheck.Length; i++)
            {
                iLetterCount += alphabetTestCount(sRefCheck[i]);
            }

#if DEBUG
            Console.WriteLine("Number of Letters: " + iLetterCount.ToString());
#endif

            while (iLetterCount > 1)
            {
                sRefCheck = sRefCheck.Remove(sRefCheck.Length - 1);
                iLetterCount--;
            }

            t.Text = sRefCheck;
            t.CaretIndex = t.Text.Length;
        }

        private static int alphabetTestCount(char _c)
        {
            for (char c = 'A'; c <= 'Z'; c++)
            {
                if (_c == c)
                {
                    return 1;
                }
            }
            return 0;
        }

        /// <summary>
        /// Combines all of the multiple parts replaced items in a datagrid into a single object.
        /// This is used for the main database submission as a reference.
        /// </summary>
        public static MultiplePartsReplaced getMPRString(DataGrid _dg, MultiplePartsReplaced mpr)
        {
            MultiplePartsReplaced mpReturn = mpr;
            for (int i = 0; i < _dg.Items.Count; i++)
            {
                var dgmpr = (MultiplePartsReplaced)_dg.Items[i];
                mpReturn.PartReplaced += unitIssuesValSubmit(dgmpr.PartReplaced);
                mpReturn.RefDesignator += unitIssuesValSubmit(dgmpr.RefDesignator);
                mpReturn.PartsReplacedPartDescription += unitIssuesValSubmit(dgmpr.PartsReplacedPartDescription);
            }

            if (!string.IsNullOrEmpty(mpReturn.PartReplaced))
                mpReturn.PartReplaced = mpReturn.PartReplaced.TrimEnd(new char[] { ',', ' ' });

            if (!string.IsNullOrEmpty(mpReturn.RefDesignator))
                mpReturn.RefDesignator = mpReturn.RefDesignator.TrimEnd(new char[] { ',', ' ' });

            if (!string.IsNullOrEmpty(mpReturn.PartsReplacedPartDescription))
                mpReturn.PartsReplacedPartDescription = mpReturn.PartsReplacedPartDescription.TrimEnd(new char[] { ',', ' ' });

            return mpr;
        }

        /// <summary>
        /// Returns a list of MultiplePartsReplaced objects from a datagrid.
        /// </summary>
        public static List<MultiplePartsReplaced> getMPRList(DataGrid _dg)
        {
            var lmpReturn = new List<MultiplePartsReplaced>();

            for (int i = 0; i < _dg.Items.Count; i++)
            {
                var dgmpr = (MultiplePartsReplaced)_dg.Items[i];
                dgmpr.PartReplaced = unitIssuesValSubmit(dgmpr.PartReplaced).TrimEnd(new char[] { ',', ' ' });
                dgmpr.RefDesignator = unitIssuesValSubmit(dgmpr.RefDesignator).TrimEnd(new char[] { ',', ' ' });
                dgmpr.PartsReplacedPartDescription = unitIssuesValSubmit(dgmpr.PartsReplacedPartDescription).TrimEnd(new char[] { ',', ' ' });
                lmpReturn.Add((MultiplePartsReplaced)_dg.Items[i]);
            }

            return lmpReturn;
        }

        /// <summary>
        /// Queries the RepairCustomerInformation table to grab the full customer information based on the Customer Number
        /// </summary>
        /// <param name="CustomerNumber"></param>
        /// <returns></returns>
        public static CustomerInformation CustomerInformationQuery(string CustomerNumber)
        {
            var _ciReturn = new CustomerInformation();
            _ciReturn.CustomerNumber = CustomerNumber;

            string query = "SELECT * FROM RepairCustomerInformation WHERE CustomerNumber = '" + CustomerNumber + "'";
            var conn = new SqlConnection(holder.RepairConnectionString);
            var cmd = new SqlCommand(query, conn);

            try
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        _ciReturn.CustomerName = reader["CustomerName"].ToString();
                        _ciReturn.CustomerAddy1 = reader["CustomerAddressLine1"].ToString();
                        _ciReturn.CustomerAddy2 = reader["CustomerAddressLine2"].ToString();
                        _ciReturn.CustomerAddy3 = reader["CustomerAddressLine3"].ToString();
                        _ciReturn.CustomerAddy4 = reader["CustomerAddressLine4"].ToString();
                        _ciReturn.CustomerCity = reader["CustomerCity"].ToString();
                        _ciReturn.CustomerState = reader["CustomerState"].ToString();
                        _ciReturn.CustomerPostalCode = reader["CustomerPostalCode"].ToString();
                        _ciReturn.CustomerCountryCode = reader["CustomerCountryCode"].ToString();
                        break;
                    }
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();
                System.Windows.Forms.MessageBox.Show("Error loading the Customer Information.\nError Message: " + ex.Message, "Customer Information Load Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }


            return _ciReturn;
        }

        private static List<UnitIssueModel> breakdownRepairIssues(List<UnitIssueModel> _lrmi)
        {
            var lCombinedRMI = new List<UnitIssueModel>();
            var dMultiParts = new Dictionary<int, List<UnitIssueModel>>();
            var lIndividualIssues = new List<UnitIssueModel>();

#region Split the overall list into two separate list: 1 with individual unit issues, 2 with same unit issue but different parts replaced/ref des
            int key = 0;
            UnitIssueModel rmi = null;
            for (int i = 0; i < _lrmi.Count; i++)
            {
                key++;
                if (_lrmi.Count.Equals(1))
                {
                    lIndividualIssues.Add(_lrmi[i]);
                    _lrmi.Remove(_lrmi[i]);
                    break;
                }

                bool bAddComparisonRMI = false;
                rmi = _lrmi[i];
                dMultiParts[key] = new List<UnitIssueModel>();
                for (int j = 1; j < _lrmi.Count; j++)
                {
                    UnitIssueModel _cRMI = _lrmi[j];

                    if (rmi.ReportedIssue == _cRMI.ReportedIssue &&
                         rmi.TestResult == _cRMI.TestResult &&
                         rmi.TestResultAbort == _cRMI.TestResultAbort &&
                         rmi.Cause == _cRMI.Cause &&
                         rmi.Replacement == _cRMI.Replacement)
                    {
                        bAddComparisonRMI = true;
                        dMultiParts[key].Add(_lrmi[j]);
                        _lrmi.Remove(_lrmi[j]);
                        j = 1;
                        continue;
                    }
                }

                if (bAddComparisonRMI)
                {
                    dMultiParts[key].Add(_lrmi[i]);
                    _lrmi.Remove(_lrmi[i]);
                    i--;
                }
                else
                {
                    lIndividualIssues.Add(_lrmi[i]);
                    _lrmi.Remove(_lrmi[i]);
                    i--;
                }
            }
#endregion

#region Add individual unit issues to the combined list
            if (lIndividualIssues.Count > 0)
                lCombinedRMI.AddRange(lIndividualIssues);
#endregion

#region Combine all of the duplicated items by generating lists of parts replaced and ref designators for use in one unit issue
            if (dMultiParts.Keys.Count > 0)
            {
                foreach (KeyValuePair<int, List<UnitIssueModel>> kvp in dMultiParts)
                {
                    if (kvp.Value.Count > 0)
                    {
                        var _rmiCombine = new UnitIssueModel();

                        for (int i = 0; i < kvp.Value.Count; i++)
                        {
                            if (i == 0)
                            {
                                _rmiCombine.ReportedIssue = kvp.Value[i].ReportedIssue;
                                _rmiCombine.TestResult = kvp.Value[i].TestResult;
                                _rmiCombine.TestResultAbort = kvp.Value[i].TestResultAbort;
                                _rmiCombine.Cause = kvp.Value[i].Cause;
                                _rmiCombine.Replacement = kvp.Value[i].Replacement;
                                _rmiCombine.MultiPartsReplaced = new List<MultiplePartsReplaced>
                                {
                                    kvp.Value[i].SinglePartReplaced
                                };
                            }
                            else
                            {
                                _rmiCombine.MultiPartsReplaced.Add(kvp.Value[i].SinglePartReplaced);
                            }
                        }
                        lCombinedRMI.Add(_rmiCombine);
                    }
                }
            }
#endregion

            return lCombinedRMI;
        }

        /// <summary>
        /// Returns a list of UnitIssueModel based off of an ID
        /// </summary>
        public static List<UnitIssueModel> GetRepairUnitIssues(string ID)
        {
            var lRMI = new List<UnitIssueModel>();

            string query = "SELECT * FROM TechnicianUnitIssues WHERE ID = '" + ID + "'";
            var conn = new SqlConnection(holder.RepairConnectionString);
            var cmd = new SqlCommand(query, conn);

            try
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lRMI.Add(new UnitIssueModel()
                        {
                            ID = int.TryParse(reader["ID"].ToString().EmptyIfNull(), out int _id) ? _id : 0,
                            ReportedIssue = reader["ReportedIssue"].ToString().EmptyIfNull(),
                            TestResult = reader["TestResult"].ToString().EmptyIfNull(),
                            TestResultAbort = reader["TestResultAbort"].ToString().EmptyIfNull(),
                            Cause = reader["Cause"].ToString().EmptyIfNull(),
                            Replacement = reader["Replacement"].ToString().EmptyIfNull(),
                            SinglePartReplaced = new MultiplePartsReplaced()
                            {
                                PartReplaced = reader["PartsReplaced"].ToString().EmptyIfNull(),
                                RefDesignator = reader["RefDesignator"].ToString().EmptyIfNull(),
                                PartsReplacedPartDescription = reader["PartsReplacedPartDescription"].ToString().EmptyIfNull()
                            }
                        });
                    }
                }
                conn.Close();

                if (lRMI.Count <= 1)
                    return lRMI;
                else
                {
                    return breakdownRepairIssues(lRMI);
                }

            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();

                System.Windows.Forms.MessageBox.Show("Error loading the Repair Unit Issues.\nError Message: " + ex.Message, "Repair Unit Issues Load Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

                return null;
            }
        }

        /// <summary>
        /// Redirection method for selecting all the text in a Combobox.
        /// </summary>
        /// <param name="cbox">Target Combobox</param>
        public static void SelectAll(this ComboBox cbox)
        {
            try
            {
                (cbox.Template.FindName("PART_EditableTextBox", cbox) as TextBox).SelectAll();
            } catch(Exception e)
            {
                csExceptionLogger.csExceptionLogger.Write("ComboBox_SelectAll", e);

                try
                {
                    cbox.Focus();                    
                    cbox.RaiseEvent(new RoutedEventArgs(Control.MouseDoubleClickEvent));
                } catch(Exception ie)
                {
                    csExceptionLogger.csExceptionLogger.Write("ComboBox_SelectFail", ie);
                }
            }
        }

        /// <summary>
        /// Generates a random array of characters. 
        /// </summary>
        /// <param name="length">[Optional] defaults to ^ characters.</param>
        /// <remarks>From: https://stackoverflow.com/a/1344258/7476183</remarks>
        public static string GenerateRandomString(int length = 8)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }

        /// <summary>
        /// Fills the specified <see cref="ucUnitIssue"/> with the data from the given <see cref="string"/>array.
        /// </summary>
        /// <param name="issue">The target <see cref="ucUnitIssue"/> to fill with data.</param>
        /// <param name="values"><see cref="string"/> values ordered by visual appearance in the control.</param>
        public static void FillUnitIssue(this ucUnitIssue issue, params string[] values)
        {
            var vi = 0;

            issue.ReportedIssue = values[vi++];
            issue.TestResult = values[vi++];
            issue.AbortResult = values[vi++];
            issue.Issue = values[vi++];
            issue.Cause = values[vi++];
            issue.Replacement = values[vi++];
            issue.Item = values[vi++];
            issue.Problem = values[vi++];

            while (vi < values.Length && values[vi] != null)
            {
                issue.PartsReplaced = new List<MultiplePartsReplaced> {
                    new MultiplePartsReplaced(values[vi++], values[vi++], values[vi++] ?? csCrossClassInteraction.GetPartReplacedPartDescription(values[vi-2]))
                };
            }
        }

        /// <summary>
        /// Removes a child element from its parent visual continer.
        /// </summary>
        /// <param name="parent">Invoking visual container</param>
        /// <param name="child"><see cref="UIElement"/> to remove</param>
        public static void RemoveChild(this DependencyObject parent, UIElement child)
        {
            if (parent is Panel panel) {
                panel.Children.Remove(child);
            } else if (parent is Decorator decorator) {
                if (decorator.Child == child) decorator.Child = null;
            } else if (parent is ContentPresenter contentPresenter) {
                if (contentPresenter.Content == child) contentPresenter.Content = null;
            } else if (parent is ContentControl contentControl) {
                if (contentControl.Content == child) contentControl.Content = null;
            }
        }

        /// <summary>
        /// Makes a copy of the given <see cref="ucUnitIssue"/>
        /// </summary>
        /// <param name="orig">The original <see cref="ucUnitIssue"/></param>
        /// <returns>New <see cref="ucUnitIssue"/> instance.</returns>
        public static ucUnitIssue Copy(this ucUnitIssue orig)
        {
            var u = new ucUnitIssue()
            {
                ReportedIssue = orig.ReportedIssue,
                TestResult = orig.TestResult,
                AbortResult = orig.AbortResult,
                Issue = orig.Issue,
                Item = orig.Item,
                Problem = orig.Problem,
                Cause = orig.Cause,
                Replacement = orig.Replacement
            };

            return u;
        }
    }
}