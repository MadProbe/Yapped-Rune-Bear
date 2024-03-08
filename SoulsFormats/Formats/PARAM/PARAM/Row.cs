using SoulsFormats.Formats.PARAM;
using SoulsFormats.Util;
using DefType = SoulsFormats.PARAMDEF.DefType;

namespace SoulsFormats {
    [SkipLocalsInit]
    public partial class PARAM {
        /// <summary>
        /// One row in a param file.
        /// </summary>
        public class Row {
            /// <summary>
            /// The ID number of this row.
            /// </summary>
            public int ID {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set;
            }

            /// <summary>
            /// A name given to this row; no functional significance, may be null.
            /// </summary>
            public string Name {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set;
            }

            /// <summary>
            /// Cells contained in this row. Must be loaded with PARAM.ApplyParamdef() before use.
            /// </summary>
            public IReadOnlyList<Cell> Cells {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private set;
            }

            /// <summary>
            /// Cells contained in this row. Must be loaded with PARAM.ApplyParamdef() before use.
            /// </summary>
            public MiniCell[] MiniCells;

            internal long DataOffset;

            /// <summary>
            /// Creates a new row based on the given paramdef with default values.
            /// </summary>
            public Row(int id, string name, PARAMDEF paramdef) {
                this.ID = id;
                this.Name = name;

                PARAMDEF.Field[] fields = paramdef.Fields.AsContents();
                int count = paramdef.Fields.Count;
                var cells = new Cell[count];
                this.Cells = cells;
                for (int i = 16, l = count * Unsafe.SizeOf<Cell>() + 16; i < l; i += Unsafe.SizeOf<Cell>()) {
                    PARAMDEF.Field field = fields.At(i);
                    cells.AssignAt(i, new Cell(field, ParamUtil.ConvertDefaultValue(field)));
                }
            }

            internal Row(BinaryReaderEx br, PARAM parent, scoped ref long actualStringsOffset) {
                long nameOffset;
                this.ID = br.ReadInt32();
                if (parent.Format2D.HasFlag(FormatFlags1.LongDataOffset)) {
                    _ = br.ReadInt32(); // I would like to assert 0, but some of the generatordbglocation params in DS2S have garbage here
                    this.DataOffset = br.ReadInt64();
                    nameOffset = br.ReadInt64();
                } else {
                    this.DataOffset = br.ReadUInt32();
                    nameOffset = br.ReadUInt32();
                }

                if (nameOffset != 0) {
                    if (actualStringsOffset == 0 || nameOffset < actualStringsOffset) {
                        actualStringsOffset = nameOffset;
                    }

                    this.Name = parent.Format2E.HasFlag(FormatFlags2.UnicodeRowNames) ? br.GetUTF16(nameOffset) : br.GetShiftJIS(nameOffset);
                }
            }

