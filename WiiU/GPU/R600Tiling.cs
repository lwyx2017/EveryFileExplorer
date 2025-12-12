using System;

namespace WiiU.GPU
{
    public class R600Tiling
    {
        public static readonly uint[] bankSwapOrder = { 0, 1, 3, 2, 6, 7, 5, 4, 0, 0 };
        public const int m_Banks = 4;
        public const int m_BanksBitCount = 2;
        public const int m_Pipes = 2;
        public const int m_PipesBitCount = 1;
        public const int m_PipeInterleaveBytes = 256;
        public const int m_PipeInterLeaveBytesBitCount = 8;
        public const int m_RowSize = 2048;
        public const int m_SwapSize = 256;
        public const int m_SplitSize = 2048;
        public const int m_ChipFamily = 2;
        public const int MicroTilePixels = 64;
        public uint m_ConfigFlags = 4;

        private const uint ElementMode_Default = 3;
        private const uint ElementMode_Compressed = 7;
        private const uint ElementMode_BitPacked = 8;
        private const uint ElementMode_Palette = 6;
        private const uint ElementMode_DepthStencil = 5;
        private const uint ElementMode_PackedRGB = 4;
        private const uint ElementMode_ThickMacroTile = 9;
        private const uint ElementMode_ThickMacroTileAlt = 12;
        private const uint ElementMode_ThickMacroTile128 = 10;
        private const uint ElementMode_ThickMacroTile128Alt = 11;
        private const uint ElementMode_ThickMacroTileRGBA = 13;
        private const uint ElementHeight_Default = 1;
        private const uint ElementHeight_ThickTile = 4;
        private const uint ElementWidth_Default = 1;
        private const uint ElementWidth_BitPacked = 8;
        private const uint ElementWidth_PackedRGB = 3;
        private const uint ElementWidth_ThickTile = 4;

        const uint ElemMode_PackedRGB = 4;
        const uint ElemMode_DepthStencil = 5;
        const uint ElemMode_Palette = 6;
        const uint ElemMode_Compressed = 7;
        const uint ElemMode_BitPacked = 8;
        const uint ElemMode_ThickMacroTile = 9;
        const uint ElemMode_ThickMacroTileAlt = 12;
        const uint ElemMode_ThickMacroTile128 = 10;
        const uint ElemMode_ThickMacroTile128Alt = 11;
        const uint ElemMode_ThickMacroTileRGBA = 13;
        uint adjustedBPP = 0;

        private const uint Format_ThickMacroTile_Start = 49;
        private const uint Format_ThickMacroTile_End = 55;
        private const uint Flag_Mipmap_ScaleDimensions = 1 << 12;
        private const uint Flag_Mipmap_PreserveDepth = 1 << 4;

        private const uint Format_PackedRGB_96BPP_Start = 47;
        private const uint Format_PackedRGB_96BPP_End = 48;
        private const uint Align_4Bytes = 4;

        private const uint TileMode_MacroTiled_RangeStart = 4;
        private const uint TileMode_MacroTiled_RangeEnd = 11;
        private const uint TileMode_AltMacroTiled_RangeStart = 12;
        private const uint TileMode_AltMacroTiled_RangeEnd = 15;

        const uint MacroTileRotationFactor = 2;
        const uint AltMacroTileRotationFactor = 1;
        uint rotationFactor = 0;

        public const uint MacroTilePitch_Base = 32;
        public const uint MacroTileWidth_Base = 32;
        public const uint MacroTileHeight_Base = 16;
        public const uint MicroTilePixelSize_Base = 8;

        private _ADDR_COMPUTE_SURFACE_ADDRFROMCOORD_INPUT AIn;
        private _ADDR_COMPUTE_SURFACE_ADDRFROMCOORD_OUTPUT AOut;
        private uint ElemMode;
        private uint ExportandY;
        private uint ExportandX;
        private uint BaseAlignment;
        private uint PitchAlignment;
        private uint HeightAlignment;
        private uint ExportPitch;
        private uint ExportHeight;
        private uint ExportNrSlices;

        private static byte[] FormatHwInfo =
        {
          0, 0, 0, 1, 8, 3, 0, 1, 8, 1, 0, 1, 0, 0, 0, 1,
          0, 0, 0, 1, 16, 7, 0, 0, 16, 3, 0, 1, 16, 3, 0, 1,
          16, 11, 0, 1, 16, 1, 0, 1, 16, 3, 0, 1, 16, 3, 0, 1,
          16, 3, 0, 1, 32, 3, 0, 0, 32, 7, 0, 0, 32, 3, 0, 0,
          32, 3, 0, 1, 32, 5, 0, 0, 0, 0, 0, 0, 32, 3, 0, 0,
          0, 0, 0, 0, 0, 0, 0, 1, 32, 3, 0, 1, 0, 0, 0, 1,
          0, 0, 0, 1, 32, 11, 0, 1, 32, 11, 0, 1, 32, 11, 0, 1,
          64, 5, 0, 0, 64, 3, 0, 0, 64, 3, 0, 0, 64, 3, 0, 0,
          64, 3, 0, 1, 0, 0, 0, 0, 128, 3, 0, 0, 128, 3, 0, 0,
          0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 16, 1, 0, 0,
          16, 1, 0, 0, 32, 1, 0, 0, 32, 1, 0, 0, 32, 1, 0, 0,
          0, 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 0, 96, 1, 0, 0,
          96, 1, 0, 0, 64, 1, 0, 1, 128, 1, 0, 1, 128, 1, 0, 1,
          64, 1, 0, 1, 128, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0,
          0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
         0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        };

        private static byte[] FormatExInfo =
        {
          0, 1, 1, 3, 8, 1, 1, 3, 8, 1, 1, 3, 8, 1, 1, 3,
          0, 1, 1, 3, 16, 1, 1, 3, 16, 1, 1, 3, 16, 1, 1, 3,
          16, 1, 1, 3, 16, 1, 1, 3, 16, 1, 1, 3, 16, 1, 1, 3,
          16, 1, 1, 3, 32, 1, 1, 3, 32, 1, 1, 3, 32, 1, 1, 3,
          32, 1, 1, 3, 32, 1, 1, 3, 32, 1, 1, 3, 32, 1, 1, 3,
          32, 1, 1, 3, 32, 1, 1, 3, 32, 1, 1, 3, 32, 1, 1, 3,
          32, 1, 1, 3, 32, 1, 1, 3, 32, 1, 1, 3, 32, 1, 1, 3,
          64, 1, 1, 3, 64, 1, 1, 3, 64, 1, 1, 3, 64, 1, 1, 3,
          64, 1, 1, 3, 0, 1, 1, 3, 128, 1, 1, 3, 128, 1, 1, 3,
          0, 1, 1, 3, 1, 8, 1, 5, 1, 8, 1, 6, 16, 1, 1, 7,
          16, 1, 1, 8, 32, 1, 1, 3, 32, 1, 1, 3, 32, 1, 1, 3,
          24, 3, 1, 4, 48, 3, 1, 4, 48, 3, 1, 4, 96, 3, 1, 4,
          96, 3, 1, 4, 64, 4, 4, 9, 128, 4, 4, 10, 128, 4, 4, 11,
          64, 4, 4, 12, 64, 4, 4, 13, 64, 4, 4, 13, 64, 4, 4, 13,
          0, 1, 1, 3, 0, 1, 1, 3, 0, 1, 1, 3, 0, 1, 1, 3,
          0, 1, 1, 3, 0, 1, 1, 3, 64, 1, 1, 3, 0, 1, 1, 3,
        };

        public class _ADDR_COMPUTE_SURFACE_ADDRFROMCOORD_INPUT
        {
            public uint Size;
            public uint TileMode;
            public uint Format;
            public uint BPP;
            public uint NrSamples;
            public uint Width;
            public uint Height;
            public uint NrSlices;
            public uint Slice;
            public uint MipLevel;
            public uint Flags;
            public uint NrFrags;
            public TileInfo ATileInfo;
            public uint TileIndex;
        }

        public class _ADDR_COMPUTE_SURFACE_ADDRFROMCOORD_OUTPUT
        {
            public uint Size;
            public uint Pitch;
            public uint Height;
            public uint Depth;
            public uint SurfaceSize;
            public uint TileMode;
            public uint BaseAlignment;
            public uint PitchAlignment;
            public uint HeightAlignment;
            public uint DepthAlignment;
            public uint BPP;
            public uint PixelPitch;
            public uint PixelHeight;
            public uint PixelBits;
            public uint SliceSize;
            public uint PitchTileMax;
            public uint HeightTileMax;
            public uint SliceTileMax;
            public TileInfo ATileInfo;
            public uint TileType;
            public uint TileIndex;
        }

        public class TileInfo
        {
            public uint Banks;
            public uint BankWidth;
            public uint BankHeight;
            public uint MacroAspectRatio;
            public uint TileSplitBytes;
            public uint PipeConfig;
        }

