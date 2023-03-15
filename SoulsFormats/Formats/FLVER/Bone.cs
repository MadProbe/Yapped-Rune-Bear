using System.Numerics;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class FLVER {
        /// <summary>
        /// A joint available for vertices to be attached to.
        /// </summary>
        public class Bone {
            /// <summary>
            /// Corresponds to the name of a bone in the parent skeleton, if present.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Index of the parent in this FLVER's bone collection, or -1 for none.
            /// </summary>
            public short ParentIndex { get; set; }

            /// <summary>
            /// Index of the first child in this FLVER's bone collection, or -1 for none.
            /// </summary>
            public short ChildIndex { get; set; }

            /// <summary>
            /// Index of the next child of this bone's parent, or -1 for none.
            /// </summary>
            public short NextSiblingIndex { get; set; }

            /// <summary>
            /// Index of the previous child of this bone's parent, or -1 for none.
            /// </summary>
            public short PreviousSiblingIndex { get; set; }

            /// <summary>
            /// Translation of this bone.
            /// </summary>
            public Vector3 Translation { get; set; }

            /// <summary>
            /// Rotation of this bone; euler radians in XZY order.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// Scale of this bone.
            /// </summary>
            public Vector3 Scale { get; set; }

            /// <summary>
            /// Minimum extent of the vertices weighted to this bone.
            /// </summary>
            public Vector3 BoundingBoxMin { get; set; }

            /// <summary>
            /// Maximum extent of the vertices weighted to this bone.
            /// </summary>
            public Vector3 BoundingBoxMax { get; set; }

            /// <summary>
            /// Unknown; only 0 or 1 before Sekiro.
            /// </summary>
            public int Unk3C { get; set; }

            /// <summary>
            /// Creates a Bone with default values.
            /// </summary>
            public Bone() {
                this.Name = "";
                this.ParentIndex = -1;
                this.ChildIndex = -1;
                this.NextSiblingIndex = -1;
                this.PreviousSiblingIndex = -1;
                this.Scale = Vector3.One;
            }

            /// <summary>
            /// Creates a transformation matrix from the scale, rotation, and translation of the bone.
            /// </summary>
            public Matrix4x4 ComputeLocalTransform() => Matrix4x4.CreateScale(this.Scale)
                    * Matrix4x4.CreateRotationX(this.Rotation.X)
                    * Matrix4x4.CreateRotationZ(this.Rotation.Z)
                    * Matrix4x4.CreateRotationY(this.Rotation.Y)
                    * Matrix4x4.CreateTranslation(this.Translation);

            /// <summary>
            /// Returns a string representation of the bone.
            /// </summary>
            public override string ToString() => this.Name;

            internal Bone(BinaryReaderEx br, bool unicode) {
                this.Translation = br.ReadVector3();
                int nameOffset = br.ReadInt32();
                this.Rotation = br.ReadVector3();
                this.ParentIndex = br.ReadInt16();
                this.ChildIndex = br.ReadInt16();
                this.Scale = br.ReadVector3();
                this.NextSiblingIndex = br.ReadInt16();
                this.PreviousSiblingIndex = br.ReadInt16();
                this.BoundingBoxMin = br.ReadVector3();
                this.Unk3C = br.ReadInt32();
                this.BoundingBoxMax = br.ReadVector3();
                br.AssertPattern(0x34, 0x00);

                this.Name = unicode ? br.GetUTF16(nameOffset) : br.GetShiftJIS(nameOffset);
            }

            internal void Write(BinaryWriterEx bw, int index) {
                bw.WriteVector3(this.Translation);
                bw.ReserveInt32($"BoneNameOffset{index}");
                bw.WriteVector3(this.Rotation);
                bw.WriteInt16(this.ParentIndex);
                bw.WriteInt16(this.ChildIndex);
                bw.WriteVector3(this.Scale);
                bw.WriteInt16(this.NextSiblingIndex);
                bw.WriteInt16(this.PreviousSiblingIndex);
                bw.WriteVector3(this.BoundingBoxMin);
                bw.WriteInt32(this.Unk3C);
                bw.WriteVector3(this.BoundingBoxMax);
                bw.WritePattern(0x34, 0x00);
            }

            internal void WriteStrings(BinaryWriterEx bw, bool unicode, int index) {
                bw.FillInt32($"BoneNameOffset{index}", (int)bw.Position);
                if (unicode) {
                    bw.WriteUTF16(this.Name, true);
                } else {
                    bw.WriteShiftJIS(this.Name, true);
                }
            }
        }
    }
}
