namespace _3DS.UI
{
    partial class CBMDViewer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.Model = new System.Windows.Forms.TabPage();
            this.Sound = new System.Windows.Forms.TabPage();
            this.button_Export = new System.Windows.Forms.Button();
            this.button_Export_Decompressed = new System.Windows.Forms.Button();
            this.button_cwav_export = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.tabControl1.SuspendLayout();
            this.Model.SuspendLayout();
            this.Sound.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.Model);
            this.tabControl1.Controls.Add(this.Sound);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(634, 543);
            this.tabControl1.TabIndex = 0;
            // 
            // Model
            // 
            this.Model.Controls.Add(this.button_Export_Decompressed);
            this.Model.Controls.Add(this.button_Export);
            this.Model.Location = new System.Drawing.Point(10, 48);
            this.Model.Name = "Model";
            this.Model.Padding = new System.Windows.Forms.Padding(3);
            this.Model.Size = new System.Drawing.Size(614, 485);
            this.Model.TabIndex = 0;
            this.Model.Text = "Model";
            this.Model.UseVisualStyleBackColor = true;
            // 
            // Sound
            // 
            this.Sound.Controls.Add(this.button_cwav_export);
            this.Sound.Location = new System.Drawing.Point(10, 48);
            this.Sound.Name = "Sound";
            this.Sound.Padding = new System.Windows.Forms.Padding(3);
            this.Sound.Size = new System.Drawing.Size(614, 485);
            this.Sound.TabIndex = 1;
            this.Sound.Text = "Sound";
            this.Sound.UseVisualStyleBackColor = true;
            // 
            // button_Export
            // 
            this.button_Export.Location = new System.Drawing.Point(173, 64);
            this.button_Export.Name = "button_Export";
            this.button_Export.Size = new System.Drawing.Size(223, 144);
            this.button_Export.TabIndex = 1;
            this.button_Export.Text = "Export";
            this.button_Export.UseVisualStyleBackColor = true;
            this.button_Export.Click += new System.EventHandler(this.button_Export_Click);
            // 
            // button_Export_Decompressed
            // 
            this.button_Export_Decompressed.Location = new System.Drawing.Point(173, 242);
            this.button_Export_Decompressed.Name = "button_Export_Decompressed";
            this.button_Export_Decompressed.Size = new System.Drawing.Size(223, 153);
            this.button_Export_Decompressed.TabIndex = 2;
            this.button_Export_Decompressed.Text = "Export Decompressed";
            this.button_Export_Decompressed.UseVisualStyleBackColor = true;
            this.button_Export_Decompressed.Click += new System.EventHandler(this.button_Export_Decompressed_Click);
            // 
            // button_cwav_export
            // 
            this.button_cwav_export.Location = new System.Drawing.Point(185, 139);
            this.button_cwav_export.Name = "button_cwav_export";
            this.button_cwav_export.Size = new System.Drawing.Size(213, 145);
            this.button_cwav_export.TabIndex = 0;
            this.button_cwav_export.Text = "Export";
            this.button_cwav_export.UseVisualStyleBackColor = true;
            this.button_cwav_export.Click += new System.EventHandler(this.button_cwav_export_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "All Files (*.*)|*.*";
            this.openFileDialog1.Title = "Import File";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Title = "Export...";
            // 
            // CBMDViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(15F, 30F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(634, 543);
            this.Controls.Add(this.tabControl1);
            this.Name = "CBMDViewer";
            this.Text = "CBMDViewer";
            this.tabControl1.ResumeLayout(false);
            this.Model.ResumeLayout(false);
            this.Sound.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage Model;
        private System.Windows.Forms.Button button_Export;
        private System.Windows.Forms.TabPage Sound;
        private System.Windows.Forms.Button button_Export_Decompressed;
        private System.Windows.Forms.Button button_cwav_export;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
    }
}