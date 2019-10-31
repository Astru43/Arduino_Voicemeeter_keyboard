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

        private OutputHandler oHandler;
        private Serial serial;

        Thread inputThread;
        Thread outputThread;

        public ArduinoMixerService() {
            InitializeComponent();
            eventLog1 = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("MySource")) {
                System.Diagnostics.EventLog.CreateEventSource(
                    "MySource", "MyNewLog");
            }
            eventLog1.Source = "MySource";
            eventLog1.Log = "MyNewLog";
        }

        protected override void OnStart(string[] args) {
            eventLog1.WriteEntry("Start");
            oHandler = new OutputHandler(ref eventLog1);
            serial = new Serial(ref oHandler);
            
            outputThread = new Thread(oHandler.Main) { Name = "Output" };
            outputThread.Start();
            inputThread = new Thread(serial.ComMain) { IsBackground = true, Name = "Input" };
            inputThread.Start();
        }

        protected override void OnStop() {
            serial.Quit();
            oHandler.PostQuit();
        }

    }

}
