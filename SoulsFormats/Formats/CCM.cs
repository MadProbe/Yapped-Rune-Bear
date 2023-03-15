using System;
using System.Collections.Generic;
using System.Numerics;
using SoulsFormats.Util;

namespace SoulsFormats.Formats {
    /// <summary>
    /// A font layout file used in DeS, DS1, DS2, and DS3; determines the texture used for each different character code.
    /// </summary>
    public class CCM : SoulsFile<CCM> {
        /// <summary>
        /// Indicates the game this CCM should be formatted for.
        /// </summary>
        public CCMVer Version { get; set; }

        /// <summary>
        /// Maximum width of a glyph; glyph widths are relative to this.
        /// </summary>
        public short FullWidth { get; set; }

        /// <summary>
        /// Width of the font textures.
        /// </summary>
        public short TexWidth { get; set; }

        /// <summary>
        /// Height of the font textures.
        /// </summary>
        public short TexHeight { get; set; }

        /// <summary>
        /// Unknown; only meaningful in DeS/DS1, always 0 or 0x20.
        /// </summary>
        public short Unk0E { get; set; }

        /// <summary>
        /// Unknown; always 1 in DeS, either 1 or 4 in DS1, and 4 in DS2.
        /// </summary>
        public byte Unk1C { get; set; }

        /// <summary>
        /// Unknown; always 1 in DeS and 0 in DS1/DS2.
        /// </summary>
        public byte Unk1D { get; set; }

        /// <summary>
        /// Number of separate font textures.
        /// </summary>
        public byte TexCount { get; set; }

        /// <summary>
        /// Individual characters in this font, keyed by their code.
        /// </summary>
        public Dictionary<int, Glyph> Glyphs { get; set; }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            br.BigEndian = false;
            this.Version = br.ReadEnum32<CCMVer>();
            if (this.Version == CCMVer.DemonsSouls) {
                br.BigEndian = true;
            }

            _ = br.ReadInt32();
            this.FullWidth = br.ReadInt16();
            this.TexWidth = br.ReadInt16();
            this.TexHeight = br.ReadInt16();

            short codeGroupCount, texRegionCount, glyphCount;
            if (this.Version is CCMVer.DemonsSouls or CCMVer.DarkSouls1) {
                this.Unk0E = br.ReadInt16();
                codeGroupCount = br.ReadInt16();
                texRegionCount = -1;
                glyphCount = br.ReadInt16();
            } else {
                this.Unk0E = 0;
                codeGroupCount = -1;
                texRegionCount = br.ReadInt16();
                glyphCount = br.ReadInt16();
                _ = br.AssertInt16(0);
            }

            _ = br.AssertInt32(0x20);
            _ = br.ReadInt32();
            this.Unk1C = br.ReadByte();
            this.Unk1D = br.ReadByte();
            this.TexCount = br.ReadByte();
            _ = br.AssertByte(0);

