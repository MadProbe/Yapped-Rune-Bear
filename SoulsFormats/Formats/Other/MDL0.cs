using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using SoulsFormats.Util;

namespace SoulsFormats.Formats.Other {
    /// <summary>
    /// A model format used in the original Otogi. Extension: .mdl
    /// </summary>
    public class MDL0 : SoulsFile<MDL0> {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int Unk04;
        public int Unk08;
        public List<Bone> Bones;
        public List<ushort> Indices;
        public List<Vertex> VerticesA;
        public List<Vertex> VerticesB;
        public List<Vertex> VerticesC;
        public List<Struct6> Struct6s;
        public List<Material> Materials;
        public List<string> Textures;

        protected internal override void Read(BinaryReaderEx br) {
            _ = br.ReadInt32(); // File size
            this.Unk04 = br.ReadInt32();
            this.Unk08 = br.ReadInt32();
            _ = br.ReadInt32(); // Face count

            int boneCount = br.ReadInt32();
            int indexCount = br.ReadInt32();
            int vertexCountA = br.ReadInt32();
            int vertexCountB = br.ReadInt32();
            int vertexCountC = br.ReadInt32();
            int count6 = br.ReadInt32();
            int materialCount = br.ReadInt32();
            int textureCount = br.ReadInt32();

            int bonesOffset = br.ReadInt32();
            int indicesOffset = br.ReadInt32();
            int verticesOffsetA = br.ReadInt32();
            int verticesOffsetB = br.ReadInt32();
            int verticesOffsetC = br.ReadInt32();
            int offset6 = br.ReadInt32();
            int materialsOffset = br.ReadInt32();
            int texturesOffset = br.ReadInt32();

            br.Position = bonesOffset;
            this.Bones = new List<Bone>(boneCount);
            for (int i = 0; i < boneCount; i++) {
                this.Bones.Add(new Bone(br));
            }

            br.Position = indicesOffset;
            this.Indices = new List<ushort>(br.ReadUInt16s(indexCount));

            br.Position = verticesOffsetA;
            this.VerticesA = new List<Vertex>(vertexCountA);
            for (int i = 0; i < vertexCountA; i++) {
                this.VerticesA.Add(new Vertex(br, VertexFormat.A));
            }

            br.Position = verticesOffsetB;
            this.VerticesB = new List<Vertex>(vertexCountB);
            for (int i = 0; i < vertexCountB; i++) {
                this.VerticesB.Add(new Vertex(br, VertexFormat.B));
            }

            br.Position = verticesOffsetC;
            this.VerticesC = new List<Vertex>(vertexCountC);
            for (int i = 0; i < vertexCountC; i++) {
                this.VerticesC.Add(new Vertex(br, VertexFormat.C));
            }

            br.Position = offset6;
            this.Struct6s = new List<Struct6>(count6);
            for (int i = 0; i < count6; i++) {
                this.Struct6s.Add(new Struct6(br));
            }

            br.Position = materialsOffset;
            this.Materials = new List<Material>(materialCount);
            for (int i = 0; i < materialCount; i++) {
                this.Materials.Add(new Material(br));
            }

            br.Position = texturesOffset;
            this.Textures = new List<string>(textureCount);
            for (int i = 0; i < textureCount; i++) {
                this.Textures.Add(br.ReadShiftJIS());
            }
        }

        public List<int> Triangulate(Mesh mesh, List<Vertex> vertices) {
            var triangles = new List<int>();
            bool flip = false;
            for (int i = mesh.StartIndex; i < mesh.StartIndex + mesh.IndexCount - 2; i++) {
                ushort vi1 = this.Indices[i];
                ushort vi2 = this.Indices[i + 1];
                ushort vi3 = this.Indices[i + 2];

                if (vi1 != vi2 && vi1 != vi3 && vi2 != vi3) {
                    Vertex v1 = vertices[vi1 - mesh.StartVertex];
                    Vertex v2 = vertices[vi2 - mesh.StartVertex];
                    Vertex v3 = vertices[vi3 - mesh.StartVertex];
                    var vertexNormal = Vector3.Normalize((v1.Normal + v2.Normal + v3.Normal) / 3);
                    var faceNormal = Vector3.Normalize(Vector3.Cross(v2.Position - v1.Position, v3.Position - v1.Position));
                    float angle = Vector3.Dot(faceNormal, vertexNormal) / (faceNormal.Length() * vertexNormal.Length());
                    flip = angle < 0;

                    if (!flip) {
                        triangles.Add(vi1);
                        triangles.Add(vi2);
                        triangles.Add(vi3);
                    } else {
                        triangles.Add(vi3);
                        triangles.Add(vi2);
                        triangles.Add(vi1);
                    }
                }
                flip = !flip;
            }
            return triangles;
        }

        public class Bone {
            public Vector3 Translation;
            public Vector3 Rotation;
            public Vector3 Scale;
            public int ParentIndex;
            public int ChildIndex;
            public int NextSiblingIndex;
            public int PrevSiblingIndex;
            public List<Mesh> MeshesA;
            public List<Mesh> MeshesB;
            public List<MeshGroup> MeshesC;
            public int Unk4C;

