namespace MarioKart.UI
{
    partial class MK7ObjFlowViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MK7ObjFlowViewer));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_add = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_remove = new System.Windows.Forms.ToolStripButton();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_add,
            this.toolStripButton_remove});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(561, 25);
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
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 25);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(561, 288);
            this.dataGridView1.TabIndex = 1;
            this.dataGridView1.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellEndEdit);
            this.dataGridView1.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellValueChanged);
            // 
            // MK7ObjFlowViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(561, 313);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.toolStrip1);
            this.Name = "MK7ObjFlowViewer";
            this.Text = "MK7ObjFlowViewer";
            this.Load += new System.EventHandler(this.MK7ObjFlowViewer_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_add;
        private System.Windows.Forms.ToolStripButton toolStripButton_remove;
        private System.Windows.Forms.DataGridView dataGridView1;
    }
}