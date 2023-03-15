using static InlineIL.IL.Emit;
using static InlineIL.IL;

namespace Chomp.Util {
    public static class Shenanigans {
        public unsafe static _2* ReadOnlySpanAsPointer<_1, _2>(this ReadOnlySpan<_1> @_) where _2 : unmanaged {
            Ldarg_0();
            return ReturnPointer<_2>();
        }
    }
}