        public static uint ComputeSurfaceAddrFromCoordLinear(uint x, uint y, uint BPP, uint Pitch)
        {
            return (y * Pitch + x) * BPP / 8;
        }

        public static uint ComputeSurfaceAddrFromCoordMicroTiled(uint x, uint y, uint BPP, uint Pitch, uint TileMode)
        {
            uint microTileThickness = 1;
            if (TileMode == 3)
            {
                microTileThickness = 4;
            }
            uint microTileBytes = (64 * microTileThickness * BPP + 7) / 8;
            uint microTilesPerRow = Pitch >> 3;
            uint microTileIndexX = x >> 3;
            uint microTileIndexY = y >> 3;
            uint microTileOffset = microTileBytes * (microTileIndexX + microTileIndexY * microTilesPerRow);
            uint pixelIndex = ComputePixelIndexWithinMicroTile(x, y, BPP, TileMode);
            return (BPP * pixelIndex >> 3) + microTileOffset;
        }

        public static uint ComputePixelIndexWithinMicroTile(uint x, uint y, uint BPP, uint TileMode)
        {
            uint pixelBit0 = 0;
            uint pixelBit1 = 0;
            uint pixelBit2 = 0;
            uint pixelBit3 = 0;
            uint pixelBit4 = 0;
            uint pixelBit5 = 0;
            uint pixelBit6 = 0;
            uint pixelBit7 = 0;
            uint pixelBit8 = 0;

            uint thickness = ComputeSurfaceThickness(TileMode);

            switch (BPP)
            {
                case 8:
                    pixelBit0 = x & 1;
                    pixelBit1 = (x & 2) >> 1;
                    pixelBit2 = (x & 4) >> 2;
                    pixelBit3 = (y & 2) >> 1;
                    pixelBit4 = y & 1;
                    pixelBit5 = (y & 4) >> 2;
                    break;
                case 16:
                    pixelBit0 = x & 1;
                    pixelBit1 = (x & 2) >> 1;
                    pixelBit2 = (x & 4) >> 2;
                    pixelBit3 = y & 1;
                    pixelBit4 = (y & 2) >> 1;
                    pixelBit5 = (y & 4) >> 2;
                    break;
                case 32:
                case 96:
                    pixelBit0 = x & 1;
                    pixelBit1 = (x & 2) >> 1;
                    pixelBit2 = y & 1;
                    pixelBit3 = (x & 4) >> 2;
                    pixelBit4 = (y & 2) >> 1;
                    pixelBit5 = (y & 4) >> 2;
                    break;
                case 64:
                    pixelBit0 = x & 1;
                    pixelBit1 = y & 1;
                    pixelBit2 = (x & 2) >> 1;
                    pixelBit3 = (x & 4) >> 2;
                    pixelBit4 = (y & 2) >> 1;
                    pixelBit5 = (y & 4) >> 2;
                    break;
                case 128:
                    pixelBit0 = y & 1;
                    pixelBit1 = x & 1;
                    pixelBit2 = (x & 2) >> 1;
                    pixelBit3 = (x & 4) >> 2;
                    pixelBit4 = (y & 2) >> 1;
                    pixelBit5 = (y & 4) >> 2;
                    break;
                default:
                    pixelBit0 = x & 1;
                    pixelBit1 = (x & 2) >> 1;
                    pixelBit2 = y & 1;
                    pixelBit3 = (x & 4) >> 2;
                    pixelBit4 = (y & 2) >> 1;
                    pixelBit5 = (y & 4) >> 2;
                    break;
            }

            uint z = 0;

            if (thickness > 1)
            {
                pixelBit6 = z & 1;
                pixelBit7 = (z & 2) >> 1;
            }

            if (thickness == 8)
            {
                pixelBit8 = (z & 4) >> 2;
            }

            return (pixelBit8 << 8) | (pixelBit7 << 7) | (pixelBit6 << 6) |
                   (32 * pixelBit5) | (16 * pixelBit4) | (8 * pixelBit3) |
                   (4 * pixelBit2) | pixelBit0 | (2 * pixelBit1);
        }

        public static uint ComputeSurfaceThickness(uint TileMode)
        {
            uint thickness = 1;
            switch (TileMode)
            {
                case 3:
                case 7:
                case 11:
                case 13:
                case 15:
                    thickness = 4;
                    break;
                case 16:
                case 17:
                    thickness = 8;
                    break;
            }
            return thickness;
        }

        public static uint SurfaceGetBitsPerPixel(uint Format)
        {
            uint PerPixel = Format & 0x3F;
            return FormatHwInfo[PerPixel * 4];
        }

        public static uint ComputePipeFromCoordWoRotation(uint x, uint y)
        {
            return ((y >> 3) ^ (x >> 3)) & 1;
        }

        public static uint ComputeBankFromCoordWoRotation(uint x, uint y)
        {
            uint surfaceRotationFactor = 2;
            uint bankCount = (uint)m_Banks;
            uint bankIndex = 0;
            switch (bankCount)
            {
                case 4:
                    bankIndex = (((y / (16 * surfaceRotationFactor)) ^ (x >> 3)) & 1)
                              | (2 * (((y / (8 * surfaceRotationFactor)) ^ (x >> 4)) & 1));
                    break;
                case 8:
                    bankIndex = (((y / (32 * surfaceRotationFactor)) ^ (x >> 3)) & 1)
                              | (2 * (((y / (32 * surfaceRotationFactor)) ^ ((y / (16 * surfaceRotationFactor)) ^ (x >> 4))) & 1))
                              | (4 * (((y / (8 * surfaceRotationFactor)) ^ (x >> 5)) & 1));
                    break;
            }
            return bankIndex;
        }

        public static uint ComputeSurfaceAddrFromCoordMacroTiled(uint x, uint y, uint BPP, uint Pitch, uint Height, uint TileMode, uint PipeSwizzle, uint BankSwizzle)
        {
            uint pipeCount = m_Pipes;
            uint bankCount = m_Banks;
            uint pipeBitCount = m_PipesBitCount;
            uint bankBitCount = m_BanksBitCount;
            uint pipeInterleaveBitCount = m_PipeInterLeaveBytesBitCount;
            uint rowSize = m_RowSize;
            uint microTileThickness = ComputeSurfaceThickness(TileMode);
            uint microTileTotalBits = BPP * microTileThickness * MicroTilePixels;
            uint microTileBytes = (microTileTotalBits + 7) / 8;
            uint pixelIndexInMicroTile = ComputePixelIndexWithinMicroTile(x, y, BPP, TileMode);
            uint pixelBitsOffset = BPP * pixelIndexInMicroTile;
            uint rowMicroTileCapacity = microTileBytes <= rowSize ? 1 : rowSize / microTileBytes;
            uint tileSlices = Math.Max(1, 1 / rowMicroTileCapacity);
            uint sliceIndex = pixelBitsOffset / (microTileTotalBits / tileSlices);
            pixelBitsOffset %= microTileTotalBits / tileSlices;
            uint pixelByteOffset = (pixelBitsOffset + 7) / 8;
            uint pipeIndex = ComputePipeFromCoordWoRotation(x, y);
            uint bankIndex = ComputeBankFromCoordWoRotation(x, y);
            uint pipeBankIndex = pipeIndex + pipeCount * bankIndex;
            uint swizzleIndex = PipeSwizzle + pipeCount * BankSwizzle;
            uint swizzledPipeBankIndex = (pipeBankIndex ^ ((pipeCount * sliceIndex * ((bankCount >> 1) + 1)) ^ swizzleIndex)) % (pipeCount * bankCount);
            pipeIndex = swizzledPipeBankIndex % pipeCount;
            bankIndex = swizzledPipeBankIndex / pipeCount;
            uint sliceOffset = (Height * Pitch * microTileThickness * BPP * rowMicroTileCapacity + 7) / 8 * (sliceIndex / microTileThickness);
            uint macroTilePitch = MacroTilePitch_Base;
            uint macroTileHeight = MacroTileHeight_Base;
            uint macroAspectRatio = ComputeMacroTileAspectRatio(TileMode);
            macroTilePitch /= macroAspectRatio;
            macroTileHeight *= macroAspectRatio;
            uint macroTilesPerRow = Pitch / macroTilePitch;
            uint macroTileBytes = (microTileThickness * BPP * macroTileHeight * macroTilePitch + 7) / 8;
            uint macroTileXIndex = x / macroTilePitch;
            uint macroTileYIndex = y / macroTileHeight;
            uint macroTileOffset = (macroTileXIndex + macroTilesPerRow * macroTileYIndex) * macroTileBytes;
            if (IsBankSwappedTileMode(TileMode) != 0)
            {
                uint bankSwappedWidth = ComputeSurfaceBankSwappedWidth(TileMode, BPP, Pitch);
                uint bankSwapXIndex = macroTilePitch * macroTileXIndex / bankSwappedWidth;
                bankIndex ^= bankSwapOrder[bankSwapXIndex & 3];
            }
            uint pipeInterleaveMask = (uint)((1 << (int)pipeInterleaveBitCount) - 1);
            int pipeBankBitShift = (int)(bankBitCount + pipeBitCount);
            uint combinedOffset = pixelByteOffset + (macroTileOffset + sliceOffset >> pipeBankBitShift);
            uint highOffset = (combinedOffset & ~pipeInterleaveMask) << pipeBankBitShift;
            uint lowOffset = combinedOffset & pipeInterleaveMask;
            uint pipeOffset = pipeIndex << (int)pipeInterleaveBitCount;
            return (bankIndex << (int)(pipeBitCount + pipeInterleaveBitCount)) | pipeOffset | lowOffset | highOffset;
        }

