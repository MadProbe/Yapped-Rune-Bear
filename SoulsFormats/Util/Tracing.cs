#define TRACING

using System.Collections;

namespace SoulsFormats.Util {
    public static class Tracing {
        public static readonly Dictionary<string, double> Timings = new ();

        public static readonly UnmanagedList<long> TimingsList = new ();
        public static readonly Dictionary<string, nint> NamedTimingsOffsetsDictionary = new ();

        static Tracing() => _init();

        public static StreamWriter Stream { get; private set; } = new (System.IO.Stream.Null);

        public static void TraceTiming(Action action, string functionName) {
            long start = Stopwatch.GetTimestamp();
            action();
            WriteTraceMessage(functionName, Stopwatch.GetElapsedTime(start, Stopwatch.GetTimestamp()).TotalMilliseconds);
        }

        public static O CollectTiming<O>(Func<O> function, string keyName) {
            long start = Stopwatch.GetTimestamp();
            O value = function();

            lock (Timings) {
                Timings.TryAdd(keyName, 0);
                Timings[keyName] += Stopwatch.GetElapsedTime(start, Stopwatch.GetTimestamp()).TotalMilliseconds;
            }

            return value;
        }

        public static void CollectTiming(Action action, string keyName) {
            long start = Stopwatch.GetTimestamp();
            action();

            lock (Timings) {
                Timings.TryAdd(keyName, 0);
                Timings[keyName] += Stopwatch.GetElapsedTime(start, Stopwatch.GetTimestamp()).TotalMilliseconds;
            }
        }

        public static void PrintTiming(string keyName) {
            lock (Timings) {
                if (Timings.Remove(keyName, out double timing)) WriteTraceMessage(keyName, timing);
            }
        }

        public static unsafe void PrintNamedTiming(string keyName) {
            WriteTraceMessage(keyName, Stopwatch.GetElapsedTime(0, TimingsList.Data[NamedTimingsOffsetsDictionary[keyName]]).TotalMilliseconds);
            TimingsList.Data[NamedTimingsOffsetsDictionary[keyName]] = 0;
        }

        public static double TracedTiming(Action action, string functionName) {
            long start = Stopwatch.GetTimestamp();
            action();
            double totalMilliseconds = Stopwatch.GetElapsedTime(start, Stopwatch.GetTimestamp()).TotalMilliseconds;
            WriteTraceMessage(functionName, totalMilliseconds);
            return totalMilliseconds;
        }

        public static void TraceTiming<I1>(Action<I1> action, I1 p1, string functionName) {
            long start = Stopwatch.GetTimestamp();
            action(p1);
            WriteTraceMessage(functionName, Stopwatch.GetElapsedTime(start, Stopwatch.GetTimestamp()).TotalMilliseconds);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("TRACING")]
        public static void WriteTraceMessage(string functionName, double end) => Stream.Write($"[TRACE] {functionName} completed in {end}ms\r\n");

        [Conditional("TRACING")]
        private static void _init() => Stream = new StreamWriter("./trace.log", false, SFEncoding.UTF16, 0x1000) {
            AutoFlush = true,
        };

        public unsafe class NamedTiming {
            private readonly string name;
            private readonly nint offset;
            private long start;

            public NamedTiming(string name) {
                this.name   = name;
                this.offset = NamedTimingsOffsetsDictionary[name] = TimingsList.Count;
                TimingsList.Add(0);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Start() => this.start = Stopwatch.GetTimestamp();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Stop() => TimingsList.Data[this.offset] += Stopwatch.GetTimestamp() - this.start;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Print() {
                WriteTraceMessage(this.name, Stopwatch.GetElapsedTime(0, TimingsList.Data[this.offset]).TotalMilliseconds);
                TimingsList.Data[this.offset] = 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Disposable AsDisposable() => new (this);

            public readonly ref struct Disposable {
                private readonly NamedTiming timing;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public Disposable(NamedTiming timing) => (this.timing = timing).Start();

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Dispose() => this.timing.Stop();
            }
        }
    }

