namespace MarioKart.UI
{
    partial class MKDSKCLViewer
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
            this.Map = new System.Windows.Forms.TabPage();
            this.Vertex = new System.Windows.Forms.TabPage();
            this.Normals = new System.Windows.Forms.TabPage();
            this.listView_Normals = new System.Windows.Forms.ListView();
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Planar = new System.Windows.Forms.TabPage();
            this.listView_Planar = new System.Windows.Forms.ListView();
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader13 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader14 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader15 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader16 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Octree = new System.Windows.Forms.TabPage();
            this.View3D = new System.Windows.Forms.TabPage();
            this.listView_Vertex = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabControl1.SuspendLayout();
            this.Vertex.SuspendLayout();
            this.Normals.SuspendLayout();
            this.Planar.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.Map);
            this.tabControl1.Controls.Add(this.Vertex);
            this.tabControl1.Controls.Add(this.Normals);
            this.tabControl1.Controls.Add(this.Planar);
            this.tabControl1.Controls.Add(this.Octree);
            this.tabControl1.Controls.Add(this.View3D);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(644, 358);
            this.tabControl1.TabIndex = 0;
            // 
            // Map
            // 
            this.Map.Location = new System.Drawing.Point(4, 22);
            this.Map.Name = "Map";
            this.Map.Padding = new System.Windows.Forms.Padding(3);
            this.Map.Size = new System.Drawing.Size(636, 332);
            this.Map.TabIndex = 0;
            this.Map.Text = "Map";
            this.Map.UseVisualStyleBackColor = true;
            // 
            // Vertex
            // 
            this.Vertex.Controls.Add(this.listView_Vertex);
            this.Vertex.Location = new System.Drawing.Point(4, 22);
            this.Vertex.Name = "Vertex";
            this.Vertex.Padding = new System.Windows.Forms.Padding(3);
            this.Vertex.Size = new System.Drawing.Size(636, 332);
            this.Vertex.TabIndex = 1;
            this.Vertex.Text = "Vertex";
            this.Vertex.UseVisualStyleBackColor = true;
            // 
            // Normals
            // 
            this.Normals.Controls.Add(this.listView_Normals);
            this.Normals.Location = new System.Drawing.Point(4, 22);
            this.Normals.Name = "Normals";
            this.Normals.Size = new System.Drawing.Size(636, 332);
            this.Normals.TabIndex = 2;
            this.Normals.Text = "Normals";
            this.Normals.UseVisualStyleBackColor = true;
            // 
            // listView_Normals
            // 
            this.listView_Normals.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8});
            this.listView_Normals.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_Normals.FullRowSelect = true;
            this.listView_Normals.HideSelection = false;
            this.listView_Normals.Location = new System.Drawing.Point(0, 0);
            this.listView_Normals.Margin = new System.Windows.Forms.Padding(6);
            this.listView_Normals.Name = "listView_Normals";
            this.listView_Normals.Size = new System.Drawing.Size(636, 332);
            this.listView_Normals.TabIndex = 4;
            this.listView_Normals.UseCompatibleStateImageBehavior = false;
            this.listView_Normals.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "ID";
            this.columnHeader5.Width = 73;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "X";
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Y (Height)";
            this.columnHeader7.Width = 105;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Z";
            // 
            // Planar
            // 
            this.Planar.Controls.Add(this.listView_Planar);
            this.Planar.Location = new System.Drawing.Point(4, 22);
            this.Planar.Name = "Planar";
            this.Planar.Size = new System.Drawing.Size(636, 332);
            this.Planar.TabIndex = 3;
            this.Planar.Text = "Planar";
            this.Planar.UseVisualStyleBackColor = true;
            // 
            // listView_Planar
            // 
            this.listView_Planar.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader9,
            this.columnHeader10,
            this.columnHeader11,
            this.columnHeader12,
            this.columnHeader13,
            this.columnHeader14,
            this.columnHeader15,
            this.columnHeader16});
            this.listView_Planar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_Planar.FullRowSelect = true;
            this.listView_Planar.HideSelection = false;
            this.listView_Planar.Location = new System.Drawing.Point(0, 0);
            this.listView_Planar.Margin = new System.Windows.Forms.Padding(6);
            this.listView_Planar.Name = "listView_Planar";
            this.listView_Planar.Size = new System.Drawing.Size(636, 332);
            this.listView_Planar.TabIndex = 5;
            this.listView_Planar.UseCompatibleStateImageBehavior = false;
            this.listView_Planar.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "ID";
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "Length";
            // 
            // columnHeader11
            // 
            this.columnHeader11.Text = "Vertex Index";
            this.columnHeader11.Width = 95;
            // 
            // columnHeader12
            // 
            this.columnHeader12.Text = "Normal Index";
            this.columnHeader12.Width = 94;
            // 
            // columnHeader13
            // 
            this.columnHeader13.Text = "Normal A Index";
            this.columnHeader13.Width = 106;
            // 
            // columnHeader14
            // 
            this.columnHeader14.Text = "Normal B Index";
            this.columnHeader14.Width = 100;
            // 
            // columnHeader15
            // 
            this.columnHeader15.Text = "Normal C Index";
            this.columnHeader15.Width = 111;
            // 
            // columnHeader16
            // 
            this.columnHeader16.Text = "Collision Type";
            this.columnHeader16.Width = 107;
            // 
            // Octree
            // 
            this.Octree.Location = new System.Drawing.Point(4, 22);
            this.Octree.Name = "Octree";
            this.Octree.Size = new System.Drawing.Size(636, 332);
            this.Octree.TabIndex = 4;
            this.Octree.Text = "Octree";
            this.Octree.UseVisualStyleBackColor = true;
            // 
            // View3D
            // 
            this.View3D.Location = new System.Drawing.Point(4, 22);
            this.View3D.Name = "View3D";
            this.View3D.Size = new System.Drawing.Size(636, 332);
            this.View3D.TabIndex = 5;
            this.View3D.Text = "3D View";
            this.View3D.UseVisualStyleBackColor = true;
            // 
            // listView_Vertex
            // 
            this.listView_Vertex.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.listView_Vertex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_Vertex.FullRowSelect = true;
            this.listView_Vertex.HideSelection = false;
            this.listView_Vertex.Location = new System.Drawing.Point(3, 3);
            this.listView_Vertex.Margin = new System.Windows.Forms.Padding(6);
            this.listView_Vertex.Name = "listView_Vertex";
            this.listView_Vertex.Size = new System.Drawing.Size(630, 326);
            this.listView_Vertex.TabIndex = 6;
            this.listView_Vertex.UseCompatibleStateImageBehavior = false;
            this.listView_Vertex.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "ID";
            this.columnHeader1.Width = 73;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "X";
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Y (Height)";
            this.columnHeader3.Width = 105;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Z";
            // 
            // MKDSKCLViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(644, 358);
            this.Controls.Add(this.tabControl1);
            this.Name = "MKDSKCLViewer";
            this.Text = "MKDSKCLViewer";
            this.Load += new System.EventHandler(this.MKDSKCLViewer_Load);
            this.tabControl1.ResumeLayout(false);
            this.Vertex.ResumeLayout(false);
            this.Normals.ResumeLayout(false);
            this.Planar.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage Map;
        private System.Windows.Forms.TabPage Vertex;
        private System.Windows.Forms.TabPage Normals;
        private System.Windows.Forms.TabPage Planar;
        private System.Windows.Forms.TabPage Octree;
        private System.Windows.Forms.TabPage View3D;
        private System.Windows.Forms.ListView listView_Normals;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ListView listView_Planar;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.ColumnHeader columnHeader12;
        private System.Windows.Forms.ColumnHeader columnHeader13;
        private System.Windows.Forms.ColumnHeader columnHeader14;
        private System.Windows.Forms.ColumnHeader columnHeader15;
        private System.Windows.Forms.ColumnHeader columnHeader16;
        private System.Windows.Forms.ListView listView_Vertex;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
    }
}