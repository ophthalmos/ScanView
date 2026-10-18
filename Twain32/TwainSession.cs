using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ScanView.Twain32;

/// <summary>Eine TWAIN-Sitzung (TWAIN 1.x-Protokoll): unsichtbares Fenster als Eltern- und Nachrichtenfenster,
/// das der TWAIN-Manager braucht. Auflisten der Quellen oder ein Scan – in Streifen mit Fortschritt
/// (Speicherübertragung) oder in einem Stück (Native Transfer, DIB → TIFF).
/// Der Manager wird zur Laufzeit geladen: TWAINDSM.dll, im 32-Bit-Prozess (Twain32.exe) ersatzweise twain_32.dll;
/// Twain64.exe ist derselbe Quelltext für 64 Bit. Zeiger und Handles sind deshalb überall IntPtr; TW_IDENTITY.Id
/// bleibt unter Windows auch in 64 Bit ein TW_UINT32 (twain.h). Keine Abhängigkeit zu ScanView.</summary>
internal sealed class TwainSession : Form, IMessageFilter
{
    // ------------------------------------------------------------------ TWAIN-Konstanten (twain.h)
    private const uint DG_CONTROL = 1, DG_IMAGE = 2;
    private const ushort DAT_CAPABILITY = 1, DAT_EVENT = 2, DAT_IDENTITY = 3, DAT_PARENT = 4, DAT_PENDINGXFERS = 5,
        DAT_SETUPMEMXFER = 6, DAT_STATUS = 8, DAT_USERINTERFACE = 9,
        DAT_IMAGEINFO = 0x0101, DAT_IMAGELAYOUT = 0x0102, DAT_IMAGEMEMXFER = 0x0103, DAT_IMAGENATIVEXFER = 0x0104;
    private const ushort MSG_GET = 1, MSG_GETCURRENT = 3, MSG_GETFIRST = 4, MSG_GETNEXT = 5, MSG_SET = 6,
        MSG_OPENDSM = 0x0301, MSG_CLOSEDSM = 0x0302, MSG_OPENDS = 0x0401, MSG_CLOSEDS = 0x0402,
        MSG_DISABLEDS = 0x0501, MSG_ENABLEDS = 0x0502, MSG_PROCESSEVENT = 0x0601, MSG_ENDXFER = 0x0701, MSG_RESET = 0x0704,
        MSG_XFERREADY = 0x0101, MSG_CLOSEDSREQ = 0x0102, MSG_CLOSEDSOK = 0x0103;
    private const ushort TWRC_SUCCESS = 0, TWRC_CHECKSTATUS = 2, TWRC_CANCEL = 3, TWRC_DSEVENT = 4, TWRC_XFERDONE = 6;
    private const ushort TWON_ONEVALUE = 5, TWON_DONTCARE16 = 0xFFFF, TWTY_INT16 = 1, TWTY_UINT16 = 4, TWTY_BOOL = 6, TWTY_FIX32 = 7;
    private const ushort CAP_XFERCOUNT = 1, ICAP_COMPRESSION = 0x0100, ICAP_PIXELTYPE = 0x0101, ICAP_UNITS = 0x0102,
        ICAP_XFERMECH = 0x0103, CAP_FEEDERENABLED = 0x1002, CAP_INDICATORS = 0x100B, ICAP_BRIGHTNESS = 0x1101,
        ICAP_XRESOLUTION = 0x1118, ICAP_YRESOLUTION = 0x1119, ICAP_PIXELFLAVOR = 0x111F, ICAP_BITDEPTH = 0x112B;
    private const ushort TWPT_BW = 0, TWPT_GRAY = 1, TWPT_RGB = 2, TWUN_INCHES = 0, TWSX_MEMORY = 2, TWCP_NONE = 0, TWPF_VANILLA = 1;
    private const uint TWMF_APPOWNS = 0x1, TWMF_POINTER = 0x8, TWON_DONTCARE32 = 0xFFFFFFFF;

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
    [StructLayout(LayoutKind.Sequential, Pack = 2)] private struct TwSetupMemXfer { public uint MinBufSize, MaxBufSize, Preferred; }
    [StructLayout(LayoutKind.Sequential, Pack = 2)] private struct TwMemory { public uint Flags, Length; public IntPtr TheMem; }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    private struct TwImageMemXfer
    {
        public ushort Compression;
        public uint BytesPerRow, Columns, Rows, XOffset, YOffset, BytesWritten;
        public TwMemory Memory;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    private struct TwImageInfo
    {
        public TwFix32 XResolution, YResolution;
        public int ImageWidth, ImageLength; // ImageLength −1: Höhe vorab unbekannt (etwa beim Einzug)
        public short SamplesPerPixel;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public short[] BitsPerSample;
        public short BitsPerPixel;
        public ushort Planar;
        public short PixelType;
        public ushort Compression;
    }

    /// <summary>Ein Streifen der Speicherübertragung: Zeilen ab YOffset, je BytesPerRow Bytes.</summary>
    private sealed class Strip
    {
        public uint XOffset, YOffset, Columns, Rows, BytesPerRow;
        public byte[] Data = [];
    }
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
    private bool newManager;     // TWAINDSM.dll statt des alten twain_32.dll
    private bool memoryTransfer; // Bild in Streifen holen (mit Fortschritt) statt in einem Stück (Native Transfer)
    private bool vanilla;        // ICAP_PIXELFLAVOR: 0 = Weiß statt 0 = Schwarz

    /// <summary>Zeilen für stdout (Quellenliste bzw. Hinweise).</summary>
    public List<string> Output { get; } = [];

    /// <summary>Fortschritt der Speicherübertragung in Prozent, sofort weiterzumelden (nur bei bekannter Bildhöhe).</summary>
    public Action<int>? Progress { get; set; }

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

    /// <summary>Lädt den TWAIN-Manager: in 64 Bit TWAINDSM.dll, in 32 Bit bevorzugt ebenfalls TWAINDSM.dll (das Setup
    /// installiert ihn), sonst den alten Manager twain_32.dll von Windows.</summary>
    private bool OpenManager()
    {
        string[] candidates = IntPtr.Size == 8 ? ["TWAINDSM.dll"] : ["TWAINDSM.dll", "twain_32.dll"];
        var name = "";
        foreach (var candidate in candidates)
        {
            dsmModule = LoadLibrary(candidate);
            if (dsmModule != IntPtr.Zero) { name = candidate; break; }
        }
        if (dsmModule == IntPtr.Zero) { Error = string.Join(" / ", candidates) + " konnte nicht geladen werden."; return false; }
        newManager = name == "TWAINDSM.dll";
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

        // Einstellungen – jede einzeln; was der Treiber nicht kennt, bleibt auf seinem Standard. Die Übertragungsart
        // zuerst: Speicherübertragung liefert das Bild in Streifen und damit einen Fortschritt; lehnt der Treiber sie
        // ab, bleibt es beim Native Transfer (ein Stück, ohne Fortschritt). Nur mit TWAINDSM.dll: Über den alten
        // Manager twain_32.dll kostet jeder Aufruf spürbar Zeit (Epson V30, 300 dpi A4: 20,4 statt 15,8 s,
        // Beispieltreiber der TWAIN Working Group bei 600 dpi 22 statt 1 s) – ein Fortschritt darf den Scan nicht bremsen.
        memoryTransfer = newManager && !request.Native && SetOneValue(ICAP_XFERMECH, TWTY_UINT16, TWSX_MEMORY);
        if (memoryTransfer) { SetOneValue(ICAP_COMPRESSION, TWTY_UINT16, TWCP_NONE); }
        SetOneValue(CAP_XFERCOUNT, TWTY_INT16, 1);
        SetOneValue(ICAP_UNITS, TWTY_UINT16, TWUN_INCHES);
        SetOneValue(ICAP_PIXELTYPE, TWTY_UINT16, request.Mode switch { TwainMode.Gray => TWPT_GRAY, TwainMode.BlackWhite => TWPT_BW, _ => TWPT_RGB });
        if (memoryTransfer)
        {
            SetOneValue(ICAP_BITDEPTH, TWTY_UINT16, request.Mode == TwainMode.BlackWhite ? 1u : 8u); // je Farbkanal
            vanilla = GetCurrentValue(ICAP_PIXELFLAVOR) == TWPF_VANILLA;
        }
        Progress?.Invoke(memoryTransfer ? 0 : -1); // gleich sagen, ob ein Fortschritt kommt – sonst zeigt ScanView bis zum ersten Streifen einen laufenden Balken
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
        if (memoryTransfer) { MemoryTransfer(); }
        else { NativeTransfer(); }
    }

    /// <summary>Speicherübertragung: der Treiber füllt einen Puffer Streifen für Streifen; nach jedem Streifen wird
    /// der Fortschritt gemeldet und ein Abbruchwunsch geprüft (TWAIN erlaubt MSG_ENDXFER mitten in der Übertragung).
    /// Scheitert sie, bevor Bilddaten kamen (Format nicht unterstützt, Treiberfehler), endet der Helfer mit
    /// MemoryTransferFailed – ScanView scannt dann noch einmal mit Native Transfer.</summary>
    private void MemoryTransfer()
    {
        List<Strip> strips = [];
        string? failure = null;
        ushort rc = TWRC_SUCCESS;
        var infoBuffer = Marshal.AllocHGlobal(Marshal.SizeOf<TwImageInfo>());
        var setupBuffer = Allocate(new TwSetupMemXfer());
        var xferBuffer = Marshal.AllocHGlobal(Marshal.SizeOf<TwImageMemXfer>());
        var dataBuffer = IntPtr.Zero;
        var info = new TwImageInfo();
        transferring = true;
        try
        {
            rc = dsm!(appIdentity, sourceIdentity, DG_IMAGE, DAT_IMAGEINFO, MSG_GET, infoBuffer);
            if (rc != TWRC_SUCCESS) { failure = "IMAGEINFO " + rc + ", Bedingung " + Condition(); }
            else
            {
                info = Marshal.PtrToStructure<TwImageInfo>(infoBuffer);
                if (info.ImageLength <= 0) { Progress?.Invoke(-1); } // Höhe vorab unbekannt: kein Prozentwert möglich
                if (info.Compression != TWCP_NONE || info.Planar != 0 || info.BitsPerPixel is not (1 or 8 or 24))
                {
                    failure = "Bildformat nicht unterstützt (" + info.BitsPerPixel + " Bit, Kompression " + info.Compression + ", planar " + info.Planar + ")";
                }
            }
            if (failure == null)
            {
                rc = dsm(appIdentity, sourceIdentity, DG_CONTROL, DAT_SETUPMEMXFER, MSG_GET, setupBuffer);
                var setup = Marshal.PtrToStructure<TwSetupMemXfer>(setupBuffer);
                // Puffer möglichst 1 MB (innerhalb der Grenzen des Treibers) statt des oft nur wenige Zeilen großen
                // Vorschlags: weniger Aufrufe, und der Fortschritt kommt trotzdem in feinen Schritten
                var size = 1024u * 1024;
                if (rc == TWRC_SUCCESS)
                {
                    if (setup.Preferred is not (0 or TWON_DONTCARE32)) { size = Math.Max(size, setup.Preferred); }
                    if (setup.MaxBufSize is not (0 or TWON_DONTCARE32)) { size = Math.Min(size, setup.MaxBufSize); }
                    size = Math.Max(size, setup.MinBufSize);
                }
                size = Math.Min(size, 16u * 1024 * 1024);
                dataBuffer = Marshal.AllocHGlobal((int)size);
                var lastPercent = -1;
                while (true)
                {
                    Marshal.StructureToPtr(new TwImageMemXfer
                    {
                        Compression = TWON_DONTCARE16,
                        BytesPerRow = TWON_DONTCARE32, Columns = TWON_DONTCARE32, Rows = TWON_DONTCARE32,
                        XOffset = TWON_DONTCARE32, YOffset = TWON_DONTCARE32, BytesWritten = TWON_DONTCARE32,
                        Memory = new TwMemory { Flags = TWMF_APPOWNS | TWMF_POINTER, Length = size, TheMem = dataBuffer },
                    }, xferBuffer, false);
                    rc = dsm(appIdentity, sourceIdentity, DG_IMAGE, DAT_IMAGEMEMXFER, MSG_GET, xferBuffer);
                    if (rc != TWRC_SUCCESS && rc != TWRC_XFERDONE) { break; }
                    var block = Marshal.PtrToStructure<TwImageMemXfer>(xferBuffer);
                    var data = new byte[Math.Min(block.BytesWritten, size)];
                    Marshal.Copy(dataBuffer, data, 0, data.Length);
                    strips.Add(new Strip { XOffset = block.XOffset, YOffset = block.YOffset, Columns = block.Columns, Rows = block.Rows, BytesPerRow = block.BytesPerRow, Data = data });
                    if (info.ImageLength > 0)
                    {
                        var percent = (int)Math.Min(100, (block.YOffset + (long)block.Rows) * 100 / info.ImageLength);
                        if (percent > lastPercent) { lastPercent = percent; Progress?.Invoke(percent); }
                    }
                    if (rc == TWRC_XFERDONE || cancelRequested) { break; }
                }
                if (rc != TWRC_SUCCESS && rc != TWRC_XFERDONE && rc != TWRC_CANCEL) { failure = "IMAGEMEMXFER " + rc + ", Bedingung " + Condition(); }
            }
        }
        finally
        {
            transferring = false;
            Marshal.FreeHGlobal(infoBuffer);
            Marshal.FreeHGlobal(setupBuffer);
            Marshal.FreeHGlobal(xferBuffer);
            if (dataBuffer != IntPtr.Zero) { Marshal.FreeHGlobal(dataBuffer); }
        }
        EndTransfers();
        if (rc == TWRC_CANCEL || cancelRequested) { Finish(TwainExitCode.Cancelled); return; }
        if (failure != null)
        {
            Error = "Speicherübertragung fehlgeschlagen: " + failure + ".";
            Finish(strips.Count == 0 ? TwainExitCode.MemoryTransferFailed : TwainExitCode.Error);
            return;
        }
        try
        {
            SaveStripsAsTiff(info, strips, request.OutputPath, request.Dpi, vanilla);
            Finish(TwainExitCode.Success);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or ExternalException)
        {
            Error = "Das Bild konnte nicht gespeichert werden: " + ex.Message;
            Finish(TwainExitCode.Error);
        }
    }

    /// <summary>Setzt die Streifen zu einem Bild zusammen und speichert es als TIFF. TWAIN liefert die Zeilen von oben
    /// nach unten, Farbe in der Reihenfolge R, G, B (GDI+ erwartet B, G, R); 1 Bit mit dem höchsten Bit zuerst wie GDI+.
    /// Bei „vanilla" steht 0 für Weiß – die Palette wird dann umgedreht statt der Bilddaten.</summary>
    private static void SaveStripsAsTiff(TwImageInfo info, List<Strip> strips, string path, int fallbackDpi, bool vanilla)
    {
        if (strips.Count == 0) { throw new ArgumentException("Der Treiber hat keine Bilddaten geliefert."); }
        int bitsPerPixel = info.BitsPerPixel;
        var width = info.ImageWidth > 0 ? info.ImageWidth : (int)strips.Max(s => s.XOffset + s.Columns);
        var height = (int)strips.Max(s => s.YOffset + s.Rows); // tatsächlich geliefert – auch bei vorab unbekannter Höhe
        var format = bitsPerPixel switch { 1 => PixelFormat.Format1bppIndexed, 8 => PixelFormat.Format8bppIndexed, _ => PixelFormat.Format24bppRgb };
        using Bitmap bitmap = new(width, height, format);
        if (bitsPerPixel != 24)
        {
            var palette = bitmap.Palette;
            var levels = palette.Entries.Length;
            for (var i = 0; i < levels; i++)
            {
                var gray = i * 255 / (levels - 1);
                if (vanilla) { gray = 255 - gray; }
                palette.Entries[i] = Color.FromArgb(gray, gray, gray);
            }
            bitmap.Palette = palette;
        }
        var bits = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, format);
        try
        {
            foreach (var strip in strips)
            {
                var xByte = (int)(strip.XOffset * bitsPerPixel / 8);
                var rowBytes = (int)Math.Min(strip.BytesPerRow, ((long)strip.Columns * bitsPerPixel + 7) / 8);
                rowBytes = Math.Min(rowBytes, bits.Stride - xByte);
                for (var row = 0; row < strip.Rows; row++)
                {
                    var y = (int)strip.YOffset + row;
                    var source = row * (int)strip.BytesPerRow;
                    if (y >= height || rowBytes <= 0 || source + rowBytes > strip.Data.Length) { continue; }
                    if (bitsPerPixel == 24)
                    {
                        for (var x = source; x + 2 < source + rowBytes; x += 3) { (strip.Data[x], strip.Data[x + 2]) = (strip.Data[x + 2], strip.Data[x]); }
                    }
                    Marshal.Copy(strip.Data, source, IntPtr.Add(bits.Scan0, y * bits.Stride + xByte), rowBytes);
                }
            }
        }
        finally { bitmap.UnlockBits(bits); }
        var resolution = info.XResolution.Whole + info.XResolution.Frac / 65536f;
        var dpi = resolution > 0 ? (float)Math.Round(resolution) : fallbackDpi;
        bitmap.SetResolution(dpi, dpi);
        if (File.Exists(path)) { File.Delete(path); }
        bitmap.Save(path, ImageFormat.Tiff);
    }

