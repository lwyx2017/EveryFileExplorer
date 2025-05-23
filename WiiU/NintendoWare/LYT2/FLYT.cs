﻿using LibEveryFileExplorer.Files;
using System.Drawing;

namespace WiiU.NintendoWare.LYT2
{
	public class FLYT : FileFormat<FLYT.FLYTIdentifier>
	{
		public class FLYTIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
                return Category_Layouts;
            }

			public override string GetFileDescription()
			{
				return "Cafe Layout (FLYT)";
			}

			public override string GetFileFilter()
			{
				return "Cafe Layout (*.bflyt)|*.bflyt";
			}

			public override Bitmap GetIcon()
			{
                return Resource.zone;
            }

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'F' && File.Data[1] == 'L' && File.Data[2] == 'Y' && File.Data[3] == 'T') return FormatMatch.Content;
				return FormatMatch.No;
			}

		}
	}
}
