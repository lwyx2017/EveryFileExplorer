using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.IO;

namespace _3DS.NintendoWare.H3D
{
	public class BCH:FileFormat<BCH.BCHIdentifier>, IViewable
	{
        public BCH(byte[] Data)
        {
            MemoryStream ms = new MemoryStream(Data, writable: true);
            EndianBinaryReaderEx er = new EndianBinaryReaderEx(ms, Endianness.LittleEndian);
            try
            {
                Header = new BCHHeader(er);
                Content = new BCHContent(er, Header);
            }
            finally
            {
                er.Close();
                ms.Close();
            }
        }

        public Form GetDialog()
		{
			return new Form();
            //return new BCHViewer(this);
        }

		public BCHHeader Header;
        public class BCHHeader
        {
            public BCHHeader(EndianBinaryReaderEx er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "BCH\0") throw new SignatureNotCorrectException(Signature, "BCH\0", er.BaseStream.Position - 4);
                BackwardCompatibility = er.ReadByte();
                ForwardCompatibility = er.ReadByte();
                Version = er.ReadUInt16();
                MainHeaderOffset = er.ReadUInt32();
                StringTableOffset = er.ReadUInt32();
                GpuCommandsOffset = er.ReadUInt32();
                DataOffset = er.ReadUInt32();
                if (BackwardCompatibility > 0x20)
                {
                    DataExtendedOffset = er.ReadUInt32();
                }
                else
                {
                    DataExtendedOffset = 0;
                }

                RelocationTableOffset = er.ReadUInt32();
                MainHeaderLength = er.ReadUInt32();
                StringTableLength = er.ReadUInt32();
                GpuCommandsLength = er.ReadUInt32();
                DataLength = er.ReadUInt32();
                if (BackwardCompatibility > 0x20)
                {
                    DataExtendedLength = er.ReadUInt32();
                }
                else
                {
                    DataExtendedLength = 0;
                }
                RelocationTableLength = er.ReadUInt32();
                UninitializedDataSectionLength = er.ReadUInt32();
                UninitializedDescriptionSectionLength = er.ReadUInt32();
                if (BackwardCompatibility > 7)
                {
                    Flags = er.ReadUInt16();
                    AddressCount = er.ReadUInt16();
                }
                else
                {
                    Flags = 0;
                    AddressCount = 0;
                }
            }
            public String Signature;
            public Byte BackwardCompatibility;
            public Byte ForwardCompatibility;
            public UInt16 Version;
            public UInt32 MainHeaderOffset;
            public UInt32 StringTableOffset;
            public UInt32 GpuCommandsOffset;
            public UInt32 DataOffset;
            public UInt32 DataExtendedOffset;
            public UInt32 RelocationTableOffset;
            public UInt32 MainHeaderLength;
            public UInt32 StringTableLength;
            public UInt32 GpuCommandsLength;
            public UInt32 DataLength;
            public UInt32 DataExtendedLength;
            public UInt32 RelocationTableLength;
            public UInt32 UninitializedDataSectionLength;
            public UInt32 UninitializedDescriptionSectionLength;
            public UInt16 Flags;
            public UInt16 AddressCount;
            public bool HasExtendedData
            {
                get { return BackwardCompatibility > 0x20; }
            }
        }

        public BCHContent Content;
        public class BCHContent
        {
            public BCHContent(EndianBinaryReaderEx er, BCHHeader header)
            {
                ProcessRelocationTable(er, header);
                er.BaseStream.Position = header.MainHeaderOffset;
                ModelsTable = new ContentTableEntry(er);
                MaterialsTable = new ContentTableEntry(er);
                ShadersTable = new ContentTableEntry(er);
                TexturesTable = new ContentTableEntry(er);
                MaterialsLUTTable = new ContentTableEntry(er);
                LightsTable = new ContentTableEntry(er);
                CamerasTable = new ContentTableEntry(er);
                FogsTable = new ContentTableEntry(er);
                SkeletalAnimationsTable = new ContentTableEntry(er);
                MaterialAnimationsTable = new ContentTableEntry(er);
                VisibilityAnimationsTable = new ContentTableEntry(er);
                LightAnimationsTable = new ContentTableEntry(er);
                CameraAnimationsTable = new ContentTableEntry(er);
                FogAnimationsTable = new ContentTableEntry(er);
                SceneTable = new ContentTableEntry(er);
                ReadModels(er, header);
                ReadMaterials(er, header);
                ReadTextures(er, header);
                //ReadMaterialsLUTTable(er, header);
                //ReadLights(er, header);
                //ReadSkeletalAnimations(er, header);
                //ReadMaterialAnimations(er, header);
                //ReadVisibilityAnimations(er, header);
            }
            public ContentTableEntry ModelsTable;
            public ContentTableEntry MaterialsTable;
            public ContentTableEntry ShadersTable;
            public ContentTableEntry TexturesTable;
            public ContentTableEntry MaterialsLUTTable;
            public ContentTableEntry LightsTable;
            public ContentTableEntry CamerasTable;
            public ContentTableEntry FogsTable;
            public ContentTableEntry SkeletalAnimationsTable;
            public ContentTableEntry MaterialAnimationsTable;
            public ContentTableEntry VisibilityAnimationsTable;
            public ContentTableEntry LightAnimationsTable;
            public ContentTableEntry CameraAnimationsTable;
            public ContentTableEntry FogAnimationsTable;
            public ContentTableEntry SceneTable;

