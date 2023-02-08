namespace Chomp.Forms {
    // Token: 0x02000016 RID: 22
    public partial class RowReferenceSearch : Form {
        // Token: 0x06000150 RID: 336 RVA: 0x0001493B File Offset: 0x00012B3B
        public RowReferenceSearch() {
            this.InitializeComponent();
            this.DialogResult = DialogResult.Cancel;
        }

        // Token: 0x06000151 RID: 337 RVA: 0x00014950 File Offset: 0x00012B50
        private void btnCreate_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // Token: 0x06000152 RID: 338 RVA: 0x0001495F File Offset: 0x00012B5F
        private void btnCancel_Click(object sender, EventArgs e) => this.Close();

        // Token: 0x06000153 RID: 339 RVA: 0x00014967 File Offset: 0x00012B67
        public string GetReferenceText() => this.textbox_referenceText.Text;
    }
}
