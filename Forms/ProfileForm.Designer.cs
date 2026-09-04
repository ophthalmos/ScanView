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
            labelHint = new Label();
            btnOk = new Button();
            btnCancel = new Button();
            SuspendLayout();
            //
            // labelName
            //
            labelName.AutoSize = true;
            labelName.Location = new Point(12, 15);
            labelName.Name = "labelName";
            labelName.Size = new Size(42, 15);
            labelName.TabIndex = 0;
            labelName.Text = "&Name:";
            //
            // textName
            //
            textName.Location = new Point(64, 12);
            textName.Name = "textName";
            textName.Size = new Size(190, 23);
            textName.TabIndex = 1;
            //
            // btnAdd
            //
            btnAdd.Location = new Point(260, 11);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(92, 25);
            btnAdd.TabIndex = 2;
            btnAdd.Text = "&Hinzufügen";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += BtnAdd_Click;
            //
            // labelHint
            //
            labelHint.AutoSize = true;
            labelHint.ForeColor = SystemColors.GrayText;
            labelHint.Location = new Point(12, 40);
            labelHint.Name = "labelHint";
            labelHint.Size = new Size(297, 15);
            labelHint.TabIndex = 3;
            labelHint.Text = "Gespeichert werden die aktuellen Scan-Einstellungen.";
            //
            // labelList
            //
            labelList.AutoSize = true;
            labelList.Location = new Point(12, 68);
            labelList.Name = "labelList";
            labelList.Size = new Size(122, 15);
            labelList.TabIndex = 4;
            labelList.Text = "&Gespeicherte Profile:";
            //
            // listProfiles
            //
            listProfiles.ItemHeight = 15;
            listProfiles.Location = new Point(12, 86);
            listProfiles.Name = "listProfiles";
            listProfiles.Size = new Size(242, 124);
            listProfiles.TabIndex = 5;
            listProfiles.SelectedIndexChanged += ListProfiles_SelectedIndexChanged;
            //
            // btnDelete
            //
            btnDelete.Enabled = false;
            btnDelete.Location = new Point(260, 86);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(92, 25);
            btnDelete.TabIndex = 6;
            btnDelete.Text = "&Löschen";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += BtnDelete_Click;
            //
            // btnOk
            //
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Location = new Point(186, 226);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(80, 26);
            btnOk.TabIndex = 7;
            btnOk.Text = "OK";
            btnOk.UseVisualStyleBackColor = true;
            //
            // btnCancel
            //
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(272, 226);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 26);
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
            ClientSize = new Size(364, 264);
            Controls.Add(labelName);
            Controls.Add(textName);
            Controls.Add(btnAdd);
            Controls.Add(labelHint);
            Controls.Add(labelList);
            Controls.Add(listProfiles);
            Controls.Add(btnDelete);
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
        private System.Windows.Forms.Label labelHint;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}
