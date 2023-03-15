using System.Collections.Generic;
using SoulsFormats.Formats.FLVER;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class FLVER0 {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class Material : IFlverMaterial {
            public string Name { get; set; }

            public string MTD { get; set; }

            public List<Texture> Textures { get; set; }
            IReadOnlyList<IFlverTexture> IFlverMaterial.Textures => this.Textures;

            public List<BufferLayout> Layouts { get; set; }

            internal Material(BinaryReaderEx br, FLVER0 flv) {
                int nameOffset = br.ReadInt32();
                int mtdOffset = br.ReadInt32();
                int texturesOffset = br.ReadInt32();
                int layoutsOffset = br.ReadInt32();
                _ = br.ReadInt32(); // Data length from name offset to end of buffer layouts
                int layoutHeaderOffset = br.ReadInt32();
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);

                this.Name = flv.Unicode ? br.GetUTF16(nameOffset) : br.GetShiftJIS(nameOffset);
                this.MTD = flv.Unicode ? br.GetUTF16(mtdOffset) : br.GetShiftJIS(mtdOffset);

                br.StepIn(texturesOffset);
                {
                    byte textureCount = br.ReadByte();
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);

                    this.Textures = new List<Texture>(textureCount);
                    for (int i = 0; i < textureCount; i++) {
                        this.Textures.Add(new Texture(br, flv));
                    }
                }
                br.StepOut();

                if (layoutHeaderOffset != 0) {
                    br.StepIn(layoutHeaderOffset);
                    {
                        int layoutCount = br.ReadInt32();
                        _ = br.AssertInt32((int)br.Position + 0xC);
                        _ = br.AssertInt32(0);
                        _ = br.AssertInt32(0);
                        this.Layouts = new List<BufferLayout>(layoutCount);
                        for (int i = 0; i < layoutCount; i++) {
                            int layoutOffset = br.ReadInt32();
                            br.StepIn(layoutOffset);
                            {
                                this.Layouts.Add(new BufferLayout(br));
                            }
                            br.StepOut();
                        }
                    }
                    br.StepOut();
                } else {
                    this.Layouts = new List<BufferLayout>(1);
                    br.StepIn(layoutsOffset);
                    {
                        this.Layouts.Add(new BufferLayout(br));
                    }
                    br.StepOut();
                }
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
