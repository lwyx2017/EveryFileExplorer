using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.Files.SimpleFileSystem;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.IO.Serialization;
using NDS.UI;

namespace NDS.NitroSystem.SND
{
    public class SDAT : FileFormat<SDAT.SDATIdentifier>, IViewable//, IWriteable
    {
        public SDAT(byte[] Data)
        {
            EndianBinaryReaderEx er = new EndianBinaryReaderEx(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new SDATHeader(er);
                if (Header.SYMBOffset != 0 && Header.SYMBLength != 0)
                {
                    er.BaseStream.Position = Header.SYMBOffset;
                    SymbolBlock = new SYMB(er);
                }
                er.BaseStream.Position = Header.INFOOffset;
                InfoBlock = new INFO(er);
                er.BaseStream.Position = Header.FATOffset;
                FileAllocationTable = new FAT(er);
                er.BaseStream.Position = Header.FILEOffset;
                File = new FILE(er);
            }
            finally
            {
                er.Close();
            }
        }

        public Form GetDialog()
        {
            return new SDATViewer(this);
        }

        public string GetSaveDefaultFileFilter()
        {
            return "Nitro Sound Data (*.sdat)|*.sdat";
        }

        public byte[] Write()
        {
            MemoryStream m = new MemoryStream();
            EndianBinaryWriter er = new EndianBinaryWriter(m, Endianness.LittleEndian);
            Header.Write(er);
            er.Write(0);
            er.Write(0);
            er.Write(0);
            er.Write(0);
            er.Write(0);
            er.Write(0);
            er.Write(0);
            er.Write(0);
            er.Write(new byte[16], 0, 16);
            long position;
            if (SymbolBlock != null)
            {
                position = er.BaseStream.Position;
                er.BaseStream.Position = 16;
                er.Write((uint)position);
                er.BaseStream.Position = position;
                SymbolBlock.Write(er);
            }
            position = er.BaseStream.Position;
            er.BaseStream.Position = 24;
            er.Write((uint)position);
            er.BaseStream.Position = position;
            InfoBlock.Write(er);
            long position2 = er.BaseStream.Position;
            er.BaseStream.Position = 28;
            er.Write((uint)(position2 - position));
            er.BaseStream.Position = position2;
            position = er.BaseStream.Position;
            er.BaseStream.Position = 32;
            er.Write((uint)position);
            er.BaseStream.Position = position;
            FileAllocationTable.Write(er);
            position2 = er.BaseStream.Position;
            er.BaseStream.Position = 36;
            er.Write((uint)(position2 - position));
            er.BaseStream.Position = position2;
            position = er.BaseStream.Position;
            er.BaseStream.Position = 40;
            er.Write((uint)position);
            er.BaseStream.Position = position;
            File.Write(er);
            position2 = er.BaseStream.Position;
            er.BaseStream.Position = 44;
            er.Write((uint)(position2 - position));
            er.BaseStream.Position = position2;
            position = er.BaseStream.Position;
            er.BaseStream.Position = 8;
            er.Write((uint)position);
            byte[] result = m.ToArray();
            er.Close();
            return result;
        }

        public SDATHeader Header;
        public class SDATHeader
        {
            public SDATHeader(EndianBinaryReaderEx er)
            {
                er.ReadObject(this);
            }
            public void Write(EndianBinaryWriter er)
            {
                er.Write(Signature, Encoding.ASCII, false);
                er.Write(Endianness);
                er.Write(Version);
                er.Write(FileSize);
                er.Write(HeaderSize);
                er.Write(NrBlocks);
                er.Write(SYMBOffset);
                er.Write(SYMBLength);
                er.Write(INFOOffset);
                er.Write(INFOLength);
                er.Write(FATOffset);
                er.Write(FATLength);
                er.Write(FILEOffset);
                er.Write(FILELength);
                er.Write(Padding, 0, 16);
            }
            [BinaryStringSignature("SDAT")]
            [BinaryFixedSize(4)]
            public String Signature;
            [BinaryBOM(0xFFFE)]
            public UInt16 Endianness;
            public UInt16 Version;
            public UInt32 FileSize;
            public UInt16 HeaderSize;
            public UInt16 NrBlocks;
            public UInt32 SYMBOffset;
            public UInt32 SYMBLength;
            public UInt32 INFOOffset;
            public UInt32 INFOLength;
            public UInt32 FATOffset;
            public UInt32 FATLength;
            public UInt32 FILEOffset;
            public UInt32 FILELength;
            [BinaryFixedSize(16)]
            public byte[] Padding;
        }

