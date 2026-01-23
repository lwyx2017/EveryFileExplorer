using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using LibEveryFileExplorer.GFX;
using LibEveryFileExplorer.IO;
using NDS.NitroSystem.G2D;

namespace NDS.GPU
{
	public class Textures
	{
		public enum ImageFormat : uint
		{
			NONE = 0,
			A3I5 = 1,
			PLTT4 = 2,
			PLTT16 = 3,
			PLTT256 = 4,
			COMP4x4 = 5,
			A5I3 = 6,
			DIRECT = 7
		}

		public enum CharFormat : uint
		{
			CHAR,
			BMP,
            MAX
        }

        public enum OBJVRamModeChar
        {
            OBJVRAMMODE_CHAR_2D = 0,
            OBJVRAMMODE_CHAR_1D_32K = 16,
            OBJVRAMMODE_CHAR_1D_64K = 17,
            OBJVRAMMODE_CHAR_1D_128K = 18,
            OBJVRAMMODE_CHAR_1D_256K = 19
        }

        public enum CharacterDataMapingType
        {
            CHARACTERMAPING_1D_32,
            CHARACTERMAPING_1D_64,
            CHARACTERMAPING_1D_128,
            CHARACTERMAPING_1D_256,
            CHARACTERMAPING_2D,
            CHARACTERMAPING_MAX
        }

        public enum G2DColorMode
        {
            SCREENCOLORMODE_16x16,
            SCREENCOLORMODE_256x1,
            SCREENCOLORMODE_256x16
        }

        public enum ScreenFormat
        {
            SCREENFORMAT_TEXT,
            SCREENFORMAT_AFFINE,
            SCREENFORMAT_AFFINEEXT
        }

        [Flags]
        public enum ParticleFlags : uint
        {
            Type0 = 0,
            Type1 = 0x10,
            Type2 = 0x20,
            Type3 = 0x30,
            Bit8 = 0x100,
            Bit9 = 0x200,
            Bit10 = 0x400,
            TextureAnimation = 0x800,
            Bit16 = 0x10000,
            Bit21 = 0x200000,
            Bit22 = 0x400000,
            Bit23 = 0x800000,
            Bit24 = 0x1000000,
            Bit25 = 0x2000000,
            Bit26 = 0x4000000,
            Bit27 = 0x8000000,
            Bit28 = 0x10000000,
            Bit29 = 0x20000000
        }

