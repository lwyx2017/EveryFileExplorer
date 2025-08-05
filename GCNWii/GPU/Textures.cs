using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using LibEveryFileExplorer.GFX;
using LibEveryFileExplorer.IO;

namespace GCNWii.GPU
{
    public class Textures
    {
        public enum ImageFormat : uint
        {
            I4 = 0,
            I8 = 1,
            IA4 = 2,
            IA8 = 3,
            RGB565 = 4,
            RGB5A3 = 5,
            RGBA32 = 6,
            CI4 = 8,
            CI8 = 9,
            CI14X2 = 10,
            CMPR = 14
        }

        public enum PaletteFormat : uint
        {
            IA8 = 0,
            RGB565 = 1,
            RGB5A3 = 2
        }

        private static readonly int[] Bpp = { 4, 8, 8, 16, 16, 16, 32, 0, 4, 8, 16, 0, 0, 0, 4 };

        public static int GetBpp(ImageFormat Format) { return Bpp[(uint)Format]; }

        private static readonly int[] TileSizeW = { 8, 8, 8, 4, 4, 4, 4, 0, 8, 8, 4, 0, 0, 0, 8 };
        private static readonly int[] TileSizeH = { 8, 4, 4, 4, 4, 4, 4, 0, 8, 4, 4, 0, 0, 0, 8 };

        public static int GetDataSize(ImageFormat Format, int Width, int Height)
        {
            while ((Width % TileSizeW[(uint)Format]) != 0) Width++;
            while ((Height % TileSizeH[(uint)Format]) != 0) Height++;
            return Width * Height * GetBpp(Format) / 8;
        }