        public SYMB SymbolBlock;
        public class SYMB
        {
            public SYMB(EndianBinaryReader er)
            {
                long baseoffset = er.BaseStream.Position;
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "SYMB") throw new SignatureNotCorrectException(Signature, "SYMB", er.BaseStream.Position - 4);
                SectionSize = er.ReadUInt32();
                RecordOffsets = er.ReadUInt32s(8);
                Padding = er.ReadBytes(24);
                long curpos = er.BaseStream.Position;
                er.BaseStream.Position = baseoffset + RecordOffsets[0];
                SEQRecord = new SymbolRecord(er);
                er.BaseStream.Position = baseoffset + RecordOffsets[1];
                SEQARCRecord = new ArchiveSymbolRecord(er);
                er.BaseStream.Position = baseoffset + RecordOffsets[2];
                BANKRecord = new SymbolRecord(er);
                er.BaseStream.Position = baseoffset + RecordOffsets[3];
                WAVEARCRecord = new SymbolRecord(er);
                er.BaseStream.Position = baseoffset + RecordOffsets[4];
                PLAYERRecord = new SymbolRecord(er);
                er.BaseStream.Position = baseoffset + RecordOffsets[5];
                GROUPRecord = new SymbolRecord(er);
                er.BaseStream.Position = baseoffset + RecordOffsets[6];
                PLAYER2Record = new SymbolRecord(er);
                er.BaseStream.Position = baseoffset + RecordOffsets[7];
                STRMRecord = new SymbolRecord(er);
                er.BaseStream.Position = curpos;
                SEQRecord.ReadNames(er, baseoffset);
                SEQARCRecord.ReadNames(er, baseoffset);
                BANKRecord.ReadNames(er, baseoffset);
                WAVEARCRecord.ReadNames(er, baseoffset);
                PLAYERRecord.ReadNames(er, baseoffset);
                GROUPRecord.ReadNames(er, baseoffset);
                PLAYER2Record.ReadNames(er, baseoffset);
                STRMRecord.ReadNames(er, baseoffset);
                er.BaseStream.Position = baseoffset + SectionSize;
            }

            public void Write(EndianBinaryWriter er)
            {
                long sectionStart = er.BaseStream.Position;
                er.Write(Signature, Encoding.ASCII, false);
                er.Write((uint)0);
                long offsetPos = er.BaseStream.Position;
                er.Write(new uint[8], 0, 8);
                er.Write(Padding, 0, 24);
                int fixedHeader = 4 + 4 + 32 + 24;
                int nameBase = fixedHeader;
                nameBase += 4 + 4 * (int)SEQRecord.NrEntries;
                nameBase += 4 + 8 * (int)SEQARCRecord.NrEntries;
                nameBase += 4 + 4 * (int)BANKRecord.NrEntries;
                nameBase += 4 + 4 * (int)WAVEARCRecord.NrEntries;
                nameBase += 4 + 4 * (int)PLAYERRecord.NrEntries;
                nameBase += 4 + 4 * (int)GROUPRecord.NrEntries;
                nameBase += 4 + 4 * (int)PLAYER2Record.NrEntries;
                nameBase += 4 + 4 * (int)STRMRecord.NrEntries;
                int nameOffset = nameBase;
                long dataPos = er.BaseStream.Position;
                er.BaseStream.Position = offsetPos; er.Write((uint)(dataPos - sectionStart)); er.BaseStream.Position = dataPos;
                SEQRecord.Write(er, sectionStart, ref nameOffset);
                dataPos = er.BaseStream.Position;
                er.BaseStream.Position = offsetPos + 4; er.Write((uint)(dataPos - sectionStart)); er.BaseStream.Position = dataPos;
                SEQARCRecord.Write(er, sectionStart, ref nameOffset);
                dataPos = er.BaseStream.Position;
                er.BaseStream.Position = offsetPos + 8; er.Write((uint)(dataPos - sectionStart)); er.BaseStream.Position = dataPos;
                BANKRecord.Write(er, sectionStart, ref nameOffset);
                dataPos = er.BaseStream.Position;
                er.BaseStream.Position = offsetPos + 12; er.Write((uint)(dataPos - sectionStart)); er.BaseStream.Position = dataPos;
                WAVEARCRecord.Write(er, sectionStart, ref nameOffset);
                dataPos = er.BaseStream.Position;
                er.BaseStream.Position = offsetPos + 16; er.Write((uint)(dataPos - sectionStart)); er.BaseStream.Position = dataPos;
                PLAYERRecord.Write(er, sectionStart, ref nameOffset);
                dataPos = er.BaseStream.Position;
                er.BaseStream.Position = offsetPos + 20; er.Write((uint)(dataPos - sectionStart)); er.BaseStream.Position = dataPos;
                GROUPRecord.Write(er, sectionStart, ref nameOffset);
                dataPos = er.BaseStream.Position;
                er.BaseStream.Position = offsetPos + 24; er.Write((uint)(dataPos - sectionStart)); er.BaseStream.Position = dataPos;
                PLAYER2Record.Write(er, sectionStart, ref nameOffset);
                dataPos = er.BaseStream.Position;
                er.BaseStream.Position = offsetPos + 28; er.Write((uint)(dataPos - sectionStart)); er.BaseStream.Position = dataPos;
                STRMRecord.Write(er, sectionStart, ref nameOffset);
                long sectionEnd = er.BaseStream.Position;
                er.BaseStream.Position = sectionStart + 4;
                er.Write((uint)(sectionEnd - sectionStart));
                er.BaseStream.Position = sectionEnd;
                while (er.BaseStream.Position % 16 != 0)
                    er.Write((byte)0);
            }
            public String Signature;
            public UInt32 SectionSize;
            public UInt32[] RecordOffsets;
            public Byte[] Padding;
            public SymbolRecord SEQRecord;
            public ArchiveSymbolRecord SEQARCRecord;
            public SymbolRecord BANKRecord;
            public SymbolRecord WAVEARCRecord;
            public SymbolRecord PLAYERRecord;
            public SymbolRecord GROUPRecord;
            public SymbolRecord PLAYER2Record;
            public SymbolRecord STRMRecord;

