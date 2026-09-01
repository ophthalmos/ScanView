using System.Drawing.Text;

namespace ScanView.Classes;

/// <summary>
/// Rendert Symbole für die Toolbar aus der Windows-Symbolschrift "Segoe MDL2 Assets"
/// (ab Windows 10 vorinstalliert) — DPI-scharf und ohne eingebettete Bilddateien
/// (gleiches Muster wie in PDFlight).
/// Die Glyphen-Codes: https://learn.microsoft.com/windows/apps/design/style/segoe-ui-symbol-font
/// </summary>
internal static class ToolbarIcons
{
    public const char Scan = '';
    public const char Save = '';
    public const char Print = '';
    public const char Clear = '';    // Neu: Seitenübersicht leeren
    public const char Previous = ''; // ChevronLeft
    public const char Next = '';     // ChevronRight
    public const char Delete = '';
    public const char ZoomOut = '';
    public const char ZoomIn = '';

    private const char Page = '';
    private const char Star = '';       // FavoriteStarFill
    private const char NewPageKey = ''; // Cache-Schlüssel für das zusammengesetzte "Neu"-Symbol

    public const char Import = '';
    public const char Power = '';      // Schließen
    public const char Cut = '';
    public const char Copy = '';
    public const char Paste = '';
    public const char Rotate = '';     // im Uhrzeigersinn; links = GetMirrored
    public const char Rotate180 = '';  // Refresh (Kreispfeil)
    public const char Interleave = ''; // Switch: Rückseiten verzahnen
    public const char Sort = '';
    public const char FitFrame = '';   // Optimale Breite
    public const char SinglePage = ''; // Ganze Seite
    public const char TwoPages = '';
    public const char GridView = '';   // Symbole
    public const char FullScreen = '';
    public const char Settings = '';
    public const char Info = '';

    private const string FontName = "Segoe MDL2 Assets";
    private static readonly Dictionary<(char Glyph, int Size), Image> cache = [];

    /// <summary>False, falls die Symbolschrift fehlt — dann bleiben die Buttons reine Textbuttons.</summary>
    public static bool FontAvailable { get; } = CheckFontAvailable();

    private static bool CheckFontAvailable()
    {
        using Font font = new(FontName, 10f);
        return string.Equals(font.Name, FontName, StringComparison.OrdinalIgnoreCase); // GDI fällt sonst stumm auf eine Standardschrift zurück
    }

    private static readonly Dictionary<(char Glyph, int Size), Image> mirroredCache = [];

    /// <summary>Horizontal gespiegelte Glyphe — z.B. der Dreh-Pfeil für "Drehen nach links".</summary>
    public static Image GetMirrored(char glyph, Size size)
    {
        if (!mirroredCache.TryGetValue((glyph, size.Width), out var image))
        {
            var mirrored = new Bitmap(Get(glyph, size));
            mirrored.RotateFlip(RotateFlipType.RotateNoneFlipX);
            image = mirrored;
            mirroredCache[(glyph, size.Width)] = image;
        }
        return image;
    }

    /// <summary>Zusammengesetztes Symbol für "Neu": leeres Blatt mit Sternchen rechts oben.</summary>
    public static Image GetNewPage(Size size)
    {
        if (!cache.TryGetValue((NewPageKey, size.Width), out var image))
        {
            Bitmap bitmap = new(size.Width, size.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                using StringFormat format = new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using SolidBrush brush = new(Color.FromArgb(64, 64, 64));
                using Font pageFont = new(FontName, size.Height * 0.72f, GraphicsUnit.Pixel);
                g.DrawString(Page.ToString(), pageFont, brush, new RectangleF(-size.Width * 0.08f, size.Height * 0.10f, size.Width, size.Height), format);
                using Font starFont = new(FontName, size.Height * 0.42f, GraphicsUnit.Pixel);
                g.DrawString(Star.ToString(), starFont, brush, new RectangleF(size.Width * 0.30f, -size.Height * 0.30f, size.Width, size.Height), format);
            }
            image = bitmap;
            cache[(NewPageKey, size.Width)] = image;
        }
        return image;
    }

    public static Image Get(char glyph, Size size)
    {
        if (!cache.TryGetValue((glyph, size.Width), out var image))
        {
            Bitmap bitmap = new(size.Width, size.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                using Font font = new(FontName, size.Height * 0.75f, GraphicsUnit.Pixel);
                using StringFormat format = new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using SolidBrush brush = new(Color.FromArgb(64, 64, 64));
                g.DrawString(glyph.ToString(), font, brush, new RectangleF(0, 0, size.Width, size.Height), format);
            }
            image = bitmap;
            cache[(glyph, size.Width)] = image;
        }
        return image;
    }
}
