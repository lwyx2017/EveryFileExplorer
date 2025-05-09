using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.IO;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using _3DS.UI;
using CommonCompressors;
using System.ComponentModel;

namespace _3DS
{
    public class CBMD: FileFormat<CBMD.CBMDIdentifier> ,IViewable//, IWriteable
    {
        public CBMD(byte[] Data)
        {
            EndianBinaryReaderEx er = new EndianBinaryReaderEx(new MemoryStream(Data), Endianness.LittleEndian);
            try
            {
                Header = new CBMDHeader(er);
                RawData = Data;
            }
            finally
            {
                er.Close();
            }
        }

        //public string GetSaveDefaultFileFilter()
        //{
        //    return "CTR Banner Model Data (*.bnr, *.cbmd)|*.bnr; *.cbmd";
        //}

        //public byte[] Write()
        //{

        //}

        public Form GetDialog()
        {
            return new CBMDViewer(this);
        }

        public CBMDHeader Header;
        public class CBMDHeader
        {
            public CBMDHeader(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "CBMD") throw new SignatureNotCorrectException(Signature, "CBMD", er.BaseStream.Position - 4);
                Padding0 = er.ReadUInt32();
                CompressedCGFXOffset = er.ReadUInt32();
                Padding1 = er.ReadBytes(0x78);
                CBMDLength = er.ReadUInt32();
            }
            public void Write(EndianBinaryWriter er)
            {
                er.Write(Signature, Encoding.ASCII, false);
                er.Write(Padding0);
                er.Write(CompressedCGFXOffset);
                er.Write(Padding1, 0, Padding1.Length);
                er.Write(CBMDLength);
                   
            }
            public string Signature;
            public uint Padding0;
            public uint CompressedCGFXOffset;
            public byte[] Padding1;
            public uint CBMDLength;
        }

        private byte[] RawData;

        public byte[] GetRawCGFX()
        {
            uint startOffset = Header.CompressedCGFXOffset;
            uint length = Header.CBMDLength - startOffset;
            if (startOffset >= RawData.Length) 
                throw new InvalidDataException("Compressed CGFX offset exceeds file size.");
            if (startOffset + length > RawData.Length)
                throw new InvalidDataException("Compressed CGFX data exceeds file bounds.");
            byte[] compressedData = new byte[length];
            Array.Copy(RawData, (int)startOffset, compressedData, 0, (int)length);
            return compressedData;
        }

        public byte[] GetDecompressedCGFX()
        {
            try
            {
                byte[] compressedData = GetRawCGFX();
                if (compressedData.Length < 4 || compressedData[0] != 0x11)
                    throw new InvalidDataException("Invalid LZ11 compressed data format.");
                LZ11 decompressor = new LZ11();
                return decompressor.Decompress(compressedData);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to decompress CGFX data.", ex);
            }
        }

        public byte[] GetCWAV()
        {
            uint startOffset = Header.CBMDLength;
            if (startOffset >= RawData.Length)
                return new byte[0];
            if (startOffset + 4 > RawData.Length)
                throw new InvalidDataException("BCWAV header exceeds file bounds.");
            string signature = Encoding.ASCII.GetString(RawData, (int)startOffset, 4);
            if (signature != "CWAV")
                throw new InvalidDataException("Invalid BCWAV signature.");
            int length = RawData.Length - (int)startOffset;
            byte[] bcwav = new byte[length];
            Array.Copy(RawData, (int)startOffset, bcwav, 0, length);
            return bcwav;
        }

        public class CBMDIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return "3DS";
            }

            public override string GetFileDescription()
            {
                return "CTR Banner Model Data (CBMD)";
            }

            public override string GetFileFilter()
            {
                return "CTR Banner Model Data (*.bnr, *.cbmd)|*.bnr; *.cbmd";
            }

            public override Bitmap GetIcon()
            {
                return null;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'C' && File.Data[1] == 'B' && File.Data[2] == 'M' && File.Data[3] == 'D') return FormatMatch.Content;
                return FormatMatch.No;
            }
        }
    }
}