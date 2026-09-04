using System.Drawing.Printing;
using ScanView.Classes;

namespace ScanView.Forms;

/// <summary>Extras → Faxprogramm: legt den virtuellen Faxdrucker fest (z.B. FRITZ!fax-Drucker) —
/// der Faxen-Button druckt die Seiten dorthin, das Faxprogramm übernimmt Empfänger und Versand.</summary>
internal sealed partial class FaxPrinterForm : Form
{
    public string FaxPrinter => comboPrinter.SelectedItem as string ?? string.Empty;

    public FaxPrinterForm(string currentPrinter)
    {
        InitializeComponent();
        Lng.Apply(this);
        labelHint.Text = Lng.T("Hint.FaxPrinter", labelHint.Text); // Mehrzeiler brauchen explizite Schlüssel
        foreach (string printer in PrinterSettings.InstalledPrinters) { comboPrinter.Items.Add(printer); }
        var match = comboPrinter.Items.Cast<string>().FirstOrDefault(p => string.Equals(p, currentPrinter, StringComparison.OrdinalIgnoreCase))
            ?? comboPrinter.Items.Cast<string>().FirstOrDefault(p => p.Contains("fax", StringComparison.OrdinalIgnoreCase)); // Vorschlag
        if (match != null) { comboPrinter.SelectedItem = match; }
    }
}
