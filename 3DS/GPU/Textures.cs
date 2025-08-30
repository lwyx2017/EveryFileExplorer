using System;
using System.Drawing;
using System.Drawing.Imaging;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.GFX;

namespace _3DS.GPU
{
    public class Textures
    {
        public enum ImageFormat : uint
        {
            RGBA8 = 0,
            RGB8 = 1,
            RGBA5551 = 2,
            RGB565 = 3,
            RGBA4 = 4,
            LA8 = 5,
            HILO8 = 6,
            L8 = 7,
            A8 = 8,
            LA4 = 9,
            L4 = 10,
            A4 = 11,
            ETC1 = 12,
            ETC1A4 = 13
        }

        private static readonly int[] Bpp = { 32, 24, 16, 16, 16, 16, 16, 8, 8, 8, 4, 4, 4, 8 };

        private static readonly int[] TileOrder =
        {
             0,  1,   4,  5,
             2,  3,   6,  7,
             8,  9,  12, 13,
            10, 11,  14, 15
        };

        private static readonly int[,] ETC1Modifiers =
        {
            { 2, 8 },
            { 5, 17 },
            { 9, 29 },
            { 13, 42 },
            { 18, 60 },
            { 24, 80 },
            { 33, 106 },
            { 47, 183 }
        };

        public static int GetBpp(ImageFormat Format) { return Bpp[(uint)Format]; }

        public static Bitmap ToBitmap(byte[] Data, int Width, int Height, ImageFormat Format, bool ExactSize = false)
        {
            return ToBitmap(Data, 0, Width, Height, Format, ExactSize);
        }

