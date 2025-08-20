using System.IO;
using System.IO.Compression;
using LibEveryFileExplorer.Compression;
using LibEveryFileExplorer.IO;

namespace CommonCompressors
{
    public unsafe class Zlib : CompressionFormat<Zlib.ZlibIdentifier>, ICompressable
    {
        public override byte[] Decompress(byte[] Data)
        {
            using (var inputStream = new MemoryStream(Data))
            using (var outputStream = new MemoryStream())
            {
                inputStream.Seek(2, SeekOrigin.Begin);

                using (var deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress, true))
                {
                    deflateStream.CopyTo(outputStream);
                }

                return outputStream.ToArray();
            }
        }

        public unsafe byte[] Compress(byte[] Data)
        {
            using (var outputStream = new MemoryStream())
            {
                outputStream.WriteByte(0x78);
                outputStream.WriteByte(0x9C);
                long compressedDataStart = outputStream.Position;

                using (var deflateStream = new DeflateStream(outputStream, CompressionLevel.Optimal, true))
                {
                    deflateStream.Write(Data, 0, Data.Length);
                }

                uint adler = CalculateAdler32(Data);

                outputStream.Position = outputStream.Length;

                byte[] adlerBytes = new byte[4];
                IOUtil.WriteU32BE(adlerBytes, 0, adler);
                outputStream.Write(adlerBytes, 0, 4);

                return outputStream.ToArray();
            }
        }

        private uint CalculateAdler32(byte[] data)
        {
            uint a = 1, b = 0;
            const uint mod = 65521;

            for (int i = 0; i < data.Length; i++)
            {
                a = (a + data[i]) % mod;
                b = (b + a) % mod;
            }

            return (b << 16) | a;
        }

        public class ZlibIdentifier : CompressionFormatIdentifier
        {
            public override string GetCompressionDescription()
            {
                return "Zlib";
            }

            public override bool IsFormat(byte[] Data)
            {
                return Data.Length > 2 && Data[0] == 0x78 &&(Data[1] == 0x01 || Data[1] == 0x9C || Data[1] == 0xDA);
            }
        }
    }
}