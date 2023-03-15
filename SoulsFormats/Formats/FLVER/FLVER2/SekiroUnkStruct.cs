using System.Collections.Generic;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class FLVER2 {
        /// <summary>
        /// Unknown; only present in Sekiro.
        /// </summary>
        public class SekiroUnkStruct {
            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Member> Members1 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Member> Members2 { get; set; }

            /// <summary>
            /// Creates an empty SekiroUnkStruct.
            /// </summary>
            public SekiroUnkStruct() {
                this.Members1 = new List<Member>();
                this.Members2 = new List<Member>();
            }

            internal SekiroUnkStruct(BinaryReaderEx br) {
                short count1 = br.ReadInt16();
                short count2 = br.ReadInt16();
                uint offset1 = br.ReadUInt32();
                uint offset2 = br.ReadUInt32();
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);

                br.StepIn(offset1);
                {
                    this.Members1 = new List<Member>(count1);
                    for (int i = 0; i < count1; i++) {
                        this.Members1.Add(new Member(br));
                    }
                }
                br.StepOut();

                br.StepIn(offset2);
                {
                    this.Members2 = new List<Member>(count2);
                    for (int i = 0; i < count2; i++) {
                        this.Members2.Add(new Member(br));
                    }
                }
                br.StepOut();
            }

            internal void Write(BinaryWriterEx bw) {
                bw.WriteInt16((short)this.Members1.Count);
                bw.WriteInt16((short)this.Members2.Count);
                bw.ReserveUInt32("SekiroUnkOffset1");
                bw.ReserveUInt32("SekiroUnkOffset2");
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);

                bw.FillUInt32("SekiroUnkOffset1", (uint)bw.Position);
                foreach (Member member in this.Members1) {
                    member.Write(bw);
                }

                bw.FillUInt32("SekiroUnkOffset2", (uint)bw.Position);
                foreach (Member member in this.Members2) {
                    member.Write(bw);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Member {
                /// <summary>
                /// Unknown; maybe bone indices? Length 4.
                /// </summary>
                public short[] Unk00 { get; private set; }

                /// <summary>
                /// Unknown; seems to just count up from 0.
                /// </summary>
                public int Index { get; set; }

                /// <summary>
                /// Creates a Member with default values.
                /// </summary>
                public Member() => this.Unk00 = new short[4];

                internal Member(BinaryReaderEx br) {
                    this.Unk00 = br.ReadInt16s(4);
                    this.Index = br.ReadInt32();
                    _ = br.AssertInt32(0);
                }

                internal void Write(BinaryWriterEx bw) {
                    bw.WriteInt16s(this.Unk00);
                    bw.WriteInt32(this.Index);
                    bw.WriteInt32(0);
                }
            }
        }
    }
}
