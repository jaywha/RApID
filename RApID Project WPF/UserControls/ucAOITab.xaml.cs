using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RApID_Project_WPF.UserControls
{
    /// <summary>
    /// Interaction logic for ucAOITab.xaml
    /// </summary>
    public partial class ucAOITab : UserControl
    {
        public static readonly DependencyProperty _serialNumber = DependencyProperty.Register("SerialNumber", typeof(string), typeof(ucAOITab));

        [Description("SN from parent Window"), Category("Common")]
        public string SerialNumber
        {
            get => (string) GetValue(_serialNumber);
            set => SetValue(_serialNumber, value);
        }
            

        public ucAOITab()
        {
            InitializeComponent();
            dgAOI.dgBuildView("AOI");
            dgDefectCodes.dgBuildView("DEFECTCODES");
        }

        /// <summary>
        /// Fills the AOI Tab with all fo the information related to the serial number.
        /// </summary>
        internal void Fill()
        {
            Reset();
            csCrossClassInteraction.AOIQuery(dgAOI, dgDefectCodes, SerialNumber);
        }

        /// <summary>
        /// Resets the AOI Tab
        /// </summary>
        internal void Reset()
        {
            dgAOI.Items.Clear();
            dgDefectCodes.Items.Clear();
        }

        /// <summary>
        /// Stops users from being able to edit the contents of the data grid.
        /// </summary>
        /// <param name="sender">The <see cref="FrameworkElement"/> that triggered this event.</param>
        /// <param name="e">The related event arguments from the data grid being edited.</param>
        private void dgBeginEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
