using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;

namespace RApID_Project_WPF
{
    /// <summary>
    /// Interaction logic for frmGlobalSearch.xaml
    /// </summary>
    public partial class frmGlobalSearch : Window
    {
        private RecordList _records;
        private static CancellationTokenSource GlobalCancelSource = new CancellationTokenSource();
        private static CancellationToken GlobalCancelToken = GlobalCancelSource.Token;

        static frmGlobalSearch() { }
        private frmGlobalSearch()
        {
            InitializeComponent();
            _records = (RecordList)Resources["records"];
            _records.ToggleButtonControls = ToggleButtonsEnabled;
            _records.ToggleFilterControls = ToggleFiltersEnabled;

            ToggleFiltersEnabled(false);
            ToggleButtonsEnabled(false);

            dpStartDate.SelectedDate = DateTime.Now.AddMonths(-6);
            dpEndDate.SelectedDate = DateTime.Now;
        }

        public static frmGlobalSearch Instance { get; } = new frmGlobalSearch();

        private void wndMain_Loaded(object sender, RoutedEventArgs e)
        {            
            ToggleButtonsEnabled(true);
            ToggleFiltersEnabled(true);
            lblLoadingIndicator.Visibility = Visibility.Collapsed;
            progData.Visibility = Visibility.Collapsed;
        }

        private void wndMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
            MainWindow.GlobalInstance.MakeFocus();
        }

