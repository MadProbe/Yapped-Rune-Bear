using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Media;
using System.Text.RegularExpressions;
using Chomp.Forms;
using Chomp.Properties;
using Chomp.Tools;
using Chomp.Util;
using SoulsFormats;
using SoulsFormats.Binder;
using SoulsFormats.Binder.BND3;
using SoulsFormats.Binder.BND4;
using SoulsFormats.Formats;
using SoulsFormats.Formats.PARAM;
using SoulsFormats.Util;
using static SoulsFormats.PARAM;
using Format = SoulsFormats.Binder.Binder.Format;

namespace Chomp {
    // Token: 0x02000002 RID: 2
    public partial class Main : Form {
        // Token: 0x06000001 RID: 1 RVA: 0x00002048 File Offset: 0x00000248
        public Main() {
            Instance = this;
            this.InitializeComponent();
            //this.MakeDarkTheme(settings.DarkTheme);
            this.GoToReferenceMenuStripItems = new[] {
                this.GotoReference1MenuItem,
                this.GotoReference2MenuItem,
                this.GotoReference3MenuItem,
                this.GotoReference4MenuItem,
                this.GotoReference5MenuItem,
                this.GotoReference6MenuItem
            };
            this.dgvRows.DataSource = this.rowSource;
            this.dgvParams.AutoGenerateColumns = false;
            this.dgvRows.AutoGenerateColumns = false;
            this.dgvCells.AutoGenerateColumns = false;
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;
        }

        // Token: 0x06000002 RID: 2 RVA: 0x00002158 File Offset: 0x00000358
        private void Main_Load(object sender, EventArgs e) {
            this.Text = $"Yapped - Rune Bear Special Edition ({Assembly.GetExecutingAssembly().GetName().Version}, {RuntimeInformation.FrameworkDescription})";
            this.toolTip_filterParams.SetToolTip(this.filter_Params.Control, this.filter_Params.ToolTipText);
            this.toolTip_filterRows.SetToolTip(this.filter_Rows.Control, this.filter_Rows.ToolTipText);
            this.toolTip_filterCells.SetToolTip(this.filter_Cells.Control, this.filter_Cells.ToolTipText);
            this.InvalidationMode = false;
            this.dgvParams.SetDoubleBuffered();
            this.dgvRows.SetDoubleBuffered();
            this.dgvCells.SetDoubleBuffered();
            this.Location = settings.WindowLocation;
            if (settings.WindowSize.Width >= this.MinimumSize.Width && settings.WindowSize.Height >= this.MinimumSize.Height) {
                this.Size = settings.WindowSize;
            }
            if (settings.WindowMaximized) {
                this.WindowState = FormWindowState.Maximized;
            }
            this.toolStripComboBoxGame.ComboBox.DisplayMember = "Name";
            this.toolStripComboBoxGame.Items.AddRange(GameMode.Modes);
            GameMode.GameType game = Enum.Parse<GameMode.GameType>(settings.GameType);
            this.toolStripComboBoxGame.SelectedIndex = Array.FindIndex(GameMode.Modes, m => m.Game == game);
            if (this.toolStripComboBoxGame.SelectedIndex == -1) {
                this.toolStripComboBoxGame.SelectedIndex = 0;
            }
            if (settings.ProjectName == "") {
                settings.ProjectName = "ExampleMod";
            }
            this.regulationPath = settings.RegulationPath;
            this.splitContainer2.SplitterDistance = settings.SplitterDistance2;
            this.splitContainer1.SplitterDistance = settings.SplitterDistance1;
            this.secondaryFilePath.FileName = settings.SecondaryFilePath;
            settings.ParamDifferenceMode = false;
            long timeToLoad = Globals.MeasureTimeSpent(() => {
                this.BuildParamDefs();
                this.BuildParamTdfs();
                this.BuildBoolTypes();
                this.BuildCustomTypes();
                this.LoadParams(true);
                if (this.secondaryFilePath.FileName != "") {
                    this.LoadSecondaryParams(true);
                }
            });
            this.processLabel.Text += $" Loaded in {timeToLoad}ms";
            // this.dgvParams.Refresh();
            // MessageBox.Show(string.Join('\n', this.dgvParams.Rows.Cast<DataGridViewRow>().SelectMany(x => x.Cells.Cast<DataGridViewCell>()).Select(x => x.Value).ToArray()));
            foreach (string[] components in DCVIndicesRegex().Matches(settings.DGVIndices).Select(obj => obj.Value.Split(':'))) {
                this.dgvIndices[components[0]] = (int.Parse(components[1]), int.Parse(components[2]));
            }

            if (settings.SelectedParam >= this.dgvParams.Rows.Count) {
                settings.SelectedParam = 0;
            }
            if (this.dgvParams.Rows.Count > 0) {
                this.dgvParams.ClearSelection();
                this.dgvParams.Rows[settings.SelectedParam].Selected = true;
                this.dgvParams.CurrentCell = this.dgvParams.SelectedCells[0];
            }
        }

        // Token: 0x06000003 RID: 3 RVA: 0x00002544 File Offset: 0x00000744
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e) {
            settings.WindowMaximized = this.WindowState == FormWindowState.Maximized;
            if (this.WindowState == FormWindowState.Normal) {
                settings.WindowLocation = this.Location;
                settings.WindowSize = this.Size;
            } else {
                settings.WindowLocation = this.RestoreBounds.Location;
                settings.WindowSize = this.RestoreBounds.Size;
            }
            settings.GameType = (this.toolStripComboBoxGame.SelectedItem as GameMode).Game.ToString();
            settings.RegulationPath = this.regulationPath;
            settings.SplitterDistance2 = this.splitContainer2.SplitterDistance;
            settings.SplitterDistance1 = this.splitContainer1.SplitterDistance;
            if (this.dgvParams.SelectedCells.Count > 0) {
                settings.SelectedParam = this.dgvParams.SelectedCells[0].RowIndex;
            }
            this.dgvParams.ClearSelection();
            settings.DGVIndices = string.Join(",", this.dgvIndices.Select(kvpair => $"{kvpair.Key}:{kvpair.Value.Row}:{kvpair.Value.Cell}").ToArray());
        }

        // Token: 0x06000004 RID: 4 RVA: 0x00002704 File Offset: 0x00000904
        public string GetProjectDirectory(string subfolder) => $"Projects\\\\{settings.ProjectName}\\\\{subfolder}\\\\{(this.toolStripComboBoxGame.SelectedItem as GameMode).Directory}";

        // Token: 0x06000005 RID: 5 RVA: 0x00002760 File Offset: 0x00000960
        public string GetParamdexDirectory(string subfolder) => $"Paramdex\\\\{(this.toolStripComboBoxGame.SelectedItem as GameMode).Directory}{(subfolder == "" ? "" : "\\\\" + subfolder)}";

        // Token: 0x06000006 RID: 6 RVA: 0x000027B4 File Offset: 0x000009B4
        private void LoadParams(bool isSilent) {
            long timeSpentToLoad = Globals.MeasureTimeSpent(() => {
                this.primary_result = this.LoadParamResult(this.regulationPath, false);
                if (this.primary_result == null) {
                    this.exportToolStripMenuItem.Enabled = false;
                    return;
                }
                this.encrypted = this.primary_result.Encrypted;
                this.regulation = this.primary_result.ParamBND;
                this.exportToolStripMenuItem.Enabled = this.encrypted;
                foreach (string name in this.primary_result.ParamWrappers.Select(wrapper => wrapper.Name).Where(name => !this.dgvIndices.ContainsKey(name))) {
                    this.dgvIndices[name] = (0, 0);
                }
                this.dgvParams.DataSource = this.primary_result.ParamWrappers;
                foreach (DataGridViewRow row in this.dgvParams.Rows.Cast<DataGridViewRow>().Where(row => (row.DataBoundItem as ParamWrapper).Error)) {
                    row.Cells[0].Style.BackColor = Color.Pink;
                }
            });

            if (this.primary_result != null && !isSilent && !settings.ShowConfirmationMessages) {
                _ = MessageBox.Show($"Primary file loaded in {timeSpentToLoad}ms.", "File Data", MessageBoxButtons.OK);
            }
        }

        // Token: 0x06000007 RID: 7 RVA: 0x00002938 File Offset: 0x00000B38
        private void LoadSecondaryParams(bool isSilent) {
            this.secondary_result = this.LoadParamResult(settings.SecondaryFilePath, true);
            if (this.secondary_result == null) {
                Utility.ShowError("Failed to load secondary data file.");
                return;
            }
            this.secondary_encrypted = this.secondary_result.Encrypted;
            this.secondary_regulation = this.secondary_result.ParamBND;
            if (!isSilent && !settings.ShowConfirmationMessages) {
                _ = MessageBox.Show("Secondary file loaded.", "File Data", MessageBoxButtons.OK);
            }
        }

        // Token: 0x06000008 RID: 8 RVA: 0x000029B4 File Offset: 0x00000BB4
        private void BuildParamDefs() {
            this.paramdefs.Clear();
            foreach (string path in Directory.GetFiles(this.GetParamdexDirectory("Defs"), "*.xml")) {
                try {
                    this.paramdefs.Add(PARAMDEF.XmlDeserialize(path));
                } catch (Exception ex) {
                    Utility.ShowError($"""
                        Failed to load layout {Path.GetFileNameWithoutExtension(path)}.txt
                        
                        {ex}
                        """);
                }
            }
        }

        // Token: 0x06000009 RID: 9 RVA: 0x00002A38 File Offset: 0x00000C38
        private void BuildParamTdfs() {
            this.paramtdfs.Clear();
            this.tdf_dict.Clear();
            foreach (string text in Directory.GetFiles(this.GetParamdexDirectory("Tdfs"), "*.tdf")) {
                try {
                    this.paramtdfs.Add(new PARAMTDF(File.ReadAllText(text)));
                } catch (Exception ex) {
                    Utility.ShowError($"""
                        Failed to load layout {Path.GetFileNameWithoutExtension(text)}.txt
                        
                        {ex}
                        """);
                }
            }
            foreach (PARAMTDF paramtdf2 in this.paramtdfs) {
                try {
                    this.tdf_dict.Add(paramtdf2.Name, paramtdf2);
                } catch (Exception ex2) {
                    Utility.ShowError($"""
                        Failed to add TDF {paramtdf2.Name}.
                        
                        {ex2}
                        """);
                }
            }
        }

        // Token: 0x0600000A RID: 10 RVA: 0x00002B48 File Offset: 0x00000D48
        private void BuildBoolTypes() {
            this.bool_type_tdfs.Clear();
            string boolean_type_file = $"{this.GetParamdexDirectory("Meta")}\\bool_types.txt";
            if (!File.Exists(boolean_type_file)) {
                this.bool_type_tdfs = null;
                return;
            }
            StreamReader reader;
            try {
                reader = new StreamReader(File.OpenRead(boolean_type_file));
            } catch (Exception ex) {
                Utility.ShowError($"""
                    Failed to open {boolean_type_file}.
                    
                    {ex}
                    """);
                return;
            }
            if (!reader.EndOfStream) {
                string line = reader.ReadLine();
                this.bool_type_tdfs.Add(line);
            }
        }

        // Token: 0x0600000B RID: 11 RVA: 0x00002BD8 File Offset: 0x00000DD8
        private void BuildCustomTypes() {
            this.custom_type_tdfs.Clear();
            string custom_type_file = $"{this.GetParamdexDirectory("Meta")}\\customizable_types.txt";
            if (!File.Exists(custom_type_file)) {
                this.custom_type_tdfs = null;
                return;
            }
            StreamReader reader;
            try {
                reader = new StreamReader(File.OpenRead(custom_type_file));
            } catch (Exception ex) {
                Utility.ShowError($"""
                    Failed to open {custom_type_file}.
                    
                    {ex}
                    """);
                return;
            }
            if (!reader.EndOfStream) {
                string line = reader.ReadLine();
                this.custom_type_tdfs.Add(line);
            }
        }

