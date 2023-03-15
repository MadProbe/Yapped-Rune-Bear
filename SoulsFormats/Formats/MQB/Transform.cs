using System.Numerics;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MQB {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class Transform {
            public float Frame { get; set; }

            public Vector3 Translation { get; set; }

            public Vector3 Unk10 { get; set; }

            public Vector3 Unk1C { get; set; }

            public Vector3 Rotation { get; set; }

            public Vector3 Unk34 { get; set; }

            public Vector3 Unk40 { get; set; }

            public Vector3 Scale { get; set; }

            public Vector3 Unk58 { get; set; }

            public Vector3 Unk64 { get; set; }

            public Transform() => this.Scale = Vector3.One;

            internal Transform(BinaryReaderEx br) {
                this.Frame = br.ReadSingle();
                this.Translation = br.ReadVector3();
                this.Unk10 = br.ReadVector3();
                this.Unk1C = br.ReadVector3();
                this.Rotation = br.ReadVector3();
                this.Unk34 = br.ReadVector3();
                this.Unk40 = br.ReadVector3();
                this.Scale = br.ReadVector3();
                this.Unk58 = br.ReadVector3();
                this.Unk64 = br.ReadVector3();
            }

            internal void Write(BinaryWriterEx bw) {
                bw.WriteSingle(this.Frame);
                bw.WriteVector3(this.Translation);
                bw.WriteVector3(this.Unk10);
                bw.WriteVector3(this.Unk1C);
                bw.WriteVector3(this.Rotation);
                bw.WriteVector3(this.Unk34);
                bw.WriteVector3(this.Unk40);
                bw.WriteVector3(this.Scale);
                bw.WriteVector3(this.Unk58);
                bw.WriteVector3(this.Unk64);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
