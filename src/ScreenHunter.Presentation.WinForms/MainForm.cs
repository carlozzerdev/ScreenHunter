using ScreenHunter.Application;

namespace ScreenHunter.Presentation.WinForms;

public sealed class MainForm : Form
{
    public EditorCanvas Canvas { get; }
    public PropertiesPanel Properties { get; }
    public L10n L { get; }
    readonly EditorSession session;
    readonly FlowLayoutPanel tools = new() { Dock = DockStyle.Left, Width = 160, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(9), BackColor = Color.FromArgb(245, 247, 251) };
    readonly Panel propertyHost = new() { Dock = DockStyle.Right, Width = 300, BackColor = Color.White, Padding = new Padding(0, 8, 0, 0) };
    readonly ToolStrip top = new() { GripStyle = ToolStripGripStyle.Hidden, Padding = new Padding(12, 8, 12, 8), BackColor = Color.White, Renderer = new ToolStripProfessionalRenderer() };
    readonly ToolStrip edits = new() { GripStyle = ToolStripGripStyle.Hidden, Padding = new Padding(10, 5, 10, 5), BackColor = Color.FromArgb(244, 247, 252) };
    readonly StatusStrip status = new() { BackColor = Color.FromArgb(18, 32, 53), ForeColor = Color.White };
    readonly ToolStripStatusLabel info = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
    readonly NotifyIcon tray;
    bool exit, trayTip;
    bool executing;
    readonly Dictionary<EditorTool, ScreenHunter.Domain.ObjectStyle> styles = [];
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public bool Busy { get; set; }
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Func<string, Task>? Command { get; set; }
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Func<bool>? ConfirmExit { get; set; }
    public MainForm(EditorSession session, L10n l)
    {
        this.session = session; L = l; Text = "ScreenHunter"; var screen = Screen.PrimaryScreen!.WorkingArea; ClientSize = new Size(Math.Min(1420, screen.Width - 50), Math.Min(900, screen.Height - 100)); MinimumSize = new Size(1060, 650); StartPosition = FormStartPosition.CenterScreen; AutoScaleMode = AutoScaleMode.Dpi; KeyPreview = true;
        using var iconStream = typeof(MainForm).Assembly.GetManifestResourceStream("ScreenHunter.ApplicationIcon")!;
        Icon = new Icon(iconStream);
        Canvas = new(session); Properties = new(session, l) { Dock = DockStyle.Fill }; propertyHost.Controls.Add(Properties);
        foreach (var tool in Enum.GetValues<EditorTool>())
        {
            var style = new ScreenHunter.Domain.ObjectStyle();
            if (tool == EditorTool.Text) style.Border = false;
            if (tool is EditorTool.Callout or EditorTool.Number) style.Filled = true;
            if (tool == EditorTool.Highlighter) { style.Stroke = 0xFFFFFF00; style.Opacity = .3; }
            styles[tool] = style;
        }
        Properties.DefaultStyle = styles[EditorTool.Select]; Canvas.NewStyle = Properties.DefaultStyle;
        Properties.StyleEdited += a => { var tool = Enum.Parse<EditorTool>(a.Kind.ToString()); styles[tool] = a.Style.Clone(); if (Canvas.Tool == tool) { Properties.DefaultStyle = styles[tool]; Canvas.NewStyle = styles[tool]; } };
        var workspace = new Panel { Dock = DockStyle.Fill }; workspace.Controls.Add(Canvas); workspace.Controls.Add(propertyHost); workspace.Controls.Add(tools);
        var header = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(18, 32, 53) };
        header.Controls.Add(new Label { Text = "SCREENHUNTER", ForeColor = Color.White, Font = new Font("Segoe UI", 18, FontStyle.Bold), Location = new Point(20, 14), AutoSize = true });
        var bar = new Panel { Dock = DockStyle.Top, AutoSize = true }; edits.Dock = DockStyle.Top; top.Dock = DockStyle.Top; bar.Controls.Add(edits); bar.Controls.Add(top);
        Controls.Add(workspace); Controls.Add(bar); Controls.Add(header); Controls.Add(status); status.Items.Add(info);
        tray = new NotifyIcon { Icon = Icon, Text = "ScreenHunter", Visible = true }; tray.DoubleClick += (_, _) => Restore();
        session.Changed += UpdateStatus;
        Canvas.InteractionFinished += () => { Properties.RefreshProperties(); RefreshTools(); UpdateStatus(); };
        Localize();
    }
    public void Localize()
    {
        top.Items.Clear(); edits.Items.Clear(); tools.Controls.Clear();
        void Button(ToolStrip strip, string key, string? shortcut = null)
        {
            var item = new ToolStripButton(L[key]) { DisplayStyle = ToolStripItemDisplayStyle.Text, ToolTipText = L[key] + (shortcut is null ? "" : $" ({shortcut})"), Padding = new Padding(5, 2, 5, 2) };
            item.Click += async (_, _) => await Run(key); strip.Items.Add(item);
        }
        var capture = new ToolStripSplitButton(L["Capture"]) { ForeColor = Color.FromArgb(25, 75, 170), Font = new Font(Font, FontStyle.Bold), Padding = new Padding(8, 3, 8, 3) };
        capture.ButtonClick += async (_, _) => await Run("Capture"); foreach (var mode in Enum.GetNames<CaptureMode>()) capture.DropDownItems.Add(L[mode], null, async (_, _) => await Run(mode)); top.Items.Add(capture);
        foreach (var key in new[] { "Open", "OpenProject", "Save", "SaveAs", "Export", "Copy", "Settings", "Help", "Exit" }) Button(top, key);
        foreach (var key in new[] { "Undo", "Redo", "Duplicate", "Delete", "Group", "Ungroup", "Front", "Back", "ResetCrop", "Fit", "Actual" }) Button(edits, key);
        var heading = new Label { Text = L["Tools"], Font = new Font(Font.FontFamily, 9, FontStyle.Bold), ForeColor = Color.SlateGray, AutoSize = true, Margin = new Padding(4, 10, 0, 14) }; tools.Controls.Add(heading);
        foreach (var tool in Enum.GetValues<EditorTool>())
        {
            var b = new Button { Text = L.Tool(tool), Width = 135, Height = 38, FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft, Tag = tool, Margin = new Padding(0, 0, 0, 5), AccessibleName = L.Tool(tool) };
            b.FlatAppearance.BorderSize = 0;
            b.Click += (_, _) => { Canvas.CancelGesture(); Canvas.Tool = tool; Properties.DefaultStyle = styles[tool]; Properties.DefaultKind = Enum.TryParse<ScreenHunter.Domain.ObjectKind>(tool.ToString(), out var kind) ? kind : ScreenHunter.Domain.ObjectKind.Text; Canvas.NewStyle = Properties.DefaultStyle; if (tool != EditorTool.Select) session.Select(null); RefreshTools(); Properties.RefreshProperties(); Canvas.Focus(); }; tools.Controls.Add(b);
        }
        var menu = new ContextMenuStrip(); foreach (var key in new[] { "Show", "Capture", "Open", "Settings", "Exit" }) menu.Items.Add(L[key], null, async (_, _) => { if (key == "Show") Restore(); else await Run(key); });
        var old = tray.ContextMenuStrip; tray.ContextMenuStrip = menu; old?.Dispose();
        Properties.RefreshProperties(); RefreshTools(); UpdateStatus();
    }
    async Task Run(string command)
    {
        if (Busy || executing || Command is null) return;
        executing = true; try { await Command(command); } finally { executing = false; }
    }
    void RefreshTools()
    {
        foreach (var b in tools.Controls.OfType<Button>()) { bool active = Equals(b.Tag, Canvas.Tool); b.BackColor = active ? Color.FromArgb(218, 231, 255) : Color.Transparent; b.ForeColor = active ? Color.FromArgb(25, 75, 170) : Color.FromArgb(34, 48, 66); }
    }
    public void UpdateStatus()
    {
        var d = session.Document;
        Text = "ScreenHunter" + (session.Dirty ? " *" : "");
        info.Text = d is null ? L["Empty"] : $"{(int)d.Scene.Crop.Width} × {(int)d.Scene.Crop.Height} px   |   {Canvas.Zoom:P0}   |   {L["Selection"]}: {session.Selection.Count}   |   {d.Scene.Objects.Count}   {(session.Dirty ? "• " + L["Unsaved"] : "")}";
    }
    public void Status(string text) => info.Text = text;
    public void Error(Exception e) => MessageBox.Show(this, L.Error(e), "ScreenHunter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    public void Restore() { Show(); WindowState = FormWindowState.Normal; Activate(); }
    public void ExitApplication() { exit = true; Close(); }
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing && !exit)
        {
            e.Cancel = true; if (Busy) return; Hide();
            if (!trayTip) { trayTip = true; tray.ShowBalloonTip(3000, "ScreenHunter", L["TrayHint"], ToolTipIcon.Info); }
        }
        else if (ConfirmExit?.Invoke() == false) { e.Cancel = true; exit = false; }
        base.OnFormClosing(e);
    }
    protected override void WndProc(ref Message m)
    {
        if (m.Msg == 0x0312) { if (!Busy) _ = Run("Capture"); return; }
        base.WndProc(ref m);
    }
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (ActiveControl is TextBoxBase || ActiveControl is ComboBox || ContainsFocus && Properties.ContainsFocus) return base.ProcessCmdKey(ref msg, keyData);
        string? cmd = keyData switch
        {
            Keys.Control | Keys.Z => "Undo", Keys.Control | Keys.Y => "Redo", Keys.Control | Keys.S => "Save", Keys.Control | Keys.Shift | Keys.S => "SaveAs", Keys.Control | Keys.C => "Copy", Keys.Control | Keys.D => "Duplicate", Keys.Control | Keys.G => "Group", Keys.Control | Keys.Shift | Keys.G => "Ungroup", Keys.Delete => "Delete", Keys.Control | Keys.O => "Open", Keys.Control | Keys.A => "SelectAll", _ => null
        };
        if (cmd is not null) { _ = Run(cmd); return true; }
        if (keyData is Keys.Left or Keys.Right or Keys.Up or Keys.Down || (keyData & ~Keys.Shift) is Keys.Left or Keys.Right or Keys.Up or Keys.Down)
        {
            int amount = keyData.HasFlag(Keys.Shift) ? 10 : 1; var key = keyData & Keys.KeyCode;
            session.Mutate(_ => session.Move(key switch { Keys.Left => new(-amount, 0), Keys.Right => new(amount, 0), Keys.Up => new(0, -amount), _ => new(0, amount) })); return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }
    protected override void Dispose(bool disposing) { if (disposing) { tray.Visible = false; tray.ContextMenuStrip?.Dispose(); tray.Dispose(); Canvas.BaseImage?.Dispose(); } base.Dispose(disposing); }
}
