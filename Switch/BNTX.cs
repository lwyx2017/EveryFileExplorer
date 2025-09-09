using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.IO;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Switch
{
    public class BNTX : FileFormat<BNTX.BNTXIdentifier>//, IViewable
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
                Signature = er.ReadString(Encoding.ASCII, 4);
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

        public BRTIInfo BRTI;
        public class BRTIInfo
        {
            public BRTIInfo(EndianBinaryReaderEx er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "BRTI") throw new SignatureNotCorrectException(Signature, "BRTI", er.BaseStream.Position - 4);
                BRTILength0 = er.ReadUInt32();
                BRTILength1 = er.ReadUInt64();
                Flags = er.ReadByte();
                Dimensions = er.ReadByte();
                TileMode = er.ReadUInt16();
                SwizzleSize = er.ReadUInt16();
                MipmapCount = er.ReadUInt16();
                MultiSampleCount = er.ReadUInt16();
                Reversed1A = er.ReadUInt16();
                Format = er.ReadUInt32();
                AccessFlags = er.ReadUInt32();
                Width = er.ReadUInt32();
                Height = er.ReadUInt32();
                Depth = er.ReadUInt32();
                ArrayCount = er.ReadUInt32();
                BlockHeightLog2 = er.ReadUInt32();
                Reserved38 = er.ReadUInt32();
                Reserved3C = er.ReadUInt32();
                Reserved40 = er.ReadUInt32();
                Reserved44 = er.ReadUInt32();
                Reserved48 = er.ReadUInt32();
                Reserved4C = er.ReadUInt32();
                DataLength = er.ReadUInt32();
                Alignment = er.ReadUInt32();
                ChannelTypes = er.ReadUInt32();
                TextureType = er.ReadUInt32();
                NameOffset = er.ReadUInt64();
                ParentOffset = er.ReadUInt64();
                PtrsOffset = er.ReadUInt64();
            }
            public String Signature;
            public UInt32 BRTILength0;
            public UInt64 BRTILength1;
            public Byte Flags;
            public Byte Dimensions;
            public UInt16 TileMode;
            public UInt16 SwizzleSize;
            public UInt16 MipmapCount;
            public UInt16 MultiSampleCount;
            public UInt16 Reversed1A;
            public UInt32 Format;
            public UInt32 AccessFlags;
            public UInt32 Width;
            public UInt32 Height;
            public UInt32 Depth;
            public UInt32 ArrayCount;
            public UInt32 BlockHeightLog2;
            public UInt32 Reserved38;
            public UInt32 Reserved3C;
            public UInt32 Reserved40;
            public UInt32 Reserved44;
            public UInt32 Reserved48;
            public UInt32 Reserved4C;
            public UInt32 DataLength;
            public UInt32 Alignment;
            public UInt32 ChannelTypes;
            public UInt32 TextureType;
            public UInt64 NameOffset;
            public UInt64 ParentOffset;
            public UInt64 PtrsOffset;
            public UInt64 PtrsSize;
        }

        public BNTXTextures Textures;
        public class BNTXTextures
        {

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