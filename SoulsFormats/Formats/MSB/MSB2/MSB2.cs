﻿using System;
using System.Collections.Generic;
using System.IO;
using SoulsFormats.Formats.MSB;
using SoulsFormats.Util;

namespace SoulsFormats {
    /// <summary>
    /// A map layout file used in DS2, both original and SotFS. Extension: .msb
    /// </summary>
    public partial class MSB2 : SoulsFile<MSB2>, IMsb {
        /// <summary>
        /// The different formats of DS2 MSBs.
        /// </summary>
        public enum MSBFormat {
            /// <summary>
            /// 32-bit little-endian format for original DS2 on PC.
            /// </summary>
            DarkSouls2LE,

            /// <summary>
            /// 32-bit big-endian format for original DS2 on consoles.
            /// </summary>
            DarkSouls2BE,

            /// <summary>
            /// 64-bit format for SotFS on all platforms.
            /// </summary>
            DarkSouls2Scholar,
        }

        /// <summary>
        /// The format to use when writing.
        /// </summary>
        public MSBFormat Format { get; set; }

        /// <summary>
        /// Model files available for parts to use.
        /// </summary>
        public ModelParam Models { get; set; }
        IMsbParam<IMsbModel> IMsb.Models => this.Models;

        /// <summary>
        /// Abstract entities that set map properties or control behaviors.
        /// </summary>
        public EventParam Events { get; set; }
        IMsbParam<IMsbEvent> IMsb.Events => this.Events;

        /// <summary>
        /// Points or volumes that trigger certain behaviors.
        /// </summary>
        public PointParam Regions { get; set; }
        IMsbParam<IMsbRegion> IMsb.Regions => this.Regions;

        /// <summary>
        /// Concrete entities in the map.
        /// </summary>
        public PartsParam Parts { get; set; }
        IMsbParam<IMsbPart> IMsb.Parts => this.Parts;

        /// <summary>
        /// Predetermined poses applied to objects such as corpses.
        /// </summary>
        public List<PartPose> PartPoses { get; set; }

