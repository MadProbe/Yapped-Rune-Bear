using System.Collections.Generic;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MQB {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class Disposition {
            public int ID { get; set; }

            public int ResourceIndex { get; set; }

            /// <summary>
            /// Unknown; possibly a timeline index.
            /// </summary>
            public int Unk08 { get; set; }

            public int StartFrame { get; set; }

            /// <summary>
            /// Duration in frames.
            /// </summary>
            public int Duration { get; set; }

            public int Unk14 { get; set; }

            public int Unk18 { get; set; }

            public int Unk1C { get; set; }

            public int Unk20 { get; set; }

            public List<CustomData> CustomData { get; set; }

            public int Unk28 { get; set; }

            public List<Transform> Transforms { get; set; }

            public Disposition() {
                this.CustomData = new List<CustomData>();
                this.Transforms = new List<Transform>();
            }

            internal Disposition(BinaryReaderEx br) {
                this.ID = br.ReadInt32();
                this.ResourceIndex = br.ReadInt32();
                this.Unk08 = br.ReadInt32();
                this.StartFrame = br.ReadInt32();
                this.Duration = br.ReadInt32();
                this.Unk14 = br.ReadInt32();
                this.Unk18 = br.ReadInt32();
                this.Unk1C = br.ReadInt32();
                this.Unk20 = br.AssertInt32(0, 1);
                int customDataCount = br.ReadInt32();
                this.Unk28 = br.ReadInt32();
                _ = br.AssertInt32(0);

                this.CustomData = new List<CustomData>(customDataCount);
                for (int i = 0; i < customDataCount; i++) {
                    this.CustomData.Add(new CustomData(br));
                }

                _ = br.AssertInt32(0);
                int transformCount = br.ReadInt32();
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);

                this.Transforms = new List<Transform>(transformCount);
                for (int i = 0; i < transformCount; i++) {
                    this.Transforms.Add(new Transform(br));
                }
            }

            internal void Write(BinaryWriterEx bw, List<CustomData> allCustomData, List<long> customDataValueOffsets) {
                bw.WriteInt32(this.ID);
                bw.WriteInt32(this.ResourceIndex);
                bw.WriteInt32(this.Unk08);
                bw.WriteInt32(this.StartFrame);
                bw.WriteInt32(this.Duration);
                bw.WriteInt32(this.Unk14);
                bw.WriteInt32(this.Unk18);
                bw.WriteInt32(this.Unk1C);
                bw.WriteInt32(this.Unk20);
                bw.WriteInt32(this.CustomData.Count);
                bw.WriteInt32(this.Unk28);
                bw.WriteInt32(0);

                foreach (CustomData customData in this.CustomData) {
                    customData.Write(bw, allCustomData, customDataValueOffsets);
                }

                bw.WriteInt32(0);
                bw.WriteInt32(this.Transforms.Count);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);

                foreach (Transform transform in this.Transforms) {
                    transform.Write(bw);
                }
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
