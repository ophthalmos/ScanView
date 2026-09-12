namespace ScanView.Classes;

/// <summary>Die Seitenübersicht als fokussierbares FlowLayoutPanel: Ein Klick auf eine Miniatur
/// holt den Tastaturfokus hierher, damit Pfeil-, Bild- und Pos1/Ende-Tasten die Markierung
/// bewegen (MainForm.NavigateSelection) statt die Scan-Einstellungen zu verstellen.
/// Ein FlowLayoutPanel ist von Haus aus nicht selektierbar.</summary>
internal sealed class PageFlowPanel : FlowLayoutPanel
{
    public PageFlowPanel()
    {
        SetStyle(ControlStyles.Selectable, true);
        TabStop = true;
    }
}
