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
        static readonly string appGuid = ((GuidAttribute)Assembly
            .GetExecutingAssembly()
            .GetCustomAttributes(typeof(GuidAttribute), false)
            .GetValue(0)).Value.ToString();

        static readonly Mutex mutex = new Mutex(true, String.Format("{{{0}}}", appGuid));
        static bool stop;
        static NamedPipeClientStream inPipe;
        static OutputHandler oHandler;

        [STAThread]
        static void Main() {
            if (mutex.WaitOne(TimeSpan.Zero, true)) {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                NotifyIcon nIcon = new NotifyIcon() { Visible = true, Text = "Exit Program", Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath) };
                nIcon.DoubleClick += new EventHandler(NIcon_DoubleClick);

                stop = false;

                oHandler = new OutputHandler();
                Thread oHandlerThread = new Thread(new ThreadStart(oHandler.Main));
                oHandlerThread.Start();

                Thread pipeHandlerThread = new Thread(new ThreadStart(PipeHandler));
                pipeHandlerThread.Start();

                oHandlerThread.Join();
            }
        }

        static void PipeHandler() {
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            inPipe = new NamedPipeClientStream(".", "VoicemeeterPipe", PipeDirection.In);
            Task pipe = Task.Factory.StartNew(() => {
                inPipe.Connect();
                while (true) {
                    int cmd = inPipe.ReadByte();
                    if (cmd == -1) NewPipe();
                    else oHandler.AddCmd(cmd);
                }
            },token);

            while (!stop) {
                Thread.Sleep(10);
            }
            source.Cancel();
            inPipe.Dispose();
        }

        static void NewPipe() {
            Console.WriteLine("Pipe disconected\n Reseting...");
            inPipe.Dispose();
            inPipe = new NamedPipeClientStream(".", "VoicemeeterPipe", PipeDirection.In);
            inPipe.Connect();
            Console.WriteLine("Pipe connected");
        }

        private static void NIcon_DoubleClick(object sender, EventArgs e) {
            stop = true;
            oHandler.PostQuit();
        }
    }
}
