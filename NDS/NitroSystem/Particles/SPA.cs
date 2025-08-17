using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using NDS.UI;
using LibEveryFileExplorer.Collections;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.IO;
using NDS.GPU;

namespace NDS.NitroSystem.Particles
{
    public class SPA:FileFormat<SPA.SPAIdentifier>, IViewable
    {
        public SPA(byte[] Data)
        {
            EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new SPAHeader(er);
                if (Header.Version == "12_1")
                {
                    Particles = new Particle[Header.NrParticles];
                    for (int i = 0; i < Header.NrParticles; i++)
                    {
                        Particles[i] = new Particle(er);
                    }
                }
                else
                {
                    er.BaseStream.Position = Header.FirstParticleTextureOffset;
                }
                er.BaseStream.Position = Header.FirstParticleTextureOffset;
                ParticleTextures = new ParticleTexture[Header.NrParticleTextures];
                for (int i = 0; i < Header.NrParticleTextures; i++)
                {
                    ParticleTextures[i] = new ParticleTexture(er);
                }
            }
            finally
            {
                er.Close();
            }
        }

        public Form GetDialog()
        {
            return new SPAViewer(this);
        }

        public SPAHeader Header;
        public class SPAHeader
        {
            public SPAHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != " APS") throw new SignatureNotCorrectException(Signature, " APS", er.BaseStream.Position - 4);
                Version = er.ReadString(Encoding.ASCII, 4);
                NrParticles = er.ReadUInt16();
                NrParticleTextures = er.ReadUInt16();
                Unknown3 = er.ReadUInt32();
                Unknown4 = er.ReadUInt32();
                Unknown5 = er.ReadUInt32();
                FirstParticleTextureOffset = er.ReadUInt32();
                Padding = er.ReadUInt32();
            }
            public string Signature;
            public string Version;
            public ushort NrParticles;
            public ushort NrParticleTextures;
            public uint Unknown3;
            public uint Unknown4;
            public uint Unknown5;
            public uint FirstParticleTextureOffset;
            public uint Padding;
        }

        public class Particle
        {
            [Flags]
            public enum ParticleFlags : uint
            {
                Type0 = 0u,
                Type1 = 0x10u,
                Type2 = 0x20u,
                Type3 = 0x30u,
                Bit8 = 0x100u,
                Bit9 = 0x200u,
                Bit10 = 0x400u,
                TextureAnimation = 0x800u,
                Bit16 = 0x10000u,
                Bit21 = 0x200000u,
                Bit22 = 0x400000u,
                Bit23 = 0x800000u,
                Bit24 = 0x1000000u,
                Bit25 = 0x2000000u,
                Bit26 = 0x4000000u,
                Bit27 = 0x8000000u,
                Bit28 = 0x10000000u,
                Bit29 = 0x20000000u
            }
            public class Bit_9
            {
                public Bit_9(EndianBinaryReader er)
                {
                    Unknown1 = er.ReadUInt16();
                    Unknown2 = er.ReadUInt16();
                    Unknown3 = er.ReadUInt16();
                    Unknown4 = er.ReadUInt16();
                    Unknown5 = er.ReadUInt32();
                }
                public ushort Unknown1;
                public ushort Unknown2;
                public ushort Unknown3;
                public ushort Unknown4;
                public uint Unknown5;
            }
            public Particle(EndianBinaryReader er)
            {
                Flag = (ParticleFlags)er.ReadUInt32();
                Position = new Vector3(er.ReadSingle(), er.ReadSingle(), er.ReadSingle());
                Unknown1 = er.ReadSingle();
                Unknown2 = er.ReadSingle();
                Unknown3 = er.ReadSingle();
                Unknown4 = er.ReadUInt16();
                Unknown5 = er.ReadUInt16();
                Unknown6 = er.ReadUInt16();
                Unknown7 = er.ReadUInt16();
                Unknown8 = er.ReadUInt32();
                Unknown9 = er.ReadUInt32();
                Unknown10 = er.ReadUInt32();
                Unknown11 = er.ReadUInt16();
                Unknown12 = er.ReadBytes(6);
                Unknown13 = er.ReadUInt16();
                Unknown14 = er.ReadUInt16();
                Unknown15 = er.ReadUInt16();
                Unknown16 = er.ReadUInt16();
                Unknown17 = er.ReadBytes(4);
                Unknown18 = er.ReadByte();
                Unknown19 = er.ReadByte();
                Unknown20 = er.ReadByte();
                TextureId = er.ReadByte();
                Unknown21 = er.ReadUInt32();
                Unknown22 = er.ReadUInt32();
                Unknown23 = er.ReadUInt16();
                Unknown24 = er.ReadUInt16();
                Unknown25 = er.ReadUInt32();
                if ((Flag & ParticleFlags.Bit8) != ParticleFlags.Type0)
                {
                    Bit8 = er.ReadBytes(12);
                }
                if ((Flag & ParticleFlags.Bit9) != ParticleFlags.Type0)
                {
                    Bit9 = new Bit_9(er);
                }
                if ((Flag & ParticleFlags.Bit10) != ParticleFlags.Type0)
                {
                    Bit10 = er.ReadBytes(8);
                }
                if ((Flag & ParticleFlags.TextureAnimation) != ParticleFlags.Type0)
                {
                    TexAnim = new TextureAnimation(er);
                }
                if ((Flag & ParticleFlags.Bit16) != ParticleFlags.Type0)
                {
                    Bit16 = er.ReadBytes(20);
                }
                if ((Flag & ParticleFlags.Bit24) != ParticleFlags.Type0)
                {
                    Bit24 = er.ReadBytes(8);
                }
                if ((Flag & ParticleFlags.Bit25) != ParticleFlags.Type0)
                {
                    Bit25 = er.ReadBytes(8);
                }
                if ((Flag & ParticleFlags.Bit26) != ParticleFlags.Type0)
                {
                    Bit26 = er.ReadBytes(16);
                }
                if ((Flag & ParticleFlags.Bit27) != ParticleFlags.Type0)
                {
                    Bit27 = er.ReadBytes(4);
                }
                if ((Flag & ParticleFlags.Bit28) != ParticleFlags.Type0)
                {
                    Bit28 = er.ReadBytes(8);
                }
                if ((Flag & ParticleFlags.Bit29) != ParticleFlags.Type0)
                {
                    Bit29 = er.ReadBytes(16);
                }
            }
            public ParticleFlags Flag;
            public Vector3 Position;
            public float Unknown1;
            public float Unknown2;
            public float Unknown3;
            public ushort Unknown4;
            public ushort Unknown5;
            public ushort Unknown6;
            public ushort Unknown7;
            public uint Unknown8;
            public uint Unknown9;
            public uint Unknown10;
            public ushort Unknown11;
            public byte[] Unknown12;
            public ushort Unknown13;
            public ushort Unknown14;
            public ushort Unknown15;
            public ushort Unknown16;
            public byte[] Unknown17;
            public byte Unknown18;
            public byte Unknown19;
            public byte Unknown20;
            public byte TextureId;
            public uint Unknown21;
            public uint Unknown22;
            public ushort Unknown23;
            public ushort Unknown24;
            public uint Unknown25;
            public byte[] Bit8;
            public Bit_9 Bit9;
            public byte[] Bit10;
            public TextureAnimation TexAnim;
            public byte[] Bit16;
            public byte[] Bit24;
            public byte[] Bit25;
            public byte[] Bit26;
            public byte[] Bit27;
            public byte[] Bit28;
            public byte[] Bit29;
        }

        public class TextureAnimation
        {
            public TextureAnimation(EndianBinaryReader er)
            {
                Textures = er.ReadBytes(8);
                NrFrames = er.ReadByte();
                Unknown1 = er.ReadByte();
                Unknown2 = er.ReadUInt16();
            }
            public byte[] Textures;
            public byte NrFrames;
            public byte Unknown1;
            public ushort Unknown2;
        }

        public class ParticleTexture
        {
            public ParticleTexture(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != " TPS") throw new SignatureNotCorrectException(Signature, " TPS", er.BaseStream.Position - 4);
                TextureInfo = er.ReadUInt16();
                Width = (ushort)(8 << ((TextureInfo >> 4) & 0xF));
                Height = (ushort)(8 << ((TextureInfo >> 8) & 0xF));
                TextureFormat = (Textures.ImageFormat)(TextureInfo & 7);
                RepeatS = ((TextureInfo >> 12) & 1) == 1;
                RepeatT = ((TextureInfo >> 13) & 1) == 1;
                FlipS = ((TextureInfo >> 14) & 1) == 1;
                FlipT = ((TextureInfo >> 15) & 1) == 1;
                Unknown1 = er.ReadUInt16();
                TextureDataLength = er.ReadUInt32();
                PaletteOffset = er.ReadUInt32();
                PaletteDataLength = er.ReadUInt32();
                Unknown2 = er.ReadUInt32();
                Unknown3 = er.ReadUInt32();
                Unknown4 = er.ReadUInt32();
                ImageData = er.ReadBytes((int)TextureDataLength);
                PaletteData = er.ReadBytes((int)PaletteDataLength);
            }
            public string Signature;
            public ushort TextureInfo;
            public ushort Width;
            public ushort Height;
            public Textures.ImageFormat TextureFormat;
            public bool RepeatS;
            public bool RepeatT;
            public bool FlipS;
            public bool FlipT;
            public ushort Unknown1;
            public uint TextureDataLength;
            public uint PaletteOffset;
            public uint PaletteDataLength;
            public uint Unknown2;
            public uint Unknown3;
            public uint Unknown4;
            public byte[] ImageData;
            public byte[] PaletteData;

            public Bitmap ToBitmap()
            {
                bool firstTransparent = (Unknown1 != 0);
                return Textures.ToBitmap(ImageData,PaletteData,0,Width,Height,TextureFormat,Textures.CharFormat.BMP,firstTransparent);
            }
        }

        public Particle[] Particles;
        public ParticleTexture[] ParticleTextures;

        public class SPAIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Particles;
            }

            public override string GetFileDescription()
            {
                return "Nitro System Particles Archive (SPA)";
            }

            public override string GetFileFilter()
            {
                return "Nitro System Particles Archive (*.spa)|*.spa";
            }

            public override Bitmap GetIcon()
            {
                return Resource.water;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[1] == 'A' && File.Data[2] == 'P' && File.Data[3] == 'S') return FormatMatch.Content;
                return FormatMatch.No;
            }
        }
    }
}