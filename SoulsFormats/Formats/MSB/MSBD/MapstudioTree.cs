using System.Collections.Generic;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MSBD {
        private class MapstudioTree : Param<Tree> {
            internal override string Name => "MAPSTUDIO_TREE_ST";

            public List<Tree> Trees { get; set; }

            public MapstudioTree() => this.Trees = new List<Tree>();

            internal override Tree ReadEntry(BinaryReaderEx br) => this.Trees.EchoAdd(new Tree(br));

            public override List<Tree> GetEntries() => this.Trees;
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        public class Tree : Entry {
            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk00 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk04 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk08 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk0C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk10 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk14 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk18 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk1C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk20 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk24 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk28 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk2C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public float Unk30 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<short> UnkShorts { get; set; }

            /// <summary>
            /// Creates a Tree with default values.
            /// </summary>
            public Tree() => this.UnkShorts = new List<short>();

            /// <summary>
            /// Creates a deep copy of the tree.
            /// </summary>
            public Tree DeepCopy() {
                var tree = (Tree)this.MemberwiseClone();
                tree.UnkShorts = new List<short>(this.UnkShorts);
                return tree;
            }

            internal Tree(BinaryReaderEx br) {
                this.Unk00 = br.ReadSingle();
                this.Unk04 = br.ReadSingle();
                this.Unk08 = br.ReadSingle();
                this.Unk0C = br.ReadInt32();
                this.Unk10 = br.ReadSingle();
                this.Unk14 = br.ReadSingle();
                this.Unk18 = br.ReadSingle();
                this.Unk1C = br.ReadInt32();
                this.Unk20 = br.ReadSingle();
                this.Unk24 = br.ReadSingle();
                this.Unk28 = br.ReadSingle();
                this.Unk2C = br.ReadInt32();
                this.Unk30 = br.ReadSingle();
                int shortCount = br.ReadInt32();
                this.UnkShorts = new List<short>(br.ReadInt16s(shortCount));
            }

            internal override void Write(BinaryWriterEx bw, int id) {
                bw.WriteSingle(this.Unk00);
                bw.WriteSingle(this.Unk04);
                bw.WriteSingle(this.Unk08);
                bw.WriteInt32(this.Unk0C);
                bw.WriteSingle(this.Unk10);
                bw.WriteSingle(this.Unk14);
                bw.WriteSingle(this.Unk18);
                bw.WriteInt32(this.Unk1C);
                bw.WriteSingle(this.Unk20);
                bw.WriteSingle(this.Unk24);
                bw.WriteSingle(this.Unk28);
                bw.WriteInt32(this.Unk2C);
                bw.WriteSingle(this.Unk30);
                bw.WriteInt32(this.UnkShorts.Count);
                bw.WriteInt16s(this.UnkShorts);
                bw.Pad(0x10);
            }
        }
    }
}
