using System;
using System.Drawing;
using System.Drawing.Imaging;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.GFX;

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
                                        byte index = TexData[offs];
                                        offs++;
                                        if (posY >= Height || posX >= Width) continue;
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
                                        res[(y + y2) * stride + x + x2] = argb;
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
                                        ushort indexData = IOUtil.ReadU16BE(TexData, offs);
                                        offs += 2;
                                        if (posX >= Width || posY >= Height) continue;
                                        ushort index = (ushort)(indexData & 0x3FFF);
                                        ushort palColor = IOUtil.ReadU16BE(PalData, PalOffset + index * 2);
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
            int dataSize = GetDataSize(Format, Width, Height);
            if (TexData == null || TexData.Length < dataSize) TexData = new byte[dataSize];
            BitmapData d = bitm.LockBits(new Rectangle(0, 0, bitm.Width, bitm.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            uint* src = (uint*)d.Scan0;
            int stride = d.Stride / 4;
            int offs = 0;
            switch (Format)
            {
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
                                        ushort rgb565 = 0;
                                        if (y + y2 < bitm.Height && x + x2 < bitm.Width)
                                        {
                                            uint argb = src[(y + y2) * stride + x + x2];
                                            rgb565 = (ushort)GFXUtil.ConvertColorFormat(argb,
                                                ColorFormat.ARGB8888,
                                                ColorFormat.RGB565
                                            );
                                        }
                                        IOUtil.WriteU16BE(TexData, offs, rgb565);
                                        offs += 2;
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
    }
}