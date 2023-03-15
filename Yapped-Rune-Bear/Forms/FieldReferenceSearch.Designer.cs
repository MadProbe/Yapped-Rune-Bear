namespace Chomp.Forms
{
	// Token: 0x0200000F RID: 15
	public partial class FieldReferenceSearch : global::System.Windows.Forms.Form
	{
		// Token: 0x06000129 RID: 297 RVA: 0x000121AA File Offset: 0x000103AA
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x0600012A RID: 298 RVA: 0x000121CC File Offset: 0x000103CC
		private void InitializeComponent()
		{
			this.label1 = new global::System.Windows.Forms.Label();
			this.btnCreate = new global::System.Windows.Forms.Button();
			this.btnCancel = new global::System.Windows.Forms.Button();
			this.textbox_referenceText = new global::System.Windows.Forms.TextBox();
			global::System.Windows.Forms.Label lblName = new global::System.Windows.Forms.Label();
			base.SuspendLayout();
			lblName.AutoSize = true;
			lblName.Location = new global::System.Drawing.Point(12, 9);
			lblName.Name = "lblName";
			lblName.Size = new global::System.Drawing.Size(79, 13);
			lblName.TabIndex = 5;
			lblName.Text = "Number to Find";
			this.label1.AutoSize = true;
			this.label1.Location = new global::System.Drawing.Point(12, 58);
			this.label1.Name = "label1";
			this.label1.Size = new global::System.Drawing.Size(0, 13);
			this.label1.TabIndex = 7;
			this.btnCreate.Location = new global::System.Drawing.Point(12, 53);
			this.btnCreate.Name = "btnCreate";
			this.btnCreate.Size = new global::System.Drawing.Size(75, 23);
			this.btnCreate.TabIndex = 2;
			this.btnCreate.Text = "OK";
			this.btnCreate.UseVisualStyleBackColor = true;
			this.btnCreate.Click += new global::System.EventHandler(this.btnCreate_Click);
			this.btnCancel.DialogResult = global::System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new global::System.Drawing.Point(143, 53);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new global::System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new global::System.EventHandler(this.btnCancel_Click);
			this.textbox_referenceText.Location = new global::System.Drawing.Point(13, 25);
			this.textbox_referenceText.Name = "textbox_referenceText";
			this.textbox_referenceText.Size = new global::System.Drawing.Size(204, 20);
			this.textbox_referenceText.TabIndex = 6;
			base.AcceptButton = this.btnCreate;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = this.btnCancel;
			base.ClientSize = new global::System.Drawing.Size(229, 85);
			base.Controls.Add(this.label1);
			base.Controls.Add(this.textbox_referenceText);
			base.Controls.Add(lblName);
			base.Controls.Add(this.btnCancel);
			base.Controls.Add(this.btnCreate);
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			base.Name = "FormReferenceFinder";
			base.StartPosition = global::System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Reference Finder";
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		// Token: 0x040000D2 RID: 210
		private global::System.ComponentModel.IContainer components;

		// Token: 0x040000D3 RID: 211
		private global::System.Windows.Forms.Button btnCreate;

		// Token: 0x040000D4 RID: 212
		private global::System.Windows.Forms.Button btnCancel;

		// Token: 0x040000D5 RID: 213
		private global::System.Windows.Forms.TextBox textbox_referenceText;

		// Token: 0x040000D6 RID: 214
		private global::System.Windows.Forms.Label label1;
	}
}
