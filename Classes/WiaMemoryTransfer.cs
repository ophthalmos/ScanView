using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace ScanView.Classes;

/// <summary>Bildübertragung vom Scanner über den Speicher (WIA-Banded-Transfer, TYMED_CALLBACK) statt über
/// eine Temp-Datei. Hintergrund: Seit dem Windows-Sicherheitsupdate vom Oktober 2026 scheitert der
/// Datei-Weg der WIA-Automatisierung (ShowTransfer/Item.Transfer) bei älteren Treibern wie dem des
/// EPSON Perfection V30/V300 mit E_ACCESSDENIED; der Speicher-Weg (so arbeitet auch Acrobat) funktioniert.
/// Die Eigenschaften setzt weiterhin die WIA-Automatisierung – deren Item liefert über „WiaItem" die
/// darunterliegende IWiaItem-Schnittstelle, die auch IWiaDataTransfer anbietet.</summary>
internal static partial class WiaMemoryTransfer
{
    public const string FormatMemoryBmp = "{B96B3CAA-0728-11D3-9D7B-0000F81EF32E}"; // WiaImgFmt_MEMORYBMP
    public const int TymedCallback = 128;
    private static readonly StrategyBasedComWrappers ComWrappersInstance = new();
    private const int ItMsgDataHeader = 1;
    private const int ItMsgData = 2;

    [StructLayout(LayoutKind.Sequential)]
    internal struct WiaDataTransferInfo
    {
        public uint Size, Section, BufferSize;
        public int DoubleBuffer;
        public uint Reserved1, Reserved2, Reserved3;
    }