        private void textBoxNumericInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                if (sender is TextBox txt)
                {
                    e.Handled = Regex.IsMatch(e.Text, "[^0-9-]+");
                }
            } catch (IndexOutOfRangeException ioore)
            {
                csExceptionLogger.csExceptionLogger.Write("NumericInput_Error", ioore);
            }
        }

        private void ToggleFiltersEnabled(bool enabled)
        {
            foreach (UIElement c in stkPnlFilters.Children)
            {
                c.IsEnabled = enabled;
            }
        }

        private void ToggleButtonsEnabled(bool enabled)
        {
            foreach (UIElement c in stkPnlButtons.Children)
            {
                c.IsEnabled = enabled;
            }
        }

        private async void GetNewRecords(object sender, RoutedEventArgs e)
        {
            _records.Clear();

            System.Data.SqlClient.SqlCommand newSqlQuery = new System.Data.SqlClient.SqlCommand
            {
                CommandText =
                "SELECT * FROM [Repair].[dbo].[TechnicianSubmission] WHERE " +
                (!string.IsNullOrEmpty(txtPartNumber.Text.Trim()) ? $"[PartNumber] LIKE '%{txtPartNumber.Text.Trim()}%' AND " : "") +
                (!string.IsNullOrEmpty(txtOrderNumber.Text.Trim()) ? $"[OrderNumber] LIKE '%{txtOrderNumber.Text.Trim()}%' AND " : "") +
                (!string.IsNullOrEmpty(txtSerialNumber.Text.Trim()) ? $"[SerialNumber] LIKE '%{txtSerialNumber.Text.Trim()}%' AND " : "") +
                (!string.IsNullOrEmpty(txtCustomerNumber.Text.Trim()) ? $"[CustomerNumber] LIKE '%{txtCustomerNumber.Text.Trim()}%' AND " : "") +
                (!string.IsNullOrEmpty(dpStartDate.Text.Trim()) ? $"[DateReceived] >= '{dpStartDate.Text.Trim()}' AND " : "") +
                (!string.IsNullOrEmpty(dpEndDate.Text.Trim()) ? $"[DateReceived] <= '{dpEndDate.Text.Trim()}' " : "") +
                $"ORDER BY [DateReceived] DESC"
            };

            newSqlQuery.CommandText = newSqlQuery.CommandText.Replace("AND ORDER", "ORDER");
            newSqlQuery.CommandText = newSqlQuery.CommandText.Replace("WHERE ORDER", "ORDER");

            //Console.WriteLine(newSqlQuery.CommandText);

            await _records.GetData(GlobalCancelToken, lblLoadingIndicator, progData,
                dgSubmissions.Dispatcher, newSqlQuery.CommandText);
        }

        private void ApplyFilters(object sender, RoutedEventArgs e)
        {
            System.ComponentModel.ICollectionView dataView = CollectionViewSource.GetDefaultView(dgSubmissions.ItemsSource);

            lblLoadingIndicator.Visibility = Visibility.Visible;
            lblLoadingIndicator.Content = "Applying filters...";
            progData.Visibility = Visibility.Visible;

            dataView.Filter
                = (obj) =>
                {
                    Record row = obj as Record;
                    var allowRow = true;
                    if (!string.IsNullOrEmpty(txtPartNumber.Text.Trim()))
                    {
                        allowRow = allowRow && row.PartNumber.Contains(txtPartNumber.Text.Trim());
                    }

                    if (!string.IsNullOrEmpty(txtOrderNumber.Text.Trim()))
                    {
                        allowRow = allowRow && row.OrderNumber.Equals(txtOrderNumber.Text.Trim());
                    }

                    if (!string.IsNullOrEmpty(txtSerialNumber.Text.Trim()))
                    {
                        allowRow = allowRow && row.SerialNumber.Equals(txtSerialNumber.Text.Trim());
                    }

                    if (!string.IsNullOrEmpty(txtCustomerNumber.Text.Trim()))
                    {
                        allowRow = allowRow && (row.CustomerNumber == int.Parse(txtCustomerNumber.Text.Trim()));
                    }

                    if (dpStartDate.SelectedDate.HasValue)
                    {
                        allowRow = allowRow && (row.DateReceived >= dpStartDate.SelectedDate.Value);
                    }

                    if (dpEndDate.SelectedDate.HasValue)
                    {
                        allowRow = allowRow && (row.DateReceived >= dpEndDate.SelectedDate.Value);
                    }

                    return allowRow;
                };

            dataView.Refresh();

            progData.Visibility = Visibility.Collapsed;
            lblLoadingIndicator.Visibility = Visibility.Collapsed;
            lblLoadingIndicator.Content = "";

            ToggleFiltersEnabled(false);
        }

        private void btnClearFilters_Click(object sender, RoutedEventArgs e)
        {
            lblLoadingIndicator.Visibility = Visibility.Visible;
            lblLoadingIndicator.Content = "Clearing filters...";
            progData.Visibility = Visibility.Visible;

            CollectionViewSource.GetDefaultView(dgSubmissions.ItemsSource).Filter = null;

            progData.Visibility = Visibility.Collapsed;
            lblLoadingIndicator.Visibility = Visibility.Collapsed;
            lblLoadingIndicator.Content = "";

            ToggleFiltersEnabled(true);
        }

        private void btnExcelExport_Click(object sender, RoutedEventArgs e)
        {
            Excel.Application applicationXL = new Excel.Application();
            try
            {
                string DESKTOP = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);               
                string WORKING_FILE = DESKTOP + @"\RApID Reports\" + DateTime.Now.Year + @"\TechnicianSubmission-Report--" + DateTime.Now.ToString("MM-dd-yy") + ".xlsx";

                if (!Directory.Exists(DESKTOP + @"\RApID Reports\" + DateTime.Now.Year))
                    Directory.CreateDirectory(DESKTOP + @"\RApID Reports\" + DateTime.Now.Year);

                File.Copy(@"\\joi\eu\Public\EE Process Test\Software\RApID Project WPF\Templates\TechnicianSubmissionReportTemplate.xlsx", WORKING_FILE, true);

                Excel.Workbook workbookMain = applicationXL.Workbooks.Open(WORKING_FILE);
                Excel.Worksheet worksheetMain = (Excel.Worksheet)workbookMain.Sheets[1];
                int lastBlankRow = worksheetMain.Cells.SpecialCells(Excel.XlCellType.xlCellTypeBlanks).Row + 1;
                int indexer = 1; // Excel 1-indexed

                foreach(Record record in _records)
                {
                    foreach(var param in record.GetType().GetProperties())
                    {
                        worksheetMain.Cells[lastBlankRow, indexer++].Value = param.GetValue(record, null);
                    }
                    lastBlankRow++;
                    indexer = 1;
                }

                worksheetMain.Columns["A:AU"].AutoFit();

                workbookMain.Save();
                workbookMain.Close();
                workbookMain = null;

                MessageBox.Show("We've completed creating the Excel report.\n" +
                                "It's located on your Dekstop in the PO Manager Reports folder.",
                                "Completed Excel Export Action", MessageBoxButton.OK, MessageBoxImage.Information);

                Process.Start(WORKING_FILE);

            } catch (Exception ex) {
                csExceptionLogger.csExceptionLogger.Write("GlobalSearch-MakeExcelSheet", ex);
                MessageBox.Show("There was an error. Attempting to close any open Excel instances...\nPlease try again afterwards.\n" + ex.Message, "Background Task Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Data Grid Context Menu Item Clicks
        private void OpenProduction(object sender, RoutedEventArgs e)
        {
            //Get the clicked MenuItem
            MenuItem menuItem = (MenuItem)sender;

            //Get the ContextMenu to which the menuItem belongs
            ContextMenu contextMenu = (ContextMenu)menuItem.Parent;

            //Find the placementTarget
            DataGrid item = (DataGrid)contextMenu.PlacementTarget;

            //Get the underlying item, that you cast to your object that is bound
            //to the DataGrid (and has subject and state as property)
            Record recordToOpen = (Record)item.SelectedCells[0].Item;

            //Open form with Record as PRI
            frmProductionPRI frm = new frmProductionPRI(recordToOpen);
            frm.Show();
        }

        private void OpenRepair(object sender, RoutedEventArgs e)
        {
            //Get the clicked MenuItem
            MenuItem menuItem = (MenuItem)sender;

            //Get the ContextMenu to which the menuItem belongs
            ContextMenu contextMenu = (ContextMenu)menuItem.Parent;

            //Find the placementTarget
            DataGrid item = (DataGrid)contextMenu.PlacementTarget;

            //Get the underlying item, that you cast to your object that is bound
            //to the DataGrid (and has subject and state as property)
            Record recordToOpen = (Record)item.SelectedCells[0].Item;

            //Open form with Record as PRI
            frmRepairPRI frm = new frmRepairPRI(recordToOpen);
            frm.Show();
        }

        //TODO: Just need to do DQE now ???
        // Add Context Menu - Open Appropriate Form using Serial Number from current row.
        // https://stackoverflow.com/a/16824343/7476183 - Need some adaptation, but should be what we need.
        private void OpenDQE(object sender, RoutedEventArgs e)
        {
            MainWindow.Notify.ShowBalloonTip("Work in Progress...", "This feature will come in a future update.",
                Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
        }
        #endregion
    }
}
