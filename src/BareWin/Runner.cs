using System.Diagnostics;
using System.Text;
namespace BareWin;
// Runs a process and streams every output line to the log callback (mini terminal).
public static class Runner {
  public static Action<string> Log = _ => { };
  public static async Task<int> Run(string exe, string args, string? shown = null) {
    Log($"> {shown ?? exe + " " + args}");
    var psi = new ProcessStartInfo(exe, args) { UseShellExecute = false, CreateNoWindow = true,
      RedirectStandardOutput = true, RedirectStandardError = true,
      StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8 };
    using var p = new Process { StartInfo = psi };
    p.OutputDataReceived += (_, e) => { if (e.Data != null) Log(e.Data); };
    p.ErrorDataReceived += (_, e) => { if (e.Data != null) Log("ERR " + e.Data); };
    try { p.Start(); } catch (Exception ex) { Log("ERR " + ex.Message); return -1; }
    p.BeginOutputReadLine(); p.BeginErrorReadLine();
    await p.WaitForExitAsync(); Log($"[exit {p.ExitCode}]"); return p.ExitCode;
  }
  public static Task<int> Ps(string script) {
    var enc = Convert.ToBase64String(Encoding.Unicode.GetBytes("$ProgressPreference='SilentlyContinue';[Console]::OutputEncoding=[Text.Encoding]::UTF8;$OutputEncoding=[Text.Encoding]::UTF8;& {\n" + script + "\n} 2>&1 | % { if ($_ -is [System.Management.Automation.ErrorRecord]) { 'ERR ' + $_.Exception.Message } else { \"$_\" } }"));
    return Run("powershell.exe", $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -EncodedCommand {enc}", "powershell: " + script.Replace("\n", " ; "));
  }
}
