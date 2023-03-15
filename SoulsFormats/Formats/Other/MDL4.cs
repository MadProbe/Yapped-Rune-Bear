using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using SoulsFormats.Util;

namespace SoulsFormats.Formats.Other {
    /// <summary>
    /// A 3D model format used in early PS3/X360 games. Extension: .mdl
    /// </summary>
    public class MDL4 : SoulsFile<MDL4> {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int Version;
        public int Unk20;
        public Vector3 BoundingBoxMin;
        public Vector3 BoundingBoxMax;
        public int TrueFaceCount;
        public int TotalFaceCount;

        public List<Dummy> Dummies;
        public List<Material> Materials;
        public List<Bone> Bones;
        public List<Mesh> Meshes;

        protected internal override bool Is(BinaryReaderEx br) {
            if (br.Length < 4) {
                return false;
            }

            string magic = br.GetASCII(0, 4);
            return magic == "MDL4";
        }

        protected internal override void Read(BinaryReaderEx br) {
            br.BigEndian = true;
            _ = br.AssertASCII("MDL4");
            this.Version = br.AssertInt32(0x40001, 0x40002);
            int dataStart = br.ReadInt32();
            _ = br.ReadInt32(); // Data length
            int dummyCount = br.ReadInt32();
            int materialCount = br.ReadInt32();
            int boneCount = br.ReadInt32();
            int meshCount = br.ReadInt32();
            this.Unk20 = br.ReadInt32();
            this.BoundingBoxMin = br.ReadVector3();
            this.BoundingBoxMax = br.ReadVector3();
            this.TrueFaceCount = br.ReadInt32();
            this.TotalFaceCount = br.ReadInt32();
            br.AssertPattern(0x3C, 0x00);

            this.Dummies = new List<Dummy>(dummyCount);
            for (int i = 0; i < dummyCount; i++) {
                this.Dummies.Add(new Dummy(br));
            }

            this.Materials = new List<Material>(materialCount);
            for (int i = 0; i < materialCount; i++) {
                this.Materials.Add(new Material(br));
            }

            this.Bones = new List<Bone>(boneCount);
            for (int i = 0; i < boneCount; i++) {
                this.Bones.Add(new Bone(br));
            }

            this.Meshes = new List<Mesh>(meshCount);
            for (int i = 0; i < meshCount; i++) {
                this.Meshes.Add(new Mesh(br, dataStart, this.Version));
            }
        }

        public class Dummy {
            public Vector3 Forward;
            public Vector3 Upward;
            public Color Color;
            public short ID;
            public short Unk1E;
            public short Unk20;
            public short Unk22;

            internal Dummy(BinaryReaderEx br) {
                this.Forward = br.ReadVector3();
                this.Upward = br.ReadVector3();
                this.Color = br.ReadRGBA();
                this.ID = br.ReadInt16();
                this.Unk1E = br.ReadInt16();
                this.Unk20 = br.ReadInt16();
                this.Unk22 = br.ReadInt16();
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);
            }
        }

        public class Material {
            public string Name;
            public string Shader;
            public byte Unk3C;
            public byte Unk3D;
            public byte Unk3E;
            public List<Param> Params;

            internal Material(BinaryReaderEx br) {
                this.Name = br.ReadFixStr(0x1F);
                this.Shader = br.ReadFixStr(0x1D);
                this.Unk3C = br.ReadByte();
                this.Unk3D = br.ReadByte();
                this.Unk3E = br.ReadByte();
                byte paramCount = br.ReadByte();

                long paramsStart = br.Position;
                this.Params = new List<Param>(paramCount);
                for (int i = 0; i < paramCount; i++) {
                    this.Params.Add(new Param(br));
                }

                br.Position = paramsStart + 0x800;
            }

            public class Param {
                public ParamType Type;
                public string Name;
                public object Value;

                internal Param(BinaryReaderEx br) {
                    long start = br.Position;
                    this.Type = br.ReadEnum8<ParamType>();
                    this.Name = br.ReadFixStr(0x1F);

                    this.Value = this.Type switch {
                        ParamType.Int => br.ReadInt32(),
                        ParamType.Float => br.ReadSingle(),
                        ParamType.Float4 => br.ReadSingles(4),
                        ParamType.String => br.ReadShiftJIS(),
                        _ => throw new NotImplementedException("Unknown param type: " + this.Type),
                    };
                    br.Position = start + 0x40;
                }
            }

            public enum ParamType : byte {
                Int = 0,
                Float = 1,
                Float4 = 4,
                String = 5,
            }
        }

