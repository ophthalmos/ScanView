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
            btnScan = new ToolStripButton();
            btnTestPage = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            btnMoveLeft = new ToolStripButton();
            btnMoveRight = new ToolStripButton();
            btnRemove = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            btnCreatePdf = new ToolStripButton();
            flowPanel = new FlowLayoutPanel();
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            toolStrip.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // toolStrip
            //
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.Items.AddRange(new ToolStripItem[] { btnScan, btnTestPage, toolStripSeparator1, btnMoveLeft, btnMoveRight, btnRemove, toolStripSeparator2, btnCreatePdf });
            toolStrip.Location = new Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.Size = new Size(884, 25);
            toolStrip.TabIndex = 0;
            //
            // btnScan
            //
            btnScan.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnScan.Name = "btnScan";
            btnScan.Size = new Size(56, 22);
            btnScan.Text = "&Scannen";
            btnScan.ToolTipText = "Eine Seite über den Windows-Scandialog scannen";
            btnScan.Click += BtnScan_Click;
            //
            // btnTestPage
            //
            btnTestPage.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnTestPage.Name = "btnTestPage";
            btnTestPage.Size = new Size(62, 22);
            btnTestPage.Text = "&Testseite";
            btnTestPage.ToolTipText = "Gerenderte Testseite hinzufügen (zum Ausprobieren ohne Scanner)";
            btnTestPage.Click += BtnTestPage_Click;
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            //
            // btnMoveLeft
            //
            btnMoveLeft.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnMoveLeft.Enabled = false;
            btnMoveLeft.Name = "btnMoveLeft";
            btnMoveLeft.Size = new Size(23, 22);
            btnMoveLeft.Text = "◀";
            btnMoveLeft.ToolTipText = "Markierte Seite nach vorn schieben";
            btnMoveLeft.Click += BtnMoveLeft_Click;
            //
            // btnMoveRight
            //
            btnMoveRight.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnMoveRight.Enabled = false;
            btnMoveRight.Name = "btnMoveRight";
            btnMoveRight.Size = new Size(23, 22);
            btnMoveRight.Text = "▶";
            btnMoveRight.ToolTipText = "Markierte Seite nach hinten schieben";
            btnMoveRight.Click += BtnMoveRight_Click;
            //
            // btnRemove
            //
            btnRemove.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnRemove.Enabled = false;
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new Size(61, 22);
            btnRemove.Text = "&Entfernen";
            btnRemove.ToolTipText = "Markierte Seite aus der Übersicht entfernen";
            btnRemove.Click += BtnRemove_Click;
            //
            // toolStripSeparator2
            //
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 25);
            //
            // btnCreatePdf
            //
            btnCreatePdf.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnCreatePdf.Enabled = false;
            btnCreatePdf.Name = "btnCreatePdf";
            btnCreatePdf.Size = new Size(94, 22);
            btnCreatePdf.Text = "&PDF erstellen …";
            btnCreatePdf.ToolTipText = "Alle Seiten per Texterkennung in eine durchsuchbare PDF schreiben";
            btnCreatePdf.Click += BtnCreatePdf_Click;
            //
            // flowPanel
            //
            flowPanel.AutoScroll = true;
            flowPanel.BackColor = SystemColors.ControlDark;
            flowPanel.Dock = DockStyle.Fill;
            flowPanel.Location = new Point(0, 25);
            flowPanel.Name = "flowPanel";
            flowPanel.Padding = new Padding(8);
            flowPanel.Size = new Size(884, 414);
            flowPanel.TabIndex = 1;
            //
            // statusStrip
            //
            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusStrip.Location = new Point(0, 439);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(884, 22);
            statusStrip.TabIndex = 2;
            //
            // statusLabel
            //
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(118, 17);
            statusLabel.Text = "Noch keine Seiten";
            //
            // MainForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(884, 461);
            Controls.Add(flowPanel);
            Controls.Add(statusStrip);
            Controls.Add(toolStrip);
            MinimumSize = new Size(600, 400);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ScanTest";
            FormClosed += MainForm_FormClosed;
            toolStrip.ResumeLayout(false);
            statusStrip.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton btnScan;
        private System.Windows.Forms.ToolStripButton btnTestPage;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnMoveLeft;
        private System.Windows.Forms.ToolStripButton btnMoveRight;
        private System.Windows.Forms.ToolStripButton btnRemove;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnCreatePdf;
        private System.Windows.Forms.FlowLayoutPanel flowPanel;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
    }
}
