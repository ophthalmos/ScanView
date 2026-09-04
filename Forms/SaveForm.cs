using ScanView.Classes;

namespace ScanView.Forms;

/// <summary>Gewählter Dateityp im Speichern-Dialog.</summary>
internal enum SaveFileType { Pdf, PdfA, Jpeg, Png, Tiff }

/// <summary>Speichern-Dialog: Seitenauswahl, Dateiname und Ordner, Dateityp (PDF, PDF/A, JPEG,
/// PNG, TIFF), Texterkennung samt JPEG-Qualität und die PDF-Metadaten. Texterkennung gibt es nur
/// in der normalen PDF — PDF/A ist bewusst eine reine Bild-PDF, weil nur so echte Konformität
/// erreichbar ist (s. PdfAHelper); Metadaten tragen PDF und PDF/A. JPEG und PNG kennen keine
/// Seiten und gelten darum nur für die markierte Seite (die Auswahl springt beim Wählen um),
/// TIFF speichert „Alle Seiten" als mehrseitige Datei.</summary>
internal sealed partial class SaveForm : Form
{
    public bool AllPages => radioAll.Checked;

    public string FileName => textFileName.Text.Trim();

    public string Folder => textFolder.Text.Trim();

    public SaveFileType FileType => comboFileType.SelectedIndex switch { 1 => SaveFileType.PdfA, 2 => SaveFileType.Jpeg, 3 => SaveFileType.Png, 4 => SaveFileType.Tiff, _ => SaveFileType.Pdf };

    /// <summary>Gewählte OCR-Sprache — null bei „Ohne Texterkennung" oder Bild-Dateitypen.</summary>
    public string OcrLanguage => FileType == SaveFileType.Pdf && comboOcr.SelectedItem is OcrLanguageItem item ? item.Code : null;

    public int JpgQuality => trackQuality.Value * 5; // der Slider läuft in 5er-Schritten (6–20 → 30–100)

    public string MetaTitle => textTitle.Text.Trim();

    public string MetaSubject => textSubject.Text.Trim();

    public string MetaKeywords => textKeywords.Text.Trim();

    public string MetaAuthor => textAuthor.Text.Trim();

    /// <summary>Die gespeicherte Datei anschließend im Standardprogramm öffnen (letzte Wahl wird gemerkt).</summary>
    public bool OpenAfter => cbOpenAfter.Checked;

    private readonly bool hasSelection;

    public SaveForm(bool hasSelection, string folder, string fileName, string ocrLanguage, int jpgQuality, string author, bool openAfter)
    {
        InitializeComponent();
        Lng.Apply(this);
        Lng.TranslateItems(comboFileType);
        this.hasSelection = hasSelection;
        textTitle.PlaceholderText = Lng.T("wie Dateiname");
        radioSelected.Enabled = hasSelection;
        textFileName.Text = fileName;
        textFolder.Text = folder;
        comboOcr.Items.Add(Lng.T("Ohne Texterkennung"));
        foreach (var code in OcrLanguages.Installed()) { comboOcr.Items.Add(new OcrLanguageItem(code)); }
        var match = comboOcr.Items.OfType<OcrLanguageItem>()
            .FirstOrDefault(item => string.Equals(item.Code, ocrLanguage, StringComparison.OrdinalIgnoreCase));
        if (match != null) { comboOcr.SelectedItem = match; }
        else { comboOcr.SelectedIndex = 0; } // Ohne Texterkennung
        trackQuality.Value = Math.Clamp((int)Math.Round(jpgQuality / 5.0), trackQuality.Minimum, trackQuality.Maximum);
        TrackQuality_ValueChanged(this, EventArgs.Empty); // Wertanzeige auch dann, wenn der Startwert dem Designer-Wert entspricht
        textAuthor.Text = author ?? string.Empty;
        cbOpenAfter.Checked = openAfter;
        comboFileType.SelectedIndex = 0;
    }

    /// <summary>Nur die normale PDF trägt eine Textschicht (PDF/A ist eine reine Bild-PDF);
    /// Metadaten gibt es in PDF und PDF/A. JPEG und PNG brauchen die markierte Seite
    /// (eine Bilddatei kennt keine Seiten), die JPEG-Qualität zählt nicht für PNG und TIFF.</summary>
    private void ComboFileType_SelectedIndexChanged(object sender, EventArgs e)
    {
        var singlePageOnly = FileType is SaveFileType.Jpeg or SaveFileType.Png;
        if (singlePageOnly)
        {
            if (!hasSelection) { comboFileType.SelectedIndex = 0; return; } // ohne markierte Seite kein JPEG/PNG
            radioSelected.Checked = true;
        }
        radioAll.Enabled = !singlePageOnly;
        var hasOcr = FileType == SaveFileType.Pdf;
        labelOcr.Enabled = hasOcr;
        comboOcr.Enabled = hasOcr;
        groupMeta.Enabled = FileType is SaveFileType.Pdf or SaveFileType.PdfA;
        var usesJpgQuality = FileType is SaveFileType.Pdf or SaveFileType.PdfA or SaveFileType.Jpeg;
        labelQuality.Enabled = usesJpgQuality;
        trackQuality.Enabled = usesJpgQuality;
    }

    private void TrackQuality_ValueChanged(object sender, EventArgs e)
    {
        labelQuality.Text = string.Format(Lng.T("JPEG-&Qualität: {0}"), JpgQuality);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (DialogResult == DialogResult.OK)
        {
            if (FileName.Length == 0 || FileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                TaskDlg.MsgTaskDlg(Handle, Lng.T("Ungültiger Dateiname."),
                    Lng.T(@"Bitte gib einen Dateinamen ohne \ / : * ? "" < > | an."), TaskDialogIcon.Warning);
                e.Cancel = true;
            }
            else if (Folder.Length == 0)
            {
                TaskDlg.MsgTaskDlg(Handle, Lng.T("Bitte wähle einen Ordner."), string.Empty, TaskDialogIcon.Warning);
                e.Cancel = true;
            }
        }
        base.OnFormClosing(e);
    }

    private void BtnBrowse_Click(object sender, EventArgs e)
    {
        using FolderBrowserDialog browser = new() { Description = Lng.T("Ordner wählen") };
        if (Directory.Exists(textFolder.Text)) { browser.SelectedPath = textFolder.Text; }
        if (browser.ShowDialog(this) == DialogResult.OK) { textFolder.Text = browser.SelectedPath; }
    }
}
