using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Text;

namespace SoulsFormats.Util {
    /// <summary>
    /// An extended writer for binary data supporting big and little endianness, value reservation, and arrays.
    /// </summary>
    public class BinaryWriterEx : IDisposable {
        private readonly BinaryWriter bw;
        private readonly Stack<long> steps;
        private readonly Dictionary<string, long> reservations;

        /// <summary>
        /// Interpret values as big-endian if set, or little-endian if not.
        /// </summary>
        public bool BigEndian;

        /// <summary>
        /// Varints are written as Int64 if set, otherwise Int32.
        /// </summary>
        public bool VarintLong;
        private bool disposedValue;

        /// <summary>
        /// Current size of varints in bytes.
        /// </summary>
        public int VarintSize => this.VarintLong ? 8 : 4;

        /// <summary>
        /// The underlying stream.
        /// </summary>
        public readonly Stream Stream;

        /// <summary>
        /// The current position of the stream.
        /// </summary>
        public long Position {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.Stream.Position;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this.Stream.Position = value;
        }

        /// <summary>
        /// The length of the stream.
        /// </summary>
        public long Length => this.Stream.Length;

        /// <summary>
        /// Initializes a new <c>BinaryWriterEx</c> writing to an empty <c>MemoryStream</c>
        /// </summary>
        public BinaryWriterEx(bool bigEndian) : this(bigEndian, new MemoryStream()) { }

        /// <summary>
        /// Initializes a new <c>BinaryWriterEx</c> writing to the specified stream.
        /// </summary>
        public BinaryWriterEx(bool bigEndian, Stream stream) {
            this.BigEndian = bigEndian;
            this.steps = new Stack<long>();
            this.reservations = new Dictionary<string, long>();
            this.Stream = stream;
            this.bw = new BinaryWriter(stream);
        }

        private void WriteReversedBytes(byte[] bytes) {
            Array.Reverse(bytes);
            this.bw.Write(bytes);
        }

        private void Reserve(string name, string typeName, int length) {
            name = $"{name}:{typeName}";
            if (this.reservations.ContainsKey(name)) {
                throw new ArgumentException("Key already reserved: " + name);
            }

            this.reservations[name] = this.Stream.Position;
            for (int i = 0; i < length; i++) {
                this.WriteByte(0xFE);
            }
        }

        private long Fill(string name, string typeName) {
            name = $"{name}:{typeName}";
            if (!this.reservations.TryGetValue(name, out long jump)) {
                throw new ArgumentException("Key is not reserved: " + name);
            }

            _ = this.reservations.Remove(name);
            return jump;
        }

        /// <summary>
        /// Verify that all reservations are filled and close the stream.
        /// </summary>
        public void Finish() {
            if (this.reservations.Count > 0) {
                throw new InvalidOperationException("Not all reservations filled: " + string.Join(", ", this.reservations.Keys));
            }
            this.bw.Close();
        }

        /// <summary>
        /// Verify that all reservations are filled, close the stream, and return the written data as an array of bytes.
        /// </summary>
        public byte[] FinishBytes() {
            var ms = (MemoryStream)this.Stream;
            byte[] result = ms.ToArray();
            this.Finish();
            return result;
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
                throw new InvalidOperationException("Writer is already stepped all the way out.");
            }

            this.Stream.Position = this.steps.Pop();
        }

        /// <summary>
        /// Writes 0x00 bytes until the stream position meets the specified alignment.
        /// </summary>
        public void Pad(int align) {
            while (this.Stream.Position % align > 0) {
                this.WriteByte(0);
            }
        }

        /// <summary>
        /// Writes 0x00 bytes until the stream position meets the specified alignment relative to the given starting position.
        /// </summary>
        public void PadRelative(long start, int align) {
            while ((this.Stream.Position - start) % align > 0) {
                this.WriteByte(0);
            }
        }

        #region Boolean
        /// <summary>
        /// Writes a one-byte boolean value.
        /// </summary>
        public void WriteBoolean(bool value) => this.bw.WriteValueType(value);

