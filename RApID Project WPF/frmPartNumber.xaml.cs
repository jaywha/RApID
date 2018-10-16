/*
 * frmPartNumber: Used when a tech scans a board but the part number has yet to be filled in.
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
using System.Data.SqlClient;
using EricStabileLibrary;
using System.Data;

namespace RApID_Project_WPF
{
    /// <summary>
    /// Interaction logic for frmPartNumber.xaml
    /// </summary>
    public partial class frmPartNumber : Window
    {
        StaticVars sVar = StaticVars.StaticVarsInstance();
        bool bIsProduction;
        DataSet ds = new DataSet();
        DataTable dt = new DataTable();
        csObjectHolder.csObjectHolder holder = csObjectHolder.csObjectHolder.ObjectHolderInstance();


        public frmPartNumber(bool _bProduction)
        {
            InitializeComponent();
            bIsProduction = _bProduction;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            getCorrectData();
        }

        /// <summary>
        /// Fills dgvPartNumber with a list of part names and part numbers.
        /// </summary>
        /// <param name="_s"></param>
        private void getCorrectData()
        {
            dgvPartNumber.Items.Clear();
            string query = "";
            if (bIsProduction)
                query = "SELECT PossibleAssemblies, ProductName FROM ProductTestParams ORDER BY ProductName ASC";
            else query = "Select PartNumber, PartName From ItemMaster Where XRefCode = 'UC' OR XRefCode = 'XD' Order By PartName ASC";

            var conn = new SqlConnection(holder.HummingBirdConnectionString);
            var cmd = new SqlCommand(query, conn);
            try
            {
                conn.Open();
                dt = new DataTable();
                ds.Tables.Add(dt);
                dt.Columns.Add("Part Number");
                dt.Columns.Add("Part Name");
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        string[] splitters = { "," };
                        if(!bIsProduction)
                        {
                            if(reader[0].ToString().Contains("0-") || reader.ToString().Contains("6-"))
                            {
                                string[] sSplit = reader[0].ToString().Split(splitters, StringSplitOptions.RemoveEmptyEntries);
                                for(int i = 0; i < sSplit.Length; i++)
                                {
                                    if (sSplit[i].Contains("0-") || sSplit[i].Contains("6-"))
                                    {
                                        dt.Rows.Add(new object[] { sSplit[i].ToString().TrimEnd(), reader[1].ToString().TrimEnd() });
                                    }
                                }
                            }
                        }
                        else
                        {
                            if(reader[0].ToString().Contains("5-") || reader[0].ToString().Contains("6-") || reader[1].ToString().ToUpper().Contains("KEYPAD"))
                            {
                                string[] sSplit = reader[0].ToString().Split(splitters, StringSplitOptions.RemoveEmptyEntries);
                                for(int i = 0; i < sSplit.Length; i++)
                                {
                                    if(sSplit[i].Contains("5-") || sSplit[i].Contains("6-") || sSplit[i].Contains("7-"))
                                    {
                                        dt.Rows.Add(new object[] { sSplit[i].ToString().TrimEnd(), reader[1].ToString().TrimEnd() });
                                    }
                                }
                            }
                        }
                    }
                }
                conn.Close();
                dgvPartNumber.DataContext = dt.DefaultView;
            }
            catch(Exception ex)
            {
                if (conn != null)
                    conn.Close();

                MessageBox.Show("There was an issue loading the initial part number/part name data.\nError Message: " + ex.Message, "Error Loading Data", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //private void sortDGV(DataGridColumn col, System.ComponentModel.ListSortDirection lsd)
        //{
        //    dgvPartNumber.Items.SortDescriptions.Clear();
        //    string propertyName = "PartNumber";
        //    dgvPartNumber.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription(propertyName, lsd));

        //    foreach (DataGridColumn c in dgvPartNumber.Columns)
        //    {
        //        c.SortDirection = null;
        //    }

        //    col.SortDirection = lsd;
        //}

        /// <summary>
        /// Searches over the DataTable for the Part Number entered
        /// </summary>
        private void searchDataTable()
        {
            var dtSearch = new DataTable();
            dtSearch.Columns.Add("Part Number");
            dtSearch.Columns.Add("Part Name");
            string dtQuery = "[Part Name] LIKE '%" + txtSearch.Text + "%'";
            DataRow[] drRes = dt.Select(dtQuery);
            foreach(DataRow dr in drRes)
            {
                dtSearch.ImportRow(dr);
            }
            dgvPartNumber.DataContext = dtSearch.DefaultView;
        }

        /// <summary>
        /// Sets the SelectedPartNumberPartName var on the Repair form.
        /// </summary>
        private void dgvPartNumber_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvPartNumber.SelectedItem == null) { sVar.SelectedPartNumberPartName.PartNumberSelected = false; return; }
            else
            {
                try
                {
                    var drv = (DataRowView)dgvPartNumber.SelectedItem;
                    var selItem = new DGVPARTNUMNAMEITEM() { PartNumber = drv[0].ToString(), PartName = drv[1].ToString(), PartSeries = csCrossClassInteraction.SeriesQuery(drv[0].ToString()) };
                    sVar.SelectedPartNumberPartName = selItem;
                    sVar.SelectedPartNumberPartName.PartNumberSelected = true;
                    this.Close();
                }
                catch { }
            }
        }

        /// <summary>
        /// This allows for double-clicking in dgvPartNumber datagrid
        /// </summary>
        private void dgvPartNumber_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            e.Cancel = true;
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            searchDataTable();
        }
    }
}
