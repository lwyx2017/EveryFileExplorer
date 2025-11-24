using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.IO;
using NDS.UI;

namespace NDS
{
    public class BMG : FileFormat<BMG.BMGIdentifier>,IViewable//,IWriteable, IEmptyCreatable
    {
        public BMG()
        {
            Header = new BMGHeader();
            INF1 = new INF1Section();
            DAT1 = new DAT1Section();
        }
        public BMG(byte[] Data)
        {
            EndianBinaryReaderEx er = new EndianBinaryReaderEx(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new BMGHeader(er);
                INF1 = new INF1Section(er);
                DAT1 = new DAT1Section(er, INF1);
            }
            finally
            {
                er.Close();
            }
        }

        public Form GetDialog()
        {
            return new BMGViewer(this);
        }

        public string GetSaveDefaultFileFilter()
        {
            return "Nitro Binary Message Strings (*.bmg)|*.bmg";
        }

        public byte[] Write()
        {
            MemoryStream m = new MemoryStream();
            EndianBinaryWriterEx er = new EndianBinaryWriterEx(m, Endianness.LittleEndian);
            long headerPos = er.BaseStream.Position;
            Header.Write(er);
            long inf1Pos = er.BaseStream.Position;
            INF1.Write(er, new uint[INF1.NrOffset]);
            long dat1Pos = er.BaseStream.Position;
            DAT1.Write(er, INF1, out uint[] offsets);
            INF1.Offsets = offsets;
            INF1.NrOffset = (ushort)offsets.Length;
            er.BaseStream.Position = inf1Pos;
            INF1.Write(er, offsets);
            er.BaseStream.Position = headerPos + 8;
            er.Write((uint)er.BaseStream.Length);
            byte[] result = m.ToArray();
            er.Close();
            return result;
        }

        public BMGHeader Header;
        public class BMGHeader
        {
            public BMGHeader()
            {
                Signature = "MESGbmg1";
                NrSections = 2;
                Unknown1 = 2;
            }
            public BMGHeader(EndianBinaryReaderEx er)
            {
                Signature = er.ReadString(Encoding.ASCII, 8);
                if (Signature != "MESGbmg1") throw new SignatureNotCorrectException(Signature, "MESGbmg1", er.BaseStream.Position - 8);
                FileSize = er.ReadUInt32();
                NrSections = er.ReadUInt32();
                Unknown1 = er.ReadUInt32();
                Unknown2 = er.ReadUInt32();
                Unknown3 = er.ReadUInt32();
                Unknown4 = er.ReadUInt32();
            }
            public void Write(EndianBinaryWriterEx er)
            {
                er.Write(Signature, Encoding.ASCII, false);
                er.Write(FileSize);
                er.Write(NrSections);
                er.Write(Unknown1);
                er.Write(Unknown2);
                er.Write(Unknown3);
                er.Write(Unknown4);
            }
            public String Signature;
            public uint FileSize;
            public uint NrSections;
            public uint Unknown1;
            public uint Unknown2;
            public uint Unknown3;
            public uint Unknown4;
        }

        public INF1Section INF1;
        public class INF1Section
        {
            public INF1Section()
            {
                Signature = "INF1";
                Unknown1 = 4;
                NrOffset = 0;
                Offsets = new uint[0];
            }
            public INF1Section(EndianBinaryReaderEx er)
            {
                long startpos = er.BaseStream.Position;
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "INF1") throw new SignatureNotCorrectException(Signature, "INF1", er.BaseStream.Position - 4);
                uint SectionSize = er.ReadUInt32();
                NrOffset = er.ReadUInt16();
                Unknown1 = er.ReadUInt16();
                Unknown2 = er.ReadUInt32();
                Offsets = er.ReadUInt32s(NrOffset);
                long bytesRead = 16 + (NrOffset * 4);
                long remainingBytes = SectionSize - bytesRead;
                if (remainingBytes > 0)
                {
                    er.ReadBytes((int)remainingBytes);
                }
            }

            public void Write(EndianBinaryWriterEx er, uint[] Offsets)
            {
                er.Write(Signature, Encoding.ASCII, false);
                long sizePos = er.BaseStream.Position;
                er.Write(0u);
                er.Write(NrOffset);
                er.Write(Unknown1);
                er.Write(Unknown2);
                if (Offsets != null && NrOffset > 0)
                {
                    er.Write(Offsets, 0, NrOffset);
                }
                while (er.BaseStream.Position % 4 != 0)
                er.Write((byte)0);
                long endPos = er.BaseStream.Position;
                uint sectionSize = (uint)(endPos - sizePos - 4);
                er.BaseStream.Position = sizePos;
                er.Write(sectionSize);
                er.BaseStream.Position = endPos;
            }
            public String Signature;
            public ushort NrOffset;
            public ushort Unknown1;
            public uint Unknown2;
            public uint[] Offsets;
        }

