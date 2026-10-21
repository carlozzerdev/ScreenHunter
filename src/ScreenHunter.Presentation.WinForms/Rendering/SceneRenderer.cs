using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using ScreenHunter.Domain;

namespace ScreenHunter.Presentation.WinForms.Rendering;

public static class SceneRenderer
{
    public static Color ColorOf(uint argb, double opacity = 1) => Color.FromArgb((int)Math.Clamp(((argb >> 24) & 255) * opacity, 0, 255), (int)(argb >> 16 & 255), (int)(argb >> 8 & 255), (int)(argb & 255));
    public static PointF Point(Vec p) => new((float)p.X, (float)p.Y);
    public static RectangleF Rect(Box b) => new((float)b.X, (float)b.Y, (float)b.Width, (float)b.Height);
    public static GraphicsPath Shape(Annotation a)
    {
        var p = new GraphicsPath(); var b = Rect(a.LocalBounds);
        if (a.IsPath)
        {
            if (a.Points.Count > 1) p.AddLines(a.Points.Select(Point).ToArray());
            return p;
        }
        if (a.Kind is ObjectKind.Ellipse or ObjectKind.Number || a.Kind == ObjectKind.Callout && a.Style.OvalCallout) p.AddEllipse(b);
        else
        {
            float r = (float)Math.Min(Math.Min(b.Width, b.Height) / 2, a.Style.CornerRadius);
            if (r < 1) p.AddRectangle(b);
            else
            {
                float d = 2 * r; p.AddArc(b.X, b.Y, d, d, 180, 90); p.AddArc(b.Right - d, b.Y, d, d, 270, 90);
                p.AddArc(b.Right - d, b.Bottom - d, d, d, 0, 90); p.AddArc(b.X, b.Bottom - d, d, d, 90, 90); p.CloseFigure();
            }
        }
        return p;
    }
    public static Font FontOf(Annotation a)
    {
        var s = a.Style; var style = (s.Bold ? FontStyle.Bold : 0) | (s.Italic ? FontStyle.Italic : 0) | (s.Underline ? FontStyle.Underline : 0);
        try { return new Font(s.FontFamily, (float)Math.Clamp(s.FontSize * 96 / 72, 1, 10000), style, GraphicsUnit.Pixel); }
        catch (ArgumentException) { return new Font("Segoe UI", (float)Math.Clamp(s.FontSize * 96 / 72, 1, 10000), style, GraphicsUnit.Pixel); }
    }
    public static void Draw(Graphics g, Annotation a)
    {
        if (a.Style.Shadow)
        {
            var shadow = a.Clone();
            shadow.Center += new Vec(4, 4);
            shadow.Style.Shadow = false;
            shadow.Style.Stroke = shadow.Style.Fill = shadow.Style.TextColor = 0xFF000000;
            shadow.Style.Opacity *= .3;
            DrawCore(g, shadow);
        }
        DrawCore(g, a);
    }
    static void DrawCore(Graphics g, Annotation a)
    {
        var state = g.Save();
        try
        {
            g.TranslateTransform((float)a.Center.X, (float)a.Center.Y); g.RotateTransform((float)a.Rotation);
            var s = a.Style; using var path = Shape(a);
            using var fill = new SolidBrush(ColorOf(s.Fill, s.Opacity));
            using var pen = new Pen(ColorOf(s.Stroke, s.Opacity), (float)s.StrokeWidth) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round, DashStyle = s.Dashed ? DashStyle.Dash : DashStyle.Solid };
            if (!a.IsPath && s.Filled) g.FillPath(fill, path);
            if (a.IsPath || s.Border) g.DrawPath(pen, path);
            if (a.Kind == ObjectKind.Arrow && a.Points.Count >= 2)
            {
                ArrowHead(g, a.Points[0], a.Points[^1], s);
                if (s.DoubleArrow) ArrowHead(g, a.Points[^1], a.Points[0], s);
            }
            if (a.Kind is ObjectKind.Text or ObjectKind.Callout or ObjectKind.Number)
            {
                using var font = FontOf(a); using var brush = new SolidBrush(ColorOf(s.TextColor, s.Opacity));
                var rect = Rect(a.LocalBounds); rect.Inflate(-(float)s.Padding, -(float)s.Padding);
                if (a.Kind == ObjectKind.Number)
                {
                    string number = a.Number.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    var measured = g.MeasureString(number, font);
                    float ratio = Math.Min(1, Math.Min(Math.Max(1, rect.Width) / measured.Width, Math.Max(1, rect.Height) / measured.Height));
                    using var numberFont = new Font(font.FontFamily, Math.Max(1, font.Size * ratio), font.Style, GraphicsUnit.Pixel);
                    using var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(number, numberFont, brush, rect, fmt);
                }
                else if (rect.Width > 0 && rect.Height > 0)
                {
                    using var fmt = new StringFormat { Alignment = (StringAlignment)s.Alignment, Trimming = StringTrimming.EllipsisCharacter };
                    g.DrawString(a.Text, font, brush, rect, fmt);
                }
            }
        }
        finally { g.Restore(state); }
    }
    static void ArrowHead(Graphics g, Vec from, Vec to, ObjectStyle style)
    {
        var d = to - from; if (d.Length < .01) return;
        var u = d * (1 / d.Length); var side = new Vec(-u.Y, u.X); double size = Math.Max(style.ArrowSize, style.StrokeWidth * 2);
        using var b = new SolidBrush(ColorOf(style.Stroke, style.Opacity));
        g.FillPolygon(b, new[] { Point(to), Point(to - u * size + side * (size * .45)), Point(to - u * size - side * (size * .45)) });
    }
    public static bool Hit(Annotation a, Vec world, double tolerance)
    {
        var p = a.ToLocal(world);
        if (a.Kind is ObjectKind.Text or ObjectKind.Callout or ObjectKind.Number)
        {
            if (a.LocalBounds.Contains(p)) return true;
        }
        using var shape = Shape(a); using var pen = new Pen(Color.Black, (float)Math.Max(a.Style.StrokeWidth, tolerance * 2));
        return (!a.IsPath && a.Style.Filled && shape.IsVisible(Point(p))) || shape.IsOutlineVisible(Point(p), pen);
    }
    public static void Configure(Graphics g)
    { g.SmoothingMode = SmoothingMode.AntiAlias; g.PixelOffsetMode = PixelOffsetMode.HighQuality; g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit; g.InterpolationMode = InterpolationMode.HighQualityBicubic; }
    public static void Document(Graphics g, ImageDocument document, Bitmap image)
    {
        Configure(g); g.DrawImage(image, new Rectangle(0, 0, document.Width, document.Height));
        foreach (var a in document.Scene.Objects) Draw(g, a);
    }
    public static Bitmap Compose(ImageDocument document, Bitmap image)
    {
        var c = document.Scene.Crop; var output = new Bitmap((int)c.Width, (int)c.Height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(output); g.TranslateTransform(-(float)c.X, -(float)c.Y); Document(g, document, image); return output;
    }
}
