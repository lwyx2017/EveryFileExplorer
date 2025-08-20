using System;
using System.IO;
using LibEveryFileExplorer.Compression;
using LibEveryFileExplorer.IO;

namespace CommonCompressors
{
    public unsafe class LZ11 : CompressionFormat<LZ11.LZ11Identifier>, ICompressable
    {
        public unsafe byte[] Compress(byte[] Data)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write((byte)0x11);
                byte[] lengthBytes = new byte[3];
                IOUtil.WriteU24LE(lengthBytes, 0, (uint)Data.Length);
                bw.Write(lengthBytes, 0, 3);

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

        private void WriteCompressedBlock(BinaryWriter bw, int length, int disp)
        {
            disp -= 1;

            if (length > 0x110)
            {
                int lenVal = length - 0x111;
                bw.Write((byte)(0x10 | (lenVal >> 12)));
                bw.Write((byte)((lenVal >> 4) & 0xFF));
                bw.Write((byte)(((lenVal & 0xF) << 4) | (disp >> 8)));
                bw.Write((byte)(disp & 0xFF));
            }
            else if (length > 0x10)
            {
                int lenVal = length - 0x11;
                bw.Write((byte)(lenVal >> 4));
                bw.Write((byte)(((lenVal & 0xF) << 4) | (disp >> 8)));
                bw.Write((byte)(disp & 0xFF));
            }
            else
            {
                int lenVal = length - 1;
                bw.Write((byte)((lenVal << 4) | (disp >> 8)));
                bw.Write((byte)(disp & 0xFF));
            }
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

        private const int MaxWindowSize = 0x1000;
        private const int MaxMatchLength = 0x10110;

        public override byte[] Decompress(byte[] Data)
        {
            UInt32 leng = (uint)(Data[1] | (Data[2] << 8) | (Data[3] << 16));
            byte[] Result = new byte[leng];
            int Offs = 4;
            int dstoffs = 0;
            while (true)
            {
                byte header = Data[Offs++];
                for (int i = 0; i < 8; i++)
                {
                    if ((header & 0x80) == 0) Result[dstoffs++] = Data[Offs++];
                    else
                    {
                        byte a = Data[Offs++];
                        int offs;
                        int length;
                        if ((a >> 4) == 0)
                        {
                            byte b = Data[Offs++];
                            byte c = Data[Offs++];
                            length = (((a & 0xF) << 4) | (b >> 4)) + 0x11;
                            offs = (((b & 0xF) << 8) | c) + 1;
                        }
                        else if ((a >> 4) == 1)
                        {
                            byte b = Data[Offs++];
                            byte c = Data[Offs++];
                            byte d = Data[Offs++];
                            length = (((a & 0xF) << 12) | (b << 4) | (c >> 4)) + 0x111;
                            offs = (((c & 0xF) << 8) | d) + 1;
                        }
                        else
                        {
                            byte b = Data[Offs++];
                            offs = (((a & 0xF) << 8) | b) + 1;
                            length = (a >> 4) + 1;
                        }
                        for (int j = 0; j < length; j++)
                        {
                            Result[dstoffs] = Result[dstoffs - offs];
                            dstoffs++;
                        }
                    }
                    if (dstoffs >= leng) return Result;
                    header <<= 1;
                }
            }
        }

        public class LZ11Identifier : CompressionFormatIdentifier
        {
            public override string GetCompressionDescription()
            {
                return "LZ11";
            }

            public override bool IsFormat(byte[] Data)
            {
                return Data.Length > 4 && Data[0] == 0x11;
            }
        }
    }
}