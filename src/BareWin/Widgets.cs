using System.Drawing.Drawing2D;
namespace BareWin;
// Rounded flat button: Primary (accent), Ghost (surface) or Nav (sidebar item).
public class Btn : Control {
  public enum Kind { Primary, Ghost, Nav }
  public Kind Style = Kind.Ghost; public bool Active; public string Glyph = "";
  bool hover, down;
  public Btn() { SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
    Cursor = Cursors.Hand; Height = 36; Font = Theme.UI(9.5f, FontStyle.Bold); BackColor = Color.Transparent; }
  protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
  protected override void OnMouseLeave(EventArgs e) { hover = down = false; Invalidate(); base.OnMouseLeave(e); }
  protected override void OnMouseDown(MouseEventArgs e) { down = true; Invalidate(); base.OnMouseDown(e); }
  protected override void OnMouseUp(MouseEventArgs e) { down = false; Invalidate(); base.OnMouseUp(e); }
  protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }
  protected override void OnPaint(PaintEventArgs e) {
    var p = Theme.P; var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias; g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
    Color bg, fg, border = Color.Empty;
    switch (Style) {
      case Kind.Primary: bg = !Enabled ? p.Surface2 : down ? p.Accent : hover ? p.AccentHover : p.Accent; fg = Enabled ? p.OnAccent : p.Muted; break;
      case Kind.Nav: bg = Active ? p.Surface2 : hover ? Blend(p.Side, p.Surface2, .6f) : p.Side; fg = Active ? p.Text : p.Muted; break;
      default: bg = hover ? p.Surface2 : p.Surface; fg = Enabled ? p.Text : p.Muted; border = p.Border; break;
    }
    var r = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
    using (var path = Theme.Round(r, 8)) { using var b = new SolidBrush(bg); g.FillPath(b, path); if (!border.IsEmpty) { using var pen = new Pen(border); g.DrawPath(pen, path); } }
    if (Style == Kind.Nav && Active) { using var a = new SolidBrush(p.Accent); using var bar = Theme.Round(new RectangleF(2, Height * .25f, 3, Height * .5f), 1.5f); g.FillPath(a, bar); }
    var txt = string.IsNullOrEmpty(Glyph) ? Text : Glyph + "   " + Text;
    var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | (Style == Kind.Nav ? TextFormatFlags.Left : TextFormatFlags.HorizontalCenter);
    TextRenderer.DrawText(g, txt, Style == Kind.Nav ? Theme.UI(10f, Active ? FontStyle.Bold : FontStyle.Regular) : Font,
      Style == Kind.Nav ? new Rectangle(16, 0, Width - 20, Height) : ClientRectangle, fg, flags);
  }
  static Color Blend(Color a, Color b, float t) => Color.FromArgb((int)(a.R + (b.R - a.R) * t), (int)(a.G + (b.G - a.G) * t), (int)(a.B + (b.B - a.B) * t));
}
// iOS-style switch.
public class Switch : Control {
  bool on; public bool Checked { get => on; set { on = value; Invalidate(); CheckedChanged?.Invoke(this, EventArgs.Empty); } }
  public event EventHandler? CheckedChanged;
  public Switch() { SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true); Size = new(40, 22); Cursor = Cursors.Hand; BackColor = Color.Transparent; }
  protected override void OnResize(EventArgs e) { base.OnResize(e); Invalidate(); }
  protected override void OnClick(EventArgs e) { Checked = !Checked; base.OnClick(e); }
  protected override void OnPaint(PaintEventArgs e) {
    var p = Theme.P; var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
    g.Clear(Parent?.BackColor ?? p.Surface); float h = Math.Min(Height, Width * 0.55f) - 1f, w = Math.Min(Width - 1f, h * 1.82f), x = (Width - w) / 2f, y = (Height - h) / 2f;
    using var path = Theme.Round(new RectangleF(x, y, w, h), h / 2);
    using (var b = new SolidBrush(on ? p.Accent : p.Border)) g.FillPath(b, path);
    float pad = Math.Max(2f, h * 0.14f), d = h - pad * 2; using var k = new SolidBrush(Color.White); g.FillEllipse(k, on ? x + w - d - pad : x + pad, y + pad, d, d);
  }
}
// Rounded card panel.
public class Card : Panel {
  public int Radius = 12;
  public Card() { SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true); }
  protected override void OnPaint(PaintEventArgs e) {
    var p = Theme.P; var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias; g.Clear(Parent?.BackColor ?? p.Bg);
    using var path = Theme.Round(new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), Radius);
    using (var b = new SolidBrush(p.Surface)) g.FillPath(b, path); using (var pen = new Pen(p.Border)) g.DrawPath(pen, path);
  }
}

// Double-buffered ListView (no hover flicker) that paints its empty header area in theme colors.
public class FlatList : ListView {
  public FlatList() { SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true); DoubleBuffered = true; }
  [System.Runtime.InteropServices.DllImport("user32.dll")] static extern IntPtr SendMessage(IntPtr h, int m, IntPtr w, IntPtr l);
  protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); SendMessage(Handle, 0x1036, (IntPtr)0x10000, (IntPtr)0x10000); /* LVS_EX_DOUBLEBUFFER */ ThemeHeader(); }
  public void ThemeHeader() { var hdr = SendMessage(Handle, 0x101F, IntPtr.Zero, IntPtr.Zero); if (hdr != IntPtr.Zero) Theme.SetWindowTheme(hdr, Theme.P.Dark ? "DarkMode_ItemsView" : "ItemsView", null); }
  protected override void WndProc(ref Message m) { if (m.Msg == 0x14 /* WM_ERASEBKGND */) { m.Result = (IntPtr)1; return; } base.WndProc(ref m); }
}
