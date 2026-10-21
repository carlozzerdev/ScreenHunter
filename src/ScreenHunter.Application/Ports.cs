using ScreenHunter.Domain;

namespace ScreenHunter.Application;

public enum CaptureMode { Region, Window, Monitor }
public sealed record CapturedImage(byte[] Png, int Width, int Height);
public interface IDocumentStore
{
    ImageDocument Load(string path);
    void Save(string path, ImageDocument document);
    ImageDocument Import(string path);
}
public sealed class Preferences
{
    public int Version { get; set; } = 1;
    public string Language { get; set; } = "es";
    public int Key { get; set; } = 83;
    public int Modifiers { get; set; } = 3;
    public CaptureMode Mode { get; set; }
    public int DelaySeconds { get; set; }
    public bool IncludeCursor { get; set; }
    public Preferences Clone() => (Preferences)MemberwiseClone();
    public void Validate()
    {
        if (Version != 1 || Language is not ("es" or "en") || !Enum.IsDefined(Mode) || DelaySeconds is not (0 or 3 or 5 or 10) || Key is < 32 or > 254 || Key == 123 || Modifiers is < 1 or > 7) throw new InvalidDataException("InvalidSettings");
    }
}
public interface IPreferencesStore { Preferences Load(); void Save(Preferences preferences); }
public sealed record DesktopWindow(nint Handle, string Title, Box Bounds);
public interface IDesktopPlatform
{
    Box VirtualBounds { get; }
    IReadOnlyList<Box> Monitors { get; }
    IReadOnlyList<DesktopWindow> Windows { get; }
    Box WindowBounds(nint handle);
    CapturedImage Grab(Box area, bool includeCursor);
    bool EscapePressed { get; }
    void Flush();
}
public interface IHotkey : IDisposable { bool Change(int modifiers, int key); }
public interface IImageOutput
{
    void Copy(byte[] png);
    void Export(byte[] png, string path, bool jpeg, int quality);
}
