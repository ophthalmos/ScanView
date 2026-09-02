using ScanView.Classes;
using ScanView.Forms;

namespace ScanView;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        var selfTest = args.Contains("--selftest");
        // Scanner-Taste: Windows startet ScanView mit /StiDevice:<WIA-DeviceID> /StiEvent:<GUID>
        var stiDevice = args.FirstOrDefault(a => a.StartsWith("/StiDevice:", StringComparison.OrdinalIgnoreCase))?["/StiDevice:".Length..];
        // Einmal-Instanz per Mutex (nur der Selbsttest läuft immer, auch neben einer offenen Instanz)
        using Mutex singleMutex = new(true, "{6D3A9C0E-52B7-4F81-9A45-C1E87D204B6A}", out var isNewInstance);
        if (isNewInstance || selfTest)
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm(selfTest, stiDevice));
        }
        else
        {
            // Zweiter Start: bestehende Instanz in den Vordergrund holen — kam er von der
            // Scanner-Taste (/StiDevice), soll sie zusätzlich sofort scannen
            NativeMethods.PostMessage(NativeMethods.HWND_BROADCAST,
                stiDevice != null ? NativeMethods.WM_SCANSCANVIEW : NativeMethods.WM_SHOWSCANVIEW, 0, 0);
        }
    }
}