            public class SymbolRecord
            {
                public SymbolRecord(EndianBinaryReader er)
                {
                    NrEntries = er.ReadUInt32();
                    EntryOffsets = er.ReadUInt32s((int)NrEntries);
                }
                public void Write(EndianBinaryWriter er, long baseOffset, ref int nameOffset)
                {
                    er.Write(NrEntries);
                    int start = nameOffset;
                    for (int i = 0; i < Entries.Length; i++)
                    {
                        if (Entries[i] != null)
                        {
                            er.Write((uint)nameOffset);
                            nameOffset += Encoding.ASCII.GetByteCount(Entries[i]) + 1;
                        }
                        else
                            er.Write((uint)0);
                    }
                    long curPos = er.BaseStream.Position;
                    er.BaseStream.Position = baseOffset + start;
                    for (int i = 0; i < Entries.Length; i++)
                    {
                        if (Entries[i] != null)
                            er.Write(Entries[i], Encoding.ASCII, true);
                    }
                    er.BaseStream.Position = curPos;
                }

                public void ReadNames(EndianBinaryReader er, long BaseOffset)
                {
                    long curpos = er.BaseStream.Position;
                    Entries = new string[NrEntries];
                    for (int i = 0; i < NrEntries; i++)
                    {
                        if (EntryOffsets[i] != 0)
                        {
                            er.BaseStream.Position = BaseOffset + EntryOffsets[i];
                            Entries[i] = er.ReadStringNT(Encoding.ASCII);
                        }
                        else Entries[i] = null;
                    }
                    er.BaseStream.Position = curpos;
                }
                public UInt32 NrEntries;
                public UInt32[] EntryOffsets;
                public String[] Entries;
            }

            public class ArchiveSymbolRecord
            {
                public ArchiveSymbolRecord(EndianBinaryReader er)
                {
                    NrEntries = er.ReadUInt32();
                    Entries = new ArchiveSymbolRecordEntry[NrEntries];
                    for (int i = 0; i < NrEntries; i++)
                        Entries[i] = new ArchiveSymbolRecordEntry(er);
                }

                public void Write(EndianBinaryWriter er, long baseOffset, ref int nameOffset)
                {
                    er.Write(NrEntries);
                    foreach (var e in Entries)
                        e.Write(er, baseOffset, ref nameOffset);
                }

                public void ReadNames(EndianBinaryReader er, long BaseOffset)
                {
                    long curpos = er.BaseStream.Position;
                    for (int i = 0; i < NrEntries; i++)
                    {
                        if (Entries[i].ArchiveNameOffset != 0)
                        {
                            er.BaseStream.Position = BaseOffset + Entries[i].ArchiveNameOffset;
                            Entries[i].ArchiveName = er.ReadStringNT(Encoding.ASCII);
                        }
                        else Entries[i].ArchiveName = null;
                        if (Entries[i].ArchiveSubRecordOffset != 0)
                        {
                            er.BaseStream.Position = BaseOffset + Entries[i].ArchiveSubRecordOffset;
                            Entries[i].ArchiveSubRecord = new SymbolRecord(er);
                            Entries[i].ArchiveSubRecord.ReadNames(er, BaseOffset);
                        }
                        else
                        {
                            Entries[i].ArchiveSubRecord = null;
                        }
                    }
                    er.BaseStream.Position = curpos;
                }