        public static uint ComputeSurfaceBankSwappedWidth(uint TileMode, uint BPP, uint Pitch, uint NumSamples = 1)
        {
            if (IsBankSwappedTileMode(TileMode) == 0)
            {
                return 0;
            }
            uint bankCount = (uint)m_Banks;
            uint pipeCount = (uint)m_Pipes;
            uint pipeInterleaveBytes = (uint)m_PipeInterleaveBytes;
            uint rowSize = (uint)m_RowSize;
            uint splitSize = (uint)m_SplitSize;
            uint swapSize = (uint)m_SwapSize;

            uint microTileBits = 8 * BPP;
            uint microTilesPerSplit = 0;
            uint numSplits = 0;

            try
            {
                microTilesPerSplit = splitSize / microTileBits;
                numSplits = Math.Max(1, NumSamples / microTilesPerSplit);
            }
            catch (DivideByZeroException)
            {
                numSplits = 1;
            }

            if (IsThickMacroTiled(TileMode))
            {
                NumSamples = 4;
            }

            uint bytesPerSplit = NumSamples * microTileBits / numSplits;
            uint macroTileAspectRatio = ComputeMacroTileAspectRatio(TileMode);
            uint minBankSwapWidth = Math.Max(1, (pipeInterleaveBytes >> 1) / BPP) * 8 * bankCount;
            uint tempVar1 = NumSamples * macroTileAspectRatio * pipeCount * BPP / numSplits;
            uint maxBankSwapWidth = pipeCount * bankCount * rowSize / tempVar1;
            uint preferredBankSwapWidth = swapSize * 8 * bankCount / bytesPerSplit;
            uint bankSwapWidth;
            for (bankSwapWidth = Math.Min(maxBankSwapWidth, Math.Max(preferredBankSwapWidth, minBankSwapWidth));
                 bankSwapWidth >= 2 * Pitch;
                 bankSwapWidth >>= 1)
            {
            }

            return bankSwapWidth;
        }

        public static uint IsBankSwappedTileMode(uint TileMode)
        {
            uint result = 0;
            if (TileMode == 8 || TileMode == 9 || TileMode == 10 || TileMode == 11 || TileMode == 14 || TileMode == 15)
            {
                result = 1;
            }
            return result;
        }

        public static bool IsThickMacroTiled(uint TileMode)
        {
            if (TileMode == 7 || TileMode == 11 || TileMode == 13 || TileMode == 15)
            {
                return true;
            }
            return false;
        }

        public static uint ComputeMacroTileAspectRatio(uint TileMode)
        {
            uint result = 1;
            switch (TileMode)
            {
                case 8:
                case 12:
                case 14:
                    result = 1;
                    break;
                case 5:
                case 9:
                    result = 2;
                    break;
                case 6:
                case 10:
                    result = 4;
                    break;
            }
            return result;
        }

        public uint GetBitsPerPixel(uint format)
        {
            uint elementHeight = ElementHeight_Default;
            uint elementMode = ElementMode_Default;
            uint bitsPerPixel = 0;
            uint elementWidth = ElementWidth_Default;

            switch (format)
            {
                case 1:
                    bitsPerPixel = 8;
                    break;
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                    bitsPerPixel = 16;
                    break;
                case 39:
                    elementMode = ElementMode_Compressed;
                    bitsPerPixel = 16;
                    break;
                case 40:
                    elementMode = ElementMode_BitPacked;
                    bitsPerPixel = 1;
                    break;
                case 0:
                    bitsPerPixel = 0;
                    break;
                case 2:
                case 3:
                    bitsPerPixel = 8;
                    break;
                case 12:
                    bitsPerPixel = 16;
                    break;
                case 13:
                case 14:
                case 15:
                case 16:
                case 19:
                case 20:
                case 21:
                case 23:
                case 25:
                case 26:
                case 17:
                case 18:
                case 22:
                case 24:
                case 27:
                case 41:
                case 42:
                case 43:
                    bitsPerPixel = 32;
                    break;
                case 29:
                case 30:
                case 31:
                case 32:
                case 62:
                case 28:
                    bitsPerPixel = 64;
                    break;
                case 34:
                case 35:
                    bitsPerPixel = 128;
                    break;
                case 44:
                    elementMode = ElementMode_PackedRGB;
                    bitsPerPixel = 24;
                    elementWidth = ElementWidth_PackedRGB;
                    break;
                case 45:
                case 46:
                    elementMode = ElementMode_PackedRGB;
                    bitsPerPixel = 48;
                    elementWidth = ElementWidth_PackedRGB;
                    break;
                case 47:
                case 48:
                    elementMode = ElementMode_PackedRGB;
                    bitsPerPixel = 96;
                    elementWidth = ElementWidth_PackedRGB;
                    break;
                case 49:
                    elementMode = ElementMode_ThickMacroTile;
                    elementHeight = ElementHeight_ThickTile;
                    bitsPerPixel = 64;
                    elementWidth = ElementWidth_ThickTile;
                    break;
                case 52:
                    elementMode = ElementMode_ThickMacroTileAlt;
                    elementHeight = ElementHeight_ThickTile;
                    bitsPerPixel = 64;
                    elementWidth = ElementWidth_ThickTile;
                    break;
                case 50:
                    elementMode = ElementMode_ThickMacroTile128;
                    elementHeight = ElementHeight_ThickTile;
                    bitsPerPixel = 128;
                    elementWidth = ElementWidth_ThickTile;
                    break;
                case 51:
                    elementMode = ElementMode_ThickMacroTile128Alt;
                    elementHeight = ElementHeight_ThickTile;
                    bitsPerPixel = 128;
                    elementWidth = ElementWidth_ThickTile;
                    break;
                case 53:
                case 54:
                case 55:
                    elementMode = ElementMode_ThickMacroTileRGBA;
                    elementHeight = ElementHeight_ThickTile;
                    bitsPerPixel = 128;
                    elementWidth = ElementWidth_ThickTile;
                    break;
                default:
                    bitsPerPixel = 0;
                    break;
            }
            ExportandY = elementHeight;
            ExportandX = elementWidth;
            ElemMode = elementMode;
            return bitsPerPixel;
        }

        public uint AdjustSurfaceInfo(uint elementMode, uint elementWidth, uint elementHeight, uint originalBPP, uint surfaceWidth, uint surfaceHeight)
        {
            adjustedBPP = originalBPP;
            if (originalBPP != 0)
            {
                switch (elementMode)
                {
                    case ElemMode_PackedRGB:
                        adjustedBPP = originalBPP / elementWidth / elementHeight;
                        break;
                    case ElemMode_DepthStencil:
                    case ElemMode_Palette:
                        adjustedBPP = elementHeight * elementWidth * originalBPP;
                        break;
                    case ElemMode_Compressed:
                    case ElemMode_BitPacked:
                        adjustedBPP = originalBPP;
                        break;
                    case ElemMode_ThickMacroTile:
                    case ElemMode_ThickMacroTileAlt:
                        adjustedBPP = 64;
                        break;
                    case ElemMode_ThickMacroTile128:
                    case ElemMode_ThickMacroTile128Alt:
                    case ElemMode_ThickMacroTileRGBA:
                        adjustedBPP = 128;
                        break;
                    default:
                        adjustedBPP = originalBPP;
                        break;
                }

                if (AIn != null)
                    AIn.BPP = adjustedBPP;
            }

            if (surfaceWidth != 0 && surfaceHeight != 0)
            {
                if (elementWidth > 1 || elementHeight > 1)
                {
                    uint adjustedWidth = 0;
                    uint adjustedHeight = 0;

                    if (elementMode == ElemMode_PackedRGB)
                    {
                        adjustedWidth = elementWidth * surfaceWidth;
                        adjustedHeight = elementHeight * surfaceHeight;
                    }
                    else if (elementMode >= ElemMode_ThickMacroTile && elementMode <= ElemMode_ThickMacroTileRGBA)
                    {
                        adjustedWidth = surfaceWidth / elementWidth;
                        adjustedHeight = surfaceHeight / elementHeight;
                    }
                    else
                    {
                        adjustedWidth = (surfaceWidth + elementWidth - 1) / elementWidth;
                        adjustedHeight = (surfaceHeight + elementHeight - 1) / elementHeight;
                    }

                    if (AIn != null)
                    {
                        AIn.Width = adjustedWidth == 0 ? 1 : adjustedWidth;
                        AIn.Height = adjustedHeight == 0 ? 1 : adjustedHeight;
                    }
                }
            }

            return adjustedBPP;
        }

