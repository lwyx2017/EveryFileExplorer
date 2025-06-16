using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.IO.Serialization;

namespace GCNWii.NintendoWare.LYT
{
    public class RLAN:FileFormat<RLAN.RLANIdentifier>//,IViewable
    {
        public RLAN(byte[] Data)
        {
            EndianBinaryReaderEx er = new EndianBinaryReaderEx(new MemoryStream(Data), Endianness.BigEndian);
            try
            {
                Header = new RLANHeader(er);
            }
            finally
            {
                er.Close();
            }
        }

        public Form GetDialog()
        {
            return new Form();
        }

        public RLANHeader Header;
        public class RLANHeader
        {
            public RLANHeader(EndianBinaryReaderEx er)
            {
                er.ReadObject(this);
            }
            [BinaryStringSignature("RLAN")]
            [BinaryFixedSize(4)]
            public string Signature;
            [BinaryBOM(0xFFFE)]
            public UInt16 Endianness;
            public UInt16 Version;
            public UInt32 FileSize;
            public UInt16 HeaderSize;
            public UInt16 NrEntries;
        }

        public class RLANIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Animations;
            }

            public override string GetFileDescription()
            {
                return "Binary Revolution Layout Animation (RLAN)";
            }

            public override string GetFileFilter()
            {
                return "Binary Revolution Layout Animation (*.brlan)|*.brlan";
            }

            public override Bitmap GetIcon()
            {
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'R' && File.Data[1] == 'L' && File.Data[2] == 'A' && File.Data[3] == 'N') return FormatMatch.Content;
                return FormatMatch.No;
            }
        }
    }
}