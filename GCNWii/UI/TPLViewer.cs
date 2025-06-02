using System;
using System.Drawing.Imaging;
using System.Windows.Forms;
using GCNWii.NintendoWare.LYT;

namespace GCNWii.UI
{
	public partial class TPLViewer : Form
	{
		TPL Image;
		public TPLViewer(TPL Image)
		{
			this.Image = Image;
			InitializeComponent();
		}

		private void TPLViewer_Load(object sender, EventArgs e)
		{
            for (int i = 0; i < Image.Header.NrTextures; i++)
            {
                toolStripComboBox1.Items.Add($"Texture {i}");
            }
            toolStripComboBox1.SelectedIndex = 0;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("TPL import has not been implemented yet.");
            return;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = toolStripComboBox1.SelectedIndex + ".png";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK
                && saveFileDialog1.FileName.Length > 0)
            {
                pictureBox1.Image.Save(saveFileDialog1.FileName, ImageFormat.Png);
            }
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBox1.Image = Image.Textures[toolStripComboBox1.SelectedIndex].ToBitmap();
        }
    }
}
