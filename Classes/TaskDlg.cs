using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace ScanView.Classes;

/// <summary>TaskDialog-Helfer als moderner Ersatz für MessageBox — aus PDFlight übernommen
/// und auf ScanView zugeschnitten (ohne Mehrsprachigkeit und Updatesuche).</summary>
internal static class TaskDlg
{
    public static void MsgTaskDlg(nint hwnd, string heading, string message, TaskDialogIcon icon = null)
    {
        TaskDialog.ShowDialog(hwnd, new TaskDialogPage() { Caption = Application.ProductName, SizeToContent = true, Heading = heading, Text = message, Icon = icon ?? TaskDialogIcon.None, AllowCancel = true, Buttons = { TaskDialogButton.OK } });
    }

    /// <summary>Fehlerdialog mit fachlicher Überschrift (z.B. "Drucken fehlgeschlagen.") statt des Ausnahmetyps.</summary>
    public static void ErrTaskDlg(nint? hwnd, string heading, Exception error)
    {
        TaskDialogPage page = new()
        {
            Caption = Application.ProductName,
            Heading = heading,
            Text = error.Message,
            Icon = TaskDialogIcon.Error,
            SizeToContent = true,
            AllowCancel = true,
            Buttons = { TaskDialogButton.OK },
            Expander = new TaskDialogExpander()
            {
                Text = error.ToString(),
                CollapsedButtonText = "Technische Details anzeigen",
                ExpandedButtonText = "Details ausblenden",
                Position = TaskDialogExpanderPosition.AfterFootnote
            }
        };
        TaskDialog.ShowDialog(hwnd ?? 0, page);
    }

    /// <summary>Ja/Nein-Frage; true nur bei ausdrücklichem Ja (Abbrechen/Esc zählt als Nein).
    /// Mit defaultNo steht der Fokus auf "Nein" — für destruktive Aktionen.</summary>
    public static bool ConfirmTaskDlg(nint hwnd, string heading, string message, TaskDialogIcon icon = null, bool defaultNo = false)
    {
        TaskDialogPage page = new() { Caption = Application.ProductName, SizeToContent = true, Heading = heading, Text = message, Icon = icon ?? TaskDialogIcon.None, AllowCancel = true, Buttons = { TaskDialogButton.Yes, TaskDialogButton.No } };
        if (defaultNo) { page.DefaultButton = page.Buttons[1]; }
        return TaskDialog.ShowDialog(hwnd, page) == TaskDialogButton.Yes;
    }

    /// <summary>Über-Dialog mit Programm-Icon, Komponenten-Versionen und PayPal-Spendenlink
    /// (derselbe wie in PDFlight).</summary>
    public static void AboutTaskDlg(nint hwnd, Icon icon)
    {
        var curVersion = Assembly.GetExecutingAssembly().GetName().Version;
        var threeVersion = curVersion?.ToString(3) ?? "unbekannt";
        var buildDate = GetBuildDate();
        var msg = "ScanView scannt Seiten per WIA, ordnet sie als Miniaturen" + Environment.NewLine +
            "und speichert sie mit Texterkennung (Tesseract) als durch-" + Environment.NewLine +
            "suchbare PDF. Der Kopiermodus druckt Scans direkt — der" + Environment.NewLine +
            "Scanner wird zum Kopierer.";
        TaskDialogButton paypalButton = new TaskDialogCommandLinkButton("Anerkennung spenden via PayPal");
        using var icon32 = icon == null ? null : new Icon(icon, 32, 32); // sonst nimmt der TaskDialog die 16-px-Variante des Fenster-Icons
        var indent = new string(' ', 14);
        var foot = $"{indent}© {buildDate:yyyy} Wilhelm Happe · Version {threeVersion} ({buildDate:d})" +
            $"\n{indent}Tesseract {typeof(Tesseract.TesseractEngine).Assembly.GetName().Version?.ToString(3)}" +
            $"\n{indent}PDFsharp {typeof(PdfSharp.Pdf.PdfDocument).Assembly.GetName().Version?.ToString(3)}" +
            $"\n{indent}<a href=\"https://www.netradio.info\">www.netradio.info</a>";
        var page = new TaskDialogPage()
        {
            Caption = "Über " + Application.ProductName,
            Heading = Application.ProductName,
            Text = msg,
            Icon = icon32 == null ? null : new TaskDialogIcon(icon32),
            AllowCancel = true,
            SizeToContent = true,
            EnableLinks = true,
            Buttons = { paypalButton, TaskDialogButton.OK },
            DefaultButton = TaskDialogButton.OK,
            Footnote = foot
        };
        page.LinkClicked += (s, e) => StartLink(hwnd, e.LinkHref);
        var result = TaskDialog.ShowDialog(hwnd, page);
        if (result == paypalButton) { StartLink(hwnd, "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=S8DVXHKFC2CVS&source=url"); }
    }

