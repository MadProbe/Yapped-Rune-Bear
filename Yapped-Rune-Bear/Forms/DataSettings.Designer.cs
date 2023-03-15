namespace Chomp.Forms {
    // Token: 0x0200000C RID: 12
    public partial class DataSettings : global::System.Windows.Forms.Form {
        // Token: 0x06000119 RID: 281 RVA: 0x000100CC File Offset: 0x0000E2CC
        protected override void Dispose(bool disposing) {
            if (disposing) {
                this.components?.Dispose();
            }
            base.Dispose(disposing);
        }

        // Token: 0x0600011A RID: 282 RVA: 0x000100EC File Offset: 0x0000E2EC
        private void InitializeComponent() {
            global::System.ComponentModel.ComponentResourceManager resources = new global::System.ComponentModel.ComponentResourceManager(typeof(global::Chomp.Forms.DataSettings));
            this.groupbox_Data = new global::System.Windows.Forms.GroupBox();
            this.checkbox_UnfurledCSVExport = new global::System.Windows.Forms.CheckBox();
            this.checkbox_ExportUniqueOnly = new global::System.Windows.Forms.CheckBox();
            this.checkbox_IncludeRowNames = new global::System.Windows.Forms.CheckBox();
            this.checkbox_IncludeHeader = new global::System.Windows.Forms.CheckBox();
            this.label_CSV_Delimiter = new global::System.Windows.Forms.Label();
            this.textbox_CSV_Delimiter = new global::System.Windows.Forms.TextBox();
            this.btnSaveSettings = new global::System.Windows.Forms.Button();
            this.btnCancel = new global::System.Windows.Forms.Button();
            this.groupBox1 = new global::System.Windows.Forms.GroupBox();
            this.checkbox_EnableFieldValidation = new global::System.Windows.Forms.CheckBox();
            this.groupbox_Data.SuspendLayout();
            this.groupBox1.SuspendLayout();
            base.SuspendLayout();
            this.groupbox_Data.Controls.Add(this.checkbox_UnfurledCSVExport);
            this.groupbox_Data.Controls.Add(this.checkbox_ExportUniqueOnly);
            this.groupbox_Data.Controls.Add(this.checkbox_IncludeRowNames);
            this.groupbox_Data.Controls.Add(this.checkbox_IncludeHeader);
            this.groupbox_Data.Controls.Add(this.label_CSV_Delimiter);
            this.groupbox_Data.Controls.Add(this.textbox_CSV_Delimiter);
            this.groupbox_Data.Location = new global::System.Drawing.Point(12, 53);
            this.groupbox_Data.Name = "groupbox_Data";
            this.groupbox_Data.Size = new global::System.Drawing.Size(255, 153);
            this.groupbox_Data.TabIndex = 32;
            this.groupbox_Data.TabStop = false;
            this.groupbox_Data.Text = "Data Export";
            this.checkbox_UnfurledCSVExport.AutoSize = true;
            this.checkbox_UnfurledCSVExport.Location = new global::System.Drawing.Point(6, 127);
            this.checkbox_UnfurledCSVExport.Name = "checkbox_UnfurledCSVExport";
            this.checkbox_UnfurledCSVExport.Size = new global::System.Drawing.Size(169, 17);
            this.checkbox_UnfurledCSVExport.TabIndex = 10;
            this.checkbox_UnfurledCSVExport.Text = "Unfurl Output in Exported CSV";
            this.checkbox_UnfurledCSVExport.UseVisualStyleBackColor = true;
            this.checkbox_ExportUniqueOnly.AutoSize = true;
            this.checkbox_ExportUniqueOnly.Location = new global::System.Drawing.Point(6, 104);
            this.checkbox_ExportUniqueOnly.Name = "checkbox_ExportUniqueOnly";
            this.checkbox_ExportUniqueOnly.Size = new global::System.Drawing.Size(154, 17);
            this.checkbox_ExportUniqueOnly.TabIndex = 9;
            this.checkbox_ExportUniqueOnly.Text = "Collate Unique Values Only";
            this.checkbox_ExportUniqueOnly.UseVisualStyleBackColor = true;
            this.checkbox_IncludeRowNames.AutoSize = true;
            this.checkbox_IncludeRowNames.Location = new global::System.Drawing.Point(6, 61);
            this.checkbox_IncludeRowNames.Name = "checkbox_IncludeRowNames";
            this.checkbox_IncludeRowNames.Size = new global::System.Drawing.Size(202, 17);
            this.checkbox_IncludeRowNames.TabIndex = 6;
            this.checkbox_IncludeRowNames.Text = "Include Row Names in Exported CSV";
            this.checkbox_IncludeRowNames.UseVisualStyleBackColor = true;
            this.checkbox_IncludeHeader.AutoSize = true;
            this.checkbox_IncludeHeader.Location = new global::System.Drawing.Point(6, 82);
            this.checkbox_IncludeHeader.Name = "checkbox_IncludeHeader";
            this.checkbox_IncludeHeader.Size = new global::System.Drawing.Size(204, 17);
            this.checkbox_IncludeHeader.TabIndex = 5;
            this.checkbox_IncludeHeader.Text = "Include Header Row in Exported CSV";
            this.checkbox_IncludeHeader.UseVisualStyleBackColor = true;
            this.label_CSV_Delimiter.AutoSize = true;
            this.label_CSV_Delimiter.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 8.25f, global::System.Drawing.FontStyle.Regular, global::System.Drawing.GraphicsUnit.Point, 0);
            this.label_CSV_Delimiter.Location = new global::System.Drawing.Point(6, 19);
            this.label_CSV_Delimiter.Name = "label_CSV_Delimiter";
            this.label_CSV_Delimiter.Size = new global::System.Drawing.Size(71, 13);
            this.label_CSV_Delimiter.TabIndex = 7;
            this.label_CSV_Delimiter.Text = "CSV Delimiter";
            this.textbox_CSV_Delimiter.Location = new global::System.Drawing.Point(6, 35);
            this.textbox_CSV_Delimiter.Name = "textbox_CSV_Delimiter";
            this.textbox_CSV_Delimiter.Size = new global::System.Drawing.Size(238, 20);
            this.textbox_CSV_Delimiter.TabIndex = 8;
            this.btnSaveSettings.Location = new global::System.Drawing.Point(12, 212);
            this.btnSaveSettings.Name = "btnSaveSettings";
            this.btnSaveSettings.Size = new global::System.Drawing.Size(75, 23);
            this.btnSaveSettings.TabIndex = 33;
            this.btnSaveSettings.Text = "Save";
            this.btnSaveSettings.UseVisualStyleBackColor = true;
            this.btnSaveSettings.Click += new global::System.EventHandler(this.btnSaveSettings_Click);
            this.btnCancel.DialogResult = global::System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new global::System.Drawing.Point(192, 212);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new global::System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 34;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new global::System.EventHandler(this.btnCancel_Click);
            this.groupBox1.Controls.Add(this.checkbox_EnableFieldValidation);
            this.groupBox1.Location = new global::System.Drawing.Point(12, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new global::System.Drawing.Size(255, 41);
            this.groupBox1.TabIndex = 33;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Yapped";
            this.checkbox_EnableFieldValidation.AutoSize = true;
            this.checkbox_EnableFieldValidation.Location = new global::System.Drawing.Point(6, 19);
            this.checkbox_EnableFieldValidation.Name = "checkbox_EnableFieldValidation";
            this.checkbox_EnableFieldValidation.Size = new global::System.Drawing.Size(133, 17);
            this.checkbox_EnableFieldValidation.TabIndex = 6;
            this.checkbox_EnableFieldValidation.Text = "Enable Field Validation";
            this.checkbox_EnableFieldValidation.TextAlign = global::System.Drawing.ContentAlignment.MiddleCenter;
            this.checkbox_EnableFieldValidation.UseVisualStyleBackColor = true;
            base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
            base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
            base.ClientSize = new global::System.Drawing.Size(275, 244);
            base.Controls.Add(this.groupBox1);
            base.Controls.Add(this.btnCancel);
            base.Controls.Add(this.btnSaveSettings);
            base.Controls.Add(this.groupbox_Data);
            base.Icon = (global::System.Drawing.Icon)resources.GetObject("$this.Icon");
            base.Name = "DataSettings";
            base.ShowIcon = false;
            base.StartPosition = global::System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Data Settings";
            this.groupbox_Data.ResumeLayout(false);
            this.groupbox_Data.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            base.ResumeLayout(false);
        }

        // Token: 0x04000091 RID: 145
        private global::System.ComponentModel.IContainer components;

        // Token: 0x04000092 RID: 146
        private global::System.Windows.Forms.GroupBox groupbox_Data;

        // Token: 0x04000093 RID: 147
        private global::System.Windows.Forms.CheckBox checkbox_UnfurledCSVExport;

        // Token: 0x04000094 RID: 148
        private global::System.Windows.Forms.CheckBox checkbox_ExportUniqueOnly;

        // Token: 0x04000095 RID: 149
        private global::System.Windows.Forms.CheckBox checkbox_IncludeRowNames;

        // Token: 0x04000096 RID: 150
        private global::System.Windows.Forms.CheckBox checkbox_IncludeHeader;

        // Token: 0x04000097 RID: 151
        private global::System.Windows.Forms.Label label_CSV_Delimiter;

        // Token: 0x04000098 RID: 152
        private global::System.Windows.Forms.TextBox textbox_CSV_Delimiter;

        // Token: 0x04000099 RID: 153
        private global::System.Windows.Forms.Button btnSaveSettings;

        // Token: 0x0400009A RID: 154
        private global::System.Windows.Forms.Button btnCancel;

        // Token: 0x0400009B RID: 155
        private global::System.Windows.Forms.GroupBox groupBox1;

        // Token: 0x0400009C RID: 156
        private global::System.Windows.Forms.CheckBox checkbox_EnableFieldValidation;
    }
}
