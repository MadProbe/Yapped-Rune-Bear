using SoulsFormats.Util;

namespace SoulsFormats.Formats.Other.KF4 {
    /// <summary>
    /// Container for character files in King's Field IV. Extension: .chr
    /// </summary>
    public class CHR : SoulsFile<CHR> {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int Unk00 { get; set; }

        public OM2 Om2 { get; set; }

        protected internal override void Read(BinaryReaderEx br) {
            this.Unk00 = br.ReadInt32();
            int om2Offset = br.ReadInt32();
            _ = br.ReadInt32();
            _ = br.ReadInt32();
            _ = br.ReadInt32();
            _ = br.ReadInt32();
            _ = br.ReadInt32();

            if (om2Offset != 0) {
                br.Position = om2Offset;
                byte[] om2Bytes = br.ReadBytes(br.GetInt32(br.Position));
                this.Om2 = OM2.Read(om2Bytes);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
