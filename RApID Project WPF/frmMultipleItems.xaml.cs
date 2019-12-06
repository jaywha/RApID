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
using System.Runtime.CompilerServices;

namespace RApID_Project_WPF
{
    /// <summary>
    /// Controls the type of data grid that <see cref="frmMultipleItems"/> loads up.
    /// </summary>
    public enum MultipleItemType
    {
        RP = 0,
        BOMFiles
    }

    /// <summary>
    /// Interaction logic for frmMultipleItems.xaml
    /// </summary>
    public partial class frmMultipleItems : Window
    {
        private string sOrderNumber = string.Empty;
        public MultipleItemType? ItemType;
        public List<string> Notes = new List<string>();
        public List<string> BOMFiles = new List<string>();
        StaticVars sVars = StaticVars.StaticVarsInstance();
        csObjectHolder.csObjectHolder holder = csObjectHolder.csObjectHolder.ObjectHolderInstance();

        public frmMultipleItems() => InitializeComponent();

        public frmMultipleItems(MultipleItemType itemType, [CallerMemberName] string callerMemberName = ""){
            InitializeComponent();
            Focus();
            Activate();
            BringIntoView();            
            ItemType = itemType;
            Console.WriteLine($"Select Multiple {ItemType.ToString()} - Initiated by {callerMemberName}");
        }

        public frmMultipleItems(string OrderNum) {
            InitializeComponent();
            sOrderNumber = OrderNum;
            ItemType = MultipleItemType.RP;
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
            switch (ItemType)
            {
                case MultipleItemType.BOMFiles:
                    csCrossClassInteraction.dgBuildView(dgItemInfo, DataGridTypes.BOMFILES);
                    break;
                default:
                case MultipleItemType.RP:
                    csCrossClassInteraction.dgBuildView(dgItemInfo, DataGridTypes.CUSTOMERINFO);
                    break;
            }
        }

        /// <summary>
        /// Loads the DataGrid with relevant RP information.
        /// </summary>
        private void loadDGView()
        {
            switch (ItemType)
            {
                case MultipleItemType.BOMFiles:
                    LoadBOMView();
                    break;
                default:
                case MultipleItemType.RP:
                    LoadRPView();
                    break;
            }
        }

        private void LoadBOMView()
        {
            try
            {
                var pairs = BOMFiles.Zip(Notes, (file, note) => file + "," + note);

                foreach (var pair in pairs)
                {
                    var info = pair.Split(',');

                    MULTIPLEBOM bominfo = new MULTIPLEBOM
                    {
                        Filename = info[0].Split('\\').Last(),
                        Notes = info[1],
                        FilePath = info[0]
                    };

                    dgItemInfo.Items.Add(bominfo);
                }
            } catch (ArgumentException ae) {
                csExceptionLogger.csExceptionLogger.Write("loadBOMView_MissingParameters",ae);
                MessageBox.Show($"Can't have empty data fields!{(BOMFiles==null?"\n\t•"+nameof(BOMFiles):"")}{(Notes==null?"\n\t•"+nameof(Notes):"")}", "loadBOMView() - Empty Fields", MessageBoxButton.OK, MessageBoxImage.Warning);
            } catch (Exception ex) {
                MessageBox.Show("There was an issue loading the BOM File list information.\nError Message: " + ex.Message, "loadBOMView()", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRPView()
        {
            string query = "SELECT * FROM CustomerRepairOrderFromJDE WHERE OrderNumber = '" + sOrderNumber + "'";
            SqlConnection conn = new SqlConnection(holder.RepairConnectionString);
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
                        dgItemInfo.Items.Add(dmpr);
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

        #region Events When an Item Is Selected
        private void btnSelect_Click(object sender, RoutedEventArgs e) => ProcessChosenItem();
        private void dgItemInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e) => ProcessChosenItem();

        private void ProcessChosenItem()
        {
            if (dgItemInfo.SelectedItem != null)
            {
                switch (ItemType)
                {
                    case MultipleItemType.BOMFiles:
                        MULTIPLEBOM item = (MULTIPLEBOM)dgItemInfo.SelectedItem;
                        if (!item.FilePath.Contains(frmBoardFileManager.ELEC_ROOT_DIR))
                            item.FilePath = frmBoardFileManager.ELEC_ROOT_DIR + item.FilePath;
                        sVars.SelectedBOMFile = item;
                        DialogResult = true;
                        break;
                    default:
                    case MultipleItemType.RP:
                        sVars.SelectedRPNumber = (DGVMULTIPLERP)dgItemInfo.SelectedItem;
                        DialogResult = true;
                        break;
                }
                this.Close();
            }
        }
        #endregion

        private void dgItemInfo_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
