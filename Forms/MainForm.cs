using ScanTest.Classes;

namespace ScanTest.Forms;

/// <summary>Arbeitstitel "ScanTest": Seiten scannen (WIA), als Miniaturen ordnen und per
/// Tesseract + PDFsharp in eine durchsuchbare PDF schreiben. Entstanden aus dem
/// Machbarkeits-Test der PDFlight-Session vom 31.08.2026.</summary>
public partial class MainForm : Form
{
    private readonly string sessionFolder = Path.Combine(Path.GetTempPath(), "ScanTest_" + Guid.NewGuid().ToString("N"));
    private readonly bool selfTest;
    private int scanCounter;
    private PictureBox selected;

    public MainForm(bool selfTest = false)
    {
        InitializeComponent();
        this.selfTest = selfTest;
        Directory.CreateDirectory(sessionFolder);
        Shown += MainForm_Shown;
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
        catch (Exception ex) when (PdfReadError(ex)) { }
        using (var shot = new Bitmap(Width, Height))
        {
            DrawToBitmap(shot, new Rectangle(Point.Empty, Size));
            shot.Save(Path.Combine(AppContext.BaseDirectory, "selftest.png"));
        }
        Environment.Exit(pageCount == 2 ? 0 : 1);
    }

    private static bool PdfReadError(Exception ex) => ex is PdfSharp.PdfSharpException or IOException or InvalidOperationException;

    private string NextScanPath() => Path.Combine(sessionFolder, $"scan_{++scanCounter:D3}.tif");

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
        UpdateUiState();
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
        btnCreatePdf.Enabled = count > 0;
        btnRemove.Enabled = selected != null;
        var index = selected != null ? flowPanel.Controls.GetChildIndex(selected) : -1;
        btnMoveLeft.Enabled = index > 0;
        btnMoveRight.Enabled = index >= 0 && index < count - 1;
        statusLabel.Text = count == 0 ? "Noch keine Seiten"
            : count == 1 ? "1 Seite"
            : $"{count} Seiten";
    }

    // ------------------------------------------------------------------ Toolbar

    private void BtnScan_Click(object sender, EventArgs e)
    {
        statusLabel.Text = "Scandialog geöffnet …";
        var scanned = ScanService.WiaScanToTiff(NextScanPath());
        if (scanned == null) { UpdateUiState(); return; } // Abbruch oder kein Scanner
        AddPage(scanned);
    }

    private void BtnTestPage_Click(object sender, EventArgs e)
    {
        AddPage(ScanService.RenderTestPage(NextScanPath(), $"Testseite {scanCounter + 1}",
            "Diese Seite wurde ohne Scanner erzeugt.",
            "Sie dient zum Ausprobieren von Übersicht und Texterkennung."));
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

    private void BtnCreatePdf_Click(object sender, EventArgs e)
    {
        using SaveFileDialog dialog = new()
        {
            Filter = "PDF-Dateien (*.pdf)|*.pdf",
            FileName = "Scan " + DateTime.Now.ToString("yyyy-MM-dd") + ".pdf",
            Title = "Durchsuchbare PDF erstellen",
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) { return; }
        CreatePdf(dialog.FileName);
        if (!selfTest)
        {
            statusLabel.Text = $"Erstellt: {dialog.FileName}";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dialog.FileName) { UseShellExecute = true });
        }
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

    private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        try { Directory.Delete(sessionFolder, true); } catch (IOException) { } // Sitzungs-Scans aufräumen
    }
}
