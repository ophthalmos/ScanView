using ScanView.Classes;

namespace ScanView.Forms;

/// <summary>Gewählter Dateityp im Speichern-Dialog.</summary>
internal enum SaveFileType { Pdf, Jpeg, Png, Tiff }

/// <summary>Speichern-Dialog: Seitenauswahl, Dateiname und Ordner, Dateityp (PDF, JPEG, PNG, TIFF),
/// Texterkennung samt JPEG-Qualität und die PDF-Metadaten. Texterkennung und Metadaten gibt es
/// nur in der PDF; JPEG und PNG kennen keine Seiten und gelten darum nur für die markierte Seite
/// (die Auswahl springt beim Wählen um), TIFF speichert „Alle Seiten" als mehrseitige Datei.</summary>
internal sealed partial class SaveForm : Form
{
    public bool AllPages => radioAll.Checked;

    public string FileName => textFileName.Text.Trim();

    public string Folder => textFolder.Text.Trim();

    public SaveFileType FileType => comboFileType.SelectedIndex switch { 1 => SaveFileType.Jpeg, 2 => SaveFileType.Png, 3 => SaveFileType.Tiff, _ => SaveFileType.Pdf };

    /// <summary>Gewählte OCR-Sprache — null bei „Ohne Texterkennung" oder Bild-Dateitypen.</summary>
    public string OcrLanguage => FileType == SaveFileType.Pdf && comboOcr.SelectedItem is OcrLanguageItem item ? item.Code : null;

    public int JpgQuality => (int)numJpgQuality.Value;

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
        numJpgQuality.Value = Math.Clamp(jpgQuality, 30, 100);
        textAuthor.Text = author ?? string.Empty;
        cbOpenAfter.Checked = openAfter;
        comboFileType.SelectedIndex = 0;
    }

    /// <summary>Nur die PDF trägt Textschicht und Metadaten; JPEG und PNG brauchen die markierte
    /// Seite (eine Bilddatei kennt keine Seiten), die JPEG-Qualität zählt nur für PDF und JPEG.</summary>
    private void ComboFileType_SelectedIndexChanged(object sender, EventArgs e)
    {
        var singlePageOnly = FileType is SaveFileType.Jpeg or SaveFileType.Png;
        if (singlePageOnly)
        {
            if (!hasSelection) { comboFileType.SelectedIndex = 0; return; } // ohne markierte Seite kein JPEG/PNG
            radioSelected.Checked = true;
        }
        radioAll.Enabled = !singlePageOnly;
        var isPdf = FileType == SaveFileType.Pdf;
        labelOcr.Enabled = isPdf;
        comboOcr.Enabled = isPdf;
        groupMeta.Enabled = isPdf;
        var usesJpgQuality = FileType is SaveFileType.Pdf or SaveFileType.Jpeg;
        labelQuality.Enabled = usesJpgQuality;
        numJpgQuality.Enabled = usesJpgQuality;
        labelQualityRange.Enabled = usesJpgQuality;
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
