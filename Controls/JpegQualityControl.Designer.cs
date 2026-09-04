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
            textBoxQuality = new TextBox();
            comboQuality = new ComboBox();
            labelLowSize = new Label();
            labelLargeSize = new Label();
            trackQuality = new TrackBar();
            ((System.ComponentModel.ISupportInitialize)trackQuality).BeginInit();
            SuspendLayout();
            //
            // textBoxQuality
            //
            textBoxQuality.Location = new Point(0, 0);
            textBoxQuality.MaxLength = 2;
            textBoxQuality.Name = "textBoxQuality";
            textBoxQuality.Size = new Size(77, 23);
            textBoxQuality.TabIndex = 0;
            textBoxQuality.TextChanged += TextBoxQuality_TextChanged;
            textBoxQuality.KeyPress += TextBoxQuality_KeyPress;
            textBoxQuality.Leave += TextBoxQuality_Leave;
            //
            // comboQuality
            //
            comboQuality.DropDownStyle = ComboBoxStyle.DropDownList;
            comboQuality.Items.AddRange(new object[] { "Niedrig", "Mittel", "Hoch" });
            comboQuality.Location = new Point(90, 0);
            comboQuality.Name = "comboQuality";
            comboQuality.Size = new Size(154, 23);
            comboQuality.TabIndex = 1;
            comboQuality.SelectedIndexChanged += ComboQuality_SelectedIndexChanged;
            //
            // labelLowSize
            //
            labelLowSize.AutoSize = true;
            labelLowSize.Font = new Font("Segoe UI", 8F);
            labelLowSize.Location = new Point(0, 26);
            labelLowSize.Name = "labelLowSize";
            labelLowSize.Size = new Size(64, 13);
            labelLowSize.TabIndex = 2;
            labelLowSize.Text = "Kleine Datei";
            //
            // labelLargeSize
            //
            labelLargeSize.AutoSize = true;
            labelLargeSize.Font = new Font("Segoe UI", 8F);
            labelLargeSize.Location = new Point(176, 26);
            labelLargeSize.Name = "labelLargeSize";
            labelLargeSize.Size = new Size(64, 13);
            labelLargeSize.TabIndex = 3;
            labelLargeSize.Text = "Große Datei";
            //
            // trackQuality
            //
            trackQuality.AutoSize = false;
            trackQuality.LargeChange = 2;
            trackQuality.Location = new Point(0, 40);
            trackQuality.Maximum = 12;
            trackQuality.Name = "trackQuality";
            trackQuality.Size = new Size(244, 23);
            trackQuality.TabIndex = 4;
            trackQuality.Value = 8;
            trackQuality.ValueChanged += TrackQuality_ValueChanged;
            //
            // JpegQualityControl
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(textBoxQuality);
            Controls.Add(comboQuality);
            Controls.Add(labelLowSize);
            Controls.Add(labelLargeSize);
            Controls.Add(trackQuality);
            Name = "JpegQualityControl";
            Size = new Size(244, 63);
            ((System.ComponentModel.ISupportInitialize)trackQuality).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox textBoxQuality;
        private System.Windows.Forms.ComboBox comboQuality;
        private System.Windows.Forms.Label labelLowSize;
        private System.Windows.Forms.Label labelLargeSize;
        private System.Windows.Forms.TrackBar trackQuality;
    }
}
