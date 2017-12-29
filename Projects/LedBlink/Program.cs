using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace LedBlink
{
    class Program
    {
        static void Main()
        {
            var interop = Properties.Resources.GetBytes(Properties.Resources.BinaryResources.LedBlink);
            Marshal.Copy(interop, 0, new IntPtr(0x2001bc00), interop.Length);
            Interop.Add(new IntPtr(0x2001bd04));
            var wio = new WioLTE();
            while (true)
            {
                // Red
                wio.LedSetRGB(50, 0, 0);
                Thread.Sleep(200);
                // Green
                wio.LedSetRGB(0, 50, 0);
                Thread.Sleep(200);
                // Blue
                wio.LedSetRGB(0, 0, 50);
                Thread.Sleep(200);
            }
        }
    }
}
