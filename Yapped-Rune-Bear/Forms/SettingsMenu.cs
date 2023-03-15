using Chomp.Properties;

namespace Chomp.Forms {
    // Token: 0x02000017 RID: 23
    public partial class SettingsMenu : Form {
        // Token: 0x06000156 RID: 342 RVA: 0x00014C78 File Offset: 0x00012E78
        public SettingsMenu() {
            this.InitializeComponent();
            this.DialogResult = DialogResult.Cancel;
            this.textbox_ProjectName.Text = Settings.Default.ProjectName;
            this.textbox_TextEditor.Text = Settings.Default.TextEditorPath.ToString();
            this.textEditorPath.FileName = Settings.Default.TextEditorPath;
            this.checkbox_VerifyRowDeletion.Checked = Settings.Default.VerifyRowDeletion;
            this.checkbox_SuppressConfirmations.Checked = Settings.Default.ShowConfirmationMessages;
            this.checkbox_UseTextEditor.Checked = Settings.Default.UseTextEditor;
            this.checkbox_SaveNoEncryption.Checked = Settings.Default.SaveWithoutEncryption;
        }

        // Token: 0x06000157 RID: 343 RVA: 0x00014D30 File Offset: 0x00012F30
        private void btnCreate_Click(object sender, EventArgs e) {
            if (this.textbox_ProjectName.Text == "") {
                this.textbox_ProjectName.Text = "ExampleMod";
                _ = MessageBox.Show("Project Name cannot be blank. It has been reset to ExampleMod", "Settings", MessageBoxButtons.OK);
            }
            Settings.Default.VerifyRowDeletion = this.checkbox_VerifyRowDeletion.Checked;
            Settings.Default.ProjectName = this.textbox_ProjectName.Text;
            Settings.Default.TextEditorPath = this.textEditorPath.FileName;
            Settings.Default.ShowConfirmationMessages = this.checkbox_SuppressConfirmations.Checked;
            Settings.Default.UseTextEditor = this.checkbox_UseTextEditor.Checked;
            Settings.Default.SaveWithoutEncryption = this.checkbox_SaveNoEncryption.Checked;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // Token: 0x06000158 RID: 344 RVA: 0x00014E00 File Offset: 0x00013000
        private void btnCancel_Click(object sender, EventArgs e) => this.Close();

        // Token: 0x06000159 RID: 345 RVA: 0x00014E08 File Offset: 0x00013008
        private void label1_Click(object sender, EventArgs e) {
        }

        // Token: 0x0600015A RID: 346 RVA: 0x00014E0A File Offset: 0x0001300A
        private void FormSettings_Load(object sender, EventArgs e) {
        }

        // Token: 0x0600015B RID: 347 RVA: 0x00014E0C File Offset: 0x0001300C
        private void button_SelectTextEditor_Click(object sender, EventArgs e) {
            this.textEditorPath.FileName = "";
            if (this.textEditorPath.ShowDialog() == DialogResult.OK) {
                Settings.Default.TextEditorPath = this.textEditorPath.FileName;
            }
            this.textbox_TextEditor.Text = Settings.Default.TextEditorPath.ToString();
        }
    }
}
