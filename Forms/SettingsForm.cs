using ScanView.Classes;

namespace ScanView.Forms;

/// <summary>Einstellungsdialog (Extras → Optionen): Allgemein und Texterkennung.</summary>
internal sealed partial class SettingsForm : Form
{
    public bool CloseOnEscape => cbCloseOnEscape.Checked;

    public int ExitAction => rbExitAsk.Checked ? 1 : rbExitClear.Checked ? 2 : 0;

    public string OcrLanguage => comboLanguage.SelectedItem is OcrLanguageItem item ? item.Code : "deu";

    public int OcrJpgQuality => trackQuality.Value * 5; // der Slider läuft intern in 5er-Einheiten

    public string SaveDirectory => textSaveDirectory.Text.Trim();

    private static readonly string[] LanguageCodes = ["de", "en", "fr", "es"]; // Reihenfolge der comboUiLanguage-Einträge

    /// <summary>Gewählte GUI-Sprache ("de", "en", "fr", "es").</summary>
    public string LanguageCode => comboUiLanguage.SelectedIndex >= 0 ? LanguageCodes[comboUiLanguage.SelectedIndex] : "de";

    /// <summary>Gewählte Hintergrundfarbe der Seitenübersicht (eines der Farbfelder).</summary>
    public Color OverviewBackColor =>
        BackColorRadios().FirstOrDefault(r => r.Checked)?.BackColor ?? Color.White;

    private RadioButton[] BackColorRadios() => [rbBackWhite, rbBackBlue, rbBackGreen, rbBackYellow, rbBackRose, rbBackGray];

    public SettingsForm(string languageCode, bool closeOnEscape, int exitAction, string saveDirectory, Color overviewBackColor, string ocrLanguage, int ocrJpgQuality)
    {
        InitializeComponent();
        Lng.Apply(this);
        // Mehrzeilige Texte brauchen explizite Schlüssel (Zeilenumbrüche taugen nicht als resx-Schlüssel)
        labelLanguageHint.Text = Lng.T("Hint.OcrLanguages", labelLanguageHint.Text);
        labelTessInstruction.Text = Lng.T("Hint.TessdataSteps", labelTessInstruction.Text);
        labelQualityHint.Text = Lng.T("Hint.JpgQuality", labelQualityHint.Text);
        linkTessdataFolder.Text = Path.Combine(AppContext.BaseDirectory, "tessdata"); // derselbe Ordner, den OcrLanguages.Installed liest
        var languageIndex = Array.IndexOf(LanguageCodes, languageCode);
        comboUiLanguage.SelectedIndex = languageIndex >= 0 ? languageIndex : 0;
        var match = BackColorRadios().FirstOrDefault(r => r.BackColor.ToArgb() == overviewBackColor.ToArgb()) ?? rbBackWhite;
        match.Checked = true;
        cbCloseOnEscape.Checked = closeOnEscape;
        rbExitKeep.Checked = exitAction == 0;
        rbExitAsk.Checked = exitAction == 1;
        rbExitClear.Checked = exitAction == 2;
        textSaveDirectory.Text = saveDirectory ?? string.Empty;
        foreach (var code in OcrLanguages.Installed())
        {
            comboLanguage.Items.Add(new OcrLanguageItem(code));
        }
        var current = comboLanguage.Items.Cast<OcrLanguageItem>().FirstOrDefault(i => string.Equals(i.Code, ocrLanguage, StringComparison.OrdinalIgnoreCase));
        if (current != null) { comboLanguage.SelectedItem = current; }
        else if (comboLanguage.Items.Count > 0) { comboLanguage.SelectedIndex = 0; }
        trackQuality.Value = Math.Clamp((int)Math.Round(ocrJpgQuality / 5.0), trackQuality.Minimum, trackQuality.Maximum);
        TrackQuality_ValueChanged(this, EventArgs.Empty); // Wertanzeige auch beim Designer-Startwert
    }

    /// <summary>Markiert das gewählte Farbfeld mit einem dickeren Rahmen.</summary>
    private void BackColorRadio_CheckedChanged(object sender, EventArgs e)
    {
        var radio = (RadioButton)sender;
        radio.FlatAppearance.BorderSize = radio.Checked ? 3 : 1;
        radio.FlatAppearance.BorderColor = radio.Checked ? SystemColors.Highlight : SystemColors.ControlDark;
    }

    private void TrackQuality_ValueChanged(object sender, EventArgs e)
    {
        labelQualityValue.Text = OcrJpgQuality.ToString();
    }

    private void LinkTessdataRepo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        TaskDlg.StartLink(Handle, linkTessdataRepo.Text);
    }

    /// <summary>Öffnet den tessdata-Ordner im Explorer — dorthin gehören die Sprachdateien.</summary>
    private void LinkTessdataFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(linkTessdataFolder.Text) { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            TaskDlg.ErrTaskDlg(Handle, Lng.T("Der Ordner konnte nicht geöffnet werden."), ex);
        }
    }

    private void BtnBrowse_Click(object sender, EventArgs e)
    {
        using FolderBrowserDialog browser = new() { Description = Lng.T("Bevorzugter Speicherort für PDF-Dateien") };
        if (Directory.Exists(textSaveDirectory.Text)) { browser.SelectedPath = textSaveDirectory.Text; }
        if (browser.ShowDialog(this) == DialogResult.OK) { textSaveDirectory.Text = browser.SelectedPath; }
    }
}
