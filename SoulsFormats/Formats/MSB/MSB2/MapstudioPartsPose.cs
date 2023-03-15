using System.Collections.Generic;
using System.Numerics;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MSB2 {
        private class MapstudioPartsPose : Param<PartPose> {
            internal override int Version => 0;
            internal override string Name => "MAPSTUDIO_PARTS_POSE_ST";

            public List<PartPose> Poses { get; set; }

            public MapstudioPartsPose() => this.Poses = new List<PartPose>();

            internal override PartPose ReadEntry(BinaryReaderEx br) => this.Poses.EchoAdd(new PartPose(br));

            public override List<PartPose> GetEntries() => this.Poses;
        }

        /// <summary>
        /// A set of bone transforms to pose a rigged object.
        /// </summary>
        public class PartPose : Entry {
            /// <summary>
            /// The name of the part to be posed.
            /// </summary>
            public string PartName { get; set; }
            private short PartIndex;

            /// <summary>
            /// Transforms for each bone in the object.
            /// </summary>
            public List<Bone> Bones { get; set; }

            /// <summary>
            /// Creates an empty PartPose.
            /// </summary>
            public PartPose() => this.Bones = new List<Bone>();

            /// <summary>
            /// Creates a deep copy of the part pose.
            /// </summary>
            public PartPose DeepCopy() {
                var pose = (PartPose)this.MemberwiseClone();
                pose.Bones = new List<Bone>(this.Bones.Count);
                foreach (Bone bone in this.Bones) {
                    pose.Bones.Add(bone.DeepCopy());
                }

                return pose;
            }

            internal PartPose(BinaryReaderEx br) {
                long start = br.Position;
                this.PartIndex = br.ReadInt16();
                short boneCount = br.ReadInt16();
                if (br.VarintLong) {
                    _ = br.AssertInt32(0);
                }

                long bonesOffset = br.ReadVarint();

                br.Position = start + bonesOffset;
                this.Bones = new List<Bone>(boneCount);
                for (int i = 0; i < boneCount; i++) {
                    this.Bones.Add(new Bone(br));
                }
            }

            internal override void Write(BinaryWriterEx bw, int index) {
                long start = bw.Position;
                bw.WriteInt16(this.PartIndex);
                bw.WriteInt16((short)this.Bones.Count);
                if (bw.VarintLong) {
                    bw.WriteInt32(0);
                }

                bw.ReserveVarint("BonesOffset");

                bw.FillVarint("BonesOffset", bw.Position - start);
                foreach (Bone bone in this.Bones) {
                    bone.Write(bw);
                }
            }

            internal void GetNames(Entries entries) {
                this.PartName = MSB.FindName(entries.Parts, this.PartIndex);
                foreach (Bone bone in this.Bones) {
                    bone.GetNames(entries);
                }
            }

            internal void GetIndices(Lookups lookups, Entries entries) {
                this.PartIndex = (short)FindIndex(lookups.Parts, this.PartName);
                foreach (Bone bone in this.Bones) {
                    bone.GetIndices(lookups, entries);
                }
            }

            /// <summary>
            /// Returns a string representation of the pose.
            /// </summary>
            public override string ToString() => $"{this.PartName} [{this.Bones?.Count} Bones]";

            /// <summary>
            /// A transform for a single bone in an object.
            /// </summary>
            public class Bone {
                /// <summary>
                /// The name of the bone to transform.
                /// </summary>
                public string Name { get; set; }
                private int NameIndex;

                /// <summary>
                /// Translation of the bone.
                /// </summary>
                public Vector3 Translation { get; set; }

                /// <summary>
                /// Rotation of the bone, in radians.
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

                internal void GetIndices(Lookups lookups, Entries entries) {
                    if (!lookups.BoneNames.ContainsKey(this.Name)) {
                        lookups.BoneNames[this.Name] = entries.BoneNames.Count;
                        entries.BoneNames.Add(new BoneName() { Name = Name });
                    }
                    this.NameIndex = FindIndex(lookups.BoneNames, this.Name);
                }

                /// <summary>
                /// Returns a string representation of the bone.
                /// </summary>
                public override string ToString() => $"{this.Name} [Trans {this.Translation:F2} | Rot {this.Rotation:F2} | Scale {this.Scale:F2}]";
            }
        }
    }
}
