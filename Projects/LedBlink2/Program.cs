using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace LedBlink2
{
    internal class WioLTEProvider
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void Init();
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void LedSetRGB(byte r, byte g, byte b);
    }

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
            var api = Api.Find("Seeed.TinyCLR.NativeApis.WioLTE.WioLTEProvider", ApiType.Custom);
            Interop.Add(api.Implementation[0]);

            var wio = new WioLTEProvider();
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
