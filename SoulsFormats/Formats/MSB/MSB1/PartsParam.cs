using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using SoulsFormats.Formats.MSB;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MSB1 {
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
        }

        /// <summary>
        /// All instances of concrete things in the map.
        /// </summary>
        public class PartsParam : Param<Part>, IMsbParam<IMsbPart> {
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
                    this.Navmeshes, this.DummyObjects, this.DummyEnemies, this.ConnectCollisions);
            IReadOnlyList<IMsbPart> IMsbParam<IMsbPart>.GetEntries() => this.GetEntries();

            internal override Part ReadEntry(BinaryReaderEx br) {
                PartType type = br.GetEnum32<PartType>(br.Position + 4);
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
                    _ => throw new NotImplementedException($"Unimplemented part type: {type}"),
                };
            }
        }

        /// <summary>
        /// Common information for all concrete entities.
        /// </summary>
        public abstract class Part : Entry, IMsbPart {
            private protected abstract PartType Type { get; }

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
            /// Identifies the part in external files.
            /// </summary>
            public int EntityID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte LightID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte FogID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte ScatterID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte LensFlareID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte ShadowID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte DofID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte ToneMapID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte ToneCorrectID { get; set; }

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
            public byte IsShadowSrc { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte IsShadowDest { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte IsShadowOnly { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte DrawByReflectCam { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte DrawOnlyReflectCam { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UseDepthBiasFloat { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte DisablePointLightEffect { get; set; }

            private protected Part(string name) {
                this.Name = name;
                this.SibPath = "";
                this.Scale = Vector3.One;
                this.DrawGroups = new uint[4] {
                    0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
                this.DispGroups = new uint[4] {
                    0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
                this.EntityID = -1;
            }

            /// <summary>
            /// Creates a deep copy of the part.
            /// </summary>
            public Part DeepCopy() {
                var part = (Part)this.MemberwiseClone();
                part.DrawGroups = (uint[])this.DrawGroups.Clone();
                part.DispGroups = (uint[])this.DispGroups.Clone();
                this.DeepCopyTo(part);
                return part;
            }
            IMsbPart IMsbPart.DeepCopy() => this.DeepCopy();

            private protected virtual void DeepCopyTo(Part part) { }

            private protected Part(BinaryReaderEx br) {
                long start = br.Position;
                int nameOffset = br.ReadInt32();
                _ = br.AssertUInt32((uint)this.Type);
                _ = br.ReadInt32(); // ID
                this.ModelIndex = br.ReadInt32();
                int sibOffset = br.ReadInt32();
                this.Position = br.ReadVector3();
                this.Rotation = br.ReadVector3();
                this.Scale = br.ReadVector3();
                this.DrawGroups = br.ReadUInt32s(4);
                this.DispGroups = br.ReadUInt32s(4);
                int entityDataOffset = br.ReadInt32();
                int typeDataOffset = br.ReadInt32();
                _ = br.AssertInt32(0);

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

                br.Position = start + nameOffset;
                this.Name = br.ReadShiftJIS();

                br.Position = start + sibOffset;
                this.SibPath = br.ReadShiftJIS();

                br.Position = start + entityDataOffset;
                this.ReadEntityData(br);

                br.Position = start + typeDataOffset;
                this.ReadTypeData(br);
            }

            private void ReadEntityData(BinaryReaderEx br) {
                this.EntityID = br.ReadInt32();
                this.LightID = br.ReadByte();
                this.FogID = br.ReadByte();
                this.ScatterID = br.ReadByte();
                this.LensFlareID = br.ReadByte();
                this.ShadowID = br.ReadByte();
                this.DofID = br.ReadByte();
                this.ToneMapID = br.ReadByte();
                this.ToneCorrectID = br.ReadByte();
                this.LanternID = br.ReadByte();
                this.LodParamID = br.ReadByte();
                _ = br.AssertByte(0);
                this.IsShadowSrc = br.ReadByte();
                this.IsShadowDest = br.ReadByte();
                this.IsShadowOnly = br.ReadByte();
                this.DrawByReflectCam = br.ReadByte();
                this.DrawOnlyReflectCam = br.ReadByte();
                this.UseDepthBiasFloat = br.ReadByte();
                this.DisablePointLightEffect = br.ReadByte();
                _ = br.AssertByte(0);
                _ = br.AssertByte(0);
            }

            private protected abstract void ReadTypeData(BinaryReaderEx br);

            internal override void Write(BinaryWriterEx bw, int id) {
                long start = bw.Position;
                bw.ReserveInt32("NameOffset");
                bw.WriteUInt32((uint)this.Type);
                bw.WriteInt32(id);
                bw.WriteInt32(this.ModelIndex);
                bw.ReserveInt32("SibOffset");
                bw.WriteVector3(this.Position);
                bw.WriteVector3(this.Rotation);
                bw.WriteVector3(this.Scale);
                bw.WriteUInt32s(this.DrawGroups);
                bw.WriteUInt32s(this.DispGroups);
                bw.ReserveInt32("EntityDataOffset");
                bw.ReserveInt32("TypeDataOffset");
                bw.WriteInt32(0);

                long stringsStart = bw.Position;
                bw.FillInt32("NameOffset", (int)(bw.Position - start));
                bw.WriteShiftJIS(MSB.ReambiguateName(this.Name), true);

                bw.FillInt32("SibOffset", (int)(bw.Position - start));
                bw.WriteShiftJIS(this.SibPath, true);
                bw.Pad(4);
                if (bw.Position - stringsStart < 0x14) {
                    bw.WritePattern((int)(0x14 - (bw.Position - stringsStart)), 0x00);
                }

                bw.FillInt32("EntityDataOffset", (int)(bw.Position - start));
                this.WriteEntityData(bw);

                bw.FillInt32("TypeDataOffset", (int)(bw.Position - start));
                this.WriteTypeData(bw);
            }

            private void WriteEntityData(BinaryWriterEx bw) {
                bw.WriteInt32(this.EntityID);
                bw.WriteByte(this.LightID);
                bw.WriteByte(this.FogID);
                bw.WriteByte(this.ScatterID);
                bw.WriteByte(this.LensFlareID);
                bw.WriteByte(this.ShadowID);
                bw.WriteByte(this.DofID);
                bw.WriteByte(this.ToneMapID);
                bw.WriteByte(this.ToneCorrectID);
                bw.WriteByte(this.LanternID);
                bw.WriteByte(this.LodParamID);
                bw.WriteByte(0);
                bw.WriteByte(this.IsShadowSrc);
                bw.WriteByte(this.IsShadowDest);
                bw.WriteByte(this.IsShadowOnly);
                bw.WriteByte(this.DrawByReflectCam);
                bw.WriteByte(this.DrawOnlyReflectCam);
                bw.WriteByte(this.UseDepthBiasFloat);
                bw.WriteByte(this.DisablePointLightEffect);
                bw.WriteByte(0);
                bw.WriteByte(0);
            }

            private protected abstract void WriteTypeData(BinaryWriterEx bw);

            internal virtual void GetNames(MSB1 msb, Entries entries) => this.ModelName = MSB.FindName(entries.Models, this.ModelIndex);

            internal virtual void GetIndices(MSB1 msb, Entries entries) => this.ModelIndex = MSB.FindIndex(entries.Models, this.ModelName);

            /// <summary>
            /// A visible but not physical model making up the map.
            /// </summary>
            public class MapPiece : Part {
                private protected override PartType Type => PartType.MapPiece;

                /// <summary>
                /// Creates a MapPiece with default values.
                /// </summary>
                public MapPiece() : base("mXXXXBX") { }

                internal MapPiece(BinaryReaderEx br) : base(br) { }

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
            /// Common base data for objects and dummy objects.
            /// </summary>
            public abstract class ObjectBase : Part {
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
                public short InitAnimID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT0E { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT10 { get; set; }

                private protected ObjectBase() : base("oXXXX_XXXX") { }

                private protected ObjectBase(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    _ = br.AssertInt32(0);
                    this.CollisionIndex = br.ReadInt32();
                    this.BreakTerm = br.ReadSByte();
                    this.NetSyncType = br.ReadSByte();
                    _ = br.AssertInt16(0);
                    this.InitAnimID = br.ReadInt16();
                    this.UnkT0E = br.ReadInt16();
                    this.UnkT10 = br.ReadInt32();
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(this.CollisionIndex);
                    bw.WriteSByte(this.BreakTerm);
                    bw.WriteSByte(this.NetSyncType);
                    bw.WriteInt16(0);
                    bw.WriteInt16(this.InitAnimID);
                    bw.WriteInt16(this.UnkT0E);
                    bw.WriteInt32(this.UnkT10);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(MSB1 msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.CollisionName = MSB.FindName(entries.Parts, this.CollisionIndex);
                }

                internal override void GetIndices(MSB1 msb, Entries entries) {
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
                /// Unknown.
                /// </summary>
                public byte PointMoveType { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public ushort PlatoonID { get; set; }

                /// <summary>
                /// ID in CharaInitParam determining equipment and stats for humans.
                /// </summary>
                public int CharaInitID { get; set; }

                /// <summary>
                /// Collision that controls loading of the enemy.
                /// </summary>
                public string CollisionName { get; set; }
                private int CollisionIndex;

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
                    this.ThinkParamID = -1;
                    this.NPCParamID = -1;
                    this.TalkID = -1;
                    this.CharaInitID = -1;
                    this.MovePointNames = new string[8];
                }

                private protected override void DeepCopyTo(Part part) {
                    var enemy = (EnemyBase)part;
                    enemy.MovePointNames = (string[])this.MovePointNames.Clone();
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
                    this.PlatoonID = br.ReadUInt16();
                    this.CharaInitID = br.ReadInt32();
                    this.CollisionIndex = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    this.MovePointIndices = br.ReadInt16s(8);
                    this.InitAnimID = br.ReadInt32();
                    this.DamageAnimID = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(this.ThinkParamID);
                    bw.WriteInt32(this.NPCParamID);
                    bw.WriteInt32(this.TalkID);
                    bw.WriteByte(this.PointMoveType);
                    bw.WriteByte(0);
                    bw.WriteUInt16(this.PlatoonID);
                    bw.WriteInt32(this.CharaInitID);
                    bw.WriteInt32(this.CollisionIndex);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt16s(this.MovePointIndices);
                    bw.WriteInt32(this.InitAnimID);
                    bw.WriteInt32(this.DamageAnimID);
                }

                internal override void GetNames(MSB1 msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.CollisionName = MSB.FindName(entries.Parts, this.CollisionIndex);

                    this.MovePointNames = new string[this.MovePointIndices.Length];
                    for (int i = 0; i < this.MovePointIndices.Length; i++) {
                        this.MovePointNames[i] = MSB.FindName(entries.Regions, this.MovePointIndices[i]);
                    }
                }

                internal override void GetIndices(MSB1 msb, Entries entries) {
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
                /// Unknown.
                /// </summary>
                public uint[] NvmGroups { get; private set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int[] VagrantEntityIDs { get; private set; }

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
                public byte UnkT27 { get; set; }

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
                public Collision() : base("hXXXXBX") {
                    this.NvmGroups = new uint[4]{
                        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
                    this.VagrantEntityIDs = new int[3] { -1, -1, -1 };
                    this.MapNameID = -1;
                    this.DisableBonfireEntityID = -1;
                    this.LockCamParamID1 = -1;
                    this.LockCamParamID2 = -1;
                }

                private protected override void DeepCopyTo(Part part) {
                    var collision = (Collision)part;
                    collision.NvmGroups = (uint[])this.NvmGroups.Clone();
                    collision.VagrantEntityIDs = (int[])this.VagrantEntityIDs.Clone();
                }

                internal Collision(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.HitFilterID = br.ReadByte();
                    this.SoundSpaceType = br.ReadByte();
                    this.EnvLightMapSpotIndex = br.ReadInt16();
                    this.ReflectPlaneHeight = br.ReadSingle();
                    this.NvmGroups = br.ReadUInt32s(4);
                    this.VagrantEntityIDs = br.ReadInt32s(3);
                    this.MapNameID = br.ReadInt16();
                    this.DisableStart = br.ReadBoolean();
                    this.UnkT27 = br.ReadByte();
                    this.DisableBonfireEntityID = br.ReadInt32();
                    _ = br.AssertInt32(-1);
                    _ = br.AssertInt32(-1);
                    _ = br.AssertInt32(-1);
                    this.PlayRegionID = br.ReadInt32();
                    this.LockCamParamID1 = br.ReadInt16();
                    this.LockCamParamID2 = br.ReadInt16();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteByte(this.HitFilterID);
                    bw.WriteByte(this.SoundSpaceType);
                    bw.WriteInt16(this.EnvLightMapSpotIndex);
                    bw.WriteSingle(this.ReflectPlaneHeight);
                    bw.WriteUInt32s(this.NvmGroups);
                    bw.WriteInt32s(this.VagrantEntityIDs);
                    bw.WriteInt16(this.MapNameID);
                    bw.WriteBoolean(this.DisableStart);
                    bw.WriteByte(this.UnkT27);
                    bw.WriteInt32(this.DisableBonfireEntityID);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(this.PlayRegionID);
                    bw.WriteInt16(this.LockCamParamID1);
                    bw.WriteInt16(this.LockCamParamID2);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// An AI navigation mesh.
            /// </summary>
            public class Navmesh : Part {
                private protected override PartType Type => PartType.Navmesh;

                /// <summary>
                /// Unknown.
                /// </summary>
                public uint[] NvmGroups { get; private set; }

                /// <summary>
                /// Creates a Navmesh with default values.
                /// </summary>
                public Navmesh() : base("nXXXXBX") => this.NvmGroups = new uint[4] {
                        0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };

                private protected override void DeepCopyTo(Part part) {
                    var navmesh = (Navmesh)part;
                    navmesh.NvmGroups = (uint[])this.NvmGroups.Clone();
                }

                internal Navmesh(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.NvmGroups = br.ReadUInt32s(4);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteUInt32s(this.NvmGroups);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
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

                internal override void GetNames(MSB1 msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.CollisionName = MSB.FindName(msb.Parts.Collisions, this.CollisionIndex);
                }

                internal override void GetIndices(MSB1 msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.CollisionIndex = MSB.FindIndex(msb.Parts.Collisions, this.CollisionName);
                }
            }
        }
    }
}
