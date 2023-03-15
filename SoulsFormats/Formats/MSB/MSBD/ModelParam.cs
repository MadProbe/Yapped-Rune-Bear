using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SoulsFormats.Formats.MSB;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MSBD {
        internal enum ModelType : uint {
            MapPiece = 0,
            Object = 1,
            Enemy = 2,
            Player = 4,
            Collision = 5,
            Navmesh = 6,
            DummyObject = 7,
            DummyEnemy = 8,
        }

        /// <summary>
        /// Model files that are available for parts to use.
        /// </summary>
        public class ModelParam : Param<Model>, IMsbParam<IMsbModel> {
            internal override string Name => "MODEL_PARAM_ST";

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
            /// Models for AI navigation.
            /// </summary>
            public List<Model.Navmesh> Navmeshes { get; set; }

            /// <summary>
            /// Models for dummy dynamic props.
            /// </summary>
            public List<Model.DummyObject> DummyObjects { get; set; }

            /// <summary>
            /// Models for dummy non-player entities.
            /// </summary>
            public List<Model.DummyEnemy> DummyEnemies { get; set; }

            /// <summary>
            /// Creates an empty ModelParam.
            /// </summary>
            public ModelParam() : base() {
                this.MapPieces = new List<Model.MapPiece>();
                this.Objects = new List<Model.Object>();
                this.Enemies = new List<Model.Enemy>();
                this.Players = new List<Model.Player>();
                this.Collisions = new List<Model.Collision>();
                this.Navmeshes = new List<Model.Navmesh>();
                this.DummyObjects = new List<Model.DummyObject>();
                this.DummyEnemies = new List<Model.DummyEnemy>();
            }

            internal override Model ReadEntry(BinaryReaderEx br) {
                ModelType type = br.GetEnum32<ModelType>(br.Position + 4);
                return type switch {
                    ModelType.MapPiece => this.MapPieces.EchoAdd(new Model.MapPiece(br)),
                    ModelType.Object => this.Objects.EchoAdd(new Model.Object(br)),
                    ModelType.Enemy => this.Enemies.EchoAdd(new Model.Enemy(br)),
                    ModelType.Player => this.Players.EchoAdd(new Model.Player(br)),
                    ModelType.Collision => this.Collisions.EchoAdd(new Model.Collision(br)),
                    ModelType.Navmesh => this.Navmeshes.EchoAdd(new Model.Navmesh(br)),
                    ModelType.DummyObject => this.DummyObjects.EchoAdd(new Model.DummyObject(br)),
                    ModelType.DummyEnemy => this.DummyEnemies.EchoAdd(new Model.DummyEnemy(br)),
                    _ => throw new NotImplementedException($"Unimplemented model type: {type}"),
                };
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
                    case Model.Navmesh m: this.Navmeshes.Add(m); break;
                    case Model.DummyObject m: this.DummyObjects.Add(m); break;
                    case Model.DummyEnemy m: this.DummyEnemies.Add(m); break;

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
                    this.MapPieces, this.Objects, this.Enemies, this.Players, this.Collisions,
                    this.Navmeshes, this.DummyObjects, this.DummyEnemies);
            IReadOnlyList<IMsbModel> IMsbParam<IMsbModel>.GetEntries() => this.GetEntries();
        }

        /// <summary>
        /// A model file available for parts to reference.
        /// </summary>
        public abstract class Model : Entry, IMsbModel {
            private protected abstract ModelType Type { get; }

            /// <summary>
            /// The name of the model.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// A path to a .sib file, presumed to be some kind of editor placeholder.
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
                int nameOffset = br.ReadInt32();
                _ = br.AssertUInt32((uint)this.Type);
                _ = br.ReadInt32(); // ID
                int sibOffset = br.ReadInt32();
                this.InstanceCount = br.ReadInt32();
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);
                _ = br.AssertInt32(0);

                if (nameOffset == 0) {
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {this.GetType()}.");
                }

                if (sibOffset == 0) {
                    throw new InvalidDataException($"{nameof(sibOffset)} must not be 0 in type {this.GetType()}.");
                }

                br.Position = start + nameOffset;
                this.Name = br.ReadShiftJIS();

                br.Position = start + sibOffset;
                this.SibPath = br.ReadShiftJIS();
            }

            internal override void Write(BinaryWriterEx bw, int id) {
                long start = bw.Position;
                bw.ReserveInt32("NameOffset");
                bw.WriteUInt32((uint)this.Type);
                bw.WriteInt32(id);
                bw.ReserveInt32("SibOffset");
                bw.WriteInt32(this.InstanceCount);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);

                bw.FillInt32("NameOffset", (int)(bw.Position - start));
                bw.WriteShiftJIS(MSB.ReambiguateName(this.Name), true);

                bw.FillInt32("SibOffset", (int)(bw.Position - start));
                bw.WriteShiftJIS(this.SibPath, true);
                bw.Pad(4);
            }

            internal void CountInstances(List<Part> parts) => this.InstanceCount = parts.Count(p => p.ModelName == this.Name);

            /// <summary>
            /// Returns a string representation of the model.
            /// </summary>
            public override string ToString() => $"{this.Type} {this.Name}";

            /// <summary>
            /// A model for a static piece of visual map geometry.
            /// </summary>
            public class MapPiece : Model {
                private protected override ModelType Type => ModelType.MapPiece;

                /// <summary>
                /// Creates a MapPiece with default values.
                /// </summary>
                public MapPiece() : base("mXXXXBX") { }

                internal MapPiece(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A model for a dynamic or interactible part.
            /// </summary>
            public class Object : Model {
                private protected override ModelType Type => ModelType.Object;

                /// <summary>
                /// Creates an Object with default values.
                /// </summary>
                public Object() : base("oXXXX") { }

                internal Object(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A model for any non-player character.
            /// </summary>
            public class Enemy : Model {
                private protected override ModelType Type => ModelType.Enemy;

                /// <summary>
                /// Creates an Enemy with default values.
                /// </summary>
                public Enemy() : base("cXXXX") { }

                internal Enemy(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A model for a player spawn point.
            /// </summary>
            public class Player : Model {
                private protected override ModelType Type => ModelType.Player;

                /// <summary>
                /// Creates a Player with default values.
                /// </summary>
                public Player() : base("c0000") { }

                internal Player(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A model for a static piece of physical map geometry.
            /// </summary>
            public class Collision : Model {
                private protected override ModelType Type => ModelType.Collision;

                /// <summary>
                /// Creates a Collision with default values.
                /// </summary>
                public Collision() : base("hXXXXBX") { }

                internal Collision(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A model for an AI navigation mesh.
            /// </summary>
            public class Navmesh : Model {
                private protected override ModelType Type => ModelType.Navmesh;

                /// <summary>
                /// Creates a Navmesh with default values.
                /// </summary>
                public Navmesh() : base("nXXXXBX") { }

                internal Navmesh(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A model for a dummy dynamic or interactible part.
            /// </summary>
            public class DummyObject : Model {
                private protected override ModelType Type => ModelType.DummyObject;

                /// <summary>
                /// Creates a DummyObject with default values.
                /// </summary>
                public DummyObject() : base("oXXXX") { }

                internal DummyObject(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A model for a dummy non-player character.
            /// </summary>
            public class DummyEnemy : Model {
                private protected override ModelType Type => ModelType.DummyEnemy;

                /// <summary>
                /// Creates a DummyEnemy with default values.
                /// </summary>
                public DummyEnemy() : base("cXXXX") { }

                internal DummyEnemy(BinaryReaderEx br) : base(br) { }
            }
        }
    }
}
