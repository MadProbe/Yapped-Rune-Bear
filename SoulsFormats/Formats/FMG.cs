using System.Collections.Generic;
using System.Linq;
using SoulsFormats.Util;

namespace SoulsFormats.Formats {
    /// <summary>
    /// A simple string container used throughout the series.
    /// </summary>
    public class FMG : SoulsFile<FMG> {
        /// <summary>
        /// The strings contained in this FMG.
        /// </summary>
        public List<Entry> Entries;

        /// <summary>
        /// Indicates file format; 0 - DeS, 1 - DS1/DS2, 2 - DS3/BB.
        /// </summary>
        public FMGVersion Version;

        /// <summary>
        /// FMG file endianness. (Big = true)
        /// </summary>
        public bool BigEndian;

        /// <summary>
        /// Creates an empty FMG configured for DS1/DS2.
        /// </summary>
        public FMG() {
            this.Entries = new List<Entry>();
            this.Version = FMGVersion.DarkSouls1;
            this.BigEndian = false;
        }

        /// <summary>
        /// Creates an empty FMG configured for the specified version.
        /// </summary>
        public FMG(FMGVersion version) {
            this.Entries = new List<Entry>();
            this.Version = version;
            this.BigEndian = this.Version == FMGVersion.DemonsSouls;
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            _ = br.AssertByte(0);
            this.BigEndian = br.ReadBoolean();
            this.Version = br.ReadEnum8<FMGVersion>();
            _ = br.AssertByte(0);

            br.BigEndian = this.BigEndian;
            bool wide = this.Version == FMGVersion.DarkSouls3;
            _ = br.ReadInt32();
            _ = br.AssertByte(1);
            _ = br.AssertByte((byte)(this.Version == FMGVersion.DemonsSouls ? 0xFF : 0x00));
            _ = br.AssertByte(0);
            _ = br.AssertByte(0);
            int groupCount = br.ReadInt32();
            _ = br.ReadInt32();

            if (wide) {
                _ = br.AssertInt32(0xFF);
            }

            long stringOffsetsOffset = wide ? br.ReadInt64() : br.ReadInt32();
            _ = wide ? br.AssertInt64(0) : br.AssertInt32(0);

            this.Entries = new List<Entry>(groupCount);
            for (int i = 0; i < groupCount; i++) {
                int offsetIndex = br.ReadInt32();
                int firstID = br.ReadInt32();
                int lastID = br.ReadInt32();

                if (wide) {
                    _ = br.AssertInt32(0);
                }

                br.StepIn(stringOffsetsOffset + offsetIndex * (wide ? 8 : 4));
                {
                    for (int j = 0; j < lastID - firstID + 1; j++) {
                        long stringOffset = wide ? br.ReadInt64() : br.ReadInt32();
                        int id = firstID + j;
                        string text = stringOffset != 0 ? br.GetUTF16(stringOffset) : null;
                        this.Entries.Add(new Entry(id, text));
                    }
                }
                br.StepOut();
            }
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected internal override void Write(BinaryWriterEx bw) {
            bw.BigEndian = this.BigEndian;
            bool wide = this.Version == FMGVersion.DarkSouls3;

            bw.WriteByte(0);
            bw.WriteBoolean(bw.BigEndian);
            bw.WriteByte((byte)this.Version);
            bw.WriteByte(0);

            bw.ReserveInt32("FileSize");
            bw.WriteByte(1);
            bw.WriteByte((byte)(this.Version == FMGVersion.DemonsSouls ? 0xFF : 0x00));
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.ReserveInt32("GroupCount");
            bw.WriteInt32(this.Entries.Count);

            if (wide) {
                bw.WriteInt32(0xFF);
            }

            if (wide) {
                bw.ReserveInt64("StringOffsets");
            } else {
                bw.ReserveInt32("StringOffsets");
            }

            if (wide) {
                bw.WriteInt64(0);
            } else {
                bw.WriteInt32(0);
            }

            int groupCount = 0;
            this.Entries.Sort((e1, e2) => e1.ID.CompareTo(e2.ID));
            for (int i = 0; i < this.Entries.Count; i++) {
                bw.WriteInt32(i);
                bw.WriteInt32(this.Entries[i].ID);
                while (i < this.Entries.Count - 1 && this.Entries[i + 1].ID == this.Entries[i].ID + 1) {
                    i++;
                }

                bw.WriteInt32(this.Entries[i].ID);

                if (wide) {
                    bw.WriteInt32(0);
                }

                groupCount++;
            }
            bw.FillInt32("GroupCount", groupCount);

            if (wide) {
                bw.FillInt64("StringOffsets", bw.Position);
            } else {
                bw.FillInt32("StringOffsets", (int)bw.Position);
            }

            for (int i = 0; i < this.Entries.Count; i++) {
                if (wide) {
                    bw.ReserveInt64($"StringOffset{i}");
                } else {
                    bw.ReserveInt32($"StringOffset{i}");
                }
            }

            for (int i = 0; i < this.Entries.Count; i++) {
                string text = this.Entries[i].Text;

                if (wide) {
                    bw.FillInt64($"StringOffset{i}", text == null ? 0 : bw.Position);
                } else {
                    bw.FillInt32($"StringOffset{i}", text == null ? 0 : (int)bw.Position);
                }

                if (text != null) {
                    bw.WriteUTF16(this.Entries[i].Text, true);
                }
            }

            bw.FillInt32("FileSize", (int)bw.Position);
        }

        /// <summary>
        /// Returns the string with the given ID, or null if not present.
        /// </summary>
        public string this[int id] {
            get => this.Entries.Find(entry => entry.ID == id)?.Text;

            set {
                if (this.Entries.Any(entry => entry.ID == id)) {
                    this.Entries.Find(entry => entry.ID == id).Text = value;
                } else {
                    this.Entries.Add(new Entry(id, value));
                }
            }
        }

        /// <summary>
        /// A string in an FMG identified with an ID number.
        /// </summary>
        public class Entry {
            /// <summary>
            /// The ID of this entry.
            /// </summary>
            public int ID;

            /// <summary>
            /// The text of this entry.
            /// </summary>
            public string Text;

            /// <summary>
            /// Creates a new entry with the specified ID and text.
            /// </summary>
            public Entry(int id, string text) {
                this.ID = id;
                this.Text = text;
            }

            /// <summary>
            /// Returns the ID and text of this entry.
            /// </summary>
            public override string ToString() => $"{this.ID}: {this.Text ?? "<null>"}";
        }

        /// <summary>
        /// Indicates the game this FMG is for, and thus the format it will be written in.
        /// </summary>
        public enum FMGVersion : byte {
            /// <summary>
            /// Demon's Souls
            /// </summary>
            DemonsSouls = 0,

            /// <summary>
            /// Dark Souls 1 and Dark Souls 2
            /// </summary>
            DarkSouls1 = 1,

            /// <summary>
            /// Bloodborne and Dark Souls 3
            /// </summary>
            DarkSouls3 = 2,
        }
    }
}