        public class Bone {
            public string Name;
            public Vector3 Translation;
            public Vector3 Rotation;
            public Vector3 Scale;
            public Vector3 BoundingBoxMin;
            public Vector3 BoundingBoxMax;
            public short ParentIndex;
            public short ChildIndex;
            public short NextSiblingIndex;
            public short PreviousSiblingIndex;
            public short[] UnkIndices;

            internal Bone(BinaryReaderEx br) {
                this.Name = br.ReadFixStr(0x20);
                this.Translation = br.ReadVector3();
                this.Rotation = br.ReadVector3();
                this.Scale = br.ReadVector3();
                this.BoundingBoxMin = br.ReadVector3();
                this.BoundingBoxMax = br.ReadVector3();
                this.ParentIndex = br.ReadInt16();
                this.ChildIndex = br.ReadInt16();
                this.NextSiblingIndex = br.ReadInt16();
                this.PreviousSiblingIndex = br.ReadInt16();
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);
                this.UnkIndices = br.ReadInt16s(16);
            }

            /// <summary>
            /// Creates a transformation matrix from the scale, rotation, and translation of the bone.
            /// </summary>
            public Matrix4x4 ComputeLocalTransform() => Matrix4x4.CreateScale(this.Scale)
                    * Matrix4x4.CreateRotationX(this.Rotation.X)
                    * Matrix4x4.CreateRotationZ(this.Rotation.Z)
                    * Matrix4x4.CreateRotationY(this.Rotation.Y)
                    * Matrix4x4.CreateTranslation(this.Translation);
        }

        public class Mesh {
            public byte VertexFormat;
            public byte MaterialIndex;
            public bool Unk02;
            public bool Unk03;
            public short Unk08;
            public short[] BoneIndices;
            public ushort[] VertexIndices;
            public List<Vertex> Vertices;
            public byte[][] UnkBlocks;

            internal Mesh(BinaryReaderEx br, int dataStart, int version) {
                this.VertexFormat = br.AssertByte(0, 1, 2);
                this.MaterialIndex = br.ReadByte();
                this.Unk02 = br.ReadBoolean();
                this.Unk03 = br.ReadBoolean();
                ushort vertexIndexCount = br.ReadUInt16();
                this.Unk08 = br.ReadInt16();
                this.BoneIndices = br.ReadInt16s(28);
                _ = br.ReadInt32(); // Vertex indices length
                int vertexIndicesOffset = br.ReadInt32();
                int bufferLength = br.ReadInt32();
                int bufferOffset = br.ReadInt32();

                if (this.VertexFormat == 2) {
                    this.UnkBlocks = new byte[16][];
                    for (int i = 0; i < 16; i++) {
                        int length = br.ReadInt32();
                        int offset = br.ReadInt32();
                        this.UnkBlocks[i] = br.GetBytes(dataStart + offset, length);
                    }
                }

                this.VertexIndices = br.GetUInt16s(dataStart + vertexIndicesOffset, vertexIndexCount);

                br.StepIn(dataStart + bufferOffset);
                {
                    int vertexSize = 0;
                    if (version == 0x40001) {
                        if (this.VertexFormat == 0) {
                            vertexSize = 0x40;
                        } else if (this.VertexFormat == 1) {
                            vertexSize = 0x54;
                        } else if (this.VertexFormat == 2) {
                            vertexSize = 0x3C;
                        }
                    } else if (version == 0x40002) {
                        if (this.VertexFormat == 0) {
                            vertexSize = 0x28;
                        }
                    }
                    int vertexCount = bufferLength / vertexSize;
                    this.Vertices = new List<Vertex>(vertexCount);
                    for (int i = 0; i < vertexCount; i++) {
                        this.Vertices.Add(new Vertex(br, version, this.VertexFormat));
                    }
                }
                br.StepOut();
            }

            public List<Vertex[]> GetFaces() {
                ushort[] indices = this.ToTriangleList();
                var faces = new List<Vertex[]>();
                for (int i = 0; i < indices.Length; i += 3) {
                    faces.Add(new Vertex[]
                    {
                        this.Vertices[indices[i + 0]],
                        this.Vertices[indices[i + 1]],
                        this.Vertices[indices[i + 2]],
                    });
                }
                return faces;
            }

            public ushort[] ToTriangleList() {
                var converted = new List<ushort>();
                bool flip = false;
                for (int i = 0; i < this.VertexIndices.Length - 2; i++) {
                    ushort vi1 = this.VertexIndices[i];
                    ushort vi2 = this.VertexIndices[i + 1];
                    ushort vi3 = this.VertexIndices[i + 2];

                    if (vi1 == 0xFFFF || vi2 == 0xFFFF || vi3 == 0xFFFF) {
                        flip = false;
                    } else {
                        if (vi1 != vi2 && vi1 != vi3 && vi2 != vi3) {
                            if (!flip) {
                                converted.Add(vi1);
                                converted.Add(vi2);
                                converted.Add(vi3);
                            } else {
                                converted.Add(vi3);
                                converted.Add(vi2);
                                converted.Add(vi1);
                            }
                        }
                        flip = !flip;
                    }
                }
                return converted.ToArray();
            }
        }

        public class Vertex {
            public Vector3 Position;
            public Vector4 Normal;
            public Vector4 Tangent;
            public Vector4 Bitangent;
            public byte[] Color;
            public List<Vector2> UVs;
            public short[] BoneIndices;
            public float[] BoneWeights;
            public short Unk3C;
            public short Unk3E;

            internal Vertex(BinaryReaderEx br, int version, byte format) {
                this.UVs = new List<Vector2>();
                if (version == 0x40001) {
                    if (format == 0) {
                        this.Position = br.ReadVector3();
                        this.Normal = Read10BitVector4(br);
                        this.Tangent = Read10BitVector4(br);
                        this.Bitangent = Read10BitVector4(br);
                        this.Color = br.ReadBytes(4);
                        this.UVs.Add(br.ReadVector2());
                        this.UVs.Add(br.ReadVector2());
                        this.UVs.Add(br.ReadVector2());
                        this.UVs.Add(br.ReadVector2());
                        this.Unk3C = br.ReadInt16();
                        this.Unk3E = br.AssertInt16(0);
                    } else if (format == 1) {
                        this.Position = br.ReadVector3();
                        this.Normal = Read10BitVector4(br);
                        this.Tangent = Read10BitVector4(br);
                        this.Bitangent = Read10BitVector4(br);
                        this.Color = br.ReadBytes(4);
                        this.UVs.Add(br.ReadVector2());
                        this.UVs.Add(br.ReadVector2());
                        this.UVs.Add(br.ReadVector2());
                        this.UVs.Add(br.ReadVector2());
                        this.BoneIndices = br.ReadInt16s(4);
                        this.BoneWeights = br.ReadSingles(4);
                    } else if (format == 2) {
                        this.Color = br.ReadBytes(4);
                        this.UVs.Add(br.ReadVector2());
                        this.UVs.Add(br.ReadVector2());
                        this.UVs.Add(br.ReadVector2());
                        this.UVs.Add(br.ReadVector2());
                        this.BoneIndices = br.ReadInt16s(4);
                        this.BoneWeights = br.ReadSingles(4);
                    }
                } else if (version == 0x40002) {
                    if (format == 0) {
                        this.Position = br.ReadVector3();
                        this.Normal = ReadSByteVector4(br);
                        this.Tangent = ReadSByteVector4(br);
                        this.Color = br.ReadBytes(4);
                        this.UVs.Add(ReadShortUV(br));
                        this.UVs.Add(ReadShortUV(br));
                        this.UVs.Add(ReadShortUV(br));
                        this.UVs.Add(ReadShortUV(br));
                    }
                }
            }

            private static Vector4 ReadByteVector4(BinaryReaderEx br) {
                byte w = br.ReadByte();
                byte z = br.ReadByte();
                byte y = br.ReadByte();
                byte x = br.ReadByte();
                return new Vector4((x - 127) / 127f, (y - 127) / 127f, (z - 127) / 127f, (w - 127) / 127f);
            }

            private static Vector4 ReadSByteVector4(BinaryReaderEx br) {
                sbyte w = br.ReadSByte();
                sbyte z = br.ReadSByte();
                sbyte y = br.ReadSByte();
                sbyte x = br.ReadSByte();
                return new Vector4(x / 127f, y / 127f, z / 127f, w / 127f);
            }

            private static Vector2 ReadShortUV(BinaryReaderEx br) {
                short u = br.ReadInt16();
                short v = br.ReadInt16();
                return new Vector2(u / 2048f, v / 2048f);
            }

            private static Vector4 Read10BitVector4(BinaryReaderEx br) {
                int vector = br.ReadInt32();
                int x = vector << 22 >> 22;
                int y = vector << 12 >> 22;
                int z = vector << 2 >> 22;
                int w = vector << 0 >> 30;
                return new Vector4(x / 511f, y / 511f, z / 511f, w);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
