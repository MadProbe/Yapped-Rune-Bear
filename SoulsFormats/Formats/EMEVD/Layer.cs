using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class EMEVD {
        private static class Layer {
            public static uint Read(BinaryReaderEx br) {
                _ = br.AssertInt32(2);
                uint layer = br.ReadUInt32();
                _ = br.AssertVarint(0);
                _ = br.AssertVarint(-1);
                _ = br.AssertVarint(1);
                return layer;
            }

            public static void Write(BinaryWriterEx bw, uint layer) {
                bw.WriteInt32(2);
                bw.WriteUInt32(layer);
                bw.WriteVarint(0);
                bw.WriteVarint(-1);
                bw.WriteVarint(1);
            }
        }
    }
}
