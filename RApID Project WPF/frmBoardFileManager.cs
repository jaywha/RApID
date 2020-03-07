using EricStabileLibrary;
using ExcelDataReader;
using RApID_Project_WPF.CustomControls;
using RApID_Project_WPF.PCBAliasDataSetTableAdapters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using RApID_Project_WPF.Classes;
using System.Threading;
using UIAction = System.Action;
using UIFunc = System.Func<object>;

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
        #region Variables
        public bool WasEntryFound { get; set; } = false;
        public string BOMFileName { get; set; } = "";

        private readonly string DBUpload_Log = $@"P:\EE Process Test\Logs\RApID\_BOMUploads\RApID_BOMUploadLog_{DateTime.Now:MM-dd-yyyy}.txt";
        public const string ELECROOTDIR = @"L:\EngDocumentation\Design\Electrical\";
        const string EMPTY_FILE_PATH = "BOMPath";
        static bool IsFirstTimeToday = true;
        static bool IsUploading = false;

        private DesignFileSet _selectedModel = new DesignFileSet();
        public DesignFileSet SelectedModel
        {
            get => _selectedModel;
            set
            {
                if (!MasterList.Contains(value)) MasterList.Add(value);
                _selectedModel = value;
                lblPartName.Text = lblPartName.Text.Replace("<NAME>", _selectedModel.PartName);
                lblCommodityClass.Text = lblCommodityClass.Text.Replace("<CLASS>", _selectedModel.CommodityClass);
                flowBOMFiles.Controls.AddRange(_selectedModel.BOMFiles.ToArray());
                flowSchematicLinks.Controls.AddRange(_selectedModel.SchematicLinks.ToArray());
                CurrentDesignFileIndex = MasterList.IndexOf(value);
            }
        }

        private AssemblyLinkLabel previouslyMarkedActive;

        /// <summary> Tracks the major form data of all interactions. </summary>
        ObservableCollection<DesignFileSet> MasterList { get; set; } = new ObservableCollection<DesignFileSet>();
        int CurrentDesignFileIndex = 0;
        int SchematicFileIndex = 0; // tracks index of Schematic File Link to handle during modifications
        int BOMFileIndex = 0; // tracks index of BOM File Link to handle during modifications
        int HeightModToShowProgress = 75;

        bool bBOM;
        bool bSchematic;
        bool bNewSchematic;
        bool bNewBOM;
        bool DirectDialog = false;
        #endregion

        /// <summary>
        /// Default Ctor
        /// </summary>
        /// <param name="partNumber">The part number to start the dialog with.</param>
        /// <param name="directDialog">Determines if the tech help yes/no dialog will show</param>
        public frmBoardFileManager(string partNumber = "", bool directDialog = false)
        {
            InitializeComponent();
            tbDatabaseView.Hide();

            if (!string.IsNullOrWhiteSpace(partNumber)) txtFullAssemblyNumber.Text = partNumber;
            DirectDialog = directDialog;

            txtFullAssemblyNumber.Focus();
        }

        private void frmBoardAliases_Load(object sender, EventArgs e)
        {
            this.techAliasTableAdapter.Fill(this.pCBAliasDataSet.TechAlias);
            if (!DirectDialog && IsFirstTimeToday)
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
                            $"{SNMapperLib.csSerialNumberMapper.Instance.AsJSON()}\n", ex);
                        Close();
                    }
                }

                IsFirstTimeToday = false;
            }

            new TechAliasTableAdapter().Fill(pCBAliasDataSet.TechAlias);

            txtFullAssemblyNumber.Focus();

            if (!string.IsNullOrWhiteSpace(txtFullAssemblyNumber.Text))
            {
                HandleTextBoxEntry(new KeyEventArgs(Keys.Enter));
            }
        }

        private void HandleTextBoxEntry(KeyEventArgs e)
        {
            errorProvider.SetError(txtFullAssemblyNumber, string.Empty);
            errorProvider.SetError(btnSearch, string.Empty);

            if (e.KeyCode == Keys.Enter)
            {
                ResetAllData();
                GetPartNumberDetailsAndAliases();
            }
        }

        private void ResetStatus()
        {
            lblStatus.Text = "";
            progbarStatus.Value = 0;
            progbarStatus.ForeColor = Color.Green;
            spltpnlActualForm.Panel2Collapsed = true;
        }

        private void frmBoardAliases_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
#if DEBUG
            if (e.KeyCode == Keys.A && e.Modifiers == (Keys.LShiftKey | Keys.RControlKey))
            {
                dgvDatabaseTable.AllowUserToAddRows = !dgvDatabaseTable.AllowUserToAddRows;
                dgvDatabaseTable.AllowUserToDeleteRows = !dgvDatabaseTable.AllowUserToDeleteRows;
            }