        public static Bitmap ToBitmap(byte[] Data, byte[] Palette, int PaletteNr, int Width, int Height, ImageFormat Type, CharFormat CharacterType, bool cut = true, bool firstTransparent = false)
		{
			return ToBitmap(Data, Palette, new byte[0], PaletteNr, Width, Height, Type, CharacterType, firstTransparent, cut);
		}
		public static Bitmap ToBitmap(byte[] Data, byte[] Palette, byte[] Tex4x4, int PaletteNr, int Width, int Height, ImageFormat Type, CharFormat CharacterType, bool firstTransparent = false, bool cut = true)
		{
			Bitmap b = null;
			int offset = 0;
			switch (Type)
			{
				case ImageFormat.NONE:
					break;
				case ImageFormat.A3I5:
					{
						Color[] pal = ConvertXBGR1555(Palette);
						b = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
						BitmapData d = b.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
						int x = 0;
						int y = 0;
						foreach (byte bb in Data)
						{
							Color c = Color.FromArgb((((bb >> 5) << 2) + ((bb >> 5) >> 1)) * 8, pal[bb & 0x1F]);
							Marshal.WriteInt32(d.Scan0, y * d.Stride + x++ * 4, c.ToArgb());
							if (x >= Width)
							{
								y++;
								x = 0;
							}
						}
						b.UnlockBits(d);
						break;
					}
				case ImageFormat.PLTT4:
					{
						Color[] pal = ConvertXBGR1555(Palette);
						if (firstTransparent)
						{
							pal[0] = Color.FromArgb(0, 0xFF, 0, 0);
						}
						if (CharacterType == CharFormat.CHAR)
						{
							b = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);//b = new Bitmap((int)(Data.Length / 16) * 8, 8, PixelFormat.Format32bppArgb);
							BitmapData d = b.LockBits(new Rectangle(0, 0, /*(int)(Data.Length / 32) * 8, 8*/Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
							for (int y = 0; y < Height; y += 8)
							{
								for (int x = 0; x < Width; x += 8)
								{
									for (int TileY = 0; TileY < 8; TileY++)
									{
										for (int TileX = 0; TileX < 2; TileX++)
										{
											byte by = Data[offset];
											int y2 = TileY + y;
											int x2 = TileX * 4 + x + 3;//(i * 8 + TileX * 4) + 3;
											Marshal.WriteInt32(d.Scan0, y2 * d.Stride + x2 * 4, pal[(by & 3) + 4 * PaletteNr].ToArgb());
											x2 = TileX * 4 + x + 2;//(i * 8 + TileX * 4) + 2;
											Marshal.WriteInt32(d.Scan0, y2 * d.Stride + x2 * 4, pal[(by >> 2 & 3) + 4 * PaletteNr].ToArgb());
											x2 = TileX * 4 + x + 1;//(i * 8 + TileX * 4) + 1;
											Marshal.WriteInt32(d.Scan0, y2 * d.Stride + x2 * 4, pal[(by >> 4 & 3) + 4 * PaletteNr].ToArgb());
											x2 = TileX * 4 + x + 0;//i * 8 + TileX * 4;
											Marshal.WriteInt32(d.Scan0, y2 * d.Stride + x2 * 4, pal[(by >> 6 & 3) + 4 * PaletteNr].ToArgb());
											offset++;

										}
									}
								}
							}
							b.UnlockBits(d);
						}
						else
						{
							b = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
							BitmapData d = b.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
							int x = 0;
							int y = 0;
							foreach (byte bb in Data)
							{
								Marshal.WriteInt32(d.Scan0, y * d.Stride + x++ * 4, pal[(bb & 3) + 4 * PaletteNr].ToArgb());
								Marshal.WriteInt32(d.Scan0, y * d.Stride + x++ * 4, pal[(bb >> 2 & 3) + 4 * PaletteNr].ToArgb());
								Marshal.WriteInt32(d.Scan0, y * d.Stride + x++ * 4, pal[(bb >> 4 & 3) + 4 * PaletteNr].ToArgb());
								Marshal.WriteInt32(d.Scan0, y * d.Stride + x++ * 4, pal[(bb >> 6 & 3) + 4 * PaletteNr].ToArgb());
								if (x >= Width)
								{
									y++;
									x = 0;
								}
							}
							b.UnlockBits(d);
						}
						break;
					}
				case ImageFormat.PLTT16:
					{
						Color[] pal = ConvertXBGR1555(Palette);
						if (firstTransparent)
						{
							pal[0] = Color.FromArgb(0, 0xFF, 0, 0);
						}
						if (CharacterType == CharFormat.CHAR)
						{
							b = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
							BitmapData d = b.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
							for (int y = 0; y < Height; y += 8)
							{
								for (int x = 0; x < Width; x += 8)
								{
									for (int TileY = 0; TileY < 8; TileY++)
									{
										for (int TileX = 0; TileX < 4; TileX++)
										{
											byte by = Data[offset];
											int y2 = TileY + y;
											int x2 = TileX * 2 + x + 1; //(i * 8 + TileX * 2) + 1;
											Marshal.WriteInt32(d.Scan0, y2 * d.Stride + x2 * 4, pal[(by / 16) + 16 * PaletteNr].ToArgb());
											x2 = TileX * 2 + x;//i * 8 + TileX * 2;
											Marshal.WriteInt32(d.Scan0, y2 * d.Stride + x2 * 4, pal[(by % 16) + 16 * PaletteNr].ToArgb());
											offset++;
										}
									}
								}
							}
							b.UnlockBits(d);
						}
						else
						{
							b = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
							BitmapData d = b.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
							int x = 0;
							int y = 0;
							foreach (byte bb in Data)
							{
								Marshal.WriteInt32(d.Scan0, y * d.Stride + x++ * 4, pal[(bb % 16) + 16 * PaletteNr].ToArgb());
								Marshal.WriteInt32(d.Scan0, y * d.Stride + x++ * 4, pal[(bb / 16) + 16 * PaletteNr].ToArgb());
								if (x >= Width)
								{
									y++;
									x = 0;
								}
							}
							b.UnlockBits(d);
						}
						break;
					}
				case ImageFormat.PLTT256:
					{
						Color[] pal = ConvertXBGR1555(Palette);
						if (firstTransparent)
						{
							pal[0] = Color.FromArgb(0, 0xFF, 0, 0);
						}
						if (CharacterType == CharFormat.CHAR)
						{
							b = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);//new Bitmap((int)(Data.Length / 64) * 8, 8, PixelFormat.Format32bppArgb);
							BitmapData d = b.LockBits(new Rectangle(0, 0, /*(int)(Data.Length / 64) * 8, 8*/Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
							for (int y = 0; y < Height; y += 8)
							{
								for (int x = 0; x < Width; x += 8)
								{
									for (int TileY = 0; TileY < 8; TileY++)
									{
										for (int TileX = 0; TileX < 8; TileX++)
										{
											byte by = Data[offset];
											int y2 = TileY + y;
											int x2 = TileX + x;//i * 8 + TileX;
											Marshal.WriteInt32(d.Scan0, y2 * d.Stride + x2 * 4, pal[by + 256 * PaletteNr].ToArgb());
											offset++;
										}
									}
								}
							}
							b.UnlockBits(d);
						}
						else
						{
							b = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
							BitmapData d = b.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
							int x = 0;
							int y = 0;
							foreach (byte bb in Data)
							{
								Marshal.WriteInt32(d.Scan0, y * d.Stride + x++ * 4, pal[bb + 256 * PaletteNr].ToArgb());
								if (x >= Width)
								{
									y++;
									x = 0;
								}
							}
							b.UnlockBits(d);
						}
						break;
					}
				case ImageFormat.COMP4x4:
					{
						Color[] pal = ConvertXBGR1555(Palette);
						b = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
						BitmapData d = b.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
						int x = 0;
						int y = 0;
						for (int i = 0; i < Data.Length / 4; i++)
						{
							uint texel = IOUtil.ReadU32LE(Data, i * 4);
							ushort data = IOUtil.ReadU16LE(Tex4x4, i * 2);
							int address = data & 0x3fff;
							address = address << 1;
							bool PTY = (data >> 14 & 0x1) == 1;
							bool A = (data >> 15 & 0x1) == 1;
							int shift = 0;
							for (int j = 0; j < 4; j++)
							{
								for (int k = 0; k < 4; k++)
								{
									uint idx = (texel >> (shift * 2) & 0x3);
									Color c = new Color();
									if (!PTY && A)
									{
										c = pal[address + idx];
									}
									else if (!PTY && !A)
									{
										c = (idx != 3) ? pal[address + idx] : Color.Transparent;
									}
									else if (PTY && A)
									{
										switch (idx)
										{
											case 0:
											case 1:
												c = pal[address + idx];
												break;
											case 2:
												c = Color.FromArgb(
													(5 * pal[address + 0].R + 3 * pal[address + 1].R) / 8,
													(5 * pal[address + 0].G + 3 * pal[address + 1].G) / 8,
													(5 * pal[address + 0].B + 3 * pal[address + 1].B) / 8);
												break;
											case 3:
												c = Color.FromArgb(
													(3 * pal[address + 0].R + 5 * pal[address + 1].R) / 8,
													(3 * pal[address + 0].G + 5 * pal[address + 1].G) / 8,
													(3 * pal[address + 0].B + 5 * pal[address + 1].B) / 8);
												break;
										}
									}
									else if (PTY && !A)
									{
										switch (idx)
										{
											case 0:
											case 1:
												c = pal[address + idx];
												break;
											case 2:
												c = Color.FromArgb(
													(pal[address + 0].R + pal[address + 1].R) / 2,
													(pal[address + 0].G + pal[address + 1].G) / 2,
													(pal[address + 0].B + pal[address + 1].B) / 2);
												break;
											case 3:
												c = Color.Transparent;
												break;
										}
									}
									Marshal.WriteInt32(d.Scan0, y * d.Stride + x++ * 4, c.ToArgb());
									shift++;
								}
								x -= 4;
								y++;
							}
							y -= 4;
							x += 4;
							if (x >= Width)
							{
								y += 4;
								x = 0;
							}
						}
						b.UnlockBits(d);
						break;
					}
				case ImageFormat.A5I3:
					{
						Color[] pal = ConvertXBGR1555(Palette);
						b = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
						BitmapData d = b.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
						int x = 0;
						int y = 0;
						foreach (byte bb in Data)
						{
							Color c = Color.FromArgb((bb >> 3) * 8, pal[bb & 0x7]);
							Marshal.WriteInt32(d.Scan0, y * d.Stride + x++ * 4, c.ToArgb());
							if (x >= Width)
							{
								y++;
								x = 0;
							}
						}
						b.UnlockBits(d);
						break;
					}
				case ImageFormat.DIRECT:
					{
						b = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
						BitmapData d = b.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
						Color[] pal = ConvertABGR1555(Data);
						int x = 0;
						int y = 0;
						for (int i = 0; i < pal.Length; i++)
						{
							/*if (Data[i * 2 + 1] >> 7 == 0)
							{
								pal[i] = Color.FromArgb(0, pal[i]);
							}*/
							Marshal.WriteInt32(d.Scan0, y * d.Stride + x++ * 4, pal[i].ToArgb());
							if (x >= Width)
							{
								y++;
								x = 0;
							}
						}
						b.UnlockBits(d);
						break;
					}
			}
			if (CharacterType == CharFormat.CHAR && cut)
			{
				b = CutImage(b, Width, 1);
			}
			return b;
		}

        public static Bitmap ToBitmap(byte[] Data, int Width, int Height, byte[] Palette, byte[] ScreenData, int ScreenWidth, int ScreenHeight, ImageFormat Type, CharFormat CharacterType)
        {
            const int TILE_SIZE = 8;
            const int TILE_INDEX_MASK = 0x3FF;
            const int HORIZONTAL_FLIP_BIT = 10;
            const int VERTICAL_FLIP_BIT = 11; 
            const int PALETTE_INDEX_SHIFT = 12;
            const int TILE_OFFSET_ADJUST_1 = 576;
            const int TILE_OFFSET_ADJUST_2 = 256;
            const int PALETTE_CHECK_BIT = 0x10;
            Bitmap screenBitmap = new Bitmap(ScreenData.Length / 2 * TILE_SIZE, TILE_SIZE);
            List<Bitmap> textureBitmaps = new List<Bitmap>();
            List<BitmapData> textureBitmapDatas = new List<BitmapData>();
            int paletteCount = Palette.Length / 2 / ((Type == ImageFormat.PLTT16) ? 16 : 256);
            for (int paletteIndex = 0; paletteIndex < paletteCount; paletteIndex++)
            {
                textureBitmaps.Add(ToBitmap(Data, Palette, paletteIndex, Width, Height, Type, CharacterType, cut: false));
                textureBitmapDatas.Add(
                    textureBitmaps[paletteIndex].LockBits(
                        new Rectangle(0, 0, textureBitmaps[paletteIndex].Width, textureBitmaps[paletteIndex].Height),
                        ImageLockMode.ReadOnly,
                        PixelFormat.Format32bppArgb
                    )
                );
            }
            BitmapData screenBitmapData = screenBitmap.LockBits(
                new Rectangle(0, 0, ScreenData.Length / 2 * TILE_SIZE, TILE_SIZE),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb
            );

            for (int screenDataIndex = 0; screenDataIndex < ScreenData.Length / 2; screenDataIndex++)
            {
                ushort screenDataValue = IOUtil.ReadU16LE(ScreenData, screenDataIndex * 2);
                int tileIndex = screenDataValue & TILE_INDEX_MASK;
                bool isHorizontallyFlipped = ((screenDataValue >> HORIZONTAL_FLIP_BIT) & 1) == 1;
                bool isVerticallyFlipped = ((screenDataValue >> VERTICAL_FLIP_BIT) & 1) == 1;
                int paletteIndex = screenDataValue >> PALETTE_INDEX_SHIFT;
                if (tileIndex > textureBitmaps[0].Width / TILE_SIZE)
                {
                    paletteIndex = ((((screenDataValue >> 8) & PALETTE_CHECK_BIT) != 0) ? 1 : 0);
                    tileIndex -= TILE_OFFSET_ADJUST_1;

                    if (tileIndex < 0)
                    {
                        tileIndex += TILE_OFFSET_ADJUST_1;
                        tileIndex -= TILE_OFFSET_ADJUST_2;
                    }
                }
                int tileY = (isVerticallyFlipped ? TILE_SIZE - 1 : 0);
                int destY = 0;
                while ((isVerticallyFlipped ? (tileY >= 0) : (tileY < TILE_SIZE)) && destY < TILE_SIZE)
                {
                    int tileX = (isHorizontallyFlipped ? TILE_SIZE - 1 : 0);
                    int destX = 0;

                    while ((isHorizontallyFlipped ? (tileX >= 0) : (tileX < TILE_SIZE)) && destX < TILE_SIZE)
                    {
                        int sourcePixelAddress = tileY * textureBitmapDatas[paletteIndex].Stride + tileIndex * TILE_SIZE * 4 + tileX * 4;
                        int destPixelAddress = destY * screenBitmapData.Stride + screenDataIndex * TILE_SIZE * 4 + destX * 4;
                        Marshal.WriteInt32(
                            screenBitmapData.Scan0,
                            destPixelAddress,
                            Marshal.ReadInt32(textureBitmapDatas[paletteIndex].Scan0, sourcePixelAddress)
                        );
                        tileX += ((!isHorizontallyFlipped) ? 1 : (-1));
                        destX++;
                    }
                    tileY += ((!isVerticallyFlipped) ? 1 : (-1));
                    destY++;
                }
            }
            screenBitmap.UnlockBits(screenBitmapData);
            for (int paletteIndex = 0; paletteIndex < paletteCount; paletteIndex++)
            {
                textureBitmaps[paletteIndex].UnlockBits(textureBitmapDatas[paletteIndex]);
            }
            return CutImage(screenBitmap, ScreenWidth, 1);
        }

        public static Bitmap ToBitmap(byte[] Data, byte[] Palette, int Width, int Height, NCER.cellDataBank.cellData cellData,CharacterDataMapingType mappingMode, ImageFormat type, CharFormat characterType)
        {
            const int TILE_SIZE = 8;
            const int BASE_CANVAS_WIDTH = 512;
            const int BASE_CANVAS_HEIGHT = 256;
            const int COORD_ADJUST_Y = 128;
            const int COORD_ADJUST_X = 256;
            const int FLIP_DIRECTION_NEGATIVE = -1;
            Bitmap baseCanvas = new Bitmap(BASE_CANVAS_WIDTH, BASE_CANVAS_HEIGHT);
            List<Bitmap> normalTextureBitmaps = new List<Bitmap>();
            List<Bitmap> transparentTextureBitmaps = new List<Bitmap>();
            int paletteCount = Palette.Length / 2 / ((type == ImageFormat.PLTT16) ? 16 : 256);
            for (int paletteIndex = 0; paletteIndex < paletteCount; paletteIndex++)
            {
                normalTextureBitmaps.Add(ToBitmap(Data, Palette, paletteIndex, Width, Height, type, characterType, cut: false));
                transparentTextureBitmaps.Add(ToBitmap(Data, Palette, paletteIndex, Width, Height, type, characterType, cut: false, firstTransparent: true));
            }
            using (Graphics graphics = Graphics.FromImage(baseCanvas))
            {
                foreach (NCER.cellDataBank.cellData.cellOAMAttrData oamAttr in cellData.CellOAMAttrData)
                {
                    Size objSize = oamAttr.GetSize();
                    int tileRowIndex = oamAttr.StartingCharacterName * TILE_SIZE /((mappingMode == CharacterDataMapingType.CHARACTERMAPING_2D) ? Width : objSize.Width);
                    int tileColumnOffset = oamAttr.StartingCharacterName * TILE_SIZE - tileRowIndex * ((mappingMode == CharacterDataMapingType.CHARACTERMAPING_2D) ? Width : objSize.Width);
                    int yCoord = oamAttr.YCoord;
                    int xCoord = oamAttr.XCoord;
                    yCoord = ((yCoord >> 7 != 1) ? (yCoord + COORD_ADJUST_Y) : (yCoord - COORD_ADJUST_Y));
                    xCoord = ((xCoord >> 8 != 1) ? (xCoord + COORD_ADJUST_X) : (xCoord - COORD_ADJUST_X));
                    if (objSize.Width + tileColumnOffset > ((mappingMode == CharacterDataMapingType.CHARACTERMAPING_2D) ? Width : objSize.Width))
                    {
                        int currentColumnOffset = tileColumnOffset;
                        int currentRowIndex = tileRowIndex;
                        int drawX = xCoord;
                        int blockCount = objSize.Width / (objSize.Width - tileColumnOffset);
                        for (int blockIndex = 0; blockIndex < blockCount; blockIndex++)
                        {
                            Bitmap textureBitmap = (oamAttr.OBJMode == 1) ?transparentTextureBitmaps[oamAttr.ColorParameter] : normalTextureBitmaps[oamAttr.ColorParameter];
                            int blockWidth = objSize.Width - tileColumnOffset;
                            graphics.DrawImage(CutImage(textureBitmap,(mappingMode == CharacterDataMapingType.CHARACTERMAPING_2D) ? Width : objSize.Width, 1),
                                new Rectangle(drawX, yCoord, blockWidth, objSize.Height),
                                new Rectangle(currentColumnOffset, currentRowIndex * TILE_SIZE, blockWidth, objSize.Height),
                                GraphicsUnit.Pixel);
                            if (currentColumnOffset + blockWidth == objSize.Width)
                            {
                                currentRowIndex++;
                                currentColumnOffset = 0;
                            }
                            drawX += blockWidth;
                        }
                    }
                    else if (oamAttr.FlipX && oamAttr.FlipY)
                    {
                        DrawFlippedTexture(graphics, oamAttr, normalTextureBitmaps, transparentTextureBitmaps,
                                          mappingMode, Width, objSize, tileColumnOffset, tileRowIndex,
                                          xCoord, yCoord, FLIP_DIRECTION_NEGATIVE, FLIP_DIRECTION_NEGATIVE);
                    }
                    else if (oamAttr.FlipX)
                    {
                        DrawFlippedTexture(graphics, oamAttr, normalTextureBitmaps, transparentTextureBitmaps,
                                          mappingMode, Width, objSize, tileColumnOffset, tileRowIndex,
                                          xCoord, yCoord, FLIP_DIRECTION_NEGATIVE, 1);
                    }
                    else if (oamAttr.FlipY)
                    {
                        DrawFlippedTexture(graphics, oamAttr, normalTextureBitmaps, transparentTextureBitmaps,
                                          mappingMode, Width, objSize, tileColumnOffset, tileRowIndex,
                                          xCoord, yCoord, 1, FLIP_DIRECTION_NEGATIVE);
                    }
                    else
                    {
                        Bitmap textureBitmap = (oamAttr.OBJMode == 1) ?transparentTextureBitmaps[oamAttr.ColorParameter] : normalTextureBitmaps[oamAttr.ColorParameter];
                        graphics.DrawImage(CutImage(textureBitmap, (mappingMode == CharacterDataMapingType.CHARACTERMAPING_2D) ? Width : objSize.Width, 1),
                            new Rectangle(xCoord, yCoord, objSize.Width, objSize.Height),
                            new Rectangle(tileColumnOffset, tileRowIndex * TILE_SIZE, objSize.Width, objSize.Height),
                            GraphicsUnit.Pixel);
                    }
                }
            }

            if (cellData.boundingRect != null)
            {
                int boundWidth = cellData.boundingRect.maxX - cellData.boundingRect.minX;
                int boundHeight = cellData.boundingRect.maxY - cellData.boundingRect.minY;

                Bitmap boundedBitmap = new Bitmap(boundWidth, boundHeight);
                using (Graphics graphics = Graphics.FromImage(boundedBitmap))
                {
                    graphics.DrawImage(baseCanvas,
                        new Rectangle(0, 0, boundWidth, boundHeight),
                        new Rectangle(COORD_ADJUST_X - boundWidth / 2, COORD_ADJUST_Y - boundHeight / 2, boundWidth, boundHeight),
                        GraphicsUnit.Pixel);
                }
                baseCanvas = boundedBitmap;
            }
            return baseCanvas;
        }

        private static void DrawFlippedTexture(Graphics graphics, NCER.cellDataBank.cellData.cellOAMAttrData oamAttr,List<Bitmap> normalTextures, List<Bitmap> transparentTextures,CharacterDataMapingType mappingMode, int width, Size objSize,int tileColumnOffset, int tileRowIndex, int xCoord, int yCoord,int xScale, int yScale)
        {
            const int TILE_SIZE = 8;
            Bitmap textureBitmap = (oamAttr.OBJMode == 1) ? transparentTextures[oamAttr.ColorParameter] : normalTextures[oamAttr.ColorParameter];
            int destX = xCoord + (xScale < 0 ? objSize.Width : 0);
            int destY = yCoord + (yScale < 0 ? objSize.Height : 0);
            graphics.DrawImage(CutImage(textureBitmap, (mappingMode == CharacterDataMapingType.CHARACTERMAPING_2D) ? width : objSize.Width, 1),
                new Rectangle(destX, destY, objSize.Width * xScale, objSize.Height * yScale),
                new Rectangle(tileColumnOffset, tileRowIndex * TILE_SIZE, objSize.Width, objSize.Height),
                GraphicsUnit.Pixel);
        }

        private static Bitmap CutImage(Image sourceImage, int targetWidth, int blockRowCount)
        {
            int blockHeight = sourceImage.Height / blockRowCount;
            int sourceBlockCount = sourceImage.Width / blockHeight;
            int targetBlockCount = targetWidth / blockHeight;
            int blockGroupCount = sourceBlockCount / targetBlockCount;
            Bitmap targetImage = new Bitmap(targetBlockCount * blockHeight, blockGroupCount * blockHeight * blockRowCount);
            Graphics graphics = Graphics.FromImage(targetImage);
            graphics.Clear(Color.Transparent);
            Rectangle sourceRect = new Rectangle(0, 0, targetBlockCount * blockHeight, blockHeight);
            Rectangle destRect = new Rectangle(0, 0, targetBlockCount * blockHeight, blockHeight);
            for (int blockRowIndex = 0; blockRowIndex < blockRowCount; blockRowIndex++)
            {
                sourceRect.Y = blockRowIndex * blockHeight;
                for (int blockGroupIndex = 0; blockGroupIndex < blockGroupCount; blockGroupIndex++)
                {
                    sourceRect.X = blockGroupIndex * targetBlockCount * blockHeight;
                    destRect.Y = blockGroupIndex * blockHeight + blockRowIndex * blockGroupCount * blockHeight;
                    graphics.DrawImage(sourceImage, destRect, sourceRect, GraphicsUnit.Pixel);
                }
            }
            return targetImage;
        }

        public static Color[] ConvertABGR1555(byte[] Data)
		{
			Color[] data = new Color[Data.Length / 2];
			for (int i = 0; i < Data.Length; i += 2)
			{
				data[i / 2] = Color.FromArgb((int)GFXUtil.ConvertColorFormat(IOUtil.ReadU16LE(Data, i), ColorFormat.ABGR1555, ColorFormat.ARGB8888));
			}
			return data;
		}

        public static byte[] ToABGR1555(Color[] Data)
        {
            byte[] result = new byte[Data.Length * 2];
            for (int i = 0; i < Data.Length; i++)
            {
                Color color = Data[i];
                uint abgr1555 = GFXUtil.ToColorFormat(
                    color.A, color.R, color.G, color.B,
                    ColorFormat.ABGR1555
                );
                IOUtil.WriteU16LE(result, i * 2, (ushort)abgr1555);
            }
            return result;
        }

        public static Color[] ConvertXBGR1555(byte[] Data)
		{
			Color[] data = new Color[Data.Length / 2];
			for (int i = 0; i < Data.Length; i += 2)
			{
				data[i / 2] = Color.FromArgb((int)GFXUtil.ConvertColorFormat(IOUtil.ReadU16LE(Data, i), ColorFormat.XBGR1555, ColorFormat.ARGB8888));
			}
			return data;
		}

        public static byte[] ToXBGR1555(Color[] Data)
        {
            byte[] result = new byte[Data.Length * 2];
            for (int i = 0; i < Data.Length; i++)
            {
                Color color = Data[i];
                uint xbgr1555 = GFXUtil.ToColorFormat(
                    color.A, color.R, color.G, color.B,
                    ColorFormat.XBGR1555
                );
                IOUtil.WriteU16LE(result, i * 2, (ushort)xbgr1555);
            }
            return result;
        }
    }
}