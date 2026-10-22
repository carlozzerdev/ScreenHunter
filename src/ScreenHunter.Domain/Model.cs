using System.Text.Json;

namespace ScreenHunter.Domain;

public readonly record struct Vec(double X, double Y)
{
    public static Vec operator +(Vec a, Vec b) => new(a.X + b.X, a.Y + b.Y);
    public static Vec operator -(Vec a, Vec b) => new(a.X - b.X, a.Y - b.Y);
    public static Vec operator *(Vec a, double b) => new(a.X * b, a.Y * b);
    public double Length => Math.Sqrt(X * X + Y * Y);
    public Vec Rotate(double degrees)
    {
        double r = degrees * Math.PI / 180, c = Math.Cos(r), s = Math.Sin(r);
        return new(X * c - Y * s, X * s + Y * c);
    }
}

public readonly record struct Box(double X, double Y, double Width, double Height)
{
    public Vec Center => new(X + Width / 2, Y + Height / 2);
    public double Right => X + Width;
    public double Bottom => Y + Height;
    public bool Contains(Vec p) => p.X >= X && p.Y >= Y && p.X <= Right && p.Y <= Bottom;
    public bool Contains(Box b) => Contains(new Vec(b.X, b.Y)) && Contains(new Vec(b.Right, b.Bottom));
    public Box Inflate(double n) => new(X - n, Y - n, Width + 2 * n, Height + 2 * n);
    public static Box Between(Vec a, Vec b) => new(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
    public static Box From(IEnumerable<Vec> points)
    {
        var p = points.ToArray();
        if (p.Length == 0) return new();
        return new(p.Min(v => v.X), p.Min(v => v.Y), p.Max(v => v.X) - p.Min(v => v.X), p.Max(v => v.Y) - p.Min(v => v.Y));
    }
    public Box Intersect(Box b)
    {
        var x = Math.Max(X, b.X); var y = Math.Max(Y, b.Y);
        return new(x, y, Math.Max(0, Math.Min(Right, b.Right) - x), Math.Max(0, Math.Min(Bottom, b.Bottom) - y));
    }
}

public enum ObjectKind { Text, Callout, Arrow, Rectangle, Ellipse, Freehand, Line, Highlighter, Number }
public sealed class ObjectStyle
{
    public uint Stroke { get; set; } = 0xFFE54747;
    public uint Fill { get; set; } = 0xFFFFFFFF;
    public uint TextColor { get; set; } = 0xFF15243B;
    public double StrokeWidth { get; set; } = 4;
    public double Opacity { get; set; } = 1;
    public bool Shadow { get; set; }
    public bool Border { get; set; } = true;
    public bool Filled { get; set; }
    public string FontFamily { get; set; } = "Segoe UI";
    public double FontSize { get; set; } = 24;
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public bool Underline { get; set; }
    public int Alignment { get; set; }
    public double CornerRadius { get; set; } = 12;
    public bool Dashed { get; set; }
    public bool DoubleArrow { get; set; }
    public double ArrowSize { get; set; } = 14;
    public bool OvalCallout { get; set; }
    public double Padding { get; set; } = 8;
    public ObjectStyle Clone() => (ObjectStyle)MemberwiseClone();
}

public sealed class Annotation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ObjectKind Kind { get; set; }
    public Vec Center { get; set; }
    public double Width { get; set; } = 120;
    public double Height { get; set; } = 80;
    public double Rotation { get; set; }
    public ObjectStyle Style { get; set; } = new();
    public string Text { get; set; } = "";
    public int Number { get; set; } = 1;
    // Local points relative to Center; lines use 2 points, strokes a polyline.
    public List<Vec> Points { get; set; } = [];
    public Vec Tail { get; set; } = new(0, 90);
    public Vec ToWorld(Vec local) => Center + local.Rotate(Rotation);
    public Vec ToLocal(Vec world) => (world - Center).Rotate(-Rotation);
    public bool IsPath => Kind is ObjectKind.Line or ObjectKind.Arrow or ObjectKind.Freehand or ObjectKind.Highlighter;
    public Box LocalBounds => IsPath && Points.Count > 0 ? Box.From(Points) : new(-Width / 2, -Height / 2, Width, Height);
    public Box Bounds
    {
        get
        {
            var b = LocalBounds;
            var p = new List<Vec> { new(b.X, b.Y), new(b.Right, b.Y), new(b.Right, b.Bottom), new(b.X, b.Bottom) };
            return Box.From(p.Select(ToWorld)).Inflate(Style.Border || IsPath ? Style.StrokeWidth / 2 + (Kind == ObjectKind.Arrow ? Style.ArrowSize / 2 : 0) : 0);
        }
    }
    public Annotation Clone() => new()
    {
        Id = Id, Kind = Kind, Center = Center, Width = Width, Height = Height, Rotation = Rotation,
        Style = Style.Clone(), Text = Text, Number = Number, Points = [.. Points], Tail = Tail
    };
    public void Scale(double sx, double sy, bool typography)
    {
        Width *= sx; Height *= sy;
        Points = Points.Select(p => new Vec(p.X * sx, p.Y * sy)).ToList();
        Tail = new(Tail.X * sx, Tail.Y * sy);
        if (typography) { Style.FontSize *= sx; Style.StrokeWidth *= sx; Style.ArrowSize *= sx; Style.Padding *= sx; Style.CornerRadius *= sx; }
    }
    public static double Normalize(double a) => (a % 360 + 360) % 360;
}

