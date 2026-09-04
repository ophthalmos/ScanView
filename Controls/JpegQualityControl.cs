using System.ComponentModel;

namespace ScanView.Controls;

/// <summary>JPEG-Qualität als Slider in 5er-Schritten (30–100) mit direkter Wertanzeige —
/// gemeinsam für Speichern-Dialog und Optionen; die angezeigte Zahl IST der Encoderwert.
/// (Die Photoshop-Stufen 0–12 waren zu weit vom Encoderbereich entfernt und sind wieder raus.)</summary>
internal sealed partial class JpegQualityControl : UserControl
{
    /// <summary>JPEG-Qualität für den Encoder (30–100), gerastert auf 5er-Schritte.
    /// Wird zur Laufzeit gesetzt — der Designer serialisiert sie nicht.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Quality
    {
        get => trackQuality.Value * 5; // der Slider läuft intern in 5er-Einheiten (6–20)
        set => trackQuality.Value = Math.Clamp((int)Math.Round(value / 5.0), trackQuality.Minimum, trackQuality.Maximum);
    }

    public JpegQualityControl()
    {
        InitializeComponent();
        TrackQuality_ValueChanged(this, EventArgs.Empty); // Wertanzeige auch beim Designer-Startwert
    }

    private void TrackQuality_ValueChanged(object sender, EventArgs e)
    {
        labelValue.Text = Quality.ToString();
    }
}
