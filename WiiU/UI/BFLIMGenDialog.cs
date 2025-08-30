using System;
using System.Windows.Forms;

namespace WiiU.UI
{
    public partial class BFLIMGenDialog : Form
    {
        public BFLIMGenDialog()
        {
            InitializeComponent();
            radioButton3DS.Checked = true;
            radioButtonWiiU.Enabled = false;
            Format3DS.SelectedIndex = 0;
            Flag3DS.SelectedIndex = 0;
            FormatWiiU.SelectedIndex = 0;
            WiiUTileMode.SelectedIndex = 4;
            UpdateControlStates();
        }

        public bool Is3DS => radioButton3DS.Checked;
        public int FormatIndex => Is3DS ? Format3DS.SelectedIndex : FormatWiiU.SelectedIndex;
        public int Flag => Is3DS ? Flag3DS.SelectedIndex : 0;
        public int TileMode => Is3DS ? 0 : WiiUTileMode.SelectedIndex;

        public int index { get; private set; }
        public int flag => Is3DS && int.TryParse(Flag3DS.Text, out int flag) ? flag : 0;

        private void UpdateControlStates()
        {
            Format3DS.Enabled = radioButton3DS.Checked;
            Flag3DS.Enabled = radioButton3DS.Checked;
            FormatWiiU.Enabled = radioButtonWiiU.Enabled && radioButtonWiiU.Checked;
            WiiUTileMode.Enabled = radioButtonWiiU.Enabled && radioButtonWiiU.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            index = Format3DS.SelectedIndex;
        }

        private void radioButton3DS_CheckedChanged(object sender, EventArgs e)
        {
            UpdateControlStates();
        }

        private void radioButtonWiiU_CheckedChanged(object sender, EventArgs e)
        {
            UpdateControlStates();
        }
    }
}