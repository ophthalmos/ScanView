; ============================================================================
; ScanView – Inno-Setup-Skript
;
; Voraussetzungen auf dem Zielrechner:
;   - .NET Desktop Runtime 10 (x64) — fehlt sie, zeigt Windows beim ersten
;     Start selbst einen Dialog mit Download-Link, daher keine Prüfung hier.
;   - Visual C++ Redistributable (x64) für die nativen Tesseract-DLLs
;     (auf den meisten Systemen vorhanden; das Setup warnt, falls es fehlt).
;
; Vor dem Kompilieren: Release-Build erstellen (dotnet build -c Release) —
; der Ordner enthält dann auch x64\ (native DLLs) und tessdata\.
; ============================================================================

#define appName "ScanView"
#define appVersion "0.1.0"
#define stiEventGuid "{{A7E4C9D2-3B61-48F5-8E27-D90B41C6A358}"
#define releaseDir "bin\Release\net10.0-windows"

[Setup]
AppId={{9C4E2D71-8B0F-4A46-B4E3-6E5F0D2A7C18}
AppName={#appName}
AppVersion={#appVersion}
AppVerName={#appName} {#appVersion} (64-Bit)
VersionInfoVersion={#appVersion}
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
AppPublisher=Wilhelm Happe
AppCopyright=© 2026 W. Happe
UsePreviousAppDir=yes
DefaultDirName={autopf}\{#appName}
DefaultGroupName={#appName}
DisableWelcomePage=yes
DisableReadyPage=yes
SetupIconFile=ScanView.ico
UninstallDisplayIcon={app}\{#appName}.exe
OutputDir=.
OutputBaseFilename={#appName}Setup
Compression=lzma2/ultra
SolidCompression=yes
DirExistsWarning=no
CloseApplications=yes
SetupMutex={#appName}_SetupMutex
WizardStyle=modern

[Languages]
; Das Setup wählt die Sprache automatisch nach der Windows-Sprache; erste = Rückfall.
; Die gewählte Sprache landet in language.default und wird von {#appName} einmalig übernommen.
Name: de; MessagesFile: "compiler:Languages\German.isl"
Name: en; MessagesFile: "compiler:Default.isl"
Name: fr; MessagesFile: "compiler:Languages\French.isl"
Name: es; MessagesFile: "compiler:Languages\Spanish.isl"

[Messages]
de.ConfirmUninstall=Bist du sicher, dass du %1 und alle zugehörigen Komponenten entfernen möchtest? Vor einem Update ist keine Deinstallation erforderlich.
en.ConfirmUninstall=Are you sure you want to remove %1 and all of its components? You do not need to uninstall before an update.
fr.ConfirmUninstall=Voulez-vous vraiment supprimer %1 et tous ses composants ? Une désinstallation n'est pas nécessaire avant une mise à jour.
es.ConfirmUninstall=¿Seguro que desea quitar %1 y todos sus componentes? No es necesario desinstalar antes de una actualización.

[CustomMessages]
de.Run={#appName} starten
de.DesktopIcon=Verknüpfung auf dem Desktop anlegen
de.VCRedistMissing=Das Visual C++ Redistributable (x64) wurde nicht gefunden.%n%n{#appName} benötigt es für die Texterkennung (Tesseract). Bitte lade es herunter von:%nhttps://aka.ms/vs/17/release/vc_redist.x64.exe%n%nDie Installation wird trotzdem fortgesetzt.
en.Run=Launch {#appName}
en.DesktopIcon=Create a desktop shortcut
en.VCRedistMissing=The Visual C++ Redistributable (x64) was not found.%n%n{#appName} needs it for text recognition (Tesseract). Please download it from:%nhttps://aka.ms/vs/17/release/vc_redist.x64.exe%n%nSetup will continue anyway.
fr.Run=Lancer {#appName}
fr.DesktopIcon=Créer un raccourci sur le Bureau
fr.VCRedistMissing=Le Visual C++ Redistributable (x64) est introuvable.%n%n{#appName} en a besoin pour la reconnaissance de texte (Tesseract). Veuillez le télécharger depuis :%nhttps://aka.ms/vs/17/release/vc_redist.x64.exe%n%nL'installation continue malgré tout.
es.Run=Iniciar {#appName}
es.DesktopIcon=Crear un acceso directo en el escritorio
es.VCRedistMissing=No se encontró el Visual C++ Redistributable (x64).%n%n{#appName} lo necesita para el reconocimiento de texto (Tesseract). Descárguelo desde:%nhttps://aka.ms/vs/17/release/vc_redist.x64.exe%n%nLa instalación continuará de todos modos.

[Tasks]
Name: desktopicon; Description: "{cm:DesktopIcon}"; Flags: unchecked

[Registry]
; WIA-Ereignis-Handler (gleiches Muster wie NAPS2): fuellt die Programmliste in der Windows-
; Systemsteuerung "Scanner und Kameras" -> Eigenschaften -> Ereignisse -> "Programm starten".
; Die Scanner-Taste startet dann ScanView mit /StiDevice-Argumenten; eine bereits laufende
; Instanz wird dank Einmal-Instanz-Logik nur in den Vordergrund geholt. Kommandozeile bewusst
; ohne Anfuehrungszeichen (WIA parst sie selbst so, s. NAPS2/Photoshop-Eintraege).
Root: HKLM; Subkey: "SYSTEM\CurrentControlSet\Control\StillImage\Events\STIProxyEvent\{#stiEventGuid}"; ValueType: string; ValueName: "Name"; ValueData: "{#appName}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SYSTEM\CurrentControlSet\Control\StillImage\Events\STIProxyEvent\{#stiEventGuid}"; ValueType: string; ValueName: "Desc"; ValueData: "Mit {#appName} scannen"
Root: HKLM; Subkey: "SYSTEM\CurrentControlSet\Control\StillImage\Events\STIProxyEvent\{#stiEventGuid}"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\{#appName}.exe,0"
Root: HKLM; Subkey: "SYSTEM\CurrentControlSet\Control\StillImage\Events\STIProxyEvent\{#stiEventGuid}"; ValueType: string; ValueName: "Cmdline"; ValueData: "{app}\{#appName}.exe /StiDevice:%1 /StiEvent:%2"
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\Handlers\WIA_{#stiEventGuid}"; ValueType: string; ValueName: "Action"; ValueData: "Mit {#appName} scannen"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\Handlers\WIA_{#stiEventGuid}"; ValueType: string; ValueName: "CLSID"; ValueData: "WIACLSID"
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\Handlers\WIA_{#stiEventGuid}"; ValueType: string; ValueName: "DefaultIcon"; ValueData: "sti.dll,0"
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\Handlers\WIA_{#stiEventGuid}"; ValueType: string; ValueName: "InitCmdLine"; ValueData: "/WiaCmd;{app}\{#appName}.exe /StiDevice:%1 /StiEvent:%2;"
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\Handlers\WIA_{#stiEventGuid}"; ValueType: string; ValueName: "Provider"; ValueData: "{#appName}"

[Files]
Source: "{#releaseDir}\*"; Excludes: "*.pdb,selftest*.png"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#appName}"; Filename: "{app}\{#appName}.exe"
Name: "{autodesktop}\{#appName}"; Filename: "{app}\{#appName}.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\{#appName}.exe"; Description: "{cm:Run}"; Flags: nowait postinstall skipifsilent

; Hinweis: Die Benutzereinstellungen (%APPDATA%\ScanView\settings.json) und die
; behaltenen Seiten (%LOCALAPPDATA%\ScanView\Seiten) bleiben bei der Deinstallation erhalten.

[UninstallDelete]
Type: files; Name: "{app}\language.default"

[Code]
procedure CurStepChanged(CurStep: TSetupStep);
begin
  { Sprachwahl des Setups für ScanView hinterlegen; das Programm übernimmt sie einmalig beim Start }
  if CurStep = ssPostInstall then
    SaveStringToFile(ExpandConstant('{app}\language.default'), ActiveLanguage, False);
end;

function InitializeSetup(): Boolean;
var
  Installed: Cardinal;
begin
  Result := True;
  { Visual C++ Redistributable vorhanden? (.NET-Runtime prüft Windows beim ersten Start selbst) }
  if not (RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64', 'Installed', Installed) and (Installed = 1)) then
    MsgBox(CustomMessage('VCRedistMissing'), mbInformation, MB_OK);
end;
