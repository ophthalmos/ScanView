namespace ScanView.Classes;

/// <summary>Ein Eintrag der Sprachauswahl — zeigt den Klarnamen, trägt den Tesseract-Code.</summary>
internal sealed record OcrLanguageItem(string Code)
{
    public override string ToString() => $"{OcrLanguages.DisplayName(Code)} ({Code})";
}

/// <summary>Sprachliste für die Texterkennung — geteilt zwischen Scan-Einstellungen und Optionen.</summary>
internal static class OcrLanguages
{
    /// <summary>Anzeigenamen der gängigen Tesseract-Sprachcodes; unbekannte Codes erscheinen roh.</summary>
    private static readonly Dictionary<string, string> Names = new(StringComparer.OrdinalIgnoreCase)
    {
        ["deu"] = "Deutsch",
        ["eng"] = "Englisch",
        ["deu+eng"] = "Deutsch + Englisch",
        ["fra"] = "Französisch",
        ["ita"] = "Italienisch",
        ["spa"] = "Spanisch",
        ["nld"] = "Niederländisch",
    };

    public static string DisplayName(string code) => Names.TryGetValue(code, out var name) ? name : code;

    /// <summary>Sprachcodes aller .traineddata-Dateien im tessdata-Ordner (mindestens "deu");
    /// sind Deutsch und Englisch vorhanden, kommt die Kombination "deu+eng" dazu.</summary>
    public static List<string> Installed()
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
