using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using SoulsFormats.Formats.MSB;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MSB2 {
        internal enum RegionType : byte {
            Region0 = 0,
            Light = 3,
            StartPoint = 5,
            Sound = 7,
            SFX = 9,
            Wind = 13,
            EnvLight = 14,
            Fog = 15,
        }

        /// <summary>
        /// Points or volumes that trigger some behavior.
        /// </summary>
        public class PointParam : Param<Region>, IMsbParam<IMsbRegion> {
            internal override int Version => 5;
            internal override string Name => "POINT_PARAM_ST";

            /// <summary>
            /// Unknown, possibly walk points for enemies.
            /// </summary>
            public List<Region.Region0> Region0s { get; set; }

            /// <summary>
            /// Unknown if these do anything.
            /// </summary>
            public List<Region.Light> Lights { get; set; }

            /// <summary>
            /// Unknown, presumably the default position for spawning into the map.
            /// </summary>
            public List<Region.StartPoint> StartPoints { get; set; }

            /// <summary>
            /// Sound effects that play in certain areas.
            /// </summary>
            public List<Region.Sound> Sounds { get; set; }

            /// <summary>
            /// Special effects that play at certain areas.
            /// </summary>
            public List<Region.SFX> SFXs { get; set; }

            /// <summary>
            /// Unknown, presumably set wind speed/direction.
            /// </summary>
            public List<Region.Wind> Winds { get; set; }

            /// <summary>
            /// Unknown, names mention lightmaps and GI.
            /// </summary>
            public List<Region.EnvLight> EnvLights { get; set; }

            /// <summary>
            /// Unknown if these do anything.
            /// </summary>
            public List<Region.Fog> Fogs { get; set; }

            /// <summary>
            /// Creates an empty PointParam.
            /// </summary>
            public PointParam() {
                this.Region0s = new List<Region.Region0>();
                this.Lights = new List<Region.Light>();
                this.StartPoints = new List<Region.StartPoint>();
                this.Sounds = new List<Region.Sound>();
                this.SFXs = new List<Region.SFX>();
                this.Winds = new List<Region.Wind>();
                this.EnvLights = new List<Region.EnvLight>();
                this.Fogs = new List<Region.Fog>();
            }

            /// <summary>
            /// Adds a region to the appropriate list for its type; returns the region.
            /// </summary>
            public Region Add(Region region) {
                switch (region) {
                    case Region.Region0 r: this.Region0s.Add(r); break;
                    case Region.Light r: this.Lights.Add(r); break;
                    case Region.StartPoint r: this.StartPoints.Add(r); break;
                    case Region.Sound r: this.Sounds.Add(r); break;
                    case Region.SFX r: this.SFXs.Add(r); break;
                    case Region.Wind r: this.Winds.Add(r); break;
                    case Region.EnvLight r: this.EnvLights.Add(r); break;
                    case Region.Fog r: this.Fogs.Add(r); break;

                    default:
                        throw new ArgumentException($"Unrecognized type {region.GetType()}.", nameof(region));
                }
                return region;
            }
            IMsbRegion IMsbParam<IMsbRegion>.Add(IMsbRegion item) => this.Add((Region)item);

            /// <summary>
            /// Returns every Region in the order they'll be written.
            /// </summary>
            public override List<Region> GetEntries() => SFUtil.ConcatAll<Region>(
                    this.Region0s, this.Lights, this.StartPoints, this.Sounds, this.SFXs,
                    this.Winds, this.EnvLights, this.Fogs);
            IReadOnlyList<IMsbRegion> IMsbParam<IMsbRegion>.GetEntries() => this.GetEntries();

            internal override Region ReadEntry(BinaryReaderEx br) {
                RegionType type = br.GetEnum8<RegionType>(br.Position + br.VarintSize + 2);
                return type switch {
                    RegionType.Region0 => this.Region0s.EchoAdd(new Region.Region0(br)),
                    RegionType.Light => this.Lights.EchoAdd(new Region.Light(br)),
                    RegionType.StartPoint => this.StartPoints.EchoAdd(new Region.StartPoint(br)),
                    RegionType.Sound => this.Sounds.EchoAdd(new Region.Sound(br)),
                    RegionType.SFX => this.SFXs.EchoAdd(new Region.SFX(br)),
                    RegionType.Wind => this.Winds.EchoAdd(new Region.Wind(br)),
                    RegionType.EnvLight => this.EnvLights.EchoAdd(new Region.EnvLight(br)),
                    RegionType.Fog => this.Fogs.EchoAdd(new Region.Fog(br)),
                    _ => throw new NotImplementedException($"Unimplemented region type: {type}"),
                };
            }
        }

        /// <summary>
        /// A point or volume that triggers some behavior.
        /// </summary>
        public abstract class Region : NamedEntry, IMsbRegion {
            private protected abstract RegionType Type { get; }
            private protected abstract bool HasTypeData { get; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public short Unk08 { get; set; }

            /// <summary>
            /// Describes the space encompassed by the region.
            /// </summary>
            public MSB.Shape Shape {
                get => this._shape;
                set {
                    if (value is MSB.Shape.Composite) {
                        throw new ArgumentException("Dark Souls 2 does not support composite shapes.");
                    }

                    this._shape = value;
                }
            }
            private MSB.Shape _shape;

            /// <summary>
            /// Unknown.
            /// </summary>
            public short Unk0E { get; set; }

            /// <summary>
            /// Location of the region.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Rotation of the region, in degrees.
            /// </summary>
            public Vector3 Rotation { get; set; }

            private protected Region(string name) {
                this.Name = name;
                this.Shape = new MSB.Shape.Point();
            }

            /// <summary>
            /// Creates a deep copy of the region.
            /// </summary>
            public Region DeepCopy() {
                var region = (Region)this.MemberwiseClone();
                region.Shape = this.Shape.DeepCopy();
                return region;
            }
            IMsbRegion IMsbRegion.DeepCopy() => this.DeepCopy();

            private protected Region(BinaryReaderEx br) {
                long start = br.Position;
                long nameOffset = br.ReadVarint();
                this.Unk08 = br.ReadInt16();
                _ = br.AssertByte((byte)this.Type);
                var shapeType = (MSB.ShapeType)br.ReadByte();
                _ = br.ReadInt16(); // ID
                this.Unk0E = br.ReadInt16();
                this.Position = br.ReadVector3();
                this.Rotation = br.ReadVector3();
                long unkOffsetA = br.ReadVarint();
                long unkOffsetB = br.ReadVarint();
                _ = br.AssertInt32(-1);
                br.AssertPattern(0x24, 0x00);
                long shapeDataOffset = br.ReadVarint();
                long typeDataOffset = br.ReadVarint();
                _ = br.AssertInt64(0);
                _ = br.AssertInt64(0);
                if (!br.VarintLong) {
                    _ = br.AssertInt64(0);
                    _ = br.AssertInt64(0);
                    _ = br.AssertInt32(0);
                }

                this.Shape = MSB.Shape.Create(shapeType);

                if (nameOffset == 0) {
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {this.GetType()}.");
                }

                if (unkOffsetA == 0) {
                    throw new InvalidDataException($"{nameof(unkOffsetA)} must not be 0 in type {this.GetType()}.");
                }

                if (unkOffsetB == 0) {
                    throw new InvalidDataException($"{nameof(unkOffsetB)} must not be 0 in type {this.GetType()}.");
                }

                if (this.Shape.HasShapeData ^ shapeDataOffset != 0) {
                    throw new InvalidDataException($"Unexpected {nameof(shapeDataOffset)} 0x{shapeDataOffset:X} in type {this.GetType()}.");
                }

                if (this.HasTypeData ^ typeDataOffset != 0) {
                    throw new InvalidDataException($"Unexpected {nameof(typeDataOffset)} 0x{typeDataOffset:X} in type {this.GetType()}.");
                }

                br.Position = start + nameOffset;
                this.Name = br.ReadUTF16();

                br.Position = start + unkOffsetA;
                _ = br.AssertInt32(0);

                br.Position = start + unkOffsetB;
                _ = br.AssertInt32(0);

                if (this.Shape.HasShapeData) {
                    br.Position = start + shapeDataOffset;
                    this.Shape.ReadShapeData(br);
                }

                if (this.HasTypeData) {
                    br.Position = start + typeDataOffset;
                    this.ReadTypeData(br);
                }
            }

            private protected virtual void ReadTypeData(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(ReadTypeData)}.");

            internal override void Write(BinaryWriterEx bw, int id) {
                long start = bw.Position;
                bw.ReserveVarint("NameOffset");
                bw.WriteInt16(this.Unk08);
                bw.WriteByte((byte)this.Type);
                bw.WriteByte((byte)this.Shape.Type);
                bw.WriteInt16((short)id);
                bw.WriteInt16(this.Unk0E);
                bw.WriteVector3(this.Position);
                bw.WriteVector3(this.Rotation);
                bw.ReserveVarint("UnkOffsetA");
                bw.ReserveVarint("UnkOffsetB");
                bw.WriteInt32(-1);
                bw.WritePattern(0x24, 0x00);
                bw.ReserveVarint("ShapeDataOffset");
                bw.ReserveVarint("TypeDataOffset");
                bw.WriteInt64(0);
                bw.WriteInt64(0);
                if (!bw.VarintLong) {
                    bw.WriteInt64(0);
                    bw.WriteInt64(0);
                    bw.WriteInt32(0);
                }

                bw.FillVarint("NameOffset", bw.Position - start);
                bw.WriteUTF16(this.Name, true);
                bw.Pad(4);

                bw.FillVarint("UnkOffsetA", bw.Position - start);
                bw.WriteInt32(0);

                bw.FillVarint("UnkOffsetB", bw.Position - start);
                bw.WriteInt32(0);
                bw.Pad(bw.VarintSize);

                if (this.Shape.HasShapeData) {
                    bw.FillVarint("ShapeDataOffset", bw.Position - start);
                    this.Shape.WriteShapeData(bw);
                } else {
                    bw.FillVarint("ShapeDataOffset", 0);
                }

                if (this.HasTypeData) {
                    bw.FillVarint("TypeDataOffset", bw.Position - start);
                    this.WriteTypeData(bw);
                } else {
                    bw.FillVarint("TypeDataOffset", 0);
                }
            }

            private protected virtual void WriteTypeData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(WriteTypeData)}.");

            /// <summary>
            /// Returns a string representation of the region.
            /// </summary>
            public override string ToString() => $"{this.Type} {this.Shape.Type} \"{this.Name}\"";

            /// <summary>
            /// Unknown, names always seem to mention enemies; possibly walk points.
            /// </summary>
            public class Region0 : Region {
                private protected override RegionType Type => RegionType.Region0;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates a Region0 with default values.
                /// </summary>
                public Region0() : base($"{nameof(Region)}: {nameof(Region0)}") { }

                internal Region0(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unknown if this does anything.
            /// </summary>
            public class Light : Region {
                private protected override RegionType Type => RegionType.Light;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public Color ColorT04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public Color ColorT08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT0C { get; set; }

                /// <summary>
                /// Creates a Light with default values.
                /// </summary>
                public Light() : base($"{nameof(Region)}: {nameof(Light)}") { }

                internal Light(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadInt32();
                    this.ColorT04 = br.ReadRGBA();
                    this.ColorT08 = br.ReadRGBA();
                    this.UnkT0C = br.ReadSingle();
                    br.AssertPattern(0x10, 0x00);
                    if (br.VarintLong) {
                        _ = br.AssertInt32(0);
                    }
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.UnkT00);
                    bw.WriteRGBA(this.ColorT04);
                    bw.WriteRGBA(this.ColorT08);
                    bw.WriteSingle(this.UnkT0C);
                    bw.WritePattern(0x10, 0x00);
                    if (bw.VarintLong) {
                        bw.WriteInt32(0);
                    }
                }
            }

            /// <summary>
            /// Unknown, presumably the default spawn location for a map.
            /// </summary>
            public class StartPoint : Region {
                private protected override RegionType Type => RegionType.StartPoint;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates a StartPoint with default values.
                /// </summary>
                public StartPoint() : base($"{nameof(Region)}: {nameof(StartPoint)}") { }

                internal StartPoint(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A sound effect that plays in a certain area.
            /// </summary>
            public class Sound : Region {
                private protected override RegionType Type => RegionType.Sound;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown; possibly sound type.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// ID of the sound to play.
                /// </summary>
                public int SoundID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT08 { get; set; }

                /// <summary>
                /// Creates a Sound with default values.
                /// </summary>
                public Sound() : base($"{nameof(Region)}: {nameof(Sound)}") { }

                internal Sound(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadInt32();
                    this.SoundID = br.ReadInt32();
                    this.UnkT08 = br.ReadInt32();
                    br.AssertPattern(0x14, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.UnkT00);
                    bw.WriteInt32(this.SoundID);
                    bw.WriteInt32(this.UnkT08);
                    bw.WritePattern(0x14, 0x00);
                }
            }

            /// <summary>
            /// A special effect that plays at a certain region.
            /// </summary>
            public class SFX : Region {
                private protected override RegionType Type => RegionType.SFX;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// The effect to play at this region.
                /// </summary>
                public int EffectID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// Creates an SFX with default values.
                /// </summary>
                public SFX() : base($"{nameof(Region)}: {nameof(SFX)}") { }

                internal SFX(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.EffectID = br.ReadInt32();
                    this.UnkT04 = br.ReadInt32();
                    br.AssertPattern(0x18, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.EffectID);
                    bw.WriteInt32(this.UnkT04);
                    bw.WritePattern(0x18, 0x00);
                }
            }

            /// <summary>
            /// Unknown, presumably sets wind speed/direction.
            /// </summary>
            public class Wind : Region {
                private protected override RegionType Type => RegionType.Wind;
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
                /// Unknown.
                /// </summary>
                public float UnkT18 { get; set; }

                /// <summary>
                /// Creates a Wind with default values.
                /// </summary>
                public Wind() : base($"{nameof(Region)}: {nameof(Wind)}") { }

                internal Wind(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadInt32();
                    this.UnkT04 = br.ReadSingle();
                    this.UnkT08 = br.ReadSingle();
                    this.UnkT0C = br.ReadSingle();
                    this.UnkT10 = br.ReadSingle();
                    this.UnkT14 = br.ReadSingle();
                    this.UnkT18 = br.ReadSingle();
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.UnkT00);
                    bw.WriteSingle(this.UnkT04);
                    bw.WriteSingle(this.UnkT08);
                    bw.WriteSingle(this.UnkT0C);
                    bw.WriteSingle(this.UnkT10);
                    bw.WriteSingle(this.UnkT14);
                    bw.WriteSingle(this.UnkT18);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown, names mention lightmaps and GI.
            /// </summary>
            public class EnvLight : Region {
                private protected override RegionType Type => RegionType.EnvLight;
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
                /// Creates an EnvLight with default values.
                /// </summary>
                public EnvLight() : base($"{nameof(Region)}: {nameof(EnvLight)}") { }

                internal EnvLight(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadInt32();
                    this.UnkT04 = br.ReadSingle();
                    this.UnkT08 = br.ReadSingle();
                    br.AssertPattern(0x14, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.UnkT00);
                    bw.WriteSingle(this.UnkT04);
                    bw.WriteSingle(this.UnkT08);
                    bw.WritePattern(0x14, 0x00);
                }
            }

            /// <summary>
            /// Unknown if this does anything.
            /// </summary>
            public class Fog : Region {
                private protected override RegionType Type => RegionType.Fog;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// Creates a Fog with default values.
                /// </summary>
                public Fog() : base($"{nameof(Region)}: {nameof(Fog)}") { }

                internal Fog(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadInt32();
                    this.UnkT04 = br.ReadInt32();
                    br.AssertPattern(0x18, 0x00);
                    if (br.VarintLong) {
                        _ = br.AssertInt32(0);
                    }
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.UnkT00);
                    bw.WriteInt32(this.UnkT04);
                    bw.WritePattern(0x18, 0x00);
                    if (bw.VarintLong) {
                        bw.WriteInt32(0);
                    }
                }
            }
        }
    }
}
