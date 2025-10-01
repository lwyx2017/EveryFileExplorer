using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace EveryFileExplorer.UI
{
    public partial class EFESCExecute:Form
    {
        public EFESCExecute()
        {
            InitializeComponent();
            checkBoxpreserve.Checked = true;
        }

        private void btnBrowseScript_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Every File Explorer Script (*.efesc)|*.efesc";
                openFileDialog.Title = "Select EFESC Script";
                string scriptsFolder = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Scripts");
                if (Directory.Exists(scriptsFolder))
                {
                    openFileDialog.InitialDirectory = scriptsFolder;
                }
                else
                {
                    openFileDialog.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath);
                }

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtScriptPath.Text = openFileDialog.FileName;
                }
            }
        }

        private void btnBrowseInput_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select Input Directory";
                folderDialog.ShowNewFolderButton = false;
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtInputPath.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select Output Directory";
                folderDialog.ShowNewFolderButton = true;
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtOutputPath.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            if (!ValidatePaths()) return;
            try
            {
                string scriptPath = EscapeArgument(txtScriptPath.Text);
                string inputPath = EscapeArgument(txtInputPath.Text);
                string outputPath = EscapeArgument(txtOutputPath.Text);
                string appendValue = checkBoxpreserve.Checked ? "true" : "false";
                string arguments = $"/C \"\"EveryFileExplorer.exe\" {scriptPath} {inputPath} {outputPath} {appendValue}\"";

                Process.Start(new ProcessStartInfo("cmd", arguments)
                {
                    WorkingDirectory = Path.GetDirectoryName(Application.ExecutablePath),
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing command: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnExecute.Enabled = true;
                btnExecute.Text = "Execute";
            }
        }

        private string EscapeArgument(string argument)
        {
            argument = argument.Trim();
            if (string.IsNullOrEmpty(argument))return "\"\"";
            return $"\"{argument}\"";
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private bool ValidatePaths()
        {
            if (string.IsNullOrWhiteSpace(txtScriptPath.Text))
            {
                MessageBox.Show("Please select an EFESC script file.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!File.Exists(txtScriptPath.Text.Trim()))
            {
                MessageBox.Show("The selected script file does not exist.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (Path.GetExtension(txtScriptPath.Text.Trim()).ToLower() != ".efesc")
            {
                MessageBox.Show("Please select a valid .efesc file.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtInputPath.Text))
            {
                MessageBox.Show("Please select an input directory.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!Directory.Exists(txtInputPath.Text.Trim()))
            {
                MessageBox.Show("The selected input directory does not exist.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtOutputPath.Text))
            {
                MessageBox.Show("Please select an output directory.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!Directory.Exists(txtOutputPath.Text.Trim()))
            {
                var result = MessageBox.Show("The output directory does not exist. Create it?",
                    "Create Directory", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        Directory.CreateDirectory(txtOutputPath.Text.Trim());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to create output directory: {ex.Message}",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
    }
}