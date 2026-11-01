namespace BareWin;
static class Program { [STAThread] static void Main() { ApplicationConfiguration.Initialize(); using (var g = Graphics.FromHwnd(IntPtr.Zero)) Theme.S = g.DpiX / 96f; Application.Run(new MainForm()); } }
