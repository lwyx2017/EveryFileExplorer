using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using LibEveryFileExplorer;
using LibEveryFileExplorer.Script;
using RuneFactory.RFWii;
using LibEveryFileExplorer.Files.SimpleFileSystem;

namespace RuneFactory
{
    public class RFPlugin : EFEPlugin
    {
        public override void OnLoad()
        {
            EFEScript.RegisterCommand("RF.HXTB.ExportAllTextures", (Action<string, string>)RF_HXTB_ExportAllTextures);
            EFEScript.RegisterCommand("RF.HXTB.ExportTexture", (Action<string, int, string>)RF_HXTB_ExportTexture);

            EFEScript.RegisterCommand("RF.FBTI.Unpack", (Action<string, string>)RF_FBTI_Unpack);
            EFEScript.RegisterCommand("RF.FBTI.UnpackSingle", (Action<string, string, string>)RF_FBTI_UnpackSingle);
        }

        public static void RF_HXTB_ExportAllTextures(string HXTBPath, string OutputDir)
        {
            if (!File.Exists(HXTBPath))throw new FileNotFoundException("HXTB File doesn't exist: " + HXTBPath);
            byte[] fileData = File.ReadAllBytes(HXTBPath);
            HXTB hxtb = new HXTB(fileData);
            if (!Directory.Exists(OutputDir))Directory.CreateDirectory(OutputDir);
            for (int i = 0; i < hxtb.Textures.Count; i++)
            {
                var texture = hxtb.Textures[i];
                string textureName = string.IsNullOrEmpty(texture.Name) ? "texture_" + i.ToString() : texture.Name;
                foreach (char c in Path.GetInvalidFileNameChars())textureName = textureName.Replace(c, '_');
                string outputPath = Path.Combine(OutputDir, textureName + ".png");
                try
                {
                    Bitmap bitmap = texture.ToBitmap();
                    bitmap.Save(outputPath, ImageFormat.Png);
                    bitmap.Dispose();
                    Console.WriteLine("Successfully exported: " + textureName + " -> " + outputPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exporting texture failed " + textureName + ": " + ex.Message);
                    throw;
                }
            }
        }

        public static void RF_HXTB_ExportTexture(string HXTBPath, int TextureIndex, string OutputPath)
        {
            if (!File.Exists(HXTBPath))throw new FileNotFoundException("HXTB File doesn't exist: " + HXTBPath);
            byte[] fileData = File.ReadAllBytes(HXTBPath);
            HXTB hxtb = new HXTB(fileData);
            if (TextureIndex < 0 || TextureIndex >= hxtb.Textures.Count)
                throw new ArgumentOutOfRangeException("TextureIndex", "Texture Index " + TextureIndex.ToString() + " out of range (0-" + (hxtb.Textures.Count - 1).ToString() + ")");
            string outputDir = Path.GetDirectoryName(OutputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))Directory.CreateDirectory(outputDir);
            var texture = hxtb.Textures[TextureIndex];
            Bitmap bitmap = texture.ToBitmap();
            bitmap.Save(OutputPath, ImageFormat.Png);
            bitmap.Dispose();
            Console.WriteLine("Successfully exported texture " + TextureIndex.ToString() + " -> " + OutputPath);
        }

        public static void RF_FBTI_Unpack(string FBTIPath, string OutputDir)
        {
            if (!File.Exists(FBTIPath))throw new FileNotFoundException("FBTI File doesn't exist: " + FBTIPath);
            byte[] fileData = File.ReadAllBytes(FBTIPath);
            FBTI fbti = new FBTI(fileData);
            if (!Directory.Exists(OutputDir))Directory.CreateDirectory(OutputDir);
            SFSDirectory root = fbti.ToFileSystem();
            Console.WriteLine("Start unpacking FBTI files: " + Path.GetFileName(FBTIPath));
            Console.WriteLine("Number of files: " + root.Files.Count);
            int extractedCount = 0;
            foreach (var file in root.Files)
            {
                try
                {
                    string outputPath = Path.Combine(OutputDir, file.FileName);
                    File.WriteAllBytes(outputPath, file.Data);
                    Console.WriteLine("Extract files: " + file.FileName + " (" + file.Data.Length + " byte)");
                    extractedCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Extracting file failed " + file.FileName + ": " + ex.Message);
                }
            }
            Console.WriteLine("Unpacking completed! Total extracted " + extractedCount + " files");
        }

        public static void RF_FBTI_UnpackSingle(string FBTIPath, string FileIndex, string OutputPath)
        {
            if (!File.Exists(FBTIPath))throw new FileNotFoundException("FBTI File doesn't exist: " + FBTIPath);
            byte[] fileData = File.ReadAllBytes(FBTIPath);
            FBTI fbti = new FBTI(fileData);
            SFSDirectory root = fbti.ToFileSystem();
            SFSFile targetFile = null;
            if (int.TryParse(FileIndex, out int index))
            {
                if (index >= 0 && index < root.Files.Count)
                {
                    targetFile = root.Files[index];
                }
                else
                {
                    throw new ArgumentOutOfRangeException("FileIndex", "File index " + index.ToString() + " out of range (0-" + (root.Files.Count - 1).ToString() + ")");
                }
            }
            else
            {
                targetFile = root.Files.Find(f => f.FileName.Equals(FileIndex, StringComparison.OrdinalIgnoreCase));
                if (targetFile == null)
                {
                    throw new FileNotFoundException("File not found in FBTI file: " + FileIndex);
                }
            }
            string outputDir = Path.GetDirectoryName(OutputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))Directory.CreateDirectory(outputDir);
            File.WriteAllBytes(OutputPath, targetFile.Data);
            Console.WriteLine("Successfully extracted file: " + targetFile.FileName + " -> " + OutputPath + " (" + targetFile.Data.Length + " byte)");
        }
    }
}