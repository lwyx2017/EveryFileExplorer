using LibEveryFileExplorer.Files;
using System.Drawing;

namespace NDS.NitroSystem.SND
{
    public class SSAR : FileFormat<SSAR.SSARIdentifier>
    {
        public class SSARIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Sound;
            }

            public override string GetFileDescription()
            {
                return "Nitro Sound Sequence Archive (SSAR)";
            }

            public override string GetFileFilter()
            {
                return "Nitro Sound Sequence Archive (*.ssar)|*.ssar";
            }

            public override Bitmap GetIcon()
            {
                return Resource.note_box;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'S' && File.Data[1] == 'S' && File.Data[2] == 'A' && File.Data[3] == 'R') return FormatMatch.Content;
                return FormatMatch.No;
            }
        }
    }
}
