using Chomp.Properties;

namespace Chomp.Forms {
    // Token: 0x02000015 RID: 21
    public partial class NewRow : Form {
        // Token: 0x06000149 RID: 329 RVA: 0x000141B0 File Offset: 0x000123B0
        public NewRow(string prompt) {
            this.InitializeComponent();
            this.Text = prompt;
            this.DialogResult = DialogResult.Cancel;
            this.textbox_RepeatCount.Text = Settings.Default.NewRow_RepeatCount.ToString();
            this.textbox_StepValue.Text = Settings.Default.NewRow_StepValue.ToString();
        }

        // Token: 0x0600014A RID: 330 RVA: 0x00014211 File Offset: 0x00012411
        public NewRow(string prompt, int row_id, string row_name) {
            this.InitializeComponent();
            this.Text = prompt;
            this.nudID.Text = row_id.ToString();
            this.txtName.Text = row_name.ToString();
            this.DialogResult = DialogResult.Cancel;
        }

        // Token: 0x0600014B RID: 331 RVA: 0x00014250 File Offset: 0x00012450
        private void btnCreate_Click(object sender, EventArgs e) {
            this.ResultID = (int)this.nudID.Value;
            this.ResultName = (this.txtName.Text.Length > 0) ? this.txtName.Text : null;
            Settings.Default.NewRow_RepeatCount = Convert.ToInt32(this.textbox_RepeatCount.Text);
            Settings.Default.NewRow_StepValue = Convert.ToInt32(this.textbox_StepValue.Text);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // Token: 0x0600014C RID: 332 RVA: 0x000142DB File Offset: 0x000124DB
        private void btnCancel_Click(object sender, EventArgs e) => this.Close();

        // Token: 0x0600014D RID: 333 RVA: 0x000142E3 File Offset: 0x000124E3
        private void nudID_Enter(object sender, EventArgs e) => this.nudID.Select(0, this.nudID.Text.Length);

        // Token: 0x04000104 RID: 260
        public int ResultID;

        // Token: 0x04000105 RID: 261
        public string ResultName;
    }
}
