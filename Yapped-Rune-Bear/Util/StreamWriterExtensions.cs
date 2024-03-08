using System.Runtime.Intrinsics.X86;
using ICell = SoulsFormats.PARAM.ICell;
using DefType = SoulsFormats.PARAMDEF.DefType;
using int32_t = System.Int32;
using int64_t = System.Int64;
using uint32_t = System.UInt32;
using uint64_t = System.UInt64;
[assembly: SuppressMessage("Usage", "CA2252:This API requires opting into preview features")]

namespace Chomp.Util {
    [SkipLocalsInit]
    public static unsafe partial class StreamWriterExtensions {
        internal static readonly uint64_t[] thr = new uint64_t[64] {
          10000000000000000000UL, uint64_t.MaxValue, uint64_t.MaxValue, uint64_t.MaxValue, 1000000000000000000UL, uint64_t.MaxValue, uint64_t.MaxValue, 100000000000000000UL, uint64_t.MaxValue, uint64_t.MaxValue,
             10000000000000000UL, uint64_t.MaxValue, uint64_t.MaxValue, uint64_t.MaxValue,    1000000000000000UL, uint64_t.MaxValue, uint64_t.MaxValue,    100000000000000UL, uint64_t.MaxValue, uint64_t.MaxValue,
                10000000000000UL, uint64_t.MaxValue, uint64_t.MaxValue, uint64_t.MaxValue,       1000000000000UL, uint64_t.MaxValue, uint64_t.MaxValue,       100000000000UL, uint64_t.MaxValue, uint64_t.MaxValue,
                   10000000000UL, uint64_t.MaxValue, uint64_t.MaxValue, uint64_t.MaxValue,          1000000000UL, uint64_t.MaxValue, uint64_t.MaxValue,          100000000UL, uint64_t.MaxValue, uint64_t.MaxValue,
                      10000000UL, uint64_t.MaxValue, uint64_t.MaxValue, uint64_t.MaxValue,             1000000UL, uint64_t.MaxValue, uint64_t.MaxValue,             100000UL, uint64_t.MaxValue, uint64_t.MaxValue,
                         10000UL, uint64_t.MaxValue, uint64_t.MaxValue, uint64_t.MaxValue,                1000UL, uint64_t.MaxValue, uint64_t.MaxValue,                100UL, uint64_t.MaxValue, uint64_t.MaxValue,
                            10UL, uint64_t.MaxValue, uint64_t.MaxValue, uint64_t.MaxValue,
        };
        internal static readonly RefKeeper<uint64_t> thrRef = new (ref MemoryMarshal.GetArrayDataReference(thr));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe uint64_t ilog10(uint64_t v) {
            uint64_t lz = Lzcnt.X64.LeadingZeroCount(v);
            return ((63 - lz) * 77UL >> 7) + CastTo<uint64_t, bool>(v >= Unsafe.Add(ref thrRef.Reference, CastTo<uint32_t, uint64_t>(lz)));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe uint64_t ilog10(int64_t v) => ilog10(CastTo<uint64_t, int64_t>(v));
        public static unsafe void WriteAllocfree(this StreamWriter stream, ICell cell) {
            switch (cell.Def.DisplayType) {
                case DefType.s8:
                    stream.WriteAllocfree((sbyte)cell.Value);
                    break;
                case DefType.u8:
                    stream.WriteAllocfree((byte)cell.Value);
                    break;
                case DefType.s16:
                    stream.WriteAllocfree((short)cell.Value);
                    break;
                case DefType.u16:
                    stream.WriteAllocfree((ushort)cell.Value);
                    break;
                case DefType.s32:
                    stream.WriteAllocfree((int)cell.Value);
                    break;
                case DefType.u32:
                    stream.WriteAllocfree((uint)cell.Value);
                    break;
                case DefType.b32:
                    stream.WriteAllocfree((int)cell.Value);
                    break;
                case DefType.f32:
                case DefType.angle32:
                    // maybe in the future I will do version without heap allocation but float to string conversion is so complicated I don't want to deal with it
                    stream.WriteAllocfree((float)cell.Value, double_conversion.FastDtoaMode.FAST_DTOA_SHORTEST_SINGLE);
                    break;
                case DefType.f64:
                    stream.WriteAllocfree((double)cell.Value);
                    break;
                case DefType.dummy8:
                    break;
                case DefType.fixstr or DefType.fixstrW:
                    stream.WriteAllocfree(cell.Value.CastTo<string, object>());
                    break;
                default:
                    break;
            }
        }
        public static unsafe void WriteAllocfree(this StreamWriter stream, ReadOnlySpan<char> chars) {
            ref int pos = ref stream.GetInternalCharBufferPositionReference();
            int charsLength = chars.Length;
            if (charsLength < 5 && charsLength <= stream.GetInternalCharBufferLength() - pos) {
                ref char buffer = ref stream.GetInternalCharBuffer().GetReference(pos);
                for (int i = 0, l = charsLength * sizeof(char); i < l; i += sizeof(char)) {
                    Unsafe.AddByteOffset(ref buffer, i) = chars.GetReference(i);
                }
                //memcpy(AsPointer<char[], char>(buffer, pos * 2 + 16), chars.AsPointer(), (nuint)chars.Length << 1);
            } else {
                int length = stream.GetInternalCharBufferLength();
                if (length == pos) {
                    stream.FlushEfficient(length);
                }
                char[] buffer = stream.GetInternalCharBuffer();
                for (; charsLength > length - pos; stream.FlushEfficient(length)) {
                    chars[..(length - pos)].CopyTo(buffer.AsSpan(pos));
                    //memcpy(AsPointer<char[], char>(buffer, pos * sizeof(char) + 16), chars.AsPointer(), (nuint)(length - pos) << 1);
                    chars = chars[(length - pos)..];
                }
                //memcpy(AsPointer<char[], char>(buffer, pos * sizeof(char) + 16), chars.AsPointer(), (nuint)chars.Length << 1);
                chars.CopyTo(buffer.AsSpan(pos));
            }
            pos += charsLength;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteAllocfreeSmall(this StreamWriter stream, ReadOnlySpan<char> chars) {
            ref int pos = ref stream.GetInternalCharBufferPositionReference();
            if (chars.Length > stream.GetInternalCharBufferLength() - pos) {
                stream.FlushEfficient(stream.GetInternalCharBufferLength());
            }

            ref char buffer = ref stream.GetInternalCharBuffer().GetReference(pos);
            for (int i = 0, l = chars.Length * sizeof(char); i < l; i += sizeof(char)) {
                Unsafe.AddByteOffset(ref buffer, i) = chars.GetReference(i);
            }

            pos += chars.Length;
        }
        const int RN = ('\n' << (8 * sizeof(char))) + '\r';
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteNewline(this StreamWriter stream) {
            ref int pos = ref stream.GetInternalCharBufferPositionReference();
            if (pos + 2 >= stream.GetInternalCharBufferLength()) {
                stream.FlushEfficient(pos);
            }
            stream.GetInternalCharBuffer().AssignAnyAt((pos + 8) * sizeof(char), RN);
            pos += 2;
        }
        public static unsafe void WriteLineAllocfree(this StreamWriter stream, ICell cell) {
            stream.WriteAllocfree(cell);
            stream.WriteNewline();
        }
        public static unsafe void WriteLineAllocfree(this StreamWriter stream, string text) {
            stream.WriteAllocfree(text);
            stream.WriteNewline();
        }
        public static unsafe void WriteAllocfree(this StreamWriter stream, double number, double_conversion.FastDtoaMode mode = double_conversion.FastDtoaMode.FAST_DTOA_SHORTEST) {
            ref int pos = ref stream.GetInternalCharBufferPositionReference();
            char[] chars = stream.GetInternalCharBuffer();
            int length = stream.GetInternalCharBufferLength();
            if (number < 0) {
                stream.WriteAllocfreeSmall("-");
                number = -number;
            }
            if (number == 0) {
                stream.WriteAllocfreeSmall("0");
                return;
            }
            int int_part = (int)number;
            stream.WriteAllocfree(int_part);
            if(number - int_part != 0) {
                stream.WriteAllocfreeSmall(".");
            }
            int log10 = 0x300; // approx length of float with additional padding to be sure to not to overflow
            int lengthWritten = 0;
            if (log10 > length - pos) {
                Span<char> floatChars = stackalloc char[log10 + 0x10];
                double_conversion.FastDtoa(number, mode, floatChars, out lengthWritten, out _);
                stream.WriteAllocfree(floatChars[..lengthWritten]);
            } else {
                double_conversion.FastDtoa(number, mode, chars.AsSpan(pos), out lengthWritten, out _);
                pos += lengthWritten;
            }
        }
        private const int int_max_chars = 11;
        private const int uint_max_chars = 10;
        private const int short_max_chars = 6;
        private const int ushort_max_chars = 5;
        private const int sbyte_max_chars = 4;
        private const int byte_max_chars = 3;
        public static unsafe void WriteAllocfree(this StreamWriter stream, int number, int max_chars = int_max_chars) {
            int internalPos = stream.GetInternalCharBufferPosition();
            int pos = internalPos + 8 << 1;
            char[] chars = stream.GetInternalCharBuffer();
            int length = stream.GetInternalCharBufferLength();
            stream.SetInternalCharBufferPosition((internalPos + max_chars < length ?
                itoa_simple(chars, pos, number) :
                itoa_simple_checked(chars, stream, length + 8 << 1, pos, number)) - 16 >> 1);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteAllocfree(this StreamWriter stream, short number) => stream.WriteAllocfree(CastTo<int, short>(number), short_max_chars);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteAllocfree(this StreamWriter stream, sbyte number) => stream.WriteAllocfree(CastTo<int, sbyte>(number), sbyte_max_chars);
        public static unsafe void WriteAllocfree(this StreamWriter stream, uint number, int max_chars = uint_max_chars) {
            int internalPos = stream.GetInternalCharBufferPosition();
            int pos = internalPos + 8 << 1;
            char[] chars = stream.GetInternalCharBuffer();
            int length = stream.GetInternalCharBufferLength();
            stream.SetInternalCharBufferPosition((internalPos + max_chars < length ?
                uitoa_simple(chars, pos, number) :
                uitoa_simple_checked(chars, stream, length + 8 << 1, pos, number)) - 16 >> 1);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteAllocfree(this StreamWriter stream, ushort number) => stream.WriteAllocfree(CastTo<uint, ushort>(number), ushort_max_chars);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteAllocfree(this StreamWriter stream, byte number) => stream.WriteAllocfree(CastTo<uint, byte>(number), byte_max_chars);

        public static int32_t itoa_simple_helper(char[] dest, int32_t pos, int32_t i) {
            if (i <= -10) {
                (int div, i) = X86Base.DivRem(CastTo<uint32_t, int32_t>(i), -1, 10);
                pos = itoa_simple_helper(dest, pos, div);
            }
            dest.AssignAnyAt(pos, '0' - i);
            return pos + sizeof(char);
        }

        public static int32_t itoa_simple(char[] dest, int32_t pos, int32_t i) {
            if (i < 0) {
                dest.AssignAt(pos, '-');
                pos += sizeof(char);
            } else {
                i = -i;
            }
            return itoa_simple_helper(dest, pos, i);
        }
        public static int32_t itoa_simple_helper_checked(char[] dest, StreamWriter stream, int32_t max_buffer_length, int32_t pos, int32_t i) {
            if (i <= -10) {
                (int div, i) = X86Base.DivRem(CastTo<uint32_t, int32_t>(i), -1, 10);
                pos = itoa_simple_helper_checked(dest, stream, max_buffer_length, pos, div);
            }
            if (pos == max_buffer_length) {
                stream.FlushEfficient(dest.Length);
                pos = 16;
            }
            dest.AssignAt(pos, (char)('0' - i));
            return pos + sizeof(char);
        }

        public static int32_t itoa_simple_checked(char[] dest, StreamWriter stream, int32_t max_buffer_length, int32_t pos, int32_t i) {
            if (i < 0) {
                if (pos == max_buffer_length) {
                    stream.FlushEfficient(dest.Length);
                    pos = 16;
                }
                dest.AssignAt(pos, '-');
                pos += sizeof(char);
            } else {
                i = -i;
            }
            return itoa_simple_helper_checked(dest, stream, max_buffer_length, pos, i);
        }
        public static int32_t uitoa_simple(char[] dest, int32_t pos, uint32_t i) {
            if (i >= 10) {
                (int div, int temp) = X86Base.DivRem(i, 0, 10);
                pos = uitoa_simple(dest, pos, CastTo<uint32_t, int32_t>(div));
                i = CastTo<uint32_t, int32_t>(temp);
            }
            dest.AssignAnyAt(pos, '0' + i);
            return pos + sizeof(char);
        }
        public static int32_t uitoa_simple_checked(char[] dest, StreamWriter stream, int32_t max_buffer_length, int32_t pos, uint32_t i) {
            if (i >= 10) {
                (int div, int temp) = X86Base.DivRem(i, 0, 10);
                pos = uitoa_simple_checked(dest, stream, max_buffer_length, pos, CastTo<uint32_t, int32_t>(div));
                i = CastTo<uint32_t, int32_t>(temp);
            }
            if (pos == max_buffer_length) {
                stream.FlushEfficient(dest.Length);
                pos = 16;
            }
            dest.AssignAt(pos, (char)('0' + i));
            return pos + sizeof(char);
        }

        private static unsafe void FlushEfficient(this StreamWriter stream, int bytesCount) {
            if (!stream.GetInternalHaveWrittenPreamble()) {
                stream.SetInternalHaveWrittenPreamble(true);
                stream.BaseStream.Write(stream.Encoding.Preamble);
            }
            Span<byte> span = stackalloc byte[stream.Encoding.GetMaxByteCount(bytesCount)];
            stream.SetInternalCharBufferPosition(0);

            stream.BaseStream.Write(span[..stream.GetInternalEncoder().GetBytes(new ReadOnlySpan<char>(stream.GetInternalCharBuffer(), 0, bytesCount), span, false)]);
        }
    }
}
