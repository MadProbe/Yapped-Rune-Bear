using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using SoulsFormats.Formats.MSB;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MSBB {
        internal enum EventType : uint {
            //Light = 0,
            Sound = 1,
            SFX = 2,
            //Wind = 3,
            Treasure = 4,
            Generator = 5,
            Message = 6,
            ObjAct = 7,
            SpawnPoint = 8,
            MapOffset = 9,
            Navmesh = 10,
            Environment = 11,
            //PseudoMultiplayer = 12,
            WindSFX = 13,
            PatrolInfo = 14,
            DarkLock = 15,
            PlatoonInfo = 16,
            MultiSummon = 17,
            Other = 0xFFFFFFFF,
        }

        /// <summary>
        /// Contains abstract entities that control various dynamic elements in the map.
        /// </summary>
        public class EventParam : Param<Event>, IMsbParam<IMsbEvent> {
            internal override int Version => 3;
            internal override string Name => "EVENT_PARAM_ST";

            /// <summary>
            /// Background music and area-based sounds.
            /// </summary>
            public List<Event.Sound> Sounds { get; set; }

            /// <summary>
            /// Particle effects.
            /// </summary>
            public List<Event.SFX> SFX { get; set; }

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
            /// Controllers for object interactions.
            /// </summary>
            public List<Event.ObjAct> ObjActs { get; set; }

            /// <summary>
            /// Unknown exactly what this is for.
            /// </summary>
            public List<Event.SpawnPoint> SpawnPoints { get; set; }

            /// <summary>
            /// Represents the origin of the map; already accounted for in MSB positions.
            /// </summary>
            public List<Event.MapOffset> MapOffsets { get; set; }

            /// <summary>
            /// Unknown, interacts with navmeshes somehow.
            /// </summary>
            public List<Event.Navmesh> Navmeshes { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.Environment> Environments { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.WindSFX> WindSFX { get; set; }

            /// <summary>
            /// Patrol info in the MSB.
            /// </summary>
            public List<Event.PatrolInfo> PatrolInfo { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.DarkLock> DarkLocks { get; set; }

            /// <summary>
            /// Platoon info in the MSB.
            /// </summary>
            public List<Event.PlatoonInfo> PlatoonInfo { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.MultiSummon> MultiSummons { get; set; }

            /// <summary>
            /// Other events in the MSB.
            /// </summary>
            public List<Event.Other> Others { get; set; }

            /// <summary>
            /// Creates an empty EventParam.
            /// </summary>
            public EventParam() : base() {
                this.Sounds = new List<Event.Sound>();
                this.SFX = new List<Event.SFX>();
                this.Treasures = new List<Event.Treasure>();
                this.Generators = new List<Event.Generator>();
                this.Messages = new List<Event.Message>();
                this.ObjActs = new List<Event.ObjAct>();
                this.SpawnPoints = new List<Event.SpawnPoint>();
                this.MapOffsets = new List<Event.MapOffset>();
                this.Navmeshes = new List<Event.Navmesh>();
                this.Environments = new List<Event.Environment>();
                this.WindSFX = new List<Event.WindSFX>();
                this.PatrolInfo = new List<Event.PatrolInfo>();
                this.DarkLocks = new List<Event.DarkLock>();
                this.PlatoonInfo = new List<Event.PlatoonInfo>();
                this.MultiSummons = new List<Event.MultiSummon>();
                this.Others = new List<Event.Other>();
            }

            /// <summary>
            /// Adds an event to the appropriate list for its type; returns the event.
            /// </summary>
            public Event Add(Event evnt) {
                switch (evnt) {
                    case Event.Sound e: this.Sounds.Add(e); break;
                    case Event.SFX e: this.SFX.Add(e); break;
                    case Event.Treasure e: this.Treasures.Add(e); break;
                    case Event.Generator e: this.Generators.Add(e); break;
                    case Event.Message e: this.Messages.Add(e); break;
                    case Event.ObjAct e: this.ObjActs.Add(e); break;
                    case Event.SpawnPoint e: this.SpawnPoints.Add(e); break;
                    case Event.MapOffset e: this.MapOffsets.Add(e); break;
                    case Event.Navmesh e: this.Navmeshes.Add(e); break;
                    case Event.Environment e: this.Environments.Add(e); break;
                    case Event.WindSFX e: this.WindSFX.Add(e); break;
                    case Event.PatrolInfo e: this.PatrolInfo.Add(e); break;
                    case Event.DarkLock e: this.DarkLocks.Add(e); break;
                    case Event.PlatoonInfo e: this.PlatoonInfo.Add(e); break;
                    case Event.MultiSummon e: this.MultiSummons.Add(e); break;
                    case Event.Other e: this.Others.Add(e); break;

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
                    this.Sounds, this.SFX, this.Treasures, this.Generators, this.Messages,
                    this.ObjActs, this.SpawnPoints, this.MapOffsets, this.Navmeshes, this.Environments,
                    this.WindSFX, this.PatrolInfo, this.DarkLocks, this.PlatoonInfo, this.MultiSummons,
                    this.Others);
            IReadOnlyList<IMsbEvent> IMsbParam<IMsbEvent>.GetEntries() => this.GetEntries();

            internal override Event ReadEntry(BinaryReaderEx br) {
                EventType type = br.GetEnum32<EventType>(br.Position + 0xC);
                return type switch {
                    EventType.Sound => this.Sounds.EchoAdd(new Event.Sound(br)),
                    EventType.SFX => this.SFX.EchoAdd(new Event.SFX(br)),
                    EventType.Treasure => this.Treasures.EchoAdd(new Event.Treasure(br)),
                    EventType.Generator => this.Generators.EchoAdd(new Event.Generator(br)),
                    EventType.Message => this.Messages.EchoAdd(new Event.Message(br)),
                    EventType.ObjAct => this.ObjActs.EchoAdd(new Event.ObjAct(br)),
                    EventType.SpawnPoint => this.SpawnPoints.EchoAdd(new Event.SpawnPoint(br)),
                    EventType.MapOffset => this.MapOffsets.EchoAdd(new Event.MapOffset(br)),
                    EventType.Navmesh => this.Navmeshes.EchoAdd(new Event.Navmesh(br)),
                    EventType.Environment => this.Environments.EchoAdd(new Event.Environment(br)),
                    EventType.WindSFX => this.WindSFX.EchoAdd(new Event.WindSFX(br)),
                    EventType.PatrolInfo => this.PatrolInfo.EchoAdd(new Event.PatrolInfo(br)),
                    EventType.DarkLock => this.DarkLocks.EchoAdd(new Event.DarkLock(br)),
                    EventType.PlatoonInfo => this.PlatoonInfo.EchoAdd(new Event.PlatoonInfo(br)),
                    EventType.MultiSummon => this.MultiSummons.EchoAdd(new Event.MultiSummon(br)),
                    EventType.Other => this.Others.EchoAdd(new Event.Other(br)),
                    _ => throw new NotImplementedException($"Unsupported event type: {type}"),
                };
            }
        }

        /// <summary>
        /// Common data for all dynamic events.
        /// </summary>
        public abstract class Event : Entry, IMsbEvent {
            private protected abstract EventType Type { get; }
            private protected abstract bool HasTypeData { get; }

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

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE0C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE0D { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE0E { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE0F { get; set; }

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
                long nameOffset = br.ReadInt64();
                this.EventID = br.ReadInt32();
                _ = br.AssertUInt32((uint)this.Type);
                _ = br.ReadInt32(); // ID
                _ = br.AssertInt32(0);
                long entityDataOffset = br.ReadInt64();
                long typeDataOffset = br.ReadInt64();

                if (nameOffset == 0) {
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {this.GetType()}.");
                }

                if (entityDataOffset == 0) {
                    throw new InvalidDataException($"{nameof(entityDataOffset)} must not be 0 in type {this.GetType()}.");
                }

                if (this.HasTypeData ^ typeDataOffset != 0) {
                    throw new InvalidDataException($"Unexpected {nameof(typeDataOffset)} 0x{typeDataOffset:X} in type {this.GetType()}.");
                }

                br.Position = start + nameOffset;
                this.Name = br.ReadUTF16();

                br.Position = start + entityDataOffset;
                this.PartIndex = br.ReadInt32();
                this.RegionIndex = br.ReadInt32();
                this.EntityID = br.ReadInt32();
                this.UnkE0C = br.ReadByte();
                this.UnkE0D = br.ReadByte();
                this.UnkE0E = br.ReadByte();
                this.UnkE0F = br.ReadByte();

                if (this.HasTypeData) {
                    br.Position = start + typeDataOffset;
                    this.ReadTypeData(br);
                }
            }

            private protected virtual void ReadTypeData(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(ReadTypeData)}.");

            internal override void Write(BinaryWriterEx bw, int id) {
                long start = bw.Position;
                bw.ReserveInt64("NameOffset");
                bw.WriteInt32(this.EventID);
                bw.WriteUInt32((uint)this.Type);
                bw.WriteInt32(id);
                bw.WriteInt32(0);
                bw.ReserveInt64("EntityDataOffset");
                bw.ReserveInt64("TypeDataOffset");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(this.Name, true);
                bw.Pad(8);

                bw.FillInt64("EntityDataOffset", bw.Position - start);
                bw.WriteInt32(this.PartIndex);
                bw.WriteInt32(this.RegionIndex);
                bw.WriteInt32(this.EntityID);
                bw.WriteByte(this.UnkE0C);
                bw.WriteByte(this.UnkE0D);
                bw.WriteByte(this.UnkE0E);
                bw.WriteByte(this.UnkE0F);

                if (this.HasTypeData) {
                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                    this.WriteTypeData(bw);
                } else {
                    bw.FillInt64("TypeDataOffset", 0);
                }
            }

            private protected virtual void WriteTypeData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(WriteTypeData)}.");

            internal virtual void GetNames(MSBB msb, Entries entries) {
                this.PartName = MSB.FindName(entries.Parts, this.PartIndex);
                this.RegionName = MSB.FindName(entries.Regions, this.RegionIndex);
            }

            internal virtual void GetIndices(MSBB msb, Entries entries) {
                this.PartIndex = MSB.FindIndex(entries.Parts, this.PartName);
                this.RegionIndex = MSB.FindIndex(entries.Regions, this.RegionName);
            }

            /// <summary>
            /// Returns the type and name of the event.
            /// </summary>
            public override string ToString() => $"{this.Type} {this.Name}";

            /// <summary>
            /// An area-based music or sound effect.
            /// </summary>
            public class Sound : Event {
                private protected override EventType Type => EventType.Sound;
                private protected override bool HasTypeData => true;

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
                    this.SoundType = br.ReadInt32();
                    this.SoundID = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.SoundType);
                    bw.WriteInt32(this.SoundID);
                }
            }

            /// <summary>
            /// A fixed particle effect.
            /// </summary>
            public class SFX : Event {
                private protected override EventType Type => EventType.SFX;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// ID of the particle effect.
                /// </summary>
                public int EffectID { get; set; }

                /// <summary>
                /// Stops the effect from playing automatically.
                /// </summary>
                public bool StartDisabled { get; set; }

                /// <summary>
                /// Creates an SFX with default values.
                /// </summary>
                public SFX() : base($"{nameof(Event)}: {nameof(SFX)}") { }

                internal SFX(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.EffectID = br.ReadInt32();
                    this.StartDisabled = br.AssertInt32(0, 1) == 1;
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.EffectID);
                    bw.WriteInt32(this.StartDisabled ? 1 : 0);
                }
            }

            /// <summary>
            /// A pick-uppable item.
            /// </summary>
            public class Treasure : Event {
                private protected override EventType Type => EventType.Treasure;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// The part that the treasure is attached to, such as an item corpse.
                /// </summary>
                public string TreasurePartName { get; set; }
                private int TreasurePartIndex;

                /// <summary>
                /// First itemlot given by the treasure.
                /// </summary>
                public int ItemLot1 { get; set; }

                /// <summary>
                /// Second itemlot given by the treasure.
                /// </summary>
                public int ItemLot2 { get; set; }

                /// <summary>
                /// Third itemlot given by the treasure.
                /// </summary>
                public int ItemLot3 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT1C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT20 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT28 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT2C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT30 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT34 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT38 { get; set; }

                /// <summary>
                /// Unknown; possible the pickup anim.
                /// </summary>
                public int UnkT3C { get; set; }

                /// <summary>
                /// Changes the text of the pickup prompt.
                /// </summary>
                public bool InChest { get; set; }

                /// <summary>
                /// Whether the treasure should be hidden by default.
                /// </summary>
                public bool StartDisabled { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT42 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT44 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT48 { get; set; }

                /// <summary>
                /// Creates a Treasure with default values.
                /// </summary>
                public Treasure() : base($"{nameof(Event)}: {nameof(Treasure)}") { }

                internal Treasure(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    this.TreasurePartIndex = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    this.ItemLot1 = br.ReadInt32();
                    this.ItemLot2 = br.ReadInt32();
                    this.ItemLot3 = br.ReadInt32();
                    this.UnkT1C = br.ReadInt32();
                    this.UnkT20 = br.ReadInt32();
                    this.UnkT24 = br.ReadInt32();
                    this.UnkT28 = br.ReadInt32();
                    this.UnkT2C = br.ReadInt32();
                    this.UnkT30 = br.ReadInt32();
                    this.UnkT34 = br.ReadInt32();
                    this.UnkT38 = br.ReadInt32();
                    this.UnkT3C = br.ReadInt32();
                    this.InChest = br.ReadBoolean();
                    this.StartDisabled = br.ReadBoolean();
                    this.UnkT42 = br.ReadInt16();
                    this.UnkT44 = br.ReadInt32();
                    this.UnkT48 = br.ReadInt32();
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(this.TreasurePartIndex);
                    bw.WriteInt32(0);
                    bw.WriteInt32(this.ItemLot1);
                    bw.WriteInt32(this.ItemLot2);
                    bw.WriteInt32(this.ItemLot3);
                    bw.WriteInt32(this.UnkT1C);
                    bw.WriteInt32(this.UnkT20);
                    bw.WriteInt32(this.UnkT24);
                    bw.WriteInt32(this.UnkT28);
                    bw.WriteInt32(this.UnkT2C);
                    bw.WriteInt32(this.UnkT30);
                    bw.WriteInt32(this.UnkT34);
                    bw.WriteInt32(this.UnkT38);
                    bw.WriteInt32(this.UnkT3C);
                    bw.WriteBoolean(this.InChest);
                    bw.WriteBoolean(this.StartDisabled);
                    bw.WriteInt16(this.UnkT42);
                    bw.WriteInt32(this.UnkT44);
                    bw.WriteInt32(this.UnkT48);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSBB msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.TreasurePartName = MSB.FindName(entries.Parts, this.TreasurePartIndex);
                }

                internal override void GetIndices(MSBB msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.TreasurePartIndex = MSB.FindIndex(entries.Parts, this.TreasurePartName);
                }
            }

            /// <summary>
            /// A repeating enemy spawner.
            /// </summary>
            public class Generator : Event {
                private protected override EventType Type => EventType.Generator;
                private protected override bool HasTypeData => true;

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
                /// Unknown.
                /// </summary>
                public byte UnkT11 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT12 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT13 { get; set; }

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
                    this.SpawnPointNames = new string[8];
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
                    this.UnkT11 = br.ReadByte();
                    this.UnkT12 = br.ReadByte();
                    this.UnkT13 = br.ReadByte();
                    br.AssertPattern(0x1C, 0x00);
                    this.SpawnPointIndices = br.ReadInt32s(8);
                    this.SpawnPartIndices = br.ReadInt32s(32);
                    br.AssertPattern(0x30, 0x00);
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
                    bw.WriteByte(this.UnkT11);
                    bw.WriteByte(this.UnkT12);
                    bw.WriteByte(this.UnkT13);
                    bw.WritePattern(0x1C, 0x00);
                    bw.WriteInt32s(this.SpawnPointIndices);
                    bw.WriteInt32s(this.SpawnPartIndices);
                    bw.WritePattern(0x30, 0x00);
                }

                internal override void GetNames(MSBB msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.SpawnPointNames = MSB.FindNames(entries.Regions, this.SpawnPointIndices);
                    this.SpawnPartNames = MSB.FindNames(entries.Parts, this.SpawnPartIndices);
                }

                internal override void GetIndices(MSBB msb, Entries entries) {
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
                private protected override bool HasTypeData => true;

                /// <summary>
                /// FMG text ID to display.
                /// </summary>
                public short MessageID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT02 { get; set; }

                /// <summary>
                /// Whether the Message requires Seek Guidance to see.
                /// </summary>
                public bool Hidden { get; set; }

                /// <summary>
                /// Creates a Message with default values.
                /// </summary>
                public Message() : base($"{nameof(Event)}: {nameof(Message)}") { }

                internal Message(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.MessageID = br.ReadInt16();
                    this.UnkT02 = br.ReadInt16();
                    this.Hidden = br.ReadBoolean();
                    _ = br.AssertByte(0);
                    _ = br.AssertInt16(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt16(this.MessageID);
                    bw.WriteInt16(this.UnkT02);
                    bw.WriteBoolean(this.Hidden);
                    bw.WriteByte(0);
                    bw.WriteInt16(0);
                }
            }

            /// <summary>
            /// Represents an interaction with an object.
            /// </summary>
            public class ObjAct : Event {
                /// <summary>
                /// Unknown.
                /// </summary>
                public enum StateType : byte {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
                    Default = 0,
                    Door = 1,
                    Loop = 2,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
                }

                private protected override EventType Type => EventType.ObjAct;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown how this differs from the Event EntityID.
                /// </summary>
                public int ObjActEntityID { get; set; }

                /// <summary>
                /// The object that the ObjAct controls.
                /// </summary>
                public string ObjActPartName { get; set; }
                private int ObjActPartIndex;

                /// <summary>
                /// ID in ObjActParam that configures the ObjAct.
                /// </summary>
                public int ObjActParamID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public StateType ObjActState { get; set; }

                /// <summary>
                /// Unknown, probably enables or disables the ObjAct.
                /// </summary>
                public int EventFlagID { get; set; }

                /// <summary>
                /// Creates an ObjAct with default values.
                /// </summary>
                public ObjAct() : base($"{nameof(Event)}: {nameof(ObjAct)}") {
                    this.ObjActEntityID = -1;
                    this.ObjActParamID = -1;
                    this.EventFlagID = -1;
                }

                internal ObjAct(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.ObjActEntityID = br.ReadInt32();
                    this.ObjActPartIndex = br.ReadInt32();
                    this.ObjActParamID = br.ReadInt32();
                    this.ObjActState = br.ReadEnum8<StateType>();
                    _ = br.AssertByte(0);
                    _ = br.AssertInt16(0);
                    this.EventFlagID = br.ReadInt32();
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.ObjActEntityID);
                    bw.WriteInt32(this.ObjActPartIndex);
                    bw.WriteInt32(this.ObjActParamID);
                    bw.WriteByte((byte)this.ObjActState);
                    bw.WriteByte(0);
                    bw.WriteInt16(0);
                    bw.WriteInt32(this.EventFlagID);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSBB msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.ObjActPartName = MSB.FindName(entries.Parts, this.ObjActPartIndex);
                }

                internal override void GetIndices(MSBB msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.ObjActPartIndex = MSB.FindIndex(entries.Parts, this.ObjActPartName);
                }
            }

            /// <summary>
            /// Unknown what this accomplishes beyond just having the region.
            /// </summary>
            public class SpawnPoint : Event {
                private protected override EventType Type => EventType.SpawnPoint;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Point for the SpawnPoint to spawn at.
                /// </summary>
                public string SpawnPointName { get; set; }
                private int SpawnPointIndex;

                /// <summary>
                /// Creates a SpawnPoint with default values.
                /// </summary>
                public SpawnPoint() : base($"{nameof(Event)}: {nameof(SpawnPoint)}") { }

                internal SpawnPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.SpawnPointIndex = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.SpawnPointIndex);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSBB msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.SpawnPointName = MSB.FindName(entries.Regions, this.SpawnPointIndex);
                }

                internal override void GetIndices(MSBB msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.SpawnPointIndex = MSB.FindIndex(entries.Regions, this.SpawnPointName);
                }
            }

            /// <summary>
            /// The origin of the map, already accounted for in MSB positions.
            /// </summary>
            public class MapOffset : Event {
                private protected override EventType Type => EventType.MapOffset;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Position of the map.
                /// </summary>
                public Vector3 Position { get; set; }

                /// <summary>
                /// Rotation of the map.
                /// </summary>
                public float Degree { get; set; }

                /// <summary>
                /// Creates a MapOffset with default values.
                /// </summary>
                public MapOffset() : base($"{nameof(Event)}: {nameof(MapOffset)}") { }

                internal MapOffset(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.Position = br.ReadVector3();
                    this.Degree = br.ReadSingle();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteVector3(this.Position);
                    bw.WriteSingle(this.Degree);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Navmesh : Event {
                private protected override EventType Type => EventType.Navmesh;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public string NavmeshRegionName { get; set; }
                private int NavmeshRegionIndex;

                /// <summary>
                /// Creates a Navmesh with default values.
                /// </summary>
                public Navmesh() : base($"{nameof(Event)}: {nameof(Navmesh)}") { }

                internal Navmesh(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.NavmeshRegionIndex = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.NavmeshRegionIndex);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSBB msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.NavmeshRegionName = MSB.FindName(entries.Regions, this.NavmeshRegionIndex);
                }

                internal override void GetIndices(MSBB msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.NavmeshRegionIndex = MSB.FindIndex(entries.Regions, this.NavmeshRegionName);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Environment : Event {
                private protected override EventType Type => EventType.Environment;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT14 { get; set; }

                /// <summary>
                /// Creates an Environment with default values.
                /// </summary>
                public Environment() : base($"{nameof(Event)}: {nameof(Environment)}") { }

                internal Environment(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadInt32();
                    this.UnkT04 = br.ReadSingle();
                    this.UnkT08 = br.ReadSingle();
                    this.UnkT0C = br.ReadSingle();
                    this.UnkT10 = br.ReadSingle();
                    this.UnkT14 = br.ReadSingle();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.UnkT00);
                    bw.WriteSingle(this.UnkT04);
                    bw.WriteSingle(this.UnkT08);
                    bw.WriteSingle(this.UnkT0C);
                    bw.WriteSingle(this.UnkT10);
                    bw.WriteSingle(this.UnkT14);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class WindSFX : Event {
                private protected override EventType Type => EventType.WindSFX;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Particle effect ID to play.
                /// </summary>
                public int EffectID { get; set; }

                /// <summary>
                /// Presumably the area where the wind takes effect.
                /// </summary>
                public string WindRegionName { get; set; }
                private int WindRegionIndex;

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT08 { get; set; }

                /// <summary>
                /// Creates a Wind with default values.
                /// </summary>
                public WindSFX() : base($"{nameof(Event)}: {nameof(WindSFX)}") { }

                internal WindSFX(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.EffectID = br.ReadInt32();
                    this.WindRegionIndex = br.ReadInt32();
                    this.UnkT08 = br.ReadSingle();
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.EffectID);
                    bw.WriteInt32(this.WindRegionIndex);
                    bw.WriteSingle(this.UnkT08);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSBB msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.WindRegionName = MSB.FindName(entries.Regions, this.WindRegionIndex);
                }

                internal override void GetIndices(MSBB msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.WindRegionIndex = MSB.FindIndex(entries.Regions, this.WindRegionName);
                }
            }

            /// <summary>
            /// A simple list of points defining a path for enemies to take.
            /// </summary>
            public class PatrolInfo : Event {
                private protected override EventType Type => EventType.PatrolInfo;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown; probably some kind of route type.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// List of points in the route.
                /// </summary>
                public string[] WalkPointNames { get; private set; }
                private short[] WalkPointIndices;

                /// <summary>
                /// Creates a WalkRoute with default values.
                /// </summary>
                public PatrolInfo() : base($"{nameof(Event)}: {nameof(PatrolInfo)}") => this.WalkPointNames = new string[32];

                private protected override void DeepCopyTo(Event evnt) {
                    var walkRoute = (PatrolInfo)evnt;
                    walkRoute.WalkPointNames = (string[])this.WalkPointNames.Clone();
                }

                internal PatrolInfo(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    this.WalkPointIndices = br.ReadInt16s(32);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.UnkT00);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt16s(this.WalkPointIndices);
                }

                internal override void GetNames(MSBB msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.WalkPointNames = new string[this.WalkPointIndices.Length];
                    for (int i = 0; i < this.WalkPointIndices.Length; i++) {
                        this.WalkPointNames[i] = MSB.FindName(entries.Regions, this.WalkPointIndices[i]);
                    }
                }

                internal override void GetIndices(MSBB msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.WalkPointIndices = new short[this.WalkPointNames.Length];
                    for (int i = 0; i < this.WalkPointNames.Length; i++) {
                        this.WalkPointIndices[i] = (short)MSB.FindIndex(entries.Regions, this.WalkPointNames[i]);
                    }
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class DarkLock : Event {
                private protected override EventType Type => EventType.DarkLock;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates a DarkLock with default values.
                /// </summary>
                public DarkLock() : base($"{nameof(Event)}: {nameof(DarkLock)}") { }

                internal DarkLock(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class PlatoonInfo : Event {
                private protected override EventType Type => EventType.PlatoonInfo;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int PlatoonIDScriptActivate { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int State { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public string[] GroupPartsNames { get; private set; }
                private int[] GroupPartsIndices;

                /// <summary>
                /// Creates a GroupTour with default values.
                /// </summary>
                public PlatoonInfo() : base($"{nameof(Event)}: {nameof(PlatoonInfo)}") => this.GroupPartsNames = new string[32];

                private protected override void DeepCopyTo(Event evnt) {
                    var groupTour = (PlatoonInfo)evnt;
                    groupTour.GroupPartsNames = (string[])this.GroupPartsNames.Clone();
                }

                internal PlatoonInfo(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.PlatoonIDScriptActivate = br.ReadInt32();
                    this.State = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    this.GroupPartsIndices = br.ReadInt32s(32);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.PlatoonIDScriptActivate);
                    bw.WriteInt32(this.State);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32s(this.GroupPartsIndices);
                }

                internal override void GetNames(MSBB msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.GroupPartsNames = MSB.FindNames(entries.Parts, this.GroupPartsIndices);
                }

                internal override void GetIndices(MSBB msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.GroupPartsIndices = MSB.FindIndices(entries.Parts, this.GroupPartsNames);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class MultiSummon : Event {
                private protected override EventType Type => EventType.MultiSummon;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT06 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT0A { get; set; }

                /// <summary>
                /// Creates a MultiSummon with default values.
                /// </summary>
                public MultiSummon() : base($"{nameof(Event)}: {nameof(MultiSummon)}") { }

                internal MultiSummon(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadInt32();
                    this.UnkT04 = br.ReadInt16();
                    this.UnkT06 = br.ReadInt16();
                    this.UnkT08 = br.ReadInt16();
                    this.UnkT0A = br.ReadInt16();
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.UnkT00);
                    bw.WriteInt16(this.UnkT04);
                    bw.WriteInt16(this.UnkT06);
                    bw.WriteInt16(this.UnkT08);
                    bw.WriteInt16(this.UnkT0A);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Other : Event {
                private protected override EventType Type => EventType.Other;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates an Other with default values.
                /// </summary>
                public Other() : base($"{nameof(Event)}: {nameof(Other)}") { }

                internal Other(BinaryReaderEx br) : base(br) { }
            }
        }
    }
}
