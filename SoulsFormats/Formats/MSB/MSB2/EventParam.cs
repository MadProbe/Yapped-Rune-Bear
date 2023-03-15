using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using SoulsFormats.Formats.MSB;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MSB2 {
        internal enum EventType : byte {
            Light = 1,
            Shadow = 2,
            Fog = 3,
            BGColor = 4,
            MapOffset = 5,
            Warp = 6,
            CheapMode = 7,
        }

        /// <summary>
        /// Abstract entities that control map properties or behaviors.
        /// </summary>
        public class EventParam : Param<Event>, IMsbParam<IMsbEvent> {
            internal override int Version => 5;
            internal override string Name => "EVENT_PARAM_ST";

            /// <summary>
            /// Unknown if these do anything.
            /// </summary>
            public List<Event.Light> Lights { get; set; }

            /// <summary>
            /// Unknown if these do anything.
            /// </summary>
            public List<Event.Shadow> Shadows { get; set; }

            /// <summary>
            /// Unknown if these do anything.
            /// </summary>
            public List<Event.Fog> Fogs { get; set; }

            /// <summary>
            /// Sets the background color when no models are in the way. Should only be one per map.
            /// </summary>
            public List<Event.BGColor> BGColors { get; set; }

            /// <summary>
            /// Sets the origin of the map; already factored into MSB positions, but affects BTL. Should only be one per map.
            /// </summary>
            public List<Event.MapOffset> MapOffsets { get; set; }

            /// <summary>
            /// Unknown exactly what this is for.
            /// </summary>
            public List<Event.Warp> Warps { get; set; }

            /// <summary>
            /// Unknown if these do anything.
            /// </summary>
            public List<Event.CheapMode> CheapModes { get; set; }

            /// <summary>
            /// Creates an empty EventParam.
            /// </summary>
            public EventParam() {
                this.Lights = new List<Event.Light>();
                this.Shadows = new List<Event.Shadow>();
                this.Fogs = new List<Event.Fog>();
                this.BGColors = new List<Event.BGColor>();
                this.MapOffsets = new List<Event.MapOffset>();
                this.Warps = new List<Event.Warp>();
                this.CheapModes = new List<Event.CheapMode>();
            }

            /// <summary>
            /// Adds an event to the appropriate list for its type; returns the event.
            /// </summary>
            public Event Add(Event evnt) {
                switch (evnt) {
                    case Event.Light e: this.Lights.Add(e); break;
                    case Event.Shadow e: this.Shadows.Add(e); break;
                    case Event.Fog e: this.Fogs.Add(e); break;
                    case Event.BGColor e: this.BGColors.Add(e); break;
                    case Event.MapOffset e: this.MapOffsets.Add(e); break;
                    case Event.Warp e: this.Warps.Add(e); break;
                    case Event.CheapMode e: this.CheapModes.Add(e); break;

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
                    this.Lights, this.Shadows, this.Fogs, this.BGColors, this.MapOffsets,
                    this.Warps, this.CheapModes);
            IReadOnlyList<IMsbEvent> IMsbParam<IMsbEvent>.GetEntries() => this.GetEntries();

            internal override Event ReadEntry(BinaryReaderEx br) {
                EventType type = br.GetEnum8<EventType>(br.Position + br.VarintSize + 4);
                return type switch {
                    EventType.Light => this.Lights.EchoAdd(new Event.Light(br)),
                    EventType.Shadow => this.Shadows.EchoAdd(new Event.Shadow(br)),
                    EventType.Fog => this.Fogs.EchoAdd(new Event.Fog(br)),
                    EventType.BGColor => this.BGColors.EchoAdd(new Event.BGColor(br)),
                    EventType.MapOffset => this.MapOffsets.EchoAdd(new Event.MapOffset(br)),
                    EventType.Warp => this.Warps.EchoAdd(new Event.Warp(br)),
                    EventType.CheapMode => this.CheapModes.EchoAdd(new Event.CheapMode(br)),
                    _ => throw new NotImplementedException($"Unimplemented event type: {type}"),
                };
            }
        }

        /// <summary>
        /// An abstract entity that controls map properties or behaviors.
        /// </summary>
        public abstract class Event : NamedEntry, IMsbEvent {
            private protected abstract EventType Type { get; }

            /// <summary>
            /// Uniquely identifies the event in the map.
            /// </summary>
            public int EventID { get; set; }

            private protected Event(string name) {
                this.Name = name;
                this.EventID = -1;
            }

            /// <summary>
            /// Creates a deep copy of the event.
            /// </summary>
            public Event DeepCopy() => (Event)this.MemberwiseClone();
            IMsbEvent IMsbEvent.DeepCopy() => this.DeepCopy();

            private protected Event(BinaryReaderEx br) {
                long start = br.Position;
                long nameOffset = br.ReadVarint();
                this.EventID = br.ReadInt32();
                _ = br.AssertByte((byte)this.Type);
                _ = br.AssertByte(0);
                _ = br.ReadInt16(); // ID
                long typeDataOffset = br.ReadVarint();
                if (!br.VarintLong) {
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                if (nameOffset == 0) {
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {this.GetType()}.");
                }

                if (typeDataOffset == 0) {
                    throw new InvalidDataException($"{nameof(typeDataOffset)} must not be 0 in type {this.GetType()}.");
                }

                br.Position = start + nameOffset;
                this.Name = br.ReadUTF16();

                br.Position = start + typeDataOffset;
                this.ReadTypeData(br);
            }

            private protected abstract void ReadTypeData(BinaryReaderEx br);

            internal override void Write(BinaryWriterEx bw, int id) {
                long start = bw.Position;
                bw.ReserveVarint("NameOffset");
                bw.WriteInt32(this.EventID);
                bw.WriteByte((byte)this.Type);
                bw.WriteByte(0);
                bw.WriteInt16((short)id);
                bw.ReserveVarint("TypeDataOffset");
                if (!bw.VarintLong) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                bw.FillVarint("NameOffset", bw.Position - start);
                bw.WriteUTF16(this.Name, true);
                bw.Pad(bw.VarintSize);

                bw.FillVarint("TypeDataOffset", bw.Position - start);
                this.WriteTypeData(bw);
            }

            private protected abstract void WriteTypeData(BinaryWriterEx bw);

            /// <summary>
            /// Returns a string representation of the event.
            /// </summary>
            public override string ToString() => $"[ID {this.EventID}] {this.Type} \"{this.Name}\"";

            /// <summary>
            /// Unknown if this does anything.
            /// </summary>
            public class Light : Event {
                private protected override EventType Type => EventType.Light;

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT00 { get; set; }

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
                public Color ColorT0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public Color ColorT10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT1C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT20 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public Color ColorT24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public Color ColorT28 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public Color ColorT34 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public Color ColorT38 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public Color ColorT3C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT40 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT44 { get; set; }

                /// <summary>
                /// Creates a Light with default values.
                /// </summary>
                public Light() : base($"{nameof(Event)}: {nameof(Light)}") { }

                internal Light(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadByte();
                    _ = br.AssertByte(0);
                    _ = br.AssertInt16(-1);
                    this.UnkT04 = br.ReadSingle();
                    this.UnkT08 = br.ReadSingle();
                    this.ColorT0C = br.ReadRGBA();
                    this.ColorT10 = br.ReadRGBA();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    this.UnkT1C = br.ReadSingle();
                    this.UnkT20 = br.ReadSingle();
                    this.ColorT24 = br.ReadRGBA();
                    this.ColorT28 = br.ReadRGBA();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    this.ColorT34 = br.ReadRGBA();
                    this.ColorT38 = br.ReadRGBA();
                    this.ColorT3C = br.ReadRGBA();
                    this.UnkT40 = br.ReadSingle();
                    this.UnkT44 = br.ReadByte();
                    br.AssertPattern(0x3B, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteByte(this.UnkT00);
                    bw.WriteByte(0);
                    bw.WriteInt16(-1);
                    bw.WriteSingle(this.UnkT04);
                    bw.WriteSingle(this.UnkT08);
                    bw.WriteRGBA(this.ColorT0C);
                    bw.WriteRGBA(this.ColorT10);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteSingle(this.UnkT1C);
                    bw.WriteSingle(this.UnkT20);
                    bw.WriteRGBA(this.ColorT24);
                    bw.WriteRGBA(this.ColorT28);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteRGBA(this.ColorT34);
                    bw.WriteRGBA(this.ColorT38);
                    bw.WriteRGBA(this.ColorT3C);
                    bw.WriteSingle(this.UnkT40);
                    bw.WriteByte(this.UnkT44);
                    bw.WritePattern(0x3B, 0x00);
                }
            }

            /// <summary>
            /// Unknown if this does anything.
            /// </summary>
            public class Shadow : Event {
                private protected override EventType Type => EventType.Shadow;

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
                public float UnkT14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT20 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public Color ColorT24 { get; set; }

                /// <summary>
                /// Creates a Shadow with default values.
                /// </summary>
                public Shadow() : base($"{nameof(Event)}: {nameof(Shadow)}") { }

                internal Shadow(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    _ = br.AssertInt32(0);
                    this.UnkT04 = br.ReadSingle();
                    this.UnkT08 = br.ReadSingle();
                    this.UnkT0C = br.ReadSingle();
                    _ = br.AssertInt32(0);
                    this.UnkT14 = br.ReadSingle();
                    this.UnkT18 = br.ReadSingle();
                    _ = br.AssertInt32(0);
                    this.UnkT20 = br.ReadSingle();
                    this.ColorT24 = br.ReadRGBA();
                    br.AssertPattern(0x18, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteSingle(this.UnkT04);
                    bw.WriteSingle(this.UnkT08);
                    bw.WriteSingle(this.UnkT0C);
                    bw.WriteInt32(0);
                    bw.WriteSingle(this.UnkT14);
                    bw.WriteSingle(this.UnkT18);
                    bw.WriteInt32(0);
                    bw.WriteSingle(this.UnkT20);
                    bw.WriteRGBA(this.ColorT24);
                    bw.WritePattern(0x18, 0x00);
                }
            }

            /// <summary>
            /// Unknown if this does anything.
            /// </summary>
            public class Fog : Event {
                private protected override EventType Type => EventType.Fog;

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public Color ColorT04 { get; set; }

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
                public byte UnkT14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT15 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT16 { get; set; }

                /// <summary>
                /// Creates a Fog with default values.
                /// </summary>
                public Fog() : base($"{nameof(Event)}: {nameof(Fog)}") { }

                internal Fog(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadByte();
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    this.ColorT04 = br.ReadRGBA();
                    this.UnkT08 = br.ReadSingle();
                    this.UnkT0C = br.ReadSingle();
                    this.UnkT10 = br.ReadSingle();
                    this.UnkT14 = br.ReadByte();
                    this.UnkT15 = br.ReadByte();
                    this.UnkT16 = br.ReadByte();
                    br.AssertPattern(0x11, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteByte(this.UnkT00);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteRGBA(this.ColorT04);
                    bw.WriteSingle(this.UnkT08);
                    bw.WriteSingle(this.UnkT0C);
                    bw.WriteSingle(this.UnkT10);
                    bw.WriteByte(this.UnkT14);
                    bw.WriteByte(this.UnkT15);
                    bw.WriteByte(this.UnkT16);
                    bw.WritePattern(0x11, 0x00);
                }
            }

            /// <summary>
            /// Sets the background color of the map when no models are in the way.
            /// </summary>
            public class BGColor : Event {
                private protected override EventType Type => EventType.BGColor;

                /// <summary>
                /// The background color.
                /// </summary>
                public Color Color { get; set; }

                /// <summary>
                /// Creates a BGColor with default values.
                /// </summary>
                public BGColor() : base($"{nameof(Event)}: {nameof(BGColor)}") { }

                internal BGColor(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.Color = br.ReadRGBA();
                    br.AssertPattern(0x24, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteRGBA(this.Color);
                    bw.WritePattern(0x24, 0x00);
                }
            }

            /// <summary>
            /// Sets the origin of the map; already factored into MSB positions but affects BTL.
            /// </summary>
            public class MapOffset : Event {
                private protected override EventType Type => EventType.MapOffset;

                /// <summary>
                /// The origin of the map.
                /// </summary>
                public Vector3 Translation { get; set; }

                /// <summary>
                /// Creates a MapOffset with default values.
                /// </summary>
                public MapOffset() : base($"{nameof(Event)}: {nameof(MapOffset)}") { }

                internal MapOffset(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.Translation = br.ReadVector3();
                    _ = br.AssertInt32(0); // Degree
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteVector3(this.Translation);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown exactly what this is for.
            /// </summary>
            public class Warp : Event {
                private protected override EventType Type => EventType.Warp;

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT00 { get; set; }

                /// <summary>
                /// Presumably the position to be warped to.
                /// </summary>
                public Vector3 Position { get; set; }

                /// <summary>
                /// Creates a Warp with default values.
                /// </summary>
                public Warp() : base($"{nameof(Event)}: {nameof(Warp)}") { }

                internal Warp(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadByte();
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    this.Position = br.ReadVector3();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteByte(this.UnkT00);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteVector3(this.Position);
                }
            }

            /// <summary>
            /// Unknown if this does anything.
            /// </summary>
            public class CheapMode : Event {
                private protected override EventType Type => EventType.CheapMode;

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT00 { get; set; }

                /// <summary>
                /// Creates a CheapMode with default values.
                /// </summary>
                public CheapMode() : base($"{nameof(Event)}: {nameof(CheapMode)}") { }

                internal CheapMode(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadInt16();
                    br.AssertPattern(0xE, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt16(this.UnkT00);
                    bw.WritePattern(0xE, 0x00);
                }
            }
        }
    }
}
