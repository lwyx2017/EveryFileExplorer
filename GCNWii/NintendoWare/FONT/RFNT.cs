using LibEveryFileExplorer.Files;
using System.Drawing;

namespace GCNWii.NintendoWare.LYT
{
	public class RFNT:FileFormat<RFNT.RFNTIdentifier>
	{
		public class RFNTIdentifier:FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return Category_Fonts;

            }

			public override string GetFileDescription()
			{
                return "Binary Revolution Font (RFNT)";
            }

			public override string GetFileFilter()
			{
				return "Binary Revolution Font (*.brfnt)|*.brfnt";
			}

			public override Bitmap GetIcon()
			{
				return null;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
                if (File.Data.Length > 0x10 && File.Data[0] == 'R' && File.Data[1] == 'F' && File.Data[2] == 'N' && File.Data[3] == 'T') return FormatMatch.Content;
                return FormatMatch.No;
            }

		}
	}
}
