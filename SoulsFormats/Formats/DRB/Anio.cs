using System.Collections.Generic;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class DRB {
        /// <summary>
        /// Unknown.
        /// </summary>
        public class Anio {
            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk00 { get; set; }

            /// <summary>
            /// Aniks in this Anio.
            /// </summary>
            public List<Anik> Aniks { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk0C { get; set; }

            /// <summary>
            /// Creates an Anio with default values.
            /// </summary>
            public Anio() => this.Aniks = new List<Anik>();

            internal Anio(BinaryReaderEx br, Dictionary<int, Anik> aniks) {
                this.Unk00 = br.ReadInt32();
                int anikCount = br.ReadInt32();
                int anikOffset = br.ReadInt32();
                this.Unk0C = br.ReadInt32();

                this.Aniks = new List<Anik>(anikCount);
                for (int i = 0; i < anikCount; i++) {
                    int offset = anikOffset + ANIK_SIZE * i;
                    this.Aniks.Add(aniks[offset]);
                    _ = aniks.Remove(offset);
                }
            }

            internal void Write(BinaryWriterEx bw, Queue<int> anikOffsets) {
                bw.WriteInt32(this.Unk00);
                bw.WriteInt32(this.Aniks.Count);
                bw.WriteInt32(anikOffsets.Dequeue());
                bw.WriteInt32(this.Unk0C);
            }

            /// <summary>
            /// Returns the number of Aniks in this Anio.
            /// </summary>
            public override string ToString() => $"Anio[{this.Aniks.Count}]";
        }
    }
}
