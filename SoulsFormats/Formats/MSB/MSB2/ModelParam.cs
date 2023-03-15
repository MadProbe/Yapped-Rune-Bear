using System;
using System.Collections.Generic;
using System.IO;
using SoulsFormats.Formats.MSB;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MSB2 {
        internal enum ModelType : byte {
            MapPiece = 0,
            Object = 1,
            Collision = 3,
            Navmesh = 4,
        }

        /// <summary>
        /// Models available for parts in the map to use.
        /// </summary>
        public class ModelParam : Param<Model>, IMsbParam<IMsbModel> {
            internal override int Version => 5;
            internal override string Name => "MODEL_PARAM_ST";

            /// <summary>
            /// Models for static visual elements.
            /// </summary>
            public List<Model.MapPiece> MapPieces { get; set; }

            /// <summary>
            /// Models for dynamic or interactible elements.
            /// </summary>
            public List<Model.Object> Objects { get; set; }

            /// <summary>
            /// Models for invisible but physical surfaces.
            /// </summary>
            public List<Model.Collision> Collisions { get; set; }

            /// <summary>
            /// Models for AI navigation.
            /// </summary>
            public List<Model.Navmesh> Navmeshes { get; set; }

            /// <summary>
            /// Creates an empty ModelParam.
            /// </summary>
            public ModelParam() {
                this.MapPieces = new List<Model.MapPiece>();
                this.Objects = new List<Model.Object>();
                this.Collisions = new List<Model.Collision>();
                this.Navmeshes = new List<Model.Navmesh>();
            }

            /// <summary>
            /// Returns every Model in the order they'll be written.
            /// </summary>
            public override List<Model> GetEntries() => SFUtil.ConcatAll<Model>(
                    this.MapPieces, this.Objects, this.Collisions, this.Navmeshes);
            IReadOnlyList<IMsbModel> IMsbParam<IMsbModel>.GetEntries() => this.GetEntries();

            /// <summary>
            /// Adds a model to the appropriate list for its type; returns the model.
            /// </summary>
            public Model Add(Model model) {
                switch (model) {
                    case Model.MapPiece m: this.MapPieces.Add(m); break;
                    case Model.Object m: this.Objects.Add(m); break;
                    case Model.Collision m: this.Collisions.Add(m); break;
                    case Model.Navmesh m: this.Navmeshes.Add(m); break;

                    default:
                        throw new ArgumentException($"Unrecognized type {model.GetType()}.", nameof(model));
                }
                return model;
            }
            IMsbModel IMsbParam<IMsbModel>.Add(IMsbModel item) => this.Add((Model)item);

            internal override Model ReadEntry(BinaryReaderEx br) {
                ModelType type = br.GetEnum8<ModelType>(br.Position + br.VarintSize);
                return type switch {
                    ModelType.MapPiece => this.MapPieces.EchoAdd(new Model.MapPiece(br)),
                    ModelType.Object => this.Objects.EchoAdd(new Model.Object(br)),
                    ModelType.Collision => this.Collisions.EchoAdd(new Model.Collision(br)),
                    ModelType.Navmesh => this.Navmeshes.EchoAdd(new Model.Navmesh(br)),
                    _ => throw new NotImplementedException($"Unimplemented model type: {type}"),
                };
            }
        }

        /// <summary>
        /// A model file available for parts to reference.
        /// </summary>
        public abstract class Model : NamedEntry, IMsbModel {
            private protected abstract ModelType Type { get; }
            private protected abstract bool HasTypeData { get; }

            private protected Model(string name) => this.Name = name;

            /// <summary>
            /// Creates a deep copy of the model.
            /// </summary>
            public Model DeepCopy() => (Model)this.MemberwiseClone();
            IMsbModel IMsbModel.DeepCopy() => this.DeepCopy();

            private protected Model(BinaryReaderEx br) {
                long start = br.Position;
                long nameOffset = br.ReadVarint();
                _ = br.AssertByte((byte)this.Type);
                _ = br.AssertByte(0);
                _ = br.ReadInt16(); // ID
                if (br.VarintLong) {
                    _ = br.AssertInt32(0);
                }

                long typeDataOffset = br.ReadVarint();
                _ = br.AssertVarint(0);

                if (nameOffset == 0) {
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {this.GetType()}.");
                }

                if (this.HasTypeData ^ typeDataOffset != 0) {
                    throw new InvalidDataException($"Unexpected {nameof(typeDataOffset)} 0x{typeDataOffset:X} in type {this.GetType()}.");
                }

                br.Position = start + nameOffset;
                this.Name = br.ReadUTF16();

                if (this.HasTypeData) {
                    br.Position = start + typeDataOffset;
                    this.ReadTypeData(br);
                }
            }

            private protected virtual void ReadTypeData(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(WriteTypeData)}.");

            internal override void Write(BinaryWriterEx bw, int id) {
                long start = bw.Position;
                bw.ReserveVarint("NameOffset");
                bw.WriteByte((byte)this.Type);
                bw.WriteByte(0);
                bw.WriteInt16((short)id);
                if (bw.VarintLong) {
                    bw.WriteInt32(0);
                }

                bw.ReserveVarint("TypeDataOffset");
                bw.WriteVarint(0);

                bw.FillVarint("NameOffset", bw.Position - start);
                bw.WriteUTF16(MSB.ReambiguateName(this.Name), true);
                bw.Pad(bw.VarintSize);

                if (this.HasTypeData) {
                    bw.FillVarint("TypeDataOffset", bw.Position - start);
                    this.WriteTypeData(bw);
                } else {
                    bw.FillVarint("TypeDataOffset", 0);
                }
            }

            private protected virtual void WriteTypeData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {this.GetType()} missing valid {nameof(WriteTypeData)}.");

            /// <summary>
            /// Returns a string representation of the model.
            /// </summary>
            public override string ToString() => $"{this.Name}";

            /// <summary>
            /// A model for a static piece of visual map geometry.
            /// </summary>
            public class MapPiece : Model {
                private protected override ModelType Type => ModelType.MapPiece;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates a MapPiece with default values.
                /// </summary>
                public MapPiece() : base("mXXXX") { }

                internal MapPiece(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A model for a dynamic or interactible part.
            /// </summary>
            public class Object : Model {
                private protected override ModelType Type => ModelType.Object;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates an Object with default values.
                /// </summary>
                public Object() : base("oXX_XXXX") { }

                private protected override void ReadTypeData(BinaryReaderEx br) => br.AssertVarint(0);

                internal Object(BinaryReaderEx br) : base(br) { }

                private protected override void WriteTypeData(BinaryWriterEx bw) => bw.WriteVarint(0);
            }

            /// <summary>
            /// A model for a static piece of physical map geometry.
            /// </summary>
            public class Collision : Model {
                private protected override ModelType Type => ModelType.Collision;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates a Collision with default values.
                /// </summary>
                public Collision() : base("hXX_XXXX") { }

                internal Collision(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A model for an AI navigation mesh.
            /// </summary>
            public class Navmesh : Model {
                private protected override ModelType Type => ModelType.Navmesh;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates a Navmesh with default values.
                /// </summary>
                public Navmesh() : base("nXX_XXXX") { }

                internal Navmesh(BinaryReaderEx br) : base(br) { }
            }
        }
    }
}
