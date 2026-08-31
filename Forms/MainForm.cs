using System.Drawing.Printing;
using ScanTest.Classes;

namespace ScanTest.Forms;

/// <summary>Arbeitstitel "ScanTest": Seiten scannen (WIA), als Miniaturen ordnen und per
/// Tesseract + PDFsharp in eine durchsuchbare PDF schreiben.</summary>
public partial class MainForm : Form
{
    private const string TestPageId = "TESTSEITE"; // Pseudo-Scanner im Geräte-Menü

    // Miniaturgrößen im A4-Verhältnis (Breite; Höhe = Breite × 1,4)
    private static readonly int[] ThumbWidths = [100, 130, 160, 200, 240, 280];
    private int thumbIndex = 2; // Start: 160 px

    private readonly string sessionFolder = Path.Combine(Path.GetTempPath(), "ScanTest_" + Guid.NewGuid().ToString("N"));
    private readonly bool selfTest;
    private int scanCounter;
    private const int NumberHeight = 18; // Streifen für die Seitenzahl unter dem Bild

    private Panel selected; // Miniatur-Container (Bild + Seitenzahl)
    private Point dragStart; // Mausposition beim Drücken — Start des Miniatur-Ziehens
    private string selectedScannerId; // DeviceID, TestPageId oder null (= noch kein Gerät gewählt)
    private string selectedScannerName;

    public MainForm() : this(false) // parameterlos für den Windows-Forms-Designer
    {
    }

