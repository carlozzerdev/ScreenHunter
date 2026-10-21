using ScreenHunter.Application;
using ScreenHunter.Infrastructure.Windows;

namespace ScreenHunter.Presentation.WinForms;

internal static class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        System.Windows.Forms.Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        System.Windows.Forms.Application.EnableVisualStyles();
        System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
        System.Windows.Forms.Application.SetDefaultFont(new Font("Segoe UI", 10));
        if (args.Contains("--self-test")) return SelfTest.Run(args);
        var l = new L10n(); l.Set("es");
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000) || !Environment.Is64BitProcess) { MessageBox.Show(l["UnsupportedOS"]); return 1; }
        var store = new PreferencesStore(Path.Combine(AppContext.BaseDirectory, "settings.json")); Preferences preferences; bool recovered = false;
        try { preferences = store.Load(); } catch { preferences = new(); recovered = true; }
        l.Set(preferences.Language); var session = new EditorSession();
        using var form = new MainForm(session, l);
        using var presenter = new EditorPresenter(form, session, new DocumentStore(), store, new GlobalHotkey(form.Handle), new DesktopPlatform(), new ImageOutput(), preferences);
        form.Shown += (_, _) =>
        {
            if (recovered) MessageBox.Show(form, l["SettingsRecovered"], "ScreenHunter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            if (!presenter.RegisterHotkey()) MessageBox.Show(form, l["HotkeyConflict"], "ScreenHunter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        };
        System.Windows.Forms.Application.Run(form); return 0;
    }
}
