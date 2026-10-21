using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using ScreenHunter.Application;
using ScreenHunter.Domain;
using ScreenHunter.Infrastructure.Windows;
using ScreenHunter.Presentation.WinForms.Rendering;

namespace ScreenHunter.Presentation.WinForms;

internal static class SelfTest
{
    public static int Run(string[] args)
    {
        string directory = Path.GetFullPath(args.SkipWhile(a => a != "--output").Skip(1).FirstOrDefault() ?? Path.Combine(Environment.CurrentDirectory, "artifacts", "verification"));
        Directory.CreateDirectory(directory); var checks = new List<string>();
        void Check(bool valid, string name) { if (!valid) throw new Exception(name); checks.Add(name); }
        try
        {
            foreach (var kind in Enum.GetValues<ObjectKind>())
            {
                var item = new Annotation { Kind = kind, Center = new(70, 60), Width = 80, Height = 40, Text = "Test", Tail = new(0, 75), Style = new() { Filled = true } };
                if (item.IsPath) item.Points = [new(-30, 0), new(30, 0)];
                using var plain = new Bitmap(160, 160); using var shaded = new Bitmap(160, 160);
                using (var g = Graphics.FromImage(plain)) { SceneRenderer.Configure(g); SceneRenderer.Draw(g, item); }
                if (kind == ObjectKind.Callout)
                {
                    Check(plain.GetPixel(70, 120).A == 0 && !SceneRenderer.Hit(item, new(70,135), 2) && item.Bounds.Bottom < 90, "Callout has no rendered, selectable or bounded tail");
                }
                item.Style.Shadow = true;
                using (var g = Graphics.FromImage(shaded)) { SceneRenderer.Configure(g); SceneRenderer.Draw(g, item); }
                bool extra = false;
                for (int y = 0; y < 160; y++) for (int x = 0; x < 160; x++)
                    if (shaded.GetPixel(x,y).A > plain.GetPixel(x,y).A) extra = true;
                Check(extra, "Shadow adds pixels for " + kind);
                var testDoc = DocumentStore.FromBitmap(plain); testDoc.Scene.Objects.Add(item);
                string path = Path.Combine(directory, "shadow-" + kind + ".shproj");
                var testStore = new DocumentStore(); testStore.Save(path, testDoc);
                Check(testStore.Load(path).Scene.Objects.Single().Style.Shadow, "Shadow project round trip for " + kind);
                File.Delete(path);
            }
            using var original = new Bitmap(1000, 650);
            using (var g = Graphics.FromImage(original))
            {
                g.Clear(Color.White); using var brush = new SolidBrush(Color.FromArgb(234,240,249)); g.FillRectangle(brush, 30, 30, 940, 590);
                using var font = new Font("Segoe UI", 24, FontStyle.Bold); g.DrawString("ScreenHunter · synthetic test", font, Brushes.MidnightBlue, 60, 50);
                using var small = new Font("Segoe UI", 13); g.DrawString("Local image · editable annotations · no private screen content", small, Brushes.SlateGray, 62, 105);
            }
            var doc = DocumentStore.FromBitmap(original); var session = new EditorSession(); session.Open(doc, true);
            var kinds = Enum.GetValues<ObjectKind>();
            for (int i=0;i<kinds.Length;i++)
            {
                var k=kinds[i]; var a = new Annotation { Kind=k,Center=new(170+(i%3)*310,220+(i/3)*160),Width=200,Height=95,Text=k==ObjectKind.Callout?"Revisar aquí":"Texto editable",Number=1,Rotation=k==ObjectKind.Text?-8:0,Style=new(){Filled=k is ObjectKind.Callout or ObjectKind.Number, StrokeWidth=k==ObjectKind.Highlighter?22:3,Stroke=k==ObjectKind.Highlighter?0xFFFFFF00:0xFFE54747,Opacity=k==ObjectKind.Highlighter?.3:1,FontSize=20},Tail=new(65,70) };
                if(a.IsPath) a.Points=[new(-80,10),new(-30,-20),new(30,20),new(80,-10)];
                if(k is ObjectKind.Arrow or ObjectKind.Line) a.Points=[new(-80,25),new(80,-25)];
                if(k==ObjectKind.Number) a.Width=a.Height=65;
                session.Add(a);
            }
            session.Selection.Clear();session.Selection.UnionWith(doc.Scene.Objects.Take(2).Select(a=>a.Id));session.Group();session.MarkSaved();
            string project=Path.Combine(directory,"sample.shproj");var store=new DocumentStore();store.Save(project,doc);var loaded=store.Load(project);
            Check(loaded.Scene.Objects.Count==9 && loaded.Scene.Groups.Count==1,"Project round trip: nine types, group, styles and rotations");
            Check(DocumentRules.Fingerprint(loaded.Scene)==DocumentRules.Fingerprint(doc.Scene),"Exact scene round trip");
            using var rendered=SceneRenderer.Compose(doc,original);rendered.Save(Path.Combine(directory,"objects.png"),ImageFormat.Png);
            var imageOutput=new ImageOutput();using var encoded=new MemoryStream();rendered.Save(encoded,ImageFormat.Png);var png=encoded.ToArray();
            imageOutput.Export(png,Path.Combine(directory,"export.jpg"),true,82);using var jpeg=store.Import(Path.Combine(directory,"export.jpg")).ImagePng is { } jp?DocumentStore.Decode(jp):throw new Exception();
            Check(jpeg.Width==1000 && jpeg.Height==650,"PNG/JPEG export and import dimensions");
            var crop=doc.Scene.Crop;doc.Scene.Crop=new(100,100,400,300);using(var cropped=SceneRenderer.Compose(doc,original))Check(cropped.Width==400 && cropped.Height==300,"Crop export dimensions independent of zoom");doc.Scene.Crop=crop;
            byte[] corrupt=[1,2,3,4];bool rejected=false;try{using var bad=DocumentStore.Decode(corrupt);}catch(InvalidDataException){rejected=true;}Check(rejected,"Reject invalid raster");
            string evil=Path.Combine(directory,"invalid.shproj");using(var zip=ZipFile.Open(evil,ZipArchiveMode.Create)){zip.CreateEntry("../outside");}rejected=false;try{store.Load(evil);}catch(InvalidDataException){rejected=true;}Check(rejected,"Reject unexpected/path-traversal archive entry");File.Delete(evil);
            var prefPath=Path.Combine(directory,"settings-test.json");var prefStore=new PreferencesStore(prefPath);prefStore.Save(new(){Language="en",DelaySeconds=5,IncludeCursor=true,Mode=CaptureMode.Window});var pref=prefStore.Load();Check(pref.Language=="en"&&pref.DelaySeconds==5&&pref.IncludeCursor&&pref.Mode==CaptureMode.Window,"Preferences round trip");File.Delete(prefPath);
            DxgiCapture.ToneMap(0,0,0,out var r,out var g0,out var b0);Check(r==0&&g0==0&&b0==0,"HDR black mapping");DxgiCapture.ToneMap(10,10,10,out r,out g0,out b0);Check(r==g0&&r==b0&&r>240,"HDR neutral shoulder preserves neutral highlights (algorithm only)");
            Check(!DxgiCapture.HasDesktopImage(0,0)&&!DxgiCapture.HasDesktopImage(0,1)&&!DxgiCapture.HasDesktopImage(10,0)&&DxgiCapture.HasDesktopImage(10,1),"Reject cursor-only/uninitialized duplication frames; accept a presented desktop image");
            using(var selection=new SelectionOverlay(new(0,0,1000,650),original,CaptureMode.Region,[],[],new L10n()))
            {
                selection.CreateControl();
                typeof(SelectionOverlay).GetMethod("OnMouseDown",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(selection,[new MouseEventArgs(MouseButtons.Left,1,100,100,0)]);
                typeof(SelectionOverlay).GetMethod("OnMouseMove",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(selection,[new MouseEventArgs(MouseButtons.Left,1,500,400,0)]);
                using var overlayImage=new Bitmap(1000,650);selection.DrawToBitmap(overlayImage,new Rectangle(0,0,1000,650));
                Check(overlayImage.GetPixel(200,200).ToArgb()==original.GetPixel(200,200).ToArgb(),"Selection preview keeps original RGB pixels inside the region");
                Check(overlayImage.GetPixel(40,40).R>0,"Selection preview dims the background without making it black");
                overlayImage.Save(Path.Combine(directory,"selection-preview.png"),ImageFormat.Png);
            }
            var l=new L10n();l.Set("es");Check(l["Group"]=="Agrupar","Spanish resources");l.Set("en");Check(l["Group"]=="Group","English resources");l.Set("es");
            using var form=new MainForm(session,l);form.ConfirmExit=()=>true;form.Canvas.BaseImage=new Bitmap(original);form.Show();form.Canvas.Fit();System.Windows.Forms.Application.DoEvents();
            using(var snapshot=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(snapshot,new Rectangle(Point.Empty,form.Size));snapshot.Save(Path.Combine(directory,"editor-es.png"),ImageFormat.Png);}
            Check(form.Canvas.Width>400 && form.Properties.Width>=250,"Editor layout has usable canvas and property panel");
            l.Set("en");form.Localize();System.Windows.Forms.Application.DoEvents();using(var snapshot=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(snapshot,new Rectangle(Point.Empty,form.Size));snapshot.Save(Path.Combine(directory,"editor-en.png"),ImageFormat.Png);}
            Check(doc.Scene.Objects.First(a=>a.Kind==ObjectKind.Text).Text=="Texto editable","Language change does not translate annotations");
            var canvas=form.Canvas;session.Select(null);canvas.Tool=EditorTool.Rectangle;
            void Mouse(string name, PointF p)=>typeof(EditorCanvas).GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(canvas,[new MouseEventArgs(MouseButtons.Left,1,(int)p.X,(int)p.Y,0)]);
            int before=doc.Scene.Objects.Count;Mouse("OnMouseDown",canvas.ToClient(new(90,130)));Mouse("OnMouseMove",canvas.ToClient(new(230,210)));Mouse("OnMouseUp",canvas.ToClient(new(230,210)));Check(doc.Scene.Objects.Count==before+1,"Canvas pointer gesture creates an editable rectangle");
            session.Undo();Check(session.Document!.Scene.Objects.Count==before,"Canvas creation undoes in one operation");
            canvas.EditText=a=>{a.Text="Synthetic editable text";return true;};
            foreach(var tool in Enum.GetValues<EditorTool>().Where(t=>t is not(EditorTool.Select or EditorTool.Crop)))
            {
                int count=doc.Scene.Objects.Count;canvas.Tool=tool;canvas.NewStyle=new(){Filled=tool is EditorTool.Callout or EditorTool.Number};
                Mouse("OnMouseDown",canvas.ToClient(new(40,140)));Mouse("OnMouseMove",canvas.ToClient(new(140,190)));Mouse("OnMouseUp",canvas.ToClient(new(140,190)));
                Check(doc.Scene.Objects.Count==count+1&&doc.Scene.Objects[^1].Kind.ToString()==tool.ToString(),"Canvas creation gesture: "+tool);
                session.Undo();
            }
            l.Set("en");
            using(var timer=new System.Windows.Forms.Timer{Interval=200})
            {
                timer.Tick+=(_,_)=>{var dialog=System.Windows.Forms.Application.OpenForms.Cast<Form>().FirstOrDefault(f=>f.Text==l["Settings"]);if(dialog is null)return;timer.Stop();using var shot=new Bitmap(dialog.Width,dialog.Height);dialog.DrawToBitmap(shot,new Rectangle(Point.Empty,dialog.Size));shot.Save(Path.Combine(directory,"settings-en.png"));dialog.DialogResult=DialogResult.Cancel;dialog.Close();};
                timer.Start();Check(Dialogs.Settings(form,new Preferences(),l)is null,"Settings Cancel preserves preferences");
            }
            using var perfImage=new Bitmap(3840,2160);using(var gp=Graphics.FromImage(perfImage))gp.Clear(Color.White);
            var perfDoc=DocumentStore.FromBitmap(perfImage);
            for(int i=0;i<100;i++)perfDoc.Scene.Objects.Add(new(){Kind=ObjectKind.Text,Center=new(150+(i%10)*350,100+(i/10)*200),Width=260,Height=95,Text="Synthetic text "+i,Style=new(){Filled=true,FontSize=18}});
            var timings=new List<double>();
            for(int i=0;i<10;i++){var watch=Stopwatch.StartNew();using var renderedPerf=SceneRenderer.Compose(perfDoc,perfImage);using var streamPerf=new MemoryStream();renderedPerf.Save(streamPerf,ImageFormat.Png);watch.Stop();timings.Add(watch.Elapsed.TotalMilliseconds);}
            var sorted=timings.Order().ToArray();File.WriteAllText(Path.Combine(directory,"performance.json"),JsonSerializer.Serialize(new{scenario="4K SDR, 100 text objects, composition plus PNG encoding, no disk/clipboard",runs=timings,medianMs=(sorted[4]+sorted[5])/2,maxMs=sorted[^1],cpu=Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"),availableMemoryBytes=GC.GetGCMemoryInfo().TotalAvailableMemoryBytes},new JsonSerializerOptions{WriteIndented=true}));
            form.ExitApplication();
            if (args.Contains("--capture-test"))
            {
                // Capture only the interior of this application's opaque synthetic window.
                // No screenshot of the surrounding desktop is persisted or inspected.
                using var target = new Form { FormBorderStyle=FormBorderStyle.None, StartPosition=FormStartPosition.Manual, Bounds=new Rectangle(Screen.PrimaryScreen!.Bounds.X+40,Screen.PrimaryScreen.Bounds.Y+40,720,460), TopMost=true, ShowInTaskbar=false, BackColor=Color.FromArgb(20,180,220) };
                target.Paint+=(_,ev)=>{ev.Graphics.Clear(Color.FromArgb(20,180,220));ev.Graphics.FillRectangle(Brushes.Magenta,360,0,360,460);};
                target.Show();target.Activate();System.Windows.Forms.Application.DoEvents();Thread.Sleep(250);NativeDesktop.DwmFlush();
                var area=target.RectangleToScreen(new Rectangle(20,20,680,400));
                using var actual=DxgiCapture.Capture(area,false);
                var left=actual.GetPixel(30,30);var right=actual.GetPixel(600,30);
                Check(Math.Abs(left.R-20)<12&&Math.Abs(left.G-180)<12&&Math.Abs(left.B-220)<12&&right.R>240&&right.B>240&&right.G<15,"Desktop capture of an opaque synthetic window: non-black reference colors");
                Check(actual.Width==680&&actual.Height==400&&left.A==255&&right.A==255,"Desktop capture region dimensions and opaque alpha");
                var capturedDoc=DocumentStore.FromBitmap(actual);using var decodedCapture=CaptureFlow.Decode(capturedDoc.ImagePng);
                Check(decodedCapture.GetPixel(30,30).ToArgb()==left.ToArgb(),"Captured image remains visible after PNG encode/decode");
                using var hk1=new GlobalHotkey(target.Handle);Check(hk1.Change(3,122),"Global shortcut registration");Check(hk1.Change(3,122),"Applying unchanged shortcut keeps registration");Check(hk1.Change(3,121),"Global shortcut replacement");
                target.Close();
            }
            File.WriteAllText(Path.Combine(directory,"report.json"),JsonSerializer.Serialize(new{status="passed",checks,framework=System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,os=Environment.OSVersion.ToString(),capture=args.Contains("--capture-test")?"Synthetic window only; HDR/multi-monitor hardware acceptance pending":"Not exercised on user desktop; HDR/multi-monitor hardware acceptance pending"},new JsonSerializerOptions{WriteIndented=true}));
            return 0;
        }
        catch(Exception e)
        {
            File.WriteAllText(Path.Combine(directory,"report.json"),JsonSerializer.Serialize(new{status="failed",checks,error=e.ToString()},new JsonSerializerOptions{WriteIndented=true}));return 1;
        }
    }
}