        public static unsafe Bitmap ToBitmap(byte[] Data, int Offset, int Width, int Height, ImageFormat Format, bool ExactSize = false)
        {
            if (Data == null || Data.Length < 1 || Offset < 0 || Offset >= Data.Length || Width < 1 || Height < 1) return null;
            if (ExactSize && ((Width % 8) != 0 || (Height % 8) != 0)) return null;
            int physicalwidth = Width;
            int physicalheight = Height;
            if (!ExactSize)
            {
                Width = 1 << (int)Math.Ceiling(Math.Log(Width, 2));
                Height = 1 << (int)Math.Ceiling(Math.Log(Height, 2));
            }
            Bitmap bitm = new Bitmap(physicalwidth, physicalheight);
            BitmapData d = bitm.LockBits(new Rectangle(0, 0, bitm.Width, bitm.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            uint* res = (uint*)d.Scan0;
            int offs = Offset;//0;
            int stride = d.Stride / 4;
            switch (Format)
            {
                case ImageFormat.RGBA8:
                    for (int y = 0; y < Height; y += 8)
                    {
                        for (int x = 0; x < Width; x += 8)
                        {
                            for (int i = 0; i < 64; i++)
                            {
                                int x2 = i % 8;
                                if (x + x2 >= physicalwidth) continue;
                                int y2 = i / 8;
                                if (y + y2 >= physicalheight) continue;
                                int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                                res[(y + y2) * stride + x + x2] =
                                    GFXUtil.ConvertColorFormat(
                                        IOUtil.ReadU32LE(Data, offs + pos * 4),
                                        ColorFormat.RGBA8888,
                                        ColorFormat.ARGB8888);
                                /*GFXUtil.ToArgb(
								Data[offs + pos * 4],
								Data[offs + pos * 4 + 3],
								Data[offs + pos * 4 + 2],
								Data[offs + pos * 4 + 1]
								);*/
                            }
                            offs += 64 * 4;
                        }
                    }
                    break;
                case ImageFormat.RGB8:
                    for (int y = 0; y < Height; y += 8)
                    {
                        for (int x = 0; x < Width; x += 8)
                        {
                            for (int i = 0; i < 64; i++)
                            {
                                int x2 = i % 8;
                                if (x + x2 >= physicalwidth) continue;
                                int y2 = i / 8;
                                if (y + y2 >= physicalheight) continue;
                                int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                                res[(y + y2) * stride + x + x2] =
                                    GFXUtil.ConvertColorFormat(
                                        IOUtil.ReadU24LE(Data, offs + pos * 3),
                                        ColorFormat.RGB888,
                                        ColorFormat.ARGB8888);
                                /*GFXUtil.ToArgb(
								Data[offs + pos * 3 + 2],
								Data[offs + pos * 3 + 1],
								Data[offs + pos * 3 + 0]
								);*/
                            }
                            offs += 64 * 3;
                        }
                    }
                    break;
                case ImageFormat.RGBA5551:
                    for (int y = 0; y < Height; y += 8)
                    {
                        for (int x = 0; x < Width; x += 8)
                        {
                            for (int i = 0; i < 64; i++)
                            {
                                int x2 = i % 8;
                                if (x + x2 >= physicalwidth) continue;
                                int y2 = i / 8;
                                if (y + y2 >= physicalheight) continue;
                                int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                                res[(y + y2) * stride + x + x2] =
                                    GFXUtil.ConvertColorFormat(
                                        IOUtil.ReadU16LE(Data, offs + pos * 2),
                                        ColorFormat.RGBA5551,
                                        ColorFormat.ARGB8888);
                                //GFXUtil.RGBA5551ToArgb(IOUtil.ReadU16LE(Data, offs + pos * 2));
                            }
                            offs += 64 * 2;
                        }
                    }
                    break;
                case ImageFormat.RGB565:
                    for (int y = 0; y < Height; y += 8)
                    {
                        for (int x = 0; x < Width; x += 8)
                        {
                            for (int i = 0; i < 64; i++)
                            {
                                int x2 = i % 8;
                                if (x + x2 >= physicalwidth) continue;
                                int y2 = i / 8;
                                if (y + y2 >= physicalheight) continue;
                                int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                                res[(y + y2) * stride + x + x2] =
                                    GFXUtil.ConvertColorFormat(
                                        IOUtil.ReadU16LE(Data, offs + pos * 2),
                                        ColorFormat.RGB565,
                                        ColorFormat.ARGB8888);
                                //GFXUtil.RGB565ToArgb(IOUtil.ReadU16LE(Data, offs + pos * 2));
                            }
                            offs += 64 * 2;
                        }
                    }
                    break;
                case ImageFormat.RGBA4:
                    for (int y = 0; y < Height; y += 8)
                    {
                        for (int x = 0; x < Width; x += 8)
                        {
                            for (int i = 0; i < 64; i++)
                            {
                                int x2 = i % 8;
                                if (x + x2 >= physicalwidth) continue;
                                int y2 = i / 8;
                                if (y + y2 >= physicalheight) continue;
                                int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                                res[(y + y2) * stride + x + x2] =
                                    GFXUtil.ConvertColorFormat(
                                        IOUtil.ReadU16LE(Data, offs + pos * 2),
                                        ColorFormat.RGBA4444,
                                        ColorFormat.ARGB8888);
                                /*GFXUtil.ToArgb(
								(byte)((Data[offs + pos * 2] & 0xF) * 0x11),
								(byte)((Data[offs + pos * 2 + 1] >> 4) * 0x11),
								(byte)((Data[offs + pos * 2 + 1] & 0xF) * 0x11),
								(byte)((Data[offs + pos * 2] >> 4) * 0x11)
								);*/
                            }
                            offs += 64 * 2;
                        }
                    }
                    break;
                case ImageFormat.LA8:
                    for (int y = 0; y < Height; y += 8)
                    {
                        for (int x = 0; x < Width; x += 8)
                        {
                            for (int i = 0; i < 64; i++)
                            {
                                int x2 = i % 8;
                                if (x + x2 >= physicalwidth) continue;
                                int y2 = i / 8;
                                if (y + y2 >= physicalheight) continue;
                                int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                                ushort pixel = IOUtil.ReadU16LE(Data, offs + pos * 2);
                                res[(y + y2) * stride + x + x2] = GFXUtil.ToColorFormat(
                                    (byte)(pixel & 0xFF),
                                    (byte)((pixel >> 8) & 0xFF),
                                    (byte)((pixel >> 8) & 0xFF),
                                    (byte)((pixel >> 8) & 0xFF),
                                    ColorFormat.ARGB8888
                                    );
                            }
                            offs += 64 * 2;
                        }
                    }
                    break;
                case ImageFormat.HILO8:
                    for (int y = 0; y < Height; y += 8)
                    {
                        for (int x = 0; x < Width; x += 8)
                        {
                            for (int i = 0; i < 64; i++)
                            {
                                int x2 = i % 8;
                                if (x + x2 >= physicalwidth) continue;
                                int y2 = i / 8;
                                if (y + y2 >= physicalheight) continue;
                                int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                                ushort pixel = IOUtil.ReadU16LE(Data, offs + pos * 2);
                                res[(y + y2) * stride + x + x2] = GFXUtil.ToColorFormat(
                                    255,
                                    (byte)(pixel >> 8),
                                    (byte)(pixel & 0xFF),
                                    255,
                                    ColorFormat.ARGB8888
                                );
                            }
                            offs += 64 * 2;
                        }
                    }
                    break;
                case ImageFormat.L8:
                    for (int y = 0; y < Height; y += 8)
                    {
                        for (int x = 0; x < Width; x += 8)
                        {
                            for (int i = 0; i < 64; i++)
                            {
                                int x2 = i % 8;
                                if (x + x2 >= physicalwidth) continue;
                                int y2 = i / 8;
                                if (y + y2 >= physicalheight) continue;
                                int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                                res[(y + y2) * stride + x + x2] = GFXUtil.ToColorFormat(
                                    Data[offs + pos],
                                    Data[offs + pos],
                                    Data[offs + pos],
                                    ColorFormat.ARGB8888
                                    );
                            }
                            offs += 64;
                        }
                    }
                    break;
                case ImageFormat.A8:
                    for (int y = 0; y < Height; y += 8)
                    {
                        for (int x = 0; x < Width; x += 8)
                        {
                            for (int i = 0; i < 64; i++)
                            {
                                int x2 = i % 8;
                                if (x + x2 >= physicalwidth) continue;
                                int y2 = i / 8;
                                if (y + y2 >= physicalheight) continue;
                                int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                                res[(y + y2) * stride + x + x2] = GFXUtil.ToColorFormat(
                                    Data[offs + pos],
                                    255,
                                    255,
                                    255,
                                    ColorFormat.ARGB8888
                                    );
                            }
                            offs += 64;
                        }
                    }
                    break;
                case ImageFormat.LA4:
                    for (int y = 0; y < Height; y += 8)
                    {
                        for (int x = 0; x < Width; x += 8)
                        {
                            for (int i = 0; i < 64; i++)
                            {
                                int x2 = i % 8;
                                if (x + x2 >= physicalwidth) continue;
                                int y2 = i / 8;
                                if (y + y2 >= physicalheight) continue;
                                int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                                res[(y + y2) * stride + x + x2] = GFXUtil.ToColorFormat(
                                    (byte)((Data[offs + pos] & 0xF) * 0x11),
                                    (byte)((Data[offs + pos] >> 4) * 0x11),
                                    (byte)((Data[offs + pos] >> 4) * 0x11),
                                    (byte)((Data[offs + pos] >> 4) * 0x11),
                                    ColorFormat.ARGB8888
                                    );
                            }
                            offs += 64;
                        }
                    }
                    break;
                case ImageFormat.L4:
                    for (int y = 0; y < Height; y += 8)
                    {
                        for (int x = 0; x < Width; x += 8)
                        {
                            for (int i = 0; i < 64; i++)
                            {
                                int x2 = i % 8;
                                if (x + x2 >= physicalwidth) continue;
                                int y2 = i / 8;
                                if (y + y2 >= physicalheight) continue;
                                int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                                int shift = (pos & 1) * 4;
                                res[(y + y2) * stride + x + x2] = GFXUtil.ToColorFormat(
                                    (byte)(((Data[offs + pos / 2] >> shift) & 0xF) * 0x11),
                                    (byte)(((Data[offs + pos / 2] >> shift) & 0xF) * 0x11),
                                    (byte)(((Data[offs + pos / 2] >> shift) & 0xF) * 0x11),
                                    ColorFormat.ARGB8888
                                    );
                            }
                            offs += 64 / 2;
                        }
                    }
                    break;
                case ImageFormat.A4:
                    for (int y = 0; y < Height; y += 8)
                    {
                        for (int x = 0; x < Width; x += 8)
                        {
                            for (int i = 0; i < 64; i++)
                            {
                                int x2 = i % 8;
                                if (x + x2 >= physicalwidth) continue;
                                int y2 = i / 8;
                                if (y + y2 >= physicalheight) continue;
                                int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                                int shift = (pos & 1) * 4;
                                res[(y + y2) * stride + x + x2] = GFXUtil.ToColorFormat(
                                    (byte)(((Data[offs + pos / 2] >> shift) & 0xF) * 0x11),
                                    255,
                                    255,
                                    255,
                                    ColorFormat.ARGB8888
                                    );
                            }
                            offs += 64 / 2;
                        }
                    }
                    break;
                case ImageFormat.ETC1://Some reference: http://www.khronos.org/registry/gles/extensions/OES/OES_compressed_ETC1_RGB8_texture.txt
                case ImageFormat.ETC1A4:
                    {
                        for (int y = 0; y < Height; y += 8)
                        {
                            for (int x = 0; x < Width; x += 8)
                            {
                                for (int i = 0; i < 8; i += 4)
                                {
                                    for (int j = 0; j < 8; j += 4)
                                    {
                                        ulong alpha = 0xFFFFFFFFFFFFFFFF;
                                        if (Format == ImageFormat.ETC1A4)
                                        {
                                            alpha = IOUtil.ReadU64LE(Data, offs);
                                            offs += 8;
                                        }
                                        ulong data = IOUtil.ReadU64LE(Data, offs);
                                        bool diffbit = ((data >> 33) & 1) == 1;
                                        bool flipbit = ((data >> 32) & 1) == 1; //0: |||, 1: |-|
                                        int r1, r2, g1, g2, b1, b2;
                                        if (diffbit) //'differential' mode
                                        {
                                            int r = (int)((data >> 59) & 0x1F);
                                            int g = (int)((data >> 51) & 0x1F);
                                            int b = (int)((data >> 43) & 0x1F);
                                            r1 = (r << 3) | ((r & 0x1C) >> 2);
                                            g1 = (g << 3) | ((g & 0x1C) >> 2);
                                            b1 = (b << 3) | ((b & 0x1C) >> 2);
                                            r += (int)((data >> 56) & 0x7) << 29 >> 29;
                                            g += (int)((data >> 48) & 0x7) << 29 >> 29;
                                            b += (int)((data >> 40) & 0x7) << 29 >> 29;
                                            r2 = (r << 3) | ((r & 0x1C) >> 2);
                                            g2 = (g << 3) | ((g & 0x1C) >> 2);
                                            b2 = (b << 3) | ((b & 0x1C) >> 2);
                                        }
                                        else //'individual' mode
                                        {
                                            r1 = (int)((data >> 60) & 0xF) * 0x11;
                                            g1 = (int)((data >> 52) & 0xF) * 0x11;
                                            b1 = (int)((data >> 44) & 0xF) * 0x11;
                                            r2 = (int)((data >> 56) & 0xF) * 0x11;
                                            g2 = (int)((data >> 48) & 0xF) * 0x11;
                                            b2 = (int)((data >> 40) & 0xF) * 0x11;
                                        }
                                        int Table1 = (int)((data >> 37) & 0x7);
                                        int Table2 = (int)((data >> 34) & 0x7);
                                        for (int y3 = 0; y3 < 4; y3++)
                                        {
                                            for (int x3 = 0; x3 < 4; x3++)
                                            {
                                                if (x + j + x3 >= physicalwidth) continue;
                                                if (y + i + y3 >= physicalheight) continue;

                                                int val = (int)((data >> (x3 * 4 + y3)) & 0x1);
                                                bool neg = ((data >> (x3 * 4 + y3 + 16)) & 0x1) == 1;
                                                uint c;
                                                if ((flipbit && y3 < 2) || (!flipbit && x3 < 2))
                                                {
                                                    int add = ETC1Modifiers[Table1, val] * (neg ? -1 : 1);
                                                    c = GFXUtil.ToColorFormat((byte)(((alpha >> ((x3 * 4 + y3) * 4)) & 0xF) * 0x11), (byte)ColorClamp(r1 + add), (byte)ColorClamp(g1 + add), (byte)ColorClamp(b1 + add), ColorFormat.ARGB8888);
                                                }
                                                else
                                                {
                                                    int add = ETC1Modifiers[Table2, val] * (neg ? -1 : 1);
                                                    c = GFXUtil.ToColorFormat((byte)(((alpha >> ((x3 * 4 + y3) * 4)) & 0xF) * 0x11), (byte)ColorClamp(r2 + add), (byte)ColorClamp(g2 + add), (byte)ColorClamp(b2 + add), ColorFormat.ARGB8888);
                                                }
                                                res[(i + y3) * stride + x + j + x3] = c;
                                            }
                                        }
                                        offs += 8;
                                    }
                                }
                            }
                            res += stride * 8;
                        }
                    }
                    break;
            }
            bitm.UnlockBits(d);
            return bitm;
        }

        public static Bitmap ToBitmap(byte[] Data, int Width, int Height, ImageFormat Format, byte Flag, bool ExactSize = false)
        {
            Bitmap baseBitmap = ToBitmap(Data, 0, Width, Height, Format, ExactSize);
            return ApplySwizzleTransformation(baseBitmap, Flag);
        }

        private static unsafe Bitmap ApplySwizzleTransformation(Bitmap baseBitmap, byte SwizzleFlag)
        {
            if (baseBitmap == null) return null;
            Bitmap transformed = null;
            BitmapData srcData = baseBitmap.LockBits(new Rectangle(0, 0, baseBitmap.Width, baseBitmap.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                int width = baseBitmap.Width;
                int height = baseBitmap.Height;
                uint* srcPtr = (uint*)srcData.Scan0;

                switch (SwizzleFlag)
                {
                    case 2:
                        transformed = new Bitmap(width, height);
                        BitmapData destDataY = transformed.LockBits(new Rectangle(0, 0, width, height),
                            ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                        uint* destPtrY = (uint*)destDataY.Scan0;
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                int srcIndex = y * width + x;
                                int destIndex = (height - 1 - y) * width + x;
                                destPtrY[destIndex] = srcPtr[srcIndex];
                            }
                        }
                        transformed.UnlockBits(destDataY);
                        break;

                    case 4:
                        transformed = new Bitmap(height, width);
                        BitmapData destData90 = transformed.LockBits(new Rectangle(0, 0, height, width),
                            ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                        uint* destPtr90 = (uint*)destData90.Scan0;
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                int srcIndex = y * width + x;
                                int destIndex = (width - 1 - x) * height + y;
                                destPtr90[destIndex] = srcPtr[srcIndex];
                            }
                        }
                        transformed.UnlockBits(destData90);
                        break;

                    case 8:
                        transformed = new Bitmap(height, width);
                        BitmapData destDataTranspose = transformed.LockBits(new Rectangle(0, 0, height, width),
                            ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                        uint* destPtrTranspose = (uint*)destDataTranspose.Scan0;
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                int srcIndex = y * width + x;
                                int destIndex = x * height + y;
                                destPtrTranspose[destIndex] = srcPtr[srcIndex];
                            }
                        }
                        transformed.UnlockBits(destDataTranspose);
                        break;

                    default:
                        throw new Exception("Unsupported Swizzle Mode!");
                }
            }
            finally
            {
                baseBitmap.UnlockBits(srcData);
                baseBitmap.Dispose();
            }
            return transformed;
        }

