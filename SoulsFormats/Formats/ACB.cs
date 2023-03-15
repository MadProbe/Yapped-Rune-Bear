using System;
using System.Collections.Generic;
using SoulsFormats.Util;

namespace SoulsFormats.Formats {
    /// <summary>
    /// An rendering configuration file for various game assets, only used in DS2. Extension: .acb
    /// </summary>
    public class ACB : SoulsFile<ACB> {
        /// <summary>
        /// True for PS3/X360, false otherwise.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// Assets configured by this ACB.
        /// </summary>
        public List<Asset> Assets { get; set; }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected internal override bool Is(BinaryReaderEx br) => br.Length >= 4 && br.GetASCII(0, 4) == "ACB\0";

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            this.BigEndian = br.GetUInt32(0xC) > br.Length;
            br.BigEndian = this.BigEndian;

            _ = br.AssertASCII("ACB\0");
            _ = br.AssertByte(2);
            _ = br.AssertByte(1);
            _ = br.AssertByte(0);
            _ = br.AssertByte(0);
            int assetCount = br.ReadInt32();
            _ = br.ReadInt32(); // Offset index offset

            this.Assets = new List<Asset>(assetCount);
            foreach (int assetOffset in br.ReadInt32s(assetCount)) {
                br.Position = assetOffset;
                AssetType type = br.GetEnum16<AssetType>(br.Position + 8);
                if (type == AssetType.PWV) {
                    this.Assets.Add(new Asset.PWV(br));
                } else if (type == AssetType.General) {
                    this.Assets.Add(new Asset.General(br));
                } else if (type == AssetType.Model) {
                    this.Assets.Add(new Asset.Model(br));
                } else if (type == AssetType.Texture) {
                    this.Assets.Add(new Asset.Texture(br));
                } else if (type == AssetType.GITexture) {
                    this.Assets.Add(new Asset.GITexture(br));
                } else if (type == AssetType.Motion) {
                    this.Assets.Add(new Asset.Motion(br));
                } else {
                    throw new NotImplementedException($"Unsupported asset type: {type}");
                }
            }
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected internal override void Write(BinaryWriterEx bw) {
            var offsetIndex = new List<int>();
            var memberOffsetsIndex = new SortedDictionary<int, List<int>>();

            bw.BigEndian = this.BigEndian;
            bw.WriteASCII("ACB\0");
            bw.WriteByte(2);
            bw.WriteByte(1);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteInt32(this.Assets.Count);
            bw.ReserveInt32("OffsetIndexOffset");

            for (int i = 0; i < this.Assets.Count; i++) {
                offsetIndex.Add((int)bw.Position);
                bw.ReserveInt32($"AssetOffset{i}");
            }

            for (int i = 0; i < this.Assets.Count; i++) {
                bw.FillInt32($"AssetOffset{i}", (int)bw.Position);
                this.Assets[i].Write(bw, i, offsetIndex, memberOffsetsIndex);
            }

            for (int i = 0; i < this.Assets.Count; i++) {
                if (this.Assets[i] is Asset.Model model) {
                    model.WriteMembers(bw, i, offsetIndex, memberOffsetsIndex);
                }
            }

            for (int i = 0; i < this.Assets.Count; i++) {
                this.Assets[i].WritePaths(bw, i);
            }

            for (int i = 0; i < this.Assets.Count; i++) {
                if (this.Assets[i] is Asset.Model model && model.Members != null) {
                    for (int j = 0; j < model.Members.Count; j++) {
                        model.Members[j].WriteText(bw, i, j);
                    }
                }
            }

            bw.Pad(4);
            bw.FillInt32("OffsetIndexOffset", (int)bw.Position);
            bw.WriteInt32s(offsetIndex);
            foreach (List<int> offsets in memberOffsetsIndex.Values) {
                bw.WriteInt32s(offsets);
            }
        }

        /// <summary>
        /// The specific type of an asset.
        /// </summary>
        public enum AssetType : ushort {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            PWV = 0,
            General = 1,
            Model = 2,
            Texture = 3,
            GITexture = 4,
            Motion = 5,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }

        /// <summary>
        /// A model, texture, or miscellanous asset configuration.
        /// </summary>
        public abstract class Asset {
            /// <summary>
            /// The specific type of this asset.
            /// </summary>
            public abstract AssetType Type { get; }

            /// <summary>
            /// Full network path to the source file.
            /// </summary>
            public string AbsolutePath { get; set; }

            /// <summary>
            /// Relative path to the source file.
            /// </summary>
            public string RelativePath { get; set; }

            internal Asset() {
                this.AbsolutePath = "";
                this.RelativePath = "";
            }

