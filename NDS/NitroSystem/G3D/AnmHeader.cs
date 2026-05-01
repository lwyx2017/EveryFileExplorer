using LibEveryFileExplorer.IO;
using System.Text;

namespace NDS.NitroSystem.G3D
{
    public class AnmHeader
    {
        public enum Category0
        {
            M,
            J,
            V
        }

        public enum Category1
        {
            AC,
            AV,
            AM,
            PT,
            AT
        }

        public string category0;
        public byte revision;
        public string category1;

        public AnmHeader(EndianBinaryReader er, Category0 category0, Category1 category1)
        {
            this.category0 = er.ReadString(Encoding.ASCII, 1);
            revision = er.ReadByte();
            this.category1 = er.ReadString(Encoding.ASCII, 2);
        }
    }
}