                public UInt32 NrEntries;
                public ArchiveSymbolRecordEntry[] Entries;

                public class ArchiveSymbolRecordEntry
                {
                    public ArchiveSymbolRecordEntry(EndianBinaryReader er)
                    {
                        ArchiveNameOffset = er.ReadUInt32();
                        ArchiveSubRecordOffset = er.ReadUInt32();
                    }

                    public void Write(EndianBinaryWriter er, long baseOffset, ref int nameOffset)
                    {
                        if (ArchiveName != null)
                        {
                            er.Write((uint)nameOffset);
                            long cur = er.BaseStream.Position;
                            er.BaseStream.Position = baseOffset + nameOffset;
                            er.Write(ArchiveName, Encoding.ASCII, true);
                            nameOffset += Encoding.ASCII.GetByteCount(ArchiveName) + 1;
                            er.BaseStream.Position = cur;
                        }
                        else
                            er.Write((uint)0);
                        long subOffsetPos = er.BaseStream.Position;
                        er.Write((uint)0);
                        if (ArchiveSubRecord != null)
                        {
                            long subStart = er.BaseStream.Position;
                            ArchiveSubRecord.Write(er, baseOffset, ref nameOffset);
                            er.BaseStream.Position = subOffsetPos;
                            er.Write((uint)(subStart - baseOffset));
                            er.BaseStream.Position = er.BaseStream.Length;
                        }
                    }
                    public UInt32 ArchiveNameOffset;
                    public UInt32 ArchiveSubRecordOffset;
                    public String ArchiveName;
                    public SymbolRecord ArchiveSubRecord;
                }
            }
        }

        public INFO InfoBlock;
        public class INFO
        {
            public INFO(EndianBinaryReader er)
            {
                long baseoffset = er.BaseStream.Position;
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "INFO") throw new SignatureNotCorrectException(Signature, "INFO", er.BaseStream.Position - 4);
                SectionSize = er.ReadUInt32();
                RecordOffsets = er.ReadUInt32s(8);
                Padding = er.ReadBytes(24);
                long curpos = er.BaseStream.Position;
                er.BaseStream.Position = baseoffset + RecordOffsets[0];
                SEQRecord = new InfoRecord<SEQInfo>(er, baseoffset);
                er.BaseStream.Position = baseoffset + RecordOffsets[1];
                SEQARCRecord = new InfoRecord<SEQARCInfo>(er, baseoffset);
                er.BaseStream.Position = baseoffset + RecordOffsets[2];
                BANKRecord = new InfoRecord<BANKInfo>(er, baseoffset);
                er.BaseStream.Position = baseoffset + RecordOffsets[3];
                WAVEARCRecord = new InfoRecord<WAVEARCInfo>(er, baseoffset);
                er.BaseStream.Position = baseoffset + RecordOffsets[4];
                PLAYERRecord = new InfoRecord<PLAYERInfo>(er, baseoffset);
                er.BaseStream.Position = baseoffset + RecordOffsets[5];
                GROUPRecord = new InfoRecord<GROUPInfo>(er, baseoffset);
                er.BaseStream.Position = baseoffset + RecordOffsets[6];
                STREAMPLAYERRecord = new InfoRecord<STREAMPLAYERInfo>(er, baseoffset);
                er.BaseStream.Position = baseoffset + RecordOffsets[7];
                STREAMRecord = new InfoRecord<STREAMInfo>(er, baseoffset);
                er.BaseStream.Position = curpos;
                er.BaseStream.Position = baseoffset + SectionSize;
            }
            public void Write(EndianBinaryWriter er)
            {
                long sectionStart = er.BaseStream.Position;
                er.Write(Signature, Encoding.ASCII, false);
                er.Write((uint)0);
                long offsetsPos = er.BaseStream.Position;
                er.Write(new uint[8], 0, 8);
                er.Write(Padding, 0, 24);
                long dataPos = er.BaseStream.Position;
                er.BaseStream.Position = offsetsPos;
                er.Write((uint)(dataPos - sectionStart));
                er.BaseStream.Position = dataPos;
                SEQRecord.Write(er);
                dataPos = er.BaseStream.Position;
                er.BaseStream.Position = offsetsPos + 4;
                er.Write((uint)(dataPos - sectionStart));
                er.BaseStream.Position = dataPos;
                SEQARCRecord.Write(er);
                dataPos = er.BaseStream.Position;
                er.BaseStream.Position = offsetsPos + 8;
                er.Write((uint)(dataPos - sectionStart));
                er.BaseStream.Position = dataPos;
                BANKRecord.Write(er);
                dataPos = er.BaseStream.Position;
                er.BaseStream.Position = offsetsPos + 12;
                er.Write((uint)(dataPos - sectionStart));
                er.BaseStream.Position = dataPos;
                WAVEARCRecord.Write(er);
                dataPos = er.BaseStream.Position;
                er.BaseStream.Position = offsetsPos + 16;
                er.Write((uint)(dataPos - sectionStart));
                er.BaseStream.Position = dataPos;
                PLAYERRecord.Write(er);
                dataPos = er.BaseStream.Position;
                er.BaseStream.Position = offsetsPos + 20;
                er.Write((uint)(dataPos - sectionStart));
                er.BaseStream.Position = dataPos;
                GROUPRecord.Write(er);
                dataPos = er.BaseStream.Position;
                er.BaseStream.Position = offsetsPos + 24;
                er.Write((uint)(dataPos - sectionStart));
                er.BaseStream.Position = dataPos;
                STREAMPLAYERRecord.Write(er);
                dataPos = er.BaseStream.Position;
                er.BaseStream.Position = offsetsPos + 28;
                er.Write((uint)(dataPos - sectionStart));
                er.BaseStream.Position = dataPos;
                STREAMRecord.Write(er);
                long sectionEnd = er.BaseStream.Position;
                er.BaseStream.Position = sectionStart + 4;
                er.Write((uint)(sectionEnd - sectionStart));
                er.BaseStream.Position = sectionEnd;
            }
            public class InfoRecord<T> where T : InfoBase, new()
            {
                public InfoRecord(EndianBinaryReader er, long infoBlockBase)
                {
                    NrEntries = er.ReadUInt32();
                    EntryOffsets = er.ReadUInt32s((int)NrEntries);
                    long cur = er.BaseStream.Position;
                    Entries = new T[NrEntries];
                    for (int i = 0; i < NrEntries; i++)
                    {
                        er.BaseStream.Position = infoBlockBase + EntryOffsets[i];
                        Entries[i] = new T();
                        Entries[i].Read(er);
                    }
                    er.BaseStream.Position = cur;
                }
                public void Write(EndianBinaryWriter er)
                {
                    er.Write((uint)Entries.Length);
                    int offset = 4 + Entries.Length * 4;
                    foreach (var e in Entries)
                    {
                        er.Write((uint)offset);
                        offset += e.GetLength();
                    }
                    foreach (var e in Entries) e.Write(er);
                }
                public uint NrEntries;
                public uint[] EntryOffsets;
                public T[] Entries;
            }