            internal void ReadCells(BinaryReaderEx br, PARAMDEF paramdef) {
                // In case someone decides to add new rows before applying the paramdef (please don't do that)
                if (this.DataOffset == 0) {
                    return;
                }

                br.Position = this.DataOffset;
                List<PARAMDEF.Field> fields = paramdef.Fields;
                PARAMDEF.Field[] fields_array = fields.AsContents();
                int start = 16, count = fields.Count, end = 2 + count << 3;
                var cells = new Cell[count];
                MiniCell[] miniCells = this.MiniCells = new MiniCell[count];

                int bitOffset = -1;
                DefType bitType = DefType.u8;
                ulong bitValue = 0; // This is ulong so checkOrphanedBits doesn't fail on offsets of 32


                for (; start < end; start += 8) {
                    PARAMDEF.Field field = fields_array.At(start);
                    object value = null;
                    DefType type = field.DisplayType;

                    switch (type) {
                        case DefType.s8:
                            value = miniCells[start >> 3].SByteValue = br.ReadSByte();
                            break;
                        case DefType.s16:
                            value = miniCells[start >> 3].ShortValue = br.ReadInt16();
                            break;
                        case DefType.s32 or DefType.b32:
                            value = miniCells[start >> 3].IntValue = br.ReadInt32();
                            break;
                        case DefType.f32 or DefType.angle32:
                            value = miniCells[start >> 3].FloatValue = br.ReadSingle();
                            break;
                        case DefType.f64:
                            value = miniCells[start >> 3].DoubleValue = br.ReadDouble();
                            break;
                        case DefType.fixstr:
                            value = miniCells[start >> 3].StringValue = br.ReadFixStr(field.ArrayLength);
                            break;
                        case DefType.fixstrW:
                            value = miniCells[start >> 3].StringValue = br.ReadFixStrW(field.ArrayLength * 2);
                            break;
                        default:
                            if (ParamUtil.IsBitType(type)) {
                                if (field.BitSize == -1) {
                                    if (type == DefType.u8) {
                                        value = miniCells[start >> 3].ByteValue = br.ReadByte();
                                    } else if (type == DefType.u16) {
                                        value = miniCells[start >> 3].UShortValue = br.ReadUInt16();
                                    } else if (type == DefType.u32) {
                                        value = miniCells[start >> 3].UIntValue = br.ReadUInt32();
                                    } else if (type == DefType.dummy8) {
                                        value = miniCells[start >> 3].ByteArrayValue = br.ReadBytes(field.ArrayLength);
                                    }
                                }
                                break;
                            } else {
                                throw new NotImplementedException($"Unsupported field type: {type}");
                            }
                    }

                    if (value != null) {
#if !CARELESS
                        this.checkOrphanedBits(br, paramdef, bitOffset, bitValue);
#endif
                        bitOffset = -1;
                    } else {
                        DefType newBitType = defTypesChange[(int)type];
                        int bitLimit = ParamUtil.GetBitLimit(newBitType);

                        //if (field.BitSize == 0) {
                        //    throw new NotImplementedException($"Bit size 0 is not supported.");
                        //}

                        //if (field.BitSize > bitLimit) {
                        //    throw new InvalidDataException($"Bit size {field.BitSize} is too large to fit in type {newBitType}.");
                        //}

                        if (bitOffset == -1 || newBitType != bitType || bitOffset + field.BitSize > bitLimit) {
#if !CARELESS
                            this.checkOrphanedBits(br, paramdef, bitOffset, bitValue);
#endif
                            bitOffset = 0;
                            bitType = newBitType;
                            if (bitType == DefType.u8) {
                                bitValue = br.ReadByte();
                            } else if (bitType == DefType.u16) {
                                bitValue = br.ReadUInt16();
                            } else if (bitType == DefType.u32) {
                                bitValue = br.ReadUInt32();
                            }
                        }

                        //_ = bitValue << (BIT_VALUE_SIZE - field.BitSize - bitOffset) >> (BIT_VALUE_SIZE - field.BitSize);
                        ulong shifted = System.Runtime.Intrinsics.X86.Bmi1.X64.BitFieldExtract(bitValue, (byte)bitOffset, (byte)field.BitSize);
                        bitOffset += field.BitSize;
                        if (bitType == DefType.u8) {
                            value = miniCells[start >> 3].ByteValue = (byte)shifted;
                        } else if (bitType == DefType.u16) {
                            value = miniCells[start >> 3].UShortValue = (ushort)shifted;
                        } else if (bitType == DefType.u32) {
                            value = miniCells[start >> 3].UIntValue = (uint)shifted;
                        }
                    }

                    cells.AssignAt(start, new Cell(field, value));
                }
#if !CARELESS
                this.checkOrphanedBits(br, paramdef, bitOffset, bitValue);
#endif
                this.Cells = cells;
            }
            private static readonly DefType[] defTypesChange = new DefType[] {
                DefType.s8,
                DefType.u8,
                DefType.s16,
                DefType.u16,
                DefType.s32,
                DefType.u32,
                DefType.b32,
                DefType.f32,
                DefType.angle32,
                DefType.f64,
                DefType.u8,
                DefType.fixstr,
                DefType.fixstrW
            };

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void checkOrphanedBits(BinaryReaderEx br, PARAMDEF paramdef, int bitOffset, ulong bitValue) {
                if (bitOffset != -1 && bitValue >> bitOffset != 0) {
                    throw new InvalidDataException($"Invalid paramdef {paramdef.ParamType}; bits would be lost before +0x{br.Position - this.DataOffset:X} in row {this.ID}.");
                }
            }