        public uint HwlComputeMipLevel()
        {
            uint mipLevelHandled = 0;
            if (AIn == null)
                return mipLevelHandled;
            if (AIn.Format >= Format_ThickMacroTile_Start && AIn.Format <= Format_ThickMacroTile_End)
            {
                if (AIn.MipLevel != 0)
                {
                    uint originalWidth = AIn.Width;
                    uint originalHeight = AIn.Height;
                    uint originalSlices = AIn.NrSlices;
                    bool enableMipmapScaling = ((AIn.Flags & Flag_Mipmap_ScaleDimensions) != 0);
                    if (enableMipmapScaling)
                    {
                        uint scaledWidth = originalWidth >> (int)AIn.MipLevel;
                        uint scaledHeight = originalHeight >> (int)AIn.MipLevel;
                        bool preserveSlices = ((AIn.Flags & Flag_Mipmap_PreserveDepth) != 0);
                        if (!preserveSlices)
                        {
                            originalSlices = originalSlices >> (int)AIn.MipLevel;
                        }
                        originalWidth = Math.Max(1, scaledWidth);
                        originalHeight = Math.Max(1, scaledHeight);
                        originalSlices = Math.Max(1, originalSlices);
                    }
                    AIn.Width = NextPow2(originalWidth);
                    AIn.Height = NextPow2(originalHeight);
                    AIn.NrSlices = originalSlices;
                }
                mipLevelHandled = 1;
            }

            return mipLevelHandled;
        }


        public static uint NextPow2(uint Dimension)
        {
            if (Dimension <= 1)
                return 1;

            if (Dimension > int.MaxValue)
                return (uint)1 << 31;

            uint nextPower = 1;
            while (nextPower < Dimension)
                nextPower <<= 1;

            return nextPower;
        }

        public void ComputeMipLevel()
        {
            if (AIn == null) return;
            uint mipScaledWidth = 0;
            uint mipScaledHeight = 0;
            uint mipScaledSlices = 0;
            bool isThickMacroTileFormat = (AIn.Format >= Format_ThickMacroTile_Start && AIn.Format <= Format_ThickMacroTile_End);
            bool skipMipScaling = (AIn.MipLevel == 0) || ((AIn.Flags & Flag_Mipmap_ScaleDimensions) != 0);
            if (isThickMacroTileFormat && skipMipScaling)
            {
                AIn.Width = PowTwoAlign(AIn.Width, Align_4Bytes);
                AIn.Height = PowTwoAlign(AIn.Height, Align_4Bytes);
            }
            bool needManualMipCalculation = (HwlComputeMipLevel() == 0) && (AIn.MipLevel != 0) && ((AIn.Flags & Flag_Mipmap_ScaleDimensions) != 0);
            if (needManualMipCalculation)
            {
                mipScaledWidth = AIn.Width >> (int)AIn.MipLevel;
                mipScaledHeight = AIn.Height >> (int)AIn.MipLevel;
                mipScaledSlices = AIn.NrSlices;
                if (((AIn.Flags & Flag_Mipmap_PreserveDepth) == 0))
                {
                    mipScaledSlices = AIn.NrSlices >> (int)AIn.MipLevel;
                }
                mipScaledWidth = Math.Max(1, mipScaledWidth);
                mipScaledHeight = Math.Max(1, mipScaledHeight);
                mipScaledSlices = Math.Max(1, mipScaledSlices);
                bool is96BppPackedRGB = (AIn.Format >= Format_PackedRGB_96BPP_Start && AIn.Format <= Format_PackedRGB_96BPP_End);
                if (!is96BppPackedRGB)
                {
                    mipScaledWidth = NextPow2(mipScaledWidth);
                    mipScaledHeight = NextPow2(mipScaledHeight);
                    mipScaledSlices = NextPow2(mipScaledSlices);
                }
                AIn.Width = mipScaledWidth;
                AIn.Height = mipScaledHeight;
                AIn.NrSlices = mipScaledSlices;
            }
        }

        public uint PowTwoAlign(uint value, uint alignment)
        {
            return ~(alignment - 1) & (value + alignment - 1);
        }

        public uint ConvertToNonBankSwappedMode(uint TileMode)
        {
            switch (TileMode)
            {
                case 8:
                    return 4;
                case 9:
                    return 5;
                case 10:
                    return 6;
                case 11:
                    return 7;
                case 14:
                    return 12;
                case 15:
                    return 13;
                default:
                    return TileMode;
            }
        }

        public uint ComputeSurfaceTileSlices(uint TileMode, uint BPP, uint NrSamples)
        {
            uint microTileTotalBits = BPP * MicroTilePixels;
            uint microTileBytes = (microTileTotalBits + 7) / 8;
            uint tileSlices = 1;
            if (IsThickMacroTiled(TileMode))
            {
                NrSamples = 4;
            }

            if (microTileBytes != 0)
            {
                uint rowMicroTileCapacity = (uint)m_RowSize / microTileBytes;
                if (rowMicroTileCapacity != 0)
                {
                    tileSlices = Math.Max(1, NrSamples / rowMicroTileCapacity);
                }
            }
            return tileSlices;
        }

        public uint ComputeSurfaceRotationFromTileMode(uint TileMode)
        {
            if (TileMode >= TileMode_MacroTiled_RangeStart && TileMode <= TileMode_MacroTiled_RangeEnd)
            {
                rotationFactor = MacroTileRotationFactor;
            }
            else if (TileMode >= TileMode_AltMacroTiled_RangeStart && TileMode <= TileMode_AltMacroTiled_RangeEnd)
            {
                rotationFactor = AltMacroTileRotationFactor;
            }
            return rotationFactor;
        }

        public uint ComputeSurfaceMipLevelTileMode(uint baseTileMode, uint bpp, uint level, uint width, uint height, uint nrSlices, uint nrSamples, uint isDepth, uint noRecursive)
        {
            uint currentTileMode = baseTileMode;
            uint pipeInterleaveBytes = (uint)m_PipeInterleaveBytes;
            uint tileSlices = ComputeSurfaceTileSlices(baseTileMode, bpp, nrSamples);

            if (baseTileMode == 7 && (nrSamples > 1 || tileSlices > 1 || isDepth != 0))
            {
                currentTileMode = 4;
            }
            else if (baseTileMode == 13 && (nrSamples > 1 || tileSlices > 1 || isDepth != 0))
            {
                currentTileMode = 12;
            }
            else if (baseTileMode == 11 && (nrSamples > 1 || tileSlices > 1 || isDepth != 0))
            {
                currentTileMode = 8;
            }
            else if (baseTileMode == 15 && (nrSamples > 1 || tileSlices > 1 || isDepth != 0))
            {
                currentTileMode = 14;
            }
            else if (baseTileMode == 2 && nrSamples > 1 && ((m_ConfigFlags >> 2) & 1) != 0)
            {
                currentTileMode = 4;
            }
            else if (baseTileMode == 3)
            {
                if (nrSamples > 1 || isDepth != 0)
                {
                    currentTileMode = 2;
                }
                if (nrSamples == 2 || nrSamples == 4)
                {
                    currentTileMode = 7;
                }
            }
            uint rotationFactor = ComputeSurfaceRotationFromTileMode(currentTileMode);
            if (rotationFactor % 2 == 0)
            {
                switch (currentTileMode)
                {
                    case 12:
                        currentTileMode = 4;
                        break;
                    case 14:
                        currentTileMode = 8;
                        break;
                    case 13:
                        currentTileMode = 7;
                        break;
                    case 15:
                        currentTileMode = 11;
                        break;
                    default:
                        break;
                }
            }

            if (noRecursive != 0)
            {
                return currentTileMode;
            }

            uint adjustedBpp = bpp;
            if (bpp == 24 || bpp == 48 || bpp == 96)
            {
                adjustedBpp /= 3;
            }
            uint alignedWidth = NextPow2(width);
            uint alignedHeight = NextPow2(height);
            uint alignedSlices = NextPow2(nrSlices);
            if (level != 0)
            {
                currentTileMode = ConvertToNonBankSwappedMode(currentTileMode);
                uint microTileThickness = ComputeSurfaceThickness(currentTileMode);
                uint microTileTotalBits = adjustedBpp * (microTileThickness * MicroTilePixels);
                uint microTileTotalBytes = (microTileTotalBits + 7) / 8;
                uint rowMicroTileCapacity = microTileTotalBytes >= pipeInterleaveBytes ? 1 : pipeInterleaveBytes / microTileTotalBytes;
                uint currentMacroTileWidth = MacroTileWidth_Base;
                uint currentMacroTileHeight = MacroTileHeight_Base;

                switch (currentTileMode)
                {
                    case 4:
                    case 12:
                        if (alignedWidth < rowMicroTileCapacity * currentMacroTileWidth ||
                            alignedHeight < currentMacroTileHeight)
                        {
                            currentTileMode = 2;
                        }
                        break;

                    case 5:
                        currentMacroTileWidth >>= 1;
                        currentMacroTileHeight *= 2;
                        if (alignedWidth < rowMicroTileCapacity * currentMacroTileWidth ||
                            alignedHeight < currentMacroTileHeight)
                        {
                            currentTileMode = 2;
                        }
                        break;

                    case 6:
                        currentMacroTileWidth >>= 2;
                        currentMacroTileHeight *= 4;
                        if (alignedWidth < rowMicroTileCapacity * currentMacroTileWidth ||
                            alignedHeight < currentMacroTileHeight)
                        {
                            currentTileMode = 4;
                        }
                        break;

                    case 7:
                    case 13:
                        if (alignedWidth < rowMicroTileCapacity * currentMacroTileWidth ||
                            alignedHeight < currentMacroTileHeight)
                        {
                            currentTileMode = 3;
                        }
                        break;
                }

                if (currentTileMode == 3 && alignedSlices < 4)
                {
                    currentTileMode = 2;
                }
                else if (currentTileMode == 7 && nrSlices < 4)
                {
                    currentTileMode = 4;
                }
                else if (currentTileMode == 13 && alignedSlices < 4)
                {
                    currentTileMode = 12;
                }

                currentTileMode = ComputeSurfaceMipLevelTileMode(
                    currentTileMode, adjustedBpp, level,
                    alignedWidth, alignedHeight, alignedSlices,
                    nrSamples, isDepth, 1);
            }

            return currentTileMode;
        }

