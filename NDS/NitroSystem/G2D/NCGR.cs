using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.IO;
using NDS.GPU;

namespace NDS.NitroSystem.G2D
{
    public class NCGR:FileFormat<NCGR.NCGRIdentifier>//, IViewable,IWriteable
    {
        public NCGR(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new NCGRHeader(er);
                CharacterData = new Characterdata(er);
                if (Header.NrBlocks > 1)
                {
                    CharacterPosInfoBlock = new CharacterposInfoBlock(er);
                }
            }
            finally
            {
                er.Close();
            }
        }

        public string GetSaveDefaultFileFilter()
        {
            return "Nitro Character Graphics For Runtime (*.ncgr)|*.ncgr";
        }

        public byte[] Write()
        {
            MemoryStream m = new MemoryStream();
            EndianBinaryWriter er = new EndianBinaryWriter(m, Endianness.LittleEndian);
            Header.Write(er);
            CharacterData.Write(er);
            er.BaseStream.Position = 8;
            er.Write((uint)er.BaseStream.Length);
            er.BaseStream.Position = 20;
            er.Write((uint)(er.BaseStream.Length - 16));
            byte[] result = m.ToArray();
            er.Close();
            return result;
        }

        public Form GetDialog()
        {
            return new Form();
        }

        public NCGRHeader Header;
        public class NCGRHeader
        {
            public NCGRHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "RGCN") throw new SignatureNotCorrectException(Signature, "RGCN", er.BaseStream.Position - 4);
                Endianness = er.ReadUInt16();
                Version = er.ReadUInt16();
                Filesize = er.ReadUInt32();
                HeaderSize = er.ReadUInt16();
                NrBlocks = er.ReadUInt16();
            }
            public void Write(EndianBinaryWriter er)
            {
                er.Write(Signature, Encoding.ASCII, false);
                er.Write(Endianness);
                er.Write(Version);
                er.Write(Filesize);
                er.Write(HeaderSize);
                er.Write(NrBlocks);
            }
            public string Signature;
            public UInt16 Endianness;
            public UInt16 Version;
            public UInt32 Filesize;
            public UInt16 HeaderSize;
            public UInt16 NrBlocks;
        }

        public Characterdata CharacterData;
        public class Characterdata
        {
            public Characterdata(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "RAHC") throw new SignatureNotCorrectException(Signature, "RAHC", er.BaseStream.Position - 4);
                BlockSize = er.ReadUInt32();
                H = er.ReadUInt16();
                W = er.ReadUInt16();
                pixelFmt = (Textures.ImageFormat)er.ReadUInt32();
                mapingType = (Textures.OBJVRamModeChar)er.ReadUInt32();
                characterFmt = (Textures.CharFormat)er.ReadUInt32();
                szByte = er.ReadUInt32();
                pRawData = er.ReadUInt32();
                Data = er.ReadBytes((int)szByte);
            }
            public void Write(EndianBinaryWriter er)
            {
                er.Write(Signature, Encoding.ASCII, false);
                er.Write(BlockSize);
                er.Write(H);
                er.Write(W);
                er.Write((uint)pixelFmt);
                er.Write((uint)mapingType);
                er.Write((uint)characterFmt);
                er.Write((uint)Data.Length);
                er.Write(pRawData);
                er.Write(Data, 0, Data.Length);
            }
            public string Signature;
            public uint BlockSize;
            public ushort W;
            public ushort H;
            public Textures.ImageFormat pixelFmt;
            public Textures.OBJVRamModeChar mapingType;
            public Textures.CharFormat characterFmt;
            public uint szByte;
            public uint pRawData;
            public byte[] Data;

            public Bitmap ToBitmap(NCLR Palette, int PalNr)
            {
                int width = 0;
                int height = 0;
                switch (mapingType)
                {
                    case Textures.OBJVRamModeChar.OBJVRAMMODE_CHAR_1D_32K:
                        width = 32;
                        height = Data.Length * ((pixelFmt != Textures.ImageFormat.PLTT16) ? 1 : 2) / 32;
                        break;
                    case Textures.OBJVRamModeChar.OBJVRAMMODE_CHAR_1D_64K:
                        width = 64;
                        height = Data.Length * ((pixelFmt != Textures.ImageFormat.PLTT16) ? 1 : 2) / 64;
                        break;
                    case Textures.OBJVRamModeChar.OBJVRAMMODE_CHAR_1D_128K:
                        width = 128;
                        height = Data.Length * ((pixelFmt != Textures.ImageFormat.PLTT16) ? 1 : 2) / 128;
                        break;
                    case Textures.OBJVRamModeChar.OBJVRAMMODE_CHAR_1D_256K:
                        width = 256;
                        height = Data.Length * ((pixelFmt != Textures.ImageFormat.PLTT16) ? 1 : 2) / 256;
                        break;
                    case Textures.OBJVRamModeChar.OBJVRAMMODE_CHAR_2D:
                        width = W * 8;
                        height = H * 8;
                        break;
                }
                return Textures.ToBitmap(Data,Palette.Palettedata.Data, PalNr, width, height, pixelFmt, characterFmt);
            }

            public Bitmap ToBitmap(byte[] Palette, int PalNr)
            {
                int width = 0;
                int height = 0;
                switch (mapingType)
                {
                    case Textures.OBJVRamModeChar.OBJVRAMMODE_CHAR_1D_32K:
                        width = 32;
                        height = Data.Length * ((pixelFmt != Textures.ImageFormat.PLTT16) ? 1 : 2) / 32;
                        break;
                    case Textures.OBJVRamModeChar.OBJVRAMMODE_CHAR_1D_64K:
                        width = 64;
                        height = Data.Length * ((pixelFmt != Textures.ImageFormat.PLTT16) ? 1 : 2) / 64;
                        break;
                    case Textures.OBJVRamModeChar.OBJVRAMMODE_CHAR_1D_128K:
                        width = 128;
                        height = Data.Length * ((pixelFmt != Textures.ImageFormat.PLTT16) ? 1 : 2) / 128;
                        break;
                    case Textures.OBJVRamModeChar.OBJVRAMMODE_CHAR_1D_256K:
                        width = 256;
                        height = Data.Length * ((pixelFmt != Textures.ImageFormat.PLTT16) ? 1 : 2) / 256;
                        break;
                    case Textures.OBJVRamModeChar.OBJVRAMMODE_CHAR_2D:
                        width = W * 8;
                        height = H * 8;
                        break;
                }
                return Textures.ToBitmap(Data, Palette, PalNr, width, height, pixelFmt, characterFmt);
            }
        }

        public CharacterposInfoBlock CharacterPosInfoBlock;
        public class CharacterposInfoBlock
        {
            public CharacterposInfoBlock(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "SOPC") throw new SignatureNotCorrectException(Signature, "SOPC", er.BaseStream.Position - 4);
                BlockSize = er.ReadUInt32();
                srcPosX = er.ReadUInt16();
                srcPosY = er.ReadUInt16();
                srcW = er.ReadUInt16();
                srcH = er.ReadUInt16();
            }
            public void Write(EndianBinaryWriter er)
            {
                er.Write(Signature, Encoding.ASCII, false);
                er.Write(BlockSize);
                er.Write(srcPosX);
                er.Write(srcPosY);
                er.Write(srcW);
                er.Write(srcH);
            }
            public string Signature;
            public UInt32 BlockSize;
            public ushort srcPosX;
            public ushort srcPosY;
            public ushort srcW;
            public ushort srcH;
        }

        public class NCGRIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Graphics;
            }

            public override string GetFileDescription()
            {
                return "Nitro Character Graphics For Runtime (NCGR)";
            }

            public override string GetFileFilter()
            {
                return "Nitro Character Graphics For Runtime (*.ncgr)|*.ncgr";
            }

            public override Bitmap GetIcon()
            {
                return Resource.image;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'R' && File.Data[1] == 'G' && File.Data[2] == 'C' && File.Data[3] == 'N') return FormatMatch.Content;
                return FormatMatch.No;
            }
        }
    }
}