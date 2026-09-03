namespace ScanView.Forms
{
    partial class SaveForm
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
            labelPages = new Label();
            radioAll = new RadioButton();
            radioSelected = new RadioButton();
            labelFileName = new Label();
            textFileName = new TextBox();
            labelFolder = new Label();
            textFolder = new TextBox();
            btnBrowse = new Button();
            labelFileType = new Label();
            comboFileType = new ComboBox();
            labelOcr = new Label();
            comboOcr = new ComboBox();
            labelQuality = new Label();
            numJpgQuality = new NumericUpDown();
            labelMeta = new Label();
            labelTitle = new Label();
            textTitle = new TextBox();
            labelSubject = new Label();
            textSubject = new TextBox();
            labelKeywords = new Label();
            textKeywords = new TextBox();
            labelAuthor = new Label();
            textAuthor = new TextBox();
            btnSave = new Button();
            btnCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)numJpgQuality).BeginInit();
            SuspendLayout();
            //
            // labelPages
            //
            labelPages.AutoSize = true;
            labelPages.Location = new Point(12, 15);
            labelPages.Name = "labelPages";
            labelPages.Size = new Size(63, 15);
            labelPages.TabIndex = 0;
            labelPages.Text = "Speichern:";
            //
            // radioAll
            //
            radioAll.AutoSize = true;
            radioAll.Checked = true;
            radioAll.Location = new Point(140, 13);
            radioAll.Name = "radioAll";
            radioAll.Size = new Size(85, 19);
            radioAll.TabIndex = 1;
            radioAll.TabStop = true;
            radioAll.Text = "&Alle Seiten";
            radioAll.UseVisualStyleBackColor = true;
            radioAll.CheckedChanged += RadioPages_CheckedChanged;
            //
            // radioSelected
            //
            radioSelected.AutoSize = true;
            radioSelected.Location = new Point(140, 35);
            radioSelected.Name = "radioSelected";
            radioSelected.Size = new Size(140, 19);
            radioSelected.TabIndex = 2;
            radioSelected.Text = "Nur &markierte Seite";
            radioSelected.UseVisualStyleBackColor = true;
            radioSelected.CheckedChanged += RadioPages_CheckedChanged;
            //
            // labelFileName
            //
            labelFileName.AutoSize = true;
            labelFileName.Location = new Point(12, 69);
            labelFileName.Name = "labelFileName";
            labelFileName.Size = new Size(66, 15);
            labelFileName.TabIndex = 3;
            labelFileName.Text = "&Dateiname:";
            //
            // textFileName
            //
            textFileName.Location = new Point(140, 66);
            textFileName.Name = "textFileName";
            textFileName.Size = new Size(252, 23);
            textFileName.TabIndex = 4;
            //
            // labelFolder
            //
            labelFolder.AutoSize = true;
            labelFolder.Location = new Point(12, 98);
            labelFolder.Name = "labelFolder";
            labelFolder.Size = new Size(48, 15);
            labelFolder.TabIndex = 5;
            labelFolder.Text = "&Ordner:";
            //
            // textFolder
            //
            textFolder.Location = new Point(140, 95);
            textFolder.Name = "textFolder";
            textFolder.Size = new Size(214, 23);
            textFolder.TabIndex = 6;
            //
            // btnBrowse
            //
            btnBrowse.Location = new Point(360, 94);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(32, 25);
            btnBrowse.TabIndex = 7;
            btnBrowse.Text = "…";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += BtnBrowse_Click;
            //
            // labelFileType
            //
            labelFileType.AutoSize = true;
            labelFileType.Location = new Point(12, 127);
            labelFileType.Name = "labelFileType";
            labelFileType.Size = new Size(55, 15);
            labelFileType.TabIndex = 8;
            labelFileType.Text = "Dateity&p:";
            //
            // comboFileType
            //
            comboFileType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboFileType.Items.AddRange(new object[] { "PDF", "JPEG (Bilddatei)" });
            comboFileType.Location = new Point(140, 124);
            comboFileType.Name = "comboFileType";
            comboFileType.Size = new Size(252, 23);
            comboFileType.TabIndex = 9;
            comboFileType.SelectedIndexChanged += ComboFileType_SelectedIndexChanged;
            //
            // labelOcr
            //
            labelOcr.AutoSize = true;
            labelOcr.Location = new Point(12, 156);
            labelOcr.Name = "labelOcr";
            labelOcr.Size = new Size(93, 15);
            labelOcr.TabIndex = 10;
            labelOcr.Text = "&Texterkennung:";
            //
            // comboOcr
            //
            comboOcr.DropDownStyle = ComboBoxStyle.DropDownList;
            comboOcr.Location = new Point(140, 153);
            comboOcr.Name = "comboOcr";
            comboOcr.Size = new Size(252, 23);
            comboOcr.TabIndex = 11;
            //
            // labelQuality
            //
            labelQuality.AutoSize = true;
            labelQuality.Location = new Point(12, 185);
            labelQuality.Name = "labelQuality";
            labelQuality.Size = new Size(122, 15);
            labelQuality.TabIndex = 12;
            labelQuality.Text = "JPEG-&Qualität (30–100):";
            //
            // numJpgQuality
            //
            numJpgQuality.Location = new Point(140, 182);
            numJpgQuality.Minimum = new decimal(new int[] { 30, 0, 0, 0 });
            numJpgQuality.Name = "numJpgQuality";
            numJpgQuality.Size = new Size(60, 23);
            numJpgQuality.TabIndex = 13;
            numJpgQuality.Value = new decimal(new int[] { 75, 0, 0, 0 });
            //
            // labelMeta
            //
            labelMeta.AutoSize = true;
            labelMeta.ForeColor = SystemColors.GrayText;
            labelMeta.Location = new Point(12, 220);
            labelMeta.Name = "labelMeta";
            labelMeta.Size = new Size(68, 15);
            labelMeta.TabIndex = 14;
            labelMeta.Text = "Metadaten:";
            //
            // labelTitle
            //
            labelTitle.AutoSize = true;
            labelTitle.Location = new Point(28, 245);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new Size(33, 15);
            labelTitle.TabIndex = 15;
            labelTitle.Text = "T&itel:";
            //
            // textTitle
            //
            textTitle.Location = new Point(140, 242);
            textTitle.Name = "textTitle";
            textTitle.Size = new Size(252, 23);
            textTitle.TabIndex = 16;
            //
            // labelSubject
            //
            labelSubject.AutoSize = true;
            labelSubject.Location = new Point(28, 274);
            labelSubject.Name = "labelSubject";
            labelSubject.Size = new Size(46, 15);
            labelSubject.TabIndex = 17;
            labelSubject.Text = "Th&ema:";
            //
            // textSubject
            //
            textSubject.Location = new Point(140, 271);
            textSubject.Name = "textSubject";
            textSubject.Size = new Size(252, 23);
            textSubject.TabIndex = 18;
            //
            // labelKeywords
            //
            labelKeywords.AutoSize = true;
            labelKeywords.Location = new Point(28, 303);
            labelKeywords.Name = "labelKeywords";
            labelKeywords.Size = new Size(66, 15);
            labelKeywords.TabIndex = 19;
            labelKeywords.Text = "Stich&worte:";
            //
            // textKeywords
            //
            textKeywords.Location = new Point(140, 300);
            textKeywords.Name = "textKeywords";
            textKeywords.Size = new Size(252, 23);
            textKeywords.TabIndex = 20;
            //
            // labelAuthor
            //
            labelAuthor.AutoSize = true;
            labelAuthor.Location = new Point(28, 332);
            labelAuthor.Name = "labelAuthor";
            labelAuthor.Size = new Size(60, 15);
            labelAuthor.TabIndex = 21;
            labelAuthor.Text = "&Verfasser:";
            //
            // textAuthor
            //
            textAuthor.Location = new Point(140, 329);
            textAuthor.Name = "textAuthor";
            textAuthor.Size = new Size(252, 23);
            textAuthor.TabIndex = 22;
            //
            // btnSave
            //
            btnSave.DialogResult = DialogResult.OK;
            btnSave.Location = new Point(226, 370);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(80, 26);
            btnSave.TabIndex = 23;
            btnSave.Text = "S&peichern";
            btnSave.UseVisualStyleBackColor = true;
            //
            // btnCancel
            //
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(312, 370);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 26);
            btnCancel.TabIndex = 24;
            btnCancel.Text = "Abbrechen";
            btnCancel.UseVisualStyleBackColor = true;
            //
            // SaveForm
            //
            AcceptButton = btnSave;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(404, 408);
            Controls.Add(labelPages);
            Controls.Add(radioAll);
            Controls.Add(radioSelected);
            Controls.Add(labelFileName);
            Controls.Add(textFileName);
            Controls.Add(labelFolder);
            Controls.Add(textFolder);
            Controls.Add(btnBrowse);
            Controls.Add(labelFileType);
            Controls.Add(comboFileType);
            Controls.Add(labelOcr);
            Controls.Add(comboOcr);
            Controls.Add(labelQuality);
            Controls.Add(numJpgQuality);
            Controls.Add(labelMeta);
            Controls.Add(labelTitle);
            Controls.Add(textTitle);
            Controls.Add(labelSubject);
            Controls.Add(textSubject);
            Controls.Add(labelKeywords);
            Controls.Add(textKeywords);
            Controls.Add(labelAuthor);
            Controls.Add(textAuthor);
            Controls.Add(btnSave);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SaveForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Speichern unter";
            ((System.ComponentModel.ISupportInitialize)numJpgQuality).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelPages;
        private System.Windows.Forms.RadioButton radioAll;
        private System.Windows.Forms.RadioButton radioSelected;
        private System.Windows.Forms.Label labelFileName;
        private System.Windows.Forms.TextBox textFileName;
        private System.Windows.Forms.Label labelFolder;
        private System.Windows.Forms.TextBox textFolder;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label labelFileType;
        private System.Windows.Forms.ComboBox comboFileType;
        private System.Windows.Forms.Label labelOcr;
        private System.Windows.Forms.ComboBox comboOcr;
        private System.Windows.Forms.Label labelQuality;
        private System.Windows.Forms.NumericUpDown numJpgQuality;
        private System.Windows.Forms.Label labelMeta;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.TextBox textTitle;
        private System.Windows.Forms.Label labelSubject;
        private System.Windows.Forms.TextBox textSubject;
        private System.Windows.Forms.Label labelKeywords;
        private System.Windows.Forms.TextBox textKeywords;
        private System.Windows.Forms.Label labelAuthor;
        private System.Windows.Forms.TextBox textAuthor;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
    }
}
