namespace ScanView.Forms
{
    partial class ScannerForm
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
            labelDevice = new Label();
            comboScanner = new ComboBox();
            btnDeviceKeys = new Button();
            labelHint = new Label();
            btnOk = new Button();
            btnCancel = new Button();
            SuspendLayout();
            //
            // labelDevice
            //
            labelDevice.AutoSize = true;
            labelDevice.Location = new Point(16, 16);
            labelDevice.Name = "labelDevice";
            labelDevice.Size = new Size(145, 15);
            labelDevice.TabIndex = 0;
            labelDevice.Text = "Dieses &Gerät verwenden:";
            //
            // comboScanner
            //
            comboScanner.DropDownStyle = ComboBoxStyle.DropDownList;
            comboScanner.Location = new Point(16, 36);
            comboScanner.Name = "comboScanner";
            comboScanner.Size = new Size(398, 23);
            comboScanner.TabIndex = 1;
            //
            // btnDeviceKeys
            //
            btnDeviceKeys.Location = new Point(16, 80);
            btnDeviceKeys.Name = "btnDeviceKeys";
            btnDeviceKeys.Size = new Size(220, 26);
            btnDeviceKeys.TabIndex = 2;
            btnDeviceKeys.Text = "Geräte&tasten konfigurieren …";
            btnDeviceKeys.UseVisualStyleBackColor = true;
            btnDeviceKeys.Click += BtnDeviceKeys_Click;
            //
            // labelHint
            //
            labelHint.AutoSize = true;
            labelHint.ForeColor = SystemColors.GrayText;
            labelHint.Location = new Point(16, 114);
            labelHint.Name = "labelHint";
            labelHint.Size = new Size(380, 45);
            labelHint.TabIndex = 3;
            labelHint.Text = "Öffnet die Windows-Einstellungen „Scanner und Kameras\".\nDort lassen sich die Tasten am Gerät mit Programmen verknüpfen\n(Eigenschaften → Ereignisse).";
            //
            // btnOk
            //
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Location = new Point(252, 170);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(80, 26);
            btnOk.TabIndex = 4;
            btnOk.Text = "OK";
            btnOk.UseVisualStyleBackColor = true;
            //
            // btnCancel
            //
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(338, 170);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 26);
            btnCancel.TabIndex = 5;
            btnCancel.Text = "Abbrechen";
            btnCancel.UseVisualStyleBackColor = true;
            //
            // ScannerForm
            //
            AcceptButton = btnOk;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(430, 208);
            Controls.Add(labelDevice);
            Controls.Add(comboScanner);
            Controls.Add(btnDeviceKeys);
            Controls.Add(labelHint);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ScannerForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Scanner wählen";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelDevice;
        private System.Windows.Forms.ComboBox comboScanner;
        private System.Windows.Forms.Button btnDeviceKeys;
        private System.Windows.Forms.Label labelHint;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}
