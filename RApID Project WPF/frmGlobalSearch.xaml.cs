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
using System.Data;

namespace RApID_Project_WPF
{
    /// <summary>
    /// Interaction logic for frmGlobalSearch.xaml
    /// </summary>
    public partial class frmGlobalSearch : Window
    {
        public frmGlobalSearch()
        {
            InitializeComponent();
        }

        private async void GetData()
        {
            await Task.Factory.StartNew(() =>
            {
                using (var conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
                {
                    conn.Open();
                    using (var adapter = new SqlDataAdapter("SELECT * FROM [Repair].[dbo].[TechnicianSubmission]", conn))
                    {
                        var t = new DataTable();
                        adapter.Fill(t);
                        dgSubmissions.Dispatcher.Invoke(() =>
                            dgSubmissions.ItemsSource = t.DefaultView
                        );
                    }
                }
            }).ContinueWith(new Action<Task>((_) => {
                lblLoadingIndicator.Dispatcher.Invoke(() =>
                    lblLoadingIndicator.Visibility = Visibility.Collapsed
                );
            }));
        }

        private void wndMain_Loaded(object sender, RoutedEventArgs e) => GetData();
    }
}
