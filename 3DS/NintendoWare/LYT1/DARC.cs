using System;
using System.Collections.Generic;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using LibEveryFileExplorer.Files.SimpleFileSystem;
using _3DS.UI;
using LibEveryFileExplorer.IO;
using System.Windows.Forms;

namespace _3DS.NintendoWare.LYT1
{
    public class DARC : FileFormat<DARC.darcIdentifier>, IViewable//, IWriteable, IEmptyCreatable
    {
        public DARC()
        {
            Header = new darcHeader();
            FileNameTable = new Dictionary<uint, string>();
            FileNameTable[0] = "";
            Data = new byte[0];
            Entries = new FileTableEntry[2];
            Entries[0] = new FileTableEntry();
            Entries[0].IsFolder = false;
            Entries[0].NameOffset = 0;
            Entries[0].DataOffset = 0;
            Entries[0].DataLength = 2;
            Entries[1] = new FileTableEntry();
            Entries[1].IsFolder = true;
            Entries[1].NameOffset = 0;
            Entries[1].DataOffset = 0;
            Entries[1].DataLength = 1;
        }
        public DARC(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new darcHeader(er);
                er.BaseStream.Position = Header.FileTableOffset;
                FileTableEntry root = new FileTableEntry(er);
                Entries = new FileTableEntry[root.DataLength];
                Entries[0] = root;
                for (int i = 1; i < root.DataLength; i++) Entries[i] = new FileTableEntry(er);
                FileNameTable = new Dictionary<uint, string>();
                uint offs = 0;
                for (int i = 0; i < root.DataLength; i++)
                {
                    String s = er.ReadStringNT(Encoding.Unicode);
                    FileNameTable.Add(offs, s);
                    offs += (uint)s.Length * 2 + 2;
                }
                er.BaseStream.Position = Header.FileDataOffset;
                this.Data = er.ReadBytes((int)(Header.FileSize - Header.FileDataOffset));
            }
            finally
            {
                er.Close();
            }
        }

        public Form GetDialog()
        {
            return new DARCViewer(this);
        }

        public String GetSaveDefaultFileFilter()
        {
            return "Data Archive (*.darc, *.arc)|*.darc;*.arc";
        }

        public byte[] Write()
        {
            MemoryStream m = new MemoryStream();
            EndianBinaryWriter er = new EndianBinaryWriter(m, Endianness.LittleEndian);
            byte[] fileData = this.Data;
            MemoryStream nameTableStream = new MemoryStream();
            EndianBinaryWriter nameWriter = new EndianBinaryWriter(nameTableStream, Endianness.LittleEndian);
            Dictionary<string, uint> nameOffsets = new Dictionary<string, uint>();
            uint currentNameOffset = 0;
            foreach (var entry in Entries)
            {
                string name = FileNameTable[entry.NameOffset];
                if (!nameOffsets.ContainsKey(name))
                {
                    nameOffsets[name] = currentNameOffset;
                    byte[] nameBytes = Encoding.Unicode.GetBytes(name);
                    nameWriter.Write(nameBytes, 0, nameBytes.Length);
                    nameWriter.Write((ushort)0);
                    currentNameOffset += (uint)(nameBytes.Length + 2);
                }
            }
            byte[] nameTable = nameTableStream.ToArray();
            MemoryStream fileTableStream = new MemoryStream();
            EndianBinaryWriter fileTableWriter = new EndianBinaryWriter(fileTableStream, Endianness.LittleEndian);
            foreach (var entry in Entries)
            {
                uint nameOffset = nameOffsets[FileNameTable[entry.NameOffset]];
                uint flags = (uint)(entry.IsFolder ? 0x01000000 : 0x00000000);
                fileTableWriter.Write(nameOffset | flags);
                fileTableWriter.Write(entry.DataOffset);
                fileTableWriter.Write(entry.DataLength);
            }
            byte[] fileTable = fileTableStream.ToArray();
            Header.FileTableOffset = (uint)Header.HeaderSize;
            Header.FileTableLength = (uint)fileTable.Length;
            Header.FileDataOffset = Header.FileTableOffset + Header.FileTableLength + (uint)nameTable.Length;
            Header.FileDataOffset = (uint)((Header.FileDataOffset + 0x7F) & ~0x7F);
            Header.FileSize = Header.FileDataOffset + (uint)fileData.Length;
            Header.Write(er);
            er.BaseStream.Position = Header.HeaderSize;
            while (er.BaseStream.Position < Header.FileTableOffset)
            er.Write((byte)0);
            er.Write(fileTable, 0, fileTable.Length);
            er.Write(nameTable, 0, nameTable.Length);
            while ((er.BaseStream.Position % 128) != 0)
            er.Write((byte)0);
            er.Write(fileData, 0, fileData.Length);
            byte[] result = m.ToArray();
            er.Close();
            return result;
        }

