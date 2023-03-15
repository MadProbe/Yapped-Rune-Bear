using System;

namespace SoulsFormats {
    public partial class FLVER {
        /// <summary>
        /// Four indices of bones to bind a vertex to, accessed like an array. Unused bones should be set to 0.
        /// </summary>
        public struct VertexBoneIndices {
            private int A, B, C, D;

            /// <summary>
            /// Length of bone indices is always 4.
            /// </summary>
            public int Length => 4;

            /// <summary>
            /// Accesses bone indices as an int[4].
            /// </summary>
            public int this[int i] {
                get => i switch {
                    0 => this.A,
                    1 => this.B,
                    2 => this.C,
                    3 => this.D,
                    _ => throw new IndexOutOfRangeException($"Index ({i}) was out of range. Must be non-negative and less than 4."),
                };

                set {
                    switch (i) {
                        case 0: this.A = value; break;
                        case 1: this.B = value; break;
                        case 2: this.C = value; break;
                        case 3: this.D = value; break;
                        default:
                            throw new IndexOutOfRangeException($"Index ({i}) was out of range. Must be non-negative and less than 4.");
                    }
                }
            }
        }
    }
}
