using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NDS.NitroSystem.SND;

namespace NDS.UI
{
	public partial class SWAVViewer : Form
	{
		SWAV Wave;

		public SWAVViewer(SWAV Wave)
		{
			this.Wave = Wave;
			InitializeComponent();
		}

		private void SWAVViewer_FormClosed(object sender, FormClosedEventArgs e)
		{
			wavePlayer1.Stop();
		}

		private void SWAVViewer_Load(object sender, EventArgs e)
		{
			//Not in the constructor, because it will cause problems when creating a bSWAV
			wavePlayer1.SetWavFile(Wave.ToWave());
		}
	}
}
