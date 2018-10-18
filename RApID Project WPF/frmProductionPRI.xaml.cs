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
            string query = "SELECT * FROM TechnicianSubmission WHERE ID = '" + PRI.ID + "'";
            var conn = new SqlConnection(holder.RepairConnectionString);
            var cmd = new SqlCommand(query, conn);
            try
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
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

                        //TODO: Get Technician Actions <- from Log Files?

                        rtbAddComm.AppendText(csCrossClassInteraction.dbValSubmit(reader["AdditionalComments"].ToString()));
                    }
                }

                conn.Close();
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
