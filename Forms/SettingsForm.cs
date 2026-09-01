using ScanView.Classes;

namespace ScanView.Forms;

/// <summary>Einstellungsdialog (Extras → Optionen): Allgemein und Texterkennung.</summary>
internal sealed partial class SettingsForm : Form
{
    public bool CloseOnEscape => cbCloseOnEscape.Checked;

    public int ExitAction => rbExitAsk.Checked ? 1 : rbExitClear.Checked ? 2 : 0;

    public string OcrLanguage => comboLanguage.SelectedItem is OcrLanguageItem item ? item.Code : "deu";

    public int OcrJpgQuality => (int)numJpgQuality.Value;

    public string SaveDirectory => textSaveDirectory.Text.Trim();

    public SettingsForm(bool closeOnEscape, int exitAction, string saveDirectory, string ocrLanguage, int ocrJpgQuality)
    {
        InitializeComponent();
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
        numJpgQuality.Value = Math.Clamp(ocrJpgQuality, 30, 100);
    }

    private void BtnBrowse_Click(object sender, EventArgs e)
    {
        using FolderBrowserDialog browser = new() { Description = "Bevorzugter Speicherort für PDF-Dateien" };
        if (Directory.Exists(textSaveDirectory.Text)) { browser.SelectedPath = textSaveDirectory.Text; }
        if (browser.ShowDialog(this) == DialogResult.OK) { textSaveDirectory.Text = browser.SelectedPath; }
    }
}