public sealed class ObjectGroup
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public List<Guid> Members { get; set; } = [];
    public ObjectGroup Clone() => new() { Id = Id, Members = [.. Members] };
}
public sealed class Scene
{
    public List<Annotation> Objects { get; set; } = [];
    public List<ObjectGroup> Groups { get; set; } = [];
    public Box Crop { get; set; }
    public Scene Clone() => new() { Objects = Objects.Select(a => a.Clone()).ToList(), Groups = Groups.Select(g => g.Clone()).ToList(), Crop = Crop };
}
public sealed class ImageDocument
{
    public int SchemaVersion { get; set; } = 1;
    public int Width { get; set; }
    public int Height { get; set; }
    public string ColorSpace { get; set; } = "sRGB";
    public Scene Scene { get; set; } = new();
    [System.Text.Json.Serialization.JsonIgnore]
    public byte[] ImagePng { get; set; } = [];
}

public static class DocumentRules
{
    public const int MaxDimension = 32768;
    public const long MaxPixels = 64_000_000;
    public const int MaxObjects = 2000;
    public static void CheckImage(int width, int height)
    {
        if (width <= 0 || height <= 0 || width > MaxDimension || height > MaxDimension || (long)width * height > MaxPixels)
            throw new InvalidDataException("ImageLimit");
    }
    public static void Validate(ImageDocument d)
    {
        if (d.SchemaVersion != 1 || d.ColorSpace != "sRGB") throw new InvalidDataException("ProjectVersion");
        CheckImage(d.Width, d.Height);
        var s = d.Scene ?? throw new InvalidDataException("InvalidProject");
        if (s.Objects is null || s.Groups is null || s.Objects.Count > MaxObjects || s.Groups.Count > MaxObjects) throw new InvalidDataException("ObjectLimit");
        bool Fin(double x) => double.IsFinite(x) && Math.Abs(x) <= 1_000_000;
        bool V(Vec p) => Fin(p.X) && Fin(p.Y);
        var crop = s.Crop;
        if (!Fin(crop.X) || !Fin(crop.Y) || !Fin(crop.Width) || !Fin(crop.Height) || crop.Width < 1 || crop.Height < 1 || !new Box(0, 0, d.Width, d.Height).Contains(crop)) throw new InvalidDataException("InvalidProject");
        var ids = new HashSet<Guid>(); int points = 0;
        foreach (var a in s.Objects)
        {
            if (a is null || !ids.Add(a.Id) || a.Id == Guid.Empty || !Enum.IsDefined(a.Kind) || !V(a.Center) || !V(a.Tail) || !Fin(a.Rotation) || !Fin(a.Width) || !Fin(a.Height) || a.Width <= 0 || a.Height <= 0 || a.Style is null || a.Points is null || a.Text is null || a.Text.Length > 100_000 || a.Number < 1) throw new InvalidDataException("InvalidProject");
            points += a.Points.Count;
            if (points > 500_000 || a.Points.Count > 50_000 || a.Points.Any(p => !V(p)) || (a.IsPath && a.Points.Count < 2)) throw new InvalidDataException("ObjectLimit");
            var st = a.Style;
            if (!Fin(st.StrokeWidth) || st.StrokeWidth <= 0 || !Fin(st.FontSize) || st.FontSize <= 0 || !Fin(st.Opacity) || st.Opacity is <= 0 or > 1 || st.Alignment is < 0 or > 2 || string.IsNullOrWhiteSpace(st.FontFamily) || st.FontFamily.Length > 200 || !Fin(st.ArrowSize) || st.ArrowSize <= 0 || !Fin(st.Padding) || st.Padding < 0 || !Fin(st.CornerRadius) || st.CornerRadius < 0) throw new InvalidDataException("InvalidProject");
        }
        var members = new HashSet<Guid>();
        foreach (var g in s.Groups)
        {
            if (g is null || !ids.Add(g.Id) || g.Id == Guid.Empty || g.Members is null || g.Members.Count < 2 || g.Members.Any(id => !s.Objects.Any(a => a.Id == id) || !members.Add(id))) throw new InvalidDataException("InvalidProject");
            var indices = g.Members.Select(id => s.Objects.FindIndex(a => a.Id == id)).ToArray();
            if (indices.Max() - indices.Min() + 1 != indices.Length || !indices.SequenceEqual(indices.Order())) throw new InvalidDataException("InvalidProject");
        }
    }
    public static string Fingerprint(Scene scene) => JsonSerializer.Serialize(scene);
}
