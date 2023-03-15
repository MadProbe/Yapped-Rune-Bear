using System;
using System.Collections.Generic;
using System.Numerics;
using SoulsFormats.Util;

namespace SoulsFormats.Formats {
    /// <summary>
    /// A graphics config file used since DS2. Extensions: .fltparam, .gparam
    /// </summary>
    public class GPARAM : SoulsFile<GPARAM> {
        /// <summary>
        /// Indicates the format of the GPARAM.
        /// </summary>
        public GPGame Game;

        /// <summary>
        /// Unknown.
        /// </summary>
        public bool Unk0D;

        /// <summary>
        /// Unknown; in DS2, number of entries in UnkBlock2.
        /// </summary>
        public int Unk14;

        /// <summary>
        /// Unknown; only present in Sekiro.
        /// </summary>
        public float Unk50;

        /// <summary>
        /// Groups of params in this file.
        /// </summary>
        public List<Group> Groups;

        /// <summary>
        /// Unknown.
        /// </summary>
        public byte[] UnkBlock2;

        /// <summary>
        /// Unknown.
        /// </summary>
        public List<Unk3> Unk3s;

        /// <summary>
        /// Creates a new empty GPARAM formatted for Sekiro.
        /// </summary>
        public GPARAM() {
            this.Game = GPGame.Sekiro;
            this.Groups = new List<Group>();
            this.UnkBlock2 = new byte[0];
            this.Unk3s = new List<Unk3>();
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected internal override bool Is(BinaryReaderEx br) {
            if (br.Length < 4) {
                return false;
            }

            string magic = br.GetASCII(0, 4);
            return magic is "filt" or "f\0i\0";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            br.BigEndian = false;

            // Don't @ me.
            if (br.AssertASCII("filt", "f\0i\0") == "f\0i\0") {
                _ = br.AssertASCII("l\0t\0");
            }

            this.Game = br.ReadEnum32<GPGame>();
            _ = br.AssertByte(0);
            this.Unk0D = br.ReadBoolean();
            _ = br.AssertInt16(0);
            int groupCount = br.ReadInt32();
            this.Unk14 = br.ReadInt32();
            // Header size or group header headers offset, you decide
            _ = br.AssertInt32(0x40, 0x50, 0x54);

            Offsets offsets = default;
            offsets.GroupHeaders = br.ReadInt32();
            offsets.ParamHeaderOffsets = br.ReadInt32();
            offsets.ParamHeaders = br.ReadInt32();
            offsets.Values = br.ReadInt32();
            offsets.ValueIDs = br.ReadInt32();
            offsets.Unk2 = br.ReadInt32();

            int unk3Count = br.ReadInt32();
            offsets.Unk3 = br.ReadInt32();
            offsets.Unk3ValueIDs = br.ReadInt32();
            _ = br.AssertInt32(0);

            if (this.Game is GPGame.DarkSouls3 or GPGame.Sekiro) {
                offsets.CommentOffsetsOffsets = br.ReadInt32();
                offsets.CommentOffsets = br.ReadInt32();
                offsets.Comments = br.ReadInt32();
            }

            if (this.Game == GPGame.Sekiro) {
                this.Unk50 = br.ReadSingle();
            }

            this.Groups = new List<Group>(groupCount);
            for (int i = 0; i < groupCount; i++) {
                this.Groups.Add(new Group(br, this.Game, i, offsets));
            }

            this.UnkBlock2 = br.GetBytes(offsets.Unk2, offsets.Unk3 - offsets.Unk2);

            br.Position = offsets.Unk3;
            this.Unk3s = new List<Unk3>(unk3Count);
            for (int i = 0; i < unk3Count; i++) {
                this.Unk3s.Add(new Unk3(br, this.Game, offsets));
            }

            if (this.Game is GPGame.DarkSouls3 or GPGame.Sekiro) {
                int[] commentOffsetsOffsets = br.GetInt32s(offsets.CommentOffsetsOffsets, groupCount);
                int commentOffsetsLength = offsets.Comments - offsets.CommentOffsets;
                for (int i = 0; i < groupCount; i++) {
                    int commentCount = i == groupCount - 1
                        ? (commentOffsetsLength - commentOffsetsOffsets[i]) / 4
                        : (commentOffsetsOffsets[i + 1] - commentOffsetsOffsets[i]) / 4;
                    br.Position = offsets.CommentOffsets + commentOffsetsOffsets[i];
                    for (int j = 0; j < commentCount; j++) {
                        int commentOffset = br.ReadInt32();
                        string comment = br.GetUTF16(offsets.Comments + commentOffset);
                        this.Groups[i].Comments.Add(comment);
                    }
                }
            }
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected internal override void Write(BinaryWriterEx bw) {
            bw.BigEndian = false;

            if (this.Game == GPGame.DarkSouls2) {
                bw.WriteASCII("filt");
            } else {
                bw.WriteUTF16("filt");
            }

            bw.WriteUInt32((uint)this.Game);
            bw.WriteByte(0);
            bw.WriteBoolean(this.Unk0D);
            bw.WriteInt16(0);
            bw.WriteInt32(this.Groups.Count);
            bw.WriteInt32(this.Unk14);
            bw.ReserveInt32("HeaderSize");

            bw.ReserveInt32("GroupHeadersOffset");
            bw.ReserveInt32("ParamHeaderOffsetsOffset");
            bw.ReserveInt32("ParamHeadersOffset");
            bw.ReserveInt32("ValuesOffset");
            bw.ReserveInt32("ValueIDsOffset");
            bw.ReserveInt32("UnkOffset2");

            bw.WriteInt32(this.Unk3s.Count);
            bw.ReserveInt32("UnkOffset3");
            bw.ReserveInt32("Unk3ValuesOffset");
            bw.WriteInt32(0);

            if (this.Game is GPGame.DarkSouls3 or GPGame.Sekiro) {
                bw.ReserveInt32("CommentOffsetsOffsetsOffset");
                bw.ReserveInt32("CommentOffsetsOffset");
                bw.ReserveInt32("CommentsOffset");
            }

            if (this.Game == GPGame.Sekiro) {
                bw.WriteSingle(this.Unk50);
            }

            bw.FillInt32("HeaderSize", (int)bw.Position);

            for (int i = 0; i < this.Groups.Count; i++) {
                this.Groups[i].WriteHeaderOffset(bw, i);
            }

            int groupHeadersOffset = (int)bw.Position;
            bw.FillInt32("GroupHeadersOffset", groupHeadersOffset);
            for (int i = 0; i < this.Groups.Count; i++) {
                this.Groups[i].WriteHeader(bw, this.Game, i, groupHeadersOffset);
            }

            int paramHeaderOffsetsOffset = (int)bw.Position;
            bw.FillInt32("ParamHeaderOffsetsOffset", paramHeaderOffsetsOffset);
            for (int i = 0; i < this.Groups.Count; i++) {
                this.Groups[i].WriteParamHeaderOffsets(bw, i, paramHeaderOffsetsOffset);
            }

            int paramHeadersOffset = (int)bw.Position;
            bw.FillInt32("ParamHeadersOffset", paramHeadersOffset);
            for (int i = 0; i < this.Groups.Count; i++) {
                this.Groups[i].WriteParamHeaders(bw, this.Game, i, paramHeadersOffset);
            }

            int valuesOffset = (int)bw.Position;
            bw.FillInt32("ValuesOffset", valuesOffset);
            for (int i = 0; i < this.Groups.Count; i++) {
                this.Groups[i].WriteValues(bw, i, valuesOffset);
            }

            int valueIDsOffset = (int)bw.Position;
            bw.FillInt32("ValueIDsOffset", (int)bw.Position);
            for (int i = 0; i < this.Groups.Count; i++) {
                this.Groups[i].WriteValueIDs(bw, this.Game, i, valueIDsOffset);
            }

            bw.FillInt32("UnkOffset2", (int)bw.Position);
            bw.WriteBytes(this.UnkBlock2);

            bw.FillInt32("UnkOffset3", (int)bw.Position);
            for (int i = 0; i < this.Unk3s.Count; i++) {
                this.Unk3s[i].WriteHeader(bw, this.Game, i);
            }

            int unk3ValuesOffset = (int)bw.Position;
            bw.FillInt32("Unk3ValuesOffset", unk3ValuesOffset);
            for (int i = 0; i < this.Unk3s.Count; i++) {
                this.Unk3s[i].WriteValues(bw, this.Game, i, unk3ValuesOffset);
            }

            if (this.Game is GPGame.DarkSouls3 or GPGame.Sekiro) {
                bw.FillInt32("CommentOffsetsOffsetsOffset", (int)bw.Position);
                for (int i = 0; i < this.Groups.Count; i++) {
                    this.Groups[i].WriteCommentOffsetsOffset(bw, i);
                }

                int commentOffsetsOffset = (int)bw.Position;
                bw.FillInt32("CommentOffsetsOffset", commentOffsetsOffset);
                for (int i = 0; i < this.Groups.Count; i++) {
                    this.Groups[i].WriteCommentOffsets(bw, i, commentOffsetsOffset);
                }

                int commentsOffset = (int)bw.Position;
                bw.FillInt32("CommentsOffset", commentsOffset);
                for (int i = 0; i < this.Groups.Count; i++) {
                    this.Groups[i].WriteComments(bw, i, commentsOffset);
                }
            }
        }

        /// <summary>
        /// Returns the first group with a matching name, or null if not found.
        /// </summary>
        public Group this[string name1] => this.Groups.Find(group => group.Name1 == name1);

        /// <summary>
        /// The game this GPARAM is from.
        /// </summary>
        public enum GPGame : uint {
            /// <summary>
            /// Dark Souls 2
            /// </summary>
            DarkSouls2 = 2,

            /// <summary>
            /// Dark Souls 3 and Bloodborne
            /// </summary>
            DarkSouls3 = 3,

            /// <summary>
            /// Sekiro
            /// </summary>
            Sekiro = 5,
        }

        internal struct Offsets {
            public int GroupHeaders;
            public int ParamHeaderOffsets;
            public int ParamHeaders;
            public int Values;
            public int ValueIDs;
            public int Unk2;
            public int Unk3;
            public int Unk3ValueIDs;
            public int CommentOffsetsOffsets;
            public int CommentOffsets;
            public int Comments;
        }

        /// <summary>
        /// A group of graphics params.
        /// </summary>
        public class Group {
            /// <summary>
            /// Identifies the group.
            /// </summary>
            public string Name1;

            /// <summary>
            /// Identifies the group, but shorter? Not present in DS2.
            /// </summary>
            public string Name2;

            /// <summary>
            /// Params in this group.
            /// </summary>
            public List<Param> Params;

            /// <summary>
            /// Comments indicating the purpose of each entry in param values. Not present in DS2.
            /// </summary>
            public List<string> Comments;

            /// <summary>
            /// Creates a new Group with no params or comments.
            /// </summary>
            public Group(string name1, string name2) {
                this.Name1 = name1;
                this.Name2 = name2;
                this.Params = new List<Param>();
                this.Comments = new List<string>();
            }

            internal Group(BinaryReaderEx br, GPGame game, int index, Offsets offsets) {
                int groupHeaderOffset = br.ReadInt32();
                br.StepIn(offsets.GroupHeaders + groupHeaderOffset);
                {
                    int paramCount = br.ReadInt32();
                    int paramHeaderOffsetsOffset = br.ReadInt32();
                    if (game == GPGame.DarkSouls2) {
                        this.Name1 = br.ReadShiftJIS();
                    } else {
                        this.Name1 = br.ReadUTF16();
                        this.Name2 = br.ReadUTF16();
                    }

                    br.StepIn(offsets.ParamHeaderOffsets + paramHeaderOffsetsOffset);
                    {
                        this.Params = new List<Param>(paramCount);
                        for (int i = 0; i < paramCount; i++) {
                            this.Params.Add(new Param(br, game, offsets));
                        }
                    }
                    br.StepOut();
                }
                br.StepOut();
                this.Comments = new List<string>();
            }

            internal void WriteHeaderOffset(BinaryWriterEx bw, int groupIndex) => bw.ReserveInt32($"GroupHeaderOffset{groupIndex}");

            internal void WriteHeader(BinaryWriterEx bw, GPGame game, int groupIndex, int groupHeadersOffset) {
                bw.FillInt32($"GroupHeaderOffset{groupIndex}", (int)bw.Position - groupHeadersOffset);
                bw.WriteInt32(this.Params.Count);
                bw.ReserveInt32($"ParamHeaderOffsetsOffset{groupIndex}");

                if (game == GPGame.DarkSouls2) {
                    bw.WriteShiftJIS(this.Name1, true);
                } else {
                    bw.WriteUTF16(this.Name1, true);
                    bw.WriteUTF16(this.Name2, true);
                }
                bw.Pad(4);
            }

            internal void WriteParamHeaderOffsets(BinaryWriterEx bw, int groupIndex, int paramHeaderOffsetsOffset) {
                bw.FillInt32($"ParamHeaderOffsetsOffset{groupIndex}", (int)bw.Position - paramHeaderOffsetsOffset);
                for (int i = 0; i < this.Params.Count; i++) {
                    this.Params[i].WriteParamHeaderOffset(bw, groupIndex, i);
                }
            }

            internal void WriteParamHeaders(BinaryWriterEx bw, GPGame game, int groupindex, int paramHeadersOffset) {
                for (int i = 0; i < this.Params.Count; i++) {
                    this.Params[i].WriteParamHeader(bw, game, groupindex, i, paramHeadersOffset);
                }
            }

            internal void WriteValues(BinaryWriterEx bw, int groupindex, int valuesOffset) {
                for (int i = 0; i < this.Params.Count; i++) {
                    this.Params[i].WriteValues(bw, groupindex, i, valuesOffset);
                }
            }

            internal void WriteValueIDs(BinaryWriterEx bw, GPGame game, int groupIndex, int valueIDsOffset) {
                for (int i = 0; i < this.Params.Count; i++) {
                    this.Params[i].WriteValueIDs(bw, game, groupIndex, i, valueIDsOffset);
                }
            }

            internal void WriteCommentOffsetsOffset(BinaryWriterEx bw, int index) => bw.ReserveInt32($"CommentOffsetsOffset{index}");

            internal void WriteCommentOffsets(BinaryWriterEx bw, int index, int commentOffsetsOffset) {
                bw.FillInt32($"CommentOffsetsOffset{index}", (int)bw.Position - commentOffsetsOffset);
                for (int i = 0; i < this.Comments.Count; i++) {
                    bw.ReserveInt32($"CommentOffset{index}:{i}");
                }
            }

            internal void WriteComments(BinaryWriterEx bw, int index, int commentsOffset) {
                for (int i = 0; i < this.Comments.Count; i++) {
                    bw.FillInt32($"CommentOffset{index}:{i}", (int)bw.Position - commentsOffset);
                    bw.WriteUTF16(this.Comments[i], true);
                    bw.Pad(4);
                }
            }

            /// <summary>
            /// Returns the first param with a matching name, or null if not found.
            /// </summary>
            public Param this[string name1] => this.Params.Find(param => param.Name1 == name1);

            /// <summary>
            /// Returns the long and short names of the group.
            /// </summary>
            public override string ToString() => this.Name2 == null ? this.Name1 : $"{this.Name1} | {this.Name2}";
        }

        /// <summary>
        /// Value types allowed in a param.
        /// </summary>
        public enum ParamType : byte {
            /// <summary>
            /// Unknown; only ever appears as a single value.
            /// </summary>
            Byte = 0x1,

            /// <summary>
            /// One short.
            /// </summary>
            Short = 0x2,

            /// <summary>
            /// One int.
            /// </summary>
            IntA = 0x3,

            /// <summary>
            /// One bool.
            /// </summary>
            BoolA = 0x5,

            /// <summary>
            /// One int.
            /// </summary>
            IntB = 0x7,

            /// <summary>
            /// One float.
            /// </summary>
            Float = 0x9,

            /// <summary>
            /// One bool.
            /// </summary>
            BoolB = 0xB,

            /// <summary>
            /// Two floats and 8 unused bytes.
            /// </summary>
            Float2 = 0xC,

            /// <summary>
            /// Three floats and 4 unused bytes.
            /// </summary>
            Float3 = 0xD,

            /// <summary>
            /// Four floats.
            /// </summary>
            Float4 = 0xE,

            /// <summary>
            /// Four bytes, used for BGRA.
            /// </summary>
            Byte4 = 0xF,
        }

        /// <summary>
        /// A collection of values controlling the same parameter in different circumstances.
        /// </summary>
        public class Param {
            /// <summary>
            /// Identifies the param specifically.
            /// </summary>
            public string Name1;

            /// <summary>
            /// Identifies the param generically. Not present in DS2.
            /// </summary>
            public string Name2;

            /// <summary>
            /// Type of values in this param.
            /// </summary>
            public ParamType Type;

            /// <summary>
            /// Values in this param.
            /// </summary>
            public List<object> Values;

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<int> ValueIDs;

            /// <summary>
            /// Unknown; one for each value ID, only present in Sekiro.
            /// </summary>
            public List<float> UnkFloats;

            /// <summary>
            /// Creates a new Param with no values or unk1s.
            /// </summary>
            public Param(string name1, string name2, ParamType type) {
                this.Name1 = name1;
                this.Name2 = name2;
                this.Type = type;
                this.Values = new List<object>();
                this.ValueIDs = new List<int>();
                this.UnkFloats = null;
            }

            internal Param(BinaryReaderEx br, GPGame game, Offsets offsets) {
                int paramHeaderOffset = br.ReadInt32();
                br.StepIn(offsets.ParamHeaders + paramHeaderOffset);
                {
                    int valuesOffset = br.ReadInt32();
                    int valueIDsOffset = br.ReadInt32();

                    this.Type = br.ReadEnum8<ParamType>();
                    byte valueCount = br.ReadByte();
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);

                    if (this.Type == ParamType.Byte && valueCount > 1) {
                        throw new Exception("Notify TKGP so he can look into this, please.");
                    }

                    if (game == GPGame.DarkSouls2) {
                        this.Name1 = br.ReadShiftJIS();
                    } else {
                        this.Name1 = br.ReadUTF16();
                        this.Name2 = br.ReadUTF16();
                    }

                    br.StepIn(offsets.Values + valuesOffset);
                    {
                        this.Values = new List<object>(valueCount);
                        for (int i = 0; i < valueCount; i++) {
                            switch (this.Type) {
                                case ParamType.Byte:
                                    this.Values.Add(br.ReadByte());
                                    break;

                                case ParamType.Short:
                                    this.Values.Add(br.ReadInt16());
                                    break;

                                case ParamType.IntA:
                                    this.Values.Add(br.ReadInt32());
                                    break;

                                case ParamType.BoolA:
                                    this.Values.Add(br.ReadBoolean());
                                    break;

                                case ParamType.IntB:
                                    this.Values.Add(br.ReadInt32());
                                    break;

                                case ParamType.Float:
                                    this.Values.Add(br.ReadSingle());
                                    break;

                                case ParamType.BoolB:
                                    this.Values.Add(br.ReadBoolean());
                                    break;

                                case ParamType.Float2:
                                    this.Values.Add(br.ReadVector2());
                                    _ = br.AssertInt32(0);
                                    _ = br.AssertInt32(0);
                                    break;

                                case ParamType.Float3:
                                    this.Values.Add(br.ReadVector3());
                                    _ = br.AssertInt32(0);
                                    break;

                                case ParamType.Float4:
                                    this.Values.Add(br.ReadVector4());
                                    break;

                                case ParamType.Byte4:
                                    this.Values.Add(br.ReadBytes(4));
                                    break;
                            }
                        }
                    }
                    br.StepOut();

                    br.StepIn(offsets.ValueIDs + valueIDsOffset);
                    {
                        this.ValueIDs = new List<int>(valueCount);
                        this.UnkFloats = game == GPGame.Sekiro ? new List<float>(valueCount) : null;

                        for (int i = 0; i < valueCount; i++) {
                            this.ValueIDs.Add(br.ReadInt32());
                            if (game == GPGame.Sekiro) {
                                this.UnkFloats.Add(br.ReadSingle());
                            }
                        }
                    }
                    br.StepOut();
                }
                br.StepOut();
            }

            internal void WriteParamHeaderOffset(BinaryWriterEx bw, int groupIndex, int paramIndex) => bw.ReserveInt32($"ParamHeaderOffset{groupIndex}:{paramIndex}");

            internal void WriteParamHeader(BinaryWriterEx bw, GPGame game, int groupIndex, int paramIndex, int paramHeadersOffset) {
                bw.FillInt32($"ParamHeaderOffset{groupIndex}:{paramIndex}", (int)bw.Position - paramHeadersOffset);
                bw.ReserveInt32($"ValuesOffset{groupIndex}:{paramIndex}");
                bw.ReserveInt32($"ValueIDsOffset{groupIndex}:{paramIndex}");

                bw.WriteByte((byte)this.Type);
                bw.WriteByte((byte)this.Values.Count);
                bw.WriteByte(0);
                bw.WriteByte(0);

                if (game == GPGame.DarkSouls2) {
                    bw.WriteShiftJIS(this.Name1, true);
                } else {
                    bw.WriteUTF16(this.Name1, true);
                    bw.WriteUTF16(this.Name2, true);
                }
                bw.Pad(4);
            }

            internal void WriteValues(BinaryWriterEx bw, int groupIndex, int paramIndex, int valuesOffset) {
                bw.FillInt32($"ValuesOffset{groupIndex}:{paramIndex}", (int)bw.Position - valuesOffset);
                for (int i = 0; i < this.Values.Count; i++) {
                    object value = this.Values[i];
                    switch (this.Type) {
                        case ParamType.Byte:
                            bw.WriteInt32((byte)value);
                            break;

                        case ParamType.Short:
                            bw.WriteInt16((short)value);
                            break;

                        case ParamType.IntA:
                            bw.WriteInt32((int)value);
                            break;

                        case ParamType.BoolA:
                            bw.WriteBoolean((bool)value);
                            break;

                        case ParamType.IntB:
                            bw.WriteInt32((int)value);
                            break;

                        case ParamType.Float:
                            bw.WriteSingle((float)value);
                            break;

                        case ParamType.BoolB:
                            bw.WriteBoolean((bool)value);
                            break;

                        case ParamType.Float2:
                            bw.WriteVector2((Vector2)value);
                            bw.WriteInt32(0);
                            bw.WriteInt32(0);
                            break;

                        case ParamType.Float3:
                            bw.WriteVector3((Vector3)value);
                            bw.WriteInt32(0);
                            break;

                        case ParamType.Float4:
                            bw.WriteVector4((Vector4)value);
                            break;

                        case ParamType.Byte4:
                            bw.WriteBytes((byte[])value);
                            break;
                    }
                }
                bw.Pad(4);
            }

            internal void WriteValueIDs(BinaryWriterEx bw, GPGame game, int groupIndex, int paramIndex, int valueIDsOffset) {
                bw.FillInt32($"ValueIDsOffset{groupIndex}:{paramIndex}", (int)bw.Position - valueIDsOffset);
                for (int i = 0; i < this.ValueIDs.Count; i++) {
                    bw.WriteInt32(this.ValueIDs[i]);
                    if (game == GPGame.Sekiro) {
                        bw.WriteSingle(this.UnkFloats[i]);
                    }
                }
            }

            /// <summary>
            /// Returns the value in this param at the given index.
            /// </summary>
            public object this[int index] {
                get => this.Values[index];
                set => this.Values[index] = value;
            }

            /// <summary>
            /// Returns the specific and generic names of the param.
            /// </summary>
            public override string ToString() => this.Name2 == null ? this.Name1 : $"{this.Name1} | {this.Name2}";
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        public class Unk3 {
            /// <summary>
            /// Index of a group.
            /// </summary>
            public int GroupIndex;

            /// <summary>
            /// Unknown; matches value IDs in the group.
            /// </summary>
            public List<int> ValueIDs;

            /// <summary>
            /// Unknown; only present in Sekiro.
            /// </summary>
            public int Unk0C;

            /// <summary>
            /// Creates a new Unk3 with no value IDs.
            /// </summary>
            public Unk3(int groupIndex) {
                this.GroupIndex = groupIndex;
                this.ValueIDs = new List<int>();
            }

            internal Unk3(BinaryReaderEx br, GPGame game, Offsets offsets) {
                this.GroupIndex = br.ReadInt32();
                int count = br.ReadInt32();
                uint valueIDsOffset = br.ReadUInt32();
                if (game == GPGame.Sekiro) {
                    this.Unk0C = br.ReadInt32();
                }

                this.ValueIDs = new List<int>(br.GetInt32s(offsets.Unk3ValueIDs + valueIDsOffset, count));
            }

            internal void WriteHeader(BinaryWriterEx bw, GPGame game, int index) {
                bw.WriteInt32(this.GroupIndex);
                bw.WriteInt32(this.ValueIDs.Count);
                bw.ReserveInt32($"Unk3ValueIDsOffset{index}");
                if (game == GPGame.Sekiro) {
                    bw.WriteInt32(this.Unk0C);
                }
            }

            internal void WriteValues(BinaryWriterEx bw, GPGame game, int index, int unk3ValueIDsOffset) {
                if (this.ValueIDs.Count == 0) {
                    bw.FillInt32($"Unk3ValueIDsOffset{index}", 0);
                } else {
                    bw.FillInt32($"Unk3ValueIDsOffset{index}", (int)bw.Position - unk3ValueIDsOffset);
                    bw.WriteInt32s(this.ValueIDs);
                }
            }
        }
    }
}
