using System.Drawing.Printing;
using ScanView.Classes;

namespace ScanView.Forms;

/// <summary>ScanView: Seiten scannen (WIA), als Miniaturen ordnen und per
/// Tesseract + PDFsharp in eine durchsuchbare PDF schreiben.</summary>
public partial class MainForm : Form
{
    // Zoomstufen für −/+ im A4-Verhältnis (Breite; Höhe = Breite × 1,4)
    private static readonly int[] ThumbWidths = [100, 130, 160, 200, 240, 280];
    private const int IconThumbWidth = 160; // Ansicht „Symbole" und Startgröße
    private int thumbWidth = IconThumbWidth; // Ansicht-Modi dürfen von den Zoomstufen abweichen

    private readonly string sessionFolder; // Seitenablage: persistent, damit „Seiten behalten" beim Beenden möglich ist
    private readonly bool selfTest;
    private int scanCounter;
    private const int NumberHeight = 18; // Streifen für die Seitenzahl unter dem Bild
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
    private readonly ToolTip toolTip = new();
    private Button btnZoomOut; // übereinander gestapelt in einem ToolStripControlHost (s. CreateZoomButtons)
    private Button btnZoomIn;

    public MainForm() : this(false) // parameterlos für den Windows-Forms-Designer
    {
    }

