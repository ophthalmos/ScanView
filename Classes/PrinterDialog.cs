using System.Drawing.Printing;

namespace ScanView.Classes;

/// <summary>Öffnet den Treiber-Eigenschaften-Dialog des Druckers (winspool DocumentProperties) —
/// dort stellt der Anwender herstellerspezifische Dinge wie Druckqualität oder Papierart ein.
/// Bei OK landen die Änderungen als DEVMODE zurück in den übergebenen PrinterSettings, sodass
/// der nachfolgende Druck sie übernimmt.</summary>
internal static class PrinterDialog
{
    /// <summary>Zeigt die Druckereigenschaften; true, wenn der Anwender mit OK bestätigt hat.</summary>
    public static bool ShowProperties(IWin32Window owner, PrinterSettings printerSettings)
    {
        const int DM_OUT_BUFFER = 2, DM_IN_PROMPT = 4, DM_IN_BUFFER = 8, IDOK = 1;
        var hDevMode = nint.Zero;
        try
        {
            hDevMode = printerSettings.GetHdevmode();
            var devMode = NativeMethods.GlobalLock(hDevMode);
            var result = NativeMethods.DocumentProperties(owner?.Handle ?? 0, 0, printerSettings.PrinterName,
                devMode, devMode, DM_IN_BUFFER | DM_IN_PROMPT | DM_OUT_BUFFER);
            NativeMethods.GlobalUnlock(hDevMode);
            if (result != IDOK) { return false; }
            printerSettings.SetHdevmode(hDevMode); // kopiert den geänderten DEVMODE in die Einstellungen
            return true;
        }
        catch (InvalidPrinterException) { return false; }
        finally
        {
            if (hDevMode != nint.Zero) { NativeMethods.GlobalFree(hDevMode); }
        }
    }
}
