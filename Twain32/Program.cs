using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ScanView.Twain32;

/// <summary>Kommandozeile des TWAIN-Helfers (aufgerufen von ScanView, nicht für Benutzer gedacht):
///   Twain32.exe list
///       eine Zeile je TWAIN-Quelle auf stdout: Produktname TAB Hersteller (UTF-8)
///   Twain32.exe scan --source NAME --out DATEI [--dpi 300] [--mode color|gray|bw]
///                    [--area BREITE,HÖHE (mm)] [--brightness -100..100] [--feeder]
///       scannt eine Seite ohne Treiberoberfläche (falls der Treiber das ablehnt: mit) und speichert sie als TIFF.
/// Rückgabewert: 0 = erfolgreich, 1 = abgebrochen, 2 = Fehler (Meldung auf stderr),
/// 3 = Quelle nicht gefunden, 4 = TWAIN-Manager nicht verfügbar, 5 = falsche Aufrufparameter.</summary>
internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        var stdout = new StreamWriter(System.Console.OpenStandardOutput(), new UTF8Encoding(false)) { AutoFlush = true };
        var stderr = new StreamWriter(System.Console.OpenStandardError(), new UTF8Encoding(false)) { AutoFlush = true };
        try
        {
            var request = TwainRequest.Parse(args);
            if (request == null)
            {
                stderr.WriteLine("Aufruf: " + Path.GetFileName(Application.ExecutablePath) + " list | scan --source NAME --out DATEI [--dpi N] [--mode color|gray|bw] [--area B,H] [--brightness N] [--feeder]");
                return TwainExitCode.BadArguments;
            }
            using TwainSession session = new(request);
            Application.Run(session);
            foreach (var line in session.Output) { stdout.WriteLine(line); }
            if (session.Error != null) { stderr.WriteLine(session.Error); }
            return session.ExitCode;
        }
        catch (Exception ex)
        {
            stderr.WriteLine(ex.Message);
            return TwainExitCode.Error;
        }
    }
}

internal static class TwainExitCode
{
    public const int Success = 0, Cancelled = 1, Error = 2, SourceNotFound = 3, NoTwain = 4, BadArguments = 5;
}

internal enum TwainMode { Color, Gray, BlackWhite }

/// <summary>Die Aufrufparameter: Auflisten oder Scannen mit den Einstellungen aus ScanViews Scan-Panel.</summary>
internal sealed class TwainRequest
{
    public bool List { get; private set; }
    public string Source { get; private set; } = "";
    public string OutputPath { get; private set; } = "";
    public int Dpi { get; private set; } = 300;
    public TwainMode Mode { get; private set; } = TwainMode.Color;
    public double? AreaWidthMm { get; private set; }
    public double? AreaHeightMm { get; private set; }
    public int Brightness { get; private set; }
    public bool Feeder { get; private set; }

    public static TwainRequest? Parse(string[] args)
    {
        if (args.Length == 0) { return null; }
        TwainRequest request = new();
        if (string.Equals(args[0], "list", StringComparison.OrdinalIgnoreCase)) { request.List = true; return request; }
        if (!string.Equals(args[0], "scan", StringComparison.OrdinalIgnoreCase)) { return null; }
        for (var i = 1; i < args.Length; i++)
        {
            string Next() => i + 1 < args.Length ? args[++i] : throw new ArgumentException("Wert fehlt für " + args[i]);
            switch (args[i].ToLowerInvariant())
            {
                case "--source": request.Source = Next(); break;
                case "--out": request.OutputPath = Next(); break;
                case "--dpi": request.Dpi = int.Parse(Next(), CultureInfo.InvariantCulture); break;
                case "--mode":
                    request.Mode = Next().ToLowerInvariant() switch { "gray" => TwainMode.Gray, "bw" => TwainMode.BlackWhite, _ => TwainMode.Color };
                    break;
                case "--area":
                    var parts = Next().Split(',');
                    request.AreaWidthMm = double.Parse(parts[0], CultureInfo.InvariantCulture);
                    request.AreaHeightMm = double.Parse(parts[1], CultureInfo.InvariantCulture);
                    break;
                case "--brightness": request.Brightness = Math.Max(-100, Math.Min(100, int.Parse(Next(), CultureInfo.InvariantCulture))); break;
                case "--feeder": request.Feeder = true; break;
                default: return null;
            }
        }
        return request.Source.Length > 0 && request.OutputPath.Length > 0 ? request : null;
    }
}
