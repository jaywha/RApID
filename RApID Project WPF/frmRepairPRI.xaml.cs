﻿using System;
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
    public partial class repairPRI : Window
    {
        PreviousRepairInformation PRI;
        csObjectHolder.csObjectHolder holder = csObjectHolder.csObjectHolder.ObjectHolderInstance();
        CustomerInformation CurrentCustomer;


        public repairPRI(PreviousRepairInformation _pri)
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
                        sUnitID = csCrossClassInteraction.dbValSubmit(reader["ID"].ToString());

                        txtTechName.Text = csCrossClassInteraction.dbValSubmit(reader["Technician"].ToString());
                        txtDateReceived.Text = csCrossClassInteraction.dbValSubmit(reader["DateReceived"].ToString());
                        txtDateSubmitted.Text = csCrossClassInteraction.dbValSubmit(reader["DateSubmitted"].ToString());
                        txtPartName.Text = csCrossClassInteraction.dbValSubmit(reader["PartName"].ToString());
                        txtPartNumber.Text = csCrossClassInteraction.dbValSubmit(reader["PartNumber"].ToString());
                        txtPartSeries.Text = csCrossClassInteraction.dbValSubmit(reader["Series"].ToString());
                        txtCommSubClass.Text = csCrossClassInteraction.dbValSubmit(reader["CommoditySubClass"].ToString());
                        txtSW.Text = csCrossClassInteraction.dbValSubmit(reader["SoftwareVersion"].ToString());
                        txtTOR.Text = csCrossClassInteraction.dbValSubmit(reader["TypeOfReturn"].ToString());
                        txtTOF.Text = csCrossClassInteraction.dbValSubmit(reader["TypeOfFailure"].ToString());
                        txtHOU.Text = csCrossClassInteraction.dbValSubmit(reader["HoursOnUnit"].ToString());

                        rtbAddComm.AppendText(csCrossClassInteraction.dbValSubmit(reader["AdditionalComments"].ToString()));
                        txtTechAct1.Text = csCrossClassInteraction.dbValSubmit(reader["TechAct1"].ToString());
                        txtTechAct2.Text = csCrossClassInteraction.dbValSubmit(reader["TechAct2"].ToString());
                        txtTechAct3.Text = csCrossClassInteraction.dbValSubmit(reader["TechAct3"].ToString());

                        txtCustNum.Text = csCrossClassInteraction.dbValSubmit(reader["CustomerNumber"].ToString());
                        if(!string.IsNullOrEmpty(txtCustNum.Text))
                            customerCmd.Parameters.AddWithValue("@CustNum", int.Parse(txtCustNum.Text));

                        rtbQCDQEComments.AppendText(csCrossClassInteraction.dbValSubmit(reader["QCDQEComments"].ToString()));
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
                                CustomerNumber = csCrossClassInteraction.dbValSubmit(customerReader["CustomerNumber"].ToString()),
                                CustomerName = csCrossClassInteraction.dbValSubmit(customerReader["CustomerName"].ToString()),
                                CustomerAddy1 = csCrossClassInteraction.dbValSubmit(customerReader["CustomerAddressLine1"].ToString()),
                                CustomerAddy2 = csCrossClassInteraction.dbValSubmit(customerReader["CustomerAddressLine2"].ToString()),
                                CustomerAddy3 = csCrossClassInteraction.dbValSubmit(customerReader["CustomerAddressLine3"].ToString()),
                                CustomerAddy4 = csCrossClassInteraction.dbValSubmit(customerReader["CustomerAddressLine4"].ToString()),
                                CustomerCity = csCrossClassInteraction.dbValSubmit(customerReader["CustomerCity"].ToString()),
                                CustomerState = csCrossClassInteraction.dbValSubmit(customerReader["CustomerState"].ToString()),
                                CustomerPostalCode = csCrossClassInteraction.dbValSubmit(customerReader["CustomerPostalCode"].ToString()),
                                CustomerCountryCode = csCrossClassInteraction.dbValSubmit(customerReader["CustomerCountryCode"].ToString())
                            };
                        }
                    }
                }

                conn.Close();
                                
                if (!string.IsNullOrEmpty(sUnitID)) {
                    List<RepairMultipleIssues> lRMI = csCrossClassInteraction.GetRepairUnitIssues(sUnitID);

                    if (lRMI.Count < 1) return true;

                    txtReportedIssue.Text = lRMI[0].ReportedIssue;
                    txtTestResult.Text = lRMI[0].TestResult;
                    txtTestResultAbort.Text = lRMI[0].TestResultAbort;
                    txtCause.Text = lRMI[0].Cause;
                    txtReplacement.Text = lRMI[0].Replacement;
                    dgMultipleParts.ItemsSource = lRMI[0].MultiPartsReplaced;

                    if (lRMI.Count < 2) return true;

                    txtReportedIssue2.Text = lRMI[1].ReportedIssue;
                    txtTestResult2.Text = lRMI[1].TestResult;
                    txtTestResultAbort2.Text = lRMI[1].TestResultAbort;
                    txtCause2.Text = lRMI[1].Cause;
                    txtReplacement2.Text = lRMI[1].Replacement;
                    dgMultipleParts2.ItemsSource = lRMI[1].MultiPartsReplaced;

                    if (lRMI.Count < 3) return true;

                    txtReportedIssue3.Text = lRMI[2].ReportedIssue;
                    txtTestResult3.Text = lRMI[2].TestResult;
                    txtTestResultAbort3.Text = lRMI[2].TestResultAbort;
                    txtCause3.Text = lRMI[2].Cause;
                    txtReplacement3.Text = lRMI[2].Replacement;
                    dgMultipleParts3.ItemsSource = lRMI[2].MultiPartsReplaced;
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
