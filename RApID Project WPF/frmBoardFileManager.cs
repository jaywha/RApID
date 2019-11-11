using EricStabileLibrary;
using Microsoft.VisualBasic;
using RApID_Project_WPF.CustomControls;
using RApID_Project_WPF.PCBAliasDataSetTableAdapters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

        /// <summary>
        /// Manage Tooltips
        /// </summary>
        ToolTip SetterTip = new ToolTip()
        {
            AutoPopDelay = 5000,
            InitialDelay = 1000,
            ReshowDelay = 500,
            ShowAlways = true,
            UseAnimation = true,
            ToolTipIcon = ToolTipIcon.Info
        };

        private DesignFileSet _selectedModel = new DesignFileSet();
        public DesignFileSet SelectedModel {
            get => _selectedModel;
            set {
                if (!MasterList.Contains(value)) MasterList.Add(value);
                _selectedModel = value;
                lblPartName.Text = lblPartName.Text.Replace("<NAME>",_selectedModel.PartName);
                lblCommodityClass.Text = lblCommodityClass.Text.Replace("<CLASS>", _selectedModel.CommodityClass);
                flowBOMFiles.Controls.AddRange(_selectedModel.BOMFiles.ToArray());
                flowSchematicLinks.Controls.AddRange(_selectedModel.SchematicLinks.ToArray());
                CurrentDesignFileIndex = MasterList.IndexOf(value);
            }
        }
        List<DesignFileSet> MasterList = new List<DesignFileSet>();
        int CurrentDesignFileIndex = 0;
        int SchematicFileIndex = 0; // tracks index of Schematic File Link to handle during modifications
        int BOMFileIndex = 0; // tracks index of BOM File Link to handle during modifications

        bool bBOM;
        bool bSchematic;
        bool bNewSchematic;
        bool bNewBOM;
        bool DirectDialog = false;

        /// <summary>
        /// Default Ctor
        /// </summary>
        /// <param name="partNumber">The part number to start the dialog with.</param>
        /// <param name="directDialog">Determines if the tech help yes/no dialog will show</param>
        public frmBoardFileManager(string partNumber = "", bool directDialog = false)
        {
            InitializeComponent();
            tbDatabaseView.Hide();

            if (!string.IsNullOrWhiteSpace(partNumber)) txtPartNumber.Text = partNumber;
            DirectDialog = directDialog;
        }

        private void frmBoardAliases_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'pCBAliasDataSet.TechAlias' table. You can move, or remove it, as needed.
            this.techAliasTableAdapter.Fill(this.pCBAliasDataSet.TechAlias);
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

            new TechAliasTableAdapter().Fill(pCBAliasDataSet.TechAlias);

            txtPartNumber.Focus();
            
            if(!string.IsNullOrWhiteSpace(txtPartNumber.Text))
            {
                HandleTextBoxEntry(new KeyEventArgs(Keys.Enter));
            }
        }

        private void HandleTextBoxEntry(KeyEventArgs e) {
            errorProvider.SetError(txtPartNumber, string.Empty);
            errorProvider.SetError(btnSearch, string.Empty);

            if (e.KeyCode == Keys.Enter)
            {
                ResetAllData();
                GetPartNumberDetailsAndAliases();
            }
        }

        private void btnSearch_Click(object sender, EventArgs e) => HandleTextBoxEntry(new KeyEventArgs(Keys.Enter));
        private void btnReset_Click(object sender, EventArgs e) => ResetAllData(resetPN: true);
        private void txtPartNumber_KeyDown(object sender, KeyEventArgs e) => HandleTextBoxEntry(e);

        private void ResetStatus()
        {
            lblStatus.Text = "";
            progbarStatus.Value = 0;
            progbarStatus.ForeColor = Color.Green;
        }

        private void frmBoardAliases_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Modifiers == (Keys.LShiftKey | Keys.RControlKey))
            {
                dgvDatabaseTable.AllowUserToAddRows = !dgvDatabaseTable.AllowUserToAddRows;
                dgvDatabaseTable.AllowUserToDeleteRows = !dgvDatabaseTable.AllowUserToDeleteRows;
            }
        }

        private void tcDataViewer_SelectedIndexChanged(object sender, EventArgs e)
            => new TechAliasTableAdapter().Fill(pCBAliasDataSet.TechAlias);

        #region Context Menu

        private void ChangeTag_Click(object sender, EventArgs e)
        {
            Console.WriteLine($"ChangeTag --> bBOM? ==> {bBOM}");
            var control = bBOM ? _selectedModel.BOMFiles[BOMFileIndex] : _selectedModel.SchematicLinks[SchematicFileIndex];
            var def = control.Tag?.ToString() ?? "";
            control.Tag = Interaction.InputBox("Please enter a new Tag value", "New Assembly Item Tag", def, MousePosition.X, MousePosition.Y);
            infoProvider.SetError(control, control.Tag?.ToString() ?? "");

            if(!string.IsNullOrWhiteSpace(control.Tag?.ToString() ?? ""))
            {
                using (SqlConnection conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand($"UPDATE TechAlias SET {(bBOM ? "[BOMTags]" : "[SchematicTags]")} = @value WHERE PartNumber = @pn ", conn))
                    {
                        try
                        {
                            conn.Open();
                            string resultant = "";
                            if (bBOM) {
                                _selectedModel.BOMTags[BOMFileIndex] = control.Tag?.ToString() ?? "";
                                resultant = _selectedModel.BOMTags.ToStrings(suffix: ",");
                            } else {
                                _selectedModel.SchematicTags[SchematicFileIndex] = control.Tag?.ToString() ?? "";
                                resultant = _selectedModel.SchematicTags.ToStrings(suffix: ",");
                            }
                            cmd.Parameters.AddWithValue("@pn", _selectedModel.PartNumber);
                            cmd.Parameters.AddWithValue("@value", resultant);
                            int result = cmd.ExecuteNonQuery();
                            conn.Close();
                        }
                        catch (Exception ex) { MessageBox.Show(ex.Message); }
                    }
                }
            }
        }

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

            spltpnlActualForm.Panel2Collapsed = false;
            progbarStatus.Value = 0;
            lblStatus.Text = "Changing "+(bBOM ? "BOM file path" : "Schematic link path")+"...";

            if (ofd.ShowDialog() != DialogResult.OK ||                
                string.IsNullOrWhiteSpace(ofd.FileName)) return;

            if (bBOM)
            {
                var ext = ofd.SafeFileName.Split('.').Last();
                Image img = imgSchematics.Images[(new List<string>() { "xls", "xlsx" }.Contains(ext) ? ext : "other")];
                if (bNewBOM)
                {
                    AssemblyLinkLabel link = new AssemblyLinkLabel(ofd.FileName, ofd.SafeFileName, img, handler: lnkBOMFile_MouseDown);
                    _selectedModel.BOMFiles.Add(link);
                    flowBOMFiles.Controls.Add(link);
                    bNewBOM = false;
                }
                else
                {
                    AssemblyLinkLabel newAssemblyLink = new AssemblyLinkLabel(ofd.FileName, ofd.SafeFileName, img, handler: lnkBOMFile_MouseDown);
                    _selectedModel.BOMFiles.RemoveAt(BOMFileIndex);
                    _selectedModel.BOMFiles.Insert(BOMFileIndex, newAssemblyLink);
                    flowBOMFiles.Controls.RemoveAt(BOMFileIndex);
                    flowBOMFiles.Controls.Add(newAssemblyLink);
                }
            }
            else if (bSchematic)
            {
                var ext = "." + ofd.SafeFileName.Split('.').Last();
                Image img = imgSchematics.Images[(new List<string>() { "pdf", "asc" }.Contains(ext) ? ext : "other")];
                if (bNewSchematic) {
                    AssemblyLinkLabel link = new AssemblyLinkLabel(ofd.FileName, ofd.SafeFileName, img, handler: lnkSchematicFile_MouseDown);
                    _selectedModel.SchematicLinks.Add(link);
                    flowSchematicLinks.Controls.Add(link);
                    bNewSchematic = false;
                } else {
                    AssemblyLinkLabel newAssemblyLink = new AssemblyLinkLabel(ofd.FileName, ofd.SafeFileName, img, handler: lnkSchematicFile_MouseDown);
                    _selectedModel.SchematicLinks.RemoveAt(SchematicFileIndex);
                    _selectedModel.SchematicLinks.Insert(SchematicFileIndex, newAssemblyLink);
                    flowSchematicLinks.Controls.RemoveAt(SchematicFileIndex);
                    flowSchematicLinks.Controls.Add(newAssemblyLink);
                }
            }

            lblStatus.Text = "Changing database entry...";
            progbarStatus.PerformStep();            

            using (SqlConnection conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("UPDATE [Repair].[dbo].[TechAlias] SET " +
                    "BOMPath = @BomPath, SchematicPaths = @SchPath " +
                    "WHERE [PartNumber] = @Pnum", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", _selectedModel.PartNumber);
                    cmd.Parameters.AddWithValue("@BomPath", _selectedModel.BOMFiles.ToStrings(suffix: ","));
                    cmd.Parameters.AddWithValue("@SchPath", _selectedModel.SchematicLinks.ToStrings(suffix: ","));
                    progbarStatus.PerformStep();
                    if(cmd.ExecuteNonQuery() > 0) {
                        lblStatus.Text = "Change successfull!";
                        progbarStatus.PerformStep();
                    } else {
                        lblStatus.Text = "DB_FAILURE --> Couldn't update database!";
                        progbarStatus.ForeColor = Color.Red;
                    }
                }
            }

            bSchematic = false;
            bBOM = false;
            ResetStatus();
        }

        private void deleteSchematicLinkToolStripMenuItem_Click(object sender, EventArgs e) {
            _selectedModel.SchematicLinks.RemoveAt(SchematicFileIndex);
            flowSchematicLinks.Controls.RemoveAt(SchematicFileIndex);
            using (SqlConnection conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("UPDATE [Repair].[dbo].[TechAlias] SET " +
                    "SchematicPaths = @SchPath " +
                    "WHERE [PartNumber] = @Pnum", conn))
                {
                    var schematicPaths = "";
                    foreach (AssemblyLinkLabel schematic in _selectedModel.SchematicLinks)
                    {
                        schematicPaths += schematic.Link + ",";
                    }
                    if(string.IsNullOrWhiteSpace(schematicPaths) || schematicPaths.Length <= 1)
                        cmd.Parameters.AddWithValue("@SchPath", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@SchPath", schematicPaths.Substring(0, schematicPaths.Length - 1));

                    cmd.Parameters.AddWithValue("@Pnum", txtPartNumber.Text);

                    int rowsAffected = cmd.ExecuteNonQuery();
                }
            }

            HandleTextBoxEntry(new KeyEventArgs(Keys.Enter));
        }

        private void AddNewBOMFile_Click(object sender, EventArgs e)
        {
            bNewBOM = true;
            bSchematic = false;
            bBOM = true;
            changeFilePathToolStripMenuItem_Click(sender, e);
        }

        private void AddNewSchematicLink_Click(object sender, EventArgs e)
        {
            bNewSchematic = true;
            bSchematic = true;
            bBOM = false;
            changeFilePathToolStripMenuItem_Click(sender, e);
        }

        /// <summary>
        /// Will reset all entered data.
        /// </summary>
        /// <param name="resetPN">Reset the PN?</param>
        private void ResetAllData(bool resetPN = false)
        {
            if (resetPN) txtPartNumber.Clear();
            lblPartName.Text = "Part Name: <NAME>";
            lblCommodityClass.Text = "Commodity Class: <CLASS>";
            _selectedModel.BOMFiles.Clear();
            _selectedModel.SchematicLinks.Clear();
            flowBOMFiles.Controls.Clear();
            flowSchematicLinks.Controls.Clear();
            MasterList.Clear();
        }

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
                        _selectedModel.PartName = reader[0].ToString();
                        _selectedModel.CommodityClass = reader[1].ToString();
                    }
                }
            }

            if (!validPartNumber)
            {
                errorProvider.SetError(txtPartNumber, "Part Number not found in ItemMaster table!");
                return;
            }

            bool dataFound = false;
            using (SqlConnection conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT PartNumber, BOMPath, SchematicPaths, BOMTags, SchematicTags FROM [Repair].[dbo].[TechAlias] " +
                    "WHERE [PartNumber] = @Pnum", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", txtPartNumber.Text);

                    SqlDataReader reader = cmd.ExecuteReader();

                    dataFound = reader.Read();
                    if (dataFound)
                    {
                        DesignFileSet model = new DesignFileSet()
                        {
                            PartNumber = reader[0]?.ToString(),
                            PartName = _selectedModel.PartName,
                            CommodityClass = _selectedModel.CommodityClass,
                            BOMTags = reader[3].ToString().Split(',').ToList(),
                            SchematicTags = reader[4].ToString().Split(',').ToList(),
                        };

                        StringBuilder sb = new StringBuilder();
                        if (!string.IsNullOrWhiteSpace(reader[1].ToString().Trim()))
                        {
                            sb.Append("Reading New BOM Files {");

                            var counter = 0;
                            foreach (var @string in reader[1].ToString().Split(','))
                            {
                                var name = @string.Substring(@string.LastIndexOf('\\') + 1) ?? "not set";
                                var ext = @string.Substring(@string.LastIndexOf('.') + 1) ?? "other";
                                System.Drawing.Image img = imgSchematics.Images[
                                        (!string.IsNullOrWhiteSpace(ext) && new List<string>() { "pdf", "asc" }.Contains(ext)
                                        ? ext : "other")
                                    ];

                                sb.Append("\n");
                                sb.Append($"\t[AssemblyLinkItem] ==> {@string}");
                                AssemblyLinkLabel link = new AssemblyLinkLabel(@string, name, img,
                                    ext.Equals("pdf") ? "Assembly File" :
                                    ext.Contains("xls") ? "BOM File" :
                                    "Other File",
                                    handler: lnkBOMFile_MouseDown);

                                if (model.BOMTags != null && model.BOMTags.Count > counter) {
                                    link.Tag = model.BOMTags[counter];
                                    infoProvider.SetError(link, model.BOMTags[counter++]);
                                }

                                model.BOMFiles.Add(link);
                                flowBOMFiles.Controls.Add(link);
                            }
                        }
                        Console.Write(sb.ToString());
                        sb = new StringBuilder();
                        if (!string.IsNullOrWhiteSpace(reader[2].ToString().Trim()))
                        {
                            sb.Append("Reading New Schematic Files {");

                            var counter = 0;
                            foreach (var @string in reader[2].ToString().Split(','))
                            {
                                var name = @string.Substring(@string.LastIndexOf('\\') + 1) ?? "not set";
                                var ext = @string.Substring(@string.LastIndexOf('.') + 1) ?? "other";
                                Image img = imgSchematics.Images[
                                        (!string.IsNullOrWhiteSpace(ext) && new List<string>() { "pdf", "asc" }.Contains(ext)
                                        ? ext : "other")
                                    ];

                                sb.Append("\n");
                                sb.Append($"\t[AssemblyLinkItem] ==> {@string}");
                                AssemblyLinkLabel link = new AssemblyLinkLabel(@string, name, img,
                                    ext.Equals("pdf") ? "Assembly File" :
                                    ext.Contains("xls") ? "BOM File" :
                                    "Other File",
                                    handler: lnkSchematicFile_MouseDown);

                                if (model.SchematicTags != null && model.SchematicTags.Count > counter) {
                                    link.Tag = model.SchematicTags[counter];
                                    infoProvider.SetError(link, model.SchematicTags[counter++]);
                                }

                                model.SchematicLinks.Add(link);
                                flowSchematicLinks.Controls.Add(link);
                            }
                        }
                        sb.AppendLine("\n}");
                        Console.Write(sb.ToString());
                        Console.WriteLine(model.ToString());
                        SelectedModel = model;
                    }
                    else
                    {
                        if (MasterList.Count == 0)
                        {
                            DialogResult answer = MessageBox.Show("No current data exists for this part number.\n" +
                                "Would you like to create a new entry to save your selected BOM file?\n" +
                                "Pressing [No] will reset the form.",
                                "Prompt to Create New Database Entry",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Question);
                            if (answer == DialogResult.Yes)
                                AddNewDatabaseRow(txtPartNumber.Text);
                            else if (answer == DialogResult.No)
                                ResetAllData(resetPN: true);
                            return;
                        }
                    }
                }
            }
            if (!dataFound)
            {
                errorProvider.SetError(btnSearch, "New Part Number!\nPlease associate a BOM File from the L: drive.");
                return;
            }
        }

        private void AddNewDatabaseRow(string partNumber)
        {
            _selectedModel.BOMFiles.Add(new AssemblyLinkLabel("BOMLink", "BOM Display Test"));
            _selectedModel.SchematicLinks.Add(new AssemblyLinkLabel("ASSYLink", "Assembly Display Text"));
            flowSchematicLinks.Controls.Add(new AssemblyLinkLabel("<EMPTY>", "ASSYLink"));
            using (SqlConnection conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("INSERT INTO TechAlias (PartNumber, BOMPath, SchematicPaths) " +
                    "VALUES (@Pnum, @BomPath, @SchPaths)", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", partNumber);
                    cmd.Parameters.AddWithValue("@BomPath", "BOMLink");
                    cmd.Parameters.AddWithValue("@SchPaths", "ASSYLink");
                    var rowsAffected = cmd.ExecuteNonQuery();
                }
            }
        }

        private void deletePartNumberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("DELETE FROM TechAlias WHERE PartNumber = @Pnum", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", txtPartNumber.Text);
                    var rowsAffected = cmd.ExecuteNonQuery();
                }
            }

            _selectedModel.BOMFiles.RemoveAt(CurrentDesignFileIndex);
            _selectedModel.SchematicLinks.RemoveAt(CurrentDesignFileIndex);
            MasterList.RemoveAt(CurrentDesignFileIndex);
            flowSchematicLinks.Controls.Clear();
        }
        #endregion

        #region Link Label
        private void lnkBOMFile_MouseDown(object sender, MouseEventArgs e)
        {
            bSchematic = false;
            bBOM = true;

            if (e.Button == MouseButtons.Right)
            {
                BOMFileIndex = -1;
                foreach (Control c in flowBOMFiles.Controls)
                {
                    System.Drawing.Point screenPoint = PointToScreen(c.Location);
                    if (screenPoint.X == MousePosition.X && screenPoint.Y == MousePosition.Y)
                        break;
                    else BOMFileIndex++;
                }
                cxmnuSchematicLinksMenu.Show(MousePosition.X, MousePosition.Y);
            }
        }

        private void lnkSchematicFile_MouseDown(object sender, MouseEventArgs e)
        {
            bSchematic = true;
            bBOM = false;

            if (e.Button == MouseButtons.Right)
            {
                SchematicFileIndex = -1;
                foreach(Control c in flowSchematicLinks.Controls)
                {
                    System.Drawing.Point screenPoint = PointToScreen(c.Location);
                    if (screenPoint.X == MousePosition.X && screenPoint.Y == MousePosition.Y)
                        break;
                    else SchematicFileIndex++;
                }
                cxmnuSchematicLinksMenu.Show(MousePosition.X, MousePosition.Y);
            }
        }
        #endregion

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new TechAliasTableAdapter().Fill(pCBAliasDataSet.TechAlias);
        }

        private ToolStripMenuItem AddNewSchematicLink = new ToolStripMenuItem("Add New Schematic Link...");
        private ToolStripMenuItem AddNewBOMFile = new ToolStripMenuItem("Add new BOM File...");
        private ToolStripMenuItem ChangeFilePath = new ToolStripMenuItem("Change File Path...");
        private ToolStripMenuItem DeleteSchematicLink = new ToolStripMenuItem("Delete Schematic Link...");
        private ToolStripMenuItem ChangeTag = new ToolStripMenuItem("Change Tag...");

        private void flowBOMFiles_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ChangeFilePath.Click -= changeFilePathToolStripMenuItem_Click;
                DeleteSchematicLink.Click -= deleteSchematicLinkToolStripMenuItem_Click;

                cxmnuSchematicLinksMenu.Items.Clear();
                cxmnuSchematicLinksMenu.Items.Add(AddNewBOMFile);
                AddNewSchematicLink.Click += AddNewBOMFile_Click;

                cxmnuSchematicLinksMenu.Show(MousePosition.X, MousePosition.Y);
                cxmnuSchematicLinksMenu.Closed += cxmnuSchematicLinksMenu_Closed;
            }
        }

        private void flowSchematicLinks_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ChangeFilePath.Click -= changeFilePathToolStripMenuItem_Click;
                DeleteSchematicLink.Click -= deleteSchematicLinkToolStripMenuItem_Click;

                cxmnuSchematicLinksMenu.Items.Clear();
                cxmnuSchematicLinksMenu.Items.Add(AddNewSchematicLink);
                AddNewSchematicLink.Click += AddNewSchematicLink_Click;

                cxmnuSchematicLinksMenu.Show(MousePosition.X, MousePosition.Y);
                cxmnuSchematicLinksMenu.Closed += cxmnuSchematicLinksMenu_Closed;
            }
        }

        private void lblPartNumberLabel_DoubleClick(object sender, EventArgs e) => Process.Start(ELEC_ROOT_DIR);

        private void dgvDatabaseTable_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
            txtPartNumber.Text = dgvDatabaseTable.Rows[e.RowIndex].Cells[0].Value.ToString();
            tcDataViewer.SelectedIndex = 0;
            HandleTextBoxEntry(new KeyEventArgs(Keys.Enter));
        }

        private void cxmnuSchematicLinksMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            cxmnuSchematicLinksMenu.Items.Clear();
            cxmnuSchematicLinksMenu.Items.Add(ChangeFilePath);
            cxmnuSchematicLinksMenu.Items.Add(ChangeTag);
            cxmnuSchematicLinksMenu.Items.Add(new ToolStripSeparator());
            cxmnuSchematicLinksMenu.Items.Add(DeleteSchematicLink);

            ChangeFilePath.Click += changeFilePathToolStripMenuItem_Click;
            ChangeTag.Click += ChangeTag_Click;
            DeleteSchematicLink.Click += deleteSchematicLinkToolStripMenuItem_Click;
            cxmnuSchematicLinksMenu.Closed -= cxmnuSchematicLinksMenu_Closed;
        }

        private void cxmnuSchematicLinksMenu_Opened(object sender, EventArgs e)
        {
            Console.Write(
            $"\nLink Menu Activated by {GetChildAtPoint(new Point(MousePosition.X, MousePosition.Y))?.Name ?? "?"}{{\n" +
                $"\tSender ==> {sender?.ToString() ?? "no sender?"},\n" +
                $"\tArgs ==> {e?.ToString() ?? "no args?"}\n" +
            $"}}\n");
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

    /// <summary>
    /// Model for all data shown in the form (row data in SQL table also)
    /// </summary>
    public class DesignFileSet : INotifyPropertyChanged {
        #region PropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        readonly string CallerMemberName;

        #region Properties
        private string _partNumber = string.Empty;
        public string PartNumber
        {
            get => _partNumber;
            set {
                _partNumber = value;
                OnPropertyChanged();
            }
        }

        private string _partName = string.Empty;   
        public string PartName
        {
            get => _partName;
            set {
                _partName = value;
                OnPropertyChanged();
            }
        }

        private string _commodityClass = string.Empty;
        public string CommodityClass
        {
            get => _commodityClass;
            set {
                _commodityClass = value;
                OnPropertyChanged();
            }
        }

        private List<AssemblyLinkLabel> _bomFiles = new List<AssemblyLinkLabel>();
        public List<AssemblyLinkLabel> BOMFiles
        {
            get => _bomFiles;
            set {
                _bomFiles = value;
                OnPropertyChanged();
            }
        }

        private List<string> _bomTags = new List<string>();
        public List<string> BOMTags
        {
            get => _bomTags;
            set {
                _bomTags = value;
                OnPropertyChanged();
            }
        }

        private List<string> _assyTags = new List<string>();
        public List<string> SchematicTags
        {
            get => _assyTags;
            set {
                _assyTags = value;
                OnPropertyChanged();
            }
        }


        private List<AssemblyLinkLabel> _schematicLinks = new List<AssemblyLinkLabel>();
        public List<AssemblyLinkLabel> SchematicLinks
        {
            get => _schematicLinks;
            set {
                _schematicLinks = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public DesignFileSet([CallerMemberName] string callerName = "") {
            CallerMemberName = callerName;
        }

        public override string ToString()
            => $"[RecordDataModel] =>\n" +
            $"\t{nameof(PartNumber)} = \"{PartNumber}\"\n" +
            $"\t{nameof(BOMFiles)} {{\n" +
            BOMFiles.ToStringsln(prefix: "\t\t", suffix: "\n") +
            $"\t}}\n" +
            $"\t{nameof(BOMTags)} {{\n" +
            BOMTags.ToStringsln(prefix: "\t\t", suffix: "\n") +
            $"\t}}\n" +
            $"\t{nameof(SchematicLinks)} {{\n" +
            SchematicLinks.ToStringsln(prefix: "\t\t", suffix: "\n") +
            $"\t}}\n" +
            $"\t{nameof(SchematicTags)} {{\n" +
            SchematicTags.ToStringsln(prefix: "\t\t", suffix: "\n") +
            $"\t}}\n" +
            $"}}";
    }
}
