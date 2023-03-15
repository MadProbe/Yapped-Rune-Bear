using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chomp.Util {

    public struct OPFileWriter : IAsyncDisposable, IDisposable {
        readonly FileStream fileStream;
        public void Dispose() => throw new NotImplementedException();
        public ValueTask DisposeAsync() => throw new NotImplementedException();
        internal void Dispose(bool disposing) {
        }
    }
}
