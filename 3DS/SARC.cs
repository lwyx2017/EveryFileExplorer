using System;
using System.Collections.Generic;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using LibEveryFileExplorer.Files.SimpleFileSystem;
using _3DS.UI;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.IO.Serialization;

namespace _3DS
{
    public class SARC : FileFormat<SARC.SARCIdentifier>, IViewable, IWriteable
    {
        public SARC(byte[] Data)
        {
            EndianBinaryReaderEx er = new EndianBinaryReaderEx(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new SARCHeader(er);
                SFat = new SFAT(er);
                SFnt = new SFNT(er, Header.FileDataOffset);
                er.BaseStream.Position = Header.FileDataOffset;
                this.Data = er.ReadBytes((int)(Header.FileSize - Header.FileDataOffset));
            }
            finally
            {
                er.Close();
            }
        }

        public System.Windows.Forms.Form GetDialog()
        {
            return new SARCViewer(this);
        }

        public string GetSaveDefaultFileFilter()
        {
            return "Simple Archive (*.sarc)|*.sarc";
        }

        public byte[] Write()
        {
            MemoryStream m = new MemoryStream();
            Endianness endian = Header.Endianness == 0xFFFE ? Endianness.LittleEndian : Endianness.BigEndian;
            EndianBinaryWriterEx er = new EndianBinaryWriterEx(m, endian);
            try
            {
                Header.Write(er);
                SFat.Write(er);
                er.Write(SFnt.Signature, Encoding.ASCII, false);
                er.Write(SFnt.HeaderSize);
                er.Write(SFnt.Reserved);
                if (SFnt.StringTable != null && SFnt.StringTable.Length > 0)
                {
                    er.Write(SFnt.StringTable);
                }
                er.WritePadding(128, 0);
                er.Write(Data, 0, Data.Length);
                er.BaseStream.Position = 8;
                er.Write((UInt32)(er.BaseStream.Length));
                er.BaseStream.Position = 0x10;
                er.Write((UInt32)(Header.FileDataOffset));
                byte[] result = m.ToArray();
                return result;
            }
            finally
            {
                er.Close();
            }
        }

        public SARCHeader Header;
        public class SARCHeader
        {
            public SARCHeader()
            {
                Signature = "SARC";
                HeaderSize = 0x14;
                Endianness = 0xFFFE;//0xFEFF;
                FileSize = 0;
                FileDataOffset = 0;
                Version = 0x0100;
            }
            public SARCHeader(EndianBinaryReaderEx er)
            {
                er.ReadObject(this);
            }
            public void Write(EndianBinaryWriter er)
            {
                er.Write(Signature, Encoding.ASCII, false);
                if (Endianness == 0xFFFE)
                { 
                    er.Write((ushort)0x14); 
                }
                else 
                { 
                    er.Write(HeaderSize); 
                
                }
                er.Endianness = LibEveryFileExplorer.IO.Endianness.BigEndian;
                er.Write(Endianness);
                if (Endianness == 0xFFFE) er.Endianness = LibEveryFileExplorer.IO.Endianness.LittleEndian;
                er.Write(FileSize);
                er.Write(FileDataOffset);
                er.Write(Version);
                er.Write(Reserved);
            }
            [BinaryStringSignature("SARC")]
            [BinaryFixedSize(4)]
            public String Signature;
            [BinaryBOM(0xFFFE)]
            public UInt16 HeaderSize;
            [BinaryBOM(0xFFFE)]
            public UInt16 Endianness;
            public UInt32 FileSize;
            public UInt32 FileDataOffset;
            public UInt16 Version;
            public UInt16 Reserved;
        }

