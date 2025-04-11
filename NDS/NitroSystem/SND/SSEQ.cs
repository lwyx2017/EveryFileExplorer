using LibEveryFileExplorer.Files;
using System.Drawing;

namespace NDS.NitroSystem.SND
{
    public class SSEQ : FileFormat<SSEQ.SSEQIdentifier>
    {
        public class SSEQIdentifier : FileFormatIdentifier
        {
            public override string GetCategory()
            {
                return Category_Sound;
            }

            public override string GetFileDescription()
            {
                return "Nitro Sound Sequence (SSEQ)";
            }

            public override string GetFileFilter()
            {
                return "Nitro Sound Sequence (*.sseq)|*.sseq";
            }

            public override Bitmap GetIcon()
            {
                return Resource.note;
            }

            public override FormatMatch IsFormat(EFEFile File)
            {
                if (File.Data.Length > 4 && File.Data[0] == 'S' && File.Data[1] == 'S' && File.Data[2] == 'E' && File.Data[3] == 'Q') return FormatMatch.Content;
                return FormatMatch.No;
            }
        }
    }
}
