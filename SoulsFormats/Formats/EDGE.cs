using System.Collections.Generic;
using System.Numerics;
using SoulsFormats.Util;

namespace SoulsFormats.Formats {
    /// <summary>
    /// A Sekiro file that defines grapple points and hangable edges for a model.
    /// </summary>
    public class EDGE : SoulsFile<EDGE> {
        /// <summary>
        /// Unknown.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Edges defined in this file.
        /// </summary>
        public List<Edge> Edges { get; set; }

        /// <summary>
        /// Creates an empty EDGE.
        /// </summary>
        public EDGE() => this.Edges = new List<Edge>();

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            br.BigEndian = false;
            _ = br.AssertInt32(4);
            int edgeCount = br.ReadInt32();
            this.ID = br.ReadInt32();
            _ = br.AssertInt32(0);

            this.Edges = new List<Edge>(edgeCount);
            for (int i = 0; i < edgeCount; i++) {
                this.Edges.Add(new Edge(br));
            }
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected internal override void Write(BinaryWriterEx bw) {
            bw.BigEndian = false;
            bw.WriteInt32(4);
            bw.WriteInt32(this.Edges.Count);
            bw.WriteInt32(this.ID);
            bw.WriteInt32(0);

            foreach (Edge edge in this.Edges) {
                edge.Write(bw);
            }
        }

        /// <summary>
        /// Which type of edge an edge is.
        /// </summary>
        public enum EdgeType : byte {
            /// <summary>
            /// A grapplable point.
            /// </summary>
            Grapple = 1,

            /// <summary>
            /// A hangable ledge.
            /// </summary>
            Hang = 2,

            /// <summary>
            /// A huggable wall.
            /// </summary>
            Hug = 3,
        }

        /// <summary>
        /// A grapple point, hangable ledge, or huggable wall.
        /// </summary>
        public class Edge {
            /// <summary>
            /// The starting point of the edge.
            /// </summary>
            public Vector3 V1 { get; set; }

            /// <summary>
            /// The ending point of the edge.
            /// </summary>
            public Vector3 V2 { get; set; }

            /// <summary>
            /// Only for wires, the point you're actually pulled towards.
            /// </summary>
            public Vector3 V3 { get; set; }

            /// <summary>
            /// Only for wires, unknown, always 1.
            /// </summary>
            public float Unk2C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk30 { get; set; }

            /// <summary>
            /// What type of edge this is.
            /// </summary>
            public EdgeType Type { get; set; }

            /// <summary>
            /// For wires, a relative ID in WireVariationParam; for walls, unknown.
            /// </summary>
            public byte VariationID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unk36 { get; set; }

            /// <summary>
            /// Creates an Edge with default values.
            /// </summary>
            public Edge() => this.Type = EdgeType.Grapple;

            /// <summary>
            /// Clones an existing Edge.
            /// </summary>
            public Edge Clone() => (Edge)this.MemberwiseClone();

            internal Edge(BinaryReaderEx br) {
                this.V1 = br.ReadVector3();
                _ = br.AssertSingle(1);
                this.V2 = br.ReadVector3();
                _ = br.AssertSingle(1);
                this.V3 = br.ReadVector3();
                this.Unk2C = br.ReadSingle();
                this.Unk30 = br.ReadInt32();
                this.Type = br.ReadEnum8<EdgeType>();
                this.VariationID = br.ReadByte();
                this.Unk36 = br.ReadByte();
                _ = br.AssertByte(0);
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);
            }

            internal void Write(BinaryWriterEx bw) {
                bw.WriteVector3(this.V1);
                bw.WriteSingle(1);
                bw.WriteVector3(this.V2);
                bw.WriteSingle(1);
                bw.WriteVector3(this.V3);
                bw.WriteSingle(this.Unk2C);
                bw.WriteInt32(this.Unk30);
                bw.WriteByte((byte)this.Type);
                bw.WriteByte(this.VariationID);
                bw.WriteByte(this.Unk36);
                bw.WriteByte(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
            }

            /// <summary>
            /// Returns relevant information about the edge as a string.
            /// </summary>
            public override string ToString() => this.Type == EdgeType.Grapple
                    ? $"{this.Type} Var:{this.VariationID} {this.Unk30} {this.Unk36} {this.V1} {this.V2} {this.V3}"
                    : $"{this.Type} Var:{this.VariationID} {this.Unk30} {this.Unk36} {this.V1} {this.V2}";
        }
    }
}
