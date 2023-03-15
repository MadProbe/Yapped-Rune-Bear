using System.Collections.Generic;
using System.ComponentModel;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class DRB {
        /// <summary>
        /// An individual UI element.
        /// </summary>
        public class Dlgo {
            /// <summary>
            /// The name of the element.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// The visual properties of the element.
            /// </summary>
            [Browsable(false)]
            public Shape Shape { get; set; }

            /// <summary>
            /// The behavior of the element.
            /// </summary>
            public Control Control { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk0C { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk10 { get; set; }

            /// <summary>
            /// Unknown; always 0.
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
            /// Creates a Dlgo with default values.
            /// </summary>
            public Dlgo() {
                this.Name = "";
                this.Shape = new Shape.Null();
                this.Control = new Control.Static();
            }

            /// <summary>
            /// Creates a Dlgo with the given values and a default control.
            /// </summary>
            public Dlgo(string name, Shape shape) {
                this.Name = name;
                this.Shape = shape;
                this.Control = shape is Shape.ScrollText ? new Control.ScrollTextDummy() : new Control.Static();
            }

            /// <summary>
            /// Creates a Dlgo with the given values.
            /// </summary>
            public Dlgo(string name, Shape shape, Control control) {
                this.Name = name;
                this.Shape = shape;
                this.Control = control;
            }

            internal Dlgo(BinaryReaderEx br, Dictionary<int, string> strings, Dictionary<int, Shape> shapes, Dictionary<int, Control> controls) {
                int nameOffset = br.ReadInt32();
                int shapOffset = br.ReadInt32();
                int ctrlOffset = br.ReadInt32();
                this.Unk0C = br.ReadInt32();
                this.Unk10 = br.ReadInt32();
                this.Unk14 = br.ReadInt32();
                this.Unk18 = br.ReadInt32();
                this.Unk1C = br.ReadInt32();

                this.Name = strings[nameOffset];
                this.Shape = shapes[shapOffset];
                _ = shapes.Remove(shapOffset);
                this.Control = controls[ctrlOffset];
                _ = controls.Remove(ctrlOffset);
            }

            internal void Write(BinaryWriterEx bw, Dictionary<string, int> stringOffsets, Queue<int> shapOffsets, Queue<int> ctrlOffsets) {
                bw.WriteInt32(stringOffsets[this.Name]);
                bw.WriteInt32(shapOffsets.Dequeue());
                bw.WriteInt32(ctrlOffsets.Dequeue());
                bw.WriteInt32(this.Unk0C);
                bw.WriteInt32(this.Unk10);
                bw.WriteInt32(this.Unk14);
                bw.WriteInt32(this.Unk18);
                bw.WriteInt32(this.Unk1C);
            }

            /// <summary>
            /// Returns the name, shape type, and control type of the element.
            /// </summary>
            public override string ToString() => $"{this.Name} ({this.Control.Type} {this.Shape.Type})";
        }
    }
}
