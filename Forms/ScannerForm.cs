using System.ComponentModel;
using ScanView.Classes;

namespace ScanView.Forms;

/// <summary>Scanner-Dialog (Extras → Scanner): Gerät wählen und die Gerätetasten über den
/// Windows-Dialog „Scanner und Kameras" (sticpl.cpl) konfigurieren.</summary>
internal sealed partial class ScannerForm : Form
{
    private readonly List<ScannerInfo> scanners;

    /// <summary>Das gewählte Gerät (gültig nach DialogResult.OK); null, wenn keines gefunden wurde.</summary>
    public ScannerInfo SelectedScanner =>
        comboScanner.SelectedIndex >= 0 && comboScanner.SelectedIndex < scanners.Count ? scanners[comboScanner.SelectedIndex] : null;

    public ScannerForm(string currentScannerId)
    {
        InitializeComponent();
        scanners = ScanService.ListScanners();
        foreach (var scanner in scanners) { comboScanner.Items.Add(scanner.Name); }
        if (scanners.Count == 0)
        {
            comboScanner.Items.Add("(kein Scanner gefunden)");
            comboScanner.Enabled = false;
            btnOk.Enabled = false;
        }
        var currentIndex = scanners.FindIndex(s => s.Id == currentScannerId);
        comboScanner.SelectedIndex = Math.Max(0, currentIndex);
    }

    /// <summary>Windows-Systemsteuerung „Scanner und Kameras": dort werden die Tasten am Gerät
    /// (Ereignisse) mit Programmen verknüpft — das verwaltet Windows, nicht ScanView.</summary>
    private void BtnDeviceKeys_Click(object sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("control.exe", "sticpl.cpl") { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
        {
            TaskDlg.ErrTaskDlg(Handle, "Die Windows-Einstellungen konnten nicht geöffnet werden.", ex);
        }
        finally { Close(); }
    }
}
