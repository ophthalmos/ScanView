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
            groupBox = new GroupBox();
            groupBox.SuspendLayout();
            SuspendLayout();
            // 
            // labelDevice
            // 
            labelDevice.AutoSize = true;
            labelDevice.Location = new Point(12, 9);
            labelDevice.Name = "labelDevice";
            labelDevice.Size = new Size(135, 15);
            labelDevice.TabIndex = 0;
            labelDevice.Text = "Dieses &Gerät verwenden:";
            // 
            // comboScanner
            // 
            comboScanner.DropDownStyle = ComboBoxStyle.DropDownList;
            comboScanner.Location = new Point(12, 27);
            comboScanner.Name = "comboScanner";
            comboScanner.Size = new Size(300, 23);
            comboScanner.TabIndex = 1;
            // 
            // btnDeviceKeys
            // 
            btnDeviceKeys.Location = new Point(6, 22);
            btnDeviceKeys.Name = "btnDeviceKeys";
            btnDeviceKeys.Size = new Size(220, 26);
            btnDeviceKeys.TabIndex = 2;
            btnDeviceKeys.Text = " Scanner und Kameras";
            btnDeviceKeys.UseVisualStyleBackColor = true;
            btnDeviceKeys.Click += BtnDeviceKeys_Click;
            // 
            // labelHint
            // 
            labelHint.AutoSize = true;
            labelHint.ForeColor = SystemColors.GrayText;
            labelHint.Location = new Point(6, 51);
            labelHint.Name = "labelHint";
            labelHint.Size = new Size(270, 30);
            labelHint.TabIndex = 3;
            labelHint.Text = "Dort lassen sich die Tasten am Gerät mit ScanView\r\nverknüpfen: (gehe zu Eigenschaften → Ereignisse).";
            // 
            // btnOk
            // 
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Location = new Point(146, 152);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(80, 26);
            btnOk.TabIndex = 4;
            btnOk.Text = "OK";
            btnOk.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(232, 152);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 26);
            btnCancel.TabIndex = 5;
            btnCancel.Text = "Abbrechen";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // groupBox
            // 
            groupBox.Controls.Add(btnDeviceKeys);
            groupBox.Controls.Add(labelHint);
            groupBox.Location = new Point(12, 56);
            groupBox.Name = "groupBox";
            groupBox.Size = new Size(300, 90);
            groupBox.TabIndex = 6;
            groupBox.TabStop = false;
            groupBox.Text = "Windows-Einstellungen";
            // 
            // ScannerForm
            // 
            AcceptButton = btnOk;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(324, 190);
            Controls.Add(labelDevice);
            Controls.Add(comboScanner);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
            Controls.Add(groupBox);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ScannerForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Scanner wählen";
            groupBox.ResumeLayout(false);
            groupBox.PerformLayout();
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
        private GroupBox groupBox;
    }
}