        /// <summary>
        /// Writes an array of one-byte boolean values.
        /// </summary>
        public void WriteBooleans(IList<bool> values) {
            foreach (bool value in values) {
                this.WriteBoolean(value);
            }
        }

        /// <summary>
        /// Reserves the current position and advance the stream by one byte.
        /// </summary>
        public void ReserveBoolean(string name) => this.Reserve(name, "Boolean", 1);

        /// <summary>
        /// Writes a one-byte boolean value to a reserved position.
        /// </summary>
        public void FillBoolean(string name, bool value) {
            this.StepIn(this.Fill(name, "Boolean"));
            this.WriteBoolean(value);
            this.StepOut();
        }
        #endregion

        #region SByte
        /// <summary>
        /// Writes a one-byte signed integer.
        /// </summary>
        public void WriteSByte(sbyte value) => this.bw.WriteValueType(value);

        /// <summary>
        /// Writes an array of one-byte signed integers.
        /// </summary>
        public void WriteSBytes(IList<sbyte> values) {
            foreach (sbyte value in values) {
                this.WriteSByte(value);
            }
        }

        /// <summary>
        /// Reserves the current position and advance the stream by one byte.
        /// </summary>
        public void ReserveSByte(string name) => this.Reserve(name, "SByte", 1);

        /// <summary>
        /// Writes a one-byte signed integer to a reserved position.
        /// </summary>
        public void FillSByte(string name, sbyte value) {
            this.StepIn(this.Fill(name, "SByte"));
            this.WriteSByte(value);
            this.StepOut();
        }
        #endregion

        #region Byte
        /// <summary>
        /// Writes a one-byte unsigned integer.
        /// </summary>
        public void WriteByte(byte value) => this.bw.WriteValueType(value);

        /// <summary>
        /// Writes an array of one-byte unsigned integers.
        /// </summary>
        public void WriteBytes(byte[] bytes) => this.bw.Write(bytes);

        /// <summary>
        /// Writes an array of one-byte unsigned integers.
        /// </summary>
        public void WriteBytes(IList<byte> values) {
            foreach (byte value in values) {
                this.WriteByte(value);
            }
        }

        /// <summary>
        /// Reserves the current position and advances the stream by one byte.
        /// </summary>
        public void ReserveByte(string name) => this.Reserve(name, "Byte", 1);

        /// <summary>
        /// Writes a one-byte unsigned integer to a reserved position.
        /// </summary>
        public void FillByte(string name, byte value) {
            this.StepIn(this.Fill(name, "Byte"));
            this.WriteByte(value);
            this.StepOut();
        }
        #endregion

        #region Int16
        /// <summary>
        /// Writes a two-byte signed integer.
        /// </summary>
        public void WriteInt16(short value) => this.bw.WriteValueType(this.BigEndian ? BinaryPrimitives.ReverseEndianness(value) : value);

        /// <summary>
        /// Writes an array of two-byte signed integers.
        /// </summary>
        public void WriteInt16s(IList<short> values) {
            foreach (short value in values) {
                this.WriteInt16(value);
            }
        }

        /// <summary>
        /// Reserves the current position and advances the stream by two bytes.
        /// </summary>
        public void ReserveInt16(string name) => this.Reserve(name, "Int16", 2);

        /// <summary>
        /// Writes a two-byte signed integer to a reserved position.
        /// </summary>
        public void FillInt16(string name, short value) {
            this.StepIn(this.Fill(name, "Int16"));
            this.WriteInt16(value);
            this.StepOut();
        }
        #endregion

        #region UInt16
        /// <summary>
        /// Writes a two-byte unsigned integer.
        /// </summary>
        public void WriteUInt16(ushort value) => this.bw.WriteValueType(this.BigEndian ? BinaryPrimitives.ReverseEndianness(value) : value);

        /// <summary>
        /// Writes an array of two-byte unsigned integers.
        /// </summary>
        public void WriteUInt16s(IList<ushort> values) {
            foreach (ushort value in values) {
                this.WriteUInt16(value);
            }
        }

        /// <summary>
        /// Reserves the current position and advances the stream by two bytes.
        /// </summary>
        public void ReserveUInt16(string name) => this.Reserve(name, "UInt16", 2);