            public List<BCHModel> Models = new List<BCHModel>();
            public List<BCHMaterial> Materials = new List<BCHMaterial>();
            public List<BCHTexture> Textures = new List<BCHTexture>();

            public class ContentTableEntry
            {
                public ContentTableEntry(EndianBinaryReaderEx er)
                {
                    Offset = er.ReadUInt32();
                    NrEntries = er.ReadUInt32();
                    NameOffset = er.ReadUInt32();
                }
                public UInt32 Offset;
                public UInt32 NrEntries;
                public UInt32 NameOffset;
            }

            private void ProcessRelocationTable(EndianBinaryReaderEx er, BCHHeader header)
            {
                if (header.RelocationTableLength == 0 || header.RelocationTableOffset == 0) return;
                long originalPosition = er.BaseStream.Position;
                EndianBinaryWriterEx ew = new EndianBinaryWriterEx(er.BaseStream, er.Endianness);
                try
                {
                    er.BaseStream.Position = header.RelocationTableOffset;
                    for (uint i = 0; i < header.RelocationTableLength / 4; i++)
                    {
                        uint value = er.ReadUInt32();
                        uint offset = value & 0x1FFFFFF;
                        byte flags = (byte)(value >> 25);
                        uint targetOffset = 0;
                        uint correctedValue = 0;
                        switch (flags)
                        {
                            case 0:
                                targetOffset = (offset * 4) + header.MainHeaderOffset;
                                er.BaseStream.Position = targetOffset;
                                correctedValue = er.ReadUInt32() + header.MainHeaderOffset;
                                break;
                            case 1:
                                targetOffset = offset + header.MainHeaderOffset;
                                er.BaseStream.Position = targetOffset;
                                correctedValue = er.ReadUInt32() + header.StringTableOffset;
                                break;
                            case 2:
                                targetOffset = (offset * 4) + header.MainHeaderOffset;
                                er.BaseStream.Position = targetOffset;
                                correctedValue = er.ReadUInt32() + header.GpuCommandsOffset;
                                break;
                            case 7:
                            case 0xC:
                                targetOffset = (offset * 4) + header.MainHeaderOffset;
                                er.BaseStream.Position = targetOffset;
                                correctedValue = er.ReadUInt32() + header.DataOffset;
                                break;
                            case 0x25:
                            case 0x26:
                                targetOffset = (offset * 4) + header.GpuCommandsOffset;
                                er.BaseStream.Position = targetOffset;
                                correctedValue = er.ReadUInt32() + header.DataOffset;
                                break;
                            case 0x27:
                                targetOffset = (offset * 4) + header.GpuCommandsOffset;
                                er.BaseStream.Position = targetOffset;
                                correctedValue = ((er.ReadUInt32() + header.DataOffset) & 0x7FFFFFFF) | 0x80000000;
                                break;
                            case 0x28:
                                targetOffset = (offset * 4) + header.GpuCommandsOffset;
                                er.BaseStream.Position = targetOffset;
                                correctedValue = (er.ReadUInt32() + header.DataOffset) & 0x7FFFFFFF;
                                break;
                            case 0x2B:
                            case 0x2C:
                            case 0x2D:
                                if (!header.HasExtendedData) continue;
                                targetOffset = (offset * 4) + header.GpuCommandsOffset;
                                er.BaseStream.Position = targetOffset;
                                correctedValue = er.ReadUInt32() + header.DataExtendedOffset;
                                if (flags == 0x2C) correctedValue |= 0x80000000;
                                if (flags == 0x2D) correctedValue &= 0x7FFFFFFF; 
                                break;
                            default: continue;
                        }

                        if (targetOffset > 0 && targetOffset < er.BaseStream.Length)
                        {
                            ew.BaseStream.Position = targetOffset;
                            ew.Write(correctedValue);
                            er.BaseStream.Position = header.RelocationTableOffset + (i + 1) * 4;
                        }
                    }
                }
                finally
                {
                    er.BaseStream.Position = originalPosition;
                }
            }

