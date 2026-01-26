using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MarioKart.MK7;

namespace MarioKart.UI
{
    public partial class MK7ObjFlowViewer : Form
    {
        ObjFlow FBOCFile;
        public MK7ObjFlowViewer(ObjFlow FBOC)
        {
            FBOCFile = FBOC;
            InitializeComponent();
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = true;
        }

        private void MK7ObjFlowViewer_Load(object sender, System.EventArgs e)
        {
            ShowMK7ObjFlow();
        }

        private void ShowMK7ObjFlow()
        {
            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();
            AddColumns();
            AddDataRows();
        }

        private void AddColumns()
        {
            dataGridView1.Columns.Add("ObjectID", "ObjectID");
            dataGridView1.Columns.Add("Unknown1", "Unknown1");
            dataGridView1.Columns.Add("Unknown2", "Unknown2");
            dataGridView1.Columns.Add("Unknown3", "Unknown3");
            dataGridView1.Columns.Add("Unknown4", "Unknown4");
            dataGridView1.Columns.Add("Unknown5", "Unknown5");
            dataGridView1.Columns.Add("Unknown6", "Unknown6");
            dataGridView1.Columns.Add("Unknown7", "Unknown7");
            dataGridView1.Columns.Add("Unknown8", "Unknown8");
            dataGridView1.Columns.Add("Unknown9", "Unknown9");
            dataGridView1.Columns.Add("Unknown10", "Unknown10");
            dataGridView1.Columns.Add("Unknown11", "Unknown11");
            dataGridView1.Columns.Add("Name", "Name");
            dataGridView1.Columns.Add("ParticleName", "ParticleName");
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        private void AddDataRows()
        {
            foreach (var obj in FBOCFile.Objects)
            {
                dataGridView1.Rows.Add(
                    obj.ObjectID,
                    obj.Unknown1,
                    obj.Unknown2,
                    obj.Unknown3,
                    obj.Unknown4,
                    obj.Unknown5,
                    obj.Unknown6,
                    obj.Unknown7,
                    obj.Unknown8,
                    obj.Unknown9,
                    obj.Unknown10,
                    obj.Unknown11,
                    obj.Name,
                    obj.ParticleName
                );
            }
        }

        private void toolStripButton_add_Click(object sender, EventArgs e)
        {
            ObjFlow.ObjFlowEntry newEntry = new ObjFlow.ObjFlowEntry();
            FBOCFile.AddEntry(newEntry);
            ShowMK7ObjFlow();
            if (dataGridView1.Rows.Count > 0)
            {
                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Selected = true;
                dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.Rows.Count - 1;
            }
        }

        private void toolStripButton_remove_Click(object sender, EventArgs e)
        {
            DialogResult status = MessageBox.Show(
             "All selected rows will be deleted. This operation cannot be undone! Are you sure you want to proceed?", "Warning",
             MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (status == DialogResult.Yes)
            {
                DelRows();
            }
        }

        private void DelRows()
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select the rows to delete first!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<int> selectedIndices = new List<int>();
            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                if (!row.IsNewRow)
                {
                    selectedIndices.Add(row.Index);
                }
            }

            selectedIndices.Sort((a, b) => b.CompareTo(a));
            foreach (int index in selectedIndices)
            {
                FBOCFile.RemoveEntry(index);
            }
            ShowMK7ObjFlow();
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= FBOCFile.Objects.Length || e.ColumnIndex < 0)
                return;

            try
            {
                var targetEntry = FBOCFile.Objects[e.RowIndex];
                var columnName = dataGridView1.Columns[e.ColumnIndex].Name;
                var cellValue = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString() ?? string.Empty;
                switch (columnName)
                {
                    case "ObjectID":
                        targetEntry.ObjectID = string.IsNullOrEmpty(cellValue) ? (ushort)0 : ushort.Parse(cellValue);
                        break;
                    case "Unknown1":
                        targetEntry.Unknown1 = string.IsNullOrEmpty(cellValue) ? (ushort)0 : ushort.Parse(cellValue);
                        break;
                    case "Unknown2":
                        targetEntry.Unknown2 = string.IsNullOrEmpty(cellValue) ? (ushort)0 : ushort.Parse(cellValue);
                        break;
                    case "Unknown3":
                        targetEntry.Unknown3 = string.IsNullOrEmpty(cellValue) ? (ushort)0 : ushort.Parse(cellValue);
                        break;
                    case "Unknown4":
                        targetEntry.Unknown4 = string.IsNullOrEmpty(cellValue) ? 0u : uint.Parse(cellValue);
                        break;
                    case "Unknown5":
                        targetEntry.Unknown5 = string.IsNullOrEmpty(cellValue) ? 0u : uint.Parse(cellValue);
                        break;
                    case "Unknown6":
                        targetEntry.Unknown6 = string.IsNullOrEmpty(cellValue) ? 0u : uint.Parse(cellValue);
                        break;
                    case "Unknown7":
                        targetEntry.Unknown7 = string.IsNullOrEmpty(cellValue) ? (ushort)0 : ushort.Parse(cellValue);
                        break;
                    case "Unknown8":
                        targetEntry.Unknown8 = string.IsNullOrEmpty(cellValue) ? (ushort)0 : ushort.Parse(cellValue);
                        break;
                    case "Unknown9":
                        targetEntry.Unknown9 = string.IsNullOrEmpty(cellValue) ? (ushort)0 : ushort.Parse(cellValue);
                        break;
                    case "Unknown10":
                        targetEntry.Unknown10 = string.IsNullOrEmpty(cellValue) ? (ushort)0 : ushort.Parse(cellValue);
                        break;
                    case "Unknown11":
                        targetEntry.Unknown11 = string.IsNullOrEmpty(cellValue) ? 0u : uint.Parse(cellValue);
                        break;
                    case "Name":
                        targetEntry.Name = cellValue;
                        break;
                    case "ParticleName":
                        targetEntry.ParticleName = cellValue;
                        break;
                }
            }
            catch (FormatException ex)
            {
                MessageBox.Show($"Input format error：{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShowMK7ObjFlow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update data：{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }
    }
}