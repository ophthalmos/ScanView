using System.ComponentModel;
using ScanView.Classes;

namespace ScanView.Controls;

/// <summary>JPEG-Qualität nach Photoshop-Vorbild, gemeinsam für Speichern-Dialog und Optionen:
/// 13 Stufen 0–12 als Slider (zentraler Wert), Zahleneingabe und Stufen-Combo
/// (0–4 Niedrig, 5–7 Mittel, 8–12 Hoch; die Wahl setzt die typischen Werte 3/5/8) —
/// alle drei synchron. Nach außen zählt der Encoderwert 30–100 (linear gemappt).</summary>
internal sealed partial class JpegQualityControl : UserControl
{
    /// <summary>JPEG-Qualität für den Encoder (30–100), gemappt aus den 13 Stufen 0–12.
    /// Wird zur Laufzeit gesetzt — der Designer serialisiert sie nicht.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Quality
    {
        get => 30 + (int)Math.Round(trackQuality.Value * 70 / 12.0);
        set
        {
            trackQuality.Value = Math.Clamp((int)Math.Round((value - 30) * 12 / 70.0), 0, 12);
            TrackQuality_ValueChanged(this, EventArgs.Empty); // auch beim Designer-Startwert verteilen
        }
    }

    private bool syncingQuality; // der Slider ist der zentrale Wert; TextBox und Stufen-Combo folgen ihm

    public JpegQualityControl()
    {
        InitializeComponent();
        Lng.Apply(this);
        Lng.TranslateItems(comboQuality); // wird über SelectedIndex ausgewertet
        TrackQuality_ValueChanged(this, EventArgs.Empty);
    }

    /// <summary>Verteilt den Sliderwert an TextBox und Stufen-Combo.</summary>
    private void TrackQuality_ValueChanged(object sender, EventArgs e)
    {
        syncingQuality = true;
        var value = trackQuality.Value;
        if (!int.TryParse(textBoxQuality.Text, out var typed) || typed != value) { textBoxQuality.Text = value.ToString(); }
        comboQuality.SelectedIndex = value <= 4 ? 0 : value <= 7 ? 1 : 2;
        syncingQuality = false;
    }

    private void TextBoxQuality_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) { e.Handled = true; } // nur Ziffern
    }

    private void TextBoxQuality_TextChanged(object sender, EventArgs e)
    {
        if (syncingQuality || !int.TryParse(textBoxQuality.Text, out var value)) { return; } // leer: erst beim Verlassen normalisieren
        if (value > 12) // Eingaben über dem Maximum sofort kappen
        {
            textBoxQuality.Text = "12";
            textBoxQuality.SelectionStart = textBoxQuality.TextLength;
            return;
        }
        trackQuality.Value = value; // verteilt über ValueChanged an die Stufen-Combo
    }

    private void TextBoxQuality_Leave(object sender, EventArgs e)
    {
        if (!int.TryParse(textBoxQuality.Text, out var value) || value != trackQuality.Value)
        {
            textBoxQuality.Text = trackQuality.Value.ToString(); // leere/ungültige Eingabe zurücksetzen
        }
    }

    /// <summary>Stufenwahl setzt den typischen Wert der Kategorie (wie in Photoshop: 3 / 5 / 8).</summary>
    private void ComboQuality_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (syncingQuality) { return; }
        trackQuality.Value = comboQuality.SelectedIndex switch { 0 => 3, 1 => 5, _ => 8 };
    }
}
