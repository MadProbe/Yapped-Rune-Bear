using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MSB3 {
        /// <summary>
        /// A section containing fixed poses for different Parts in the map.
        /// </summary>
        internal class MapstudioPartsPose : Param<PartsPose> {
            internal override int Version => 0;
            internal override string Type => "MAPSTUDIO_PARTS_POSE_ST";

            /// <summary>
            /// Parts pose entries in this section.
            /// </summary>
            public List<PartsPose> Poses { get; set; }

            /// <summary>
            /// Creates a new PartsPoseSection with no entries.
            /// </summary>
            public MapstudioPartsPose() => this.Poses = new List<PartsPose>();

            /// <summary>
            /// Returns every parts pose in the order they will be written.
            /// </summary>
            public override List<PartsPose> GetEntries() => this.Poses;

            internal override PartsPose ReadEntry(BinaryReaderEx br) => this.Poses.EchoAdd(new PartsPose(br));
        }

        /// <summary>
        /// A set of bone transforms to pose an individual Part in the map.
        /// </summary>
        public class PartsPose : Entry {
            /// <summary>
            /// The name of the part to pose.
            /// </summary>
            public string PartName { get; set; }
            private short PartIndex;

            /// <summary>
            /// Transforms for each bone.
            /// </summary>
            public List<Bone> Bones { get; set; }

            /// <summary>
            /// Creates an empty PartsPose.
            /// </summary>
            public PartsPose() => this.Bones = new List<Bone>();

            /// <summary>
            /// Creates a deep copy of the parts pose.
            /// </summary>
            public PartsPose DeepCopy() {
                var pose = (PartsPose)this.MemberwiseClone();
                pose.Bones = new List<Bone>(this.Bones.Count);
                foreach (Bone bone in this.Bones) {
                    pose.Bones.Add(bone.DeepCopy());
                }

                return pose;
            }

            internal PartsPose(BinaryReaderEx br) {
                this.PartIndex = br.ReadInt16();
                short boneCount = br.ReadInt16();
                _ = br.AssertInt32(0);
                _ = br.AssertInt64(0x10);

                this.Bones = new List<Bone>(boneCount);
                for (int i = 0; i < boneCount; i++) {
                    this.Bones.Add(new Bone(br));
                }
            }

            internal override void Write(BinaryWriterEx bw, int id) {
                bw.WriteInt16(this.PartIndex);
                bw.WriteInt16((short)this.Bones.Count);
                bw.WriteInt32(0);
                bw.WriteInt64(0x10);

                foreach (Bone member in this.Bones) {
                    member.Write(bw);
                }
            }

            internal void GetNames(MSB3 msb, Entries entries) {
                this.PartName = MSB.FindName(entries.Parts, this.PartIndex);
                foreach (Bone bone in this.Bones) {
                    bone.GetNames(entries);
                }
            }

            internal void GetIndices(MSB3 msb, Entries entries) {
                this.PartIndex = (short)MSB.FindIndex(entries.Parts, this.PartName);
                foreach (Bone bone in this.Bones) {
                    bone.GetIndices(entries);
                }
            }

            /// <summary>
            /// A transform for one bone in a model.
            /// </summary>
            public class Bone {
                /// <summary>
                /// The name of the bone to transform.
                /// </summary>
                public string Name { get; set; }
                private int NameIndex { get; set; }

                /// <summary>
                /// Translation of the bone.
                /// </summary>
                public Vector3 Translation { get; set; }

                /// <summary>
                /// Rotation of the bone.
                /// </summary>
                public Vector3 Rotation { get; set; }

                /// <summary>
                /// Scale of the bone.
                /// </summary>
                public Vector3 Scale { get; set; }

                /// <summary>
                /// Creates a Bone with default values.
                /// </summary>
                public Bone() {
                    this.Name = "Master";
                    this.Scale = Vector3.One;
                }

                /// <summary>
                /// Creates a deep copy of the bone.
                /// </summary>
                public Bone DeepCopy() => (Bone)this.MemberwiseClone();

                internal Bone(BinaryReaderEx br) {
                    this.NameIndex = br.ReadInt32();
                    this.Translation = br.ReadVector3();
                    this.Rotation = br.ReadVector3();
                    this.Scale = br.ReadVector3();
                }

                internal void Write(BinaryWriterEx bw) {
                    bw.WriteInt32(this.NameIndex);
                    bw.WriteVector3(this.Translation);
                    bw.WriteVector3(this.Rotation);
                    bw.WriteVector3(this.Scale);
                }

                internal void GetNames(Entries entries) => this.Name = MSB.FindName(entries.BoneNames, this.NameIndex);

                internal void GetIndices(Entries entries) {
                    if (!entries.BoneNames.Any(bn => bn.Name == this.Name)) {
                        entries.BoneNames.Add(new BoneName() { Name = Name });
                    }

                    this.NameIndex = MSB.FindIndex(entries.BoneNames, this.Name);
                }

                /// <summary>
                /// Returns the bone name index and transforms of this bone.
                /// </summary>
                public override string ToString() => $"{this.Name} : {this.Translation} {this.Rotation} {this.Scale}";
            }
        }
    }
}
