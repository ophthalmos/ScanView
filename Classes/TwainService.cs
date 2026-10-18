using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace ScanView.Classes;

/// <summary>TWAIN-Scanner über die Helfer Twain32.exe und Twain64.exe (ein 64-Bit-Programm kann keine 32-Bit-Treiber
/// laden – und die meisten TWAIN-Treiber gibt es nur in 32 Bit). TWAIN-Geräte tragen die ID „twain:" bzw. „twain64:"
/// + Produktname; ScanService leitet sie hierher weiter. Die Quellenliste wird je Sitzung einmal abgefragt (ein
/// Helferstart dauert spürbar). Der 64-Bit-Helfer läuft nur, wenn der 64-Bit-Manager TWAINDSM.dll installiert ist
/// (Windows bringt nur den alten 32-Bit-Manager twain_32.dll mit; ScanViews Setup installiert TWAINDSM.dll in
/// 32 und 64 Bit, ebenso viele Treiber). Mit TWAINDSM.dll melden die Helfer auch den Fortschritt.</summary>
internal static class TwainService
{
    public const string IdPrefix = "twain:", IdPrefix64 = "twain64:";
    private const int ExitSuccess = 0, ExitCancelled = 1, ExitSourceNotFound = 3, ExitNoTwain = 4, ExitMemoryTransferFailed = 6;
    private const string ProgressPrefix = "progress ";
    private static List<(string Name, string Manufacturer, bool Is64)>? cachedSources;
    private static readonly HashSet<string> nativeOnlySources = []; // Quellen ohne Übertragung in Streifen (je Sitzung)

    private static string HelperName(bool is64) => is64 ? "Twain64.exe" : "Twain32.exe";

    private static string HelperPath(bool is64) => Path.Combine(AppContext.BaseDirectory, HelperName(is64));

    // ScanView ist ein 64-Bit-Prozess: SystemDirectory ist das echte System32
    private static bool Has64BitManager => File.Exists(Path.Combine(Environment.SystemDirectory, "TWAINDSM.dll"));

    /// <summary>True, wenn der letzte Aufruf von ListSources den Helfer erfolgreich ausgeführt hat (Selbsttest).</summary>
    public static bool LastListSucceeded { get; private set; }

    public static bool IsTwainId(string? id) => id != null
        && (id.StartsWith(IdPrefix, StringComparison.Ordinal) || id.StartsWith(IdPrefix64, StringComparison.Ordinal));

    /// <summary>Die TWAIN-Quellen (ohne die WIA-Brücken von Windows), deren Gerät angeschlossen ist – wie bei WIA.
    /// Der TWAIN-Manager listet auch Treiber ausgeschalteter Geräte; ImagingDevices gleicht sie bei jedem Aufruf mit
    /// der Geräteverwaltung ab, Quellen ohne zuordenbares Gerät bleiben sichtbar. Leer, wenn die Helfer fehlen oder scheitern.</summary>
    public static List<ScannerInfo> ListSources(bool refresh = false)
    {
        var sources = QuerySources(refresh);
        var devices = ImagingDevices.Enumerate();
        return [.. sources.Where(s => ImagingDevices.IsPresent(devices, s.Name, s.Manufacturer) != false)
                          .Select(s => new ScannerInfo((s.Is64 ? IdPrefix64 : IdPrefix) + s.Name, s.Name + " (TWAIN)"))];
    }