        /// <summary>
        /// Creates an empty MSB2 for SotFS.
        /// </summary>
        public MSB2() {
            this.Format = MSBFormat.DarkSouls2Scholar;
            this.Models = new ModelParam();
            this.Events = new EventParam();
            this.Regions = new PointParam();
            this.Parts = new PartsParam();
            this.PartPoses = new List<PartPose>();
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected internal override bool Is(BinaryReaderEx br) {
            if (br.Length < 0x14) {
                return false;
            }

            br.BigEndian = false;
            string magic = br.GetASCII(0, 4);
            int modelVersion = br.GetInt32(0x10);
            return magic == "MSB " && modelVersion == 5;
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            br.BigEndian = false;
            br.VarintLong = false;
            if (br.GetASCII(0, 4) == "MSB ") {
                this.Format = MSBFormat.DarkSouls2Scholar;
                br.VarintLong = true;
                MSB.AssertHeader(br);
            } else if (br.GetUInt32(0) == 5) {
                this.Format = MSBFormat.DarkSouls2LE;
            } else {
                this.Format = MSBFormat.DarkSouls2BE;
                br.BigEndian = true;
            }

            Entries entries;
            this.Models = new ModelParam();
            entries.Models = this.Models.Read(br);
            this.Events = new EventParam();
            entries.Events = this.Events.Read(br);
            this.Regions = new PointParam();
            entries.Regions = this.Regions.Read(br);
            _ = new RouteParam().Read(br);
            _ = new LayerParam().Read(br);
            this.Parts = new PartsParam();
            entries.Parts = this.Parts.Read(br);
            this.PartPoses = new MapstudioPartsPose().Read(br);
            entries.BoneNames = new MapstudioBoneName().Read(br);

            if (br.Position != 0) {
                throw new InvalidDataException($"The next param offset of the final param should be 0, but it was 0x{br.Position:X}.");
            }

            MSB.DisambiguateNames(entries.Models);
            MSB.DisambiguateNames(entries.Parts);
            MSB.DisambiguateNames(entries.BoneNames);

            foreach (Part part in entries.Parts) {
                part.GetNames(this, entries);
            }

            foreach (PartPose pose in this.PartPoses) {
                pose.GetNames(entries);
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
            entries.BoneNames = new List<BoneName>();

            Lookups lookups;
            lookups.Models = MakeNameLookup(entries.Models);
            lookups.Parts = MakeNameLookup(entries.Parts);
            lookups.Collisions = MakeNameLookup(this.Parts.Collisions);
            lookups.BoneNames = new Dictionary<string, int>();

            foreach (Part part in entries.Parts) {
                part.GetIndices(lookups);
            }

            foreach (PartPose pose in this.PartPoses) {
                pose.GetIndices(lookups, entries);
            }

            bw.BigEndian = this.Format == MSBFormat.DarkSouls2BE;
            bw.VarintLong = this.Format == MSBFormat.DarkSouls2Scholar;
            if (this.Format == MSBFormat.DarkSouls2Scholar) {
                MSB.WriteHeader(bw);
            }

            this.Models.Write(bw, entries.Models);
            bw.FillVarint("NextParamOffset", bw.Position);
            this.Events.Write(bw, entries.Events);
            bw.FillVarint("NextParamOffset", bw.Position);
            this.Regions.Write(bw, entries.Regions);
            bw.FillVarint("NextParamOffset", bw.Position);
            new RouteParam().Write(bw, new List<Entry>());
            bw.FillVarint("NextParamOffset", bw.Position);
            new LayerParam().Write(bw, new List<Entry>());
            bw.FillVarint("NextParamOffset", bw.Position);
            this.Parts.Write(bw, entries.Parts);
            bw.FillVarint("NextParamOffset", bw.Position);
            new MapstudioPartsPose().Write(bw, this.PartPoses);
            bw.FillVarint("NextParamOffset", bw.Position);
            new MapstudioBoneName().Write(bw, entries.BoneNames);
            bw.FillVarint("NextParamOffset", 0);
        }

        internal struct Entries {
            public List<Model> Models;
            public List<Event> Events;
            public List<Region> Regions;
            public List<Part> Parts;
            public List<BoneName> BoneNames;
        }

        internal struct Lookups {
            public Dictionary<string, int> Models;
            public Dictionary<string, int> Parts;
            public Dictionary<string, int> Collisions;
            public Dictionary<string, int> BoneNames;
        }

        /// <summary>
        /// A generic entry in an MSB param.
        /// </summary>
        public abstract class Entry {
            internal abstract void Write(BinaryWriterEx bw, int index);
        }

        /// <summary>
        /// A generic entry in an MSB param that has a name.
        /// </summary>
        public abstract class NamedEntry : Entry, IMsbEntry {
            /// <summary>
            /// The name of this entry; should generally be unique.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// A collection of entries in the MSB that share common properties.
        /// </summary>
        public abstract class Param<T> where T : Entry {
            internal abstract string Name { get; }
            internal abstract int Version { get; }

            internal List<T> Read(BinaryReaderEx br) {
                _ = br.AssertInt32(this.Version);

                int offsetCount;
                long nameOffset;
                if (br.VarintLong) {
                    offsetCount = br.ReadInt32();
                    nameOffset = br.ReadInt64();
                } else {
                    nameOffset = br.ReadInt32();
                    offsetCount = br.ReadInt32();
                }

                long[] entryOffsets = br.ReadVarints(offsetCount - 1);
                long nextParamOffset = br.ReadVarint();

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

                if (bw.VarintLong) {
                    bw.WriteInt32(entries.Count + 1);
                    bw.ReserveVarint("ParamNameOffset");
                } else {
                    bw.ReserveVarint("ParamNameOffset");
                    bw.WriteInt32(entries.Count + 1);
                }

                for (int i = 0; i < entries.Count; i++) {
                    bw.ReserveVarint($"EntryOffset{i}");
                }

                bw.ReserveVarint("NextParamOffset");

                bw.FillVarint("ParamNameOffset", bw.Position);
                bw.WriteUTF16(this.Name, true);
                bw.Pad(bw.VarintSize);

                int index = 0;
                Type type = null;
                for (int i = 0; i < entries.Count; i++) {
                    if (type != entries[i].GetType()) {
                        type = entries[i].GetType();
                        index = 0;
                    }

                    bw.FillVarint($"EntryOffset{i}", bw.Position);
                    entries[i].Write(bw, index);
                    bw.Pad(bw.VarintSize);
                    index++;
                }
            }

            /// <summary>
            /// Returns every entry in the order they'll be written.
            /// </summary>
            public abstract List<T> GetEntries();
        }

        private static int FindIndex(Dictionary<string, int> lookup, string name) => name == null ? -1 : !lookup.ContainsKey(name) ? throw new KeyNotFoundException($"Name not found: {name}") : lookup[name];

        private static Dictionary<string, int> MakeNameLookup<T>(List<T> list) where T : NamedEntry {
            var lookup = new Dictionary<string, int>();
            for (int i = 0; i < list.Count; i++) {
                lookup[list[i].Name] = i;
            }

            return lookup;
        }
    }
}