            internal void WriteHeader(BinaryWriterEx bw, PARAM parent, int i) {
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

            internal void WriteCells(BinaryWriterEx bw, PARAM parent, int index) {
                if (parent.Format2D.HasFlag(FormatFlags1.LongDataOffset)) {
                    bw.FillInt64($"RowOffset{index}", bw.Position);
                } else {
                    bw.FillUInt32($"RowOffset{index}", (uint)bw.Position);
                }

                int bitOffset = -1;
                DefType bitType = DefType.u8;
                ulong bitValue = 0;
                const int BIT_VALUE_SIZE = 64;
                Cell[] cells = this.Cells.CastTo<Cell[], IReadOnlyList<Cell>>();

                for (int i = 16, l_pre = (cells.Length << 3) + 8, l = l_pre + 8; i < l; i += 8) {
                    Cell cell = cells.At(i);
                    object value = cell.Value;
                    PARAMDEF.Field field = cell.Def;
                    DefType type = field.DisplayType;

                    switch (type) {
                        case DefType.s8:
                            bw.WriteSByte((sbyte)value);
                            break;
                        case DefType.s16:
                            bw.WriteInt16((short)value);
                            break;
                        case DefType.s32 or DefType.b32:
                            bw.WriteInt32((int)value);
                            break;
                        case DefType.f32 or DefType.angle32:
                            bw.WriteSingle((float)value);
                            break;
                        case DefType.f64:
                            bw.WriteDouble((double)value);
                            break;
                        case DefType.fixstr:
                            bw.WriteFixStr((string)value, field.ArrayLength);
                            break;
                        case DefType.fixstrW:
                            bw.WriteFixStrW((string)value, field.ArrayLength * 2);
                            break;
                        case DefType.u8 or DefType.u16 or DefType.u32 or DefType.dummy8:
                            if (field.BitSize == -1) {
                                if (type == DefType.u8) {
                                    bw.WriteByte((byte)value);
                                } else if (type == DefType.u16) {
                                    bw.WriteUInt16((ushort)value);
                                } else if (type == DefType.u32) {
                                    bw.WriteUInt32((uint)value);
                                } else if (type == DefType.dummy8) {
                                    bw.WriteBytes((byte[])value);
                                }
                            } else {
                                if (bitOffset == -1) {
                                    bitOffset = 0;
                                    bitType = defTypesChange[(int)type];
                                    bitValue = 0;
                                }

                                ulong shifted = 0;
                                if (bitType == DefType.u8) {
                                    shifted = (byte)value;
                                } else if (bitType == DefType.u16) {
                                    shifted = (ushort)value;
                                } else if (bitType == DefType.u32) {
                                    shifted = (uint)value;
                                }
                                // Shift left first to clear any out-of-range bits
                                shifted = shifted << (BIT_VALUE_SIZE - field.BitSize) >> (BIT_VALUE_SIZE - field.BitSize - bitOffset);
                                bitValue |= shifted;
                                bitOffset += field.BitSize;

                                if (i == l_pre || IsWritable(bitOffset, bitType, type, cells.At(i + 8).Def)) {
                                    bitOffset = -1;
                                    if (bitType == DefType.u8) {
                                        bw.WriteByte((byte)bitValue);
                                    } else if (bitType == DefType.u16) {
                                        bw.WriteUInt16((ushort)bitValue);
                                    } else if (bitType == DefType.u32) {
                                        bw.WriteUInt32((uint)bitValue);
                                    }
                                }
                            }
                            break;
                        default:
                            throw new NotImplementedException($"Unsupported field type: {type}");
                    }
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                static bool IsWritable(int bitOffset, DefType bitType, DefType type, PARAMDEF.Field nextField) =>
                    !ParamUtil.IsBitType(nextField.DisplayType) || nextField.BitSize == -1 || bitOffset + nextField.BitSize > ParamUtil.GetBitLimit(bitType) || defTypesChange[(int)type] != bitType;
            }

            internal void WriteName(BinaryWriterEx bw, PARAM parent, int i) {
                long nameOffset = 0;
                if (this.Name != null) {
                    nameOffset = bw.Position;
                    if (parent.Format2E.HasFlag(FormatFlags2.UnicodeRowNames)) {
                        bw.WriteUTF16(this.Name, true);
                    } else {
                        bw.WriteShiftJIS(this.Name, true);
                    }
                }

                if (parent.Format2D.HasFlag(FormatFlags1.LongDataOffset)) {
                    bw.FillInt64($"NameOffset{i}", nameOffset);
                } else {
                    bw.FillUInt32($"NameOffset{i}", (uint)nameOffset);
                }
            }

            /// <summary>
            /// Returns a string representation of the row.
            /// </summary>
            public override string ToString() => $"{this.ID} {this.Name}";

            /// <summary>
            /// Returns the first cell in the row with the given internal name.
            /// </summary>
            public Cell this[string name] => this.Cells.First(cell => cell.Def.InternalName == name);
        }
    }
}
