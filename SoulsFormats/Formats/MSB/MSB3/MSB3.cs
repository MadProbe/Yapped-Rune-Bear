using System;
using System.Collections.Generic;
using System.IO;
using SoulsFormats.Formats.MSB;
using SoulsFormats.Util;

namespace SoulsFormats {
    /// <summary>
    /// A map layout format used in DS3.
    /// </summary>
    public partial class MSB3 : SoulsFile<MSB3>, IMsb {
        /// <summary>
        /// Models in this MSB.
        /// </summary>
        public ModelParam Models { get; set; }
        IMsbParam<IMsbModel> IMsb.Models => this.Models;

        /// <summary>
        /// Events in this MSB.
        /// </summary>
        public EventParam Events { get; set; }
        IMsbParam<IMsbEvent> IMsb.Events => this.Events;

        /// <summary>
        /// Regions in this MSB.
        /// </summary>
        public PointParam Regions { get; set; }
        IMsbParam<IMsbRegion> IMsb.Regions => this.Regions;

        /// <summary>
        /// Routes in this MSB.
        /// </summary>
        public List<Route> Routes { get; set; }

        /// <summary>
        /// Layers in this MSB.
        /// </summary>
        public List<Layer> Layers { get; set; }

        /// <summary>
        /// Parts in this MSB.
        /// </summary>
        public PartsParam Parts { get; set; }
        IMsbParam<IMsbPart> IMsb.Parts => this.Parts;

        /// <summary>
        /// PartsPose data in this MSB.
        /// </summary>
        public List<PartsPose> PartsPoses { get; set; }

        /// <summary>
        /// Creates a new MSB3 with all sections empty.
        /// </summary>
        public MSB3() {
            this.Models = new ModelParam();
            this.Events = new EventParam();
            this.Regions = new PointParam();
            this.Routes = new List<Route>();
            this.Layers = new List<Layer>();
            this.Parts = new PartsParam();
            this.PartsPoses = new List<PartsPose>();
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

        internal struct Entries {
            public List<Model> Models;
            public List<Event> Events;
            public List<Region> Regions;
            public List<Part> Parts;
            public List<BoneName> BoneNames;
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            br.BigEndian = false;
            MSB.AssertHeader(br);

            Entries entries = default;
            this.Models = new ModelParam();
            entries.Models = this.Models.Read(br);
            this.Events = new EventParam();
            entries.Events = this.Events.Read(br);
            this.Regions = new PointParam();
            entries.Regions = this.Regions.Read(br);
            this.Routes = new RouteParam().Read(br);
            this.Layers = new LayerParam().Read(br);
            this.Parts = new PartsParam();
            entries.Parts = this.Parts.Read(br);
            this.PartsPoses = new MapstudioPartsPose().Read(br);
            entries.BoneNames = new MapstudioBoneName().Read(br);

            if (br.Position != 0) {
                throw new InvalidDataException($"The next param offset of the final param should be 0, but it was 0x{br.Position:X}.");
            }

            MSB.DisambiguateNames(entries.Models);
            MSB.DisambiguateNames(entries.Parts);
            MSB.DisambiguateNames(entries.Regions);
            MSB.DisambiguateNames(entries.BoneNames);

            foreach (Event evt in entries.Events) {
                evt.GetNames(this, entries);
            }

            foreach (Region region in entries.Regions) {
                region.GetNames(this, entries);
            }

            foreach (Part part in entries.Parts) {
                part.GetNames(this, entries);
            }

            foreach (PartsPose pose in this.PartsPoses) {
                pose.GetNames(this, entries);
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

            foreach (Model model in entries.Models) {
                model.CountInstances(entries.Parts);
            }

            foreach (Event evt in entries.Events) {
                evt.GetIndices(this, entries);
            }

            foreach (Region region in entries.Regions) {
                region.GetIndices(this, entries);
            }

            foreach (Part part in entries.Parts) {
                part.GetIndices(this, entries);
            }

            foreach (PartsPose pose in this.PartsPoses) {
                pose.GetIndices(this, entries);
            }

            bw.BigEndian = false;
            MSB.WriteHeader(bw);

            this.Models.Write(bw, entries.Models);
            bw.FillInt64("NextParamOffset", bw.Position);
            this.Events.Write(bw, entries.Events);
            bw.FillInt64("NextParamOffset", bw.Position);
            this.Regions.Write(bw, entries.Regions);
            bw.FillInt64("NextParamOffset", bw.Position);
            new RouteParam().Write(bw, this.Routes);
            bw.FillInt64("NextParamOffset", bw.Position);
            new LayerParam().Write(bw, this.Layers);
            bw.FillInt64("NextParamOffset", bw.Position);
            this.Parts.Write(bw, entries.Parts);
            bw.FillInt64("NextParamOffset", bw.Position);
            new MapstudioPartsPose().Write(bw, this.PartsPoses);
            bw.FillInt64("NextParamOffset", bw.Position);
            new MapstudioBoneName().Write(bw, entries.BoneNames);
            bw.FillInt64("NextParamOffset", 0);
        }

        /// <summary>
        /// A generic MSB section containing a list of entries.
        /// </summary>
        public abstract class Param<T> where T : Entry {
            internal abstract int Version { get; }
            internal abstract string Type { get; }

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

                string type = br.GetUTF16(nameOffset);
                if (type != this.Type) {
                    throw new InvalidDataException($"Expected param \"{this.Type}\", got param \"{type}\"");
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
                bw.WriteUTF16(this.Type, true);
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
            /// Returns the type string, unknown value and number of entries in this section.
            /// </summary>
            public override string ToString() => $"{this.Type}:{this.Version}[{this.GetEntries().Count}]";
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
            public abstract string Name { get; set; }
        }
    }
}
