using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CommonFiles;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.IO;
using NDS.SND;
using NDS.UI;

namespace NDS.NitroSystem.SND
{
    public class SWAV : FileFormat<SWAV.SWAVIdentifier>, IViewable, IConvertable, IWriteable, IFileCreatable
    {
        public SWAV() 
        {
            Header = new SWAVHeader();
            SWAVDataSection = new DataSection();
        }
        public SWAV(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new SWAVHeader(er);
                SWAVDataSection =new DataSection(er);
            }
            finally
            {
                er.Close();
            }
        }

        public Form GetDialog()
        {
            return new SWAVViewer(this);
        }

        public string GetConversionFileFilters()
        {
            return "Wave File (*.wav)|*.wav";
        }

        public bool Convert(int FilterIndex, String Path)
        {
            switch (FilterIndex)
            {
                case 0:
                    byte[] Data = ToWave().Write();
                    File.Create(Path).Close();
                    File.WriteAllBytes(Path, Data);
                    return true;
                default:
                    return false;
            }
        }

        public string GetSaveDefaultFileFilter()
        {
            return "Nitro Sound Wave (*.swav)|*.swav";
        }

        public byte[] Write()
        {
            MemoryStream m = new MemoryStream();
            EndianBinaryWriterEx er = new EndianBinaryWriterEx(m, Endianness.LittleEndian);
            uint sectionSize = (uint)(12 + SWAVDataSection.Data.Length);
            uint fileSize = 16 + 4 + 4 + sectionSize;
            Header.FileSize = fileSize;
            SWAVDataSection.SectionSize = sectionSize;
            Header.Write(er);
            SWAVDataSection.Write(er);
            byte[] result = m.ToArray();
            er.Close();
            return result;
        }

        public bool CreateFromFile()
        {
            OpenFileDialog f = new OpenFileDialog();
            f.Filter = WAV.Identifier.GetFileFilter();
            if (f.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(f.FileName)) return false;
            try
            {
                byte[] wavBytes = File.ReadAllBytes(f.FileName);
                WAV sourceWav = new WAV(wavBytes);
                Header = new SWAVHeader();
                SWAVDataSection = new DataSection();
                SWAVDataSection.Info = new SWAVInfo(new EndianBinaryReader(new MemoryStream(new byte[16]), Endianness.LittleEndian));
                uint sampleRate = sourceWav.Wave.FMT.SampleRate;
                ushort bits = sourceWav.Wave.FMT.BitsPerSample;
                byte[] rawPcm = sourceWav.Wave.DATA.Data;
                if (bits == 8)
                {
                    SWAVDataSection.Info.nWaveType = 0;
                    SWAVDataSection.Data = IOUtil.SignedPCM8ToUnsigned(rawPcm);
                }
                else if (bits == 16)
                {
                    SWAVDataSection.Info.nWaveType = 1;
                    SWAVDataSection.Data = rawPcm;
                }
                SWAVDataSection.Info.nSampleRate = (ushort)sampleRate;
                SWAVDataSection.Info.bLoop = 0;
                SWAVDataSection.Info.nLoopOffset = 0;
                SWAVDataSection.Info.nNonLoopLen = (uint)((uint)rawPcm.Length / (bits / 8));
                SWAVDataSection.Info.nTime = 0;
                int infoStructSize = 12;
                SWAVDataSection.SectionSize = (uint)(infoStructSize + SWAVDataSection.Data.Length + 4);
                Header.FileSize = 16 + SWAVDataSection.SectionSize;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public SWAVHeader Header;
        public class SWAVHeader
        {
            public SWAVHeader() 
            {
                Signature = "SWAV";
                Endianness = 0xFEFF;
                HeaderSize = 0x10;
                Version = 256;
                NrBlocks = 2;
            }
            public SWAVHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "SWAV") throw new SignatureNotCorrectException(Signature, "SWAV", er.BaseStream.Position - 4);
                Endianness = er.ReadUInt16();
                Version = er.ReadUInt16();
                FileSize = er.ReadUInt32();
                HeaderSize = er.ReadUInt16();
                NrBlocks = er.ReadUInt16();
            }
            public void Write(EndianBinaryWriterEx er)
            {
                er.Write(Signature, Encoding.ASCII, false);
                er.Write(Endianness);
                er.Write(Version);
                er.Write(FileSize);
                er.Write(HeaderSize);
                er.Write(NrBlocks);
            }
            public string Signature;
            public UInt16 Endianness;
            public UInt16 Version;
            public UInt32 FileSize;
            public UInt16 HeaderSize;
            public UInt16 NrBlocks;
        }

