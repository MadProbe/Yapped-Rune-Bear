using SoulsFormats.Formats.FLVER;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class FLVER0 {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class Texture : IFlverTexture {
            public string Type { get; set; }

            public string Path { get; set; }

            internal Texture(BinaryReaderEx br, FLVER0 flv) {
                int pathOffset = br.ReadInt32();
                int typeOffset = br.ReadInt32();
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);

                this.Path = flv.Unicode ? br.GetUTF16(pathOffset) : br.GetShiftJIS(pathOffset);
                this.Type = typeOffset > 0 ? flv.Unicode ? br.GetUTF16(typeOffset) : br.GetShiftJIS(typeOffset) : null;
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
