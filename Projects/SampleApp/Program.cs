using GHIElectronics.TinyCLR.Devices.SerialCommunication;
using Seeed.TinyCLR.WioLTE;
using System.Diagnostics;
using System.Threading;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.Storage.Streams;
using System;
using System.Text;

namespace SampleApp
{
    class Program
    {
        static void Main()
        {
            var serial = SerialDevice.FromId(STM32F4.UartPort.Usart2);
            serial.BaudRate = 115200;
            serial.ReadTimeout = TimeSpan.Zero;
            var atSerial = new AtSerial(serial);

            var Wio = new WioLTE();

            Thread.Sleep(200);

            Debug.WriteLine("");
            Debug.WriteLine("--- START ---------------------------------------------------");

            Debug.WriteLine("### I/O Initialize.");
            Wio.Init();

            Debug.WriteLine("### Power supply ON.");
            Wio.PowerSupplyLTE(true);
            Thread.Sleep(500);

            #region TurnOnOrReset

            #region TurnOn

            Debug.WriteLine("### Turn on or reset.");
            Wio.TurnOnOrReset();

            var sw = new Stopwatch();
            sw.Restart();
            while (atSerial.WaitForResponse("RDY", 100, 10) == null)
            {
                if (sw.ElapsedMilliseconds >= 10000) throw new ApplicationException();
            }

            #endregion

            sw.Restart();
            while (atSerial.WriteCommandAndWaitForResponse("AT", "OK", 500, 10) == null)
            {
                if (sw.ElapsedMilliseconds >= 10000) throw new ApplicationException();
            }

            if (atSerial.WriteCommandAndWaitForResponse("ATE0", "OK", 500, 10) == null) throw new ApplicationException();
            if (atSerial.WriteCommandAndWaitForResponse("AT+QURCCFG=\"urcport\",\"uart1\"", "OK", 500, 10) == null) throw new ApplicationException();

            // TODO

            #endregion

            Debug.WriteLine("### Finish.");
        }
    }
}
