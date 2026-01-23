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
    public class NSCR:FileFormat<NSCR.NSCRIdentifier>//,IViewable,IWriteable
    {
        public NSCR(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new NSCRHeader(er);
                ScreenData = new Screendata(er);
            }
            finally
            {
                er.Close();
            }
        }

        public string GetSaveDefaultFileFilter()
        {
            return "Nitro Screen For Runtime (*.nscr)|*.nscr";
        }

        public byte[] Write()
        {
            MemoryStream m = new MemoryStream();
            EndianBinaryWriter er = new EndianBinaryWriter(m, Endianness.LittleEndian);
            Header.Write(er);
            ScreenData.Write(er);
            er.BaseStream.Position = 8L;
            er.Write((uint)er.BaseStream.Length);
            er.BaseStream.Position = 20L;
            er.Write((uint)(er.BaseStream.Length - 16));
            byte[] result = m.ToArray();
            er.Close();
            return result;
        }

        public Form GetDialog()
        {
            return new Form();
        }

        public NSCRHeader Header;
        public class NSCRHeader
        {
            public NSCRHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "RCSN") throw new SignatureNotCorrectException(Signature, "RCSN", er.BaseStream.Position - 4);
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

        public Screendata ScreenData;
        public class Screendata
        {
            public Screendata(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "NRCS") throw new SignatureNotCorrectException(Signature, "NRCS", er.BaseStream.Position - 4);
                BlockSize = er.ReadUInt32();
                screenWidth = er.ReadUInt16();
                screenHeight = er.ReadUInt16();
                colorMode = (Textures.G2DColorMode)er.ReadUInt16();
                screenFormat = (Textures.ScreenFormat)er.ReadUInt16();
                szByte = er.ReadUInt32();
                Data = er.ReadBytes((int)szByte);
            }
            public void Write(EndianBinaryWriter er)
            {
                er.Write(Signature, Encoding.ASCII, false);
                er.Write(BlockSize);
                er.Write(screenWidth);
                er.Write(screenHeight);
                er.Write((ushort)colorMode);
                er.Write((ushort)screenFormat);
                er.Write(szByte);
                er.Write(Data, 0, Data.Length);
            }
            public string Signature;
            public UInt32 BlockSize;
            public ushort screenWidth;
            public ushort screenHeight;
            public Textures.G2DColorMode colorMode;
            public Textures.ScreenFormat screenFormat;
            public uint szByte;
            public byte[] Data;

            public Bitmap ToBitmap(NCGR Image, NCLR Palette)
            {
                return Textures.ToBitmap(Image.CharacterData.Data,Image.CharacterData.W * 8,Image.CharacterData.H * 8,Palette.Palettedata.Data,Data,screenWidth,screenHeight,Image.CharacterData.pixelFmt,Image.CharacterData.characterFmt);
            }
        }
        public class NSCRIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Screens;
            }

            public override string GetFileDescription()
            {
                return "Nitro Screen For Runtime (NSCR)";
            }

            public override string GetFileFilter()
            {
                return "Nitro Screen For Runtime (*.nscr)|*.nscr";
            }

            public override Bitmap GetIcon()
            {
                return Resource.map;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'R' && File.Data[1] == 'C' && File.Data[2] == 'S' && File.Data[3] == 'N') return FormatMatch.Content;
                return FormatMatch.No;
            }
        }
    }
}