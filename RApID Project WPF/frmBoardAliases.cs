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

    public partial class frmBoardAliases : Form
    {
        private const string EMPTY_FILE_PATH = "Empty BOM Path...";
        private static bool FirstTimeToday = true;

        private List<string> CurrentAliases = new List<string>();
        private List<string> CurrentBOMs = new List<string>();
        private List<AssemblyLinkLabel> CurrentSchematics = new List<AssemblyLinkLabel>();

        private ToolTip SetterTip = new ToolTip()
        {
            AutoPopDelay = 5000,
            InitialDelay = 1000,
            ReshowDelay = 500,
            ShowAlways = true,
            UseAnimation = true,
            ToolTipIcon = ToolTipIcon.Info
        };
        private bool bBOM;
        private bool bSchematic;
        private int posX;
        private int posY;

        /// <summary>
        /// Technician Interface form to assign BOMs and Assembly files to a part number or alias thereof.
        /// </summary>
        public frmBoardAliases()
        {
            InitializeComponent();
            tbDatabaseView.Hide();

            posX = (int) (Width / 2.0);
            posY = (int) (Height / 2.0);
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

            this.pCBAAliasesTableAdapter.Fill(this.pCBAAliasesDataSet.PCBAAliases);

            txtPartNumber.Focus();
        }

        private void frmBoardAliases_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Modifiers == (Keys.LShiftKey | Keys.RControlKey))
            {
                dgvDatabaseTable.AllowUserToAddRows = !dgvDatabaseTable.AllowUserToAddRows;
                dgvDatabaseTable.AllowUserToDeleteRows = !dgvDatabaseTable.AllowUserToDeleteRows;
            }
        }

        private void txtPartNumber_KeyDown(object sender, KeyEventArgs e)
        {
            errorProvider.SetError(txtPartNumber, string.Empty);

            if (e.KeyCode == Keys.Enter)
            {
                lstbxAliases.Enabled = false;
                lnkBOMFile.Text = "BOMLink";
                lblPartName.Text = "Part Name: <NAME>";
                lblCommodityClass.Text = "Commodity Class: <CLASS>";
                CurrentAliases.Clear();
                CurrentBOMs.Clear();
                CurrentSchematics.Clear();
                lstbxAliases.DataSource = new List<string>();
                GetPartNumberDetailsAndAliases();
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
                CheckFileExists = true,
                CheckPathExists = true,
                Title = $"Please choose the new {(bBOM ? "BOM" : "Schematic")} file path...",
                AutoUpgradeEnabled = true,
                RestoreDirectory = true,
                Multiselect = false
            };

            ofd.ShowDialog();
            if (string.IsNullOrWhiteSpace(ofd.FileName)) return;
            if (bBOM)
            {
                lnkBOMFile.Text = ofd.SafeFileName;
                CurrentBOMs.Insert(lstbxAliases.SelectedIndex, ofd.FileName);
                bBOM = false;
            }
            else if (bSchematic)
            {
                var ext = ofd.SafeFileName.Split('.').Last();
                var img = new List<string>() { ".pdf", ".asc" }.Contains(ext) ? imgSchematics.Images[ext] : imgSchematics.Images[2];
                CurrentSchematics.Insert(lstbxAliases.SelectedIndex,
                    new AssemblyLinkLabel(ofd.FileName, ofd.SafeFileName, img)
                );
                bBOM = false;
            }

            using (var conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("UPDATE [Repair].[dbo].[PCBAAliases] SET " +
                    "BOMPath = @BomPath, SchematicPaths = @SchPath " +
                    "WHERE [TargetPartNumber] = @Pnum AND Alias = @alias", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", txtPartNumber.Text);
                    cmd.Parameters.AddWithValue("@BomPath", lnkBOMFile.Text);
                    var schematicPaths = "";
                    foreach (var @string in CurrentSchematics)
                    {
                        schematicPaths += @string + ",";
                    }
                    cmd.Parameters.AddWithValue("@SchPath", schematicPaths.Substring(0, schematicPaths.Length-1));
                    cmd.Parameters.AddWithValue("@alias", CurrentAliases[lstbxAliases.SelectedIndex].ToString());
                    var rowsAffected = cmd.ExecuteNonQuery();
                }
            }
        }

        private void AddNewDatabaseRow(AliasDatabaseItem itemType)
        {
            string input = "";

            switch (itemType)
            {
                default:
                case AliasDatabaseItem.Alias:
                    input = Interaction.InputBox("Please input the new alias:", $"New {txtPartNumber.Text} Alias",
                        int.Parse(txtPartNumber.Text.Substring(0, txtPartNumber.Text.LastIndexOf('-'))).ToString(),
                        posY, posX);
                    break;
                case AliasDatabaseItem.TargetPartNumber:
                    input = Interaction.InputBox("Please input the new part number:", $"New Target Part Number", "", posY, posX);
                    break;
            }

            if (string.IsNullOrWhiteSpace(input)) return;
            CurrentAliases.Add(input);

            CurrentBOMs.Add("BOMPath");
            CurrentSchematics.Add(new AssemblyLinkLabel("ASSYLink", "Assembly Display Text"));
            lnkBOMFile.Text = "BOMLink";
            flowSchematicLinks.Controls.Add(new AssemblyLinkLabel("<EMPTY>", "ASSYLink"));
            using (var conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("INSERT INTO PCBAAliases (TargetPartNumber, Alias, BOMPath, SchematicPaths) VALUES (@Pnum, @alias, @BomPath, @SchPaths)", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", itemType is AliasDatabaseItem.Alias ? txtPartNumber.Text : input);
                    cmd.Parameters.AddWithValue("@BomPath", "BOMLink");
                    cmd.Parameters.AddWithValue("@SchPaths", "ASSYLink");
                    cmd.Parameters.AddWithValue("@alias", input);
                    var rowsAffected = cmd.ExecuteNonQuery();
                }
            }

            lstbxAliases.DataSource = new List<string>();
            lstbxAliases.DataSource = CurrentAliases;
            lstbxAliases.SelectedItem = input;
        }

        private void btnNewPN_Click(object sender, EventArgs e) 
            => AddNewDatabaseRow(AliasDatabaseItem.TargetPartNumber);

        private void addNewAliasToolStripMenuItem_Click(object sender, EventArgs e) 
            => AddNewDatabaseRow(AliasDatabaseItem.Alias);

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
            CurrentSchematics.RemoveAt(lstbxAliases.SelectedIndex);
            CurrentAliases.RemoveAt(lstbxAliases.SelectedIndex);
            lstbxAliases.DataSource = new List<string>();
            lstbxAliases.DataSource = CurrentAliases;
            flowSchematicLinks.Controls.Clear();
        }
        #endregion

        #region ListBox
        private void lstbxAliases_SelectedIndexChanged(object sender, EventArgs e)
        {
            var lastSlashInPath = CurrentBOMs[lstbxAliases.SelectedIndex].LastIndexOf('\\')+1;
            if (lastSlashInPath > 0)
                lnkBOMFile.Text = CurrentBOMs?[lstbxAliases.SelectedIndex].Substring(lastSlashInPath) ?? EMPTY_FILE_PATH;

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
        }

        private void lnkSchematicFileTop_MouseDown(object sender, MouseEventArgs e)
        {
            bSchematic = true;
            bBOM = false;
        }

        private void lnkBOMFile_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !(string.IsNullOrEmpty(e.Link.ToString()) || e.Link.ToString().Equals("")))
            {
                try
                {
                    Process.Start(CurrentBOMs?[lstbxAliases.SelectedIndex]);
                }
                catch { }
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
                        lstbxAliases.Enabled = true;
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
                using (var cmd = new SqlCommand("SELECT Alias, BOMPath, SchematicPaths FROM [Repair].[dbo].[PCBAAliases] " +
                    "WHERE [TargetPartNumber] = @Pnum", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", txtPartNumber.Text);

                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        CurrentAliases.Add(reader[0].ToString() ?? "empty alias ???");
                        CurrentBOMs.Add(reader[1]?.ToString() ?? "not set");
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

            lstbxAliases.DataSource = CurrentAliases;
            if (lstbxAliases.Items.Count > 0) lstbxAliases.SelectedIndex = 0;
            else lstbxAliases.SelectedIndex = -1;
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
