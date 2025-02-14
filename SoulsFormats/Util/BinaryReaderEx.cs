﻿namespace SoulsFormats.Util {
    /// <summary>
    ///     An extended reader for binary data supporting big and little endianness, value assertions, and arrays.
    /// </summary>
    public class BinaryReaderEx {
        private readonly BinaryReader br;
        private readonly Stack<long>  steps;

        /// <summary>
        ///     The underlying stream.
        /// </summary>
        public readonly Stream Stream;

        /// <summary>
        ///     Interpret values as big-endian if set, or little-endian if not.
        /// </summary>
        public bool BigEndian;

        /// <summary>
        ///     Varints are read as Int64 if set, otherwise Int32.
        /// </summary>
        public bool VarintLong;

        /// <summary>
        ///     Initializes a new BinaryReaderEx reading from the specified byte array.
        /// </summary>
        public BinaryReaderEx(bool bigEndian, byte[] input) : this(bigEndian, new MemoryStream(input)) { }

        /// <summary>
        ///     Initializes a new BinaryReaderEx reading from the specified stream.
        /// </summary>
        public BinaryReaderEx(bool bigEndian, Stream stream) {
            this.BigEndian = bigEndian;
            this.steps     = new Stack<long>(0x20);
            this.Stream    = stream;
            this.br        = new BinaryReader(stream);
        }

        /// <summary>
        ///     Current size of varints in bytes.
        /// </summary>
        public int VarintSize => this.VarintLong ? 8 : 4;

        /// <summary>
        ///     The current position of the stream.
        /// </summary>
        public long Position {
            get => this.Stream.Position;
            set => this.Stream.Position = value;
        }

        /// <summary>
        ///     The length of the stream.
        /// </summary>
        public long Length => this.Stream.Length;

        /// <summary>
        ///     Reads length bytes and returns them in reversed order.
        /// </summary>
        private byte[] ReadReversedBytes(int length) {
            byte[] bytes = this.ReadBytes(length);
            Array.Reverse(bytes);
            return bytes;
        }

        /// <summary>
        ///     Reads a value from the specified offset using the given function, returning the stream to its original position
        ///     afterwards.
        /// </summary>
        private T GetValue<T>(Func<T> readValue, long offset) {
            this.StepIn(offset);
            T result = readValue();
            this.StepOut();
            return result;
        }

        /// <summary>
        ///     Reads an array of values from the specified offset using the given function, returning the stream to its original
        ///     position afterwards.
        /// </summary>
        private T[] GetValues<T>(Func<int, T[]> readValues, long offset, int count) {
            this.StepIn(offset);
            T[] result = readValues(count);
            this.StepOut();
            return result;
        }

        /// <summary>
        ///     Compares a value to a list of options, returning it if found or excepting if not.
        /// </summary>
        private T AssertValue<T>(T value, string typeName, string valueFormat, ReadOnlySpan<T> options) where T : IEquatable<T> {
            int indexOf = options.IndexOf(value);

            if (indexOf >= 0) return options[indexOf];

            throw new
                InvalidDataException($"Read {typeName}: {string.Format(valueFormat, value)} | Expected: {string.Join(", ", options.ToArray().Select(o => string.Format(valueFormat, o)))} | Ending position: 0x{this.Position:X}");
        }

        /// <summary>
        ///     Store the current position of the stream on a stack, then move to the specified offset.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StepIn(long offset) {
            this.steps.Push(this.Stream.Position);
            this.Stream.Position = offset;
        }

        /// <summary>
        ///     Restore the previous position of the stream from a stack.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StepOut() {
            if (this.steps.Count == 0) throw new InvalidOperationException("Reader is already stepped all the way out.");

            this.Stream.Position = this.steps.Pop();
        }

        /// <summary>
        ///     Advances the stream position until it meets the specified alignment.
        /// </summary>
        public void Pad(int align) {
            if (this.Stream.Position % align > 0) this.Stream.Position += align - this.Stream.Position % align;
        }

        /// <summary>
        ///     Advances the stream position until it meets the specified alignment relative to the given starting position.
        /// </summary>
        public void PadRelative(long start, int align) {
            long relPos                                  = this.Stream.Position - start;
            if (relPos % align > 0) this.Stream.Position += align - relPos % align;
        }

        /// <summary>
        ///     Advances the stream position by count bytes.
        /// </summary>
        public void Skip(int count) => this.Stream.Position += count;

        #region Boolean

        /// <summary>
        ///     Reads a one-byte boolean value.
        /// </summary>
        public bool ReadBoolean() {
            var b = this.br.ReadValueTypeEffectively<byte>();
            // BinaryReader.ReadBoolean accepts any non-zero value as true, which I don't want.
            return b > 1 ? throw new InvalidDataException($"ReadBoolean encountered non-boolean value: 0x{b:X2}") : CastTo<bool, byte>(b);
        }

        /// <summary>
        ///     Reads an array of one-byte boolean values.
        /// </summary>
        public bool[] ReadBooleans(int count) {
            var result                                = new bool[count];
            for (var i = 0; i < count; i++) result[i] = this.ReadBoolean();

            return result;
        }

        /// <summary>
        ///     Reads a one-byte boolean value from the specified offset without advancing the stream.
        /// </summary>
        public bool GetBoolean(long offset) => this.GetValue(this.ReadBoolean, offset);

        /// <summary>
        ///     Reads an array of one-byte boolean values from the specified offset without advancing the stream.
        /// </summary>
        public bool[] GetBooleans(long offset, int count) => this.GetValues(this.ReadBooleans, offset, count);

        /// <summary>
        ///     Reads a one-byte boolean value and throws an exception if it does not match the specified option.
        /// </summary>
        public bool AssertBoolean(bool option) => this.AssertValue(this.ReadBoolean(), "Boolean", "{0}", new[] { option });

        #endregion

        #region SByte

        /// <summary>
        ///     Reads a one-byte signed integer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte ReadSByte() => this.br.ReadValueTypeEffectively<sbyte>();

        /// <summary>
        ///     Reads an array of one-byte signed integers.
        /// </summary>
        public sbyte[] ReadSBytes(int count) {
            var result                                = new sbyte[count];
            for (var i = 0; i < count; i++) result[i] = this.ReadSByte();

            return result;
        }

        /// <summary>
        ///     Reads a one-byte signed integer from the specified offset without advancing the stream.
        /// </summary>
        public sbyte GetSByte(long offset) => this.GetValue(this.ReadSByte, offset);

        /// <summary>
        ///     Reads an array of one-byte signed integers from the specified offset without advancing the stream.
        /// </summary>
        public sbyte[] GetSBytes(long offset, int count) => this.GetValues(this.ReadSBytes, offset, count);

        /// <summary>
        ///     Reads a one-byte signed integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public sbyte AssertSByte(params sbyte[] options) => this.AssertValue(this.ReadSByte(), "SByte", "0x{0:X}", options);

        #endregion

        #region Byte

        /// <summary>
        ///     Reads a one-byte unsigned integer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte() => this.br.ReadValueTypeEffectively<byte>();

        /// <summary>
        ///     Reads an array of one-byte unsigned integers.
        /// </summary>
        public byte[] ReadBytes(int count) {
            byte[] res = GC.AllocateUninitializedArray<byte>(count);
            this.br.ReadExactly(res);
            return res;
        }

        /// <summary>
        ///     Reads the specified number of bytes from the stream into the buffer starting at the specified index.
        /// </summary>
        public void ReadBytes(byte[] buffer, int index, int count) {
            int read = this.br.Read(buffer, index, count);
            if (read != count) throw new EndOfStreamException("Remaining size of stream was smaller than requested number of bytes.");
        }

        /// <summary>
        ///     Reads the specified number of bytes from the stream into the buffer starting at the specified index.
        /// </summary>
        public void ReadSpanBytes(Span<byte> bytes) => this.br.ReadExactly(bytes);

        /// <summary>
        ///     Reads a one-byte unsigned integer from the specified offset without advancing the stream.
        /// </summary>
        public byte GetByte(long offset) => this.GetValue(this.ReadByte, offset);

        /// <summary>
        ///     Reads an array of one-byte unsigned integers from the specified offset without advancing the stream.
        /// </summary>
        public byte[] GetBytes(long offset, int count) {
            this.StepIn(offset);
            byte[] result = this.ReadBytes(count);
            this.StepOut();
            return result;
        }

        /// <summary>
        ///     Reads the specified number of bytes from the offset into the buffer starting at the specified index without
        ///     advancing the stream.
        /// </summary>
        public void GetBytes(long offset, byte[] buffer, int index, int count) {
            this.StepIn(offset);
            this.ReadBytes(buffer, index, count);
            this.StepOut();
        }

        /// <summary>
        ///     Reads a one-byte unsigned integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public byte AssertByte(params byte[] options) => this.AssertValue(this.ReadByte(), "Byte", "0x{0:X}", options);

        #endregion

        #region Int16

        /// <summary>
        ///     Reads a two-byte signed integer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadInt16() {
            var result                 = this.br.ReadValueTypeEffectively<short>();
            if (this.BigEndian) result = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(result);
            return result;
        }

        /// <summary>
        ///     Reads an array of two-byte signed integers.
        /// </summary>
        public short[] ReadInt16s(int count) {
            var result                                = new short[count];
            for (var i = 0; i < count; i++) result[i] = this.ReadInt16();

            return result;
        }

        /// <summary>
        ///     Reads a two-byte signed integer from the specified offset without advancing the stream.
        /// </summary>
        public short GetInt16(long offset) => this.GetValue(this.ReadInt16, offset);

        /// <summary>
        ///     Reads an array of two-byte signed integers from the specified offset without advancing the stream.
        /// </summary>
        public short[] GetInt16s(long offset, int count) => this.GetValues(this.ReadInt16s, offset, count);

        /// <summary>
        ///     Reads a two-byte signed integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public short AssertInt16(params short[] options) => this.AssertValue(this.ReadInt16(), "Int16", "0x{0:X}", options);

        #endregion

        #region UInt16

        /// <summary>
        ///     Reads a two-byte unsigned integer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUInt16() {
            var result                 = this.br.ReadValueTypeEffectively<ushort>();
            if (this.BigEndian) result = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(result);
            return result;
        }

        /// <summary>
        ///     Reads an array of two-byte unsigned integers.
        /// </summary>
        public ushort[] ReadUInt16s(int count) {
            var result                                = new ushort[count];
            for (var i = 0; i < count; i++) result[i] = this.ReadUInt16();

            return result;
        }

        /// <summary>
        ///     Reads a two-byte unsigned integer from the specified position without advancing the stream.
        /// </summary>
        public ushort GetUInt16(long offset) => this.GetValue(this.ReadUInt16, offset);

        /// <summary>
        ///     Reads an array of two-byte unsigned integers from the specified position without advancing the stream.
        /// </summary>
        public ushort[] GetUInt16s(long offset, int count) => this.GetValues(this.ReadUInt16s, offset, count);

        /// <summary>
        ///     Reads a two-byte unsigned integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public ushort AssertUInt16(params ushort[] options) => this.AssertValue(this.ReadUInt16(), "UInt16", "0x{0:X}", options);

        #endregion

        #region Int32

        /// <summary>
        ///     Reads a four-byte signed integer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt32() {
            var result                 = this.br.ReadValueTypeEffectively<int>();
            if (this.BigEndian) result = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(result);
            return result;
        }

        /// <summary>
        ///     Reads an array of four-byte signed integers.
        /// </summary>
        public int[] ReadInt32s(int count) {
            var result                                = new int[count];
            for (var i = 0; i < count; i++) result[i] = this.ReadInt32();

            return result;
        }

        /// <summary>
        ///     Reads a four-byte signed integer from the specified position without advancing the stream.
        /// </summary>
        public int GetInt32(long offset) => this.GetValue(this.ReadInt32, offset);

        /// <summary>
        ///     Reads an array of four-byte signed integers from the specified position without advancing the stream.
        /// </summary>
        public int[] GetInt32s(long offset, int count) => this.GetValues(this.ReadInt32s, offset, count);

        /// <summary>
        ///     Reads a four-byte signed integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public int AssertInt32(params int[] options) => this.AssertValue(this.ReadInt32(), "Int32", "0x{0:X}", options);

        #endregion

        #region UInt32

        /// <summary>
        ///     Reads a four-byte unsigned integer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt32() {
            var result                 = this.br.ReadValueTypeEffectively<uint>();
            if (this.BigEndian) result = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(result);
            return result;
        }

        /// <summary>
        ///     Reads an array of four-byte unsigned integers.
        /// </summary>
        public uint[] ReadUInt32s(int count) {
            var result                                = new uint[count];
            for (var i = 0; i < count; i++) result[i] = this.ReadUInt32();

            return result;
        }

        /// <summary>
        ///     Reads a four-byte unsigned integer from the specified position without advancing the stream.
        /// </summary>
        public uint GetUInt32(long offset) => this.GetValue(this.ReadUInt32, offset);

        /// <summary>
        ///     Reads an array of four-byte unsigned integers from the specified position without advancing the stream.
        /// </summary>
        public uint[] GetUInt32s(long offset, int count) => this.GetValues(this.ReadUInt32s, offset, count);

        /// <summary>
        ///     Reads a four-byte unsigned integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public uint AssertUInt32(params uint[] options) => this.AssertValue(this.ReadUInt32(), "UInt32", "0x{0:X}", options);

        #endregion

        #region Int64

        /// <summary>
        ///     Reads an eight-byte signed integer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadInt64() {
            var result                 = this.br.ReadValueTypeEffectively<long>();
            if (this.BigEndian) result = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(result);
            return result;
        }

        /// <summary>
        ///     Reads an array of eight-byte signed integers.
        /// </summary>
        public long[] ReadInt64s(int count) {
            var result                                = new long[count];
            for (var i = 0; i < count; i++) result[i] = this.ReadInt64();

            return result;
        }

        /// <summary>
        ///     Reads an eight-byte signed integer from the specified position without advancing the stream.
        /// </summary>
        public long GetInt64(long offset) => this.GetValue(this.ReadInt64, offset);

        /// <summary>
        ///     Reads an array eight-byte signed integers from the specified position without advancing the stream.
        /// </summary>
        public long[] GetInt64s(long offset, int count) => this.GetValues(this.ReadInt64s, offset, count);

        /// <summary>
        ///     Reads an eight-byte signed integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public long AssertInt64(params long[] options) => this.AssertValue(this.ReadInt64(), "Int64", "0x{0:X}", options);

        #endregion

        #region UInt64

        /// <summary>
        ///     Reads an eight-byte unsigned integer.
        /// </summary>
        public ulong ReadUInt64() {
            var result                 = this.br.ReadValueTypeEffectively<ulong>();
            if (this.BigEndian) result = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(result);
            return result;
        }

        /// <summary>
        ///     Reads an array of eight-byte unsigned integers.
        /// </summary>
        public ulong[] ReadUInt64s(int count) {
            var result                                = new ulong[count];
            for (var i = 0; i < count; i++) result[i] = this.ReadUInt64();

            return result;
        }

        /// <summary>
        ///     Reads an eight-byte unsigned integer from the specified position without advancing the stream.
        /// </summary>
        public ulong GetUInt64(long offset) => this.GetValue(this.ReadUInt64, offset);

        /// <summary>
        ///     Reads an array of eight-byte unsigned integers from the specified position without advancing the stream.
        /// </summary>
        public ulong[] GetUInt64s(long offset, int count) => this.GetValues(this.ReadUInt64s, offset, count);

        /// <summary>
        ///     Reads an eight-byte unsigned integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public ulong AssertUInt64(params ulong[] options) => this.AssertValue(this.ReadUInt64(), "UInt64", "0x{0:X}", options);

        #endregion

        #region Varint

        /// <summary>
        ///     Reads either a four or eight-byte signed integer depending on VarintLong.
        /// </summary>
        public long ReadVarint() => this.VarintLong ? this.ReadInt64() : this.ReadInt32();

        /// <summary>
        ///     Reads an array of either four or eight-byte signed integers depending on VarintLong.
        /// </summary>
        public long[] ReadVarints(int count) {
            var result                                = new long[count];
            for (var i = 0; i < count; i++) result[i] = this.VarintLong ? this.ReadInt64() : this.ReadInt32();
            return result;
        }

        /// <summary>
        ///     Reads either a four or eight-byte signed integer depending on VarintLong from the specified position without
        ///     advancing the stream.
        /// </summary>
        public long GetVarint(long offset) => this.VarintLong ? this.GetInt64(offset) : this.GetInt32(offset);

        /// <summary>
        ///     Reads an array of either four or eight-byte signed integers depending on VarintLong from the specified position
        ///     without advancing the stream.
        /// </summary>
        public long[] GetVarints(long offset, int count) => this.GetValues(this.ReadVarints, offset, count);

        /// <summary>
        ///     Reads either a four or eight-byte signed integer depending on VarintLong and throws an exception if it does not
        ///     match any of the specified options.
        /// </summary>
        public long AssertVarint(params long[] options) =>
            this.AssertValue(this.ReadVarint(), this.VarintLong ? "Varint64" : "Varint32", "0x{0:X}", options);

        #endregion

        #region Single

        /// <summary>
        ///     Reads a four-byte floating point number.
        /// </summary>
        public float ReadSingle() {
            var result                 = this.br.ReadValueTypeEffectively<float>();
            if (this.BigEndian) result = CastTo<float, uint>(System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(CastTo<uint, float>(result)));
            return result;
        }

        /// <summary>
        ///     Reads an array of four-byte floating point numbers.
        /// </summary>
        public float[] ReadSingles(int count) {
            var result                                = new float[count];
            for (var i = 0; i < count; i++) result[i] = this.ReadSingle();

            return result;
        }

        /// <summary>
        ///     Reads a four-byte floating point number from the specified position without advancing the stream.
        /// </summary>
        public float GetSingle(long offset) => this.GetValue(this.ReadSingle, offset);

        /// <summary>
        ///     Reads an array of four-byte floating point numbers from the specified position without advancing the stream.
        /// </summary>
        public float[] GetSingles(long offset, int count) => this.GetValues(this.ReadSingles, offset, count);

        /// <summary>
        ///     Reads a four-byte floating point number and throws an exception if it does not match any of the specified options.
        /// </summary>
        public float AssertSingle(params float[] options) => this.AssertValue(this.ReadSingle(), "Single", "{0}", options);

        #endregion

        #region Double

        /// <summary>
        ///     Reads an eight-byte floating point number.
        /// </summary>
        // avoid bloating call-sites
        public double ReadDouble() {
            var result = this.br.ReadValueTypeEffectively<double>();
            if (this.BigEndian)
                result = CastTo<double, ulong>(System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(CastTo<ulong, double>(result)));
            return result;
        }

        /// <summary>
        ///     Reads an array of eight-byte floating point numbers.
        /// </summary>
        public double[] ReadDoubles(int count) {
            var result                                = new double[count];
            for (var i = 0; i < count; i++) result[i] = this.ReadDouble();

            return result;
        }

        /// <summary>
        ///     Reads an eight-byte floating point number from the specified position without advancing the stream.
        /// </summary>
        public double GetDouble(long offset) => this.GetValue(this.ReadDouble, offset);

        /// <summary>
        ///     Reads an array of eight-byte floating point numbers from the specified position without advancing the stream.
        /// </summary>
        public double[] GetDoubles(long offset, int count) => this.GetValues(this.ReadDoubles, offset, count);

        /// <summary>
        ///     Reads an eight-byte floating point number and throws an exception if it does not match any of the specified
        ///     options.
        /// </summary>
        public double AssertDouble(params double[] options) => this.AssertValue(this.ReadDouble(), "Double", "{0}", options);

        #endregion

        #region Enum

        private static TEnum ReadEnum<TEnum, TValue>(Func<TValue> readValue) where TEnum : struct, Enum where TValue : struct, IFormattable {
            TValue value = readValue();
            return Enum.IsDefined(CastTo<TEnum, TValue>(value))
                ? CastTo<TEnum, TValue>(value)
                : throw new InvalidDataException($"Read {typeof(TValue).Name} not present in enum: 0x{value:X}");
        }

        /// <summary>
        ///     Reads a one-byte value as the specified enum, throwing an exception if not present.
        /// </summary>
        public TEnum ReadEnum8<TEnum>() where TEnum : struct, Enum => ReadEnum<TEnum, byte>(this.ReadByte);


        /// <summary>
        ///     Reads a one-byte enum from the specified position without advancing the stream.
        /// </summary>
        public TEnum GetEnum8<TEnum>(long position) where TEnum : struct, Enum {
            this.StepIn(position);
            var result = this.ReadEnum8<TEnum>();
            this.StepOut();
            return result;
        }

        /// <summary>
        ///     Reads a two-byte value as the specified enum, throwing an exception if not present.
        /// </summary>
        public TEnum ReadEnum16<TEnum>() where TEnum : struct, Enum => ReadEnum<TEnum, ushort>(this.ReadUInt16);

        /// <summary>
        ///     Reads a two-byte enum from the specified position without advancing the stream.
        /// </summary>
        public TEnum GetEnum16<TEnum>(long position) where TEnum : struct, Enum {
            this.StepIn(position);
            var result = this.ReadEnum16<TEnum>();
            this.StepOut();
            return result;
        }

        /// <summary>
        ///     Reads a four-byte value as the specified enum, throwing an exception if not present.
        /// </summary>
        public TEnum ReadEnum32<TEnum>() where TEnum : struct, Enum => ReadEnum<TEnum, uint>(this.ReadUInt32);

        /// <summary>
        ///     Reads a four-byte enum from the specified position without advancing the stream.
        /// </summary>
        public TEnum GetEnum32<TEnum>(long position) where TEnum : struct, Enum {
            this.StepIn(position);
            var result = this.ReadEnum32<TEnum>();
            this.StepOut();
            return result;
        }

        /// <summary>
        ///     Reads an eight-byte value as the specified enum, throwing an exception if not present.
        /// </summary>
        public TEnum ReadEnum64<TEnum>() where TEnum : struct, Enum => ReadEnum<TEnum, ulong>(this.ReadUInt64);

        /// <summary>
        ///     Reads an eight-byte enum from the specified position without advancing the stream.
        /// </summary>
        public TEnum GetEnum64<TEnum>(long position) where TEnum : struct, Enum {
            this.StepIn(position);
            var result = this.ReadEnum64<TEnum>();
            this.StepOut();
            return result;
        }

        #endregion

        #region String

        /// <summary>
        ///     Reads the specified number of bytes and interprets them according to the specified encoding.
        /// </summary>
        private string ReadChars(System.Text.Encoding encoding, int length) {
            byte[] bytes = this.ReadBytes(length);
            return encoding.GetString(bytes);
        }

        /// <summary>
        ///     Reads bytes until a single-byte null terminator is found, then interprets them according to the specified encoding.
        /// </summary>
        private string ReadCharsTerminated(System.Text.Encoding encoding) {
            var bytes = new List<byte>();

            byte b;

            while ((b = this.ReadByte()) != 0) bytes.Add(b);

            return encoding.GetString(bytes.ToArray());
        }

        /// <summary>
        ///     Reads a null-terminated ASCII string.
        /// </summary>
        public string ReadASCII() => this.ReadCharsTerminated(SFEncoding.ASCII);

        /// <summary>
        ///     Reads an ASCII string with the specified length in bytes.
        /// </summary>
        public string ReadASCII(int length) => this.ReadChars(SFEncoding.ASCII, length);

        /// <summary>
        ///     Reads a null-terminated ASCII string from the specified position without advancing the stream.
        /// </summary>
        public string GetASCII(long offset) {
            this.StepIn(offset);
            string result = this.ReadASCII();
            this.StepOut();
            return result;
        }

        /// <summary>
        ///     Reads an ASCII string with the specified length in bytes from the specified position without advancing the stream.
        /// </summary>
        public string GetASCII(long offset, int length) {
            this.StepIn(offset);
            string result = this.ReadASCII(length);
            this.StepOut();
            return result;
        }

        /// <summary>
        ///     Reads as many ASCII characters as are in the specified value and throws an exception if they do not match.
        /// </summary>
        public string AssertASCII(params string[] values) {
            string s = this.ReadASCII(values[0].Length);

            return values.Contains(s)
                ? s
                : throw new InvalidDataException($"Read ASCII: {s} | Expected ASCII: {string.Join(", ", values)}");
        }

        /// <summary>
        ///     Reads a null-terminated Shift JIS string.
        /// </summary>
        public string ReadShiftJIS() => this.ReadCharsTerminated(SFEncoding.ShiftJIS);

        /// <summary>
        ///     Reads a Shift JIS string with the specified length in bytes.
        /// </summary>
        public string ReadShiftJIS(int length) => this.ReadChars(SFEncoding.ShiftJIS, length);

        /// <summary>
        ///     Reads a null-terminated Shift JIS string from the specified position without advancing the stream.
        /// </summary>
        public string GetShiftJIS(long offset) {
            this.StepIn(offset);
            string result = this.ReadShiftJIS();
            this.StepOut();
            return result;
        }

        /// <summary>
        ///     Reads a Shift JIS string with the specified length in bytes from the specified position without advancing the
        ///     stream.
        /// </summary>
        public string GetShiftJIS(long offset, int length) {
            this.StepIn(offset);
            string result = this.ReadShiftJIS(length);
            this.StepOut();
            return result;
        }

        /// <summary>
        ///     Reads a null-terminated UTF-16 string.
        /// </summary>
        public unsafe string ReadUTF16() {
            if (this.Stream is MemoryStream memoryStream && !this.BigEndian) {
                ref byte start = ref Unsafe
                    .AddByteOffset(ref MemoryMarshal.GetArrayDataReference(memoryStream.GetInternalBuffer()),
                                   (nint)memoryStream.Position + memoryStream.GetInternalOrigin());

                fixed (byte* _ = MemoryMarshal.CreateReadOnlySpan(ref start, (int)memoryStream.Length)) {
                    char* pointerStart = (char*)Unsafe.AsPointer(ref start);
                    var   length       = (int)(memoryStream.Length >> 1);
                    int index =
                        CharPointerIndexOfSingle(pointerStart, length, '\0');
                    memoryStream.Position += index < 0 ? length : index + 1;
                    return new string(pointerStart, 0, index < 0 ? length : index);
                }
            }

            var  bytes = new List<char>();
            char @char;
            while ((@char = (char)this.ReadUInt16()) != 0) bytes.Add(@char);
            var span = new ReadOnlySpan<byte>(bytes.AsPointer<char, byte>(), bytes.Count << 1);
            return this.BigEndian ? SFEncoding.UTF16BE.GetString(span) : SFEncoding.UTF16.GetString(span);
        }

        /// <summary>
        ///     Reads a null-terminated UTF-16 string from the specified position without advancing the stream.
        /// </summary>
        public string GetUTF16(long offset) {
            this.StepIn(offset);
            string result = this.ReadUTF16();
            this.StepOut();
            return result;
        }

        /// <summary>
        ///     Reads a null-terminated Shift JIS string in a fixed-size field.
        /// </summary>
        public unsafe string ReadFixStr(int size) {
            byte* bytes = stackalloc byte[size];
            this.br.ReadExactly(new Span<byte>(bytes, size));
            int length             = BytePointerIndexOfSingle(bytes, size, 0);
            if (length < 0) length = size;
            return SFEncoding.ShiftJIS.GetString(bytes, length);
        }

        /// <summary>
        ///     Reads a null-terminated UTF-16 string in a fixed-size field.
        /// </summary>
        public unsafe string ReadFixStrW(int size) {
            byte* bytes = stackalloc byte[size];
            size = this.br.Read(new Span<byte>(bytes, size));
            if ((size & 1) != 0) // Really, UTF16 strings which have odd length?
                throw new InvalidDataException("Cannot read fixed-length wide string with odd length");
            int length             = CharPointerIndexOfSingle((char*)bytes, size >> 1, '\0');
            if (length < 0) length = size;

            return this.BigEndian ? SFEncoding.UTF16BE.GetString(bytes, length) : SFEncoding.UTF16.GetString(bytes, length);
        }

        #endregion

        #region Other

        /// <summary>
        ///     Reads a vector of two four-byte floating point numbers.
        /// </summary>
        public Vector2 ReadVector2() {
            float x = this.ReadSingle();
            float y = this.ReadSingle();
            return new Vector2(x, y);
        }

        /// <summary>
        ///     Reads a vector of three four-byte floating point numbers.
        /// </summary>
        public Vector3 ReadVector3() {
            float x = this.ReadSingle();
            float y = this.ReadSingle();
            float z = this.ReadSingle();
            return new Vector3(x, y, z);
        }

        /// <summary>
        ///     Reads a vector of four four-byte floating point numbers.
        /// </summary>
        public Vector4 ReadVector4() {
            float x = this.ReadSingle();
            float y = this.ReadSingle();
            float z = this.ReadSingle();
            float w = this.ReadSingle();
            return new Vector4(x, y, z, w);
        }

        /// <summary>
        ///     Read length number of bytes and assert that they all match the given value.
        /// </summary>
        public void AssertPattern(int length, byte pattern) {
            Span<byte> bytes = stackalloc byte[length];
            this.br.ReadExactly(bytes);
            int index = bytes.IndexOfAnyExcept(pattern);
            if (index >= 0) throw new InvalidDataException($"Expected {length} 0x{pattern:X2}, got {bytes[index]:X2} at position {index}");
        }

        /// <summary>
        ///     Reads a 4-byte color in ARGB order.
        /// </summary>
        public System.Drawing.Color ReadARGB() {
            byte a = this.br.ReadByte();
            byte r = this.br.ReadByte();
            byte g = this.br.ReadByte();
            byte b = this.br.ReadByte();
            return System.Drawing.Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        ///     Reads a 4-byte color in ABGR order.
        /// </summary>
        public System.Drawing.Color ReadABGR() {
            byte a = this.br.ReadByte();
            byte b = this.br.ReadByte();
            byte g = this.br.ReadByte();
            byte r = this.br.ReadByte();
            return System.Drawing.Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        ///     Reads a 4-byte color in RGBA order.
        /// </summary>
        public System.Drawing.Color ReadRGBA() {
            byte r = this.br.ReadByte();
            byte g = this.br.ReadByte();
            byte b = this.br.ReadByte();
            byte a = this.br.ReadByte();
            return System.Drawing.Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        ///     Reads a 4-byte color in BGRA order.
        /// </summary>
        public System.Drawing.Color ReadBGRA() {
            byte b = this.br.ReadByte();
            byte g = this.br.ReadByte();
            byte r = this.br.ReadByte();
            byte a = this.br.ReadByte();
            return System.Drawing.Color.FromArgb(a, r, g, b);
        }

        #endregion
    }
}
