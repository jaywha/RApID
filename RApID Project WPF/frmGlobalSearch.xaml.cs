using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace RApID_Project_WPF
{
    /// <summary>
    /// Interaction logic for frmGlobalSearch.xaml
    /// </summary>
    public partial class frmGlobalSearch : Window
    {
        private readonly RecordList _records;

        public frmGlobalSearch()
        {
            InitializeComponent();
            _records = (RecordList)Resources["records"];
        }

        private void wndMain_Loaded(object sender, RoutedEventArgs e)
        {
            _records.GetData(lblLoadingIndicator, Dispatcher).ContinueWith(new Action<Task>((_) =>
            {
                Console.WriteLine("[INFO]: Number of rows in data grid (" + dgSubmissions.Items.Count + ").");

                lblLoadingIndicator.Dispatcher.Invoke(() =>
                    lblLoadingIndicator.Visibility = Visibility.Collapsed
                );

                progData.Dispatcher.Invoke(() =>
                    progData.Visibility = Visibility.Collapsed
                );

            }));
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

        private void textBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter
                && sender is TextBox txt
                && !string.IsNullOrEmpty(txt.Text.Trim()))
            {
                var dataView = CollectionViewSource.GetDefaultView(dgSubmissions.ItemsSource);

                dataView.Filter
                    = (obj) =>
                    {
                        var row = obj as Record;
                        var allowRow = true;
                        if(!string.IsNullOrEmpty(txtPartNumber.Text.Trim()))
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
                        return allowRow;
                    };

                dataView.Refresh();
            }
        }

        private void btnClearFilters_Click(object sender, RoutedEventArgs e)
            => CollectionViewSource.GetDefaultView(dgSubmissions.ItemsSource).Filter = null;
    }
}
