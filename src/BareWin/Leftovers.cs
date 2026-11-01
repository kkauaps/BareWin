using Microsoft.Win32;
namespace BareWin;
public record Leftover(string Kind, string Path) { public string KindText => !L.En ? Kind : Kind switch { "pasta" => "folder", "arquivo" => "file", _ => "registry" };
  public override string ToString() => $"[{KindText}] {Path}"; }
public static class Leftovers {
  // Conservative: exact-name folders in well-known locations, InstallLocation, and exact-name registry keys.
  public static List<Leftover> Find(InstalledApp a) {
    var hits = new List<Leftover>();
    var names = new[] { a.Name, a.Publisher }.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
    var bases = new[] { Environment.SpecialFolder.ProgramFiles, Environment.SpecialFolder.ProgramFilesX86,
      Environment.SpecialFolder.ApplicationData, Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolder.CommonApplicationData }
      .Select(Environment.GetFolderPath);
    foreach (var b in bases) foreach (var n in names) { var d = System.IO.Path.Combine(b, n!); if (Directory.Exists(d)) hits.Add(new("pasta", d)); }
    if (!string.IsNullOrWhiteSpace(a.InstallLocation) && Directory.Exists(a.InstallLocation)) hits.Add(new("pasta", a.InstallLocation.TrimEnd('\\')));
    foreach (var f in new[] { Environment.GetFolderPath(Environment.SpecialFolder.Desktop), Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory) })
      { var l = System.IO.Path.Combine(f, a.Name + ".lnk"); if (File.Exists(l)) hits.Add(new("arquivo", l)); }
    var regs = new List<string>();
    foreach (var n in names) { regs.Add($@"HKCU\Software\{n}"); regs.Add($@"HKLM\SOFTWARE\{n}"); regs.Add($@"HKLM\SOFTWARE\WOW6432Node\{n}"); }
    if (a.Publisher != null) { regs.Add($@"HKCU\Software\{a.Publisher}\{a.Name}"); regs.Add($@"HKLM\SOFTWARE\{a.Publisher}\{a.Name}"); }
    regs.Add(a.RegPath);
    foreach (var r in regs.Distinct(StringComparer.OrdinalIgnoreCase)) if (Open(r, out var root, out var sub)) { using var k = root.OpenSubKey(sub); if (k != null) hits.Add(new("registro", r)); }
    return hits.DistinctBy(h => h.Path.ToLowerInvariant()).ToList();
  }
  static bool Open(string p, out RegistryKey root, out string sub) {
    var i = p.IndexOf('\\'); var hive = p[..i]; sub = p[(i + 1)..];
    root = hive switch { "HKCU" => Registry.CurrentUser, "HKLM32" => RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32),
      _ => RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64) };
    return true;
  }
  public static int Delete(IEnumerable<Leftover> items) {
    int ok = 0;
    foreach (var h in items) try {
      if ((h.Kind == "pasta" && !Directory.Exists(h.Path)) || (h.Kind == "arquivo" && !File.Exists(h.Path))) { Runner.Log($"ok {h} (já não existe / already gone)"); ok++; continue; }
      if (h.Kind == "pasta") Directory.Delete(h.Path, true);
      else if (h.Kind == "arquivo") File.Delete(h.Path);
      else { Open(h.Path, out var root, out var sub); root.DeleteSubKeyTree(sub, false); }
      Runner.Log($"removido {h}"); ok++;
    } catch (Exception ex) { Runner.Log($"ERR {h}: {ex.Message}"); }
    return ok;
  }
}
