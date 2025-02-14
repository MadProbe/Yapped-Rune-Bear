﻿using System;
using System.Collections.Generic;
using System.Numerics;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MSBN {
        /// <summary>
        /// Instances of various "things" in this MSB.
        /// </summary>
        public class PartsSection : Section<Part> {
            internal override string Type => "PARTS_PARAM_ST";

            /// <summary>
            /// Map pieces in the MSB.
            /// </summary>
            public List<Part> MapPieces;

            /// <summary>
            /// Objects in the MSB.
            /// </summary>
            public List<Part> Objects;

            /// <summary>
            /// Enemies in the MSB.
            /// </summary>
            public List<Part> Enemies;

            /// <summary>
            /// Items in the MSB.
            /// </summary>
            public List<Part> Items;

            /// <summary>
            /// Players in the MSB.
            /// </summary>
            public List<Part> Players;

            /// <summary>
            /// Collisions in the MSB.
            /// </summary>
            public List<Part> Collisions;

            /// <summary>
            /// Protobosses in the MSB.
            /// </summary>
            public List<Part> Protobosses;

            /// <summary>
            /// Navmeshes in the MSB.
            /// </summary>
            public List<Part> Navmeshes;

            /// <summary>
            /// Dummy objects in the MSB.
            /// </summary>
            public List<Part> DummyObjects;

            /// <summary>
            /// Dummy enemies in the MSB.
            /// </summary>
            public List<Part> DummyEnemies;

            /// <summary>
            /// Connect collisions in the MSB.
            /// </summary>
            public List<Part> ConnectCollisions;

            internal PartsSection(BinaryReaderEx br, int unk1) : base(br, unk1) {
                this.MapPieces = new List<Part>();
                this.Objects = new List<Part>();
                this.Enemies = new List<Part>();
                this.Items = new List<Part>();
                this.Players = new List<Part>();
                this.Collisions = new List<Part>();
                this.Protobosses = new List<Part>();
                this.Navmeshes = new List<Part>();
                this.DummyObjects = new List<Part>();
                this.DummyEnemies = new List<Part>();
                this.ConnectCollisions = new List<Part>();
            }

            /// <summary>
            /// Returns every part in the order they'll be written.
            /// </summary>
            public override List<Part> GetEntries() => SFUtil.ConcatAll<Part>(
                    this.MapPieces, this.Objects, this.Enemies, this.Items, this.Players, this.Collisions, this.Protobosses, this.Navmeshes, this.DummyObjects, this.DummyEnemies, this.ConnectCollisions);

            internal override Part ReadEntry(BinaryReaderEx br) {
                PartsType type = br.GetEnum32<PartsType>(br.Position + 4);

                switch (type) {
                    case PartsType.MapPiece:
                        var mapPiece = new Part(br);
                        this.MapPieces.Add(mapPiece);
                        return mapPiece;

                    case PartsType.Object:
                        var obj = new Part(br);
                        this.Objects.Add(obj);
                        return obj;

                    case PartsType.Enemy:
                        var enemy = new Part(br);
                        this.Enemies.Add(enemy);
                        return enemy;

                    case PartsType.Item:
                        var item = new Part(br);
                        this.Items.Add(item);
                        return item;

                    case PartsType.Player:
                        var player = new Part(br);
                        this.Players.Add(player);
                        return player;

                    case PartsType.Collision:
                        var collision = new Part(br);
                        this.Collisions.Add(collision);
                        return collision;

                    case PartsType.Protoboss:
                        var protoboss = new Part(br);
                        this.Protobosses.Add(protoboss);
                        return protoboss;

                    case PartsType.Navmesh:
                        var navmesh = new Part(br);
                        this.Navmeshes.Add(navmesh);
                        return navmesh;

                    case PartsType.DummyObject:
                        var dummyObj = new Part(br);
                        this.DummyObjects.Add(dummyObj);
                        return dummyObj;

                    case PartsType.DummyEnemy:
                        var dummyEne = new Part(br);
                        this.DummyEnemies.Add(dummyEne);
                        return dummyEne;

                    case PartsType.ConnectCollision:
                        var connectColl = new Part(br);
                        this.ConnectCollisions.Add(connectColl);
                        return connectColl;

                    default:
                        throw new NotImplementedException($"Unsupported part type: {type}");
                }
            }

            internal override void WriteEntries(BinaryWriterEx bw, List<Part> entries) => throw new NotImplementedException();

            internal void GetNames(MSBN msb, Entries entries) {
                foreach (Part part in entries.Parts) {
                    part.GetNames(msb, entries);
                }
            }

            internal void GetIndices(MSBN msb, Entries entries) {
                foreach (Part part in entries.Parts) {
                    part.GetIndices(msb, entries);
                }
            }
        }

        internal enum PartsType : uint {
            Collision = 0x0,
            MapPiece = 0x1,
            Object = 0x2,
            Enemy = 0x3,
            Item = 0x4,
            Player = 0x5,
            NPCWander = 0x6,
            Protoboss = 0x7,
            Navmesh = 0x8,
            DummyObject = 0x9,
            DummyEnemy = 0xA,
            ConnectCollision = 0xB,
        }

        /// <summary>
        /// Any instance of some "thing" in a map.
        /// </summary>
        public class Part : Entry {
            internal PartsType Type { get; private set; }

            /// <summary>
            /// The name of this part.
            /// </summary>
            public override string Name { get; set; }

            private int modelIndex;
            /// <summary>
            /// The name of this part's model.
            /// </summary>
            public string ModelName;

            /// <summary>
            /// The center of the part.
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// The rotation of the part.
            /// </summary>
            public Vector3 Rotation;

            /// <summary>
            /// The scale of the part, which only really works right for map pieces.
            /// </summary>
            public Vector3 Scale;

            internal Part(BinaryReaderEx br) {
                long start = br.Position;

                int nameOffset = br.ReadInt32();
                this.Type = br.ReadEnum32<PartsType>();
                _ = br.ReadInt32(); // ID
                this.modelIndex = br.ReadInt32();
                _ = br.ReadInt32();
                this.Position = br.ReadVector3();
                this.Rotation = br.ReadVector3();
                this.Scale = br.ReadVector3();

                this.Name = br.GetShiftJIS(start + nameOffset);
            }

            internal virtual void GetNames(MSBN msb, Entries entries) => this.ModelName = MSB.FindName(entries.Models, this.modelIndex);

            internal virtual void GetIndices(MSBN msb, Entries entries) => this.modelIndex = MSB.FindIndex(entries.Models, this.ModelName);

            /// <summary>
            /// Returns the type and name of this part.
            /// </summary>
            public override string ToString() => $"{this.Type} : {this.Name}";
        }
    }
}
