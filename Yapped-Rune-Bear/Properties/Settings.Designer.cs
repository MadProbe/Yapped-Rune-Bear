using System;
using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Chomp.Properties {
    // Token: 0x02000005 RID: 5
    [CompilerGenerated]
    [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "11.0.0.0")]
    public unsafe sealed partial class Settings : ApplicationSettingsBase {
        // Token: 0x17000003 RID: 3
        // (get) Token: 0x06000068 RID: 104 RVA: 0x0000E1FD File Offset: 0x0000C3FD
        public static Settings Default => Settings.defaultInstance;

        // Token: 0x17000004 RID: 4
        // (get) Token: 0x06000069 RID: 105 RVA: 0x0000E204 File Offset: 0x0000C404
        // (set) Token: 0x0600006A RID: 106 RVA: 0x0000E216 File Offset: 0x0000C416
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("ExampleMod")]
        public string ProjectName {
            get => (string)this["ProjectName"];
            set => this["ProjectName"] = value;
        }

        // Token: 0x17000005 RID: 5
        // (get) Token: 0x0600006B RID: 107 RVA: 0x0000E224 File Offset: 0x0000C424
        // (set) Token: 0x0600006C RID: 108 RVA: 0x0000E236 File Offset: 0x0000C436
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("200, 200")]
        public Point WindowLocation {
            get {
                return (Point)this["WindowLocation"];
            }
            set {
                this["WindowLocation"] = value;
            }
        }

        // Token: 0x17000006 RID: 6
        // (get) Token: 0x0600006D RID: 109 RVA: 0x0000E249 File Offset: 0x0000C449
        // (set) Token: 0x0600006E RID: 110 RVA: 0x0000E25B File Offset: 0x0000C45B
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("776, 584")]
        public Size WindowSize {
            get {
                return (Size)this["WindowSize"];
            }
            set {
                this["WindowSize"] = value;
            }
        }

        // Token: 0x17000007 RID: 7
        // (get) Token: 0x0600006F RID: 111 RVA: 0x0000E26E File Offset: 0x0000C46E
        // (set) Token: 0x06000070 RID: 112 RVA: 0x0000E280 File Offset: 0x0000C480
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("False")]
        public bool WindowMaximized {
            get => (bool)this["WindowMaximized"];
            set => this["WindowMaximized"] = value;
        }

        // Token: 0x17000008 RID: 8
        // (get) Token: 0x06000071 RID: 113 RVA: 0x0000E293 File Offset: 0x0000C493
        // (set) Token: 0x06000072 RID: 114 RVA: 0x0000E2A5 File Offset: 0x0000C4A5
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("C:\\Program Files (x86)\\Steam\\steamapps\\common\\ELDEN RING\\Game\\regulation.bin")]
        public string RegulationPath {
            get => (string)this["RegulationPath"];
            set => this["RegulationPath"] = value;
        }

        // Token: 0x17000009 RID: 9
        // (get) Token: 0x06000073 RID: 115 RVA: 0x0000E2B3 File Offset: 0x0000C4B3
        // (set) Token: 0x06000074 RID: 116 RVA: 0x0000E2C5 File Offset: 0x0000C4C5
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("250")]
        public int SplitterDistance1 {
            get => (int)this["SplitterDistance1"];
            set => this["SplitterDistance1"] = value;
        }

        // Token: 0x1700000A RID: 10
        // (get) Token: 0x06000075 RID: 117 RVA: 0x0000E2D8 File Offset: 0x0000C4D8
        // (set) Token: 0x06000076 RID: 118 RVA: 0x0000E2EA File Offset: 0x0000C4EA
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("0")]
        public int SelectedParam {
            get => (int)this["SelectedParam"];
            set => this["SelectedParam"] = value;
        }

        // Token: 0x1700000B RID: 11
        // (get) Token: 0x06000077 RID: 119 RVA: 0x0000E2FD File Offset: 0x0000C4FD
        // (set) Token: 0x06000078 RID: 120 RVA: 0x0000E30F File Offset: 0x0000C50F
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("0")]
        public int SelectedRow {
            get => (int)this["SelectedRow"];
            set => this["SelectedRow"] = value;
        }

        // Token: 0x1700000C RID: 12
        // (get) Token: 0x06000079 RID: 121 RVA: 0x0000E322 File Offset: 0x0000C522
        // (set) Token: 0x0600007A RID: 122 RVA: 0x0000E334 File Offset: 0x0000C534
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("0")]
        public int SelectedField {
            get => (int)this["SelectedField"];
            set => this["SelectedField"] = value;
        }

        // Token: 0x1700000D RID: 13
        // (get) Token: 0x0600007B RID: 123 RVA: 0x0000E347 File Offset: 0x0000C547
        // (set) Token: 0x0600007C RID: 124 RVA: 0x0000E359 File Offset: 0x0000C559
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("True")]
        public bool VerifyRowDeletion {
            get => (bool)this["VerifyRowDeletion"];
            set => this["VerifyRowDeletion"] = value;
        }

        // Token: 0x1700000E RID: 14
        // (get) Token: 0x0600007D RID: 125 RVA: 0x0000E36C File Offset: 0x0000C56C
        // (set) Token: 0x0600007E RID: 126 RVA: 0x0000E37E File Offset: 0x0000C57E
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("True")]
        public bool IncludeHeaderInCSV {
            get => (bool)this["IncludeHeaderInCSV"];
            set => this["IncludeHeaderInCSV"] = value;
        }

        // Token: 0x1700000F RID: 15
        // (get) Token: 0x0600007F RID: 127 RVA: 0x0000E391 File Offset: 0x0000C591
        // (set) Token: 0x06000080 RID: 128 RVA: 0x0000E3A3 File Offset: 0x0000C5A3
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("True")]
        public bool IncludeRowNameInCSV {
            get => (bool)this["IncludeRowNameInCSV"];
            set => this["IncludeRowNameInCSV"] = value;
        }

        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("True")]
        public bool DarkTheme {
            get => (bool)this["DarkTheme"];
            set => this["DarkTheme"] = value;
        }

        // Token: 0x17000010 RID: 16
        // (get) Token: 0x06000081 RID: 129 RVA: 0x0000E3B6 File Offset: 0x0000C5B6
        // (set) Token: 0x06000082 RID: 130 RVA: 0x0000E3C8 File Offset: 0x0000C5C8
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue(";")]
        public string ExportDelimiter {
            get => (string)this["ExportDelimiter"];
            set => this["ExportDelimiter"] = value;
        }

        // Token: 0x17000011 RID: 17
        // (get) Token: 0x06000083 RID: 131 RVA: 0x0000E3D6 File Offset: 0x0000C5D6
        // (set) Token: 0x06000084 RID: 132 RVA: 0x0000E3E8 File Offset: 0x0000C5E8
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("249")]
        public int SplitterDistance2 {
            get => (int)this["SplitterDistance2"];
            set => this["SplitterDistance2"] = value;
        }

        // Token: 0x17000012 RID: 18
        // (get) Token: 0x06000085 RID: 133 RVA: 0x0000E3FB File Offset: 0x0000C5FB
        // (set) Token: 0x06000086 RID: 134 RVA: 0x0000E40D File Offset: 0x0000C60D
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string DGVIndices {
            get => (string)this["DGVIndices"];
            set => this["DGVIndices"] = value;
        }

        // Token: 0x17000013 RID: 19
        // (get) Token: 0x06000087 RID: 135 RVA: 0x0000E41B File Offset: 0x0000C61B
        // (set) Token: 0x06000088 RID: 136 RVA: 0x0000E42D File Offset: 0x0000C62D
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("False")]
        public bool ShowCommonParamsOnly {
            get => (bool)this["ShowCommonParamsOnly"];
            set => this["ShowCommonParamsOnly"] = value;
        }

        // Token: 0x17000014 RID: 20
        // (get) Token: 0x06000089 RID: 137 RVA: 0x0000E440 File Offset: 0x0000C640
        // (set) Token: 0x0600008A RID: 138 RVA: 0x0000E452 File Offset: 0x0000C652
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("False")]
        public bool ChangedCommonParamView {
            get => (bool)this["ChangedCommonParamView"];
            set => this["ChangedCommonParamView"] = value;
        }

        // Token: 0x17000015 RID: 21
        // (get) Token: 0x0600008B RID: 139 RVA: 0x0000E465 File Offset: 0x0000C665
        // (set) Token: 0x0600008C RID: 140 RVA: 0x0000E477 File Offset: 0x0000C677
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string ParamsToIgnoreDuringSave {
            get => (string)this["ParamsToIgnoreDuringSave"];
            set => this["ParamsToIgnoreDuringSave"] = value;
        }

        // Token: 0x17000016 RID: 22
        // (get) Token: 0x0600008D RID: 141 RVA: 0x0000E485 File Offset: 0x0000C685
        // (set) Token: 0x0600008E RID: 142 RVA: 0x0000E497 File Offset: 0x0000C697
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("EldenRing")]
        public string GameType {
            get => (string)this["GameType"];
            set => this["GameType"] = value;
        }

        // Token: 0x17000017 RID: 23
        // (get) Token: 0x0600008F RID: 143 RVA: 0x0000E4A5 File Offset: 0x0000C6A5
        // (set) Token: 0x06000090 RID: 144 RVA: 0x0000E4B7 File Offset: 0x0000C6B7
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("False")]
        public bool ShowConfirmationMessages {
            get => (bool)this["ShowConfirmationMessages"];
            set => this["ShowConfirmationMessages"] = value;
        }

        // Token: 0x17000018 RID: 24
        // (get) Token: 0x06000091 RID: 145 RVA: 0x0000E4CA File Offset: 0x0000C6CA
        // (set) Token: 0x06000092 RID: 146 RVA: 0x0000E4DC File Offset: 0x0000C6DC
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("False")]
        public bool UseTextEditor {
            get => (bool)this["UseTextEditor"];
            set => this["UseTextEditor"] = value;
        }

        // Token: 0x17000019 RID: 25
        // (get) Token: 0x06000093 RID: 147 RVA: 0x0000E4EF File Offset: 0x0000C6EF
        // (set) Token: 0x06000094 RID: 148 RVA: 0x0000E501 File Offset: 0x0000C701
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("False")]
        public bool FieldExporter_RetainFieldText {
            get => (bool)this["FieldExporter_RetainFieldText"];
            set => this["FieldExporter_RetainFieldText"] = value;
        }

        // Token: 0x1700001A RID: 26
        // (get) Token: 0x06000095 RID: 149 RVA: 0x0000E514 File Offset: 0x0000C714
        // (set) Token: 0x06000096 RID: 150 RVA: 0x0000E526 File Offset: 0x0000C726
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string FieldExporter_FieldMatch {
            get => (string)this["FieldExporter_FieldMatch"];
            set => this["FieldExporter_FieldMatch"] = value;
        }

        // Token: 0x1700001B RID: 27
        // (get) Token: 0x06000097 RID: 151 RVA: 0x0000E534 File Offset: 0x0000C734
        // (set) Token: 0x06000098 RID: 152 RVA: 0x0000E546 File Offset: 0x0000C746
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string FieldExporter_FieldMinimum {
            get => (string)this["FieldExporter_FieldMinimum"];
            set => this["FieldExporter_FieldMinimum"] = value;
        }

        // Token: 0x1700001C RID: 28
        // (get) Token: 0x06000099 RID: 153 RVA: 0x0000E554 File Offset: 0x0000C754
        // (set) Token: 0x0600009A RID: 154 RVA: 0x0000E566 File Offset: 0x0000C766
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string FieldExporter_FieldMaximum {
            get => (string)this["FieldExporter_FieldMaximum"];
            set => this["FieldExporter_FieldMaximum"] = value;
        }

        // Token: 0x1700001D RID: 29
        // (get) Token: 0x0600009B RID: 155 RVA: 0x0000E574 File Offset: 0x0000C774
        // (set) Token: 0x0600009C RID: 156 RVA: 0x0000E586 File Offset: 0x0000C786
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string FieldExporter_FieldExclusion {
            get => (string)this["FieldExporter_FieldExclusion"];
            set => this["FieldExporter_FieldExclusion"] = value;
        }

        // Token: 0x1700001E RID: 30
        // (get) Token: 0x0600009D RID: 157 RVA: 0x0000E594 File Offset: 0x0000C794
        // (set) Token: 0x0600009E RID: 158 RVA: 0x0000E5A6 File Offset: 0x0000C7A6
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string FieldExporter_FieldInclusion {
            get => (string)this["FieldExporter_FieldInclusion"];
            set => this["FieldExporter_FieldInclusion"] = value;
        }

        // Token: 0x1700001F RID: 31
        // (get) Token: 0x0600009F RID: 159 RVA: 0x0000E5B4 File Offset: 0x0000C7B4
        // (set) Token: 0x060000A0 RID: 160 RVA: 0x0000E5C6 File Offset: 0x0000C7C6
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string TextEditorPath {
            get => (string)this["TextEditorPath"];
            set => this["TextEditorPath"] = value;
        }

        // Token: 0x17000020 RID: 32
        // (get) Token: 0x060000A1 RID: 161 RVA: 0x0000E5D4 File Offset: 0x0000C7D4
        // (set) Token: 0x060000A2 RID: 162 RVA: 0x0000E5E6 File Offset: 0x0000C7E6
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string SecondaryFilePath {
            get => (string)this["SecondaryFilePath"];
            set => this["SecondaryFilePath"] = value;
        }

        // Token: 0x17000021 RID: 33
        // (get) Token: 0x060000A3 RID: 163 RVA: 0x0000E5F4 File Offset: 0x0000C7F4
        // (set) Token: 0x060000A4 RID: 164 RVA: 0x0000E606 File Offset: 0x0000C806
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("False")]
        public bool ParamDifferenceMode {
            get => (bool)this["ParamDifferenceMode"];
            set => this["ParamDifferenceMode"] = value;
        }

        // Token: 0x17000022 RID: 34
        // (get) Token: 0x060000A5 RID: 165 RVA: 0x0000E619 File Offset: 0x0000C819
        // (set) Token: 0x060000A6 RID: 166 RVA: 0x0000E62B File Offset: 0x0000C82B
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string FieldAdjuster_FieldMatch {
            get => (string)this["FieldAdjuster_FieldMatch"];
            set => this["FieldAdjuster_FieldMatch"] = value;
        }

        // Token: 0x17000023 RID: 35
        // (get) Token: 0x060000A7 RID: 167 RVA: 0x0000E639 File Offset: 0x0000C839
        // (set) Token: 0x060000A8 RID: 168 RVA: 0x0000E64B File Offset: 0x0000C84B
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string FieldAdjuster_RowRange {
            get => (string)this["FieldAdjuster_RowRange"];
            set => this["FieldAdjuster_RowRange"] = value;
        }

        // Token: 0x17000024 RID: 36
        // (get) Token: 0x060000A9 RID: 169 RVA: 0x0000E659 File Offset: 0x0000C859
        // (set) Token: 0x060000AA RID: 170 RVA: 0x0000E66B File Offset: 0x0000C86B
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string FieldAdjuster_RowPartialMatch {
            get => (string)this["FieldAdjuster_RowPartialMatch"];
            set => this["FieldAdjuster_RowPartialMatch"] = value;
        }

        // Token: 0x17000025 RID: 37
        // (get) Token: 0x060000AB RID: 171 RVA: 0x0000E679 File Offset: 0x0000C879
        // (set) Token: 0x060000AC RID: 172 RVA: 0x0000E68B File Offset: 0x0000C88B
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string FieldAdjuster_FieldMinimum {
            get => (string)this["FieldAdjuster_FieldMinimum"];
            set => this["FieldAdjuster_FieldMinimum"] = value;
        }

        // Token: 0x17000026 RID: 38
        // (get) Token: 0x060000AD RID: 173 RVA: 0x0000E699 File Offset: 0x0000C899
        // (set) Token: 0x060000AE RID: 174 RVA: 0x0000E6AB File Offset: 0x0000C8AB
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string FieldAdjuster_FieldMaximum {
            get => (string)this["FieldAdjuster_FieldMaximum"];
            set => this["FieldAdjuster_FieldMaximum"] = value;
        }

        // Token: 0x17000027 RID: 39
        // (get) Token: 0x060000AF RID: 175 RVA: 0x0000E6B9 File Offset: 0x0000C8B9
        // (set) Token: 0x060000B0 RID: 176 RVA: 0x0000E6CB File Offset: 0x0000C8CB
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string FieldAdjuster_FieldExclusion {
            get => (string)this["FieldAdjuster_FieldExclusion"];
            set => this["FieldAdjuster_FieldExclusion"] = value;
        }

        // Token: 0x17000028 RID: 40
        // (get) Token: 0x060000B1 RID: 177 RVA: 0x0000E6D9 File Offset: 0x0000C8D9
        // (set) Token: 0x060000B2 RID: 178 RVA: 0x0000E6EB File Offset: 0x0000C8EB
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string FieldAdjuster_FieldInclusion {
            get => (string)this["FieldAdjuster_FieldInclusion"];
            set => this["FieldAdjuster_FieldInclusion"] = value;
        }

        // Token: 0x17000029 RID: 41
        // (get) Token: 0x060000B3 RID: 179 RVA: 0x0000E6F9 File Offset: 0x0000C8F9
        // (set) Token: 0x060000B4 RID: 180 RVA: 0x0000E70B File Offset: 0x0000C90B
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string FieldAdjuster_Formula {
            get => (string)this["FieldAdjuster_Formula"];
            set => this["FieldAdjuster_Formula"] = value;
        }

        // Token: 0x1700002A RID: 42
        // (get) Token: 0x060000B5 RID: 181 RVA: 0x0000E719 File Offset: 0x0000C919
        // (set) Token: 0x060000B6 RID: 182 RVA: 0x0000E72B File Offset: 0x0000C92B
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string FieldAdjuster_ValueMin {
            get => (string)this["FieldAdjuster_ValueMin"];
            set => this["FieldAdjuster_ValueMin"] = value;
        }

        // Token: 0x1700002B RID: 43
        // (get) Token: 0x060000B7 RID: 183 RVA: 0x0000E739 File Offset: 0x0000C939
        // (set) Token: 0x060000B8 RID: 184 RVA: 0x0000E74B File Offset: 0x0000C94B
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string FieldAdjuster_ValueMax {
            get => (string)this["FieldAdjuster_ValueMax"];
            set => this["FieldAdjuster_ValueMax"] = value;
        }

        // Token: 0x1700002C RID: 44
        // (get) Token: 0x060000B9 RID: 185 RVA: 0x0000E759 File Offset: 0x0000C959
        // (set) Token: 0x060000BA RID: 186 RVA: 0x0000E76B File Offset: 0x0000C96B
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("False")]
        public bool FieldAdjuster_RetainFieldText {
            get => (bool)this["FieldAdjuster_RetainFieldText"];
            set => this["FieldAdjuster_RetainFieldText"] = value;
        }

        // Token: 0x1700002D RID: 45
        // (get) Token: 0x060000BB RID: 187 RVA: 0x0000E77E File Offset: 0x0000C97E
        // (set) Token: 0x060000BC RID: 188 RVA: 0x0000E790 File Offset: 0x0000C990
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("False")]
        public bool Settings_LogParamSize {
            get => (bool)this["Settings_LogParamSize"];
            set => this["Settings_LogParamSize"] = value;
        }

        // Token: 0x1700002E RID: 46
        // (get) Token: 0x060000BD RID: 189 RVA: 0x0000E7A3 File Offset: 0x0000C9A3
        // (set) Token: 0x060000BE RID: 190 RVA: 0x0000E7B5 File Offset: 0x0000C9B5
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("False")]
        public bool SaveWithoutEncryption {
            get => (bool)this["SaveWithoutEncryption"];
            set => this["SaveWithoutEncryption"] = value;
        }

        // Token: 0x1700002F RID: 47
        // (get) Token: 0x060000BF RID: 191 RVA: 0x0000E7C8 File Offset: 0x0000C9C8
        // (set) Token: 0x060000C0 RID: 192 RVA: 0x0000E7DA File Offset: 0x0000C9DA
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("False")]
        public bool ExportUniqueOnly {
            get => (bool)this["ExportUniqueOnly"];
            set => this["ExportUniqueOnly"] = value;
        }

        // Token: 0x17000030 RID: 48
        // (get) Token: 0x060000C1 RID: 193 RVA: 0x0000E7ED File Offset: 0x0000C9ED
        // (set) Token: 0x060000C2 RID: 194 RVA: 0x0000E7FF File Offset: 0x0000C9FF
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("True")]
        public bool CellView_ShowEditorNames {
            get => (bool)this["CellView_ShowEditorNames"];
            set => this["CellView_ShowEditorNames"] = value;
        }

        // Token: 0x17000031 RID: 49
        // (get) Token: 0x060000C3 RID: 195 RVA: 0x0000E812 File Offset: 0x0000CA12
        // (set) Token: 0x060000C4 RID: 196 RVA: 0x0000E824 File Offset: 0x0000CA24
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("False")]
        public bool CellView_ShowTypes {
            get => (bool)this["CellView_ShowTypes"];
            set => this["CellView_ShowTypes"] = value;
        }

        // Token: 0x17000032 RID: 50
        // (get) Token: 0x060000C5 RID: 197 RVA: 0x0000E837 File Offset: 0x0000CA37
        // (set) Token: 0x060000C6 RID: 198 RVA: 0x0000E849 File Offset: 0x0000CA49
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("True")]
        public bool ShowFieldDescriptions {
            get => (bool)this["ShowFieldDescriptions"];
            set => this["ShowFieldDescriptions"] = value;
        }

        // Token: 0x17000033 RID: 51
        // (get) Token: 0x060000C7 RID: 199 RVA: 0x0000E85C File Offset: 0x0000CA5C
        // (set) Token: 0x060000C8 RID: 200 RVA: 0x0000E86E File Offset: 0x0000CA6E
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("False")]
        public bool VerboseCSVExport {
            get => (bool)this["VerboseCSVExport"];
            set => this["VerboseCSVExport"] = value;
        }

        // Token: 0x17000034 RID: 52
        // (get) Token: 0x060000C9 RID: 201 RVA: 0x0000E881 File Offset: 0x0000CA81
        // (set) Token: 0x060000CA RID: 202 RVA: 0x0000E893 File Offset: 0x0000CA93
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("255")]
        public int FieldColor_Int_R {
            get => (int)this["FieldColor_Int_R"];
            set => this["FieldColor_Int_R"] = value;
        }

        // Token: 0x17000035 RID: 53
        // (get) Token: 0x060000CB RID: 203 RVA: 0x0000E8A6 File Offset: 0x0000CAA6
        // (set) Token: 0x060000CC RID: 204 RVA: 0x0000E8B8 File Offset: 0x0000CAB8
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("255")]
        public int FieldColor_Int_G {
            get => (int)this["FieldColor_Int_G"];
            set => this["FieldColor_Int_G"] = value;
        }

        // Token: 0x17000036 RID: 54
        // (get) Token: 0x060000CD RID: 205 RVA: 0x0000E8CB File Offset: 0x0000CACB
        // (set) Token: 0x060000CE RID: 206 RVA: 0x0000E8DD File Offset: 0x0000CADD
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("255")]
        public int FieldColor_Int_B {
            get => (int)this["FieldColor_Int_B"];
            set => this["FieldColor_Int_B"] = value;
        }

        // Token: 0x17000037 RID: 55
        // (get) Token: 0x060000CF RID: 207 RVA: 0x0000E8F0 File Offset: 0x0000CAF0
        // (set) Token: 0x060000D0 RID: 208 RVA: 0x0000E902 File Offset: 0x0000CB02
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("255")]
        public int FieldColor_Float_R {
            get => (int)this["FieldColor_Float_R"];
            set => this["FieldColor_Float_R"] = value;
        }

        // Token: 0x17000038 RID: 56
        // (get) Token: 0x060000D1 RID: 209 RVA: 0x0000E915 File Offset: 0x0000CB15
        // (set) Token: 0x060000D2 RID: 210 RVA: 0x0000E927 File Offset: 0x0000CB27
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("255")]
        public int FieldColor_Float_G {
            get => (int)this["FieldColor_Float_G"];
            set => this["FieldColor_Float_G"] = value;
        }

        // Token: 0x17000039 RID: 57
        // (get) Token: 0x060000D3 RID: 211 RVA: 0x0000E93A File Offset: 0x0000CB3A
        // (set) Token: 0x060000D4 RID: 212 RVA: 0x0000E94C File Offset: 0x0000CB4C
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("255")]
        public int FieldColor_Float_B {
            get => (int)this["FieldColor_Float_B"];
            set => this["FieldColor_Float_B"] = value;
        }

        // Token: 0x1700003A RID: 58
        // (get) Token: 0x060000D5 RID: 213 RVA: 0x0000E95F File Offset: 0x0000CB5F
        // (set) Token: 0x060000D6 RID: 214 RVA: 0x0000E971 File Offset: 0x0000CB71
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("230")]
        public int FieldColor_Bool_R {
            get => (int)this["FieldColor_Bool_R"];
            set => this["FieldColor_Bool_R"] = value;
        }

        // Token: 0x1700003B RID: 59
        // (get) Token: 0x060000D7 RID: 215 RVA: 0x0000E984 File Offset: 0x0000CB84
        // (set) Token: 0x060000D8 RID: 216 RVA: 0x0000E996 File Offset: 0x0000CB96
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("230")]
        public int FieldColor_Bool_G {
            get => (int)this["FieldColor_Bool_G"];
            set => this["FieldColor_Bool_G"] = value;
        }

        // Token: 0x1700003C RID: 60
        // (get) Token: 0x060000D9 RID: 217 RVA: 0x0000E9A9 File Offset: 0x0000CBA9
        // (set) Token: 0x060000DA RID: 218 RVA: 0x0000E9BB File Offset: 0x0000CBBB
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("230")]
        public int FieldColor_Bool_B {
            get => (int)this["FieldColor_Bool_B"];
            set => this["FieldColor_Bool_B"] = value;
        }

        // Token: 0x1700003D RID: 61
        // (get) Token: 0x060000DB RID: 219 RVA: 0x0000E9CE File Offset: 0x0000CBCE
        // (set) Token: 0x060000DC RID: 220 RVA: 0x0000E9E0 File Offset: 0x0000CBE0
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("255")]
        public int FieldColor_String_R {
            get => (int)this["FieldColor_String_R"];
            set => this["FieldColor_String_R"] = value;
        }

        // Token: 0x1700003E RID: 62
        // (get) Token: 0x060000DD RID: 221 RVA: 0x0000E9F3 File Offset: 0x0000CBF3
        // (set) Token: 0x060000DE RID: 222 RVA: 0x0000EA05 File Offset: 0x0000CC05
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("255")]
        public int FieldColor_String_G {
            get => (int)this["FieldColor_String_G"];
            set => this["FieldColor_String_G"] = value;
        }

        // Token: 0x1700003F RID: 63
        // (get) Token: 0x060000DF RID: 223 RVA: 0x0000EA18 File Offset: 0x0000CC18
        // (set) Token: 0x060000E0 RID: 224 RVA: 0x0000EA2A File Offset: 0x0000CC2A
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("255")]
        public int FieldColor_String_B {
            get => (int)this["FieldColor_String_B"];
            set => this["FieldColor_String_B"] = value;
        }

        // Token: 0x17000040 RID: 64
        // (get) Token: 0x060000E1 RID: 225 RVA: 0x0000EA3D File Offset: 0x0000CC3D
        // (set) Token: 0x060000E2 RID: 226 RVA: 0x0000EA4F File Offset: 0x0000CC4F
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("True")]
        public bool ShowEnums {
            get => (bool)this["ShowEnums"];
            set => this["ShowEnums"] = value;

        }

        // Token: 0x17000041 RID: 65
        // (get) Token: 0x060000E3 RID: 227 RVA: 0x0000EA62 File Offset: 0x0000CC62
        // (set) Token: 0x060000E4 RID: 228 RVA: 0x0000EA74 File Offset: 0x0000CC74
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("True")]
        public bool ShowEnumValueInName {
            get => (bool)this["ShowEnumValueInName"];
            set => this["ShowEnumValueInName"] = value;
        }

        // Token: 0x17000042 RID: 66
        // (get) Token: 0x060000E5 RID: 229 RVA: 0x0000EA87 File Offset: 0x0000CC87
        // (set) Token: 0x060000E6 RID: 230 RVA: 0x0000EA99 File Offset: 0x0000CC99
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("False")]
        public bool EnableFieldValidation {
            get => (bool)this["EnableFieldValidation"];
            set => this["EnableFieldValidation"] = value;
        }

        // Token: 0x17000043 RID: 67
        // (get) Token: 0x060000E7 RID: 231 RVA: 0x0000EAAC File Offset: 0x0000CCAC
        // (set) Token: 0x060000E8 RID: 232 RVA: 0x0000EABE File Offset: 0x0000CCBE
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("True")]
        public bool EnableFilterBar {
            get => (bool)this["EnableFilterBar"];
            set => this["EnableFilterBar"] = value;
        }

        // Token: 0x17000044 RID: 68
        // (get) Token: 0x060000E9 RID: 233 RVA: 0x0000EAD1 File Offset: 0x0000CCD1
        // (set) Token: 0x060000EA RID: 234 RVA: 0x0000EAE3 File Offset: 0x0000CCE3
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue(":")]
        public string Filter_CommandDelimiter {
            get => (string)this["Filter_CommandDelimiter"];
            set => this["Filter_CommandDelimiter"] = value;
        }

        // Token: 0x17000045 RID: 69
        // (get) Token: 0x060000EB RID: 235 RVA: 0x0000EAF1 File Offset: 0x0000CCF1
        // (set) Token: 0x060000EC RID: 236 RVA: 0x0000EB03 File Offset: 0x0000CD03
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("~")]
        public string Filter_SectionDelimiter {
            get => (string)this["Filter_SectionDelimiter"];
            set => this["Filter_SectionDelimiter"] = value;
        }

        // Token: 0x17000046 RID: 70
        // (get) Token: 0x060000ED RID: 237 RVA: 0x0000EB11 File Offset: 0x0000CD11
        // (set) Token: 0x060000EE RID: 238 RVA: 0x0000EB23 File Offset: 0x0000CD23
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("True")]
        public bool DisplayBooleanEnumAsCheckbox {
            get => (bool)this["DisplayBooleanEnumAsCheckbox"];
            set => this["DisplayBooleanEnumAsCheckbox"] = value;
        }

        // Token: 0x17000047 RID: 71
        // (get) Token: 0x060000EF RID: 239 RVA: 0x0000EB36 File Offset: 0x0000CD36
        // (set) Token: 0x060000F0 RID: 240 RVA: 0x0000EB48 File Offset: 0x0000CD48
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("True")]
        public bool DisableEnumForCustomTypes {
            get => (bool)this["DisableEnumForCustomTypes"];
            set => this["DisableEnumForCustomTypes"] = value;
        }

        // Token: 0x17000048 RID: 72
        // (get) Token: 0x060000F1 RID: 241 RVA: 0x0000EB5B File Offset: 0x0000CD5B
        // (set) Token: 0x060000F2 RID: 242 RVA: 0x0000EB6D File Offset: 0x0000CD6D
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("0")]
        public int NewRow_RepeatCount {
            get => (int)this["NewRow_RepeatCount"];
            set => this["NewRow_RepeatCount"] = value;
        }

        // Token: 0x17000049 RID: 73
        // (get) Token: 0x060000F3 RID: 243 RVA: 0x0000EB80 File Offset: 0x0000CD80
        // (set) Token: 0x060000F4 RID: 244 RVA: 0x0000EB92 File Offset: 0x0000CD92
        [UserScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("0")]
        public int NewRow_StepValue {
            get => (int)this["NewRow_StepValue"];
            set => this["NewRow_StepValue"] = value;
        }

        // Token: 0x04000084 RID: 132
        private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());
    }
}
