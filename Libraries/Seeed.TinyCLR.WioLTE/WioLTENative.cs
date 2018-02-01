using System.Runtime.CompilerServices;

namespace Seeed.TinyCLR.WioLTE
{
    internal class WioLTENative
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int slre_match(string regexp, string buf);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string slre_match2(string regexp, string buf);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void Init();
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void LedSetRGB(byte r, byte g, byte b);
    }
}
