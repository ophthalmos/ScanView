using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Tesseract;

namespace ScanView.Classes;

/// <summary>OCR und PDF-Zusammenbau: Tesseract macht aus jedem Scan eine durchsuchbare
/// Einzelseiten-PDF (Bild + unsichtbare Textschicht), PDFsharp fügt sie zum Enddokument zusammen.</summary>
internal static class OcrPdfService
{
    private static string TessData => Path.Combine(AppContext.BaseDirectory, "tessdata");

    /// <summary>Erstellt eine PDF ohne Textschicht: jede Seite als JPEG in Originalgröße —
    /// für Scans, bei denen keine Texterkennung gewünscht ist.</summary>
    public static void CreateImagePdf(IReadOnlyList<string> tiffFiles, string outputPdf, int jpgQuality, Action<int, int> progress)
    {
        var encoder = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()
            .First(c => c.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);
        using System.Drawing.Imaging.EncoderParameters encoderParams = new(1);
        encoderParams.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)jpgQuality);
        using PdfDocument result = new();
        result.Info.Title = Path.GetFileNameWithoutExtension(outputPdf);
        result.Info.Author = Environment.UserName;
        for (var i = 0; i < tiffFiles.Count; i++)
        {
            using var image = ScanService.LoadUnlocked(tiffFiles[i]);
            using MemoryStream jpeg = new();
            image.Save(jpeg, encoder, encoderParams); // als JPEG einbetten — PDFsharp übernimmt den Stream unverändert
            jpeg.Position = 0;
            using var ximage = PdfSharp.Drawing.XImage.FromStream(jpeg);
            var page = result.AddPage();
            page.Width = PdfSharp.Drawing.XUnit.FromPoint(ximage.PointWidth); // Seitengröße = physische Bildgröße (dpi)
            page.Height = PdfSharp.Drawing.XUnit.FromPoint(ximage.PointHeight);
            using var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page);
            gfx.DrawImage(ximage, 0, 0, page.Width.Point, page.Height.Point);
            progress?.Invoke(i + 1, tiffFiles.Count);
        }
        result.Save(outputPdf);
    }

    /// <summary>Erstellt aus den TIFF-Scans eine durchsuchbare PDF; progress meldet (fertige Seite,
    /// Gesamtzahl). Engine und PDF-Renderer werden EINMAL für alle Seiten erzeugt — die Engine-
    /// Initialisierung lädt sonst pro Seite das komplette Sprachmodell (tessdata_best, ~9 MB) neu.</summary>
    public static void CreateSearchablePdf(IReadOnlyList<string> tiffFiles, string outputPdf, string language, int jpgQuality, Action<int, int> progress)
    {
        TesseractEnviornment.CustomSearchPath = Path.Combine(AppContext.BaseDirectory, "x64"); // native DLLs des NuGet-Pakets
        var pdfBase = Path.Combine(Path.GetTempPath(), "ScanView_" + Guid.NewGuid().ToString("N"));
        var tempPdf = pdfBase + ".pdf";
        try
        {
            using (var renderer = ResultRenderer.CreatePdfRenderer(pdfBase, TessData, false))
            using (renderer.BeginDocument("ScanView"))
            // user_defined_dpi ist eine Init-Variable und nur der FALLBACK für Bilder ohne
            // dpi-Metadaten — Scans und LoadUnlocked-Kopien bringen ihre Auflösung selbst mit
            using (TesseractEngine engine = new(TessData, language, EngineMode.LstmOnly, [],
                new Dictionary<string, object> { { "user_defined_dpi", 300 }, { "jpg_quality", jpgQuality } }, false))
            {
                for (var i = 0; i < tiffFiles.Count; i++)
                {
                    using var pix = Pix.LoadFromFile(tiffFiles[i]);
                    // zweiter Parameter = Bilddateiname: daraus lädt der PDF-Renderer das einzubettende Bild
                    using var page = engine.Process(pix, tiffFiles[i], PageSegMode.Auto);
                    renderer.AddPage(page);
                    progress?.Invoke(i + 1, tiffFiles.Count);
                }
            }
            // nur noch Metadaten setzen und ans Ziel schreiben — kein Seiten-Merge mehr nötig
            using var result = PdfReader.Open(tempPdf, PdfDocumentOpenMode.Modify);
            result.Info.Title = Path.GetFileNameWithoutExtension(outputPdf);
            result.Info.Author = Environment.UserName;
            result.Save(outputPdf);
        }
        finally
        {
            try { File.Delete(tempPdf); } catch (IOException) { }
        }
    }
}
