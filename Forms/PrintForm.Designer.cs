namespace ScanView.Forms
{
    partial class PrintForm
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
            groupPrinter = new GroupBox();
            labelPrinter = new Label();
            comboPrinter = new ComboBox();
            labelPaper = new Label();
            comboPaper = new ComboBox();
            labelSource = new Label();
            comboSource = new ComboBox();
            labelDuplex = new Label();
            comboDuplex = new ComboBox();
            labelCopies = new Label();
            numCopies = new NumericUpDown();
            chkColor = new CheckBox();
            chkFit = new CheckBox();
            btnPrint = new Button();
            btnCancel = new Button();
            groupScope.SuspendLayout();
            groupPrinter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCopies).BeginInit();
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
            // groupPrinter
            //
            groupPrinter.Controls.Add(labelPrinter);
            groupPrinter.Controls.Add(comboPrinter);
            groupPrinter.Controls.Add(labelPaper);
            groupPrinter.Controls.Add(comboPaper);
            groupPrinter.Controls.Add(labelSource);
            groupPrinter.Controls.Add(comboSource);
            groupPrinter.Controls.Add(labelDuplex);
            groupPrinter.Controls.Add(comboDuplex);
            groupPrinter.Controls.Add(labelCopies);
            groupPrinter.Controls.Add(numCopies);
            groupPrinter.Controls.Add(chkColor);
            groupPrinter.Controls.Add(chkFit);
            groupPrinter.Location = new Point(12, 68);
            groupPrinter.Name = "groupPrinter";
            groupPrinter.Size = new Size(380, 198);
            groupPrinter.TabIndex = 1;
            groupPrinter.TabStop = false;
            groupPrinter.Text = "Drucker";
            //
            // labelPrinter
            //
            labelPrinter.AutoSize = true;
            labelPrinter.Location = new Point(12, 25);
            labelPrinter.Name = "labelPrinter";
            labelPrinter.Size = new Size(52, 15);
            labelPrinter.TabIndex = 0;
            labelPrinter.Text = "Drucker:";
            //
            // comboPrinter
            //
            comboPrinter.DropDownStyle = ComboBoxStyle.DropDownList;
            comboPrinter.Location = new Point(124, 22);
            comboPrinter.Name = "comboPrinter";
            comboPrinter.Size = new Size(244, 23);
            comboPrinter.TabIndex = 1;
            comboPrinter.SelectedIndexChanged += ComboPrinter_SelectedIndexChanged;
            //
            // labelPaper
            //
            labelPaper.AutoSize = true;
            labelPaper.Location = new Point(12, 54);
            labelPaper.Name = "labelPaper";
            labelPaper.Size = new Size(82, 15);
            labelPaper.TabIndex = 2;
            labelPaper.Text = "&Papierformat:";
            //
            // comboPaper
            //
            comboPaper.DropDownStyle = ComboBoxStyle.DropDownList;
            comboPaper.Location = new Point(124, 51);
            comboPaper.Name = "comboPaper";
            comboPaper.Size = new Size(244, 23);
            comboPaper.TabIndex = 3;
            //
            // labelSource
            //
            labelSource.AutoSize = true;
            labelSource.Location = new Point(12, 83);
            labelSource.Name = "labelSource";
            labelSource.Size = new Size(77, 15);
            labelSource.TabIndex = 4;
            labelSource.Text = "Papier&zufuhr:";
            //
            // comboSource
            //
            comboSource.DropDownStyle = ComboBoxStyle.DropDownList;
            comboSource.Location = new Point(124, 80);
            comboSource.Name = "comboSource";
            comboSource.Size = new Size(244, 23);
            comboSource.TabIndex = 5;
            //
            // labelDuplex
            //
            labelDuplex.AutoSize = true;
            labelDuplex.Location = new Point(12, 112);
            labelDuplex.Name = "labelDuplex";
            labelDuplex.Size = new Size(112, 15);
            labelDuplex.TabIndex = 6;
            labelDuplex.Text = "&Beidseitiger Druck:";
            //
            // comboDuplex
            //
            comboDuplex.DropDownStyle = ComboBoxStyle.DropDownList;
            comboDuplex.Items.AddRange(new object[] { "Einseitig", "Beidseitig (lange Kante)", "Beidseitig (kurze Kante)" });
            comboDuplex.Location = new Point(124, 109);
            comboDuplex.Name = "comboDuplex";
            comboDuplex.Size = new Size(244, 23);
            comboDuplex.TabIndex = 7;
            //
            // labelCopies
            //
            labelCopies.AutoSize = true;
            labelCopies.Location = new Point(12, 141);
            labelCopies.Name = "labelCopies";
            labelCopies.Size = new Size(68, 15);
            labelCopies.TabIndex = 8;
            labelCopies.Text = "E&xemplare:";
            //
            // numCopies
            //
            numCopies.Location = new Point(124, 138);
            numCopies.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            numCopies.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numCopies.Name = "numCopies";
            numCopies.Size = new Size(60, 23);
            numCopies.TabIndex = 9;
            numCopies.Value = new decimal(new int[] { 1, 0, 0, 0 });
            //
            // chkColor
            //
            chkColor.AutoSize = true;
            chkColor.Location = new Point(214, 140);
            chkColor.Name = "chkColor";
            chkColor.Size = new Size(108, 19);
            chkColor.TabIndex = 10;
            chkColor.Text = "Far&big drucken";
            chkColor.UseVisualStyleBackColor = true;
            //
            // chkFit
            //
            chkFit.AutoSize = true;
            chkFit.Location = new Point(124, 168);
            chkFit.Name = "chkFit";
            chkFit.Size = new Size(214, 19);
            chkFit.TabIndex = 11;
            chkFit.Text = "Seiten auf Druck&fläche skalieren";
            chkFit.UseVisualStyleBackColor = true;
            //
            // btnPrint
            //
            btnPrint.DialogResult = DialogResult.OK;
            btnPrint.Location = new Point(226, 278);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(80, 26);
            btnPrint.TabIndex = 2;
            btnPrint.Text = "&Drucken";
            btnPrint.UseVisualStyleBackColor = true;
            //
            // btnCancel
            //
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(312, 278);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 26);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "Abbrechen";
            btnCancel.UseVisualStyleBackColor = true;
            //
            // PrintForm
            //
            AcceptButton = btnPrint;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(404, 316);
            Controls.Add(groupScope);
            Controls.Add(groupPrinter);
            Controls.Add(btnPrint);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PrintForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Drucken";
            groupScope.ResumeLayout(false);
            groupScope.PerformLayout();
            groupPrinter.ResumeLayout(false);
            groupPrinter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCopies).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox groupScope;
        private System.Windows.Forms.RadioButton radioAll;
        private System.Windows.Forms.RadioButton radioSelected;
        private System.Windows.Forms.GroupBox groupPrinter;
        private System.Windows.Forms.Label labelPrinter;
        private System.Windows.Forms.ComboBox comboPrinter;
        private System.Windows.Forms.Label labelPaper;
        private System.Windows.Forms.ComboBox comboPaper;
        private System.Windows.Forms.Label labelSource;
        private System.Windows.Forms.ComboBox comboSource;
        private System.Windows.Forms.Label labelDuplex;
        private System.Windows.Forms.ComboBox comboDuplex;
        private System.Windows.Forms.Label labelCopies;
        private System.Windows.Forms.NumericUpDown numCopies;
        private System.Windows.Forms.CheckBox chkColor;
        private System.Windows.Forms.CheckBox chkFit;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.Button btnCancel;
    }
}
