using ScanView.Forms;

namespace ScanView;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm(selfTest: args.Contains("--selftest")));
    }
}
