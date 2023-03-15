namespace Chomp.Forms
{
	// Token: 0x02000014 RID: 20
	public partial class InterfaceSettings : global::System.Windows.Forms.Form
	{
		// Token: 0x06000147 RID: 327 RVA: 0x000136AE File Offset: 0x000118AE
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000148 RID: 328 RVA: 0x000136D0 File Offset: 0x000118D0
		private void InitializeComponent()
		{
            this.groupbox_Field = new System.Windows.Forms.GroupBox();
            this.textBox_FieldColor_String = new System.Windows.Forms.TextBox();
            this.button_FieldColor_String = new System.Windows.Forms.Button();
            this.textBox_FieldColor_Bool = new System.Windows.Forms.TextBox();
            this.button_FieldColor_Bool = new System.Windows.Forms.Button();
            this.textBox_FieldColor_Float = new System.Windows.Forms.TextBox();
            this.button_FieldColor_Float = new System.Windows.Forms.Button();
            this.textBox_FieldColor_Int = new System.Windows.Forms.TextBox();
            this.button_FieldColor_int = new System.Windows.Forms.Button();
            this.btnSaveSettings = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.colorDialog_FieldValue_Int = new System.Windows.Forms.ColorDialog();
            this.colorDialog_FieldValue_Float = new System.Windows.Forms.ColorDialog();
            this.colorDialog_FieldValue_Bool = new System.Windows.Forms.ColorDialog();
            this.colorDialog_FieldValue_String = new System.Windows.Forms.ColorDialog();
            this.group_FieldElements = new System.Windows.Forms.GroupBox();
            this.checkbox_customizableEnumToggle = new System.Windows.Forms.CheckBox();
            this.checkbox_BooleanEnumToggle = new System.Windows.Forms.CheckBox();
            this.checkbox_FieldDescriptions = new System.Windows.Forms.CheckBox();
            this.checkbox_EnumValueInName = new System.Windows.Forms.CheckBox();
            this.checkbox_DisplayEnums = new System.Windows.Forms.CheckBox();
            this.groupbox_Field.SuspendLayout();
            this.group_FieldElements.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupbox_Field
            // 
            this.groupbox_Field.Controls.Add(this.textBox_FieldColor_String);
            this.groupbox_Field.Controls.Add(this.button_FieldColor_String);
            this.groupbox_Field.Controls.Add(this.textBox_FieldColor_Bool);
            this.groupbox_Field.Controls.Add(this.button_FieldColor_Bool);
            this.groupbox_Field.Controls.Add(this.textBox_FieldColor_Float);
            this.groupbox_Field.Controls.Add(this.button_FieldColor_Float);
            this.groupbox_Field.Controls.Add(this.textBox_FieldColor_Int);
            this.groupbox_Field.Controls.Add(this.button_FieldColor_int);
            this.groupbox_Field.Location = new System.Drawing.Point(14, 14);
            this.groupbox_Field.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupbox_Field.Name = "groupbox_Field";
            this.groupbox_Field.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupbox_Field.Size = new System.Drawing.Size(292, 160);
            this.groupbox_Field.TabIndex = 31;
            this.groupbox_Field.TabStop = false;
            this.groupbox_Field.Text = "Field Coloring";
            // 
            // textBox_FieldColor_String
            // 
            this.textBox_FieldColor_String.Location = new System.Drawing.Point(102, 125);
            this.textBox_FieldColor_String.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBox_FieldColor_String.Name = "textBox_FieldColor_String";
            this.textBox_FieldColor_String.ReadOnly = true;
            this.textBox_FieldColor_String.Size = new System.Drawing.Size(178, 23);
            this.textBox_FieldColor_String.TabIndex = 7;
            this.textBox_FieldColor_String.Text = "Example";
            // 
            // button_FieldColor_String
            // 
            this.button_FieldColor_String.Location = new System.Drawing.Point(7, 122);
            this.button_FieldColor_String.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_FieldColor_String.Name = "button_FieldColor_String";
            this.button_FieldColor_String.Size = new System.Drawing.Size(88, 27);
            this.button_FieldColor_String.TabIndex = 6;
            this.button_FieldColor_String.Text = "String";
            this.button_FieldColor_String.UseVisualStyleBackColor = true;
            this.button_FieldColor_String.Click += new System.EventHandler(this.button_FieldColor_String_Click);
            // 
            // textBox_FieldColor_Bool
            // 
            this.textBox_FieldColor_Bool.Location = new System.Drawing.Point(102, 91);
            this.textBox_FieldColor_Bool.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBox_FieldColor_Bool.Name = "textBox_FieldColor_Bool";
            this.textBox_FieldColor_Bool.ReadOnly = true;
            this.textBox_FieldColor_Bool.Size = new System.Drawing.Size(178, 23);
            this.textBox_FieldColor_Bool.TabIndex = 5;
            this.textBox_FieldColor_Bool.Text = "0";
            // 
            // button_FieldColor_Bool
            // 
            this.button_FieldColor_Bool.Location = new System.Drawing.Point(7, 89);
            this.button_FieldColor_Bool.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_FieldColor_Bool.Name = "button_FieldColor_Bool";
            this.button_FieldColor_Bool.Size = new System.Drawing.Size(88, 27);
            this.button_FieldColor_Bool.TabIndex = 4;
            this.button_FieldColor_Bool.Text = "Boolean";
            this.button_FieldColor_Bool.UseVisualStyleBackColor = true;
            this.button_FieldColor_Bool.Click += new System.EventHandler(this.button_FieldColor_Bool_Click);
            // 
            // textBox_FieldColor_Float
            // 
            this.textBox_FieldColor_Float.Location = new System.Drawing.Point(102, 58);
            this.textBox_FieldColor_Float.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBox_FieldColor_Float.Name = "textBox_FieldColor_Float";
            this.textBox_FieldColor_Float.ReadOnly = true;
            this.textBox_FieldColor_Float.Size = new System.Drawing.Size(178, 23);
            this.textBox_FieldColor_Float.TabIndex = 3;
            this.textBox_FieldColor_Float.Text = "1.05";
            // 
            // button_FieldColor_Float
            // 
            this.button_FieldColor_Float.Location = new System.Drawing.Point(7, 55);
            this.button_FieldColor_Float.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_FieldColor_Float.Name = "button_FieldColor_Float";
            this.button_FieldColor_Float.Size = new System.Drawing.Size(88, 27);
            this.button_FieldColor_Float.TabIndex = 2;
            this.button_FieldColor_Float.Text = "Float";
            this.button_FieldColor_Float.UseVisualStyleBackColor = true;
            this.button_FieldColor_Float.Click += new System.EventHandler(this.button_FieldColor_Float_Click);
            // 
            // textBox_FieldColor_Int
            // 
            this.textBox_FieldColor_Int.Location = new System.Drawing.Point(102, 24);
            this.textBox_FieldColor_Int.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBox_FieldColor_Int.Name = "textBox_FieldColor_Int";
            this.textBox_FieldColor_Int.ReadOnly = true;
            this.textBox_FieldColor_Int.Size = new System.Drawing.Size(178, 23);
            this.textBox_FieldColor_Int.TabIndex = 1;
            this.textBox_FieldColor_Int.Text = "1";
            // 
            // button_FieldColor_int
            // 
            this.button_FieldColor_int.Location = new System.Drawing.Point(7, 22);
            this.button_FieldColor_int.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_FieldColor_int.Name = "button_FieldColor_int";
            this.button_FieldColor_int.Size = new System.Drawing.Size(88, 27);
            this.button_FieldColor_int.TabIndex = 0;
            this.button_FieldColor_int.Text = "Integer";
            this.button_FieldColor_int.UseVisualStyleBackColor = true;
            this.button_FieldColor_int.Click += new System.EventHandler(this.button_FieldColor_int_Click);
            // 
            // btnSaveSettings
            // 
            this.btnSaveSettings.Location = new System.Drawing.Point(14, 355);
            this.btnSaveSettings.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnSaveSettings.Name = "btnSaveSettings";
            this.btnSaveSettings.Size = new System.Drawing.Size(88, 27);
            this.btnSaveSettings.TabIndex = 32;
            this.btnSaveSettings.Text = "Save";
            this.btnSaveSettings.UseVisualStyleBackColor = true;
            this.btnSaveSettings.Click += new System.EventHandler(this.btnSaveSettings_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(218, 355);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 27);
            this.btnCancel.TabIndex = 33;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // colorDialog_FieldValue_Int
            // 
            this.colorDialog_FieldValue_Int.Color = System.Drawing.Color.White;
            // 
            // colorDialog_FieldValue_Float
            // 
            this.colorDialog_FieldValue_Float.Color = System.Drawing.Color.White;
            // 
            // colorDialog_FieldValue_Bool
            // 
            this.colorDialog_FieldValue_Bool.Color = System.Drawing.Color.White;
            // 
            // colorDialog_FieldValue_String
            // 
            this.colorDialog_FieldValue_String.Color = System.Drawing.Color.White;
            // 
            // group_FieldElements
            // 
            this.group_FieldElements.Controls.Add(this.checkbox_customizableEnumToggle);
            this.group_FieldElements.Controls.Add(this.checkbox_BooleanEnumToggle);
            this.group_FieldElements.Controls.Add(this.checkbox_FieldDescriptions);
            this.group_FieldElements.Controls.Add(this.checkbox_EnumValueInName);
            this.group_FieldElements.Controls.Add(this.checkbox_DisplayEnums);
            this.group_FieldElements.Location = new System.Drawing.Point(14, 181);
            this.group_FieldElements.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.group_FieldElements.Name = "group_FieldElements";
            this.group_FieldElements.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.group_FieldElements.Size = new System.Drawing.Size(292, 167);
            this.group_FieldElements.TabIndex = 34;
            this.group_FieldElements.TabStop = false;
            this.group_FieldElements.Text = "Field Interface";
            // 
            // checkbox_customizableEnumToggle
            // 
            this.checkbox_customizableEnumToggle.AutoSize = true;
            this.checkbox_customizableEnumToggle.Location = new System.Drawing.Point(8, 76);
            this.checkbox_customizableEnumToggle.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkbox_customizableEnumToggle.Name = "checkbox_customizableEnumToggle";
            this.checkbox_customizableEnumToggle.Size = new System.Drawing.Size(249, 19);
            this.checkbox_customizableEnumToggle.TabIndex = 22;
            this.checkbox_customizableEnumToggle.Text = "Show Customizable Enum as Normal Field";
            this.checkbox_customizableEnumToggle.UseVisualStyleBackColor = true;
            // 
            // checkbox_BooleanEnumToggle
            // 
            this.checkbox_BooleanEnumToggle.AutoSize = true;
            this.checkbox_BooleanEnumToggle.Location = new System.Drawing.Point(8, 103);
            this.checkbox_BooleanEnumToggle.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkbox_BooleanEnumToggle.Name = "checkbox_BooleanEnumToggle";
            this.checkbox_BooleanEnumToggle.Size = new System.Drawing.Size(214, 19);
            this.checkbox_BooleanEnumToggle.TabIndex = 21;
            this.checkbox_BooleanEnumToggle.Text = "Display Boolean Enum as Checkbox";
            this.checkbox_BooleanEnumToggle.UseVisualStyleBackColor = true;
            // 
            // checkbox_FieldDescriptions
            // 
            this.checkbox_FieldDescriptions.AutoSize = true;
            this.checkbox_FieldDescriptions.Location = new System.Drawing.Point(8, 129);
            this.checkbox_FieldDescriptions.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkbox_FieldDescriptions.Name = "checkbox_FieldDescriptions";
            this.checkbox_FieldDescriptions.Size = new System.Drawing.Size(207, 19);
            this.checkbox_FieldDescriptions.TabIndex = 20;
            this.checkbox_FieldDescriptions.Text = "Display Field Description in Tooltip";
            this.checkbox_FieldDescriptions.UseVisualStyleBackColor = true;
            // 
            // checkbox_EnumValueInName
            // 
            this.checkbox_EnumValueInName.AutoSize = true;
            this.checkbox_EnumValueInName.Location = new System.Drawing.Point(8, 50);
            this.checkbox_EnumValueInName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkbox_EnumValueInName.Name = "checkbox_EnumValueInName";
            this.checkbox_EnumValueInName.Size = new System.Drawing.Size(219, 19);
            this.checkbox_EnumValueInName.TabIndex = 1;
            this.checkbox_EnumValueInName.Text = "Show Enum Value in Selection Name";
            this.checkbox_EnumValueInName.UseVisualStyleBackColor = true;
            // 
            // checkbox_DisplayEnums
            // 
            this.checkbox_DisplayEnums.AutoSize = true;
            this.checkbox_DisplayEnums.Location = new System.Drawing.Point(8, 23);
            this.checkbox_DisplayEnums.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkbox_DisplayEnums.Name = "checkbox_DisplayEnums";
            this.checkbox_DisplayEnums.Size = new System.Drawing.Size(149, 19);
            this.checkbox_DisplayEnums.TabIndex = 0;
            this.checkbox_DisplayEnums.Text = "Display Enum Selection";
            this.checkbox_DisplayEnums.UseVisualStyleBackColor = true;
            // 
            // InterfaceSettings
            // 
            this.AcceptButton = this.btnSaveSettings;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(317, 397);
            this.Controls.Add(this.group_FieldElements);
            this.Controls.Add(this.btnSaveSettings);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.groupbox_Field);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "InterfaceSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Interface Settings";
            this.groupbox_Field.ResumeLayout(false);
            this.groupbox_Field.PerformLayout();
            this.group_FieldElements.ResumeLayout(false);
            this.group_FieldElements.PerformLayout();
            this.ResumeLayout(false);

		}

		// Token: 0x040000EE RID: 238
		private global::System.ComponentModel.IContainer components;

		// Token: 0x040000EF RID: 239
		private global::System.Windows.Forms.GroupBox groupbox_Field;

		// Token: 0x040000F0 RID: 240
		private global::System.Windows.Forms.Button btnSaveSettings;

		// Token: 0x040000F1 RID: 241
		private global::System.Windows.Forms.Button btnCancel;

		// Token: 0x040000F2 RID: 242
		private global::System.Windows.Forms.ColorDialog colorDialog_FieldValue_Int;

		// Token: 0x040000F3 RID: 243
		private global::System.Windows.Forms.ColorDialog colorDialog_FieldValue_Float;

		// Token: 0x040000F4 RID: 244
		private global::System.Windows.Forms.ColorDialog colorDialog_FieldValue_Bool;

		// Token: 0x040000F5 RID: 245
		private global::System.Windows.Forms.ColorDialog colorDialog_FieldValue_String;

		// Token: 0x040000F6 RID: 246
		private global::System.Windows.Forms.TextBox textBox_FieldColor_String;

		// Token: 0x040000F7 RID: 247
		private global::System.Windows.Forms.Button button_FieldColor_String;

		// Token: 0x040000F8 RID: 248
		private global::System.Windows.Forms.TextBox textBox_FieldColor_Bool;

		// Token: 0x040000F9 RID: 249
		private global::System.Windows.Forms.Button button_FieldColor_Bool;

		// Token: 0x040000FA RID: 250
		private global::System.Windows.Forms.TextBox textBox_FieldColor_Float;

		// Token: 0x040000FB RID: 251
		private global::System.Windows.Forms.Button button_FieldColor_Float;

		// Token: 0x040000FC RID: 252
		private global::System.Windows.Forms.TextBox textBox_FieldColor_Int;

		// Token: 0x040000FD RID: 253
		private global::System.Windows.Forms.Button button_FieldColor_int;

		// Token: 0x040000FE RID: 254
		private global::System.Windows.Forms.GroupBox group_FieldElements;

		// Token: 0x040000FF RID: 255
		private global::System.Windows.Forms.CheckBox checkbox_DisplayEnums;

		// Token: 0x04000100 RID: 256
		private global::System.Windows.Forms.CheckBox checkbox_EnumValueInName;

		// Token: 0x04000101 RID: 257
		private global::System.Windows.Forms.CheckBox checkbox_FieldDescriptions;

		// Token: 0x04000102 RID: 258
		private global::System.Windows.Forms.CheckBox checkbox_BooleanEnumToggle;

		// Token: 0x04000103 RID: 259
		private global::System.Windows.Forms.CheckBox checkbox_customizableEnumToggle;
    }
}
