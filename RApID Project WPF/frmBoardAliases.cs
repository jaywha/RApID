using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace RApID_Project_WPF
{
    public partial class frmBoardAliases : Form
    {
        private List<string> CurrentAliases = new List<string>();
        private List<string> CurrentBOMs = new List<string>();
        private List<string> CurrentSchematics = new List<string>();

        private ToolTip SetterTip = new ToolTip()
        {
            AutoPopDelay = 5000,
            InitialDelay = 1000,
            ReshowDelay = 500,
            ShowAlways = true,
            UseAnimation = true,
            ToolTipIcon = ToolTipIcon.Info
        };

        public frmBoardAliases()
        {
            InitializeComponent();
            tbDatabaseView.Hide();
        }

        private void frmBoardAliases_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'pCBAAliasesDataSet.PCBAAliases' table. You can move, or remove it, as needed.
            this.pCBAAliasesTableAdapter.Fill(this.pCBAAliasesDataSet.PCBAAliases);

            SetterTip.SetToolTip(lnkBOMFile, "Go to -> " + lnkBOMFile.Text);
            SetterTip.SetToolTip(lnkSchematicFile, "Go to -> " + lnkSchematicFile.Text);

            txtPartNumber.Focus();
        }

        private void frmBoardAliases_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Modifiers == (Keys.LShiftKey | Keys.RControlKey))
            {
                dgvDatabaseTable.AllowUserToAddRows = !currAdmin;
                dgvDatabaseTable.AllowUserToDeleteRows = !currAdmin;
            }
        }

        private void txtPartNumber_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CurrentAliases.Clear();
                CurrentBOMs.Clear();
                CurrentSchematics.Clear();

                GetPartNumberDetailsAndAliases();
            }
        }

        private void GetPartNumberDetailsAndAliases()
        {
            using (var conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().HummingBirdConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT PartName, CommodityClass FROM [HummingBird].[dbo].[ItemMaster] " +
                    "WHERE [PartNumber] = @Pnum AND ([StockingType] <> 'O' AND [StockingType] <> 'U')", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", txtPartNumber.Text);

                    var reader = cmd.ExecuteReader();

                    if (reader.HasRows && reader.Read())
                    {
                        lblPartName.Text = lblPartName.Text.Replace("<NAME>", reader[0].ToString());
                        lblCommodityClass.Text = lblCommodityClass.Text.Replace("<CLASS>", reader[1].ToString());
                    }
                }
            }

            using (var conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT Alias, BOMPath, SchematicPath FROM [Repair].[dbo].[PCBAAliases] " +
                    "WHERE [TargetPartNumber] = @Pnum", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", txtPartNumber.Text);

                    var reader = cmd.ExecuteReader();

                    if (reader.HasRows && reader.Read())
                    {
                        CurrentAliases.Add(reader[0].ToString() ?? "empty alias ???");
                        CurrentBOMs.Add(reader[1]?.ToString() ?? "not set");
                        CurrentSchematics.Add(reader[2]?.ToString() ?? "not set");
                    }
                }
            }
        }

        #region Context Menu

        private void changeFilePathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var IsBOM = (sender as LinkLabel).Name.Contains("BOM");

            var ofd = new OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Title = $"Please choose the new {(IsBOM ? "BOM" : "Schematic")} file path...",
                AutoUpgradeEnabled = true,
                RestoreDirectory = true,
                Multiselect = false
            };

            ofd.ShowDialog();

            ofd.FileOk += delegate
            {
                using (var conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("UPDATE [Repair].[dbo].[PCBAAliases] SET " +
                        "BOMPath = @BomPath, SchematicPath = @SchPath " +
                        "WHERE [TargetPartNumber] = @Pnum", conn))
                    {
                        cmd.Parameters.AddWithValue("@Pnum", txtPartNumber.Text);
                        cmd.Parameters.AddWithValue("@BomPath", lnkBOMFile.Text);
                        cmd.Parameters.AddWithValue("@SchPath", lnkSchematicFile.Text);

                        var rowsAffected = cmd.ExecuteNonQuery();
                    }
                }
            };
        }

        private void addNewAliasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string input = Interaction.InputBox("Please input the new alias:", $"New {txtPartNumber.Text} Alias", 
                int.Parse(txtPartNumber.Text.Substring(0, txtPartNumber.Text.LastIndexOf('-'))).ToString(), 
                Screen.PrimaryScreen.WorkingArea.Width / 2, Screen.PrimaryScreen.WorkingArea.Height);

            lstbxAliases.Items.Add(input);
        }

        private void deleteAliasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lstbxAliases.Items.Remove(lstbxAliases.SelectedItem);
        }
        #endregion

        private void lstbxAliases_SelectedIndexChanged(object sender, EventArgs e)
        {
            lnkBOMFile.Text = CurrentBOMs[lstbxAliases.SelectedIndex-1];
            lnkSchematicFile.Text = CurrentBOMs[lstbxAliases.SelectedIndex-1];
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            //TODO: Save ALL changes, like new & removed aliases <- File Paths are easier to change.
        }
    }
}
