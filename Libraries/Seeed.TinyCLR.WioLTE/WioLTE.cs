#define WIOLTE_DEBUG

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
        public enum SocketType
        {
            TCP,
            UDP,
        }

        private const int POLLING_INTERVAL = 100;

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

            var sw = new Stopwatch();
            sw.Restart();
            while (_Module.WaitForResponse("^(RDY)$", 100, 10) == null)
            {
                if (sw.ElapsedMilliseconds >= 10000) return false;
            }

            return true;
        }

        private bool TurnOn()
        {
            Thread.Sleep(100);
            _PwrKeyPin.Write(GpioPinValue.High);
            Thread.Sleep(200);
            _PwrKeyPin.Write(GpioPinValue.Low);

            var sw = new Stopwatch();
            sw.Restart();
            while (IsBusy())
            {
                if (sw.ElapsedMilliseconds >= 5000) return false;
                Thread.Sleep(100);
            }

            sw.Restart();
            while (_Module.WaitForResponse("^(RDY)$", 100, 10) == null)
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
            while (_Module.WriteCommandAndWaitForResponse("AT", "^(OK)$", 500, 10) == null)
            {
                if (sw.ElapsedMilliseconds >= 10000) throw new ApplicationException();
            }

            if (_Module.WriteCommandAndWaitForResponse("ATE0", "^(OK)$", 500, 10) == null) throw new ApplicationException();
            if (_Module.WriteCommandAndWaitForResponse("AT+QURCCFG=\"urcport\",\"uart1\"", "^(OK)$", 500, 10) == null) throw new ApplicationException();
            if (_Module.WriteCommandAndWaitForResponse("AT+QSCLK=1", "^(OK|ERROR)$", 500, 10) == null) throw new ApplicationException();

            sw.Restart();
            while (true)
            {
                var response = _Module.WriteCommandAndWaitForResponse("AT+CPIN?", "^(OK|.CME ERROR: .*)$", 5000, 10);
                if (response == null) throw new ApplicationException();
                if (response == "OK") break;
                if (sw.ElapsedMilliseconds >= 10000) throw new ApplicationException();
                Thread.Sleep(POLLING_INTERVAL);
            }
        }

        public void Activate(string accessPointName, string userName, string password)
        {
            string response;
            string[] parser;
            int resultCode;
            int status;


            var sw = new Stopwatch();
            sw.Restart();
            while (true)
            {
                response = _Module.WriteCommandAndWaitForResponse("AT+CGREG?", "^\\+CGREG: ", 500, 10);
                if (response == null) throw new ApplicationException();
                parser = response.Substring(8).Split(',');
                if (parser.Length < 2) throw new ApplicationException();
                resultCode = int.Parse(parser[0]);
                status = int.Parse(parser[1]);
                if (_Module.WaitForResponse("^(OK)$", 500, 10) == null) throw new ApplicationException();
                if (status == 0) throw new ApplicationException();
                if (status == 1 || status == 5) break;

                response = _Module.WriteCommandAndWaitForResponse("AT+CEREG?", "^\\+CEREG: ", 500, 10);
                if (response == null) throw new ApplicationException();
                parser = response.Substring(8).Split(',');
                if (parser.Length < 2) throw new ApplicationException();
                resultCode = int.Parse(parser[0]);
                status = int.Parse(parser[1]);
                if (_Module.WaitForResponse("^(OK)$", 500, 10) == null) throw new ApplicationException();
                if (status == 0) throw new ApplicationException();
                if (status == 1 || status == 5) break;

                if (sw.ElapsedMilliseconds >= 120000) throw new ApplicationException();
            }

            // for debug.
#if WIOLTE_DEBUG
            _Module.WriteCommandAndWaitForResponse("AT+CREG?", "^(OK)$", 500, 10);
            _Module.WriteCommandAndWaitForResponse("AT+CGREG?", "^(OK)$", 500, 10);
            _Module.WriteCommandAndWaitForResponse("AT+CEREG?", "^(OK)$", 500, 10);
#endif // WIOLTE_DEBUG

            if (_Module.WriteCommandAndWaitForResponse($"AT+QICSGP=1,1,\"{accessPointName}\",\"{userName}\",\"{password}\",1", "^(OK)$", 500, 10) == null) throw new ApplicationException();

            sw.Restart();
            while (true)
            {
                response = _Module.WriteCommandAndWaitForResponse("AT+QIACT=1", "^(OK|ERROR)$", 150000, 10);
                if (response == null) throw new ApplicationException();
                if (response == "OK") break;
                if (_Module.WriteCommandAndWaitForResponse("AT+QIGETERROR", "^(OK)$", 500, 10) == null) throw new ApplicationException();
                if (sw.ElapsedMilliseconds >= 150000) throw new ApplicationException();
                Thread.Sleep(POLLING_INTERVAL);
            }

            // for debug.
#if WIOLTE_DEBUG
            if (_Module.WriteCommandAndWaitForResponse("AT+QIACT?", "^(OK)$", 150000, 10) == null) throw new ApplicationException();
#endif // WIOLTE_DEBUG
        }

        public int SocketOpen(string host, int port, SocketType type)
        {
            if (host == null || host.Length <= 0) throw new ApplicationException();
            if (port < 0 || 65535 < port) throw new ApplicationException();

            string typeStr;
            switch (type)
            {
                case SocketType.TCP:
                    typeStr = "TCP";
                    break;
                case SocketType.UDP:
                    typeStr = "UDP";
                    break;
                default:
                    throw new ApplicationException();
            }

	        int connectId = 0;  // TODO

            if (_Module.WriteCommandAndWaitForResponse($"AT+QIOPEN=1,{connectId},\"{typeStr}\",\"{host}\",{port}", "^(OK)$", 150000, 10) == null) throw new ApplicationException();
            if (_Module.WaitForResponse($"^\\+QIOPEN: {connectId},0$", 150000, 10) == null) throw new ApplicationException();

	        return connectId;
        }

        public void SocketSend(int connectId, byte[] data)
        {
            // TODO
            if (data.Length > 1460) throw new ApplicationException();

            _Module.WriteCommand($"AT+QISEND={connectId},{data.Length}");
            if (_Module.WaitForResponse(new AtSerial.ResponseCompare[] { new AtSerial.ResponseCompare(AtSerial.ResponseCompareType.RegExWithoutDelim, "^> $"), }, 500, 10) == null) throw new ApplicationException();
            _Module.Write(data);
            if (_Module.WaitForResponse("^(SEND OK)$", 5000, 10) == null) throw new ApplicationException();
        }

        public void SocketClose(int connectId)
        {
            // TODO

            if (_Module.WriteCommandAndWaitForResponse($"AT+QICLOSE={connectId}", "^(OK)$", 10000, 100) == null) throw new ApplicationException();
        }

    }
}
