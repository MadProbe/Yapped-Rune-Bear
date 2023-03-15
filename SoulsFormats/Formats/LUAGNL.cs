using System.Collections.Generic;
using SoulsFormats.Util;

namespace SoulsFormats.Formats {
    /// <summary>
    /// A list of global variable names for Lua scripts.
    /// </summary>
    public class LUAGNL : SoulsFile<LUAGNL> {
        /// <summary>
        /// If true, write as big endian.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// If true, write with 64-bit offsets and UTF-16 strings.
        /// </summary>
        public bool LongFormat { get; set; }

        /// <summary>
        /// Global variable names.
        /// </summary>
        public List<string> Globals { get; set; }

        /// <summary>
        /// Create an empty LUAGNL formatted for PC DS1.
        /// </summary>
        public LUAGNL() : this(false, false) { }

        /// <summary>
        /// Create an empty LUAGNL with the specified format.
        /// </summary>
        public LUAGNL(bool bigEndian, bool longFormat) {
            this.BigEndian = bigEndian;
            this.LongFormat = longFormat;
            this.Globals = new List<string>();
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            this.BigEndian = br.GetInt16(0) == 0;
            br.BigEndian = this.BigEndian;
            this.LongFormat = br.GetInt32(this.BigEndian ? 0 : 4) == 0;

            this.Globals = new List<string>();
            long offset;
            do {
                offset = this.LongFormat ? br.ReadInt64() : br.ReadUInt32();
                if (offset != 0) {
                    this.Globals.Add(this.LongFormat ? br.GetUTF16(offset) : br.GetShiftJIS(offset));
                }
            }
            while (offset != 0);
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected internal override void Write(BinaryWriterEx bw) {
            bw.BigEndian = this.BigEndian;
            for (int i = 0; i < this.Globals.Count; i++) {
                if (this.LongFormat) {
                    bw.ReserveInt64($"Offset{i}");
                } else {
                    bw.ReserveUInt32($"Offset{i}");
                }
            }

            if (this.LongFormat) {
                bw.WriteInt64(0);
            } else {
                bw.WriteUInt32(0);
            }

            for (int i = 0; i < this.Globals.Count; i++) {
                if (this.LongFormat) {
                    bw.FillInt64($"Offset{i}", bw.Position);
                    bw.WriteUTF16(this.Globals[i], true);
                } else {
                    bw.FillUInt32($"Offset{i}", (uint)bw.Position);
                    bw.WriteShiftJIS(this.Globals[i], true);
                }
            }
            bw.Pad(0x10);
        }
    }
}
