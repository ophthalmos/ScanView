namespace ScanTest.Forms
{
    partial class MainForm
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
            splitScan = new ToolStripSplitButton();
            btnSave = new ToolStripButton();
            btnPrint = new ToolStripButton();
            btnNew = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            btnMoveLeft = new ToolStripButton();
            btnMoveRight = new ToolStripButton();
            btnRemove = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            btnZoomOut = new ToolStripButton();
            btnZoomIn = new ToolStripButton();
            panelSettings = new Panel();
            labelSettings = new Label();
            labelDpi = new Label();
            comboDpi = new ComboBox();
            labelColor = new Label();
            comboColor = new ComboBox();
            flowPanel = new FlowLayoutPanel();
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            toolStrip.SuspendLayout();
            panelSettings.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // toolStrip
            //
            toolStrip.AutoSize = false;
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.Items.AddRange(new ToolStripItem[] { splitScan, btnSave, btnPrint, btnNew, toolStripSeparator1, btnMoveLeft, btnMoveRight, btnRemove, toolStripSeparator2, btnZoomOut, btnZoomIn });
            toolStrip.Location = new Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.Padding = new Padding(0);
            toolStrip.Size = new Size(984, 60);
            toolStrip.TabIndex = 0;
            //
            // splitScan
            //
            splitScan.AutoSize = false;
            splitScan.DisplayStyle = ToolStripItemDisplayStyle.Text;
            splitScan.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            splitScan.Name = "splitScan";
            splitScan.Size = new Size(150, 57);
            splitScan.Text = "&Scannen";
            splitScan.ToolTipText = "Seite vom gewählten Scanner holen — Pfeil: Scanner auswählen";
            splitScan.ButtonClick += SplitScan_ButtonClick;
            splitScan.DropDownOpening += SplitScan_DropDownOpening;
            //
            // btnSave
            //
            btnSave.AutoSize = false;
            btnSave.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnSave.Enabled = false;
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(90, 57);
            btnSave.Text = "S&peichern";
            btnSave.ToolTipText = "Alle Seiten per Texterkennung als durchsuchbare PDF speichern";
            btnSave.Click += BtnSave_Click;
            //
            // btnPrint
            //
            btnPrint.AutoSize = false;
            btnPrint.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnPrint.Enabled = false;
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(90, 57);
            btnPrint.Text = "&Drucken";
            btnPrint.ToolTipText = "Alle Seiten drucken";
            btnPrint.Click += BtnPrint_Click;
            //
            // btnNew
            //
            btnNew.AutoSize = false;
            btnNew.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnNew.Enabled = false;
            btnNew.Name = "btnNew";
            btnNew.Size = new Size(90, 57);
            btnNew.Text = "&Neu";
            btnNew.ToolTipText = "Seitenübersicht leeren";
            btnNew.Click += BtnNew_Click;
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 60);
            //
            // btnMoveLeft
            //
            btnMoveLeft.AutoSize = false;
            btnMoveLeft.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnMoveLeft.Enabled = false;
            btnMoveLeft.Name = "btnMoveLeft";
            btnMoveLeft.Size = new Size(32, 57);
            btnMoveLeft.Text = "◀";
            btnMoveLeft.ToolTipText = "Markierte Seite nach vorn schieben";
            btnMoveLeft.Click += BtnMoveLeft_Click;
            //
            // btnMoveRight
            //
            btnMoveRight.AutoSize = false;
            btnMoveRight.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnMoveRight.Enabled = false;
            btnMoveRight.Name = "btnMoveRight";
            btnMoveRight.Size = new Size(32, 57);
            btnMoveRight.Text = "▶";
            btnMoveRight.ToolTipText = "Markierte Seite nach hinten schieben";
            btnMoveRight.Click += BtnMoveRight_Click;
            //
            // btnRemove
            //
            btnRemove.AutoSize = false;
            btnRemove.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnRemove.Enabled = false;
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new Size(80, 57);
            btnRemove.Text = "&Entfernen";
            btnRemove.ToolTipText = "Markierte Seite aus der Übersicht entfernen";
            btnRemove.Click += BtnRemove_Click;
            //
            // toolStripSeparator2
            //
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 60);
            //
            // btnZoomOut
            //
            btnZoomOut.AutoSize = false;
            btnZoomOut.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnZoomOut.Font = new Font("Segoe UI", 12F);
            btnZoomOut.Name = "btnZoomOut";
            btnZoomOut.Size = new Size(32, 57);
            btnZoomOut.Text = "−";
            btnZoomOut.ToolTipText = "Miniaturen verkleinern";
            btnZoomOut.Click += BtnZoomOut_Click;
            //
            // btnZoomIn
            //
            btnZoomIn.AutoSize = false;
            btnZoomIn.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnZoomIn.Font = new Font("Segoe UI", 12F);
            btnZoomIn.Name = "btnZoomIn";
            btnZoomIn.Size = new Size(32, 57);
            btnZoomIn.Text = "+";
            btnZoomIn.ToolTipText = "Miniaturen vergrößern";
            btnZoomIn.Click += BtnZoomIn_Click;
            //
            // panelSettings
            //
            panelSettings.Controls.Add(labelSettings);
            panelSettings.Controls.Add(labelDpi);
            panelSettings.Controls.Add(comboDpi);
            panelSettings.Controls.Add(labelColor);
            panelSettings.Controls.Add(comboColor);
            panelSettings.Dock = DockStyle.Left;
            panelSettings.Location = new Point(0, 60);
            panelSettings.Name = "panelSettings";
            panelSettings.Padding = new Padding(8);
            panelSettings.Size = new Size(150, 379);
            panelSettings.TabIndex = 1;
            //
            // labelSettings
            //
            labelSettings.AutoSize = true;
            labelSettings.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelSettings.Location = new Point(8, 12);
            labelSettings.Name = "labelSettings";
            labelSettings.Size = new Size(112, 15);
            labelSettings.TabIndex = 0;
            labelSettings.Text = "Scan-Einstellungen";
            //
            // labelDpi
            //
            labelDpi.AutoSize = true;
            labelDpi.Location = new Point(8, 44);
            labelDpi.Name = "labelDpi";
            labelDpi.Size = new Size(65, 15);
            labelDpi.TabIndex = 1;
            labelDpi.Text = "&Auflösung:";
            //
            // comboDpi
            //
            comboDpi.DropDownStyle = ComboBoxStyle.DropDownList;
            comboDpi.Items.AddRange(new object[] { "150 dpi", "200 dpi", "300 dpi", "600 dpi" });
            comboDpi.Location = new Point(8, 62);
            comboDpi.Name = "comboDpi";
            comboDpi.Size = new Size(132, 23);
            comboDpi.TabIndex = 2;
            //
            // labelColor
            //
            labelColor.AutoSize = true;
            labelColor.Location = new Point(8, 96);
            labelColor.Name = "labelColor";
            labelColor.Size = new Size(65, 15);
            labelColor.TabIndex = 3;
            labelColor.Text = "&Farbmodus:";
            //
            // comboColor
            //
            comboColor.DropDownStyle = ComboBoxStyle.DropDownList;
            comboColor.Items.AddRange(new object[] { "Farbe", "Graustufen", "Schwarz-weiß" });
            comboColor.Location = new Point(8, 114);
            comboColor.Name = "comboColor";
            comboColor.Size = new Size(132, 23);
            comboColor.TabIndex = 4;
            //
            // flowPanel
            //
            flowPanel.AllowDrop = true;
            flowPanel.AutoScroll = true;
            flowPanel.BackColor = SystemColors.ControlDark;
            flowPanel.DragEnter += FlowPanel_DragEnter;
            flowPanel.DragOver += FlowPanel_DragOver;
            flowPanel.Dock = DockStyle.Fill;
            flowPanel.Location = new Point(150, 60);
            flowPanel.Name = "flowPanel";
            flowPanel.Padding = new Padding(8);
            flowPanel.Size = new Size(834, 379);
            flowPanel.TabIndex = 2;
            //
            // statusStrip
            //
            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusStrip.Location = new Point(0, 439);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(984, 22);
            statusStrip.TabIndex = 3;
            //
            // statusLabel
            //
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(101, 17);
            statusLabel.Text = "Noch keine Seiten";
            //
            // MainForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(984, 461);
            Controls.Add(flowPanel);
            Controls.Add(panelSettings);
            Controls.Add(statusStrip);
            Controls.Add(toolStrip);
            MinimumSize = new Size(700, 420);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ScanTest";
            FormClosed += MainForm_FormClosed;
            Shown += MainForm_Shown;
            toolStrip.ResumeLayout(false);
            panelSettings.ResumeLayout(false);
            panelSettings.PerformLayout();
            statusStrip.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripSplitButton splitScan;
        private System.Windows.Forms.ToolStripButton btnSave;
        private System.Windows.Forms.ToolStripButton btnPrint;
        private System.Windows.Forms.ToolStripButton btnNew;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnMoveLeft;
        private System.Windows.Forms.ToolStripButton btnMoveRight;
        private System.Windows.Forms.ToolStripButton btnRemove;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnZoomOut;
        private System.Windows.Forms.ToolStripButton btnZoomIn;
        private System.Windows.Forms.Panel panelSettings;
        private System.Windows.Forms.Label labelSettings;
        private System.Windows.Forms.Label labelDpi;
        private System.Windows.Forms.ComboBox comboDpi;
        private System.Windows.Forms.Label labelColor;
        private System.Windows.Forms.ComboBox comboColor;
        private System.Windows.Forms.FlowLayoutPanel flowPanel;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
    }
}
