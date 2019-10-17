using System;
using System.Windows;
using System.Data.SqlClient;
using EricStabileLibrary;

namespace RApID_Project_WPF
{
    /// <summary>
    /// Interaction logic for frmProductionPRI.xaml
    /// </summary>
    public partial class frmProductionPRI : Window
    {
        PreviousRepairInformation PRI;
        csObjectHolder.csObjectHolder holder = csObjectHolder.csObjectHolder.ObjectHolderInstance();

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="_pri">Previous Repair Information data to show in this form</param>
        public frmProductionPRI(PreviousRepairInformation _pri)
        {
            InitializeComponent();
            PRI = _pri;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (PRI == null || !loadPRI()) { Close(); }
        }

        private bool loadPRI()
        {
            SqlConnection conn = new SqlConnection(holder.RepairConnectionString);

            string query = "SELECT * FROM TechnicianSubmission WHERE ID = '" + PRI.ID + "'"; 
            string logQuery = "SELECT * FROM TechLogs WHERE ID = @logID";
            string actionQuery = "SELECT * FROM TechLogActions WHERE ActionID = @aid";
            string unitIssueQuery = $"SELECT * FROM TechnicianUnitIssues WHERE ID = '{PRI.ID}'";

            SqlCommand cmd = new SqlCommand(query, conn);
            SqlCommand logCmd = new SqlCommand(logQuery, conn);
            SqlCommand actionCmd = new SqlCommand(actionQuery, conn);
            SqlCommand unitIssueCmd = new SqlCommand(unitIssueQuery, conn);

            try
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        txtTechName.Text = reader["Technician"].ToString().EmptyIfNull();
                        txtDateReceived.Text = reader["DateReceived"].ToString().EmptyIfNull();
                        txtDateSubmitted.Text = reader["DateSubmitted"].ToString().EmptyIfNull();
                        txtPartName.Text = reader["PartName"].ToString().EmptyIfNull();
                        txtPartNumber.Text = reader["PartNumber"].ToString().EmptyIfNull();
                        txtPartSeries.Text = reader["Series"].ToString().EmptyIfNull();
                        txtCommSubClass.Text = reader["CommoditySubClass"].ToString().EmptyIfNull();
                        txtSW.Text = reader["SoftwareVersion"].ToString().EmptyIfNull();
                        txtTOR.Text = reader["TypeOfReturn"].ToString().EmptyIfNull();
                        txtFromArea.Text = reader["FromArea"].ToString().EmptyIfNull();

                        txtTechAct1.Text = reader["TechAct1"].ToString().EmptyIfNull();
                        txtTechAct2.Text = reader["TechAct2"].ToString().EmptyIfNull();
                        txtTechAct3.Text = reader["TechAct3"].ToString().EmptyIfNull();

                        if (reader["LogID"] != DBNull.Value)
                            logCmd.Parameters.AddWithValue("@logID",
                                int.Parse(reader["LogID"].ToString().EmptyIfNull()));

                        rtbAddComm.AppendText((reader["AdditionalComments"]?.ToString() ?? "").EmptyIfNull());
                    }
                }

                using(SqlDataReader reader = unitIssueCmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        if(string.IsNullOrEmpty(ucIssues[0].ReportedIssue)) {
                            #region First Issue
                            ucIssues[0].FillUnitIssue(
                                reader["ReportedIssue"].ToString().EmptyIfNull(),
                                reader["TestResult"].ToString().EmptyIfNull(),
                                reader["TestResultAbort"].ToString().EmptyIfNull(),
                                reader["Cause"].ToString().EmptyIfNull(),
                                reader["Replacement"].ToString().EmptyIfNull(),
                                reader["Issue"].ToString().EmptyIfNull(),
                                reader["Item"].ToString().EmptyIfNull(),
                                reader["Problem"].ToString().EmptyIfNull()
                            );
                            ucIssues[0].AddUnitIssuePart(
                                reader["PartsReplaced"].ToString(),
                                reader["RefDesignator"].ToString()
                            );
                            #endregion  
                            continue;
                        }

                        (System.Windows.Controls.TabItem Tab, int ActualTabIndex) = ucIssues.AddTabItem();
                        ucIssues[ActualTabIndex].FillUnitIssue(
                            reader["ReportedIssue"].ToString().EmptyIfNull(),
                            reader["TestResult"].ToString().EmptyIfNull(),
                            reader["TestResultAbort"].ToString().EmptyIfNull(),
                            reader["Cause"].ToString().EmptyIfNull(),
                            reader["Replacement"].ToString().EmptyIfNull(),
                            reader["Issue"].ToString().EmptyIfNull(),
                            reader["Item"].ToString().EmptyIfNull(),
                            reader["Problem"].ToString().EmptyIfNull()
                        );
                        ucIssues[ActualTabIndex].AddUnitIssuePart(
                            reader["PartsReplaced"].ToString(),
                            reader["RefDesignator"].ToString()
                        );
                    }
                }

                if (logCmd.Parameters.Count == 0
                    || logCmd.Parameters[0].Value == null
                    || logCmd.Parameters[0].Value == DBNull.Value)
                {

                    conn.Close();
                    return true;
                }

                ucTechActions.Visibility = Visibility.Visible;

                using (SqlDataReader reader = logCmd.ExecuteReader())
                {
                    reader.Read(); // only one record

                    actionCmd.Parameters.AddWithValue("@aid", reader["ActionID"].ToString().EmptyIfNull());

                    ucTechActions.LogToView = new csLog() {                        
                        Tech = reader["Tech"].ToString().EmptyIfNull(),
                        LogCreationTime = DateTime.Parse(reader["LogCreationTime"].ToString().EmptyIfNull()),
                        LogSubmitTime = DateTime.Parse(reader["LogSubmitTime"].ToString().EmptyIfNull())
                    };
                }

                ucTechActions.LogToView.lActions = new System.Collections.Generic.List<csLogAction>();
                using (SqlDataReader reader = actionCmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        csLogAction @action = new csLogAction()
                        {
                            ControlType = reader["ControlType"].ToString().EmptyIfNull(),
                            ControlName = reader["ControlName"].ToString().EmptyIfNull(),
                            ControlContent = reader["ControlContent"].ToString().EmptyIfNull(),
                            EventType = (csLogging.LogState) Enum.Parse(typeof(csLogging.LogState),
                                reader["LogState"].ToString().EmptyIfNull()),
                            EventTiming = DateTime.Parse(reader["EventTiming"].ToString().EmptyIfNull()),
                            LogNote = reader["LogNote"].ToString().EmptyIfNull(),
                            LogError = reader.GetBoolean(reader.GetOrdinal("LogError"))
                        };

                        ucTechActions.LogToView.lActions.Add(@action);
                    }
                }

                conn.Close();

                ucTechActions.InitView();

                return true;
            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();

                MessageBox.Show("Error loading the previous repair information for this submission.\nError Message: " + ex.Message, "Load Issue", MessageBoxButton.OK, MessageBoxImage.Error);

                return false;
            }
        }
    }
}
