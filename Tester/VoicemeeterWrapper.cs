using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace VoiceMeeterWrapper {
    public static class VoiceMeeterRemote {
        [DllImport("VoicemeeterRemote64.dll", EntryPoint = "VBVMR_Login")]
        public static extern VbLoginResponse Login();
        [DllImport("VoicemeeterRemote64.dll", EntryPoint = "VBVMR_Logout")]
        public static extern VbLoginResponse Logout();

        [DllImport("VoicemeeterRemote64.dll", EntryPoint = "VBVMR_SetParameterFloat")]
        public static extern int SetParameter(string szParamName, float value);
        [DllImport("VoicemeeterRemote64.dll", EntryPoint = "VBVMR_GetParameterFloat")]
        public static extern int GetParameter(string szParamName, ref float value);
        [DllImport("VoicemeeterRemote64.dll", EntryPoint = "VBVMR_GetParameterStringA")]
        public static extern int GetParameter(string szParamName, ref string value);

        [DllImport("VoicemeeterRemote64.dll", EntryPoint = "VBVMR_IsParametersDirty")]
        public static extern int IsParametersDirty();
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllToLoad);
        private static IntPtr? _dllHandle;
        public static void LoadDll(string dllPath) {
            if (!_dllHandle.HasValue) {
                _dllHandle = LoadLibrary(dllPath);
            }
        }
    }


    public class VmClient : IDisposable {
        private Action _onClose = null;
        private string GetVoicemeeterDir() {
            const string regKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
            const string uninstKey = "VB:Voicemeeter {17359A74-1236-5467}";
            var key = $"{regKey}\\{uninstKey}";
            var k = Registry.GetValue(key, "UninstallString", null);
            if (k == null) {
                throw new Exception("Voicemeeter not found");
            }
            return System.IO.Path.GetDirectoryName(k.ToString());
        }
        public VmClient() {
            //Find Voicemeeter dir.
            var vmDir = GetVoicemeeterDir();
            VoiceMeeterRemote.LoadDll(System.IO.Path.Combine(vmDir, "VoicemeeterRemote64.dll"));
                var lr = VoiceMeeterRemote.Login();
            switch (lr) {
                case VbLoginResponse.OK:
                    Console.WriteLine("Attached.");
                    break;
                case VbLoginResponse.AlreadyLoggedIn:
                    Console.WriteLine("Attached. Was already logged in");
                    break;
                case VbLoginResponse.OkVoicemeeterNotRunning:
                    //Launch.
                    Console.WriteLine("Attached. VM Not running.");
                    break;
                default:
                    throw new InvalidOperationException("Bad response from voicemeeter: " + lr);
            }
        }
        public float GetParam(string n) {
            float output = -1;
            VoiceMeeterRemote.GetParameter(n, ref output);
            return output;
        }
        public void SetParam(string n, float v) {
            VoiceMeeterRemote.SetParameter(n, v);
        }
        public bool Poll() {
            return VoiceMeeterRemote.IsParametersDirty() == 1;
        }
        private bool disposed = false;
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing) {
            if (!disposed) {
                Console.WriteLine($"VmClient Disposing {disposing}");
                _onClose?.Invoke();
                VoiceMeeterRemote.Logout();
            }
            disposed = true;
        }
        ~VmClient() { Dispose(false); }
        public void OnClose(Action a) {
            _onClose = a;
        }
    }
}

namespace VoiceMeeterWrapper {
    public enum VbLoginResponse {
        OK = 0,
        OkVoicemeeterNotRunning = 1,
        NoClient = -1,
        AlreadyLoggedIn = -2,
    }
}
