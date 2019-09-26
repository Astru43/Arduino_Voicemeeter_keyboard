using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Save {
    class save {
        class Program {

            Serial serial;
            Thread thread;

            static void Main(string[] args) {
                Program program = new Program();
                program.StartSerial();
            }

            private void StartSerial() {
                serial = new Serial();
                thread = new Thread(new ThreadStart(serial.ComMain)) { IsBackground = true };
                thread.Start();
                Thread.Sleep(200);
                Console.WriteLine(serial.portFound);
                thread.Join();
            }

        }
        class Serial {

            public SerialPort port;
            public bool portFound = false;

            public bool PortFound() {
                Byte[] buffer = new byte[4];
                buffer[0] = Convert.ToByte(255);

                port.Open();
                port.Write(buffer, 0, 1);
                Thread.Sleep(100);
                if (port.BytesToRead > 0) port.Read(buffer, 2, 1);

                if (buffer[2] == 254) return true;
                else {
                    port.Close();
                    return false;
                }

            }

            void GetSerialPort() {
                while (!portFound) {
                    string[] portNames = SerialPort.GetPortNames();
                    try {
                        foreach (var name in portNames) {
                            port = new SerialPort(name, 250000);
                            if (portFound = PortFound()) break;
                        }
                    }
                    catch (Exception) {
                        try {
                            port = new SerialPort(portNames[1], 250000);
                            if (portFound = PortFound()) break;
                        }
                        catch (Exception) { }
                    }
                }
            }

            internal void ComMain() {
                GetSerialPort();
                Byte[] buffer = new byte[2];
                buffer[1] = Convert.ToByte(253);

                if (!port.IsOpen) { port.Open(); }
                port.Write(buffer, 1, 1);
                Thread.Sleep(100);

                while (true) {
                    while (port.BytesToRead < 0) Thread.Sleep(1);

                    port.Read(buffer, 0, 1);
                    if (buffer[0] == 244) break;
                    Console.WriteLine("0x" + buffer[0].ToString("X"));
                    port.DiscardInBuffer();
                }
                buffer[1] = 252;
                port.Write(buffer, 1, 1);
                port.Close();
            }
        }
    }
}

/*
namespace Tester {
    class Program {
        static void Main(string[] args) {
            Serial _serial = new Serial();
            while (!_serial.portFound) {
                try {
                    string[] portNames = SerialPort.GetPortNames();
                    foreach (var name in portNames) {
                        _serial.port = new SerialPort(name, 9600);
                        if (_serial.portFound = _serial.PortFound()) break;
                    }
                }
                catch (Exception) { }
            }
            Byte[] buffer = new byte[2];
            buffer[1] = Convert.ToByte(253);
            if (!_serial.port.IsOpen) { _serial.port.Open(); }
            _serial.port.Write(buffer, 1, 1);
            Thread.Sleep(100);
            while (true) {
                int test = _serial.port.BytesToRead;
                if (test > 0) {
                    _serial.port.Read(buffer, 0, 1);
                    if (buffer[0] == 222) break;
                    Console.Write("0x" + buffer[0].ToString("X"));
                }
            }
            _serial.port.Close();
        }


    }
    class Serial {

        public SerialPort port;
        public bool portFound = false;

        public bool PortFound() {
            Byte[] buffer = new byte[4];
            buffer[0] = Convert.ToByte(255);

            port.Open();
            port.Write(buffer, 0, 1);
            Thread.Sleep(100);
            if (port.BytesToRead > 0) port.Read(buffer, 2, 1);

            if (buffer[2] == 254) return true;
            else {
                port.Close();
                return false;
            }

        }
    }
}
*/

