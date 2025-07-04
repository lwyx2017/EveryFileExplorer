using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.Files.SimpleFileSystem;
using LibEveryFileExplorer.IO;
using RuneFactory.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RuneFactory
{
    public class FBTI:FileFormat<FBTI.FBTIIdentifier>,IViewable
    {
        public FBTI(byte[] Data)
        {
            EndianBinaryReaderEx er = new EndianBinaryReaderEx(new MemoryStream(Data), Endianness.BigEndian);
            try
            {
                Header = new FBTIHeader(er);
                Entries = new FBTIFileTableEntry[Header.FileCount];
                for (int i = 0; i < Header.FileCount; i++)
                {
                    Entries[i] = new FBTIFileTableEntry(er);
                }
                this.Data = Data;
            }
            finally
            {
                er.Close();
            }
        }

        public Form GetDialog()
        {
            return new FBTIViewer(this);
        }

        public FBTIHeader Header;
        public class FBTIHeader
        {
            public FBTIHeader(EndianBinaryReaderEx er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "FBTI") throw new SignatureNotCorrectException(Signature, "FBTI", er.BaseStream.Position - 4);
                Version = er.ReadString(Encoding.ASCII, 4);
                FileCount = er.ReadUInt32();
                StartOffset = er.ReadUInt32();

            }
            public String Signature;
            public String Version;
            public UInt32 FileCount;
            public UInt32 StartOffset;
        }

        public FBTIFileTableEntry[] Entries;
        public class FBTIFileTableEntry
        {
            public FBTIFileTableEntry(EndianBinaryReaderEx er)
            {
                DataOffset = er.ReadUInt32();
                Size = er.ReadUInt32();
            }
            public UInt32 DataOffset; 
            public UInt32 Size;
        }

        public byte[] Data;

        public SFSDirectory ToFileSystem()
        {
            SFSDirectory root = new SFSDirectory("/", true);
            for (int i = 0; i < Header.FileCount; i++)
            {
                FBTIFileTableEntry entry = Entries[i];
                string fileName = $"{i.ToString("D5")}.bin";
                uint dataOffset = entry.DataOffset;
                if (dataOffset + entry.Size > (uint)Data.Length)
                {
                    fileName = $"INVALID_{i.ToString("D5")}.bin";
                    SFSFile invalidFile = new SFSFile(-1, fileName, root)
                    {
                        Data = Encoding.ASCII.GetBytes($"Invalid file data: Offset 0x{dataOffset:X}, Size 0x{entry.Size:X}")
                    };
                    root.Files.Add(invalidFile); continue;
                }
                SFSFile file = new SFSFile(-1, fileName, root);
                file.Data = new byte[entry.Size];
                Buffer.BlockCopy(Data,(int)dataOffset,file.Data,0,(int)entry.Size);
                root.Files.Add(file);
            }
            root.Files = root.Files.OrderBy(f => int.Parse(f.FileName.Substring(0, 5))).ToList();
            return root;
        }

        public class FBTIIdentifier:FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Archives;
            }

            public override string GetFileDescription()
            {
                return "Rune Factory Wii FBTI Archives (FBTI)";
            }

            public override string GetFileFilter()
            {
                return "Rune Factory Wii FBTI Archives (*.fbti)|*.fbti";
            }

            public override Bitmap GetIcon()
            {
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'F' && File.Data[1] == 'B' && File.Data[2] == 'T' && File.Data[3] == 'I') return FormatMatch.Content;
                return FormatMatch.No;
            }
        }
    }
}