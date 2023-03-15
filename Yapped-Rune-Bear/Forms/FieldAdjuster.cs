using Chomp.Properties;

namespace Chomp.Forms {
    // Token: 0x0200000D RID: 13
    public partial class FieldAdjuster : Form {
        // Token: 0x0600011B RID: 283 RVA: 0x000107E4 File Offset: 0x0000E9E4
        public FieldAdjuster() {
            this.InitializeComponent();
            this.DialogResult = DialogResult.Cancel;
            if (Settings.Default.FieldAdjuster_RetainFieldText) {
                this.textbox_FieldMatch.Text = Settings.Default.FieldAdjuster_FieldMatch;
                this.textbox_RowRange.Text = Settings.Default.FieldAdjuster_RowRange;
                this.textbox_RowPartialMatch.Text = Settings.Default.FieldAdjuster_RowPartialMatch;
                this.textbox_FieldMinimum.Text = Settings.Default.FieldAdjuster_FieldMinimum;
                this.textbox_FieldMaximum.Text = Settings.Default.FieldAdjuster_FieldMaximum;
                this.textbox_FieldExclusion.Text = Settings.Default.FieldAdjuster_FieldExclusion;
                this.textbox_FieldInclusion.Text = Settings.Default.FieldAdjuster_FieldInclusion;
                this.textbox_Formula.Text = Settings.Default.FieldAdjuster_Formula;
                this.textbox_ValueMin.Text = Settings.Default.FieldAdjuster_ValueMin;
                this.textbox_ValueMax.Text = Settings.Default.FieldAdjuster_ValueMax;
                this.checkbox_RetainFieldText.Checked = Settings.Default.FieldAdjuster_RetainFieldText;
                return;
            }
            this.textbox_FieldMatch.Text = "";
            this.textbox_RowRange.Text = "";
            this.textbox_RowPartialMatch.Text = "";
            this.textbox_FieldMinimum.Text = "";
            this.textbox_FieldMaximum.Text = "";
            this.textbox_FieldExclusion.Text = "";
            this.textbox_FieldInclusion.Text = "";
            this.textbox_Formula.Text = "";
            this.textbox_ValueMin.Text = "";
            this.textbox_ValueMax.Text = "";
        }

        // Token: 0x0600011C RID: 284 RVA: 0x0001099C File Offset: 0x0000EB9C
        private void btnCreate_Click(object sender, EventArgs e) {
            Settings.Default.FieldAdjuster_FieldMatch = this.textbox_FieldMatch.Text;
            Settings.Default.FieldAdjuster_RowRange = this.textbox_RowRange.Text;
            Settings.Default.FieldAdjuster_RowPartialMatch = this.textbox_RowPartialMatch.Text;
            Settings.Default.FieldAdjuster_FieldMinimum = this.textbox_FieldMinimum.Text;
            Settings.Default.FieldAdjuster_FieldMaximum = this.textbox_FieldMaximum.Text;
            Settings.Default.FieldAdjuster_FieldExclusion = this.textbox_FieldExclusion.Text;
            Settings.Default.FieldAdjuster_FieldInclusion = this.textbox_FieldInclusion.Text;
            Settings.Default.FieldAdjuster_Formula = this.textbox_Formula.Text;
            Settings.Default.FieldAdjuster_ValueMin = this.textbox_ValueMin.Text;
            Settings.Default.FieldAdjuster_ValueMax = this.textbox_ValueMax.Text;
            Settings.Default.FieldAdjuster_RetainFieldText = this.checkbox_RetainFieldText.Checked;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // Token: 0x0600011D RID: 285 RVA: 0x00010A9D File Offset: 0x0000EC9D
        private void btnCancel_Click(object sender, EventArgs e) => this.Close();
    }
}
