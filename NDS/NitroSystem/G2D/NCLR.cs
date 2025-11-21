using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.IO;
using NDS.GPU;
using NDS.UI;

namespace NDS.NitroSystem.G2D
{
    public class NCLR : FileFormat<NCLR.NCLRIdentifier>, IViewable, IWriteable, IEmptyCreatable
    {
        public NCLR()
        {
            Header = new NCLRHeader();
            Palettedata = new PaletteData();
        }

        public NCLR(byte[] Data)
        {
            EndianBinaryReaderEx er = new EndianBinaryReaderEx(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new NCLRHeader(er);
                Palettedata = new PaletteData(er, Header);
                if (er.BaseStream.Position < er.BaseStream.Length)
                {
                    try
                    {
                        PalettecompressData = new PaletteCompressData(er);
                    }
                    catch
                    {
                        PalettecompressData = null;
                    }
                }
            }
            finally
            {
                er.Close();
            }
        }

        public Form GetDialog()
        {
            return new NCLRViewer(this);
        }

        public string GetSaveDefaultFileFilter()
        {
            return "Nitro Color Palette For Runtime (*.nclr)|*.nclr";
        }

        public byte[] Write()
        {
            MemoryStream m = new MemoryStream();
            EndianBinaryWriterEx er = new EndianBinaryWriterEx(m, Endianness.LittleEndian);
            Header.Write(er);
            Palettedata.Write(er);
            if (PalettecompressData != null)
            {
                PalettecompressData.Write(er);
            }
            er.BaseStream.Position = 8;
            er.Write((uint)er.BaseStream.Length);
            byte[] result = m.ToArray();
            er.Close();
            return result;
        }

        public NCLRHeader Header;
        public class NCLRHeader
        {
            public NCLRHeader()
            {
                Signature = "RLCN";
                Endianness = 0xFEFF;
                HeaderSize = 16;
                Constant = 256;
            }
            public NCLRHeader(EndianBinaryReaderEx er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "RLCN" && Signature != "RPCN") throw new SignatureNotCorrectException(Signature, "RLCN or RPCN", er.BaseStream.Position);
                Endianness = er.ReadUInt16();
                Constant = er.ReadUInt16();
                Filesize = er.ReadUInt32();
                HeaderSize = er.ReadUInt16();
                Section = er.ReadUInt16();
            }
            public void Write(EndianBinaryWriterEx er)
            {
                er.Write(Signature, Encoding.ASCII, false);
                er.Write(Endianness);
                er.Write(Constant);
                er.Write(Filesize);
                er.Write(HeaderSize);
                er.Write(Section);
            }
            public string Signature;
            public UInt16 Endianness;
            public UInt16 Constant;
            public UInt32 Filesize;
            public UInt16 HeaderSize;
            public UInt16 Section;
        }

        public PaletteData Palettedata;
        public class PaletteData
        {
            public PaletteData()
            {
                Signature = "TTLP";
                Format = Textures.ImageFormat.PLTT16;
                bExtendedPlt = false;
                szByte = 0;
                pRawData = 16;
                Data = new byte[0];
            }
            public PaletteData(EndianBinaryReaderEx er, NCLRHeader Header)
            {
                long startPos = er.BaseStream.Position;
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "TTLP") throw new SignatureNotCorrectException(Signature, "TTLP", er.BaseStream.Position);
                uint SectionSize = er.ReadUInt32();
                Format = (Textures.ImageFormat)er.ReadUInt32();
                bExtendedPlt = er.ReadUInt32() == 1;
                szByte = er.ReadUInt32();
                pRawData = er.ReadUInt32();
                int dataSize = (int)(SectionSize - 24);
                Data = er.ReadBytes(dataSize);
            }

            public void Write(EndianBinaryWriterEx er)
            {
                er.Write(Signature, Encoding.ASCII, false);
                er.Write((uint)(24 + Data.Length));
                er.Write((uint)Format);
                er.Write(bExtendedPlt ? 1 : 0);
                er.Write(szByte);
                er.Write(pRawData);
                er.Write(Data, 0, Data.Length);
            }
            public string Signature;
            public Textures.ImageFormat Format;
            public bool bExtendedPlt;
            public uint szByte;
            public uint pRawData;
            public byte[] Data;
        }

        public PaletteCompressData PalettecompressData;
        public class PaletteCompressData
        {
            public PaletteCompressData(EndianBinaryReaderEx er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "PMCP") throw new SignatureNotCorrectException(Signature, "PMCP", er.BaseStream.Position);
                numPalette = er.ReadUInt16();
                pad16 = er.ReadUInt16();
                pPlttIdxTbl = er.ReadUInt32();
                Data = new ushort[numPalette];
                for (int i = 0; i < numPalette; i++)
                {
                    Data[i] = er.ReadUInt16();
                }
            }

            public void Write(EndianBinaryWriterEx er)
            {
                er.Write(Signature, Encoding.ASCII, false);
                er.Write(numPalette);
                er.Write(pad16);
                er.Write(pPlttIdxTbl);
                for (int i = 0; i < numPalette; i++)
                {
                    er.Write(Data[i]);
                }
            }
            public string Signature;
            public ushort numPalette;
            public ushort pad16;
            public uint pPlttIdxTbl;
            public ushort[] Data;
        }

        public Color[] ToColorArray()
        {
            return GPU.Textures.ConvertXBGR1555(Palettedata.Data);
        }

        public class NCLRIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Palettes;
            }

            public override string GetFileDescription()
            {
                return "Nitro Color Palette For Runtime (NCLR)";
            }

            public override string GetFileFilter()
            {
                return "Nitro Color Palette For Runtime (*.nclr)|*.nclr";
            }

            public override Bitmap GetIcon()
            {
                return Resource.color_swatch1;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && ((File.Data[0] == 'R' && File.Data[1] == 'L' && File.Data[2] == 'C' && File.Data[3] == 'N') ||
                   (File.Data[0] == 'R' && File.Data[1] == 'P' && File.Data[2] == 'C' && File.Data[3] == 'N'))) return FormatMatch.Content;
                return FormatMatch.No;
            }
        }
    }
}