    /// <summary>Alle Tastenkürzel: Kürzel, Kurztext und optionale Zusatzerklärung für die PDF-Übersicht.</summary>
    public static readonly (string Key, string Text, string Detail)[] ShortcutRows =
    [
        ("F4", "Seite scannen", null),
        ("F5", "durchsuchbare PDF speichern (mit Texterkennung)", null),
        ("F6", "alle Seiten drucken", null),
        ("F7", "Kopiermodus ein/aus",
            "Jeder Scan geht direkt an den Drucker — der Scanner wird zum Kopierer. Drucker, Exemplare und Skalierung stellst du im Kopiermodus-Bereich ein."),
        ("F9", "Seitenübersicht leeren (Neu)", null),
        ("Strg+I", "Bilddateien als Seiten importieren", null),
        ("Strg+X / C / V", "Seite ausschneiden / kopieren / einfügen", null),
        ("Entf", "markierte Seite entfernen", null),
        ("Strg+L / Strg+R", "Seite nach links / rechts drehen", null),
        ("Strg+Umschalt+R", "Seite um 180° drehen", null),
        ("Strg+D", "Rückseiten einfügen",
            "Duplex von Hand: erst alle Vorderseiten scannen, dann den gewendeten Stapel — die Rückseiten werden verzahnt einsortiert."),
        ("Strg+U", "Sortierung umkehren", null),
        ("Strg+1 … 4", "Ansicht: Optimale Breite, Ganze Seite, Zwei Seiten, Symbole", null),
        ("Strg++ / Strg+−", "Miniaturen vergrößern / verkleinern", null),
        ("Alt+← / →", "markierte Seite verschieben (auch: Ziehen mit der Maus)", null),
        ("Doppelklick", "Seite im Bildbetrachter öffnen", null),
        ("F11", "Vollbild ein/aus", null),
        ("Strg+,", "Optionen öffnen", null),
        ("2× Esc / Umschalt+Esc", "Programm beenden (Option)", null),
        ("F1", "diese Kürzel-Übersicht", null),
    ];

    /// <summary>Kürzel-Übersicht (F1 und ?-Menü): erstellt die PDF im Downloads-Ordner und öffnet sie
    /// im Standard-PDF-Programm. Existiert die Datei schon, fragt ein Dialog, ob sie geöffnet oder
    /// neu erstellt werden soll (Muster aus PDFlight).</summary>
    public static void ShowShortcutsPdf(nint hwnd, Icon icon)
    {
        var path = ShortcutsPdf.DefaultPath;
        if (File.Exists(path))
        {
            TaskDialogButton openButton = new TaskDialogCommandLinkButton("Vorhandene öffnen", path);
            TaskDialogButton recreateButton = new TaskDialogCommandLinkButton("Neu erstellen", "z.B. nach einem Update");
            using var icon32 = icon == null ? null : new Icon(icon, 32, 32); // sonst nimmt der TaskDialog die 16-px-Variante
            var page = new TaskDialogPage()
            {
                Caption = Application.ProductName,
                Heading = "Kürzel-Übersicht bereits vorhanden",
                Icon = icon32 == null ? null : new TaskDialogIcon(icon32),
                AllowCancel = true,
                SizeToContent = true,
                Buttons = { openButton, recreateButton, TaskDialogButton.Cancel },
                DefaultButton = openButton
            };
            var result = TaskDialog.ShowDialog(hwnd, page);
            if (result != openButton && result != recreateButton) { return; }
            if (result == openButton) { OpenShell(hwnd, path); return; }
        }
        try
        {
            OpenShell(hwnd, ShortcutsPdf.Create());
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or PdfSharp.PdfSharpException)
        {
            ErrTaskDlg(hwnd, "Die PDF-Übersicht konnte nicht erstellt werden.", ex);
        }
    }

    private static void OpenShell(nint hwnd, string path)
    {
        try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
        {
            ErrTaskDlg(hwnd, "Die Datei konnte nicht geöffnet werden.", ex);
        }
    }

    internal static void StartLink(nint hwnd, string url)
    {
        try
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else { MsgTaskDlg(hwnd, "Ungültiger Link!", $"'{url}' ist keine gültige URL.", TaskDialogIcon.ShieldWarningYellowBar); }
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException) { ErrTaskDlg(hwnd, "Der Link konnte nicht geöffnet werden.", ex); }
    }

    private static DateTime GetBuildDate()
    { // s. <SourceRevisionId>build$([System.DateTime]::UtcNow.ToString("yyyyMMddHHmmss"))</SourceRevisionId> in ScanView.csproj
        const string BuildVersionMetadataPrefix = "+build";
        var attribute = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attribute?.InformationalVersion != null)
        {
            var value = attribute.InformationalVersion;
            var index = value.IndexOf(BuildVersionMetadataPrefix, StringComparison.Ordinal);
            if (index > 0)
            {
                value = value[(index + BuildVersionMetadataPrefix.Length)..];
                if (DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)) { return result; }
            }
        }
        return File.GetLastWriteTime(Application.ExecutablePath);
    }
}
