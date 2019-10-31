using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using VoiceMeeterWrapper;
using System;

namespace Tester {
    class Program {

        static Serial serial;
        static Thread inputThread;
        static OutputHandler oHandler;
        static Thread outputThread;

        static void Main() {
            //Console.Title = "Mixer controller";

            oHandler = new OutputHandler();
            serial = new Serial(ref oHandler);

            StartThreads();
            inputThread.Join();
        }

        private static void OnExit(object sender, EventArgs e) {
        }

        private static void StartThreads() {
            outputThread = new Thread(new ThreadStart(oHandler.Main)) { Name = "Output Thread" };
            outputThread.Start();

            inputThread = new Thread(new ThreadStart(serial.ComMain)) { IsBackground = true, Name = "Input Thread" };
            inputThread.Start();
        }

    }
}