using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.IO;
using NDS.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace NDS
{
    public class BMG : FileFormat<BMG.BMGIdentifier>, IViewable, IWriteable, IEmptyCreatable
    {
        public BMG()
        {
            Header = new BMGHeader();
            INF1 = new INF1Section();
            DAT1 = new DAT1Section();
        }
        public BMG(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
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
            EndianBinaryWriter er = new EndianBinaryWriter(m, Endianness.LittleEndian);
            Header.Write(er);
            long position = INF1.Write(er);
            uint[] value = DAT1.Write(er);
            er.BaseStream.Position = position;
            er.Write(value, 0, INF1.NrOffset);
            er.BaseStream.Position = 8;
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
            public BMGHeader(EndianBinaryReader er)
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
            public void Write(EndianBinaryWriter er)
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
                Unknown1 = 0;
                NrOffset = 0;
                Offsets = new uint[0];
                SectionSize = 0;
            }

            public INF1Section(EndianBinaryReader er)
            {
                long startpos = er.BaseStream.Position;
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "INF1") throw new SignatureNotCorrectException(Signature, "INF1", er.BaseStream.Position - 4);
                SectionSize = er.ReadUInt32();
                NrOffset = er.ReadUInt16();
                Unknown1 = er.ReadUInt16();
                Unknown2 = er.ReadUInt32();
                Offsets = er.ReadUInt32s(NrOffset);
                er.ReadBytes((int)(SectionSize - NrOffset * 4 - 16));
            }

            public long Write(EndianBinaryWriter er)
            {
                long position = er.BaseStream.Position;
                er.Write(Signature, Encoding.ASCII, false);
                er.Write(SectionSize);
                er.Write(NrOffset);
                er.Write(Unknown1);
                er.Write(Unknown2);
                long position2 = er.BaseStream.Position;
                er.Write(new uint[NrOffset], 0, NrOffset);
                while (er.BaseStream.Position % 16 != 0)
                {
                    er.Write((byte)0);
                }
                long position3 = er.BaseStream.Position;
                er.BaseStream.Position = position + 4;
                er.Write((uint)(position3 - position));
                er.BaseStream.Position = position3;
                return position2;
            }

            public String Signature;
            public uint SectionSize;
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
            public DAT1Section(EndianBinaryReader er, INF1Section INF1)
            {
                long startpos = er.BaseStream.Position;
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "DAT1") throw new SignatureNotCorrectException(Signature, "DAT1", er.BaseStream.Position - 4);
                SectionSize = er.ReadUInt32();
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

            public uint[] Write(EndianBinaryWriter er)
            {
                List<uint> list = new List<uint>();
                long num = er.BaseStream.Position + 4;
                er.Write(Signature, Encoding.ASCII, false);
                er.Write(SectionSize);
                long position = er.BaseStream.Position;
                er.Write((ushort)0);
                string[] strings = Strings;
                foreach (string text in strings)
                {
                    list.Add((uint)(er.BaseStream.Position - position));
                    for (int j = 0; j < text.Length; j++)
                    {
                        if (text[j] == '\r')
                        {
                            er.Write('\n', Encoding.Unicode);
                            j++;
                        }
                        else if (j != text.Length - 1 && text[j] == '[' && text[j + 1] == '#')
                        {
                            er.Write('\u001a', Encoding.Unicode);
                            j += 2;
                            while (true)
                            {
                                string text2 = "";
                                char c = text[j++];
                                if (c == ']')
                                {
                                    break;
                                }
                                text2 += c;
                                text2 += text[j++];
                                er.Write(IOUtil.StringToByte(text2)[0]);
                            }
                            j--;
                        }
                        else
                        {
                            er.Write(text[j], Encoding.Unicode);
                        }
                    }
                    er.Write((byte)0);
                    er.Write((byte)0);
                }
                while (er.BaseStream.Position % 4 != 0)
                {
                    er.Write((byte)0);
                }
                long position2 = er.BaseStream.Position;
                er.BaseStream.Position = num;
                er.Write((uint)(position2 - num + 4));
                er.BaseStream.Position = position2;
                return list.ToArray();
            }
            public String Signature;
            public uint SectionSize;
            public string[] Strings;

            public byte[] ToTxt()
            {
                MemoryStream m = new MemoryStream();
                StreamWriter textWriter = new StreamWriter(m, Encoding.Unicode);

                for (int i = 0; i < Strings.Length; i++)
                {
                    textWriter.Write(Strings[i]);
                    if (i < Strings.Length - 1)
                    {
                        textWriter.Write("/p");
                    }
                }

                textWriter.Flush();
                byte[] result = m.ToArray();
                textWriter.Close();
                m.Close();
                return result;
            }

            public void FromTxt(byte[] file)
            {
                using (MemoryStream m = new MemoryStream(file))
                using (StreamReader textReader = new StreamReader(m, Encoding.Unicode))
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