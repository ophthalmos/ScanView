using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Tesseract;

namespace ScanView.Classes;

/// <summary>Metadaten der erzeugten PDF (Speichern-Dialog); PdfA ergänzt die PDF/A-2b-Bausteine —
/// nur für Bild-PDFs ohne Texterkennung vorgesehen (s. PdfAHelper).</summary>
internal sealed record PdfMeta(string Title, string Subject, string Keywords, string Author, bool PdfA = false);

/// <summary>OCR und PDF-Zusammenbau: Tesseract macht aus jedem Scan eine durchsuchbare
/// Einzelseiten-PDF (Bild + unsichtbare Textschicht), PDFsharp fügt sie zum Enddokument zusammen.</summary>
internal static class OcrPdfService
{
    private static string TessData => Path.Combine(AppContext.BaseDirectory, "tessdata");

    /// <summary>Info-Dictionary aus den Dialog-Metadaten füllen (leere Felder: Dateiname als Titel,
    /// Windows-Benutzer als Verfasser).</summary>
    private static void ApplyMeta(PdfDocument result, string outputPdf, PdfMeta meta)
    {
        // Ersteller = Anwendung, Produzent = PDF-Bibliothek (PDF-Konvention) — den /Producer
        // trägt PDFsharp ohnehin selbst ein, die Eigenschaft ist dort schreibgeschützt
        result.Info.Creator = "ScanView " + (System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "");
        result.Info.Title = string.IsNullOrWhiteSpace(meta?.Title) ? Path.GetFileNameWithoutExtension(outputPdf) : meta.Title;
        result.Info.Author = string.IsNullOrWhiteSpace(meta?.Author) ? Environment.UserName : meta.Author;
        if (!string.IsNullOrWhiteSpace(meta?.Subject)) { result.Info.Subject = meta.Subject; }
        if (!string.IsNullOrWhiteSpace(meta?.Keywords)) { result.Info.Keywords = meta.Keywords; }
    }

    /// <summary>Erstellt eine PDF ohne Textschicht: jede Seite als JPEG in Originalgröße —
    /// für Scans, bei denen keine Texterkennung gewünscht ist.</summary>
    public static void CreateImagePdf(IReadOnlyList<string> tiffFiles, string outputPdf, int jpgQuality, Action<int, int> progress, PdfMeta meta = null)
    {
        var encoder = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()
            .First(c => c.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);
        using System.Drawing.Imaging.EncoderParameters encoderParams = new(1);
        encoderParams.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)jpgQuality);
        using PdfDocument result = new();
        for (var i = 0; i < tiffFiles.Count; i++)
        {
            using var image = ScanService.LoadUnlocked(tiffFiles[i]);
            using MemoryStream jpeg = new();
            image.Save(jpeg, encoder, encoderParams); // als JPEG einbetten — PDFsharp übernimmt den Stream unverändert
            jpeg.Position = 0;
            using var ximage = PdfSharp.Drawing.XImage.FromStream(jpeg);
            ximage.Interpolate = false; // PDF/A verbietet /Interpolate true (veraPDF-Regel 6.2.4); PDFsharp-Standard wäre true
            var page = result.AddPage();
            page.Width = PdfSharp.Drawing.XUnit.FromPoint(ximage.PointWidth); // Seitengröße = physische Bildgröße (dpi)
            page.Height = PdfSharp.Drawing.XUnit.FromPoint(ximage.PointHeight);
            using var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page);
            gfx.DrawImage(ximage, 0, 0, page.Width.Point, page.Height.Point);
            progress?.Invoke(i + 1, tiffFiles.Count);
        }
        ApplyMeta(result, outputPdf, meta);
        result.Save(outputPdf);
        if (meta?.PdfA == true) { PdfAHelper.Finish(outputPdf); } // Nachbearbeitung — s. PdfAHelper
    }

    /// <summary>Erstellt aus den TIFF-Scans eine durchsuchbare PDF; progress meldet (fertige Seite,
    /// Gesamtzahl) und kann aus BELIEBIGEN Threads kommen. Die Seiten werden PARALLEL erkannt —
    /// die Texterkennung selbst ist der einzige nennenswerte Kostenpunkt (~2,6 s je volle Seite),
    /// Engine-Initialisierung ist mit ~0,03 s vernachlässigbar, daher eine Engine je Seite.</summary>
    public static void CreateSearchablePdf(IReadOnlyList<string> tiffFiles, string outputPdf, string language, int jpgQuality, Action<int, int> progress, PdfMeta meta = null)
    {
        TesseractEnviornment.CustomSearchPath = Path.Combine(AppContext.BaseDirectory, "x64"); // native DLLs des NuGet-Pakets
        var pagePdfs = new string[tiffFiles.Count];
        var done = 0;
        try
        {
            Parallel.For(0, tiffFiles.Count,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
            {
                var pdfBase = Path.Combine(Path.GetTempPath(), "ScanView_" + Guid.NewGuid().ToString("N"));
                using (var renderer = ResultRenderer.CreatePdfRenderer(pdfBase, TessData, false))
                using (renderer.BeginDocument("ScanView"))
                {
                    // user_defined_dpi ist eine Init-Variable und nur der FALLBACK für Bilder ohne
                    // dpi-Metadaten — Scans und LoadUnlocked-Kopien bringen ihre Auflösung selbst mit
                    using TesseractEngine engine = new(TessData, language, EngineMode.LstmOnly, [],
                        new Dictionary<string, object> { { "user_defined_dpi", 300 }, { "jpg_quality", jpgQuality } }, false);
                    using var pix = Pix.LoadFromFile(tiffFiles[i]);
                    // zweiter Parameter = Bilddateiname: daraus lädt der PDF-Renderer das einzubettende Bild
                    using var page = engine.Process(pix, tiffFiles[i], PageSegMode.Auto);
                    renderer.AddPage(page);
                }
                pagePdfs[i] = pdfBase + ".pdf";
                progress?.Invoke(Interlocked.Increment(ref done), tiffFiles.Count);
            });

            using PdfDocument result = new();
            foreach (var pagePdf in pagePdfs) // in Seitenreihenfolge zusammensetzen
            {
                using var source = PdfReader.Open(pagePdf, PdfDocumentOpenMode.Import);
                foreach (var page in source.Pages) { result.AddPage(page); }
            }
            ApplyMeta(result, outputPdf, meta);
            result.Save(outputPdf);
        }
        finally
        {
            foreach (var pagePdf in pagePdfs)
            {
                if (pagePdf != null) { try { File.Delete(pagePdf); } catch (IOException) { } }
            }
        }
    }
}
