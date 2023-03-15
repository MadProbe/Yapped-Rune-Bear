using System.Collections.Generic;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class DRB {
        /// <summary>
        /// Unknown.
        /// </summary>
        public class Scdl {
            /// <summary>
            /// The name of this Scdl.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Scdos in this Scdl.
            /// </summary>
            public List<Scdo> Scdos { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk0C { get; set; }

            /// <summary>
            /// Creates a Scdl with default values.
            /// </summary>
            public Scdl() {
                this.Name = "";
                this.Scdos = new List<Scdo>();
            }

            internal Scdl(BinaryReaderEx br, Dictionary<int, string> strings, Dictionary<int, Scdo> scdos) {
                int nameOffset = br.ReadInt32();
                int scdoCount = br.ReadInt32();
                int scdoOffset = br.ReadInt32();
                this.Unk0C = br.ReadInt32();

                this.Name = strings[nameOffset];
                this.Scdos = new List<Scdo>(scdoCount);
                for (int i = 0; i < scdoCount; i++) {
                    int offset = scdoOffset + SCDO_SIZE * i;
                    this.Scdos.Add(scdos[offset]);
                    _ = scdos.Remove(offset);
                }
            }

            internal void Write(BinaryWriterEx bw, Dictionary<string, int> stringOffsets, Queue<int> scdoOffsets) {
                bw.WriteInt32(stringOffsets[this.Name]);
                bw.WriteInt32(this.Scdos.Count);
                bw.WriteInt32(scdoOffsets.Dequeue());
                bw.WriteInt32(this.Unk0C);
            }

            /// <summary>
            /// Returns the name and number of Scdos.
            /// </summary>
            public override string ToString() => $"{this.Name}[{this.Scdos.Count}]";
        }
    }
}
