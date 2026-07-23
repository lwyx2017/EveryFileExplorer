using NDS.NitroSystem.Particles;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace NDS.UI
{
    public partial class SPAViewer : Form
    {
        private SPA SPAFile;

        public SPAViewer(SPA SPAFile)
        {
            this.SPAFile = SPAFile;
            InitializeComponent();
        }

        private void SPAViewer_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < SPAFile.Header.NrParticleTextures; i++)
            {
                toolStripComboBox1.Items.Add($"Particle {i}");
            }
            toolStripComboBox1.SelectedIndex = 0;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("SPA import has not been implemented yet.");
            return;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            int texIndex = toolStripComboBox1.SelectedIndex;
            string baseName;
            if (!string.IsNullOrWhiteSpace(this.Text))
            {
                string pureFileName = Path.GetFileName(this.Text);
                baseName = Path.GetFileNameWithoutExtension(pureFileName);
            }
            else
            {
                baseName = "Particle";
            }
            saveFileDialog1.FileName = $"{baseName}_{texIndex}.png";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK
                && saveFileDialog1.FileName.Length > 0)
            {
                pictureBox1.Image.Save(saveFileDialog1.FileName, ImageFormat.Png);
            }
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBox1.Image = SPAFile.ParticleTextures[toolStripComboBox1.SelectedIndex].ToBitmap();
        }
    }
}