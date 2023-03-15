using System;
using System.Collections.Generic;
using System.IO;
using SoulsFormats.Formats.MSB;
using SoulsFormats.Util;

namespace SoulsFormats {
    /// <summary>
    /// A map layout file used in Sekiro. Extension: .msb
    /// </summary>
    public partial class MSBS : SoulsFile<MSBS>, IMsb {
        /// <summary>
        /// Model files that are available for parts to use.
        /// </summary>
        public ModelParam Models { get; set; }
        IMsbParam<IMsbModel> IMsb.Models => this.Models;

        /// <summary>
        /// Dynamic or interactive systems such as item pickups, levers, enemy spawners, etc.
        /// </summary>
        public EventParam Events { get; set; }
        IMsbParam<IMsbEvent> IMsb.Events => this.Events;

        /// <summary>
        /// Points or areas of space that trigger some sort of behavior.
        /// </summary>
        public PointParam Regions { get; set; }
        IMsbParam<IMsbRegion> IMsb.Regions => this.Regions;

        /// <summary>
        /// Unknown, but related to muffling regions somehow.
        /// </summary>
        public RouteParam Routes { get; set; }

        /// <summary>
        /// Instances of actual things in the map.
        /// </summary>
        public PartsParam Parts { get; set; }
        IMsbParam<IMsbPart> IMsb.Parts => this.Parts;

        /// <summary>
        /// Unknown and unused.
        /// </summary>
        public EmptyParam Layers { get; set; }

        /// <summary>
        /// Sets bone positions for fixed objects; not used in Sekiro.
        /// </summary>
        public EmptyParam PartsPoses { get; set; }

        /// <summary>
        /// Bone names for the parts pose param; not used in Sekiro.
        /// </summary>
        public EmptyParam BoneNames { get; set; }

