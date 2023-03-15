using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using SoulsFormats.Util;

namespace SoulsFormats.Formats {
    /// <summary>
    /// A file that defines the placement and properties of navmeshes in BB, DS3, and Sekiro. Extension: .nva
    /// </summary>
    public class NVA : SoulsFile<NVA> {
        /// <summary>
        /// Version of the overall format.
        /// </summary>
        public enum NVAVersion : uint {
            /// <summary>
            /// Used for a single BB test map, m29_03_10_00; has no Section8
            /// </summary>
            OldBloodborne = 3,

            /// <summary>
            /// Dark Souls 3 and Bloodborne
            /// </summary>
            DarkSouls3 = 4,

            /// <summary>
            /// Sekiro
            /// </summary>
            Sekiro = 5,
        }

        /// <summary>
        /// The format version of this file.
        /// </summary>
        public NVAVersion Version { get; set; }

        /// <summary>
        /// Navmesh instances in the map.
        /// </summary>
        public NavmeshSection Navmeshes { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public Section1 Entries1 { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public Section2 Entries2 { get; set; }

        /// <summary>
        /// Connections between different navmeshes.
        /// </summary>
        public ConnectorSection Connectors { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public Section7 Entries7 { get; set; }

        /// <summary>
        /// Creates an empty NVA formatted for DS3.
        /// </summary>
        public NVA() {
            this.Version = NVAVersion.DarkSouls3;
            this.Navmeshes = new NavmeshSection(2);
            this.Entries1 = new Section1();
            this.Entries2 = new Section2();
            this.Connectors = new ConnectorSection();
            this.Entries7 = new Section7();
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected internal override bool Is(BinaryReaderEx br) {
            if (br.Length < 4) {
                return false;
            }

            string magic = br.GetASCII(0, 4);
            return magic == "NVMA";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            br.BigEndian = false;
            _ = br.AssertASCII("NVMA");
            this.Version = br.ReadEnum32<NVAVersion>();
            _ = br.ReadUInt32(); // File size
            _ = br.AssertInt32(this.Version == NVAVersion.OldBloodborne ? 8 : 9); // Section count

            this.Navmeshes = new NavmeshSection(br);
            this.Entries1 = new Section1(br);
            this.Entries2 = new Section2(br);
            _ = new Section3(br);
            this.Connectors = new ConnectorSection(br);
            var connectorPoints = new ConnectorPointSection(br);
            var connectorConditions = new ConnectorConditionSection(br);
            this.Entries7 = new Section7(br);
            MapNodeSection mapNodes = this.Version == NVAVersion.OldBloodborne ? new MapNodeSection(1) : new MapNodeSection(br);
            foreach (Navmesh navmesh in this.Navmeshes) {
                navmesh.TakeMapNodes(mapNodes);
            }

            foreach (Connector connector in this.Connectors) {
                connector.TakePointsAndConds(connectorPoints, connectorConditions);
            }
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected internal override void Write(BinaryWriterEx bw) {
            var connectorPoints = new ConnectorPointSection();
            var connectorConditions = new ConnectorConditionSection();
            foreach (Connector connector in this.Connectors) {
                connector.GivePointsAndConds(connectorPoints, connectorConditions);
            }

            var mapNodes = new MapNodeSection(this.Version == NVAVersion.Sekiro ? 2 : 1);
            foreach (Navmesh navmesh in this.Navmeshes) {
                navmesh.GiveMapNodes(mapNodes);
            }

            bw.BigEndian = false;
            bw.WriteASCII("NVMA");
            bw.WriteUInt32((uint)this.Version);
            bw.ReserveUInt32("FileSize");
            bw.WriteInt32(this.Version == NVAVersion.OldBloodborne ? 8 : 9);

            this.Navmeshes.Write(bw, 0);
            this.Entries1.Write(bw, 1);
            this.Entries2.Write(bw, 2);
            new Section3().Write(bw, 3);
            this.Connectors.Write(bw, 4);
            connectorPoints.Write(bw, 5);
            connectorConditions.Write(bw, 6);
            this.Entries7.Write(bw, 7);
            if (this.Version != NVAVersion.OldBloodborne) {
                mapNodes.Write(bw, 8);
            }

            bw.FillUInt32("FileSize", (uint)bw.Position);
        }

        /// <summary>
        /// NVA is split up into 8 lists of different types.
        /// </summary>
        public abstract class Section<T> : List<T> {
            /// <summary>
            /// A version number indicating the format of the section. Don't change this unless you know what you're doing.
            /// </summary>
            public int Version { get; set; }

            internal Section(int version) : base() => this.Version = version;

            internal Section(BinaryReaderEx br, int index, params int[] versions) : base() {
                _ = br.AssertInt32(index);
                this.Version = br.AssertInt32(versions);
                int length = br.ReadInt32();
                int count = br.ReadInt32();
                this.Capacity = count;

                long start = br.Position;
                this.ReadEntries(br, count);
                br.Position = start + length;
            }

            internal abstract void ReadEntries(BinaryReaderEx br, int count);

            internal void Write(BinaryWriterEx bw, int index) {
                bw.WriteInt32(index);
                bw.WriteInt32(this.Version);
                bw.ReserveInt32("SectionLength");
                bw.WriteInt32(this.Count);

                long start = bw.Position;
                this.WriteEntries(bw);
                if (bw.Position % 0x10 != 0) {
                    bw.WritePattern(0x10 - (int)bw.Position % 0x10, 0xFF);
                }

                bw.FillInt32("SectionLength", (int)(bw.Position - start));
            }

            internal abstract void WriteEntries(BinaryWriterEx bw);
        }

        /// <summary>
        /// A list of navmesh instances. Version: 2 for DS3 and the BB test map, 3 for BB, 4 for Sekiro.
        /// </summary>
        public class NavmeshSection : Section<Navmesh> {
            /// <summary>
            /// Creates an empty NavmeshSection with the given version.
            /// </summary>
            public NavmeshSection(int version) : base(version) { }

            internal NavmeshSection(BinaryReaderEx br) : base(br, 0, 2, 3, 4) { }

            internal override void ReadEntries(BinaryReaderEx br, int count) {
                for (int i = 0; i < count; i++) {
                    this.Add(new Navmesh(br, this.Version));
                }
            }

            internal override void WriteEntries(BinaryWriterEx bw) {
                for (int i = 0; i < this.Count; i++) {
                    this[i].Write(bw, this.Version, i);
                }

                for (int i = 0; i < this.Count; i++) {
                    this[i].WriteNameRefs(bw, this.Version, i);
                }
            }
        }

        /// <summary>
        /// An instance of a navmesh.
        /// </summary>
        public class Navmesh {
            /// <summary>
            /// Position of the mesh.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Rotation of the mesh, in radians.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// Scale of the mesh.
            /// </summary>
            public Vector3 Scale { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int NameID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int ModelID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk38 { get; set; }

            /// <summary>
            /// Should equal number of vertices in the model file.
            /// </summary>
            public int VertexCount { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<int> NameReferenceIDs { get; set; }

            /// <summary>
            /// Adjacent nodes in an inter-navmesh graph.
            /// </summary>
            public List<MapNode> MapNodes { get; set; }

            /// <summary>
            /// Unknown
            /// </summary>
            public bool Unk4C { get; set; }

            private short MapNodesIndex;
            private short MapNodeCount;

            /// <summary>
            /// Creates a Navmesh with default values.
            /// </summary>
            public Navmesh() {
                this.Scale = Vector3.One;
                this.NameReferenceIDs = new List<int>();
                this.MapNodes = new List<MapNode>();
            }

            internal Navmesh(BinaryReaderEx br, int version) {
                this.Position = br.ReadVector3();
                _ = br.AssertSingle(1);
                this.Rotation = br.ReadVector3();
                _ = br.AssertInt32(0);
                this.Scale = br.ReadVector3();
                _ = br.AssertInt32(0);
                this.NameID = br.ReadInt32();
                this.ModelID = br.ReadInt32();
                this.Unk38 = br.ReadInt32();
                _ = br.AssertInt32(0);
                this.VertexCount = br.ReadInt32();
                int nameRefCount = br.ReadInt32();
                this.MapNodesIndex = br.ReadInt16();
                this.MapNodeCount = br.ReadInt16();
                this.Unk4C = br.AssertInt32(0, 1) == 1;

                if (version < 4) {
                    if (nameRefCount > 16) {
                        throw new InvalidDataException("Name reference count should not exceed 16 in DS3/BB.");
                    }

                    this.NameReferenceIDs = new List<int>(br.ReadInt32s(nameRefCount));
                    for (int i = 0; i < 16 - nameRefCount; i++) {
                        _ = br.AssertInt32(-1);
                    }
                } else {
                    int nameRefOffset = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    this.NameReferenceIDs = new List<int>(br.GetInt32s(nameRefOffset, nameRefCount));
                }
            }

            internal void TakeMapNodes(MapNodeSection entries8) {
                this.MapNodes = new List<MapNode>(this.MapNodeCount);
                for (int i = 0; i < this.MapNodeCount; i++) {
                    this.MapNodes.Add(entries8[this.MapNodesIndex + i]);
                }

                this.MapNodeCount = -1;

                foreach (MapNode mapNode in this.MapNodes) {
                    if (mapNode.SiblingDistances.Count > this.MapNodes.Count) {
                        mapNode.SiblingDistances.RemoveRange(this.MapNodes.Count, mapNode.SiblingDistances.Count - this.MapNodes.Count);
                    }
                }
            }

            internal void Write(BinaryWriterEx bw, int version, int index) {
                bw.WriteVector3(this.Position);
                bw.WriteSingle(1);
                bw.WriteVector3(this.Rotation);
                bw.WriteInt32(0);
                bw.WriteVector3(this.Scale);
                bw.WriteInt32(0);
                bw.WriteInt32(this.NameID);
                bw.WriteInt32(this.ModelID);
                bw.WriteInt32(this.Unk38);
                bw.WriteInt32(0);
                bw.WriteInt32(this.VertexCount);
                bw.WriteInt32(this.NameReferenceIDs.Count);
                bw.WriteInt16(this.MapNodesIndex);
                bw.WriteInt16((short)this.MapNodes.Count);
                bw.WriteInt32(this.Unk4C ? 1 : 0);

                if (version < 4) {
                    if (this.NameReferenceIDs.Count > 16) {
                        throw new InvalidDataException("Name reference count should not exceed 16 in DS3/BB.");
                    }

                    bw.WriteInt32s(this.NameReferenceIDs);
                    for (int i = 0; i < 16 - this.NameReferenceIDs.Count; i++) {
                        bw.WriteInt32(-1);
                    }
                } else {
                    bw.ReserveInt32($"NameRefOffset{index}");
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            internal void WriteNameRefs(BinaryWriterEx bw, int version, int index) {
                if (version >= 4) {
                    bw.FillInt32($"NameRefOffset{index}", (int)bw.Position);
                    bw.WriteInt32s(this.NameReferenceIDs);
                }
            }

            internal void GiveMapNodes(MapNodeSection mapNodes) {
                // Sometimes when the map node count is 0 the index is also 0,
                // but usually this is accurate.
                this.MapNodesIndex = (short)mapNodes.Count;
                mapNodes.AddRange(this.MapNodes);
            }

            /// <summary>
            /// Returns a string representation of the navmesh.
            /// </summary>
            public override string ToString() => $"{this.NameID} {this.Position} {this.Rotation} [{this.NameReferenceIDs.Count} References] [{this.MapNodes.Count} MapNodes]";
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        public class Section1 : Section<Entry1> {
            /// <summary>
            /// Creates an empty Section1.
            /// </summary>
            public Section1() : base(1) { }

            internal Section1(BinaryReaderEx br) : base(br, 1, 1) { }

            internal override void ReadEntries(BinaryReaderEx br, int count) {
                for (int i = 0; i < count; i++) {
                    this.Add(new Entry1(br));
                }
            }

            internal override void WriteEntries(BinaryWriterEx bw) {
                foreach (Entry1 entry in this) {
                    entry.Write(bw);
                }
            }
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        public class Entry1 {
            /// <summary>
            /// Unknown; always 0 in DS3 and SDT, sometimes 1 in BB.
            /// </summary>
            public int Unk00 { get; set; }

            /// <summary>
            /// Creates an Entry1 with default values.
            /// </summary>
            public Entry1() { }

            internal Entry1(BinaryReaderEx br) {
                this.Unk00 = br.ReadInt32();
                _ = br.AssertInt32(0);
            }

            internal void Write(BinaryWriterEx bw) {
                bw.WriteInt32(this.Unk00);
                bw.WriteInt32(0);
            }

            /// <summary>
            /// Returns a string representation of the entry.
            /// </summary>
            public override string ToString() => $"{this.Unk00}";
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        public class Section2 : Section<Entry2> {
            /// <summary>
            /// Creates an empty Section2.
            /// </summary>
            public Section2() : base(1) { }

            internal Section2(BinaryReaderEx br) : base(br, 2, 1) { }

            internal override void ReadEntries(BinaryReaderEx br, int count) {
                for (int i = 0; i < count; i++) {
                    this.Add(new Entry2(br));
                }
            }

            internal override void WriteEntries(BinaryWriterEx bw) {
                foreach (Entry2 entry in this) {
                    entry.Write(bw);
                }
            }
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        public class Entry2 {
            /// <summary>
            /// Unknown; seems to just be the index of this entry.
            /// </summary>
            public int Unk00 { get; set; }

            /// <summary>
            /// References in this entry; maximum of 64.
            /// </summary>
            public List<Reference> References { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk08 { get; set; }

            /// <summary>
            /// Creates an Entry2 with default values.
            /// </summary>
            public Entry2() {
                this.References = new List<Reference>();
                this.Unk08 = -1;
            }

            internal Entry2(BinaryReaderEx br) {
                this.Unk00 = br.ReadInt32();
                int referenceCount = br.ReadInt32();
                this.Unk08 = br.ReadInt32();
                _ = br.AssertInt32(0);
                if (referenceCount > 64) {
                    throw new InvalidDataException("Entry2 reference count should not exceed 64.");
                }

                this.References = new List<Reference>(referenceCount);
                for (int i = 0; i < referenceCount; i++) {
                    this.References.Add(new Reference(br));
                }

                for (int i = 0; i < 64 - referenceCount; i++) {
                    _ = br.AssertInt64(0);
                }
            }

            internal void Write(BinaryWriterEx bw) {
                bw.WriteInt32(this.Unk00);
                bw.WriteInt32(this.References.Count);
                bw.WriteInt32(this.Unk08);
                bw.WriteInt32(0);
                if (this.References.Count > 64) {
                    throw new InvalidDataException("Entry2 reference count should not exceed 64.");
                }

                foreach (Reference reference in this.References) {
                    reference.Write(bw);
                }

                for (int i = 0; i < 64 - this.References.Count; i++) {
                    bw.WriteInt64(0);
                }
            }

            /// <summary>
            /// Returns a string representation of the entry.
            /// </summary>
            public override string ToString() => $"{this.Unk00} {this.Unk08} [{this.References.Count} References]";

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Reference {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkIndex { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int NameID { get; set; }

                /// <summary>
                /// Creates a Reference with defalt values.
                /// </summary>
                public Reference() { }

                internal Reference(BinaryReaderEx br) {
                    this.UnkIndex = br.ReadInt32();
                    this.NameID = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw) {
                    bw.WriteInt32(this.UnkIndex);
                    bw.WriteInt32(this.NameID);
                }

                /// <summary>
                /// Returns a string representation of the reference.
                /// </summary>
                public override string ToString() => $"{this.UnkIndex} {this.NameID}";
            }
        }

        private class Section3 : Section<Entry3> {
            public Section3() : base(1) { }

            internal Section3(BinaryReaderEx br) : base(br, 3, 1) { }

            internal override void ReadEntries(BinaryReaderEx br, int count) {
                for (int i = 0; i < count; i++) {
                    this.Add(new Entry3(br));
                }
            }

            internal override void WriteEntries(BinaryWriterEx bw) {
                foreach (Entry3 entry in this) {
                    entry.Write(bw);
                }
            }
        }

        private class Entry3 {
            internal Entry3(BinaryReaderEx br) => throw new NotImplementedException("Section3 is empty in all known NVAs.");

            internal void Write(BinaryWriterEx bw) => throw new NotImplementedException("Section3 is empty in all known NVAs.");
        }

        /// <summary>
        /// A list of connections between navmeshes.
        /// </summary>
        public class ConnectorSection : Section<Connector> {
            /// <summary>
            /// Creates an empty ConnectorSection.
            /// </summary>
            public ConnectorSection() : base(1) { }

            internal ConnectorSection(BinaryReaderEx br) : base(br, 4, 1) { }

            internal override void ReadEntries(BinaryReaderEx br, int count) {
                for (int i = 0; i < count; i++) {
                    this.Add(new Connector(br));
                }
            }

            internal override void WriteEntries(BinaryWriterEx bw) {
                foreach (Connector entry in this) {
                    entry.Write(bw);
                }
            }
        }

        /// <summary>
        /// A connection between two navmeshes.
        /// </summary>
        public class Connector {
            /// <summary>
            /// Unknown.
            /// </summary>
            public int MainNameID { get; set; }

            /// <summary>
            /// The navmesh to be attached.
            /// </summary>
            public int TargetNameID { get; set; }

            /// <summary>
            /// Points used by this connection.
            /// </summary>
            public List<ConnectorPoint> Points { get; set; }

            /// <summary>
            /// Conditions used by this connection.
            /// </summary>
            public List<ConnectorCondition> Conditions { get; set; }

            private int PointCount;
            private int ConditionCount;
            private int PointsIndex;
            private int ConditionsIndex;

            /// <summary>
            /// Creates a Connector with default values.
            /// </summary>
            public Connector() {
                this.Points = new List<ConnectorPoint>();
                this.Conditions = new List<ConnectorCondition>();
            }

            internal Connector(BinaryReaderEx br) {
                this.MainNameID = br.ReadInt32();
                this.TargetNameID = br.ReadInt32();
                this.PointCount = br.ReadInt32();
                this.ConditionCount = br.ReadInt32();
                this.PointsIndex = br.ReadInt32();
                _ = br.AssertInt32(0);
                this.ConditionsIndex = br.ReadInt32();
                _ = br.AssertInt32(0);
            }

            internal void TakePointsAndConds(ConnectorPointSection points, ConnectorConditionSection conds) {
                this.Points = new List<ConnectorPoint>(this.PointCount);
                for (int i = 0; i < this.PointCount; i++) {
                    this.Points.Add(points[this.PointsIndex + i]);
                }

                this.PointCount = -1;

                this.Conditions = new List<ConnectorCondition>(this.ConditionCount);
                for (int i = 0; i < this.ConditionCount; i++) {
                    this.Conditions.Add(conds[this.ConditionsIndex + i]);
                }

                this.ConditionCount = -1;
            }

            internal void GivePointsAndConds(ConnectorPointSection points, ConnectorConditionSection conds) {
                this.PointsIndex = points.Count;
                points.AddRange(this.Points);

                this.ConditionsIndex = conds.Count;
                conds.AddRange(this.Conditions);
            }

            internal void Write(BinaryWriterEx bw) {
                bw.WriteInt32(this.MainNameID);
                bw.WriteInt32(this.TargetNameID);
                bw.WriteInt32(this.Points.Count);
                bw.WriteInt32(this.Conditions.Count);
                bw.WriteInt32(this.PointsIndex);
                bw.WriteInt32(0);
                bw.WriteInt32(this.ConditionsIndex);
                bw.WriteInt32(0);
            }

            /// <summary>
            /// Returns a string representation of the connector.
            /// </summary>
            public override string ToString() => $"{this.MainNameID} -> {this.TargetNameID} [{this.Points.Count} Points][{this.Conditions.Count} Conditions]";
        }

        /// <summary>
        /// A list of points used to connect navmeshes.
        /// </summary>
        internal class ConnectorPointSection : Section<ConnectorPoint> {
            /// <summary>
            /// Creates an empty ConnectorPointSection.
            /// </summary>
            public ConnectorPointSection() : base(1) { }

            internal ConnectorPointSection(BinaryReaderEx br) : base(br, 5, 1) { }

            internal override void ReadEntries(BinaryReaderEx br, int count) {
                for (int i = 0; i < count; i++) {
                    this.Add(new ConnectorPoint(br));
                }
            }

            internal override void WriteEntries(BinaryWriterEx bw) {
                foreach (ConnectorPoint entry in this) {
                    entry.Write(bw);
                }
            }
        }

        /// <summary>
        /// A point used to connect two navmeshes.
        /// </summary>
        public class ConnectorPoint {
            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk00 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk04 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk08 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk0C { get; set; }

            /// <summary>
            /// Creates a ConnectorPoint with default values.
            /// </summary>
            public ConnectorPoint() { }

            internal ConnectorPoint(BinaryReaderEx br) {
                this.Unk00 = br.ReadInt32();
                this.Unk04 = br.ReadInt32();
                this.Unk08 = br.ReadInt32();
                this.Unk0C = br.ReadInt32();
            }

            internal void Write(BinaryWriterEx bw) {
                bw.WriteInt32(this.Unk00);
                bw.WriteInt32(this.Unk04);
                bw.WriteInt32(this.Unk08);
                bw.WriteInt32(this.Unk0C);
            }

            /// <summary>
            /// Returns a string representation of the point.
            /// </summary>
            public override string ToString() => $"{this.Unk00} {this.Unk04} {this.Unk08} {this.Unk0C}";
        }

        /// <summary>
        /// A list of unknown conditions used by connectors.
        /// </summary>
        internal class ConnectorConditionSection : Section<ConnectorCondition> {
            /// <summary>
            /// Creates an empty ConnectorConditionSection.
            /// </summary>
            public ConnectorConditionSection() : base(1) { }

            internal ConnectorConditionSection(BinaryReaderEx br) : base(br, 6, 1) { }

            internal override void ReadEntries(BinaryReaderEx br, int count) {
                for (int i = 0; i < count; i++) {
                    this.Add(new ConnectorCondition(br));
                }
            }

            internal override void WriteEntries(BinaryWriterEx bw) {
                foreach (ConnectorCondition entry in this) {
                    entry.Write(bw);
                }
            }
        }

        /// <summary>
        /// An unknown condition used by a connector.
        /// </summary>
        public class ConnectorCondition {
            /// <summary>
            /// Unknown.
            /// </summary>
            public int Condition1 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Condition2 { get; set; }

            /// <summary>
            /// Creates a ConnectorCondition with default values.
            /// </summary>
            public ConnectorCondition() { }

            internal ConnectorCondition(BinaryReaderEx br) {
                this.Condition1 = br.ReadInt32();
                this.Condition2 = br.ReadInt32();
            }

            internal void Write(BinaryWriterEx bw) {
                bw.WriteInt32(this.Condition1);
                bw.WriteInt32(this.Condition2);
            }

            /// <summary>
            /// Returns a string representation of the condition.
            /// </summary>
            public override string ToString() => $"{this.Condition1} {this.Condition2}";
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        public class Section7 : Section<Entry7> {
            /// <summary>
            /// Creates an empty Section7.
            /// </summary>
            public Section7() : base(1) { }

            internal Section7(BinaryReaderEx br) : base(br, 7, 1) { }

            internal override void ReadEntries(BinaryReaderEx br, int count) {
                for (int i = 0; i < count; i++) {
                    this.Add(new Entry7(br));
                }
            }

            internal override void WriteEntries(BinaryWriterEx bw) {
                foreach (Entry7 entry in this) {
                    entry.Write(bw);
                }
            }
        }

        /// <summary>
        /// Unknown; believed to have something to do with connecting maps.
        /// </summary>
        public class Entry7 {
            /// <summary>
            /// Unknown.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int NameID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk18 { get; set; }

            /// <summary>
            /// Creates an Entry7 with default values.
            /// </summary>
            public Entry7() { }

            internal Entry7(BinaryReaderEx br) {
                this.Position = br.ReadVector3();
                _ = br.AssertSingle(1);
                this.NameID = br.ReadInt32();
                _ = br.AssertInt32(0);
                this.Unk18 = br.ReadInt32();
                _ = br.AssertInt32(0);
            }

            internal void Write(BinaryWriterEx bw) {
                bw.WriteVector3(this.Position);
                bw.WriteSingle(1);
                bw.WriteInt32(this.NameID);
                bw.WriteInt32(0);
                bw.WriteInt32(this.Unk18);
                bw.WriteInt32(0);
            }

            /// <summary>
            /// Returns a string representation of the entry.
            /// </summary>
            public override string ToString() => $"{this.Position} {this.NameID} {this.Unk18}";
        }

        /// <summary>
        /// Unknown. Version: 1 for BB and DS3, 2 for Sekiro.
        /// </summary>
        internal class MapNodeSection : Section<MapNode> {
            /// <summary>
            /// Creates an empty Section8 with the given version.
            /// </summary>
            public MapNodeSection(int version) : base(version) { }

            internal MapNodeSection(BinaryReaderEx br) : base(br, 8, 1, 2) { }

            internal override void ReadEntries(BinaryReaderEx br, int count) {
                for (int i = 0; i < count; i++) {
                    this.Add(new MapNode(br, this.Version));
                }
            }

            internal override void WriteEntries(BinaryWriterEx bw) {
                for (int i = 0; i < this.Count; i++) {
                    this[i].Write(bw, this.Version, i);
                }

                for (int i = 0; i < this.Count; i++) {
                    this[i].WriteSubIDs(bw, this.Version, i);
                }
            }
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        public class MapNode {
            /// <summary>
            /// Unknown.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Index to a navmesh.
            /// </summary>
            public short Section0Index { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public short MainID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<float> SiblingDistances { get; set; }

            /// <summary>
            /// Unknown; only present in Sekiro.
            /// </summary>
            public int Unk14 { get; set; }

            /// <summary>
            /// Creates an Entry8 with default values.
            /// </summary>
            public MapNode() => this.SiblingDistances = new List<float>();

            internal MapNode(BinaryReaderEx br, int version) {
                this.Position = br.ReadVector3();
                this.Section0Index = br.ReadInt16();
                this.MainID = br.ReadInt16();

                if (version < 2) {
                    this.SiblingDistances = new List<float>(
                        br.ReadUInt16s(16).Select(s => s == 0xFFFF ? -1 : s * 0.01f));
                } else {
                    int subIDCount = br.ReadInt32();
                    this.Unk14 = br.ReadInt32();
                    int subIDsOffset = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    this.SiblingDistances = new List<float>(
                        br.GetUInt16s(subIDsOffset, subIDCount).Select(s => s == 0xFFFF ? -1 : s * 0.01f));
                }
            }

            internal void Write(BinaryWriterEx bw, int version, int index) {
                bw.WriteVector3(this.Position);
                bw.WriteInt16(this.Section0Index);
                bw.WriteInt16(this.MainID);

                if (version < 2) {
                    if (this.SiblingDistances.Count > 16) {
                        throw new InvalidDataException("MapNode distance count must not exceed 16 in DS3/BB.");
                    }

                    foreach (float distance in this.SiblingDistances) {
                        bw.WriteUInt16((ushort)(distance == -1 ? 0xFFFF : Math.Round(distance * 100)));
                    }

                    for (int i = 0; i < 16 - this.SiblingDistances.Count; i++) {
                        bw.WriteUInt16(0xFFFF);
                    }
                } else {
                    bw.WriteInt32(this.SiblingDistances.Count);
                    bw.WriteInt32(this.Unk14);
                    bw.ReserveInt32($"SubIDsOffset{index}");
                    bw.WriteInt32(0);
                }
            }

            internal void WriteSubIDs(BinaryWriterEx bw, int version, int index) {
                if (version >= 2) {
                    bw.FillInt32($"SubIDsOffset{index}", (int)bw.Position);
                    foreach (float distance in this.SiblingDistances) {
                        bw.WriteUInt16((ushort)(distance == -1 ? 0xFFFF : Math.Round(distance * 100)));
                    }
                }
            }

            /// <summary>
            /// Returns a string representation of the entry.
            /// </summary>
            public override string ToString() => $"{this.Position} {this.Section0Index} {this.MainID} [{this.SiblingDistances.Count} SubIDs]";
        }
    }
}