    [SkipLocalsInit]
    public unsafe class UnmanagedList<T> : IList<T>, IEquatable<UnmanagedList<T>> {
        public nuint Alignment;
        public int Capacity;
        public T* Data;

        public UnmanagedList(int capacity = 4) : this(capacity, (nuint)Unsafe.SizeOf<T>()) { }

        public UnmanagedList(int capacity, nuint alignment) {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(capacity, 0);

            this.Capacity  = capacity;
            this.Alignment = BitOperations.RoundUpToPowerOf2(alignment);
            this.Data      = (T*)NativeMemory.AlignedAlloc(capacity.CastTo<nuint, int>() * (nuint)Unsafe.SizeOf<T>(), this.Alignment);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(UnmanagedList<T> other) => !ReferenceEquals(null, other) && (ReferenceEquals(this, other) || this == other);

        public IEnumerator<T> GetEnumerator() {
            for (var i = 0; i < this.Count; i++) yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public void Add(T item) {
            if (this.Count == this.Capacity) this.SetCapacity(this.Capacity * 2);
            this.Data[this.Count++] = item;
        }

        public void Clear() {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>()) new Span<byte>(this.Data, this.Count * Unsafe.SizeOf<T>()).Clear();

            this.Count = 0;
        }

        public bool Contains(T item) {
            for (int index = 0, count = this.Count; index < count; index++)
                if (SequenceEqual(ref item, ref Unsafe.AsRef<T>(this.Data), 1))
                    return true;

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex = 0) => new ReadOnlySpan<T>(this.Data, this.Count).CopyTo(array.AsSpan(arrayIndex));

        public bool Remove(T item) {
            int index = this.IndexOf(item);

            if (index < 0) return false;
            this.RemoveAt(index);
            return true;
        }

        public int Count { get; set; }
        public bool IsReadOnly => false;

        public int IndexOf(T item) {
            for (int index = 0, count = this.Count; index < count; index++)
                if (SequenceEqual(ref item, ref Unsafe.AsRef<T>(this.Data), 1))
                    return index;

            return -1;
        }

        public void Insert(int index, T item) {
            if (index >= this.Capacity) this.SetCapacity(index);

            this.Data[index] = item;
        }

        public void RemoveAt(int index) => new Span<T>(this.Data + index, Unsafe.SizeOf<T>()).Clear();

        public T this[int index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.Data[index < this.Capacity && index >= 0 ? index : throw new ArgumentOutOfRangeException(nameof(index))];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set {
                if (index >= this.Capacity) this.SetCapacity(index);
                if (index >= this.Count) this.Count = index + 1;

                this.Data[index >= 0 ? index : throw new ArgumentOutOfRangeException(nameof(index))] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetItemReferenceUnsafe(nint index) => ref Unsafe.AsRef<T>(this.Data + index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetItemUnsafe(nint index, T item) => this.Data[index] = item;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapacity(int newCapacity, bool roundToPowerOfTwo = true) =>
            this.Data = (T*)NativeMemory.AlignedRealloc(this.Data,
                                                        (this.Capacity = roundToPowerOfTwo
                                                            ? BitOperations.RoundUpToPowerOf2(newCapacity.CastTo<uint, int>())
                                                                           .CastTo<int, uint>()
                                                            : newCapacity).CastTo<nuint, int>() *
                                                        (nuint)Unsafe.SizeOf<T>(), this.Alignment);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(UnmanagedList<T> @this) => new (@this.Data, @this.Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>(UnmanagedList<T> @this) => new (@this.Data, @this.Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(UnmanagedList<T> left, UnmanagedList<T> right) => left.Count == right.Count &&
                                                                                         SequenceEqual(ref Unsafe.AsRef<T>(left.Data),
                                                                                             ref Unsafe.AsRef<T>(right.Data),
                                                                                             (nuint)left.Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(UnmanagedList<T> left, UnmanagedList<T> right) => !(left == right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) =>
            !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) || (obj is UnmanagedList<T> other && this == other));

        public override int GetHashCode() => throw new NotSupportedException();

        ~UnmanagedList() => NativeMemory.AlignedFree(this.Data);
    }
}
