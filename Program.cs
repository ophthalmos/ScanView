using ScanView.Classes;
using ScanView.Forms;

namespace ScanView;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        var selfTest = args.Contains("--selftest");
        // Einmal-Instanz per Mutex (nur der Selbsttest läuft immer, auch neben einer offenen Instanz)
        using Mutex singleMutex = new(true, "{6D3A9C0E-52B7-4F81-9A45-C1E87D204B6A}", out var isNewInstance);
        if (isNewInstance || selfTest)
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm(selfTest));
        }
        else
        {
            // Zweiter Start (z.B. über die Scanner-Taste): die bestehende Instanz in den Vordergrund holen
            NativeMethods.PostMessage(NativeMethods.HWND_BROADCAST, NativeMethods.WM_SHOWSCANVIEW, 0, 0);
        }
    }
}
