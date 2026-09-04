namespace ScanView.Controls
{
    partial class JpegQualityControl
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            trackQuality = new TrackBar();
            labelValue = new Label();
            ((System.ComponentModel.ISupportInitialize)trackQuality).BeginInit();
            SuspendLayout();
            //
            // trackQuality
            //
            trackQuality.AutoSize = false;
            trackQuality.LargeChange = 2;
            trackQuality.Location = new Point(0, 0);
            trackQuality.Maximum = 20;
            trackQuality.Minimum = 6;
            trackQuality.Name = "trackQuality";
            trackQuality.Size = new Size(208, 23);
            trackQuality.TabIndex = 0;
            trackQuality.Value = 15;
            trackQuality.ValueChanged += TrackQuality_ValueChanged;
            //
            // labelValue
            //
            labelValue.Location = new Point(214, 4);
            labelValue.Name = "labelValue";
            labelValue.Size = new Size(30, 15);
            labelValue.TabIndex = 1;
            labelValue.Text = "75";
            labelValue.TextAlign = ContentAlignment.MiddleRight;
            //
            // JpegQualityControl
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(trackQuality);
            Controls.Add(labelValue);
            Name = "JpegQualityControl";
            Size = new Size(244, 23);
            ((System.ComponentModel.ISupportInitialize)trackQuality).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TrackBar trackQuality;
        private System.Windows.Forms.Label labelValue;
    }
}
