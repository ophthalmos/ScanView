namespace ScanView.Classes;

/// <summary>TrackBar, die ein Doppelklick auf die Neutralstellung 0 zurücksetzt (Helligkeitsregler).
/// Die WinForms-TrackBar löst selbst kein Doppelklick-Ereignis aus, daher über die Fensternachricht.</summary>
internal sealed class ResetTrackBar : TrackBar
{
    private const int WM_LBUTTONDBLCLK = 0x0203;

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_LBUTTONDBLCLK)
        {
            Value = 0;
            return; // den zweiten Klick nicht als Sprung zur Mausposition auswerten
        }
        base.WndProc(ref m);
    }
}
