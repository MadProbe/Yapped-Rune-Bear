using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using SoulsFormats.Formats.MSB;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MSB3 {
        internal enum RegionType : uint {
            InvasionPoint = 1,
            EnvironmentMapPoint = 2,
            //Region3 = 3,
            Sound = 4,
            SFX = 5,
            WindSFX = 6,
            //Region7 = 7,
            SpawnPoint = 8,
            Message = 9,
            //PseudoMultiplayer = 10,
            PatrolRoute = 11,
            MovementPoint = 12,
            WarpPoint = 13,
            ActivationArea = 14,
            Event = 15,
            Logic = 0, // There are no regions of type 16 and type 0 is written in this order, so I suspect this is correct
            EnvironmentMapEffectBox = 17,
            WindArea = 18,
            //Region19 = 19,
            MufflingBox = 20,
            MufflingPortal = 21,
            Other = 0xFFFFFFFF,
        }

        /// <summary>
        /// A section containing points and volumes for various purposes.
        /// </summary>
        public class PointParam : Param<Region>, IMsbParam<IMsbRegion> {
            internal override int Version => 3;
            internal override string Type => "POINT_PARAM_ST";

            /// <summary>
            /// InvasionPoints in the MSB.
            /// </summary>
            public List<Region.InvasionPoint> InvasionPoints { get; set; }

            /// <summary>
            /// EnvironmentMapPoints in the MSB.
            /// </summary>
            public List<Region.EnvironmentMapPoint> EnvironmentMapPoints { get; set; }

            /// <summary>
            /// Sound regions in the MSB.
            /// </summary>
            public List<Region.Sound> Sounds { get; set; }

            /// <summary>
            /// SFX regions in the MSB.
            /// </summary>
            public List<Region.SFX> SFX { get; set; }

            /// <summary>
            /// WindSFX regions in the MSB.
            /// </summary>
            public List<Region.WindSFX> WindSFX { get; set; }

            /// <summary>
            /// SpawnPoints in the MSB.
            /// </summary>
            public List<Region.SpawnPoint> SpawnPoints { get; set; }

            /// <summary>
            /// Messages in the MSB.
            /// </summary>
            public List<Region.Message> Messages { get; set; }

            /// <summary>
            /// PatrolRoute points in the MSB.
            /// </summary>
            public List<Region.PatrolRoute> PatrolRoutes { get; set; }

            /// <summary>
            /// MovementPoints in the MSB.
            /// </summary>
            public List<Region.MovementPoint> MovementPoints { get; set; }

            /// <summary>
            /// WarpPoints in the MSB.
            /// </summary>
            public List<Region.WarpPoint> WarpPoints { get; set; }

            /// <summary>
            /// ActivationAreas in the MSB.
            /// </summary>
            public List<Region.ActivationArea> ActivationAreas { get; set; }

            /// <summary>
            /// Event regions in the MSB.
            /// </summary>
            public List<Region.Event> Events { get; set; }

            /// <summary>
            /// Logic regions in the MSB.
            /// </summary>
            public List<Region.Logic> Logic { get; set; }

            /// <summary>
            /// EnvironmentMapEffectBoxes in the MSB.
            /// </summary>
            public List<Region.EnvironmentMapEffectBox> EnvironmentMapEffectBoxes { get; set; }

            /// <summary>
            /// WindAreas in the MSB.
            /// </summary>
            public List<Region.WindArea> WindAreas { get; set; }

            /// <summary>
            /// MufflingBoxes in the MSB.
            /// </summary>
            public List<Region.MufflingBox> MufflingBoxes { get; set; }

            /// <summary>
            /// MufflingPortals in the MSB.
            /// </summary>
            public List<Region.MufflingPortal> MufflingPortals { get; set; }

            /// <summary>
            /// Most likely a dumping ground for unused regions.
            /// </summary>
            public List<Region.Other> Others { get; set; }

            /// <summary>
            /// Creates a new PointParam with no regions.
            /// </summary>
            public PointParam() {
                this.InvasionPoints = new List<Region.InvasionPoint>();
                this.EnvironmentMapPoints = new List<Region.EnvironmentMapPoint>();
                this.Sounds = new List<Region.Sound>();
                this.SFX = new List<Region.SFX>();
                this.WindSFX = new List<Region.WindSFX>();
                this.SpawnPoints = new List<Region.SpawnPoint>();
                this.Messages = new List<Region.Message>();
                this.PatrolRoutes = new List<Region.PatrolRoute>();
                this.MovementPoints = new List<Region.MovementPoint>();
                this.WarpPoints = new List<Region.WarpPoint>();
                this.ActivationAreas = new List<Region.ActivationArea>();
                this.Events = new List<Region.Event>();
                this.Logic = new List<Region.Logic>();
                this.EnvironmentMapEffectBoxes = new List<Region.EnvironmentMapEffectBox>();
                this.WindAreas = new List<Region.WindArea>();
                this.MufflingBoxes = new List<Region.MufflingBox>();
                this.MufflingPortals = new List<Region.MufflingPortal>();
                this.Others = new List<Region.Other>();
            }

            /// <summary>
            /// Adds a region to the appropriate list for its type; returns the region.
            /// </summary>
            public Region Add(Region region) {
                switch (region) {
                    case Region.InvasionPoint r: this.InvasionPoints.Add(r); break;
                    case Region.EnvironmentMapPoint r: this.EnvironmentMapPoints.Add(r); break;
                    case Region.Sound r: this.Sounds.Add(r); break;
                    case Region.SFX r: this.SFX.Add(r); break;
                    case Region.WindSFX r: this.WindSFX.Add(r); break;
                    case Region.SpawnPoint r: this.SpawnPoints.Add(r); break;
                    case Region.Message r: this.Messages.Add(r); break;
                    case Region.PatrolRoute r: this.PatrolRoutes.Add(r); break;
                    case Region.MovementPoint r: this.MovementPoints.Add(r); break;
                    case Region.WarpPoint r: this.WarpPoints.Add(r); break;
                    case Region.ActivationArea r: this.ActivationAreas.Add(r); break;
                    case Region.Event r: this.Events.Add(r); break;
                    case Region.Logic r: this.Logic.Add(r); break;
                    case Region.EnvironmentMapEffectBox r: this.EnvironmentMapEffectBoxes.Add(r); break;
                    case Region.WindArea r: this.WindAreas.Add(r); break;
                    case Region.MufflingBox r: this.MufflingBoxes.Add(r); break;
                    case Region.MufflingPortal r: this.MufflingPortals.Add(r); break;
                    case Region.Other r: this.Others.Add(r); break;

                    default:
                        throw new ArgumentException($"Unrecognized type {region.GetType()}.", nameof(region));
                }
                return region;
            }
            IMsbRegion IMsbParam<IMsbRegion>.Add(IMsbRegion item) => this.Add((Region)item);

            /// <summary>
            /// Returns every region in the order they will be written.
            /// </summary>
            public override List<Region> GetEntries() => SFUtil.ConcatAll<Region>(
                    this.InvasionPoints, this.EnvironmentMapPoints, this.Sounds, this.SFX, this.WindSFX,
                    this.SpawnPoints, this.Messages, this.PatrolRoutes, this.MovementPoints, this.WarpPoints,
                    this.ActivationAreas, this.Events, this.Logic, this.EnvironmentMapEffectBoxes, this.WindAreas,
                    this.MufflingBoxes, this.MufflingPortals, this.Others);
            IReadOnlyList<IMsbRegion> IMsbParam<IMsbRegion>.GetEntries() => this.GetEntries();

            internal override Region ReadEntry(BinaryReaderEx br) {
                RegionType type = br.GetEnum32<RegionType>(br.Position + 8);
                return type switch {
                    RegionType.InvasionPoint => this.InvasionPoints.EchoAdd(new Region.InvasionPoint(br)),
                    RegionType.EnvironmentMapPoint => this.EnvironmentMapPoints.EchoAdd(new Region.EnvironmentMapPoint(br)),
                    RegionType.Sound => this.Sounds.EchoAdd(new Region.Sound(br)),
                    RegionType.SFX => this.SFX.EchoAdd(new Region.SFX(br)),
                    RegionType.WindSFX => this.WindSFX.EchoAdd(new Region.WindSFX(br)),
                    RegionType.SpawnPoint => this.SpawnPoints.EchoAdd(new Region.SpawnPoint(br)),
                    RegionType.Message => this.Messages.EchoAdd(new Region.Message(br)),
                    RegionType.PatrolRoute => this.PatrolRoutes.EchoAdd(new Region.PatrolRoute(br)),
                    RegionType.MovementPoint => this.MovementPoints.EchoAdd(new Region.MovementPoint(br)),
                    RegionType.WarpPoint => this.WarpPoints.EchoAdd(new Region.WarpPoint(br)),
                    RegionType.ActivationArea => this.ActivationAreas.EchoAdd(new Region.ActivationArea(br)),
                    RegionType.Event => this.Events.EchoAdd(new Region.Event(br)),
                    RegionType.Logic => this.Logic.EchoAdd(new Region.Logic(br)),
                    RegionType.EnvironmentMapEffectBox => this.EnvironmentMapEffectBoxes.EchoAdd(new Region.EnvironmentMapEffectBox(br)),
                    RegionType.WindArea => this.WindAreas.EchoAdd(new Region.WindArea(br)),
                    RegionType.MufflingBox => this.MufflingBoxes.EchoAdd(new Region.MufflingBox(br)),
                    RegionType.MufflingPortal => this.MufflingPortals.EchoAdd(new Region.MufflingPortal(br)),
                    RegionType.Other => this.Others.EchoAdd(new Region.Other(br)),
                    _ => throw new NotImplementedException($"Unsupported region type: {type}"),
                };
            }
        }

        /// <summary>
        /// A point or volumetric area used for a variety of purposes.
        /// </summary>
        public abstract class Region : NamedEntry, IMsbRegion {
            private protected enum TypeDataPresence {
                Never,
                Sometimes,
                Always,
                AlwaysNull,
            }

            private protected abstract RegionType Type { get; }
            private protected abstract TypeDataPresence ShouldHaveTypeData { get; }
            private protected abstract bool DoesHaveTypeData { get; }

            /// <summary>
            /// The name of this region.
            /// </summary>
            public override string Name { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk2C { get; set; }

            /// <summary>
            /// The shape of this region.
            /// </summary>
            public MSB.Shape Shape {
                get => this._shape;
                set {
                    if (value is MSB.Shape.Composite) {
                        throw new ArgumentException("Dark Souls 3 does not support composite shapes.");
                    }

                    this._shape = value;
                }
            }
            private MSB.Shape _shape;

            /// <summary>
            /// Controls whether the event is present in different ceremonies. Maybe only used for Messages?
            /// </summary>
            public uint MapStudioLayer { get; set; }

            /// <summary>
            /// Center of the region.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Rotation of the region, in degrees.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<short> UnkA { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<short> UnkB { get; set; }

            /// <summary>
            /// Region is inactive unless this part is drawn; null for always active.
            /// </summary>
            public string ActivationPartName { get; set; }
            private int ActivationPartIndex;

            /// <summary>
            /// An ID used to identify this region in event scripts.
            /// </summary>
            public int EntityID { get; set; }

            private protected Region(string name) {
                this.Name = name;
                this.Shape = new MSB.Shape.Point();
                this.EntityID = -1;
                this.UnkA = new List<short>();
                this.UnkB = new List<short>();
            }

            /// <summary>
            /// Creates a deep copy of the region.
            /// </summary>
            public Region DeepCopy() {
                var region = (Region)this.MemberwiseClone();
                region.Shape = this.Shape.DeepCopy();
                region.UnkA = new List<short>(this.UnkA);
                region.UnkB = new List<short>(this.UnkB);
                this.DeepCopyTo(region);
                return region;
            }
            IMsbRegion IMsbRegion.DeepCopy() => this.DeepCopy();

            private protected virtual void DeepCopyTo(Region region) { }

            private protected Region(BinaryReaderEx br) {
                long start = br.Position;

                long nameOffset = br.ReadInt64();
                _ = br.AssertUInt32((uint)this.Type);
                _ = br.ReadInt32(); // ID
                MSB.ShapeType shapeType = br.ReadEnum32<MSB.ShapeType>();
                this.Position = br.ReadVector3();
                this.Rotation = br.ReadVector3();
                this.Unk2C = br.ReadInt32();
                long baseDataOffset1 = br.ReadInt64();
                long baseDataOffset2 = br.ReadInt64();
                _ = br.AssertInt32(-1);
                this.MapStudioLayer = br.ReadUInt32();
                long shapeDataOffset = br.ReadInt64();
                long baseDataOffset3 = br.ReadInt64();
                long typeDataOffset = br.ReadInt64();

                this.Shape = MSB.Shape.Create(shapeType);

                if (nameOffset == 0) {
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {this.GetType()}.");
                }

                if (baseDataOffset1 == 0) {
                    throw new InvalidDataException($"{nameof(baseDataOffset1)} must not be 0 in type {this.GetType()}.");
                }

                if (baseDataOffset2 == 0) {
                    throw new InvalidDataException($"{nameof(baseDataOffset2)} must not be 0 in type {this.GetType()}.");
                }

                if (this.Shape.HasShapeData ^ shapeDataOffset != 0) {
                    throw new InvalidDataException($"Unexpected {nameof(shapeDataOffset)} 0x{shapeDataOffset:X} in type {this.GetType()}.");
                }

                if (baseDataOffset3 == 0) {
                    throw new InvalidDataException($"{nameof(baseDataOffset3)} must not be 0 in type {this.GetType()}.");
                }

                if (this.ShouldHaveTypeData == TypeDataPresence.Never && typeDataOffset != 0
                    || this.ShouldHaveTypeData == TypeDataPresence.Always && typeDataOffset == 0
                    || this.ShouldHaveTypeData == TypeDataPresence.AlwaysNull && typeDataOffset != 0) {
                    throw new InvalidDataException($"Unexpected {nameof(typeDataOffset)} 0x{typeDataOffset:X} in type {this.GetType()}.");
                }

                br.Position = start + nameOffset;
                this.Name = br.ReadUTF16();

                br.Position = start + baseDataOffset1;
                short countA = br.ReadInt16();
                this.UnkA = new List<short>(br.ReadInt16s(countA));

                br.Position = start + baseDataOffset2;
                short countB = br.ReadInt16();
                this.UnkB = new List<short>(br.ReadInt16s(countB));

                if (this.Shape.HasShapeData) {
                    br.Position = start + shapeDataOffset;
                    this.Shape.ReadShapeData(br);
                }

                br.Position = start + baseDataOffset3;
                this.ActivationPartIndex = br.ReadInt32();
                this.EntityID = br.ReadInt32();

                if (typeDataOffset != 0 || this.ShouldHaveTypeData == TypeDataPresence.AlwaysNull) {
                    if (typeDataOffset != 0) {
                        br.Position = start + typeDataOffset;
                    }

                    this.ReadTypeData(br);
                }
            }

            private protected virtual void ReadTypeData(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(ReadTypeData)}.");

            internal override void Write(BinaryWriterEx bw, int id) {
                long start = bw.Position;

                bw.ReserveInt64("NameOffset");
                bw.WriteUInt32((uint)this.Type);
                bw.WriteInt32(id);
                bw.WriteUInt32((uint)this.Shape.Type);
                bw.WriteVector3(this.Position);
                bw.WriteVector3(this.Rotation);
                bw.WriteInt32(this.Unk2C);
                bw.ReserveInt64("BaseDataOffset1");
                bw.ReserveInt64("BaseDataOffset2");
                bw.WriteInt32(-1);
                bw.WriteUInt32(this.MapStudioLayer);

                bw.ReserveInt64("ShapeDataOffset");
                bw.ReserveInt64("BaseDataOffset3");
                bw.ReserveInt64("TypeDataOffset");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(MSB.ReambiguateName(this.Name), true);
                bw.Pad(4);

                bw.FillInt64("BaseDataOffset1", bw.Position - start);
                bw.WriteInt16((short)this.UnkA.Count);
                bw.WriteInt16s(this.UnkA);
                bw.Pad(4);

                bw.FillInt64("BaseDataOffset2", bw.Position - start);
                bw.WriteInt16((short)this.UnkB.Count);
                bw.WriteInt16s(this.UnkB);
                bw.Pad(8);

                if (this.Shape.HasShapeData) {
                    bw.FillInt64("ShapeDataOffset", bw.Position - start);
                    this.Shape.WriteShapeData(bw);
                } else {
                    bw.FillInt64("ShapeDataOffset", 0);
                }

                bw.FillInt64("BaseDataOffset3", bw.Position - start);
                bw.WriteInt32(this.ActivationPartIndex);
                bw.WriteInt32(this.EntityID);

                if (this.DoesHaveTypeData && this.ShouldHaveTypeData != TypeDataPresence.AlwaysNull) {
                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                } else {
                    bw.FillInt64("TypeDataOffset", 0);
                }

                if (this.DoesHaveTypeData) {
                    this.WriteTypeData(bw);
                }

                bw.Pad(8);
            }

            private protected virtual void WriteTypeData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(WriteTypeData)}.");

            internal virtual void GetNames(MSB3 msb, Entries entries) => this.ActivationPartName = MSB.FindName(entries.Parts, this.ActivationPartIndex);

            internal virtual void GetIndices(MSB3 msb, Entries entries) => this.ActivationPartIndex = MSB.FindIndex(entries.Parts, this.ActivationPartName);

            /// <summary>
            /// Returns the region type, shape type, and name of this region.
            /// </summary>
            public override string ToString() => $"{this.Type} {this.Shape.Type} : {this.Name}";

            /// <summary>
            /// A point where other players invade your world.
            /// </summary>
            public class InvasionPoint : Region {
                private protected override RegionType Type => RegionType.InvasionPoint;
                private protected override TypeDataPresence ShouldHaveTypeData => TypeDataPresence.Always;
                private protected override bool DoesHaveTypeData => true;

                /// <summary>
                /// Not sure what this does.
                /// </summary>
                public int Priority { get; set; }

                /// <summary>
                /// Creates an InvasionPoint with default values.
                /// </summary>
                public InvasionPoint() : base($"{nameof(Region)}: {nameof(InvasionPoint)}") { }

                internal InvasionPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) => this.Priority = br.ReadInt32();

                private protected override void WriteTypeData(BinaryWriterEx bw) => bw.WriteInt32(this.Priority);
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class EnvironmentMapPoint : Region {
                private protected override RegionType Type => RegionType.EnvironmentMapPoint;
                private protected override TypeDataPresence ShouldHaveTypeData => TypeDataPresence.Sometimes;
                private protected override bool DoesHaveTypeData => this.SaveTypeData;

                /// <summary>
                /// Whether or not the UnkFlags will be written to the file.
                /// </summary>
                public bool SaveTypeData { get; set; }

                /// <summary>
                /// Unknown; observed values 0x80 and 0x100.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Creates an EnvironmentMapPoint with default values.
                /// </summary>
                public EnvironmentMapPoint() : base($"{nameof(Region)}: {nameof(EnvironmentMapPoint)}") {
                    this.SaveTypeData = true;
                    this.UnkT00 = 0x80;
                }

                internal EnvironmentMapPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.SaveTypeData = true;
                    this.UnkT00 = br.ReadInt32();
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.UnkT00);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// A region that plays a sound while you're in it.
            /// </summary>
            public class Sound : Region {
                /// <summary>
                /// Types of sound that may be in a Sound region.
                /// </summary>
                public enum SndType : uint {
                    /// <summary>
                    /// Ambient sounds like wind, creaking, etc.
                    /// </summary>
                    Environment = 0,

                    /// <summary>
                    /// Boss fight music.
                    /// </summary>
                    BGM = 6,

                    /// <summary>
                    /// Character voices.
                    /// </summary>
                    Voice = 7,
                }

                private protected override RegionType Type => RegionType.Sound;
                private protected override TypeDataPresence ShouldHaveTypeData => TypeDataPresence.Always;
                private protected override bool DoesHaveTypeData => true;

                /// <summary>
                /// Type of sound in this region; determines mixing behavior like muffling.
                /// </summary>
                public SndType SoundType { get; set; }

                /// <summary>
                /// ID of the sound to play in this region, or 0 for child regions.
                /// </summary>
                public int SoundID { get; set; }

                /// <summary>
                /// Names of other Sound regions which extend this one.
                /// </summary>
                public string[] ChildRegionNames { get; private set; }
                private int[] ChildRegionIndices;

                /// <summary>
                /// Creates a Sound with default values.
                /// </summary>
                public Sound() : base($"{nameof(Region)}: {nameof(Sound)}") {
                    this.SoundType = SndType.Environment;
                    this.ChildRegionNames = new string[16];
                }

                private protected override void DeepCopyTo(Region region) {
                    var sound = (Sound)region;
                    sound.ChildRegionNames = (string[])this.ChildRegionNames.Clone();
                }

                internal Sound(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.SoundType = br.ReadEnum32<SndType>();
                    this.SoundID = br.ReadInt32();
                    this.ChildRegionIndices = br.ReadInt32s(16);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteUInt32((uint)this.SoundType);
                    bw.WriteInt32(this.SoundID);
                    bw.WriteInt32s(this.ChildRegionIndices);
                }

                internal override void GetNames(MSB3 msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.ChildRegionNames = MSB.FindNames(entries.Regions, this.ChildRegionIndices);
                }

                internal override void GetIndices(MSB3 msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.ChildRegionIndices = MSB.FindIndices(entries.Regions, this.ChildRegionNames);
                }
            }

            /// <summary>
            /// A region that plays a special effect.
            /// </summary>
            public class SFX : Region {
                private protected override RegionType Type => RegionType.SFX;
                private protected override TypeDataPresence ShouldHaveTypeData => TypeDataPresence.Always;
                private protected override bool DoesHaveTypeData => true;

                /// <summary>
                /// The ID of the .fxr file to play in this region.
                /// </summary>
                public int EffectID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// If true, the effect is off by default until enabled by event scripts.
                /// </summary>
                public bool StartDisabled { get; set; }

                /// <summary>
                /// Creates a SFX with default values.
                /// </summary>
                public SFX() : base($"{nameof(Region)}: {nameof(SFX)}") {
                    this.EffectID = -1;
                    this.UnkT04 = -1;
                }

                internal SFX(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.EffectID = br.ReadInt32();
                    // These are not additional FFX IDs, I checked
                    this.UnkT04 = br.ReadInt32();
                    _ = br.AssertInt32(-1);
                    _ = br.AssertInt32(-1);
                    _ = br.AssertInt32(-1);
                    this.StartDisabled = br.AssertInt32(0, 1) == 1;
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.EffectID);
                    bw.WriteInt32(this.UnkT04);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(this.StartDisabled ? 1 : 0);
                }
            }

            /// <summary>
            /// Unknown exactly what this does.
            /// </summary>
            public class WindSFX : Region {
                private protected override RegionType Type => RegionType.WindSFX;
                private protected override TypeDataPresence ShouldHaveTypeData => TypeDataPresence.Always;
                private protected override bool DoesHaveTypeData => true;

                /// <summary>
                /// ID of an .fxr file.
                /// </summary>
                public int EffectID { get; set; }

                /// <summary>
                /// Name of a corresponding WindArea region.
                /// </summary>
                public string WindAreaName { get; set; }
                private int WindAreaIndex;

                /// <summary>
                /// Creates a WindSFX with default values.
                /// </summary>
                public WindSFX() : base($"{nameof(Region)}: {nameof(WindSFX)}") => this.EffectID = -1;

                internal WindSFX(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.EffectID = br.ReadInt32();
                    _ = br.AssertInt32(-1);
                    _ = br.AssertInt32(-1);
                    _ = br.AssertInt32(-1);
                    _ = br.AssertInt32(-1);
                    this.WindAreaIndex = br.ReadInt32();
                    _ = br.AssertSingle(-1);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.EffectID);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(this.WindAreaIndex);
                    bw.WriteSingle(-1);
                }

                internal override void GetNames(MSB3 msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.WindAreaName = MSB.FindName(entries.Regions, this.WindAreaIndex);
                }

                internal override void GetIndices(MSB3 msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.WindAreaIndex = MSB.FindIndex(entries.Regions, this.WindAreaName);
                }
            }

            /// <summary>
            /// A region where players enter the map.
            /// </summary>
            public class SpawnPoint : Region {
                private protected override RegionType Type => RegionType.SpawnPoint;
                private protected override TypeDataPresence ShouldHaveTypeData => TypeDataPresence.Always;
                private protected override bool DoesHaveTypeData => true;

                /// <summary>
                /// Unknown; seems kind of like a region index, but also kind of doesn't.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Creates a SpawnPoint with default values.
                /// </summary>
                public SpawnPoint() : base($"{nameof(Region)}: {nameof(SpawnPoint)}") => this.UnkT00 = -1;

                internal SpawnPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.UnkT00);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// An orange developer message.
            /// </summary>
            public class Message : Region {
                private protected override RegionType Type => RegionType.Message;
                private protected override TypeDataPresence ShouldHaveTypeData => TypeDataPresence.Always;
                private protected override bool DoesHaveTypeData => true;

                /// <summary>
                /// ID of the message's text in the FMGs.
                /// </summary>
                public short MessageID { get; set; }

                /// <summary>
                /// Unknown. Always 0 or 2.
                /// </summary>
                public short UnkT02 { get; set; }

                /// <summary>
                /// Whether the message requires Seek Guidance to appear.
                /// </summary>
                public bool Hidden { get; set; }

                /// <summary>
                /// Creates a Message with default values.
                /// </summary>
                public Message() : base($"{nameof(Region)}: {nameof(Message)}") => this.MessageID = -1;

                internal Message(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.MessageID = br.ReadInt16();
                    this.UnkT02 = br.ReadInt16();
                    this.Hidden = br.AssertInt32(0, 1) == 1;
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt16(this.MessageID);
                    bw.WriteInt16(this.UnkT02);
                    bw.WriteInt32(this.Hidden ? 1 : 0);
                }
            }

            /// <summary>
            /// A point in a patrol route.
            /// </summary>
            public class PatrolRoute : Region {
                private protected override RegionType Type => RegionType.PatrolRoute;
                private protected override TypeDataPresence ShouldHaveTypeData => TypeDataPresence.Never;
                private protected override bool DoesHaveTypeData => false;

                /// <summary>
                /// Creates a PatrolRoute with default values.
                /// </summary>
                public PatrolRoute() : base($"{nameof(Region)}: {nameof(PatrolRoute)}") { }

                internal PatrolRoute(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class MovementPoint : Region {
                private protected override RegionType Type => RegionType.MovementPoint;
                private protected override TypeDataPresence ShouldHaveTypeData => TypeDataPresence.Never;
                private protected override bool DoesHaveTypeData => false;

                /// <summary>
                /// Creates a MovementPoint with default values.
                /// </summary>
                public MovementPoint() : base($"{nameof(Region)}: {nameof(MovementPoint)}") { }

                internal MovementPoint(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unknown; seems to be used for moving enemies around.
            /// </summary>
            public class WarpPoint : Region {
                private protected override RegionType Type => RegionType.WarpPoint;
                private protected override TypeDataPresence ShouldHaveTypeData => TypeDataPresence.Never;
                private protected override bool DoesHaveTypeData => false;

                /// <summary>
                /// Creates a WarpPoint with default values.
                /// </summary>
                public WarpPoint() : base($"{nameof(Region)}: {nameof(WarpPoint)}") { }

                internal WarpPoint(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Triggers an enemy when entered.
            /// </summary>
            public class ActivationArea : Region {
                private protected override RegionType Type => RegionType.ActivationArea;
                private protected override TypeDataPresence ShouldHaveTypeData => TypeDataPresence.Never;
                private protected override bool DoesHaveTypeData => false;

                /// <summary>
                /// Creates an ActivationArea with default values.
                /// </summary>
                public ActivationArea() : base($"{nameof(Region)}: {nameof(ActivationArea)}") { }

                internal ActivationArea(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Any kind of region for use with event scripts.
            /// </summary>
            public class Event : Region {
                private protected override RegionType Type => RegionType.Event;
                private protected override TypeDataPresence ShouldHaveTypeData => TypeDataPresence.Never;
                private protected override bool DoesHaveTypeData => false;

                /// <summary>
                /// Creates an Event with default values.
                /// </summary>
                public Event() : base($"{nameof(Region)}: {nameof(Event)}") { }

                internal Event(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unknown; only used 3 times in Catacombs.
            /// </summary>
            public class Logic : Region {
                private protected override RegionType Type => RegionType.Logic;
                private protected override TypeDataPresence ShouldHaveTypeData => TypeDataPresence.Never;
                private protected override bool DoesHaveTypeData => false;

                /// <summary>
                /// Creates a Logic with default values.
                /// </summary>
                public Logic() : base($"{nameof(Region)}: {nameof(Logic)}") { }

                internal Logic(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class EnvironmentMapEffectBox : Region {
                private protected override RegionType Type => RegionType.EnvironmentMapEffectBox;
                private protected override TypeDataPresence ShouldHaveTypeData => TypeDataPresence.Always;
                private protected override bool DoesHaveTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Compare { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool UnkT08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT09 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT0A { get; set; }

                /// <summary>
                /// Creates an EnvironmentMapEffectBox with default values.
                /// </summary>
                public EnvironmentMapEffectBox() : base($"{nameof(Region)}: {nameof(EnvironmentMapEffectBox)}") { }

                internal EnvironmentMapEffectBox(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadSingle();
                    this.Compare = br.ReadSingle();
                    this.UnkT08 = br.ReadBoolean();
                    this.UnkT09 = br.ReadByte();
                    this.UnkT0A = br.ReadInt16();
                    _ = br.AssertInt32(0); // float (6)
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteSingle(this.UnkT00);
                    bw.WriteSingle(this.Compare);
                    bw.WriteBoolean(this.UnkT08);
                    bw.WriteByte(this.UnkT09);
                    bw.WriteInt16(this.UnkT0A);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown; each WindSFX has a reference to a WindArea.
            /// </summary>
            public class WindArea : Region {
                private protected override RegionType Type => RegionType.WindArea;
                private protected override TypeDataPresence ShouldHaveTypeData => TypeDataPresence.Never;
                private protected override bool DoesHaveTypeData => false;

                /// <summary>
                /// Creates a WindArea with default values.
                /// </summary>
                public WindArea() : base($"{nameof(Region)}: {nameof(WindArea)}") { }

                internal WindArea(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Muffles environmental sound while inside it.
            /// </summary>
            public class MufflingBox : Region {
                private protected override RegionType Type => RegionType.MufflingBox;
                private protected override TypeDataPresence ShouldHaveTypeData => TypeDataPresence.AlwaysNull;
                private protected override bool DoesHaveTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Creates a MufflingBox with default values.
                /// </summary>
                public MufflingBox() : base($"{nameof(Region)}: {nameof(MufflingBox)}") { }

                internal MufflingBox(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) => this.UnkT00 = br.ReadInt32();

                private protected override void WriteTypeData(BinaryWriterEx bw) => bw.WriteInt32(this.UnkT00);
            }

            /// <summary>
            /// A region leading into a MufflingBox.
            /// </summary>
            public class MufflingPortal : Region {
                private protected override RegionType Type => RegionType.MufflingPortal;
                private protected override TypeDataPresence ShouldHaveTypeData => TypeDataPresence.AlwaysNull;
                private protected override bool DoesHaveTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Creates a MufflingPortal with default values.
                /// </summary>
                public MufflingPortal() : base($"{nameof(Region)}: {nameof(MufflingPortal)}") { }

                internal MufflingPortal(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.UnkT00);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Most likely an unused region.
            /// </summary>
            public class Other : Region {
                private protected override RegionType Type => RegionType.Other;
                private protected override TypeDataPresence ShouldHaveTypeData => TypeDataPresence.Never;
                private protected override bool DoesHaveTypeData => false;

                /// <summary>
                /// Creates an Other with default values.
                /// </summary>
                public Other() : base($"{nameof(Region)}: {nameof(Other)}") { }

                internal Other(BinaryReaderEx br) : base(br) { }
            }
        }
    }
}
