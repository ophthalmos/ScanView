using System.Drawing.Printing;
using ScanTest.Classes;

namespace ScanTest.Forms;

/// <summary>Arbeitstitel "ScanTest": Seiten scannen (WIA), als Miniaturen ordnen und per
/// Tesseract + PDFsharp in eine durchsuchbare PDF schreiben.</summary>
public partial class MainForm : Form
{
    private const string TestPageId = "TESTSEITE"; // Pseudo-Scanner im Geräte-Menü

    private readonly string sessionFolder = Path.Combine(Path.GetTempPath(), "ScanTest_" + Guid.NewGuid().ToString("N"));
    private readonly bool selfTest;
    private int scanCounter;
    private PictureBox selected;
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
                ? ScanService.ScanFromDevice(selectedScannerId, NextScanPath(), SelectedDpi, SelectedColorIntent)
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

    /// <summary>Hängt einen Scan als Miniatur an die Übersicht an.</summary>
    private void AddPage(string tiffPath)
    {
        if (tiffPath == null) { return; }
        PictureBox box = new()
        {
            Width = 160,
            Height = 224, // A4-Verhältnis
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.White,
            Margin = new Padding(8),
            Image = ScanService.LoadUnlocked(tiffPath),
            Tag = tiffPath,
            Cursor = Cursors.Hand,
        };
        box.Click += (s, e) => Select(box);
        box.DoubleClick += (s, e) => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(tiffPath) { UseShellExecute = true });
        flowPanel.Controls.Add(box);
        Select(box);
    }

    private void Select(PictureBox box)
    {
        if (selected != null) { selected.BackColor = Color.White; selected.Padding = Padding.Empty; }
        selected = box;
        if (selected != null) { selected.BackColor = Color.SteelBlue; selected.Padding = new Padding(3); }
        UpdateUiState();
    }

    private void UpdateUiState()
    {
        var count = flowPanel.Controls.Count;
        btnSave.Enabled = count > 0;
        btnPrint.Enabled = count > 0;
        btnNew.Enabled = count > 0;
        btnRemove.Enabled = selected != null;
        var index = selected != null ? flowPanel.Controls.GetChildIndex(selected) : -1;
        btnMoveLeft.Enabled = index > 0;
        btnMoveRight.Enabled = index >= 0 && index < count - 1;
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
        box.Image?.Dispose();
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
        foreach (var box in flowPanel.Controls.Cast<PictureBox>().ToList())
        {
            flowPanel.Controls.Remove(box);
            box.Image?.Dispose();
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
        var tiffFiles = flowPanel.Controls.Cast<PictureBox>().Select(b => (string)b.Tag).ToList();
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
        var pages = flowPanel.Controls.Cast<PictureBox>().Select(b => (string)b.Tag).ToList();
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
