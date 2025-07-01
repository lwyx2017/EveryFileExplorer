using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GCNWii.GPU;
using GCNWii.UI;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.IO;

namespace GCNWii.NintendoWare.LYT
{
    public class TPL : FileFormat<TPL.TPLIdentifier>, IViewable, IWriteable, IFileCreatable//, IConvertable
    {
        public TPL()
        {
            Header = new TPLHeader();
            Textures = new[] {new TPLTexture()};
        }

        public TPL(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.BigEndian);
            try
            {
                Header = new TPLHeader(er);
                Textures = new TPLTexture[Header.NrTextures];
                for (int i = 0; i < Header.NrTextures; i++)
                {
                    Textures[i] = new TPLTexture(er);
                }
            }
            finally
            {
                er.Close();
            }
        }

        public Form GetDialog()
        {
            return new TPLViewer(this);
        }

        public string GetSaveDefaultFileFilter()
        {
            return "Texture Palette Library (*.tpl)|*.tpl";
        }

        public byte[] Write()
        {
            MemoryStream m = new MemoryStream();
            EndianBinaryWriter er = new EndianBinaryWriter(m, Endianness.BigEndian);
            Header.Write(er);
            long textureEntriesPos = er.BaseStream.Position;
            er.Write(new byte[Header.NrTextures * 8]);
            var fixups = new List<Action>();
            var paletteDataPositions = new Dictionary<int, long>();
            for (int i = 0; i < Textures.Length; i++)
            {
                var tex = Textures[i];
                if (tex.PaletteHeader == null || tex.PaletteData == null) continue;
                while (er.BaseStream.Position % 32 != 0)
                    er.Write((byte)0x00);
                long palDataPos = er.BaseStream.Position;
                paletteDataPositions.Add(i, palDataPos);
                er.Write(tex.PaletteData);
                while (er.BaseStream.Position % 32 != 0)
                    er.Write((byte)0x00);
            }
            var textureHeaderPositions = new Dictionary<int, long>();
            for (int i = 0; i < Textures.Length; i++)
            {
                var tex = Textures[i];
                while (er.BaseStream.Position % 32 != 0)
                    er.Write((byte)0x00);
                long texHeaderPos = er.BaseStream.Position;
                textureHeaderPositions.Add(i, texHeaderPos);
                er.Write(new byte[36]);
                while (er.BaseStream.Position % 32 != 0)
                    er.Write((byte)0x00);
                long texDataPos = er.BaseStream.Position;
                er.Write(tex.TextureData);
                while (er.BaseStream.Position % 32 != 0)
                    er.Write((byte)0x00);
                fixups.Add(() =>
                {
                    long currentPos = er.BaseStream.Position;
                    er.BaseStream.Position = texHeaderPos;

                    tex.TextureHeader.TextureDataOffset = (uint)texDataPos;
                    tex.TextureHeader.Write(er);

                    er.BaseStream.Position = currentPos;
                });
            }
            var paletteHeaderPositions = new Dictionary<int, long>();
            for (int i = 0; i < Textures.Length; i++)
            {
                var tex = Textures[i];
                if (tex.PaletteHeader == null || tex.PaletteData == null) continue;
                while (er.BaseStream.Position % 32 != 0)
                    er.Write((byte)0x00);
                long palHeaderPos = er.BaseStream.Position;
                paletteHeaderPositions.Add(i, palHeaderPos);
                tex.PaletteHeader.PaletteDataOffset = (uint)paletteDataPositions[i];
                tex.PaletteHeader.Write(er);
                while (er.BaseStream.Position % 32 != 0)
                    er.Write((byte)0x00);
            }
            foreach (var fix in fixups) fix();
            er.BaseStream.Position = textureEntriesPos;
            for (int i = 0; i < Textures.Length; i++)
            {
                var tex = Textures[i];
                uint texHeaderOffset = textureHeaderPositions.ContainsKey(i) ?
                (uint)textureHeaderPositions[i] : 0;
                uint palHeaderOffset = paletteHeaderPositions.ContainsKey(i) ?
                (uint)paletteHeaderPositions[i] : 0;
                er.Write(texHeaderOffset);
                er.Write(palHeaderOffset);
            }
            while (er.BaseStream.Position % 32 != 0)
                er.Write((byte)0x00);
            er.Close();
            return m.ToArray();
        }