            internal Bone(BinaryReaderEx br) {
                this.Translation = br.ReadVector3();
                this.Rotation = br.ReadVector3();
                this.Scale = br.ReadVector3();
                this.ParentIndex = br.ReadInt32();
                this.ChildIndex = br.ReadInt32();
                this.NextSiblingIndex = br.ReadInt32();
                this.PrevSiblingIndex = br.ReadInt32();
                int meshCountA = br.ReadInt32();
                int meshCountB = br.ReadInt32();
                int meshCountC = br.ReadInt32();
                int meshesOffsetA = br.ReadInt32();
                int meshesOffsetB = br.ReadInt32();
                int meshesOffsetC = br.ReadInt32();
                this.Unk4C = br.ReadInt32();

                br.StepIn(meshesOffsetA);
                {
                    this.MeshesA = new List<Mesh>(meshCountA);
                    for (int i = 0; i < meshCountA; i++) {
                        this.MeshesA.Add(new Mesh(br));
                    }
                }
                br.StepOut();

                br.StepIn(meshesOffsetB);
                {
                    this.MeshesB = new List<Mesh>(meshCountB);
                    for (int i = 0; i < meshCountB; i++) {
                        this.MeshesB.Add(new Mesh(br));
                    }
                }
                br.StepOut();

                br.StepIn(meshesOffsetC);
                {
                    this.MeshesC = new List<MeshGroup>(meshCountC);
                    for (int i = 0; i < meshCountC; i++) {
                        this.MeshesC.Add(new MeshGroup(br));
                    }
                }
                br.StepOut();
            }
        }

        public class MeshGroup {
            public List<Mesh> Meshes;
            public byte Unk02;
            public byte Unk03;
            public short[] BoneIndices;

            internal MeshGroup(BinaryReaderEx br) {
                short meshCount = br.ReadInt16();
                this.Unk02 = br.ReadByte();
                this.Unk03 = br.ReadByte();
                this.BoneIndices = br.ReadInt16s(4);
                int meshesOffset = br.ReadInt32();

                br.StepIn(meshesOffset);
                {
                    this.Meshes = new List<Mesh>(meshCount);
                    for (int i = 0; i < meshCount; i++) {
                        this.Meshes.Add(new Mesh(br));
                    }
                }
                br.StepOut();
            }
        }

        public class Mesh {
            public byte MaterialIndex;
            public byte Unk01;
            public short VertexCount;
            public int IndexCount;
            public int StartVertex;
            public int StartIndex;

            internal Mesh(BinaryReaderEx br) {
                this.MaterialIndex = br.ReadByte();
                this.Unk01 = br.AssertByte(0, 1, 2);
                this.VertexCount = br.ReadInt16();
                this.IndexCount = br.ReadInt32();
                this.StartVertex = br.ReadInt32();
                this.StartIndex = br.ReadInt32();
            }
        }

        public enum VertexFormat { A, B, C }

        public class Vertex {
            public Vector3 Position;
            public Vector3 Normal;
            public Color Color;
            public Vector2[] UVs;
            public short UnkShortA;
            public short UnkShortB;
            public float UnkFloatA;
            public float UnkFloatB;

            public Vertex(Vector3 position, Vector3 normal) {
                this.Position = position;
                this.Normal = normal;
                this.UVs = new Vector2[2];
            }

            internal Vertex(BinaryReaderEx br, VertexFormat format) {
                this.Position = br.ReadVector3();
                this.Normal = Read11_11_10Vector3(br);
                this.Color = br.ReadRGBA();
                this.UVs = new Vector2[2];
                for (int i = 0; i < 2; i++) {
                    this.UVs[i] = br.ReadVector2();
                }

                if (format >= VertexFormat.B) {
                    this.UnkShortA = br.ReadInt16();
                    this.UnkShortB = br.ReadInt16();
                }

                if (format >= VertexFormat.C) {
                    this.UnkFloatA = br.ReadSingle();
                    this.UnkFloatB = br.ReadSingle();
                }
            }

            private static Vector3 Read11_11_10Vector3(BinaryReaderEx br) {
                int vector = br.ReadInt32();
                int x = vector << 21 >> 21;
                int y = vector << 10 >> 21;
                int z = vector << 0 >> 22;
                return new Vector3(x / (float)0b11_1111_1111, y / (float)0b11_1111_1111, z / (float)0b1_1111_1111);
            }
        }

        public class Struct6 {
            public Vector3 Position;
            public Vector3 Rotation;
            public int BoneIndex;

            internal Struct6(BinaryReaderEx br) {
                this.Position = br.ReadVector3();
                this.Rotation = br.ReadVector3();
                this.BoneIndex = br.ReadInt32();
                _ = br.AssertInt32(0);
            }
        }

        public class Material {
            public int Unk04;
            public int Unk08;
            public int Unk0C;
            public int DiffuseMapIndex;
            public int ReflectionMaskIndex;
            public int ReflectionMapIndex;
            public Vector4 Unk20;
            public Vector4 Unk30;
            public Vector4 Unk40;
            public float Unk60;
            public float Unk64;
            public float Unk68;
            public int Unk6C;

            internal Material(BinaryReaderEx br) {
                _ = br.AssertInt32(0);
                this.Unk04 = br.ReadInt32();
                this.Unk08 = br.ReadInt32();
                this.Unk0C = br.ReadInt32();
                this.DiffuseMapIndex = br.ReadInt32();
                this.ReflectionMaskIndex = br.ReadInt32();
                this.ReflectionMapIndex = br.ReadInt32();
                _ = br.AssertInt32(-1);
                this.Unk20 = br.ReadVector4();
                this.Unk30 = br.ReadVector4();
                this.Unk40 = br.ReadVector4();
                br.AssertPattern(0x10, 0x00);
                this.Unk60 = br.ReadSingle();
                this.Unk64 = br.ReadSingle();
                this.Unk68 = br.ReadSingle();
                this.Unk6C = br.ReadInt32();
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
