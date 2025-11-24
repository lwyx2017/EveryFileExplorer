using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

namespace NDS.UI
{
    public partial class BMGViewer : Form
    {
        BMG BMGString;
        public BMGViewer(BMG BMGString)
        {
            InitializeComponent();
            this.BMGString = BMGString;
        }

        private bool IsValidSelection()
        {
            return dataGridView1.SelectedCells.Count > 0 &&
                   dataGridView1.SelectedCells[0].RowIndex >= 0 &&
                   dataGridView1.SelectedCells[0].RowIndex < dataGridView1.Rows.Count;
        }

        private int GetSelectedRowIndex()
        {
            return IsValidSelection() ? dataGridView1.SelectedCells[0].RowIndex : -1;
        }

        private void toolStripButton_add_Click(object sender, EventArgs e)
        {
            BMGString.INF1.NrOffset++;
            List<string> list = new List<string>(BMGString.DAT1.Strings);
            list.Add("New Text");
            BMGString.DAT1.Strings = list.ToArray();
            int newRowIndex = dataGridView1.Rows.Add("New Text");
            if (dataGridView1.Rows.Count > 0)
            {
                dataGridView1.ClearSelection();
                if (newRowIndex >= 0 && newRowIndex < dataGridView1.Rows.Count)
                {
                    dataGridView1.Rows[newRowIndex].Selected = true;
                    dataGridView1.FirstDisplayedScrollingRowIndex = newRowIndex;
                    textBox1.Text = "New Text";
                }
            }
        }

        private void toolStripButton_remove_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count == 0 || dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("Please select an entry to delete first.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int selectedIndex = dataGridView1.SelectedCells[0].RowIndex;

            if (selectedIndex < 0 || selectedIndex >= BMGString.DAT1.Strings.Length)
            {
                MessageBox.Show("Selected entry index is invalid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult result = MessageBox.Show(
                "The selected entry will be deleted. Continue?",
                "Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;
            try
            {
                BMGString.INF1.NrOffset--;
                List<string> list = new List<string>(BMGString.DAT1.Strings);
                list.RemoveAt(selectedIndex);
                BMGString.DAT1.Strings = list.ToArray();
                BMGString.INF1.Offsets = new uint[BMGString.INF1.NrOffset];
                dataGridView1.Rows.RemoveAt(selectedIndex);
                if (dataGridView1.Rows.Count > 0)
                {
                    int newSelectedIndex = Math.Min(selectedIndex, dataGridView1.Rows.Count - 1);
                    dataGridView1.Rows[newSelectedIndex].Selected = true;
                    textBox1.Text = BMGString.DAT1.Strings[newSelectedIndex];
                }
                else
                {
                    textBox1.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting entry: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void toolStripButton_up_Click(object sender, EventArgs e)
        {
            int rowIndex = GetSelectedRowIndex();
            if (rowIndex <= 0) return;
            if (rowIndex != 0)
            {
                string text = BMGString.DAT1.Strings[rowIndex];
                string text2 = BMGString.DAT1.Strings[rowIndex - 1];
                BMGString.DAT1.Strings[rowIndex] = text2;
                BMGString.DAT1.Strings[rowIndex - 1] = text;
                dataGridView1.Rows[rowIndex].SetValues(text2);
                dataGridView1.Rows[rowIndex - 1].SetValues(text);
                dataGridView1.Rows[rowIndex].Selected = false;
                dataGridView1.Rows[rowIndex - 1].Selected = true;
            }
        }

        private void toolStripButton_down_Click(object sender, EventArgs e)
        {
            int rowIndex = GetSelectedRowIndex();
            if (rowIndex < 0 || rowIndex >= dataGridView1.Rows.Count - 1) return;
            if (rowIndex != dataGridView1.Rows.Count - 1)
            {
                string text = BMGString.DAT1.Strings[rowIndex];
                string text2 = BMGString.DAT1.Strings[rowIndex + 1];
                BMGString.DAT1.Strings[rowIndex] = text2;
                BMGString.DAT1.Strings[rowIndex + 1] = text;
                dataGridView1.Rows[rowIndex].SetValues(text2);
                dataGridView1.Rows[rowIndex + 1].SetValues(text);
                dataGridView1.Rows[rowIndex].Selected = false;
                dataGridView1.Rows[rowIndex + 1].Selected = true;
            }
        }

        private void toolStripButton_import_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            ofd.Title = "Import Text File";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    byte[] fileData = File.ReadAllBytes(ofd.FileName);
                    BMGString.DAT1.FromTxt(fileData);
                    BMGString.INF1.NrOffset = (ushort)BMGString.DAT1.Strings.Length;
                    BMGString.INF1.Offsets = new uint[BMGString.INF1.NrOffset];
                    dataGridView1.Rows.Clear();
                    foreach (string text in BMGString.DAT1.Strings)
                    {
                        dataGridView1.Rows.Add(text);
                    }

                    MessageBox.Show("Text imported successfully!", "Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing text file: {ex.Message}", "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void toolStripButton_export_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            sfd.Title = "Export Text File";
            sfd.DefaultExt = "txt";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    byte[] txtData = BMGString.DAT1.ToTxt();
                    File.WriteAllBytes(sfd.FileName, txtData);
                    MessageBox.Show("Text exported successfully!", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting text file: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BMGViewer_Load(object sender, EventArgs e)
        {
            string[] strings = BMGString.DAT1.Strings;
            foreach (string text in strings)
            {
                if (text == null) continue;
                dataGridView1.Rows.Add(text);
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 0 || dataGridView1.SelectedCells.Count == 0)
            {
                textBox1.Text = string.Empty;
                return;
            }
            int rowIndex = dataGridView1.SelectedCells[0].RowIndex;
            if (rowIndex >= 0 && rowIndex < BMGString.DAT1.Strings.Length)
            {
                textBox1.Text = BMGString.DAT1.Strings[rowIndex];
            }
            else
            {
                textBox1.Text = string.Empty;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 0 || dataGridView1.SelectedCells.Count == 0)
            {
                return;
            }
            int rowIndex = dataGridView1.SelectedCells[0].RowIndex;
            if (rowIndex >= 0 && rowIndex < dataGridView1.Rows.Count && rowIndex < BMGString.DAT1.Strings.Length)
            {
                BMGString.DAT1.Strings[rowIndex] = textBox1.Text;
                dataGridView1.Rows[rowIndex].Cells[0].Value = textBox1.Text;
            }
        }
    }
}