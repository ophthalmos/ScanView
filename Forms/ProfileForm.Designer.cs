namespace ScanView.Forms
{
    partial class ProfileForm
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
            labelName = new Label();
            textName = new TextBox();
            btnAdd = new Button();
            labelList = new Label();
            listProfiles = new ListBox();
            btnDelete = new Button();
            btnUp = new Button();
            btnDown = new Button();
            picHint = new PictureBox();
            labelHint = new Label();
            labelSettings = new Label();
            labelSeparator = new Label();
            labelRename = new Label();
            textRename = new TextBox();
            btnRename = new Button();
            btnOk = new Button();
            btnCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)picHint).BeginInit();
            SuspendLayout();
            // 
            // labelName
            // 
            labelName.AutoSize = true;
            labelName.Location = new Point(12, 15);
            labelName.Name = "labelName";
            labelName.Size = new Size(68, 15);
            labelName.TabIndex = 0;
            labelName.Text = "Profil&name:";
            // 
            // textName
            // 
            textName.Location = new Point(12, 33);
            textName.Name = "textName";
            textName.Size = new Size(340, 23);
            textName.TabIndex = 1;
            textName.Enter += TextName_Enter;
            textName.Leave += TextName_Leave;
            //
            // btnAdd
            //
            btnAdd.Location = new Point(12, 104);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(340, 25);
            btnAdd.TabIndex = 2;
            btnAdd.Text = "&Aktuelle Einstellungen speichern";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += BtnAdd_Click;
            // 
            // labelList
            // 
            labelList.AutoSize = true;
            labelList.Location = new Point(12, 150);
            labelList.Name = "labelList";
            labelList.Size = new Size(115, 15);
            labelList.TabIndex = 4;
            labelList.Text = "&Gespeicherte Profile:";
            // 
            // listProfiles
            // 
            listProfiles.Location = new Point(12, 168);
            listProfiles.Name = "listProfiles";
            listProfiles.Size = new Size(242, 124);
            listProfiles.TabIndex = 5;
            listProfiles.SelectedIndexChanged += ListProfiles_SelectedIndexChanged;
            listProfiles.MouseDown += ListProfiles_MouseDown;
            // 
            // btnDelete
            // 
            btnDelete.Enabled = false;
            btnDelete.Location = new Point(260, 168);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(92, 25);
            btnDelete.TabIndex = 6;
            btnDelete.Text = "&Löschen";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += BtnDelete_Click;
            // 
            // btnUp
            // 
            btnUp.Enabled = false;
            btnUp.Location = new Point(260, 209);
            btnUp.Name = "btnUp";
            btnUp.Size = new Size(92, 25);
            btnUp.TabIndex = 9;
            btnUp.Text = "Nach &oben";
            btnUp.UseVisualStyleBackColor = true;
            btnUp.Click += BtnUp_Click;
            // 
            // btnDown
            // 
            btnDown.Enabled = false;
            btnDown.Location = new Point(260, 240);
            btnDown.Name = "btnDown";
            btnDown.Size = new Size(92, 25);
            btnDown.TabIndex = 10;
            btnDown.Text = "Nach &unten";
            btnDown.UseVisualStyleBackColor = true;
            btnDown.Click += BtnDown_Click;
            //
            // picHint
            //
            picHint.Location = new Point(12, 62);
            picHint.Name = "picHint";
            picHint.Size = new Size(16, 16);
            picHint.SizeMode = PictureBoxSizeMode.Zoom;
            picHint.TabIndex = 11;
            picHint.TabStop = false;
            //
            // labelHint
            //
            labelHint.AutoSize = true;
            labelHint.ForeColor = SystemColors.GrayText;
            labelHint.Location = new Point(32, 63);
            labelHint.Name = "labelHint";
            labelHint.Size = new Size(288, 15);
            labelHint.TabIndex = 3;
            labelHint.Text = "Gespeichert werden die aktuellen Scan-Einstellungen:";
            //
            // labelSettings
            //
            labelSettings.AutoSize = true;
            labelSettings.Location = new Point(32, 81);
            labelSettings.Name = "labelSettings";
            labelSettings.Size = new Size(220, 15);
            labelSettings.TabIndex = 12;
            labelSettings.Text = "300 dpi · Graustufen · A4 · Flachbett";
            //
            // labelSeparator
            //
            labelSeparator.BorderStyle = BorderStyle.Fixed3D;
            labelSeparator.Location = new Point(12, 140);
            labelSeparator.Name = "labelSeparator";
            labelSeparator.Size = new Size(340, 2);
            labelSeparator.TabIndex = 13;
            //
            // labelRename
            //
            labelRename.AutoSize = true;
            labelRename.Location = new Point(12, 301);
            labelRename.Name = "labelRename";
            labelRename.Size = new Size(75, 15);
            labelRename.TabIndex = 14;
            labelRename.Text = "Neuer Na&me:";
            //
            // textRename
            //
            textRename.Location = new Point(110, 298);
            textRename.Name = "textRename";
            textRename.Size = new Size(144, 23);
            textRename.TabIndex = 15;
            textRename.Enter += TextRename_Enter;
            textRename.Leave += TextRename_Leave;
            //
            // btnRename
            //
            btnRename.Enabled = false;
            btnRename.Location = new Point(260, 297);
            btnRename.Name = "btnRename";
            btnRename.Size = new Size(92, 25);
            btnRename.TabIndex = 16;
            btnRename.Text = "&Umbenennen";
            btnRename.UseVisualStyleBackColor = true;
            btnRename.Click += BtnRename_Click;
            //
            // btnOk
            // 
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Location = new Point(169, 335);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(85, 26);
            btnOk.TabIndex = 7;
            btnOk.Text = "OK";
            btnOk.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(260, 335);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(92, 26);
            btnCancel.TabIndex = 8;
            btnCancel.Text = "Abbrechen";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // ProfileForm
            // 
            AcceptButton = btnOk;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(364, 373);
            Controls.Add(labelName);
            Controls.Add(textName);
            Controls.Add(btnAdd);
            Controls.Add(picHint);
            Controls.Add(labelHint);
            Controls.Add(labelSettings);
            Controls.Add(labelSeparator);
            Controls.Add(labelList);
            Controls.Add(listProfiles);
            Controls.Add(btnDelete);
            Controls.Add(btnUp);
            Controls.Add(btnDown);
            Controls.Add(labelRename);
            Controls.Add(textRename);
            Controls.Add(btnRename);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ProfileForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Scan-Profile";
            ((System.ComponentModel.ISupportInitialize)picHint).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.TextBox textName;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Label labelList;
        private System.Windows.Forms.ListBox listProfiles;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.PictureBox picHint;
        private System.Windows.Forms.Label labelHint;
        private System.Windows.Forms.Label labelSettings;
        private System.Windows.Forms.Label labelSeparator;
        private System.Windows.Forms.Label labelRename;
        private System.Windows.Forms.TextBox textRename;
        private System.Windows.Forms.Button btnRename;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}
