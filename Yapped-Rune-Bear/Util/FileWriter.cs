using System.Runtime.Intrinsics.X86;
using Cell = SoulsFormats.PARAM.Cell;
using DefType = SoulsFormats.PARAMDEF.DefType;
using int32_t = System.Int32;
using MiniCell = SoulsFormats.PARAM.MiniCell;
using uint32_t = System.UInt32;

namespace Chomp.Util {
    [SkipLocalsInit]
    public unsafe ref partial struct FileWriter {
        public static readonly Encoding encoding = new UTF8Encoding(false, true);
        public readonly Encoder encoder = encoding.GetEncoder();
        public readonly FileStream fileStream;
        public readonly Span<char> char_buffer;
        public int position;
        public bool haveWrittenPreamble = encoding.Preamble.Length == 0;
        public FileWriter(string path, FileStreamOptions options, Span<char> buffer) : this(new FileStream(path, options), buffer) { }
        public FileWriter(FileStream fileStream, Span<char> buffer) {
            this.fileStream = fileStream;
            this.char_buffer = buffer;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(scoped ReadOnlySpan<byte> bytes) => this.fileStream.Write(bytes);
        public unsafe void Write(Cell cell) {
            switch (cell.Def.DisplayType) {
                case DefType.s8:
                    this.Write((sbyte)cell.Value);
                    break;
                case DefType.u8:
                    this.Write((byte)cell.Value);
                    break;
                case DefType.s16:
                    this.Write((short)cell.Value);
                    break;
                case DefType.u16:
                    this.Write((ushort)cell.Value);
                    break;
                case DefType.s32:
                    this.Write((int)cell.Value);
                    break;
                case DefType.u32:
                    this.Write((uint)cell.Value);
                    break;
                case DefType.b32:
                    this.Write((int)cell.Value);
                    break;
                case DefType.f32:
                case DefType.angle32:
                    // maybe in the future I will do version without heap allocation but float to string conversion is so complicated I don't want to deal with it
                    this.Write(cell.Value.ToString());
                    this.Write((float)cell.Value, double_conversion.FastDtoaMode.FAST_DTOA_SHORTEST_SINGLE);
                    break;
                case DefType.f64:
                    this.Write(cell.Value.ToString());
                    this.Write((double)cell.Value);
                    break;
                case DefType.dummy8:
                    break;
                case DefType.fixstr or DefType.fixstrW:
                    this.Write(cell.Value.CastTo<string, object>());
                    break;
                default:
                    break;
            }
        }
        public unsafe void Write(DefType type, MiniCell cell) {
            switch (type) {
                case DefType.s8:
                    this.Write(cell.SByteValue);
                    break;
                case DefType.u8:
                    this.Write(cell.ByteValue);
                    break;
                case DefType.s16:
                    this.Write(cell.ShortValue);
                    break;
                case DefType.u16:
                    this.Write(cell.UShortValue);
                    break;
                case DefType.s32:
                    this.Write(cell.IntValue);
                    break;
                case DefType.u32:
                    this.Write(cell.UIntValue);
                    break;
                case DefType.b32:
                    this.Write(cell.IntValue);
                    break;
                case DefType.f32 or DefType.angle32:
                    // maybe in the future I will do version without heap allocation but float to string conversion is so complicated I don't want to deal with it
                    this.Write(cell.FloatValue.ToString());
                    this.Write(cell.FloatValue, double_conversion.FastDtoaMode.FAST_DTOA_SHORTEST_SINGLE);
                    break;
                case DefType.f64:
                    this.Write(cell.DoubleValue.ToString());
                    this.Write(cell.DoubleValue);
                    break;
                case DefType.dummy8:
                    break;
                case DefType.fixstr or DefType.fixstrW:
                    this.Write(cell.StringValue);
                    break;
                default:
                    break;
            }
        }
        public unsafe void Write(scoped ReadOnlySpan<char> chars) {
            int charsLength = chars.Length;
            if (charsLength <= 4 && charsLength <= this.char_buffer.Length - this.position) {
                ref char buffer = ref this.char_buffer.GetReference(this.position * sizeof(char));
                for (int i = 0, l = charsLength * sizeof(char); i < l; i += sizeof(char)) {
                    Unsafe.AddByteOffset(ref buffer, i) = chars.GetReference(i);
                }
                //memcpy(AsPointer<char[], char>(buffer, pos * 2 + 16), chars.AsPointer(), (nuint)chars.Length << 1);
            } else {
                int length = this.char_buffer.Length;
                if (length == this.position) {
                    this.Flush(length);
                }
                for (; charsLength > length - this.position; this.Flush(length)) {
                    chars.CopyTo(this.char_buffer, this.position * sizeof(char), length - this.position);
                    //memcpy(AsPointer<char[], char>(buffer, pos * sizeof(char) + 16), chars.AsPointer(), (nuint)(length - pos) << 1);
                    chars = chars[(length - this.position)..];
                }
                //memcpy(AsPointer<char[], char>(buffer, pos * sizeof(char) + 16), chars.AsPointer(), (nuint)chars.Length << 1);
                chars.CopyTo(this.char_buffer, this.position * sizeof(char), chars.Length);
            }
            this.position += charsLength;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteSmallString(scoped ReadOnlySpan<char> chars) {
            if (chars.Length >= this.char_buffer.Length - this.position) {
                this.Flush(this.position);
            }

            scoped ref char buffer = ref this.char_buffer.GetReference(this.position * sizeof(char));
            for (int i = 0, l = chars.Length * sizeof(char); i < l; i += sizeof(char)) {
                Unsafe.AddByteOffset(ref buffer, i) = chars.GetReference(i);
            }

            this.position += chars.Length;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteChar(char @char) {
            if (this.position >= this.char_buffer.Length) {
                this.Flush(this.position);
            }
            this.char_buffer.AssignAnyAt(this.position++, @char);
        }
        const int RN = ('\n' << (8 * sizeof(char))) + '\r';
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteNewline() {
            if (this.position + 2 >= this.char_buffer.Length) {
                this.Flush(this.position);
            }
            this.char_buffer.AssignAnyAt(this.position, RN);
            this.position += 2;
        }
        public void WriteLine(Cell cell) {
            this.Write(cell);
            this.WriteNewline();
        }
        public void WriteLine(DefType type, MiniCell cell) {
            this.Write(type, cell);
            this.WriteNewline();
        }
        public unsafe void WriteLine(string text) {
            this.Write(text);
            this.WriteNewline();
        }
        public unsafe void Write(double number, double_conversion.FastDtoaMode mode = double_conversion.FastDtoaMode.FAST_DTOA_SHORTEST) {
            this.WriteChar('f');
            if (number < 0) {
                this.WriteChar('-');
                number = -number;
            }
            if (number == 0) {
                this.WriteChar('0');
                return;
            }
            scoped Span<char> floatChars = stackalloc char[0x300];
            _ = double_conversion.FastDtoa(number, mode, floatChars, out int lengthWritten, out int decimal_point);
            this.Write(floatChars[..lengthWritten]);
        }
        private const int int_max_chars = 11;
        private const int uint_max_chars = 10;
        private const int short_max_chars = 6;
        private const int ushort_max_chars = 5;
        private const int sbyte_max_chars = 4;
        private const int byte_max_chars = 3;
        public unsafe void Write(int number, int max_chars = int_max_chars) {
            if (this.position + max_chars < this.char_buffer.Length) {
                this.itoa_simple(number);
            } else {
                this.itoa_simple_checked(number);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(short number) => this.Write(CastTo<int, short>(number), short_max_chars);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(sbyte number) => this.Write(CastTo<int, sbyte>(number), sbyte_max_chars);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(uint number, int max_chars = uint_max_chars) {
            if (this.position + max_chars < this.char_buffer.Length) {
                this.uitoa_simple(number);
            } else {
                this.uitoa_simple_checked(number);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ushort number) => this.Write(CastTo<uint, ushort>(number), ushort_max_chars);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(byte number) => this.Write(CastTo<uint, byte>(number), byte_max_chars);
        public void itoa_simple_helper(int32_t i) {
            if (i <= -10) {
                (int div, i) = X86Base.DivRem(CastTo<uint32_t, int32_t>(i), -1, 10);
                this.itoa_simple_helper(div);
            }
            this.char_buffer.AssignAnyAt(this.position++, '0' - i);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void itoa_simple(int32_t i) {
            if (i < 0) {
                this.char_buffer.AssignAnyAt(this.position++, '-');
            } else {
                i = -i;
            }
            this.itoa_simple_helper(i);
        }
        public void itoa_simple_helper_checked(int32_t i) {
            if (i <= -10) {
                (int div, i) = X86Base.DivRem(CastTo<uint32_t, int32_t>(i), -1, 10);
                this.itoa_simple_helper_checked(div);
            }
            if (this.position == this.char_buffer.Length) {
                this.Flush(this.char_buffer.Length);
            }
            this.char_buffer.AssignAnyAt(this.position++, (char)('0' - i));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void itoa_simple_checked(int32_t i) {
            if (i < 0) {
                this.WriteChar('-');
            } else {
                i = -i;
            }
            this.itoa_simple_helper_checked(i);
        }
        public void uitoa_simple(uint32_t i) {
            if (i >= 10) {
                (int div, int temp) = X86Base.DivRem(i, 0, 10);
                this.uitoa_simple(CastTo<uint32_t, int32_t>(div));
                i = CastTo<uint32_t, int32_t>(temp);
            }
            this.char_buffer.AssignAnyAt(this.position++, CastTo<char, uint32_t>('0' + i));
        }
        public void uitoa_simple_checked(uint32_t i) {
            if (i >= 10) {
                (int div, int temp) = X86Base.DivRem(i, 0, 10);
                this.uitoa_simple_checked(CastTo<uint32_t, int32_t>(div));
                i = CastTo<uint32_t, int32_t>(temp);
            }
            if (this.position == this.char_buffer.Length) {
                this.Flush(this.char_buffer.Length);
            }
            this.char_buffer.AssignAnyAt(this.position++, (char)('0' + i));
        }

        private unsafe void Flush(int bytesCount, bool flushEncoder = false) {
            int offset = encoding.Preamble.Length * CastTo<byte, bool>(!this.haveWrittenPreamble), length = encoding.GetMaxByteCount(bytesCount);
            byte* bytes = stackalloc byte[length + offset];

            if (!this.haveWrittenPreamble) {
                this.haveWrittenPreamble = true;
                encoding.Preamble.CopyTo(bytes);
            }

            this.Write(new ReadOnlySpan<byte>(bytes, this.encoder.GetBytes(this.char_buffer.AsPointer(), bytesCount, bytes + offset, length, flushEncoder) + offset));
            this.position = 0;
        }
        public void Dispose() {
            if (this.position != 0) {
                this.Flush(this.position, true);
            }
            this.fileStream.Dispose();
        }
    }
}
