/*
 * frmPrintQCDQELabel: Allows you to reprint a QC/DQE Label.
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

//TODO: Make textbox only accept numbers

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
            if(String.IsNullOrEmpty(txtQCDQEID.Text) || String.IsNullOrEmpty(cbLocation.Text))
            {
                MessageBox.Show("Please enter a Location and a Save ID before attempting to print the label.", "Missing Data", MessageBoxButton.OK, MessageBoxImage.Information);
                txtQCDQEID.Focus();
            }
            else
            {
                csPrintQCDQELabel printLabel = new csPrintQCDQELabel(cbLocation.Text.ToString(), System.Environment.UserName, DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"), txtQCDQEID.Text.ToString());
                printLabel.PrintLabel();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
