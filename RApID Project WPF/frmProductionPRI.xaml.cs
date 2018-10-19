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


        public frmProductionPRI(PreviousRepairInformation _pri)
        {
            InitializeComponent();
            PRI = _pri;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (PRI == null)
                Close();

            if (!loadPRI())
                Close();
        }

        private bool loadPRI()
        {
            var conn = new SqlConnection(holder.RepairConnectionString);

            string query = "SELECT * FROM TechnicianSubmission WHERE ID = '" + PRI.ID + "'";
            string logQuery = "SELECT * FROM TechLogs WHERE ID = @logID";
            string actionQuery = "SELECT * FROM TechLogActions WHERE ActionID = @aid";

            var cmd = new SqlCommand(query, conn);
            var logCmd = new SqlCommand(logQuery, conn);
            var actionCmd = new SqlCommand(actionQuery, conn);

            try
            {
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        txtTechName.Text = csCrossClassInteraction.dbValSubmit(reader["Technician"].ToString());
                        txtDateReceived.Text = csCrossClassInteraction.dbValSubmit(reader["DateReceived"].ToString());
                        txtDateSubmitted.Text = csCrossClassInteraction.dbValSubmit(reader["DateSubmitted"].ToString());
                        txtPartName.Text = csCrossClassInteraction.dbValSubmit(reader["PartName"].ToString());
                        txtPartNumber.Text = csCrossClassInteraction.dbValSubmit(reader["PartNumber"].ToString());
                        txtPartSeries.Text = csCrossClassInteraction.dbValSubmit(reader["Series"].ToString());
                        txtCommSubClass.Text = csCrossClassInteraction.dbValSubmit(reader["CommoditySubClass"].ToString());
                        txtSW.Text = csCrossClassInteraction.dbValSubmit(reader["SoftwareVersion"].ToString());
                        txtTOR.Text = csCrossClassInteraction.dbValSubmit(reader["TypeOfReturn"].ToString());
                        txtFromArea.Text = csCrossClassInteraction.dbValSubmit(reader["FromArea"].ToString());

                        logCmd.Parameters.AddWithValue("@logID",
                            int.Parse(csCrossClassInteraction.dbValSubmit(reader["LogID"].ToString())));

                        rtbAddComm.AppendText(csCrossClassInteraction.dbValSubmit(reader["AdditionalComments"].ToString()));
                    }
                }

                using(var reader = logCmd.ExecuteReader())
                {
                    reader.Read(); // only one record

                    actionCmd.Parameters.AddWithValue("@aid", csCrossClassInteraction.dbValSubmit(reader["ActionID"].ToString()));

                    uclaTechActions.LogToView = new csLog() {                        
                        Tech = csCrossClassInteraction.dbValSubmit(reader["Tech"].ToString()),
                        LogCreationTime = DateTime.Parse(csCrossClassInteraction.dbValSubmit(reader["LogCreationTime"].ToString())),
                        LogSubmitTime = DateTime.Parse(csCrossClassInteraction.dbValSubmit(reader["LogSubmitTime"].ToString()))
                    };
                }

                using(var reader = actionCmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        var @action = new csLogAction()
                        {
                            ControlType = csCrossClassInteraction.dbValSubmit(reader["ControlType"].ToString()),
                            ControlName = csCrossClassInteraction.dbValSubmit(reader["ControlName"].ToString()),
                            ControlContent = csCrossClassInteraction.dbValSubmit(reader["ControlContent"].ToString()),
                            EventType = (csLogging.LogState) Enum.Parse(typeof(csLogging.LogState),
                                csCrossClassInteraction.dbValSubmit(reader["LogState"].ToString())),
                            EventTiming = DateTime.Parse(csCrossClassInteraction.dbValSubmit(reader["EventTiming"].ToString())),
                            LogNote = csCrossClassInteraction.dbValSubmit(reader["LogNote"].ToString()),
                            LogError = reader.GetBoolean(reader.GetOrdinal("LogError"))
                        };

                        uclaTechActions.LogToView.lActions.Add(@action);
                    }
                }

                conn.Close();

                uclaTechActions.InitView();

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
