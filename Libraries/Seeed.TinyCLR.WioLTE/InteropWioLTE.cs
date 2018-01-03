﻿using System.Runtime.CompilerServices;

namespace Seeed.TinyCLR.WioLTE
{
    internal class InteropWioLTE
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void Init();
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void LedSetRGB(byte r, byte g, byte b);
    }
}
