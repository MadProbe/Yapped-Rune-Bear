#define CAREFULL
namespace SoulsFormats {
    [SkipLocalsInit]
    public partial class PARAM {
        /// <summary>
        ///     One row in a param file.
        /// </summary>
        public unsafe class Row {
            internal long DataOffset;

            /// <summary>
            ///     Cells contained in this row. Must be loaded with PARAM.ApplyParamdef() before use.
            /// </summary>
            public SoulsFormats.Util.AllocatorHandle<MiniCell> MiniCells;

            /// <summary>
            ///     Parent PARAM
            /// </summary>
            public PARAM Parent;

            /// <summary>
            ///     Creates a new row based on the given paramdef without default values.
            /// </summary>
            public Row(int id, string name, PARAM param) {
                this.ID        = id;
                this.Name      = name;
                this.Parent    = param;
                this.MiniCells = param.MiniCellsAllocator.Allocate<MiniCell>();
            }

            internal Row(SoulsFormats.Util.BinaryReaderEx br, PARAM parent, scoped ref long actualStringsOffset) {
                this.Parent = parent;
                long nameOffset;
                this.ID = br.ReadInt32();

                if (parent.Format2D.HasFlag(FormatFlags1.LongDataOffset)) {
                    _ = br.ReadInt32(); // I would like to assert 0, but some of the generatordbglocation params in DS2S have garbage here
                    this.DataOffset = br.ReadInt64();
                    nameOffset = br.ReadInt64();
                } else {
                    this.DataOffset = br.ReadUInt32();
                    nameOffset      = br.ReadUInt32();
                }

                if (nameOffset == 0) return;

                if (actualStringsOffset == 0 || nameOffset < actualStringsOffset) actualStringsOffset = nameOffset;

                this.Name = parent.Format2E.HasFlag(FormatFlags2.UnicodeRowNames) ? br.GetUTF16(nameOffset) : br.GetShiftJIS(nameOffset);
            }

            private static ReadOnlySpan<PARAMDEF.DefType> defTypesChange => new[] {
                PARAMDEF.DefType.s8,
                PARAMDEF.DefType.u8,
                PARAMDEF.DefType.s16,
                PARAMDEF.DefType.u16,
                PARAMDEF.DefType.s32,
                PARAMDEF.DefType.u32,
                PARAMDEF.DefType.b32,
                PARAMDEF.DefType.f32,
                PARAMDEF.DefType.angle32,
                PARAMDEF.DefType.f64,
                PARAMDEF.DefType.u8,
                PARAMDEF.DefType.fixstr,
                PARAMDEF.DefType.fixstrW,
            };

            /// <summary>
            ///     The ID number of this row.
            /// </summary>
            public int ID {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set;
            }

            /// <summary>
            ///     A name given to this row; no functional significance, may be null.
            /// </summary>
            public string Name {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set;
            }

            /// <summary>
            ///     Cells contained in this row. Must be loaded with PARAM.ApplyParamdef() before use.
            /// </summary>
            public IReadOnlyList<ICell> Cells {
                get => this.Parent.AppliedParamdef.FieldBitOffsetMap.FieldOffsetsFiltered
                           .Zip(this.Parent.AppliedParamdef.FieldBitOffsetMap.OffsetsFiltered)
                           .Select(field => new MiniCellWrapper(this.Parent.AppliedParamdef.Fields.AsContents().At(field.First),
                                                                PointerOffset<MiniCell,
                                                                    MiniCell>(this.MiniCells.GetOffset(this.Parent.MiniCellsAllocator),
                                                                              field.Second))).ToArray()
                           .CastTo<IReadOnlyList<ICell>, MiniCellWrapper[]>();
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private set { }
            }

