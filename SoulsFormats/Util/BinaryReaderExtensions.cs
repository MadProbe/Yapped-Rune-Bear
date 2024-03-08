namespace SoulsFormats.Util {
    /// <summary>
    /// BinaryReader extensions for efficient read from it
    /// </summary>
    [SkipLocalsInit]
    public static unsafe class BinaryReaderExtensions {
        /// <summary>
        /// Reads value of struct type effectively from BinaryReader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadValueTypeEffectively<T>(this BinaryReader reader) where T : struct {
            T result;
            reader.ReadExactly(new Span<byte>(&result, sizeof(T)));
            return result;
        }
        /// <summary>
        /// Read exactly length of span bytes to span specified, throw error otherwise
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="span"></param>
        /// <exception cref="EndOfStreamException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReadExactly(this BinaryReader reader, Span<byte> span) {
            for (int length = span.Length, read; (length -= read = reader.GetInternalStreamFast().Read(span)) > 0; span = span[read..]) {
                if (read == 0) {
                    throw new EndOfStreamException();
                }
            }
        }
    }
}