        /// <summary>
        /// Writes a two-byte unsigned integer to a reserved position.
        /// </summary>
        public void FillUInt16(string name, ushort value) {
            this.StepIn(this.Fill(name, "UInt16"));
            this.WriteUInt16(value);
            this.StepOut();
        }
        #endregion

        #region Int32
        /// <summary>
        /// Writes a four-byte signed integer.
        /// </summary>
        public void WriteInt32(int value) => this.bw.WriteValueType(this.BigEndian ? BinaryPrimitives.ReverseEndianness(value) : value);

        /// <summary>
        /// Writes an array of four-byte signed integers.
        /// </summary>
        public void WriteInt32s(IList<int> values) {
            foreach (int value in values) {
                this.WriteInt32(value);
            }
        }

        /// <summary>
        /// Reserves the current position and advances the stream by four bytes.
        /// </summary>
        public void ReserveInt32(string name) => this.Reserve(name, "Int32", 4);

        /// <summary>
        /// Writes a four-byte signed integer to a reserved position.
        /// </summary>
        public void FillInt32(string name, int value) {
            this.StepIn(this.Fill(name, "Int32"));
            this.WriteInt32(value);
            this.StepOut();
        }
        #endregion

        #region UInt32
        /// <summary>
        /// Writes a four-byte unsigned integer.
        /// </summary>
        public void WriteUInt32(uint value) => this.bw.WriteValueType(this.BigEndian ? BinaryPrimitives.ReverseEndianness(value) : value);

        /// <summary>
        /// Writes an array of four-byte unsigned integers.
        /// </summary>
        public void WriteUInt32s(IList<uint> values) {
            foreach (uint value in values) {
                this.WriteUInt32(value);
            }
        }

        /// <summary>
        /// Reserves the current position and advances the stream by four bytes.
        /// </summary>
        public void ReserveUInt32(string name) => this.Reserve(name, "UInt32", 4);

        /// <summary>
        /// Writes a four-byte unsigned integer to a reserved position.
        /// </summary>
        public void FillUInt32(string name, uint value) {
            this.StepIn(this.Fill(name, "UInt32"));
            this.WriteUInt32(value);
            this.StepOut();
        }
        #endregion

        #region Int64
        /// <summary>
        /// Writes an eight-byte signed integer.
        /// </summary>
        public void WriteInt64(long value) => this.bw.WriteValueType(this.BigEndian ? BinaryPrimitives.ReverseEndianness(value) : value);

        /// <summary>
        /// Writes an array of eight-byte signed integers.
        /// </summary>
        public void WriteInt64s(IList<long> values) {
            foreach (long value in values) {
                this.WriteInt64(value);
            }
        }

        /// <summary>
        /// Reserves the current position and advances the stream by eight bytes.
        /// </summary>
        public void ReserveInt64(string name) => this.Reserve(name, "Int64", 8);

        /// <summary>
        /// Writes an eight-byte signed integer to a reserved position.
        /// </summary>
        public void FillInt64(string name, long value) {
            this.StepIn(this.Fill(name, "Int64"));
            this.WriteInt64(value);
            this.StepOut();
        }
        #endregion

        #region UInt64
        /// <summary>
        /// Writes an eight-byte unsigned integer.
        /// </summary>
        public void WriteUInt64(ulong value) => this.bw.WriteValueType(this.BigEndian ? BinaryPrimitives.ReverseEndianness(value) : value);

        /// <summary>
        /// Writes an array of eight-byte unsigned integers.
        /// </summary>
        public void WriteUInt64s(IList<ulong> values) {
            foreach (ulong value in values) {
                this.WriteUInt64(value);
            }
        }

        /// <summary>
        /// Reserves the current position and advances the stream by eight bytes.
        /// </summary>
        public void ReserveUInt64(string name) => this.Reserve(name, "UInt64", 8);

        /// <summary>
        /// Writes an eight-byte unsigned integer to a reserved position.
        /// </summary>
        public void FillUInt64(string name, ulong value) {
            this.StepIn(this.Fill(name, "UInt64"));
            this.WriteUInt64(value);
            this.StepOut();
        }
        #endregion