        public DAT1Section DAT1;
        public class DAT1Section
        {
            public DAT1Section()
            {
                Signature = "DAT1";
                Strings = new string[0];
            }
            public DAT1Section(EndianBinaryReaderEx er, INF1Section INF1)
            {
                long startpos = er.BaseStream.Position;
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "DAT1") throw new SignatureNotCorrectException(Signature, "DAT1", er.BaseStream.Position - 4);
                uint SectionSize = er.ReadUInt32();
                Strings = new string[INF1.NrOffset];
                long datStart = er.BaseStream.Position;
                for (int i = 0; i < INF1.NrOffset; i++)
                {
                    if (INF1.Offsets[i] == 0xFFFFFFFF)
                    {
                        Strings[i] = "";
                        continue;
                    }
                    er.BaseStream.Position = datStart + INF1.Offsets[i];
                    StringBuilder sb = new StringBuilder();
                    while (true)
                    {
                        char c = er.ReadChar(Encoding.Unicode);
                        if (c == '\0') break;
                        if (c == '\u001a')
                        {
                            sb.Append("[#");
                            int dataLength = er.ReadByte() - 2;
                            er.BaseStream.Position--;
                            byte[] data = er.ReadBytes(dataLength);
                            sb.Append(BitConverter.ToString(data).Replace("-", ""));
                            sb.Append("]");
                        }
                        else if (c == '\n')
                        {
                            sb.Append("\r\n");
                        }
                        else
                        {
                            sb.Append(c);
                        }
                    }

                    Strings[i] = sb.ToString();
                }
                while (er.BaseStream.Position % 4 != 0)
                    er.ReadByte();
            }

            public void Write(EndianBinaryWriterEx er, INF1Section INF1, out uint[] Offsets)
            {
                er.Write(Signature, Encoding.ASCII, false);
                long sizePos = er.BaseStream.Position;
                er.Write(0);
                long dataStart = er.BaseStream.Position;
                Offsets = new uint[INF1.NrOffset];

                for (int i = 0; i < INF1.NrOffset; i++)
                {
                    if (string.IsNullOrEmpty(Strings[i]))
                    {
                        Offsets[i] = 0xFFFFFFFF;
                        continue;
                    }

                    Offsets[i] = (uint)(er.BaseStream.Position - dataStart);
                    string text = Strings[i];

                    for (int j = 0; j < text.Length; j++)
                    {
                        if (text[j] == '\r' && j + 1 < text.Length && text[j + 1] == '\n')
                        {
                            er.Write('\n', Encoding.Unicode);
                            j++;
                        }
                        else if (text[j] == '[' && j + 2 < text.Length && text[j + 1] == '#')
                        {
                            j += 2;
                            StringBuilder hexData = new StringBuilder();
                            while (j < text.Length && text[j] != ']')
                            {
                                hexData.Append(text[j]);
                                j++;
                            }
                            string hexString = hexData.ToString();
                            if (hexString.Length % 2 != 0)
                            {
                                throw new FormatException($"Invalid hex string length: {hexString}");
                            }
                            byte[] data = new byte[hexString.Length / 2];
                            for (int k = 0; k < data.Length; k++)
                            {
                                data[k] = Convert.ToByte(hexString.Substring(k * 2, 2), 16);
                            }
                            er.Write('\u001a', Encoding.Unicode);
                            er.Write((byte)(data.Length + 2));
                            er.Write(data);
                        }
                        else
                        {
                            er.Write(text[j], Encoding.Unicode);
                        }
                    }
                    er.Write((ushort)0);
                    if (er.BaseStream.Position % 2 != 0)
                    {
                        er.Write((byte)0);
                    }
                }
                while (er.BaseStream.Position % 4 != 0)
                {
                    er.Write((byte)0);
                }
                long endPos = er.BaseStream.Position;
                uint sectionSize = (uint)(endPos - sizePos - 4);
                er.BaseStream.Position = sizePos;
                er.Write(sectionSize);
                er.BaseStream.Position = endPos;
            }
            public String Signature;
            public string[] Strings;

            public byte[] ToTxt()
            {
                MemoryStream memoryStream = new MemoryStream();
                StreamWriter textWriter = new StreamWriter(memoryStream, Encoding.Unicode);

                for (int i = 0; i < Strings.Length; i++)
                {
                    textWriter.Write(Strings[i]);
                    if (i < Strings.Length - 1)
                    {
                        textWriter.Write("/p");
                    }
                }

                textWriter.Flush();
                byte[] result = memoryStream.ToArray();
                textWriter.Close();
                memoryStream.Close();
                return result;
            }

            public void FromTxt(byte[] file)
            {
                using (MemoryStream stream = new MemoryStream(file))
                using (StreamReader textReader = new StreamReader(stream, Encoding.Unicode))
                {
                    string content = textReader.ReadToEnd();
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        Strings = new string[0];
                    }
                    else
                    {
                        Strings = content.Split(new string[] { "/p" }, StringSplitOptions.None);
                    }
                }
            }
        }

        public class BMGIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Strings;
            }

            public override string GetFileDescription()
            {
                return "Nitro Binary Message Strings (BMG)";
            }

            public override string GetFileFilter()
            {
                return "Nitro Binary Message Strings (*.bmg)|*.bmg";
            }

            public override Bitmap GetIcon()
            {
                return Resource.script_text;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 8 && File.Data[0] == 'M' && File.Data[1] == 'E' && File.Data[2] == 'S' && File.Data[3] == 'G' && File.Data[4] == 'b' && File.Data[5] == 'm' && File.Data[6] == 'g' && File.Data[7] == '1') return FormatMatch.Content;
                return FormatMatch.No;
            }
        }
    }
}