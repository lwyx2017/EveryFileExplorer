using System;
using System.Drawing;
using System.Drawing.Imaging;
using LibEveryFileExplorer.GFX;
using LibEveryFileExplorer.IO;

namespace WiiU.GPU
{
    public class Textures
    {
        public enum TileMode : uint
        {
            Default = 0,
            LinearAligned = 1,
            Tiled1DThin1 = 2,
            Tiled1DThick = 3,
            Tiled2DThin1 = 4,
            Tiled2DThin2 = 5,
            Tiled2DThin4 = 6,
            Tiled2DThick = 7,
            Tiled2BThin1 = 8,
            Tiled2BThin2 = 9,
            Tiled2BThin4 = 10,
            Tiled2BThick = 11,
            Tiled3DThin1 = 12,
            Tiled3DThick = 13,
            Tiled3BThin1 = 14,
            Tiled3BThick = 15,
            LinearSpecial = 16
        }

        public enum AAMode : uint
        {
            AAMode_1X = 0,
            AAMode_2X = 1,
            AAMode_4X = 2,
            AAMode_8X = 3,
        }

        public enum ImageFormat : uint
        {
            RGBA8 = 0,
            RGBA8_sRGB = 1,
            RGBA1010102 = 2,
            RGB8 = 3,
            RGBA5551 = 4,
            RGB555 = 5,
            RGB565 = 6,
            RGBA4 = 7,
            LA8 = 8,
            HILO8 = 9,
            L8 = 10,
            A8 = 11,
            LA4 = 12,
            L4 = 13,
            A4 = 14,
            ETC1 = 15,
            ETC1A4 = 16,
            BC1 = 17,
            BC1_sRGB = 18,
            BC2 = 19,
            BC2_sRGB = 20,
            BC3 = 21,
            BC3_sRGB = 22,
            BC4L = 23,
            BC4A = 24,
            BC5 = 25
        }

        private static readonly int[] Bpp = { 32, 32, 32, 24, 16, 16, 16, 16, 16, 16, 8, 8, 8, 4, 4, 4, 8, 4, 4, 8, 8, 8, 8, 4, 4, 8 };

        public static int GetBpp(ImageFormat Format) { return Bpp[(uint)Format]; }

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

        public static uint GetTextureFormatConstant(ImageFormat Format)
        {
            switch (Format)
            {
                case ImageFormat.RGBA8:
                case ImageFormat.RGB8:
                    return 26;
                case ImageFormat.RGBA8_sRGB:
                    return 1050;
                case ImageFormat.RGBA1010102:
                    return 25;
                case ImageFormat.RGBA5551:
                    return 10;
                case ImageFormat.RGB565:
                    return 8;
                default:
                    return 0;
            }
        }

        public static uint GetTextureFormatConstant(string formatName)
        {
            if (Enum.TryParse<ImageFormat>(formatName, out ImageFormat format))
            {
                return GetTextureFormatConstant(format);
            }
            return 0;
        }

        public static readonly uint[] BCFormats = new uint[]
        {
          (uint)ImageFormat.BC1,
          (uint)ImageFormat.BC1_sRGB,
          (uint)ImageFormat.BC2,
          (uint)ImageFormat.BC2_sRGB,
          (uint)ImageFormat.BC3,
          (uint)ImageFormat.BC3_sRGB,
          (uint)ImageFormat.BC4L,
          (uint)ImageFormat.BC4A,
          (uint)ImageFormat.BC5
        };

        public static bool IsCompressed(uint Format)
        {
            for (int i = 0; i < BCFormats.Length; i++)
            {
                if (BCFormats[i] == Format)
                {
                    return true;
                }
            }
            return false;
        }

        public static R600Tiling._ADDR_COMPUTE_SURFACE_ADDRFROMCOORD_OUTPUT CalculateParameters(uint Format, uint Width, uint Height, uint Depth, uint Dim, TileMode TileMode, AAMode AA, uint MipLevel)
        {
            R600Tiling t = new R600Tiling();
            return t.GetSurfaceInfo(Format, Width, Height, Depth, Dim, (uint)TileMode, (uint)AA, MipLevel);
        }

