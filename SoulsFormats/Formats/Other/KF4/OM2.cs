using System.Collections.Generic;
using System.Numerics;
using SoulsFormats.Util;

namespace SoulsFormats.Formats.Other.KF4 {
    /// <summary>
    /// A 3D model format used in King's Field IV. Extension: .om2
    /// </summary>
    public class OM2 : SoulsFile<OM2> {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public Struct1[] Struct1s { get; private set; }

        public List<Struct2> Struct2s { get; set; }

        protected internal override void Read(BinaryReaderEx br) {
            _ = br.ReadInt32(); // File size
            short struct2Count = br.ReadInt16();
            _ = br.ReadInt16(); // Mesh count
            _ = br.ReadInt16(); // ? count
            _ = br.ReadInt16(); // Vertex count
            _ = br.AssertInt32(0);

            this.Struct1s = new Struct1[32];
            for (int i = 0; i < 32; i++) {
                this.Struct1s[i] = new Struct1(br);
            }

            this.Struct2s = new List<Struct2>(struct2Count);
            for (int i = 0; i < struct2Count; i++) {
                this.Struct2s.Add(new Struct2(br));
            }
        }

        public class Struct1 {
            public float Unk00 { get; set; }

            public float Unk04 { get; set; }

            public float Unk08 { get; set; }

            public byte Unk0C { get; set; }

            internal Struct1(BinaryReaderEx br) {
                this.Unk00 = br.ReadSingle();
                this.Unk04 = br.ReadSingle();
                this.Unk08 = br.ReadSingle();
                this.Unk0C = br.ReadByte();
                _ = br.AssertByte(0);
                _ = br.AssertByte(0);
                _ = br.AssertByte(0);
            }
        }

        public class Struct2 {
            public List<Mesh> Meshes { get; set; }

            public byte Unk05 { get; set; }

            public byte Struct2Index { get; set; }

            internal Struct2(BinaryReaderEx br) {
                int meshesOffset = br.ReadInt32();
                short meshCount = br.ReadInt16();
                this.Unk05 = br.ReadByte();
                this.Struct2Index = br.ReadByte();
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);

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
            public List<Vertex> Vertices { get; set; }

            internal Mesh(BinaryReaderEx br) {
                br.Skip(0xA0);
                byte vertexCount = br.ReadByte();
                br.Skip(0xF);

                this.Vertices = new List<Vertex>(vertexCount);
                for (int i = 0; i < vertexCount; i++) {
                    this.Vertices.Add(new Vertex(br));
                }

                br.Skip(0x10);
            }
        }

        public class Vertex {
            public Vector3 Position { get; set; }

            public float Unk0C { get; set; }

            public Vector3 Normal { get; set; }

            public int Unk1C { get; set; }

            public Vector3 Unk20 { get; set; }

            public int Unk2C { get; set; }

            public Vector4 Unk30 { get; set; }

            internal Vertex(BinaryReaderEx br) {
                this.Position = br.ReadVector3();
                this.Unk0C = br.ReadSingle();
                this.Normal = br.ReadVector3();
                this.Unk1C = br.ReadInt32();
                this.Unk20 = br.ReadVector3();
                this.Unk2C = br.ReadInt32();
                this.Unk30 = br.ReadVector4();
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
