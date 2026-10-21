using ScreenHunter.Application;
using ScreenHunter.Domain;

namespace ScreenHunter.Presentation.WinForms;

public static class Dialogs
{
    public static Form Base(string title, Size size)
    {
        return new Form { Text = title, ClientSize = size, StartPosition = FormStartPosition.CenterParent, MinimizeBox = false, MaximizeBox = false, FormBorderStyle = FormBorderStyle.FixedDialog, Font = new Font("Segoe UI", 10), BackColor = Color.White, AutoScaleMode = AutoScaleMode.Dpi };
    }
    public static FlowLayoutPanel Buttons(Form f, L10n l)
    {
        var p = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 54, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8), BackColor = Color.FromArgb(243, 246, 250) };
        var ok = new Button { Text = l["OK"], AutoSize = true, DialogResult = DialogResult.OK };
        var cancel = new Button { Text = l["Cancel"], AutoSize = true, DialogResult = DialogResult.Cancel };
        p.Controls.Add(ok); p.Controls.Add(cancel); f.AcceptButton = ok; f.CancelButton = cancel; f.Controls.Add(p); return p;
    }
    public static bool Text(IWin32Window owner, Annotation a, L10n l)
    {
        using var f = Base(l["EditText"], new Size(520, 310)); Buttons(f, l);
        var box = new TextBox { Multiline = true, AcceptsReturn = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, Text = a.Text, Margin = new Padding(16), MaxLength = 100000 };
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) }; panel.Controls.Add(box); f.Controls.Add(panel); panel.BringToFront();
        f.Shown += (_, _) => { box.Focus(); box.SelectAll(); }; f.AcceptButton = null;
        if (f.ShowDialog(owner) != DialogResult.OK) return false;
        if (string.IsNullOrWhiteSpace(box.Text)) return false;
        a.Text = box.Text; return true;
    }
    public static Preferences? Settings(IWin32Window owner, Preferences old, L10n l)
    {
        var p = old.Clone(); using var f = Base(l["Settings"], new Size(560, 440)); Buttons(f, l);
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), ColumnCount = 2, RowCount = 7 };
        table.ColumnStyles.Add(new(SizeType.Percent, 53)); table.ColumnStyles.Add(new(SizeType.Percent, 47));
        void Row(string key, Control control) { int r = table.Controls.Count / 2; table.RowStyles.Add(new(SizeType.Absolute, 45)); table.Controls.Add(new Label { Text = l[key], AutoSize = true, Anchor = AnchorStyles.Left }, 0, r); control.Dock = DockStyle.Fill; table.Controls.Add(control, 1, r); }
        var language = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList }; language.Items.AddRange(["Español", "English"]); language.SelectedIndex = p.Language == "es" ? 0 : 1; Row("Language", language);
        var shortcut = new TextBox { ReadOnly = true, Text = Shortcut(p, l) };
        shortcut.KeyDown += (_, e) => { e.SuppressKeyPress = true; if (e.KeyCode is Keys.ControlKey or Keys.ShiftKey or Keys.Menu or Keys.LWin or Keys.RWin || e.KeyCode == Keys.F12) return; int mod = (e.Alt ? 1 : 0) | (e.Control ? 2 : 0) | (e.Shift ? 4 : 0); if (mod == 0) return; p.Key = (int)e.KeyCode; p.Modifiers = mod; shortcut.Text = Shortcut(p, l); };
        Row("Shortcut", shortcut);
        var mode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList }; mode.Items.AddRange(Enum.GetNames<CaptureMode>().Select(k => (object)l[k]).ToArray()); mode.SelectedIndex = (int)p.Mode; Row("CaptureMode", mode);
        var delay = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList }; delay.Items.AddRange(["0", "3", "5", "10"]); delay.SelectedItem = p.DelaySeconds.ToString(); Row("Delay", delay);
        var cursor = new CheckBox { Checked = p.IncludeCursor }; Row("Cursor", cursor);
        var help = new Label { Text = l["ShortcutHelp"], AutoSize = true, Dock = DockStyle.Fill, ForeColor = Color.DimGray }; table.Controls.Add(help, 0, 5); table.SetColumnSpan(help, 2);
        f.Controls.Add(table); table.BringToFront();
        if (f.ShowDialog(owner) != DialogResult.OK) return null;
        p.Language = language.SelectedIndex == 0 ? "es" : "en"; p.Mode = (CaptureMode)mode.SelectedIndex; p.DelaySeconds = int.Parse((string)delay.SelectedItem!); p.IncludeCursor = cursor.Checked; p.Validate(); return p;
    }
    public static string Shortcut(Preferences p, L10n l)
    {
        var parts = new List<string>(); if ((p.Modifiers & 2) != 0) parts.Add(l["Ctrl"]); if ((p.Modifiers & 1) != 0) parts.Add(l["Alt"]); if ((p.Modifiers & 4) != 0) parts.Add(l["Shift"]); parts.Add(((Keys)p.Key).ToString()); return string.Join("+", parts);
    }
    public static int? JpegQuality(IWin32Window owner, L10n l)
    {
        using var f = Base(l["Quality"], new Size(360, 140)); Buttons(f, l);
        var n = new NumericUpDown { Minimum = 1, Maximum = 100, Value = 90, Location = new Point(20, 22), Width = 300 }; f.Controls.Add(n);
        return f.ShowDialog(owner) == DialogResult.OK ? (int)n.Value : null;
    }
}
