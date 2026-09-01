namespace ScanView.Forms
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
            menuStrip = new MenuStrip();
            menuAction = new ToolStripMenuItem();
            menuActionNew = new ToolStripMenuItem();
            menuActionImport = new ToolStripMenuItem();
            menuActionScan = new ToolStripMenuItem();
            menuActionSave = new ToolStripMenuItem();
            menuActionPrint = new ToolStripMenuItem();
            menuActionCopyMode = new ToolStripMenuItem();
            menuActionSeparator = new ToolStripSeparator();
            menuActionClose = new ToolStripMenuItem();
            menuEdit = new ToolStripMenuItem();
            menuEditCut = new ToolStripMenuItem();
            menuEditCopy = new ToolStripMenuItem();
            menuEditPaste = new ToolStripMenuItem();
            menuEditDelete = new ToolStripMenuItem();
            menuEditSeparator1 = new ToolStripSeparator();
            menuEditCrop = new ToolStripMenuItem();
            menuEditRotateLeft = new ToolStripMenuItem();
            menuEditRotate180 = new ToolStripMenuItem();
            menuEditRotateRight = new ToolStripMenuItem();
            menuEditSeparator2 = new ToolStripSeparator();
            menuEditBacks = new ToolStripMenuItem();
            menuEditReverse = new ToolStripMenuItem();
            menuView = new ToolStripMenuItem();
            menuViewFitWidth = new ToolStripMenuItem();
            menuViewFitPage = new ToolStripMenuItem();
            menuViewTwoPages = new ToolStripMenuItem();
            menuViewIcons = new ToolStripMenuItem();
            menuViewSeparator1 = new ToolStripSeparator();
            menuViewZoomIn = new ToolStripMenuItem();
            menuViewZoomOut = new ToolStripMenuItem();
            menuViewSeparator2 = new ToolStripSeparator();
            menuViewFullScreen = new ToolStripMenuItem();
            menuExtras = new ToolStripMenuItem();
            menuExtrasScanner = new ToolStripMenuItem();
            menuExtrasOptions = new ToolStripMenuItem();
            menuHelp = new ToolStripMenuItem();
            menuHelpShortcuts = new ToolStripMenuItem();
            menuHelpAbout = new ToolStripMenuItem();
            toolStrip = new ToolStrip();
            splitScan = new ToolStripSplitButton();
            btnSave = new ToolStripButton();
            btnPrint = new ToolStripButton();
            btnNew = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            btnMoveLeft = new ToolStripButton();
            btnMoveRight = new ToolStripButton();
            btnRemove = new ToolStripButton();
            btnCrop = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            btnCopyMode = new ToolStripButton();
            toolStripSeparatorRight = new ToolStripSeparator();
            panelSettings = new Panel();
            labelSettings = new Label();
            labelDpi = new Label();
            comboDpi = new ComboBox();
            labelColor = new Label();
            comboColor = new ComboBox();
            labelArea = new Label();
            comboArea = new ComboBox();
            labelFeed = new Label();
            comboFeed = new ComboBox();
            labelOcr = new Label();
            comboOcr = new ComboBox();
            labelBrightness = new Label();
            trackBrightness = new TrackBar();
            panelCopyMode = new Panel();
            labelCopyTitle = new Label();
            labelCopyPrinter = new Label();
            comboCopyPrinter = new ComboBox();
            labelCopyPaper = new Label();
            comboCopyPaper = new ComboBox();
            labelCopySource = new Label();
            comboCopySource = new ComboBox();
            labelCopyDuplex = new Label();
            comboCopyDuplex = new ComboBox();
            labelCopyCount = new Label();
            numCopies = new NumericUpDown();
            chkCopyColor = new CheckBox();
            chkCopyFit = new CheckBox();
            flowPanel = new FlowLayoutPanel();
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            menuStrip.SuspendLayout();
            toolStrip.SuspendLayout();
            panelSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBrightness).BeginInit();
            panelCopyMode.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCopies).BeginInit();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip
            // 
            menuStrip.Items.AddRange(new ToolStripItem[] { menuAction, menuEdit, menuView, menuExtras, menuHelp });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.ShowItemToolTips = true;
            menuStrip.Size = new Size(984, 24);
            menuStrip.TabIndex = 4;
            // 
            // menuAction
            // 
            menuAction.DropDownItems.AddRange(new ToolStripItem[] { menuActionNew, menuActionImport, menuActionScan, menuActionSave, menuActionPrint, menuActionCopyMode, menuActionSeparator, menuActionClose });
            menuAction.Name = "menuAction";
            menuAction.Size = new Size(54, 20);
            menuAction.Text = "&Aktion";
            // 
            // menuActionNew
            // 
            menuActionNew.Enabled = false;
            menuActionNew.Name = "menuActionNew";
            menuActionNew.ShortcutKeys = Keys.F9;
            menuActionNew.Size = new Size(187, 22);
            menuActionNew.Text = "&Neu";
            menuActionNew.ToolTipText = "Seitenübersicht leeren";
            menuActionNew.Click += BtnNew_Click;
            // 
            // menuActionImport
            // 
            menuActionImport.Name = "menuActionImport";
            menuActionImport.ShortcutKeyDisplayString = "Strg+I";
            menuActionImport.ShortcutKeys = Keys.Control | Keys.I;
            menuActionImport.Size = new Size(187, 22);
            menuActionImport.Text = "&Importieren …";
            menuActionImport.ToolTipText = "Bilddateien als Seiten in die Übersicht aufnehmen";
            menuActionImport.Click += MenuImport_Click;
            // 
            // menuActionScan
            // 
            menuActionScan.Name = "menuActionScan";
            menuActionScan.ShortcutKeys = Keys.F4;
            menuActionScan.Size = new Size(187, 22);
            menuActionScan.Text = "&Scannen";
            menuActionScan.Click += SplitScan_ButtonClick;
            // 
            // menuActionSave
            // 
            menuActionSave.Enabled = false;
            menuActionSave.Name = "menuActionSave";
            menuActionSave.ShortcutKeys = Keys.F5;
            menuActionSave.Size = new Size(187, 22);
            menuActionSave.Text = "S&peichern …";
            menuActionSave.Click += BtnSave_Click;
            // 
            // menuActionPrint
            // 
            menuActionPrint.Enabled = false;
            menuActionPrint.Name = "menuActionPrint";
            menuActionPrint.ShortcutKeys = Keys.F6;
            menuActionPrint.Size = new Size(187, 22);
            menuActionPrint.Text = "&Drucken …";
            menuActionPrint.Click += BtnPrint_Click;
            // 
            // menuActionCopyMode
            // 
            menuActionCopyMode.Name = "menuActionCopyMode";
            menuActionCopyMode.ShortcutKeys = Keys.F7;
            menuActionCopyMode.Size = new Size(187, 22);
            menuActionCopyMode.Text = "Kopier&modus";
            menuActionCopyMode.ToolTipText = "Scans direkt drucken — der Scanner wird zum Kopierer";
            menuActionCopyMode.Click += BtnCopyMode_Click;
            // 
            // menuActionSeparator
            // 
            menuActionSeparator.Name = "menuActionSeparator";
            menuActionSeparator.Size = new Size(184, 6);
            // 
            // menuActionClose
            // 
            menuActionClose.Name = "menuActionClose";
            menuActionClose.Size = new Size(187, 22);
            menuActionClose.Text = "Schließen";
            menuActionClose.Click += MenuClose_Click;
            // 
            // menuEdit
            // 
            menuEdit.DropDownItems.AddRange(new ToolStripItem[] { menuEditCut, menuEditCopy, menuEditPaste, menuEditDelete, menuEditSeparator1, menuEditCrop, menuEditRotateLeft, menuEditRotate180, menuEditRotateRight, menuEditSeparator2, menuEditBacks, menuEditReverse });
            menuEdit.Name = "menuEdit";
            menuEdit.Size = new Size(75, 20);
            menuEdit.Text = "&Bearbeiten";
            // 
            // menuEditCut
            // 
            menuEditCut.Enabled = false;
            menuEditCut.Name = "menuEditCut";
            menuEditCut.ShortcutKeyDisplayString = "Strg+X";
            menuEditCut.ShortcutKeys = Keys.Control | Keys.X;
            menuEditCut.Size = new Size(260, 22);
            menuEditCut.Text = "&Ausschneiden";
            menuEditCut.Click += MenuEditCut_Click;
            // 
            // menuEditCopy
            // 
            menuEditCopy.Enabled = false;
            menuEditCopy.Name = "menuEditCopy";
            menuEditCopy.ShortcutKeyDisplayString = "Strg+C";
            menuEditCopy.ShortcutKeys = Keys.Control | Keys.C;
            menuEditCopy.Size = new Size(260, 22);
            menuEditCopy.Text = "&Kopieren";
            menuEditCopy.Click += MenuEditCopy_Click;
            // 
            // menuEditPaste
            // 
            menuEditPaste.Enabled = false;
            menuEditPaste.Name = "menuEditPaste";
            menuEditPaste.ShortcutKeyDisplayString = "Strg+V";
            menuEditPaste.ShortcutKeys = Keys.Control | Keys.V;
            menuEditPaste.Size = new Size(260, 22);
            menuEditPaste.Text = "Ein&fügen";
            menuEditPaste.Click += MenuEditPaste_Click;
            // 
            // menuEditDelete
            // 
            menuEditDelete.Enabled = false;
            menuEditDelete.Name = "menuEditDelete";
            menuEditDelete.ShortcutKeyDisplayString = "Entf";
            menuEditDelete.ShortcutKeys = Keys.Delete;
            menuEditDelete.Size = new Size(260, 22);
            menuEditDelete.Text = "&Löschen";
            menuEditDelete.Click += BtnRemove_Click;
            // 
            // menuEditSeparator1
            // 
            menuEditSeparator1.Name = "menuEditSeparator1";
            menuEditSeparator1.Size = new Size(257, 6);
            // 
            // menuEditCrop
            // 
            menuEditCrop.Enabled = false;
            menuEditCrop.Name = "menuEditCrop";
            menuEditCrop.ShortcutKeys = Keys.F10;
            menuEditCrop.Size = new Size(260, 22);
            menuEditCrop.Text = "&Zuschneiden …";
            menuEditCrop.Click += MenuEditCrop_Click;
            // 
            // menuEditRotateLeft
            // 
            menuEditRotateLeft.Enabled = false;
            menuEditRotateLeft.Name = "menuEditRotateLeft";
            menuEditRotateLeft.ShortcutKeyDisplayString = "Strg+L";
            menuEditRotateLeft.ShortcutKeys = Keys.Control | Keys.L;
            menuEditRotateLeft.Size = new Size(260, 22);
            menuEditRotateLeft.Text = "Drehen nach &links";
            menuEditRotateLeft.Click += MenuEditRotateLeft_Click;
            // 
            // menuEditRotate180
            // 
            menuEditRotate180.Enabled = false;
            menuEditRotate180.Name = "menuEditRotate180";
            menuEditRotate180.ShortcutKeyDisplayString = "Strg+Umschalt+R";
            menuEditRotate180.ShortcutKeys = Keys.Control | Keys.Shift | Keys.R;
            menuEditRotate180.Size = new Size(260, 22);
            menuEditRotate180.Text = "Drehen um 1&80°";
            menuEditRotate180.Click += MenuEditRotate180_Click;
            // 
            // menuEditRotateRight
            // 
            menuEditRotateRight.Enabled = false;
            menuEditRotateRight.Name = "menuEditRotateRight";
            menuEditRotateRight.ShortcutKeyDisplayString = "Strg+R";
            menuEditRotateRight.ShortcutKeys = Keys.Control | Keys.R;
            menuEditRotateRight.Size = new Size(260, 22);
            menuEditRotateRight.Text = "Drehen nach &rechts";
            menuEditRotateRight.Click += MenuEditRotateRight_Click;
            // 
            // menuEditSeparator2
            // 
            menuEditSeparator2.Name = "menuEditSeparator2";
            menuEditSeparator2.Size = new Size(257, 6);
            // 
            // menuEditBacks
            // 
            menuEditBacks.Enabled = false;
            menuEditBacks.Name = "menuEditBacks";
            menuEditBacks.ShortcutKeyDisplayString = "Strg+D";
            menuEditBacks.ShortcutKeys = Keys.Control | Keys.D;
            menuEditBacks.Size = new Size(260, 22);
            menuEditBacks.Text = "Rück&seiten einfügen";
            menuEditBacks.ToolTipText = "Zweite Hälfte der Seiten (Rückseiten in umgekehrter Reihenfolge) hinter die Vorderseiten einsortieren";
            menuEditBacks.Click += MenuEditBacks_Click;
            // 
            // menuEditReverse
            // 
            menuEditReverse.Enabled = false;
            menuEditReverse.Name = "menuEditReverse";
            menuEditReverse.ShortcutKeyDisplayString = "Strg+U";
            menuEditReverse.ShortcutKeys = Keys.Control | Keys.U;
            menuEditReverse.Size = new Size(260, 22);
            menuEditReverse.Text = "Sortierung &umkehren";
            menuEditReverse.Click += MenuEditReverse_Click;
            // 
            // menuView
            // 
            menuView.DropDownItems.AddRange(new ToolStripItem[] { menuViewFitWidth, menuViewFitPage, menuViewTwoPages, menuViewIcons, menuViewSeparator1, menuViewZoomIn, menuViewZoomOut, menuViewSeparator2, menuViewFullScreen });
            menuView.Name = "menuView";
            menuView.Size = new Size(59, 20);
            menuView.Text = "A&nsicht";
            // 
            // menuViewFitWidth
            // 
            menuViewFitWidth.Name = "menuViewFitWidth";
            menuViewFitWidth.ShortcutKeyDisplayString = "Strg+1";
            menuViewFitWidth.ShortcutKeys = Keys.Control | Keys.D1;
            menuViewFitWidth.Size = new Size(198, 22);
            menuViewFitWidth.Text = "&Optimale Breite";
            menuViewFitWidth.Click += MenuViewFitWidth_Click;
            // 
            // menuViewFitPage
            // 
            menuViewFitPage.Name = "menuViewFitPage";
            menuViewFitPage.ShortcutKeyDisplayString = "Strg+2";
            menuViewFitPage.ShortcutKeys = Keys.Control | Keys.D2;
            menuViewFitPage.Size = new Size(198, 22);
            menuViewFitPage.Text = "&Ganze Seite";
            menuViewFitPage.Click += MenuViewFitPage_Click;
            // 
            // menuViewTwoPages
            // 
            menuViewTwoPages.Name = "menuViewTwoPages";
            menuViewTwoPages.ShortcutKeyDisplayString = "Strg+3";
            menuViewTwoPages.ShortcutKeys = Keys.Control | Keys.D3;
            menuViewTwoPages.Size = new Size(198, 22);
            menuViewTwoPages.Text = "&Zwei Seiten";
            menuViewTwoPages.Click += MenuViewTwoPages_Click;
            // 
            // menuViewIcons
            // 
            menuViewIcons.Name = "menuViewIcons";
            menuViewIcons.ShortcutKeyDisplayString = "Strg+4";
            menuViewIcons.ShortcutKeys = Keys.Control | Keys.D4;
            menuViewIcons.Size = new Size(198, 22);
            menuViewIcons.Text = "&Symbole";
            menuViewIcons.Click += MenuViewIcons_Click;
            // 
            // menuViewSeparator1
            // 
            menuViewSeparator1.Name = "menuViewSeparator1";
            menuViewSeparator1.Size = new Size(195, 6);
            // 
            // menuViewZoomIn
            // 
            menuViewZoomIn.Name = "menuViewZoomIn";
            menuViewZoomIn.ShortcutKeyDisplayString = "Strg++";
            menuViewZoomIn.ShortcutKeys = Keys.Control | Keys.Oemplus;
            menuViewZoomIn.Size = new Size(198, 22);
            menuViewZoomIn.Text = "&Vergrößern";
            menuViewZoomIn.Click += BtnZoomIn_Click;
            // 
            // menuViewZoomOut
            // 
            menuViewZoomOut.Name = "menuViewZoomOut";
            menuViewZoomOut.ShortcutKeyDisplayString = "Strg+−";
            menuViewZoomOut.ShortcutKeys = Keys.Control | Keys.OemMinus;
            menuViewZoomOut.Size = new Size(198, 22);
            menuViewZoomOut.Text = "Ver&kleinern";
            menuViewZoomOut.Click += BtnZoomOut_Click;
            // 
            // menuViewSeparator2
            // 
            menuViewSeparator2.Name = "menuViewSeparator2";
            menuViewSeparator2.Size = new Size(195, 6);
            // 
            // menuViewFullScreen
            // 
            menuViewFullScreen.Name = "menuViewFullScreen";
            menuViewFullScreen.ShortcutKeys = Keys.F11;
            menuViewFullScreen.Size = new Size(198, 22);
            menuViewFullScreen.Text = "Ganzer &Bildschirm";
            menuViewFullScreen.Click += MenuViewFullScreen_Click;
            // 
            // menuExtras
            // 
            menuExtras.DropDownItems.AddRange(new ToolStripItem[] { menuExtrasOptions, menuExtrasScanner });
            menuExtras.Name = "menuExtras";
            menuExtras.Size = new Size(49, 20);
            menuExtras.Text = "E&xtras";
            // 
            // menuExtrasScanner
            // 
            menuExtrasScanner.Name = "menuExtrasScanner";
            menuExtrasScanner.Size = new Size(180, 22);
            menuExtrasScanner.Text = "&Scanner …";
            menuExtrasScanner.ToolTipText = "Scanner wählen und Gerätetasten konfigurieren";
            menuExtrasScanner.Click += MenuExtrasScanner_Click;
            // 
            // menuExtrasOptions
            // 
            menuExtrasOptions.Name = "menuExtrasOptions";
            menuExtrasOptions.ShortcutKeyDisplayString = "Strg+,";
            menuExtrasOptions.ShortcutKeys = Keys.Control | Keys.Oemcomma;
            menuExtrasOptions.Size = new Size(180, 22);
            menuExtrasOptions.Text = "&Optionen …";
            menuExtrasOptions.Click += MenuExtrasOptions_Click;
            // 
            // menuHelp
            // 
            menuHelp.DropDownItems.AddRange(new ToolStripItem[] { menuHelpShortcuts, menuHelpAbout });
            menuHelp.Name = "menuHelp";
            menuHelp.Size = new Size(24, 20);
            menuHelp.Text = "?";
            // 
            // menuHelpShortcuts
            // 
            menuHelpShortcuts.Name = "menuHelpShortcuts";
            menuHelpShortcuts.ShortcutKeys = Keys.F1;
            menuHelpShortcuts.Size = new Size(194, 22);
            menuHelpShortcuts.Text = "&Hilfe (Tastenkürzel)";
            menuHelpShortcuts.ToolTipText = "Tastenkürzel-Übersicht als PDF erstellen und anzeigen";
            menuHelpShortcuts.Click += MenuHelpShortcuts_Click;
            // 
            // menuHelpAbout
            // 
            menuHelpAbout.Name = "menuHelpAbout";
            menuHelpAbout.Size = new Size(194, 22);
            menuHelpAbout.Text = "&Info …";
            menuHelpAbout.Click += MenuHelpAbout_Click;
            // 
            // toolStrip
            // 
            toolStrip.AutoSize = false;
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.Items.AddRange(new ToolStripItem[] { splitScan, btnSave, btnPrint, btnNew, toolStripSeparator1, btnMoveLeft, btnMoveRight, btnRemove, btnCrop, toolStripSeparator2, btnCopyMode, toolStripSeparatorRight });
            toolStrip.Location = new Point(0, 24);
            toolStrip.Name = "toolStrip";
            toolStrip.Padding = new Padding(0);
            toolStrip.Size = new Size(984, 60);
            toolStrip.TabIndex = 0;
            toolStrip.Paint += ToolStrip_Paint;
            // 
            // splitScan
            // 
            splitScan.AutoSize = false;
            splitScan.DisplayStyle = ToolStripItemDisplayStyle.Text;
            splitScan.DropDownButtonWidth = 28;
            splitScan.Name = "splitScan";
            splitScan.Size = new Size(150, 57);
            splitScan.Text = "&Scannen";
            splitScan.ToolTipText = "Seite vom gewählten Scanner holen (F4) — Pfeil: Scanner auswählen";
            splitScan.ButtonClick += SplitScan_ButtonClick;
            splitScan.DropDownOpening += SplitScan_DropDownOpening;
            // 
            // btnSave
            // 
            btnSave.AutoSize = false;
            btnSave.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnSave.Enabled = false;
            btnSave.Margin = new Padding(8, 1, 0, 2);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(90, 57);
            btnSave.Text = "S&peichern";
            btnSave.ToolTipText = "Alle Seiten per Texterkennung als durchsuchbare PDF speichern (F5)";
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
            btnPrint.ToolTipText = "Alle Seiten drucken (F6)";
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
            btnNew.ToolTipText = "Seitenübersicht leeren (F9)";
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
            btnMoveLeft.ToolTipText = "Markierte Seite nach vorn schieben (Alt+←)";
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
            btnMoveRight.ToolTipText = "Markierte Seite nach hinten schieben (Alt+→)";
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
            btnRemove.ToolTipText = "Markierte Seite aus der Übersicht entfernen (Entf)";
            btnRemove.Click += BtnRemove_Click;
            // 
            // btnCrop
            // 
            btnCrop.AutoSize = false;
            btnCrop.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnCrop.Enabled = false;
            btnCrop.Name = "btnCrop";
            btnCrop.Size = new Size(100, 57);
            btnCrop.Text = "&Zuschneiden";
            btnCrop.ToolTipText = "Markierte Seite zuschneiden (F10)";
            btnCrop.Click += MenuEditCrop_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 60);
            // 
            // btnCopyMode
            // 
            btnCopyMode.Alignment = ToolStripItemAlignment.Right;
            btnCopyMode.AutoSize = false;
            btnCopyMode.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnCopyMode.Name = "btnCopyMode";
            btnCopyMode.Size = new Size(130, 57);
            btnCopyMode.Text = "&Kopiermodus";
            btnCopyMode.ToolTipText = "Scans direkt drucken — der Scanner wird zum Kopierer (F7)";
            btnCopyMode.Click += BtnCopyMode_Click;
            // 
            // toolStripSeparatorRight
            // 
            toolStripSeparatorRight.Alignment = ToolStripItemAlignment.Right;
            toolStripSeparatorRight.Name = "toolStripSeparatorRight";
            toolStripSeparatorRight.Size = new Size(6, 60);
            // 
            // panelSettings
            // 
            panelSettings.BackColor = Color.FromArgb(233, 241, 248);
            panelSettings.Controls.Add(labelSettings);
            panelSettings.Controls.Add(labelDpi);
            panelSettings.Controls.Add(comboDpi);
            panelSettings.Controls.Add(labelColor);
            panelSettings.Controls.Add(comboColor);
            panelSettings.Controls.Add(labelArea);
            panelSettings.Controls.Add(comboArea);
            panelSettings.Controls.Add(labelFeed);
            panelSettings.Controls.Add(comboFeed);
            panelSettings.Controls.Add(labelOcr);
            panelSettings.Controls.Add(comboOcr);
            panelSettings.Controls.Add(labelBrightness);
            panelSettings.Controls.Add(trackBrightness);
            panelSettings.Dock = DockStyle.Left;
            panelSettings.Location = new Point(0, 84);
            panelSettings.Name = "panelSettings";
            panelSettings.Padding = new Padding(8);
            panelSettings.Size = new Size(150, 355);
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
            labelColor.Size = new Size(70, 15);
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
            // labelArea
            // 
            labelArea.AutoSize = true;
            labelArea.Location = new Point(8, 148);
            labelArea.Name = "labelArea";
            labelArea.Size = new Size(74, 15);
            labelArea.TabIndex = 5;
            labelArea.Text = "Scan&bereich:";
            // 
            // comboArea
            // 
            comboArea.DropDownStyle = ComboBoxStyle.DropDownList;
            comboArea.Items.AddRange(new object[] { "maximal", "A4", "A5", "A6", "US-Letter", "Visitenkarte" });
            comboArea.Location = new Point(8, 166);
            comboArea.Name = "comboArea";
            comboArea.Size = new Size(132, 23);
            comboArea.TabIndex = 6;
            // 
            // labelFeed
            // 
            labelFeed.AutoSize = true;
            labelFeed.Location = new Point(8, 200);
            labelFeed.Name = "labelFeed";
            labelFeed.Size = new Size(77, 15);
            labelFeed.TabIndex = 9;
            labelFeed.Text = "Papier&zufuhr:";
            // 
            // comboFeed
            // 
            comboFeed.DropDownStyle = ComboBoxStyle.DropDownList;
            comboFeed.Items.AddRange(new object[] { "Flachbett", "Automatischer Einzug" });
            comboFeed.Location = new Point(8, 218);
            comboFeed.Name = "comboFeed";
            comboFeed.Size = new Size(132, 23);
            comboFeed.TabIndex = 10;
            // 
            // labelOcr
            // 
            labelOcr.AutoSize = true;
            labelOcr.Location = new Point(8, 252);
            labelOcr.Name = "labelOcr";
            labelOcr.Size = new Size(88, 15);
            labelOcr.TabIndex = 11;
            labelOcr.Text = "&Texterkennung:";
            // 
            // comboOcr
            // 
            comboOcr.DropDownStyle = ComboBoxStyle.DropDownList;
            comboOcr.Location = new Point(8, 270);
            comboOcr.Name = "comboOcr";
            comboOcr.Size = new Size(132, 23);
            comboOcr.TabIndex = 12;
            // 
            // labelBrightness
            // 
            labelBrightness.AutoSize = true;
            labelBrightness.Location = new Point(8, 304);
            labelBrightness.Name = "labelBrightness";
            labelBrightness.Size = new Size(69, 15);
            labelBrightness.TabIndex = 7;
            labelBrightness.Text = "&Helligkeit: 0";
            // 
            // trackBrightness
            // 
            trackBrightness.AutoSize = false;
            trackBrightness.LargeChange = 25;
            trackBrightness.Location = new Point(4, 322);
            trackBrightness.Maximum = 100;
            trackBrightness.Minimum = -100;
            trackBrightness.Name = "trackBrightness";
            trackBrightness.Size = new Size(142, 30);
            trackBrightness.SmallChange = 5;
            trackBrightness.TabIndex = 13;
            trackBrightness.TickFrequency = 25;
            trackBrightness.ValueChanged += TrackBrightness_ValueChanged;
            // 
            // panelCopyMode
            // 
            panelCopyMode.BackColor = Color.FromArgb(233, 241, 248);
            panelCopyMode.BorderStyle = BorderStyle.FixedSingle;
            panelCopyMode.Controls.Add(labelCopyTitle);
            panelCopyMode.Controls.Add(labelCopyPrinter);
            panelCopyMode.Controls.Add(comboCopyPrinter);
            panelCopyMode.Controls.Add(labelCopyPaper);
            panelCopyMode.Controls.Add(comboCopyPaper);
            panelCopyMode.Controls.Add(labelCopySource);
            panelCopyMode.Controls.Add(comboCopySource);
            panelCopyMode.Controls.Add(labelCopyDuplex);
            panelCopyMode.Controls.Add(comboCopyDuplex);
            panelCopyMode.Controls.Add(labelCopyCount);
            panelCopyMode.Controls.Add(numCopies);
            panelCopyMode.Controls.Add(chkCopyColor);
            panelCopyMode.Controls.Add(chkCopyFit);
            panelCopyMode.Dock = DockStyle.Fill;
            panelCopyMode.Location = new Point(150, 84);
            panelCopyMode.Name = "panelCopyMode";
            panelCopyMode.Padding = new Padding(16);
            panelCopyMode.Size = new Size(834, 355);
            panelCopyMode.TabIndex = 3;
            panelCopyMode.Visible = false;
            // 
            // labelCopyTitle
            // 
            labelCopyTitle.AutoSize = true;
            labelCopyTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelCopyTitle.Location = new Point(16, 12);
            labelCopyTitle.Name = "labelCopyTitle";
            labelCopyTitle.Size = new Size(276, 15);
            labelCopyTitle.TabIndex = 0;
            labelCopyTitle.Text = "Kopiermodus — jeder Scan wird direkt gedruckt";
            // 
            // labelCopyPrinter
            // 
            labelCopyPrinter.AutoSize = true;
            labelCopyPrinter.Location = new Point(16, 44);
            labelCopyPrinter.Name = "labelCopyPrinter";
            labelCopyPrinter.Size = new Size(51, 15);
            labelCopyPrinter.TabIndex = 1;
            labelCopyPrinter.Text = "&Drucker:";
            // 
            // comboCopyPrinter
            // 
            comboCopyPrinter.DropDownStyle = ComboBoxStyle.DropDownList;
            comboCopyPrinter.Location = new Point(16, 62);
            comboCopyPrinter.Name = "comboCopyPrinter";
            comboCopyPrinter.Size = new Size(267, 23);
            comboCopyPrinter.TabIndex = 2;
            comboCopyPrinter.SelectedIndexChanged += ComboCopyPrinter_SelectedIndexChanged;
            // 
            // labelCopyPaper
            // 
            labelCopyPaper.AutoSize = true;
            labelCopyPaper.Location = new Point(16, 96);
            labelCopyPaper.Name = "labelCopyPaper";
            labelCopyPaper.Size = new Size(79, 15);
            labelCopyPaper.TabIndex = 3;
            labelCopyPaper.Text = "&Papierformat:";
            // 
            // comboCopyPaper
            // 
            comboCopyPaper.DropDownStyle = ComboBoxStyle.DropDownList;
            comboCopyPaper.Location = new Point(16, 114);
            comboCopyPaper.Name = "comboCopyPaper";
            comboCopyPaper.Size = new Size(267, 23);
            comboCopyPaper.TabIndex = 4;
            // 
            // labelCopySource
            // 
            labelCopySource.AutoSize = true;
            labelCopySource.Location = new Point(16, 148);
            labelCopySource.Name = "labelCopySource";
            labelCopySource.Size = new Size(77, 15);
            labelCopySource.TabIndex = 5;
            labelCopySource.Text = "Papier&zufuhr:";
            // 
            // comboCopySource
            // 
            comboCopySource.DropDownStyle = ComboBoxStyle.DropDownList;
            comboCopySource.Location = new Point(16, 166);
            comboCopySource.Name = "comboCopySource";
            comboCopySource.Size = new Size(267, 23);
            comboCopySource.TabIndex = 6;
            // 
            // labelCopyDuplex
            // 
            labelCopyDuplex.AutoSize = true;
            labelCopyDuplex.Location = new Point(16, 200);
            labelCopyDuplex.Name = "labelCopyDuplex";
            labelCopyDuplex.Size = new Size(105, 15);
            labelCopyDuplex.TabIndex = 7;
            labelCopyDuplex.Text = "&Beidseitiger Druck:";
            // 
            // comboCopyDuplex
            // 
            comboCopyDuplex.DropDownStyle = ComboBoxStyle.DropDownList;
            comboCopyDuplex.Items.AddRange(new object[] { "Einseitig", "Beidseitig (lange Kante)", "Beidseitig (kurze Kante)" });
            comboCopyDuplex.Location = new Point(16, 218);
            comboCopyDuplex.Name = "comboCopyDuplex";
            comboCopyDuplex.Size = new Size(267, 23);
            comboCopyDuplex.TabIndex = 8;
            // 
            // labelCopyCount
            // 
            labelCopyCount.AutoSize = true;
            labelCopyCount.Location = new Point(16, 252);
            labelCopyCount.Name = "labelCopyCount";
            labelCopyCount.Size = new Size(64, 15);
            labelCopyCount.TabIndex = 9;
            labelCopyCount.Text = "E&xemplare:";
            // 
            // numCopies
            // 
            numCopies.Location = new Point(16, 270);
            numCopies.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            numCopies.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numCopies.Name = "numCopies";
            numCopies.Size = new Size(60, 23);
            numCopies.TabIndex = 10;
            numCopies.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // chkCopyColor
            // 
            chkCopyColor.AutoSize = true;
            chkCopyColor.Location = new Point(96, 272);
            chkCopyColor.Name = "chkCopyColor";
            chkCopyColor.Size = new Size(105, 19);
            chkCopyColor.TabIndex = 11;
            chkCopyColor.Text = "Far&big drucken";
            // 
            // chkCopyFit
            // 
            chkCopyFit.AutoSize = true;
            chkCopyFit.Checked = true;
            chkCopyFit.CheckState = CheckState.Checked;
            chkCopyFit.Location = new Point(16, 320);
            chkCopyFit.Name = "chkCopyFit";
            chkCopyFit.Size = new Size(193, 19);
            chkCopyFit.TabIndex = 12;
            chkCopyFit.Text = "Seiten auf Druck&fläche skalieren";
            // 
            // flowPanel
            // 
            flowPanel.AllowDrop = true;
            flowPanel.AutoScroll = true;
            flowPanel.BackColor = Color.White;
            flowPanel.BorderStyle = BorderStyle.FixedSingle;
            flowPanel.Dock = DockStyle.Fill;
            flowPanel.Location = new Point(150, 84);
            flowPanel.Name = "flowPanel";
            flowPanel.Padding = new Padding(8);
            flowPanel.Size = new Size(834, 355);
            flowPanel.TabIndex = 2;
            flowPanel.DragEnter += FlowPanel_DragEnter;
            flowPanel.DragOver += FlowPanel_DragOver;
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
            statusLabel.Size = new Size(102, 17);
            statusLabel.Text = "Noch keine Seiten";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(984, 461);
            Controls.Add(flowPanel);
            Controls.Add(panelCopyMode);
            Controls.Add(panelSettings);
            Controls.Add(statusStrip);
            Controls.Add(toolStrip);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
            MinimumSize = new Size(700, 480);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ScanView";
            FormClosing += MainForm_FormClosing;
            FormClosed += MainForm_FormClosed;
            Shown += MainForm_Shown;
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            panelSettings.ResumeLayout(false);
            panelSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBrightness).EndInit();
            panelCopyMode.ResumeLayout(false);
            panelCopyMode.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCopies).EndInit();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem menuAction;
        private System.Windows.Forms.ToolStripMenuItem menuActionNew;
        private System.Windows.Forms.ToolStripMenuItem menuActionImport;
        private System.Windows.Forms.ToolStripMenuItem menuActionScan;
        private System.Windows.Forms.ToolStripMenuItem menuActionSave;
        private System.Windows.Forms.ToolStripMenuItem menuActionPrint;
        private System.Windows.Forms.ToolStripMenuItem menuActionCopyMode;
        private System.Windows.Forms.ToolStripSeparator menuActionSeparator;
        private System.Windows.Forms.ToolStripMenuItem menuActionClose;
        private System.Windows.Forms.ToolStripMenuItem menuEdit;
        private System.Windows.Forms.ToolStripMenuItem menuEditCut;
        private System.Windows.Forms.ToolStripMenuItem menuEditCopy;
        private System.Windows.Forms.ToolStripMenuItem menuEditPaste;
        private System.Windows.Forms.ToolStripMenuItem menuEditDelete;
        private System.Windows.Forms.ToolStripSeparator menuEditSeparator1;
        private System.Windows.Forms.ToolStripMenuItem menuEditCrop;
        private System.Windows.Forms.ToolStripMenuItem menuEditRotateLeft;
        private System.Windows.Forms.ToolStripMenuItem menuEditRotate180;
        private System.Windows.Forms.ToolStripMenuItem menuEditRotateRight;
        private System.Windows.Forms.ToolStripSeparator menuEditSeparator2;
        private System.Windows.Forms.ToolStripMenuItem menuEditBacks;
        private System.Windows.Forms.ToolStripMenuItem menuEditReverse;
        private System.Windows.Forms.ToolStripMenuItem menuView;
        private System.Windows.Forms.ToolStripMenuItem menuViewFitWidth;
        private System.Windows.Forms.ToolStripMenuItem menuViewFitPage;
        private System.Windows.Forms.ToolStripMenuItem menuViewTwoPages;
        private System.Windows.Forms.ToolStripMenuItem menuViewIcons;
        private System.Windows.Forms.ToolStripSeparator menuViewSeparator1;
        private System.Windows.Forms.ToolStripMenuItem menuViewZoomIn;
        private System.Windows.Forms.ToolStripMenuItem menuViewZoomOut;
        private System.Windows.Forms.ToolStripSeparator menuViewSeparator2;
        private System.Windows.Forms.ToolStripMenuItem menuViewFullScreen;
        private System.Windows.Forms.ToolStripMenuItem menuExtras;
        private System.Windows.Forms.ToolStripMenuItem menuExtrasScanner;
        private System.Windows.Forms.ToolStripMenuItem menuExtrasOptions;
        private System.Windows.Forms.ToolStripMenuItem menuHelp;
        private System.Windows.Forms.ToolStripMenuItem menuHelpShortcuts;
        private System.Windows.Forms.ToolStripMenuItem menuHelpAbout;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripSplitButton splitScan;
        private System.Windows.Forms.ToolStripButton btnSave;
        private System.Windows.Forms.ToolStripButton btnPrint;
        private System.Windows.Forms.ToolStripButton btnNew;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnMoveLeft;
        private System.Windows.Forms.ToolStripButton btnMoveRight;
        private System.Windows.Forms.ToolStripButton btnRemove;
        private System.Windows.Forms.ToolStripButton btnCrop;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorRight;
        private System.Windows.Forms.ToolStripButton btnCopyMode;
        private System.Windows.Forms.Panel panelSettings;
        private System.Windows.Forms.Panel panelCopyMode;
        private System.Windows.Forms.Label labelCopyTitle;
        private System.Windows.Forms.Label labelCopyPrinter;
        private System.Windows.Forms.ComboBox comboCopyPrinter;
        private System.Windows.Forms.Label labelCopyPaper;
        private System.Windows.Forms.ComboBox comboCopyPaper;
        private System.Windows.Forms.Label labelCopySource;
        private System.Windows.Forms.ComboBox comboCopySource;
        private System.Windows.Forms.Label labelCopyDuplex;
        private System.Windows.Forms.ComboBox comboCopyDuplex;
        private System.Windows.Forms.CheckBox chkCopyColor;
        private System.Windows.Forms.Label labelCopyCount;
        private System.Windows.Forms.NumericUpDown numCopies;
        private System.Windows.Forms.CheckBox chkCopyFit;
        private System.Windows.Forms.Label labelSettings;
        private System.Windows.Forms.Label labelDpi;
        private System.Windows.Forms.ComboBox comboDpi;
        private System.Windows.Forms.Label labelColor;
        private System.Windows.Forms.ComboBox comboColor;
        private System.Windows.Forms.Label labelArea;
        private System.Windows.Forms.ComboBox comboArea;
        private System.Windows.Forms.Label labelFeed;
        private System.Windows.Forms.ComboBox comboFeed;
        private System.Windows.Forms.Label labelOcr;
        private System.Windows.Forms.ComboBox comboOcr;
        private System.Windows.Forms.Label labelBrightness;
        private System.Windows.Forms.TrackBar trackBrightness;
        private System.Windows.Forms.FlowLayoutPanel flowPanel;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
    }
}
