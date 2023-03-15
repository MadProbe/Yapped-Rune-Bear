using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using SoulsFormats.Formats.PARAM;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class PARAMDEF {
        /// <summary>
        /// Supported primitive field types.
        /// </summary>
        public enum DefType : byte {
            /// <summary>
            /// Signed 1-byte integer.
            /// </summary>
            s8,

            /// <summary>
            /// Unsigned 1-byte integer.
            /// </summary>
            u8,

            /// <summary>
            /// Signed 2-byte integer.
            /// </summary>
            s16,

            /// <summary>
            /// Unsigned 2-byte integer.
            /// </summary>
            u16,

            /// <summary>
            /// Signed 4-byte integer.
            /// </summary>
            s32,

            /// <summary>
            /// Unsigned 4-byte integer.
            /// </summary>
            u32,

            /// <summary>
            /// 4-byte integer representing a boolean.
            /// </summary>
            b32,

            /// <summary>
            /// Single-precision floating point value.
            /// </summary>
            f32,

            /// <summary>
            /// Single-precision floating point value representing an angle.
            /// </summary>
            angle32,

            /// <summary>
            /// Double-precision floating point value.
            /// </summary>
            f64,

            /// <summary>
            /// Byte or array of bytes used for padding or placeholding.
            /// </summary>
            dummy8,

            /// <summary>
            /// Fixed-width Shift-JIS string.
            /// </summary>
            fixstr,

            /// <summary>
            /// Fixed-width UTF-16 string.
            /// </summary>
            fixstrW,
        }

        /// <summary>
        /// Flags that control editor behavior for a field.
        /// </summary>
        [Flags]
        public enum EditFlags : byte {
            /// <summary>
            /// Value is editable and does not wrap.
            /// </summary>
            None = 0,

            /// <summary>
            /// Value wraps around when scrolled past the minimum or maximum.
            /// </summary>
            Wrap = 1,

            /// <summary>
            /// Value may not be edited.
            /// </summary>
            Lock = 4,
        }

        /// <summary>
        /// Information about a field present in each row in a param.
        /// </summary>
        public partial class Field {
            /// <summary>
            /// PARAMDEF associated with this field
            /// </summary>
            public PARAMDEF Def;

            /// <summary>
            /// Name to display in the editor.
            /// </summary>
            public string DisplayName;

            /// <summary>
            /// Type of value to display in the editor.
            /// </summary>
            public DefType DisplayType;

            /// <summary>
            /// Flags determining behavior of the field in the editor.
            /// </summary>
            public EditFlags EditFlags;

            /// <summary>
            /// Printf-style format string to apply to the value in the editor.
            /// </summary>
            public string DisplayFormat;

            /// <summary>
            /// Default value for new rows.
            /// </summary>
            public object Default;

            /// <summary>
            /// Minimum valid value.
            /// </summary>
            public object Minimum;

            /// <summary>
            /// Maximum valid value.
            /// </summary>
            public object Maximum;

            /// <summary>
            /// Amount of increase or decrease per step when scrolling in the editor.
            /// </summary>
            public object Increment;

            /// <summary>
            /// Number of elements for array types; only supported for dummy8, fixstr, and fixstrW.
            /// </summary>
            public int ArrayLength;

            /// <summary>
            /// Optional description of the field; may be null.
            /// </summary>
            public string Description;

            /// <summary>
            /// Type of the value in the engine; may be an enum type.
            /// </summary>
            public string InternalType;

            /// <summary>
            /// Name of the value in the engine; not present before version 102.
            /// </summary>
            public string InternalName;

            /// <summary>
            /// Number of bits used by a bitfield; only supported for unsigned types, -1 when not used.
            /// </summary>
            public int BitSize;

            /// <summary>
            /// Fields are ordered by this value in the editor; not present before version 104.
            /// </summary>
            public int SortID;

            /// <summary>
            /// Unknown; appears to be an identifier. May be null, only supported in versions >= 200, only present in version 202 so far.
            /// </summary>
            public string UnkB8;

            /// <summary>
            /// Unknown; appears to be a param type. May be null, only supported in versions >= 200, only present in version 202 so far.
            /// </summary>
            public string UnkC0;

            /// <summary>
            /// Unknown; appears to be a display string. May be null, only supported in versions >= 200, only present in version 202 so far.
            /// </summary>
            public string UnkC8;

            private static readonly Regex arrayLengthRx = ArrayLengthRegex();
            private static readonly Regex bitSizeRx = BitSizeRegex();

            /// <summary>
            /// Creates a Field with placeholder values.
            /// </summary>
            public Field() : this(null, DefType.f32, "placeholder") { }

            /// <summary>
            /// Creates a Field with the given type, name, and appropriate default values.
            /// </summary>
            public Field(PARAMDEF def, DefType displayType, string internalName) {
                this.Def = def;
                this.DisplayName = internalName;
                this.DisplayType = displayType;
                this.DisplayFormat = ParamUtil.GetDefaultFormat(this.DisplayType);
                this.Default = ParamUtil.GetDefaultDefault(def, this.DisplayType);
                this.Minimum = ParamUtil.GetDefaultMinimum(def, this.DisplayType);
                this.Maximum = ParamUtil.GetDefaultMaximum(def, this.DisplayType);
                this.Increment = ParamUtil.GetDefaultIncrement(def, this.DisplayType);
                this.EditFlags = ParamUtil.GetDefaultEditFlags(this.DisplayType);
                this.ArrayLength = 1;
                this.InternalType = this.DisplayType.ToString();
                this.InternalName = internalName;
                this.BitSize = -1;
            }

            internal Field(BinaryReaderEx br, PARAMDEF def) {
                this.Def = def;
                this.DisplayName = def.FormatVersion is >= 202 or >= 106 and < 200
                    ? br.GetUTF16(br.ReadVarint())
                    : def.Unicode ? br.ReadFixStrW(0x40) : br.ReadFixStr(0x40);

                this.DisplayType = Enum.Parse<DefType>(br.ReadFixStr(8));
                this.DisplayFormat = br.ReadFixStr(8);

                if (def.FormatVersion >= 203) {
                    br.AssertPattern(0x10, 0x00);
                } else {
                    this.Default = br.ReadSingle();
                    this.Minimum = br.ReadSingle();
                    this.Maximum = br.ReadSingle();
                    this.Increment = br.ReadSingle();
                }

                this.EditFlags = (EditFlags)br.ReadInt32();

                int byteCount = br.ReadInt32();
                if (!ParamUtil.IsArrayType(this.DisplayType) && byteCount != ParamUtil.GetValueSize(this.DisplayType)
                    || ParamUtil.IsArrayType(this.DisplayType) && byteCount % ParamUtil.GetValueSize(this.DisplayType) != 0) {
                    throw new InvalidDataException($"Unexpected byte count {byteCount} for type {this.DisplayType}.");
                }

                this.ArrayLength = byteCount / ParamUtil.GetValueSize(this.DisplayType);

                long descriptionOffset = br.ReadVarint();
                if (descriptionOffset != 0) {
                    this.Description = def.Unicode ? br.GetUTF16(descriptionOffset) : br.GetShiftJIS(descriptionOffset);
                }

                this.InternalType = def.FormatVersion is >= 202 or >= 106 and < 200
                    ? br.GetASCII(br.ReadVarint()).Trim()
                    : br.ReadFixStr(0x20).Trim();

                this.BitSize = -1;
                if (def.FormatVersion >= 102) {
                    this.InternalName = def.FormatVersion is >= 202 or >= 106 and < 200
                        ? br.GetASCII(br.ReadVarint()).Trim()
                        : br.ReadFixStr(0x20).Trim();

                    Match match = bitSizeRx.Match(this.InternalName);
                    if (match.Success) {
                        this.InternalName = match.Groups["name"].Value;
                        this.BitSize = int.Parse(match.Groups["size"].Value);
                    }

                    if (ParamUtil.IsArrayType(this.DisplayType)) {
                        match = arrayLengthRx.Match(this.InternalName);
                        int length = match.Success ? int.Parse(match.Groups["length"].Value) : 1;
                        if (length != this.ArrayLength) {
                            throw new InvalidDataException($"Mismatched array length in {this.InternalName} with byte count {byteCount}.");
                        }

                        if (match.Success) {
                            this.InternalName = match.Groups["name"].Value;
                        }
                    }
                }

                if (def.FormatVersion >= 104) {
                    this.SortID = br.ReadInt32();
                }

                if (def.FormatVersion >= 200) {
                    _ = br.AssertInt32(0);
                    long unkB8Offset = br.ReadInt64();
                    long unkC0Offset = br.ReadInt64();
                    long unkC8Offset = br.ReadInt64();

                    if (unkB8Offset != 0) {
                        this.UnkB8 = br.GetASCII(unkB8Offset);
                    }

                    if (unkC0Offset != 0) {
                        this.UnkC0 = br.GetASCII(unkC0Offset);
                    }

                    if (unkC8Offset != 0) {
                        this.UnkC8 = br.GetUTF16(unkC8Offset);
                    }
                } else if (def.FormatVersion >= 106) {
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                if (def.FormatVersion >= 203) {
                    object readVariableValue() {
                        object value;
                        switch (this.DisplayType) {
                            case DefType.s8:
                            case DefType.u8:
                            case DefType.s16:
                            case DefType.u16:
                            case DefType.s32:
                            case DefType.u32:
                            case DefType.b32:
                                value = br.ReadInt32();
                                _ = br.AssertInt32(0);
                                break;

                            case DefType.f32:
                            case DefType.angle32:
                                value = br.ReadSingle();
                                _ = br.AssertInt32(0);
                                break;

                            case DefType.f64:
                                value = br.ReadDouble();
                                break;

                            // Given that there are 8 bytes available, these could possibly be offsets
                            case DefType.dummy8:
                            case DefType.fixstr:
                            case DefType.fixstrW:
                                value = null;
                                _ = br.AssertInt64(0);
                                break;

                            default:
                                throw new NotImplementedException($"Missing variable read for type: {this.DisplayType}");
                        }
                        return value;
                    }

                    this.Default = readVariableValue();
                    this.Minimum = readVariableValue();
                    this.Maximum = readVariableValue();
                    this.Increment = readVariableValue();
                }
            }

            internal void Write(BinaryWriterEx bw, PARAMDEF def, int index) {
                if (def.FormatVersion is >= 202 or >= 106 and < 200) {
                    bw.ReserveVarint($"DisplayNameOffset{index}");
                } else if (def.Unicode) {
                    bw.WriteFixStrW(this.DisplayName, 0x40, (byte)(def.FormatVersion >= 104 ? 0x00 : 0x20));
                } else {
                    bw.WriteFixStr(this.DisplayName, 0x40, (byte)(def.FormatVersion >= 104 ? 0x00 : 0x20));
                }

                byte padding = (byte)(def.FormatVersion >= 106 ? 0x00 : 0x20);
                bw.WriteFixStr(this.DisplayType.ToString(), 8, padding);
                bw.WriteFixStr(this.DisplayFormat, 8, padding);

                if (def.FormatVersion >= 203) {
                    bw.WritePattern(0x10, 0x00);
                } else {
                    bw.WriteSingle(Convert.ToSingle(this.Default));
                    bw.WriteSingle(Convert.ToSingle(this.Minimum));
                    bw.WriteSingle(Convert.ToSingle(this.Maximum));
                    bw.WriteSingle(Convert.ToSingle(this.Increment));
                }

                bw.WriteInt32((int)this.EditFlags);
                bw.WriteInt32(ParamUtil.GetValueSize(this.DisplayType) * (ParamUtil.IsArrayType(this.DisplayType) ? this.ArrayLength : 1));
                bw.ReserveVarint($"DescriptionOffset{index}");

                if (def.FormatVersion is >= 202 or >= 106 and < 200) {
                    bw.ReserveVarint($"InternalTypeOffset{index}");
                } else {
                    bw.WriteFixStr(this.InternalType, 0x20, padding);
                }

                if (def.FormatVersion is >= 202 or >= 106 and < 200) {
                    bw.ReserveVarint($"InternalNameOffset{index}");
                } else if (def.FormatVersion >= 102) {
                    bw.WriteFixStr(this.MakeInternalName(), 0x20, padding);
                }

                if (def.FormatVersion >= 104) {
                    bw.WriteInt32(this.SortID);
                }

                if (def.FormatVersion >= 200) {
                    bw.WriteInt32(0);
                    bw.ReserveInt64($"UnkB8Offset{index}");
                    bw.ReserveInt64($"UnkC0Offset{index}");
                    bw.ReserveInt64($"UnkC8Offset{index}");
                } else if (def.FormatVersion >= 106) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                if (def.FormatVersion >= 203) {
                    void writeVariableValue(object value) {
                        switch (this.DisplayType) {
                            case DefType.s8:
                            case DefType.u8:
                            case DefType.s16:
                            case DefType.u16:
                            case DefType.s32:
                            case DefType.u32:
                            case DefType.b32:
                                bw.WriteInt32(Convert.ToInt32(value));
                                bw.WriteInt32(0);
                                break;

                            case DefType.f32:
                            case DefType.angle32:
                                bw.WriteSingle(Convert.ToSingle(value));
                                bw.WriteInt32(0);
                                break;

                            case DefType.f64:
                                bw.WriteDouble(Convert.ToDouble(value));
                                break;

                            case DefType.dummy8:
                            case DefType.fixstr:
                            case DefType.fixstrW:
                                bw.WriteInt64(0);
                                break;

                            default:
                                throw new NotImplementedException($"Missing variable write for type: {this.DisplayType}");
                        }
                    }

                    writeVariableValue(this.Default);
                    writeVariableValue(this.Minimum);
                    writeVariableValue(this.Maximum);
                    writeVariableValue(this.Increment);
                }
            }

            internal void WriteStrings(BinaryWriterEx bw, PARAMDEF def, int index, Dictionary<string, long> sharedStringOffsets) {
                if (def.FormatVersion is >= 202 or >= 106 and < 200) {
                    bw.FillVarint($"DisplayNameOffset{index}", bw.Position);
                    bw.WriteUTF16(this.DisplayName, true);
                }

                long descriptionOffset = 0;
                if (this.Description != null) {
                    descriptionOffset = bw.Position;
                    if (def.Unicode) {
                        bw.WriteUTF16(this.Description, true);
                    } else {
                        bw.WriteShiftJIS(this.Description, true);
                    }
                }
                bw.FillVarint($"DescriptionOffset{index}", descriptionOffset);

                if (def.FormatVersion is >= 202 or >= 106 and < 200) {
                    bw.FillVarint($"InternalTypeOffset{index}", bw.Position);
                    bw.WriteASCII(this.InternalType, true);

                    bw.FillVarint($"InternalNameOffset{index}", bw.Position);
                    bw.WriteASCII(this.MakeInternalName(), true);
                }

                if (def.FormatVersion >= 200) {
                    long writeSharedStringMaybe(string str, bool unicode) {
                        if (str == null) {
                            return 0;
                        }

                        if (!sharedStringOffsets.ContainsKey(str)) {
                            sharedStringOffsets[str] = bw.Position;
                            if (unicode) {
                                bw.WriteUTF16(str, true);
                            } else {
                                bw.WriteASCII(str, true);
                            }
                        }
                        return sharedStringOffsets[str];
                    }

                    bw.FillInt64($"UnkB8Offset{index}", writeSharedStringMaybe(this.UnkB8, false));
                    bw.FillInt64($"UnkC0Offset{index}", writeSharedStringMaybe(this.UnkC0, false));
                    bw.FillInt64($"UnkC8Offset{index}", writeSharedStringMaybe(this.UnkC8, true));
                }
            }

            private string MakeInternalName() =>
                // This formatting is almost 100% accurate in DS1, less so in BB, and a complete crapshoot in DS3
                // C'est la vie.
                this.BitSize != -1
                    ? $"{this.InternalName}:{this.BitSize}"
                    : ParamUtil.IsArrayType(this.DisplayType) ? $"{this.InternalName}[{this.ArrayLength}]" : this.InternalName;

            /// <summary>
            /// Returns a string representation of the field.
            /// </summary>
            public override string ToString() => ParamUtil.IsBitType(this.DisplayType) && this.BitSize != -1
                    ? $"{this.DisplayType} {this.InternalName}:{this.BitSize}"
                    : ParamUtil.IsArrayType(this.DisplayType)
                    ? $"{this.DisplayType} {this.InternalName}[{this.ArrayLength}]"
                    : $"{this.DisplayType} {this.InternalName}";
            [GeneratedRegex("^\\s*(?<name>.+?)\\s*\\[\\s*(?<length>\\d+)\\s*\\]\\s*$")]
            private static partial Regex ArrayLengthRegex();
            [GeneratedRegex("^\\s*(?<name>.+?)\\s*\\:\\s*(?<size>\\d+)\\s*$")]
            private static partial Regex BitSizeRegex();
        }
    }
}
