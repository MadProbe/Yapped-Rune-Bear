using System.Collections.Generic;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MSB2 {
        private class MapstudioBoneName : Param<BoneName> {
            internal override int Version => 0;
            internal override string Name => "MAPSTUDIO_BONE_NAME_STRING";

            public List<BoneName> BoneNames { get; set; }

            public MapstudioBoneName() => this.BoneNames = new List<BoneName>();

            internal override BoneName ReadEntry(BinaryReaderEx br) => this.BoneNames.EchoAdd(new BoneName(br));

            public override List<BoneName> GetEntries() => this.BoneNames;
        }

        internal class BoneName : NamedEntry {
            public BoneName() => this.Name = "Master";

            public BoneName DeepCopy() => (BoneName)this.MemberwiseClone();

            internal BoneName(BinaryReaderEx br) => this.Name = br.ReadUTF16();

            internal override void Write(BinaryWriterEx bw, int index) => bw.WriteUTF16(MSB.ReambiguateName(this.Name), true);
        }
    }
}
