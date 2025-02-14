﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SoulsFormats.Formats.FLVER;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class FLVER2 {
        /// <summary>
        /// An individual chunk of a model.
        /// </summary>
        public class Mesh : IFlverMesh {
            /// <summary>
            /// When 1, mesh is in bind pose; when 0, it isn't. Most likely has further implications.
            /// </summary>
            public byte Dynamic { get; set; }

            /// <summary>
            /// Index of the material used by all triangles in this mesh.
            /// </summary>
            public int MaterialIndex { get; set; }

            /// <summary>
            /// Apparently does nothing. Usually points to a dummy bone named after the model, possibly just for labelling.
            /// </summary>
            public int DefaultBoneIndex { get; set; }

            /// <summary>
            /// Indexes of bones in the bone collection which may be used by vertices in this mesh.
            /// </summary>
            public List<int> BoneIndices { get; set; }

            /// <summary>
            /// Triangles in this mesh.
            /// </summary>
            public List<FaceSet> FaceSets { get; set; }

            /// <summary>
            /// Vertex buffers in this mesh.
            /// </summary>
            public List<VertexBuffer> VertexBuffers { get; set; }

            /// <summary>
            /// Vertices in this mesh.
            /// </summary>
            public List<FLVER.Vertex> Vertices { get; set; }
            IReadOnlyList<FLVER.Vertex> IFlverMesh.Vertices => this.Vertices;

            /// <summary>
            /// Optional bounding box struct; may be null.
            /// </summary>
            public BoundingBoxes BoundingBox { get; set; }

            private int[] faceSetIndices, vertexBufferIndices;

            /// <summary>
            /// Creates a new Mesh with default values.
            /// </summary>
            public Mesh() {
                this.DefaultBoneIndex = -1;
                this.BoneIndices = new List<int>();
                this.FaceSets = new List<FaceSet>();
                this.VertexBuffers = new List<VertexBuffer>();
                this.Vertices = new List<FLVER.Vertex>();
            }

            internal Mesh(BinaryReaderEx br, FLVERHeader header) {
                this.Dynamic = br.AssertByte(0, 1);
                _ = br.AssertByte(0);
                _ = br.AssertByte(0);
                _ = br.AssertByte(0);

                this.MaterialIndex = br.ReadInt32();
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);
                this.DefaultBoneIndex = br.ReadInt32();
                int boneCount = br.ReadInt32();
                int boundingBoxOffset = br.ReadInt32();
                int boneOffset = br.ReadInt32();
                int faceSetCount = br.ReadInt32();
                int faceSetOffset = br.ReadInt32();
                int vertexBufferCount = br.AssertInt32(1, 2, 3);
                int vertexBufferOffset = br.ReadInt32();

                if (boundingBoxOffset != 0) {
                    br.StepIn(boundingBoxOffset);
                    {
                        this.BoundingBox = new BoundingBoxes(br, header);
                    }
                    br.StepOut();
                }

                this.BoneIndices = new List<int>(br.GetInt32s(boneOffset, boneCount));
                this.faceSetIndices = br.GetInt32s(faceSetOffset, faceSetCount);
                this.vertexBufferIndices = br.GetInt32s(vertexBufferOffset, vertexBufferCount);
            }

            internal void TakeFaceSets(Dictionary<int, FaceSet> faceSetDict) {
                this.FaceSets = new List<FaceSet>(this.faceSetIndices.Length);
                foreach (int i in this.faceSetIndices) {
                    if (!faceSetDict.ContainsKey(i)) {
                        throw new NotSupportedException("Face set not found or already taken: " + i);
                    }

                    this.FaceSets.Add(faceSetDict[i]);
                    _ = faceSetDict.Remove(i);
                }
                this.faceSetIndices = null;
            }

            internal void TakeVertexBuffers(Dictionary<int, VertexBuffer> vertexBufferDict, List<BufferLayout> layouts) {
                this.VertexBuffers = new List<VertexBuffer>(this.vertexBufferIndices.Length);
                foreach (int i in this.vertexBufferIndices) {
                    if (!vertexBufferDict.ContainsKey(i)) {
                        throw new NotSupportedException("Vertex buffer not found or already taken: " + i);
                    }

                    this.VertexBuffers.Add(vertexBufferDict[i]);
                    _ = vertexBufferDict.Remove(i);
                }
                this.vertexBufferIndices = null;

                // Make sure no semantics repeat that aren't known to
                var semantics = new List<FLVER.LayoutSemantic>();
                foreach (VertexBuffer buffer in this.VertexBuffers) {
                    foreach (FLVER.LayoutMember member in layouts[buffer.LayoutIndex]) {
                        if (member.Semantic is not FLVER.LayoutSemantic.UV
                            and not FLVER.LayoutSemantic.Tangent
                            and not FLVER.LayoutSemantic.VertexColor
                            and not FLVER.LayoutSemantic.Position
                            and not FLVER.LayoutSemantic.Normal) {
                            if (semantics.Contains(member.Semantic)) {
                                throw new NotImplementedException("Unexpected semantic list.");
                            }

                            semantics.Add(member.Semantic);
                        }
                    }
                }

                for (int i = 0; i < this.VertexBuffers.Count; i++) {
                    VertexBuffer buffer = this.VertexBuffers[i];
                    // This appears to be some kind of flag on edge-compressed vertex buffers
                    if ((buffer.BufferIndex & ~0x60000000) != i) {
                        throw new FormatException("Unexpected vertex buffer index.");
                    }
                }
            }

            internal void ReadVertices(BinaryReaderEx br, int dataOffset, List<BufferLayout> layouts, FLVERHeader header) {
                IEnumerable<FLVER.LayoutMember> layoutMembers = layouts.SelectMany(l => l);
                int uvCap = layoutMembers.Where(m => m.Semantic == FLVER.LayoutSemantic.UV).Count();
                int tanCap = layoutMembers.Where(m => m.Semantic == FLVER.LayoutSemantic.Tangent).Count();
                int colorCap = layoutMembers.Where(m => m.Semantic == FLVER.LayoutSemantic.VertexColor).Count();

                int vertexCount = this.VertexBuffers[0].VertexCount;
                this.Vertices = new List<FLVER.Vertex>(vertexCount);
                for (int i = 0; i < vertexCount; i++) {
                    this.Vertices.Add(new FLVER.Vertex(uvCap, tanCap, colorCap));
                }

                foreach (VertexBuffer buffer in this.VertexBuffers) {
                    buffer.ReadBuffer(br, layouts, this.Vertices, dataOffset, header);
                }
            }

            internal void Write(BinaryWriterEx bw, int index) {
                bw.WriteByte(this.Dynamic);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteByte(0);

                bw.WriteInt32(this.MaterialIndex);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(this.DefaultBoneIndex);
                bw.WriteInt32(this.BoneIndices.Count);
                bw.ReserveInt32($"MeshBoundingBox{index}");
                bw.ReserveInt32($"MeshBoneIndices{index}");
                bw.WriteInt32(this.FaceSets.Count);
                bw.ReserveInt32($"MeshFaceSetIndices{index}");
                bw.WriteInt32(this.VertexBuffers.Count);
                bw.ReserveInt32($"MeshVertexBufferIndices{index}");
            }

            internal void WriteBoundingBox(BinaryWriterEx bw, int index, FLVERHeader header) {
                if (this.BoundingBox == null) {
                    bw.FillInt32($"MeshBoundingBox{index}", 0);
                } else {
                    bw.FillInt32($"MeshBoundingBox{index}", (int)bw.Position);
                    this.BoundingBox.Write(bw, header);
                }
            }

            internal void WriteBoneIndices(BinaryWriterEx bw, int index, int boneIndicesStart) {
                if (this.BoneIndices.Count == 0) {
                    // Just a weird case for byte-perfect writing
                    bw.FillInt32($"MeshBoneIndices{index}", boneIndicesStart);
                } else {
                    bw.FillInt32($"MeshBoneIndices{index}", (int)bw.Position);
                    bw.WriteInt32s(this.BoneIndices);
                }
            }

            /// <summary>
            /// Returns a list of arrays of 3 vertices, each representing a triangle in the mesh.
            /// Faces are taken from the first FaceSet in the mesh with the given flags,
            /// using None by default for the highest detail mesh. If not found, the first FaceSet is used.
            /// </summary>
            public List<FLVER.Vertex[]> GetFaces(FaceSet.FSFlags fsFlags = FaceSet.FSFlags.None) {
                if (this.FaceSets.Count == 0) {
                    return new List<FLVER.Vertex[]>();
                } else {
                    FaceSet faceSet = this.FaceSets.Find(fs => fs.Flags == fsFlags) ?? this.FaceSets[0];
                    List<int> indices = faceSet.Triangulate(this.Vertices.Count < ushort.MaxValue);
                    var vertices = new List<FLVER.Vertex[]>(indices.Count);
                    for (int i = 0; i < indices.Count - 2; i += 3) {
                        int vi1 = indices[i];
                        int vi2 = indices[i + 1];
                        int vi3 = indices[i + 2];
                        vertices.Add(new FLVER.Vertex[] { this.Vertices[vi1], this.Vertices[vi2], this.Vertices[vi3] });
                    }
                    return vertices;
                }
            }

            /// <summary>
            /// An optional bounding box for meshes added in DS2.
            /// </summary>
            public class BoundingBoxes {
                /// <summary>
                /// Minimum extent of the mesh.
                /// </summary>
                public Vector3 Min { get; set; }

                /// <summary>
                /// Maximum extent of the mesh.
                /// </summary>
                public Vector3 Max { get; set; }

                /// <summary>
                /// Unknown; only present in Sekiro.
                /// </summary>
                public Vector3 Unk { get; set; }

                /// <summary>
                /// Creates a BoundingBoxes with default values.
                /// </summary>
                public BoundingBoxes() {
                    this.Min = new Vector3(float.MinValue);
                    this.Max = new Vector3(float.MaxValue);
                }

                internal BoundingBoxes(BinaryReaderEx br, FLVERHeader header) {
                    this.Min = br.ReadVector3();
                    this.Max = br.ReadVector3();
                    if (header.Version >= 0x2001A) {
                        this.Unk = br.ReadVector3();
                    }
                }

                internal void Write(BinaryWriterEx bw, FLVERHeader header) {
                    bw.WriteVector3(this.Min);
                    bw.WriteVector3(this.Max);
                    if (header.Version >= 0x2001A) {
                        bw.WriteVector3(this.Unk);
                    }
                }
            }
        }
    }
}
