using System.Drawing.Printing;
using ScanView.Classes;

namespace ScanView.Forms;

/// <summary>Drucken-Dialog statt des klassischen Windows-Druckdialogs: Umfang (alle Seiten /
/// nur markierte Seite) plus dieselben Drucker-Einstellungen wie im Kopiermodus. Startet mit
/// den gemeinsamen Druckvorgaben (settings.Copy*); nach OK übernimmt die MainForm die Werte
/// dorthin zurück, sodass Drucken und Kopiermodus dieselbe Vorgabe teilen.</summary>
internal sealed partial class PrintForm : Form
{
    public bool AllPages => radioAll.Checked;

    public string PrinterName => comboPrinter.SelectedItem as string ?? string.Empty;

    /// <summary>Gewähltes Papierformat — null, wenn der Treiber keines meldet.</summary>
    public PaperSize SelectedPaper => comboPaper.SelectedIndex >= 0 && comboPaper.SelectedIndex < paperSizes.Count
        ? paperSizes[comboPaper.SelectedIndex] : null;

    /// <summary>Gewählte Papierzufuhr — null, wenn der Treiber keine meldet.</summary>
    public PaperSource SelectedSource => comboSource.SelectedIndex >= 0 && comboSource.SelectedIndex < paperSources.Count
        ? paperSources[comboSource.SelectedIndex] : null;

    public int DuplexIndex => Math.Max(0, comboDuplex.SelectedIndex);

    public int Copies => (int)numCopies.Value;

    public bool PrintColor => chkColor.Checked;

    public bool FitToPage => chkFit.Checked;

    /// <summary>Die PrinterSettings mit dem DEVMODE aus dem Eigenschaften-Dialog — die MainForm
    /// hängt sie an das PrintDocument, damit Treiber-Extras (Qualität, Papierart …) den Druck erreichen.</summary>
    public PrinterSettings DriverSettings => printerSettings;

    private readonly PrinterSettings printerSettings = new(); // zum Abfragen der Fähigkeiten des gewählten Druckers
    private readonly List<PaperSize> paperSizes = [];         // Papierformate parallel zur Combo
    private readonly List<PaperSource> paperSources = [];     // Papierzufuhren parallel zur Combo

    public PrintForm(bool hasSelection, AppSettings settings)
    {
        InitializeComponent();
        Lng.Apply(this);
        Lng.TranslateItems(comboDuplex); // wird über SelectedIndex ausgewertet
        radioSelected.Enabled = hasSelection;
        foreach (string printer in PrinterSettings.InstalledPrinters) { comboPrinter.Items.Add(printer); }
        var index = comboPrinter.Items.IndexOf(settings.CopyPrinter); // gemeinsame Vorgabe, sonst der Standarddrucker
        if (index < 0) { index = comboPrinter.Items.IndexOf(printerSettings.PrinterName); }
        if (index < 0 && comboPrinter.Items.Count > 0) { index = 0; }
        if (index >= 0) { comboPrinter.SelectedIndex = index; } // lädt die Fähigkeiten (s. Handler)
        // die gespeicherten Vorgaben über die Treiber-Standards legen
        var paperIndex = paperSizes.FindIndex(p => p.RawKind == settings.CopyPaperRawKind);
        if (paperIndex >= 0) { comboPaper.SelectedIndex = paperIndex; }
        var sourceIndex = paperSources.FindIndex(s => s.RawKind == settings.CopyPaperSourceRawKind);
        if (sourceIndex >= 0) { comboSource.SelectedIndex = sourceIndex; }
        if (comboDuplex.Enabled) { comboDuplex.SelectedIndex = Math.Clamp(settings.CopyDuplexIndex, 0, comboDuplex.Items.Count - 1); }
        numCopies.Value = Math.Clamp(settings.CopyCopies, (int)numCopies.Minimum, (int)numCopies.Maximum);
        if (chkColor.Enabled) { chkColor.Checked = settings.CopyColor; }
        chkFit.Checked = settings.CopyFit;
    }

    /// <summary>Lädt Papierformate, Duplex- und Farbfähigkeit des gewählten Druckers in die
    /// Controls — dasselbe Muster wie im Kopiermodus.</summary>
    private void ComboPrinter_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (comboPrinter.SelectedItem is not string printer) { return; }
        printerSettings.PrinterName = printer;
        comboPaper.Items.Clear();
        paperSizes.Clear();
        comboSource.Items.Clear();
        paperSources.Clear();
        try
        {
            foreach (PaperSize paper in printerSettings.PaperSizes)
            {
                paperSizes.Add(paper);
                comboPaper.Items.Add(paper.PaperName);
            }
            var defaultPaper = printerSettings.DefaultPageSettings.PaperSize;
            var index = defaultPaper != null ? paperSizes.FindIndex(p => p.RawKind == defaultPaper.RawKind) : -1;
            if (comboPaper.Items.Count > 0) { comboPaper.SelectedIndex = Math.Max(0, index); }
            foreach (PaperSource source in printerSettings.PaperSources)
            {
                paperSources.Add(source);
                comboSource.Items.Add(source.SourceName);
            }
            var defaultSource = printerSettings.DefaultPageSettings.PaperSource;
            var sourceIndex = defaultSource != null ? paperSources.FindIndex(s => s.RawKind == defaultSource.RawKind) : -1;
            if (comboSource.Items.Count > 0) { comboSource.SelectedIndex = Math.Max(0, sourceIndex); }
            comboDuplex.Enabled = printerSettings.CanDuplex;
            comboDuplex.SelectedIndex = 0; // Einseitig
            chkColor.Enabled = printerSettings.SupportsColor;
            chkColor.Checked = printerSettings.SupportsColor && printerSettings.DefaultPageSettings.Color;
        }
        catch (InvalidPrinterException) { } // Drucker gerade entfernt — die Combos bleiben leer
    }

    /// <summary>Öffnet den Treiber-Eigenschaften-Dialog und übernimmt danach die dort geänderten
    /// Werte (Papierformat, Zufuhr, Duplex, Farbe, Exemplare) in die Controls.</summary>
    private void LinkProperties_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        if (comboPrinter.SelectedItem is not string) { return; }
        if (!PrinterDialog.ShowProperties(this, printerSettings)) { return; }
        var paper = printerSettings.DefaultPageSettings.PaperSize;
        var paperIndex = paper != null ? paperSizes.FindIndex(p => p.RawKind == paper.RawKind) : -1;
        if (paperIndex >= 0) { comboPaper.SelectedIndex = paperIndex; }
        var source = printerSettings.DefaultPageSettings.PaperSource;
        var sourceIndex = source != null ? paperSources.FindIndex(s => s.RawKind == source.RawKind) : -1;
        if (sourceIndex >= 0) { comboSource.SelectedIndex = sourceIndex; }
        if (comboDuplex.Enabled)
        {
            comboDuplex.SelectedIndex = printerSettings.Duplex switch
            {
                Duplex.Vertical => 1,
                Duplex.Horizontal => 2,
                _ => 0
            };
        }
        if (chkColor.Enabled) { chkColor.Checked = printerSettings.DefaultPageSettings.Color; }
        if (printerSettings.Copies >= numCopies.Minimum && printerSettings.Copies <= numCopies.Maximum) { numCopies.Value = printerSettings.Copies; }
    }
}
