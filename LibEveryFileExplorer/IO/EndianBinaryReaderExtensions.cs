using LibEveryFileExplorer.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace LibEveryFileExplorer.IO
{
    public static class EndianBinaryReaderExtensions
    {
        public static Dictionary<string, long> Markers = new Dictionary<string, long>();

        public static void SetMarkerOnCurrentOffset(this EndianBinaryReader er, string name)
        {
            Markers.Add(name, er.BaseStream.Position);
        }

        public static void SetMarker(this EndianBinaryReader er, string name, long offset)
        {
            Markers.Add(name, offset);
        }

        public static long GetMarker(this EndianBinaryReader er, string name)
        {
            return Markers[name];
        }

        public static void RemoveMarker(this EndianBinaryReader er, string name)
        {
            Markers.Remove(name);
        }

        public static void ClearMarkers(this EndianBinaryReader er)
        {
            Markers.Clear();
        }

        public static int ReadVariableLength(this EndianBinaryReader er)
        {
            byte b = er.ReadByte();
            int value = b & 0x7F;
            while (er.BaseStream.Position < er.BaseStream.Length && (b & 0x80) != 0)
            {
                value <<= 7;
                b = er.ReadByte();
                value |= b & 0x7F;
            }
            return value;
        }

        public static Color ReadColor4Singles(this EndianBinaryReader er)
        {
            int red = (int)(er.ReadSingle() * 255);
            int green = (int)(er.ReadSingle() * 255);
            int blue = (int)(er.ReadSingle() * 255);
            int alpha = (int)(er.ReadSingle() * 255);
            return Color.FromArgb(alpha, red, green, blue);
        }

        public static Color ReadColor16(this EndianBinaryReader er)
        {
            short r = er.ReadInt16();
            short g = er.ReadInt16();
            short b = er.ReadInt16();
            short a = er.ReadInt16();
            return Color.FromArgb(System.Math.Abs(a) & 0xFF, System.Math.Abs(r) & 0xFF, System.Math.Abs(g) & 0xFF, System.Math.Abs(b) & 0xFF);
        }

        public static Color ReadColor8(this EndianBinaryReader er)
        {
            int red = er.ReadByte();
            int green = er.ReadByte();
            int blue = er.ReadByte();
            int alpha = er.ReadByte();
            return Color.FromArgb(alpha, red, green, blue);
        }

        private static int smfGetVarLengthSize(int value)
        {
            int size = 1;
            int temp = value;
            while (temp > 127 && size < 4)
            {
                size++;
                temp >>= 7;
            }
            return size;
        }

        public static unsafe long IndexOf(this byte[] haystack, byte[] needle)
        {
            fixed (byte* hPtr = haystack)
            fixed (byte* nPtr = needle)
            {
                long index = 0;
                byte* hCurrent = hPtr;
                byte* hEnd = hPtr + haystack.LongLength;

                for (; hCurrent < hEnd; hCurrent++)
                {
                    bool match = true;
                    byte* hTemp = hCurrent;
                    byte* nTemp = nPtr;
                    byte* nEnd = nPtr + needle.LongLength;

                    while (match && nTemp < nEnd)
                    {
                        match = *nTemp == *hTemp;
                        nTemp++;
                        hTemp++;
                    }

                    if (match)
                        return index;

                    index++;
                }

                return -1;
            }
        }

        public static unsafe List<long> IndexesOf(this byte[] haystack, byte[] needle)
        {
            List<long> indexes = new List<long>();

            fixed (byte* hPtr = haystack)
            fixed (byte* nPtr = needle)
            {
                long index = 0;
                byte* hCurrent = hPtr;
                byte* hEnd = hPtr + haystack.LongLength;

                for (; hCurrent < hEnd; hCurrent++)
                {
                    bool match = true;
                    byte* hTemp = hCurrent;
                    byte* nTemp = nPtr;
                    byte* nEnd = nPtr + needle.LongLength;

                    while (match && nTemp < nEnd)
                    {
                        match = *nTemp == *hTemp;
                        nTemp++;
                        hTemp++;
                    }

                    if (match)
                        indexes.Add(index);

                    index++;
                }
            }

            return indexes;
        }

        public static Vector3 ToVector3(this Color c)
        {
            return new Vector3((float)c.R / 255, (float)c.G / 255, (float)c.B / 255);
        }
    }
}