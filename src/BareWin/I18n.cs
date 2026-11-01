namespace BareWin;
public static class L {
  public static bool En;
  static readonly Dictionary<string, (string Pt, string En)> S = new() {
    ["nav.apps"] = ("Aplicativos", "Apps"), ["nav.debloat"] = ("Debloat", "Debloat"), ["nav.about"] = ("Sobre", "About"),
    ["apps.title"] = ("Aplicativos instalados", "Installed apps"),
    ["apps.sub"] = ("Desinstale e revise os restos antes de apagar.", "Uninstall and review leftovers before deleting."),
    ["apps.search"] = ("Pesquisar por nome, editor ou pasta…", "Search by name, publisher or folder…"),
    ["col.name"] = ("Programa", "Program"), ["col.pub"] = ("Editor", "Publisher"), ["col.date"] = ("Instalado em", "Installed"),
    ["col.size"] = ("Tamanho", "Size"), ["col.disk"] = ("Disco", "Drive"), ["col.loc"] = ("Local", "Location"),
    ["apps.btn"] = ("Desinstalar selecionados", "Uninstall selected"), ["apps.refresh"] = ("Atualizar", "Refresh"),
    ["apps.count"] = ("{0} programas", "{0} programs"), ["apps.countf"] = ("{0} de {1} programas", "{0} of {1} programs"),
    ["apps.sel"] = ("{0} selecionado(s)", "{0} selected"),
    ["apps.confirm"] = ("Desinstalar {0} programa(s)?", "Uninstall {0} program(s)?"),
    ["apps.removing"] = ("Removendo {0}…", "Removing {0}…"), ["apps.left"] = ("{0} resto(s) encontrado(s)", "{0} leftover(s) found"),
    ["apps.leftdone"] = ("{0}: {1} resto(s) removido(s).", "{0}: {1} leftover(s) removed."),
    ["left.title"] = ("Restos de {0}", "Leftovers of {0}"),
    ["left.info"] = ("Marque o que deve ser apagado (pastas, arquivos, chaves de registro):", "Check what should be deleted (folders, files, registry keys):"),
    ["left.ok"] = ("Apagar selecionados", "Delete selected"), ["left.all"] = ("Marcar/desmarcar todos", "Toggle all"), ["left.cancel"] = ("Manter tudo", "Keep all"),
    ["deb.title"] = ("Debloat do Windows", "Windows debloat"),
    ["deb.sub"] = ("Tenta criar um ponto de restauração antes de aplicar.", "Tries to create a restore point before applying."),
    ["deb.btn"] = ("Aplicar selecionados", "Apply selected"), ["deb.restore"] = ("Criando ponto de restauração…", "Creating restore point…"),
    ["deb.applying"] = ("Aplicando: {0}", "Applying: {0}"), ["deb.done"] = ("Debloat concluído. Reinicie o PC.", "Debloat finished. Restart your PC."),
    ["about.sub"] = ("Desinstalador + Debloat open source para Windows.", "Open source uninstaller + debloater for Windows."),
    ["about.body"] = ("• Lista apps do registro e pastas soltas (ex.: Ollama)\n• Mostra ícone, tamanho, disco e pasta\n• Revisa os restos antes de apagar\n• Debloat: telemetria, Copilot, Edge, Bing, animações, transparência\n• Terminal ao vivo com todos os comandos executados",
                     "• Lists apps from the registry and loose folders (e.g. Ollama)\n• Shows icon, size, drive and folder\n• Review leftovers before deleting\n• Debloat: telemetry, Copilot, Edge, Bing, animations, transparency\n• Live terminal with every command executed"),
    ["term"] = ("Terminal", "Terminal"), ["term.copy"] = ("Copiar", "Copy"), ["term.clear"] = ("Limpar", "Clear"),
    ["ready"] = ("Pronto.", "Ready."), ["busy"] = ("Ocupado — aguarde a tarefa atual.", "Busy — wait for the current task."),
    ["theme.dark"] = ("Escuro", "Dark"), ["theme.light"] = ("Claro", "Light"),
  };
  public static string T(string k) => S.TryGetValue(k, out var v) ? (En ? v.En : v.Pt) : k;
  public static string F(string k, params object[] a) => string.Format(T(k), a);
}