        public DataSection SWAVDataSection;

        public class DataSection
        {
            public DataSection()
            {
                Signature = "DATA";

            }
            public DataSection(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "DATA") throw new SignatureNotCorrectException(Signature, "DATA", er.BaseStream.Position);
                SectionSize = er.ReadUInt32();
                long position = er.BaseStream.Position;
                Info = new SWAVInfo(er);
                if (Info.nWaveType == 2)
                {
                    er.BaseStream.Position = position + 12;
                    Data = er.ReadBytes((int)(Info.nLoopOffset * 4 + Info.nNonLoopLen * 4 - 4));
                }
                else
                {
                    Data = er.ReadBytes((int)(er.BaseStream.Length - er.BaseStream.Position));
                }
            }
            public void Write(EndianBinaryWriterEx er)
            {
                er.Write(Signature, Encoding.ASCII, false);
                er.Write(SectionSize);
                er.Write(Info.nWaveType);
                er.Write(Info.bLoop);
                er.Write(Info.nSampleRate);
                er.Write(Info.nTime);
                er.Write(Info.nLoopOffset);
                er.Write(Info.nNonLoopLen);
                er.Write(Data);
            }
            public string Signature;
            public uint SectionSize;
            public SWAVInfo Info;
            public byte[] Data;
        }

        public class SWAVInfo
        {
            public SWAVInfo(EndianBinaryReader er)
            {
                nWaveType = er.ReadByte();
                bLoop = er.ReadByte();
                nSampleRate = er.ReadUInt16();
                nTime = er.ReadUInt16();
                nLoopOffset = er.ReadUInt16();
                nNonLoopLen = er.ReadUInt32();
            }
            public byte nWaveType;
            public byte bLoop;
            public ushort nSampleRate;
            public ushort nTime;
            public ushort nLoopOffset;
            public uint nNonLoopLen;
        }

        public unsafe WAV ToWave()
        {
            if (SWAVDataSection.Info.nWaveType == 0)
            {
                return new WAV(IOUtil.SignedPCM8ToUnsigned(SWAVDataSection.Data), SWAVDataSection.Info.nSampleRate, 8, 1);
            }
            if (SWAVDataSection.Info.nWaveType == 1)
            {
                return new WAV(SWAVDataSection.Data, SWAVDataSection.Info.nSampleRate, 16, 1);
            }
            if (SWAVDataSection.Info.nWaveType == 2)
            {
                IMAADPCMDecoder decoder = new IMAADPCMDecoder();
                short[] pcm16 = decoder.GetWaveData(SWAVDataSection.Data, 0, SWAVDataSection.Data.Length);
                byte[] waveBytes = new byte[pcm16.Length * 2];
                Buffer.BlockCopy(pcm16, 0, waveBytes, 0, waveBytes.Length);

                return new WAV(waveBytes, SWAVDataSection.Info.nSampleRate, 16, 1);
            }
            return null;
        }
        public class SWAVIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Audio;
            }

            public override string GetFileDescription()
            {
                return "Nitro Sound Wave (SWAV)";
            }

            public override string GetFileFilter()
            {
                return "Nitro Sound Wave (*.swav)|*.swav";
            }

            public override Bitmap GetIcon()
            {
                return Resource.speaker2;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'S' && File.Data[1] == 'W' && File.Data[2] == 'A' && File.Data[3] == 'V') return FormatMatch.Content;
                return FormatMatch.No;
            }
        }
    }
}