    private void NativeTransfer()
    {
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

    /// <summary>Setzt eine Einstellung; true nur, wenn der Treiber sie genau so übernommen hat.</summary>
    private bool SetOneValue(ushort capability, ushort itemType, uint value)
    {
        var container = GlobalAlloc(0x42, (UIntPtr)Marshal.SizeOf<TwOneValue>()); // GHND
        Marshal.StructureToPtr(new TwOneValue { ItemType = itemType, Item = value }, GlobalLock(container), false);
        GlobalUnlock(container);
        var cap = Allocate(new TwCapability { Cap = capability, ConType = TWON_ONEVALUE, Container = container });
        try { return dsm!(appIdentity, sourceIdentity, DG_CONTROL, DAT_CAPABILITY, MSG_SET, cap) == TWRC_SUCCESS; }
        finally
        {
            Marshal.FreeHGlobal(cap);
            GlobalFree(container);
        }
    }

    /// <summary>Aktueller Wert einer Einstellung (nur Einzelwerte), null wenn der Treiber ihn nicht liefert.</summary>
    private uint? GetCurrentValue(ushort capability)
    {
        var cap = Allocate(new TwCapability { Cap = capability, ConType = TWON_DONTCARE16 });
        try
        {
            if (dsm!(appIdentity, sourceIdentity, DG_CONTROL, DAT_CAPABILITY, MSG_GETCURRENT, cap) != TWRC_SUCCESS) { return null; }
            var result = Marshal.PtrToStructure<TwCapability>(cap);
            if (result.Container == IntPtr.Zero) { return null; }
            try
            {
                if (result.ConType != TWON_ONEVALUE) { return null; }
                var value = Marshal.PtrToStructure<TwOneValue>(GlobalLock(result.Container));
                return value.ItemType is TWTY_INT16 or TWTY_UINT16 or TWTY_BOOL ? value.Item & 0xFFFF : value.Item;
            }
            finally
            {
                GlobalUnlock(result.Container);
                GlobalFree(result.Container); // den Container legt der Treiber an, freigeben muss die Anwendung
            }
        }
        finally { Marshal.FreeHGlobal(cap); }
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
