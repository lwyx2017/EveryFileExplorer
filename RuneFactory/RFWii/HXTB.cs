using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GCNWii.GPU;
using RuneFactory.UI;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.IO;

namespace RuneFactory
{
    public class HXTB:FileFormat<HXTB.HXTBIdentifier>,IViewable//, IConvertable
    {
        public List<HXTBTextureEntry> Textures { get; private set; }

        public HXTB(byte[] Data)
        {
            EndianBinaryReaderEx er = new EndianBinaryReaderEx(new MemoryStream(Data), Endianness.BigEndian);
            try
            {
                Header = new HXTBHeader(er);
                er.BaseStream.Position = Header.NameTableOffset;
                Textures = new List<HXTBTextureEntry>();
                for (uint i = 0; i < Header.FileCount; i++)
                {
                    long entryPosition = Header.NameTableOffset + i * 0x20;
                    er.BaseStream.Position = entryPosition;
                    var nameTable = new FileNameTable(er);
                    er.BaseStream.Position = nameTable.TextureHeaderOffset;
                    var texHeader = new HTXBTextureHeader(er);
                    er.BaseStream.Position = texHeader.TextureDataOffset;
                    byte[] texData = er.ReadBytes((int)texHeader.DataSize);
                    HTXBPaletteHeader palHeader = null;
                    byte[] palData = null;
                    if (nameTable.PaletteHeaderOffset != 0)
                    {
                        er.BaseStream.Position = nameTable.PaletteHeaderOffset;
                        palHeader = new HTXBPaletteHeader(er);

                        er.BaseStream.Position = palHeader.PaletteDataOffset;
                        palData = er.ReadBytes((int)palHeader.DataSize);
                    }

                    Textures.Add(new HXTBTextureEntry(
                        nameTable.TexturesName.TrimEnd('\0'),
                        texHeader,
                        texData,
                        palHeader,
                        palData
                    ));
                }
            }
            finally
            {
                er.Close();
            }
        }

        public Form GetDialog()
        {
            return new HXTBViewer(this);
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

        public HXTBHeader Header;
        public class HXTBHeader
        {
            public HXTBHeader(EndianBinaryReaderEx er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "HXTB") throw new SignatureNotCorrectException(Signature, "HXTB", er.BaseStream.Position - 4);
                Version = er.ReadString(Encoding.ASCII, 4);
                NameTableOffset = er.ReadUInt32();
                FileCount = er.ReadUInt32();

            }
            public String Signature;
            public String Version;
            public UInt32 NameTableOffset;
            public UInt32 FileCount;
        }

        public FileNameTable NameTable;
        public class FileNameTable
        {
            public FileNameTable(EndianBinaryReaderEx er)
            {
                TexturesName = er.ReadString(Encoding.ASCII, 16);
                HashValue = er.ReadUInt32();
                TextureHeaderOffset = er.ReadUInt32();
                PaletteHeaderOffset = er.ReadUInt32();
                Unknown = er.ReadUInt32();
            }
            public String TexturesName;
            public UInt32 HashValue;
            public UInt32 TextureHeaderOffset;
            public UInt32 PaletteHeaderOffset;
            public UInt32 Unknown;
        }

        public HTXBTextureHeader TextureHeader;
        public class HTXBTextureHeader
        {
            public HTXBTextureHeader(EndianBinaryReaderEx er)
            {
                TextureDataOffset = er.ReadUInt32();
                TextureFormat = (Textures.ImageFormat)er.ReadByte();
                MaxLOD = er.ReadByte();
                Padding = er.ReadUInt16();
                Width = er.ReadUInt16();
                Height = er.ReadUInt16();
                DataSize = er.ReadUInt32();
            }
            public UInt32 TextureDataOffset;
            public Textures.ImageFormat TextureFormat;
            public Byte MaxLOD;
            public UInt16 Padding;
            public UInt16 Width;
            public UInt16 Height;
            public UInt32 DataSize;
            public int MipMapCount => MaxLOD > 0 ? MaxLOD - 1 : 0;
        }
        public byte[] TextureData;

        public HTXBPaletteHeader PaletteHeader;
        public class HTXBPaletteHeader
        {
            public HTXBPaletteHeader(EndianBinaryReaderEx er)
            {
                PaletteDataOffset = er.ReadUInt32();
                PaletteFormat = (Textures.PaletteFormat)er.ReadByte();
                Unknown1 = er.ReadByte();
                Unknown2 = er.ReadUInt16();
                ColorCount = er.ReadUInt16();
                Unknown3 = er.ReadUInt16();
                DataSize = er.ReadUInt32();
            }
            public UInt32 PaletteDataOffset;
            public Textures.PaletteFormat PaletteFormat;
            public byte Unknown1;
            public UInt16 Unknown2;
            public UInt16 ColorCount;
            public UInt16 Unknown3;
            public UInt32 DataSize;
        }
        public byte[] PaletteData;

        public class HXTBTextureEntry
        {
            public string Name { get; }
            public HTXBTextureHeader TextureHeader { get; }
            public byte[] TextureData { get; }
            public HTXBPaletteHeader PaletteHeader { get; }
            public byte[] PaletteData { get; }

            public HXTBTextureEntry(
                string name,
                HTXBTextureHeader texHeader,
                byte[] textureData,
                HTXBPaletteHeader paletteHeader,
                byte[] paletteData)
            {
                Name = name;
                TextureHeader = texHeader;
                TextureData = textureData;
                PaletteHeader = paletteHeader;
                PaletteData = paletteData;
            }

            public Bitmap ToBitmap()
            {
                return GCNWii.GPU.Textures.ToBitmap(
                    TextureData,
                    0,
                    PaletteData,
                    0,
                    TextureHeader.Width,
                    TextureHeader.Height,
                    TextureHeader.TextureFormat,
                    PaletteHeader?.PaletteFormat ?? 0
                );
            }
        }

        public class HXTBIdentifier:FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Textures;
            }

            public override string GetFileDescription()
            {
                return "Rune Factory Wii Textures (HXTB)";
            }

            public override string GetFileFilter()
            {
                return "Rune Factory Wii Textures (*.hxtb)|*.hxtb";
            }

            public override Bitmap GetIcon()
            {
                return Resource.image_sunset;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'H' && File.Data[1] == 'X' && File.Data[2] == 'T' && File.Data[3] == 'B') return FormatMatch.Content;
                return FormatMatch.No;
            }
        }
    }
}