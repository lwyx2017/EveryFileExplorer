namespace NDS.UI
{
    partial class BMGViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BMGViewer));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_add = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_remove = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_up = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_down = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_import = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_export = new System.Windows.Forms.ToolStripButton();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.TextColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_add,
            this.toolStripButton_remove,
            this.toolStripButton_up,
            this.toolStripButton_down,
            this.toolStripButton_import,
            this.toolStripButton_export});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(610, 25);
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
            // toolStripButton_up
            // 
            this.toolStripButton_up.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_up.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_up.Image")));
            this.toolStripButton_up.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_up.Name = "toolStripButton_up";
            this.toolStripButton_up.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_up.Text = "Up";
            this.toolStripButton_up.Click += new System.EventHandler(this.toolStripButton_up_Click);
            // 
            // toolStripButton_down
            // 
            this.toolStripButton_down.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_down.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_down.Image")));
            this.toolStripButton_down.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_down.Name = "toolStripButton_down";
            this.toolStripButton_down.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_down.Text = "Down";
            this.toolStripButton_down.Click += new System.EventHandler(this.toolStripButton_down_Click);
            // 
            // toolStripButton_import
            // 
            this.toolStripButton_import.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_import.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_import.Image")));
            this.toolStripButton_import.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_import.Name = "toolStripButton_import";
            this.toolStripButton_import.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_import.Text = "Import";
            this.toolStripButton_import.Click += new System.EventHandler(this.toolStripButton_import_Click);
            // 
            // toolStripButton_export
            // 
            this.toolStripButton_export.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_export.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_export.Image")));
            this.toolStripButton_export.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_export.Name = "toolStripButton_export";
            this.toolStripButton_export.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_export.Text = "Export";
            this.toolStripButton_export.Click += new System.EventHandler(this.toolStripButton_export_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 25);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.dataGridView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.textBox1);
            this.splitContainer1.Size = new System.Drawing.Size(610, 365);
            this.splitContainer1.SplitterDistance = 231;
            this.splitContainer1.TabIndex = 1;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeColumns = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.TextColumn});
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowHeadersWidth = 102;
            this.dataGridView1.Size = new System.Drawing.Size(610, 231);
            this.dataGridView1.TabIndex = 1;
            this.dataGridView1.SelectionChanged += new System.EventHandler(this.dataGridView1_SelectionChanged);
            // 
            // TextColumn
            // 
            this.TextColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.TextColumn.HeaderText = "Text";
            this.TextColumn.MinimumWidth = 12;
            this.TextColumn.Name = "TextColumn";
            this.TextColumn.ReadOnly = true;
            this.TextColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Location = new System.Drawing.Point(0, 0);
            this.textBox1.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(610, 130);
            this.textBox1.TabIndex = 1;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // BMGViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(610, 390);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
            this.Name = "BMGViewer";
            this.Text = "BMGViewer";
            this.Load += new System.EventHandler(this.BMGViewer_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_add;
        private System.Windows.Forms.ToolStripButton toolStripButton_remove;
        private System.Windows.Forms.ToolStripButton toolStripButton_up;
        private System.Windows.Forms.ToolStripButton toolStripButton_down;
        private System.Windows.Forms.ToolStripButton toolStripButton_import;
        private System.Windows.Forms.ToolStripButton toolStripButton_export;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn TextColumn;
        private System.Windows.Forms.TextBox textBox1;
    }
}