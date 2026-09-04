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
            groupScope = new GroupBox();
            radioAll = new RadioButton();
            radioSelected = new RadioButton();
            groupFile = new GroupBox();
            labelFileName = new Label();
            textFileName = new TextBox();
            labelFolder = new Label();
            textFolder = new TextBox();
            btnBrowse = new Button();
            labelFileType = new Label();
            comboFileType = new ComboBox();
            groupOcr = new GroupBox();
            textBoxQuality = new TextBox();
            comboQuality = new ComboBox();
            labelLargeSize = new Label();
            labelLowSize = new Label();
            labelOcr = new Label();
            comboOcr = new ComboBox();
            labelQuality = new Label();
            trackQuality = new TrackBar();
            groupMeta = new GroupBox();
            labelTitle = new Label();
            textTitle = new TextBox();
            labelSubject = new Label();
            textSubject = new TextBox();
            labelKeywords = new Label();
            textKeywords = new TextBox();
            labelAuthor = new Label();
            textAuthor = new TextBox();
            cbOpenAfter = new CheckBox();
            btnSave = new Button();
            btnCancel = new Button();
            groupScope.SuspendLayout();
            groupFile.SuspendLayout();
            groupOcr.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackQuality).BeginInit();
            groupMeta.SuspendLayout();
            SuspendLayout();
            // 
            // groupScope
            // 
            groupScope.Controls.Add(radioAll);
            groupScope.Controls.Add(radioSelected);
            groupScope.Location = new Point(12, 8);
            groupScope.Name = "groupScope";
            groupScope.Size = new Size(380, 52);
            groupScope.TabIndex = 0;
            groupScope.TabStop = false;
            groupScope.Text = "Umfang";
            // 
            // radioAll
            // 
            radioAll.AutoSize = true;
            radioAll.Checked = true;
            radioAll.Location = new Point(15, 21);
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
            radioSelected.Location = new Point(190, 21);
            radioSelected.Name = "radioSelected";
            radioSelected.Size = new Size(126, 19);
            radioSelected.TabIndex = 1;
            radioSelected.Text = "Nur &markierte Seite";
            radioSelected.UseVisualStyleBackColor = true;
            // 
            // groupFile
            // 
            groupFile.Controls.Add(labelFileName);
            groupFile.Controls.Add(textFileName);
            groupFile.Controls.Add(labelFolder);
            groupFile.Controls.Add(textFolder);
            groupFile.Controls.Add(btnBrowse);
            groupFile.Controls.Add(labelFileType);
            groupFile.Controls.Add(comboFileType);
            groupFile.Location = new Point(12, 68);
            groupFile.Name = "groupFile";
            groupFile.Size = new Size(380, 115);
            groupFile.TabIndex = 1;
            groupFile.TabStop = false;
            groupFile.Text = "Datei";
            // 
            // labelFileName
            // 
            labelFileName.AutoSize = true;
            labelFileName.Location = new Point(12, 25);
            labelFileName.Name = "labelFileName";
            labelFileName.Size = new Size(67, 15);
            labelFileName.TabIndex = 0;
            labelFileName.Text = "&Dateiname:";
            // 
            // textFileName
            // 
            textFileName.Location = new Point(124, 22);
            textFileName.Name = "textFileName";
            textFileName.Size = new Size(244, 23);
            textFileName.TabIndex = 1;
            // 
            // labelFolder
            // 
            labelFolder.AutoSize = true;
            labelFolder.Location = new Point(12, 54);
            labelFolder.Name = "labelFolder";
            labelFolder.Size = new Size(47, 15);
            labelFolder.TabIndex = 2;
            labelFolder.Text = "&Ordner:";
            // 
            // textFolder
            // 
            textFolder.Location = new Point(124, 51);
            textFolder.Name = "textFolder";
            textFolder.Size = new Size(206, 23);
            textFolder.TabIndex = 3;
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(336, 50);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(32, 25);
            btnBrowse.TabIndex = 4;
            btnBrowse.Text = "…";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += BtnBrowse_Click;
            // 
            // labelFileType
            // 
            labelFileType.AutoSize = true;
            labelFileType.Location = new Point(12, 83);
            labelFileType.Name = "labelFileType";
            labelFileType.Size = new Size(54, 15);
            labelFileType.TabIndex = 5;
            labelFileType.Text = "Dateity&p:";
            // 
            // comboFileType
            // 
            comboFileType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboFileType.Items.AddRange(new object[] { "PDF", "PDF/A (ohne Texterkennung)", "JPEG (Bilddatei)", "PNG (verlustfrei)", "TIFF (verlustfrei)" });
            comboFileType.Location = new Point(124, 80);
            comboFileType.Name = "comboFileType";
            comboFileType.Size = new Size(244, 23);
            comboFileType.TabIndex = 6;
            comboFileType.SelectedIndexChanged += ComboFileType_SelectedIndexChanged;
            // 
            // groupOcr
            // 
            groupOcr.Controls.Add(textBoxQuality);
            groupOcr.Controls.Add(comboQuality);
            groupOcr.Controls.Add(labelLargeSize);
            groupOcr.Controls.Add(labelLowSize);
            groupOcr.Controls.Add(labelOcr);
            groupOcr.Controls.Add(comboOcr);
            groupOcr.Controls.Add(labelQuality);
            groupOcr.Controls.Add(trackQuality);
            groupOcr.Location = new Point(12, 191);
            groupOcr.Name = "groupOcr";
            groupOcr.Size = new Size(380, 122);
            groupOcr.TabIndex = 2;
            groupOcr.TabStop = false;
            groupOcr.Text = "Texterkennung";
            // 
            // textBoxQuality
            // 
            textBoxQuality.Location = new Point(124, 53);
            textBoxQuality.MaxLength = 2;
            textBoxQuality.Name = "textBoxQuality";
            textBoxQuality.Size = new Size(77, 23);
            textBoxQuality.TabIndex = 7;
            textBoxQuality.TextChanged += TextBoxQuality_TextChanged;
            textBoxQuality.KeyPress += TextBoxQuality_KeyPress;
            textBoxQuality.Leave += TextBoxQuality_Leave;
            // 
            // comboQuality
            // 
            comboQuality.DropDownStyle = ComboBoxStyle.DropDownList;
            comboQuality.Items.AddRange(new object[] { "Niedrig", "Mittel", "Hoch" });
            comboQuality.Location = new Point(214, 53);
            comboQuality.Name = "comboQuality";
            comboQuality.Size = new Size(154, 23);
            comboQuality.TabIndex = 6;
            comboQuality.SelectedIndexChanged += ComboQuality_SelectedIndexChanged;
            // 
            // labelLargeSize
            // 
            labelLargeSize.AutoSize = true;
            labelLargeSize.Font = new Font("Segoe UI", 8F);
            labelLargeSize.Location = new Point(300, 79);
            labelLargeSize.Name = "labelLargeSize";
            labelLargeSize.Size = new Size(68, 13);
            labelLargeSize.TabIndex = 5;
            labelLargeSize.Text = "Große Datei";
            // 
            // labelLowSize
            // 
            labelLowSize.AutoSize = true;
            labelLowSize.Font = new Font("Segoe UI", 8F);
            labelLowSize.Location = new Point(124, 79);
            labelLowSize.Name = "labelLowSize";
            labelLowSize.Size = new Size(68, 13);
            labelLowSize.TabIndex = 4;
            labelLowSize.Text = "Kleine Datei";
            // 
            // labelOcr
            // 
            labelOcr.AutoSize = true;
            labelOcr.Location = new Point(12, 25);
            labelOcr.Name = "labelOcr";
            labelOcr.Size = new Size(52, 15);
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
            labelQuality.Size = new Size(82, 15);
            labelQuality.TabIndex = 2;
            labelQuality.Text = "JPEG-&Qualität:";
            // 
            // trackQuality
            // 
            trackQuality.AutoSize = false;
            trackQuality.LargeChange = 2;
            trackQuality.Location = new Point(124, 93);
            trackQuality.Maximum = 12;
            trackQuality.Name = "trackQuality";
            trackQuality.Size = new Size(244, 23);
            trackQuality.TabIndex = 3;
            trackQuality.Value = 8;
            trackQuality.ValueChanged += TrackQuality_ValueChanged;
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
            groupMeta.Location = new Point(12, 319);
            groupMeta.Name = "groupMeta";
            groupMeta.Size = new Size(380, 146);
            groupMeta.TabIndex = 3;
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
            labelTitle.Text = "&Title:";
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
            labelSubject.Size = new Size(49, 15);
            labelSubject.TabIndex = 2;
            labelSubject.Text = "Su&bject:";
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
            labelKeywords.Size = new Size(61, 15);
            labelKeywords.TabIndex = 4;
            labelKeywords.Text = "&Keywords:";
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
            labelAuthor.Size = new Size(47, 15);
            labelAuthor.TabIndex = 6;
            labelAuthor.Text = "Aut&hor:";
            // 
            // textAuthor
            // 
            textAuthor.Location = new Point(124, 109);
            textAuthor.Name = "textAuthor";
            textAuthor.Size = new Size(244, 23);
            textAuthor.TabIndex = 7;
            // 
            // cbOpenAfter
            // 
            cbOpenAfter.AutoSize = true;
            cbOpenAfter.Location = new Point(28, 482);
            cbOpenAfter.Name = "cbOpenAfter";
            cbOpenAfter.Size = new Size(174, 19);
            cbOpenAfter.TabIndex = 6;
            cbOpenAfter.Text = "Nach dem Speichern öff&nen";
            cbOpenAfter.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            btnSave.DialogResult = DialogResult.OK;
            btnSave.Location = new Point(226, 477);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(80, 26);
            btnSave.TabIndex = 4;
            btnSave.Text = "S&peichern";
            btnSave.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(312, 477);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 26);
            btnCancel.TabIndex = 5;
            btnCancel.Text = "Abbrechen";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // SaveForm
            // 
            AcceptButton = btnSave;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(404, 515);
            Controls.Add(groupScope);
            Controls.Add(groupFile);
            Controls.Add(groupOcr);
            Controls.Add(groupMeta);
            Controls.Add(cbOpenAfter);
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
            groupScope.ResumeLayout(false);
            groupScope.PerformLayout();
            groupFile.ResumeLayout(false);
            groupFile.PerformLayout();
            groupOcr.ResumeLayout(false);
            groupOcr.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackQuality).EndInit();
            groupMeta.ResumeLayout(false);
            groupMeta.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.GroupBox groupScope;
        private System.Windows.Forms.RadioButton radioAll;
        private System.Windows.Forms.RadioButton radioSelected;
        private System.Windows.Forms.GroupBox groupFile;
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
        private System.Windows.Forms.TrackBar trackQuality;
        private System.Windows.Forms.GroupBox groupMeta;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.TextBox textTitle;
        private System.Windows.Forms.Label labelSubject;
        private System.Windows.Forms.TextBox textSubject;
        private System.Windows.Forms.Label labelKeywords;
        private System.Windows.Forms.TextBox textKeywords;
        private System.Windows.Forms.Label labelAuthor;
        private System.Windows.Forms.TextBox textAuthor;
        private System.Windows.Forms.CheckBox cbOpenAfter;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private ComboBox comboQuality;
        private Label labelLargeSize;
        private Label labelLowSize;
        private TextBox textBoxQuality;
    }
}