        public darcHeader Header;
        public class darcHeader
        {
            public darcHeader()
            {
                Signature = "darc";
                Endianness = 0xFEFF;
                HeaderSize = 0x1C;
                Version = 0x1000000;
            }
            public darcHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "darc") throw new SignatureNotCorrectException(Signature, "darc", er.BaseStream.Position - 4);
                Endianness = er.ReadUInt16();
                HeaderSize = er.ReadUInt16();
                Version = er.ReadUInt32();
                FileSize = er.ReadUInt32();
                FileTableOffset = er.ReadUInt32();
                FileTableLength = er.ReadUInt32();
                FileDataOffset = er.ReadUInt32();
            }
            public void Write(EndianBinaryWriter er)
            {
                er.Write(Signature, Encoding.ASCII, false);
                er.Write(Endianness);
                er.Write(HeaderSize);
                er.Write(Version);
                er.Write(FileSize);
                er.Write(FileTableOffset);
                er.Write(FileTableLength);
                er.Write(FileDataOffset);
            }
            public String Signature;
            public UInt16 Endianness;
            public UInt16 HeaderSize;
            public UInt32 Version;
            public UInt32 FileSize;
            public UInt32 FileTableOffset;
            public UInt32 FileTableLength;
            public UInt32 FileDataOffset;
        }

        public FileTableEntry[] Entries;
        public class FileTableEntry
        {
            public FileTableEntry()
            {

            }
            public FileTableEntry(EndianBinaryReader er)
            {
                NameOffset = er.ReadUInt32();
                IsFolder = (NameOffset >> 24) == 1;
                NameOffset &= 0xFFFFFF;
                DataOffset = er.ReadUInt32();
                DataLength = er.ReadUInt32();
            }
            public UInt32 NameOffset;
            public Boolean IsFolder;
            public UInt32 DataOffset;//Parent Entry Index if folder
            public UInt32 DataLength;//Nr Files if folder
        }

        public Dictionary<UInt32, String> FileNameTable;

        public byte[] Data;

        public SFSDirectory ToFileSystem()
        {
            SFSDirectory[] dirs = new SFSDirectory[Entries.Length];
            dirs[1] = new SFSDirectory("/", true);
            var curdir = dirs[1];
            for (int i = 2; i < Entries.Length; i++)
            {
                if (Entries[i].IsFolder)
                {
                    var folder = new SFSDirectory(FileNameTable[Entries[i].NameOffset], false);
                    dirs[i] = folder;
                    folder.Parent = dirs[Entries[i].DataOffset];
                    dirs[Entries[i].DataOffset].SubDirectories.Add(folder);
                    curdir = folder;
                }
                else
                {
                    var file = new SFSFile(-1, FileNameTable[Entries[i].NameOffset], curdir);
                    byte[] data = new byte[Entries[i].DataLength];
                    Array.Copy(Data, Entries[i].DataOffset - Header.FileDataOffset, data, 0, Entries[i].DataLength);
                    file.Data = data;
                    curdir.Files.Add(file);
                }
            }
            return dirs[1];
        }

        public void FromFileSystem(SFSDirectory Root)
        {
            List<FileTableEntry> entries = new List<FileTableEntry>();
            List<SFSFile> fileRefs = new List<SFSFile>();
            Dictionary<string, uint> nameOffsets = new Dictionary<string, uint>();
            FileNameTable = new Dictionary<uint, string>();
            uint curNameOffset = 0;
            FileTableEntry rootRec = new FileTableEntry();
            rootRec.IsFolder = false;
            rootRec.NameOffset = GetOrAddName("", nameOffsets, FileNameTable, ref curNameOffset);
            rootRec.DataOffset = 0;
            entries.Add(rootRec);
            fileRefs.Add(null);
            FileTableEntry rootDir = new FileTableEntry();
            rootDir.IsFolder = true;
            rootDir.NameOffset = GetOrAddName("", nameOffsets, FileNameTable, ref curNameOffset);
            rootDir.DataOffset = 0;
            rootDir.DataLength = (uint)ComputeSubtreeCount(Root);
            entries.Add(rootDir);
            fileRefs.Add(null);
            BuildDir(Root, 1u, entries, fileRefs, nameOffsets, FileNameTable, ref curNameOffset);
            rootRec.DataLength = (uint)entries.Count;
            uint fileTableLength = (uint)entries.Count * 12u;
            uint fileTableOffset = Header.HeaderSize;
            uint fileDataOffset = (uint)((fileTableOffset + fileTableLength + curNameOffset + 0x7Fu) & ~0x7Fu);
            MemoryStream dataStream = new MemoryStream();
            for (int i = 0; i < entries.Count; i++)
            {
                FileTableEntry e = entries[i];
                if (i == 0) continue;
                if (!e.IsFolder)
                {
                    SFSFile f = fileRefs[i];
                    byte[] fdata = (f != null && f.Data != null) ? f.Data : new byte[0];
                    e.DataOffset = fileDataOffset + (uint)dataStream.Position;
                    e.DataLength = (uint)fdata.Length;
                    dataStream.Write(fdata, 0, fdata.Length);
                }
            }
        }

        private static void BuildDir(SFSDirectory dir, uint selfIndex,
            List<FileTableEntry> entries, List<SFSFile> fileRefs,
            Dictionary<string, uint> nameOffsets, Dictionary<uint, string> fileNameTable, ref uint curNameOffset)
        {
            foreach (SFSFile f in dir.Files)
            {
                FileTableEntry e = new FileTableEntry();
                e.IsFolder = false;
                e.NameOffset = GetOrAddName(f.FileName, nameOffsets, fileNameTable, ref curNameOffset);
                entries.Add(e);
                fileRefs.Add(f);
            }
            foreach (SFSDirectory sub in dir.SubDirectories)
            {
                uint subIndex = (uint)entries.Count;
                FileTableEntry fe = new FileTableEntry();
                fe.IsFolder = true;
                fe.NameOffset = GetOrAddName(sub.DirectoryName, nameOffsets, fileNameTable, ref curNameOffset);
                fe.DataOffset = selfIndex;
                fe.DataLength = (uint)ComputeSubtreeCount(sub);
                entries.Add(fe);
                fileRefs.Add(null);
                BuildDir(sub, subIndex, entries, fileRefs, nameOffsets, fileNameTable, ref curNameOffset);
            }

        }

        private static int ComputeSubtreeCount(SFSDirectory dir)
        {
            int n = 1;
            n += dir.Files.Count;
            foreach (SFSDirectory sd in dir.SubDirectories) n += ComputeSubtreeCount(sd);
            return n;
        }

        private static uint GetOrAddName(string name, Dictionary<string, uint> nameOffsets,
            Dictionary<uint, string> fileNameTable, ref uint curOffset)
        {
            uint off;
            if (!nameOffsets.TryGetValue(name, out off))
            {
                off = curOffset;
                nameOffsets[name] = off;
                fileNameTable[off] = name;
                curOffset += (uint)(Encoding.Unicode.GetByteCount(name) + 2);
            }
            return off;
        }


        public class darcIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Archives;
            }

            public override string GetFileDescription()
            {
                return "Data Archive (DARC)";
            }

            public override string GetFileFilter()
            {
                return "Data Archive (*.darc, *.arc, *.bcma)|*.darc;*.arc;*.bcma";
            }

            public override Bitmap GetIcon()
            {
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'd' && File.Data[1] == 'a' && File.Data[2] == 'r' && File.Data[3] == 'c') return FormatMatch.Content;
                return FormatMatch.No;
            }

        }
    }
}