using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.IO;
using MarioKart.UI;

namespace MarioKart.MK7
{
    public class ObjFlow : FileFormat<ObjFlow.ObjFlowIdentifier>, IViewable, IWriteable,IEmptyCreatable
    {
        public ObjFlow()
        {
            Header = new ObjFlowHeader();
            Objects = new ObjFlowEntry[0];
        }
        public ObjFlow(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new ObjFlowHeader(er);
                Objects = new ObjFlowEntry[Header.NrObjects];
                for (int i = 0; i < Header.NrObjects; i++)
                {
                    Objects[i] = new ObjFlowEntry(er);
                }
            }
            finally
            {
                er.Close();
            }
        }

        public Form GetDialog()
        {
            return new MK7ObjFlowViewer(this);
        }

        public string GetSaveDefaultFileFilter()
        {
            return "CTR ObjFlow (ObjFlow.bin)|ObjFlow.bin";
        }

        public byte[] Write()
        {
            MemoryStream m = new MemoryStream();
            EndianBinaryWriter er = new EndianBinaryWriter(m, Endianness.LittleEndian);
            Header.Write(er);
            foreach (var obj in Objects)
            {
                obj.Write(er);
            }
            byte[] result = m.ToArray();
            er.Close();
            return result;
        }

        public ObjFlowHeader Header;
        public class ObjFlowHeader
        {
            public ObjFlowHeader()
            {
                Signature = "FBOC";
                HeaderSize = 8;
                NrObjects = 0;
            }
            public ObjFlowHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "FBOC") throw new SignatureNotCorrectException(Signature, "FBOC", er.BaseStream.Position - 4);
                HeaderSize = er.ReadUInt16();
                NrObjects = er.ReadUInt16();
            }
            public void Write(EndianBinaryWriter er)
            {
                er.Write(Signature, Encoding.ASCII, false);
                er.Write(HeaderSize);
                er.Write(NrObjects);
            }
            public String Signature;
            public UInt16 HeaderSize;
            public UInt16 NrObjects;
        }
        public ObjFlowEntry[] Objects;
        public class ObjFlowEntry
        {
            public ObjFlowEntry()
            {
                ObjectID = 0;
                Unknown1 = 0;
                Unknown2 = 0;
                Unknown3 = 0;
                Unknown4 = 0;
                Unknown5 = 0;
                Unknown6 = 0;
                Unknown7 = 0;
                Unknown8 = 0;
                Unknown9 = 0;
                Unknown10 = 0;
                Unknown11 = 0;
                Name = string.Empty;
                ParticleName = string.Empty;
            }
            public ObjFlowEntry(EndianBinaryReader er)
            {
                ObjectID = er.ReadUInt16();
                Unknown1 = er.ReadUInt16();
                Unknown2 = er.ReadUInt16();
                Unknown3 = er.ReadUInt16();
                Unknown4 = er.ReadUInt32();
                Unknown5 = er.ReadUInt32();
                Unknown6 = er.ReadUInt32();
                Unknown7 = er.ReadUInt16();
                Unknown8 = er.ReadUInt16();
                Unknown9 = er.ReadUInt16();
                Unknown10 = er.ReadUInt16();
                Unknown11 = er.ReadUInt32();
                Name = er.ReadString(Encoding.ASCII, 0x40).Replace("\0", "");
                ParticleName = er.ReadString(Encoding.ASCII, 0x40).Replace("\0", "");
            }
            public void Write(EndianBinaryWriter er)
            {
                er.Write(ObjectID);
                er.Write(Unknown1);
                er.Write(Unknown2);
                er.Write(Unknown3);
                er.Write(Unknown4);
                er.Write(Unknown5);
                er.Write(Unknown6);
                er.Write(Unknown7);
                er.Write(Unknown8);
                er.Write(Unknown9);
                er.Write(Unknown10);
                er.Write(Unknown11);
                byte[] nameBytes = Encoding.ASCII.GetBytes(Name);
                byte[] nameBuffer = new byte[0x40];
                Array.Copy(nameBytes, 0, nameBuffer, 0, Math.Min(nameBytes.Length, 0x40));
                er.Write(nameBuffer);
                byte[] particleBytes = Encoding.ASCII.GetBytes(ParticleName);
                byte[] particleBuffer = new byte[0x40];
                Array.Copy(particleBytes, 0, particleBuffer, 0, Math.Min(particleBytes.Length, 0x40));
                er.Write(particleBuffer);
            }
            public UInt16 ObjectID;
            public UInt16 Unknown1;
            public UInt16 Unknown2;
            public UInt16 Unknown3;
            public UInt32 Unknown4;
            public UInt32 Unknown5;
            public UInt32 Unknown6;
            public UInt16 Unknown7;
            public UInt16 Unknown8;
            public UInt16 Unknown9;
            public UInt16 Unknown10;
            public UInt32 Unknown11;
            public String Name;
            public String ParticleName;
        }

        public void AddEntry(ObjFlowEntry newEntry)
        {
            var entriesList = new System.Collections.Generic.List<ObjFlowEntry>(Objects);
            entriesList.Add(newEntry);
            Objects = entriesList.ToArray();
            Header.NrObjects = (ushort)Objects.Length;
        }

        public void RemoveEntry(int index)
        {
            if (index < 0 || index >= Objects.Length)
                throw new ArgumentOutOfRangeException(nameof(index), "The index is beyond the valid range!");
            var entriesList = new System.Collections.Generic.List<ObjFlowEntry>(Objects);
            entriesList.RemoveAt(index);
            Objects = entriesList.ToArray();
            Header.NrObjects = (ushort)Objects.Length;
        }

        public class ObjFlowIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return "Mario Kart 7";
            }

            public override string GetFileDescription()
            {
                return "CTR ObjFlow (FBOC)";
            }

            public override string GetFileFilter()
            {
                return "CTR ObjFlow (ObjFlow.bin)|ObjFlow.bin";
            }

            public override Bitmap GetIcon()
            {
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'F' && File.Data[1] == 'B' && File.Data[2] == 'O' && File.Data[3] == 'C') return FormatMatch.Content;
                return FormatMatch.No;
            }

        }
    }
}