    /// <summary>Produktname und Hersteller aller TWAIN-Quellen, je Sitzung einmal von den Helfern abgefragt (beide
    /// gleichzeitig). Bietet ein Treiber beide Bitbreiten an, gilt die 32-Bit-Quelle – sie ist der erprobte Weg.</summary>
    private static List<(string Name, string Manufacturer, bool Is64)> QuerySources(bool refresh)
    {
        if (cachedSources != null && !refresh) { return cachedSources; }
        var query64 = Has64BitManager ? Task.Run(() => QueryHelper(is64: true)) : null;
        var (ok32, sources32) = QueryHelper(is64: false);
        var (ok64, sources64) = query64?.Result ?? (true, []);
        LastListSucceeded = ok32 && ok64;
        List<(string Name, string Manufacturer, bool Is64)> sources = [.. sources32.Select(s => (s.Name, s.Manufacturer, false))];
        foreach (var (name, manufacturer) in sources64)
        {
            if (!sources.Exists(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase))) { sources.Add((name, manufacturer, true)); }
        }
        cachedSources = sources;
        return sources;
    }

    /// <summary>Quellenliste eines Helfers. Ok ist false, wenn er fehlt oder scheitert; ein fehlender Manager
    /// (Rückgabe 4) gilt beim 64-Bit-Helfer nicht als Fehler – dann gibt es eben keine 64-Bit-Quellen.</summary>
    private static (bool Ok, List<(string Name, string Manufacturer)> Sources) QueryHelper(bool is64)
    {
        List<(string Name, string Manufacturer)> sources = [];
        if (!File.Exists(HelperPath(is64))) { return (false, sources); }
        var (exitCode, output, _) = RunHelper(is64, ["list"], TimeSpan.FromSeconds(20), keepWaiting: null);
        if (exitCode != ExitSuccess) { return (is64 && exitCode == ExitNoTwain, sources); }
        foreach (var line in output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split('\t');
            var name = parts[0].Trim();
            if (name.Length > 0) { sources.Add((name, parts.Length > 1 ? parts[1].Trim() : "")); }
        }
        return (true, sources);
    }

    /// <summary>Scannt eine Seite über den Helfer und speichert sie als TIFF unter path. Null bei Abbruch (error null)
    /// oder Fehler (error beschreibt ihn, übersetzt). progress bekommt den Fortschritt in Prozent – oder −1, solange
    /// er unbekannt ist (vor dem ersten Streifen, Native Transfer, Bildhöhe vorab unbekannt) – und liefert false zum
    /// Abbrechen; dann beendet der Helfer den Treiber geordnet.</summary>
    public static string? Scan(string deviceId, string path, int dpi, int colorIntent, SizeF? areaMm, int brightnessPercent, bool useFeeder, Func<int, bool>? progress, out string? error)
    {
        error = null;
        var is64 = deviceId.StartsWith(IdPrefix64, StringComparison.Ordinal);
        if (!File.Exists(HelperPath(is64)))
        {
            error = string.Format(Lng.T("Das Hilfsprogramm {0} fehlt im Programmordner – bitte ScanView neu installieren."), HelperName(is64));
            return null;
        }
        var sourceName = deviceId[(is64 ? IdPrefix64 : IdPrefix).Length..];
        var manufacturer = cachedSources?.FirstOrDefault(s => s.Name == sourceName && s.Is64 == is64).Manufacturer ?? "";
        if (ImagingDevices.IsPresent(ImagingDevices.Enumerate(), sourceName, manufacturer) == false)
        { // Gerät aus: gar nicht erst den Treiber starten – manche zeigen dann einen eigenen Fehlerdialog
            error = Lng.T("Der gewählte Scanner wurde nicht gefunden. Ist er angeschlossen und eingeschaltet?");
            return null;
        }
        List<string> arguments =
        [
            "scan", "--source", sourceName, "--out", path,
            "--dpi", dpi.ToString(CultureInfo.InvariantCulture),
            "--mode", colorIntent switch { 2 => "gray", 4 => "bw", _ => "color" }, // WIA-Intent: 1 Farbe, 2 Grau, 4 Schwarz-weiß
            "--brightness", brightnessPercent.ToString(CultureInfo.InvariantCulture),
        ];
        if (areaMm is { } area) { arguments.AddRange(["--area", string.Format(CultureInfo.InvariantCulture, "{0},{1}", area.Width, area.Height)]); }
        if (useFeeder) { arguments.Add("--feeder"); }
        if (nativeOnlySources.Contains(deviceId)) { arguments.Add("--native"); }
        var (exitCode, _, errorText) = RunHelper(is64, arguments, Timeout.InfiniteTimeSpan, keepWaiting: progress);
        if (exitCode == ExitMemoryTransferFailed)
        { // Treiber kann die Übertragung in Streifen nicht: einmal in einem Stück wiederholen und sich das merken
            Debug.WriteLine("TWAIN-Speicherübertragung: " + errorText.Trim());
            nativeOnlySources.Add(deviceId);
            arguments.Add("--native");
            (exitCode, _, errorText) = RunHelper(is64, arguments, Timeout.InfiniteTimeSpan, keepWaiting: progress);
        }
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
    /// mit dem zuletzt gemeldeten Fortschritt aufgerufen (Zeilen „progress N" auf stdout, sonst −1) und hält die
    /// Oberfläche bedienbar; liefert es false oder läuft timeout ab, bekommt der Helfer „cancel" und beendet den
    /// Treiber geordnet – ein hart beendeter Treiber hält den Scanner sonst fest, bis er aus- und wieder
    /// eingeschaltet wird. Nur wenn der Helfer danach eine Minute lang nicht reagiert, wird er beendet; dann ist
    /// der Rückgabecode null. Output enthält alle übrigen Zeilen.</summary>
    private static (int? ExitCode, string Output, string Error) RunHelper(bool is64, IEnumerable<string> arguments, TimeSpan timeout, Func<int, bool>? keepWaiting)
    {
        ProcessStartInfo info = new(HelperPath(is64))
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
            var percent = -1;
            var outputTask = Task.Run(async () =>
            {
                StringBuilder output = new();
                while (await process.StandardOutput.ReadLineAsync() is { } line)
                {
                    if (line.StartsWith(ProgressPrefix, StringComparison.Ordinal)
                        && int.TryParse(line.AsSpan(ProgressPrefix.Length), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
                    {
                        Volatile.Write(ref percent, value);
                    }
                    else { output.AppendLine(line); }
                }
                return output.ToString();
            });
            var errorTask = process.StandardError.ReadToEndAsync();
            var watch = Stopwatch.StartNew();
            TimeSpan? cancelSentAt = null;
            while (!process.WaitForExit(100))
            {
                var keep = keepWaiting?.Invoke(Volatile.Read(ref percent)) ?? true; // auch nach dem Abbruchwunsch weiter aufrufen: die Oberfläche bleibt lebendig
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
