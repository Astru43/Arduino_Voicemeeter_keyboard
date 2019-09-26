using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Ports;



namespace ArduinoMixer {
    public partial class ArduinoMixerService : ServiceBase {
        SerialThread serial;
        byte[] buffer;

        public ArduinoMixerService() {
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {
            buffer = new byte[256];
            serial = new SerialThread();

            serial.serialThread = new Thread(new ThreadStart(serial.LoadSerial)) { IsBackground = true };
            serial.serialThread.Start();
        }

        protected override void OnStop() {
            if (serial.serialThread != null) serial.serialThread.Abort();
            serial.serialThread = null;
            buffer = null;
        }

    }

    class SerialThread {
        SerialPort port;
        public Thread serialThread;
        string portName = null;

        public void LoadSerial() {
            string[] portNames = SerialPort.GetPortNames();
            while (portName == null) {
                try {
                    foreach (var name in portNames) {
                        port = new SerialPort(name, 9600);
                        if (PortFound()) break;
                    }
                }
                catch (Exception) { }
            }
            if (!port.IsOpen) port.Open();
            Byte[] buffer = new byte[4];
            buffer[2] = Convert.ToByte(253);
            port.Write(buffer, 2, 1);
            while (true) {
                if(port.BytesToRead > 0) {

                }
            }
        }

        bool PortFound() {
            Byte[] buffer = new byte[4];
            buffer[0] = Convert.ToByte(255);

            port.Open();
            port.Write(buffer, 0, 1);
            Thread.Sleep(100);
            if (port.BytesToRead > 0) port.Read(buffer, 2, 1);

            if (buffer[2] == 254) {
                portName = port.PortName;
                return true;
            } else {
                port.Close();
                return false;
            }
        }
    }
}
