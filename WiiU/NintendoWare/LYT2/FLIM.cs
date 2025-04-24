using System;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using LibEveryFileExplorer.IO;
using System.Windows.Forms;
using WiiU.UI;
using System.Drawing.Imaging;
using LibEveryFileExplorer.IO.Serialization;

namespace WiiU.NintendoWare.LYT2
{
	public class FLIM : FileFormat<FLIM.FLIMIdentifier>, IConvertable, IFileCreatable, IViewable, IWriteable
    {
        public FLIM()
        {
            Data = new byte[0];
            Header = new FLIMHeader();
            Image = new imag();
        }
        public FLIM(byte[] Data)
		{
			EndianBinaryReaderEx er = new EndianBinaryReaderEx(new MemoryStream(Data), Endianness.LittleEndian);
			er.BaseStream.Position = Data.Length - 0x28;
			try
			{
				Header = new FLIMHeader(er);
				Image = new imag(er);
				DataLength = er.ReadUInt32();
				er.BaseStream.Position = 0;
				this.Data = er.ReadBytes((int)DataLength);
			}
			finally
			{
				er.Close();
			}
		}

		public Form GetDialog()
		{
			return new FLIMViewer(this);
		}

		public string GetSaveDefaultFileFilter()
		{
			return "Cafe Layout Images (*.bflim)|*.bflim";
		}

        public byte[] Write()
        {
            MemoryStream m = new MemoryStream();
            Endianness endian = Header.Endianness == 0xFFFE ? Endianness.LittleEndian : Endianness.BigEndian;
            EndianBinaryWriterEx er = new EndianBinaryWriterEx(m, endian);
            er.Write(Data, 0, Data.Length);
            long headerPosition = er.BaseStream.Position;
            er.Write(new byte[0x14], 0, 0x14);
            Image.Write(er);
            er.Write(DataLength);
            Header.FileSize = (uint)er.BaseStream.Length;
            er.BaseStream.Position = headerPosition;
            Header.Write(er);
            er.BaseStream.Position = er.BaseStream.Length;
            byte[] result = m.ToArray();
            er.Close();
            return result;
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
					ToBitmap().Save(Path, ImageFormat.Png);
					return true;
			}
			return false;
		}

