﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using SoulsFormats.Formats.MSB;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MSBS {
        internal enum PartType : uint {
            MapPiece = 0,
            Object = 1,
            Enemy = 2,
            Player = 4,
            Collision = 5,
            DummyObject = 9,
            DummyEnemy = 10,
            ConnectCollision = 11,
        }

        /// <summary>
        /// Instances of actual things in the map.
        /// </summary>
        public class PartsParam : Param<Part>, IMsbParam<IMsbPart> {
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
            /// Creates an empty PartsParam with the default version.
            /// </summary>
            public PartsParam() : base(35, "PARTS_PARAM_ST") {
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
            /// Returns every Part in the order they'll be written.
            /// </summary>
            public override List<Part> GetEntries() => SFUtil.ConcatAll<Part>(
                    this.MapPieces, this.Objects, this.Enemies, this.Players, this.Collisions,
                    this.DummyObjects, this.DummyEnemies, this.ConnectCollisions);
            IReadOnlyList<IMsbPart> IMsbParam<IMsbPart>.GetEntries() => this.GetEntries();

            internal override Part ReadEntry(BinaryReaderEx br) {
                PartType type = br.GetEnum32<PartType>(br.Position + 8);
                return type switch {
                    PartType.MapPiece => this.MapPieces.EchoAdd(new Part.MapPiece(br)),
                    PartType.Object => this.Objects.EchoAdd(new Part.Object(br)),
                    PartType.Enemy => this.Enemies.EchoAdd(new Part.Enemy(br)),
                    PartType.Player => this.Players.EchoAdd(new Part.Player(br)),
                    PartType.Collision => this.Collisions.EchoAdd(new Part.Collision(br)),
                    PartType.DummyObject => this.DummyObjects.EchoAdd(new Part.DummyObject(br)),
                    PartType.DummyEnemy => this.DummyEnemies.EchoAdd(new Part.DummyEnemy(br)),
                    PartType.ConnectCollision => this.ConnectCollisions.EchoAdd(new Part.ConnectCollision(br)),
                    _ => throw new NotImplementedException($"Unimplemented part type: {type}"),
                };
            }
        }

        /// <summary>
        /// Common data for all types of part.
        /// </summary>
        public abstract class Part : Entry, IMsbPart {
            private protected abstract PartType Type { get; }
            private protected abstract bool HasUnk1 { get; }
            private protected abstract bool HasUnk2 { get; }
            private protected abstract bool HasGparamConfig { get; }
            private protected abstract bool HasSceneGparamConfig { get; }
            private protected abstract bool HasUnk7 { get; }

            /// <summary>
            /// The model used by this part; requires an entry in ModelParam.
            /// </summary>
            public string ModelName { get; set; }
            private int ModelIndex;

            /// <summary>
            /// A path to a .sib file, presumably some kind of editor placeholder.
            /// </summary>
            public string SibPath { get; set; }

            /// <summary>
            /// Location of the part.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Rotation of the part.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// Scale of the part; only works for map pieces and objects.
            /// </summary>
            public Vector3 Scale { get; set; }

            /// <summary>
            /// Identifies the part in event scripts.
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
            public byte LanternID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte LodParamID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE09 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool IsPointLightShadowSrc { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE0B { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool IsShadowSrc { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte IsStaticShadowSrc { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte IsCascade3ShadowSrc { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE0F { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE10 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool IsShadowDest { get; set; }

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
            public byte EnableOnAboveShadow { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool DisablePointLightEffect { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE17 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int UnkE18 { get; set; }

            /// <summary>
            /// Allows multiple parts to be identified by the same entity ID.
            /// </summary>
            public int[] EntityGroupIDs { get; private set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int UnkE3C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int UnkE40 { get; set; }

            private protected Part(string name) {
                this.Name = name;
                this.SibPath = "";
                this.Scale = Vector3.One;
                this.EntityID = -1;
                this.EntityGroupIDs = new int[8];
                for (int i = 0; i < 8; i++) {
                    this.EntityGroupIDs[i] = -1;
                }
            }

            /// <summary>
            /// Creates a deep copy of the part.
            /// </summary>
            public Part DeepCopy() {
                var part = (Part)this.MemberwiseClone();
                part.EntityGroupIDs = (int[])this.EntityGroupIDs.Clone();
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
                _ = br.AssertInt32(-1);
                _ = br.AssertInt32(0);
                long unkOffset1 = br.ReadInt64();
                long unkOffset2 = br.ReadInt64();
                long entityDataOffset = br.ReadInt64();
                long typeDataOffset = br.ReadInt64();
                long gparamOffset = br.ReadInt64();
                long unkOffset6 = br.ReadInt64();
                long unkOffset7 = br.ReadInt64();
                _ = br.AssertInt64(0);
                _ = br.AssertInt64(0);
                _ = br.AssertInt64(0);

                if (nameOffset == 0) {
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {this.GetType()}.");
                }

                if (sibOffset == 0) {
                    throw new InvalidDataException($"{nameof(sibOffset)} must not be 0 in type {this.GetType()}.");
                }

                if (this.HasUnk1 ^ unkOffset1 != 0) {
                    throw new InvalidDataException($"Unexpected {nameof(unkOffset1)} 0x{unkOffset1:X} in type {this.GetType()}.");
                }

                if (this.HasUnk2 ^ unkOffset2 != 0) {
                    throw new InvalidDataException($"Unexpected {nameof(unkOffset2)} 0x{unkOffset2:X} in type {this.GetType()}.");
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

                if (this.HasSceneGparamConfig ^ unkOffset6 != 0) {
                    throw new InvalidDataException($"Unexpected {nameof(unkOffset6)} 0x{unkOffset6:X} in type {this.GetType()}.");
                }

                if (this.HasUnk7 ^ unkOffset7 != 0) {
                    throw new InvalidDataException($"Unexpected {nameof(unkOffset7)} 0x{unkOffset7:X} in type {this.GetType()}.");
                }

                br.Position = start + nameOffset;
                this.Name = br.ReadUTF16();

                br.Position = start + sibOffset;
                this.SibPath = br.ReadUTF16();

                if (this.HasUnk1) {
                    br.Position = start + unkOffset1;
                    this.ReadUnk1(br);
                }

                if (this.HasUnk2) {
                    br.Position = start + unkOffset2;
                    this.ReadUnk2(br);
                }

                br.Position = start + entityDataOffset;
                this.ReadEntityData(br);

                br.Position = start + typeDataOffset;
                this.ReadTypeData(br);

                if (this.HasGparamConfig) {
                    br.Position = start + gparamOffset;
                    this.ReadGparamConfig(br);
                }

                if (this.HasSceneGparamConfig) {
                    br.Position = start + unkOffset6;
                    this.ReadSceneGparamConfig(br);
                }

                if (this.HasUnk7) {
                    br.Position = start + unkOffset7;
                    this.ReadUnk7(br);
                }
            }

            private void ReadEntityData(BinaryReaderEx br) {
                this.EntityID = br.ReadInt32();
                this.UnkE04 = br.ReadByte();
                this.UnkE05 = br.ReadByte();
                this.UnkE06 = br.ReadByte();
                this.LanternID = br.ReadByte();
                this.LodParamID = br.ReadByte();
                this.UnkE09 = br.ReadByte();
                this.IsPointLightShadowSrc = br.ReadBoolean();
                this.UnkE0B = br.ReadByte();
                this.IsShadowSrc = br.ReadBoolean();
                this.IsStaticShadowSrc = br.ReadByte();
                this.IsCascade3ShadowSrc = br.ReadByte();
                this.UnkE0F = br.ReadByte();
                this.UnkE10 = br.ReadByte();
                this.IsShadowDest = br.ReadBoolean();
                this.IsShadowOnly = br.ReadBoolean();
                this.DrawByReflectCam = br.ReadBoolean();
                this.DrawOnlyReflectCam = br.ReadBoolean();
                this.EnableOnAboveShadow = br.ReadByte();
                this.DisablePointLightEffect = br.ReadBoolean();
                this.UnkE17 = br.ReadByte();
                this.UnkE18 = br.ReadInt32();
                this.EntityGroupIDs = br.ReadInt32s(8);
                this.UnkE3C = br.ReadInt32();
                this.UnkE40 = br.ReadInt32();
                br.AssertPattern(0x10, 0x00);
            }

            private protected abstract void ReadTypeData(BinaryReaderEx br);

            private protected virtual void ReadUnk1(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(ReadUnk1)}.");

            private protected virtual void ReadUnk2(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(ReadUnk2)}.");

            private protected virtual void ReadGparamConfig(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(ReadGparamConfig)}.");

            private protected virtual void ReadSceneGparamConfig(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(ReadSceneGparamConfig)}.");

            private protected virtual void ReadUnk7(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(ReadUnk7)}.");

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
                bw.WriteInt32(-1);
                bw.WriteInt32(0);
                bw.ReserveInt64("UnkOffset1");
                bw.ReserveInt64("UnkOffset2");
                bw.ReserveInt64("EntityDataOffset");
                bw.ReserveInt64("TypeDataOffset");
                bw.ReserveInt64("GparamOffset");
                bw.ReserveInt64("SceneGparamOffset");
                bw.ReserveInt64("UnkOffset7");
                bw.WriteInt64(0);
                bw.WriteInt64(0);
                bw.WriteInt64(0);

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(MSB.ReambiguateName(this.Name), true);

                bw.FillInt64("SibOffset", bw.Position - start);
                bw.WriteUTF16(this.SibPath, true);
                bw.Pad(8);

                if (this.HasUnk1) {
                    bw.FillInt64("UnkOffset1", bw.Position - start);
                    this.WriteUnk1(bw);
                } else {
                    bw.FillInt64("UnkOffset1", 0);
                }

                if (this.HasUnk2) {
                    bw.FillInt64("UnkOffset2", bw.Position - start);
                    this.WriteUnk2(bw);
                } else {
                    bw.FillInt64("UnkOffset2", 0);
                }

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

                if (this.HasUnk7) {
                    bw.FillInt64("UnkOffset7", bw.Position - start);
                    this.WriteUnk7(bw);
                } else {
                    bw.FillInt64("UnkOffset7", 0);
                }
            }

            private void WriteEntityData(BinaryWriterEx bw) {
                bw.WriteInt32(this.EntityID);
                bw.WriteByte(this.UnkE04);
                bw.WriteByte(this.UnkE05);
                bw.WriteByte(this.UnkE06);
                bw.WriteByte(this.LanternID);
                bw.WriteByte(this.LodParamID);
                bw.WriteByte(this.UnkE09);
                bw.WriteBoolean(this.IsPointLightShadowSrc);
                bw.WriteByte(this.UnkE0B);
                bw.WriteBoolean(this.IsShadowSrc);
                bw.WriteByte(this.IsStaticShadowSrc);
                bw.WriteByte(this.IsCascade3ShadowSrc);
                bw.WriteByte(this.UnkE0F);
                bw.WriteByte(this.UnkE10);
                bw.WriteBoolean(this.IsShadowDest);
                bw.WriteBoolean(this.IsShadowOnly);
                bw.WriteBoolean(this.DrawByReflectCam);
                bw.WriteBoolean(this.DrawOnlyReflectCam);
                bw.WriteByte(this.EnableOnAboveShadow);
                bw.WriteBoolean(this.DisablePointLightEffect);
                bw.WriteByte(this.UnkE17);
                bw.WriteInt32(this.UnkE18);
                bw.WriteInt32s(this.EntityGroupIDs);
                bw.WriteInt32(this.UnkE3C);
                bw.WriteInt32(this.UnkE40);
                bw.WritePattern(0x10, 0x00);
                bw.Pad(8);
            }

            private protected abstract void WriteTypeData(BinaryWriterEx bw);

            private protected virtual void WriteUnk1(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(WriteUnk1)}.");

            private protected virtual void WriteUnk2(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(WriteUnk2)}.");

            private protected virtual void WriteGparamConfig(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(WriteGparamConfig)}.");

            private protected virtual void WriteSceneGparamConfig(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(WriteSceneGparamConfig)}.");

            private protected virtual void WriteUnk7(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(WriteUnk7)}.");

            internal virtual void GetNames(MSBS msb, Entries entries) => this.ModelName = MSB.FindName(entries.Models, this.ModelIndex);

            internal virtual void GetIndices(MSBS msb, Entries entries) => this.ModelIndex = MSB.FindIndex(entries.Models, this.ModelName);

            /// <summary>
            /// Returns the type and name of the part as a string.
            /// </summary>
            public override string ToString() => $"{this.Type} {this.Name}";

            /// <summary>
            /// Unknown.
            /// </summary>
            public class UnkStruct1 {
                /// <summary>
                /// Unknown.
                /// </summary>
                public uint[] CollisionMask { get; private set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Condition1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Condition2 { get; set; }

                /// <summary>
                /// Creates an UnkStruct1 with default values.
                /// </summary>
                public UnkStruct1() {
                    this.CollisionMask = new uint[48];
                    this.Condition1 = 0;
                    this.Condition2 = 0;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkStruct1 DeepCopy() {
                    var unk1 = (UnkStruct1)this.MemberwiseClone();
                    unk1.CollisionMask = (uint[])this.CollisionMask.Clone();
                    return unk1;
                }

                internal UnkStruct1(BinaryReaderEx br) {
                    this.CollisionMask = br.ReadUInt32s(48);
                    this.Condition1 = br.ReadByte();
                    this.Condition2 = br.ReadByte();
                    _ = br.AssertInt16(0);
                    br.AssertPattern(0xC0, 0x00);
                }

                internal void Write(BinaryWriterEx bw) {
                    bw.WriteUInt32s(this.CollisionMask);
                    bw.WriteByte(this.Condition1);
                    bw.WriteByte(this.Condition2);
                    bw.WriteInt16(0);
                    bw.WritePattern(0xC0, 0x00);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class UnkStruct2 {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int Condition { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int[] DispGroups { get; private set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk26 { get; set; }

                /// <summary>
                /// Creates an UnkStruct2 with default values.
                /// </summary>
                public UnkStruct2() => this.DispGroups = new int[8];

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkStruct2 DeepCopy() {
                    var unk2 = (UnkStruct2)this.MemberwiseClone();
                    unk2.DispGroups = (int[])this.DispGroups.Clone();
                    return unk2;
                }

                internal UnkStruct2(BinaryReaderEx br) {
                    this.Condition = br.ReadInt32();
                    this.DispGroups = br.ReadInt32s(8);
                    this.Unk24 = br.ReadInt16();
                    this.Unk26 = br.ReadInt16();
                    br.AssertPattern(0x20, 0x00);
                }

                internal void Write(BinaryWriterEx bw) {
                    bw.WriteInt32(this.Condition);
                    bw.WriteInt32s(this.DispGroups);
                    bw.WriteInt16(this.Unk24);
                    bw.WriteInt16(this.Unk26);
                    bw.WritePattern(0x20, 0x00);
                }
            }

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
            /// Unknown; sceneGParam Struct according to Pav.
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

                /// <summary>
                /// Returns a string representation of the object.
                /// </summary>
                public override string ToString() => $"EventID[{this.EventIDs[0],2}][{this.EventIDs[1],2}][{this.EventIDs[2],2}][{this.EventIDs[3],2}] {this.Unk40:0.0}";
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class UnkStruct7 {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk04 { get; set; }

                /// <summary>
                /// ID in GrassTypeParam determining properties of dynamic grass on a map piece.
                /// </summary>
                public int GrassTypeParamID { get; set; }

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
                /// Creates an UnkStruct7 with default values.
                /// </summary>
                public UnkStruct7() { }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkStruct7 DeepCopy() => (UnkStruct7)this.MemberwiseClone();

                internal UnkStruct7(BinaryReaderEx br) {
                    this.Unk00 = br.ReadInt32();
                    this.Unk04 = br.ReadInt32();
                    this.GrassTypeParamID = br.ReadInt32();
                    this.Unk0C = br.ReadInt32();
                    this.Unk10 = br.ReadInt32();
                    this.Unk14 = br.ReadInt32();
                    _ = br.AssertInt32(-1);
                    _ = br.AssertInt32(0);
                }

                internal void Write(BinaryWriterEx bw) {
                    bw.WriteInt32(this.Unk00);
                    bw.WriteInt32(this.Unk04);
                    bw.WriteInt32(this.GrassTypeParamID);
                    bw.WriteInt32(this.Unk0C);
                    bw.WriteInt32(this.Unk10);
                    bw.WriteInt32(this.Unk14);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Fixed visual geometry.
            /// </summary>
            public class MapPiece : Part {
                private protected override PartType Type => PartType.MapPiece;
                private protected override bool HasUnk1 => true;
                private protected override bool HasUnk2 => false;
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => false;
                private protected override bool HasUnk7 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct1 Unk1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public GparamConfig Gparam { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct7 Unk7 { get; set; }

                /// <summary>
                /// Creates a MapPiece with default values.
                /// </summary>
                public MapPiece() : base("mXXXXXX_XXXX") {
                    this.Unk1 = new UnkStruct1();
                    this.Gparam = new GparamConfig();
                    this.Unk7 = new UnkStruct7();
                }

                private protected override void DeepCopyTo(Part part) {
                    var piece = (MapPiece)part;
                    piece.Unk1 = this.Unk1.DeepCopy();
                    piece.Gparam = this.Gparam.DeepCopy();
                    piece.Unk7 = this.Unk7.DeepCopy();
                }

                internal MapPiece(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                private protected override void ReadUnk1(BinaryReaderEx br) => this.Unk1 = new UnkStruct1(br);
                private protected override void ReadGparamConfig(BinaryReaderEx br) => this.Gparam = new GparamConfig(br);
                private protected override void ReadUnk7(BinaryReaderEx br) => this.Unk7 = new UnkStruct7(br);

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                private protected override void WriteUnk1(BinaryWriterEx bw) => this.Unk1.Write(bw);
                private protected override void WriteGparamConfig(BinaryWriterEx bw) => this.Gparam.Write(bw);
                private protected override void WriteUnk7(BinaryWriterEx bw) => this.Unk7.Write(bw);
            }

            /// <summary>
            /// Common base data for objects and dummy objects.
            /// </summary>
            public abstract class ObjectBase : Part {
                private protected override bool HasUnk2 => false;
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => false;
                private protected override bool HasUnk7 => false;

                /// <summary>
                /// Unknown.
                /// </summary>
                public GparamConfig Gparam { get; set; }

                /// <summary>
                /// Reference to a map piece or collision; believed to determine when the object is loaded.
                /// </summary>
                public string ObjPartName1 { get; set; }
                private int ObjPartIndex1;

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
                public byte UnkT0E { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool SetMainObjStructureBooleans { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short AnimID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT1A { get; set; }

                /// <summary>
                /// Reference to a collision; believed to be involved with loading when grappling to the object.
                /// </summary>
                public string ObjPartName2 { get; set; }
                private int ObjPartIndex2;

                /// <summary>
                /// Reference to a collision; believed to be involved with loading when grappling to the object.
                /// </summary>
                public string ObjPartName3 { get; set; }
                private int ObjPartIndex3;

                private protected ObjectBase() : base("oXXXXXX_XXXX") => this.Gparam = new GparamConfig();

                private protected override void DeepCopyTo(Part part) {
                    var obj = (ObjectBase)part;
                    obj.Gparam = this.Gparam.DeepCopy();
                }

                private protected ObjectBase(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    this.ObjPartIndex1 = br.ReadInt32();
                    this.BreakTerm = br.ReadByte();
                    this.NetSyncType = br.ReadBoolean();
                    this.UnkT0E = br.ReadByte();
                    this.SetMainObjStructureBooleans = br.ReadBoolean();
                    this.AnimID = br.ReadInt16();
                    _ = br.AssertInt16(-1);
                    _ = br.AssertInt32(-1);
                    this.UnkT18 = br.ReadInt16();
                    this.UnkT1A = br.ReadInt16();
                    _ = br.AssertInt32(-1);
                    this.ObjPartIndex2 = br.ReadInt32();
                    this.ObjPartIndex3 = br.ReadInt32();
                }

                private protected override void ReadGparamConfig(BinaryReaderEx br) => this.Gparam = new GparamConfig(br);

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(this.ObjPartIndex1);
                    bw.WriteByte(this.BreakTerm);
                    bw.WriteBoolean(this.NetSyncType);
                    bw.WriteByte(this.UnkT0E);
                    bw.WriteBoolean(this.SetMainObjStructureBooleans);
                    bw.WriteInt16(this.AnimID);
                    bw.WriteInt16(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt16(this.UnkT18);
                    bw.WriteInt16(this.UnkT1A);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(this.ObjPartIndex2);
                    bw.WriteInt32(this.ObjPartIndex3);
                }

                private protected override void WriteGparamConfig(BinaryWriterEx bw) => this.Gparam.Write(bw);

                internal override void GetNames(MSBS msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.ObjPartName1 = MSB.FindName(entries.Parts, this.ObjPartIndex1);
                    this.ObjPartName2 = MSB.FindName(entries.Parts, this.ObjPartIndex2);
                    this.ObjPartName3 = MSB.FindName(entries.Parts, this.ObjPartIndex3);
                }

                internal override void GetIndices(MSBS msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.ObjPartIndex1 = MSB.FindIndex(entries.Parts, this.ObjPartName1);
                    this.ObjPartIndex2 = MSB.FindIndex(entries.Parts, this.ObjPartName2);
                    this.ObjPartIndex3 = MSB.FindIndex(entries.Parts, this.ObjPartName3);
                }
            }

            /// <summary>
            /// A dynamic or interactible element in the map.
            /// </summary>
            public class Object : ObjectBase {
                private protected override PartType Type => PartType.Object;
                private protected override bool HasUnk1 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct1 Unk1 { get; set; }

                /// <summary>
                /// Creates an Object with default values.
                /// </summary>
                public Object() : base() => this.Unk1 = new UnkStruct1();

                private protected override void DeepCopyTo(Part part) {
                    base.DeepCopyTo(part);
                    var obj = (Object)part;
                    obj.Unk1 = this.Unk1.DeepCopy();
                }

                internal Object(BinaryReaderEx br) : base(br) { }

                private protected override void ReadUnk1(BinaryReaderEx br) => this.Unk1 = new UnkStruct1(br);

                private protected override void WriteUnk1(BinaryWriterEx bw) => this.Unk1.Write(bw);
            }

            /// <summary>
            /// Common base data for enemies and dummy enemies.
            /// </summary>
            public abstract class EnemyBase : Part {
                private protected override bool HasUnk2 => false;
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => false;
                private protected override bool HasUnk7 => false;

                /// <summary>
                /// Unknown.
                /// </summary>
                public GparamConfig Gparam { get; set; }

                /// <summary>
                /// An ID in NPCThinkParam that determines the enemy's AI characteristics.
                /// </summary>
                public int ThinkParamID { get; set; }

                /// <summary>
                /// An ID in NPCParam that determines a variety of enemy properties.
                /// </summary>
                public int NPCParamID { get; set; }

                /// <summary>
                /// Unknown; previously talk ID, now always 0 or 1 except for the Memorial Mob in Senpou.
                /// </summary>
                public int UnkT10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short PlatoonID { get; set; }

                /// <summary>
                /// An ID in CharaInitParam that determines a human's inventory and stats.
                /// </summary>
                public int CharaInitID { get; set; }

                /// <summary>
                /// Should reference the collision the enemy starts on.
                /// </summary>
                public string CollisionPartName { get; set; }
                private int CollisionPartIndex;

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT20 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT22 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int BackupEventAnimID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int EventFlagID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int EventFlagCompareState { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT48 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT4C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT50 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT78 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT84 { get; set; }

                private protected EnemyBase() : base("cXXXX_XXXX") {
                    this.Gparam = new GparamConfig();
                    this.ThinkParamID = -1;
                    this.NPCParamID = -1;
                    this.UnkT10 = -1;
                    this.CharaInitID = -1;
                    this.BackupEventAnimID = -1;
                    this.EventFlagID = -1;
                }

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
                    this.UnkT10 = br.ReadInt32();
                    _ = br.AssertInt16(0);
                    this.PlatoonID = br.ReadInt16();
                    this.CharaInitID = br.ReadInt32();
                    this.CollisionPartIndex = br.ReadInt32();
                    this.UnkT20 = br.ReadInt16();
                    this.UnkT22 = br.ReadInt16();
                    this.UnkT24 = br.ReadInt32();
                    br.AssertPattern(0x10, 0xFF);
                    this.BackupEventAnimID = br.ReadInt32();
                    _ = br.AssertInt32(-1);
                    this.EventFlagID = br.ReadInt32();
                    this.EventFlagCompareState = br.ReadInt32();
                    this.UnkT48 = br.ReadInt32();
                    this.UnkT4C = br.ReadInt32();
                    this.UnkT50 = br.ReadInt32();
                    _ = br.AssertInt32(1);
                    _ = br.AssertInt32(-1);
                    _ = br.AssertInt32(1);
                    br.AssertPattern(0x18, 0x00);
                    this.UnkT78 = br.ReadInt32();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    this.UnkT84 = br.ReadSingle();
                    for (int i = 0; i < 5; i++) {
                        _ = br.AssertInt32(-1);
                        _ = br.AssertInt16(-1);
                        _ = br.AssertInt16(0xA);
                    }
                    br.AssertPattern(0x10, 0x00);
                }

                private protected override void ReadGparamConfig(BinaryReaderEx br) => this.Gparam = new GparamConfig(br);

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(this.ThinkParamID);
                    bw.WriteInt32(this.NPCParamID);
                    bw.WriteInt32(this.UnkT10);
                    bw.WriteInt16(0);
                    bw.WriteInt16(this.PlatoonID);
                    bw.WriteInt32(this.CharaInitID);
                    bw.WriteInt32(this.CollisionPartIndex);
                    bw.WriteInt16(this.UnkT20);
                    bw.WriteInt16(this.UnkT22);
                    bw.WriteInt32(this.UnkT24);
                    bw.WritePattern(0x10, 0xFF);
                    bw.WriteInt32(this.BackupEventAnimID);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(this.EventFlagID);
                    bw.WriteInt32(this.EventFlagCompareState);
                    bw.WriteInt32(this.UnkT48);
                    bw.WriteInt32(this.UnkT4C);
                    bw.WriteInt32(this.UnkT50);
                    bw.WriteInt32(1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(1);
                    bw.WritePattern(0x18, 0x00);
                    bw.WriteInt32(this.UnkT78);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteSingle(this.UnkT84);
                    for (int i = 0; i < 5; i++) {
                        bw.WriteInt32(-1);
                        bw.WriteInt16(-1);
                        bw.WriteInt16(0xA);
                    }
                    bw.WritePattern(0x10, 0x00);
                }

                private protected override void WriteGparamConfig(BinaryWriterEx bw) => this.Gparam.Write(bw);

                internal override void GetNames(MSBS msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.CollisionPartName = MSB.FindName(entries.Parts, this.CollisionPartIndex);
                }

                internal override void GetIndices(MSBS msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.CollisionPartIndex = MSB.FindIndex(entries.Parts, this.CollisionPartName);
                }
            }

            /// <summary>
            /// Any non-player character.
            /// </summary>
            public class Enemy : EnemyBase {
                private protected override PartType Type => PartType.Enemy;
                private protected override bool HasUnk1 => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct1 Unk1 { get; set; }

                /// <summary>
                /// Creates an Enemy with default values.
                /// </summary>
                public Enemy() : base() => this.Unk1 = new UnkStruct1();

                private protected override void DeepCopyTo(Part part) {
                    base.DeepCopyTo(part);
                    var enemy = (Enemy)part;
                    enemy.Unk1 = this.Unk1.DeepCopy();
                }

                internal Enemy(BinaryReaderEx br) : base(br) { }

                private protected override void ReadUnk1(BinaryReaderEx br) => this.Unk1 = new UnkStruct1(br);

                private protected override void WriteUnk1(BinaryWriterEx bw) => this.Unk1.Write(bw);
            }

            /// <summary>
            /// A spawn point for the player, or something.
            /// </summary>
            public class Player : Part {
                private protected override PartType Type => PartType.Player;
                private protected override bool HasUnk1 => false;
                private protected override bool HasUnk2 => false;
                private protected override bool HasGparamConfig => false;
                private protected override bool HasSceneGparamConfig => false;
                private protected override bool HasUnk7 => false;

                /// <summary>
                /// Creates a Player with default values.
                /// </summary>
                public Player() : base("c0000_XXXX") { }

                internal Player(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) => br.AssertPattern(0x10, 0x00);

                private protected override void WriteTypeData(BinaryWriterEx bw) => bw.WritePattern(0x10, 0x00);
            }

            /// <summary>
            /// Invisible but physical geometry.
            /// </summary>
            public class Collision : Part {
                private protected override PartType Type => PartType.Collision;
                private protected override bool HasUnk1 => true;
                private protected override bool HasUnk2 => true;
                private protected override bool HasGparamConfig => true;
                private protected override bool HasSceneGparamConfig => true;
                private protected override bool HasUnk7 => false;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct1 Unk1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct2 Unk2 { get; set; }

                /// <summary>
                /// Unknown.
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
                /// Adds reverb to sounds while on this collision to simulate echoes.
                /// </summary>
                public byte SoundSpaceType { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float ReflectPlaneHeight { get; set; }

                /// <summary>
                /// Determines the text to display for map popups and save files.
                /// </summary>
                public short MapNameID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool DisableStart { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT17 { get; set; }

                /// <summary>
                /// If not -1, the bonfire with this ID will be disabled when enemies are on this collision.
                /// </summary>
                public int DisableBonfireEntityID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT25 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT26 { get; set; }

                /// <summary>
                /// Should alter visibility while on this collision, but doesn't seem to do much.
                /// </summary>
                public byte MapVisibility { get; set; }

                /// <summary>
                /// Used to determine invasion eligibility.
                /// </summary>
                public int PlayRegionID { get; set; }

                /// <summary>
                /// Alters camera properties while on this collision.
                /// </summary>
                public short LockCamParamID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT3C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT40 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT44 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT48 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT4C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT50 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT54 { get; set; }

                /// <summary>
                /// Creates a Collision with default values.
                /// </summary>
                public Collision() : base("hXXXXXX") {
                    this.Unk1 = new UnkStruct1();
                    this.Unk2 = new UnkStruct2();
                    this.Gparam = new GparamConfig();
                    this.SceneGparam = new SceneGparamConfig();
                    this.DisableBonfireEntityID = -1;
                }

                private protected override void DeepCopyTo(Part part) {
                    var collision = (Collision)part;
                    collision.Unk1 = this.Unk1.DeepCopy();
                    collision.Unk2 = this.Unk2.DeepCopy();
                    collision.Gparam = this.Gparam.DeepCopy();
                    collision.SceneGparam = this.SceneGparam.DeepCopy();
                }

                internal Collision(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.HitFilterID = br.ReadByte(); // Pav says Type, did it change?
                    this.SoundSpaceType = br.ReadByte();
                    _ = br.AssertInt16(0);
                    this.ReflectPlaneHeight = br.ReadSingle();
                    _ = br.AssertInt32(-1);
                    _ = br.AssertInt32(-1);
                    _ = br.AssertInt32(-1);
                    this.MapNameID = br.ReadInt16();
                    this.DisableStart = br.ReadBoolean();
                    this.UnkT17 = br.ReadByte();
                    this.DisableBonfireEntityID = br.ReadInt32();
                    _ = br.AssertInt32(-1);
                    _ = br.AssertInt32(-1);
                    this.UnkT24 = br.ReadByte();
                    this.UnkT25 = br.ReadByte();
                    this.UnkT26 = br.ReadByte();
                    this.MapVisibility = br.ReadByte();
                    this.PlayRegionID = br.ReadInt32();
                    this.LockCamParamID = br.ReadInt16();
                    _ = br.AssertInt16(-1);
                    _ = br.AssertInt32(-1);
                    _ = br.AssertInt32(-1);
                    _ = br.AssertInt32(-1);
                    this.UnkT3C = br.ReadInt32();
                    this.UnkT40 = br.ReadInt32();
                    this.UnkT44 = br.ReadSingle();
                    this.UnkT48 = br.ReadSingle();
                    this.UnkT4C = br.ReadInt32();
                    this.UnkT50 = br.ReadSingle();
                    this.UnkT54 = br.ReadSingle();
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                private protected override void ReadUnk1(BinaryReaderEx br) => this.Unk1 = new UnkStruct1(br);
                private protected override void ReadUnk2(BinaryReaderEx br) => this.Unk2 = new UnkStruct2(br);
                private protected override void ReadGparamConfig(BinaryReaderEx br) => this.Gparam = new GparamConfig(br);
                private protected override void ReadSceneGparamConfig(BinaryReaderEx br) => this.SceneGparam = new SceneGparamConfig(br);

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteByte(this.HitFilterID);
                    bw.WriteByte(this.SoundSpaceType);
                    bw.WriteInt16(0);
                    bw.WriteSingle(this.ReflectPlaneHeight);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt16(this.MapNameID);
                    bw.WriteBoolean(this.DisableStart);
                    bw.WriteByte(this.UnkT17);
                    bw.WriteInt32(this.DisableBonfireEntityID);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteByte(this.UnkT24);
                    bw.WriteByte(this.UnkT25);
                    bw.WriteByte(this.UnkT26);
                    bw.WriteByte(this.MapVisibility);
                    bw.WriteInt32(this.PlayRegionID);
                    bw.WriteInt16(this.LockCamParamID);
                    bw.WriteInt16(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(this.UnkT3C);
                    bw.WriteInt32(this.UnkT40);
                    bw.WriteSingle(this.UnkT44);
                    bw.WriteSingle(this.UnkT48);
                    bw.WriteInt32(this.UnkT4C);
                    bw.WriteSingle(this.UnkT50);
                    bw.WriteSingle(this.UnkT54);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                private protected override void WriteUnk1(BinaryWriterEx bw) => this.Unk1.Write(bw);
                private protected override void WriteUnk2(BinaryWriterEx bw) => this.Unk2.Write(bw);
                private protected override void WriteGparamConfig(BinaryWriterEx bw) => this.Gparam.Write(bw);
                private protected override void WriteSceneGparamConfig(BinaryWriterEx bw) => this.SceneGparam.Write(bw);
            }

            /// <summary>
            /// An object that either isn't used, or is used for a cutscene.
            /// </summary>
            public class DummyObject : ObjectBase {
                private protected override PartType Type => PartType.DummyObject;
                private protected override bool HasUnk1 => false;

                /// <summary>
                /// Creates a DummyObject with default values.
                /// </summary>
                public DummyObject() : base() { }

                internal DummyObject(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// An enemy that either isn't used, or is used for a cutscene.
            /// </summary>
            public class DummyEnemy : EnemyBase {
                private protected override PartType Type => PartType.DummyEnemy;
                private protected override bool HasUnk1 => false;

                /// <summary>
                /// Creates a DummyEnemy with default values.
                /// </summary>
                public DummyEnemy() : base() { }

                internal DummyEnemy(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// References an actual collision and causes another map to be loaded while on it.
            /// </summary>
            public class ConnectCollision : Part {
                private protected override PartType Type => PartType.ConnectCollision;
                private protected override bool HasUnk1 => false;
                private protected override bool HasUnk2 => true;
                private protected override bool HasGparamConfig => false;
                private protected override bool HasSceneGparamConfig => false;
                private protected override bool HasUnk7 => false;

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkStruct2 Unk2 { get; set; }

                /// <summary>
                /// The collision part to attach to.
                /// </summary>
                public string CollisionName { get; set; }
                private int CollisionIndex;

                /// <summary>
                /// The map to load when on this collision.
                /// </summary>
                public byte[] MapID { get; private set; }

                /// <summary>
                /// Creates a ConnectCollision with default values.
                /// </summary>
                public ConnectCollision() : base("hXXXXXX_XXXX") {
                    this.Unk2 = new UnkStruct2();
                    this.MapID = new byte[4];
                }

                private protected override void DeepCopyTo(Part part) {
                    var connect = (ConnectCollision)part;
                    connect.Unk2 = this.Unk2.DeepCopy();
                    connect.MapID = (byte[])this.MapID.Clone();
                }

                internal ConnectCollision(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.CollisionIndex = br.ReadInt32();
                    this.MapID = br.ReadBytes(4);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                private protected override void ReadUnk2(BinaryReaderEx br) => this.Unk2 = new UnkStruct2(br);

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.CollisionIndex);
                    bw.WriteBytes(this.MapID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                private protected override void WriteUnk2(BinaryWriterEx bw) => this.Unk2.Write(bw);

                internal override void GetNames(MSBS msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.CollisionName = MSB.FindName(msb.Parts.Collisions, this.CollisionIndex);
                }

                internal override void GetIndices(MSBS msb, Entries entries) {
                    base.GetIndices(msb, entries);
                    this.CollisionIndex = MSB.FindIndex(msb.Parts.Collisions, this.CollisionName);
                }
            }
        }
    }
}
