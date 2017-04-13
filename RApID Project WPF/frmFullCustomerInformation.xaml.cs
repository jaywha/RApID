/*
 * frmFullCustomerInformation: Used to display all of the customer data we have on file.
 * Created By:Eric Stabile
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

namespace RApID_Project_WPF
{
    /// <summary>
    /// Interaction logic for frmFullCustomerInformation.xaml
    /// </summary>
    public partial class frmFullCustomerInformation : Window
    {
        private CustomerInformation ciCustInfo;

        public frmFullCustomerInformation(CustomerInformation _ci)
        {
            InitializeComponent();
            ciCustInfo = _ci;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtCustomerNumber.Text = ciCustInfo.CustomerNumber;
            txtCustomerName.Text = ciCustInfo.CustomerName;
            txtCustAddy1.Text = ciCustInfo.CustomerAddy1;
            txtCustAddy2.Text = ciCustInfo.CustomerAddy2;
            txtCustAddy3.Text = ciCustInfo.CustomerAddy3;
            txtCustAddy4.Text = ciCustInfo.CustomerAddy4;
            txtCustCity.Text = ciCustInfo.CustomerCity;
            txtCustState.Text = ciCustInfo.CustomerState;
            txtCustPostal.Text = ciCustInfo.CustomerPostalCode;
            txtCustCountryCode.Text = ciCustInfo.CustomerCountryCode;
        }
    }
}
