﻿namespace Chomp {
    // Token: 0x02000002 RID: 2
    public partial class Main : global::System.Windows.Forms.Form {
        // Token: 0x06000060 RID: 96 RVA: 0x0000B044 File Offset: 0x00009244
        protected override void Dispose(bool disposing) {
            if (disposing && this.components != null) {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        // Token: 0x06000061 RID: 97 RVA: 0x0000B064 File Offset: 0x00009264
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.dataFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.importedRegulation = new System.Windows.Forms.OpenFileDialog();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripComboBoxGame = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exploreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ProjectFolderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.duplicateRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findNextRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gotoRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findFieldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findNextFieldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toggleFieldNameTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toggleFieldTypeVisibilityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toggleFilterVisibilityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.WorkflowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fieldAdjusterMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.affinityGeneratorMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logParamSizesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importRowNames_Stock_MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.importRowNames_Project_MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportRowNames_Project_MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.importDataMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportDataMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.massImportDataMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.massExportDataMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.fieldExporterMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.valueReferenceFinderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rowReferenceFinderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.secondaryDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectSecondaryFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearSecondaryFileToolMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showParamDifferencesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewDataSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewInterfaceSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewFilterSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.dgvParams = new System.Windows.Forms.DataGridView();
            this.dgvParamsParamCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.menuStrip2 = new System.Windows.Forms.MenuStrip();
            this.filter_Params = new System.Windows.Forms.ToolStripTextBox();
            this.button_FilterParams = new System.Windows.Forms.ToolStripMenuItem();
            this.button_ResetFilterParams = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.dgvRows = new System.Windows.Forms.DataGridView();
            this.dgvRowsIDCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvRowsNameCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.menuStrip3 = new System.Windows.Forms.MenuStrip();
            this.filter_Rows = new System.Windows.Forms.ToolStripTextBox();
            this.button_FilterRows = new System.Windows.Forms.ToolStripMenuItem();
            this.button_ResetFilterRows = new System.Windows.Forms.ToolStripMenuItem();
            this.dgvCells = new System.Windows.Forms.DataGridView();
            this.dgvCellsNameCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvCellsEditorNameCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvCellsValueCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvCellsTypeCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.menuStrip4 = new System.Windows.Forms.MenuStrip();
            this.filter_Cells = new System.Windows.Forms.ToolStripTextBox();
            this.button_FilterCells = new System.Windows.Forms.ToolStripMenuItem();
            this.button_ResetFilterCells = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.processLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.fbdExport = new System.Windows.Forms.FolderBrowserDialog();
            this.fieldContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.GotoReference1MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.GotoReference2MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.GotoReference3MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.GotoReference4MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.GotoReference5MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.GotoReference6MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rowContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToParamMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.secondaryFilePath = new System.Windows.Forms.OpenFileDialog();
            this.BottomToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.TopToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.RightToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.LeftToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.ContentPanel = new System.Windows.Forms.ToolStripContentPanel();
            this.toolTip_filterParams = new System.Windows.Forms.ToolTip(this.components);
            this.toolTip_filterRows = new System.Windows.Forms.ToolTip(this.components);
            this.toolTip_filterCells = new System.Windows.Forms.ToolTip(this.components);
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvParams)).BeginInit();
            this.menuStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRows)).BeginInit();
            this.menuStrip3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCells)).BeginInit();
            this.menuStrip4.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.fieldContextMenu.SuspendLayout();
            this.rowContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataFileDialog
            // 
            this.dataFileDialog.FileName = "gameparam.parambnd.dcx";
            this.dataFileDialog.Filter = "Regulation or parambnd|*";
            // 
            // importedRegulation
            // 
            this.importedRegulation.FileName = "regulation.bin";
            this.importedRegulation.Filter = "Regulation or parambnd|*";
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.LightGray;
            this.menuStrip1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.WorkflowToolStripMenuItem,
            this.ToolStripMenuItem,
            this.secondaryDataToolStripMenuItem,
            this.settingsMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(2, 2);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.menuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.menuStrip1.Size = new System.Drawing.Size(1136, 24);
            this.menuStrip1.TabIndex = 8;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.restoreToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.toolStripSeparator5,
            this.toolStripComboBoxGame,
            this.toolStripSeparator2,
            this.exploreToolStripMenuItem,
            this.ProjectFolderMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.ToolTipText = "Browse for a regulation file to edit.";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.saveToolStripMenuItem.Text = "&Save";
            this.saveToolStripMenuItem.ToolTipText = "Save changes to the regulation file.";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.SaveToolStripMenuItem_Click);
            // 
            // restoreToolStripMenuItem
            // 
            this.restoreToolStripMenuItem.Name = "restoreToolStripMenuItem";
            this.restoreToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.restoreToolStripMenuItem.Text = "Restore";
            this.restoreToolStripMenuItem.ToolTipText = "Restore the regulation file backup.";
            this.restoreToolStripMenuItem.Click += new System.EventHandler(this.RestoreToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.exportToolStripMenuItem.Text = "Export";
            this.exportToolStripMenuItem.ToolTipText = "Export an encrypted Data0.bdt to decrypted parambnds.";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.ExportToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(181, 6);
            // 
            // toolStripComboBoxGame
            // 
            this.toolStripComboBoxGame.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBoxGame.Name = "toolStripComboBoxGame";
            this.toolStripComboBoxGame.Size = new System.Drawing.Size(121, 23);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(181, 6);
            // 
            // exploreToolStripMenuItem
            // 
            this.exploreToolStripMenuItem.Name = "exploreToolStripMenuItem";
            this.exploreToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.exploreToolStripMenuItem.Text = "Show Regulation File";
            this.exploreToolStripMenuItem.ToolTipText = "Open the regulation file directory in Explorer.";
            this.exploreToolStripMenuItem.Click += new System.EventHandler(this.ExploreToolStripMenuItem_Click);
            // 
            // ProjectFolderMenuItem
            // 
            this.ProjectFolderMenuItem.Name = "ProjectFolderMenuItem";
            this.ProjectFolderMenuItem.Size = new System.Drawing.Size(184, 22);
            this.ProjectFolderMenuItem.Text = "View Project Folder";
            this.ProjectFolderMenuItem.Click += new System.EventHandler(this.ProjectFolderMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addRowToolStripMenuItem,
            this.duplicateRowToolStripMenuItem,
            this.deleteRowToolStripMenuItem,
            this.findRowToolStripMenuItem,
            this.findNextRowToolStripMenuItem,
            this.gotoRowToolStripMenuItem,
            this.findFieldToolStripMenuItem,
            this.findNextFieldToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // addRowToolStripMenuItem
            // 
            this.addRowToolStripMenuItem.Name = "addRowToolStripMenuItem";
            this.addRowToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.addRowToolStripMenuItem.Text = "Add Row";
            this.addRowToolStripMenuItem.ToolTipText = "Add a new row to the active param.";
            this.addRowToolStripMenuItem.Click += new System.EventHandler(this.AddRowToolStripMenuItem_Click);
            // 
            // duplicateRowToolStripMenuItem
            // 
            this.duplicateRowToolStripMenuItem.Name = "duplicateRowToolStripMenuItem";
            this.duplicateRowToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.duplicateRowToolStripMenuItem.Text = "Duplicate Row";
            this.duplicateRowToolStripMenuItem.ToolTipText = "Create a new row with values identical to the selected one";
            this.duplicateRowToolStripMenuItem.Click += new System.EventHandler(this.DuplicateRowToolStripMenuItem_Click);
            // 
            // deleteRowToolStripMenuItem
            // 
            this.deleteRowToolStripMenuItem.Name = "deleteRowToolStripMenuItem";
            this.deleteRowToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Delete)));
            this.deleteRowToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.deleteRowToolStripMenuItem.Text = "Delete Row";
            this.deleteRowToolStripMenuItem.ToolTipText = "Delete the currently selected row";
            this.deleteRowToolStripMenuItem.Click += new System.EventHandler(this.DeleteRowToolStripMenuItem_Click);
            // 
            // findRowToolStripMenuItem
            // 
            this.findRowToolStripMenuItem.Name = "findRowToolStripMenuItem";
            this.findRowToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.findRowToolStripMenuItem.Text = "&Find Row";
            this.findRowToolStripMenuItem.ToolTipText = "Search for a row with a matching name";
            this.findRowToolStripMenuItem.Click += new System.EventHandler(this.FindRowToolStripMenuItem_Click);
            // 
            // findNextRowToolStripMenuItem
            // 
            this.findNextRowToolStripMenuItem.Name = "findNextRowToolStripMenuItem";
            this.findNextRowToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.findNextRowToolStripMenuItem.Text = "Find Next Row";
            this.findNextRowToolStripMenuItem.ToolTipText = "Search again with the previous pattern";
            this.findNextRowToolStripMenuItem.Click += new System.EventHandler(this.FindNextRowToolStripMenuItem_Click);
            // 
            // gotoRowToolStripMenuItem
            // 
            this.gotoRowToolStripMenuItem.Name = "gotoRowToolStripMenuItem";
            this.gotoRowToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.gotoRowToolStripMenuItem.Text = "&Goto Row";
            this.gotoRowToolStripMenuItem.ToolTipText = "Go to a row with a certain ID";
            this.gotoRowToolStripMenuItem.Click += new System.EventHandler(this.GotoRowToolStripMenuItem_Click);
            // 
            // findFieldToolStripMenuItem
            // 
            this.findFieldToolStripMenuItem.Name = "findFieldToolStripMenuItem";
            this.findFieldToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.findFieldToolStripMenuItem.Text = "Find Field";
            this.findFieldToolStripMenuItem.ToolTipText = "Search for a field with a matching name";
            this.findFieldToolStripMenuItem.Click += new System.EventHandler(this.FindFieldToolStripMenuItem_Click);
            // 
            // findNextFieldToolStripMenuItem
            // 
            this.findNextFieldToolStripMenuItem.Name = "findNextFieldToolStripMenuItem";
            this.findNextFieldToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.findNextFieldToolStripMenuItem.Text = "Find Next Field";
            this.findNextFieldToolStripMenuItem.ToolTipText = "Search again with the previous pattern";
            this.findNextFieldToolStripMenuItem.Click += new System.EventHandler(this.FindNextFieldToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toggleFieldNameTypeToolStripMenuItem,
            this.toggleFieldTypeVisibilityToolStripMenuItem,
            this.toggleFilterVisibilityToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // toggleFieldNameTypeToolStripMenuItem
            // 
            this.toggleFieldNameTypeToolStripMenuItem.Name = "toggleFieldNameTypeToolStripMenuItem";
            this.toggleFieldNameTypeToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this.toggleFieldNameTypeToolStripMenuItem.Text = "Toggle Field Name Scheme";
            this.toggleFieldNameTypeToolStripMenuItem.Click += new System.EventHandler(this.toggleFieldNameTypeToolStripMenuItem_Click);
            // 
            // toggleFieldTypeVisibilityToolStripMenuItem
            // 
            this.toggleFieldTypeVisibilityToolStripMenuItem.Name = "toggleFieldTypeVisibilityToolStripMenuItem";
            this.toggleFieldTypeVisibilityToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this.toggleFieldTypeVisibilityToolStripMenuItem.Text = "Toggle Field Type Visibility";
            this.toggleFieldTypeVisibilityToolStripMenuItem.Click += new System.EventHandler(this.toggleFieldTypeVisibilityToolStripMenuItem_Click);
            // 
            // toggleFilterVisibilityToolStripMenuItem
            // 
            this.toggleFilterVisibilityToolStripMenuItem.Name = "toggleFilterVisibilityToolStripMenuItem";
            this.toggleFilterVisibilityToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this.toggleFilterVisibilityToolStripMenuItem.Text = "Toggle Filter Visibility";
            this.toggleFilterVisibilityToolStripMenuItem.Click += new System.EventHandler(this.toggleFilterVisibilityToolStripMenuItem_Click);
            // 
            // WorkflowToolStripMenuItem
            // 
            this.WorkflowToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fieldAdjusterMenuItem,
            this.affinityGeneratorMenuItem,
            this.logParamSizesToolStripMenuItem});
            this.WorkflowToolStripMenuItem.Name = "WorkflowToolStripMenuItem";
            this.WorkflowToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.WorkflowToolStripMenuItem.Text = "Tools";
            // 
            // fieldAdjusterMenuItem
            // 
            this.fieldAdjusterMenuItem.AutoToolTip = true;
            this.fieldAdjusterMenuItem.Name = "fieldAdjusterMenuItem";
            this.fieldAdjusterMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.fieldAdjusterMenuItem.Size = new System.Drawing.Size(168, 22);
            this.fieldAdjusterMenuItem.Text = "Field Adjuster";
            this.fieldAdjusterMenuItem.ToolTipText = "Use this to quickly apply a field edit to multiple rows.\r\nContains various tools " +
    "to filter which rows are edited.";
            this.fieldAdjusterMenuItem.Click += new System.EventHandler(this.fieldAdjusterMenuItem_Click);
            // 
            // affinityGeneratorMenuItem
            // 
            this.affinityGeneratorMenuItem.AutoToolTip = true;
            this.affinityGeneratorMenuItem.Name = "affinityGeneratorMenuItem";
            this.affinityGeneratorMenuItem.Size = new System.Drawing.Size(168, 22);
            this.affinityGeneratorMenuItem.Text = "Affinity Generator";
            this.affinityGeneratorMenuItem.ToolTipText = "Use this to quickly generate rows for weapons\r\nthat lack affinities.\r\n\r\nYou can c" +
    "hange the configuration applied by\r\nediting the text files in:\r\n.\\<gametype>\\Con" +
    "figuration\\AffinityGenerator\\";
            this.affinityGeneratorMenuItem.Click += new System.EventHandler(this.affinityGeneratorMenuItem_Click);
            // 
            // logParamSizesToolStripMenuItem
            // 
            this.logParamSizesToolStripMenuItem.Name = "logParamSizesToolStripMenuItem";
            this.logParamSizesToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.logParamSizesToolStripMenuItem.Text = "Log Param Sizes";
            this.logParamSizesToolStripMenuItem.Click += new System.EventHandler(this.logParamSizesToolStripMenuItem_Click);
            // 
            // ToolStripMenuItem
            // 
            this.ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importRowNames_Stock_MenuItem,
            this.toolStripSeparator1,
            this.importRowNames_Project_MenuItem,
            this.exportRowNames_Project_MenuItem,
            this.toolStripSeparator8,
            this.importDataMenuItem,
            this.exportDataMenuItem,
            this.toolStripSeparator7,
            this.massImportDataMenuItem,
            this.massExportDataMenuItem,
            this.toolStripSeparator9,
            this.fieldExporterMenuItem,
            this.valueReferenceFinderMenuItem,
            this.rowReferenceFinderMenuItem});
            this.ToolStripMenuItem.Name = "ToolStripMenuItem";
            this.ToolStripMenuItem.Size = new System.Drawing.Size(71, 20);
            this.ToolStripMenuItem.Text = "Field Data";
            // 
            // importRowNames_Stock_MenuItem
            // 
            this.importRowNames_Stock_MenuItem.AutoToolTip = true;
            this.importRowNames_Stock_MenuItem.Name = "importRowNames_Stock_MenuItem";
            this.importRowNames_Stock_MenuItem.Size = new System.Drawing.Size(235, 22);
            this.importRowNames_Stock_MenuItem.Text = "Import Stock Row Names";
            this.importRowNames_Stock_MenuItem.ToolTipText = "Import row names from Paramdex names.\r\n";
            this.importRowNames_Stock_MenuItem.Click += new System.EventHandler(this.importRowNames_Stock_MenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(232, 6);
            // 
            // importRowNames_Project_MenuItem
            // 
            this.importRowNames_Project_MenuItem.AutoToolTip = true;
            this.importRowNames_Project_MenuItem.Name = "importRowNames_Project_MenuItem";
            this.importRowNames_Project_MenuItem.ShortcutKeys = System.Windows.Forms.Keys.F6;
            this.importRowNames_Project_MenuItem.Size = new System.Drawing.Size(235, 22);
            this.importRowNames_Project_MenuItem.Text = "Import Project Row Names";
            this.importRowNames_Project_MenuItem.ToolTipText = "Import row names from Project row names.\r\n\r\n";
            this.importRowNames_Project_MenuItem.Click += new System.EventHandler(this.importRowNames_Project_MenuItem_Click);
            // 
            // exportRowNames_Project_MenuItem
            // 
            this.exportRowNames_Project_MenuItem.AutoToolTip = true;
            this.exportRowNames_Project_MenuItem.Name = "exportRowNames_Project_MenuItem";
            this.exportRowNames_Project_MenuItem.ShortcutKeys = System.Windows.Forms.Keys.F8;
            this.exportRowNames_Project_MenuItem.Size = new System.Drawing.Size(235, 22);
            this.exportRowNames_Project_MenuItem.Text = "Export Project Row Names";
            this.exportRowNames_Project_MenuItem.ToolTipText = "Export row names to Project row names.\r\n\r\n";
            this.exportRowNames_Project_MenuItem.Click += new System.EventHandler(this.exportRowNames_Project_MenuItem_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(232, 6);
            // 
            // importDataMenuItem
            // 
            this.importDataMenuItem.AutoToolTip = true;
            this.importDataMenuItem.Name = "importDataMenuItem";
            this.importDataMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F11;
            this.importDataMenuItem.Size = new System.Drawing.Size(235, 22);
            this.importDataMenuItem.Text = "Import Data";
            this.importDataMenuItem.ToolTipText = "For the currently selected param, \r\nimport param data from a CSV file of the same" +
    " name.\r\n\r\nData files are found in .\\<gametype>\\Data\\";
            this.importDataMenuItem.Click += new System.EventHandler(this.importDataMenuItem_Click);
            // 
            // exportDataMenuItem
            // 
            this.exportDataMenuItem.AutoToolTip = true;
            this.exportDataMenuItem.Name = "exportDataMenuItem";
            this.exportDataMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F12;
            this.exportDataMenuItem.Size = new System.Drawing.Size(235, 22);
            this.exportDataMenuItem.Text = "Export Data";
            this.exportDataMenuItem.ToolTipText = "For the currently selected param, \r\nexport param data into a CSV file of the same" +
    " name.\r\n\r\nData files are found in .\\<gametype>\\Data\\\r\n";
            this.exportDataMenuItem.Click += new System.EventHandler(this.exportDataMenuItem_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(232, 6);
            // 
            // massImportDataMenuItem
            // 
            this.massImportDataMenuItem.AutoToolTip = true;
            this.massImportDataMenuItem.Name = "massImportDataMenuItem";
            this.massImportDataMenuItem.Size = new System.Drawing.Size(235, 22);
            this.massImportDataMenuItem.Text = "Mass Import Data";
            this.massImportDataMenuItem.ToolTipText = "For all params, import param data from \r\na CSV file of the same name.\r\n\r\nData fil" +
    "es are found in .\\<gametype>\\Data\\\r\n";
            this.massImportDataMenuItem.Click += new System.EventHandler(this.massImportDataMenuItem_Click);
            // 
            // massExportDataMenuItem
            // 
            this.massExportDataMenuItem.AutoToolTip = true;
            this.massExportDataMenuItem.Name = "massExportDataMenuItem";
            this.massExportDataMenuItem.Size = new System.Drawing.Size(235, 22);
            this.massExportDataMenuItem.Text = "Mass Export Data";
            this.massExportDataMenuItem.ToolTipText = "For all params, export param data into a CSV file \r\nof the same name.\r\n\r\nData fil" +
    "es are found in .\\<gametype>\\Data\\\r\n";
            this.massExportDataMenuItem.Click += new System.EventHandler(this.massExportDataMenuItem_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(232, 6);
            // 
            // fieldExporterMenuItem
            // 
            this.fieldExporterMenuItem.AutoToolTip = true;
            this.fieldExporterMenuItem.Name = "fieldExporterMenuItem";
            this.fieldExporterMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.fieldExporterMenuItem.Size = new System.Drawing.Size(235, 22);
            this.fieldExporterMenuItem.Text = "Field Exporter";
            this.fieldExporterMenuItem.ToolTipText = "Export the specific field values for a field.";
            this.fieldExporterMenuItem.Click += new System.EventHandler(this.fieldExporterMenuItem_Click);
            // 
            // valueReferenceFinderMenuItem
            // 
            this.valueReferenceFinderMenuItem.AutoToolTip = true;
            this.valueReferenceFinderMenuItem.Name = "valueReferenceFinderMenuItem";
            this.valueReferenceFinderMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.valueReferenceFinderMenuItem.Size = new System.Drawing.Size(235, 22);
            this.valueReferenceFinderMenuItem.Text = "Value Reference Finder";
            this.valueReferenceFinderMenuItem.ToolTipText = "Find all references to a field value.";
            this.valueReferenceFinderMenuItem.Click += new System.EventHandler(this.valueReferenceFinderMenuItem_Click);
            // 
            // rowReferenceFinderMenuItem
            // 
            this.rowReferenceFinderMenuItem.AutoToolTip = true;
            this.rowReferenceFinderMenuItem.Name = "rowReferenceFinderMenuItem";
            this.rowReferenceFinderMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F4;
            this.rowReferenceFinderMenuItem.Size = new System.Drawing.Size(235, 22);
            this.rowReferenceFinderMenuItem.Text = "Row Reference Finder";
            this.rowReferenceFinderMenuItem.ToolTipText = "Find all references to a row ID.";
            this.rowReferenceFinderMenuItem.Click += new System.EventHandler(this.rowReferenceFinderMenuItem_Click);
            // 
            // secondaryDataToolStripMenuItem
            // 
            this.secondaryDataToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectSecondaryFileToolStripMenuItem,
            this.clearSecondaryFileToolMenuItem,
            this.showParamDifferencesToolStripMenuItem});
            this.secondaryDataToolStripMenuItem.Name = "secondaryDataToolStripMenuItem";
            this.secondaryDataToolStripMenuItem.Size = new System.Drawing.Size(64, 20);
            this.secondaryDataToolStripMenuItem.Text = "File Data";
            // 
            // selectSecondaryFileToolStripMenuItem
            // 
            this.selectSecondaryFileToolStripMenuItem.Name = "selectSecondaryFileToolStripMenuItem";
            this.selectSecondaryFileToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
            this.selectSecondaryFileToolStripMenuItem.Text = "Select Secondary File";
            this.selectSecondaryFileToolStripMenuItem.Click += new System.EventHandler(this.selectSecondaryFileToolStripMenuItem_Click);
            // 
            // clearSecondaryFileToolMenuItem
            // 
            this.clearSecondaryFileToolMenuItem.Name = "clearSecondaryFileToolMenuItem";
            this.clearSecondaryFileToolMenuItem.Size = new System.Drawing.Size(237, 22);
            this.clearSecondaryFileToolMenuItem.Text = "Clear Secondary File";
            this.clearSecondaryFileToolMenuItem.Click += new System.EventHandler(this.clearSecondaryFileToolMenuItem_Click);
            // 
            // showParamDifferencesToolStripMenuItem
            // 
            this.showParamDifferencesToolStripMenuItem.Name = "showParamDifferencesToolStripMenuItem";
            this.showParamDifferencesToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
            this.showParamDifferencesToolStripMenuItem.Text = "Toggle Param Difference Mode";
            this.showParamDifferencesToolStripMenuItem.Click += new System.EventHandler(this.showParamDifferencesToolStripMenuItem_Click);
            // 
            // settingsMenuItem
            // 
            this.settingsMenuItem.AutoToolTip = true;
            this.settingsMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewSettingsToolStripMenuItem,
            this.viewDataSettingsToolStripMenuItem,
            this.viewInterfaceSettingsToolStripMenuItem,
            this.viewFilterSettingsToolStripMenuItem});
            this.settingsMenuItem.Name = "settingsMenuItem";
            this.settingsMenuItem.Size = new System.Drawing.Size(61, 20);
            this.settingsMenuItem.Text = "Settings";
            // 
            // viewSettingsToolStripMenuItem
            // 
            this.viewSettingsToolStripMenuItem.Name = "viewSettingsToolStripMenuItem";
            this.viewSettingsToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.viewSettingsToolStripMenuItem.Text = "View General Settings";
            this.viewSettingsToolStripMenuItem.ToolTipText = "Configure the settings used by Yapped for various features.";
            this.viewSettingsToolStripMenuItem.Click += new System.EventHandler(this.viewSettingsToolStripMenuItem_Click);
            // 
            // viewDataSettingsToolStripMenuItem
            // 
            this.viewDataSettingsToolStripMenuItem.Name = "viewDataSettingsToolStripMenuItem";
            this.viewDataSettingsToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.viewDataSettingsToolStripMenuItem.Text = "View Data Settings";
            this.viewDataSettingsToolStripMenuItem.Click += new System.EventHandler(this.viewDataSettingsToolStripMenuItem_Click);
            // 
            // viewInterfaceSettingsToolStripMenuItem
            // 
            this.viewInterfaceSettingsToolStripMenuItem.Name = "viewInterfaceSettingsToolStripMenuItem";
            this.viewInterfaceSettingsToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.viewInterfaceSettingsToolStripMenuItem.Text = "View Interface Settings";
            this.viewInterfaceSettingsToolStripMenuItem.Click += new System.EventHandler(this.viewInterfaceSettingsToolStripMenuItem_Click);
            // 
            // viewFilterSettingsToolStripMenuItem
            // 
            this.viewFilterSettingsToolStripMenuItem.Name = "viewFilterSettingsToolStripMenuItem";
            this.viewFilterSettingsToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.viewFilterSettingsToolStripMenuItem.Text = "View Filter Settings";
            this.viewFilterSettingsToolStripMenuItem.Click += new System.EventHandler(this.viewFilterSettingsToolStripMenuItem_Click);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(2, 26);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.dgvParams);
            this.splitContainer2.Panel1.Controls.Add(this.menuStrip2);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer1);
            this.splitContainer2.Size = new System.Drawing.Size(1136, 672);
            this.splitContainer2.SplitterDistance = 249;
            this.splitContainer2.SplitterWidth = 5;
            this.splitContainer2.TabIndex = 2;
            // 
            // dgvParams
            // 
            this.dgvParams.AllowUserToAddRows = false;
            this.dgvParams.AllowUserToDeleteRows = false;
            this.dgvParams.AllowUserToResizeColumns = false;
            this.dgvParams.AllowUserToResizeRows = false;
            this.dgvParams.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvParams.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvParams.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgvParamsParamCol});
            this.dgvParams.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvParams.Location = new System.Drawing.Point(0, 31);
            this.dgvParams.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.dgvParams.MinimumSize = new System.Drawing.Size(187, 0);
            this.dgvParams.MultiSelect = false;
            this.dgvParams.Name = "dgvParams";
            this.dgvParams.RowHeadersVisible = false;
            this.dgvParams.RowHeadersWidth = 51;
            this.dgvParams.Size = new System.Drawing.Size(249, 641);
            this.dgvParams.TabIndex = 0;
            this.dgvParams.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvParams_CellContentClick);
            this.dgvParams.CellToolTipTextNeeded += new System.Windows.Forms.DataGridViewCellToolTipTextNeededEventHandler(this.DgvParams_CellToolTipTextNeeded);
            this.dgvParams.SelectionChanged += new System.EventHandler(this.DgvParams_SelectionChanged);
            // 
            // dgvParamsParamCol
            // 
            this.dgvParamsParamCol.DataPropertyName = "Name";
            this.dgvParamsParamCol.HeaderText = "Param";
            this.dgvParamsParamCol.MinimumWidth = 6;
            this.dgvParamsParamCol.Name = "dgvParamsParamCol";
            this.dgvParamsParamCol.ReadOnly = true;
            // 
            // menuStrip2
            // 
            this.menuStrip2.BackColor = System.Drawing.Color.Transparent;
            this.menuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.filter_Params,
            this.button_FilterParams,
            this.button_ResetFilterParams});
            this.menuStrip2.Location = new System.Drawing.Point(0, 0);
            this.menuStrip2.Name = "menuStrip2";
            this.menuStrip2.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.menuStrip2.Size = new System.Drawing.Size(249, 31);
            this.menuStrip2.TabIndex = 1;
            this.menuStrip2.Text = "menuStrip2";
            // 
            // filter_Params
            // 
            this.filter_Params.AutoToolTip = true;
            this.filter_Params.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.filter_Params.Margin = new System.Windows.Forms.Padding(2);
            this.filter_Params.Name = "filter_Params";
            this.filter_Params.Size = new System.Drawing.Size(140, 23);
            // 
            // button_FilterParams
            // 
            this.button_FilterParams.AutoToolTip = true;
            this.button_FilterParams.BackColor = System.Drawing.Color.DarkGray;
            this.button_FilterParams.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.button_FilterParams.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.button_FilterParams.Margin = new System.Windows.Forms.Padding(2);
            this.button_FilterParams.Name = "button_FilterParams";
            this.button_FilterParams.Size = new System.Drawing.Size(50, 23);
            this.button_FilterParams.Text = "Apply";
            this.button_FilterParams.ToolTipText = "Apply param view filter.";
            this.button_FilterParams.Click += new System.EventHandler(this.button_FilterParams_Click);
            // 
            // button_ResetFilterParams
            // 
            this.button_ResetFilterParams.AutoToolTip = true;
            this.button_ResetFilterParams.BackColor = System.Drawing.Color.DarkGray;
            this.button_ResetFilterParams.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.button_ResetFilterParams.Margin = new System.Windows.Forms.Padding(2);
            this.button_ResetFilterParams.Name = "button_ResetFilterParams";
            this.button_ResetFilterParams.Size = new System.Drawing.Size(47, 23);
            this.button_ResetFilterParams.Text = "Reset";
            this.button_ResetFilterParams.ToolTipText = "Reset param view.";
            this.button_ResetFilterParams.Click += new System.EventHandler(this.button_ResetFilterParams_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.dgvRows);
            this.splitContainer1.Panel1.Controls.Add(this.menuStrip3);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.dgvCells);
            this.splitContainer1.Panel2.Controls.Add(this.menuStrip4);
            this.splitContainer1.Size = new System.Drawing.Size(882, 672);
            this.splitContainer1.SplitterDistance = 386;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 7;
            // 
            // dgvRows
            // 
            this.dgvRows.AllowUserToAddRows = false;
            this.dgvRows.AllowUserToDeleteRows = false;
            this.dgvRows.AllowUserToResizeColumns = false;
            this.dgvRows.AllowUserToResizeRows = false;
            this.dgvRows.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRows.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgvRowsIDCol,
            this.dgvRowsNameCol});
            this.dgvRows.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvRows.Location = new System.Drawing.Point(0, 31);
            this.dgvRows.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.dgvRows.MinimumSize = new System.Drawing.Size(187, 0);
            this.dgvRows.MultiSelect = false;
            this.dgvRows.Name = "dgvRows";
            this.dgvRows.RowHeadersVisible = false;
            this.dgvRows.RowHeadersWidth = 51;
            this.dgvRows.Size = new System.Drawing.Size(386, 641);
            this.dgvRows.TabIndex = 1;
            this.dgvRows.CellContextMenuStripNeeded += new System.Windows.Forms.DataGridViewCellContextMenuStripNeededEventHandler(this.dgvRows_CellContextMenuStripNeeded);
            this.dgvRows.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvRows_CellMouseDown);
            this.dgvRows.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.DgvRows_CellValidating);
            this.dgvRows.Scroll += new System.Windows.Forms.ScrollEventHandler(this.dgvRows_Scroll);
            this.dgvRows.SelectionChanged += new System.EventHandler(this.DgvRows_SelectionChanged);
            // 
            // dgvRowsIDCol
            // 
            this.dgvRowsIDCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvRowsIDCol.DataPropertyName = "ID";
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.dgvRowsIDCol.DefaultCellStyle = dataGridViewCellStyle1;
            this.dgvRowsIDCol.HeaderText = "Row";
            this.dgvRowsIDCol.MinimumWidth = 6;
            this.dgvRowsIDCol.Name = "dgvRowsIDCol";
            this.dgvRowsIDCol.Width = 55;
            // 
            // dgvRowsNameCol
            // 
            this.dgvRowsNameCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dgvRowsNameCol.DataPropertyName = "Name";
            this.dgvRowsNameCol.HeaderText = "Name";
            this.dgvRowsNameCol.MinimumWidth = 6;
            this.dgvRowsNameCol.Name = "dgvRowsNameCol";
            // 
            // menuStrip3
            // 
            this.menuStrip3.BackColor = System.Drawing.Color.Transparent;
            this.menuStrip3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.menuStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.filter_Rows,
            this.button_FilterRows,
            this.button_ResetFilterRows});
            this.menuStrip3.Location = new System.Drawing.Point(0, 0);
            this.menuStrip3.Name = "menuStrip3";
            this.menuStrip3.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.menuStrip3.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.menuStrip3.Size = new System.Drawing.Size(386, 31);
            this.menuStrip3.TabIndex = 2;
            this.menuStrip3.Text = "menuStrip3";
            // 
            // filter_Rows
            // 
            this.filter_Rows.AutoToolTip = true;
            this.filter_Rows.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.filter_Rows.Margin = new System.Windows.Forms.Padding(2);
            this.filter_Rows.Name = "filter_Rows";
            this.filter_Rows.Size = new System.Drawing.Size(140, 23);
            // 
            // button_FilterRows
            // 
            this.button_FilterRows.AutoToolTip = true;
            this.button_FilterRows.BackColor = System.Drawing.Color.DarkGray;
            this.button_FilterRows.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.button_FilterRows.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.button_FilterRows.Margin = new System.Windows.Forms.Padding(2);
            this.button_FilterRows.Name = "button_FilterRows";
            this.button_FilterRows.Size = new System.Drawing.Size(50, 23);
            this.button_FilterRows.Text = "Apply";
            this.button_FilterRows.ToolTipText = "Apply row view filter.";
            this.button_FilterRows.Click += new System.EventHandler(this.button_FilterRows_Click);
            // 
            // button_ResetFilterRows
            // 
            this.button_ResetFilterRows.AutoToolTip = true;
            this.button_ResetFilterRows.BackColor = System.Drawing.Color.DarkGray;
            this.button_ResetFilterRows.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.button_ResetFilterRows.Margin = new System.Windows.Forms.Padding(2);
            this.button_ResetFilterRows.Name = "button_ResetFilterRows";
            this.button_ResetFilterRows.Size = new System.Drawing.Size(47, 23);
            this.button_ResetFilterRows.Text = "Reset";
            this.button_ResetFilterRows.ToolTipText = "Reset row view.";
            this.button_ResetFilterRows.Click += new System.EventHandler(this.button_ResetFilterRows_Click);
            // 
            // dgvCells
            // 
            this.dgvCells.AllowUserToAddRows = false;
            this.dgvCells.AllowUserToDeleteRows = false;
            this.dgvCells.AllowUserToResizeColumns = false;
            this.dgvCells.AllowUserToResizeRows = false;
            this.dgvCells.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCells.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgvCellsNameCol,
            this.dgvCellsEditorNameCol,
            this.dgvCellsValueCol,
            this.dgvCellsTypeCol});
            this.dgvCells.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvCells.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvCells.Location = new System.Drawing.Point(0, 31);
            this.dgvCells.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.dgvCells.MinimumSize = new System.Drawing.Size(187, 0);
            this.dgvCells.MultiSelect = false;
            this.dgvCells.Name = "dgvCells";
            this.dgvCells.RowHeadersVisible = false;
            this.dgvCells.RowHeadersWidth = 51;
            this.dgvCells.Size = new System.Drawing.Size(491, 641);
            this.dgvCells.TabIndex = 2;
            this.dgvCells.CellContextMenuStripNeeded += new System.Windows.Forms.DataGridViewCellContextMenuStripNeededEventHandler(this.dgvCells_CellContextMenuStripNeeded);
            this.dgvCells.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.DgvCells_CellFormatting);
            this.dgvCells.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvCells_CellMouseDown);
            this.dgvCells.CellParsing += new System.Windows.Forms.DataGridViewCellParsingEventHandler(this.DgvCells_CellParsing);
            this.dgvCells.CellToolTipTextNeeded += new System.Windows.Forms.DataGridViewCellToolTipTextNeededEventHandler(this.DgvCells_CellToolTipTextNeeded);
            this.dgvCells.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.DgvCells_CellValidating);
            this.dgvCells.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.DgvCells_DataBindingComplete);
            this.dgvCells.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.DgvCells_DataError);
            this.dgvCells.Scroll += new System.Windows.Forms.ScrollEventHandler(this.dgvCells_Scroll);
            this.dgvCells.SelectionChanged += new System.EventHandler(this.dgvCells_SelectionChanged);
            // 
            // dgvCellsNameCol
            // 
            this.dgvCellsNameCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvCellsNameCol.DataPropertyName = "Name";
            this.dgvCellsNameCol.HeaderText = "Field";
            this.dgvCellsNameCol.MinimumWidth = 60;
            this.dgvCellsNameCol.Name = "dgvCellsNameCol";
            this.dgvCellsNameCol.ReadOnly = true;
            this.dgvCellsNameCol.Width = 60;
            // 
            // dgvCellsEditorNameCol
            // 
            this.dgvCellsEditorNameCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvCellsEditorNameCol.DataPropertyName = "EditorName";
            this.dgvCellsEditorNameCol.HeaderText = "Field";
            this.dgvCellsEditorNameCol.MinimumWidth = 60;
            this.dgvCellsEditorNameCol.Name = "dgvCellsEditorNameCol";
            this.dgvCellsEditorNameCol.ReadOnly = true;
            this.dgvCellsEditorNameCol.Width = 60;
            // 
            // dgvCellsValueCol
            // 
            this.dgvCellsValueCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvCellsValueCol.DataPropertyName = "Value";
            this.dgvCellsValueCol.HeaderText = "Value";
            this.dgvCellsValueCol.MinimumWidth = 50;
            this.dgvCellsValueCol.Name = "dgvCellsValueCol";
            this.dgvCellsValueCol.Width = 60;
            // 
            // dgvCellsTypeCol
            // 
            this.dgvCellsTypeCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvCellsTypeCol.DataPropertyName = "Type";
            this.dgvCellsTypeCol.HeaderText = "Type";
            this.dgvCellsTypeCol.MinimumWidth = 30;
            this.dgvCellsTypeCol.Name = "dgvCellsTypeCol";
            this.dgvCellsTypeCol.ReadOnly = true;
            this.dgvCellsTypeCol.Width = 56;
            // 
            // menuStrip4
            // 
            this.menuStrip4.BackColor = System.Drawing.Color.Transparent;
            this.menuStrip4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.menuStrip4.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.filter_Cells,
            this.button_FilterCells,
            this.button_ResetFilterCells});
            this.menuStrip4.Location = new System.Drawing.Point(0, 0);
            this.menuStrip4.Name = "menuStrip4";
            this.menuStrip4.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.menuStrip4.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.menuStrip4.Size = new System.Drawing.Size(491, 31);
            this.menuStrip4.TabIndex = 3;
            this.menuStrip4.Text = "menuStrip4";
            // 
            // filter_Cells
            // 
            this.filter_Cells.AutoToolTip = true;
            this.filter_Cells.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.filter_Cells.Margin = new System.Windows.Forms.Padding(2);
            this.filter_Cells.Name = "filter_Cells";
            this.filter_Cells.Size = new System.Drawing.Size(140, 23);
            // 
            // button_FilterCells
            // 
            this.button_FilterCells.AutoToolTip = true;
            this.button_FilterCells.BackColor = System.Drawing.Color.DarkGray;
            this.button_FilterCells.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.button_FilterCells.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.button_FilterCells.Margin = new System.Windows.Forms.Padding(2);
            this.button_FilterCells.Name = "button_FilterCells";
            this.button_FilterCells.Size = new System.Drawing.Size(50, 23);
            this.button_FilterCells.Text = "Apply";
            this.button_FilterCells.ToolTipText = "Apply field view filter.";
            this.button_FilterCells.Click += new System.EventHandler(this.button_FilterCells_Click);
            // 
            // button_ResetFilterCells
            // 
            this.button_ResetFilterCells.AutoToolTip = true;
            this.button_ResetFilterCells.BackColor = System.Drawing.Color.DarkGray;
            this.button_ResetFilterCells.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.button_ResetFilterCells.Margin = new System.Windows.Forms.Padding(2);
            this.button_ResetFilterCells.Name = "button_ResetFilterCells";
            this.button_ResetFilterCells.Size = new System.Drawing.Size(47, 23);
            this.button_ResetFilterCells.Text = "Reset";
            this.button_ResetFilterCells.ToolTipText = "Reset field view.";
            this.button_ResetFilterCells.Click += new System.EventHandler(this.button_ResetFilterCells_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.processLabel});
            this.statusStrip1.Location = new System.Drawing.Point(2, 698);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            this.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.statusStrip1.Size = new System.Drawing.Size(1136, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 9;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // processLabel
            // 
            this.processLabel.BorderStyle = System.Windows.Forms.Border3DStyle.Raised;
            this.processLabel.Name = "processLabel";
            this.processLabel.Size = new System.Drawing.Size(103, 17);
            this.processLabel.Text = "No active process.";
            // 
            // fbdExport
            // 
            this.fbdExport.Description = "Choose the folder to export parambnds to";
            this.fbdExport.RootFolder = System.Environment.SpecialFolder.MyComputer;
            // 
            // fieldContextMenu
            // 
            this.fieldContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.GotoReference1MenuItem,
            this.GotoReference2MenuItem,
            this.GotoReference3MenuItem,
            this.GotoReference4MenuItem,
            this.GotoReference5MenuItem,
            this.GotoReference6MenuItem});
            this.fieldContextMenu.Name = "fieldContextMenu";
            this.fieldContextMenu.Size = new System.Drawing.Size(160, 136);
            this.fieldContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.fieldContextMenu_Opening);
            // 
            // GotoReference1MenuItem
            // 
            this.GotoReference1MenuItem.Name = "GotoReference1MenuItem";
            this.GotoReference1MenuItem.Size = new System.Drawing.Size(159, 22);
            this.GotoReference1MenuItem.Text = "Reference 1 Text";
            this.GotoReference1MenuItem.Click += new System.EventHandler(this.GotoReference1MenuItem_Click);
            // 
            // GotoReference2MenuItem
            // 
            this.GotoReference2MenuItem.Name = "GotoReference2MenuItem";
            this.GotoReference2MenuItem.Size = new System.Drawing.Size(159, 22);
            this.GotoReference2MenuItem.Text = "Reference 2 Text";
            this.GotoReference2MenuItem.Click += new System.EventHandler(this.GotoReference2MenuItem_Click);
            // 
            // GotoReference3MenuItem
            // 
            this.GotoReference3MenuItem.Name = "GotoReference3MenuItem";
            this.GotoReference3MenuItem.Size = new System.Drawing.Size(159, 22);
            this.GotoReference3MenuItem.Text = "Reference 3 Text";
            this.GotoReference3MenuItem.Click += new System.EventHandler(this.GotoReference3MenuItem_Click);
            // 
            // GotoReference4MenuItem
            // 
            this.GotoReference4MenuItem.Name = "GotoReference4MenuItem";
            this.GotoReference4MenuItem.Size = new System.Drawing.Size(159, 22);
            this.GotoReference4MenuItem.Text = "Reference 4 Text";
            this.GotoReference4MenuItem.Click += new System.EventHandler(this.GotoReference4MenuItem_Click);
            // 
            // GotoReference5MenuItem
            // 
            this.GotoReference5MenuItem.Name = "GotoReference5MenuItem";
            this.GotoReference5MenuItem.Size = new System.Drawing.Size(159, 22);
            this.GotoReference5MenuItem.Text = "Reference 5 Text";
            this.GotoReference5MenuItem.Click += new System.EventHandler(this.GotoReference5MenuItem_Click);
            // 
            // GotoReference6MenuItem
            // 
            this.GotoReference6MenuItem.Name = "GotoReference6MenuItem";
            this.GotoReference6MenuItem.Size = new System.Drawing.Size(159, 22);
            this.GotoReference6MenuItem.Text = "Reference 6 Text";
            this.GotoReference6MenuItem.Click += new System.EventHandler(this.GotoReference6MenuItem_Click);
            // 
            // rowContextMenu
            // 
            this.rowContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToParamMenuItem});
            this.rowContextMenu.Name = "rowContextMenu";
            this.rowContextMenu.Size = new System.Drawing.Size(154, 26);
            this.rowContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.rowContextMenu_Opening);
            // 
            // copyToParamMenuItem
            // 
            this.copyToParamMenuItem.Name = "copyToParamMenuItem";
            this.copyToParamMenuItem.Size = new System.Drawing.Size(153, 22);
            this.copyToParamMenuItem.Text = "Copy to Param";
            this.copyToParamMenuItem.Click += new System.EventHandler(this.copyToParamMenuItem_Click);
            // 
            // secondaryFilePath
            // 
            this.secondaryFilePath.Filter = "All files|*.*";
            // 
            // BottomToolStripPanel
            // 
            this.BottomToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.BottomToolStripPanel.Name = "BottomToolStripPanel";
            this.BottomToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.BottomToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.BottomToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // TopToolStripPanel
            // 
            this.TopToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.TopToolStripPanel.Name = "TopToolStripPanel";
            this.TopToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.TopToolStripPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.TopToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.TopToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // RightToolStripPanel
            // 
            this.RightToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.RightToolStripPanel.Name = "RightToolStripPanel";
            this.RightToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.RightToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.RightToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // LeftToolStripPanel
            // 
            this.LeftToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.LeftToolStripPanel.Name = "LeftToolStripPanel";
            this.LeftToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.LeftToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.LeftToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // ContentPanel
            // 
            this.ContentPanel.Size = new System.Drawing.Size(150, 150);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightGray;
            this.ClientSize = new System.Drawing.Size(1140, 722);
            this.Controls.Add(this.splitContainer2);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "Main";
            this.Padding = new System.Windows.Forms.Padding(2);
            this.Text = "Yapped - Rune Bear Special Edition (<version>, <framework-version>)";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Load += new System.EventHandler(this.Main_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvParams)).EndInit();
            this.menuStrip2.ResumeLayout(false);
            this.menuStrip2.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRows)).EndInit();
            this.menuStrip3.ResumeLayout(false);
            this.menuStrip3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCells)).EndInit();
            this.menuStrip4.ResumeLayout(false);
            this.menuStrip4.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.fieldContextMenu.ResumeLayout(false);
            this.rowContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        // Token: 0x0400001E RID: 30
        private global::System.ComponentModel.IContainer components;

        // Token: 0x0400001F RID: 31
        private global::System.Windows.Forms.DataGridView dgvParams;

        // Token: 0x04000020 RID: 32
        private global::System.Windows.Forms.DataGridView dgvRows;

        // Token: 0x04000021 RID: 33
        private global::System.Windows.Forms.DataGridView dgvCells;

        // Token: 0x04000022 RID: 34
        private global::System.Windows.Forms.OpenFileDialog dataFileDialog;

        // Token: 0x04000023 RID: 35
        private global::System.Windows.Forms.OpenFileDialog importedRegulation;

        // Token: 0x04000024 RID: 36
        private global::System.Windows.Forms.SplitContainer splitContainer1;

        // Token: 0x04000025 RID: 37
        private global::System.Windows.Forms.MenuStrip menuStrip1;

        // Token: 0x04000026 RID: 38
        private global::System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;

        // Token: 0x04000027 RID: 39
        private global::System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;

        // Token: 0x04000028 RID: 40
        private global::System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;

        // Token: 0x04000029 RID: 41
        private global::System.Windows.Forms.ToolStripMenuItem restoreToolStripMenuItem;

        // Token: 0x0400002A RID: 42
        private global::System.Windows.Forms.ToolStripMenuItem exploreToolStripMenuItem;

        // Token: 0x0400002B RID: 43
        private global::System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;

        // Token: 0x0400002C RID: 44
        private global::System.Windows.Forms.ToolStripMenuItem addRowToolStripMenuItem;

        // Token: 0x0400002D RID: 45
        private global::System.Windows.Forms.ToolStripMenuItem deleteRowToolStripMenuItem;

        // Token: 0x0400002E RID: 46
        private global::System.Windows.Forms.DataGridViewTextBoxColumn dgvParamsParamCol;

        // Token: 0x0400002F RID: 47
        private global::System.Windows.Forms.DataGridViewTextBoxColumn dgvRowsIDCol;

        // Token: 0x04000030 RID: 48
        private global::System.Windows.Forms.DataGridViewTextBoxColumn dgvRowsNameCol;

        // Token: 0x04000031 RID: 49
        private global::System.Windows.Forms.SplitContainer splitContainer2;

        // Token: 0x04000032 RID: 50
        private global::System.Windows.Forms.ToolStripSeparator toolStripSeparator2;

        // Token: 0x04000033 RID: 51
        private global::System.Windows.Forms.ToolStripMenuItem findRowToolStripMenuItem;

        // Token: 0x04000034 RID: 52
        private global::System.Windows.Forms.ToolStripMenuItem gotoRowToolStripMenuItem;

        // Token: 0x04000035 RID: 53
        private global::System.Windows.Forms.ToolStripMenuItem findNextRowToolStripMenuItem;

        // Token: 0x04000036 RID: 54
        private global::System.Windows.Forms.ToolStripMenuItem findFieldToolStripMenuItem;

        // Token: 0x04000037 RID: 55
        private global::System.Windows.Forms.ToolStripMenuItem findNextFieldToolStripMenuItem;

        // Token: 0x04000038 RID: 56
        private global::System.Windows.Forms.StatusStrip statusStrip1;

        // Token: 0x04000039 RID: 57
        private global::System.Windows.Forms.ToolStripStatusLabel processLabel;

        // Token: 0x0400003A RID: 58
        private global::System.Windows.Forms.ToolStripMenuItem duplicateRowToolStripMenuItem;

        // Token: 0x0400003B RID: 59
        private global::System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;

        // Token: 0x0400003C RID: 60
        private global::System.Windows.Forms.FolderBrowserDialog fbdExport;

        // Token: 0x0400003D RID: 61
        private global::System.Windows.Forms.ToolStripComboBox toolStripComboBoxGame;

        // Token: 0x0400003E RID: 62
        private global::System.Windows.Forms.ToolStripMenuItem WorkflowToolStripMenuItem;

        // Token: 0x0400003F RID: 63
        private global::System.Windows.Forms.ToolStripMenuItem settingsMenuItem;

        // Token: 0x04000040 RID: 64
        private global::System.Windows.Forms.ToolStripMenuItem fieldAdjusterMenuItem;

        // Token: 0x04000041 RID: 65
        private global::System.Windows.Forms.ToolStripMenuItem ProjectFolderMenuItem;

        // Token: 0x04000042 RID: 66
        private global::System.Windows.Forms.ToolStripSeparator toolStripSeparator5;

        // Token: 0x04000043 RID: 67
        private global::System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem;

        // Token: 0x04000044 RID: 68
        private global::System.Windows.Forms.ToolStripMenuItem importDataMenuItem;

        // Token: 0x04000045 RID: 69
        private global::System.Windows.Forms.ToolStripMenuItem exportDataMenuItem;

        // Token: 0x04000046 RID: 70
        private global::System.Windows.Forms.ToolStripSeparator toolStripSeparator7;

        // Token: 0x04000047 RID: 71
        private global::System.Windows.Forms.ToolStripMenuItem massImportDataMenuItem;

        // Token: 0x04000048 RID: 72
        private global::System.Windows.Forms.ToolStripMenuItem massExportDataMenuItem;

        // Token: 0x04000049 RID: 73
        private global::System.Windows.Forms.ToolStripSeparator toolStripSeparator8;

        // Token: 0x0400004A RID: 74
        private global::System.Windows.Forms.ToolStripMenuItem importRowNames_Project_MenuItem;

        // Token: 0x0400004B RID: 75
        private global::System.Windows.Forms.ToolStripMenuItem importRowNames_Stock_MenuItem;

        // Token: 0x0400004C RID: 76
        private global::System.Windows.Forms.ToolStripSeparator toolStripSeparator9;

        // Token: 0x0400004D RID: 77
        private global::System.Windows.Forms.ToolStripMenuItem fieldExporterMenuItem;

        // Token: 0x0400004E RID: 78
        private global::System.Windows.Forms.ToolStripMenuItem valueReferenceFinderMenuItem;

        // Token: 0x0400004F RID: 79
        private global::System.Windows.Forms.ToolStripMenuItem rowReferenceFinderMenuItem;

        // Token: 0x04000050 RID: 80
        private global::System.Windows.Forms.ToolStripMenuItem affinityGeneratorMenuItem;

        // Token: 0x04000051 RID: 81
        private global::System.Windows.Forms.ToolStripSeparator toolStripSeparator1;

        // Token: 0x04000052 RID: 82
        private global::System.Windows.Forms.ToolStripMenuItem exportRowNames_Project_MenuItem;

        // Token: 0x04000053 RID: 83
        private global::System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;

        // Token: 0x04000054 RID: 84
        private global::System.Windows.Forms.ToolStripMenuItem viewSettingsToolStripMenuItem;

        // Token: 0x04000055 RID: 85
        private global::System.Windows.Forms.ToolStripMenuItem logParamSizesToolStripMenuItem;

        // Token: 0x04000056 RID: 86
        private global::System.Windows.Forms.ToolStripMenuItem toggleFieldNameTypeToolStripMenuItem;

        // Token: 0x04000057 RID: 87
        private global::System.Windows.Forms.ToolStripMenuItem toggleFieldTypeVisibilityToolStripMenuItem;

        // Token: 0x04000058 RID: 88
        private global::System.Windows.Forms.ContextMenuStrip fieldContextMenu;

        // Token: 0x04000059 RID: 89
        private global::System.Windows.Forms.ToolStripMenuItem GotoReference6MenuItem;

        // Token: 0x0400005A RID: 90
        private global::System.Windows.Forms.DataGridViewTextBoxColumn dgvCellsNameCol;

        // Token: 0x0400005B RID: 91
        private global::System.Windows.Forms.DataGridViewTextBoxColumn dgvCellsEditorNameCol;

        // Token: 0x0400005C RID: 92
        private global::System.Windows.Forms.DataGridViewTextBoxColumn dgvCellsValueCol;

        // Token: 0x0400005D RID: 93
        private global::System.Windows.Forms.DataGridViewTextBoxColumn dgvCellsTypeCol;

        // Token: 0x0400005E RID: 94
        private global::System.Windows.Forms.ToolStripMenuItem GotoReference1MenuItem;

        // Token: 0x0400005F RID: 95
        private global::System.Windows.Forms.ToolStripMenuItem GotoReference2MenuItem;

        // Token: 0x04000060 RID: 96
        private global::System.Windows.Forms.ToolStripMenuItem GotoReference3MenuItem;

        // Token: 0x04000061 RID: 97
        private global::System.Windows.Forms.ToolStripMenuItem GotoReference4MenuItem;

        // Token: 0x04000062 RID: 98
        private global::System.Windows.Forms.ToolStripMenuItem GotoReference5MenuItem;

        // Token: 0x04000063 RID: 99
        private global::System.Windows.Forms.ContextMenuStrip rowContextMenu;

        // Token: 0x04000064 RID: 100
        private global::System.Windows.Forms.ToolStripMenuItem copyToParamMenuItem;

        // Token: 0x04000065 RID: 101
        private global::System.Windows.Forms.ToolStripMenuItem viewInterfaceSettingsToolStripMenuItem;

        // Token: 0x04000066 RID: 102
        private global::System.Windows.Forms.ToolStripMenuItem viewDataSettingsToolStripMenuItem;

        // Token: 0x04000067 RID: 103
        private global::System.Windows.Forms.ToolStripMenuItem secondaryDataToolStripMenuItem;

        // Token: 0x04000068 RID: 104
        private global::System.Windows.Forms.ToolStripMenuItem selectSecondaryFileToolStripMenuItem;

        // Token: 0x04000069 RID: 105
        private global::System.Windows.Forms.OpenFileDialog secondaryFilePath;

        // Token: 0x0400006A RID: 106
        private global::System.Windows.Forms.ToolStripMenuItem showParamDifferencesToolStripMenuItem;

        // Token: 0x0400006B RID: 107
        private global::System.Windows.Forms.ToolStripMenuItem clearSecondaryFileToolMenuItem;

        // Token: 0x0400006C RID: 108
        private global::System.Windows.Forms.MenuStrip menuStrip2;

        // Token: 0x0400006D RID: 109
        private global::System.Windows.Forms.ToolStripTextBox filter_Params;

        // Token: 0x0400006E RID: 110
        private global::System.Windows.Forms.ToolStripMenuItem button_FilterParams;

        // Token: 0x0400006F RID: 111
        private global::System.Windows.Forms.ToolStripPanel BottomToolStripPanel;

        // Token: 0x04000070 RID: 112
        private global::System.Windows.Forms.ToolStripPanel TopToolStripPanel;

        // Token: 0x04000071 RID: 113
        private global::System.Windows.Forms.ToolStripPanel RightToolStripPanel;

        // Token: 0x04000072 RID: 114
        private global::System.Windows.Forms.ToolStripPanel LeftToolStripPanel;

        // Token: 0x04000073 RID: 115
        private global::System.Windows.Forms.ToolStripContentPanel ContentPanel;

        // Token: 0x04000074 RID: 116
        private global::System.Windows.Forms.ToolStripMenuItem button_ResetFilterParams;

        // Token: 0x04000075 RID: 117
        private global::System.Windows.Forms.MenuStrip menuStrip3;

        // Token: 0x04000076 RID: 118
        private global::System.Windows.Forms.ToolStripTextBox filter_Rows;

        // Token: 0x04000077 RID: 119
        private global::System.Windows.Forms.ToolStripMenuItem button_FilterRows;

        // Token: 0x04000078 RID: 120
        private global::System.Windows.Forms.ToolStripMenuItem button_ResetFilterRows;

        // Token: 0x04000079 RID: 121
        private global::System.Windows.Forms.MenuStrip menuStrip4;

        // Token: 0x0400007A RID: 122
        private global::System.Windows.Forms.ToolStripTextBox filter_Cells;

        // Token: 0x0400007B RID: 123
        private global::System.Windows.Forms.ToolStripMenuItem button_FilterCells;

        // Token: 0x0400007C RID: 124
        private global::System.Windows.Forms.ToolStripMenuItem button_ResetFilterCells;

        // Token: 0x0400007D RID: 125
        private global::System.Windows.Forms.ToolTip toolTip_filterParams;

        // Token: 0x0400007E RID: 126
        private global::System.Windows.Forms.ToolTip toolTip_filterRows;

        // Token: 0x0400007F RID: 127
        private global::System.Windows.Forms.ToolTip toolTip_filterCells;

        // Token: 0x04000080 RID: 128
        private global::System.Windows.Forms.ToolStripMenuItem toggleFilterVisibilityToolStripMenuItem;

        // Token: 0x04000081 RID: 129
        private global::System.Windows.Forms.ToolStripMenuItem viewFilterSettingsToolStripMenuItem;
    }
}
