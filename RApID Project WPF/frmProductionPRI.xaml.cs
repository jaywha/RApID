using System;
using System.Windows;
using System.Data.SqlClient;
using EricStabileLibrary;
using RApID_Project_WPF.UserControls;
using System.Linq;

namespace RApID_Project_WPF
{
    /// <summary>
    /// Interaction logic for frmProductionPRI.xaml
    /// </summary>
    public partial class frmProductionPRI : Window
    {
        PreviousRepairInformation PRI;
        csObjectHolder.csObjectHolder holder = csObjectHolder.csObjectHolder.ObjectHolderInstance();

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
            var conn = new SqlConnection(holder.RepairConnectionString);

            string query = "SELECT * FROM TechnicianSubmission WHERE ID = '" + PRI.ID + "'";
            string logQuery = "SELECT * FROM TechLogs WHERE ID = @logID";
            string actionQuery = "SELECT * FROM TechLogActions WHERE ActionID = @aid";
            string unitIssueQuery = $"SELECT * FROM TechnicianUnitIssues WHERE ID = '{PRI.ID}'";

            var cmd = new SqlCommand(query, conn);
            var logCmd = new SqlCommand(logQuery, conn); logCmd.Parameters.Add("@logID", System.Data.SqlDbType.Int);
            var actionCmd = new SqlCommand(actionQuery, conn); actionCmd.Parameters.Add("@aid", System.Data.SqlDbType.Int);
            var unitIssueCmd = new SqlCommand(unitIssueQuery, conn);

            try
            {
                conn.Open();

                using (var reader = cmd.ExecuteReader())
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

                        if (reader["LogID"] != DBNull.Value)
                            logCmd.Parameters.AddWithValue("@logID",
                                int.Parse(reader["LogID"].ToString().EmptyIfNull()));
                                                
                        var issue = ucIssues[0];
                        if (issue != null)
                        {
                            issue.FillUnitIssue(null,
                                reader["ReportedIssue"].ToString().EmptyIfNull(),
                                reader["TestResult"].ToString().EmptyIfNull(),
                                reader["TestResultAbort"].ToString().EmptyIfNull(),
                                reader["Cause"].ToString().EmptyIfNull(),
                                reader["Replacement"].ToString().EmptyIfNull(),
                                reader["Issue"].ToString().EmptyIfNull(),
                                reader["Item"].ToString().EmptyIfNull(),
                                reader["Problem"].ToString().EmptyIfNull(),
                                reader["PartsReplaced"].ToString(),
                                reader["RefDesignator"].ToString()
                            );
                        }

                        rtbAddComm.AppendText((reader["AdditionalComments"]?.ToString() ?? "").EmptyIfNull());
                    }
                }

                using(var reader = unitIssueCmd.ExecuteReader())
                {
                    int index = 1;
                    while(reader.Read())
                    {
                        ucIssues.AddTabItem();
                        ucIssues[index++].FillUnitIssue(null,
                            reader["ReportedIssue"].ToString().EmptyIfNull(),
                            reader["TestResult"].ToString().EmptyIfNull(),
                            reader["TestResultAbort"].ToString().EmptyIfNull(),
                            reader["Cause"].ToString().EmptyIfNull(),
                            reader["Replacement"].ToString().EmptyIfNull(),
                            reader["Issue"].ToString().EmptyIfNull(),
                            reader["Item"].ToString().EmptyIfNull(),
                            reader["Problem"].ToString().EmptyIfNull(),
                            reader["PartsReplaced"].ToString(),
                            reader["RefDesignator"].ToString()
                        );
                    }
                }

                if (logCmd.Parameters[0].Value == null || logCmd.Parameters[0].Value == DBNull.Value)
                {
                    conn.Close();
                    return true;
                }

                using (var reader = logCmd.ExecuteReader())
                {
                    reader.Read(); // only one record

                    actionCmd.Parameters.AddWithValue("@aid", csCrossClassInteraction.EmptyIfNull(reader["ActionID"].ToString()));

                    ucTechActions.LogToView = new csLog() {                        
                        Tech = csCrossClassInteraction.EmptyIfNull(reader["Tech"].ToString()),
                        LogCreationTime = DateTime.Parse(csCrossClassInteraction.EmptyIfNull(reader["LogCreationTime"].ToString())),
                        LogSubmitTime = DateTime.Parse(csCrossClassInteraction.EmptyIfNull(reader["LogSubmitTime"].ToString()))
                    };
                }

                using(var reader = actionCmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        var @action = new csLogAction()
                        {
                            ControlType = csCrossClassInteraction.EmptyIfNull(reader["ControlType"].ToString()),
                            ControlName = csCrossClassInteraction.EmptyIfNull(reader["ControlName"].ToString()),
                            ControlContent = csCrossClassInteraction.EmptyIfNull(reader["ControlContent"].ToString()),
                            EventType = (csLogging.LogState) Enum.Parse(typeof(csLogging.LogState),
                                csCrossClassInteraction.EmptyIfNull(reader["LogState"].ToString())),
                            EventTiming = DateTime.Parse(csCrossClassInteraction.EmptyIfNull(reader["EventTiming"].ToString())),
                            LogNote = csCrossClassInteraction.EmptyIfNull(reader["LogNote"].ToString()),
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
