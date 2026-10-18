# ScanView

ScanView turns your scanner into a tool for the paperless office: scan pages without the driver dialog, arrange them as thumbnails, rotate and crop them, then save them with text recognition as a **searchable PDF** – or as PDF/A, JPEG, PNG or TIFF.

In **copy mode** every scan is printed right away, so scanner and printer become a copier. The button on the scanner starts a scan immediately.

For Windows 10/11, in German, English, French and Spanish.

## Features

- Scanning without the driver dialog via WIA or TWAIN (32- and 64-bit drivers): resolution, colour mode, scan area, paper source and brightness are set in the program and can be saved as profiles
- Only scanners that are switched on are listed; a scanner that offers both interfaces appears twice, as "(WIA)" and "(TWAIN)"
- Scan progress in percent (if the driver supports it), cancel at any time
- Pages as thumbnails: reorder by drag & drop or keyboard, rotate, crop/clear/cut out, import image files, interleave back sides (manual duplex), undo
- Text recognition with Tesseract (German, English or both) – or a plain image PDF, optionally PDF/A
- Export as JPEG, PNG or multi-page TIFF; print and fax with page selection
- Copy mode with printer, paper size, paper source, duplex and number of copies
- The scanner button starts ScanView (or brings the running instance to the front) and scans at once
- Keyboard shortcuts for all important functions (overview as PDF via F1)

## Requirements

- Windows 10/11 (64-bit) with a scanner that has a WIA or TWAIN driver
- [.NET Desktop Runtime 10](https://dotnet.microsoft.com/download/dotnet/10.0) (x64)
- [Visual C++ Redistributable](https://aka.ms/vs/17/release/vc_redist.x64.exe) (x64) for text recognition

## Building

`dotnet build` on `ScanView.csproj` (Visual Studio 2026, .NET 10 WinForms). This also builds the TWAIN helpers `Twain32.exe` and `Twain64.exe` (.NET Framework 4.8, part of Windows 10/11; shared source in `Twain32/`): most TWAIN drivers exist only as 32-bit, and a 64-bit program cannot load them directly. The installer is built from `Installer.iss` with Inno Setup; it registers ScanView as a WIA event handler so it can be selected under "Scanners and Cameras → Properties → Events", and installs the TWAIN Data Source Manager (`TWAINDSM.dll`, 32 and 64 bit) of the TWAIN Working Group into the Windows system folders.

## Origin

The user interface concept loosely follows "Scanner-Interface 7" by Grewe Computertechnik GmbH, Berlin (last released in 2012, no longer available). ScanView is a complete re-implementation and contains neither code nor graphics from that program.

## License

[GNU AGPL 3.0](LICENSE) – © 2026 Wilhelm Happe. Earlier versions were released under the MIT License.
Bundled third-party components and their licenses are listed in [NOTICE.txt](NOTICE.txt).
