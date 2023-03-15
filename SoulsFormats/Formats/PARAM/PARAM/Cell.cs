namespace SoulsFormats {
    public partial class PARAM {
        /// <summary>
        /// One cell in one row in a param.
        /// </summary>
        public class Cell {
            /// <summary>
            /// The paramdef field that describes this cell.
            /// </summary>
            public readonly PARAMDEF.Field Def;

            /// <summary>
            /// The value of this cell.
            /// </summary>
            public object Value {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.value;
                set => this.value = value == null ? throw new NullReferenceException($"Cell value may not be null.") : this.Def.DisplayType switch {
                    PARAMDEF.DefType.s8 => Convert.ToSByte(value),
                    PARAMDEF.DefType.u8 => Convert.ToByte(value),
                    PARAMDEF.DefType.s16 => Convert.ToInt16(value),
                    PARAMDEF.DefType.u16 => Convert.ToUInt16(value),
                    PARAMDEF.DefType.s32 => Convert.ToInt32(value),
                    PARAMDEF.DefType.u32 => Convert.ToUInt32(value),
                    PARAMDEF.DefType.b32 => Convert.ToInt32(value),
                    PARAMDEF.DefType.f32 => Convert.ToSingle(value),
                    PARAMDEF.DefType.angle32 => Convert.ToSingle(value),
                    PARAMDEF.DefType.f64 => Convert.ToDouble(value),
                    PARAMDEF.DefType.fixstr => Convert.ToString(value),
                    PARAMDEF.DefType.fixstrW => Convert.ToString(value),
                    PARAMDEF.DefType.dummy8 => this.Def.BitSize == -1 ? (byte[])value : Convert.ToByte(value),
                    _ => throw new NotImplementedException($"Conversion not specified for type {this.Def.DisplayType}"),
                };
            }
            private object value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Cell(PARAMDEF.Field def, object value) {
                this.Def = def;
                this.value = value;
            }

            /// <summary>
            /// Returns a string representation of the cell.
            /// </summary>
            public override string ToString() => $"{this.Def.DisplayType} {this.Def.InternalName} = {this.Value}";

            /// <summary>
            /// Sets value of this cell by span param and converting it to type of this cell
            /// </summary>
            /// <param name="span"></param>
            public void SetValue(ReadOnlySpan<char> span) => this.value = this.Def.DisplayType switch {
                PARAMDEF.DefType.s8 => sbyte.Parse(span),
                PARAMDEF.DefType.u8 => byte.Parse(span),
                PARAMDEF.DefType.s16 => short.Parse(span),
                PARAMDEF.DefType.u16 => ushort.Parse(span),
                PARAMDEF.DefType.s32 => int.Parse(span),
                PARAMDEF.DefType.u32 => uint.Parse(span),
                PARAMDEF.DefType.b32 => int.Parse(span),
                PARAMDEF.DefType.f32 => float.Parse(span),
                PARAMDEF.DefType.angle32 => float.Parse(span),
                PARAMDEF.DefType.f64 => double.Parse(span),
                PARAMDEF.DefType.fixstr => new string(span),
                PARAMDEF.DefType.fixstrW => new string(span),
                PARAMDEF.DefType.dummy8 => this.Def.BitSize == -1 ? throw new NotSupportedException("Convertion from ReadOnlySpan<char> to byte[] is not supported") : (object)byte.Parse(span),
                _ => throw new NotImplementedException($"Conversion not specified for type {this.Def.DisplayType}"),
            };

            /// <summary>
            /// For Yapped
            /// </summary>
            public object Name {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.Def.InternalName;
            }

            /// <summary>
            /// For Yapped
            /// </summary>
            public object EditorName {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.Def.DisplayName;
            }

            /// <summary>
            /// For Yapped
            /// </summary>
            public object Type {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.Def.InternalType;
            }
        }
    }
}
