using System.Buffers;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace RemoteCamReceiver;

public class Frame
{
    public int Id;
    public int Width, Height;
    public byte[] Data;

    public Frame(int id, byte[] data)
    {
        Id = id;

        var bitmap = new Bitmap(new MemoryStream(data));
        Width = bitmap.Width;
        Height = bitmap.Height;

        var bitmapData = bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

        Data = new byte[Width * Height * 3];

        Marshal.Copy(bitmapData.Scan0, Data, 0, Width * Height * 3);

        bitmap.UnlockBits(bitmapData);
        bitmap.Dispose();
    }
}