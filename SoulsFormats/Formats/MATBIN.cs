using System;
using System.Collections.Generic;
using System.Numerics;
using SoulsFormats.Util;

namespace SoulsFormats.Formats {
    /// <summary>
    /// A material config format introduced in Elden Ring. Extension: .matbin
    /// </summary>
    public class MATBIN : SoulsFile<MATBIN> {
        /// <summary>
        /// Network path to the shader source file.
        /// </summary>
        public string ShaderPath { get; set; }

        /// <summary>
        /// Network path to the material source file, either a matxml or an mtd.
        /// </summary>
        public string SourcePath { get; set; }

        /// <summary>
        /// Unknown, presumed to be an identifier for documentation.
        /// </summary>
        public uint Key { get; set; }

        /// <summary>
        /// Parameters set by this material.
        /// </summary>
        public List<Param> Params { get; set; }

        /// <summary>
        /// Texture samplers used by this material.
        /// </summary>
        public List<Sampler> Samplers { get; set; }

        /// <summary>
        /// Creates an empty MATBIN.
        /// </summary>
        public MATBIN() {
            this.ShaderPath = "";
            this.SourcePath = "";
            this.Params = new List<Param>();
            this.Samplers = new List<Sampler>();
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected internal override bool Is(BinaryReaderEx br) {
            if (br.Length < 4) {
                return false;
            }

            string magic = br.GetASCII(0, 4);
            return magic == "MAB\0";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            br.BigEndian = false;

            _ = br.AssertASCII("MAB\0");
            _ = br.AssertInt32(2);
            this.ShaderPath = br.GetUTF16(br.ReadInt64());
            this.SourcePath = br.GetUTF16(br.ReadInt64());
            this.Key = br.ReadUInt32();
            int paramCount = br.ReadInt32();
            int samplerCount = br.ReadInt32();
            br.AssertPattern(0x14, 0x00);

            this.Params = new List<Param>(paramCount);
            for (int i = 0; i < paramCount; i++) {
                this.Params.Add(new Param(br));
            }

            this.Samplers = new List<Sampler>(samplerCount);
            for (int i = 0; i < samplerCount; i++) {
                this.Samplers.Add(new Sampler(br));
            }
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected internal override void Write(BinaryWriterEx bw) {
            bw.BigEndian = false;

            bw.WriteASCII("MAB\0");
            bw.WriteInt32(2);
            bw.ReserveInt64("ShaderPathOffset");
            bw.ReserveInt64("SourcePathOffset");
            bw.WriteUInt32(this.Key);
            bw.WriteInt32(this.Params.Count);
            bw.WriteInt32(this.Samplers.Count);
            bw.WritePattern(0x14, 0x00);

            for (int i = 0; i < this.Params.Count; i++) {
                this.Params[i].Write(bw, i);
            }

            for (int i = 0; i < this.Samplers.Count; i++) {
                this.Samplers[i].Write(bw, i);
            }

            for (int i = 0; i < this.Params.Count; i++) {
                this.Params[i].WriteData(bw, i);
            }

            for (int i = 0; i < this.Samplers.Count; i++) {
                this.Samplers[i].WriteData(bw, i);
            }

            bw.FillInt64("ShaderPathOffset", bw.Position);
            bw.WriteUTF16(this.ShaderPath, true);

            bw.FillInt64("SourcePathOffset", bw.Position);
            bw.WriteUTF16(this.SourcePath, true);
        }

        /// <summary>
        /// Available types for param values.
        /// </summary>
        public enum ParamType : uint {
            /// <summary>
            /// (bool) A 1-byte boolean.
            /// </summary>
            Bool = 0,

            /// <summary>
            /// (int) A 32-bit integer. 
            /// </summary>
            Int = 4,

            /// <summary>
            /// (int[2]) Two 32-bit integers.
            /// </summary>
            Int2 = 5,

            /// <summary>
            /// (float) A 32-bit float.
            /// </summary>
            Float = 8,

            /// <summary>
            /// (float[2]) Two 32-bit floats.
            /// </summary>
            Float2 = 9,

            /// <summary>
            /// (float[3]) Three 32-bit floats.
            /// </summary>
            Float3 = 10,

            /// <summary>
            /// (float[4]) Four 32-bit floats.
            /// </summary>
            Float4 = 11,

            /// <summary>
            /// (float[5]) Five 32-bit floats.
            /// </summary>
            Float5 = 12,
        }

        /// <summary>
        /// A parameter set per material.
        /// </summary>
        public class Param {
            /// <summary>
            /// The name of the parameter.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// The value to be used.
            /// </summary>
            public object Value { get; set; }

            /// <summary>
            /// Unknown, presumed to be an identifier for documentation.
            /// </summary>
            public uint Key { get; set; }

            /// <summary>
            /// The type of value provided.
            /// </summary>
            public ParamType Type { get; set; }

            /// <summary>
            /// Creates a default Param.
            /// </summary>
            public Param() {
                this.Name = "";
                this.Type = ParamType.Int;
                this.Value = 0;
            }

            internal Param(BinaryReaderEx br) {
                this.Name = br.GetUTF16(br.ReadInt64());
                long valueOffset = br.ReadInt64();
                this.Key = br.ReadUInt32();
                this.Type = br.ReadEnum32<ParamType>();
                br.AssertPattern(0x10, 0x00);

                br.StepIn(valueOffset);
                {
                    this.Value = this.Type switch {
                        ParamType.Bool => br.ReadBoolean(),
                        ParamType.Int => br.ReadInt32(),
                        ParamType.Int2 => br.ReadInt32s(2),
                        ParamType.Float => br.ReadSingle(),
                        ParamType.Float2 => br.ReadSingles(2),
                        // For colors that use this type, there are actually five floats in the file.
                        // Because the extra values appear to be useless, they are being discarded
                        // for the sake of the API not being a complete nightmare trying to preserve them.
                        ParamType.Float3 => br.ReadSingles(3),
                        ParamType.Float4 => br.ReadSingles(4),
                        ParamType.Float5 => br.ReadSingles(5),
                        _ => throw new NotImplementedException($"Unimplemented value type: {this.Type}"),
                    };
                }
                br.StepOut();
            }

            internal void Write(BinaryWriterEx bw, int index) {
                bw.ReserveInt64($"ParamNameOffset[{index}]");
                bw.ReserveInt64($"ParamValueOffset[{index}]");
                bw.WriteUInt32(this.Key);
                bw.WriteUInt32((uint)this.Type);
                bw.WritePattern(0x10, 0x00);
            }

            internal void WriteData(BinaryWriterEx bw, int index) {
                bw.FillInt64($"ParamNameOffset[{index}]", bw.Position);
                bw.WriteUTF16(this.Name, true);

                bw.FillInt64($"ParamValueOffset[{index}]", bw.Position);
                switch (this.Type) {
                    // These assume that the arrays are the correct length, which it probably shouldn't.
                    case ParamType.Bool: bw.WriteBoolean((bool)this.Value); break;
                    case ParamType.Int: bw.WriteInt32((int)this.Value); break;
                    case ParamType.Int2: bw.WriteInt32s((int[])this.Value); break;
                    case ParamType.Float: bw.WriteSingle((float)this.Value); break;
                    case ParamType.Float2:
                    case ParamType.Float4:
                    case ParamType.Float5: bw.WriteSingles((float[])this.Value); break;

                    case ParamType.Float3:
                        bw.WriteSingles((float[])this.Value);
                        // Included on the slim chance that the aforementioned extra floats
                        // actually do anything, since they are always 1 when present.
                        bw.WriteSingle(1);
                        bw.WriteSingle(1);
                        break;

                    default:
                        throw new NotImplementedException($"Unimplemented value type: {this.Type}");
                }
            }
        }

        /// <summary>
        /// A texture sampler used by a material.
        /// </summary>
        public class Sampler {
            /// <summary>
            /// The type of the sampler.
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// An optional network path to the texture, if not specified in the FLVER.
            /// </summary>
            public string Path { get; set; }

            /// <summary>
            /// Unknown, presumed to be an identifier for documentation.
            /// </summary>
            public uint Key { get; set; }

            /// <summary>
            /// Unknown; most likely to be the scale, but typically 0, 0.
            /// </summary>
            public Vector2 Unk14 { get; set; }

            /// <summary>
            /// Creates a default Sampler.
            /// </summary>
            public Sampler() {
                this.Type = "";
                this.Path = "";
            }

            internal Sampler(BinaryReaderEx br) {
                this.Type = br.GetUTF16(br.ReadInt64());
                this.Path = br.GetUTF16(br.ReadInt64());
                this.Key = br.ReadUInt32();
                this.Unk14 = br.ReadVector2();
                br.AssertPattern(0x14, 0x00);
            }

            internal void Write(BinaryWriterEx bw, int index) {
                bw.ReserveInt64($"SamplerTypeOffset[{index}]");
                bw.ReserveInt64($"SamplerPathOffset[{index}]");
                bw.WriteUInt32(this.Key);
                bw.WriteVector2(this.Unk14);
                bw.WritePattern(0x14, 0x00);
            }

            internal void WriteData(BinaryWriterEx bw, int index) {
                bw.FillInt64($"SamplerTypeOffset[{index}]", bw.Position);
                bw.WriteUTF16(this.Type, true);

                bw.FillInt64($"SamplerPathOffset[{index}]", bw.Position);
                bw.WriteUTF16(this.Path, true);
            }
        }
    }
}
