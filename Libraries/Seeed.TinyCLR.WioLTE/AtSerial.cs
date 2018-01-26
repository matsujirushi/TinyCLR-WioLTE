using GHIElectronics.TinyCLR.Devices.SerialCommunication;
using GHIElectronics.TinyCLR.Storage.Streams;
using System;
using System.Diagnostics;
using System.Text;

namespace Seeed.TinyCLR.WioLTE
{
    internal class AtSerial
    {
        private const byte CHAR_CR = 0x0d;
        private const byte CHAR_LF = 0x0a;

        private DataWriter _SerialWriter;
        private DataReader _SerialReader;
        private bool _ReadedByteValid;
        private byte _ReadedByte;

        #region Serial APIs

        private bool SerialAvailable()
        {
            if (_ReadedByteValid) return true;

            if (_SerialReader.Load(1) > 0)
            {
                _ReadedByte = _SerialReader.ReadByte();
                _ReadedByteValid = true;
            }

            return false;
        }

        private byte SerialRead()
        {
            if (!_ReadedByteValid) throw new ApplicationException();

            var b = _ReadedByte;
            _ReadedByteValid = false;

            return b;
        }

        private void SerialWrite(byte data)
        {
            _SerialWriter.WriteByte(data);
            _SerialWriter.Store();
        }

        private void SerialWrite(byte[] data)
        {
            _SerialWriter.WriteBytes(data);
            _SerialWriter.Store();
        }

        #endregion

        private bool WaitForAvailable(Stopwatch sw, int timeout)
        {
            while (!SerialAvailable())
            {
                if (sw.ElapsedMilliseconds >= timeout) return false;
            }
            return true;
        }

        private string ReadResponse(int timeout)
        {
            var response = new StringBuilder();

            var sw = new Stopwatch();
            while (true)
            {
                // Wait for available.
                sw.Restart();
                if (!WaitForAvailable(sw, timeout)) return null;

                // Read byte.
                var b = SerialRead();
                response.Append(Convert.ToChar(b));

                // Is received delimiter?
                if (response.Length >= 2 && response[response.Length - 2] == CHAR_CR && response[response.Length - 1] == CHAR_LF)
                {
                    response.Remove(response.Length - 2, 2);
                    return response.ToString();
                }
            }
        }

        public AtSerial(SerialDevice serial)
        {
            serial.BaudRate = 115200;
            serial.ReadTimeout = TimeSpan.Zero;
            _SerialWriter = new DataWriter(serial.OutputStream);
            _SerialReader = new DataReader(serial.InputStream);
            _ReadedByteValid = false;
        }

        public void Write(byte[] data)
        {
            SerialWrite(data);
        }

        public void WriteCommand(string command)
        {
            Debug.WriteLine($"<- {command}");

            SerialWrite(Encoding.UTF8.GetBytes(command));
            SerialWrite(CHAR_CR);
        }

        public string WaitForResponse(string responsePattern, int firstTimeout, int nextTimeout)
        {
            var sw = new Stopwatch();
            sw.Restart();
            while (true)
            {
                if (!WaitForAvailable(sw, firstTimeout)) return null;

                var response = ReadResponse(nextTimeout);

                // responsePattern?
                if (responsePattern != null)
                {
                    if (WioLTENative.slre_match(responsePattern, response) >= 0)
                    {
                        Debug.WriteLine($"-> {response}");
                        return response;
                    }
                }

                Debug.WriteLine($"-> ({response})");
            }
        }

        public string WriteCommandAndWaitForResponse(string command, string responsePattern, int firstTimeout, int nextTimeout)
        {
            WriteCommand(command);
            return WaitForResponse(responsePattern, firstTimeout, nextTimeout);
        }

    }
}
