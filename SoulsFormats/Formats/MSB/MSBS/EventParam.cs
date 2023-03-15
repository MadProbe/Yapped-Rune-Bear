using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using SoulsFormats.Formats.MSB;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MSBS {
        internal enum EventType : uint {
            //Light = 0,
            //Sound = 1,
            //SFX = 2,
            //Wind = 3,
            Treasure = 4,
            Generator = 5,
            //Message = 6,
            ObjAct = 7,
            //SpawnPoint = 8,
            MapOffset = 9,
            //Navmesh = 10,
            //Environment = 11,
            //PseudoMultiplayer = 12,
            //WindSFX = 13,
            PatrolInfo = 14,
            PlatoonInfo = 15,
            //DarkSight = 16,
            ResourceItemInfo = 17,
            GrassLodParam = 18,
            //AutoDrawGroupSettings = 19,
            SkitInfo = 20,
            PlacementGroup = 21,
            PartsGroup = 22,
            Talk = 23,
            AutoDrawGroupCollision = 24,
            Other = 0xFFFFFFFF,
        }

        /// <summary>
        /// Dynamic or interactive systems such as item pickups, levers, enemy spawners, etc.
        /// </summary>
        public class EventParam : Param<Event>, IMsbParam<IMsbEvent> {
            /// <summary>
            /// Item pickups out in the open or inside containers.
            /// </summary>
            public List<Event.Treasure> Treasures { get; set; }

            /// <summary>
            /// Enemy spawners.
            /// </summary>
            public List<Event.Generator> Generators { get; set; }

            /// <summary>
            /// Interactive objects like levers and doors.
            /// </summary>
            public List<Event.ObjAct> ObjActs { get; set; }

            /// <summary>
            /// Indicates a shift of the entire map; already accounted for in MSB positions, but must be applied to other formats such as BTL. Should only be one per map, if any.
            /// </summary>
            public List<Event.MapOffset> MapOffsets { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.PatrolInfo> PatrolInfo { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.PlatoonInfo> PlatoonInfo { get; set; }

            /// <summary>
            /// Resource items such as spirit emblems placed in the map.
            /// </summary>
            public List<Event.ResourceItemInfo> ResourceItemInfo { get; set; }

            /// <summary>
            /// Sets the grass lod parameters for the map. Should only be one per map, if any.
            /// </summary>
            public List<Event.GrassLodParam> GrassLodParams { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.SkitInfo> SkitInfo { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.PlacementGroup> PlacementGroups { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.PartsGroup> PartsGroups { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.Talk> Talks { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.AutoDrawGroupCollision> AutoDrawGroupCollisions { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Event.Other> Others { get; set; }

            /// <summary>
            /// Creates an empty EventParam with the default version.
            /// </summary>
            public EventParam() : base(35, "EVENT_PARAM_ST") {
                this.Treasures = new List<Event.Treasure>();
                this.Generators = new List<Event.Generator>();
                this.ObjActs = new List<Event.ObjAct>();
                this.MapOffsets = new List<Event.MapOffset>();
                this.PatrolInfo = new List<Event.PatrolInfo>();
                this.PlatoonInfo = new List<Event.PlatoonInfo>();
                this.ResourceItemInfo = new List<Event.ResourceItemInfo>();
                this.GrassLodParams = new List<Event.GrassLodParam>();
                this.SkitInfo = new List<Event.SkitInfo>();
                this.PlacementGroups = new List<Event.PlacementGroup>();
                this.PartsGroups = new List<Event.PartsGroup>();
                this.Talks = new List<Event.Talk>();
                this.AutoDrawGroupCollisions = new List<Event.AutoDrawGroupCollision>();
                this.Others = new List<Event.Other>();
            }

            /// <summary>
            /// Adds an event to the appropriate list for its type; returns the event.
            /// </summary>
            public Event Add(Event evnt) {
                switch (evnt) {
                    case Event.Treasure e: this.Treasures.Add(e); break;
                    case Event.Generator e: this.Generators.Add(e); break;
                    case Event.ObjAct e: this.ObjActs.Add(e); break;
                    case Event.MapOffset e: this.MapOffsets.Add(e); break;
                    case Event.PatrolInfo e: this.PatrolInfo.Add(e); break;
                    case Event.PlatoonInfo e: this.PlatoonInfo.Add(e); break;
                    case Event.ResourceItemInfo e: this.ResourceItemInfo.Add(e); break;
                    case Event.GrassLodParam e: this.GrassLodParams.Add(e); break;
                    case Event.SkitInfo e: this.SkitInfo.Add(e); break;
                    case Event.PlacementGroup e: this.PlacementGroups.Add(e); break;
                    case Event.PartsGroup e: this.PartsGroups.Add(e); break;
                    case Event.Talk e: this.Talks.Add(e); break;
                    case Event.AutoDrawGroupCollision e: this.AutoDrawGroupCollisions.Add(e); break;
                    case Event.Other e: this.Others.Add(e); break;

                    default:
                        throw new ArgumentException($"Unrecognized type {evnt.GetType()}.", nameof(evnt));
                }
                return evnt;
            }
            IMsbEvent IMsbParam<IMsbEvent>.Add(IMsbEvent item) => this.Add((Event)item);

            /// <summary>
            /// Returns every Event in the order they'll be written.
            /// </summary>
            public override List<Event> GetEntries() => SFUtil.ConcatAll<Event>(
                    this.Treasures, this.Generators, this.ObjActs, this.MapOffsets, this.PatrolInfo,
                    this.PlatoonInfo, this.ResourceItemInfo, this.GrassLodParams, this.SkitInfo, this.PlacementGroups,
                    this.PartsGroups, this.Talks, this.AutoDrawGroupCollisions, this.Others);
            IReadOnlyList<IMsbEvent> IMsbParam<IMsbEvent>.GetEntries() => this.GetEntries();

            internal override Event ReadEntry(BinaryReaderEx br) {
                EventType type = br.GetEnum32<EventType>(br.Position + 0xC);
                return type switch {
                    EventType.Treasure => this.Treasures.EchoAdd(new Event.Treasure(br)),
                    EventType.Generator => this.Generators.EchoAdd(new Event.Generator(br)),
                    EventType.ObjAct => this.ObjActs.EchoAdd(new Event.ObjAct(br)),
                    EventType.MapOffset => this.MapOffsets.EchoAdd(new Event.MapOffset(br)),
                    EventType.PatrolInfo => this.PatrolInfo.EchoAdd(new Event.PatrolInfo(br)),
                    EventType.PlatoonInfo => this.PlatoonInfo.EchoAdd(new Event.PlatoonInfo(br)),
                    EventType.ResourceItemInfo => this.ResourceItemInfo.EchoAdd(new Event.ResourceItemInfo(br)),
                    EventType.GrassLodParam => this.GrassLodParams.EchoAdd(new Event.GrassLodParam(br)),
                    EventType.SkitInfo => this.SkitInfo.EchoAdd(new Event.SkitInfo(br)),
                    EventType.PlacementGroup => this.PlacementGroups.EchoAdd(new Event.PlacementGroup(br)),
                    EventType.PartsGroup => this.PartsGroups.EchoAdd(new Event.PartsGroup(br)),
                    EventType.Talk => this.Talks.EchoAdd(new Event.Talk(br)),
                    EventType.AutoDrawGroupCollision => this.AutoDrawGroupCollisions.EchoAdd(new Event.AutoDrawGroupCollision(br)),
                    EventType.Other => this.Others.EchoAdd(new Event.Other(br)),
                    _ => throw new NotImplementedException($"Unimplemented event type: {type}"),
                };
            }
        }

        /// <summary>
        /// A dynamic or interactive system.
        /// </summary>
        public abstract class Event : Entry, IMsbEvent {
            private protected abstract EventType Type { get; }
            private protected abstract bool HasTypeData { get; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int EventID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public string PartName { get; set; }
            private int PartIndex;

            /// <summary>
            /// Unknown.
            /// </summary>
            public string RegionName { get; set; }
            private int RegionIndex;

            /// <summary>
            /// Identifies the Event in event scripts.
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
                long nameOffset = br.ReadInt64();
                this.EventID = br.ReadInt32();
                _ = br.AssertUInt32((uint)this.Type);
                _ = br.ReadInt32(); // ID
                _ = br.AssertInt32(0);
                long baseDataOffset = br.ReadInt64();
                long typeDataOffset = br.ReadInt64();

                if (nameOffset == 0) {
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {this.GetType()}.");
                }

                if (baseDataOffset == 0) {
                    throw new InvalidDataException($"{nameof(baseDataOffset)} must not be 0 in type {this.GetType()}.");
                }

                if (this.HasTypeData ^ typeDataOffset != 0) {
                    throw new InvalidDataException($"Unexpected {nameof(typeDataOffset)} 0x{typeDataOffset:X} in type {this.GetType()}.");
                }

                br.Position = start + nameOffset;
                this.Name = br.ReadUTF16();

                br.Position = start + baseDataOffset;
                this.PartIndex = br.ReadInt32();
                this.RegionIndex = br.ReadInt32();
                this.EntityID = br.ReadInt32();
                _ = br.AssertInt32(0);

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
                bw.ReserveInt64("BaseDataOffset");
                bw.ReserveInt64("TypeDataOffset");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(this.Name, true);
                bw.Pad(8);

                bw.FillInt64("BaseDataOffset", bw.Position - start);
                bw.WriteInt32(this.PartIndex);
                bw.WriteInt32(this.RegionIndex);
                bw.WriteInt32(this.EntityID);
                bw.WriteInt32(0);

                if (this.HasTypeData) {
                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                    this.WriteTypeData(bw);
                } else {
                    bw.FillInt64("TypeDataOffset", 0);
                }
            }

            private protected virtual void WriteTypeData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(ReadTypeData)}.");

            internal virtual void GetNames(MSBS msb, Entries entries) {
                this.PartName = MSB.FindName(entries.Parts, this.PartIndex);
                this.RegionName = MSB.FindName(entries.Regions, this.RegionIndex);
            }

            internal virtual void GetIndices(MSBS msb, Entries entries) {
                this.PartIndex = MSB.FindIndex(entries.Parts, this.PartName);
                this.RegionIndex = MSB.FindIndex(entries.Regions, this.RegionName);
            }

            /// <summary>
            /// Returns the type and name of the event as a string.
            /// </summary>
            public override string ToString() => $"{this.Type} {this.Name}";

            /// <summary>
            /// An item pickup in the open or inside a container.
            /// </summary>
            public class Treasure : Event {
                private protected override EventType Type => EventType.Treasure;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// The part that the treasure is attached to.
                /// </summary>
                public string TreasurePartName { get; set; }
                private int TreasurePartIndex;

                /// <summary>
                /// The item lot to be given.
                /// </summary>
                public int ItemLotID { get; set; }

                /// <summary>
                /// If not -1, uses an entry from ActionButtonParam for the pickup prompt.
                /// </summary>
                public int ActionButtonID { get; set; }

                /// <summary>
                /// Animation to play when taking this treasure.
                /// </summary>
                public int PickupAnimID { get; set; }

                /// <summary>
                /// Changes the text of the pickup prompt.
                /// </summary>
                public bool InChest { get; set; }

                /// <summary>
                /// Whether the treasure should be hidden by default.
                /// </summary>
                public bool StartDisabled { get; set; }

                /// <summary>
                /// Creates a Treasure with default values.
                /// </summary>
                public Treasure() : base($"{nameof(Event)}: {nameof(Treasure)}") {
                    this.ItemLotID = -1;
                    this.ActionButtonID = -1;
                    this.PickupAnimID = -1;
                }

                internal Treasure(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    this.TreasurePartIndex = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    this.ItemLotID = br.ReadInt32();
                    br.AssertPattern(0x24, 0xFF);
                    this.ActionButtonID = br.ReadInt32();
                    this.PickupAnimID = br.ReadInt32();
                    this.InChest = br.ReadBoolean();
                    this.StartDisabled = br.ReadBoolean();
                    _ = br.AssertInt16(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(this.TreasurePartIndex);
                    bw.WriteInt32(0);
                    bw.WriteInt32(this.ItemLotID);
                    bw.WritePattern(0x24, 0xFF);
                    bw.WriteInt32(this.ActionButtonID);
                    bw.WriteInt32(this.PickupAnimID);
                    bw.WriteBoolean(this.InChest);
                    bw.WriteBoolean(this.StartDisabled);
                    bw.WriteInt16(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSBS msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.TreasurePartName = MSB.FindName(entries.Parts, this.TreasurePartIndex);
                }

                internal override void GetIndices(MSBS msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.TreasurePartIndex = MSB.FindIndex(entries.Parts, this.TreasurePartName);
                }
            }

            /// <summary>
            /// An enemy spawner.
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
                public float UnkT14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT18 { get; set; }

                /// <summary>
                /// Regions where parts will spawn from.
                /// </summary>
                public string[] SpawnRegionNames { get; private set; }
                private int[] SpawnRegionIndices;

                /// <summary>
                /// Parts that will be respawned.
                /// </summary>
                public string[] SpawnPartNames { get; private set; }
                private int[] SpawnPartIndices;

                /// <summary>
                /// Creates a Generator with default values.
                /// </summary>
                public Generator() : base($"{nameof(Event)}: {nameof(Generator)}") {
                    this.SpawnRegionNames = new string[8];
                    this.SpawnPartNames = new string[32];
                }

                private protected override void DeepCopyTo(Event evnt) {
                    var generator = (Generator)evnt;
                    generator.SpawnRegionNames = (string[])this.SpawnRegionNames.Clone();
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
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    this.UnkT14 = br.ReadSingle();
                    this.UnkT18 = br.ReadSingle();
                    br.AssertPattern(0x14, 0x00);
                    this.SpawnRegionIndices = br.ReadInt32s(8);
                    br.AssertPattern(0x10, 0x00);
                    this.SpawnPartIndices = br.ReadInt32s(32);
                    br.AssertPattern(0x20, 0x00);
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
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteSingle(this.UnkT14);
                    bw.WriteSingle(this.UnkT18);
                    bw.WritePattern(0x14, 0x00);
                    bw.WriteInt32s(this.SpawnRegionIndices);
                    bw.WritePattern(0x10, 0x00);
                    bw.WriteInt32s(this.SpawnPartIndices);
                    bw.WritePattern(0x20, 0x00);
                }

                internal override void GetNames(MSBS msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.SpawnRegionNames = MSB.FindNames(entries.Regions, this.SpawnRegionIndices);
                    this.SpawnPartNames = MSB.FindNames(entries.Parts, this.SpawnPartIndices);
                }

                internal override void GetIndices(MSBS msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.SpawnRegionIndices = MSB.FindIndices(entries.Regions, this.SpawnRegionNames);
                    this.SpawnPartIndices = MSB.FindIndices(entries.Parts, this.SpawnPartNames);
                }
            }

            /// <summary>
            /// An interactive object.
            /// </summary>
            public class ObjAct : Event {
                private protected override EventType Type => EventType.ObjAct;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown why objacts need an extra entity ID.
                /// </summary>
                public int ObjActEntityID { get; set; }

                /// <summary>
                /// The part to be interacted with.
                /// </summary>
                public string ObjActPartName { get; set; }
                private int ObjActPartIndex;

                /// <summary>
                /// A row in ObjActParam.
                /// </summary>
                public int ObjActID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte StateType { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int EventFlagID { get; set; }

                /// <summary>
                /// Creates an ObjAct with default values.
                /// </summary>
                public ObjAct() : base($"{nameof(Event)}: {nameof(ObjAct)}") {
                    this.ObjActEntityID = -1;
                    this.ObjActID = -1;
                    this.EventFlagID = -1;
                }

                internal ObjAct(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.ObjActEntityID = br.ReadInt32();
                    this.ObjActPartIndex = br.ReadInt32();
                    this.ObjActID = br.ReadInt32();
                    this.StateType = br.ReadByte();
                    _ = br.AssertByte(0);
                    _ = br.AssertInt16(0);
                    this.EventFlagID = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.ObjActEntityID);
                    bw.WriteInt32(this.ObjActPartIndex);
                    bw.WriteInt32(this.ObjActID);
                    bw.WriteByte(this.StateType);
                    bw.WriteByte(0);
                    bw.WriteInt16(0);
                    bw.WriteInt32(this.EventFlagID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSBS msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.ObjActPartName = MSB.FindName(entries.Parts, this.ObjActPartIndex);
                }

                internal override void GetIndices(MSBS msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.ObjActPartIndex = MSB.FindIndex(entries.Parts, this.ObjActPartName);
                }
            }

            /// <summary>
            /// Shifts the entire map; already accounted for in MSB coordinates.
            /// </summary>
            public class MapOffset : Event {
                private protected override EventType Type => EventType.MapOffset;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// How much to shift by.
                /// </summary>
                public Vector3 Position { get; set; }

                /// <summary>
                /// Unknown, but looks like rotation.
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
            public class PatrolInfo : Event {
                private protected override EventType Type => EventType.PatrolInfo;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public string[] WalkRegionNames { get; private set; }
                private short[] WalkRegionIndices;

                /// <summary>
                /// Unknown.
                /// </summary>
                public WREntry[] WREntries { get; set; }

                /// <summary>
                /// Creates a PatrolInfo with default values.
                /// </summary>
                public PatrolInfo() : base($"{nameof(Event)}: {nameof(PatrolInfo)}") {
                    this.WalkRegionNames = new string[32];
                    this.WREntries = new WREntry[5];
                    for (int i = 0; i < 5; i++) {
                        this.WREntries[i] = new WREntry();
                    }
                }

                private protected override void DeepCopyTo(Event evnt) {
                    var walkRoute = (PatrolInfo)evnt;
                    walkRoute.WalkRegionNames = (string[])this.WalkRegionNames.Clone();
                    walkRoute.WREntries = new WREntry[this.WREntries.Length];
                    for (int i = 0; i < this.WREntries.Length; i++) {
                        walkRoute.WREntries[i] = this.WREntries[i].DeepCopy();
                    }
                }

                internal PatrolInfo(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    this.WalkRegionIndices = br.ReadInt16s(32);
                    this.WREntries = new WREntry[5];
                    for (int i = 0; i < 5; i++) {
                        this.WREntries[i] = new WREntry(br);
                    }

                    br.AssertPattern(0x14, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.UnkT00);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt16s(this.WalkRegionIndices);
                    for (int i = 0; i < 5; i++) {
                        this.WREntries[i].Write(bw);
                    }

                    bw.WritePattern(0x14, 0x00);
                }

                internal override void GetNames(MSBS msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.WalkRegionNames = new string[this.WalkRegionIndices.Length];
                    for (int i = 0; i < this.WalkRegionIndices.Length; i++) {
                        this.WalkRegionNames[i] = MSB.FindName(entries.Regions, this.WalkRegionIndices[i]);
                    }

                    foreach (WREntry wrEntry in this.WREntries) {
                        wrEntry.GetNames(entries);
                    }
                }

                internal override void GetIndices(MSBS msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.WalkRegionIndices = new short[this.WalkRegionNames.Length];
                    for (int i = 0; i < this.WalkRegionNames.Length; i++) {
                        this.WalkRegionIndices[i] = (short)MSB.FindIndex(entries.Regions, this.WalkRegionNames[i]);
                    }

                    foreach (WREntry wrEntry in this.WREntries) {
                        wrEntry.GetIndices(entries);
                    }
                }

                /// <summary>
                /// Unknown.
                /// </summary>
                public class WREntry {
                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public string RegionName { get; set; }
                    private short RegionIndex;

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk04 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk08 { get; set; }

                    /// <summary>
                    /// Creates a WREntry with default values.
                    /// </summary>
                    public WREntry() { }

                    /// <summary>
                    /// Creates a deep copy of the entry.
                    /// </summary>
                    public WREntry DeepCopy() => (WREntry)this.MemberwiseClone();

                    internal WREntry(BinaryReaderEx br) {
                        this.RegionIndex = br.ReadInt16();
                        _ = br.AssertInt16(0);
                        this.Unk04 = br.ReadInt32();
                        this.Unk08 = br.ReadInt32();
                    }

                    internal void Write(BinaryWriterEx bw) {
                        bw.WriteInt16(this.RegionIndex);
                        bw.WriteInt16(0);
                        bw.WriteInt32(this.Unk04);
                        bw.WriteInt32(this.Unk08);
                    }

                    internal void GetNames(Entries entries) => this.RegionName = MSB.FindName(entries.Regions, this.RegionIndex);

                    internal void GetIndices(Entries entries) => this.RegionIndex = (short)MSB.FindIndex(entries.Regions, this.RegionName);
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
                public int PlatoonIDScriptActive { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int State { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public string[] GroupPartNames { get; private set; }
                private int[] GroupPartIndices;

                /// <summary>
                /// Creates a PlatoonInfo with default values.
                /// </summary>
                public PlatoonInfo() : base($"{nameof(Event)}: {nameof(PlatoonInfo)}") => this.GroupPartNames = new string[32];

                private protected override void DeepCopyTo(Event evnt) {
                    var groupTour = (PlatoonInfo)evnt;
                    groupTour.GroupPartNames = (string[])this.GroupPartNames.Clone();
                }

                internal PlatoonInfo(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.PlatoonIDScriptActive = br.ReadInt32();
                    this.State = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    this.GroupPartIndices = br.ReadInt32s(32);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.PlatoonIDScriptActive);
                    bw.WriteInt32(this.State);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32s(this.GroupPartIndices);
                }

                internal override void GetNames(MSBS msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.GroupPartNames = MSB.FindNames(entries.Parts, this.GroupPartIndices);
                }

                internal override void GetIndices(MSBS msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.GroupPartIndices = MSB.FindIndices(entries.Parts, this.GroupPartNames);
                }
            }

            /// <summary>
            /// A resource item placed in the map; uses the base Event's region for positioning.
            /// </summary>
            public class ResourceItemInfo : Event {
                private protected override EventType Type => EventType.ResourceItemInfo;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// ID of a row in ResourceItemLotParam that determines the resource(s) to give.
                /// </summary>
                public int ResourceItemLotParamID { get; set; }

                /// <summary>
                /// Creates a ResourceItemInfo with default values.
                /// </summary>
                public ResourceItemInfo() : base($"{nameof(Event)}: {nameof(ResourceItemInfo)}") { }

                internal ResourceItemInfo(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.ResourceItemLotParamID = br.ReadInt32();
                    br.AssertPattern(0x1C, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.ResourceItemLotParamID);
                    bw.WritePattern(0x1C, 0x00);
                }
            }

            /// <summary>
            /// Sets the grass lod parameters for the map.
            /// </summary>
            public class GrassLodParam : Event {
                private protected override EventType Type => EventType.GrassLodParam;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// ID of a row in GrassLodRangeParam.
                /// </summary>
                public int GrassLodRangeParamID { get; set; }

                /// <summary>
                /// Creates a GrassLodParam with default values.
                /// </summary>
                public GrassLodParam() : base($"{nameof(Event)}: {nameof(GrassLodParam)}") { }

                internal GrassLodParam(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.GrassLodRangeParamID = br.ReadInt32();
                    br.AssertPattern(0x1C, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.GrassLodRangeParamID);
                    bw.WritePattern(0x1C, 0x00);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class SkitInfo : Event {
                private protected override EventType Type => EventType.SkitInfo;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT05 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT06 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT07 { get; set; }

                /// <summary>
                /// Creates a SkitInfo with default values.
                /// </summary>
                public SkitInfo() : base($"{nameof(Event)}: {nameof(SkitInfo)}") { }

                internal SkitInfo(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadInt32();
                    this.UnkT04 = br.ReadByte();
                    this.UnkT05 = br.ReadByte();
                    this.UnkT06 = br.ReadByte();
                    this.UnkT07 = br.ReadByte();
                    br.AssertPattern(0x18, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.UnkT00);
                    bw.WriteByte(this.UnkT04);
                    bw.WriteByte(this.UnkT05);
                    bw.WriteByte(this.UnkT06);
                    bw.WriteByte(this.UnkT07);
                    bw.WritePattern(0x18, 0x00);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class PlacementGroup : Event {
                private protected override EventType Type => EventType.PlacementGroup;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public string[] Event21PartNames { get; private set; }
                private int[] Event21PartIndices;

                /// <summary>
                /// Creates a PlacementGroup with default values.
                /// </summary>
                public PlacementGroup() : base($"{nameof(Event)}: {nameof(PlacementGroup)}") => this.Event21PartNames = new string[32];

                private protected override void DeepCopyTo(Event evnt) {
                    var event21 = (PlacementGroup)evnt;
                    event21.Event21PartNames = (string[])this.Event21PartNames.Clone();
                }

                internal PlacementGroup(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) => this.Event21PartIndices = br.ReadInt32s(32);

                private protected override void WriteTypeData(BinaryWriterEx bw) => bw.WriteInt32s(this.Event21PartIndices);

                internal override void GetNames(MSBS msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.Event21PartNames = MSB.FindNames(entries.Parts, this.Event21PartIndices);
                }

                internal override void GetIndices(MSBS msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.Event21PartIndices = MSB.FindIndices(entries.Parts, this.Event21PartNames);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class PartsGroup : Event {
                private protected override EventType Type => EventType.PartsGroup;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates a PartsGroup with default values.
                /// </summary>
                public PartsGroup() : base($"{nameof(Event)}: {nameof(PartsGroup)}") { }

                internal PartsGroup(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Talk : Event {
                private protected override EventType Type => EventType.Talk;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public string[] EnemyNames { get; private set; }
                private int[] EnemyIndices;

                /// <summary>
                /// IDs of talk ESDs.
                /// </summary>
                public int[] TalkIDs { get; private set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT44 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT46 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT48 { get; set; }

                /// <summary>
                /// Creates a Talk with default values.
                /// </summary>
                public Talk() : base($"{nameof(Event)}: {nameof(Talk)}") {
                    this.EnemyNames = new string[8];
                    this.TalkIDs = new int[8];
                }

                private protected override void DeepCopyTo(Event evnt) {
                    var talk = (Talk)evnt;
                    talk.EnemyNames = (string[])this.EnemyNames.Clone();
                    talk.TalkIDs = (int[])this.TalkIDs.Clone();
                }

                internal Talk(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadInt32();
                    this.EnemyIndices = br.ReadInt32s(8);
                    this.TalkIDs = br.ReadInt32s(8);
                    this.UnkT44 = br.ReadInt16();
                    this.UnkT46 = br.ReadInt16();
                    this.UnkT48 = br.ReadInt32();
                    br.AssertPattern(0x34, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.UnkT00);
                    bw.WriteInt32s(this.EnemyIndices);
                    bw.WriteInt32s(this.TalkIDs);
                    bw.WriteInt16(this.UnkT44);
                    bw.WriteInt16(this.UnkT46);
                    bw.WriteInt32(this.UnkT48);
                    bw.WritePattern(0x34, 0x00);
                }

                internal override void GetNames(MSBS msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.EnemyNames = MSB.FindNames(msb.Parts.Enemies, this.EnemyIndices);
                }

                internal override void GetIndices(MSBS msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.EnemyIndices = MSB.FindIndices(msb.Parts.Enemies, this.EnemyNames);
                }
            }

            /// <summary>
            /// Specifies a collision to which an autodrawgroup filming point belongs, whatever that means.
            /// </summary>
            public class AutoDrawGroupCollision : Event {
                private protected override EventType Type => EventType.AutoDrawGroupCollision;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Name of the filming point for the autodrawgroup capture, probably.
                /// </summary>
                public string AutoDrawGroupPointName { get; set; }
                private int AutoDrawGroupPointIndex;

                /// <summary>
                /// The collision that the filming point belongs to, presumably.
                /// </summary>
                public string OwningCollisionName { get; set; }
                private int OwningCollisionIndex;

                /// <summary>
                /// Creates an AutoDrawGroupCollision with default values.
                /// </summary>
                public AutoDrawGroupCollision() : base($"{nameof(Event)}: {nameof(AutoDrawGroupCollision)}") { }

                internal AutoDrawGroupCollision(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.AutoDrawGroupPointIndex = br.ReadInt32();
                    this.OwningCollisionIndex = br.ReadInt32();
                    br.AssertPattern(0x18, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.AutoDrawGroupPointIndex);
                    bw.WriteInt32(this.OwningCollisionIndex);
                    bw.WritePattern(0x18, 0x00);
                }

                internal override void GetNames(MSBS msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.AutoDrawGroupPointName = MSB.FindName(msb.Regions.AutoDrawGroupPoints, this.AutoDrawGroupPointIndex);
                    this.OwningCollisionName = MSB.FindName(msb.Parts.Collisions, this.OwningCollisionIndex);
                }

                internal override void GetIndices(MSBS msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.AutoDrawGroupPointIndex = MSB.FindIndex(msb.Regions.AutoDrawGroupPoints, this.AutoDrawGroupPointName);
                    this.OwningCollisionIndex = MSB.FindIndex(msb.Parts.Collisions, this.OwningCollisionName);
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
