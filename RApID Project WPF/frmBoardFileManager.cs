using EricStabileLibrary;
using Microsoft.VisualBasic;
using RApID_Project_WPF.CustomControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        PartNumber = 1
    }

    /// <summary>
    /// Technician Interface form to assign BOMs and Assembly files to a part number or alias thereof.
    /// </summary>
    public partial class frmBoardFileManager : Form
    {
        public bool WasEntryFound = false;
        public string BOMFileName = "";

        const string ELEC_ROOT_DIR = @"L:\EngDocumentation\Design\Electrical";
        const string EMPTY_FILE_PATH = "BOMPath";
        static bool FirstTimeToday = true;

        BindingList<string> CurrentPartNumbers { get; set; } = new BindingList<string>();
        List<string> CurrentBOMs { get; set; } = new List<string>();
        List<AssemblyLinkLabel> CurrentSchematics { get; set; } = new List<AssemblyLinkLabel>();

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
        bool DirectDialog = false;
        int selectedSchematicIndex;

        /// <summary>
        /// Default Ctor
        /// </summary>
        /// <param name="partNumber">The part number to start the dialog with.</param>
        /// <param name="directDialog">Determines if the tech help yes/no dialog will show</param>
        public frmBoardFileManager(string partNumber = "", bool directDialog = false)
        {
            InitializeComponent();
            tbDatabaseView.Hide();
            lstbxPNHistory.DataSource = CurrentPartNumbers;

            AssemblyLinkLabel.ChangeLink += lnkSchematicFile_MouseDown;

            if (!string.IsNullOrWhiteSpace(partNumber)) txtPartNumber.Text = partNumber;
            DirectDialog = directDialog;
        }

        private void frmBoardAliases_Load(object sender, EventArgs e)
        {
            if (!DirectDialog && FirstTimeToday)
            {
                DialogResult ans = MessageBox.Show("Please use this form to enter in the correct information to locate the files related to this repair item.",
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

            pCBAAliasesTableAdapter.Fill(pCBAliasDataSet.PCBAAliases);

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
                ResetAllData();
                GetPartNumberDetailsAndAliases();
            }
        }

        private void btnSearch_Click(object sender, EventArgs e) => HandleTextBoxEntry(new KeyEventArgs(Keys.Enter));
        private void btnAddNewPN_Click(object sender, EventArgs e)
        {
            string partNumber = Interaction.InputBox("Please input the new part number:", $"New Target Part Number",
                txtPartNumber.Text, Left + (Width / 2) - 200, Top + (Height / 2) - 100);

            if (string.IsNullOrWhiteSpace(partNumber)) return;
            AddNewDatabaseRow(partNumber);
        }
        private void btnReset_Click(object sender, EventArgs e) => ResetAllData(resetPN: true);

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
            => pCBAAliasesTableAdapter.Fill(pCBAliasDataSet.PCBAAliases);

        #region Context Menu

        private void changeFilePathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPartNumber.Text)) return;

            OpenFileDialog ofd = new OpenFileDialog()
            {
                InitialDirectory = ELEC_ROOT_DIR,
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
                if(lstbxPNHistory.Items.Count == 0)
                {
                    DialogResult answer = MessageBox.Show("No current data exists for this part number.\n" +
                        "Would you like to create a new entry to save your selected BOM file?\n" +
                        "Pressing [No] will reset the form.",
                        "Prompt to Create New Database Entry",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);
                    if (answer==DialogResult.Yes)
                        AddNewDatabaseRow(txtPartNumber.Text, ofd.SafeFileName, ofd.FileName);
                    else if (answer==DialogResult.No)
                        ResetAllData(resetPN: true);
                    return;
                }
                CurrentBOMs.Insert(lstbxPNHistory.SelectedIndex, ofd.FileName);
                bBOM = false;
            }
            else if (bSchematic)
            {
                var ext = ofd.SafeFileName.Split('.').Last();
                System.Drawing.Image img = imgSchematics.Images[(new List<string>() { ".pdf", ".asc" }.Contains(ext) ? ext : "other")];
                if (bNewSchematic) {
                    CurrentSchematics.Add(new AssemblyLinkLabel(ofd.FileName, ofd.SafeFileName, img));
                    bNewSchematic = false;
                } else {
                    CurrentSchematics.RemoveAt(selectedSchematicIndex-1);
                    CurrentSchematics.Insert(selectedSchematicIndex-1, new AssemblyLinkLabel(ofd.FileName, ofd.SafeFileName, img));
                }
                bBOM = false;
            }

            using (SqlConnection conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("UPDATE [Repair].[dbo].[PCBAAliases] SET " +
                    "BOMPath = @BomPath, BOMFileName = @BOMName, SchematicPaths = @SchPath" +
                    "WHERE [PartNumber] = @Pnum", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", CurrentPartNumbers[lstbxPNHistory.SelectedIndex].ToString());
                    cmd.Parameters.AddWithValue("@BomPath", CurrentBOMs[lstbxPNHistory.SelectedIndex]);
                    cmd.Parameters.AddWithValue("@BOMName", lnkBOMFile.Text);
                    var schematicPaths = "";
                    foreach (AssemblyLinkLabel schematic in CurrentSchematics)
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
            using (SqlConnection conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("UPDATE [Repair].[dbo].[PCBAAliases] SET " +
                    "SchematicPaths = @SchPath " +
                    "WHERE [PartNumber] = @Pnum", conn))
                {
                    var schematicPaths = "";
                    foreach (AssemblyLinkLabel schematic in CurrentSchematics)
                    {
                        schematicPaths += schematic.Link + ",";
                    }
                    if(string.IsNullOrWhiteSpace(schematicPaths) || schematicPaths.Length <= 1)
                        cmd.Parameters.AddWithValue("@SchPath", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@SchPath", schematicPaths.Substring(0, schematicPaths.Length - 1));

                    int rowsAffected = cmd.ExecuteNonQuery();
                }
            }

            HandleTextBoxEntry(new KeyEventArgs(Keys.Enter));
        }

        private ToolStripMenuItem AddNewSchematicLink = new ToolStripMenuItem("Add New Schematic Link...");
        private void flowSchematicLinks_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var tempMenuItem = cxmnuLinkMenu.Items.Add(AddNewSchematicLink);
                AddNewSchematicLink.Click += AddNewSchematicLink_Click;

                cxmnuLinkMenu.Show(MousePosition.X, MousePosition.Y);
                cxmnuLinkMenu.Closed += cxmnuLinkMenu_Closed;
            }
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

        /// <summary>
        /// Will reset all entered data.
        /// </summary>
        /// <param name="resetPN">Reset the PN?</param>
        private void ResetAllData(bool resetPN = false)
        {
            if (resetPN) txtPartNumber.Clear();
            lnkBOMFile.Text = "BOMLink";
            lblPartName.Text = "Part Name: <NAME>";
            lblCommodityClass.Text = "Commodity Class: <CLASS>";
            CurrentBOMs.Clear();
            CurrentSchematics.Clear();
        }

        private void AddNewDatabaseRow(string partNumber, string initialBOM = "BOMLink", string initialBOMPath = "BOMPath")
        {
            CurrentPartNumbers.Add(partNumber);
            CurrentBOMs.Add(initialBOMPath);
            CurrentSchematics.Add(new AssemblyLinkLabel("ASSYLink", "Assembly Display Text"));
            lnkBOMFile.Text = initialBOM;
            flowSchematicLinks.Controls.Add(new AssemblyLinkLabel("<EMPTY>", "ASSYLink"));
            using (SqlConnection conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("INSERT INTO PCBAAliases (PartNumber, BOMPath, BOMFileName, SchematicPaths) " +
                    "VALUES (@Pnum, @BomPath, @BomName, @SchPaths)", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", partNumber);
                    cmd.Parameters.AddWithValue("@BomPath", initialBOMPath);
                    cmd.Parameters.AddWithValue("@BomName", initialBOM);
                    cmd.Parameters.AddWithValue("@SchPaths", "ASSYLink");
                    var rowsAffected = cmd.ExecuteNonQuery();
                }
            }

            lstbxPNHistory.SelectedItem = partNumber;
        }

        private void deletePartNumberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("DELETE FROM PCBAAliases WHERE PartNumber = @Pnum", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", txtPartNumber.Text);
                    var rowsAffected = cmd.ExecuteNonQuery();
                }
            }

            CurrentBOMs.RemoveAt(lstbxPNHistory.SelectedIndex);
            CurrentSchematics.RemoveAt(lstbxPNHistory.SelectedIndex);
            CurrentPartNumbers.RemoveAt(lstbxPNHistory.SelectedIndex);
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
            foreach (AssemblyLinkLabel schematic in CurrentSchematics)
            {
                //Console.WriteLine($"The schematic ({schematic}) {(IsValidFile ? "does" : "doesn't")} exist.");
                if (!File.Exists(schematic.Link)) continue;

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
                    System.Drawing.Point screenPoint = PointToScreen(c.Location);
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
                    Console.WriteLine($"[{nameof(lnkBOMFile)}::Clicked] ==> Path: \"{lnkBOMFile.Text}\"");
                    Process.Start(CurrentBOMs[lstbxPNHistory.SelectedIndex]);
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

            using (SqlConnection conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().HummingBirdConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT PartName, CommodityClass FROM [HummingBird].[dbo].[ItemMaster] " +
                    "WHERE [PartNumber] = @Pnum AND ([StockingType] <> 'O' AND [StockingType] <> 'U')", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", txtPartNumber.Text);

                    SqlDataReader reader = cmd.ExecuteReader();
                    validPartNumber = reader.HasRows;
                    if (validPartNumber && reader.Read())
                    {
                        lblPartName.Text = lblPartName.Text.Replace("<NAME>", reader[0].ToString());
                        lblCommodityClass.Text = lblCommodityClass.Text.Replace("<CLASS>", reader[1].ToString());
                    }
                }
            }

            if (!validPartNumber) {
                errorProvider.SetError(txtPartNumber, "Part Number not found in ItemMaster table!");
                return;
            }

            using (SqlConnection conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT PartNumber, BOMPath, BOMFileName, SchematicPaths FROM [Repair].[dbo].[PCBAAliases] " +
                    "WHERE [PartNumber] = @Pnum", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", txtPartNumber.Text);

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        CurrentPartNumbers.EnsureElement(reader[0].ToString() ?? "empty alias ???");
                        CurrentBOMs.Add(reader[1]?.ToString() ?? "not set");
                        lnkBOMFile.Text = reader[2].ToString();
                        StringBuilder sb = new StringBuilder();
                        sb.Append("Reading New Schematic Files {");
                        foreach (var @string in reader[3].ToString().Split(','))
                        {
                            var name = @string.Substring(@string.LastIndexOf('\\')+1) ?? "not set";
                            var ext = @string.Substring(@string.LastIndexOf('.') + 1) ?? "other";
                            System.Drawing.Image img = imgSchematics.Images[
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
            pCBAAliasesTableAdapter.Fill(pCBAliasDataSet.PCBAAliases);
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
