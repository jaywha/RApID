namespace RApID_Project_WPF
{
    partial class frmBoardFileManager
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmBoardFileManager));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.cxmnuSchematicLinksMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.changeFilePathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteSchematicLinkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cxmnuDatabaseMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.techAliasesBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.pCBAliasDataSet = new RApID_Project_WPF.PCBAliasDataSet();
            this.imgSchematics = new System.Windows.Forms.ImageList(this.components);
            this.lblWarning = new System.Windows.Forms.Label();
            this.spltpnlMain = new System.Windows.Forms.SplitContainer();
            this.spltpnlActualForm = new System.Windows.Forms.SplitContainer();
            this.tcDataViewer = new System.Windows.Forms.TabControl();
            this.tbTechnicianView = new System.Windows.Forms.TabPage();
            this.spltpnlTechView = new System.Windows.Forms.SplitContainer();
            this.spltpnlPartNumToDetail = new System.Windows.Forms.SplitContainer();
            this.flopnlPartNumberInput = new System.Windows.Forms.FlowLayoutPanel();
            this.lblPartNumberLabel = new System.Windows.Forms.Label();
            this.txtPartNumber = new System.Windows.Forms.TextBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.grpbxBOMLinkHolder = new System.Windows.Forms.GroupBox();
            this.flowBOMFiles = new System.Windows.Forms.FlowLayoutPanel();
            this.spltpnlAliasToDetail = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.grpbxPartNumberDetail = new System.Windows.Forms.GroupBox();
            this.lblCommodityClass = new System.Windows.Forms.Label();
            this.lblPartName = new System.Windows.Forms.Label();
            this.pnlFilePaths = new System.Windows.Forms.Panel();
            this.grpbxSchematicFiles = new System.Windows.Forms.GroupBox();
            this.flowSchematicLinks = new System.Windows.Forms.FlowLayoutPanel();
            this.tbDatabaseView = new System.Windows.Forms.TabPage();
            this.dgvDatabaseTable = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.progbarStatus = new System.Windows.Forms.ToolStripProgressBar();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.techAliasesTableAdapter = new RApID_Project_WPF.PCBAliasDataSetTableAdapters.TechAliasesTableAdapter();
            this.infoProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.cxmnuSchematicLinksMenu.SuspendLayout();
            this.cxmnuDatabaseMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.techAliasesBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pCBAliasDataSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spltpnlMain)).BeginInit();
            this.spltpnlMain.Panel1.SuspendLayout();
            this.spltpnlMain.Panel2.SuspendLayout();
            this.spltpnlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spltpnlActualForm)).BeginInit();
            this.spltpnlActualForm.Panel1.SuspendLayout();
            this.spltpnlActualForm.Panel2.SuspendLayout();
            this.spltpnlActualForm.SuspendLayout();
            this.tcDataViewer.SuspendLayout();
            this.tbTechnicianView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spltpnlTechView)).BeginInit();
            this.spltpnlTechView.Panel1.SuspendLayout();
            this.spltpnlTechView.Panel2.SuspendLayout();
            this.spltpnlTechView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spltpnlPartNumToDetail)).BeginInit();
            this.spltpnlPartNumToDetail.Panel1.SuspendLayout();
            this.spltpnlPartNumToDetail.Panel2.SuspendLayout();
            this.spltpnlPartNumToDetail.SuspendLayout();
            this.flopnlPartNumberInput.SuspendLayout();
            this.grpbxBOMLinkHolder.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spltpnlAliasToDetail)).BeginInit();
            this.spltpnlAliasToDetail.Panel1.SuspendLayout();
            this.spltpnlAliasToDetail.Panel2.SuspendLayout();
            this.spltpnlAliasToDetail.SuspendLayout();
            this.panel1.SuspendLayout();
            this.grpbxPartNumberDetail.SuspendLayout();
            this.pnlFilePaths.SuspendLayout();
            this.grpbxSchematicFiles.SuspendLayout();
            this.tbDatabaseView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDatabaseTable)).BeginInit();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.infoProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // cxmnuSchematicLinksMenu
            // 
            this.cxmnuSchematicLinksMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.changeFilePathToolStripMenuItem,
            this.changeTagToolStripMenuItem,
            this.toolStripSeparator1,
            this.deleteSchematicLinkToolStripMenuItem});
            this.cxmnuSchematicLinksMenu.Name = "cxmnuLinkMenu";
            this.cxmnuSchematicLinksMenu.Size = new System.Drawing.Size(200, 76);
            this.cxmnuSchematicLinksMenu.Closed += new System.Windows.Forms.ToolStripDropDownClosedEventHandler(this.cxmnuSchematicLinksMenu_Closed);
            this.cxmnuSchematicLinksMenu.Opened += new System.EventHandler(this.cxmnuSchematicLinksMenu_Opened);
            // 
            // changeFilePathToolStripMenuItem
            // 
            this.changeFilePathToolStripMenuItem.Name = "changeFilePathToolStripMenuItem";
            this.changeFilePathToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.changeFilePathToolStripMenuItem.Text = "Change File Path...";
            this.changeFilePathToolStripMenuItem.Click += new System.EventHandler(this.changeFilePathToolStripMenuItem_Click);
            // 
            // changeTagToolStripMenuItem
            // 
            this.changeTagToolStripMenuItem.Name = "changeTagToolStripMenuItem";
            this.changeTagToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.changeTagToolStripMenuItem.Text = "Change Tag...";
            this.changeTagToolStripMenuItem.Click += new System.EventHandler(this.ChangeTag_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(196, 6);
            // 
            // deleteSchematicLinkToolStripMenuItem
            // 
            this.deleteSchematicLinkToolStripMenuItem.Name = "deleteSchematicLinkToolStripMenuItem";
            this.deleteSchematicLinkToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.deleteSchematicLinkToolStripMenuItem.Text = "Delete Schematic Link...";
            this.deleteSchematicLinkToolStripMenuItem.Click += new System.EventHandler(this.deleteSchematicLinkToolStripMenuItem_Click);
            // 
            // cxmnuDatabaseMenu
            // 
            this.cxmnuDatabaseMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.refreshToolStripMenuItem});
            this.cxmnuDatabaseMenu.Name = "cxmnuDatabaseMenu";
            this.cxmnuDatabaseMenu.Size = new System.Drawing.Size(114, 26);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Image = global::RApID_Project_WPF.Properties.Resources.refresh;
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(113, 22);
            this.refreshToolStripMenuItem.Text = "Refresh";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // techAliasesBindingSource
            // 
            this.techAliasesBindingSource.DataMember = "TechAliases";
            this.techAliasesBindingSource.DataSource = this.pCBAliasDataSet;
            // 
            // pCBAliasDataSet
            // 
            this.pCBAliasDataSet.DataSetName = "PCBAliasDataSet";
            this.pCBAliasDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // imgSchematics
            // 
            this.imgSchematics.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgSchematics.ImageStream")));
            this.imgSchematics.TransparentColor = System.Drawing.Color.Transparent;
            this.imgSchematics.Images.SetKeyName(0, "pdf");
            this.imgSchematics.Images.SetKeyName(1, "asc");
            this.imgSchematics.Images.SetKeyName(2, "other");
            this.imgSchematics.Images.SetKeyName(3, "xls");
            this.imgSchematics.Images.SetKeyName(4, "xlsx");
            // 
            // lblWarning
            // 
            this.lblWarning.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblWarning.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWarning.Location = new System.Drawing.Point(0, 0);
            this.lblWarning.Name = "lblWarning";
            this.lblWarning.Size = new System.Drawing.Size(800, 53);
            this.lblWarning.TabIndex = 0;
            this.lblWarning.Text = "Database Changes Are Immediately LIVE";
            this.lblWarning.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // spltpnlMain
            // 
            this.spltpnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spltpnlMain.IsSplitterFixed = true;
            this.spltpnlMain.Location = new System.Drawing.Point(0, 0);
            this.spltpnlMain.Name = "spltpnlMain";
            this.spltpnlMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // spltpnlMain.Panel1
            // 
            this.spltpnlMain.Panel1.BackColor = System.Drawing.Color.Black;
            this.spltpnlMain.Panel1.Controls.Add(this.lblWarning);
            this.spltpnlMain.Panel1.ForeColor = System.Drawing.Color.Goldenrod;
            // 
            // spltpnlMain.Panel2
            // 
            this.spltpnlMain.Panel2.Controls.Add(this.spltpnlActualForm);
            this.spltpnlMain.Size = new System.Drawing.Size(800, 554);
            this.spltpnlMain.SplitterDistance = 53;
            this.spltpnlMain.TabIndex = 0;
            // 
            // spltpnlActualForm
            // 
            this.spltpnlActualForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spltpnlActualForm.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.spltpnlActualForm.IsSplitterFixed = true;
            this.spltpnlActualForm.Location = new System.Drawing.Point(0, 0);
            this.spltpnlActualForm.Name = "spltpnlActualForm";
            this.spltpnlActualForm.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // spltpnlActualForm.Panel1
            // 
            this.spltpnlActualForm.Panel1.BackColor = System.Drawing.Color.Black;
            this.spltpnlActualForm.Panel1.Controls.Add(this.tcDataViewer);
            // 
            // spltpnlActualForm.Panel2
            // 
            this.spltpnlActualForm.Panel2.BackColor = System.Drawing.Color.Black;
            this.spltpnlActualForm.Panel2.Controls.Add(this.statusStrip1);
            this.spltpnlActualForm.Panel2Collapsed = true;
            this.spltpnlActualForm.Size = new System.Drawing.Size(800, 497);
            this.spltpnlActualForm.SplitterDistance = 471;
            this.spltpnlActualForm.SplitterWidth = 1;
            this.spltpnlActualForm.TabIndex = 3;
            // 
            // tcDataViewer
            // 
            this.tcDataViewer.Controls.Add(this.tbTechnicianView);
            this.tcDataViewer.Controls.Add(this.tbDatabaseView);
            this.tcDataViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcDataViewer.Location = new System.Drawing.Point(0, 0);
            this.tcDataViewer.Name = "tcDataViewer";
            this.tcDataViewer.SelectedIndex = 0;
            this.tcDataViewer.Size = new System.Drawing.Size(800, 497);
            this.tcDataViewer.TabIndex = 2;
            // 
            // tbTechnicianView
            // 
            this.tbTechnicianView.BackColor = System.Drawing.Color.DarkGoldenrod;
            this.tbTechnicianView.Controls.Add(this.spltpnlTechView);
            this.tbTechnicianView.Location = new System.Drawing.Point(4, 22);
            this.tbTechnicianView.Name = "tbTechnicianView";
            this.tbTechnicianView.Padding = new System.Windows.Forms.Padding(3);
            this.tbTechnicianView.Size = new System.Drawing.Size(792, 471);
            this.tbTechnicianView.TabIndex = 0;
            this.tbTechnicianView.Text = "Technician View";
            this.tbTechnicianView.ToolTipText = "Shows main format for managing part number aliases.";
            // 
            // spltpnlTechView
            // 
            this.spltpnlTechView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spltpnlTechView.IsSplitterFixed = true;
            this.spltpnlTechView.Location = new System.Drawing.Point(3, 3);
            this.spltpnlTechView.Name = "spltpnlTechView";
            this.spltpnlTechView.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // spltpnlTechView.Panel1
            // 
            this.spltpnlTechView.Panel1.Controls.Add(this.spltpnlPartNumToDetail);
            // 
            // spltpnlTechView.Panel2
            // 
            this.spltpnlTechView.Panel2.Controls.Add(this.spltpnlAliasToDetail);
            this.spltpnlTechView.Size = new System.Drawing.Size(786, 465);
            this.spltpnlTechView.SplitterDistance = 208;
            this.spltpnlTechView.TabIndex = 6;
            // 
            // spltpnlPartNumToDetail
            // 
            this.spltpnlPartNumToDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spltpnlPartNumToDetail.Location = new System.Drawing.Point(0, 0);
            this.spltpnlPartNumToDetail.Name = "spltpnlPartNumToDetail";
            // 
            // spltpnlPartNumToDetail.Panel1
            // 
            this.spltpnlPartNumToDetail.Panel1.Controls.Add(this.flopnlPartNumberInput);
            // 
            // spltpnlPartNumToDetail.Panel2
            // 
            this.spltpnlPartNumToDetail.Panel2.Controls.Add(this.grpbxBOMLinkHolder);
            this.spltpnlPartNumToDetail.Size = new System.Drawing.Size(786, 208);
            this.spltpnlPartNumToDetail.SplitterDistance = 281;
            this.spltpnlPartNumToDetail.TabIndex = 0;
            // 
            // flopnlPartNumberInput
            // 
            this.flopnlPartNumberInput.BackColor = System.Drawing.Color.Black;
            this.flopnlPartNumberInput.Controls.Add(this.lblPartNumberLabel);
            this.flopnlPartNumberInput.Controls.Add(this.txtPartNumber);
            this.flopnlPartNumberInput.Controls.Add(this.btnSearch);
            this.flopnlPartNumberInput.Controls.Add(this.btnReset);
            this.flopnlPartNumberInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flopnlPartNumberInput.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flopnlPartNumberInput.ForeColor = System.Drawing.Color.Goldenrod;
            this.flopnlPartNumberInput.Location = new System.Drawing.Point(0, 0);
            this.flopnlPartNumberInput.Name = "flopnlPartNumberInput";
            this.flopnlPartNumberInput.Size = new System.Drawing.Size(281, 208);
            this.flopnlPartNumberInput.TabIndex = 0;
            // 
            // lblPartNumberLabel
            // 
            this.lblPartNumberLabel.AutoSize = true;
            this.lblPartNumberLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPartNumberLabel.ForeColor = System.Drawing.Color.Goldenrod;
            this.lblPartNumberLabel.Location = new System.Drawing.Point(3, 0);
            this.lblPartNumberLabel.Name = "lblPartNumberLabel";
            this.lblPartNumberLabel.Size = new System.Drawing.Size(102, 20);
            this.lblPartNumberLabel.TabIndex = 0;
            this.lblPartNumberLabel.Text = "Part Number:";
            // 
            // txtPartNumber
            // 
            this.txtPartNumber.BackColor = System.Drawing.Color.Black;
            this.txtPartNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPartNumber.ForeColor = System.Drawing.Color.Goldenrod;
            this.txtPartNumber.Location = new System.Drawing.Point(3, 23);
            this.txtPartNumber.Name = "txtPartNumber";
            this.txtPartNumber.Size = new System.Drawing.Size(144, 26);
            this.txtPartNumber.TabIndex = 1;
            this.txtPartNumber.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtPartNumber_KeyDown);
            // 
            // btnSearch
            // 
            this.btnSearch.BackColor = System.Drawing.Color.DarkGreen;
            this.btnSearch.ForeColor = System.Drawing.Color.Gold;
            this.btnSearch.Location = new System.Drawing.Point(3, 55);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(144, 23);
            this.btnSearch.TabIndex = 3;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = false;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // btnReset
            // 
            this.btnReset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnReset.Location = new System.Drawing.Point(3, 84);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(144, 23);
            this.btnReset.TabIndex = 4;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = false;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // grpbxBOMLinkHolder
            // 
            this.grpbxBOMLinkHolder.BackColor = System.Drawing.Color.Black;
            this.grpbxBOMLinkHolder.Controls.Add(this.flowBOMFiles);
            this.grpbxBOMLinkHolder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpbxBOMLinkHolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpbxBOMLinkHolder.ForeColor = System.Drawing.Color.Gold;
            this.grpbxBOMLinkHolder.Location = new System.Drawing.Point(0, 0);
            this.grpbxBOMLinkHolder.Name = "grpbxBOMLinkHolder";
            this.grpbxBOMLinkHolder.Size = new System.Drawing.Size(501, 208);
            this.grpbxBOMLinkHolder.TabIndex = 6;
            this.grpbxBOMLinkHolder.TabStop = false;
            this.grpbxBOMLinkHolder.Text = "BOM File Link";
            // 
            // flowBOMFiles
            // 
            this.flowBOMFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowBOMFiles.Location = new System.Drawing.Point(3, 22);
            this.flowBOMFiles.Name = "flowBOMFiles";
            this.flowBOMFiles.Size = new System.Drawing.Size(495, 183);
            this.flowBOMFiles.TabIndex = 0;
            this.flowBOMFiles.MouseDown += new System.Windows.Forms.MouseEventHandler(this.flowBOMFiles_MouseDown);
            // 
            // spltpnlAliasToDetail
            // 
            this.spltpnlAliasToDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spltpnlAliasToDetail.Location = new System.Drawing.Point(0, 0);
            this.spltpnlAliasToDetail.Name = "spltpnlAliasToDetail";
            // 
            // spltpnlAliasToDetail.Panel1
            // 
            this.spltpnlAliasToDetail.Panel1.Controls.Add(this.panel1);
            // 
            // spltpnlAliasToDetail.Panel2
            // 
            this.spltpnlAliasToDetail.Panel2.Controls.Add(this.pnlFilePaths);
            this.spltpnlAliasToDetail.Size = new System.Drawing.Size(786, 253);
            this.spltpnlAliasToDetail.SplitterDistance = 280;
            this.spltpnlAliasToDetail.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Black;
            this.panel1.Controls.Add(this.grpbxPartNumberDetail);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(280, 253);
            this.panel1.TabIndex = 0;
            // 
            // grpbxPartNumberDetail
            // 
            this.grpbxPartNumberDetail.BackColor = System.Drawing.Color.Black;
            this.grpbxPartNumberDetail.Controls.Add(this.lblCommodityClass);
            this.grpbxPartNumberDetail.Controls.Add(this.lblPartName);
            this.grpbxPartNumberDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpbxPartNumberDetail.ForeColor = System.Drawing.Color.Goldenrod;
            this.grpbxPartNumberDetail.Location = new System.Drawing.Point(0, 0);
            this.grpbxPartNumberDetail.Name = "grpbxPartNumberDetail";
            this.grpbxPartNumberDetail.Size = new System.Drawing.Size(280, 253);
            this.grpbxPartNumberDetail.TabIndex = 0;
            this.grpbxPartNumberDetail.TabStop = false;
            this.grpbxPartNumberDetail.Text = "Part Number Details";
            // 
            // lblCommodityClass
            // 
            this.lblCommodityClass.AutoSize = true;
            this.lblCommodityClass.Location = new System.Drawing.Point(6, 36);
            this.lblCommodityClass.Name = "lblCommodityClass";
            this.lblCommodityClass.Size = new System.Drawing.Size(138, 13);
            this.lblCommodityClass.TabIndex = 1;
            this.lblCommodityClass.Text = "Commodity Class: <CLASS>";
            // 
            // lblPartName
            // 
            this.lblPartName.AutoSize = true;
            this.lblPartName.Location = new System.Drawing.Point(7, 20);
            this.lblPartName.Name = "lblPartName";
            this.lblPartName.Size = new System.Drawing.Size(106, 13);
            this.lblPartName.TabIndex = 0;
            this.lblPartName.Text = "Part Name: <NAME>";
            // 
            // pnlFilePaths
            // 
            this.pnlFilePaths.BackColor = System.Drawing.Color.Black;
            this.pnlFilePaths.Controls.Add(this.grpbxSchematicFiles);
            this.pnlFilePaths.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFilePaths.ForeColor = System.Drawing.Color.Gold;
            this.pnlFilePaths.Location = new System.Drawing.Point(0, 0);
            this.pnlFilePaths.Name = "pnlFilePaths";
            this.pnlFilePaths.Size = new System.Drawing.Size(502, 253);
            this.pnlFilePaths.TabIndex = 5;
            // 
            // grpbxSchematicFiles
            // 
            this.grpbxSchematicFiles.Controls.Add(this.flowSchematicLinks);
            this.grpbxSchematicFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpbxSchematicFiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpbxSchematicFiles.ForeColor = System.Drawing.Color.Gold;
            this.grpbxSchematicFiles.Location = new System.Drawing.Point(0, 0);
            this.grpbxSchematicFiles.Name = "grpbxSchematicFiles";
            this.grpbxSchematicFiles.Size = new System.Drawing.Size(502, 253);
            this.grpbxSchematicFiles.TabIndex = 7;
            this.grpbxSchematicFiles.TabStop = false;
            this.grpbxSchematicFiles.Text = "Schematic File Links";
            // 
            // flowSchematicLinks
            // 
            this.flowSchematicLinks.AutoScroll = true;
            this.flowSchematicLinks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowSchematicLinks.ForeColor = System.Drawing.Color.Gold;
            this.flowSchematicLinks.Location = new System.Drawing.Point(3, 22);
            this.flowSchematicLinks.Name = "flowSchematicLinks";
            this.flowSchematicLinks.Size = new System.Drawing.Size(496, 228);
            this.flowSchematicLinks.TabIndex = 0;
            this.flowSchematicLinks.MouseDown += new System.Windows.Forms.MouseEventHandler(this.flowSchematicLinks_MouseDown);
            // 
            // tbDatabaseView
            // 
            this.tbDatabaseView.Controls.Add(this.dgvDatabaseTable);
            this.tbDatabaseView.Location = new System.Drawing.Point(4, 22);
            this.tbDatabaseView.Name = "tbDatabaseView";
            this.tbDatabaseView.Padding = new System.Windows.Forms.Padding(3);
            this.tbDatabaseView.Size = new System.Drawing.Size(792, 471);
            this.tbDatabaseView.TabIndex = 1;
            this.tbDatabaseView.Text = "Database View";
            this.tbDatabaseView.ToolTipText = "Shows data using SQL Server Edit format.";
            this.tbDatabaseView.UseVisualStyleBackColor = true;
            // 
            // dgvDatabaseTable
            // 
            this.dgvDatabaseTable.AllowUserToAddRows = false;
            this.dgvDatabaseTable.AllowUserToDeleteRows = false;
            this.dgvDatabaseTable.AutoGenerateColumns = false;
            this.dgvDatabaseTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvDatabaseTable.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvDatabaseTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDatabaseTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3});
            this.dgvDatabaseTable.ContextMenuStrip = this.cxmnuDatabaseMenu;
            this.dgvDatabaseTable.DataSource = this.techAliasesBindingSource;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.DimGray;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.HotTrack;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlLightLight;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvDatabaseTable.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvDatabaseTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDatabaseTable.Location = new System.Drawing.Point(3, 3);
            this.dgvDatabaseTable.Name = "dgvDatabaseTable";
            this.dgvDatabaseTable.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvDatabaseTable.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvDatabaseTable.Size = new System.Drawing.Size(786, 465);
            this.dgvDatabaseTable.TabIndex = 0;
            this.dgvDatabaseTable.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvDatabaseTable_CellDoubleClick);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "PartNumber";
            this.dataGridViewTextBoxColumn1.HeaderText = "PartNumber";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.DataPropertyName = "BOMPath";
            this.dataGridViewTextBoxColumn2.HeaderText = "BOMPath";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.DataPropertyName = "SchematicPaths";
            this.dataGridViewTextBoxColumn3.HeaderText = "SchematicPaths";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.BackColor = System.Drawing.Color.Black;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.progbarStatus,
            this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 24);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(150, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // progbarStatus
            // 
            this.progbarStatus.ForeColor = System.Drawing.Color.Green;
            this.progbarStatus.Maximum = 3;
            this.progbarStatus.Name = "progbarStatus";
            this.progbarStatus.Size = new System.Drawing.Size(200, 16);
            this.progbarStatus.Step = 1;
            this.progbarStatus.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // lblStatus
            // 
            this.lblStatus.BackColor = System.Drawing.Color.Transparent;
            this.lblStatus.ForeColor = System.Drawing.Color.Goldenrod;
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(59, 15);
            this.lblStatus.Text = "Loading...";
            // 
            // errorProvider
            // 
            this.errorProvider.BlinkRate = 333;
            this.errorProvider.ContainerControl = this;
            // 
            // techAliasesTableAdapter
            // 
            this.techAliasesTableAdapter.ClearBeforeFill = true;
            // 
            // infoProvider
            // 
            this.infoProvider.BlinkRate = 500;
            this.infoProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.infoProvider.ContainerControl = this;
            this.infoProvider.Icon = ((System.Drawing.Icon)(resources.GetObject("infoProvider.Icon")));
            // 
            // frmBoardFileManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 554);
            this.Controls.Add(this.spltpnlMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(750, 400);
            this.Name = "frmBoardFileManager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "PCB File Manager";
            this.Load += new System.EventHandler(this.frmBoardAliases_Load);
            this.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.frmBoardAliases_PreviewKeyDown);
            this.cxmnuSchematicLinksMenu.ResumeLayout(false);
            this.cxmnuDatabaseMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.techAliasesBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pCBAliasDataSet)).EndInit();
            this.spltpnlMain.Panel1.ResumeLayout(false);
            this.spltpnlMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spltpnlMain)).EndInit();
            this.spltpnlMain.ResumeLayout(false);
            this.spltpnlActualForm.Panel1.ResumeLayout(false);
            this.spltpnlActualForm.Panel2.ResumeLayout(false);
            this.spltpnlActualForm.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spltpnlActualForm)).EndInit();
            this.spltpnlActualForm.ResumeLayout(false);
            this.tcDataViewer.ResumeLayout(false);
            this.tbTechnicianView.ResumeLayout(false);
            this.spltpnlTechView.Panel1.ResumeLayout(false);
            this.spltpnlTechView.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spltpnlTechView)).EndInit();
            this.spltpnlTechView.ResumeLayout(false);
            this.spltpnlPartNumToDetail.Panel1.ResumeLayout(false);
            this.spltpnlPartNumToDetail.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spltpnlPartNumToDetail)).EndInit();
            this.spltpnlPartNumToDetail.ResumeLayout(false);
            this.flopnlPartNumberInput.ResumeLayout(false);
            this.flopnlPartNumberInput.PerformLayout();
            this.grpbxBOMLinkHolder.ResumeLayout(false);
            this.spltpnlAliasToDetail.Panel1.ResumeLayout(false);
            this.spltpnlAliasToDetail.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spltpnlAliasToDetail)).EndInit();
            this.spltpnlAliasToDetail.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.grpbxPartNumberDetail.ResumeLayout(false);
            this.grpbxPartNumberDetail.PerformLayout();
            this.pnlFilePaths.ResumeLayout(false);
            this.grpbxSchematicFiles.ResumeLayout(false);
            this.tbDatabaseView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDatabaseTable)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.infoProvider)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ContextMenuStrip cxmnuSchematicLinksMenu;
        private System.Windows.Forms.ToolStripMenuItem changeFilePathToolStripMenuItem;
        private System.Windows.Forms.Label lblWarning;
        private System.Windows.Forms.SplitContainer spltpnlMain;
        private System.Windows.Forms.DataGridViewLinkColumn schematicPathDataGridViewTextBoxColumn;
        private System.Windows.Forms.ImageList imgSchematics;
        private System.Windows.Forms.DataGridViewTextBoxColumn aliasDataGridViewTextBoxColumn;
        private System.Windows.Forms.ErrorProvider errorProvider;
        private System.Windows.Forms.ContextMenuStrip cxmnuDatabaseMenu;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem deleteSchematicLinkToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn partNumberDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn bOMPathDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn bOMFileNameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn schematicPathsDataGridViewTextBoxColumn;
        private PCBAliasDataSet pCBAliasDataSet;
        private System.Windows.Forms.BindingSource techAliasesBindingSource;
        private PCBAliasDataSetTableAdapters.TechAliasesTableAdapter techAliasesTableAdapter;
        private System.Windows.Forms.SplitContainer spltpnlActualForm;
        private System.Windows.Forms.TabControl tcDataViewer;
        private System.Windows.Forms.TabPage tbTechnicianView;
        private System.Windows.Forms.SplitContainer spltpnlTechView;
        private System.Windows.Forms.SplitContainer spltpnlPartNumToDetail;
        private System.Windows.Forms.FlowLayoutPanel flopnlPartNumberInput;
        private System.Windows.Forms.Label lblPartNumberLabel;
        private System.Windows.Forms.TextBox txtPartNumber;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.GroupBox grpbxBOMLinkHolder;
        private System.Windows.Forms.FlowLayoutPanel flowBOMFiles;
        private System.Windows.Forms.SplitContainer spltpnlAliasToDetail;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox grpbxPartNumberDetail;
        private System.Windows.Forms.Label lblCommodityClass;
        private System.Windows.Forms.Label lblPartName;
        private System.Windows.Forms.Panel pnlFilePaths;
        private System.Windows.Forms.GroupBox grpbxSchematicFiles;
        private System.Windows.Forms.FlowLayoutPanel flowSchematicLinks;
        private System.Windows.Forms.TabPage tbDatabaseView;
        private System.Windows.Forms.DataGridView dgvDatabaseTable;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar progbarStatus;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripMenuItem changeTagToolStripMenuItem;
        private System.Windows.Forms.ErrorProvider infoProvider;
    }
}