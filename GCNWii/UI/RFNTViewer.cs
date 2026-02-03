using GCNWii.NintendoWare.LYT;
using LibEveryFileExplorer.GFX;
using LibEveryFileExplorer.UI;
using System;
using System.Windows.Forms;

namespace GCNWii.UI
{
    public partial class RFNTViewer : Form
    {
        RFNT BRFNTFont;
        BitmapFont f;
        private FontPreviewer FontPrev;
        private FontTextureGlyphViewer FontTGV;

        public RFNTViewer(RFNT BRFNTFont)
        {
            this.BRFNTFont = BRFNTFont;
            InitializeComponent();
            FontPrev = new FontPreviewer();
            FontTGV = new FontTextureGlyphViewer();
            FontPrev.Dock = DockStyle.Fill;
            FontTGV.Dock = DockStyle.Fill;
            tabPage1.Controls.Add(FontPrev);
            tabPage2.Controls.Add(FontTGV);
        }

        private void RFNTViewer_Load(object sender, EventArgs e)
        {
            f = BRFNTFont.GetBitmapFont();
            InitUserControlData();
        }

        private void InitUserControlData()
        {
            FontPrev.Font = f;
            if (BRFNTFont?.TGLP?.Images != null)
            {
                FontTGV.Images = BRFNTFont.TGLP.Images;
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage2)
            {
                tabPage2.Invalidate(true);
                FontTGV.RefreshDisplay();
                tabPage2.Update();
            }
            else if (tabControl1.SelectedTab == tabPage1)
            {
                tabPage1.Invalidate(true);
                tabPage1.Update();
            }
        }
    }
}