# ScanView

Seiten scannen, als Miniaturen ordnen und mit Texterkennung als **durchsuchbare PDF** speichern.
Im **Kopiermodus** wird jeder Scan direkt gedruckt — Scanner und Drucker werden zusammen zum Kopierer.

## Funktionen

- Scannen ohne Treiberdialog (WIA): Auflösung, Farbmodus, Scanbereich, Papierzufuhr und Helligkeit direkt im Programm
- Seiten per Drag&Drop ordnen, drehen, zuschneiden/freistellen/ausschneiden, importieren, Rückseiten verzahnen (Duplex von Hand)
- Texterkennung mit Tesseract (Deutsch, Englisch oder kombiniert) — wahlweise auch reine Bild-PDF
- Kopiermodus mit Druckerwahl, Papierformat, Papierzufuhr, Duplex und Exemplaren
- Die Taste am Scanner startet ScanView bzw. holt die laufende Instanz nach vorn und löst sofort einen Scan aus
- Tastenkürzel für alle wichtigen Funktionen (Übersicht als PDF über F1)
- Oberfläche in Deutsch, Englisch, Französisch und Spanisch; alle Einstellungen bleiben erhalten

## Voraussetzungen

- Windows 10/11 (64-Bit) mit WIA-fähigem Scanner
- [.NET Desktop Runtime 10](https://dotnet.microsoft.com/download/dotnet/10.0) (x64)
- [Visual C++ Redistributable](https://aka.ms/vs/17/release/vc_redist.x64.exe) (x64) für die Texterkennung

## Bauen

`dotnet build` auf `ScanView.csproj` (Visual Studio 2026, .NET 10 WinForms).
Der Installer entsteht aus `Installer.iss` mit Inno Setup; er registriert ScanView auch als
WIA-Ereignis-Handler, damit es unter „Scanner und Kameras → Eigenschaften → Ereignisse" wählbar ist.

## Herkunft

Das Bedienkonzept ist in Grundzügen dem Programm „Scanner-Interface 7" der Grewe Computertechnik GmbH
Berlin (zuletzt erschienen 2012, nicht mehr erhältlich) nachempfunden. ScanView ist eine vollständige
Neuentwicklung und enthält weder Code noch Grafiken dieses Programms.

## Lizenz

[MIT](LICENSE) · © Wilhelm Happe
