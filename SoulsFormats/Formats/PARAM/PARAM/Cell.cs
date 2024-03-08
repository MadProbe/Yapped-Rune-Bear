using System;
using System.Text;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class PARAM {
        /// <summary>
        ///     One cell in one row in a param.
        /// </summary>
        public interface ICell {
            /// <summary>
            ///     The paramdef field that describes this cell.
            /// </summary>
            PARAMDEF.Field Def { get; }

            /// <summary>
            ///     For Yapped
            /// </summary>
            string EditorName { get; }

            /// <summary>
            ///     For Yapped
            /// </summary>
            string Name { get; }

            /// <summary>
            ///     For Yapped
            /// </summary>
            string Type { get; }

            /// <summary>
            ///     The value of this cell.
            /// </summary>
            object Value { get; set; }

            /// <summary>
            ///     Returns a string representation of the cell.
            /// </summary>
            string ToString();
        }

        // /// <summary>
        // /// One cell in one row in a param.
        // /// </summary>
        // [Obsolete("Shitty design, now unused")]
        // public class Cell : ICell {
        //     /// <summary>
        //     /// 
        //     /// </summary>
        //     public static int cell_byte_used { get; set; }
        //     /// <summary>
        //     /// The paramdef field that describes this cell.
        //     /// </summary>
        //     public PARAMDEF.Field Def { get; init; }

        //     /// <summary>
        //     /// The value of this cell.
        //     /// </summary>
        //     public object Value {
        //         [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //         get => this.value;
        //         set => this.value = value == null ? throw new NullReferenceException("Cell value may not be null.") : this.Def.DisplayType switch {
        //             PARAMDEF.DefType.s8 => Convert.ToSByte(value),
        //             PARAMDEF.DefType.u8 => Convert.ToByte(value),
        //             PARAMDEF.DefType.s16 => Convert.ToInt16(value),
        //             PARAMDEF.DefType.u16 => Convert.ToUInt16(value),
        //             PARAMDEF.DefType.s32 => Convert.ToInt32(value),
        //             PARAMDEF.DefType.u32 => Convert.ToUInt32(value),
        //             PARAMDEF.DefType.b32 => Convert.ToInt32(value),
        //             PARAMDEF.DefType.f32 => Convert.ToSingle(value),
        //             PARAMDEF.DefType.angle32 => Convert.ToSingle(value),
        //             PARAMDEF.DefType.f64 => Convert.ToDouble(value),
        //             PARAMDEF.DefType.fixstr => Convert.ToString(value),
        //             PARAMDEF.DefType.fixstrW => Convert.ToString(value),
        //             PARAMDEF.DefType.dummy8 => this.Def.BitSize == -1 ? (byte[])value : Convert.ToByte(value),
        //             _ => throw new NotImplementedException($"Conversion not specified for type {this.Def.DisplayType}"),
        //         };
        //     }
        //     private object value;

        //     [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //     internal Cell(PARAMDEF.Field def, object value) {
        //         cell_byte_used += 40;
        //         this.Def = def;
        //         this.value = value;
        //     }

        //     /// <summary>
        //     /// Returns a string representation of the cell.
        //     /// </summary>
        //     public override string ToString() => $"{this.Def.DisplayType} {this.Def.InternalName} = {this.Value}";

        //     /// <summary>
        //     /// Sets value of this cell by span param and converting it to type of this cell
        //     /// </summary>
        //     /// <param name="span"></param>
        //     public void SetValue(ReadOnlySpan<char> span) => this.value = this.Def.DisplayType switch {
        //         PARAMDEF.DefType.s8 => sbyte.Parse(span),
        //         PARAMDEF.DefType.u8 => byte.Parse(span),
        //         PARAMDEF.DefType.s16 => short.Parse(span),
        //         PARAMDEF.DefType.u16 => ushort.Parse(span),
        //         PARAMDEF.DefType.s32 => int.Parse(span),
        //         PARAMDEF.DefType.u32 => uint.Parse(span),
        //         PARAMDEF.DefType.b32 => int.Parse(span),
        //         PARAMDEF.DefType.f32 => float.Parse(span),
        //         PARAMDEF.DefType.angle32 => float.Parse(span),
        //         PARAMDEF.DefType.f64 => double.Parse(span),
        //         PARAMDEF.DefType.fixstr => new string(span),
        //         PARAMDEF.DefType.fixstrW => new string(span),
        //         PARAMDEF.DefType.dummy8 => byte.Parse(span),
        //         _ => throw new NotImplementedException($"Conversion not specified for type {this.Def.DisplayType}"),
        //     };

        //     /// <summary>
        //     /// For Yapped
        //     /// </summary>
        //     public object Name {
        //         [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //         get => this.Def.InternalName;
        //     }

        //     /// <summary>
        //     /// For Yapped
        //     /// </summary>
        //     public object EditorName {
        //         [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //         get => this.Def.DisplayName;
        //     }

        //     /// <summary>
        //     /// For Yapped
        //     /// </summary>
        //     public object Type {
        //         [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //         get => this.Def.InternalType;
        //     }
        // }
        /// <summary>
        ///     One cell in one row in a param.
        /// </summary>
        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
        public struct MiniCell {
            /// <summary>
            /// </summary>
            [FieldOffset(0)] public byte ByteValue;
            /// <summary>
            /// </summary>
            [FieldOffset(0)] public sbyte SByteValue;
            /// <summary>
            /// </summary>
            [FieldOffset(0)] public short ShortValue;
            /// <summary>
            /// </summary>
            [FieldOffset(0)] public ushort UShortValue;
            /// <summary>
            /// </summary>
            [FieldOffset(0)] public int IntValue;
            /// <summary>
            /// </summary>
            [FieldOffset(0)] public uint UIntValue;
            /// <summary>
            /// </summary>
            [FieldOffset(0)] public long LongValue;
            /// <summary>
            /// </summary>
            [FieldOffset(0)] public ulong ULongValue;
            /// <summary>
            /// </summary>
            [FieldOffset(0)] public float FloatValue;
            /// <summary>
            /// </summary>
            [FieldOffset(0)] public double DoubleValue;
            /// <summary>
            /// </summary>
            [FieldOffset(0)] public char CharPointerValue;
            /// <summary>
            /// </summary>
            [FieldOffset(0)] public byte BytePointerValue;

            /// <summary>
            ///     Sets value of this cell by span param and converting it to type of this cell
            /// </summary>
            /// <param name="type"></param>
            /// <param name="span"></param>
            /// <param name="array_length"></param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetValue(PARAMDEF.DefType type, ReadOnlySpan<char> span, int array_length) {
                switch (type) {
                    case PARAMDEF.DefType.s8:
                        this.SByteValue = sbyte.Parse(span);
                        break;
                    case PARAMDEF.DefType.u8:
                        this.ByteValue = byte.Parse(span);
                        break;
                    case PARAMDEF.DefType.s16:
                        this.ShortValue = short.Parse(span);
                        break;
                    case PARAMDEF.DefType.u16:
                        this.UShortValue = ushort.Parse(span);
                        break;
                    case PARAMDEF.DefType.s32:
                        this.IntValue = int.Parse(span);
                        break;
                    case PARAMDEF.DefType.u32:
                        this.UIntValue = uint.Parse(span);
                        break;
                    case PARAMDEF.DefType.b32:
                        this.IntValue = int.Parse(span);
                        break;
                    case PARAMDEF.DefType.f32:
                        this.FloatValue = float.Parse(span);
                        break;
                    case PARAMDEF.DefType.angle32:
                        this.FloatValue = float.Parse(span);
                        break;
                    case PARAMDEF.DefType.f64:
                        this.DoubleValue = double.Parse(span);
                        break;
                    case PARAMDEF.DefType.fixstr:
                        SFEncoding.ShiftJIS.GetBytes(span, MemoryMarshal.CreateSpan(ref Unsafe.As<MiniCell, byte>(ref this), array_length));
                        break;
                    case PARAMDEF.DefType.fixstrW:
                        Memmove(ref Unsafe.As<MiniCell, char>(ref this), ref MemoryMarshal.GetReference(span),
                                Math.Min(array_length >> 1, span.Length));
                        break;
                    case PARAMDEF.DefType.dummy8:
                        Memmove(ref Unsafe.As<MiniCell, char>(ref this), ref MemoryMarshal.GetReference(span),
                                Math.Min(array_length, span.Length));
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(type));
                }
            }
        }

        /// <summary>
        /// </summary>
        public unsafe class MiniCellWrapper : ICell {
            private object cached_value;
            /// <summary>
            /// </summary>
            public MiniCell* Cell;

            /// <summary>
            /// </summary>
            /// <param name="def"></param>
            /// <param name="cell"></param>
            public MiniCellWrapper(PARAMDEF.Field def, MiniCell* cell) {
                this.Def = def;
                this.Cell = cell;
            }

            /// <summary>
            /// </summary>
            public PARAMDEF.Field Def { get; set; }

            /// <summary>
            ///     The value of this cell.
            /// </summary>
            public object Value {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.cached_value is not null && this.Def.DisplayType switch {
                    PARAMDEF.DefType.s8 => (sbyte)this.cached_value == this.Cell->SByteValue,
                    PARAMDEF.DefType.u8 => (byte)this.cached_value == this.Cell->ByteValue,
                    PARAMDEF.DefType.s16 => (short)this.cached_value == this.Cell->ShortValue,
                    PARAMDEF.DefType.u16 => (ushort)this.cached_value == this.Cell->UShortValue,
                    PARAMDEF.DefType.s32 => (int)this.cached_value == this.Cell->IntValue,
                    PARAMDEF.DefType.u32 => (uint)this.cached_value == this.Cell->UIntValue,
                    PARAMDEF.DefType.b32 => (int)this.cached_value == this.Cell->IntValue,
                    PARAMDEF.DefType.f32 => (float)this.cached_value == this.Cell->FloatValue,
                    PARAMDEF.DefType.angle32 => (float)this.cached_value == this.Cell->FloatValue,
                    PARAMDEF.DefType.f64 => (double)this.cached_value == this.Cell->DoubleValue,
                    PARAMDEF.DefType.fixstr => (string)this.cached_value ==
                                               SFEncoding.ShiftJIS.GetString(MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(this.Cell),
                                                                                 this.Def.ArrayLength)),
                    PARAMDEF.DefType.fixstrW => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<char>(this.Cell), this.Def.ArrayLength) ==
                                                (string)this.cached_value,
                    PARAMDEF.DefType.dummy8 when this.Def.BitSize == -1 => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<byte>(this.Cell),
                        this.Def.ArrayLength) == (byte[])this.cached_value,
                    PARAMDEF.DefType.dummy8 => (byte)this.cached_value == this.Cell->ByteValue,
                    _ => throw new NotImplementedException($"Conversion not specified for type {this.Def.DisplayType}"),
                }
                    ? this.cached_value
                    : this.cached_value = this.Def.DisplayType switch {
                        PARAMDEF.DefType.s8 => this.Cell->SByteValue,
                        PARAMDEF.DefType.u8 => this.Cell->ByteValue,
                        PARAMDEF.DefType.s16 => this.Cell->ShortValue,
                        PARAMDEF.DefType.u16 => this.Cell->UShortValue,
                        PARAMDEF.DefType.s32 => this.Cell->IntValue,
                        PARAMDEF.DefType.u32 => this.Cell->UIntValue,
                        PARAMDEF.DefType.b32 => this.Cell->IntValue,
                        PARAMDEF.DefType.f32 => this.Cell->FloatValue,
                        PARAMDEF.DefType.angle32 => this.Cell->FloatValue,
                        PARAMDEF.DefType.f64 => this.Cell->DoubleValue,
                        PARAMDEF.DefType.fixstr => SFEncoding.ShiftJIS.GetString(MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(this.Cell),
                                                                                     this.Def.ArrayLength)),
                        PARAMDEF.DefType.fixstrW => new string(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<char>(this.Cell),
                                                                                                this.Def.ArrayLength)),
                        PARAMDEF.DefType.dummy8 when this.Def.BitSize == -1 => MemoryMarshal
                                                                               .CreateReadOnlySpan(ref Unsafe.AsRef<byte>(this.Cell),
                                                                                                   this.Def.ArrayLength).ToArray(),
                        PARAMDEF.DefType.dummy8 => this.Cell->ByteValue,
                        _ => throw new
                            NotImplementedException($"Conversion not specified for type {this.Def.DisplayType}"),
                    };

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set {
                    switch (this.Def.DisplayType) {
                        case PARAMDEF.DefType.s8:
                            this.Cell->SByteValue = Convert.ToSByte(value);
                            break;
                        case PARAMDEF.DefType.u8:
                            this.Cell->ByteValue = Convert.ToByte(value);
                            break;
                        case PARAMDEF.DefType.s16:
                            this.Cell->ShortValue = Convert.ToInt16(value);
                            break;
                        case PARAMDEF.DefType.u16:
                            this.Cell->UShortValue = Convert.ToUInt16(value);
                            break;
                        case PARAMDEF.DefType.s32:
                            this.Cell->IntValue = Convert.ToInt32(value);
                            break;
                        case PARAMDEF.DefType.u32:
                            this.Cell->UIntValue = Convert.ToUInt32(value);
                            break;
                        case PARAMDEF.DefType.b32:
                            this.Cell->IntValue = Convert.ToInt32(value);
                            break;
                        case PARAMDEF.DefType.f32:
                            this.Cell->FloatValue = Convert.ToSingle(value);
                            break;
                        case PARAMDEF.DefType.angle32:
                            this.Cell->FloatValue = Convert.ToSingle(value);
                            break;
                        case PARAMDEF.DefType.f64:
                            this.Cell->DoubleValue = Convert.ToDouble(value);
                            break;
                        case PARAMDEF.DefType.fixstr:
                            SFEncoding.ShiftJIS.GetBytes((string)value,
                                                         MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(this.Cell), this.Def.ArrayLength));
                            break;
                        case PARAMDEF.DefType.fixstrW:
                            Memmove(ref Unsafe.AsRef<char>(this.Cell), ref MemoryMarshal.GetReference((ReadOnlySpan<char>)(string)value),
                                    Math.Min(this.Def.ArrayLength >> 1, ((string)value).Length));
                            break;
                        case PARAMDEF.DefType.dummy8 when this.Def.BitSize == -1:
                            Memmove(ref Unsafe.AsRef<byte>(this.Cell), ref MemoryMarshal.GetArrayDataReference((byte[])value),
                                    Math.Min(this.Def.ArrayLength, ((byte[])value).Length));
                            break;
                        case PARAMDEF.DefType.dummy8:
                            this.Cell->ByteValue = Convert.ToByte(value);
                            break;
                        default:
                            Unsafe.NullRef<object>();
                            break;
                    }
                }
            }

            /// <summary>
            ///     For Yapped
            /// </summary>
            public string Name {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.Def.InternalName;
            }

            /// <summary>
            ///     For Yapped
            /// </summary>
            public string EditorName {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.Def.DisplayName;
            }

            /// <summary>
            ///     For Yapped
            /// </summary>
            public string Type {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.Def.InternalType;
            }

            /// <summary>
            ///     Returns a string representation of the cell.
            /// </summary>
            public override string ToString() => $"{this.Def.DisplayType} {this.Def.InternalName} = {this.Value}";
        }
    }
}
