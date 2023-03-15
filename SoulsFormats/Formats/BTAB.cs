using System.Collections.Generic;
using System.Numerics;
using SoulsFormats.Util;

namespace SoulsFormats.Formats {
    /// <summary>
    /// A lightmap atlasing config file introduced in DS2. Extension: .btab
    /// </summary>
    public class BTAB : SoulsFile<BTAB> {
        /// <summary>
        /// Whether the file is big-endian; true for PS3/X360, false otherwise.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// Whether the file uses the 64-bit format; true for DS3/BB, false for DS2.
        /// </summary>
        public bool LongFormat { get; set; }

        /// <summary>
        /// Material configs in this file.
        /// </summary>
        public List<Entry> Entries { get; set; }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            br.BigEndian = br.GetBoolean(0x10);

            _ = br.AssertInt32(1);
            _ = br.AssertInt32(0);
            int entryCount = br.ReadInt32();
            int stringsLength = br.ReadInt32();
            this.BigEndian = br.ReadBoolean();
            _ = br.AssertByte(0);
            _ = br.AssertByte(0);
            _ = br.AssertByte(0);
            br.VarintLong = this.LongFormat = br.AssertInt32(0x1C, 0x28) == 0x28; // Entry size
            br.AssertPattern(0x24, 0x00);

            long stringsStart = br.Position;
            br.Skip(stringsLength);
            this.Entries = new List<Entry>(entryCount);
            for (int i = 0; i < entryCount; i++) {
                this.Entries.Add(new Entry(br, stringsStart));
            }
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected internal override void Write(BinaryWriterEx bw) {
            bw.BigEndian = this.BigEndian;
            bw.VarintLong = this.LongFormat;

            bw.WriteInt32(1);
            bw.WriteInt32(0);
            bw.WriteInt32(this.Entries.Count);
            bw.ReserveInt32("StringsLength");
            bw.WriteBoolean(this.BigEndian);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteInt32(this.LongFormat ? 0x28 : 0x1C);
            bw.WritePattern(0x24, 0x00);

            long stringsStart = bw.Position;
            var stringOffsets = new List<long>(this.Entries.Count * 2);
            foreach (Entry entry in this.Entries) {
                long partNameOffset = bw.Position - stringsStart;
                stringOffsets.Add(partNameOffset);
                bw.WriteUTF16(entry.PartName, true);
                bw.PadRelative(stringsStart, 8); // This padding is not consistent, but it's the best I can do

                long materialNameOffset = bw.Position - stringsStart;
                stringOffsets.Add(materialNameOffset);
                bw.WriteUTF16(entry.MaterialName, true);
                bw.PadRelative(stringsStart, 8);
            }

            bw.FillInt32("StringsLength", (int)(bw.Position - stringsStart));
            for (int i = 0; i < this.Entries.Count; i++) {
                this.Entries[i].Write(bw, stringOffsets[i * 2], stringOffsets[i * 2 + 1]);
            }
        }

        /// <summary>
        /// Configures lightmap atlasing for a certain part and material.
        /// </summary>
        public class Entry {
            /// <summary>
            /// The name of the target part defined in an MSB file.
            /// </summary>
            public string PartName { get; set; }

            /// <summary>
            /// The name of the target material in the part's FLVER model.
            /// </summary>
            public string MaterialName { get; set; }

            /// <summary>
            /// The ID of the atlas texture to use.
            /// </summary>
            public int AtlasID { get; set; }

            /// <summary>
            /// Offsets the lightmap UVs.
            /// </summary>
            public Vector2 UVOffset { get; set; }

            /// <summary>
            /// Scales the lightmap UVs.
            /// </summary>
            public Vector2 UVScale { get; set; }

            /// <summary>
            /// Creates an Entry with default values.
            /// </summary>
            public Entry() {
                this.PartName = "";
                this.MaterialName = "";
                this.UVScale = Vector2.One;
            }

            internal Entry(BinaryReaderEx br, long nameStart) {
                long msbNameOffset = br.ReadVarint();
                long flverNameOffset = br.ReadVarint();
                this.AtlasID = br.ReadInt32();
                this.UVOffset = br.ReadVector2();
                this.UVScale = br.ReadVector2();
                if (br.VarintLong) {
                    _ = br.AssertInt32(0);
                }

                this.PartName = br.GetUTF16(nameStart + msbNameOffset);
                this.MaterialName = br.GetUTF16(nameStart + flverNameOffset);
            }

            internal void Write(BinaryWriterEx bw, long partNameOffset, long materialNameOffset) {
                bw.WriteVarint(partNameOffset);
                bw.WriteVarint(materialNameOffset);
                bw.WriteInt32(this.AtlasID);
                bw.WriteVector2(this.UVOffset);
                bw.WriteVector2(this.UVScale);
                if (bw.VarintLong) {
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Returns the MSB part name and FLVER material name of the entry.
            /// </summary>
            public override string ToString() => $"{this.PartName} : {this.MaterialName}";
        }
    }
}
