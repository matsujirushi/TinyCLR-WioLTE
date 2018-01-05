using Seeed.TinyCLR.WioLTE;
using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace SampleApp
{
    class Program
    {
        static void Main()
        {
            var Wio = new WioLTE();

            Thread.Sleep(200);

            Debug.WriteLine("");
            Debug.WriteLine("--- START ---------------------------------------------------");

            Debug.WriteLine("### I/O Initialize.");
            Wio.Init();

            Debug.WriteLine("### Power supply ON.");
            Wio.PowerSupplyLTE(true);
            Thread.Sleep(5000);

            Debug.WriteLine("### Turn on or reset.");
            Wio.TurnOnOrReset();
        }
    }
}
