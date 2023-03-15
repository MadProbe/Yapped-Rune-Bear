using System.Drawing;
using System.Numerics;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class FLVER {
        /// <summary>
        /// "Dummy polygons" used for hit detection, particle effect locations, and much more.
        /// </summary>
        public class Dummy {
            /// <summary>
            /// Location of the dummy point.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Vector indicating the dummy point's forward direction.
            /// </summary>
            public Vector3 Forward { get; set; }

            /// <summary>
            /// Vector indicating the dummy point's upward direction.
            /// </summary>
            public Vector3 Upward { get; set; }

            /// <summary>
            /// Indicates the type of dummy point this is (hitbox, sfx, etc).
            /// </summary>
            public short ReferenceID { get; set; }

            /// <summary>
            /// Index of a bone that the dummy point is initially transformed to before binding to the attach bone.
            /// </summary>
            public short ParentBoneIndex { get; set; }

            /// <summary>
            /// Index of the bone that the dummy point follows physically.
            /// </summary>
            public short AttachBoneIndex { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public Color Color { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool Flag1 { get; set; }

            /// <summary>
            /// If false, the upward vector is not read.
            /// </summary>
            public bool UseUpwardVector { get; set; }

            /// <summary>
            /// Unknown; only used in Sekiro.
            /// </summary>
            public int Unk30 { get; set; }

            /// <summary>
            /// Unknown; only used in Sekiro.
            /// </summary>
            public int Unk34 { get; set; }

            /// <summary>
            /// Creates a new dummy point with default values.
            /// </summary>
            public Dummy() {
                this.ParentBoneIndex = -1;
                this.AttachBoneIndex = -1;
            }

            /// <summary>
            /// Returns a string representation of the dummy.
            /// </summary>
            public override string ToString() => $"{this.ReferenceID}";

            internal Dummy(BinaryReaderEx br, int version) {
                this.Position = br.ReadVector3();
                // Not certain about the ordering of RGB here
                this.Color = version == 0x20010 ? br.ReadBGRA() : br.ReadARGB();
                this.Forward = br.ReadVector3();
                this.ReferenceID = br.ReadInt16();
                this.ParentBoneIndex = br.ReadInt16();
                this.Upward = br.ReadVector3();
                this.AttachBoneIndex = br.ReadInt16();
                this.Flag1 = br.ReadBoolean();
                this.UseUpwardVector = br.ReadBoolean();
                this.Unk30 = br.ReadInt32();
                this.Unk34 = br.ReadInt32();
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);
            }

            internal void Write(BinaryWriterEx bw, int version) {
                bw.WriteVector3(this.Position);
                if (version == 0x20010) {
                    bw.WriteBGRA(this.Color);
                } else {
                    bw.WriteARGB(this.Color);
                }

                bw.WriteVector3(this.Forward);
                bw.WriteInt16(this.ReferenceID);
                bw.WriteInt16(this.ParentBoneIndex);
                bw.WriteVector3(this.Upward);
                bw.WriteInt16(this.AttachBoneIndex);
                bw.WriteBoolean(this.Flag1);
                bw.WriteBoolean(this.UseUpwardVector);
                bw.WriteInt32(this.Unk30);
                bw.WriteInt32(this.Unk34);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
            }
        }
    }
}
