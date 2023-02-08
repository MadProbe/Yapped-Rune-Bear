namespace Chomp.Util {
    public unsafe ref struct IterativeStringSplitter {
        private char* _value;
        private readonly char* _seperator;
        private readonly char* _endp;
        private readonly char* _safe_range;
        private readonly int _sep_length;
        public IterativeStringSplitter(ReadOnlySpan<char> value, char* separator, int separator_length) {
            fixed (char* valuePtr = value) {
                this._seperator = separator;
                this._safe_range = (this._endp = (this._value = valuePtr) + value.Length) - (_sep_length = separator_length);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsExhausted() => this._value > this._endp;
        public ReadOnlySpan<char> Next() {
            char* startp = this._value;
            char* result = null;
            int s;
            for (; this._value <= this._endp; this._value++) {
                if (this._safe_range >= this._value) {
                    for (s = 0; s < this._sep_length && this._value[s] == this._seperator[s]; s++) ;
                    if (s == this._sep_length) {
                        result = this._value;
                        this._value += s;
                        goto _;
                    }
                }
            }
            result = this._value > this._endp ? this._endp : this._value;
            _:
            return new ReadOnlySpan<char>(startp, unchecked((int)((ulong)result - (ulong)startp)) >> 1); // / sizeof(char)
        }
    }
}
