using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using LibEveryFileExplorer;
using RuneFactory.RFWii;

namespace RuneFactory.UI
{
    public partial class HXTBViewer : Form
    {
        HXTB Image;
        ImageList ImageL;

        public HXTBViewer(HXTB Image)
        {
            this.Image = Image;
            InitializeComponent();

            Win32Util.SetWindowTheme(treeView1.Handle, "explorer", null);
            ImageL = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = new Size(16, 16)
            };
            ImageL.Images.Add(Resource.images_stack);
            ImageL.Images.Add(Resource.image_sunset);
            treeView1.ImageList = ImageL;
            treeView1.AfterSelect += TreeView1_AfterSelect;
        }

        private void HXTBViewer_Load(object sender, EventArgs e)
        {
            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();

            if (Image.Textures != null && Image.Textures.Count > 0)
            {
                TreeNode root = new TreeNode("Textures", 0, 0);
                treeView1.Nodes.Add(root);

                foreach (var texture in Image.Textures)
                {
                    TreeNode node = new TreeNode(texture.Name, 1, 1) { Tag = texture };
                    root.Nodes.Add(node);
                }
                root.Expand();

                if (root.Nodes.Count > 0)
                {
                    treeView1.SelectedNode = root.Nodes[0];
                    ShowTexture((HXTB.HXTBTextureEntry)root.Nodes[0].Tag);
                }
            }
            treeView1.EndUpdate();
        }

        private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is HXTB.HXTBTextureEntry texture)
            {
                ShowTexture(texture);
            }
        }

        private void ShowTexture(HXTB.HXTBTextureEntry texture)
        {
            try
            {
                var prev = pictureBox1.Image;
                pictureBox1.Image = texture.ToBitmap();
                prev?.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Texture loading error:\n{ex.Message}");
                pictureBox1.Image = null;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            pictureBox1.Image?.Dispose();
            base.OnFormClosing(e);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("HXTB import has not been implemented yet.");
            return;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = treeView1.SelectedNode;
            if (selectedNode?.Tag is HXTB.HXTBTextureEntry texture)
            {
                string fileName = $"{texture.Name}.png";
                saveFileDialog1.FileName = fileName;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK &&
                    !string.IsNullOrWhiteSpace(saveFileDialog1.FileName))
                {
                    pictureBox1.Image.Save(saveFileDialog1.FileName, ImageFormat.Png);
                }

            }
        }
    }
}
