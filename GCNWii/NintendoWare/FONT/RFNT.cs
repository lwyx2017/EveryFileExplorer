using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GCNWii.GPU;
using GCNWii.UI;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.GFX;
using LibEveryFileExplorer.IO;

namespace GCNWii.NintendoWare.LYT
{
    public class RFNT : FileFormat<RFNT.RFNTIdentifier>, IViewable
    {
        public RFNT(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.BigEndian);
            try
            {
                Header = new RFNTHeader(er);
                FINF = new FINFSection(er);
                er.BaseStream.Position = FINF.TGLPOffset - 8;
                TGLP = new TGLPSection(er);
                er.BaseStream.Position = FINF.CWDHOffset - 8;
                CWDH = new CWDHSection(er);
                List<CMAPSection> cmap = new List<CMAPSection>();
                int offset = (int)FINF.CMAPOffset;
                do
                {
                    er.BaseStream.Position = offset - 8;
                    var section = new CMAPSection(er);
                    cmap.Add(section);
                    offset = (int)section.NextCMAPOffset;
                }
                while (offset != 0);
                CMAP = cmap.ToArray();
            }
            finally
            {
                er.Close();
            }
        }

        public Form GetDialog()
        {
            return new RFNTViewer(this);;
        }

        public RFNTHeader Header;
        public class RFNTHeader
        {
            public RFNTHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "RFNT") throw new SignatureNotCorrectException(Signature, "RFNT", er.BaseStream.Position - 4);
                Magic = er.ReadUInt32();
                FileSize = er.ReadUInt32();
                HeaderSize = er.ReadUInt16();
                Unknown = er.ReadUInt16();
            }
            public string Signature;
            public UInt32 Magic;
            public UInt32 FileSize;
            public UInt16 HeaderSize;
            public UInt16 Unknown;
        }

        public FINFSection FINF;
        public class FINFSection
        {
            public FINFSection(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "FINF") throw new SignatureNotCorrectException(Signature, "FINF", er.BaseStream.Position - 4);
                SectionSize = er.ReadUInt32();
                Unknown1 = er.ReadByte();
                Unknown2 = er.ReadByte();
                Unknown3 = er.ReadUInt16();
                Unknown4 = er.ReadByte();
                Unknown5 = er.ReadByte();
                Unknown6 = er.ReadByte();
                Unknown7 = er.ReadByte();
                TGLPOffset = er.ReadUInt32();
                CWDHOffset = er.ReadUInt32();
                CMAPOffset = er.ReadUInt32();
            }
            public String Signature;
            public UInt32 SectionSize;
            public Byte Unknown1;
            public Byte Unknown2;
            public UInt16 Unknown3;
            public Byte Unknown4;
            public Byte Unknown5;
            public Byte Unknown6;
            public Byte Unknown7;
            public UInt32 TGLPOffset;
            public UInt32 CWDHOffset;
            public UInt32 CMAPOffset;
        }

        public TGLPSection TGLP;
        public class TGLPSection
        {
            public TGLPSection(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "TGLP") throw new SignatureNotCorrectException(Signature, "TGLP", er.BaseStream.Position - 4);
                SectionSize = er.ReadUInt32();
                FontWidth = (byte)(er.ReadByte() - 1);
                FontHeight = (byte)(er.ReadByte() - 1);
                CharWidth = (byte)(er.ReadByte() - 1);
                CharHeight = (byte)(er.ReadByte() - 1);
                ImageLength = er.ReadUInt32();
                NrImages = er.ReadUInt16();
                Unknown1 = er.ReadByte();
                ImageFormat = (Textures.ImageFormat)er.ReadByte();
                CharsARow = er.ReadUInt16();
                CharsAColumn = er.ReadUInt16();
                ImageWidth = er.ReadUInt16();
                ImageHeight = er.ReadUInt16();
                ImageOffset = er.ReadUInt32();
                er.BaseStream.Position = ImageOffset;
                Images = new Bitmap[NrImages];
                for (int i = 0; i < NrImages; i++)
                {
                    byte[] Texturedata = er.ReadBytes((int)ImageLength);
                    Images[i] = Textures.ToBitmap(TexData: Texturedata, TexOffset: 0, PalData: null, PalOffset: 0, Width: ImageWidth, Height: ImageHeight, Format: ImageFormat, PalFormat: 0);
                }
            }
            public String Signature;
            public UInt32 SectionSize;
            public Byte FontWidth;
            public Byte FontHeight;
            public Byte CharWidth;
            public Byte CharHeight;
            public UInt32 ImageLength;
            public UInt16 NrImages;
            public Byte Unknown1;
            public Textures.ImageFormat ImageFormat;
            public UInt16 CharsARow;
            public UInt16 CharsAColumn;
            public UInt16 ImageWidth;
            public UInt16 ImageHeight;
            public UInt32 ImageOffset;
            public Bitmap[] Images;
        }

        public CWDHSection CWDH;
        public class CWDHSection
        {
            public CWDHSection(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "CWDH") throw new SignatureNotCorrectException(Signature, "CWDH", er.BaseStream.Position - 4);
                SectionSize = er.ReadUInt32();
                NrChars = er.ReadUInt32();
                FirstChar = (Char)er.ReadUInt32();
                Charspacings = new CharSpacing[NrChars];
                for (int i = 0; i < NrChars; i++)
                {
                    Charspacings[i] = new CharSpacing(er);
                }
            }
            public String Signature;
            public UInt32 SectionSize;
            public UInt32 NrChars;
            public Char FirstChar;
            public CharSpacing[] Charspacings;

            public class CharSpacing
            {
                public CharSpacing(EndianBinaryReader er)
                {
                    Unknown1 = er.ReadByte();
                    Unknown2 = er.ReadByte();
                    Unknown3 = er.ReadByte();
                }
                public byte Unknown1;
                public byte Unknown2;
                public byte Unknown3;
            }
        }

        public CMAPSection[] CMAP;
        public class CMAPSection
        {
            public CMAPSection(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "CMAP") throw new SignatureNotCorrectException(Signature, "CMAP", er.BaseStream.Position - 4);
                SectionSize = er.ReadUInt32();
                FirstChar = er.ReadChar(Encoding.Unicode);
                LastChar = er.ReadChar(Encoding.Unicode);
                Unknown1 = er.ReadUInt32();
                NextCMAPOffset = er.ReadUInt32();
                CharIndex = new Dictionary<char, ushort>();
                if (FirstChar == 0 && LastChar == 0xFFFF)
                {
                    NrChar = er.ReadUInt16();
                    for (int i = 0; i < NrChar; i++)
                    {
                        char c = er.ReadChar(Encoding.Unicode);
                        ushort index = er.ReadUInt16();
                        if (!CharIndex.ContainsKey(c))
                            CharIndex.Add(c, index);
                    }
                }
                else
                {
                    int charCount = LastChar - FirstChar + 1;
                    long savedPos = er.BaseStream.Position;
                    uint possibleFlag = er.ReadUInt32();

                    if (possibleFlag == 0)
                    {
                        for (int i = 0; i < charCount; i++)
                        {
                            CharIndex.Add((char)(FirstChar + i), (ushort)i);
                        }
                    }
                    else
                    {
                        er.BaseStream.Position = savedPos;

                        for (int i = 0; i < charCount; i++)
                        {
                            ushort idx = er.ReadUInt16();
                            if (idx != 0xFFFF)
                            {
                                try
                                {
                                    CharIndex.Add((char)(FirstChar + i), idx);
                                }
                                catch (ArgumentException)
                                {

                                }
                            }
                        }
                    }
                }
            }
            public String Signature;
            public UInt32 SectionSize;
            public Char FirstChar;
            public Char LastChar;
            public UInt32 Unknown1;
            public UInt32 NextCMAPOffset;
            public UInt16 NrChar;
            public Dictionary<Char, UInt16> CharIndex;
        }

        public BitmapFont GetBitmapFont()
        {
            BitmapFont font = new BitmapFont();
            font.LineHeight = (int)TGLP.CharHeight + 1;
            float realCellWidth = TGLP.FontWidth + 1 + 2;
            float realCellHeight = TGLP.FontHeight + 1 + 2;
            int totalChars = TGLP.CharsARow * TGLP.CharsAColumn * TGLP.NrImages;
            Bitmap[] charBitmaps = new Bitmap[totalChars];
            for (int sheet = 0; sheet < TGLP.NrImages; sheet++)
            {
                Bitmap sheetBitmap = TGLP.Images[sheet];

                for (int y = 0; y < TGLP.CharsAColumn; y++)
                {
                    for (int x = 0; x < TGLP.CharsARow; x++)
                    {
                        int charIndex = (sheet * TGLP.CharsARow * TGLP.CharsAColumn) +
                                       (y * TGLP.CharsARow) + x;

                        if (charIndex >= totalChars) continue;
                        Bitmap charBitmap = new Bitmap(TGLP.FontWidth + 1, TGLP.FontHeight + 1);
                        using (Graphics g = Graphics.FromImage(charBitmap))
                        {
                            int srcX = (int)(x * realCellWidth) + 1;
                            int srcY = (int)(y * realCellHeight) + 1;

                            Rectangle srcRect = new Rectangle(
                                srcX, srcY,
                                TGLP.FontWidth + 1,
                                TGLP.FontHeight + 1);

                            Rectangle destRect = new Rectangle(0, 0,
                                TGLP.FontWidth + 1,
                                TGLP.FontHeight + 1);

                            g.DrawImage(sheetBitmap, destRect, srcRect, GraphicsUnit.Pixel);
                        }

                        charBitmaps[charIndex] = charBitmap;
                    }
                }
            }

            foreach (var cmap in CMAP)
            {
                foreach (var kvp in cmap.CharIndex)
                {
                    char character = kvp.Key;
                    ushort charIndex = kvp.Value;
                    if (charIndex >= totalChars || charIndex >= CWDH.Charspacings.Length)
                    {
                        continue;
                    }
                    var spacing = CWDH.Charspacings[charIndex];
                    font.Characters.Add(character, new BitmapFont.Character(
                        charBitmaps[charIndex],
                        (int)spacing.Unknown1,
                        (int)spacing.Unknown2,
                        (int)spacing.Unknown3
                    ));
                }
            }

            return font;
        }

        public class RFNTIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Fonts;
            }

            public override string GetFileDescription()
            {
                return "Binary Revolution Font (RFNT)";
            }

            public override string GetFileFilter()
            {
                return "Binary Revolution Font (*.brfnt)|*.brfnt";
            }

            public override Bitmap GetIcon()
            {
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 0x10 && File.Data[0] == 'R' && File.Data[1] == 'F' && File.Data[2] == 'N' && File.Data[3] == 'T') return FormatMatch.Content;
                return FormatMatch.No;
            }

        }
    }
}