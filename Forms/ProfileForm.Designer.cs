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
            groupSave = new GroupBox();
            labelName = new Label();
            textName = new TextBox();
            picHint = new PictureBox();
            labelHint = new Label();
            labelSettings = new Label();
            btnAdd = new Button();
            groupProfiles = new GroupBox();
            listProfiles = new ListBox();
            btnDelete = new Button();
            btnUp = new Button();
            btnDown = new Button();
            labelRename = new Label();
            textRename = new TextBox();
            btnRename = new Button();
            btnOk = new Button();
            btnCancel = new Button();
            groupSave.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picHint).BeginInit();
            groupProfiles.SuspendLayout();
            SuspendLayout();
            //
            // groupSave
            //
            groupSave.Controls.Add(labelName);
            groupSave.Controls.Add(textName);
            groupSave.Controls.Add(picHint);
            groupSave.Controls.Add(labelHint);
            groupSave.Controls.Add(labelSettings);
            groupSave.Controls.Add(btnAdd);
            groupSave.Location = new Point(12, 8);
            groupSave.Name = "groupSave";
            groupSave.Size = new Size(340, 146);
            groupSave.TabIndex = 0;
            groupSave.TabStop = false;
            groupSave.Text = "Neues Profil";
            //
            // labelName
            //
            labelName.AutoSize = true;
            labelName.Location = new Point(12, 22);
            labelName.Name = "labelName";
            labelName.Size = new Size(68, 15);
            labelName.TabIndex = 0;
            labelName.Text = "Profil&name:";
            //
            // textName
            //
            textName.Location = new Point(12, 40);
            textName.Name = "textName";
            textName.Size = new Size(316, 23);
            textName.TabIndex = 1;
            textName.Enter += TextName_Enter;
            textName.Leave += TextName_Leave;
            //
            // picHint
            //
            picHint.Location = new Point(12, 69);
            picHint.Name = "picHint";
            picHint.Size = new Size(16, 16);
            picHint.SizeMode = PictureBoxSizeMode.Zoom;
            picHint.TabIndex = 4;
            picHint.TabStop = false;
            //
            // labelHint
            //
            labelHint.AutoSize = true;
            labelHint.ForeColor = SystemColors.GrayText;
            labelHint.Location = new Point(32, 70);
            labelHint.Name = "labelHint";
            labelHint.Size = new Size(288, 15);
            labelHint.TabIndex = 3;
            labelHint.Text = "Gespeichert werden die aktuellen Scan-Einstellungen:";
            //
            // labelSettings
            //
            labelSettings.AutoSize = true;
            labelSettings.Location = new Point(32, 88);
            labelSettings.Name = "labelSettings";
            labelSettings.Size = new Size(220, 15);
            labelSettings.TabIndex = 5;
            labelSettings.Text = "300 dpi · Graustufen · A4 · Flachbett";
            //
            // btnAdd
            //
            btnAdd.Location = new Point(12, 111);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(316, 25);
            btnAdd.TabIndex = 2;
            btnAdd.Text = "&Aktuelle Einstellungen speichern";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += BtnAdd_Click;
            //
            // groupProfiles
            //
            groupProfiles.Controls.Add(listProfiles);
            groupProfiles.Controls.Add(btnDelete);
            groupProfiles.Controls.Add(btnUp);
            groupProfiles.Controls.Add(btnDown);
            groupProfiles.Controls.Add(labelRename);
            groupProfiles.Controls.Add(textRename);
            groupProfiles.Controls.Add(btnRename);
            groupProfiles.Location = new Point(12, 162);
            groupProfiles.Name = "groupProfiles";
            groupProfiles.Size = new Size(340, 186);
            groupProfiles.TabIndex = 1;
            groupProfiles.TabStop = false;
            groupProfiles.Text = "Gespeicherte Profile";
            //
            // listProfiles
            //
            listProfiles.Location = new Point(12, 22);
            listProfiles.Name = "listProfiles";
            listProfiles.Size = new Size(216, 124);
            listProfiles.TabIndex = 0;
            listProfiles.SelectedIndexChanged += ListProfiles_SelectedIndexChanged;
            listProfiles.MouseDown += ListProfiles_MouseDown;
            //
            // btnDelete
            //
            btnDelete.Enabled = false;
            btnDelete.Location = new Point(236, 22);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(92, 25);
            btnDelete.TabIndex = 1;
            btnDelete.Text = "&Löschen";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += BtnDelete_Click;
            //
            // btnUp
            //
            btnUp.Enabled = false;
            btnUp.Location = new Point(236, 63);
            btnUp.Name = "btnUp";
            btnUp.Size = new Size(92, 25);
            btnUp.TabIndex = 2;
            btnUp.Text = "Nach &oben";
            btnUp.UseVisualStyleBackColor = true;
            btnUp.Click += BtnUp_Click;
            //
            // btnDown
            //
            btnDown.Enabled = false;
            btnDown.Location = new Point(236, 94);
            btnDown.Name = "btnDown";
            btnDown.Size = new Size(92, 25);
            btnDown.TabIndex = 3;
            btnDown.Text = "Nach &unten";
            btnDown.UseVisualStyleBackColor = true;
            btnDown.Click += BtnDown_Click;
            //
            // labelRename
            //
            labelRename.AutoSize = true;
            labelRename.Location = new Point(12, 155);
            labelRename.Name = "labelRename";
            labelRename.Size = new Size(75, 15);
            labelRename.TabIndex = 4;
            labelRename.Text = "Neuer Na&me:";
            //
            // textRename
            //
            textRename.Location = new Point(110, 152);
            textRename.Name = "textRename";
            textRename.Size = new Size(118, 23);
            textRename.TabIndex = 5;
            textRename.Enter += TextRename_Enter;
            textRename.Leave += TextRename_Leave;
            //
            // btnRename
            //
            btnRename.Enabled = false;
            btnRename.Location = new Point(236, 151);
            btnRename.Name = "btnRename";
            btnRename.Size = new Size(92, 25);
            btnRename.TabIndex = 6;
            btnRename.Text = "&Umbenennen";
            btnRename.UseVisualStyleBackColor = true;
            btnRename.Click += BtnRename_Click;
            //
            // btnOk
            //
            btnOk.AutoSize = true;
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Enabled = false;
            btnOk.Location = new Point(112, 358);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(142, 26);
            btnOk.TabIndex = 2;
            btnOk.Text = "Änderungen &speichern";
            btnOk.UseVisualStyleBackColor = true;
            //
            // btnCancel
            //
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(260, 358);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(92, 26);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "Abbrechen";
            btnCancel.UseVisualStyleBackColor = true;
            //
            // ProfileForm
            //
            AcceptButton = btnOk;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(364, 396);
            Controls.Add(groupSave);
            Controls.Add(groupProfiles);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ProfileForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Profilverwaltung";
            groupSave.ResumeLayout(false);
            groupSave.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picHint).EndInit();
            groupProfiles.ResumeLayout(false);
            groupProfiles.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.GroupBox groupSave;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.TextBox textName;
        private System.Windows.Forms.PictureBox picHint;
        private System.Windows.Forms.Label labelHint;
        private System.Windows.Forms.Label labelSettings;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.GroupBox groupProfiles;
        private System.Windows.Forms.ListBox listProfiles;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.Label labelRename;
        private System.Windows.Forms.TextBox textRename;
        private System.Windows.Forms.Button btnRename;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}
