using ScanView.Classes;

namespace ScanView.Forms;

/// <summary>Einstellungsdialog (Extras → Optionen): Allgemein und Texterkennung.
/// Bewusst ohne Designer-Datei — der Dialog ist klein und komplett in Code aufgebaut.</summary>
internal sealed class SettingsForm : Form
{
    private readonly CheckBox cbCloseOnEscape;
    private readonly RadioButton rbExitKeep;
    private readonly RadioButton rbExitAsk;
    private readonly RadioButton rbExitClear;
    private readonly ComboBox comboLanguage;
    private readonly NumericUpDown numJpgQuality;
    private readonly TextBox textSaveDirectory;

    public bool CloseOnEscape => cbCloseOnEscape.Checked;

    public int ExitAction => rbExitAsk.Checked ? 1 : rbExitClear.Checked ? 2 : 0;

    public string OcrLanguage => comboLanguage.SelectedItem is OcrLanguageItem item ? item.Code : "deu";

    public int OcrJpgQuality => (int)numJpgQuality.Value;

    public string SaveDirectory => textSaveDirectory.Text.Trim();

    public SettingsForm(bool closeOnEscape, int exitAction, string saveDirectory, string ocrLanguage, int ocrJpgQuality)
    {
        Text = "Einstellungen";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowIcon = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(440, 318);

        TabControl tabs = new() { Bounds = new Rectangle(12, 12, 416, 254) };
        TabPage tabGeneral = new("Allgemein");
        TabPage tabOcr = new("Texterkennung");
        tabs.TabPages.Add(tabGeneral);
        tabs.TabPages.Add(tabOcr);

        cbCloseOnEscape = new CheckBox()
        {
            AutoSize = true,
            Location = new Point(16, 20),
            Text = "Programm mit 2× &Esc beenden (Umschalt+Esc: sofort)",
            Checked = closeOnEscape,
        };
        Label labelExit = new() { AutoSize = true, Location = new Point(16, 54), Text = "Beim Beenden des Programms:" };
        rbExitKeep = new RadioButton() { AutoSize = true, Location = new Point(28, 74), Text = "Seiten in der Seitenübersicht &behalten", Checked = exitAction == 0 };
        rbExitAsk = new RadioButton() { AutoSize = true, Location = new Point(28, 98), Text = "Seitenübersicht nach &Rückfrage leeren", Checked = exitAction == 1 };
        rbExitClear = new RadioButton() { AutoSize = true, Location = new Point(28, 122), Text = "Seitenübersicht &ohne Rückfrage leeren", Checked = exitAction == 2 };
        Label labelDirectory = new() { AutoSize = true, Location = new Point(16, 156), Text = "Bevorzugter &Speicherort für PDF-Dateien:" };
        textSaveDirectory = new TextBox() { Location = new Point(16, 176), Width = 330, Text = saveDirectory ?? string.Empty };
        Button btnBrowse = new() { Location = new Point(352, 175), Size = new Size(32, 25), Text = "…" };
        btnBrowse.Click += (s, e) =>
        {
            using FolderBrowserDialog browser = new() { Description = "Bevorzugter Speicherort für PDF-Dateien" };
            if (Directory.Exists(textSaveDirectory.Text)) { browser.SelectedPath = textSaveDirectory.Text; }
            if (browser.ShowDialog(this) == DialogResult.OK) { textSaveDirectory.Text = browser.SelectedPath; }
        };
        Label labelDirectoryHint = new()
        {
            AutoSize = true,
            ForeColor = SystemColors.GrayText,
            Location = new Point(16, 204),
            Text = "Leer: Windows schlägt den zuletzt verwendeten Ordner vor.",
        };
        tabGeneral.Controls.AddRange([cbCloseOnEscape, labelExit, rbExitKeep, rbExitAsk, rbExitClear, labelDirectory, textSaveDirectory, btnBrowse, labelDirectoryHint]);

        Label labelLanguage = new() { AutoSize = true, Location = new Point(16, 20), Text = "Bevorzugte &Sprache der Texterkennung (Vorgabe für neue Sitzungen):" };
        comboLanguage = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(16, 40), Width = 220 };
        foreach (var code in OcrLanguages.Installed())
        {
            comboLanguage.Items.Add(new OcrLanguageItem(code));
        }
        var current = comboLanguage.Items.Cast<OcrLanguageItem>().FirstOrDefault(i => string.Equals(i.Code, ocrLanguage, StringComparison.OrdinalIgnoreCase));
        if (current != null) { comboLanguage.SelectedItem = current; }
        else if (comboLanguage.Items.Count > 0) { comboLanguage.SelectedIndex = 0; }
        Label labelLanguageHint = new()
        {
            AutoSize = true,
            ForeColor = SystemColors.GrayText,
            Location = new Point(16, 72),
            Text = "Weitere Sprachen: .traineddata-Dateien (tessdata_best)\nin den Ordner \"tessdata\" neben der Programmdatei legen.",
        };
        Label labelQuality = new() { AutoSize = true, Location = new Point(16, 122), Text = "&JPEG-Qualität der Bilder in der PDF (30–100):" };
        numJpgQuality = new NumericUpDown()
        {
            Location = new Point(16, 142),
            Width = 60,
            Minimum = 30,
            Maximum = 100,
            Value = Math.Clamp(ocrJpgQuality, 30, 100),
        };
        Label labelQualityHint = new()
        {
            AutoSize = true,
            ForeColor = SystemColors.GrayText,
            Location = new Point(16, 174),
            Text = "Kleinere Werte ergeben kleinere Dateien; 75 ist ein guter\nKompromiss. Graustufen-Scans sparen zusätzlich Platz.",
        };
        tabOcr.Controls.AddRange([labelLanguage, comboLanguage, labelLanguageHint, labelQuality, numJpgQuality, labelQualityHint]);

        Button btnOk = new() { Text = "OK", DialogResult = DialogResult.OK, Bounds = new Rectangle(262, 280, 80, 26) };
        Button btnCancel = new() { Text = "Abbrechen", DialogResult = DialogResult.Cancel, Bounds = new Rectangle(348, 280, 80, 26) };
        AcceptButton = btnOk;
        CancelButton = btnCancel;

        Controls.AddRange([tabs, btnOk, btnCancel]);
    }
}
