namespace Chomp.Forms
{
	// Token: 0x0200000D RID: 13
	public partial class FieldAdjuster : global::System.Windows.Forms.Form
	{
		// Token: 0x0600011E RID: 286 RVA: 0x00010AA5 File Offset: 0x0000ECA5
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x0600011F RID: 287 RVA: 0x00010AC4 File Offset: 0x0000ECC4
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			this.btnSaveSettings = new global::System.Windows.Forms.Button();
			this.btnCancel = new global::System.Windows.Forms.Button();
			this.label1 = new global::System.Windows.Forms.Label();
			this.label2 = new global::System.Windows.Forms.Label();
			this.textbox_FieldMatch = new global::System.Windows.Forms.TextBox();
			this.textbox_Formula = new global::System.Windows.Forms.TextBox();
			this.label3 = new global::System.Windows.Forms.Label();
			this.label4 = new global::System.Windows.Forms.Label();
			this.label5 = new global::System.Windows.Forms.Label();
			this.label6 = new global::System.Windows.Forms.Label();
			this.label8 = new global::System.Windows.Forms.Label();
			this.label9 = new global::System.Windows.Forms.Label();
			this.label10 = new global::System.Windows.Forms.Label();
			this.label11 = new global::System.Windows.Forms.Label();
			this.label7 = new global::System.Windows.Forms.Label();
			this.textbox_RowRange = new global::System.Windows.Forms.TextBox();
			this.textbox_RowPartialMatch = new global::System.Windows.Forms.TextBox();
			this.textbox_FieldMinimum = new global::System.Windows.Forms.TextBox();
			this.textbox_FieldMaximum = new global::System.Windows.Forms.TextBox();
			this.textbox_FieldExclusion = new global::System.Windows.Forms.TextBox();
			this.textbox_FieldInclusion = new global::System.Windows.Forms.TextBox();
			this.label12 = new global::System.Windows.Forms.Label();
			this.textbox_ValueMin = new global::System.Windows.Forms.TextBox();
			this.textbox_ValueMax = new global::System.Windows.Forms.TextBox();
			this.checkbox_RetainFieldText = new global::System.Windows.Forms.CheckBox();
			this.toolTip1 = new global::System.Windows.Forms.ToolTip(this.components);
			this.toolTip2 = new global::System.Windows.Forms.ToolTip(this.components);
			this.toolTip3 = new global::System.Windows.Forms.ToolTip(this.components);
			this.toolTip4 = new global::System.Windows.Forms.ToolTip(this.components);
			this.toolTip5 = new global::System.Windows.Forms.ToolTip(this.components);
			this.toolTip6 = new global::System.Windows.Forms.ToolTip(this.components);
			this.toolTip7 = new global::System.Windows.Forms.ToolTip(this.components);
			this.toolTip8 = new global::System.Windows.Forms.ToolTip(this.components);
			this.toolTip9 = new global::System.Windows.Forms.ToolTip(this.components);
			this.toolTip10 = new global::System.Windows.Forms.ToolTip(this.components);
			this.toolTip11 = new global::System.Windows.Forms.ToolTip(this.components);
			base.SuspendLayout();
			this.btnSaveSettings.Location = new global::System.Drawing.Point(17, 380);
			this.btnSaveSettings.Name = "btnSaveSettings";
			this.btnSaveSettings.Size = new global::System.Drawing.Size(75, 23);
			this.btnSaveSettings.TabIndex = 2;
			this.btnSaveSettings.Text = "Apply";
			this.btnSaveSettings.UseVisualStyleBackColor = true;
			this.btnSaveSettings.Click += new global::System.EventHandler(this.btnCreate_Click);
			this.btnCancel.DialogResult = global::System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new global::System.Drawing.Point(147, 380);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new global::System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new global::System.EventHandler(this.btnCancel_Click);
			this.label1.AutoSize = true;
			this.label1.Location = new global::System.Drawing.Point(15, 279);
			this.label1.Name = "label1";
			this.label1.Size = new global::System.Drawing.Size(44, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Formula";
			this.toolTip1.SetToolTip(this.label1, "Enter the mathematical formula to apply - Use x for the field value.\nExample: x*3");
			this.label2.AutoSize = true;
			this.label2.Location = new global::System.Drawing.Point(14, 9);
			this.label2.Name = "label2";
			this.label2.Size = new global::System.Drawing.Size(73, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "Field to Adjust";
			this.toolTip2.SetToolTip(this.label2, "Field to apply the adjustment to.\nExample: Scaling: STR");
			this.textbox_FieldMatch.Location = new global::System.Drawing.Point(17, 25);
			this.textbox_FieldMatch.Name = "textbox_FieldMatch";
			this.textbox_FieldMatch.Size = new global::System.Drawing.Size(205, 20);
			this.textbox_FieldMatch.TabIndex = 6;
			this.textbox_Formula.Location = new global::System.Drawing.Point(18, 295);
			this.textbox_Formula.Name = "textbox_Formula";
			this.textbox_Formula.Size = new global::System.Drawing.Size(205, 20);
			this.textbox_Formula.TabIndex = 7;
			this.label3.AutoSize = true;
			this.label3.Location = new global::System.Drawing.Point(14, 103);
			this.label3.Name = "label3";
			this.label3.Size = new global::System.Drawing.Size(64, 13);
			this.label3.TabIndex = 10;
			this.label3.Text = "Row Range";
			this.toolTip3.SetToolTip(this.label3, "Specify a row range, use the CSV delimiter to indicate start and end values. \nExample: 1000,9000");
			this.label4.AutoSize = true;
			this.label4.Location = new global::System.Drawing.Point(120, 103);
			this.label4.Name = "label4";
			this.label4.Size = new global::System.Drawing.Size(94, 13);
			this.label4.TabIndex = 11;
			this.label4.Text = "Substring End Match";
			this.toolTip4.SetToolTip(this.label4, "Define a substring to match with the end of a row ID. \nExample: 700");
			this.label5.AutoSize = true;
			this.label5.Location = new global::System.Drawing.Point(122, 328);
			this.label5.Name = "label5";
			this.label5.Size = new global::System.Drawing.Size(86, 13);
			this.label5.TabIndex = 13;
			this.label5.Text = "Output Maximum";
			this.toolTip5.SetToolTip(this.label5, "Cap final value for field adjustment at this value.");
			this.label6.AutoSize = true;
			this.label6.Location = new global::System.Drawing.Point(18, 328);
			this.label6.Name = "label6";
			this.label6.Size = new global::System.Drawing.Size(83, 13);
			this.label6.TabIndex = 14;
			this.label6.Text = "Output Minimum";
			this.toolTip6.SetToolTip(this.label6, "Floor final value for field adjustment at this value.");
			this.label8.AutoSize = true;
			this.label8.Location = new global::System.Drawing.Point(14, 151);
			this.label8.Name = "label8";
			this.label8.Size = new global::System.Drawing.Size(73, 13);
			this.label8.TabIndex = 17;
			this.label8.Text = "Field Minimum";
			this.toolTip7.SetToolTip(this.label8, "Define a minimum existing value to apply the formula to.");
			this.label9.AutoSize = true;
			this.label9.Location = new global::System.Drawing.Point(120, 151);
			this.label9.Name = "label9";
			this.label9.Size = new global::System.Drawing.Size(76, 13);
			this.label9.TabIndex = 18;
			this.label9.Text = "Field Maximum";
			this.toolTip8.SetToolTip(this.label9, "Define a maximum existing value to apply the formula to.");
			this.label10.AutoSize = true;
			this.label10.Location = new global::System.Drawing.Point(15, 199);
			this.label10.Name = "label10";
			this.label10.Size = new global::System.Drawing.Size(77, 13);
			this.label10.TabIndex = 23;
			this.label10.Text = "Field Exclusion";
			this.toolTip9.SetToolTip(this.label10, "Define an existing value to ignore when applying the formula.\nMultiple can be included by using the CSV delimiter to split them.");
			this.label11.AutoSize = true;
			this.label11.Location = new global::System.Drawing.Point(122, 199);
			this.label11.Name = "label11";
			this.label11.Size = new global::System.Drawing.Size(74, 13);
			this.label11.TabIndex = 24;
			this.label11.Text = "Field Inclusion";
			this.toolTip10.SetToolTip(this.label11, "Define an existing value to ignore when applying the formula.\nMultiple can be included by using the CSV delimiter to split them.");
			this.label7.AutoSize = true;
			this.label7.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 12f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.label7.Location = new global::System.Drawing.Point(13, 74);
			this.label7.Name = "label7";
			this.label7.Size = new global::System.Drawing.Size(75, 20);
			this.label7.TabIndex = 16;
			this.label7.Text = "Selection";
			this.textbox_RowRange.Location = new global::System.Drawing.Point(17, 119);
			this.textbox_RowRange.Name = "textbox_RowRange";
			this.textbox_RowRange.Size = new global::System.Drawing.Size(100, 20);
			this.textbox_RowRange.TabIndex = 19;
			this.textbox_RowPartialMatch.Location = new global::System.Drawing.Point(123, 119);
			this.textbox_RowPartialMatch.Name = "textbox_RowPartialMatch";
			this.textbox_RowPartialMatch.Size = new global::System.Drawing.Size(100, 20);
			this.textbox_RowPartialMatch.TabIndex = 20;
			this.textbox_FieldMinimum.Location = new global::System.Drawing.Point(17, 167);
			this.textbox_FieldMinimum.Name = "textbox_FieldMinimum";
			this.textbox_FieldMinimum.Size = new global::System.Drawing.Size(100, 20);
			this.textbox_FieldMinimum.TabIndex = 21;
			this.textbox_FieldMaximum.Location = new global::System.Drawing.Point(123, 167);
			this.textbox_FieldMaximum.Name = "textbox_FieldMaximum";
			this.textbox_FieldMaximum.Size = new global::System.Drawing.Size(100, 20);
			this.textbox_FieldMaximum.TabIndex = 22;
			this.textbox_FieldExclusion.Location = new global::System.Drawing.Point(17, 215);
			this.textbox_FieldExclusion.Name = "textbox_FieldExclusion";
			this.textbox_FieldExclusion.Size = new global::System.Drawing.Size(100, 20);
			this.textbox_FieldExclusion.TabIndex = 25;
			this.textbox_FieldInclusion.Location = new global::System.Drawing.Point(123, 215);
			this.textbox_FieldInclusion.Name = "textbox_FieldInclusion";
			this.textbox_FieldInclusion.Size = new global::System.Drawing.Size(100, 20);
			this.textbox_FieldInclusion.TabIndex = 26;
			this.label12.AutoSize = true;
			this.label12.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 12f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
			this.label12.Location = new global::System.Drawing.Point(14, 249);
			this.label12.Name = "label12";
			this.label12.Size = new global::System.Drawing.Size(58, 20);
			this.label12.TabIndex = 27;
			this.label12.Text = "Output";
			this.textbox_ValueMin.Location = new global::System.Drawing.Point(17, 344);
			this.textbox_ValueMin.Name = "textbox_ValueMin";
			this.textbox_ValueMin.Size = new global::System.Drawing.Size(100, 20);
			this.textbox_ValueMin.TabIndex = 28;
			this.textbox_ValueMax.Location = new global::System.Drawing.Point(123, 344);
			this.textbox_ValueMax.Name = "textbox_ValueMax";
			this.textbox_ValueMax.Size = new global::System.Drawing.Size(100, 20);
			this.textbox_ValueMax.TabIndex = 29;
			this.checkbox_RetainFieldText.AutoSize = true;
			this.checkbox_RetainFieldText.Location = new global::System.Drawing.Point(17, 51);
			this.checkbox_RetainFieldText.Name = "checkbox_RetainFieldText";
			this.checkbox_RetainFieldText.Size = new global::System.Drawing.Size(106, 17);
			this.checkbox_RetainFieldText.TabIndex = 30;
			this.checkbox_RetainFieldText.Text = "Retain Field Text";
			this.checkbox_RetainFieldText.UseVisualStyleBackColor = true;
			this.toolTip11.SetToolTip(this.checkbox_RetainFieldText, "Retain the field text between uses.");
			base.AcceptButton = this.btnSaveSettings;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = this.btnCancel;
			base.ClientSize = new global::System.Drawing.Size(243, 415);
			base.Controls.Add(this.checkbox_RetainFieldText);
			base.Controls.Add(this.textbox_ValueMax);
			base.Controls.Add(this.textbox_ValueMin);
			base.Controls.Add(this.label12);
			base.Controls.Add(this.textbox_FieldInclusion);
			base.Controls.Add(this.textbox_FieldExclusion);
			base.Controls.Add(this.label11);
			base.Controls.Add(this.label10);
			base.Controls.Add(this.textbox_FieldMaximum);
			base.Controls.Add(this.textbox_FieldMinimum);
			base.Controls.Add(this.textbox_RowPartialMatch);
			base.Controls.Add(this.textbox_RowRange);
			base.Controls.Add(this.label9);
			base.Controls.Add(this.label8);
			base.Controls.Add(this.label7);
			base.Controls.Add(this.label6);
			base.Controls.Add(this.label5);
			base.Controls.Add(this.label4);
			base.Controls.Add(this.label3);
			base.Controls.Add(this.textbox_Formula);
			base.Controls.Add(this.textbox_FieldMatch);
			base.Controls.Add(this.label2);
			base.Controls.Add(this.label1);
			base.Controls.Add(this.btnCancel);
			base.Controls.Add(this.btnSaveSettings);
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			base.Name = "FormFieldAdjuster";
			base.StartPosition = global::System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Field Adjuster";
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		// Token: 0x0400009D RID: 157
		private global::System.ComponentModel.IContainer components;

		// Token: 0x0400009E RID: 158
		private global::System.Windows.Forms.Button btnSaveSettings;

		// Token: 0x0400009F RID: 159
		private global::System.Windows.Forms.Button btnCancel;

		// Token: 0x040000A0 RID: 160
		private global::System.Windows.Forms.Label label1;

		// Token: 0x040000A1 RID: 161
		private global::System.Windows.Forms.Label label2;

		// Token: 0x040000A2 RID: 162
		private global::System.Windows.Forms.TextBox textbox_FieldMatch;

		// Token: 0x040000A3 RID: 163
		private global::System.Windows.Forms.TextBox textbox_Formula;

		// Token: 0x040000A4 RID: 164
		private global::System.Windows.Forms.Label label3;

		// Token: 0x040000A5 RID: 165
		private global::System.Windows.Forms.Label label4;

		// Token: 0x040000A6 RID: 166
		private global::System.Windows.Forms.Label label5;

		// Token: 0x040000A7 RID: 167
		private global::System.Windows.Forms.Label label6;

		// Token: 0x040000A8 RID: 168
		private global::System.Windows.Forms.ToolTip toolTip1;

		// Token: 0x040000A9 RID: 169
		private global::System.Windows.Forms.ToolTip toolTip2;

		// Token: 0x040000AA RID: 170
		private global::System.Windows.Forms.ToolTip toolTip3;

		// Token: 0x040000AB RID: 171
		private global::System.Windows.Forms.ToolTip toolTip4;

		// Token: 0x040000AC RID: 172
		private global::System.Windows.Forms.ToolTip toolTip5;

		// Token: 0x040000AD RID: 173
		private global::System.Windows.Forms.ToolTip toolTip6;

		// Token: 0x040000AE RID: 174
		private global::System.Windows.Forms.ToolTip toolTip7;

		// Token: 0x040000AF RID: 175
		private global::System.Windows.Forms.ToolTip toolTip8;

		// Token: 0x040000B0 RID: 176
		private global::System.Windows.Forms.ToolTip toolTip9;

		// Token: 0x040000B1 RID: 177
		private global::System.Windows.Forms.ToolTip toolTip10;

		// Token: 0x040000B2 RID: 178
		private global::System.Windows.Forms.ToolTip toolTip11;

		// Token: 0x040000B3 RID: 179
		private global::System.Windows.Forms.Label label7;

		// Token: 0x040000B4 RID: 180
		private global::System.Windows.Forms.Label label8;

		// Token: 0x040000B5 RID: 181
		private global::System.Windows.Forms.Label label9;

		// Token: 0x040000B6 RID: 182
		private global::System.Windows.Forms.TextBox textbox_RowRange;

		// Token: 0x040000B7 RID: 183
		private global::System.Windows.Forms.TextBox textbox_RowPartialMatch;

		// Token: 0x040000B8 RID: 184
		private global::System.Windows.Forms.TextBox textbox_FieldMinimum;

		// Token: 0x040000B9 RID: 185
		private global::System.Windows.Forms.TextBox textbox_FieldMaximum;

		// Token: 0x040000BA RID: 186
		private global::System.Windows.Forms.Label label10;

		// Token: 0x040000BB RID: 187
		private global::System.Windows.Forms.Label label11;

		// Token: 0x040000BC RID: 188
		private global::System.Windows.Forms.TextBox textbox_FieldExclusion;

		// Token: 0x040000BD RID: 189
		private global::System.Windows.Forms.TextBox textbox_FieldInclusion;

		// Token: 0x040000BE RID: 190
		private global::System.Windows.Forms.Label label12;

		// Token: 0x040000BF RID: 191
		private global::System.Windows.Forms.TextBox textbox_ValueMin;

		// Token: 0x040000C0 RID: 192
		private global::System.Windows.Forms.TextBox textbox_ValueMax;

		// Token: 0x040000C1 RID: 193
		private global::System.Windows.Forms.CheckBox checkbox_RetainFieldText;
	}
}
