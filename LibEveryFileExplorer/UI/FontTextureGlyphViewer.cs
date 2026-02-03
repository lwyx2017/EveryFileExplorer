using System;
using System.Drawing;
using System.Windows.Forms;

namespace LibEveryFileExplorer.UI
{
    public partial class FontTextureGlyphViewer : UserControl
    {
        private Bitmap[] TGLPImages;
        public FontTextureGlyphViewer()
        {
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        }

        public Bitmap[] Images
        {
            get => TGLPImages;
            set
            {
                TGLPImages = value;
                InitializeImageList();
            }
        }

        private void InitializeImageList()
        {
            toolStripComboBox1.Items.Clear();
            if (TGLPImages != null && TGLPImages.Length > 0)
            {
                for (int i = 0; i < TGLPImages.Length; i++)
                {
                    toolStripComboBox1.Items.Add($"Image {i}");
                }
                toolStripComboBox1.SelectedIndex = 0;
            }
            RefreshDisplay();
        }

        public void RefreshDisplay()
        {
            if (TGLPImages == null || toolStripComboBox1.SelectedIndex < 0
                || toolStripComboBox1.SelectedIndex >= TGLPImages.Length)
            {
                pictureBox1.Image = null;
                return;
            }
            var image = TGLPImages[toolStripComboBox1.SelectedIndex];
            pictureBox1.Image = image;
            pictureBox1.Size = new Size(
                System.Math.Min(image.Width, this.ClientSize.Width - 20),
                System.Math.Min(image.Height, this.ClientSize.Height - 40)
            );
            this.Invalidate(true);
            pictureBox1.Invalidate();
            this.Update();
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshDisplay();
        }
    }
}