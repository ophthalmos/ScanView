using ScanView.Classes;

namespace ScanView.Forms;

/// <summary>Faxen-Dialog (Toolbar-Button): alle Seiten oder nur die markierte an den
/// virtuellen Faxdrucker (Extras → Faxprogramm) drucken — das Faxprogramm übernimmt dann
/// Empfänger und Versand.</summary>
internal sealed partial class FaxForm : Form
{
    public bool AllPages => radioAll.Checked;

    public FaxForm(bool hasSelection, string faxPrinter)
    {
        InitializeComponent();
        Lng.Apply(this);
        radioSelected.Enabled = hasSelection;
        labelPrinter.Text = string.Format(Lng.T("Faxdrucker: {0}"), faxPrinter);
    }
}
