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

            SerialDevice ser = SerialDevice.FromId(STM32F4.UartPort.Usart2);
            ser.BaudRate = 115200;
            ser.ReadTimeout = TimeSpan.Zero;
            var serReader = new DataReader(ser.InputStream);
            var serWriter = new DataWriter(ser.OutputStream);

            while (true)
            {
                var i = serReader.Load(1);
                if (i > 0)
                {
                    byte b = serReader.ReadByte();
                    Debug.WriteLine("Recieved: " + b + ":" + new string(Encoding.UTF8.GetChars(new byte[] { b })));
                }

                Thread.Sleep(10);
            }
        }
    }
}
