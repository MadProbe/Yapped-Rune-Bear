namespace Chomp.Forms
{
	// Token: 0x0200000E RID: 14
	public partial class FieldExporter : global::System.Windows.Forms.Form
	{
		// Token: 0x06000123 RID: 291 RVA: 0x00011A78 File Offset: 0x0000FC78
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000124 RID: 292 RVA: 0x00011A98 File Offset: 0x0000FC98
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			this.btnCreate = new global::System.Windows.Forms.Button();
			this.btnCancel = new global::System.Windows.Forms.Button();
			this.textbox_FieldMatch = new global::System.Windows.Forms.TextBox();
			this.textbox_FieldMinimum = new global::System.Windows.Forms.TextBox();
			this.textbox_FieldMaximum = new global::System.Windows.Forms.TextBox();
			this.textbox_FieldExclusions = new global::System.Windows.Forms.TextBox();
			this.textbox_FieldInclusions = new global::System.Windows.Forms.TextBox();
			this.toolTip_fieldlist = new global::System.Windows.Forms.ToolTip(this.components);
			this.toolTip_field_minimums = new global::System.Windows.Forms.ToolTip(this.components);
			this.toolTip_field_maximums = new global::System.Windows.Forms.ToolTip(this.components);
			this.toolTip_strictMatching = new global::System.Windows.Forms.ToolTip(this.components);
			this.toolTip_FieldExclusions = new global::System.Windows.Forms.ToolTip(this.components);
			this.toolTip_FieldInclusions = new global::System.Windows.Forms.ToolTip(this.components);
			this.toolTip_RetainFieldText = new global::System.Windows.Forms.ToolTip(this.components);
			this.checkbox_RetainFieldText = new global::System.Windows.Forms.CheckBox();
			global::System.Windows.Forms.Label lblName = new global::System.Windows.Forms.Label();
			global::System.Windows.Forms.Label label_fieldMinimum = new global::System.Windows.Forms.Label();
			global::System.Windows.Forms.Label label = new global::System.Windows.Forms.Label();
			global::System.Windows.Forms.Label label2 = new global::System.Windows.Forms.Label();
			global::System.Windows.Forms.Label label3 = new global::System.Windows.Forms.Label();
			base.SuspendLayout();
			lblName.AutoSize = true;
			lblName.Location = new global::System.Drawing.Point(12, 9);
			lblName.Name = "lblName";
			lblName.Size = new global::System.Drawing.Size(74, 13);
			lblName.TabIndex = 5;
			lblName.Text = "Field to Export";
			this.toolTip_fieldlist.SetToolTip(lblName, "Field to export. Order of operations: Field Minimum, Field Maximum, Field Inclusion, Field Exclusion");
			label_fieldMinimum.AutoSize = true;
			label_fieldMinimum.Location = new global::System.Drawing.Point(9, 48);
			label_fieldMinimum.Name = "label_fieldMinimum";
			label_fieldMinimum.Size = new global::System.Drawing.Size(73, 13);
			label_fieldMinimum.TabIndex = 6;
			label_fieldMinimum.Text = "Field Minimum";
			this.toolTip_field_minimums.SetToolTip(label_fieldMinimum, "Lowest value allowed for inclusion in export. Ignored if blank.");
			label.AutoSize = true;
			label.Location = new global::System.Drawing.Point(95, 48);
			label.Name = "label1";
			label.Size = new global::System.Drawing.Size(76, 13);
			label.TabIndex = 7;
			label.Text = "Field Maximum";
			this.toolTip_field_maximums.SetToolTip(label, "Highest value allowed for inclusion in export. Ignored if blank.");
			label2.AutoSize = true;
			label2.Location = new global::System.Drawing.Point(270, 48);
			label2.Name = "label2";
			label2.Size = new global::System.Drawing.Size(77, 13);
			label2.TabIndex = 12;
			label2.Text = "Field Exclusion";
			this.toolTip_FieldExclusions.SetToolTip(label2, "Ignore values that match this in export. Ignored if blank.");
			label3.AutoSize = true;
			label3.Location = new global::System.Drawing.Point(184, 48);
			label3.Name = "label3";
			label3.Size = new global::System.Drawing.Size(74, 13);
			label3.TabIndex = 14;
			label3.Text = "Field Inclusion";
			this.toolTip_FieldInclusions.SetToolTip(label3, "Only include values that match this in the export. Ignored if blank.");
			this.btnCreate.Location = new global::System.Drawing.Point(12, 99);
			this.btnCreate.Name = "btnCreate";
			this.btnCreate.Size = new global::System.Drawing.Size(75, 23);
			this.btnCreate.TabIndex = 2;
			this.btnCreate.Text = "OK";
			this.btnCreate.UseVisualStyleBackColor = true;
			this.btnCreate.Click += new global::System.EventHandler(this.btnCreate_Click);
			this.btnCancel.DialogResult = global::System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new global::System.Drawing.Point(281, 99);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new global::System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new global::System.EventHandler(this.btnCancel_Click);
			this.textbox_FieldMatch.Location = new global::System.Drawing.Point(12, 25);
			this.textbox_FieldMatch.Name = "textbox_FieldMatch";
			this.textbox_FieldMatch.Size = new global::System.Drawing.Size(341, 20);
			this.textbox_FieldMatch.TabIndex = 1;
			this.textbox_FieldMinimum.Location = new global::System.Drawing.Point(12, 64);
			this.textbox_FieldMinimum.Name = "textbox_FieldMinimum";
			this.textbox_FieldMinimum.Size = new global::System.Drawing.Size(80, 20);
			this.textbox_FieldMinimum.TabIndex = 8;
			this.textbox_FieldMaximum.Location = new global::System.Drawing.Point(98, 64);
			this.textbox_FieldMaximum.Name = "textbox_FieldMaximum";
			this.textbox_FieldMaximum.Size = new global::System.Drawing.Size(80, 20);
			this.textbox_FieldMaximum.TabIndex = 9;
			this.textbox_FieldExclusions.Location = new global::System.Drawing.Point(273, 64);
			this.textbox_FieldExclusions.Name = "textbox_FieldExclusions";
			this.textbox_FieldExclusions.Size = new global::System.Drawing.Size(80, 20);
			this.textbox_FieldExclusions.TabIndex = 11;
			this.textbox_FieldInclusions.Location = new global::System.Drawing.Point(187, 64);
			this.textbox_FieldInclusions.Name = "textbox_FieldInclusions";
			this.textbox_FieldInclusions.Size = new global::System.Drawing.Size(80, 20);
			this.textbox_FieldInclusions.TabIndex = 13;
			this.checkbox_RetainFieldText.AutoSize = true;
			this.checkbox_RetainFieldText.Location = new global::System.Drawing.Point(98, 103);
			this.checkbox_RetainFieldText.Name = "checkbox_RetainFieldText";
			this.checkbox_RetainFieldText.Size = new global::System.Drawing.Size(106, 17);
			this.checkbox_RetainFieldText.TabIndex = 15;
			this.checkbox_RetainFieldText.Text = "Retain Field Text";
			this.toolTip_RetainFieldText.SetToolTip(this.checkbox_RetainFieldText, "Tick to save the text in fields between uses.");
			this.checkbox_RetainFieldText.UseVisualStyleBackColor = true;
			base.AcceptButton = this.btnCreate;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = this.btnCancel;
			base.ClientSize = new global::System.Drawing.Size(368, 133);
			base.Controls.Add(this.checkbox_RetainFieldText);
			base.Controls.Add(label3);
			base.Controls.Add(this.textbox_FieldInclusions);
			base.Controls.Add(label2);
			base.Controls.Add(this.textbox_FieldExclusions);
			base.Controls.Add(this.textbox_FieldMaximum);
			base.Controls.Add(this.textbox_FieldMinimum);
			base.Controls.Add(label);
			base.Controls.Add(label_fieldMinimum);
			base.Controls.Add(lblName);
			base.Controls.Add(this.textbox_FieldMatch);
			base.Controls.Add(this.btnCancel);
			base.Controls.Add(this.btnCreate);
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			base.Name = "FormFieldExporter";
			base.StartPosition = global::System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Field Exporter";
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		// Token: 0x040000C2 RID: 194
		private global::System.ComponentModel.IContainer components;

		// Token: 0x040000C3 RID: 195
		private global::System.Windows.Forms.Button btnCreate;

		// Token: 0x040000C4 RID: 196
		private global::System.Windows.Forms.Button btnCancel;

		// Token: 0x040000C5 RID: 197
		private global::System.Windows.Forms.TextBox textbox_FieldMatch;

		// Token: 0x040000C6 RID: 198
		private global::System.Windows.Forms.TextBox textbox_FieldMinimum;

		// Token: 0x040000C7 RID: 199
		private global::System.Windows.Forms.TextBox textbox_FieldMaximum;

		// Token: 0x040000C8 RID: 200
		private global::System.Windows.Forms.TextBox textbox_FieldExclusions;

		// Token: 0x040000C9 RID: 201
		private global::System.Windows.Forms.TextBox textbox_FieldInclusions;

		// Token: 0x040000CA RID: 202
		private global::System.Windows.Forms.ToolTip toolTip_fieldlist;

		// Token: 0x040000CB RID: 203
		private global::System.Windows.Forms.ToolTip toolTip_field_minimums;

		// Token: 0x040000CC RID: 204
		private global::System.Windows.Forms.ToolTip toolTip_field_maximums;

		// Token: 0x040000CD RID: 205
		private global::System.Windows.Forms.ToolTip toolTip_strictMatching;

		// Token: 0x040000CE RID: 206
		private global::System.Windows.Forms.ToolTip toolTip_FieldExclusions;

		// Token: 0x040000CF RID: 207
		private global::System.Windows.Forms.ToolTip toolTip_FieldInclusions;

		// Token: 0x040000D0 RID: 208
		private global::System.Windows.Forms.ToolTip toolTip_RetainFieldText;

		// Token: 0x040000D1 RID: 209
		private global::System.Windows.Forms.CheckBox checkbox_RetainFieldText;
	}
}
