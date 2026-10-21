using System.Diagnostics;
using System.Drawing.Imaging;
using ScreenHunter.Application;
using ScreenHunter.Domain;
using ScreenHunter.Presentation.WinForms.Rendering;

namespace ScreenHunter.Presentation.WinForms;

public sealed class EditorPresenter : IDisposable
{
    readonly MainForm view; readonly EditorSession session; readonly IDocumentStore store; readonly IPreferencesStore settingsStore; readonly IHotkey hotkey; readonly IDesktopPlatform desktop; readonly IImageOutput output;
    Preferences preferences; string? projectPath; bool disposing;
    L10n L => view.L;
    public EditorPresenter(MainForm view, EditorSession session, IDocumentStore store, IPreferencesStore settingsStore, IHotkey hotkey, IDesktopPlatform desktop, IImageOutput output, Preferences preferences)
    {
        this.view = view; this.session = session; this.store = store; this.settingsStore = settingsStore; this.hotkey = hotkey; this.desktop = desktop; this.output = output; this.preferences = preferences;
        view.Command = Execute; view.ConfirmExit = ConfirmDiscard;
        view.Canvas.EditText = a => Dialogs.Text(view, a, L); view.Properties.EditText = view.Canvas.EditText; view.Properties.OnError = view.Error; view.Canvas.NewStyle = view.Properties.DefaultStyle;
        view.Canvas.Failed += view.Error;
        view.FormClosed += (_, _) => Dispose();
    }
    public bool RegisterHotkey() => hotkey.Change(preferences.Modifiers, preferences.Key);
    public async Task Execute(string command)
    {
        if (view.Busy) return;
        try
        {
            view.Canvas.CancelGesture();
            switch (command)
            {
                case "Capture": await Capture(preferences.Mode); break;
                case "Region": case "Window": case "Monitor": await Capture(Enum.Parse<CaptureMode>(command)); break;
                case "Open": Open(false); break;
                case "OpenProject": Open(true); break;
                case "Save": Save(false); break;
                case "SaveAs": Save(true); break;
                case "Export": Export(); break;
                case "Copy": if (session.Document is not null) { output.Copy(Composition()); view.Status(L["Copied"]); } break;
                case "Undo": session.Undo(); break;
                case "Redo": session.Redo(); break;
                case "Duplicate": session.Duplicate(); break;
                case "Delete": session.Delete(); break;
                case "Group": session.Group(); break;
                case "Ungroup": session.Ungroup(); break;
                case "Front": session.Order(true); break;
                case "Back": session.Order(false); break;
                case "SelectAll": if (session.Document is { } d) { session.Selection.UnionWith(d.Scene.Objects.Select(a => a.Id)); session.Notify(); } break;
                case "ResetCrop": if (session.Document is { } doc) session.Mutate(s => s.Crop = new(0, 0, doc.Width, doc.Height)); break;
                case "Fit": view.Canvas.Fit(); break;
                case "Actual": view.Canvas.SetZoom(1); break;
                case "Settings": Settings(); break;
                case "Help": var path = Path.Combine(AppContext.BaseDirectory, "docs", "guia_de_uso.md"); var help = new ProcessStartInfo("notepad.exe") { UseShellExecute = false }; help.ArgumentList.Add(path); Process.Start(help); break;
                case "Exit": view.ExitApplication(); break;
            }
            if (!view.IsDisposed) { view.Properties.RefreshProperties(); view.Canvas.Invalidate(); }
        }
        catch (Exception e) { if (!view.IsDisposed) view.Error(e); }
    }
    void Open(bool project)
    {
        using var dialog = new OpenFileDialog { Filter = L[project ? "ProjectFilter" : "ImageFilter"], CheckFileExists = true };
        if (dialog.ShowDialog(view) != DialogResult.OK) return;
        // Fully validate a new document before asking to replace the current one.
        var doc = project ? store.Load(dialog.FileName) : store.Import(dialog.FileName);
        if (!ConfirmDiscard()) return;
        OpenDocument(doc, project, project ? dialog.FileName : null);
        var available = FontFamily.Families.Select(f => f.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (doc.Scene.Objects.Any(a => a.Kind is ObjectKind.Text or ObjectKind.Callout or ObjectKind.Number && !available.Contains(a.Style.FontFamily))) MessageBox.Show(view, L["FontFallback"], "ScreenHunter", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    public void OpenDocument(ImageDocument doc, bool saved, string? path = null)
    {
        var bitmap = CaptureFlow.Decode(doc.ImagePng); var previous = view.Canvas.BaseImage;
        try { session.Open(doc, saved); }
        catch { bitmap.Dispose(); throw; }
        view.Canvas.BaseImage = bitmap; previous?.Dispose(); projectPath = path; view.Canvas.Fit(); view.Properties.RefreshProperties();
    }
    public bool ConfirmDiscard()
    {
        if (!session.Dirty) return true;
        view.Restore();
        var answer = MessageBox.Show(view, L["Pending"], "ScreenHunter", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        if (answer == DialogResult.Cancel) return false;
        if (answer == DialogResult.No) return true;
        try { return Save(false); } catch (Exception e) { view.Error(e); return false; }
    }
    bool Save(bool saveAs)
    {
        if (session.Document is null) return true;
        string? path = projectPath;
        if (path is null || saveAs)
        {
            using var dialog = new SaveFileDialog { Filter = L["ProjectFilter"], DefaultExt = "shproj", AddExtension = true, FileName = path is null ? "ScreenHunter.shproj" : Path.GetFileName(path), OverwritePrompt = true };
            if (dialog.ShowDialog(view) != DialogResult.OK) return false; path = dialog.FileName;
        }
        store.Save(path, session.Document); projectPath = path; session.MarkSaved(); view.Status(L["Saved"]); return true;
    }
    byte[] Composition()
    {
        using var bitmap = SceneRenderer.Compose(session.Document!, view.Canvas.BaseImage!); using var s = new MemoryStream(); bitmap.Save(s, ImageFormat.Png); return s.ToArray();
    }
    void Export()
    {
        if (session.Document is null) return;
        using var dialog = new SaveFileDialog { Filter = L["ExportFilter"], DefaultExt = "png", AddExtension = true, FileName = "ScreenHunter.png", OverwritePrompt = true };
        if (dialog.ShowDialog(view) != DialogResult.OK) return;
        bool jpeg = dialog.FilterIndex == 2 || Path.GetExtension(dialog.FileName).Equals(".jpg", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(dialog.FileName).Equals(".jpeg", StringComparison.OrdinalIgnoreCase);
        int? quality = jpeg ? Dialogs.JpegQuality(view, L) : 100; if (quality is null) return;
        output.Export(Composition(), dialog.FileName, jpeg, quality.Value); view.Status(L["Exported"]);
    }
    async Task Capture(CaptureMode mode)
    {
        if (!ConfirmDiscard()) return;
        view.Busy = true; bool visible = view.Visible;
        try
        {
            view.Hide(); await Task.Delay(180); desktop.Flush();
            var doc = await new CaptureFlow(desktop, L).Run(mode, preferences);
            if (doc is not null) { OpenDocument(doc, false); visible = true; }
        }
        catch (Exception e) when (e is System.Runtime.InteropServices.ExternalException or UnauthorizedAccessException or System.ComponentModel.Win32Exception)
        { throw new InvalidOperationException("CaptureUnavailable", e); }
        finally { view.Busy = false; if (visible) view.Restore(); }
    }
    void Settings()
    {
        view.Restore(); var next = Dialogs.Settings(view, preferences, L); if (next is null) return;
        if (!hotkey.Change(next.Modifiers, next.Key)) { MessageBox.Show(view, L["HotkeyConflict"], "ScreenHunter", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        try { settingsStore.Save(next); }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            if (MessageBox.Show(view, L["SettingsTemporary"], "ScreenHunter", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            { hotkey.Change(preferences.Modifiers, preferences.Key); return; }
        }
        preferences = next; L.Set(next.Language); view.Localize();
    }
    public void Dispose() { if (disposing) return; disposing = true; hotkey.Dispose(); }
}