    public MainForm(bool selfTest)
    {
        InitializeComponent();
        this.selfTest = selfTest;
        Directory.CreateDirectory(sessionFolder);
        comboDpi.SelectedIndex = 2;   // 300 dpi — der OCR-Sweet-Spot
        comboColor.SelectedIndex = 0; // Farbe
        comboArea.SelectedIndex = 0;  // maximal
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
        Environment.Exit(pageCount == 2 ? 0 : 1);
    }

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
        string scanned;
        if (selectedScannerId == TestPageId)
        {
            scanned = ScanService.RenderTestPage(NextScanPath(), $"Testseite {scanCounter + 1}",
                "Diese Seite wurde ohne Scanner erzeugt.",
                "Sie dient zum Ausprobieren von Übersicht und Texterkennung.");
        }
        else
        {
            statusLabel.Text = "Scanne …";
            statusStrip.Refresh();
            scanned = selectedScannerId != null
                ? ScanService.ScanFromDevice(selectedScannerId, NextScanPath(), SelectedDpi, SelectedColorIntent, SelectedAreaMm, trackBrightness.Value)
                : ScanService.WiaScanToTiff(NextScanPath()); // noch kein Gerät gewählt → Windows-Dialog
            if (scanned == null)
            {
                statusLabel.Text = "Scan abgebrochen oder fehlgeschlagen";
                return;
            }
        }
        AddPage(scanned);
    }

    /// <summary>Baut das Geräte-Menü auf: alle Scanner plus die Testseite, Auswahl per Häkchen.</summary>
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
        if (splitScan.DropDownItems.Count > 0) { splitScan.DropDownItems.Add(new ToolStripSeparator()); }
        ToolStripMenuItem testPage = new("Testseite (ohne Scanner)") { Checked = selectedScannerId == TestPageId };
        testPage.Click += (s, args) => { selectedScannerId = TestPageId; selectedScannerName = "Testseite"; UpdateUiState(); };
        splitScan.DropDownItems.Add(testPage);
    }

    // ------------------------------------------------------------------ Seitenverwaltung

    /// <summary>Hängt einen Scan als Miniatur (Bild mit Seitenzahl darunter) an die Übersicht an.</summary>
    private void AddPage(string tiffPath)
    {
        if (tiffPath == null) { return; }
        var width = ThumbWidths[thumbIndex];
        Panel thumb = new()
        {
            Width = width,
            Height = width * 7 / 5 + NumberHeight, // A4-Verhältnis plus Seitenzahl-Streifen
            BackColor = Color.Transparent,
            Margin = new Padding(8),
            Tag = tiffPath,
        };
        PictureBox pic = new()
        {
            Bounds = new Rectangle(0, 0, width, width * 7 / 5),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.White,
            Image = ScanService.LoadUnlocked(tiffPath),
            Cursor = Cursors.Hand,
        };
        Label num = new()
        {
            Bounds = new Rectangle(0, pic.Height, width, NumberHeight),
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.White,
            BackColor = Color.Transparent,
        };
        thumb.Controls.Add(pic);
        thumb.Controls.Add(num);
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
        ApplyThumbSize(thumbIndex - 1);
    }

    private void BtnZoomIn_Click(object sender, EventArgs e)
    {
        ApplyThumbSize(thumbIndex + 1);
    }

    private void ApplyThumbSize(int index)
    {
        if (index < 0 || index >= ThumbWidths.Length) { return; }
        thumbIndex = index;
        flowPanel.SuspendLayout();
        var width = ThumbWidths[thumbIndex];
        foreach (var thumb in flowPanel.Controls.Cast<Panel>())
        {
            thumb.Size = new Size(width, width * 7 / 5 + NumberHeight);
            PicOf(thumb).Bounds = new Rectangle(0, 0, width, width * 7 / 5);
            NumOf(thumb).Bounds = new Rectangle(0, width * 7 / 5, width, NumberHeight);
        }
        flowPanel.ResumeLayout();
        UpdateUiState();
    }

    private void Select(Panel thumb)
    {
        if (selected != null) { PicOf(selected).BackColor = Color.White; PicOf(selected).Padding = Padding.Empty; }
        selected = thumb;
        if (selected != null) { PicOf(selected).BackColor = Color.SteelBlue; PicOf(selected).Padding = new Padding(3); }
        UpdateUiState();
    }

    private void UpdateUiState()
    {
        RenumberPages();
        var count = flowPanel.Controls.Count;
        btnSave.Enabled = count > 0;
        btnPrint.Enabled = count > 0;
        btnNew.Enabled = count > 0;
        btnRemove.Enabled = selected != null;
        var index = selected != null ? flowPanel.Controls.GetChildIndex(selected) : -1;
        btnMoveLeft.Enabled = index > 0;
        btnMoveRight.Enabled = index >= 0 && index < count - 1;
        btnZoomOut.Enabled = thumbIndex > 0;
        btnZoomIn.Enabled = thumbIndex < ThumbWidths.Length - 1;
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
        if (flowPanel.Controls.Count > 0 && MessageBox.Show(this, "Alle Seiten aus der Übersicht entfernen?", "Neu",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
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
            Title = "Durchsuchbare PDF speichern",
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) { return; }
        CreatePdf(dialog.FileName);
        statusLabel.Text = $"Gespeichert: {dialog.FileName}";
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dialog.FileName) { UseShellExecute = true });
    }

    /// <summary>Texterkennung (deutsch) über alle Seiten in der aktuellen Reihenfolge, dann Zusammenbau.</summary>
    private void CreatePdf(string outputPdf)
    {
        var tiffFiles = flowPanel.Controls.Cast<Panel>().Select(b => (string)b.Tag).ToList();
        toolStrip.Enabled = false;
        Cursor.Current = Cursors.WaitCursor;
        try
        {
            OcrPdfService.CreateSearchablePdf(tiffFiles, outputPdf, "deu", (done, total) =>
            {
                statusLabel.Text = $"Texterkennung {done}/{total} …";
                statusStrip.Refresh();
            });
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or Tesseract.TesseractException)
        {
            MessageBox.Show(this, ex.Message, "PDF erstellen fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            Cursor.Current = Cursors.Default;
            toolStrip.Enabled = true;
            UpdateUiState();
        }
    }

    private void BtnPrint_Click(object sender, EventArgs e)
    {
        var pages = flowPanel.Controls.Cast<Panel>().Select(b => (string)b.Tag).ToList();
        if (pages.Count == 0) { return; }
        using PrintDocument document = new();
        document.DocumentName = "ScanTest";
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
            MessageBox.Show(this, ex.Message, "Drucken fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        try { Directory.Delete(sessionFolder, true); } catch (IOException) { } // Sitzungs-Scans aufräumen
    }
}