        public bool IsDualPitchAlignNeeded(uint TileMode, uint IsDepth, uint MipLevel)
        {
            if (IsDepth == 0)
            {
                return false;
            }
            return false;
        }

        public bool IsPow2(uint Dim)
        {
            return (Dim & (Dim - 1)) == 0;
        }

        public void PadDimensions(uint tileMode, uint padDims, uint isCube, uint cubeAsArray, uint pitchAlign, uint heightAlign, uint sliceAlign)
        {
            uint microTileThickness = ComputeSurfaceThickness(tileMode);
            uint actualPadDims = padDims == 0 ? 3 : padDims;
            if (IsPow2(pitchAlign))
            {
                ExportPitch = PowTwoAlign(ExportPitch, pitchAlign);
            }
            else
            {
                ExportPitch = (ExportPitch + pitchAlign - 1) / pitchAlign * pitchAlign;
            }

            if (actualPadDims > 1)
            {
                ExportHeight = PowTwoAlign(ExportHeight, heightAlign);
            }

            if (actualPadDims > 2 || microTileThickness > 1)
            {
                bool isCubeNonArray = (isCube != 0 && ((m_ConfigFlags >> 3) & 1) == 0);
                if (isCubeNonArray || cubeAsArray != 0)
                {
                    ExportNrSlices = NextPow2(ExportNrSlices);
                }

                if (microTileThickness > 1)
                {
                    ExportNrSlices = PowTwoAlign(ExportNrSlices, sliceAlign);
                }
            }
        }

        public uint AdjustPitchAlignment(uint Flags, uint PitchAlign)
        {
            if (((Flags >> 13) & 1) != 0)
            {
                PitchAlign = PowTwoAlign(PitchAlign, 32);
            }
            return PitchAlign;
        }

        public void ComputeSurfaceAlignmentsLinear(uint tileMode, uint bpp, uint flags)
        {
            switch (tileMode)
            {
                case 0:
                    BaseAlignment = 1;
                    PitchAlignment = (bpp != 1) ? (uint)1 : 8;
                    HeightAlignment = 1;
                    break;
                case 1:
                    uint rowSizeDivBpp = (bpp == 0) ? 64 : (uint)m_RowSize / bpp;
                    BaseAlignment = 256;
                    PitchAlignment = Math.Max(64, rowSizeDivBpp);
                    HeightAlignment = 1;
                    break;
                default:
                    BaseAlignment = 1;
                    PitchAlignment = 1;
                    HeightAlignment = 1;
                    break;
            }
            PitchAlignment = AdjustPitchAlignment(flags, PitchAlignment);
        }

        public void ComputeSurfaceInfoLinear(uint tileMode, uint bpp, uint nrSamples, uint pitch, uint height, uint nrSlices, uint mipLevel, uint padDims, uint flags, out uint valid, out uint pPitchOut, out uint pHeightOut, out uint pNrSlicesOut, out uint pSurfSize, out uint pBaseAlign, out uint pPitchAlign, out uint pHeightAlign, out uint pDepthAlign)
        {
            valid = 1;
            ExportPitch = pitch;
            ExportHeight = height;
            ExportNrSlices = nrSlices;
            uint microTileThickness = ComputeSurfaceThickness(tileMode);
            ComputeSurfaceAlignmentsLinear(tileMode, bpp, flags);
            if (((flags >> 9) & 1) != 0 && mipLevel == 0)
            {
                ExportPitch /= 3;
                ExportPitch = NextPow2(ExportPitch);
            }

            if (mipLevel != 0)
            {
                ExportPitch = NextPow2(ExportPitch);
                ExportHeight = NextPow2(ExportHeight);

                if (((flags >> 4) & 1) != 0)
                {
                    ExportNrSlices = nrSlices;
                    padDims = (uint)((nrSlices <= 1) ? 2 : 0);
                }
                else
                {
                    ExportNrSlices = NextPow2(nrSlices);
                }
            }

            PadDimensions(tileMode, padDims, (flags >> 4) & 1, (flags >> 7) & 1, PitchAlignment, HeightAlignment, microTileThickness);

            if (((flags >> 9) & 1) != 0 && mipLevel == 0)
            {
                ExportPitch *= 3;
            }

            uint sliceCountAdjusted = (ExportNrSlices * nrSamples) / microTileThickness;
            pPitchOut = ExportPitch;
            pHeightOut = ExportHeight;
            pNrSlicesOut = ExportNrSlices;

            if (bpp == 0 || nrSamples == 0)
            {
                pSurfSize = 0;
            }
            else
            {
                pSurfSize = (ExportHeight * ExportPitch * sliceCountAdjusted * bpp * nrSamples + 7) / 8;
            }

            pBaseAlign = BaseAlignment;
            pPitchAlign = PitchAlignment;
            pHeightAlign = HeightAlignment;
            pDepthAlign = microTileThickness;
        }

        public void ComputeSurfaceAlignmentsMicroTiled(uint tileMode, uint bpp, uint flags, uint sampleCount)
        {
            uint adjustedBpp = bpp;
            if (bpp == 24 || bpp == 48 || bpp == 96)
            {
                adjustedBpp /= 3;
            }

            uint microTileThickness = ComputeSurfaceThickness(tileMode);
            BaseAlignment = 256;
            PitchAlignment = Math.Max(8, 256 / adjustedBpp / sampleCount / microTileThickness);
            HeightAlignment = 8;
            PitchAlignment = AdjustPitchAlignment(flags, PitchAlignment);
        }

        public void ComputeSurfaceInfoMicroTiled(uint inputTileMode, uint bpp, uint sampleCount, uint inputPitch, uint inputHeight, uint inputSliceCount, uint mipLevel, uint padDimensions, uint flags, out uint isValid, out uint outputPitch, out uint outputHeight, out uint outputSliceCount, out uint surfaceSize, out uint outputTileMode, out uint baseAlignment, out uint pitchAlignment, out uint heightAlignment, out uint depthAlignment)
        {
            isValid = 1;
            uint currentTileMode = inputTileMode;
            ExportPitch = inputPitch;
            ExportHeight = inputHeight;
            ExportNrSlices = inputSliceCount;
            uint microTileThickness = ComputeSurfaceThickness(inputTileMode);
            if (mipLevel != 0)
            {
                ExportPitch = NextPow2(inputPitch);
                ExportHeight = NextPow2(inputHeight);
                if (((flags >> 4) & 1) != 0)
                {
                    ExportNrSlices = inputSliceCount;
                    padDimensions = (uint)((inputSliceCount <= 1) ? 2 : 0);
                }
                else
                {
                    ExportNrSlices = NextPow2(inputSliceCount);
                }

                if (currentTileMode == 3 && ExportNrSlices < 4)
                {
                    currentTileMode = 2;
                    microTileThickness = 1;
                }
            }

            ComputeSurfaceAlignmentsMicroTiled(currentTileMode, bpp, flags, sampleCount);
            PadDimensions(currentTileMode, padDimensions, (flags >> 4) & 1, (flags >> 7) & 1, PitchAlignment, HeightAlignment, microTileThickness);
            outputPitch = ExportPitch;
            outputHeight = ExportHeight;
            outputSliceCount = ExportNrSlices;
            surfaceSize = (ExportHeight * ExportPitch * ExportNrSlices * bpp * sampleCount + 7) / 8;
            outputTileMode = currentTileMode;
            baseAlignment = BaseAlignment;
            pitchAlignment = PitchAlignment;
            heightAlignment = HeightAlignment;
            depthAlignment = microTileThickness;
        }

