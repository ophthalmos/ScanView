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

    /// <summary>JPEG-Qualität für den Encoder (30–100), gemappt aus den 13 Photoshop-Stufen 0–12.</summary>
    public int JpgQuality => 30 + (int)Math.Round(trackQuality.Value * 70 / 12.0);

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
        Lng.TranslateItems(comboFileType, comboQuality); // beide werden über SelectedIndex ausgewertet
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
        trackQuality.Value = Math.Clamp((int)Math.Round((jpgQuality - 30) * 12 / 70.0), 0, 12);
        TrackQuality_ValueChanged(this, EventArgs.Empty); // TextBox und Stufen-Combo auch beim Designer-Startwert füllen
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
        foreach (Control control in new Control[] { labelQuality, textBoxQuality, comboQuality, trackQuality, labelLowSize, labelLargeSize })
        {
            control.Enabled = usesJpgQuality;
        }
    }

    // ------------------------------------------------------------------ Qualitätsstufen (0–12 wie in Photoshop)

    private bool syncingQuality; // der Slider ist der zentrale Wert; TextBox und Stufen-Combo folgen ihm

    /// <summary>Verteilt den Sliderwert an TextBox und Stufen-Combo (0–4 Niedrig, 5–7 Mittel, 8–12 Hoch).</summary>
    private void TrackQuality_ValueChanged(object sender, EventArgs e)
    {
        syncingQuality = true;
        var value = trackQuality.Value;
        if (!int.TryParse(textBoxQuality.Text, out var typed) || typed != value) { textBoxQuality.Text = value.ToString(); }
        comboQuality.SelectedIndex = value <= 4 ? 0 : value <= 7 ? 1 : 2;
        syncingQuality = false;
    }

    private void TextBoxQuality_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) { e.Handled = true; } // nur Ziffern
    }

    private void TextBoxQuality_TextChanged(object sender, EventArgs e)
    {
        if (syncingQuality || !int.TryParse(textBoxQuality.Text, out var value)) { return; } // leer: erst beim Verlassen normalisieren
        if (value > 12) // Eingaben über dem Maximum sofort kappen
        {
            textBoxQuality.Text = "12";
            textBoxQuality.SelectionStart = textBoxQuality.TextLength;
            return;
        }
        trackQuality.Value = value; // verteilt über ValueChanged an die Stufen-Combo
    }

    private void TextBoxQuality_Leave(object sender, EventArgs e)
    {
        if (!int.TryParse(textBoxQuality.Text, out var value) || value != trackQuality.Value)
        {
            textBoxQuality.Text = trackQuality.Value.ToString(); // leere/ungültige Eingabe zurücksetzen
        }
    }

    /// <summary>Stufenwahl setzt den typischen Wert der Kategorie (wie in Photoshop: 3 / 5 / 8).</summary>
    private void ComboQuality_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (syncingQuality) { return; }
        trackQuality.Value = comboQuality.SelectedIndex switch { 0 => 3, 1 => 5, _ => 8 };
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