        public static unsafe byte[] FromBitmap(Bitmap Picture, ImageFormat Format, bool ExactSize = false)
        {
            if (ExactSize && ((Picture.Width % 8) != 0 || (Picture.Height % 8) != 0)) return null;
            int physicalwidth = Picture.Width;
            int physicalheight = Picture.Height;
            int ConvWidth = Picture.Width;
            int ConvHeight = Picture.Height;
            if (!ExactSize)
            {
                ConvWidth = 1 << (int)Math.Ceiling(Math.Log(Picture.Width, 2));
                ConvHeight = 1 << (int)Math.Ceiling(Math.Log(Picture.Height, 2));
            }
            BitmapData d = null;
            try 
            {
                d = Picture.LockBits(new Rectangle(0, 0, Picture.Width, Picture.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                uint* res = (uint*)d.Scan0;
                byte[] result = new byte[ConvWidth * ConvHeight * GetBpp(Format) / 8];
                int offs = 0;
                switch (Format)
                {
                    case ImageFormat.RGBA8:
                        for (int y = 0; y < ConvHeight; y += 8)
                        {
                            for (int x = 0; x < ConvWidth; x += 8)
                            {
                                for (int i = 0; i < 64; i++)
                                {
                                    int x2 = i % 8;
                                    if (x + x2 >= physicalwidth) continue;
                                    int y2 = i / 8;
                                    if (y + y2 >= physicalheight) continue;
                                    int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                                    Color c = Color.FromArgb((int)res[(y + y2) * d.Stride / 4 + x + x2]);
                                    result[offs + pos * 4 + 0] = c.A;
                                    result[offs + pos * 4 + 1] = c.B;
                                    result[offs + pos * 4 + 2] = c.G;
                                    result[offs + pos * 4 + 3] = c.R;
                                }
                                offs += 64 * 4;
                            }
                        }
                        break;
                    case ImageFormat.RGB8:
                        for (int y = 0; y < ConvHeight; y += 8)
                        {
                            for (int x = 0; x < ConvWidth; x += 8)
                            {
                                for (int i = 0; i < 64; i++)
                                {
                                    int x2 = i % 8;
                                    if (x + x2 >= physicalwidth) continue;
                                    int y2 = i / 8;
                                    if (y + y2 >= physicalheight) continue;
                                    int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                                    Color c = Color.FromArgb((int)res[(y + y2) * d.Stride / 4 + x + x2]);
                                    result[offs + pos * 3 + 0] = c.B;
                                    result[offs + pos * 3 + 1] = c.G;
                                    result[offs + pos * 3 + 2] = c.R;
                                }
                                offs += 64 * 3;
                            }
                        }
                        break;
                    case ImageFormat.RGBA5551:
                        for (int y = 0; y < ConvHeight; y += 8)
                        {
                            for (int x = 0; x < ConvHeight; x += 8)
                            {
                                for (int i = 0; i < 64; i++)
                                {
                                    int x2 = i % 8;
                                    if (x + x2 >= physicalwidth) continue;
                                    int y2 = i / 8;
                                    if (y + y2 >= physicalheight) continue;
                                    int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                                    IOUtil.WriteU16LE(result, offs + pos * 2, (ushort)GFXUtil.ConvertColorFormat(res[(y + y2) * d.Stride / 4 + x + x2],
                                    ColorFormat.ARGB8888,
                                    ColorFormat.RGBA5551));
                                }
                                offs += 64 * 2;
                            }
                        }
                        break;
                    case ImageFormat.RGB565:
                        for (int y = 0; y < ConvHeight; y += 8)
                        {
                            for (int x = 0; x < ConvWidth; x += 8)
                            {
                                for (int i = 0; i < 64; i++)
                                {
                                    int x2 = i % 8;
                                    if (x + x2 >= physicalwidth) continue;
                                    int y2 = i / 8;
                                    if (y + y2 >= physicalheight) continue;
                                    int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                                    IOUtil.WriteU16LE(result, offs + pos * 2, (ushort)GFXUtil.ConvertColorFormat(res[(y + y2) * d.Stride / 4 + x + x2],
                                    ColorFormat.ARGB8888,
                                    ColorFormat.RGB565));
                                    //GFXUtil.ArgbToRGB565(res[(y + y2) * d.Stride / 4 + x + x2]));
                                }
                                offs += 64 * 2;
                            }
                        }
                        break;
                    case ImageFormat.RGBA4:
                        for (int y = 0; y < ConvHeight; y += 8)
                        {
                            for (int x = 0; x < ConvWidth; x += 8)
                            {
                                for (int i = 0; i < 64; i++)
                                {
                                    int x2 = i % 8;
                                    if (x + x2 >= physicalwidth) continue;
                                    int y2 = i / 8;
                                    if (y + y2 >= physicalheight) continue;
                                    int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                                    IOUtil.WriteU16LE(result, offs + pos * 2, (ushort)GFXUtil.ConvertColorFormat(res[(y + y2) * d.Stride / 4 + x + x2],
                                    ColorFormat.ARGB8888,
                                    ColorFormat.RGBA4444));
                                }
                                offs += 64 * 2;
                            }
                        }
                        break;
                    case ImageFormat.LA8:
                        for (int y = 0; y < ConvHeight; y += 8)
                        {
                            for (int x = 0; x < ConvWidth; x += 8)
                            {
                                for (int i = 0; i < 64; i++)
                                {
                                    int x2 = i % 8;
                                    if (x + x2 >= physicalwidth) continue;
                                    int y2 = i / 8;
                                    if (y + y2 >= physicalheight) continue;
                                    int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                                    Color c = Color.FromArgb((int)res[(y + y2) * d.Stride / 4 + x + x2]);
                                    ushort pixel = (ushort)((c.A << 8) | (byte)((0.299 * c.R) + (0.587 * c.G) + (0.114 * c.B)));
                                    IOUtil.WriteU16LE(result, offs + pos * 2, pixel);
                                }
                                offs += 64 * 2;
                            }
                        }
                        break;
                    case ImageFormat.HILO8:
                        for (int y = 0; y < ConvHeight; y += 8)
                        {
                            for (int x = 0; x < ConvWidth; x += 8)
                            {
                                for (int i = 0; i < 64; i++)
                                {
                                    int x2 = i % 8;
                                    if (x + x2 >= physicalwidth) continue;
                                    int y2 = i / 8;
                                    if (y + y2 >= physicalheight) continue;
                                    int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                                    Color c = Color.FromArgb((int)res[(y + y2) * d.Stride / 4 + x + x2]);
                                    ushort pixel = (ushort)((c.G & 0xFF) | ((c.R & 0xFF) << 8));
                                    IOUtil.WriteU16LE(result, offs + pos * 2, pixel);
                                }
                                offs += 64 * 2;
                            }
                        }
                        break;
                    case ImageFormat.L8:
                        for (int y = 0; y < ConvHeight; y += 8)
                        {
                            for (int x = 0; x < ConvWidth; x += 8)
                            {
                                for (int i = 0; i < 64; i++)
                                {
                                    int x2 = i % 8;
                                    if (x + x2 >= physicalwidth) continue;
                                    int y2 = i / 8;
                                    if (y + y2 >= physicalheight) continue;
                                    int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                                    Color c = Color.FromArgb((int)res[(y + y2) * d.Stride / 4 + x + x2]);
                                    result[offs + pos] = c.B;
                                    result[offs + pos] = c.G;
                                    result[offs + pos] = c.R;
                                }
                                offs += 64;
                            }
                        }
                        break;
                    case ImageFormat.A8:
                        for (int y = 0; y < ConvHeight; y += 8)
                        {
                            for (int x = 0; x < ConvWidth; x += 8)
                            {
                                for (int i = 0; i < 64; i++)
                                {
                                    int x2 = i % 8;
                                    if (x + x2 >= physicalwidth) continue;
                                    int y2 = i / 8;
                                    if (y + y2 >= physicalheight) continue;
                                    int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                                    Color c = Color.FromArgb((int)res[(y + y2) * d.Stride / 4 + x + x2]);
                                    result[offs + pos] = c.A;
                                }
                                offs += 64;
                            }
                        }
                        break;
                    case ImageFormat.LA4:
                        for (int y = 0; y < ConvHeight; y += 8)
                        {
                            for (int x = 0; x < ConvWidth; x += 8)
                            {
                                for (int i = 0; i < 64; i++)
                                {
                                    int x2 = i % 8;
                                    if (x + x2 >= physicalwidth) continue;
                                    int y2 = i / 8;
                                    if (y + y2 >= physicalheight) continue;
                                    int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                                    Color c = Color.FromArgb((int)res[(y + y2) * d.Stride / 4 + x + x2]);
                                    result[offs + pos + 1] = c.A;
                                    result[offs + pos] = c.B;
                                    result[offs + pos] = c.G;
                                    result[offs + pos] = c.R;
                                }
                                offs += 64;
                            }
                        }
                        break;
                    case ImageFormat.L4:
                        for (int y = 0; y < ConvHeight; y += 8)
                        {
                            for (int x = 0; x < ConvWidth; x += 8)
                            {
                                for (int i = 0; i < 64; i++)
                                {
                                    int x2 = i % 8;
                                    if (x + x2 >= physicalwidth) continue;
                                    int y2 = i / 8;
                                    if (y + y2 >= physicalheight) continue;
                                    int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                                    Color c = Color.FromArgb((int)res[(y + y2) * d.Stride / 4 + x + x2]);
                                    int bytePos = offs + pos / 2;
                                    int shift = (pos % 2) * 4;
                                    byte luma = (byte)((c.R >> 4) & 0xF);
                                    result[bytePos] &= (byte)(0xF0 >> shift);
                                    result[bytePos] |= (byte)(luma << shift);
                                }
                                offs += 64 / 2;
                            }
                        }
                        break;
                    case ImageFormat.A4:
                        for (int y = 0; y < ConvHeight; y += 8)
                        {
                            for (int x = 0; x < ConvWidth; x += 8)
                            {
                                for (int i = 0; i < 64; i++)
                                {
                                    int x2 = i % 8;
                                    if (x + x2 >= physicalwidth) continue;
                                    int y2 = i / 8;
                                    if (y + y2 >= physicalheight) continue;
                                    int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                                    Color c = Color.FromArgb((int)res[(y + y2) * d.Stride / 4 + x + x2]);
                                    int bytePos = offs + pos / 2;
                                    int shift = (pos % 2) * 4;
                                    byte alpha = (byte)((c.A >> 4) & 0xF);
                                    result[bytePos] &= (byte)(0xF0 >> shift);
                                    result[bytePos] |= (byte)(alpha << shift);
                                }
                                offs += 64 / 2;
                            }
                        }
                        break;
                    case ImageFormat.ETC1:
                    case ImageFormat.ETC1A4:
                        for (int y = 0; y < ConvHeight; y += 8)
                        {
                            for (int x = 0; x < ConvWidth; x += 8)
                            {
                                for (int i = 0; i < 8; i += 4)
                                {
                                    for (int j = 0; j < 8; j += 4)
                                    {
                                        if (Format == ImageFormat.ETC1A4)
                                        {
                                            ulong alpha = 0;
                                            int iiii = 0;
                                            for (int xx = 0; xx < 4; xx++)
                                            {
                                                for (int yy = 0; yy < 4; yy++)
                                                {
                                                    uint color;
                                                    if (x + j + xx >= physicalwidth) color = 0x00FFFFFF;
                                                    else if (y + i + yy >= physicalheight) color = 0x00FFFFFF;
                                                    else color = res[((y + i + yy) * (d.Stride / 4)) + x + j + xx];
                                                    uint a = color >> 24;
                                                    a >>= 4;
                                                    alpha |= (ulong)a << (iiii * 4);
                                                    iiii++;
                                                }
                                            }
                                            IOUtil.WriteU64LE(result, offs, alpha);
                                            offs += 8;
                                        }
                                        Color[] pixels = new Color[4 * 4];
                                        for (int yy = 0; yy < 4; yy++)
                                        {
                                            for (int xx = 0; xx < 4; xx++)
                                            {
                                                if (x + j + xx >= physicalwidth) pixels[yy * 4 + xx] = Color.Transparent;
                                                else if (y + i + yy >= physicalheight) pixels[yy * 4 + xx] = Color.Transparent;
                                                else pixels[yy * 4 + xx] = Color.FromArgb((int)res[((y + i + yy) * (d.Stride / 4)) + x + j + xx]);
                                            }
                                        }
                                        IOUtil.WriteU64LE(result, offs, ETC1.GenETC1(pixels));
                                        offs += 8;
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        throw new NotImplementedException("This format is not implemented yet.");
                }
                return result;
            }
            finally
            {
                if (d != null)
                    Picture.UnlockBits(d);
            }
        }

        public static unsafe byte[] FromBitmap(Bitmap Picture, ImageFormat Format, byte Flag, bool ExactSize = false)
        {
            Bitmap transformedPicture = ApplyInverseTransformation(Picture, Flag);
            return FromBitmap(transformedPicture, Format, ExactSize);
        }

        private static Bitmap ApplyInverseTransformation(Bitmap original, byte flag)
        {
            switch (flag)
            {
                case 2:
                    Bitmap flippedY = new Bitmap(original);
                    flippedY.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    return flippedY;

                case 4:
                    Bitmap rotated90 = (Bitmap)original.Clone();
                    rotated90.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    return rotated90;

                case 8:
                    return Transpose(original);

                default:
                    return new Bitmap(original);
            }
        }

        private static Bitmap Transpose(Bitmap original)
        {
            Bitmap transposed = new Bitmap(original.Height, original.Width);

            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    Color pixel = original.GetPixel(x, y);
                    transposed.SetPixel(y, x, pixel);
                }
            }

            return transposed;
        }

        private static int ColorClamp(int Color)
        {
            if (Color > 255) Color = 255;
            if (Color < 0) Color = 0;
            return Color;
        }

    }
}