        public string GetConversionFileFilters()
        {
            return "Portable Network Graphics (*.png)|*.png";
        }

        public bool Convert(int FilterIndex, string Path)
        {
            File.Create(Path).Close();
            Textures[0].ToBitmap().Save(Path, System.Drawing.Imaging.ImageFormat.Png);
            return true;
        }

        public bool CreateFromFile()
        {
            OpenFileDialog f = new OpenFileDialog();
            f.Filter = "PNG Files (*.png)|*.png";
            if (f.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(f.FileName))
            {
                Bitmap bmp = new Bitmap(Image.FromStream(new MemoryStream(File.ReadAllBytes(f.FileName))));
                Header = new TPLHeader();
                Textures = new TPLTexture[1];
                byte[] textureData = null;
                byte[] paletteData = null;
                GPU.Textures.FromBitmap(
                    bmp,
                    ref textureData,
                    ref paletteData,
                    bmp.Width,
                    bmp.Height,
                    GPU.Textures.ImageFormat.RGB565,
                    GPU.Textures.PaletteFormat.RGB565
                );

                var textureHeader = new TPLTexture.TPLTextureHeader
                {
                    Height = (ushort)bmp.Height,
                    Width = (ushort)bmp.Width,
                    TextureFormat = GPU.Textures.ImageFormat.RGB565,
                };

                Textures[0] = new TPLTexture
                {
                    TextureHeader = textureHeader,
                    TextureData = textureData,
                    PaletteHeader = null,
                    PaletteData = null
                };

                bmp.Dispose();
                return true;
            }
            return false;
        }

        public TPLHeader Header;
        public class TPLHeader
        {
            public TPLHeader()
            {
                Signature = Signature = new byte[] { 0x00, 0x20, 0xAF, 0x30 };
                NrTextures = 1;
                HeaderSize = 0x0C;
            }
            public TPLHeader(EndianBinaryReader er)
            {
                Signature = er.ReadBytes(4);
                if (Signature[0] != 0 || Signature[1] != 0x20 || Signature[2] != 0xAF || Signature[3] != 0x30)
                    throw new SignatureNotCorrectException("{ " + BitConverter.ToString(Signature, 0, 4).Replace("-", ", ") + " }", "{ 0x00, 0x20, 0xAF, 0x30 }", er.BaseStream.Position - 4);
                NrTextures = er.ReadUInt32();
                HeaderSize = er.ReadUInt32();
            }
            public void Write(EndianBinaryWriter er)
            {
                er.Write(Signature);
                er.Write(NrTextures);
                er.Write(HeaderSize);
            }
            public byte[] Signature;//0x00, 0x20, 0xAF, 0x30
            public UInt32 NrTextures;
            public UInt32 HeaderSize;
        }

        public TPLTexture[] Textures;
        public class TPLTexture
        {
            public TPLTexture()
            {
                TextureHeader = new TPLTextureHeader();
                PaletteHeader = null;
                TextureData = new byte[0];
                PaletteData = null;
                TextureHeaderOffset = 0;
                PaletteHeaderOffset = 0;
            }
            public TPLTexture(EndianBinaryReader er)
            {
                TextureHeaderOffset = er.ReadUInt32();
                PaletteHeaderOffset = er.ReadUInt32();
                long curpos = er.BaseStream.Position;
                er.BaseStream.Position = curpos;
                er.BaseStream.Position = TextureHeaderOffset;
                TextureHeader = new TPLTextureHeader(er);
                er.BaseStream.Position = TextureHeader.TextureDataOffset;
                TextureData = er.ReadBytes(GPU.Textures.GetDataSize(TextureHeader.TextureFormat, TextureHeader.Width, TextureHeader.Height));
                if (PaletteHeaderOffset != 0)
                {
                    er.BaseStream.Position = PaletteHeaderOffset;
                    PaletteHeader = new TPLPaletteHeader(er);
                    er.BaseStream.Position = PaletteHeader.PaletteDataOffset;
                    PaletteData = er.ReadBytes((int)(PaletteHeader.NrEntries * 2));
                }
                er.BaseStream.Position = curpos;
            }
            public void Write(EndianBinaryWriter er)
            {
                er.Write(TextureHeaderOffset);
                er.Write(PaletteHeaderOffset);
            }
            public UInt32 TextureHeaderOffset;
            public UInt32 PaletteHeaderOffset;

