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
using System.Windows.Automation;
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
            if (_ci != null) ciCustInfo = _ci;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtCustomerNumber.Text = ciCustInfo?.CustomerNumber ?? "";
            txtCustomerName.Text = ciCustInfo?.CustomerName ?? "";
            txtCustAddy1.Text = ciCustInfo?.CustomerAddy1 ?? "";
            AddCustomerAddressLine(txtCustAddy2, ciCustInfo?.CustomerAddy2 ?? "");
            AddCustomerAddressLine(txtCustAddy3, ciCustInfo?.CustomerAddy3 ?? "");
            AddCustomerAddressLine(txtCustAddy4, ciCustInfo?.CustomerAddy4 ?? "");
            txtCustCity.Text = ciCustInfo?.CustomerCity ?? "";
            txtCustState.Text = ciCustInfo?.CustomerState ?? "";
            txtCustPostal.Text = ciCustInfo?.CustomerPostalCode ?? "";
            txtCustCountryCode.Text = ciCustInfo?.CustomerCountryCode ?? "";
        }

        private void AddCustomerAddressLine(TextBox textbox, string value)
        {
            if (string.IsNullOrEmpty(value.Trim()))
            {
                ((Label)AutomationProperties.GetLabeledBy(textbox)).Visibility = Visibility.Collapsed;
                textbox.Visibility = Visibility.Collapsed;
            }
            else textbox.Text = value;
        }
    }
}
