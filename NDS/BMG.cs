﻿using LibEveryFileExplorer.Files;
using System.Drawing;

namespace NDS
{
	public class BMG: FileFormat<BMG.BMGIdentifier>
	{
		public class BMGIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
                return Category_Strings;
            }

			public override string GetFileDescription()
			{
                return "Binary Message Strings (BMG)";
            }

			public override string GetFileFilter()
			{
                return "Binary Message Strings (*.bmg)|*.bmg";
            }

			public override Bitmap GetIcon()
			{
                return Resource.script_text;
            }

			public override FormatMatch IsFormat(EFEFile File)
			{
                if (File.Data.Length > 8 && File.Data[0] == 'M' && File.Data[1] == 'E' && File.Data[2] == 'S' && File.Data[3] == 'G' && File.Data[4] == 'b' && File.Data[5] == 'm' && File.Data[6] == 'g' && File.Data[7] == '1') return FormatMatch.Content;
                return FormatMatch.No;
            }
		}
	}
}
