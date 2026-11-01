using System.Runtime.InteropServices;
using System.Text.Json;
namespace BareWin;
public record Palette(Color Bg, Color Side, Color Surface, Color Surface2, Color Border, Color Text, Color Muted, Color Accent, Color AccentHover,
  Color OnAccent, Color Danger, Color Ok, Color Warn, Color TermBg, Color TermText, Color Cmd, bool Dark);
public static class Theme {
  public static readonly Palette DarkP = new(C(0x0E1014), C(0x12151B), C(0x171B22), C(0x1F2430), C(0x2A303C), C(0xE7EAF0), C(0x8B93A3),
    C(0x5B8CFF), C(0x7AA2FF), Color.White, C(0xFF5D6C), C(0x3DDC97), C(0xF5C451), C(0x0A0C10), C(0xC9D1D9), C(0x6CB6FF), true);
  public static readonly Palette LightP = new(C(0xF4F6FA), C(0xEBEEF4), Color.White, C(0xF0F2F7), C(0xDCE1EA), C(0x161A22), C(0x5F6777),
    C(0x2F6FEB), C(0x1F5BD1), Color.White, C(0xD93545), C(0x168A5A), C(0xB7791F), C(0x0F1218), C(0xC9D1D9), C(0x6CB6FF), false);
  public static Palette P = DarkP;
  public static float S = 1f; // DPI scale, set in Program
  public static int D(int v) => (int)Math.Round(v * S);
  static Color C(int rgb) => Color.FromArgb(255, (rgb >> 16) & 255, (rgb >> 8) & 255, rgb & 255);
  public static Font UI(float s, FontStyle st = FontStyle.Regular) => new("Segoe UI", s, st);
  public static Font Mono(float s) { foreach (var n in new[] { "Cascadia Mono", "Cascadia Code", "Consolas" }) { var f = new Font(n, s); if (f.Name == n) return f; f.Dispose(); } return new Font(FontFamily.GenericMonospace, s); }
  public static System.Drawing.Drawing2D.GraphicsPath Round(RectangleF r, float rad) {
    var p = new System.Drawing.Drawing2D.GraphicsPath(); float d = rad * 2;
    if (rad <= 0) { p.AddRectangle(r); return p; }
    p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90); p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90); p.CloseFigure(); return p;
  }
  [DllImport("dwmapi.dll")] static extern int DwmSetWindowAttribute(IntPtr h, int a, ref int v, int s);
  [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)] public static extern int SetWindowTheme(IntPtr h, string? app, string? id);
  public static void TitleBar(Form f) {
    if (!f.IsHandleCreated) return;
    int on = P.Dark ? 1 : 0; DwmSetWindowAttribute(f.Handle, 20, ref on, 4);
    int round = 2; DwmSetWindowAttribute(f.Handle, 33, ref round, 4);
    int cap = ColorTranslator.ToWin32(P.Side); DwmSetWindowAttribute(f.Handle, 35, ref cap, 4);
  }
  public static void Scrollbars(Control c) { if (c.IsHandleCreated) SetWindowTheme(c.Handle, P.Dark ? "DarkMode_Explorer" : "Explorer", null); }
}
public class Settings {
  public bool Dark { get; set; } = true;
  public bool En { get; set; } = !System.Globalization.CultureInfo.CurrentUICulture.Name.StartsWith("pt", StringComparison.OrdinalIgnoreCase);
  public bool TermOpen { get; set; } = true;
  static string Path => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BareWin", "settings.json");
  public static Settings Load() { try { return JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path)) ?? new(); } catch { return new(); } }
  public void Save() { try { Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!); File.WriteAllText(Path, JsonSerializer.Serialize(this)); } catch { } }
}
