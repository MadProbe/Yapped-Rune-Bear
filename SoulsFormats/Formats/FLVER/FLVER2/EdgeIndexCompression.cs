using System.Collections.Generic;
using System.Runtime.InteropServices;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class FLVER2 {
        public partial class FaceSet {
            // This is basically shitcode which makes a lot of dubious assumptions.
            // If I ever intend to implement edge writing this will require much more investigation.
            private static class EdgeIndexCompression {
                [DllImport("EdgeIndexDecompressor.dll")]
                private static extern void DecompressIndexes_C_Standalone(uint numIndexes, byte[] indexes);

                public static List<int> ReadEdgeIndexGroup(BinaryReaderEx br, int indexCount) {
                    long start = br.Position;
                    short memberCount = br.ReadInt16();
                    _ = br.ReadInt16();
                    _ = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    _ = br.ReadInt32();

                    var indices = new List<int>(indexCount);
                    for (int i = 0; i < memberCount; i++) {
                        int dataLength = br.ReadInt32();
                        int dataOffset = br.ReadInt32();
                        _ = br.AssertInt32(0);
                        _ = br.AssertInt32(0);
                        _ = br.ReadInt16();
                        _ = br.ReadInt16();
                        ushort baseIndex = br.ReadUInt16();
                        _ = br.ReadInt16();
                        _ = br.ReadInt32();
                        _ = br.ReadInt32();
                        _ = br.AssertInt32(0);
                        _ = br.AssertInt32(0);
                        _ = br.AssertInt32(0);
                        _ = br.AssertInt32(0);
                        _ = br.ReadInt16();
                        _ = br.ReadInt16();
                        _ = br.ReadInt16();
                        _ = br.ReadInt16();
                        _ = br.ReadInt16();
                        ushort memberIndexCount = br.ReadUInt16();
                        _ = br.AssertInt32(-1);

                        // I feel like one of those fields should be the required buffer size, but none I tried worked
                        byte[] buffer = new byte[memberIndexCount * 32];
                        br.GetBytes(start + dataOffset, buffer, 0, dataLength);
                        DecompressIndexes_C_Standalone(memberIndexCount, buffer);
                        var brBuffer = new BinaryReaderEx(true, buffer);
                        for (int j = 0; j < memberIndexCount; j++) {
                            indices.Add(baseIndex + brBuffer.ReadUInt16());
                        }
                    }
                    return indices;
                }
            }
        }
    }
}
