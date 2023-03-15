namespace Chomp.Forms
{
	// Token: 0x02000015 RID: 21
	public partial class NewRow : global::System.Windows.Forms.Form
	{
		// Token: 0x0600014E RID: 334 RVA: 0x00014301 File Offset: 0x00012501
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x0600014F RID: 335 RVA: 0x00014320 File Offset: 0x00012520
		private void InitializeComponent()
		{
			this.lblID = new global::System.Windows.Forms.Label();
			this.lblName = new global::System.Windows.Forms.Label();
			this.nudID = new global::System.Windows.Forms.NumericUpDown();
			this.btnCreate = new global::System.Windows.Forms.Button();
			this.btnCancel = new global::System.Windows.Forms.Button();
			this.txtName = new global::System.Windows.Forms.TextBox();
			this.label1 = new global::System.Windows.Forms.Label();
			this.label2 = new global::System.Windows.Forms.Label();
			this.textbox_RepeatCount = new global::System.Windows.Forms.NumericUpDown();
			this.textbox_StepValue = new global::System.Windows.Forms.NumericUpDown();
			((global::System.ComponentModel.ISupportInitialize)this.nudID).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.textbox_RepeatCount).BeginInit();
			((global::System.ComponentModel.ISupportInitialize)this.textbox_StepValue).BeginInit();
			base.SuspendLayout();
			this.lblID.AutoSize = true;
			this.lblID.Location = new global::System.Drawing.Point(12, 9);
			this.lblID.Name = "lblID";
			this.lblID.Size = new global::System.Drawing.Size(18, 13);
			this.lblID.TabIndex = 4;
			this.lblID.Text = "ID";
			this.lblName.AutoSize = true;
			this.lblName.Location = new global::System.Drawing.Point(12, 87);
			this.lblName.Name = "lblName";
			this.lblName.Size = new global::System.Drawing.Size(81, 13);
			this.lblName.TabIndex = 5;
			this.lblName.Text = "Name (optional)";
			this.nudID.Location = new global::System.Drawing.Point(12, 25);
			global::System.Windows.Forms.NumericUpDown numericUpDown = this.nudID;
			int[] array = new int[4];
			array[0] = 1215752192;
			array[1] = 23;
			numericUpDown.Maximum = new decimal(array);
			this.nudID.Name = "nudID";
			this.nudID.Size = new global::System.Drawing.Size(210, 20);
			this.nudID.TabIndex = 0;
			this.nudID.TextAlign = global::System.Windows.Forms.HorizontalAlignment.Right;
			this.nudID.Enter += new global::System.EventHandler(this.nudID_Enter);
			this.btnCreate.Location = new global::System.Drawing.Point(15, 129);
			this.btnCreate.Name = "btnCreate";
			this.btnCreate.Size = new global::System.Drawing.Size(75, 23);
			this.btnCreate.TabIndex = 2;
			this.btnCreate.Text = "Create";
			this.btnCreate.UseVisualStyleBackColor = true;
			this.btnCreate.Click += new global::System.EventHandler(this.btnCreate_Click);
			this.btnCancel.DialogResult = global::System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new global::System.Drawing.Point(147, 129);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new global::System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new global::System.EventHandler(this.btnCancel_Click);
			this.txtName.Location = new global::System.Drawing.Point(12, 103);
			this.txtName.Name = "txtName";
			this.txtName.Size = new global::System.Drawing.Size(210, 20);
			this.txtName.TabIndex = 1;
			this.label1.AutoSize = true;
			this.label1.Location = new global::System.Drawing.Point(12, 48);
			this.label1.Name = "label1";
			this.label1.Size = new global::System.Drawing.Size(73, 13);
			this.label1.TabIndex = 6;
			this.label1.Text = "Repeat Count";
			this.label2.AutoSize = true;
			this.label2.Location = new global::System.Drawing.Point(119, 48);
			this.label2.Name = "label2";
			this.label2.Size = new global::System.Drawing.Size(59, 13);
			this.label2.TabIndex = 7;
			this.label2.Text = "Step Value";
			this.textbox_RepeatCount.Location = new global::System.Drawing.Point(12, 64);
			global::System.Windows.Forms.NumericUpDown numericUpDown2 = this.textbox_RepeatCount;
			int[] array2 = new int[4];
			array2[0] = 100000;
			numericUpDown2.Maximum = new decimal(array2);
			this.textbox_RepeatCount.Name = "textbox_RepeatCount";
			this.textbox_RepeatCount.Size = new global::System.Drawing.Size(100, 20);
			this.textbox_RepeatCount.TabIndex = 8;
			this.textbox_RepeatCount.TextAlign = global::System.Windows.Forms.HorizontalAlignment.Right;
			this.textbox_StepValue.Location = new global::System.Drawing.Point(122, 64);
			global::System.Windows.Forms.NumericUpDown numericUpDown3 = this.textbox_StepValue;
			int[] array3 = new int[4];
			array3[0] = 100000;
			numericUpDown3.Maximum = new decimal(array3);
			this.textbox_StepValue.Name = "textbox_StepValue";
			this.textbox_StepValue.Size = new global::System.Drawing.Size(100, 20);
			this.textbox_StepValue.TabIndex = 9;
			this.textbox_StepValue.TextAlign = global::System.Windows.Forms.HorizontalAlignment.Right;
			base.AcceptButton = this.btnCreate;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = this.btnCancel;
			base.ClientSize = new global::System.Drawing.Size(236, 162);
			base.Controls.Add(this.textbox_StepValue);
			base.Controls.Add(this.textbox_RepeatCount);
			base.Controls.Add(this.label2);
			base.Controls.Add(this.label1);
			base.Controls.Add(this.lblName);
			base.Controls.Add(this.lblID);
			base.Controls.Add(this.txtName);
			base.Controls.Add(this.btnCancel);
			base.Controls.Add(this.btnCreate);
			base.Controls.Add(this.nudID);
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			base.Name = "NewRow";
			base.StartPosition = global::System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "FormNewRow";
			((global::System.ComponentModel.ISupportInitialize)this.nudID).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.textbox_RepeatCount).EndInit();
			((global::System.ComponentModel.ISupportInitialize)this.textbox_StepValue).EndInit();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		// Token: 0x04000106 RID: 262
		private global::System.ComponentModel.IContainer components;

		// Token: 0x04000107 RID: 263
		private global::System.Windows.Forms.NumericUpDown nudID;

		// Token: 0x04000108 RID: 264
		private global::System.Windows.Forms.Button btnCreate;

		// Token: 0x04000109 RID: 265
		private global::System.Windows.Forms.Button btnCancel;

		// Token: 0x0400010A RID: 266
		private global::System.Windows.Forms.TextBox txtName;

		// Token: 0x0400010B RID: 267
		private global::System.Windows.Forms.Label lblID;

		// Token: 0x0400010C RID: 268
		private global::System.Windows.Forms.Label lblName;

		// Token: 0x0400010D RID: 269
		private global::System.Windows.Forms.Label label1;

		// Token: 0x0400010E RID: 270
		private global::System.Windows.Forms.Label label2;

		// Token: 0x0400010F RID: 271
		private global::System.Windows.Forms.NumericUpDown textbox_RepeatCount;

		// Token: 0x04000110 RID: 272
		private global::System.Windows.Forms.NumericUpDown textbox_StepValue;
	}
}
