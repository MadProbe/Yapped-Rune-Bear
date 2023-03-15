using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using SoulsFormats.Formats.MSB;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MSBB {
        internal enum PartType : uint {
            MapPiece = 0,
            Object = 1,
            Enemy = 2,
            Player = 4,
            Collision = 5,
            Navmesh = 8,
            DummyObject = 9,
            DummyEnemy = 10,
            ConnectCollision = 11,
            Other = 0xFFFFFFFF,
        }

        /// <summary>
        /// All instances of concrete things in the map.
        /// </summary>
        public class PartsParam : Param<Part>, IMsbParam<IMsbPart> {
            internal override int Version => 3;
            internal override string Name => "PARTS_PARAM_ST";

            /// <summary>
            /// All of the fixed visual geometry of the map.
            /// </summary>
            public List<Part.MapPiece> MapPieces { get; set; }

            /// <summary>
            /// Dynamic props and interactive things.
            /// </summary>
            public List<Part.Object> Objects { get; set; }

            /// <summary>
            /// All non-player characters.
            /// </summary>
            public List<Part.Enemy> Enemies { get; set; }

            /// <summary>
            /// These have something to do with player spawn points.
            /// </summary>
            public List<Part.Player> Players { get; set; }

            /// <summary>
            /// Invisible physical geometry of the map.
            /// </summary>
            public List<Part.Collision> Collisions { get; set; }

            /// <summary>
            /// AI navigation meshes.
            /// </summary>
            public List<Part.Navmesh> Navmeshes { get; set; }

            /// <summary>
            /// Objects that don't appear normally; either unused, or used for cutscenes.
            /// </summary>
            public List<Part.DummyObject> DummyObjects { get; set; }

            /// <summary>
            /// Enemies that don't appear normally; either unused, or used for cutscenes.
            /// </summary>
            public List<Part.DummyEnemy> DummyEnemies { get; set; }

            /// <summary>
            /// Dummy parts that reference an actual collision and cause it to load another map.
            /// </summary>
            public List<Part.ConnectCollision> ConnectCollisions { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Part.Other> Others { get; set; }

            /// <summary>
            /// Creates an empty PartsParam.
            /// </summary>
            public PartsParam() : base() {
                this.MapPieces = new List<Part.MapPiece>();
                this.Objects = new List<Part.Object>();
                this.Enemies = new List<Part.Enemy>();
                this.Players = new List<Part.Player>();
                this.Collisions = new List<Part.Collision>();
                this.Navmeshes = new List<Part.Navmesh>();
                this.DummyObjects = new List<Part.DummyObject>();
                this.DummyEnemies = new List<Part.DummyEnemy>();
                this.ConnectCollisions = new List<Part.ConnectCollision>();
                this.Others = new List<Part.Other>();
            }

            /// <summary>
            /// Adds a part to the appropriate list for its type; returns the part.
            /// </summary>
            public Part Add(Part part) {
                switch (part) {
                    case Part.MapPiece p: this.MapPieces.Add(p); break;
                    case Part.Object p: this.Objects.Add(p); break;
                    case Part.Enemy p: this.Enemies.Add(p); break;
                    case Part.Player p: this.Players.Add(p); break;
                    case Part.Collision p: this.Collisions.Add(p); break;
                    case Part.Navmesh p: this.Navmeshes.Add(p); break;
                    case Part.DummyObject p: this.DummyObjects.Add(p); break;
                    case Part.DummyEnemy p: this.DummyEnemies.Add(p); break;
                    case Part.ConnectCollision p: this.ConnectCollisions.Add(p); break;
                    case Part.Other p: this.Others.Add(p); break;

                    default:
                        throw new ArgumentException($"Unrecognized type {part.GetType()}.", nameof(part));
                }
                return part;
            }
            IMsbPart IMsbParam<IMsbPart>.Add(IMsbPart item) => this.Add((Part)item);

            /// <summary>
            /// Returns every Part in the order they'll be written.
            /// </summary>
            public override List<Part> GetEntries() => SFUtil.ConcatAll<Part>(
                    this.MapPieces, this.Objects, this.Enemies, this.Players, this.Collisions,
                    this.Navmeshes, this.DummyObjects, this.DummyEnemies, this.ConnectCollisions, this.Others);
            IReadOnlyList<IMsbPart> IMsbParam<IMsbPart>.GetEntries() => this.GetEntries();

            internal override Part ReadEntry(BinaryReaderEx br) {
                PartType type = br.GetEnum32<PartType>(br.Position + 0x14);
                return type switch {
                    PartType.MapPiece => this.MapPieces.EchoAdd(new Part.MapPiece(br)),
                    PartType.Object => this.Objects.EchoAdd(new Part.Object(br)),
                    PartType.Enemy => this.Enemies.EchoAdd(new Part.Enemy(br)),
                    PartType.Player => this.Players.EchoAdd(new Part.Player(br)),
                    PartType.Collision => this.Collisions.EchoAdd(new Part.Collision(br)),
                    PartType.Navmesh => this.Navmeshes.EchoAdd(new Part.Navmesh(br)),
                    PartType.DummyObject => this.DummyObjects.EchoAdd(new Part.DummyObject(br)),
                    PartType.DummyEnemy => this.DummyEnemies.EchoAdd(new Part.DummyEnemy(br)),
                    PartType.ConnectCollision => this.ConnectCollisions.EchoAdd(new Part.ConnectCollision(br)),
                    PartType.Other => this.Others.EchoAdd(new Part.Other(br)),
                    _ => throw new NotImplementedException($"Unimplemented part type: {type}"),
                };
            }
        }

        /// <summary>
        /// Common information for all concrete entities.
        /// </summary>
        public abstract class Part : Entry, IMsbPart {
            private protected abstract PartType Type { get; }
            private protected abstract bool HasTypeData { get; }
            private protected abstract bool HasGparamConfig { get; }
            private protected abstract bool HasSceneGparamConfig { get; }

            /// <summary>
            /// A description of the part, usually left blank.
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// The name of the part.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Unknown; appears to count up with each instance of a model that was added.
            /// </summary>
            public int InstanceID { get; set; }

            /// <summary>
            /// The model of the Part, corresponding to an entry in the ModelParam.
            /// </summary>
            public string ModelName { get; set; }
            private int ModelIndex;

            /// <summary>
            /// A path to a .sib file, presumed to be some kind of editor placeholder.
            /// </summary>
            public string SibPath { get; set; }

            /// <summary>
            /// Location of the part.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Rotation of the part, in degrees.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// Scale of the part, only meaningful for map pieces and objects.
            /// </summary>
            public Vector3 Scale { get; set; }

            /// <summary>
            /// Controls when the part is visible.
            /// </summary>
            public uint[] DrawGroups { get; private set; }

            /// <summary>
            /// Controls when the part is visible.
            /// </summary>
            public uint[] DispGroups { get; private set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public uint[] BackreadGroups { get; private set; }

            /// <summary>
            /// Identifies the part in external files.
            /// </summary>
            public int EntityID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE04 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE05 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE06 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE07 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte LanternID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte LodParamID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE0E { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE0F { get; set; }

            private protected Part(string name) {
                this.Description = "";
                this.Name = name;
                this.SibPath = "";
                this.Scale = Vector3.One;
                this.DrawGroups = new uint[8] {
                    0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
                this.DispGroups = new uint[8] {
                    0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
                this.BackreadGroups = new uint[8] {
                    0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
                this.EntityID = -1;
            }

            /// <summary>
            /// Creates a deep copy of the part.
            /// </summary>
            public Part DeepCopy() {
                var part = (Part)this.MemberwiseClone();
                part.DrawGroups = (uint[])this.DrawGroups.Clone();
                part.DispGroups = (uint[])this.DispGroups.Clone();
                part.BackreadGroups = (uint[])this.BackreadGroups.Clone();
                this.DeepCopyTo(part);
                return part;
            }
            IMsbPart IMsbPart.DeepCopy() => this.DeepCopy();

            private protected virtual void DeepCopyTo(Part part) { }

            private protected Part(BinaryReaderEx br) {
                long start = br.Position;
                long descOffset = br.ReadInt64();
                long nameOffset = br.ReadInt64();
                this.InstanceID = br.ReadInt32();
                _ = br.AssertUInt32((uint)this.Type);
                _ = br.ReadInt32(); // ID
                this.ModelIndex = br.ReadInt32();
                long sibOffset = br.ReadInt64();
                this.Position = br.ReadVector3();
                this.Rotation = br.ReadVector3();
                this.Scale = br.ReadVector3();
                this.DrawGroups = br.ReadUInt32s(8);
                this.DispGroups = br.ReadUInt32s(8);
                this.BackreadGroups = br.ReadUInt32s(8);
                _ = br.AssertInt32(0);
                long entityDataOffset = br.ReadInt64();
                long typeDataOffset = br.ReadInt64();
                long gparamOffset = br.ReadInt64();
                long sceneGparamOffset = br.ReadInt64();

                if (descOffset == 0) {
                    throw new InvalidDataException($"{nameof(descOffset)} must not be 0 in type {this.GetType()}.");
                }

                if (nameOffset == 0) {
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {this.GetType()}.");
                }

                if (sibOffset == 0) {
                    throw new InvalidDataException($"{nameof(sibOffset)} must not be 0 in type {this.GetType()}.");
                }

                if (entityDataOffset == 0) {
                    throw new InvalidDataException($"{nameof(entityDataOffset)} must not be 0 in type {this.GetType()}.");
                }

                if (this.HasTypeData ^ typeDataOffset != 0) {
                    throw new InvalidDataException($"Unexpected {nameof(typeDataOffset)} 0x{typeDataOffset:X} in type {this.GetType()}.");
                }

                if (this.HasGparamConfig ^ gparamOffset != 0) {
                    throw new InvalidDataException($"Unexpected {nameof(gparamOffset)} 0x{gparamOffset:X} in type {this.GetType()}.");
                }

                if (this.HasSceneGparamConfig ^ sceneGparamOffset != 0) {
                    throw new InvalidDataException($"Unexpected {nameof(sceneGparamOffset)} 0x{sceneGparamOffset:X} in type {this.GetType()}.");
                }

                br.Position = start + descOffset;
                this.Description = br.ReadUTF16();

                br.Position = start + nameOffset;
                this.Name = br.ReadUTF16();

                br.Position = start + sibOffset;
                this.SibPath = br.ReadUTF16();

                br.Position = start + entityDataOffset;
                this.ReadEntityData(br);

                if (this.HasTypeData) {
                    br.Position = start + typeDataOffset;
                    this.ReadTypeData(br);
                }

                if (this.HasGparamConfig) {
                    br.Position = start + gparamOffset;
                    this.ReadGparamConfig(br);
                }

                if (this.HasSceneGparamConfig) {
                    br.Position = start + sceneGparamOffset;
                    this.ReadSceneGparamConfig(br);
                }
            }

            private void ReadEntityData(BinaryReaderEx br) {
                this.EntityID = br.ReadInt32();
                this.UnkE04 = br.ReadByte();
                this.UnkE05 = br.ReadByte();
                this.UnkE06 = br.ReadByte();
                this.UnkE07 = br.ReadByte();
                _ = br.AssertInt32(0);
                this.LanternID = br.ReadByte();
                this.LodParamID = br.ReadByte();
                this.UnkE0E = br.ReadByte();
                this.UnkE0F = br.ReadByte();
            }

            private protected virtual void ReadTypeData(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(ReadTypeData)}.");

            private protected virtual void ReadGparamConfig(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(ReadGparamConfig)}.");

            private protected virtual void ReadSceneGparamConfig(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(ReadSceneGparamConfig)}.");

            internal override void Write(BinaryWriterEx bw, int id) {
                long start = bw.Position;
                bw.ReserveInt64("DescOffset");
                bw.ReserveInt64("NameOffset");
                bw.WriteInt32(this.InstanceID);
                bw.WriteUInt32((uint)this.Type);
                bw.WriteInt32(this.Type == PartType.Other ? 0 : id);
                bw.WriteInt32(this.ModelIndex);
                bw.ReserveInt64("SibOffset");
                bw.WriteVector3(this.Position);
                bw.WriteVector3(this.Rotation);
                bw.WriteVector3(this.Scale);
                bw.WriteUInt32s(this.DrawGroups);
                bw.WriteUInt32s(this.DispGroups);
                bw.WriteUInt32s(this.BackreadGroups);
                bw.WriteInt32(0);
                bw.ReserveInt64("EntityDataOffset");
                bw.ReserveInt64("TypeDataOffset");
                bw.ReserveInt64("GparamOffset");
                bw.ReserveInt64("SceneGparamOffset");

                long stringsStart = bw.Position;
                bw.FillInt64("DescOffset", bw.Position - start);
                bw.WriteUTF16(this.Description, true);

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(MSB.ReambiguateName(this.Name), true);

                bw.FillInt64("SibOffset", bw.Position - start);
                bw.WriteUTF16(this.SibPath, true);
                if (bw.Position - stringsStart <= 0x38) {
                    bw.WritePattern(0x3C - (int)(bw.Position - stringsStart), 0x00);
                } else {
                    bw.Pad(8);
                }

                bw.FillInt64("EntityDataOffset", bw.Position - start);
                this.WriteEntityData(bw);
                if (this.Type != PartType.MapPiece) {
                    bw.Pad(8);
                }

                if (this.HasTypeData) {
                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                    this.WriteTypeData(bw);
                } else {
                    bw.FillInt64("TypeDataOffset", 0);
                }
                bw.Pad(8);

                if (this.HasGparamConfig) {
                    bw.FillInt64("GparamOffset", bw.Position - start);
                    this.WriteGparamConfig(bw);
                } else {
                    bw.FillInt64("GparamOffset", 0);
                }

                if (this.HasSceneGparamConfig) {
                    bw.FillInt64("SceneGparamOffset", bw.Position - start);
                    this.WriteSceneGparamConfig(bw);
                } else {
                    bw.FillInt64("SceneGparamOffset", 0);
                }
            }

            private void WriteEntityData(BinaryWriterEx bw) {
                bw.WriteInt32(this.EntityID);
                bw.WriteByte(this.UnkE04);
                bw.WriteByte(this.UnkE05);
                bw.WriteByte(this.UnkE06);
                bw.WriteByte(this.UnkE07);
                bw.WriteInt32(0);
                bw.WriteByte(this.LanternID);
                bw.WriteByte(this.LodParamID);
                bw.WriteByte(this.UnkE0E);
                bw.WriteByte(this.UnkE0F);
            }

            private protected virtual void WriteTypeData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(WriteTypeData)}.");

            private protected virtual void WriteGparamConfig(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(WriteGparamConfig)}.");

            private protected virtual void WriteSceneGparamConfig(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(WriteSceneGparamConfig)}.");

            internal virtual void GetNames(MSBB msb, Entries entries) => this.ModelName = MSB.FindName(entries.Models, this.ModelIndex);

            internal virtual void GetIndices(MSBB msb, Entries entries) => this.ModelIndex = MSB.FindIndex(entries.Models, this.ModelName);

            /// <summary>
            /// Returns a string representation of the part.
            /// </summary>
            public override string ToString() => this.Description == "" ? $"{this.Type} {this.Name}" : $"{this.Type} {this.Name} - {this.Description}";

            /// <summary>
            /// Gparam value IDs for various part types.
            /// </summary>
            public class GparamConfig {
                /// <summary>
                /// ID of the value set from LightSet ParamEditor to use.
                /// </summary>
                public int LightSetID { get; set; }

                /// <summary>
                /// ID of the value set from FogParamEditor to use.
                /// </summary>
                public int FogParamID { get; set; }

                /// <summary>
                /// ID of the value set from LightScattering : ParamEditor to use.
                /// </summary>
                public int LightScatteringID { get; set; }

                /// <summary>
                /// ID of the value set from Env Map:Editor to use.
                /// </summary>
                public int EnvMapID { get; set; }

                /// <summary>
                /// Creates a GparamConfig with default values.
                /// </summary>
                public GparamConfig() { }

                /// <summary>
                /// Creates a deep copy of the gparam config.
                /// </summary>
                public GparamConfig DeepCopy() => (GparamConfig)this.MemberwiseClone();

                internal GparamConfig(BinaryReaderEx br) {
                    this.LightSetID = br.ReadInt32();
                    this.FogParamID = br.ReadInt32();
                    this.LightScatteringID = br.ReadInt32();
                    this.EnvMapID = br.ReadInt32();
                    br.AssertPattern(0x10, 0x00);
                }

                internal void Write(BinaryWriterEx bw) {
                    bw.WriteInt32(this.LightSetID);
                    bw.WriteInt32(this.FogParamID);
                    bw.WriteInt32(this.LightScatteringID);
                    bw.WriteInt32(this.EnvMapID);
                    bw.WritePattern(0x10, 0x00);
                }

                /// <summary>
                /// Returns the four gparam values as a string.
                /// </summary>
                public override string ToString() => $"{this.LightSetID}, {this.FogParamID}, {this.LightScatteringID}, {this.EnvMapID}";
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class SceneGparamConfig {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte[] EventIDs { get; private set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk40 { get; set; }

                /// <summary>
                /// Creates a SceneGparamConfig with default values.
                /// </summary>
                public SceneGparamConfig() => this.EventIDs = new sbyte[4];

                /// <summary>
                /// Creates a deep copy of the scene gparam config.
                /// </summary>
                public SceneGparamConfig DeepCopy() {
                    var config = (SceneGparamConfig)this.MemberwiseClone();
                    config.EventIDs = (sbyte[])this.EventIDs.Clone();
                    return config;
                }

                internal SceneGparamConfig(BinaryReaderEx br) {
                    this.Unk00 = br.ReadInt32();
                    this.Unk04 = br.ReadInt32();
                    this.Unk08 = br.ReadInt32();
                    this.Unk0C = br.ReadInt32();
                    this.Unk10 = br.ReadInt32();
                    this.Unk14 = br.ReadInt32();
                    br.AssertPattern(0x24, 0x00);
                    this.EventIDs = br.ReadSBytes(4);
                    this.Unk40 = br.ReadSingle();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal void Write(BinaryWriterEx bw) {
                    bw.WriteInt32(this.Unk00);
                    bw.WriteInt32(this.Unk04);
                    bw.WriteInt32(this.Unk08);
                    bw.WriteInt32(this.Unk0C);
                    bw.WriteInt32(this.Unk10);
                    bw.WriteInt32(this.Unk14);
                    bw.WritePattern(0x24, 0x00);
                    bw.WriteSBytes(this.EventIDs);
                    bw.WriteSingle(this.Unk40);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// A visible but not physical model making up the map.
            /// </summary>
            public class MapPiece : Part {
                private protected override PartType Type => PartType.MapPiece;
                private protected override bool HasTypeData => true;
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => false;

                /// <summary>
                /// Gparam IDs for this map piece.
                /// </summary>
                public GparamConfig Gparam { get; set; }

                /// <summary>
                /// Creates a MapPiece with default values.
                /// </summary>
                public MapPiece() : base("mXXXXXX_XXXX") => this.Gparam = new GparamConfig();

                private protected override void DeepCopyTo(Part part) {
                    var piece = (MapPiece)part;
                    piece.Gparam = this.Gparam.DeepCopy();
                }

                internal MapPiece(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                private protected override void ReadGparamConfig(BinaryReaderEx br) => this.Gparam = new GparamConfig(br);

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                private protected override void WriteGparamConfig(BinaryWriterEx bw) => this.Gparam.Write(bw);
            }

            /// <summary>
            /// Common base data for objects and dummy objects.
            /// </summary>
            public abstract class ObjectBase : Part {
                private protected override bool HasTypeData => true;
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => false;

                /// <summary>
                /// Gparam IDs for this object.
                /// </summary>
                public GparamConfig Gparam { get; set; }

                /// <summary>
                /// Collision that controls loading of the object.
                /// </summary>
                public string CollisionName { get; set; }
                private int CollisionIndex;

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte BreakTerm { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte NetSyncType { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool CollisionFilter { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool SetMainObjStructureBooleans { get; set; }

                /// <summary>
                /// Automatically playing animations; only the first is actually used, according to Pav.
                /// </summary>
                public short[] AnimIDs { get; private set; }

                /// <summary>
                /// Value added to the base ModelSfxParam ID; only the first is actually used, according to Pav.
                /// </summary>
                public short[] ModelSfxParamRelativeIDs { get; private set; }

                private protected ObjectBase() : base("oXXXXXX_XXXX") {
                    this.Gparam = new GparamConfig();
                    this.AnimIDs = new short[4] { -1, -1, -1, -1 };
                    this.ModelSfxParamRelativeIDs = new short[4];
                }

                private protected override void DeepCopyTo(Part part) {
                    var obj = (ObjectBase)part;
                    obj.Gparam = this.Gparam.DeepCopy();
                    obj.AnimIDs = (short[])this.AnimIDs.Clone();
                    obj.ModelSfxParamRelativeIDs = (short[])this.ModelSfxParamRelativeIDs.Clone();
                }

                private protected ObjectBase(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    this.CollisionIndex = br.ReadInt32();
                    this.BreakTerm = br.ReadSByte();
                    this.NetSyncType = br.ReadSByte();
                    this.CollisionFilter = br.ReadBoolean();
                    this.SetMainObjStructureBooleans = br.ReadBoolean();
                    this.AnimIDs = br.ReadInt16s(4);
                    this.ModelSfxParamRelativeIDs = br.ReadInt16s(4);
                }

                private protected override void ReadGparamConfig(BinaryReaderEx br) => this.Gparam = new GparamConfig(br);

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(this.CollisionIndex);
                    bw.WriteSByte(this.BreakTerm);
                    bw.WriteSByte(this.NetSyncType);
                    bw.WriteBoolean(this.CollisionFilter);
                    bw.WriteBoolean(this.SetMainObjStructureBooleans);
                    bw.WriteInt16s(this.AnimIDs);
                    bw.WriteInt16s(this.ModelSfxParamRelativeIDs);
                }

                private protected override void WriteGparamConfig(BinaryWriterEx bw) => this.Gparam.Write(bw);

                internal override void GetNames(MSBB msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.CollisionName = MSB.FindName(entries.Parts, this.CollisionIndex);
                }

                internal override void GetIndices(MSBB msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.CollisionIndex = MSB.FindIndex(entries.Parts, this.CollisionName);
                }
            }

            /// <summary>
            /// A dynamic or interactible part of the map.
            /// </summary>
            public class Object : ObjectBase {
                private protected override PartType Type => PartType.Object;

                /// <summary>
                /// Creates an Object with default values.
                /// </summary>
                public Object() : base() { }

                internal Object(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Common base data for enemies and dummy enemies.
            /// </summary>
            public abstract class EnemyBase : Part {
                private protected override bool HasTypeData => true;
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => false;

                /// <summary>
                /// Gparam IDs for this enemy.
                /// </summary>
                public GparamConfig Gparam { get; set; }

                /// <summary>
                /// ID in NPCThinkParam determining AI properties.
                /// </summary>
                public int ThinkParamID { get; set; }

                /// <summary>
                /// ID in NPCParam determining character properties.
                /// </summary>
                public int NPCParamID { get; set; }

                /// <summary>
                /// ID of a talk ESD used by the character.
                /// </summary>
                public int TalkID { get; set; }

                /// <summary>
                /// ID in CharaInitParam determining equipment and stats for humans.
                /// </summary>
                public int CharaInitID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT18 { get; set; }

                /// <summary>
                /// Collision that controls loading of the enemy.
                /// </summary>
                public string CollisionName { get; set; }
                private int CollisionIndex;

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT20 { get; set; }

                /// <summary>
                /// Regions for the enemy to patrol.
                /// </summary>
                public string[] MovePointNames { get; private set; }
                private short[] MovePointIndices;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int InitAnimID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int DamageAnimID { get; set; }

                private protected EnemyBase() : base("cXXXX_XXXX") {
                    this.Gparam = new GparamConfig();
                    this.ThinkParamID = -1;
                    this.NPCParamID = -1;
                    this.TalkID = -1;
                    this.CharaInitID = -1;
                    this.MovePointNames = new string[8];
                }

                private protected override void DeepCopyTo(Part part) {
                    var enemy = (EnemyBase)part;
                    enemy.Gparam = this.Gparam.DeepCopy();
                    enemy.MovePointNames = (string[])this.MovePointNames.Clone();
                }

                private protected EnemyBase(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    this.ThinkParamID = br.ReadInt32();
                    this.NPCParamID = br.ReadInt32();
                    this.TalkID = br.ReadInt32();
                    this.CharaInitID = br.ReadInt32();
                    this.UnkT18 = br.ReadInt32();
                    this.CollisionIndex = br.ReadInt32();
                    this.UnkT20 = br.ReadInt16();
                    _ = br.AssertInt16(0);
                    _ = br.AssertInt32(0);
                    this.MovePointIndices = br.ReadInt16s(8);
                    this.InitAnimID = br.ReadInt32();
                    this.DamageAnimID = br.ReadInt32();
                }

                private protected override void ReadGparamConfig(BinaryReaderEx br) => this.Gparam = new GparamConfig(br);

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(this.ThinkParamID);
                    bw.WriteInt32(this.NPCParamID);
                    bw.WriteInt32(this.TalkID);
                    bw.WriteInt32(this.CharaInitID);
                    bw.WriteInt32(this.UnkT18);
                    bw.WriteInt32(this.CollisionIndex);
                    bw.WriteInt16(this.UnkT20);
                    bw.WriteInt16(0);
                    bw.WriteInt32(0);
                    bw.WriteInt16s(this.MovePointIndices);
                    bw.WriteInt32(this.InitAnimID);
                    bw.WriteInt32(this.DamageAnimID);
                }

                private protected override void WriteGparamConfig(BinaryWriterEx bw) => this.Gparam.Write(bw);

                internal override void GetNames(MSBB msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.CollisionName = MSB.FindName(entries.Parts, this.CollisionIndex);

                    this.MovePointNames = new string[this.MovePointIndices.Length];
                    for (int i = 0; i < this.MovePointIndices.Length; i++) {
                        this.MovePointNames[i] = MSB.FindName(entries.Regions, this.MovePointIndices[i]);
                    }
                }

                internal override void GetIndices(MSBB msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.CollisionIndex = MSB.FindIndex(entries.Parts, this.CollisionName);

                    this.MovePointIndices = new short[this.MovePointNames.Length];
                    for (int i = 0; i < this.MovePointNames.Length; i++) {
                        this.MovePointIndices[i] = (short)MSB.FindIndex(entries.Regions, this.MovePointNames[i]);
                    }
                }
            }

            /// <summary>
            /// Any living entity besides the player character.
            /// </summary>
            public class Enemy : EnemyBase {
                private protected override PartType Type => PartType.Enemy;

                /// <summary>
                /// Creates an Enemy with default values.
                /// </summary>
                public Enemy() : base() { }

                internal Enemy(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unknown exactly what these do.
            /// </summary>
            public class Player : Part {
                private protected override PartType Type => PartType.Player;
                private protected override bool HasTypeData => true;
                private protected override bool HasGparamConfig => false;
                private protected override bool HasSceneGparamConfig => false;

                /// <summary>
                /// Creates a Player with default values.
                /// </summary>
                public Player() : base("c0000_XXXX") { }

                internal Player(BinaryReaderEx br) : base(br) { }

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
            /// Invisible but physical geometry.
            /// </summary>
            public class Collision : Part {
                private protected override PartType Type => PartType.Collision;
                private protected override bool HasTypeData => true;
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => true;

                /// <summary>
                /// Gparam IDs for this collision.
                /// </summary>
                public GparamConfig Gparam { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public SceneGparamConfig SceneGparam { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte HitFilterID { get; set; }

                /// <summary>
                /// Causes sounds to be modulated when standing on the collision.
                /// </summary>
                public byte SoundSpaceType { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short EnvLightMapSpotIndex { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float ReflectPlaneHeight { get; set; }

                /// <summary>
                /// Controls displays of the map name on screen or the loading menu.
                /// </summary>
                public short MapNameID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool DisableStart { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT0B { get; set; }

                /// <summary>
                /// If set, disables a bonfire when any enemy is on the collision.
                /// </summary>
                public int DisableBonfireEntityID { get; set; }

                /// <summary>
                /// An ID used for multiplayer eligibility.
                /// </summary>
                public int PlayRegionID { get; set; }

                /// <summary>
                /// ID in LockCamParam determining camera properties.
                /// </summary>
                public short LockCamParamID1 { get; set; }

                /// <summary>
                /// ID in LockCamParam determining camera properties.
                /// </summary>
                public short LockCamParamID2 { get; set; }

                /// <summary>
                /// Creates a Collision with default values.
                /// </summary>
                public Collision() : base("hXXXXXX_XXXX") {
                    this.Gparam = new GparamConfig();
                    this.SceneGparam = new SceneGparamConfig();
                    this.MapNameID = -1;
                    this.DisableBonfireEntityID = -1;
                    this.LockCamParamID1 = -1;
                    this.LockCamParamID2 = -1;
                }

                private protected override void DeepCopyTo(Part part) {
                    var collision = (Collision)part;
                    collision.Gparam = this.Gparam.DeepCopy();
                    collision.SceneGparam = this.SceneGparam.DeepCopy();
                }

                internal Collision(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.HitFilterID = br.ReadByte();
                    this.SoundSpaceType = br.ReadByte();
                    this.EnvLightMapSpotIndex = br.ReadInt16();
                    this.ReflectPlaneHeight = br.ReadSingle();
                    this.MapNameID = br.ReadInt16();
                    this.DisableStart = br.ReadBoolean();
                    this.UnkT0B = br.ReadByte();
                    this.DisableBonfireEntityID = br.ReadInt32();
                    this.PlayRegionID = br.ReadInt32();
                    this.LockCamParamID1 = br.ReadInt16();
                    this.LockCamParamID2 = br.ReadInt16();
                }

                private protected override void ReadGparamConfig(BinaryReaderEx br) => this.Gparam = new GparamConfig(br);
                private protected override void ReadSceneGparamConfig(BinaryReaderEx br) => this.SceneGparam = new SceneGparamConfig(br);

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteByte(this.HitFilterID);
                    bw.WriteByte(this.SoundSpaceType);
                    bw.WriteInt16(this.EnvLightMapSpotIndex);
                    bw.WriteSingle(this.ReflectPlaneHeight);
                    bw.WriteInt16(this.MapNameID);
                    bw.WriteBoolean(this.DisableStart);
                    bw.WriteByte(this.UnkT0B);
                    bw.WriteInt32(this.DisableBonfireEntityID);
                    bw.WriteInt32(this.PlayRegionID);
                    bw.WriteInt16(this.LockCamParamID1);
                    bw.WriteInt16(this.LockCamParamID2);
                }

                private protected override void WriteGparamConfig(BinaryWriterEx bw) => this.Gparam.Write(bw);
                private protected override void WriteSceneGparamConfig(BinaryWriterEx bw) => this.SceneGparam.Write(bw);
            }

            /// <summary>
            /// An AI navigation mesh.
            /// </summary>
            public class Navmesh : Part {
                private protected override PartType Type => PartType.Navmesh;
                private protected override bool HasTypeData => true;
                private protected override bool HasGparamConfig => false;
                private protected override bool HasSceneGparamConfig => false;

                /// <summary>
                /// Creates a Navmesh with default values.
                /// </summary>
                public Navmesh() : base("nXXXXBX") { }

                internal Navmesh(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// A normally invisible object, either unused or for a cutscene.
            /// </summary>
            public class DummyObject : ObjectBase {
                private protected override PartType Type => PartType.DummyObject;

                /// <summary>
                /// Creates a DummyObject with default values.
                /// </summary>
                public DummyObject() : base() { }

                internal DummyObject(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A normally invisible enemy, either unused or for a cutscene.
            /// </summary>
            public class DummyEnemy : EnemyBase {
                private protected override PartType Type => PartType.DummyEnemy;

                /// <summary>
                /// Creates a DummyEnemy with default values.
                /// </summary>
                public DummyEnemy() : base() { }

                internal DummyEnemy(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Attaches to an actual Collision and causes another map to be loaded when standing on it.
            /// </summary>
            public class ConnectCollision : Part {
                private protected override PartType Type => PartType.ConnectCollision;
                private protected override bool HasTypeData => true;
                private protected override bool HasGparamConfig => false;
                private protected override bool HasSceneGparamConfig => false;

                /// <summary>
                /// The collision which will load another map.
                /// </summary>
                public string CollisionName { get; set; }
                private int CollisionIndex;

                /// <summary>
                /// Four bytes specifying the map ID to load.
                /// </summary>
                public byte[] MapID { get; private set; }

                /// <summary>
                /// Creates a ConnectCollision with default values.
                /// </summary>
                public ConnectCollision() : base("hXXXXBX_XXXX") => this.MapID = new byte[4] { 10, 2, 0, 0 };

                private protected override void DeepCopyTo(Part part) {
                    var connect = (ConnectCollision)part;
                    connect.MapID = (byte[])this.MapID.Clone();
                }

                internal ConnectCollision(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.CollisionIndex = br.ReadInt32();
                    this.MapID = br.ReadBytes(4);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.CollisionIndex);
                    bw.WriteBytes(this.MapID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSBB msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.CollisionName = MSB.FindName(msb.Parts.Collisions, this.CollisionIndex);
                }

                internal override void GetIndices(MSBB msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.CollisionIndex = MSB.FindIndex(msb.Parts.Collisions, this.CollisionName);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Other : Part {
                private protected override PartType Type => PartType.Other;
                private protected override bool HasTypeData => false;
                private protected override bool HasGparamConfig => false;
                private protected override bool HasSceneGparamConfig => false;

                /// <summary>
                /// Creates an Other with default values.
                /// </summary>
                // TODO verify this
                public Other() : base("hXXXXBX_XXXX") { }

                internal Other(BinaryReaderEx br) : base(br) { }
            }
        }
    }
}
