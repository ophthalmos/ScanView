using ScanView.Classes;

namespace ScanView.Forms;

/// <summary>Gewählter Dateityp im Speichern-Dialog.</summary>
internal enum SaveFileType { Pdf, Jpeg, Tiff }

/// <summary>Speichern-Dialog: Seitenauswahl, Dateiname und Ordner, Dateityp (PDF, JPEG, TIFF),
/// Texterkennung samt JPEG-Qualität und die PDF-Metadaten. Texterkennung und Metadaten gibt es
/// nur in der PDF; JPEG kennt keine Seiten und gilt darum nur für die markierte Seite (die
/// Auswahl springt beim Wählen um), TIFF speichert „Alle Seiten" als mehrseitige Datei.</summary>
internal sealed partial class SaveForm : Form
{
    public bool AllPages => radioAll.Checked;

    public string FileName => textFileName.Text.Trim();

    public string Folder => textFolder.Text.Trim();

    public SaveFileType FileType => comboFileType.SelectedIndex switch { 1 => SaveFileType.Jpeg, 2 => SaveFileType.Tiff, _ => SaveFileType.Pdf };

    /// <summary>Gewählte OCR-Sprache — null bei „Ohne Texterkennung" oder Bild-Dateitypen.</summary>
    public string OcrLanguage => FileType == SaveFileType.Pdf && comboOcr.SelectedItem is OcrLanguageItem item ? item.Code : null;

    public int JpgQuality => (int)numJpgQuality.Value;

    public string MetaTitle => textTitle.Text.Trim();

    public string MetaSubject => textSubject.Text.Trim();

    public string MetaKeywords => textKeywords.Text.Trim();

    public string MetaAuthor => textAuthor.Text.Trim();

    private readonly bool hasSelection;

    public SaveForm(bool hasSelection, string folder, string fileName, string ocrLanguage, int jpgQuality, string author)
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
        comboFileType.SelectedIndex = 0;
    }

    /// <summary>Nur die PDF trägt Textschicht und Metadaten; JPEG braucht die markierte Seite
    /// (eine Bilddatei kennt keine Seiten), TIFF speichert verlustfrei ohne JPEG-Qualität.</summary>
    private void ComboFileType_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (FileType == SaveFileType.Jpeg)
        {
            if (!hasSelection) { comboFileType.SelectedIndex = 0; return; } // ohne markierte Seite kein JPEG
            radioSelected.Checked = true;
        }
        radioAll.Enabled = FileType != SaveFileType.Jpeg;
        var isPdf = FileType == SaveFileType.Pdf;
        labelOcr.Enabled = isPdf;
        comboOcr.Enabled = isPdf;
        groupMeta.Enabled = isPdf;
        labelQuality.Enabled = FileType != SaveFileType.Tiff;
        numJpgQuality.Enabled = FileType != SaveFileType.Tiff;
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
