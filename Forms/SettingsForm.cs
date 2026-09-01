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

    /// <summary>Anzeigenamen der gängigen Tesseract-Sprachcodes; unbekannte Codes erscheinen roh.</summary>
    private static readonly Dictionary<string, string> LanguageNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["deu"] = "Deutsch",
        ["eng"] = "Englisch",
        ["deu+eng"] = "Deutsch + Englisch",
        ["fra"] = "Französisch",
        ["ita"] = "Italienisch",
        ["spa"] = "Spanisch",
        ["nld"] = "Niederländisch",
    };

    private sealed record LanguageItem(string Code, string Name)
    {
        public override string ToString() => $"{Name} ({Code})";
    }

    public bool CloseOnEscape => cbCloseOnEscape.Checked;

    public int ExitAction => rbExitAsk.Checked ? 1 : rbExitClear.Checked ? 2 : 0;

    public string OcrLanguage => comboLanguage.SelectedItem is LanguageItem item ? item.Code : "deu";

    public int OcrJpgQuality => (int)numJpgQuality.Value;

    public SettingsForm(bool closeOnEscape, int exitAction, string ocrLanguage, int ocrJpgQuality)
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
        GroupBox groupExit = new() { Bounds = new Rectangle(16, 56, 376, 120), Text = "Beim Beenden des Programms" };
        rbExitKeep = new RadioButton() { AutoSize = true, Location = new Point(16, 26), Text = "Seiten in der Seitenübersicht &behalten", Checked = exitAction == 0 };
        rbExitAsk = new RadioButton() { AutoSize = true, Location = new Point(16, 54), Text = "Seitenübersicht nach &Rückfrage leeren", Checked = exitAction == 1 };
        rbExitClear = new RadioButton() { AutoSize = true, Location = new Point(16, 82), Text = "Seitenübersicht &ohne Rückfrage leeren", Checked = exitAction == 2 };
        groupExit.Controls.AddRange([rbExitKeep, rbExitAsk, rbExitClear]);
        tabGeneral.Controls.AddRange([cbCloseOnEscape, groupExit]);

        Label labelLanguage = new() { AutoSize = true, Location = new Point(16, 20), Text = "&Sprache der Texterkennung:" };
        comboLanguage = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(16, 40), Width = 220 };
        foreach (var code in ListInstalledLanguages())
        {
            comboLanguage.Items.Add(new LanguageItem(code, LanguageNames.TryGetValue(code, out var name) ? name : code));
        }
        var current = comboLanguage.Items.Cast<LanguageItem>().FirstOrDefault(i => string.Equals(i.Code, ocrLanguage, StringComparison.OrdinalIgnoreCase));
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

    /// <summary>Sprachcodes aller .traineddata-Dateien im tessdata-Ordner (mindestens "deu");
    /// sind Deutsch und Englisch vorhanden, kommt die Kombination "deu+eng" dazu.</summary>
    private static List<string> ListInstalledLanguages()
    {
        List<string> result = [];
        try
        {
            var folder = Path.Combine(AppContext.BaseDirectory, "tessdata");
            result.AddRange(Directory.EnumerateFiles(folder, "*.traineddata")
                .Select(Path.GetFileNameWithoutExtension)
                .OrderBy(code => code, StringComparer.OrdinalIgnoreCase));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or DirectoryNotFoundException) { }
        if (result.Count == 0) { result.Add("deu"); }
        if (result.Contains("deu", StringComparer.OrdinalIgnoreCase) && result.Contains("eng", StringComparer.OrdinalIgnoreCase))
        {
            result.Add("deu+eng"); // Tesseract erkennt beide Sprachen gemischt
        }
        return result;
    }
}
