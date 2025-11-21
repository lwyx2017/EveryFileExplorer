using System.Drawing;
using System.Windows.Forms;
using NDS.GPU;
using NDS.NitroSystem.G2D;

namespace NDS.UI
{
    public partial class NCLRViewer : Form
    {
        NCLR nclrFile;
        NCLREditor nclrEditor;
        public NCLRViewer(NCLR nclrFile)
        {
            InitializeComponent();
            this.nclrFile = nclrFile;
            nclrEditor = new NCLREditor(nclrFile);
            nclrEditor.Dock = DockStyle.Fill;
            nclrEditor.BorderStyle = BorderStyle.FixedSingle;
            bool is16Color = nclrFile.Palettedata.Format == Textures.ImageFormat.PLTT16;
            nclrEditor.Use16ColorStyle = is16Color;
            Color[] colors = nclrFile.ToColorArray();
            nclrEditor.Colors = colors;
            nclrEditor.OnSelectedColorChanged += NclrEditor_OnSelectedColorChanged;
            splitContainer1.Panel1.Controls.Add(nclrEditor);
        }

        private class HSLColor
        {
            private const double scale = 240.0;
            private double hue = 1.0;
            private double saturation = 1.0;
            private double luminosity = 1.0;

            public double Hue
            {
                get => hue * 240.0;
                set => hue = CheckRange(value / 240.0);
            }

            public double Saturation
            {
                get => saturation * 240.0;
                set => saturation = CheckRange(value / 240.0);
            }

            public double Luminosity
            {
                get => luminosity * 240.0;
                set => luminosity = CheckRange(value / 240.0);
            }

            private double CheckRange(double value)
            {
                if (value < 0.0) value = 0.0;
                else if (value > 1.0) value = 1.0;
                return value;
            }

            public static implicit operator Color(HSLColor hslColor)
            {
                double r = 0.0, g = 0.0, b = 0.0;
                if (hslColor.luminosity != 0.0)
                {
                    if (hslColor.saturation == 0.0)
                    {
                        r = g = b = hslColor.luminosity;
                    }
                    else
                    {
                        double chroma = CalculateChroma(hslColor);
                        double minLuminanceComponent = 2.0 * hslColor.luminosity - chroma;
                        r = CalculateColorComponent(minLuminanceComponent, chroma, hslColor.hue + 1.0 / 3.0);
                        g = CalculateColorComponent(minLuminanceComponent, chroma, hslColor.hue);
                        b = CalculateColorComponent(minLuminanceComponent, chroma, hslColor.hue - 1.0 / 3.0);
                    }
                }
                return Color.FromArgb((int)(255.0 * r), (int)(255.0 * g), (int)(255.0 * b));
            }

            private static double CalculateColorComponent(double minLuminanceComponent, double chroma, double hueComponent)
            {
                hueComponent = NormalizeHueComponent(hueComponent);
                if (hueComponent < 1.0 / 6.0)
                    return minLuminanceComponent + (chroma - minLuminanceComponent) * 6.0 * hueComponent;
                if (hueComponent < 0.5)
                    return chroma;
                if (hueComponent < 2.0 / 3.0)
                    return minLuminanceComponent + (chroma - minLuminanceComponent) * (2.0 / 3.0 - hueComponent) * 6.0;
                return minLuminanceComponent;
            }

            private static double NormalizeHueComponent(double hueComponent)
            {
                if (hueComponent < 0.0) hueComponent += 1.0;
                else if (hueComponent > 1.0) hueComponent -= 1.0;
                return hueComponent;
            }

            private static double CalculateChroma(HSLColor hslColor)
            {
                if (hslColor.luminosity < 0.5)
                    return hslColor.luminosity * (1.0 + hslColor.saturation);
                return hslColor.luminosity + hslColor.saturation - hslColor.luminosity * hslColor.saturation;
            }

            public static implicit operator HSLColor(Color color)
            {
                HSLColor hslColor = new HSLColor();
                hslColor.hue = (double)color.GetHue() / 360.0;
                hslColor.luminosity = color.GetBrightness();
                hslColor.saturation = color.GetSaturation();
                return hslColor;
            }

            public HSLColor() { }

            public HSLColor(Color color)
            {
                SetRGB(color.R, color.G, color.B);
            }

