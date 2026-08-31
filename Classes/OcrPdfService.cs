using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Tesseract;

namespace ScanTest.Classes;

/// <summary>OCR und PDF-Zusammenbau: Tesseract macht aus jedem Scan eine durchsuchbare
/// Einzelseiten-PDF (Bild + unsichtbare Textschicht), PDFsharp fügt sie zum Enddokument zusammen.</summary>
internal static class OcrPdfService
{
    private static string TessData => Path.Combine(AppContext.BaseDirectory, "tessdata");

    /// <summary>Erstellt aus den TIFF-Scans eine durchsuchbare PDF; progress meldet (fertige Seite, Gesamtzahl).</summary>
    public static void CreateSearchablePdf(IReadOnlyList<string> tiffFiles, string outputPdf, string language, Action<int, int> progress)
    {
        TesseractEnviornment.CustomSearchPath = Path.Combine(AppContext.BaseDirectory, "x64"); // native DLLs des NuGet-Pakets
        List<string> pagePdfs = [];
        try
        {
            for (var i = 0; i < tiffFiles.Count; i++)
            {
                var pdfBase = Path.Combine(Path.GetTempPath(), "ScanTest_" + Guid.NewGuid().ToString("N"));
                using (var renderer = ResultRenderer.CreatePdfRenderer(pdfBase, TessData, false))
                using (renderer.BeginDocument("ScanTest"))
                {
                    using TesseractEngine engine = new(TessData, language, EngineMode.LstmOnly, [],
                        new Dictionary<string, object> { { "user_defined_dpi", 300 }, { "jpg_quality", 75 } }, false);
                    using var pix = Pix.LoadFromFile(tiffFiles[i]);
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
