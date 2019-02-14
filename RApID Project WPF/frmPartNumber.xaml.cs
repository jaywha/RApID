/*
 * frmPartNumber: Used when a tech scans a board but the part number has yet to be filled in.
 * Created By: Eric Stabile
 */

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data.SqlClient;
using System.Data;
using MaterialDesignThemes.Wpf;

namespace RApID_Project_WPF
{
    /// <summary>
    /// Interaction logic for frmPartNumber.xaml
    /// </summary>
    public partial class frmPartNumber : UserControl
    {
        /// <summary> Will act as a mutex for setting the value of the backed event. </summary>
        private object @lockDialogHostCloseEvent = new object();
        /// <summary> Will pipe the dialog close event to the invoking member for this control. </summary>
        public static readonly DependencyProperty DialogHostCloseProperty =
            DependencyProperty.Register("DialogHostCloseEvent", typeof(EventHandler), typeof(frmPartNumber));

        /// <summary> EventHandler property DialogHostClose </summary>
        /// <remarks> To raise the event: ((EventHandler)GetValue(DialogHostClose))?.Invoke(object sender, EventArgs e) </remarks>
        public event EventHandler DialogHostCloseEvent
        {
            add
            {
                lock (@lockDialogHostCloseEvent)
                {
                    SetValue(DialogHostCloseProperty, value);
                }
            }
            remove
            {
                lock (@lockDialogHostCloseEvent)
                {
                    SetValue(DialogHostCloseProperty, null);
                }
            }
        }


        StaticVars sVar = StaticVars.StaticVarsInstance();
        bool bIsProduction;
        DataSet ds = new DataSet();
        DataTable dt = new DataTable();
        csObjectHolder.csObjectHolder holder = csObjectHolder.csObjectHolder.ObjectHolderInstance();

        public frmPartNumber(bool _bProduction, DialogHost caller = null)
        {
            InitializeComponent();
            bIsProduction = _bProduction;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) => getCorrectData();

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
                        if(!bIsProduction) // REPAIR
                        {
                            if(reader[0].ToString().Contains("0-") || reader.ToString().Contains("6-"))
                            {
                                string[] sSplit = reader[0].ToString().Split(splitters, StringSplitOptions.RemoveEmptyEntries);
                                for(int i = 0; i < sSplit.Length; i++)
                                {
                                    if (sSplit[i].Contains("0-") || sSplit[i].Contains("6-") || sSplit[i].Contains("8-"))
                                    {
                                        dt.Rows.Add(new object[] { sSplit[i].ToString().TrimEnd(), reader[1].ToString().TrimEnd() });
                                    }
                                }
                            }
                        }
                        else // PRODUCTION
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

        /// <summary>
        /// Searches over the DataTable for the Part Number entered
        /// </summary>
        private void searchDataTable()
        {
            var dtSearch = new DataTable();
            dtSearch.Columns.Add("Part Number");
            dtSearch.Columns.Add("Part Name");
            string dtQuery = (!string.IsNullOrEmpty(txtSearchName.Text))
                ? $"[Part Name] LIKE '%{txtSearchName.Text}%' " : "";

            if(!string.IsNullOrEmpty(txtSearchName.Text))
                dtQuery += (rbtnAND.IsChecked.Value ? " AND " :
                            rbtnOR.IsChecked.Value ? " OR " : "");

            dtQuery += (!string.IsNullOrEmpty(txtSearchNum.Text))
                ? $"[Part Number] LIKE '%{txtSearchNum.Text}%'" : "";

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
                    ((EventHandler)GetValue(DialogHostCloseProperty))?.Invoke(sender, e);
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

        private void uccPartNumber_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape) ((EventHandler)GetValue(DialogHostCloseProperty))?.Invoke(sender, e);
        }
    }
}
