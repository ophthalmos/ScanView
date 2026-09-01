using System.ComponentModel;
using ScanView.Classes;

namespace ScanView.Forms;

/// <summary>Scanner-Dialog (Extras → Scanner): Gerät wählen und die Gerätetasten über den
/// Windows-Dialog „Scanner und Kameras" (sticpl.cpl) konfigurieren.
/// Bewusst ohne Designer-Datei — der Dialog ist klein und komplett in Code aufgebaut.</summary>
internal sealed class ScannerForm : Form
{
    private readonly ComboBox comboScanner;
    private readonly List<ScannerInfo> scanners;

    /// <summary>Das gewählte Gerät (gültig nach DialogResult.OK); null, wenn keines gefunden wurde.</summary>
    public ScannerInfo SelectedScanner =>
        comboScanner.SelectedIndex >= 0 && comboScanner.SelectedIndex < scanners.Count ? scanners[comboScanner.SelectedIndex] : null;

    public ScannerForm(string currentScannerId)
    {
        Text = "Scanner wählen";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowIcon = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(430, 208);

        Label labelDevice = new() { AutoSize = true, Location = new Point(16, 16), Text = "Dieses &Gerät verwenden:" };
        comboScanner = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(16, 36), Width = 398 };
        scanners = ScanService.ListScanners();
        foreach (var scanner in scanners) { comboScanner.Items.Add(scanner.Name); }
        if (scanners.Count == 0)
        {
            comboScanner.Items.Add("(kein Scanner gefunden)");
            comboScanner.Enabled = false;
        }
        var currentIndex = scanners.FindIndex(s => s.Id == currentScannerId);
        comboScanner.SelectedIndex = Math.Max(0, currentIndex);

        Button btnDeviceKeys = new() { Location = new Point(16, 80), Size = new Size(220, 26), Text = "Geräte&tasten konfigurieren …" };
        btnDeviceKeys.Click += (s, e) =>
        {
            try
            {
                // Windows-Systemsteuerung „Scanner und Kameras": dort werden die Tasten am Gerät
                // (Ereignisse) mit Programmen verknüpft — das verwaltet Windows, nicht ScanView
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("control.exe", "sticpl.cpl") { UseShellExecute = true });
            }
            catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
            {
                TaskDlg.ErrTaskDlg(Handle, "Die Windows-Einstellungen konnten nicht geöffnet werden.", ex);
            }
        };
        Label labelHint = new()
        {
            AutoSize = true,
            ForeColor = SystemColors.GrayText,
            Location = new Point(16, 114),
            Text = "Öffnet die Windows-Einstellungen „Scanner und Kameras\".\nDort lassen sich die Tasten am Gerät mit Programmen verknüpfen\n(Eigenschaften → Ereignisse).",
        };

        Button btnOk = new() { Text = "OK", DialogResult = DialogResult.OK, Bounds = new Rectangle(252, 170, 80, 26), Enabled = scanners.Count > 0 };
        Button btnCancel = new() { Text = "Abbrechen", DialogResult = DialogResult.Cancel, Bounds = new Rectangle(338, 170, 80, 26) };
        AcceptButton = btnOk;
        CancelButton = btnCancel;

        Controls.AddRange([labelDevice, comboScanner, btnDeviceKeys, labelHint, btnOk, btnCancel]);
    }
}
