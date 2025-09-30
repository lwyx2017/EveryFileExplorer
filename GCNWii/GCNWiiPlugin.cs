using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using GCNWii.JSystem;
using GCNWii.NintendoWare.LYT;
using LibEveryFileExplorer;
using LibEveryFileExplorer.Files.SimpleFileSystem;
using LibEveryFileExplorer.Script;

namespace GCNWii
{
    public class GCNWiiPlugin : EFEPlugin
    {
        public override void OnLoad()
        {
            EFEScript.RegisterCommand("GCNWii.U8.Unpack", (Action<string, string>)GCNWii_U8_Unpack);
            EFEScript.RegisterCommand("GCNWii.U8.UnpackSingle", (Action<string, string, string>)GCNWii_U8_UnpackSingle);

            EFEScript.RegisterCommand("GCNWii.TPL.ExportAllTextures", (Action<string, string>)GCNWii_TPL_ExportAllTextures);
            EFEScript.RegisterCommand("GCNWii.TPL.ExportTexture", (Action<string, int, string>)GCNWii_TPL_ExportTexture);

            EFEScript.RegisterCommand("GCNWii.BTI.Export", (Action<string, string>)GCNWii_BTI_Export);
            EFEScript.RegisterCommand("GCNWii.BTI.ExportWithMipmaps", (Action<string, string>)GCNWii_BTI_ExportWithMipmaps);
        }

        public static void GCNWii_U8_Unpack(string U8Path, string OutputDir)
        {
            if (!File.Exists(U8Path))throw new FileNotFoundException("U8 File doesn't exist: " + U8Path);
            byte[] fileData = File.ReadAllBytes(U8Path);
            U8 u8 = new U8(fileData);
            if (!Directory.Exists(OutputDir))Directory.CreateDirectory(OutputDir);
            SFSDirectory root = u8.ToFileSystem();
            Console.WriteLine("Start unpacking U8 files: " + Path.GetFileName(U8Path));
            ExtractDirectory(root, OutputDir);
            Console.WriteLine("U8 unpacking completed!");
        }

        private static void ExtractDirectory(SFSDirectory directory, string outputPath)
        {
            if (!Directory.Exists(outputPath))Directory.CreateDirectory(outputPath);
            foreach (var file in directory.Files)
            {
                try
                {
                    string filePath = Path.Combine(outputPath, file.FileName);
                    File.WriteAllBytes(filePath, file.Data);
                    Console.WriteLine("Extract files: " + filePath + " (" + file.Data.Length + " byte)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Extracting file failed " + file.FileName + ": " + ex.Message);
                }
            }

            foreach (var subDir in directory.SubDirectories)
            {
                string subDirPath = Path.Combine(outputPath, subDir.DirectoryName);
                ExtractDirectory(subDir, subDirPath);
            }
        }

        public static void GCNWii_U8_UnpackSingle(string U8Path, string FilePath, string OutputPath)
        {
            if (!File.Exists(U8Path)) throw new FileNotFoundException("U8 File doesn't exist: " + U8Path);
            byte[] fileData = File.ReadAllBytes(U8Path);
            U8 u8 = new U8(fileData);
            SFSDirectory root = u8.ToFileSystem();
            SFSFile targetFile = FindFileInDirectory(root, FilePath);
            if (targetFile == null)
            {
                throw new FileNotFoundException("File not found in U8 file: " + FilePath);
            }
            string outputDir = Path.GetDirectoryName(OutputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))Directory.CreateDirectory(outputDir);
            File.WriteAllBytes(OutputPath, targetFile.Data);
            Console.WriteLine("Successfully extracted file: " + FilePath + " -> " + OutputPath + " (" + targetFile.Data.Length + " byte)");
        }

