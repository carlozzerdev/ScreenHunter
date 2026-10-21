using System.ComponentModel;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ScreenHunter.Domain;

namespace ScreenHunter.Infrastructure.Windows;

/// <summary>One-shot SDR desktop capture. RGB avoids treating BitBlt's unused alpha as transparency.</summary>
public static class GdiCapture
{
    [DllImport("user32.dll", SetLastError = true)] static extern nint GetDC(nint window);
    [DllImport("user32.dll")] static extern int ReleaseDC(nint window, nint dc);
    [DllImport("gdi32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool BitBlt(nint destination, int x, int y, int width, int height, nint source, int sourceX, int sourceY, uint operation);

    public static Bitmap Capture(Rectangle area)
    {
        DocumentRules.CheckImage(area.Width, area.Height);
        nint source = GetDC(0);
        if (source == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
        var bitmap = new Bitmap(area.Width, area.Height, PixelFormat.Format24bppRgb);
        try
        {
            using var g = Graphics.FromImage(bitmap); nint destination = g.GetHdc();
            try
            {
                // Include ordinary layered desktop windows; respect Windows capture exclusions.
                if (!BitBlt(destination, 0, 0, area.Width, area.Height, source, area.X, area.Y, 0x40CC0020))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            finally { g.ReleaseHdc(destination); }
            return bitmap;
        }
        catch { bitmap.Dispose(); throw; }
        finally { ReleaseDC(0, source); }
    }
}
