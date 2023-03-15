namespace Chomp.Forms
{
	// Token: 0x02000013 RID: 19
	public partial class GoToRow : global::System.Windows.Forms.Form
	{
		// Token: 0x0600013E RID: 318 RVA: 0x00012F0B File Offset: 0x0001110B
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x0600013F RID: 319 RVA: 0x00012F2C File Offset: 0x0001112C
		private void InitializeComponent()
		{
			this.nudID = new global::System.Windows.Forms.NumericUpDown();
			this.btnGoto = new global::System.Windows.Forms.Button();
			this.btnCancel = new global::System.Windows.Forms.Button();
			((global::System.ComponentModel.ISupportInitialize)this.nudID).BeginInit();
			base.SuspendLayout();
			this.nudID.Location = new global::System.Drawing.Point(12, 12);
			global::System.Windows.Forms.NumericUpDown numericUpDown = this.nudID;
			int[] array = new int[4];
			array[0] = 1215752192;
			array[1] = 23;
			numericUpDown.Maximum = new decimal(array);
			this.nudID.Name = "nudID";
			this.nudID.Size = new global::System.Drawing.Size(156, 20);
			this.nudID.TabIndex = 0;
			this.nudID.TextAlign = global::System.Windows.Forms.HorizontalAlignment.Right;
			this.nudID.Enter += new global::System.EventHandler(this.nudID_Enter);
			this.btnGoto.Location = new global::System.Drawing.Point(12, 38);
			this.btnGoto.Name = "btnGoto";
			this.btnGoto.Size = new global::System.Drawing.Size(75, 23);
			this.btnGoto.TabIndex = 1;
			this.btnGoto.Text = "Goto";
			this.btnGoto.UseVisualStyleBackColor = true;
			this.btnGoto.Click += new global::System.EventHandler(this.btnGoto_Click);
			this.btnCancel.DialogResult = global::System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new global::System.Drawing.Point(93, 38);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new global::System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 2;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new global::System.EventHandler(this.btnCancel_Click);
			base.AcceptButton = this.btnGoto;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = this.btnCancel;
			base.ClientSize = new global::System.Drawing.Size(180, 73);
			base.Controls.Add(this.btnCancel);
			base.Controls.Add(this.btnGoto);
			base.Controls.Add(this.nudID);
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			base.Name = "FormGoto";
			base.StartPosition = global::System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Go to row ID...";
			((global::System.ComponentModel.ISupportInitialize)this.nudID).EndInit();
			base.ResumeLayout(false);
		}

		// Token: 0x040000EA RID: 234
		private global::System.ComponentModel.IContainer components;

		// Token: 0x040000EB RID: 235
		private global::System.Windows.Forms.NumericUpDown nudID;

		// Token: 0x040000EC RID: 236
		private global::System.Windows.Forms.Button btnGoto;

		// Token: 0x040000ED RID: 237
		private global::System.Windows.Forms.Button btnCancel;
	}
}
