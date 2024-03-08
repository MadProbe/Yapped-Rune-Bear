using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using static System.Runtime.Intrinsics.Vector256;

namespace Chomp.Util {
    [SkipLocalsInit]
    public static class VectorExtensions {
        public static Vector256<T> Compare<T>(this Vector256<T> @this, Vector256<T> comparee) where T : struct => Vector256.Equals(comparee, @this);
    }

    public unsafe ref struct IterativeStringSplitter {
        private char* _value;
        private readonly char* _seperator;
        private readonly char* _endp;
        private readonly char* _safe_range;
        private readonly int _sep_length;
        private readonly Vector256<char> _chars;

        [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IterativeStringSplitter(ReadOnlySpan<char> value, char* separator, int separator_length) {
            this._seperator = separator;
            this._safe_range = (this._endp = (this._value = value.AsPointer()) + value.Length) - (this._sep_length = separator_length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsExhausted() => this._value > this._endp;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<char> Next() {
            char* startp = this._value;
            char* result;
            for (Unsafe.SkipInit(out int s); this._value <= this._safe_range; this._value++) {
                for (s = 0; s < this._sep_length && this._value[s] == this._seperator[s]; s++) ;

                if (s != this._sep_length) continue;
                result = this._value;
                this._value += s;
                goto _;
            }

            result = this._value > this._safe_range ? this._endp : this._value;
            _:
            return new ReadOnlySpan<char>(startp, (int)((nuint)result - (nuint)startp >> 1)); // / sizeof(char)
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string NextString() => new (this.Next());
    }

    [SkipLocalsInit]
    public unsafe struct IterativeStringSplitterSmartVectorized {
        private const int CharsInVector256 = 256 / 16;
        private readonly bool _is_vectorizable;
        private char* _value;
        private readonly char* _seperatorp;
        private readonly char* _endp;
        private readonly char* _safe_range;
        private readonly int _sep_length;
        private uint _char_bits;
        private readonly int _value_length;
        private readonly ushort _separator;
        private int _offset;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IterativeStringSplitterSmartVectorized(ReadOnlySpan<char> value, char* separator, int separator_length) {
            this._value = value.AsPointer();
            // ReSharper disable once AssignmentInConditionalExpression
            if (this._is_vectorizable = IsVectorizable(value, separator_length)) {
                this._value_length = value.Length - CharsInVector256;
                this._separator = *separator;
                _ = this.InitNextBits();
            }
            else {
                this._seperatorp = separator;
                this._safe_range = (this._endp = this._value + value.Length) - (this._sep_length = separator_length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IterativeStringSplitterSmartVectorized(ReadOnlySpan<char> value, ReadOnlySpan<char> separator) :
            this(value, separator.AsPointer(), separator.Length) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsVectorizable(ReadOnlySpan<char> chars, int separator_length) =>
            Avx2.IsSupported & Bmi2.IsSupported & (chars.Length > CharsInVector256) & (separator_length == 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsExhausted() => this._is_vectorizable ? this._value_length + CharsInVector256 < this._offset : this._value >= this._endp;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsExhaustedVectorized() => this._value_length + CharsInVector256 < this._offset;

        public ReadOnlySpan<char> Next() {
            if (this._is_vectorizable) {
                int start_offset = this._offset;
                if (this._char_bits == 0) {
                    this._offset += CharsInVector256 - this._offset & 0xf;
                    while (this.InitNextBits() == 0) this._offset += CharsInVector256;
                }

                this._offset += CastTo<int, uint>(Bmi1.TrailingZeroCount(this._char_bits)) - this._offset & 0xf;
                this._char_bits = Bmi1.ResetLowestSetBit(this._char_bits);
                return new ReadOnlySpan<char>(this._value + start_offset, this._offset++ - start_offset);
            }

            char* startp = this._value;
            char* result;
            for (Unsafe.SkipInit(out int s); this._value <= this._safe_range; this._value++) {
                for (s = 0; s < this._sep_length && this._value[s] == this._seperatorp[s]; s++) { }

                if (s != this._sep_length) continue;
                result = this._value;
                this._value += s;
                goto _;
            }

            result = this._value > this._safe_range ? this._endp : this._value;
            _:
            return new ReadOnlySpan<char>(startp, (int)((nuint)result - (nuint)startp >> 1)); // / sizeof(char)
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint InitNextBits() => this._char_bits = this._value_length > this._offset
            ? Bmi2.ParallelBitExtract(CastTo<uint, int>(Avx2.MoveMask(Load((ushort*)this._value + this._offset)
                                                                      .Compare(Create(this._separator))
                                                                      .AsByte())), 0xaaaaaaaa)
            : Bmi2.ParallelBitExtract(CastTo<uint, int>(Avx2.MoveMask(Load((ushort*)this._value + this._value_length)
                                                                      .Compare(Create(this._separator))
                                                                      .AsByte())),
                                      0xaaaaaaaa << (this._offset - this._value_length) * 2)
              | (0x10000u >> (this._offset - this._value_length & 0xf));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string NextString() => new (this.Next());
    }
}