        // Token: 0x0600000C RID: 12 RVA: 0x00002C68 File Offset: 0x00000E68
        private LoadParamsResult LoadParamResult(string target_path, bool isSecondary) {
            if (!File.Exists(target_path)) {
                Utility.ShowError($"""
                    Parambnd not found:
                    {target_path}
                    Please browse to the Data0.bdt or parambnd you would like to edit.
                    """);
                return null;
            }
            var result = new LoadParamsResult() { ParamWrappers = new List<ParamWrapper>() };
            var gameMode = this.toolStripComboBoxGame.SelectedItem as GameMode;
            try {
                if (SoulsFile<BND4>.Is(target_path)) {
                    result.ParamBND = SoulsFile<BND4>.Read(target_path);
                    result.Encrypted = false;
                } else if (SoulsFile<BND3>.Is(target_path)) {
                    result.ParamBND = SoulsFile<BND3>.Read(target_path);
                    result.Encrypted = false;
                } else if (gameMode.Game is GameMode.GameType.DarkSouls2 or GameMode.GameType.DarkSouls2Scholar) {
                    result.ParamBND = Utility.DecryptDS2Regulation(target_path);
                    result.Encrypted = true;
                } else if (gameMode.Game == GameMode.GameType.DarkSouls3) {
                    result.ParamBND = SFUtil.DecryptDS3Regulation(target_path);
                    result.Encrypted = true;
                } else if (gameMode.Game == GameMode.GameType.EldenRing) {
                    result.ParamBND = SFUtil.DecryptERRegulation(target_path);
                    result.Encrypted = true;
                } else {
                    throw new FormatException("Unrecognized file format.");
                }
            } catch (DllNotFoundException ex3) when (ex3.Message.Contains("oo2core_6_win64.dll")) {
                Utility.ShowError("In order to load Sekiro params, you must copy oo2core_6_win64.dll from Sekiro into Yapped's lib folder.");
                return null;
            } catch (Exception ex) {
                Utility.ShowError($"""
                    "Failed to load parambnd:
                    {target_path}
                    
                    {ex}
                    """);
                return null;
            }
            if (!isSecondary) {
                this.processLabel.Text = target_path;
            }
            long timeInPARAMRead = 0;
            long timeInApplyPARAMDEF = 0;
            this.processLabel.Text += $" (cum in ass in {Globals.MeasureTimeSpent(() => {
                foreach (BinderFile file in result.ParamBND.Files.Where((BinderFile f) => f.Name.EndsWith(".param"))) {
                    string name = Path.GetFileNameWithoutExtension(file.Name);
                    try {
                        Unsafe.SkipInit(out PARAM param); // param init is guaranteed
                        timeInPARAMRead += Globals.MeasureTimeSpent(() => param = SoulsFile<PARAM>.Read(file.Bytes), new Stopwatch { });
                        timeInApplyPARAMDEF += Globals.MeasureTimeSpent(() => {
                            if (param.ApplyParamdefCarefully(this.paramdefs)) {
                                result.ParamWrappers.Add(new ParamWrapper(name, param, param.AppliedParamdef));
                            }
                        }, new Stopwatch { });
                    } catch (Exception ex2) {
                        Utility.ShowError($"""
                        Failed to load param file: {name}.param

                        {ex2}
                        """);
                    }
                }
            }, new Stopwatch { })}ms, Read in {timeInPARAMRead}ms, Apply in {timeInApplyPARAMDEF}ms)";
            result.ParamWrappers.Sort();
            return result;
        }

        // Token: 0x0600000D RID: 13 RVA: 0x00002EBC File Offset: 0x000010BC
        private void toggleFieldNameTypeToolStripMenuItem_Click(object sender, EventArgs e) {
            if (this.InvalidationMode) {
                return;
            }
            settings.CellView_ShowEditorNames = !settings.CellView_ShowEditorNames;
        }

        // Token: 0x0600000E RID: 14 RVA: 0x00002EEA File Offset: 0x000010EA
        private void toggleFieldTypeVisibilityToolStripMenuItem_Click(object sender, EventArgs e) {
            if (this.InvalidationMode) {
                return;
            }
            settings.CellView_ShowTypes = !settings.CellView_ShowTypes;
        }

        // Token: 0x0600000F RID: 15 RVA: 0x00002F18 File Offset: 0x00001118
        private void viewSettingsToolStripMenuItem_Click(object sender, EventArgs e) {
            if (this.InvalidationMode) {
                return;
            }
            if (new SettingsMenu().ShowDialog() == DialogResult.OK && !settings.ShowConfirmationMessages) {
                _ = MessageBox.Show("Settings changed.", "Settings", MessageBoxButtons.OK);
            }
            this.GenerateProjectDirectories(settings.ProjectName);
        }

        // Token: 0x06000010 RID: 16 RVA: 0x00002F68 File Offset: 0x00001168
        private void logParamSizesToolStripMenuItem_Click(object sender, EventArgs e) {
            if (this.InvalidationMode) {
                return;
            }
            string param_size_path = $"{this.GetProjectDirectory("Logs")}\\ParamSizes.log";
            if (this.primary_result != null) {
                using var output_file = new StreamWriter(param_size_path);
                foreach (ParamWrapper wrapper in this.primary_result.ParamWrappers) {
                    output_file.WriteLine(wrapper.Name.ToString());
                    output_file.WriteLine(wrapper.Param.DetectedSize.ToString());
                }
            }
            StartTextEditorIfNecessary(param_size_path);
            if (!settings.ShowConfirmationMessages) {
                _ = MessageBox.Show($"Param sizes logged to {param_size_path}", "Field Exporter", MessageBoxButtons.OK);
            }
        }

        // Token: 0x06000011 RID: 17 RVA: 0x000030C8 File Offset: 0x000012C8
        private void OpenToolStripMenuItem_Click(object sender, EventArgs e) {
            if (this.InvalidationMode) {
                return;
            }
            this.dataFileDialog.FileName = this.regulationPath;
            if (this.dataFileDialog.ShowDialog() == DialogResult.OK) {
                this.regulationPath = this.dataFileDialog.FileName;
                this.BuildParamDefs();
                this.BuildParamTdfs();
                this.BuildBoolTypes();
                this.BuildCustomTypes();
                this.LoadParams(false);
            }
        }

        // Token: 0x06000012 RID: 18 RVA: 0x0000312D File Offset: 0x0000132D
        private void SaveToolStripMenuItem_Click(object sender, EventArgs e) {
            if (!this.InvalidationMode) {
                long ms = Globals.MeasureTimeSpent(() => this.SaveParams(".bak"));
                if (!settings.ShowConfirmationMessages) {
                    _ = MessageBox.Show($"Params saved to {this.regulationPath} in {ms} ms", "Save", MessageBoxButtons.OK);
                }
            }
        }

        // Token: 0x06000013 RID: 19 RVA: 0x0000316C File Offset: 0x0000136C
        private void SaveParams(string backup_format) {
            foreach (BinderFile file in this.regulation.Files) {
                foreach (DataGridViewRow obj in this.dgvParams.Rows) {
                    ParamWrapper paramFile = obj.DataBoundItem as ParamWrapper ?? this.ThrowCannotGetParamWrapper<ParamWrapper>();
                    if (Path.GetFileNameWithoutExtension(file.Name) == paramFile.Name) {
                        try {
                            file.Bytes = paramFile.Param.Write();
                        } catch {
                            _ = MessageBox.Show($"Invalid data, failed to save {paramFile}. Data must be fixed before saving can complete.", "Save", MessageBoxButtons.OK);
                            return;
                        }
                    }
                }
            }
            var gameMode = this.toolStripComboBoxGame.SelectedItem as GameMode;
            if (!File.Exists(this.regulationPath + backup_format)) {
                File.Copy(this.regulationPath, this.regulationPath + backup_format);
            }

            if (this.encrypted && !settings.SaveWithoutEncryption) {
                switch (gameMode.Game) {
                    case GameMode.GameType.DarkSouls2:
                        Utility.EncryptDS2Regulation(this.regulationPath, this.regulation as BND4);
                        break;
                    case GameMode.GameType.DarkSouls3:
                        SFUtil.EncryptDS3Regulation(this.regulationPath, this.regulation as BND4);
                        break;
                    case GameMode.GameType.EldenRing:
                        SFUtil.EncryptERRegulation(this.regulationPath, this.regulation as BND4);
                        break;
                    default:
                        Utility.ShowError("Encryption is not valid for this file.");
                        break;
                }
            } else if (this.regulation is BND3 bnd3) {
                bnd3.Write(this.regulationPath);
            } else if (this.regulation is BND4 bnd4) {
                bnd4.Write(this.regulationPath);

            }
            SystemSounds.Asterisk.Play();
        }

        // Token: 0x06000014 RID: 20 RVA: 0x00003378 File Offset: 0x00001578
        private void RestoreToolStripMenuItem_Click(object sender, EventArgs e) {
            if (this.InvalidationMode) {
                return;
            }
            if (File.Exists($"{this.regulationPath}.bak")) {
                if (MessageBox.Show("Are you sure you want to restore the backup?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) {
                    return;
                }
                try {
                    File.Copy($"{this.regulationPath}.bak", this.regulationPath, true);
                    this.BuildParamDefs();
                    this.BuildParamTdfs();
                    this.BuildBoolTypes();
                    this.BuildCustomTypes();
                    this.LoadParams(false);
                    SystemSounds.Asterisk.Play();
                    return;
                } catch (Exception ex) {
                    Utility.ShowError($"""
                        Failed to restore backup
                        
                        {this.regulationPath}.bak
                        
                        {ex}
                        """);
                    return;
                }
            }
            Utility.ShowError($"""
                There is no backup to restore at:
                
                {this.regulationPath}.bak
                """);
        }

        // Token: 0x06000015 RID: 21 RVA: 0x00003444 File Offset: 0x00001644
        private void ExploreToolStripMenuItem_Click(object sender, EventArgs e) {
            try {
                _ = Process.Start("C:/Windows/explorer.exe", Path.GetDirectoryName(this.regulationPath));
            } catch {
                SystemSounds.Hand.Play();
            }
        }

        // Token: 0x06000016 RID: 22 RVA: 0x00003484 File Offset: 0x00001684
        private void ExportToolStripMenuItem_Click(object sender, EventArgs e) {
            if (this.InvalidationMode) {
                return;
            }
            IBinder binder = this.regulation;
            string dir = this.fbdExport.SelectedPath;
            this.fbdExport.SelectedPath = Path.GetDirectoryName(this.regulationPath);
            if (this.fbdExport.ShowDialog() == DialogResult.OK) {
                try {
                    new BND4 {
                        BigEndian = false,
                        Compression = DCX.Type.DCX_DFLT_10000_44_9,
                        Extended = 4,
                        Unk04 = false,
                        Unk05 = false,
                        Format = Format.Names1 | Format.LongOffsets | Format.Compression | Format.Flag6,
                        Unicode = true,
                        Files = this.regulation.Files.Where(f => f.Name.EndsWith(".param")).ToList()
                    }.Write($"{dir}\\gameparam.parambnd.dcx");
                    if ((this.toolStripComboBoxGame.SelectedItem as GameMode).Game == GameMode.GameType.DarkSouls3) {
                        new BND4 {
                            BigEndian = false,
                            Compression = DCX.Type.DCX_DFLT_10000_44_9,
                            Extended = 4,
                            Unk04 = false,
                            Unk05 = false,
                            Format = Format.Names1 | Format.LongOffsets | Format.Compression | Format.Flag6,
                            Unicode = true,
                            Files = this.regulation.Files.Where(f => f.Name.EndsWith(".stayparam")).ToList()
                        }.Write($"{dir}\\stayparam.parambnd.dcx");
                    }
                } catch (Exception ex) {
                    Utility.ShowError($"""
                        Failed to write exported parambnds.
                        
                        {ex}
                        """);
                }
            }
        }

        // Token: 0x06000017 RID: 23 RVA: 0x00003644 File Offset: 0x00001844
        private void ProjectFolderMenuItem_Click(object sender, EventArgs e) {
            try {
                _ = Process.Start("Projects");
            } catch {
                SystemSounds.Hand.Play();
            }
        }

        // Token: 0x06000018 RID: 24 RVA: 0x0000367C File Offset: 0x0000187C
        private void AddRowToolStripMenuItem_Click(object sender, EventArgs e) {
            if (this.InvalidationMode) {
                return;
            }
            _ = this.CreateRow("Add a new row...");
        }

        // Token: 0x06000019 RID: 25 RVA: 0x00003694 File Offset: 0x00001894
        private void DuplicateRowToolStripMenuItem_Click(object sender, EventArgs e) {
            if (this.InvalidationMode) {
                return;
            }
            if (this.dgvRows.SelectedCells.Count == 0) {
                Utility.ShowError("You can't duplicate a row without one selected!");
                return;
            }
            int index = this.dgvRows.SelectedCells[0].RowIndex;
            Row oldRow = (this.rowSource.DataSource as ParamWrapper).Rows[index];
            if (this.rowSource.DataSource == null) {
                Utility.ShowError("You can't create a row with no param selected!");
                return;
            }
            var newRowForm = new NewRow("Duplicate a row...");
            if (newRowForm.ShowDialog() == DialogResult.OK) {
                string name = newRowForm.ResultName;
                int base_id = newRowForm.ResultID;
                int repeat_count = Math.Max(settings.NewRow_RepeatCount, 1);
                int step_value = Math.Max(settings.NewRow_StepValue, 1);
                int current_id = base_id;
                ParamWrapper paramWrapper = this.rowSource.DataSource as ParamWrapper ?? this.ThrowCannotGetParamWrapper<ParamWrapper>();
                Row newRow;
                for (int i = 0; i < repeat_count; i++) {
                    if (paramWrapper.Rows.Any((Row row) => row.ID == current_id)) {
                        Utility.ShowError($"A row with this ID already exists: {current_id}");
                    } else {
                        newRow = new Row(current_id, name, paramWrapper.AppliedParamDef);
                        _ = this.rowSource.Add(newRow);
                        List<Row> rows2 = paramWrapper.Rows;
                        int row_index = rows2.FindIndex((Row row) => row == newRow);
                        int displayedRows = this.dgvRows.DisplayedRowCount(false);
                        this.dgvRows.FirstDisplayedScrollingRowIndex = Math.Max(0, row_index - displayedRows / 2);
                        this.dgvRows.ClearSelection();
                        this.dgvRows.Rows[row_index].Selected = true;
                        this.dgvRows.Refresh();
                        for (int j = 0; j < oldRow.Cells.Count; j++) {
                            newRow.Cells[j].Value = oldRow.Cells[j].Value;
                        }
                    }
                    current_id += step_value;
                }
                paramWrapper.Rows.Sort((r1, r2) => r1.ID - r2.ID);
            }
        }

        // Token: 0x0600001A RID: 26 RVA: 0x00003920 File Offset: 0x00001B20
        private Row CreateRow(string prompt) {
            if (this.rowSource.DataSource == null) {
                Utility.ShowError("You can't create a row with no param selected!");
                return null;
            }
            Row row_result = null;
            var newRowForm = new NewRow(prompt);
            if (newRowForm.ShowDialog() == DialogResult.OK) {
                int id = newRowForm.ResultID;
                string name = newRowForm.ResultName;
                ParamWrapper paramWrapper = this.rowSource.DataSource as ParamWrapper ?? this.ThrowCannotGetParamWrapper<ParamWrapper>();
                if (paramWrapper.Rows.Any(row => row.ID == id)) {
                    Utility.ShowError($"A row with this ID already exists: {id}");
                } else {
                    row_result = new Row(id, name, paramWrapper.AppliedParamDef);
                    _ = this.rowSource.Add(row_result);
                    paramWrapper.Rows.Sort((Row r1, Row r2) => r1.ID.CompareTo(r2.ID));
                    int index = paramWrapper.Rows.FindIndex((Row row) => row == row_result);
                    int displayedRows = this.dgvRows.DisplayedRowCount(false);
                    this.dgvRows.FirstDisplayedScrollingRowIndex = Math.Max(0, index - displayedRows / 2);
                    this.dgvRows.ClearSelection();
                    this.dgvRows.Rows[index].Selected = true;
                    this.dgvRows.Refresh();
                }
            }
            return row_result;
        }

        // Token: 0x0600001B RID: 27 RVA: 0x00003A94 File Offset: 0x00001C94
        private void DeleteRowToolStripMenuItem_Click(object sender, EventArgs e) {
            if (!this.InvalidationMode && this.dgvRows.SelectedCells.Count > 0) {
                DialogResult choice = DialogResult.Yes;
                if (settings.VerifyRowDeletion) {
                    choice = MessageBox.Show("Are you sure you want to delete this row?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                }
                if (choice == DialogResult.Yes) {
                    int rowIndex = this.dgvRows.SelectedCells[0].RowIndex;
                    this.rowSource.RemoveAt(rowIndex);
                    if (rowIndex == this.dgvRows.RowCount) {
                        if (this.dgvRows.RowCount > 0) {
                            this.dgvRows.Rows[this.dgvRows.RowCount - 1].Selected = true;
                            return;
                        }
                        this.dgvCells.DataSource = null;
                    }
                }
            }
        }

        // Token: 0x0600001C RID: 28 RVA: 0x00003B54 File Offset: 0x00001D54
        private void FindRowToolStripMenuItem_Click(object sender, EventArgs e) {
            if (this.InvalidationMode) {
                return;
            }
            var findForm = new FindRow("Find row with name...");
            if (findForm.ShowDialog() == DialogResult.OK) {
                this.FindRow(findForm.ResultPattern);
            }
        }

        // Token: 0x0600001D RID: 29 RVA: 0x00003B8A File Offset: 0x00001D8A
        private void FindNextRowToolStripMenuItem_Click(object sender, EventArgs e) {
            if (!this.InvalidationMode) {
                this.FindRow(this.lastFindRowPattern);
            }
        }

        // Token: 0x0600001E RID: 30 RVA: 0x00003BA4 File Offset: 0x00001DA4
        private void FindRow(string pattern) {
            if (this.InvalidationMode) {
                return;
            }
            if (this.rowSource.DataSource == null) {
                Utility.ShowError("You can't search for a row when there are no rows!");
                return;
            }
            int startIndex = (this.dgvRows.SelectedCells.Count > 0) ? (this.dgvRows.SelectedCells[0].RowIndex + 1) : 0;
            List<Row> rows = (this.rowSource.DataSource as ParamWrapper).Rows;
            int index = -1;
            for (int i = 0; i < rows.Count; i++) {
                if ((rows[(startIndex + i) % rows.Count].Name ?? "").ToLower().Contains(pattern.ToLower())) {
                    index = (startIndex + i) % rows.Count;
                    break;
                }
            }
            if (index != -1) {
                int displayedRows = this.dgvRows.DisplayedRowCount(false);
                this.dgvRows.FirstDisplayedScrollingRowIndex = Math.Max(0, index - displayedRows / 2);
                this.dgvRows.ClearSelection();
                this.dgvRows.Rows[index].Selected = true;
                this.lastFindRowPattern = pattern;
                return;
            }
            Utility.ShowError($"No row found matching: {pattern}");
        }

        // Token: 0x0600001F RID: 31 RVA: 0x00003CCC File Offset: 0x00001ECC
        private void GotoRowToolStripMenuItem_Click(object sender, EventArgs e) {
            if (this.InvalidationMode) {
                return;
            }
            var gotoForm = new GoToRow();
            if (gotoForm.ShowDialog() == DialogResult.OK) {
                if (this.rowSource.DataSource == null) {
                    Utility.ShowError("You can't goto a row when there are no rows!");
                    return;
                }
                long id = gotoForm.ResultID;
                int index = (this.rowSource.DataSource as ParamWrapper).Rows.FindIndex((Row row) => row.ID == id);
                if (index != -1) {
                    int displayedRows = this.dgvRows.DisplayedRowCount(false);
                    this.dgvRows.FirstDisplayedScrollingRowIndex = Math.Max(0, index - displayedRows / 2);
                    this.dgvRows.ClearSelection();
                    this.dgvRows.Rows[index].Selected = true;
                    return;
                }
                Utility.ShowError($"Row ID not found: {id}");
            }
        }

        // Token: 0x06000020 RID: 32 RVA: 0x00003DAC File Offset: 0x00001FAC
        private void FindFieldToolStripMenuItem_Click(object sender, EventArgs e) {
            if (this.InvalidationMode) {
                return;
            }
            var findForm = new FindField("Find field with name...");
            if (findForm.ShowDialog() == DialogResult.OK) {
                this.FindField(findForm.ResultPattern);
            }
        }

        // Token: 0x06000021 RID: 33 RVA: 0x00003DE2 File Offset: 0x00001FE2
        private void FindNextFieldToolStripMenuItem_Click(object sender, EventArgs e) => this.FindField(this.lastFindFieldPattern);

        // Token: 0x06000022 RID: 34 RVA: 0x00003DF0 File Offset: 0x00001FF0
        private void FindField(string pattern) {
            if (!this.InvalidationMode) {
                if (this.dgvCells.DataSource != null) {
                    int startIndex = (this.dgvCells.SelectedCells.Count > 0) ? (this.dgvCells.SelectedCells[0].RowIndex + 1) : 0;
                    var cells = this.dgvCells.DataSource as Cell[];
                    int index = -1;
                    string value = pattern.ToLower();
                    int length = cells.Length;
                    for (int i = 0; i < length; i++) {
                        if (settings.CellView_ShowEditorNames) {
                            if ((cells[(startIndex + i) % length].EditorName.ToString() ?? "").ToLower().Contains(value)) {
                                index = (startIndex + i) % length;
                                break;
                            }
                        } else if ((cells[(startIndex + i) % length].Name.ToString() ?? "").ToLower().Contains(pattern)) {
                            index = (startIndex + i) % length;
                            break;
                        }
                    }
                    if (index != -1) {
                        int displayedRows = this.dgvCells.DisplayedRowCount(false);
                        this.dgvCells.FirstDisplayedScrollingRowIndex = Math.Max(0, index - displayedRows / 2);
                        this.dgvCells.ClearSelection();
                        this.dgvCells.Rows[index].Selected = true;
                        this.lastFindFieldPattern = pattern;
                        return;
                    }
                    Utility.ShowError($"No field found matching: {pattern}");
                } else {
                    Utility.ShowError("You can't search for a field when there are no fields!");
                    return;
                }
            }
        }

        // Token: 0x06000023 RID: 35 RVA: 0x00003F60 File Offset: 0x00002160
        private void importRowNames_Project_MenuItem_Click(object sender, EventArgs e) {
            if (!this.InvalidationMode) {
                bool replace = MessageBox.Show("""
                If a row already has a name, would you like to skip it?
                Click Yes to skip existing names.
                Click No to replace existing names.
                """, "Importing Names", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
                string name_dir = this.GetProjectDirectory("Names");
                foreach (ParamWrapper paramFile in (List<ParamWrapper>)this.dgvParams.DataSource) {
                    if (File.Exists($"{name_dir}\\{paramFile.Name}.txt")) {
                        var names = new Dictionary<long, string>();
                        foreach (string line in SplitWhitespacedLines().Split(File.ReadAllText($"{name_dir}\\{paramFile.Name}.txt"))) {
                            if (line.Length > 0) {
                                Match match = IDRegex().Match(line);
                                names[long.Parse(match.Groups[1].Value)] = match.Groups[2].Value;
                            }
                        }
                        foreach (Row row in paramFile.Param.Rows) {
                            if (names.ContainsKey(row.ID) && (replace || row.Name is null or "")) {
                                row.Name = names[row.ID];
                            }
                        }
                    }
                }
                this.dgvRows.Refresh();
            }
        }

        // Token: 0x06000024 RID: 36 RVA: 0x00004148 File Offset: 0x00002348
        private void importRowNames_Stock_MenuItem_Click(object sender, EventArgs e) {
            if (!this.InvalidationMode) {
                bool replace = MessageBox.Show("""
                If a row already has a name, would you like to skip it?
                Click Yes to skip existing names.
                Click No to replace existing names.
                """, "Importing Names", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
                string name_dir = this.GetParamdexDirectory("Names");
                foreach (ParamWrapper paramFile in (List<ParamWrapper>)this.dgvParams.DataSource) {
                    if (File.Exists($"{name_dir}\\{paramFile.Name}.txt")) {
                        var names = new Dictionary<long, string>();
                        foreach (string line in SplitWhitespacedLines().Split(File.ReadAllText($"{name_dir}\\{paramFile.Name}.txt"))) {
                            if (line.Length > 0) {
                                Match match = IDRegex().Match(line);
                                long id = long.Parse(match.Groups[1].Value);
                                string name = match.Groups[2].Value;
                                names[id] = name;
                            }
                        }
                        foreach (Row row in paramFile.Param.Rows) {
                            if (names.ContainsKey(row.ID) && (replace || row.Name == null || row.Name == "")) {
                                row.Name = names[row.ID];
                            }
                        }
                    }
                }
                this.dgvRows.Refresh();
            }
        }

        // Token: 0x06000025 RID: 37 RVA: 0x00004330 File Offset: 0x00002530
        private void exportRowNames_Project_MenuItem_Click(object sender, EventArgs e) {
            if (!this.InvalidationMode) {
                string name_dir = this.GetProjectDirectory("Names");
                foreach (ParamWrapper paramFile in this.dgvParams.DataSource as List<ParamWrapper>) {
                    var sb = new StringBuilder();
                    foreach (Row row in paramFile.Param.Rows) {
                        string name = row.Name?.Trim() ?? "";
                        if (name != "") {
                            _ = sb.AppendLine($"{row.ID} {name}");
                        }
                    }
                    try {
                        File.WriteAllText($"{name_dir}\\{paramFile.Name}.txt", sb.ToString());
                    } catch (Exception ex) {
                        Utility.ShowError($"""
                            Failed to write name file: {paramFile.Name}.txt
                            
                            {ex}
                            """);
                        break;
                    }
                }
                if (!settings.ShowConfirmationMessages) {
                    _ = MessageBox.Show("Names exported!", "Export Names", MessageBoxButtons.OK);
                }
            }
        }

        // Token: 0x06000026 RID: 38 RVA: 0x00004498 File Offset: 0x00002698
        private void importDataMenuItem_Click(object sender, EventArgs e) {
            if (this.InvalidationMode) {
                return;
            }
            this.ImportParamData(this.dgvParams.CurrentRow.DataBoundItem as ParamWrapper, false);
        }

        // Token: 0x06000027 RID: 39 RVA: 0x000044CC File Offset: 0x000026CC
        private void exportDataMenuItem_Click(object sender, EventArgs e) {
            if (this.InvalidationMode) {
                return;
            }
            this.ExportParamData(this.dgvParams.CurrentRow.DataBoundItem as ParamWrapper, false);
        }

        // Token: 0x06000028 RID: 40 RVA: 0x00004500 File Offset: 0x00002700
        private void fieldExporterMenuItem_Click(object sender, EventArgs e) {
            if (this.InvalidationMode) {
                return;
            }
            if (new FieldExporter().ShowDialog() == DialogResult.OK) {
                ParamWrapper paramWrapper = this.rowSource.DataSource as ParamWrapper ?? this.ThrowCannotGetParamWrapper<ParamWrapper>();
                string field_data_path = $"{this.GetProjectDirectory("Logs")}\\\\FieldValue_{paramWrapper.Name}.log";
                string exportDelimiter = settings.ExportDelimiter;
                char delimiter = settings.ExportDelimiter.ToCharArray()[0];
                string field_match = settings.FieldExporter_FieldMatch;
                string field_minimum = settings.FieldExporter_FieldMinimum;
                string field_maximum = settings.FieldExporter_FieldMaximum;
                string[] field_inclusions_list = settings.FieldExporter_FieldInclusion.Split(delimiter);
                string[] field_exclusions_list = settings.FieldExporter_FieldExclusion.Split(delimiter);
                bool show_editor_names = settings.CellView_ShowEditorNames;
                if (field_match == "") {
                    _ = MessageBox.Show("You did not specify any field names.", "Field Exporter", MessageBoxButtons.OK);
                    return;
                }
                if (File.Exists(field_data_path)) {
                    if (MessageBox.Show($"{field_data_path} exists. Overwrite?", "Export Field Values", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No) {
                        return;
                    }
                } else if (!File.Exists(field_data_path)) {
                    using FileStream fs = File.Create(field_data_path);
                }
                var output_file = new StreamWriter(field_data_path);
                if (settings.IncludeHeaderInCSV) {
                    output_file.WriteLine($"Row ID{exportDelimiter}Row Name{exportDelimiter}{field_match}");
                }
                var unique_list = new List<string>();
                foreach (Row row in paramWrapper.Rows) {
                    string row_line = $"{row.ID}{exportDelimiter}{(settings.IncludeRowNameInCSV ? row.Name + exportDelimiter : "")}";
                    bool isValidRow = false;
                    foreach (Cell cell in row.Cells) {
                        PARAMDEF.DefType type = cell.Def.DisplayType;
                        string value = cell.Value.ToString();
                        if (
                            type != PARAMDEF.DefType.dummy8 &&
                            field_match == (show_editor_names ? cell.Def.DisplayName : cell.Def.InternalName) &&
                            (field_minimum == "" || type switch {
                                PARAMDEF.DefType.s8 => Convert.ToSByte(value) >= Convert.ToSByte(field_minimum),
                                PARAMDEF.DefType.u8 => Convert.ToByte(value) >= Convert.ToByte(field_minimum),
                                PARAMDEF.DefType.s16 => Convert.ToInt16(value) >= Convert.ToInt16(field_minimum),
                                PARAMDEF.DefType.u16 => Convert.ToUInt16(value) >= Convert.ToUInt16(field_minimum),
                                PARAMDEF.DefType.s32 => Convert.ToInt32(value) >= Convert.ToInt32(field_minimum),
                                PARAMDEF.DefType.u32 => Convert.ToUInt32(value) >= Convert.ToUInt32(field_minimum),
                                PARAMDEF.DefType.f32 => Convert.ToSingle(value) >= Convert.ToSingle(field_minimum),
                                _ => true,
                            }) && (field_maximum == "" || type switch {
                                PARAMDEF.DefType.s8 => Convert.ToSByte(value) <= Convert.ToSByte(field_maximum),
                                PARAMDEF.DefType.u8 => Convert.ToByte(value) <= Convert.ToByte(field_maximum),
                                PARAMDEF.DefType.s16 => Convert.ToInt16(value) <= Convert.ToInt16(field_maximum),
                                PARAMDEF.DefType.u16 => Convert.ToUInt16(value) <= Convert.ToUInt16(field_maximum),
                                PARAMDEF.DefType.s32 => Convert.ToInt32(value) <= Convert.ToInt32(field_maximum),
                                PARAMDEF.DefType.u32 => Convert.ToUInt32(value) <= Convert.ToUInt32(field_maximum),
                                PARAMDEF.DefType.f32 => Convert.ToSingle(value) <= Convert.ToSingle(field_maximum),
                                _ => true,
                            }) && (field_inclusions_list.Length == 0 || type switch {
                                PARAMDEF.DefType.s8 => field_inclusions_list.Select(sbyte.Parse).Contains(Convert.ToSByte(value)),
                                PARAMDEF.DefType.u8 => field_inclusions_list.Select(byte.Parse).Contains(Convert.ToByte(value)),
                                PARAMDEF.DefType.s16 => field_inclusions_list.Select(short.Parse).Contains(Convert.ToInt16(value)),
                                PARAMDEF.DefType.u16 => field_inclusions_list.Select(ushort.Parse).Contains(Convert.ToUInt16(value)),
                                PARAMDEF.DefType.s32 => field_inclusions_list.Select(int.Parse).Contains(Convert.ToInt32(value)),
                                PARAMDEF.DefType.u32 => field_inclusions_list.Select(uint.Parse).Contains(Convert.ToUInt32(value)),
                                PARAMDEF.DefType.f32 => field_inclusions_list.Select(float.Parse).Contains(Convert.ToSingle(value)),
                                _ => true,
                            }) && (field_exclusions_list.Length == 0 || type switch {
                                PARAMDEF.DefType.s8 => !field_exclusions_list.Select(sbyte.Parse).Contains(Convert.ToSByte(value)),
                                PARAMDEF.DefType.u8 => !field_exclusions_list.Select(byte.Parse).Contains(Convert.ToByte(value)),
                                PARAMDEF.DefType.s16 => !field_exclusions_list.Select(short.Parse).Contains(Convert.ToInt16(value)),
                                PARAMDEF.DefType.u16 => !field_exclusions_list.Select(ushort.Parse).Contains(Convert.ToUInt16(value)),
                                PARAMDEF.DefType.s32 => !field_exclusions_list.Select(int.Parse).Contains(Convert.ToInt32(value)),
                                PARAMDEF.DefType.u32 => !field_exclusions_list.Select(uint.Parse).Contains(Convert.ToUInt32(value)),
                                PARAMDEF.DefType.f32 => !field_exclusions_list.Select(float.Parse).Contains(Convert.ToSingle(value)),
                                _ => true,
                            }) && (!settings.ExportUniqueOnly || unique_list.AddNoReplacement(value))
                        ) {
                            isValidRow = true;
                            row_line += type switch {
                                PARAMDEF.DefType.s8 => Convert.ToSByte(value).ToString(),
                                PARAMDEF.DefType.u8 => Convert.ToByte(value).ToString(),
                                PARAMDEF.DefType.s16 => Convert.ToInt16(value).ToString(),
                                PARAMDEF.DefType.u16 => Convert.ToUInt16(value).ToString(),
                                PARAMDEF.DefType.s32 => Convert.ToInt32(value).ToString(),
                                PARAMDEF.DefType.u32 => Convert.ToUInt32(value).ToString(),
                                PARAMDEF.DefType.f32 => Convert.ToSingle(value).ToString(),
                                PARAMDEF.DefType.fixstr or PARAMDEF.DefType.fixstrW => value,
                                _ => "",
                            };
                        }
                    }
                    if (isValidRow) {
                        output_file.WriteLine(row_line);
                    }
                }
                output_file.Close();
                StartTextEditorIfNecessary(field_data_path);
                if (!settings.ShowConfirmationMessages) {
                    _ = MessageBox.Show($"Field values exported to {field_data_path}", "Field Exporter", MessageBoxButtons.OK);
                }
            }
        }

        // Token: 0x06000029 RID: 41 RVA: 0x0000517C File Offset: 0x0000337C
        private void rowReferenceFinderMenuItem_Click(object sender, EventArgs e) {
            if (this.InvalidationMode) {
                return;
            }
            var newFormReferenceFinder = new RowReferenceSearch();
            if (newFormReferenceFinder.ShowDialog() == DialogResult.OK) {
                string reference_text = newFormReferenceFinder.GetReferenceText();
                if (reference_text == "") {
                    _ = MessageBox.Show("You did not specify a value.", "Reference Finder", MessageBoxButtons.OK);
                    return;
                }
                string reference_file_path = $"{this.GetProjectDirectory("Logs")}\\\\RowReference.log";
                var output_file = new StreamWriter(reference_file_path);
                foreach (ParamWrapper param in this.primary_result.ParamWrappers) {
                    foreach (Row row in param.Rows) {
                        foreach (Cell cell in row.Cells) {
                            Unsafe.SkipInit(out int value_2);
                            bool threw = false;
                            try {
                                value_2 = Convert.ToInt32(cell.Value);
                            } catch {
                                threw = true;
                            }
                            if (!threw && CheckFieldReference(reference_text, value_2)) {
                                output_file.WriteLine(param.Name);
                                if (row.Name != null) {
                                    output_file.WriteLine($"  - Row Name: {row.Name}");
                                }
                                if (cell.Def?.ToString() != null) {
                                    output_file.WriteLine($"  - Cell Name: {cell.Def}");
                                }
                                output_file.WriteLine($"  - Row ID: {row.ID}");
                                output_file.WriteLine($"  - Reference Value: {reference_text}");
                                output_file.WriteLine("");
                            }
                        }
                    }
                }
                output_file.Close();
                StartTextEditorIfNecessary(reference_file_path);
                if (!settings.ShowConfirmationMessages) {
                    _ = MessageBox.Show($"References exported to {reference_file_path}", "Reference Finder", MessageBoxButtons.OK);
                }
            }
        }

        // Token: 0x0600002A RID: 42 RVA: 0x0000545C File Offset: 0x0000365C
        private void valueReferenceFinderMenuItem_Click(object sender, EventArgs e) {
            if (this.InvalidationMode) {
                return;
            }
            var newFormReferenceFinder = new FieldReferenceSearch();
            if (newFormReferenceFinder.ShowDialog() == DialogResult.OK) {
                string reference_text = newFormReferenceFinder.GetReferenceText();
                if (reference_text == "") {
                    _ = MessageBox.Show("You did not specify a value.", "Reference Finder", MessageBoxButtons.OK);
                    return;
                }
                string reference_file_path = $"{this.GetProjectDirectory("Logs")}\\\\ValueReference.log";
                using (var output_file = new StreamWriter(reference_file_path)) {
                    foreach (ParamWrapper param in this.primary_result.ParamWrappers) {
                        foreach (Row row in param.Rows) {
                            if (CheckFieldReference(reference_text, row.ID)) {
                                output_file.WriteLine(param.Name.ToString());
                                if (row.Name != null) {
                                    output_file.WriteLine($"  - Row Name: {row.Name}");
                                }
                                output_file.WriteLine($"  - Row ID: {row.ID}");
                                output_file.WriteLine($"  - Reference Value: {reference_text}");
                                output_file.WriteLine("");
                            }
                        }
                    }
                }
                StartTextEditorIfNecessary(reference_file_path);
                if (!settings.ShowConfirmationMessages) {
                    _ = MessageBox.Show($"References exported to {reference_file_path}", "Reference Finder", MessageBoxButtons.OK);
                }
            }
        }

        // Token: 0x0600002B RID: 43 RVA: 0x000056A0 File Offset: 0x000038A0
        private static bool CheckFieldReference(string value_1, string value_2) => int.TryParse(value_1, out int v1) && int.TryParse(value_2, out int v2) && v1 == v2;
        private static bool CheckFieldReference(string value_1, int value_2) => int.TryParse(value_1, out int v1) && v1 == value_2;

        // Token: 0x0600002C RID: 44 RVA: 0x000056E0 File Offset: 0x000038E0
        private void massImportDataMenuItem_Click(object sender, EventArgs e) => _ = this.InvalidationMode
                || MessageBox.Show("Mass Import will import data from CSV files for all params. Continue?",
                    "Mass Import", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No
                || this.primary_result == null || MessageBox.Show($"Mass data import complete in {Globals.MeasureTimeSpent(() =>
                this.primary_result.ParamWrappers.ForEach(wrapper => this.ImportParamData(wrapper, true)), new())} ms!", "Mass Import") <= DialogResult.None;

        // Token: 0x0600002D RID: 45 RVA: 0x00005770 File Offset: 0x00003970
        private void massExportDataMenuItem_Click(object sender, EventArgs e) => _ = this.InvalidationMode ||
                MessageBox.Show("Mass Export will export all params to CSV. Continue?", "Mass Export", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No ||
                !settings.IncludeRowNameInCSV && MessageBox.Show("Row Names are currently not included. Continue?", "Mass Export", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No
                || this.primary_result != null && MessageBox.Show($"Mass data export complete in {Globals.MeasureTimeSpent(() => this.primary_result.ParamWrappers.ForEach(wrapper => this.ExportParamData(wrapper, true)), new())} ms!", "Data Export") > DialogResult.None;

        // Token: 0x0600002E RID: 46 RVA: 0x00005824 File Offset: 0x00003A24
        private void fieldAdjusterMenuItem_Click(object sender, EventArgs e) {
            if (this.InvalidationMode) {
                return;
            }
            if (new FieldAdjuster().ShowDialog() == DialogResult.OK) {
                this.SaveParams(".bak");
                ParamWrapper paramWrapper = this.rowSource.DataSource as ParamWrapper ?? this.ThrowCannotGetParamWrapper<ParamWrapper>();
                string adjustment_file_path = $"{this.GetProjectDirectory("Logs")}\\\\FieldAdjustment_{paramWrapper.Name}.log";
                char delimiter = settings.ExportDelimiter.ToCharArray()[0];
                string field_match = settings.FieldAdjuster_FieldMatch;
                string row_range = settings.FieldAdjuster_RowRange;
                string row_partial_match = settings.FieldAdjuster_RowPartialMatch;
                string field_minimum = settings.FieldAdjuster_FieldMinimum;
                string field_maximum = settings.FieldAdjuster_FieldMaximum;
                string field_formula = settings.FieldAdjuster_Formula;
                string output_max = settings.FieldAdjuster_ValueMax;
                string output_min = settings.FieldAdjuster_ValueMin;
                string[] field_inclusions_list = settings.FieldExporter_FieldInclusion.Split(delimiter);
                string[] field_exclusions_list = settings.FieldExporter_FieldExclusion.Split(delimiter);
                string[] row_range_array = row_range.Split(delimiter);
                if (row_range_array.Length != 2) {
                    _ = MessageBox.Show("Row range invalid.", "Field Adjuster", MessageBoxButtons.OK);
                    return;
                }
                int row_range_array_0 = 0;
                int row_range_array_1 = int.MaxValue;
                bool has_row_range_array_0 = NumberRegex().Matches(row_range_array[0]).Count == 1 && int.TryParse(row_range_array[0], out row_range_array_0);
                bool has_row_range_array_1 = NumberRegex().Matches(row_range_array[1]).Count == 1 && int.TryParse(row_range_array[1], out row_range_array_1);
                bool show_editor_names = settings.CellView_ShowEditorNames;
                if (field_match == "") {
                    _ = MessageBox.Show("You did not specify a target field.", "Field Adjuster", MessageBoxButtons.OK);
                    return;
                }
                if (field_formula == "") {
                    _ = MessageBox.Show("You did not specify a field formula.", "Field Adjuster", MessageBoxButtons.OK);
                    return;
                }
                using (var output_file = new StreamWriter(adjustment_file_path)) {
                    foreach (Row row in paramWrapper.Rows) {
                        if ((row_range == "" || row.ID >= row_range_array_0 && row.ID <= row_range_array_1) && (row_partial_match == "" || row.ID.ToString()[^row_partial_match.Length..] == row_partial_match)) {
                            foreach (Cell cell in row.Cells) {
                                PARAMDEF.DefType type = cell.Def.DisplayType;
                                string display_name = cell.Def.DisplayName;
                                string internal_name = cell.Def.InternalName;
                                string value = cell.Value.ToString();
                                // if (type != PARAMDEF.DefType.dummy8 && (settings.CellView_ShowEditorNames && field_match == display_name || !settings.CellView_ShowEditorNames && field_match == internal_name)) {
                                // bool isMatchedField = true;
                                // //_ = $"Entered value: {value} is invalid for [{cell.Name}].";
                                // if (field_minimum != "") {
                                //     switch (type) {
                                //         case PARAMDEF.DefType.s8 when Convert.ToSByte(value) < Convert.ToSByte(field_minimum):
                                //         case PARAMDEF.DefType.u8 when Convert.ToByte(value) < Convert.ToByte(field_minimum):
                                //         case PARAMDEF.DefType.s16 when Convert.ToInt16(value) < Convert.ToInt16(field_minimum):
                                //         case PARAMDEF.DefType.u16 when Convert.ToUInt16(value) < Convert.ToUInt16(field_minimum):
                                //         case PARAMDEF.DefType.s32 when Convert.ToInt32(value) < Convert.ToInt32(field_minimum):
                                //         case PARAMDEF.DefType.u32 when Convert.ToUInt32(value) < Convert.ToUInt32(field_minimum):
                                //         case PARAMDEF.DefType.f32 when Convert.ToSingle(value) < Convert.ToSingle(field_minimum):
                                //             isMatchedField = false;
                                //             break;
                                //     }
                                // }
                                // if (field_maximum != "") {
                                //     switch (type) {
                                //         case PARAMDEF.DefType.s8 when Convert.ToSByte(value) > Convert.ToSByte(field_maximum):
                                //         case PARAMDEF.DefType.u8 when Convert.ToByte(value) > Convert.ToByte(field_maximum):
                                //         case PARAMDEF.DefType.s16 when Convert.ToInt16(value) > Convert.ToInt16(field_maximum):
                                //         case PARAMDEF.DefType.u16 when Convert.ToUInt16(value) > Convert.ToUInt16(field_maximum):
                                //         case PARAMDEF.DefType.s32 when Convert.ToInt32(value) > Convert.ToInt32(field_maximum):
                                //         case PARAMDEF.DefType.u32 when Convert.ToUInt32(value) > Convert.ToUInt32(field_maximum):
                                //         case PARAMDEF.DefType.f32 when Convert.ToSingle(value) > Convert.ToSingle(field_maximum):
                                //             isMatchedField = false;
                                //             break;
                                //     }
                                // }
                                // if (field_inclusions != "") {
                                //     if (type == PARAMDEF.DefType.s8) {
                                //         foreach (sbyte array_value in Array.ConvertAll(field_inclusions.Split(delimiter), sbyte.Parse)) {
                                //             if (Convert.ToSByte(value) != array_value) {
                                //                 isMatchedField = false;
                                //             }
                                //         }
                                //     } else if (type == PARAMDEF.DefType.u8) {
                                //         foreach (byte array_value2 in Array.ConvertAll(field_inclusions.Split(delimiter), byte.Parse)) {
                                //             if (Convert.ToByte(value) != array_value2) {
                                //                 isMatchedField = false;
                                //             }
                                //         }
                                //     } else if (type == PARAMDEF.DefType.s16) {
                                //         foreach (short array_value3 in Array.ConvertAll(field_inclusions.Split(delimiter), short.Parse)) {
                                //             if (Convert.ToInt16(value) != array_value3) {
                                //                 isMatchedField = false;
                                //             }
                                //         }
                                //     } else if (type == PARAMDEF.DefType.u16) {
                                //         foreach (ushort array_value4 in Array.ConvertAll(field_inclusions.Split(delimiter), ushort.Parse)) {
                                //             if (Convert.ToUInt16(value) != array_value4) {
                                //                 isMatchedField = false;
                                //             }
                                //         }
                                //     } else if (type == PARAMDEF.DefType.s32) {
                                //         foreach (int array_value5 in Array.ConvertAll(field_inclusions.Split(delimiter), int.Parse)) {
                                //             if (Convert.ToInt32(value) != array_value5) {
                                //                 isMatchedField = false;
                                //             }
                                //         }
                                //     } else if (type == PARAMDEF.DefType.u32) {
                                //         foreach (uint array_value6 in Array.ConvertAll(field_inclusions.Split(delimiter), uint.Parse)) {
                                //             if (Convert.ToUInt32(value) != array_value6) {
                                //                 isMatchedField = false;
                                //             }
                                //         }
                                //     } else if (type == PARAMDEF.DefType.f32) {
                                //         foreach (float array_value7 in Array.ConvertAll(field_inclusions.Split(delimiter), float.Parse)) {
                                //             if (Convert.ToSingle(value) != array_value7) {
                                //                 isMatchedField = false;
                                //             }
                                //         }
                                //     }
                                // }
                                // if (field_exclusions != "") {
                                //     if (type == PARAMDEF.DefType.s8) {
                                //         foreach (sbyte array_value8 in Array.ConvertAll(field_exclusions.Split(delimiter), sbyte.Parse)) {
                                //             if (Convert.ToSByte(value) == array_value8) {
                                //                 isMatchedField = false;
                                //             }
                                //         }
                                //     } else if (type == PARAMDEF.DefType.u8) {
                                //         foreach (byte array_value9 in Array.ConvertAll(field_exclusions.Split(delimiter), byte.Parse)) {
                                //             if (Convert.ToByte(value) == array_value9) {
                                //                 isMatchedField = false;
                                //             }
                                //         }
                                //     } else if (type == PARAMDEF.DefType.s16) {
                                //         foreach (short array_value10 in Array.ConvertAll(field_exclusions.Split(delimiter), short.Parse)) {
                                //             if (Convert.ToInt16(value) == array_value10) {
                                //                 isMatchedField = false;
                                //             }
                                //         }
                                //     } else if (type == PARAMDEF.DefType.u16) {
                                //         foreach (ushort array_value11 in Array.ConvertAll(field_exclusions.Split(delimiter), ushort.Parse)) {
                                //             if (Convert.ToUInt16(value) == array_value11) {
                                //                 isMatchedField = false;
                                //             }
                                //         }
                                //     } else if (type == PARAMDEF.DefType.s32) {
                                //         foreach (int array_value12 in Array.ConvertAll(field_exclusions.Split(delimiter), int.Parse)) {
                                //             if (Convert.ToInt32(value) == array_value12) {
                                //                 isMatchedField = false;
                                //             }
                                //         }
                                //     } else if (type == PARAMDEF.DefType.u32) {
                                //         foreach (uint array_value13 in Array.ConvertAll(field_exclusions.Split(delimiter), uint.Parse)) {
                                //             if (Convert.ToUInt32(value) == array_value13) {
                                //                 isMatchedField = false;
                                //             }
                                //         }
                                //     } else if (type == PARAMDEF.DefType.f32) {
                                //         foreach (float array_value14 in Array.ConvertAll(field_exclusions.Split(delimiter), float.Parse)) {
                                //             if (Convert.ToSingle(value) == array_value14) {
                                //                 isMatchedField = false;
                                //             }
                                //         }
                                //     }
                                // }
                                if (
                                    type != PARAMDEF.DefType.dummy8 &&
                                    field_match == (show_editor_names ? cell.Def.DisplayName : cell.Def.InternalName) &&
                                    (field_minimum == "" || type switch {
                                        PARAMDEF.DefType.s8 => Convert.ToSByte(value) >= Convert.ToSByte(field_minimum),
                                        PARAMDEF.DefType.u8 => Convert.ToByte(value) >= Convert.ToByte(field_minimum),
                                        PARAMDEF.DefType.s16 => Convert.ToInt16(value) >= Convert.ToInt16(field_minimum),
                                        PARAMDEF.DefType.u16 => Convert.ToUInt16(value) >= Convert.ToUInt16(field_minimum),
                                        PARAMDEF.DefType.s32 => Convert.ToInt32(value) >= Convert.ToInt32(field_minimum),
                                        PARAMDEF.DefType.u32 => Convert.ToUInt32(value) >= Convert.ToUInt32(field_minimum),
                                        PARAMDEF.DefType.f32 => Convert.ToSingle(value) >= Convert.ToSingle(field_minimum),
                                        _ => true,
                                    }) && (field_maximum == "" || type switch {
                                        PARAMDEF.DefType.s8 => Convert.ToSByte(value) <= Convert.ToSByte(field_maximum),
                                        PARAMDEF.DefType.u8 => Convert.ToByte(value) <= Convert.ToByte(field_maximum),
                                        PARAMDEF.DefType.s16 => Convert.ToInt16(value) <= Convert.ToInt16(field_maximum),
                                        PARAMDEF.DefType.u16 => Convert.ToUInt16(value) <= Convert.ToUInt16(field_maximum),
                                        PARAMDEF.DefType.s32 => Convert.ToInt32(value) <= Convert.ToInt32(field_maximum),
                                        PARAMDEF.DefType.u32 => Convert.ToUInt32(value) <= Convert.ToUInt32(field_maximum),
                                        PARAMDEF.DefType.f32 => Convert.ToSingle(value) <= Convert.ToSingle(field_maximum),
                                        _ => true,
                                    }) && (field_inclusions_list.Length == 0 || type switch {
                                        PARAMDEF.DefType.s8 => field_inclusions_list.Select(sbyte.Parse).Contains(Convert.ToSByte(value)),
                                        PARAMDEF.DefType.u8 => field_inclusions_list.Select(byte.Parse).Contains(Convert.ToByte(value)),
                                        PARAMDEF.DefType.s16 => field_inclusions_list.Select(short.Parse).Contains(Convert.ToInt16(value)),
                                        PARAMDEF.DefType.u16 => field_inclusions_list.Select(ushort.Parse).Contains(Convert.ToUInt16(value)),
                                        PARAMDEF.DefType.s32 => field_inclusions_list.Select(int.Parse).Contains(Convert.ToInt32(value)),
                                        PARAMDEF.DefType.u32 => field_inclusions_list.Select(uint.Parse).Contains(Convert.ToUInt32(value)),
                                        PARAMDEF.DefType.f32 => field_inclusions_list.Select(float.Parse).Contains(Convert.ToSingle(value)),
                                        _ => true,
                                    }) && (field_exclusions_list.Length == 0 || type switch {
                                        PARAMDEF.DefType.s8 => !field_exclusions_list.Select(sbyte.Parse).Contains(Convert.ToSByte(value)),
                                        PARAMDEF.DefType.u8 => !field_exclusions_list.Select(byte.Parse).Contains(Convert.ToByte(value)),
                                        PARAMDEF.DefType.s16 => !field_exclusions_list.Select(short.Parse).Contains(Convert.ToInt16(value)),
                                        PARAMDEF.DefType.u16 => !field_exclusions_list.Select(ushort.Parse).Contains(Convert.ToUInt16(value)),
                                        PARAMDEF.DefType.s32 => !field_exclusions_list.Select(int.Parse).Contains(Convert.ToInt32(value)),
                                        PARAMDEF.DefType.u32 => !field_exclusions_list.Select(uint.Parse).Contains(Convert.ToUInt32(value)),
                                        PARAMDEF.DefType.f32 => !field_exclusions_list.Select(float.Parse).Contains(Convert.ToSingle(value)),
                                        _ => true,
                                    })
                                ) {
                                    output_file.WriteLine("Row: " + row.ID.ToString());
                                    output_file.WriteLine("- Field " + cell.Def.InternalName.ToString());
                                    output_file.WriteLine("- Old Value " + cell.Value.ToString());
                                    decimal field_result;
                                    if (field_formula.Contains('x')) {
                                        string cell_formula = field_formula.Replace("x", cell.Value.ToString());
                                        field_result = new StringToFormula().Eval(cell_formula);
                                        if (output_max != "" && field_result > decimal.Parse(output_max)) {
                                            field_result = decimal.Parse(output_max);
                                        }
                                        if (output_min != "" && field_result < decimal.Parse(output_min)) {
                                            field_result = decimal.Parse(output_min);
                                        }
                                    } else {
                                        field_result = decimal.Parse(field_formula);
                                    }
                                    if (type == PARAMDEF.DefType.s8) {
                                        cell.Value = Convert.ToSByte(field_result);
                                    } else if (type == PARAMDEF.DefType.u8) {
                                        cell.Value = Convert.ToByte(field_result);
                                    } else if (type == PARAMDEF.DefType.s16) {
                                        cell.Value = Convert.ToInt16(field_result);
                                    } else if (type == PARAMDEF.DefType.u16) {
                                        cell.Value = Convert.ToUInt16(field_result);
                                    } else if (type == PARAMDEF.DefType.s32) {
                                        cell.Value = Convert.ToInt32(field_result);
                                    } else if (type == PARAMDEF.DefType.u32) {
                                        cell.Value = Convert.ToUInt32(field_result);
                                    } else if (type == PARAMDEF.DefType.f32) {
                                        cell.Value = Convert.ToSingle(field_result);
                                    }
                                    output_file.WriteLine("- New Value " + cell.Value.ToString());
                                    output_file.WriteLine("");
                                }
                                // }
                            }
                        }
                    }
                }

                StartTextEditorIfNecessary(adjustment_file_path);
                if (!settings.ShowConfirmationMessages) {
                    _ = MessageBox.Show("Field Adjustment complete.", "Field Adjuster", MessageBoxButtons.OK);
                }
            }
        }

        [DoesNotReturn]
        private T ThrowCannotGetParamWrapper<T>() => throw new NullReferenceException($"{nameof(this.rowSource.DataSource)} Row source has null DataSource");

        private static void StartTextEditorIfNecessary(string file_path) {
            if (settings.UseTextEditor && settings.TextEditorPath != "") {
                try {
                    _ = Process.Start($"\"{settings.TextEditorPath}\"", $"\"{Application.StartupPath}\\{file_path}\"");
                } catch {
                    SystemSounds.Hand.Play();
                }
            }
        }

        // Token: 0x0600002F RID: 47 RVA: 0x000065D0 File Offset: 0x000047D0
        private void affinityGeneratorMenuItem_Click(object sender, EventArgs e) {
            if (this.InvalidationMode) {
                return;
            }
            DataGridViewRow currentRow = this.dgvParams.CurrentRow;
            AffinityGeneration.GenerateAffinityRows(currentRow, currentRow.DataBoundItem as ParamWrapper, this.dgvRows, this.toolStripComboBoxGame.SelectedItem as GameMode);
        }

        private const char NullCharacter = '\0';
        public readonly struct PointerKeeper<T> {
            private readonly nuint _value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe PointerKeeper(T* p) => _value = AsManaged<nuint>(p);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe PointerKeeper(ref T p) => _value = (nuint)AsPointer(ref p);
            public unsafe T Value {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => *(T*)this._value;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => *(T*)this._value = value;
            }
            public unsafe T this[ulong index] {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ((T*)this._value)[index];
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => ((T*)this._value)[index] = value;
            }
            public unsafe T this[long index] {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ((T*)this._value)[index];
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => ((T*)this._value)[index] = value;
            }
            public unsafe T this[uint index] {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ((T*)this._value)[index];
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => ((T*)this._value)[index] = value;
            }
            public unsafe T this[int index] {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ((T*)this._value)[index];
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => ((T*)this._value)[index] = value;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static unsafe implicit operator PointerKeeper<T>(T* reference) => new(reference);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static unsafe implicit operator T*(PointerKeeper<T> keeper) => (T*)keeper._value;
        }
        // Token: 0x06000030 RID: 48 RVA: 0x0000661C File Offset: 0x0000481C
        [SkipLocalsInit]
        private unsafe void ImportParamData(ParamWrapper wrapper, bool isSilent) {
            string paramName = wrapper.Name;
            string paramPath = $"{this.GetProjectDirectory("CSV")}\\{paramName}.csv";
            if (!File.Exists(paramPath)) {
                if (!isSilent) {
                    _ = MessageBox.Show($"{paramPath} does not exist.", "Import Data");
                }
                return;
            }
            if (!isSilent && MessageBox.Show($"Importing will overwrite {paramName} data. Continue?", "Import Data", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No) {
                return;
            }
            StreamReader reader = null;
            try {
                reader = new StreamReader(File.Open(paramPath, new FileStreamOptions {
                    Access = FileAccess.Read,
                    BufferSize = 0x1000,
                    Mode = FileMode.Open,
                    Options = FileOptions.RandomAccess
                }), null, detectEncodingFromByteOrderMarks: true, 0x1000);
            } catch (Exception ex) {
                Utility.ShowError($"""
                    Failed to open {paramPath}.
                    
                    {ex}
                    """);
                return;
            }
            _ = reader.ReadLine();
            List<Row> rows = wrapper.Rows;
            Row[] rows_array = rows.AsContents();
            ulong cells_length = (ulong)(rows[0].Cells as Cell[]).Length + 2 << 3;
            ulong rows_length = (ulong)rows.Count + 2 << 3;
            string export_delimeter = settings.ExportDelimiter;
            bool includeRowNameInCSV = settings.IncludeRowNameInCSV;
            ulong i = 16;
            int export_delimeter_length = export_delimeter.Length;
            PARAMDEF.Field[] fields = wrapper.AppliedParamDef.Fields.AsContents();
            int fields_count = wrapper.AppliedParamDef.Fields.GetLength();
            ulong* cells_filtered = stackalloc ulong[fields_count];
            int cells_filtered_length = 0;
            for (int index = 0; index < fields_count; index++) {
                if (fields[index].DisplayType != PARAMDEF.DefType.dummy8) {
                    cells_filtered[cells_filtered_length++] = 2 + (ulong)index << 3;
                }
            }
            cells_filtered_length <<= 3;
            while (!reader.EndOfStream) {
                string line = reader.ReadLine();
                fixed (char* p = line) {
                    var splitter = new IterativeStringSplitterSmartVectorized(new ReadOnlySpan<char>(p, line.Length), export_delimeter);
                    int id = int.Parse(splitter.Next());
                    Row newRow;
                    if (rows_length > i) {
                        newRow = rows_array.At(i);
                        newRow.ID = id;
                        newRow.Name = includeRowNameInCSV ? splitter.NextString() : string.Empty;
                        i += 8;
                    } else {
                        newRow = rows.EchoAdd(new Row(id, includeRowNameInCSV ? splitter.NextString() : string.Empty, wrapper.AppliedParamDef));
                    }
                    // foreach (Cell cell in newRow.Cells) {
                    //     if (cell.Def.DisplayType != PARAMDEF.DefType.dummy8) {
                    //         try {
                    //             cell.Value = values[cell_index];
                    //             // if (type == PARAMDEF.DefType.u8) {
                    //             //     cell.Value = Convert.ToByte(values[cell_index], CultureInfo.InvariantCulture);
                    //             // }
                    //             // if (type == PARAMDEF.DefType.s16) {
                    //             //     cell.Value = Convert.ToInt16(values[cell_index], CultureInfo.InvariantCulture);
                    //             // }
                    //             // if (type == PARAMDEF.DefType.u16) {
                    //             //     cell.Value = Convert.ToUInt16(values[cell_index], CultureInfo.InvariantCulture);
                    //             // }
                    //             // if (type == PARAMDEF.DefType.s32) {
                    //             //     cell.Value = Convert.ToInt32(values[cell_index], CultureInfo.InvariantCulture);
                    //             // }
                    //             // if (type == PARAMDEF.DefType.u32) {
                    //             //     cell.Value = Convert.ToUInt32(values[cell_index], CultureInfo.InvariantCulture);
                    //             // }
                    //             // if (type == PARAMDEF.DefType.f32) {
                    //             //     cell.Value = Convert.ToSingle(values[cell_index]);
                    //             // }
                    //             // if (type == PARAMDEF.DefType.fixstr || type == PARAMDEF.DefType.fixstrW) {
                    //             //     cell.Value = Convert.ToString(values[cell_index]);
                    //             // }
                    //         } catch {
                    //             _ = MessageBox.Show($"Row {newRow.ID}, Field {cell.Name} has invalid value {values[cell_index]}, skipped import of this value.", "Data Import");
                    //         }
                    //         cell_index++;
                    //     }
                    // }
                    Cell[] cells = newRow.Cells.CastTo<Cell[], IReadOnlyList<Cell>>();
                    for (int j = 0; j < cells_filtered_length; j += 8) {
                        Cell cell = cells.At(cells_filtered.At(j));
                        ReadOnlySpan<char> chars = splitter.Next();
                        try {
                            cell.SetValue(chars);
                            // if (type == PARAMDEF.DefType.u8) {
                            //     cell.Value = Convert.ToByte(values[cell_index], CultureInfo.InvariantCulture);
                            // }
                            // if (type == PARAMDEF.DefType.s16) {
                            //     cell.Value = Convert.ToInt16(values[cell_index], CultureInfo.InvariantCulture);
                            // }
                            // if (type == PARAMDEF.DefType.u16) {
                            //     cell.Value = Convert.ToUInt16(values[cell_index], CultureInfo.InvariantCulture);
                            // }
                            // if (type == PARAMDEF.DefType.s32) {
                            //     cell.Value = Convert.ToInt32(values[cell_index], CultureInfo.InvariantCulture);
                            // }
                            // if (type == PARAMDEF.DefType.u32) {
                            //     cell.Value = Convert.ToUInt32(values[cell_index], CultureInfo.InvariantCulture);
                            // }
                            // if (type == PARAMDEF.DefType.f32) {
                            //     cell.Value = Convert.ToSingle(values[cell_index]);
                            // }
                            // if (type == PARAMDEF.DefType.fixstr || type == PARAMDEF.DefType.fixstrW) {
                            //     cell.Value = Convert.ToString(values[cell_index]);
                            // }
                        } catch {
                            _ = MessageBox.Show($"Row {newRow.ID}, Field {cell.Name} of {cell.Def.Def.ParamType}, {cell.Type} and {cell.EditorName} has invalid value \"{chars}\", skipped import of this value.", "Data Import");
                        }
                    }
                }
            }
            if (i <= rows_length) {
                if (i < rows_length) {
                    rows.SetLength((int)(i - 16 >> 3)); // Set List size;
                    // Clear references to remaining rows so they can be later reclaimed by GC
                    for (; i != rows_length; i += 8) {
                        rows_array.AssignAnyAt(i, 0UL);
                    }
                }
            } else {
                rows.Sort((Row r1, Row r2) => r1.ID - r2.ID);
            }
            reader.Close();
            if (!settings.ShowConfirmationMessages && !isSilent) {
                _ = MessageBox.Show($"{paramName} data import complete!", "Data Import");
            }
        }
        public static void RunSync(Func<Task> task) {
            SynchronizationContext oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            synch.Post(async _ => {
                try {
                    await task();
                } catch (Exception e) {
                    synch.InnerException = e;
                    throw;
                } finally {
                    synch.EndMessageLoop();
                }
            }, null);
            synch.BeginMessageLoop();

            SynchronizationContext.SetSynchronizationContext(oldContext);
        }

        private class ExclusiveSynchronizationContext : SynchronizationContext {
            private bool done;
            public Exception InnerException { get; set; }
            private readonly AutoResetEvent workItemsWaiting = new(false);
            private readonly Queue<ValueTuple<SendOrPostCallback, object>> items =
                new();

            public override void Send(SendOrPostCallback d, object state) => throw new NotSupportedException("We cannot send to our same thread");

            public override void Post(SendOrPostCallback d, object state) {
                lock (this.items) {
                    this.items.Enqueue((d, state));
                }
                _ = this.workItemsWaiting.Set();
            }

            public void EndMessageLoop() => this.Post(_ => this.done = true, null);

            public void BeginMessageLoop() {
                while (!this.done) {
                    Unsafe.SkipInit(out ValueTuple<SendOrPostCallback, object> task);
                    bool done = false;
                    lock (this.items) {
                        if (this.items.Count > 0) {
                            task = this.items.Dequeue();
                            done = true;
                        }
                    }
                    if (done) {
                        task.Item1(task.Item2);
                        if (this.InnerException != null) // the method threw an exeption
                        {
                            throw new AggregateException("AsyncHelpers.Run method threw an exception.", this.InnerException);
                        }
                    } else {
                        _ = this.workItemsWaiting.WaitOne();
                    }
                }
            }

            public override SynchronizationContext CreateCopy() => this;
        }

        // Token: 0x06000031 RID: 49 RVA: 0x00006AAC File Offset: 0x00004CAC
        private unsafe void ExportParamData(ParamWrapper wrapper, bool isSilent) {
            bool includeRowNameInCSV = settings.IncludeRowNameInCSV;
            bool verboseCSVExport = settings.VerboseCSVExport;
            string exportDelimiter = settings.ExportDelimiter;
            ReadOnlySpan<char> export_delimeter_chars = exportDelimiter;
            string paramPath = $"{this.GetProjectDirectory("CSV")}\\{wrapper.Name}.csv";
            List<Row> rows = wrapper.Rows;
            if (!isSilent && File.Exists(paramPath) && MessageBox.Show($"{paramPath} exists. Overwrite?", "Export Data", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No) {
                return;
            }
            _ = Directory.CreateDirectory(this.GetProjectDirectory("CSV"));
            {
                using var out_file = new FileWriter(paramPath, new FileStreamOptions {
                    BufferSize = 0,
                    Access = FileAccess.Write,
                    Mode = FileMode.Create,
                    Options = FileOptions.SequentialScan
                }, stackalloc char[0x1000]);
                if (verboseCSVExport) {
                    out_file.WriteLine("UNFURLED");
                    foreach (Row row in wrapper.Rows) {
                        out_file.Write(row.ID);
                        out_file.WriteSmallString(export_delimeter_chars);
                        out_file.WriteSmallString("~#");
                        out_file.Write(row.Name);
                        out_file.WriteSmallString(export_delimeter_chars);
                        for (int i = 0; i < row.Cells.Count; i++) {
                            Cell cell2 = row.Cells[i];
                            if (cell2.Def.DisplayType != PARAMDEF.DefType.dummy8) {
                                out_file.WriteSmallString("~#");
                                if (row.Cells.Count == i) {
                                    out_file.WriteLine(cell2);
                                } else {
                                    out_file.Write(cell2);
                                    out_file.WriteSmallString(export_delimeter_chars);
                                }
                            }
                        }
                    }
                } else {
                    PARAMDEF.Field[] fields = wrapper.AppliedParamDef.Fields.AsContents();
                    int fields_count = wrapper.AppliedParamDef.Fields.GetLength();
                    PARAMDEF.DefType* def_types_filtered = stackalloc PARAMDEF.DefType[fields_count];
                    int* cells_filtered = stackalloc int[fields_count];
                    int cells_filtered_length = 0;
                    for (int i = 0; i < fields_count; i++) {
                        if (fields[i].DisplayType != PARAMDEF.DefType.dummy8) {
                            cells_filtered[cells_filtered_length] = 16 + (i << 3);
                            def_types_filtered[cells_filtered_length++] = fields[i].DisplayType;
                        }
                    }

                    int cells_filtered_prelast_length = cells_filtered_length - 1;
                    if (cells_filtered_length != 0) {
                        if (settings.IncludeHeaderInCSV) {
                            out_file.Write("Row ID");
                            out_file.WriteSmallString(export_delimeter_chars);
                            if (includeRowNameInCSV) {
                                out_file.Write("Row Name");
                                out_file.WriteSmallString(export_delimeter_chars);
                            }
                            for (int i = 0; i < cells_filtered_prelast_length; i++) {
                                out_file.Write(fields.At(cells_filtered[i]).InternalName);
                                out_file.WriteSmallString(export_delimeter_chars);
                            }
                            out_file.WriteLine(fields.At(cells_filtered[cells_filtered_prelast_length]).InternalName);
                        }
                        Row[] rows_array = rows.AsContents();
                        /*
                         * Classic CLR Array Representation.
                         * struct CLRArray<T> {
                         *     RuntimeType typeHandle; // offset: 0
                         *     ulong length; // offset: 8
                         *     ...T* data; // offset: 16, end: length * sizeof(T) + 8
                         * }
                         */
                        for (int j = 16, l = j + rows.Count * sizeof(nuint); j != l; j += sizeof(nuint)) {
                            Row row = rows_array.At(j);
                            out_file.Write(row.ID);
                            out_file.WriteSmallString(export_delimeter_chars);
                            if (includeRowNameInCSV) {
                                out_file.Write(row.Name);
                                out_file.WriteSmallString(export_delimeter_chars);
                            }
                            MiniCell[] cells = row.MiniCells;
                            for (int start = 0; start < cells_filtered_prelast_length; start++) {
                                out_file.Write(def_types_filtered[start], cells.At(cells_filtered[start]));
                                out_file.WriteSmallString(export_delimeter_chars);
                            }
                            out_file.WriteLine(def_types_filtered[cells_filtered_prelast_length], cells.At(cells_filtered[cells_filtered_prelast_length]));
                            // for (void** i2 = Pointers.ManagedToPointerArray(cells) + sizeof(ulong), end = i2 + *(ulong*)Pointers.ManagedToPointer(cells); i2 != end;) {
                            //     PARAM.Cell cell = Pointers.PointerToManaged<PARAM.Cell>(*i2);
                            //     if (cell.Def.DisplayType != PARAMDEF.DefType.dummy8) {
                            //         output_file.OPWrite(cell.Value.ToString(), end == ++i2 ? "\n" : exportDelimiter);
                            //     } else i2++;
                            // }
                        }
                    }
                    // foreach (PARAM.Row row2 in wrapper.Rows) {
                    //     output_file.OPWrite(row2.ID.ToString(), exportDelimiter);
                    //     if (Main.settings.IncludeRowNameInCSV) {
                    //         output_file.OPWrite(row2.Name, exportDelimiter);
                    //     }
                    //     int cell_idx3 = 0;
                    //     foreach (PARAM.Cell cell3 in row2.Cells) {
                    //         if (cell3.Def.DisplayType != PARAMDEF.DefType.dummy8) {
                    //             if (row2.Cells.Count == cell_idx3) {
                    //                 output_file.OPWriteLine(cell3.Value.ToString());
                    //             } else {
                    //                 output_file.OPWrite(cell3.Value.ToString(), exportDelimiter);
                    //             }
                    //         }
                    //         cell_idx3++;
                    //     }
                    // }
                }
            }
            StartTextEditorIfNecessary(paramPath);
            if (!isSilent && !settings.ShowConfirmationMessages) {
                _ = MessageBox.Show($"{paramPath} data export complete!", "Data Export");
            }
        }

        // Token: 0x06000032 RID: 50 RVA: 0x00007058 File Offset: 0x00005258
        private void dgvParams_CellContentClick(object sender, DataGridViewCellEventArgs e) {
        }

        // Token: 0x06000033 RID: 51 RVA: 0x0000705C File Offset: 0x0000525C
        private void DgvParams_SelectionChanged(object sender, EventArgs e) {
            if (this.rowSource.DataSource != null) {
                this.dgvIndices[(this.rowSource.DataSource as ParamWrapper ?? this.ThrowCannotGetParamWrapper<ParamWrapper>()).Name] = (
                    this.dgvRows.SelectedCells.Count > 0 ?
                        this.dgvRows.SelectedCells[0].RowIndex :
                        Math.Max(this.dgvRows.FirstDisplayedScrollingRowIndex, 0),
                    Math.Max(this.dgvCells.FirstDisplayedScrollingRowIndex, 0)
                );
            }
            this.rowSource.DataSource = null;
            this.dgvCells.DataSource = null;
            if (this.dgvParams.SelectedCells.Count > 0) {
                settings.SelectedParam = this.dgvParams.SelectedCells[0].RowIndex;
                ParamWrapper paramFile2 = this.dgvParams.SelectedCells[0].OwningRow.DataBoundItem as ParamWrapper ?? this.ThrowCannotGetParamWrapper<ParamWrapper>();
                this.rowSource.DataMember = "Rows";
                this.rowSource.DataSource = paramFile2;
                (int Row, int Cell) indices2 = this.dgvIndices[paramFile2.Name];
                if (indices2.Row >= this.dgvRows.RowCount) {
                    indices2.Row = this.dgvRows.RowCount - 1;
                }
                if (indices2.Row < 0) {
                    indices2.Row = 0;
                }
                this.dgvIndices[paramFile2.Name] = indices2;
                this.dgvRows.ClearSelection();
                if (this.dgvRows.RowCount > 0) {
                    this.dgvRows.FirstDisplayedScrollingRowIndex = indices2.Row;
                    this.dgvRows.Rows[indices2.Row].Selected = true;
                }
            }
        }

        // Token: 0x06000034 RID: 52 RVA: 0x00007244 File Offset: 0x00005444
        private void DgvParams_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e) {
            if (e.RowIndex >= 0) {
                ParamWrapper paramWrapper = this.dgvParams.Rows[e.RowIndex].DataBoundItem as ParamWrapper ?? this.ThrowCannotGetParamWrapper<ParamWrapper>();
                e.ToolTipText = paramWrapper.Description;
            }
        }

        // Token: 0x06000035 RID: 53 RVA: 0x00007288 File Offset: 0x00005488
        private void DgvRows_SelectionChanged(object sender, EventArgs e) {
            if (this.dgvRows.SelectedCells.Count > 0) {
                settings.SelectedRow = this.dgvRows.SelectedCells[0].RowIndex;
                ParamWrapper paramFile = this.dgvParams.SelectedCells[0].OwningRow.DataBoundItem as ParamWrapper ?? this.ThrowCannotGetParamWrapper<ParamWrapper>();
                (int Row, int Cell) indices = this.dgvIndices[paramFile.Name];
                if (this.dgvCells.FirstDisplayedScrollingRowIndex >= 0) {
                    indices.Cell = this.dgvCells.FirstDisplayedScrollingRowIndex;
                }
                var row = (Row)this.dgvRows.SelectedCells[0].OwningRow.DataBoundItem;
                this.dgvCells.DataSource = row.Cells.Where((Cell cell) => cell.Def.DisplayType != PARAMDEF.DefType.dummy8).ToArray();
                if (indices.Cell >= this.dgvCells.RowCount) {
                    indices.Cell = this.dgvCells.RowCount - 1;
                }
                if (indices.Cell < 0) {
                    indices.Cell = 0;
                }
                this.dgvIndices[paramFile.Name] = indices;
                if (this.dgvCells.RowCount > 0) {
                    this.dgvCells.FirstDisplayedScrollingRowIndex = indices.Cell;
                }
                for (int i = 0; i < this.dgvCells.Rows.Count; i++) {
                    var cell3 = this.dgvCells.Rows[i].DataBoundItem as Cell;
                    if (settings.ShowEnums && !settings.ParamDifferenceMode && this.tdf_dict.TryGetValue(cell3.Def.InternalType, out PARAMTDF tdf)) {
                        var enum_dict = new Dictionary<object, string>(tdf.Entries.Count);
                        foreach (PARAMTDF.Entry entry in tdf.Entries) {
                            enum_dict.Add(entry.Value, settings.ShowEnumValueInName ? $"{entry.Value} - {entry.Name}" : entry.Name);
                        }
                        if (enum_dict.ContainsKey(cell3.Value)) {
                            if (this.bool_type_tdfs.Contains(tdf.Name) && settings.DisplayBooleanEnumAsCheckbox) {
                                this.dgvCells.Rows[i].Cells[this.FIELD_VALUE_COL] = new DataGridViewCheckBoxCell {
                                    TrueValue = "1",
                                    FalseValue = "0",
                                    ValueType = cell3.Value.GetType()
                                };
                            } else if (settings.DisableEnumForCustomTypes) {
                                if (!this.custom_type_tdfs.Contains(tdf.Name)) {
                                    (this.dgvCells.Rows[i].Cells[this.FIELD_VALUE_COL] = new DataGridViewComboBoxCell {
                                        DataSource = enum_dict.ToArray(),
                                        DisplayMember = "Value",
                                        ValueMember = "Key",
                                        ValueType = cell3.Value.GetType(),
                                        FlatStyle = FlatStyle.Flat,
                                        DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton
                                    }).Style.SelectionBackColor = GridSelectionBackColor;
                                }
                            } else {
                                (this.dgvCells.Rows[i].Cells[this.FIELD_VALUE_COL] = new DataGridViewComboBoxCell {
                                    DataSource = enum_dict.ToArray(),
                                    DisplayMember = "Value",
                                    ValueMember = "Key",
                                    ValueType = cell3.Value.GetType(),
                                    FlatStyle = FlatStyle.Flat,
                                    DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton
                                }).Style.SelectionBackColor = GridSelectionBackColor;
                            }
                        }
                    }
                }
                var int_color = Color.FromArgb(settings.FieldColor_Int_R, settings.FieldColor_Int_G, settings.FieldColor_Int_B);
                var float_color = Color.FromArgb(settings.FieldColor_Float_R, settings.FieldColor_Float_G, settings.FieldColor_Float_B);
                var bool_color = Color.FromArgb(settings.FieldColor_Bool_R, settings.FieldColor_Bool_G, settings.FieldColor_Bool_B);
                var string_color = Color.FromArgb(settings.FieldColor_String_R, settings.FieldColor_String_G, settings.FieldColor_String_B);
                for (int j = 0; j < this.dgvCells.Rows.Count; j++) {
                    DataGridViewRow cell2 = this.dgvCells.Rows[j];
                    string type = cell2.Cells[this.FIELD_TYPE_COL].Value.ToString();
                    cell2.Cells[2].Style.BackColor = this.darkTheme ? GridBackColor : Color.White;
                    if (!settings.ParamDifferenceMode) {
                        cell2.Cells[2].Style.BackColor = type.Contains("BOOL") || type.Contains("ON_OFF")
                            ? bool_color
                            : type == "f32" ? float_color : type is "fixStr" or "fixStrW" ? string_color // : type is "u32" or "s32" or "u16" or "s16" or "u8" or "s8"
                                                                                                         // ? int_color
                                : int_color;
                    }
                    if (settings.ParamDifferenceMode && this.secondary_result != null) {
                        foreach (Row secondary_row in this.secondary_result.ParamWrappers
                                .Where(secondary_wrapper => secondary_wrapper.Name == paramFile.Name)
                                .SelectMany(secondary_wrapper => secondary_wrapper.Rows
                                    .Where(secondary_row => row != null && secondary_row.ID == row.ID))) {
                            int dgvOffset = 0;
                            for (int p = 0; p < secondary_row.Cells.Count; p++) {
                                Cell primary_cell = row.Cells[p];
                                Cell secondary_cell = secondary_row.Cells[p];
                                if (primary_cell.Def.DisplayType != PARAMDEF.DefType.dummy8) {
                                    if (!primary_cell.Value.Equals(secondary_cell.Value)) {
                                        this.dgvCells.Rows[p - dgvOffset].Cells[2].Style.BackColor = Color.Yellow;
                                    }
                                } else {
                                    dgvOffset++;
                                }
                            }
                        }
                    }
                }
                this.ApplyCellFilter(false);
            }
        }

        // Token: 0x06000036 RID: 54 RVA: 0x00007A48 File Offset: 0x00005C48
        private void DgvRows_CellValidating(object sender, DataGridViewCellValidatingEventArgs e) {
            if (e.ColumnIndex == this.ROW_ID_COL && (!int.TryParse((string)e.FormattedValue, out int value) && (string)e.FormattedValue != "" || value < 0)) {
                Utility.ShowError("""
                    Row ID must be a positive integer.
                    Enter a valid number or press Esc to cancel.
                    """);
                e.Cancel = true;
            }
        }

        // Token: 0x06000037 RID: 55 RVA: 0x00007A8C File Offset: 0x00005C8C
        private void dgvRows_Scroll(object sender, ScrollEventArgs e) => this.rowContextMenu.Close();

        // Token: 0x06000038 RID: 56 RVA: 0x00007A9C File Offset: 0x00005C9C
        private void dgvRows_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e) {
            if (this.InvalidationMode) {
                return;
            }
            if (e.ColumnIndex != -1 && e.RowIndex != -1 && e.Button == MouseButtons.Right && sender is DataGridView dgv) {
                DataGridViewCell c = dgv[e.ColumnIndex, e.RowIndex];
                if (!c.Selected) {
                    c.DataGridView.ClearSelection();
                    c.DataGridView.CurrentCell = c;
                    c.Selected = true;
                }
                DataGridViewCell currentCell = dgv.CurrentCell;
                if (currentCell != null) {
                    ContextMenuStrip cms = currentCell.ContextMenuStrip;
                    if (cms != null) {
                        Rectangle r = dgv.GetCellDisplayRectangle(currentCell.ColumnIndex, currentCell.RowIndex, false);
                        var p = new Point(r.X + r.Width, r.Y + r.Height);
                        cms.Show(currentCell.DataGridView, p);
                    }
                }
            }
        }

        // Token: 0x06000039 RID: 57 RVA: 0x00007B80 File Offset: 0x00005D80
        private void dgvRows_CellContextMenuStripNeeded(object sender, DataGridViewCellContextMenuStripNeededEventArgs e) {
            if (this.InvalidationMode) {
                return;
            }

            if (e.RowIndex == -1 || e.ColumnIndex == -1) {
                return;
            }
            string row_param_name = this.dgvParams.CurrentCell.Value.ToString();
            if (row_param_name == this.ATKPARAM_PC || row_param_name == this.ATKPARAM_NPC || row_param_name == this.BEHAVIORPARAM_PC || row_param_name == this.BEHAVIORPARAM_NPC) {
                e.ContextMenuStrip = this.rowContextMenu;
            }
        }

        // Token: 0x0600003A RID: 58 RVA: 0x00007C0C File Offset: 0x00005E0C
        private void rowContextMenu_Opening(object sender, CancelEventArgs e) {
            var row = (Row)this.dgvRows.Rows[this.dgvRows.CurrentCell.RowIndex].DataBoundItem;
            string row_param_name = this.dgvParams.CurrentCell.Value.ToString();
            this.copyToParamMenuItem.Visible = false;
            this.copyToParamMenuItem.Text = "";
            if (row_param_name == this.ATKPARAM_PC ||
                row_param_name == this.ATKPARAM_NPC ||
                row_param_name == this.BEHAVIORPARAM_PC ||
                row_param_name == this.BEHAVIORPARAM_NPC) {
                this.copyToParamMenuItem.Visible = true;
                this.copyToParamMenuItem.Text = $"Copy {row.ID} to {true switch {
                    true when row_param_name == this.ATKPARAM_PC => this.ATKPARAM_NPC,
                    true when row_param_name == this.ATKPARAM_NPC => this.ATKPARAM_PC,
                    true when row_param_name == this.BEHAVIORPARAM_PC => this.BEHAVIORPARAM_NPC,
                    true when row_param_name == this.BEHAVIORPARAM_NPC => this.BEHAVIORPARAM_PC,
                    _ => throw new Exception(),
                }}.";
            }
        }

        // Token: 0x0600003B RID: 59 RVA: 0x00007E04 File Offset: 0x00006004
        private void copyToParamMenuItem_Click(object sender, EventArgs e) {
            var row = this.dgvRows.Rows[this.dgvRows.CurrentCell.RowIndex].DataBoundItem as Row;
            string current_param_name = this.dgvParams.CurrentCell.Value.ToString();
            string target_param_name = true switch {
                true when current_param_name == this.ATKPARAM_PC => this.ATKPARAM_NPC,
                true when current_param_name == this.ATKPARAM_NPC => this.ATKPARAM_PC,
                true when current_param_name == this.BEHAVIORPARAM_PC => this.BEHAVIORPARAM_NPC,
                true when current_param_name == this.BEHAVIORPARAM_NPC => this.BEHAVIORPARAM_PC,
                _ => "",
            };
            ParamWrapper target_wrapper = this.primary_result.ParamWrappers.Where(wrapper => wrapper.Name == target_param_name).FirstOrDefault();

            var newRowForm = new NewRow("New Row", row.ID, row.Name);
            if (newRowForm.ShowDialog() == DialogResult.OK) {
                int id = newRowForm.ResultID;
                string name = newRowForm.ResultName;
                if (target_wrapper.Rows.Any((Row wrapper_row) => row.ID == id)) {
                    Utility.ShowError($"A row with this ID already exists: {id}");
                    return;
                }
                var row_result = new Row(id, name, target_wrapper.AppliedParamDef);
                for (int i = 0; i < row.Cells.Count; i++) {
                    row_result.Cells[i].Value = row.Cells[i].Value;
                }
                target_wrapper.Rows.Add(row_result);
                target_wrapper.Rows.Sort((r1, r2) => r1.ID - r2.ID);
                for (int j = 0; j < this.dgvParams.Rows.Count; j++) {
                    if (this.dgvParams.Rows[j].Cells[0].Value.ToString() == target_param_name) {
                        int target_param_idx = j;
                        this.dgvParams.ClearSelection();
                        this.dgvParams.Rows[target_param_idx].Selected = true;
                    }
                }
                for (int k = 0; k < this.dgvRows.Rows.Count; k++) {
                    if (id == Convert.ToInt32(this.dgvRows.Rows[k].Cells[0].Value)) {
                        int target_row_idx = k;
                        this.dgvRows.ClearSelection();
                        this.dgvRows.Rows[target_row_idx].Selected = true;
                        this.dgvRows.CurrentCell = this.dgvRows.Rows[target_row_idx].Cells[0];
                    }
                }
            }
        }

        // Token: 0x0600003C RID: 60 RVA: 0x00008148 File Offset: 0x00006348
        private void dgvCells_SelectionChanged(object sender, EventArgs e) {
            if (this.dgvCells.SelectedCells.Count > 0) {
                settings.SelectedField = this.dgvCells.SelectedCells[0].RowIndex;
            }
        }

        // Token: 0x0600003D RID: 61 RVA: 0x0000817D File Offset: 0x0000637D
        private void dgvCells_Scroll(object sender, ScrollEventArgs e) => this.fieldContextMenu.Close();

        // Token: 0x0600003E RID: 62 RVA: 0x0000818C File Offset: 0x0000638C
        private void DgvCells_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e) {
            if (settings.CellView_ShowEditorNames) {
                this.dgvCells.Columns[this.FIELD_PARAM_NAME_COL].Visible = false;
                this.dgvCells.Columns[this.FIELD_EDITOR_NAME_COL].Visible = true;
            } else {
                this.dgvCells.Columns[this.FIELD_PARAM_NAME_COL].Visible = true;
                this.dgvCells.Columns[this.FIELD_EDITOR_NAME_COL].Visible = false;
            }
            if (settings.CellView_ShowTypes) {
                this.dgvCells.Columns[this.FIELD_TYPE_COL].Visible = true;
                this.dgvCells.Columns[this.FIELD_TYPE_COL].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                this.dgvCells.Columns[this.FIELD_VALUE_COL].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                return;
            }
            this.dgvCells.Columns[this.FIELD_TYPE_COL].Visible = false;
            this.dgvCells.Columns[this.FIELD_TYPE_COL].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvCells.Columns[this.FIELD_VALUE_COL].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        // Token: 0x0600003F RID: 63 RVA: 0x000082CE File Offset: 0x000064CE
        private void DgvCells_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {
        }

        // Token: 0x06000040 RID: 64 RVA: 0x000082D0 File Offset: 0x000064D0
        private void DgvCells_CellValidating(object sender, DataGridViewCellValidatingEventArgs e) {
            if (settings.EnableFieldValidation && e.ColumnIndex == this.FIELD_VALUE_COL) {
                var cell = this.dgvCells.Rows[e.RowIndex].DataBoundItem as Cell;
                if (cell.Def.DisplayType != PARAMDEF.DefType.fixstr && cell.Def.DisplayType != PARAMDEF.DefType.fixstrW && !(settings.ShowEnums && this.tdf_dict.ContainsKey(cell.Def.InternalType))) {
                    if (float.TryParse(e.FormattedValue.ToString(), out float current_value)) {
                        if (current_value > Convert.ToSingle(cell.Def.Maximum) || current_value < Convert.ToSingle(cell.Def.Minimum)) {
                            e.Cancel = true;
                            this.EnterInvalidationMode();
                            if (this.dgvCells.EditingPanel != null) {
                                this.dgvCells.EditingPanel.BackColor = Color.Pink;
                                this.dgvCells.EditingControl.BackColor = Color.Pink;
                            }
                            SystemSounds.Hand.Play();
                        } else {
                            this.ExitInvalidationMode();
                        }
                    } else {
                        e.Cancel = true;
                        this.EnterInvalidationMode();
                        if (this.dgvCells.EditingPanel != null) {
                            this.dgvCells.EditingPanel.BackColor = Color.Pink;
                            this.dgvCells.EditingControl.BackColor = Color.Pink;
                        }
                        SystemSounds.Hand.Play();
                    }
                }
            }
        }

        // Token: 0x06000041 RID: 65 RVA: 0x00008468 File Offset: 0x00006668
        private void EnterInvalidationMode() {
            this.InvalidationMode = true;
            this.dgvParams.Enabled = false;
            this.dgvRows.Enabled = false;
            this.fileToolStripMenuItem.Enabled = false;
            this.editToolStripMenuItem.Enabled = false;
            this.viewToolStripMenuItem.Enabled = false;
            this.ToolStripMenuItem.Enabled = false;
            this.WorkflowToolStripMenuItem.Enabled = false;
            this.settingsMenuItem.Enabled = false;
            this.filter_Params.Enabled = false;
            this.button_FilterParams.Enabled = false;
            this.button_ResetFilterParams.Enabled = false;
            this.filter_Rows.Enabled = false;
            this.button_FilterRows.Enabled = false;
            this.button_ResetFilterRows.Enabled = false;
            this.filter_Cells.Enabled = false;
            this.button_FilterCells.Enabled = false;
            this.button_ResetFilterCells.Enabled = false;
        }

        // Token: 0x06000042 RID: 66 RVA: 0x00008548 File Offset: 0x00006748
        private void ExitInvalidationMode() {
            this.InvalidationMode = false;
            this.dgvParams.Enabled = true;
            this.dgvRows.Enabled = true;
            this.fileToolStripMenuItem.Enabled = true;
            this.editToolStripMenuItem.Enabled = true;
            this.viewToolStripMenuItem.Enabled = true;
            this.ToolStripMenuItem.Enabled = true;
            this.WorkflowToolStripMenuItem.Enabled = true;
            this.settingsMenuItem.Enabled = true;
            this.filter_Params.Enabled = true;
            this.button_FilterParams.Enabled = true;
            this.button_ResetFilterParams.Enabled = true;
            this.filter_Rows.Enabled = true;
            this.button_FilterRows.Enabled = true;
            this.button_ResetFilterRows.Enabled = true;
            this.filter_Cells.Enabled = true;
            this.button_FilterCells.Enabled = true;
            this.button_ResetFilterCells.Enabled = true;
        }

        // Token: 0x06000043 RID: 67 RVA: 0x00008628 File Offset: 0x00006828
        private void DgvCells_CellParsing(object sender, DataGridViewCellParsingEventArgs e) {
        }

        // Token: 0x06000044 RID: 68 RVA: 0x0000862C File Offset: 0x0000682C
        private void DgvCells_DataError(object sender, DataGridViewDataErrorEventArgs e) {
            e.Cancel = true;
            if (this.dgvCells.EditingPanel != null) {
                this.dgvCells.EditingPanel.BackColor = Color.Pink;
                this.dgvCells.EditingControl.BackColor = Color.Pink;
            }
            SystemSounds.Hand.Play();
        }

        // Token: 0x06000045 RID: 69 RVA: 0x00008684 File Offset: 0x00006884
        private void DgvCells_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e) {
            if ((e.ColumnIndex == this.FIELD_PARAM_NAME_COL || e.ColumnIndex == this.FIELD_EDITOR_NAME_COL) &&
                e.RowIndex >= 0 && settings.ShowFieldDescriptions) {
                var cell = this.dgvCells.Rows[e.RowIndex].DataBoundItem as Cell;
                e.ToolTipText = $"""
                    {cell.Def.Description}
                    Minimum: {cell.Def.Minimum}
                    Maximum: {cell.Def.Maximum}
                    Increment: {cell.Def.Increment}
                    """;
            }
            if (e.ColumnIndex == this.FIELD_VALUE_COL && e.RowIndex >= 0 && this.dgvRows.CurrentCell != null) {
                var cell2 = this.dgvCells.Rows[e.RowIndex].DataBoundItem as Cell;
                var current_row = (Row)this.dgvRows.Rows[this.dgvRows.CurrentCell.RowIndex].DataBoundItem;
                _ = current_row.ID;
                string tooltip = "";
                string cell_name = cell2.Name.ToString();
                object cell_value = cell2.Value;
                if (cell2.GetCellReferences() is string[] def_names) {
                    ParamWrapper wrapper = this.primary_result.ParamWrappers.Find(wrapper => Array.Exists(def_names, x => x == wrapper.Name));
                    int v = Convert.ToInt32(cell_value);
                    if (cell_name == "behaviorVariationId") {
                        foreach (Row row in wrapper.Rows
                            .Where(row => row["variationId"] is Cell field && Convert.ToInt32(field.Value) == v)) {
                            tooltip = $"{tooltip}{v} {row.Name ?? ""}\n";
                        }
                    } else if (wrapper.Rows.Find(x => x.ID == v) is Row row) {
                        tooltip = $"{tooltip}{v} {row.Name ?? ""}\n";
                    }
                }
                if (settings.ParamDifferenceMode) {
                    string current_param_name = this.dgvParams.CurrentCell.Value.ToString();
                    if (this.secondary_result != null) {
                        foreach (Cell secondary_cell in this.secondary_result.ParamWrappers
                            .Where(secondary_wrapper => secondary_wrapper.Name == current_param_name)
                            .SelectMany(secondary_wrapper => secondary_wrapper.Rows
                                .Where(secondary_row => current_row != null && secondary_row.ID == current_row.ID))
                                    .SelectMany(secondary_row => secondary_row.Cells
                                        .Where(secondary_cell => cell2.Def.DisplayName == secondary_cell.Def.DisplayName &&
                                            cell2.Def.DisplayType != PARAMDEF.DefType.dummy8 &&
                                            !cell2.Value.Equals(secondary_cell.Value)))) {
                            tooltip += $"Secondary Value: {secondary_cell.Value}\n";
                        }
                    }
                }
                e.ToolTipText = tooltip;
            }
        }

        // Token: 0x06000046 RID: 70 RVA: 0x0000900C File Offset: 0x0000720C
        private void dgvCells_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e) {
            if (this.InvalidationMode) {
                return;
            }
            if (e.ColumnIndex != -1 && e.RowIndex != -1 && e.Button == MouseButtons.Right) {
                DataGridViewCell c = (sender as DataGridView)[e.ColumnIndex, e.RowIndex];
                if (!c.Selected) {
                    c.DataGridView.ClearSelection();
                    c.DataGridView.CurrentCell = c;
                    c.Selected = true;
                }
                DataGridViewCell currentCell = (sender as DataGridView).CurrentCell;
                if (currentCell != null) {
                    ContextMenuStrip cms = currentCell.ContextMenuStrip;
                    if (cms != null) {
                        Rectangle r = currentCell.DataGridView.GetCellDisplayRectangle(currentCell.ColumnIndex, currentCell.RowIndex, false);
                        var p = new Point(r.X + r.Width, r.Y + r.Height);
                        cms.Show(currentCell.DataGridView, p);
                    }
                }
            }
        }

        // Token: 0x06000047 RID: 71 RVA: 0x000090F0 File Offset: 0x000072F0
        private void dgvCells_CellContextMenuStripNeeded(object sender, DataGridViewCellContextMenuStripNeededEventArgs e) {
            if (this.InvalidationMode) {
                return;
            }

            if (e.RowIndex == -1 || e.ColumnIndex == -1) {
                return;
            }
            if (this.dgvCells.Rows[e.RowIndex].DataBoundItem is Cell cell && cell.GetCellReferences() != null && Convert.ToInt32(cell.Value) > -1) {
                e.ContextMenuStrip = this.fieldContextMenu;
            }
        }

        //private void OpenFieldContextMenu(Cell cell) {
        //    foreach (ParamWrapper item in this.primary_result.ParamWrappers) {

        //    }
        //}

        // Token: 0x06000048 RID: 72 RVA: 0x00009164 File Offset: 0x00007364
        private unsafe void fieldContextMenu_Opening(object sender, CancelEventArgs e) {
            var cell = this.dgvCells.Rows[this.dgvCells.CurrentCell.RowIndex].DataBoundItem as Cell;
            int cell_value_id = Convert.ToInt32(cell.Value);
            bool behaviorRow_FirstOnly = false;
            this.GotoReference1MenuItem.Text = "";
            this.GotoReference2MenuItem.Text = "";
            this.GotoReference3MenuItem.Text = "";
            this.GotoReference4MenuItem.Text = "";
            this.GotoReference5MenuItem.Text = "";
            this.GotoReference6MenuItem.Text = "";
            this.GotoReference1MenuItem.Visible = false;
            this.GotoReference2MenuItem.Visible = false;
            this.GotoReference3MenuItem.Visible = false;
            this.GotoReference4MenuItem.Visible = false;
            this.GotoReference5MenuItem.Visible = false;
            this.GotoReference6MenuItem.Visible = false;
            if (cell.GetCellReferences() is string[] def_names) {
                bool isBehaviorVariationID = cell.Name.ToString() is "behaviorVariationId" or "Behavior Variation ID";
                foreach (ParamWrapper wrapper in this.primary_result.ParamWrappers) {
                    int index = Array.FindIndex(def_names, x => x == wrapper.Name);
                    if (index != -1) {
                        Row[] rowp = wrapper.Rows.AsContents();
                        for (int i = 0, count = wrapper.Rows.Count; i < count; i++) {
                            Row row = rowp[i];
                            if (!behaviorRow_FirstOnly && isBehaviorVariationID) {
                                Cell* cellp = AsPointer<IReadOnlyList<Cell>, Cell>(row.Cells);
                                for (int i2 = 2, count2 = row.Cells.Count + 2; i2 < count2; i2++) {
                                    Cell behavior_cell = cellp[i2];
                                    if (behavior_cell.Def.InternalName == "variationId" && Convert.ToInt32(behavior_cell.Value) == cell_value_id) {
                                        this.GotoReference1MenuItem.Visible = true;
                                        this.GotoReference1MenuItem.Text = $"Go to row {row.ID} in {def_names[index]}";
                                        behaviorRow_FirstOnly = true;
                                    }
                                }
                            } else if (row.ID == cell_value_id) {
                                this.GoToReferenceMenuStripItems[index].Visible = true;
                                this.GoToReferenceMenuStripItems[index].Text = $"Go to row {row.ID} in {def_names[index]}";
                            }
                        }
                    }
                }
            }
        }

        // Token: 0x06000049 RID: 73 RVA: 0x00009810 File Offset: 0x00007A10
        private void GotoReferenceHelper(string paramref) {
            var cell = this.dgvCells.Rows[this.dgvCells.CurrentCell.RowIndex].DataBoundItem as Cell;
            int cell_value_id = Convert.ToInt32(cell.Value);
            for (int i = 0; i < this.dgvParams.Rows.Count; i++) {
                string name = this.dgvParams.Rows[i].Cells[0].Value.ToString();
                if (paramref == name) {
                    int target_param_idx = i;
                    this.dgvParams.ClearSelection();
                    this.dgvParams.Rows[target_param_idx].Selected = true;
                }
            }
            if (cell.Name.ToString() is "behaviorVariationId" or "Behavior Variation ID" && cell.GetCellReferences() is string[] def_names) {
                int target_row = 0;
                bool isBehaviorMatched = false;
                foreach (ParamWrapper wrapper in this.primary_result.ParamWrappers) {
                    if (Array.Exists(def_names, x => x == wrapper.Name) && !isBehaviorMatched) {
                        for (int i = 0; i < wrapper.Rows.Count; i++) {
                            Row wrapper_row = wrapper.Rows[i];
                            if (!isBehaviorMatched) {
                                for (int i2 = 0; i2 < wrapper_row.Cells.Count; i2++) {
                                    Cell wrapper_cell = wrapper_row.Cells[i2];
                                    if (!isBehaviorMatched && (wrapper_cell.Name.ToString() == "variationId" || wrapper_cell.EditorName.ToString() == "Variation ID") && cell_value_id == Convert.ToInt32(wrapper_cell.Value)) {
                                        target_row = wrapper_row.ID;
                                        isBehaviorMatched = true;
                                    }
                                }
                            }
                        }
                    }
                }
                for (int j = 0; j < this.dgvRows.Rows.Count; j++) {
                    if (target_row == Convert.ToInt32(this.dgvRows.Rows[j].Cells[0].Value)) {
                        int target_row_idx = j;
                        this.dgvRows.ClearSelection();
                        this.dgvRows.Rows[target_row_idx].Selected = true;
                        this.dgvRows.CurrentCell = this.dgvRows.Rows[target_row_idx].Cells[0];
                    }
                }
                return;
            }
            for (int j = 0; j < this.dgvRows.Rows.Count; j++) {
                if (cell_value_id == Convert.ToInt32(this.dgvRows.Rows[j].Cells[0].Value)) {
                    int target_row_idx = j;
                    this.dgvRows.ClearSelection();
                    this.dgvRows.Rows[target_row_idx].Selected = true;
                    this.dgvRows.CurrentCell = this.dgvRows.Rows[target_row_idx].Cells[0];
                }
            }
        }

        // Token: 0x0600004A RID: 74 RVA: 0x00009B98 File Offset: 0x00007D98
        private void GotoReference1MenuItem_Click(object sender, EventArgs e) =>
            this.GotoReferenceHelper((this.dgvCells.Rows[this.dgvCells.CurrentCell.RowIndex].DataBoundItem as Cell).GetCellReferences()[0]);

        // Token: 0x0600004B RID: 75 RVA: 0x00009BE4 File Offset: 0x00007DE4
        private void GotoReference2MenuItem_Click(object sender, EventArgs e) =>
            this.GotoReferenceHelper((this.dgvCells.Rows[this.dgvCells.CurrentCell.RowIndex].DataBoundItem as Cell).GetCellReferences()[1]);

        // Token: 0x0600004C RID: 76 RVA: 0x00009C30 File Offset: 0x00007E30
        private void GotoReference3MenuItem_Click(object sender, EventArgs e) =>
            this.GotoReferenceHelper((this.dgvCells.Rows[this.dgvCells.CurrentCell.RowIndex].DataBoundItem as Cell).GetCellReferences()[2]);

        // Token: 0x0600004D RID: 77 RVA: 0x00009C7C File Offset: 0x00007E7C
        private void GotoReference4MenuItem_Click(object sender, EventArgs e) =>
            this.GotoReferenceHelper((this.dgvCells.Rows[this.dgvCells.CurrentCell.RowIndex].DataBoundItem as Cell).GetCellReferences()[3]);

        // Token: 0x0600004E RID: 78 RVA: 0x00009CC8 File Offset: 0x00007EC8
        private void GotoReference5MenuItem_Click(object sender, EventArgs e) =>
            this.GotoReferenceHelper((this.dgvCells.Rows[this.dgvCells.CurrentCell.RowIndex].DataBoundItem as Cell).GetCellReferences()[4]);

        // Token: 0x0600004F RID: 79 RVA: 0x00009D14 File Offset: 0x00007F14
        private void GotoReference6MenuItem_Click(object sender, EventArgs e) =>
            this.GotoReferenceHelper((this.dgvCells.Rows[this.dgvCells.CurrentCell.RowIndex].DataBoundItem as Cell).GetCellReferences()[5]);

        // Token: 0x06000050 RID: 80 RVA: 0x00009D5D File Offset: 0x00007F5D
        private void viewInterfaceSettingsToolStripMenuItem_Click(object sender, EventArgs e) {
            if (this.InvalidationMode) {
                return;
            }
            if (new InterfaceSettings().ShowDialog() == DialogResult.OK && !settings.ShowConfirmationMessages) {
                _ = MessageBox.Show("Interace Settings changed.", "Settings", MessageBoxButtons.OK);
            }
        }

        // Token: 0x06000051 RID: 81 RVA: 0x00009D92 File Offset: 0x00007F92
        private void viewDataSettingsToolStripMenuItem_Click(object sender, EventArgs e) {
            if (this.InvalidationMode) {
                return;
            }
            if (new DataSettings().ShowDialog() == DialogResult.OK && !settings.ShowConfirmationMessages) {
                _ = MessageBox.Show("Data Settings changed.", "Settings", MessageBoxButtons.OK);
            }
        }

        // Token: 0x06000052 RID: 82 RVA: 0x00009DC7 File Offset: 0x00007FC7
        private void viewFilterSettingsToolStripMenuItem_Click(object sender, EventArgs e) {
            if (this.InvalidationMode) {
                return;
            }
            if (new FilterSettings().ShowDialog() == DialogResult.OK && !settings.ShowConfirmationMessages) {
                _ = MessageBox.Show("Filter Settings changed.", "Settings", MessageBoxButtons.OK);
            }
        }

        // Token: 0x06000053 RID: 83 RVA: 0x00009DFC File Offset: 0x00007FFC
        private void selectSecondaryFileToolStripMenuItem_Click(object sender, EventArgs e) {
            if (this.InvalidationMode) {
                return;
            }
            this.secondaryFilePath.FileName = "";
            if (this.secondaryFilePath.ShowDialog() == DialogResult.OK) {
                settings.SecondaryFilePath = this.secondaryFilePath.FileName;
                this.LoadSecondaryParams(false);
            }
        }

        // Token: 0x06000054 RID: 84 RVA: 0x00009E4C File Offset: 0x0000804C
        private void showParamDifferencesToolStripMenuItem_Click(object sender, EventArgs e) {
            if (this.InvalidationMode) {
                return;
            }
            if (this.secondary_result == null) {
                _ = MessageBox.Show("Secondary File not set.", "Secondary File", MessageBoxButtons.OK);
                return;
            }
            if (settings.ParamDifferenceMode) {
                settings.ParamDifferenceMode = false;
                if (!settings.ShowConfirmationMessages) {
                    _ = MessageBox.Show("Param Difference mode coloring removed", "Param Difference Mode", MessageBoxButtons.OK);
                    return;
                }
            } else {
                settings.ParamDifferenceMode = true;
                if (!settings.ShowConfirmationMessages) {
                    _ = MessageBox.Show("Param Difference mode coloring added", "Param Difference Mode", MessageBoxButtons.OK);
                }
            }
        }

        // Token: 0x06000055 RID: 85 RVA: 0x00009EDC File Offset: 0x000080DC
        private void clearSecondaryFileToolMenuItem_Click(object sender, EventArgs e) {
            if (this.secondaryFilePath.FileName == "") {
                _ = MessageBox.Show("Secondary File not set.", "Secondary File", MessageBoxButtons.OK);
                return;
            }
            this.secondaryFilePath.FileName = "";
            settings.SecondaryFilePath = this.secondaryFilePath.FileName;
            settings.ParamDifferenceMode = false;
            if (!settings.ShowConfirmationMessages) {
                _ = MessageBox.Show("Removed set secondary file path.", "Secondary File", MessageBoxButtons.OK);
            }
        }

        // Token: 0x06000056 RID: 86 RVA: 0x00009F60 File Offset: 0x00008160
        private void button_FilterParams_Click(object sender, EventArgs e) {
            string command_delimiter_string = settings.Filter_CommandDelimiter;
            char[] command_delimiter = command_delimiter_string.ToCharArray();
            char[] section_delimiter = settings.Filter_SectionDelimiter.ToCharArray();
            if (this.dgvParams.Rows.Count != 0) {
                if (this.filter_Params.Text.ToLower().Split(section_delimiter) is string[] input_list && input_list[0] != "") {
                    this.dgvParamsParamCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    this.EnterInvalidationMode();
                    bool hasSelectedParam = false;
                    if (this.dgvRows.Rows.Count != 0) {
                        this.dgvRows.Rows[0].Selected = true;
                    }
                    if (this.dgvCells.Rows.Count != 0) {
                        this.dgvCells.Rows[0].Selected = true;
                    }
                    for (int i = 0; i < this.dgvParams.Rows.Count; i++) {
                        DataGridViewRow param = this.dgvParams.Rows[i];
                        string param_name = param.Cells[0].Value.ToString().ToLower();
                        var currencyManager = this.BindingContext[this.dgvParams.DataSource] as CurrencyManager;
                        currencyManager.SuspendBinding();
                        param.Selected = false;
                        currencyManager.ResumeBinding();
                        if (param.Visible = input_list.All(current_input =>
                            current_input.Contains($"view{command_delimiter_string}")
                                ? BuildViewList("Views\\\\Param\\\\",
                                    current_input.Split(command_delimiter)[1].TrimStart(' ').ToLower())
                                    .Any(param_name.Contains)
                                : current_input.Contains($"exact{command_delimiter_string}")
                                    ? (current_input = current_input.Split(command_delimiter)[1].TrimStart(' ').ToLower()).Length > 0 &&
                                        param_name == current_input
                                    : current_input.Length > 0 && param_name.Contains(current_input))) {
                            if (!hasSelectedParam) {
                                hasSelectedParam = true;
                                param.Selected = true;
                            }
                        }
                    }
                    this.dgvParamsParamCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    this.ExitInvalidationMode();
                } else {
                    Utility.ShowError("No filter command present.");
                }
            }
        }

        // Token: 0x06000057 RID: 87 RVA: 0x0000A274 File Offset: 0x00008474
        private void button_ResetFilterParams_Click(object sender, EventArgs e) {
            this.dgvParamsParamCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            bool hasSelectedFirstMatch = false;
            this.filter_Params.Text = "";
            for (int i = 0; i < this.dgvParams.Rows.Count; i++) {
                DataGridViewRow dgv_row = this.dgvParams.Rows[i];
                dgv_row.Visible = true;
                dgv_row.Selected = false;
                if (!hasSelectedFirstMatch) {
                    dgv_row.Selected = true;
                    hasSelectedFirstMatch = true;
                }
            }
            this.dgvParamsParamCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        // Token: 0x06000058 RID: 88 RVA: 0x0000A2F4 File Offset: 0x000084F4
        private void button_FilterRows_Click(object sender, EventArgs e) {
            string command_delimiter_string = settings.Filter_CommandDelimiter;
            char[] command_delimiter = command_delimiter_string.ToCharArray();
            char[] section_delimiter = settings.Filter_SectionDelimiter.ToCharArray();
            if (this.dgvRows.Rows.Count != 0) {
                if (this.filter_Rows.Text.ToLower().Split(section_delimiter) is string[] input_list && input_list[0] != "") {
                    var currencyManager = this.BindingContext[this.dgvRows.DataSource] as CurrencyManager;
                    bool hasSelectedRow = false;
                    this.dgvRowsIDCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    this.dgvRowsNameCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    this.EnterInvalidationMode();
                    currencyManager.SuspendBinding();
                    foreach (DataGridViewRow current_row in this.dgvRows.Rows) {
                        current_row.Selected = (current_row.Visible =
                            current_row.DataBoundItem is Row row_data &&
                            current_row.Cells[this.ROW_ID_COL].Value.ToString().ToLower() is string current_row_id &&
                            current_row.Cells[this.ROW_NAME_COL].Value.ToString().ToLower() is string current_row_name &&
                            input_list.All(current_input => current_input != "" && (
                                current_input.Contains($"view{command_delimiter_string}")
                                    ? BuildViewList("Views\\\\Row\\\\", current_input.Split(command_delimiter)[1].TrimStart(' ').ToLower())
                                        .Any(view_name => current_row_name.Contains(view_name) || current_row_id.Contains(view_name))
                                    : current_input.Contains($"exact{command_delimiter_string}")
                                        ? (current_input = current_input.Split(command_delimiter)[1].TrimStart(' ').ToLower()) != "" &&
                                            (current_row_name == current_input || current_row_id == current_input)
                                        : (current_input.Contains($"field{command_delimiter_string}") &&
                                            current_input.Split(command_delimiter) is string[] temp_input &&
                                            temp_input[1].TrimStart(' ').ToLower() is string field_input &&
                                            field_input != "" &&
                                            temp_input[2].TrimStart(' ').ToLower() is string value_input &&
                                            value_input != ""
                                                ? row_data.Cells.Any(cell =>
                                                    (cell.EditorName.ToString().ToLower() == field_input ||
                                                        cell.Name.ToString().ToLower() == field_input) &&
                                                    cell.Value.ToString() is string field_value &&
                                                    (value_input.Contains(">=") &&
                                                    Convert.ToSingle(field_value) >= Convert.ToSingle(value_input.Replace(">=", "")) ||
                                                    value_input.Contains('>') &&
                                                    Convert.ToSingle(field_value) > Convert.ToSingle(value_input.Replace(">", "")) ||
                                                    value_input.Contains("<=") &&
                                                    Convert.ToSingle(field_value) <= Convert.ToSingle(value_input.Replace("<=", "")) ||
                                                    value_input.Contains('<') &&
                                                    Convert.ToSingle(field_value) < Convert.ToSingle(value_input.Replace("<", "")) ||
                                                    field_value == value_input)
                                                )
                                                : current_row_name.Contains(current_input) || current_row_id.Contains(current_input))
                            ))
                        ) && !hasSelectedRow && (hasSelectedRow = true);
                    }
                    currencyManager.ResumeBinding();
                    this.dgvRowsIDCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    this.dgvRowsNameCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    this.ExitInvalidationMode();
                } else {
                    Utility.ShowError("No filter command present.");
                }
            }
        }

        // Token: 0x06000059 RID: 89 RVA: 0x0000A850 File Offset: 0x00008A50
        private void button_ResetFilterRows_Click(object sender, EventArgs e) {
            this.dgvRowsIDCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            this.dgvRowsNameCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            bool hasSelectedFirstMatch = false;
            this.filter_Rows.Text = "";
            for (int i = 0; i < this.dgvRows.Rows.Count; i++) {
                DataGridViewRow dgv_row = this.dgvRows.Rows[i];
                dgv_row.Visible = true;
                dgv_row.Selected = false;
                if (!hasSelectedFirstMatch) {
                    dgv_row.Selected = true;
                    hasSelectedFirstMatch = true;
                }
            }
            this.dgvRowsIDCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvRowsNameCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        // Token: 0x0600005A RID: 90 RVA: 0x0000A8E7 File Offset: 0x00008AE7
        private void button_FilterCells_Click(object sender, EventArgs e) => this.ApplyCellFilter(true);

        // Token: 0x0600005B RID: 91 RVA: 0x0000A8F0 File Offset: 0x00008AF0
        private void ApplyCellFilter(bool invokeInvalidationMode) {
            char[] command_delimiter = settings.Filter_CommandDelimiter.ToCharArray();
            char[] section_delimiter = settings.Filter_SectionDelimiter.ToCharArray();
            string command_delimiter_string = settings.Filter_CommandDelimiter;
            if (this.dgvCells.Rows.Count == 0) {
                return;
            }
            if (this.filter_Cells.Text == "") {
                return;
            }
            this.dgvCellsNameCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            this.dgvCellsEditorNameCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            this.dgvCellsValueCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            this.dgvCellsTypeCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            if (invokeInvalidationMode) {
                this.EnterInvalidationMode();
            }
            string[] input_list = this.filter_Cells.Text.ToLower().Split(section_delimiter);
            if (input_list[0].Length > 0) {
                bool hasSelectedCell = false;
                for (int i = 0; i < this.dgvCells.Rows.Count; i++) {
                    DataGridViewRow cell_row = this.dgvCells.Rows[i];
                    string cell_row_param_name = cell_row.Cells[this.FIELD_PARAM_NAME_COL].Value.ToString().ToLower();
                    string cell_row_editor_name = cell_row.Cells[this.FIELD_EDITOR_NAME_COL].Value.ToString().ToLower();
                    string cell_row_value = cell_row.Cells[this.FIELD_VALUE_COL].Value.ToString().ToLower();
                    var currencyManager = this.BindingContext[this.dgvCells.DataSource] as CurrencyManager;
                    currencyManager.SuspendBinding();
                    cell_row.Visible = false;
                    cell_row.Selected = false;
                    currencyManager.ResumeBinding();
                    if (input_list.All([MethodImpl(MethodImplOptions.AggressiveInlining)] (current_input) => current_input.Contains($"view{command_delimiter_string}")
                        && BuildViewList("Views\\\\Field\\\\", current_input.Split(command_delimiter)[1].TrimStart(' ').ToLower())
                            .Any(view_name => cell_row_param_name.Contains(view_name) || cell_row_editor_name.Contains(view_name) || cell_row_value.Contains(view_name))
                        || current_input.Contains("exact" + command_delimiter_string)
                            && (current_input = current_input.Split(command_delimiter)[1].TrimStart(' ').ToLower()).Length != 0
                            && (cell_row_param_name.Equals(current_input) || cell_row_editor_name.Equals(current_input) || cell_row_value.Equals(current_input))
                            || current_input.Length > 0 && (cell_row_param_name.Contains(current_input) || cell_row_editor_name.Contains(current_input) || cell_row_value.Contains(current_input)))) {
                        cell_row.Visible = true;
                        if (!hasSelectedCell) {
                            hasSelectedCell = true;
                            cell_row.Selected = true;
                        }
                    }
                }
                this.dgvCellsNameCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                this.dgvCellsEditorNameCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                if (settings.CellView_ShowTypes) {
                    this.dgvCellsValueCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    this.dgvCellsTypeCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                } else {
                    this.dgvCellsValueCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    this.dgvCellsTypeCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
            }
            if (invokeInvalidationMode) {
                this.ExitInvalidationMode();
            }
        }

        // Token: 0x0600005C RID: 92 RVA: 0x0000ACBC File Offset: 0x00008EBC
        private void button_ResetFilterCells_Click(object sender, EventArgs e) {
            this.dgvCellsNameCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            this.dgvCellsEditorNameCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            this.dgvCellsValueCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            this.dgvCellsTypeCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            bool hasSelectedFirstMatch = false;
            this.filter_Cells.Text = "";
            for (int i = 0; i < this.dgvCells.Rows.Count; i++) {
                DataGridViewRow dgv_row = this.dgvCells.Rows[i];
                dgv_row.Visible = true;
                dgv_row.Selected = false;
                if (!hasSelectedFirstMatch) {
                    dgv_row.Selected = true;
                    hasSelectedFirstMatch = true;
                }
            }
            this.dgvCellsNameCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            this.dgvCellsEditorNameCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            if (settings.CellView_ShowTypes) {
                this.dgvCellsValueCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                this.dgvCellsTypeCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                return;
            }
            this.dgvCellsValueCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.dgvCellsTypeCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        }

        // Token: 0x0600005D RID: 93 RVA: 0x0000ADAC File Offset: 0x00008FAC
        private static List<string> BuildViewList(string viewDir, string current_input) {
            var names = new List<string>();
            if (!Directory.Exists(viewDir)) {
                Utility.ShowError("Views directory not found.");
                return names;
            }
            foreach (FileInfo file in new DirectoryInfo(viewDir).GetFiles("*.txt")) {
                if (file.Name.ToLower().Contains(current_input)) {
                    using var reader = new StreamReader(file.FullName);
                    while (!reader.EndOfStream) {
                        string line = reader.ReadLine();
                        names.Add(line.ToString().ToLower());
                    }
                }
            }
            return names;
        }

        // Token: 0x0600005E RID: 94 RVA: 0x0000AE5C File Offset: 0x0000905C
        private void toggleFilterVisibilityToolStripMenuItem_Click(object sender, EventArgs e) {
            settings.EnableFilterBar = !settings.EnableFilterBar;
            if (settings.EnableFilterBar) {
                this.menuStrip2.Visible = true;
                this.menuStrip3.Visible = true;
                this.menuStrip4.Visible = true;
                return;
            }
            this.menuStrip2.Visible = false;
            this.menuStrip3.Visible = false;
            this.menuStrip4.Visible = false;
        }

        // Token: 0x0600005F RID: 95 RVA: 0x0000AEE4 File Offset: 0x000090E4
        private void GenerateProjectDirectories(string project) {
            _ = (GameMode)this.toolStripComboBoxGame.SelectedItem;
            string projectDir = $"Projects\\\\{project}";
            bool flag = Directory.Exists(projectDir);
            string[] folders = new string[] {
                "CSV",
                "Logs",
                "Names"
            };
            string[] gametypes = new string[] {
                "DS1",
                "DS1R",
                "DS2",
                "DS3",
                "SDT",
                "ER"
            };
            if (!flag) {
                _ = Directory.CreateDirectory(projectDir);
            }
            foreach (string folder in folders) {
                foreach (string type in gametypes) {
                    string dir = $"{projectDir}\\{folder}\\{type}";
                    _ = Directory.CreateDirectory(dir);
                }
            }
        }

        private void MakeDarkTheme(bool theme = true) {
            this.darkTheme = theme;
            if (theme || true) {
                this.BackColor = Color.Black;
                this.ForeColor = BlackThemeTextColor;
                this.dgvParams.EnableHeadersVisualStyles = false;
                this.dgvParams.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
                this.dgvParams.ColumnHeadersDefaultCellStyle.BackColor = ColumnHeadersBackColor;
                this.dgvParams.ColumnHeadersDefaultCellStyle.ForeColor = BlackThemeTextColor;
                this.dgvParams.DefaultCellStyle.BackColor = GridBackColor;
                this.dgvParams.DefaultCellStyle.ForeColor = BlackThemeTextColor;
                this.dgvParams.DefaultCellStyle.SelectionBackColor = GridSelectionBackColor;
                this.dgvParams.DefaultCellStyle.SelectionForeColor = BlackThemeTextColor;
                this.dgvParams.GridColor = GridBackColor;
                this.dgvParams.BackColor = Color.Black;
                this.dgvParams.BackgroundColor = Color.Black;
                this.dgvCells.EnableHeadersVisualStyles = false;
                this.dgvCells.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
                this.dgvCells.ColumnHeadersDefaultCellStyle.BackColor = ColumnHeadersBackColor;
                this.dgvCells.ColumnHeadersDefaultCellStyle.ForeColor = BlackThemeTextColor;
                this.dgvCells.DefaultCellStyle.BackColor = GridBackColor;
                this.dgvCells.DefaultCellStyle.ForeColor = BlackThemeTextColor;
                this.dgvCells.DefaultCellStyle.SelectionBackColor = GridSelectionBackColor;
                this.dgvCells.DefaultCellStyle.SelectionForeColor = BlackThemeTextColor;
                this.dgvCells.GridColor = GridBackColor;
                this.dgvCells.BackColor = Color.Black;
                this.dgvCells.BackgroundColor = Color.Black;
                foreach (DataGridViewRow row in this.dgvCells.Rows) {
                    DataGridViewCell cell = row.Cells[1];
                    cell.Style.BackColor = ControlsBackColor;
                }
                this.dgvRows.EnableHeadersVisualStyles = false;
                this.dgvRows.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
                this.dgvRows.ColumnHeadersDefaultCellStyle.BackColor = ColumnHeadersBackColor;
                this.dgvRows.ColumnHeadersDefaultCellStyle.ForeColor = BlackThemeTextColor;
                this.dgvRows.DefaultCellStyle.BackColor = GridBackColor;
                this.dgvRows.DefaultCellStyle.ForeColor = BlackThemeTextColor;
                this.dgvRows.DefaultCellStyle.SelectionBackColor = GridSelectionBackColor;
                this.dgvRows.DefaultCellStyle.SelectionForeColor = BlackThemeTextColor;
                this.dgvRows.GridColor = GridBackColor;
                this.dgvRows.BackColor = Color.Black;
                this.dgvRows.BackgroundColor = Color.Black;
                this.fileToolStripMenuItem.SetBackAndForeColors(Color.Black, Color.White);
                this.addRowToolStripMenuItem.SetBackAndForeColors(Color.Black, Color.White);
                this.ToolStripMenuItem.SetBackAndForeColors(Color.Black, Color.White);
                this.editToolStripMenuItem.SetBackAndForeColors(Color.Black, Color.White);
                this.secondaryDataToolStripMenuItem.SetBackAndForeColors(Color.Black, Color.White);
                this.WorkflowToolStripMenuItem.SetBackAndForeColors(Color.Black, Color.White);
                this.settingsMenuItem.SetBackAndForeColors(Color.Black, Color.White);
                this.viewToolStripMenuItem.SetBackAndForeColors(Color.Black, Color.White);
                //this.toolStripSeparator1.SetBackAndForeColors(BlackThemeTextColor, ControlsBackColor);
                //this.toolStripSeparator5.SetBackAndForeColors(BlackThemeTextColor, ControlsBackColor);
                //this.toolStripSeparator7.SetBackAndForeColors(BlackThemeTextColor, ControlsBackColor);
                //this.toolStripSeparator8.SetBackAndForeColors(BlackThemeTextColor, ControlsBackColor);
                //this.toolStripSeparator9.SetBackAndForeColors(BlackThemeTextColor, ControlsBackColor);
                //this.TopToolStripPanel.Renderer = new Globals.ToolStripCustomRenderer();
                this.statusStrip1.BackColor = ControlsBackColor;
                this.statusStrip1.ForeColor = BlackThemeTextColor;
                this.menuStrip1.BackColor = ControlsBackColor;
                this.menuStrip1.ForeColor = BlackThemeTextColor;
                this.menuStrip2.BackColor = ControlsBackColor;
                this.menuStrip2.ForeColor = BlackThemeTextColor;
                this.menuStrip3.BackColor = ControlsBackColor;
                this.menuStrip3.ForeColor = BlackThemeTextColor;
                this.menuStrip4.BackColor = ControlsBackColor;
                this.menuStrip4.ForeColor = BlackThemeTextColor;
                this.filter_Params.BackColor = ControlsBackColor;
                this.filter_Params.ForeColor = BlackThemeTextColor;
                this.filter_Cells.BackColor = ControlsBackColor;
                this.filter_Cells.ForeColor = BlackThemeTextColor;
                this.filter_Rows.BackColor = ControlsBackColor;
                this.filter_Rows.ForeColor = BlackThemeTextColor;
                this.button_FilterParams.BackColor = ButtonsBackColor;
                this.button_FilterParams.ForeColor = BlackThemeTextColor;
                this.button_FilterCells.BackColor = ButtonsBackColor;
                this.button_FilterCells.ForeColor = BlackThemeTextColor;
                this.button_FilterRows.BackColor = ButtonsBackColor;
                this.button_FilterRows.ForeColor = BlackThemeTextColor;
                this.menuStrip2.Renderer = new ToolStripProfessionalRenderer(new ProfessionalColorTable { UseSystemColors = false });
                this.button_ResetFilterParams.BackColor = ButtonsBackColor;
                this.button_ResetFilterParams.ForeColor = BlackThemeTextColor;
                this.button_ResetFilterCells.BackColor = ButtonsBackColor;
                this.button_ResetFilterCells.ForeColor = BlackThemeTextColor;
                this.button_ResetFilterRows.BackColor = ButtonsBackColor;
                this.button_ResetFilterRows.ForeColor = BlackThemeTextColor;
            } else {
                this.BackColor = Color.LightGray;
                this.ForeColor = Color.Black;
                this.menuStrip1.BackColor = Color.LightGray; // ????
                this.menuStrip1.ForeColor = Color.Black;
                this.menuStrip2.BackColor = Color.Transparent;
                this.menuStrip2.ForeColor = Color.Black;
                this.menuStrip3.BackColor = Color.Transparent;
                this.menuStrip3.ForeColor = Color.Black;
                this.menuStrip4.BackColor = Color.Transparent;
                this.menuStrip4.ForeColor = Color.Black;
                this.button_FilterParams.BackColor = Color.DarkGray;
                this.button_FilterParams.ForeColor = Color.Black;
                this.button_FilterCells.BackColor = Color.DarkGray;
                this.button_FilterCells.ForeColor = Color.Black;
                this.button_FilterRows.BackColor = Color.DarkGray;
                this.button_FilterRows.ForeColor = Color.Black;
                this.button_ResetFilterParams.BackColor = Color.DarkGray;
                this.button_ResetFilterParams.ForeColor = Color.Black;
                this.button_ResetFilterCells.BackColor = Color.DarkGray;
                this.button_ResetFilterCells.ForeColor = Color.Black;
                this.button_ResetFilterRows.BackColor = Color.DarkGray;
                this.button_ResetFilterRows.ForeColor = Color.Black;
            }
        }

        public class BlackThemeButtonColorTable : ProfessionalColorTable {
            public override Color ButtonCheckedHighlight => GridSelectionBackColor;
            public override Color ButtonPressedHighlight => GridSelectionBackColor;
            public override Color ButtonCheckedHighlightBorder => GridSelectionBackColor;
            public override Color ButtonPressedHighlightBorder => GridSelectionBackColor;

        }

        protected internal static readonly Color GridBackColor = Globals.FromARGB(0xFF080808);
        protected internal static readonly Color GridSelectionBackColor = Globals.FromARGB(0xFF383838);
        protected internal static readonly Color ControlsBackColor = Globals.FromARGB(0xFF101010);
        protected internal static readonly Color ColumnHeadersBackColor = Globals.FromARGB(0xFF202020);
        protected internal static readonly Color BlackThemeTextColor = Globals.FromARGB(0xFFD0D0D0);
        protected internal static readonly Color ButtonsBackColor = Globals.FromARGB(0xFF202020);
        protected internal static readonly Color ButtonsTextColor = Globals.FromARGB(0xFFFFFFFF);

        private static readonly nuint unpin_value = ~(nuint)1;

        // Token: 0x04000001 RID: 1
        public static readonly Settings settings = Settings.Default;

        // Token: 0x04000002 RID: 2
        private bool InvalidationMode;

        // Token: 0x04000003 RID: 3
        private string regulationPath;

        // Token: 0x04000004 RID: 4
        private IBinder regulation;

        // Token: 0x04000005 RID: 5
        private IBinder secondary_regulation;

        // Token: 0x04000006 RID: 6
        private bool encrypted;

        // Token: 0x04000007 RID: 7
        private bool secondary_encrypted;

        // Token: 0x04000008 RID: 8
        private readonly BindingSource rowSource = new();

        // Token: 0x04000009 RID: 9
        private readonly Dictionary<string, (int Row, int Cell)> dgvIndices = new();

        // Token: 0x0400000A RID: 10
        private string lastFindRowPattern = "";

        // Token: 0x0400000B RID: 11
        private string lastFindFieldPattern;

        // Token: 0x0400000C RID: 12
        private readonly int PARAM_NAME_COL;

        // Token: 0x0400000D RID: 13
        private readonly int ROW_ID_COL;

        // Token: 0x0400000E RID: 14
        private readonly int ROW_NAME_COL = 1;

        // Token: 0x0400000F RID: 15
        private readonly int FIELD_PARAM_NAME_COL;

        // Token: 0x04000010 RID: 16
        private readonly int FIELD_EDITOR_NAME_COL = 1;

        // Token: 0x04000011 RID: 17
        private readonly int FIELD_VALUE_COL = 2;

        // Token: 0x04000012 RID: 18
        private readonly int FIELD_TYPE_COL = 3;

        // Token: 0x04000013 RID: 19
        private LoadParamsResult primary_result;

        // Token: 0x04000014 RID: 20
        private LoadParamsResult secondary_result;

        // Token: 0x04000015 RID: 21
        private readonly List<PARAMDEF> paramdefs = new();

        // Token: 0x04000016 RID: 22
        private readonly List<PARAMTDF> paramtdfs = new();

        // Token: 0x04000017 RID: 23
        private readonly Dictionary<string, PARAMTDF> tdf_dict = new();

        // Token: 0x04000018 RID: 24
        private List<string> bool_type_tdfs = new();

        // Token: 0x04000019 RID: 25
        private List<string> custom_type_tdfs = new();

        protected internal static readonly SafeDictionary<string, SafeDictionary<string, string[]>> refs_dict = new(
            File.ReadAllText("./Paramdex/ER/defs_refs.refs")
                .Split("\n\n")
                .Select(s => s.Trim().Split(':'))
                .Select(strings =>
                    new KeyValuePair<string, SafeDictionary<string, string[]>>(strings[0],
                        new SafeDictionary<string, string[]>(strings[1]
                            .Trim()
                            .Split(';')
                            .Select(s => s.Split('='))
                            .Select(s => new KeyValuePair<string, string[]>(s[0].Trim(), s[1].Split(',').Select(s => s.Trim()).ToArray()))
                        )
                    )
                )
        );

        private bool darkTheme = true;

        private readonly ToolStripMenuItem[] GoToReferenceMenuStripItems;

        // Token: 0x0400001A RID: 26
        private readonly string ATKPARAM_PC = "AtkParam_Pc";

        // Token: 0x0400001B RID: 27
        private readonly string ATKPARAM_NPC = "AtkParam_Npc";

        // Token: 0x0400001C RID: 28
        private readonly string BEHAVIORPARAM_PC = "BehaviorParam_PC";

        // Token: 0x0400001D RID: 29
        private readonly string BEHAVIORPARAM_NPC = "BehaviorParam";

        public static Main Instance { get; set; }

        // Token: 0x02000019 RID: 25
        public class LoadParamsResult {
            // Token: 0x17000051 RID: 81
            // (get) Token: 0x0600015F RID: 351 RVA: 0x000157CC File Offset: 0x000139CC
            // (set) Token: 0x06000160 RID: 352 RVA: 0x000157D4 File Offset: 0x000139D4
            public bool Encrypted;

            // Token: 0x17000052 RID: 82
            // (get) Token: 0x06000161 RID: 353 RVA: 0x000157DD File Offset: 0x000139DD
            // (set) Token: 0x06000162 RID: 354 RVA: 0x000157E5 File Offset: 0x000139E5
            public IBinder ParamBND;

            // Token: 0x17000053 RID: 83
            // (get) Token: 0x06000163 RID: 355 RVA: 0x000157EE File Offset: 0x000139EE
            // (set) Token: 0x06000164 RID: 356 RVA: 0x000157F6 File Offset: 0x000139F6
            [Bindable(true)]
            public List<ParamWrapper> ParamWrappers;
        }

        [GeneratedRegex("[^,]+")]
        private static partial Regex DCVIndicesRegex();
        [GeneratedRegex("\\s*[\\r\\n]+\\s*")]
        private static partial Regex SplitWhitespacedLines();
        [GeneratedRegex("^(\\d+) (.+)$")]
        private static partial Regex IDRegex();
        [GeneratedRegex(".*[0-9].*")]
        private static partial Regex NumberRegex();
    }
}
