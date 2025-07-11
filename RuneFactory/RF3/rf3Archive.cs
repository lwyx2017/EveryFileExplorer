/*The analysis of rf3Archive has been completed, 
but it may cause the program to freeze when processing a Archive containing a large number of files, 
therefore it has been temporarily commented out.

using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.Files.SimpleFileSystem;
using LibEveryFileExplorer.IO;
using RuneFactory.UI;
using System.Linq;

namespace RuneFactory.RF3
{
    public class rf3Archive : FileFormat<rf3Archive.rf3ArchiveIdentifier>, IViewable
    {
        public rf3Archive(byte[] Data)
        {
            EndianBinaryReaderEx er = new EndianBinaryReaderEx(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new rf3ArchiveHeader(er);
                Entries = new rf3FileTableEntry[Header.FileCount];
                for (int i = 0; i < Header.FileCount; i++)
                {
                    Entries[i] = new rf3FileTableEntry(er);
                }
                data = Data;
            }
            finally
            {
                er.Close();
            }
        }

        public Form GetDialog()
        {
            return new Form();
            //return new rf3ArchiveViewer(this);
        }

        public rf3ArchiveHeader Header;
        public class rf3ArchiveHeader
        {
            public rf3ArchiveHeader(EndianBinaryReaderEx er)
            {
                FileEntries = er.ReadUInt16();
                FileEntryLength = er.ReadUInt16();
                Padding = er.ReadUInt32();
                FileCount = er.ReadUInt32();
            }
            public UInt16 FileEntries;
            public UInt16 FileEntryLength;
            public UInt32 Padding;
            public UInt32 FileCount;
        }

        public rf3FileTableEntry[] Entries;
        public class rf3FileTableEntry
        {
            public rf3FileTableEntry(EndianBinaryReaderEx er)
            {
                RelativeOffset = er.ReadUInt32();
                Size = er.ReadUInt32();
            }
            public UInt32 RelativeOffset;
            public UInt32 Size;
        }

        private byte[] data;

        public SFSDirectory ToFileSystem()
        {
            SFSDirectory root = new SFSDirectory("/", true);
            uint dataStart = 0x0C + (Header.FileCount * 8);

            for (int i = 0; i < Header.FileCount; i++)
            {
                rf3FileTableEntry entry = Entries[i];
                uint absoluteOffset = dataStart + entry.RelativeOffset;
                string fileName = $"{i.ToString("D4")}.bin";
                if (absoluteOffset + entry.Size > (uint)data.Length)
                {
                    fileName = $"INVALID_{i.ToString("D4")}.bin";
                    SFSFile invalidFile = new SFSFile(-1, fileName, root)
                    {
                        Data = Encoding.ASCII.GetBytes(
                            $"Invalid file data: Offset 0x{absoluteOffset:X}, Size 0x{entry.Size:X}")
                    };
                    root.Files.Add(invalidFile); continue;
                }
                SFSFile file = new SFSFile(-1, fileName, root);
                file.Data = new byte[entry.Size];
                Buffer.BlockCopy(data, (int)absoluteOffset, file.Data, 0, (int)entry.Size);
                root.Files.Add(file);
            }
            root.Files = root.Files.OrderBy(f => int.Parse(f.FileName.Substring(0, 4))).ToList();
            return root;
        }

        public class rf3ArchiveIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Archives;
            }

            public override string GetFileDescription()
            {
                return "Rune Factory 3 Archive (rf3Archive.arc)";
            }

            public override string GetFileFilter()
            {
                return "Rune Factory 3 Archive (rf3Archive.arc)|rf3Archive.arc";
            }

            public override Bitmap GetIcon()
            {
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Name.Equals("rf3Archive.arc"))
                    return FormatMatch.Content;
                else
                    return FormatMatch.No;
            }
        }
    }
}*/