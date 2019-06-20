using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace RApID_Project_WPF
{
    public partial class frmBoardAliases : Form
    {
        private List<string> CurrentAliases = new List<string>();
        private List<string> CurrentBOMs = new List<string>();
        private List<string> CurrentSchematicsTop = new List<string>();
        private List<string> CurrentSchematicsBottom = new List<string>();

        private ToolTip SetterTip = new ToolTip()
        {
            AutoPopDelay = 5000,
            InitialDelay = 1000,
            ReshowDelay = 500,
            ShowAlways = true,
            UseAnimation = true,
            ToolTipIcon = ToolTipIcon.Info
        };
        private bool currAdmin;
        private bool bBOM;
        private bool bBottom;
        private bool bTop;
        private int posX;
        private int posY;

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
            SetterTip.SetToolTip(lnkSchematicFileTop, "Go to -> " + lnkSchematicFileTop.Text);

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
                lstbxAliases.Enabled = false;
                lnkBOMFile.Text = "BOMLink";
                lnkSchematicFileBottom.Text = "ASSYLink";
                lnkSchematicFileTop.Text = "ASSYLink";
                lblPartName.Text = "Part Name: <NAME>";
                lblCommodityClass.Text = "Commodity Class: <CLASS>";
                CurrentAliases.Clear();
                CurrentBOMs.Clear();
                CurrentSchematicsTop.Clear();
                CurrentSchematicsBottom.Clear();
                lstbxAliases.DataSource = new List<string>();
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
                        lstbxAliases.Enabled = true;
                    }
                }
            }

            using (var conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT Alias, BOMPath, SchematicPathTop, SchematicPathBottom FROM [Repair].[dbo].[PCBAAliases] " +
                    "WHERE [TargetPartNumber] = @Pnum", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", txtPartNumber.Text);

                    var reader = cmd.ExecuteReader();

                    while (reader.Read()) { 
                        CurrentAliases.Add(reader[0].ToString() ?? "empty alias ???");
                        CurrentBOMs.Add(reader[1]?.ToString() ?? "not set");
                        CurrentSchematicsTop.Add(reader[2]?.ToString() ?? "not set");
                        CurrentSchematicsBottom.Add(reader[3]?.ToString() ?? "not set");
                    }
                }
            }

            lstbxAliases.DataSource = CurrentAliases;
            if( lstbxAliases.Items.Count>0) lstbxAliases.SelectedIndex = 0;
            else lstbxAliases.SelectedIndex = -1;
        }

        #region Context Menu

        private void changeFilePathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(txtPartNumber.Text)) return;
            //var IsBOM = sender.ToString().Contains("BOM");

            //var ofd = new OpenFileDialog()
            //{
            //    CheckFileExists = true,
            //    CheckPathExists = true,
            //    Title = $"Please choose the new {(IsBOM ? "BOM" : "Schematic")} file path...",
            //    AutoUpgradeEnabled = true,
            //    RestoreDirectory = true,
            //    Multiselect = false
            //};

            var ofd = new OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Title = $"Please choose the new file path...",
                AutoUpgradeEnabled = true,
                RestoreDirectory = true,
                Multiselect = false
            };

            ofd.ShowDialog();
            if (String.IsNullOrWhiteSpace(ofd.FileName)) return;
            if (bBOM)
            {
                lnkBOMFile.Text = ofd.FileName;
                CurrentBOMs.Insert(lstbxAliases.SelectedIndex, ofd.FileName);
                bBOM = false;
            }
            else if (bTop)
            {
                lnkSchematicFileTop.Text = ofd.FileName;
                CurrentSchematicsTop.Insert(lstbxAliases.SelectedIndex, ofd.FileName);
                bTop = false;
            }
            else if (bBottom)
            {
                lnkSchematicFileBottom.Text = ofd.FileName;
                CurrentSchematicsBottom.Insert(lstbxAliases.SelectedIndex, ofd.FileName);
                bBottom = false;
            }
            
            using (var conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("UPDATE [Repair].[dbo].[PCBAAliases] SET " +
                        "BOMPath = @BomPath, SchematicPathTop = @SchPath, SchematicPathBottom = @SchPathB " +
                        "WHERE [TargetPartNumber] = @Pnum AND Alias = @alias", conn))
                    {
                        cmd.Parameters.AddWithValue("@Pnum", txtPartNumber.Text);
                        cmd.Parameters.AddWithValue("@BomPath", lnkBOMFile.Text);
                        cmd.Parameters.AddWithValue("@SchPath", lnkSchematicFileTop.Text);
                        cmd.Parameters.AddWithValue("@SchPathB", lnkSchematicFileBottom.Text);
                        cmd.Parameters.AddWithValue("@alias", CurrentAliases[ lstbxAliases.SelectedIndex].ToString());
                        var rowsAffected = cmd.ExecuteNonQuery();
                    }
                }
        }

        private void addNewAliasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string input = Interaction.InputBox("Please input the new alias:", $"New {txtPartNumber.Text} Alias", 
                int.Parse(txtPartNumber.Text.Substring(0, txtPartNumber.Text.LastIndexOf('-'))).ToString(), 
                posY, posX);
            if (String.IsNullOrWhiteSpace(input)) return;
            CurrentAliases.Add(input);
            
            CurrentBOMs.Add("BOMPath");
            CurrentSchematicsBottom.Add("ASSYLink");
            CurrentSchematicsTop.Add("ASSYLink");
            lnkBOMFile.Text = "BOMLink";
            lnkSchematicFileBottom.Text = "ASSYLink";
            lnkSchematicFileTop.Text = "ASSYLink";
            using (var conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("INSERT INTO PCBAAliases (TargetPartNumber, Alias, BOMPath, SchematicPathTop, SchematicPathBottom) VALUES (@Pnum, @alias, @BomPath, @SchPath, @SchPathB)", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", txtPartNumber.Text);
                    cmd.Parameters.AddWithValue("@BomPath", "BOMLink");
                    cmd.Parameters.AddWithValue("@SchPath", "ASSYLink");
                    cmd.Parameters.AddWithValue("@SchPathB", "ASSYLink");
                    cmd.Parameters.AddWithValue("@alias", input);
                    var rowsAffected = cmd.ExecuteNonQuery();
                }
            }

            lstbxAliases.DataSource = new List<string>();
            lstbxAliases.DataSource = CurrentAliases;
            lstbxAliases.SelectedItem = input;
        }

        private void deleteAliasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("DELETE FROM PCBAAliases WHERE TargetPartNumber = @Pnum AND Alias = @alias", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", txtPartNumber.Text);
                    cmd.Parameters.AddWithValue("@alias", CurrentAliases[lstbxAliases.SelectedIndex]);
                    var rowsAffected = cmd.ExecuteNonQuery();
                }
            }

            CurrentBOMs.RemoveAt(lstbxAliases.SelectedIndex);
            CurrentSchematicsTop.RemoveAt(lstbxAliases.SelectedIndex);
            CurrentSchematicsBottom.RemoveAt(lstbxAliases.SelectedIndex);
            CurrentAliases.RemoveAt(lstbxAliases.SelectedIndex);
            lstbxAliases.DataSource = new List<string>();
            lstbxAliases.DataSource = CurrentAliases;
        }
        #endregion

        private void lstbxAliases_SelectedIndexChanged(object sender, EventArgs e)
        {
            lnkBOMFile.Text = CurrentBOMs[lstbxAliases.SelectedIndex];
            lnkSchematicFileTop.Text = CurrentSchematicsTop[lstbxAliases.SelectedIndex];
            lnkSchematicFileBottom.Text = CurrentSchematicsBottom[lstbxAliases.SelectedIndex];
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            //TODO: Save ALL changes, like new & removed aliases <- File Paths are easier to change.
        }

        private void lnkBOMFile_Click(object sender, MouseEventArgs e)
        {
            bBOM = true;
            bTop = false;
            bBottom = false;
        }

        private void lnkSchematicFileTop_MouseDown(object sender, MouseEventArgs e)
        {
            bTop = true;
            bBOM = false;
            bBottom = false;
        }

        private void lnkSchematicFileBottom_MouseDown(object sender, MouseEventArgs e)
        {
            bBOM = false;
            bTop = false;
            bBottom = true;
        }

        private void lstbxAliases_MouseDown(object sender, MouseEventArgs e)
        {
            posX = e.X;
            posY = e.Y;
        }

        private void lnkFile_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                try
                {
                    Process.Start((sender as LinkLabel).Text);
                }
                catch { }
            }
        }

        private void tcDataViewer_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.pCBAAliasesTableAdapter.Fill(this.pCBAAliasesDataSet.PCBAAliases);
        }
    }
}
