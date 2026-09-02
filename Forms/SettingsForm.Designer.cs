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
            labelUiLanguage = new Label();
            comboUiLanguage = new ComboBox();
            cbCloseOnEscape = new CheckBox();
            labelExit = new Label();
            rbExitKeep = new RadioButton();
            rbExitAsk = new RadioButton();
            rbExitClear = new RadioButton();
            labelDirectory = new Label();
            textSaveDirectory = new TextBox();
            btnBrowse = new Button();
            labelDirectoryHint = new Label();
            labelBackColor = new Label();
            panelBackColors = new Panel();
            rbBackWhite = new RadioButton();
            rbBackBlue = new RadioButton();
            rbBackGreen = new RadioButton();
            rbBackYellow = new RadioButton();
            rbBackRose = new RadioButton();
            rbBackGray = new RadioButton();
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
            panelBackColors.SuspendLayout();
            tabOcr.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numJpgQuality).BeginInit();
            SuspendLayout();
            // 
            // tabs
            // 
            tabs.Controls.Add(tabGeneral);
            tabs.Controls.Add(tabOcr);
            tabs.Dock = DockStyle.Top;
            tabs.Location = new Point(0, 0);
            tabs.Name = "tabs";
            tabs.SelectedIndex = 0;
            tabs.Size = new Size(374, 338);
            tabs.TabIndex = 0;
            // 
            // tabGeneral
            // 
            tabGeneral.Controls.Add(labelUiLanguage);
            tabGeneral.Controls.Add(comboUiLanguage);
            tabGeneral.Controls.Add(cbCloseOnEscape);
            tabGeneral.Controls.Add(labelExit);
            tabGeneral.Controls.Add(rbExitKeep);
            tabGeneral.Controls.Add(rbExitAsk);
            tabGeneral.Controls.Add(rbExitClear);
            tabGeneral.Controls.Add(labelDirectory);
            tabGeneral.Controls.Add(textSaveDirectory);
            tabGeneral.Controls.Add(btnBrowse);
            tabGeneral.Controls.Add(labelDirectoryHint);
            tabGeneral.Controls.Add(labelBackColor);
            tabGeneral.Controls.Add(panelBackColors);
            tabGeneral.Location = new Point(4, 24);
            tabGeneral.Name = "tabGeneral";
            tabGeneral.Padding = new Padding(3);
            tabGeneral.Size = new Size(366, 310);
            tabGeneral.TabIndex = 0;
            tabGeneral.Text = "Allgemein";
            tabGeneral.UseVisualStyleBackColor = true;
            // 
            // labelUiLanguage
            // 
            labelUiLanguage.AutoSize = true;
            labelUiLanguage.Location = new Point(8, 19);
            labelUiLanguage.Name = "labelUiLanguage";
            labelUiLanguage.Size = new Size(115, 15);
            labelUiLanguage.TabIndex = 16;
            labelUiLanguage.Text = "Sprache / &Language:";
            // 
            // comboUiLanguage
            // 
            comboUiLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            comboUiLanguage.Items.AddRange(new object[] { "Deutsch", "English", "Français", "Español" });
            comboUiLanguage.Location = new Point(136, 16);
            comboUiLanguage.Name = "comboUiLanguage";
            comboUiLanguage.Size = new Size(222, 23);
            comboUiLanguage.TabIndex = 17;
            // 
            // cbCloseOnEscape
            // 
            cbCloseOnEscape.AutoSize = true;
            cbCloseOnEscape.Location = new Point(12, 56);
            cbCloseOnEscape.Name = "cbCloseOnEscape";
            cbCloseOnEscape.Size = new Size(313, 19);
            cbCloseOnEscape.TabIndex = 0;
            cbCloseOnEscape.Text = "Programm mit 2× &Esc beenden (Umschalt+Esc: sofort)";
            cbCloseOnEscape.UseVisualStyleBackColor = true;
            // 
            // labelExit
            // 
            labelExit.AutoSize = true;
            labelExit.Location = new Point(8, 88);
            labelExit.Name = "labelExit";
            labelExit.Size = new Size(172, 15);
            labelExit.TabIndex = 1;
            labelExit.Text = "Beim Beenden des Programms:";
            // 
            // rbExitKeep
            // 
            rbExitKeep.AutoSize = true;
            rbExitKeep.Location = new Point(28, 106);
            rbExitKeep.Name = "rbExitKeep";
            rbExitKeep.Size = new Size(223, 19);
            rbExitKeep.TabIndex = 2;
            rbExitKeep.Text = "Seiten in der Seitenübersicht &behalten";
            rbExitKeep.UseVisualStyleBackColor = true;
            // 
            // rbExitAsk
            // 
            rbExitAsk.AutoSize = true;
            rbExitAsk.Location = new Point(28, 130);
            rbExitAsk.Name = "rbExitAsk";
            rbExitAsk.Size = new Size(226, 19);
            rbExitAsk.TabIndex = 3;
            rbExitAsk.Text = "Seitenübersicht nach &Rückfrage leeren";
            rbExitAsk.UseVisualStyleBackColor = true;
            // 
            // rbExitClear
            // 
            rbExitClear.AutoSize = true;
            rbExitClear.Location = new Point(28, 154);
            rbExitClear.Name = "rbExitClear";
            rbExitClear.Size = new Size(227, 19);
            rbExitClear.TabIndex = 4;
            rbExitClear.Text = "Seitenübersicht &ohne Rückfrage leeren";
            rbExitClear.UseVisualStyleBackColor = true;
            // 
            // labelDirectory
            // 
            labelDirectory.AutoSize = true;
            labelDirectory.Location = new Point(8, 190);
            labelDirectory.Name = "labelDirectory";
            labelDirectory.Size = new Size(223, 15);
            labelDirectory.TabIndex = 5;
            labelDirectory.Text = "Bevorzugter &Speicherort für PDF-Dateien:";
            // 
            // textSaveDirectory
            // 
            textSaveDirectory.Location = new Point(28, 208);
            textSaveDirectory.Name = "textSaveDirectory";
            textSaveDirectory.Size = new Size(292, 23);
            textSaveDirectory.TabIndex = 6;
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(326, 208);
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
            labelDirectoryHint.Location = new Point(28, 236);
            labelDirectoryHint.Name = "labelDirectoryHint";
            labelDirectoryHint.Size = new Size(319, 15);
            labelDirectoryHint.TabIndex = 8;
            labelDirectoryHint.Text = "Leer: Windows schlägt den zuletzt verwendeten Ordner vor.";
            // 
            // labelBackColor
            // 
            labelBackColor.AutoSize = true;
            labelBackColor.Location = new Point(16, 270);
            labelBackColor.Name = "labelBackColor";
            labelBackColor.Size = new Size(102, 15);
            labelBackColor.TabIndex = 9;
            labelBackColor.Text = "&Hintergrundfarbe:";
            // 
            // panelBackColors
            // 
            panelBackColors.Controls.Add(rbBackWhite);
            panelBackColors.Controls.Add(rbBackBlue);
            panelBackColors.Controls.Add(rbBackGreen);
            panelBackColors.Controls.Add(rbBackYellow);
            panelBackColors.Controls.Add(rbBackRose);
            panelBackColors.Controls.Add(rbBackGray);
            panelBackColors.Location = new Point(136, 264);
            panelBackColors.Name = "panelBackColors";
            panelBackColors.Size = new Size(226, 28);
            panelBackColors.TabIndex = 10;
            // 
            // rbBackWhite
            // 
            rbBackWhite.Appearance = Appearance.Button;
            rbBackWhite.BackColor = Color.White;
            rbBackWhite.FlatStyle = FlatStyle.Flat;
            rbBackWhite.Location = new Point(0, 0);
            rbBackWhite.Name = "rbBackWhite";
            rbBackWhite.Size = new Size(32, 28);
            rbBackWhite.TabIndex = 10;
            rbBackWhite.UseVisualStyleBackColor = false;
            rbBackWhite.CheckedChanged += BackColorRadio_CheckedChanged;
            // 
            // rbBackBlue
            // 
            rbBackBlue.Appearance = Appearance.Button;
            rbBackBlue.BackColor = Color.FromArgb(214, 230, 245);
            rbBackBlue.FlatStyle = FlatStyle.Flat;
            rbBackBlue.Location = new Point(38, 0);
            rbBackBlue.Name = "rbBackBlue";
            rbBackBlue.Size = new Size(32, 28);
            rbBackBlue.TabIndex = 11;
            rbBackBlue.UseVisualStyleBackColor = false;
            rbBackBlue.CheckedChanged += BackColorRadio_CheckedChanged;
            // 
            // rbBackGreen
            // 
            rbBackGreen.Appearance = Appearance.Button;
            rbBackGreen.BackColor = Color.FromArgb(220, 238, 220);
            rbBackGreen.FlatStyle = FlatStyle.Flat;
            rbBackGreen.Location = new Point(76, 0);
            rbBackGreen.Name = "rbBackGreen";
            rbBackGreen.Size = new Size(32, 28);
            rbBackGreen.TabIndex = 12;
            rbBackGreen.UseVisualStyleBackColor = false;
            rbBackGreen.CheckedChanged += BackColorRadio_CheckedChanged;
            // 
            // rbBackYellow
            // 
            rbBackYellow.Appearance = Appearance.Button;
            rbBackYellow.BackColor = Color.FromArgb(247, 243, 216);
            rbBackYellow.FlatStyle = FlatStyle.Flat;
            rbBackYellow.Location = new Point(114, 0);
            rbBackYellow.Name = "rbBackYellow";
            rbBackYellow.Size = new Size(32, 28);
            rbBackYellow.TabIndex = 13;
            rbBackYellow.UseVisualStyleBackColor = false;
            rbBackYellow.CheckedChanged += BackColorRadio_CheckedChanged;
            // 
            // rbBackRose
            // 
            rbBackRose.Appearance = Appearance.Button;
            rbBackRose.BackColor = Color.FromArgb(246, 224, 230);
            rbBackRose.FlatStyle = FlatStyle.Flat;
            rbBackRose.Location = new Point(152, 0);
            rbBackRose.Name = "rbBackRose";
            rbBackRose.Size = new Size(32, 28);
            rbBackRose.TabIndex = 14;
            rbBackRose.UseVisualStyleBackColor = false;
            rbBackRose.CheckedChanged += BackColorRadio_CheckedChanged;
            // 
            // rbBackGray
            // 
            rbBackGray.Appearance = Appearance.Button;
            rbBackGray.BackColor = Color.FromArgb(232, 232, 232);
            rbBackGray.FlatStyle = FlatStyle.Flat;
            rbBackGray.Location = new Point(190, 0);
            rbBackGray.Name = "rbBackGray";
            rbBackGray.Size = new Size(32, 28);
            rbBackGray.TabIndex = 15;
            rbBackGray.UseVisualStyleBackColor = false;
            rbBackGray.CheckedChanged += BackColorRadio_CheckedChanged;
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
            tabOcr.Size = new Size(366, 310);
            tabOcr.TabIndex = 1;
            tabOcr.Text = "Texterkennung";
            tabOcr.UseVisualStyleBackColor = true;
            // 
            // labelLanguage
            // 
            labelLanguage.AutoSize = true;
            labelLanguage.Location = new Point(8, 19);
            labelLanguage.Name = "labelLanguage";
            labelLanguage.Size = new Size(270, 15);
            labelLanguage.TabIndex = 0;
            labelLanguage.Text = "Bevorzugte &Sprache (Vorgabe für neue Sitzungen):";
            // 
            // comboLanguage
            // 
            comboLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            comboLanguage.Location = new Point(28, 37);
            comboLanguage.Name = "comboLanguage";
            comboLanguage.Size = new Size(220, 23);
            comboLanguage.TabIndex = 1;
            // 
            // labelLanguageHint
            // 
            labelLanguageHint.AutoSize = true;
            labelLanguageHint.ForeColor = SystemColors.GrayText;
            labelLanguageHint.Location = new Point(28, 63);
            labelLanguageHint.Name = "labelLanguageHint";
            labelLanguageHint.Size = new Size(313, 30);
            labelLanguageHint.TabIndex = 2;
            labelLanguageHint.Text = "Weitere Sprachen: .traineddata-Dateien (tessdata_best)\nin den Ordner \"tessdata\" neben der Programmdatei legen.";
            // 
            // labelQuality
            // 
            labelQuality.AutoSize = true;
            labelQuality.Location = new Point(8, 124);
            labelQuality.Name = "labelQuality";
            labelQuality.Size = new Size(239, 15);
            labelQuality.TabIndex = 3;
            labelQuality.Text = "&JPEG-Qualität der Bilder in der PDF (30–100):";
            // 
            // numJpgQuality
            // 
            numJpgQuality.Location = new Point(28, 142);
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
            labelQualityHint.Location = new Point(28, 168);
            labelQualityHint.Name = "labelQualityHint";
            labelQualityHint.Size = new Size(299, 30);
            labelQualityHint.TabIndex = 5;
            labelQualityHint.Text = "Kleinere Werte ergeben kleinere Dateien; 75 ist ein guter\nKompromiss. Graustufen-Scans sparen zusätzlich Platz.";
            // 
            // btnOk
            // 
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Location = new Point(196, 340);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(80, 26);
            btnOk.TabIndex = 1;
            btnOk.Text = "OK";
            btnOk.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(282, 340);
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
            ClientSize = new Size(374, 378);
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
            panelBackColors.ResumeLayout(false);
            tabOcr.ResumeLayout(false);
            tabOcr.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numJpgQuality).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.Label labelUiLanguage;
        private System.Windows.Forms.ComboBox comboUiLanguage;
        private System.Windows.Forms.CheckBox cbCloseOnEscape;
        private System.Windows.Forms.Label labelExit;
        private System.Windows.Forms.RadioButton rbExitKeep;
        private System.Windows.Forms.RadioButton rbExitAsk;
        private System.Windows.Forms.RadioButton rbExitClear;
        private System.Windows.Forms.Label labelDirectory;
        private System.Windows.Forms.TextBox textSaveDirectory;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label labelDirectoryHint;
        private System.Windows.Forms.Label labelBackColor;
        private System.Windows.Forms.Panel panelBackColors;
        private System.Windows.Forms.RadioButton rbBackWhite;
        private System.Windows.Forms.RadioButton rbBackBlue;
        private System.Windows.Forms.RadioButton rbBackGreen;
        private System.Windows.Forms.RadioButton rbBackYellow;
        private System.Windows.Forms.RadioButton rbBackRose;
        private System.Windows.Forms.RadioButton rbBackGray;
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
