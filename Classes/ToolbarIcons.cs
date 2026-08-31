using System.Drawing.Text;

namespace ScanTest.Classes;

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

    private const string FontName = "Segoe MDL2 Assets";
    private static readonly Dictionary<(char Glyph, int Size), Image> cache = [];

    /// <summary>False, falls die Symbolschrift fehlt — dann bleiben die Buttons reine Textbuttons.</summary>
    public static bool FontAvailable { get; } = CheckFontAvailable();

    private static bool CheckFontAvailable()
    {
        using Font font = new(FontName, 10f);
        return string.Equals(font.Name, FontName, StringComparison.OrdinalIgnoreCase); // GDI fällt sonst stumm auf eine Standardschrift zurück
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