        #region Varint
        /// <summary>
        /// Writes either a four or eight-byte signed integer depending on VarintLong.
        /// </summary>
        public void WriteVarint(long value) {
            if (this.VarintLong) {
                this.WriteInt64(value);
            } else {
                this.WriteInt32((int)value);
            }
        }

        /// <summary>
        /// Writes an array of either four or eight-byte signed integers depending on VarintLong.
        /// </summary>
        public void WriteVarints(IList<long> values) {
            foreach (long value in values) {
                if (this.VarintLong) {
                    this.WriteInt64(value);
                } else {
                    this.WriteInt32((int)value);
                }
            }
        }

        /// <summary>
        /// Reserves the current position and advances the stream by either four or eight bytes depending on VarintLong.
        /// </summary>
        public void ReserveVarint(string name) {
            if (this.VarintLong) {
                this.Reserve(name, "Varint64", 8);
            } else {
                this.Reserve(name, "Varint32", 4);
            }
        }

        /// <summary>
        /// Writes either a four or eight-byte signed integer depending on VarintLong to a reserved position.
        /// </summary>
        public void FillVarint(string name, long value) {
            if (this.VarintLong) {
                this.StepIn(this.Fill(name, "Varint64"));
                this.WriteInt64(value);
                this.StepOut();
            } else {
                this.StepIn(this.Fill(name, "Varint32"));
                this.WriteInt32((int)value);
                this.StepOut();
            }
        }
        #endregion

        #region Single
        /// <summary>
        /// Writes a four-byte floating point number.
        /// </summary>
        public void WriteSingle(float value) => this.bw.WriteValueType(this.BigEndian ? CastTo<float, uint>(BinaryPrimitives.ReverseEndianness(CastTo<uint, float>(value))) : value);

        /// <summary>
        /// Writes an array of four-byte floating point numbers.
        /// </summary>
        public void WriteSingles(IList<float> values) {
            foreach (float value in values) {
                this.WriteSingle(value);
            }
        }

        /// <summary>
        /// Reserves the current position and advances the stream by four bytes.
        /// </summary>
        public void ReserveSingle(string name) => this.Reserve(name, "Single", 4);

        /// <summary>
        /// Writes a four-byte floating point number to a reserved position.
        /// </summary>
        public void FillSingle(string name, float value) {
            this.StepIn(this.Fill(name, "Single"));
            this.WriteSingle(value);
            this.StepOut();
        }
        #endregion

        #region Double
        /// <summary>
        /// Writes an eight-byte floating point number.
        /// </summary>
        public void WriteDouble(double value) => this.bw.WriteValueType(this.BigEndian ? CastTo<double, ulong>(BinaryPrimitives.ReverseEndianness(CastTo<ulong, double>(value))) : value);

        /// <summary>
        /// Writes an array of eight-byte floating point numbers.
        /// </summary>
        public void WriteDoubles(IList<double> values) {
            foreach (double value in values) {
                this.WriteDouble(value);
            }
        }

        /// <summary>
        /// Reserves the current position and advances the stream by eight bytes.
        /// </summary>
        public void ReserveDouble(string name) => this.Reserve(name, "Double", 8);

        /// <summary>
        /// Writes a eight-byte floating point number to a reserved position.
        /// </summary>
        public void FillDouble(string name, double value) {
            this.StepIn(this.Fill(name, "Double"));
            this.WriteDouble(value);
            this.StepOut();
        }
        #endregion

        #region String
        private void WriteChars(string text, Encoding encoding, bool terminate) {
            if (terminate) {
                text += '\0';
            }

            byte[] bytes = encoding.GetBytes(text);
            this.bw.Write(bytes);
        }

        /// <summary>
        /// Writes an ASCII string, with null terminator if specified.
        /// </summary>
        public void WriteASCII(string text, bool terminate = false) => this.WriteChars(text, SFEncoding.ASCII, terminate);

        /// <summary>
        /// Writes a Shift JIS string, with null terminator if specified.
        /// </summary>
        public void WriteShiftJIS(string text, bool terminate = false) => this.WriteChars(text, SFEncoding.ShiftJIS, terminate);

