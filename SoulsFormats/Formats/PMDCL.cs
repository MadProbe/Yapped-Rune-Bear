using System.Collections.Generic;
using System.Numerics;
using SoulsFormats.Util;

namespace SoulsFormats.Formats {
    /// <summary>
    /// Defines static decals in DS3 maps. Extension: .pmdcl
    /// </summary>
    public class PMDCL : SoulsFile<PMDCL> {
        /// <summary>
        /// Decals in this map.
        /// </summary>
        public List<Decal> Decals;

        /// <summary>
        /// Creates a new PMDCL with no decals.
        /// </summary>
        public PMDCL() => this.Decals = new List<Decal>();

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            br.BigEndian = false;

            long decalCount = br.ReadInt64();
            // Header size/offsets offset
            _ = br.AssertInt64(0x20);
            _ = br.AssertInt64(0);
            _ = br.AssertInt64(0);

            this.Decals = new List<Decal>((int)decalCount);
            for (int i = 0; i < decalCount; i++) {
                long offset = br.ReadInt64();
                br.StepIn(offset);
                {
                    this.Decals.Add(new Decal(br));
                }
                br.StepOut();
            }
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected internal override void Write(BinaryWriterEx bw) {
            bw.BigEndian = false;

            bw.WriteInt64(this.Decals.Count);
            bw.WriteInt64(0x20);
            bw.WriteInt64(0);
            bw.WriteInt64(0);

            for (int i = 0; i < this.Decals.Count; i++) {
                bw.ReserveInt64($"Decal{i}");
            }

            bw.Pad(0x20);
            for (int i = 0; i < this.Decals.Count; i++) {
                bw.FillInt64($"Decal{i}", bw.Position);
                this.Decals[i].Write(bw);
            }
        }

        /// <summary>
        /// Effects such as blood spatter that are applied on nearby surfaces.
        /// </summary>
        public class Decal {
            /// <summary>
            /// Unknown. Might not even be floats.
            /// </summary>
            public Vector3 XAngles, YAngles, ZAngles;

            /// <summary>
            /// Coordinates of the decal.
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// Unknown, 1 or 0 in existing files.
            /// </summary>
            public float Unk3C;

            /// <summary>
            /// ID of a row in DecalParam.
            /// </summary>
            public int DecalParamID;

            /// <summary>
            /// Controls the size of the decal in ways that are not entirely clear to me.
            /// </summary>
            public short Size1, Size2;

            /// <summary>
            /// Creates a new Decal with the given decal param and position, and other values default.
            /// </summary>
            public Decal(int decalParamID, Vector3 position) {
                this.XAngles = Vector3.Zero;
                this.YAngles = Vector3.Zero;
                this.ZAngles = Vector3.Zero;
                this.Position = position;
                this.Unk3C = 1;
                this.DecalParamID = decalParamID;
                this.Size1 = 10;
                this.Size2 = 10;
            }

            /// <summary>
            /// Creates a new Decal with values copied from another.
            /// </summary>
            public Decal(Decal clone) {
                this.XAngles = clone.XAngles;
                this.YAngles = clone.YAngles;
                this.ZAngles = clone.ZAngles;
                this.Position = clone.Position;
                this.Unk3C = clone.Unk3C;
                this.DecalParamID = clone.DecalParamID;
                this.Size1 = clone.Size1;
                this.Size2 = clone.Size2;
            }

            internal Decal(BinaryReaderEx br) {
                this.XAngles = br.ReadVector3();
                _ = br.AssertInt32(0);
                this.YAngles = br.ReadVector3();
                _ = br.AssertInt32(0);
                this.ZAngles = br.ReadVector3();
                _ = br.AssertInt32(0);
                this.Position = br.ReadVector3();
                this.Unk3C = br.ReadSingle();
                this.DecalParamID = br.ReadInt32();
                this.Size1 = br.ReadInt16();
                this.Size2 = br.ReadInt16();
                _ = br.AssertInt64(0);
                _ = br.AssertInt64(0);
                _ = br.AssertInt64(0);
            }

            internal void Write(BinaryWriterEx bw) {
                bw.WriteVector3(this.XAngles);
                bw.WriteInt32(0);
                bw.WriteVector3(this.YAngles);
                bw.WriteInt32(0);
                bw.WriteVector3(this.ZAngles);
                bw.WriteInt32(0);
                bw.WriteVector3(this.Position);
                bw.WriteSingle(this.Unk3C);
                bw.WriteInt32(this.DecalParamID);
                bw.WriteInt16(this.Size1);
                bw.WriteInt16(this.Size2);
                bw.WriteInt64(0);
                bw.WriteInt64(0);
                bw.WriteInt64(0);
            }
        }
    }
}
