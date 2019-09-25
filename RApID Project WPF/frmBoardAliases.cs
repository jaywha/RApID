using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace RApID_Project_WPF
{
    public partial class frmBoardAliases : Form
    {
        private const string EMPTY_FILE_PATH = "Empty BOM Path...";
        private static bool FirstTimeToday = true;

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
                        throw new Exception($"Current User ({Environment.UserName}) said NO to helping me.");
                    }
                    catch (Exception ex)
                    {
                        Mailman.SendEmail($"{Environment.UserName} did not provide BOM assistance.",
                            $"Timestamp: {DateTime.Now}\nSerial Number Mapper Data:\n{SNMapperLib.csSerialNumberMapper.Instance.AsDataPackage()}\n", ex);
                        Close();
                    }
                }

                FirstTimeToday = false;
            }

            this.pCBAAliasesTableAdapter.Fill(this.pCBAAliasesDataSet.PCBAAliases);

            SetterTip.SetToolTip(lnkBOMFile, "Go to -> " + lnkBOMFile.Text);
            // need to do each AssemblyLinkItem at their initializations

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
                lstvwSchematics.Items.Add(new AssemblyLinkItem("ASSYLink", "Assembly Link", 0));
                lstvwSchematics.Items.Add(new AssemblyLinkItem("ASSYLink", "Assembly Link", "asc"));
                lstvwSchematics.Items.Add(new AssemblyLinkItem("ASSYLink", "Assembly Link"));
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
                CurrentSchematics.Insert(lstbxAliases.SelectedIndex, ofd.FileName);
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

        private void addNewAliasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string input = Interaction.InputBox("Please input the new alias:", $"New {txtPartNumber.Text} Alias",
                int.Parse(txtPartNumber.Text.Substring(0, txtPartNumber.Text.LastIndexOf('-'))).ToString(),
                posY, posX);
            if (String.IsNullOrWhiteSpace(input)) return;
            CurrentAliases.Add(input);

            CurrentBOMs.Add("BOMPath");
            CurrentSchematics.Add("ASSYLink");
            lnkBOMFile.Text = "BOMLink";
            lstvwSchematics.Items.Add(new AssemblyLinkItem("<EMPTY>", "ASSYLink"));
            using (var conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("INSERT INTO PCBAAliases (TargetPartNumber, Alias, BOMPath, SchematicPaths) VALUES (@Pnum, @alias, @BomPath, @SchPaths)", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", txtPartNumber.Text);
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
            lstvwSchematics.Items.Clear();
        }
        #endregion

        #region ListBox
        private void lstbxAliases_SelectedIndexChanged(object sender, EventArgs e)
        {
            var lastSlashInPath = CurrentBOMs[lstbxAliases.SelectedIndex].LastIndexOf('\\');
            if (lastSlashInPath > 0)
                lnkBOMFile.Text = CurrentBOMs?[lstbxAliases.SelectedIndex].Substring(lastSlashInPath) ?? EMPTY_FILE_PATH;

            foreach (var schematic in CurrentSchematics)
            {
                if (!Directory.Exists(schematic)) continue;

                var assyItem = 
                    new AssemblyLinkItem(
                    link: schematic,
                    displayText: schematic.Substring(schematic.LastIndexOf('\\')),
                    imageKey: schematic.Substring(schematic.LastIndexOf('.'))) {
                        ToolTipText = "Open file:\n" + schematic
                    };
                lstvwSchematics.Items.Add(schematic);
            }
        }

        private void lstbxAliases_MouseDown(object sender, MouseEventArgs e)
        {
            posX = e.X;
            posY = e.Y;
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

        private void lnkFile_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !(string.IsNullOrEmpty(e.Link.ToString()) || e.Link.ToString().Equals("")))
            {
                try
                {
                    Process.Start((sender as LinkLabel).Text);
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
                        foreach (var @string in reader[2].ToString().Split(','))
                        {
                            CurrentSchematics.Add(@string ?? "not set");
                        }
                    }
                }
            }

            lstbxAliases.DataSource = CurrentAliases;
            if (lstbxAliases.Items.Count > 0) lstbxAliases.SelectedIndex = 0;
            else lstbxAliases.SelectedIndex = -1;
        }

        /// <summary>
        /// Will return the BOM file and any schematic files if the part number has them.
        /// </summary>
        /// <param name="partNumber">The scanned part number (or derived from serial number)</param>
        /// <returns>A ValueTuple with all files available.</returns>
        public static (string BOM, List<string> ASSYS) FindFilesFor(string partNumber)
        {
            using (var conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT Alias, BOMPath, SchematicPaths FROM [Repair].[dbo].[PCBAAliases] " +
                    "WHERE [TargetPartNumber] = @Pnum", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", partNumber);

                    var reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        var dbBOM = reader[1].ToString();

                        var filePaths = reader[2].ToString().Split(',');
                        return (
                            BOM: dbBOM,
                            ASSYS: new List<string>() { filePaths[0] ?? "UNSET", filePaths[1] ?? "UNSET" }
                        );
                    }
                }
            }

            return ("", new List<string>() { "EMPTY" });
        }

        private void lstvwSchematics_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if(lstvwSchematics.SelectedItems != null && lstvwSchematics.SelectedItems.Count > 0)
            {
                (lstvwSchematics.SelectedItems[0] as AssemblyLinkItem).Activate();
            }
        }
    }

    /// <summary>
    /// Custom <see cref="ListViewItem"/> implementation for ease of use.
    /// </summary>
    public class AssemblyLinkItem : ListViewItem
    {
        /// <summary> URL or File Path </summary>
        public string Link { get; set; }
        /// <summary> Text to show -- can be unrelated to link </summary>
        public string DisplayText { get; set; }

        #region Constructors
        /// <summary>
        /// Default ctor -- will also execute base constructor.
        /// </summary>
        /// <param name="link">[Optional] Sets this <see cref="Link"/></param>
        /// <param name="displayText">[Optional] Sets this <see cref="DisplayText"/></param>
        /// <remarks>Deleagates to a default Image Index ctor</remarks>
        public AssemblyLinkItem(string link = "", string displayText = "") 
            : this(link, displayText, 2) {}

        /// <summary>
        /// Image Index ctor -- will also execute base constructor.
        /// </summary>
        /// <param name="link">[Optional] Sets this <see cref="Link"/></param>
        /// <param name="displayText">[Optional] Sets this <see cref="DisplayText"/></param>
        /// <param name="imageIndex">[Recommended] 0=PDF, 1=ASC, 2=Other/Default</param>
        public AssemblyLinkItem(string link = "", string displayText = "",
            int imageIndex = 2) : base(displayText, imageIndex)
        {
            if (!string.IsNullOrWhiteSpace(link))
            {
                Link = link;
                ToolTipText = Link;
            }

            if (!string.IsNullOrWhiteSpace(displayText))
            {
                DisplayText = displayText;
            }            
        }

        /// <summary>
        /// Image Key ctor -- will also execute base constructor.
        /// </summary>
        /// <param name="link">[Optional] Sets this <see cref="Link"/></param>
        /// <param name="displayText">[Optional] Sets this <see cref="DisplayText"/></param>
        /// <param name="imageKey">[Recommended] 0=PDF, 1=ASC, 2=Other/Default</param>
        public AssemblyLinkItem(string link = "", string displayText = "",
            string imageKey = "other") : base(displayText, imageKey)
        {
            if (!string.IsNullOrWhiteSpace(link))
            {
                Link = link;
                ToolTipText = Link;
            }

            if (!string.IsNullOrWhiteSpace(displayText))
            {
                DisplayText = displayText;
            }            
        }
        #endregion

        /// <summary> Will start the default associated process on the linked item. </summary>
        public void Activate() {
            try
            {
                Process.Start(Link);
            } catch(System.ComponentModel.Win32Exception wex) {
                csExceptionLogger.csExceptionLogger.Write($"Link_Activate->({Link})", wex);
                MessageBox.Show("Couldn't start the process meant for this link.\nPlease ensure you have permission to access the path given.",
                    "Link::Activate() - Win32Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } catch(ObjectDisposedException ode)
            {
                csExceptionLogger.csExceptionLogger.Write($"Link_Activate->({Link})", ode);
                MessageBox.Show("The process object was disposed before the process stopped!", 
                    "Link::Activate() - ObjectDisposedException", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } catch(System.IO.FileNotFoundException fnfe)
            {
                csExceptionLogger.csExceptionLogger.Write($"Link_Activate->({Link})", fnfe);
                MessageBox.Show("Couldn't find the file specificed in the link!",
                    "Link::Activate() - FileNotFoundException", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
