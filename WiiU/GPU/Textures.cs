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
                case ImageFormat.RGBA4:
                    return 11;
                case ImageFormat.LA8:
                case ImageFormat.HILO8:
                    return 7;
                case ImageFormat.L8:
                case ImageFormat.A8:
                    return 1;
                case ImageFormat.LA4:
                    return 2;
                case ImageFormat.BC1:
                    return 49;
                case ImageFormat.BC1_sRGB:
                    return 1073;
                case ImageFormat.BC2:
                    return 50;
                case ImageFormat.BC2_sRGB:
                    return 1074;
                case ImageFormat.BC3:
                    return 51;
                case ImageFormat.BC3_sRGB:
                    return 1075;
                case ImageFormat.BC4L:
                case ImageFormat.BC4A:
                    return 52;
                case ImageFormat.BC5:
                    return 53;
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

        public static readonly uint[] BCnFormats = new uint[]
        {
           GetTextureFormatConstant(ImageFormat.BC1),
           GetTextureFormatConstant(ImageFormat.BC1_sRGB),
           GetTextureFormatConstant(ImageFormat.BC2),
           GetTextureFormatConstant(ImageFormat.BC2_sRGB),
           GetTextureFormatConstant(ImageFormat.BC3),
           GetTextureFormatConstant(ImageFormat.BC3_sRGB),
           GetTextureFormatConstant(ImageFormat.BC4L),
           GetTextureFormatConstant(ImageFormat.BC4A),
           GetTextureFormatConstant(ImageFormat.BC5)
        };

        public static bool IsCompressed(uint Format)
        {
            for (int i = 0; i < BCnFormats.Length; i++)
            {
                if (BCnFormats[i] == Format)
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

        public static byte[] Swizzle(byte[] LinearData, uint OriginalWidth, uint OriginalHeight, uint Format, TileMode TileMode, uint SwizzleConfig, uint Pitch, uint Depth, uint Dimension, AAMode AntiAliasing, uint MipLevel)
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

        public static byte[] Deswizzle(byte[] SwizzledData, uint OriginalWidth, uint OriginalHeight, uint Format, TileMode TileMode, uint SwizzleConfig, uint Pitch, uint Depth, uint Dimension, AAMode AntiAliasing, uint MipLevel)
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
            BitmapData d = null;
            try
            {
                d = bitm.LockBits(new Rectangle(0, 0, bitm.Width, bitm.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
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
                    case ImageFormat.LA8:
                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                if (x >= physicalwidth) continue;
                                if (y >= physicalheight) continue;
                                ushort pixel = IOUtil.ReadU16BE(Data, offs);
                                res[y * stride + x] = GFXUtil.ToColorFormat(
                                        (byte)(pixel & 0xFF),
                                        (byte)((pixel >> 8) & 0xFF),
                                        (byte)((pixel >> 8) & 0xFF),
                                        (byte)((pixel >> 8) & 0xFF),
                                        ColorFormat.ARGB8888);
                                offs += 2;
                            }
                        }
                        break;
                    case ImageFormat.HILO8:
                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                if (x >= physicalwidth) continue;
                                if (y >= physicalheight) continue;
                                ushort pixel = IOUtil.ReadU16BE(Data, offs);
                                res[y * stride + x] = GFXUtil.ToColorFormat(
                                    255,
                                    (byte)(pixel >> 8),
                                    (byte)(pixel & 0xFF),
                                    255,
                                    ColorFormat.ARGB8888);
                                offs += 2;
                            }
                        }
                        break;
                    case ImageFormat.L8:
                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                if (x >= physicalwidth) continue;
                                if (y >= physicalheight) continue;
                                res[y * stride + x] = GFXUtil.ToColorFormat(
                                    Data[offs],
                                    Data[offs],
                                    Data[offs],
                                    ColorFormat.ARGB8888);
                                offs += 1;
                            }
                        }
                        break;
                    case ImageFormat.A8:
                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                if (x >= physicalwidth) continue;
                                if (y >= physicalheight) continue;
                                res[y * stride + x] = GFXUtil.ToColorFormat(
                                    Data[offs],
                                    255,
                                    255,
                                    255,
                                    ColorFormat.ARGB8888);
                                offs += 1;
                            }
                        }
                        break;
                    case ImageFormat.LA4:
                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                if (x >= physicalwidth) continue;
                                if (y >= physicalheight) continue;
                                res[y * stride + x] = GFXUtil.ToColorFormat(
                                    (byte)((Data[offs] >> 4) * 0x11),
                                    (byte)((Data[offs] & 0x0F) * 0x11),
                                    (byte)((Data[offs] & 0x0F) * 0x11),
                                    (byte)((Data[offs] & 0x0F) * 0x11),
                                    ColorFormat.ARGB8888);
                                offs += 1;
                            }
                        }
                        break;
                    case ImageFormat.L4:
                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                if (x >= physicalwidth) continue;
                                if (y >= physicalheight) continue;
                                int byteIndex = offs / 2;
                                int shift = (offs % 2 == 0) ? 4 : 0;
                                res[y * stride + x] = GFXUtil.ToColorFormat(
                                    255,
                                    (byte)(((Data[byteIndex] >> shift) & 0x0F) * 0x11),
                                    (byte)(((Data[byteIndex] >> shift) & 0x0F) * 0x11),
                                    (byte)(((Data[byteIndex] >> shift) & 0x0F) * 0x11),
                                    ColorFormat.ARGB8888);
                                offs += 1;
                            }
                        }
                        break;
                    case ImageFormat.A4:
                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                if (x >= physicalwidth) continue;
                                if (y >= physicalheight) continue;
                                int byteIndex = offs / 2;
                                int shift = (offs % 2 == 0) ? 4 : 0;
                                res[y * stride + x] = GFXUtil.ToColorFormat(
                                    (byte)(((Data[byteIndex] >> shift) & 0x0F) * 0x11),
                                    255,
                                    255,
                                    255,
                                    ColorFormat.ARGB8888);
                                offs += 1;
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
                    case ImageFormat.BC1:
                    case ImageFormat.BC1_sRGB:
                        {
                            int blockWidth = (physicalwidth + 3) / 4;
                            int blockHeight = (physicalheight + 3) / 4;
                            for (int by = 0; by < blockHeight; by++)
                            {
                                for (int bx = 0; bx < blockWidth; bx++)
                                {
                                    if (offs + 8 > Data.Length) break;
                                    int x2 = bx * 4;
                                    int y2 = by * 4;
                                    ushort color0 = IOUtil.ReadU16LE(Data, offs);
                                    ushort color1 = IOUtil.ReadU16LE(Data, offs + 2);
                                    uint data = IOUtil.ReadU32LE(Data, offs + 4);
                                    uint[] blockPixels = DXT.DecodeDXT1(color0, color1, data);
                                    for (int y3 = 0; y3 < 4; y3++)
                                    {
                                        for (int x3 = 0; x3 < 4; x3++)
                                        {
                                            int destX = x2 + x3;
                                            int destY = y2 + y3;
                                            if (destX < physicalwidth && destY < physicalheight)
                                            {
                                                res[destY * stride + destX] = blockPixels[y3 * 4 + x3];
                                            }
                                        }
                                    }
                                    offs += 8;
                                }
                            }
                        }
                        break;
                    case ImageFormat.BC2:
                    case ImageFormat.BC2_sRGB:
                        {
                            int blockWidth = (physicalwidth + 3) / 4;
                            int blockHeight = (physicalheight + 3) / 4;
                            for (int by = 0; by < blockHeight; by++)
                            {
                                for (int bx = 0; bx < blockWidth; bx++)
                                {
                                    if (offs + 16 > Data.Length) break;
                                    int x2 = bx * 4;
                                    int y2 = by * 4;
                                    ulong alphaData = IOUtil.ReadU64LE(Data, offs);
                                    ushort color0 = IOUtil.ReadU16LE(Data, offs + 8);
                                    ushort color1 = IOUtil.ReadU16LE(Data, offs + 10);
                                    uint colorData = IOUtil.ReadU32LE(Data, offs + 12);
                                    uint[] blockPixels = DXT.DecodeDXT3(color0, color1, colorData, alphaData);
                                    for (int y3 = 0; y3 < 4; y3++)
                                    {
                                        for (int x3 = 0; x3 < 4; x3++)
                                        {
                                            int destX = x2 + x3;
                                            int destY = y2 + y3;

                                            if (destX < physicalwidth && destY < physicalheight)
                                            {
                                                res[destY * stride + destX] = blockPixels[y3 * 4 + x3];
                                            }
                                        }
                                    }
                                    offs += 16;
                                }
                            }
                        }
                        break;
                    case ImageFormat.BC3:
                    case ImageFormat.BC3_sRGB:
                        {
                            int blockWidth = (physicalwidth + 3) / 4;
                            int blockHeight = (physicalheight + 3) / 4;
                            for (int by = 0; by < blockHeight; by++)
                            {
                                for (int bx = 0; bx < blockWidth; bx++)
                                {
                                    if (offs + 16 > Data.Length) break;
                                    int x2 = bx * 4;
                                    int y2 = by * 4;
                                    ulong alphaData = IOUtil.ReadU64LE(Data, offs);
                                    ushort color0 = IOUtil.ReadU16LE(Data, offs + 8);
                                    ushort color1 = IOUtil.ReadU16LE(Data, offs + 10);
                                    uint colorData = IOUtil.ReadU32LE(Data, offs + 12);
                                    uint[] blockPixels = DXT.DecodeDXT5(color0, color1, colorData, alphaData);
                                    for (int y3 = 0; y3 < 4; y3++)
                                    {
                                        for (int x3 = 0; x3 < 4; x3++)
                                        {
                                            int destX = x2 + x3;
                                            int destY = y2 + y3;

                                            if (destX < physicalwidth && destY < physicalheight)
                                            {
                                                res[destY * stride + destX] = blockPixels[y3 * 4 + x3];
                                            }
                                        }
                                    }
                                    offs += 16;
                                }
                            }
                        }
                        break;
                    case ImageFormat.BC4L:
                        {
                            int blockWidth = (physicalwidth + 3) / 4;
                            int blockHeight = (physicalheight + 3) / 4;
                            for (int by = 0; by < blockHeight; by++)
                            {
                                for (int bx = 0; bx < blockWidth; bx++)
                                {
                                    if (offs + 8 > Data.Length) break;
                                    int x2 = bx * 4;
                                    int y2 = by * 4;
                                    byte r0 = Data[offs];
                                    byte r1 = Data[offs + 1];
                                    ulong rData = IOUtil.ReadU64LE(Data, offs) >> 16;
                                    byte[] blockPixels = ATI.DecodeATI1(r0, r1, rData, false);
                                    for (int y3 = 0; y3 < 4; y3++)
                                    {
                                        for (int x3 = 0; x3 < 4; x3++)
                                        {
                                            int destX = x2 + x3;
                                            int destY = y2 + y3;
                                            if (destX < physicalwidth && destY < physicalheight)
                                            {
                                                byte intensity = blockPixels[y3 * 4 + x3];
                                                res[destY * stride + destX] = GFXUtil.ToColorFormat(
                                                    255, intensity, intensity, intensity, ColorFormat.ARGB8888);
                                            }
                                        }
                                    }
                                    offs += 8;
                                }
                            }
                        }
                        break;
                    case ImageFormat.BC4A:
                        {
                            int blockWidth = (physicalwidth + 3) / 4;
                            int blockHeight = (physicalheight + 3) / 4;
                            for (int by = 0; by < blockHeight; by++)
                            {
                                for (int bx = 0; bx < blockWidth; bx++)
                                {
                                    if (offs + 8 > Data.Length) break;
                                    int x2 = bx * 4;
                                    int y2 = by * 4;
                                    byte a0 = Data[offs];
                                    byte a1 = Data[offs + 1];
                                    ulong aData = IOUtil.ReadU64LE(Data, offs) >> 16;
                                    byte[] alphaPixels = ATI.DecodeATI1(a0, a1, aData, false);
                                    for (int y3 = 0; y3 < 4; y3++)
                                    {
                                        for (int x3 = 0; x3 < 4; x3++)
                                        {
                                            int destX = x2 + x3;
                                            int destY = y2 + y3;
                                            if (destX < physicalwidth && destY < physicalheight)
                                            {
                                                byte alpha = alphaPixels[y3 * 4 + x3];
                                                res[destY * stride + destX] = GFXUtil.ToColorFormat(
                                                    alpha, 255, 255, 255, ColorFormat.ARGB8888);
                                            }
                                        }
                                    }
                                    offs += 8;
                                }
                            }
                        }
                        break;
                    case ImageFormat.BC5:
                        {
                            int blockWidth = (physicalwidth + 3) / 4;
                            int blockHeight = (physicalheight + 3) / 4;
                            for (int by = 0; by < blockHeight; by++)
                            {
                                for (int bx = 0; bx < blockWidth; bx++)
                                {
                                    if (offs + 16 > Data.Length) break;
                                    int x2 = bx * 4;
                                    int y2 = by * 4;
                                    ulong redBlock = IOUtil.ReadU64LE(Data, offs);
                                    ulong greenBlock = IOUtil.ReadU64LE(Data, offs + 8);
                                    byte red0 = (byte)(redBlock & 0xFF);
                                    byte red1 = (byte)((redBlock >> 8) & 0xFF);
                                    ulong redData = redBlock >> 16;
                                    byte green0 = (byte)(greenBlock & 0xFF);
                                    byte green1 = (byte)((greenBlock >> 8) & 0xFF);
                                    ulong greenData = greenBlock >> 16;
                                    uint[] blockPixels = ATI.DecodeATI2(red0, red1, redData, green0, green1, greenData, IsSNORM: false);
                                    for (int y3 = 0; y3 < 4; y3++)
                                    {
                                        for (int x3 = 0; x3 < 4; x3++)
                                        {
                                            int destX = x2 + x3;
                                            int destY = y2 + y3;
                                            if (destX < physicalwidth && destY < physicalheight)
                                            {
                                                res[destY * stride + destX] = blockPixels[y3 * 4 + x3];
                                            }
                                        }
                                    }
                                    offs += 16;
                                }
                            }
                        }
                        break;
                    default:
                        throw new NotImplementedException("This format is not implemented yet.");
                }
            }
            finally
            {
                if (d != null)
                    bitm.UnlockBits(d);
            }
            return bitm;
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
                d = Picture.LockBits(new Rectangle(0, 0, Picture.Width, Picture.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                uint* res = (uint*)d.Scan0;
                int stride = d.Stride / 4;
                byte[] result = new byte[ConvWidth * ConvHeight * GetBpp(Format) / 8];
                int offs = 0;
                switch (Format)
                {
                    case ImageFormat.RGBA8:
                    case ImageFormat.RGBA8_sRGB:
                        for (int y = 0; y < ConvHeight; y++)
                        {
                            for (int x = 0; x < ConvWidth; x++)
                            {
                                if (x >= physicalwidth) continue;
                                if (y >= physicalheight) continue;
                                uint pixel;
                                pixel = GFXUtil.ConvertColorFormat(res[y * stride + x],
                                    ColorFormat.ARGB8888,
                                    ColorFormat.RGBA8888);
                                IOUtil.WriteU32BE(result, offs, pixel);
                                offs += 4;
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

        private static int ColorClamp(int Color)
        {
            if (Color > 255) Color = 255;
            if (Color < 0) Color = 0;
            return Color;
        }
    }
}