using System.Drawing.Printing;
using ScanView.Classes;

namespace ScanView.Forms;

/// <summary>Extras → Faxprogramm: legt den virtuellen Faxdrucker fest (z.B. FRITZ!fax-Drucker) —
/// der Faxen-Button druckt die Seiten dorthin, das Faxprogramm übernimmt Empfänger und Versand.</summary>
internal sealed partial class FaxPrinterForm : Form
{
    /// <summary>Gewählter Faxdrucker — leer beim Eintrag „(kein Faxdrucker)", dann bleibt
    /// der Faxen-Button in der Toolbar ausgeblendet.</summary>
    public string FaxPrinter => comboPrinter.SelectedIndex > 0 ? (string)comboPrinter.SelectedItem : string.Empty;

    public FaxPrinterForm(string currentPrinter)
    {
        InitializeComponent();
        Lng.Apply(this);
        labelHint.Text = Lng.T("Hint.FaxPrinter", labelHint.Text); // Mehrzeiler brauchen explizite Schlüssel
        comboPrinter.Items.Add(Lng.T("(kein Faxdrucker)"));
        foreach (string printer in PrinterSettings.InstalledPrinters) { comboPrinter.Items.Add(printer); }
        var match = comboPrinter.Items.Cast<string>().Skip(1).FirstOrDefault(p => string.Equals(p, currentPrinter, StringComparison.OrdinalIgnoreCase))
            ?? comboPrinter.Items.Cast<string>().Skip(1).FirstOrDefault(p => p.Contains("fax", StringComparison.OrdinalIgnoreCase)); // Vorschlag
        if (match != null) { comboPrinter.SelectedItem = match; }
        else { comboPrinter.SelectedIndex = 0; }
    }
}
