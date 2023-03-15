using System.Collections.Generic;
using SoulsFormats.Util;

namespace SoulsFormats.Formats.Other.KF4 {
    /// <summary>
    /// A map asset container used in King's Field IV. Extension: .map
    /// </summary>
    public class MAP : SoulsFile<MAP> {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public List<Struct4> Struct4s { get; set; }

        protected internal override void Read(BinaryReaderEx br) {
            _ = br.ReadInt32(); // File size
            _ = br.ReadInt32();
            _ = br.ReadInt32();
            _ = br.ReadInt32();
            int offset4 = br.ReadInt32();
            _ = br.ReadInt32();
            _ = br.ReadInt32();
            _ = br.AssertInt32(0);
            _ = br.ReadInt32();
            _ = br.ReadInt32();
            _ = br.ReadInt32();
            _ = br.ReadInt16();
            _ = br.ReadInt16();
            _ = br.ReadInt16();
            short count4 = br.ReadInt16();
            _ = br.ReadInt16();
            _ = br.ReadInt16();
            _ = br.AssertInt16(0);
            _ = br.ReadInt16();
            _ = br.ReadInt16();
            _ = br.ReadInt16();

            br.Position = offset4;
            this.Struct4s = new List<Struct4>(count4);
            for (int i = 0; i < count4; i++) {
                this.Struct4s.Add(new Struct4(br));
            }
        }

        public class Struct4 {
            public OM2 Om2 { get; set; }

            internal Struct4(BinaryReaderEx br) {
                byte[] om2Bytes = br.ReadBytes(br.GetInt32(br.Position));
                _ = br.ReadBytes(br.GetInt32(br.Position));

                this.Om2 = OM2.Read(om2Bytes);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
