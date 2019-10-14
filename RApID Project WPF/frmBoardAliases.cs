using Microsoft.VisualBasic;
using RApID_Project_WPF.CustomControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;

namespace RApID_Project_WPF
{
    /// <summary>
    /// Specifies the database item type.
    /// </summary>
    public enum AliasDatabaseItem
    {
        /// <summary> A board alias which points to another part number's BOM. </summary>
        Alias = 0,
        /// <summary> A part number which points to a BOM via its own "alias." </summary>
        TargetPartNumber = 1
    }

    /// <summary>
    /// Technician Interface form to assign BOMs and Assembly files to a part number or alias thereof.
    /// </summary>
    public partial class frmBoardAliases : Form
    {
        public bool WasEntryFound = false;
        public string BOMFileName = "";

        const string EMPTY_FILE_PATH = "Empty BOM Path...";
        static bool FirstTimeToday = true;

        List<string> CurrentPartNumbers = new List<string>();
        List<string> CurrentBOMs = new List<string>();
        List<AssemblyLinkLabel> CurrentSchematics = new List<AssemblyLinkLabel>();

        ToolTip SetterTip = new ToolTip()
        {
            AutoPopDelay = 5000,
            InitialDelay = 1000,
            ReshowDelay = 500,
            ShowAlways = true,
            UseAnimation = true,
            ToolTipIcon = ToolTipIcon.Info
        };
        bool bBOM;
        bool bSchematic;
        bool bNewSchematic;
        int selectedSchematicIndex;

        /// <summary>
        /// Default Ctor
        /// </summary>
        public frmBoardAliases(string partNumber = "")
        {
            InitializeComponent();
            tbDatabaseView.Hide();

            AssemblyLinkLabel.ChangeLink += lnkSchematicFile_MouseDown;

            if (!string.IsNullOrWhiteSpace(partNumber)) txtPartNumber.Text = partNumber;
        }

        private void frmBoardAliases_Load(object sender, EventArgs e)
        {
            if (FirstTimeToday)
            {
                var ans = MessageBox.Show("Please use this form to enter in the correct information to locate the files related to this repair item.",
                    "Locate BOM & Schematic Files", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (ans == DialogResult.No || ans == DialogResult.Cancel)
                {
                    try
                    { // TRIGGERED EXCEPTION EMAIL
                        throw new WhiningException($"Current User ({Environment.UserName}) said NO to helping me.");
                    }
                    catch (Exception ex)
                    {
                        Mailman.SendEmail($"{Environment.UserName} did not provide BOM assistance.",
                            $"Timestamp: {DateTime.Now}\n" +
                            $"Serial Number Mapper Data:\n" +
                            $"{SNMapperLib.csSerialNumberMapper.Instance.AsDataPackage()}\n", ex);
                        Close();
                    }
                }

                FirstTimeToday = false;
            }

            pCBAAliasesTableAdapter.Fill(pCBAAliasesDataSet.PCBAAliases);

            txtPartNumber.Focus();
            SetterTip.SetToolTip(btnChangeBOMPath, "Changes the BOM File path.");
            
            if(!string.IsNullOrWhiteSpace(txtPartNumber.Text))
            {
                HandleTextBoxEntry(new KeyEventArgs(Keys.Enter));
            }
        }

        private void HandleTextBoxEntry(KeyEventArgs e) {
            errorProvider.SetError(txtPartNumber, string.Empty);

            if (e.KeyCode == Keys.Enter)
            {
                lstbxPNHistory.Enabled = false;
                lnkBOMFile.Text = "BOMLink";
                lblPartName.Text = "Part Name: <NAME>";
                lblCommodityClass.Text = "Commodity Class: <CLASS>";
                CurrentPartNumbers.Clear();
                CurrentBOMs.Clear();
                CurrentSchematics.Clear();
                lstbxPNHistory.DataSource = new List<string>();
                GetPartNumberDetailsAndAliases();
            }
        }

        private void btnSearch_Click(object sender, EventArgs e) => HandleTextBoxEntry(new KeyEventArgs(Keys.Enter));
        private void txtPartNumber_KeyDown(object sender, KeyEventArgs e) => HandleTextBoxEntry(e);

        private void frmBoardAliases_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Modifiers == (Keys.LShiftKey | Keys.RControlKey))
            {
                dgvDatabaseTable.AllowUserToAddRows = !dgvDatabaseTable.AllowUserToAddRows;
                dgvDatabaseTable.AllowUserToDeleteRows = !dgvDatabaseTable.AllowUserToDeleteRows;
            }
        }

