using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace ScanView.Classes;

/// <summary>Ein Bildverarbeitungsgerät aus der Windows-Geräteverwaltung; Present = gerade angeschlossen und eingeschaltet.</summary>
internal sealed record ImagingDevice(string Name, string Manufacturer, bool Present);

/// <summary>Bildverarbeitungsgeräte (Scanner, Kameras) aus der Windows-Geräteverwaltung – auch die gerade nicht
/// angeschlossenen, die Windows sich merkt. Damit blendet TwainService TWAIN-Quellen aus, deren Gerät aus ist:
/// Der TWAIN-Manager kennt nur die installierten Treiber, nicht die Geräte dahinter (WIA dagegen nur angeschlossene).</summary>
internal static partial class ImagingDevices
{
    private static readonly Guid ImageClass = new("6bdd1fc6-810f-11d0-bec7-08002be2092f"); // GUID_DEVCLASS_IMAGE

    /// <summary>Alle Geräte der Klasse; leer, wenn die Abfrage scheitert (dann bleiben alle TWAIN-Quellen sichtbar).</summary>
    public static List<ImagingDevice> Enumerate()
    {
        List<ImagingDevice> devices = [];
        var set = NativeMethods.SetupDiGetClassDevs(ImageClass, 0, 0, 0); // ohne DIGCF_PRESENT: auch abgezogene Geräte
        if (set == -1) { return devices; }
        try
        {
            var data = new NativeMethods.SpDevinfoData { cbSize = (uint)Marshal.SizeOf<NativeMethods.SpDevinfoData>() };
            for (uint index = 0; NativeMethods.SetupDiEnumDeviceInfo(set, index, ref data); index++)
            {
                var name = Property(set, ref data, NativeMethods.SPDRP_FRIENDLYNAME);
                if (name.Length == 0) { name = Property(set, ref data, NativeMethods.SPDRP_DEVICEDESC); }
                var present = NativeMethods.CM_Get_DevNode_Status(out _, out _, data.DevInst, 0) == NativeMethods.CR_SUCCESS;
                devices.Add(new ImagingDevice(name, Property(set, ref data, NativeMethods.SPDRP_MFG), present));
            }
        }
        finally { NativeMethods.SetupDiDestroyDeviceInfoList(set); }
        return devices;
    }

    /// <summary>True/false, wenn sich die TWAIN-Quelle einem bekannten Gerät zuordnen lässt und es (nicht) angeschlossen
    /// ist; null, wenn keine Zuordnung gelingt (z. B. Netzwerkscanner) – im Zweifel soll die Quelle sichtbar bleiben.
    /// Zuerst über den Produktnamen, erst wenn der nichts findet über den Hersteller.</summary>
    public static bool? IsPresent(List<ImagingDevice> devices, string productName, string manufacturer)
    {
        var matches = devices.Where(d => ProductMatches(d, productName, manufacturer)).ToList();
        if (matches.Count == 0) { matches = [.. devices.Where(d => MakerMatches(d, manufacturer))]; }
        return matches.Count == 0 ? null : matches.Any(d => d.Present);
    }

    /// <summary>Gleiche Namen (ohne Satzzeichen), einer im anderen enthalten oder – beim selben Hersteller – alle
    /// Modellkürzel (Wörter mit Ziffern, etwa „V30") der TWAIN-Quelle im Gerätenamen.</summary>
    private static bool ProductMatches(ImagingDevice device, string productName, string manufacturer)
    {
        var name = Compact(device.Name);
        var product = Compact(productName);
        if (name.Length < 4 || product.Length < 4) { return false; }
        if (name.Contains(product, StringComparison.Ordinal) || product.Contains(name, StringComparison.Ordinal)) { return true; }
        var models = Words(productName).Where(w => w.Any(char.IsDigit)).ToList();
        if (models.Count == 0 || !MakerMatches(device, manufacturer)) { return false; }
        var deviceWords = Words(device.Name).ToHashSet();
        return models.All(deviceWords.Contains);
    }

    /// <summary>Herstellerwort der TWAIN-Quelle im Hersteller des Geräts oder als erstes Wort seines Namens
    /// („Hewlett-Packard" gilt als „HP").</summary>
    private static bool MakerMatches(ImagingDevice device, string manufacturer)
    {
        var maker = Words(manufacturer).FirstOrDefault() ?? "";
        if (maker.Length < 2) { return false; }
        return Words(device.Manufacturer).Contains(maker) || Words(device.Name).FirstOrDefault() == maker;
    }

    private static string Property(nint set, ref NativeMethods.SpDevinfoData data, uint property)
    {
        var buffer = new byte[512];
        return NativeMethods.SetupDiGetDeviceRegistryProperty(set, ref data, property, out _, buffer, (uint)buffer.Length, out _)
            ? Encoding.Unicode.GetString(buffer).Split('\0')[0].Trim() : "";
    }

    private static string Compact(string text) => new([.. text.ToLowerInvariant().Where(char.IsLetterOrDigit)]);

    private static IEnumerable<string> Words(string text) =>
        NonWordChars().Split(text.ToLowerInvariant()).Where(w => w.Length > 0).Select(w => w == "hewlett" ? "hp" : w);

    [GeneratedRegex(@"[^\p{L}\p{N}]+")]
    private static partial Regex NonWordChars();
}
