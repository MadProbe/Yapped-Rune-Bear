using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using SoulsFormats.Util;

namespace SoulsFormats.Formats {
    /// <summary>
    /// A navigation format used in DeS and DS1 that defines a coarse graph for moving around the map. Extension: .mcg
    /// </summary>
    public class MCG : SoulsFile<MCG> {
        /// <summary>
        /// True for DeS, false for DS1.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// Unknown; possibly uninitialized memory.
        /// </summary>
        public int Unk04 { get; set; }

        /// <summary>
        /// Vertices of the navigation graph.
        /// </summary>
        public List<Node> Nodes { get; set; }

        /// <summary>
        /// Edges of the navigation graph.
        /// </summary>
        public List<Edge> Edges { get; set; }

        /// <summary>
        /// Unknown; possibly uninitialized memory.
        /// </summary>
        public int Unk18 { get; set; }

        /// <summary>
        /// Unknown; possibly uninitialized memory.
        /// </summary>
        public int Unk1C { get; set; }

        /// <summary>
        /// Creates an empty MCG.
        /// </summary>
        public MCG() {
            this.Nodes = new List<Node>();
            this.Edges = new List<Edge>();
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            br.BigEndian = true;
            this.BigEndian = br.AssertInt32(1, 0x1000000) == 1;
            br.BigEndian = this.BigEndian;
            this.Unk04 = br.ReadInt32();
            int nodeCount = br.ReadInt32();
            int nodesOffset = br.ReadInt32();
            int edgeCount = br.ReadInt32();
            int edgesOffset = br.ReadInt32();
            this.Unk18 = br.ReadInt32();
            this.Unk1C = br.ReadInt32();

            br.Position = nodesOffset;
            this.Nodes = new List<Node>(nodeCount);
            for (int i = 0; i < nodeCount; i++) {
                this.Nodes.Add(new Node(br));
            }

            br.Position = edgesOffset;
            this.Edges = new List<Edge>(edgeCount);
            for (int i = 0; i < edgeCount; i++) {
                this.Edges.Add(new Edge(br));
            }
        }

