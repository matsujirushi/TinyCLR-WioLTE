using System.Runtime.InteropServices;

namespace Seeed.TinyCLR.WioLTE
{
    public class WioLTE
    {
        private InteropWioLTE _Interop;

        public WioLTE()
        {
            var api = Api.Find("Seeed.TinyCLR.NativeApis.WioLTE.WioLTE", ApiType.Custom);
            Interop.Add(api.Implementation[0]);

            _Interop = new InteropWioLTE();
        }

        public void Init()
        {
            _Interop.Init();
        }

        public void LedSetRGB(byte r, byte g, byte b)
        {
            _Interop.LedSetRGB(r, g, b);
        }

    }
}