        public bool IsDualBaseAlignNeeded(uint TileMode)
        {
            return false;
        }

        public void ComputeSurfaceAlignmentsMacroTiled(uint tileMode, uint bpp, uint flags, uint sampleCount, out uint baseAlignment, out uint pitchAlignment, out uint heightAlignment, out uint macroTileWidth, out uint macroTileHeight)
        {
            uint pipeInterleaveBytes = (uint)m_PipeInterleaveBytes;
            uint rowSize = (uint)m_RowSize;
            uint MicroTilePixelSize = MicroTilePixelSize_Base;
            uint macroTileAspectRatio = ComputeMacroTileAspectRatio(tileMode);
            uint microTileThickness = ComputeSurfaceThickness(tileMode);
            uint adjustedBpp = bpp;
            if (bpp == 24 || bpp == 48 || bpp == 96)
            {
                adjustedBpp /= 3;
            }
            if (adjustedBpp == 3)
            {
                adjustedBpp = 1;
            }
            macroTileWidth = MacroTileWidth_Base / macroTileAspectRatio;
            macroTileHeight = MacroTileHeight_Base * macroTileAspectRatio;
            uint pitchAlignBySample = macroTileWidth * (pipeInterleaveBytes / adjustedBpp / (MicroTilePixelSize * microTileThickness) / sampleCount);
            pitchAlignment = Math.Max(macroTileWidth, pitchAlignBySample);
            pitchAlignment = AdjustPitchAlignment(flags, pitchAlignment);
            heightAlignment = macroTileHeight;
            uint singleMacroTileBytes = (adjustedBpp * macroTileHeight * macroTileWidth + 7) >> 3;
            uint macroTileTotalBytes = sampleCount * singleMacroTileBytes;

            if (microTileThickness == 1)
            {
                uint sampleRowBytes = (sampleCount * heightAlignment * adjustedBpp * pitchAlignment + 7) >> 3;
                baseAlignment = Math.Max(macroTileTotalBytes, sampleRowBytes);
            }
            else
            {
                uint multiLayerRowBytes = (4 * heightAlignment * adjustedBpp * pitchAlignment + 7) >> 3;
                baseAlignment = Math.Max(pipeInterleaveBytes, multiLayerRowBytes);
            }

            uint microTileTotalBytes = (adjustedBpp * MicroTilePixels * microTileThickness + 7) >> 3;
            uint sampleSliceCount = (microTileTotalBytes < rowSize) ? 1 : (microTileTotalBytes / rowSize);
            sampleSliceCount = Math.Max(sampleSliceCount, 1);
            baseAlignment /= sampleSliceCount;

            if (IsDualBaseAlignNeeded(tileMode))
            {
                if (baseAlignment / singleMacroTileBytes % 2 != 0)
                {
                    baseAlignment += singleMacroTileBytes;
                }
            }
        }

        public void ComputeSurfaceInfoMacroTiled(uint tileMode, uint baseTileMode, uint bpp, uint sampleCount, uint inputPitch, uint inputHeight, uint inputSliceCount, uint mipLevel, uint padDimensions, uint flags, out uint result, out uint outputPitch, out uint outputHeight, out uint outputSliceCount, out uint surfaceSize, out uint outputTileMode, out uint baseAlignment, out uint pitchAlignment, out uint heightAlignment, out uint depthAlignment)
        {
            uint valid = 1;
            result = 0;
            ExportPitch = inputPitch;
            ExportHeight = inputHeight;
            ExportNrSlices = inputSliceCount;
            uint currentTileMode = tileMode;
            uint microTileThickness = ComputeSurfaceThickness(tileMode);

            if (mipLevel != 0)
            {
                ExportPitch = NextPow2(inputPitch);
                ExportHeight = NextPow2(inputHeight);

                if (((flags >> 4) & 1) != 0)
                {
                    ExportNrSlices = inputSliceCount;
                    padDimensions = (uint)((inputSliceCount <= 1) ? 2 : 0);
                }
                else
                {
                    ExportNrSlices = NextPow2(inputSliceCount);
                    microTileThickness = 1;
                }

                if (tileMode == 7 && ExportNrSlices < 4)
                {
                    currentTileMode = 4;
                }
            }

            if (tileMode == baseTileMode || mipLevel == 0 || IsThickMacroTiled(baseTileMode) || IsThickMacroTiled(tileMode))
            {
                ComputeSurfaceAlignmentsMacroTiled(
                    tileMode, bpp, flags, sampleCount,
                    out BaseAlignment, out PitchAlignment, out HeightAlignment,
                    out uint macroTileWidth, out uint macroTileHeight);

                uint bankSwapWidth = ComputeSurfaceBankSwappedWidth(tileMode, bpp, inputPitch, sampleCount);
                if (bankSwapWidth > PitchAlignment)
                {
                    PitchAlignment = bankSwapWidth;
                }

                if (IsDualPitchAlignNeeded(tileMode, (flags >> 1) & 1, mipLevel))
                {
                    uint tileWidthDivBranch1 = MacroTileWidth_Base / (bpp == 0 ? 1 : bpp) / (sampleCount == 0 ? 1 : sampleCount) / microTileThickness;
                    tileWidthDivBranch1 = Math.Max(tileWidthDivBranch1, 1);

                    uint heightTileIdx = ((ExportHeight - 1) / macroTileHeight) & 1;
                    uint pitchTileIdx = ((ExportPitch - 1) / macroTileWidth) & 1;

                    if (sampleCount == 1 && tileWidthDivBranch1 == 1 && pitchTileIdx == 0 &&
                        (ExportPitch > macroTileWidth || (heightTileIdx == 0 && ExportHeight > macroTileHeight)))
                    {
                        ExportPitch += macroTileWidth;
                    }
                }
                PadDimensions(tileMode, padDimensions, (flags >> 4) & 1, (flags >> 7) & 1, PitchAlignment, HeightAlignment, microTileThickness);
                outputPitch = ExportPitch;
                outputHeight = ExportHeight;
                outputSliceCount = ExportNrSlices;
                surfaceSize = (ExportHeight * ExportPitch * ExportNrSlices * bpp * sampleCount + 7) / 8;
                outputTileMode = currentTileMode;
                baseAlignment = BaseAlignment;
                pitchAlignment = PitchAlignment;
                heightAlignment = HeightAlignment;
                depthAlignment = microTileThickness;
                result = valid;
                return;
            }

            ComputeSurfaceAlignmentsMacroTiled(baseTileMode, bpp, flags, sampleCount, out uint baseAlignTemp, out uint pitchAlignTemp, out uint heightAlignTemp, out uint macroTileWidthBase, out uint macroTileHeightBase);
            uint tileWidthDivBranch2 = MacroTileWidth_Base / (bpp == 0 ? 1 : bpp);
            tileWidthDivBranch2 = Math.Max(tileWidthDivBranch2, 1);

            if (ExportPitch < pitchAlignTemp * tileWidthDivBranch2 || ExportHeight < heightAlignTemp)
            {
                currentTileMode = 2;
                ComputeSurfaceInfoMicroTiled(2, bpp, sampleCount, inputPitch, inputHeight, inputSliceCount, mipLevel, padDimensions, flags, out valid, out outputPitch, out outputHeight, out outputSliceCount, out surfaceSize, out outputTileMode, out baseAlignment, out pitchAlignment, out heightAlignment, out depthAlignment);
                return;
            }

            ComputeSurfaceAlignmentsMacroTiled(
                tileMode, bpp, flags, sampleCount,
                out baseAlignTemp, out pitchAlignTemp, out heightAlignTemp,
                out uint macroTileWidthCurrent, out uint macroTileHeightCurrent);

            uint bankSwapWidthCurrent = ComputeSurfaceBankSwappedWidth(tileMode, bpp, inputPitch, sampleCount);
            if (bankSwapWidthCurrent > pitchAlignTemp)
            {
                pitchAlignTemp = bankSwapWidthCurrent;
            }

            if (IsDualPitchAlignNeeded(tileMode, (flags >> 1) & 1, mipLevel))
            {
                uint tileWidthDivBranch3 = MacroTileWidth_Base / (bpp == 0 ? 1 : bpp) / (sampleCount == 0 ? 1 : sampleCount) / microTileThickness;
                tileWidthDivBranch3 = Math.Max(tileWidthDivBranch3, 1);

                uint heightTileIdxCurrent = ((ExportHeight - 1) / macroTileHeightCurrent) & 1;
                uint pitchTileIdxCurrent = ((ExportPitch - 1) / macroTileWidthCurrent) & 1;

                if (sampleCount == 1 && tileWidthDivBranch3 == 1 && pitchTileIdxCurrent == 0 &&
                    (ExportPitch > macroTileWidthCurrent || (heightTileIdxCurrent == 0 && ExportHeight > macroTileHeightCurrent)))
                {
                    ExportPitch += macroTileWidthCurrent;
                }
            }
            PadDimensions(tileMode, padDimensions, (flags >> 4) & 1, (flags >> 7) & 1, pitchAlignTemp, heightAlignTemp, microTileThickness);
            outputPitch = ExportPitch;
            outputHeight = ExportHeight;
            outputSliceCount = ExportNrSlices;
            surfaceSize = (ExportHeight * ExportPitch * ExportNrSlices * bpp * sampleCount + 7) / 8;
            outputTileMode = currentTileMode;
            baseAlignment = baseAlignTemp;
            pitchAlignment = pitchAlignTemp;
            heightAlignment = heightAlignTemp;
            depthAlignment = microTileThickness;
            result = valid;
        }

