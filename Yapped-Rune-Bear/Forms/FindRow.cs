namespace Chomp.Forms {
    // Token: 0x02000012 RID: 18
    public partial class FindRow : Form {
        // Token: 0x06000135 RID: 309 RVA: 0x00012C27 File Offset: 0x00010E27
        public FindRow(string prompt) {
            this.InitializeComponent();
            this.Text = prompt;
            this.DialogResult = DialogResult.Cancel;
        }

        // Token: 0x06000136 RID: 310 RVA: 0x00012C43 File Offset: 0x00010E43
        private void btnFind_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.OK;
            this.ResultPattern = this.txtPattern.Text;
            this.Close();
        }

        // Token: 0x06000137 RID: 311 RVA: 0x00012C63 File Offset: 0x00010E63
        private void btnCancel_Click(object sender, EventArgs e) => this.Close();

        // Token: 0x040000E4 RID: 228
        public string ResultPattern;
    }
}
