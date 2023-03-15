using System.Collections.Generic;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MSB3 {
        /// <summary>
        /// A list of names to be referenced by parts pose bones.
        /// </summary>
        private class MapstudioBoneName : Param<BoneName> {
            internal override int Version => 0;
            internal override string Type => "MAPSTUDIO_BONE_NAME_STRING";

            /// <summary>
            /// All available names.
            /// </summary>
            public List<BoneName> Names { get; set; }

            /// <summary>
            /// Creates an empty MapstudioBoneName.
            /// </summary>
            public MapstudioBoneName() => this.Names = new List<BoneName>();

            /// <summary>
            /// Returns every bone name in the order they will be written.
            /// </summary>
            public override List<BoneName> GetEntries() => this.Names;

            internal override BoneName ReadEntry(BinaryReaderEx br) => this.Names.EchoAdd(new BoneName(br));
        }

        /// <summary>
        /// A single string for naming a bone.
        /// </summary>
        internal class BoneName : NamedEntry {
            /// <summary>
            /// The name of a bone.
            /// </summary>
            public override string Name { get; set; }

            /// <summary>
            /// Creates a BoneName with default values.
            /// </summary>
            public BoneName() => this.Name = "Master";

            /// <summary>
            /// Creates a deep copy of the bone name.
            /// </summary>
            public BoneName DeepCopy() => (BoneName)this.MemberwiseClone();

            internal BoneName(BinaryReaderEx br) => this.Name = br.ReadUTF16();

            internal override void Write(BinaryWriterEx bw, int id) {
                bw.WriteUTF16(this.Name, true);
                bw.Pad(8);
            }
        }
    }
}
