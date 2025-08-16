using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace _3DS.UI
{
	public partial class SAHTViewer : Form
	{
		public SARCHashTable HashTable;
		public SAHTViewer(SARCHashTable HashTable)
		{
			this.HashTable = HashTable;
			InitializeComponent();
		}

        private bool isAscending = true;

        private void SAHTViewer_Load(object sender, EventArgs e)
		{
			UpdateEntries();
            toolStripButton3.Text = "Sort A-Z";
            toolStripButton2.Enabled = false;
            toolStripButton4.Enabled = false;
        }

		private void listView1_SelectedIndexChanged(object sender, EventArgs e)
		{
            bool hasSelection = listView1.SelectedIndices.Count > 0;
            toolStripButton2.Enabled = hasSelection;
            toolStripButton4.Enabled = hasSelection;
        }

		private void UpdateEntries()
		{
			listView1.BeginUpdate();
			listView1.Items.Clear();
			foreach (var v in HashTable.Entries)
			{
				listView1.Items.Add(new ListViewItem(new string[] { v.Name, "0x" + v.Hash.ToString("X8") }));
			}
			listView1.EndUpdate();
		}

		private void toolStripButton1_Click(object sender, EventArgs e)
		{
			String name = Microsoft.VisualBasic.Interaction.InputBox("Please give the name or path of the file:", "New Entry");
			if (name == null || name.Length == 0) return;
			if (HashTable.GetEntryByName(name) != null)
			{
				MessageBox.Show("This name is already in the table!");
				return;
			}
			HashTable.Entries.Add(new SARCHashTable.SAHTEntry(name));
			UpdateEntries();
		}

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count == 0) return;
            DialogResult result = 
				MessageBox.Show($"The selected file name will be deleted, Do you want to continue?", "Warning",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result == DialogResult.OK)
            {
                HashTable.Entries.RemoveAt(listView1.SelectedIndices[0]);
                UpdateEntries();
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count == 0)
            {
                MessageBox.Show("Please select an Entry to Rename first", "Prompt", MessageBoxButtons.OK, MessageBoxIcon.Information);return;
            }
            int selectedIndex = listView1.SelectedIndices[0];
            SARCHashTable.SAHTEntry selectedEntry = HashTable.Entries[selectedIndex];
            string newName = Microsoft.VisualBasic.Interaction.InputBox(
                "Please give the new name or path of the file:",
                "Rename Entry",
                selectedEntry.Name
            );

            if (string.IsNullOrWhiteSpace(newName))
            {
                return;
            }


            var existingEntry = HashTable.GetEntryByName(newName);
            if (existingEntry != null && existingEntry != selectedEntry)
            {
                MessageBox.Show($"The name '{newName}' already exists!", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            selectedEntry.Name = newName;
            selectedEntry.Hash = SARC.GetHashFromName(newName, 0x65);
            UpdateEntries();
            if (selectedIndex < listView1.Items.Count)
            {
                listView1.Items[selectedIndex].Selected = true;
                listView1.Items[selectedIndex].Focused = true;
            }
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            ImportFromText();
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            ExportToText();
        }

        private void ExportToText()
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                sfd.Title = "Export Hash Table Entries";
                sfd.FileName = "HashTable";
                sfd.DefaultExt = "txt";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (StreamWriter writer = new StreamWriter(sfd.FileName))
                        {
                            foreach (var entry in HashTable.Entries)
                            {
                                writer.WriteLine(entry.Name);
                            }
                        }
                        MessageBox.Show($"Successfully exported {HashTable.Entries.Count} entries!", "Export Complete",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error exporting entries: {ex.Message}", "Export Error",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ImportFromText()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                ofd.Title = "Import Hash Table Entries";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var existingNames = new HashSet<string>(HashTable.Entries.Select(e => e.Name));
                        var newEntries = new List<SARCHashTable.SAHTEntry>();
                        int addedCount = 0;
                        int duplicateCount = 0;

                        using (StreamReader reader = new StreamReader(ofd.FileName))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                line = line.Trim();
                                if (string.IsNullOrWhiteSpace(line)) continue;

                                if (!existingNames.Contains(line))
                                {
                                    newEntries.Add(new SARCHashTable.SAHTEntry(line));
                                    existingNames.Add(line);
                                    addedCount++;
                                }
                                else
                                {
                                    duplicateCount++;
                                }
                            }
                        }

                        HashTable.Entries.AddRange(newEntries);
                        UpdateEntries();

                        string message = $"Imported {addedCount} new entries";
                        if (duplicateCount > 0)
                        {
                            message += $"\nSkipped {duplicateCount} duplicates";
                        }

                        MessageBox.Show(message, "Import Complete",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error importing entries: {ex.Message}", "Import Error",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (isAscending)
            {
                HashTable.Entries.Sort((v1, v2) => v1.Name.CompareTo(v2.Name));
                toolStripButton3.Text = "Sort Z-A";
            }
            else
            {
                HashTable.Entries.Sort((v1, v2) => v2.Name.CompareTo(v1.Name));
                toolStripButton3.Text = "Sort A-Z";
            }

            isAscending = !isAscending;
            UpdateEntries();
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            RemoveDuplicates();
        }

        private void RemoveDuplicates()
        {
            DialogResult result = MessageBox.Show(
                "This will remove all entries with duplicate hash values. Continue?",
                "Remove duplicates",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result != DialogResult.Yes) return;

            var uniqueEntries = new Dictionary<uint, SARCHashTable.SAHTEntry>();
            var duplicates = new List<SARCHashTable.SAHTEntry>();

            foreach (var entry in HashTable.Entries)
            {
                if (!uniqueEntries.ContainsKey(entry.Hash))
                {
                    uniqueEntries.Add(entry.Hash, entry);
                }
                else
                {
                    duplicates.Add(entry);
                }
            }

            foreach (var dup in duplicates)
            {
                HashTable.Entries.Remove(dup);
            }

            UpdateEntries();
            toolStripButton2.Enabled = false;
            toolStripButton4.Enabled = false;
            MessageBox.Show($"Removed {duplicates.Count} duplicate entries", "Cleanup completed");
        }
    }
}
