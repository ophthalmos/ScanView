using System.Drawing.Printing;
using ScanView.Classes;

namespace ScanView.Forms;

/// <summary>ScanView: Seiten scannen (WIA), als Miniaturen ordnen und per
/// Tesseract + PDFsharp in eine durchsuchbare PDF schreiben.</summary>
public partial class MainForm : Form, IMessageFilter
{
    private const int WM_MOUSEWHEEL = 0x020A;

    /// <summary>Strg+Mausrad über der Seitenübersicht blättert durch die Zoomstufen —
    /// als Nachrichtenfilter, weil Radnachrichten sonst nur das fokussierte Control erreichen.</summary>
    public bool PreFilterMessage(ref Message m)
    {
        if (m.Msg != WM_MOUSEWHEEL || (ModifierKeys & Keys.Control) == 0) { return false; }
        // ActiveForm statt Enabled: ShowDialog deaktiviert den Owner nur nativ, die Enabled-Property bleibt true
        if (Form.ActiveForm != this || !flowPanel.Visible) { return false; } // Dialog offen bzw. Kopiermodus aktiv
        if (!flowPanel.RectangleToScreen(flowPanel.ClientRectangle).Contains(Cursor.Position)) { return false; }
        var delta = (short)((long)m.WParam >> 16);
        if (delta > 0) { BtnZoomIn_Click(this, EventArgs.Empty); }
        else { BtnZoomOut_Click(this, EventArgs.Empty); }
        return true; // nicht zusätzlich scrollen
    }

    // Zoomstufen für −/+ im A4-Verhältnis (Breite; Höhe = Breite × 1,4); die oberen Stufen für große Bildschirme
    private static readonly int[] ThumbWidths = [100, 130, 160, 200, 240, 280, 340, 410, 490, 590, 700];
    private const int IconThumbWidth = 160; // Ansicht „Symbole" und Startgröße
    private int thumbWidth = IconThumbWidth; // Ansicht-Modi dürfen von den Zoomstufen abweichen

    private readonly string sessionFolder; // Seitenablage: persistent, damit „Seiten behalten" beim Beenden möglich ist
    private readonly bool selfTest;
    private int scanCounter;
    private const int NumberHeight = 18; // Streifen für die Seitenzahl unter dem Bild
    private const int ThumbImageWidth = 800; // Miniaturbilder verkleinert vorhalten (voller Scan wäre ~25 MB je Seite);
                                             // zugleich die Obergrenze der DARSTELLUNG — so wird nie hochskaliert
    private const int FramePadding = 8;  // Rahmen oben und seitlich um das Seitenbild
    private static readonly Color SelectionColor = Color.FromArgb(0xA6, 0xD0, 0xF1); // Rahmen und Seitenzahl-Streifen der markierten Seite
    private static readonly Color FrameColor = Color.LightGray; // derselbe Rahmen im Ruhezustand

    private Panel selected; // Miniatur-Container (Bild + Seitenzahl)
    private Point dragStart; // Mausposition beim Drücken — Start des Miniatur-Ziehens
    private string selectedScannerId; // DeviceID, TestPageId oder null (= noch kein Gerät gewählt)
    private string selectedScannerName;
    private string clipboardPath; // interne Seiten-Zwischenablage (Ausschneiden/Kopieren)
    private FormWindowState previousWindowState; // zum Verlassen des Vollbildmodus
    private readonly AppSettings settings;
    private readonly PrinterSettings copyPrinterSettings = new(); // Kopiermodus: gewählter Drucker samt Treiber-Einstellungen
    private readonly List<PaperSize> copyPaperSizes = []; // Papierformate des gewählten Druckers (parallel zur Combo)
    private readonly List<PaperSource> copyPaperSources = []; // Papierzufuhren des gewählten Druckers (parallel zur Combo)
    private readonly ToolTip toolTip = new();
    private Button btnZoomOut; // übereinander gestapelt in einem ToolStripControlHost (s. CreateZoomButtons)
    private Button btnZoomIn;
    private Font copyModeBoldFont; // „Kopiermodus beenden" fett, solange der Modus aktiv ist
    private bool pendingStiScan;   // Start über die Scanner-Taste: nach dem Anzeigen sofort scannen
    private bool ocrBusy;          // Texterkennung läuft im Hintergrund — Beenden solange abweisen

    public MainForm() : this(false) // parameterlos für den Windows-Forms-Designer
    {
    }

