using System.Collections.Generic;
using System.Linq;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MQB {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class Cut {
            public string Name { get; set; }

            public int Unk44 { get; set; }

            /// <summary>
            /// Duration of the cut in frames.
            /// </summary>
            public int Duration { get; set; }

            public List<Timeline> Timelines { get; set; }

            public Cut() {
                this.Name = "";
                this.Timelines = new List<Timeline>();
            }

            internal Cut(BinaryReaderEx br, MQBVersion version) {
                this.Name = br.ReadFixStrW(0x40);
                int disposCount = br.ReadInt32();
                this.Unk44 = br.ReadInt32();
                this.Duration = br.ReadInt32();
                _ = br.AssertInt32(0);

                int timelineCount = br.ReadInt32();
                if (version == MQBVersion.DarkSouls2Scholar) {
                    _ = br.AssertInt32(0);
                }

                long timelinesOffset = br.ReadVarint();
                if (version != MQBVersion.DarkSouls2Scholar) {
                    _ = br.AssertInt64(0);
                }

                var disposesByOffset = new Dictionary<long, Disposition>(disposCount);
                for (int i = 0; i < disposCount; i++) {
                    disposesByOffset[br.Position] = new Disposition(br);
                }

                br.StepIn(timelinesOffset);
                {
                    this.Timelines = new List<Timeline>(timelineCount);
                    for (int i = 0; i < timelineCount; i++) {
                        this.Timelines.Add(new Timeline(br, version, disposesByOffset));
                    }
                }
                br.StepOut();
            }

            internal void Write(BinaryWriterEx bw, MQBVersion version, Dictionary<Disposition, long> offsetsByDispos, int cutIndex, List<CustomData> allCustomData, List<long> customDataValueOffsets) {
                int disposCount = this.Timelines.Sum(g => g.Dispositions.Count);
                bw.WriteFixStrW(this.Name, 0x40, 0x00);
                bw.WriteInt32(disposCount);
                bw.WriteInt32(this.Unk44);
                bw.WriteInt32(this.Duration);
                bw.WriteInt32(0);

                bw.WriteInt32(this.Timelines.Count);
                if (version == MQBVersion.DarkSouls2Scholar) {
                    bw.WriteInt32(0);
                }

                bw.ReserveVarint($"TimelinesOffset{cutIndex}");
                if (version != MQBVersion.DarkSouls2Scholar) {
                    bw.WriteInt64(0);
                }

                foreach (Timeline timeline in this.Timelines) {
                    timeline.WriteDispositions(bw, offsetsByDispos, allCustomData, customDataValueOffsets);
                }
            }

            internal void WriteTimelines(BinaryWriterEx bw, MQBVersion version, int cutIndex) {
                bw.FillVarint($"TimelinesOffset{cutIndex}", bw.Position);
                for (int i = 0; i < this.Timelines.Count; i++) {
                    this.Timelines[i].Write(bw, version, cutIndex, i);
                }
            }

            internal void WriteTimelineCustomData(BinaryWriterEx bw, int cutIndex, List<CustomData> allCustomData, List<long> customDataValueOffsets) {
                for (int i = 0; i < this.Timelines.Count; i++) {
                    this.Timelines[i].WriteCustomData(bw, cutIndex, i, allCustomData, customDataValueOffsets);
                }
            }

            internal void WriteDisposOffsets(BinaryWriterEx bw, Dictionary<Disposition, long> offsetsByDispos, int cutIndex) {
                for (int i = 0; i < this.Timelines.Count; i++) {
                    this.Timelines[i].WriteDisposOffsets(bw, offsetsByDispos, cutIndex, i);
                }
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
