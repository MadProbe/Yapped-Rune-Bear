using System;
using System.Collections.Generic;
using SoulsFormats.Formats.FLVER;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class FLVER2 {
        /// <summary>
        /// A reference to an MTD file, specifying textures to use.
        /// </summary>
        public class Material : IFlverMaterial {
            /// <summary>
            /// Identifies the mesh that uses this material, may include keywords that determine hideable parts.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Virtual path to an MTD file.
            /// </summary>
            public string MTD { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Flags { get; set; }

            /// <summary>
            /// Textures used by this material.
            /// </summary>
            public List<Texture> Textures { get; set; }
            IReadOnlyList<IFlverTexture> IFlverMaterial.Textures => this.Textures;

            /// <summary>
            /// Index to the flver's list of GX lists.
            /// </summary>
            public int GXIndex { get; set; }

            /// <summary>
            /// Unknown; only used in Sekiro.
            /// </summary>
            public int Unk18 { get; set; }

            private int textureIndex, textureCount;

            /// <summary>
            /// Creates a new Material with null or default values.
            /// </summary>
            public Material() {
                this.Name = "";
                this.MTD = "";
                this.Textures = new List<Texture>();
                this.GXIndex = -1;
            }

            /// <summary>
            /// Creates a new Material with the given values and an empty texture list.
            /// </summary>
            public Material(string name, string mtd, int flags) {
                this.Name = name;
                this.MTD = mtd;
                this.Flags = flags;
                this.Textures = new List<Texture>();
                this.GXIndex = -1;
                this.Unk18 = 0;
            }

            internal Material(BinaryReaderEx br, FLVERHeader header, List<GXList> gxLists, Dictionary<int, int> gxListIndices) {
                int nameOffset = br.ReadInt32();
                int mtdOffset = br.ReadInt32();
                this.textureCount = br.ReadInt32();
                this.textureIndex = br.ReadInt32();
                this.Flags = br.ReadInt32();
                int gxOffset = br.ReadInt32();
                this.Unk18 = br.ReadInt32();
                _ = br.AssertInt32(0);

                if (header.Unicode) {
                    this.Name = br.GetUTF16(nameOffset);
                    this.MTD = br.GetUTF16(mtdOffset);
                } else {
                    this.Name = br.GetShiftJIS(nameOffset);
                    this.MTD = br.GetShiftJIS(mtdOffset);
                }

                if (gxOffset == 0) {
                    this.GXIndex = -1;
                } else {
                    if (!gxListIndices.ContainsKey(gxOffset)) {
                        br.StepIn(gxOffset);
                        {
                            gxListIndices[gxOffset] = gxLists.Count;
                            gxLists.Add(new GXList(br, header));
                        }
                        br.StepOut();
                    }
                    this.GXIndex = gxListIndices[gxOffset];
                }
            }

            internal void TakeTextures(Dictionary<int, Texture> textureDict) {
                this.Textures = new List<Texture>(this.textureCount);
                for (int i = this.textureIndex; i < this.textureIndex + this.textureCount; i++) {
                    if (!textureDict.ContainsKey(i)) {
                        throw new NotSupportedException("Texture not found or already taken: " + i);
                    }

                    this.Textures.Add(textureDict[i]);
                    _ = textureDict.Remove(i);
                }

                this.textureIndex = -1;
                this.textureCount = -1;
            }

            internal void Write(BinaryWriterEx bw, int index) {
                bw.ReserveInt32($"MaterialName{index}");
                bw.ReserveInt32($"MaterialMTD{index}");
                bw.WriteInt32(this.Textures.Count);
                bw.ReserveInt32($"TextureIndex{index}");
                bw.WriteInt32(this.Flags);
                bw.ReserveInt32($"GXOffset{index}");
                bw.WriteInt32(this.Unk18);
                bw.WriteInt32(0);
            }

            internal void FillGXOffset(BinaryWriterEx bw, int index, List<int> gxOffsets) {
                if (this.GXIndex == -1) {
                    bw.FillInt32($"GXOffset{index}", 0);
                } else {
                    bw.FillInt32($"GXOffset{index}", gxOffsets[this.GXIndex]);
                }
            }

            internal void WriteTextures(BinaryWriterEx bw, int index, int textureIndex) {
                bw.FillInt32($"TextureIndex{index}", textureIndex);
                for (int i = 0; i < this.Textures.Count; i++) {
                    this.Textures[i].Write(bw, textureIndex + i);
                }
            }

            internal void WriteStrings(BinaryWriterEx bw, FLVERHeader header, int index) {
                bw.FillInt32($"MaterialName{index}", (int)bw.Position);
                if (header.Unicode) {
                    bw.WriteUTF16(this.Name, true);
                } else {
                    bw.WriteShiftJIS(this.Name, true);
                }

                bw.FillInt32($"MaterialMTD{index}", (int)bw.Position);
                if (header.Unicode) {
                    bw.WriteUTF16(this.MTD, true);
                } else {
                    bw.WriteShiftJIS(this.MTD, true);
                }
            }

            /// <summary>
            /// Returns the name and MTD path of the material.
            /// </summary>
            public override string ToString() => $"{this.Name} | {this.MTD}";
        }
    }
}
