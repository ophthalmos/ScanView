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
}