            public abstract class InfoBase
            {
                public abstract void Read(EndianBinaryReader er);
                public abstract void Write(EndianBinaryWriter er);
                public abstract int GetLength();
            }

            public class SEQInfo : InfoBase
            {
                public uint FileID;
                public ushort Bank;
                public byte Volume;
                public byte ChannelPrio;
                public byte PlayerPrio;
                public byte PlayerNo;
                public byte[] Unknown;
                public override void Read(EndianBinaryReader er)
                {
                    FileID = er.ReadUInt32();
                    Bank = er.ReadUInt16();
                    Volume = er.ReadByte();
                    ChannelPrio = er.ReadByte();
                    PlayerPrio = er.ReadByte();
                    PlayerNo = er.ReadByte();
                    Unknown = er.ReadBytes(2);
                }
                public override void Write(EndianBinaryWriter er)
                {
                    er.Write(FileID);
                    er.Write(Bank);
                    er.Write(Volume);
                    er.Write(ChannelPrio);
                    er.Write(PlayerPrio);
                    er.Write(PlayerNo);
                    er.Write(Unknown, 0, 2);
                }
                public override int GetLength() => 12;
            }
            public class SEQARCInfo : InfoBase
            {
                public uint FileID;
                public override void Read(EndianBinaryReader er) => FileID = er.ReadUInt32();
                public override void Write(EndianBinaryWriter er) => er.Write(FileID);
                public override int GetLength() => 4;
            }
            public class BANKInfo : InfoBase
            {
                public uint FileID;
                public ushort[] WaveArc;
                public override void Read(EndianBinaryReader er)
                {
                    FileID = er.ReadUInt32();
                    WaveArc = er.ReadUInt16s(4);
                }
                public override void Write(EndianBinaryWriter er)
                {
                    er.Write(FileID);
                    er.Write(WaveArc, 0, 4);
                }
                public override int GetLength() => 12;
            }
            public class WAVEARCInfo : InfoBase
            {
                public uint FileID;
                public byte Flags;
                public override void Read(EndianBinaryReader er)
                {
                    uint val = er.ReadUInt32();
                    FileID = val & 0xFFFFFF;
                    Flags = (byte)(val >> 24);
                }
                public override void Write(EndianBinaryWriter er)
                {
                    er.Write(((uint)Flags << 24) | (FileID & 0xFFFFFF));
                }
                public override int GetLength() => 4;
            }
            public class PLAYERInfo : InfoBase
            {
                public byte SeqMax;
                public byte Padding;
                public ushort AllocChBitFlag;
                public uint HeapSize;
                public override void Read(EndianBinaryReader er)
                {
                    SeqMax = er.ReadByte();
                    Padding = er.ReadByte();
                    AllocChBitFlag = er.ReadUInt16();
                    HeapSize = er.ReadUInt32();
                }
                public override void Write(EndianBinaryWriter er)
                {
                    er.Write(SeqMax);
                    er.Write(Padding);
                    er.Write(AllocChBitFlag);
                    er.Write(HeapSize);
                }
                public override int GetLength() => 8;
            }
            public class GROUPInfo : InfoBase
            {
                public class GroupItem
                {
                    public byte Type;
                    public byte LoadFlag;
                    public ushort Padding;
                    public uint Index;
                    public void Read(EndianBinaryReader er)
                    {
                        Type = er.ReadByte();
                        LoadFlag = er.ReadByte();
                        Padding = er.ReadUInt16();
                        Index = er.ReadUInt32();
                    }
                    public void Write(EndianBinaryWriter er)
                    {
                        er.Write(Type);
                        er.Write(LoadFlag);
                        er.Write(Padding);
                        er.Write(Index);
                    }
                }
                public uint NrItems;
                public GroupItem[] Items;

