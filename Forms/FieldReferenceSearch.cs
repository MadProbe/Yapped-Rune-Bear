namespace Chomp.Forms {
    // Token: 0x0200000F RID: 15
    public partial class FieldReferenceSearch : Form {
        // Token: 0x06000125 RID: 293 RVA: 0x00012171 File Offset: 0x00010371
        public FieldReferenceSearch() {
            this.InitializeComponent();
            this.DialogResult = DialogResult.Cancel;
        }

        // Token: 0x06000126 RID: 294 RVA: 0x00012186 File Offset: 0x00010386
        private void btnCreate_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // Token: 0x06000127 RID: 295 RVA: 0x00012195 File Offset: 0x00010395
        private void btnCancel_Click(object sender, EventArgs e) => this.Close();

        // Token: 0x06000128 RID: 296 RVA: 0x0001219D File Offset: 0x0001039D
        public string GetReferenceText() => this.textbox_referenceText.Text;
    }
}
