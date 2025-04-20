using LibEveryFileExplorer.Compression;
using System;
using System.IO;

namespace CommonCompressors
{
    public unsafe class LZ10 : CompressionFormat<LZ10.LZ10Identifier>, ICompressable
    {
        public override byte[] Decompress(byte[] Data)
        {
            using (var ms = new MemoryStream(Data))
            using (var br = new BinaryReader(ms))
            {
                int header = br.ReadInt32();
                int decompressedSize = header >> 8;
                
                byte[] result = new byte[decompressedSize];
                int destOffset = 0;
                int flags = 0, mask = 1;
                byte[] buffer = new byte[MaxWindowSize];
                int bufferPos = 0;

                while (destOffset < decompressedSize)
                {
                    if (mask == 1)
                    {
                        if (ms.Position >= ms.Length) break;
                        flags = br.ReadByte();
                        mask = 0x80;
                    }
                    else
                    {
                        mask >>= 1;
                    }

                    if ((flags & mask) != 0)
                    {
                        byte byte1 = br.ReadByte();
                        byte byte2 = br.ReadByte();

                        int length = (byte1 >> 4) + 3;
                        int disp = ((byte1 & 0x0F) << 8) | byte2;
                        disp += 1;

                        int srcPos = destOffset - disp;
                        for (int i = 0; i < length; i++)
                        {
                            byte val = result[srcPos + i];
                            result[destOffset++] = val;
                            buffer[bufferPos] = val;
                            bufferPos = (bufferPos + 1) % MaxWindowSize;
                        }
                    }
                    else
                    {
                        byte val = br.ReadByte();
                        result[destOffset++] = val;
                        buffer[bufferPos] = val;
                        bufferPos = (bufferPos + 1) % MaxWindowSize;
                    }
                }

                return result;
            }
        }

        public unsafe byte[] Compress(byte[] Data)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                int header = (Data.Length << 8) | 0x10;
                bw.Write(header);

                fixed (byte* dataPtr = Data)
                {
                    int position = 0;
                    byte currentHeader = 0;
                    int headerPosition = (int)ms.Position;
                    bw.Write((byte)0);
                    int headerBit = 0;

                    while (position < Data.Length)
                    {
                        if (headerBit == 8)
                        {
                            ms.Position = headerPosition;
                            bw.Write(currentHeader);
                            ms.Position = ms.Length;
                            headerPosition = (int)ms.Position;
                            bw.Write((byte)0);
                            currentHeader = 0;
                            headerBit = 0;
                        }

                        int disp;
                        int matchLen = FindBestMatch(dataPtr, Data.Length, position, out disp);

                        if (matchLen >= 3)
                        {
                            currentHeader |= (byte)(1 << (7 - headerBit));
                            WriteCompressedBlock(bw, matchLen, disp);
                            position += matchLen;
                        }
                        else
                        {
                            bw.Write(dataPtr[position]);
                            position++;
                        }

                        headerBit++;
                    }
                    ms.Position = headerPosition;
                    bw.Write(currentHeader);
                }

                return ms.ToArray();
            }
        }

        private const int MaxWindowSize = 0x1000;
        private const int MaxMatchLength = 0x12;

        private void WriteCompressedBlock(BinaryWriter bw, int length, int disp)
        {
            disp -= 1;
            length -= 3;
            byte byte1 = (byte)((length << 4) | (disp >> 8));
            byte byte2 = (byte)(disp & 0xFF);
            bw.Write(byte1);
            bw.Write(byte2);
        }

        private unsafe int FindBestMatch(byte* data, int dataLen, int position, out int bestDisp)
        {
            bestDisp = 0;
            int maxLen = 0;
            int start = Math.Max(0, position - MaxWindowSize);
            int remaining = dataLen - position;
            int maxPossibleLen = Math.Min(remaining, MaxMatchLength);

            for (int disp = position - start; disp >= 1; disp--)
            {
                int currentLen = 0;
                while (currentLen < maxPossibleLen &&
                       data[position + currentLen] == data[position - disp + currentLen])
                {
                    currentLen++;
                }

                if (currentLen > maxLen)
                {
                    maxLen = currentLen;
                    bestDisp = disp;
                    if (maxLen == maxPossibleLen) break;
                }
            }

            return maxLen;
        }

        public class LZ10Identifier : CompressionFormatIdentifier
        {
            public override string GetCompressionDescription()
            {
                return "LZ10";
            }

            public override bool IsFormat(byte[] Data)
            {
                return Data.Length > 4 && (Data[0] & 0xF0) == 0x10;
            }
        }
    }
}