                public override void Read(EndianBinaryReader er)
                {
                    NrItems = er.ReadUInt32();
                    Items = new GroupItem[NrItems];
                    for (int i = 0; i < NrItems; i++)
                    {
                        Items[i] = new GroupItem();
                        Items[i].Read(er);
                    }
                }
                public override void Write(EndianBinaryWriter er)
                {
                    er.Write(NrItems);
                    foreach (var i in Items) i.Write(er);
                }
                public override int GetLength() => 4 + Items.Length * 8;
            }
            public class STREAMPLAYERInfo : InfoBase
            {
                public byte NumChannels;
                public byte[] ChNo;
                public byte[] Padding;
                public override void Read(EndianBinaryReader er)
                {
                    NumChannels = er.ReadByte();
                    ChNo = er.ReadBytes(16);
                    Padding = er.ReadBytes(7);
                }
                public override void Write(EndianBinaryWriter er)
                {
                    er.Write(NumChannels);
                    er.Write(ChNo, 0, 16);
                    er.Write(Padding, 0, 7);
                }
                public override int GetLength() => 24;
            }
            public class STREAMInfo : InfoBase
            {
                public uint FileID;
                public byte Volume;
                public byte PlayerPrio;
                public byte PlayerNo;
                public byte Flags;
                public override void Read(EndianBinaryReader er)
                {
                    FileID = er.ReadUInt32();
                    Volume = er.ReadByte();
                    PlayerPrio = er.ReadByte();
                    PlayerNo = er.ReadByte();
                    Flags = er.ReadByte();
                }
                public override void Write(EndianBinaryWriter er)
                {
                    er.Write(FileID);
                    er.Write(Volume);
                    er.Write(PlayerPrio);
                    er.Write(PlayerNo);
                    er.Write(Flags);
                }
                public override int GetLength() => 8;
            }

