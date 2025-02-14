﻿using System;
using System.Collections.Generic;
using System.Numerics;
using SoulsFormats.Formats.FLVER;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class FLVER0 {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class Mesh : SoulsFormats.Formats.FLVER.IFlverMesh {
            public byte Dynamic { get; set; }

            public byte MaterialIndex { get; set; }
            int IFlverMesh.MaterialIndex => this.MaterialIndex;

            public bool Unk02 { get; set; }

            public byte Unk03 { get; set; }

            public short DefaultBoneIndex { get; set; }

            public short[] BoneIndices { get; private set; }

            public short Unk46 { get; set; }

            public List<int> VertexIndices { get; set; }

            public List<FLVER.Vertex> Vertices { get; set; }
            IReadOnlyList<FLVER.Vertex> IFlverMesh.Vertices => this.Vertices;

            public int LayoutIndex { get; set; }

            internal Mesh(BinaryReaderEx br, FLVER0 flv, int dataOffset) {
                this.Dynamic = br.ReadByte();
                this.MaterialIndex = br.ReadByte();
                this.Unk02 = br.ReadBoolean();
                this.Unk03 = br.ReadByte();

                int vertexIndexCount = br.ReadInt32();
                int vertexCount = br.ReadInt32();
                this.DefaultBoneIndex = br.ReadInt16();
                this.BoneIndices = br.ReadInt16s(28);
                this.Unk46 = br.ReadInt16();
                _ = br.ReadInt32(); // Vertex indices length
                int vertexIndicesOffset = br.ReadInt32();
                int bufferDataLength = br.ReadInt32();
                int bufferDataOffset = br.ReadInt32();
                int vertexBuffersOffset1 = br.ReadInt32();
                int vertexBuffersOffset2 = br.ReadInt32();
                _ = br.AssertInt32(0);

                if (flv.VertexIndexSize == 16) {
                    this.VertexIndices = new List<int>(vertexCount);
                    foreach (ushort index in br.GetUInt16s(dataOffset + vertexIndicesOffset, vertexIndexCount)) {
                        this.VertexIndices.Add(index);
                    }
                } else if (flv.VertexIndexSize == 32) {
                    this.VertexIndices = new List<int>(br.GetInt32s(dataOffset + vertexIndicesOffset, vertexIndexCount));
                }

                VertexBuffer buffer;
                // Stupid hack for old (version F?) flvers; for example DeS o9993.
                if (vertexBuffersOffset1 == 0) {
                    buffer = new VertexBuffer() {
                        BufferLength = bufferDataLength,
                        BufferOffset = bufferDataOffset,
                        LayoutIndex = 0,
                    };
                } else {
                    br.StepIn(vertexBuffersOffset1);
                    {
                        List<VertexBuffer> vertexBuffers1 = VertexBuffer.ReadVertexBuffers(br);
                        if (vertexBuffers1.Count == 0) {
                            throw new NotSupportedException("First vertex buffer list is expected to contain at least 1 buffer.");
                        }

                        for (int i = 1; i < vertexBuffers1.Count; i++) {
                            if (vertexBuffers1[i].BufferLength != 0) {
                                throw new NotSupportedException("Vertex buffers after the first one in the first buffer list are expected to be empty.");
                            }
                        }

                        buffer = vertexBuffers1[0];
                    }
                    br.StepOut();
                }

                if (vertexBuffersOffset2 != 0) {
                    br.StepIn(vertexBuffersOffset2);
                    {
                        List<VertexBuffer> vertexBuffers2 = VertexBuffer.ReadVertexBuffers(br);
                        if (vertexBuffers2.Count != 0) {
                            throw new NotSupportedException("Second vertex buffer list is expected to contain exactly 0 buffers.");
                        }
                    }
                    br.StepOut();
                }

                br.StepIn(dataOffset + buffer.BufferOffset);
                {
                    this.LayoutIndex = buffer.LayoutIndex;
                    BufferLayout layout = flv.Materials[this.MaterialIndex].Layouts[this.LayoutIndex];

                    float uvFactor = 1024;
                    // NB hack
                    if (!br.BigEndian) {
                        uvFactor = 2048;
                    }

                    this.Vertices = new List<FLVER.Vertex>(vertexCount);
                    for (int i = 0; i < vertexCount; i++) {
                        var vert = new FLVER.Vertex();
                        vert.Read(br, layout, uvFactor);
                        this.Vertices.Add(vert);
                    }
                }
                br.StepOut();
            }

            public List<FLVER.Vertex[]> GetFaces(int version) {
                List<int> indices = this.Triangulate(version);
                var faces = new List<FLVER.Vertex[]>();
                for (int i = 0; i < indices.Count; i += 3) {
                    faces.Add(new FLVER.Vertex[]
                    {
                        this.Vertices[indices[i + 0]],
                        this.Vertices[indices[i + 1]],
                        this.Vertices[indices[i + 2]],
                    });
                }
                return faces;
            }

            public List<int> Triangulate(int version) {
                var triangles = new List<int>();
                if (version >= 0x15 && this.Unk03 == 0) {
                    triangles = new List<int>(this.VertexIndices);
                } else {
                    bool checkFlip = false;
                    bool flip = false;
                    for (int i = 0; i < this.VertexIndices.Count - 2; i++) {
                        int vi1 = this.VertexIndices[i];
                        int vi2 = this.VertexIndices[i + 1];
                        int vi3 = this.VertexIndices[i + 2];

                        if (vi1 == 0xFFFF || vi2 == 0xFFFF || vi3 == 0xFFFF) {
                            checkFlip = true;
                        } else {
                            if (vi1 != vi2 && vi1 != vi3 && vi2 != vi3) {
                                // Every time the triangle strip restarts, compare the average vertex normal to the face normal
                                // and flip the starting direction if they're pointing away from each other.
                                // I don't know why this is necessary; in most models they always restart with the same orientation
                                // as you'd expect. But on some, I can't discern any logic to it, thus this approach.
                                // It's probably hideously slow because I don't know anything about math.
                                // Feel free to hit me with a PR. :slight_smile:
                                if (checkFlip) {
                                    FLVER.Vertex v1 = this.Vertices[vi1];
                                    FLVER.Vertex v2 = this.Vertices[vi2];
                                    FLVER.Vertex v3 = this.Vertices[vi3];
                                    Vector3 n1 = v1.Normal;
                                    Vector3 n2 = v2.Normal;
                                    Vector3 n3 = v3.Normal;
                                    var vertexNormal = Vector3.Normalize((n1 + n2 + n3) / 3);
                                    var faceNormal = Vector3.Normalize(Vector3.Cross(v2.Position - v1.Position, v3.Position - v1.Position));
                                    float angle = Vector3.Dot(faceNormal, vertexNormal) / (faceNormal.Length() * vertexNormal.Length());
                                    flip = angle >= 0;
                                    checkFlip = false;
                                }

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
                    }
                }
                return triangles;
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
