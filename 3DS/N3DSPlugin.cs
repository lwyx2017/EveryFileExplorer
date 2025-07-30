using System;
using LibEveryFileExplorer;
using LibEveryFileExplorer.Script;
using _3DS.NintendoWare.GFX;
using System.IO;
using CommonFiles;

namespace _3DS
{
	public class N3DSPlugin : EFEPlugin
	{
		public override void OnLoad()
		{
			EFEScript.RegisterCommand("N3DS.NW4C.GFX.ToOBJ", (Action<String, int, String>)N3DS_NW4C_GFX_ToOBJ);
		}

        public static void N3DS_NW4C_GFX_ToOBJ(String CGFXPath, int ModelIdx, String OBJPath)
        {
            CGFX c = new CGFX(File.ReadAllBytes(CGFXPath));
            string baseDir = Path.GetDirectoryName(OBJPath);
            string baseName = Path.GetFileNameWithoutExtension(OBJPath);
            string texDir = Path.Combine(baseDir, baseName + "_Tex");
            if (c.Data.Textures != null && c.Data.Textures.Length > 0)
            {
                Directory.CreateDirectory(texDir);
                foreach (var v in c.Data.Textures)
                {
                    if (!(v is ImageTextureCtr)) continue;
                    string texPath = Path.Combine(texDir, v.Name + ".png");
                    ((ImageTextureCtr)v).GetBitmap().Save(texPath);
                }
            }
            if (c.Data.Models != null && ModelIdx < c.Data.Models.Length)
            {
                CMDL Model = c.Data.Models[ModelIdx];
                OBJ o = Model.ToOBJ();
                o.MTLPath = baseName + ".mtl";
                MTL m = Model.ToMTL(baseName + "_Tex");
                File.WriteAllBytes(OBJPath, o.Write());
                File.WriteAllBytes(Path.ChangeExtension(OBJPath, "mtl"), m.Write());
            }
        }
    }
}