    [GeneratedComInterface, Guid("a558a866-a5b0-11d2-a08f-00c04f72dc3c"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal partial interface IWiaDataCallback
    {
        [PreserveSig]
        int BandedDataCallback(int message, int status, int percentComplete, int offset, int length, int reserved, int resLength, IntPtr buffer);
    }

    [GeneratedComInterface, Guid("a6cef998-a5b0-11d2-a08f-00c04f72dc3c"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal partial interface IWiaDataTransfer
    {
        [PreserveSig] int IdtGetData(IntPtr medium, IWiaDataCallback callback);
        [PreserveSig] int IdtGetBandedData(ref WiaDataTransferInfo info, IWiaDataCallback callback);
        [PreserveSig] int IdtQueryGetData(IntPtr formatInfo);
        [PreserveSig] int IdtEnumWIA_FORMAT_INFO(out IntPtr enumerator);
        [PreserveSig] int IdtGetExtendedTransferInfo(IntPtr info);
    }

    /// <summary>Sammelt die Datenblöcke (an ihrem Offset) und meldet den Fortschritt; S_FALSE bricht ab.</summary>
    [GeneratedComClass]
    internal sealed partial class Collector(Func<int, bool>? progress) : IWiaDataCallback
    {
        public MemoryStream Data { get; } = new();
        public bool Cancelled { get; private set; }
        private long expected; // Gesamtgröße laut Kopfmeldung – viele Treiber melden selbst keinen Prozentwert

        public int BandedDataCallback(int message, int status, int percentComplete, int offset, int length, int reserved, int resLength, IntPtr buffer)
        {
            if (message == ItMsgDataHeader && buffer != IntPtr.Zero) { expected = Marshal.ReadInt32(buffer, 20); } // WIA_DATA_CALLBACK_HEADER.lBufferSize
            if (message == ItMsgData && length > 0)
            {
                var chunk = new byte[length];
                Marshal.Copy(buffer, chunk, 0, length);
                Data.Position = offset;
                Data.Write(chunk, 0, length);
            }
            var percent = percentComplete > 0 || expected <= 0 ? percentComplete : (int)((Data.Length * 100) / expected);
            if (progress != null && !progress(Math.Clamp(percent, 0, 100)))
            {
                Cancelled = true;
                return 1; // S_FALSE: Übertragung abbrechen
            }
            return 0;
        }
    }

    /// <summary>Überträgt das Bild des Items (Format MEMORYBMP und TYMED_CALLBACK müssen gesetzt sein) und
    /// speichert es als TIFF im gelieferten Pixelformat (Graustufen bleiben 8 Bit, Schwarz-weiß 1 Bit).
    /// progress bekommt den Fortschritt in Prozent und liefert false zum Abbrechen; dann ist das Ergebnis
    /// false und keine Datei entsteht. Fehler des Treibers kommen als COMException.</summary>
    public static bool TransferToTiff(object wiaItem, string path, int fallbackDpi, Func<int, bool>? progress)
    {
        // Das Item stammt aus der WIA-Automatisierung (eingebautes COM-Interop); quellgenerierte Schnittstellen
        // brauchen ein ComWrappers-Objekt – daher über den IUnknown-Zeiger umwandeln statt direkt zu casten
        var unknown = Marshal.GetIUnknownForObject(wiaItem);
        IWiaDataTransfer transfer;
        try { transfer = (IWiaDataTransfer)ComWrappersInstance.GetOrCreateObjectForComInstance(unknown, CreateObjectFlags.None); }
        finally { Marshal.Release(unknown); }
        Collector collector = new(progress);
        var info = new WiaDataTransferInfo
        {
            Size = (uint)Marshal.SizeOf<WiaDataTransferInfo>(),
            BufferSize = 256 * 1024,
            DoubleBuffer = 1,
        };
        var hr = transfer.IdtGetBandedData(ref info, collector);
        if (collector.Cancelled) { return false; }
        Marshal.ThrowExceptionForHR(hr);
        SaveAsTiff(collector.Data.ToArray(), path, fallbackDpi);
        return true;
    }

    /// <summary>MEMORYBMP = DIB ohne Dateikopf (BITMAPINFOHEADER, ggf. Palette, Bildpunkte): den Kopf
    /// ergänzen, laden und mit der Scanauflösung als TIFF speichern – ohne Umweg über eine Kopie,
    /// die das Pixelformat auf 32 Bit aufblähen würde.</summary>
    private static void SaveAsTiff(byte[] dib, string path, int fallbackDpi)
    {
        if (dib.Length < 40) { throw new InvalidOperationException(Lng.T("Der Scanner hat kein Bild geliefert.")); }
        var headerSize = BitConverter.ToInt32(dib, 0);
        var width = BitConverter.ToInt32(dib, 4);
        var height = BitConverter.ToInt32(dib, 8);
        int bitCount = BitConverter.ToInt16(dib, 14);
        var compression = BitConverter.ToInt32(dib, 16);
        var xPelsPerMeter = BitConverter.ToInt32(dib, 24);
        var colorsUsed = BitConverter.ToInt32(dib, 32);
        var paletteEntries = colorsUsed != 0 ? colorsUsed : bitCount <= 8 ? 1 << bitCount : 0;
        var pixelOffset = headerSize + paletteEntries * 4 + (compression == 3 && headerSize == 40 ? 12 : 0); // BI_BITFIELDS: drei Masken
        if (height == 0 && width > 0) // Einzug mit unbekannter Länge: Zeilen aus der Datenmenge bestimmen (von oben nach unten)
        {
            var stride = (width * bitCount + 31) / 32 * 4;
            BitConverter.GetBytes(-((dib.Length - pixelOffset) / stride)).CopyTo(dib, 8);
        }
        var file = new byte[14 + dib.Length];
        file[0] = (byte)'B';
        file[1] = (byte)'M';
        BitConverter.GetBytes(file.Length).CopyTo(file, 2);
        BitConverter.GetBytes(14 + pixelOffset).CopyTo(file, 10);
        dib.CopyTo(file, 14);
        using MemoryStream stream = new(file);
        using var image = (Bitmap)Image.FromStream(stream);
        var dpi = xPelsPerMeter > 0 ? (float)Math.Round(xPelsPerMeter * 0.0254) : fallbackDpi;
        image.SetResolution(dpi, dpi);
        if (File.Exists(path)) { File.Delete(path); }
        image.Save(path, System.Drawing.Imaging.ImageFormat.Tiff);
    }
}
