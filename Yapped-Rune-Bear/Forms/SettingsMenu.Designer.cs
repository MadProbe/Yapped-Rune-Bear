namespace Chomp.Forms
{
	// Token: 0x02000017 RID: 23
	public partial class SettingsMenu : global::System.Windows.Forms.Form
	{
		// Token: 0x0600015C RID: 348 RVA: 0x00014E66 File Offset: 0x00013066
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x0600015D RID: 349 RVA: 0x00014E88 File Offset: 0x00013088
		private void InitializeComponent()
		{
			this.components = new global::System.ComponentModel.Container();
			global::System.ComponentModel.ComponentResourceManager resources = new global::System.ComponentModel.ComponentResourceManager(typeof(global::Chomp.Forms.SettingsMenu));
			this.btnSaveSettings = new global::System.Windows.Forms.Button();
			this.btnCancel = new global::System.Windows.Forms.Button();
			this.checkbox_VerifyRowDeletion = new global::System.Windows.Forms.CheckBox();
			this.toolTip1 = new global::System.Windows.Forms.ToolTip(this.components);
			this.toolTip2 = new global::System.Windows.Forms.ToolTip(this.components);
			this.toolTip3 = new global::System.Windows.Forms.ToolTip(this.components);
			this.toolTip4 = new global::System.Windows.Forms.ToolTip(this.components);
			this.toolTip5 = new global::System.Windows.Forms.ToolTip(this.components);
			this.toolTip6 = new global::System.Windows.Forms.ToolTip(this.components);
			this.toolTip7 = new global::System.Windows.Forms.ToolTip(this.components);
			this.label1 = new global::System.Windows.Forms.Label();
			this.toolTip8 = new global::System.Windows.Forms.ToolTip(this.components);
			this.toolTip9 = new global::System.Windows.Forms.ToolTip(this.components);
			this.checkbox_SaveNoEncryption = new global::System.Windows.Forms.CheckBox();
			this.label4 = new global::System.Windows.Forms.Label();
			this.textbox_ProjectName = new global::System.Windows.Forms.TextBox();
			this.button_SelectTextEditor = new global::System.Windows.Forms.Button();
			this.textbox_TextEditor = new global::System.Windows.Forms.TextBox();
			this.textEditorPath = new global::System.Windows.Forms.OpenFileDialog();
			this.checkbox_SuppressConfirmations = new global::System.Windows.Forms.CheckBox();
			this.checkbox_UseTextEditor = new global::System.Windows.Forms.CheckBox();
			this.groupbox_General = new global::System.Windows.Forms.GroupBox();
			this.groupbox_Workflow = new global::System.Windows.Forms.GroupBox();
			this.secondaryDataPath = new global::System.Windows.Forms.OpenFileDialog();
			this.groupbox_General.SuspendLayout();
			this.groupbox_Workflow.SuspendLayout();
			base.SuspendLayout();
			this.btnSaveSettings.Location = new global::System.Drawing.Point(13, 253);
			this.btnSaveSettings.Name = "btnSaveSettings";
			this.btnSaveSettings.Size = new global::System.Drawing.Size(75, 23);
			this.btnSaveSettings.TabIndex = 2;
			this.btnSaveSettings.Text = "Save";
			this.btnSaveSettings.UseVisualStyleBackColor = true;
			this.btnSaveSettings.Click += new global::System.EventHandler(this.btnCreate_Click);
			this.btnCancel.DialogResult = global::System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new global::System.Drawing.Point(188, 253);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new global::System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new global::System.EventHandler(this.btnCancel_Click);
			this.checkbox_VerifyRowDeletion.AutoSize = true;
			this.checkbox_VerifyRowDeletion.Location = new global::System.Drawing.Point(6, 84);
			this.checkbox_VerifyRowDeletion.Name = "checkbox_VerifyRowDeletion";
			this.checkbox_VerifyRowDeletion.Size = new global::System.Drawing.Size(119, 17);
			this.checkbox_VerifyRowDeletion.TabIndex = 10;
			this.checkbox_VerifyRowDeletion.Text = "Verify Row Deletion";
			this.toolTip4.SetToolTip(this.checkbox_VerifyRowDeletion, "Toggle warning before row deletion.");
			this.checkbox_VerifyRowDeletion.UseVisualStyleBackColor = true;
			this.label1.AutoSize = true;
			this.label1.Location = new global::System.Drawing.Point(6, 19);
			this.label1.Name = "label1";
			this.label1.Size = new global::System.Drawing.Size(71, 13);
			this.label1.TabIndex = 17;
			this.label1.Text = "Project Name";
			this.toolTip7.SetToolTip(this.label1, "Define a project name. This is used to isolate mod-specific names so the original Name files are not overwritten.");
			this.label1.Click += new global::System.EventHandler(this.label1_Click);
			this.checkbox_SaveNoEncryption.AutoSize = true;
			this.checkbox_SaveNoEncryption.Location = new global::System.Drawing.Point(6, 61);
			this.checkbox_SaveNoEncryption.Name = "checkbox_SaveNoEncryption";
			this.checkbox_SaveNoEncryption.Size = new global::System.Drawing.Size(141, 17);
			this.checkbox_SaveNoEncryption.TabIndex = 26;
			this.checkbox_SaveNoEncryption.Text = "Save without Encryption";
			this.checkbox_SaveNoEncryption.UseVisualStyleBackColor = true;
			this.label4.AutoSize = true;
			this.label4.Location = new global::System.Drawing.Point(6, 19);
			this.label4.Name = "label4";
			this.label4.Size = new global::System.Drawing.Size(95, 13);
			this.label4.TabIndex = 24;
			this.label4.Text = "Current Text Editor";
			this.textbox_ProjectName.Location = new global::System.Drawing.Point(6, 35);
			this.textbox_ProjectName.Name = "textbox_ProjectName";
			this.textbox_ProjectName.Size = new global::System.Drawing.Size(238, 20);
			this.textbox_ProjectName.TabIndex = 18;
			this.button_SelectTextEditor.Location = new global::System.Drawing.Point(140, 33);
			this.button_SelectTextEditor.Name = "button_SelectTextEditor";
			this.button_SelectTextEditor.Size = new global::System.Drawing.Size(104, 23);
			this.button_SelectTextEditor.TabIndex = 22;
			this.button_SelectTextEditor.Text = "Select Text Editor";
			this.button_SelectTextEditor.UseVisualStyleBackColor = true;
			this.button_SelectTextEditor.Click += new global::System.EventHandler(this.button_SelectTextEditor_Click);
			this.textbox_TextEditor.Location = new global::System.Drawing.Point(6, 35);
			this.textbox_TextEditor.Name = "textbox_TextEditor";
			this.textbox_TextEditor.Size = new global::System.Drawing.Size(128, 20);
			this.textbox_TextEditor.TabIndex = 23;
			this.textEditorPath.Filter = ".exe|*";
			this.checkbox_SuppressConfirmations.AutoSize = true;
			this.checkbox_SuppressConfirmations.Location = new global::System.Drawing.Point(6, 61);
			this.checkbox_SuppressConfirmations.Name = "checkbox_SuppressConfirmations";
			this.checkbox_SuppressConfirmations.Size = new global::System.Drawing.Size(182, 17);
			this.checkbox_SuppressConfirmations.TabIndex = 25;
			this.checkbox_SuppressConfirmations.Text = "Suppress Confirmation Messages";
			this.checkbox_SuppressConfirmations.UseVisualStyleBackColor = true;
			this.checkbox_UseTextEditor.AutoSize = true;
			this.checkbox_UseTextEditor.Location = new global::System.Drawing.Point(6, 84);
			this.checkbox_UseTextEditor.Name = "checkbox_UseTextEditor";
			this.checkbox_UseTextEditor.Size = new global::System.Drawing.Size(169, 17);
			this.checkbox_UseTextEditor.TabIndex = 26;
			this.checkbox_UseTextEditor.Text = "Automatically open output files";
			this.checkbox_UseTextEditor.UseVisualStyleBackColor = true;
			this.groupbox_General.Controls.Add(this.checkbox_SaveNoEncryption);
			this.groupbox_General.Controls.Add(this.checkbox_VerifyRowDeletion);
			this.groupbox_General.Controls.Add(this.textbox_ProjectName);
			this.groupbox_General.Controls.Add(this.label1);
			this.groupbox_General.Location = new global::System.Drawing.Point(12, 12);
			this.groupbox_General.Name = "groupbox_General";
			this.groupbox_General.Size = new global::System.Drawing.Size(250, 115);
			this.groupbox_General.TabIndex = 30;
			this.groupbox_General.TabStop = false;
			this.groupbox_General.Text = "General";
			this.groupbox_Workflow.Controls.Add(this.checkbox_SuppressConfirmations);
			this.groupbox_Workflow.Controls.Add(this.checkbox_UseTextEditor);
			this.groupbox_Workflow.Controls.Add(this.button_SelectTextEditor);
			this.groupbox_Workflow.Controls.Add(this.label4);
			this.groupbox_Workflow.Controls.Add(this.textbox_TextEditor);
			this.groupbox_Workflow.Location = new global::System.Drawing.Point(13, 133);
			this.groupbox_Workflow.Name = "groupbox_Workflow";
			this.groupbox_Workflow.Size = new global::System.Drawing.Size(250, 114);
			this.groupbox_Workflow.TabIndex = 32;
			this.groupbox_Workflow.TabStop = false;
			this.groupbox_Workflow.Text = "Workflow";
			this.secondaryDataPath.Filter = ".bdt|.bin|All files|*";
			base.AcceptButton = this.btnSaveSettings;
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = this.btnCancel;
			base.ClientSize = new global::System.Drawing.Size(275, 289);
			base.Controls.Add(this.groupbox_Workflow);
			base.Controls.Add(this.groupbox_General);
			base.Controls.Add(this.btnSaveSettings);
			base.Controls.Add(this.btnCancel);
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			base.Icon = (global::System.Drawing.Icon)resources.GetObject("$this.Icon");
			base.Name = "SettingsMenu";
			base.StartPosition = global::System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Settings";
			base.Load += new global::System.EventHandler(this.FormSettings_Load);
			this.groupbox_General.ResumeLayout(false);
			this.groupbox_General.PerformLayout();
			this.groupbox_Workflow.ResumeLayout(false);
			this.groupbox_Workflow.PerformLayout();
			base.ResumeLayout(false);
		}

		// Token: 0x04000116 RID: 278
		private global::System.ComponentModel.IContainer components;

		// Token: 0x04000117 RID: 279
		private global::System.Windows.Forms.Button btnSaveSettings;

		// Token: 0x04000118 RID: 280
		private global::System.Windows.Forms.Button btnCancel;

		// Token: 0x04000119 RID: 281
		private global::System.Windows.Forms.CheckBox checkbox_VerifyRowDeletion;

		// Token: 0x0400011A RID: 282
		private global::System.Windows.Forms.ToolTip toolTip1;

		// Token: 0x0400011B RID: 283
		private global::System.Windows.Forms.ToolTip toolTip2;

		// Token: 0x0400011C RID: 284
		private global::System.Windows.Forms.ToolTip toolTip3;

		// Token: 0x0400011D RID: 285
		private global::System.Windows.Forms.ToolTip toolTip4;

		// Token: 0x0400011E RID: 286
		private global::System.Windows.Forms.ToolTip toolTip5;

		// Token: 0x0400011F RID: 287
		private global::System.Windows.Forms.ToolTip toolTip6;

		// Token: 0x04000120 RID: 288
		private global::System.Windows.Forms.ToolTip toolTip7;

		// Token: 0x04000121 RID: 289
		private global::System.Windows.Forms.ToolTip toolTip8;

		// Token: 0x04000122 RID: 290
		private global::System.Windows.Forms.ToolTip toolTip9;

		// Token: 0x04000123 RID: 291
		private global::System.Windows.Forms.Label label1;

		// Token: 0x04000124 RID: 292
		private global::System.Windows.Forms.TextBox textbox_ProjectName;

		// Token: 0x04000125 RID: 293
		private global::System.Windows.Forms.Button button_SelectTextEditor;

		// Token: 0x04000126 RID: 294
		private global::System.Windows.Forms.TextBox textbox_TextEditor;

		// Token: 0x04000127 RID: 295
		private global::System.Windows.Forms.Label label4;

		// Token: 0x04000128 RID: 296
		private global::System.Windows.Forms.OpenFileDialog textEditorPath;

		// Token: 0x04000129 RID: 297
		private global::System.Windows.Forms.CheckBox checkbox_SuppressConfirmations;

		// Token: 0x0400012A RID: 298
		private global::System.Windows.Forms.CheckBox checkbox_UseTextEditor;

		// Token: 0x0400012B RID: 299
		private global::System.Windows.Forms.GroupBox groupbox_General;

		// Token: 0x0400012C RID: 300
		private global::System.Windows.Forms.GroupBox groupbox_Workflow;

		// Token: 0x0400012D RID: 301
		private global::System.Windows.Forms.CheckBox checkbox_SaveNoEncryption;

		// Token: 0x0400012E RID: 302
		private global::System.Windows.Forms.OpenFileDialog secondaryDataPath;
	}
}