        public static unsafe Bitmap ToBitmap(byte[] TexData, int TexOffset, byte[] PalData, int PalOffset, int Width, int Height, ImageFormat Format, PaletteFormat PalFormat, bool ExactSize = false)
        {
            Bitmap bitm = new Bitmap(Width, Height);
            BitmapData d = bitm.LockBits(new Rectangle(0, 0, bitm.Width, bitm.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            uint* res = (uint*)d.Scan0;
            int offs = TexOffset;
            int stride = d.Stride / 4;
            switch (Format)
            {
                case ImageFormat.I4:
                    {
                        for (int y = 0; y < Height; y += 8)
                        {
                            for (int x = 0; x < Width; x += 8)
                            {
                                for (int y2 = 0; y2 < 8; y2++)
                                {
                                    for (int x2 = 0; x2 < 8; x2 += 2)
                                    {
                                        int posY = y + y2;
                                        int posX1 = x + x2;
                                        int posX2 = x + x2 + 1;
                                        byte pixel = TexData[offs++];
                                        byte I1 = (byte)((pixel >> 4) * 0x11);
                                        byte I2 = (byte)((pixel & 0xF) * 0x11);
                                        if (posY < Height && posX1 < Width)
                                        {
                                            res[posY * stride + posX1] =
                                                GFXUtil.ToColorFormat(I1, I1, I1, ColorFormat.ARGB8888);
                                        }
                                        if (posY < Height && posX2 < Width)
                                        {
                                            res[posY * stride + posX2] =
                                                GFXUtil.ToColorFormat(I2, I2, I2, ColorFormat.ARGB8888);
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    }
                case ImageFormat.I8:
                    {
                        for (int y = 0; y < Height; y += 4)
                        {
                            for (int x = 0; x < Width; x += 8)
                            {
                                for (int y2 = 0; y2 < 4; y2++)
                                {
                                    for (int x2 = 0; x2 < 8; x2++)
                                    {
                                        int posY = y + y2;
                                        int posX = x + x2;
                                        byte I = TexData[offs];
                                        offs++;
                                        if (posY >= Height || posX >= Width) continue;
                                        res[posY * stride + posX] =
                                            GFXUtil.ToColorFormat(I, I, I, ColorFormat.ARGB8888);
                                    }
                                }
                            }
                        }
                        break;
                    }
                case ImageFormat.IA4:
                    {
                        for (int y = 0; y < Height; y += 4)
                        {
                            for (int x = 0; x < Width; x += 8)
                            {
                                for (int y2 = 0; y2 < 4; y2++)
                                {
                                    for (int x2 = 0; x2 < 8; x2++)
                                    {
                                        int posY = y + y2;
                                        int posX = x + x2;
                                        byte data = TexData[offs];
                                        offs++;
                                        if (posY >= Height || posX >= Width) continue;
                                        byte I = (byte)((data & 0xF) * 0x11);
                                        byte A = (byte)((data >> 4) * 0x11);
                                        res[posY * stride + posX] =
                                            GFXUtil.ToColorFormat(A, I, I, I, ColorFormat.ARGB8888);
                                    }
                                }
                            }
                        }
                        break;
                    }
                case ImageFormat.IA8:
                    {
                        for (int y = 0; y < Height; y += 4)
                        {
                            for (int x = 0; x < Width; x += 4)
                            {
                                for (int y2 = 0; y2 < 4; y2++)
                                {
                                    for (int x2 = 0; x2 < 4; x2++)
                                    {
                                        int posY = y + y2;
                                        int posX = x + x2;
                                        byte A = TexData[offs];
                                        byte I = TexData[offs + 1];
                                        offs += 2;
                                        if (posY >= Height || posX >= Width) continue;
                                        res[posY * stride + posX] =
                                            GFXUtil.ToColorFormat(A, I, I, I, ColorFormat.ARGB8888);
                                    }
                                }
                            }
                        }
                        break;
                    }
                case ImageFormat.RGB565:
                    {
                        for (int y = 0; y < Height; y += 4)
                        {
                            for (int x = 0; x < Width; x += 4)
                            {
                                for (int y2 = 0; y2 < 4; y2++)
                                {
                                    for (int x2 = 0; x2 < 4; x2++)
                                    {
                                        int posY = y + y2;
                                        int posX = x + x2;
                                        ushort color = IOUtil.ReadU16BE(TexData, offs);
                                        offs += 2;
                                        if (posY >= Height || posX >= Width) continue;
                                        res[posY * stride + posX] =
                                            GFXUtil.ConvertColorFormat(color, ColorFormat.RGB565, ColorFormat.ARGB8888);
                                    }
                                }
                            }
                        }
                        break;
                    }
                case ImageFormat.RGB5A3:
                    {
                        for (int y = 0; y < Height; y += 4)
                        {
                            for (int x = 0; x < Width; x += 4)
                            {
                                for (int y2 = 0; y2 < 4; y2++)
                                {
                                    for (int x2 = 0; x2 < 4; x2++)
                                    {
                                        int posY = y + y2;
                                        int posX = x + x2;
                                        ushort data = IOUtil.ReadU16BE(TexData, offs);
                                        offs += 2;
                                        if (posY >= Height || posX >= Width) continue;
                                        if ((data & 0x8000) != 0)
                                            res[posY * stride + posX] =
                                                GFXUtil.ConvertColorFormat(data, ColorFormat.XRGB1555, ColorFormat.ARGB8888);
                                        else
                                            res[posY * stride + posX] =
                                                GFXUtil.ConvertColorFormat(data, ColorFormat.ARGB3444, ColorFormat.ARGB8888);
                                    }
                                }
                            }
                        }
                        break;
                    }
                case ImageFormat.RGBA32:
                    {
                        for (int y = 0; y < Height; y += 4)
                        {
                            for (int x = 0; x < Width; x += 4)
                            {
                                for (int y2 = 0; y2 < 4; y2++)
                                {
                                    for (int x2 = 0; x2 < 4; x2++)
                                    {
                                        int posY = y + y2;
                                        int posX = x + x2;
                                        ushort ar = IOUtil.ReadU16BE(TexData, offs);
                                        offs += 2;
                                        if (posY < Height && posX < Width)
                                        {
                                            byte a = (byte)(ar >> 8);
                                            byte r = (byte)(ar & 0xFF);
                                            res[posY * stride + posX] = (uint)((a << 24) | (r << 16));
                                        }
                                    }
                                }
                                for (int y2 = 0; y2 < 4; y2++)
                                {
                                    for (int x2 = 0; x2 < 4; x2++)
                                    {
                                        int posY = y + y2;
                                        int posX = x + x2;
                                        ushort gb = IOUtil.ReadU16BE(TexData, offs);
                                        offs += 2;

                                        if (posY < Height && posX < Width)
                                        {
                                            byte g = (byte)(gb >> 8);
                                            byte b = (byte)(gb & 0xFF);
                                            res[posY * stride + posX] |= (uint)((g << 8) | b);
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    }
                case ImageFormat.CI4:
                    {
                        for (int y = 0; y < Height; y += 8)
                        {
                            for (int x = 0; x < Width; x += 8)
                            {
                                for (int y2 = 0; y2 < 8; y2++)
                                {
                                    for (int x2 = 0; x2 < 8; x2 += 2)
                                    {
                                        int posY = y + y2;
                                        int posX1 = x + x2;
                                        int posX2 = x + x2 + 1;
                                        byte pixelData = TexData[offs];
                                        offs++;
                                        byte index1 = (byte)(pixelData >> 4);
                                        byte index2 = (byte)(pixelData & 0xF);

                                        if (posY < Height && posX1 < Width)
                                        {
                                            switch (PalFormat)
                                            {
                                                case PaletteFormat.IA8:
                                                    {
                                                        ushort data1 = IOUtil.ReadU16BE(PalData, PalOffset + index1 * 2);
                                                        byte I1 = (byte)(data1 & 0xFF);
                                                        byte A1 = (byte)(data1 >> 8);
                                                        res[posY * stride + posX1] =
                                                            GFXUtil.ToColorFormat(A1, I1, I1, I1, ColorFormat.ARGB8888);
                                                        break;
                                                    }
                                                case PaletteFormat.RGB565:
                                                    {
                                                        ushort data1 = IOUtil.ReadU16BE(PalData, PalOffset + index1 * 2);
                                                        res[posY * stride + posX1] =
                                                            GFXUtil.ConvertColorFormat(data1, ColorFormat.RGB565, ColorFormat.ARGB8888);
                                                        break;
                                                    }
                                                case PaletteFormat.RGB5A3:
                                                    {
                                                        ushort data1 = IOUtil.ReadU16BE(PalData, PalOffset + index1 * 2);
                                                        if ((data1 & 0x8000) != 0)
                                                            res[posY * stride + posX1] =
                                                                GFXUtil.ConvertColorFormat(data1, ColorFormat.XRGB1555, ColorFormat.ARGB8888);
                                                        else
                                                            res[posY * stride + posX1] =
                                                                GFXUtil.ConvertColorFormat(data1, ColorFormat.ARGB3444, ColorFormat.ARGB8888);
                                                        break;
                                                    }
                                            }
                                        }

                                        if (posY < Height && posX2 < Width)
                                        {
                                            switch (PalFormat)
                                            {
                                                case PaletteFormat.IA8:
                                                    {
                                                        ushort data2 = IOUtil.ReadU16BE(PalData, PalOffset + index2 * 2);
                                                        byte I2 = (byte)(data2 & 0xFF);
                                                        byte A2 = (byte)(data2 >> 8);
                                                        res[posY * stride + posX2] =
                                                            GFXUtil.ToColorFormat(A2, I2, I2, I2, ColorFormat.ARGB8888);
                                                        break;
                                                    }
                                                case PaletteFormat.RGB565:
                                                    {
                                                        ushort data2 = IOUtil.ReadU16BE(PalData, PalOffset + index2 * 2);
                                                        res[posY * stride + posX2] =
                                                            GFXUtil.ConvertColorFormat(data2, ColorFormat.RGB565, ColorFormat.ARGB8888);
                                                        break;
                                                    }
                                                case PaletteFormat.RGB5A3:
                                                    {
                                                        ushort data2 = IOUtil.ReadU16BE(PalData, PalOffset + index2 * 2);
                                                        if ((data2 & 0x8000) != 0)
                                                            res[posY * stride + posX2] =
                                                                GFXUtil.ConvertColorFormat(data2, ColorFormat.XRGB1555, ColorFormat.ARGB8888);
                                                        else
                                                            res[posY * stride + posX2] =
                                                                GFXUtil.ConvertColorFormat(data2, ColorFormat.ARGB3444, ColorFormat.ARGB8888);
                                                        break;
                                                    }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    }
                case ImageFormat.CI8:
                    {
                        for (int y = 0; y < Height; y += 4)
                        {
                            for (int x = 0; x < Width; x += 8)
                            {
                                for (int y2 = 0; y2 < 4; y2++)
                                {
                                    for (int x2 = 0; x2 < 8; x2++)
                                    {
                                        int posX = x + x2;
                                        int posY = y + y2;

                                        if (posY >= Height || posX >= Width)
                                        {
                                            offs++;
                                            continue;
                                        }

                                        byte index = TexData[offs];
                                        offs++;

                                        if (PalData != null && PalOffset + index * 2 + 1 < PalData.Length)
                                        {
                                            ushort palColor = IOUtil.ReadU16BE(PalData, PalOffset + index * 2);
                                            uint argb;

                                            switch (PalFormat)
                                            {
                                                case PaletteFormat.IA8:
                                                    {
                                                        byte I = (byte)(palColor & 0xFF);
                                                        byte A = (byte)(palColor >> 8);
                                                        argb = GFXUtil.ToColorFormat(A, I, I, I, ColorFormat.ARGB8888);
                                                        break;
                                                    }
                                                case PaletteFormat.RGB565:
                                                    {
                                                        argb = GFXUtil.ConvertColorFormat(palColor, ColorFormat.RGB565, ColorFormat.ARGB8888);
                                                        break;
                                                    }
                                                case PaletteFormat.RGB5A3:
                                                    {
                                                        if ((palColor & 0x8000) != 0)
                                                            argb = GFXUtil.ConvertColorFormat(palColor, ColorFormat.XRGB1555, ColorFormat.ARGB8888);
                                                        else
                                                            argb = GFXUtil.ConvertColorFormat(palColor, ColorFormat.ARGB3444, ColorFormat.ARGB8888);
                                                        break;
                                                    }
                                                default:
                                                    throw new Exception("Unsupported Palette Format!");
                                            }
                                            res[posY * stride + posX] = argb;
                                        }
                                        else
                                        {
                                            res[posY * stride + posX] = 0xFF000000;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    }
                case ImageFormat.CI14X2:
                    {
                        for (int y = 0; y < Height; y += 4)
                        {
                            for (int x = 0; x < Width; x += 4)
                            {
                                for (int y2 = 0; y2 < 4; y2++)
                                {
                                    for (int x2 = 0; x2 < 4; x2++)
                                    {
                                        int posX = x + x2;
                                        int posY = y + y2;

                                        if (offs + 1 >= TexData.Length) continue;

                                        ushort indexData = IOUtil.ReadU16BE(TexData, offs);
                                        offs += 2;
                                        if (posX >= Width || posY >= Height) continue;

                                        ushort index = (ushort)(indexData & 0x3FFF);

                                        int palIndexOffset = PalOffset + index * 2;
                                        if (PalData == null || palIndexOffset + 1 >= PalData.Length) continue;

                                        ushort palColor = IOUtil.ReadU16BE(PalData, palIndexOffset);
                                        uint argb;
                                        switch (PalFormat)
                                        {
                                            case PaletteFormat.IA8:
                                                byte I = (byte)(palColor & 0xFF);
                                                byte A = (byte)(palColor >> 8);
                                                argb = GFXUtil.ToColorFormat(A, I, I, I, ColorFormat.ARGB8888);
                                                break;
                                            case PaletteFormat.RGB565:
                                                argb = GFXUtil.ConvertColorFormat(palColor, ColorFormat.RGB565, ColorFormat.ARGB8888);
                                                break;
                                            case PaletteFormat.RGB5A3:
                                                if ((palColor & 0x8000) != 0)
                                                    argb = GFXUtil.ConvertColorFormat(palColor, ColorFormat.XRGB1555, ColorFormat.ARGB8888);
                                                else
                                                    argb = GFXUtil.ConvertColorFormat(palColor, ColorFormat.ARGB3444, ColorFormat.ARGB8888);
                                                break;
                                            default:
                                                throw new Exception("Unsupported Palette Format!");
                                        }
                                        res[posY * stride + posX] = argb;
                                    }
                                }
                            }
                        }
                        break;
                    }
                case ImageFormat.CMPR:
                    for (int y = 0; y < Height; y += 8)
                    {
                        for (int x = 0; x < Width; x += 8)
                        {
                            for (int y2 = 0; y2 < 8; y2 += 4)
                            {
                                for (int x2 = 0; x2 < 8; x2 += 4)
                                {
                                    ushort color0 = IOUtil.ReadU16BE(TexData, offs);
                                    ushort color1 = IOUtil.ReadU16BE(TexData, offs + 2);
                                    uint data = IOUtil.ReadU32BE(TexData, offs + 4);
                                    uint[] Palette = new uint[4];
                                    Palette[0] = GFXUtil.ConvertColorFormat(color0, ColorFormat.RGB565, ColorFormat.ARGB8888);
                                    Palette[1] = GFXUtil.ConvertColorFormat(color1, ColorFormat.RGB565, ColorFormat.ARGB8888);
                                    Color a = Color.FromArgb((int)Palette[0]);
                                    Color b = Color.FromArgb((int)Palette[1]);
                                    if (color0 > color1)//1/3 and 2/3
                                    {
                                        Palette[2] = GFXUtil.ToColorFormat((a.R * 2 + b.R * 1) / 3, (a.G * 2 + b.G * 1) / 3, (a.B * 2 + b.B * 1) / 3, ColorFormat.ARGB8888);
                                        Palette[3] = GFXUtil.ToColorFormat((a.R * 1 + b.R * 2) / 3, (a.G * 1 + b.G * 2) / 3, (a.B * 1 + b.B * 2) / 3, ColorFormat.ARGB8888);
                                    }
                                    else//1/2 and transparent
                                    {
                                        Palette[2] = GFXUtil.ToColorFormat((a.R + b.R) / 2, (a.G + b.G) / 2, (a.B + b.B) / 2, ColorFormat.ARGB8888);
                                        Palette[3] = 0;
                                    }

                                    int q = 30;
                                    for (int y3 = 0; y3 < 4; y3++)
                                    {
                                        for (int x3 = 0; x3 < 4; x3++)
                                        {
                                            int posX = x + x2 + x3;
                                            int posY = y + y2 + y3;
                                            if (posX >= Width || posY >= Height)
                                                continue;
                                            res[posY * stride + posX] = Palette[(data >> q) & 3];
                                            q -= 2;
                                        }
                                    }
                                    offs += 8;
                                }
                            }
                        }
                    }
                    break;
            }
            bitm.UnlockBits(d);
            return bitm;
        }

        public static unsafe void FromBitmap(Bitmap bitm, ref byte[] TexData, ref byte[] PalData, int Width, int Height, ImageFormat Format, PaletteFormat PalFormat, bool ExactSize = false)
        {
            BitmapData d = null;
            try
            {
                int dataSize = GetDataSize(Format, Width, Height);
                if (TexData == null || TexData.Length < dataSize) TexData = new byte[dataSize];
                d = bitm.LockBits(new Rectangle(0, 0, bitm.Width, bitm.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                uint* src = (uint*)d.Scan0;
                int stride = d.Stride / 4;
                int offs = 0;
                switch (Format)
                {
                    case ImageFormat.I4:
                        {
                            for (int y = 0; y < Height; y += 8)
                            {
                                for (int x = 0; x < Width; x += 8)
                                {
                                    for (int y2 = 0; y2 < 8; y2++)
                                    {
                                        for (int x2 = 0; x2 < 8; x2 += 2)
                                        {
                                            byte I1 = 0;
                                            byte I2 = 0;
                                            int posY = y + y2;
                                            int posX1 = x + x2;
                                            if (posY < Height && posX1 < Width)
                                            {
                                                uint argb = src[posY * stride + posX1];
                                                byte r = (byte)((argb >> 16) & 0xFF);
                                                byte g = (byte)((argb >> 8) & 0xFF);
                                                byte b = (byte)(argb & 0xFF);
                                                I1 = (byte)((r + g + b) / 3);
                                            }
                                            int posX2 = x + x2 + 1;
                                            if (posY < Height && posX2 < Width)
                                            {
                                                uint argb = src[posY * stride + posX2];
                                                byte r = (byte)((argb >> 16) & 0xFF);
                                                byte g = (byte)((argb >> 8) & 0xFF);
                                                byte b = (byte)(argb & 0xFF);
                                                I2 = (byte)((r + g + b) / 3);
                                            }
                                            byte pixel = (byte)((((I1 * 15 + 127) / 255) << 4) | ((I2 * 15 + 127) / 255));
                                            TexData[offs++] = pixel;
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    case ImageFormat.I8:
                        {
                            for (int y = 0; y < Height; y += 4)
                            {
                                for (int x = 0; x < Width; x += 8)
                                {
                                    for (int y2 = 0; y2 < 4; y2++)
                                    {
                                        for (int x2 = 0; x2 < 8; x2++)
                                        {
                                            int posY = y + y2;
                                            int posX = x + x2;
                                            byte I = 0;
                                            if (posY < Height && posX < Width)
                                            {
                                                uint argb = src[posY * stride + posX];
                                                byte r = (byte)((argb >> 16) & 0xFF);
                                                byte g = (byte)((argb >> 8) & 0xFF);
                                                byte b = (byte)(argb & 0xFF);
                                                I = (byte)((r + g + b) / 3);
                                            }
                                            TexData[offs++] = I;
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    case ImageFormat.IA4:
                        {
                            for (int y = 0; y < Height; y += 4)
                            {
                                for (int x = 0; x < Width; x += 8)
                                {
                                    for (int y2 = 0; y2 < 4; y2++)
                                    {
                                        for (int x2 = 0; x2 < 8; x2++)
                                        {
                                            int posY = y + y2;
                                            int posX = x + x2;
                                            byte data = 0;
                                            if (posY < Height && posX < Width)
                                            {
                                                uint argb = src[posY * stride + posX];
                                                byte a = (byte)((argb >> 24) & 0xFF);
                                                byte r = (byte)((argb >> 16) & 0xFF);
                                                byte g = (byte)((argb >> 8) & 0xFF);
                                                byte b = (byte)(argb & 0xFF);
                                                byte I = (byte)((r + g + b) / 3);
                                                byte a4 = (byte)((a * 15 + 127) / 255);
                                                byte I4 = (byte)((I * 15 + 127) / 255);
                                                data = (byte)((a4 << 4) | I4);
                                            }
                                            TexData[offs++] = data;
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    case ImageFormat.IA8:
                        {
                            for (int y = 0; y < Height; y += 4)
                            {
                                for (int x = 0; x < Width; x += 4)
                                {
                                    for (int y2 = 0; y2 < 4; y2++)
                                    {
                                        for (int x2 = 0; x2 < 4; x2++)
                                        {
                                            int posY = y + y2;
                                            int posX = x + x2;
                                            byte A = 0;
                                            byte I = 0;
                                            if (posY < Height && posX < Width)
                                            {
                                                uint argb = src[posY * stride + posX];
                                                A = (byte)((argb >> 24) & 0xFF);
                                                byte r = (byte)((argb >> 16) & 0xFF);
                                                byte g = (byte)((argb >> 8) & 0xFF);
                                                byte b = (byte)(argb & 0xFF);
                                                I = (byte)((r + g + b) / 3);
                                            }
                                            TexData[offs++] = A;
                                            TexData[offs++] = I;
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    case ImageFormat.RGB565:
                        {
                            for (int y = 0; y < Height; y += 4)
                            {
                                for (int x = 0; x < Width; x += 4)
                                {
                                    for (int y2 = 0; y2 < 4; y2++)
                                    {
                                        for (int x2 = 0; x2 < 4; x2++)
                                        {
                                            ushort data = 0;
                                            if (y + y2 < bitm.Height && x + x2 < bitm.Width)
                                            {
                                                uint argb = src[(y + y2) * stride + x + x2];
                                                data = (ushort)GFXUtil.ConvertColorFormat(argb,
                                                    ColorFormat.ARGB8888,
                                                    ColorFormat.RGB565
                                                );
                                            }
                                            IOUtil.WriteU16BE(TexData, offs, data);
                                            offs += 2;
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    case ImageFormat.RGB5A3:
                        {
                            for (int y = 0; y < Height; y += 4)
                            {
                                for (int x = 0; x < Width; x += 4)
                                {
                                    for (int y2 = 0; y2 < 4; y2++)
                                    {
                                        for (int x2 = 0; x2 < 4; x2++)
                                        {
                                            int posY = y + y2;
                                            int posX = x + x2;
                                            ushort data = 0;
                                            if (posY < Height && posX < Width)
                                            {
                                                uint argb = src[posY * stride + posX];
                                                byte a = (byte)((argb >> 24) & 0xFF);
                                                if (a > 0xDA)
                                                {
                                                    data = (ushort)GFXUtil.ConvertColorFormat(
                                                    argb, ColorFormat.ARGB8888, ColorFormat.XRGB1555);
                                                    data |= 0x8000;
                                                }
                                                else
                                                {
                                                    data = (ushort)GFXUtil.ConvertColorFormat(
                                                    argb, ColorFormat.ARGB8888, ColorFormat.ARGB3444);
                                                }
                                            }
                                            IOUtil.WriteU16BE(TexData, offs, data);
                                            offs += 2;
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    case ImageFormat.RGBA32:
                        {
                            for (int y = 0; y < Height; y += 4)
                            {
                                for (int x = 0; x < Width; x += 4)
                                {
                                    for (int y2 = 0; y2 < 4; y2++)
                                    {
                                        for (int x2 = 0; x2 < 4; x2++)
                                        {
                                            int posY = y + y2;
                                            int posX = x + x2;
                                            ushort ar = 0;

                                            if (posY < Height && posX < Width)
                                            {
                                                uint argb = src[posY * stride + posX];
                                                byte a = (byte)((argb >> 24) & 0xFF);
                                                byte r = (byte)((argb >> 16) & 0xFF);
                                                ar = (ushort)((a << 8) | r);
                                            }
                                            IOUtil.WriteU16BE(TexData, offs, ar);
                                            offs += 2;
                                        }
                                    }
                                    for (int y2 = 0; y2 < 4; y2++)
                                    {
                                        for (int x2 = 0; x2 < 4; x2++)
                                        {
                                            int posY = y + y2;
                                            int posX = x + x2;
                                            ushort gb = 0;

                                            if (posY < Height && posX < Width)
                                            {
                                                uint argb = src[posY * stride + posX];
                                                byte g = (byte)((argb >> 8) & 0xFF);
                                                byte b = (byte)(argb & 0xFF);
                                                gb = (ushort)((g << 8) | b);
                                            }
                                            IOUtil.WriteU16BE(TexData, offs, gb);
                                            offs += 2;
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    case ImageFormat.CI4:
                        {
                            Dictionary<uint, int> colorCounts = new Dictionary<uint, int>();
                            for (int y = 0; y < Height; y++)
                            {
                                for (int x = 0; x < Width; x++)
                                {
                                    uint argb = src[y * stride + x];
                                    if (colorCounts.ContainsKey(argb)) colorCounts[argb]++;
                                    else colorCounts.Add(argb, 1);
                                }
                            }

                            var sortedColors = colorCounts.OrderByDescending(o => o.Value).Select(o => o.Key).Take(16).ToList();
                            uint[] palette = new uint[16];
                            sortedColors.CopyTo(palette, 0);

                            for (int i = sortedColors.Count; i < 16; i++)
                            {
                                palette[i] = 0;
                            }

                            int palSize = 16 * 2;
                            if (PalData == null || PalData.Length < palSize) PalData = new byte[palSize];

                            for (int i = 0; i < 16; i++)
                            {
                                ushort palEntry;
                                switch (PalFormat)
                                {
                                    case PaletteFormat.IA8:
                                        byte a = (byte)((palette[i] >> 24) & 0xFF);
                                        byte r = (byte)((palette[i] >> 16) & 0xFF);
                                        byte g = (byte)((palette[i] >> 8) & 0xFF);
                                        byte b = (byte)(palette[i] & 0xFF);
                                        byte intensity = (byte)((r + g + b) / 3);
                                        palEntry = (ushort)((a << 8) | intensity);
                                        break;
                                    case PaletteFormat.RGB565:
                                        palEntry = (ushort)GFXUtil.ConvertColorFormat(
                                            palette[i],
                                            ColorFormat.ARGB8888,
                                            ColorFormat.RGB565);
                                        break;
                                    case PaletteFormat.RGB5A3:
                                        byte alpha = (byte)((palette[i] >> 24) & 0xFF);
                                        if (alpha > 0xDA)
                                        {
                                            palEntry = (ushort)GFXUtil.ConvertColorFormat(
                                                palette[i] | 0xFF000000,
                                                ColorFormat.ARGB8888,
                                                ColorFormat.XRGB1555);
                                            palEntry |= 0x8000;
                                        }
                                        else
                                        {
                                            palEntry = (ushort)GFXUtil.ConvertColorFormat(
                                                palette[i],
                                                ColorFormat.ARGB8888,
                                                ColorFormat.ARGB3444);
                                        }
                                        break;
                                    default:
                                        throw new Exception("Unsupported palette format!");
                                }
                                IOUtil.WriteU16BE(PalData, i * 2, palEntry);
                            }

                            uint[] optimizedPalette = new uint[16];
                            for (int i = 0; i < 16; i++)
                            {
                                ushort palEntry = IOUtil.ReadU16BE(PalData, i * 2);
                                switch (PalFormat)
                                {
                                    case PaletteFormat.IA8:
                                        byte intensity = (byte)(palEntry & 0xFF);
                                        byte alpha = (byte)(palEntry >> 8);
                                        optimizedPalette[i] = GFXUtil.ToColorFormat(
                                            alpha, intensity, intensity, intensity, ColorFormat.ARGB8888);
                                        break;
                                    case PaletteFormat.RGB565:
                                        optimizedPalette[i] = GFXUtil.ConvertColorFormat(
                                            palEntry, ColorFormat.RGB565, ColorFormat.ARGB8888);
                                        break;
                                    case PaletteFormat.RGB5A3:
                                        if ((palEntry & 0x8000) != 0)
                                            optimizedPalette[i] = GFXUtil.ConvertColorFormat(
                                                palEntry, ColorFormat.XRGB1555, ColorFormat.ARGB8888);
                                        else
                                            optimizedPalette[i] = GFXUtil.ConvertColorFormat(
                                                palEntry, ColorFormat.ARGB3444, ColorFormat.ARGB8888);
                                        break;
                                }
                            }

                            for (int y = 0; y < Height; y += 8)
                            {
                                for (int x = 0; x < Width; x += 8)
                                {
                                    for (int y2 = 0; y2 < 8; y2++)
                                    {
                                        for (int x2 = 0; x2 < 8; x2 += 2)
                                        {
                                            int posY = y + y2;
                                            int posX1 = x + x2;
                                            int posX2 = x + x2 + 1;

                                            byte index1 = 0;
                                            byte index2 = 0;

                                            if (posY < Height)
                                            {
                                                if (posX1 < Width)
                                                {
                                                    uint color = src[posY * stride + posX1];
                                                    index1 = FindClosestColor(color, optimizedPalette);
                                                }

                                                if (posX2 < Width)
                                                {
                                                    uint color = src[posY * stride + posX2];
                                                    index2 = FindClosestColor(color, optimizedPalette);
                                                }
                                            }

                                            TexData[offs++] = (byte)((index1 << 4) | index2);
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    case ImageFormat.CI8:
                        {
                            Dictionary<uint, int> colorCounts = new Dictionary<uint, int>();
                            for (int y = 0; y < Height; y++)
                            {
                                for (int x = 0; x < Width; x++)
                                {
                                    if (y < bitm.Height && x < bitm.Width)
                                    {
                                        uint argb = src[y * stride + x];
                                        if (colorCounts.ContainsKey(argb))
                                            colorCounts[argb]++;
                                        else
                                            colorCounts.Add(argb, 1);
                                    }
                                }
                            }

                            var sortedColors = colorCounts.OrderByDescending(o => o.Value)
                                .Select(o => o.Key)
                                .Take(256)
                                .ToList();

                            uint[] palette = new uint[256];
                            for (int i = 0; i < sortedColors.Count && i < 256; i++)
                            {
                                palette[i] = sortedColors[i];
                            }

                            for (int i = sortedColors.Count; i < 256; i++)
                            {
                                palette[i] = 0;
                            }

                            int palSize = 256 * 2;
                            if (PalData == null || PalData.Length < palSize)
                                PalData = new byte[palSize];

                            for (int i = 0; i < 256; i++)
                            {
                                ushort palEntry;
                                switch (PalFormat)
                                {
                                    case PaletteFormat.IA8:
                                        byte a = (byte)((palette[i] >> 24) & 0xFF);
                                        byte r = (byte)((palette[i] >> 16) & 0xFF);
                                        byte g = (byte)((palette[i] >> 8) & 0xFF);
                                        byte b = (byte)(palette[i] & 0xFF);
                                        byte intensity = (byte)((r + g + b) / 3);
                                        palEntry = (ushort)((a << 8) | intensity);
                                        break;

                                    case PaletteFormat.RGB565:
                                        palEntry = (ushort)GFXUtil.ConvertColorFormat(
                                            palette[i],
                                            ColorFormat.ARGB8888,
                                            ColorFormat.RGB565);
                                        break;

                                    case PaletteFormat.RGB5A3:
                                        byte alpha = (byte)((palette[i] >> 24) & 0xFF);
                                        if (alpha > 0xDA)
                                        {
                                            palEntry = (ushort)GFXUtil.ConvertColorFormat(
                                                palette[i] | 0xFF000000,
                                                ColorFormat.ARGB8888,
                                                ColorFormat.XRGB1555);
                                            palEntry |= 0x8000;
                                        }
                                        else
                                        {
                                            palEntry = (ushort)GFXUtil.ConvertColorFormat(
                                                palette[i],
                                                ColorFormat.ARGB8888,
                                                ColorFormat.ARGB3444);
                                        }
                                        break;

                                    default:
                                        throw new Exception("Unsupported palette format!");
                                }
                                IOUtil.WriteU16BE(PalData, i * 2, palEntry);
                            }

                            uint[] optimizedPalette = new uint[256];
                            for (int i = 0; i < 256; i++)
                            {
                                if (i * 2 + 1 < PalData.Length)
                                {
                                    ushort palEntry = IOUtil.ReadU16BE(PalData, i * 2);
                                    switch (PalFormat)
                                    {
                                        case PaletteFormat.IA8:
                                            byte intensity = (byte)(palEntry & 0xFF);
                                            byte alpha = (byte)(palEntry >> 8);
                                            optimizedPalette[i] = GFXUtil.ToColorFormat(
                                                alpha, intensity, intensity, intensity,
                                                ColorFormat.ARGB8888);
                                            break;

                                        case PaletteFormat.RGB565:
                                            optimizedPalette[i] = GFXUtil.ConvertColorFormat(
                                                palEntry,
                                                ColorFormat.RGB565,
                                                ColorFormat.ARGB8888);
                                            break;

                                        case PaletteFormat.RGB5A3:
                                            if ((palEntry & 0x8000) != 0)
                                                optimizedPalette[i] = GFXUtil.ConvertColorFormat(
                                                    palEntry,
                                                    ColorFormat.XRGB1555,
                                                    ColorFormat.ARGB8888);
                                            else
                                                optimizedPalette[i] = GFXUtil.ConvertColorFormat(
                                                    palEntry,
                                                    ColorFormat.ARGB3444,
                                                    ColorFormat.ARGB8888);
                                            break;
                                    }
                                }
                                else
                                {
                                    optimizedPalette[i] = 0xFF000000;
                                }
                            }

                            offs = 0;
                            for (int y = 0; y < Height; y += 4)
                            {
                                for (int x = 0; x < Width; x += 8)
                                {
                                    for (int y2 = 0; y2 < 4; y2++)
                                    {
                                        for (int x2 = 0; x2 < 8; x2++)
                                        {
                                            int posX = x + x2;
                                            int posY = y + y2;
                                            byte index = 0;

                                            if (posY < Height && posX < Width && posY < bitm.Height && posX < bitm.Width)
                                            {
                                                uint color = src[posY * stride + posX];
                                                index = FindClosestColor(color, optimizedPalette);
                                            }
                                            TexData[offs++] = index;
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    case ImageFormat.CI14X2:
                        {
                            Dictionary<uint, int> colorCounts = new Dictionary<uint, int>();
                            for (int y = 0; y < Height; y++)
                            {
                                for (int x = 0; x < Width; x++)
                                {
                                    uint argb = src[y * stride + x];
                                    if (colorCounts.ContainsKey(argb)) colorCounts[argb]++;
                                    else colorCounts.Add(argb, 1);
                                }
                            }

                            var sortedColors = colorCounts.OrderByDescending(o => o.Value)
                            .Select(o => o.Key)
                            .Take(16384)
                            .ToList();
                            uint[] palette = new uint[16384];
                            int paletteSize = sortedColors.Count;
                            for (int i = 0; i < paletteSize; i++)
                                palette[i] = sortedColors[i];
                            for (int i = paletteSize; i < 16384; i++)
                                palette[i] = 0;
                            int palSize = 16384 * 2;
                            if (PalData == null || PalData.Length < palSize)
                                PalData = new byte[palSize];

                            for (int i = 0; i < 16384; i++)
                            {
                                ushort palEntry;
                                switch (PalFormat)
                                {
                                    case PaletteFormat.IA8:
                                        byte a = (byte)((palette[i] >> 24) & 0xFF);
                                        byte r = (byte)((palette[i] >> 16) & 0xFF);
                                        byte g = (byte)((palette[i] >> 8) & 0xFF);
                                        byte b = (byte)(palette[i] & 0xFF);
                                        byte intensity = (byte)((r + g + b) / 3);
                                        palEntry = (ushort)((a << 8) | intensity);
                                        break;
                                    case PaletteFormat.RGB565:
                                        palEntry = (ushort)GFXUtil.ConvertColorFormat(
                                            palette[i], ColorFormat.ARGB8888, ColorFormat.RGB565);
                                        break;
                                    case PaletteFormat.RGB5A3:
                                        byte alpha = (byte)((palette[i] >> 24) & 0xFF);
                                        if (alpha > 0xDA)
                                        {
                                            palEntry = (ushort)GFXUtil.ConvertColorFormat(
                                                palette[i] | 0xFF000000,
                                                ColorFormat.ARGB8888,
                                                ColorFormat.XRGB1555
                                            );
                                            palEntry |= 0x8000;
                                        }
                                        else
                                        {
                                            palEntry = (ushort)GFXUtil.ConvertColorFormat(
                                                palette[i],
                                                ColorFormat.ARGB8888,
                                                ColorFormat.ARGB3444
                                            );
                                        }
                                        break;
                                    default:
                                        throw new Exception("Unsupported palette format!");
                                }
                                IOUtil.WriteU16BE(PalData, i * 2, palEntry);
                            }

                            uint[] optimizedPalette = new uint[16384];
                            for (int i = 0; i < 16384; i++)
                            {
                                ushort palEntry = IOUtil.ReadU16BE(PalData, i * 2);
                                switch (PalFormat)
                                {
                                    case PaletteFormat.IA8:
                                        byte intensity = (byte)(palEntry & 0xFF);
                                        byte alpha = (byte)(palEntry >> 8);
                                        optimizedPalette[i] = GFXUtil.ToColorFormat(
                                            alpha, intensity, intensity, intensity, ColorFormat.ARGB8888);
                                        break;
                                    case PaletteFormat.RGB565:
                                        optimizedPalette[i] = GFXUtil.ConvertColorFormat(
                                            palEntry, ColorFormat.RGB565, ColorFormat.ARGB8888);
                                        break;
                                    case PaletteFormat.RGB5A3:
                                        if ((palEntry & 0x8000) != 0)
                                            optimizedPalette[i] = GFXUtil.ConvertColorFormat(
                                                palEntry, ColorFormat.XRGB1555, ColorFormat.ARGB8888);
                                        else
                                            optimizedPalette[i] = GFXUtil.ConvertColorFormat(
                                                palEntry, ColorFormat.ARGB3444, ColorFormat.ARGB8888);
                                        break;
                                }
                            }

                            for (int y = 0; y < Height; y += 4)
                            {
                                for (int x = 0; x < Width; x += 4)
                                {
                                    for (int y2 = 0; y2 < 4; y2++)
                                    {
                                        for (int x2 = 0; x2 < 4; x2++)
                                        {
                                            int posX = x + x2;
                                            int posY = y + y2;
                                            ushort index = 0;

                                            if (posY < Height && posX < Width)
                                            {
                                                uint color = src[posY * stride + posX];
                                                index = FindClosestColorInPalette(color, optimizedPalette);
                                            }
                                            IOUtil.WriteU16BE(TexData, offs, index);
                                            offs += 2;
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    case ImageFormat.CMPR:
                        {
                            for (int y = 0; y < Height; y += 8)
                            {
                                for (int x = 0; x < Width; x += 8)
                                {
                                    for (int y2 = 0; y2 < 8; y2 += 4)
                                    {
                                        for (int x2 = 0; x2 < 8; x2 += 4)
                                        {
                                            uint[] blockPixels = new uint[16];
                                            int idx = 0;
                                            for (int y3 = 0; y3 < 4; y3++)
                                            {
                                                int posY = y + y2 + y3;
                                                for (int x3 = 0; x3 < 4; x3++)
                                                {
                                                    int posX = x + x2 + x3;
                                                    blockPixels[idx++] = (posX < Width && posY < Height)
                                                        ? src[posY * stride + posX] : 0;
                                                }
                                            }

                                            int transparentCount = 0;
                                            foreach (uint pixel in blockPixels)
                                            {
                                                if ((pixel >> 24) < 128) transparentCount++;
                                            }
                                            bool useTransparentMode = transparentCount > 12;

                                            ushort color0 = 0, color1 = 0;
                                            uint minColor = 0xFFFFFFFF, maxColor = 0;
                                            foreach (uint pixel in blockPixels)
                                            {
                                                if ((pixel >> 24) == 0) continue;
                                                if (pixel < minColor) minColor = pixel;
                                                if (pixel > maxColor) maxColor = pixel;
                                            }

                                            if (minColor == 0xFFFFFFFF)
                                            {
                                                minColor = maxColor = 0;
                                            }

                                            color0 = (ushort)GFXUtil.ConvertColorFormat(minColor, ColorFormat.ARGB8888, ColorFormat.RGB565);
                                            color1 = (ushort)GFXUtil.ConvertColorFormat(maxColor, ColorFormat.ARGB8888, ColorFormat.RGB565);

                                            if (useTransparentMode)
                                            {
                                                if (color0 > color1) (color0, color1) = (color1, color0);
                                            }
                                            else if (color0 < color1)
                                            {
                                                (color0, color1) = (color1, color0);
                                            }

                                            uint[] palette = new uint[4];
                                            palette[0] = GFXUtil.ConvertColorFormat(color0, ColorFormat.RGB565, ColorFormat.ARGB8888);
                                            palette[1] = GFXUtil.ConvertColorFormat(color1, ColorFormat.RGB565, ColorFormat.ARGB8888);

                                            void Unpack(uint color, out byte r, out byte g, out byte b)
                                            {
                                                r = (byte)((color >> 16) & 0xFF);
                                                g = (byte)((color >> 8) & 0xFF);
                                                b = (byte)(color & 0xFF);
                                            }

                                            Unpack(palette[0], out byte r0, out byte g0, out byte b0);
                                            Unpack(palette[1], out byte r1, out byte g1, out byte b1);

                                            if (color0 > color1)
                                            {
                                                palette[2] = GFXUtil.ToColorFormat(
                                                    (2 * r0 + r1) / 3,
                                                    (2 * g0 + g1) / 3,
                                                    (2 * b0 + b1) / 3,
                                                    ColorFormat.ARGB8888
                                                );
                                                palette[3] = GFXUtil.ToColorFormat(
                                                    (r0 + 2 * r1) / 3,
                                                    (g0 + 2 * g1) / 3,
                                                    (b0 + 2 * b1) / 3,
                                                    ColorFormat.ARGB8888
                                                );
                                            }
                                            else
                                            {
                                                palette[2] = GFXUtil.ToColorFormat(
                                                    (r0 + r1) / 2,
                                                    (g0 + g1) / 2,
                                                    (b0 + b1) / 2,
                                                    ColorFormat.ARGB8888
                                                );
                                                palette[3] = 0;
                                            }
                                            uint indices = 0;
                                            for (int i = 0; i < 16; i++)
                                            {
                                                uint pixel = blockPixels[i];
                                                byte bestIndex = 0;
                                                if (useTransparentMode && (pixel >> 24) < 128)
                                                {
                                                    bestIndex = 3;
                                                }
                                                else
                                                {
                                                    double minDist = double.MaxValue;
                                                    for (byte j = 0; j < 4; j++)
                                                    {
                                                        if (palette[j] == 0 && j == 3) continue;

                                                        double dist = ColorDistance(pixel, palette[j]);
                                                        if (dist < minDist)
                                                        {
                                                            minDist = dist;
                                                            bestIndex = j;
                                                        }
                                                    }
                                                }
                                                indices = (indices << 2) | bestIndex;
                                            }
                                            IOUtil.WriteU16BE(TexData, offs, color0);
                                            IOUtil.WriteU16BE(TexData, offs + 2, color1);
                                            IOUtil.WriteU32BE(TexData, offs + 4, indices);
                                            offs += 8;
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    default:
                        throw new NotImplementedException("This format is not implemented yet.");
                }
            }
            finally
            {
                if (d != null)
                    bitm.UnlockBits(d);
            }
        }

        private static byte FindClosestColor(uint target, uint[] palette)
        {
            double minDistance = double.MaxValue;
            byte bestIndex = 0;

            for (byte i = 0; i < palette.Length; i++)
            {
                double dist = CalculateColorDistance(target, palette[i]);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    bestIndex = i;
                }
            }
            return bestIndex;
        }

        private static ushort FindClosestColorInPalette(uint target, uint[] palette)
        {
            double minDistance = double.MaxValue;
            ushort bestIndex = 0;

            for (ushort i = 0; i < palette.Length; i++)
            {
                double dist = CalculateColorDistance(target, palette[i]);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    bestIndex = i;
                }
            }
            return bestIndex;
        }

        private static double CalculateColorDistance(uint c1, uint c2)
        {
            int a1 = (int)((c1 >> 24) & 0xFF);
            int r1 = (int)((c1 >> 16) & 0xFF);
            int g1 = (int)((c1 >> 8) & 0xFF);
            int b1 = (int)(c1 & 0xFF);
            int a2 = (int)((c2 >> 24) & 0xFF);
            int r2 = (int)((c2 >> 16) & 0xFF);
            int g2 = (int)((c2 >> 8) & 0xFF);
            int b2 = (int)(c2 & 0xFF);
            double alphaWeight = a1 > 0 || a2 > 0 ? 0.5 : 0.1;
            return Math.Sqrt(
                alphaWeight * Math.Pow(a1 - a2, 2) +
                Math.Pow(r1 - r2, 2) +
                Math.Pow(g1 - g2, 2) +
                Math.Pow(b1 - b2, 2)
            );
        }

        private static double ColorDistance(uint c1, uint c2)
        {
            int dr = (int)((c1 >> 16) & 0xFF) - (int)((c2 >> 16) & 0xFF);
            int dg = (int)((c1 >> 8) & 0xFF) - (int)((c2 >> 8) & 0xFF);
            int db = (int)(c1 & 0xFF) - (int)(c2 & 0xFF);
            return dr * dr + dg * dg + db * db;
        }
    }
}