#endif
        }

        private void frmBoardFileManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            WasEntryFound = string.IsNullOrWhiteSpace(BOMFileName);
        }

        private void GetPartNumberDetailsAndAliases()
        {
            var validPartNumber = false;

            using (SqlConnection conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().HummingBirdConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT PartName, CommodityClass FROM [HummingBird].[dbo].[ItemMaster] WHERE [PartNumber] = @Pnum", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", txtFullAssemblyNumber.Text);

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
                errorProvider.SetError(txtFullAssemblyNumber, "Part Number not found in ItemMaster table!");
                return;
            }

            bool dataFound = false;
            using (SqlConnection conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT PartNumber, BOMPath, SchematicPaths, BOMTags, SchematicTags, REV, ECO " +
                    "FROM [Repair].[dbo].[TechAlias] " +
                    "WHERE [PartNumber] = @Pnum", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", txtFullAssemblyNumber.Text);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (!reader.HasRows || reader.RecordsAffected == 0)
                    {
                        DialogResult answer = MessageBox.Show("No current data exists for this part number.\n" +
                                    "Would you like to create a new entry for it?\n" +
                                    "Pressing [No] will reset the form.",
                                    "Prompt to Create New Database Entry",
                                    MessageBoxButtons.YesNoCancel,
                                    MessageBoxIcon.Question);
                        if (answer == DialogResult.Yes)
                            AddNewDatabaseRow(txtFullAssemblyNumber.Text);
                        else if (answer == DialogResult.No)
                            ResetAllData(resetFAN: true);
                        return;
                    }

                    dataFound = reader.Read();
                    while (dataFound)
                    {
                        DesignFileSet model = new DesignFileSet()
                        {
                            PartNumber = reader[0]?.ToString(),
                            PartName = _selectedModel.PartName,
                            CommodityClass = _selectedModel.CommodityClass,
                            BOMTags = reader[3].ToString().Split(',').ToList(),
                            SchematicTags = reader[4].ToString().Split(',').ToList(),
                            REV = reader[5].ToString() ?? "",
                            ECO = reader[6]?.ToString() ?? ""
                        };

                        if (!string.IsNullOrWhiteSpace(reader[1].ToString().Trim()))
                        {
                            var counter = 0;
                            foreach (var @string in reader[1].ToString().Split(','))
                            {
                                var displayText = @string.Substring(@string.LastIndexOf('\\') + 1) ?? "not set";
                                var ext = @string.Split('.').LastOrDefault() ?? "other";
                                var rev = counter < model.REV.Split(',').Length ? model.REV.Split(',')[counter] : "";
                                var eco = counter < model.ECO.Split(',').Length ? model.ECO.Split(',')[counter] : "";
                                Image img = imgSchematics.Images[new List<string>() { "xls", "xlsx", "xlsm" }.Contains(ext) ? "xls" : "other"];

                                AssemblyLinkLabel link = new AssemblyLinkLabel(link: @string, displayText: displayText, img: img,
                                    name: ext.Contains("xls") ? "BOM File" : "Other File",
                                    handler: lnkBOMFile_MouseDown,
                                    rev: rev,
                                    eco: eco);

                                if (model.BOMTags != null && model.BOMTags.Count > counter)
                                {
                                    link.Tag = model.BOMTags[counter];
                                    infoProvider.SetError(link, model.BOMTags[counter] + $"\nREV: {model.REV}\nECO: {model.ECO}");
                                }

                                model.BOMFiles.Add(link);
                                flowBOMFiles.Controls.Add(link);

                                counter++;
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(reader[2].ToString().Trim()))
                        {
                            var counter = 0;
                            foreach (var @string in reader[2].ToString().Split(','))
                            {
                                var displayText = @string.Substring(@string.LastIndexOf('\\') + 1) ?? "not set";
                                var ext = @string.Split('.').LastOrDefault() ?? "other";
                                var rev = counter < model.REV.Split(',').Length ? model.REV.Split(',')[counter] : "";
                                var eco = counter < model.ECO.Split(',').Length ? model.ECO.Split(',')[counter] : "";
                                Image img = imgSchematics.Images[new List<string>() { "pdf", "asc" }.Contains(ext) ? ext : "other"];

                                AssemblyLinkLabel link = new AssemblyLinkLabel(link: @string, displayText: displayText, img: img,
                                    name: ext.Equals("pdf") ? "Assembly File" : "Other File",
                                    handler: lnkSchematicFile_MouseDown,
                                    rev: rev,
                                    eco: eco);

                                if (model.SchematicTags != null && model.SchematicTags.Count > counter)
                                {
                                    link.Tag = model.SchematicTags[counter];
                                    infoProvider.SetError(link, model.SchematicTags[counter] + $"\nREV: {model.REV}\nECO: {model.ECO}");
                                }

                                model.SchematicLinks.Add(link);
                                flowSchematicLinks.Controls.Add(link);
                                counter++;
                            }
                        }

                        //Console.WriteLine(model.ToString());
                        SelectedModel = model;

                        dataFound = reader.Read();
                    }
                }
            }
        }

        private void AddNewDatabaseRow(string partNumber)
        {
            using (SqlConnection conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("INSERT INTO TechAlias (PartNumber, BOMPath, SchematicPaths) " +
                    "VALUES (@Pnum, @BomPath, @SchPaths)", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", partNumber);
                    cmd.Parameters.AddWithValue("@BomPath", DBNull.Value);
                    cmd.Parameters.AddWithValue("@SchPaths", DBNull.Value);
                    var rowsAffected = cmd.ExecuteNonQuery();
                }
            }

            HandleTextBoxEntry(new KeyEventArgs(Keys.Enter));
        }

        /// <summary>
        /// Will reset all entered data.
        /// </summary>
        /// <param name="resetFAN">Reset the FAN?</param>
        private void ResetAllData(bool resetFAN = false)
        {
            if (resetFAN) txtFullAssemblyNumber.Clear();
            lblPartName.Text = "Part Name: <NAME>";
            lblCommodityClass.Text = "Commodity Class: <CLASS>";
            _selectedModel.BOMFiles.Clear();
            _selectedModel.SchematicLinks.Clear();
            flowBOMFiles.Controls.Clear();
            flowSchematicLinks.Controls.Clear();
            MasterList.Clear();
        }

        #region Main View
        private void btnSearch_Click(object sender, EventArgs e) => HandleTextBoxEntry(new KeyEventArgs(Keys.Enter));
        private void btnReset_Click(object sender, EventArgs e) => ResetAllData(resetFAN: true);
        private void txtFullAssemblyNumber_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
                txtFullAssemblyNumber.SelectAll();
            else if (e.KeyCode == Keys.Escape)
                txtFullAssemblyNumber.DeselectAll();
            else if (e.KeyCode == Keys.Enter)
                HandleTextBoxEntry(e);
        }

        #region Assembly Context Menu

        private void ChangeTag_Click(object sender, EventArgs e)
        {
            var control = bBOM ? _selectedModel.BOMFiles[BOMFileIndex] : _selectedModel.SchematicLinks[SchematicFileIndex];
            string def = control.Tag?.ToString() ?? "";
            var request = new frmTextRequest(def) { StartPosition = FormStartPosition.CenterParent };
            var dialogResult = request.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                infoProvider.SetError(control, request.Input + $"\nREV: {_selectedModel.REV}\nECO: {_selectedModel.ECO}");

                using (SqlConnection conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand($"UPDATE TechAlias SET {(bBOM ? "[BOMTags]" : "[SchematicTags]")} = @value WHERE PartNumber = @pn ", conn))
                    {
                        try
                        {
                            conn.Open();
                            string resultant = "";
                            if (bBOM)
                            {
                                if (_selectedModel.BOMTags.Count == BOMFileIndex)
                                    _selectedModel.BOMTags.Add(request.Input);
                                else
                                    _selectedModel.BOMTags[BOMFileIndex] = request.Input;
                                resultant = _selectedModel.BOMTags.ToStrings(suffix: ",");
                            }
                            else
                            {
                                if (_selectedModel.SchematicTags.Count == SchematicFileIndex)
                                    _selectedModel.SchematicTags.Add(request.Input);
                                else
                                    _selectedModel.SchematicTags[SchematicFileIndex] = request.Input;
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

            HandleTextBoxEntry(new KeyEventArgs(Keys.Enter));
        }

        private void changeREVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var control = bBOM ? _selectedModel.BOMFiles[BOMFileIndex] : _selectedModel.SchematicLinks[SchematicFileIndex];
            string rev = control.REV ?? "";
            var request = new frmTextRequest(rev) { StartPosition = FormStartPosition.CenterParent };
            var dialogResult = request.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                infoProvider.SetError(control, request.Input + $"\nREV: {_selectedModel.REV}\nECO: {_selectedModel.ECO}");

                using (SqlConnection conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE TechAlias SET [REV] = @value WHERE PartNumber = @pn ", conn))
                {
                    try
                    {
                        conn.Open();
                        if (string.IsNullOrWhiteSpace(_selectedModel.REV))
                            _selectedModel.REV = request.Input;
                        else
                            _selectedModel.REV += $",{request.Input}";

                        cmd.Parameters.AddWithValue("@pn", _selectedModel.PartNumber);
                        cmd.Parameters.AddWithValue("@value", _selectedModel.REV);
                        int result = cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                }

            }

            HandleTextBoxEntry(new KeyEventArgs(Keys.Enter));
        }

        private void changeECOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var control = bBOM ? _selectedModel.BOMFiles[BOMFileIndex] : _selectedModel.SchematicLinks[SchematicFileIndex];
            string eco = control.ECO ?? "";
            var request = new frmTextRequest(eco) { StartPosition = FormStartPosition.CenterParent };
            var dialogResult = request.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                infoProvider.SetError(control, request.Input);

                using (SqlConnection conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE TechAlias SET [ECO] = @value WHERE PartNumber = @pn ", conn))
                {
                    try
                    {
                        conn.Open();
                        if (string.IsNullOrWhiteSpace(_selectedModel.ECO))
                            _selectedModel.ECO = request.Input;
                        else
                            _selectedModel.ECO += $",{request.Input}";

                        cmd.Parameters.AddWithValue("@pn", _selectedModel.PartNumber);
                        cmd.Parameters.AddWithValue("@value", _selectedModel.ECO);
                        int result = cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                }

            }

            HandleTextBoxEntry(new KeyEventArgs(Keys.Enter));
        }

        private void changeFilePathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullAssemblyNumber.Text)) return;

            using (OpenFileDialog ofd = new OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Title = $"Please choose the new {(bBOM ? "BOM" : "Schematic")} file path...",
                AutoUpgradeEnabled = true,
                Multiselect = false
            }) {

                ofd.InitialDirectory = bNewBOM || bNewSchematic ? ELECROOTDIR
                    : bBOM ? ELECROOTDIR + (flowBOMFiles.Controls[BOMFileIndex] as AssemblyLinkLabel).Link.Split('\\').TakeWhile(token => !token.Contains(".")).Aggregate((c, n) => c + "\\" + n)
                    : bSchematic ? ELECROOTDIR + (flowSchematicLinks.Controls[SchematicFileIndex] as AssemblyLinkLabel).Link.Split('\\').TakeWhile(token => !token.Contains(".")).Aggregate((c, n) => c + "\\" + n)
                    : ELECROOTDIR;

                Console.WriteLine($"Current Init Directory {{\n\t{ofd.InitialDirectory}\n}}");

                spltpnlActualForm.Panel2Collapsed = false;
                progbarStatus.Maximum = 3;
                progbarStatus.Value = 0;
                lblStatus.Text = "Changing " + (bBOM ? "BOM file path" : "Schematic link path") + "...";

                if (ofd.ShowDialog() != DialogResult.OK ||
                    string.IsNullOrWhiteSpace(ofd.FileName))
                {
                    spltpnlActualForm.Panel2Collapsed = true;
                    lblStatus.Text = "";
                    return;
                }

                if (bBOM)
                {
                    var ext = ofd.SafeFileName.Split('.').Last();
                    Image img = imgSchematics.Images[(new List<string>() { "xls", "xlsx", "xlsm" }.Contains(ext) ? "xls" : "other")];
                    if (bNewBOM)
                    {
                        AssemblyLinkLabel link = new AssemblyLinkLabel(ofd.FileName.Replace(ELECROOTDIR, ""), ofd.SafeFileName, img, handler: lnkBOMFile_MouseDown);
                        _selectedModel.BOMFiles.Add(link);
                        flowBOMFiles.Controls.Add(link);
                        bNewBOM = false;
                    }
                    else
                    {
                        AssemblyLinkLabel newAssemblyLink = new AssemblyLinkLabel(ofd.FileName.Replace(ELECROOTDIR, ""), ofd.SafeFileName, img, handler: lnkBOMFile_MouseDown);
                        _selectedModel.BOMFiles.RemoveAt(BOMFileIndex);
                        _selectedModel.BOMFiles.Insert(BOMFileIndex, newAssemblyLink);
                        flowBOMFiles.Controls.RemoveAt(BOMFileIndex);
                        flowBOMFiles.Controls.Add(newAssemblyLink);
                    }
                }
                else if (bSchematic)
                {
                    var ext = ofd.SafeFileName.Split('.').Last();
                    Image img = imgSchematics.Images[(new List<string>() { "pdf", "asc" }.Contains(ext) ? ext : "other")];
                    if (bNewSchematic)
                    {
                        AssemblyLinkLabel link = new AssemblyLinkLabel(ofd.FileName.Replace(ELECROOTDIR, ""), ofd.SafeFileName, img, handler: lnkSchematicFile_MouseDown);
                        _selectedModel.SchematicLinks.Add(link);
                        flowSchematicLinks.Controls.Add(link);
                        bNewSchematic = false;
                    }
                    else
                    {
                        AssemblyLinkLabel newAssemblyLink = new AssemblyLinkLabel(ofd.FileName.Replace(ELECROOTDIR, ""), ofd.SafeFileName, img, handler: lnkSchematicFile_MouseDown);
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
                        cmd.Parameters.AddWithValue("@BomPath", _selectedModel.BOMFiles.GetLinks().ToStrings(suffix: ","));
                        cmd.Parameters.AddWithValue("@SchPath", _selectedModel.SchematicLinks.GetLinks().ToStrings(suffix: ","));
                        progbarStatus.PerformStep();
                        if (cmd.ExecuteNonQuery() > 0)
                        {
                            lblStatus.Text = "Change successfull!";
                            progbarStatus.PerformStep();
                        }
                        else
                        {
                            lblStatus.Text = "DB_FAILURE --> Couldn't update database!";
                            progbarStatus.ForeColor = Color.Red;
                        }
                    }
                }
            }

            bSchematic = false;
            bBOM = false;
            ResetStatus();
        }

        private void openFileLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var control = (bBOM ? flowBOMFiles.Controls[BOMFileIndex] : flowSchematicLinks.Controls[SchematicFileIndex]) as AssemblyLinkLabel;
            try
            {
                Process.Start(Path.Combine(ELECROOTDIR, control.Link.Substring(0, control.Link.LastIndexOf('\\') + 1)));
            }
            catch (FileNotFoundException fnfe)
            {
                csExceptionLogger.csExceptionLogger.Write("EV_openFileLocation", fnfe);
            }
        }

        private void markAsActiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var control = (flowBOMFiles.Controls[BOMFileIndex] as AssemblyLinkLabel);
            BOMFileName = control.Link;
            WasEntryFound = true;

            control.IsMarkedActive = !control.IsMarkedActive;
            control.Invalidate();

            using (SqlConnection conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "UPDATE TechAlias SET [REV] = @rev, [ECO] = @eco WHERE PartNumber = @pn ", conn))
            {
                try
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@pn", _selectedModel.PartNumber);
                    cmd.Parameters.AddWithValue("@rev", _selectedModel.REV);
                    cmd.Parameters.AddWithValue("@eco", _selectedModel.ECO);
                    int result = cmd.ExecuteNonQuery();
                    conn.Close();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }

            if (previouslyMarkedActive != null)
            {
                previouslyMarkedActive.IsMarkedActive = false;
                previouslyMarkedActive.Invalidate();
            }

            previouslyMarkedActive = control.IsMarkedActive ? control : null;
        }

        private void deleteSchematicLinkToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (bSchematic)
            {
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
                        if (string.IsNullOrWhiteSpace(schematicPaths) || schematicPaths.Length <= 1)
                            cmd.Parameters.AddWithValue("@SchPath", DBNull.Value);
                        else
                            cmd.Parameters.AddWithValue("@SchPath", schematicPaths.Substring(0, schematicPaths.Length - 1));

                        cmd.Parameters.AddWithValue("@Pnum", txtFullAssemblyNumber.Text);

                        int rowsAffected = cmd.ExecuteNonQuery();
                    }
                }
            }
            else if (bBOM)
            {
                _selectedModel.BOMFiles.RemoveAt(BOMFileIndex);
                flowBOMFiles.Controls.RemoveAt(BOMFileIndex);
                using (SqlConnection conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("UPDATE [Repair].[dbo].[TechAlias] SET " +
                        "BOMPath = @BOMPaths " +
                        "WHERE [PartNumber] = @Pnum", conn))
                    {
                        var bomFiles = "";
                        foreach (AssemblyLinkLabel bomFile in _selectedModel.BOMFiles)
                        {
                            bomFiles += bomFile.Link + ",";
                        }
                        if (string.IsNullOrWhiteSpace(bomFiles) || bomFiles.Length <= 1)
                            cmd.Parameters.AddWithValue("@BOMPaths", DBNull.Value);
                        else
                            cmd.Parameters.AddWithValue("@BOMPaths", bomFiles.Substring(0, bomFiles.Length - 1));

                        cmd.Parameters.AddWithValue("@Pnum", txtFullAssemblyNumber.Text);

                        int rowsAffected = cmd.ExecuteNonQuery();
                    }
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

        private void deletePartNumberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("DELETE FROM TechAlias WHERE PartNumber = @Pnum", conn))
                {
                    cmd.Parameters.AddWithValue("@Pnum", txtFullAssemblyNumber.Text);
                    var rowsAffected = cmd.ExecuteNonQuery();
                }
            }

            _selectedModel.BOMFiles.RemoveAt(CurrentDesignFileIndex);
            _selectedModel.SchematicLinks.RemoveAt(CurrentDesignFileIndex);
            MasterList.RemoveAt(CurrentDesignFileIndex);
            flowSchematicLinks.Controls.Clear();
        }

        #region BackGround Process DBOps
        private void BckgrndProcessDBOps_DoWork(object sender, DoWorkEventArgs e)
        {
            if (e.Argument == null)
            {
                Console.WriteLine("[BackgrounProcessDBOps]::DoWork -> Null AssemblyLinkLabel Passed");
                return;
            }

            AssemblyLinkLabel BoMFile = (AssemblyLinkLabel) e.Argument;

            var targetFile = ELECROOTDIR + BoMFile.Link;
            var localFile = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + $"\\{DateTime.Now.Ticks}" + BoMFile.Link.Split('\\').Last();
            Excel.Application excel = null;
            Workbooks allBooks = null;
            Workbook macroBook = null;
            Sheets allSheets = null;
            Worksheet jukiSheet = null;

            #region Insert OR Update data in DB Table

            var mergeQuery = "MERGE [Repair].[dbo].[BoMInfo] AS TARGET " +
                                                   "USING (SELECT @BoardNumber as BoardNumber, @RefDes AS RefDes, @Revision AS REV) AS SOURCE " +
                                                   "ON (TARGET.BoardNumber = SOURCE.BoardNumber " +
                                                   "    AND TARGET.Rev = SOURCE.REV " +
                                                   "    AND TARGET.ReferenceDesignator = SOURCE.RefDes) " +
                                                   "WHEN MATCHED THEN " +
                                                   "    UPDATE SET " +
                                                   "    PartNumber = @PartNum, " +
                                                   "    Rev = @Revision, " +
                                                   "    ECO = @ECONum " +
                                                   "WHEN NOT MATCHED THEN " +
                                                   "    INSERT (BoardNumber, ReferenceDesignator, PartNumber, Rev, ECO) " +
                                                   "    VALUES (@BoardNumber, @RefDes, @PartNum, @Revision, @ECONum) " +
                                                   ";";

            using (SqlConnection conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
            using (SqlCommand cmd = new SqlCommand(mergeQuery, conn))
            {
                try
                {
                    conn.Open();

                    #region Create XL Sheet Local Copy
                    if (File.Exists(targetFile))
                        File.Copy(targetFile, localFile, true);
                    #endregion

                    excel = new Excel.Application();
                    allBooks = excel.Workbooks;
                    macroBook = allBooks.Open(localFile);
                    allSheets = macroBook.Sheets;
                    jukiSheet = (Worksheet)allSheets["JUKI"];

                    Invoke(new UIAction(() => lblStatus.Text = $"Pushing {BoMFile.Link.Split('\\').Last()} data to DB..."));

                    cmd.Parameters.AddRange(new SqlParameter[] {
                            new SqlParameter("BoardNumber",System.Data.SqlDbType.VarChar, 100),
                            new SqlParameter("RefDes",System.Data.SqlDbType.VarChar, 100),
                            new SqlParameter("PartNum",System.Data.SqlDbType.VarChar, 200),
                            new SqlParameter("Revision",System.Data.SqlDbType.VarChar, 50),
                            new SqlParameter("ECONum",System.Data.SqlDbType.VarChar, 500)
                        });

                    cmd.Parameters["BoardNumber"].Value = Invoke(new UIFunc(() => txtFullAssemblyNumber.Text));

                    if (BoMFile.REV != null && !string.IsNullOrWhiteSpace(BoMFile.REV))
                        cmd.Parameters["Revision"].Value = BoMFile.REV;
                    else
                        cmd.Parameters["Revision"].Value = DBNull.Value;


                    if (BoMFile.ECO != null && !string.IsNullOrWhiteSpace(BoMFile.ECO))
                        cmd.Parameters["ECONum"].Value = BoMFile.ECO;
                    else
                        cmd.Parameters["ECONum"].Value = DBNull.Value;

                    var usedRows = jukiSheet.UsedRange.Rows.Count;

                    Invoke(new UIAction(() => progbarStatus.Maximum = usedRows));

                    for (int row = 1; row < usedRows; row++)
                    {
                        Range refDes = ((Range)jukiSheet.Cells[row, 1]);
                        Range partNum = ((Range)jukiSheet.Cells[row, 5]);

                        var rd = refDes.Value2 == null ? "" : refDes.Value2.ToString();
                        var pn = partNum.Value2 == null ? "" : partNum.Value2.ToString();

                        if (!string.IsNullOrEmpty(rd) && !string.IsNullOrEmpty(pn))
                        {
                            cmd.Parameters["RefDes"].Value = rd;
                            cmd.Parameters["PartNum"].Value = pn;
                            var rowsAffected = -100;
                            try {
                                rowsAffected = cmd.ExecuteNonQuery();
                            } catch (SqlException sqlex) {
                                if (rowsAffected == 0) File.AppendAllText(DBUpload_Log, $"\n\tError updating reference designator: {rd}\n\t\tMessage: {sqlex.Message}\n");
                            }
                        }

                        bckgrndProcessDBOps.ReportProgress(row);

                        if (row + 1 >= usedRows)
                        {
                            Marshal.FinalReleaseComObject(refDes);
                            Marshal.FinalReleaseComObject(partNum);
                        }
                    }

                    conn.Close();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); Console.WriteLine(ex.Message); }
                finally
                {
                    // Cleanup
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    if (jukiSheet != null) Marshal.FinalReleaseComObject(jukiSheet);
                    if (allSheets != null) Marshal.FinalReleaseComObject(allSheets);
                    if (macroBook != null)
                    {
                        macroBook.Close(false, Type.Missing, Type.Missing);
                        Marshal.FinalReleaseComObject(macroBook);
                    }
                    if (allBooks != null) Marshal.FinalReleaseComObject(allBooks);

                    if (excel != null)
                    {
                        excel.Quit();
                        Marshal.FinalReleaseComObject(excel);
                    }

                    File.Delete(localFile);
                    File.AppendAllText(DBUpload_Log, $"}} End Record @ {DateTime.Now:hh:mm:ss tt}\n\n");

                    Invoke(new UIAction(() => {
                        Update();
                        BringToFront();
                        spltpnlActualForm.Panel2Collapsed = true;
                        lblStatus.Text = "";
                        progbarStatus.Value = 0;
                    }));
                }
            }

            #endregion
        }

        private void bckgrndProcessDBOps_ProgressChanged(object sender, ProgressChangedEventArgs e) =>
            Invoke(new UIAction(() => {
                progbarStatus.Value = e.ProgressPercentage;
            }));
        #endregion

        private void uploadBOMDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool PreviousDataFound = false;
            AssemblyLinkLabel assemblyLink = (flowBOMFiles.Controls[BOMFileIndex] as AssemblyLinkLabel);
            spltpnlActualForm.Panel2Collapsed = false;

            foreach (var p in Process.GetProcessesByName("EXCEL")) p.Kill();
            File.AppendAllText(DBUpload_Log, $"Start {Environment.MachineName}\\{Environment.UserName} Record @ {DateTime.Now:hh:mm:ss tt} {{\n");
            
            MessageBox.Show("Starting upload of BOM data!");

            try
            {
                var selectQuery = "SELECT TOP(1) * FROM [Repair].[dbo].[BoMInfo] WHERE [BoardNumber] = @BoardNumber AND " +
                    $"[Rev] {(!string.IsNullOrEmpty(assemblyLink.REV) ? "= @Revision" : "IS NULL")}";// AND ECO = @ECO

                using (SqlConnection conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(selectQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@BoardNumber", txtFullAssemblyNumber?.Text ?? "");
                        cmd.Parameters.AddWithValue("@Revision", assemblyLink?.REV ?? "");

                        try
                        {
                            conn.Open();
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if(reader.Read())
                                {
                                    PreviousDataFound = true;
                                }
                            }
                            conn.Close();
                        }
                        catch (Exception ex) { MessageBox.Show(ex.Message); }
                    }
                }

                DialogResult replaceData = DialogResult.No;
                if(PreviousDataFound)
                {
                    replaceData = MessageBox.Show($"We found data for {assemblyLink.Text} with REV {assemblyLink.REV} on the database!\n" +
                        "Would you like to replace this data?",
                        "BOM Info - Found Previous Data in Database!",MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                }

                if (replaceData == DialogResult.Yes)
                {
                    #region Delete Old Entries
                    var deleteQuery = "DELETE FROM [Repair].[dbo].[BoMInfo] WHERE [BoardNumber] = @BoardNumber AND " +
                        $"[Rev] {(!string.IsNullOrEmpty(assemblyLink.REV) ? "= @Revision" : "IS NULL")}";// AND [ECO] = @ECONum";

                    using (SqlConnection conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().RepairConnectionString))
                    using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                    {
                        try
                        {
                            conn.Open();

                            cmd.Parameters.AddWithValue("@BoardNumber", txtFullAssemblyNumber?.Text ?? "");
                            cmd.Parameters.AddWithValue("@Revision", assemblyLink?.REV ?? "");
                            cmd.Parameters.AddWithValue("@ECONum", assemblyLink?.ECO ?? "");

                            int rowsAffected = cmd.ExecuteNonQuery();
                            conn.Close();
                        }
                        catch (Exception ex) { MessageBox.Show("Inside of Delete Query:\n" + ex.Message); Console.WriteLine(ex.Message); }
                    }

                    #endregion

                    bckgrndProcessDBOps.RunWorkerAsync(assemblyLink);
                } else
                {
                    spltpnlActualForm.Panel2Collapsed = true;
                    lblStatus.Text = "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError Message => {ex.Message}|\nStack Trace {{\n{ex.StackTrace}\n}}\n");
                csExceptionLogger.csExceptionLogger.Write("EV_uploadBOMData", ex);
            }
        }
        #endregion

        private void lblFullAssemblyNumberLabel_DoubleClick(object sender, EventArgs e) => Process.Start(ELECROOTDIR);

        private void cxmnuAssemblyLinksMenu_Opening(object sender, CancelEventArgs e)
        {
            deleteSchematicLinkToolStripMenuItem.Text = bBOM ? "Delete BOM File..." : "Delete Schematic Link...";
            uploadBOMDataToolStripMenuItem.Visible = bBOM; // && Extensions.Devs.Contains(Environment.UserName);
            markAsActiveToolStripMenuItem.Visible = bBOM;

            tssMarkAsActive.Visible = bBOM;
            tssUploadBOMData.Visible = bBOM;
        }

        private void cxmnuBOMFlowMenu_Opening(object sender, CancelEventArgs e)
        {
            if (cxmnuAssemblyLinksMenu.Visible) e.Cancel = true;
        }

        private void cxmnuSchematicFlowMenu_Opening(object sender, CancelEventArgs e)
        {
            if (cxmnuAssemblyLinksMenu.Visible) e.Cancel = true;
        }

        private DateTime LastHelp = DateTime.Now.AddMinutes(-5);
        private void btnRemoteHelp_Click(object sender, EventArgs e)
        {
            if (DateTime.Now > LastHelp.AddMinutes(5))
            {
                LastHelp = DateTime.Now;
#if !DEBUG
                new FNS.FrmFirebaseContactForm().ShowDialog();
#else
                FNS.Classes.FirebasePushService.SendPushNotification("fGaGwvh6EDE:APA91bG0mr3kNHuMItXz_C8DxZbBQFIgC7ADOOXLNEluAkwO9l-7Md-xYLWsiyy_4jiKyiGbwojjPhneL2Wf-AlpzJ5_IPhiQqwddaff11_Y5zTDtJ3WeO5h3kcBJ07sfj5xzKiJwOAE",
                    $"Remote Assitance for {Environment.UserName}", $"RApID on {Environment.MachineName}", "https://drive.google.com/open?id=1Xpt7g2DoBImjtan3DQdzYwHQ_CkUV125");
#endif
            }
            else
            {
                var minute_time = DateTime.Now.Subtract(LastHelp).TotalMinutes;
                MessageBox.Show($"Help message was sent {(minute_time >= 2 ? $"{(int)minute_time} minutes" : "about a minute")} ago.\nPlease wait 5 minutes before next request.",
                    "Please Don't Spam Help", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        #region Link Label
        private void lnkBOMFile_MouseDown(object sender, MouseEventArgs e)
        {
            AssemblyLinkLabel callinglink = (sender as AssemblyLinkLabel);

            if (e.Button == MouseButtons.Right)
            {
                bSchematic = false;
                bBOM = true;

                BOMFileIndex = 0;
                foreach (Control c in flowBOMFiles.Controls)
                {
                    if (c is AssemblyLinkLabel link && link == callinglink)
                        break;
                    else
                        BOMFileIndex++;
                }
                cxmnuAssemblyLinksMenu.Show(MousePosition.X, MousePosition.Y);
            }
        }

        private void lnkSchematicFile_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                bSchematic = true;
                bBOM = false;

                SchematicFileIndex = -1;
                foreach (Control c in flowSchematicLinks.Controls)
                {
                    if (this.ClickIsInBounds(c, e.Location)) break;
                    else SchematicFileIndex++;
                }
                cxmnuAssemblyLinksMenu.Show(MousePosition.X, MousePosition.Y);
            }
        }
        #endregion

        #endregion

        #region Database Tab
        private void tcDataViewer_SelectedIndexChanged(object sender, EventArgs e)
            => new TechAliasTableAdapter().Fill(pCBAliasDataSet.TechAlias);

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new TechAliasTableAdapter().Fill(pCBAliasDataSet.TechAlias);
        }

        private void dgvDatabaseTable_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvDatabaseTable.Rows.Count < 1 || e.RowIndex < 0 || e.ColumnIndex < 0) return;

            txtFullAssemblyNumber.Text = dgvDatabaseTable.Rows[e.RowIndex].Cells[0].Value.ToString();
            tcDataViewer.SelectedIndex = 0;
            HandleTextBoxEntry(new KeyEventArgs(Keys.Enter));
        }
        #endregion
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
        public WhiningException(string message = "") : base(message) { }
    }

    /// <summary>
    /// Model for all data shown in the form (row data in SQL table also)
    /// </summary>
    [DataContract]
    public class DesignFileSet : INotifyPropertyChanged
    {
        #region PropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        readonly string CallerMemberName;

        #region Properties
        [DataMember]
        private string _partNumber = string.Empty;
        public string PartNumber
        {
            get => _partNumber;
            set
            {
                _partNumber = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        private string _partName = string.Empty;
        public string PartName
        {
            get => _partName;
            set
            {
                _partName = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        private string _commodityClass = string.Empty;
        public string CommodityClass
        {
            get => _commodityClass;
            set
            {
                _commodityClass = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        private List<AssemblyLinkLabel> _bomFiles = new List<AssemblyLinkLabel>();
        public List<AssemblyLinkLabel> BOMFiles
        {
            get => _bomFiles;
            set
            {
                _bomFiles = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        private List<string> _bomTags = new List<string>();
        public List<string> BOMTags
        {
            get => _bomTags;
            set
            {
                _bomTags = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        private List<AssemblyLinkLabel> _schematicLinks = new List<AssemblyLinkLabel>();
        public List<AssemblyLinkLabel> SchematicLinks
        {
            get => _schematicLinks;
            set
            {
                _schematicLinks = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        private List<string> _assyTags = new List<string>();
        public List<string> SchematicTags
        {
            get => _assyTags;
            set
            {
                _assyTags = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        private string _rev;
        public string REV
        {
            get => _rev;
            set
            {
                _rev = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        private string _eco;
        public string ECO
        {
            get => _eco;
            set
            {
                _eco = value;
                OnPropertyChanged();
            }
        }


        #endregion

        public DesignFileSet([CallerMemberName] string callerName = "")
        {
            CallerMemberName = callerName;
        }

        public override string ToString()
            => $"[RecordDataModel] =>{{\n" +
            $"\t{nameof(PartNumber)} = \"{PartNumber}\"\n" +
            $"\t{nameof(REV)} = \"{REV}\"\n" +
            $"\t{nameof(ECO)} = \"{ECO}\"\n" +
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