            internal Asset(BinaryReaderEx br) {
                int absolutePathOffset = br.ReadInt32();
                int relativePathOffset = br.ReadInt32();
                _ = br.AssertUInt16((ushort)this.Type);

                this.AbsolutePath = br.GetUTF16(absolutePathOffset);
                this.RelativePath = br.GetUTF16(relativePathOffset);
            }

            internal virtual void Write(BinaryWriterEx bw, int index, List<int> offsetIndex, SortedDictionary<int, List<int>> membersOffsetIndex) {
                offsetIndex.Add((int)bw.Position);
                bw.ReserveInt32($"AbsolutePathOffset{index}");
                offsetIndex.Add((int)bw.Position);
                bw.ReserveInt32($"RelativePathOffset{index}");
                bw.WriteUInt16((ushort)this.Type);
            }

            internal void WritePaths(BinaryWriterEx bw, int index) {
                bw.FillInt32($"AbsolutePathOffset{index}", (int)bw.Position);
                bw.WriteUTF16(this.AbsolutePath, true);

                bw.FillInt32($"RelativePathOffset{index}", (int)bw.Position);
                bw.WriteUTF16(this.RelativePath, true);
            }

            /// <summary>
            /// Returns a string representation of the entry.
            /// </summary>
            public override string ToString() => $"{this.Type}: {this.RelativePath} | {this.AbsolutePath}";

            /// <summary>
            /// Unknown.
            /// </summary>
            public class PWV : Asset {
                /// <summary>
                /// AssetType.PWV
                /// </summary>
                public override AssetType Type => AssetType.PWV;

                /// <summary>
                /// Creates a PWV with default values.
                /// </summary>
                public PWV() : base() { }

                internal PWV(BinaryReaderEx br) : base(br) {
                    _ = br.AssertInt16(0);
                    _ = br.AssertInt32(0);
                }

