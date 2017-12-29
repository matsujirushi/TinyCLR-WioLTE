using System.Runtime.CompilerServices;

namespace LedBlink
{
    class WioLTE
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void LedSetRGB(byte red, byte green, byte blue);
    }
}