        /// <summary>
        /// Verifies that there are no null references or invalid indices.
        /// </summary>
        public override bool Validate(out Exception ex) {
            if (!ValidateNull(this.Nodes, $"{nameof(this.Nodes)} may not be null.", out ex)
                || !ValidateNull(this.Edges, $"{nameof(this.Edges)} may not be null.", out ex)) {
                return false;
            }

            for (int i = 0; i < this.Nodes.Count; i++) {
                Node node = this.Nodes[i];
                if (!ValidateNull(node, $"{nameof(this.Nodes)}[{i}]: {nameof(Node)} may not be null.", out ex)
                    || !ValidateNull(node.ConnectedNodeIndices, $"{nameof(this.Nodes)}[{i}]: {nameof(Node.ConnectedNodeIndices)} may not be null.", out ex)
                    || !ValidateNull(node.ConnectedEdgeIndices, $"{nameof(this.Nodes)}[{i}]: {nameof(Node.ConnectedEdgeIndices)} may not be null.", out ex)) {
                    return false;
                }

                if (node.ConnectedNodeIndices.Count != node.ConnectedEdgeIndices.Count) {
                    ex = new InvalidDataException($"{nameof(this.Nodes)}[{i}]: {nameof(Node.ConnectedNodeIndices)} count must equal {nameof(Node.ConnectedEdgeIndices)} count.");
                    return false;
                }

                for (int j = 0; j < node.ConnectedNodeIndices.Count; j++) {
                    int nodeIndex = node.ConnectedNodeIndices[j];
                    int edgeIndex = node.ConnectedEdgeIndices[j];
                    if (!ValidateIndex(this.Nodes.Count, nodeIndex, $"{nameof(this.Nodes)}[{i}].{nameof(Node.ConnectedNodeIndices)}[{j}]: Index out of range: {nodeIndex}", out ex)
                        || !ValidateIndex(this.Edges.Count, edgeIndex, $"{nameof(this.Nodes)}[{i}].{nameof(Node.ConnectedEdgeIndices)}[{j}]: Index out of range: {edgeIndex}", out ex)) {
                        return false;
                    }
                }
            }

            for (int i = 0; i < this.Edges.Count; i++) {
                Edge edge = this.Edges[i];
                if (!ValidateNull(edge, $"{nameof(this.Edges)}[{i}]: {nameof(Edge)} may not be null.", out ex)
                    || !ValidateNull(edge.UnkIndicesA, $"{nameof(this.Edges)}[{i}]: {nameof(Edge.UnkIndicesA)} may not be null.", out ex)
                    || !ValidateNull(edge.UnkIndicesB, $"{nameof(this.Edges)}[{i}]: {nameof(Edge.UnkIndicesB)} may not be null.", out ex)
                    || !ValidateIndex(this.Nodes.Count, edge.NodeIndexA, $"{nameof(this.Edges)}[{i}].{nameof(Edge.NodeIndexA)}: Index out of range: {edge.NodeIndexA}", out ex)
                    || !ValidateIndex(this.Nodes.Count, edge.NodeIndexB, $"{nameof(this.Edges)}[{i}].{nameof(Edge.NodeIndexB)}: Index out of range: {edge.NodeIndexB}", out ex)) {
                    return false;
                }
            }

            ex = null;
            return true;
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected internal override void Write(BinaryWriterEx bw) {
            bw.BigEndian = this.BigEndian;
            bw.WriteInt32(1);
            bw.WriteInt32(this.Unk04);
            bw.WriteInt32(this.Nodes.Count);
            bw.ReserveInt32("NodesOffset");
            bw.WriteInt32(this.Edges.Count);
            bw.ReserveInt32("EdgesOffset");
            bw.WriteInt32(this.Unk18);
            bw.WriteInt32(this.Unk1C);

            long[] edgeIndicesAOffsets = new long[this.Edges.Count];
            long[] edgeIndicesBOffsets = new long[this.Edges.Count];
            for (int i = 0; i < this.Edges.Count; i++) {
                edgeIndicesAOffsets[i] = bw.Position;
                bw.WriteInt32s(this.Edges[i].UnkIndicesA);
                edgeIndicesBOffsets[i] = bw.Position;
                bw.WriteInt32s(this.Edges[i].UnkIndicesB);
            }

            long[] nodeNodeIndicesOffsets = new long[this.Nodes.Count];
            long[] nodeEdgeIndicesOffsets = new long[this.Nodes.Count];
            for (int i = 0; i < this.Nodes.Count; i++) {
                Node node = this.Nodes[i];
                nodeNodeIndicesOffsets[i] = node.ConnectedNodeIndices.Count == 0 ? 0 : bw.Position;
                bw.WriteInt32s(node.ConnectedNodeIndices);
                nodeEdgeIndicesOffsets[i] = node.ConnectedEdgeIndices.Count == 0 ? 0 : bw.Position;
                bw.WriteInt32s(node.ConnectedEdgeIndices);
            }

            bw.FillInt32("EdgesOffset", (int)bw.Position);
            for (int i = 0; i < this.Edges.Count; i++) {
                this.Edges[i].Write(bw, edgeIndicesAOffsets[i], edgeIndicesBOffsets[i]);
            }

            bw.FillInt32("NodesOffset", (int)bw.Position);
            for (int i = 0; i < this.Nodes.Count; i++) {
                this.Nodes[i].Write(bw, nodeNodeIndicesOffsets[i], nodeEdgeIndicesOffsets[i]);
            }
        }

        /// <summary>
        /// A vertex in the map navigation graph.
        /// </summary>
        public class Node {
            /// <summary>
            /// Coordinates of the node.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Indices of connected nodes; parallel to ConnectedEdgeIndices.
            /// </summary>
            public List<int> ConnectedNodeIndices { get; set; }

            /// <summary>
            /// Edges leading to connected nodes; parallel to ConnectedNodeIndices.
            /// </summary>
            public List<int> ConnectedEdgeIndices { get; set; }

            /// <summary>
            /// Unknown; possibly an index of another node, may be -1.
            /// </summary>
            public int Unk18 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk1C { get; set; }

            /// <summary>
            /// Creates a Node with default values.
            /// </summary>
            public Node() {
                this.ConnectedNodeIndices = new List<int>();
                this.ConnectedEdgeIndices = new List<int>();
                this.Unk18 = -1;
            }

            internal Node(BinaryReaderEx br) {
                int connectionCount = br.ReadInt32();
                this.Position = br.ReadVector3();
                int nodeIndicesOffset = br.ReadInt32();
                int edgeIndicesOffset = br.ReadInt32();
                this.Unk18 = br.ReadInt32();
                this.Unk1C = br.ReadInt32();

                this.ConnectedNodeIndices = new List<int>(br.GetInt32s(nodeIndicesOffset, connectionCount));
                this.ConnectedEdgeIndices = new List<int>(br.GetInt32s(edgeIndicesOffset, connectionCount));
            }

            internal void Write(BinaryWriterEx bw, long nodeIndicesOffset, long edgeIndicesOffset) {
                bw.WriteInt32(this.ConnectedNodeIndices.Count);
                bw.WriteVector3(this.Position);
                bw.WriteInt32((int)nodeIndicesOffset);
                bw.WriteInt32((int)edgeIndicesOffset);
                bw.WriteInt32(this.Unk18);
                bw.WriteInt32(this.Unk1C);
            }
        }

        /// <summary>
        /// A connection between two nodes.
        /// </summary>
        public class Edge {
            /// <summary>
            /// Index of one of the endpoints of the edge.
            /// </summary>
            public int NodeIndexA { get; set; }

            /// <summary>
            /// Unknown; not indices of anything in MCG or MCP.
            /// </summary>
            public List<int> UnkIndicesA { get; set; }

            /// <summary>
            /// Index of the other endpoint of the edge.
            /// </summary>
            public int NodeIndexB { get; set; }

            /// <summary>
            /// Unknown; not indices of anything in MCG or MCP.
            /// </summary>
            public List<int> UnkIndicesB { get; set; }

            /// <summary>
            /// Index of the room in the corresponding MCP file containing this edge.
            /// </summary>
            public int MCPRoomIndex { get; set; }

            /// <summary>
            /// The ID of the map the edge is in, where mAA_BB_CC_DD is packed into bytes AABBCCDD of the uint.
            /// </summary>
            public uint MapID { get; set; }

            /// <summary>
            /// Unknown, presumably a weight.
            /// </summary>
            public float Unk20 { get; set; }

            /// <summary>
            /// Creates an Edge with default values.
            /// </summary>
            public Edge() {
                this.UnkIndicesA = new List<int>();
                this.UnkIndicesB = new List<int>();
            }

            internal Edge(BinaryReaderEx br) {
                this.NodeIndexA = br.ReadInt32();
                int indexCountA = br.ReadInt32();
                int indicesOffsetA = br.ReadInt32();
                this.NodeIndexB = br.ReadInt32();
                int indexCountB = br.ReadInt32();
                int indicesOffsetB = br.ReadInt32();
                this.MCPRoomIndex = br.ReadInt32();
                this.MapID = br.ReadUInt32();
                this.Unk20 = br.ReadSingle();

                this.UnkIndicesA = new List<int>(br.GetInt32s(indicesOffsetA, indexCountA));
                this.UnkIndicesB = new List<int>(br.GetInt32s(indicesOffsetB, indexCountB));
            }

            internal void Write(BinaryWriterEx bw, long indicesOffsetA, long indicesOffsetB) {
                bw.WriteInt32(this.NodeIndexA);
                bw.WriteInt32(this.UnkIndicesA.Count);
                bw.WriteInt32((int)indicesOffsetA);
                bw.WriteInt32(this.NodeIndexB);
                bw.WriteInt32(this.UnkIndicesB.Count);
                bw.WriteInt32((int)indicesOffsetB);
                bw.WriteInt32(this.MCPRoomIndex);
                bw.WriteUInt32(this.MapID);
                bw.WriteSingle(this.Unk20);
            }
        }
    }
}
