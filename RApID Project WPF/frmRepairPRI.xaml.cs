using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SqlClient;

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


        public frmRepairPRI(PreviousRepairInformation _pri)
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
            string sUnitID = string.Empty;
            var conn = new SqlConnection(holder.RepairConnectionString);

            string query = "SELECT * FROM TechnicianSubmission WHERE ID = '" + PRI.ID + "'";
            string customerQuery = "SELECT * FROM RepairCustomerInformation WHERE CustomerNumber = @CustNum";
           
            var cmd = new SqlCommand(query, conn);
            var customerCmd = new SqlCommand(customerQuery, conn);

            try
            {
                conn.Open();
                
                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        sUnitID = csCrossClassInteraction.EmptyIfNull(reader["ID"].ToString());

                        txtTechName.Text = csCrossClassInteraction.EmptyIfNull(reader["Technician"].ToString());
                        txtDateReceived.Text = csCrossClassInteraction.EmptyIfNull(reader["DateReceived"].ToString());
                        txtDateSubmitted.Text = csCrossClassInteraction.EmptyIfNull(reader["DateSubmitted"].ToString());
                        txtPartName.Text = csCrossClassInteraction.EmptyIfNull(reader["PartName"].ToString());
                        txtPartNumber.Text = csCrossClassInteraction.EmptyIfNull(reader["PartNumber"].ToString());
                        txtPartSeries.Text = csCrossClassInteraction.EmptyIfNull(reader["Series"].ToString());
                        txtCommSubClass.Text = csCrossClassInteraction.EmptyIfNull(reader["CommoditySubClass"].ToString());
                        txtSW.Text = csCrossClassInteraction.EmptyIfNull(reader["SoftwareVersion"].ToString());
                        txtTOR.Text = csCrossClassInteraction.EmptyIfNull(reader["TypeOfReturn"].ToString());
                        txtTOF.Text = csCrossClassInteraction.EmptyIfNull(reader["TypeOfFailure"].ToString());
                        txtHOU.Text = csCrossClassInteraction.EmptyIfNull(reader["HoursOnUnit"].ToString());

                        rtbAddComm.AppendText(csCrossClassInteraction.EmptyIfNull(reader["AdditionalComments"].ToString()));
                        txtTechAct1.Text = csCrossClassInteraction.EmptyIfNull(reader["TechAct1"].ToString());
                        txtTechAct2.Text = csCrossClassInteraction.EmptyIfNull(reader["TechAct2"].ToString());
                        txtTechAct3.Text = csCrossClassInteraction.EmptyIfNull(reader["TechAct3"].ToString());

                        txtCustNum.Text = csCrossClassInteraction.EmptyIfNull(reader["CustomerNumber"].ToString());
                        if(!string.IsNullOrEmpty(txtCustNum.Text))
                            customerCmd.Parameters.AddWithValue("@CustNum", int.Parse(txtCustNum.Text));

                        rtbQCDQEComments.AppendText(csCrossClassInteraction.EmptyIfNull(reader["QCDQEComments"].ToString()));
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
                                CustomerNumber = csCrossClassInteraction.EmptyIfNull(customerReader["CustomerNumber"].ToString()),
                                CustomerName = csCrossClassInteraction.EmptyIfNull(customerReader["CustomerName"].ToString()),
                                CustomerAddy1 = csCrossClassInteraction.EmptyIfNull(customerReader["CustomerAddressLine1"].ToString()),
                                CustomerAddy2 = csCrossClassInteraction.EmptyIfNull(customerReader["CustomerAddressLine2"].ToString()),
                                CustomerAddy3 = csCrossClassInteraction.EmptyIfNull(customerReader["CustomerAddressLine3"].ToString()),
                                CustomerAddy4 = csCrossClassInteraction.EmptyIfNull(customerReader["CustomerAddressLine4"].ToString()),
                                CustomerCity = csCrossClassInteraction.EmptyIfNull(customerReader["CustomerCity"].ToString()),
                                CustomerState = csCrossClassInteraction.EmptyIfNull(customerReader["CustomerState"].ToString()),
                                CustomerPostalCode = csCrossClassInteraction.EmptyIfNull(customerReader["CustomerPostalCode"].ToString()),
                                CustomerCountryCode = csCrossClassInteraction.EmptyIfNull(customerReader["CustomerCountryCode"].ToString())
                            };
                        }
                    }
                }

                conn.Close();
                                
                if (!string.IsNullOrEmpty(sUnitID)) {
                    List<RepairMultipleIssues> lRMI = csCrossClassInteraction.GetRepairUnitIssues(sUnitID);

                    int ucIndex = 0;
                    foreach (var issue in lRMI)
                    {
                        ucIssues[ucIndex].FillUnitIssue(
                            issue.MultiPartsReplaced,
                            issue.ReportedIssue,
                            issue.TestResult,
                            issue.TestResultAbort,
                            issue.Cause,
                            issue.Replacement                            
                        );
                        if (lRMI.Count > ucIndex) ucIssues.AddTabItem();
                    }
                }
                

                return true;
            }
            catch(Exception ex)
            {
                if (conn != null)
                    conn.Close();

                MessageBox.Show("Error loading the previous repair information for this submission.\nError Message: " + ex.Message, "Load Issue", MessageBoxButton.OK, MessageBoxImage.Error);

                return false;
            }
        }

        private void btnViewCustInfo_Click(object sender, RoutedEventArgs e)
        {
            var fullInfo = new frmFullCustomerInformation(CurrentCustomer);
            fullInfo.Closed += delegate { btnViewCustInfo.IsEnabled = true; };
            fullInfo.Show();
            btnViewCustInfo.IsEnabled = false;
        }
    }
}
