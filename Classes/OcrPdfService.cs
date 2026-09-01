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

    /// <summary>Erstellt aus den TIFF-Scans eine durchsuchbare PDF; progress meldet (fertige Seite, Gesamtzahl).</summary>
    public static void CreateSearchablePdf(IReadOnlyList<string> tiffFiles, string outputPdf, string language, int jpgQuality, Action<int, int> progress)
    {
        TesseractEnviornment.CustomSearchPath = Path.Combine(AppContext.BaseDirectory, "x64"); // native DLLs des NuGet-Pakets
        List<string> pagePdfs = [];
        try
        {
            for (var i = 0; i < tiffFiles.Count; i++)
            {
                var pdfBase = Path.Combine(Path.GetTempPath(), "ScanView_" + Guid.NewGuid().ToString("N"));
                using (var renderer = ResultRenderer.CreatePdfRenderer(pdfBase, TessData, false))
                using (renderer.BeginDocument("ScanView"))
                {
                    using var pix = Pix.LoadFromFile(tiffFiles[i]);
                    // Echte Auflösung der Seite an Tesseract melden (wie in PDFMover) — sonst stimmen
                    // Zeichenskalierung der Erkennung und PDF-Seitengröße nicht
                    var dpi = pix.XRes >= 70 ? pix.XRes : 300;
                    using TesseractEngine engine = new(TessData, language, EngineMode.LstmOnly, [],
                        new Dictionary<string, object> { { "user_defined_dpi", dpi }, { "jpg_quality", jpgQuality } }, false);
                    // zweiter Parameter = Bilddateiname: daraus lädt der PDF-Renderer das einzubettende Bild
                    using var page = engine.Process(pix, tiffFiles[i], PageSegMode.Auto);
                    renderer.AddPage(page);
                }
                pagePdfs.Add(pdfBase + ".pdf");
                progress?.Invoke(i + 1, tiffFiles.Count);
            }

            using PdfDocument result = new();
            result.Info.Title = Path.GetFileNameWithoutExtension(outputPdf);
            result.Info.Author = Environment.UserName;
            foreach (var pagePdf in pagePdfs)
            {
                using var source = PdfReader.Open(pagePdf, PdfDocumentOpenMode.Import);
                foreach (var page in source.Pages) { result.AddPage(page); }
            }
            result.Save(outputPdf);
        }
        finally
        {
            foreach (var pagePdf in pagePdfs)
            {
                try { File.Delete(pagePdf); } catch (IOException) { }
            }
        }
    }
}
