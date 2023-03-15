using System.Collections.Generic;
using System.Numerics;
using SoulsFormats.Util;

namespace SoulsFormats.Formats.Other.SOM {
    /// <summary>
    /// A model format used in Sword of Moonlight for basic models like items.
    /// </summary>
    public class MDO : SoulsFile<MDO> {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public List<string> Textures;
        public List<Unk1> Unk1s;
        public List<Mesh> Meshes;

        protected internal override void Read(BinaryReaderEx br) {
            br.BigEndian = false;

            int textureCount = br.ReadInt32();
            this.Textures = new List<string>(textureCount);
            for (int i = 0; i < textureCount; i++) {
                this.Textures.Add(br.ReadShiftJIS());
            }

            br.Pad(4);

            int unk1Count = br.ReadInt32();
            this.Unk1s = new List<Unk1>(unk1Count);
            for (int i = 0; i < unk1Count; i++) {
                this.Unk1s.Add(new Unk1(br));
            }

            for (int i = 0; i < 12; i++) {
                _ = br.AssertInt32(0);
            }

            int meshCount = br.ReadInt32();
            this.Meshes = new List<Mesh>(meshCount);
            for (int i = 0; i < meshCount; i++) {
                this.Meshes.Add(new Mesh(br));
            }
        }

        public class Unk1 {
            public float Unk00, Unk04, Unk08, Unk0C, Unk10, Unk14, Unk18;

            internal Unk1(BinaryReaderEx br) {
                this.Unk00 = br.ReadSingle();
                this.Unk04 = br.ReadSingle();
                this.Unk08 = br.ReadSingle();
                this.Unk0C = br.ReadSingle();
                this.Unk10 = br.ReadSingle();
                this.Unk14 = br.ReadSingle();
                this.Unk18 = br.ReadSingle();
                _ = br.AssertInt32(0);
            }
        }

        public class Mesh {
            public int Unk00;
            public short TextureIndex;
            public short Unk06;
            public ushort[] Indices;
            public List<Vertex> Vertices;

            internal Mesh(BinaryReaderEx br) {
                this.Unk00 = br.ReadInt32();
                this.TextureIndex = br.ReadInt16();
                this.Unk06 = br.ReadInt16();
                ushort indexCount = br.ReadUInt16();
                ushort vertexCount = br.ReadUInt16();
                uint indicesOffset = br.ReadUInt32();
                uint verticesOffset = br.ReadUInt32();

                this.Indices = br.GetUInt16s(indicesOffset, indexCount);

                br.StepIn(verticesOffset);
                {
                    this.Vertices = new List<Vertex>(vertexCount);
                    for (int i = 0; i < vertexCount; i++) {
                        this.Vertices.Add(new Vertex(br));
                    }
                }
                br.StepOut();
            }

            public List<Vertex[]> GetFaces() {
                var faces = new List<Vertex[]>();
                for (int i = 0; i < this.Indices.Length; i += 3) {
                    faces.Add(new Vertex[]
                    {
                        this.Vertices[this.Indices[i + 0]],
                        this.Vertices[this.Indices[i + 1]],
                        this.Vertices[this.Indices[i + 2]],
                    });
                }
                return faces;
            }
        }

        public class Vertex {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 UV;

            internal Vertex(BinaryReaderEx br) {
                this.Position = br.ReadVector3();
                this.Normal = br.ReadVector3();
                this.UV = br.ReadVector2();
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
