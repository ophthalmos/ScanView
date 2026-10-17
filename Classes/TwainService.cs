using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace ScanView.Classes;

/// <summary>TWAIN-Scanner über den 32-Bit-Helfer Twain32.exe (die meisten TWAIN-Treiber gibt es nur in 32 Bit,
/// ScanView ist ein 64-Bit-Programm). TWAIN-Geräte tragen die ID „twain:" + Produktname; ScanService leitet
/// sie hierher weiter. Die Quellenliste wird je Sitzung einmal abgefragt (ein Helferstart dauert spürbar).</summary>
internal static class TwainService
{
    public const string IdPrefix = "twain:";
    private const int ExitSuccess = 0, ExitCancelled = 1, ExitSourceNotFound = 3, ExitNoTwain = 4;
    private static List<ScannerInfo>? cachedSources;

    private static string HelperPath => Path.Combine(AppContext.BaseDirectory, "Twain32.exe");

    /// <summary>True, wenn der letzte Aufruf von ListSources den Helfer erfolgreich ausgeführt hat (Selbsttest).</summary>
    public static bool LastListSucceeded { get; private set; }

    public static bool IsTwainId(string? id) => id != null && id.StartsWith(IdPrefix, StringComparison.Ordinal);

    /// <summary>Die TWAIN-Quellen (ohne die WIA-Brücken von Windows); leer, wenn der Helfer fehlt oder scheitert.</summary>
    public static List<ScannerInfo> ListSources(bool refresh = false)
    {
        if (cachedSources != null && !refresh) { return cachedSources; }
        List<ScannerInfo> sources = [];
        LastListSucceeded = false;
        if (File.Exists(HelperPath))
        {
            var (exitCode, output, _) = RunHelper(["list"], TimeSpan.FromSeconds(20), keepWaiting: null);
            if (exitCode == ExitSuccess)
            {
                LastListSucceeded = true;
                foreach (var line in output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
                {
                    var name = line.Split('\t')[0].Trim();
                    if (name.Length > 0) { sources.Add(new ScannerInfo(IdPrefix + name, name + " (TWAIN)")); }
                }
            }
        }
        cachedSources = sources;
        return sources;
    }

    /// <summary>Scannt eine Seite über den Helfer und speichert sie als TIFF unter path. Null bei Abbruch (error null)
    /// oder Fehler (error beschreibt ihn, übersetzt). progress wird mit −1 (Fortschritt unbekannt) aufgerufen und
    /// liefert false zum Abbrechen – dann wird der Helfer beendet.</summary>
    public static string? Scan(string deviceId, string path, int dpi, int colorIntent, SizeF? areaMm, int brightnessPercent, bool useFeeder, Func<int, bool>? progress, out string? error)
    {
        error = null;
        if (!File.Exists(HelperPath))
        {
            error = Lng.T("Das Hilfsprogramm Twain32.exe fehlt im Programmordner – bitte ScanView neu installieren.");
            return null;
        }
        List<string> arguments =
        [
            "scan", "--source", deviceId[IdPrefix.Length..], "--out", path,
            "--dpi", dpi.ToString(CultureInfo.InvariantCulture),
            "--mode", colorIntent switch { 2 => "gray", 4 => "bw", _ => "color" }, // WIA-Intent: 1 Farbe, 2 Grau, 4 Schwarz-weiß
            "--brightness", brightnessPercent.ToString(CultureInfo.InvariantCulture),
        ];
        if (areaMm is { } area) { arguments.AddRange(["--area", string.Format(CultureInfo.InvariantCulture, "{0},{1}", area.Width, area.Height)]); }
        if (useFeeder) { arguments.Add("--feeder"); }
        var (exitCode, _, errorText) = RunHelper(arguments, Timeout.InfiniteTimeSpan, keepWaiting: progress == null ? null : () => progress(-1));
        switch (exitCode)
        {
            case ExitSuccess when File.Exists(path):
                return path;
            case ExitCancelled or null: // null = vom Benutzer abgebrochen (Helfer beendet)
                return null;
            case ExitSourceNotFound:
                cachedSources = null; // beim nächsten Öffnen der Geräteliste neu abfragen
                error = Lng.T("Der gewählte Scanner wurde nicht gefunden. Ist er angeschlossen und eingeschaltet?");
                return null;
            case ExitNoTwain:
                error = Lng.T("Der TWAIN-Manager von Windows konnte nicht geladen werden.");
                return null;
            default:
                error = string.Format(Lng.T("Der TWAIN-Treiber meldet einen Fehler: {0}"), errorText.Trim().Length > 0 ? errorText.Trim() : exitCode.ToString());
                return null;
        }
    }

    /// <summary>Startet den Helfer ohne Fenster und wartet auf sein Ende. keepWaiting wird etwa zehnmal je Sekunde
    /// aufgerufen (Oberfläche bedienbar halten); liefert es false oder läuft timeout ab, bekommt der Helfer
    /// „cancel" und beendet den Treiber geordnet – ein hart beendeter Treiber hält den Scanner sonst fest, bis
    /// er aus- und wieder eingeschaltet wird. Nur wenn der Helfer danach eine Minute lang nicht reagiert, wird
    /// er beendet; dann ist der Rückgabecode null.</summary>
    private static (int? ExitCode, string Output, string Error) RunHelper(IEnumerable<string> arguments, TimeSpan timeout, Func<bool>? keepWaiting)
    {
        ProcessStartInfo info = new(HelperPath)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            WorkingDirectory = AppContext.BaseDirectory,
        };
        foreach (var argument in arguments) { info.ArgumentList.Add(argument); }
        try
        {
            using var process = Process.Start(info)!;
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();
            var watch = Stopwatch.StartNew();
            TimeSpan? cancelSentAt = null;
            while (!process.WaitForExit(100))
            {
                var keep = keepWaiting?.Invoke() ?? true; // auch nach dem Abbruchwunsch weiter aufrufen: die Oberfläche bleibt lebendig
                if (cancelSentAt == null && (!keep || (timeout != Timeout.InfiniteTimeSpan && watch.Elapsed > timeout)))
                {
                    cancelSentAt = watch.Elapsed;
                    try { process.StandardInput.WriteLine("cancel"); process.StandardInput.Flush(); }
                    catch (IOException) { } // Helfer beendet sich gerade
                }
                if (cancelSentAt is { } sent && watch.Elapsed - sent > TimeSpan.FromMinutes(1))
                {
                    try { process.Kill(); } catch (InvalidOperationException) { } // schon beendet
                    process.WaitForExit();
                    return (null, outputTask.Result, errorTask.Result);
                }
            }
            process.WaitForExit(); // auch die umgeleiteten Ausgaben vollständig einlesen
            return (process.ExitCode, outputTask.Result, errorTask.Result);
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            return (-1, string.Empty, ex.Message);
        }
    }
}