            this.Glyphs = new Dictionary<int, Glyph>(glyphCount);
            if (this.Version is CCMVer.DemonsSouls or CCMVer.DarkSouls1) {
                var codeGroups = new List<CodeGroup>(codeGroupCount);
                for (int i = 0; i < codeGroupCount; i++) {
                    codeGroups.Add(new CodeGroup(br));
                }

                var glyphs = new List<Glyph>(glyphCount);
                for (int i = 0; i < glyphCount; i++) {
                    Vector2 uv1 = br.ReadVector2();
                    Vector2 uv2 = br.ReadVector2();
                    short preSpace = br.ReadInt16();
                    short width = br.ReadInt16();
                    short advance = br.ReadInt16();
                    short texIndex = br.ReadInt16();

                    glyphs.Add(new Glyph(uv1, uv2, preSpace, width, advance, texIndex));
                }

                foreach (CodeGroup group in codeGroups) {
                    int codeCount = group.EndCode - group.StartCode + 1;
                    for (int i = 0; i < codeCount; i++) {
                        this.Glyphs[group.StartCode + i] = glyphs[group.GlyphIndex + i];
                    }
                }
            } else if (this.Version == CCMVer.DarkSouls2) {
                var texRegions = new Dictionary<int, TexRegion>(texRegionCount);
                for (int i = 0; i < texRegionCount; i++) {
                    texRegions[(int)br.Position] = new TexRegion(br);
                }

                for (int i = 0; i < glyphCount; i++) {
                    int code = br.ReadInt32();
                    int texRegionOffset = br.ReadInt32();
                    short texIndex = br.ReadInt16();
                    short preSpace = br.ReadInt16();
                    short width = br.ReadInt16();
                    short advance = br.ReadInt16();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);

                    TexRegion texRegion = texRegions[texRegionOffset];
                    var uv1 = new Vector2(texRegion.X1 / (float)this.TexWidth, texRegion.Y1 / (float)this.TexHeight);
                    var uv2 = new Vector2(texRegion.X2 / (float)this.TexWidth, texRegion.Y2 / (float)this.TexHeight);
                    this.Glyphs[code] = new Glyph(uv1, uv2, preSpace, width, advance, texIndex);
                }
            }
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected internal override void Write(BinaryWriterEx bw) {
            bw.BigEndian = false;
            bw.WriteUInt32((uint)this.Version);
            bw.BigEndian = this.Version == CCMVer.DemonsSouls;

            bw.ReserveInt32("FileSize");
            bw.WriteInt16(this.FullWidth);
            bw.WriteInt16(this.TexWidth);
            bw.WriteInt16(this.TexHeight);

            if (this.Version is CCMVer.DemonsSouls or CCMVer.DarkSouls1) {
                bw.WriteInt16(this.Unk0E);
                bw.ReserveInt16("CodeGroupCount");
                bw.WriteInt16((short)this.Glyphs.Count);
            } else if (this.Version == CCMVer.DarkSouls2) {
                bw.ReserveInt16("TexRegionCount");
                bw.WriteInt16((short)this.Glyphs.Count);
                bw.WriteInt16(0);
            }

            bw.WriteInt32(0x20);
            bw.ReserveInt32("GlyphOffset");
            bw.WriteByte(this.Unk1C);
            bw.WriteByte(this.Unk1D);
            bw.WriteByte(this.TexCount);
            bw.WriteByte(0);

            var codes = new List<int>(this.Glyphs.Keys);
            codes.Sort();
            if (this.Version is CCMVer.DemonsSouls or CCMVer.DarkSouls1) {
                var codeGroups = new List<CodeGroup>();
                for (int i = 0; i < this.Glyphs.Count;) {
                    int startCode = codes[i];
                    int glyphIndex = i;
                    for (i++; i < this.Glyphs.Count && codes[i] == codes[i - 1] + 1; i++) {
                        ;
                    }

                    int endCode = codes[i - 1];
                    codeGroups.Add(new CodeGroup(startCode, endCode, glyphIndex));
                }

                bw.FillInt16("CodeGroupCount", (short)codeGroups.Count);
                foreach (CodeGroup group in codeGroups) {
                    group.Write(bw);
                }

                bw.FillInt32("GlyphOffset", (int)bw.Position);
                foreach (int code in codes) {
                    Glyph glyph = this.Glyphs[code];
                    bw.WriteVector2(glyph.UV1);
                    bw.WriteVector2(glyph.UV2);
                    bw.WriteInt16(glyph.PreSpace);
                    bw.WriteInt16(glyph.Width);
                    bw.WriteInt16(glyph.Advance);
                    bw.WriteInt16(glyph.TexIndex);
                }
            } else if (this.Version == CCMVer.DarkSouls2) {
                var texRegionsByCode = new Dictionary<int, TexRegion>(this.Glyphs.Count);
                var texRegions = new HashSet<TexRegion>();
                foreach (int code in codes) {
                    Glyph glyph = this.Glyphs[code];
                    short x1 = (short)Math.Round(glyph.UV1.X * this.TexWidth);
                    short y1 = (short)Math.Round(glyph.UV1.Y * this.TexHeight);
                    short x2 = (short)Math.Round(glyph.UV2.X * this.TexWidth);
                    short y2 = (short)Math.Round(glyph.UV2.Y * this.TexHeight);
                    var region = new TexRegion(x1, y1, x2, y2);
                    texRegionsByCode[code] = region;
                    _ = texRegions.Add(region);
                }

                bw.FillInt16("TexRegionCount", (short)texRegions.Count);
                var texRegionOffsets = new Dictionary<TexRegion, int>(texRegions.Count);
                foreach (TexRegion region in texRegions) {
                    texRegionOffsets[region] = (int)bw.Position;
                    region.Write(bw);
                }

                bw.FillInt32("GlyphOffset", (int)bw.Position);
                foreach (int code in codes) {
                    Glyph glyph = this.Glyphs[code];
                    bw.WriteInt32(code);
                    bw.WriteInt32(texRegionOffsets[texRegionsByCode[code]]);
                    bw.WriteInt16(glyph.TexIndex);
                    bw.WriteInt16(glyph.PreSpace);
                    bw.WriteInt16(glyph.Width);
                    bw.WriteInt16(glyph.Advance);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            bw.FillInt32("FileSize", (int)bw.Position);
        }

        // This is stupid because it's really two shorts but I am lazy
        /// <summary>
        /// Which game the CCM should be formatted for.
        /// </summary>
        public enum CCMVer : uint {
            /// <summary>
            /// Demon's Souls
            /// </summary>
            DemonsSouls = 0x100,

            /// <summary>
            /// Dark Souls 1
            /// </summary>
            DarkSouls1 = 0x10001,

            /// <summary>
            /// Dark Souls 2 and Dark Souls 3
            /// </summary>
            DarkSouls2 = 0x20000,
        }

        /// <summary>
        /// An individual character in the font.
        /// </summary>
        public class Glyph {
            /// <summary>
            /// The UV of the top-left corner of the texture.
            /// </summary>
            public Vector2 UV1 { get; set; }

            /// <summary>
            /// The UV of the bottom-right corner of the texture.
            /// </summary>
            public Vector2 UV2 { get; set; }

            /// <summary>
            /// Padding before the character.
            /// </summary>
            public short PreSpace { get; set; }

            /// <summary>
            /// Width of the character texture.
            /// </summary>
            public short Width { get; set; }

            /// <summary>
            /// Distance to the next character.
            /// </summary>
            public short Advance { get; set; }

            /// <summary>
            /// Index of the font texture with this character.
            /// </summary>
            public short TexIndex { get; set; }

            /// <summary>
            /// Creates a new Glyph with the given values.
            /// </summary>
            public Glyph(Vector2 uv1, Vector2 uv2, short preSpace, short width, short advance, short texIndex) {
                this.UV1 = uv1;
                this.UV2 = uv2;
                this.PreSpace = preSpace;
                this.Width = width;
                this.Advance = advance;
                this.TexIndex = texIndex;
            }
        }

        private struct CodeGroup {
            public int StartCode, EndCode;
            public int GlyphIndex;

            public CodeGroup(BinaryReaderEx br) {
                this.StartCode = br.ReadInt32();
                this.EndCode = br.ReadInt32();
                this.GlyphIndex = br.ReadInt32();
            }

            public CodeGroup(int startCode, int endCode, int glyphIndex) {
                this.StartCode = startCode;
                this.EndCode = endCode;
                this.GlyphIndex = glyphIndex;
            }

            public void Write(BinaryWriterEx bw) {
                bw.WriteInt32(this.StartCode);
                bw.WriteInt32(this.EndCode);
                bw.WriteInt32(this.GlyphIndex);
            }
        }

        private struct TexRegion : IEquatable<TexRegion> {
            public short X1, Y1;
            public short X2, Y2;

            public TexRegion(BinaryReaderEx br) {
                this.X1 = br.ReadInt16();
                this.Y1 = br.ReadInt16();
                this.X2 = br.ReadInt16();
                this.Y2 = br.ReadInt16();
            }

            public TexRegion(short x1, short y1, short x2, short y2) {
                this.X1 = x1;
                this.Y1 = y1;
                this.X2 = x2;
                this.Y2 = y2;
            }

            public void Write(BinaryWriterEx bw) {
                bw.WriteInt16(this.X1);
                bw.WriteInt16(this.Y1);
                bw.WriteInt16(this.X2);
                bw.WriteInt16(this.Y2);
            }

            public override bool Equals(object obj) => obj is TexRegion && this.Equals((TexRegion)obj);

            public bool Equals(TexRegion other) => this.X1 == other.X1 &&
                       this.Y1 == other.Y1 &&
                       this.X2 == other.X2 &&
                       this.Y2 == other.Y2;

            public override int GetHashCode() {
                int hashCode = 268039418;
                hashCode = hashCode * -1521134295 + this.X1.GetHashCode();
                hashCode = hashCode * -1521134295 + this.Y1.GetHashCode();
                hashCode = hashCode * -1521134295 + this.X2.GetHashCode();
                hashCode = hashCode * -1521134295 + this.Y2.GetHashCode();
                return hashCode;
            }
        }
    }
}
