using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SoulsFormats.Formats.MSB;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MSBS {
        internal enum ModelType : uint {
            MapPiece = 0,
            Object = 1,
            Enemy = 2,
            Player = 4,
            Collision = 5,
        }

        /// <summary>
        /// Model files that are available for parts to use.
        /// </summary>
        public class ModelParam : Param<Model>, IMsbParam<IMsbModel> {
            /// <summary>
            /// Models for fixed terrain and scenery.
            /// </summary>
            public List<Model.MapPiece> MapPieces { get; set; }

            /// <summary>
            /// Models for dynamic props.
            /// </summary>
            public List<Model.Object> Objects { get; set; }

            /// <summary>
            /// Models for non-player entities.
            /// </summary>
            public List<Model.Enemy> Enemies { get; set; }

            /// <summary>
            /// Models for player spawn points, I think.
            /// </summary>
            public List<Model.Player> Players { get; set; }

            /// <summary>
            /// Models for physics collision.
            /// </summary>
            public List<Model.Collision> Collisions { get; set; }

            /// <summary>
            /// Creates an empty ModelParam with the default version.
            /// </summary>
            public ModelParam() : base(35, "MODEL_PARAM_ST") {
                this.MapPieces = new List<Model.MapPiece>();
                this.Objects = new List<Model.Object>();
                this.Enemies = new List<Model.Enemy>();
                this.Players = new List<Model.Player>();
                this.Collisions = new List<Model.Collision>();
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

                    default:
                        throw new ArgumentException($"Unrecognized type {model.GetType()}.", nameof(model));
                }
                return model;
            }
            IMsbModel IMsbParam<IMsbModel>.Add(IMsbModel item) => this.Add((Model)item);

            /// <summary>
            /// Returns every Model in the order they will be written.
            /// </summary>
            public override List<Model> GetEntries() => SFUtil.ConcatAll<Model>(
                    this.MapPieces, this.Objects, this.Enemies, this.Players, this.Collisions);
            IReadOnlyList<IMsbModel> IMsbParam<IMsbModel>.GetEntries() => this.GetEntries();

            internal override Model ReadEntry(BinaryReaderEx br) {
                ModelType type = br.GetEnum32<ModelType>(br.Position + 8);
                return type switch {
                    ModelType.MapPiece => this.MapPieces.EchoAdd(new Model.MapPiece(br)),
                    ModelType.Object => this.Objects.EchoAdd(new Model.Object(br)),
                    ModelType.Enemy => this.Enemies.EchoAdd(new Model.Enemy(br)),
                    ModelType.Player => this.Players.EchoAdd(new Model.Player(br)),
                    ModelType.Collision => this.Collisions.EchoAdd(new Model.Collision(br)),
                    _ => throw new NotImplementedException($"Unimplemented model type: {type}"),
                };
            }
        }

        /// <summary>
        /// A model file available for parts to reference.
        /// </summary>
        public abstract class Model : Entry, IMsbModel {
            private protected abstract ModelType Type { get; }
            private protected abstract bool HasTypeData { get; }

            /// <summary>
            /// A path to a .sib file, presumed to be some kind of editor placeholder.
            /// </summary>
            public string SibPath { get; set; }

            private int InstanceCount;

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk1C { get; set; }

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
                this.Unk1C = br.ReadInt32();
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
                bw.WriteInt32(this.Unk1C);
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
            /// Returns the type and name of the model as a string.
            /// </summary>
            public override string ToString() => $"{this.Type} {this.Name}";

            /// <summary>
            /// A model for fixed terrain or scenery.
            /// </summary>
            public class MapPiece : Model {
                private protected override ModelType Type => ModelType.MapPiece;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool UnkT01 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool UnkT02 { get; set; }

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
                /// Creates a MapPiece with default values.
                /// </summary>
                public MapPiece() : base("mXXXXXX") { }

                internal MapPiece(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadBoolean();
                    this.UnkT01 = br.ReadBoolean();
                    this.UnkT02 = br.ReadBoolean();
                    _ = br.AssertByte(0);
                    this.UnkT04 = br.ReadSingle();
                    this.UnkT08 = br.ReadSingle();
                    this.UnkT0C = br.ReadSingle();
                    this.UnkT10 = br.ReadSingle();
                    this.UnkT14 = br.ReadSingle();
                    this.UnkT18 = br.ReadSingle();
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteBoolean(this.UnkT00);
                    bw.WriteBoolean(this.UnkT01);
                    bw.WriteBoolean(this.UnkT02);
                    bw.WriteByte(0);
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
            /// A model for a dynamic prop.
            /// </summary>
            public class Object : Model {
                private protected override ModelType Type => ModelType.Object;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates an Object with default values.
                /// </summary>
                public Object() : base("oXXXXXX") { }

                internal Object(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A model for a non-player entity.
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
            /// A model for a player spawn point?
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
            /// A model for collision physics.
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
        }
    }
}
