using System.Runtime.Intrinsics.X86;
using SoulsFormats.Util;
using static SoulsFormats.PARAM;
using static SoulsFormats.PARAMDEF;
using int32_t = System.Int32;
using uint32_t = System.UInt32;

namespace Chomp.Util {
    [SkipLocalsInit]
    public unsafe ref struct UTF8FileWriter {
        public static readonly UTF8Encoding encoding = new (false, true);
        public readonly FileStream fileStream;
        public readonly Span<byte> char_buffer;
        public int position;
        public bool haveWrittenPreamble = encoding.Preamble.Length == 0;
        public UTF8FileWriter(string path, FileStreamOptions options, Span<byte> buffer) : this(new FileStream(path, options), buffer) { }

        public UTF8FileWriter(FileStream fileStream, Span<byte> buffer) {
            this.fileStream = fileStream;
            this.char_buffer = buffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(DefType type, ref MiniCell cell, int array_length) {
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
                    this.Write(cell.FloatValue, double_conversion.FastDtoaMode.FAST_DTOA_SHORTEST_SINGLE);
                    break;
                case DefType.f64:
                    this.Write(cell.DoubleValue);
                    break;
                case DefType.dummy8:
                    break;
                case DefType.fixstr:
                    ReadOnlySpan<byte> inputSpan = new(AsPointer<MiniCell, byte>(ref cell), array_length);
                    Span<char> buffer = stackalloc char[Utility.GetMaxCharBytesOfFixedString(inputSpan)];
                    this.Write(Utility.GetFixedString(buffer, inputSpan));
                    break;
                case DefType.fixstrW:
                    this.Write(new ReadOnlySpan<char>(AsPointer<MiniCell, char>(ref cell), array_length));
                    break;
            }
        }

        public void Write(scoped ReadOnlySpan<byte> chars) {
            int charsLength = chars.Length;
            if (charsLength <= 4 && charsLength <= this.char_buffer.Length - this.position) {
                scoped ref byte buffer = ref this.char_buffer.GetReference(this.position);
                for (var i = 0; i < charsLength; i++) Unsafe.AddByteOffset(ref buffer, i) = chars.GetReference(i);
                //memcpy(AsPointer<char[], char>(buffer, pos * 2 + 16), chars.AsPointer(), (nuint)chars.Length << 1);
            } else {
                int length = this.char_buffer.Length;
                if (length == this.position) this.Flush(length);
                for (; charsLength > length - this.position; this.Flush(length)) {
                    chars.CopyTo(this.char_buffer, this.position, length - this.position);
                    //memcpy(AsPointer<char[], char>(buffer, pos * sizeof(char) + 16), chars.AsPointer(), (nuint)(length - pos) << 1);
                    chars = chars[(length - this.position)..];
                }

                //memcpy(AsPointer<char[], char>(buffer, pos * sizeof(char) + 16), chars.AsPointer(), (nuint)chars.Length << 1);
                chars.CopyTo(this.char_buffer, this.position, chars.Length);
            }

            this.position += charsLength;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSmallString(scoped ReadOnlySpan<byte> chars) {
            if (chars.Length > this.char_buffer.Length - this.position) this.Flush(this.position);

            scoped ref byte buffer = ref this.char_buffer.GetReference(this.position);
            for (int i = 0, l = chars.Length; i < l;) Unsafe.AddByteOffset(ref buffer, i) = chars.GetReference(i++);

            this.position += chars.Length;
        }

        public void Write(scoped ReadOnlySpan<char> chars) {
            int byteCount = encoding.GetByteCount(chars);
            if (byteCount <= this.char_buffer.Length - this.position) {
                encoding.GetBytes(chars, this.char_buffer[this.position..]);
                //memcpy(AsPointer<char[], char>(buffer, pos * 2 + 16), chars.AsPointer(), (nuint)chars.Length << 1);
            } else {
                Span<byte> bytes = byteCount > 0x10000 ? stackalloc byte[byteCount] : GC.AllocateUninitializedArray<byte>(byteCount, true);
                encoding.GetBytes(chars, this.char_buffer[this.position..]);
                int length = this.char_buffer.Length;
                if (length == this.position) this.Flush(length);
                for (; byteCount > length - this.position; this.Flush(length)) {
                    int remainingLength = length - this.position;
                    bytes.CopyTo(this.char_buffer, this.position, remainingLength);
                    bytes = bytes[remainingLength..];
                }

                bytes.CopyTo(this.char_buffer, this.position, bytes.Length);
            }

            this.position += byteCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChar(char @char) {
            if (this.position >= this.char_buffer.Length) this.Flush(this.position);
            this.char_buffer.AssignAnyAt(this.position++, @char);
        }

        private const short RN = ('\n' << 8 * sizeof(byte)) + '\r';

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNewline() {
            if (this.position + 2 > this.char_buffer.Length) this.Flush(this.position);
            this.char_buffer.AssignAnyAt(this.position, RN);
            this.position += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteLine(DefType type, ref MiniCell cell, int array_length) {
            this.Write(type, ref cell, array_length);
            this.WriteNewline();
        }

        public void WriteLine(string text) {
            this.Write(text);
            this.WriteNewline();
        }

        public void Write(double number, double_conversion.FastDtoaMode mode = double_conversion.FastDtoaMode.FAST_DTOA_SHORTEST) {
            if (number < 0) {
                this.WriteChar('-');
                number = -number;
            }

            if (number == 0) {
                this.WriteChar('0');
                return;
            }

            scoped Span<char> floatChars = stackalloc char[0x300];
            _ = double_conversion.FastDtoa(number, mode, floatChars, out int lengthWritten, out _);
            this.Write(floatChars[..lengthWritten]);
        }

        private const int int_max_chars = 11;
        private const int uint_max_chars = 10;
        private const int short_max_chars = 6;
        private const int ushort_max_chars = 5;
        private const int sbyte_max_chars = 4;
        private const int byte_max_chars = 3;

        public void Write(int number, int max_chars = int_max_chars) {
            if (this.position + max_chars < this.char_buffer.Length)
                this.itoa_simple(number);
            else
                this.itoa_simple_checked(number);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(short number) => this.Write(CastTo<int, short>(number), short_max_chars);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(sbyte number) => this.Write(CastTo<int, sbyte>(number), sbyte_max_chars);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(uint number, int max_chars = uint_max_chars) {
            if (this.position + max_chars < this.char_buffer.Length)
                this.uitoa_simple(number);
            else
                this.uitoa_simple_checked(number);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ushort number) => this.Write(CastTo<uint, ushort>(number), ushort_max_chars);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte number) => this.Write(CastTo<uint, byte>(number), byte_max_chars);

        public void itoa_simple_helper(int32_t i) {
            if (i <= -10) {
                (int div, i) = X86Base.DivRem(CastTo<uint32_t, int32_t>(i), -1, 10);
                this.itoa_simple_helper(div);
            }

            this.char_buffer.AssignAnyAt(this.position++, '0' - i);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void itoa_simple(int32_t i) {
            if (i < 0)
                this.char_buffer.AssignAnyAt(this.position++, '-');
            else
                i = -i;
            this.itoa_simple_helper(i);
        }

        public void itoa_simple_helper_checked(int32_t i) {
            if (i <= -10) {
                (int div, i) = X86Base.DivRem(CastTo<uint32_t, int32_t>(i), -1, 10);
                this.itoa_simple_helper_checked(div);
            }

            if (this.position == this.char_buffer.Length) this.Flush(this.char_buffer.Length);
            this.char_buffer.AssignAnyAtCarefully(this.position++, '0' - i);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void itoa_simple_checked(int32_t i) {
            if (i < 0)
                this.WriteChar('-');
            else
                i = -i;
            this.itoa_simple_helper_checked(i);
        }

        public void uitoa_simple(uint32_t i) {
            if (i >= 10) {
                (int div, int temp) = X86Base.DivRem(i, 0, 10);
                this.uitoa_simple(CastTo<uint32_t, int32_t>(div));
                i = CastTo<uint32_t, int32_t>(temp);
            }

            this.char_buffer.AssignAnyAt(this.position++, '0' + i);
        }

        public void uitoa_simple_checked(uint32_t i) {
            if (i >= 10) {
                (int div, int temp) = X86Base.DivRem(i, 0, 10);
                this.uitoa_simple_checked(CastTo<uint32_t, int32_t>(div));
                i = CastTo<uint32_t, int32_t>(temp);
            }

            if (this.position == this.char_buffer.Length) this.Flush(this.char_buffer.Length);
            this.char_buffer.AssignAnyAtCarefully(this.position++, '0' + i);
        }

        private void Flush(int bytesCount) {
            int offset = encoding.Preamble.Length * CastTo<byte, bool>(!this.haveWrittenPreamble), length = encoding.GetMaxByteCount(bytesCount);
            byte* bytes = stackalloc byte[length + offset];

            if (!this.haveWrittenPreamble) {
                this.haveWrittenPreamble = true;
                encoding.Preamble.CopyTo(bytes);
            }

            FileStream stream = this.fileStream;
            Tracing.CollectTiming(() => stream.Write(new ReadOnlySpan<byte>(bytes, bytesCount + offset)), "FileWriter.Write");
            this.position = 0;
        }

        public void Dispose() {
            if (this.position != 0) this.Flush(this.position);
            this.fileStream.Dispose();
        }
    }
}