        private static SFSFile FindFileInDirectory(SFSDirectory directory, string filePath)
        {
            string[] pathParts = filePath.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (pathParts.Length == 1)
            {
                return directory.Files.Find(f => f.FileName.Equals(pathParts[0], StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                string subDirName = pathParts[0];
                SFSDirectory subDir = directory.SubDirectories.Find(d => d.DirectoryName.Equals(subDirName, StringComparison.OrdinalIgnoreCase));
                if (subDir != null)
                {
                    string remainingPath = string.Join("/", pathParts, 1, pathParts.Length - 1);
                    return FindFileInDirectory(subDir, remainingPath);
                }
            }
            return null;
        }

        public static void GCNWii_TPL_ExportAllTextures(string TPLPath, string OutputDir)
        {
            if (!File.Exists(TPLPath))throw new FileNotFoundException("TPL File doesn't exist: " + TPLPath);
            byte[] fileData = File.ReadAllBytes(TPLPath);
            TPL tpl = new TPL(fileData);
            if (!Directory.Exists(OutputDir))Directory.CreateDirectory(OutputDir);
            string tplFileName = Path.GetFileNameWithoutExtension(TPLPath);
            for (int i = 0; i < tpl.Textures.Length; i++)
            {
                var texture = tpl.Textures[i];
                string textureName = tplFileName;
                if (tpl.Textures.Length > 1)
                {
                    textureName += "_" + i.ToString("D1");
                }
                textureName += ".png";
                string outputPath = Path.Combine(OutputDir, textureName);
                try
                {
                    Bitmap bitmap = texture.ToBitmap();
                    bitmap.Save(outputPath, ImageFormat.Png);
                    bitmap.Dispose();
                    Console.WriteLine("Successfully exported: " + textureName + " (" + texture.TextureHeader.Width + "x" + texture.TextureHeader.Height + ")");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exporting texture failed " + textureName + ": " + ex.Message);
                    throw;
                }
            }
        }

        public static void GCNWii_TPL_ExportTexture(string TPLPath, int TextureIndex, string OutputPath)
        {
            if (!File.Exists(TPLPath))throw new FileNotFoundException("TPL File doesn't exist: " + TPLPath);
            byte[] fileData = File.ReadAllBytes(TPLPath);
            TPL tpl = new TPL(fileData);
            if (TextureIndex < 0 || TextureIndex >= tpl.Textures.Length)
                throw new ArgumentOutOfRangeException("TextureIndex", "Texture Index " + TextureIndex.ToString() + " out of range (0-" + (tpl.Textures.Length - 1).ToString() + ")");
            string outputDir = Path.GetDirectoryName(OutputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))Directory.CreateDirectory(outputDir);
            var texture = tpl.Textures[TextureIndex];
            Bitmap bitmap = texture.ToBitmap();
            bitmap.Save(OutputPath, ImageFormat.Png);
            bitmap.Dispose();
            Console.WriteLine("Successfully exported texture " + TextureIndex.ToString() + " -> " + OutputPath + " (" + texture.TextureHeader.Width + "x" + texture.TextureHeader.Height + ")");
        }

        public static void GCNWii_BTI_Export(string BTIPath, string OutputPath)
        {
            if (!File.Exists(BTIPath))throw new FileNotFoundException("BTI File doesn't exist: " + BTIPath);
            byte[] fileData = File.ReadAllBytes(BTIPath);
            BTI bti = new BTI(fileData);
            string outputDir = Path.GetDirectoryName(OutputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))Directory.CreateDirectory(outputDir);
            try
            {
                Bitmap bitmap = bti.ToBitmap(0);
                bitmap.Save(OutputPath, ImageFormat.Png);
                bitmap.Dispose();
                Console.WriteLine("Successfully exported BTI: " + Path.GetFileName(BTIPath) + " -> " + OutputPath +
                    " (" + bti.Header.Width + "x" + bti.Header.Height + ")");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Export BTI failed " + Path.GetFileName(BTIPath) + ": " + ex.Message);
                throw;
            }
        }

        public static void GCNWii_BTI_ExportWithMipmaps(string BTIPath, string OutputDir)
        {
            if (!File.Exists(BTIPath))throw new FileNotFoundException("BTI File doesn't exist: " + BTIPath);
            byte[] fileData = File.ReadAllBytes(BTIPath);
            BTI bti = new BTI(fileData);
            if (!Directory.Exists(OutputDir))Directory.CreateDirectory(OutputDir);
            string btiFileName = Path.GetFileNameWithoutExtension(BTIPath);
            try
            {
                int exportedCount = 0;
                for (int level = 0; level < bti.Header.MipMapCount; level++)
                {
                    try
                    {
                        int width = bti.Header.Width >> level;
                        int height = bti.Header.Height >> level;
                        if (width < 1 || height < 1)continue;
                        string outputName = btiFileName;
                        if (bti.Header.MipMapCount > 1)
                        {
                            outputName += "_Mip" + level.ToString("D1");
                        }
                        outputName += ".png";
                        string outputPath = Path.Combine(OutputDir, outputName);
                        Bitmap bitmap = bti.ToBitmap(level);
                        bitmap.Save(outputPath, ImageFormat.Png);
                        bitmap.Dispose();
                        Console.WriteLine("  Export MipMap " + level + ": " + outputName + " (" + width + "x" + height + ")");
                        exportedCount++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("  Export MipMap " + level + " failure: " + ex.Message);
                    }
                }
                if (exportedCount > 0)
                {
                    Console.WriteLine("Successfully exported BTI: " + Path.GetFileName(BTIPath) + " (" + exportedCount + " MipMap level)");
                }
                else
                {
                    throw new Exception("Failed to export any MipMap levels");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Export BTI failed " + Path.GetFileName(BTIPath) + ": " + ex.Message);
                throw;
            }
        }
    }
}