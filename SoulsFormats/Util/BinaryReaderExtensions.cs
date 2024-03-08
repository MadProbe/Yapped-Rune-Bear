namespace SoulsFormats.Util {
    /// <summary>
    ///     BinaryReader extensions for efficient read from it
    /// </summary>
    [SkipLocalsInit]
    public static unsafe class BinaryReaderExtensions {
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_length")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern ref int GetLengthRef(MemoryStream stream);
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_origin")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern ref int GetOriginRef(MemoryStream stream);
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_position")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern ref int GetPositionRef(MemoryStream stream);
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_buffer")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern ref byte[] GetBufferRef(MemoryStream stream);
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_stream")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern ref Stream GetStreamRef(this BinaryReader reader);
        /// <summary>
        ///     Reads value of struct type effectively from BinaryReader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadValueTypeEffectively<T>(this BinaryReader reader) where T : struct {
            if (GetStreamRef(reader) is MemoryStream stream) {
                int position = GetPositionRef(stream) + GetOriginRef(stream);

                //if (GetLengthRef(stream) < position + sizeof(T)) {
                //    throw new EndOfStreamException();
                //}
                T value = Unsafe.As<byte, T>(ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(GetBufferRef(stream)),
                                                            position));
                GetPositionRef(stream) += sizeof(T);
                return value;
            }

            Unsafe.SkipInit(out T result);
            reader.ReadExactly(MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref result), sizeof(T)));
            return result;
        }

        /// <summary>
        ///     Reads value of struct type effectively from BinaryReader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReadValueTypeEffectively<T>(this BinaryReader reader, out T value) where T : struct {
            Unsafe.SkipInit(out value);
            reader.ReadExactly(new Span<byte>(AsPointer(ref value), sizeof(T)));
        }

        /// <summary>
        ///     Read exactly length of span bytes to span specified, throw error otherwise
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="span"></param>
        /// <exception cref="EndOfStreamException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReadExactly(this BinaryReader reader, Span<byte> span) {
            for (int length = span.Length, read; (length -= read = GetStreamRef(reader).Read(span)) > 0; span = span[read..])
                if (read == 0)
                    throw new EndOfStreamException();
        }
    }
}
