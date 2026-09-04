namespace ScanView.Forms
{
    partial class CropForm
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
            toolStrip = new ToolStrip();
            labelZoom = new ToolStripLabel();
            comboZoom = new ToolStripComboBox();
            btnZoomOut = new ToolStripButton();
            btnZoomIn = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            btnIsolate = new ToolStripButton();
            btnCropAction = new ToolStripButton();
            btnRemove = new ToolStripButton();
            btnApply = new ToolStripButton();
            btnCancel = new ToolStripButton();
            btnSaveAsNew = new ToolStripButton();
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            scrollPanel = new Panel();
            pictureBox = new PictureBox();
            toolStripSeparator2 = new ToolStripSeparator();
            toolStrip.SuspendLayout();
            statusStrip.SuspendLayout();
            scrollPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
            SuspendLayout();
            // 
            // toolStrip
            // 
            toolStrip.Font = new Font("Segoe UI", 10F);
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.ImageScalingSize = new Size(24, 24);
            toolStrip.Items.AddRange(new ToolStripItem[] { labelZoom, comboZoom, btnZoomOut, btnZoomIn, toolStripSeparator1, btnIsolate, btnCropAction, btnApply, btnCancel, toolStripSeparator2, btnRemove, btnSaveAsNew });
            toolStrip.Location = new Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.Size = new Size(884, 26);
            toolStrip.TabIndex = 0;
            // 
            // labelZoom
            // 
            labelZoom.Name = "labelZoom";
            labelZoom.Size = new Size(48, 23);
            labelZoom.Text = "&Zoom:";
            // 
            // comboZoom
            // 
            comboZoom.AutoSize = false;
            comboZoom.DropDownStyle = ComboBoxStyle.DropDownList;
            comboZoom.Items.AddRange(new object[] { "Einpassen", "50 %", "75 %", "100 %", "150 %", "200 %" });
            comboZoom.Name = "comboZoom";
            comboZoom.Size = new Size(110, 23);
            comboZoom.SelectedIndexChanged += ComboZoom_SelectedIndexChanged;
            // 
            // btnZoomOut
            // 
            btnZoomOut.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnZoomOut.Name = "btnZoomOut";
            btnZoomOut.Size = new Size(23, 23);
            btnZoomOut.ToolTipText = "Verkleinern";
            btnZoomOut.Click += BtnZoomOut_Click;
            // 
            // btnZoomIn
            // 
            btnZoomIn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnZoomIn.Name = "btnZoomIn";
            btnZoomIn.Size = new Size(23, 23);
            btnZoomIn.ToolTipText = "Vergrößern";
            btnZoomIn.Click += BtnZoomIn_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 26);
            // 
            // btnIsolate
            // 
            btnIsolate.Enabled = false;
            btnIsolate.Name = "btnIsolate";
            btnIsolate.Size = new Size(74, 23);
            btnIsolate.Text = "&Freistellen";
            btnIsolate.ToolTipText = "Außerhalb der Auswahl wird weiß — die Bildgröße (z.B. A4) bleibt erhalten";
            btnIsolate.Click += BtnIsolate_Click;
            // 
            // btnCropAction
            // 
            btnCropAction.Enabled = false;
            btnCropAction.Name = "btnCropAction";
            btnCropAction.Size = new Size(90, 23);
            btnCropAction.Text = "&Zuschneiden";
            btnCropAction.ToolTipText = "Das Bild wird auf die Auswahl verkleinert";
            btnCropAction.Click += BtnCropAction_Click;
            // 
            // btnRemove
            // 
            btnRemove.Enabled = false;
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new Size(97, 23);
            btnRemove.Text = "&Ausschneiden";
            btnRemove.ToolTipText = "Die Auswahl wird aus dem Bild entfernt (weiß) — die Bildgröße bleibt erhalten";
            btnRemove.Click += BtnRemove_Click;
            // 
            // btnApply
            // 
            btnApply.Alignment = ToolStripItemAlignment.Right;
            btnApply.Enabled = false;
            btnApply.Name = "btnApply";
            btnApply.Size = new Size(93, 23);
            btnApply.Text = "&Übernehmen";
            btnApply.ToolTipText = "Ergebnis in die Seite übernehmen (Enter)";
            btnApply.Click += BtnApply_Click;
            // 
            // btnCancel
            // 
            btnCancel.Alignment = ToolStripItemAlignment.Right;
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(79, 23);
            btnCancel.Text = "Abbrechen";
            btnCancel.ToolTipText = "Alle Aktionen verwerfen und schließen (Esc)";
            btnCancel.Click += BtnCancel_Click;
            // 
            // btnSaveAsNew
            // 
            btnSaveAsNew.Alignment = ToolStripItemAlignment.Right;
            btnSaveAsNew.Enabled = false;
            btnSaveAsNew.Name = "btnSaveAsNew";
            btnSaveAsNew.Size = new Size(160, 23);
            btnSaveAsNew.Text = "Als &neue Seite speichern";
            btnSaveAsNew.ToolTipText = "Das Ergebnis wird als zusätzliche Seite hinter der bearbeiteten eingefügt — danach zeigt der Dialog wieder das Original (z.B. um mehrere Fotos zu vereinzeln)";
            btnSaveAsNew.Click += BtnSaveAsNew_Click;
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusStrip.Location = new Point(0, 539);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(884, 22);
            statusStrip.TabIndex = 2;
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(395, 17);
            statusLabel.Text = "Rahmen aufziehen oder Griffe verschieben — Esc schließt ohne Änderung";
            // 
            // scrollPanel
            // 
            scrollPanel.AutoScroll = true;
            scrollPanel.BackColor = Color.FromArgb(48, 48, 48);
            scrollPanel.Controls.Add(pictureBox);
            scrollPanel.Dock = DockStyle.Fill;
            scrollPanel.Location = new Point(0, 26);
            scrollPanel.Name = "scrollPanel";
            scrollPanel.Size = new Size(884, 513);
            scrollPanel.TabIndex = 1;
            scrollPanel.Resize += ScrollPanel_Resize;
            // 
            // pictureBox
            // 
            pictureBox.BackColor = Color.FromArgb(48, 48, 48);
            pictureBox.Location = new Point(0, 0);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new Size(100, 50);
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.TabIndex = 0;
            pictureBox.TabStop = false;
            pictureBox.Paint += PictureBox_Paint;
            pictureBox.MouseDown += PictureBox_MouseDown;
            pictureBox.MouseMove += PictureBox_MouseMove;
            pictureBox.MouseUp += PictureBox_MouseUp;
            pictureBox.Resize += PictureBox_Resize;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Alignment = ToolStripItemAlignment.Right;
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 26);
            // 
            // CropForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(884, 561);
            Controls.Add(scrollPanel);
            Controls.Add(statusStrip);
            Controls.Add(toolStrip);
            MinimizeBox = false;
            MinimumSize = new Size(560, 400);
            Name = "CropForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Zuschneiden";
            Shown += CropForm_Shown;
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            scrollPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripLabel labelZoom;
        private System.Windows.Forms.ToolStripComboBox comboZoom;
        private System.Windows.Forms.ToolStripButton btnZoomOut;
        private System.Windows.Forms.ToolStripButton btnZoomIn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnIsolate;
        private System.Windows.Forms.ToolStripButton btnCropAction;
        private System.Windows.Forms.ToolStripButton btnRemove;
        private System.Windows.Forms.ToolStripButton btnSaveAsNew;
        private System.Windows.Forms.ToolStripButton btnApply;
        private System.Windows.Forms.ToolStripButton btnCancel;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.Panel scrollPanel;
        private System.Windows.Forms.PictureBox pictureBox;
        private ToolStripSeparator toolStripSeparator2;
    }
}
