using Chomp.Properties;

namespace Chomp.Forms {
    // Token: 0x0200000E RID: 14
    public partial class FieldExporter : Form {
        // Token: 0x06000120 RID: 288 RVA: 0x000118DC File Offset: 0x0000FADC
        public FieldExporter() {
            this.InitializeComponent();
            this.DialogResult = DialogResult.Cancel;
            this.checkbox_RetainFieldText.Checked = Settings.Default.FieldExporter_RetainFieldText;
            if (this.checkbox_RetainFieldText.Checked) {
                this.textbox_FieldMatch.Text = Settings.Default.FieldExporter_FieldMatch;
                this.textbox_FieldMinimum.Text = Settings.Default.FieldExporter_FieldMinimum;
                this.textbox_FieldMaximum.Text = Settings.Default.FieldExporter_FieldMaximum;
                this.textbox_FieldExclusions.Text = Settings.Default.FieldExporter_FieldExclusion;
                this.textbox_FieldInclusions.Text = Settings.Default.FieldExporter_FieldInclusion;
                return;
            }
            this.textbox_FieldMatch.Text = "";
            this.textbox_FieldMinimum.Text = "";
            this.textbox_FieldMaximum.Text = "";
            this.textbox_FieldExclusions.Text = "";
            this.textbox_FieldInclusions.Text = "";
        }

        // Token: 0x06000121 RID: 289 RVA: 0x000119D8 File Offset: 0x0000FBD8
        private void btnCreate_Click(object sender, EventArgs e) {
            Settings.Default.FieldExporter_FieldMatch = this.textbox_FieldMatch.Text;
            Settings.Default.FieldExporter_FieldMinimum = this.textbox_FieldMinimum.Text;
            Settings.Default.FieldExporter_FieldMaximum = this.textbox_FieldMaximum.Text;
            Settings.Default.FieldExporter_FieldExclusion = this.textbox_FieldExclusions.Text;
            Settings.Default.FieldExporter_FieldInclusion = this.textbox_FieldInclusions.Text;
            Settings.Default.FieldExporter_RetainFieldText = this.checkbox_RetainFieldText.Checked;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // Token: 0x06000122 RID: 290 RVA: 0x00011A70 File Offset: 0x0000FC70
        private void btnCancel_Click(object sender, EventArgs e) => this.Close();
    }
}
