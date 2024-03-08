using static System.Runtime.CompilerServices.Unsafe;
using static System.Runtime.InteropServices.MemoryMarshal;

namespace MadProbe.Adler32 {
    [SkipLocalsInit]
    public static unsafe class Adler32Implementation {
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        [FixedAddressValueType] private static byte __zero__ = 0;
        [FixedAddressValueType] public static Vector256<byte> zeroes = Vector256<byte>.Zero;
        public static ulong[] __constants__ = AllocatePinnedArray<ulong>([1, 0xFFF1, 0x0000_0000_434B_F173, 0x0000_0000_4003_C038]);
        [FixedAddressValueType] public static readonly nuint __one__ = (nuint)Unsafe.AsPointer(ref __constants__[0]);
        [FixedAddressValueType] public static readonly nuint __divisor__ = (nuint)Unsafe.AsPointer(ref __constants__[1]);
        [FixedAddressValueType] public static readonly nuint __y0__ = (nuint)Unsafe.AsPointer(ref __constants__[2]);
        [FixedAddressValueType] public static readonly nuint __y1__ = (nuint)Unsafe.AsPointer(ref __constants__[3]);
        public static ReadOnlySpan<ulong> y1 => new[]
            { 0x0000_0000_4003_C038ul, 0x0000_0000_4003_C038ul, 0x0000_0000_4003_C038ul, 0x0000_0000_4003_C038ul };
        public static ReadOnlySpan<ulong> y0 => new[]
            { 0x0000_0000_434B_F173ul, 0x0000_0000_434B_F173ul, 0x0000_0000_434B_F173ul, 0x0000_0000_434B_F173ul };
        public static ReadOnlySpan<ulong> ones => new[] { 1ul, 1ul, 1ul, 1ul };
        public static ReadOnlySpan<ulong> y    => new[] { 0x80078071ul, 0x80078071ul, 0x80078071ul, 0x80078071ul };

