using System.Drawing.Imaging;

namespace ScanTest.Classes;

internal sealed record ScannerInfo(string Id, string Name);

/// <summary>Bildbeschaffung: WIA-Scanner (aufzählen und gezielt ansteuern, spätbindend ohne
/// COM-Verweis) oder eine gerenderte Testseite zum Ausprobieren ohne Gerät.</summary>
internal static class ScanService
{
    private const string WiaFormatTiff = "{B96B3CB1-0728-11D3-9D7B-0000F81EF32E}";

    // WIA-Property-IDs des Scan-Items
    private const int WiaCurrentIntent = 6146;        // 1 = Farbe, 2 = Graustufen, 4 = Schwarz-weiß
    private const int WiaHorizontalResolution = 6147;
    private const int WiaVerticalResolution = 6148;

    /// <summary>Alle angeschlossenen Scanner (WIA-Gerätetyp 1).</summary>
    public static List<ScannerInfo> ListScanners()
    {
        List<ScannerInfo> result = [];
        try
        {
            var managerType = Type.GetTypeFromProgID("WIA.DeviceManager");
            if (managerType == null) { return result; }
            dynamic manager = Activator.CreateInstance(managerType);
            foreach (dynamic info in manager.DeviceInfos)
            {
                if ((int)info.Type != 1) { continue; } // nur Scanner
                string name;
                try { name = (string)info.Properties["Name"].Value; }
                catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
                { name = (string)info.DeviceID; }
                result.Add(new ScannerInfo((string)info.DeviceID, name));
            }
        }
        catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or InvalidOperationException)
        {
            System.Diagnostics.Debug.WriteLine("WIA-Aufzählung: " + ex.Message);
        }
        return result;
    }

    /// <summary>Scannt eine Seite direkt vom angegebenen Gerät (ohne Gerätewahl-Dialog) und
    /// speichert sie als TIFF. Auflösung und Farbmodus werden gesetzt, soweit das Gerät sie annimmt.
    /// Null bei Fehlern.</summary>
    public static string ScanFromDevice(string deviceId, string path, int dpi, int colorIntent)
    {
        try
        {
            var managerType = Type.GetTypeFromProgID("WIA.DeviceManager");
            if (managerType == null) { return null; }
            dynamic manager = Activator.CreateInstance(managerType);
            dynamic device = null;
            foreach (dynamic info in manager.DeviceInfos)
            {
                if ((string)info.DeviceID == deviceId) { device = info.Connect(); break; }
            }
            if (device == null) { return null; }
            dynamic item = device.Items[1];
            TrySetProperty(item, WiaCurrentIntent, colorIntent);
            TrySetProperty(item, WiaHorizontalResolution, dpi);
            TrySetProperty(item, WiaVerticalResolution, dpi);
            dynamic image = item.Transfer(WiaFormatTiff); // Format ist ein Wunsch — das Gerät darf abweichen
            return SaveAsTiff(image, path);
        }
        catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or InvalidOperationException)
        {
            System.Diagnostics.Debug.WriteLine("WIA-Scan: " + ex.Message);
            return null;
        }
    }

    /// <summary>Fallback ohne Gerätewahl im Menü: der komplette Windows-Scandialog.</summary>
    public static string WiaScanToTiff(string path)
    {
        try
        {
            var dialogType = Type.GetTypeFromProgID("WIA.CommonDialog");
            if (dialogType == null) { return null; }
            dynamic dialog = Activator.CreateInstance(dialogType);
            dynamic image = dialog.ShowAcquireImage();
            return image == null ? null : SaveAsTiff(image, path);
        }
        catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or InvalidOperationException)
        {
            System.Diagnostics.Debug.WriteLine("WIA-Dialog: " + ex.Message);
            return null;
        }
    }

    private static void TrySetProperty(dynamic item, int propertyId, int value)
    {
        try { item.Properties[propertyId.ToString()].Value = value; }
        catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
        { } // Gerät kennt die Eigenschaft nicht — Standard verwenden
    }

    /// <summary>Speichert eine WIA-ImageFile als TIFF; liefert das Gerät ein anderes Format,
    /// wird über GDI+ konvertiert.</summary>
    private static string SaveAsTiff(dynamic image, string path)
    {
        if (File.Exists(path)) { File.Delete(path); }
        var extension = ((string)image.FileExtension).ToLowerInvariant();
        if (extension is "tif" or "tiff")
        {
            image.SaveFile(path);
            return path;
        }
        var temp = Path.ChangeExtension(path, extension);
        if (File.Exists(temp)) { File.Delete(temp); }
        image.SaveFile(temp);
        using (var loaded = LoadUnlocked(temp)) { loaded.Save(path, ImageFormat.Tiff); }
        File.Delete(temp);
        return path;
    }

    /// <summary>Gerenderte 300-dpi-A4-Testseite — zum Ausprobieren ohne Scanner.</summary>
    public static string RenderTestPage(string path, string title, params string[] lines)
    {
        using Bitmap bmp = new(2480, 3508);
        bmp.SetResolution(300, 300);
        using (var g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.White);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            using Font titleFont = new("Segoe UI", 26, FontStyle.Bold, GraphicsUnit.Point);
            using Font textFont = new("Segoe UI", 14, GraphicsUnit.Point);
            var y = 300f;
            g.DrawString(title, titleFont, Brushes.Black, 300, y);
            y += 220;
            foreach (var line in lines)
            {
                g.DrawString(line, textFont, Brushes.Black, 300, y);
                y += 120;
            }
        }
        bmp.Save(path, ImageFormat.Tiff);
        return path;
    }

    /// <summary>Lädt ein Bild ohne Dateisperre (Kopie im Speicher) — für die Miniaturansichten.</summary>
    public static Image LoadUnlocked(string path)
    {
        using var stream = new MemoryStream(File.ReadAllBytes(path));
        using var original = Image.FromStream(stream);
        return new Bitmap(original); // vom Stream gelöste Kopie
    }
}
