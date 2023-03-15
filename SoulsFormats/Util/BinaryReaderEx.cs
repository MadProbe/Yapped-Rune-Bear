using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace SoulsFormats.Util {
    /// <summary>
    /// An extended reader for binary data supporting big and little endianness, value assertions, and arrays.
    /// </summary>
    public class BinaryReaderEx {
        private readonly BinaryReader br;
        private readonly Stack<long> steps;

        /// <summary>
        /// Interpret values as big-endian if set, or little-endian if not.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// Varints are read as Int64 if set, otherwise Int32.
        /// </summary>
        public bool VarintLong { get; set; }

        /// <summary>
        /// Current size of varints in bytes.
        /// </summary>
        public int VarintSize => this.VarintLong ? 8 : 4;

        /// <summary>
        /// The underlying stream.
        /// </summary>
        public Stream Stream { get; }

        /// <summary>
        /// The current position of the stream.
        /// </summary>
        public long Position {
            get => this.Stream.Position;
            set => this.Stream.Position = value;
        }

        /// <summary>
        /// The length of the stream.
        /// </summary>
        public long Length => this.Stream.Length;

        /// <summary>
        /// Initializes a new BinaryReaderEx reading from the specified byte array.
        /// </summary>
        public BinaryReaderEx(bool bigEndian, byte[] input) : this(bigEndian, new MemoryStream(input)) { }

        /// <summary>
        /// Initializes a new BinaryReaderEx reading from the specified stream.
        /// </summary>
        public BinaryReaderEx(bool bigEndian, Stream stream) {
            this.BigEndian = bigEndian;
            this.steps = new Stack<long>();
            this.Stream = stream;
            this.br = new BinaryReader(stream);
        }

        /// <summary>
        /// Reads length bytes and returns them in reversed order.
        /// </summary>
        private byte[] ReadReversedBytes(int length) {
            byte[] bytes = this.ReadBytes(length);
            Array.Reverse(bytes);
            return bytes;
        }

        /// <summary>
        /// Reads a value from the specified offset using the given function, returning the stream to its original position afterwards.
        /// </summary>
        private T GetValue<T>(Func<T> readValue, long offset) {
            this.StepIn(offset);
            T result = readValue();
            this.StepOut();
            return result;
        }

        /// <summary>
        /// Reads an array of values from the specified offset using the given function, returning the stream to its original position afterwards.
        /// </summary>
        private T[] GetValues<T>(Func<int, T[]> readValues, long offset, int count) {
            this.StepIn(offset);
            T[] result = readValues(count);
            this.StepOut();
            return result;
        }

        /// <summary>
        /// Compares a value to a list of options, returning it if found or excepting if not.
        /// </summary>
        private T AssertValue<T>(T value, string typeName, string valueFormat, T[] options) where T : IEquatable<T> {
            for (int i = 0, length = options.Length; i < length; i++) {
                if (value.Equals(options[i])) {
                    return value;
                }
            }

            string strValue = string.Format(valueFormat, value);
            string strOptions = string.Join(", ", options.Select(o => string.Format(valueFormat, o)));
            throw new InvalidDataException($"Read {typeName}: {strValue} | Expected: {strOptions} | Ending position: 0x{this.Position:X}");
        }

        /// <summary>
        /// Store the current position of the stream on a stack, then move to the specified offset.
        /// </summary>
        public void StepIn(long offset) {
            this.steps.Push(this.Stream.Position);
            this.Stream.Position = offset;
        }

        /// <summary>
        /// Restore the previous position of the stream from a stack.
        /// </summary>
        public void StepOut() {
            if (this.steps.Count == 0) {
                throw new InvalidOperationException("Reader is already stepped all the way out.");
            }

            this.Stream.Position = this.steps.Pop();
        }

        /// <summary>
        /// Advances the stream position until it meets the specified alignment.
        /// </summary>
        public void Pad(int align) {
            if (this.Stream.Position % align > 0) {
                this.Stream.Position += align - this.Stream.Position % align;
            }
        }

        /// <summary>
        /// Advances the stream position until it meets the specified alignment relative to the given starting position.
        /// </summary>
        public void PadRelative(long start, int align) {
            long relPos = this.Stream.Position - start;
            if (relPos % align > 0) {
                this.Stream.Position += align - relPos % align;
            }
        }

        /// <summary>
        /// Advances the stream position by count bytes.
        /// </summary>
        public void Skip(int count) => this.Stream.Position += count;

        #region Boolean
        /// <summary>
        /// Reads a one-byte boolean value.
        /// </summary>
        public bool ReadBoolean() {
            // BinaryReader.ReadBoolean accepts any non-zero value as true, which I don't want.
            byte b = this.br.ReadByte();
            return b != 0 && (b == 1 ? true : throw new InvalidDataException($"ReadBoolean encountered non-boolean value: 0x{b:X2}"));
        }

        /// <summary>
        /// Reads an array of one-byte boolean values.
        /// </summary>
        public bool[] ReadBooleans(int count) {
            bool[] result = new bool[count];
            for (int i = 0; i < count; i++) {
                result[i] = this.ReadBoolean();
            }

            return result;
        }

        /// <summary>
        /// Reads a one-byte boolean value from the specified offset without advancing the stream.
        /// </summary>
        public bool GetBoolean(long offset) => this.GetValue(this.ReadBoolean, offset);

        /// <summary>
        /// Reads an array of one-byte boolean values from the specified offset without advancing the stream.
        /// </summary>
        public bool[] GetBooleans(long offset, int count) => this.GetValues(this.ReadBooleans, offset, count);

        /// <summary>
        /// Reads a one-byte boolean value and throws an exception if it does not match the specified option.
        /// </summary>
        public bool AssertBoolean(bool option) => this.AssertValue(this.ReadBoolean(), "Boolean", "{0}", new bool[] { option });
        #endregion

        #region SByte
        /// <summary>
        /// Reads a one-byte signed integer.
        /// </summary>
        public sbyte ReadSByte() => this.br.ReadSByte();

        /// <summary>
        /// Reads an array of one-byte signed integers.
        /// </summary>
        public sbyte[] ReadSBytes(int count) {
            sbyte[] result = new sbyte[count];
            for (int i = 0; i < count; i++) {
                result[i] = this.ReadSByte();
            }

            return result;
        }

        /// <summary>
        /// Reads a one-byte signed integer from the specified offset without advancing the stream.
        /// </summary>
        public sbyte GetSByte(long offset) => this.GetValue(this.ReadSByte, offset);

        /// <summary>
        /// Reads an array of one-byte signed integers from the specified offset without advancing the stream.
        /// </summary>
        public sbyte[] GetSBytes(long offset, int count) => this.GetValues(this.ReadSBytes, offset, count);

        /// <summary>
        /// Reads a one-byte signed integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public sbyte AssertSByte(params sbyte[] options) => this.AssertValue(this.ReadSByte(), "SByte", "0x{0:X}", options);
        #endregion

        #region Byte
        /// <summary>
        /// Reads a one-byte unsigned integer.
        /// </summary>
        public byte ReadByte() => this.br.ReadByte();

        /// <summary>
        /// Reads an array of one-byte unsigned integers.
        /// </summary>
        public byte[] ReadBytes(int count) {
            byte[] result = this.br.ReadBytes(count);
            return result.Length != count
                ? throw new EndOfStreamException("Remaining size of stream was smaller than requested number of bytes.")
                : result;
        }

        /// <summary>
        /// Reads the specified number of bytes from the stream into the buffer starting at the specified index.
        /// </summary>
        public void ReadBytes(byte[] buffer, int index, int count) {
            int read = this.br.Read(buffer, index, count);
            if (read != count) {
                throw new EndOfStreamException("Remaining size of stream was smaller than requested number of bytes.");
            }
        }

        /// <summary>
        /// Reads a one-byte unsigned integer from the specified offset without advancing the stream.
        /// </summary>
        public byte GetByte(long offset) => this.GetValue(this.ReadByte, offset);

        /// <summary>
        /// Reads an array of one-byte unsigned integers from the specified offset without advancing the stream.
        /// </summary>
        public byte[] GetBytes(long offset, int count) {
            this.StepIn(offset);
            byte[] result = this.ReadBytes(count);
            this.StepOut();
            return result;
        }

        /// <summary>
        /// Reads the specified number of bytes from the offset into the buffer starting at the specified index without advancing the stream.
        /// </summary>
        public void GetBytes(long offset, byte[] buffer, int index, int count) {
            this.StepIn(offset);
            this.ReadBytes(buffer, index, count);
            this.StepOut();
        }

        /// <summary>
        /// Reads a one-byte unsigned integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public byte AssertByte(params byte[] options) => this.AssertValue(this.ReadByte(), "Byte", "0x{0:X}", options);
        #endregion

        #region Int16
        /// <summary>
        /// Reads a two-byte signed integer.
        /// </summary>
        public short ReadInt16() => this.BigEndian ? BitConverter.ToInt16(this.ReadReversedBytes(2), 0) : this.br.ReadInt16();

        /// <summary>
        /// Reads an array of two-byte signed integers.
        /// </summary>
        public short[] ReadInt16s(int count) {
            short[] result = new short[count];
            for (int i = 0; i < count; i++) {
                result[i] = this.ReadInt16();
            }

            return result;
        }

        /// <summary>
        /// Reads a two-byte signed integer from the specified offset without advancing the stream.
        /// </summary>
        public short GetInt16(long offset) => this.GetValue(this.ReadInt16, offset);

        /// <summary>
        /// Reads an array of two-byte signed integers from the specified offset without advancing the stream.
        /// </summary>
        public short[] GetInt16s(long offset, int count) => this.GetValues(this.ReadInt16s, offset, count);

        /// <summary>
        /// Reads a two-byte signed integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public short AssertInt16(params short[] options) => this.AssertValue(this.ReadInt16(), "Int16", "0x{0:X}", options);
        #endregion

        #region UInt16
        /// <summary>
        /// Reads a two-byte unsigned integer.
        /// </summary>
        public ushort ReadUInt16() => this.BigEndian ? BitConverter.ToUInt16(this.ReadReversedBytes(2), 0) : this.br.ReadUInt16();

        /// <summary>
        /// Reads an array of two-byte unsigned integers.
        /// </summary>
        public ushort[] ReadUInt16s(int count) {
            ushort[] result = new ushort[count];
            for (int i = 0; i < count; i++) {
                result[i] = this.ReadUInt16();
            }

            return result;
        }

        /// <summary>
        /// Reads a two-byte unsigned integer from the specified position without advancing the stream.
        /// </summary>
        public ushort GetUInt16(long offset) => this.GetValue(this.ReadUInt16, offset);

        /// <summary>
        /// Reads an array of two-byte unsigned integers from the specified position without advancing the stream.
        /// </summary>
        public ushort[] GetUInt16s(long offset, int count) => this.GetValues(this.ReadUInt16s, offset, count);

        /// <summary>
        /// Reads a two-byte unsigned integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public ushort AssertUInt16(params ushort[] options) => this.AssertValue(this.ReadUInt16(), "UInt16", "0x{0:X}", options);
        #endregion

        #region Int32
        /// <summary>
        /// Reads a four-byte signed integer.
        /// </summary>
        public int ReadInt32() => this.BigEndian ? BitConverter.ToInt32(this.ReadReversedBytes(4), 0) : this.br.ReadInt32();

        /// <summary>
        /// Reads an array of four-byte signed integers.
        /// </summary>
        public int[] ReadInt32s(int count) {
            int[] result = new int[count];
            for (int i = 0; i < count; i++) {
                result[i] = this.ReadInt32();
            }

            return result;
        }

        /// <summary>
        /// Reads a four-byte signed integer from the specified position without advancing the stream.
        /// </summary>
        public int GetInt32(long offset) => this.GetValue(this.ReadInt32, offset);

        /// <summary>
        /// Reads an array of four-byte signed integers from the specified position without advancing the stream.
        /// </summary>
        public int[] GetInt32s(long offset, int count) => this.GetValues(this.ReadInt32s, offset, count);

        /// <summary>
        /// Reads a four-byte signed integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public int AssertInt32(params int[] options) => this.AssertValue(this.ReadInt32(), "Int32", "0x{0:X}", options);
        #endregion

        #region UInt32
        /// <summary>
        /// Reads a four-byte unsigned integer.
        /// </summary>
        public uint ReadUInt32() => this.BigEndian ? BitConverter.ToUInt32(this.ReadReversedBytes(4), 0) : this.br.ReadUInt32();

        /// <summary>
        /// Reads an array of four-byte unsigned integers.
        /// </summary>
        public uint[] ReadUInt32s(int count) {
            uint[] result = new uint[count];
            for (int i = 0; i < count; i++) {
                result[i] = this.ReadUInt32();
            }

            return result;
        }

        /// <summary>
        /// Reads a four-byte unsigned integer from the specified position without advancing the stream.
        /// </summary>
        public uint GetUInt32(long offset) => this.GetValue(this.ReadUInt32, offset);

        /// <summary>
        /// Reads an array of four-byte unsigned integers from the specified position without advancing the stream.
        /// </summary>
        public uint[] GetUInt32s(long offset, int count) => this.GetValues(this.ReadUInt32s, offset, count);

        /// <summary>
        /// Reads a four-byte unsigned integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public uint AssertUInt32(params uint[] options) => this.AssertValue(this.ReadUInt32(), "UInt32", "0x{0:X}", options);
        #endregion

        #region Int64
        /// <summary>
        /// Reads an eight-byte signed integer.
        /// </summary>
        public long ReadInt64() => this.BigEndian ? BitConverter.ToInt64(this.ReadReversedBytes(8), 0) : this.br.ReadInt64();

        /// <summary>
        /// Reads an array of eight-byte signed integers.
        /// </summary>
        public long[] ReadInt64s(int count) {
            long[] result = new long[count];
            for (int i = 0; i < count; i++) {
                result[i] = this.ReadInt64();
            }

            return result;
        }

        /// <summary>
        /// Reads an eight-byte signed integer from the specified position without advancing the stream.
        /// </summary>
        public long GetInt64(long offset) => this.GetValue(this.ReadInt64, offset);

        /// <summary>
        /// Reads an array eight-byte signed integers from the specified position without advancing the stream.
        /// </summary>
        public long[] GetInt64s(long offset, int count) => this.GetValues(this.ReadInt64s, offset, count);

        /// <summary>
        /// Reads an eight-byte signed integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public long AssertInt64(params long[] options) => this.AssertValue(this.ReadInt64(), "Int64", "0x{0:X}", options);
        #endregion

        #region UInt64
        /// <summary>
        /// Reads an eight-byte unsigned integer.
        /// </summary>
        public ulong ReadUInt64() => this.BigEndian ? BitConverter.ToUInt64(this.ReadReversedBytes(8), 0) : this.br.ReadUInt64();

        /// <summary>
        /// Reads an array of eight-byte unsigned integers.
        /// </summary>
        public ulong[] ReadUInt64s(int count) {
            ulong[] result = new ulong[count];
            for (int i = 0; i < count; i++) {
                result[i] = this.ReadUInt64();
            }

            return result;
        }

        /// <summary>
        /// Reads an eight-byte unsigned integer from the specified position without advancing the stream.
        /// </summary>
        public ulong GetUInt64(long offset) => this.GetValue(this.ReadUInt64, offset);

        /// <summary>
        /// Reads an array of eight-byte unsigned integers from the specified position without advancing the stream.
        /// </summary>
        public ulong[] GetUInt64s(long offset, int count) => this.GetValues(this.ReadUInt64s, offset, count);

        /// <summary>
        /// Reads an eight-byte unsigned integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public ulong AssertUInt64(params ulong[] options) => this.AssertValue(this.ReadUInt64(), "UInt64", "0x{0:X}", options);
        #endregion

        #region Varint
        /// <summary>
        /// Reads either a four or eight-byte signed integer depending on VarintLong.
        /// </summary>
        public long ReadVarint() => this.VarintLong ? this.ReadInt64() : this.ReadInt32();

        /// <summary>
        /// Reads an array of either four or eight-byte signed integers depending on VarintLong.
        /// </summary>
        public long[] ReadVarints(int count) {
            long[] result = new long[count];
            for (int i = 0; i < count; i++) {
                result[i] = this.VarintLong ? this.ReadInt64() : this.ReadInt32();
            }
            return result;
        }

        /// <summary>
        /// Reads either a four or eight-byte signed integer depending on VarintLong from the specified position without advancing the stream.
        /// </summary>
        public long GetVarint(long offset) => this.VarintLong ? this.GetInt64(offset) : this.GetInt32(offset);

        /// <summary>
        /// Reads an array of either four or eight-byte signed integers depending on VarintLong from the specified position without advancing the stream.
        /// </summary>
        public long[] GetVarints(long offset, int count) => this.GetValues(this.ReadVarints, offset, count);

        /// <summary>
        /// Reads either a four or eight-byte signed integer depending on VarintLong and throws an exception if it does not match any of the specified options.
        /// </summary>
        public long AssertVarint(params long[] options) => this.AssertValue(this.ReadVarint(), this.VarintLong ? "Varint64" : "Varint32", "0x{0:X}", options);
        #endregion

        #region Single
        /// <summary>
        /// Reads a four-byte floating point number.
        /// </summary>
        public float ReadSingle() => this.BigEndian ? BitConverter.ToSingle(this.ReadReversedBytes(4), 0) : this.br.ReadSingle();

        /// <summary>
        /// Reads an array of four-byte floating point numbers.
        /// </summary>
        public float[] ReadSingles(int count) {
            float[] result = new float[count];
            for (int i = 0; i < count; i++) {
                result[i] = this.ReadSingle();
            }

            return result;
        }

        /// <summary>
        /// Reads a four-byte floating point number from the specified position without advancing the stream.
        /// </summary>
        public float GetSingle(long offset) => this.GetValue(this.ReadSingle, offset);

        /// <summary>
        /// Reads an array of four-byte floating point numbers from the specified position without advancing the stream.
        /// </summary>
        public float[] GetSingles(long offset, int count) => this.GetValues(this.ReadSingles, offset, count);

        /// <summary>
        /// Reads a four-byte floating point number and throws an exception if it does not match any of the specified options.
        /// </summary>
        public float AssertSingle(params float[] options) => this.AssertValue(this.ReadSingle(), "Single", "{0}", options);
        #endregion

        #region Double
        /// <summary>
        /// Reads an eight-byte floating point number.
        /// </summary>
        public double ReadDouble() => this.BigEndian ? BitConverter.ToDouble(this.ReadReversedBytes(8), 0) : this.br.ReadDouble();

        /// <summary>
        /// Reads an array of eight-byte floating point numbers.
        /// </summary>
        public double[] ReadDoubles(int count) {
            double[] result = new double[count];
            for (int i = 0; i < count; i++) {
                result[i] = this.ReadDouble();
            }

            return result;
        }

        /// <summary>
        /// Reads an eight-byte floating point number from the specified position without advancing the stream.
        /// </summary>
        public double GetDouble(long offset) => this.GetValue(this.ReadDouble, offset);

        /// <summary>
        /// Reads an array of eight-byte floating point numbers from the specified position without advancing the stream.
        /// </summary>
        public double[] GetDoubles(long offset, int count) => this.GetValues(this.ReadDoubles, offset, count);

        /// <summary>
        /// Reads an eight-byte floating point number and throws an exception if it does not match any of the specified options.
        /// </summary>
        public double AssertDouble(params double[] options) => this.AssertValue(this.ReadDouble(), "Double", "{0}", options);
        #endregion

        #region Enum
        private TEnum ReadEnum<TEnum, TValue>(Func<TValue> readValue, string valueFormat) {
            TValue value = readValue();
            if (!Enum.IsDefined(typeof(TEnum), value)) {
                string strValue = string.Format(valueFormat, value);
                throw new InvalidDataException($"Read Byte not present in enum: {strValue}");
            }
            return (TEnum)(object)value;
        }

        /// <summary>
        /// Reads a one-byte value as the specified enum, throwing an exception if not present.
        /// </summary>
        public TEnum ReadEnum8<TEnum>() where TEnum : Enum => this.ReadEnum<TEnum, byte>(this.ReadByte, "0x{0:X}");


        /// <summary>
        /// Reads a one-byte enum from the specified position without advancing the stream.
        /// </summary>
        public TEnum GetEnum8<TEnum>(long position) where TEnum : Enum {
            this.StepIn(position);
            TEnum result = this.ReadEnum8<TEnum>();
            this.StepOut();
            return result;
        }

        /// <summary>
        /// Reads a two-byte value as the specified enum, throwing an exception if not present.
        /// </summary>
        public TEnum ReadEnum16<TEnum>() where TEnum : Enum => this.ReadEnum<TEnum, ushort>(this.ReadUInt16, "0x{0:X}");

        /// <summary>
        /// Reads a two-byte enum from the specified position without advancing the stream.
        /// </summary>
        public TEnum GetEnum16<TEnum>(long position) where TEnum : Enum {
            this.StepIn(position);
            TEnum result = this.ReadEnum16<TEnum>();
            this.StepOut();
            return result;
        }

        /// <summary>
        /// Reads a four-byte value as the specified enum, throwing an exception if not present.
        /// </summary>
        public TEnum ReadEnum32<TEnum>() where TEnum : Enum => this.ReadEnum<TEnum, uint>(this.ReadUInt32, "0x{0:X}");

        /// <summary>
        /// Reads a four-byte enum from the specified position without advancing the stream.
        /// </summary>
        public TEnum GetEnum32<TEnum>(long position) where TEnum : Enum {
            this.StepIn(position);
            TEnum result = this.ReadEnum32<TEnum>();
            this.StepOut();
            return result;
        }

        /// <summary>
        /// Reads an eight-byte value as the specified enum, throwing an exception if not present.
        /// </summary>
        public TEnum ReadEnum64<TEnum>() where TEnum : Enum => this.ReadEnum<TEnum, ulong>(this.ReadUInt64, "0x{0:X}");

        /// <summary>
        /// Reads an eight-byte enum from the specified position without advancing the stream.
        /// </summary>
        public TEnum GetEnum64<TEnum>(long position) where TEnum : Enum {
            this.StepIn(position);
            TEnum result = this.ReadEnum64<TEnum>();
            this.StepOut();
            return result;
        }
        #endregion

        #region String
        /// <summary>
        /// Reads the specified number of bytes and interprets them according to the specified encoding.
        /// </summary>
        private string ReadChars(Encoding encoding, int length) {
            byte[] bytes = this.ReadBytes(length);
            return encoding.GetString(bytes);
        }

        /// <summary>
        /// Reads bytes until a single-byte null terminator is found, then interprets them according to the specified encoding.
        /// </summary>
        private string ReadCharsTerminated(Encoding encoding) {
            var bytes = new List<byte>();

            byte b = this.ReadByte();
            while (b != 0) {
                bytes.Add(b);
                b = this.ReadByte();
            }

            return encoding.GetString(bytes.ToArray());
        }

        /// <summary>
        /// Reads a null-terminated ASCII string.
        /// </summary>
        public string ReadASCII() => this.ReadCharsTerminated(SFEncoding.ASCII);

        /// <summary>
        /// Reads an ASCII string with the specified length in bytes.
        /// </summary>
        public string ReadASCII(int length) => this.ReadChars(SFEncoding.ASCII, length);

        /// <summary>
        /// Reads a null-terminated ASCII string from the specified position without advancing the stream.
        /// </summary>
        public string GetASCII(long offset) {
            this.StepIn(offset);
            string result = this.ReadASCII();
            this.StepOut();
            return result;
        }

        /// <summary>
        /// Reads an ASCII string with the specified length in bytes from the specified position without advancing the stream.
        /// </summary>
        public string GetASCII(long offset, int length) {
            this.StepIn(offset);
            string result = this.ReadASCII(length);
            this.StepOut();
            return result;
        }

        /// <summary>
        /// Reads as many ASCII characters as are in the specified value and throws an exception if they do not match.
        /// </summary>
        public string AssertASCII(params string[] values) {
            string s = this.ReadASCII(values[0].Length);

            return values.Contains(s) ? s
                : throw new InvalidDataException($"Read ASCII: {s} | Expected ASCII: {string.Join(", ", values)}");
        }

        /// <summary>
        /// Reads a null-terminated Shift JIS string.
        /// </summary>
        public string ReadShiftJIS() => this.ReadCharsTerminated(SFEncoding.ShiftJIS);

        /// <summary>
        /// Reads a Shift JIS string with the specified length in bytes.
        /// </summary>
        public string ReadShiftJIS(int length) => this.ReadChars(SFEncoding.ShiftJIS, length);

        /// <summary>
        /// Reads a null-terminated Shift JIS string from the specified position without advancing the stream.
        /// </summary>
        public string GetShiftJIS(long offset) {
            this.StepIn(offset);
            string result = this.ReadShiftJIS();
            this.StepOut();
            return result;
        }

        /// <summary>
        /// Reads a Shift JIS string with the specified length in bytes from the specified position without advancing the stream.
        /// </summary>
        public string GetShiftJIS(long offset, int length) {
            this.StepIn(offset);
            string result = this.ReadShiftJIS(length);
            this.StepOut();
            return result;
        }

        /// <summary>
        /// Reads a null-terminated UTF-16 string.
        /// </summary>
        public unsafe string ReadUTF16() {
            var bytes = new List<char>();
            char @char;
            while ((@char = (char)this.ReadUInt16()) != 0) {
                bytes.Add(@char);
            }
            var span = new ReadOnlySpan<byte>(bytes.AsPointer<char, byte>(), bytes.Count << 1);
            return this.BigEndian ? SFEncoding.UTF16BE.GetString(span) : SFEncoding.UTF16.GetString(span);
        }

        /// <summary>
        /// Reads a null-terminated UTF-16 string from the specified position without advancing the stream.
        /// </summary>
        public string GetUTF16(long offset) {
            this.StepIn(offset);
            string result = this.ReadUTF16();
            this.StepOut();
            return result;
        }

        /// <summary>
        /// Reads a null-terminated Shift JIS string in a fixed-size field.
        /// </summary>
        public string ReadFixStr(int size) {
            byte[] bytes = this.ReadBytes(size);
            int terminator;
            for (terminator = 0; terminator < size; terminator++) {
                if (bytes[terminator] == 0) {
                    break;
                }
            }
            return SFEncoding.ShiftJIS.GetString(bytes, 0, terminator);
        }

        /// <summary>
        /// Reads a null-terminated UTF-16 string in a fixed-size field.
        /// </summary>
        public string ReadFixStrW(int size) {
            byte[] bytes = this.ReadBytes(size);
            int terminator;
            for (terminator = 0; terminator < size; terminator += 2) {
                // If length is odd (which it really shouldn't be), avoid indexing out of the array and align the terminator to the end
                if (terminator == size - 1) {
                    terminator--;
                } else if (bytes[terminator] == 0 && bytes[terminator + 1] == 0) {
                    break;
                }
            }

            return this.BigEndian ? SFEncoding.UTF16BE.GetString(bytes, 0, terminator) : SFEncoding.UTF16.GetString(bytes, 0, terminator);
        }
        #endregion

        #region Other
        /// <summary>
        /// Reads a vector of two four-byte floating point numbers.
        /// </summary>
        public Vector2 ReadVector2() {
            float x = this.ReadSingle();
            float y = this.ReadSingle();
            return new Vector2(x, y);
        }

        /// <summary>
        /// Reads a vector of three four-byte floating point numbers.
        /// </summary>
        public Vector3 ReadVector3() {
            float x = this.ReadSingle();
            float y = this.ReadSingle();
            float z = this.ReadSingle();
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Reads a vector of four four-byte floating point numbers.
        /// </summary>
        public Vector4 ReadVector4() {
            float x = this.ReadSingle();
            float y = this.ReadSingle();
            float z = this.ReadSingle();
            float w = this.ReadSingle();
            return new Vector4(x, y, z, w);
        }

        /// <summary>
        /// Read length number of bytes and assert that they all match the given value.
        /// </summary>
        public void AssertPattern(int length, byte pattern) {
            byte[] bytes = this.ReadBytes(length);
            for (int i = 0; i < length; i++) {
                if (bytes[i] != pattern) {
                    throw new InvalidDataException($"Expected {length} 0x{pattern:X2}, got {bytes[i]:X2} at position {i}");
                }
            }
        }

        /// <summary>
        /// Reads a 4-byte color in ARGB order.
        /// </summary>
        public Color ReadARGB() {
            byte a = this.br.ReadByte();
            byte r = this.br.ReadByte();
            byte g = this.br.ReadByte();
            byte b = this.br.ReadByte();
            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Reads a 4-byte color in ABGR order.
        /// </summary>
        public Color ReadABGR() {
            byte a = this.br.ReadByte();
            byte b = this.br.ReadByte();
            byte g = this.br.ReadByte();
            byte r = this.br.ReadByte();
            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Reads a 4-byte color in RGBA order.
        /// </summary>
        public Color ReadRGBA() {
            byte r = this.br.ReadByte();
            byte g = this.br.ReadByte();
            byte b = this.br.ReadByte();
            byte a = this.br.ReadByte();
            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Reads a 4-byte color in BGRA order.
        /// </summary>
        public Color ReadBGRA() {
            byte b = this.br.ReadByte();
            byte g = this.br.ReadByte();
            byte r = this.br.ReadByte();
            byte a = this.br.ReadByte();
            return Color.FromArgb(a, r, g, b);
        }
        #endregion
    }
}
