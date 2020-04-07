using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using VoiceMeeterWrapper;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Voicemeeter_Keyboard {
    class OutputHandler {
        VmClient vm;
        float nVal;
        readonly List<byte> cmd = new List<byte>();
        readonly Timer tr = new Timer(10);

        private enum Mode {
            ON = 1,
            OFF = 0,
            UNKNOWN = -1
        }
        Mode _m = Mode.UNKNOWN;
        private struct VolumeLane {
            public enum Vol {
                Strip7 = 0x2b,
                Strip6 = 0x2a,
                Strip5 = 0x29,
                Music = 0xff

            }
            public Vol cVol;
            public string GetLane() {
                switch (cVol) {
                    case Vol.Strip5:
                        return "Strip(5).Gain";
                    case Vol.Strip6:
                        return "Strip(6).Gain";
                    case Vol.Strip7:
                        return "Strip(7).Gain";
                    default:
                        return "Bus(4).Gain";
                }
            }
        }
        VolumeLane vL = new VolumeLane();
        private int vol = 3;

        public void Main() {
            tr.Elapsed += OnTime;
            vm = new VmClient();
            tr.AutoReset = true;
            tr.Enabled = true;
            vL.cVol = VolumeLane.Vol.Music;
            Thread.Sleep(50);

            while (true) {
                while (cmd.Count == 0) Thread.Sleep(1);
                if (cmd[0] == 0xff) break;
                switch (cmd[0]) {
                    case 0x29:
                    case 0x2a:
                    case 0x2b:
                        SwitchVolOutput(cmd[0]);
                        break;
                    case 0x28:
                        SwitchMute();
                        break;
                    case 0x027:
                        SwitchMode();
                        break;
                    case 0x22:
                        VolumeUp(vol);
                        break;
                    case 0x21:
                        VolumeDown(vol);
                        break;
                    case 0x26:
                        vm.SetParam("Command.Restart", 1.0f);
                        break;
                    case 0x23:
                        vol = vol == 3 ? 1 : 3;
                        break;
                }
                cmd.RemoveAt(0);
            }

            tr.Dispose();
            vm.Dispose();
        }

        private void SwitchVolOutput(byte v) {
            switch (v) {
                case 0x29:
                    if (vL.cVol == VolumeLane.Vol.Strip5) {
                        vL.cVol = VolumeLane.Vol.Music;
                    } else vL.cVol = VolumeLane.Vol.Strip5;
                    break;
                case 0x2a:
                    if (vL.cVol == VolumeLane.Vol.Strip6) {
                        vL.cVol = VolumeLane.Vol.Music;
                    } else vL.cVol = VolumeLane.Vol.Strip6;
                    break;
                case 0x2b:
                    if (vL.cVol == VolumeLane.Vol.Strip7) {
                        vL.cVol = VolumeLane.Vol.Music;
                        //nIcon.Icon = new System.Drawing.Icon("Icons\\O-5.ico");
                    } else {
                        vL.cVol = VolumeLane.Vol.Strip7;
                        //nIcon.Icon = new System.Drawing.Icon("Icons\\I-8.ico");
                    }
                    break;
            }
            Console.WriteLine(vL.cVol + "\n");
        }

        private void SwitchMute() {
            if (vm.GetParam("Bus(4).Mute") == 1.0f) {
                vm.SetParam("Bus(4).Mute", 0.0f);
            } else {
                vm.SetParam("Bus(4).Mute", 1.0f);
            }
            Thread.Sleep(20);
        }

        private void SwitchMode() {
            if (_m == Mode.UNKNOWN) {
                nVal = vm.GetParam("Bus(0).ReturnReverb");
                if (nVal > 3.0f) {
                    _m = Mode.ON;
                } else _m = Mode.OFF;
            }
            if (_m == Mode.OFF) {
                vm.SetParam("Strip(6).EQGain1", 8.0f);
                vm.SetParam("Strip(6).EQGain2", 4.0f);
                vm.SetParam("Bus(0).ReturnReverb", 3.4f);
                _m = Mode.ON;
            } else if (_m == Mode.ON) {
                vm.SetParam("Strip(6).EQGain1", 0.0f);
                vm.SetParam("Strip(6).EQGain2", 0.0f);
                vm.SetParam("Bus(0).ReturnReverb", 0.0f);
                _m = Mode.OFF;
            }
            Thread.Sleep(20);
        }

        private void OnTime(object sender, ElapsedEventArgs e) {
            _ = vm.Poll();
        }

        private void VolumeUp(float val) {
            nVal = vm.GetParam(vL.GetLane());
            Console.WriteLine(nVal);

            nVal += val;
            Console.WriteLine(nVal + "\n");
            vm.SetParam(vL.GetLane(), nVal);
            Thread.Sleep(20);
        }

        private void VolumeDown(float val) {
            nVal = vm.GetParam(vL.GetLane());
            Console.WriteLine(nVal);

            nVal -= val;
            Console.WriteLine(nVal + "\n");
            vm.SetParam(vL.GetLane(), nVal);
            Thread.Sleep(20);
        }

        public void AddCmd(int newCmd) {
            lock (cmd) {
                cmd.Add(Convert.ToByte(newCmd));
            }
        }

        public void PostQuit() {
            lock (cmd) {
                cmd.Insert(0, 0xff);
            }
        }
    }
}
