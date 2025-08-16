namespace GCNWii.UI
{
	partial class BTIGenDialog
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
            this.comboBoxTextureFormat = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxPaletteFormat = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // comboBoxTextureFormat
            // 
            this.comboBoxTextureFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTextureFormat.FormattingEnabled = true;
            this.comboBoxTextureFormat.Items.AddRange(new object[] {
            "I4",
            "I8",
            "IA4",
            "IA8",
            "RGB565",
            "RGB5A3",
            "RGBA32",
            "CI4",
            "CI8",
            "CI14X2",
            "CMPR"});
            this.comboBoxTextureFormat.Location = new System.Drawing.Point(105, 10);
            this.comboBoxTextureFormat.Name = "comboBoxTextureFormat";
            this.comboBoxTextureFormat.Size = new System.Drawing.Size(121, 20);
            this.comboBoxTextureFormat.TabIndex = 0;
            this.comboBoxTextureFormat.SelectedIndexChanged += new System.EventHandler(this.comboBoxTextureFormat_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "TextureFormat: ";
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(151, 75);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 21);
            this.button1.TabIndex = 2;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "PaletteFormat:";
            // 
            // comboBoxPaletteFormat
            // 
            this.comboBoxPaletteFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPaletteFormat.FormattingEnabled = true;
            this.comboBoxPaletteFormat.Items.AddRange(new object[] {
            "IA8",
            "RGB565",
            "RGB5A3"});
            this.comboBoxPaletteFormat.Location = new System.Drawing.Point(105, 42);
            this.comboBoxPaletteFormat.Name = "comboBoxPaletteFormat";
            this.comboBoxPaletteFormat.Size = new System.Drawing.Size(121, 20);
            this.comboBoxPaletteFormat.TabIndex = 4;
            // 
            // BTIGenDialog
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(234, 107);
            this.ControlBox = false;
            this.Controls.Add(this.comboBoxPaletteFormat);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxTextureFormat);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BTIGenDialog";
            this.Text = "BTIGenDialog";
            this.Load += new System.EventHandler(this.BTIGenDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ComboBox comboBoxTextureFormat;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxPaletteFormat;
    }
}