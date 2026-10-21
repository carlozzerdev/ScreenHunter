using System.Drawing.Imaging;
using ScreenHunter.Application;

namespace ScreenHunter.Infrastructure.Windows;

public sealed class ImageOutput : IImageOutput
{
    public void Copy(byte[] png)
    {
        using var image = DocumentStore.Decode(png);
        var data = new DataObject(); data.SetData(DataFormats.Bitmap, true, image);
        using var pngStream = new MemoryStream(png); data.SetData("PNG", false, pngStream);
        Clipboard.SetDataObject(data, true, 3, 100);
    }
    public void Export(byte[] png, string path, bool jpeg, int quality)
    {
        AtomicFile.Write(path, tmp =>
        {
            if (!jpeg) { File.WriteAllBytes(tmp, png); return; }
            using var source = DocumentStore.Decode(png); using var opaque = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(opaque)) { g.Clear(Color.White); g.DrawImageUnscaled(source, 0, 0); }
            var codec = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
            using var parameters = new EncoderParameters(1); parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)Math.Clamp(quality, 1, 100)); opaque.Save(tmp, codec, parameters);
        });
    }
}
