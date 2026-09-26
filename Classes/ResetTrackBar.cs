namespace ScanView.Classes;

/// <summary>TrackBar, die ein Doppelklick auf die Neutralstellung 0 zurücksetzt (Helligkeitsregler).
/// Die native Trackbar-Fensterklasse hat keinen Doppelklick-Stil (kein WM_LBUTTONDBLCLK), daher
/// werden zwei schnelle Klicks an derselben Stelle selbst als Doppelklick erkannt.</summary>
internal sealed class ResetTrackBar : TrackBar
{
    private const int WM_LBUTTONDOWN = 0x0201;
    private int lastClickTick = int.MinValue;
    private Point lastClickPoint;

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_LBUTTONDOWN)
        {
            var point = new Point((short)((long)m.LParam & 0xFFFF), (short)(((long)m.LParam >> 16) & 0xFFFF));
            var tick = Environment.TickCount;
            var size = SystemInformation.DoubleClickSize;
            if (tick - lastClickTick <= SystemInformation.DoubleClickTime
                && Math.Abs(point.X - lastClickPoint.X) <= size.Width / 2
                && Math.Abs(point.Y - lastClickPoint.Y) <= size.Height / 2)
            {
                lastClickTick = int.MinValue; // ein dritter Klick beginnt von vorn
                Value = 0;
                return; // den zweiten Klick nicht als Sprung auswerten
            }
            lastClickTick = tick;
            lastClickPoint = point;
        }
        base.WndProc(ref m);
    }
}
