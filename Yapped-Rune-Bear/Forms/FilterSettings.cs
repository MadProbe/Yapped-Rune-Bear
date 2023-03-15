using Chomp.Properties;

namespace Chomp.Forms {
    // Token: 0x02000010 RID: 16
    public partial class FilterSettings : Form {
        // Token: 0x0600012B RID: 299 RVA: 0x000124AD File Offset: 0x000106AD
        public FilterSettings() {
            this.InitializeComponent();
            this.textbox_Filter_CommandDelimiter.Text = Settings.Default.Filter_CommandDelimiter;
            this.textbox_Filter_SectionDelimiter.Text = Settings.Default.Filter_SectionDelimiter;
        }

        // Token: 0x0600012C RID: 300 RVA: 0x000124E8 File Offset: 0x000106E8
        private void btnSaveSettings_Click(object sender, EventArgs e) {
            string command_delimiter = this.textbox_Filter_CommandDelimiter.Text;
            string section_delimiter = this.textbox_Filter_SectionDelimiter.Text;
            if (command_delimiter == "") {
                command_delimiter = ":";
            }
            if (section_delimiter == "") {
                section_delimiter = "~";
            }
            Settings.Default.Filter_CommandDelimiter = command_delimiter;
            Settings.Default.Filter_SectionDelimiter = section_delimiter;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // Token: 0x0600012D RID: 301 RVA: 0x00012556 File Offset: 0x00010756
        private void btnCancel_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
