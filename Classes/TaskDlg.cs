using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Reflection;

namespace ScanView.Classes;

/// <summary>TaskDialog-Helfer als moderner Ersatz für MessageBox — aus PDFlight übernommen
/// und auf ScanView zugeschnitten.</summary>
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
                CollapsedButtonText = Lng.T("Technische Details anzeigen"),
                ExpandedButtonText = Lng.T("Details ausblenden"),
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
        var threeVersion = curVersion?.ToString(3) ?? Lng.T("unbekannt");
        var buildDate = GetBuildDate();
        var msg = Lng.T("About.Text",
            "ScanView scannt Seiten, ordnet sie als Miniaturen und" + Environment.NewLine +
            "speichert sie mit Texterkennung als durchsuchbare PDF." + Environment.NewLine + Environment.NewLine +
            "Im Kopiermodus wird jeder Scan direkt gedruckt." + Environment.NewLine + Environment.NewLine +
            "Das Design wurde dem Programm „Scanner Interface 7“" + Environment.NewLine +
            "(Grewe Computertechnik GmbH 2012) nachempfunden.");
        TaskDialogButton paypalButton = new TaskDialogCommandLinkButton(Lng.T("Anerkennung spenden via PayPal"));
        using var icon32 = icon == null ? null : new Icon(icon, 32, 32); // sonst nimmt der TaskDialog die 16-px-Variante des Fenster-Icons
        var indent = new string(' ', 14);
        var foot = $"{indent}© {buildDate:yyyy} Wilhelm Happe · Version {threeVersion} ({buildDate:d})" +
            $"\n{indent}Tesseract {typeof(Tesseract.TesseractEngine).Assembly.GetName().Version?.ToString(3)}" +
            $"\n{indent}PDFsharp {typeof(PdfSharp.Pdf.PdfDocument).Assembly.GetName().Version?.ToString(3)}" +
            $"\n{indent}<a href=\"https://www.netradio.info\">www.netradio.info</a>";
        var page = new TaskDialogPage()
        {
            Caption = Lng.T("Über") + " " + Application.ProductName,
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

    // Updatesuche über die XML-Datei auf der Webseite des Autors (wie bei den übrigen Programmen);
    // erwartete Elemente unterhalb der Wurzel: <version>, <date>, <url64>
    private const string UpdateXmlUrl = "https://www.netradio.info/download/scanview.xml";
    private const string WebsiteUrl = "https://www.netradio.info";

    private static readonly Lazy<HttpClient> httpClient = new(() =>
        new HttpClient() { Timeout = TimeSpan.FromSeconds(15) });

    /// <summary>Manuelle Updatesuche (?-Menü): lädt die XML-Datei von der Webseite des Autors
    /// und zeigt das Ergebnis; bei einem Update mit Download-Schaltfläche.</summary>
    public static async Task UpdateTaskDlg(nint hwnd)
    {
        var curVersion = Assembly.GetExecutingAssembly().GetName().Version;
        var threeVersion = curVersion?.ToString(3) ?? Lng.T("unbekannt");
        TaskDialogButton downloadButton = new TaskDialogCommandLinkButton(Lng.T("ScanViewSetup.exe herunterladen"),
            Lng.T("Download.Detail", "ScanViewSetup.exe wird im Download-Ordner\ngespeichert. Führe das Setupprogramm aus,\num die neueste Version zu installieren."));
        var updatePage = new TaskDialogPage()
        {
            Caption = Application.ProductName,
            Heading = string.Format(Lng.T("{0} ist auf dem neuesten Stand."), Application.ProductName),
            Text = $"Version {threeVersion} (64-Bit)",
            Icon = TaskDialogIcon.Information,
            AllowCancel = true,
            SizeToContent = true,
            Buttons = { TaskDialogButton.Close }
        };
        var urlString = WebsiteUrl; // Fallback: die Webseite, falls die XML keinen Download-Link nennt
        Version updateVersion = null;
        var dateString = string.Empty;
        var failed = false;
        Cursor.Current = Cursors.WaitCursor; // die Abfrage dauert im Normalfall unter einer Sekunde
        try
        {
            await using var stream = await httpClient.Value.GetStreamAsync(UpdateXmlUrl);
            var root = System.Xml.Linq.XDocument.Load(stream).Root; // Wurzelname egal — Load verkraftet auch die BOM
            var versionString = root?.Element("version")?.Value;
            if (Version.TryParse(versionString ?? string.Empty, out var parsed)) { updateVersion = parsed; }
            dateString = root?.Element("date")?.Value ?? string.Empty;
            var url64 = root?.Element("url64")?.Value;
            if (!string.IsNullOrEmpty(url64)) { urlString = url64; }
        }
        catch (HttpRequestException ex)
        {
            failed = true;
            updatePage.Heading = Lng.T("Die Update-Suche ist fehlgeschlagen.");
            updatePage.Text = ex.StatusCode == HttpStatusCode.NotFound
                ? Lng.T("Die Update-Informationen wurden nicht gefunden.")
                : (ex.StatusCode != null ? $"Status-Code: {ex.StatusCode}\n" : string.Empty) + ex.Message;
        }
        catch (Exception ex) when (ex is TaskCanceledException or System.Xml.XmlException or InvalidOperationException)
        {
            failed = true;
            updatePage.Heading = Lng.T("Die Update-Suche ist fehlgeschlagen.");
            updatePage.Text = ex is TaskCanceledException
                ? Lng.T("Zeitüberschreitung – bitte prüfe die Internetverbindung.")
                : ex.Message;
        }
        finally { Cursor.Current = Cursors.Default; }
        if (!failed && updateVersion == null)
        {
            failed = true;
            updatePage.Heading = Lng.T("Die Update-Suche ist fehlgeschlagen.");
            updatePage.Text = Lng.T("Die Versionsangabe in der Update-Datei konnte nicht gelesen werden.");
        }
        if (failed) { updatePage.Icon = TaskDialogIcon.Error; }
        else if (curVersion != null && updateVersion.CompareTo(curVersion) > 0)
        {
            updatePage.Heading = Lng.T("Es steht ein Update zur Verfügung!");
            updatePage.Text = "Version " + updateVersion + (dateString.Length > 0 ? " " + Lng.T("vom") + " " + dateString : string.Empty);
            updatePage.Buttons.Add(downloadButton);
        }
        if (TaskDialog.ShowDialog(hwnd, updatePage) == downloadButton) { StartLink(hwnd, urlString); }
    }

    /// <summary>Alle Tastenkürzel: Kürzel, Kurztext und optionale Zusatzerklärung für die PDF-Übersicht.</summary>
    // Sortierung: F-Tasten (numerisch), Strg+Zahl, Strg+Buchstabe (alphabetisch), Strg+Sondertaste, Übrige
    public static readonly (string Key, string Text, string Detail)[] ShortcutRows =
    [
        ("F1", "diese Kürzel-Übersicht", null),
        ("F4", "Seite scannen", null),
        ("F6", "Seiten drucken (alle oder markierte)", null),
        ("F7", "Kopiermodus ein/aus",
            "Jeder Scan geht direkt an den Drucker — der Scanner wird zum Kopierer. Drucker, Exemplare und Skalierung stellst du im Kopiermodus-Bereich ein."),
        ("F9", "Seitenübersicht leeren (Neu)", null),
        ("F10", "markierte Seite zuschneiden", null),
        ("F11", "Vollbild ein/aus", null),
        ("Strg+1 … 4", "Ansicht: Optimale Breite, Ganze Seite, Zwei Seiten, Symbole", null),
        ("Strg+D", "Rückseiten einfügen",
            "Duplex von Hand: erst alle Vorderseiten scannen, dann den gewendeten Stapel — die Rückseiten werden verzahnt einsortiert."),
        ("Strg+I", "Bilddateien als Seiten importieren", null),
        ("Strg+L / Strg+R", "Seite nach links / rechts drehen", null),
        ("Strg+Umschalt+R", "Seite um 180° drehen", null),
        ("Strg+S", "durchsuchbare PDF speichern (mit Texterkennung)", null),
        ("Strg+U", "Sortierung umkehren", null),
        ("Strg+X / C / V", "Seite ausschneiden / kopieren / einfügen", null),
        ("Strg+Z", "letzten Schritt rückgängig machen", null),
        ("Strg++ / Strg+−", "Miniaturen vergrößern / verkleinern", null),
        ("Strg+,", "Optionen öffnen", null),
        ("Strg+Mausrad", "Miniaturen vergrößern / verkleinern", null),
        ("Entf", "markierte Seite entfernen", null),
        ("Alt+← / →", "markierte Seite verschieben (auch: Ziehen mit der Maus)", null),
        ("Doppelklick", "Seite im Zuschneiden-Dialog öffnen (Bildbetrachter: Kontextmenü)", null),
        ("2× Esc / Umschalt+Esc", "Programm beenden (Option)", null),
    ];

    /// <summary>Kürzel-Übersicht (F1 und ?-Menü): erstellt die PDF im Downloads-Ordner und öffnet sie
    /// im Standard-PDF-Programm. Existiert die Datei schon, fragt ein Dialog, ob sie geöffnet oder
    /// neu erstellt werden soll (Muster aus PDFlight).</summary>
    public static void ShowShortcutsPdf(nint hwnd, Icon icon)
    {
        var path = ShortcutsPdf.DefaultPath;
        if (File.Exists(path))
        {
            TaskDialogButton openButton = new TaskDialogCommandLinkButton(Lng.T("Vorhandene öffnen"), path);
            TaskDialogButton recreateButton = new TaskDialogCommandLinkButton(Lng.T("Neu erstellen"), Lng.T("z.B. nach einem Update"));
            using var icon32 = icon == null ? null : new Icon(icon, 32, 32); // sonst nimmt der TaskDialog die 16-px-Variante
            var page = new TaskDialogPage()
            {
                Caption = Application.ProductName,
                Heading = Lng.T("Kürzel-Übersicht bereits vorhanden"),
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
            ErrTaskDlg(hwnd, Lng.T("Die PDF-Übersicht konnte nicht erstellt werden."), ex);
        }
    }

    private static void OpenShell(nint hwnd, string path)
    {
        try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
        {
            ErrTaskDlg(hwnd, Lng.T("Die Datei konnte nicht geöffnet werden."), ex);
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
            else { MsgTaskDlg(hwnd, Lng.T("Ungültiger Link!"), string.Format(Lng.T("'{0}' ist keine gültige URL."), url), TaskDialogIcon.ShieldWarningYellowBar); }
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException) { ErrTaskDlg(hwnd, Lng.T("Der Link konnte nicht geöffnet werden."), ex); }
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
