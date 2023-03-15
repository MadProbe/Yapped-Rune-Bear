using System;
using System.Collections.Generic;
using SoulsFormats.Util;

namespace SoulsFormats.Formats {
    /// <summary>
    /// "expression" files introduced in Elden Ring. Extension: .expb
    /// </summary>
    public class FMB : SoulsFile<FMB> {
        /// <summary>
        /// Unknown.
        /// </summary>
        public int Unk20 { get; set; }

        /// <summary>
        /// Entries in this FMB.
        /// </summary>
        public List<Entry> Entries { get; set; }

        /// <summary>
        /// Creates a default FMB.
        /// </summary>
        public FMB() => this.Entries = new List<Entry>();

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected internal override bool Is(BinaryReaderEx br) {
            if (br.Length < 4) {
                return false;
            }

            string magic = br.GetASCII(0, 4);
            return magic == "FMB ";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            br.BigEndian = false;

            _ = br.AssertASCII("FMB ");
            _ = br.AssertInt32(1);
            _ = br.AssertInt32(1);
            _ = br.AssertInt32(0);
            _ = br.AssertInt64(0x20);

            _ = br.AssertInt32(0);
            _ = br.AssertInt32(0);
            this.Unk20 = br.ReadInt32();
            _ = br.AssertInt32(0);
            _ = br.AssertInt64(0x30);

            _ = br.AssertInt32(0);
            _ = br.AssertInt32(0);
            _ = br.AssertInt64(0x40);

            int entryCount = br.ReadInt32();
            _ = br.AssertInt32(0);
            _ = br.AssertInt64(0x10);

            long[] entryOffsets = br.ReadInt64s(entryCount);

            this.Entries = new List<Entry>(entryCount);
            foreach (long offset in entryOffsets) {
                br.Position = 0x40 + offset;
                int type = br.GetInt32(br.Position);
                switch (type) {
                    case 2:
                    case 5:
                    case 6:
                    case 12:
                    case 14:
                    case 21:
                    case 31:
                    case 32:
                    case 33:
                    case 34:
                    case 43:
                        this.Entries.Add(new Entry(br)); break;

                    case 7:
                    case 11:
                        this.Entries.Add(new StringEntry(br)); break;

                    case 1:
                    case 3:
                    case 4:
                    case 8:
                    case 51:
                    case 61:
                        this.Entries.Add(new DoubleEntry(br)); break;

                    case 52:
                        this.Entries.Add(new Double2Entry(br)); break;

                    default:
                        throw new NotImplementedException($"Unknown entry type: {type}");
                }
            }
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected internal override void Write(BinaryWriterEx bw) {
            bw.BigEndian = false;

            bw.WriteASCII("FMB ");
            bw.WriteInt32(1);
            bw.WriteInt32(1);
            bw.WriteInt32(0);
            bw.WriteInt64(0x20);

            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(this.Unk20);
            bw.WriteInt32(0);
            bw.WriteInt64(0x30);

            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt64(0x40);

            bw.WriteInt32(this.Entries.Count);
            bw.WriteInt32(0);
            bw.WriteInt64(0x10);

            for (int i = 0; i < this.Entries.Count; i++) {
                bw.ReserveInt64($"{nameof(Entry)}Offset[{i}]");
            }

            bw.Pad(0x10);
            for (int i = 0; i < this.Entries.Count; i++) {
                bw.FillInt64($"{nameof(Entry)}Offset[{i}]", bw.Position - 0x40);
                this.Entries[i].Write(bw, i);
            }

            for (int i = 0; i < this.Entries.Count; i++) {
                this.Entries[i].WriteOffsetData(bw, i);
            }
            bw.Pad(0x10);
        }

        /// <summary>
        /// An entry with no additional data.
        /// </summary>
        public class Entry {
            /// <summary>
            /// Type of the entry.
            /// </summary>
            public int Type { get; set; }

            internal Entry(BinaryReaderEx br) {
                this.Type = br.ReadInt32();
                _ = br.AssertInt32(0);
                this.ReadData(br);
            }

            private protected virtual void ReadData(BinaryReaderEx br) {
                _ = br.AssertInt64(0);
                _ = br.AssertInt64(0);
                _ = br.AssertInt64(0);
            }

            internal void Write(BinaryWriterEx bw, int index) {
                bw.WriteInt32(this.Type);
                bw.WriteInt32(0);
                this.WriteData(bw, index);
            }

            private protected virtual void WriteData(BinaryWriterEx bw, int index) {
                bw.WriteInt64(0);
                bw.WriteInt64(0);
                bw.WriteInt64(0);
            }

            internal virtual void WriteOffsetData(BinaryWriterEx bw, int index) { }

            /// <summary>
            /// Returns a string representation of the entry.
            /// </summary>
            public override string ToString() => $"{this.Type}";
        }

        /// <summary>
        /// An entry with an additional string.
        /// </summary>
        public class StringEntry : Entry {
            /// <summary>
            /// The additional value of the entry.
            /// </summary>
            public string Value { get; set; }

            internal StringEntry(BinaryReaderEx br) : base(br) { }

            private protected override void ReadData(BinaryReaderEx br) {
                this.Value = br.GetASCII(0x40 + br.ReadInt64());
                _ = br.AssertInt64(0);
                _ = br.AssertInt64(0);
            }

            private protected override void WriteData(BinaryWriterEx bw, int index) {
                bw.ReserveInt64($"{nameof(this.Value)}Offset[{index}]");
                bw.WriteInt64(0);
                bw.WriteInt64(0);
            }

            internal override void WriteOffsetData(BinaryWriterEx bw, int index) {
                bw.FillInt64($"{nameof(this.Value)}Offset[{index}]", bw.Position - 0x40);
                bw.WriteASCII(this.Value, true);
            }

            /// <summary>
            /// Returns a string representation of the entry.
            /// </summary>
            public override string ToString() => base.ToString() + $": {this.Value}";
        }

        /// <summary>
        /// An entry with an additional double.
        /// </summary>
        public class DoubleEntry : Entry {
            /// <summary>
            /// The additional value of the entry.
            /// </summary>
            public double Value { get; set; }

            internal DoubleEntry(BinaryReaderEx br) : base(br) { }

            private protected override void ReadData(BinaryReaderEx br) {
                this.Value = br.ReadDouble();
                _ = br.AssertInt64(0);
                _ = br.AssertInt64(0);
            }

            private protected override void WriteData(BinaryWriterEx bw, int index) {
                bw.WriteDouble(this.Value);
                bw.WriteInt64(0);
                bw.WriteInt64(0);
            }

            /// <summary>
            /// Returns a string representation of the entry.
            /// </summary>
            public override string ToString() => base.ToString() + $": {this.Value}";
        }

        /// <summary>
        /// An entry with two additional doubles.
        /// </summary>
        public class Double2Entry : Entry {
            /// <summary>
            /// The first additional value of the entry.
            /// </summary>
            public double Value1 { get; set; }

            /// <summary>
            /// The second additional value of the entry.
            /// </summary>
            public double Value2 { get; set; }

            internal Double2Entry(BinaryReaderEx br) : base(br) { }

            private protected override void ReadData(BinaryReaderEx br) {
                this.Value1 = br.ReadDouble();
                this.Value2 = br.ReadDouble();
                _ = br.AssertInt64(0);
            }

            private protected override void WriteData(BinaryWriterEx bw, int index) {

                bw.WriteDouble(this.Value1);
                bw.WriteDouble(this.Value2);
                bw.WriteInt64(0);
            }

            /// <summary>
            /// Returns a string representation of the entry.
            /// </summary>
            public override string ToString() => base.ToString() + $": {this.Value1}, {this.Value2}";
        }
    }
}
