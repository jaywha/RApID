using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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

        private async void wndMain_Loaded(object sender, RoutedEventArgs e)
        {
            await _records.GetData(GlobalCancelToken, lblLoadingIndicator, progData, dgSubmissions.Dispatcher);
            dgSubmissions.UpdateLayout();
        }

        private void wndMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        private void textBoxNumericInput(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox txt && (txt.Text[0] > 57 || txt.Text[0] < 48))
            {
                txt.Text = txt.Text.Substring(0, txt.Text.Length - 1);
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

            var newSqlQuery = new System.Data.SqlClient.SqlCommand
            {
                CommandText =
                "SELECT * FROM [Repair].[dbo].[TechnicianSubmission] WHERE " +
                $"([PartNumber] = '{(!string.IsNullOrEmpty(txtPartNumber.Text.Trim()) ? txtPartNumber.Text.Trim() : "")}' " +
                $"OR [OrderNumber] = '{(!string.IsNullOrEmpty(txtOrderNumber.Text.Trim()) ? txtOrderNumber.Text.Trim() : "")}' " +
                $"OR [SerialNumber] = '{(!string.IsNullOrEmpty(txtSerialNumber.Text.Trim()) ? txtSerialNumber.Text.Trim() : "")}' " +
                $"OR [CustomerNumber] = '{(!string.IsNullOrEmpty(txtCustomerNumber.Text.Trim()) ? txtCustomerNumber.Text.Trim() : "")}') " +
                $"AND (([DateReceived] >= '{(!string.IsNullOrEmpty(dpStartDate.Text.Trim()) ? dpStartDate.Text.Trim() : "")}' " +
                $"OR '{(!string.IsNullOrEmpty(dpStartDate.Text.Trim()) ? dpStartDate.Text.Trim() : "")}' IS NULL) " +
                $"AND ([DateReceived] <= '{(!string.IsNullOrEmpty(dpEndDate.Text.Trim()) ? dpEndDate.Text.Trim() : "")}' " +
                $"OR '{(!string.IsNullOrEmpty(dpEndDate.Text.Trim()) ? dpEndDate.Text.Trim() : "")}' IS NULL)) " +
                $"ORDER BY [DateReceived] DESC"
            };

            await _records.GetData(GlobalCancelToken, lblLoadingIndicator, progData,
                dgSubmissions.Dispatcher, newSqlQuery.CommandText, "Update Complete!",
                "RApID - Global Search Update!", "Updates completed based on filters.");
        }

        private void ApplyFilters(object sender, RoutedEventArgs e)
        {
            var dataView = CollectionViewSource.GetDefaultView(dgSubmissions.ItemsSource);

            lblLoadingIndicator.Visibility = Visibility.Visible;
            lblLoadingIndicator.Content = "Applying filters...";
            progData.Visibility = Visibility.Visible;

            dataView.Filter
                = (obj) =>
                {
                    var row = obj as Record;
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

        #region Data Grid Context Menu Item Clicks
        //TODO: Add Context Menu - Open Appropriate Form using Serial Number from current row.
        // https://stackoverflow.com/a/16824343/7476183 - Need some adaptation, but should be what we need.
        private void OpenProduction(object sender, RoutedEventArgs e)
        {
            MainWindow.Notify.ShowBalloonTip("Work in Progress...", "This feature will come in a future update.",
                Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
        }

        private void OpenRepair(object sender, RoutedEventArgs e)
        {
            MainWindow.Notify.ShowBalloonTip("Work in Progress...", "This feature will come in a future update.",
                Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
        }

        private void OpenDQE(object sender, RoutedEventArgs e)
        {
            MainWindow.Notify.ShowBalloonTip("Work in Progress...", "This feature will come in a future update.",
                Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
        }
        #endregion
    }
}
