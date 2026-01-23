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
    public class NCER : FileFormat<NCER.NCERIdentifier>//, IViewable, IWriteable
    {
        public NCER(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new NCERHeader(er);
                CellDataBank = new cellDataBank(er);
                if (Header.NrBlocks > 1)
                {
                    er.BaseStream.Position = CellDataBank.SectionSize + Header.HeaderSize;
                    LabelBlock = new labelBlock(er,CellDataBank.numCells);
                }
            }
            finally
            {
                er.Close();
            }
        }

        public string GetSaveDefaultFileFilter()
        {
            return "Nitro Cells For Runtime (*.ncer)|*.ncer";
        }

        public byte[] Write()
        {
            MemoryStream m = new MemoryStream();
            EndianBinaryWriter er = new EndianBinaryWriter(m, Endianness.LittleEndian);
            Header.Write(er);
            CellDataBank.Write(er);
            if (LabelBlock != null)
            {
                LabelBlock.Write(er);
            }
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

        public NCERHeader Header;
        public class NCERHeader
        {
            public NCERHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "RECN") throw new SignatureNotCorrectException(Signature, "RECN", er.BaseStream.Position - 4);
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

        public cellDataBank CellDataBank;
        public class cellDataBank
        {
            public cellDataBank(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "KBEC") throw new SignatureNotCorrectException(Signature, "KBEC", er.BaseStream.Position - 4);
                SectionSize = er.ReadUInt32();
                numCells = er.ReadUInt16();
                cellBankAttr = er.ReadUInt16();
                pCellDataArrayHead = er.ReadUInt32();
                mappingMode = (Textures.CharacterDataMapingType)er.ReadUInt32();
                pVramTransferData = er.ReadUInt32();
                pStringBank = er.ReadUInt32();
                pExtendedData = er.ReadUInt32();
                CellData = new cellData[numCells];
                for (int i = 0; i < numCells; i++)
                {
                    CellData[i] = new cellData(er, cellBankAttr, (int)(pCellDataArrayHead + 24) + numCells * ((cellBankAttr == 1) ? 16 : 8));
                }
            }
            public void Write(EndianBinaryWriter er)
            {
                er.Write(Signature, Encoding.ASCII, false);
                er.Write(SectionSize);
                er.Write(numCells);
                er.Write(cellBankAttr);
                er.Write(24);
                er.Write((uint)mappingMode);
                er.Write(pVramTransferData);
                er.Write(pStringBank);
                er.Write(pExtendedData);
                int Offset = 0;
                for (int i = 0; i < numCells; i++)
                {
                    CellData[i].Write(er, ref Offset);
                }
                for (int i = 0; i < numCells; i++)
                {
                    for (int j = 0; j < CellData[i].numOAMAttrs; j++)
                    {
                        CellData[i].CellOAMAttrData[j].Write(er);
                    }
                }
            }
            public string Signature;
            public uint SectionSize;
            public ushort numCells;
            public ushort cellBankAttr;
            public uint pCellDataArrayHead;
            public Textures.CharacterDataMapingType mappingMode;
            public uint pVramTransferData;
            public uint pStringBank;
            public uint pExtendedData;
            public cellData[] CellData;

            public class cellData
            {
                public cellData(EndianBinaryReader er, int Format, int offset)
                {
                    numOAMAttrs = er.ReadUInt16();
                    cellAttr = er.ReadUInt16();
                    pOamAttrArray = er.ReadUInt32();
                    if (Format == 1)
                    {
                        boundingRect = new CellBoundingRectS16(er);
                    }
                    long position = er.BaseStream.Position;
                    er.BaseStream.Position = pOamAttrArray + offset;
                    CellOAMAttrData = new cellOAMAttrData[numOAMAttrs];
                    for (int i = 0; i < numOAMAttrs; i++)
                    {
                        CellOAMAttrData[i] = new cellOAMAttrData(er);
                    }
                    er.BaseStream.Position = position;
                }
                public void Write(EndianBinaryWriter er, ref int Offset)
                {
                    er.Write(numOAMAttrs);
                    er.Write(cellAttr);
                    er.Write((uint)Offset);
                    Offset += 6 * numOAMAttrs;
                    if (boundingRect != null)
                    {
                        boundingRect.Write(er);
                    }
                }
                public ushort numOAMAttrs;
                public ushort cellAttr;
                public uint pOamAttrArray;
                public CellBoundingRectS16 boundingRect;
                public cellOAMAttrData[] CellOAMAttrData;

                public class CellBoundingRectS16
                {

                    public CellBoundingRectS16(EndianBinaryReader er)
                    {
                        maxX = er.ReadInt16();
                        maxY = er.ReadInt16();
                        minX = er.ReadInt16();
                        minY = er.ReadInt16();
                    }

                    public CellBoundingRectS16(short Width, short Height)
                    {
                        minX = (short)(-(Width / 2));
                        minY = (short)(-(Height / 2));
                        maxX = (short)(Width / 2);
                        maxY = (short)(Height / 2);
                    }

                    public void Write(EndianBinaryWriter er)
                    {
                        er.Write(maxX);
                        er.Write(maxY);
                        er.Write(minX);
                        er.Write(minY);
                    }
                    public short maxX;
                    public short maxY;
                    public short minX;
                    public short minY;


                }
                public class cellOAMAttrData
                {
                    public cellOAMAttrData(EndianBinaryReader er)
                    {
                        attr0 = er.ReadUInt16();
                        attr1 = er.ReadUInt16();
                        attr2 = er.ReadUInt16();
                        YCoord = (byte)(attr0 & 0xFF);
                        AffineTransformation = ((attr0 >> 8) & 1) == 1;
                        DoubleSizeAffineTransformation = ((attr0 >> 9) & 1) == 1;
                        OBJMode = (byte)((attr0 >> 10) & 3);
                        Mosaic = ((attr0 >> 12) & 1) == 1;
                        ColorMode = ((attr0 >> 13) & 1) == 1;
                        OBJShape = (byte)((attr0 >> 14) & 3);
                        XCoord = (short)(attr1 & 0x1FF);
                        AffineTransformationParameterSelection = (byte)((attr1 >> 9) & 0x1F);
                        if (!AffineTransformation)
                        {
                            FlipX = ((AffineTransformationParameterSelection >> 3) & 1) == 1;
                            FlipY = ((AffineTransformationParameterSelection >> 4) & 1) == 1;
                        }
                        OBJSize = (byte)((attr1 >> 14) & 3);
                        StartingCharacterName = (short)(attr2 & 0x3FF);
                        DisplayPriority = (byte)((attr2 >> 10) & 3);
                        ColorParameter = (byte)((attr2 >> 12) & 0xF);
                    }

                    public cellOAMAttrData(short XCoord, sbyte YCoord, byte OBJSize, byte OBJShape, byte ColorParameter)
                    {
                        this.XCoord = XCoord;
                        this.YCoord = (byte)YCoord;
                        if (YCoord < 0)
                        {
                            this.YCoord |= 128;
                        }
                        this.OBJShape = OBJShape;
                        this.OBJSize = OBJSize;
                        this.ColorParameter = ColorParameter;
                        AffineTransformation = false;
                        DoubleSizeAffineTransformation = false;
                        Mosaic = false;
                        ColorMode = false;
                        FlipX = false;
                        FlipY = false;
                        DisplayPriority = 1;
                        AffineTransformationParameterSelection = 0;
                        OBJMode = 0;
                    }

                    public Size GetSize()
                    {
                        switch (OBJShape)
                        {
                            case 0:
                                switch (OBJSize)
                                {
                                    case 0:
                                        return new Size(8, 8);
                                    case 1:
                                        return new Size(16, 16);
                                    case 2:
                                        return new Size(32, 32);
                                    case 3:
                                        return new Size(64, 64);
                                }
                                break;
                            case 1:
                                switch (OBJSize)
                                {
                                    case 0:
                                        return new Size(16, 8);
                                    case 1:
                                        return new Size(32, 8);
                                    case 2:
                                        return new Size(32, 16);
                                    case 3:
                                        return new Size(64, 32);
                                }
                                break;
                            case 2:
                                switch (OBJSize)
                                {
                                    case 0:
                                        return new Size(8, 16);
                                    case 1:
                                        return new Size(8, 32);
                                    case 2:
                                        return new Size(16, 32);
                                    case 3:
                                        return new Size(32, 64);
                                }
                                break;
                            case 3:
                                throw new NotSupportedException("Prohibited Cell setting");
                        }
                        return new Size(0, 0);
                    }

                    public void Write(EndianBinaryWriter er)
                    {
                        attr0 = 0;
                        attr0 |= (ushort)(OBJShape << 14);
                        attr0 |= (ushort)((ColorMode ? 1 : 0) << 13);
                        attr0 |= (ushort)((Mosaic ? 1 : 0) << 12);
                        attr0 |= (ushort)(OBJMode << 10);
                        attr0 |= (ushort)((DoubleSizeAffineTransformation ? 1 : 0) << 9);
                        attr0 |= (ushort)((AffineTransformation ? 1 : 0) << 8);
                        attr0 |= YCoord;
                        er.Write(attr0);
                        attr1 = 0;
                        attr1 |= (ushort)(OBJSize << 14);
                        if (!AffineTransformation)
                        {
                            AffineTransformationParameterSelection = 0;
                            AffineTransformationParameterSelection |= (byte)((FlipY ? 1 : 0) << 4);
                            AffineTransformationParameterSelection |= (byte)((FlipX ? 1 : 0) << 3);
                        }
                        attr1 |= (ushort)(AffineTransformationParameterSelection << 9);
                        attr1 |= (ushort)(XCoord & 0x1FF);
                        er.Write(attr1);
                        attr2 = 0;
                        attr2 |= (ushort)(ColorParameter << 12);
                        attr2 |= (ushort)(DisplayPriority << 10);
                        attr2 |= (ushort)StartingCharacterName;
                        er.Write(attr2);
                    }
                    public ushort attr0;
                    public byte YCoord;
                    public bool AffineTransformation;
                    public bool DoubleSizeAffineTransformation;
                    public byte OBJMode;
                    public bool Mosaic;
                    public bool ColorMode;
                    public byte OBJShape;
                    public ushort attr1;
                    public short XCoord;
                    public byte AffineTransformationParameterSelection;
                    public bool FlipY = false;
                    public bool FlipX = false;
                    public byte OBJSize;
                    public ushort attr2;
                    public short StartingCharacterName;
                    public byte DisplayPriority;
                    public byte ColorParameter;
                }
            }

            public Bitmap GetBitmap(int Index, NCGR Graphic, NCLR Palette)
            {
                int width = 0;
                int height = 0;
                switch (mappingMode)
                {
                    case Textures.CharacterDataMapingType.CHARACTERMAPING_1D_32:
                        width = 32;
                        height = Graphic.CharacterData.Data.Length * ((Graphic.CharacterData.pixelFmt != Textures.ImageFormat.PLTT16) ? 1 : 2) / 32;
                        break;
                    case Textures.CharacterDataMapingType.CHARACTERMAPING_1D_64:
                        width = 64;
                        height = Graphic.CharacterData.Data.Length * ((Graphic.CharacterData.pixelFmt != Textures.ImageFormat.PLTT16) ? 1 : 2) / 64;
                        break;
                    case Textures.CharacterDataMapingType.CHARACTERMAPING_1D_128:
                        width = 128;
                        height = Graphic.CharacterData.Data.Length * ((Graphic.CharacterData.pixelFmt != Textures.ImageFormat.PLTT16) ? 1 : 2) / 128;
                        break;
                    case Textures.CharacterDataMapingType.CHARACTERMAPING_1D_256:
                        width = 256;
                        height = Graphic.CharacterData.Data.Length * ((Graphic.CharacterData.pixelFmt != Textures.ImageFormat.PLTT16) ? 1 : 2) / 256;
                        break;
                    case Textures.CharacterDataMapingType.CHARACTERMAPING_2D:
                        width = Graphic.CharacterData.W * 8;
                        height = Graphic.CharacterData.H * 8;
                        break;
                }
                return Textures.ToBitmap(Graphic.CharacterData.Data, Palette.Palettedata.Data, width, height, CellData[Index], mappingMode, Graphic.CharacterData.pixelFmt, Textures.CharFormat.CHAR);
            }
        }

        public labelBlock LabelBlock;
        public class labelBlock
        {
            public labelBlock(EndianBinaryReader er, int NrCells)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "LBAL") throw new SignatureNotCorrectException(Signature, "LBAL", er.BaseStream.Position - 4);
                SectionSize = er.ReadUInt32();
                Offsets = new uint[NrCells];
                int num = NrCells;
                for (int i = 0; i < NrCells; i++)
                {
                    Offsets[i] = er.ReadUInt32();
                    if (Offsets[i] > SectionSize)
                    {
                        num = i;
                        Array.Resize(ref Offsets, i);
                        er.BaseStream.Position -= 4L;
                        break;
                    }
                }
                Names = new string[num];
                long position = er.BaseStream.Position;
                for (int i = 0; i < num; i++)
                {
                    er.BaseStream.Position = position + Offsets[i];
                    Names[i] = er.ReadStringNT(Encoding.ASCII);
                }
            }
            public void Write(EndianBinaryWriter er)
            {
                long startPos = er.BaseStream.Position;

                er.Write(Signature, Encoding.ASCII, false);
                uint sectionSizePos = (uint)er.BaseStream.Position;
                er.Write((uint)0);
                uint[] offsets = new uint[Names.Length];
                uint currentOffset = (uint)(4 + 4 + 4 * Names.Length);
                for (int i = 0; i < Names.Length; i++)
                {
                    offsets[i] = currentOffset;
                    er.Write(offsets[i]);
                    currentOffset += (uint)(Encoding.ASCII.GetByteCount(Names[i]) + 1);
                }
                for (int i = 0; i < Names.Length; i++)
                {
                    er.Write(Names[i], Encoding.ASCII, true);
                }
                long endPos = er.BaseStream.Position;
                uint sectionSize = (uint)(endPos - startPos);
                er.BaseStream.Position = sectionSizePos;
                er.Write(sectionSize);
                er.BaseStream.Position = endPos;
            }
            public string Signature;
            public uint SectionSize;
            public uint[] Offsets;
            public string[] Names;
        }

        public class NCERIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Cells;
            }

            public override string GetFileDescription()
            {
                return "Nitro Cells For Runtime (NCER)";
            }

            public override string GetFileFilter()
            {
                return "Nitro Cells For Runtime (*.ncer)|*.ncer";
            }

            public override Bitmap GetIcon()
            {
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'R' && File.Data[1] == 'E' && File.Data[2] == 'C' && File.Data[3] == 'N') return FormatMatch.Content;
                return FormatMatch.No;
            }
        }
    }
}