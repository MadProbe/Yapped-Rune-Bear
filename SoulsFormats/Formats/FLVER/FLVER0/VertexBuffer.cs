using System.Collections.Generic;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class FLVER0 {
        private class VertexBuffer {
            public int LayoutIndex;

            public int BufferLength;

            public int BufferOffset;

            public VertexBuffer() { }

            internal VertexBuffer(BinaryReaderEx br) {
                this.LayoutIndex = br.ReadInt32();
                this.BufferLength = br.ReadInt32();
                this.BufferOffset = br.ReadInt32();
                _ = br.AssertInt32(0);
            }

            internal static List<VertexBuffer> ReadVertexBuffers(BinaryReaderEx br) {
                int bufferCount = br.ReadInt32();
                int buffersOffset = br.ReadInt32();
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);

                var buffers = new List<VertexBuffer>(bufferCount);
                br.StepIn(buffersOffset);
                {
                    for (int i = 0; i < bufferCount; i++) {
                        buffers.Add(new VertexBuffer(br));
                    }
                }
                br.StepOut();
                return buffers;
            }
        }
    }
}
