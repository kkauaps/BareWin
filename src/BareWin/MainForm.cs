using System.Reflection;
using System.Runtime.InteropServices;
namespace BareWin;
public class MainForm : Form {
  readonly Settings cfg = Settings.Load();
  // shell
  readonly Panel side = new() { Dock = DockStyle.Left, Width = 216, Padding = new Padding(12, 16, 12, 12) };
  readonly Panel content = new() { Dock = DockStyle.Fill, Padding = new Padding(24, 20, 24, 12) };
  readonly PictureBox logo = new() { Size = new(36, 36), SizeMode = PictureBoxSizeMode.Zoom, Location = new(14, 18) };
  readonly Label brand = new() { AutoSize = true, Location = new(58, 18), Font = Theme.UI(14f, FontStyle.Bold), Text = "BareWin" };
  readonly Label ver = new() { AutoSize = true, Location = new(60, 42), Font = Theme.UI(8.5f) };
  readonly Btn navApps = new() { Style = Btn.Kind.Nav, Glyph = "\uE71D" }, navDeb = new() { Style = Btn.Kind.Nav, Glyph = "\uE90F" }, navAbout = new() { Style = Btn.Kind.Nav, Glyph = "\uE946" };
  readonly Btn langBtn = new(), themeBtn = new();
  readonly Panel pApps = new() { Dock = DockStyle.Fill }, pDeb = new() { Dock = DockStyle.Fill }, pAbout = new() { Dock = DockStyle.Fill };
  // apps
  readonly Label appsTitle = H1(), appsSub = Sub(), appsCount = Sub();
  readonly Card searchCard = new() { Dock = DockStyle.Top, Height = 40, Radius = 10 };
   readonly TextBox busca = new() { BorderStyle = BorderStyle.None, Font = Theme.UI(10.5f) };
  readonly FlatList lista = new() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, CheckBoxes = true, OwnerDraw = true, BorderStyle = BorderStyle.None, HeaderStyle = ColumnHeaderStyle.Clickable, Font = Theme.UI(9.5f) };
  readonly Card listCard = new() { Dock = DockStyle.Fill, Padding = new Padding(1, 6, 1, 6) };
  readonly Btn btnUninst = new() { Style = Btn.Kind.Primary, Width = 220, Dock = DockStyle.Right }, btnRefresh = new() { Width = 110, Dock = DockStyle.Right };
  readonly ImageList icones = new() { ImageSize = new Size(20, 20), ColorDepth = ColorDepth.Depth32Bit };
  readonly ImageList rowH = new() { ImageSize = new Size(1, Theme.D(34)) };
  // debloat
  readonly Label debTitle = H1(), debSub = Sub();
  readonly FlowLayoutPanel debList = new() { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(0, 4, 0, 4) };
  readonly List<(Card card, Label lbl, Switch sw)> rows = new();
  readonly Btn btnApply = new() { Style = Btn.Kind.Primary, Width = 220, Dock = DockStyle.Right };
  // about
  readonly Label aboutTitle = new() { AutoSize = true, Font = Theme.UI(22f, FontStyle.Bold), Text = "BareWin" }, aboutSub = Sub(), aboutBody = new() { AutoSize = true, Font = Theme.UI(10.5f) };
  readonly LinkLabel gh = new() { AutoSize = true, Font = Theme.UI(10f), Text = "github.com/kkauaps" };
  readonly PictureBox aboutIcon = new() { Size = new(96, 96), SizeMode = PictureBoxSizeMode.Zoom };
  // terminal
  readonly Panel term = new() { Dock = DockStyle.Bottom, Height = 220 };
  readonly Panel termBar = new() { Dock = DockStyle.Top, Height = 34, Padding = new Padding(12, 5, 8, 5) };
  readonly Label termTitle = new() { AutoSize = false, Dock = DockStyle.Left, Width = 110, TextAlign = ContentAlignment.MiddleLeft, Font = Theme.UI(9f, FontStyle.Bold) };
  readonly Label status = new() { AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = Theme.UI(9f) };
  readonly Btn tCopy = new() { Width = 72, Dock = DockStyle.Right, Height = 24 }, tClear = new() { Width = 72, Dock = DockStyle.Right, Height = 24 }, tToggle = new() { Width = 34, Dock = DockStyle.Right, Height = 24 };
  readonly RichTextBox termBox = new() { Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.None, WordWrap = false, DetectUrls = false, ScrollBars = RichTextBoxScrollBars.Both };
  readonly Panel termWrap = new() { Dock = DockStyle.Fill, Padding = new Padding(12, 6, 6, 6) };
  List<InstalledApp> todos = new();
  int sortCol = 0; bool sortAsc = true, busy;
  Panel? current;

  static Label H1() => new() { AutoSize = true, Font = Theme.UI(18f, FontStyle.Bold) };
  static Label Sub() => new() { AutoSize = true, Font = Theme.UI(9.75f) };
  static Panel Gap(int h) => new() { Dock = DockStyle.Top, Height = h };

  public MainForm() {
    AutoScaleDimensions = new SizeF(96f, 96f); AutoScaleMode = AutoScaleMode.Dpi;
    L.En = cfg.En; Theme.P = cfg.Dark ? Theme.DarkP : Theme.LightP;
    Width = 1200; Height = 800; MinimumSize = new Size(960, 620); StartPosition = FormStartPosition.CenterScreen; Font = Theme.UI(9.5f);
    DoubleBuffered = true;
    using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("app.ico")) if (s != null) { Icon = new Icon(s); logo.Image = new Icon(Icon, 64, 64).ToBitmap(); aboutIcon.Image = new Icon(Icon, 256, 256).ToBitmap(); }
    ver.Text = "v" + (Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "");
    Runner.Log = Log;

    // sidebar
    var nav = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 140, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(0) };
    foreach (var b in new[] { navApps, navDeb, navAbout }) { b.Width = 190; b.Height = 40; b.Margin = new Padding(0, 0, 0, 4); nav.Controls.Add(b); }
    var head = new Panel { Dock = DockStyle.Top, Height = 84 }; head.Controls.AddRange(new Control[] { logo, brand, ver });
    void LayHead() { int lh = logo.Height, bh = brand.PreferredHeight, vh = ver.PreferredHeight, th = bh + vh - 2, top = (head.ClientSize.Height - Math.Max(lh, th)) / 2; logo.Location = new(Theme.D(14), top + (Math.Max(lh, th) - lh) / 2); int tx = logo.Right + Theme.D(10), ty = top + (Math.Max(lh, th) - th) / 2; brand.Location = new(tx - 2, ty); ver.Location = new(tx, ty + bh - 2); }
    head.Layout += (_, _) => LayHead(); head.Resize += (_, _) => LayHead();
    var bottom = new TableLayoutPanel { Dock = DockStyle.Bottom, Height = 36, ColumnCount = 2 };
    bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
    langBtn.Dock = themeBtn.Dock = DockStyle.Fill; langBtn.Margin = new Padding(0, 0, 3, 0); themeBtn.Margin = new Padding(3, 0, 0, 0);
    bottom.Controls.Add(langBtn, 0, 0); bottom.Controls.Add(themeBtn, 1, 0);
    side.Controls.Add(nav); side.Controls.Add(head); side.Controls.Add(bottom);
    navApps.Click += (_, _) => Show(pApps); navDeb.Click += (_, _) => Show(pDeb); navAbout.Click += (_, _) => Show(pAbout);
    langBtn.Click += (_, _) => { L.En = cfg.En = !cfg.En; cfg.Save(); Texts(); Carregar(false); };
    themeBtn.Click += (_, _) => { cfg.Dark = !cfg.Dark; cfg.Save(); Theme.P = cfg.Dark ? Theme.DarkP : Theme.LightP; Paint_(); };

    // apps page
    foreach (var w in new[] { 330, 180, 130, 84, 56, 300 }) lista.Columns.Add("", Theme.D(w));
    lista.ClientSizeChanged += (_, _) => FitLastCol(); lista.ColumnWidthChanged += (_, e) => { if (e.ColumnIndex != 5) FitLastCol(); };
    lista.SmallImageList = rowH;
    lista.DrawColumnHeader += DrawHeader; lista.DrawItem += (_, e) => e.DrawDefault = false; lista.DrawSubItem += DrawCell;
    lista.ColumnClick += (_, e) => { sortAsc = sortCol == e.Column ? !sortAsc : true; sortCol = e.Column; Ordenar(); lista.Invalidate(); };
    lista.ItemChecked += (_, _) => UpdCount(); lista.MouseMove += (_, e) => { var h = lista.GetItemAt(Theme.D(40), e.Y); if (h != hot) { var old = hot; hot = h; InvRow(old); InvRow(h); } };
    lista.MouseLeave += (_, _) => { var old = hot; hot = null; InvRow(old); };
    lista.MouseDoubleClick += (_, e) => { var it = lista.HitTest(e.Location).Item; if (it != null) it.Checked = !it.Checked; };
    lista.HandleCreated += (_, _) => { Theme.Scrollbars(lista); FitLastCol(); };
    listCard.Controls.Add(lista);
    var glass = new Label { Text = "\uE721", Font = new Font("Segoe MDL2 Assets", 11f), AutoSize = true, BackColor = Color.Transparent };
    searchCard.Controls.Add(busca); searchCard.Controls.Add(glass);
    void LaySearch() { int h = searchCard.ClientSize.Height, x = Theme.D(14); glass.Location = new(x, (h - glass.PreferredHeight) / 2 + 1);
      int bx = x + glass.PreferredWidth + Theme.D(8); busca.SetBounds(bx, (h - busca.Height) / 2, Math.Max(10, searchCard.ClientSize.Width - bx - x), busca.Height); }
    searchCard.Resize += (_, _) => LaySearch(); searchCard.Layout += (_, _) => LaySearch();
    busca.TextChanged += (_, _) => Carregar(false);
    var appsBar = new Panel { Dock = DockStyle.Bottom, Height = 52, Padding = new Padding(0, 12, 0, 0) };
    appsCount.AutoSize = false; appsCount.Dock = DockStyle.Fill; appsCount.TextAlign = ContentAlignment.MiddleLeft;
    appsBar.Controls.Add(appsCount); appsBar.Controls.Add(btnRefresh); appsBar.Controls.Add(new Panel { Dock = DockStyle.Right, Width = 8 }); appsBar.Controls.Add(btnUninst);
    btnUninst.Click += async (_, _) => await Desinstalar(); btnRefresh.Click += (_, _) => Carregar();
    Stack(pApps, listCard, Gap(12), searchCard, Gap(16), appsSub, Gap(2), appsTitle); pApps.Controls.Add(appsBar);

    // debloat page
    foreach (var t in Debloat.Itens) {
      var c = new Card { Height = 56, Width = 600, Margin = new Padding(0, 0, 0, 8), Padding = new Padding(18, 0, 16, 0) };
      var lbl = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = Theme.UI(10f), BackColor = Color.Transparent };
      var sw = new Switch { Checked = t.Padrao, Anchor = AnchorStyles.Right };
      var holder = new Panel { Dock = DockStyle.Right, Width = Theme.D(56), BackColor = Color.Transparent }; holder.Controls.Add(sw); sw.Size = new(Theme.D(40), Theme.D(22)); sw.Anchor = AnchorStyles.None;
      void LaySw() { sw.Location = new(holder.ClientSize.Width - sw.Width, (holder.ClientSize.Height - sw.Height) / 2); } holder.Layout += (_, _) => LaySw(); holder.Resize += (_, _) => LaySw();
      c.Controls.Add(lbl); c.Controls.Add(holder); lbl.Click += (_, _) => sw.Checked = !sw.Checked;
      debList.Controls.Add(c); rows.Add((c, lbl, sw));
    }
    debList.Resize += (_, _) => { foreach (var r in rows) r.card.Width = debList.ClientSize.Width - debList.Padding.Horizontal - (debList.VerticalScroll.Visible ? 2 : 0); };
    debList.HandleCreated += (_, _) => Theme.Scrollbars(debList);
    var debBar = new Panel { Dock = DockStyle.Bottom, Height = 52, Padding = new Padding(0, 12, 0, 0) }; debBar.Controls.Add(btnApply);
    btnApply.Click += async (_, _) => await AplicarDebloat();
    Stack(pDeb, debList, Gap(14), debSub, Gap(2), debTitle); pDeb.Controls.Add(debBar);

    // about page (no wordmark — icon only)
    var aboutFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(8, 24, 8, 8) };
    aboutFlow.Controls.AddRange(new Control[] { aboutIcon, aboutTitle, aboutSub, aboutBody, gh });
    aboutTitle.Margin = new Padding(0, 16, 0, 0); aboutBody.Margin = new Padding(0, 18, 0, 0); gh.Margin = new Padding(0, 18, 0, 0); aboutSub.Text = ver.Text;
    gh.LinkClicked += (_, _) => { try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://github.com/kkauaps") { UseShellExecute = true }); } catch { } };
    pAbout.Controls.Add(aboutFlow);

    // terminal
    termBar.Controls.Add(status); termBar.Controls.Add(termTitle); termBar.Controls.Add(tCopy); termBar.Controls.Add(new Panel { Dock = DockStyle.Right, Width = 6 }); termBar.Controls.Add(tClear); termBar.Controls.Add(new Panel { Dock = DockStyle.Right, Width = 6 }); termBar.Controls.Add(tToggle);
    termWrap.Controls.Add(termBox); term.Controls.Add(termWrap); term.Controls.Add(termBar);
    termBox.Font = Theme.Mono(9.5f); termBox.HandleCreated += (_, _) => Theme.Scrollbars(termBox);
    tCopy.Click += (_, _) => { if (termBox.TextLength > 0) Clipboard.SetText(termBox.Text); };
    tClear.Click += (_, _) => termBox.Clear();
    tToggle.Click += (_, _) => { cfg.TermOpen = !cfg.TermOpen; cfg.Save(); Term(); };
    termBar.Paint += (_, e) => { using var pen = new Pen(Theme.P.Border); e.Graphics.DrawLine(pen, 0, 0, termBar.Width, 0); };
    termBar.DoubleClick += (_, _) => { cfg.TermOpen = !cfg.TermOpen; cfg.Save(); Term(); };

    content.Controls.AddRange(new Control[] { pApps, pDeb, pAbout });
    Controls.Add(content); Controls.Add(term); Controls.Add(side);
    side.Paint += (_, e) => { using var pen = new Pen(Theme.P.Border); e.Graphics.DrawLine(pen, side.Width - 1, 0, side.Width - 1, side.Height); };
    HandleCreated += (_, _) => Theme.TitleBar(this);
    Texts(); Paint_(); Term(); Show(pApps);
    Load += (_, _) => { Carregar(); Log(L.T("ready")); };
  }
  static void Stack(Panel host, params Control[] topToBottomReversed) { foreach (var c in topToBottomReversed) { if (c is Label l && l.AutoSize) l.Dock = DockStyle.Top; else if (c.Dock == DockStyle.None) c.Dock = DockStyle.Top; host.Controls.Add(c); } }
  void Show(Panel p) { current = p; foreach (var x in new[] { pApps, pDeb, pAbout }) x.Visible = x == p; navApps.Active = p == pApps; navDeb.Active = p == pDeb; navAbout.Active = p == pAbout; foreach (var b in new[] { navApps, navDeb, navAbout }) b.Invalidate(); }
  void Term() { term.Height = cfg.TermOpen ? 230 : 34; termWrap.Visible = cfg.TermOpen; tToggle.Text = cfg.TermOpen ? "▾" : "▴"; tToggle.Invalidate(); }

  void Texts() {
    Text = L.En ? "BareWin — Uninstaller + Debloat" : "BareWin — Desinstalador + Debloat";
    navApps.Text = L.T("nav.apps"); navDeb.Text = L.T("nav.debloat"); navAbout.Text = L.T("nav.about");
    langBtn.Text = L.En ? "EN → PT" : "PT → EN"; themeBtn.Text = cfg.Dark ? "☀ " + L.T("theme.light") : "☾ " + L.T("theme.dark");
    appsTitle.Text = L.T("apps.title"); appsSub.Text = L.T("apps.sub"); busca.PlaceholderText = L.T("apps.search");
    string[] k = { "col.name", "col.pub", "col.date", "col.size", "col.disk", "col.loc" }; for (int i = 0; i < k.Length; i++) lista.Columns[i].Text = L.T(k[i]);
    btnUninst.Text = L.T("apps.btn"); btnRefresh.Text = L.T("apps.refresh");
    debTitle.Text = L.T("deb.title"); debSub.Text = L.T("deb.sub"); btnApply.Text = L.T("deb.btn");
    for (int i = 0; i < rows.Count; i++) rows[i].lbl.Text = Debloat.Itens[i].Titulo;
    aboutBody.Text = L.T("about.sub") + "\n\n" + L.T("about.body");
    termTitle.Text = "›_  " + L.T("term"); tCopy.Text = L.T("term.copy"); tClear.Text = L.T("term.clear");
    if (!busy) status.Text = L.T("ready");
    foreach (Control c in new Control[] { navApps, navDeb, navAbout, langBtn, themeBtn, btnUninst, btnRefresh, btnApply, tCopy, tClear, lista }) c.Invalidate();
  }
  void Paint_() {
    var p = Theme.P;
    BackColor = content.BackColor = pApps.BackColor = pDeb.BackColor = pAbout.BackColor = p.Bg; ForeColor = p.Text;
    side.BackColor = p.Side; foreach (Control c in side.Controls) Recolor(c, p.Side, p.Text);
    ver.ForeColor = p.Muted;
    foreach (var l in new[] { appsTitle, debTitle, aboutTitle, aboutBody }) l.ForeColor = p.Text;
    foreach (var l in new[] { appsSub, debSub, aboutSub, appsCount }) l.ForeColor = p.Muted;
    foreach (Control c in pAbout.Controls) c.BackColor = p.Bg;
    gh.LinkColor = gh.ActiveLinkColor = p.Accent; gh.BackColor = p.Bg;
    busca.BackColor = p.Surface; busca.ForeColor = p.Text; foreach (Control c in searchCard.Controls) if (c is Label l) l.ForeColor = p.Muted;
    lista.BackColor = p.Surface; lista.ForeColor = p.Text; listCard.BackColor = p.Bg;
    foreach (var r in rows) { r.lbl.ForeColor = p.Text; r.card.Invalidate(); }
    debList.BackColor = p.Bg;
    term.BackColor = termWrap.BackColor = termBox.BackColor = p.TermBg; termBar.BackColor = p.Dark ? Color.FromArgb(0x11, 0x14, 0x1A) : Color.FromArgb(0x16, 0x1A, 0x22);
    termTitle.ForeColor = Color.FromArgb(0xE6, 0xE8, 0xEE); status.ForeColor = Color.FromArgb(0x8B, 0x93, 0xA3);
    foreach (Control c in termBar.Controls) if (c is Panel) c.BackColor = termBar.BackColor;
    foreach (Control c in pApps.Controls) if (c is Panel pp && pp is not Card) { pp.BackColor = p.Bg; foreach (Control cc in pp.Controls) if (cc is Panel) cc.BackColor = p.Bg; }
    foreach (Control c in pDeb.Controls) if (c is Panel pp && pp is not Card) pp.BackColor = p.Bg;
    Theme.TitleBar(this); Theme.Scrollbars(lista); if (lista.IsHandleCreated) lista.ThemeHeader(); Theme.Scrollbars(debList);
    Texts(); Invalidate(true);
  }
  static void Recolor(Control c, Color bg, Color fg) { if (c is not Btn) { c.BackColor = bg; if (c is Label) c.ForeColor = fg; } foreach (Control x in c.Controls) Recolor(x, bg, fg); }

  // ---------- list drawing ----------
  ListViewItem? hot;
  void InvRow(ListViewItem? it) { if (it != null && it.ListView == lista) lista.Invalidate(new Rectangle(0, it.Bounds.Y, lista.ClientSize.Width, it.Bounds.Height)); }
  bool fitting;
  void FitLastCol() {
    if (fitting || lista.Columns.Count < 6 || !lista.IsHandleCreated) return; fitting = true;
    int used = 0; for (int i = 0; i < 5; i++) used += lista.Columns[i].Width;
    lista.Columns[5].Width = Math.Max(Theme.D(160), lista.ClientSize.Width - used); fitting = false; lista.Invalidate();
  }
  void DrawHeader(object? s, DrawListViewColumnHeaderEventArgs e) {
    var p = Theme.P; var fb = e.Bounds; if (e.ColumnIndex == lista.Columns.Count - 1) fb.Width = Math.Max(fb.Width, lista.ClientSize.Width - fb.X + Theme.D(40));
    using (var b = new SolidBrush(p.Surface)) e.Graphics.FillRectangle(b, fb);
    using (var pen = new Pen(p.Border)) e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
    var t = e.Header!.Text + (e.ColumnIndex == sortCol ? (sortAsc ? "  ↑" : "  ↓") : "");
    var r = e.Bounds; r.X += e.ColumnIndex == 0 ? Theme.D(40) : Theme.D(10); r.Width -= 12;
    TextRenderer.DrawText(e.Graphics, t.ToUpperInvariant(), Theme.UI(8f, FontStyle.Bold), r, p.Muted, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.Left);
  }
  void DrawCell(object? s, DrawListViewSubItemEventArgs e) {
    var p = Theme.P; var g = e.Graphics; var it = e.Item!;
    var bg = it.Checked ? Color.FromArgb(p.Dark ? 40 : 28, p.Accent) : it == hot ? p.Surface2 : p.Surface;
    using (var b = new SolidBrush(p.Surface)) g.FillRectangle(b, e.Bounds);
    using (var b = new SolidBrush(bg)) g.FillRectangle(b, e.Bounds);
    var r = e.Bounds; var col = p.Text; var font = lista.Font;
    if (e.ColumnIndex == 0) {
      g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
      float k = Theme.S, cs = 16 * k; var cb = new RectangleF(r.X + 8 * k, r.Y + (r.Height - cs) / 2f, cs, cs);
      using (var path = Theme.Round(cb, 4 * k)) {
        if (it.Checked) { using var b = new SolidBrush(p.Accent); g.FillPath(b, path); using var pen = new Pen(Color.White, 2f * k);
          g.DrawLines(pen, new[] { new PointF(cb.X + 4 * k, cb.Y + 8.5f * k), new PointF(cb.X + 7 * k, cb.Y + 11.5f * k), new PointF(cb.X + 12.5f * k, cb.Y + 5 * k) }); }
        else { using var pen = new Pen(p.Muted, 1.2f); g.DrawPath(pen, path); }
      }
      var key = it.ImageKey; if (!string.IsNullOrEmpty(key) && icones.Images.ContainsKey(key)) { int isz = Theme.D(20); g.DrawImage(icones.Images[key]!, r.X + Theme.D(34), r.Y + (r.Height - isz) / 2, isz, isz); }
      r.X += Theme.D(62); r.Width -= Theme.D(62); font = Theme.UI(9.5f, FontStyle.Bold);
    } else { r.X += 10; r.Width -= 12; if (e.ColumnIndex != 3) col = p.Muted; }
    TextRenderer.DrawText(g, e.SubItem!.Text, font, r, col, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.Left | TextFormatFlags.NoPrefix
      | (e.ColumnIndex == 3 ? TextFormatFlags.Right : 0));
  }

  // ---------- terminal ----------
  void Log(string s) {
    if (InvokeRequired) { BeginInvoke(() => Log(s)); return; }
    var p = Theme.P; var muted = Color.FromArgb(0x6E, 0x76, 0x81);
    Color c = s.StartsWith("> ") ? p.Cmd : s.StartsWith("ERR") ? Color.FromArgb(0xFF, 0x7B, 0x72) : s.StartsWith("== ") ? Color.FromArgb(0xF5, 0xC4, 0x51)
      : s.StartsWith("[exit 0]") ? Color.FromArgb(0x3D, 0xDC, 0x97) : s.StartsWith("[exit") ? Color.FromArgb(0xFF, 0x7B, 0x72)
      : s.StartsWith("AVISO") || s.StartsWith("WARN") ? Color.FromArgb(0xF5, 0xC4, 0x51) : p.TermText;
    Append($"{DateTime.Now:HH:mm:ss}  ", muted, false);
    Append((s.StartsWith("> ") ? "❯ " + s[2..] : s) + "\n", c, s.StartsWith("== "));
    termBox.SelectionStart = termBox.TextLength; termBox.ScrollToCaret();
    if (!s.StartsWith(">") && !s.StartsWith("[exit")) status.Text = s.Length > 140 ? s[..140] + "…" : s;
  }
  void Append(string t, Color c, bool bold) {
    termBox.SelectionStart = termBox.TextLength; termBox.SelectionLength = 0; termBox.SelectionColor = c;
    termBox.SelectionFont = bold ? new Font(termBox.Font, FontStyle.Bold) : termBox.Font; termBox.AppendText(t);
  }
  void Status(string s) { status.Text = s; }

  // ---------- data ----------
  static bool Bate(InstalledApp a, string q) => q.Length == 0 || q.Split(' ', StringSplitOptions.RemoveEmptyEntries).All(t =>
    a.Name.Contains(t, StringComparison.CurrentCultureIgnoreCase) || (a.Publisher?.Contains(t, StringComparison.CurrentCultureIgnoreCase) ?? false) || (a.InstallLocation?.Contains(t, StringComparison.CurrentCultureIgnoreCase) ?? false));
  void Carregar(bool reload = true) {
    var marcados = lista.CheckedItems.Cast<ListViewItem>().Select(i => ((InstalledApp)i.Tag!).Name).ToHashSet();
    lista.BeginUpdate(); lista.Items.Clear();
    if (reload) { todos = Apps.List(); icones.Images.Clear(); }
    var q = busca.Text.Trim();
    foreach (var a in todos.Where(x => Bate(x, q))) {
      string key = "";
      if (a.IconPath != null) { key = a.IconPath.ToLowerInvariant(); if (!icones.Images.ContainsKey(key)) try { using var ic = Icon.ExtractAssociatedIcon(a.IconPath); if (ic != null) icones.Images.Add(key, ic.ToBitmap()); else key = ""; } catch { key = ""; } }
      lista.Items.Add(new ListViewItem(new[] { a.Name, a.Publisher ?? "", a.Installed?.ToString(L.En ? "yyyy-MM-dd HH:mm" : "dd/MM/yyyy HH:mm") ?? "", a.SizeText, a.Disk, a.InstallLocation ?? "" }) { Tag = a, ImageKey = key, Checked = marcados.Contains(a.Name) });
    }
    lista.EndUpdate(); Ordenar(); UpdCount();
  }
  void UpdCount() {
    var n = lista.Items.Count; var q = busca.Text.Trim().Length > 0;
    var t = q ? L.F("apps.countf", n, todos.Count) : L.F("apps.count", todos.Count);
    var sel = lista.CheckedItems.Count; appsCount.Text = sel > 0 ? t + "  ·  " + L.F("apps.sel", sel) : t; btnUninst.Enabled = sel > 0 && !busy;
  }
  void Ordenar() {
    lista.ListViewItemSorter = Comparer<ListViewItem>.Create((x, y) => {
      var a = (InstalledApp)x.Tag!; var b = (InstalledApp)y.Tag!;
      int r = sortCol == 2 ? Nullable.Compare(a.Installed, b.Installed) : sortCol == 3 ? Nullable.Compare(a.SizeKb, b.SizeKb) : string.Compare(x.SubItems[sortCol].Text, y.SubItems[sortCol].Text, StringComparison.CurrentCultureIgnoreCase);
      return sortAsc ? r : -r; }) as System.Collections.IComparer;
  }
  bool Begin() { if (busy) { Status(L.T("busy")); return false; } busy = true; btnApply.Enabled = btnUninst.Enabled = btnRefresh.Enabled = false; if (!cfg.TermOpen) { cfg.TermOpen = true; Term(); } return true; }
  void End() { busy = false; btnApply.Enabled = btnRefresh.Enabled = true; UpdCount(); }
  async Task Desinstalar() {
    var sel = lista.CheckedItems.Cast<ListViewItem>().Select(i => (InstalledApp)i.Tag!).ToList();
    if (sel.Count == 0 || !Confirm(L.F("apps.confirm", sel.Count))) return;
    if (!Begin()) return;
    try {
      foreach (var a in sel) {
        Status(L.F("apps.removing", a.Name)); Log($"== {(L.En ? "Uninstalling" : "Desinstalando")} {a.Name}"); await Apps.Uninstall(a);
        var achados = await Task.Run(() => Leftovers.Find(a)); Log(L.F("apps.left", achados.Count));
        if (achados.Count == 0) continue;
        var escolhidos = EscolherRestos(a, achados);
        if (escolhidos.Count > 0) Log(L.F("apps.leftdone", a.Name, Leftovers.Delete(escolhidos)));
      }
    } finally { End(); }
    Carregar();
  }
  bool Confirm(string msg) {
    using var f = Dialog("BareWin", 460, 200);
    var l = new Label { Dock = DockStyle.Fill, Text = msg, Font = Theme.UI(11f), ForeColor = Theme.P.Text, Padding = new Padding(20, 24, 20, 0) };
    var bar = Buttons(f, L.En ? "Yes, continue" : "Sim, continuar", L.En ? "Cancel" : "Cancelar");
    f.Controls.Add(l); f.Controls.Add(bar); return f.ShowDialog(this) == DialogResult.OK;
  }
  Form Dialog(string title, int w, int h) {
    var f = new Form { Text = title, Width = w, Height = h, StartPosition = FormStartPosition.CenterParent, Icon = Icon, BackColor = Theme.P.Bg, ForeColor = Theme.P.Text,
      FormBorderStyle = FormBorderStyle.Sizable, MinimizeBox = false, ShowInTaskbar = false, Font = Theme.UI(9.5f) };
    f.HandleCreated += (_, _) => Theme.TitleBar(f); return f;
  }
  Panel Buttons(Form f, string ok, string cancel, Btn? extra = null) {
    var bar = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(16, 12, 16, 12), BackColor = Theme.P.Bg };
    var bOk = new Btn { Style = Btn.Kind.Primary, Text = ok, Width = 180, Dock = DockStyle.Right }; var bNo = new Btn { Text = cancel, Width = 130, Dock = DockStyle.Right };
    bOk.Click += (_, _) => { f.DialogResult = DialogResult.OK; f.Close(); }; bNo.Click += (_, _) => { f.DialogResult = DialogResult.Cancel; f.Close(); };
    bar.Controls.Add(bNo); bar.Controls.Add(new Panel { Dock = DockStyle.Right, Width = 8 }); bar.Controls.Add(bOk);
    if (extra != null) { extra.Dock = DockStyle.Left; extra.Width = 180; bar.Controls.Add(extra); }
    return bar;
  }
  List<Leftover> EscolherRestos(InstalledApp a, List<Leftover> itens) {
    using var f = Dialog(L.F("left.title", a.Name), 860, 480);
    var cl = new CheckedListBox { Dock = DockStyle.Fill, CheckOnClick = true, HorizontalScrollbar = true, BorderStyle = BorderStyle.None,
      BackColor = Theme.P.Surface, ForeColor = Theme.P.Text, Font = Theme.Mono(9.5f), IntegralHeight = false };
    cl.HandleCreated += (_, _) => Theme.Scrollbars(cl);
    foreach (var i in itens) cl.Items.Add(i, true);
    var card = new Card { Dock = DockStyle.Fill, Padding = new Padding(12, 10, 12, 10) }; card.Controls.Add(cl);
    var wrap = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 16, 0), BackColor = Theme.P.Bg }; wrap.Controls.Add(card);
    var info = new Label { Dock = DockStyle.Top, Height = 48, Text = L.T("left.info"), ForeColor = Theme.P.Muted, Padding = new Padding(16, 16, 16, 0), Font = Theme.UI(10f) };
    var all = new Btn { Text = L.T("left.all") }; all.Click += (_, _) => { bool v = cl.CheckedItems.Count < cl.Items.Count; for (int i = 0; i < cl.Items.Count; i++) cl.SetItemChecked(i, v); };
    f.Controls.Add(wrap); f.Controls.Add(info); f.Controls.Add(Buttons(f, L.T("left.ok"), L.T("left.cancel"), all));
    return f.ShowDialog(this) == DialogResult.OK ? cl.CheckedItems.Cast<Leftover>().ToList() : new();
  }
  async Task AplicarDebloat() {
    if (!Begin()) return;
    try {
      Status(L.T("deb.restore")); await Debloat.CriarPontoRestauracao();
      for (int i = 0; i < rows.Count; i++) if (rows[i].sw.Checked) { Status(L.F("deb.applying", Debloat.Itens[i].Titulo)); await Debloat.Aplicar(Debloat.Itens[i]); }
      Log(L.T("deb.done"));
    } finally { End(); }
  }
}