    public MainForm(bool selfTest, string stiDeviceId = null)
    {
        this.selfTest = selfTest;
        settings = AppSettings.Load();
        if (string.IsNullOrEmpty(settings.Language)) // einmalig die Sprachwahl des Installers übernehmen
        {
            try
            {
                var languageDefault = Path.Combine(AppContext.BaseDirectory, "language.default");
                if (File.Exists(languageDefault)) { settings.Language = File.ReadAllText(languageDefault).Trim(); }
            }
            catch (IOException) { }
            if (string.IsNullOrEmpty(settings.Language)) { settings.Language = "de"; }
        }
        Lng.Initialize(selfTest ? "de" : settings.Language); // der Selbsttest-Screenshot bleibt deutsch
        InitializeComponent();
        Lng.Apply(this); // Designer-Texte übersetzen (direkt nach InitializeComponent)
        Lng.Apply(thumbContextMenu);
        Lng.TranslateItems(comboColor, comboArea, comboFeed, comboCopyDuplex); // alle werden über SelectedIndex ausgewertet
        toolStrip.Renderer = new BigArrowRenderer();
        // Eigenes Menü statt des auto-generierten: das erbt in WinForms live die fette
        // 11-pt-Schrift des Toolbar-Buttons (gleiche Lösung wie in PDFlight)
        splitScan.DropDown = new ToolStripDropDownMenu { Font = new Font(Font.FontFamily, 9f) };
        sessionFolder = selfTest // der Selbsttest bleibt in einem Wegwerf-Ordner
            ? Path.Combine(Path.GetTempPath(), "ScanView_" + Guid.NewGuid().ToString("N"))
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ScanView", "Seiten");
        Directory.CreateDirectory(sessionFolder);
        foreach (var file in Directory.EnumerateFiles(sessionFolder, "scan_*")) // Zähler hinter vorhandene Seiten setzen
        {
            if (int.TryParse(Path.GetFileNameWithoutExtension(file).AsSpan(5), out var number)) { scanCounter = Math.Max(scanCounter, number); }
        }
        static int Clamped(int value, ComboBox combo, int fallback) => value >= 0 && value < combo.Items.Count ? value : fallback;
        comboDpi.SelectedIndex = Clamped(settings.DpiIndex, comboDpi, 2);      // Standard: 300 dpi — der OCR-Sweet-Spot
        comboColor.SelectedIndex = Clamped(settings.ColorIndex, comboColor, 0);
        comboArea.SelectedIndex = Clamped(settings.AreaIndex, comboArea, 0);
        comboFeed.SelectedIndex = Clamped(settings.FeedIndex, comboFeed, 0);
        trackBrightness.Value = Math.Clamp(settings.Brightness, trackBrightness.Minimum, trackBrightness.Maximum);
        comboOcr.Items.Add(Lng.T("Ohne Texterkennung"));
        foreach (var code in OcrLanguages.Installed()) { comboOcr.Items.Add(new OcrLanguageItem(code)); }
        SelectOcrLanguage(settings.OcrLanguage); // bevorzugte Sprache aus den Optionen als Vorgabe
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } // Fenstersymbol = Programmicon der EXE
        catch (Exception ex) when (ex is ArgumentException or IOException) { }
        CreateZoomButtons();
        ApplyToolbarIcons();
        ApplyMenuIcons();
        Application.AddMessageFilter(this); // Strg+Mausrad-Zoom (s. PreFilterMessage)
        if (!selfTest) // der Selbsttest-Screenshot soll deterministisch bleiben
        {
            RestoreWindowBounds();
            flowPanel.BackColor = Color.FromArgb(settings.OverviewBackColor); // Hintergrund der Seitenübersicht
            thumbWidth = Math.Max(ThumbWidths[0], settings.ThumbWidth);
            selectedScannerId = settings.ScannerId; // zuletzt benutzter Scanner; geprüft wird erst beim Scannen
            selectedScannerName = settings.ScannerName;
            foreach (var file in settings.PageFiles.Where(File.Exists)) { AddPage(file); } // „Seiten behalten"
            Select(null);
            if (stiDeviceId != null) // Start über die Scanner-Taste: das auslösende Gerät verwenden und sofort scannen
            {
                var device = ScanService.ListScanners().FirstOrDefault(s => string.Equals(s.Id, stiDeviceId, StringComparison.OrdinalIgnoreCase));
                if (device != null)
                {
                    selectedScannerId = device.Id;
                    selectedScannerName = device.Name;
                }
                pendingStiScan = true;
            }
        }
    }

    /// <summary>Beenden-Verhalten aus den Optionen: Seiten behalten, nach Rückfrage leeren oder still leeren.</summary>
    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (ocrBusy) { e.Cancel = true; return; } // erst die laufende Texterkennung fertigstellen
        if (selfTest) { return; }
        var pages = flowPanel.Controls.Cast<Panel>().Select(p => (string)p.Tag).ToList();
        var keep = pages.Count > 0 && (settings.ExitAction == 0
            || (settings.ExitAction == 1 && !TaskDlg.ConfirmTaskDlg(Handle, Lng.T("Seitenübersicht leeren?"),
                Lng.T("Bei Nein stehen die Seiten beim nächsten Programmstart wieder in der Übersicht."))));
        settings.PageFiles = keep ? pages : [];
        try
        {
            if (keep) // nicht mehr referenzierte Dateien (entfernte Seiten, Zwischenablage-Reste) trotzdem aufräumen
            {
                foreach (var file in Directory.EnumerateFiles(sessionFolder).Where(f => !pages.Contains(f, StringComparer.OrdinalIgnoreCase)))
                {
                    File.Delete(file);
                }
            }
            else { Directory.Delete(sessionFolder, true); }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
    }

    /// <summary>Baut die beiden Zoom-Buttons übereinander in einen ToolStripControlHost — der
    /// ToolStrip selbst kann Items nur nebeneinander anordnen.</summary>
    private void CreateZoomButtons()
    {
        Button Make(string text, string tip, EventHandler onClick)
        {
            Button button = new() { Size = new Size(32, 27), Text = text, TabStop = false, FlatStyle = FlatStyle.Flat };
            button.FlatAppearance.BorderSize = 0;
            toolTip.SetToolTip(button, tip);
            button.Click += onClick;
            return button;
        }
        btnZoomIn = Make("+", "Miniaturen vergrößern (Strg++)", BtnZoomIn_Click);
        btnZoomOut = Make("−", "Miniaturen verkleinern (Strg+−)", BtnZoomOut_Click);
        Panel host = new() { Size = new Size(32, 56) };
        btnZoomIn.Location = new Point(0, 1);
        btnZoomOut.Location = new Point(0, 28);
        host.Controls.Add(btnZoomIn);
        host.Controls.Add(btnZoomOut);
        toolStrip.Items.Insert(toolStrip.Items.IndexOf(toolStripSeparator2) + 1, new ToolStripControlHost(host));
    }

    /// <summary>Versieht die Toolbar-Buttons mit Symbolen aus der Windows-Symbolschrift
    /// "Segoe MDL2 Assets" — fehlt sie, bleiben es reine Textbuttons.</summary>
    private void ApplyToolbarIcons()
    {
        if (!ToolbarIcons.FontAvailable) { return; }
        var edge = LogicalToDeviceUnits(24);
        toolStrip.ImageScalingSize = new Size(edge, edge);
        var size = toolStrip.ImageScalingSize;
        void Set(ToolStripItem item, char glyph, bool imageOnly = false)
        {
            item.Image = ToolbarIcons.Get(glyph, size);
            item.TextImageRelation = TextImageRelation.ImageAboveText;
            item.DisplayStyle = imageOnly ? ToolStripItemDisplayStyle.Image : ToolStripItemDisplayStyle.ImageAndText;
        }
        Set(splitScan, ToolbarIcons.Scan);
        Set(btnSave, ToolbarIcons.Save);
        Set(btnPrint, ToolbarIcons.Print);
        Set(btnMoveLeft, ToolbarIcons.Previous, imageOnly: true);
        Set(btnMoveRight, ToolbarIcons.Next, imageOnly: true);
        Set(btnRemove, ToolbarIcons.Delete);
        Set(btnCrop, ToolbarIcons.Crop);
        Set(btnCopyMode, ToolbarIcons.Copy);
        btnNew.Image = ToolbarIcons.GetNewPage(size); // leeres Blatt mit Sternchen
        btnNew.TextImageRelation = TextImageRelation.ImageAboveText;
        btnNew.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
        var zoomEdge = LogicalToDeviceUnits(18);
        btnZoomOut.Image = ToolbarIcons.Get(ToolbarIcons.ZoomOut, new Size(zoomEdge, zoomEdge));
        btnZoomIn.Image = ToolbarIcons.Get(ToolbarIcons.ZoomIn, new Size(zoomEdge, zoomEdge));
        btnZoomOut.Text = string.Empty;
        btnZoomIn.Text = string.Empty;
    }

    /// <summary>Symbole für alle Menüeinträge (16 px, wie in PDFlight).</summary>
    private void ApplyMenuIcons()
    {
        if (!ToolbarIcons.FontAvailable) { return; }
        var size = LogicalToDeviceUnits(new Size(16, 16));
        menuStrip.ImageScalingSize = size;
        Image Icon16(char glyph) => ToolbarIcons.Get(glyph, size);
        menuActionNew.Image = ToolbarIcons.GetNewPage(size);
        menuActionImport.Image = Icon16(ToolbarIcons.Import);
        menuActionScan.Image = Icon16(ToolbarIcons.Scan);
        menuActionSave.Image = Icon16(ToolbarIcons.Save);
        menuActionPrint.Image = Icon16(ToolbarIcons.Print);
        menuActionClose.Image = Icon16(ToolbarIcons.Power);
        menuEditCut.Image = Icon16(ToolbarIcons.Cut);
        menuEditCopy.Image = Icon16(ToolbarIcons.Copy);
        menuEditPaste.Image = Icon16(ToolbarIcons.Paste);
        menuEditDelete.Image = Icon16(ToolbarIcons.Delete);
        menuEditCrop.Image = Icon16(ToolbarIcons.Crop);
        menuEditRotateLeft.Image = ToolbarIcons.GetMirrored(ToolbarIcons.Rotate, size);
        menuEditRotate180.Image = Icon16(ToolbarIcons.Rotate180);
        menuEditRotateRight.Image = Icon16(ToolbarIcons.Rotate);
        menuEditBacks.Image = Icon16(ToolbarIcons.Interleave);
        menuEditReverse.Image = Icon16(ToolbarIcons.Sort);
        menuViewFitWidth.Image = Icon16(ToolbarIcons.FitFrame);
        menuViewFitPage.Image = Icon16(ToolbarIcons.SinglePage);
        menuViewTwoPages.Image = Icon16(ToolbarIcons.TwoPages);
        menuViewIcons.Image = Icon16(ToolbarIcons.GridView);
        menuViewZoomIn.Image = Icon16(ToolbarIcons.ZoomIn);
        menuViewZoomOut.Image = Icon16(ToolbarIcons.ZoomOut);
        menuViewFullScreen.Image = Icon16(ToolbarIcons.FullScreen);
        menuExtrasScanner.Image = Icon16(ToolbarIcons.Scan);
        menuExtrasOptions.Image = Icon16(ToolbarIcons.Settings);
        menuHelpShortcuts.Image = Icon16(ToolbarIcons.Help);
        menuHelpUpdate.Image = Icon16(ToolbarIcons.UpdateSearch);
        menuHelpAbout.Image = Icon16(ToolbarIcons.Info);
        thumbContextMenu.ImageScalingSize = size; // Kontextmenü der Miniaturen
        contextCrop.Image = Icon16(ToolbarIcons.Crop);
        contextRotateLeft.Image = ToolbarIcons.GetMirrored(ToolbarIcons.Rotate, size);
        contextRotate180.Image = Icon16(ToolbarIcons.Rotate180);
        contextRotateRight.Image = Icon16(ToolbarIcons.Rotate);
        contextCut.Image = Icon16(ToolbarIcons.Cut);
        contextCopy.Image = Icon16(ToolbarIcons.Copy);
        contextPaste.Image = Icon16(ToolbarIcons.Paste);
        contextDelete.Image = Icon16(ToolbarIcons.Delete);
        contextOpenViewer.Image = Icon16(ToolbarIcons.OpenFile);
    }

    /// <summary>Der Rechtsklick hat die Miniatur bereits markiert — nur Einfügen hängt vom Zustand ab.</summary>
    private void ThumbContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
    {
        contextPaste.Enabled = clipboardPath != null;
    }

    private void ContextOpenViewer_Click(object sender, EventArgs e)
    {
        if (selected == null) { return; }
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo((string)selected.Tag) { UseShellExecute = true });
    }

    // ------------------------------------------------------------------ Fensterposition merken

    private void RestoreWindowBounds()
    {
        Rectangle bounds = new(settings.WindowX, settings.WindowY, settings.WindowWidth, settings.WindowHeight);
        if (bounds.Width >= MinimumSize.Width && bounds.Height >= MinimumSize.Height
            && Screen.AllScreens.Any(screen => screen.WorkingArea.IntersectsWith(bounds))) // Monitor kann inzwischen fehlen
        {
            StartPosition = FormStartPosition.Manual;
            Bounds = bounds;
        }
        if (settings.WindowMaximized) { WindowState = FormWindowState.Maximized; }
    }

    private void SaveWindowBounds()
    {
        var bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
        settings.WindowX = bounds.X;
        settings.WindowY = bounds.Y;
        settings.WindowWidth = bounds.Width;
        settings.WindowHeight = bounds.Height;
        // Vollbild (F11, ohne Rahmen) nicht als "maximiert" einfrieren
        settings.WindowMaximized = WindowState == FormWindowState.Maximized && FormBorderStyle != FormBorderStyle.None;
        settings.Save();
    }

    // ------------------------------------------------------------------ Beenden mit Escape (aus PDFlight übernommen)

    /// <summary>Broadcast einer zweiten Instanz: dieses Fenster in den Vordergrund holen — bei der
    /// Scanner-Taste (WM_SCANSCANVIEW) zusätzlich sofort scannen. Dynamisch registrierte
    /// Nachrichten müssen per 'if' geprüft werden.</summary>
    protected override void WndProc(ref Message m)
    {
        if (m.Msg == NativeMethods.WM_SHOWSCANVIEW || m.Msg == NativeMethods.WM_SCANSCANVIEW)
        {
            if (WindowState == FormWindowState.Minimized) { WindowState = FormWindowState.Normal; }
            Activate();
            if (m.Msg == NativeMethods.WM_SCANSCANVIEW && Enabled) // nicht, während ein modaler Dialog offen ist
            {
                StartStiScanDelayed(); // erst den Vordergrundwechsel vollziehen lassen, dann scannen
            }
        }
        base.WndProc(ref m);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        switch (keyData)
        {
            case Keys.Control | Keys.Add when !panelCopyMode.Visible: BtnZoomIn_Click(this, EventArgs.Empty); return true; // Ziffernblock; Strg+± liegt auf den Menükürzeln
            case Keys.Control | Keys.Subtract when !panelCopyMode.Visible: BtnZoomOut_Click(this, EventArgs.Empty); return true;
            case Keys.Alt | Keys.Left when !panelCopyMode.Visible: MoveSelected(-1); return true;
            case Keys.Alt | Keys.Right when !panelCopyMode.Visible: MoveSelected(1); return true;
            case Keys.Escape | Keys.Shift when settings.CloseOnEscape: Close(); return true; // Umschalt+Esc beendet sofort
            case Keys.Escape when menuViewFullScreen.Checked: MenuViewFullScreen_Click(this, EventArgs.Empty); return true;
            case Keys.Escape when settings.CloseOnEscape: return HandleEscapeToClose();
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private DateTime lastEscape = DateTime.MinValue;

    /// <summary>Beenden erst beim zweiten Esc kurz hintereinander.</summary>
    private bool HandleEscapeToClose()
    {
        var now = DateTime.UtcNow;
        if ((now - lastEscape).TotalMilliseconds <= 1500) { Close(); return true; }
        lastEscape = now;
        statusLabel.Text = Lng.T("Esc erneut drücken, um das Programm zu beenden");
        return true;
    }

    /// <summary>Startet den Tasten-Scan erst, nachdem das Fenster gezeichnet und aktiviert ist —
    /// der Scan blockiert den UI-Thread; ein sofortiger Start ließe das Fenster bis zum
    /// Scan-Ende unsichtbar im Hintergrund hängen.</summary>
    private void StartStiScanDelayed()
    {
        var timer = new System.Windows.Forms.Timer { Interval = 250 };
        timer.Tick += (s, e) =>
        {
            timer.Stop();
            timer.Dispose();
            SplitScan_ButtonClick(this, EventArgs.Empty);
        };
        timer.Start();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        if (pendingStiScan) // Scanner-Taste: erst das Fenster anzeigen, dann scannen
        {
            pendingStiScan = false;
            Activate();
            StartStiScanDelayed();
            return;
        }
        if (!selfTest) { return; }
        // Selbsttest für die Werkzeugkette: zwei Testseiten, PDF erstellen, Ergebnis melden, beenden
        AddPage(ScanService.RenderTestPage(NextScanPath(), "Selbsttest Seite eins", "Der Blutdruck lag bei 120 zu 80 mmHg."));
        AddPage(ScanService.RenderTestPage(NextScanPath(), "Selbsttest Seite zwei", "Prüfung der Umlaute: Ärzte, Öfen, Übungen."));
        var output = Path.Combine(sessionFolder, "Selbsttest.pdf");
        // synchron direkt über den Service — ein GetResult auf CreatePdfAsync würde den UI-Thread deadlocken
        OcrPdfService.CreateSearchablePdf(flowPanel.Controls.Cast<Panel>().Select(b => (string)b.Tag).ToList(), output, "deu", 75, null);
        var pageCount = 0;
        try
        {
            using var check = PdfSharp.Pdf.IO.PdfReader.Open(output, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);
            pageCount = check.PageCount;
        }
        catch (Exception ex) when (ex is PdfSharp.PdfSharpException or IOException or InvalidOperationException) { }
        using (var shot = new Bitmap(Width, Height))
        {
            DrawToBitmap(shot, new Rectangle(Point.Empty, Size));
            shot.Save(Path.Combine(AppContext.BaseDirectory, "selftest.png"));
        }
        BtnCopyMode_Click(this, EventArgs.Empty); // Kopiermodus-Ansicht ebenfalls festhalten
        using (var shot = new Bitmap(Width, Height))
        {
            DrawToBitmap(shot, new Rectangle(Point.Empty, Size));
            shot.Save(Path.Combine(AppContext.BaseDirectory, "selftest-copymode.png"));
        }
        using (SettingsForm settingsDialog = new("de", true, 0, "", Color.White, "deu", 75)) // und der Optionen-Dialog
        {
            settingsDialog.StartPosition = FormStartPosition.Manual;
            settingsDialog.Show(this);
            using var shot = new Bitmap(settingsDialog.Width, settingsDialog.Height);
            settingsDialog.DrawToBitmap(shot, new Rectangle(Point.Empty, settingsDialog.Size));
            shot.Save(Path.Combine(AppContext.BaseDirectory, "selftest-settings.png"));
        }
        using (SaveForm saveDialog = new(true, sessionFolder, "Selbsttest", "deu", 75, "ScanView")) // und der Speichern-Dialog
        {
            saveDialog.StartPosition = FormStartPosition.Manual;
            saveDialog.Show(this);
            using var shot = new Bitmap(saveDialog.Width, saveDialog.Height);
            saveDialog.DrawToBitmap(shot, new Rectangle(Point.Empty, saveDialog.Size));
            shot.Save(Path.Combine(AppContext.BaseDirectory, "selftest-save.png"));
        }
        using (var image = ScanService.LoadUnlocked((string)((Panel)flowPanel.Controls[0]).Tag)) // und der Zuschneide-Dialog
        using (CropForm cropDialog = new(image, Rectangle.Empty))
        {
            cropDialog.StartPosition = FormStartPosition.Manual;
            cropDialog.Show(this);
            Application.DoEvents(); // Shown-Ereignis (Startauswahl) braucht die Nachrichtenschleife
            using var shot = new Bitmap(cropDialog.Width, cropDialog.Height);
            cropDialog.DrawToBitmap(shot, new Rectangle(Point.Empty, cropDialog.Size));
            shot.Save(Path.Combine(AppContext.BaseDirectory, "selftest-crop.png"));
        }
        Environment.Exit(pageCount == 2 ? 0 : 1);
    }

    /// <summary>Stellt die Texterkennungs-Combo auf die Sprache mit dem angegebenen Code.</summary>
    private void SelectOcrLanguage(string code)
    {
        var match = comboOcr.Items.OfType<OcrLanguageItem>()
            .FirstOrDefault(item => string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase));
        if (match != null) { comboOcr.SelectedItem = match; }
        else if (comboOcr.Items.Count > 1) { comboOcr.SelectedIndex = 1; } // erste Sprache
        else { comboOcr.SelectedIndex = 0; } // Ohne Texterkennung
    }

    /// <summary>Gewählte OCR-Sprache des aktuellen Scans — null bei „Ohne Texterkennung".</summary>
    private string CurrentOcrLanguage => comboOcr.SelectedItem is OcrLanguageItem item ? item.Code : null;

    private string NextScanPath() => Path.Combine(sessionFolder, $"scan_{++scanCounter:D3}.tif");

    private int SelectedDpi => int.Parse(comboDpi.Text.Split(' ')[0]);

    private int SelectedColorIntent => comboColor.SelectedIndex switch { 1 => 2, 2 => 4, _ => 1 }; // WIA: 1 Farbe, 2 Grau, 4 SW

    /// <summary>Scanfenster in Millimetern — null steht für „maximal" (Gerätestandard).</summary>
    private SizeF? SelectedAreaMm => comboArea.SelectedIndex switch
    {
        1 => new SizeF(210, 297),       // A4
        2 => new SizeF(148, 210),       // A5
        3 => new SizeF(105, 148),       // A6
        4 => new SizeF(215.9f, 279.4f), // US-Letter
        5 => new SizeF(85, 54),         // Visitenkarte
        _ => null,                      // maximal
    };

    /// <summary>Verlängert die rechte Kante des Einstellungsbereichs optisch in die Toolbar —
    /// Scannen-Button und Einstellungen bilden so eine gemeinsame Spalte.</summary>
    private void ToolStrip_Paint(object sender, PaintEventArgs e)
    {
        using Pen pen = new(SystemColors.ControlDark);
        e.Graphics.DrawLine(pen, panelSettings.Width - 1, 0, panelSettings.Width - 1, toolStrip.Height);
    }

    private void TrackBrightness_ValueChanged(object sender, EventArgs e)
    {
        labelBrightness.Text = string.Format(Lng.T("&Helligkeit: {0}"), trackBrightness.Value);
    }

    // ------------------------------------------------------------------ Scannen

    private void SplitScan_ButtonClick(object sender, EventArgs e)
    {
        if (selectedScannerId == null) // noch kein Gerät gewählt: einzigen Scanner automatisch nehmen
        {
            var scanners = ScanService.ListScanners();
            if (scanners.Count == 1)
            {
                selectedScannerId = scanners[0].Id;
                selectedScannerName = scanners[0].Name;
                UpdateUiState();
            }
            else if (scanners.Count > 1) { splitScan.ShowDropDown(); return; } // Auswahl im Gerätemenü statt Windows-Dialog
            else
            {
                TaskDlg.MsgTaskDlg(Handle, Lng.T("Kein Scanner gefunden."),
                    Lng.T("Bitte schließe einen Scanner an oder schalte ihn ein."), TaskDialogIcon.Warning);
                return;
            }
        }
        statusLabel.Text = Lng.T("Scanne …");
        statusStrip.Refresh();
        // UseWaitCursor statt Cursor.Current: die WIA-Fortschrittsanzeige pumpt Nachrichten und würde Cursor.Current sofort zurücksetzen
        Application.UseWaitCursor = true;
        string scanned;
        try
        {
            scanned = ScanService.ScanFromDevice(selectedScannerId, NextScanPath(), SelectedDpi, SelectedColorIntent, SelectedAreaMm, trackBrightness.Value, comboFeed.SelectedIndex == 1);
        }
        finally
        {
            Application.UseWaitCursor = false;
        }
        if (scanned == null)
        {
            statusLabel.Text = Lng.T("Scan abgebrochen oder fehlgeschlagen");
            return;
        }
        if (panelCopyMode.Visible) { PrintCopy(scanned); return; } // Kopiermodus: direkt drucken statt sammeln
        AddPage(scanned);
    }

    // ------------------------------------------------------------------ Kopiermodus

    /// <summary>Kopiermodus umschalten: statt der Seitenübersicht erscheinen die Druckoptionen,
    /// und jeder Scan geht direkt an den Drucker.</summary>
    private void BtnCopyMode_Click(object sender, EventArgs e)
    {
        var active = !panelCopyMode.Visible;
        if (active && comboCopyPrinter.Items.Count == 0) // Druckerliste erst beim ersten Aufruf füllen
        {
            foreach (string printer in PrinterSettings.InstalledPrinters) { comboCopyPrinter.Items.Add(printer); }
            var storedIndex = settings.CopyPrinter != null ? comboCopyPrinter.Items.IndexOf(settings.CopyPrinter) : -1;
            var defaultIndex = storedIndex >= 0 ? storedIndex : comboCopyPrinter.Items.IndexOf(copyPrinterSettings.PrinterName); // sonst Standarddrucker
            comboCopyPrinter.SelectedIndex = defaultIndex >= 0 ? defaultIndex : (comboCopyPrinter.Items.Count > 0 ? 0 : -1);
            ApplyStoredCopySelections(); // gespeicherte Einstellungen (nach dem Laden der Druckerfähigkeiten)
        }
        panelCopyMode.Visible = active;
        flowPanel.Visible = !active;
        // Aktiv: zweizeilig, fett und ohne Symbol — inaktiv: einzeilig mit Symbol
        btnCopyMode.Text = active ? Lng.T("CopyMode.Stop", "Kopiermodus\nbeenden") : Lng.T("&Kopiermodus");
        copyModeBoldFont ??= new Font(toolStrip.Font, FontStyle.Bold);
        btnCopyMode.Font = active ? copyModeBoldFont : null; // null = ToolStrip-Schrift
        btnCopyMode.DisplayStyle = active || !ToolbarIcons.FontAvailable
            ? ToolStripItemDisplayStyle.Text
            : ToolStripItemDisplayStyle.ImageAndText;
        menuActionCopyMode.Checked = active;
        if (!active) { StoreCopyUiInSettings(); } // Änderungen sofort als gemeinsame Vorgabe übernehmen
        UpdateUiState(); // sperrt bzw. reaktiviert alles Seitenbezogene
        if (active) { statusLabel.Text = Lng.T("Kopiermodus: jeder Scan wird direkt gedruckt"); }
    }

    /// <summary>Überträgt die gespeicherten Druckvorgaben auf die Kopiermodus-Controls
    /// (nachdem die Fähigkeiten des gewählten Druckers geladen sind).</summary>
    private void ApplyStoredCopySelections()
    {
        var paperIndex = copyPaperSizes.FindIndex(p => p.RawKind == settings.CopyPaperRawKind);
        if (paperIndex >= 0) { comboCopyPaper.SelectedIndex = paperIndex; }
        var sourceIndex = copyPaperSources.FindIndex(s => s.RawKind == settings.CopyPaperSourceRawKind);
        if (sourceIndex >= 0) { comboCopySource.SelectedIndex = sourceIndex; }
        if (comboCopyDuplex.Enabled) { comboCopyDuplex.SelectedIndex = Math.Clamp(settings.CopyDuplexIndex, 0, comboCopyDuplex.Items.Count - 1); }
        numCopies.Value = Math.Clamp(settings.CopyCopies, (int)numCopies.Minimum, (int)numCopies.Maximum);
        if (chkCopyColor.Enabled) { chkCopyColor.Checked = settings.CopyColor; }
        chkCopyFit.Checked = settings.CopyFit;
    }

    /// <summary>Wendet die gespeicherten Druckvorgaben (Drucker, Exemplare, Duplex, Papier, Zufuhr,
    /// Farbe) auf ein Druckdokument an — dieselbe Voreinstellung wie im Kopiermodus.</summary>
    private void ApplySharedPrinterSettings(PrintDocument document)
    {
        var printerSettings = document.PrinterSettings;
        if (!string.IsNullOrEmpty(settings.CopyPrinter)
            && PrinterSettings.InstalledPrinters.Cast<string>().Contains(settings.CopyPrinter))
        {
            printerSettings.PrinterName = settings.CopyPrinter;
        }
        printerSettings.Copies = (short)Math.Clamp(settings.CopyCopies, 1, 99);
        try
        {
            if (printerSettings.CanDuplex)
            {
                printerSettings.Duplex = settings.CopyDuplexIndex switch { 1 => Duplex.Vertical, 2 => Duplex.Horizontal, _ => Duplex.Simplex };
            }
            foreach (PaperSize paper in printerSettings.PaperSizes)
            {
                if (paper.RawKind == settings.CopyPaperRawKind) { document.DefaultPageSettings.PaperSize = paper; break; }
            }
            foreach (PaperSource source in printerSettings.PaperSources)
            {
                if (source.RawKind == settings.CopyPaperSourceRawKind) { document.DefaultPageSettings.PaperSource = source; break; }
            }
            if (printerSettings.SupportsColor) { document.DefaultPageSettings.Color = settings.CopyColor; }
        }
        catch (InvalidPrinterException) { }
    }

    /// <summary>Schreibt die Kopiermodus-Controls in die gespeicherten Druckvorgaben —
    /// dieselben Werte nutzt auch der normale Drucken-Dialog als Voreinstellung.</summary>
    private void StoreCopyUiInSettings()
    {
        if (comboCopyPrinter.SelectedItem is not string copyPrinter) { return; } // Kopiermodus wurde nie benutzt
        settings.CopyPrinter = copyPrinter;
        settings.CopyPaperRawKind = comboCopyPaper.SelectedIndex >= 0 && comboCopyPaper.SelectedIndex < copyPaperSizes.Count
            ? copyPaperSizes[comboCopyPaper.SelectedIndex].RawKind : -1;
        settings.CopyPaperSourceRawKind = comboCopySource.SelectedIndex >= 0 && comboCopySource.SelectedIndex < copyPaperSources.Count
            ? copyPaperSources[comboCopySource.SelectedIndex].RawKind : -1;
        settings.CopyDuplexIndex = Math.Max(0, comboCopyDuplex.SelectedIndex);
        settings.CopyCopies = (int)numCopies.Value;
        settings.CopyColor = chkCopyColor.Checked;
        settings.CopyFit = chkCopyFit.Checked;
    }

    /// <summary>Gleicht die Kopiermodus-Controls an die (z.B. vom Druckdialog geänderten) Vorgaben an.</summary>
    private void SyncCopyModeUi()
    {
        if (comboCopyPrinter.Items.Count == 0) { return; } // Kopiermodus wurde noch nie geöffnet
        var index = comboCopyPrinter.Items.IndexOf(settings.CopyPrinter);
        if (index >= 0 && index != comboCopyPrinter.SelectedIndex) { comboCopyPrinter.SelectedIndex = index; } // lädt die Fähigkeiten neu
        ApplyStoredCopySelections();
    }

    /// <summary>Lädt Papierformate, Duplex- und Farbfähigkeit des gewählten Druckers in die Controls —
    /// die Einstellungen stehen direkt auf dem Panel, ein Treiber-Dialog ist nicht mehr nötig.</summary>
    private void ComboCopyPrinter_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (comboCopyPrinter.SelectedItem is not string printer) { return; }
        copyPrinterSettings.PrinterName = printer;
        comboCopyPaper.Items.Clear();
        copyPaperSizes.Clear();
        comboCopySource.Items.Clear();
        copyPaperSources.Clear();
        try
        {
            foreach (PaperSize paper in copyPrinterSettings.PaperSizes)
            {
                copyPaperSizes.Add(paper);
                comboCopyPaper.Items.Add(paper.PaperName);
            }
            var defaultPaper = copyPrinterSettings.DefaultPageSettings.PaperSize;
            var index = defaultPaper != null ? copyPaperSizes.FindIndex(p => p.RawKind == defaultPaper.RawKind) : -1;
            if (comboCopyPaper.Items.Count > 0) { comboCopyPaper.SelectedIndex = Math.Max(0, index); }
            foreach (PaperSource source in copyPrinterSettings.PaperSources)
            {
                copyPaperSources.Add(source);
                comboCopySource.Items.Add(source.SourceName);
            }
            var defaultSource = copyPrinterSettings.DefaultPageSettings.PaperSource;
            var sourceIndex = defaultSource != null ? copyPaperSources.FindIndex(s => s.RawKind == defaultSource.RawKind) : -1;
            if (comboCopySource.Items.Count > 0) { comboCopySource.SelectedIndex = Math.Max(0, sourceIndex); }
            comboCopyDuplex.Enabled = copyPrinterSettings.CanDuplex;
            comboCopyDuplex.SelectedIndex = 0; // Einseitig
            chkCopyColor.Enabled = copyPrinterSettings.SupportsColor;
            chkCopyColor.Checked = copyPrinterSettings.SupportsColor && copyPrinterSettings.DefaultPageSettings.Color;
        }
        catch (InvalidPrinterException) { } // Drucker gerade entfernt — die Combos bleiben leer
    }

    /// <summary>Druckt einen frischen Scan sofort mit den Kopiermodus-Einstellungen.</summary>
    private void PrintCopy(string tiffPath)
    {
        StoreCopyUiInSettings(); // die aktuellen Werte sind zugleich die gemeinsame Druckvorgabe
        using PrintDocument document = new();
        document.DocumentName = "ScanView Kopie";
        if (comboCopyPrinter.SelectedItem is string printer) { copyPrinterSettings.PrinterName = printer; }
        copyPrinterSettings.Copies = (short)numCopies.Value;
        copyPrinterSettings.Duplex = comboCopyDuplex.Enabled
            ? comboCopyDuplex.SelectedIndex switch
            {
                1 => Duplex.Vertical,
                2 => Duplex.Horizontal,
                _ => Duplex.Simplex
            }
            : Duplex.Default;
        document.PrinterSettings = copyPrinterSettings;
        document.DefaultPageSettings.Color = chkCopyColor.Checked;
        if (comboCopyPaper.SelectedIndex >= 0 && comboCopyPaper.SelectedIndex < copyPaperSizes.Count)
        {
            document.DefaultPageSettings.PaperSize = copyPaperSizes[comboCopyPaper.SelectedIndex];
        }
        if (comboCopySource.SelectedIndex >= 0 && comboCopySource.SelectedIndex < copyPaperSources.Count)
        {
            document.DefaultPageSettings.PaperSource = copyPaperSources[comboCopySource.SelectedIndex];
        }
        document.PrintPage += (s, args) =>
        {
            using var image = ScanService.LoadUnlocked(tiffPath);
            if (chkCopyFit.Checked)
            {
                args.Graphics.DrawImage(image, args.MarginBounds); // in die Ränder eingepasst
            }
            else
            {
                // Originalgröße: die Druck-Graphics rechnet in 1/100 Zoll
                args.Graphics.DrawImage(image, 0, 0, image.Width * 100f / image.HorizontalResolution, image.Height * 100f / image.VerticalResolution);
            }
            args.HasMorePages = false;
        };
        try
        {
            document.Print();
            statusLabel.Text = string.Format(Lng.T("Kopie ({0}×) an {1} übergeben"), numCopies.Value, document.PrinterSettings.PrinterName);
        }
        catch (InvalidPrinterException ex)
        {
            TaskDlg.ErrTaskDlg(Handle, Lng.T("Drucken fehlgeschlagen."), ex);
        }
    }

    /// <summary>Baut das Geräte-Menü auf: alle Scanner, Auswahl per Häkchen.</summary>
    private void SplitScan_DropDownOpening(object sender, EventArgs e)
    {
        splitScan.DropDownItems.Clear();
        foreach (var scanner in ScanService.ListScanners())
        {
            ToolStripMenuItem item = new(scanner.Name) { Checked = scanner.Id == selectedScannerId, Tag = scanner };
            item.Click += (s, args) =>
            {
                var info = (ScannerInfo)((ToolStripMenuItem)s).Tag;
                selectedScannerId = info.Id;
                selectedScannerName = info.Name;
                UpdateUiState();
            };
            splitScan.DropDownItems.Add(item);
        }
        if (splitScan.DropDownItems.Count == 0)
        {
            splitScan.DropDownItems.Add(new ToolStripMenuItem(Lng.T("(kein Scanner gefunden)")) { Enabled = false });
        }
    }

    // ------------------------------------------------------------------ Menü „Aktion"

    /// <summary>Bilddateien als Seiten aufnehmen — Kopien im Sitzungsordner, damit die
    /// Originale unangetastet bleiben und die Aufräumlogik beim Beenden greift.</summary>
    private void MenuImport_Click(object sender, EventArgs e)
    {
        using OpenFileDialog dialog = new()
        {
            Filter = Lng.T("Bilddateien") + " (*.tif;*.tiff;*.png;*.jpg;*.jpeg;*.bmp)|*.tif;*.tiff;*.png;*.jpg;*.jpeg;*.bmp|" + Lng.T("Alle Dateien") + " (*.*)|*.*",
            Multiselect = true,
            Title = Lng.T("Bilder importieren"),
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) { return; }
        foreach (var file in dialog.FileNames)
        {
            var copy = Path.Combine(sessionFolder, $"scan_{++scanCounter:D3}{Path.GetExtension(file).ToLowerInvariant()}");
            try
            {
                File.Copy(file, copy);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                TaskDlg.ErrTaskDlg(Handle, Lng.T("Importieren fehlgeschlagen."), ex);
                continue;
            }
            AddPage(copy);
        }
    }

    private void MenuClose_Click(object sender, EventArgs e)
    {
        Close();
    }

    // ------------------------------------------------------------------ Menü „Bearbeiten"

    private void MenuEditCut_Click(object sender, EventArgs e)
    {
        if (selected == null) { return; }
        clipboardPath = (string)selected.Tag; // Datei bleibt im Sitzungsordner liegen
        BtnRemove_Click(sender, e);
    }

    private void MenuEditCopy_Click(object sender, EventArgs e)
    {
        if (selected == null) { return; }
        clipboardPath = (string)selected.Tag;
        UpdateUiState();
    }

    /// <summary>Fügt eine Kopie der Zwischenablage-Seite hinter der Markierung ein (ohne Markierung: ans Ende).</summary>
    private void MenuEditPaste_Click(object sender, EventArgs e)
    {
        if (clipboardPath == null || !File.Exists(clipboardPath)) { return; }
        var insertAt = selected != null ? flowPanel.Controls.GetChildIndex(selected) + 1 : flowPanel.Controls.Count;
        var copy = Path.Combine(sessionFolder, $"scan_{++scanCounter:D3}{Path.GetExtension(clipboardPath)}");
        File.Copy(clipboardPath, copy);
        AddPage(copy);
        flowPanel.Controls.SetChildIndex(flowPanel.Controls[flowPanel.Controls.Count - 1], insertAt);
        UpdateUiState();
    }

    private void MenuEditRotateLeft_Click(object sender, EventArgs e)
    {
        RotateSelected(RotateFlipType.Rotate270FlipNone);
    }

    private void MenuEditRotate180_Click(object sender, EventArgs e)
    {
        RotateSelected(RotateFlipType.Rotate180FlipNone);
    }

    private void MenuEditRotateRight_Click(object sender, EventArgs e)
    {
        RotateSelected(RotateFlipType.Rotate90FlipNone);
    }

    private static System.Drawing.Imaging.ImageFormat ImageFormatFor(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".png" => System.Drawing.Imaging.ImageFormat.Png,
        ".jpg" or ".jpeg" => System.Drawing.Imaging.ImageFormat.Jpeg,
        ".bmp" => System.Drawing.Imaging.ImageFormat.Bmp,
        _ => System.Drawing.Imaging.ImageFormat.Tiff,
    };

    /// <summary>Lädt die Miniatur der markierten Seite neu — nach Drehen oder Zuschneiden.</summary>
    private void ReloadSelectedThumbnail()
    {
        var pic = PicOf(selected);
        var old = pic.Image;
        pic.Image = ScanService.LoadThumbnail((string)selected.Tag, ThumbImageWidth);
        old?.Dispose();
    }

    /// <summary>Dreht die Seitendatei selbst (nicht nur die Miniatur), damit auch OCR und PDF die Drehung sehen.</summary>
    private void RotateSelected(RotateFlipType rotation)
    {
        if (selected == null) { return; }
        var path = (string)selected.Tag;
        using (var image = ScanService.LoadUnlocked(path))
        {
            image.RotateFlip(rotation);
            image.Save(path, ImageFormatFor(path));
        }
        ReloadSelectedThumbnail();
    }

    /// <summary>Zuschneide-Dialog für die markierte Seite: die Aktionen (Freistellen/Zuschneiden/
    /// Ausschneiden) wirken dort sofort auf die Vorschau; „Übernehmen" ersetzt die Seitendatei.</summary>
    private void MenuEditCrop_Click(object sender, EventArgs e)
    {
        if (selected == null) { return; }
        var path = (string)selected.Tag;
        using var image = ScanService.LoadUnlocked(path);
        using CropForm dialog = new(image, new Rectangle(settings.CropX, settings.CropY, settings.CropWidth, settings.CropHeight));
        var result = dialog.ShowDialog(this);
        var bounds = dialog.WindowState == FormWindowState.Normal ? dialog.Bounds : dialog.RestoreBounds;
        settings.CropX = bounds.X; // Größe/Position des Dialogs merken — auch bei Abbruch
        settings.CropY = bounds.Y;
        settings.CropWidth = bounds.Width;
        settings.CropHeight = bounds.Height;
        if (result != DialogResult.OK || !dialog.Edited) { return; }
        dialog.ResultImage.Save(path, ImageFormatFor(path));
        ReloadSelectedThumbnail();
        statusLabel.Text = string.Format(Lng.T("Seite bearbeitet übernommen ({0} × {1} Pixel)"), dialog.ResultImage.Width, dialog.ResultImage.Height);
    }

    /// <summary>Duplex von Hand: erst alle Vorderseiten scannen, dann den Stapel gewendet — die
    /// Rückseiten liegen dadurch in umgekehrter Reihenfolge und werden hier verzahnt einsortiert.</summary>
    private void MenuEditBacks_Click(object sender, EventArgs e)
    {
        var panels = flowPanel.Controls.Cast<Panel>().ToList();
        var half = panels.Count / 2;
        List<Panel> order = [];
        for (var i = 0; i < half; i++)
        {
            order.Add(panels[i]);
            order.Add(panels[panels.Count - 1 - i]);
        }
        ReorderPages(order);
    }

    private void MenuEditReverse_Click(object sender, EventArgs e) => ReorderPages([.. flowPanel.Controls.Cast<Panel>().Reverse()]);

    private void ReorderPages(List<Panel> order)
    {
        flowPanel.SuspendLayout();
        for (var i = 0; i < order.Count; i++)
        {
            flowPanel.Controls.SetChildIndex(order[i], i);
        }
        flowPanel.ResumeLayout();
        UpdateUiState();
    }

    // ------------------------------------------------------------------ Menü „Ansicht"

    private void MenuViewFitWidth_Click(object sender, EventArgs e) => ApplyThumbWidth(ColumnThumbWidth(1));

    private void MenuViewTwoPages_Click(object sender, EventArgs e) => ApplyThumbWidth(ColumnThumbWidth(2));

    private void MenuViewFitPage_Click(object sender, EventArgs e)
    {
        // Miniaturhöhe = Rahmen + Bild (Breite−16 × 7/5) + Seitenzahl-Streifen — nach der Breite aufgelöst
        ApplyThumbWidth((flowPanel.ClientSize.Height - 32 - FramePadding - NumberHeight) * 5 / 7 + 2 * FramePadding);
    }

    private void MenuViewIcons_Click(object sender, EventArgs e)
    {
        ApplyThumbWidth(IconThumbWidth);
    }

    /// <summary>Miniaturbreite, bei der genau so viele Spalten in die Übersicht passen.</summary>
    private int ColumnThumbWidth(int columns)
    {
        var scrollbar = flowPanel.VerticalScroll.Visible ? 0 : SystemInformation.VerticalScrollBarWidth;
        return (flowPanel.ClientSize.Width - 16 - scrollbar) / columns - 16; // Panel-Padding bzw. Miniatur-Margins
    }

    private void MenuViewFullScreen_Click(object sender, EventArgs e)
    {
        if (!menuViewFullScreen.Checked)
        {
            previousWindowState = WindowState;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Normal; // erzwingt die Neuberechnung, falls schon maximiert
            WindowState = FormWindowState.Maximized;
            menuViewFullScreen.Checked = true;
        }
        else
        {
            FormBorderStyle = FormBorderStyle.Sizable;
            WindowState = previousWindowState;
            menuViewFullScreen.Checked = false;
        }
    }

    // ------------------------------------------------------------------ Menüs „Extras" und „?"

    /// <summary>Extras → Scanner: Gerät wählen; die Gerätetasten verwaltet der Windows-Dialog.</summary>
    private void MenuExtrasScanner_Click(object sender, EventArgs e)
    {
        using ScannerForm dialog = new(selectedScannerId);
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.SelectedScanner == null) { return; }
        selectedScannerId = dialog.SelectedScanner.Id;
        selectedScannerName = dialog.SelectedScanner.Name;
        UpdateUiState();
    }

    private void MenuExtrasOptions_Click(object sender, EventArgs e)
    {
        using SettingsForm dialog = new(settings.Language, settings.CloseOnEscape, settings.ExitAction, settings.SaveDirectory,
            Color.FromArgb(settings.OverviewBackColor), settings.OcrLanguage, settings.OcrJpgQuality);
        if (dialog.ShowDialog(this) != DialogResult.OK) { return; }
        var languageChanged = dialog.LanguageCode != settings.Language;
        settings.Language = dialog.LanguageCode;
        settings.CloseOnEscape = dialog.CloseOnEscape;
        settings.ExitAction = dialog.ExitAction;
        settings.SaveDirectory = dialog.SaveDirectory;
        settings.OverviewBackColor = dialog.OverviewBackColor.ToArgb();
        flowPanel.BackColor = dialog.OverviewBackColor; // sofort anwenden
        settings.OcrLanguage = dialog.OcrLanguage;
        settings.OcrJpgQuality = dialog.OcrJpgQuality;
        settings.Save();
        SelectOcrLanguage(settings.OcrLanguage); // neue bevorzugte Sprache auch für den aktuellen Scan übernehmen
        if (languageChanged) // kein automatischer Neustart — der würde mit der Einmal-Instanz kollidieren
        {
            TaskDlg.MsgTaskDlg(Handle, Lng.T("Sprache geändert."),
                Lng.T("Die neue Sprache gilt nach dem nächsten Programmstart."), TaskDialogIcon.Information);
        }
    }

    private void MenuHelpShortcuts_Click(object sender, EventArgs e)
    {
        TaskDlg.ShowShortcutsPdf(Handle, Icon);
    }

    private async void MenuHelpUpdate_Click(object sender, EventArgs e)
    {
        await TaskDlg.UpdateTaskDlg(Handle);
    }

    private void MenuHelpAbout_Click(object sender, EventArgs e)
    {
        TaskDlg.AboutTaskDlg(Handle, Icon);
    }

    // ------------------------------------------------------------------ Seitenverwaltung

    /// <summary>Hängt einen Scan als Miniatur (Bild mit Seitenzahl darunter) an die Übersicht an.</summary>
    private void AddPage(string tiffPath)
    {
        if (tiffPath == null) { return; }
        Panel thumb = new()
        {
            BackColor = FrameColor,
            Margin = new Padding(8),
            Tag = tiffPath,
        };
        PictureBox pic = new()
        {
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.White,
            Image = ScanService.LoadThumbnail(tiffPath, ThumbImageWidth),
            Cursor = Cursors.Hand,
        };
        Label num = new()
        {
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.Black,
            BackColor = Color.Transparent,
        };
        thumb.Controls.Add(pic);
        thumb.Controls.Add(num);
        LayoutThumb(thumb);
        thumb.Paint += (s, e) => // kleiner Schlagschatten des Seitenbilds auf dem Rahmen (rechts und unten)
        {
            var r = pic.Bounds;
            using SolidBrush shadowBrush = new(Color.FromArgb(45, 0, 0, 0));
            e.Graphics.FillRectangle(shadowBrush, r.Right, r.Top + 3, 3, r.Height);
            e.Graphics.FillRectangle(shadowBrush, r.Left + 3, r.Bottom, r.Width - 3, 3);
        };
        pic.Click += (s, e) => Select(thumb);
        num.Click += (s, e) => Select(thumb);
        pic.DoubleClick += (s, e) => { Select(thumb); MenuEditCrop_Click(thumb, EventArgs.Empty); }; // direkt in den Zuschneiden-Dialog
        pic.MouseDown += (s, e) =>
        {
            dragStart = e.Location;
            if (e.Button == MouseButtons.Right) { Select(thumb); } // fürs Kontextmenü zuerst markieren
        };
        num.MouseDown += (s, e) => { if (e.Button == MouseButtons.Right) { Select(thumb); } };
        thumb.ContextMenuStrip = thumbContextMenu;
        pic.ContextMenuStrip = thumbContextMenu;
        num.ContextMenuStrip = thumbContextMenu;
        pic.MouseMove += (s, e) =>
        {
            // Ziehen erst ab der System-Schwelle starten, damit ein normaler Klick ein Klick bleibt
            if (e.Button != MouseButtons.Left
                || (Math.Abs(e.X - dragStart.X) < SystemInformation.DragSize.Width
                    && Math.Abs(e.Y - dragStart.Y) < SystemInformation.DragSize.Height))
            {
                return;
            }
            Select(thumb);
            pic.DoDragDrop(thumb, DragDropEffects.Move);
            UpdateUiState();
        };
        flowPanel.Controls.Add(thumb);
        Select(thumb);
    }

    /// <summary>Setzt Panel-, Bild- und Seitenzahl-Bounds passend zur aktuellen Miniaturbreite.</summary>
    private void LayoutThumb(Panel thumb)
    {
        var picWidth = thumbWidth - 2 * FramePadding;
        var picHeight = picWidth * 7 / 5; // A4-Verhältnis
        thumb.Size = new Size(thumbWidth, FramePadding + picHeight + NumberHeight);
        PicOf(thumb).Bounds = new Rectangle(FramePadding, FramePadding, picWidth, picHeight);
        NumOf(thumb).Bounds = new Rectangle(0, FramePadding + picHeight, thumbWidth, NumberHeight);
        thumb.Invalidate(); // Schatten an der neuen Bildkante nachzeichnen
    }

    private static PictureBox PicOf(Panel thumb) => (PictureBox)thumb.Controls[0];

    private static Label NumOf(Panel thumb) => (Label)thumb.Controls[1];

    /// <summary>Schreibt die laufenden Seitenzahlen unter alle Miniaturen.</summary>
    private void RenumberPages()
    {
        for (var i = 0; i < flowPanel.Controls.Count; i++)
        {
            NumOf((Panel)flowPanel.Controls[i]).Text = (i + 1).ToString();
        }
    }

    private void FlowPanel_DragEnter(object sender, DragEventArgs e)
    {
        e.Effect = e.Data.GetDataPresent(typeof(Panel)) ? DragDropEffects.Move : DragDropEffects.None;
    }

    /// <summary>Sortiert die gezogene Miniatur schon während des Ziehens live an die Zielposition.</summary>
    private void FlowPanel_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(typeof(Panel)) is not Panel dragged) { return; }
        e.Effect = DragDropEffects.Move;
        var point = flowPanel.PointToClient(new Point(e.X, e.Y));
        if (point.Y < 40 || point.Y > flowPanel.Height - 40) // am Rand weiterscrollen
        {
            var offset = -flowPanel.AutoScrollPosition.Y + (point.Y < 40 ? -20 : 20);
            flowPanel.AutoScrollPosition = new Point(-flowPanel.AutoScrollPosition.X, Math.Max(0, offset));
        }
        if (flowPanel.GetChildAtPoint(point) is Panel target && target != dragged)
        {
            flowPanel.Controls.SetChildIndex(dragged, flowPanel.Controls.GetChildIndex(target));
            RenumberPages();
        }
    }

    private void BtnZoomOut_Click(object sender, EventArgs e)
    {
        ApplyThumbWidth(ThumbWidths.LastOrDefault(w => w < thumbWidth)); // nächstkleinere Zoomstufe
    }

    private void BtnZoomIn_Click(object sender, EventArgs e)
    {
        ApplyThumbWidth(ThumbWidths.FirstOrDefault(w => w > thumbWidth)); // nächstgrößere Zoomstufe
    }

    private void ApplyThumbWidth(int width)
    {
        if (width < ThumbWidths[0]) { return; } // 0 = keine passende Zoomstufe mehr
        width = Math.Min(width, ThumbImageWidth); // nie größer darstellen, als das Miniaturbild Daten hat
        thumbWidth = width;
        flowPanel.SuspendLayout();
        foreach (var thumb in flowPanel.Controls.Cast<Panel>())
        {
            LayoutThumb(thumb);
        }
        flowPanel.ResumeLayout();
        UpdateUiState();
    }

    private void Select(Panel thumb)
    {
        // Der Rahmen (samt Seitenzahl-Streifen) bleibt immer stehen und wechselt nur die Farbe
        selected?.BackColor = FrameColor;
        selected = thumb;
        selected?.BackColor = SelectionColor;
        UpdateUiState();
    }

    private void UpdateUiState()
    {
        RenumberPages();
        var count = flowPanel.Controls.Count;
        var pagesVisible = !panelCopyMode.Visible; // im Kopiermodus ist die Übersicht ausgeblendet — alles Seitenbezogene sperren
        btnSave.Enabled = pagesVisible && count > 0;
        btnPrint.Enabled = pagesVisible && count > 0;
        btnNew.Enabled = pagesVisible && count > 0;
        menuActionSave.Enabled = pagesVisible && count > 0;
        menuActionPrint.Enabled = pagesVisible && count > 0;
        menuActionNew.Enabled = pagesVisible && count > 0;
        menuActionImport.Enabled = pagesVisible;
        btnRemove.Enabled = pagesVisible && selected != null;
        var index = selected != null ? flowPanel.Controls.GetChildIndex(selected) : -1;
        btnMoveLeft.Enabled = pagesVisible && index > 0;
        btnMoveRight.Enabled = pagesVisible && index >= 0 && index < count - 1;
        btnZoomOut.Enabled = pagesVisible && thumbWidth > ThumbWidths[0];
        btnZoomIn.Enabled = pagesVisible && thumbWidth < ThumbWidths[^1];
        menuViewZoomOut.Enabled = btnZoomOut.Enabled;
        menuViewZoomIn.Enabled = btnZoomIn.Enabled;
        menuViewFitWidth.Enabled = pagesVisible;
        menuViewFitPage.Enabled = pagesVisible;
        menuViewTwoPages.Enabled = pagesVisible;
        menuViewIcons.Enabled = pagesVisible;
        menuEditCut.Enabled = pagesVisible && selected != null;
        menuEditCopy.Enabled = pagesVisible && selected != null;
        menuEditPaste.Enabled = pagesVisible && clipboardPath != null;
        menuEditDelete.Enabled = pagesVisible && selected != null;
        menuEditRotateLeft.Enabled = pagesVisible && selected != null;
        menuEditRotate180.Enabled = pagesVisible && selected != null;
        menuEditRotateRight.Enabled = pagesVisible && selected != null;
        menuEditCrop.Enabled = pagesVisible && selected != null;
        btnCrop.Enabled = pagesVisible && selected != null;
        menuEditBacks.Enabled = pagesVisible && count >= 2 && count % 2 == 0; // Vorder- und Rückseiten paarweise
        menuEditReverse.Enabled = pagesVisible && count >= 2;
        if (pagesVisible) // im Kopiermodus gehört die Statuszeile dem Kopiermodus
        {
            var scannerHint = selectedScannerName != null ? string.Format(Lng.T("   ·   Scanner: {0}"), selectedScannerName) : string.Empty;
            statusLabel.Text = (count == 0 ? Lng.T("Noch keine Seiten") : count == 1 ? Lng.T("1 Seite") : string.Format(Lng.T("{0} Seiten"), count)) + scannerHint;
        }
    }

    private void BtnMoveLeft_Click(object sender, EventArgs e)
    {
        MoveSelected(-1);
    }

    private void BtnMoveRight_Click(object sender, EventArgs e)
    {
        MoveSelected(1);
    }

    private void MoveSelected(int delta)
    {
        if (selected == null) { return; }
        var index = flowPanel.Controls.GetChildIndex(selected) + delta;
        if (index < 0 || index >= flowPanel.Controls.Count) { return; }
        flowPanel.Controls.SetChildIndex(selected, index);
        UpdateUiState();
    }

    private void BtnRemove_Click(object sender, EventArgs e)
    {
        if (selected == null) { return; }
        var box = selected;
        Select(null);
        flowPanel.Controls.Remove(box);
        PicOf(box).Image?.Dispose();
        box.Dispose();
        UpdateUiState();
    }

    private void BtnNew_Click(object sender, EventArgs e)
    {
        if (flowPanel.Controls.Count > 0 && !TaskDlg.ConfirmTaskDlg(Handle, Lng.T("Alle Seiten aus der Übersicht entfernen?"),
            Lng.T("Die gescannten Seiten dieser Sitzung gehen verloren."), defaultNo: true))
        {
            return;
        }
        Select(null);
        foreach (var box in flowPanel.Controls.Cast<Panel>().ToList())
        {
            flowPanel.Controls.Remove(box);
            PicOf(box).Image?.Dispose();
            box.Dispose();
        }
        UpdateUiState();
    }

    // ------------------------------------------------------------------ Speichern und Drucken

    private async void BtnSave_Click(object sender, EventArgs e)
    {
        var folder = Directory.Exists(settings.SaveDirectory) // bevorzugter Speicherort, sonst Dokumente
            ? settings.SaveDirectory : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var author = string.IsNullOrWhiteSpace(settings.SaveAuthor) ? Environment.UserName : settings.SaveAuthor;
        using SaveForm dialog = new(selected != null, folder, Lng.T("Scan") + " " + DateTime.Now.ToString("yyyy-MM-dd"),
            CurrentOcrLanguage, settings.OcrJpgQuality, author);
        if (dialog.ShowDialog(this) != DialogResult.OK) { return; }
        settings.SaveAuthor = dialog.MetaAuthor; // Verfasser fürs nächste Mal vorbelegen
        settings.Save();
        var extension = dialog.FileType switch { SaveFileType.Jpeg => ".jpg", SaveFileType.Png => ".png", SaveFileType.Tiff => ".tif", _ => ".pdf" };
        var outputPath = Path.Combine(dialog.Folder, dialog.FileName + extension);
        try { Directory.CreateDirectory(dialog.Folder); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            TaskDlg.ErrTaskDlg(Handle, Lng.T("Der Ordner konnte nicht erstellt werden."), ex);
            return;
        }
        if (File.Exists(outputPath) && !TaskDlg.ConfirmTaskDlg(Handle, Lng.T("Die Datei existiert bereits."),
            Lng.T("Soll die vorhandene Datei ersetzt werden?"), TaskDialogIcon.Warning, defaultNo: true))
        {
            return;
        }
        List<string> files = dialog.AllPages
            ? flowPanel.Controls.Cast<Panel>().Select(b => (string)b.Tag).ToList()
            : [(string)selected.Tag];
        if (dialog.FileType != SaveFileType.Pdf)
        {
            try
            {
                if (dialog.FileType == SaveFileType.Jpeg) { ScanService.SaveAsJpeg(files[0], outputPath, dialog.JpgQuality); }
                else if (dialog.FileType == SaveFileType.Png) { ScanService.SaveAsPng(files[0], outputPath); }
                else { ScanService.SaveAsMultipageTiff(files, outputPath); }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Runtime.InteropServices.ExternalException)
            {
                TaskDlg.ErrTaskDlg(Handle, Lng.T("Speichern fehlgeschlagen."), ex);
                return;
            }
        }
        else
        {
            PdfMeta meta = new(dialog.MetaTitle, dialog.MetaSubject, dialog.MetaKeywords, dialog.MetaAuthor);
            await CreatePdfAsync(files, outputPath, dialog.OcrLanguage, dialog.JpgQuality, meta);
        }
        statusLabel.Text = string.Format(Lng.T("Gespeichert: {0}"), outputPath);
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(outputPath) { UseShellExecute = true });
    }

    /// <summary>PDF über die übergebenen Seiten in der aktuellen Reihenfolge — je nach Auswahl im
    /// Speichern-Dialog mit Texterkennung (durchsuchbar) oder als reine Bild-PDF.
    /// Läuft im Hintergrund (die Seiten werden parallel erkannt); die Oberfläche bleibt
    /// bedienbar-gesperrt und zeigt den Fortschritt aus den Worker-Threads.</summary>
    private async Task CreatePdfAsync(List<string> tiffFiles, string outputPdf, string language, int jpgQuality, PdfMeta meta)
    {
        ocrBusy = true; // FormClosing-Guard: nicht mitten in der Texterkennung beenden
        toolStrip.Enabled = false;
        menuStrip.Enabled = false;
        Application.UseWaitCursor = true;
        try
        {
            void Progress(int done, int total) => BeginInvoke(() => // kommt aus den Worker-Threads
                statusLabel.Text = string.Format(Lng.T(language != null ? "Texterkennung {0}/{1} …" : "Seite {0}/{1} …"), done, total));
            await Task.Run(() =>
            {
                if (language != null) { OcrPdfService.CreateSearchablePdf(tiffFiles, outputPdf, language, jpgQuality, Progress, meta); }
                else { OcrPdfService.CreateImagePdf(tiffFiles, outputPdf, jpgQuality, Progress, meta); }
            });
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or Tesseract.TesseractException)
        {
            TaskDlg.ErrTaskDlg(Handle, Lng.T("PDF erstellen fehlgeschlagen."), ex);
        }
        finally
        {
            ocrBusy = false;
            Application.UseWaitCursor = false;
            toolStrip.Enabled = true;
            menuStrip.Enabled = true;
            UpdateUiState();
        }
    }

    private void BtnPrint_Click(object sender, EventArgs e)
    {
        var pages = flowPanel.Controls.Cast<Panel>().Select(b => (string)b.Tag).ToList();
        if (pages.Count == 0) { return; }
        using PrintDocument document = new();
        document.DocumentName = "ScanView";
        ApplySharedPrinterSettings(document); // dieselben Vorgaben wie im Kopiermodus
        var pageIndex = 0;
        document.PrintPage += (s, args) =>
        {
            using var image = ScanService.LoadUnlocked(pages[pageIndex]);
            args.Graphics.DrawImage(image, args.MarginBounds); // in die Ränder eingepasst
            pageIndex++;
            args.HasMorePages = pageIndex < pages.Count;
        };
        // UseEXDialog=false: der klassische Windows-Druckdialog — der moderne zeigt eine Vorschau an,
        // die für WinForms-PrintDocuments nicht funktioniert
        using PrintDialog dialog = new() { Document = document, UseEXDialog = false };
        if (dialog.ShowDialog(this) != DialogResult.OK) { return; }
        try
        {
            document.Print();
            statusLabel.Text = string.Format(Lng.T("{0} Seite(n) an den Drucker übergeben"), pages.Count);
            // Grundeinstellungen aus dem Dialog als gemeinsame Vorgabe übernehmen (gilt auch für den Kopiermodus)
            settings.CopyPrinter = document.PrinterSettings.PrinterName;
            settings.CopyCopies = Math.Clamp((int)document.PrinterSettings.Copies, 1, 99);
            settings.CopyDuplexIndex = document.PrinterSettings.Duplex switch { Duplex.Vertical => 1, Duplex.Horizontal => 2, _ => 0 };
            SyncCopyModeUi();
        }
        catch (InvalidPrinterException ex)
        {
            TaskDlg.ErrTaskDlg(Handle, Lng.T("Drucken fehlgeschlagen."), ex);
        }
    }

    private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        Application.RemoveMessageFilter(this);
        settings.DpiIndex = comboDpi.SelectedIndex;
        settings.ColorIndex = comboColor.SelectedIndex;
        settings.AreaIndex = comboArea.SelectedIndex;
        settings.FeedIndex = comboFeed.SelectedIndex;
        settings.Brightness = trackBrightness.Value;
        settings.ThumbWidth = thumbWidth;
        settings.ScannerId = selectedScannerId;
        settings.ScannerName = selectedScannerName;
        StoreCopyUiInSettings();
        SaveWindowBounds(); // ruft settings.Save()
        if (selfTest) { try { Directory.Delete(sessionFolder, true); } catch (IOException) { } } // Wegwerf-Ordner des Selbsttests
    }

    /// <summary>Zeichnet den Aufklapp-Pfeil des Scannen-SplitButtons größer, als der Standard-Renderer es tut.</summary>
    private sealed class BigArrowRenderer : ToolStripProfessionalRenderer
    {
        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            if (e.Item is not ToolStripSplitButton) { base.OnRenderArrow(e); return; }
            var mid = new Point(e.ArrowRectangle.Left + e.ArrowRectangle.Width / 2, e.ArrowRectangle.Top + e.ArrowRectangle.Height / 2);
            using SolidBrush brush = new(e.ArrowColor);
            var smoothing = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.FillPolygon(brush, [new Point(mid.X - 6, mid.Y - 3), new Point(mid.X + 6, mid.Y - 3), new Point(mid.X, mid.Y + 4)]);
            e.Graphics.SmoothingMode = smoothing;
        }
    }
}