            public TPLTextureHeader TextureHeader;
            public class TPLTextureHeader
            {
                public TPLTextureHeader()
                {
                    WrapS = 0;
                    WrapT = 0;
                    MinFilter = 0;
                    MagFilter = 0;
                    LodBias = 0;
                    EdgeLod = 0;
                    MinLod = 0;
                    MaxLod = 0;
                    Padding = 0;
                }
                public TPLTextureHeader(EndianBinaryReader er)
                {
                    Height = er.ReadUInt16();
                    Width = er.ReadUInt16();
                    TextureFormat = (Textures.ImageFormat)er.ReadUInt32();
                    TextureDataOffset = er.ReadUInt32();
                    WrapS = er.ReadUInt32();
                    WrapT = er.ReadUInt32();
                    MinFilter = er.ReadUInt32();
                    MagFilter = er.ReadUInt32();
                    LodBias = er.ReadSingle();
                    EdgeLod = er.ReadByte();
                    MinLod = er.ReadByte();
                    MaxLod = er.ReadByte();
                    Padding = er.ReadByte();
                }
                public void Write(EndianBinaryWriter er)
                {
                    er.Write(Height);
                    er.Write(Width);
                    er.Write((UInt32)TextureFormat);
                    er.Write(TextureDataOffset);
                    er.Write(WrapS);
                    er.Write(WrapT);
                    er.Write(MinFilter);
                    er.Write(MagFilter);
                    er.Write(LodBias);
                    er.Write(EdgeLod);
                    er.Write(MinLod);
                    er.Write(MaxLod);
                    er.Write(Padding);
                }
                public UInt16 Height;
                public UInt16 Width;
                public Textures.ImageFormat TextureFormat;//4
                public UInt32 TextureDataOffset;
                public UInt32 WrapS;
                public UInt32 WrapT;
                public UInt32 MinFilter;
                public UInt32 MagFilter;
                public Single LodBias;
                public Byte EdgeLod;
                public Byte MinLod;
                public Byte MaxLod;
                public Byte Padding;//?
            }
            public byte[] TextureData;

            public TPLPaletteHeader PaletteHeader;
            public class TPLPaletteHeader
            {
                public TPLPaletteHeader(EndianBinaryReader er)
                {
                    NrEntries = er.ReadUInt16();
                    Padding = er.ReadUInt16();
                    PaletteFormat = (Textures.PaletteFormat)er.ReadUInt32();
                    PaletteDataOffset = er.ReadUInt32();
                }
                public void Write(EndianBinaryWriter er)
                {
                    er.Write(NrEntries);
                    er.Write(Padding);
                    er.Write((UInt32)PaletteFormat);
                    er.Write(PaletteDataOffset);
                }
                public UInt16 NrEntries;
                public UInt16 Padding;//?
                public Textures.PaletteFormat PaletteFormat;//4
                public UInt32 PaletteDataOffset;
            }
            public byte[] PaletteData;

            public Bitmap ToBitmap()
            {
                return GPU.Textures.ToBitmap
                    (TextureData, 0, PaletteData, 0,
                    TextureHeader.Width,
                    TextureHeader.Height,
                    TextureHeader.TextureFormat,
                    (PaletteHeader == null ? 0 : PaletteHeader.PaletteFormat));
            }
        }

        public class TPLIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Graphics;
            }

            public override string GetFileDescription()
            {
                return "Texture Palette Library (TPL)";
            }

            public override string GetFileFilter()
            {
                return "Texture Palette Library (*.tpl)|*.tpl";
            }

            public override Bitmap GetIcon()
            {
                return Resource.image;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 0x40 && File.Data[0] == 0x00 && File.Data[1] == 0x20 && File.Data[2] == 0xAF && File.Data[3] == 0x30) return FormatMatch.Content;
                return FormatMatch.No;
            }

        }
    }
}