        public static byte[] Swizzle(byte[] LinearData, uint OriginalWidth, uint OriginalHeight, uint Format, uint TileMode, uint SwizzleConfig, uint Pitch, uint Depth, uint Dimension, AAMode AntiAliasing, uint MipLevel)
        {
            byte[] SwizzledData = new byte[LinearData.Length];
            Array.Copy(LinearData, 0, SwizzledData, 0, LinearData.Length);
            uint MipLevelWidth = Math.Max(OriginalWidth >> (int)MipLevel, 1);
            uint MipLevelHeight = Math.Max(OriginalHeight >> (int)MipLevel, 1);
            if (IsCompressed(Format))
            {
                FixDimension(ref MipLevelWidth, ref MipLevelHeight);
            }
            R600Tiling._ADDR_COMPUTE_SURFACE_ADDRFROMCOORD_OUTPUT SurfaceParams =
            CalculateParameters(Format, OriginalWidth, OriginalHeight, Depth, Dimension, (TileMode)TileMode, (AAMode)AntiAliasing, MipLevel);
            uint PipeSwizzleMode = (SwizzleConfig >> 8) & 1;
            uint BankSwizzleMode = (SwizzleConfig >> 9) & 3;
            uint BitsPerPixel = SurfaceParams.BPP;
            uint BytesPerPixel = BitsPerPixel / 8;
            for (uint RowIndex = 0; RowIndex < MipLevelHeight; RowIndex++)
            {
                for (uint ColumnIndex = 0; ColumnIndex < MipLevelWidth; ColumnIndex++)
                {
                    uint SwizzledAddress = CalculateSwizzledAddress(ColumnIndex, RowIndex, SurfaceParams, PipeSwizzleMode, BankSwizzleMode);
                    uint LinearAddress = (RowIndex * MipLevelWidth + ColumnIndex) * BytesPerPixel;
                    if (SwizzledAddress + BytesPerPixel <= SwizzledData.Length &&
                        LinearAddress + BytesPerPixel <= LinearData.Length)
                    {
                        for (int ByteOffset = 0; ByteOffset < BytesPerPixel; ByteOffset++)
                        {
                            SwizzledData[SwizzledAddress + (uint)ByteOffset] = LinearData[LinearAddress + (uint)ByteOffset];
                        }
                    }
                }
            }
            return SwizzledData;
        }

        public static byte[] Deswizzle(byte[] SwizzledData, uint OriginalWidth, uint OriginalHeight, uint Format, uint TileMode, uint SwizzleConfig, uint Pitch, uint Depth, uint Dimension, AAMode AntiAliasing, uint MipLevel)
        {
            byte[] DeswizzledData = new byte[SwizzledData.Length];
            Array.Copy(SwizzledData, 0, DeswizzledData, 0, SwizzledData.Length);
            uint MipLevelWidth = Math.Max(OriginalWidth >> (int)MipLevel, 1);
            uint MipLevelHeight = Math.Max(OriginalHeight >> (int)MipLevel, 1);
            if (IsCompressed(Format))
            {
                FixDimension(ref MipLevelWidth, ref MipLevelHeight);
            }
            R600Tiling._ADDR_COMPUTE_SURFACE_ADDRFROMCOORD_OUTPUT SurfaceParams =
            CalculateParameters(Format, OriginalWidth, OriginalHeight, Depth, Dimension, (TileMode)TileMode, (AAMode)AntiAliasing, MipLevel);
            uint PipeSwizzleMode = (SwizzleConfig >> 8) & 1;
            uint BankSwizzleMode = (SwizzleConfig >> 9) & 3;
            uint BitsPerPixel = SurfaceParams.BPP;
            uint BytesPerPixel = BitsPerPixel / 8;
            for (uint RowIndex = 0; RowIndex < MipLevelHeight; RowIndex++)
            {
                for (uint ColumnIndex = 0; ColumnIndex < MipLevelWidth; ColumnIndex++)
                {
                    uint SwizzledAddress = CalculateSwizzledAddress(ColumnIndex, RowIndex, SurfaceParams, PipeSwizzleMode, BankSwizzleMode);
                    uint LinearAddress = (RowIndex * MipLevelWidth + ColumnIndex) * BytesPerPixel;
                    if (LinearAddress + BytesPerPixel <= DeswizzledData.Length &&
                        SwizzledAddress + BytesPerPixel <= SwizzledData.Length)
                    {
                        for (int ByteOffset = 0; ByteOffset < BytesPerPixel; ByteOffset++)
                        {
                            DeswizzledData[LinearAddress + (uint)ByteOffset] = SwizzledData[SwizzledAddress + (uint)ByteOffset];
                        }
                    }
                }
            }
            return DeswizzledData;
        }

