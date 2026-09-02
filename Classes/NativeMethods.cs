using System.Runtime.InteropServices;

namespace ScanView.Classes;

/// <summary>Win32-Aufrufe für die Einmal-Instanz: eine registrierte Broadcast-Nachricht holt die
/// laufende Instanz in den Vordergrund (gleiche Lösung wie in NetRadio).</summary>
internal static partial class NativeMethods
{
    public const int HWND_BROADCAST = 0xffff;

    /// <summary>Systemweit registrierte Nachricht — in allen Prozessen derselbe Wert.</summary>
    public static readonly uint WM_SHOWSCANVIEW = RegisterWindowMessage("WM_SHOWSCANVIEW");

    [LibraryImport("user32.dll", EntryPoint = "RegisterWindowMessageW", StringMarshalling = StringMarshalling.Utf16)]
    public static partial uint RegisterWindowMessage(string lpString);

    [LibraryImport("user32.dll", EntryPoint = "PostMessageW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool PostMessage(nint hWnd, uint msg, nint wParam, nint lParam);
}
