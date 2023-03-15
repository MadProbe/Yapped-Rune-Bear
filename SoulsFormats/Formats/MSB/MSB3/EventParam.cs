using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using SoulsFormats.Formats.MSB;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MSB3 {
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
            PseudoMultiplayer = 12,
            //WindSFX = 13,
            PatrolInfo = 14,
            PlatoonInfo = 15,
            //DarkSight = 16,
            Other = 0xFFFFFFFF,
        }

        /// <summary>
        /// Events controlling various interactive or dynamic features in the map.
        /// </summary>
        public class EventParam : Param<Event>, IMsbParam<IMsbEvent> {
            internal override int Version => 3;
            internal override string Type => "EVENT_PARAM_ST";

            /// <summary>
            /// Treasures in the MSB.
            /// </summary>
            public List<Event.Treasure> Treasures { get; set; }

            /// <summary>
            /// Generators in the MSB.
            /// </summary>
            public List<Event.Generator> Generators { get; set; }

            /// <summary>
            /// Object actions in the MSB.
            /// </summary>
            public List<Event.ObjAct> ObjActs { get; set; }

            /// <summary>
            /// Map offsets in the MSB.
            /// </summary>
            public List<Event.MapOffset> MapOffsets { get; set; }

            /// <summary>
            /// Pseudo multiplayer events in the MSB.
            /// </summary>
            public List<Event.PseudoMultiplayer> PseudoMultiplayers { get; set; }

            /// <summary>
            /// Patrol info in the MSB.
            /// </summary>
            public List<Event.PatrolInfo> PatrolInfo { get; set; }

            /// <summary>
            /// Platoon info in the MSB.
            /// </summary>
            public List<Event.PlatoonInfo> PlatoonInfo { get; set; }

            /// <summary>
            /// Other events in the MSB.
            /// </summary>
            public List<Event.Other> Others { get; set; }

            /// <summary>
            /// Creates a new EventParam with no events.
            /// </summary>
            public EventParam() {
                this.Treasures = new List<Event.Treasure>();
                this.Generators = new List<Event.Generator>();
                this.ObjActs = new List<Event.ObjAct>();
                this.MapOffsets = new List<Event.MapOffset>();
                this.PseudoMultiplayers = new List<Event.PseudoMultiplayer>();
                this.PatrolInfo = new List<Event.PatrolInfo>();
                this.PlatoonInfo = new List<Event.PlatoonInfo>();
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
                    case Event.PseudoMultiplayer e: this.PseudoMultiplayers.Add(e); break;
                    case Event.PatrolInfo e: this.PatrolInfo.Add(e); break;
                    case Event.PlatoonInfo e: this.PlatoonInfo.Add(e); break;
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
                    this.Treasures, this.Generators, this.ObjActs, this.MapOffsets, this.PseudoMultiplayers,
                    this.PatrolInfo, this.PlatoonInfo, this.Others);
            IReadOnlyList<IMsbEvent> IMsbParam<IMsbEvent>.GetEntries() => this.GetEntries();

            internal override Event ReadEntry(BinaryReaderEx br) {
                EventType type = br.GetEnum32<EventType>(br.Position + 0xC);
                return type switch {
                    EventType.Treasure => this.Treasures.EchoAdd(new Event.Treasure(br)),
                    EventType.Generator => this.Generators.EchoAdd(new Event.Generator(br)),
                    EventType.ObjAct => this.ObjActs.EchoAdd(new Event.ObjAct(br)),
                    EventType.MapOffset => this.MapOffsets.EchoAdd(new Event.MapOffset(br)),
                    EventType.PseudoMultiplayer => this.PseudoMultiplayers.EchoAdd(new Event.PseudoMultiplayer(br)),
                    EventType.PatrolInfo => this.PatrolInfo.EchoAdd(new Event.PatrolInfo(br)),
                    EventType.PlatoonInfo => this.PlatoonInfo.EchoAdd(new Event.PlatoonInfo(br)),
                    EventType.Other => this.Others.EchoAdd(new Event.Other(br)),
                    _ => throw new NotImplementedException($"Unsupported event type: {type}"),
                };
            }
        }

        /// <summary>
        /// An interactive or dynamic feature of the map.
        /// </summary>
        public abstract class Event : NamedEntry, IMsbEvent {
            private protected abstract EventType Type { get; }

            /// <summary>
            /// The name of this event.
            /// </summary>
            public override string Name { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int EventID { get; set; }

            /// <summary>
            /// The name of a part the event is attached to.
            /// </summary>
            public string PartName { get; set; }
            private int PartIndex;

            /// <summary>
            /// The name of a region the event is attached to.
            /// </summary>
            public string PointName { get; set; }
            private int PointIndex;

            /// <summary>
            /// Used to identify the event in event scripts.
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

                if (typeDataOffset == 0) {
                    throw new InvalidDataException($"{nameof(typeDataOffset)} must not be 0 in type {this.GetType()}.");
                }

                br.Position = start + nameOffset;
                this.Name = br.ReadUTF16();

                br.Position = start + baseDataOffset;
                this.PartIndex = br.ReadInt32();
                this.PointIndex = br.ReadInt32();
                this.EntityID = br.ReadInt32();
                _ = br.AssertInt32(0);

                br.Position = start + typeDataOffset;
                this.ReadTypeData(br);
            }

            private protected abstract void ReadTypeData(BinaryReaderEx br);

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
                bw.WriteInt32(this.PointIndex);
                bw.WriteInt32(this.EntityID);
                bw.WriteInt32(0);

                bw.FillInt64("TypeDataOffset", bw.Position - start);
                this.WriteTypeData(bw);
            }

            private protected abstract void WriteTypeData(BinaryWriterEx bw);

            internal virtual void GetNames(MSB3 msb, Entries entries) {
                this.PartName = MSB.FindName(entries.Parts, this.PartIndex);
                this.PointName = MSB.FindName(entries.Regions, this.PointIndex);
            }

            internal virtual void GetIndices(MSB3 msb, Entries entries) {
                this.PartIndex = MSB.FindIndex(entries.Parts, this.PartName);
                this.PointIndex = MSB.FindIndex(entries.Regions, this.PointName);
            }

            /// <summary>
            /// Returns the type and name of this event.
            /// </summary>
            public override string ToString() => $"{this.Type} : {this.Name}";

            /// <summary>
            /// A pickuppable item.
            /// </summary>
            public class Treasure : Event {
                private protected override EventType Type => EventType.Treasure;

                /// <summary>
                /// The part the treasure is attached to.
                /// </summary>
                public string TreasurePartName { get; set; }
                private int TreasurePartIndex;

                /// <summary>
                /// First item lot given by this treasure.
                /// </summary>
                public int ItemLot1 { get; set; }

                /// <summary>
                /// Second item lot given by this treasure; rarely used.
                /// </summary>
                public int ItemLot2 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT18 { get; set; }

                /// <summary>
                /// If not -1, uses an entry from ActionButtonParam for the pickup prompt.
                /// </summary>
                public int ActionButtonParamID { get; set; }

                /// <summary>
                /// Animation to play when taking this treasure.
                /// </summary>
                public int PickupAnimID { get; set; }

                /// <summary>
                /// Changes the text of the pickup prompt and causes the treasure to be uninteractible by default.
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
                    this.ItemLot1 = -1;
                    this.ItemLot2 = -1;
                    this.UnkT18 = -1;
                    this.ActionButtonParamID = -1;
                    this.PickupAnimID = 60070;
                }

                internal Treasure(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    this.TreasurePartIndex = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    this.ItemLot1 = br.ReadInt32();
                    this.ItemLot2 = br.ReadInt32();
                    this.UnkT18 = br.ReadInt32();
                    _ = br.AssertInt32(-1);
                    _ = br.AssertInt32(-1);
                    _ = br.AssertInt32(-1);
                    _ = br.AssertInt32(-1);
                    _ = br.AssertInt32(-1);
                    _ = br.AssertInt32(-1);
                    _ = br.AssertInt32(-1);
                    this.ActionButtonParamID = br.ReadInt32();
                    this.PickupAnimID = br.ReadInt32();

                    this.InChest = br.ReadBoolean();
                    this.StartDisabled = br.ReadBoolean();
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);

                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(this.TreasurePartIndex);
                    bw.WriteInt32(0);
                    bw.WriteInt32(this.ItemLot1);
                    bw.WriteInt32(this.ItemLot2);
                    bw.WriteInt32(this.UnkT18);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(this.ActionButtonParamID);
                    bw.WriteInt32(this.PickupAnimID);

                    bw.WriteBoolean(this.InChest);
                    bw.WriteBoolean(this.StartDisabled);
                    bw.WriteByte(0);
                    bw.WriteByte(0);

                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSB3 msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.TreasurePartName = MSB.FindName(entries.Parts, this.TreasurePartIndex);
                }

                internal override void GetIndices(MSB3 msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.TreasurePartIndex = MSB.FindIndex(entries.Parts, this.TreasurePartName);
                }
            }

            /// <summary>
            /// A continuous enemy spawner.
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
                /// Regions that enemies can be spawned at.
                /// </summary>
                public string[] SpawnPointNames { get; private set; }
                private int[] SpawnPointIndices;

                /// <summary>
                /// Enemies spawned by this generator.
                /// </summary>
                public string[] SpawnPartNames { get; private set; }
                private int[] SpawnPartIndices;

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
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    this.UnkT14 = br.ReadSingle();
                    this.UnkT18 = br.ReadSingle();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    this.SpawnPointIndices = br.ReadInt32s(8);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    this.SpawnPartIndices = br.ReadInt32s(32);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
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
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32s(this.SpawnPointIndices);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32s(this.SpawnPartIndices);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSB3 msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.SpawnPointNames = MSB.FindNames(entries.Regions, this.SpawnPointIndices);
                    this.SpawnPartNames = MSB.FindNames(entries.Parts, this.SpawnPartIndices);
                }

                internal override void GetIndices(MSB3 msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.SpawnPointIndices = MSB.FindIndices(entries.Regions, this.SpawnPointNames);
                    this.SpawnPartIndices = MSB.FindIndices(entries.Parts, this.SpawnPartNames);
                }
            }

            /// <summary>
            /// Controls usable objects like levers.
            /// </summary>
            public class ObjAct : Event {
                /// <summary>
                /// Unknown.
                /// </summary>
                public enum ObjActState : byte {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
                    OneState = 0,
                    DoorState = 1,
                    OneLoopState = 2,
                    OneLoopState2 = 3,
                    DoorState2 = 4,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
                }

                private protected override EventType Type => EventType.ObjAct;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int ObjActEntityID { get; set; }

                /// <summary>
                /// The object which is being interacted with.
                /// </summary>
                public string ObjActPartName { get; set; }
                private int ObjActPartIndex;

                /// <summary>
                /// ID in ObjActParam that configures this ObjAct.
                /// </summary>
                public int ObjActParamID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public ObjActState ObjActStateType { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int EventFlagID { get; set; }

                /// <summary>
                /// Creates an ObjAct with default values.
                /// </summary>
                public ObjAct() : base($"{nameof(Event)}: {nameof(ObjAct)}") {
                    this.ObjActEntityID = -1;
                    this.ObjActStateType = ObjActState.OneState;
                }

                internal ObjAct(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.ObjActEntityID = br.ReadInt32();
                    this.ObjActPartIndex = br.ReadInt32();
                    this.ObjActParamID = br.ReadInt32();

                    this.ObjActStateType = br.ReadEnum8<ObjActState>();
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);

                    this.EventFlagID = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.ObjActEntityID);
                    bw.WriteInt32(this.ObjActPartIndex);
                    bw.WriteInt32(this.ObjActParamID);

                    bw.WriteByte((byte)this.ObjActStateType);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);

                    bw.WriteInt32(this.EventFlagID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSB3 msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.ObjActPartName = MSB.FindName(entries.Parts, this.ObjActPartIndex);
                }

                internal override void GetIndices(MSB3 msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.ObjActPartIndex = MSB.FindIndex(entries.Parts, this.ObjActPartName);
                }
            }

            /// <summary>
            /// Moves all of the map pieces when cutscenes are played.
            /// </summary>
            public class MapOffset : Event {
                private protected override EventType Type => EventType.MapOffset;

                /// <summary>
                /// Position of the map offset.
                /// </summary>
                public Vector3 Position { get; set; }

                /// <summary>
                /// Rotation of the map offset.
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
            /// A fake multiplayer interaction where the player goes to an NPC's world.
            /// </summary>
            public class PseudoMultiplayer : Event {
                private protected override EventType Type => EventType.PseudoMultiplayer;

                /// <summary>
                /// The NPC whose world you're entering.
                /// </summary>
                public int HostEntityID { get; set; }

                /// <summary>
                /// Set when inside the event's region, unset when outside it.
                /// </summary>
                public int EventFlagID { get; set; }

                /// <summary>
                /// ID of a goods item that is used to trigger the event.
                /// </summary>
                public int ActivateGoodsID { get; set; }

                /// <summary>
                /// Unknown; possibly a sound ID.
                /// </summary>
                public int UnkT0C { get; set; }

                /// <summary>
                /// Unknown; possibly a map event ID.
                /// </summary>
                public int UnkT10 { get; set; }

                /// <summary>
                /// Unknown; possibly flags.
                /// </summary>
                public int UnkT14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT18 { get; set; }

                /// <summary>
                /// Creates a new PseudoMultiplayer with the given name.
                /// </summary>
                public PseudoMultiplayer() : base($"{nameof(Event)}: {nameof(PseudoMultiplayer)}") {
                    this.HostEntityID = -1;
                    this.EventFlagID = -1;
                    this.ActivateGoodsID = -1;
                    this.UnkT0C = -1;
                    this.UnkT10 = -1;
                }

                internal PseudoMultiplayer(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.HostEntityID = br.ReadInt32();
                    this.EventFlagID = br.ReadInt32();
                    this.ActivateGoodsID = br.ReadInt32();
                    this.UnkT0C = br.ReadInt32();
                    this.UnkT10 = br.ReadInt32();
                    this.UnkT14 = br.ReadInt32();
                    this.UnkT18 = br.ReadInt32();
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.HostEntityID);
                    bw.WriteInt32(this.EventFlagID);
                    bw.WriteInt32(this.ActivateGoodsID);
                    bw.WriteInt32(this.UnkT0C);
                    bw.WriteInt32(this.UnkT10);
                    bw.WriteInt32(this.UnkT14);
                    bw.WriteInt32(this.UnkT18);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// A simple list of points defining a path for enemies to take.
            /// </summary>
            public class PatrolInfo : Event {
                private protected override EventType Type => EventType.PatrolInfo;

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
                /// Creates a PatrolInfo with default values.
                /// </summary>
                public PatrolInfo() : base($"{nameof(Event)}: {nameof(PatrolInfo)}") => this.WalkPointNames = new string[32];

                private protected override void DeepCopyTo(Event evnt) {
                    var walkRoute = (PatrolInfo)evnt;
                    walkRoute.WalkPointNames = (string[])this.WalkPointNames.Clone();
                }

                internal PatrolInfo(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.AssertInt32(0, 1, 2, 5);
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

                internal override void GetNames(MSB3 msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.WalkPointNames = new string[this.WalkPointIndices.Length];
                    for (int i = 0; i < this.WalkPointIndices.Length; i++) {
                        this.WalkPointNames[i] = MSB.FindName(entries.Regions, this.WalkPointIndices[i]);
                    }
                }

                internal override void GetIndices(MSB3 msb, Entries entries) {
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
            public class PlatoonInfo : Event {
                private protected override EventType Type => EventType.PlatoonInfo;

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
                /// Creates a PlatoonInfo with default values.
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

                internal override void GetNames(MSB3 msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.GroupPartsNames = MSB.FindNames(entries.Parts, this.GroupPartsIndices);
                }

                internal override void GetIndices(MSB3 msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.GroupPartsIndices = MSB.FindIndices(entries.Parts, this.GroupPartsNames);
                }
            }

            /// <summary>
            /// Unknown. Only appears once in one unused MSB so it's hard to draw too many conclusions from it.
            /// </summary>
            public class Other : Event {
                private protected override EventType Type => EventType.Other;

                /// <summary>
                /// Unknown; possibly a sound type.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Unknown; possibly a sound ID.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// Creates an Other with default values.
                /// </summary>
                public Other() : base($"{nameof(Event)}: {nameof(Other)}") { }

                internal Other(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadInt32();
                    this.UnkT04 = br.ReadInt32();
                    br.AssertPattern(0x40, 0xFF);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.UnkT00);
                    bw.WriteInt32(this.UnkT04);
                    bw.WritePattern(0x40, 0xFF);
                }
            }
        }
    }
}
