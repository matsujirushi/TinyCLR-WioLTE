using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.SerialCommunication;
using GHIElectronics.TinyCLR.Pins;

namespace Seeed.TinyCLR.WioLTE
{
    public class WioLTE : IDisposable
    {
        private WioLTENative _Native;

        private GpioPin _ModulePwrPin;
        private GpioPin _AntPwrPin;
        private GpioPin _EnableVccbPin;
        private GpioPin _RgbLedPwrPin;
        private GpioPin _SdPowrPin;
        private GpioPin _PwrKeyPin;
        private GpioPin _ResetModulePin;
        private GpioPin _StatusPin;
        private GpioPin _DtrPin;
        private GpioPin _WakeupInPin;
        private GpioPin _WDisablePin;

        private AtSerial _Module;

        private static GpioPin NewGpioInputPin(GpioController controller, int pinNumber)
        {
            var pin = controller.OpenPin(pinNumber);

            pin.SetDriveMode(GpioPinDriveMode.Input);

            return pin;
        }

        private static GpioPin NewGpioOutputPin(GpioController controller, int pinNumber, GpioPinValue defaultValue)
        {
            var pin = controller.OpenPin(pinNumber);

            pin.SetDriveMode(GpioPinDriveMode.Output);
            pin.Write(defaultValue);

            return pin;
        }

        public WioLTE()
        {
            #region Find original interop code.
            var api = Api.Find("Seeed.TinyCLR.WioLTE.Interop", ApiType.Custom);
            Interop.Add(api.Implementation[0]);
            _Native = new WioLTENative();
            #endregion

            var controller = GpioController.GetDefault();

            // Power supply
            _ModulePwrPin = NewGpioOutputPin(controller, 21, GpioPinValue.Low);
            _AntPwrPin = NewGpioOutputPin(controller, 28, GpioPinValue.Low);
            _EnableVccbPin = NewGpioOutputPin(controller, 26, GpioPinValue.Low);
            _RgbLedPwrPin = NewGpioOutputPin(controller, 8, GpioPinValue.High);
            _SdPowrPin = NewGpioOutputPin(controller, 15, GpioPinValue.Low);

            // Turn on/off Pins
            _PwrKeyPin = NewGpioOutputPin(controller, 36, GpioPinValue.Low);
            _ResetModulePin = NewGpioOutputPin(controller, 35, GpioPinValue.High);

            // Status Indication Pins
            _StatusPin = NewGpioInputPin(controller, 31);

            // UART Interface
            _DtrPin = NewGpioOutputPin(controller, 1, GpioPinValue.Low);

            // GPIO Pins
            _WakeupInPin = NewGpioOutputPin(controller, 32, GpioPinValue.Low);
            _WDisablePin = NewGpioOutputPin(controller, 34, GpioPinValue.High);
            //GpioPin _ApReadyPin = NewGpioOutputPin(controller, 33, GpioPinValue.Low);   // NOT use

            var serial = SerialDevice.FromId(STM32F4.UartPort.Usart2);
            serial.BaudRate = 115200;
            serial.ReadTimeout = TimeSpan.Zero;
            _Module = new AtSerial(serial);
        }

        public void Dispose()
        {
            _ModulePwrPin.Dispose();
            _AntPwrPin.Dispose();
            _EnableVccbPin.Dispose();
            _RgbLedPwrPin.Dispose();
            _SdPowrPin.Dispose();
            _PwrKeyPin.Dispose();
            _ResetModulePin.Dispose();
            _StatusPin.Dispose();
            _DtrPin.Dispose();
            _WakeupInPin.Dispose();
            _WDisablePin.Dispose();
        }

        private bool Reset()
        {
            _ResetModulePin.Write(GpioPinValue.Low);
            Thread.Sleep(200);
            _ResetModulePin.Write(GpioPinValue.High);
            Thread.Sleep(300);

            //Seeed::Stopwatch sw;
            //sw.Restart();
            //while (_Module.WaitForResponse("^RDY$", 100, 10) == NULL)
            //{
            //    DEBUG_PRINT(".");
            //    if (sw.ElapsedMilliseconds() >= 10000) return false;
            //}
            //DEBUG_PRINTLN("");

            return true;
        }

        private bool TurnOn()
        {
            Thread.Sleep(100);
            _PwrKeyPin.Write(GpioPinValue.High);
            Thread.Sleep(200);
            _PwrKeyPin.Write(GpioPinValue.Low);

            //Seeed::Stopwatch sw;
            //sw.Restart();
            //while (IsBusy())
            //{
            //    DEBUG_PRINT(".");
            //    if (sw.ElapsedMilliseconds() >= 5000) return false;
            //    wait_ms(100);
            //}
            //DEBUG_PRINTLN("");

            var sw = new Stopwatch();
            sw.Restart();
            while (_Module.WaitForResponse("^RDY$", 100, 10) == null)
            {
                if (sw.ElapsedMilliseconds >= 10000) return false;
            }

            return true;
        }

        public void Init()
        {
            _Native.Init();
        }

        public void LedSetRGB(byte r, byte g, byte b)
        {
            _Native.LedSetRGB(r, g, b);
        }

        public void PowerSupplyLTE(bool on)
        {
            _ModulePwrPin.Write(on ? GpioPinValue.High : GpioPinValue.Low);
        }

        public void PowerSupplyGrove(bool on)
        {
            _EnableVccbPin.Write(on ? GpioPinValue.High : GpioPinValue.Low);
        }

        public bool IsBusy()
        {
            return _StatusPin.Read() == GpioPinValue.High ? true : false;
        }

        public void TurnOnOrReset()
        {
            if (!IsBusy())
            {
                if (!Reset()) throw new ApplicationException();
            }
            else
            {
                if (!TurnOn()) throw new ApplicationException();
            }

            var sw = new Stopwatch();
            sw.Restart();
            while (_Module.WriteCommandAndWaitForResponse("AT", "^OK$", 500, 10) == null)
            {
                if (sw.ElapsedMilliseconds >= 10000) throw new ApplicationException();
            }

            if (_Module.WriteCommandAndWaitForResponse("ATE0", "^OK$", 500, 10) == null) throw new ApplicationException();
            if (_Module.WriteCommandAndWaitForResponse("AT+QURCCFG=\"urcport\",\"uart1\"", "^OK$", 500, 10) == null) throw new ApplicationException();

            // TODO
        }
    }
}
