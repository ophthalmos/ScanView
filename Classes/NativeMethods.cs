using System.Runtime.InteropServices;

namespace ScanView.Classes;

/// <summary>Win32-Aufrufe für die Einmal-Instanz: eine registrierte Broadcast-Nachricht holt die
/// laufende Instanz in den Vordergrund (gleiche Lösung wie in NetRadio).</summary>
internal static partial class NativeMethods
{
    public const int HWND_BROADCAST = 0xffff;

    /// <summary>Systemweit registrierte Nachrichten — in allen Prozessen derselbe Wert.</summary>
    public static readonly uint WM_SHOWSCANVIEW = RegisterWindowMessage("WM_SHOWSCANVIEW");

    /// <summary>Wie WM_SHOWSCANVIEW, löst zusätzlich sofort einen Scan aus (Scanner-Taste).</summary>
    public static readonly uint WM_SCANSCANVIEW = RegisterWindowMessage("WM_SCANSCANVIEW");

    [LibraryImport("user32.dll", EntryPoint = "RegisterWindowMessageW", StringMarshalling = StringMarshalling.Utf16)]
    public static partial uint RegisterWindowMessage(string lpString);

    [LibraryImport("user32.dll", EntryPoint = "PostMessageW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool PostMessage(nint hWnd, uint msg, nint wParam, nint lParam);

    // Für EM_SETMARGINS (Innenabstand der TextBoxen, s. TextBoxMargins.Apply)
    [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
    public static partial nint SendMessage(nint hWnd, uint msg, nint wParam, nint lParam);

    // Für den Treiber-Eigenschaften-Dialog des Druckers (s. PrinterDialog.ShowProperties)
    [LibraryImport("winspool.drv", EntryPoint = "DocumentPropertiesW", StringMarshalling = StringMarshalling.Utf16)]
    public static partial int DocumentProperties(nint hwnd, nint hPrinter, string deviceName, nint devModeOutput, nint devModeInput, int mode);

    [LibraryImport("kernel32.dll")]
    public static partial nint GlobalLock(nint handle);

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GlobalUnlock(nint handle);

    [LibraryImport("kernel32.dll")]
    public static partial nint GlobalFree(nint handle);

    // Für die Geräteprüfung der TWAIN-Quellen (s. ImagingDevices): Geräte der Klasse „Bildverarbeitungsgeräte"
    public const uint SPDRP_DEVICEDESC = 0x0, SPDRP_MFG = 0xB, SPDRP_FRIENDLYNAME = 0xC, CR_SUCCESS = 0;

    [StructLayout(LayoutKind.Sequential)]
    public struct SpDevinfoData
    {
        public uint cbSize;
        public Guid ClassGuid;
        public uint DevInst;
        public nint Reserved;
    }

    [LibraryImport("setupapi.dll", EntryPoint = "SetupDiGetClassDevsW")]
    public static partial nint SetupDiGetClassDevs(in Guid classGuid, nint enumerator, nint hwndParent, uint flags);

    [LibraryImport("setupapi.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetupDiEnumDeviceInfo(nint deviceInfoSet, uint memberIndex, ref SpDevinfoData deviceInfoData);

    [LibraryImport("setupapi.dll", EntryPoint = "SetupDiGetDeviceRegistryPropertyW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetupDiGetDeviceRegistryProperty(nint deviceInfoSet, ref SpDevinfoData deviceInfoData, uint property, out uint propertyRegDataType, [Out] byte[] propertyBuffer, uint propertyBufferSize, out uint requiredSize);

    [LibraryImport("setupapi.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetupDiDestroyDeviceInfoList(nint deviceInfoSet);

    // Liefert CR_SUCCESS nur für Geräte, die gerade angeschlossen und eingeschaltet sind
    [LibraryImport("cfgmgr32.dll")]
    public static partial uint CM_Get_DevNode_Status(out uint status, out uint problemNumber, uint devInst, uint flags);
}