            public void SetRGB(int red, int green, int blue)
            {
                HSLColor hslColor = Color.FromArgb(red, green, blue);
                hue = hslColor.hue;
                saturation = hslColor.saturation;
                luminosity = hslColor.luminosity;
            }
        }

        private void ApplyHSLAdjustments()
        {
            float hueAdjust = (float)TrackBarHue.Value / 240f;
            float saturationAdjust = (float)TrackBarSaturation.Value / 240f;
            float luminosityAdjust = (float)TrackBarLuminosity.Value / 240f;
            Color[] colors = nclrFile.ToColorArray();

            for (int i = 0; i < colors.Length; i++)
            {
                HSLColor hslColor = colors[i];
                hslColor.Hue += hslColor.Hue * (double)hueAdjust;
                hslColor.Saturation += hslColor.Saturation * (double)saturationAdjust;
                hslColor.Luminosity += hslColor.Luminosity * (double)luminosityAdjust;

                colors[i] = hslColor;
            }
            nclrEditor.Colors = colors;
        }

        private void NclrEditor_OnSelectedColorChanged(Color c)
        {
            numericUpDownRed.Enabled = true;
            numericUpDownGreen.Enabled = true;
            numericUpDownBlue.Enabled = true;
            trackBarRed.Enabled = true;
            trackBarGreen.Enabled = true;
            trackBarBlue.Enabled = true;
            numericUpDownRed.Value = c.R;
            numericUpDownGreen.Value = c.G;
            numericUpDownBlue.Value = c.B;
            trackBarRed.Value = c.R;
            trackBarGreen.Value = c.G;
            trackBarBlue.Value = c.B;
            panelColor.BackColor = c;
        }

        private void UpdateSelectedColorFromControls()
        {
            if (nclrEditor.SelectedIndex >= 0)
            {
                Color newColor = Color.FromArgb(
                    (int)numericUpDownRed.Value,
                    (int)numericUpDownGreen.Value,
                    (int)numericUpDownBlue.Value
                );

                nclrEditor.SelectedColor = newColor;
                panelColor.BackColor = newColor;
            }
        }

        private void toolStripButton_add_Click(object sender, System.EventArgs e)
        {
            nclrEditor.AddColor();
        }

        private void toolStripButton_remove_Click(object sender, System.EventArgs e)
        {
            nclrEditor.RemoveColor();
        }

        private void toolStripButton_copy_Click(object sender, System.EventArgs e)
        {
            nclrEditor.CopyColor();
        }

        private void toolStripButton_paste_Click(object sender, System.EventArgs e)
        {
            nclrEditor.PasteColor();
        }

        private void TrackBarHue_Scroll(object sender, System.EventArgs e)
        {
            ApplyHSLAdjustments();
        }

        private void TrackBarSaturation_Scroll(object sender, System.EventArgs e)
        {
            ApplyHSLAdjustments();
        }

        private void TrackBarLuminosity_Scroll(object sender, System.EventArgs e)
        {
            ApplyHSLAdjustments();
        }

        private void numericUpDownRed_ValueChanged(object sender, System.EventArgs e)
        {
            trackBarRed.Value = (int)numericUpDownRed.Value;
            UpdateSelectedColorFromControls();
        }

        private void trackBarRed_ValueChanged(object sender, System.EventArgs e)
        {
            numericUpDownRed.Value = trackBarRed.Value;
            UpdateSelectedColorFromControls();
        }

        private void numericUpDownGreen_ValueChanged(object sender, System.EventArgs e)
        {
            trackBarGreen.Value = (int)numericUpDownGreen.Value;
            UpdateSelectedColorFromControls();
        }

        private void trackBarGreen_ValueChanged(object sender, System.EventArgs e)
        {
            numericUpDownGreen.Value = trackBarGreen.Value;
            UpdateSelectedColorFromControls();
        }

        private void numericUpDownBlue_ValueChanged(object sender, System.EventArgs e)
        {
            trackBarBlue.Value = (int)numericUpDownBlue.Value;
            UpdateSelectedColorFromControls();
        }

        private void trackBarBlue_ValueChanged(object sender, System.EventArgs e)
        {
            numericUpDownBlue.Value = trackBarBlue.Value;
            UpdateSelectedColorFromControls();
        }
    }
}