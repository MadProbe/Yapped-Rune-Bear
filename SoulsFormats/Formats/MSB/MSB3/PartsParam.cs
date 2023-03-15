using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using SoulsFormats.Formats.MSB;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MSB3 {
        internal enum PartsType : uint {
            MapPiece = 0,
            Object = 1,
            Enemy = 2,
            Item = 3,
            Player = 4,
            Collision = 5,
            NPCWander = 6,
            Protoboss = 7,
            Navmesh = 8,
            DummyObject = 9,
            DummyEnemy = 10,
            ConnectCollision = 11,
        }

        /// <summary>
        /// Instances of various "things" in this MSB.
        /// </summary>
        public class PartsParam : Param<Part>, IMsbParam<IMsbPart> {
            internal override int Version => 3;
            internal override string Type => "PARTS_PARAM_ST";

            /// <summary>
            /// Map pieces in the MSB.
            /// </summary>
            public List<Part.MapPiece> MapPieces { get; set; }

            /// <summary>
            /// Objects in the MSB.
            /// </summary>
            public List<Part.Object> Objects { get; set; }

            /// <summary>
            /// Enemies in the MSB.
            /// </summary>
            public List<Part.Enemy> Enemies { get; set; }

            /// <summary>
            /// Players in the MSB.
            /// </summary>
            public List<Part.Player> Players { get; set; }

            /// <summary>
            /// Collisions in the MSB.
            /// </summary>
            public List<Part.Collision> Collisions { get; set; }

            /// <summary>
            /// Dummy objects in the MSB.
            /// </summary>
            public List<Part.DummyObject> DummyObjects { get; set; }

            /// <summary>
            /// Dummy enemies in the MSB.
            /// </summary>
            public List<Part.DummyEnemy> DummyEnemies { get; set; }

            /// <summary>
            /// Connect collisions in the MSB.
            /// </summary>
            public List<Part.ConnectCollision> ConnectCollisions { get; set; }

            /// <summary>
            /// Creates a new PartsParam with no parts.
            /// </summary>
            public PartsParam() {
                this.MapPieces = new List<Part.MapPiece>();
                this.Objects = new List<Part.Object>();
                this.Enemies = new List<Part.Enemy>();
                this.Players = new List<Part.Player>();
                this.Collisions = new List<Part.Collision>();
                this.DummyObjects = new List<Part.DummyObject>();
                this.DummyEnemies = new List<Part.DummyEnemy>();
                this.ConnectCollisions = new List<Part.ConnectCollision>();
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
                    case Part.DummyObject p: this.DummyObjects.Add(p); break;
                    case Part.DummyEnemy p: this.DummyEnemies.Add(p); break;
                    case Part.ConnectCollision p: this.ConnectCollisions.Add(p); break;

                    default:
                        throw new ArgumentException($"Unrecognized type {part.GetType()}.", nameof(part));
                }
                return part;
            }
            IMsbPart IMsbParam<IMsbPart>.Add(IMsbPart item) => this.Add((Part)item);

            /// <summary>
            /// Returns every part in the order they'll be written.
            /// </summary>
            public override List<Part> GetEntries() => SFUtil.ConcatAll<Part>(
                    this.MapPieces, this.Objects, this.Enemies, this.Players, this.Collisions,
                    this.DummyObjects, this.DummyEnemies, this.ConnectCollisions);
            IReadOnlyList<IMsbPart> IMsbParam<IMsbPart>.GetEntries() => this.GetEntries();

            internal override Part ReadEntry(BinaryReaderEx br) {
                PartsType type = br.GetEnum32<PartsType>(br.Position + 8);
                return type switch {
                    PartsType.MapPiece => this.MapPieces.EchoAdd(new Part.MapPiece(br)),
                    PartsType.Object => this.Objects.EchoAdd(new Part.Object(br)),
                    PartsType.Enemy => this.Enemies.EchoAdd(new Part.Enemy(br)),
                    PartsType.Player => this.Players.EchoAdd(new Part.Player(br)),
                    PartsType.Collision => this.Collisions.EchoAdd(new Part.Collision(br)),
                    PartsType.DummyObject => this.DummyObjects.EchoAdd(new Part.DummyObject(br)),
                    PartsType.DummyEnemy => this.DummyEnemies.EchoAdd(new Part.DummyEnemy(br)),
                    PartsType.ConnectCollision => this.ConnectCollisions.EchoAdd(new Part.ConnectCollision(br)),
                    _ => throw new NotImplementedException($"Unsupported part type: {type}"),
                };
            }
        }

        /// <summary>
        /// Any instance of some "thing" in a map.
        /// </summary>
        public abstract class Part : NamedEntry, IMsbPart {
            private protected abstract PartsType Type { get; }
            private protected abstract bool HasGparamConfig { get; }
            private protected abstract bool HasSceneGparamConfig { get; }

            /// <summary>
            /// The name of this part.
            /// </summary>
            public override string Name { get; set; }

            /// <summary>
            /// Unknown network path to a .sib file.
            /// </summary>
            public string SibPath { get; set; }

            /// <summary>
            /// The name of this part's model.
            /// </summary>
            public string ModelName { get; set; }
            private int ModelIndex;

            /// <summary>
            /// The center of the part.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// The rotation of the part.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// The scale of the part, which only really works right for map pieces.
            /// </summary>
            public Vector3 Scale { get; set; }

            /// <summary>
            /// A bitmask that determines which ceremonies the part appears in.
            /// </summary>
            public uint MapStudioLayer { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public uint[] DrawGroups { get; private set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public uint[] DispGroups { get; private set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public uint[] BackreadGroups { get; private set; }

            /// <summary>
            /// Used to identify the part in event scripts.
            /// </summary>
            public int EntityID { get; set; }

            /// <summary>
            /// Used to identify multiple parts with the same ID in event scripts.
            /// </summary>
            public int[] EntityGroups { get; private set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public sbyte UnkE04 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public sbyte UnkE05 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public sbyte LanternID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public sbyte LodParamID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public sbyte UnkE0E { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool PointLightShadowSource { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool ShadowSource { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool ShadowDest { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool IsShadowOnly { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool DrawByReflectCam { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool DrawOnlyReflectCam { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool UseDepthBiasFloat { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool DisablePointLightEffect { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int UnkE18 { get; set; }

            private protected Part(string name) {
                this.Name = name;
                this.Scale = Vector3.One;
                this.DrawGroups = new uint[8] { 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF,
                    0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
                this.DispGroups = new uint[8] { 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF,
                    0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
                this.BackreadGroups = new uint[8] { 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF,
                    0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
                this.EntityID = -1;
                this.EntityGroups = new int[8] { -1, -1, -1, -1, -1, -1, -1, -1 };
            }

            /// <summary>
            /// Creates a deep copy of the part.
            /// </summary>
            public Part DeepCopy() {
                var part = (Part)this.MemberwiseClone();
                part.DrawGroups = (uint[])this.DrawGroups.Clone();
                part.DispGroups = (uint[])this.DispGroups.Clone();
                part.BackreadGroups = (uint[])this.BackreadGroups.Clone();
                part.EntityGroups = (int[])this.EntityGroups.Clone();
                this.DeepCopyTo(part);
                return part;
            }
            IMsbPart IMsbPart.DeepCopy() => this.DeepCopy();

            private protected virtual void DeepCopyTo(Part part) { }

            private protected Part(BinaryReaderEx br) {
                long start = br.Position;

                long nameOffset = br.ReadInt64();
                _ = br.AssertUInt32((uint)this.Type);
                _ = br.ReadInt32(); // ID
                this.ModelIndex = br.ReadInt32();
                _ = br.AssertInt32(0);
                long sibOffset = br.ReadInt64();
                this.Position = br.ReadVector3();
                this.Rotation = br.ReadVector3();
                this.Scale = br.ReadVector3();

                _ = br.AssertInt32(-1);
                this.MapStudioLayer = br.ReadUInt32();
                this.DrawGroups = br.ReadUInt32s(8);
                this.DispGroups = br.ReadUInt32s(8);
                this.BackreadGroups = br.ReadUInt32s(8);
                _ = br.AssertInt32(0);

                long entityDataOffset = br.ReadInt64();
                long typeDataOffset = br.ReadInt64();
                long gparamOffset = br.ReadInt64();
                long sceneGparamOffset = br.ReadInt64();

                if (nameOffset == 0) {
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {this.GetType()}.");
                }

                if (sibOffset == 0) {
                    throw new InvalidDataException($"{nameof(sibOffset)} must not be 0 in type {this.GetType()}.");
                }

                if (entityDataOffset == 0) {
                    throw new InvalidDataException($"{nameof(entityDataOffset)} must not be 0 in type {this.GetType()}.");
                }

                if (typeDataOffset == 0) {
                    throw new InvalidDataException($"{nameof(typeDataOffset)} must not be 0 in type {this.GetType()}.");
                }

                if (this.HasGparamConfig ^ gparamOffset != 0) {
                    throw new InvalidDataException($"Unexpected {nameof(gparamOffset)} 0x{gparamOffset:X} in type {this.GetType()}.");
                }

                if (this.HasSceneGparamConfig ^ sceneGparamOffset != 0) {
                    throw new InvalidDataException($"Unexpected {nameof(sceneGparamOffset)} 0x{sceneGparamOffset:X} in type {this.GetType()}.");
                }

                br.Position = start + nameOffset;
                this.Name = br.ReadUTF16();

                br.Position = start + sibOffset;
                this.SibPath = br.ReadUTF16();

                br.Position = start + entityDataOffset;
                this.ReadEntityData(br);

                br.Position = start + typeDataOffset;
                this.ReadTypeData(br);

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
                this.UnkE04 = br.ReadSByte();
                this.UnkE05 = br.ReadSByte();
                _ = br.AssertInt16(0);
                _ = br.AssertInt32(0);
                this.LanternID = br.ReadSByte();
                this.LodParamID = br.ReadSByte();
                this.UnkE0E = br.ReadSByte();
                this.PointLightShadowSource = br.ReadBoolean();
                this.ShadowSource = br.ReadBoolean();
                this.ShadowDest = br.ReadBoolean();
                this.IsShadowOnly = br.ReadBoolean();
                this.DrawByReflectCam = br.ReadBoolean();
                this.DrawOnlyReflectCam = br.ReadBoolean();
                this.UseDepthBiasFloat = br.ReadBoolean();
                this.DisablePointLightEffect = br.ReadBoolean();
                _ = br.AssertByte(0);
                this.UnkE18 = br.ReadInt32();
                this.EntityGroups = br.ReadInt32s(8);
                _ = br.AssertInt32(0);
            }

            private protected abstract void ReadTypeData(BinaryReaderEx br);

            private protected virtual void ReadGparamConfig(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(ReadGparamConfig)}.");

            private protected virtual void ReadSceneGparamConfig(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(ReadSceneGparamConfig)}.");

            internal override void Write(BinaryWriterEx bw, int id) {
                long start = bw.Position;

                bw.ReserveInt64("NameOffset");
                bw.WriteUInt32((uint)this.Type);
                bw.WriteInt32(id);
                bw.WriteInt32(this.ModelIndex);
                bw.WriteInt32(0);
                bw.ReserveInt64("SibOffset");
                bw.WriteVector3(this.Position);
                bw.WriteVector3(this.Rotation);
                bw.WriteVector3(this.Scale);

                bw.WriteInt32(-1);
                bw.WriteUInt32(this.MapStudioLayer);
                bw.WriteUInt32s(this.DrawGroups);
                bw.WriteUInt32s(this.DispGroups);
                bw.WriteUInt32s(this.BackreadGroups);
                bw.WriteInt32(0);

                bw.ReserveInt64("EntityDataOffset");
                bw.ReserveInt64("TypeDataOffset");
                bw.ReserveInt64("GparamOffset");
                bw.ReserveInt64("SceneGparamOffset");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(MSB.ReambiguateName(this.Name), true);

                bw.FillInt64("SibOffset", bw.Position - start);
                bw.WriteUTF16(this.SibPath, true);
                // This is purely here for byte-perfect writes because From is nasty
                if (this.SibPath == "") {
                    bw.WritePattern(0x24, 0x00);
                }

                bw.Pad(8);

                bw.FillInt64("EntityDataOffset", bw.Position - start);
                this.WriteEntityData(bw);

                bw.FillInt64("TypeDataOffset", bw.Position - start);
                this.WriteTypeData(bw);

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

                bw.WriteSByte(this.UnkE04);
                bw.WriteSByte(this.UnkE05);
                bw.WriteInt16(0);

                bw.WriteInt32(0);

                bw.WriteSByte(this.LanternID);
                bw.WriteSByte(this.LodParamID);
                bw.WriteSByte(this.UnkE0E);
                bw.WriteBoolean(this.PointLightShadowSource);

                bw.WriteBoolean(this.ShadowSource);
                bw.WriteBoolean(this.ShadowDest);
                bw.WriteBoolean(this.IsShadowOnly);
                bw.WriteBoolean(this.DrawByReflectCam);

                bw.WriteBoolean(this.DrawOnlyReflectCam);
                bw.WriteBoolean(this.UseDepthBiasFloat);
                bw.WriteBoolean(this.DisablePointLightEffect);
                bw.WriteByte(0);

                bw.WriteInt32(this.UnkE18);
                bw.WriteInt32s(this.EntityGroups);
                bw.WriteInt32(0);
                bw.Pad(8);
            }

            private protected abstract void WriteTypeData(BinaryWriterEx bw);

            private protected virtual void WriteGparamConfig(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(WriteGparamConfig)}.");

            private protected virtual void WriteSceneGparamConfig(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(WriteSceneGparamConfig)}.");

            internal virtual void GetNames(MSB3 msb, Entries entries) => this.ModelName = MSB.FindName(entries.Models, this.ModelIndex);

            internal virtual void GetIndices(MSB3 msb, Entries entries) => this.ModelIndex = MSB.FindIndex(entries.Models, this.ModelName);

            /// <summary>
            /// Returns the type and name of this part.
            /// </summary>
            public override string ToString() => $"{this.Type} : {this.Name}";

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
                    br.AssertPattern(0x3C, 0x00);
                    this.EventIDs = br.ReadSBytes(4);
                    this.Unk40 = br.ReadSingle();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                internal void Write(BinaryWriterEx bw) {
                    bw.WritePattern(0x3C, 0x00);
                    bw.WriteSBytes(this.EventIDs);
                    bw.WriteSingle(this.Unk40);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// A static model making up the map.
            /// </summary>
            public class MapPiece : Part {
                private protected override PartsType Type => PartsType.MapPiece;
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
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => false;

                /// <summary>
                /// Gparam IDs for this object.
                /// </summary>
                public GparamConfig Gparam { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public string CollisionName { get; set; }
                private int CollisionPartIndex;

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte BreakTerm { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool NetSyncType { get; set; }

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
                    this.ModelSfxParamRelativeIDs = new short[4] { -1, -1, -1, -1 };
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
                    this.CollisionPartIndex = br.ReadInt32();
                    this.BreakTerm = br.ReadByte();
                    this.NetSyncType = br.ReadBoolean();
                    this.CollisionFilter = br.ReadBoolean();
                    this.SetMainObjStructureBooleans = br.ReadBoolean();
                    this.AnimIDs = br.ReadInt16s(4);
                    this.ModelSfxParamRelativeIDs = br.ReadInt16s(4);
                }

                private protected override void ReadGparamConfig(BinaryReaderEx br) => this.Gparam = new GparamConfig(br);

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(this.CollisionPartIndex);
                    bw.WriteByte(this.BreakTerm);
                    bw.WriteBoolean(this.NetSyncType);
                    bw.WriteBoolean(this.CollisionFilter);
                    bw.WriteBoolean(this.SetMainObjStructureBooleans);
                    bw.WriteInt16s(this.AnimIDs);
                    bw.WriteInt16s(this.ModelSfxParamRelativeIDs);
                }

                private protected override void WriteGparamConfig(BinaryWriterEx bw) => this.Gparam.Write(bw);

                internal override void GetNames(MSB3 msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.CollisionName = MSB.FindName(entries.Parts, this.CollisionPartIndex);
                }

                internal override void GetIndices(MSB3 msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.CollisionPartIndex = MSB.FindIndex(entries.Parts, this.CollisionName);
                }
            }

            /// <summary>
            /// Any dynamic object such as elevators, crates, ladders, etc.
            /// </summary>
            public class Object : ObjectBase {
                private protected override PartsType Type => PartsType.Object;

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
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => false;

                /// <summary>
                /// Gparam IDs for this enemy.
                /// </summary>
                public GparamConfig Gparam { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public string CollisionName { get; set; }
                private int CollisionPartIndex;

                /// <summary>
                /// Controls enemy AI.
                /// </summary>
                public int ThinkParamID { get; set; }

                /// <summary>
                /// Controls enemy stats.
                /// </summary>
                public int NPCParamID { get; set; }

                /// <summary>
                /// Controls enemy speech.
                /// </summary>
                public int TalkID { get; set; }

                /// <summary>
                /// Controls enemy equipment.
                /// </summary>
                public int CharaInitID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte PointMoveType { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short PlatoonID { get; set; }

                /// <summary>
                /// Walk route followed by this enemy.
                /// </summary>
                public string WalkRouteName { get; set; }
                private short WalkRouteIndex;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int BackupEventAnimID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT78 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT84 { get; set; }

                private protected EnemyBase() : base("cXXXX_XXXX") => this.Gparam = new GparamConfig();

                private protected override void DeepCopyTo(Part part) {
                    var enemy = (EnemyBase)part;
                    enemy.Gparam = this.Gparam.DeepCopy();
                }

                private protected EnemyBase(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    this.ThinkParamID = br.ReadInt32();
                    this.NPCParamID = br.ReadInt32();
                    this.TalkID = br.ReadInt32();
                    this.PointMoveType = br.ReadByte();
                    _ = br.AssertByte(0);
                    this.PlatoonID = br.ReadInt16();
                    this.CharaInitID = br.ReadInt32();
                    this.CollisionPartIndex = br.ReadInt32();
                    this.WalkRouteIndex = br.ReadInt16();
                    _ = br.AssertInt16(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(-1);
                    _ = br.AssertInt32(-1);
                    _ = br.AssertInt32(-1);
                    _ = br.AssertInt32(-1);
                    this.BackupEventAnimID = br.ReadInt32();
                    _ = br.AssertInt32(-1); // BackupThrowAnimID
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    this.UnkT78 = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    this.UnkT84 = br.ReadSingle();
                    for (int i = 0; i < 5; i++) {
                        _ = br.AssertInt32(-1);
                        _ = br.AssertInt16(-1);
                        _ = br.AssertInt16(0xA);
                    }
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                private protected override void ReadGparamConfig(BinaryReaderEx br) => this.Gparam = new GparamConfig(br);

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(this.ThinkParamID);
                    bw.WriteInt32(this.NPCParamID);
                    bw.WriteInt32(this.TalkID);
                    bw.WriteByte(this.PointMoveType);
                    bw.WriteByte(0);
                    bw.WriteInt16(this.PlatoonID);
                    bw.WriteInt32(this.CharaInitID);
                    bw.WriteInt32(this.CollisionPartIndex);
                    bw.WriteInt16(this.WalkRouteIndex);
                    bw.WriteInt16(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(this.BackupEventAnimID);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(this.UnkT78);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteSingle(this.UnkT84);
                    for (int i = 0; i < 5; i++) {
                        bw.WriteInt32(-1);
                        bw.WriteInt16(-1);
                        bw.WriteInt16(0xA);
                    }
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                private protected override void WriteGparamConfig(BinaryWriterEx bw) => this.Gparam.Write(bw);

                internal override void GetNames(MSB3 msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.CollisionName = MSB.FindName(entries.Parts, this.CollisionPartIndex);
                    this.WalkRouteName = MSB.FindName(msb.Events.PatrolInfo, this.WalkRouteIndex);
                }

                internal override void GetIndices(MSB3 msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.CollisionPartIndex = MSB.FindIndex(entries.Parts, this.CollisionName);
                    this.WalkRouteIndex = (short)MSB.FindIndex(msb.Events.PatrolInfo, this.WalkRouteName);
                }
            }

            /// <summary>
            /// Any non-player character, not necessarily hostile.
            /// </summary>
            public class Enemy : EnemyBase {
                private protected override PartsType Type => PartsType.Enemy;

                /// <summary>
                /// Creates an Enemy with default values.
                /// </summary>
                public Enemy() : base() { }

                internal Enemy(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A player spawn point.
            /// </summary>
            public class Player : Part {
                private protected override PartsType Type => PartsType.Player;
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
            /// An invisible collision mesh, also used for death planes.
            /// </summary>
            public class Collision : Part {
                /// <summary>
                /// Amount of reverb to apply to sounds.
                /// </summary>
                public enum SoundSpace : byte {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
                    NoReverb = 0,
                    SmallReverbA = 1,
                    SmallReverbB = 2,
                    MiddleReverbA = 3,
                    MiddleReverbB = 4,
                    LargeReverbA = 5,
                    LargeReverbB = 6,
                    ExtraLargeReverbA = 7,
                    ExtraLargeReverbB = 8,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
                }

                /// <summary>
                /// Unknown.
                /// </summary>
                public enum MapVisiblity : byte {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
                    Good = 0,
                    Dark = 1,
                    PitchDark = 2,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
                }

                private protected override PartsType Type => PartsType.Collision;
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
                /// Modifies sounds while the player is touching this collision.
                /// </summary>
                public SoundSpace SoundSpaceType { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short EnvLightMapSpotIndex { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float ReflectPlaneHeight { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short MapNameID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool DisableStart { get; set; }

                /// <summary>
                /// Disables a bonfire with this entity ID when an enemy is touching this collision.
                /// </summary>
                public int DisableBonfireEntityID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int PlayRegionID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short LockCamID1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short LockCamID2 { get; set; }

                /// <summary>
                /// Unknown. Always refers to another collision part.
                /// </summary>
                public string UnkHitName { get; set; }
                private int UnkHitIndex;

                /// <summary>
                /// ID in MapMimicryEstablishmentParam.
                /// </summary>
                public int ChameleonParamID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT34 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT35 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT36 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public MapVisiblity MapVisType { get; set; }

                /// <summary>
                /// Creates a Collision with default values.
                /// </summary>
                public Collision() : base("hXXXXXX") {
                    this.Gparam = new GparamConfig();
                    this.SceneGparam = new SceneGparamConfig();
                    this.SoundSpaceType = SoundSpace.NoReverb;
                    this.MapNameID = -1;
                    this.DisableStart = false;
                    this.DisableBonfireEntityID = -1;
                    this.MapVisType = MapVisiblity.Good;
                    this.PlayRegionID = -1;
                }

                private protected override void DeepCopyTo(Part part) {
                    var collision = (Collision)part;
                    collision.Gparam = this.Gparam.DeepCopy();
                    collision.SceneGparam = this.SceneGparam.DeepCopy();
                }

                internal Collision(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.HitFilterID = br.ReadByte();
                    this.SoundSpaceType = br.ReadEnum8<SoundSpace>();
                    this.EnvLightMapSpotIndex = br.ReadInt16();
                    this.ReflectPlaneHeight = br.ReadSingle();
                    _ = br.AssertInt32(0); // Navmesh Group (4)
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(-1); // Vagrant Entity ID (3)
                    _ = br.AssertInt32(-1);
                    _ = br.AssertInt32(-1);
                    this.MapNameID = br.ReadInt16();
                    this.DisableStart = br.ReadBoolean();
                    _ = br.AssertByte(0);
                    this.DisableBonfireEntityID = br.ReadInt32();
                    this.ChameleonParamID = br.ReadInt32();
                    this.UnkHitIndex = br.ReadInt32();
                    this.UnkT34 = br.ReadByte();
                    this.UnkT35 = br.ReadByte();
                    this.UnkT36 = br.ReadByte();
                    this.MapVisType = br.ReadEnum8<MapVisiblity>();
                    this.PlayRegionID = br.ReadInt32();
                    this.LockCamID1 = br.ReadInt16();
                    this.LockCamID2 = br.ReadInt16();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                private protected override void ReadGparamConfig(BinaryReaderEx br) => this.Gparam = new GparamConfig(br);
                private protected override void ReadSceneGparamConfig(BinaryReaderEx br) => this.SceneGparam = new SceneGparamConfig(br);

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteByte(this.HitFilterID);
                    bw.WriteByte((byte)this.SoundSpaceType);
                    bw.WriteInt16(this.EnvLightMapSpotIndex);
                    bw.WriteSingle(this.ReflectPlaneHeight);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt16(this.MapNameID);
                    bw.WriteBoolean(this.DisableStart);
                    bw.WriteByte(0);
                    bw.WriteInt32(this.DisableBonfireEntityID);
                    bw.WriteInt32(this.ChameleonParamID);
                    bw.WriteInt32(this.UnkHitIndex);
                    bw.WriteByte(this.UnkT34);
                    bw.WriteByte(this.UnkT35);
                    bw.WriteByte(this.UnkT36);
                    bw.WriteByte((byte)this.MapVisType);
                    bw.WriteInt32(this.PlayRegionID);
                    bw.WriteInt16(this.LockCamID1);
                    bw.WriteInt16(this.LockCamID2);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                private protected override void WriteGparamConfig(BinaryWriterEx bw) => this.Gparam.Write(bw);
                private protected override void WriteSceneGparamConfig(BinaryWriterEx bw) => this.SceneGparam.Write(bw);

                internal override void GetNames(MSB3 msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.UnkHitName = MSB.FindName(entries.Parts, this.UnkHitIndex);
                }

                internal override void GetIndices(MSB3 msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.UnkHitIndex = MSB.FindIndex(entries.Parts, this.UnkHitName);
                }
            }

            /// <summary>
            /// An object that is either unused, or used for a cutscene.
            /// </summary>
            public class DummyObject : ObjectBase {
                private protected override PartsType Type => PartsType.DummyObject;

                /// <summary>
                /// Creates a DummyObject with default values.
                /// </summary>
                public DummyObject() : base() { }

                internal DummyObject(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// An enemy that is either unused, or used for a cutscene.
            /// </summary>
            public class DummyEnemy : EnemyBase {
                private protected override PartsType Type => PartsType.DummyEnemy;

                /// <summary>
                /// Creates a DummyEnemy with default values.
                /// </summary>
                public DummyEnemy() : base() { }

                internal DummyEnemy(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Determines which collision parts load other maps.
            /// </summary>
            public class ConnectCollision : Part {
                private protected override PartsType Type => PartsType.ConnectCollision;
                private protected override bool HasGparamConfig => false;
                private protected override bool HasSceneGparamConfig => false;

                /// <summary>
                /// The name of the associated collision part.
                /// </summary>
                public string CollisionName { get; set; }
                private int CollisionIndex;

                /// <summary>
                /// The map to load when on this collision.
                /// </summary>
                public byte[] MapID { get; private set; }

                /// <summary>
                /// Creates a new ConnectCollision with default values.
                /// </summary>
                public ConnectCollision() : base("hXXXXXX_XXXX") => this.MapID = new byte[4];

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

                internal override void GetNames(MSB3 msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.CollisionName = MSB.FindName(msb.Parts.Collisions, this.CollisionIndex);
                }

                internal override void GetIndices(MSB3 msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.CollisionIndex = MSB.FindIndex(msb.Parts.Collisions, this.CollisionName);
                }
            }
        }
    }
}
