﻿using Seeed.TinyCLR.WioLTE;
using System.Diagnostics;
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
            Thread.Sleep(500);

            Debug.WriteLine("### Turn on or reset.");
            Wio.TurnOnOrReset();

            Wio.Activate("soracom.io", "sora", "sora");

            Debug.WriteLine("### Finish.");
        }
    }
}
