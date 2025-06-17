using System;
using System.Collections.Generic;
using LibEveryFileExplorer.Compression;

namespace CommonCompressors
{
    public unsafe class RLE:CompressionFormat<RLE.RLEIdentifier>, ICompressable
    {
        public unsafe byte[] Compress(byte[] Data)
        {
            if (Data.Length > 0xFFFFFF)
                throw new Exception("The file to be compressed is too big!");

            List<byte> compressedData = new List<byte>();
            byte[] buffer = new byte[130];
            int bufferIndex = 0;
            int dataIndex = 0;
            int repCount = 1;

            while (dataIndex < Data.Length)
            {
                bool foundRepetition = false;
                while (bufferIndex < buffer.Length && dataIndex < Data.Length)
                {
                    byte nextByte = Data[dataIndex++];
                    buffer[bufferIndex++] = nextByte;

                    if (bufferIndex > 1 && nextByte == buffer[bufferIndex - 2])
                        repCount++;
                    else
                        repCount = 1;

                    if (repCount > 2)
                    {
                        foundRepetition = true;
                        break;
                    }
                }

                int uncompressedCount = 0;
                if (foundRepetition)
                {
                    uncompressedCount = bufferIndex - 3;
                }
                else
                {
                    uncompressedCount = Math.Min(bufferIndex, buffer.Length - 2);
                }

                if (uncompressedCount > 0)
                {
                    compressedData.Add((byte)(uncompressedCount - 1));
                    for (int i = 0; i < uncompressedCount; i++)
                        compressedData.Add(buffer[i]);

                    for (int i = uncompressedCount; i < bufferIndex; i++)
                        buffer[i - uncompressedCount] = buffer[i];
                    bufferIndex -= uncompressedCount;
                }

                if (foundRepetition)
                {
                    while (bufferIndex < buffer.Length && dataIndex < Data.Length)
                    {
                        byte nextByte = Data[dataIndex++];
                        buffer[bufferIndex++] = nextByte;

                        if (nextByte != buffer[0]) break;
                        repCount++;
                    }

                    compressedData.Add((byte)(0x80 | (repCount - 3)));
                    compressedData.Add(buffer[0]);

                    if (repCount != bufferIndex)
                        buffer[0] = buffer[bufferIndex - 1];
                    bufferIndex -= repCount;
                    repCount = 1;
                }
            }

            if (bufferIndex > 0)
            {
                compressedData.Add((byte)(bufferIndex - 1));
                for (int i = 0; i < bufferIndex; i++)
                    compressedData.Add(buffer[i]);
            }

            byte[] result = new byte[compressedData.Count + 4];
            result[0] = 0x30;
            result[1] = (byte)(Data.Length & 0xFF);
            result[2] = (byte)((Data.Length >> 8) & 0xFF);
            result[3] = (byte)((Data.Length >> 16) & 0xFF);
            Array.Copy(compressedData.ToArray(), 0, result, 4, compressedData.Count);

            return result;
        }

        public override byte[] Decompress(byte[] Data)
        {
            int decompressedSize = Data[1] | (Data[2] << 8) | (Data[3] << 16);
            if (decompressedSize == 0 && Data.Length >= 8)
            {
                decompressedSize = Data[4] | (Data[5] << 8) | (Data[6] << 16) | (Data[7] << 24);
            }

            byte[] result = new byte[decompressedSize];
            int srcIndex = decompressedSize == 0 ? 8 : 4;
            int dstIndex = 0;

            while (dstIndex < decompressedSize)
            {
                byte flag = Data[srcIndex++];
                bool compressed = (flag & 0x80) != 0;
                int length = (flag & 0x7F) + (compressed ? 3 : 1);

                if (compressed)
                {
                    byte value = Data[srcIndex++];
                    for (int i = 0; i < length; i++)
                    {
                        if (dstIndex >= result.Length) break;
                        result[dstIndex++] = value;
                    }
                }
                else
                {
                    int copyLength = Math.Min(length, decompressedSize - dstIndex);
                    Array.Copy(Data, srcIndex, result, dstIndex, copyLength);
                    srcIndex += length;
                    dstIndex += copyLength;
                }
            }

            return result;
        }

        public class RLEIdentifier : CompressionFormatIdentifier
        {
            public override string GetCompressionDescription()
            {
                return "RLE";
            }

            public override bool IsFormat(byte[] Data)
            {
                return Data.Length > 4 && Data[0] == 0x30;
            }
        }
    }
 }