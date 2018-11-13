using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EricStabileLibrary;

namespace RApID_Project_WPF.UserControls
{
    /// <summary>
    /// User control version of the End-of-Line tabitem
    /// </summary>
    public partial class ucEOLTab : UserControl
    {
        public static readonly DependencyProperty _serialNumber = DependencyProperty.Register("SerialNumber", typeof(string), typeof(ucEOLTab), new PropertyMetadata(""));
        public static readonly DependencyProperty _partSeries = DependencyProperty.Register("PartSeries", typeof(string), typeof(ucEOLTab), new PropertyMetadata(""));

        [Description("SN from parent Window"), Category("Common")]
        public string SerialNumber
        {
            get => (string)GetValue(_serialNumber);
            set => SetValue(_serialNumber, value);
        }

        [Description("Part Series from parent Window"), Category("Common")]
        public string PartSeries
        {
            get => (string)GetValue(_partSeries);
            set => SetValue(_partSeries, value);
        }

        InitSplash initS = new InitSplash();
        csObjectHolder.csObjectHolder holder = csObjectHolder.csObjectHolder.ObjectHolderInstanceRight();

        /// <summary>
        /// Default contstructor
        /// </summary>
        public ucEOLTab()
        {
            InitializeComponent();
            holder.vGetServerName("");
        }

        /// <summary>
        /// Fills the EOL Data Tab with all of the information related to the serial number.
        /// </summary>
        internal void Fill()
        {
            Reset();
            string query = $"SELECT TestID FROM tblEOL WHERE PCBSerial = '{SerialNumber}';";
            cbEOLTestID.FillFromQuery(query);

            query = $"SELECT TestID FROM tblPre WHERE PCBSerial = '{SerialNumber}';";
            cbPRETestID.FillFromQuery(query);

            query = $"SELECT TestID FROM tblPost WHERE PCBSerial = '{SerialNumber}';";
            cbPOSTTestID.FillFromQuery(query);

            if (cbEOLTestID.Items.Count > 0)
            {
                cbBEAMSTestType.Items.Add("EOL");
            }

            if (cbPRETestID.Items.Count > 0)
                cbBEAMSTestType.Items.Add("PRE");

            if (cbPOSTTestID.Items.Count > 0)
            {
                cbBEAMSTestType.Items.Add("POST");
            }
        }

        /// <summary>
        /// Resets the EOL Test Tab
        /// </summary>
        internal void Reset()
        {
            cbEOLTestID.Items.Clear();
            cbPRETestID.Items.Clear();
            cbPOSTTestID.Items.Clear();
            cbBEAMSTestType.Items.Clear();
            cbBEAMSTestID.Items.Clear();
            cbBEAMSBeamNum.Items.Clear();

            lsvEOL.Items.Clear();
            lsvPreBurnIn.Items.Clear();
            lsvPostBurnIn.Items.Clear();
            lsvBeamTestId.Items.Clear();
        }

        private void cbEOLTestID_DropDownClosed(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(cbEOLTestID.Text))
            {
                initS.InitSplash1("Loading EOL Data...");
                csCrossClassInteraction.lsvFillFromQuery(holder.HummingBirdConnectionString,
                    "SELECT * FROM tblEOL WHERE TestID = '" + cbEOLTestID.Text + "';", lsvEOL);
                csSplashScreenHelper.ShowText("Done...");
                csSplashScreenHelper.Hide();
            }
        }

        private void cbPRETestID_DropDownClosed(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(cbPRETestID.Text))
            {
                initS.InitSplash1("Loading PRE Data...");
                csCrossClassInteraction.lsvFillFromQuery(holder.HummingBirdConnectionString,
                    "SELECT * FROM tblPRE WHERE TestID = '" + cbPRETestID.Text + "';", lsvPreBurnIn);
                csSplashScreenHelper.ShowText("Done...");
                csSplashScreenHelper.Hide();
            }
        }

        private void cbPOSTTestID_DropDownClosed(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(cbPOSTTestID.Text))
            {
                initS.InitSplash1("Loading POST Data...");
                csCrossClassInteraction.lsvFillFromQuery(holder.HummingBirdConnectionString,
                    "SELECT * FROM tblPOST WHERE TestID = '" + cbPOSTTestID.Text + "';", lsvPostBurnIn);
                csSplashScreenHelper.ShowText("Done...");
                csSplashScreenHelper.Hide();
            }
        }

        private void cbBEAMSTestType_DropDownClosed(object sender, EventArgs e)
        {
            switch (cbBEAMSTestType.Text)
            {
                case "EOL":
                    csCrossClassInteraction.FillBeamTestIDFromType(cbEOLTestID, cbBEAMSTestID, lsvBeamTestId, cbBEAMSBeamNum);
                    break;
                case "PRE":
                    csCrossClassInteraction.FillBeamTestIDFromType(cbPRETestID, cbBEAMSTestID, lsvBeamTestId, cbBEAMSBeamNum);
                    break;
                case "POST":
                    csCrossClassInteraction.FillBeamTestIDFromType(cbPOSTTestID, cbBEAMSTestID, lsvBeamTestId, cbBEAMSBeamNum);
                    break;
                default:
                    break;
            }
        }

        private void cbBEAMSTestID_DropDownClosed(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(cbBEAMSTestID.Text))
            {
                initS.InitSplash1("Generating Beams...");
                csCrossClassInteraction.BeamsQuery(SerialNumber, cbBEAMSBeamNum,
                    lsvBeamTestId, PartSeries.Contains("XDR"));
                csSplashScreenHelper.ShowText("Done...");
                csSplashScreenHelper.Hide();
            }
        }

        private void cbBEAMSBeamNum_DropDownClosed(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(cbBEAMSTestType.Text) && !string.IsNullOrEmpty(cbBEAMSTestID.Text) && !string.IsNullOrEmpty(cbBEAMSBeamNum.Text))
            {
                initS.InitSplash1("Loading Beam Data...");
                var tableName = PartSeries.Contains("XDR") ? "tblXducerTestResults" : "Beams";
                var serialName = PartSeries.Contains("XDR") ? "SerialNumber" : "PCBSerial";
                var _query = $"SELECT * FROM {tableName} WHERE TestID = '{cbBEAMSTestID.Text}' AND {serialName} = '{SerialNumber}' " +
                    $"AND BeamNumber = '{csCrossClassInteraction.GetSpecificBeamNumber(cbBEAMSBeamNum.Text)}';";
                csCrossClassInteraction.BeamsQuery(_query, lsvBeamTestId, serialName);
                csSplashScreenHelper.ShowText("Done...");
                csSplashScreenHelper.Hide();
            }
        }
    }
}
