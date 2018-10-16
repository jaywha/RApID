/*
 * frmPrintQCDQELabel: Allows you to reprint a QC/DQE Label.
 * Created By: Eric Stabile
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RApID_Project_WPF
{
    /// <summary>
    /// Interaction logic for frmPrintQCDQELabel.xaml
    /// </summary>
    public partial class frmPrintQCDQELabel : Window
    {
        public frmPrintQCDQELabel()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            btnPrint.Margin = new Thickness((this.Width / 2) - btnPrint.Width - 5, btnPrint.Margin.Top, 0, 0);
            btnClose.Margin = new Thickness(this.Width / 2 + 5, btnClose.Margin.Top, 0, 0);
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(txtQCDQEID.Text) || string.IsNullOrEmpty(cbLocation.Text))
            {
                MessageBox.Show("Please enter a Location and a Save ID before attempting to print the label.", "Missing Data", MessageBoxButton.OK, MessageBoxImage.Information);
                txtQCDQEID.Focus();
            }
            else
            {
                var printLabel = new csPrintQCDQELabel(cbLocation.Text.ToString(), System.Environment.UserName, DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"), txtQCDQEID.Text.ToString());
                printLabel.PrintLabel();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void txtQCDQEID_TextChanged(object sender, TextChangedEventArgs e)
        {
            var pattern = new Regex(@"^\d+$");
            if(!pattern.IsMatch(((TextBox)sender).Text))
            {
                MessageBox.Show("Please only enter numbers for the ID.",
                    "Bad Save ID", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