            /// <summary>
            ///     Returns the first cell in the row with the given internal name.
            /// </summary>
            public ICell this[string name] => this.Cells.First(cell => cell.Def.InternalName == name);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void ReadCells(SoulsFormats.Util.BinaryReaderEx br, PARAMDEF paramdef) {
                br.Position = this.DataOffset;
                List<PARAMDEF.Field> fields = paramdef.Fields;
                ref PARAMDEF.Field   field  = ref MemoryMarshal.GetArrayDataReference(fields.AsContents());
                int                  count  = fields.Count;
                MiniCell* miniCells =
                    (this.MiniCells = this.Parent.MiniCellsAllocator.Allocate<MiniCell>()).GetOffset(this.Parent.MiniCellsAllocator);
                SoulsFormats.Formats.PARAM.FieldBitOffsetMap fieldBitOffsetMap = this.Parent.AppliedParamdef.FieldBitOffsetMap;
                int*                                         offsets           = fieldBitOffsetMap.OffsetsRef;
                int*                                         sizes             = fieldBitOffsetMap.SizesRef;

                int   bitOffset = -1;
                var   bitType   = PARAMDEF.DefType.u8;
                ulong bitValue  = 0; // This is ulong so checkOrphanedBits doesn't fail on offsets of 32


                for (var i = 0; i < count; i++, field = ref Unsafe.Add(ref field, 1)) {
                    int              field_offset = offsets[i];
                    ref MiniCell     minicell     = ref miniCells.AtRef(field_offset);
                    PARAMDEF.DefType type         = field.DisplayType;
                    var              checkBits    = true;

                    switch (type) {
                        case PARAMDEF.DefType.s8:
                            minicell.SByteValue = br.ReadSByte();
                            break;
                        case PARAMDEF.DefType.s16:
                            minicell.ShortValue = br.ReadInt16();
                            break;
                        case PARAMDEF.DefType.s32 or PARAMDEF.DefType.b32:
                            minicell.IntValue = br.ReadInt32();
                            break;
                        case PARAMDEF.DefType.f32 or PARAMDEF.DefType.angle32:
                            minicell.FloatValue = br.ReadSingle();
                            break;
                        case PARAMDEF.DefType.f64:
                            minicell.DoubleValue = br.ReadDouble();
                            break;
                        case PARAMDEF.DefType.fixstr:
                        case PARAMDEF.DefType.fixstrW:
                            br.ReadSpanBytes(new Span<byte>(AsPointer(ref minicell), sizes[i]));
                            break;
                        case PARAMDEF.DefType.u8 or PARAMDEF.DefType.u16 or PARAMDEF.DefType.u32 or PARAMDEF.DefType.dummy8:
                            if (field.BitSize == -1) {
                                switch (type) {
                                    case PARAMDEF.DefType.u8:
                                        minicell.ByteValue = br.ReadByte();
                                        break;
                                    case PARAMDEF.DefType.u16:
                                        minicell.UShortValue = br.ReadUInt16();
                                        break;
                                    case PARAMDEF.DefType.u32:
                                        minicell.UIntValue = br.ReadUInt32();
                                        break;
                                    case PARAMDEF.DefType.dummy8:
                                        br.ReadSpanBytes(new Span<byte>(AsPointer(ref minicell), sizes[i]));
                                        break;
                                }
                            } else {
                                checkBits = false;
                                PARAMDEF.DefType newBitType = defTypesChange[(int)type];
                                int              bitLimit   = SoulsFormats.Formats.PARAM.ParamUtil.GetBitLimit(newBitType);

                                //if (field.BitSize == 0) {
                                //    throw new NotImplementedException($"Bit size 0 is not supported.");
                                //}

                                //if (field.BitSize > bitLimit) {
                                //    throw new InvalidDataException($"Bit size {field.BitSize} is too large to fit in type {newBitType}.");
                                //}

                                if (bitOffset == -1 || newBitType != bitType || bitOffset + field.BitSize > bitLimit) {
                                    this.checkOrphanedBits(br, paramdef, bitOffset, bitValue);
                                    bitOffset = 0;
                                    bitType   = newBitType;
                                    bitValue = bitType switch {
                                        PARAMDEF.DefType.u8  => br.ReadByte(),
                                        PARAMDEF.DefType.u16 => br.ReadUInt16(),
                                        PARAMDEF.DefType.u32 => br.ReadUInt32(),
                                        _                    => bitValue,
                                    };
                                }

                                //_ = bitValue << (BIT_VALUE_SIZE - field.BitSize - bitOffset) >> (BIT_VALUE_SIZE - field.BitSize);
                                ulong shifted = Bmi1.X64.BitFieldExtract(bitValue, (byte)bitOffset, (byte)field.BitSize);
                                bitOffset += field.BitSize;

                                switch (bitType) {
                                    case PARAMDEF.DefType.u8:
                                        minicell.ByteValue = (byte)shifted;
                                        break;
                                    case PARAMDEF.DefType.u16:
                                        minicell.UShortValue = (ushort)shifted;
                                        break;
                                    case PARAMDEF.DefType.u32:
                                        minicell.UIntValue = (uint)shifted;
                                        break;
                                }
                            }

                            break;
                        default:
                            throw new NotImplementedException($"Unsupported field type: {type}");
                    }

                    if (!checkBits) continue;
                    this.checkOrphanedBits(br, paramdef, bitOffset, bitValue);
                    bitOffset = -1;
                }

                this.checkOrphanedBits(br, paramdef, bitOffset, bitValue);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [Conditional("CAREFULL")]
            private void checkOrphanedBits(SoulsFormats.Util.BinaryReaderEx br, PARAMDEF paramdef, int bitOffset, ulong bitValue) {
                if (bitOffset != -1 && bitValue >> bitOffset != 0)
                    throw new
                        InvalidDataException($"Invalid paramdef {paramdef.ParamType}; bits would be lost before +0x{br.Position - this.DataOffset:X} in row {this.ID}.");
            }

            internal void WriteHeader(SoulsFormats.Util.BinaryWriterEx bw, PARAM parent, int i) {
                if (parent.Format2D.HasFlag(FormatFlags1.LongDataOffset)) {
                    bw.WriteInt32(this.ID);
                    bw.WriteInt32(0);
                    bw.ReserveInt64($"RowOffset{i}");
                    bw.ReserveInt64($"NameOffset{i}");
                } else {
                    bw.WriteInt32(this.ID);
                    bw.ReserveUInt32($"RowOffset{i}");
                    bw.ReserveUInt32($"NameOffset{i}");
                }
            }

            internal void WriteCells(SoulsFormats.Util.BinaryWriterEx bw, PARAM parent, int index) {
                if (parent.Format2D.HasFlag(FormatFlags1.LongDataOffset))
                    bw.FillInt64($"RowOffset{index}", bw.Position);
                else
                    bw.FillUInt32($"RowOffset{index}", (uint)bw.Position);

                int       bitOffset = -1;
                var       bitType   = PARAMDEF.DefType.u8;
                ulong     bitValue  = 0;
                MiniCell* minicells = this.MiniCells.GetOffset(this.Parent.MiniCellsAllocator);
                int*      offsets   = this.Parent.AppliedParamdef.FieldBitOffsetMap.OffsetsRef;
                int*      sizes     = this.Parent.AppliedParamdef.FieldBitOffsetMap.SizesRef;
                int       length    = this.Parent.AppliedParamdef.FieldBitOffsetMap.Sizes.Length;

                for (var i = 0; i < length; i++) {
                    ref MiniCell     miniCell = ref minicells.AtRef(offsets[i]);
                    PARAMDEF.Field   field    = this.Parent.AppliedParamdef.Fields[i];
                    PARAMDEF.DefType type     = field.DisplayType;


                    switch (type) {
                        case PARAMDEF.DefType.s8:
                            bw.WriteSByte(miniCell.SByteValue);
                            break;
                        case PARAMDEF.DefType.s16:
                            bw.WriteInt16(miniCell.ShortValue);
                            break;
                        case PARAMDEF.DefType.s32 or PARAMDEF.DefType.b32:
                            bw.WriteInt32(miniCell.IntValue);
                            break;
                        case PARAMDEF.DefType.f32 or PARAMDEF.DefType.angle32:
                            bw.WriteSingle(miniCell.FloatValue);
                            break;
                        case PARAMDEF.DefType.f64:
                            bw.WriteDouble(miniCell.DoubleValue);
                            break;
                        case PARAMDEF.DefType.fixstr:
                        case PARAMDEF.DefType.fixstrW:
                            bw.WriteByteSpan(new ReadOnlySpan<byte>(AsPointer(ref miniCell), sizes[i]));
                            break;
                        case PARAMDEF.DefType.u8 or PARAMDEF.DefType.u16 or PARAMDEF.DefType.u32 or PARAMDEF.DefType.dummy8:
                            if (field.BitSize == -1) {
                                if (type == PARAMDEF.DefType.u8)
                                    bw.WriteByte(miniCell.ByteValue);
                                else if (type == PARAMDEF.DefType.u16)
                                    bw.WriteUInt16(miniCell.UShortValue);
                                else if (type == PARAMDEF.DefType.u32)
                                    bw.WriteUInt32(miniCell.UIntValue);
                                else if (type == PARAMDEF.DefType.dummy8) bw.WriteByteSpan(new ReadOnlySpan<byte>(AsPointer(ref miniCell), sizes[i]));
                            } else {
                                if (bitOffset == -1) {
                                    bitOffset = 0;
                                    bitType   = defTypesChange[(int)type];
                                    bitValue  = 0;
                                }

                                ulong shifted = miniCell.ULongValue;
                                // Shift left first to clear any out-of-range bits
                                bitValue  |= Bmi1.X64.BitFieldExtract(shifted, 0, (byte)field.BitSize) << bitOffset;
                                bitOffset += field.BitSize;

                                if (i + 1 == length || IsWritable(bitOffset, bitType, type, this.Parent.AppliedParamdef.Fields[i + 1])) {
                                    bitOffset = -1;
                                    if (bitType == PARAMDEF.DefType.u8)
                                        bw.WriteByte((byte)bitValue);
                                    else if (bitType == PARAMDEF.DefType.u16)
                                        bw.WriteUInt16((ushort)bitValue);
                                    else if (bitType == PARAMDEF.DefType.u32) bw.WriteUInt32((uint)bitValue);
                                }
                            }

                            break;
                        default:
                            throw new NotImplementedException($"Unsupported field type: {type}");
                    }
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                static bool IsWritable(int bitOffset, PARAMDEF.DefType bitType, PARAMDEF.DefType type, PARAMDEF.Field nextField) =>
                    !SoulsFormats.Formats.PARAM.ParamUtil.IsBitType(nextField.DisplayType) || nextField.BitSize == -1 ||
                    bitOffset + nextField.BitSize > SoulsFormats.Formats.PARAM.ParamUtil.GetBitLimit(bitType) || defTypesChange[(int)type] != bitType;
            }

            internal void WriteName(SoulsFormats.Util.BinaryWriterEx bw, PARAM parent, int i) {
                long nameOffset = 0;

                if (this.Name != null) {
                    nameOffset = bw.Position;
                    if (parent.Format2E.HasFlag(FormatFlags2.UnicodeRowNames))
                        bw.WriteUTF16(this.Name, true);
                    else
                        bw.WriteShiftJIS(this.Name, true);
                }

                if (parent.Format2D.HasFlag(FormatFlags1.LongDataOffset))
                    bw.FillInt64($"NameOffset{i}", nameOffset);
                else
                    bw.FillUInt32($"NameOffset{i}", (uint)nameOffset);
            }

            /// <summary>
            ///     Returns a string representation of the row.
            /// </summary>
            public override string ToString() => $"{this.ID} {this.Name}";
        }
    }
}
