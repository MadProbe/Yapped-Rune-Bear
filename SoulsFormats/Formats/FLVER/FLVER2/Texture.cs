using System.Numerics;
using SoulsFormats.Formats.FLVER;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class FLVER2 {
        /// <summary>
        /// A texture used by the shader specified in an MTD.
        /// </summary>
        public class Texture : IFlverTexture {
            /// <summary>
            /// The type of texture this is, corresponding to the entries in the MTD.
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// Network path to the texture file to use.
            /// </summary>
            public string Path { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public Vector2 Scale { get; set; }

            /// <summary>
            /// Unknown; observed values 0, 1, 2.
            /// </summary>
            public byte Unk10 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool Unk11 { get; set; }

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
            public float Unk1C { get; set; }

            /// <summary>
            /// Creates a Texture with default values.
            /// </summary>
            public Texture() {
                this.Type = "";
                this.Path = "";
                this.Scale = Vector2.One;
            }

            /// <summary>
            /// Creates a new Texture with the specified values.
            /// </summary>
            public Texture(string type, string path, Vector2 scale, byte unk10, bool unk11, float unk14, float unk18, float unk1C) {
                this.Type = type;
                this.Path = path;
                this.Scale = scale;
                this.Unk10 = unk10;
                this.Unk11 = unk11;
                this.Unk14 = unk14;
                this.Unk18 = unk18;
                this.Unk1C = unk1C;
            }

            internal Texture(BinaryReaderEx br, FLVERHeader header) {
                int pathOffset = br.ReadInt32();
                int typeOffset = br.ReadInt32();
                this.Scale = br.ReadVector2();

                this.Unk10 = br.AssertByte(0, 1, 2);
                this.Unk11 = br.ReadBoolean();
                _ = br.AssertByte(0);
                _ = br.AssertByte(0);

                this.Unk14 = br.ReadSingle();
                this.Unk18 = br.ReadSingle();
                this.Unk1C = br.ReadSingle();

                if (header.Unicode) {
                    this.Type = br.GetUTF16(typeOffset);
                    this.Path = br.GetUTF16(pathOffset);
                } else {
                    this.Type = br.GetShiftJIS(typeOffset);
                    this.Path = br.GetShiftJIS(pathOffset);
                }
            }

            internal void Write(BinaryWriterEx bw, int index) {
                bw.ReserveInt32($"TexturePath{index}");
                bw.ReserveInt32($"TextureType{index}");
                bw.WriteVector2(this.Scale);

                bw.WriteByte(this.Unk10);
                bw.WriteBoolean(this.Unk11);
                bw.WriteByte(0);
                bw.WriteByte(0);

                bw.WriteSingle(this.Unk14);
                bw.WriteSingle(this.Unk18);
                bw.WriteSingle(this.Unk1C);
            }

            internal void WriteStrings(BinaryWriterEx bw, FLVERHeader header, int index) {
                bw.FillInt32($"TexturePath{index}", (int)bw.Position);
                if (header.Unicode) {
                    bw.WriteUTF16(this.Path, true);
                } else {
                    bw.WriteShiftJIS(this.Path, true);
                }

                bw.FillInt32($"TextureType{index}", (int)bw.Position);
                if (header.Unicode) {
                    bw.WriteUTF16(this.Type, true);
                } else {
                    bw.WriteShiftJIS(this.Type, true);
                }
            }

            /// <summary>
            /// Returns this texture's type and path.
            /// </summary>
            public override string ToString() => $"{this.Type} = {this.Path}";
        }
    }
}
