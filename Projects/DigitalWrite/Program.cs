using GHIElectronics.TinyCLR.Devices.Gpio;
using System.Threading;

namespace DigitalWrite
{
    class Program
    {
        static void Main()
        {
            GpioPin pin = GpioController.GetDefault().OpenPin(38);
            pin.SetDriveMode(GpioPinDriveMode.Output);
            while (true)
            {
                pin.Write(GpioPinValue.High);
                Thread.Sleep(1);
                pin.Write(GpioPinValue.Low);
                Thread.Sleep(1);
            }
        }
    }
}
