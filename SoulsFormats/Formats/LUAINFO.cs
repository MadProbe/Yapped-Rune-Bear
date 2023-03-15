using System;
using System.Collections.Generic;
using SoulsFormats.Util;

namespace SoulsFormats.Formats {
    /// <summary>
    /// A list of AI goals for Lua scripts.
    /// </summary>
    public class LUAINFO : SoulsFile<LUAINFO> {
        /// <summary>
        /// If true, write as big endian.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// If true, write with 64-bit offsets and UTF-16 strings.
        /// </summary>
        public bool LongFormat { get; set; }

        /// <summary>
        /// AI goals for a luabnd.
        /// </summary>
        public List<Goal> Goals { get; set; }

        /// <summary>
        /// Creates an empty LUAINFO formatted for PC DS1.
        /// </summary>
        public LUAINFO() : this(false, false) { }

        /// <summary>
        /// Creates an empty LUAINFO with the specified format.
        /// </summary>
        public LUAINFO(bool bigEndian, bool longFormat) {
            this.BigEndian = bigEndian;
            this.LongFormat = longFormat;
            this.Goals = new List<Goal>();
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected internal override bool Is(BinaryReaderEx br) {
            if (br.Length < 4) {
                return false;
            }

            string magic = br.GetASCII(0, 4);
            return magic == "LUAI";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            br.BigEndian = false;
            _ = br.AssertASCII("LUAI");
            this.BigEndian = br.AssertInt32(1, 0x1000000) == 0x1000000;
            br.BigEndian = this.BigEndian;
            int goalCount = br.ReadInt32();
            _ = br.AssertInt32(0);

            this.LongFormat = goalCount == 0
                ? throw new NotSupportedException("LUAINFO format cannot be detected on files with 0 goals.")
                : goalCount >= 2
                    ? br.GetInt32(0x24) == 0
                    : br.GetInt32(0x18) == 0x10 + 0x18 * goalCount
                || (br.GetInt32(0x14) == 0x10 + 0x10 * goalCount ? false : throw new NotSupportedException("Could not detect LUAINFO format."));
            this.Goals = new List<Goal>(goalCount);
            for (int i = 0; i < goalCount; i++) {
                this.Goals.Add(new Goal(br, this.LongFormat));
            }
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected internal override void Write(BinaryWriterEx bw) {
            bw.BigEndian = this.BigEndian;
            bw.WriteASCII("LUAI");
            bw.WriteInt32(1);
            bw.WriteInt32(this.Goals.Count);
            bw.WriteInt32(0);

            for (int i = 0; i < this.Goals.Count; i++) {
                this.Goals[i].Write(bw, this.LongFormat, i);
            }

            for (int i = 0; i < this.Goals.Count; i++) {
                this.Goals[i].WriteStrings(bw, this.LongFormat, i);
            }

            bw.Pad(0x10);
        }

        /// <summary>
        /// Goal information for AI scripts.
        /// </summary>
        public class Goal {
            /// <summary>
            /// ID of this goal.
            /// </summary>
            public int ID { get; set; }

            /// <summary>
            /// Name of this goal.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Whether to trigger a battle interrupt.
            /// </summary>
            public bool BattleInterrupt { get; set; }

            /// <summary>
            /// Whether to trigger a logic interrupt.
            /// </summary>
            public bool LogicInterrupt { get; set; }

            /// <summary>
            /// Function name of the logic interrupt, or null if not present.
            /// </summary>
            public string LogicInterruptName { get; set; }

            /// <summary>
            /// Creates a new Goal with the specified values.
            /// </summary>
            public Goal(int id, string name, bool battleInterrupt, bool logicInterrupt, string logicInterruptName = null) {
                this.ID = id;
                this.Name = name;
                this.BattleInterrupt = battleInterrupt;
                this.LogicInterrupt = logicInterrupt;
                this.LogicInterruptName = logicInterruptName;
            }

            internal Goal(BinaryReaderEx br, bool longFormat) {
                this.ID = br.ReadInt32();
                if (longFormat) {
                    this.BattleInterrupt = br.ReadBoolean();
                    this.LogicInterrupt = br.ReadBoolean();
                    _ = br.AssertInt16(0);
                    long nameOffset = br.ReadInt64();
                    long interruptNameOffset = br.ReadInt64();

                    this.Name = br.GetUTF16(nameOffset);
                    this.LogicInterruptName = interruptNameOffset == 0 ? null : br.GetUTF16(interruptNameOffset);
                } else {
                    uint nameOffset = br.ReadUInt32();
                    uint interruptNameOffset = br.ReadUInt32();
                    this.BattleInterrupt = br.ReadBoolean();
                    this.LogicInterrupt = br.ReadBoolean();
                    _ = br.AssertInt16(0);

                    this.Name = br.GetShiftJIS(nameOffset);
                    this.LogicInterruptName = interruptNameOffset == 0 ? null : br.GetShiftJIS(interruptNameOffset);
                }
            }

            internal void Write(BinaryWriterEx bw, bool longFormat, int index) {
                bw.WriteInt32(this.ID);
                if (longFormat) {
                    bw.WriteBoolean(this.BattleInterrupt);
                    bw.WriteBoolean(this.LogicInterrupt);
                    bw.WriteInt16(0);
                    bw.ReserveInt64($"NameOffset{index}");
                    bw.ReserveInt64($"LogicInterruptNameOffset{index}");
                } else {
                    bw.ReserveUInt32($"NameOffset{index}");
                    bw.ReserveUInt32($"LogicInterruptNameOffset{index}");
                    bw.WriteBoolean(this.BattleInterrupt);
                    bw.WriteBoolean(this.LogicInterrupt);
                    bw.WriteInt16(0);
                }
            }

            internal void WriteStrings(BinaryWriterEx bw, bool longFormat, int index) {
                if (longFormat) {
                    bw.FillInt64($"NameOffset{index}", bw.Position);
                    bw.WriteUTF16(this.Name, true);
                    if (this.LogicInterruptName == null) {
                        bw.FillInt64($"LogicInterruptNameOffset{index}", 0);
                    } else {
                        bw.FillInt64($"LogicInterruptNameOffset{index}", bw.Position);
                        bw.WriteUTF16(this.LogicInterruptName, true);
                    }
                } else {
                    bw.FillUInt32($"NameOffset{index}", (uint)bw.Position);
                    bw.WriteShiftJIS(this.Name, true);
                    if (this.LogicInterruptName == null) {
                        bw.FillUInt32($"LogicInterruptNameOffset{index}", 0);
                    } else {
                        bw.FillUInt32($"LogicInterruptNameOffset{index}", (uint)bw.Position);
                        bw.WriteShiftJIS(this.LogicInterruptName, true);
                    }
                }
            }
        }
    }
}
