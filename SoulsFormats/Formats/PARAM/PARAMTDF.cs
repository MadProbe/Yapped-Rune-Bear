using System.Text;

namespace SoulsFormats.Formats.PARAM {
    /// <summary>
    /// A companion format to PARAM and PARAMDEF that provides friendly names for enumerated value types. Extension: .tdf
    /// </summary>
    public class PARAMTDF {
        /// <summary>
        /// The identifier of this type.
        /// </summary>
        public string Name;

        /// <summary>
        /// The type of values in this TDF; must be an integral type.
        /// </summary>
        public PARAMDEF.DefType Type {
            get => this.type;
            set => this.type = value is PARAMDEF.DefType.s8 or PARAMDEF.DefType.u8
                or PARAMDEF.DefType.s16 or PARAMDEF.DefType.u16
                or PARAMDEF.DefType.s32 or PARAMDEF.DefType.u32 ? value : throw new ArgumentException($"TDF type may only be s8, u8, s16, u16, s32, or u32, but {value} was given.");
        }
        private PARAMDEF.DefType type;

        /// <summary>
        /// Named values in this TDF.
        /// </summary>
        public List<Entry> Entries;

        /// <summary>
        /// Returns the value of the entry with the given name.
        /// </summary>
        public object this[string name] => this.Entries.Find(e => e.Name == name).Value;

        /// <summary>
        /// Returns the name of the entry with the given value.
        /// </summary>
        public string this[object value] => this.Entries.Find(e => e.Value == value).Name;

        /// <summary>
        /// Creates an empty TDF.
        /// </summary>
        public PARAMTDF() {
            this.Name = "UNSPECIFIED";
            this.Type = PARAMDEF.DefType.s32;
            this.Entries = new List<Entry>();
        }

        /// <summary>
        /// Reads a TDF from From's plaintext format.
        /// </summary>
        public PARAMTDF(string text) {
            string[] lines = text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            this.Name = lines[0].Trim('"');
            this.Type = Enum.Parse<PARAMDEF.DefType>(lines[1].Trim('"'));

            this.Entries = new List<Entry>(lines.Length - 2);
            for (int i = 2; i < lines.Length; i++) {
                string[] elements = lines[i].Split(',');
                string valueStr = elements[1].Trim('"');
                object value = this.Type switch {
                    PARAMDEF.DefType.s8 => sbyte.Parse(valueStr),
                    PARAMDEF.DefType.u8 => byte.Parse(valueStr),
                    PARAMDEF.DefType.s16 => short.Parse(valueStr),
                    PARAMDEF.DefType.u16 => ushort.Parse(valueStr),
                    PARAMDEF.DefType.s32 => int.Parse(valueStr),
                    PARAMDEF.DefType.u32 => uint.Parse(valueStr),
                    _ => throw new NotImplementedException($"Parsing not implemented for type {this.Type}."),
                };
                this.Entries.Add(new Entry(elements[0] == "" ? null : elements[0].Trim('"'), value));
            }
        }

        /// <summary>
        /// Writes the TDF in From's plaintext format.
        /// </summary>
        public string Write() {
            var sb = new StringBuilder();
            _ = sb.AppendLine($"\"{this.Name}\"");
            _ = sb.AppendLine($"\"{this.Type}\"");
            foreach (Entry entry in this.Entries) {
                if (entry.Name != null) {
                    _ = sb.Append($"\"{entry.Name}\"");
                }

                _ = sb.AppendLine($",\"{entry.Value}\"");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Returns a string representation of the TDF.
        /// </summary>
        public override string ToString() => $"{this.Type} {this.Name}";

        /// <summary>
        /// A named enumerator.
        /// Calling constructor creates a new Entry with the given values.
        /// </summary>
        /// <param name="Name">Name given to this value.</param>
        /// <param name="Value">Value of this entry, of the same type as the parent TDF.</param>
        public readonly record struct Entry(string Name, object Value) {
            /// <summary>
            /// Returns a string representation of the Entry.
            /// </summary>
            public override string ToString() => $"{this.Name ?? "<null>"} = {this.Value}";
        }
    }
}
