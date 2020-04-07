using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using VoiceMeeterWrapper;
using System;
using System.Windows.Forms;

namespace Tester {
    class Program {

        static Serial serial;
        static Thread inputThread;
        static OutputHandler oHandler;
        static Thread outputThread;
        static readonly Mutex mutex = new Mutex(true, "{02099ead-bb0c-4994-b20e-6a858174a43b}");

        [STAThread]
        static void Main() {
            if (mutex.WaitOne(TimeSpan.Zero, true)) {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                NotifyIcon nIcon = new NotifyIcon() { Visible = true, Text = "Exit Program", ContextMenu = new ContextMenu(), Icon = new System.Drawing.Icon("Icons\\O-5.ico") };
                //nIcon.ContextMenu.MenuItems.Add("Exit", new EventHandler(NIcon_Exit_onClick));
                nIcon.DoubleClick += NIcon_DoubleClick;
                

                oHandler = new OutputHandler(ref nIcon);
                serial = new Serial(ref oHandler);

                StartThreads();
                inputThread.Join();
                nIcon.Dispose();
            }
        }

        private static void NIcon_DoubleClick(object sender, EventArgs e) {oHandler.PostQuit();
            serial.Quit();
        }

        private static void NIcon_Exit_onClick(object sender, EventArgs e) {
            oHandler.PostQuit();
            serial.Quit();
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