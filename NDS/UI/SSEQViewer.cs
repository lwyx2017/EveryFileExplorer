using NAudio.Midi;
using NDS.NitroSystem.SND;
using System;
using System.IO;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NDS.UI
{
    public partial class SSEQViewer : Form
    {
        SSEQ Seq;
        private MusicPlayer m;
        private byte[] dls;
        private bool first = true;
        private string midi = "";
        private string dlss = "";
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int SetWindowTheme(IntPtr hWnd, string appName, string partList);
        public SSEQViewer(SSEQ Seq)
        {
            this.Seq = Seq;
            m = new MusicPlayer();
            InitializeComponent();
            SetWindowTheme(treeView1.Handle, "explorer", null);
            imageList1.Images.Add(Resource.metronome);
            imageList1.Images.Add(Resource.music);
            imageList1.Images.Add(Resource.speaker_network);
            imageList1.Images.Add(Resource.clock_select);
            imageList1.Images.Add(Resource.speaker_new);
            imageList1.Images.Add(Resource.guitar_small);
            imageList1.Images.Add(Resource.arrow_return_180);
            imageList1.Images.Add(Resource.arrow_return_180_left);
            imageList1.Images.Add(Resource.speaker_volume);
            imageList1.Images.Add(Resource.question_white);
            treeView1.ImageList = imageList1;
            treeView1.ShowNodeToolTips = true;
            var dataSec = Seq.SSEQDataSection;
            foreach (var sourceNode in dataSec.GetSequenceTreeData())
            {
                treeView1.Nodes.Add((TreeNode)sourceNode.Clone());
            }
            if (treeView1.Nodes.Count > 0)
                treeView1.Nodes[0].Expand();
            var trackNames = dataSec.GetTrackNameList();
            foreach (var trackName in trackNames)
            {
                checkedListBox1.Items.Add(trackName, isChecked: true);
            }
            toolStripButton_wav.Enabled = false;
            toolStripButton_dls.Enabled = false;
        }

        private void toolStripButton_play_Click(object sender, EventArgs e)
        {
            MidiEventCollection midiEventCollection = Seq.SSEQDataSection.Midi;
            if (first)
            {
                midiEventCollection.PrepareForExport();
                midi = Path.GetTempFileName();
                MidiFile.Export(midi, midiEventCollection);
            }
            if (first && dls != null)
            {
                dlss = Path.GetTempFileName();
                File.WriteAllBytes(dlss, dls);
            }
            m.SetMidi(midi);
            if (first && dls != null)
            {
                m.SetDLS(dlss);
            }
            int loopStart = Seq.SSEQDataSection.GetLoopStart();
            int loopEnd = Seq.SSEQDataSection.GetLoopEnd();
            int loopCount = Seq.SSEQDataSection.GetNrLoop();
            if (loopStart != -1 && loopEnd != -1 && first)
            {
                int length = m.GetLength();
                int num = loopStart * 16;
                int num2 = loopEnd * 16;
                if (num <= length && num2 >= num)
                {
                    m.SetLoop(num, num2, loopCount);
                }
            }

            if (first)
                first = false;
            m.Play();
        }

        private void toolStripButton_stop_Click(object sender, EventArgs e)
        {
            m.Stop();
        }

        private void toolStripButton_wav_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton_dls_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton_midi_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show("Do you want to export all channels?", "Export", MessageBoxButtons.YesNo);
            MidiEventCollection targetMidi;

            if (res == DialogResult.No)
            {
                MidiEventCollection midiEventCollection = new MidiEventCollection(1, 48);
                int trackCount = Math.Min(Seq.SSEQDataSection.Midi.Tracks, checkedListBox1.Items.Count);
                for (int i = 0; i < trackCount; i++)
                {
                    if (checkedListBox1.GetItemChecked(i))
                    {
                        midiEventCollection.AddTrack(Seq.SSEQDataSection.Midi[i]);
                    }
                }
                targetMidi = midiEventCollection;
            }
            else
            {
                targetMidi = Seq.SSEQDataSection.Midi;
            }

            saveFileDialog1.Filter = "MIDI file (*.mid)|*.mid";
            saveFileDialog1.DefaultExt = "mid";
            saveFileDialog1.FileName = "";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(saveFileDialog1.FileName))
            {
                MidiFile.Export(saveFileDialog1.FileName, targetMidi);
            }
        }

        private void SSEQViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            m.Stop();
            m.Unload();
            if (File.Exists(midi))
            {
                File.Delete(midi);
            }
        }
    }
}