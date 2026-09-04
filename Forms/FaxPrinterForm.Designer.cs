namespace ScanView.Forms
{
    partial class FaxPrinterForm
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
            labelPrinter = new Label();
            comboPrinter = new ComboBox();
            labelHint = new Label();
            btnOk = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // labelPrinter
            // 
            labelPrinter.AutoSize = true;
            labelPrinter.Location = new Point(12, 15);
            labelPrinter.Name = "labelPrinter";
            labelPrinter.Size = new Size(243, 15);
            labelPrinter.TabIndex = 0;
            labelPrinter.Text = "Virtueller Fax&drucker (z.B. FRITZ!fax-Drucker):";
            // 
            // comboPrinter
            // 
            comboPrinter.DropDownStyle = ComboBoxStyle.DropDownList;
            comboPrinter.Location = new Point(12, 33);
            comboPrinter.Name = "comboPrinter";
            comboPrinter.Size = new Size(300, 23);
            comboPrinter.TabIndex = 1;
            // 
            // labelHint
            // 
            labelHint.AutoSize = true;
            labelHint.ForeColor = SystemColors.GrayText;
            labelHint.Location = new Point(12, 66);
            labelHint.Name = "labelHint";
            labelHint.Size = new Size(275, 30);
            labelHint.TabIndex = 2;
            labelHint.Text = "Wähle hier den Faxdrucker deines Faxprogramms.\r\nOhne Eintrag wird keine Schaltfläche eingeblendet.";
            // 
            // btnOk
            // 
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Location = new Point(146, 110);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(80, 26);
            btnOk.TabIndex = 3;
            btnOk.Text = "OK";
            btnOk.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(232, 110);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 26);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "Abbrechen";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // FaxPrinterForm
            // 
            AcceptButton = btnOk;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(324, 148);
            Controls.Add(labelPrinter);
            Controls.Add(comboPrinter);
            Controls.Add(labelHint);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FaxPrinterForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Faxprogramm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelPrinter;
        private System.Windows.Forms.ComboBox comboPrinter;
        private System.Windows.Forms.Label labelHint;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}
