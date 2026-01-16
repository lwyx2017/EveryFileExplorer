using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using NDS.GPU;
using NDS.NitroSystem.G2D;

namespace NDS.UI
{
    public partial class NCLREditor : UserControl
    {
        NCLR nclrFile;
        private List<Color> colors = new List<Color>();
        private int selectedIndex = -1;
        private bool use16ColorStyle;

        public NCLREditor(NCLR nclrFile)
        {
            this.nclrFile = nclrFile;
            InitializeComponent();
            this.colors = new List<Color>(nclrFile.ToColorArray());
            AutoScroll = true;
            UpdateScrollSize();
        }

        public delegate void SelectedColorChanged(Color c);

        [Browsable(false)]
        public Color[] Colors
        {
            get { return colors.ToArray(); }
            set
            {
                colors.Clear();
                if (value != null)
                    colors.AddRange(value);
                nclrFile.Palettedata.Data = Textures.ToXBGR1555(colors.ToArray());
                UpdateScrollSize();
                Invalidate();
            }
        }

        public bool Use16ColorStyle
        {
            get { return use16ColorStyle; }
            set
            {
                if (use16ColorStyle != value)
                {
                    use16ColorStyle = value;
                    UpdateScrollSize();
                    Invalidate();
                }
            }
        }

        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                if (value >= 0 && value < colors.Count)
                {
                    selectedIndex = value;
                    Invalidate();
                    OnSelectedColorChanged?.Invoke(SelectedColor);
                    ScrollToSelected();
                }
            }
        }

        public Color SelectedColor
        {
            get
            {
                if (selectedIndex >= 0 && selectedIndex < colors.Count)
                    return colors[selectedIndex];
                return Color.Black;
            }
            set
            {
                if (selectedIndex >= 0 && selectedIndex < colors.Count)
                {
                    colors[selectedIndex] = value;
                    nclrFile.Palettedata.Data = Textures.ToXBGR1555(colors.ToArray());
                    Invalidate();
                }
            }
        }

        public event SelectedColorChanged OnSelectedColorChanged;

        public void UpdateScrollSize()
        {
            if (colors.Count == 0)
            {
                AutoScrollMinSize = Size.Empty;
                return;
            }

            int columns = Use16ColorStyle ? 16 : Math.Max((ClientSize.Width - 4) / 20, 1);
            int rows = (colors.Count + columns - 1) / columns;
            AutoScrollMinSize = new Size(columns * 20 + 8, rows * 20 + 8);
        }

        private void ScrollToSelected()
        {
            if (selectedIndex < 0 || colors.Count == 0) return;

            int columns = Use16ColorStyle ? 16 : Math.Max((ClientSize.Width - 4) / 20, 1);
            int x = selectedIndex % columns;
            int y = selectedIndex / columns;

            int blockX = 4 + x * 20;
            int blockY = 4 + y * 20;
            Rectangle blockRect = new Rectangle(blockX, blockY, 20, 20);

            int visibleContentX = -AutoScrollPosition.X;
            int visibleContentY = -AutoScrollPosition.Y;
            Rectangle visibleContentRect = new Rectangle(visibleContentX, visibleContentY, ClientSize.Width, ClientSize.Height);

            if (visibleContentRect.Contains(blockRect))
                return;

            int targetContentX = visibleContentX;
            int targetContentY = visibleContentY;

            if (blockRect.Right > visibleContentRect.Right)
            {
                targetContentX = blockRect.Right - ClientSize.Width;
            }
            else if (blockRect.Left < visibleContentRect.Left)
            {
                targetContentX = blockRect.Left;
            }

            if (blockRect.Bottom > visibleContentRect.Bottom)
            {
                targetContentY = blockRect.Bottom - ClientSize.Height;
            }
            else if (blockRect.Top < visibleContentRect.Top)
            {
                targetContentY = blockRect.Top;
            }

            int maxScrollX = Math.Max(0, AutoScrollMinSize.Width - ClientSize.Width);
            int maxScrollY = Math.Max(0, AutoScrollMinSize.Height - ClientSize.Height);
            targetContentX = Math.Max(0, Math.Min(targetContentX, maxScrollX));
            targetContentY = Math.Max(0, Math.Min(targetContentY, maxScrollY));
            AutoScrollPosition = new Point(-targetContentX, -targetContentY);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.PixelOffsetMode = PixelOffsetMode.None;
            e.Graphics.SmoothingMode = SmoothingMode.None;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

            int columns = Use16ColorStyle ? 16 : Math.Max((ClientSize.Width - 4) / 20, 1);
            int x = 0, y = 0;

            for (int i = 0; i < colors.Count; i++)
            {
                int drawX = 4 + x * 20 + AutoScrollPosition.X;
                int drawY = 4 + y * 20 + AutoScrollPosition.Y;
                Rectangle colorRect = new Rectangle(drawX, drawY, 16, 16);

                using (SolidBrush brush = new SolidBrush(colors[i]))
                    e.Graphics.FillRectangle(brush, colorRect);

                using (Pen pen = new Pen(Color.Black, 1f))
                    e.Graphics.DrawRectangle(pen, colorRect);

                if (selectedIndex == i)
                    DrawSelectionIndicator(e.Graphics, x, y);

                if (x == columns - 1)
                {
                    x = 0;
                    y++;
                }
                else
                    x++;
            }
        }

        private void DrawSelectionIndicator(Graphics g, int x, int y)
        {
            using (Pen pen = new Pen(Color.Black, 1f))
            {
                int baseX = 4 + x * 20 + AutoScrollPosition.X;
                int baseY = 4 + y * 20 + AutoScrollPosition.Y;

                g.DrawLine(pen, baseX - 2, baseY - 2, baseX, baseY - 2);
                g.DrawLine(pen, baseX - 2, baseY - 2, baseX - 2, baseY);
                g.DrawLine(pen, baseX + 16, baseY - 2, baseX + 18, baseY - 2);
                g.DrawLine(pen, baseX + 18, baseY - 2, baseX + 18, baseY);
                g.DrawLine(pen, baseX - 2, baseY + 16, baseX, baseY + 16);
                g.DrawLine(pen, baseX - 2, baseY + 16, baseX - 2, baseY + 18);
                g.DrawLine(pen, baseX + 16, baseY + 18, baseX + 18, baseY + 18);
                g.DrawLine(pen, baseX + 18, baseY + 16, baseX + 18, baseY + 18);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            int columns = Use16ColorStyle ? 16 : Math.Max((ClientSize.Width - 4) / 20, 1);
            int contentX = e.X - AutoScrollPosition.X;
            int contentY = e.Y - AutoScrollPosition.Y;

            int x = (contentX - 4) / 20;
            int y = (contentY - 4) / 20;

            if (x >= 0 && y >= 0)
            {
                int index = x + y * columns;
                if (index < colors.Count)
                    SelectedIndex = index;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateScrollSize();
            Invalidate();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Right:
                    if (SelectedIndex < colors.Count - 1)
                        SelectedIndex++;
                    return true;

                case Keys.Left:
                    if (SelectedIndex > 0)
                        SelectedIndex--;
                    return true;

                case Keys.Up:
                    int columns = Use16ColorStyle ? 16 : Math.Max((ClientSize.Width - 4) / 20, 1);
                    if (SelectedIndex >= columns)
                        SelectedIndex -= columns;
                    return true;

                case Keys.Down:
                    columns = Use16ColorStyle ? 16 : Math.Max((ClientSize.Width - 4) / 20, 1);
                    if (SelectedIndex < colors.Count - columns)
                        SelectedIndex += columns;
                    return true;

                case Keys.C | Keys.Control:
                    CopyColor();
                    return true;

                case Keys.V | Keys.Control:
                    PasteColor();
                    return true;

                case Keys.Delete:
                    RemoveColor();
                    return true;

                case Keys.Insert:
                case Keys.A | Keys.Control:
                    AddColor();
                    return true;

                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            this.Focus();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (!this.Focused)
                this.Focus();
        }

        public class ColorPanel : Panel
        {
            public Color PanelColor = Color.Black;
        }

        public void AddColor(Color colorToAdd = default)
        {
            if (colorToAdd == default)
                colorToAdd = Color.Black;
            colors.Add(colorToAdd);
            nclrFile.Palettedata.Data = Textures.ToXBGR1555(colors.ToArray());
            UpdateScrollSize();
            Invalidate();
            SelectedIndex = colors.Count - 1;
        }

        public void RemoveColor()
        {
            if (selectedIndex < 0 || selectedIndex >= colors.Count)
            {
                MessageBox.Show("Please select a color block to remove first!", "Information",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show(
                "The selected color will be deleted. Continue?",
                "Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
            {
                colors.RemoveAt(selectedIndex);
                nclrFile.Palettedata.Data = Textures.ToXBGR1555(colors.ToArray());
                UpdateScrollSize();
                if (selectedIndex >= colors.Count)
                    selectedIndex = colors.Count - 1;
                Invalidate();
                OnSelectedColorChanged?.Invoke(SelectedColor);
            }
        }

        private Color? copiedColor = null;

        public void CopyColor()
        {
            if (selectedIndex < 0 || selectedIndex >= colors.Count)
            {
                MessageBox.Show("Please select a color block to copy first!", "Information",
                               MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            copiedColor = colors[selectedIndex];
        }

        public void PasteColor()
        {
            if (copiedColor == null)
            {
                MessageBox.Show("Please copy a color block first!", "Information",
                               MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (selectedIndex < 0 || selectedIndex >= colors.Count)
            {
                MessageBox.Show("Please select a target color block to paste first!", "Information",
                               MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            colors[selectedIndex] = copiedColor.Value;
            nclrFile.Palettedata.Data = Textures.ToXBGR1555(colors.ToArray());
            Invalidate();
            OnSelectedColorChanged?.Invoke(SelectedColor);
        }
    }
}