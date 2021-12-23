using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;

namespace Vm_Serial {
    public class OutputHandler {
        private NamedPipeServerStream? _pipe;

        public OutputHandler() {
            getNewPipe();
        }

        public void getNewPipe() {
            _pipe?.Dispose();
            _pipe = new NamedPipeServerStream("VoicemeeterPipe", PipeDirection.Out);
            _pipe.WaitForConnection();
        }

        public void Write(byte[] data) {
            _pipe?.WriteByte(data[0]);
        }

    }
}
