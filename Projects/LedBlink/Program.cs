using System;
using System.Runtime.InteropServices;

namespace LedBlink
{
    class Program
    {
        static void Main()
        {
            var interop = Properties.Resources.GetBytes(Properties.Resources.BinaryResources.LedBlink);
            Marshal.Copy(interop, 0, new IntPtr(0x2001bc00), interop.Length);
            //Interop.Add(new IntPtr(0x2001bc28));
            Interop.Add(new IntPtr(0x2001bc34));
        }
    }
}
