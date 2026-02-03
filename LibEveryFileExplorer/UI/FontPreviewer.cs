using System;
using System.Windows.Forms;
using LibEveryFileExplorer.GFX;

namespace LibEveryFileExplorer.UI
{
    public partial class FontPreviewer : UserControl
    {
        private BitmapFont BMFont;

        public FontPreviewer()
        {
            InitializeComponent();
        }

        public new BitmapFont Font
        {
            get => BMFont;
            set
            {
                BMFont = value;
                UpdatePreview();
            }
        }

        private void UpdatePreview()
        {
            if (BMFont != null && !string.IsNullOrEmpty(textBox1.Text))
            {
                pictureBox1.Image = BMFont.PrintToBitmap(textBox1.Text, new BitmapFont.FontRenderSettings());
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }
    }
}