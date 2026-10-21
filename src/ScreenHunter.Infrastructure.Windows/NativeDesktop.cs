using System.Runtime.InteropServices;
using System.Text;

namespace ScreenHunter.Infrastructure.Windows;

public static partial class NativeDesktop
{
    [StructLayout(LayoutKind.Sequential)] public struct Rect { public int Left, Top, Right, Bottom; public readonly Rectangle Rectangle => Rectangle.FromLTRB(Left, Top, Right, Bottom); }
    [DllImport("user32.dll")] public static extern bool RegisterHotKey(nint h, int id, uint modifiers, uint key);
    [DllImport("user32.dll")] public static extern bool UnregisterHotKey(nint h, int id);
    [DllImport("user32.dll")] public static extern short GetAsyncKeyState(int key);
    [DllImport("dwmapi.dll")] static extern int DwmGetWindowAttribute(nint hwnd, int attribute, out Rect value, int size);
    [DllImport("user32.dll")] static extern bool EnumWindows(EnumWindow callback, nint param);
    delegate bool EnumWindow(nint hwnd, nint param);
    [DllImport("user32.dll")] public static extern bool IsWindow(nint hwnd);
    [DllImport("user32.dll")] static extern bool IsWindowVisible(nint hwnd);
    [DllImport("user32.dll")] static extern bool IsIconic(nint hwnd);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern int GetWindowText(nint hwnd, StringBuilder text, int count);
    [DllImport("user32.dll")] static extern bool GetWindowRect(nint hwnd, out Rect rect);
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(nint hwnd, out uint pid);
    [DllImport("dwmapi.dll")] public static extern int DwmFlush();
    public static Rectangle WindowBounds(nint hwnd)
    {
        if (!IsWindow(hwnd) || IsIconic(hwnd)) throw new InvalidOperationException("CaptureUnavailable");
        if (DwmGetWindowAttribute(hwnd, 9, out var r, Marshal.SizeOf<Rect>()) != 0 && !GetWindowRect(hwnd, out r)) throw new InvalidOperationException("CaptureUnavailable");
        return r.Rectangle;
    }
    public sealed record WindowInfo(nint Handle, string Title, Rectangle Bounds);
    public static List<WindowInfo> Windows()
    {
        var list = new List<WindowInfo>();
        EnumWindows((h, _) =>
        {
            GetWindowThreadProcessId(h, out uint pid);
            if (pid == Environment.ProcessId || !IsWindowVisible(h) || IsIconic(h)) return true;
            var s = new StringBuilder(512); GetWindowText(h, s, s.Capacity);
            if (s.Length > 0) try { var b = WindowBounds(h); if (b.Width > 1 && b.Height > 1) list.Add(new(h, s.ToString(), b)); } catch (InvalidOperationException) { }
            return true;
        }, 0);
        return list;
    }
    [StructLayout(LayoutKind.Sequential)] struct CursorInfo { public int Size, Flags; public nint Handle; public Point Position; }
    [StructLayout(LayoutKind.Sequential)] struct IconInfo { public int IsIcon; public uint XHotspot, YHotspot; public nint Mask, Color; }
    [DllImport("user32.dll")] static extern bool GetCursorInfo(ref CursorInfo info);
    [DllImport("user32.dll")] static extern nint CopyIcon(nint icon);
    [DllImport("user32.dll")] static extern bool GetIconInfo(nint icon, out IconInfo info);
    [DllImport("user32.dll")] static extern bool DestroyIcon(nint icon);
    [DllImport("gdi32.dll")] static extern bool DeleteObject(nint obj);
    [DllImport("user32.dll")] static extern bool DrawIconEx(nint dc, int x, int y, nint icon, int width, int height, uint step, nint brush, uint flags);
    public static void DrawCursor(Bitmap image, Point origin)
    {
        var ci = new CursorInfo { Size = Marshal.SizeOf<CursorInfo>() };
        if (!GetCursorInfo(ref ci) || ci.Flags != 1) return;
        nint icon = CopyIcon(ci.Handle);
        if (icon == 0) return;
        try
        {
            if (!GetIconInfo(icon, out var info)) return;
            try
            {
                using var g = Graphics.FromImage(image); var dc = g.GetHdc();
                try { DrawIconEx(dc, ci.Position.X - origin.X - (int)info.XHotspot, ci.Position.Y - origin.Y - (int)info.YHotspot, icon, 0, 0, 0, 0, 3); }
                finally { g.ReleaseHdc(dc); }
            }
            finally { if (info.Mask != 0) DeleteObject(info.Mask); if (info.Color != 0) DeleteObject(info.Color); }
        }
        finally { DestroyIcon(icon); }
    }
}

public sealed class GlobalHotkey(nint handle) : ScreenHunter.Application.IHotkey
{
    int current;
    int registeredModifiers, registeredKey;
    public bool Change(int modifiers, int key)
    {
        if (current != 0 && modifiers == registeredModifiers && key == registeredKey) return true;
        int next = current == 1 ? 2 : 1;
        if (!NativeDesktop.RegisterHotKey(handle, next, (uint)modifiers | 0x4000, (uint)key)) return false;
        if (current != 0) NativeDesktop.UnregisterHotKey(handle, current);
        current = next; registeredModifiers = modifiers; registeredKey = key; return true;
    }
    public void Dispose() { if (current != 0) NativeDesktop.UnregisterHotKey(handle, current); current = 0; }
}
