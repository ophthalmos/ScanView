namespace ScanView.Forms
{
    partial class SettingsForm
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
            tabs = new TabControl();
            tabGeneral = new TabPage();
            cbCloseOnEscape = new CheckBox();
            labelExit = new Label();
            rbExitKeep = new RadioButton();
            rbExitAsk = new RadioButton();
            rbExitClear = new RadioButton();
            labelDirectory = new Label();
            textSaveDirectory = new TextBox();
            btnBrowse = new Button();
            labelDirectoryHint = new Label();
            tabOcr = new TabPage();
            labelLanguage = new Label();
            comboLanguage = new ComboBox();
            labelLanguageHint = new Label();
            labelQuality = new Label();
            numJpgQuality = new NumericUpDown();
            labelQualityHint = new Label();
            btnOk = new Button();
            btnCancel = new Button();
            tabs.SuspendLayout();
            tabGeneral.SuspendLayout();
            tabOcr.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numJpgQuality).BeginInit();
            SuspendLayout();
            //
            // tabs
            //
            tabs.Controls.Add(tabGeneral);
            tabs.Controls.Add(tabOcr);
            tabs.Location = new Point(12, 12);
            tabs.Name = "tabs";
            tabs.SelectedIndex = 0;
            tabs.Size = new Size(416, 254);
            tabs.TabIndex = 0;
            //
            // tabGeneral
            //
            tabGeneral.Controls.Add(cbCloseOnEscape);
            tabGeneral.Controls.Add(labelExit);
            tabGeneral.Controls.Add(rbExitKeep);
            tabGeneral.Controls.Add(rbExitAsk);
            tabGeneral.Controls.Add(rbExitClear);
            tabGeneral.Controls.Add(labelDirectory);
            tabGeneral.Controls.Add(textSaveDirectory);
            tabGeneral.Controls.Add(btnBrowse);
            tabGeneral.Controls.Add(labelDirectoryHint);
            tabGeneral.Location = new Point(4, 24);
            tabGeneral.Name = "tabGeneral";
            tabGeneral.Padding = new Padding(3);
            tabGeneral.Size = new Size(408, 226);
            tabGeneral.TabIndex = 0;
            tabGeneral.Text = "Allgemein";
            tabGeneral.UseVisualStyleBackColor = true;
            //
            // cbCloseOnEscape
            //
            cbCloseOnEscape.AutoSize = true;
            cbCloseOnEscape.Location = new Point(16, 20);
            cbCloseOnEscape.Name = "cbCloseOnEscape";
            cbCloseOnEscape.Size = new Size(320, 19);
            cbCloseOnEscape.TabIndex = 0;
            cbCloseOnEscape.Text = "Programm mit 2× &Esc beenden (Umschalt+Esc: sofort)";
            cbCloseOnEscape.UseVisualStyleBackColor = true;
            //
            // labelExit
            //
            labelExit.AutoSize = true;
            labelExit.Location = new Point(16, 54);
            labelExit.Name = "labelExit";
            labelExit.Size = new Size(180, 15);
            labelExit.TabIndex = 1;
            labelExit.Text = "Beim Beenden des Programms:";
            //
            // rbExitKeep
            //
            rbExitKeep.AutoSize = true;
            rbExitKeep.Location = new Point(28, 74);
            rbExitKeep.Name = "rbExitKeep";
            rbExitKeep.Size = new Size(240, 19);
            rbExitKeep.TabIndex = 2;
            rbExitKeep.Text = "Seiten in der Seitenübersicht &behalten";
            rbExitKeep.UseVisualStyleBackColor = true;
            //
            // rbExitAsk
            //
            rbExitAsk.AutoSize = true;
            rbExitAsk.Location = new Point(28, 98);
            rbExitAsk.Name = "rbExitAsk";
            rbExitAsk.Size = new Size(240, 19);
            rbExitAsk.TabIndex = 3;
            rbExitAsk.Text = "Seitenübersicht nach &Rückfrage leeren";
            rbExitAsk.UseVisualStyleBackColor = true;
            //
            // rbExitClear
            //
            rbExitClear.AutoSize = true;
            rbExitClear.Location = new Point(28, 122);
            rbExitClear.Name = "rbExitClear";
            rbExitClear.Size = new Size(240, 19);
            rbExitClear.TabIndex = 4;
            rbExitClear.Text = "Seitenübersicht &ohne Rückfrage leeren";
            rbExitClear.UseVisualStyleBackColor = true;
            //
            // labelDirectory
            //
            labelDirectory.AutoSize = true;
            labelDirectory.Location = new Point(16, 156);
            labelDirectory.Name = "labelDirectory";
            labelDirectory.Size = new Size(240, 15);
            labelDirectory.TabIndex = 5;
            labelDirectory.Text = "Bevorzugter &Speicherort für PDF-Dateien:";
            //
            // textSaveDirectory
            //
            textSaveDirectory.Location = new Point(16, 176);
            textSaveDirectory.Name = "textSaveDirectory";
            textSaveDirectory.Size = new Size(330, 23);
            textSaveDirectory.TabIndex = 6;
            //
            // btnBrowse
            //
            btnBrowse.Location = new Point(352, 175);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(32, 25);
            btnBrowse.TabIndex = 7;
            btnBrowse.Text = "…";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += BtnBrowse_Click;
            //
            // labelDirectoryHint
            //
            labelDirectoryHint.AutoSize = true;
            labelDirectoryHint.ForeColor = SystemColors.GrayText;
            labelDirectoryHint.Location = new Point(16, 204);
            labelDirectoryHint.Name = "labelDirectoryHint";
            labelDirectoryHint.Size = new Size(320, 15);
            labelDirectoryHint.TabIndex = 8;
            labelDirectoryHint.Text = "Leer: Windows schlägt den zuletzt verwendeten Ordner vor.";
            //
            // tabOcr
            //
            tabOcr.Controls.Add(labelLanguage);
            tabOcr.Controls.Add(comboLanguage);
            tabOcr.Controls.Add(labelLanguageHint);
            tabOcr.Controls.Add(labelQuality);
            tabOcr.Controls.Add(numJpgQuality);
            tabOcr.Controls.Add(labelQualityHint);
            tabOcr.Location = new Point(4, 24);
            tabOcr.Name = "tabOcr";
            tabOcr.Padding = new Padding(3);
            tabOcr.Size = new Size(408, 226);
            tabOcr.TabIndex = 1;
            tabOcr.Text = "Texterkennung";
            tabOcr.UseVisualStyleBackColor = true;
            //
            // labelLanguage
            //
            labelLanguage.AutoSize = true;
            labelLanguage.Location = new Point(16, 20);
            labelLanguage.Name = "labelLanguage";
            labelLanguage.Size = new Size(380, 15);
            labelLanguage.TabIndex = 0;
            labelLanguage.Text = "Bevorzugte &Sprache der Texterkennung (Vorgabe für neue Sitzungen):";
            //
            // comboLanguage
            //
            comboLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            comboLanguage.Location = new Point(16, 40);
            comboLanguage.Name = "comboLanguage";
            comboLanguage.Size = new Size(220, 23);
            comboLanguage.TabIndex = 1;
            //
            // labelLanguageHint
            //
            labelLanguageHint.AutoSize = true;
            labelLanguageHint.ForeColor = SystemColors.GrayText;
            labelLanguageHint.Location = new Point(16, 72);
            labelLanguageHint.Name = "labelLanguageHint";
            labelLanguageHint.Size = new Size(330, 30);
            labelLanguageHint.TabIndex = 2;
            labelLanguageHint.Text = "Weitere Sprachen: .traineddata-Dateien (tessdata_best)\nin den Ordner \"tessdata\" neben der Programmdatei legen.";
            //
            // labelQuality
            //
            labelQuality.AutoSize = true;
            labelQuality.Location = new Point(16, 122);
            labelQuality.Name = "labelQuality";
            labelQuality.Size = new Size(260, 15);
            labelQuality.TabIndex = 3;
            labelQuality.Text = "&JPEG-Qualität der Bilder in der PDF (30–100):";
            //
            // numJpgQuality
            //
            numJpgQuality.Location = new Point(16, 142);
            numJpgQuality.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numJpgQuality.Minimum = new decimal(new int[] { 30, 0, 0, 0 });
            numJpgQuality.Name = "numJpgQuality";
            numJpgQuality.Size = new Size(60, 23);
            numJpgQuality.TabIndex = 4;
            numJpgQuality.Value = new decimal(new int[] { 75, 0, 0, 0 });
            //
            // labelQualityHint
            //
            labelQualityHint.AutoSize = true;
            labelQualityHint.ForeColor = SystemColors.GrayText;
            labelQualityHint.Location = new Point(16, 174);
            labelQualityHint.Name = "labelQualityHint";
            labelQualityHint.Size = new Size(330, 30);
            labelQualityHint.TabIndex = 5;
            labelQualityHint.Text = "Kleinere Werte ergeben kleinere Dateien; 75 ist ein guter\nKompromiss. Graustufen-Scans sparen zusätzlich Platz.";
            //
            // btnOk
            //
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Location = new Point(262, 280);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(80, 26);
            btnOk.TabIndex = 1;
            btnOk.Text = "OK";
            btnOk.UseVisualStyleBackColor = true;
            //
            // btnCancel
            //
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(348, 280);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 26);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Abbrechen";
            btnCancel.UseVisualStyleBackColor = true;
            //
            // SettingsForm
            //
            AcceptButton = btnOk;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(440, 318);
            Controls.Add(tabs);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Einstellungen";
            tabs.ResumeLayout(false);
            tabGeneral.ResumeLayout(false);
            tabGeneral.PerformLayout();
            tabOcr.ResumeLayout(false);
            tabOcr.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numJpgQuality).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.CheckBox cbCloseOnEscape;
        private System.Windows.Forms.Label labelExit;
        private System.Windows.Forms.RadioButton rbExitKeep;
        private System.Windows.Forms.RadioButton rbExitAsk;
        private System.Windows.Forms.RadioButton rbExitClear;
        private System.Windows.Forms.Label labelDirectory;
        private System.Windows.Forms.TextBox textSaveDirectory;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label labelDirectoryHint;
        private System.Windows.Forms.TabPage tabOcr;
        private System.Windows.Forms.Label labelLanguage;
        private System.Windows.Forms.ComboBox comboLanguage;
        private System.Windows.Forms.Label labelLanguageHint;
        private System.Windows.Forms.Label labelQuality;
        private System.Windows.Forms.NumericUpDown numJpgQuality;
        private System.Windows.Forms.Label labelQualityHint;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}
