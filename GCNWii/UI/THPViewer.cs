using GCNWii.JSystem;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GCNWii.UI
{
    public partial class THPViewer : Form
    {
        THP ThpFile;
        bool audio = false;
        int frame = 0;
        bool isPlaying = true;
        Stopwatch frameTimer = new Stopwatch();
        Stopwatch audioSyncTimer = new Stopwatch();
        long totalAudioBytesPlayed = 0;
        NAudio.Wave.BufferedWaveProvider bb;
        NAudio.Wave.WaveOut ww;

        public THPViewer(THP Thp)
        {
            ThpFile = Thp;
            InitializeComponent();
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            double targetFrameTime = 1000.0 / ThpFile.Header.FPS;
            while (!e.Cancel && isPlaying)
            {
                frameTimer.Restart();
                this.Invoke((MethodInvoker)delegate
                {
                    if (frame < ThpFile.Header.NrFrames)
                    {
                        THP.THPFrame f = ThpFile.GetFrame(frame);
                        pictureBox1.Image = f.ToBitmap();
                        if (audio && frame < ThpFile.Header.NrFrames)
                        {
                            byte[] audioData;
                            f.ToPCM16(out audioData);
                            if (audioData != null && bb != null)
                            {
                                double audioTimeForThisFrame = 1000.0 / ThpFile.Header.FPS;
                                long expectedAudioBytes = (long)(bb.WaveFormat.AverageBytesPerSecond *
                                    (audioTimeForThisFrame / 1000.0));
                                if (audioData.Length > expectedAudioBytes)
                                {
                                    byte[] trimmedData = new byte[expectedAudioBytes];
                                    Array.Copy(audioData, trimmedData, expectedAudioBytes);
                                    bb.AddSamples(trimmedData, 0, trimmedData.Length);
                                }
                                else
                                {
                                    bb.AddSamples(audioData, 0, audioData.Length);
                                }
                                totalAudioBytesPlayed += audioData.Length;
                            }
                        }
                        frame++;
                        if (frame >= ThpFile.Header.NrFrames)
                        {
                            isPlaying = false;
                            frame = (int)(ThpFile.Header.NrFrames - 1);
                            if (audio && ww != null)
                            {
                                ww.Stop();
                            }
                        }
                    }
                });
                frameTimer.Stop();
                long elapsedMs = frameTimer.ElapsedMilliseconds;
                if (elapsedMs < targetFrameTime)
                {
                    Thread.Sleep((int)(targetFrameTime - elapsedMs));
                }
            }
        }

        private void THPViewer_Load(object sender, EventArgs e)
        {
            pictureBox1.Image = ThpFile.GetFrame(0).ToBitmap();
            Width = (int)((THP.THPComponents.THPVideoInfo)ThpFile.Components.THPInfos[0]).Width + 20;
            Height = (int)((THP.THPComponents.THPVideoInfo)ThpFile.Components.THPInfos[0]).Height + 29;
            audio = ThpFile.Components.THPInfos[1] != null;

            if (audio)
            {
                var audioInfo = (THP.THPComponents.THPAudioInfo)ThpFile.Components.THPInfos[1];
                bb = new NAudio.Wave.BufferedWaveProvider(
                    new NAudio.Wave.WaveFormat(
                        (int)audioInfo.Frequentie,
                        (int)audioInfo.NrChannels));
                bb.BufferLength = (int)(audioInfo.Frequentie * audioInfo.NrChannels * 2 * (1.0 / ThpFile.Header.FPS) * 10);
                bb.DiscardOnBufferOverflow = true;
                ww = new NAudio.Wave.WaveOut();
                ww.Init(bb);
                ww.Play();
                audioSyncTimer.Start();
            }

            frameTimer.Start();
            backgroundWorker1.RunWorkerAsync();
        }

        private void THPViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            isPlaying = false;
            backgroundWorker1.CancelAsync();
            if (audio && ww != null)
            {
                ww.Stop();
                ww.Dispose();
                bb = null;
            }
            if (ThpFile != null)
            {
                ThpFile.Close();
            }
        }
    }
}