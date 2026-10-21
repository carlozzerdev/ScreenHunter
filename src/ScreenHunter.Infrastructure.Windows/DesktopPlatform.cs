using ScreenHunter.Application;
using ScreenHunter.Domain;

namespace ScreenHunter.Infrastructure.Windows;

public sealed class DesktopPlatform : IDesktopPlatform
{
    static Box BoxOf(Rectangle r) => new(r.X, r.Y, r.Width, r.Height);
    public Box VirtualBounds => BoxOf(SystemInformation.VirtualScreen);
    public IReadOnlyList<Box> Monitors => Screen.AllScreens.Select(s => BoxOf(s.Bounds)).ToArray();
    public IReadOnlyList<DesktopWindow> Windows => NativeDesktop.Windows().Select(w => new DesktopWindow(w.Handle, w.Title, BoxOf(w.Bounds))).ToArray();
    public Box WindowBounds(nint h) => BoxOf(NativeDesktop.WindowBounds(h));
    public CapturedImage Grab(Box area, bool includeCursor)
    {
        using var image = DxgiCapture.Capture(Rectangle.FromLTRB((int)area.X, (int)area.Y, (int)area.Right, (int)area.Bottom), includeCursor);
        var d = DocumentStore.FromBitmap(image); return new(d.ImagePng, d.Width, d.Height);
    }
    public bool EscapePressed => (NativeDesktop.GetAsyncKeyState(27) & 0x8000) != 0;
    public void Flush() => NativeDesktop.DwmFlush();
}
