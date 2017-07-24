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
    public partial class repairPRI : Window
    {
        PreviousRepairInformation PRI;
        csObjectHolder.csObjectHolder holder = csObjectHolder.csObjectHolder.ObjectHolderInstance();


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
            string query = "SELECT * FROM TechnicianSubmission WHERE ID = '" + PRI.ID + "'";
            SqlConnection conn = new SqlConnection(holder.RepairConnectionString);
            SqlCommand cmd = new SqlCommand(query, conn);
            try
            {
                conn.Open();
                
                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
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
                        txtTOF.Text = csCrossClassInteraction.dbValSubmit(reader["TypeOfFailure"].ToString());
                        txtHOU.Text = csCrossClassInteraction.dbValSubmit(reader["HoursOnUnit"].ToString());

                        //TODO: Get Unit Issues
                        sUnitID = csCrossClassInteraction.dbValSubmit(reader["ID"].ToString());

                        rtbAddComm.AppendText(csCrossClassInteraction.dbValSubmit(reader["AdditionalComments"].ToString()));
                        txtTechAct1.Text = csCrossClassInteraction.dbValSubmit(reader["TechAct1"].ToString());
                        txtTechAct2.Text = csCrossClassInteraction.dbValSubmit(reader["TechAct2"].ToString());
                        txtTechAct3.Text = csCrossClassInteraction.dbValSubmit(reader["TechAct3"].ToString());

                        //TODO: Load Full Customer Information??
                        txtCustNum.Text = csCrossClassInteraction.dbValSubmit(reader["CustomerNumber"].ToString());
                        //TODO: Get Customer Name

                        rtbQCDQEComments.AppendText(csCrossClassInteraction.dbValSubmit(reader["QCDQEComments"].ToString()));

                    }
                }

                conn.Close();

                if(!string.IsNullOrEmpty(sUnitID))
                {
                    List<RepairMultipleIssues> lRMI = csCrossClassInteraction.GetRepairUnitIssues(sUnitID);
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
    }
}
