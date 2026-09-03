using System.Text;
using PdfSharp.Pdf;

namespace ScanView.Classes;

/// <summary>Rüstet einer frisch erzeugten Scan-PDF die PDF/A-2b-Bausteine nach, die PDFsharp 6.2
/// selbst noch nicht anbietet: XMP-Metadatenstrom (inhaltsgleich mit dem Info-Dictionary, wie die
/// Norm es verlangt) und sRGB-OutputIntent mit dem ICC-Profil aus dem Windows-Farbordner.
/// Die übrigen Anforderungen erfüllt die Werkzeugkette bereits (eingebettete Tesseract-Schrift,
/// JPEG/DCTDecode, keine Verschlüsselung; PDF/A-2 erlaubt die Transparenz-Gruppen der
/// Tesseract-Seiten). Läuft als NACHBEARBEITUNG der gespeicherten Datei, weil PDFsharp den
/// /Producer erst beim Speichern einträgt — das XMP muss ihn aber wortgleich wiederholen.</summary>
internal static class PdfAHelper
{
    /// <summary>Ergänzt die eben gespeicherte PDF um Metadata-Stream und OutputIntent
    /// und speichert sie erneut.</summary>
    public static void Finish(string pdfPath)
    {
        using var document = PdfSharp.Pdf.IO.PdfReader.Open(pdfPath, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Modify);
        var stamp = document.Info.CreationDate; // Info-Dictionary und XMP müssen laut Norm übereinstimmen
        if (stamp.Kind == DateTimeKind.Utc) { stamp = stamp.ToLocalTime(); } // einheitlich lokale Zeitzone wie das gespeicherte /CreationDate
        document.Info.ModificationDate = stamp;
        var xmp = BuildXmp(document, stamp);
        PdfDictionary metadata = new(document);
        metadata.CreateStream(Encoding.UTF8.GetBytes(xmp)); // unkomprimiert — der Metadata-Stream darf keinen Filter tragen
        metadata.Elements["/Type"] = new PdfName("/Metadata");
        metadata.Elements["/Subtype"] = new PdfName("/XML");
        document.Internals.AddObject(metadata);
        document.Internals.Catalog.Elements["/Metadata"] = metadata.Reference;

        var iccPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),
            "spool", "drivers", "color", "sRGB Color Space Profile.icm"); // auf jedem Windows vorhanden
        if (!File.Exists(iccPath)) { throw new InvalidOperationException("sRGB-Farbprofil nicht gefunden: " + iccPath); }
        PdfDictionary profile = new(document);
        profile.CreateStream(File.ReadAllBytes(iccPath));
        profile.Elements["/N"] = new PdfInteger(3);
        document.Internals.AddObject(profile);
        PdfDictionary intent = new(document);
        intent.Elements["/Type"] = new PdfName("/OutputIntent");
        intent.Elements["/S"] = new PdfName("/GTS_PDFA1");
        intent.Elements["/OutputConditionIdentifier"] = new PdfString("sRGB IEC61966-2.1");
        intent.Elements["/Info"] = new PdfString("sRGB IEC61966-2.1");
        intent.Elements["/RegistryName"] = new PdfString("http://www.color.org");
        intent.Elements["/DestOutputProfile"] = profile.Reference;
        document.Internals.AddObject(intent);
        document.Internals.Catalog.Elements["/OutputIntents"] = new PdfArray(document, intent.Reference);
        document.Save(pdfPath);
    }

    private static string BuildXmp(PdfDocument document, DateTime stamp)
    {
        var date = stamp.ToString("yyyy-MM-dd'T'HH:mm:sszzz");
        StringBuilder sb = new();
        sb.Append("<?xpacket begin=\"﻿\" id=\"W5M0MpCehiHzreSzNTczkc9d\"?>\n"); // im begin-Attribut steckt das unsichtbare U+FEFF (vorgeschriebene UTF-8-Kennung)
        sb.Append("<x:xmpmeta xmlns:x=\"adobe:ns:meta/\">\n <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">\n");
        sb.Append("  <rdf:Description rdf:about=\"\" xmlns:pdfaid=\"http://www.aiim.org/pdfa/ns/id/\" pdfaid:part=\"2\" pdfaid:conformance=\"B\"/>\n");
        sb.Append("  <rdf:Description rdf:about=\"\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\">\n");
        sb.Append("   <dc:title><rdf:Alt><rdf:li xml:lang=\"x-default\">").Append(Esc(document.Info.Title)).Append("</rdf:li></rdf:Alt></dc:title>\n");
        sb.Append("   <dc:creator><rdf:Seq><rdf:li>").Append(Esc(document.Info.Author)).Append("</rdf:li></rdf:Seq></dc:creator>\n");
        if (document.Info.Subject.Length > 0)
        {
            sb.Append("   <dc:description><rdf:Alt><rdf:li xml:lang=\"x-default\">").Append(Esc(document.Info.Subject)).Append("</rdf:li></rdf:Alt></dc:description>\n");
        }
        sb.Append("  </rdf:Description>\n");
        sb.Append("  <rdf:Description rdf:about=\"\" xmlns:pdf=\"http://ns.adobe.com/pdf/1.3/\" pdf:Producer=\"").Append(Esc(document.Info.Producer)).Append('"');
        if (document.Info.Keywords.Length > 0) { sb.Append(" pdf:Keywords=\"").Append(Esc(document.Info.Keywords)).Append('"'); }
        sb.Append("/>\n");
        sb.Append("  <rdf:Description rdf:about=\"\" xmlns:xmp=\"http://ns.adobe.com/xap/1.0/\" xmp:CreateDate=\"").Append(date)
            .Append("\" xmp:ModifyDate=\"").Append(date).Append("\"/>\n");
        sb.Append(" </rdf:RDF>\n</x:xmpmeta>\n<?xpacket end=\"w\"?>");
        return sb.ToString();
    }

    private static string Esc(string value) =>
        (value ?? string.Empty).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
}
