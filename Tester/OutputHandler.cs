using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using VoiceMeeterWrapper;

namespace Tester {
    class OutputHandler {
        VmClient vm;
        float nVal;
        List<byte> cmd = new List<byte>();
        System.Timers.Timer tr = new System.Timers.Timer(10);

        private enum Mode {
            ON = 1,
            OFF = 0,
            UNKNOWN = -1
        }
        Mode _m = Mode.UNKNOWN;

        public void Main() {
            tr.Elapsed += OnTime;
            vm = new VmClient();
            tr.AutoReset = true;
            tr.Enabled = true;
            Thread.Sleep(50);

            while (true) {
                while (cmd.Count == 0) Thread.Sleep(1);
                if (cmd[0] == 0x04) break;
                switch (cmd[0]) {
                    case 0x023:
                        SwitchMode();
                        break;
                    case 0x22:
                        VolumeUp(3);
                        break;
                    case 0x21:
                        VolumeDown(3);
                        break;
                }
                cmd.RemoveAt(0);
            }

            tr.Dispose();
            vm.Dispose();
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
            }
            else if (_m == Mode.ON) {
                vm.SetParam("Strip(6).EQGain1", 0.0f);
                vm.SetParam("Strip(6).EQGain2", 0.0f);
                vm.SetParam("Bus(0).ReturnReverb", 0.0f);
                _m = Mode.OFF;
            }
            Thread.Sleep(20);
        }

        private void OnTime(object sender, ElapsedEventArgs e) {
            bool val = vm.Poll();
        }

        private void VolumeUp(float val) {
            nVal = vm.GetParam("Bus(4).Gain");
            Console.WriteLine(nVal);

            nVal += val;
            Console.WriteLine(nVal);
            vm.SetParam("Bus(4).Gain", nVal);
            Thread.Sleep(20);
        }

        private void VolumeDown(float val) {
            nVal = vm.GetParam("Bus(4).Gain");
            Console.WriteLine(nVal);

            nVal -= val;
            Console.WriteLine(nVal);
            vm.SetParam("Bus(4).Gain", nVal);
            Thread.Sleep(20);
        }

        internal void AddCmd(byte v) {
            lock (this) {
                cmd.Add(v);
            }
        }

        internal void PostQuit(byte v) {
            lock (this) {
                cmd.Insert(0, v);
            }
        }
    }
}