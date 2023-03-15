using System.Collections.Generic;
using System.ComponentModel;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class DRB {
        /// <summary>
        /// A group of UI elements, which is itself a UI element.
        /// </summary>
        public class Dlg : Dlgo {
            /// <summary>
            /// The child elements attached to this group.
            /// </summary>
            [Browsable(false)]
            public List<Dlgo> Dlgos { get; set; }

            /// <summary>
            /// Left edge of the group.
            /// </summary>
            public short LeftEdge { get; set; }

            /// <summary>
            /// Top edge of the group.
            /// </summary>
            public short TopEdge { get; set; }

            /// <summary>
            /// Right edge of the group.
            /// </summary>
            public short RightEdge { get; set; }

            /// <summary>
            /// Bottom edge of the group.
            /// </summary>
            public short BottomEdge { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public short[] Unk30 { get; private set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public short Unk3A { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk3C { get; set; }

            /// <summary>
            /// Creates a Dlg with default values.
            /// </summary>
            public Dlg() : base() {
                this.Dlgos = new List<Dlgo>();
                this.Unk30 = new short[5] { -1, -1, -1, -1, -1 };
            }

            /// <summary>
            /// Creates a Dlg with the given values.
            /// </summary>
            public Dlg(string name, Shape shape, Control control) : base(name, shape, control) {
                this.Dlgos = new List<Dlgo>();
                this.Unk30 = new short[5] { -1, -1, -1, -1, -1 };
            }

            internal Dlg(BinaryReaderEx br, Dictionary<int, string> strings, Dictionary<int, Shape> shapes, Dictionary<int, Control> controls, Dictionary<int, Dlgo> dlgos) : base(br, strings, shapes, controls) {
                int dlgoCount = br.ReadInt32();
                int dlgoOffset = br.ReadInt32();
                this.LeftEdge = br.ReadInt16();
                this.TopEdge = br.ReadInt16();
                this.RightEdge = br.ReadInt16();
                this.BottomEdge = br.ReadInt16();
                this.Unk30 = br.ReadInt16s(5);
                this.Unk3A = br.ReadInt16();
                this.Unk3C = br.ReadInt32();

                this.Dlgos = new List<Dlgo>(dlgoCount);
                for (int i = 0; i < dlgoCount; i++) {
                    int offset = dlgoOffset + DLGO_SIZE * i;
                    this.Dlgos.Add(dlgos[offset]);
                    _ = dlgos.Remove(offset);
                }
            }

            internal void Write(BinaryWriterEx bw, Dictionary<string, int> stringOffsets, Queue<int> shapOffsets, Queue<int> ctrlOffsets, Queue<int> dlgoOffsets) {
                this.Write(bw, stringOffsets, shapOffsets, ctrlOffsets);
                bw.WriteInt32(this.Dlgos.Count);
                bw.WriteInt32(dlgoOffsets.Dequeue());
                bw.WriteInt16(this.LeftEdge);
                bw.WriteInt16(this.TopEdge);
                bw.WriteInt16(this.RightEdge);
                bw.WriteInt16(this.BottomEdge);
                bw.WriteInt16s(this.Unk30);
                bw.WriteInt16(this.Unk3A);
                bw.WriteInt32(this.Unk3C);
            }

            /// <summary>
            /// Returns the child element with the given name, or null if not found.
            /// </summary>
            public Dlgo this[string name] => this.Dlgos.Find(dlgo => dlgo.Name == name);

            /// <summary>
            /// Returns the name, number of child elements, shape type, and control type of this group.
            /// </summary>
            public override string ToString() => $"{this.Name} ({this.Control.Type} {this.Shape.Type} [{this.Dlgos.Count}])";
        }
    }
}
