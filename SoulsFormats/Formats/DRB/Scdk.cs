using System.Collections.Generic;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class DRB {
        /// <summary>
        /// Unknown.
        /// </summary>
        public class Scdk {
            /// <summary>
            /// The name of this Scdk.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk04 { get; set; }

            /// <summary>
            /// Unknown; always 1.
            /// </summary>
            public int Unk08 { get; set; }

            /// <summary>
            /// Unknown; always 0-1.
            /// </summary>
            public int Unk0C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk14 { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk18 { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk1C { get; set; }

            /// <summary>
            /// An index into the Anim list.
            /// </summary>
            public int AnimIndex { get; set; }

            /// <summary>
            /// Unknown; always 0-1.
            /// </summary>
            public int Scdp04 { get; set; }

            /// <summary>
            /// Creates a Scdk with default values.
            /// </summary>
            public Scdk() {
                this.Name = "";
                this.Unk08 = 1;
            }

            internal Scdk(BinaryReaderEx br, Dictionary<int, string> strings, long scdpStart) {
                int nameOffset = br.ReadInt32();
                this.Unk04 = br.ReadInt32();
                this.Unk08 = br.ReadInt32();
                this.Unk0C = br.ReadInt32();
                int scdpOffset = br.ReadInt32();
                this.Unk14 = br.ReadInt32();
                this.Unk18 = br.ReadInt32();
                this.Unk1C = br.ReadInt32();

                this.Name = strings[nameOffset];
                br.StepIn(scdpStart + scdpOffset);
                {
                    this.AnimIndex = br.ReadInt32();
                    this.Scdp04 = br.ReadInt32();
                }
                br.StepOut();
            }

            internal void WriteSCDP(BinaryWriterEx bw) {
                bw.WriteInt32(this.AnimIndex);
                bw.WriteInt32(this.Scdp04);
            }

            internal void Write(BinaryWriterEx bw, Dictionary<string, int> stringOffsets, Queue<int> scdpOffsets) {
                bw.WriteInt32(stringOffsets[this.Name]);
                bw.WriteInt32(this.Unk04);
                bw.WriteInt32(this.Unk08);
                bw.WriteInt32(this.Unk0C);
                bw.WriteInt32(scdpOffsets.Dequeue());
                bw.WriteInt32(this.Unk14);
                bw.WriteInt32(this.Unk18);
                bw.WriteInt32(this.Unk1C);
            }

            /// <summary>
            /// Returns the name of this Scdk.
            /// </summary>
            public override string ToString() => $"{this.Name}";
        }
    }
}