        private static uint CalculateSwizzledAddress(uint ColumnIndex, uint RowIndex, R600Tiling._ADDR_COMPUTE_SURFACE_ADDRFROMCOORD_OUTPUT SurfaceParams, uint PipeSwizzleMode, uint BankSwizzleMode)
        {
            if (SurfaceParams.TileMode == 0 || SurfaceParams.TileMode == 1)
            {
                return R600Tiling.ComputeSurfaceAddrFromCoordLinear(ColumnIndex, RowIndex, SurfaceParams.BPP, SurfaceParams.Pitch);
            }
            else if (SurfaceParams.TileMode == 2 || SurfaceParams.TileMode == 3)
            {
                return R600Tiling.ComputeSurfaceAddrFromCoordMicroTiled(ColumnIndex, RowIndex, SurfaceParams.BPP, SurfaceParams.Pitch, SurfaceParams.TileMode);
            }
            else
            {
                return R600Tiling.ComputeSurfaceAddrFromCoordMacroTiled(ColumnIndex, RowIndex, SurfaceParams.BPP, SurfaceParams.Pitch, SurfaceParams.Height, SurfaceParams.TileMode, PipeSwizzleMode, BankSwizzleMode);
            }
        }

        public static void FixDimension(ref uint Width, ref uint Height)
        {
            Width += 3;
            Height += 3;
            Width /= 4;
            Height /= 4;
        }

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
            int offs = Offset;
            int stride = d.Stride / 4;
            switch (Format)
            {
                case ImageFormat.RGBA8:
                case ImageFormat.RGBA8_sRGB:
                    for (int y = 0; y < Height; y++)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            if (x >= physicalwidth) continue;
                            if (y >= physicalheight) continue;
                            res[y * stride + x] =
                                GFXUtil.ConvertColorFormat(
                                    IOUtil.ReadU32BE(Data, offs),
                                    ColorFormat.RGBA8888,
                                    ColorFormat.ARGB8888);
                            offs += 4;
                        }
                    }
                    break;
                case ImageFormat.RGBA1010102:
                    for (int y = 0; y < Height; y++)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            if (x >= physicalwidth) continue;
                            if (y >= physicalheight) continue;
                            res[y * stride + x] =
                                GFXUtil.ConvertColorFormat(
                                    IOUtil.ReadU32BE(Data, offs),
                                    ColorFormat.RGBA1010102,
                                    ColorFormat.ARGB8888);
                            offs += 4;
                        }
                    }
                    break;
                case ImageFormat.RGB8:
                    for (int y = 0; y < Height; y++)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            if (x >= physicalwidth) continue;
                            if (y >= physicalheight) continue;
                            res[y * stride + x] =
                                GFXUtil.ConvertColorFormat(
                                    IOUtil.ReadU24BE(Data, offs),
                                    ColorFormat.RGB888,
                                    ColorFormat.ARGB8888);
                            offs += 3;
                        }
                    }
                    break;
                case ImageFormat.RGBA5551:
                    for (int y = 0; y < Height; y++)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            if (x >= physicalwidth) continue;
                            if (y >= physicalheight) continue;
                            res[y * stride + x] =
                                GFXUtil.ConvertColorFormat(
                                    IOUtil.ReadU16BE(Data, offs),
                                    ColorFormat.RGBA5551,
                                    ColorFormat.ARGB8888);
                            offs += 2;
                        }
                    }
                    break;
                case ImageFormat.RGB565:
                    for (int y = 0; y < Height; y++)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            if (x >= physicalwidth) continue;
                            if (y >= physicalheight) continue;
                            res[y * stride + x] =
                                GFXUtil.ConvertColorFormat(
                                    IOUtil.ReadU16LE(Data, offs),
                                    ColorFormat.RGB565,
                                    ColorFormat.ARGB8888);
                            offs += 2;
                        }
                    }
                    break;
                case ImageFormat.RGB555:
                    for (int y = 0; y < Height; y++)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            if (x >= physicalwidth) continue;
                            if (y >= physicalheight) continue;
                            res[y * stride + x] =
                                GFXUtil.ConvertColorFormat(
                                    IOUtil.ReadU16BE(Data, offs),
                                    ColorFormat.XRGB1555,
                                    ColorFormat.ARGB8888);
                            offs += 2;
                        }
                    }
                    break;
                case ImageFormat.RGBA4:
                    for (int y = 0; y < Height; y++)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            if (x >= physicalwidth) continue;
                            if (y >= physicalheight) continue;
                            res[y * stride + x] =
                                GFXUtil.ConvertColorFormat(
                                    IOUtil.ReadU16BE(Data, offs),
                                    ColorFormat.RGBA4444,
                                    ColorFormat.ARGB8888);
                            offs += 2;
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
                                        ulong data = IOUtil.ReadU64BE(Data, offs);
                                        if (Format == ImageFormat.ETC1A4)
                                        {
                                            offs += 8;
                                            alpha = IOUtil.ReadU64BE(Data, offs);
                                        }
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
                case ImageFormat.BC3:
                case ImageFormat.BC3_sRGB:
                    for (int y2 = 0; y2 < Height; y2 += 4)
                    {
                        for (int x2 = 0; x2 < Width; x2 += 4)
                        {
                            ulong a_data = IOUtil.ReadU64BE(Data, offs);
                            byte[] AlphaPalette = new byte[8];
                            AlphaPalette[0] = (byte)(a_data & 0xFF);
                            AlphaPalette[1] = (byte)((a_data >> 8) & 0xFF);
                            a_data >>= 16;
                            if (AlphaPalette[0] > AlphaPalette[1])
                            {
                                AlphaPalette[2] = (byte)((6 * AlphaPalette[0] + 1 * AlphaPalette[1]) / 7);
                                AlphaPalette[3] = (byte)((5 * AlphaPalette[0] + 2 * AlphaPalette[1]) / 7);
                                AlphaPalette[4] = (byte)((4 * AlphaPalette[0] + 3 * AlphaPalette[1]) / 7);
                                AlphaPalette[5] = (byte)((3 * AlphaPalette[0] + 4 * AlphaPalette[1]) / 7);
                                AlphaPalette[6] = (byte)((2 * AlphaPalette[0] + 5 * AlphaPalette[1]) / 7);
                                AlphaPalette[7] = (byte)((1 * AlphaPalette[0] + 6 * AlphaPalette[1]) / 7);
                            }
                            else
                            {
                                AlphaPalette[2] = (byte)((4 * AlphaPalette[0] + 1 * AlphaPalette[1]) / 5);
                                AlphaPalette[3] = (byte)((3 * AlphaPalette[0] + 2 * AlphaPalette[1]) / 5);
                                AlphaPalette[4] = (byte)((2 * AlphaPalette[0] + 3 * AlphaPalette[1]) / 5);
                                AlphaPalette[5] = (byte)((1 * AlphaPalette[0] + 4 * AlphaPalette[1]) / 5);
                                AlphaPalette[6] = 0;
                                AlphaPalette[7] = 255;
                            }
                            offs += 8;
                            ushort color0 = IOUtil.ReadU16LE(Data, offs);
                            ushort color1 = IOUtil.ReadU16LE(Data, offs + 2);
                            uint data = IOUtil.ReadU32LE(Data, offs + 4);
                            uint[] Palette = new uint[4];
                            Palette[0] = GFXUtil.ConvertColorFormat(color0, ColorFormat.RGB565, ColorFormat.ARGB8888);
                            Palette[1] = GFXUtil.ConvertColorFormat(color1, ColorFormat.RGB565, ColorFormat.ARGB8888);
                            Color a = System.Drawing.Color.FromArgb((int)Palette[0]);
                            Color b = System.Drawing.Color.FromArgb((int)Palette[1]);
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
                            int aq = 45;
                            for (int y3 = 0; y3 < 4; y3++)
                            {
                                for (int x3 = 0; x3 < 4; x3++)
                                {
                                    //if (x2 + x3 >= physicalwidth) continue;
                                    if (y2 + y3 >= physicalheight) continue;
                                    res[(y2 + y3) * stride + x2 + x3] = (Palette[(data >> q) & 3] & 0xFFFFFF) | ((uint)AlphaPalette[(a_data >> aq) & 7] << 24);
                                    q -= 2;
                                    aq -= 3;
                                }
                            }
                            offs += 8;
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException("This format is not implemented yet.");
            }
            bitm.UnlockBits(d);
            return bitm;
        }

        private static int ColorClamp(int Color)
        {
            if (Color > 255) Color = 255;
            if (Color < 0) Color = 0;
            return Color;
        }
    }
}