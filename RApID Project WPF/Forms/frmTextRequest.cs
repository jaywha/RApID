using RApID_Project_WPF.CustomControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RApID_Project_WPF.Forms
{
    /// <summary>
    /// Better controls for text input
    /// </summary>
    public partial class frmTextRequest : Form
    {
        public string OriginalValue = "";
        /// <summary>
        /// Value of input from previous input interaction.
        /// </summary>
        public string Input { get; set; } = "";

        /// <summary>
        /// Default constructor -- modelled after <see cref="Microsoft.VisualBasic.Interaction.InputBox(string, string, string, int, int)"/>
        /// </summary>
        /// <param name="defaultValue">Will place this value in the input box</param>
        public frmTextRequest(string defaultValue = "")
        {
            InitializeComponent();
            txtInput.DataBindings.Add(new Binding("Text", this, "Input"));

            if (!string.IsNullOrWhiteSpace(defaultValue))
            {
                Input = defaultValue;
                OriginalValue = defaultValue;
            }
        }

        private void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) btnOK.PerformClick();
            else if (e.KeyCode == Keys.Escape) btnCancel.PerformClick();
        }

        private void btnOK_Click(object sender, EventArgs e) => DialogResult = DialogResult.OK;
        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(OriginalValue)) Input = OriginalValue;
            DialogResult = DialogResult.Cancel;
        }
    }
}