        /// <summary>
        /// Creates an MSBS with nothing in it.
        /// </summary>
        public MSBS() {
            this.Models = new ModelParam();
            this.Events = new EventParam();
            this.Regions = new PointParam();
            this.Routes = new RouteParam();
            this.Parts = new PartsParam();
            this.Layers = new EmptyParam(0x23, "LAYER_PARAM_ST");
            this.PartsPoses = new EmptyParam(0, "MAPSTUDIO_PARTS_POSE_ST");
            this.BoneNames = new EmptyParam(0, "MAPSTUDIO_BONE_NAME_STRING");
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected internal override bool Is(BinaryReaderEx br) {
            if (br.Length < 4) {
                return false;
            }

            string magic = br.GetASCII(0, 4);
            return magic == "MSB ";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            br.BigEndian = false;
            MSB.AssertHeader(br);

            Entries entries;
            this.Models = new ModelParam();
            entries.Models = this.Models.Read(br);
            this.Events = new EventParam();
            entries.Events = this.Events.Read(br);
            this.Regions = new PointParam();
            entries.Regions = this.Regions.Read(br);
            this.Routes = new RouteParam();
            entries.Routes = this.Routes.Read(br);
            this.Layers = new EmptyParam(0x23, "LAYER_PARAM_ST");
            _ = this.Layers.Read(br);
            this.Parts = new PartsParam();
            entries.Parts = this.Parts.Read(br);
            this.PartsPoses = new EmptyParam(0, "MAPSTUDIO_PARTS_POSE_ST");
            _ = this.PartsPoses.Read(br);
            this.BoneNames = new EmptyParam(0, "MAPSTUDIO_BONE_NAME_STRING");
            _ = this.BoneNames.Read(br);

            if (br.Position != 0) {
                throw new InvalidDataException("The next param offset of the final param should be 0, but it wasn't.");
            }

            MSB.DisambiguateNames(entries.Models);
            MSB.DisambiguateNames(entries.Regions);
            MSB.DisambiguateNames(entries.Parts);

            foreach (Event evt in entries.Events) {
                evt.GetNames(this, entries);
            }

            foreach (Region region in entries.Regions) {
                region.GetNames(entries);
            }

            foreach (Part part in entries.Parts) {
                part.GetNames(this, entries);
            }
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected internal override void Write(BinaryWriterEx bw) {
            Entries entries;
            entries.Models = this.Models.GetEntries();
            entries.Events = this.Events.GetEntries();
            entries.Regions = this.Regions.GetEntries();
            entries.Routes = this.Routes.GetEntries();
            entries.Parts = this.Parts.GetEntries();

            foreach (Model model in entries.Models) {
                model.CountInstances(entries.Parts);
            }

            foreach (Event evt in entries.Events) {
                evt.GetIndices(this, entries);
            }

            foreach (Region region in entries.Regions) {
                region.GetIndices(entries);
            }

            foreach (Part part in entries.Parts) {
                part.GetIndices(this, entries);
            }

            bw.BigEndian = false;
            MSB.WriteHeader(bw);

            this.Models.Write(bw, entries.Models);
            bw.FillInt64("NextParamOffset", bw.Position);
            this.Events.Write(bw, entries.Events);
            bw.FillInt64("NextParamOffset", bw.Position);
            this.Regions.Write(bw, entries.Regions);
            bw.FillInt64("NextParamOffset", bw.Position);
            this.Routes.Write(bw, entries.Routes);
            bw.FillInt64("NextParamOffset", bw.Position);
            this.Layers.Write(bw, this.Layers.GetEntries());
            bw.FillInt64("NextParamOffset", bw.Position);
            this.Parts.Write(bw, entries.Parts);
            bw.FillInt64("NextParamOffset", bw.Position);
            this.PartsPoses.Write(bw, this.Layers.GetEntries());
            bw.FillInt64("NextParamOffset", bw.Position);
            this.BoneNames.Write(bw, this.Layers.GetEntries());
            bw.FillInt64("NextParamOffset", 0);
        }

        internal struct Entries {
            public List<Model> Models;
            public List<Event> Events;
            public List<Region> Regions;
            public List<Route> Routes;
            public List<Part> Parts;
        }

        /// <summary>
        /// A generic group of entries in an MSB.
        /// </summary>
        public abstract class Param<T> where T : Entry {
            /// <summary>
            /// Unknown; probably some kind of version number.
            /// </summary>
            public int Version { get; set; }

            private protected string Name { get; }

            internal Param(int version, string name) {
                this.Version = version;
                this.Name = name;
            }

            internal List<T> Read(BinaryReaderEx br) {
                this.Version = br.ReadInt32();
                int offsetCount = br.ReadInt32();
                long nameOffset = br.ReadInt64();
                long[] entryOffsets = br.ReadInt64s(offsetCount - 1);
                long nextParamOffset = br.ReadInt64();

                string name = br.GetUTF16(nameOffset);
                if (name != this.Name) {
                    throw new InvalidDataException($"Expected param \"{this.Name}\", got param \"{name}\"");
                }

                var entries = new List<T>(offsetCount - 1);
                foreach (long offset in entryOffsets) {
                    br.Position = offset;
                    entries.Add(this.ReadEntry(br));
                }
                br.Position = nextParamOffset;
                return entries;
            }

            internal abstract T ReadEntry(BinaryReaderEx br);

            internal virtual void Write(BinaryWriterEx bw, List<T> entries) {
                bw.WriteInt32(this.Version);
                bw.WriteInt32(entries.Count + 1);
                bw.ReserveInt64("ParamNameOffset");
                for (int i = 0; i < entries.Count; i++) {
                    bw.ReserveInt64($"EntryOffset{i}");
                }

                bw.ReserveInt64("NextParamOffset");

                bw.FillInt64("ParamNameOffset", bw.Position);
                bw.WriteUTF16(this.Name, true);
                bw.Pad(8);

                int id = 0;
                Type type = null;
                for (int i = 0; i < entries.Count; i++) {
                    if (type != entries[i].GetType()) {
                        type = entries[i].GetType();
                        id = 0;
                    }

                    bw.FillInt64($"EntryOffset{i}", bw.Position);
                    entries[i].Write(bw, id);
                    id++;
                }
            }

            /// <summary>
            /// Returns all of the entries in this param, in the order they will be written to the file.
            /// </summary>
            public abstract List<T> GetEntries();

            /// <summary>
            /// Returns the version number and name of the param as a string.
            /// </summary>
            public override string ToString() => $"0x{this.Version:X2} {this.Name}";
        }

        /// <summary>
        /// A generic entry in an MSB param.
        /// </summary>
        public abstract class Entry : IMsbEntry {
            /// <summary>
            /// The name of this entry.
            /// </summary>
            public string Name { get; set; }

            internal abstract void Write(BinaryWriterEx bw, int id);
        }

        /// <summary>
        /// Used to represent unused params that should never have any entries in them.
        /// </summary>
        public class EmptyParam : Param<Entry> {
            /// <summary>
            /// Creates an EmptyParam with the given values.
            /// </summary>
            public EmptyParam(int version, string name) : base(version, name) { }

            internal override Entry ReadEntry(BinaryReaderEx br) => throw new InvalidDataException($"Expected param \"{this.Name}\" to be empty, but it wasn't.");

            /// <summary>
            /// Returns an empty list.
            /// </summary>
            public override List<Entry> GetEntries() => new();
        }
    }
}
