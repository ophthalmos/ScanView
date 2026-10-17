using ScanView.Classes;

namespace ScanView.Forms;

/// <summary>Kleine Fortschrittsanzeige (Balken + Text), bildschirmzentriert, mit Titelleiste (den Titel
/// setzt der Aufrufer), ohne Schließen-Knopf. Beim Speichern ohne Abbrechen (die parallele Erkennung
/// läuft durch), beim Scannen mit „Abbrechen" (CanCancel) – der Aufrufer fragt CancelRequested ab.</summary>
internal sealed partial class ProgressForm : Form
{
    public ProgressForm()
    {
        InitializeComponent();
        Lng.Apply(this);
    }

    /// <summary>True, sobald „Abbrechen" geklickt wurde.</summary>
    public bool CancelRequested { get; private set; }

    /// <summary>Blendet „Abbrechen" ein und vergrößert das Fenster entsprechend.</summary>
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool CanCancel
    {
        get => btnCancel.Visible;
        set
        {
            btnCancel.Visible = value;
            ClientSize = new Size(ClientSize.Width, value ? btnCancel.Bottom + 10 : progressBar.Bottom + 14);
        }
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
        CancelRequested = true;
        btnCancel.Enabled = false; // der Treiber beendet die Übertragung beim nächsten Datenblock
    }

    /// <summary>Text und Balkenstand aktualisieren (auf dem UI-Thread aufzurufen).</summary>
    public void SetProgress(string text, int done, int total)
    {
        labelStatus.Text = text;
        progressBar.Maximum = Math.Max(1, total);
        progressBar.Value = Math.Clamp(done, 0, progressBar.Maximum);
    }
}
