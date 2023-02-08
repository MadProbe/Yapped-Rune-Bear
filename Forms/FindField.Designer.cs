namespace Chomp.Forms {
    // Token: 0x02000011 RID: 17
    public partial class FindField : global::System.Windows.Forms.Form {
        // Token: 0x06000133 RID: 307 RVA: 0x000129E6 File Offset: 0x00010BE6
        protected override void Dispose(bool disposing) {
            if (disposing && this.components != null) {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        // Token: 0x06000134 RID: 308 RVA: 0x00012A08 File Offset: 0x00010C08
        private void InitializeComponent() {
            this.btnFind = new global::System.Windows.Forms.Button();
            this.btnCancel = new global::System.Windows.Forms.Button();
            this.txtPattern = new global::System.Windows.Forms.TextBox();
            base.SuspendLayout();
            this.btnFind.Location = new global::System.Drawing.Point(12, 38);
            this.btnFind.Name = "btnFind";
            this.btnFind.Size = new global::System.Drawing.Size(75, 23);
            this.btnFind.TabIndex = 1;
            this.btnFind.Text = "Find";
            this.btnFind.UseVisualStyleBackColor = true;
            this.btnFind.Click += new global::System.EventHandler(this.btnFind_Click);
            this.btnCancel.DialogResult = global::System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new global::System.Drawing.Point(93, 38);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new global::System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new global::System.EventHandler(this.btnCancel_Click);
            this.txtPattern.Location = new global::System.Drawing.Point(12, 12);
            this.txtPattern.Name = "txtPattern";
            this.txtPattern.Size = new global::System.Drawing.Size(156, 20);
            this.txtPattern.TabIndex = 0;
            base.AcceptButton = this.btnFind;
            base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
            base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
            base.CancelButton = this.btnCancel;
            base.ClientSize = new global::System.Drawing.Size(180, 73);
            base.Controls.Add(this.txtPattern);
            base.Controls.Add(this.btnCancel);
            base.Controls.Add(this.btnFind);
            base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            base.Name = "FormFind";
            base.StartPosition = global::System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Find row with name...";
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        // Token: 0x040000E0 RID: 224
        private global::System.ComponentModel.IContainer components;

        // Token: 0x040000E1 RID: 225
        private global::System.Windows.Forms.Button btnFind;

        // Token: 0x040000E2 RID: 226
        private global::System.Windows.Forms.Button btnCancel;

        // Token: 0x040000E3 RID: 227
        private global::System.Windows.Forms.TextBox txtPattern;
    }
}
