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
            groupFile = new GroupBox();
            radioAll = new RadioButton();
            radioSelected = new RadioButton();
            labelFileName = new Label();
            textFileName = new TextBox();
            labelFolder = new Label();
            textFolder = new TextBox();
            btnBrowse = new Button();
            labelFileType = new Label();
            comboFileType = new ComboBox();
            groupOcr = new GroupBox();
            labelOcr = new Label();
            comboOcr = new ComboBox();
            labelQuality = new Label();
            numJpgQuality = new NumericUpDown();
            groupMeta = new GroupBox();
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
            groupFile.SuspendLayout();
            groupOcr.SuspendLayout();
            groupMeta.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numJpgQuality).BeginInit();
            SuspendLayout();
            //
            // groupFile
            //
            groupFile.Controls.Add(radioAll);
            groupFile.Controls.Add(radioSelected);
            groupFile.Controls.Add(labelFileName);
            groupFile.Controls.Add(textFileName);
            groupFile.Controls.Add(labelFolder);
            groupFile.Controls.Add(textFolder);
            groupFile.Controls.Add(btnBrowse);
            groupFile.Controls.Add(labelFileType);
            groupFile.Controls.Add(comboFileType);
            groupFile.Location = new Point(12, 8);
            groupFile.Name = "groupFile";
            groupFile.Size = new Size(380, 148);
            groupFile.TabIndex = 0;
            groupFile.TabStop = false;
            groupFile.Text = "Datei";
            //
            // radioAll
            //
            radioAll.AutoSize = true;
            radioAll.Checked = true;
            radioAll.Location = new Point(15, 22);
            radioAll.Name = "radioAll";
            radioAll.Size = new Size(85, 19);
            radioAll.TabIndex = 0;
            radioAll.TabStop = true;
            radioAll.Text = "&Alle Seiten";
            radioAll.UseVisualStyleBackColor = true;
            //
            // radioSelected
            //
            radioSelected.AutoSize = true;
            radioSelected.Location = new Point(124, 22);
            radioSelected.Name = "radioSelected";
            radioSelected.Size = new Size(140, 19);
            radioSelected.TabIndex = 1;
            radioSelected.Text = "Nur &markierte Seite";
            radioSelected.UseVisualStyleBackColor = true;
            //
            // labelFileName
            //
            labelFileName.AutoSize = true;
            labelFileName.Location = new Point(12, 56);
            labelFileName.Name = "labelFileName";
            labelFileName.Size = new Size(66, 15);
            labelFileName.TabIndex = 2;
            labelFileName.Text = "&Dateiname:";
            //
            // textFileName
            //
            textFileName.Location = new Point(124, 53);
            textFileName.Name = "textFileName";
            textFileName.Size = new Size(244, 23);
            textFileName.TabIndex = 3;
            //
            // labelFolder
            //
            labelFolder.AutoSize = true;
            labelFolder.Location = new Point(12, 85);
            labelFolder.Name = "labelFolder";
            labelFolder.Size = new Size(48, 15);
            labelFolder.TabIndex = 4;
            labelFolder.Text = "&Ordner:";
            //
            // textFolder
            //
            textFolder.Location = new Point(124, 82);
            textFolder.Name = "textFolder";
            textFolder.Size = new Size(206, 23);
            textFolder.TabIndex = 5;
            //
            // btnBrowse
            //
            btnBrowse.Location = new Point(336, 81);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(32, 25);
            btnBrowse.TabIndex = 6;
            btnBrowse.Text = "…";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += BtnBrowse_Click;
            //
            // labelFileType
            //
            labelFileType.AutoSize = true;
            labelFileType.Location = new Point(12, 114);
            labelFileType.Name = "labelFileType";
            labelFileType.Size = new Size(55, 15);
            labelFileType.TabIndex = 7;
            labelFileType.Text = "Dateity&p:";
            //
            // comboFileType
            //
            comboFileType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboFileType.Items.AddRange(new object[] { "PDF", "JPEG (Bilddatei)", "TIFF (verlustfrei)" });
            comboFileType.Location = new Point(124, 111);
            comboFileType.Name = "comboFileType";
            comboFileType.Size = new Size(244, 23);
            comboFileType.TabIndex = 8;
            comboFileType.SelectedIndexChanged += ComboFileType_SelectedIndexChanged;
            //
            // groupOcr
            //
            groupOcr.Controls.Add(labelOcr);
            groupOcr.Controls.Add(comboOcr);
            groupOcr.Controls.Add(labelQuality);
            groupOcr.Controls.Add(numJpgQuality);
            groupOcr.Location = new Point(12, 164);
            groupOcr.Name = "groupOcr";
            groupOcr.Size = new Size(380, 90);
            groupOcr.TabIndex = 1;
            groupOcr.TabStop = false;
            groupOcr.Text = "Texterkennung";
            //
            // labelOcr
            //
            labelOcr.AutoSize = true;
            labelOcr.Location = new Point(12, 25);
            labelOcr.Name = "labelOcr";
            labelOcr.Size = new Size(54, 15);
            labelOcr.TabIndex = 0;
            labelOcr.Text = "&Sprache:";
            //
            // comboOcr
            //
            comboOcr.DropDownStyle = ComboBoxStyle.DropDownList;
            comboOcr.Location = new Point(124, 22);
            comboOcr.Name = "comboOcr";
            comboOcr.Size = new Size(244, 23);
            comboOcr.TabIndex = 1;
            //
            // labelQuality
            //
            labelQuality.AutoSize = true;
            labelQuality.Location = new Point(12, 56);
            labelQuality.Name = "labelQuality";
            labelQuality.Size = new Size(122, 15);
            labelQuality.TabIndex = 2;
            labelQuality.Text = "JPEG-&Qualität (30–100):";
            //
            // numJpgQuality
            //
            numJpgQuality.Location = new Point(154, 53);
            numJpgQuality.Minimum = new decimal(new int[] { 30, 0, 0, 0 });
            numJpgQuality.Name = "numJpgQuality";
            numJpgQuality.Size = new Size(60, 23);
            numJpgQuality.TabIndex = 3;
            numJpgQuality.Value = new decimal(new int[] { 75, 0, 0, 0 });
            //
            // groupMeta
            //
            groupMeta.Controls.Add(labelTitle);
            groupMeta.Controls.Add(textTitle);
            groupMeta.Controls.Add(labelSubject);
            groupMeta.Controls.Add(textSubject);
            groupMeta.Controls.Add(labelKeywords);
            groupMeta.Controls.Add(textKeywords);
            groupMeta.Controls.Add(labelAuthor);
            groupMeta.Controls.Add(textAuthor);
            groupMeta.Location = new Point(12, 262);
            groupMeta.Name = "groupMeta";
            groupMeta.Size = new Size(380, 146);
            groupMeta.TabIndex = 2;
            groupMeta.TabStop = false;
            groupMeta.Text = "Metadaten";
            //
            // labelTitle
            //
            labelTitle.AutoSize = true;
            labelTitle.Location = new Point(12, 25);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new Size(33, 15);
            labelTitle.TabIndex = 0;
            labelTitle.Text = "T&itel:";
            //
            // textTitle
            //
            textTitle.Location = new Point(124, 22);
            textTitle.Name = "textTitle";
            textTitle.Size = new Size(244, 23);
            textTitle.TabIndex = 1;
            //
            // labelSubject
            //
            labelSubject.AutoSize = true;
            labelSubject.Location = new Point(12, 54);
            labelSubject.Name = "labelSubject";
            labelSubject.Size = new Size(46, 15);
            labelSubject.TabIndex = 2;
            labelSubject.Text = "Th&ema:";
            //
            // textSubject
            //
            textSubject.Location = new Point(124, 51);
            textSubject.Name = "textSubject";
            textSubject.Size = new Size(244, 23);
            textSubject.TabIndex = 3;
            //
            // labelKeywords
            //
            labelKeywords.AutoSize = true;
            labelKeywords.Location = new Point(12, 83);
            labelKeywords.Name = "labelKeywords";
            labelKeywords.Size = new Size(66, 15);
            labelKeywords.TabIndex = 4;
            labelKeywords.Text = "Stich&worte:";
            //
            // textKeywords
            //
            textKeywords.Location = new Point(124, 80);
            textKeywords.Name = "textKeywords";
            textKeywords.Size = new Size(244, 23);
            textKeywords.TabIndex = 5;
            //
            // labelAuthor
            //
            labelAuthor.AutoSize = true;
            labelAuthor.Location = new Point(12, 112);
            labelAuthor.Name = "labelAuthor";
            labelAuthor.Size = new Size(60, 15);
            labelAuthor.TabIndex = 6;
            labelAuthor.Text = "&Verfasser:";
            //
            // textAuthor
            //
            textAuthor.Location = new Point(124, 109);
            textAuthor.Name = "textAuthor";
            textAuthor.Size = new Size(244, 23);
            textAuthor.TabIndex = 7;
            //
            // btnSave
            //
            btnSave.DialogResult = DialogResult.OK;
            btnSave.Location = new Point(226, 420);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(80, 26);
            btnSave.TabIndex = 3;
            btnSave.Text = "S&peichern";
            btnSave.UseVisualStyleBackColor = true;
            //
            // btnCancel
            //
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(312, 420);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 26);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "Abbrechen";
            btnCancel.UseVisualStyleBackColor = true;
            //
            // SaveForm
            //
            AcceptButton = btnSave;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(404, 458);
            Controls.Add(groupFile);
            Controls.Add(groupOcr);
            Controls.Add(groupMeta);
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
            groupFile.ResumeLayout(false);
            groupFile.PerformLayout();
            groupOcr.ResumeLayout(false);
            groupOcr.PerformLayout();
            groupMeta.ResumeLayout(false);
            groupMeta.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numJpgQuality).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox groupFile;
        private System.Windows.Forms.RadioButton radioAll;
        private System.Windows.Forms.RadioButton radioSelected;
        private System.Windows.Forms.Label labelFileName;
        private System.Windows.Forms.TextBox textFileName;
        private System.Windows.Forms.Label labelFolder;
        private System.Windows.Forms.TextBox textFolder;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label labelFileType;
        private System.Windows.Forms.ComboBox comboFileType;
        private System.Windows.Forms.GroupBox groupOcr;
        private System.Windows.Forms.Label labelOcr;
        private System.Windows.Forms.ComboBox comboOcr;
        private System.Windows.Forms.Label labelQuality;
        private System.Windows.Forms.NumericUpDown numJpgQuality;
        private System.Windows.Forms.GroupBox groupMeta;
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
