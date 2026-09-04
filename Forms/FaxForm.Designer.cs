namespace ScanView.Forms
{
    partial class FaxForm
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
            radioAll = new RadioButton();
            radioSelected = new RadioButton();
            btnFax = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // radioAll
            // 
            radioAll.AutoSize = true;
            radioAll.Checked = true;
            radioAll.Location = new Point(14, 12);
            radioAll.Name = "radioAll";
            radioAll.Size = new Size(80, 19);
            radioAll.TabIndex = 0;
            radioAll.TabStop = true;
            radioAll.Text = "&Alle Seiten";
            radioAll.UseVisualStyleBackColor = true;
            // 
            // radioSelected
            // 
            radioSelected.AutoSize = true;
            radioSelected.Location = new Point(14, 34);
            radioSelected.Name = "radioSelected";
            radioSelected.Size = new Size(126, 19);
            radioSelected.TabIndex = 1;
            radioSelected.Text = "Nur &markierte Seite";
            radioSelected.UseVisualStyleBackColor = true;
            // 
            // btnFax
            // 
            btnFax.DialogResult = DialogResult.OK;
            btnFax.Location = new Point(12, 73);
            btnFax.Name = "btnFax";
            btnFax.Size = new Size(80, 26);
            btnFax.TabIndex = 3;
            btnFax.Text = "&Faxen";
            btnFax.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(98, 73);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 26);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "Abbrechen";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // FaxForm
            // 
            AcceptButton = btnFax;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(190, 111);
            Controls.Add(radioAll);
            Controls.Add(radioSelected);
            Controls.Add(btnFax);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FaxForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Faxen";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.RadioButton radioAll;
        private System.Windows.Forms.RadioButton radioSelected;
        private System.Windows.Forms.Button btnFax;
        private System.Windows.Forms.Button btnCancel;
    }
}
