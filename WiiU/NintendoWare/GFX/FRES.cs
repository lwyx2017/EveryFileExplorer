using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.IO;

namespace WiiU.NintendoWare.GFX
{
	public class FRES:FileFormat<FRES.FRESIdentifier>,IViewable
	{
        public FRES(byte[] Data)
        {
            EndianBinaryReaderEx er = new EndianBinaryReaderEx(new MemoryStream(Data), Endianness.BigEndian);
            try
            {
                Header = new FRESHeader(er);
            }
            finally
            {
                er.Close();
            }
        }

        public Form GetDialog()
        {
            return new Form();
        }

        public FRESHeader Header;
        public class FRESHeader
        {
            public FRESHeader(EndianBinaryReaderEx er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "FRES") throw new SignatureNotCorrectException(Signature, "FRES", er.BaseStream.Position - 4);
                Version = er.ReadUInt32();
                Endianness = er.ReadUInt16();
                HeaderSize = er.ReadUInt16();
                FileSize = er.ReadUInt32();
                Alignment = er.ReadUInt32();
                FileNameOffset = er.ReadUInt32();
                StringTableSize = er.ReadUInt32();
                StringTableOffset = er.ReadUInt32();
                FileOffsets = er.ReadUInt32();
                FileCounts = er.ReadUInt16();
                UserPointer = er.ReadUInt16();
            }
            public string Signature;
            public UInt32 Version;
            public UInt16 Endianness;
            public UInt16 HeaderSize;
            public UInt32 FileSize;
            public UInt32 Alignment;
            public UInt32 FileNameOffset;
            public UInt32 StringTableSize;
            public UInt32 StringTableOffset;
            public UInt32 FileOffsets;
            public UInt16 FileCounts;
            public UInt16 UserPointer;
        }

        //public FMDL[] Models;
        //public FTEX[] Textures;
        //public FSKA[] SkeletonAnimations;
        //public FSHU[] ShaderParameters;
        //public FSHU[] ColorAnimation;
        //public FSHU[] TextureSRTAnimation;
        //public FTXP[] TexturePatternAnimation;
        //public FVIS[] BoneVisibilityAnimation;
        //public FVIS[] MaterialVisibilityAnimation;
        //public FSHA[] ShapeAnimation;
        //public FSCN[] SceneAnimation;
        //public Embedded[] EmbeddedFiles;

        public class FRESIdentifier:FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Models;

            }

            public override string GetFileDescription()
            {
                return "Cafe Graphics (FRES)";
            }

            public override string GetFileFilter()
            {
                return "Cafe Graphics (*.bfres, *.bftex, *.bfmdl, *.bfmat, *.bfmts, *.bfskl)|*.bfres;*.bftex;*.bfmdl;*.bfmat;*.bfmts;*.bfskl";
            }

            public override Bitmap GetIcon()
            {
                return Resource.leaf;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'F' && File.Data[1] == 'R' && File.Data[2] == 'E' && File.Data[3] == 'S') return FormatMatch.Content;
                return FormatMatch.No;
            }

        }
	}
}
