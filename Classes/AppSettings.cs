using System.Text.Json;

namespace ScanView.Classes;

/// <summary>Programmeinstellungen, gespeichert als JSON in %APPDATA%\ScanView\settings.json.</summary>
internal sealed class AppSettings
{
    public int WindowX { get; set; }
    public int WindowY { get; set; }
    public int WindowWidth { get; set; }
    public int WindowHeight { get; set; }
    public bool WindowMaximized { get; set; }
    public int CropX { get; set; }
    public int CropY { get; set; }
    public int CropWidth { get; set; }
    public int CropHeight { get; set; }
    public bool CloseOnEscape { get; set; } = true;
    public int ExitAction { get; set; }      // 0 = Seiten behalten, 1 = nach Rückfrage leeren, 2 = ohne Rückfrage leeren
    public string SaveDirectory { get; set; } = ""; // bevorzugter Speicherort für PDFs (leer = zuletzt verwendeter)
    public int OverviewBackColor { get; set; } = -1; // ARGB der Seitenübersicht (-1 = Weiß)
    public string OcrLanguage { get; set; } = "deu";
    public int OcrJpgQuality { get; set; } = 75; // JPEG-Qualität der Bilder in der erzeugten PDF
    public List<string> PageFiles { get; set; } = []; // Seiten der letzten Sitzung (bei "behalten")
    public string ScannerId { get; set; }
    public string ScannerName { get; set; }
    public int DpiIndex { get; set; } = 2;   // 300 dpi
    public int ColorIndex { get; set; }      // Farbe
    public int AreaIndex { get; set; }       // maximal
    public int FeedIndex { get; set; }       // Flachbett
    public int Brightness { get; set; }
    public int ThumbWidth { get; set; } = 160;
    public string CopyPrinter { get; set; }            // Kopiermodus: Drucker samt Einstellungen
    public int CopyPaperRawKind { get; set; } = -1;    // -1 = Druckerstandard
    public int CopyPaperSourceRawKind { get; set; } = -1;
    public int CopyDuplexIndex { get; set; }           // 0 = Einseitig
    public int CopyCopies { get; set; } = 1;
    public bool CopyColor { get; set; }
    public bool CopyFit { get; set; } = true;

    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true }; // CA1869: eine Instanz wiederverwenden

    private static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ScanView", "settings.json");

    private static string LegacyFilePath => Path.Combine( // vor der Umbenennung von ScanTest
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ScanTest", "settings.json");

    public static AppSettings Load()
    {
        foreach (var path in new[] { FilePath, LegacyFilePath })
        {
            try { return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path)) ?? new AppSettings(); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException) { } // erster Start oder defekte Datei
        }
        return new AppSettings();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, SerializerOptions));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
    }
}
