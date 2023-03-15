namespace Chomp.Forms {
    // Token: 0x02000013 RID: 19
    public partial class GoToRow : Form {
        // Token: 0x0600013A RID: 314 RVA: 0x00012EAB File Offset: 0x000110AB
        public GoToRow() {
            this.InitializeComponent();
            this.DialogResult = DialogResult.Cancel;
        }

        // Token: 0x0600013B RID: 315 RVA: 0x00012EC0 File Offset: 0x000110C0
        private void btnGoto_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.OK;
            this.ResultID = (long)this.nudID.Value;
            this.Close();
        }

        // Token: 0x0600013C RID: 316 RVA: 0x00012EE5 File Offset: 0x000110E5
        private void btnCancel_Click(object sender, EventArgs e) => this.Close();

        // Token: 0x0600013D RID: 317 RVA: 0x00012EED File Offset: 0x000110ED
        private void nudID_Enter(object sender, EventArgs e) => this.nudID.Select(0, this.nudID.Text.Length);

        // Token: 0x040000E9 RID: 233
        public long ResultID;
    }
}
