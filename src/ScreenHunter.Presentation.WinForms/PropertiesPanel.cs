using System.Globalization;
using ScreenHunter.Application;
using ScreenHunter.Domain;
using ScreenHunter.Presentation.WinForms.Rendering;

namespace ScreenHunter.Presentation.WinForms;

public sealed class PropertiesPanel(EditorSession session, L10n l) : Panel
{
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public ObjectStyle DefaultStyle { get; set; } = new();
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public ObjectKind DefaultKind { get; set; } = ObjectKind.Text;
    public event Action<Annotation>? StyleEdited;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Func<Annotation, bool>? EditText { get; set; }
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Action<Exception>? OnError { get; set; }
    TableLayoutPanel? table;
    public void RefreshProperties()
    {
        SuspendLayout(); var old = table;
        table = new() { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Padding = new Padding(12), BackColor = Color.White };
        table.ColumnStyles.Add(new(SizeType.Percent, 54)); table.ColumnStyles.Add(new(SizeType.Percent, 46));
        Controls.Clear(); old?.Dispose(); Controls.Add(table); AutoScroll = true;
        var all = session.Selected.ToArray(); bool defaults = all.Length == 0;
        var targets = defaults ? new[] { new Annotation { Kind = DefaultKind, Style = DefaultStyle } } : all;
        bool Has(Func<Annotation, bool> match) => targets.Any(match);
        bool Textual(Annotation a) => a.Kind is ObjectKind.Text or ObjectKind.Callout or ObjectKind.Number;
        bool Shape(Annotation a) => !a.IsPath;
        void Edit(Action<Annotation> action, Func<Annotation, bool>? compatible = null)
        {
            try
            {
                if (defaults) action(targets[0]);
                else session.Mutate(_ => { foreach (var a in session.Selected.Where(compatible ?? (_ => true))) action(a); });
                foreach (var a in defaults ? targets : session.Selected.Where(compatible ?? (_ => true))) StyleEdited?.Invoke(a);
            }
            catch (Exception e) { OnError?.Invoke(e); }
        }
        void Add(string key, Control c)
        {
            int row = table.RowCount++; table.RowStyles.Add(new(SizeType.AutoSize));
            var label = new Label { Text = l[key], AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, 7, 6, 7) };
            c.Dock = DockStyle.Top; c.Margin = new Padding(0, 3, 0, 5); table.Controls.Add(label, 0, row); table.Controls.Add(c, 1, row);
        }
        void Number(string key, Func<Annotation, double> get, Action<Annotation, double> set, double min, double max, Func<Annotation, bool>? compatible = null)
        {
            var list = targets.Where(compatible ?? (_ => true)).ToArray(); if (list.Length == 0) return;
            var values = list.Select(get).ToArray(); string value = values.Distinct().Count() == 1 ? values[0].ToString("0.##", CultureInfo.CurrentCulture) : "";
            var box = new TextBox { Text = value, PlaceholderText = l["Mixed"] }; string initial = value;
            void Commit()
            {
                if (box.Text == initial) return;
                if (!double.TryParse(box.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out double n) || !double.IsFinite(n) || n < min || n > max) { box.Text = initial; return; }
                Edit(a => set(a, n), compatible); initial = box.Text;
            }
            box.Leave += (_, _) => Commit(); box.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { Commit(); e.SuppressKeyPress = true; } }; Add(key, box);
        }
        void Check(string key, Func<Annotation, bool> get, Action<Annotation, bool> set, Func<Annotation, bool>? compatible = null)
        {
            var list = targets.Where(compatible ?? (_ => true)).ToArray(); if (list.Length == 0) return;
            var values = list.Select(get).Distinct().ToArray();
            var check = new CheckBox { ThreeState = values.Length > 1, CheckState = values.Length > 1 ? CheckState.Indeterminate : values[0] ? CheckState.Checked : CheckState.Unchecked };
            check.Click += (_, _) => { bool v = check.CheckState != CheckState.Unchecked; check.ThreeState = false; check.Checked = v; Edit(a => set(a, v), compatible); }; Add(key, check);
        }
        void ColorButton(string key, Func<Annotation, uint> get, Action<Annotation, uint> set, Func<Annotation, bool>? compatible = null)
        {
            var list = targets.Where(compatible ?? (_ => true)).ToArray(); if (list.Length == 0) return;
            var b = new Button { Text = "●  " + (list.Select(get).Distinct().Count() > 1 ? l["Mixed"] : ""), ForeColor = SceneRenderer.ColorOf(get(list[0]) | 0xFF000000), Height = 29, FlatStyle = FlatStyle.Flat };
            b.Click += (_, _) => { using var dialog = new ColorDialog { FullOpen = true, Color = SceneRenderer.ColorOf(get(list[0]) | 0xFF000000) }; if (dialog.ShowDialog(FindForm()) == DialogResult.OK) { Edit(a => set(a, unchecked((uint)dialog.Color.ToArgb())), compatible); b.ForeColor = dialog.Color; b.Text = "●"; } }; Add(key, b);
        }
        var title = new Label { Text = defaults ? l["NewStyle"] : $"{l["Selection"]}: {all.Length}", AutoSize = true, Font = new Font(Font, FontStyle.Bold), Margin = new Padding(0, 0, 0, 12) };
        table.Controls.Add(title, 0, table.RowCount++); table.SetColumnSpan(title, 2);
        if (!defaults)
        {
            void Position(bool x)
            {
                double value = x ? session.SelectionBounds.Center.X : session.SelectionBounds.Center.Y;
                var input = new TextBox { Text = value.ToString("0.##", CultureInfo.CurrentCulture) }; string initial = input.Text;
                void Commit()
                {
                    if (initial == input.Text) return;
                    if (!double.TryParse(input.Text, out double target) || !double.IsFinite(target) || Math.Abs(target) > 100000) { input.Text = initial; return; }
                    var center = session.SelectionBounds.Center; var delta = x ? new Vec(target - center.X, 0) : new Vec(0, target - center.Y);
                    try { session.Mutate(_ => session.Move(delta)); initial = input.Text; } catch (Exception ex) { OnError?.Invoke(ex); }
                }
                input.Leave += (_, _) => Commit(); input.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { Commit(); e.SuppressKeyPress = true; } }; Add(x ? "X" : "Y", input);
            }
            Position(true); Position(false);
            if (all.Length == 1)
            {
                Number("Width", a => a.Width, (a, v) => { double scale = v / a.Width; a.Scale(scale, a.Kind == ObjectKind.Number ? scale : 1, a.Kind == ObjectKind.Number); }, 1, 100000);
                Number("Height", a => a.Height, (a, v) => { double scale = v / a.Height; a.Scale(a.Kind == ObjectKind.Number ? scale : 1, scale, a.Kind == ObjectKind.Number); }, 1, 100000);
                Number("Angle", a => a.Rotation, (a, v) => a.Rotation = Annotation.Normalize(v), -36000, 36000);
            }
            else
            {
                var angle = new TextBox { Text = "0" };
                void Rotate()
                {
                    if (double.TryParse(angle.Text, out var v) && double.IsFinite(v) && v != 0 && Math.Abs(v) <= 36000) { try { session.Mutate(_ => session.Rotate(v)); } catch (Exception e) { OnError?.Invoke(e); } angle.Text = "0"; }
                }
                angle.Leave += (_, _) => Rotate(); angle.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { Rotate(); e.SuppressKeyPress = true; } }; Add("RotateSelection", angle);
            }
            if (all.Length == 1 && all[0].Kind is ObjectKind.Text or ObjectKind.Callout)
            {
                var b = new Button { Text = l["EditText"], AutoSize = true }; b.Click += (_, _) => { session.Begin(); if (EditText?.Invoke(all[0]) == true) session.Commit(); else session.Cancel(); }; Add("Content", b);
            }
            if (all.Length == 1 && all[0].Kind == ObjectKind.Number) Number("Number", a => a.Number, (a,v) => a.Number = (int)v, 1, 1_000_000);
        }
        ColorButton("Stroke", a => a.Style.Stroke, (a,v) => a.Style.Stroke = v);
        Number("StrokeWidth", a => a.Style.StrokeWidth, (a,v) => a.Style.StrokeWidth = v, .25, 200);
        Number("Opacity", a => a.Style.Opacity * 100, (a,v) => a.Style.Opacity = v / 100, 1, 100);
        if (Has(Shape))
        {
            Check("Border", a => a.Style.Border, (a,v) => { a.Style.Border = v; if (!v && !a.Style.Filled && a.Kind is ObjectKind.Rectangle or ObjectKind.Ellipse) a.Style.Filled = true; }, Shape);
            Check("Filled", a => a.Style.Filled, (a,v) => { a.Style.Filled = v; if (!v && !a.Style.Border && a.Kind is ObjectKind.Rectangle or ObjectKind.Ellipse) a.Style.Border = true; }, Shape);
            ColorButton("Fill", a => a.Style.Fill, (a,v) => a.Style.Fill = v, Shape);
            Number("CornerRadius", a => a.Style.CornerRadius, (a,v) => a.Style.CornerRadius = v, 0, 1000, Shape);
        }
        Check("Shadow", a => a.Style.Shadow, (a,v) => a.Style.Shadow = v);
        Check("Dashed", a => a.Style.Dashed, (a,v) => a.Style.Dashed = v);
        if (Has(a => a.Kind == ObjectKind.Arrow))
        {
            Check("DoubleArrow", a => a.Style.DoubleArrow, (a,v) => a.Style.DoubleArrow = v, a => a.Kind == ObjectKind.Arrow);
            Number("ArrowSize", a => a.Style.ArrowSize, (a,v) => a.Style.ArrowSize = v, 2, 200, a => a.Kind == ObjectKind.Arrow);
        }
        if (Has(Textual))
        {
            ColorButton("TextColor", a => a.Style.TextColor, (a,v) => a.Style.TextColor = v, Textual);
            Number("FontSize", a => a.Style.FontSize, (a,v) => a.Style.FontSize = v, 1, 600, Textual);
            var names = targets.Where(Textual).Select(a => a.Style.FontFamily).Distinct().ToArray();
            var family = new ComboBox { DropDownStyle = ComboBoxStyle.DropDown, Text = names.Length == 1 ? names[0] : "" }; family.Items.AddRange(FontFamily.Families.Select(f => (object)f.Name).ToArray());
            family.SelectionChangeCommitted += (_, _) => Edit(a => a.Style.FontFamily = family.Text, Textual); family.Leave += (_, _) => { if (!string.IsNullOrWhiteSpace(family.Text)) Edit(a => a.Style.FontFamily = family.Text, Textual); }; Add("FontFamily", family);
            Check("Bold", a => a.Style.Bold, (a,v) => a.Style.Bold = v, Textual); Check("Italic", a => a.Style.Italic, (a,v) => a.Style.Italic = v, Textual); Check("Underline", a => a.Style.Underline, (a,v) => a.Style.Underline = v, Textual);
            Number("Padding", a => a.Style.Padding, (a,v) => a.Style.Padding = v, 0, 200, Textual);
            var alignment = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList }; alignment.Items.AddRange([l["Left"], l["Center"], l["Right"]]); var aligns = targets.Where(Textual).Select(a => a.Style.Alignment).Distinct().ToArray(); alignment.SelectedIndex = aligns.Length == 1 ? aligns[0] : -1; alignment.SelectionChangeCommitted += (_, _) => Edit(a => a.Style.Alignment = alignment.SelectedIndex, Textual); Add("Alignment", alignment);
        }
        if (Has(a => a.Kind == ObjectKind.Callout)) Check("OvalCallout", a => a.Style.OvalCallout, (a,v) => a.Style.OvalCallout = v, a => a.Kind == ObjectKind.Callout);
        ResumeLayout(true);
    }
}
