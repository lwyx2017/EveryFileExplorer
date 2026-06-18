using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.IO;
using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace NDS.NitroSystem.SND
{
    public class SSAR : FileFormat<SSAR.SSARIdentifier>
    {
        public SSAR(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new SSARHeader(er);
                SSARDATA = new DATA(er);
            }
            finally
            {
                er.Close();
            }
        }

        public SSARHeader Header;
        public class SSARHeader
        {
            public SSARHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "SSAR") throw new SignatureNotCorrectException(Signature, "SSAR", er.BaseStream.Position - 4);
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

        public DATA SSARDATA;
        public class DATA
        {
            public DATA(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "DATA") throw new SignatureNotCorrectException(Signature, "DATA", er.BaseStream.Position);
                SectionSize = er.ReadUInt32();
                DataOffset = er.ReadUInt32();
                NrRecord = er.ReadUInt32();
                Records = new Record[NrRecord];
                for (int i = 0; i < NrRecord; i++)
                {
                    Records[i] = new Record(er);
                }
                long position = er.BaseStream.Position;
                er.BaseStream.Position = DataOffset;
                Data = er.ReadBytes((int)(er.BaseStream.Length - DataOffset));
                er.BaseStream.Position = position;
            }
            public String Signature;
            public UInt32 SectionSize;
            public uint DataOffset;
            public uint NrRecord;
            public Record[] Records;
            public byte[] Data;
        }

        public class Record
        {
            public Record(EndianBinaryReader er)
            {
                Offset = er.ReadUInt32();
                bnk = er.ReadUInt16();
                vol = er.ReadByte();
                cpr = er.ReadByte();
                ppr = er.ReadByte();
                ply = er.ReadByte();
                unknown2 = er.ReadBytes(2);
            }
            public uint Offset;
            public ushort bnk;
            public byte vol;
            public byte cpr;
            public byte ppr;
            public byte ply;
            public byte[] unknown2;
        }

        public class SSARIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Archives;
            }

            public override string GetFileDescription()
            {
                return "Nitro Sound Sequence Archive (SSAR)";
            }

            public override string GetFileFilter()
            {
                return "Nitro Sound Sequence Archive (*.ssar)|*.ssar";
            }

            public override Bitmap GetIcon()
            {
                return Resource.note_box;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'S' && File.Data[1] == 'S' && File.Data[2] == 'A' && File.Data[3] == 'R') return FormatMatch.Content;
                return FormatMatch.No;
            }

        }
    }
}