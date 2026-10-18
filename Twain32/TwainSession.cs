using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ScanView.Twain32;

/// <summary>Eine TWAIN-Sitzung (TWAIN 1.x-Protokoll): unsichtbares Fenster als Eltern- und Nachrichtenfenster,
/// das der TWAIN-Manager braucht. Auflisten der Quellen oder ein Scan mit Native Transfer (DIB → TIFF).
/// Der Manager wird zur Laufzeit geladen: twain_32.dll im 32-Bit-Prozess (Twain32.exe), TWAINDSM.dll im
/// 64-Bit-Prozess (Twain64.exe, derselbe Quelltext). Zeiger und Handles sind deshalb überall IntPtr; TW_IDENTITY.Id
/// bleibt unter Windows auch in 64 Bit ein TW_UINT32 (twain.h). Keine Abhängigkeit zu ScanView.</summary>
internal sealed class TwainSession : Form, IMessageFilter
{
    // ------------------------------------------------------------------ TWAIN-Konstanten (twain.h)
    private const uint DG_CONTROL = 1, DG_IMAGE = 2;
    private const ushort DAT_CAPABILITY = 1, DAT_EVENT = 2, DAT_IDENTITY = 3, DAT_PARENT = 4, DAT_PENDINGXFERS = 5,
        DAT_STATUS = 8, DAT_USERINTERFACE = 9, DAT_IMAGELAYOUT = 0x0102, DAT_IMAGENATIVEXFER = 0x0104;
    private const ushort MSG_GET = 1, MSG_GETFIRST = 4, MSG_GETNEXT = 5, MSG_SET = 6,
        MSG_OPENDSM = 0x0301, MSG_CLOSEDSM = 0x0302, MSG_OPENDS = 0x0401, MSG_CLOSEDS = 0x0402,
        MSG_DISABLEDS = 0x0501, MSG_ENABLEDS = 0x0502, MSG_PROCESSEVENT = 0x0601, MSG_ENDXFER = 0x0701, MSG_RESET = 0x0704,
        MSG_XFERREADY = 0x0101, MSG_CLOSEDSREQ = 0x0102, MSG_CLOSEDSOK = 0x0103;
    private const ushort TWRC_SUCCESS = 0, TWRC_CHECKSTATUS = 2, TWRC_CANCEL = 3, TWRC_DSEVENT = 4, TWRC_XFERDONE = 6;
    private const ushort TWON_ONEVALUE = 5, TWTY_INT16 = 1, TWTY_UINT16 = 4, TWTY_BOOL = 6, TWTY_FIX32 = 7;
    private const ushort CAP_XFERCOUNT = 1, ICAP_PIXELTYPE = 0x0101, ICAP_UNITS = 0x0102, CAP_FEEDERENABLED = 0x1002,
        CAP_INDICATORS = 0x100B, ICAP_BRIGHTNESS = 0x1101, ICAP_XRESOLUTION = 0x1118, ICAP_YRESOLUTION = 0x1119;
    private const ushort TWPT_BW = 0, TWPT_GRAY = 1, TWPT_RGB = 2, TWUN_INCHES = 0;

