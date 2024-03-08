namespace SoulsFormats.Util {
    /// <summary>
    /// Extensions for <see cref="BinaryWriter" />
    /// </summary>
    [SkipLocalsInit]
    public static unsafe partial class BinaryWriterExtensions {
        /// <summary>
        /// Writes struct-type value to BinaryWriter efficiently
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValueType<T>(this BinaryWriter writer, T value) where T : struct =>
            writer.GetInternalStream().Write(new ReadOnlySpan<byte>(&value, sizeof(T)));
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="writer"></param>
        /// <param name="span"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteSpan<T>(this BinaryWriter writer, ReadOnlySpan<T> span) =>
            writer.GetInternalStream().Write(new ReadOnlySpan<byte>(span.AsPointer(), span.Length * sizeof(T)));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="span"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteSpan(this BinaryWriter writer, ReadOnlySpan<byte> span) =>
            writer.GetInternalStream().Write(span);
    }
}