        public uint ComputeSurfaceInfoEx()
        {
            if (AIn == null || AOut == null)
                return 3;
            uint tileMode = AIn.TileMode;
            uint bPP = AIn.BPP == 0 ? GetBitsPerPixel(AIn.Format) : AIn.BPP;
            uint nrSamples = AIn.NrSamples == 0 ? 1 : AIn.NrSamples;
            uint width = AIn.Width;
            uint height = AIn.Height;
            uint nrSlices = AIn.NrSlices;
            uint mipLevel = AIn.MipLevel;
            uint flags = AIn.Flags;
            uint baseTileMode = tileMode;
            uint padDims = 0;
            uint pPitchOut = 0;
            uint pHeightOut = 0;
            uint pNrSlicesOut = 0;
            uint pSurfSize = 0;
            uint pTileModeOut = tileMode;
            uint pBaseAlign = 0;
            uint pPitchAlign = 0;
            uint pHeightAlign = 0;
            uint pDepthAlign = 0;
            uint Result = 0;

            if (((flags >> 4) & 1) != 0 && mipLevel == 0)
            {
                padDims = 2;
            }

            if (((flags >> 6) & 1) == 0)
            {
                uint isDepth = (flags >> 1) & 1;
                tileMode = ComputeSurfaceMipLevelTileMode(
                    tileMode, bPP, mipLevel, width, height, nrSlices,
                    nrSamples, isDepth, 0);
            }
            else
            {
                tileMode = ConvertToNonBankSwappedMode(tileMode);
            }

            switch (tileMode)
            {
                case 0:
                case 1:
                    ComputeSurfaceInfoLinear(
                        tileMode, bPP, nrSamples, width, height, nrSlices,
                        mipLevel, padDims, flags, out Result, out pPitchOut,
                        out pHeightOut, out pNrSlicesOut, out pSurfSize,
                        out pBaseAlign, out pPitchAlign, out pHeightAlign, out pDepthAlign);
                    pTileModeOut = tileMode;
                    break;

                case 2:
                case 3:
                    ComputeSurfaceInfoMicroTiled(
                        tileMode, bPP, nrSamples, width, height, nrSlices,
                        mipLevel, padDims, flags, out Result, out pPitchOut,
                        out pHeightOut, out pNrSlicesOut, out pSurfSize,
                        out pTileModeOut, out pBaseAlign, out pPitchAlign,
                        out pHeightAlign, out pDepthAlign);
                    break;

                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                    ComputeSurfaceInfoMacroTiled(
                        tileMode, baseTileMode, bPP, nrSamples, width, height, nrSlices,
                        mipLevel, padDims, flags, out Result, out pPitchOut,
                        out pHeightOut, out pNrSlicesOut, out pSurfSize,
                        out pTileModeOut, out pBaseAlign, out pPitchAlign,
                        out pHeightAlign, out pDepthAlign);
                    break;

                default:
                    Result = 0;
                    break;
            }

            AOut.Pitch = pPitchOut;
            AOut.Height = pHeightOut;
            AOut.Depth = pNrSlicesOut;
            AOut.TileMode = pTileModeOut;
            AOut.SurfaceSize = pSurfSize;
            AOut.BaseAlignment = pBaseAlign;
            AOut.PitchAlignment = pPitchAlign;
            AOut.HeightAlignment = pHeightAlign;
            AOut.DepthAlignment = pDepthAlign;
            return (uint)(Result == 0 ? 3 : 0);
        }

        public uint RestoreSurfaceInfo(uint elemMode, uint expandX, uint expandY, uint originalBPP)
        {
            if (AOut == null)
                return originalBPP;

            uint restoredBPP = originalBPP;
            if (restoredBPP == 0)
                return 0;
            switch (elemMode)
            {
                case ElemMode_PackedRGB:
                    restoredBPP = expandX * expandY * restoredBPP;
                    break;

                case ElemMode_DepthStencil:
                case ElemMode_Palette:
                    uint safeExpandX = expandX == 0 ? 1 : expandX;
                    uint safeExpandY = expandY == 0 ? 1 : expandY;
                    restoredBPP = restoredBPP / safeExpandX / safeExpandY;
                    break;

                case ElemMode_ThickMacroTile:
                case ElemMode_ThickMacroTileAlt:
                    restoredBPP = 64;
                    break;

                case ElemMode_ThickMacroTile128:
                case ElemMode_ThickMacroTile128Alt:
                case ElemMode_ThickMacroTileRGBA:
                    restoredBPP = 128;
                    break;
                default:
                    break;
            }

            if (AOut.PixelPitch != 0 && AOut.PixelHeight != 0)
            {
                uint pixelPitch = AOut.PixelPitch;
                uint pixelHeight = AOut.PixelHeight;
                uint safeExpandX = expandX == 0 ? 1 : expandX;
                uint safeExpandY = expandY == 0 ? 1 : expandY;

                if (safeExpandX > 1 || safeExpandY > 1)
                {
                    if (elemMode == ElemMode_PackedRGB)
                    {
                        pixelPitch /= safeExpandX;
                        pixelHeight /= safeExpandY;
                    }
                    else if (elemMode >= ElemMode_ThickMacroTile && elemMode <= ElemMode_ThickMacroTileRGBA)
                    {
                        pixelPitch *= safeExpandX;
                        pixelHeight *= safeExpandY;
                    }
                    else
                    {
                        pixelPitch *= safeExpandX;
                        pixelHeight *= safeExpandY;
                    }
                }

                AOut.PixelPitch = pixelPitch == 0 ? 1 : pixelPitch;
                AOut.PixelHeight = pixelHeight == 0 ? 1 : pixelHeight;
            }

            return restoredBPP;
        }

        public void ComputeSurfaceInfo(_ADDR_COMPUTE_SURFACE_ADDRFROMCOORD_INPUT ADDRIn, _ADDR_COMPUTE_SURFACE_ADDRFROMCOORD_OUTPUT ADDROut)
        {
            AIn = ADDRIn ?? throw new ArgumentNullException(nameof(ADDRIn));
            AOut = ADDROut ?? throw new ArgumentNullException(nameof(ADDROut));
            TileInfo tileInfo = new TileInfo();
            uint errorCode = 0;
            uint fillSizeFlags = GetFillSizeFieldsFlags();

            if (fillSizeFlags == 1 && (AIn.Size != 60 || AOut.Size != 96))
            {
                errorCode = 6;
                return;
            }
            if (AIn.BPP > 128)
            {
                errorCode = 3;
                return;
            }
            ComputeMipLevel();
            uint width = AIn.Width;
            uint height = AIn.Height;
            uint bpp = AIn.BPP;
            ExportandX = 1;
            ExportandY = 1;
            uint sliceComputeFlags = GetSliceComputingFlags();
            uint formatValid = 0;

            if (UseTileIndex((int)AIn.TileIndex) && AIn.ATileInfo == null)
            {
                if (AOut.ATileInfo != null)
                {
                    AIn.ATileInfo = AOut.ATileInfo;
                }
                else
                {
                    AOut.ATileInfo = tileInfo;
                    AIn.ATileInfo = tileInfo;
                }
            }

            if (errorCode == 0)
            {
                AOut.PixelBits = AIn.BPP;
                if (AIn.Format != 0)
                {
                    formatValid = 1;
                    uint format = AIn.Format;
                    bpp = GetBitsPerPixel(format);

                    if (ElemMode == ElemMode_PackedRGB && ExportandX == 3 && AIn.TileMode == 1)
                    {
                        AIn.Flags |= 512;
                    }

                    bpp = AdjustSurfaceInfo(ElemMode, ExportandX, ExportandY, bpp, width, height);
                    AIn.BPP = bpp;
                }
                else if (AIn.BPP != 0)
                {
                    AIn.Width = AIn.Width == 0 ? 1 : AIn.Width;
                    AIn.Height = AIn.Height == 0 ? 1 : AIn.Height;
                }
                else
                {
                    errorCode = 3;
                }
            }

            if (errorCode == 0)
            {
                errorCode = ComputeSurfaceInfoEx();
            }
            if (errorCode != 0)
            {
                return;
            }

            AOut.BPP = AIn.BPP;
            AOut.PixelPitch = AOut.Pitch;
            AOut.PixelHeight = AOut.Height;
            if ((AIn.Format == 0 || ((AIn.Flags >> 9) & 1) != 0) && AIn.MipLevel != 0)
            {
                return;
            }

            if (formatValid == 0)
            {
                throw new InvalidOperationException("Invalid request: Format is required when BPP is derived from format.");
            }

            bpp = RestoreSurfaceInfo(ElemMode, ExportandX, ExportandY, bpp);

            switch (sliceComputeFlags)
            {
                case 1:
                    AOut.SliceSize = (AOut.Height * AOut.Pitch * AOut.BPP * AIn.NrSamples + 7) / 8;
                    break;
                case 0:
                    if (((AIn.Flags >> 5) & 1) != 0)
                    {
                        AOut.SliceSize = AOut.SurfaceSize;
                    }
                    else
                    {
                        AOut.SliceSize = AOut.SurfaceSize / AOut.Depth;
                        if (AIn.Slice == AIn.NrSlices - 1 && AIn.NrSlices > 1)
                        {
                            AOut.SliceSize += AOut.SliceSize * (AOut.Depth - AIn.NrSlices);
                        }
                    }
                    break;
            }

            AOut.PitchTileMax = (AOut.Pitch >> 3) - 1;
            AOut.HeightTileMax = (AOut.Height >> 3) - 1;
            AOut.SliceTileMax = (AOut.Height * AOut.Pitch >> 6) - 1;
        }