                internal override void Write(BinaryWriterEx bw, int index, List<int> offsetIndex, SortedDictionary<int, List<int>> membersOffsetIndex) {
                    base.Write(bw, index, offsetIndex, membersOffsetIndex);
                    bw.WriteInt16(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Miscellaneous assets including collisions and lighting configs.
            /// </summary>
            public class General : Asset {
                /// <summary>
                /// AssetType.General
                /// </summary>
                public override AssetType Type => AssetType.General;

                /// <summary>
                /// Creates a General with default values.
                /// </summary>
                public General() : base() { }

                internal General(BinaryReaderEx br) : base(br) {
                    _ = br.AssertInt16(0);
                    _ = br.AssertInt32(0);
                }

                internal override void Write(BinaryWriterEx bw, int index, List<int> offsetIndex, SortedDictionary<int, List<int>> membersOffsetIndex) {
                    base.Write(bw, index, offsetIndex, membersOffsetIndex);
                    bw.WriteInt16(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Rendering options for 3D models.
            /// </summary>
            public class Model : Asset {
                /// <summary>
                /// AssetType.Model
                /// </summary>
                public override AssetType Type => AssetType.Model;

                /// <summary>
                /// 0 for objects and characters, 1 for map pieces.
                /// </summary>
                public short Unk0A { get; set; }

                /// <summary>
                /// Unknown; may be null.
                /// </summary>
                public MemberList Members { get; set; }

                /// <summary>
                /// Distance at which the model becomes invisible.
                /// </summary>
                public int DrawDistance { get; set; }

                /// <summary>
                /// Indirectly determines when lod facesets are used; observed values 0-3.
                /// </summary>
                public short MeshLodRate { get; set; }

                /// <summary>
                /// Whether the model appears in reflective surfaces like water.
                /// </summary>
                public bool Reflectible { get; set; }

                /// <summary>
                /// Enables interaction normals for water.
                /// </summary>
                public bool NormalInteraction { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk20 { get; set; }

                /// <summary>
                /// Unknown; alters rendering mode somehow.
                /// </summary>
                public byte RenderType { get; set; }

                /// <summary>
                /// If true, the model does not cast shadows.
                /// </summary>
                public bool DisableShadowSource { get; set; }

                /// <summary>
                /// If true, shadows will not be cast on the model.
                /// </summary>
                public bool DisableShadowTarget { get; set; }

                /// <summary>
                /// Unknown; makes things render in reverse order or reverses culling or something.
                /// </summary>
                public bool Unk27 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk28 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool Unk2C { get; set; }

                /// <summary>
                /// If true, the model is always centered on the camera position. Used for skyboxes.
                /// </summary>
                public bool FixToCamera { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool Unk2E { get; set; }

                /// <summary>
                /// Distance at which low textures are used.
                /// </summary>
                public short LowTextureDistance { get; set; }

                /// <summary>
                /// Distance at which the model uses simplified rendering.
                /// </summary>
                public short CheapRenderDistance { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Unk34 { get; set; }

                /// <summary>
                /// Unknown; disables lighting on water/transparencies.
                /// </summary>
                public bool Unk35 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool Unk36 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool Unk37 { get; set; }

                /// <summary>
                /// Creates a Model with default values.
                /// </summary>
                public Model() : base() => this.Reflectible = true;

                internal Model(BinaryReaderEx br) : base(br) {
                    this.Unk0A = br.ReadInt16();
                    int membersOffset = br.ReadInt32();
                    this.DrawDistance = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    this.MeshLodRate = br.ReadInt16();
                    this.Reflectible = br.ReadBoolean();
                    this.NormalInteraction = br.ReadBoolean();
                    this.Unk20 = br.ReadInt32();
                    this.RenderType = br.ReadByte();
                    this.DisableShadowSource = br.ReadBoolean();
                    this.DisableShadowTarget = br.ReadBoolean();
                    this.Unk27 = br.ReadBoolean();
                    this.Unk28 = br.ReadSingle();
                    this.Unk2C = br.ReadBoolean();
                    this.FixToCamera = br.ReadBoolean();
                    this.Unk2E = br.ReadBoolean();
                    _ = br.AssertByte(0);
                    this.LowTextureDistance = br.ReadInt16();
                    this.CheapRenderDistance = br.ReadInt16();
                    this.Unk34 = br.ReadByte();
                    this.Unk35 = br.ReadBoolean();
                    this.Unk36 = br.ReadBoolean();
                    this.Unk37 = br.ReadBoolean();
                    br.AssertPattern(0x18, 0x00);

                    if (membersOffset != 0) {
                        br.Position = membersOffset;
                        this.Members = new MemberList(br);
                    }
                }

                internal override void Write(BinaryWriterEx bw, int index, List<int> offsetIndex, SortedDictionary<int, List<int>> membersOffsetIndex) {
                    base.Write(bw, index, offsetIndex, membersOffsetIndex);
                    bw.WriteInt16(this.Unk0A);
                    membersOffsetIndex[index] = new List<int>();
                    if (this.Members != null) {
                        membersOffsetIndex[index].Add((int)bw.Position);
                    }

                    bw.ReserveInt32($"MembersOffset{index}");
                    bw.WriteInt32(this.DrawDistance);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt16(this.MeshLodRate);
                    bw.WriteBoolean(this.Reflectible);
                    bw.WriteBoolean(this.NormalInteraction);
                    bw.WriteInt32(this.Unk20);
                    bw.WriteByte(this.RenderType);
                    bw.WriteBoolean(this.DisableShadowSource);
                    bw.WriteBoolean(this.DisableShadowTarget);
                    bw.WriteBoolean(this.Unk27);
                    bw.WriteSingle(this.Unk28);
                    bw.WriteBoolean(this.Unk2C);
                    bw.WriteBoolean(this.FixToCamera);
                    bw.WriteBoolean(this.Unk2E);
                    bw.WriteByte(0);
                    bw.WriteInt16(this.LowTextureDistance);
                    bw.WriteInt16(this.CheapRenderDistance);
                    bw.WriteByte(this.Unk34);
                    bw.WriteBoolean(this.Unk35);
                    bw.WriteBoolean(this.Unk36);
                    bw.WriteBoolean(this.Unk37);
                    bw.WritePattern(0x18, 0x00);
                }

                internal void WriteMembers(BinaryWriterEx bw, int index, List<int> offsetIndex, SortedDictionary<int, List<int>> membersOffsetIndex) {
                    if (this.Members == null) {
                        bw.FillInt32($"MembersOffset{index}", 0);
                    } else {
                        bw.FillInt32($"MembersOffset{index}", (int)bw.Position);
                        this.Members.Write(bw, index, offsetIndex, membersOffsetIndex);
                    }
                }

                /// <summary>
                /// Unknown collection of unknown items.
                /// </summary>
                public class MemberList : List<Member> {
                    /// <summary>
                    /// Unknown; usually -1.
                    /// </summary>
                    public short Unk00 { get; set; }

                    /// <summary>
                    /// Creates an empty MemberList.
                    /// </summary>
                    public MemberList() : base() { }

                    /// <summary>
                    /// Creates an empty MemberList with the specified capacity.
                    /// </summary>
                    public MemberList(int capacity) : base(capacity) { }

                    /// <summary>
                    /// Creates a MemberList with elements copied from the specified collection.
                    /// </summary>
                    public MemberList(IEnumerable<Member> collection) : base(collection) { }

                    internal MemberList(BinaryReaderEx br) {
                        this.Unk00 = br.ReadInt16();
                        short memberCount = br.ReadInt16();
                        int memberOffsetsOffset = br.ReadInt32();

                        br.StepIn(memberOffsetsOffset);
                        {
                            this.Capacity = memberCount;
                            int[] memberOffsets = br.ReadInt32s(memberCount);
                            for (int i = 0; i < memberCount; i++) {
                                br.Position = memberOffsets[i];
                                this.Add(new Member(br));
                            }
                        }
                        br.StepOut();
                    }

                    internal void Write(BinaryWriterEx bw, int index, List<int> offsetIndex, SortedDictionary<int, List<int>> membersOffsetIndex) {
                        bw.WriteInt16(this.Unk00);
                        bw.WriteInt16((short)this.Count);
                        membersOffsetIndex[index].Add((int)bw.Position);
                        bw.ReserveInt32($"MemberOffsetsOffset{index}");

                        // :^)
                        bw.FillInt32($"MemberOffsetsOffset{index}", (int)bw.Position);
                        for (int i = 0; i < this.Count; i++) {
                            membersOffsetIndex[index].Add((int)bw.Position);
                            bw.ReserveInt32($"MemberOffset{index}:{i}");
                        }

                        for (int i = 0; i < this.Count; i++) {
                            bw.FillInt32($"MemberOffset{index}:{i}", (int)bw.Position);
                            this[i].Write(bw, index, i, offsetIndex);
                        }
                    }
                }

                /// <summary>
                /// Unknown.
                /// </summary>
                public class Member {
                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public string Text { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk04 { get; set; }

                    /// <summary>
                    /// Creates a Member with default values.
                    /// </summary>
                    public Member() => this.Text = "";

                    internal Member(BinaryReaderEx br) {
                        int textOffset = br.ReadInt32();
                        this.Unk04 = br.ReadInt32();

                        this.Text = br.GetUTF16(textOffset);
                    }

                    internal void Write(BinaryWriterEx bw, int entryIndex, int memberIndex, List<int> offsetIndex) {
                        offsetIndex.Add((int)bw.Position);
                        bw.ReserveInt32($"MemberTextOffset{entryIndex}:{memberIndex}");
                        bw.WriteInt32(this.Unk04);
                    }

                    internal void WriteText(BinaryWriterEx bw, int entryIndex, int memberIndex) {
                        bw.FillInt32($"MemberTextOffset{entryIndex}:{memberIndex}", (int)bw.Position);
                        bw.WriteUTF16(this.Text, true);
                    }
                }
            }

            /// <summary>
            /// Diffuse, normal, and specular maps.
            /// </summary>
            public class Texture : Asset {
                /// <summary>
                /// AssetType.Texture
                /// </summary>
                public override AssetType Type => AssetType.Texture;

                /// <summary>
                /// Creates a Texture with default values.
                /// </summary>
                public Texture() : base() { }

                internal Texture(BinaryReaderEx br) : base(br) {
                    _ = br.AssertInt16(0);
                    _ = br.AssertInt32(0);
                }

                internal override void Write(BinaryWriterEx bw, int index, List<int> offsetIndex, SortedDictionary<int, List<int>> membersOffsetIndex) {
                    base.Write(bw, index, offsetIndex, membersOffsetIndex);
                    bw.WriteInt16(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Lightmaps and envmaps.
            /// </summary>
            public class GITexture : Asset {
                /// <summary>
                /// AssetType.GITexture
                /// </summary>
                public override AssetType Type => AssetType.GITexture;

                /// <summary>
                /// Unknown; probably 4 bytes.
                /// </summary>
                public int Unk10 { get; set; }

                /// <summary>
                /// Unknown; probably 4 bytes.
                /// </summary>
                public int Unk14 { get; set; }

                /// <summary>
                /// Creates a GITexture with default values.
                /// </summary>
                public GITexture() : base() { }

                internal GITexture(BinaryReaderEx br) : base(br) {
                    _ = br.AssertInt16(0);
                    _ = br.AssertInt32(0);
                    this.Unk10 = br.ReadInt32();
                }

                internal override void Write(BinaryWriterEx bw, int index, List<int> offsetIndex, SortedDictionary<int, List<int>> membersOffsetIndex) {
                    base.Write(bw, index, offsetIndex, membersOffsetIndex);
                    bw.WriteInt16(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(this.Unk10);
                }
            }

            /// <summary>
            /// Animation files used in cutscenes.
            /// </summary>
            public class Motion : Asset {
                /// <summary>
                /// AssetType.Motion
                /// </summary>
                public override AssetType Type => AssetType.Motion;

                /// <summary>
                /// Creates a Motion with default values.
                /// </summary>
                public Motion() : base() { }

                internal Motion(BinaryReaderEx br) : base(br) {
                    _ = br.AssertInt16(0);
                    _ = br.AssertInt32(0);
                }

                internal override void Write(BinaryWriterEx bw, int index, List<int> offsetIndex, SortedDictionary<int, List<int>> membersOffsetIndex) {
                    base.Write(bw, index, offsetIndex, membersOffsetIndex);
                    bw.WriteInt16(0);
                    bw.WriteInt32(0);
                }
            }
        }
    }
}
