using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Pipes;

namespace Voicemeeter_Keyboard {
    class Program {
        static readonly Guid appGuid = new Guid("6fc38b01-cb8a-48a6-9b18-206d059ec842");
        static readonly Mutex mutex = new Mutex(true, string.Format("{{{0}}}", appGuid.ToString()));
        private static bool stop;
        private static NamedPipeClientStream? inPipe;
        private static OutputHandler? oHandler;

        [STAThread]
        static void Main() {
            MessageBox.Show(appGuid.ToString());

            if (mutex.WaitOne(TimeSpan.Zero, true)) {
                Application.EnableVisualStyles();
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.SetCompatibleTextRenderingDefault(false);

                NotifyIcon nIcon = new NotifyIcon() { Visible = true, Text = "Exit Program", Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath) };
                nIcon.DoubleClick += new EventHandler(NIcon_DoubleClick);

                stop = false;

                oHandler = new OutputHandler();
                Thread oHandlerThread = new(new ThreadStart(oHandler.Main));
                oHandlerThread.Name = "Output thread";
                oHandlerThread.Start();

                Thread pipeHandlerThread = new(new ThreadStart(PipeHandler));
                pipeHandlerThread.Name = "Pipe thread";
                pipeHandlerThread.Start();

                oHandlerThread.Join();
            } else {
                MessageBox.Show("Application already running");
            }
        }

        static void PipeHandler() {
            CancellationTokenSource source = new();
            CancellationToken token = source.Token;
            inPipe = new NamedPipeClientStream(".", "VoicemeeterPipe", PipeDirection.In);
            Task pipe = Task.Factory.StartNew(() => {
                inPipe.Connect();
                while (true) {
                    int cmd = inPipe.ReadByte();
                    if (cmd == -1) NewPipe();
                    else oHandler?.AddCmd(cmd);
                }
            }, token);

            while (!stop) {
                Thread.Sleep(10);
            }
            source.Cancel();
            inPipe.Dispose();
        }

        static void NewPipe() {
            Console.WriteLine("Pipe disconected\n Reseting...");
            inPipe?.Dispose();
            inPipe = new NamedPipeClientStream(".", "VoicemeeterPipe", PipeDirection.In);
            inPipe.Connect();
            Console.WriteLine("Pipe connected");
        }

        private static void NIcon_DoubleClick(object? sender, EventArgs e) {
            stop = true;
            oHandler?.PostQuit();
        }
    }
}
