using System;
using System.Collections.Generic;
using System.IO;
using SoulsFormats.Formats.MSB;
using SoulsFormats.Util;

namespace SoulsFormats {
    /// <summary>
    /// A map layout file used in DS1. Extension: .msb
    /// </summary>
    public partial class MSB1 : SoulsFile<MSB1>, IMsb {
        /// <summary>
        /// True for PS3/X360, false for PC.
        /// </summary>
        public bool BigEndian { get; set; }

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
        /// Instances of actual things in the map.
        /// </summary>
        public PartsParam Parts { get; set; }
        IMsbParam<IMsbPart> IMsb.Parts => this.Parts;

        internal struct Entries {
            public List<Model> Models;
            public List<Event> Events;
            public List<Region> Regions;
            public List<Part> Parts;
        }

        /// <summary>
        /// Creates an empty MSB1.
        /// </summary>
        public MSB1() {
            this.Models = new ModelParam();
            this.Events = new EventParam();
            this.Regions = new PointParam();
            this.Parts = new PartsParam();
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            br.BigEndian = false;
            br.BigEndian = this.BigEndian = br.GetUInt32(4) > 0xFFFF;

            Entries entries;
            this.Models = new ModelParam();
            entries.Models = this.Models.Read(br);
            this.Events = new EventParam();
            entries.Events = this.Events.Read(br);
            this.Regions = new PointParam();
            entries.Regions = this.Regions.Read(br);
            this.Parts = new PartsParam();
            entries.Parts = this.Parts.Read(br);

            if (br.Position != 0) {
                throw new InvalidDataException("The next param offset of the final param should be 0, but it wasn't.");
            }

            MSB.DisambiguateNames(entries.Models);
            MSB.DisambiguateNames(entries.Regions);
            MSB.DisambiguateNames(entries.Parts);

            foreach (Event evt in entries.Events) {
                evt.GetNames(this, entries);
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
            entries.Parts = this.Parts.GetEntries();

            foreach (Model model in entries.Models) {
                model.CountInstances(entries.Parts);
            }

            foreach (Event evt in entries.Events) {
                evt.GetIndices(this, entries);
            }

            foreach (Part part in entries.Parts) {
                part.GetIndices(this, entries);
            }

            bw.BigEndian = this.BigEndian;

            this.Models.Write(bw, entries.Models);
            bw.FillInt32("NextParamOffset", (int)bw.Position);
            this.Events.Write(bw, entries.Events);
            bw.FillInt32("NextParamOffset", (int)bw.Position);
            this.Regions.Write(bw, entries.Regions);
            bw.FillInt32("NextParamOffset", (int)bw.Position);
            this.Parts.Write(bw, entries.Parts);
            bw.FillInt32("NextParamOffset", 0);
        }

        /// <summary>
        /// A generic group of entries in an MSB.
        /// </summary>
        public abstract class Param<T> where T : Entry {
            /// <summary>
            /// A string identifying the type of entries in the param.
            /// </summary>
            internal abstract string Name { get; }

            internal List<T> Read(BinaryReaderEx br) {
                _ = br.AssertInt32(0);
                int nameOffset = br.ReadInt32();
                int offsetCount = br.ReadInt32();
                int[] entryOffsets = br.ReadInt32s(offsetCount - 1);
                int nextParamOffset = br.ReadInt32();

                string name = br.GetASCII(nameOffset);
                if (name != this.Name) {
                    throw new InvalidDataException($"Expected param \"{this.Name}\", got param \"{name}\"");
                }

                var entries = new List<T>(offsetCount - 1);
                foreach (int offset in entryOffsets) {
                    br.Position = offset;
                    entries.Add(this.ReadEntry(br));
                }
                br.Position = nextParamOffset;
                return entries;
            }

            internal abstract T ReadEntry(BinaryReaderEx br);

            internal virtual void Write(BinaryWriterEx bw, List<T> entries) {
                bw.WriteInt32(0);
                bw.ReserveInt32("ParamNameOffset");
                bw.WriteInt32(entries.Count + 1);
                for (int i = 0; i < entries.Count; i++) {
                    bw.ReserveInt32($"EntryOffset{i}");
                }

                bw.ReserveInt32("NextParamOffset");

                bw.FillInt32("ParamNameOffset", (int)bw.Position);
                bw.WriteASCII(this.Name, true);
                bw.Pad(4);

                int id = 0;
                Type type = null;
                for (int i = 0; i < entries.Count; i++) {
                    if (type != entries[i].GetType()) {
                        type = entries[i].GetType();
                        id = 0;
                    }

                    bw.FillInt32($"EntryOffset{i}", (int)bw.Position);
                    entries[i].Write(bw, id);
                    id++;
                }
            }

            /// <summary>
            /// Returns all of the entries in this param, in the order they will be written to the file.
            /// </summary>
            public abstract List<T> GetEntries();

            /// <summary>
            /// Returns the name of the param as a string.
            /// </summary>
            public override string ToString() => $"{this.Name}";
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
    }
}
