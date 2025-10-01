namespace EveryFileExplorer.UI
{
    partial class EFESCExecute
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnBrowseScript = new System.Windows.Forms.Button();
            this.btnBrowseInput = new System.Windows.Forms.Button();
            this.btnBrowseOutput = new System.Windows.Forms.Button();
            this.btnExecute = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtScriptPath = new System.Windows.Forms.TextBox();
            this.txtInputPath = new System.Windows.Forms.TextBox();
            this.txtOutputPath = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnBrowseScript
            // 
            this.btnBrowseScript.Location = new System.Drawing.Point(504, 32);
            this.btnBrowseScript.Name = "btnBrowseScript";
            this.btnBrowseScript.Size = new System.Drawing.Size(32, 21);
            this.btnBrowseScript.TabIndex = 0;
            this.btnBrowseScript.Text = "...";
            this.btnBrowseScript.UseVisualStyleBackColor = true;
            this.btnBrowseScript.Click += new System.EventHandler(this.btnBrowseScript_Click);
            // 
            // btnBrowseInput
            // 
            this.btnBrowseInput.Location = new System.Drawing.Point(504, 90);
            this.btnBrowseInput.Name = "btnBrowseInput";
            this.btnBrowseInput.Size = new System.Drawing.Size(32, 22);
            this.btnBrowseInput.TabIndex = 1;
            this.btnBrowseInput.Text = "...";
            this.btnBrowseInput.UseVisualStyleBackColor = true;
            this.btnBrowseInput.Click += new System.EventHandler(this.btnBrowseInput_Click);
            // 
            // btnBrowseOutput
            // 
            this.btnBrowseOutput.Location = new System.Drawing.Point(504, 151);
            this.btnBrowseOutput.Name = "btnBrowseOutput";
            this.btnBrowseOutput.Size = new System.Drawing.Size(32, 24);
            this.btnBrowseOutput.TabIndex = 2;
            this.btnBrowseOutput.Text = "...";
            this.btnBrowseOutput.UseVisualStyleBackColor = true;
            this.btnBrowseOutput.Click += new System.EventHandler(this.btnBrowseOutput_Click);
            // 
            // btnExecute
            // 
            this.btnExecute.Location = new System.Drawing.Point(79, 221);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(110, 46);
            this.btnExecute.TabIndex = 3;
            this.btnExecute.Text = "Execute";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(327, 221);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(108, 46);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(125, 12);
            this.label1.TabIndex = 5;
            this.label1.Text = "Select efesc script:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 95);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(143, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "Select input directory:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 154);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(149, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "Select output directory:";
            // 
            // txtScriptPath
            // 
            this.txtScriptPath.Location = new System.Drawing.Point(143, 32);
            this.txtScriptPath.Name = "txtScriptPath";
            this.txtScriptPath.Size = new System.Drawing.Size(349, 21);
            this.txtScriptPath.TabIndex = 8;
            // 
            // txtInputPath
            // 
            this.txtInputPath.Location = new System.Drawing.Point(161, 90);
            this.txtInputPath.Name = "txtInputPath";
            this.txtInputPath.Size = new System.Drawing.Size(331, 21);
            this.txtInputPath.TabIndex = 9;
            // 
            // txtOutputPath
            // 
            this.txtOutputPath.Location = new System.Drawing.Point(167, 151);
            this.txtOutputPath.Name = "txtOutputPath";
            this.txtOutputPath.Size = new System.Drawing.Size(325, 21);
            this.txtOutputPath.TabIndex = 10;
            // 
            // EFESCExecute
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(552, 289);
            this.Controls.Add(this.txtOutputPath);
            this.Controls.Add(this.txtInputPath);
            this.Controls.Add(this.txtScriptPath);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnExecute);
            this.Controls.Add(this.btnBrowseOutput);
            this.Controls.Add(this.btnBrowseInput);
            this.Controls.Add(this.btnBrowseScript);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EFESCExecute";
            this.Text = "EFE Batch Script Execute";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnBrowseScript;
        private System.Windows.Forms.Button btnBrowseInput;
        private System.Windows.Forms.Button btnBrowseOutput;
        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtScriptPath;
        private System.Windows.Forms.TextBox txtInputPath;
        private System.Windows.Forms.TextBox txtOutputPath;
    }
}