namespace SoulsFormats.Util {
    [SkipLocalsInit]
    public unsafe class Allocator {
        private const nuint memory_realloc_instance_count_growth_factor = 0x200;
        private const nuint memory_padding = 0x10;
        public void* memory;
        public nuint* memory_allocated_indicies_compact;
        public nuint memory_used;
        public nuint memory_allocated;
        public nuint memory_allocated_indicies_compact_length;
        public nuint memory_last_free_allocated_index;
        public readonly nuint value_size;
        public Allocator(nuint value_size, nuint estimated_instances = 0x1000) {
            var nuint_size = (nuint)sizeof(nuint);
            estimated_instances += nuint_size - estimated_instances & nuint_size - 1;
            nuint byteCountMemory = estimated_instances * value_size;
            this.memory = NativeMemory.Alloc(byteCountMemory + memory_padding);
            GC.AddMemoryPressure((long)(byteCountMemory + memory_padding));
            nuint byteCount = estimated_instances >> BitOperations.TrailingZeroCount(nuint_size);
            //this.memory_allocated_indicies_compact = (nuint*)NativeMemory.AllocZeroed(byteCount);
            this.memory_used = 0;
            this.memory_allocated = byteCountMemory;
            this.memory_last_free_allocated_index = 0;
            this.memory_allocated_indicies_compact_length = byteCount;
            this.value_size = value_size;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AllocatorHandle<T> Allocate<T>() {
            Unsafe.SkipInit(out nuint offset);
            if (this.memory_last_free_allocated_index == nuint.MaxValue) {
                _ = Unsafe.NullRef<nuint>();
            } else {
                // offset = this.memory_last_free_allocated_index++ * this.value_size;
                offset = this.memory_used;
                if (this.memory_allocated == offset) {
                    this.memory = NativeMemory.Realloc(this.memory, memory_padding + (this.memory_allocated += this.value_size * memory_realloc_instance_count_growth_factor));
                }
                this.memory_used += this.value_size;
            }

            return new AllocatorHandle<T>(offset);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Free<T>(AllocatorHandle<T> handle) => Unsafe.NullRef<nuint>();
        ~Allocator() {
            NativeMemory.Free(this.memory);
            //NativeMemory.Free(this.memory_allocated_indicies_compact);
            GC.RemoveMemoryPressure((long)(this.memory_allocated + memory_padding));
        }
    }
    public readonly unsafe record struct AllocatorHandle<T> {
        public readonly nuint offset;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AllocatorHandle(nuint offset) => this.offset = offset;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetOffset(Allocator allocator) => PointerOffset<T>(allocator.memory, this.offset);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetOffsetRef(Allocator allocator) => ref Unsafe.AsRef<T>(PointerOffset<T>(allocator.memory, this.offset));
    }
}
