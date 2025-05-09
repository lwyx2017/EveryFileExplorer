using System;
using System.IO;
using System.Windows.Forms;

namespace _3DS.UI
{
    public partial class CBMDViewer : Form
    {
        CBMD _cbmd;

        public CBMDViewer(CBMD cBMD)
        {
            InitializeComponent();
            _cbmd = cBMD;
        }

        private void button_Export_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = "banner.cgfx";
            saveFileDialog1.Filter = "CTR Graphics (*.cgfx)|*.cgfx";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK &&
                !string.IsNullOrWhiteSpace(saveFileDialog1.FileName))
            {
                try
                {
                    byte[] RawCGFX = _cbmd.GetRawCGFX();
                    File.WriteAllBytes(saveFileDialog1.FileName, RawCGFX);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Save failed: {ex.Message}", "Error");
                }
            }
        }

        private void button_Export_Decompressed_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = "banner_decompressed.cgfx";
            saveFileDialog1.Filter = "CTR Graphics (*.cgfx)|*.cgfx";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK &&
                !string.IsNullOrWhiteSpace(saveFileDialog1.FileName))
            {
                try
                {
                    byte[] decompressedData = _cbmd.GetDecompressedCGFX();
                    File.WriteAllBytes(saveFileDialog1.FileName, decompressedData);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Save failed: {ex.Message}", "Error");
                }
            }
        }

        private void button_cwav_export_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = "banner.bcwav";
            saveFileDialog1.Filter = "CTR Wave Audio (*.bcwav)|*.bcwav";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK &&
                !string.IsNullOrWhiteSpace(saveFileDialog1.FileName))
            {
                try
                {
                    byte[] CWAV = _cbmd.GetCWAV();
                    File.WriteAllBytes(saveFileDialog1.FileName, CWAV);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Save failed: {ex.Message}", "Error");
                }
            }
        }
    }
}
