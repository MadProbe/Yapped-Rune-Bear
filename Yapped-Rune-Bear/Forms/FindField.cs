namespace Chomp.Forms {
    // Token: 0x02000011 RID: 17
    public partial class FindField : Form {
        // Token: 0x06000130 RID: 304 RVA: 0x000129A2 File Offset: 0x00010BA2
        public FindField(string prompt) {
            this.InitializeComponent();
            this.Text = prompt;
            this.DialogResult = DialogResult.Cancel;
        }

        // Token: 0x06000131 RID: 305 RVA: 0x000129BE File Offset: 0x00010BBE
        private void btnFind_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.OK;
            this.ResultPattern = this.txtPattern.Text;
            this.Close();
        }

        // Token: 0x06000132 RID: 306 RVA: 0x000129DE File Offset: 0x00010BDE
        private void btnCancel_Click(object sender, EventArgs e) => this.Close();

        // Token: 0x040000DF RID: 223
        public string ResultPattern;
    }
}
