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
	public class DARC : FileFormat<DARC.darcIdentifier>, IViewable, IWriteable
    {
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
				FileNameTable = new Dictionary<uint,string>();
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

		public class darcIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Archives;
			}

			public override string GetFileDescription()
			{
				return "Data Archive (darc)";
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
