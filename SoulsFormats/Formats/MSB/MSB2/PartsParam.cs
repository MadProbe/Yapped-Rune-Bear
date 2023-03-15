using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using SoulsFormats.Formats.MSB;
using SoulsFormats.Util;

namespace SoulsFormats {
    public partial class MSB2 {
        internal enum PartType : byte {
            MapPiece = 0,
            Object = 1,
            Collision = 3,
            Navmesh = 4,
            ConnectCollision = 5,
        }

        /// <summary>
        /// Concrete map elements.
        /// </summary>
        public class PartsParam : Param<Part>, IMsbParam<IMsbPart> {
            internal override int Version => 5;
            internal override string Name => "PARTS_PARAM_ST";

            /// <summary>
            /// Visible but intangible models.
            /// </summary>
            public List<Part.MapPiece> MapPieces { get; set; }

            /// <summary>
            /// Dynamic or interactible elements.
            /// </summary>
            public List<Part.Object> Objects { get; set; }

            /// <summary>
            /// Invisible but physical surfaces.
            /// </summary>
            public List<Part.Collision> Collisions { get; set; }

            /// <summary>
            /// AI navigation meshes.
            /// </summary>
            public List<Part.Navmesh> Navmeshes { get; set; }

            /// <summary>
            /// Connections to other maps.
            /// </summary>
            public List<Part.ConnectCollision> ConnectCollisions { get; set; }

            /// <summary>
            /// Creates an empty PartsParam.
            /// </summary>
            public PartsParam() {
                this.MapPieces = new List<Part.MapPiece>();
                this.Objects = new List<Part.Object>();
                this.Collisions = new List<Part.Collision>();
                this.Navmeshes = new List<Part.Navmesh>();
                this.ConnectCollisions = new List<Part.ConnectCollision>();
            }

            /// <summary>
            /// Adds a part to the appropriate list for its type; returns the part.
            /// </summary>
            public Part Add(Part part) {
                switch (part) {
                    case Part.MapPiece p: this.MapPieces.Add(p); break;
                    case Part.Object p: this.Objects.Add(p); break;
                    case Part.Collision p: this.Collisions.Add(p); break;
                    case Part.Navmesh p: this.Navmeshes.Add(p); break;
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
                    this.MapPieces, this.Objects, this.Collisions, this.Navmeshes, this.ConnectCollisions);
            IReadOnlyList<IMsbPart> IMsbParam<IMsbPart>.GetEntries() => this.GetEntries();

            internal override Part ReadEntry(BinaryReaderEx br) {
                PartType type = br.GetEnum8<PartType>(br.Position + br.VarintSize);
                return type switch {
                    PartType.MapPiece => this.MapPieces.EchoAdd(new Part.MapPiece(br)),
                    PartType.Object => this.Objects.EchoAdd(new Part.Object(br)),
                    PartType.Collision => this.Collisions.EchoAdd(new Part.Collision(br)),
                    PartType.Navmesh => this.Navmeshes.EchoAdd(new Part.Navmesh(br)),
                    PartType.ConnectCollision => this.ConnectCollisions.EchoAdd(new Part.ConnectCollision(br)),
                    _ => throw new NotImplementedException($"Unimplemented part type: {type}"),
                };
            }
        }

        /// <summary>
        /// A concrete map element.
        /// </summary>
        public abstract class Part : NamedEntry, IMsbPart {
            private protected abstract PartType Type { get; }

            /// <summary>
            /// The name of the part's model, referencing ModelParam.
            /// </summary>
            public string ModelName { get; set; }
            private short ModelIndex;

            /// <summary>
            /// Location of the part.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Rotation of the part, in degrees.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// Scale of the part; only supported for map pieces and objects.
            /// </summary>
            public Vector3 Scale { get; set; }

            /// <summary>
            /// Not confirmed; determines when the part is loaded.
            /// </summary>
            public uint[] DrawGroups { get; private set; }

            /// <summary>
            /// Unknown; possibly nvm groups.
            /// </summary>
            public int Unk44 { get; set; }

            /// <summary>
            /// Unknown; possibly nvm groups.
            /// </summary>
            public int Unk48 { get; set; }

