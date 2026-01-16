namespace NDS.UI
{
    partial class NCLRViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NCLRViewer));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_add = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_remove = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_copy = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_paste = new System.Windows.Forms.ToolStripButton();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.TrackBarLuminosity = new System.Windows.Forms.TrackBar();
            this.label5 = new System.Windows.Forms.Label();
            this.TrackBarSaturation = new System.Windows.Forms.TrackBar();
            this.label4 = new System.Windows.Forms.Label();
            this.TrackBarHue = new System.Windows.Forms.TrackBar();
            this.numericUpDownBlue = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.trackBarBlue = new System.Windows.Forms.TrackBar();
            this.numericUpDownGreen = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.trackBarGreen = new System.Windows.Forms.TrackBar();
            this.numericUpDownRed = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.panelColor = new System.Windows.Forms.Panel();
            this.trackBarRed = new System.Windows.Forms.TrackBar();
            this.toolStripComboBox_format = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripLabel_format = new System.Windows.Forms.ToolStripLabel();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TrackBarLuminosity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TrackBarSaturation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TrackBarHue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBlue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarBlue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownGreen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarGreen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarRed)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_add,
            this.toolStripButton_remove,
            this.toolStripButton_copy,
            this.toolStripButton_paste,
            this.toolStripComboBox_format,
            this.toolStripLabel_format});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(693, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_add
            // 
            this.toolStripButton_add.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_add.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_add.Image")));
            this.toolStripButton_add.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_add.Name = "toolStripButton_add";
            this.toolStripButton_add.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_add.Text = "Add";
            this.toolStripButton_add.Click += new System.EventHandler(this.toolStripButton_add_Click);
            // 
            // toolStripButton_remove
            // 
            this.toolStripButton_remove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_remove.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_remove.Image")));
            this.toolStripButton_remove.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_remove.Name = "toolStripButton_remove";
            this.toolStripButton_remove.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_remove.Text = "Remove";
            this.toolStripButton_remove.Click += new System.EventHandler(this.toolStripButton_remove_Click);
            // 
            // toolStripButton_copy
            // 
            this.toolStripButton_copy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_copy.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_copy.Image")));
            this.toolStripButton_copy.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_copy.Name = "toolStripButton_copy";
            this.toolStripButton_copy.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_copy.Text = "Copy";
            this.toolStripButton_copy.Click += new System.EventHandler(this.toolStripButton_copy_Click);
            // 
            // toolStripButton_paste
            // 
            this.toolStripButton_paste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_paste.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_paste.Image")));
            this.toolStripButton_paste.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_paste.Name = "toolStripButton_paste";
            this.toolStripButton_paste.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_paste.Text = "Paste";
            this.toolStripButton_paste.Click += new System.EventHandler(this.toolStripButton_paste_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 25);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer1.Panel2.Controls.Add(this.numericUpDownBlue);
            this.splitContainer1.Panel2.Controls.Add(this.label3);
            this.splitContainer1.Panel2.Controls.Add(this.trackBarBlue);
            this.splitContainer1.Panel2.Controls.Add(this.numericUpDownGreen);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.trackBarGreen);
            this.splitContainer1.Panel2.Controls.Add(this.numericUpDownRed);
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Panel2.Controls.Add(this.panelColor);
            this.splitContainer1.Panel2.Controls.Add(this.trackBarRed);
            this.splitContainer1.Size = new System.Drawing.Size(693, 372);
            this.splitContainer1.SplitterDistance = 416;
            this.splitContainer1.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.TrackBarLuminosity);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.TrackBarSaturation);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.TrackBarHue);
            this.groupBox1.Location = new System.Drawing.Point(3, 111);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(256, 219);
            this.groupBox1.TabIndex = 20;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "All Colors";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 124);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(71, 12);
            this.label6.TabIndex = 15;
            this.label6.Text = "Luminosity:";
            // 
            // TrackBarLuminosity
            // 
            this.TrackBarLuminosity.Location = new System.Drawing.Point(71, 114);
            this.TrackBarLuminosity.Maximum = 240;
            this.TrackBarLuminosity.Minimum = -240;
            this.TrackBarLuminosity.Name = "TrackBarLuminosity";
            this.TrackBarLuminosity.Size = new System.Drawing.Size(176, 45);
            this.TrackBarLuminosity.TabIndex = 14;
            this.TrackBarLuminosity.TickFrequency = 24;
            this.TrackBarLuminosity.Scroll += new System.EventHandler(this.TrackBarLuminosity_Scroll);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 77);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 12);
            this.label5.TabIndex = 13;
            this.label5.Text = "Saturation:";
            // 
            // TrackBarSaturation
            // 
            this.TrackBarSaturation.Location = new System.Drawing.Point(71, 67);
            this.TrackBarSaturation.Maximum = 240;
            this.TrackBarSaturation.Minimum = -240;
            this.TrackBarSaturation.Name = "TrackBarSaturation";
            this.TrackBarSaturation.Size = new System.Drawing.Size(176, 45);
            this.TrackBarSaturation.TabIndex = 12;
            this.TrackBarSaturation.TickFrequency = 24;
            this.TrackBarSaturation.Scroll += new System.EventHandler(this.TrackBarSaturation_Scroll);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(35, 30);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 11;
            this.label4.Text = "Hue:";
            // 
            // TrackBarHue
            // 
            this.TrackBarHue.Location = new System.Drawing.Point(71, 20);
            this.TrackBarHue.Maximum = 240;
            this.TrackBarHue.Minimum = -240;
            this.TrackBarHue.Name = "TrackBarHue";
            this.TrackBarHue.Size = new System.Drawing.Size(176, 45);
            this.TrackBarHue.TabIndex = 10;
            this.TrackBarHue.TickFrequency = 24;
            this.TrackBarHue.Scroll += new System.EventHandler(this.TrackBarHue_Scroll);
            // 
            // numericUpDownBlue
            // 
            this.numericUpDownBlue.Enabled = false;
            this.numericUpDownBlue.Location = new System.Drawing.Point(81, 72);
            this.numericUpDownBlue.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownBlue.Name = "numericUpDownBlue";
            this.numericUpDownBlue.Size = new System.Drawing.Size(44, 21);
            this.numericUpDownBlue.TabIndex = 19;
            this.numericUpDownBlue.ValueChanged += new System.EventHandler(this.numericUpDownBlue_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(57, 74);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 12);
            this.label3.TabIndex = 18;
            this.label3.Text = "B:";
            // 
            // trackBarBlue
            // 
            this.trackBarBlue.Enabled = false;
            this.trackBarBlue.Location = new System.Drawing.Point(131, 69);
            this.trackBarBlue.Maximum = 255;
            this.trackBarBlue.MaximumSize = new System.Drawing.Size(125, 28);
            this.trackBarBlue.MinimumSize = new System.Drawing.Size(125, 28);
            this.trackBarBlue.Name = "trackBarBlue";
            this.trackBarBlue.Size = new System.Drawing.Size(125, 45);
            this.trackBarBlue.TabIndex = 17;
            this.trackBarBlue.TickFrequency = 16;
            this.trackBarBlue.ValueChanged += new System.EventHandler(this.trackBarBlue_ValueChanged);
            // 
            // numericUpDownGreen
            // 
            this.numericUpDownGreen.Enabled = false;
            this.numericUpDownGreen.Location = new System.Drawing.Point(81, 39);
            this.numericUpDownGreen.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownGreen.Name = "numericUpDownGreen";
            this.numericUpDownGreen.Size = new System.Drawing.Size(44, 21);
            this.numericUpDownGreen.TabIndex = 16;
            this.numericUpDownGreen.ValueChanged += new System.EventHandler(this.numericUpDownGreen_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(57, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 12);
            this.label2.TabIndex = 15;
            this.label2.Text = "G:";
            // 
            // trackBarGreen
            // 
            this.trackBarGreen.Enabled = false;
            this.trackBarGreen.Location = new System.Drawing.Point(131, 36);
            this.trackBarGreen.Maximum = 255;
            this.trackBarGreen.MaximumSize = new System.Drawing.Size(125, 28);
            this.trackBarGreen.MinimumSize = new System.Drawing.Size(125, 28);
            this.trackBarGreen.Name = "trackBarGreen";
            this.trackBarGreen.Size = new System.Drawing.Size(125, 45);
            this.trackBarGreen.TabIndex = 14;
            this.trackBarGreen.TickFrequency = 16;
            this.trackBarGreen.ValueChanged += new System.EventHandler(this.trackBarGreen_ValueChanged);
            // 
            // numericUpDownRed
            // 
            this.numericUpDownRed.Enabled = false;
            this.numericUpDownRed.Location = new System.Drawing.Point(81, 6);
            this.numericUpDownRed.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownRed.Name = "numericUpDownRed";
            this.numericUpDownRed.Size = new System.Drawing.Size(44, 21);
            this.numericUpDownRed.TabIndex = 13;
            this.numericUpDownRed.ValueChanged += new System.EventHandler(this.numericUpDownRed_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(57, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(17, 12);
            this.label1.TabIndex = 12;
            this.label1.Text = "R:";
            // 
            // panelColor
            // 
            this.panelColor.BackColor = System.Drawing.Color.Black;
            this.panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColor.Location = new System.Drawing.Point(3, 3);
            this.panelColor.Name = "panelColor";
            this.panelColor.Size = new System.Drawing.Size(48, 44);
            this.panelColor.TabIndex = 11;
            // 
            // trackBarRed
            // 
            this.trackBarRed.Enabled = false;
            this.trackBarRed.Location = new System.Drawing.Point(131, 3);
            this.trackBarRed.Maximum = 255;
            this.trackBarRed.MaximumSize = new System.Drawing.Size(125, 28);
            this.trackBarRed.MinimumSize = new System.Drawing.Size(125, 28);
            this.trackBarRed.Name = "trackBarRed";
            this.trackBarRed.Size = new System.Drawing.Size(125, 45);
            this.trackBarRed.TabIndex = 10;
            this.trackBarRed.TickFrequency = 16;
            this.trackBarRed.ValueChanged += new System.EventHandler(this.trackBarRed_ValueChanged);
            // 
            // toolStripComboBox_format
            // 
            this.toolStripComboBox_format.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripComboBox_format.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBox_format.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.toolStripComboBox_format.Name = "toolStripComboBox_format";
            this.toolStripComboBox_format.Size = new System.Drawing.Size(121, 25);
            this.toolStripComboBox_format.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBox_format_SelectedIndexChanged);
            // 
            // toolStripLabel_format
            // 
            this.toolStripLabel_format.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel_format.Name = "toolStripLabel_format";
            this.toolStripLabel_format.Size = new System.Drawing.Size(52, 22);
            this.toolStripLabel_format.Text = "Format:";
            // 
            // NCLRViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(693, 397);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
            this.Name = "NCLRViewer";
            this.Text = "NCLRViewer";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TrackBarLuminosity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TrackBarSaturation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TrackBarHue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBlue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarBlue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownGreen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarGreen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarRed)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_add;
        private System.Windows.Forms.ToolStripButton toolStripButton_remove;
        private System.Windows.Forms.ToolStripButton toolStripButton_copy;
        private System.Windows.Forms.ToolStripButton toolStripButton_paste;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TrackBar TrackBarLuminosity;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TrackBar TrackBarSaturation;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TrackBar TrackBarHue;
        private System.Windows.Forms.NumericUpDown numericUpDownBlue;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TrackBar trackBarBlue;
        private System.Windows.Forms.NumericUpDown numericUpDownGreen;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TrackBar trackBarGreen;
        private System.Windows.Forms.NumericUpDown numericUpDownRed;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panelColor;
        private System.Windows.Forms.TrackBar trackBarRed;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox_format;
        private System.Windows.Forms.ToolStripLabel toolStripLabel_format;
    }
}