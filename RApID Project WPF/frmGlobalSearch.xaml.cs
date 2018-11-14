using System;
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
        private readonly RecordList _records;

        static frmGlobalSearch() { }
        private frmGlobalSearch()
        {
            InitializeComponent();
            _records = (RecordList)Resources["records"];
            _records.ToggleButtonControls = ToggleButtonsEnabled;
            _records.ToggleFilterControls = ToggleFiltersEnabled;

            ToggleFiltersEnabled(false);
            ToggleButtonsEnabled(false);
        }

        public static frmGlobalSearch Instance { get; } = new frmGlobalSearch();

        private async void wndMain_Loaded(object sender, RoutedEventArgs e) 
            => await _records.GetData(lblLoadingIndicator, progData, dgSubmissions.Dispatcher);

        private void wndMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        private void textBoxGotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox txt)
            {
                if (txt.ToolTip == null)
                {
                    txt.ToolTip = new ToolTip()
                    {
                        Content = "Press enter to search on this value...",
                        IsOpen = true
                    };
                }
                else
                {
                    ((ToolTip)txt.ToolTip).IsOpen = true;
                }
            }
        }

        private void textBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox txt && txt.ToolTip is ToolTip tip)
            {
                tip.IsOpen = false;
            }
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

        private async void GetLatestRecords(object sender, RoutedEventArgs e)
        {
            await _records.AppendData(lblLoadingIndicator, dgSubmissions.Dispatcher)
            .ContinueWith((_) =>
            {
                Console.WriteLine("[INFO]: Number of rows in data grid (" + dgSubmissions.Items.Count + ").");

                lblLoadingIndicator.Dispatcher.Invoke(() =>
                    lblLoadingIndicator.Visibility = Visibility.Collapsed
                );

                progData.Dispatcher.Invoke(() =>
                    progData.Visibility = Visibility.Collapsed
                );

            });
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

                    if (!string.IsNullOrEmpty(txtCutomerNumber.Text.Trim()))
                    {
                        allowRow = allowRow && (row.CustomerNumber == int.Parse(txtCutomerNumber.Text.Trim()));
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
    }
}
