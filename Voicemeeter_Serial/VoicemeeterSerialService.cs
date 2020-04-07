using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Ports;
using System.IO.Pipes;
using System.Runtime.InteropServices;

namespace Voicemeeter_Serial {

    public partial class VoicemeeterSerialService : ServiceBase {
        public VoicemeeterSerialService() {
            InitializeComponent();
            CanHandlePowerEvent = true;
        }

        public enum ServiceState {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus {
            public int dwServiceType;
            public ServiceState dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
        };

        Thread serial;
        bool stop = false;
        bool portFound = false;
        SerialPort port;
        NamedPipeServerStream outPipe;

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus) {
            ServiceStatus serviceStatus = new ServiceStatus { dwWaitHint = 10000 };
            if (powerStatus == PowerBroadcastStatus.Suspend) {
                stop = true;

                serial.Join(100);
                try {
                    if (serial.IsAlive) serial.Abort();
                    if (port.IsOpen) port.Dispose();
                    try {
                        outPipe.Disconnect();
                    }
                    catch (Exception) { }
                    outPipe.Dispose();
                }
                catch (Exception) { }

                serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
                return base.OnPowerEvent(powerStatus);
            }
            if (powerStatus == PowerBroadcastStatus.ResumeAutomatic) {
                serial = new Thread(new ThreadStart(Serial)) { IsBackground = true };
                stop = false;
                portFound = false;

                outPipe = new NamedPipeServerStream("VoicemeeterPipe", PipeDirection.Out);

                serial.Start();

                serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
                return base.OnPowerEvent(powerStatus);
            }
            return base.OnPowerEvent(powerStatus);
        }

        protected override void OnStart(string[] args) {
            ServiceStatus serviceStatus = new ServiceStatus {
                dwCurrentState = ServiceState.SERVICE_START_PENDING,
                dwWaitHint = 100000
            };
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            serial = new Thread(new ThreadStart(Serial)) { IsBackground = true };
            stop = false;
            portFound = false;

            outPipe = new NamedPipeServerStream("VoicemeeterPipe", PipeDirection.Out);

            serial.Start();

            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        protected override void OnStop() {
            ServiceStatus serviceStatus = new ServiceStatus {
                dwCurrentState = ServiceState.SERVICE_STOP_PENDING,
                dwWaitHint = 100000
            };
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            stop = true;

            serial.Join(100);

            if (outPipe.IsConnected) {
                outPipe.Flush();
                outPipe.Disconnect();
            }

            try {
                if (serial.IsAlive) serial.Abort();
            }
            catch (Exception) {
                ExitCode = 1;
            }
            try {
                if (port.IsOpen) port.Close();
            }
            catch (Exception) { }
            try {
                port.Dispose();
            }
            catch (Exception) { }

            outPipe.Dispose();

            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);

        void Serial() {
            outPipe.WaitForConnection();
            GetSerialPort();
            Byte[] buffer = new byte[2];
            buffer[1] = Convert.ToByte(0x83);

            if (!port.IsOpen) { port.Open(); }
            port.Write(buffer, 1, 1);
            Thread.Sleep(100);

            while (true) {
                //try {
                while (port.BytesToRead <= 0 && !stop) Thread.Sleep(1);

                if (stop) break;

                port.Read(buffer, 0, 1);
                //}
                /*catch (System.IO.IOException) {
                    port.Dispose();
                    return;
                }*/

                try {
                    outPipe.WriteByte(buffer[0]);
                }
                catch (System.IO.IOException) {
                    GetNewPipe();
                    port.DiscardInBuffer();
                }
            }
            port.Close();
        }

        void GetNewPipe() {
            outPipe.Dispose();
            outPipe = new NamedPipeServerStream("VoicemeeterPipe", PipeDirection.Out);
            outPipe.WaitForConnection();
        }

        void GetSerialPort() {
            while (!portFound) {
                string[] portNames = SerialPort.GetPortNames();
                foreach (var name in portNames) {
                    try {
                        port = new SerialPort(name, 250000);
                        if (portFound = PortFound()) {
                            break;
                        }
                    }
                    catch (Exception) {
                        try {
                            port = new SerialPort(name, 9600);
                            if (portFound = PortFound()) {
                                break;
                            }
                        }
                        catch (Exception) { }
                    }
                }
            }
        }

        bool PortFound() {
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
    }
}
