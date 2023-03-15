using System.Collections.Generic;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MQB {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class Timeline {
            public List<Disposition> Dispositions { get; set; }

            public List<CustomData> CustomData { get; set; }

            /// <summary>
            /// Unknown; possibly a timeline index.
            /// </summary>
            public int Unk10 { get; set; }

            public Timeline() {
                this.Dispositions = new List<Disposition>();
                this.CustomData = new List<CustomData>();
            }

            internal Timeline(BinaryReaderEx br, MQBVersion version, Dictionary<long, Disposition> disposesByOffset) {
                long disposOffsetsOffset = br.ReadVarint();
                int disposCount = br.ReadInt32();
                if (version == MQBVersion.DarkSouls2Scholar) {
                    _ = br.AssertInt32(0);
                }

                long customDataOffset = br.ReadVarint();
                int customDataCount = br.ReadInt32();
                this.Unk10 = br.ReadInt32();

                this.Dispositions = new List<Disposition>(disposCount);
                long[] disposOffsets = br.GetVarints(disposOffsetsOffset, disposCount);
                foreach (long disposOffset in disposOffsets) {
                    this.Dispositions.Add(disposesByOffset[disposOffset]);
                    _ = disposesByOffset.Remove(disposOffset);
                }

                br.StepIn(customDataOffset);
                {
                    this.CustomData = new List<CustomData>(customDataCount);
                    for (int i = 0; i < customDataCount; i++) {
                        this.CustomData.Add(new MQB.CustomData(br));
                    }
                }
                br.StepOut();
            }

            internal void WriteDispositions(BinaryWriterEx bw, Dictionary<Disposition, long> offsetsByDispos, List<CustomData> allCustomData, List<long> customDataValueOffsets) {
                foreach (Disposition dispos in this.Dispositions) {
                    offsetsByDispos[dispos] = bw.Position;
                    dispos.Write(bw, allCustomData, customDataValueOffsets);
                }
            }

            internal void Write(BinaryWriterEx bw, MQBVersion version, int cutIndex, int timelineIndex) {
                bw.ReserveVarint($"DisposOffsetsOffset[{cutIndex}:{timelineIndex}]");
                bw.WriteInt32(this.Dispositions.Count);
                if (version == MQBVersion.DarkSouls2Scholar) {
                    bw.WriteInt32(0);
                }

                bw.ReserveVarint($"TimelineCustomDataOffset[{cutIndex}:{timelineIndex}]");
                bw.WriteInt32(this.CustomData.Count);
                bw.WriteInt32(this.Unk10);
            }

            internal void WriteCustomData(BinaryWriterEx bw, int cutIndex, int timelineIndex, List<CustomData> allCustomData, List<long> customDataValueOffsets) {
                bw.FillVarint($"TimelineCustomDataOffset[{cutIndex}:{timelineIndex}]", bw.Position);
                foreach (CustomData customData in this.CustomData) {
                    customData.Write(bw, allCustomData, customDataValueOffsets);
                }
            }

            internal void WriteDisposOffsets(BinaryWriterEx bw, Dictionary<Disposition, long> offsetsByDispos, int cutIndex, int timelineIndex) {
                bw.FillVarint($"DisposOffsetsOffset[{cutIndex}:{timelineIndex}]", bw.Position);
                foreach (Disposition dispos in this.Dispositions) {
                    bw.WriteVarint(offsetsByDispos[dispos]);
                }
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
