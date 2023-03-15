using System;
using SoulsFormats.Util;

namespace SoulsFormats {
    public static partial class FLVER {
        /// <summary>
        /// Represents one property of a vertex.
        /// </summary>
        public class LayoutMember {
            /// <summary>
            /// Unknown; 0, 1, or 2.
            /// </summary>
            public int Unk00 { get; set; }

            /// <summary>
            /// Format used to store this member.
            /// </summary>
            public LayoutType Type { get; set; }

            /// <summary>
            /// Vertex property being stored.
            /// </summary>
            public LayoutSemantic Semantic { get; set; }

            /// <summary>
            /// For semantics that may appear more than once such as UVs, which one this member is.
            /// </summary>
            public int Index { get; set; }

            /// <summary>
            /// The size of this member's ValueType, in bytes.
            /// </summary>
            public int Size => this.Type switch {
                LayoutType.EdgeCompressed => 1,
                LayoutType.Byte4A or LayoutType.Byte4B or LayoutType.Short2toFloat2 or LayoutType.Byte4C or LayoutType.UV or LayoutType.Byte4E => 4,
                LayoutType.Float2 or LayoutType.UVPair or LayoutType.ShortBoneIndices or LayoutType.Short4toFloat4A or LayoutType.Short4toFloat4B => 8,
                LayoutType.Float3 => 12,
                LayoutType.Float4 => 16,
                _ => throw new NotImplementedException($"No size defined for buffer layout type: {this.Type}"),
            };

            /// <summary>
            /// Creates a LayoutMember with the specified values.
            /// </summary>
            public LayoutMember(LayoutType type, LayoutSemantic semantic, int index = 0, int unk00 = 0) {
                this.Unk00 = unk00;
                this.Type = type;
                this.Semantic = semantic;
                this.Index = index;
            }

            internal LayoutMember(BinaryReaderEx br, int structOffset) {
                this.Unk00 = br.ReadInt32();
                _ = br.AssertInt32(structOffset);
                this.Type = br.ReadEnum32<LayoutType>();
                this.Semantic = br.ReadEnum32<LayoutSemantic>();
                this.Index = br.ReadInt32();
            }

            internal void Write(BinaryWriterEx bw, int structOffset) {
                bw.WriteInt32(this.Unk00);
                bw.WriteInt32(structOffset);
                bw.WriteUInt32((uint)this.Type);
                bw.WriteUInt32((uint)this.Semantic);
                bw.WriteInt32(this.Index);
            }

            /// <summary>
            /// Returns the value type and semantic of this member.
            /// </summary>
            public override string ToString() => $"{this.Type}: {this.Semantic}";
        }

        /// <summary>
        /// Format of a vertex property.
        /// </summary>
        public enum LayoutType : uint {
            /// <summary>
            /// Two single-precision floats.
            /// </summary>
            Float2 = 0x01,

            /// <summary>
            /// Three single-precision floats.
            /// </summary>
            Float3 = 0x02,

            /// <summary>
            /// Four single-precision floats.
            /// </summary>
            Float4 = 0x03,

            /// <summary>
            /// Unknown.
            /// </summary>
            Byte4A = 0x10,

            /// <summary>
            /// Four bytes.
            /// </summary>
            Byte4B = 0x11,

            /// <summary>
            /// Two shorts?
            /// </summary>
            Short2toFloat2 = 0x12,

            /// <summary>
            /// Four bytes.
            /// </summary>
            Byte4C = 0x13,

            /// <summary>
            /// Two shorts.
            /// </summary>
            UV = 0x15,

            /// <summary>
            /// Two shorts and two shorts.
            /// </summary>
            UVPair = 0x16,

            /// <summary>
            /// Four shorts, maybe unsigned?
            /// </summary>
            ShortBoneIndices = 0x18,

            /// <summary>
            /// Four shorts.
            /// </summary>
            Short4toFloat4A = 0x1A,

            /// <summary>
            /// Unknown.
            /// </summary>
            Short4toFloat4B = 0x2E,

            /// <summary>
            /// Unknown.
            /// </summary>
            Byte4E = 0x2F,

            /// <summary>
            /// Unknown but appears to be another form of edge compression; not actually supported.
            /// </summary>
            EdgeCompressed = 0xF0,
        }

        /// <summary>
        /// Property of a vertex.
        /// </summary>
        public enum LayoutSemantic : uint {
            /// <summary>
            /// Location of the vertex.
            /// </summary>
            Position = 0,

            /// <summary>
            /// Weight of the vertex's attachment to bones.
            /// </summary>
            BoneWeights = 1,

            /// <summary>
            /// Bones the vertex is weighted to, indexing the parent mesh's bone indices.
            /// </summary>
            BoneIndices = 2,

            /// <summary>
            /// Orientation of the vertex.
            /// </summary>
            Normal = 3,

            /// <summary>
            /// Texture coordinates of the vertex.
            /// </summary>
            UV = 5,

            /// <summary>
            /// Vector pointing perpendicular to the normal.
            /// </summary>
            Tangent = 6,

            /// <summary>
            /// Vector pointing perpendicular to the normal and tangent.
            /// </summary>
            Bitangent = 7,

            /// <summary>
            /// Data used for blending, alpha, etc.
            /// </summary>
            VertexColor = 10,
        }
    }
}