        public SFAT SFat;
        public class SFAT
        {
            public SFAT()
            {
                Signature = "SFAT";
                HeaderSize = 0xC;
                NrEntries = 0;
                HashMultiplier = 0x65;
                Entries = new List<SFATEntry>();
            }
            public SFAT(EndianBinaryReaderEx er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "SFAT") throw new SignatureNotCorrectException(Signature, "SFAT", er.BaseStream.Position - 4);
                HeaderSize = er.ReadUInt16();
                NrEntries = er.ReadUInt16();
                HashMultiplier = er.ReadUInt32();
                Entries = new List<SFATEntry>();
                for (int i = 0; i < NrEntries; i++)
                {
                    Entries.Add(new SFATEntry(er));
                }
            }
            public void Write(EndianBinaryWriterEx er)
            {
                NrEntries = (ushort)Entries.Count;
                er.Write(Signature, Encoding.ASCII, false);
                er.Write(HeaderSize);
                er.Write(NrEntries);
                er.Write(HashMultiplier);
                foreach (var v in Entries) v.Write(er);
            }

            public String Signature;
            public UInt16 HeaderSize;
            public UInt16 NrEntries;
            public UInt32 HashMultiplier;
            public List<SFATEntry> Entries;
            public class SFATEntry
            {
                public SFATEntry() { }
                public SFATEntry(EndianBinaryReaderEx er)
                {
                    er.ReadObject(this);
                }
                public void Write(EndianBinaryWriterEx er)
                {
                    er.Write(FileNameHash);
                    er.Write(FileNameOffset);
                    er.Write(FileDataStart);
                    er.Write(FileDataEnd);
                }

                public UInt32 FileNameHash;
                public UInt32 FileNameOffset;//If filenames are available
                public UInt32 FileDataStart;
                public UInt32 FileDataEnd;
            }
        }

        public SFNT SFnt;
        public class SFNT
        {
            public SFNT()
            {
                Signature = "SFNT";
                HeaderSize = 8;
                Reserved = 0;
                StringTable = new byte[0];
            }
            public SFNT(EndianBinaryReaderEx er, uint fileDataOffset)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "SFNT") throw new SignatureNotCorrectException(Signature, "SFNT", er.BaseStream.Position - 4);
                HeaderSize = er.ReadUInt16();
                Reserved = er.ReadUInt16();

