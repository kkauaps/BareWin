using Microsoft.Win32;
using System.Globalization;
using System.Runtime.InteropServices;
namespace BareWin;
public record InstalledApp(string Name, string? Publisher, string? UninstallString, string? QuietUninstallString,
  string? InstallLocation, DateTime? Installed, string RegPath, string? IconPath = null, long? SizeKb = null) {
  public string Disk => !string.IsNullOrWhiteSpace(InstallLocation) && InstallLocation.Length > 1 && InstallLocation[1] == ':' ? InstallLocation[..2].ToUpper() : "";
  public string SizeText => SizeKb is long k && k > 0 ? (k >= 1048576 ? $"{k / 1048576.0:0.0} GB" : k >= 1024 ? $"{k / 1024.0:0.0} MB" : $"{k} KB") : "";
}
public static class Apps {
  static readonly (RegistryHive, RegistryView, string)[] Roots = {
    (RegistryHive.LocalMachine, RegistryView.Registry64, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),
    (RegistryHive.LocalMachine, RegistryView.Registry32, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),
    (RegistryHive.CurrentUser,  RegistryView.Default,    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall") };
  [DllImport("advapi32.dll")] static extern int RegQueryInfoKey(Microsoft.Win32.SafeHandles.SafeRegistryHandle h, IntPtr c, IntPtr cc, IntPtr r,
    IntPtr sk, IntPtr msk, IntPtr mc, IntPtr v, IntPtr mvn, IntPtr mvl, IntPtr sd, out long lastWrite);
  // InstallDate (yyyyMMdd) gives the day; key last-write time gives the hour. Prefer last-write when same day.
  static DateTime? When(RegistryKey k) {
    DateTime? lw = RegQueryInfoKey(k.Handle, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, out var ft) == 0 ? DateTime.FromFileTime(ft) : null;
    if (k.GetValue("InstallDate") is string d && DateTime.TryParseExact(d.Trim(), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var day))
      return lw?.Date == day ? lw : day;
    return lw;
  }
  public static List<InstalledApp> List() {
    var r = new Dictionary<string, InstalledApp>(StringComparer.OrdinalIgnoreCase);
    foreach (var (h, v, p) in Roots) {
      using var k = RegistryKey.OpenBaseKey(h, v).OpenSubKey(p); if (k == null) continue;
      foreach (var s in k.GetSubKeyNames()) { using var a = k.OpenSubKey(s);
        var n = a?.GetValue("DisplayName") as string;
        if (string.IsNullOrWhiteSpace(n) || (a!.GetValue("SystemComponent") is int sc && sc == 1)) continue;
        var hive = h == RegistryHive.LocalMachine ? "HKLM" : "HKCU";
        r.TryAdd(n, new(n, a.GetValue("Publisher") as string, a.GetValue("UninstallString") as string,
          a.GetValue("QuietUninstallString") as string, a.GetValue("InstallLocation") as string, When(a),
          $@"{hive}{(v == RegistryView.Registry32 ? "32" : "")}\{p}\{s}", IconFile(a), a.GetValue("EstimatedSize") is int es ? es : null)); } }
    AddFolders(r);
    return r.Values.OrderBy(x => x.Name).ToList();
  }
  // Apps sem entrada de Uninstall (ex.: Ollama em %LOCALAPPDATA%\Programs): varre pastas de programas.
  static void AddFolders(Dictionary<string, InstalledApp> r) {
    var known = new HashSet<string>(r.Values.Where(a => !string.IsNullOrWhiteSpace(a.InstallLocation))
      .Select(a => Path.TrimEndingDirectorySeparator(Environment.ExpandEnvironmentVariables(a.InstallLocation!.Trim().Trim('"')))), StringComparer.OrdinalIgnoreCase);
    var roots = new[] { Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs") }.Where(Directory.Exists).Distinct(StringComparer.OrdinalIgnoreCase);
    var skip = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Common Files", "Windows Defender", "Windows Mail", "Windows Media Player", "Windows NT",
      "Windows Photo Viewer", "Windows Portable Devices", "Windows Security", "WindowsPowerShell", "WindowsApps", "ModifiableWindowsApps", "Internet Explorer", "Microsoft Update Health Tools", "Uninstall Information", "Reference Assemblies", "MSBuild" };
    foreach (var root in roots) {
      string[] dirs; try { dirs = Directory.GetDirectories(root); } catch { continue; }
      foreach (var d in dirs) {
        var name = Path.GetFileName(d); if (skip.Contains(name) || known.Contains(d) || r.ContainsKey(name)) continue;
        if (r.Values.Any(a => a.Name.StartsWith(name, StringComparison.OrdinalIgnoreCase))) continue;
        string[] exes; try { exes = Directory.GetFiles(d, "*.exe"); } catch { continue; }
        if (exes.Length == 0) continue;
        var main = exes.FirstOrDefault(e => !Path.GetFileName(e).StartsWith("unins", StringComparison.OrdinalIgnoreCase)) ?? exes[0];
        var unins = exes.FirstOrDefault(e => Path.GetFileName(e).StartsWith("unins", StringComparison.OrdinalIgnoreCase));
        long kb = 0; try { kb = new DirectoryInfo(d).EnumerateFiles("*", new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true }).Sum(x => x.Length) / 1024; } catch { }
        DateTime? when = null; try { when = Directory.GetCreationTime(d); } catch { }
        r.TryAdd(name, new(name, "(pasta, sem registro)", unins != null ? $"\"{unins}\"" : null, null, d, when, d, main, kb > 0 ? kb : null));
      }
    }
  }
  static string? IconFile(RegistryKey a) {
    string? Clean(string? s) { if (string.IsNullOrWhiteSpace(s)) return null; s = Environment.ExpandEnvironmentVariables(s.Trim());
      if (s.StartsWith("\"")) { var e = s.IndexOf('"', 1); s = e > 0 ? s[1..e] : s.Trim('"'); }
      else { var c = s.LastIndexOf('.'); var x = s.IndexOfAny(new[] { ',', ' ' }, Math.Max(c, 0)); if (x > 0) s = s[..x]; }
      return File.Exists(s) ? s : null; }
    var r = Clean(a.GetValue("DisplayIcon") as string); if (r != null) return r;
    if (a.GetValue("InstallLocation") is string loc && Directory.Exists(loc))
      try { var exe = Directory.EnumerateFiles(loc, "*.exe").FirstOrDefault(f => !Path.GetFileName(f).StartsWith("unins", StringComparison.OrdinalIgnoreCase)); if (exe != null) return exe; } catch { }
    return Clean(a.GetValue("UninstallString") as string);
  }
  public static async Task Uninstall(InstalledApp app) {
    var cmd = app.QuietUninstallString ?? app.UninstallString;
    if (cmd == null) { Runner.Log($"{app.Name}: sem desinstalador registrado"); return; }
    await Runner.Run("cmd.exe", $"/c \"{cmd}\"");
  }
}
