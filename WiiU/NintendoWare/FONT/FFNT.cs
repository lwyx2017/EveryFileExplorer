﻿using LibEveryFileExplorer.Files;
using System.Drawing;

namespace WiiU.NintendoWare.FONT
{
	public class FFNT : FileFormat<FFNT.FFNTIdentifier>
	{
		public class FFNTIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Fonts;

            }

			public override string GetFileDescription()
			{
                return "Cafe Font (FFNT)";
            }

			public override string GetFileFilter()
			{
				return "Cafe Font (*.bffnt)|*.bffnt";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
                if (File.Data.Length > 0x10 && File.Data[0] == 'F' && File.Data[1] == 'F' && File.Data[2] == 'N' && File.Data[3] == 'T') return FormatMatch.Content;
                return FormatMatch.No;
            }

		}
	}
}
