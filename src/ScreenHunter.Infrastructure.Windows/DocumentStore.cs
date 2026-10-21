using System.Buffers.Binary;
using System.Drawing.Imaging;
using System.IO.Compression;
using System.Text.Json;
using ScreenHunter.Application;
using ScreenHunter.Domain;

namespace ScreenHunter.Infrastructure.Windows;

public sealed class DocumentStore : IDocumentStore
{
    const long MaxFile = 256_000_000;
    public ImageDocument Load(string path)
    {
        if (new FileInfo(path).Length > MaxFile) throw new InvalidDataException("ImageLimit");
        using var zip = ZipFile.OpenRead(path);
        if (zip.Entries.Count != 2 || zip.Entries.Any(e => e.FullName is not ("document.json" or "base.png")) || zip.Entries.Select(e => e.FullName).Distinct().Count() != 2) throw new InvalidDataException("InvalidProject");
        var metadata = Read(zip.GetEntry("document.json")!, 16_000_000);
        var d = JsonSerializer.Deserialize<ImageDocument>(metadata, new JsonSerializerOptions { MaxDepth = 32 }) ?? throw new InvalidDataException("InvalidProject");
        DocumentRules.Validate(d);
        var bytes = Read(zip.GetEntry("base.png")!, MaxFile);
        using var image = Decode(bytes);
        if (image.Width != d.Width || image.Height != d.Height) throw new InvalidDataException("InvalidProject");
        d.ImagePng = bytes; return d;
    }
    static byte[] Read(ZipArchiveEntry e, long max)
    {
        if (e.Length <= 0 || e.Length > max) throw new InvalidDataException("ImageLimit");
        using var input = e.Open(); using var output = new MemoryStream(); input.CopyTo(output);
        return output.ToArray();
    }
    public void Save(string path, ImageDocument document)
    {
        DocumentRules.Validate(document);
        AtomicFile.Write(path, tmp =>
        {
            using var zip = ZipFile.Open(tmp, ZipArchiveMode.Create);
            using (var s = zip.CreateEntry("document.json").Open()) JsonSerializer.Serialize(s, document);
            using (var s = zip.CreateEntry("base.png", CompressionLevel.NoCompression).Open()) s.Write(document.ImagePng);
        });
    }
    public ImageDocument Import(string path)
    {
        if (new FileInfo(path).Length > MaxFile) throw new InvalidDataException("ImageLimit");
        using var image = Decode(File.ReadAllBytes(path)); return FromBitmap(image);
    }
    public static ImageDocument FromBitmap(Bitmap bitmap)
    {
        DocumentRules.CheckImage(bitmap.Width, bitmap.Height);
        using var s = new MemoryStream(); bitmap.Save(s, ImageFormat.Png);
        return new() { Width = bitmap.Width, Height = bitmap.Height, ImagePng = s.ToArray(), Scene = new() { Crop = new(0, 0, bitmap.Width, bitmap.Height) } };
    }
    public static Bitmap Decode(byte[] bytes)
    {
        // Check raster size before asking GDI+ to allocate/decode pixels.
        var (w, h) = ReadSize(bytes); DocumentRules.CheckImage(w, h);
        using var stream = new MemoryStream(bytes, false);
        using var image = Image.FromStream(stream, false, true);
        if (image.Width != w || image.Height != h) throw new InvalidDataException("InvalidImage");
        return new Bitmap(image);
    }
    static (int, int) ReadSize(byte[] b)
    {
        if (b.Length >= 24 && b.AsSpan(0, 8).SequenceEqual(new byte[] {137,80,78,71,13,10,26,10}))
            return (BinaryPrimitives.ReadInt32BigEndian(b.AsSpan(16)), BinaryPrimitives.ReadInt32BigEndian(b.AsSpan(20)));
        if (b.Length > 4 && b[0] == 255 && b[1] == 216)
        {
            int p = 2;
            while (p + 3 < b.Length)
            {
                if (b[p++] != 255) break;
                while (p < b.Length && b[p] == 255) p++;
                if (p >= b.Length) break;
                int marker = b[p++];
                if (marker is 0xD8 or 0xD9 || marker is >= 0xD0 and <= 0xD7) continue;
                if (p + 2 > b.Length) break;
                int size = BinaryPrimitives.ReadUInt16BigEndian(b.AsSpan(p));
                if (size < 2 || p + size > b.Length) break;
                if (marker is 0xC0 or 0xC1 or 0xC2 && size >= 7) return (BinaryPrimitives.ReadUInt16BigEndian(b.AsSpan(p + 5)), BinaryPrimitives.ReadUInt16BigEndian(b.AsSpan(p + 3)));
                p += size;
            }
        }
        throw new InvalidDataException("InvalidImage");
    }
}

public static class AtomicFile
{
    public static void Write(string path, Action<string> write)
    {
        string full = Path.GetFullPath(path), tmp = full + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try { write(tmp); File.Move(tmp, full, true); }
        finally { if (File.Exists(tmp)) File.Delete(tmp); }
    }
}
public sealed class PreferencesStore(string path) : IPreferencesStore
{
    public Preferences Load()
    {
        if (!File.Exists(path)) return new();
        if (new FileInfo(path).Length > 16384) throw new InvalidDataException("InvalidSettings");
        var p = JsonSerializer.Deserialize<Preferences>(File.ReadAllText(path)) ?? throw new InvalidDataException("InvalidSettings");
        p.Validate(); return p;
    }
    public void Save(Preferences p) { p.Validate(); AtomicFile.Write(path, tmp => File.WriteAllText(tmp, JsonSerializer.Serialize(p, new JsonSerializerOptions { WriteIndented = true }))); }
}
