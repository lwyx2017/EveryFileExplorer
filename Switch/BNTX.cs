using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.IO;

namespace Switch
{
    public class BNTX:FileFormat<BNTX.BNTXIdentifier>,IViewable
    {
        public BNTX(byte[] Data)
        {
            EndianBinaryReaderEx er = new EndianBinaryReaderEx(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new BNTXHeader(er);
                Info = new TextureInfo(er);
            }
            finally
            {
                er.Close();
            }
        }

        public Form GetDialog()
        {
            return new Form();
            //return new BNTXViewer(this);
        }

        public BNTXHeader Header;
        public class BNTXHeader
        {
            public BNTXHeader(EndianBinaryReaderEx er)
            {
                Signature = er.ReadString(Encoding.ASCII,4);
                if (Signature != "BNTX") throw new SignatureNotCorrectException(Signature, "BNTX", er.BaseStream.Position - 4);
                Padding = er.ReadUInt32();
                DataLength = er.ReadUInt32();
                Endianness = er.ReadUInt16();
                Version = er.ReadUInt16();
                NameOffset = er.ReadUInt32();
                StringOffset = er.ReadUInt32();
                RelocationTableOffset = er.ReadUInt32();
                FileSize = er.ReadUInt32();
            }
            public String Signature;
            public UInt32 Padding;
            public UInt32 DataLength;
            public UInt16 Endianness;
            public UInt16 Version;
            public UInt32 NameOffset;
            public UInt32 StringOffset;
            public UInt32 RelocationTableOffset;
            public UInt32 FileSize;
        }

        public TextureInfo Info;
        public class TextureInfo
        {
            public TextureInfo(EndianBinaryReaderEx er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "NX  ") throw new SignatureNotCorrectException(Signature, "NX  ", er.BaseStream.Position - 4);
                TextureCount = er.ReadUInt32();
                BRTIOffset = er.ReadUInt64();
                DataBlockOffset = er.ReadUInt64();
                NameDictionaryOffset = er.ReadUInt64();
                NameDictionaryLength = er.ReadUInt32();
            }
            public String Signature;
            public UInt32 TextureCount;
            public UInt64 BRTIOffset;
            public UInt64 DataBlockOffset;
            public UInt64 NameDictionaryOffset;
            public UInt32 NameDictionaryLength;
        }

        public class BNTXIdentifier:FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Textures;
            }

            public override string GetFileDescription()
            {
                return "Binary NX Texture (BNTX)";
            }

            public override string GetFileFilter()
            {
                return "Binary NX Texture (*.bntx)|*.bntx";
            }

            public override Bitmap GetIcon()
            {
                return Resource.image_sunset;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'B' && File.Data[1] == 'N' && File.Data[2] == 'T' && File.Data[3] == 'X') return FormatMatch.Content;
                return FormatMatch.No;
            }
        }
    }
}