            public class BCHModel
            {
                public BCHModel(EndianBinaryReaderEx er, BCHHeader header)
                {
                    Flags = er.ReadByte();
                    SkeletonScalingType = er.ReadByte();
                    SilhouetteMaterialEntries = er.ReadUInt16();

                    WorldTransform = new float[12];
                    for (int i = 0; i < 12; i++)
                    {
                        WorldTransform[i] = er.ReadSingle();
                    }

                    MaterialsTableOffset = er.ReadUInt32();
                    MaterialsTableEntries = er.ReadUInt32();
                    MaterialsNameOffset = er.ReadUInt32();
                    VerticesTableOffset = er.ReadUInt32();
                    VerticesTableEntries = er.ReadUInt32();

                    if (header.BackwardCompatibility > 6)
                        er.BaseStream.Position += 0x28;
                    else
                        er.BaseStream.Position += 0x20;

                    SkeletonOffset = er.ReadUInt32();
                    SkeletonEntries = er.ReadUInt32();
                    SkeletonNameOffset = er.ReadUInt32();
                    ObjectsNodeVisibilityOffset = er.ReadUInt32();
                    ObjectsNodeCount = er.ReadUInt32();
                    ModelName = ReadString(er, er.ReadUInt32());
                    ObjectsNodeNameEntries = er.ReadUInt32();
                    ObjectsNodeNameOffset = er.ReadUInt32();
                    er.ReadUInt32();
                    MetaDataPointerOffset = er.ReadUInt32();
                }
                public byte Flags;
                public byte SkeletonScalingType;
                public ushort SilhouetteMaterialEntries;
                public float[] WorldTransform;
                public uint MaterialsTableOffset;
                public uint MaterialsTableEntries;
                public uint MaterialsNameOffset;
                public uint VerticesTableOffset;
                public uint VerticesTableEntries;
                public uint SkeletonOffset;
                public uint SkeletonEntries;
                public uint SkeletonNameOffset;
                public uint ObjectsNodeVisibilityOffset;
                public uint ObjectsNodeCount;
                public string ModelName;
                public uint ObjectsNodeNameEntries;
                public uint ObjectsNodeNameOffset;
                public uint MetaDataPointerOffset;

                public List<BCHMaterial> Materials = new List<BCHMaterial>();
                //public List<BCHBone> Skeleton = new List<BCHBone>();
                //public List<BCHMesh> Meshes = new List<BCHMesh>();

                private string ReadString(EndianBinaryReaderEx er, uint offset)
                {
                    if (offset == 0) return null;
                    long currentPos = er.BaseStream.Position;
                    er.BaseStream.Position = offset;
                    string result = er.ReadStringNT(Encoding.UTF8);
                    er.BaseStream.Position = currentPos;
                    return result;
                }
            }

            private void ReadModels(EndianBinaryReaderEx er, BCHHeader header)
            {

            }

            public class BCHMaterial
            {

            }

            private void ReadMaterials(EndianBinaryReaderEx er, BCHHeader header)
            {

            }

            public class BCHTexture
            {

            }


            private void ReadTextures(EndianBinaryReaderEx er, BCHHeader header)
            {

            }
        }

        public class BCHIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Models;
			}

			public override string GetFileDescription()
			{
				return "Binary CTR H3D Resource (BCH)";
			}

			public override string GetFileFilter()
			{
				return "Binary CTR H3D Resource (*.bch)|*.bch";
			}

			public override Bitmap GetIcon()
			{
				return Resource.leaf;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'B' && File.Data[1] == 'C' && File.Data[2] == 'H' && File.Data[3] == 0) return FormatMatch.Content;
				return FormatMatch.No;
			}
		}
	}
}