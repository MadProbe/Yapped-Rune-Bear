﻿using System;
using System.Collections.Generic;
using System.IO;
using SoulsFormats.Formats.MSB;
using SoulsFormats.Util;

namespace SoulsFormats {
    /// <summary>
    /// A map layout file used in BB. Extension: .msb
    /// </summary>
    public partial class MSBB : SoulsFile<MSBB>, IMsb {
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
        /// Creates an empty MSBB.
        /// </summary>
        public MSBB() {
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
            MSB.AssertHeader(br);

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
                throw new InvalidDataException($"The next param offset of the final param should be 0, but it was 0x{br.Position:X}.");
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

            bw.BigEndian = false;
            MSB.WriteHeader(bw);

            this.Models.Write(bw, entries.Models);
            bw.FillInt64("NextParamOffset", bw.Position);
            this.Events.Write(bw, entries.Events);
            bw.FillInt64("NextParamOffset", bw.Position);
            this.Regions.Write(bw, entries.Regions);
            bw.FillInt64("NextParamOffset", bw.Position);
            this.Parts.Write(bw, entries.Parts);
            bw.FillInt64("NextParamOffset", 0);
        }

        /// <summary>
        /// A generic MSB section containing a list of entries.
        /// </summary>
        public abstract class Param<T> where T : Entry {
            internal abstract int Version { get; }
            internal abstract string Name { get; }

            /// <summary>
            /// Returns every entry in this section in the order they will be written.
            /// </summary>
            public abstract List<T> GetEntries();

            internal List<T> Read(BinaryReaderEx br) {
                _ = br.AssertInt32(this.Version);
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

            internal void Write(BinaryWriterEx bw, List<T> entries) {
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
                Type currentType = null;
                for (int i = 0; i < entries.Count; i++) {
                    if (currentType != entries[i].GetType()) {
                        currentType = entries[i].GetType();
                        id = 0;
                    }

                    bw.FillInt64($"EntryOffset{i}", bw.Position);
                    entries[i].Write(bw, id);
                    id++;
                }
            }

            /// <summary>
            /// Returns a string representation of the param.
            /// </summary>
            public override string ToString() => $"{this.Name}:{this.Version}[{this.GetEntries().Count}]";
        }

        /// <summary>
        /// A generic entry in an MSB param.
        /// </summary>
        public abstract class Entry {
            internal abstract void Write(BinaryWriterEx bw, int id);
        }
    }
}
