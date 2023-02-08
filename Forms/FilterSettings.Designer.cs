namespace Chomp.Forms {
    // Token: 0x02000010 RID: 16
    public partial class FilterSettings : global::System.Windows.Forms.Form {
        // Token: 0x0600012E RID: 302 RVA: 0x00012565 File Offset: 0x00010765
        protected override void Dispose(bool disposing) {
            if (disposing && this.components != null) {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        // Token: 0x0600012F RID: 303 RVA: 0x00012584 File Offset: 0x00010784
        private void InitializeComponent() {
            this.group_Filter = new global::System.Windows.Forms.GroupBox();
            this.label1 = new global::System.Windows.Forms.Label();
            this.textbox_Filter_CommandDelimiter = new global::System.Windows.Forms.TextBox();
            this.label2 = new global::System.Windows.Forms.Label();
            this.textbox_Filter_SectionDelimiter = new global::System.Windows.Forms.TextBox();
            this.btnSaveSettings = new global::System.Windows.Forms.Button();
            this.btnCancel = new global::System.Windows.Forms.Button();
            this.group_Filter.SuspendLayout();
            base.SuspendLayout();
            this.group_Filter.Controls.Add(this.textbox_Filter_SectionDelimiter);
            this.group_Filter.Controls.Add(this.label2);
            this.group_Filter.Controls.Add(this.textbox_Filter_CommandDelimiter);
            this.group_Filter.Controls.Add(this.label1);
            this.group_Filter.Location = new global::System.Drawing.Point(12, 12);
            this.group_Filter.Name = "group_Filter";
            this.group_Filter.Size = new global::System.Drawing.Size(250, 112);
            this.group_Filter.TabIndex = 35;
            this.group_Filter.TabStop = false;
            this.group_Filter.Text = "Filter ";
            this.label1.AutoSize = true;
            this.label1.Location = new global::System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new global::System.Drawing.Size(122, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Filter Command Delimiter";
            this.textbox_Filter_CommandDelimiter.Location = new global::System.Drawing.Point(9, 37);
            this.textbox_Filter_CommandDelimiter.Name = "textbox_Filter_CommandDelimiter";
            this.textbox_Filter_CommandDelimiter.Size = new global::System.Drawing.Size(235, 20);
            this.textbox_Filter_CommandDelimiter.TabIndex = 1;
            this.label2.AutoSize = true;
            this.label2.Location = new global::System.Drawing.Point(7, 64);
            this.label2.Name = "label2";
            this.label2.Size = new global::System.Drawing.Size(111, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Filter Section Delimiter";
            this.textbox_Filter_SectionDelimiter.Location = new global::System.Drawing.Point(9, 80);
            this.textbox_Filter_SectionDelimiter.Name = "textbox_Filter_SectionDelimiter";
            this.textbox_Filter_SectionDelimiter.Size = new global::System.Drawing.Size(235, 20);
            this.textbox_Filter_SectionDelimiter.TabIndex = 3;
            this.btnSaveSettings.Location = new global::System.Drawing.Point(12, 130);
            this.btnSaveSettings.Name = "btnSaveSettings";
            this.btnSaveSettings.Size = new global::System.Drawing.Size(75, 23);
            this.btnSaveSettings.TabIndex = 36;
            this.btnSaveSettings.Text = "Save";
            this.btnSaveSettings.UseVisualStyleBackColor = true;
            this.btnSaveSettings.Click += new global::System.EventHandler(this.btnSaveSettings_Click);
            this.btnCancel.DialogResult = global::System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new global::System.Drawing.Point(187, 130);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new global::System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 37;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new global::System.EventHandler(this.btnCancel_Click);
            base.AutoScaleDimensions = new global::System.Drawing.SizeF(6f, 13f);
            base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
            base.ClientSize = new global::System.Drawing.Size(273, 167);
            base.Controls.Add(this.btnSaveSettings);
            base.Controls.Add(this.btnCancel);
            base.Controls.Add(this.group_Filter);
            base.Name = "FilterSettings";
            base.ShowIcon = false;
            base.StartPosition = global::System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Filter Settings";
            this.group_Filter.ResumeLayout(false);
            this.group_Filter.PerformLayout();
            base.ResumeLayout(false);
        }

        // Token: 0x040000D7 RID: 215
        private global::System.ComponentModel.IContainer components;

        // Token: 0x040000D8 RID: 216
        private global::System.Windows.Forms.GroupBox group_Filter;

        // Token: 0x040000D9 RID: 217
        private global::System.Windows.Forms.TextBox textbox_Filter_SectionDelimiter;

        // Token: 0x040000DA RID: 218
        private global::System.Windows.Forms.Label label2;

        // Token: 0x040000DB RID: 219
        private global::System.Windows.Forms.TextBox textbox_Filter_CommandDelimiter;

        // Token: 0x040000DC RID: 220
        private global::System.Windows.Forms.Label label1;

        // Token: 0x040000DD RID: 221
        private global::System.Windows.Forms.Button btnSaveSettings;

        // Token: 0x040000DE RID: 222
        private global::System.Windows.Forms.Button btnCancel;
    }
}
