using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.IO.Serialization;

namespace GCNWii.NintendoWare.LYT
{
    public class RLYT:FileFormat<RLYT.RLYTIdentifier>//, IViewable, IWriteable
    {
        public RLYT(byte[] Data)
        {
            EndianBinaryReaderEx er = new EndianBinaryReaderEx(new MemoryStream(Data), Endianness.BigEndian);
            try
            {
                Header = new RLYTHeader(er);
                for (int i = 0; i < Header.NrEntries; i++)
                {
                    string s;
                    switch (s = er.ReadString(Encoding.ASCII, 4))
                    {
                        case "lyt1":
                            Layout = new lyt1(er); break;
                        default:
                            er.BaseStream.Position += er.ReadUInt32() - 4;
                            break;
                    }
                }
            }
            finally
            {
                er.Close();
            }
        }

        public Form GetDialog()
        {
            return new Form();
            //return new RLYTViewer(this);
        }

        public string GetSaveDefaultFileFilter()
        {
            return "Binary Revolution Layout (*.brlyt)|*.brlyt";
        }

        public byte[] Write()
        {
            MemoryStream m = new MemoryStream();
            EndianBinaryWriterEx er = new EndianBinaryWriterEx(m, Endianness.BigEndian);
            Header.Write(er);
            Layout.Write(er);
            byte[] b = m.ToArray();
            er.Close();
            return b;
        }

        public RLYTHeader Header;
        public class RLYTHeader
        {
            public RLYTHeader(EndianBinaryReaderEx er)
            {
                er.ReadObject(this);
            }
            public void Write(EndianBinaryWriterEx er)
            {
                er.Write(Signature, Encoding.ASCII, false);
                er.Write(Endianness);
                er.Write(Version);
                er.Write(FileSize);
                er.Write(HeaderSize);
                er.Write(NrEntries);
            }
            [BinaryStringSignature("RLYT")]
            [BinaryFixedSize(4)]
            public string Signature;
            [BinaryBOM(0xFFFE)]
            public UInt16 Endianness;
            public UInt16 Version;
            public UInt32 FileSize;
            public UInt16 HeaderSize;
            public UInt16 NrEntries;
        }

        public lyt1 Layout;
        public class lyt1
        {
            public lyt1(EndianBinaryReaderEx er)
            {
                Size = er.ReadUInt32();
                DrawFromMiddle = er.ReadByte() == 1;
                Padding = er.ReadBytes(3);
                Width = er.ReadSingle();
                Height = er.ReadSingle();
            }
            public void Write(EndianBinaryWriterEx er)
            {
                er.Write("lyt1", Encoding.ASCII, false);
                er.Write(Size);
                er.Write((byte)(DrawFromMiddle ? 1 : 0));
                er.Write(Padding, 0, 3);
                er.Write(Width);
                er.Write(Height);
            }
            public UInt32 Size;
            public bool DrawFromMiddle;
            public byte[] Padding;
            public Single Width;
            public Single Height;
        }

        public class RLYTIdentifier:FileFormatIdentifier
        {

            public override string GetCategory()
            {
                return Category_Layouts;
            }

            public override string GetFileDescription()
            {
                return "Binary Revolution Layout (RLYT)";
            }

            public override string GetFileFilter()
            {
                return "Binary Revolution Layout (*.brlyt)|*.brlyt";
            }

            public override Bitmap GetIcon()
            {
                return Resource.zone;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'R' && File.Data[1] == 'L' && File.Data[2] == 'Y' && File.Data[3] == 'T') return FormatMatch.Content;
                return FormatMatch.No;
            }
        }
    }
}