    public MainForm(bool selfTest)
    {
        InitializeComponent();
        toolStrip.Renderer = new BigArrowRenderer();
        // Eigenes Menü statt des auto-generierten: das erbt in WinForms live die fette
        // 11-pt-Schrift des Toolbar-Buttons (gleiche Lösung wie in PDFlight)
        splitScan.DropDown = new ToolStripDropDownMenu { Font = new Font(Font.FontFamily, 9f) };
        this.selfTest = selfTest;
        settings = AppSettings.Load();
        sessionFolder = selfTest // der Selbsttest bleibt in einem Wegwerf-Ordner
            ? Path.Combine(Path.GetTempPath(), "ScanView_" + Guid.NewGuid().ToString("N"))
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ScanView", "Seiten");
        Directory.CreateDirectory(sessionFolder);
        foreach (var file in Directory.EnumerateFiles(sessionFolder, "scan_*")) // Zähler hinter vorhandene Seiten setzen
        {
            if (int.TryParse(Path.GetFileNameWithoutExtension(file).AsSpan(5), out var number)) { scanCounter = Math.Max(scanCounter, number); }
        }
        FormClosing += MainForm_FormClosing;
        int Clamped(int value, ComboBox combo, int fallback) => value >= 0 && value < combo.Items.Count ? value : fallback;
        comboDpi.SelectedIndex = Clamped(settings.DpiIndex, comboDpi, 2);      // Standard: 300 dpi — der OCR-Sweet-Spot
        comboColor.SelectedIndex = Clamped(settings.ColorIndex, comboColor, 0);
        comboArea.SelectedIndex = Clamped(settings.AreaIndex, comboArea, 0);
        comboFeed.SelectedIndex = Clamped(settings.FeedIndex, comboFeed, 0);
        trackBrightness.Value = Math.Clamp(settings.Brightness, trackBrightness.Minimum, trackBrightness.Maximum);
        comboOcr.Items.Add("Ohne Texterkennung");
        foreach (var code in OcrLanguages.Installed()) { comboOcr.Items.Add(new OcrLanguageItem(code)); }
        SelectOcrLanguage(settings.OcrLanguage); // bevorzugte Sprache aus den Optionen als Vorgabe
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } // Fenstersymbol = Programmicon der EXE
        catch (Exception ex) when (ex is ArgumentException or IOException) { }
        CreateZoomButtons();
        ApplyToolbarIcons();
        ApplyMenuIcons();
        if (!selfTest) // der Selbsttest-Screenshot soll deterministisch bleiben
        {
            RestoreWindowBounds();
            thumbWidth = Math.Max(ThumbWidths[0], settings.ThumbWidth);
            selectedScannerId = settings.ScannerId; // zuletzt benutzter Scanner; geprüft wird erst beim Scannen
            selectedScannerName = settings.ScannerName;
            foreach (var file in settings.PageFiles.Where(File.Exists)) { AddPage(file); } // „Seiten behalten"
            Select(null);
        }
    }

    /// <summary>Beenden-Verhalten aus den Optionen: Seiten behalten, nach Rückfrage leeren oder still leeren.</summary>
    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (selfTest) { return; }
        var pages = flowPanel.Controls.Cast<Panel>().Select(p => (string)p.Tag).ToList();
        var keep = pages.Count > 0 && (settings.ExitAction == 0
            || (settings.ExitAction == 1 && !TaskDlg.ConfirmTaskDlg(Handle, "Seitenübersicht leeren?",
                "Bei Nein stehen die Seiten beim nächsten Programmstart wieder in der Übersicht.")));
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
        menuExtrasOptions.Image = Icon16(ToolbarIcons.Settings);
        menuHelpShortcuts.Image = Icon16(ToolbarIcons.Help);
        menuHelpAbout.Image = Icon16(ToolbarIcons.Info);
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

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        switch (keyData)
        {
            case Keys.Control | Keys.Add: BtnZoomIn_Click(this, EventArgs.Empty); return true;      // Ziffernblock; Strg+± liegt auf den Menükürzeln
            case Keys.Control | Keys.Subtract: BtnZoomOut_Click(this, EventArgs.Empty); return true;
            case Keys.Alt | Keys.Left: MoveSelected(-1); return true;
            case Keys.Alt | Keys.Right: MoveSelected(1); return true;
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
        statusLabel.Text = "Esc erneut drücken, um das Programm zu beenden";
        return true;
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        if (!selfTest) { return; }
        // Selbsttest für die Werkzeugkette: zwei Testseiten, PDF erstellen, Ergebnis melden, beenden
        AddPage(ScanService.RenderTestPage(NextScanPath(), "Selbsttest Seite eins", "Der Blutdruck lag bei 120 zu 80 mmHg."));
        AddPage(ScanService.RenderTestPage(NextScanPath(), "Selbsttest Seite zwei", "Prüfung der Umlaute: Ärzte, Öfen, Übungen."));
        var output = Path.Combine(sessionFolder, "Selbsttest.pdf");
        CreatePdf(output);
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
        using (SettingsForm settingsDialog = new(true, 0, "", "deu", 75)) // und der Optionen-Dialog
        {
            settingsDialog.StartPosition = FormStartPosition.Manual;
            settingsDialog.Show(this);
            using var shot = new Bitmap(settingsDialog.Width, settingsDialog.Height);
            settingsDialog.DrawToBitmap(shot, new Rectangle(Point.Empty, settingsDialog.Size));
            shot.Save(Path.Combine(AppContext.BaseDirectory, "selftest-settings.png"));
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
        labelBrightness.Text = $"&Helligkeit: {trackBrightness.Value}";
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
                TaskDlg.MsgTaskDlg(Handle, "Kein Scanner gefunden.",
                    "Bitte schließe einen Scanner an oder schalte ihn ein.", TaskDialogIcon.Warning);
                return;
            }
        }
        statusLabel.Text = "Scanne …";
        statusStrip.Refresh();
        var scanned = ScanService.ScanFromDevice(selectedScannerId, NextScanPath(), SelectedDpi, SelectedColorIntent, SelectedAreaMm, trackBrightness.Value, comboFeed.SelectedIndex == 1);
        if (scanned == null)
        {
            statusLabel.Text = "Scan abgebrochen oder fehlgeschlagen";
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
            var defaultIndex = comboCopyPrinter.Items.IndexOf(copyPrinterSettings.PrinterName); // Standarddrucker
            comboCopyPrinter.SelectedIndex = defaultIndex >= 0 ? defaultIndex : (comboCopyPrinter.Items.Count > 0 ? 0 : -1);
        }
        panelCopyMode.Visible = active;
        flowPanel.Visible = !active;
        btnCopyMode.Text = active ? "&Kopiermodus beenden" : "&Kopiermodus";
        menuActionCopyMode.Checked = active;
        statusLabel.Text = active ? "Kopiermodus: jeder Scan wird direkt gedruckt" : string.Empty;
        if (!active) { UpdateUiState(); }
    }

    /// <summary>Öffnet den Windows-Druckdialog für die Treiber-Einstellungen des gewählten Druckers
    /// (Duplex, Papierfach, Farbe …) und übernimmt dort geänderte Werte.</summary>
    private void BtnCopyPrinterSettings_Click(object sender, EventArgs e)
    {
        if (comboCopyPrinter.SelectedItem is string printer) { copyPrinterSettings.PrinterName = printer; }
        copyPrinterSettings.Copies = (short)numCopies.Value;
        using PrintDialog dialog = new() { PrinterSettings = copyPrinterSettings, UseEXDialog = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) { return; }
        var index = comboCopyPrinter.Items.IndexOf(copyPrinterSettings.PrinterName); // Auswahl aus dem Dialog übernehmen
        if (index >= 0) { comboCopyPrinter.SelectedIndex = index; }
        numCopies.Value = Math.Clamp((int)copyPrinterSettings.Copies, (int)numCopies.Minimum, (int)numCopies.Maximum);
    }

    /// <summary>Druckt einen frischen Scan sofort mit den Kopiermodus-Einstellungen.</summary>
    private void PrintCopy(string tiffPath)
    {
        using PrintDocument document = new();
        document.DocumentName = "ScanView Kopie";
        if (comboCopyPrinter.SelectedItem is string printer) { copyPrinterSettings.PrinterName = printer; }
        copyPrinterSettings.Copies = (short)numCopies.Value;
        document.PrinterSettings = copyPrinterSettings;
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
            statusLabel.Text = $"Kopie ({numCopies.Value}×) an {document.PrinterSettings.PrinterName} übergeben";
        }
        catch (InvalidPrinterException ex)
        {
            TaskDlg.ErrTaskDlg(Handle, "Drucken fehlgeschlagen.", ex);
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
            splitScan.DropDownItems.Add(new ToolStripMenuItem("(kein Scanner gefunden)") { Enabled = false });
        }
    }

    // ------------------------------------------------------------------ Menü „Aktion"

    /// <summary>Bilddateien als Seiten aufnehmen — Kopien im Sitzungsordner, damit die
    /// Originale unangetastet bleiben und die Aufräumlogik beim Beenden greift.</summary>
    private void MenuImport_Click(object sender, EventArgs e)
    {
        using OpenFileDialog dialog = new()
        {
            Filter = "Bilddateien (*.tif;*.tiff;*.png;*.jpg;*.jpeg;*.bmp)|*.tif;*.tiff;*.png;*.jpg;*.jpeg;*.bmp|Alle Dateien (*.*)|*.*",
            Multiselect = true,
            Title = "Bilder importieren",
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
                TaskDlg.ErrTaskDlg(Handle, "Importieren fehlgeschlagen.", ex);
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
        pic.Image = ScanService.LoadUnlocked((string)selected.Tag);
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
        statusLabel.Text = $"Seite bearbeitet übernommen ({dialog.ResultImage.Width} × {dialog.ResultImage.Height} Pixel)";
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

    private void MenuEditReverse_Click(object sender, EventArgs e)
    {
        ReorderPages(flowPanel.Controls.Cast<Panel>().Reverse().ToList());
    }

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

    private void MenuViewFitWidth_Click(object sender, EventArgs e)
    {
        ApplyThumbWidth(ColumnThumbWidth(1));
    }

    private void MenuViewTwoPages_Click(object sender, EventArgs e)
    {
        ApplyThumbWidth(ColumnThumbWidth(2));
    }

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

    private void MenuExtrasOptions_Click(object sender, EventArgs e)
    {
        using SettingsForm dialog = new(settings.CloseOnEscape, settings.ExitAction, settings.SaveDirectory, settings.OcrLanguage, settings.OcrJpgQuality);
        if (dialog.ShowDialog(this) != DialogResult.OK) { return; }
        settings.CloseOnEscape = dialog.CloseOnEscape;
        settings.ExitAction = dialog.ExitAction;
        settings.SaveDirectory = dialog.SaveDirectory;
        settings.OcrLanguage = dialog.OcrLanguage;
        settings.OcrJpgQuality = dialog.OcrJpgQuality;
        settings.Save();
        SelectOcrLanguage(settings.OcrLanguage); // neue bevorzugte Sprache auch für den aktuellen Scan übernehmen
    }

    private void MenuHelpShortcuts_Click(object sender, EventArgs e)
    {
        TaskDlg.ShowShortcutsPdf(Handle, Icon);
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
            Image = ScanService.LoadUnlocked(tiffPath),
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
        pic.DoubleClick += (s, e) => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(tiffPath) { UseShellExecute = true });
        pic.MouseDown += (s, e) => dragStart = e.Location;
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
        if (selected != null) { selected.BackColor = FrameColor; }
        selected = thumb;
        if (selected != null) { selected.BackColor = SelectionColor; }
        UpdateUiState();
    }

    private void UpdateUiState()
    {
        RenumberPages();
        var count = flowPanel.Controls.Count;
        btnSave.Enabled = count > 0;
        btnPrint.Enabled = count > 0;
        btnNew.Enabled = count > 0;
        menuActionSave.Enabled = count > 0;
        menuActionPrint.Enabled = count > 0;
        menuActionNew.Enabled = count > 0;
        btnRemove.Enabled = selected != null;
        var index = selected != null ? flowPanel.Controls.GetChildIndex(selected) : -1;
        btnMoveLeft.Enabled = index > 0;
        btnMoveRight.Enabled = index >= 0 && index < count - 1;
        btnZoomOut.Enabled = thumbWidth > ThumbWidths[0];
        btnZoomIn.Enabled = thumbWidth < ThumbWidths[^1];
        menuViewZoomOut.Enabled = btnZoomOut.Enabled;
        menuViewZoomIn.Enabled = btnZoomIn.Enabled;
        menuEditCut.Enabled = selected != null;
        menuEditCopy.Enabled = selected != null;
        menuEditPaste.Enabled = clipboardPath != null;
        menuEditDelete.Enabled = selected != null;
        menuEditRotateLeft.Enabled = selected != null;
        menuEditRotate180.Enabled = selected != null;
        menuEditRotateRight.Enabled = selected != null;
        menuEditCrop.Enabled = selected != null;
        btnCrop.Enabled = selected != null;
        menuEditBacks.Enabled = count >= 2 && count % 2 == 0; // Vorder- und Rückseiten paarweise
        menuEditReverse.Enabled = count >= 2;
        var scannerHint = selectedScannerName != null ? $"   ·   Scanner: {selectedScannerName}" : string.Empty;
        statusLabel.Text = (count == 0 ? "Noch keine Seiten" : count == 1 ? "1 Seite" : $"{count} Seiten") + scannerHint;
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
        if (flowPanel.Controls.Count > 0 && !TaskDlg.ConfirmTaskDlg(Handle, "Alle Seiten aus der Übersicht entfernen?",
            "Die gescannten Seiten dieser Sitzung gehen verloren.", defaultNo: true))
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

    private void BtnSave_Click(object sender, EventArgs e)
    {
        using SaveFileDialog dialog = new()
        {
            Filter = "PDF-Dateien (*.pdf)|*.pdf",
            FileName = "Scan " + DateTime.Now.ToString("yyyy-MM-dd") + ".pdf",
            Title = "PDF speichern",
        };
        if (Directory.Exists(settings.SaveDirectory)) { dialog.InitialDirectory = settings.SaveDirectory; } // bevorzugter Speicherort
        if (dialog.ShowDialog(this) != DialogResult.OK) { return; }
        CreatePdf(dialog.FileName);
        statusLabel.Text = $"Gespeichert: {dialog.FileName}";
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dialog.FileName) { UseShellExecute = true });
    }

    /// <summary>PDF über alle Seiten in der aktuellen Reihenfolge — je nach Auswahl im
    /// Einstellungsbereich mit Texterkennung (durchsuchbar) oder als reine Bild-PDF.</summary>
    private void CreatePdf(string outputPdf)
    {
        var tiffFiles = flowPanel.Controls.Cast<Panel>().Select(b => (string)b.Tag).ToList();
        var language = CurrentOcrLanguage;
        toolStrip.Enabled = false;
        menuStrip.Enabled = false;
        Cursor.Current = Cursors.WaitCursor;
        try
        {
            void Progress(int done, int total)
            {
                statusLabel.Text = language != null ? $"Texterkennung {done}/{total} …" : $"Seite {done}/{total} …";
                statusStrip.Refresh();
            }
            if (language != null) { OcrPdfService.CreateSearchablePdf(tiffFiles, outputPdf, language, settings.OcrJpgQuality, Progress); }
            else { OcrPdfService.CreateImagePdf(tiffFiles, outputPdf, settings.OcrJpgQuality, Progress); }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or Tesseract.TesseractException)
        {
            TaskDlg.ErrTaskDlg(Handle, "PDF erstellen fehlgeschlagen.", ex);
        }
        finally
        {
            Cursor.Current = Cursors.Default;
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
        var pageIndex = 0;
        document.PrintPage += (s, args) =>
        {
            using var image = ScanService.LoadUnlocked(pages[pageIndex]);
            args.Graphics.DrawImage(image, args.MarginBounds); // in die Ränder eingepasst
            pageIndex++;
            args.HasMorePages = pageIndex < pages.Count;
        };
        using PrintDialog dialog = new() { Document = document, UseEXDialog = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) { return; }
        try
        {
            document.Print();
            statusLabel.Text = $"{pages.Count} Seite(n) an den Drucker übergeben";
        }
        catch (InvalidPrinterException ex)
        {
            TaskDlg.ErrTaskDlg(Handle, "Drucken fehlgeschlagen.", ex);
        }
    }

    private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        settings.DpiIndex = comboDpi.SelectedIndex;
        settings.ColorIndex = comboColor.SelectedIndex;
        settings.AreaIndex = comboArea.SelectedIndex;
        settings.FeedIndex = comboFeed.SelectedIndex;
        settings.Brightness = trackBrightness.Value;
        settings.ThumbWidth = thumbWidth;
        settings.ScannerId = selectedScannerId;
        settings.ScannerName = selectedScannerName;
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
            e.Graphics.FillPolygon(brush, new[] { new Point(mid.X - 6, mid.Y - 3), new Point(mid.X + 6, mid.Y - 3), new Point(mid.X, mid.Y + 4) });
            e.Graphics.SmoothingMode = smoothing;
        }
    }
}
