using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using SoulsFormats.Formats.MSB;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MSBS {
        internal enum RegionType : uint {
            InvasionPoint = 1,
            EnvironmentMapPoint = 2,
            //Region3 = 3,
            Sound = 4,
            SFX = 5,
            WindSFX = 6,
            //Region7 = 7,
            SpawnPoint = 8,
            //Message = 9,
            //PseudoMultiplayer = 10,
            PatrolRoute = 11,
            //MovementPoint = 12,
            WarpPoint = 13,
            ActivationArea = 14,
            Event = 15,
            Logic = 0, // There are no regions of type 16 and type 0 is written in this order, so I suspect this is correct
            EnvironmentMapEffectBox = 17,
            WindArea = 18,
            //Region19 = 19,
            MufflingBox = 20,
            MufflingPortal = 21,
            //DrawGroupArea = 22,
            SoundSpaceOverride = 23,
            MufflingPlane = 24,
            PartsGroupArea = 25,
            AutoDrawGroupPoint = 26,
            Other = 0xFFFFFFFF,
        }

        /// <summary>
        /// Points and volumes used to trigger various effects.
        /// </summary>
        public class PointParam : Param<Region>, IMsbParam<IMsbRegion> {
            /// <summary>
            /// Previously points where players will appear when invading; not sure if they do anything in Sekiro.
            /// </summary>
            public List<Region.InvasionPoint> InvasionPoints { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.EnvironmentMapPoint> EnvironmentMapPoints { get; set; }

            /// <summary>
            /// Areas where a sound will play.
            /// </summary>
            public List<Region.Sound> Sounds { get; set; }

            /// <summary>
            /// Points for particle effects to play at.
            /// </summary>
            public List<Region.SFX> SFX { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.WindSFX> WindSFX { get; set; }

            /// <summary>
            /// Points where the player can spawn into a map.
            /// </summary>
            public List<Region.SpawnPoint> SpawnPoints { get; set; }

            /// <summary>
            /// Points that describe an NPC patrol path.
            /// </summary>
            public List<Region.PatrolRoute> PatrolRoutes { get; set; }

            /// <summary>
            /// Regions for warping the player.
            /// </summary>
            public List<Region.WarpPoint> WarpPoints { get; set; }

            /// <summary>
            /// Regions that trigger enemies when entered.
            /// </summary>
            public List<Region.ActivationArea> ActivationAreas { get; set; }

            /// <summary>
            /// Generic regions for use with event scripts.
            /// </summary>
            public List<Region.Event> Events { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.Logic> Logic { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.EnvironmentMapEffectBox> EnvironmentMapEffectBoxes { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.WindArea> WindAreas { get; set; }

            /// <summary>
            /// Areas where sound is muffled.
            /// </summary>
            public List<Region.MufflingBox> MufflingBoxes { get; set; }

            /// <summary>
            /// Entrances to muffling boxes.
            /// </summary>
            public List<Region.MufflingPortal> MufflingPortals { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.SoundSpaceOverride> SoundSpaceOverrides { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.MufflingPlane> MufflingPlanes { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.PartsGroupArea> PartsGroupAreas { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.AutoDrawGroupPoint> AutoDrawGroupPoints { get; set; }

            /// <summary>
            /// Most likely a dumping ground for unused regions.
            /// </summary>
            public List<Region.Other> Others { get; set; }

            /// <summary>
            /// Creates an empty PointParam with the default version.
            /// </summary>
            public PointParam() : base(35, "POINT_PARAM_ST") {
                this.InvasionPoints = new List<Region.InvasionPoint>();
                this.EnvironmentMapPoints = new List<Region.EnvironmentMapPoint>();
                this.Sounds = new List<Region.Sound>();
                this.SFX = new List<Region.SFX>();
                this.WindSFX = new List<Region.WindSFX>();
                this.SpawnPoints = new List<Region.SpawnPoint>();
                this.PatrolRoutes = new List<Region.PatrolRoute>();
                this.WarpPoints = new List<Region.WarpPoint>();
                this.ActivationAreas = new List<Region.ActivationArea>();
                this.Events = new List<Region.Event>();
                this.Logic = new List<Region.Logic>();
                this.EnvironmentMapEffectBoxes = new List<Region.EnvironmentMapEffectBox>();
                this.WindAreas = new List<Region.WindArea>();
                this.MufflingBoxes = new List<Region.MufflingBox>();
                this.MufflingPortals = new List<Region.MufflingPortal>();
                this.SoundSpaceOverrides = new List<Region.SoundSpaceOverride>();
                this.MufflingPlanes = new List<Region.MufflingPlane>();
                this.PartsGroupAreas = new List<Region.PartsGroupArea>();
                this.AutoDrawGroupPoints = new List<Region.AutoDrawGroupPoint>();
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
                    case Region.PatrolRoute r: this.PatrolRoutes.Add(r); break;
                    case Region.WarpPoint r: this.WarpPoints.Add(r); break;
                    case Region.ActivationArea r: this.ActivationAreas.Add(r); break;
                    case Region.Event r: this.Events.Add(r); break;
                    case Region.Logic r: this.Logic.Add(r); break;
                    case Region.EnvironmentMapEffectBox r: this.EnvironmentMapEffectBoxes.Add(r); break;
                    case Region.WindArea r: this.WindAreas.Add(r); break;
                    case Region.MufflingBox r: this.MufflingBoxes.Add(r); break;
                    case Region.MufflingPortal r: this.MufflingPortals.Add(r); break;
                    case Region.SoundSpaceOverride r: this.SoundSpaceOverrides.Add(r); break;
                    case Region.MufflingPlane r: this.MufflingPlanes.Add(r); break;
                    case Region.PartsGroupArea r: this.PartsGroupAreas.Add(r); break;
                    case Region.AutoDrawGroupPoint r: this.AutoDrawGroupPoints.Add(r); break;
                    case Region.Other r: this.Others.Add(r); break;

                    default:
                        throw new ArgumentException($"Unrecognized type {region.GetType()}.", nameof(region));
                }
                return region;
            }
            IMsbRegion IMsbParam<IMsbRegion>.Add(IMsbRegion item) => this.Add((Region)item);

            /// <summary>
            /// Returns every region in the order they'll be written.
            /// </summary>
            public override List<Region> GetEntries() => SFUtil.ConcatAll<Region>(
                    this.InvasionPoints, this.EnvironmentMapPoints, this.Sounds, this.SFX, this.WindSFX,
                    this.SpawnPoints, this.PatrolRoutes, this.WarpPoints, this.ActivationAreas, this.Events,
                    this.Logic, this.EnvironmentMapEffectBoxes, this.WindAreas, this.MufflingBoxes, this.MufflingPortals,
                    this.SoundSpaceOverrides, this.MufflingPlanes, this.PartsGroupAreas, this.AutoDrawGroupPoints, this.Others);
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
                    RegionType.PatrolRoute => this.PatrolRoutes.EchoAdd(new Region.PatrolRoute(br)),
                    RegionType.WarpPoint => this.WarpPoints.EchoAdd(new Region.WarpPoint(br)),
                    RegionType.ActivationArea => this.ActivationAreas.EchoAdd(new Region.ActivationArea(br)),
                    RegionType.Event => this.Events.EchoAdd(new Region.Event(br)),
                    RegionType.Logic => this.Logic.EchoAdd(new Region.Logic(br)),
                    RegionType.EnvironmentMapEffectBox => this.EnvironmentMapEffectBoxes.EchoAdd(new Region.EnvironmentMapEffectBox(br)),
                    RegionType.WindArea => this.WindAreas.EchoAdd(new Region.WindArea(br)),
                    RegionType.MufflingBox => this.MufflingBoxes.EchoAdd(new Region.MufflingBox(br)),
                    RegionType.MufflingPortal => this.MufflingPortals.EchoAdd(new Region.MufflingPortal(br)),
                    RegionType.SoundSpaceOverride => this.SoundSpaceOverrides.EchoAdd(new Region.SoundSpaceOverride(br)),
                    RegionType.MufflingPlane => this.MufflingPlanes.EchoAdd(new Region.MufflingPlane(br)),
                    RegionType.PartsGroupArea => this.PartsGroupAreas.EchoAdd(new Region.PartsGroupArea(br)),
                    RegionType.AutoDrawGroupPoint => this.AutoDrawGroupPoints.EchoAdd(new Region.AutoDrawGroupPoint(br)),
                    RegionType.Other => this.Others.EchoAdd(new Region.Other(br)),
                    _ => throw new NotImplementedException($"Unimplemented region type: {type}"),
                };
            }
        }

        /// <summary>
        /// A point or volume that triggers some sort of interaction.
        /// </summary>
        public abstract class Region : Entry, IMsbRegion {
            private protected abstract RegionType Type { get; }
            private protected abstract bool HasTypeData { get; }

            /// <summary>
            /// The shape of the region.
            /// </summary>
            public MSB.Shape Shape { get; set; }

            /// <summary>
            /// The location of the region.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// The rotiation of the region, in degrees.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk2C { get; set; }

            /// <summary>
            /// Controls whether the region is active in different ceremonies.
            /// </summary>
            public uint MapStudioLayer { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<short> UnkA { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<short> UnkB { get; set; }

            /// <summary>
            /// If specified, the region is only active when the part is loaded.
            /// </summary>
            public string ActivationPartName { get; set; }
            private int ActivationPartIndex;

            /// <summary>
            /// Identifies the region in event scripts.
            /// </summary>
            public int EntityID { get; set; }

            private protected Region(string name) {
                this.Name = name;
                this.Shape = new MSB.Shape.Point();
                this.MapStudioLayer = 0xFFFFFFFF;
                this.UnkA = new List<short>();
                this.UnkB = new List<short>();
                this.EntityID = -1;
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

                if (this.HasTypeData ^ typeDataOffset != 0) {
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

                if (this.HasTypeData) {
                    if (this.Type is RegionType.SoundSpaceOverride or RegionType.PartsGroupArea or RegionType.AutoDrawGroupPoint) {
                        bw.Pad(8);
                    }

                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                    this.WriteTypeData(bw);
                } else {
                    bw.FillInt64("TypeDataOffset", 0);
                }
                bw.Pad(8);
            }

            private protected virtual void WriteTypeData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(ReadTypeData)}.");

            internal virtual void GetNames(Entries entries) {
                this.ActivationPartName = MSB.FindName(entries.Parts, this.ActivationPartIndex);
                if (this.Shape is MSB.Shape.Composite composite) {
                    composite.GetNames(entries.Regions);
                }
            }

            internal virtual void GetIndices(Entries entries) {
                this.ActivationPartIndex = MSB.FindIndex(entries.Parts, this.ActivationPartName);
                if (this.Shape is MSB.Shape.Composite composite) {
                    composite.GetIndices(entries.Regions);
                }
            }

            /// <summary>
            /// Returns the type, shape type, and name of the region as a string.
            /// </summary>
            /// <returns></returns>
            public override string ToString() => $"{this.Type} {this.Shape.Type} {this.Name}";

            /// <summary>
            /// A point where a player can invade your world.
            /// </summary>
            public class InvasionPoint : Region {
                private protected override RegionType Type => RegionType.InvasionPoint;
                private protected override bool HasTypeData => true;

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
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT0C { get; set; }

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
                public int UnkT18 { get; set; }

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
                /// Creates an EnvironmentMapPoint with default values.
                /// </summary>
                public EnvironmentMapPoint() : base($"{nameof(Region)}: {nameof(EnvironmentMapPoint)}") { }

                internal EnvironmentMapPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadSingle();
                    this.UnkT04 = br.ReadInt32();
                    _ = br.AssertInt32(-1);
                    this.UnkT0C = br.ReadInt32();
                    this.UnkT10 = br.ReadSingle();
                    this.UnkT14 = br.ReadSingle();
                    this.UnkT18 = br.ReadInt32();
                    this.UnkT1C = br.ReadInt32();
                    this.UnkT20 = br.ReadInt32();
                    this.UnkT24 = br.ReadInt32();
                    this.UnkT28 = br.ReadInt32();
                    _ = br.AssertInt32(-1);
                    br.AssertPattern(0x10, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteSingle(this.UnkT00);
                    bw.WriteInt32(this.UnkT04);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(this.UnkT0C);
                    bw.WriteSingle(this.UnkT10);
                    bw.WriteSingle(this.UnkT14);
                    bw.WriteInt32(this.UnkT18);
                    bw.WriteInt32(this.UnkT1C);
                    bw.WriteInt32(this.UnkT20);
                    bw.WriteInt32(this.UnkT24);
                    bw.WriteInt32(this.UnkT28);
                    bw.WriteInt32(-1);
                    bw.WritePattern(0x10, 0x00);
                }
            }

            /// <summary>
            /// An area where a sound plays.
            /// </summary>
            public class Sound : Region {
                private protected override RegionType Type => RegionType.Sound;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// The category of the sound.
                /// </summary>
                public int SoundType { get; set; }

                /// <summary>
                /// The ID of the sound.
                /// </summary>
                public int SoundID { get; set; }

                /// <summary>
                /// References to other regions used to build a composite shape.
                /// </summary>
                public string[] ChildRegionNames { get; private set; }
                private int[] ChildRegionIndices;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT48 { get; set; }

                /// <summary>
                /// Creates a Sound with default values.
                /// </summary>
                public Sound() : base($"{nameof(Region)}: {nameof(Sound)}") => this.ChildRegionNames = new string[16];

                private protected override void DeepCopyTo(Region region) {
                    var sound = (Sound)region;
                    sound.ChildRegionNames = (string[])this.ChildRegionNames.Clone();
                }

                internal Sound(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.SoundType = br.ReadInt32();
                    this.SoundID = br.ReadInt32();
                    this.ChildRegionIndices = br.ReadInt32s(16);
                    this.UnkT48 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.SoundType);
                    bw.WriteInt32(this.SoundID);
                    bw.WriteInt32s(this.ChildRegionIndices);
                    bw.WriteInt32(this.UnkT48);
                }

                internal override void GetNames(Entries entries) {
                    base.GetNames(entries);
                    this.ChildRegionNames = MSB.FindNames(entries.Regions, this.ChildRegionIndices);
                }

                internal override void GetIndices(Entries entries) {
                    base.GetIndices(entries);
                    this.ChildRegionIndices = MSB.FindIndices(entries.Regions, this.ChildRegionNames);
                }
            }

            /// <summary>
            /// A point where a particle effect can play.
            /// </summary>
            public class SFX : Region {
                private protected override RegionType Type => RegionType.SFX;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// The ID of the particle effect FFX.
                /// </summary>
                public int EffectID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// Whether the effect is off until activated.
                /// </summary>
                public int StartDisabled { get; set; }

                /// <summary>
                /// Creates an SFX with default values.
                /// </summary>
                public SFX() : base($"{nameof(Region)}: {nameof(SFX)}") { }

                internal SFX(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.EffectID = br.ReadInt32();
                    this.UnkT04 = br.ReadInt32();
                    _ = br.AssertInt32(-1);
                    _ = br.AssertInt32(-1);
                    _ = br.AssertInt32(-1);
                    this.StartDisabled = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.EffectID);
                    bw.WriteInt32(this.UnkT04);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(this.StartDisabled);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class WindSFX : Region {
                private protected override RegionType Type => RegionType.WindSFX;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// ID of the effect FFX.
                /// </summary>
                public int EffectID { get; set; }

                /// <summary>
                /// Reference to a WindArea region.
                /// </summary>
                public string WindAreaName { get; set; }
                private int WindAreaIndex;

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT18 { get; set; }

                /// <summary>
                /// Creates a WindSFX with default values.
                /// </summary>
                public WindSFX() : base($"{nameof(Region)}: {nameof(WindSFX)}") { }

                internal WindSFX(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.EffectID = br.ReadInt32();
                    br.AssertPattern(0x10, 0xFF);
                    this.WindAreaIndex = br.ReadInt32();
                    this.UnkT18 = br.ReadSingle();
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.EffectID);
                    bw.WritePattern(0x10, 0xFF);
                    bw.WriteInt32(this.WindAreaIndex);
                    bw.WriteSingle(this.UnkT18);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(Entries entries) {
                    base.GetNames(entries);
                    this.WindAreaName = MSB.FindName(entries.Regions, this.WindAreaIndex);
                }

                internal override void GetIndices(Entries entries) {
                    base.GetIndices(entries);
                    this.WindAreaIndex = MSB.FindIndex(entries.Regions, this.WindAreaName);
                }
            }

            /// <summary>
            /// A point where the player can spawn into the map.
            /// </summary>
            public class SpawnPoint : Region {
                private protected override RegionType Type => RegionType.SpawnPoint;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates a SpawnPoint with default values.
                /// </summary>
                public SpawnPoint() : base($"{nameof(Region)}: {nameof(SpawnPoint)}") { }

                internal SpawnPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    _ = br.AssertInt32(-1);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(-1);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// A point along an NPC patrol path.
            /// </summary>
            public class PatrolRoute : Region {
                private protected override RegionType Type => RegionType.PatrolRoute;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates a PatrolRoute with default values.
                /// </summary>
                public PatrolRoute() : base($"{nameof(Region)}: {nameof(PatrolRoute)}") { }

                internal PatrolRoute(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A point the player can be warped to.
            /// </summary>
            public class WarpPoint : Region {
                private protected override RegionType Type => RegionType.WarpPoint;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates a WarpPoint with default values.
                /// </summary>
                public WarpPoint() : base($"{nameof(Region)}: {nameof(WarpPoint)}") { }

                internal WarpPoint(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// An area that triggers enemies when entered.
            /// </summary>
            public class ActivationArea : Region {
                private protected override RegionType Type => RegionType.ActivationArea;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates an ActivationArea with default values.
                /// </summary>
                public ActivationArea() : base($"{nameof(Region)}: {nameof(ActivationArea)}") { }

                internal ActivationArea(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A generic area used by event scripts.
            /// </summary>
            public class Event : Region {
                private protected override RegionType Type => RegionType.Event;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates an Event with default values.
                /// </summary>
                public Event() : base($"{nameof(Region)}: {nameof(Event)}") { }

                internal Event(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Logic : Region {
                private protected override RegionType Type => RegionType.Logic;
                private protected override bool HasTypeData => false;

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
                private protected override bool HasTypeData => true;

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
                public byte UnkT08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT09 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT0A { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT28 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT2C { get; set; }

                /// <summary>
                /// Creates an EnvironmentMapEffectBox with default values.
                /// </summary>
                public EnvironmentMapEffectBox() : base($"{nameof(Region)}: {nameof(EnvironmentMapEffectBox)}") { }

                internal EnvironmentMapEffectBox(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadSingle();
                    this.Compare = br.ReadSingle();
                    this.UnkT08 = br.ReadByte();
                    this.UnkT09 = br.ReadByte();
                    this.UnkT0A = br.ReadInt16();
                    br.AssertPattern(0x18, 0x00);
                    this.UnkT24 = br.ReadInt32();
                    this.UnkT28 = br.ReadSingle();
                    this.UnkT2C = br.ReadSingle();
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteSingle(this.UnkT00);
                    bw.WriteSingle(this.Compare);
                    bw.WriteByte(this.UnkT08);
                    bw.WriteByte(this.UnkT09);
                    bw.WriteInt16(this.UnkT0A);
                    bw.WritePattern(0x18, 0x00);
                    bw.WriteInt32(this.UnkT24);
                    bw.WriteSingle(this.UnkT28);
                    bw.WriteSingle(this.UnkT2C);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class WindArea : Region {
                private protected override RegionType Type => RegionType.WindArea;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates a WindArea with default values.
                /// </summary>
                public WindArea() : base($"{nameof(Region)}: {nameof(WindArea)}") { }

                internal WindArea(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// An area where sound is muffled.
            /// </summary>
            public class MufflingBox : Region {
                private protected override RegionType Type => RegionType.MufflingBox;
                private protected override bool HasTypeData => true;

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
            /// An entrance to a muffling box.
            /// </summary>
            public class MufflingPortal : Region {
                private protected override RegionType Type => RegionType.MufflingPortal;
                private protected override bool HasTypeData => true;

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
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.UnkT00);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class SoundSpaceOverride : Region {
                private protected override RegionType Type => RegionType.SoundSpaceOverride;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown, probably a soundspace type.
                /// </summary>
                public byte UnkT00 { get; set; }

                /// <summary>
                /// Unknown, probably a soundspace type.
                /// </summary>
                public byte UnkT01 { get; set; }

                /// <summary>
                /// Creates a SoundSpaceOverride with default values.
                /// </summary>
                public SoundSpaceOverride() : base($"{nameof(Region)}: {nameof(SoundSpaceOverride)}") { }

                internal SoundSpaceOverride(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadByte();
                    this.UnkT01 = br.ReadByte();
                    br.AssertPattern(0x1E, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteByte(this.UnkT00);
                    bw.WriteByte(this.UnkT01);
                    bw.WritePattern(0x1E, 0x00);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class MufflingPlane : Region {
                private protected override RegionType Type => RegionType.MufflingPlane;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates a MufflingPlane with default values.
                /// </summary>
                public MufflingPlane() : base($"{nameof(Region)}: {nameof(MufflingPlane)}") { }

                internal MufflingPlane(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class PartsGroupArea : Region {
                private protected override RegionType Type => RegionType.PartsGroupArea;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public long UnkT00 { get; set; }

                /// <summary>
                /// Creates a PartsGroupArea with default values.
                /// </summary>
                public PartsGroupArea() : base($"{nameof(Region)}: {nameof(PartsGroupArea)}") { }

                internal PartsGroupArea(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) => this.UnkT00 = br.ReadInt64();

                private protected override void WriteTypeData(BinaryWriterEx bw) => bw.WriteInt64(this.UnkT00);
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class AutoDrawGroupPoint : Region {
                private protected override RegionType Type => RegionType.AutoDrawGroupPoint;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public long UnkT00 { get; set; }

                /// <summary>
                /// Creates an AutoDrawGroupPoint with default values.
                /// </summary>
                public AutoDrawGroupPoint() : base($"{nameof(Region)}: {nameof(AutoDrawGroupPoint)}") { }

                internal AutoDrawGroupPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadInt64();
                    br.AssertPattern(0x18, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt64(this.UnkT00);
                    bw.WritePattern(0x18, 0x00);
                }
            }

            /// <summary>
            /// Most likely an unused region.
            /// </summary>
            public class Other : Region {
                private protected override RegionType Type => RegionType.Other;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates an Other with default values.
                /// </summary>
                public Other() : base($"{nameof(Region)}: {nameof(Other)}") { }

                internal Other(BinaryReaderEx br) : base(br) { }
            }
        }
    }
}
