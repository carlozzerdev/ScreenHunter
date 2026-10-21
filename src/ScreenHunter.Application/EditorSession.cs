using ScreenHunter.Domain;

namespace ScreenHunter.Application;

public sealed class EditorSession
{
    public ImageDocument? Document { get; private set; }
    public HashSet<Guid> Selection { get; } = [];
    readonly List<(Scene Scene, string Token)> undo = [], redo = [];
    Scene? transaction;
    string token = Guid.NewGuid().ToString(), savedToken = "";
    public bool Dirty => Document is not null && token != savedToken;
    public bool CanUndo => undo.Count > 0;
    public bool CanRedo => redo.Count > 0;
    public bool InTransaction => transaction is not null;
    public event Action? Changed;
    public IEnumerable<Annotation> Selected => Document?.Scene.Objects.Where(a => Selection.Contains(a.Id)) ?? [];
    public Box SelectionBounds => Box.From(Selected.SelectMany(a => new[] { new Vec(a.Bounds.X, a.Bounds.Y), new Vec(a.Bounds.Right, a.Bounds.Bottom) }));
    public void Open(ImageDocument d, bool saved)
    {
        DocumentRules.Validate(d); Document = d; Selection.Clear(); undo.Clear(); redo.Clear(); transaction = null;
        token = Guid.NewGuid().ToString(); savedToken = saved ? token : ""; Notify();
    }
    public void MarkSaved() { savedToken = token; Notify(); }
    public void Notify() => Changed?.Invoke();
    public void Begin()
    {
        if (transaction is not null) throw new InvalidOperationException("Transaction already active.");
        if (Document is not null) transaction = Document.Scene.Clone();
    }
    public void Commit()
    {
        if (Document is null || transaction is null) return;
        if (DocumentRules.Fingerprint(transaction) != DocumentRules.Fingerprint(Document.Scene))
        {
            try { DocumentRules.Validate(Document); }
            catch { Document.Scene = transaction; transaction = null; Notify(); throw; }
            undo.Add((transaction, token));
            // Bound history by geometry volume as well as number of interactions.
            while (undo.Count > 80 || (undo.Count > 1 && undo.Sum(h => h.Scene.Objects.Sum(a => (long)a.Points.Count + 32 + a.Text.Length / 8)) > 1_000_000)) undo.RemoveAt(0);
            redo.Clear(); token = Guid.NewGuid().ToString();
        }
        transaction = null; Prune(); Notify();
    }
    public void Cancel()
    {
        if (Document is not null && transaction is not null) Document.Scene = transaction;
        transaction = null; Prune(); Notify();
    }
    public void Mutate(Action<Scene> action)
    {
        if (Document is null) return;
        Begin(); try { action(Document.Scene); Commit(); } catch { Cancel(); throw; }
    }
    public void Undo() => Travel(undo, redo);
    public void Redo() => Travel(redo, undo);
    void Travel(List<(Scene Scene, string Token)> from, List<(Scene Scene, string Token)> to)
    {
        if (Document is null || from.Count == 0) return;
        if (transaction is not null) Cancel();
        to.Add((Document.Scene.Clone(), token)); var h = from[^1]; from.RemoveAt(from.Count - 1);
        Document.Scene = h.Scene; token = h.Token; Prune(); Notify();
    }
    void Prune() => Selection.RemoveWhere(id => Document is null || !Document.Scene.Objects.Any(a => a.Id == id));
    public IEnumerable<Guid> Unit(Guid id) => Document?.Scene.Groups.FirstOrDefault(g => g.Members.Contains(id))?.Members ?? (IEnumerable<Guid>)[id];
    public void Select(Guid? id, bool toggle = false)
    {
        if (!toggle) Selection.Clear();
        if (id is { } value)
        {
            var unit = Unit(value).ToArray(); bool remove = toggle && unit.All(Selection.Contains);
            foreach (var u in unit) { if (remove) Selection.Remove(u); else Selection.Add(u); }
        }
        Notify();
    }
    public void SelectFrame(Box box, bool add)
    {
        if (Document is null) return;
        if (!add) Selection.Clear();
        foreach (var a in Document.Scene.Objects)
        {
            var unit = Unit(a.Id).ToArray();
            if (unit.All(id => box.Contains(Document.Scene.Objects.First(o => o.Id == id).Bounds))) Selection.UnionWith(unit);
        }
        Notify();
    }
    public void Add(Annotation a) => Mutate(s => { s.Objects.Add(a); Selection.Clear(); Selection.Add(a.Id); });
    public void Delete() => Mutate(s => { s.Objects.RemoveAll(a => Selection.Contains(a.Id)); s.Groups.RemoveAll(g => g.Members.Any(Selection.Contains)); Selection.Clear(); });
    public void Move(Vec delta) { foreach (var a in Selected) a.Center += delta; Notify(); }
    public void Rotate(double angle)
    {
        var selected = Selected.ToArray();
        var pivot = selected.Length == 1 ? selected[0].Center : SelectionBounds.Center;
        foreach (var a in Selected) { a.Center = pivot + (a.Center - pivot).Rotate(angle); a.Rotation = Annotation.Normalize(a.Rotation + angle); }
        Notify();
    }
    public void Scale(double factor, Vec pivot)
    {
        if (factor is < .01 or > 100 || !double.IsFinite(factor)) return;
        foreach (var a in Selected) { a.Center = pivot + (a.Center - pivot) * factor; a.Scale(factor, factor, true); }
        Notify();
    }
    public void RestorePreview()
    {
        if (Document is not null && transaction is not null) Document.Scene = transaction.Clone();
    }
    public void Duplicate() => Mutate(s =>
    {
        int n = NextNumber(); var map = new Dictionary<Guid, Guid>(); var copies = new List<Annotation>();
        foreach (var a in s.Objects.Where(a => Selection.Contains(a.Id)).ToArray())
        {
            var b = a.Clone(); b.Id = Guid.NewGuid(); b.Center += new Vec(10, 10);
            if (b.Kind == ObjectKind.Number) b.Number = n++;
            map.Add(a.Id, b.Id); copies.Add(b);
        }
        foreach (var g in s.Groups.Where(g => g.Members.All(Selection.Contains)).ToArray()) s.Groups.Add(new() { Members = g.Members.Select(id => map[id]).ToList() });
        s.Objects.AddRange(copies); Selection.Clear(); Selection.UnionWith(copies.Select(a => a.Id));
    });
    public int NextNumber() => checked((Document?.Scene.Objects.Where(a => a.Kind == ObjectKind.Number).Select(a => a.Number).DefaultIfEmpty(0).Max() ?? 0) + 1);
    public void Order(bool front) => Mutate(s =>
    {
        var chosen = s.Objects.Where(a => Selection.Contains(a.Id)).ToList(); s.Objects.RemoveAll(a => Selection.Contains(a.Id));
        s.Objects.InsertRange(front ? s.Objects.Count : 0, chosen);
    });
    public bool CanGroup => Selected.Count() >= 2 && !(Document?.Scene.Groups.Any(g => g.Members.Count == Selection.Count && g.Members.All(Selection.Contains)) ?? false);
    public void Group()
    {
        if (!CanGroup) return;
        Mutate(s =>
        {
            var chosen = s.Objects.Where(a => Selection.Contains(a.Id)).ToList(); int max = s.Objects.FindLastIndex(a => Selection.Contains(a.Id));
            int insert = s.Objects.Take(max + 1).Count(a => !Selection.Contains(a.Id));
            s.Objects.RemoveAll(a => Selection.Contains(a.Id)); s.Objects.InsertRange(insert, chosen);
            s.Groups.RemoveAll(g => g.Members.Any(Selection.Contains)); s.Groups.Add(new() { Members = chosen.Select(a => a.Id).ToList() });
        });
    }
    public void Ungroup() => Mutate(s => s.Groups.RemoveAll(g => g.Members.All(Selection.Contains)));
    public void Crop(Box box)
    {
        if (Document is null) return;
        var b = box.Intersect(Document.Scene.Crop);
        var left = Math.Ceiling(b.X); var top = Math.Ceiling(b.Y);
        var crop = new Box(left, top, Math.Floor(b.Right) - left, Math.Floor(b.Bottom) - top);
        if (crop.Width >= 1 && crop.Height >= 1) Mutate(s => s.Crop = crop);
    }
}
