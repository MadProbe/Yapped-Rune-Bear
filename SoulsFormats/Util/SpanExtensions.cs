namespace SoulsFormats.Util {
    /// <summary>
    /// 
    /// </summary>
    [SkipLocalsInit]
    public static unsafe partial class SpanExtensions {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <param name="span"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> AsBytes<TFrom>(this ReadOnlySpan<TFrom> span) => new (span.AsPointer(), span.Length * sizeof(TFrom));
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <param name="span"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> AsBytes<TFrom>(this Span<TFrom> span) => new (span.AsPointer(), span.Length * sizeof(TFrom));
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTo"></typeparam>
        /// <param name="span"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<TTo> As<TFrom, TTo>(this ReadOnlySpan<TFrom> span) =>
            new(span.AsPointer<TFrom, TTo>(),
                sizeof(TFrom) == sizeof(TTo) ?
                    span.Length :
                    sizeof(TFrom) == 1 ?
                        span.Length / sizeof(TTo) :
                        sizeof(TTo) == 1 ?
                            span.Length * sizeof(TFrom) :
                            span.Length * sizeof(TFrom) / sizeof(TTo));
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTo"></typeparam>
        /// <param name="span"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<TTo> As<TFrom, TTo>(this Span<TFrom> span) =>
            new(span.AsPointer<TFrom, TTo>(),
                sizeof(TFrom) == sizeof(TTo) ?
                    span.Length :
                    sizeof(TFrom) == 1 ?
                        span.Length / sizeof(TTo) :
                        sizeof(TTo) == 1 ?
                            span.Length * sizeof(TFrom) :
                            span.Length * sizeof(TFrom) / sizeof(TTo));
    }
}
