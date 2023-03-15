using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using SoulsFormats.Formats.MSB;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MSBD {
        internal enum EventType : uint {
            Light = 0,
            Sound = 1,
            SFX = 2,
            Wind = 3,
            Treasure = 4,
            Generator = 5,
            Message = 6,
        }

        /// <summary>
        /// Contains abstract entities that control various dynamic elements in the map.
        /// </summary>
        public class EventParam : Param<Event>, IMsbParam<IMsbEvent> {
            internal override string Name => "EVENT_PARAM_ST";

            /// <summary>
            /// Fixed point light sources.
            /// </summary>
            public List<Event.Light> Lights { get; set; }

            /// <summary>
            /// Background music and area-based sounds.
            /// </summary>
            public List<Event.Sound> Sounds { get; set; }

            /// <summary>
            /// Particle effects.
            /// </summary>
            public List<Event.SFX> SFX { get; set; }

            /// <summary>
            /// Wind that affects SFX in the map; should only be one per map, if any.
            /// </summary>
            public List<Event.Wind> Wind { get; set; }

            /// <summary>
            /// Item pickups in the open or in chests.
            /// </summary>
            public List<Event.Treasure> Treasures { get; set; }

            /// <summary>
            /// Repeated enemy spawners.
            /// </summary>
            public List<Event.Generator> Generators { get; set; }

            /// <summary>
            /// Static soapstone messages.
            /// </summary>
            public List<Event.Message> Messages { get; set; }

            /// <summary>
            /// Creates an empty EventParam.
            /// </summary>
            public EventParam() : base() {
                this.Lights = new List<Event.Light>();
                this.Sounds = new List<Event.Sound>();
                this.SFX = new List<Event.SFX>();
                this.Wind = new List<Event.Wind>();
                this.Treasures = new List<Event.Treasure>();
                this.Generators = new List<Event.Generator>();
                this.Messages = new List<Event.Message>();
            }

            /// <summary>
            /// Adds an event to the appropriate list for its type; returns the event.
            /// </summary>
            public Event Add(Event evnt) {
                switch (evnt) {
                    case Event.Light e: this.Lights.Add(e); break;
                    case Event.Sound e: this.Sounds.Add(e); break;
                    case Event.SFX e: this.SFX.Add(e); break;
                    case Event.Wind e: this.Wind.Add(e); break;
                    case Event.Treasure e: this.Treasures.Add(e); break;
                    case Event.Generator e: this.Generators.Add(e); break;
                    case Event.Message e: this.Messages.Add(e); break;

                    default:
                        throw new ArgumentException($"Unrecognized type {evnt.GetType()}.", nameof(evnt));
                }
                return evnt;
            }
            IMsbEvent IMsbParam<IMsbEvent>.Add(IMsbEvent item) => this.Add((Event)item);

            /// <summary>
            /// Returns a list of every event in the order they'll be written.
            /// </summary>
            public override List<Event> GetEntries() => SFUtil.ConcatAll<Event>(
                    this.Lights, this.Sounds, this.SFX, this.Wind, this.Treasures,
                    this.Generators, this.Messages);
            IReadOnlyList<IMsbEvent> IMsbParam<IMsbEvent>.GetEntries() => this.GetEntries();

            internal override Event ReadEntry(BinaryReaderEx br) {
                EventType type = br.GetEnum32<EventType>(br.Position + 8);
                return type switch {
                    EventType.Light => this.Lights.EchoAdd(new Event.Light(br)),
                    EventType.Sound => this.Sounds.EchoAdd(new Event.Sound(br)),
                    EventType.SFX => this.SFX.EchoAdd(new Event.SFX(br)),
                    EventType.Wind => this.Wind.EchoAdd(new Event.Wind(br)),
                    EventType.Treasure => this.Treasures.EchoAdd(new Event.Treasure(br)),
                    EventType.Generator => this.Generators.EchoAdd(new Event.Generator(br)),
                    EventType.Message => this.Messages.EchoAdd(new Event.Message(br)),
                    _ => throw new NotImplementedException($"Unsupported event type: {type}"),
                };
            }
        }

        /// <summary>
        /// Common data for all dynamic events.
        /// </summary>
        public abstract class Event : Entry, IMsbEvent {
            private protected abstract EventType Type { get; }

            /// <summary>
            /// The name of the event.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Unknown, should be unique.
            /// </summary>
            public int EventID { get; set; }

            /// <summary>
            /// Part referenced by the event.
            /// </summary>
            public string PartName { get; set; }
            private int PartIndex;

            /// <summary>
            /// Region referenced by the event.
            /// </summary>
            public string RegionName { get; set; }
            private int RegionIndex;

            /// <summary>
            /// Identifies the event in external files.
            /// </summary>
            public int EntityID { get; set; }

            private protected Event(string name) {
                this.Name = name;
                this.EventID = -1;
                this.EntityID = -1;
            }

            /// <summary>
            /// Creates a deep copy of the event.
            /// </summary>
            public Event DeepCopy() {
                var evnt = (Event)this.MemberwiseClone();
                this.DeepCopyTo(evnt);
                return evnt;
            }
            IMsbEvent IMsbEvent.DeepCopy() => this.DeepCopy();

            private protected virtual void DeepCopyTo(Event evnt) { }

            private protected Event(BinaryReaderEx br) {
                long start = br.Position;
                int nameOffset = br.ReadInt32();
                this.EventID = br.ReadInt32();
                _ = br.AssertUInt32((uint)this.Type);
                _ = br.ReadInt32(); // ID
                int baseDataOffset = br.ReadInt32();
                int typeDataOffset = br.ReadInt32();
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);

                if (nameOffset == 0) {
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {this.GetType()}.");
                }

                if (baseDataOffset == 0) {
                    throw new InvalidDataException($"{nameof(baseDataOffset)} must not be 0 in type {this.GetType()}.");
                }

                if (typeDataOffset == 0) {
                    throw new InvalidDataException($"{nameof(typeDataOffset)} must not be 0 in type {this.GetType()}.");
                }

                br.Position = start + nameOffset;
                this.Name = br.ReadShiftJIS();

                br.Position = start + baseDataOffset;
                this.PartIndex = br.ReadInt32();
                this.RegionIndex = br.ReadInt32();
                this.EntityID = br.ReadInt32();
                _ = br.AssertInt32(0);

                br.Position = start + typeDataOffset;
                this.ReadTypeData(br);
            }

            private protected abstract void ReadTypeData(BinaryReaderEx br);

            internal override void Write(BinaryWriterEx bw, int id) {
                long start = bw.Position;
                bw.ReserveInt32("NameOffset");
                bw.WriteInt32(this.EventID);
                bw.WriteUInt32((uint)this.Type);
                bw.WriteInt32(id);
                bw.ReserveInt32("BaseDataOffset");
                bw.ReserveInt32("TypeDataOffset");
                bw.WriteInt32(0);
                bw.WriteInt32(0);

                bw.FillInt32("NameOffset", (int)(bw.Position - start));
                bw.WriteShiftJIS(this.Name, true);
                bw.Pad(4);

                bw.FillInt32("BaseDataOffset", (int)(bw.Position - start));
                bw.WriteInt32(this.PartIndex);
                bw.WriteInt32(this.RegionIndex);
                bw.WriteInt32(this.EntityID);
                bw.WriteInt32(0);

                bw.FillInt32("TypeDataOffset", (int)(bw.Position - start));
                this.WriteTypeData(bw);
            }

            private protected abstract void WriteTypeData(BinaryWriterEx bw);

            internal virtual void GetNames(MSBD msb, Entries entries) {
                this.PartName = MSB.FindName(entries.Parts, this.PartIndex);
                this.RegionName = MSB.FindName(entries.Regions, this.RegionIndex);
            }

            internal virtual void GetIndices(MSBD msb, Entries entries) {
                this.PartIndex = MSB.FindIndex(entries.Parts, this.PartName);
                this.RegionIndex = MSB.FindIndex(entries.Regions, this.RegionName);
            }

            /// <summary>
            /// Returns a string representation of the event.
            /// </summary>
            public override string ToString() => $"{this.Type} {this.Name}";

            /// <summary>
            /// A fixed point light.
            /// </summary>
            public class Light : Event {
                private protected override EventType Type => EventType.Light;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int PointLightID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// Creates a Light with default values.
                /// </summary>
                public Light() : base($"{nameof(Event)}: {nameof(Light)}") { }

                internal Light(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.PointLightID = br.ReadInt32();
                    this.UnkT04 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.PointLightID);
                    bw.WriteInt32(this.UnkT04);
                }
            }

            /// <summary>
            /// An area-based music or sound effect.
            /// </summary>
            public class Sound : Event {
                private protected override EventType Type => EventType.Sound;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Category of sound.
                /// </summary>
                public int SoundType { get; set; }

                /// <summary>
                /// ID of the sound file in the FSBs.
                /// </summary>
                public int SoundID { get; set; }

                /// <summary>
                /// Creates a Sound with default values.
                /// </summary>
                public Sound() : base($"{nameof(Event)}: {nameof(Sound)}") { }

                internal Sound(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadInt32();
                    this.SoundType = br.ReadInt32();
                    this.SoundID = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.UnkT00);
                    bw.WriteInt32(this.SoundType);
                    bw.WriteInt32(this.SoundID);
                }
            }

            /// <summary>
            /// A fixed particle effect.
            /// </summary>
            public class SFX : Event {
                private protected override EventType Type => EventType.SFX;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// ID of the effect in the ffxbnds.
                /// </summary>
                public int EffectID { get; set; }

                /// <summary>
                /// Creates an SFX with default values.
                /// </summary>
                public SFX() : base($"{nameof(Event)}: {nameof(SFX)}") { }

                internal SFX(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadInt32();
                    this.EffectID = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.UnkT00);
                    bw.WriteInt32(this.EffectID);
                }
            }

            /// <summary>
            /// Wind that affects particle effects.
            /// </summary>
            public class Wind : Event {
                private protected override EventType Type => EventType.Wind;

                /// <summary>
                /// Unknown.
                /// </summary>
                public Vector3 WindVecMin { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public Vector3 WindVecMax { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT1C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float WindSwingCycle0 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float WindSwingCycle1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float WindSwingCycle2 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float WindSwingCycle3 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float WindSwingPow0 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float WindSwingPow1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float WindSwingPow2 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float WindSwingPow3 { get; set; }

                /// <summary>
                /// Creates a Wind with default values.
                /// </summary>
                public Wind() : base($"{nameof(Event)}: {nameof(Wind)}") { }

                internal Wind(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.WindVecMin = br.ReadVector3();
                    this.UnkT0C = br.ReadSingle();
                    this.WindVecMax = br.ReadVector3();
                    this.UnkT1C = br.ReadSingle();
                    this.WindSwingCycle0 = br.ReadSingle();
                    this.WindSwingCycle1 = br.ReadSingle();
                    this.WindSwingCycle2 = br.ReadSingle();
                    this.WindSwingCycle3 = br.ReadSingle();
                    this.WindSwingPow0 = br.ReadSingle();
                    this.WindSwingPow1 = br.ReadSingle();
                    this.WindSwingPow2 = br.ReadSingle();
                    this.WindSwingPow3 = br.ReadSingle();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteVector3(this.WindVecMin);
                    bw.WriteSingle(this.UnkT0C);
                    bw.WriteVector3(this.WindVecMax);
                    bw.WriteSingle(this.UnkT1C);
                    bw.WriteSingle(this.WindSwingCycle0);
                    bw.WriteSingle(this.WindSwingCycle1);
                    bw.WriteSingle(this.WindSwingCycle2);
                    bw.WriteSingle(this.WindSwingCycle3);
                    bw.WriteSingle(this.WindSwingPow0);
                    bw.WriteSingle(this.WindSwingPow1);
                    bw.WriteSingle(this.WindSwingPow2);
                    bw.WriteSingle(this.WindSwingPow3);
                }
            }

            /// <summary>
            /// A pick-uppable item.
            /// </summary>
            public class Treasure : Event {
                private protected override EventType Type => EventType.Treasure;

                /// <summary>
                /// The part that the treasure is attached to, such as an item corpse.
                /// </summary>
                public string TreasurePartName { get; set; }
                private int TreasurePartIndex;

                /// <summary>
                /// Five ItemLotParam IDs.
                /// </summary>
                public int[] ItemLots { get; private set; }

                /// <summary>
                /// Creates a Treasure with default values.
                /// </summary>
                public Treasure() : base($"{nameof(Event)}: {nameof(Treasure)}") => this.ItemLots = new int[5] { -1, -1, -1, -1, -1 };

                private protected override void DeepCopyTo(Event evnt) {
                    var treasure = (Treasure)evnt;
                    treasure.ItemLots = (int[])this.ItemLots.Clone();
                }

                internal Treasure(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    _ = br.AssertInt32(0);
                    this.TreasurePartIndex = br.ReadInt32();
                    this.ItemLots = new int[5];
                    for (int i = 0; i < 5; i++) {
                        this.ItemLots[i] = br.ReadInt32();
                        _ = br.AssertInt32(-1);
                    }
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(this.TreasurePartIndex);
                    for (int i = 0; i < 5; i++) {
                        bw.WriteInt32(this.ItemLots[i]);
                        bw.WriteInt32(-1);
                    }
                }

                internal override void GetNames(MSBD msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.TreasurePartName = MSB.FindName(entries.Parts, this.TreasurePartIndex);
                }

                internal override void GetIndices(MSBD msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.TreasurePartIndex = MSB.FindIndex(entries.Parts, this.TreasurePartName);
                }
            }

            /// <summary>
            /// A repeating enemy spawner.
            /// </summary>
            public class Generator : Event {
                private protected override EventType Type => EventType.Generator;

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte MaxNum { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte GenType { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short LimitNum { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short MinGenNum { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short MaxGenNum { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float MinInterval { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float MaxInterval { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte InitialSpawnCount { get; set; }

                /// <summary>
                /// Points that enemies may be spawned at.
                /// </summary>
                public string[] SpawnPointNames { get; private set; }
                private int[] SpawnPointIndices;

                /// <summary>
                /// Enemies to be respawned.
                /// </summary>
                public string[] SpawnPartNames { get; private set; }
                private int[] SpawnPartIndices;

                /// <summary>
                /// Creates a Generator with default values.
                /// </summary>
                public Generator() : base($"{nameof(Event)}: {nameof(Generator)}") {
                    this.SpawnPointNames = new string[4];
                    this.SpawnPartNames = new string[32];
                }

                private protected override void DeepCopyTo(Event evnt) {
                    var generator = (Generator)evnt;
                    generator.SpawnPointNames = (string[])this.SpawnPointNames.Clone();
                    generator.SpawnPartNames = (string[])this.SpawnPartNames.Clone();
                }

                internal Generator(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.MaxNum = br.ReadByte();
                    this.GenType = br.ReadSByte();
                    this.LimitNum = br.ReadInt16();
                    this.MinGenNum = br.ReadInt16();
                    this.MaxGenNum = br.ReadInt16();
                    this.MinInterval = br.ReadSingle();
                    this.MaxInterval = br.ReadSingle();
                    this.InitialSpawnCount = br.ReadByte();
                    br.AssertPattern(0x1F, 0x00);
                    this.SpawnPointIndices = br.ReadInt32s(4);
                    this.SpawnPartIndices = br.ReadInt32s(32);
                    br.AssertPattern(0x40, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteByte(this.MaxNum);
                    bw.WriteSByte(this.GenType);
                    bw.WriteInt16(this.LimitNum);
                    bw.WriteInt16(this.MinGenNum);
                    bw.WriteInt16(this.MaxGenNum);
                    bw.WriteSingle(this.MinInterval);
                    bw.WriteSingle(this.MaxInterval);
                    bw.WriteByte(this.InitialSpawnCount);
                    bw.WritePattern(0x1F, 0x00);
                    bw.WriteInt32s(this.SpawnPointIndices);
                    bw.WriteInt32s(this.SpawnPartIndices);
                    bw.WritePattern(0x40, 0x00);
                }

                internal override void GetNames(MSBD msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.SpawnPointNames = MSB.FindNames(entries.Regions, this.SpawnPointIndices);
                    this.SpawnPartNames = MSB.FindNames(entries.Parts, this.SpawnPartIndices);
                }

                internal override void GetIndices(MSBD msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.SpawnPointIndices = MSB.FindIndices(entries.Regions, this.SpawnPointNames);
                    this.SpawnPartIndices = MSB.FindIndices(entries.Parts, this.SpawnPartNames);
                }
            }

            /// <summary>
            /// A fixed orange soapstone message.
            /// </summary>
            public class Message : Event {
                private protected override EventType Type => EventType.Message;

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT00 { get; set; }

                /// <summary>
                /// FMG text ID to display.
                /// </summary>
                public short MessageID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int MessageParam { get; set; }

                /// <summary>
                /// Creates a Message with default values.
                /// </summary>
                public Message() : base($"{nameof(Event)}: {nameof(Message)}") { }

                internal Message(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadInt16();
                    this.MessageID = br.ReadInt16();
                    this.MessageParam = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt16(this.UnkT00);
                    bw.WriteInt16(this.MessageID);
                    bw.WriteInt32(this.MessageParam);
                }
            }
        }
    }
}
