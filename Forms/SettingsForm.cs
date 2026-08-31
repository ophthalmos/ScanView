namespace ScanTest.Forms;

/// <summary>Einstellungsdialog (Extras → Optionen): Allgemein und Texterkennung.
/// Bewusst ohne Designer-Datei — der Dialog ist klein und komplett in Code aufgebaut.</summary>
internal sealed class SettingsForm : Form
{
    private readonly CheckBox cbCloseOnEscape;
    private readonly ComboBox comboLanguage;

    /// <summary>Anzeigenamen der gängigen Tesseract-Sprachcodes; unbekannte Codes erscheinen roh.</summary>
    private static readonly Dictionary<string, string> LanguageNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["deu"] = "Deutsch",
        ["eng"] = "Englisch",
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

    public string OcrLanguage => comboLanguage.SelectedItem is LanguageItem item ? item.Code : "deu";

    public SettingsForm(bool closeOnEscape, string ocrLanguage)
    {
        Text = "Einstellungen";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowIcon = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(430, 250);

        TabControl tabs = new() { Bounds = new Rectangle(12, 12, 406, 186) };
        TabPage tabGeneral = new("Allgemein");
        TabPage tabOcr = new("Texterkennung");
        tabs.TabPages.Add(tabGeneral);
        tabs.TabPages.Add(tabOcr);

        cbCloseOnEscape = new CheckBox()
        {
            AutoSize = true,
            Location = new Point(16, 24),
            Text = "Programm mit 2× &Esc beenden (Umschalt+Esc: sofort)",
            Checked = closeOnEscape,
        };
        tabGeneral.Controls.Add(cbCloseOnEscape);

        Label labelLanguage = new() { AutoSize = true, Location = new Point(16, 24), Text = "&Sprache der Texterkennung:" };
        comboLanguage = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(16, 44), Width = 220 };
        foreach (var code in ListInstalledLanguages())
        {
            comboLanguage.Items.Add(new LanguageItem(code, LanguageNames.TryGetValue(code, out var name) ? name : code));
        }
        var current = comboLanguage.Items.Cast<LanguageItem>().FirstOrDefault(i => string.Equals(i.Code, ocrLanguage, StringComparison.OrdinalIgnoreCase));
        if (current != null) { comboLanguage.SelectedItem = current; }
        else if (comboLanguage.Items.Count > 0) { comboLanguage.SelectedIndex = 0; }
        Label labelHint = new()
        {
            AutoSize = true,
            ForeColor = SystemColors.GrayText,
            Location = new Point(16, 84),
            Text = "Weitere Sprachen: .traineddata-Dateien (tessdata_fast)\nin den Ordner \"tessdata\" neben der Programmdatei legen.",
        };
        tabOcr.Controls.AddRange([labelLanguage, comboLanguage, labelHint]);

        Button btnOk = new() { Text = "OK", DialogResult = DialogResult.OK, Bounds = new Rectangle(252, 212, 80, 26) };
        Button btnCancel = new() { Text = "Abbrechen", DialogResult = DialogResult.Cancel, Bounds = new Rectangle(338, 212, 80, 26) };
        AcceptButton = btnOk;
        CancelButton = btnCancel;

        Controls.AddRange([tabs, btnOk, btnCancel]);
    }

    /// <summary>Sprachcodes aller .traineddata-Dateien im tessdata-Ordner (mindestens "deu").</summary>
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
        return result;
    }
}
