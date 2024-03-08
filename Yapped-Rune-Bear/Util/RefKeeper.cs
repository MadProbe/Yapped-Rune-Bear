namespace Chomp.Util {
    internal readonly unsafe struct RefKeeper<T> {
        private readonly T* _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RefKeeper(ref T value) => this._value = (T*)Unsafe.AsPointer(ref value);

        public ref T Reference {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.AsRef<T>(this._value);
        }
    }
}
