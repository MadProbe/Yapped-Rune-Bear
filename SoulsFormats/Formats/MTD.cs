using System;
using System.Collections.Generic;
using System.IO;
using SoulsFormats.Util;

namespace SoulsFormats.Formats {
    /// <summary>
    /// A material definition format used in all souls games.
    /// </summary>
    public class MTD : SoulsFile<MTD> {
        /// <summary>
        /// A path to the shader source file, which also determines which compiled shader to use for this material.
        /// </summary>
        public string ShaderPath { get; set; }

        /// <summary>
        /// A description of this material's purpose.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Values determining material properties.
        /// </summary>
        public List<Param> Params { get; set; }

        /// <summary>
        /// Texture types required by the material shader.
        /// </summary>
        public List<Texture> Textures { get; set; }

        /// <summary>
        /// Creates an MTD with default values.
        /// </summary>
        public MTD() {
            this.ShaderPath = "Unknown.spx";
            this.Description = "";
            this.Params = new List<Param>();
            this.Textures = new List<Texture>();
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected internal override bool Is(BinaryReaderEx br) {
            if (br.Length < 0x30) {
                return false;
            }

            string magic = br.GetASCII(0x2C, 4);
            return magic == "MTD ";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            br.BigEndian = false;
            _ = Block.Read(br, 0, 3, 0x01); // File
            {
                _ = Block.Read(br, 1, 2, 0xB0); // Header
                {
                    _ = AssertMarkedString(br, 0x34, "MTD ");
                    _ = br.AssertInt32(1000);
                }
                _ = AssertMarker(br, 0x01);

                _ = Block.Read(br, 2, 4, 0xA3); // Data
                {
                    this.ShaderPath = ReadMarkedString(br, 0xA3);
                    this.Description = ReadMarkedString(br, 0x03);
                    _ = br.AssertInt32(1);

                    _ = Block.Read(br, 3, 4, 0xA3); // Lists
                    {
                        _ = br.AssertInt32(0);
                        _ = AssertMarker(br, 0x03);

                        int paramCount = br.ReadInt32();
                        this.Params = new List<Param>(paramCount);
                        for (int i = 0; i < paramCount; i++) {
                            this.Params.Add(new Param(br));
                        }

                        _ = AssertMarker(br, 0x03);

                        int textureCount = br.ReadInt32();
                        this.Textures = new List<Texture>(textureCount);
                        for (int i = 0; i < textureCount; i++) {
                            this.Textures.Add(new Texture(br));
                        }

                        _ = AssertMarker(br, 0x04);
                        _ = br.AssertInt32(0);
                    }
                    _ = AssertMarker(br, 0x04);
                    _ = br.AssertInt32(0);
                }
                _ = AssertMarker(br, 0x04);
                _ = br.AssertInt32(0);
            }
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected internal override void Write(BinaryWriterEx bw) {
            bw.BigEndian = false;
            var fileBlock = Block.Write(bw, 0, 3, 0x01);
            {
                var headerBlock = Block.Write(bw, 1, 2, 0xB0);
                {
                    WriteMarkedString(bw, 0x34, "MTD ");
                    bw.WriteInt32(1000);
                }
                headerBlock.Finish(bw);
                WriteMarker(bw, 0x01);

                var dataBlock = Block.Write(bw, 2, 4, 0xA3);
                {
                    WriteMarkedString(bw, 0xA3, this.ShaderPath);
                    WriteMarkedString(bw, 0x03, this.Description);
                    bw.WriteInt32(1);

                    var listsBlock = Block.Write(bw, 3, 4, 0xA3);
                    {
                        bw.WriteInt32(0);
                        WriteMarker(bw, 0x03);

                        bw.WriteInt32(this.Params.Count);
                        foreach (Param internalEntry in this.Params) {
                            internalEntry.Write(bw);
                        }

                        WriteMarker(bw, 0x03);

                        bw.WriteInt32(this.Textures.Count);
                        foreach (Texture externalEntry in this.Textures) {
                            externalEntry.Write(bw);
                        }

                        WriteMarker(bw, 0x04);
                        bw.WriteInt32(0);
                    }
                    listsBlock.Finish(bw);
                    WriteMarker(bw, 0x04);
                    bw.WriteInt32(0);
                }
                dataBlock.Finish(bw);
                WriteMarker(bw, 0x04);
                bw.WriteInt32(0);
            }
            fileBlock.Finish(bw);
        }

        /// <summary>
        /// A value defining the material's properties.
        /// </summary>
        public class Param {
            /// <summary>
            /// The name of the param.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// The type of this value.
            /// </summary>
            public ParamType Type { get; }

            /// <summary>
            /// The value itself.
            /// </summary>
            public object Value { get; set; }

            /// <summary>
            /// Creates a new Param with the specified values.
            /// </summary>
            public Param(string name, ParamType type, object value = null) {
                this.Name = name;
                this.Type = type;
                this.Value = value;
                if (this.Value == null) {
                    switch (type) {
                        case ParamType.Bool: this.Value = false; break;
                        case ParamType.Float: this.Value = 0f; break;
                        case ParamType.Float2: this.Value = new float[2]; break;
                        case ParamType.Float3: this.Value = new float[3]; break;
                        case ParamType.Float4: this.Value = new float[4]; break;
                        case ParamType.Int: this.Value = 0; break;
                        case ParamType.Int2: this.Value = new int[2]; break;
                    }
                }
            }

            internal Param(BinaryReaderEx br) {
                _ = Block.Read(br, 4, 4, 0xA3); // Param
                {
                    this.Name = ReadMarkedString(br, 0xA3);
                    string type = ReadMarkedString(br, 0x04);
                    this.Type = (ParamType)Enum.Parse(typeof(ParamType), type, true);
                    _ = br.AssertInt32(1);

                    _ = Block.Read(br, null, 1, null); // Value
                    {
                        _ = br.ReadInt32(); // Value count

                        if (this.Type == ParamType.Int) {
                            this.Value = br.ReadInt32();
                        } else if (this.Type == ParamType.Int2) {
                            this.Value = br.ReadInt32s(2);
                        } else if (this.Type == ParamType.Bool) {
                            this.Value = br.ReadBoolean();
                        } else if (this.Type == ParamType.Float) {
                            this.Value = br.ReadSingle();
                        } else if (this.Type == ParamType.Float2) {
                            this.Value = br.ReadSingles(2);
                        } else if (this.Type == ParamType.Float3) {
                            this.Value = br.ReadSingles(3);
                        } else if (this.Type == ParamType.Float4) {
                            this.Value = br.ReadSingles(4);
                        }
                    }
                    _ = AssertMarker(br, 0x04);
                    _ = br.AssertInt32(0);
                }
            }

            internal void Write(BinaryWriterEx bw) {
                var paramBlock = Block.Write(bw, 4, 4, 0xA3);
                {
                    WriteMarkedString(bw, 0xA3, this.Name);
                    WriteMarkedString(bw, 0x04, this.Type.ToString().ToLower());
                    bw.WriteInt32(1);

                    int valueBlockType = -1;
                    byte valueBlockMarker = 0xFF;
                    if (this.Type == ParamType.Bool) {
                        valueBlockType = 0x1000;
                        valueBlockMarker = 0xC0;
                    } else if (this.Type is ParamType.Int or ParamType.Int2) {
                        valueBlockType = 0x1001;
                        valueBlockMarker = 0xC5;
                    } else if (this.Type is ParamType.Float or ParamType.Float2 or ParamType.Float3 or ParamType.Float4) {
                        valueBlockType = 0x1002;
                        valueBlockMarker = 0xCA;
                    }

                    var valueBlock = Block.Write(bw, valueBlockType, 1, valueBlockMarker);
                    {
                        int valueCount = -1;
                        if (this.Type is ParamType.Bool or ParamType.Int or ParamType.Float) {
                            valueCount = 1;
                        } else if (this.Type is ParamType.Int2 or ParamType.Float2) {
                            valueCount = 2;
                        } else if (this.Type == ParamType.Float3) {
                            valueCount = 3;
                        } else if (this.Type == ParamType.Float4) {
                            valueCount = 4;
                        }

                        bw.WriteInt32(valueCount);

                        if (this.Type == ParamType.Int) {
                            bw.WriteInt32((int)this.Value);
                        } else if (this.Type == ParamType.Int2) {
                            bw.WriteInt32s((int[])this.Value);
                        } else if (this.Type == ParamType.Bool) {
                            bw.WriteBoolean((bool)this.Value);
                        } else if (this.Type == ParamType.Float) {
                            bw.WriteSingle((float)this.Value);
                        } else if (this.Type == ParamType.Float2) {
                            bw.WriteSingles((float[])this.Value);
                        } else if (this.Type == ParamType.Float3) {
                            bw.WriteSingles((float[])this.Value);
                        } else if (this.Type == ParamType.Float4) {
                            bw.WriteSingles((float[])this.Value);
                        }
                    }
                    valueBlock.Finish(bw);
                    WriteMarker(bw, 0x04);
                    bw.WriteInt32(0);
                }
                paramBlock.Finish(bw);
            }

            /// <summary>
            /// Returns the name and value of the param.
            /// </summary>
            public override string ToString() => this.Type is ParamType.Float2 or ParamType.Float3 or ParamType.Float4
                    ? $"{this.Name} = {{{string.Join(", ", (float[])this.Value)}}}"
                    : this.Type == ParamType.Int2 ? $"{this.Name} = {{{string.Join(", ", (int[])this.Value)}}}" : $"{this.Name} = {this.Value}";
        }

        /// <summary>
        /// Value types of MTD params.
        /// </summary>
        // I believe the engine supports Bool2-4 and Int3-4 as well, but they're never used so I won't bother yet.
        public enum ParamType {
            /// <summary>
            /// A one-byte boolean value.
            /// </summary>
            Bool,

            /// <summary>
            /// A four-byte integer.
            /// </summary>
            Int,

            /// <summary>
            /// An array of two four-byte integers.
            /// </summary>
            Int2,

            /// <summary>
            /// A four-byte floating point number.
            /// </summary>
            Float,

            /// <summary>
            /// An array of two four-byte floating point numbers.
            /// </summary>
            Float2,

            /// <summary>
            /// An array of three four-byte floating point numbers.
            /// </summary>
            Float3,

            /// <summary>
            /// An array of four four-byte floating point numbers.
            /// </summary>
            Float4,
        }

        /// <summary>
        /// Texture types used by the material, filled in in each FLVER.
        /// </summary>
        public class Texture {
            /// <summary>
            /// The type of texture (g_Diffuse, g_Specular, etc).
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// Whether the texture has extended information for Sekiro.
            /// </summary>
            public bool Extended { get; set; }

            /// <summary>
            /// Indicates the order of UVs in FLVER vertex data.
            /// </summary>
            public int UVNumber { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int ShaderDataIndex { get; set; }

            /// <summary>
            /// A fixed texture path for this material, only used in Sekiro.
            /// </summary>
            public string Path { get; set; }

            /// <summary>
            /// Floats for an unknown purpose, only used in Sekiro.
            /// </summary>
            public List<float> UnkFloats { get; set; }

            /// <summary>
            /// Creates a Texture with default values.
            /// </summary>
            public Texture() {
                this.Type = "g_DiffuseTexture";
                this.Path = "";
                this.UnkFloats = new List<float>();
            }

            internal Texture(BinaryReaderEx br) {
                var textureBlock = Block.Read(br, 0x2000, null, 0xA3);
                {
                    this.Extended = textureBlock.Version != 3
&& (textureBlock.Version == 5
                        ? true
                        : throw new InvalidDataException($"Texture block version is expected to be 3 or 5, but it was {textureBlock.Version}."));

                    this.Type = ReadMarkedString(br, 0x35);
                    this.UVNumber = br.ReadInt32();
                    _ = AssertMarker(br, 0x35);
                    this.ShaderDataIndex = br.ReadInt32();

                    if (this.Extended) {
                        _ = br.AssertInt32(0xA3);
                        this.Path = ReadMarkedString(br, 0xBA);
                        int floatCount = br.ReadInt32();
                        this.UnkFloats = new List<float>(br.ReadSingles(floatCount));
                    } else {
                        this.Path = "";
                        this.UnkFloats = new List<float>();
                    }
                }
            }

            internal void Write(BinaryWriterEx bw) {
                var textureBlock = Block.Write(bw, 0x2000, this.Extended ? 5 : 3, 0xA3);
                {
                    WriteMarkedString(bw, 0x35, this.Type);
                    bw.WriteInt32(this.UVNumber);
                    WriteMarker(bw, 0x35);
                    bw.WriteInt32(this.ShaderDataIndex);

                    if (this.Extended) {
                        bw.WriteInt32(0xA3);
                        WriteMarkedString(bw, 0xBA, this.Path);
                        bw.WriteInt32(this.UnkFloats.Count);
                        bw.WriteSingles(this.UnkFloats);
                    }
                }
                textureBlock.Finish(bw);
            }

            /// <summary>
            /// Returns the type of the texture.
            /// </summary>
            public override string ToString() => this.Type;
        }

        /// <summary>
        /// The blending mode of the material, used in value g_BlendMode.
        /// </summary>
        public enum BlendMode {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            Normal = 0,
            TexEdge = 1,
            Blend = 2,
            Water = 3,
            Add = 4,
            Sub = 5,
            Mul = 6,
            AddMul = 7,
            SubMul = 8,
            WaterWave = 9,
            LSNormal = 32,
            LSTexEdge = 33,
            LSBlend = 34,
            LSWater = 35,
            LSAdd = 36,
            LSSub = 37,
            LSMul = 38,
            LSAddMul = 39,
            LSSubMul = 40,
            LSWaterWave = 41,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }

        /// <summary>
        /// The lighting type of a material, used in value g_LightingType.
        /// </summary>
        public enum LightingType {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            None = 0,
            HemDirDifSpcx3 = 1,
            HemEnvDifSpc = 3,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }

        #region Read/Write utilities
        private static byte ReadMarker(BinaryReaderEx br) {
            byte marker = br.ReadByte();
            br.Pad(4);
            return marker;
        }

        private static byte AssertMarker(BinaryReaderEx br, byte marker) {
            _ = br.AssertByte(marker);
            br.Pad(4);
            return marker;
        }

        private static void WriteMarker(BinaryWriterEx bw, byte marker) {
            bw.WriteByte(marker);
            bw.Pad(4);
        }

        private static string ReadMarkedString(BinaryReaderEx br, byte marker) {
            int length = br.ReadInt32();
            string str = br.ReadShiftJIS(length);
            _ = AssertMarker(br, marker);
            return str;
        }

        private static string AssertMarkedString(BinaryReaderEx br, byte marker, string assert) {
            string str = ReadMarkedString(br, marker);
            return str != assert
                ? throw new InvalidDataException($"Read marked string: {str} | Expected: {assert} | Ending position: 0x{br.Position:X}")
                : str;
        }

        private static void WriteMarkedString(BinaryWriterEx bw, byte marker, string str) {
            byte[] bytes = SFEncoding.ShiftJIS.GetBytes(str);
            bw.WriteInt32(bytes.Length);
            bw.WriteBytes(bytes);
            WriteMarker(bw, marker);
        }

        private class Block {
            public long Start;
            public uint Length;
            public int Type;
            public int Version;
            public byte Marker;

            private Block(long start, uint length, int type, int version, byte marker) {
                this.Start = start;
                this.Length = length;
                this.Type = type;
                this.Version = version;
                this.Marker = marker;
            }

            public static Block Read(BinaryReaderEx br, int? assertType, int? assertVersion, byte? assertMarker) {
                _ = br.AssertInt32(0);
                uint length = br.ReadUInt32();
                long start = br.Position;
                int type = assertType.HasValue ? br.AssertInt32(assertType.Value) : br.ReadInt32();
                int version = assertVersion.HasValue ? br.AssertInt32(assertVersion.Value) : br.ReadInt32();
                byte marker = assertMarker.HasValue ? AssertMarker(br, assertMarker.Value) : ReadMarker(br);
                return new Block(start, length, type, version, marker);
            }

            public static Block Write(BinaryWriterEx bw, int type, int version, byte marker) {
                bw.WriteInt32(0);
                long start = bw.Position + 4;
                bw.ReserveUInt32($"Block{start:X}");
                bw.WriteInt32(type);
                bw.WriteInt32(version);
                WriteMarker(bw, marker);
                return new Block(start, 0, type, version, marker);
            }

            public void Finish(BinaryWriterEx bw) {
                this.Length = (uint)(bw.Position - this.Start);
                bw.FillUInt32($"Block{this.Start:X}", this.Length);
            }
        }
        #endregion
    }
}
