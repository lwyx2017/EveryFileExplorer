﻿using System;
using System.Drawing;
using System.Windows.Forms;
using LibEveryFileExplorer.Files.SimpleFileSystem;
using LibEveryFileExplorer.Files;
using LibEveryFileExplorer;
using RuneFactory.RFWii;

namespace RuneFactory.UI
{
	public partial class FBTIViewer : Form
	{
		FBTI Archive;
		SFSDirectory Root;
		public FBTIViewer(FBTI Archive)
		{
			Root = Archive.ToFileSystem();
			InitializeComponent();
		}

		private void FBTIViewer_Load(object sender, EventArgs e)
		{
			fileBrowser1.UpdateDirectories(Root.GetTreeNodes());
		}

		private void fileBrowser1_OnDirectoryChanged(string Path)
		{
			var d = Root.GetDirectoryByPath(Path);
			fileBrowser1.UpdateContent(d.GetContent());
		}

		private void fileBrowser1_OnFileActivated(string Path)
		{
			var s = Root.GetFileByPath(Path);
			EveryFileExplorerUtil.OpenFile(new EFESFSFile(s), ((ViewableFile)Tag).File);
		}

		private void OnExport(object sender, EventArgs e)
		{
			var file = Root.GetFileByPath(fileBrowser1.SelectedPath);
			if (file == null) return;
			saveFileDialog1.Filter = System.IO.Path.GetExtension(fileBrowser1.SelectedPath).Replace(".", "").ToUpper() + " Files (*" + System.IO.Path.GetExtension(fileBrowser1.SelectedPath).ToLower() + ")|*" + System.IO.Path.GetExtension(fileBrowser1.SelectedPath).ToLower() + "|All Files (*.*)|*.*";
			saveFileDialog1.FileName = System.IO.Path.GetFileName(fileBrowser1.SelectedPath);
			if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK
				&& saveFileDialog1.FileName.Length > 0)
			{
				System.IO.File.Create(saveFileDialog1.FileName).Close();
				System.IO.File.WriteAllBytes(saveFileDialog1.FileName, file.Data);
			}
		}

		private void fileBrowser1_OnSelectionChanged(object sender, EventArgs e)
		{
			menuExport.Enabled = !(fileBrowser1.SelectedPath == fileBrowser1.SelectedFolderPath);
		}

		private void fileBrowser1_OnRightClick(Point Location)
		{
			var dir = Root.GetDirectoryByPath(fileBrowser1.SelectedPath);
			if (dir != null)
			{

			}
			else
			{
				//var file = Root.GetFileByPath(fileBrowser1.SelectedPath);
				contextMenu1.Show(fileBrowser1, Location);
			}
		}

		private void menuExportDir_Click(object sender, EventArgs e)
		{
			var dir = Root.GetDirectoryByPath(fileBrowser1.SelectedFolderPath);
			if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK
				&& folderBrowserDialog1.SelectedPath.Length > 0)
			{
				dir.Export(folderBrowserDialog1.SelectedPath);
			}
		}
	}
}
