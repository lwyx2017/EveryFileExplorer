using System;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using LibEveryFileExplorer.IO;
using _3DS.UI;
using System.Windows.Forms;
using CommonFiles;

namespace _3DS.NintendoWare.SND
{
    public class CWAV : FileFormat<CWAV.CWAVIdentifier>//, IViewable
    {
        public CWAV(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new CWAVHeader(er);
                er.BaseStream.Position = Header.Sections[0].Offset;
                Info = new INFO(er);
                er.BaseStream.Position = Header.Sections[1].Offset;
                this.Data = new DATA(er);
            }
            finally
            {
                er.Close();
            }
        }

        public Form GetDialog()
        {
            //return new CWAVViewer(this);
            return new Form();
        }

        public CWAVHeader Header;
        public class CWAVHeader
        {
            public CWAVHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "CWAV") throw new SignatureNotCorrectException(Signature, "CWAV", er.BaseStream.Position - 4);
                Endianness = er.ReadUInt16();
                HeaderSize = er.ReadUInt16();
                Version = er.ReadUInt32();
                FileSize = er.ReadUInt32();
                NrSections = er.ReadUInt32();
                Sections = new SectionInfo[NrSections];
                for (int i = 0; i < NrSections; i++) Sections[i] = new SectionInfo(er);
            }
            public String Signature;
            public UInt16 Endianness;
            public UInt16 HeaderSize;
            public UInt32 Version;
            public UInt32 FileSize;
            public UInt16 NumBlocks;

            public UInt32 NrSections;
            public SectionInfo[] Sections;
            public class SectionInfo
            {
                public SectionInfo(uint Id) { this.Id = Id; }
                public SectionInfo(EndianBinaryReader er)
                {
                    Id = er.ReadUInt32();
                    Offset = er.ReadUInt32();
                    Size = er.ReadUInt32();
                }
                public UInt32 Id;
                public UInt32 Offset;
                public UInt32 Size;
            }
        }

        public INFO Info;
        public class INFO
        {
            public INFO(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "INFO") throw new SignatureNotCorrectException(Signature, "INFO", er.BaseStream.Position - 4);
                InfoDataLength = er.ReadUInt32();
                Type = er.ReadUInt32();
                SampleRate = er.ReadUInt16();
                Volume = er.ReadUInt16();
            }

            public string Signature;
            public UInt32 InfoDataLength;
            public UInt32 Type;
            public UInt16 SampleRate;
            public UInt16 Volume;
        }

        public DATA Data;
        public class DATA
        {
            public DATA(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "DATA") throw new SignatureNotCorrectException(Signature, "DATA", er.BaseStream.Position - 4);
                SectionSize = er.ReadUInt32();
            }

            public String Signature;
            public UInt32 SectionSize;
            public byte[] Data;
        }

        public class CWAVIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Audio;
            }

            public override string GetFileDescription()
            {
                return "CTR Wave Audio (CWAV)";
            }

            public override string GetFileFilter()
            {
                return "CTR Wave Audio (*.bcwav)|*.bcwav";
            }

            public override Bitmap GetIcon()
            {
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'C' && File.Data[1] == 'W' && File.Data[2] == 'A' && File.Data[3] == 'V') return FormatMatch.Content;
                return FormatMatch.No;
            }

        }
    }
}