/*
 * frmMultipleRP: Used when multiple RP numbers are found.
 * Created By: Eric Stabile
 */

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
using System.Data.OleDb;
using System.Data.SqlClient;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace RApID_Project_WPF
{
    /// <summary>
    /// Interaction logic for frmMultipleRP.xaml
    /// </summary>
    public partial class frmMultipleRP : Window
    {
        private string sOrderNumber = String.Empty;
        StaticVars sVars = StaticVars.StaticVarsInstance();
        public frmMultipleRP(string OrderNum)
        {
            InitializeComponent();
            sOrderNumber = OrderNum;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            buildDGView();
            loadDGView();
        }

        /// <summary>
        /// Builds the DataGrid
        /// </summary>
        private void buildDGView()
        {
            csCrossClassInteraction.dgBuildView(dgRPInfo, "CUSTOMERINFO");
        }

        /// <summary>
        /// Loads the DataGrid with relevant RP information.
        /// </summary>
        private void loadDGView()
        {
            string query = "SELECT * FROM CustomerRepairOrderFromJDE WHERE OrderNumber = '" + sOrderNumber + "'";
            SqlConnection conn = new SqlConnection(Properties.Settings.Default.RepairConn);
            SqlCommand cmd = new SqlCommand(query, conn);
            try
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DGVMULTIPLERP dmpr = new DGVMULTIPLERP
                        {
                            RPNumber = reader["ItemNumber"].ToString().TrimEnd(),
                            LineNumber = Convert.ToDouble(reader["LineNumber"]),
                            CustomerNumber = reader["CustomerNumber"].ToString(),
                            CustomerName = reader["CustomerName"].ToString(),
                            CustInfo = new CustomerInformation()
                            {
                                CustomerNumber = reader["CustomerNumber"].ToString(),
                                CustomerName = reader["CustomerName"].ToString(),
                                CustomerAddy1 = reader["CustomerAddressLine1"].ToString(),
                                CustomerAddy2 = reader["CustomerAddressLine2"].ToString(),
                                CustomerAddy3 = reader["CustomerAddressLine3"].ToString(),
                                CustomerAddy4 = reader["CustomerAddressLine4"].ToString(),
                                CustomerCity = reader["CustomerCity"].ToString(),
                                CustomerState = reader["CustomerState"].ToString(),
                                CustomerPostalCode = reader["CustomerPostalCode"].ToString(),
                                CustomerCountryCode = reader["CustomerCountryCode"].ToString()
                            }
                        };
                        dgRPInfo.Items.Add(dmpr);
                    }
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                if (conn != null)
                    conn.Close();

                MessageBox.Show("There was an issue loading the Customer Information.\nError Message: " + ex.Message, "loadDGView()", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Events When A RP Number Is Selected
        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (dgRPInfo.SelectedItem != null)
            {
                sVars.SelectedRPNumber = (DGVMULTIPLERP)dgRPInfo.SelectedItem;
                this.Close();
            }
        }

        private void dgRPInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgRPInfo.SelectedItem != null)
            {
                sVars.SelectedRPNumber = (DGVMULTIPLERP)dgRPInfo.SelectedItem;
                this.Close();
            }
        }
        #endregion

        private void dgRPInfo_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
