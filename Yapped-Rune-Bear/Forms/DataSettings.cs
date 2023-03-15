using Chomp.Properties;

namespace Chomp.Forms {
    // Token: 0x0200000C RID: 12
    public partial class DataSettings : Form {
        // Token: 0x06000116 RID: 278 RVA: 0x0000FF5C File Offset: 0x0000E15C
        public DataSettings() {
            this.InitializeComponent();
            this.checkbox_IncludeHeader.Checked = Settings.Default.IncludeHeaderInCSV;
            this.checkbox_IncludeRowNames.Checked = Settings.Default.IncludeRowNameInCSV;
            this.checkbox_ExportUniqueOnly.Checked = Settings.Default.ExportUniqueOnly;
            this.checkbox_UnfurledCSVExport.Checked = Settings.Default.VerboseCSVExport;
            this.textbox_CSV_Delimiter.Text = Settings.Default.ExportDelimiter;
            this.checkbox_EnableFieldValidation.Checked = Settings.Default.EnableFieldValidation;
        }

        // Token: 0x06000117 RID: 279 RVA: 0x0000FFF4 File Offset: 0x0000E1F4
        private void btnSaveSettings_Click(object sender, EventArgs e) {
            if (this.textbox_CSV_Delimiter.Text == "") {
                this.textbox_CSV_Delimiter.Text = ";";
                _ = MessageBox.Show("CSV Delimiter cannot be blank. It has been reset to ;", "Settings", MessageBoxButtons.OK);
            }
            Settings.Default.ExportDelimiter = this.textbox_CSV_Delimiter.Text;
            Settings.Default.IncludeHeaderInCSV = this.checkbox_IncludeHeader.Checked;
            Settings.Default.IncludeRowNameInCSV = this.checkbox_IncludeRowNames.Checked;
            Settings.Default.ExportUniqueOnly = this.checkbox_ExportUniqueOnly.Checked;
            Settings.Default.VerboseCSVExport = this.checkbox_UnfurledCSVExport.Checked;
            Settings.Default.EnableFieldValidation = this.checkbox_EnableFieldValidation.Checked;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // Token: 0x06000118 RID: 280 RVA: 0x000100C4 File Offset: 0x0000E2C4
        private void btnCancel_Click(object sender, EventArgs e) => this.Close();
    }
}
