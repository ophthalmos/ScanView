using System.Drawing.Imaging;

namespace ScanTest.Classes;

/// <summary>Bildbeschaffung: echter Scan über den Windows-WIA-Dialog oder eine gerenderte Testseite.</summary>
internal static class ScanService
{
    /// <summary>Scannt eine Seite über den Windows-Scandialog (WIA, spätbindend — kein COM-Verweis nötig)
    /// und speichert sie als TIFF. Null bei Abbruch oder ohne Scanner.</summary>
    public static string WiaScanToTiff(string path)
    {
        try
        {
            var dialogType = Type.GetTypeFromProgID("WIA.CommonDialog");
            if (dialogType == null) { return null; }
            dynamic dialog = Activator.CreateInstance(dialogType);
            dynamic image = dialog.ShowAcquireImage(); // Gerätewahl und Scan übernimmt der Windows-Dialog
            if (image == null) { return null; }
            if (File.Exists(path)) { File.Delete(path); }
            image.SaveFile(path);
            return path;
        }
        catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or InvalidOperationException)
        {
            System.Diagnostics.Debug.WriteLine("WIA: " + ex.Message);
            return null;
        }
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
