namespace ScanView.Forms
{
    partial class ProgressForm
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
            labelStatus = new Label();
            progressBar = new ProgressBar();
            SuspendLayout();
            //
            // labelStatus
            //
            labelStatus.Location = new Point(12, 12);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(296, 16);
            labelStatus.TabIndex = 0;
            //
            // progressBar
            //
            progressBar.Location = new Point(12, 34);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(296, 20);
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.TabIndex = 1;
            //
            // ProgressForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(320, 68);
            ControlBox = false;
            Controls.Add(labelStatus);
            Controls.Add(progressBar);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ProgressForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.ProgressBar progressBar;
    }
}
