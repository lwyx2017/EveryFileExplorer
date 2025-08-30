namespace WiiU.UI
{
	partial class BFLIMGenDialog
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
            this.Format3DS = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.radioButton3DS = new System.Windows.Forms.RadioButton();
            this.radioButtonWiiU = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.Flag3DS = new System.Windows.Forms.ComboBox();
            this.WiiUTileMode = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.FormatWiiU = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // Format3DS
            // 
            this.Format3DS.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Format3DS.FormattingEnabled = true;
            this.Format3DS.Items.AddRange(new object[] {
            "RGBA8",
            "RGB8",
            "RGBA5551",
            "RGB565",
            "RGBA4",
            "LA8",
            "HILO8",
            "L8",
            "A8",
            "LA4",
            "L4",
            "A4",
            "ETC1",
            "ETC1A4"});
            this.Format3DS.Location = new System.Drawing.Point(133, 50);
            this.Format3DS.Name = "Format3DS";
            this.Format3DS.Size = new System.Drawing.Size(121, 20);
            this.Format3DS.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(131, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "Image Format: ";
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(324, 173);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 21);
            this.button1.TabIndex = 2;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(16, 212);
            this.label2.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(389, 37);
            this.label2.TabIndex = 3;
            this.label2.Text = "Note:Now currently only supports creating BFLIM for 3DS, the function of WiiU has" +
    " not been implemented yet.";
            // 
            // radioButton3DS
            // 
            this.radioButton3DS.AutoSize = true;
            this.radioButton3DS.Location = new System.Drawing.Point(75, 22);
            this.radioButton3DS.Name = "radioButton3DS";
            this.radioButton3DS.Size = new System.Drawing.Size(41, 16);
            this.radioButton3DS.TabIndex = 4;
            this.radioButton3DS.TabStop = true;
            this.radioButton3DS.Text = "3DS";
            this.radioButton3DS.UseVisualStyleBackColor = true;
            this.radioButton3DS.CheckedChanged += new System.EventHandler(this.radioButton3DS_CheckedChanged);
            // 
            // radioButtonWiiU
            // 
            this.radioButtonWiiU.AutoSize = true;
            this.radioButtonWiiU.Location = new System.Drawing.Point(75, 97);
            this.radioButtonWiiU.Name = "radioButtonWiiU";
            this.radioButtonWiiU.Size = new System.Drawing.Size(47, 16);
            this.radioButtonWiiU.TabIndex = 5;
            this.radioButtonWiiU.TabStop = true;
            this.radioButtonWiiU.Text = "WiiU";
            this.radioButtonWiiU.UseVisualStyleBackColor = true;
            this.radioButtonWiiU.CheckedChanged += new System.EventHandler(this.radioButtonWiiU_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(276, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 12);
            this.label3.TabIndex = 6;
            this.label3.Text = "Flag:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "Platform:";
            // 
            // Flag3DS
            // 
            this.Flag3DS.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Flag3DS.FormattingEnabled = true;
            this.Flag3DS.Items.AddRange(new object[] {
            "0",
            "2",
            "4",
            "8"});
            this.Flag3DS.Location = new System.Drawing.Point(278, 50);
            this.Flag3DS.Name = "Flag3DS";
            this.Flag3DS.Size = new System.Drawing.Size(121, 20);
            this.Flag3DS.TabIndex = 9;
            // 
            // WiiUTileMode
            // 
            this.WiiUTileMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.WiiUTileMode.FormattingEnabled = true;
            this.WiiUTileMode.Items.AddRange(new object[] {
            "Default",
            "LinearAligned",
            "Tiled1DThin1",
            "Tiled1DThick",
            "Tiled2DThin1",
            "Tiled2DThin2",
            "Tiled2DThin4",
            "Tiled2DThick",
            "Tiled2BThin1",
            "Tiled2BThin2",
            "Tiled2BThin4",
            "Tiled2BThick",
            "Tiled3DThin1",
            "Tiled3DThick",
            "Tiled3BThin1",
            "Tiled3BThick",
            "LinearSpecial"});
            this.WiiUTileMode.Location = new System.Drawing.Point(278, 128);
            this.WiiUTileMode.Name = "WiiUTileMode";
            this.WiiUTileMode.Size = new System.Drawing.Size(121, 20);
            this.WiiUTileMode.TabIndex = 13;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(276, 102);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(59, 12);
            this.label5.TabIndex = 12;
            this.label5.Text = "TileMode:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(131, 102);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(89, 12);
            this.label6.TabIndex = 11;
            this.label6.Text = "Image Format: ";
            // 
            // FormatWiiU
            // 
            this.FormatWiiU.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.FormatWiiU.FormattingEnabled = true;
            this.FormatWiiU.Items.AddRange(new object[] {
            "RGBA8",
            "RGBA8_sRGB",
            "RGBA1010102",
            "RGB8",
            "RGBA5551",
            "RGB555",
            "RGB565",
            "RGBA4",
            "LA8",
            "HILO8",
            "L8",
            "A8",
            "LA4",
            "L4",
            "A4",
            "ETC1",
            "ETC1A4",
            "BC1",
            "BC1_sRGB",
            "BC2",
            "BC2_sRGB",
            "BC3",
            "BC3_sRGB",
            "BC4L",
            "BC4A",
            "BC5"});
            this.FormatWiiU.Location = new System.Drawing.Point(133, 128);
            this.FormatWiiU.Name = "FormatWiiU";
            this.FormatWiiU.Size = new System.Drawing.Size(121, 20);
            this.FormatWiiU.TabIndex = 10;
            // 
            // BFLIMGenDialog
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(416, 256);
            this.ControlBox = false;
            this.Controls.Add(this.WiiUTileMode);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.FormatWiiU);
            this.Controls.Add(this.Flag3DS);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.radioButtonWiiU);
            this.Controls.Add(this.radioButton3DS);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Format3DS);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BFLIMGenDialog";
            this.Text = "BFLIMGenDialog";
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ComboBox Format3DS;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton radioButton3DS;
        private System.Windows.Forms.RadioButton radioButtonWiiU;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox Flag3DS;
        private System.Windows.Forms.ComboBox WiiUTileMode;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox FormatWiiU;
    }
}