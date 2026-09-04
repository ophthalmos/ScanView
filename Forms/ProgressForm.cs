namespace ScanView.Forms;

/// <summary>Kleine Fortschrittsanzeige (Balken + Text, ohne Titelleiste — ControlBox=false und
/// leerer Text blenden sie aus) für die Texterkennung bzw. das PDF-Erstellen beim Speichern;
/// im Stil der WIA-Scananzeige. Kein Abbrechen — die parallele Erkennung läuft durch.</summary>
internal sealed partial class ProgressForm : Form
{
    public ProgressForm()
    {
        InitializeComponent();
    }

    /// <summary>Text und Balkenstand aktualisieren (auf dem UI-Thread aufzurufen).</summary>
    public void SetProgress(string text, int done, int total)
    {
        labelStatus.Text = text;
        progressBar.Maximum = Math.Max(1, total);
        progressBar.Value = Math.Clamp(done, 0, progressBar.Maximum);
    }
}
