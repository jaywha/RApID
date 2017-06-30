/*
 * csCrossClassInteraction.cs: Used when the same class/function/method is needed across multiple forms.
 * Created By: Eric Stabile
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Data.SqlClient;
using System.Windows.Data;

namespace RApID_Project_WPF
{
    class csCrossClassInteraction
    {
        private static StaticVars sVar = StaticVars.StaticVarsInstance();
        
        /// <summary>
        /// Takes an empy datagrid and fills it with the appropriate columns based on the criteria.
        /// This is used in Production and Repair.
        /// </summary>
        /// <param name="dgToBuid"></param>
        /// <param name="sType">"MULTIPLEPARTS", "AOI", "DEFECTCODES", "PREVREPAIRINFO", "PARTNUMBERNAME", "CUSTOMERINFO"</param>
        public static void dgBuildView(DataGrid dgToBuid, string sType)
        {
            switch(sType)
            {
                case "MULTIPLEPARTS":
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Ref Designator", "RefDesignator"));
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Part Replaced", "PartReplaced"));
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Part Description", "PartsReplacedPartDescription"));
                    break;
                case "AOI":
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Shift", "Shift"));
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
                case "DEFECTCODES":
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Code", "Code"));
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Description", "Description"));
                    break;
                case "PREVREPAIRINFO":
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Tech Name", "TechName"));
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Date Submitted", "DateSubmitted"));
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Serial Number", "SerialNumber"));
                    break;
                case "PARTNUMBERNAME":
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Part Name", "PartName"));
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Part Number", "PartNumber"));
                    break;
                case "CUSTOMERINFO":
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("RP Number", "RPNumber"));
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Customer Number", "CustomerNumber"));
                    dgToBuid.Columns.Add(DataGridViewHelper.newColumn("Customer Name", "CustomerName"));
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Takes an unsorted list and sorts it.
        /// </summary>
        /// <param name="lToSort">List to be sorted</param>
        /// <returns>Sorted List</returns>
        public static List<string> lSortList(List<string> lToSort)
        {
            List<string> lReturn = lToSort;
            lReturn.Sort((a, b) => a.CompareTo(b));
            return lReturn;
        }

        /// <summary>
        /// Fills a combobox with data from a list.
        /// </summary>
        /// <param name="cb">ComboBox to be filled.</param>
        /// <param name="lData">List to fill CB with</param>
        public static void cbFill(ComboBox cb, List<string> lData)
        {
            for(int i = 0; i < lData.Count; i++)
            {
                if (!cb.Items.Contains(lData[i]))
                    cb.Items.Add(lData[i]);
            }
        }

        /// <summary>
        /// Fills a combobox with data from a sql query.
        /// </summary>
        /// <param name="cbToFill">ComboBox to fill</param>
        /// <param name="_query">SQL Query</param>
        public static void cbFillFromQuery(ComboBox cbToFill, string _query)
        {
            sVar.LogHandler.CreateLogAction(String.Format("Attempting to fill {0} from a SQL Query.", cbToFill.Name.ToString()), EricStabileLibrary.csLogging.LogState.NOTE);
            sVar.LogHandler.CreateLogAction("SQL QUERY: " + _query, EricStabileLibrary.csLogging.LogState.SQLQUERY);

            SqlConnection conn = new SqlConnection(Properties.Settings.Default.HBConn);
            SqlCommand cmd = new SqlCommand(_query, conn);
            try
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        if (reader[0] != DBNull.Value)
                        {
                            cbToFill.Items.Add(reader[0].ToString());
                            sVar.LogHandler.CreateLogAction(String.Format("{0} was found and added to {1}.", reader[0].ToString(), cbToFill.Name.ToString()), EricStabileLibrary.csLogging.LogState.NOTE);
                        }
                    }
                }
                conn.Close();
                sVar.LogHandler.CreateLogAction("Query was successful!", EricStabileLibrary.csLogging.LogState.NOTE);
            }
            catch(Exception ex)
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
            sVar.LogHandler.CreateLogAction(String.Format("Attempting to fill {0} from a SQL Query.", txtToFill.Name.ToString()), EricStabileLibrary.csLogging.LogState.NOTE);
            sVar.LogHandler.CreateLogAction("SQL QUERY: " + _query, EricStabileLibrary.csLogging.LogState.SQLQUERY);
            
            SqlConnection conn = new SqlConnection(Properties.Settings.Default.HBConn);
            SqlCommand cmd = new SqlCommand(_query, conn);
            try
            {
                conn.Open();
                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        if (reader[0] != DBNull.Value)
                        {
                            txtToFill.Text = reader[0].ToString().TrimEnd();
                            sVar.LogHandler.CreateLogAction(String.Format("{0} was found and added to {1}.", reader[0].ToString().TrimEnd(), txtToFill.Name.ToString()), EricStabileLibrary.csLogging.LogState.NOTE);
                            break;
                        }
                    }
                }
                conn.Close();
            }
            catch(Exception ex)
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
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = _conn;
            SqlCommand cmd = new SqlCommand(_query, conn);
            try
            {
                string[] testVal = new string[2];
                lsvToFill.Items.Clear();
                GridView gv = new GridView();
                lsvToFill.View = gv;
                gv.Columns.Add(new GridViewColumn { Header = "Test", DisplayMemberBinding = new Binding("Test") });
                gv.Columns.Add(new GridViewColumn { Header = "Value", DisplayMemberBinding = new Binding("Value") });
                conn.Open();
                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        for(int i = 0; i < reader.FieldCount; i++)
                        {
                            if(reader[i] != DBNull.Value)
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
            SqlConnection conn = new SqlConnection(Properties.Settings.Default.RepairConn);
            SqlCommand cmd = new SqlCommand(_query, conn);
            try
            {
                conn.Open();
                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        PreviousRepairInformation pri = new PreviousRepairInformation();
                        pri.DateSubmitted = Convert.ToDateTime(reader[0]);
                        pri.TechName = reader[1].ToString();
                        if (oldDb)
                            pri.ID = -1;
                        else pri.ID = Convert.ToInt32(reader[2]);
                        pri.SerialNumber = _serialNum;
                        pri.OldDB = oldDb;
                        dgToFill.Items.Add(pri);
                    }
                }
                conn.Close();
            }
            catch(Exception ex)
            {
                if (conn != null)
                    conn.Close();

                string sErr = "";

                if(oldDb)
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

            SqlConnection conn = new SqlConnection(Properties.Settings.Default.HBConn);
            SqlCommand cmd = new SqlCommand(query, conn);
            try
            {
                conn.Open();
                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        if (reader[0] != DBNull.Value) { _txtPNum.Text = reader[0].ToString().TrimEnd(); }
                        if (reader[1] != DBNull.Value) { _txtPName.Text = reader[1].ToString().TrimEnd(); }
                        
                        break;
                    }
                }
                conn.Close();

                if (String.IsNullOrEmpty(_txtPName.Text) && String.IsNullOrEmpty(_txtPNum.Text))
                    bQueryPassed = false;
                else
                {
                    sVar.LogHandler.CreateLogAction(String.Format("Found Part Number: {0}, Part Name: {1}", _txtPNum.Text, _txtPName.Text), EricStabileLibrary.csLogging.LogState.NOTE);
                    bQueryPassed = true;
                }
            }
            catch(Exception ex)
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

            SqlConnection conn = new SqlConnection(Properties.Settings.Default.HBConn);
            SqlCommand cmd = new SqlCommand(query, conn);
            try
            {
                conn.Open();
                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        sReturnSeries = reader[0].ToString().TrimEnd();
                    }
                }
                conn.Close();
            }
            catch(Exception ex)
            {
                if (conn != null)
                    conn.Close();
                string sErr = "Error Querying for the series.\n\nError Message: " + ex.Message;
                System.Windows.Forms.MessageBox.Show(sErr, "SeriesQuery()", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }

            if (!String.IsNullOrEmpty(sReturnSeries))
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
            
            SqlConnection conn = new SqlConnection(Properties.Settings.Default.HBConn);
            SqlCommand cmd = new SqlCommand(_query, conn);
            try
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        if(reader[0] != DBNull.Value)
                        {
                            sReturn = reader[0].ToString();
                            break;
                        }
                    }
                }
                conn.Close();
            }
            catch(Exception ex)
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

            SqlConnection conn = new SqlConnection(Properties.Settings.Default.HBConn);
            SqlCommand cmd = new SqlCommand(_query, conn);
            try
            {
                conn.Open();
                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    if(!reader.HasRows)
                    {
                        sReturn = "N/A";
                        sVar.LogHandler.CreateLogAction("No information was found related to this query.", EricStabileLibrary.csLogging.LogState.NOTE);
                    }
                    while(reader.Read())
                    {
                        if(reader[0] != DBNull.Value)
                        {
                            sReturn = reader[0].ToString().TrimEnd();
                            sVar.LogHandler.CreateLogAction(String.Format("{0} was found in the ItemMaster database.", sReturn), EricStabileLibrary.csLogging.LogState.NOTE);
                            break;
                        }
                    }
                }
                conn.Close();
            }
            catch(Exception ex)
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
            List<string> lDefectCode = new List<string>();

            string query = "SELECT * FROM DexterAOI WHERE SN = '" + _sSN + "';";

            sVar.LogHandler.CreateLogAction("Attemping to query the AOI Table.", EricStabileLibrary.csLogging.LogState.NOTE);
            sVar.LogHandler.CreateLogAction("SQL QUERY: " + query, EricStabileLibrary.csLogging.LogState.SQLQUERY);

            SqlConnection conn = new SqlConnection(Properties.Settings.Default.HBConn);
            SqlCommand cmd = new SqlCommand(query, conn);
            try
            {
                conn.Open();
                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        _dgAOI.Items.Add(new DGVAOI
                        {
                            Shift = reader["Shift"].ToString(),
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
                                       "Shift = " + reader["Shift"].ToString() + "\n" +
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
                            sVar.LogHandler.CreateLogAction(String.Format("{0} was added to the DefectCode List.", reader["DefectCode"].ToString()), EricStabileLibrary.csLogging.LogState.NOTE);
                        }
                    }
                }
                conn.Close();
            }
            catch(Exception ex)
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

            SqlConnection conn = new SqlConnection(Properties.Settings.Default.RepairConn);
            SqlCommand cmd = new SqlCommand(_query, conn);
            try
            {
                conn.Open();
                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
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

            SqlConnection conn = new SqlConnection(Properties.Settings.Default.YesDBConn);
            SqlCommand cmd = new SqlCommand(query, conn);
            try
            {
                conn.Open();
                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
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
            catch(Exception ex)
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
        public static void BeamsQuery(string _sn, ComboBox cbBeams, ListView lsvToClear)
        {
            cbBeams.Items.Clear();
            lsvToClear.Items.Clear();

            bool bFoundSN = false;

            string _query = "SELECT PCBSerial FROM Beams WHERE PCBSerial = '" + _sn + "';";

            sVar.LogHandler.CreateLogAction("Attempting to query the Beams table to see if the Serial Number exists.", EricStabileLibrary.csLogging.LogState.NOTE);
            sVar.LogHandler.CreateLogAction("SQL QUERY: " + _query, EricStabileLibrary.csLogging.LogState.SQLQUERY);

            SqlConnection conn = new SqlConnection(Properties.Settings.Default.HBConn);
            SqlCommand cmd = new SqlCommand(_query, conn);
            try
            {
                conn.Open();
                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        if(reader[0].ToString().Equals(_sn))
                        {
                            bFoundSN = true;
                            break;
                        }
                    }
                }
                conn.Close();
            }
            catch(Exception ex)
            {
                if (conn != null)
                    conn.Close();
                string sErr = "Error loading beam data.\nError Message: " + ex.Message;
                System.Windows.Forms.MessageBox.Show(sErr, "BeamsQuery()", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                sVar.LogHandler.CreateLogAction(sErr, EricStabileLibrary.csLogging.LogState.ERROR);
            }

            if (bFoundSN)
            {
                HashSet<string> hBeams = new HashSet<string>();
                List<string> lBeams = new List<string>();
                _query = "SELECT BeamNumber FROM Beams WHERE PCBSerial = '" + _sn + "' ORDER BY BeamNumber ASC;";
                cmd = new SqlCommand(_query, conn);
                try
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            if (reader[0] != DBNull.Value)
                            {
                                if (!lBeams.Contains(reader[0].ToString()))
                                    lBeams.Add(reader[0].ToString());
                            }
                        }
                    }
                    conn.Close();

                    for(int i = 0; i < lBeams.Count; i++)
                    {
                        cbBeams.Items.Add("Beam " + lBeams[i]);
                    }
                }
                catch(Exception ex)
                {
                    if (conn != null)
                        conn.Close();
                    //TODO: Log
                }

            }
            else sVar.LogHandler.CreateLogAction("The Serial Number does not exist in the beams table.", EricStabileLibrary.csLogging.LogState.NOTE);

        }

        public static void BeamsQuery(string _query, ListView lsvToFill)
        {
            SqlConnection conn = new SqlConnection(Properties.Settings.Default.HBConn);
            SqlCommand cmd = new SqlCommand(_query, conn);
            try
            {
                string[] testVal = new string[2];
                lsvToFill.Items.Clear();
                GridView gv = new GridView();
                lsvToFill.View = gv;
                gv.Columns.Add(new GridViewColumn { Header = "Test", DisplayMemberBinding = new Binding("Test") });
                gv.Columns.Add(new GridViewColumn { Header = "Value", DisplayMemberBinding = new Binding("Value") });
                conn.Open();
                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        for(int i = 0; i < reader.FieldCount; i++)
                        {
                            if (reader[i] != DBNull.Value)
                            {
                                try
                                {
                                    testVal[0] = reader.GetName(i).ToString();
                                    if (testVal[0].Equals("TestID") || testVal[0].Equals("PCBSerial") || testVal[0].Equals("BeamNumber") || testVal[0].Equals("TestType") || testVal[0].Equals("BeamKey")) { }//ignore these values
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
            frmPartNumber fpn = new frmPartNumber(bProduction);
            fpn.ShowDialog();
            if (sVar.SelectedPartNumberPartName.PartNumberSelected)
            {
                lTB[0].Text = sVar.SelectedPartNumberPartName.PartNumber;
                lTB[1].Text = sVar.SelectedPartNumberPartName.PartName;
                lTB[2].Text = sVar.SelectedPartNumberPartName.PartSeries;
            }
        }

        /// <summary>
        /// Checks to see if the string passed in is null.
        /// </summary>
        /// <param name="valToTest">Value that needs to be checked.</param>
        /// <returns>Returns String.Empty if the value is null; otherwise, return the value passed in.</returns>
        public static string dbValSubmit(string valToTest)
        {
            return String.IsNullOrEmpty(valToTest) ? String.Empty : valToTest;
        }

        public static string unitIssuesValSubmit(ComboBox cbToCheck)
        {
            return String.IsNullOrEmpty(cbToCheck.Text) ? "NF, " : cbToCheck.Text + ", ";
        }

        public static string unitIssuesValSubmit(TextBox txtToCheck)
        {
            return String.IsNullOrEmpty(txtToCheck.Text) ? "NF, " : txtToCheck.Text + ", ";
        }

        public static string unitIssuesValSubmit(string strToCheck)
        {
            return String.IsNullOrEmpty(strToCheck) ? "NF, " : strToCheck + ", ";
        }

        public static string unitStripNF(string strToCheck)
        {
            return strToCheck.Equals("NF") ? String.Empty : strToCheck;
        }
        /* ---- END ---- */

        /// <summary>
        /// Helps determine if the ref designator is valid.
        /// </summary>
        public static void checkForValidRefDes(TextBox txtToCheck)
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
            for(int i = 0; i < _dg.Items.Count; i++)
            {
                MultiplePartsReplaced dgmpr = (MultiplePartsReplaced)_dg.Items[i];
                mpReturn.PartReplaced += unitIssuesValSubmit(dgmpr.PartReplaced);
                mpReturn.RefDesignator += unitIssuesValSubmit(dgmpr.RefDesignator);
                mpReturn.PartsReplacedPartDescription += unitIssuesValSubmit(dgmpr.PartsReplacedPartDescription);
            }

            if(!string.IsNullOrEmpty(mpReturn.PartReplaced))
                mpReturn.PartReplaced = mpReturn.PartReplaced.TrimEnd(new Char[] { ',', ' ' });

            if(!string.IsNullOrEmpty(mpReturn.RefDesignator))
                mpReturn.RefDesignator = mpReturn.RefDesignator.TrimEnd(new Char[] { ',', ' ' });

            if(!string.IsNullOrEmpty(mpReturn.PartsReplacedPartDescription))
                mpReturn.PartsReplacedPartDescription = mpReturn.PartsReplacedPartDescription.TrimEnd(new Char[] { ',', ' ' });

            return mpr;
        }

        /// <summary>
        /// Returns a list of MultiplePartsReplaced objects from a datagrid.
        /// </summary>
        public static List<MultiplePartsReplaced> getMPRList(DataGrid _dg)
        {
            List<MultiplePartsReplaced> lmpReturn = new List<MultiplePartsReplaced>();

            for(int i = 0; i < _dg.Items.Count; i++)
            {
                MultiplePartsReplaced dgmpr = (MultiplePartsReplaced)_dg.Items[i];
                dgmpr.PartReplaced = unitIssuesValSubmit(dgmpr.PartReplaced).TrimEnd(new Char[] { ',', ' ' });
                dgmpr.RefDesignator = unitIssuesValSubmit(dgmpr.RefDesignator).TrimEnd(new Char[] { ',', ' ' });
                dgmpr.PartsReplacedPartDescription = unitIssuesValSubmit(dgmpr.PartsReplacedPartDescription).TrimEnd(new Char[] { ',', ' ' });
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
            CustomerInformation _ciReturn = new CustomerInformation();
            _ciReturn.CustomerNumber = CustomerNumber;

            string query = "SELECT * FROM RepairCustomerInformation WHERE CustomerNumber = '" + CustomerNumber + "'";
            SqlConnection conn = new SqlConnection(Properties.Settings.Default.RepairConn);
            SqlCommand cmd = new SqlCommand(query, conn);

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

        private static List<RepairMultipleIssues> breakdownRepairIssues(List<RepairMultipleIssues> _lrmi)
        {
            List<RepairMultipleIssues> lCombinedRMI = new List<RepairMultipleIssues>();
            Dictionary<int, List<RepairMultipleIssues>> dMultiParts = new Dictionary<int, List<RepairMultipleIssues>>();
            List<RepairMultipleIssues> lIndividualIssues = new List<RepairMultipleIssues>();

            #region Split the overall list into two separate list: 1 with individual unit issues, 2 with same unit issue but different parts replaced/ref des
            int key = 0;
            RepairMultipleIssues rmi = null;
            for (int i = 0; i < _lrmi.Count; i++)
            {
                key++;
                if(_lrmi.Count.Equals(1))
                {
                    lIndividualIssues.Add(_lrmi[i]);
                    _lrmi.Remove(_lrmi[i]);
                    break;
                }

                bool bAddComparisonRMI = false;
                rmi = _lrmi[i];
                dMultiParts[key] = new List<RepairMultipleIssues>();
                for (int j = 1; j < _lrmi.Count; j++) 
                {
                    RepairMultipleIssues _cRMI = _lrmi[j];

                    if ( rmi.ReportedIssue == _cRMI.ReportedIssue &&
                         rmi.TestResult == _cRMI.TestResult &&
                         rmi.TestResultAbort == _cRMI.TestResultAbort &&
                         rmi.Cause == _cRMI.Cause &&
                         rmi.Replacement == _cRMI.Replacement )
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
                foreach (KeyValuePair<int, List<RepairMultipleIssues>> kvp in dMultiParts)
                {
                    if (kvp.Value.Count > 0)
                    {
                        RepairMultipleIssues _rmiCombine = new RepairMultipleIssues();

                        for(int i = 0; i < kvp.Value.Count; i++)
                        {
                            if (i == 0)
                            {
                                _rmiCombine.ReportedIssue = kvp.Value[i].ReportedIssue;
                                _rmiCombine.TestResult = kvp.Value[i].TestResult;
                                _rmiCombine.TestResultAbort = kvp.Value[i].TestResultAbort;
                                _rmiCombine.Cause = kvp.Value[i].Cause;
                                _rmiCombine.Replacement = kvp.Value[i].Replacement;
                                _rmiCombine.MultiPartsReplaced = new List<MultiplePartsReplaced>();
                                _rmiCombine.MultiPartsReplaced.Add(kvp.Value[i].SinglePartReplaced);
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
        /// Returns a list of RepairMultipleIssues based off of an ID
        /// </summary>
        public static List<RepairMultipleIssues> GetRepairUnitIssues(string ID)
        {
            List<RepairMultipleIssues> lRMI = new List<RepairMultipleIssues>();

            string query = "SELECT TestResult, TestResultAbort, Cause, Replacement, PartsReplaced, RefDesignator, PartsReplacedPartDescription FROM TechnicianUnitIssues WHERE ID = '" + ID + "'";
            SqlConnection conn = new SqlConnection(Properties.Settings.Default.RepairConn);
            SqlCommand cmd = new SqlCommand(query, conn);

            try
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        lRMI.Add(new RepairMultipleIssues()
                        {
                            TestResult = dbValSubmit(reader[0].ToString()),
                            TestResultAbort = dbValSubmit(reader[1].ToString()),
                            Cause = dbValSubmit(reader[2].ToString()),
                            Replacement = dbValSubmit(reader[3].ToString()),
                            SinglePartReplaced = new MultiplePartsReplaced()
                            {
                                PartReplaced = dbValSubmit(reader[4].ToString()),
                                RefDesignator = dbValSubmit(reader[5].ToString()),
                                PartsReplacedPartDescription = dbValSubmit(reader[6].ToString())
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

        #region Not In Use Anymore
        //public static List<ProductionMultipleUnitIssues> GetProductionUnitIssues(string ID)
        //{
        //    List<ProductionMultipleUnitIssues> lRMI = new List<ProductionMultipleUnitIssues>();

        //    string query = "SELECT TestResult, TestResultAbort, Issue, Item, Problem, PartsReplaced, RefDesignator, PartsReplacedPartDescription FROM TechnicianUnitIssues WHERE ID = '" + ID + "'";
        //    SqlConnection conn = new SqlConnection(Properties.Settings.Default.RepairConn);
        //    SqlCommand cmd = new SqlCommand(query, conn);

        //    try
        //    {
        //        conn.Open();
        //        conn.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        if (conn != null)
        //            conn.Close();

        //        System.Windows.Forms.MessageBox.Show("Error loading the Production Unit Issues.\nError Message: " + ex.Message, "Repair Unit Issues Load Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
        //    }


        //    return lRMI;
        //}
        #endregion
    }
}