        private bool UseTileIndex(int tileIndex)
        {
            return tileIndex != 0;
        }

        public uint GetFillSizeFieldsFlags()
        {
            return (m_ConfigFlags >> 6) & 1;
        }

        public uint GetSliceComputingFlags()
        {
            return (m_ConfigFlags >> 4) & 3;
        }

        public _ADDR_COMPUTE_SURFACE_ADDRFROMCOORD_OUTPUT GetSurfaceInfo(uint SurfaceFormat, uint SurfaceWidth, uint SurfaceHeight, uint SurfaceDepth, uint SurfaceDim, uint SurfaceTileMode, uint SurfaceAA, uint Level)
        {
            uint sampleCount = (uint)(1 << (int)SurfaceAA);
            uint pixelAlignFactor = 1;
            uint alignedWidth = 0;
            uint thickTileFormatMin = 49;
            uint thickTileFormatMax = 53;
            uint formatIndex = SurfaceFormat & 0x3F;

            var surfaceIn = new _ADDR_COMPUTE_SURFACE_ADDRFROMCOORD_INPUT();
            var surfaceOut = new _ADDR_COMPUTE_SURFACE_ADDRFROMCOORD_OUTPUT();

            if (SurfaceTileMode == 16)
            {
                if (formatIndex >= thickTileFormatMin && formatIndex <= thickTileFormatMax)
                {
                    pixelAlignFactor = 4;
                }

                if (formatIndex == 53)
                {
                    throw new NotSupportedException($"Format {formatIndex} (ThickMacroTileRGBA) is not supported for TileMode=16.");
                }

                surfaceOut.BPP = FormatHwInfo[formatIndex * 4];
                surfaceOut.Size = 96;
                uint scaledWidth = SurfaceWidth >> (int)Level;
                alignedWidth = AlignTo(pixelAlignFactor, scaledWidth);
                surfaceOut.Pitch = alignedWidth / pixelAlignFactor;
                surfaceOut.PixelBits = FormatHwInfo[formatIndex * 4];
                surfaceOut.BaseAlignment = 1;
                surfaceOut.PitchAlignment = 1;
                surfaceOut.HeightAlignment = 1;
                surfaceOut.DepthAlignment = 1;

                switch (SurfaceDim)
                {
                    case 0:
                        surfaceOut.Height = 1;
                        surfaceOut.Depth = 1;
                        break;
                    case 1:
                        surfaceOut.Height = Math.Max(1, SurfaceHeight >> (int)Level);
                        surfaceOut.Depth = 1;
                        break;
                    case 2:
                        surfaceOut.Height = Math.Max(1, SurfaceHeight >> (int)Level);
                        surfaceOut.Depth = Math.Max(1, SurfaceDepth >> (int)Level);
                        break;
                    case 3:
                        surfaceOut.Height = Math.Max(1, SurfaceHeight >> (int)Level);
                        surfaceOut.Depth = Math.Max(6, SurfaceDepth);
                        break;
                    case 4:
                        surfaceOut.Height = 1;
                        surfaceOut.Depth = Math.Max(1, SurfaceDepth);
                        break;
                    case 5:
                        surfaceOut.Height = Math.Max(1, SurfaceHeight >> (int)Level);
                        surfaceOut.Depth = Math.Max(1, SurfaceDepth);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(SurfaceDim), $"Unsupported SurfaceDim value: {SurfaceDim}");
                }

                uint scaledHeight = SurfaceHeight >> (int)Level;
                uint alignedHeight = AlignTo(pixelAlignFactor, scaledHeight);
                surfaceOut.Height = alignedHeight / pixelAlignFactor;
                surfaceOut.PixelPitch = Math.Max(pixelAlignFactor, AlignTo(pixelAlignFactor, scaledWidth));
                surfaceOut.PixelHeight = Math.Max(pixelAlignFactor, AlignTo(pixelAlignFactor, scaledHeight));
                surfaceOut.Pitch = Math.Max(1, surfaceOut.Pitch);
                surfaceOut.Height = Math.Max(1, surfaceOut.Height);
                long surfaceSize = (long)surfaceOut.BPP * sampleCount * surfaceOut.Depth * surfaceOut.Height * surfaceOut.Pitch;
                surfaceOut.SurfaceSize = (uint)(surfaceSize >> 3);
                if (surfaceOut.Depth == 0)
                {
                    surfaceOut.SliceSize = 0;
                }
                else
                {
                    surfaceOut.SliceSize = SurfaceDim == 2
                        ? surfaceOut.SurfaceSize
                        : surfaceOut.SurfaceSize / surfaceOut.Depth;
                }
                surfaceOut.PitchTileMax = (surfaceOut.Pitch >> 3) - 1;
                surfaceOut.HeightTileMax = (surfaceOut.Height >> 3) - 1;
                surfaceOut.SliceTileMax = (surfaceOut.Height * surfaceOut.Pitch >> 6) - 1;
            }
            else
            {
                surfaceIn.Size = 60;
                surfaceIn.TileMode = SurfaceTileMode & 0xF;
                surfaceIn.Format = formatIndex;
                if (formatIndex * 4 >= FormatHwInfo.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(SurfaceFormat), $"Format index {formatIndex} is out of range for FormatHwInfo.");
                }
                surfaceIn.BPP = FormatHwInfo[formatIndex * 4];
                surfaceIn.NrSamples = sampleCount;
                surfaceIn.NrFrags = surfaceIn.NrSamples;
                surfaceIn.Width = Math.Max(1, SurfaceWidth >> (int)Level);
                switch (SurfaceDim)
                {
                    case 0:
                        surfaceIn.Height = 1;
                        surfaceIn.NrSlices = 1;
                        break;
                    case 1:
                        surfaceIn.Height = Math.Max(1, SurfaceHeight >> (int)Level);
                        surfaceIn.NrSlices = 1;
                        break;
                    case 2:
                        surfaceIn.Height = Math.Max(1, SurfaceHeight >> (int)Level);
                        surfaceIn.NrSlices = Math.Max(1, SurfaceDepth >> (int)Level);
                        surfaceIn.Flags |= 3;
                        break;
                    case 3:
                        surfaceIn.Height = Math.Max(1, SurfaceHeight >> (int)Level);
                        surfaceIn.NrSlices = Math.Max(6, SurfaceDepth);
                        surfaceIn.Flags |= 16;
                        break;
                    case 4:
                        surfaceIn.Height = 1;
                        surfaceIn.NrSlices = Math.Max(1, SurfaceDepth);
                        break;
                    case 5:
                        surfaceIn.Height = Math.Max(1, SurfaceHeight >> (int)Level);
                        surfaceIn.NrSlices = Math.Max(1, SurfaceDepth);
                        break;
                    case 6:
                        surfaceIn.Height = Math.Max(1, SurfaceHeight >> (int)Level);
                        surfaceIn.NrSlices = 1;
                        break;
                    case 7:
                        surfaceIn.Height = Math.Max(1, SurfaceHeight >> (int)Level);
                        surfaceIn.NrSlices = Math.Max(1, SurfaceDepth);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(SurfaceDim), $"Unsupported SurfaceDim value: {SurfaceDim}");
                }

                surfaceIn.Slice = 0;
                surfaceIn.MipLevel = Level;
                if (Level == 0)
                {
                    surfaceIn.Flags |= Flag_Mipmap_ScaleDimensions;
                }
                surfaceOut.Size = 96;
                ComputeSurfaceInfo(surfaceIn, surfaceOut);
            }

            return surfaceOut;
        }

        private uint AlignTo(uint alignFactor, uint value)
        {
            if (alignFactor == 0)
            {
                return value;
            }
            return (value + alignFactor - 1) & ~(alignFactor - 1);
        }
    }
}