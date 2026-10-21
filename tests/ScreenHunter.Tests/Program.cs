using ScreenHunter.Application;
using ScreenHunter.Domain;

try
{
int passed = 0;
void Check(bool condition, string message) { if (!condition) throw new Exception(message); passed++; Console.WriteLine("PASS " + message); }
Annotation Shape(double x = 100) => new() { Kind = ObjectKind.Rectangle, Center = new(x, 100), Width = 80, Height = 40 };
var e = new EditorSession(); e.Open(new() { Width = 1000, Height = 800, Scene = new() { Crop = new(0,0,1000,800) } }, true);
var a = Shape(); var b = Shape(300); var middle = Shape(200);
e.Add(a); e.Add(middle); e.Add(b); e.MarkSaved();
e.Select(a.Id); e.Select(b.Id, true); e.Group();
Check(e.Document!.Scene.Groups.Count == 1 && e.Document.Scene.Objects[0].Id == middle.Id, "Grouping interleaved objects makes a topmost contiguous block");
e.Undo(); Check(e.Document.Scene.Groups.Count == 0 && e.Document.Scene.Objects[1].Id == middle.Id && !e.Dirty, "Undo restores z-order and saved-state token");
e.Redo(); e.Select(a.Id); Check(e.Selection.Count == 2, "Click on member selects persistent group");
var bounds = e.SelectionBounds; e.Mutate(_ => e.Rotate(90));
var aa = e.Selected.First(v=>v.Id==a.Id); var bb = e.Selected.First(v=>v.Id==b.Id);
Check(Math.Abs(aa.Center.X - bb.Center.X) < .001 && Math.Abs(Math.Abs(aa.Center.Y - bb.Center.Y) - 200) < .001, "Group rotation preserves center distance");
Check(Math.Abs(e.SelectionBounds.Center.X - bounds.Center.X) < .001, "Rotation preserves common pivot");
e.Ungroup(); Check(e.Document.Scene.Groups.Count == 0 && aa.Rotation == 90, "Ungroup preserves transformed appearance");
e.Undo(); Check(e.Document.Scene.Groups.Count == 1, "Undo ungroup restores membership");
e.Duplicate(); Check(e.Document.Scene.Groups.Count == 2 && e.Selection.Count == 2 && e.Document.Scene.Objects.Select(o=>o.Id).Distinct().Count()==5, "Duplicate gives new object/group identities");
e.Begin(); e.Move(new(42,7)); e.Cancel(); Check(!e.Selected.Any(v=>Math.Abs(v.Center.X - aa.Center.X-52)<.001), "Cancel restores transaction");
e.Undo(); e.Add(Shape(600)); Check(!e.CanRedo, "New edit invalidates redo branch");
e.Crop(new(10,10,50,40)); Check(e.Document.Scene.Crop.Width == 50 && e.Document.Scene.Objects.Count > 0, "Crop preserves annotations"); e.Undo();
e.Select(null); e.SelectFrame(new(-1000,-1000,3000,3000), false); Check(e.Selection.Count==e.Document.Scene.Objects.Count,"Marquee includes complete units");
e.Delete(); Check(e.Document.Scene.Objects.Count==0 && e.Document.Scene.Groups.Count==0,"Delete selection removes associated groups");e.Undo();
var n = new Annotation { Kind=ObjectKind.Number,Number=3,Center=new(400,400) };e.Add(n);e.Duplicate();Check(e.Selected.Single().Number==4,"Duplicate marker advances maximum");var copyId=e.Selected.Single().Id;e.Undo();e.Redo();Check(e.Document.Scene.Objects.Single(o=>o.Id==copyId).Number==4,"Redo preserves assigned marker number");e.Select(copyId);
var before = DocumentRules.Fingerprint(e.Document.Scene);e.Begin();e.Move(new(30,40));e.Move(new(2,2));e.Commit();e.Undo();Check(DocumentRules.Fingerprint(e.Document.Scene)==before,"One transaction undoes the entire gesture");
var bad = e.Document.Scene.Objects[0].Clone();bad.Id=e.Document.Scene.Objects[0].Id;
bool rejected=false;try{e.Add(bad);}catch(InvalidDataException){rejected=true;}Check(rejected && !e.InTransaction,"Invalid mutation rolls back");
var d=e.Document; d.Scene.Groups.Add(new(){Members=[d.Scene.Objects[0].Id,Guid.NewGuid()]}); rejected=false;try{DocumentRules.Validate(d);}catch(InvalidDataException){rejected=true;}Check(rejected,"Reject dangling group members");d.Scene.Groups.Clear();
rejected=false;try{DocumentRules.CheckImage(100000,100000);}catch(InvalidDataException){rejected=true;}Check(rejected,"Enforce decoded image memory limits");
Check(Annotation.Normalize(-15)==345 && Annotation.Normalize(720)==0,"Normalize rotation");
var callout=new Annotation{Kind=ObjectKind.Callout,Center=new(150,150),Tail=new(100,300)};e.Add(callout);e.Mutate(_=>e.Rotate(30));Check((e.Selected.Single().Center-new Vec(150,150)).Length<1e-9,"Individual callout rotates around body center, not tail extent");
var s=new Annotation{Center=new(30,40),Rotation=37};var p=new Vec(5,-12);Check((s.ToLocal(s.ToWorld(p))-p).Length<1e-9,"Inverse transformation round trip");
var pref=new Preferences();pref.Validate();pref.Modifiers=8;rejected=false;try{pref.Validate();}catch(InvalidDataException){rejected=true;}Check(rejected,"Reject reserved Windows-modifier shortcuts");
Check(!System.Text.Json.JsonSerializer.Deserialize<ObjectStyle>("{}")!.Shadow, "Legacy styles load without shadow");
e.Select(callout.Id); e.Mutate(_ => e.Selected.Single().Style.Shadow = true);
Check(e.Selected.Single().Clone().Style.Shadow, "Clone retains shadow");
e.Undo(); Check(!e.Document.Scene.Objects.Single(o => o.Id == callout.Id).Style.Shadow, "Undo shadow");
e.Redo(); Check(e.Document.Scene.Objects.Single(o => o.Id == callout.Id).Style.Shadow, "Redo shadow");
Console.WriteLine($"{passed} assertions passed.");
return 0;
}
catch(Exception ex) { Console.Error.WriteLine(ex); return 1; }