        public static T[] AllocatePinnedArray<T>(ReadOnlySpan<T> entries) {
            T[] array = GC.AllocateUninitializedArray<T>(entries.Length, true);
            entries.CopyTo(array);
            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Prefetch0(ref byte p) => Sse.Prefetch0(Unsafe.AsPointer(ref p));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint NativeLength(this Array @this) => Add(ref As<byte, nuint>(ref GetArrayDataReference(@this)), -1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<ulong> MultiplyHigh(Vector256<ulong> x,
                                                    Vector256<ulong> y0,
                                                    Vector256<ulong> y1,
                                                    Vector256<ulong> zero) {
            // #region affected by outer lisence code
            // original code snippet is from https://github.com/ridiculousfish/libdivide/blob/master/libdivide.h#L2335, ported to C#
            // License: zlib License
            // Copyright(C) 2010 - 2019 ridiculous_fish, <libdivide@ridiculousfish.com>
            // Copyright(C) 2016 - 2019 Kim Walisch, <kim.walisch@gmail.com>
            // For full version see https://github.com/ridiculousfish/libdivide/blob/3bd34388573681ce563348cdf04fe15d24770d04/LICENSE.txt
            Vector256<ulong> x0y0    = Avx2.Multiply(x.AsUInt32(), y0.AsUInt32());
            Vector256<uint>  x1      = Avx2.UnpackHigh(x.AsUInt32(), zero.AsUInt32());
            Vector256<ulong> x0y0_hi = x0y0 >>> 32;
            Vector256<ulong> x1y0    = Avx2.Multiply(x1, y0.AsUInt32());
            Vector256<ulong> x1y1    = Avx2.Multiply(x1, y1.AsUInt32());
            Vector256<ulong> temp    = x1y0 + x0y0_hi;
            Vector256<ulong> x0y1    = Avx2.Multiply(y1.AsUInt32(), x.AsUInt32());
            Vector256<ulong> temp_hi = Avx2.UnpackHigh(temp.AsUInt32(), zero.AsUInt32()).AsUInt64();
            Vector256<ulong> temp_lo = Avx2.Blend(temp.AsUInt32(), zero.AsUInt32(), 0b01010101).AsUInt64();

            temp_lo =  x0y1 + temp_lo >>> 32;
            temp_hi += x1y1;
            return temp_lo + temp_hi;
            // #endregion affected by outer lisence code
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<ulong> MultiplyLow(Vector256<ulong> x,
                                                   Vector256<ulong> y,
                                                   Vector256<ulong> zero) {
            // #region affected by outer lisence code
            // original code is taken from this SO answer and ported to C# & vectorized
            // https://stackoverflow.com/a/51587262/12983298
            // Lisence: CC BY-SA 4.0
            if (Avx512DQ.VL.IsSupported) return Avx512DQ.VL.MultiplyLow(x, y);

            //Vector256<ulong> x0y0   = Avx2.Multiply(x.AsUInt32(), y.AsUInt32()),
            //                 x0y1   = Avx2.Multiply(x.AsUInt32(), Avx2.UnpackHigh(y.AsUInt32(), zero.AsUInt32())),
            //                 x1y0   = Avx2.Multiply(y.AsUInt32(), Avx2.UnpackHigh(x.AsUInt32(), zero.AsUInt32())),
            //                 middle = x1y0 + (x0y1 & Vector256.Create(0x0000_0000_ffff_fffful)) + (x0y0 >> 32);
            //return (middle << 32) | (x0y0 & Vector256.Create(0x0000_0000_ffff_fffful));
            Vector256<ulong> x0y0 =
                                 Avx2.Multiply(x.AsUInt32(), y.AsUInt32()),
                             x0y1 = Avx2.Multiply(x.AsUInt32(), Avx2.UnpackHigh(y.AsUInt32(), zero.AsUInt32())),
                             x1y0 = Avx2.Multiply(y.AsUInt32(), Avx2.UnpackHigh(x.AsUInt32(), zero.AsUInt32())),
                             middle =
                                 x1y0 + x0y1;                // x0y1 high 32 bits won't matter as they got removed by shift to left by 32
            return Avx2.ShiftLeftLogical(middle, 32) + x0y0; // folded to just + x0y0
            // #endregion affected by outer lisence code
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<ulong> MultiplyLow64By32(Vector256<ulong> x,
                                                         Vector256<ulong> y,
                                                         Vector256<ulong> zero) {
            // #region affected by outer lisence code
            // original code is taken from this SO answer and ported to C# & vectorized
            // https://stackoverflow.com/a/51587262/12983298
            // Lisence: CC BY-SA 4.0
            if (Avx512DQ.VL.IsSupported) return Avx512DQ.VL.MultiplyLow(x, y);

            Vector256<ulong> x0y0 =
                                 Avx2.Multiply(x.AsUInt32(), y.AsUInt32()),
                             x1y0 = Avx2.Multiply(y.AsUInt32(),
                                                  Avx2.UnpackHigh(x.AsUInt32(),
                                                                  zero.AsUInt32()));
            return Avx2.ShiftLeftLogical(x1y0, 32) + x0y0; // folded to just + x0y0
            // #endregion affected by outer lisence code
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Adler32<T>(T[] data) =>
            Adler32(ref As<T, byte>(ref GetArrayDataReference(data)), data.NativeLength() * (nuint)SizeOf<T>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Adler32<T>(ReadOnlySpan<T> data) =>
            Adler32(ref As<T, byte>(ref GetReference(data)), (nuint)(data.Length * SizeOf<T>()));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Adler32<T>(Span<T> data) =>
            Adler32(ref As<T, byte>(ref GetReference(data)), (nuint)(data.Length * SizeOf<T>()));

        public static uint Adler32(ref byte dataRef, nuint length) => Adler32Inlined(ref dataRef, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Adler32Inlined(ref byte dataRef, nuint length) =>
            #if NET8_0_OR_GREATER
            Avx512BW.IsSupported && Avx512F.IsSupported
                ? Avx512Impl(ref dataRef, length)
                :
                #endif
                Avx2.IsSupported
                    ? Avx2Impl(ref dataRef, length)
                    : ScalarImpl(ref dataRef, length);

        // pretty crappy remainder realization but whatcha gonna do :/
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector128<ulong> FastVectorRemainder(Vector128<ulong> part, uint div) =>
            Sse3.X64.IsSupported
                ? Vector128.Create(part[0] % div, part[1] % div)
                : Vector128
                  .Create(X86Base.DivRem(part.AsUInt32()[0], part.AsUInt32()[1], div).Remainder,
                          0u,
                          X86Base.DivRem(part.AsUInt32()[2], part.AsUInt32()[3], div).Remainder,
                          0u).AsUInt64();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<ulong>
            FastVectorRemainder(Vector256<ulong> vector256, uint div) =>
            Vector256.Create(FastVectorRemainder(vector256.GetLower(), div),
                             FastVectorRemainder(vector256.GetUpper(), div));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<ulong> FastVectorRemainderFFF1(Vector256<ulong> vector256,
                                                               Vector256<ulong> zero,
                                                               Vector256<ulong> ones,
                                                               Vector256<ulong> y0,
                                                               Vector256<ulong> y1,
                                                               Vector256<ulong> divisor) {
            Vector256<ulong> added = vector256 + ones;
            added |= Avx2.CompareEqual(vector256, zero); // restore original value if we overflowed
            return vector256 - MultiplyLow64By32(Avx2.ShiftRightLogical(MultiplyHigh(added, y0, y1, zero), 0xe),
                                                 divisor, zero);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<ulong> FastVectorRemainderFFF1(Vector256<ulong> vector256,
                                                               Vector256<ulong> y,
                                                               Vector256<ulong> divisor) =>
            vector256 - Avx2.Multiply(Avx2.ShiftRightLogical(Avx2.Multiply(vector256.AsUInt32(), y.AsUInt32()), 0x2f).AsUInt32(), divisor.AsUInt32());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong __sum_helper__(Vector128<ulong> vector) =>
            Sse3.X64.IsSupported
                ? vector[0]
                : vector.AsUInt32()[0] | ((ulong)vector.AsUInt32()[1] << 32);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong FastVectorSum(Vector128<ulong> vector) =>
            __sum_helper__(Sse2.ShiftRightLogical128BitLane(vector, 64) + vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong FastVectorSum(Vector256<ulong> vector) =>
            FastVectorSum(vector.GetUpper() + vector.GetLower());

        #if NET8_0_OR_GREATER

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Avx512Impl(ref byte dataRef, nuint length) {
            const int overflowPoint = 0x20_0000;
            ulong     adlerA        = 1, adlerB = 0;

            if (length >= (nuint)Vector512<byte>.Count) {
                nuint           i    = 0, limit = length & (nuint)~(Vector512<byte>.Count - 1);
                Vector512<byte> zero = Vector512.Create<byte>(__zero__);
                var weights = Vector512.Create(64, 63, 62, 61, 60, 59, 58, 57, 56, 55, 54, 53, 52, 51, 50, 49,
                                               48, 47, 46, 45, 44, 43, 42, 41, 40, 39, 38, 37, 36, 35, 34, 33,
                                               32, 31, 30, 29, 28, 27, 26, 25, 24, 23, 22, 21, 20, 19, 18, 17,
                                               16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1);
                Vector512<ulong> vAdlerA            = Vector512.CreateScalar(1ul),
                                 vAdlerBMultWidened = zero.AsUInt64(),
                                 vAdlerBA           = Vector512<ulong>.Zero;

                do {
                    nuint           elementOffset = Math.Min(overflowPoint, limit - i);
                    ref byte        end           = ref Add(ref dataRef, elementOffset);
                    Vector512<uint> vAdlerBMult   = zero.AsUInt32();


                    for (ref byte end_8_pairs =
                             ref Add(ref dataRef, elementOffset & (nuint)~(Vector512<byte>.Count * 8 - 1));
                         IsAddressLessThan(ref dataRef, ref end_8_pairs);
                         dataRef = ref Add(ref dataRef, Vector512<byte>.Count * 8)) {
                        Prefetch0(ref Add(ref dataRef, Vector512<byte>.Count * 24));
                        Prefetch0(ref Add(ref dataRef, Vector512<byte>.Count * 25));
                        Prefetch0(ref Add(ref dataRef, Vector512<byte>.Count * 26));
                        Prefetch0(ref Add(ref dataRef, Vector512<byte>.Count * 27));
                        Prefetch0(ref Add(ref dataRef, Vector512<byte>.Count * 28));
                        Prefetch0(ref Add(ref dataRef, Vector512<byte>.Count * 29));
                        Prefetch0(ref Add(ref dataRef, Vector512<byte>.Count * 30));
                        Prefetch0(ref Add(ref dataRef, Vector512<byte>.Count * 31));

                        Vector512<byte> bytes = Vector512.LoadUnsafe(ref dataRef);
                        Vector512<byte> bytes2 =
                            Vector512.LoadUnsafe(ref Add(ref dataRef, Vector512<byte>.Count * 1));
                        Vector512<byte> bytes3 =
                            Vector512.LoadUnsafe(ref Add(ref dataRef, Vector512<byte>.Count * 2));
                        Vector512<byte> bytes4 =
                            Vector512.LoadUnsafe(ref Add(ref dataRef, Vector512<byte>.Count * 3));
                        Vector512<byte> bytes5 =
                            Vector512.LoadUnsafe(ref Add(ref dataRef, Vector512<byte>.Count * 4));
                        Vector512<byte> bytes6 =
                            Vector512.LoadUnsafe(ref Add(ref dataRef, Vector512<byte>.Count * 5));
                        Vector512<byte> bytes7 =
                            Vector512.LoadUnsafe(ref Add(ref dataRef, Vector512<byte>.Count * 6));
                        Vector512<byte> bytes8 =
                            Vector512.LoadUnsafe(ref Add(ref dataRef, Vector512<byte>.Count * 7));

                        // Until https://github.com/dotnet/runtime/issues/86849 is approved and implemented this code will stay commented out
                        // if (Avx512Vnni.IsSupported) {
                        //     vAdlerBA += vAdlerA;
                        //     vAdlerBMult = Avx512Vnni.MultiplyWideningAndAdd(vAdlerBMult.AsInt32(), bytes, weights).AsUInt32();
                        //     vAdlerA += Avx512BW.SumAbsoluteDifferences(bytes, zero).AsUInt64();
                        //     vAdlerBA += vAdlerA;
                        //     vAdlerBMult = Avx512Vnni.MultiplyWideningAndAdd(vAdlerBMult.AsInt32(), bytes2, weights).AsUInt32();
                        //     vAdlerA += Avx512BW.SumAbsoluteDifferences(bytes2, zero).AsUInt64();
                        //     vAdlerBA += vAdlerA;
                        //     vAdlerBMult = Avx512Vnni.MultiplyWideningAndAdd(vAdlerBMult.AsInt32(), bytes3, weights).AsUInt32();
                        //     vAdlerA += Avx512BW.SumAbsoluteDifferences(bytes3, zero).AsUInt64();
                        //     vAdlerBA += vAdlerA;
                        //     vAdlerBMult = Avx512Vnni.MultiplyWideningAndAdd(vAdlerBMult.AsInt32(), bytes4, weights).AsUInt32();
                        //     vAdlerA += Avx512BW.SumAbsoluteDifferences(bytes4, zero).AsUInt64();
                        //     vAdlerBA += vAdlerA;
                        //     vAdlerBMult = Avx512Vnni.MultiplyWideningAndAdd(vAdlerBMult.AsInt32(), bytes5, weights).AsUInt32();
                        //     vAdlerA += Avx512BW.SumAbsoluteDifferences(bytes5, zero).AsUInt64();
                        //     vAdlerBA += vAdlerA;
                        //     vAdlerBMult = Avx512Vnni.MultiplyWideningAndAdd(vAdlerBMult.AsInt32(), bytes6, weights).AsUInt32();
                        //     vAdlerA += Avx512BW.SumAbsoluteDifferences(bytes6, zero).AsUInt64();
                        //     vAdlerBA += vAdlerA;
                        //     vAdlerBMult = Avx512Vnni.MultiplyWideningAndAdd(vAdlerBMult.AsInt32(), bytes7, weights).AsUInt32();
                        //     vAdlerA += Avx512BW.SumAbsoluteDifferences(bytes7, zero).AsUInt64();
                        //     vAdlerBA += vAdlerA;
                        //     vAdlerBMult = Avx512Vnni.MultiplyWideningAndAdd(vAdlerBMult.AsInt32(), bytes8, weights).AsUInt32();
                        //     vAdlerA += Avx512BW.SumAbsoluteDifferences(bytes8, zero).AsUInt64();
                        // } else {
                        Vector512<uint>   vAdlerBMultTempSum, vAdlerBMultTempSum2;
                        Vector512<ulong>  vAdlerASumTemp,     vAdlerASumTemp2;
                        Vector512<ushort> vAdlerBMultTemp,    vAdlerBMultTemp2;
                        vAdlerBMultTemp  =  Avx512BW.MultiplyAddAdjacent(bytes,  weights).AsUInt16();
                        vAdlerBMultTemp2 =  Avx512BW.MultiplyAddAdjacent(bytes2, weights).AsUInt16();
                        vAdlerASumTemp   =  Avx512BW.SumAbsoluteDifferences(bytes,  zero).AsUInt64();
                        vAdlerASumTemp2  =  Avx512BW.SumAbsoluteDifferences(bytes2, zero).AsUInt64();
                        vAdlerBA         += vAdlerA;
                        vAdlerBMultTemp  += vAdlerBMultTemp2;
                        vAdlerA          += vAdlerASumTemp;
                        vAdlerBMultTempSum =
                            Avx512BW.MultiplyAddAdjacent(vAdlerBMultTemp.AsInt16(),
                                                         Vector512<short>.One).AsUInt32();
                        vAdlerBMultTempSum2 =
                            Avx512BW.MultiplyAddAdjacent(vAdlerBMultTemp2.AsInt16(),
                                                         Vector512<short>.One).AsUInt32();
                        vAdlerBA           += vAdlerA;
                        vAdlerBMultTempSum += vAdlerBMultTempSum2;
                        vAdlerA            += vAdlerASumTemp2;
                        vAdlerBMultTemp    =  Avx512BW.MultiplyAddAdjacent(bytes3, weights).AsUInt16();
                        vAdlerBMultTemp2   =  Avx512BW.MultiplyAddAdjacent(bytes4, weights).AsUInt16();
                        vAdlerBMult        += vAdlerBMultTempSum;
                        vAdlerASumTemp     =  Avx512BW.SumAbsoluteDifferences(bytes3, zero).AsUInt64();
                        vAdlerASumTemp2    =  Avx512BW.SumAbsoluteDifferences(bytes4, zero).AsUInt64();
                        vAdlerBA           += vAdlerA;
                        vAdlerBMultTemp    += vAdlerBMultTemp2;
                        vAdlerA            += vAdlerASumTemp;
                        vAdlerBMultTempSum =
                            Avx512BW.MultiplyAddAdjacent(vAdlerBMultTemp.AsInt16(),
                                                         Vector512<short>.One).AsUInt32();
                        vAdlerBMultTempSum2 =
                            Avx512BW.MultiplyAddAdjacent(vAdlerBMultTemp2.AsInt16(),
                                                         Vector512<short>.One).AsUInt32();
                        vAdlerBA           += vAdlerA;
                        vAdlerBMultTempSum += vAdlerBMultTempSum2;
                        vAdlerA            += vAdlerASumTemp2;
                        vAdlerBMult        += vAdlerBMultTempSum;
                        vAdlerBMultTemp    =  Avx512BW.MultiplyAddAdjacent(bytes5, weights).AsUInt16();
                        vAdlerBMultTemp2   =  Avx512BW.MultiplyAddAdjacent(bytes6, weights).AsUInt16();
                        vAdlerASumTemp     =  Avx512BW.SumAbsoluteDifferences(bytes5, zero).AsUInt64();
                        vAdlerASumTemp2    =  Avx512BW.SumAbsoluteDifferences(bytes6, zero).AsUInt64();
                        vAdlerBA           += vAdlerA;
                        vAdlerBMultTemp    += vAdlerBMultTemp2;
                        vAdlerA            += vAdlerASumTemp;
                        vAdlerBMultTempSum =
                            Avx512BW.MultiplyAddAdjacent(vAdlerBMultTemp.AsInt16(),
                                                         Vector512<short>.One).AsUInt32();
                        vAdlerBMultTempSum2 =
                            Avx512BW.MultiplyAddAdjacent(vAdlerBMultTemp2.AsInt16(),
                                                         Vector512<short>.One).AsUInt32();
                        vAdlerBA           += vAdlerA;
                        vAdlerBMultTempSum += vAdlerBMultTempSum2;
                        vAdlerA            += vAdlerASumTemp2;
                        vAdlerBMult        += vAdlerBMultTempSum;
                        vAdlerBMultTemp    =  Avx512BW.MultiplyAddAdjacent(bytes7, weights).AsUInt16();
                        vAdlerBMultTemp2   =  Avx512BW.MultiplyAddAdjacent(bytes8, weights).AsUInt16();
                        vAdlerASumTemp     =  Avx512BW.SumAbsoluteDifferences(bytes7, zero).AsUInt64();
                        vAdlerASumTemp2    =  Avx512BW.SumAbsoluteDifferences(bytes8, zero).AsUInt64();
                        vAdlerBA           += vAdlerA;
                        vAdlerBMultTemp    += vAdlerBMultTemp2;
                        vAdlerA            += vAdlerASumTemp;
                        vAdlerBMultTempSum =
                            Avx512BW.MultiplyAddAdjacent(vAdlerBMultTemp.AsInt16(),
                                                         Vector512<short>.One).AsUInt32();
                        vAdlerBMultTempSum2 =
                            Avx512BW.MultiplyAddAdjacent(vAdlerBMultTemp2.AsInt16(),
                                                         Vector512<short>.One).AsUInt32();
                        vAdlerBA           += vAdlerA;
                        vAdlerBMultTempSum += vAdlerBMultTempSum2;
                        vAdlerA            += vAdlerASumTemp2;
                        vAdlerBMult        += vAdlerBMultTempSum;
                        // }
                    }

                    while (IsAddressLessThan(ref dataRef, ref end)) {
                        Vector512<byte> bytes = Vector512.LoadUnsafe(ref dataRef);
                        vAdlerBA += vAdlerA;
                        vAdlerA  += Avx512BW.SumAbsoluteDifferences(bytes, zero).AsUInt64();

                        // if (Avx512Vnni.IsSupported)
                        //     vAdlerBMult = Avx512Vnni.MultiplyWideningAndAdd(vAdlerBMult.AsInt32(), bytes, weights).AsUInt32();
                        // else
                        vAdlerBMult +=
                            Avx512BW
                                .MultiplyAddAdjacent(Avx512BW.MultiplyAddAdjacent(bytes, weights),
                                                     Vector512<short>.One).AsUInt32();

                        dataRef = ref Add(ref dataRef, Vector512<byte>.Count);
                    }

                    vAdlerBMultWidened += Vector512.WidenLower(vAdlerBMult) +
                                          Vector512.WidenUpper(vAdlerBMult);
                    vAdlerA  =  FastVectorRemainder(vAdlerA,  0xFFF1);
                    vAdlerBA =  FastVectorRemainder(vAdlerBA, 0xFFF1);
                    i        += overflowPoint;
                } while (i < limit);

                adlerA = FastVectorSum(vAdlerA);
                adlerB = FastVectorSum((vAdlerBA << 6 /* * 64 */) + vAdlerBMultWidened);
            }

            for (ref byte end = ref Add(ref dataRef, length & (nuint)(Vector512<byte>.Count - 1));
                 IsAddressLessThan(ref dataRef, ref end);
                 dataRef = ref Add(ref dataRef, 1)) {
                adlerA += dataRef;
                adlerB += adlerA;
            }

            return (uint)((adlerB % 0xFFF1 << 16) | (adlerA % 0xFFF1));
        }

        #endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Avx2Impl(ref byte dataRef, nuint length) {
            ulong adlerA = 1, adlerB = 0;

            if (length >= (nuint)Vector256<byte>.Count) {
                nuint           i    = 0, limit = length & (nuint)~(Vector256<byte>.Count - 1);
                Vector256<byte> zero = Vector256.Create(__zero__);
                var weights = Vector256.Create(32, 31, 30, 29, 28, 27, 26, 25, 24, 23, 22, 21, 20, 19, 18, 17,
                                               16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1);
                Vector256<ulong> vAdlerA            = Vector256.CreateScalar(1ul),
                                 vAdlerBMultWidened = zero.AsUInt64(),
                                 vAdlerBA           = Vector256<ulong>.Zero;

                do {
                    const int       overflowPoint = 0x20_0000;
                    nuint           elementOffset = Math.Min(overflowPoint, limit - i);
                    ref byte        end           = ref Add(ref dataRef, elementOffset);
                    Vector256<uint> vAdlerBMult   = Vector256<uint>.Zero;

                    // On 32-bit targets there are only 8 avx registers available
                    // so doing this will cause lots of register spills and
                    // performance will plummet into oblivion
                    if (Avx2.X64.IsSupported)
                        for (ref byte end_8_pairs =
                                 ref Add(ref dataRef, elementOffset & (nuint)~(Vector256<byte>.Count * 8 - 1));
                             IsAddressLessThan(ref dataRef, ref end_8_pairs);
                             dataRef = ref Add(ref dataRef, Vector256<byte>.Count * 8)) {
                            Prefetch0(ref Add(ref dataRef, Vector256<byte>.Count * 24));
                            Prefetch0(ref Add(ref dataRef, Vector256<byte>.Count * 26));
                            Prefetch0(ref Add(ref dataRef, Vector256<byte>.Count * 28));
                            Prefetch0(ref Add(ref dataRef, Vector256<byte>.Count * 30));

                            Vector256<byte> bytes = Vector256.LoadUnsafe(ref dataRef);
                            Vector256<byte> bytes2 =
                                Vector256.LoadUnsafe(ref Add(ref dataRef, Vector256<byte>.Count * 1));

                            if (AvxVnni.IsSupported) {
                                Vector256<byte>  bytes3 = Vector256.LoadUnsafe(ref Add(ref dataRef, Vector256<byte>.Count * 2));
                                Vector256<byte>  bytes4 = Vector256.LoadUnsafe(ref Add(ref dataRef, Vector256<byte>.Count * 3));
                                Vector256<byte>  bytes5 = Vector256.LoadUnsafe(ref Add(ref dataRef, Vector256<byte>.Count * 4));
                                Vector256<byte>  bytes6 = Vector256.LoadUnsafe(ref Add(ref dataRef, Vector256<byte>.Count * 5));
                                Vector256<byte>  bytes7 = Vector256.LoadUnsafe(ref Add(ref dataRef, Vector256<byte>.Count * 6));
                                Vector256<byte>  bytes8 = Vector256.LoadUnsafe(ref Add(ref dataRef, Vector256<byte>.Count * 7));
                                Vector256<ulong> vAdlerSum;
                                vAdlerSum   =  Avx2.SumAbsoluteDifferences(bytes, zero).AsUInt64();
                                vAdlerBA    += vAdlerA;
                                vAdlerBMult =  AvxVnni.MultiplyWideningAndAdd(vAdlerBMult.AsInt32(), bytes, weights).AsUInt32();
                                vAdlerA     += vAdlerSum;
                                vAdlerSum   =  Avx2.SumAbsoluteDifferences(bytes2, zero).AsUInt64();
                                vAdlerBA    += vAdlerA;
                                vAdlerBMult =  AvxVnni.MultiplyWideningAndAdd(vAdlerBMult.AsInt32(), bytes2, weights).AsUInt32();
                                vAdlerA     += vAdlerSum;
                                vAdlerSum   =  Avx2.SumAbsoluteDifferences(bytes3, zero).AsUInt64();
                                vAdlerBA    += vAdlerA;
                                vAdlerBMult =  AvxVnni.MultiplyWideningAndAdd(vAdlerBMult.AsInt32(), bytes3, weights).AsUInt32();
                                vAdlerA     += vAdlerSum;
                                vAdlerSum   =  Avx2.SumAbsoluteDifferences(bytes4, zero).AsUInt64();
                                vAdlerBA    += vAdlerA;
                                vAdlerBMult =  AvxVnni.MultiplyWideningAndAdd(vAdlerBMult.AsInt32(), bytes4, weights).AsUInt32();
                                vAdlerA     += vAdlerSum;
                                vAdlerSum   =  Avx2.SumAbsoluteDifferences(bytes5, zero).AsUInt64();
                                vAdlerBA    += vAdlerA;
                                vAdlerBMult =  AvxVnni.MultiplyWideningAndAdd(vAdlerBMult.AsInt32(), bytes5, weights).AsUInt32();
                                vAdlerA     += vAdlerSum;
                                vAdlerSum   =  Avx2.SumAbsoluteDifferences(bytes6, zero).AsUInt64();
                                vAdlerBA    += vAdlerA;
                                vAdlerBMult =  AvxVnni.MultiplyWideningAndAdd(vAdlerBMult.AsInt32(), bytes6, weights).AsUInt32();
                                vAdlerA     += vAdlerSum;
                                vAdlerSum   =  Avx2.SumAbsoluteDifferences(bytes7, zero).AsUInt64();
                                vAdlerBA    += vAdlerA;
                                vAdlerBMult =  AvxVnni.MultiplyWideningAndAdd(vAdlerBMult.AsInt32(), bytes7, weights).AsUInt32();
                                vAdlerA     += vAdlerSum;
                                vAdlerSum   =  Avx2.SumAbsoluteDifferences(bytes8, zero).AsUInt64();
                                vAdlerBA    += vAdlerA;
                                vAdlerBMult =  AvxVnni.MultiplyWideningAndAdd(vAdlerBMult.AsInt32(), bytes8, weights).AsUInt32();
                                vAdlerA     += vAdlerSum;
                            } else {
                                Vector256<byte>   bytes3,          bytes4, bytes5, bytes6, bytes7, bytes8;
                                Vector256<ulong>  vAdlerASumTemp,  vAdlerASumTemp2;
                                Vector256<ushort> vAdlerBMultTemp, vAdlerBMultTemp2;
                                bytes3           =  Vector256.LoadUnsafe(ref Add(ref dataRef, Vector256<byte>.Count * 2));
                                bytes4           =  Vector256.LoadUnsafe(ref Add(ref dataRef, Vector256<byte>.Count * 3));
                                vAdlerBMultTemp  =  Avx2.MultiplyAddAdjacent(bytes,  weights).AsUInt16();
                                vAdlerBMultTemp2 =  Avx2.MultiplyAddAdjacent(bytes2, weights).AsUInt16();
                                vAdlerASumTemp   =  Avx2.SumAbsoluteDifferences(bytes, zero).AsUInt64();
                                vAdlerBMultTemp  += vAdlerBMultTemp2;
                                vAdlerASumTemp2  =  Avx2.SumAbsoluteDifferences(bytes2, zero).AsUInt64();
                                vAdlerBA         += vAdlerA;
                                vAdlerA          += vAdlerASumTemp;
                                vAdlerBA         += vAdlerA;
                                vAdlerA          += vAdlerASumTemp2;
                                bytes5           =  Avx.LoadVector256((byte*)Unsafe.AsPointer(ref Add(ref dataRef, Vector256<byte>.Count * 4)));
                                bytes6           =  Avx.LoadVector256((byte*)Unsafe.AsPointer(ref Add(ref dataRef, Vector256<byte>.Count * 5)));
                                bytes7           =  Avx.LoadVector256((byte*)Unsafe.AsPointer(ref Add(ref dataRef, Vector256<byte>.Count * 6)));
                                bytes8           =  Avx.LoadVector256((byte*)Unsafe.AsPointer(ref Add(ref dataRef, Vector256<byte>.Count * 7)));
                                vAdlerBMultTemp  += Avx2.MultiplyAddAdjacent(bytes3, weights).AsUInt16();
                                vAdlerASumTemp   =  Avx2.SumAbsoluteDifferences(bytes3, zero).AsUInt64();
                                vAdlerBMultTemp2 =  Avx2.MultiplyAddAdjacent(bytes4, weights).AsUInt16();
                                vAdlerBMultTemp  += vAdlerBMultTemp2;
                                vAdlerASumTemp2  =  Avx2.SumAbsoluteDifferences(bytes4, zero).AsUInt64();
                                vAdlerBMult      += Avx2.UnpackLow(vAdlerBMultTemp, zero.AsUInt16()).AsUInt32();
                                vAdlerBA         += vAdlerA;
                                vAdlerA          += vAdlerASumTemp;
                                vAdlerBA         += vAdlerA;
                                vAdlerBMult      += Avx2.UnpackHigh(vAdlerBMultTemp, zero.AsUInt16()).AsUInt32();
                                vAdlerA          += vAdlerASumTemp2;
                                vAdlerBMultTemp  =  Avx2.MultiplyAddAdjacent(bytes5, weights).AsUInt16();
                                vAdlerBMultTemp2 =  Avx2.MultiplyAddAdjacent(bytes6, weights).AsUInt16();
                                vAdlerASumTemp   =  Avx2.SumAbsoluteDifferences(bytes5, zero).AsUInt64();
                                vAdlerBMultTemp  += vAdlerBMultTemp2;
                                vAdlerASumTemp2  =  Avx2.SumAbsoluteDifferences(bytes6, zero).AsUInt64();
                                vAdlerBA         += vAdlerA;
                                vAdlerA          += vAdlerASumTemp;
                                vAdlerBA         += vAdlerA;
                                vAdlerA          += vAdlerASumTemp2;
                                vAdlerBMultTemp  += Avx2.MultiplyAddAdjacent(bytes7, weights).AsUInt16();
                                vAdlerASumTemp   =  Avx2.SumAbsoluteDifferences(bytes7, zero).AsUInt64();
                                vAdlerBMultTemp2 =  Avx2.MultiplyAddAdjacent(bytes8, weights).AsUInt16();
                                vAdlerBMultTemp  += vAdlerBMultTemp2;
                                vAdlerASumTemp2  =  Avx2.SumAbsoluteDifferences(bytes8, zero).AsUInt64();
                                vAdlerBMult      += Avx2.UnpackLow(vAdlerBMultTemp, zero.AsUInt16()).AsUInt32();
                                vAdlerBA         += vAdlerA;
                                vAdlerA          += vAdlerASumTemp;
                                vAdlerBA         += vAdlerA;
                                vAdlerBMult      += Avx2.UnpackHigh(vAdlerBMultTemp, zero.AsUInt16()).AsUInt32();
                                vAdlerA          += vAdlerASumTemp2;
                            }
                        }

                    while (IsAddressLessThan(ref dataRef, ref end)) {
                        Vector256<byte> bytes = Vector256.LoadUnsafe(ref dataRef);
                        vAdlerBA += vAdlerA;
                        vAdlerA  += Avx2.SumAbsoluteDifferences(bytes, zero).AsUInt64();

                        if (AvxVnni.IsSupported)
                            vAdlerBMult = AvxVnni.MultiplyWideningAndAdd(vAdlerBMult.AsInt32(), bytes, weights).AsUInt32();
                        else
                            vAdlerBMult += Avx2.MultiplyAddAdjacent(Avx2.MultiplyAddAdjacent(bytes, weights), Vector256<short>.One).AsUInt32();

                        dataRef = ref Add(ref dataRef, Vector256<byte>.Count);
                    }

                    Vector256<ulong> ones = Avx2.BroadcastScalarToVector256((ulong*)__one__);
                    Vector256<ulong> divisor =
                        Avx2.BroadcastScalarToVector256((ulong*)__divisor__);
                    Vector256<ulong> y0 = Avx2.BroadcastScalarToVector256((ulong*)__y0__);
                    Vector256<ulong> y1 = Avx2.BroadcastScalarToVector256((ulong*)__y1__);
                    vAdlerBMultWidened += (vAdlerBMult.AsUInt64() >>> 32) +
                                          Avx2.UnpackLow(vAdlerBMult, zero.AsUInt32()).AsUInt64();
                    vAdlerBA =  FastVectorRemainderFFF1(vAdlerBA, zero.AsUInt64(),     ones, y0, y1, divisor);
                    vAdlerA  =  FastVectorRemainderFFF1(vAdlerA,  Vector256.Create(y), divisor);
                    i        += overflowPoint;
                } while (i < limit);

                adlerA = FastVectorSum(vAdlerA);
                adlerB = FastVectorSum((vAdlerBA << 5 /* * 32 */) + vAdlerBMultWidened);
            }

            for (ref byte end = ref Add(ref dataRef, length & (nuint)(Vector256<byte>.Count - 1));
                 IsAddressLessThan(ref dataRef, ref end);
                 dataRef = ref Add(ref dataRef, 1)) {
                adlerA += dataRef;
                adlerB += adlerA;
            }

            return (uint)((adlerB % 0xFFF1 << 16) | ((uint)adlerA % 0xFFF1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ScalarImpl(ref byte dataRef, nuint length) {
            ulong adlerA = 1;
            ulong adlerB = 0;
            nuint i      = 0;

            do {
                nuint lengthWide = Math.Min(length - i, 0x400_0000);

                for (ref byte end = ref Add(ref dataRef, lengthWide - lengthWide % 0x10);
                     IsAddressLessThan(ref dataRef, ref end);
                     dataRef = ref Add(ref dataRef, 16)) {
                    adlerB += adlerA += dataRef;
                    adlerB += adlerA += Add(ref dataRef, 1);
                    adlerB += adlerA += Add(ref dataRef, 2);
                    adlerB += adlerA += Add(ref dataRef, 3);
                    adlerB += adlerA += Add(ref dataRef, 4);
                    adlerB += adlerA += Add(ref dataRef, 5);
                    adlerB += adlerA += Add(ref dataRef, 6);
                    adlerB += adlerA += Add(ref dataRef, 7);
                    adlerB += adlerA += Add(ref dataRef, 8);
                    adlerB += adlerA += Add(ref dataRef, 9);
                    adlerB += adlerA += Add(ref dataRef, 10);
                    adlerB += adlerA += Add(ref dataRef, 11);
                    adlerB += adlerA += Add(ref dataRef, 12);
                    adlerB += adlerA += Add(ref dataRef, 13);
                    adlerB += adlerA += Add(ref dataRef, 14);
                    adlerB += adlerA += Add(ref dataRef, 15);
                }

                for (ref byte end = ref Add(ref dataRef, lengthWide % 0x10);
                     IsAddressLessThan(ref dataRef, ref end);
                     dataRef = ref Add(ref dataRef, 1)) adlerB += adlerA += dataRef;

                i += 0x400_0000;

                adlerA %= 0xFFF1;
                adlerB %= 0xFFF1;
            } while (length - i > 0);

            return (uint)((adlerB << 16) | adlerA);
        }

        #if NET8_0_OR_GREATER

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong FastVectorSum(Vector512<ulong> vector) =>
            FastVectorSum(vector.GetUpper() + vector.GetLower());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<ulong> FastVectorRemainder(Vector512<ulong> vector512, uint div) {
            Vector512<ulong> result =
                FastVectorRemainder(Avx512F.ExtractVector128(vector512, 0), div)
                    .ToVector256Unsafe().ToVector512Unsafe();
            Avx512F.InsertVector128(result,
                                    FastVectorRemainder(Avx512F.ExtractVector128(vector512, 1), div), 1);
            Avx512F.InsertVector128(result,
                                    FastVectorRemainder(Avx512F.ExtractVector128(vector512, 2), div), 2);
            Avx512F.InsertVector128(result,
                                    FastVectorRemainder(Avx512F.ExtractVector128(vector512, 3), div), 3);
            return result;
        }
        #endif
    }
}
