using DefType = SoulsFormats.PARAMDEF.DefType;
using int32_t = System.Int32;
using MiniCell = SoulsFormats.PARAM.MiniCell;
using uint32_t = System.UInt32;
using SafeFileHandle = Microsoft.Win32.SafeHandles.SafeFileHandle;


namespace Chomp.Util {
    [SkipLocalsInit]
    public unsafe ref struct FileWriter {
        public static readonly UTF8Encoding   encoding = new (false, true);
        public readonly        Encoder        encoder  = encoding.GetEncoder();
        public readonly        SafeFileHandle file_handle;
        public readonly        Span<char>     char_buffer;
        public                 nuint          position    = default;
        public                 long           file_offset = default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FileWriter(string path, FileStreamOptions options, Span<char> buffer) {
            this.file_handle = File.OpenHandle(path, FileMode.Create, options.Access, options.Share, options.Options);
            this.char_buffer = buffer;
            this.Write(encoding.Preamble);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(scoped ReadOnlySpan<byte> bytes) {
            RandomAccess.Write(this.file_handle, bytes, this.file_offset);
            this.file_offset += bytes.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(DefType type, ref MiniCell cell, int array_length) {
            Unsafe.SkipInit(out uint uint_value);
            Unsafe.SkipInit(out int int_value);

            switch (type) {
                case DefType.s8:
                    int_value = cell.SByteValue;
                    goto int_write;
                case DefType.u8:
                    uint_value = cell.ByteValue;
                    goto uint_write;
                case DefType.s16:
                    int_value = cell.ShortValue;
                    goto int_write;
                case DefType.u16:
                    uint_value = cell.UShortValue;
                    goto uint_write;
                case DefType.s32:
                case DefType.b32:
                    int_value = cell.IntValue;
                    goto int_write;
                case DefType.u32:
                    uint_value = cell.UIntValue;
                    goto uint_write;
                case DefType.f32 or DefType.angle32:
                    // maybe in the future I will do version without heap allocation but float to string conversion is so complicated I don't want to deal with it
                    this.Write(cell.FloatValue, double_conversion.FastDtoaMode.FAST_DTOA_SHORTEST_SINGLE);
                    //Span<char> chars = stackalloc char[0x100];
                    // ReSharper disable once RedundantAssignment
                    //bool success = cell.FloatValue.TryFormat(chars, out int charsWritten);
                    //double_conversion.DOUBLE_CONVERSION_ASSERT(success);
                    //this.Write(chars[..charsWritten]);
                    break;
                case DefType.f64:
                    //chars = stackalloc char[0x100];
                    // ReSharper disable once RedundantAssignment
                    //success = cell.DoubleValue.TryFormat(chars, out charsWritten);
                    //double_conversion.DOUBLE_CONVERSION_ASSERT(success);
                    //this.Write(chars[..charsWritten]);
                    this.Write(cell.DoubleValue);
                    break;
                case DefType.fixstr:
                    ref byte           reference = ref Unsafe.As<MiniCell, byte>(ref cell);
                    ReadOnlySpan<byte> inputSpan = MemoryMarshal.CreateReadOnlySpan(ref reference, array_length);
                    Span<char>         buffer    = stackalloc char[Utility.GetMaxCharBytesOfFixedString(inputSpan)];
                    this.Write(Utility.GetFixedString(buffer, inputSpan));
                    break;
                case DefType.fixstrW:
                    this.Write(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<MiniCell, char>(ref cell), array_length));
                    break;
            }

            return;

            uint_write:
            this.Write(uint_value);
            return;

            int_write:
            this.Write(int_value);
        }

        public void Write(scoped ReadOnlySpan<char> chars) {
            nuint charsLength = chars.GetLength();

            if (charsLength <= 4 && charsLength <= this.char_buffer.GetLength() - this.position) {
                scoped ref char buffer = ref this.char_buffer.GetReference(this.position * Utility.SizeOf<char>());
                for (nuint i = 0, l = charsLength * Utility.SizeOf<char>(); i < l; i += Utility.SizeOf<char>())
                    Unsafe.AddByteOffset(ref buffer, i) = chars.GetReference(i);
                //memcpy(AsPointer<char[], char>(buffer, pos * 2 + 16), chars.AsPointer(), (nuint)chars.Length << 1);
            } else {
                nuint length = this.char_buffer.GetLength();
                if (length == this.position) this.Flush();

                for (nuint rem; charsLength > (rem = length - this.position); this.Flush()) {
                    chars.CopyTo(this.char_buffer, this.position.AsInt(), rem.AsInt());
                    //memcpy(AsPointer<char[], char>(buffer, pos * sizeof(char) + 16), chars.AsPointer(), (nuint)(length - pos) << 1);
                    chars       =  chars[rem.AsInt()..];
                    charsLength -= rem;
                }

                //memcpy(AsPointer<char[], char>(buffer, pos * sizeof(char) + 16), chars.AsPointer(), (nuint)chars.Length << 1);
                chars.CopyTo(this.char_buffer, this.position.AsInt(), charsLength.AsInt());
            }

            this.position += charsLength;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSmallString(scoped ReadOnlySpan<char> chars) {
            if (chars.GetLength() > this.char_buffer.GetLength() - this.position) this.Flush();

            scoped ref char buffer = ref this.char_buffer.GetReference(this.position * Utility.SizeOf<char>());
            for (nuint i = 0, l = chars.GetLength() * Utility.SizeOf<char>(); i < l; i += Utility.SizeOf<char>())
                Unsafe.AddByteOffset(ref buffer, i) = chars.GetReference(i);

            this.position += chars.GetLength();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChar(char @char) {
            if (this.position >= this.char_buffer.GetLength()) this.Flush();

            this.char_buffer.AssignAnyAt(this.position++, @char);
        }

        private const int RN = ('\n' << 8 * sizeof(char)) + '\r';

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNewline() {
            if (this.position + 2 > this.char_buffer.GetLength()) this.Flush();

            this.char_buffer.AssignAnyAt(this.position, RN);
            this.position += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteLine(DefType type, ref MiniCell cell, int array_length) {
            this.Write(type, ref cell, array_length);
            this.WriteNewline();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteLine(string text) {
            this.Write(text);
            this.WriteNewline();
        }

        [InlineArray(0x20)]
        private struct local {
            public char @base;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(double number, double_conversion.FastDtoaMode mode = double_conversion.FastDtoaMode.FAST_DTOA_SHORTEST) {
            if (number < 0) {
                this.WriteChar('-');
                number = -number;
            }

            if (number == 0) {
                this.WriteChar('0');
                return;
            }

            local             chars      = new ();
            scoped Span<char> floatChars = chars;
            _ = double_conversion.FastDtoa(number, mode, floatChars, out int lengthWritten, out _);
            this.Write(floatChars[..lengthWritten]);
        }

        private const int int_max_chars    = 11;
        private const int uint_max_chars   = 10;
        private const int short_max_chars  = 6;
        private const int ushort_max_chars = 5;
        private const int sbyte_max_chars  = 4;
        private const int byte_max_chars   = 3;

        public void Write(int number, nuint max_chars = int_max_chars) {
            if (this.position + max_chars < this.char_buffer.GetLength())
                this.itoa_simple(number);
            else
                this.itoa_simple_checked(number);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(uint number, nuint max_chars = uint_max_chars) {
            if (this.position + max_chars < this.char_buffer.GetLength())
                this.uitoa_simple(number);
            else
                this.uitoa_simple_checked(number);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void itoa_simple(int32_t i) {
            if (i < 0)
                this.char_buffer.AssignAnyAt(this.position++, '-');
            else
                i = -i;

            while (i <= -10) {
                (i, int rem) = System.Runtime.Intrinsics.X86.X86Base.DivRem(i.AsUint(), -1, 10);
                this.char_buffer.AssignAnyAt(this.position++, (ushort)('0' - rem).AsUint());
            }

            this.char_buffer.AssignAnyAt(this.position++, (ushort)('0' - i).AsUint());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void itoa_simple_checked(int32_t i) {
            if (i < 0)
                this.WriteChar('-');
            else
                i = -i;


            while (i <= -10) {
                (i, int rem) = System.Runtime.Intrinsics.X86.X86Base.DivRem(i.AsUint(), -1, 10);

                if (this.position == this.char_buffer.GetLength()) this.Flush();

                this.char_buffer.AssignAnyAt(this.position++, (ushort)('0' - rem).AsUint());
            }

            if (this.position == this.char_buffer.GetLength()) this.Flush();

            this.char_buffer.AssignAnyAt(this.position++, (ushort)('0' - i).AsUint());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void uitoa_simple(uint32_t i) {
            while (i >= 10) {
                (i, uint rem) = Unsafe.BitCast<(int, int), (uint, uint)>(System.Runtime.Intrinsics.X86.X86Base.DivRem(i, 0, 10));
                this.char_buffer.AssignAnyAt(this.position++, (ushort)('0' + rem));
            }

            this.char_buffer.AssignAnyAt(this.position++, (ushort)('0' + i));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void uitoa_simple_checked(uint32_t i) {
            while (i >= 10) {
                (i, uint rem) = Unsafe.BitCast<(int, int), (uint, uint)>(System.Runtime.Intrinsics.X86.X86Base.DivRem(i, 0, 10));
                if (this.position == this.char_buffer.GetLength()) this.Flush();
                this.char_buffer.AssignAnyAt(this.position++, (ushort)('0' + rem));
            }

            if (this.position == this.char_buffer.GetLength()) this.Flush();

            this.char_buffer.AssignAnyAt(this.position++, (ushort)('0' + i));
        }

        public void Flush(bool flushEncoder = false) {
            int   length = encoding.GetMaxByteCount(this.position.AsInt());
            byte* bytes  = stackalloc byte[length];

            Encoder encoder     = this.encoder;
            var     char_buffer = (char*)Unsafe.AsPointer(ref this.char_buffer.GetReference());
            NamedTimings.FileWriterFlushEncodeNamedTiming.Start();
            int encodedLength = encoder.GetBytes(char_buffer, this.position.AsInt(), bytes, length, flushEncoder);
            NamedTimings.FileWriterFlushEncodeNamedTiming.Stop();
            NamedTimings.FileWriterFlushFileWriteNamedTiming.Start();
            this.Write(new ReadOnlySpan<byte>(bytes, encodedLength));
            NamedTimings.FileWriterFlushFileWriteNamedTiming.Stop();
            this.position = 0;
        }

        public void Dispose() {
            if (this.position != 0) this.Flush(true);

            this.file_handle.Close();
        }
    }
}
