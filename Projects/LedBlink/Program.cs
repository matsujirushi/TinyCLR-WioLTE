using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace LedBlink
{
    class Program
    {
        static void HueToRGB(int hue, byte value, out byte r, out byte g, out byte b)
        {
            if (hue < 60)
            {
                r = value;
                g = (byte)(hue * value / 60);
                b = 0;
            }
            else if (hue < 120)
            {
                r = (byte)((120 - hue) * value / 60);
                g = value;
                b = 0;
            }
            else if (hue < 180)
            {
                r = 0;
                g = value;
                b = (byte)((hue - 120) * value / 60);
            }
            else if (hue < 240)
            {
                r = 0;
                g = (byte)((240 - hue) * value / 60);
                b = value;
            }
            else if (hue < 300)
            {
                r = (byte)((hue - 240) * value / 60);
                g = 0;
                b = value;
            }
            else
            {
                r = value;
                g = 0;
                b = (byte)((360 - hue) * value / 60);
            }
        }

        static void Main()
        {
            var interop = Properties.Resources.GetBytes(Properties.Resources.BinaryResources.LedBlink);
            Marshal.Copy(interop, 0, new IntPtr(0x2001bc00), interop.Length);
            Interop.Add(new IntPtr(0x2001bd28));

            var wio = new WioLTE();
            wio.Init();

            int hue = 0;
            while (true)
            {
                byte r;
                byte g;
                byte b;

                HueToRGB(hue, 10, out r, out g, out b);
                wio.LedSetRGB(r, g, b);

                hue += 10;
                if (hue >= 360) hue = 0;

                Thread.Sleep(50);
            }
        }
    }
}
