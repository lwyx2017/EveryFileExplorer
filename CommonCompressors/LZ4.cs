using System;
using System.Collections.Generic;
using System.IO;
using LibEveryFileExplorer.Compression;

namespace CommonCompressors
{
    public unsafe class LZ4 : CompressionFormat<LZ4.LZ4Identifier>
    {
        public override byte[] Decompress(byte[] Data)
        {
            using (var ms = new MemoryStream(Data))
            using (var br = new BinaryReader(ms))
            {
                if (br.ReadUInt32() != 0x184D2204) throw new Exception("Invalid LZ4 Header");
                byte flg = br.ReadByte();
                int version = (flg >> 6) & 0x03;
                if (version != 1) throw new Exception("Unsupported LZ4 Version");
                bool blockIndependence = ((flg >> 5) & 1) == 1;
                bool blockChecksum = ((flg >> 4) & 1) == 1;
                bool contentSize = ((flg >> 3) & 1) == 1;
                bool contentChecksum = ((flg >> 2) & 1) == 1;
                bool dictId = (flg & 1) == 1;
                byte bd = br.ReadByte();
                int blockMaxSize = (bd >> 4) & 0x07;

                long frameContentSize = 0;
                if (contentSize)
                {
                    frameContentSize = br.ReadInt64();
                }

                uint dictionaryId = 0;
                if (dictId)
                {
                    dictionaryId = br.ReadUInt32();
                }

                br.ReadByte();
                List<byte> decompressedData = new List<byte>();
                while (true)
                {
                    uint blockSize = br.ReadUInt32();
                    if (blockSize == 0) break;

                    bool compressed = (blockSize & 0x80000000) == 0;
                    blockSize &= 0x7FFFFFFF;

                    byte[] blockData = br.ReadBytes((int)blockSize);
                    
                    if (compressed)
                    {
                        byte[] decompressedBlock = DecompressBlock(blockData);
                        decompressedData.AddRange(decompressedBlock);
                    }
                    else
                    {
                        decompressedData.AddRange(blockData);
                    }

                    if (blockChecksum)
                    {
                        br.ReadUInt32();
                    }
                }

                if (contentChecksum)
                {
                    br.ReadUInt32();
                }

                return decompressedData.ToArray();
            }
        }

        private byte[] DecompressBlock(byte[] compressedData)
        {
            List<byte> result = new List<byte>();
            int offset = 0;

            while (offset < compressedData.Length)
            {
                byte token = compressedData[offset++];
                int literalLength = token >> 4;
                int matchLength = (token & 0x0F) + 4;

                if (literalLength == 15)
                {
                    byte value;
                    do
                    {
                        value = compressedData[offset++];
                        literalLength += value;
                    } while (value == 0xFF);
                }

                for (int i = 0; i < literalLength; i++)
                {
                    result.Add(compressedData[offset++]);
                }

                if (offset >= compressedData.Length) break;
                int matchOffset = compressedData[offset++] | (compressedData[offset++] << 8);
                if (matchOffset == 0 || matchOffset > result.Count)
                    throw new Exception("Invalid match offset");

                if (matchLength == 15 + 4)
                {
                    byte value;
                    do
                    {
                        value = compressedData[offset++];
                        matchLength += value;
                    } while (value == 0xFF);
                }

                int startIndex = result.Count - matchOffset;
                for (int i = 0; i < matchLength; i++)
                {
                    result.Add(result[startIndex + i]);
                }
            }
            return result.ToArray();
        }

        public class LZ4Identifier : CompressionFormatIdentifier
        {
            public override string GetCompressionDescription()
            {
                return "LZ4";
            }

            public override bool IsFormat(byte[] Data)
            {
                return Data.Length > 4 && (Data[0] == 0x04 && Data[1] == 0x22 && Data[2] == 0x4D && Data[3] == 0x18);
            }
        }
    }
}