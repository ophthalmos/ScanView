using ScanView.Classes;

namespace ScanView.Forms;

/// <summary>Gewählter Dateityp im Speichern-Dialog.</summary>
internal enum SaveFileType { Pdf, Jpeg }

/// <summary>Speichern-Dialog: Seitenauswahl, Dateiname und Ordner, Dateityp (PDF, JPEG),
/// Texterkennung samt JPEG-Qualität und die PDF-Metadaten. JPEG steht nur für die markierte
/// Seite zur Wahl (eine Bilddatei kennt keine Seiten) — ohne Textschicht und Metadaten.</summary>
internal sealed partial class SaveForm : Form
{
    public bool AllPages => radioAll.Checked;

    public string FileName => textFileName.Text.Trim();

    public string Folder => textFolder.Text.Trim();

    public SaveFileType FileType => comboFileType.SelectedIndex == 1 ? SaveFileType.Jpeg : SaveFileType.Pdf;

    /// <summary>Gewählte OCR-Sprache — null bei „Ohne Texterkennung" oder Dateityp JPEG.</summary>
    public string OcrLanguage => FileType != SaveFileType.Jpeg && comboOcr.SelectedItem is OcrLanguageItem item ? item.Code : null;

    public int JpgQuality => (int)numJpgQuality.Value;

    public string MetaTitle => textTitle.Text.Trim();

    public string MetaSubject => textSubject.Text.Trim();

    public string MetaKeywords => textKeywords.Text.Trim();

    public string MetaAuthor => textAuthor.Text.Trim();

    private readonly string jpegItem; // übersetzter JPEG-Eintrag — wird bei „Alle Seiten" aus der Liste genommen

    public SaveForm(bool hasSelection, string folder, string fileName, string ocrLanguage, int jpgQuality, string author)
    {
        InitializeComponent();
        Lng.Apply(this);
        Lng.TranslateItems(comboFileType);
        jpegItem = (string)comboFileType.Items[1];
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
        RadioPages_CheckedChanged(radioAll, EventArgs.Empty);
    }

    /// <summary>JPEG gibt es nur für die markierte Seite — bei „Alle Seiten" verschwindet der Eintrag.</summary>
    private void RadioPages_CheckedChanged(object sender, EventArgs e)
    {
        if (radioAll.Checked && comboFileType.Items.Count == 2)
        {
            if (comboFileType.SelectedIndex == 1) { comboFileType.SelectedIndex = 0; }
            comboFileType.Items.RemoveAt(1);
        }
        else if (!radioAll.Checked && comboFileType.Items.Count == 1)
        {
            comboFileType.Items.Add(jpegItem);
        }
    }

    /// <summary>Eine JPEG-Datei trägt weder Textschicht noch PDF-Metadaten —
    /// die JPEG-Qualität gilt dagegen auch für die Bilddatei und bleibt aktiv.</summary>
    private void ComboFileType_SelectedIndexChanged(object sender, EventArgs e)
    {
        var isPdf = FileType != SaveFileType.Jpeg;
        labelOcr.Enabled = isPdf;
        comboOcr.Enabled = isPdf;
        groupMeta.Enabled = isPdf;
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
