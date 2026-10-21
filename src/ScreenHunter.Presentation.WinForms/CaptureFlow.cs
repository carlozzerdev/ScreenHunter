using ScreenHunter.Application;
using ScreenHunter.Domain;
using ScreenHunter.Presentation.WinForms.Rendering;

namespace ScreenHunter.Presentation.WinForms;

public sealed class CaptureFlow(IDesktopPlatform platform, L10n l)
{
    public async Task<ImageDocument?> Run(CaptureMode mode, Preferences preferences)
    {
        var virtualBox = platform.VirtualBounds;
        var windows = platform.Windows; var monitors = platform.Monitors;
        var preview = await Task.Run(() => platform.Grab(virtualBox, preferences.DelaySeconds == 0 && preferences.IncludeCursor));
        using var image = Decode(preview.Png);
        using var overlay = new SelectionOverlay(virtualBox, image, mode, windows, monitors, l);
        if (overlay.ShowDialog() != DialogResult.OK) return null;
        var bounds = overlay.Selected;
        if (preferences.DelaySeconds == 0)
        {
            var local = bounds.Intersect(virtualBox); using var cropped = image.Clone(new Rectangle((int)(local.X - virtualBox.X), (int)(local.Y - virtualBox.Y), (int)local.Width, (int)local.Height), image.PixelFormat);
            return Document(cropped);
        }
        using var waiting = Dialogs.Base(l["Waiting"], new Size(410, 90));
        waiting.TopMost = true; waiting.FormBorderStyle = FormBorderStyle.FixedToolWindow;
        var label = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 15) }; waiting.Controls.Add(label); waiting.Show();
        var finish = DateTime.UtcNow.AddSeconds(preferences.DelaySeconds);
        while (DateTime.UtcNow < finish)
        {
            if (platform.EscapePressed || !waiting.Visible) return null;
            label.Text = $"{l["Waiting"]}  {Math.Ceiling((finish - DateTime.UtcNow).TotalSeconds)}"; await Task.Delay(40);
        }
        waiting.Hide(); await Task.Delay(150); platform.Flush();
        if (mode == CaptureMode.Window) bounds = platform.WindowBounds(overlay.SelectedWindow);
        bounds = bounds.Intersect(platform.VirtualBounds);
        var shot = await Task.Run(() => platform.Grab(bounds, preferences.IncludeCursor));
        return new() { Width = shot.Width, Height = shot.Height, ImagePng = shot.Png, Scene = new() { Crop = new(0, 0, shot.Width, shot.Height) } };
    }
    public static Bitmap Decode(byte[] png) { using var s = new MemoryStream(png, false); using var i = Image.FromStream(s); return new(i); }
    public static ImageDocument Document(Bitmap bitmap)
    {
        DocumentRules.CheckImage(bitmap.Width, bitmap.Height); using var s = new MemoryStream(); bitmap.Save(s, System.Drawing.Imaging.ImageFormat.Png);
        return new() { Width = bitmap.Width, Height = bitmap.Height, ImagePng = s.ToArray(), Scene = new() { Crop = new(0, 0, bitmap.Width, bitmap.Height) } };
    }
}

public sealed class SelectionOverlay : Form
{
    readonly Box desktop; readonly Bitmap image; readonly CaptureMode mode; readonly IReadOnlyList<DesktopWindow> windows; readonly IReadOnlyList<Box> monitors; readonly L10n l;
    Vec start; bool dragging; Box? hover;
    public Box Selected { get; private set; }
    public nint SelectedWindow { get; private set; }
    public SelectionOverlay(Box desktop, Bitmap image, CaptureMode mode, IReadOnlyList<DesktopWindow> windows, IReadOnlyList<Box> monitors, L10n l)
    {
        this.desktop = desktop; this.image = image; this.mode = mode; this.windows = windows; this.monitors = monitors; this.l = l;
        FormBorderStyle = FormBorderStyle.None; StartPosition = FormStartPosition.Manual; AutoScaleMode = AutoScaleMode.None; Bounds = Rectangle.FromLTRB((int)desktop.X, (int)desktop.Y, (int)desktop.Right, (int)desktop.Bottom);
        TopMost = true; ShowInTaskbar = false; DoubleBuffered = true; KeyPreview = true; Cursor = Cursors.Cross;
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics; g.DrawImageUnscaled(image, 0, 0); using var dim = new SolidBrush(Color.FromArgb(115, 5, 15, 30)); g.FillRectangle(dim, ClientRectangle);
        if (hover is { } b)
        {
            var local = new Box(b.X - desktop.X, b.Y - desktop.Y, b.Width, b.Height); var rect = SceneRenderer.Rect(local);
            var state = g.Save(); g.SetClip(rect); g.DrawImageUnscaled(image, 0, 0); g.Restore(state);
            using var pen = new Pen(Color.DeepSkyBlue, 2); g.DrawRectangle(pen, rect);
        }
        using var font = new Font("Segoe UI", 13); var point = PointToClient(Cursor.Position); float tx = Math.Clamp(point.X + 20, 10, Math.Max(10, ClientSize.Width - 650)), ty = Math.Clamp(point.Y + 28, 10, Math.Max(10, ClientSize.Height - 60));
        g.FillRectangle(Brushes.MidnightBlue, tx, ty, 620, 38); g.DrawString(l["CaptureHint"], font, Brushes.White, tx + 10, ty + 8);
    }
    Vec World(Point p) => new(p.X + desktop.X, p.Y + desktop.Y);
    protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) { start = World(e.Location); dragging = true; if (mode != CaptureMode.Region) OnMouseMove(e); } }
    protected override void OnMouseMove(MouseEventArgs e)
    {
        var p = World(e.Location);
        if (mode == CaptureMode.Region && dragging) hover = Box.Between(start, p).Intersect(desktop);
        else if (mode == CaptureMode.Monitor) hover = monitors.FirstOrDefault(b => b.Contains(p));
        else if (mode == CaptureMode.Window) { var w = windows.FirstOrDefault(w => w.Bounds.Contains(p)); hover = w?.Bounds.Intersect(desktop); SelectedWindow = w?.Handle ?? 0; }
        Invalidate();
    }
    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left || !dragging) return; dragging = false;
        if (mode == CaptureMode.Region) hover = Box.Between(start, World(e.Location)).Intersect(desktop);
        if (hover is not { Width: >= 2, Height: >= 2 } b) return;
        Selected = new(Math.Floor(b.X), Math.Floor(b.Y), Math.Floor(b.Width), Math.Floor(b.Height)); DialogResult = DialogResult.OK; Close();
    }
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData) { if (keyData == Keys.Escape) { DialogResult = DialogResult.Cancel; Close(); return true; } return base.ProcessCmdKey(ref msg, keyData); }
}
