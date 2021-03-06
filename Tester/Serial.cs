﻿using System;
using System.Threading;
using System.IO.Ports;

namespace Tester {
    class Serial {

        bool quit = false;
        public SerialPort port;
        public bool portFound = false;
        private readonly OutputHandler oHandler;

        public Serial(ref OutputHandler oHandler) {
            this.oHandler = oHandler;
        }

        public bool PortFound() {
            Byte[] buffer = new byte[4];
            buffer[0] = Convert.ToByte(0x81);

            port.Open();
            port.Write(buffer, 0, 1);
            Thread.Sleep(100);
            if (port.BytesToRead > 0) port.Read(buffer, 2, 1);

            if (buffer[2] == 0x82) return true;
            else {
                port.Close();
                return false;
            }

        }

        void GetSerialPort() {
            while (!portFound) {
                string[] portNames = SerialPort.GetPortNames();
                foreach (var name in portNames) {
                    try {
                        port = new SerialPort(name, 250000);
                        if (portFound = PortFound()) {
                            Console.WriteLine("No Error : " + portFound);
                            break;
                        }

                    }
                    catch (Exception) {
                        try {
                            port = new SerialPort(name, 9600);
                            if (portFound = PortFound()) {
                                Console.WriteLine("Error : " + portFound);
                                break;
                            }
                        }
                        catch (Exception) { }
                    }
                }
            }
        }

        internal void Quit() => quit = true;

        internal void ComMain() {
            GetSerialPort();
            Byte[] buffer = new byte[2];
            buffer[1] = Convert.ToByte(0x83);

            if (!port.IsOpen) { port.Open(); }
            port.Write(buffer, 1, 1);
            Thread.Sleep(100);

            while (true) {
                try {
                    while (port.BytesToRead <= 0 && !quit) Thread.Sleep(1);

                    if (quit) break;

                    port.Read(buffer, 0, 1);
                    if (buffer[0] == 0x0c) {
                        oHandler.PostQuit();
                        Console.WriteLine("0x" + buffer[0].ToString("X"));
                        break;
                    }
                    //Console.WriteLine("0x" + buffer[0].ToString("X"));
                    oHandler.AddCmd(buffer[0]);
                    //if ((buffer[0]&0x20) == 0x0) Console.WriteLine("");
                } catch (InvalidOperationException) {
                    portFound = false;
                    GetSerialPort();
                    if (!port.IsOpen) port.Open();
                }
            }
            port.Close();
        }
    }
}