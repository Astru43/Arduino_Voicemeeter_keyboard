using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace Vm_Serial {
    public class SerialService {
        private bool _portFound = false;
        SerialPort? port;

        public SerialService() {
            GetPort();
        }

        internal void GetPort() {
            while (!_portFound) {
                string[] names = SerialPort.GetPortNames();
                foreach (string name in names) {
                    try {
                        port = new SerialPort(name, 250000);
                        if (_portFound = PortFound()) break;
                    } catch (IOException) {
                        try {
                            port = new SerialPort(name, 9600);
                            if (_portFound = PortFound()) break;
                        } catch (IOException) { }
                    }
                }
            }
        }

        internal void Discard() {
            port?.DiscardInBuffer();
        }

        internal bool HasPort() {
            return _portFound;
        }

        private bool PortFound() {
            Byte[] buffer = new byte[4];
            buffer[0] = Convert.ToByte(0x81);

            port?.Open();
            port?.Write(buffer, 0, 1);
            Thread.Sleep(100);
            if (port?.BytesToRead > 0) port.Read(buffer, 2, 1);

            if (buffer[2] == 0x82) return true;
            else {
                port?.Close();
                return false;
            }
        }

        public byte[] ReadSerial() {
            byte[] buffer = new byte[2];
            port?.Read(buffer, 0, 1);
            return buffer;
        }

        public Task WaitRead(CancellationToken token) {
            return Task.Run(() => {
                while (port?.BytesToRead <= 0) Task.Delay(1);
            }, token);

        }
    }
}
