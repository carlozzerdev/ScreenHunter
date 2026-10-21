using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ScreenHunter.Domain;

namespace ScreenHunter.Infrastructure.Windows;

/// <summary>Per-output desktop capture: GDI for SDR, FP16 desktop duplication for HDR.</summary>
public static unsafe class DxgiCapture
{
    [DllImport("dxgi.dll")] static extern int CreateDXGIFactory1(in Guid riid, out nint factory);
    [DllImport("d3d11.dll")] static extern int D3D11CreateDevice(nint adapter, int driverType, nint software, uint flags, nint levels, uint count, uint sdk, out nint device, out int level, out nint context);
    static void** V(nint p) => *(void***)p;
    static void Release(ref nint p) { if (p != 0) { ((delegate* unmanaged[Stdcall]<nint, uint>)V(p)[2])(p); p = 0; } }
    static nint Query(nint p, Guid iid)
    {
        nint q = 0; Check(((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)V(p)[0])(p, &iid, &q)); return q;
    }
    static void Check(int hr) { if (hr < 0) Marshal.ThrowExceptionForHR(hr); }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)] struct OutputDesc
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string Name;
        public NativeDesktop.Rect Bounds; public int Attached, Rotation; public nint Monitor;
    }
    [StructLayout(LayoutKind.Sequential)] struct TextureDesc
    { public uint Width, Height, MipLevels, ArraySize; public int Format; public uint SampleCount, SampleQuality, Usage, BindFlags, CPUAccessFlags, MiscFlags; }
    [StructLayout(LayoutKind.Sequential)] struct Mapped { public nint Data; public uint RowPitch, DepthPitch; }
    static readonly Guid FactoryId = new("770aae78-f26f-4dba-a829-253c83d1b387");
    static readonly Guid Output5Id = new("80a07424-ab52-42eb-833c-0c42fd282d98");
    static readonly Guid Output6Id = new("068346e8-aaec-4b84-add7-137f513f77a1");
    static readonly Guid TextureId = new("6f15aaf2-d208-4e89-9ab4-489535d34f9c");
    public static Bitmap Capture(Rectangle area, bool cursor)
    {
        DocumentRules.CheckImage(area.Width, area.Height);
        var result = new Bitmap(area.Width, area.Height, PixelFormat.Format32bppArgb);
        nint factory = 0;
        try
        {
            Check(CreateDXGIFactory1(FactoryId, out factory)); int outputs = 0; bool separateCursor = false;
            using var g = Graphics.FromImage(result); g.Clear(Color.Black);
            byte* descBuffer = stackalloc byte[128];
            for (uint ai = 0; ; ai++)
            {
                nint adapter = 0;
                int hr = ((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)V(factory)[12])(factory, ai, &adapter);
                if (hr == unchecked((int)0x887A0002)) break; Check(hr);
                try
                {
                    for (uint oi = 0; ; oi++)
                    {
                        nint output = 0;
                        hr = ((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)V(adapter)[7])(adapter, oi, &output);
                        if (hr == unchecked((int)0x887A0002)) break; Check(hr);
                        try
                        {
                            // Native GetDesc writes an inline WCHAR buffer; marshal afterwards.
                            Check(((delegate* unmanaged[Stdcall]<nint, void*, int>)V(output)[7])(output, descBuffer));
                            var desc = Marshal.PtrToStructure<OutputDesc>((nint)descBuffer);
                            var bounds = desc.Bounds.Rectangle;
                            var intersection = Rectangle.Intersect(area, bounds);
                            if (desc.Attached == 0 || intersection.Width <= 0 || intersection.Height <= 0) continue;
                            bool hdr = IsHdr(output);
                            bool outputCursor = true;
                            using var shot = hdr ? ReadOutput(adapter, output, desc.Rotation, out outputCursor) : GdiCapture.Capture(bounds);
                            if (bounds.Contains(Cursor.Position)) separateCursor |= outputCursor;
                            if (shot.Width != bounds.Width || shot.Height != bounds.Height) throw new InvalidOperationException("CaptureUnavailable");
                            g.DrawImage(shot, new Rectangle(intersection.X - area.X, intersection.Y - area.Y, intersection.Width, intersection.Height), new Rectangle(intersection.X - bounds.X, intersection.Y - bounds.Y, intersection.Width, intersection.Height), GraphicsUnit.Pixel);
                            outputs++;
                        }
                        finally { Release(ref output); }
                    }
                }
                finally { Release(ref adapter); }
            }
            if (outputs == 0) throw new InvalidOperationException("CaptureUnavailable");
            // Duplication surfaces normally exclude the separate hardware cursor; add once if requested.
            g.Dispose();
            if (cursor && separateCursor) NativeDesktop.DrawCursor(result, area.Location);
            return result;
        }
        catch { result.Dispose(); throw; }
        finally { Release(ref factory); }
    }
    static bool IsHdr(nint output)
    {
        nint output6 = Query(output, Output6Id);
        try
        {
            byte* colorDesc = stackalloc byte[160];
            Check(((delegate* unmanaged[Stdcall]<nint, void*, int>)V(output6)[27])(output6, colorDesc));
            // DXGI_OUTPUT_DESC1 ColorSpace follows BitsPerColor at x64 offset 100.
            return *(int*)(colorDesc + 100) is 12 or 14;
        }
        finally { Release(ref output6); }
    }
    public static bool HasDesktopImage(long lastPresentTime, uint accumulatedFrames) => lastPresentTime != 0 && accumulatedFrames != 0;
    static Bitmap ReadOutput(nint adapter, nint output, int rotation, out bool separateCursor)
    {
        separateCursor = false;
        nint device = 0, context = 0, output5 = 0, duplication = 0, resource = 0, texture = 0, staging = 0;
        bool acquired = false, mapped = false;
        try
        {
            Check(D3D11CreateDevice(adapter, 0, 0, 0x20, 0, 0, 7, out device, out _, out context));
            output5 = Query(output, Output5Id);
            int* formats = stackalloc int[] {10, 87, 28};
            Check(((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, int*, nint*, int>)V(output5)[26])(output5, device, 0, 3, formats, &duplication));
            byte* info = stackalloc byte[128];
            var deadline = Environment.TickCount64 + 2500;
            while (true)
            {
                int hr = ((delegate* unmanaged[Stdcall]<nint, uint, void*, nint*, int>)V(duplication)[8])(duplication, 250, info, &resource);
                if (hr == unchecked((int)0x887A0027)) // DXGI_ERROR_WAIT_TIMEOUT
                { if (Environment.TickCount64 >= deadline) throw new InvalidOperationException("CaptureUnavailable"); continue; }
                Check(hr); acquired = true;
                if (*(long*)(info + 8) != 0) separateCursor = *(int*)(info + 36) != 0;
                if (HasDesktopImage(*(long*)info, *(uint*)(info + 16))) break;
                Release(ref resource);
                Check(((delegate* unmanaged[Stdcall]<nint, int>)V(duplication)[14])(duplication)); acquired = false;
                if (Environment.TickCount64 >= deadline) throw new InvalidOperationException("CaptureUnavailable");
            }
            texture = Query(resource, TextureId); TextureDesc d;
            ((delegate* unmanaged[Stdcall]<nint, TextureDesc*, void>)V(texture)[10])(texture, &d);
            if (d.Format != 10) throw new InvalidOperationException("CaptureFormat");
            DocumentRules.CheckImage((int)d.Width, (int)d.Height);
            var sd = d; sd.Usage = 3; sd.BindFlags = 0; sd.CPUAccessFlags = 0x20000; sd.MiscFlags = 0;
            Check(((delegate* unmanaged[Stdcall]<nint, TextureDesc*, void*, nint*, int>)V(device)[5])(device, &sd, null, &staging));
            ((delegate* unmanaged[Stdcall]<nint, nint, nint, void>)V(context)[47])(context, staging, texture);
            Mapped map;
            Check(((delegate* unmanaged[Stdcall]<nint, nint, uint, int, uint, Mapped*, int>)V(context)[14])(context, staging, 0, 1, 0, &map)); mapped = true;
            var bmp = new Bitmap((int)d.Width, (int)d.Height, PixelFormat.Format32bppArgb);
            try
            {
                var bits = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                try
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        byte* src = (byte*)map.Data + y * map.RowPitch, dst = (byte*)bits.Scan0 + y * bits.Stride;
                        for (int x = 0; x < bmp.Width; x++, dst += 4)
                        {
                            if (d.Format == 10)
                            {
                                var h = (Half*)(src + x * 8);
                                ToneMap((float)h[0], (float)h[1], (float)h[2], out dst[2], out dst[1], out dst[0]);
                            }
                            else if (d.Format is 87 or 28)
                            {
                                byte* s = src + x * 4; dst[0] = s[d.Format == 87 ? 0 : 2]; dst[1] = s[1]; dst[2] = s[d.Format == 87 ? 2 : 0];
                            }
                            else throw new InvalidOperationException("CaptureFormat");
                            dst[3] = 255;
                        }
                    }
                }
                finally { bmp.UnlockBits(bits); }
                if (rotation == 2) bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                if (rotation == 3) bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                if (rotation == 4) bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                return bmp;
            }
            catch { bmp.Dispose(); throw; }
        }
        finally
        {
            if (mapped) ((delegate* unmanaged[Stdcall]<nint, nint, uint, void>)V(context)[15])(context, staging, 0);
            Release(ref staging); Release(ref texture); Release(ref resource);
            if (acquired) ((delegate* unmanaged[Stdcall]<nint, int>)V(duplication)[14])(duplication);
            Release(ref duplication); Release(ref output5); Release(ref context); Release(ref device);
        }
    }
    public static void ToneMap(float r, float g, float b, out byte red, out byte green, out byte blue)
    {
        // scRGB: 1 = 80 nits. SDR white is mapped to 203 nits; a luminance shoulder compresses highlights.
        double rr = Math.Max(0, r / 2.5375), gg = Math.Max(0, g / 2.5375), bb = Math.Max(0, b / 2.5375);
        double l = .2126 * rr + .7152 * gg + .0722 * bb;
        double mapped = l <= .75 ? l : .75 + .25 * (1 - Math.Exp(-(l - .75) / .25));
        double scale = l > 0 ? mapped / l : 0;
        red = Srgb(rr * scale); green = Srgb(gg * scale); blue = Srgb(bb * scale);
    }
    static byte Srgb(double x) { x = Math.Clamp(double.IsFinite(x) ? x : 0, 0, 1); return (byte)Math.Round(255 * (x <= .0031308 ? 12.92 * x : 1.055 * Math.Pow(x, 1 / 2.4) - .055)); }
}
