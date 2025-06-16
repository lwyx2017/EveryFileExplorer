using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.IO;

namespace GCNWii.JSystem.THP
{
    public class THP:FileFormat<THP.THPIdentifier>//, IViewable
    {
        public THP(byte[] Data)
        {
            EndianBinaryReaderEx er = new EndianBinaryReaderEx(new MemoryStream(Data), Endianness.BigEndian);
            try
            {
                Header = new THPHeader(er);
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

        public THPHeader Header;
        public class THPHeader
        {
            public THPHeader(EndianBinaryReaderEx er)
            {
                Signature = er.ReadBytes(5);
                if (Signature[0] != 0x54 || Signature[1] != 0x48 || Signature[2] != 0x50 || Signature[3] != 0 || Signature[4] != 0)
                    throw new SignatureNotCorrectException("{ " + BitConverter.ToString(Signature, 0, 5).Replace("-", ", ") + " }", "{ 0x54, 0x48, 0x50, 0x00, 0x00 }", er.BaseStream.Position - 5);
            }
            public byte[] Signature;
        }

        public class THPIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Videos;
            }

            public override string GetFileDescription()
            {
                return "JSystem Cutscenes (THP)";
            }

            public override string GetFileFilter()
            {
                return "JSystem Cutscenes (*.thp)|*.thp";
            }

            public override Bitmap GetIcon()
            {
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 0x10 && File.Data[0] == 0x54 && File.Data[1] == 0x48 && File.Data[2] == 0x50 && File.Data[3] == 0x00 && File.Data[4] == 0x00) return FormatMatch.Content;
                return FormatMatch.No;
            }
        }
    }
}
