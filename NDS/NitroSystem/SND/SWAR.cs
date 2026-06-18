using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.Files.SimpleFileSystem;
using LibEveryFileExplorer.IO;
using NDS.UI;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace NDS.NitroSystem.SND
{
    public class SWAR : FileFormat<SWAR.SWARIdentifier>,IViewable
    {
        public SWAR(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new SWARHeader(er);
                SWARData = new DATA(er);
            }
            finally
            {
                er.Close();
            }
        }

        public Form GetDialog()
        {
            return new SWARViewer(this);
        }

        public SWARHeader Header;
        public class SWARHeader
        {
            public SWARHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "SWAR") throw new SignatureNotCorrectException(Signature, "SWAR", er.BaseStream.Position - 4);
                Endianness = er.ReadUInt16();
                Version = er.ReadUInt16();
                FileSize = er.ReadUInt32();
                HeaderSize = er.ReadUInt16();
                NrBlocks = er.ReadUInt16();
            }
            public string Signature;
            public UInt16 Endianness;
            public UInt16 Version;
            public UInt32 FileSize;
            public UInt16 HeaderSize;
            public UInt16 NrBlocks;
        }

        public DATA SWARData;
        public class DATA
        {
            public DATA(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "DATA") throw new SignatureNotCorrectException(Signature, "DATA", er.BaseStream.Position);
                SectionSize = er.ReadUInt32();
                er.ReadBytes(32);
                nSample = er.ReadUInt32();
                Offsets = er.ReadUInt32s((int)nSample);
                SamplesInfo = new SWAV.SWAVInfo[nSample];
                SampleData = new byte[nSample][];
                for (int i = 0; i < nSample; i++)
                {
                    SamplesInfo[i] = new SWAV.SWAVInfo(er);
                    if (i < Offsets.Length - 1)
                    {
                        SampleData[i] = er.ReadBytes((int)(Offsets[i + 1] - Offsets[i] - 12));
                    }
                    else
                    {
                        SampleData[i] = er.ReadBytes((int)(er.BaseStream.Length - Offsets[i]) - 12);
                    }
                }
            }
            public String Signature;
            public UInt32 SectionSize;
            public uint nSample;
            public uint[] Offsets;
            public SWAV.SWAVInfo[] SamplesInfo;
            public byte[][] SampleData;
        }

        public SWAV this[int i] => ToWave(i);

        private SWAV ToWave(int idx)
        {
            return new SWAV(SWARData.SamplesInfo[idx], SWARData.SampleData[idx]);
        }

        public SFSDirectory ToFileSystem()
        {
            SFSDirectory root = new SFSDirectory("/", true);
            int fileId = 0;
            uint totalSamples = SWARData.nSample;

            for (int i = 0; i < totalSamples; i++)
            {
                string fileName = $"Swav {i}.swav";
                SWAV wave = this[i];
                byte[] swavBin = wave.Write();
                SFSFile sfsEntry = new SFSFile(fileId, fileName, root)
                {
                    Data = swavBin
                };
                root.Files.Add(sfsEntry);
                fileId++;
            }
            int dirCounter = 0;
            root.UpdateIDs(ref dirCounter, ref fileId);
            return root;
        }

        public class SWARIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Archives;
            }

            public override string GetFileDescription()
            {
                return "Nitro Sound Wave Archive (SWAR)";
            }

            public override string GetFileFilter()
            {
                return "Nitro Sound Wave Archive (*.swar)|*.swar";
            }

            public override Bitmap GetIcon()
            {
                return Resource.speaker2_box;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'S' && File.Data[1] == 'W' && File.Data[2] == 'A' && File.Data[3] == 'R') return FormatMatch.Content;
                return FormatMatch.No;
            }

        }
    }
}