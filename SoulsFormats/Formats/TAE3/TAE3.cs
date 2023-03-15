using System.Collections.Generic;
using System.Linq;
using SoulsFormats.Util;

namespace SoulsFormats {
    /// <summary>
    /// Controls when different events happen during animations; this specific version is used in DS3. Extension: .tae
    /// </summary>
    public partial class TAE3 : SoulsFile<TAE3> {
        /// <summary>
        /// ID number of this TAE.
        /// </summary>
        public int ID;

        /// <summary>
        /// Unknown flags.
        /// </summary>
        public byte[] Flags { get; private set; }

        /// <summary>
        /// Unknown .hkt file.
        /// </summary>
        public string SkeletonName;

        /// <summary>
        /// Unknown .sib file.
        /// </summary>
        public string SibName;

        /// <summary>
        /// Animations controlled by this TAE.
        /// </summary>
        public List<Animation> Animations;

        /// <summary>
        /// Unknown; chr tae: 0x15; obj tae: 0x4, 0x8, 0xE, 0xF, 0x10, 0x12, 0x13, 0x14, 0x15; mov tae: 0x4.
        /// </summary>
        public long Unk30;

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected internal override bool Is(BinaryReaderEx br) {
            if (br.Length < 4) {
                return false;
            }

            string magic = br.GetASCII(0, 4);
            return magic == "TAE ";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected internal override void Read(BinaryReaderEx br) {
            br.BigEndian = false;

            _ = br.AssertASCII("TAE ");
            _ = br.AssertByte(0);
            _ = br.AssertByte(0);
            _ = br.AssertByte(0);
            _ = br.AssertByte(0xFF);
            _ = br.AssertInt32(0x1000C);
            _ = br.ReadInt32(); // File size
            _ = br.AssertInt64(0x40);
            _ = br.AssertInt64(1);
            _ = br.AssertInt64(0x50);
            _ = br.AssertInt64(0x80);
            this.Unk30 = br.ReadInt64();
            _ = br.AssertInt64(0);
            this.Flags = br.ReadBytes(8);
            _ = br.AssertInt64(1);
            this.ID = br.ReadInt32();
            int animCount = br.ReadInt32();
            long animsOffset = br.ReadInt64();
            _ = br.ReadInt64(); // Anim groups offset
            _ = br.AssertInt64(0xA0);
            _ = br.AssertInt64(animCount);
            _ = br.ReadInt64(); // First anim offset
            _ = br.AssertInt64(1);
            _ = br.AssertInt64(0x90);
            _ = br.AssertInt32(this.ID);
            _ = br.AssertInt32(this.ID);
            _ = br.AssertInt64(0x50);
            _ = br.AssertInt64(0);
            _ = br.AssertInt64(0xB0);
            long skeletonNameOffset = br.ReadInt64();
            long sibNameOffset = br.ReadInt64();
            _ = br.AssertInt64(0);
            _ = br.AssertInt64(0);

            this.SkeletonName = br.GetUTF16(skeletonNameOffset);
            this.SibName = br.GetUTF16(sibNameOffset);

            br.StepIn(animsOffset);
            {
                this.Animations = new List<Animation>(animCount);
                for (int i = 0; i < animCount; i++) {
                    this.Animations.Add(new Animation(br));
                }
            }
            br.StepOut();

            // Don't bother reading anim groups.
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected internal override void Write(BinaryWriterEx bw) {
            bw.WriteASCII("TAE ");
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteByte(0xFF);
            bw.WriteInt32(0x1000C);
            bw.ReserveInt32("FileSize");
            bw.WriteInt64(0x40);
            bw.WriteInt64(1);
            bw.WriteInt64(0x50);
            bw.WriteInt64(0x80);
            bw.WriteInt64(this.Unk30);
            bw.WriteInt64(0);
            bw.WriteBytes(this.Flags);
            bw.WriteInt64(1);
            bw.WriteInt32(this.ID);
            bw.WriteInt32(this.Animations.Count);
            bw.ReserveInt64("AnimsOffset");
            bw.ReserveInt64("AnimGroupsOffset");
            bw.WriteInt64(0xA0);
            bw.WriteInt64(this.Animations.Count);
            bw.ReserveInt64("FirstAnimOffset");
            bw.WriteInt64(1);
            bw.WriteInt64(0x90);
            bw.WriteInt32(this.ID);
            bw.WriteInt32(this.ID);
            bw.WriteInt64(0x50);
            bw.WriteInt64(0);
            bw.WriteInt64(0xB0);
            bw.ReserveInt64("SkeletonName");
            bw.ReserveInt64("SibName");
            bw.WriteInt64(0);
            bw.WriteInt64(0);

            bw.FillInt64("SkeletonName", bw.Position);
            bw.WriteUTF16(this.SkeletonName, true);
            bw.Pad(0x10);

            bw.FillInt64("SibName", bw.Position);
            bw.WriteUTF16(this.SibName, true);
            bw.Pad(0x10);

            this.Animations.Sort((a1, a2) => a1.ID.CompareTo(a2.ID));

            var animOffsets = new List<long>(this.Animations.Count);
            if (this.Animations.Count == 0) {
                bw.FillInt64("AnimsOffset", 0);
            } else {
                bw.FillInt64("AnimsOffset", bw.Position);
                for (int i = 0; i < this.Animations.Count; i++) {
                    animOffsets.Add(bw.Position);
                    this.Animations[i].WriteHeader(bw, i);
                }
            }

            bw.FillInt64("AnimGroupsOffset", bw.Position);
            bw.ReserveInt64("AnimGroupsCount");
            bw.ReserveInt64("AnimGroupsOffset");
            int groupCount = 0;
            long groupStart = bw.Position;
            for (int i = 0; i < this.Animations.Count; i++) {
                int firstIndex = i;
                bw.WriteInt32((int)this.Animations[i].ID);
                while (i < this.Animations.Count - 1 && this.Animations[i + 1].ID == this.Animations[i].ID + 1) {
                    i++;
                }

                bw.WriteInt32((int)this.Animations[i].ID);
                bw.WriteInt64(animOffsets[firstIndex]);
                groupCount++;
            }
            bw.FillInt64("AnimGroupsCount", groupCount);
            if (groupCount == 0) {
                bw.FillInt64("AnimGroupsOffset", 0);
            } else {
                bw.FillInt64("AnimGroupsOffset", groupStart);
            }

            if (this.Animations.Count == 0) {
                bw.FillInt64("FirstAnimOffset", 0);
            } else {
                bw.FillInt64("FirstAnimOffset", bw.Position);
                for (int i = 0; i < this.Animations.Count; i++) {
                    this.Animations[i].WriteBody(bw, i);
                }
            }

            for (int i = 0; i < this.Animations.Count; i++) {
                Animation anim = this.Animations[i];
                anim.WriteAnimFile(bw, i);
                Dictionary<float, long> timeOffsets = anim.WriteTimes(bw, i);
                List<long> eventHeaderOffsets = anim.WriteEventHeaders(bw, i, timeOffsets);
                anim.WriteEventData(bw, i);
                anim.WriteEventGroupHeaders(bw, i);
                anim.WriteEventGroupData(bw, i, eventHeaderOffsets);
            }

            bw.FillInt32("FileSize", (int)bw.Position);
        }

        /// <summary>
        /// Controls an individual animation.
        /// </summary>
        public class Animation {
            /// <summary>
            /// ID number of this animation.
            /// </summary>
            public long ID;

            /// <summary>
            /// Timed events in this animation.
            /// </summary>
            public List<Event> Events;

            /// <summary>
            /// Unknown groups of events.
            /// </summary>
            public List<EventGroup> EventGroups;

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool AnimFileReference;

            /// <summary>
            /// Unknown.
            /// </summary>
            public int AnimFileUnk18;

            /// <summary>
            /// Unknown.
            /// </summary>
            public int AnimFileUnk1C;

            /// <summary>
            /// Unknown.
            /// </summary>
            public string AnimFileName;

            internal Animation(BinaryReaderEx br) {
                this.ID = br.ReadInt64();
                long offset = br.ReadInt64();
                br.StepIn(offset);
                {
                    long eventHeadersOffset = br.ReadInt64();
                    long eventGroupsOffset = br.ReadInt64();
                    _ = br.ReadInt64(); // Times offset
                    long animFileOffset = br.ReadInt64();
                    int eventCount = br.ReadInt32();
                    int eventGroupCount = br.ReadInt32();
                    _ = br.ReadInt32(); // Times count
                    _ = br.AssertInt32(0);

                    var eventHeaderOffsets = new List<long>(eventCount);
                    this.Events = new List<Event>(eventCount);
                    br.StepIn(eventHeadersOffset);
                    {
                        for (int i = 0; i < eventCount; i++) {
                            eventHeaderOffsets.Add(br.Position);
                            this.Events.Add(Event.Read(br));
                        }
                    }
                    br.StepOut();

                    this.EventGroups = new List<EventGroup>(eventGroupCount);
                    br.StepIn(eventGroupsOffset);
                    {
                        for (int i = 0; i < eventGroupCount; i++) {
                            this.EventGroups.Add(new EventGroup(br, eventHeaderOffsets));
                        }
                    }
                    br.StepOut();

                    br.StepIn(animFileOffset);
                    {
                        this.AnimFileReference = br.AssertInt64(0, 1) == 1;
                        _ = br.AssertInt64(br.Position + 8);
                        long animFileNameOffset = br.ReadInt64();
                        this.AnimFileUnk18 = br.ReadInt32();
                        this.AnimFileUnk1C = br.ReadInt32();
                        _ = br.AssertInt64(0);
                        _ = br.AssertInt64(0);

                        this.AnimFileName = animFileNameOffset < br.Length ? br.GetUTF16(animFileNameOffset) : "";
                        // When Reference is false, there's always a filename.
                        // When true, there's usually not, but sometimes there is, and I cannot figure out why.
                        // Thus, this stupid hack to achieve byte-perfection.
                        if (!(this.AnimFileName.EndsWith(".hkt") || this.AnimFileName.EndsWith(".hkx"))) {
                            this.AnimFileName = "";
                        }
                    }
                    br.StepOut();
                }
                br.StepOut();
            }

            internal void WriteHeader(BinaryWriterEx bw, int i) {
                bw.WriteInt64(this.ID);
                bw.ReserveInt64($"AnimationOffset{i}");
            }

            internal void WriteBody(BinaryWriterEx bw, int i) {
                bw.FillInt64($"AnimationOffset{i}", bw.Position);
                bw.ReserveInt64($"EventHeadersOffset{i}");
                bw.ReserveInt64($"EventGroupHeadersOffset{i}");
                bw.ReserveInt64($"TimesOffset{i}");
                bw.ReserveInt64($"AnimFileOffset{i}");
                bw.WriteInt32(this.Events.Count);
                bw.WriteInt32(this.EventGroups.Count);
                bw.ReserveInt32($"TimesCount{i}");
                bw.WriteInt32(0);
            }

            internal void WriteAnimFile(BinaryWriterEx bw, int i) {
                bw.FillInt64($"AnimFileOffset{i}", bw.Position);
                bw.WriteInt64(this.AnimFileReference ? 1 : 0);
                bw.WriteInt64(bw.Position + 8);
                bw.ReserveInt64("AnimFileNameOffset");
                bw.WriteInt32(this.AnimFileUnk18);
                bw.WriteInt32(this.AnimFileUnk1C);
                bw.WriteInt64(0);
                bw.WriteInt64(0);

                bw.FillInt64("AnimFileNameOffset", bw.Position);
                if (this.AnimFileName != "") {
                    bw.WriteUTF16(this.AnimFileName, true);
                    bw.Pad(0x10);
                }
            }

            internal Dictionary<float, long> WriteTimes(BinaryWriterEx bw, int animIndex) {
                var times = new SortedSet<float>();
                foreach (Event evt in this.Events) {
                    _ = times.Add(evt.StartTime);
                    _ = times.Add(evt.EndTime);
                }
                bw.FillInt32($"TimesCount{animIndex}", times.Count);

                if (times.Count == 0) {
                    bw.FillInt64($"TimesOffset{animIndex}", 0);
                } else {
                    bw.FillInt64($"TimesOffset{animIndex}", bw.Position);
                }

                var timeOffsets = new Dictionary<float, long>();
                foreach (float time in times) {
                    timeOffsets[time] = bw.Position;
                    bw.WriteSingle(time);
                }
                bw.Pad(0x10);

                return timeOffsets;
            }

            internal List<long> WriteEventHeaders(BinaryWriterEx bw, int animIndex, Dictionary<float, long> timeOffsets) {
                var eventHeaderOffsets = new List<long>(this.Events.Count);
                if (this.Events.Count > 0) {
                    bw.FillInt64($"EventHeadersOffset{animIndex}", bw.Position);
                    for (int i = 0; i < this.Events.Count; i++) {
                        eventHeaderOffsets.Add(bw.Position);
                        this.Events[i].WriteHeader(bw, animIndex, i, timeOffsets);
                    }
                } else {
                    bw.FillInt64($"EventHeadersOffset{animIndex}", 0);
                }
                return eventHeaderOffsets;
            }

            internal void WriteEventData(BinaryWriterEx bw, int i) {
                for (int j = 0; j < this.Events.Count; j++) {
                    this.Events[j].WriteData(bw, i, j);
                }
            }

            internal void WriteEventGroupHeaders(BinaryWriterEx bw, int i) {
                if (this.EventGroups.Count > 0) {
                    bw.FillInt64($"EventGroupHeadersOffset{i}", bw.Position);
                    for (int j = 0; j < this.EventGroups.Count; j++) {
                        this.EventGroups[j].WriteHeader(bw, i, j);
                    }
                } else {
                    bw.FillInt64($"EventGroupHeadersOffset{i}", 0);
                }
            }

            internal void WriteEventGroupData(BinaryWriterEx bw, int i, List<long> eventHeaderOffsets) {
                for (int j = 0; j < this.EventGroups.Count; j++) {
                    this.EventGroups[j].WriteData(bw, i, j, eventHeaderOffsets);
                }
            }
        }

        /// <summary>
        /// A group of events in an animation with an associated EventType that does not necessarily match theirs.
        /// </summary>
        public class EventGroup {
            /// <summary>
            /// Unknown.
            /// </summary>
            public EventType Type;

            /// <summary>
            /// Indices of events in this group in the parent animation's collection.
            /// </summary>
            public List<int> Indices;

            /// <summary>
            /// Creates a new empty EventGroup with the given type.
            /// </summary>
            public EventGroup(EventType type) {
                this.Type = type;
                this.Indices = new List<int>();
            }

            internal EventGroup(BinaryReaderEx br, List<long> eventHeaderOffsets) {
                long entryCount = br.ReadInt64();
                long valuesOffset = br.ReadInt64();
                long typeOffset = br.ReadInt64();
                _ = br.AssertInt64(0);

                br.StepIn(typeOffset);
                {
                    this.Type = br.ReadEnum64<EventType>();
                    _ = br.AssertInt64(0);
                }
                br.StepOut();

                br.StepIn(valuesOffset);
                {
                    this.Indices = br.ReadInt32s((int)entryCount).Select(offset => eventHeaderOffsets.FindIndex(headerOffset => headerOffset == offset)).ToList();
                }
                br.StepOut();
            }

            internal void WriteHeader(BinaryWriterEx bw, int i, int j) {
                bw.WriteInt64(this.Indices.Count);
                bw.ReserveInt64($"EventGroupValuesOffset{i}:{j}");
                bw.ReserveInt64($"EventGroupTypeOffset{i}:{j}");
                bw.WriteInt64(0);
            }

            internal void WriteData(BinaryWriterEx bw, int i, int j, List<long> eventHeaderOffsets) {
                bw.FillInt64($"EventGroupTypeOffset{i}:{j}", bw.Position);
                bw.WriteUInt64((ulong)this.Type);
                bw.WriteInt64(0);

                bw.FillInt64($"EventGroupValuesOffset{i}:{j}", bw.Position);
                for (int k = 0; k < this.Indices.Count; k++) {
                    bw.WriteInt32((int)eventHeaderOffsets[this.Indices[k]]);
                }

                bw.Pad(0x10);
            }
        }
    }
}
