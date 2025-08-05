using System;
using System.Windows.Forms;
using GCNWii.GPU;

namespace GCNWii.UI
{
    public partial class TPLGenDialog : Form
    {
        public Textures.ImageFormat SelectedTextureFormat { get; private set; }
        public Textures.PaletteFormat SelectedPaletteFormat { get; private set; }

        public TPLGenDialog()
        {
            InitializeComponent();
            comboBoxTextureFormat.SelectedIndex = 6;
            comboBoxPaletteFormat.SelectedIndex = 2;
        }
        public int index { get; private set; }

        private void button1_Click(object sender, EventArgs e)
        {
            SelectedTextureFormat = (Textures.ImageFormat)Enum.Parse(typeof(Textures.ImageFormat),
            comboBoxTextureFormat.SelectedItem.ToString());
            SelectedPaletteFormat = (Textures.PaletteFormat)Enum.Parse(
                typeof(Textures.PaletteFormat),
                comboBoxPaletteFormat.SelectedItem.ToString());
            DialogResult = DialogResult.OK;
            Close();
        }

        private void TPLGenDialog_Load(object sender, EventArgs e)
        {
            comboBoxPaletteFormat.Enabled = false;
        }

        private void comboBoxTextureFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isCIFormat = comboBoxTextureFormat.SelectedItem.ToString().StartsWith("CI");
            comboBoxPaletteFormat.Enabled = isCIFormat;
        }
    }
}