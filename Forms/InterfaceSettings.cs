using Chomp.Properties;

namespace Chomp.Forms {
    // Token: 0x02000014 RID: 20
    public partial class InterfaceSettings : Form {
        // Token: 0x06000140 RID: 320 RVA: 0x000131A4 File Offset: 0x000113A4
        public InterfaceSettings() {
            this.InitializeComponent();
            var int_color = Color.FromArgb(Settings.Default.FieldColor_Int_R, Settings.Default.FieldColor_Int_G, Settings.Default.FieldColor_Int_B);
            var float_color = Color.FromArgb(Settings.Default.FieldColor_Float_R, Settings.Default.FieldColor_Float_G, Settings.Default.FieldColor_Float_B);
            var bool_color = Color.FromArgb(Settings.Default.FieldColor_Bool_R, Settings.Default.FieldColor_Bool_G, Settings.Default.FieldColor_Bool_B);
            var string_color = Color.FromArgb(Settings.Default.FieldColor_String_R, Settings.Default.FieldColor_String_G, Settings.Default.FieldColor_String_B);
            this.colorDialog_FieldValue_Int.Color = int_color;
            this.colorDialog_FieldValue_Float.Color = float_color;
            this.colorDialog_FieldValue_Bool.Color = bool_color;
            this.colorDialog_FieldValue_String.Color = string_color;
            this.textBox_FieldColor_Int.BackColor = this.colorDialog_FieldValue_Int.Color;
            this.textBox_FieldColor_Float.BackColor = this.colorDialog_FieldValue_Float.Color;
            this.textBox_FieldColor_Bool.BackColor = this.colorDialog_FieldValue_Bool.Color;
            this.textBox_FieldColor_String.BackColor = this.colorDialog_FieldValue_String.Color;
            this.checkbox_DisplayEnums.Checked = Settings.Default.ShowEnums;
            this.checkbox_EnumValueInName.Checked = Settings.Default.ShowEnumValueInName;
            this.checkbox_FieldDescriptions.Checked = Settings.Default.ShowFieldDescriptions;
            this.checkbox_BooleanEnumToggle.Checked = Settings.Default.DisplayBooleanEnumAsCheckbox;
            this.checkbox_customizableEnumToggle.Checked = Settings.Default.DisableEnumForCustomTypes;
        }

        // Token: 0x06000141 RID: 321 RVA: 0x00013340 File Offset: 0x00011540
        private void button_FieldColor_int_Click(object sender, EventArgs e) {
            this.colorDialog_FieldValue_Int.AllowFullOpen = true;
            this.colorDialog_FieldValue_Int.ShowHelp = true;
            this.colorDialog_FieldValue_Int.Color = this.textBox_FieldColor_Int.BackColor;
            if (this.colorDialog_FieldValue_Int.ShowDialog() == DialogResult.OK) {
                this.textBox_FieldColor_Int.BackColor = this.colorDialog_FieldValue_Int.Color;
            }
        }

        // Token: 0x06000142 RID: 322 RVA: 0x000133A0 File Offset: 0x000115A0
        private void button_FieldColor_Float_Click(object sender, EventArgs e) {
            this.colorDialog_FieldValue_Float.AllowFullOpen = true;
            this.colorDialog_FieldValue_Float.ShowHelp = true;
            this.colorDialog_FieldValue_Float.Color = this.textBox_FieldColor_Float.BackColor;
            if (this.colorDialog_FieldValue_Float.ShowDialog() == DialogResult.OK) {
                this.textBox_FieldColor_Float.BackColor = this.colorDialog_FieldValue_Float.Color;
            }
        }

        // Token: 0x06000143 RID: 323 RVA: 0x00013400 File Offset: 0x00011600
        private void button_FieldColor_Bool_Click(object sender, EventArgs e) {
            this.colorDialog_FieldValue_Bool.AllowFullOpen = true;
            this.colorDialog_FieldValue_Bool.ShowHelp = true;
            this.colorDialog_FieldValue_Bool.Color = this.textBox_FieldColor_Bool.BackColor;
            if (this.colorDialog_FieldValue_Bool.ShowDialog() == DialogResult.OK) {
                this.textBox_FieldColor_Bool.BackColor = this.colorDialog_FieldValue_Bool.Color;
            }
        }

        // Token: 0x06000144 RID: 324 RVA: 0x00013460 File Offset: 0x00011660
        private void button_FieldColor_String_Click(object sender, EventArgs e) {
            this.colorDialog_FieldValue_String.AllowFullOpen = true;
            this.colorDialog_FieldValue_String.ShowHelp = true;
            this.colorDialog_FieldValue_String.Color = this.textBox_FieldColor_String.BackColor;
            if (this.colorDialog_FieldValue_String.ShowDialog() == DialogResult.OK) {
                this.textBox_FieldColor_String.BackColor = this.colorDialog_FieldValue_String.Color;
            }
        }

        // Token: 0x06000145 RID: 325 RVA: 0x000134C0 File Offset: 0x000116C0
        private void btnSaveSettings_Click(object sender, EventArgs e) {
            Settings.Default.FieldColor_Int_R = this.colorDialog_FieldValue_Int.Color.R;
            Settings.Default.FieldColor_Int_G = this.colorDialog_FieldValue_Int.Color.G;
            Settings.Default.FieldColor_Int_B = this.colorDialog_FieldValue_Int.Color.B;
            Settings.Default.FieldColor_Float_R = this.colorDialog_FieldValue_Float.Color.R;
            Settings.Default.FieldColor_Float_G = this.colorDialog_FieldValue_Float.Color.G;
            Settings.Default.FieldColor_Float_B = this.colorDialog_FieldValue_Float.Color.B;
            Settings.Default.FieldColor_Bool_R = this.colorDialog_FieldValue_Bool.Color.R;
            Settings.Default.FieldColor_Bool_G = this.colorDialog_FieldValue_Bool.Color.G;
            Settings.Default.FieldColor_Bool_B = this.colorDialog_FieldValue_Bool.Color.B;
            Settings.Default.FieldColor_String_R = this.colorDialog_FieldValue_String.Color.R;
            Settings.Default.FieldColor_String_G = this.colorDialog_FieldValue_String.Color.G;
            Settings.Default.FieldColor_String_B = this.colorDialog_FieldValue_String.Color.B;
            Settings.Default.ShowEnums = this.checkbox_DisplayEnums.Checked;
            Settings.Default.ShowEnumValueInName = this.checkbox_EnumValueInName.Checked;
            Settings.Default.ShowFieldDescriptions = this.checkbox_FieldDescriptions.Checked;
            Settings.Default.DisplayBooleanEnumAsCheckbox = this.checkbox_BooleanEnumToggle.Checked;
            Settings.Default.DisableEnumForCustomTypes = this.checkbox_customizableEnumToggle.Checked;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // Token: 0x06000146 RID: 326 RVA: 0x0001369F File Offset: 0x0001189F
        private void btnCancel_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
