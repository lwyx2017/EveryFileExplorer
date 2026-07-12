using LibEveryFileExplorer;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer.Files.SimpleFileSystem;
using NDS.NitroSystem.SND;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace NDS.UI
{
    public partial class SDATViewer : Form
    {
        private readonly SDAT Archive;
        private readonly SFSDirectory root;

        public SDATViewer(SDAT archive)
        {
            Archive = archive;
            root = archive.ToFileSystem();
            InitializeComponent();
        }

        private void SDATViewer_Load(object sender, EventArgs e)
        {
            fileBrowser1.UpdateDirectories(root.GetTreeNodes());
        }

        private void fileBrowser1_OnDirectoryChanged(string Path)
        {
            var d = root.GetDirectoryByPath(Path);
            fileBrowser1.UpdateContent(d.GetContent());
        }

        private void fileBrowser1_OnFileActivated(string Path)
        {
            var sfsFile = root.GetFileByPath(Path);
            if (sfsFile == null) return;

            EFESFSFile efFile = new EFESFSFile(sfsFile);
            string ext = System.IO.Path.GetExtension(sfsFile.FileName).ToLower();
            if (ext != ".sseq")
            {
                EveryFileExplorerUtil.OpenFile(efFile, ((ViewableFile)Tag).File);
                return;
            }
            string fileNameNoExt = System.IO.Path.GetFileNameWithoutExtension(sfsFile.FileName);
            int seqIndex = 0;
            if (fileNameNoExt.StartsWith("Seq "))
            {
                string numStr = fileNameNoExt.Substring(4);
                int.TryParse(numStr, out seqIndex);
            }
            byte[] dlsData = null;
            SBNK targetBank = Archive.GetSBNKBySeqIndex(seqIndex);
            if (targetBank != null)
            {
                ushort bankId = Archive.InfoBlock.SEQRecord.Entries[seqIndex].Bank;
                SWAR[] bankWaves = Archive.GetSWARByBankId(bankId);
                SBNK filledBank = SBNK.InitDLS(targetBank, bankWaves);
                dlsData = SBNK.ToDLS(filledBank);
            }
            EveryFileExplorerUtil.OpenFile(efFile, ((ViewableFile)Tag).File);
            ViewableFile[] openSeqFiles = EveryFileExplorerUtil.GetOpenFilesOfType(typeof(SSEQ));
            foreach (var vf in openSeqFiles)
            {
                if (vf.File.Name == sfsFile.FileName)
                {
                    Form form = vf.Dialog;
                    SSEQViewer viewer = form as SSEQViewer;
                    if (viewer != null && dlsData != null)
                    {
                        viewer.SetDLS(dlsData);
                        viewer.DlsButtonEnabled = true;
                    }
                    break;
                }
            }
        }

        private void OnExport(object sender, EventArgs e)
        {
            var file = root.GetFileByPath(fileBrowser1.SelectedPath);
            if (file == null) return;
            string ext = System.IO.Path.GetExtension(fileBrowser1.SelectedPath).ToLower();
            saveFileDialog1.Filter = $"{ext.TrimStart('.').ToUpper()} Files (*{ext})|*{ext}|All Files (*.*)|*.*";
            saveFileDialog1.FileName = System.IO.Path.GetFileName(fileBrowser1.SelectedPath);
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK
                && saveFileDialog1.FileName.Length > 0)
            {
                System.IO.File.WriteAllBytes(saveFileDialog1.FileName, file.Data);
            }
        }

        private void fileBrowser1_OnSelectionChanged(object sender, EventArgs e)
        {
            menuExport.Enabled = !(fileBrowser1.SelectedPath == fileBrowser1.SelectedFolderPath);
        }

        private void fileBrowser1_OnRightClick(Point Location)
        {
            var dir = root.GetDirectoryByPath(fileBrowser1.SelectedPath);
            if (dir == null)
            {
                contextMenu1.Show(fileBrowser1, Location);
            }
        }

        private void menuExportDir_Click(object sender, EventArgs e)
        {
            var dir = root.GetDirectoryByPath(fileBrowser1.SelectedFolderPath);
            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK
                && folderBrowserDialog1.SelectedPath.Length > 0)
            {
                dir.Export(folderBrowserDialog1.SelectedPath);
            }
        }
    }
}