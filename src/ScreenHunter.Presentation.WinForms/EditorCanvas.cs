using System.Drawing.Drawing2D;
using ScreenHunter.Application;
using ScreenHunter.Domain;
using ScreenHunter.Presentation.WinForms.Rendering;

namespace ScreenHunter.Presentation.WinForms;

public enum EditorTool { Select, Text, Callout, Arrow, Rectangle, Ellipse, Freehand, Line, Highlighter, Number, Crop }
public sealed class EditorCanvas : ScrollableControl
{
    public EditorSession Session { get; }
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Bitmap? BaseImage { get; set; }
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public EditorTool Tool { get; set; }
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public ObjectStyle NewStyle { get; set; } = new();
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Func<Annotation, bool>? EditText { get; set; }
    public event Action? InteractionFinished;
    public event Action<Exception>? Failed;
    public float Zoom { get; private set; } = 1;
    string gesture = ""; int handle = -1;
    Vec start, previous, pivot; Box originalBox; double initialAngle;
    Annotation? creating;
    Box? marquee;
    bool addFrame;
    public EditorCanvas(EditorSession session)
    {
        Session = session; Dock = DockStyle.Fill; BackColor = Color.FromArgb(226, 232, 240); AutoScroll = true; TabStop = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.Selectable, true);
        Session.Changed += () => { UpdateScroll(); Invalidate(); };
    }
    Vec Origin => new(32 + AutoScrollPosition.X, 32 + AutoScrollPosition.Y);
    public Vec ToDocument(Point p) => (new Vec(p.X, p.Y) - Origin) * (1 / Zoom) + (Session.Document?.Scene.Crop is { } c ? new Vec(c.X, c.Y) : new Vec());
    public PointF ToClient(Vec p)
    {
        var c = Session.Document?.Scene.Crop ?? new(); return SceneRenderer.Point(Origin + (p - new Vec(c.X, c.Y)) * Zoom);
    }
    public void SetZoom(float z)
    {
        Zoom = Math.Clamp(z, .05f, 8); UpdateScroll(); Invalidate(); InteractionFinished?.Invoke();
    }
    public void Fit()
    {
        if (Session.Document is not { } d) return;
        SetZoom((float)Math.Min((ClientSize.Width - 80) / d.Scene.Crop.Width, (ClientSize.Height - 80) / d.Scene.Crop.Height)); AutoScrollPosition = Point.Empty;
    }
    void UpdateScroll()
    {
        var c = Session.Document?.Scene.Crop ?? new();
        var size = new Size((int)Math.Ceiling(c.Width * Zoom) + 64, (int)Math.Ceiling(c.Height * Zoom) + 64);
        if (AutoScrollMinSize != size) AutoScrollMinSize = size;
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e); var g = e.Graphics;
        if (Session.Document is not { } d || BaseImage is null) return;
        var state = g.Save(); var origin = Origin; var crop = d.Scene.Crop;
        g.TranslateTransform((float)origin.X, (float)origin.Y); g.ScaleTransform(Zoom, Zoom); g.TranslateTransform(-(float)crop.X, -(float)crop.Y);
        var clip = g.Save(); g.SetClip(SceneRenderer.Rect(crop)); g.FillRectangle(Brushes.White, SceneRenderer.Rect(crop));
        SceneRenderer.Document(g, d, BaseImage); g.Restore(clip);
        if (Session.Selection.Count > 0 && gesture != "draw") DrawSelection(g);
        if (marquee is { } m)
        {
            using var brush = new SolidBrush(Color.FromArgb(35, 30, 106, 230)); using var pen = new Pen(Color.RoyalBlue, 1 / Zoom) { DashStyle = DashStyle.Dash };
            g.FillRectangle(brush, SceneRenderer.Rect(m)); g.DrawRectangle(pen, SceneRenderer.Rect(m));
        }
        g.Restore(state);
    }
    List<Vec> Handles()
    {
        var selected = Session.Selected.ToArray(); if (selected.Length == 0) return [];
        if (selected.Length == 1)
        {
            var a = selected[0]; var b = a.LocalBounds;
            var list = new List<Vec> { new(b.X,b.Y), new(b.Right,b.Y), new(b.Right,b.Bottom), new(b.X,b.Bottom), new(b.Center.X,b.Y), new(b.Right,b.Center.Y), new(b.Center.X,b.Bottom), new(b.X,b.Center.Y), new(b.Center.X,b.Y - 30 / Zoom) };
            var world = list.Select(a.ToWorld).ToList();
            if (a.Kind is ObjectKind.Arrow or ObjectKind.Line) { world.Add(a.ToWorld(a.Points[0])); world.Add(a.ToWorld(a.Points[^1])); }
            return world;
        }
        var r = Session.SelectionBounds;
        return [new(r.X,r.Y),new(r.Right,r.Y),new(r.Right,r.Bottom),new(r.X,r.Bottom),r.Center,r.Center,r.Center,r.Center,new(r.Center.X,r.Y - 30 / Zoom)];
    }
    void DrawSelection(Graphics g)
    {
        var h = Handles(); if (h.Count == 0) return;
        using var pen = new Pen(Color.FromArgb(35, 100, 225), 1.5f / Zoom);
        g.DrawPolygon(pen, h.Take(4).Select(SceneRenderer.Point).ToArray()); g.DrawLine(pen, SceneRenderer.Point((h[0] + h[1]) * .5), SceneRenderer.Point(h[8]));
        float r = 4 / Zoom; bool multi = Session.Selected.Count() > 1;
        for (int i = 0; i < h.Count; i++)
        {
            if (multi && i is >= 4 and <= 7) continue;
            var p = h[i]; var rect = new RectangleF((float)p.X - r, (float)p.Y - r, r * 2, r * 2);
            g.FillRectangle(i >= 8 ? Brushes.LightSkyBlue : Brushes.White, rect); g.DrawRectangle(pen, rect);
        }
    }
    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e); if (e.Button != MouseButtons.Left || Session.Document is null) return;
        Focus(); start = previous = ToDocument(e.Location); addFrame = ModifierKeys.HasFlag(Keys.Shift); Capture = true;
        if (Tool == EditorTool.Crop) { gesture = "crop"; marquee = Box.Between(start, start); return; }
        if (Tool != EditorTool.Select)
        {
            if (!Session.Document.Scene.Crop.Contains(start)) { Capture = false; return; }
            var kind = Enum.Parse<ObjectKind>(Tool.ToString());
            var style = NewStyle.Clone();
            creating = new() { Kind = kind, Center = start, Width = 1, Height = 1, Style = style, Number = kind == ObjectKind.Number ? Session.NextNumber() : 1 };
            if (creating.IsPath) creating.Points = [new(0, 0), new(.1, .1)];
            Session.Begin(); Session.Document.Scene.Objects.Add(creating); Session.Selection.Clear(); Session.Selection.Add(creating.Id); gesture = "draw"; Session.Notify(); return;
        }
        var h = Handles(); handle = -1;
        for (int i = h.Count - 1; i >= 0; i--) if (!(Session.Selected.Count() > 1 && i is >= 4 and <= 7) && (h[i] - start).Length <= 8 / Zoom) { handle = i; break; }
        if (handle >= 0)
        {
            Session.Begin(); originalBox = Session.SelectionBounds; pivot = Session.Selected.Count() == 1 ? Session.Selected.First().Center : originalBox.Center;
            initialAngle = Math.Atan2(start.Y - pivot.Y, start.X - pivot.X) * 180 / Math.PI;
            gesture = handle == 8 ? "rotate" : handle >= 9 ? "point" : "size"; return;
        }
        var hit = Session.Document.Scene.Objects.LastOrDefault(a => SceneRenderer.Hit(a, start, 5 / Zoom));
        if (hit is not null)
        {
            if (addFrame) { Session.Select(hit.Id, true); gesture = ""; Capture = false; InteractionFinished?.Invoke(); return; }
            if (!Session.Selection.Contains(hit.Id)) Session.Select(hit.Id);
            Session.Begin(); gesture = "move";
        }
        else { if (!addFrame) Session.Select(null); gesture = "frame"; marquee = Box.Between(start, start); }
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e); if (gesture == "") return;
        var current = ToDocument(e.Location); bool shift = ModifierKeys.HasFlag(Keys.Shift);
        if (gesture is "frame" or "crop") { marquee = Box.Between(start, current); Invalidate(); return; }
        if (gesture == "draw" && creating is not null)
        {
            var a = creating;
            if (a.Kind is ObjectKind.Freehand or ObjectKind.Highlighter)
            {
                if ((current - previous).Length >= 1 / Zoom && a.Points.Count < 50000) { a.Points.Add(current - start); previous = current; }
            }
            else if (a.Kind is ObjectKind.Line or ObjectKind.Arrow)
            {
                var d = current - start;
                if (shift) { var angle = Math.Round(Math.Atan2(d.Y, d.X) / (Math.PI / 4)) * Math.PI / 4; d = new Vec(Math.Cos(angle), Math.Sin(angle)) * d.Length; }
                a.Points = [new(0, 0), d];
            }
            else
            {
                var b = Box.Between(start, current); double w = Math.Max(1, b.Width), ht = Math.Max(1, b.Height);
                if (shift || a.Kind == ObjectKind.Number) w = ht = Math.Max(w, ht);
                a.Width = w; a.Height = ht; a.Center = new(Math.Min(start.X, current.X) + w / 2, Math.Min(start.Y, current.Y) + ht / 2); a.Tail = new(0, ht / 2 + 40);
            }
            Invalidate(); return;
        }
        Session.RestorePreview();
        if (gesture == "move") Session.Move(current - start);
        else if (gesture == "rotate")
        {
            double angle = Math.Atan2(current.Y - pivot.Y, current.X - pivot.X) * 180 / Math.PI - initialAngle;
            if (shift) angle = Math.Round(angle / 15) * 15;
            Session.Rotate(angle);
        }
        else if (gesture == "point")
        {
            var a = Session.Selected.Single(); var local = a.ToLocal(current);
            a.Points[handle == 9 ? 0 : a.Points.Count - 1] = local;
            Session.Notify();
        }
        else if (gesture == "size") ResizeSelection(current, shift);
    }
    void ResizeSelection(Vec current, bool shift)
    {
        var selected = Session.Selected.ToArray();
        if (selected.Length == 1 && !selected[0].IsPath && selected[0].Kind != ObjectKind.Number)
        {
            var a = selected[0]; var b = a.LocalBounds; var p = a.ToLocal(current);
            double l = b.X, t = b.Y, r = b.Right, bot = b.Bottom;
            if (handle is 0 or 3 or 7) l = Math.Min(p.X, r - 8);
            if (handle is 1 or 2 or 5) r = Math.Max(p.X, l + 8);
            if (handle is 0 or 1 or 4) t = Math.Min(p.Y, bot - 8);
            if (handle is 2 or 3 or 6) bot = Math.Max(p.Y, t + 8);
            double sx = (r - l) / a.Width, sy = (bot - t) / a.Height;
            if (shift) { sy = sx; bot = t + a.Height * sy; }
            a.Center = a.ToWorld(new((l + r) / 2, (t + bot) / 2)); a.Scale(sx, sy, false); Session.Notify();
        }
        else
        {
            var corners = new[] {new Vec(originalBox.X, originalBox.Y),new Vec(originalBox.Right, originalBox.Y),new Vec(originalBox.Right, originalBox.Bottom),new Vec(originalBox.X, originalBox.Bottom)};
            var anchor = corners[(handle % 4 + 2) % 4];
            var baseline = start - anchor;
            double factor = baseline.Length > 0 ? ((current.X - anchor.X) * baseline.X + (current.Y - anchor.Y) * baseline.Y) / (baseline.Length * baseline.Length) : 1;
            Session.Scale(Math.Clamp(factor, .05, 20), anchor);
        }
    }
    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e); if (gesture == "") return;
        string action = gesture; gesture = ""; Capture = false;
        try
        {
            if (action == "frame" && marquee is { } frame) Session.SelectFrame(frame, addFrame);
            else if (action == "crop" && marquee is { } crop) Session.Crop(crop);
            else if (action == "draw" && creating is { } a)
            {
                if (a.Kind is ObjectKind.Text or ObjectKind.Callout)
                {
                    if (a.Width < 20 || a.Height < 20) { a.Width = 280; a.Height = 110; a.Center = start + new Vec(140, 55); a.Tail = new(20, 95); }
                    if (EditText?.Invoke(a) != true) { Session.Cancel(); return; }
                }
                if (a.Kind == ObjectKind.Number && a.Width < 12) { a.Width = a.Height = 56; a.Center = start; a.Style.Padding = 4; }
                if (a.IsPath && (a.Points[^1] - a.Points[0]).Length < .5 && a.Points.Count <= 2) { Session.Cancel(); return; }
                if (a.Kind is ObjectKind.Freehand or ObjectKind.Highlighter or ObjectKind.Line or ObjectKind.Arrow)
                {
                    var center = Box.From(a.Points).Center; a.Center += center; a.Points = a.Points.Select(p => p - center).ToList();
                    var b = Box.From(a.Points); a.Width = Math.Max(1, b.Width); a.Height = Math.Max(1, b.Height);
                }
                Session.Commit(); Tool = EditorTool.Select;
            }
            else Session.Commit();
        }
        catch (Exception error) { Session.Cancel(); Failed?.Invoke(error); }
        finally { marquee = null; creating = null; Invalidate(); InteractionFinished?.Invoke(); }
    }
    protected override void OnMouseDoubleClick(MouseEventArgs e)
    {
        base.OnMouseDoubleClick(e);
        if (Session.Document is null || Tool != EditorTool.Select) return;
        CancelGesture(); var hit = Session.Document.Scene.Objects.LastOrDefault(a => SceneRenderer.Hit(a, ToDocument(e.Location), 5 / Zoom));
        if (hit is null || hit.Kind is not (ObjectKind.Text or ObjectKind.Callout) || Session.Unit(hit.Id).Count() > 1) return;
        Session.Select(hit.Id); Session.Begin(); if (EditText?.Invoke(hit) == true) Session.Commit(); else Session.Cancel(); InteractionFinished?.Invoke();
    }
    public void CancelGesture()
    {
        gesture = ""; creating = null; marquee = null; Capture = false;
        if (Session.InTransaction) Session.Cancel(); Invalidate();
    }
    protected override void OnMouseCaptureChanged(EventArgs e)
    { base.OnMouseCaptureChanged(e); if (!Capture && gesture != "") CancelGesture(); }
    protected override void OnMouseWheel(MouseEventArgs e)
    { if (ModifierKeys.HasFlag(Keys.Control)) SetZoom(Zoom * (e.Delta > 0 ? 1.15f : 1 / 1.15f)); else base.OnMouseWheel(e); }
    protected override bool IsInputKey(Keys keyData) => keyData is Keys.Left or Keys.Right or Keys.Up or Keys.Down || base.IsInputKey(keyData);
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape) { bool active = gesture != ""; CancelGesture(); if (!active) Session.Select(null); e.Handled = true; }
        base.OnKeyDown(e);
    }
}