        public bool CreateFromFile()
        {
            System.Windows.Forms.OpenFileDialog f = new System.Windows.Forms.OpenFileDialog();
            f.Filter = "PNG Files (*.png)|*.png";
            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK
                && f.FileName.Length > 0)
            {
                Bitmap bitmap = new Bitmap(new MemoryStream(File.ReadAllBytes(f.FileName)));
                UI.BFLIMGenDialog FLIMGenDialog = new BFLIMGenDialog();
                FLIMGenDialog.ShowDialog();
                Image.Width = (ushort)bitmap.Width;
                Image.Height = (ushort)bitmap.Height;
                switch (FLIMGenDialog.index)
                {
                    case 0:
                        Image.Format = 9;
                        Data = _3DS.GPU.Textures.FromBitmap(bitmap, _3DS.GPU.Textures.ImageFormat.RGBA8);
                        break;
                    case 1:
                        Image.Format = 6;
                        Data = _3DS.GPU.Textures.FromBitmap(bitmap, _3DS.GPU.Textures.ImageFormat.RGB8);
                        break;
                    case 2:
                        Image.Format = 7;
                        Data = _3DS.GPU.Textures.FromBitmap(bitmap, _3DS.GPU.Textures.ImageFormat.RGBA5551);
                        break;
                    case 3:
                        Image.Format = 5;
                        Data = _3DS.GPU.Textures.FromBitmap(bitmap, _3DS.GPU.Textures.ImageFormat.RGB565);
                        break;
                    case 4:
                        Image.Format = 8;
                        Data = _3DS.GPU.Textures.FromBitmap(bitmap, _3DS.GPU.Textures.ImageFormat.RGBA4);
                        break;
                    case 5:
                        Image.Format = 3;
                        Data = _3DS.GPU.Textures.FromBitmap(bitmap, _3DS.GPU.Textures.ImageFormat.LA8);
                        break;
                    case 6:
                        Image.Format = 4;
                        Data = _3DS.GPU.Textures.FromBitmap(bitmap, _3DS.GPU.Textures.ImageFormat.HILO8);
                        break;
                    case 7:
                        Image.Format = 0;
                        Data = _3DS.GPU.Textures.FromBitmap(bitmap, _3DS.GPU.Textures.ImageFormat.L8);
                        break;
                    case 8:
                        Image.Format = 1;
                        Data = _3DS.GPU.Textures.FromBitmap(bitmap, _3DS.GPU.Textures.ImageFormat.A8);
                        break;
                    case 9:
                        Image.Format = 2;
                        Data = _3DS.GPU.Textures.FromBitmap(bitmap, _3DS.GPU.Textures.ImageFormat.LA4);
                        break;
                    case 10:
                        Image.Format = 12;
                        Data = _3DS.GPU.Textures.FromBitmap(bitmap, _3DS.GPU.Textures.ImageFormat.L4);
                        break;
                    case 11:
                        Image.Format = 13;
                        Data = _3DS.GPU.Textures.FromBitmap(bitmap, _3DS.GPU.Textures.ImageFormat.A4);
                        break;
                    case 12:
                        Image.Format = 10;
                        Data = _3DS.GPU.Textures.FromBitmap(bitmap, _3DS.GPU.Textures.ImageFormat.ETC1);
                        break;
                    case 13:
                        Image.Format = 11;
                        Data = _3DS.GPU.Textures.FromBitmap(bitmap, _3DS.GPU.Textures.ImageFormat.ETC1A4);
                        break;
                    default:
                        throw new Exception("Please select the Image format!");
                }
                DataLength = (uint)Data.Length;
                return true;
            }
            return false;
        }

        public byte[] Data;

		public FLIMHeader Header;

		public class FLIMHeader
		{
			public FLIMHeader()
			{
				Signature = "FLIM";
				Endianness = 0xFFFE;
				HeaderSize = 0x14;
				Version = 0x02020000;
				NrBlocks = 1;
			}

			public FLIMHeader(EndianBinaryReaderEx er)
			{
				er.ReadObject(this);
			}
            public void Write(EndianBinaryWriterEx er)
            {
                if (Endianness == 0xFFFE)
                {
                    er.Write(Signature, Encoding.ASCII, false);
                    er.Write((ushort)0xFEFF);
                    er.Write(HeaderSize);
                    er.Write(Version);
                    er.Write(FileSize);
                    er.Write(NrBlocks);
                }
                else
                {
                    er.Write(Signature, Encoding.ASCII, false);
                    er.Write(Endianness);
                    er.Write(HeaderSize);
                    er.Write(Version);
                    er.Write(FileSize);
                    er.Write(NrBlocks);
                }
            }
            [BinaryStringSignature("FLIM")]
			[BinaryFixedSize(4)]
			public string Signature;
			[BinaryBOM(0xFFFE)]
			public UInt16 Endianness;
			public UInt16 HeaderSize;
			public UInt32 Version;
			public UInt32 FileSize;
			public UInt32 NrBlocks;
		}

        public imag Image;
		public class imag
		{
            public imag()
            {
                Signature = "imag";
                SectionSize = 0x10;
            }
            public imag(EndianBinaryReaderEx er)
			{
				er.ReadObject(this);
			}
			public void Write(EndianBinaryWriterEx er)
			{
				er.Write(Signature, Encoding.ASCII, false);
				er.Write(SectionSize);
				er.Write(Width);
				er.Write(Height);
                er.Write(Alignment);
                er.Write(Format);
                er.Write(Flag);
            }
			[BinaryStringSignature("imag")]
			[BinaryFixedSize(4)]
			public String Signature;
			public UInt32 SectionSize;
			public UInt16 Width;
			public UInt16 Height;
			public UInt16 Alignment;
			public Byte Format;
			public Byte Flag;
        }
        public UInt32 DataLength;

        public Bitmap ToBitmap()
		{
			if (Header.Endianness == 0xFFFE)//3ds
			{
                _3DS.GPU.Textures.ImageFormat f3;
                switch (Image.Format)
				{
					case 0: f3 = _3DS.GPU.Textures.ImageFormat.L8; break;
					case 1: f3 = _3DS.GPU.Textures.ImageFormat.A8; break;
					case 2: f3 = _3DS.GPU.Textures.ImageFormat.LA4; break;
					case 3: f3 = _3DS.GPU.Textures.ImageFormat.LA8; break;
					case 4: f3 = _3DS.GPU.Textures.ImageFormat.HILO8; break;
					case 5: f3 = _3DS.GPU.Textures.ImageFormat.RGB565; break;
					case 6: f3 = _3DS.GPU.Textures.ImageFormat.RGB8; break;
					case 7: f3 = _3DS.GPU.Textures.ImageFormat.RGBA5551; break;
					case 8: f3 = _3DS.GPU.Textures.ImageFormat.RGBA4; break;
					case 9: f3 = _3DS.GPU.Textures.ImageFormat.RGBA8; break;
					case 10: f3 = _3DS.GPU.Textures.ImageFormat.ETC1; break;
					case 11: f3 = _3DS.GPU.Textures.ImageFormat.ETC1A4; break;
					case 0x12: f3 = _3DS.GPU.Textures.ImageFormat.L4; break;
					case 0x13: f3 = _3DS.GPU.Textures.ImageFormat.A4; break;
					default: throw new Exception("Unknown Image Format!");
				}
				if (Image.Flag == 0) return _3DS.GPU.Textures.ToBitmap(Data, Image.Width, Image.Height, f3);
				return _3DS.GPU.Textures.ToBitmap(Data, Image.Height, Image.Width, f3);
			}
			else
			{
                GPU.Textures.ImageFormat fu;
                switch (Image.Format)
				{
                    case 0: fu = GPU.Textures.ImageFormat.L8; break;
                    case 1: fu = GPU.Textures.ImageFormat.A8; break;
                    case 2: fu = GPU.Textures.ImageFormat.LA4; break;
                    case 3: fu = GPU.Textures.ImageFormat.LA8; break;
                    case 4: fu = GPU.Textures.ImageFormat.HILO8; break;
                    case 5: fu = GPU.Textures.ImageFormat.RGB565; break;
                    case 6: fu = GPU.Textures.ImageFormat.RGB8; break;
                    case 7: fu = GPU.Textures.ImageFormat.RGBA5551; break;
                    case 8: fu = GPU.Textures.ImageFormat.RGBA4; break;
                    case 9: fu = GPU.Textures.ImageFormat.RGBA8; break;
                    case 10: fu = GPU.Textures.ImageFormat.ETC1; break;
                    case 11: fu = GPU.Textures.ImageFormat.ETC1A4; break;
                    case 12: fu = GPU.Textures.ImageFormat.BC1; break;
                    case 13: fu = GPU.Textures.ImageFormat.BC2; break;
                    case 14: fu = GPU.Textures.ImageFormat.BC3; break;
                    case 15: fu = GPU.Textures.ImageFormat.BC4L; break;
                    case 16: fu = GPU.Textures.ImageFormat.BC4A; break;
                    case 17: fu = GPU.Textures.ImageFormat.BC5; break;
                    case 18: fu = GPU.Textures.ImageFormat.L4; break;
                    case 19: fu = GPU.Textures.ImageFormat.A4; break;
                    case 20: fu = GPU.Textures.ImageFormat.RGBA8_sRGB; break;
                    case 21: fu = GPU.Textures.ImageFormat.BC1_sRGB; break;
                    case 22: fu = GPU.Textures.ImageFormat.BC2_sRGB; break;
                    case 23: fu = GPU.Textures.ImageFormat.BC3_sRGB; break;
                    case 24: fu = GPU.Textures.ImageFormat.RGBA1010102; break;
                    case 25: fu = GPU.Textures.ImageFormat.RGB555; break;
                    default: throw new Exception("Unknown Image Format!");
				}
				return GPU.Textures.ToBitmap(Data, Image.Width, Image.Height, fu, (GPU.Textures.TileMode)(Image.Flag & 0x1F), (uint)Image.Flag >> 5);
			}
		}

		public class FLIMIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Graphics;
			}

			public override string GetFileDescription()
			{
				return "Cafe Layout Images (FLIM)";
			}

			public override string GetFileFilter()
			{
				return "Cafe Layout Images (*.bflim)|*.bflim";
			}

			public override Bitmap GetIcon()
			{
				return Resource.image;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 0x28 && File.Data[File.Data.Length - 0x28] == 'F' && File.Data[File.Data.Length - 0x27] == 'L' && File.Data[File.Data.Length - 0x26] == 'I' && File.Data[File.Data.Length - 0x25] == 'M' && (IOUtil.ReadU32LE(File.Data, File.Data.Length - 0x4) == (File.Data.Length - 0x28) || IOUtil.ReadU32BE(File.Data, File.Data.Length - 0x4) == (File.Data.Length - 0x28))) return FormatMatch.Content;
				return FormatMatch.No;
			}
		}
	}
}