        private void tcDataViewer_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.pCBAAliasesTableAdapter.Fill(this.pCBAAliasesDataSet.PCBAAliases);
        }

        #region Context Menu

        private void changeFilePathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPartNumber.Text)) return;

            var ofd = new OpenFileDialog()
            {
                InitialDirectory = @"L:\EngDocumentation\Design\Electrical",
                CheckFileExists = true,
                CheckPathExists = true,
                Title = $"Please choose the new {(bBOM ? "BOM" : "Schematic")} file path...",
                AutoUpgradeEnabled = true,
                RestoreDirectory = true,
                Multiselect = false
            };

            if (ofd.ShowDialog() != DialogResult.OK ||                
                string.IsNullOrWhiteSpace(ofd.FileName)) return;

            if (bBOM)
            {
                lnkBOMFile.Text = ofd.SafeFileName;
                CurrentBOMs.Insert(lstbxPNHistory.SelectedIndex, ofd.FileName);
                bBOM = false;
            }
            else if (bSchematic)
            {
                var ext = ofd.SafeFileName.Split('.').Last();
                var img = imgSchematics.Images[(new List<string>() { ".pdf", ".asc" }.Contains(ext) ? ext : "other")];
                if (bNewSchematic) {
                    CurrentSchematics.Add(new AssemblyLinkLabel(ofd.FileName, ofd.SafeFileName, img));
                    bNewSchematic = false;
                } else {
                    CurrentSchematics.RemoveAt(selectedSchematicIndex-1);
                    CurrentSchematics.Insert(selectedSchematicIndex-1, new AssemblyLinkLabel(ofd.FileName, ofd.SafeFileName, img));
                }
                bBOM = false;
            }

            using (var conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("UPDATE [Repair].[dbo].[PCBAAliases] SET " +
                    "BOMPath = @BomPath, SchematicPaths = @SchPath " +
                    "WHERE [TargetPartNumber] = @Pnum", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", CurrentPartNumbers[lstbxPNHistory.SelectedIndex].ToString());
                    cmd.Parameters.AddWithValue("@BomPath", lnkBOMFile.Text);
                    var schematicPaths = "";
                    foreach (var schematic in CurrentSchematics)
                    {
                        schematicPaths += schematic.Link + ",";
                    }
                    cmd.Parameters.AddWithValue("@SchPath", schematicPaths.Substring(0, schematicPaths.Length-1));
                    var rowsAffected = cmd.ExecuteNonQuery();
                }
            }

            HandleTextBoxEntry(new KeyEventArgs(Keys.Enter));
        }

        private void deleteSchematicLinkToolStripMenuItem_Click(object sender, EventArgs e) {
            CurrentSchematics.RemoveAt(selectedSchematicIndex-1);
            using (var conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("UPDATE [Repair].[dbo].[PCBAAliases] SET " +
                    "BOMPath = @BomPath, SchematicPaths = @SchPath " +
                    "WHERE [TargetPartNumber] = @Pnum", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", CurrentPartNumbers[lstbxPNHistory.SelectedIndex].ToString());
                    cmd.Parameters.AddWithValue("@BomPath", lnkBOMFile.Text);
                    var schematicPaths = "";
                    foreach (var schematic in CurrentSchematics)
                    {
                        schematicPaths += schematic.Link + ",";
                    }
                    cmd.Parameters.AddWithValue("@SchPath", schematicPaths.Substring(0, schematicPaths.Length - 1));
                    var rowsAffected = cmd.ExecuteNonQuery();
                }
            }

            HandleTextBoxEntry(new KeyEventArgs(Keys.Enter));
        }

        private ToolStripMenuItem AddNewSchematicLink = new ToolStripMenuItem("Add New Schematic Link...");
        private void flowSchematicLinks_MouseDown(object sender, MouseEventArgs e)
        {
            //TODO: Offer to add new schematic link
            var tempMenuItem = cxmnuLinkMenu.Items.Add(AddNewSchematicLink);
            AddNewSchematicLink.Click += AddNewSchematicLink_Click;

            cxmnuLinkMenu.Show(MousePosition.X, MousePosition.Y);
            cxmnuLinkMenu.Closed += cxmnuLinkMenu_Closed;
        }

        private void AddNewSchematicLink_Click(object sender, EventArgs e)
        {
            bNewSchematic = true;
            bSchematic = true;
            bBOM = false;
            changeFilePathToolStripMenuItem_Click(sender, e);
        }

        private void cxmnuLinkMenu_Closed(object sender, EventArgs e)
        {
            cxmnuLinkMenu.Items.Remove(AddNewSchematicLink);
            cxmnuLinkMenu.Closed -= cxmnuLinkMenu_Closed;
        }

        private void AddNewDatabaseRow()
        {
            var input = Interaction.InputBox("Please input the new part number:", $"New Target Part Number", 
                txtPartNumber.Text, Left + (Width / 2) - 200, Top + (Height / 2) - 100);

            if (string.IsNullOrWhiteSpace(input)) return;

            CurrentPartNumbers.Add(input);
            CurrentBOMs.Add("BOMPath");
            CurrentSchematics.Add(new AssemblyLinkLabel("ASSYLink", "Assembly Display Text"));
            lnkBOMFile.Text = "BOMLink";
            flowSchematicLinks.Controls.Add(new AssemblyLinkLabel("<EMPTY>", "ASSYLink"));
            using (var conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("INSERT INTO PCBAAliases (TargetPartNumber, BOMPath, SchematicPaths) VALUES (@Pnum, @BomPath, @SchPaths)", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", input);
                    cmd.Parameters.AddWithValue("@BomPath", "BOMLink");
                    cmd.Parameters.AddWithValue("@SchPaths", "ASSYLink");
                    var rowsAffected = cmd.ExecuteNonQuery();
                }
            }

            lstbxPNHistory.DataSource = new List<string>();
            lstbxPNHistory.DataSource = CurrentPartNumbers;
            lstbxPNHistory.SelectedItem = input;
        }

        private void btnNewPN_Click(object sender, EventArgs e) => AddNewDatabaseRow();

        private void deletePartNumberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("DELETE FROM PCBAAliases WHERE TargetPartNumber = @Pnum", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", txtPartNumber.Text);
                    var rowsAffected = cmd.ExecuteNonQuery();
                }
            }

            CurrentBOMs.RemoveAt(lstbxPNHistory.SelectedIndex);
            CurrentSchematics.RemoveAt(lstbxPNHistory.SelectedIndex);
            CurrentPartNumbers.RemoveAt(lstbxPNHistory.SelectedIndex);
            lstbxPNHistory.DataSource = new List<string>();
            lstbxPNHistory.DataSource = CurrentPartNumbers;
            flowSchematicLinks.Controls.Clear();
        }
        #endregion

        #region ListBox
        private void lstbxPNHistory_SelectedIndexChanged(object sender, EventArgs e)
        {
            var lastSlashInPath = CurrentBOMs[lstbxPNHistory.SelectedIndex].LastIndexOf('\\')+1;
            if (lastSlashInPath > 0)
                lnkBOMFile.Text = CurrentBOMs?[lstbxPNHistory.SelectedIndex].Substring(lastSlashInPath) ?? EMPTY_FILE_PATH;

            SetterTip.SetToolTip(lnkBOMFile, "Go to -> " + lnkBOMFile.Text);

            flowSchematicLinks.Controls.Clear();
            foreach (var schematic in CurrentSchematics)
            {
                var IsValidFile = File.Exists(schematic.Link);
                //Console.WriteLine($"The schematic ({schematic}) {(IsValidFile ? "does" : "doesn't")} exist.");
                if (!IsValidFile) continue;

                SetterTip.SetToolTip(schematic, "Open file:\n" + schematic.Link);

                flowSchematicLinks.Controls.Add(schematic);
            }
        }
        #endregion

        #region Link Label
        private void lnkBOMFile_Click(object sender, MouseEventArgs e)
        {
            bBOM = true;
            bSchematic = false;

            if(e.Button == MouseButtons.Right)
            {
                cxmnuLinkMenu.Show(MousePosition.X, MousePosition.Y);
            }
        }

        private void lnkSchematicFile_MouseDown(object sender, MouseEventArgs e)
        {
            bSchematic = true;
            bBOM = false;

            if (e.Button == MouseButtons.Right)
            {
                selectedSchematicIndex = 0;
                foreach(Control c in flowSchematicLinks.Controls)
                {
                    var screenPoint = PointToScreen(c.Location);
                    if (screenPoint.X == MousePosition.X && screenPoint.Y == MousePosition.Y)
                        break;
                    else selectedSchematicIndex++;
                }
                cxmnuLinkMenu.Show(MousePosition.X, MousePosition.Y);
            }
        }

        private void lnkBOMFile_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !(string.IsNullOrEmpty(e.Link.ToString()) || e.Link.ToString().Equals("")))
            {
                try
                {
                    Process.Start(CurrentBOMs?[lstbxPNHistory.SelectedIndex]);
                }
                catch (Exception ex)
                {
                    csExceptionLogger.csExceptionLogger.Write("frmBoardAliases::lnkBOMFile_LinkClicked", ex);
                }
            }
        }
        #endregion

        private void GetPartNumberDetailsAndAliases()
        {
            var validPartNumber = false;

            using (var conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().HummingBirdConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT PartName, CommodityClass FROM [HummingBird].[dbo].[ItemMaster] " +
                    "WHERE [PartNumber] = @Pnum AND ([StockingType] <> 'O' AND [StockingType] <> 'U')", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", txtPartNumber.Text);

                    var reader = cmd.ExecuteReader();
                    validPartNumber = reader.HasRows;
                    if (validPartNumber && reader.Read())
                    {
                        lblPartName.Text = lblPartName.Text.Replace("<NAME>", reader[0].ToString());
                        lblCommodityClass.Text = lblCommodityClass.Text.Replace("<CLASS>", reader[1].ToString());
                        lstbxPNHistory.Enabled = true;
                    }
                }
            }

            if (!validPartNumber) {
                errorProvider.SetError(txtPartNumber, "Part Number not found in ItemMaster table!");
                return;
            }

            using (var conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT TargetPartNumber, BOMPath, SchematicPaths FROM [Repair].[dbo].[PCBAAliases] " +
                    "WHERE [TargetPartNumber] = @Pnum", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", txtPartNumber.Text);

                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        CurrentPartNumbers.Add(reader[0].ToString() ?? "empty alias ???");
                        CurrentBOMs.Add(reader[1]?.ToString() ?? "not set");
                        lnkBOMFile.Text = reader[1].ToString();
                        var sb = new StringBuilder();
                        sb.Append("Reading New Schematic Files {");
                        foreach (var @string in reader[2].ToString().Split(','))
                        {
                            var name = @string.Substring(@string.LastIndexOf('\\')+1) ?? "not set";
                            var ext = @string.Substring(@string.LastIndexOf('.') + 1) ?? "other";
                            var img = imgSchematics.Images[
                                    (!string.IsNullOrWhiteSpace(ext) && new List<string>() { "pdf", "asc" }.Contains(ext)
                                    ? ext : "other")
                                ];

                            sb.Append("\n");
                            sb.Append($"\t[AssemblyLinkItem] ==> {@string}");
                            CurrentSchematics.Add(new AssemblyLinkLabel(@string, name, img));
                        }
                        sb.AppendLine("\n}");
                        Console.Write(sb.ToString());
                    }
                }
            }

            lstbxPNHistory.DataSource = CurrentPartNumbers;
            if (lstbxPNHistory.Items.Count > 0) lstbxPNHistory.SelectedIndex = 0;
            else lstbxPNHistory.SelectedIndex = -1;
        }

        private void btnChangeBOMPath_Click(object sender, EventArgs e)
        {
            bBOM = true;
            bSchematic = false;

            changeFilePathToolStripMenuItem_Click(null, null);
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pCBAAliasesTableAdapter.Fill(pCBAAliasesDataSet.PCBAAliases);
        }
    }

    /// <summary>
    /// Complains to the devs that a technician didn't select a new BOM file.
    /// </summary>
    public class WhiningException : Exception
    {
        /// <summary> Simplify the stack trace to easily distinguish this exception. </summary>
        public override string StackTrace => "Inside of frmBoardAlias class, in the constructor.";

        /// <summary>
        /// Default ctor -- supplies a message to base <see cref="Exception"/>.
        /// </summary>
        /// <param name="message">The message to tie in to this instance of <see cref="Exception"/>.</param>
        public WhiningException(string message = "") : base(message) {}
    }
}
