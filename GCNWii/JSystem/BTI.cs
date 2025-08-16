using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GCNWii.GPU;
using GCNWii.UI;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.IO;

namespace GCNWii.JSystem
{
	public class BTI:FileFormat<BTI.BTIIdentifier>, IConvertable, IViewable, IWriteable, IFileCreatable
    {
		public BTI()
		{
			Header = new BTIHeader();
        }
        public BTI(byte[] Data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.BigEndian);
			try
			{
				Header = new BTIHeader(er);
				er.BaseStream.Position = Header.TextureOffset;
				int len = (int)(er.BaseStream.Length - Header.TextureOffset);
				if (Header.PaletteOffset != 0) len = (int)(Header.PaletteOffset - Header.TextureOffset);
				Texture = er.ReadBytes(len);
				if (Header.PaletteOffset != 0)
				{
					er.BaseStream.Position = Header.PaletteOffset;
					Palette = er.ReadBytes(Header.NrPaletteEntries * 2);
				}
			}
			finally
			{
				er.Close();
			}
		}

		public string GetConversionFileFilters()
		{
			return "Portable Network Graphics (*.png)|*.png";
		}

		public bool Convert(int FilterIndex, string Path)
		{
			switch (FilterIndex)
			{
				case 0:
					File.Create(Path).Close();
					ToBitmap().Save(Path, System.Drawing.Imaging.ImageFormat.Png);
					return true;
			}
			return false;
		}

		public Form GetDialog()
		{
			return new BTIViewer(this);
		}

        public string GetSaveDefaultFileFilter()
        {
            return "Binary Texture Image (*.bti)|*.bti";
        }

        public byte[] Write()
        {
            MemoryStream m = new MemoryStream();
            EndianBinaryWriter er = new EndianBinaryWriter(m, Endianness.BigEndian);
            const uint HeaderSize = 32;
            Header.TextureOffset = HeaderSize;
            if (Palette != null && Palette.Length > 0)
            {
                Header.PaletteOffset = HeaderSize + (uint)Texture.Length;
            }
            else
            {
                Header.PaletteOffset = 0;
                Header.NrPaletteEntries = 0;
            }
            Header.Write(er);
            er.Write(Texture);
            if (Header.PaletteOffset != 0)
            {
                er.Write(Palette);
            }
            er.Close();
            return m.ToArray();
        }

        public bool CreateFromFile()
        {
            OpenFileDialog f = new OpenFileDialog();
            f.Filter = "PNG Files (*.png)|*.png";
            if (f.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(f.FileName))
            {
                using (BTIGenDialog Dialog = new BTIGenDialog())
                {
                    if (Dialog.ShowDialog() != DialogResult.OK) return false;
                    Bitmap bmp = new Bitmap(Image.FromStream(new MemoryStream(File.ReadAllBytes(f.FileName))));
                    byte[] textureData = null;
                    byte[] paletteData = null;
                    GPU.Textures.FromBitmap(
                        bmp,ref textureData,ref paletteData,
                        bmp.Width,bmp.Height,
                        Dialog.SelectedTextureFormat,
                        Dialog.SelectedPaletteFormat
                    );

                    Header = new BTIHeader
                    {
                        TextureFormat = Dialog.SelectedTextureFormat,
                        EnableAlpha = 1,
                        Width = (ushort)bmp.Width,
                        Height = (ushort)bmp.Height,
                        PaletteFormat = Dialog.SelectedPaletteFormat,
                        NrPaletteEntries = (ushort)(paletteData?.Length / 2 ?? 0)
                    };
                    Texture = textureData;
                    Palette = paletteData;
                    bmp.Dispose();
                    return true;
                }
            }
            return false;
        }

        public BTIHeader Header;
		public class BTIHeader
		{
			public BTIHeader()
			{
				TextureFormat = 0;
				EnableAlpha = 1;
				Width = 0;
				Height = 0;
				WrapS = 0;
				WrapT = 0;
				PaletteFormat =0;
				NrPaletteEntries = 0;
				PaletteOffset = 0;
				Unknown1 = 0;
				MagnificationFilterType = 0;
				Unknown2 = 0;
				MipMapCount = 1;
				Unknown3 = 0;
				Unknown4 = 0;
				TextureOffset = 32;
            }
            public BTIHeader(EndianBinaryReader er)
			{
				TextureFormat = (Textures.ImageFormat)er.ReadByte();
                EnableAlpha = er.ReadByte();
				Width = er.ReadUInt16();
				Height = er.ReadUInt16();
                WrapS = er.ReadUInt16();
                WrapT = er.ReadByte();
				PaletteFormat = (Textures.PaletteFormat)er.ReadByte();
				NrPaletteEntries = er.ReadUInt16();
				PaletteOffset = er.ReadUInt32();
				Unknown1 = er.ReadUInt32();
                MagnificationFilterType = er.ReadUInt16();
				Unknown2 = er.ReadUInt16();
				MipMapCount = er.ReadByte();
				Unknown3 = er.ReadByte();
				Unknown4 = er.ReadUInt16();
				TextureOffset = er.ReadUInt32();
			}
            public void Write(EndianBinaryWriter er)
            {
                er.Write((byte)TextureFormat);
                er.Write(EnableAlpha);
                er.Write(Width);
				er.Write(Height);
				er.Write(WrapS);
				er.Write(WrapT);
				er.Write((byte)PaletteFormat);
				er.Write(NrPaletteEntries);
				er.Write(PaletteOffset);
				er.Write(Unknown1);
				er.Write(MagnificationFilterType);
				er.Write(Unknown2);
				er.Write(MipMapCount);
				er.Write(Unknown3);
				er.Write(Unknown4);
				er.Write(TextureOffset);
            }
            public Textures.ImageFormat TextureFormat; //1
			public Byte EnableAlpha;
			public UInt16 Width;
			public UInt16 Height;
			public UInt16 WrapS;
			public Byte WrapT;
			public Textures.PaletteFormat PaletteFormat; //1
			public UInt16 NrPaletteEntries;
			public UInt32 PaletteOffset;
			public UInt32 Unknown1;
			public UInt16 MagnificationFilterType;
			public UInt16 Unknown2;
			public Byte MipMapCount;
			public Byte Unknown3;
			public UInt16 Unknown4;
			public UInt32 TextureOffset;
		}
		public byte[] Texture;
		public byte[] Palette;

		public Bitmap ToBitmap(int Level = 0)
		{
			int l = Level;
			uint w = Header.Width;
			uint h = Header.Height;
			int bpp = GPU.Textures.GetBpp(Header.TextureFormat);
			int offset = 0;
			while (l > 0)
			{
				offset += (int)(w * h * bpp / 8);
				w /= 2;
				h /= 2;
				l--;
			}
			return GPU.Textures.ToBitmap(Texture, offset, Palette, 0, (int)w, (int)h, Header.TextureFormat, Header.PaletteFormat);
		}

		public class BTIIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Graphics;
			}

			public override string GetFileDescription()
			{
				return "Binary Texture Image (BTI)";
			}

			public override string GetFileFilter()
			{
				return "Binary Texture Image (*.bti)|*.bti";
			}

			public override Bitmap GetIcon()
			{
				return Resource.image;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Name.ToLower().EndsWith(".bti")) return FormatMatch.Extension;
				return FormatMatch.No;
			}
		}
	}
}