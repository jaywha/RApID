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
            }
        }
        List<DesignFileSet> MasterList = new List<DesignFileSet>();
        int CurrentDesignFileIndex = 0;
        int SchematicFileIndex = 0; // tracks index of Schematic File Link to handle during modifications
        int BOMFileIndex = 0; // tracks index of BOM File Link to handle during modifications

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

            techAliasesTableAdapter.Fill(pCBAliasDataSet.TechAliases);

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

        private void frmBoardAliases_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Modifiers == (Keys.LShiftKey | Keys.RControlKey))
            {
                dgvDatabaseTable.AllowUserToAddRows = !dgvDatabaseTable.AllowUserToAddRows;
                dgvDatabaseTable.AllowUserToDeleteRows = !dgvDatabaseTable.AllowUserToDeleteRows;
            }
        }

        private void tcDataViewer_SelectedIndexChanged(object sender, EventArgs e)
            => techAliasesTableAdapter.Fill(pCBAliasDataSet.TechAliases);

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
                var ext = "." + ofd.SafeFileName.Split('.').Last();
                System.Drawing.Image img = imgSchematics.Images[(new List<string>() { ".pdf", ".asc" }.Contains(ext) ? ext : "other")];
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
                    _selectedModel.SchematicLinks.RemoveAt(CurrentDesignFileIndex - 1);
                    _selectedModel.SchematicLinks.Insert(CurrentDesignFileIndex - 1, newAssemblyLink);
                    flowSchematicLinks.Controls.RemoveAt(CurrentDesignFileIndex - 1);
                    flowSchematicLinks.Controls.Add(newAssemblyLink);
                }
                bBOM = false;
            }
            else if (bSchematic)
            {
                var ext = "." + ofd.SafeFileName.Split('.').Last();
                System.Drawing.Image img = imgSchematics.Images[(new List<string>() { ".pdf", ".asc" }.Contains(ext) ? ext : "other")];
                if (bNewSchematic) {
                    AssemblyLinkLabel link = new AssemblyLinkLabel(ofd.FileName, ofd.SafeFileName, img, handler: lnkSchematicFile_MouseDown);
                    _selectedModel.SchematicLinks.Add(link);
                    flowSchematicLinks.Controls.Add(link);
                    bNewSchematic = false;
                } else {
                    AssemblyLinkLabel newAssemblyLink = new AssemblyLinkLabel(ofd.FileName, ofd.SafeFileName, img, handler: lnkSchematicFile_MouseDown);
                    _selectedModel.SchematicLinks.RemoveAt(CurrentDesignFileIndex-1);
                    _selectedModel.SchematicLinks.Insert(CurrentDesignFileIndex-1, newAssemblyLink);
                    flowSchematicLinks.Controls.RemoveAt(CurrentDesignFileIndex-1);
                    flowSchematicLinks.Controls.Add(newAssemblyLink);
                }
                bBOM = false;
            }

            using (SqlConnection conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("UPDATE [Repair].[dbo].[TechAliases] SET " +
                    "BOMPath = @BomPath, SchematicPaths = @SchPath " +
                    "WHERE [PartNumber] = @Pnum", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", _selectedModel.PartNumber);
                    cmd.Parameters.AddWithValue("@BomPath", _selectedModel.BOMFiles.ToStrings(suffix: ","));
                    var schematicPaths = "";
                    foreach (AssemblyLinkLabel schematic in _selectedModel.SchematicLinks)
                    {
                        schematicPaths += schematic.Link + ",";
                    }
                    cmd.Parameters.AddWithValue("@SchPath", schematicPaths.Substring(0, schematicPaths.Length-1));
                    var rowsAffected = cmd.ExecuteNonQuery();
                }
            }
        }

        private void deleteSchematicLinkToolStripMenuItem_Click(object sender, EventArgs e) {
            _selectedModel.SchematicLinks.RemoveAt(SchematicFileIndex);
            flowSchematicLinks.Controls.RemoveAt(SchematicFileIndex);
            using (SqlConnection conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("UPDATE [Repair].[dbo].[TechAliases] SET " +
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

        private void AddNewDatabaseRow(string partNumber)
        {
            _selectedModel.BOMFiles.Add(new AssemblyLinkLabel("BOMLink", "BOM Display Test"));
            _selectedModel.SchematicLinks.Add(new AssemblyLinkLabel("ASSYLink", "Assembly Display Text"));
            flowSchematicLinks.Controls.Add(new AssemblyLinkLabel("<EMPTY>", "ASSYLink"));
            using (SqlConnection conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("INSERT INTO TechAliases (PartNumber, BOMPath, SchematicPaths) " +
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
                using (SqlCommand cmd = new SqlCommand("DELETE FROM TechAliases WHERE PartNumber = @Pnum", conn))
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

            if (!validPartNumber) {
                errorProvider.SetError(txtPartNumber, "Part Number not found in ItemMaster table!");
                return;
            }

            bool dataFound = false;
            using (SqlConnection conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString)) {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT PartNumber, BOMPath, SchematicPaths FROM [Repair].[dbo].[TechAliases] " +
                    "WHERE [PartNumber] = @Pnum", conn)) {
                    cmd.Parameters.AddWithValue("@Pnum", txtPartNumber.Text);

                    SqlDataReader reader = cmd.ExecuteReader();

                    dataFound = reader.Read();
                    if (dataFound) {
                        DesignFileSet model = new DesignFileSet() {
                            PartNumber = reader[0]?.ToString()
                        };

                        StringBuilder sb = new StringBuilder();
                        if (!string.IsNullOrWhiteSpace(reader[1].ToString().Trim()))
                        {
                            sb.Append("Reading New BOM Files {");

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

                                _selectedModel.BOMFiles.Add(link);
                                flowBOMFiles.Controls.Add(link);
                            }
                            model.BOMFiles = _selectedModel.BOMFiles;
                        }
                        sb = new StringBuilder();
                        if (!string.IsNullOrWhiteSpace(reader[2].ToString().Trim()))
                        {
                            sb.Append("Reading New Schematic Files {");
                            
                            foreach (var @string in reader[2].ToString().Split(','))
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
                                    handler: lnkSchematicFile_MouseDown);

                                _selectedModel.SchematicLinks.Add(link);
                                flowSchematicLinks.Controls.Add(link);
                            }
                            model.SchematicLinks = _selectedModel.SchematicLinks;
                        }
                        sb.AppendLine("\n}");
                        Console.Write(sb.ToString());
                        Console.WriteLine(model.ToString());
                        SelectedModel = model;
                    } else {
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
            if(!dataFound) {
                errorProvider.SetError(btnSearch, "New Part Number!\nPlease associate a BOM File from the L: drive.");
                return;
            }
        }

        private void btnChangeBOMPath_Click(object sender, EventArgs e)
        {
            bBOM = true;
            bSchematic = false;

            changeFilePathToolStripMenuItem_Click(null, null);
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            techAliasesTableAdapter.Fill(pCBAliasDataSet.TechAliases);
        }

        private ToolStripMenuItem AddNewSchematicLink = new ToolStripMenuItem("Add New Schematic Link...");
        private ToolStripMenuItem AddNewBOMFile = new ToolStripMenuItem("Add new BOM File...");
        private ToolStripMenuItem ChangeFilePath = new ToolStripMenuItem("Change File Path...");
        private ToolStripMenuItem DeleteSchematicLink = new ToolStripMenuItem("Delete Schematic Link...");

        private void flowSchematicLinks_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ChangeFilePath.Click -= changeFilePathToolStripMenuItem_Click;
                DeleteSchematicLink.Click -= deleteSchematicLinkToolStripMenuItem_Click;

                cxmnuSchematicLinksMenu.Items.Clear();

                FlowLayoutPanel flw = sender as FlowLayoutPanel;
                if (flw.Name.Equals(nameof(flowSchematicLinks)))
                {
                    cxmnuSchematicLinksMenu.Items.Add(AddNewSchematicLink);
                    AddNewSchematicLink.Click += AddNewSchematicLink_Click;
                } else if(flw.Name.Equals(nameof(flowBOMFiles)))
                {
                    cxmnuSchematicLinksMenu.Items.Add(AddNewBOMFile);
                    AddNewSchematicLink.Click += AddNewBOMFile_Click;
                }

                cxmnuSchematicLinksMenu.Show(MousePosition.X, MousePosition.Y);
                cxmnuSchematicLinksMenu.Closed += cxmnuLinkMenu_Closed;
            }
        }

        private void cxmnuLinkMenu_Closed(object sender, EventArgs e)
        {
            cxmnuSchematicLinksMenu.Items.Clear();
            cxmnuSchematicLinksMenu.Items.Add(ChangeFilePath);
            cxmnuSchematicLinksMenu.Items.Add(new ToolStripSeparator());
            cxmnuSchematicLinksMenu.Items.Add(DeleteSchematicLink);

            ChangeFilePath.Click += changeFilePathToolStripMenuItem_Click;
            DeleteSchematicLink.Click += deleteSchematicLinkToolStripMenuItem_Click;
            cxmnuSchematicLinksMenu.Closed -= cxmnuLinkMenu_Closed;
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

    public class DesignFileSet : INotifyPropertyChanged {
        #region PropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        readonly string CallerMemberName;

        #region Properties
        private string _partNumber;
        public string PartNumber
        {
            get => _partNumber;
            set {
                _partNumber = value;
                OnPropertyChanged();
            }
        }

        private string _partName;   
        public string PartName
        {
            get => _partName;
            set {
                _partName = value;
                OnPropertyChanged();
            }
        }

        private string _commodityClass;
        public string CommodityClass
        {
            get => _commodityClass;
            set {
                _commodityClass = value;
                OnPropertyChanged();
            }
        }



        private List<AssemblyLinkLabel> _bomFiles;
        public List<AssemblyLinkLabel> BOMFiles
        {
            get => _bomFiles;
            set {
                _bomFiles = value;
                OnPropertyChanged();
            }
        }

        private List<AssemblyLinkLabel> _schematicLinks;
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
            $"\t{nameof(SchematicLinks)} {{\n" +
            SchematicLinks.ToStringsln(prefix: "\t\t", suffix: "\n") +
            $"\t}}\n" +
            $"}}";
    }
}
