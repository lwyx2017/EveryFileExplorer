using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace LibEveryFileExplorer.Files.SimpleFileSystem
{
	public class SFSFile
	{
		public SFSFile(Int32 Id, String Name, SFSDirectory Parent)
		{
			FileID = Id;
			FileName = Name;
			this.Parent = Parent;
            Tag = null;
        }
		public String FileName;
		public Int32 FileID;
		public Byte[] Data;
		public object Tag;

		public SFSDirectory Parent;

		public override string ToString()
		{
			return Parent.ToString() + FileName;
		}
	}
}
