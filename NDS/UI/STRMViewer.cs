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
	public partial class STRMViewer : Form
	{
		STRM Stream;

		public STRMViewer(STRM Stream)
		{
			this.Stream = Stream;
			InitializeComponent();
		}

		private void STRMViewer_FormClosed(object sender, FormClosedEventArgs e)
		{
			wavePlayer1.Stop();
		}

		private void STRMViewer_Load(object sender, EventArgs e)
		{
			//Not in the constructor, because it will cause problems when creating a bSTRM
			wavePlayer1.SetWavFile(Stream.ToWave());
		}
	}
}
