namespace SoulsFormats.Formats.PARAM {
    public readonly unsafe struct FieldBitOffsetMap {
        public static readonly int                ARRAY_TO_CONTENTS_POINTER_OFFSET = sizeof(nuint) * 2;
        public readonly        int[]              Offsets;
        public readonly        int[]              OffsetsFiltered;
        public readonly        int[]              FieldOffsetsFiltered;
        public readonly        int[]              ArrayLengthsFiltered;
        public readonly        int[]              Sizes;
        public readonly        int[]              ArrayLengths;
        public readonly        PARAMDEF.DefType[] DefTypesFiltered;
        public readonly        CellHelperInfo[]   CellHelperInfoes;
        public readonly        int*               OffsetsRef;
        public readonly        int*               OffsetsFilteredRef;
        public readonly        int*               FieldOffsetsFilteredRef;
        public readonly        int*               SizesRef;
        public readonly        int*               ArrayLengthsRef;
        public readonly        int*               ArrayLengthsFilteredRef;
        public readonly        CellHelperInfo*    CellHelperInfoesRef;
        public readonly        PARAMDEF.DefType*  DefTypesFilteredRef;
        public readonly        int                Size;
        public int this[int index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.OffsetsRef[index];
        }

        public FieldBitOffsetMap(List<PARAMDEF.Field> fields) {
            Span<int>              filtered_offsets       = stackalloc int[fields.Count];
            Span<int>              filtered_field_offsets = stackalloc int[fields.Count];
            Span<int>              filtered_array_lengths = stackalloc int[fields.Count];
            Span<PARAMDEF.DefType> filtered_deftypes      = stackalloc PARAMDEF.DefType[fields.Count];
            this.Size = 0;
            var filtered_offsets_size = 0;
            // array item refs are ok since arrays are pinned
            this.OffsetsRef = AsPointer<int[], int>(this.Offsets = GC.AllocateUninitializedArray<int>(fields.Count, true),
                                                    ARRAY_TO_CONTENTS_POINTER_OFFSET);
            this.SizesRef = AsPointer<int[], int>(this.Sizes = GC.AllocateUninitializedArray<int>(fields.Count, true),
                                                  ARRAY_TO_CONTENTS_POINTER_OFFSET);
            this.ArrayLengthsRef = AsPointer<int[], int>(this.ArrayLengths = GC.AllocateUninitializedArray<int>(fields.Count, true),
                                                         ARRAY_TO_CONTENTS_POINTER_OFFSET);
            this.CellHelperInfoesRef =
                AsPointer<CellHelperInfo[], CellHelperInfo>(this.CellHelperInfoes = GC.AllocateUninitializedArray<CellHelperInfo>(fields.Count, true),
                                                            ARRAY_TO_CONTENTS_POINTER_OFFSET);
            PARAMDEF.Field[] fieldsArray = fields.AsContents();

            for (var i = 0; i < fields.Count; i++) {
                PARAMDEF.Field field = fieldsArray[i];
                int size = field.DisplayType switch {
                    PARAMDEF.DefType.s8      => sizeof(sbyte),
                    PARAMDEF.DefType.u8      => sizeof(byte),
                    PARAMDEF.DefType.s16     => sizeof(short),
                    PARAMDEF.DefType.u16     => sizeof(ushort),
                    PARAMDEF.DefType.s32     => sizeof(int),
                    PARAMDEF.DefType.u32     => sizeof(uint),
                    PARAMDEF.DefType.b32     => sizeof(int),
                    PARAMDEF.DefType.f32     => sizeof(float),
                    PARAMDEF.DefType.angle32 => sizeof(float),
                    PARAMDEF.DefType.f64     => sizeof(double),
                    PARAMDEF.DefType.dummy8  => field.BitSize == -1 ? sizeof(byte) * field.ArrayLength : sizeof(byte),
                    PARAMDEF.DefType.fixstr  => sizeof(byte) * field.ArrayLength,
                    PARAMDEF.DefType.fixstrW => sizeof(char) * field.ArrayLength,
                    _                        => Unsafe.NullRef<int>(),
                };
                this.OffsetsRef[i]          =  this.Size;
                this.Size                   += size;
                this.SizesRef[i]            =  size;
                this.CellHelperInfoesRef[i] =  new CellHelperInfo(this.ArrayLengthsRef[i] = field.ArrayLength, field.DisplayType);
                if (field.DisplayType == PARAMDEF.DefType.dummy8) continue;
                filtered_offsets[filtered_offsets_size]       = this.OffsetsRef[i];
                filtered_array_lengths[filtered_offsets_size] = this.ArrayLengthsRef[i];
                filtered_field_offsets[filtered_offsets_size] = i * Unsafe.SizeOf<PARAMDEF.Field>() + ARRAY_TO_CONTENTS_POINTER_OFFSET;
                filtered_deftypes[filtered_offsets_size++]    = field.DisplayType;
            }

            int[] filtered_offsets_array       = this.OffsetsFiltered = GC.AllocateUninitializedArray<int>(filtered_offsets_size,      true);
            int[] filtered_field_offsets_array = this.FieldOffsetsFiltered = GC.AllocateUninitializedArray<int>(filtered_offsets_size, true);
            int[] filtered_array_lengths_array = this.ArrayLengthsFiltered = GC.AllocateUninitializedArray<int>(filtered_offsets_size, true);
            PARAMDEF.DefType[] filtered_deftypes_array =
                this.DefTypesFiltered = GC.AllocateUninitializedArray<PARAMDEF.DefType>(filtered_offsets_size, true);
            filtered_offsets[..filtered_offsets_size].CopyTo(filtered_offsets_array);
            filtered_field_offsets[..filtered_offsets_size].CopyTo(filtered_field_offsets_array);
            filtered_array_lengths[..filtered_offsets_size].CopyTo(filtered_array_lengths_array);
            filtered_deftypes[..filtered_offsets_size].CopyTo(filtered_deftypes_array);
            this.OffsetsFilteredRef      = AsPointer<int[], int>(filtered_offsets_array,       ARRAY_TO_CONTENTS_POINTER_OFFSET);
            this.FieldOffsetsFilteredRef = AsPointer<int[], int>(filtered_field_offsets_array, ARRAY_TO_CONTENTS_POINTER_OFFSET);
            this.ArrayLengthsFilteredRef = AsPointer<int[], int>(filtered_array_lengths_array, ARRAY_TO_CONTENTS_POINTER_OFFSET);
            this.DefTypesFilteredRef     = AsPointer<PARAMDEF.DefType[], PARAMDEF.DefType>(filtered_deftypes_array, ARRAY_TO_CONTENTS_POINTER_OFFSET);
        }

        public SoulsFormats.Util.Allocator CreateAllocator(nuint estimated_instances) => new ((nuint)this.Size, estimated_instances);

        public readonly struct CellHelperInfo {
            public readonly int              length;
            public readonly PARAMDEF.DefType type;

            public CellHelperInfo(int length, PARAMDEF.DefType type) {
                this.length = length;
                this.type   = type;
            }
        }
    }
}
