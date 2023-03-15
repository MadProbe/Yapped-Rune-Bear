using System.Collections.Generic;
using System.Numerics;
using SoulsFormats.Util;

namespace SoulsFormats.Formats {
    /// <summary>
    /// Defines a dynamic grass mesh attached to a model; only used in Sekiro. Extension: .grass
    /// </summary>
    public class GRASS : SoulsFile<GRASS> {
        /// <summary>
        /// A recursive subdivision of space for efficient culling or collision testing.
        /// </summary>
        public List<Volume> BoundingVolumeHierarchy { get; set; }

        /// <summary>
        /// Points making up the grass mesh.
        /// </summary>
        public List<Vertex> Vertices { get; set; }

        /// <summary>
        /// Triangular patches of grass.
        /// </summary>
        public List<Face> Faces { get; set; }

        /// <summary>
        /// Creates an empty GRASS.
        /// </summary>
        public GRASS() {
            this.BoundingVolumeHierarchy = new List<Volume>();
            this.Vertices = new List<Vertex>();
            this.Faces = new List<Face>();
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected internal override bool Is(BinaryReaderEx br) {
            if (br.Length < 0x28) {
                return false;
            }

            int version = br.GetInt32(0);
            int headerSize = br.GetInt32(4);
            int volumeSize = br.GetInt32(8);
            int vertexSize = br.GetInt32(0x10);
            int faceSize = br.GetInt32(0x18);
            int boundingBoxSize = br.GetInt32(0x20);
            return version == 1 && headerSize == 0x28
                && volumeSize == 0x14 && vertexSize == 0x24 && faceSize == 0x18 && boundingBoxSize == 0x18;
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            br.BigEndian = false;
            _ = br.AssertInt32(1); // Version?
            _ = br.AssertInt32(0x28); // Header size
            _ = br.AssertInt32(0x14); // Struct 1 size
            int volumeCount = br.ReadInt32();
            _ = br.AssertInt32(0x24); // Vertex size
            int vertexCount = br.ReadInt32();
            _ = br.AssertInt32(0x18); // Face size
            int faceCount = br.ReadInt32();
            _ = br.AssertInt32(0x18); // Bounding box size
            _ = br.AssertInt32(volumeCount); // Bounding box count

            this.BoundingVolumeHierarchy = new List<Volume>(volumeCount);
            for (int i = 0; i < volumeCount; i++) {
                this.BoundingVolumeHierarchy.Add(new Volume(br));
            }

            this.Vertices = new List<Vertex>(vertexCount);
            for (int i = 0; i < vertexCount; i++) {
                this.Vertices.Add(new Vertex(br));
            }

            this.Faces = new List<Face>(faceCount);
            for (int i = 0; i < faceCount; i++) {
                this.Faces.Add(new Face(br));
            }

            for (int i = 0; i < volumeCount; i++) {
                this.BoundingVolumeHierarchy[i].BoundingBox = new BoundingBox(br);
            }
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected internal override void Write(BinaryWriterEx bw) {
            bw.BigEndian = false;
            bw.WriteInt32(1);
            bw.WriteInt32(0x28);
            bw.WriteInt32(0x14);
            bw.WriteInt32(this.BoundingVolumeHierarchy.Count);
            bw.WriteInt32(0x24);
            bw.WriteInt32(this.Vertices.Count);
            bw.WriteInt32(0x18);
            bw.WriteInt32(this.Faces.Count);
            bw.WriteInt32(0x18);
            bw.WriteInt32(this.BoundingVolumeHierarchy.Count);

            foreach (Volume volume in this.BoundingVolumeHierarchy) {
                volume.Write(bw);
            }

            foreach (Vertex vertex in this.Vertices) {
                vertex.Write(bw);
            }

            foreach (Face face in this.Faces) {
                face.Write(bw);
            }

            foreach (Volume volume in this.BoundingVolumeHierarchy) {
                volume.BoundingBox.Write(bw);
            }
        }

        /// <summary>
        /// A volume of space in the bounding volume hierarchy.
        /// </summary>
        public class Volume {
            /// <summary>
            /// Index of first child volume.
            /// </summary>
            public int StartChildIndex { get; set; }

            /// <summary>
            /// Index of last child volume, exclusive.
            /// </summary>
            public int EndChildIndex { get; set; }

            /// <summary>
            /// Index of first contained face.
            /// </summary>
            public int StartFaceIndex { get; set; }

            /// <summary>
            /// Index of last contained face, exclusive.
            /// </summary>
            public int EndFaceIndex { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk10 { get; set; }

            /// <summary>
            /// Space contained within the volume.
            /// </summary>
            public BoundingBox BoundingBox { get; set; }

            /// <summary>
            /// Creates a Volume with default values.
            /// </summary>
            public Volume() { }

            internal Volume(BinaryReaderEx br) {
                this.StartChildIndex = br.ReadInt32();
                this.EndChildIndex = br.ReadInt32();
                this.StartFaceIndex = br.ReadInt32();
                this.EndFaceIndex = br.ReadInt32();
                this.Unk10 = br.ReadInt32();
            }

            internal void Write(BinaryWriterEx bw) {
                bw.WriteInt32(this.StartChildIndex);
                bw.WriteInt32(this.EndChildIndex);
                bw.WriteInt32(this.StartFaceIndex);
                bw.WriteInt32(this.EndFaceIndex);
                bw.WriteInt32(this.Unk10);
            }
        }

        /// <summary>
        /// A point in the grass mesh with weights for each grass type.
        /// </summary>
        public class Vertex {
            /// <summary>
            /// Position of the vertex, relative to the parent model.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Densities of the six possible grass types; usual range is 0 to 1 but higher is supported.
            /// </summary>
            public float[] GrassDensities { get; private set; }

            /// <summary>
            /// Creates a Vertex with default values.
            /// </summary>
            public Vertex() => this.GrassDensities = new float[6];

            internal Vertex(BinaryReaderEx br) {
                this.Position = br.ReadVector3();
                this.GrassDensities = br.ReadSingles(6);
            }

            internal void Write(BinaryWriterEx bw) {
                bw.WriteVector3(this.Position);
                bw.WriteSingles(this.GrassDensities);
            }
        }

        /// <summary>
        /// A triangular patch of grass.
        /// </summary>
        public class Face {
            /// <summary>
            /// Unknown; affects direction/rotation somehow, components range from -1 to 1.
            /// </summary>
            public Vector3 Unk00 { get; set; }

            /// <summary>
            /// Index of the first vertex in the triangle.
            /// </summary>
            public int VertexIndexA { get; set; }

            /// <summary>
            /// Index of the second vertex in the triangle.
            /// </summary>
            public int VertexIndexB { get; set; }

            /// <summary>
            /// Index of the third vertex in the triangle.
            /// </summary>
            public int VertexIndexC { get; set; }

            /// <summary>
            /// Creates a Face with default values.
            /// </summary>
            public Face() { }

            internal Face(BinaryReaderEx br) {
                this.Unk00 = br.ReadVector3();
                this.VertexIndexA = br.ReadInt32();
                this.VertexIndexB = br.ReadInt32();
                this.VertexIndexC = br.ReadInt32();
            }

            internal void Write(BinaryWriterEx bw) {
                bw.WriteVector3(this.Unk00);
                bw.WriteInt32(this.VertexIndexA);
                bw.WriteInt32(this.VertexIndexB);
                bw.WriteInt32(this.VertexIndexC);
            }
        }

        /// <summary>
        /// Defines the space contained by a Volume.
        /// </summary>
        public struct BoundingBox {
            /// <summary>
            /// Minimum extent of the box.
            /// </summary>
            public Vector3 Min { get; set; }

            /// <summary>
            /// Maximum extent of the box.
            /// </summary>
            public Vector3 Max { get; set; }

            /// <summary>
            /// Creates a BoundingBox with the given bounds.
            /// </summary>
            public BoundingBox(Vector3 min, Vector3 max) {
                this.Min = min;
                this.Max = max;
            }

            internal BoundingBox(BinaryReaderEx br) {
                this.Min = br.ReadVector3();
                this.Max = br.ReadVector3();
            }

            internal void Write(BinaryWriterEx bw) {
                bw.WriteVector3(this.Min);
                bw.WriteVector3(this.Max);
            }

            /// <summary>
            /// Returns the bounds of the box as a string.
            /// </summary>
            public override string ToString() => $"{this.Min:F3} - {this.Max:F3}";
        }
    }
}