            public String Signature;
            public UInt32 SectionSize;
            public UInt32[] RecordOffsets;
            public Byte[] Padding;
            public InfoRecord<SEQInfo> SEQRecord;
            public InfoRecord<SEQARCInfo> SEQARCRecord;
            public InfoRecord<BANKInfo> BANKRecord;
            public InfoRecord<WAVEARCInfo> WAVEARCRecord;
            public InfoRecord<PLAYERInfo> PLAYERRecord;
            public InfoRecord<GROUPInfo> GROUPRecord;
            public InfoRecord<STREAMPLAYERInfo> STREAMPLAYERRecord;
            public InfoRecord<STREAMInfo> STREAMRecord;
        }

        public FAT FileAllocationTable;
        public class FAT
        {
            public FAT(EndianBinaryReader er)
            {
                long baseoffset = er.BaseStream.Position;
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "FAT ") throw new SignatureNotCorrectException(Signature, "FAT ", er.BaseStream.Position - 4);
                SectionSize = er.ReadUInt32();
                NrEntries = er.ReadUInt32();
                Entries = new FATEntry[NrEntries];
                for (int i = 0; i < NrEntries; i++)
                {
                    Entries[i] = new FATEntry(er);
                }
                er.BaseStream.Position = baseoffset + SectionSize;
            }
            public void Write(EndianBinaryWriter er)
            {
                long sectionStart = er.BaseStream.Position;
                er.Write(Signature, Encoding.ASCII, false);
                er.Write((uint)0);
                er.Write(NrEntries);
                long dataStart = er.BaseStream.Position;
                dataStart += Entries.Length * 16;
                while (dataStart % 32 != 0) dataStart++;
                long dataOffset = dataStart;
                foreach (var entry in Entries)
                    entry.Write(er, ref dataOffset);
                long sectionEnd = er.BaseStream.Position;
                er.BaseStream.Position = sectionStart + 4;
                er.Write((uint)(sectionEnd - sectionStart));
                er.BaseStream.Position = sectionEnd;
            }
            public String Signature;
            public UInt32 SectionSize;
            public UInt32 NrEntries;
            public FATEntry[] Entries;

            public class FATEntry
            {
                public FATEntry(EndianBinaryReader er)
                {
                    Offset = er.ReadUInt32();
                    Length = er.ReadUInt32();
                    Padding = er.ReadBytes(8);
                    long curpos = er.BaseStream.Position;
                    er.BaseStream.Position = Offset;
                    Data = er.ReadBytes((int)Length);
                    er.BaseStream.Position = curpos;
                }
                public void Write(EndianBinaryWriter er, ref long dataOffset)
                {
                    er.Write((uint)dataOffset);
                    er.Write((uint)Data.Length);
                    er.Write(Padding, 0, 8);
                    long curpos = er.BaseStream.Position;
                    er.BaseStream.Position = dataOffset;
                    er.Write(Data, 0, Data.Length);
                    while (er.BaseStream.Position % 32 != 0)
                        er.Write((byte)0);
                    dataOffset = er.BaseStream.Position;
                    er.BaseStream.Position = curpos;
                }

                public UInt32 Offset;
                public UInt32 Length;
                public Byte[] Padding;
                public Byte[] Data;
            }
        }

        public FILE File;
        public class FILE
        {
            public FILE(EndianBinaryReader er)
            {
                long baseoffset = er.BaseStream.Position;
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "FILE") throw new SignatureNotCorrectException(Signature, "FILE", er.BaseStream.Position - 4);
                SectionSize = er.ReadUInt32();
                NrFiles = er.ReadUInt32();
                while ((er.BaseStream.Position % 32) != 0)
                    er.ReadByte();
                er.BaseStream.Position = baseoffset + SectionSize;
            }
            public void Write(EndianBinaryWriter er)
            {
                long sectionStart = er.BaseStream.Position;
                er.Write(Signature, Encoding.ASCII, false);
                er.Write((uint)0);
                er.Write(NrFiles);
                while (er.BaseStream.Position % 32 != 0)
                    er.Write((byte)0);
                long sectionEnd = er.BaseStream.Position;
                er.BaseStream.Position = sectionStart + 4;
                er.Write((uint)(sectionEnd - sectionStart));
                er.BaseStream.Position = sectionEnd;
            }
            public String Signature;
            public UInt32 SectionSize;
            public UInt32 NrFiles;
        }

        //public void FromFileSystem(SFSDirectory Root)
        //{

        //}

        public SFSDirectory ToFileSystem()
        {
            SFSDirectory root = new SFSDirectory("/", true);
            SFSDirectory seqDir = new SFSDirectory("Seq", false) { Parent = root };
            SFSDirectory seqArcDir = new SFSDirectory("SeqArc", false) { Parent = root };
            SFSDirectory bankDir = new SFSDirectory("Bank", false) { Parent = root };
            SFSDirectory waveArcDir = new SFSDirectory("WaveArc", false) { Parent = root };
            SFSDirectory strmDir = new SFSDirectory("Strm", false) { Parent = root };
            root.SubDirectories.Add(seqDir);
            root.SubDirectories.Add(seqArcDir);
            root.SubDirectories.Add(bankDir);
            root.SubDirectories.Add(waveArcDir);
            root.SubDirectories.Add(strmDir);
            int fileId = 0;
            var seqRecord = InfoBlock.SEQRecord;
            for (int i = 0; i < seqRecord.Entries.Length; i++)
            {
                var info = seqRecord.Entries[i];
                int fatIndex = (int)info.FileID;
                if (fatIndex < 0 || fatIndex >= FileAllocationTable.Entries.Length) continue;
                var fat = FileAllocationTable.Entries[fatIndex];
                byte[] data = fat.Data;
                string name = "Seq " + i.ToString("D0");
                if (SymbolBlock != null && i < SymbolBlock.SEQRecord.Entries.Length)
                {
                    string symName = SymbolBlock.SEQRecord.Entries[i];
                    if (!string.IsNullOrEmpty(symName)) name = symName;
                }
                SFSFile file = new SFSFile(fileId++, name + ".sseq", seqDir);
                file.Data = data;
                seqDir.Files.Add(file);
            }
            var ssrcRecord = InfoBlock.SEQARCRecord;
            for (int i = 0; i < ssrcRecord.Entries.Length; i++)
            {
                var info = ssrcRecord.Entries[i];
                int fatIndex = (int)info.FileID;
                if (fatIndex < 0 || fatIndex >= FileAllocationTable.Entries.Length) continue;
                var fat = FileAllocationTable.Entries[fatIndex];
                byte[] data = fat.Data;
                string name = "SeqArc " + i.ToString("D0");
                if (SymbolBlock != null && i < SymbolBlock.SEQARCRecord.Entries.Length)
                {
                    var sym = SymbolBlock.SEQARCRecord.Entries[i];
                    if (!string.IsNullOrEmpty(sym.ArchiveName)) name = sym.ArchiveName;
                }
                SFSFile file = new SFSFile(fileId++, name + ".ssar", seqArcDir);
                file.Data = data;
                seqArcDir.Files.Add(file);
            }
            var bnkRecord = InfoBlock.BANKRecord;
            for (int i = 0; i < bnkRecord.Entries.Length; i++)
            {
                var info = bnkRecord.Entries[i];
                int fatIndex = (int)info.FileID;
                if (fatIndex < 0 || fatIndex >= FileAllocationTable.Entries.Length) continue;
                var fat = FileAllocationTable.Entries[fatIndex];
                byte[] data = fat.Data;
                string name = "Bank " + i.ToString("D0");
                if (SymbolBlock != null && i < SymbolBlock.BANKRecord.Entries.Length)
                {
                    string symName = SymbolBlock.BANKRecord.Entries[i];
                    if (!string.IsNullOrEmpty(symName)) name = symName;
                }
                SFSFile file = new SFSFile(fileId++, name + ".sbnk", bankDir);
                file.Data = data;
                bankDir.Files.Add(file);
            }
            var warcRecord = InfoBlock.WAVEARCRecord;
            for (int i = 0; i < warcRecord.Entries.Length; i++)
            {
                var info = warcRecord.Entries[i];
                int fatIndex = (int)info.FileID;
                if (fatIndex < 0 || fatIndex >= FileAllocationTable.Entries.Length) continue;
                var fat = FileAllocationTable.Entries[fatIndex];
                byte[] data = fat.Data;
                string name = "WaveArc " + i.ToString("D0");
                if (SymbolBlock != null && i < SymbolBlock.WAVEARCRecord.Entries.Length)
                {
                    string symName = SymbolBlock.WAVEARCRecord.Entries[i];
                    if (!string.IsNullOrEmpty(symName)) name = symName;
                }
                SFSFile file = new SFSFile(fileId++, name + ".swar", waveArcDir);
                file.Data = data;
                waveArcDir.Files.Add(file);
            }
            var strmRecord = InfoBlock.STREAMRecord;
            for (int i = 0; i < strmRecord.Entries.Length; i++)
            {
                var info = strmRecord.Entries[i];
                int fatIndex = (int)info.FileID;
                if (fatIndex < 0 || fatIndex >= FileAllocationTable.Entries.Length) continue;
                var fat = FileAllocationTable.Entries[fatIndex];
                byte[] data = fat.Data;
                string name = "Stream " + i.ToString("D0");
                if (SymbolBlock != null && i < SymbolBlock.STRMRecord.Entries.Length)
                {
                    string symName = SymbolBlock.STRMRecord.Entries[i];
                    if (!string.IsNullOrEmpty(symName)) name = symName;
                }
                SFSFile file = new SFSFile(fileId++, name + ".strm", strmDir);
                file.Data = data;
                strmDir.Files.Add(file);
            }
            return root;
        }

        public class SDATIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Sound;
            }

            public override string GetFileDescription()
            {
                return "Nitro Sound Data (SDAT)";
            }

            public override string GetFileFilter()
            {
                return "Nitro Sound Data (*.sdat)|*.sdat";
            }

            public override Bitmap GetIcon()
            {
                return Resource.disc_music;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'S' && File.Data[1] == 'D' && File.Data[2] == 'A' && File.Data[3] == 'T') return FormatMatch.Content;
                return FormatMatch.No;
            }
        }
    }
}