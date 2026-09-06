using System.Drawing.Imaging;

namespace ScanView.Classes;

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
    private const int WiaHorizontalStart = 6149;      // Scanfenster in Pixeln bei der wirksamen Auflösung
    private const int WiaVerticalStart = 6150;
    private const int WiaHorizontalExtent = 6151;
    private const int WiaVerticalExtent = 6152;
    private const int WiaBrightness = 6154;
    private const int WiaDocumentHandlingSelect = 3088; // GERÄTE-Eigenschaft: 1 = Einzug, 2 = Flachbett

    /// <summary>Alle angeschlossenen Scanner (WIA-Gerätetyp 1).</summary>
    public static List<ScannerInfo> ListScanners()
    {
        List<ScannerInfo> result = [];
        try
        {
            var managerType = Type.GetTypeFromProgID("WIA.DeviceManager");
            if (managerType == null) { return result; }
            dynamic manager = Activator.CreateInstance(managerType);
            foreach (var info in manager.DeviceInfos)
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
    /// speichert sie als TIFF. Auflösung, Farbmodus, Scanfenster (areaMm, null = maximal),
    /// Helligkeit (−100 … +100, 0 = neutral) und Papierzufuhr werden gesetzt, soweit das
    /// Gerät sie annimmt. Null bei Fehlern — error beschreibt sie (übersetzt); beim
    /// Nutzer-Abbruch im WIA-Fortschritt bleibt error null.</summary>
    public static string ScanFromDevice(string deviceId, string path, int dpi, int colorIntent, SizeF? areaMm, int brightnessPercent, bool useFeeder, out string error)
    {
        error = null;
        try
        {
            var managerType = Type.GetTypeFromProgID("WIA.DeviceManager");
            if (managerType == null)
            {
                error = Lng.T("Die Windows-Bilderfassung (WIA) ist nicht verfügbar.");
                return null;
            }
            dynamic manager = Activator.CreateInstance(managerType);
            dynamic device = null;
            foreach (var info in manager.DeviceInfos)
            {
                if ((string)info.DeviceID == deviceId) { device = info.Connect(); break; }
            }
            if (device == null)
            {
                error = Lng.T("Der gewählte Scanner wurde nicht gefunden. Ist er angeschlossen und eingeschaltet?");
                return null;
            }
            TrySetProperty(device, WiaDocumentHandlingSelect, useFeeder ? 1 : 2); // Geräte ohne Einzug kennen die Eigenschaft nicht
            dynamic item = device.Items[1];
            TrySetProperty(item, WiaCurrentIntent, colorIntent);
            TrySetProperty(item, WiaHorizontalResolution, dpi);
            TrySetProperty(item, WiaVerticalResolution, dpi);
            if (areaMm is { } area)
            {
                // Das Fenster rechnet in Pixeln der WIRKSAMEN Auflösung — das Gerät darf unsere abgelehnt haben
                var actualDpi = TryGetProperty(item, WiaHorizontalResolution, dpi);
                TrySetProperty(item, WiaHorizontalStart, 0);
                TrySetProperty(item, WiaVerticalStart, 0);
                TrySetProperty(item, WiaHorizontalExtent, (int)Math.Round(area.Width / 25.4 * actualDpi));
                TrySetProperty(item, WiaVerticalExtent, (int)Math.Round(area.Height / 25.4 * actualDpi));
            }
            if (brightnessPercent != 0) { TrySetScaledProperty(item, WiaBrightness, brightnessPercent); }
            // ShowTransfer zeigt nur eine schlichte Fortschrittsanzeige (keinen Einstellungsdialog)
            var dialogType = Type.GetTypeFromProgID("WIA.CommonDialog");
            dynamic dialog = Activator.CreateInstance(dialogType);
            dynamic image = dialog.ShowTransfer(item, WiaFormatTiff, false); // Format ist ein Wunsch — das Gerät darf abweichen
            return image == null ? null : SaveAsTiff(image, path); // null ohne Ausnahme = Abbruch durch den Anwender
        }
        catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or InvalidOperationException)
        {
            System.Diagnostics.Debug.WriteLine("WIA-Scan: " + ex.Message);
            error = ex.Message;
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
        try
        {
            dynamic prop = item.Properties[propertyId.ToString()];
            try
            {
                if ((int)prop.SubType == 1) { value = Math.Clamp(value, (int)prop.SubTypeMin, (int)prop.SubTypeMax); } // 1 = Bereichs-Eigenschaft
            }
            catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or Microsoft.CSharp.RuntimeBinder.RuntimeBinderException) { }
            prop.Value = value;
        }
        catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
        { } // Gerät kennt die Eigenschaft nicht — Standard verwenden
    }

    private static int TryGetProperty(dynamic item, int propertyId, int fallback)
    {
        try { return (int)item.Properties[propertyId.ToString()].Value; }
        catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or Microsoft.CSharp.RuntimeBinder.RuntimeBinderException or InvalidCastException)
        { return fallback; }
    }

    /// <summary>Setzt einen Prozentwert (−100 … +100, 0 = Mitte) skaliert auf den Wertebereich,
    /// den das Gerät für die Eigenschaft meldet — Helligkeitsbereiche sind herstellerabhängig.</summary>
    private static void TrySetScaledProperty(dynamic item, int propertyId, int percent)
    {
        try
        {
            dynamic prop = item.Properties[propertyId.ToString()];
            var value = percent;
            try
            {
                if ((int)prop.SubType == 1)
                {
                    int min = (int)prop.SubTypeMin, max = (int)prop.SubTypeMax;
                    value = min + (int)Math.Round((percent + 100) / 200.0 * (max - min));
                }
            }
            catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or Microsoft.CSharp.RuntimeBinder.RuntimeBinderException) { }
            prop.Value = value;
        }
        catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or Microsoft.CSharp.RuntimeBinder.RuntimeBinderException) { }
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

    /// <summary>Speichert einen Scan als JPEG-Datei (Speichern-Dialog, Dateityp JPEG) —
    /// mit der eingestellten Qualität; die dpi-Angabe des Originals bleibt erhalten.</summary>
    public static void SaveAsJpeg(string sourcePath, string outputPath, int jpgQuality)
    {
        var encoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
        using EncoderParameters encoderParams = new(1);
        encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)jpgQuality);
        using var image = LoadUnlocked(sourcePath);
        image.Save(outputPath, encoder, encoderParams);
    }

    /// <summary>Legt eine Grafik zentriert und mit Rand auf eine weiße Seite mit 300 dpi — Hoch- oder
    /// Querformat je nach Seitenverhältnis des Bildes. Für importierte Bilder, die keinem Papierformat
    /// entsprechen (kleine Grafiken, Screenshots): als PDF-Seite wären sie sonst nur so groß wie das Bild.
    /// JPEG-Quellen bleiben JPEG (Fotos), alles andere wird PNG.</summary>
    public static void PlaceOnPage(string sourcePath, string outputPath, SizeF pageMm, float marginMm = 15, int dpi = 300)
    {
        using var image = LoadUnlocked(sourcePath);
        var landscape = image.Width > image.Height;
        var pageWidth = (int)Math.Round((landscape ? pageMm.Height : pageMm.Width) / 25.4 * dpi);
        var pageHeight = (int)Math.Round((landscape ? pageMm.Width : pageMm.Height) / 25.4 * dpi);
        var margin = (int)Math.Round(marginMm / 25.4 * dpi);
        var scale = Math.Min((pageWidth - 2 * margin) / (double)image.Width, (pageHeight - 2 * margin) / (double)image.Height);
        var width = Math.Max(1, (int)Math.Round(image.Width * scale));
        var height = Math.Max(1, (int)Math.Round(image.Height * scale));
        using Bitmap page = new(pageWidth, pageHeight);
        page.SetResolution(dpi, dpi);
        using (var g = Graphics.FromImage(page))
        {
            g.Clear(Color.White);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            using System.Drawing.Imaging.ImageAttributes attributes = new();
            attributes.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY); // kein Geistersaum an den Bildkanten
            g.DrawImage(image, new Rectangle((pageWidth - width) / 2, (pageHeight - height) / 2, width, height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
        }
        if (Path.GetExtension(outputPath).ToLowerInvariant() is ".jpg" or ".jpeg")
        {
            var encoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
            using EncoderParameters encoderParams = new(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L);
            page.Save(outputPath, encoder, encoderParams);
        }
        else
        {
            page.Save(outputPath, ImageFormat.Png);
        }
    }

    /// <summary>Speichert einen Scan als PNG-Datei (verlustfrei); die dpi-Angabe bleibt erhalten.</summary>
    public static void SaveAsPng(string sourcePath, string outputPath)
    {
        using var image = LoadUnlocked(sourcePath);
        image.Save(outputPath, ImageFormat.Png);
    }

    /// <summary>Speichert Scans als (bei mehreren Seiten mehrseitige) TIFF-Datei — verlustfrei
    /// mit LZW-Kompression; die dpi-Angaben der Originale bleiben erhalten.</summary>
    public static void SaveAsMultipageTiff(IReadOnlyList<string> sourcePaths, string outputPath)
    {
        var encoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Tiff.Guid);
        using var first = (Bitmap)LoadUnlocked(sourcePaths[0]);
        using (EncoderParameters firstParams = new(2))
        {
            firstParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, (long)EncoderValue.MultiFrame);
            firstParams.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, (long)EncoderValue.CompressionLZW);
            first.Save(outputPath, encoder, firstParams);
        }
        using (EncoderParameters pageParams = new(2))
        {
            pageParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage);
            pageParams.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, (long)EncoderValue.CompressionLZW);
            for (var i = 1; i < sourcePaths.Count; i++)
            {
                using var page = LoadUnlocked(sourcePaths[i]);
                first.SaveAdd(page, pageParams);
            }
        }
        using EncoderParameters flushParams = new(1);
        flushParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, (long)EncoderValue.Flush);
        first.SaveAdd(flushParams);
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

    /// <summary>Verkleinerte, von der Datei gelöste Kopie für die Miniaturansicht — hält statt des
    /// vollen Scans (~25 MB je Farbseite) nur wenige MB im Speicher. Alle Qualitätspfade (OCR,
    /// PDF, Druck, Zuschneiden) laden weiterhin das Original über LoadUnlocked.</summary>
    public static Image LoadThumbnail(string path, int maxWidth)
    {
        using var stream = new MemoryStream(File.ReadAllBytes(path));
        using var original = Image.FromStream(stream);
        if (original.Width <= maxWidth)
        {
            Bitmap copy = new(original);
            copy.SetResolution(original.HorizontalResolution, original.VerticalResolution);
            return copy;
        }
        var height = Math.Max(1, (int)Math.Round((double)original.Height * maxWidth / original.Width));
        Bitmap thumb = new(maxWidth, height);
        using var g = Graphics.FromImage(thumb);
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        using System.Drawing.Imaging.ImageAttributes attributes = new();
        attributes.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY); // verhindert den Geistersaum an den Rändern
        g.DrawImage(original, new Rectangle(0, 0, maxWidth, height), 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
        return thumb;
    }

    /// <summary>Lädt ein Bild ohne Dateisperre (Kopie im Speicher) — für Miniaturen, Druck und Drehen.</summary>
    public static Image LoadUnlocked(string path)
    {
        using var stream = new MemoryStream(File.ReadAllBytes(path));
        using var original = Image.FromStream(stream);
        Bitmap copy = new(original); // vom Stream gelöste Kopie …
        copy.SetResolution(original.HorizontalResolution, original.VerticalResolution); // … erbt aber sonst 96 dpi (GDI+)
        return copy;
    }
}
