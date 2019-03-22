using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Data.SqlClient;
using EricStabileLibrary;

namespace RApID_Project_WPF
{
    /// <summary>
    /// Interaction logic for repairPRI.xaml
    /// </summary>
    public partial class frmRepairPRI : Window
    {
        PreviousRepairInformation PRI;
        csObjectHolder.csObjectHolder holder = csObjectHolder.csObjectHolder.ObjectHolderInstance();
        CustomerInformation CurrentCustomer;
        UserControls.ucLogActionView PRILogView = new UserControls.ucLogActionView();

        public frmRepairPRI(PreviousRepairInformation _pri)
        {
            InitializeComponent();
            PRI = _pri;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PRI == null || !loadPRI()) { Close(); }
            } catch (Exception ex)
            {
                csExceptionLogger.csExceptionLogger.Write("frmRepairPRI-Window_Loaded", ex);
            }
        }
        
        private bool loadPRI()
        {
            string sUnitID = string.Empty;
            var conn = new SqlConnection(holder.RepairConnectionString);

            string query = "SELECT * FROM TechnicianSubmission WHERE ID = '" + PRI.ID + "'";
            string logQuery = "SELECT * FROM TechLogs WHERE ID = @logID";
            string actionQuery = "SELECT * FROM TechLogActions WHERE ActionID = @aid";
            string customerQuery = "SELECT * FROM RepairCustomerInformation WHERE CustomerNumber = @CustNum";
           
            var cmd = new SqlCommand(query, conn);
            var customerCmd = new SqlCommand(customerQuery, conn);
            var logCmd = new SqlCommand(logQuery, conn); logCmd.Parameters.Add("@logID", System.Data.SqlDbType.Int);
            var actionCmd = new SqlCommand(actionQuery, conn); actionCmd.Parameters.Add("@aid", System.Data.SqlDbType.Int);

            try
            {
                conn.Open();
                
                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        sUnitID = reader["ID"].ToString().EmptyIfNull();

                        txtTechName.Text = reader["Technician"].ToString().EmptyIfNull();
                        txtDateReceived.Text = reader["DateReceived"].ToString().EmptyIfNull();
                        txtDateSubmitted.Text = reader["DateSubmitted"].ToString().EmptyIfNull();
                        txtPartName.Text = reader["PartName"].ToString().EmptyIfNull();
                        txtPartNumber.Text = reader["PartNumber"].ToString().EmptyIfNull();
                        txtPartSeries.Text = reader["Series"].ToString().EmptyIfNull();
                        txtCommSubClass.Text = reader["CommoditySubClass"].ToString().EmptyIfNull();
                        txtSW.Text = reader["SoftwareVersion"].ToString().EmptyIfNull();
                        txtTOR.Text = reader["TypeOfReturn"].ToString().EmptyIfNull();
                        txtTOF.Text = reader["TypeOfFailure"].ToString().EmptyIfNull();
                        txtHOU.Text = reader["HoursOnUnit"].ToString().EmptyIfNull();

                        rtbAddComm.AppendText(reader["AdditionalComments"].ToString().EmptyIfNull());
                        txtTechAct1.Text = reader["TechAct1"].ToString().EmptyIfNull();
                        txtTechAct2.Text = reader["TechAct2"].ToString().EmptyIfNull();
                        txtTechAct3.Text = reader["TechAct3"].ToString().EmptyIfNull();

                        txtCustNum.Text = reader["CustomerNumber"].ToString().EmptyIfNull();
                        if(!string.IsNullOrEmpty(txtCustNum.Text))
                            customerCmd.Parameters.AddWithValue("@CustNum", int.Parse(txtCustNum.Text));

                        rtbQCDQEComments.AppendText(reader["QCDQEComments"].ToString().EmptyIfNull());
                    }
                }

                if (!string.IsNullOrEmpty(txtCustNum.Text)) // if number available, load customer info
                {
                    using (var customerReader = customerCmd.ExecuteReader())
                    {
                        while (customerReader.Read())
                        {
                            txtCustName.Text = customerReader["CustomerName"].ToString();
                            CurrentCustomer = new CustomerInformation()
                            {
                                CustomerNumber = customerReader["CustomerNumber"].ToString().EmptyIfNull(),
                                CustomerName = customerReader["CustomerName"].ToString().EmptyIfNull(),
                                CustomerAddy1 = customerReader["CustomerAddressLine1"].ToString().EmptyIfNull(),
                                CustomerAddy2 = customerReader["CustomerAddressLine2"].ToString().EmptyIfNull(),
                                CustomerAddy3 = customerReader["CustomerAddressLine3"].ToString().EmptyIfNull(),
                                CustomerAddy4 = customerReader["CustomerAddressLine4"].ToString().EmptyIfNull(),
                                CustomerCity = customerReader["CustomerCity"].ToString().EmptyIfNull(),
                                CustomerState = customerReader["CustomerState"].ToString().EmptyIfNull(),
                                CustomerPostalCode = customerReader["CustomerPostalCode"].ToString().EmptyIfNull(),
                                CustomerCountryCode = customerReader["CustomerCountryCode"].ToString().EmptyIfNull()
                            };
                        }
                    }
                }

                conn.Close();
                                
                if (!string.IsNullOrEmpty(sUnitID)) {
                    List<UnitIssueModel> lRMI = csCrossClassInteraction.GetRepairUnitIssues(sUnitID);

                    int ucIndex = 0;
                    foreach (var issue in lRMI)
                    {
                        ucIssues[ucIndex].FillUnitIssue(
                            //TODO: Find Multiple Parts to be added here
                            issue.ReportedIssue,
                            issue.TestResult,
                            issue.TestResultAbort,
                            issue.Issue,
                            issue.Cause,
                            issue.Replacement,
                            issue.Item,
                            issue.Problem,
                            issue.SinglePartReplaced.RefDesignator,
                            issue.SinglePartReplaced.PartReplaced,
                            issue.SinglePartReplaced.PartsReplacedPartDescription
                        );
                        if (lRMI.Count > ucIndex) ucIssues.AddTabItem();
                    }
                }

                if (logCmd.Parameters[0].Value == null || logCmd.Parameters[0].Value == DBNull.Value)
                {
                    conn.Close();
                    return true;
                }

                btnViewLog.Visibility = Visibility.Visible;

                using (var reader = logCmd.ExecuteReader())
                {
                    reader.Read(); // only one record

                    actionCmd.Parameters.AddWithValue("@aid", reader["ActionID"].ToString().EmptyIfNull());

                    PRILogView.LogToView = new csLog()
                    {
                        Tech = reader["Tech"].ToString().EmptyIfNull(),
                        LogCreationTime = DateTime.Parse(reader["LogCreationTime"].ToString().EmptyIfNull()),
                        LogSubmitTime = DateTime.Parse(reader["LogSubmitTime"].ToString().EmptyIfNull())
                    };
                }

                using (var reader = actionCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var @action = new csLogAction()
                        {
                            ControlType = reader["ControlType"].ToString().EmptyIfNull(),
                            ControlName = reader["ControlName"].ToString().EmptyIfNull(),
                            ControlContent = reader["ControlContent"].ToString().EmptyIfNull(),
                            EventType = (csLogging.LogState)Enum.Parse(typeof(csLogging.LogState),
                                reader["LogState"].ToString().EmptyIfNull()),
                            EventTiming = DateTime.Parse(reader["EventTiming"].ToString().EmptyIfNull()),
                            LogNote = reader["LogNote"].ToString().EmptyIfNull(),
                            LogError = reader.GetBoolean(reader.GetOrdinal("LogError"))
                        };

                        PRILogView.LogToView.lActions.Add(@action);
                    }
                }


                return true;
            }
            catch(Exception ex)
            {
                if (conn != null)
                    conn.Close();

                MessageBox.Show("Error loading the previous repair information for this submission.\nError Message: " + ex.Message, "Load Issue", MessageBoxButton.OK, MessageBoxImage.Error);
                csExceptionLogger.csExceptionLogger.Write("RepairPRI_Issues", ex);

                return false;
            }
        }

        private void gbCustomerInfo_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var fullInfo = new frmFullCustomerInformation(CurrentCustomer);
            fullInfo.Closed += delegate {
                gbCustomerInfo.IsEnabled = true;
            };
            fullInfo.Show();
            gbCustomerInfo.IsEnabled = false;
        }

        private void gbCustomerInfo_MouseEnter(object sender, MouseEventArgs e)
        {
            gbCustomerInfo.Header = "Double Click for more information.";
            gbCustomerInfo.Background = Brushes.PeachPuff;
        }

        private void gbCustomerInfo_MouseLeave(object sender, MouseEventArgs e)
        {
            gbCustomerInfo.Header = "Customer Information";
            gbCustomerInfo.Background = Brushes.LightGray;
        }

        private void BtnViewLog_Click(object sender, RoutedEventArgs e)
        {
            var frm = new Window() { Content = PRILogView };
            (frm.Content as UserControls.ucLogActionView).InitView();
        }
    }
}
