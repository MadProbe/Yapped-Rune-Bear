using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SoulsFormats.Formats.MSB;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MSB3 {
        internal enum ModelType : uint {
            MapPiece = 0,
            Object = 1,
            Enemy = 2,
            Item = 3,
            Player = 4,
            Collision = 5,
            Navmesh = 6,
            DummyObject = 7,
            DummyEnemy = 8,
            Other = 0xFFFFFFFF
        }

        /// <summary>
        /// A section containing all the models available to parts in this map.
        /// </summary>
        public class ModelParam : Param<Model>, IMsbParam<IMsbModel> {
            internal override int Version => 3;
            internal override string Type => "MODEL_PARAM_ST";

            /// <summary>
            /// Map piece models in this section.
            /// </summary>
            public List<Model.MapPiece> MapPieces { get; set; }

            /// <summary>
            /// Object models in this section.
            /// </summary>
            public List<Model.Object> Objects { get; set; }

            /// <summary>
            /// Enemy models in this section.
            /// </summary>
            public List<Model.Enemy> Enemies { get; set; }

            /// <summary>
            /// Player models in this section.
            /// </summary>
            public List<Model.Player> Players { get; set; }

            /// <summary>
            /// Collision models in this section.
            /// </summary>
            public List<Model.Collision> Collisions { get; set; }

            /// <summary>
            /// Other models in this section.
            /// </summary>
            public List<Model.Other> Others { get; set; }

            /// <summary>
            /// Creates a new ModelParam with no models.
            /// </summary>
            public ModelParam() {
                this.MapPieces = new List<Model.MapPiece>();
                this.Objects = new List<Model.Object>();
                this.Enemies = new List<Model.Enemy>();
                this.Players = new List<Model.Player>();
                this.Collisions = new List<Model.Collision>();
                this.Others = new List<Model.Other>();
            }

            /// <summary>
            /// Adds a model to the appropriate list for its type; returns the model.
            /// </summary>
            public Model Add(Model model) {
                switch (model) {
                    case Model.MapPiece m: this.MapPieces.Add(m); break;
                    case Model.Object m: this.Objects.Add(m); break;
                    case Model.Enemy m: this.Enemies.Add(m); break;
                    case Model.Player m: this.Players.Add(m); break;
                    case Model.Collision m: this.Collisions.Add(m); break;
                    case Model.Other m: this.Others.Add(m); break;

                    default:
                        throw new ArgumentException($"Unrecognized type {model.GetType()}.", nameof(model));
                }
                return model;
            }
            IMsbModel IMsbParam<IMsbModel>.Add(IMsbModel item) => this.Add((Model)item);

            /// <summary>
            /// Returns every model in the order they will be written.
            /// </summary>
            public override List<Model> GetEntries() => SFUtil.ConcatAll<Model>(
                    this.MapPieces, this.Objects, this.Enemies, this.Players, this.Collisions,
                    this.Others);
            IReadOnlyList<IMsbModel> IMsbParam<IMsbModel>.GetEntries() => this.GetEntries();

            internal override Model ReadEntry(BinaryReaderEx br) {
                ModelType type = br.GetEnum32<ModelType>(br.Position + 8);
                return type switch {
                    ModelType.MapPiece => this.MapPieces.EchoAdd(new Model.MapPiece(br)),
                    ModelType.Object => this.Objects.EchoAdd(new Model.Object(br)),
                    ModelType.Enemy => this.Enemies.EchoAdd(new Model.Enemy(br)),
                    ModelType.Player => this.Players.EchoAdd(new Model.Player(br)),
                    ModelType.Collision => this.Collisions.EchoAdd(new Model.Collision(br)),
                    ModelType.Other => this.Others.EchoAdd(new Model.Other(br)),
                    _ => throw new NotImplementedException($"Unsupported model type: {type}"),
                };
            }
        }

        /// <summary>
        /// A model available for use by parts in this map.
        /// </summary>
        public abstract class Model : NamedEntry, IMsbModel {
            private protected abstract ModelType Type { get; }
            private protected abstract bool HasTypeData { get; }

            /// <summary>
            /// The name of this model.
            /// </summary>
            public override string Name { get; set; }

            /// <summary>
            /// Unknown network path to a .sib file.
            /// </summary>
            public string SibPath { get; set; }

            private int InstanceCount;

            private protected Model(string name) {
                this.Name = name;
                this.SibPath = "";
            }

            /// <summary>
            /// Creates a deep copy of the model.
            /// </summary>
            public Model DeepCopy() => (Model)this.MemberwiseClone();
            IMsbModel IMsbModel.DeepCopy() => this.DeepCopy();

            private protected Model(BinaryReaderEx br) {
                long start = br.Position;

                long nameOffset = br.ReadInt64();
                _ = br.AssertUInt32((uint)this.Type);
                _ = br.ReadInt32(); // ID
                long sibOffset = br.ReadInt64();
                this.InstanceCount = br.ReadInt32();
                _ = br.AssertInt32(0);
                long typeDataOffset = br.ReadInt64();

                if (nameOffset == 0) {
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {this.GetType()}.");
                }

                if (sibOffset == 0) {
                    throw new InvalidDataException($"{nameof(sibOffset)} must not be 0 in type {this.GetType()}.");
                }

                if (this.HasTypeData ^ typeDataOffset != 0) {
                    throw new InvalidDataException($"Unexpected {nameof(typeDataOffset)} 0x{typeDataOffset:X} in type {this.GetType()}.");
                }

                br.Position = start + nameOffset;
                this.Name = br.ReadUTF16();

                br.Position = start + sibOffset;
                this.SibPath = br.ReadUTF16();

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
                bw.ReserveInt64("SibOffset");
                bw.WriteInt32(this.InstanceCount);
                bw.WriteInt32(0);
                bw.ReserveInt64("TypeDataOffset");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(MSB.ReambiguateName(this.Name), true);
                bw.FillInt64("SibOffset", bw.Position - start);
                bw.WriteUTF16(this.SibPath, true);
                bw.Pad(8);

                if (this.HasTypeData) {
                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                    this.WriteTypeData(bw);
                } else {
                    bw.FillInt64("TypeDataOffset", 0);
                }
            }

            private protected virtual void WriteTypeData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(ReadTypeData)}.");

            internal void CountInstances(List<Part> parts) => this.InstanceCount = parts.Count(p => p.ModelName == this.Name);

            /// <summary>
            /// Returns the type and name of this model.
            /// </summary>
            public override string ToString() => $"{this.Type} : {this.Name}";

            /// <summary>
            /// A fixed part of the level geometry.
            /// </summary>
            public class MapPiece : Model {
                private protected override ModelType Type => ModelType.MapPiece;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT01 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool UnkT02 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool UnkT03 { get; set; }

                /// <summary>
                /// Creates a MapPiece with default values.
                /// </summary>
                public MapPiece() : base("mXXXXXX") {
                    this.UnkT02 = true;
                    this.UnkT03 = true;
                }

                internal MapPiece(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadByte();
                    this.UnkT01 = br.ReadByte();
                    this.UnkT02 = br.ReadBoolean();
                    this.UnkT03 = br.ReadBoolean();

                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteByte(this.UnkT00);
                    bw.WriteByte(this.UnkT01);
                    bw.WriteBoolean(this.UnkT02);
                    bw.WriteBoolean(this.UnkT03);

                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// A dynamic or interactible entity.
            /// </summary>
            public class Object : Model {
                private protected override ModelType Type => ModelType.Object;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT01 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool UnkT02 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool UnkT03 { get; set; }

                /// <summary>
                /// Creates an Object with default values.
                /// </summary>
                public Object() : base("oXXXXXX") {
                    this.UnkT02 = true;
                    this.UnkT03 = true;
                }

                internal Object(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadByte();
                    this.UnkT01 = br.ReadByte();
                    this.UnkT02 = br.ReadBoolean();
                    this.UnkT03 = br.ReadBoolean();

                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteByte(this.UnkT00);
                    bw.WriteByte(this.UnkT01);
                    bw.WriteBoolean(this.UnkT02);
                    bw.WriteBoolean(this.UnkT03);

                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Any character in the map that is not the player.
            /// </summary>
            public class Enemy : Model {
                private protected override ModelType Type => ModelType.Enemy;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates an Enemy with default values.
                /// </summary>
                public Enemy() : base("cXXXX") { }

                internal Enemy(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// The player character.
            /// </summary>
            public class Player : Model {
                private protected override ModelType Type => ModelType.Player;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates a Player with default values.
                /// </summary>
                public Player() : base("c0000") { }

                internal Player(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// The invisible physical surface of the map.
            /// </summary>
            public class Collision : Model {
                private protected override ModelType Type => ModelType.Collision;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates a Collision with default values.
                /// </summary>
                public Collision() : base("hXXXXXX") { }

                internal Collision(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Other : Model {
                private protected override ModelType Type => ModelType.Other;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates an Other with default values.
                /// </summary>
                public Other() : base("lXXXXXX") { }

                internal Other(BinaryReaderEx br) : base(br) { }
            }
        }
    }
}