                long tableSize = fileDataOffset - er.BaseStream.Position;
                if (tableSize > 0)
                {
                    StringTable = er.ReadBytes((int)tableSize);
                }
                else
                {
                    StringTable = new byte[0];
                }
            }

            public void Write(EndianBinaryWriterEx er)
            {
                er.Write(Signature, Encoding.ASCII, false);
                er.Write(HeaderSize);
                er.Write(Reserved);
            }

            public String Signature;
            public UInt16 HeaderSize;
            public UInt16 Reserved;
            public byte[] StringTable;
        }

        public byte[] Data;

        public byte[] GetFileDataByIndex(int Index)
        {
            if (SFat.Entries.Count <= Index) return null;
            SFAT.SFATEntry v = SFat.Entries[Index];
            uint start = v.FileDataStart;
            uint length = v.FileDataEnd - start;
            byte[] Result = new byte[length];
            Array.Copy(Data, start, Result, 0, length);
            return Result;
        }

        public byte[] GetFileDataByHash(UInt32 Hash)
        {
            foreach (SFAT.SFATEntry v in SFat.Entries)
            {
                if (v.FileNameHash == Hash)
                {
                    uint start = v.FileDataStart;
                    uint length = v.FileDataEnd - start;
                    byte[] Result = new byte[length];
                    Array.Copy(Data, start, Result, 0, length);
                    return Result;
                }
            }
            return null;
        }

        public UInt32 GetHashFromName(String Name)
        {
            return GetHashFromName(Name, SFat.HashMultiplier);
        }

        public static UInt32 GetHashFromName(String Name, UInt32 HashMultiplier)
        {
            uint res = 0;
            for (int i = 0; i < Name.Length; i++)
            {
                res = Name[i] + res * HashMultiplier;
            }
            return res;
        }

        public bool HasValidStringTable
        {
            get
            {
                return (SFnt != null && SFnt.StringTable != null &&
                SFnt.StringTable.Length > 0 && SFat != null && SFat.NrEntries > 0);
            }
        }

        public SFSDirectory ToFileSystem()
        {
            SFSDirectory Root = new SFSDirectory("/", true);
            Dictionary<string, SFSDirectory> directoryMap = new Dictionary<string, SFSDirectory>();
            directoryMap["/"] = Root;
            foreach (var v in SFat.Entries)
            {
                string fullPath = null;
                if (SFnt != null && SFnt.StringTable != null && SFnt.StringTable.Length > 0)
                {
                    uint nameOffset;
                    if ((v.FileNameOffset >> 16) == 0x100)
                    {
                        nameOffset = (v.FileNameOffset & 0xFFFF) * 4;
                    }
                    else
                    {
                        nameOffset = (v.FileNameOffset & 0x00FFFFFF) * 4;
                    }

                    if (nameOffset < SFnt.StringTable.Length)
                    {
                        int end = Array.IndexOf(SFnt.StringTable, (byte)0, (int)nameOffset);
                        if (end >= 0)
                        {
                            fullPath = Encoding.ASCII.GetString(
                                SFnt.StringTable,
                                (int)nameOffset,
                                end - (int)nameOffset
                            );
                        }
                    }
                }

                if (string.IsNullOrEmpty(fullPath))
                {
                    fullPath = string.Format("0x{0:X8}", v.FileNameHash);
                    if (SARCHashTable.DefaultHashTable != null)
                    {
                        var vv = SARCHashTable.DefaultHashTable.GetEntryByHash(v.FileNameHash);
                        if (vv != null) fullPath = vv.Name;
                    }
                }

                string fileName = Path.GetFileName(fullPath);
                string directoryPath = Path.GetDirectoryName(fullPath)?.Replace('\\', '/');

                if (string.IsNullOrEmpty(directoryPath))
                {
                    directoryPath = "/";
                }
                else if (!directoryPath.StartsWith("/"))
                {
                    directoryPath = "/" + directoryPath;
                }

                if (!directoryPath.EndsWith("/")) directoryPath += "/";

                SFSDirectory targetDir = Root;
                if (directoryPath != "/")
                {
                    string[] pathParts = directoryPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    string currentPath = "/";

                    foreach (string part in pathParts)
                    {
                        currentPath += part + "/";

                        if (!directoryMap.TryGetValue(currentPath, out SFSDirectory nextDir))
                        {
                            SFSDirectory parentDir = directoryMap[currentPath.Substring(0, currentPath.LastIndexOf('/', currentPath.Length - 2) + 1)];
                            nextDir = new SFSDirectory(part, false)
                            {
                                Parent = parentDir
                            };
                            parentDir.SubDirectories.Add(nextDir);
                            directoryMap[currentPath] = nextDir;
                        }
                        targetDir = nextDir;
                    }
                }
                var file = new SFSFile((int)v.FileNameHash, fileName, targetDir);
                file.Data = GetFileDataByHash(v.FileNameHash);
                targetDir.Files.Add(file);
            }
            return Root;
        }

        public void FromFileSystem(SFSDirectory Root, Endianness? targetEndianness = null)
        {
            Endianness endian;
            if (targetEndianness.HasValue)
            {
                endian = targetEndianness.Value;
            }
            else
            {

                endian = Header.Endianness == 0xFFFE ? Endianness.LittleEndian : Endianness.BigEndian;
            }
            Header = new SARCHeader();
            Header.Endianness = (endian == Endianness.LittleEndian) ? (ushort)0xFFFE : (ushort)0xFEFF;
            SFat = new SFAT();
            List<SFSFile> allFiles = new List<SFSFile>();
            GetAllFilesRecursive(Root, allFiles, "");
            SFat.NrEntries = (ushort)allFiles.Count;
            uint DataStart = 0;
            bool hasStringTable = HasValidStringTable;
            if (hasStringTable)
            {
                MemoryStream strTableStream = new MemoryStream();
                EndianBinaryWriterEx er = new EndianBinaryWriterEx(strTableStream, endian);

                foreach (var file in allFiles)
                {
                    string fullPath = GetFullPath(file);
                    uint hash = GetHashFromName(fullPath, SFat.HashMultiplier);
                    uint offset = (uint)strTableStream.Position;
                    SFat.Entries.Add(new SFAT.SFATEntry()
                    {
                        FileNameHash = hash,
                        FileNameOffset = (0x100 << 16) | (offset / 4),
                        FileDataStart = DataStart,
                        FileDataEnd = DataStart + (uint)file.Data.Length
                    });
                    byte[] strBytes = Encoding.ASCII.GetBytes(fullPath);
                    er.Write(strBytes, 0, strBytes.Length);
                    er.Write((byte)0);
                    er.WritePadding(4, 0);
                    DataStart = SFat.Entries[SFat.Entries.Count - 1].FileDataEnd;
                    er.WritePadding(128, 0);
                }

                SFnt.StringTable = strTableStream.ToArray();
                er.Close();
            }
            else
            {
                foreach (var file in allFiles)
                {
                    uint alignedDataStart = DataStart;
                    while ((alignedDataStart % 128) != 0) alignedDataStart++;
                    DataStart = alignedDataStart;

                    uint hash;
                    if (file.FileName.StartsWith("0x") && file.FileName.Length == 10)
                        hash = (uint)file.FileID;
                    else
                        hash = GetHashFromName(file.FileName, SFat.HashMultiplier);

                    SFat.Entries.Add(new SFAT.SFATEntry()
                    {
                        FileDataStart = DataStart,
                        FileDataEnd = DataStart + (uint)file.Data.Length,
                        FileNameHash = hash,
                        FileNameOffset = 0
                    });
                    DataStart += (uint)file.Data.Length;
                }
            }

            Data = new byte[DataStart];
            int i = 0;
            foreach (var file in allFiles)
            {
                Array.Copy(file.Data, 0, Data, SFat.Entries[i].FileDataStart, file.Data.Length);
                i++;
            }

            uint baseOffset = (uint)(0x14 + 0xC + SFat.NrEntries * 0x10 + 0x8);
            if (hasStringTable)
            {
                baseOffset += (uint)SFnt.StringTable.Length;
            }
            Header.FileDataOffset = baseOffset;
            while ((Header.FileDataOffset % 128) != 0) Header.FileDataOffset++;

            Header.FileSize = Header.FileDataOffset + (uint)Data.Length;
        }

        private void GetAllFilesRecursive(SFSDirectory dir, List<SFSFile> files, string currentPath)
        {
            foreach (var file in dir.Files)
            {
                files.Add(file);
            }

            foreach (var subdir in dir.SubDirectories)
            {
                string newPath = currentPath + subdir.DirectoryName + "/";
                GetAllFilesRecursive(subdir, files, newPath);
            }
        }

        private string GetFullPath(SFSFile file)
        {
            SFSDirectory dir = file.Parent;
            string path = file.FileName;

            while (dir != null && !dir.IsRoot)
            {
                path = dir.DirectoryName + "/" + path;
                dir = dir.Parent;
            }

            return path;
        }

        public class SARCIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Archives;
            }

            public override string GetFileDescription()
            {
                return "Simple Archive (SARC)";
            }

            public override string GetFileFilter()
            {
                return "Simple Archive (*.sarc, *.szs, *.bfma)|*.sarc;*.szs;*.bfma";
            }

            public override Bitmap GetIcon()
            {
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 20 && File.Data[0] == 'S' && File.Data[1] == 'A' && File.Data[2] == 'R' && File.Data[3] == 'C' && File.Data[0x14] == 'S' && File.Data[0x15] == 'F' && File.Data[0x16] == 'A' && File.Data[0x17] == 'T') return FormatMatch.Content;
                return FormatMatch.No;
            }

        }
    }
}