        /// <summary>
        /// Writes a UTF-16 string, with null terminator if specified.
        /// </summary>
        public void WriteUTF16(string text, bool terminate = false) {
            if (this.BigEndian) {
                this.WriteChars(text, SFEncoding.UTF16BE, terminate);
            } else {
                this.WriteChars(text, SFEncoding.UTF16, terminate);
            }
        }

        /// <summary>
        /// Writes a null-terminated Shift JIS string in a fixed-size field.
        /// </summary>
        public void WriteFixStr(string text, int size, byte padding = 0) {
            byte[] fixstr = new byte[size];
            for (int i = 0; i < size; i++) {
                fixstr[i] = padding;
            }

            byte[] bytes = SFEncoding.ShiftJIS.GetBytes(text + '\0');
            Array.Copy(bytes, fixstr, Math.Min(size, bytes.Length));
            this.bw.Write(fixstr);
        }

        /// <summary>
        /// Writes a null-terminated UTF-16 string in a fixed-size field.
        /// </summary>
        public void WriteFixStrW(string text, int size, byte padding = 0) {
            byte[] fixstr = new byte[size];
            for (int i = 0; i < size; i++) {
                fixstr[i] = padding;
            }

            byte[] bytes = this.BigEndian ? SFEncoding.UTF16BE.GetBytes(text + '\0') : SFEncoding.UTF16.GetBytes(text + '\0');
            Array.Copy(bytes, fixstr, Math.Min(size, bytes.Length));
            this.bw.Write(fixstr);
        }
        #endregion

        #region Other
        /// <summary>
        /// Writes a vector of two four-byte floating point numbers.
        /// </summary>
        public void WriteVector2(Vector2 vector) {
            this.WriteSingle(vector.X);
            this.WriteSingle(vector.Y);
        }

        /// <summary>
        /// Writes a vector of three four-byte floating point numbers.
        /// </summary>
        public void WriteVector3(Vector3 vector) {
            this.WriteSingle(vector.X);
            this.WriteSingle(vector.Y);
            this.WriteSingle(vector.Z);
        }

        /// <summary>
        /// Writes a vector of four four-byte floating point numbers.
        /// </summary>
        public void WriteVector4(Vector4 vector) {
            this.WriteSingle(vector.X);
            this.WriteSingle(vector.Y);
            this.WriteSingle(vector.Z);
            this.WriteSingle(vector.W);
        }

        /// <summary>
        /// Write length number of the given value.
        /// </summary>
        public void WritePattern(int length, byte pattern) {
            byte[] bytes = new byte[length];
            if (pattern != 0) {
                for (int i = 0; i < length; i++) {
                    bytes[i] = pattern;
                }
            }
            this.WriteBytes(bytes);
        }

        /// <summary>
        /// Writes a 4-byte color in ARGB order.
        /// </summary>
        public void WriteARGB(Color color) {
            this.bw.WriteValueType(color.A);
            this.bw.WriteValueType(color.R);
            this.bw.WriteValueType(color.G);
            this.bw.WriteValueType(color.B);
        }

        /// <summary>
        /// Writes a 4-byte color in ABGR order.
        /// </summary>
        public void WriteABGR(Color color) {
            this.bw.WriteValueType(color.A);
            this.bw.WriteValueType(color.B);
            this.bw.WriteValueType(color.G);
            this.bw.WriteValueType(color.R);
        }

        /// <summary>
        /// Writes a 4-byte color in RGBA order.
        /// </summary>
        public void WriteRGBA(Color color) {
            this.bw.WriteValueType(color.R);
            this.bw.WriteValueType(color.G);
            this.bw.WriteValueType(color.B);
            this.bw.WriteValueType(color.A);
        }

        /// <summary>
        /// Writes a 4-byte color in BGRA order.
        /// </summary>
        public void WriteBGRA(Color color) {
            this.bw.WriteValueType(color.B);
            this.bw.WriteValueType(color.G);
            this.bw.WriteValueType(color.R);
            this.bw.WriteValueType(color.A);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected internal virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    this.Finish();
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~BinaryWriterEx()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }
        
        /// <summary>
        /// 
        /// </summary>
        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
