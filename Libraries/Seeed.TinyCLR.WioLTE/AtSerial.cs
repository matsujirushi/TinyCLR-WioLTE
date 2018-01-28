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

        public enum ResponseCompareType
        {
            RegEx,
            RegExWithoutDelim,
        }

        public struct ResponseCompare
        {
            public ResponseCompareType Type;
            public string Pattern;

            public ResponseCompare(ResponseCompareType type, string pattern)
            {
                Type = type;
                Pattern = pattern;
            }

        }

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

        private string ReadResponse(ResponseCompare[] responseCompare, int timeout)
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

                // Is match responseCompare?
                foreach (var compare in responseCompare)
                {
                    switch (compare.Type)
                    {
                        //case ResponseCompareType.RegEx:
                        case ResponseCompareType.RegExWithoutDelim:
                            if (WioLTENative.slre_match(compare.Pattern, response.ToString()) >= 0)
                            {
                                return response.ToString();
                            }
                            break;
                    }
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

        public string WaitForResponse(ResponseCompare[] responseCompare, int firstTimeout, int nextTimeout)
        {
            var sw = new Stopwatch();
            sw.Restart();
            while (true)
            {
                if (!WaitForAvailable(sw, firstTimeout)) return null;

                var response = ReadResponse(responseCompare, nextTimeout);

                // Is match responseCompare?
                foreach (var compare in responseCompare)
                {
                    switch (compare.Type)
                    {
                        case ResponseCompareType.RegEx:
                        case ResponseCompareType.RegExWithoutDelim:
                            if (WioLTENative.slre_match(compare.Pattern, response) >= 0)
                            {
                                Debug.WriteLine($"-> {response}");
                                return response;
                            }
                            break;
                    }
                }

                Debug.WriteLine($"-> ({response})");
            }
        }

        public string WaitForResponse(string regEx, int firstTimeout, int nextTimeout)
        {
            return WaitForResponse(new ResponseCompare[] { new ResponseCompare(ResponseCompareType.RegEx, regEx), }, firstTimeout, nextTimeout);
        }

        public string WriteCommandAndWaitForResponse(string command, string responsePattern, int firstTimeout, int nextTimeout)
        {
            WriteCommand(command);
            return WaitForResponse(responsePattern, firstTimeout, nextTimeout);
        }

    }
}
