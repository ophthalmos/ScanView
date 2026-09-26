using ScanView.Classes;

namespace ScanView.Forms;

/// <summary>Faxen-Dialog (Toolbar-Button): alle Seiten oder nur die markierte an den
/// virtuellen Faxdrucker (Extras → Faxprogramm) drucken — das Faxprogramm übernimmt dann
/// Empfänger und Versand.</summary>
internal sealed partial class FaxForm : Form
{
    public bool AllPages => radioAll.Checked;

    public FaxForm(int selectedCount)
    {
        InitializeComponent();
        Lng.Apply(this);
        radioSelected.Enabled = selectedCount > 0;
        if (selectedCount > 1) { radioSelected.Text = string.Format(Lng.T("Nur &markierte Seiten ({0})"), selectedCount); } // nach Lng.Apply
        if (radioSelected.Right + 14 > ClientSize.Width) { Width += radioSelected.Right + 14 - ClientSize.Width; } // Fenster wächst mit dem Text
    }
}