    // ------------------------------------------------------------------ Strukturen (Windows: Pack = 2)
    [StructLayout(LayoutKind.Sequential, Pack = 2, CharSet = CharSet.Ansi)]
    private struct TwVersion
    {
        public ushort MajorNum, MinorNum, Language, Country;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)] public string Info;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2, CharSet = CharSet.Ansi)]
    private struct TwIdentity
    {
        public uint Id;
        public TwVersion Version;
        public ushort ProtocolMajor, ProtocolMinor;
        public uint SupportedGroups;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)] public string Manufacturer;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)] public string ProductFamily;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)] public string ProductName;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)] private struct TwUserInterface { public ushort ShowUI, ModalUI; public IntPtr Parent; }
    [StructLayout(LayoutKind.Sequential, Pack = 2)] private struct TwEvent { public IntPtr Event; public ushort Message; }
    [StructLayout(LayoutKind.Sequential, Pack = 2)] private struct TwPendingXfers { public ushort Count; public uint EndOfJob; }
    [StructLayout(LayoutKind.Sequential, Pack = 2)] private struct TwStatus { public ushort ConditionCode, Reserved; }
    [StructLayout(LayoutKind.Sequential, Pack = 2)] private struct TwCapability { public ushort Cap, ConType; public IntPtr Container; }
    [StructLayout(LayoutKind.Sequential, Pack = 2)] private struct TwFix32 { public short Whole; public ushort Frac; }
    [StructLayout(LayoutKind.Sequential, Pack = 2)] private struct TwFrame { public TwFix32 Left, Top, Right, Bottom; }
    [StructLayout(LayoutKind.Sequential, Pack = 2)] private struct TwImageLayout { public TwFrame Frame; public uint DocumentNumber, PageNumber, FrameNumber; }
    [StructLayout(LayoutKind.Sequential, Pack = 2)] private struct TwOneValue { public ushort ItemType; public uint Item; }
    [StructLayout(LayoutKind.Sequential)] private struct NativeMsg { public IntPtr Hwnd; public int Message; public IntPtr WParam, LParam; public int Time, X, Y; }

    // ------------------------------------------------------------------ TWAIN-Manager, zur Laufzeit geladen
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate ushort DsmEntryProc(IntPtr origin, IntPtr destination, uint group, ushort dataType, ushort message, IntPtr data);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern IntPtr LoadLibrary(string fileName);
    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)] private static extern IntPtr GetProcAddress(IntPtr module, string name);
    [DllImport("kernel32.dll")] private static extern bool FreeLibrary(IntPtr module);
    [DllImport("kernel32.dll")] private static extern IntPtr GlobalAlloc(uint flags, UIntPtr size);
    [DllImport("kernel32.dll")] private static extern IntPtr GlobalLock(IntPtr handle);
    [DllImport("kernel32.dll")] private static extern bool GlobalUnlock(IntPtr handle);
    [DllImport("kernel32.dll")] private static extern IntPtr GlobalFree(IntPtr handle);
    [DllImport("kernel32.dll")] private static extern UIntPtr GlobalSize(IntPtr handle);

    private readonly TwainRequest request;
    private readonly Timer watchdog = new() { Interval = 200 };
    private readonly DateTime started = DateTime.Now;
    private volatile bool cancelRequested;
    private bool transferring, showingUi;
    private static readonly TimeSpan NoImageTimeout = TimeSpan.FromMinutes(2); // ohne Treiberoberfläche: so lange höchstens auf das Bild warten
    private IntPtr dsmModule;
    private DsmEntryProc? dsm;
    private IntPtr appIdentity;    // TW_IDENTITY der Anwendung – feste Adresse, der Manager trägt die Id ein
    private IntPtr sourceIdentity; // TW_IDENTITY der geöffneten Quelle
    private bool dsmOpen, sourceOpen, sourceEnabled, finished;

    /// <summary>Zeilen für stdout (Quellenliste bzw. Hinweise).</summary>
    public List<string> Output { get; } = [];

    /// <summary>Fehlermeldung für stderr (technisch, englisch oder deutsch – ScanView zeigt sie als Detail).</summary>
    public string? Error { get; private set; }

    public int ExitCode { get; private set; } = TwainExitCode.Error;

    public TwainSession(TwainRequest request)
    {
        this.request = request;
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.None;
        Opacity = 0;
        Size = new Size(1, 1);
        StartPosition = FormStartPosition.Manual;
        Location = new Point(-32000, -32000);
        Text = "ScanView TWAIN";
        watchdog.Tick += Watchdog_Tick;
    }

    /// <summary>ScanView schreibt „cancel" auf die Standardeingabe – der Helfer darf nicht hart beendet werden:
    /// ein mitten im Scan beendeter Treiber hält den Scanner fest, bis er aus- und wieder eingeschaltet wird.</summary>
    private void ListenForCancel()
    {
        System.Threading.Thread reader = new(() =>
        {
            try
            {
                string? line;
                while ((line = System.Console.In.ReadLine()) != null)
                {
                    if (string.Equals(line.Trim(), "cancel", StringComparison.OrdinalIgnoreCase)) { cancelRequested = true; }
                }
            }
            catch (IOException) { } // keine Standardeingabe – dann eben ohne Abbruchwunsch
        }) { IsBackground = true };
        reader.Start();
    }

    /// <summary>Abbruchwunsch und Zeitlimit prüfen – nur zwischen den TWAIN-Aufrufen, nie während der Übertragung.</summary>
    private void Watchdog_Tick(object? sender, EventArgs e)
    {
        if (finished || transferring) { return; }
        if (cancelRequested) { Finish(TwainExitCode.Cancelled); return; }
        if (!showingUi && DateTime.Now - started > NoImageTimeout)
        {
            Error = "Der Scanner hat innerhalb von zwei Minuten kein Bild geliefert.";
            Finish(TwainExitCode.Error);
        }
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        try
        {
            if (!OpenManager()) { Finish(TwainExitCode.NoTwain); return; }
            if (request.List) { ListSources(); Finish(TwainExitCode.Success); }
            else { StartScan(); }
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            Finish(TwainExitCode.Error);
        }
    }

    private bool OpenManager()
    {
        var name = IntPtr.Size == 8 ? "TWAINDSM.dll" : "twain_32.dll";
        dsmModule = LoadLibrary(name);
        if (dsmModule == IntPtr.Zero) { Error = name + " konnte nicht geladen werden."; return false; }
        var entry = GetProcAddress(dsmModule, "DSM_Entry");
        if (entry == IntPtr.Zero) { Error = name + ": DSM_Entry fehlt."; return false; }
        dsm = (DsmEntryProc)Marshal.GetDelegateForFunctionPointer(entry, typeof(DsmEntryProc));
        appIdentity = Allocate(new TwIdentity
        {
            Version = new TwVersion { MajorNum = 1, MinorNum = 0, Language = 13, Country = 49, Info = "ScanView" },
            ProtocolMajor = 1,
            ProtocolMinor = 9,
            SupportedGroups = DG_CONTROL | DG_IMAGE,
            Manufacturer = "ophthalmos",
            ProductFamily = "ScanView",
            ProductName = "ScanView",
        });
        var handle = Marshal.AllocHGlobal(IntPtr.Size);
        try
        {
            Marshal.WriteIntPtr(handle, Handle);
            var rc = dsm(appIdentity, IntPtr.Zero, DG_CONTROL, DAT_PARENT, MSG_OPENDSM, handle);
            if (rc != TWRC_SUCCESS) { Error = "TWAIN-Manager: OPENDSM fehlgeschlagen (" + rc + ")."; return false; }
        }
        finally { Marshal.FreeHGlobal(handle); }
        dsmOpen = true;
        return true;
    }

    /// <summary>Alle Quellen außer den WIA-Brücken von Windows („Twain Data Source On WIA") – die leiten nur auf WIA weiter.</summary>
    private List<TwIdentity> EnumerateSources()
    {
        List<TwIdentity> sources = [];
        var buffer = Allocate(new TwIdentity());
        try
        {
            for (var rc = dsm!(appIdentity, IntPtr.Zero, DG_CONTROL, DAT_IDENTITY, MSG_GETFIRST, buffer); rc == TWRC_SUCCESS;
                 rc = dsm(appIdentity, IntPtr.Zero, DG_CONTROL, DAT_IDENTITY, MSG_GETNEXT, buffer))
            {
                var identity = Marshal.PtrToStructure<TwIdentity>(buffer);
                var wiaBridge = (identity.ProductFamily ?? "").IndexOf("On WIA", StringComparison.OrdinalIgnoreCase) >= 0
                    || (identity.ProductName ?? "").StartsWith("WIA-", StringComparison.OrdinalIgnoreCase);
                if (!wiaBridge) { sources.Add(identity); }
            }
        }
        finally { Marshal.FreeHGlobal(buffer); }
        return sources;
    }

    private void ListSources()
    {
        foreach (var source in EnumerateSources()) { Output.Add(source.ProductName + "\t" + source.Manufacturer); }
    }

    private void StartScan()
    {
        var sources = EnumerateSources();
        var index = sources.FindIndex(s => string.Equals(s.ProductName, request.Source, StringComparison.OrdinalIgnoreCase));
        if (index < 0) { Error = "TWAIN-Quelle nicht gefunden: " + request.Source; Finish(TwainExitCode.SourceNotFound); return; }
        sourceIdentity = Allocate(sources[index]);
        var rc = dsm!(appIdentity, IntPtr.Zero, DG_CONTROL, DAT_IDENTITY, MSG_OPENDS, sourceIdentity);
        if (rc != TWRC_SUCCESS) { Error = "Die TWAIN-Quelle ließ sich nicht öffnen (" + rc + ", Bedingung " + Condition() + ")."; Finish(TwainExitCode.Error); return; }
        sourceOpen = true;

        // Einstellungen – jede einzeln; was der Treiber nicht kennt, bleibt auf seinem Standard
        SetOneValue(CAP_XFERCOUNT, TWTY_INT16, 1);
        SetOneValue(ICAP_UNITS, TWTY_UINT16, TWUN_INCHES);
        SetOneValue(ICAP_PIXELTYPE, TWTY_UINT16, request.Mode switch { TwainMode.Gray => TWPT_GRAY, TwainMode.BlackWhite => TWPT_BW, _ => TWPT_RGB });
        SetOneValue(ICAP_XRESOLUTION, TWTY_FIX32, ToFix32Bits(request.Dpi));
        SetOneValue(ICAP_YRESOLUTION, TWTY_FIX32, ToFix32Bits(request.Dpi));
        SetOneValue(ICAP_BRIGHTNESS, TWTY_FIX32, ToFix32Bits(request.Brightness * 10)); // TWAIN: −1000 … +1000
        SetOneValue(CAP_FEEDERENABLED, TWTY_BOOL, request.Feeder ? 1u : 0u);
        SetOneValue(CAP_INDICATORS, TWTY_BOOL, 0); // keine Fortschrittsanzeige des Treibers – ScanView zeigt eine eigene
        if (request.AreaWidthMm is { } width && request.AreaHeightMm is { } height)
        {
            var layout = Allocate(new TwImageLayout
            {
                Frame = new TwFrame { Left = ToFix32(0), Top = ToFix32(0), Right = ToFix32(width / 25.4), Bottom = ToFix32(height / 25.4) },
                DocumentNumber = 1,
                PageNumber = 1,
                FrameNumber = 1,
            });
            try { dsm(appIdentity, sourceIdentity, DG_IMAGE, DAT_IMAGELAYOUT, MSG_SET, layout); }
            finally { Marshal.FreeHGlobal(layout); }
        }

        Application.AddMessageFilter(this);
        ListenForCancel();
        watchdog.Start();
        // Erst ohne Treiberoberfläche; lehnt der Treiber das ab, mit seiner Oberfläche (dann scannt der Benutzer dort)
        if (!EnableSource(showUi: false) && !EnableSource(showUi: true))
        {
            Error = "Die TWAIN-Quelle ließ sich nicht starten (Bedingung " + Condition() + ").";
            Finish(TwainExitCode.Error);
        }
    }

    private bool EnableSource(bool showUi)
    {
        var ui = Allocate(new TwUserInterface { ShowUI = (ushort)(showUi ? 1 : 0), ModalUI = 0, Parent = Handle });
        try
        {
            var rc = dsm!(appIdentity, sourceIdentity, DG_CONTROL, DAT_USERINTERFACE, MSG_ENABLEDS, ui);
            if (rc != TWRC_SUCCESS && rc != TWRC_CHECKSTATUS) { return false; }
            sourceEnabled = true;
            showingUi = showUi;
            if (showUi) { Output.Add("ui"); } // Hinweis für ScanView: der Treiber zeigt seine Oberfläche
            return true;
        }
        finally { Marshal.FreeHGlobal(ui); }
    }

    /// <summary>Solange die Quelle aktiv ist, gehen alle Fensternachrichten zuerst an sie (TWAIN 1.x).</summary>
    public bool PreFilterMessage(ref Message m)
    {
        if (!sourceEnabled || finished) { return false; }
        var nativeMsg = Allocate(new NativeMsg { Hwnd = m.HWnd, Message = m.Msg, WParam = m.WParam, LParam = m.LParam });
        var twEvent = Allocate(new TwEvent { Event = nativeMsg });
        try
        {
            var rc = dsm!(appIdentity, sourceIdentity, DG_CONTROL, DAT_EVENT, MSG_PROCESSEVENT, twEvent);
            var message = Marshal.PtrToStructure<TwEvent>(twEvent).Message;
            if (message == MSG_XFERREADY) { BeginInvoke(new Action(Transfer)); }
            else if (message == MSG_CLOSEDSREQ || message == MSG_CLOSEDSOK) { BeginInvoke(new Action(() => Finish(TwainExitCode.Cancelled))); }
            return rc == TWRC_DSEVENT;
        }
        finally
        {
            Marshal.FreeHGlobal(twEvent);
            Marshal.FreeHGlobal(nativeMsg);
        }
    }

    private void Transfer()
    {
        if (finished) { return; }
        var handleBuffer = Marshal.AllocHGlobal(IntPtr.Size);
        Marshal.WriteIntPtr(handleBuffer, IntPtr.Zero);
        ushort rc;
        IntPtr dib;
        transferring = true; // die Übertragung blockiert und lässt sich nicht unterbrechen – ein Abbruch greift danach
        try
        {
            rc = dsm!(appIdentity, sourceIdentity, DG_IMAGE, DAT_IMAGENATIVEXFER, MSG_GET, handleBuffer);
            dib = Marshal.ReadIntPtr(handleBuffer);
        }
        finally
        {
            Marshal.FreeHGlobal(handleBuffer);
            transferring = false;
        }
        EndTransfers();
        if (rc == TWRC_CANCEL || cancelRequested)
        {
            if (dib != IntPtr.Zero) { GlobalFree(dib); }
            Finish(TwainExitCode.Cancelled);
            return;
        }
        if (rc != TWRC_XFERDONE || dib == IntPtr.Zero)
        {
            Error = "Die Bildübertragung ist fehlgeschlagen (" + rc + ", Bedingung " + Condition() + ").";
            Finish(TwainExitCode.Error);
            return;
        }
        try
        {
            SaveDibAsTiff(dib, request.OutputPath, request.Dpi);
            Finish(TwainExitCode.Success);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or ExternalException)
        {
            Error = "Das Bild konnte nicht gespeichert werden: " + ex.Message;
            Finish(TwainExitCode.Error);
        }
        finally { GlobalFree(dib); }
    }

    /// <summary>Beendet die Übertragung; noch wartende Seiten (Einzug) werden verworfen – ScanView scannt seitenweise.</summary>
    private void EndTransfers()
    {
        var pending = Allocate(new TwPendingXfers());
        try
        {
            dsm!(appIdentity, sourceIdentity, DG_CONTROL, DAT_PENDINGXFERS, MSG_ENDXFER, pending);
            if (Marshal.PtrToStructure<TwPendingXfers>(pending).Count != 0)
            {
                dsm(appIdentity, sourceIdentity, DG_CONTROL, DAT_PENDINGXFERS, MSG_RESET, pending);
            }
        }
        finally { Marshal.FreeHGlobal(pending); }
    }

    /// <summary>Native Transfer liefert ein DIB (BITMAPINFOHEADER, ggf. Palette, Bildpunkte) als globalen Speicher:
    /// Dateikopf ergänzen und direkt als TIFF speichern – das Pixelformat bleibt (Graustufen 8 Bit, Schwarz-weiß 1 Bit).</summary>
    private static void SaveDibAsTiff(IntPtr dib, string path, int fallbackDpi)
    {
        var size = (int)(ulong)GlobalSize(dib);
        var bytes = new byte[size];
        Marshal.Copy(GlobalLock(dib), bytes, 0, size);
        GlobalUnlock(dib);
        var headerSize = BitConverter.ToInt32(bytes, 0);
        int bitCount = BitConverter.ToInt16(bytes, 14);
        var compression = BitConverter.ToInt32(bytes, 16);
        var xPelsPerMeter = BitConverter.ToInt32(bytes, 24);
        var colorsUsed = BitConverter.ToInt32(bytes, 32);
        var paletteEntries = colorsUsed != 0 ? colorsUsed : bitCount <= 8 ? 1 << bitCount : 0;
        var pixelOffset = headerSize + paletteEntries * 4 + (compression == 3 && headerSize == 40 ? 12 : 0);
        var file = new byte[14 + size];
        file[0] = (byte)'B';
        file[1] = (byte)'M';
        BitConverter.GetBytes(file.Length).CopyTo(file, 2);
        BitConverter.GetBytes(14 + pixelOffset).CopyTo(file, 10);
        bytes.CopyTo(file, 14);
        using MemoryStream stream = new(file);
        using var image = (Bitmap)Image.FromStream(stream);
        var dpi = xPelsPerMeter > 0 ? (float)Math.Round(xPelsPerMeter * 0.0254) : fallbackDpi;
        image.SetResolution(dpi, dpi);
        if (File.Exists(path)) { File.Delete(path); }
        image.Save(path, ImageFormat.Tiff);
    }

    private void SetOneValue(ushort capability, ushort itemType, uint value)
    {
        var container = GlobalAlloc(0x42, (UIntPtr)Marshal.SizeOf<TwOneValue>()); // GHND
        Marshal.StructureToPtr(new TwOneValue { ItemType = itemType, Item = value }, GlobalLock(container), false);
        GlobalUnlock(container);
        var cap = Allocate(new TwCapability { Cap = capability, ConType = TWON_ONEVALUE, Container = container });
        try { dsm!(appIdentity, sourceIdentity, DG_CONTROL, DAT_CAPABILITY, MSG_SET, cap); }
        finally
        {
            Marshal.FreeHGlobal(cap);
            GlobalFree(container);
        }
    }

    private ushort Condition()
    {
        var status = Allocate(new TwStatus());
        try
        {
            dsm!(appIdentity, sourceOpen ? sourceIdentity : IntPtr.Zero, DG_CONTROL, DAT_STATUS, MSG_GET, status);
            return Marshal.PtrToStructure<TwStatus>(status).ConditionCode;
        }
        finally { Marshal.FreeHGlobal(status); }
    }

    private static TwFix32 ToFix32(double value)
    {
        var whole = (int)Math.Floor(value);
        return new TwFix32 { Whole = (short)whole, Frac = (ushort)Math.Round((value - whole) * 65536) };
    }

    private static uint ToFix32Bits(double value)
    {
        var fix = ToFix32(value);
        return (ushort)fix.Whole | ((uint)fix.Frac << 16);
    }

    private static IntPtr Allocate<T>(T value) where T : struct
    {
        var pointer = Marshal.AllocHGlobal(Marshal.SizeOf<T>());
        Marshal.StructureToPtr(value, pointer, false);
        return pointer;
    }

    /// <summary>Sitzung geordnet zurückfahren (Quelle deaktivieren und schließen, Manager schließen) und das Fenster schließen.</summary>
    private void Finish(int exitCode)
    {
        if (finished) { return; }
        finished = true;
        ExitCode = exitCode;
        watchdog.Stop();
        Application.RemoveMessageFilter(this);
        if (sourceEnabled)
        {
            var ui = Allocate(new TwUserInterface { Parent = Handle });
            try { dsm!(appIdentity, sourceIdentity, DG_CONTROL, DAT_USERINTERFACE, MSG_DISABLEDS, ui); }
            finally { Marshal.FreeHGlobal(ui); }
            sourceEnabled = false;
        }
        if (sourceOpen)
        {
            dsm!(appIdentity, IntPtr.Zero, DG_CONTROL, DAT_IDENTITY, MSG_CLOSEDS, sourceIdentity);
            sourceOpen = false;
        }
        if (dsmOpen)
        {
            var handle = Marshal.AllocHGlobal(IntPtr.Size);
            Marshal.WriteIntPtr(handle, Handle);
            try { dsm!(appIdentity, IntPtr.Zero, DG_CONTROL, DAT_PARENT, MSG_CLOSEDSM, handle); }
            finally { Marshal.FreeHGlobal(handle); }
            dsmOpen = false;
        }
        BeginInvoke(new Action(Close));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { watchdog.Dispose(); }
        if (sourceIdentity != IntPtr.Zero) { Marshal.FreeHGlobal(sourceIdentity); sourceIdentity = IntPtr.Zero; }
        if (appIdentity != IntPtr.Zero) { Marshal.FreeHGlobal(appIdentity); appIdentity = IntPtr.Zero; }
        if (dsmModule != IntPtr.Zero) { FreeLibrary(dsmModule); dsmModule = IntPtr.Zero; }
        base.Dispose(disposing);
    }
}