            /// <summary>
            /// Unknown; possibly nvm groups.
            /// </summary>
            public int Unk4C { get; set; }

            /// <summary>
            /// Unknown; possibly nvm groups.
            /// </summary>
            public int Unk50 { get; set; }

            /// <summary>
            /// Not confirmed; determines when the part is visible.
            /// </summary>
            public uint[] DispGroups { get; private set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk64 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unk6C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unk6E { get; set; }

            private protected Part(string name) {
                this.Name = name;
                this.Scale = Vector3.One;
                this.DrawGroups = new uint[4] {
                    0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
                this.DispGroups = new uint[4] {
                    0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF };
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
                long nameOffset = br.ReadVarint();
                _ = br.AssertByte((byte)this.Type);
                _ = br.AssertByte(0);
                _ = br.ReadInt16(); // ID
                this.ModelIndex = br.ReadInt16();
                _ = br.AssertInt16(0);
                this.Position = br.ReadVector3();
                this.Rotation = br.ReadVector3();
                this.Scale = br.ReadVector3();
                this.DrawGroups = br.ReadUInt32s(4);
                this.Unk44 = br.ReadInt32();
                this.Unk48 = br.ReadInt32();
                this.Unk4C = br.ReadInt32();
                this.Unk50 = br.ReadInt32();
                this.DispGroups = br.ReadUInt32s(4);
                this.Unk64 = br.ReadInt32();
                _ = br.AssertInt32(0);
                this.Unk6C = br.ReadByte();
                _ = br.AssertByte(0);
                this.Unk6E = br.ReadByte();
                _ = br.AssertByte(0);
                long typeDataOffset = br.ReadVarint();
                if (br.VarintLong) {
                    _ = br.AssertInt64(0);
                }

                if (nameOffset == 0) {
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {this.GetType()}.");
                }

                if (typeDataOffset == 0) {
                    throw new InvalidDataException($"{nameof(typeDataOffset)} must not be 0 in type {this.GetType()}.");
                }

                br.Position = start + nameOffset;
                this.Name = br.GetUTF16(start + nameOffset);

                br.Position = start + typeDataOffset;
                this.ReadTypeData(br);
            }

            private protected abstract void ReadTypeData(BinaryReaderEx br);

            internal override void Write(BinaryWriterEx bw, int id) {
                long start = bw.Position;
                bw.ReserveVarint("NameOffset");
                bw.WriteByte((byte)this.Type);
                bw.WriteByte(0);
                bw.WriteInt16((short)id);
                bw.WriteInt16(this.ModelIndex);
                bw.WriteInt16(0);
                bw.WriteVector3(this.Position);
                bw.WriteVector3(this.Rotation);
                bw.WriteVector3(this.Scale);
                bw.WriteUInt32s(this.DrawGroups);
                bw.WriteInt32(this.Unk44);
                bw.WriteInt32(this.Unk48);
                bw.WriteInt32(this.Unk4C);
                bw.WriteInt32(this.Unk50);
                bw.WriteUInt32s(this.DispGroups);
                bw.WriteInt32(this.Unk64);
                bw.WriteInt32(0);
                bw.WriteByte(this.Unk6C);
                bw.WriteByte(0);
                bw.WriteByte(this.Unk6E);
                bw.WriteByte(0);
                bw.ReserveVarint("TypeDataOffset");
                if (bw.VarintLong) {
                    bw.WriteInt64(0);
                }

                long nameStart = bw.Position;
                int namePad = bw.VarintLong ? 0x20 : 0x2C;
                bw.FillVarint("NameOffset", nameStart - start);
                bw.WriteUTF16(MSB.ReambiguateName(this.Name), true);
                if (bw.Position - nameStart < namePad) {
                    bw.Position += namePad - (bw.Position - nameStart);
                }

                bw.Pad(bw.VarintSize);

                bw.FillVarint("TypeDataOffset", bw.Position - start);
                this.WriteTypeData(bw);
            }

            private protected abstract void WriteTypeData(BinaryWriterEx bw);

            internal virtual void GetNames(MSB2 msb, Entries entries) => this.ModelName = MSB.FindName(entries.Models, this.ModelIndex);

            internal virtual void GetIndices(Lookups lookups) => this.ModelIndex = (short)FindIndex(lookups.Models, this.ModelName);

            /// <summary>
            /// Returns a string representation of the part.
            /// </summary>
            public override string ToString() => $"{this.Type} \"{this.Name}\"";

            /// <summary>
            /// A visible but intangible model.
            /// </summary>
            public class MapPiece : Part {
                private protected override PartType Type => PartType.MapPiece;

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT02 { get; set; }

                /// <summary>
                /// Creates a MapPiece with default values.
                /// </summary>
                public MapPiece() : base("mXXXX_XXXX") { }

                internal MapPiece(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadInt16();
                    this.UnkT02 = br.ReadByte();
                    _ = br.AssertByte(0);
                    if (br.VarintLong) {
                        _ = br.AssertInt32(0);
                    }
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt16(this.UnkT00);
                    bw.WriteByte(this.UnkT02);
                    bw.WriteByte(0);
                    if (bw.VarintLong) {
                        bw.WriteInt32(0);
                    }
                }
            }

            /// <summary>
            /// A dynamic or interactible element.
            /// </summary>
            public class Object : Part {
                private protected override PartType Type => PartType.Object;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int MapObjectInstanceParamID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT04 { get; set; }

                /// <summary>
                /// Creates an Object with default values.
                /// </summary>
                public Object() : base("oXX_XXXX_XXXX") { }

                internal Object(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.MapObjectInstanceParamID = br.ReadInt32();
                    this.UnkT04 = br.ReadInt16();
                    _ = br.AssertInt16(0);
                    _ = br.AssertInt32(0);
                    _ = br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.MapObjectInstanceParamID);
                    bw.WriteInt16(this.UnkT04);
                    bw.WriteInt16(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// An invisible but physical surface that controls map loading and graphics settings, among other things.
            /// </summary>
            public class Collision : Part {
                private protected override PartType Type => PartType.Collision;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT11 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT12 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT13 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT15 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT17 { get; set; }

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
                public byte UnkT26 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT27 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT28 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT2C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT2E { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT30 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT35 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT36 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT3C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT40 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT44 { get; set; }

                /// <summary>
                /// Creates a Collision with default values.
                /// </summary>
                public Collision() : base("hXX_XXXX_XXXX") { }

                internal Collision(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadInt32();
                    this.UnkT04 = br.ReadInt32();
                    this.UnkT08 = br.ReadInt32();
                    this.UnkT0C = br.ReadInt32();
                    this.UnkT10 = br.ReadByte();
                    this.UnkT11 = br.ReadByte();
                    this.UnkT12 = br.ReadByte();
                    this.UnkT13 = br.ReadByte();
                    this.UnkT14 = br.ReadByte();
                    this.UnkT15 = br.ReadByte();
                    _ = br.AssertByte(0);
                    this.UnkT17 = br.ReadByte();
                    this.UnkT18 = br.ReadInt32();
                    this.UnkT1C = br.ReadInt32();
                    this.UnkT20 = br.ReadInt32();
                    _ = br.AssertInt16(0);
                    this.UnkT26 = br.ReadByte();
                    this.UnkT27 = br.ReadByte();
                    this.UnkT28 = br.ReadInt32();
                    this.UnkT2C = br.ReadByte();
                    _ = br.AssertByte(0);
                    this.UnkT2E = br.ReadInt16();
                    this.UnkT30 = br.ReadInt32();
                    _ = br.AssertByte(0);
                    this.UnkT35 = br.ReadByte();
                    this.UnkT36 = br.ReadInt16();
                    _ = br.AssertInt32(0);
                    this.UnkT3C = br.ReadInt32();
                    this.UnkT40 = br.ReadByte();
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    this.UnkT44 = br.ReadInt32();
                    br.AssertPattern(0x10, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.UnkT00);
                    bw.WriteInt32(this.UnkT04);
                    bw.WriteInt32(this.UnkT08);
                    bw.WriteInt32(this.UnkT0C);
                    bw.WriteByte(this.UnkT10);
                    bw.WriteByte(this.UnkT11);
                    bw.WriteByte(this.UnkT12);
                    bw.WriteByte(this.UnkT13);
                    bw.WriteByte(this.UnkT14);
                    bw.WriteByte(this.UnkT15);
                    bw.WriteByte(0);
                    bw.WriteByte(this.UnkT17);
                    bw.WriteInt32(this.UnkT18);
                    bw.WriteInt32(this.UnkT1C);
                    bw.WriteInt32(this.UnkT20);
                    bw.WriteInt16(0);
                    bw.WriteByte(this.UnkT26);
                    bw.WriteByte(this.UnkT27);
                    bw.WriteInt32(this.UnkT28);
                    bw.WriteByte(this.UnkT2C);
                    bw.WriteByte(0);
                    bw.WriteInt16(this.UnkT2E);
                    bw.WriteInt32(this.UnkT30);
                    bw.WriteByte(0);
                    bw.WriteByte(this.UnkT35);
                    bw.WriteInt16(this.UnkT36);
                    bw.WriteInt32(0);
                    bw.WriteInt32(this.UnkT3C);
                    bw.WriteByte(this.UnkT40);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(this.UnkT44);
                    bw.WritePattern(0x10, 0x00);
                }
            }

            /// <summary>
            /// An AI navigation mesh.
            /// </summary>
            public class Navmesh : Part {
                private protected override PartType Type => PartType.Navmesh;

                /// <summary>
                /// Unknown; possibly nvm groups.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Unknown; possibly nvm groups.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// Unknown; possibly nvm groups.
                /// </summary>
                public int UnkT08 { get; set; }

                /// <summary>
                /// Unknown; possibly nvm groups.
                /// </summary>
                public int UnkT0C { get; set; }

                /// <summary>
                /// Creates a Navmesh with default values.
                /// </summary>
                public Navmesh() : base("nXX_XXXX_XXXX") { }

                internal Navmesh(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.UnkT00 = br.ReadInt32();
                    this.UnkT04 = br.ReadInt32();
                    this.UnkT08 = br.ReadInt32();
                    this.UnkT0C = br.ReadInt32();
                    br.AssertPattern(0x10, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.UnkT00);
                    bw.WriteInt32(this.UnkT04);
                    bw.WriteInt32(this.UnkT08);
                    bw.WriteInt32(this.UnkT0C);
                    bw.WritePattern(0x10, 0x00);
                }
            }

            /// <summary>
            /// Causes another map to be loaded when standing on the referenced collision.
            /// </summary>
            public class ConnectCollision : Part {
                private protected override PartType Type => PartType.ConnectCollision;

                /// <summary>
                /// Name of the referenced collision part.
                /// </summary>
                public string CollisionName { get; set; }
                private int CollisionIndex;

                /// <summary>
                /// The map to load when on this collision.
                /// </summary>
                public byte[] MapID { get; private set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT0C { get; set; }

                /// <summary>
                /// Creates a ConnectCollision with default values.
                /// </summary>
                public ConnectCollision() : base("hXX_XXXX_XXXX") => this.MapID = new byte[4];

                private protected override void DeepCopyTo(Part part) {
                    var connect = (ConnectCollision)part;
                    connect.MapID = (byte[])this.MapID.Clone();
                }

                internal ConnectCollision(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) {
                    this.CollisionIndex = br.ReadInt32();
                    this.MapID = br.ReadBytes(4);
                    this.UnkT08 = br.ReadInt32();
                    this.UnkT0C = br.ReadByte();
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                    _ = br.AssertByte(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw) {
                    bw.WriteInt32(this.CollisionIndex);
                    bw.WriteBytes(this.MapID);
                    bw.WriteInt32(this.UnkT08);
                    bw.WriteByte(this.UnkT0C);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                }

                internal override void GetNames(MSB2 msb, Entries entries) {
                    base.GetNames(msb, entries);
                    this.CollisionName = MSB.FindName(msb.Parts.Collisions, this.CollisionIndex);
                }

                internal override void GetIndices(Lookups lookups) {
                    base.GetIndices(lookups);
                    this.CollisionIndex = FindIndex(lookups.Collisions, this.CollisionName);
                }
            }
        }
    }
}
