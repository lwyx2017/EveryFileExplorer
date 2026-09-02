using System;
using System.Drawing;
using System.Windows.Forms;

namespace MarioKart.UI
{
    public partial class MKDSKCLViewer : Form
    {
        MKDS.KCL MKDSKCL;
        public MKDSKCLViewer(MKDS.KCL kclData)
        {
            this.MKDSKCL = kclData;
            InitializeComponent();
        }

        private void MKDSKCLViewer_Load(object sender, EventArgs e)
        {
            FillVertexListView();
            FillNormalListView();
            FillPlaneListView();
        }

        private void FillVertexListView()
        {
            listView_Vertex.Items.Clear();
            if (MKDSKCL?.Vertices == null) return;

            for (int i = 0; i < MKDSKCL.Vertices.Length; i++)
            {
                LibEveryFileExplorer.Collections.Vector3 v = MKDSKCL.Vertices[i];
                ListViewItem item = new ListViewItem(i.ToString());
                item.SubItems.Add(v.X.ToString("F4"));
                item.SubItems.Add(v.Y.ToString("F4"));
                item.SubItems.Add(v.Z.ToString("F4"));
                listView_Vertex.Items.Add(item);
            }
        }

        private void FillNormalListView()
        {
            listView_Normals.Items.Clear();
            if (MKDSKCL?.Normals == null) return;

            for (int i = 0; i < MKDSKCL.Normals.Length; i++)
            {
                LibEveryFileExplorer.Collections.Vector3 n = MKDSKCL.Normals[i];
                ListViewItem item = new ListViewItem(i.ToString());
                item.SubItems.Add(n.X.ToString("F4"));
                item.SubItems.Add(n.Y.ToString("F4"));
                item.SubItems.Add(n.Z.ToString("F4"));
                listView_Normals.Items.Add(item);
            }
        }

        private void FillPlaneListView()
        {
            listView_Planar.Items.Clear();
            if (MKDSKCL?.Planes == null) return;
            for (int i = 0; i < MKDSKCL.Planes.Length; i++)
            {
                MKDS.KCL.KCLPlane plane = MKDSKCL.Planes[i];
                Color planeColor = MKDS.KCL.GetColor(plane.CollisionType);
                ListViewItem item = new ListViewItem(i.ToString());
                item.SubItems.Add(plane.Length.ToString("F4"));
                item.SubItems.Add(plane.VertexIndex.ToString());
                item.SubItems.Add(plane.NormalIndex.ToString());
                item.SubItems.Add(plane.NormalAIndex.ToString());
                item.SubItems.Add(plane.NormalBIndex.ToString());
                item.SubItems.Add(plane.NormalCIndex.ToString());
                item.SubItems.Add(plane.CollisionType.ToString("X4"));
                item.ForeColor = planeColor;
                listView_Planar.Items.Add(item);
            }
        }
    }
}
