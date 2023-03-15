using System;
using System.Collections.Generic;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class FLVER2 {
        /// <summary>
        /// A collection of items that set various material properties.
        /// </summary>
        public class GXList : List<GXItem> {
            /// <summary>
            /// Value indicating the terminating item; typically int.MaxValue, sometimes -1.
            /// </summary>
            public int TerminatorID { get; set; }

            /// <summary>
            /// The length in bytes of the terminator data block; most likely not important, but varies in original files.
            /// </summary>
            public int TerminatorLength { get; set; }

            /// <summary>
            /// Creates an empty GXList.
            /// </summary>
            public GXList() : base() => this.TerminatorID = int.MaxValue;

            internal GXList(BinaryReaderEx br, FLVERHeader header) : base() {
                if (header.Version < 0x20010) {
                    this.Add(new GXItem(br, header));
                } else {
                    int id;
                    while ((id = br.GetInt32(br.Position)) != int.MaxValue && id != -1) {
                        this.Add(new GXItem(br, header));
                    }

                    this.TerminatorID = br.AssertInt32(id);
                    _ = br.AssertInt32(100);
                    this.TerminatorLength = br.ReadInt32() - 0xC;
                    br.AssertPattern(this.TerminatorLength, 0x00);
                }
            }

            internal void Write(BinaryWriterEx bw, FLVERHeader header) {
                if (header.Version < 0x20010) {
                    this[0].Write(bw, header);
                } else {
                    foreach (GXItem item in this) {
                        item.Write(bw, header);
                    }

                    bw.WriteInt32(this.TerminatorID);
                    bw.WriteInt32(100);
                    bw.WriteInt32(this.TerminatorLength + 0xC);
                    bw.WritePattern(this.TerminatorLength, 0x00);
                }
            }
        }

        /// <summary>
        /// Rendering parameters used by materials.
        /// </summary>
        public class GXItem {
            /// <summary>
            /// In DS2, ID is just a number; in other games, it's 4 ASCII characters.
            /// </summary>
            public string ID { get; set; }

            /// <summary>
            /// Unknown; typically 100.
            /// </summary>
            public int Unk04 { get; set; }

            /// <summary>
            /// Raw parameter data, usually just a bunch of floats.
            /// </summary>
            public byte[] Data { get; set; }

            /// <summary>
            /// Creates a GXItem with default values.
            /// </summary>
            public GXItem() {
                this.ID = "0";
                this.Unk04 = 100;
                this.Data = new byte[0];
            }

            /// <summary>
            /// Creates a GXItem with the given values.
            /// </summary>
            public GXItem(string id, int unk04, byte[] data) {
                this.ID = id;
                this.Unk04 = unk04;
                this.Data = data;
            }

            internal GXItem(BinaryReaderEx br, FLVERHeader header) {
                this.ID = header.Version <= 0x20010 ? br.ReadInt32().ToString() : br.ReadFixStr(4);
                this.Unk04 = br.ReadInt32();
                int length = br.ReadInt32();
                this.Data = br.ReadBytes(length - 0xC);
            }

            internal void Write(BinaryWriterEx bw, FLVERHeader header) {
                if (header.Version <= 0x20010) {
                    if (int.TryParse(this.ID, out int id)) {
                        bw.WriteInt32(id);
                    } else {
                        throw new FormatException("For Dark Souls 2, GX IDs must be convertible to int.");
                    }
                } else {
                    bw.WriteFixStr(this.ID, 4);
                }
                bw.WriteInt32(this.Unk04);
                bw.WriteInt32(this.Data.Length + 0xC);
                bw.WriteBytes(this.Data);
            }
        }
    }
}
