namespace RApID_Project_WPF
{
    partial class frmBoardAliases
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmBoardAliases));
            this.cxmnuAliasesMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addNewAliasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteAliasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cxmnuLinkMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.changeFilePathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tcDataViewer = new System.Windows.Forms.TabControl();
            this.tbTechnicianView = new System.Windows.Forms.TabPage();
            this.spltpnlTechView = new System.Windows.Forms.SplitContainer();
            this.spltpnlPartNumToDetail = new System.Windows.Forms.SplitContainer();
            this.flopnlPartNumberInput = new System.Windows.Forms.FlowLayoutPanel();
            this.lblPartNumberLabel = new System.Windows.Forms.Label();
            this.txtPartNumber = new System.Windows.Forms.TextBox();
            this.grpbxPartNumberDetail = new System.Windows.Forms.GroupBox();
            this.lblCommodityClass = new System.Windows.Forms.Label();
            this.lblPartName = new System.Windows.Forms.Label();
            this.spltpnlAliasToDetail = new System.Windows.Forms.SplitContainer();
            this.lstbxAliases = new System.Windows.Forms.ListBox();
            this.pnlFilePaths = new System.Windows.Forms.Panel();
            this.lnkSchematicFileBottom = new System.Windows.Forms.LinkLabel();
            this.label1lblSchematicPathLabelBottom = new System.Windows.Forms.Label();
            this.lblBOMPathLabel = new System.Windows.Forms.Label();
            this.lnkSchematicFileTop = new System.Windows.Forms.LinkLabel();
            this.lblSchematicPathLabelTop = new System.Windows.Forms.Label();
            this.lnkBOMFile = new System.Windows.Forms.LinkLabel();
            this.tbDatabaseView = new System.Windows.Forms.TabPage();
            this.dgvDatabaseTable = new System.Windows.Forms.DataGridView();
            this.pCBAAliasesBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.pCBAAliasesDataSet = new RApID_Project_WPF.PCBAAliasesDataSet();
            this.lblWarning = new System.Windows.Forms.Label();
            this.spltpnlMain = new System.Windows.Forms.SplitContainer();
            this.pCBAAliasesTableAdapter = new RApID_Project_WPF.PCBAAliasesDataSetTableAdapters.PCBAAliasesTableAdapter();
            this.iDDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.targetPartNumberDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.aliasDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bOMPathDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewLinkColumn();
            this.SchematicPathTop = new System.Windows.Forms.DataGridViewLinkColumn();
            this.SchematicPathBottom = new System.Windows.Forms.DataGridViewLinkColumn();
            this.cxmnuAliasesMenu.SuspendLayout();
            this.cxmnuLinkMenu.SuspendLayout();
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
            this.grpbxPartNumberDetail.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spltpnlAliasToDetail)).BeginInit();
            this.spltpnlAliasToDetail.Panel1.SuspendLayout();
            this.spltpnlAliasToDetail.Panel2.SuspendLayout();
            this.spltpnlAliasToDetail.SuspendLayout();
            this.pnlFilePaths.SuspendLayout();
            this.tbDatabaseView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDatabaseTable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pCBAAliasesBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pCBAAliasesDataSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spltpnlMain)).BeginInit();
            this.spltpnlMain.Panel1.SuspendLayout();
            this.spltpnlMain.Panel2.SuspendLayout();
            this.spltpnlMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // cxmnuAliasesMenu
            // 
            this.cxmnuAliasesMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addNewAliasToolStripMenuItem,
            this.deleteAliasToolStripMenuItem});
            this.cxmnuAliasesMenu.Name = "cxmnuAliasesMenu";
            this.cxmnuAliasesMenu.Size = new System.Drawing.Size(183, 48);
            // 
            // addNewAliasToolStripMenuItem
            // 
            this.addNewAliasToolStripMenuItem.Name = "addNewAliasToolStripMenuItem";
            this.addNewAliasToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.addNewAliasToolStripMenuItem.Text = "Add New Alias";
            this.addNewAliasToolStripMenuItem.Click += new System.EventHandler(this.addNewAliasToolStripMenuItem_Click);
            // 
            // deleteAliasToolStripMenuItem
            // 
            this.deleteAliasToolStripMenuItem.Name = "deleteAliasToolStripMenuItem";
            this.deleteAliasToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.deleteAliasToolStripMenuItem.Text = "Delete Selected Alias";
            this.deleteAliasToolStripMenuItem.Click += new System.EventHandler(this.deleteAliasToolStripMenuItem_Click);
            // 
            // cxmnuLinkMenu
            // 
            this.cxmnuLinkMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.changeFilePathToolStripMenuItem});
            this.cxmnuLinkMenu.Name = "cxmnuLinkMenu";
            this.cxmnuLinkMenu.Size = new System.Drawing.Size(173, 26);
            // 
            // changeFilePathToolStripMenuItem
            // 
            this.changeFilePathToolStripMenuItem.Name = "changeFilePathToolStripMenuItem";
            this.changeFilePathToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.changeFilePathToolStripMenuItem.Text = "Change File Path...";
            this.changeFilePathToolStripMenuItem.Click += new System.EventHandler(this.changeFilePathToolStripMenuItem_Click);
            // 
            // tcDataViewer
            // 
            this.tcDataViewer.Controls.Add(this.tbTechnicianView);
            this.tcDataViewer.Controls.Add(this.tbDatabaseView);
            this.tcDataViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcDataViewer.Location = new System.Drawing.Point(0, 0);
            this.tcDataViewer.Name = "tcDataViewer";
            this.tcDataViewer.SelectedIndex = 0;
            this.tcDataViewer.Size = new System.Drawing.Size(800, 387);
            this.tcDataViewer.TabIndex = 1;
            this.tcDataViewer.SelectedIndexChanged += new System.EventHandler(this.tcDataViewer_SelectedIndexChanged);
            // 
            // tbTechnicianView
            // 
            this.tbTechnicianView.BackColor = System.Drawing.Color.DarkGoldenrod;
            this.tbTechnicianView.Controls.Add(this.spltpnlTechView);
            this.tbTechnicianView.Location = new System.Drawing.Point(4, 22);
            this.tbTechnicianView.Name = "tbTechnicianView";
            this.tbTechnicianView.Padding = new System.Windows.Forms.Padding(3);
            this.tbTechnicianView.Size = new System.Drawing.Size(792, 361);
            this.tbTechnicianView.TabIndex = 0;
            this.tbTechnicianView.Text = "Technician View";
            this.tbTechnicianView.ToolTipText = "Shows main format for managing part number aliases.";
            // 
            // spltpnlTechView
            // 
            this.spltpnlTechView.Dock = System.Windows.Forms.DockStyle.Fill;
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
            this.spltpnlTechView.Size = new System.Drawing.Size(786, 355);
            this.spltpnlTechView.SplitterDistance = 149;
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
            this.spltpnlPartNumToDetail.Panel2.Controls.Add(this.grpbxPartNumberDetail);
            this.spltpnlPartNumToDetail.Size = new System.Drawing.Size(786, 149);
            this.spltpnlPartNumToDetail.SplitterDistance = 262;
            this.spltpnlPartNumToDetail.TabIndex = 0;
            // 
            // flopnlPartNumberInput
            // 
            this.flopnlPartNumberInput.BackColor = System.Drawing.Color.Black;
            this.flopnlPartNumberInput.Controls.Add(this.lblPartNumberLabel);
            this.flopnlPartNumberInput.Controls.Add(this.txtPartNumber);
            this.flopnlPartNumberInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flopnlPartNumberInput.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flopnlPartNumberInput.ForeColor = System.Drawing.Color.Goldenrod;
            this.flopnlPartNumberInput.Location = new System.Drawing.Point(0, 0);
            this.flopnlPartNumberInput.Name = "flopnlPartNumberInput";
            this.flopnlPartNumberInput.Size = new System.Drawing.Size(262, 149);
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
            // grpbxPartNumberDetail
            // 
            this.grpbxPartNumberDetail.BackColor = System.Drawing.Color.Black;
            this.grpbxPartNumberDetail.Controls.Add(this.lblCommodityClass);
            this.grpbxPartNumberDetail.Controls.Add(this.lblPartName);
            this.grpbxPartNumberDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpbxPartNumberDetail.ForeColor = System.Drawing.Color.Goldenrod;
            this.grpbxPartNumberDetail.Location = new System.Drawing.Point(0, 0);
            this.grpbxPartNumberDetail.Name = "grpbxPartNumberDetail";
            this.grpbxPartNumberDetail.Size = new System.Drawing.Size(520, 149);
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
            // spltpnlAliasToDetail
            // 
            this.spltpnlAliasToDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spltpnlAliasToDetail.Location = new System.Drawing.Point(0, 0);
            this.spltpnlAliasToDetail.Name = "spltpnlAliasToDetail";
            // 
            // spltpnlAliasToDetail.Panel1
            // 
            this.spltpnlAliasToDetail.Panel1.Controls.Add(this.lstbxAliases);
            // 
            // spltpnlAliasToDetail.Panel2
            // 
            this.spltpnlAliasToDetail.Panel2.Controls.Add(this.pnlFilePaths);
            this.spltpnlAliasToDetail.Size = new System.Drawing.Size(786, 202);
            this.spltpnlAliasToDetail.SplitterDistance = 262;
            this.spltpnlAliasToDetail.TabIndex = 0;
            // 
            // lstbxAliases
            // 
            this.lstbxAliases.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstbxAliases.BackColor = System.Drawing.Color.Black;
            this.lstbxAliases.ContextMenuStrip = this.cxmnuAliasesMenu;
            this.lstbxAliases.Enabled = false;
            this.lstbxAliases.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstbxAliases.ForeColor = System.Drawing.Color.Goldenrod;
            this.lstbxAliases.FormattingEnabled = true;
            this.lstbxAliases.ItemHeight = 20;
            this.lstbxAliases.Location = new System.Drawing.Point(0, 0);
            this.lstbxAliases.Name = "lstbxAliases";
            this.lstbxAliases.Size = new System.Drawing.Size(262, 202);
            this.lstbxAliases.TabIndex = 0;
            this.lstbxAliases.SelectedIndexChanged += new System.EventHandler(this.lstbxAliases_SelectedIndexChanged);
            this.lstbxAliases.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lstbxAliases_MouseDown);
            // 
            // pnlFilePaths
            // 
            this.pnlFilePaths.BackColor = System.Drawing.Color.Black;
            this.pnlFilePaths.Controls.Add(this.lnkSchematicFileBottom);
            this.pnlFilePaths.Controls.Add(this.label1lblSchematicPathLabelBottom);
            this.pnlFilePaths.Controls.Add(this.lblBOMPathLabel);
            this.pnlFilePaths.Controls.Add(this.lnkSchematicFileTop);
            this.pnlFilePaths.Controls.Add(this.lblSchematicPathLabelTop);
            this.pnlFilePaths.Controls.Add(this.lnkBOMFile);
            this.pnlFilePaths.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFilePaths.ForeColor = System.Drawing.Color.Gold;
            this.pnlFilePaths.Location = new System.Drawing.Point(0, 0);
            this.pnlFilePaths.Name = "pnlFilePaths";
            this.pnlFilePaths.Size = new System.Drawing.Size(520, 202);
            this.pnlFilePaths.TabIndex = 5;
            // 
            // lnkSchematicFileBottom
            // 
            this.lnkSchematicFileBottom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lnkSchematicFileBottom.AutoSize = true;
            this.lnkSchematicFileBottom.ContextMenuStrip = this.cxmnuLinkMenu;
            this.lnkSchematicFileBottom.DisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(133)))), ((int)(((byte)(133)))), ((int)(((byte)(133)))));
            this.lnkSchematicFileBottom.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lnkSchematicFileBottom.LinkColor = System.Drawing.Color.Aqua;
            this.lnkSchematicFileBottom.Location = new System.Drawing.Point(5, 159);
            this.lnkSchematicFileBottom.Name = "lnkSchematicFileBottom";
            this.lnkSchematicFileBottom.Size = new System.Drawing.Size(82, 20);
            this.lnkSchematicFileBottom.TabIndex = 6;
            this.lnkSchematicFileBottom.TabStop = true;
            this.lnkSchematicFileBottom.Text = "ASSYLink";
            this.lnkSchematicFileBottom.VisitedLinkColor = System.Drawing.Color.Fuchsia;
            this.lnkSchematicFileBottom.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkFile_LinkClicked);
            this.lnkSchematicFileBottom.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lnkSchematicFileBottom_MouseDown);
            // 
            // label1lblSchematicPathLabelBottom
            // 
            this.label1lblSchematicPathLabelBottom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1lblSchematicPathLabelBottom.AutoSize = true;
            this.label1lblSchematicPathLabelBottom.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1lblSchematicPathLabelBottom.ForeColor = System.Drawing.Color.Goldenrod;
            this.label1lblSchematicPathLabelBottom.Location = new System.Drawing.Point(5, 135);
            this.label1lblSchematicPathLabelBottom.Name = "label1lblSchematicPathLabelBottom";
            this.label1lblSchematicPathLabelBottom.Size = new System.Drawing.Size(275, 24);
            this.label1lblSchematicPathLabelBottom.TabIndex = 5;
            this.label1lblSchematicPathLabelBottom.Text = "Schematic File Path (BOTTOM)";
            // 
            // lblBOMPathLabel
            // 
            this.lblBOMPathLabel.AutoSize = true;
            this.lblBOMPathLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBOMPathLabel.ForeColor = System.Drawing.Color.Goldenrod;
            this.lblBOMPathLabel.Location = new System.Drawing.Point(3, 0);
            this.lblBOMPathLabel.Name = "lblBOMPathLabel";
            this.lblBOMPathLabel.Size = new System.Drawing.Size(131, 24);
            this.lblBOMPathLabel.TabIndex = 1;
            this.lblBOMPathLabel.Text = "BOM File Path";
            // 
            // lnkSchematicFileTop
            // 
            this.lnkSchematicFileTop.AutoSize = true;
            this.lnkSchematicFileTop.ContextMenuStrip = this.cxmnuLinkMenu;
            this.lnkSchematicFileTop.DisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(133)))), ((int)(((byte)(133)))), ((int)(((byte)(133)))));
            this.lnkSchematicFileTop.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lnkSchematicFileTop.LinkColor = System.Drawing.Color.Aqua;
            this.lnkSchematicFileTop.Location = new System.Drawing.Point(3, 89);
            this.lnkSchematicFileTop.Name = "lnkSchematicFileTop";
            this.lnkSchematicFileTop.Size = new System.Drawing.Size(82, 20);
            this.lnkSchematicFileTop.TabIndex = 4;
            this.lnkSchematicFileTop.TabStop = true;
            this.lnkSchematicFileTop.Text = "ASSYLink";
            this.lnkSchematicFileTop.VisitedLinkColor = System.Drawing.Color.Fuchsia;
            this.lnkSchematicFileTop.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkFile_LinkClicked);
            this.lnkSchematicFileTop.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lnkSchematicFileTop_MouseDown);
            // 
            // lblSchematicPathLabelTop
            // 
            this.lblSchematicPathLabelTop.AutoSize = true;
            this.lblSchematicPathLabelTop.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSchematicPathLabelTop.ForeColor = System.Drawing.Color.Goldenrod;
            this.lblSchematicPathLabelTop.Location = new System.Drawing.Point(3, 65);
            this.lblSchematicPathLabelTop.Name = "lblSchematicPathLabelTop";
            this.lblSchematicPathLabelTop.Size = new System.Drawing.Size(232, 24);
            this.lblSchematicPathLabelTop.TabIndex = 2;
            this.lblSchematicPathLabelTop.Text = "Schematic File Path (TOP)";
            // 
            // lnkBOMFile
            // 
            this.lnkBOMFile.AutoSize = true;
            this.lnkBOMFile.ContextMenuStrip = this.cxmnuLinkMenu;
            this.lnkBOMFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lnkBOMFile.LinkColor = System.Drawing.Color.Aqua;
            this.lnkBOMFile.Location = new System.Drawing.Point(3, 24);
            this.lnkBOMFile.Name = "lnkBOMFile";
            this.lnkBOMFile.Size = new System.Drawing.Size(74, 20);
            this.lnkBOMFile.TabIndex = 3;
            this.lnkBOMFile.TabStop = true;
            this.lnkBOMFile.Text = "BOMLink";
            this.lnkBOMFile.VisitedLinkColor = System.Drawing.Color.Fuchsia;
            this.lnkBOMFile.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkFile_LinkClicked);
            this.lnkBOMFile.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lnkBOMFile_Click);
            // 
            // tbDatabaseView
            // 
            this.tbDatabaseView.Controls.Add(this.dgvDatabaseTable);
            this.tbDatabaseView.Location = new System.Drawing.Point(4, 22);
            this.tbDatabaseView.Name = "tbDatabaseView";
            this.tbDatabaseView.Padding = new System.Windows.Forms.Padding(3);
            this.tbDatabaseView.Size = new System.Drawing.Size(792, 361);
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
            this.dgvDatabaseTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDatabaseTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.iDDataGridViewTextBoxColumn,
            this.targetPartNumberDataGridViewTextBoxColumn,
            this.aliasDataGridViewTextBoxColumn,
            this.bOMPathDataGridViewTextBoxColumn,
            this.SchematicPathTop,
            this.SchematicPathBottom});
            this.dgvDatabaseTable.DataSource = this.pCBAAliasesBindingSource;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.DimGray;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.HotTrack;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlLightLight;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvDatabaseTable.DefaultCellStyle = dataGridViewCellStyle1;
            this.dgvDatabaseTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDatabaseTable.Location = new System.Drawing.Point(3, 3);
            this.dgvDatabaseTable.Name = "dgvDatabaseTable";
            this.dgvDatabaseTable.ReadOnly = true;
            this.dgvDatabaseTable.Size = new System.Drawing.Size(786, 355);
            this.dgvDatabaseTable.TabIndex = 0;
            // 
            // pCBAAliasesBindingSource
            // 
            this.pCBAAliasesBindingSource.DataMember = "PCBAAliases";
            this.pCBAAliasesBindingSource.DataSource = this.pCBAAliasesDataSet;
            // 
            // pCBAAliasesDataSet
            // 
            this.pCBAAliasesDataSet.DataSetName = "PCBAAliasesDataSet";
            this.pCBAAliasesDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // lblWarning
            // 
            this.lblWarning.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWarning.AutoSize = true;
            this.lblWarning.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWarning.Location = new System.Drawing.Point(12, 9);
            this.lblWarning.Name = "lblWarning";
            this.lblWarning.Size = new System.Drawing.Size(552, 31);
            this.lblWarning.TabIndex = 0;
            this.lblWarning.Text = "Database Changes Are Immediately LIVE";
            // 
            // spltpnlMain
            // 
            this.spltpnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
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
            this.spltpnlMain.Panel2.Controls.Add(this.tcDataViewer);
            this.spltpnlMain.Size = new System.Drawing.Size(800, 450);
            this.spltpnlMain.SplitterDistance = 59;
            this.spltpnlMain.TabIndex = 0;
            // 
            // pCBAAliasesTableAdapter
            // 
            this.pCBAAliasesTableAdapter.ClearBeforeFill = true;
            // 
            // iDDataGridViewTextBoxColumn
            // 
            this.iDDataGridViewTextBoxColumn.DataPropertyName = "ID";
            this.iDDataGridViewTextBoxColumn.HeaderText = "ID";
            this.iDDataGridViewTextBoxColumn.Name = "iDDataGridViewTextBoxColumn";
            this.iDDataGridViewTextBoxColumn.ReadOnly = true;
            this.iDDataGridViewTextBoxColumn.Visible = false;
            // 
            // targetPartNumberDataGridViewTextBoxColumn
            // 
            this.targetPartNumberDataGridViewTextBoxColumn.DataPropertyName = "TargetPartNumber";
            this.targetPartNumberDataGridViewTextBoxColumn.HeaderText = "TargetPartNumber";
            this.targetPartNumberDataGridViewTextBoxColumn.Name = "targetPartNumberDataGridViewTextBoxColumn";
            this.targetPartNumberDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // aliasDataGridViewTextBoxColumn
            // 
            this.aliasDataGridViewTextBoxColumn.DataPropertyName = "Alias";
            this.aliasDataGridViewTextBoxColumn.HeaderText = "Alias";
            this.aliasDataGridViewTextBoxColumn.Name = "aliasDataGridViewTextBoxColumn";
            this.aliasDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // bOMPathDataGridViewTextBoxColumn
            // 
            this.bOMPathDataGridViewTextBoxColumn.DataPropertyName = "BOMPath";
            this.bOMPathDataGridViewTextBoxColumn.HeaderText = "BOMPath";
            this.bOMPathDataGridViewTextBoxColumn.Name = "bOMPathDataGridViewTextBoxColumn";
            this.bOMPathDataGridViewTextBoxColumn.ReadOnly = true;
            this.bOMPathDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.bOMPathDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.bOMPathDataGridViewTextBoxColumn.VisitedLinkColor = System.Drawing.Color.Fuchsia;
            // 
            // SchematicPathTop
            // 
            this.SchematicPathTop.DataPropertyName = "SchematicPathTop";
            this.SchematicPathTop.HeaderText = "SchematicPathTop";
            this.SchematicPathTop.Name = "SchematicPathTop";
            this.SchematicPathTop.ReadOnly = true;
            // 
            // SchematicPathBottom
            // 
            this.SchematicPathBottom.DataPropertyName = "SchematicPathBottom";
            this.SchematicPathBottom.HeaderText = "SchematicPathBottom";
            this.SchematicPathBottom.Name = "SchematicPathBottom";
            this.SchematicPathBottom.ReadOnly = true;
            // 
            // frmBoardAliases
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.spltpnlMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmBoardAliases";
            this.Text = "PCBA Alias Manager";
            this.Load += new System.EventHandler(this.frmBoardAliases_Load);
            this.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.frmBoardAliases_PreviewKeyDown);
            this.cxmnuAliasesMenu.ResumeLayout(false);
            this.cxmnuLinkMenu.ResumeLayout(false);
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
            this.grpbxPartNumberDetail.ResumeLayout(false);
            this.grpbxPartNumberDetail.PerformLayout();
            this.spltpnlAliasToDetail.Panel1.ResumeLayout(false);
            this.spltpnlAliasToDetail.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spltpnlAliasToDetail)).EndInit();
            this.spltpnlAliasToDetail.ResumeLayout(false);
            this.pnlFilePaths.ResumeLayout(false);
            this.pnlFilePaths.PerformLayout();
            this.tbDatabaseView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDatabaseTable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pCBAAliasesBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pCBAAliasesDataSet)).EndInit();
            this.spltpnlMain.Panel1.ResumeLayout(false);
            this.spltpnlMain.Panel1.PerformLayout();
            this.spltpnlMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spltpnlMain)).EndInit();
            this.spltpnlMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ContextMenuStrip cxmnuLinkMenu;
        private System.Windows.Forms.ContextMenuStrip cxmnuAliasesMenu;
        private System.Windows.Forms.ToolStripMenuItem changeFilePathToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addNewAliasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteAliasToolStripMenuItem;
        private System.Windows.Forms.TabControl tcDataViewer;
        private System.Windows.Forms.TabPage tbTechnicianView;
        private System.Windows.Forms.SplitContainer spltpnlTechView;
        private System.Windows.Forms.SplitContainer spltpnlPartNumToDetail;
        private System.Windows.Forms.FlowLayoutPanel flopnlPartNumberInput;
        private System.Windows.Forms.Label lblPartNumberLabel;
        private System.Windows.Forms.TextBox txtPartNumber;
        private System.Windows.Forms.GroupBox grpbxPartNumberDetail;
        private System.Windows.Forms.Label lblCommodityClass;
        private System.Windows.Forms.Label lblPartName;
        private System.Windows.Forms.SplitContainer spltpnlAliasToDetail;
        private System.Windows.Forms.ListBox lstbxAliases;
        private System.Windows.Forms.Panel pnlFilePaths;
        private System.Windows.Forms.Label lblBOMPathLabel;
        private System.Windows.Forms.LinkLabel lnkSchematicFileTop;
        private System.Windows.Forms.Label lblSchematicPathLabelTop;
        private System.Windows.Forms.LinkLabel lnkBOMFile;
        private System.Windows.Forms.TabPage tbDatabaseView;
        private System.Windows.Forms.DataGridView dgvDatabaseTable;
        private System.Windows.Forms.Label lblWarning;
        private System.Windows.Forms.SplitContainer spltpnlMain;
        private PCBAAliasesDataSet pCBAAliasesDataSet;
        private System.Windows.Forms.BindingSource pCBAAliasesBindingSource;
        private PCBAAliasesDataSetTableAdapters.PCBAAliasesTableAdapter pCBAAliasesTableAdapter;
        private System.Windows.Forms.DataGridViewLinkColumn schematicPathDataGridViewTextBoxColumn;
        private System.Windows.Forms.LinkLabel lnkSchematicFileBottom;
        private System.Windows.Forms.Label label1lblSchematicPathLabelBottom;
        private System.Windows.Forms.DataGridViewTextBoxColumn iDDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn targetPartNumberDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn aliasDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewLinkColumn bOMPathDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewLinkColumn SchematicPathTop;
        private System.Windows.Forms.DataGridViewLinkColumn SchematicPathBottom;
    }
}