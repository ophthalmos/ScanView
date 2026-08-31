using System.Text.Json;

namespace ScanTest.Classes;

/// <summary>Programmeinstellungen, gespeichert als JSON in %APPDATA%\ScanTest\settings.json.</summary>
internal sealed class AppSettings
{
    public int WindowX { get; set; }
    public int WindowY { get; set; }
    public int WindowWidth { get; set; }
    public int WindowHeight { get; set; }
    public bool WindowMaximized { get; set; }
    public bool CloseOnEscape { get; set; } = true;
    public string OcrLanguage { get; set; } = "deu";

    private static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ScanTest", "settings.json");

    public static AppSettings Load()
    {
        try { return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ?? new AppSettings(); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        { return new AppSettings(); } // erster Start oder defekte Datei
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
    }
}
