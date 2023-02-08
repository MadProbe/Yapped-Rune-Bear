namespace Chomp.Forms
{
	// Token: 0x02000012 RID: 18
	public partial class FindRow : global::System.Windows.Forms.Form
	{
		// Token: 0x06000138 RID: 312 RVA: 0x00012C6B File Offset: 0x00010E6B
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000139 RID: 313 RVA: 0x00012C8C File Offset: 0x00010E8C
		private void InitializeComponent()
		{
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

		// Token: 0x040000E5 RID: 229
		private global::System.ComponentModel.IContainer components;

		// Token: 0x040000E6 RID: 230
		private global::System.Windows.Forms.Button btnFind;

		// Token: 0x040000E7 RID: 231
		private global::System.Windows.Forms.Button btnCancel;

		// Token: 0x040000E8 RID: 232
		private global::System.Windows.Forms.TextBox txtPattern;
	}
}
