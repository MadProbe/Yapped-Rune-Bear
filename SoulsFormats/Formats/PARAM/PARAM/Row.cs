using System.Runtime.InteropServices;
using SoulsFormats.Formats.PARAM;
using SoulsFormats.Util;

namespace SoulsFormats {
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
                for (int i = 16, l = (count << 3) + 16; i < l; i += 8) {
                    PARAMDEF.Field field = fields.At(i);
                    cells.AssignAt(i, new Cell(field, ParamUtil.ConvertDefaultValue(field)));
                }
            }

            internal Row(BinaryReaderEx br, PARAM parent, ref long actualStringsOffset) {
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
                int start = 16, count = fields.Count, end = 16 + (count << 3);
                var cells = new Cell[count];

                int bitOffset = -1;
                PARAMDEF.DefType bitType = PARAMDEF.DefType.u8;
                ulong bitValue = 0; // This is ulong so checkOrphanedBits doesn't fail on offsets of 32
                //const int BIT_VALUE_SIZE = 64;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                void checkOrphanedBits() {
                    if (bitOffset != -1 && bitValue >> bitOffset != 0) {
                        throw new InvalidDataException($"Invalid paramdef {paramdef.ParamType}; bits would be lost before +0x{br.Position - this.DataOffset:X} in row {this.ID}.");
                    }
                }


                for (; start < end; start += 8) {
                    PARAMDEF.Field field = fields_array.At(start);
                    object value = null;
                    PARAMDEF.DefType type = field.DisplayType;

                    switch (type) {
                        case PARAMDEF.DefType.s8:
                            value = br.ReadSByte();
                            break;
                        case PARAMDEF.DefType.s16:
                            value = br.ReadInt16();
                            break;
                        case PARAMDEF.DefType.s32 or PARAMDEF.DefType.b32:
                            value = br.ReadInt32();
                            break;
                        case PARAMDEF.DefType.f32 or PARAMDEF.DefType.angle32:
                            value = br.ReadSingle();
                            break;
                        case PARAMDEF.DefType.f64:
                            value = br.ReadDouble();
                            break;
                        case PARAMDEF.DefType.fixstr:
                            value = br.ReadFixStr(field.ArrayLength);
                            break;
                        case PARAMDEF.DefType.fixstrW:
                            value = br.ReadFixStrW(field.ArrayLength << 1); // * 2
                            break;
                        default:
                            if (ParamUtil.IsBitType(type)) {
                                if (field.BitSize == -1) {
                                    if (type == PARAMDEF.DefType.u8) {
                                        value = br.ReadByte();
                                    } else if (type == PARAMDEF.DefType.u16) {
                                        value = br.ReadUInt16();
                                    } else if (type == PARAMDEF.DefType.u32) {
                                        value = br.ReadUInt32();
                                    } else if (type == PARAMDEF.DefType.dummy8) {
                                        value = br.ReadBytes(field.ArrayLength);
                                    }
                                }
                            } else {
                                throw new NotImplementedException($"Unsupported field type: {type}");
                            }

                            break;
                    }

                    if (value != null) {
                        checkOrphanedBits();
                        bitOffset = -1;
                    } else {
                        PARAMDEF.DefType newBitType = type == PARAMDEF.DefType.dummy8 ? PARAMDEF.DefType.u8 : type;
                        int bitLimit = ParamUtil.GetBitLimit(newBitType);

                        if (field.BitSize == 0) {
                            throw new NotImplementedException($"Bit size 0 is not supported.");
                        }

                        if (field.BitSize > bitLimit) {
                            throw new InvalidDataException($"Bit size {field.BitSize} is too large to fit in type {newBitType}.");
                        }

                        if (bitOffset == -1 || newBitType != bitType || bitOffset + field.BitSize > bitLimit) {
                            checkOrphanedBits();
                            bitOffset = 0;
                            bitType = newBitType;
                            if (bitType == PARAMDEF.DefType.u8) {
                                bitValue = br.ReadByte();
                            } else if (bitType == PARAMDEF.DefType.u16) {
                                bitValue = br.ReadUInt16();
                            } else if (bitType == PARAMDEF.DefType.u32) {
                                bitValue = br.ReadUInt32();
                            }
                        }

                        //_ = bitValue << (BIT_VALUE_SIZE - field.BitSize - bitOffset) >> (BIT_VALUE_SIZE - field.BitSize);
                        ulong shifted = System.Runtime.Intrinsics.X86.Bmi1.X64.BitFieldExtract(bitValue, (byte)bitOffset, (byte)field.BitSize);
                        bitOffset += field.BitSize;
                        if (bitType == PARAMDEF.DefType.u8) {
                            value = (byte)shifted;
                        } else if (bitType == PARAMDEF.DefType.u16) {
                            value = (ushort)shifted;
                        } else if (bitType == PARAMDEF.DefType.u32) {
                            value = (uint)shifted;
                        }
                    }

                    cells.AssignAnyAt(start, new Cell(field, value));
                }
                checkOrphanedBits();
                this.Cells = cells;
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
                PARAMDEF.DefType bitType = PARAMDEF.DefType.u8;
                ulong bitValue = 0;
                const int BIT_VALUE_SIZE = 64;
                Cell[] cells = this.Cells.CastTo<IReadOnlyList<Cell>, Cell[]>();

                for (int i = 16, l_pre = (cells.Length << 3) + 8, l = l_pre + 8; i < l; i += 8) {
                    Cell cell = cells.At(i);
                    object value = cell.Value;
                    PARAMDEF.Field field = cell.Def;
                    PARAMDEF.DefType type = field.DisplayType;

                    switch (type) {
                        case PARAMDEF.DefType.s8:
                            bw.WriteSByte((sbyte)value);
                            break;
                        case PARAMDEF.DefType.s16:
                            bw.WriteInt16((short)value);
                            break;
                        case PARAMDEF.DefType.s32 or PARAMDEF.DefType.b32:
                            bw.WriteInt32((int)value);
                            break;
                        case PARAMDEF.DefType.f32 or PARAMDEF.DefType.angle32:
                            bw.WriteSingle((float)value);
                            break;
                        case PARAMDEF.DefType.f64:
                            bw.WriteDouble((double)value);
                            break;
                        case PARAMDEF.DefType.fixstr:
                            bw.WriteFixStr((string)value, field.ArrayLength);
                            break;
                        case PARAMDEF.DefType.fixstrW:
                            bw.WriteFixStrW((string)value, field.ArrayLength << 1); // * 2
                            break;
                        default:
                            if (ParamUtil.IsBitType(type)) {
                                if (field.BitSize == -1) {
                                    if (type == PARAMDEF.DefType.u8) {
                                        bw.WriteByte((byte)value);
                                    } else if (type == PARAMDEF.DefType.u16) {
                                        bw.WriteUInt16((ushort)value);
                                    } else if (type == PARAMDEF.DefType.u32) {
                                        bw.WriteUInt32((uint)value);
                                    } else if (type == PARAMDEF.DefType.dummy8) {
                                        bw.WriteBytes((byte[])value);
                                    }
                                } else {
                                    if (bitOffset == -1) {
                                        bitOffset = 0;
                                        bitType = type == PARAMDEF.DefType.dummy8 ? PARAMDEF.DefType.u8 : type;
                                        bitValue = 0;
                                    }

                                    uint shifted = 0;
                                    if (bitType == PARAMDEF.DefType.u8) {
                                        shifted = (byte)value;
                                    } else if (bitType == PARAMDEF.DefType.u16) {
                                        shifted = (ushort)value;
                                    } else if (bitType == PARAMDEF.DefType.u32) {
                                        shifted = (uint)value;
                                    }
                                    // Shift left first to clear any out-of-range bits
                                    shifted = shifted << (BIT_VALUE_SIZE - field.BitSize) >> (BIT_VALUE_SIZE - field.BitSize - bitOffset);
                                    bitValue |= shifted;
                                    bitOffset += field.BitSize;

                                    bool write = false;
                                    if (i == l_pre) {
                                        write = true;
                                    } else {
                                        PARAMDEF.Field nextField = cells.At(i + 8).Def;
                                        PARAMDEF.DefType nextType = nextField.DisplayType;
                                        int bitLimit = ParamUtil.GetBitLimit(bitType);
                                        if (!ParamUtil.IsBitType(nextType) || nextField.BitSize == -1 || bitOffset + nextField.BitSize > bitLimit
                                            || (nextType == PARAMDEF.DefType.dummy8 ? PARAMDEF.DefType.u8 : nextType) != bitType) {
                                            write = true;
                                        }
                                    }

                                    if (write) {
                                        bitOffset = -1;
                                        if (bitType == PARAMDEF.DefType.u8) {
                                            bw.WriteByte((byte)bitValue);
                                        } else if (bitType == PARAMDEF.DefType.u16) {
                                            bw.WriteUInt16((ushort)bitValue);
                                        } else if (bitType == PARAMDEF.DefType.u32) {
                                            bw.WriteUInt32((uint)bitValue);
                                        }
                                    }
                                }
                            } else {
                                throw new NotImplementedException($"Unsupported field type: {type}");
                            }

                            break;
                    }
                }
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
